"""Export only the approved C badge to three compact vertex-colour mesh payloads.
Run through Blender MCP; never replaces the source scene or touches Unity.
"""
import bpy, json
from pathlib import Path
from mathutils import Vector

out=Path(r'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/MissilePickup-20261105')
previous=bpy.context.window.scene
source=bpy.data.objects.get('C • Hex Ammo')
if source is None:raise RuntimeError('Load MissilePickup_ABC.blend before export')
def tree(o):return [o]+[v for child in o.children for v in tree(child)]
objects=[o for o in tree(source) if o.type=='MESH']
scene=bpy.data.scenes.new('PV_Approved_C_Export_2131')
payload=[]
try:
    bpy.context.window.scene=scene
    for level,factor in enumerate([1,.52,.24]):
        verts=[];normals=[];colors=[];indices=[];lookup={}
        for original in objects:
            o=original.copy();o.data=original.data.copy();scene.collection.objects.link(o)
            o.parent=None;o.matrix_world=source.matrix_world.inverted()@original.matrix_world
            for m in list(o.modifiers):
                if m.type=='BEVEL':m.segments=1
            bpy.context.view_layer.objects.active=o;o.select_set(True)
            for m in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=m.name)
            tri_count=sum(len(p.vertices)-2 for p in o.data.polygons)
            cap=360 if 'nose' in o.name else 160 if 'nozzle' in o.name else 150
            ratio=min(1,cap*factor/max(1,tri_count))
            if ratio<.99:
                dec=o.modifiers.new('Budgeted runtime LOD','DECIMATE');dec.ratio=ratio
                bpy.ops.object.modifier_apply(modifier=dec.name)
            mesh=o.data;mesh.calc_loop_triangles();transform=o.matrix_world
            nmat=transform.to_3x3().inverted().transposed()
            material=next(n for n in mesh.materials[0].node_tree.nodes if n.type=='BSDF_PRINCIPLED')
            rgb=material.inputs['Base Color'].default_value[:3]
            emission=material.inputs['Emission Strength'].default_value
            alpha=min(.85,emission*.4)
            for tri in mesh.loop_triangles:
                for li in reversed(tri.loops):
                    v=transform@mesh.vertices[mesh.loops[li].vertex_index].co
                    n=(nmat@mesh.corner_normals[li].vector).normalized()
                    # Face the +Z pickup camera with the same left/right layout
                    # as Blender's -Y front view; mirror winding with X as well.
                    p=(-v.x,v.z,-v.y);normal=(-n.x,n.z,-n.y)
                    key=tuple(round(x,6) for x in (*p,*normal,*rgb,alpha))
                    if key not in lookup:
                        lookup[key]=len(verts);verts.append(list(p));normals.append(list(normal));colors.append([*rgb,alpha])
                    indices.append(lookup[key])
            bpy.data.objects.remove(o,do_unlink=True)
        payload.append(dict(name='PV_MissilePickup_LOD'+str(level),vertices=verts,normals=normals,colors=colors,triangles=indices))
    # One shared transform across LODs preserves size/centre during switches.
    all_vertices=[v for lod in payload for v in lod['vertices']]
    low=[min(v[a] for v in all_vertices) for a in range(3)];high=[max(v[a] for v in all_vertices) for a in range(3)]
    scale=1.85/max(high[0]-low[0],high[1]-low[1]);center=[(a+b)*.5 for a,b in zip(low,high)]
    for lod in payload:lod['vertices']=[[round((v[a]-center[a])*scale,6) for a in range(3)] for v in lod['vertices']]
    data=dict(approved='C',source='MissilePickup_ABC.blend',front='+Z',maximumDimension=1.85,lods=payload)
    (out/'approved-C-geometry.json').write_text(json.dumps(data,separators=(',',':')),encoding='utf-8')
    print(json.dumps({'success':True,'meshes':len(payload),'triangles':[len(x['triangles'])//3 for x in payload],'vertices':[len(x['vertices']) for x in payload],'sourceMeshes':len(objects)}))
finally:
    bpy.context.window.scene=previous
    for o in list(scene.objects):bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.scenes.remove(scene)
