import bpy, math, json
from pathlib import Path
from mathutils import Vector
OUT=Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/Energy-20261103')
scene=bpy.data.scenes.new('PV_Energy_Approval_2121')
bpy.context.window.scene=scene
scene.world=bpy.data.worlds.new('Energy_Studio_World');scene.world.use_nodes=True
bg=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs['Color'].default_value=(.035,.075,.14,1);bg.inputs['Strength'].default_value=.45
scene.render.resolution_x=1800;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
scene.view_settings.view_transform='AgX'

def mat(name,color,metal=0,rough=.3,emission=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
 n.inputs['Base Color'].default_value=(*color,1);n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=rough
 n.inputs['Emission Color'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=emission
 return m
mats={
 'side':mat('PV_Energy_DeepBlue',(.008,.12,.52),.55,.22),
 'cyan':mat('PV_Energy_Turquoise',(.005,.65,.95),.32,.20,.22),
 'edge':mat('PV_Energy_IceEdge',(.08,.8,1),.25,.20,.4),
 'blue':mat('PV_Energy_Cobalt',(.015,.30,.95),.45,.24,.15),
 'facet':mat('PV_Energy_LightFacet',(.11,.72,1),.35,.23,.2),
 'white':mat('PV_Energy_Highlight',(.65,.96,1),.15,.18,.6),
 'glow':mat('PV_Energy_RingLight',(.015,.55,1),.35,.22,2),
 'metal':mat('PV_Energy_BlueMetal',(.018,.12,.24),.8,.26),
 'floor':mat('PV_Energy_Studio',(.012,.032,.065),.15,.5),
 'text':mat('PV_Energy_Labels',(.6,.83,1),.1,.5,.3)}
points=[(.12,1.35),(-.95,-.1),(-.18,-.1),(-.46,-1.35),(.95,.30),(.15,.30),(.61,1.35)]
roots=[]
def mesh_obj(name,verts,faces,material):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(name,me);scene.collection.objects.link(o);o.data.materials.append(material);return o

def bolt(name,x,bevel=.10,front='cyan',depth=.42):
 root=bpy.data.objects.new(name,None);scene.collection.objects.link(root);root.location=(x,0,2.3);root.rotation_euler[2]=math.radians(-8)
 n=len(points);vs=[(px,y,pz) for y in [-depth/2,depth/2] for px,pz in points]
 faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 o=mesh_obj(name+'_Core',vs,faces,mats[front]);o.data.materials.append(mats['side']);o.data.materials.append(mats['edge'])
 for p in o.data.polygons:p.material_index=0 if p.index==0 else 1
 o.parent=root
 b=o.modifiers.new('Soft manufactured edge','BEVEL');b.width=bevel;b.segments=4 if bevel>.06 else 2;b.material=2
 o.modifiers.new('Weighted surface normals','WEIGHTED_NORMAL')
 roots.append(root);return root,o

def face(name,root,coords,y,material):
 o=mesh_obj(name,[(x,y,z) for x,z in coords],[tuple(range(len(coords)))],material);o.parent=root;return o

a,ao=bolt('A_SoftBolt',-3.55,.12,'cyan',.52)
# Small specular inlay shapes emphasize rounded volume without white glare hiding the silhouette.
face('A_UpperGlint',a,[(-.33,.63),(-.12,.99),(.14,1.19),(.26,1.19),(.10,1.05)],-.273,mats['white'])
face('A_LowerGlint',a,[(.39,.23),(.52,.23),(.06,-.28),(.10,-.09)],-.273,mats['white'])
b,bo=bolt('B_CrystalBolt',0,.035,'blue',.64)
face('B_UpperFacet',b,[(.13,1.24),(-.73,.01),(-.11,.01),(.31,.72)],-.331,mats['facet'])
face('B_TopFacet',b,[(.13,1.24),(.48,1.24),(.16,.39),(-.11,.01)],-.334,mats['cyan'])
face('B_LowerFacet',b,[(.15,.21),(.75,.21),(-.34,-1.13),(-.04,-.06)],-.333,mats['edge'])
c,co=bolt('C_HaloBolt',3.55,.075,'facet',.40)
# Real mesh halo behind the bolt, around the Y axis.
for radius,minor,y,ma,suffix in [(1.59,.11,.22,'metal','Shell'),(1.56,.035,.075,'glow','InnerLight')]:
 bpy.ops.mesh.primitive_torus_add(major_radius=radius,minor_radius=minor,major_segments=80,minor_segments=12,location=(0,0,0))
 o=bpy.context.object;o.name='C_Halo_'+suffix;o.parent=c;o.location=(0,y,0);o.rotation_euler[0]=math.pi/2;o.data.materials.append(mats[ma])
 for p in o.data.polygons:p.use_smooth=True
# Ground plane and subtly elevated discs only belong to the presentation scene.
bpy.ops.mesh.primitive_plane_add(size=200);floor=bpy.context.object;floor.name='Presentation_Backdrop';floor.data.materials.append(mats['floor'])
for i,x in enumerate([-3.55,0,3.55]):
 bpy.ops.mesh.primitive_cylinder_add(vertices=96,radius=1.48,depth=.09,location=(x,.0,.05));o=bpy.context.object;o.name='Presentation_Platform_'+str(i);o.data.materials.append(mats['metal'])
 bmod=o.modifiers.new('Platform bevel','BEVEL');bmod.width=.06;bmod.segments=3;o.modifiers.new('Platform normals','WEIGHTED_NORMAL')
 # Letter and subtitle face the camera, rendered in Blender.
 for body,z,size in [(chr(65+i),.53,.35),(['SOFT BOLT','CRYSTAL BOLT','HALO BOLT'][i],.10,.14)]:
  curve=bpy.data.curves.new('Label_'+body,'FONT');curve.body=body;curve.align_x='CENTER';curve.size=size;curve.extrude=.001
  obj=bpy.data.objects.new('Label_'+body,curve);scene.collection.objects.link(obj);obj.location=(x,-1.9,z);obj.rotation_euler=(math.pi/2,0,0);obj.data.materials.append(mats['text'])

def area(name,loc,power,color,size):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=size;o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,2))-o.location).to_track_quat('-Z','Y').to_euler()
area('Large softbox',(-5,-8,10),1900,(.73,.92,1),7)
area('Blue rim',(4,3,7),1800,(.1,.45,1),5)
area('Front fill',(4,-5,4),800,(.72,.85,1),5)
camd=bpy.data.cameras.new('Energy_Approval_Camera');cam=bpy.data.objects.new('Energy_Approval_Camera',camd);scene.collection.objects.link(cam);scene.camera=cam
cam.location=(3.0,-22,8.8);target=Vector((0,-.1,1.8));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();camd.type='ORTHO';camd.ortho_scale=12.4
scene.render.filepath=str(OUT/'energy-abc-board.png')
scene['user_approval_required']=True;scene['installed_in_game']=False
scene['design_labels']='A Soft Bolt / B Crystal Bolt / C Halo Bolt'
# The original startup scene is left intact; only this new scene is saved as a review file.
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'PhoViet_Energy_Approval.blend'))
print(json.dumps({'scene':scene.name,'objects':len(scene.objects),'models':[r.name for r in roots],'file':bpy.data.filepath,'engine':scene.render.engine}))

# Final preview adjustments: keep the brighter material values below reproducible.
import bmesh
for obj in scene.objects:
 if obj.type=='MESH' and obj.name.endswith('_Core'):
  bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
for name,color in [('PV_Energy_Turquoise',(.003,.42,.8)),('PV_Energy_LightFacet',(.008,.28,.8)),('PV_Energy_IceEdge',(.01,.62,1)),('PV_Energy_Cobalt',(.005,.12,.6))]:
 n=next(n for n in bpy.data.materials[name].node_tree.nodes if n.type=='BSDF_PRINCIPLED');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Metallic'].default_value=.08;n.inputs['Emission Color'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=.30
for name,angle in [('A_SoftBolt',-22),('B_CrystalBolt',-18),('C_HaloBolt',-16)]:bpy.data.objects[name].rotation_euler[2]=math.radians(angle)
for name,power in [('Large softbox',950),('Blue rim',950),('Front fill',350)]:bpy.data.lights[name].energy=power
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'PhoViet_Energy_Approval.blend'))
bpy.ops.render.render(write_still=True)
