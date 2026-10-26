"""Rig the received combined rider/bike in its existing seated/gripping pose.
Only the dedicated Urban Rider scene/assets are authored; other art is untouched.
"""
import bpy,json,math,shutil
import numpy as np
from pathlib import Path
from mathutils import Vector,Quaternion
ROOT=Path(__file__).resolve().parents[2]
__name__='urban_library'
exec(compile((ROOT/'ArtSource/Blender/pho_viet_bikes.py').read_text(encoding='utf-8'),'pho_viet_bikes.py','exec'))
ROOT=Path(__file__).resolve().parents[2]
OUT=str(ROOT/'Assets/PhoViet/Art/UrbanRider');NAME='PV_UrbanRider';Path(OUT).mkdir(parents=True,exist_ok=True)
SOURCE=ROOT/'Assets/MeshyImports/Urban Rider_20261027_035549'
if not bpy.context.scene.name.startswith('PV_UrbanRider'):bpy.context.window.scene=bpy.data.scenes.new('PV_UrbanRider_Work')
for o in list(bpy.context.scene.objects):bpy.data.objects.remove(o,do_unlink=True)
cache=ROOT/'.tools/urban-source-decimated.blend'
if cache.exists():
    with bpy.data.libraries.load(str(cache),link=False) as (src,dst):dst.objects=list(src.objects)
    for o in dst.objects:
        if o is not None:bpy.context.collection.objects.link(o)
else:
    bpy.ops.import_scene.fbx(filepath=str(next(SOURCE.glob('*.fbx'))))
    o=next(o for o in bpy.context.scene.objects if o.type=='MESH');bpy.context.view_layer.objects.active=o;o.select_set(True)
    d=o.modifiers.new('MobileUrbanRider','DECIMATE');d.ratio=42000/2664028;d.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=d.name)
source=next(o for o in bpy.context.scene.objects if o.type=='MESH');mesh=source.data
raw=np.array([tuple(source.matrix_world@v.co) for v in mesh.vertices]);lo=raw.min(0);hi=raw.max(0);scale=2.15/(hi[2]-lo[2]);mid=(-.61891+.50)/2
points=np.c_[-raw[:,0]*scale,(raw[:,2]-lo[2])*scale,-(raw[:,1]-mid)*scale+.025]
front=(0,(-.57757-lo[2])*scale,-(-.61891-mid)*scale+.025)
rear=(0,(-.63-lo[2])*scale,-(.50-mid)*scale+.025)
FR=.32025*scale;RR=.27*scale
joints={'Frame':(None,(0,0,0)),'Steering':('Frame',(0,1.04,.33)),'WheelFront':('Steering',front),'WheelRear':('Frame',rear),
 'Pelvis':('Frame',(0,1.04,-.34)),'Torso':('Pelvis',(0,1.43,-.25)),'Head':('Torso',(0,1.91,-.15))}
for side,sign in [('L',-1),('R',1)]:
    joints.update({side+'UpperArm':('Torso',(sign*.23,1.70,-.22)),side+'Forearm':(side+'UpperArm',(sign*.34,1.44,0)),side+'Hand':('Steering',(sign*.39,1.30,.26)),side+'Thigh':('Pelvis',(sign*.20,1.03,-.34)),side+'Shin':(side+'Thigh',(sign*.29,.85,.05)),side+'Foot':(side+'Shin',(sign*.30,.57,-.10))})
segments={}
for side in ['L','R']:
    for a,b in [('UpperArm','Forearm'),('Forearm','Hand'),('Thigh','Shin'),('Shin','Foot')]:segments[side+a]=(np.array(joints[side+a][1]),np.array(joints[side+b][1]))
segments['Torso']=(np.array((0,1.19,-.29)),np.array((0,1.76,-.18)))
def segment(p,key):
    a,b=segments[key];ab=b-a;t=np.clip(np.dot(p-a,ab)/np.dot(ab,ab),0,1);return np.linalg.norm(p-a-ab*t),t
img=bpy.data.images.load(str(SOURCE/'meshy_basecolor.png'),check_existing=True);pixels=np.empty(img.size[0]*img.size[1]*4,np.float32);img.pixels.foreach_get(pixels);pixels=pixels.reshape(img.size[1],img.size[0],4)
uv=mesh.uv_layers.active.data
verts=[];faces=[];uvs=[];weights=[];normals=[];regions=[];lookup={}
for poly in mesh.polygons:
    pts=points[list(poly.vertices)];p=pts.mean(0);x,y,z=p
    coords=[tuple(uv[i].uv) for i in poly.loop_indices];u,v=np.mean(coords,0);colour=pixels[min(img.size[1]-1,max(0,int(v*img.size[1]))),min(img.size[0]-1,max(0,int(u*img.size[0]))),:3]
    r,g,b=colour;value=max(colour);sat=(value-min(colour))/max(value,.001);skin=r>b*1.12 and g>b*1.05 and r>.13;blue=b>r*1.13 and b>g*.98 and sat>.13;white=value>.42 and sat<.27
    side='L' if x<0 else 'R';bone='Frame';human=False
    if y>1.80 and z<.19:bone='Head';human=True
    elif y>1.18 and z<.20 and abs(x)<.31 or y>1.53 and z<.10 and abs(x)<.40:
        bone='Torso';human=True
        if abs(x)>.20 and segment(p,side+'UpperArm')[0]<.16:bone=side+'UpperArm'
    elif skin and 1.15<y<1.74 and abs(x)>.20 and -.27<z<.49:
        bone=side+'Hand' if np.linalg.norm(p-np.array(joints[side+'Hand'][1]))<.135 else min([side+'UpperArm',side+'Forearm'],key=lambda k:segment(p,k)[0]);human=True
    elif blue and .45<y<1.2 and .08<abs(x)<.47 and -.65<z<.27:
        bone=min([side+'Thigh',side+'Shin'],key=lambda k:segment(p,k)[0]);human=True
    elif white and .43<y<.75 and .255<abs(x)<.44 and -.32<z<.24:bone=side+'Foot';human=True
    if not human:
        if z>.18 and y>1.05 or z>front[2]-.19 and y<1.08:bone='Steering'
        for name,ax,rad in [('WheelFront',front,FR),('WheelRear',rear,RR)]:
            radial=np.linalg.norm(pts[:,1:]-np.array(ax)[1:],axis=1)
            if max(radial)<rad*.64 and max(abs(pts[:,0]))<.135:bone=name;break
    face=[];fuv=[]
    for li,coord in zip(poly.loop_indices,coords):
        vi=mesh.loops[li].vertex_index;key=(vi,bone)
        if key not in lookup:
            lookup[key]=len(verts);verts.append(tuple(B(points[vi])));w={bone:1.}
            # Soft shoulder/elbow/knee blends; hands remain rigidly attached to
            # steering so the received finger/grip contact never drifts.
            if bone.endswith('Forearm'):
                _,t=segment(points[vi],bone);a=max(0,(.20-t)/.20)*.45
                if a:w={bone:1-a,side+'UpperArm':a}
            if bone.endswith('Shin'):
                _,t=segment(points[vi],bone);a=max(0,(.20-t)/.20)*.45
                if a:w={bone:1-a,side+'Thigh':a}
            weights.append(w)
        face.append(lookup[key]);fuv.append(coord)
        n=(source.matrix_world.to_3x3()@mesh.corner_normals[li].vector).normalized();normals.append(tuple(n))
    faces.append(face);uvs.append(fuv)
    head=not human and z>.37 and 1.17<y<1.32 and abs(x)<.23 and ((y-1.245)/.082)**2+(x/.235)**2<1
    tail=not human and z<-.77 and .63<y<1.07 and abs(x)<.25
    regions.append(dict(uv=coords,shirt=bool(human and bone in ['Torso','LUpperArm','RUpperArm']),paint=bool(not human and bone not in ['WheelFront','WheelRear'] and y>.65 and not head and not tail),head=bool(head),tail=bool(tail)))
# The source shares vertices across wrist/sleeve/fork regions. UV/bone splits
# must keep identical skin weights on coincident vertices, otherwise steering
# opens a seam even when the arm endpoint reaches the hand exactly.
shared={}
for (source_index,bone),i in lookup.items():
    votes=shared.setdefault(source_index,{})
    for name,w in weights[i].items():votes[name]=votes.get(name,0)+w
for (source_index,bone),i in lookup.items():
    votes=sorted(shared[source_index].items(),key=lambda pair:-pair[1])[:2];total=sum(w for _,w in votes)
    weights[i]={name:w/total for name,w in votes}
data=bpy.data.meshes.new(NAME+'_Mesh');data.from_pydata(verts,[],faces);data.update();layer=data.uv_layers.new(name='UVMap')
for poly,coords in zip(data.polygons,uvs):
    poly.use_smooth=True
    for li,coord in zip(poly.loop_indices,coords):layer.data[li].uv=coord
data.normals_split_custom_set(normals)
bpy.data.objects.remove(source,do_unlink=True)
arm=bpy.data.armatures.new(NAME+'_Skeleton');rig=bpy.data.objects.new(NAME+'_Rig',arm);bpy.context.collection.objects.link(rig);bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
for name,(parent,p) in joints.items():
    bone=arm.edit_bones.new(name);bone.head=B(p);bone.tail=bone.head+Vector((0,0,.10))
    if parent:bone.parent=arm.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
obj=bpy.data.objects.new(NAME+'_Body',data);bpy.context.collection.objects.link(obj);obj.parent=rig
for bone in joints:obj.vertex_groups.new(name=bone)
for i,w in enumerate(weights):
    for name,weight in w.items():obj.vertex_groups[name].add([i],weight,'REPLACE')
modifier=obj.modifiers.new('VehicleAndRiderRig','ARMATURE');modifier.object=rig
mat=bpy.data.materials.new(NAME+'_Atlas');mat.use_nodes=True;tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=img;mat.node_tree.links.new(tex.outputs['Color'],mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color']);data.materials.append(mat)
clips=['Idle','Ride','KickLeft','KickRight','FallLeft','FallRight','Recover'];rig.animation_data_create()
for name in clips:
    action=bpy.data.actions.new(NAME+'_'+name);rig.animation_data.action=action
    for frame in range(1,32,2):
        t=(frame-1)/30
        for pb in rig.pose.bones:pb.rotation_mode='QUATERNION';pb.rotation_quaternion=Quaternion();pb.location=(0,0,0);pb.scale=(1,1,1)
        if name in ['Ride','Idle']:
            pose_offset(rig,'Torso',(0,.0025*math.sin(t*2*math.pi),0));pose_rotation(rig,'Head',(.35*math.sin(t*2*math.pi),0,0))
        elif name.startswith('Fall') or name=='Recover':
            f=(1-t if name=='Recover' else min(1,t*2));pose_rotation(rig,'Head',(10*f,0,0))
        for pb in rig.pose.bones:pb.keyframe_insert(data_path='rotation_quaternion',frame=frame);pb.keyframe_insert(data_path='location',frame=frame)
    action.use_fake_user=True;slot=rig.animation_data.action_slot;rig.animation_data.action=None;track=rig.animation_data.nla_tracks.new();track.name=name;strip=track.strips.new(name,1,action);strip.action_slot=slot;strip.name=name;track.mute=True
for pb in rig.pose.bones:pb.rotation_quaternion=Quaternion();pb.location=(0,0,0)
data.calc_loop_triangles();size=(points.max(0)-points.min(0)).tolist()
info=dict(name=NAME,design='urban-rider-combined',source=str(next(SOURCE.glob('*.fbx')).relative_to(ROOT)).replace('\\','/'),sourceTriangles=2664028,triangles=len(data.loop_triangles),size=size,bones=len(joints),frontAxle=list(front),rearAxle=list(rear),frontRadius=FR,rearRadius=RR,steeringPivot=[0,1.04,.33],steeringAxis=[0,.96,-.28],clips=clips,anchors=dict(headlight=[0,1.17,.46],taillight=[0,.88,-.90],exhaust_tip=[.24,.40,-.84]),collisionHalfWidth=.33,collisionHalfLength=float(max(abs(points[:,2]))))
MODELS[NAME]=(rig,obj,info);make_lods(NAME);export_model(NAME)
for old,new in [('meshy_basecolor','Palette'),('meshy_normal','Normal'),('meshy_metallic_smoothness','Surface')]:shutil.copyfile(SOURCE/(old+'.png'),Path(OUT)/(NAME+'_'+new+'.png'))
(ROOT/'.tools/urban-rider-uvregions.json').write_text(json.dumps(regions),encoding='utf-8')
(ROOT/'Captures/urban-rider-export.json').write_text(json.dumps(info,indent=2),encoding='utf-8')
for image in bpy.data.images:
    if image==img and not image.packed_file:image.pack()
bpy.data.libraries.write(str(ROOT/'ArtSource/Blender/PhoViet_UrbanRider.blend'),{bpy.context.scene},path_remap='RELATIVE',fake_user=True,compress=True)
print('URBAN_EXPORT',json.dumps(info),flush=True)
