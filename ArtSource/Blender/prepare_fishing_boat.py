"""Convert the received fishing boat in a separate Blender process; keep source intact."""
from pathlib import Path
import bpy,json,numpy as np
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/PhoViet/Art/River/FishingBoat';OUT.mkdir(parents=True,exist_ok=True)
assert not bpy.data.filepath,'Requires isolated factory scene'
for o in list(bpy.data.objects):bpy.data.objects.remove(o,do_unlink=True)
folder=ROOT/'Assets/MeshyImports/Vietnamese_Fishing_Boat_20261030_145637'
bpy.ops.import_scene.fbx(filepath=str(next(folder.glob('*.fbx'))))
source=next(o for o in bpy.context.scene.objects if o.type=='MESH')
points=np.array([tuple(source.matrix_world@v.co) for v in source.data.vertices]);lo=points.min(axis=0);hi=points.max(axis=0)
source.data.calc_loop_triangles()
report=dict(sourceTriangles=len(source.data.loop_triangles),sourceBounds=[lo.tolist(),hi.tolist()],sourceVertices=len(points))
(ROOT/'Captures/fishing-boat-source-blender.json').write_text(json.dumps(report,indent=2))
print('SOURCE',json.dumps(report),flush=True)
# Meshy boat length is X; keep the same orientation/9m length as existing moored boats.
assert hi[0]-lo[0]>max(hi[1]-lo[1],hi[2]-lo[2]),'Inspect source axes before conversion'
scale=9/(hi[0]-lo[0]);mid=(lo+hi)*.5
for v,p in zip(source.data.vertices,points):v.co=((p[0]-mid[0])*scale,(p[1]-mid[1])*scale,(p[2]-lo[2])*scale-.45)
source.matrix_world.identity();source.data.update()
raw_count=report['sourceTriangles'];lods=[]
for i,budget in enumerate((40000,20000,10000)):
 ob=source if i==0 else lods[0].copy()
 if i>0:ob.data=lods[0].data.copy();bpy.context.collection.objects.link(ob)
 ob.name='PV_FishingBoat_LOD'+str(i);bpy.context.view_layer.objects.active=ob
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True)
 ob.data.calc_loop_triangles();count=len(ob.data.loop_triangles)
 mod=ob.modifiers.new('RiverMobileLOD','DECIMATE');mod.ratio=min(1,budget/count);mod.use_collapse_triangulate=True
 bpy.ops.object.modifier_apply(modifier=mod.name)
 ob.data.calc_loop_triangles();lods.append(ob)
 print('LOD',i,len(ob.data.loop_triangles),flush=True)
report['length']=9;report['waterline']=-.45;report['lodTriangles']=[len(o.data.loop_triangles) for o in lods]
# Preserve UVs/normals and texture; dedicated shared material is assigned by Unity.
for ob in lods:
 for f in ob.data.polygons:f.use_smooth=True
 ob.data.materials.clear()
mat=bpy.data.materials.new('PV_FishingBoat');mat.use_nodes=True
bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bsdf.inputs['Roughness'].default_value=.68
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(folder/'meshy_basecolor.png'));mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
for ob in lods:ob.data.materials.append(mat)
bpy.ops.object.select_all(action='DESELECT')
for ob in lods:ob.select_set(True)
bpy.context.view_layer.objects.active=lods[0]
bpy.ops.export_scene.fbx(filepath=str(OUT/'PV_FishingBoat.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,add_leaf_bones=False,mesh_smooth_type='FACE')
for i,ob in enumerate(lods):ob.hide_render=i>0;ob.hide_set(i>0)
tex.image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_FishingBoat.blend'))
(OUT/'PV_FishingBoat.json').write_text(json.dumps(report,indent=2))
(ROOT/'Captures/fishing-boat-export.json').write_text(json.dumps(dict(success=True,**report),indent=2))
print('EXPORTED',report['lodTriangles'],flush=True)
