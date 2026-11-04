"""Prepare received Meshy helicopter in an isolated Blender process, preserving UVs."""
import bpy,bmesh,json,math,shutil,random,struct,wave
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2];SRC=ROOT/'Assets/MeshyImports/Redline_Rescue_Helicopter_20261104_195539';OUT=ROOT/'Assets/PhoViet/Art/Helicopter';OUT.mkdir(parents=True,exist_ok=True)
assert not bpy.data.filepath,'Use isolated --factory-startup'
for o in list(bpy.data.objects):bpy.data.objects.remove(o,do_unlink=True)
bpy.ops.import_scene.fbx(filepath=str(next(SRC.glob('*.fbx'))))
source=next(o for o in bpy.context.scene.objects if o.type=='MESH');bpy.context.view_layer.objects.active=source;source.select_set(True);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
m=source.data;adj=[[] for _ in m.vertices]
for e in m.edges:a,b=e.vertices;adj[a].append(b);adj[b].append(a)
seen=set();kinds={};component_counts={'Body':0,'MainRotor':0,'TailRotor':0}
for start in range(len(adj)):
 if start in seen:continue
 stack=[start];seen.add(start);group=[]
 while stack:
  j=stack.pop();group.append(j)
  for k in adj[j]:
   if k not in seen:seen.add(k);stack.append(k)
 coords=[m.vertices[j].co for j in group];lo=[min(v[a] for v in coords) for a in range(3)];hi=[max(v[a] for v in coords) for a in range(3)]
 kind='MainRotor' if lo[2]>.09 and hi[0]<.25 else 'TailRotor' if lo[0]>.30 and hi[1]<-.012 and hi[2]<.09 else 'Body'
 component_counts[kind]+=1
 for j in group:kinds[j]=kind
scale=6.8/(max(v.co.x for v in m.vertices)-min(v.co.x for v in m.vertices))
def convert(p):return Vector((-p[1]*scale,(p[0]+.132)*scale,(p[2]+.025)*scale))
pivots={'Body':Vector((0,0,0)),'MainRotor':convert((-.132,0,.132)),'TailRotor':convert((.402,-.025,.005))}
for v in m.vertices:v.co=convert(v.co)
mat=bpy.data.materials.new('PV_Helicopter');mat.use_nodes=True;bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bsdf.inputs['Roughness'].default_value=.52
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(SRC/'meshy_basecolor.png'));tex.image.pack();mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
report={'source':'Redline Rescue Helicopter','length':6.8,'sourceTriangles':len(m.polygons),'components':component_counts,'parts':{},'pivotsUnity':{k:[v.x,v.z,-v.y] for k,v in pivots.items()}}
export=[];parts={}
for kind in ['Body','MainRotor','TailRotor']:
 mesh=m.copy();bm=bmesh.new();bm.from_mesh(mesh);bm.verts.ensure_lookup_table();bmesh.ops.delete(bm,geom=[v for v in bm.verts if kinds[v.index]!=kind],context='VERTS');bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
 for v in mesh.vertices:v.co-=pivots[kind]
 mesh.materials.clear();mesh.materials.append(mat);mesh.calc_loop_triangles();assert len(mesh.polygons)>20,kind
 parts[kind]=[]
 for lod,ratio in enumerate([1,.52,.24]):
  ob=bpy.data.objects.new('Heli_'+kind+'_LOD'+str(lod),mesh.copy());bpy.context.collection.objects.link(ob);ob.location=pivots[kind];bpy.context.view_layer.objects.active=ob
  if lod:
   mod=ob.modifiers.new('HelicopterLOD','DECIMATE');mod.ratio=ratio;mod.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=mod.name)
  ob.data.calc_loop_triangles();parts[kind].append(ob);export.append(ob)
 report['parts'][kind]=[len(o.data.loop_triangles) for o in parts[kind]]
bpy.data.objects.remove(source,do_unlink=True)
bpy.ops.object.select_all(action='DESELECT')
for ob in export:ob.select_set(True)
bpy.context.view_layer.objects.active=export[0]
bpy.ops.export_scene.fbx(filepath=str(OUT/'PV_Helicopter.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,add_leaf_bones=False,mesh_smooth_type='FACE')
for kind,obs in parts.items():
 for lod,ob in enumerate(obs):
  ob.hide_render=lod>0;ob.hide_set(lod>0)
  if kind!='Body':
   axis=2 if kind=='MainRotor' else 0;ob.rotation_mode='XYZ';ob.rotation_euler[axis]=0;ob.keyframe_insert(data_path='rotation_euler',frame=1);ob.rotation_euler[axis]=math.tau*(1 if kind=='MainRotor' else 4);ob.keyframe_insert(data_path='rotation_euler',frame=31)
bpy.context.scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_Helicopter.blend'))
for a,b in [('meshy_basecolor','Palette'),('meshy_normal','Normal'),('meshy_metallic_smoothness','Surface')]:shutil.copyfile(SRC/(a+'.png'),OUT/('PV_Helicopter_'+b+'.png'))
(OUT/'PV_Helicopter.json').write_text(json.dumps(report,indent=2),encoding='utf8');(ROOT/'Captures/heli2124-export.json').write_text(json.dumps(dict(success=True,**report),indent=2),encoding='utf8');print('HELI_EXPORTED',report)

# Deterministic, seamlessly looped rotor sound; no downloaded audio dependency.
def write_rotor_audio(path):
 rng=random.Random(2124)
 partials=[(rng.randint(1000,6000)/4,rng.random()*math.tau) for _ in range(48)]
 samples=bytearray()
 for i in range(22050*4):
  t=i/22050
  pulse=(.5+.5*math.cos(math.tau*26*t))**3
  noise=sum(math.sin(math.tau*f*t+a) for f,a in partials)/math.sqrt(48)
  value=.38*(.22+.78*pulse)*(math.sin(math.tau*104*t)+.34*math.sin(math.tau*156*t))+.09*noise*pulse
  samples.extend(struct.pack('<h',max(-32767,min(32767,round(value*18000)))))
 with wave.open(str(path),'wb') as clip:
  clip.setnchannels(1);clip.setsampwidth(2);clip.setframerate(22050);clip.writeframes(samples)
write_rotor_audio(OUT/'PV_HelicopterRotor.wav')
