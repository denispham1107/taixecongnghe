import bpy, math, json
from pathlib import Path
from mathutils import Vector

OUT = Path(r'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/MissilePickup-20261105')
OUT.mkdir(parents=True, exist_ok=True)
previous = bpy.context.window.scene
old = bpy.data.scenes.get('PV_PickupMissile_Concepts_2130')
if old:
    if previous == old:
        previous = bpy.data.scenes.get('PV_Missile2126') or next(s for s in bpy.data.scenes if s != old)
        bpy.context.window.scene = previous
    for o in list(old.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    bpy.data.scenes.remove(old)
scene = bpy.data.scenes.new('PV_PickupMissile_Concepts_2130')
bpy.context.window.scene = scene
scene.render.engine = previous.render.engine
scene.render.resolution_x = 1800
scene.render.resolution_y = 1100
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.world = bpy.data.worlds.new('Pickup concept studio world')
scene.world.use_nodes = True
scene.world.node_tree.nodes.get('Background').inputs[0].default_value = (.035,.065,.13,1)
scene.world.node_tree.nodes.get('Background').inputs[1].default_value = .4
try: scene.view_settings.view_transform = 'AgX'
except TypeError: pass

def mat(name, rgb, metal=0, rough=.3, glow=0):
    m=bpy.data.materials.new('Concept2130 '+name);m.diffuse_color=(*rgb,1);m.use_nodes=True
    p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value=(*rgb,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
    p.inputs['Emission Color'].default_value=(*rgb,1);p.inputs['Emission Strength'].default_value=glow
    return m
navy=mat('deep cobalt',(.018,.055,.13),.55)
blue=mat('blue enamel',(.012,.19,.8),.3)
cyan=mat('cyan edge',(.015,.68,1),.25,.24,1.8)
white=mat('porcelain white',(.88,.94,1),.25,.26)
red=mat('coral red',(.95,.045,.028),.25,.26)
orange=mat('warm amber', (1,.37,.035),.45,.28,.8)
dark=mat('studio floor',(.012,.023,.045),.25,.45)
muted=mat('muted text',(.38,.57,.72),0,.5,.2)
asphalt=mat('road',(.035,.045,.06),0,.95)
yellow=mat('lane yellow',(.9,.65,.12),0,.6)
roots=[];studio=[]
def finish(o,name,m,parent=None,bevel=0):
    o.name='2130 '+name
    if m:o.data.materials.append(m)
    if bevel:
        b=o.modifiers.new('Soft machined edges','BEVEL');b.width=bevel;b.segments=3
        o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
    if parent:o.parent=parent
    if o.type=='MESH':
        for f in o.data.polygons:f.use_smooth=True
    return o
def cyl(name,pos,r,depth,m,parent=None,vertices=48,rotate=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=depth,location=pos)
    o=finish(bpy.context.object,name,m,parent,.035)
    if rotate:o.rotation_euler=rotate
    return o
def sphere(name,pos,scale,m,parent):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=16,location=pos)
    o=finish(bpy.context.object,name,m,parent);o.scale=scale;return o
def torus(name,pos,major,minor,m,parent=None,vertical=False):
    bpy.ops.mesh.primitive_torus_add(major_segments=64,minor_segments=12,location=pos,major_radius=major,minor_radius=minor)
    o=finish(bpy.context.object,name,m,parent)
    if vertical:o.rotation_euler.x=math.pi/2
    return o
def box(name,pos,scale,m,parent=None,bevel=.04):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish(o,name,m,parent,bevel)
def fin(name,angle,parent,m):
    verts=[(.18,-.10,-.50),(.53,-.10,-.97),(.52,-.10,-1.18),(.15,-.10,-1.02),(.18,.10,-.50),(.53,.10,-.97),(.52,.10,-1.18),(.15,.10,-1.02)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)])
    o=bpy.data.objects.new(name,mesh);scene.collection.objects.link(o);o.rotation_euler.z=angle;return finish(o,name,m,parent,.04)
def rocket(name,parent,scale=1,angle=0):
    r=bpy.data.objects.new(name,None);scene.collection.objects.link(r);r.parent=parent
    cyl(name+' white body',(0,0,-.12),.26,1.47,white,r)
    sphere(name+' rounded red nose',(0,0,.69),(.265,.265,.47),red,r)
    cyl(name+' red collar',(0,0,.34),.28,.12,red,r)
    cyl(name+' cobalt engine',(0,0,-.92),.22,.30,navy,r)
    torus(name+' amber nozzle',(0,0,-1.08),.17,.055,orange,r)
    for i in range(4):fin(name+' swept fin '+str(i),i*math.pi/2,r,blue)
    box(name+' blue center stripe',(0,-.262,-.05),(.08,.025,.65),blue,r,.015)
    r.scale=(scale,scale,scale);r.rotation_euler.y=angle
    return r
def root(name,x):
    o=bpy.data.objects.new(name,None);scene.collection.objects.link(o);o.location=(x,0,3.1);roots.append(o);return o

A=root('A • Rocket Pop',-5.2)
rocket('A rocket',A,1.18,-.23)
torus('A floating halo',(0,0,-1.55),.85,.075,cyan,A)
torus('A halo inner',(0,0,-1.55),.62,.028,blue,A)

B=root('B • Orbit Coin',0)
cyl('B solid coin',(0,.10,0),1.55,.24,navy,B,rotate=(math.pi/2,0,0))
torus('B bright rim',(0,-.05,0),1.48,.085,cyan,B,True)
torus('B inner bevel',(0,-.09,0),1.29,.028,blue,B,True)
r=rocket('B raised rocket',B,1.0,-.32);r.location.y=-.28
for z in [-.7,0,.7]:box('B side notch',(1.31,-.1,z),(.10,.12,.18),white,B,.025)

C=root('C • Hex Ammo',5.2)
cyl('C hex shell',(0,.13,0),1.65,.30,navy,C,6,(math.pi/2,0,0))
cyl('C amber inset',(0,-.04,0),1.48,.12,orange,C,6,(math.pi/2,0,0))
cyl('C blue enamel face',(0,-.12,0),1.35,.13,blue,C,6,(math.pi/2,0,0))
r=rocket('C chunky rocket',C,1.02,.28);r.location.y=-.4
for side in [-1,1]:
    box('C light marker',(side*1.22,-.23,0),(.09,.1,.36),cyan,C,.03)

fontpath=Path('C:/Windows/Fonts/arialbd.ttf')
font=bpy.data.fonts.load(str(fontpath)) if fontpath.exists() else None
def text(body,pos,size,m):
    cu=bpy.data.curves.new(body,'FONT');cu.body=body;cu.align_x='CENTER';cu.size=size;cu.extrude=.001
    if font:cu.font=font
    o=bpy.data.objects.new(body,cu);scene.collection.objects.link(o);o.location=pos;o.rotation_euler=(math.pi/2,0,0);cu.materials.append(m);studio.append(o);return o
for x,label,sub in [(-5.2,'A  /  ROCKET POP','Tên lửa khối nổi'),(0,'B  /  ORBIT COIN','Huy hiệu vòng sáng'),(5.2,'C  /  HEX AMMO','Huy hiệu lục giác')]:
    studio.append(cyl(label+' pedestal',(x,0,.2),1.95,.26,navy))
    studio.append(torus(label+' pedestal rim',(x,0,.35),1.85,.025,cyan))
    text(label,(x,-1.3,.58),.36,white);text(sub,(x,-1.34,.10),.25,muted)
text('PHỐ VIỆT  /  VẬT PHẨM TÊN LỬA',(0,.2,6.7),.45,white)
text('BA PHƯƠNG ÁN 3D • CHỈ LÀ MẪU XEM TRƯỚC',(0,.2,6.12),.23,muted)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.10));floor=finish(bpy.context.object,'studio floor',dark)
def aim(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
def light(name,pos,power,size,color):
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color
    o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=pos;aim(o,(0,0,2));return o
light('Concept key',(-5,-8,11),2500,8,(.8,.9,1))
light('Concept fill',(8,-4,7),2000,7,(1,.82,.65))
light('Concept rim',(0,5,9),3000,6,(.2,.65,1))
d=bpy.data.cameras.new('Concept camera');cam=bpy.data.objects.new('Concept camera',d);scene.collection.objects.link(cam);scene.camera=cam
cam.location=(0,-25,11);aim(cam,(0,0,3));d.type='ORTHO';d.ortho_scale=17.4
scene.render.filepath=str(OUT/'missile-pickup-ABC.png')
bpy.data.libraries.write(str(OUT/'MissilePickup_ABC.blend'), {scene}, fake_user=True)
print(json.dumps({'scene':scene.name,'roots':[r.name for r in roots],'output':scene.render.filepath,'previousScene':previous.name}))

def render_board():
    bpy.context.window.scene=scene
    bpy.ops.render.render(write_still=True)
    bpy.context.window.scene=previous

def render_road():
    bpy.context.window.scene=scene
    for o in studio:o.hide_render=True
    floor.data.materials.clear();floor.data.materials.append(asphalt)
    for r in roots:r.location.z=1.75;r.scale=(.60,.60,.60)
    for x in [-7.8,-2.6,2.6,7.8]:
        for y in [-7,-3,1,5,9]:box('Road preview marking',(x,y,-.07),(.075,1.8,.02),white,bevel=0)
    for x,label in [(-5.2,'A'),(0,'B'),(5.2,'C')]:text(label,(x,-2,.03),.65,white)
    scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.45
    for o in scene.objects:
        if o.type=='LIGHT':o.data.energy*=.55
    cam.location=(0,-15,13);aim(cam,(0,1,0));d.ortho_scale=17.4
    scene.render.resolution_y=850;scene.render.filepath=str(OUT/'missile-pickup-road-preview.png')
    bpy.data.libraries.write(str(OUT/'MissilePickup_RoadPreview.blend'), {scene}, fake_user=True)
    bpy.ops.render.render(write_still=True)
    bpy.context.window.scene=previous

bpy.app.driver_namespace['PV2130'] = globals().copy()
bpy.context.window.scene=previous
