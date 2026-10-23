"""Repair rotating fender fragments in existing Meshy skins, without changing the source shape.
Run in an isolated Blender background process; never touches the user's live scene.
"""
import bpy,json,math,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
FOLDER=ROOT/'Assets/PhoViet/Art/Bikes/MeshyRiders'
BIKE={'Frame','Steering','WheelFront','WheelRear'}
CLIPS=['Idle','Ride','KickLeft','KickRight','FallLeft','FallRight','Recover']

def stationary_parent(name):return 'Steering' if name=='WheelFront' else 'Frame'

def wheel_face(points,name):
    z=.78 if name=='WheelFront' else -.73
    # A centroid inside the wheel is insufficient: long fender triangles can
    # cross the tyre cylinder while most of their surface belongs to the frame.
    radius=[math.hypot(p[1]-.34,p[2]-z) for p in points]
    # The generated tyre and mudguard share irregular/fused surface patches.
    # Keep their outer silhouette rigid; only the rim/hub spins. A black tyre
    # has the same silhouette at road speed, without the AI shell's flying fins.
    width=.09 if name=='WheelFront' else .065
    return max(radius)<=.245 and max(abs(p[0]) for p in points)<=width

def repair(obj):
    mesh=obj.data;names={g.index:g.name for g in obj.vertex_groups}
    original_name=mesh.name;normals=[tuple(n.vector) for n in mesh.corner_normals]
    weights=[{names[g.group]:g.weight for g in v.groups} for v in mesh.vertices]
    verts=[];faces=[];uvs=[];new_weights=[];lookup={};changed=0;moving=0
    uv=mesh.uv_layers.active.data
    for poly in mesh.polygons:
        influence={}
        for vi in poly.vertices:
            for n,w in weights[vi].items():influence[n]=influence.get(n,0)+w/len(poly.vertices)
        dominant=max(influence,key=influence.get)
        override=None
        wheels=[n for n in ['WheelFront','WheelRear'] if influence.get(n,0)>1e-6]
        vehicle_face=all(sum(w for n,w in weights[i].items() if n in BIKE)>.999 for i in poly.vertices)
        if wheels and vehicle_face:
            wheel=max(wheels,key=lambda n:influence[n])
            points=[(-mesh.vertices[i].co.x,mesh.vertices[i].co.z,-mesh.vertices[i].co.y) for i in poly.vertices]
            override=wheel if influence[wheel]>.5 and wheel_face(points,wheel) else stationary_parent(wheel)
            if override!=wheel:changed+=1
            else:moving+=1
        face=[];coords=[]
        for li in poly.loop_indices:
            vi=mesh.loops[li].vertex_index;key=(vi,override)
            if key not in lookup:
                lookup[key]=len(verts);verts.append(tuple(mesh.vertices[vi].co));new_weights.append({override:1.} if override else weights[vi])
            face.append(lookup[key]);coords.append(tuple(uv[li].uv))
        faces.append(face);uvs.append(coords)
    data=bpy.data.meshes.new(mesh.name+'_StableWheels');data.from_pydata(verts,[],faces);data.update()
    for mat in mesh.materials:data.materials.append(mat)
    layer=data.uv_layers.new(name=mesh.uv_layers.active.name)
    for before,after,coords in zip(mesh.polygons,data.polygons,uvs):
        after.use_smooth=before.use_smooth;after.material_index=before.material_index
        for li,coord in zip(after.loop_indices,coords):layer.data[li].uv=coord
    data.normals_split_custom_set(normals)
    obj.data=data
    for g in list(obj.vertex_groups):obj.vertex_groups.remove(g)
    for n in names.values():obj.vertex_groups.new(name=n)
    for i,ws in enumerate(new_weights):
        for n,w in ws.items():obj.vertex_groups[n].add([i],w,'REPLACE')
    assert len(data.polygons)==len(mesh.polygons),'Never remove source faces to hide flicker'
    if mesh.users==0:bpy.data.meshes.remove(mesh)
    data.name=original_name
    return {'mesh':obj.name,'stationaryFacesRestored':changed,'rotatingFaces':moving,'faces':len(data.polygons)}

def main():
    names=['PV_HeroScooter'] if '--hero-only' in sys.argv else ['PV_HeroScooter','PV_CityScooter','PV_Underbone','PV_SportBike','PV_Racer']
    reports=[]
    for name in names:
        bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
        for a in list(bpy.data.actions):bpy.data.actions.remove(a)
        path=FOLDER/(name+'.fbx');bpy.ops.import_scene.fbx(filepath=str(path))
        rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
        if rig.animation_data:
            rig.animation_data.action=None
        # FBX import loads separate actions, not the exported NLA strips. Rebuild
        # those strips before export so reinstalling the model retains all 7 takes.
        rig.animation_data_create()
        for track in list(rig.animation_data.nla_tracks):rig.animation_data.nla_tracks.remove(track)
        for clip in CLIPS:
            action=next((a for a in bpy.data.actions if a.name.endswith('|'+clip) or a.name.endswith('_'+clip)),None)
            if action is None:raise RuntimeError('Missing original action '+name+'/'+clip)
            action.name=name+'_'+clip
            track=rig.animation_data.nla_tracks.new();track.name=clip
            track.strips.new(clip,int(action.frame_range[0]),action)
        report={'asset':name,'meshes':[repair(o) for o in bpy.context.scene.objects if o.type=='MESH']}
        bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
        for o in rig.children:
            if o.type=='MESH':o.select_set(True)
        bpy.context.view_layer.objects.active=rig
        bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,use_armature_deform_only=True,mesh_smooth_type='FACE',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=True,bake_anim_simplify_factor=.35,path_mode='RELATIVE',embed_textures=False)
        reports.append(report)
        metadata=FOLDER/(name+'.json');info=json.loads(metadata.read_text());info['wheelBinding']='stable-rim-only-v1';metadata.write_text(json.dumps(info,indent=2))
    if '--hero-only' not in sys.argv:
        bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
        for i,name in enumerate(names):
            before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(FOLDER/(name+'.fbx')))
            for o in set(bpy.context.scene.objects)-before:
                if o.parent is None:o.location.x+=i*1.6
        bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_MeshyRiders.blend'))
    (ROOT/'Captures/rear-wheel-repair-export.json').write_text(json.dumps(reports,indent=2))
    print('REPAIRED_WHEELS',json.dumps(reports),flush=True)
if __name__=='__main__':main()
