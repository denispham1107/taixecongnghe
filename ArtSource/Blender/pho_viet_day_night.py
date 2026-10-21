"""Run through Blender MCP. Original LED fixture, optical light cookies and 60 s lighting study.
Writes a separate .blend library; never overwrites the user's other Blender source files.
"""
import bpy, math, json, os
from mathutils import Vector
ROOT = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT = ROOT + '/Assets/PhoViet/Art/Lighting'
os.makedirs(OUT, exist_ok=True)

def linear(h):
    return tuple((int(h[i:i+2],16)/255)**2.2 for i in (0,2,4))+(1,)
def material(name, color, metal=0, rough=.45, emission=0):
    m=bpy.data.materials.new(name);m.use_nodes=True
    p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value=linear(color);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
    p.inputs['Emission Color'].default_value=linear(color);p.inputs['Emission Strength'].default_value=emission
    return m
def link(obj, collection):
    for c in list(obj.users_collection):c.objects.unlink(obj)
    collection.objects.link(obj);return obj
def cube(name, loc, scale, mat, collection, bevel=.03):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=link(bpy.context.object,collection);o.name=name;o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(mat)
    if bevel:
        b=o.modifiers.new('Rounded manufactured edges','BEVEL');b.width=bevel;b.segments=3
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name)
    return o
def rod(name, a, b, radius, mat, collection, vertices=12):
    a,b=Vector(a),Vector(b);bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=(b-a).length,location=(a+b)/2)
    o=link(bpy.context.object,collection);o.name=name;o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();o.data.materials.append(mat)
    return o
def lookat(obj, target):obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()
def light(name, kind, loc, energy, collection, target=None, color='FFFFFF'):
    data=bpy.data.lights.new(name,kind);data.energy=energy;data.color=linear(color)[:3]
    o=bpy.data.objects.new(name,data);collection.objects.link(o);o.location=loc
    if target is not None:lookat(o,target)
    return o
def render_settings(scene, size=512):
    try:scene.render.engine='BLENDER_EEVEE'
    except TypeError as e:raise RuntimeError('Connected Blender does not expose expected EEVEE engine: '+str(e))
    scene.render.resolution_x=size;scene.render.resolution_y=size;scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
    scene.view_settings.view_transform='Standard';scene.view_settings.exposure=0;scene.view_settings.gamma=1
    scene.render.film_transparent=False

def optical_psf():
    # Lens point-spread function: aperture core, scattered halo and restrained diffraction.
    # Authored as linear-light pixels in Blender; the black boundary is exactly zero.
    data=[];size=512
    for y in range(size):
        for x in range(size):
            dx=(x+.5)/size-.5;dy=(y+.5)/size-.5;r=math.hypot(dx,dy)
            value=math.exp(-(r/.032)**4)+.22*math.exp(-(r/.115)**2)+.045*math.exp(-(r/.245)**2)
            value+=.025*math.exp(-(dy/.006)**2-(dx/.19)**2)+.025*math.exp(-(dx/.006)**2-(dy/.19)**2)
            value*=max(0,1-(r/.49)**8);v=min(1,max(0,value));data.extend((v,v,v,1))
    image=bpy.data.images.new('PV_OpticalPSF',size,size,alpha=True);image.pixels=data;image.filepath_raw=OUT+'/PV_OpticalGlare.png';image.file_format='PNG';image.save()

def build():
    original=bpy.context.window.scene
    scene=bpy.data.scenes.new('PV • Day night optical studio');bpy.context.window.scene=scene
    render_settings(scene);scene.world=bpy.data.worlds.new('PV Optical Black');scene.world.use_nodes=True
    bg=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs['Color'].default_value=(0,0,0,1);bg.inputs['Strength'].default_value=0
    studio=bpy.data.collections.new('PV optical bake');scene.collection.children.link(studio)
    matte=material('PV cookie diffuse white','FFFFFF',rough=1)
    floor=cube('Optical receiver',(0,12,-.04),(50,56,.08),matte,studio,0)
    cam=bpy.data.objects.new('PV cookie camera',bpy.data.cameras.new('PV cookie camera'));studio.objects.link(cam)
    cam.location=(0,12,50);cam.rotation_euler=(0,0,0);cam.data.type='ORTHO';cam.data.ortho_scale=32;scene.camera=cam
    head=light('Real low beam photometric study','SPOT',(0,0,1.1),1600,studio,(0,13,0));head.data.spot_size=math.radians(58);head.data.spot_blend=.85;head.data.shadow_soft_size=.09
    scene.render.filepath=OUT+'/PV_HeadlightCookie.png';bpy.ops.render.render(write_still=True)
    head.hide_render=True;floor.location=(0,0,-.04);cam.location=(0,0,50);cam.data.ortho_scale=18
    pool=light('LED street luminaire distribution','SPOT',(0,0,6.65),900,studio,(.9,0,0));pool.data.spot_size=math.radians(99);pool.data.spot_blend=.8;pool.data.shadow_soft_size=.3
    scene.render.filepath=OUT+'/PV_StreetPool.png';bpy.ops.render.render(write_still=True);pool.hide_render=True;floor.hide_render=True
    # Optical glare is rendered from a real emissive aperture through a soft glow compositor.
    aperture=material('PV white optical aperture','FFFFFF',emission=9)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=12,radius=.035,location=(0,0,0));glow=link(bpy.context.object,studio);glow.name='LED aperture';glow.data.materials.append(aperture)
    cam.location=(0,0,5);cam.data.ortho_scale=1.2;scene.use_nodes=True
    compositor=bpy.data.node_groups.new('PV Optical compositor','CompositorNodeTree');scene.compositing_node_group=compositor
    compositor.interface.new_socket(name='Image',in_out='OUTPUT',socket_type='NodeSocketColor')
    nodes=compositor.nodes;r=nodes.new('CompositorNodeRLayers');g=nodes.new('CompositorNodeGlare')
    g.inputs['Type'].default_value='Fog Glow';g.inputs['Threshold'].default_value=.03;g.inputs['Size'].default_value=.95;g.inputs['Strength'].default_value=5
    out=nodes.new('NodeGroupOutput');compositor.links.new(r.outputs['Image'],g.inputs['Image']);compositor.links.new(g.outputs['Image'],out.inputs['Image'])
    scene.render.filepath=OUT+'/PV_OpticalGlare.png';bpy.ops.render.render(write_still=True)
    optical_psf();g.inputs['Threshold'].default_value=1.5;g.inputs['Strength'].default_value=.15;g.inputs['Size'].default_value=.65
    studio.hide_render=True;studio.hide_viewport=True

    fixture=bpy.data.collections.new('PV LED street fixture');scene.collection.children.link(fixture)
    metal=material('PV_LampMetal','344553',metal=.4,rough=.34);silver=material('PV_LampSilver','BFCED8',metal=.6,rough=.3);lens=material('PV_LampLens','F4FAFF',rough=.18,emission=2)
    # Blender: Z up; export conversion follows the existing game's X/Y/Z contract.
    rod('Tapered pole',(0,0,.15),(0,0,6.65),.075,metal,fixture,16)
    cube('Bolted base',(0,0,.13),(.34,.34,.26),metal,fixture,.045)
    for x in [-.12,.12]:
        for y in [-.12,.12]:rod('Foundation bolt',(x,y,.23),(x,y,.29),.018,silver,fixture,8)
    points=[(0,0,6.4),(-.12,0,6.67),(-.43,0,6.83),(-.82,0,6.85),(-1.4,0,6.85)]
    for a,b in zip(points,points[1:]):rod('Swept outreach',a,b,.055,metal,fixture,12)
    cube('Luminaire cast housing',(-1.45,0,6.78),(.79,.35,.17),metal,fixture,.07)
    for i in range(9):cube('Cooling fin',(-1.79+i*.083,0,6.9),(.033,.27,.055),silver,fixture,.006)
    cube('Recessed optic bezel',(-1.45,0,6.68),(.68,.29,.06),silver,fixture,.04)
    for x in range(6):
        for y in [-1,0,1]:cube('LED lens module',(-1.72+x*.108,y*.073,6.643),(.080,.053,.012),lens,fixture,.012)
    # Join the static parts into one low-cost mesh while preserving three material slots.
    bpy.ops.object.select_all(action='DESELECT')
    for o in fixture.objects:o.select_set(True)
    bpy.context.view_layer.objects.active=next(o for o in fixture.objects if o.type=='MESH');bpy.ops.object.join();model=bpy.context.object;model.name='PV_LEDStreetLamp'
    bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    lods=[]
    for level,ratio in [(1,.30),(2,.075)]:
        lod=model.copy();lod.data=model.data.copy();fixture.objects.link(lod);lod.name='PV_LEDStreetLamp_LOD'+str(level);lod.parent=model;lod.location=(0,0,0)
        bpy.context.view_layer.objects.active=lod;dec=lod.modifiers.new('Mobile distance detail','DECIMATE');dec.ratio=ratio;bpy.ops.object.modifier_apply(modifier=dec.name);lods.append(lod)
    for o in scene.objects:o.select_set(False)
    model.select_set(True)
    for lod in lods:lod.select_set(True)
    bpy.ops.export_scene.fbx(filepath=OUT+'/PV_LEDStreetLamp.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,mesh_smooth_type='FACE',path_mode='RELATIVE',embed_textures=False)
    for lod in lods:lod.hide_render=True;lod.hide_set(True)
    # Emission masks address the existing semantic UV slots; no vehicle mesh or rig is rebuilt.
    for name, slots in [('PV_BikeEmission',{20:'EFF8FF',21:'FF1B22'}),('PV_CarEmission',{14:'EFF8FF',15:'FF1B22'})]:
        data=[]
        for y in range(128):
            for x in range(128):data.extend(linear(slots.get((y//32)*8+x//16,'000000')))
        image=bpy.data.images.new(name,128,128,alpha=True);image.pixels=data;image.filepath_raw=OUT+'/'+name+'.png';image.file_format='PNG';image.save()
    # Street pigments stay in the original PV_StreetPalette; daylight only changes illumination.
    profile={'halfCycleSeconds':30,'daySun':'FFFFFF','nightMoon':'A9C6FF','daySky':'C3DFF9','dayHorizon':'F0F7FF','nightSky':'071329','nightHorizon':'172846',
     'dayAmbientSky':'E4F1FF','dayAmbientEquator':'D9E4EF','dayAmbientGround':'ADB7C2','nightAmbientSky':'263D67','nightAmbientEquator':'182841','nightAmbientGround':'0E172B',
     'sunIntensity':1.12,'moonIntensity':.24,'lampColor':'F1F7FF','lightPoolMax':6,'authoredIn':'Blender MCP 5.2','headlightCookieBounds':[-16,-4,16,28],'streetCookieWidth':18}
    with open(OUT+'/PV_LightingProfile.json','w',encoding='utf-8') as f:json.dump(profile,f,indent=2)
    # A compact, physically lit street study for review of the repeating 60-second cycle.
    demo=bpy.data.collections.new('PV morning to midnight study');scene.collection.children.link(demo)
    road=material('PV Study Asphalt','626F7D',rough=.82);white=material('PV Morning Plaster','F4F7FB',rough=.77);red=material('PV Red Accent','D83833',rough=.45)
    cube('Study roadway',(0,-6,-.1),(16,34,.2),road,demo,0)
    for side in [-1,1]:
        cube('Study pavement',(side*9,-6,.1),(3,34,.2),white,demo,.04)
        for i in range(4):cube('Study shop',(side*12,-15+i*8,4.7),(4,7,9.4),white,demo,.06)
        for i in range(4):cube('Study red shop sign',(side*9.94,-15+i*8,2.9),(.1,5.5,.75),red,demo,.02)
    for i in range(8):cube('Study lane paint',(0,-19+i*4,.012),(.13,2,.024),white,demo,0)
    model.location=(-7.4,-4,0);model.rotation_euler.z=math.pi
    emitter=light('Study street LED','SPOT',(-5.95,-4,6.64),0,demo,(-5.95,-4,0),profile['lampColor']);emitter.data.spot_size=math.radians(99);emitter.data.spot_blend=.7;emitter.data.shadow_soft_size=.22
    sun=light('Morning sun','SUN',(0,0,12),3,demo,(4,5,0));sun.data.angle=math.radians(4)
    moon=light('Blue hour fill','SUN',(0,0,12),0,demo,(-8,-4,0),'A9C6FF');moon.data.angle=math.radians(8)
    scene.render.fps=30;scene.frame_start=1;scene.frame_end=1801
    for frame,night in [(1,0),(451,.5),(901,1),(1351,.5),(1801,0)]:
        sun.data.energy=3*(1-night)**2;sun.data.keyframe_insert('energy',frame=frame)
        moon.data.energy=.35*night;moon.data.keyframe_insert('energy',frame=frame)
        emitter.data.energy=900*night;emitter.data.keyframe_insert('energy',frame=frame)
        bg.inputs['Color'].default_value=linear(profile['nightSky'] if night==1 else 'BDDCF5' if night==0 else '657799');bg.inputs['Color'].keyframe_insert('default_value',frame=frame)
        bg.inputs['Strength'].default_value=.5*(1-night)+.04;bg.inputs['Strength'].keyframe_insert('default_value',frame=frame)
        p=next(n for n in lens.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Emission Strength'].default_value=8*night;p.inputs['Emission Strength'].keyframe_insert('default_value',frame=frame)
    cam.location=(18,-27,12);lookat(cam,(0,-1,3));cam.data.type='PERSP';cam.data.lens=28;scene.camera=cam
    scene.render.resolution_x=960;scene.render.resolution_y=640
    for frame,label in [(1,'day'),(901,'night')]:
        scene.frame_set(frame);scene.render.filepath=ROOT+'/Captures/day-night-blender-'+label+'.png';bpy.ops.render.render(write_still=True)
    scene.frame_set(1)
    # Library write saves only this study and dependencies; the other open scenes remain untouched.
    bpy.data.libraries.write(ROOT+'/ArtSource/Blender/PhoViet_DayNight.blend',{scene},fake_user=True)
    bpy.context.window.scene=scene
    print(json.dumps({'success':True,'fixtureTriangles':sum(len(p.vertices)-2 for p in model.data.polygons),'textures':5,'halfCycleSeconds':30,'source':'PhoViet_DayNight.blend','previousScene':original.name}))

build()
