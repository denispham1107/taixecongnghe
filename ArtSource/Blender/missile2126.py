import bpy,math,json,random
from pathlib import Path
from mathutils import Vector, noise
ROOT=Path(r"C:/Users/HP/Documents/GameUnity/Taixecongnghe")
OUT=ROOT/'Assets/PhoViet/Art/Helicopter/Missile';OUT.mkdir(parents=True,exist_ok=True)
assert 'PV_Missile2126' not in bpy.data.scenes,'Already built; inspect rather than duplicate'
previous_scene=bpy.context.window.scene
scene=bpy.data.scenes.new('PV_Missile2126');bpy.context.window.scene=scene
scene.render.fps=30;scene.frame_start=1;scene.frame_end=61
bodyparts=[]
def material(name,color,metal=.25,rough=.4,emission=0):
 m=bpy.data.materials.new(name);m.diffuse_color=color;m.use_nodes=True
 b=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');b.inputs['Base Color'].default_value=color;b.inputs['Metallic'].default_value=metal;b.inputs['Roughness'].default_value=rough
 if emission:b.inputs['Emission Color'].default_value=color;b.inputs['Emission Strength'].default_value=emission
 return m
pearl=material('Missile Pearl ceramic',(.78,.83,.88,1));red=material('Missile Signal red',(.72,.045,.025,1));dark=material('Missile Carbon fins',(.06,.09,.12,1),.65);gold=material('Missile Safety band',(1,.48,.03,1))
flamemat=material('Missile Exhaust hot',(1,.35,.06,.7),0,.85,3)
def cylinder(name,radius,depth,y,mat,radius2=None,vertices=20):
 if radius2 is None:bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=(0,y,0),rotation=(math.pi/2,0,0))
 else:bpy.ops.mesh.primitive_cone_add(vertices=vertices,radius1=radius,radius2=radius2,depth=depth,location=(0,y,0),rotation=(math.pi/2,0,0))
 o=bpy.context.object;o.name=name;o.data.materials.append(mat);bodyparts.append(o);return o
cylinder('Ceramic body',.115,1.03,.035,pearl)
cylinder('Red ogive',.115,.30,-.63,red,.015)
cylinder('Graphite nose tip',.025,.075,-.81,dark,.004)
cylinder('Nozzle collar',.123,.09,.57,dark)
cylinder('Nozzle aperture',.085,.035,.63,gold)
for y in [-.42,.39]:cylinder('Band',.117,.055,y,gold)
for n in range(4):
 a=n*math.pi/2
 verts=[]
 for t in [-.012,.012]:
  for radius,y in [(.105,.06),(.27,.40),(.27,.57),(.105,.51)]:verts.append((math.cos(a)*radius+math.sin(a)*t,y,math.sin(a)*radius-math.cos(a)*t))
 faces=[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]
 me=bpy.data.meshes.new('Fin');me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new('Swept stabilizer '+str(n),me);scene.collection.objects.link(o);o.data.materials.append(dark);bodyparts.append(o)
# Bake per-corner author colors into the one shared runtime surface.
for o in bodyparts:
 bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);o.select_set(False)
 col=o.data.color_attributes.new(name='Col',type='BYTE_COLOR',domain='CORNER')
 for q in col.data:q.color=o.data.materials[0].diffuse_color
for o in bodyparts:o.select_set(True)
bpy.context.view_layer.objects.active=bodyparts[0];bpy.ops.object.join();body=bpy.context.object;body.name='PV_MissileBody_LOD0'
baked=material('Missile baked vertex colors',(.8,.8,.8,1));vc=baked.node_tree.nodes.new('ShaderNodeVertexColor');vc.layer_name='Col';bs=next(n for n in baked.node_tree.nodes if n.type=='BSDF_PRINCIPLED');baked.node_tree.links.new(vc.outputs['Color'],bs.inputs['Base Color']);body.data.materials.clear();body.data.materials.append(baked)
for p in body.data.polygons:p.material_index=0
for p in body.data.polygons:p.use_smooth=True
bpy.ops.object.select_all(action='DESELECT');lods=[body]
for i,ratio in [(1,.56),(2,.27)]:
 o=body.copy();o.data=body.data.copy();scene.collection.objects.link(o);o.name='PV_MissileBody_LOD'+str(i);bpy.context.view_layer.objects.active=o
 mod=o.modifiers.new('MissileLOD','DECIMATE');mod.ratio=ratio;mod.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=mod.name);lods.append(o)
# Flame core and motion pulse authored here, reused by Unity; soft outer plume uses the atlas below.
bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,location=(0,1.04,0),scale=(.065,.42,.065));flame=bpy.context.object;flame.name='PV_MissileExhaust';flame.data.materials.append(flamemat)
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
for v in flame.data.vertices:v.co.y-=.64
flame.location.y=.64
for p in flame.data.polygons:p.use_smooth=True
col=flame.data.color_attributes.new(name='Col',type='BYTE_COLOR',domain='CORNER')
for q in col.data:q.color=(1,.38,.07,.65)
bpy.ops.object.select_all(action='DESELECT')
for o in lods+[flame]:o.select_set(True)
bpy.context.view_layer.objects.active=body
bpy.ops.export_scene.fbx(filepath=str(OUT/'PV_Missile.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,add_leaf_bones=False)
for o in lods[1:]:o.hide_render=True;o.hide_set(True)
rig=bpy.data.objects.new('Missile flight authored',None);scene.collection.objects.link(rig)
body.parent=flame.parent=rig
pulse=[1,.84,1.20,.93,1.12,1]
for i,v in enumerate(pulse):flame.scale=(1,v,1);flame.keyframe_insert(data_path='scale',frame=1+i)
for frame,y in [(1,0),(31,-50),(61,-100)]:rig.location=(0,y,0);rig.keyframe_insert(data_path='location',frame=frame)
# Baked volumetric-looking white exhaust: 4 evolving noise-density lobes per sheet.
N=128;img=bpy.data.images.new('PV_MissileExhaustSmoke',width=N*4,height=N,alpha=True);pixels=[0.]*(N*N*4*4)
for f in range(4):
 for y in range(N):
  for x in range(N):
   u=(x+.5)/N*2-1;v=(y+.5)/N*2-1
   turb=noise.fractal(Vector((u*2.7,v*2.7,f*.39)),1.0,2.0,3)
   r=math.sqrt((u+.10*math.sin(v*8+f))**2+(v+.06*math.cos(u*9-f))**2)
   density=max(0,1-r/(.75+.12*turb));alpha=min(.84,density**1.1*(.52+.38*max(0,turb)))
   light=.7+.25*max(0,1-r)+.05*turb
   k=(y*N*4+f*N+x)*4;pixels[k:k+4]=[light,light*.98,light*.94,alpha]
img.pixels.foreach_set(pixels);img.filepath_raw=str(OUT/'PV_MissileSmoke.png');img.file_format='PNG';img.save();img.pack()
scene.world=bpy.data.worlds.new('Missile studio');scene.world.use_nodes=True
next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND').inputs[0].default_value=(.07,.10,.16,1)
for name,location,power,size in [('Key',(3,-3,5),700,4),('Rim',(-3,2,3),900,3)]:
 l=bpy.data.lights.new(name,'AREA');l.energy=power;l.shape='DISK';l.size=size;o=bpy.data.objects.new(name,l);scene.collection.objects.link(o);o.location=location;o.rotation_euler=(Vector((0,0,0))-o.location).to_track_quat('-Z','Y').to_euler()
scene.frame_set(1)
report={'success':True,'length':1.50,'forwardUnity':[0,0,1],'lodTriangles':[],'plumePulse':pulse,'plumePeriod':.16666667,'missileSpeed':50,'smokeSheet':[4,1],'smokeMaxParticles':24,'smokeRate':50,'smokeLifetime':.48}
for o in lods:o.data.calc_loop_triangles();report['lodTriangles'].append(len(o.data.loop_triangles))
(OUT/'PV_Missile.json').write_text(json.dumps(report,indent=2),encoding='utf8');(ROOT/'Captures/missile2126-blender.json').write_text(json.dumps(report,indent=2),encoding='utf8')
bpy.data.libraries.write(str(ROOT/'ArtSource/Blender/PhoViet_HelicopterMissile.blend'),{scene},fake_user=True)
print(json.dumps(report))
