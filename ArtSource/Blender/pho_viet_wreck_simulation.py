"""Blender MCP: bake Mantaflow fuel combustion, then render mobile fire/smoke atlases.
Simulation cache is reproducible in Temp/WreckGasCache96; source and PNGs are durable.
"""
import bpy,os,json
from mathutils import Vector
from array import array
ROOT=r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT=ROOT+'/Assets/PhoViet/Resources/Wreck';os.makedirs(OUT,exist_ok=True)
def normalize_smoke_atlas(path):
 # All frames share one alpha crop, preserving the simulation's relative motion
 # while removing empty domain space so runtime puffs are centred on the emitter.
 image=bpy.data.images.load(path,check_existing=False);src=array('f',[0.0])*(768*384*4);image.pixels.foreach_get(src)
 xmin=ymin=192;xmax=ymax=-1
 for i in range(8):
  col=i%4;row=i//4
  for y in range(192):
   for x in range(192):
    if src[((row*192+y)*768+col*192+x)*4+3]>.025:xmin=min(xmin,x);xmax=max(xmax,x);ymin=min(ymin,y);ymax=max(ymax,y)
 if xmax<0:raise RuntimeError('Smoke atlas is empty')
 width=xmax-xmin+1;height=ymax-ymin+1;side=max(width,height)+12;result=array('f',[0.0])*(768*384*4)
 for i in range(8):
  col=i%4;row=i//4;temp=bpy.data.images.new('Smoke crop frame',width=side,height=side,alpha=True);buf=array('f',[0.0])*(side*side*4)
  for y in range(height):
   start=(((side-height)//2+y)*side+(side-width)//2)*4;old=((row*192+ymin+y)*768+col*192+xmin)*4;buf[start:start+width*4]=src[old:old+width*4]
  temp.pixels.foreach_set(buf);temp.scale(192,192);tile=array('f',[0.0])*(192*192*4);temp.pixels.foreach_get(tile)
  for y in range(192):
   start=((row*192+y)*768+col*192)*4;result[start:start+192*4]=tile[y*192*4:(y+1)*192*4]
  bpy.data.images.remove(temp)
 image.pixels.foreach_set(result);image.filepath_raw=path;image.file_format='PNG';image.save();bpy.data.images.remove(image)
 return [xmin,ymin,xmax,ymax]
scene=bpy.data.scenes.new('PV_Wreck_GasSimulation');bpy.context.window.scene=scene
scene.render.engine='CYCLES';scene.cycles.samples=128;scene.cycles.use_denoising=True
scene.render.resolution_x=192;scene.render.resolution_y=192;scene.render.resolution_percentage=100
scene.render.film_transparent=True;scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Wreck smoke daylight');world.use_nodes=True
bg=next(n for n in world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs['Strength'].default_value=.3;scene.world=world
bpy.ops.mesh.primitive_cube_add(size=2,location=(0,0,3.9));domain=bpy.context.object;domain.name='Mantaflow combustion domain';domain.scale=(3,3,4)
bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
mod=domain.modifiers.new('Fuel combustion','FLUID');mod.fluid_type='DOMAIN';ds=mod.domain_settings;ds.domain_type='GAS'
ds.resolution_max=96;ds.vorticity=.7;ds.burning_rate=1.2;ds.flame_vorticity=1.7;ds.flame_smoke=.8
ds.use_dissolve_smoke=True;ds.dissolve_speed=24
ds.cache_type='MODULAR';ds.cache_frame_start=1;ds.cache_frame_end=48;ds.cache_directory=ROOT+'/Temp/WreckGasCache96'
bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=.32,location=(0,0,.18));source=bpy.context.object;source.name='Burning engine fuel source';source.scale=(1.4,1, .6)
bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
flow=source.modifiers.new('Continuous burning fuel','FLUID');flow.fluid_type='FLOW';fs=flow.flow_settings;fs.flow_type='BOTH';fs.flow_behavior='INFLOW';fs.fuel_amount=1.4;fs.density=.65;fs.temperature=2
fs.use_initial_velocity=True;fs.velocity_coord=(0,0,.9);fs.velocity_random=.45;source.hide_render=True
for f,x in [(1,-.06),(12,.12),(24,-.14),(36,.09),(48,-.06)]:source.location.x=x;source.keyframe_insert(data_path='location',frame=f)
mat=bpy.data.materials.new('Physical flame blackbody and soot volume');mat.use_nodes=True;n=mat.node_tree.nodes;n.clear();l=mat.node_tree.links
info=n.new('ShaderNodeVolumeInfo');vol=n.new('ShaderNodeVolumePrincipled');out=n.new('ShaderNodeOutputMaterial')
vol.inputs['Color'].default_value=(.025,.028,.03,1);vol.inputs['Color Attribute'].default_value='';vol.inputs['Density Attribute'].default_value='';vol.inputs['Temperature Attribute'].default_value=''
def mul(socket,k):
 m=n.new('ShaderNodeMath');m.operation='MULTIPLY';l.new(socket,m.inputs[0]);m.inputs[1].default_value=k;return m.outputs[0]
smokeGain=n.new('ShaderNodeValue');smokeGain.outputs[0].default_value=3
density=n.new('ShaderNodeMath');density.operation='MULTIPLY';l.new(info.outputs['Density'],density.inputs[0]);l.new(smokeGain.outputs[0],density.inputs[1])
# Dissipate at the cache boundary rather than displaying a clipped, flat smoke lid.
coord=n.new('ShaderNodeTexCoord');separate=n.new('ShaderNodeSeparateXYZ');l.new(coord.outputs['Generated'],separate.inputs[0])
fade=n.new('ShaderNodeMapRange');fade.clamp=True;fade.inputs['From Min'].default_value=.72;fade.inputs['From Max'].default_value=1;fade.inputs['To Min'].default_value=1;fade.inputs['To Max'].default_value=0;l.new(separate.outputs['Z'],fade.inputs['Value'])
soft=n.new('ShaderNodeMath');soft.operation='MULTIPLY';l.new(density.outputs[0],soft.inputs[0]);l.new(fade.outputs[0],soft.inputs[1]);l.new(soft.outputs[0],vol.inputs['Density'])
fireGain=n.new('ShaderNodeValue');fireGain.outputs[0].default_value=2
burn=n.new('ShaderNodeMath');burn.operation='MULTIPLY';l.new(info.outputs['Flame'],burn.inputs[0]);l.new(fireGain.outputs[0],burn.inputs[1]);l.new(burn.outputs[0],vol.inputs['Blackbody Intensity'])
heat=n.new('ShaderNodeMath');heat.operation='ADD';l.new(mul(info.outputs['Flame'],950),heat.inputs[0]);heat.inputs[1].default_value=1100;l.new(heat.outputs[0],vol.inputs['Temperature'])
l.new(vol.outputs['Volume'],out.inputs['Volume']);domain.data.materials.append(mat)
data=bpy.data.cameras.new('Wreck atlas camera');camera=bpy.data.objects.new('Wreck atlas camera',data);scene.collection.objects.link(camera)
camera.location=(0,-8,2.25);camera.rotation_euler=(Vector((0,0,2.25))-camera.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=5.0;scene.camera=camera
for name,loc,power in [('Smoke key',(-3,-4,5),1800),('Smoke rim',(3,2,4),2100)]:
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=4
 o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,2))-o.location).to_track_quat('-Z','Y').to_euler()
scene.frame_start=1;scene.frame_end=48;scene.render.fps=24
bpy.context.view_layer.objects.active=domain;domain.select_set(True);source.select_set(False)
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Blender/PhoViet_WreckFire.blend')
bpy.ops.fluid.bake_data()
scene.frame_set(36);scene.render.filepath=ROOT+'/Captures/wreck-fire-simulation-preview.png';bpy.ops.render.render(write_still=True)
def bake_simulation_atlases():
 for name,fire in [('PV_WreckFlame',True),('PV_WreckSmoke',False)]:
  smokeGain.outputs[0].default_value=2 if fire else 3;fireGain.outputs[0].default_value=2 if fire else 0
  l.new(info.outputs['Flame' if fire else 'Density'],density.inputs[0])
  camera.data.ortho_scale=2.6 if fire else 6.4;camera.location.z=1.1 if fire else 2.9
  atlas=bpy.data.images.new(name,width=768,height=384,alpha=True);pixels=array('f',[0.0])*(768*384*4)
  for i in range(8):
   scene.frame_set(25+i*3);path=OUT+'/gas-frame-'+str(i)+'.png';scene.render.filepath=path;bpy.ops.render.render(write_still=True)
   img=bpy.data.images.load(path,check_existing=False);src=array('f',[0.0])*(192*192*4);img.pixels.foreach_get(src)
   for y in range(192):
    start=(((1-i//4)*192+y)*768+(i%4)*192)*4;pixels[start:start+192*4]=src[y*192*4:(y+1)*192*4]
   bpy.data.images.remove(img);os.remove(path)
  atlas.pixels.foreach_set(pixels);atlas.filepath_raw=OUT+'/'+name+'.png';atlas.file_format='PNG';atlas.save()
  if not fire:normalize_smoke_atlas(atlas.filepath_raw)
 smokeGain.outputs[0].default_value=3;fireGain.outputs[0].default_value=2;camera.data.ortho_scale=5;camera.location.z=2.25;scene.frame_set(36)
 bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Blender/PhoViet_WreckFire.blend')
 print(json.dumps({'success':True,'simulation':'Mantaflow gas fuel combustion','resolution':96,'simulationFrames':48,'atlasFrames':8,'size':[768,384]}))
print(json.dumps({'baked':ds.has_cache_baked_data,'frame':scene.frame_current,'preview':scene.render.filepath}))
if globals().get('__name__')=='__main__':bake_simulation_atlases()
