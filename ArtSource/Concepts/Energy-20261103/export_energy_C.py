import bpy,math,json
from pathlib import Path
from mathutils import Vector,Matrix
ROOT=Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe')
scene=bpy.data.scenes.new('PV_Energy_C_Runtime');bpy.context.window.scene=scene
source=bpy.data.objects['C_HaloBolt_Core'];deps=bpy.context.evaluated_depsgraph_get()
# Export one vertex-coloured mesh per LOD: all three C parts remain one draw call.
for level,segments,minor in [(0,48,8),(1,28,6),(2,16,4)]:
 verts=[];faces=[];colors=[]
 temp=source.copy();temp.data=source.data.copy();scene.collection.objects.link(temp);temp.parent=None;temp.matrix_world=Matrix.Identity(4)
 for mod in temp.modifiers:
  if mod.type=='BEVEL':mod.segments=[3,2,1][level]
 deps.update();evaluated=temp.evaluated_get(deps);me=evaluated.to_mesh()
 verts.extend([tuple(v.co*.31) for v in me.vertices])
 for poly in me.polygons:
  faces.append(tuple(poly.vertices));ma=me.materials[poly.material_index];n=next(n for n in ma.node_tree.nodes if n.type=='BSDF_PRINCIPLED');c=tuple(n.inputs['Base Color'].default_value[:3]);colors.append((*c,.32 if poly.material_index!=1 else .06))
 evaluated.to_mesh_clear();bpy.data.objects.remove(temp,do_unlink=True)
 for radius,tube,y,col in [(1.59,.11,.22,(.018,.12,.24,.10)),(1.56,.035,.075,(.01,.62,1,1))]:
  offset=len(verts)
  for a in range(segments):
   theta=a/segments*math.tau
   for b in range(minor):
    phi=b/minor*math.tau;r=radius+tube*math.cos(phi)
    verts.append((r*math.cos(theta)*.31,(y+tube*math.sin(phi))*.31,r*math.sin(theta)*.31))
  for a in range(segments):
   for b in range(minor):faces.append(tuple(offset+aa*minor+bb for aa,bb in [(a,b),((a+1)%segments,b),((a+1)%segments,(b+1)%minor),(a,(b+1)%minor)]));colors.append(col)
 mesh=bpy.data.meshes.new('PV_EnergyHalo_LOD'+str(level));mesh.from_pydata(verts,[],faces);mesh.update()
 typ=next(i.identifier for i in bpy.types.Attribute.bl_rna.properties['data_type'].enum_items if i.identifier=='FLOAT_COLOR')
 domain=next(i.identifier for i in bpy.types.Attribute.bl_rna.properties['domain'].enum_items if i.identifier=='CORNER')
 attr=mesh.color_attributes.new(name='Color',type=typ,domain=domain)
 for poly,col in zip(mesh.polygons,colors):
  poly.use_smooth=len(poly.vertices)==4
  for loop in poly.loop_indices:attr.data[loop].color=col
 obj=bpy.data.objects.new(mesh.name,mesh);scene.collection.objects.link(obj)
 obj.select_set(True)
fbx=ROOT/'Assets/PhoViet/Art/Energy/PV_EnergyHalo.fbx'
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,path_mode='RELATIVE')
# Separate transparent HUD render from the approved C object, with no plinth or text.
hud=bpy.data.scenes.new('PV_Energy_C_Hud');bpy.context.window.scene=hud
for child in bpy.data.objects['C_HaloBolt'].children:
 o=child.copy();o.data=child.data.copy();hud.collection.objects.link(o);o.parent=None;o.matrix_world=child.matrix_local.copy()
world=bpy.data.worlds.new('PV_Energy_Hud_World');world.use_nodes=True;hud.world=world
bg=next(n for n in world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs['Color'].default_value=(.1,.25,.4,1);bg.inputs['Strength'].default_value=.6
for loc,power,size in [((-4,-6,8),1000,5),((5,-2,3),600,4)]:
 d=bpy.data.lights.new('HUD softbox','AREA');d.energy=power;d.size=size;o=bpy.data.objects.new(d.name,d);hud.collection.objects.link(o);o.location=loc;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
d=bpy.data.cameras.new('HUD Camera');o=bpy.data.objects.new(d.name,d);hud.collection.objects.link(o);hud.camera=o;o.location=(2,-10,3);o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler();d.type='ORTHO';d.ortho_scale=3.7
hud.render.resolution_x=hud.render.resolution_y=256;hud.render.resolution_percentage=100;hud.render.film_transparent=True;hud.render.image_settings.file_format='PNG';hud.render.image_settings.color_mode='RGBA';hud.render.filepath=str(ROOT/'Assets/PhoViet/Resources/Energy/PV_EnergyHaloHud.png')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_Energy_C.blend'))
print(json.dumps({'fbx':str(fbx),'LODs':[(o.name,len(o.data.polygons)) for o in scene.objects],'hudReady':True}))
