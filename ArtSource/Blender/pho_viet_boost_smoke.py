"""Run via Blender MCP. Bake a real volumetric white exhaust puff to a mobile flipbook.
The editable volume, noise, lights and keyed exhaust preview stay in a separate .blend.
"""
import bpy, math, os, json
from mathutils import Vector
from array import array
ROOT=r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT=ROOT+'/Assets/PhoViet/Resources/Boost'
os.makedirs(OUT,exist_ok=True)
scene=bpy.data.scenes.new('PV_BoostSmoke')
bpy.context.window.scene=scene
scene.render.engine='CYCLES'
scene.cycles.samples=64
scene.cycles.use_denoising=True
scene.render.resolution_x=192;scene.render.resolution_y=192;scene.render.resolution_percentage=100
scene.render.film_transparent=True
scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
scene.view_settings.view_transform='Standard'
world=bpy.data.worlds.new('Boost smoke soft light');world.use_nodes=True
world.node_tree.nodes.get('Background').inputs['Color'].default_value=(1,1,1,1)
world.node_tree.nodes.get('Background').inputs['Strength'].default_value=.8
scene.world=world
mat=bpy.data.materials.new('White turbulent exhaust volume');mat.use_nodes=True
n=mat.node_tree.nodes;n.clear();links=mat.node_tree.links
out=n.new('ShaderNodeOutputMaterial');vol=n.new('ShaderNodeVolumePrincipled')
vol.inputs['Color'].default_value=(.97,.99,1,1);vol.inputs['Anisotropy'].default_value=.1
tex=n.new('ShaderNodeTexCoord');noise=n.new('ShaderNodeTexNoise');noise.noise_dimensions='4D'
noise.inputs['Scale'].default_value=4.3;noise.inputs['Detail'].default_value=3;noise.inputs['Roughness'].default_value=.7
links.new(tex.outputs['Generated'],noise.inputs['Vector'])
sub=n.new('ShaderNodeVectorMath');sub.operation='SUBTRACT';sub.inputs[1].default_value=(.5,.5,.5)
links.new(tex.outputs['Generated'],sub.inputs[0])
length=n.new('ShaderNodeVectorMath');length.operation='LENGTH';links.new(sub.outputs['Vector'],length.inputs[0])
fall=n.new('ShaderNodeMath');fall.operation='SUBTRACT';fall.inputs[0].default_value=.53;links.new(length.outputs['Value'],fall.inputs[1])
cloud=n.new('ShaderNodeMath');cloud.operation='SUBTRACT';cloud.inputs[1].default_value=.31;cloud.use_clamp=True;links.new(noise.outputs['Fac'],cloud.inputs[0])
mul=n.new('ShaderNodeMath');mul.operation='MULTIPLY';links.new(fall.outputs[0],mul.inputs[0]);links.new(cloud.outputs[0],mul.inputs[1])
gain=n.new('ShaderNodeMath');gain.operation='MULTIPLY';gain.inputs[1].default_value=13;links.new(mul.outputs[0],gain.inputs[0])
links.new(gain.outputs[0],vol.inputs['Density']);links.new(vol.outputs['Volume'],out.inputs['Volume'])
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3,radius=1)
puff=bpy.context.object;puff.name='Volumetric white puff';puff.data.materials.append(mat)
for v in puff.data.vertices:
    x,y,z=v.co;v.co*=1+.12*math.sin(x*7+z*5)+.12*math.cos(y*7-x*3)+.05*math.sin(y*z*8)
puff.scale=(1.05,.8,.85)
camera_data=bpy.data.cameras.new('Smoke bake camera');camera=bpy.data.objects.new('Smoke bake camera',camera_data);scene.collection.objects.link(camera)
camera.location=(0,-5,.2);camera.rotation_euler=(Vector((0,0,0))-camera.location).to_track_quat('-Z','Y').to_euler()
camera_data.type='ORTHO';camera_data.ortho_scale=2.5;scene.camera=camera
for name,pos,power,size in [('Key',(1,-3,4),1100,4),('Fill',(-3,-2,1),850,3)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size
    lamp=bpy.data.objects.new(name,data);scene.collection.objects.link(lamp);lamp.location=pos
    lamp.rotation_euler=(-lamp.location).to_track_quat('-Z','Y').to_euler()
frames=[]
for i in range(4):
    frame=1+i*8;noise.inputs['W'].default_value=i*.25
    noise.inputs['W'].keyframe_insert(data_path='default_value',frame=frame)
    puff.rotation_euler.z=i*.23;puff.keyframe_insert(data_path='rotation_euler',frame=frame)
    scene.frame_set(frame);scene.render.filepath=OUT+'/smoke-frame-'+str(i)+'.png'
    bpy.ops.render.render(write_still=True)
    frames.append(scene.render.filepath)
atlas=bpy.data.images.new('PV_WhiteBoostSmoke',width=768,height=192,alpha=True)
pixels=array('f',[0.0])*(768*192*4)
for i,path in enumerate(frames):
    img=bpy.data.images.load(path,check_existing=False);source=array('f',[0.0])*(192*192*4);img.pixels.foreach_get(source)
    for y in range(192):pixels[(y*768+i*192)*4:(y*768+(i+1)*192)*4]=source[y*192*4:(y+1)*192*4]
    bpy.data.images.remove(img)
atlas.pixels.foreach_set(pixels);atlas.filepath_raw=OUT+'/PV_WhiteBoostSmoke.png';atlas.file_format='PNG';atlas.save()
for path in frames:os.remove(path)
scene.frame_start=1;scene.frame_end=25;scene.render.fps=24
# The live preview shows the same puff expanding and moving away from an exhaust.
for frame,position,scale in [(1,(0,0,0),(.18,.24,.18)),(13,(0,-1.8,.12),(.60,.85,.6)),(25,(0,-3.8,.32),(1.05,1.3,.95))]:
    puff.location=position;puff.scale=scale;puff.keyframe_insert(data_path='location',frame=frame);puff.keyframe_insert(data_path='scale',frame=frame)
scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Blender/PhoViet_BoostSmoke.blend')
print(json.dumps({'success':True,'texture':atlas.filepath_raw,'size':[768,192],'frames':4,'source':bpy.data.filepath}))
