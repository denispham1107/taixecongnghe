"""Design studies only. Run in live Blender via MCP; never imports/exports to Unity.
Blender coordinates X lateral / Y along 50.4m span / Z up, road surface Z=0.
"""
import bpy, bmesh, math, os, json
from mathutils import Vector
from math import sin, cos, pi
OUT = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/Bridge-20261028'
PREFIX = 'PV85_'
if any(s.name.startswith(PREFIX) for s in bpy.data.scenes):
    raise RuntimeError('Concept scenes already exist; inspect before changing them')
ORIGINAL = bpy.context.scene
ENGINE = ORIGINAL.render.engine
VIEW = ORIGINAL.view_settings.view_transform
FMT = next(i.identifier for i in bpy.types.ImageFormatSettings.bl_rna.properties['file_format'].enum_items if i.identifier=='PNG')
ORTHO = next(i.identifier for i in bpy.types.Camera.bl_rna.properties['type'].enum_items if i.identifier=='ORTHO')
BEVEL = next(i.identifier for i in bpy.types.Modifier.bl_rna.properties['type'].enum_items if i.identifier=='BEVEL')
font = bpy.data.fonts.load('C:/Windows/Fonts/arialbd.ttf')
M={}
def mat(name,color,metal=0,rough=.5,emission=0):
    m=bpy.data.materials.new(PREFIX+name);m.use_nodes=True
    n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    n.inputs['Base Color'].default_value=(*color,1)
    n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=rough
    if emission:
        n.inputs['Emission Color'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=emission
    m.diffuse_color=(*color,1);M[name]=m;return m
mat('stone',(.72,.65,.49),0,.62);mat('ivory',(.86,.81,.68),0,.48)
mat('edge',(.48,.43,.32),0,.62);mat('concrete',(.49,.55,.57),0,.65)
mat('pave',(.54,.62,.64),0,.67);mat('pave_light',(.7,.75,.74),0,.65)
mat('asphalt',(.075,.1,.115),0,.88);mat('mark',(.92,.94,.87),0,.6)
mat('yellow',(.98,.69,.09),0,.5);mat('bronze',(.23,.15,.075),.72,.32)
mat('blue',(.025,.22,.34),.65,.3);mat('blue_edge',(.09,.42,.51),.65,.3)
mat('navy',(.028,.065,.105),.5,.35);mat('silver',(.62,.72,.74),.78,.3)
mat('warm',(.99,.69,.3),.1,.25,4);mat('cool',(.49,.83,.97),.1,.24,2)
mat('soil',(.09,.067,.045),0,.9);mat('leaf',(.08,.29,.17),0,.8)
mat('flower',(.85,.13,.36),0,.5);mat('red',(.72,.028,.04),0,.6)
mat('water',(.055,.23,.29),.42,.25)
# Fine mineral/road grain is shading only, no extra scene geometry.
for key,scale,strength in [('asphalt',58,.13),('stone',18,.09),('concrete',22,.08),('water',.65,.15)]:
    m=M[key];nt=m.node_tree;bs=next(n for n in nt.nodes if n.type=='BSDF_PRINCIPLED')
    tex=nt.nodes.new('ShaderNodeTexNoise');tex.inputs['Scale'].default_value=scale
    tex.inputs['Detail'].default_value=2;bum=nt.nodes.new('ShaderNodeBump');bum.inputs['Strength'].default_value=strength;bum.inputs['Distance'].default_value=.08
    nt.links.new(tex.outputs['Fac'],bum.inputs['Height']);nt.links.new(bum.outputs['Normal'],bs.inputs['Normal'])
C=None; ROOT=None
def link(obj,name,material,part=True):
    obj.name=PREFIX+name;C.objects.link(obj)
    if material:obj.data.materials.append(M[material])
    if part:obj.parent=ROOT
    return obj

def mesh(name,verts,faces,material,bevel=0,part=True):
    me=bpy.data.meshes.new(PREFIX+name);me.from_pydata(verts,[],faces);me.update()
    bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
    ob=link(bpy.data.objects.new(PREFIX+name,me),name,material,part)
    if bevel:
        mod=ob.modifiers.new('Rounded fabricated edges',BEVEL);mod.width=bevel;mod.segments=2
    return ob

def box(name,loc,dims,material,bevel=.02,part=True):
    x,y,z=(d/2 for d in dims)
    vs=[(a*x,b*y,c*z) for a,b,c in [(-1,-1,-1),(-1,-1,1),(-1,1,-1),(-1,1,1),(1,-1,-1),(1,-1,1),(1,1,-1),(1,1,1)]]
    ob=mesh(name,vs,[(0,4,6,2),(1,3,7,5),(0,1,5,4),(2,6,7,3),(0,2,3,1),(4,5,7,6)],material,bevel,part);ob.location=loc;return ob

def rod(name,a,b,r,material,n=10):
    a,b=Vector(a),Vector(b);v=b-a
    q=Vector((0,0,1)).rotation_difference(v.normalized())
    pts=[tuple(a+q@Vector((r*cos(2*pi*i/n),r*sin(2*pi*i/n),z))) for z in (0,v.length) for i in range(n)]
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    ob=mesh(name,pts,faces,material,.008)
    for p in ob.data.polygons[2:]:p.use_smooth=True
    return ob

def arch(name,x,y0,y1,z0,rise,width,depth,material,n=48):
    vs=[]
    for i in range(n+1):
        t=i/n;y=y0+(y1-y0)*t;z=z0+rise*sin(pi*t)
        vs.extend([(x-width/2,y,z-depth/2),(x+width/2,y,z-depth/2),(x-width/2,y,z+depth/2),(x+width/2,y,z+depth/2)])
    faces=[(0,2,3,1),(4*n,4*n+1,4*n+3,4*n+2)]
    for i in range(n):
        a=4*i;b=a+4;faces.extend([(a,b,b+1,a+1),(a+2,a+3,b+3,b+2),(a,a+2,b+2,b),(a+1,b+1,b+3,a+3)])
    return mesh(name,vs,faces,material,.025)

def leaf_spray(x,y,z):
    for j in range(5):
        a=2*pi*j/5;rod('stem',(x,y,z),(x+.17*cos(a),y+.17*sin(a),z+.23),.025,'leaf',6)
        # Small folded petals, distinctly botanical without a large prop budget.
        for k in range(4):
            b=a+k*pi/2;cx=x+.16*cos(a);cy=y+.16*sin(a);cz=z+.24
            mesh('petal',[(cx,cy,cz),(cx+.095*cos(b),cy+.095*sin(b),cz+.06),(cx+.075*cos(b+.8),cy+.075*sin(b+.8),cz+.09)],[(0,1,2)],'flower')

def planter(x,y):
    box('planter', (x,y,1.18),(.48,.64,.28),'ivory',.045);box('soil',(x,y,1.33),(.37,.52,.015),'soil',.006)
    leaf_spray(x,y,1.33)

def star_flag(x,y):
    # Keep national accents restrained and outside the driving corridor.
    rod('flag_mast',(x,y,1.15),(x,y,3.35),.026,'silver',8)
    verts=[(x,y,3.28),(x+.78,y+.08,3.25),(x+.78,y+.08,2.78),(x,y,2.81)]
    mesh('Vietnam_flag',verts,[(0,1,2,3)],'red')
    vs=[(x+.39,y-.01,3.04)]+[(x+.39+(.15 if k%2==0 else .062)*cos(pi/2+k*pi/5),y-.015,3.04+(.15 if k%2==0 else .062)*sin(pi/2+k*pi/5)) for k in range(10)]
    mesh('gold_star',vs,[(0,k+1,(k+1)%10+1) for k in range(10)],'yellow')

def lamp(x,y,classic=False):
    box('lamp_foot',(x,y,.22),(.32,.4,.44),'bronze' if classic else 'navy',.045)
    rod('lamp_pole',(x,y,.4),(x,y,4.2),.055,'bronze' if classic else 'navy')
    side=-1 if x>0 else 1
    if classic:
        rod('lamp_arm',(x,y,3.8),(x+side*.42,y,4.05),.035,'bronze')
        box('lantern',(x+side*.42,y,3.93),(.28,.25,.36),'warm',.04)
        box('lantern_cap',(x+side*.42,y,4.15),(.38,.34,.07),'bronze')
    else:
        rod('LED_arm',(x,y,4.2),(x+side*.64,y,4.22),.045,'navy')
        box('LED_housing',(x+side*.55,y,4.18),(.48,.3,.09),'silver')
        box('LED_lens',(x+side*.55,y,4.12),(.4,.23,.015),'warm',.006)
    data=bpy.data.lights.new(PREFIX+'luminaire','POINT');data.energy=30;data.color=(1,.75,.4);data.shadow_soft_size=.65
    ob=link(bpy.data.objects.new(PREFIX+'luminaire',data),'luminaire',None);ob.location=(x,y,3.8)

def common_deck(style):
    box('structural_deck',(0,0,-.4),(7.4,50.4,.8),'concrete',.09)
    box('road',(0,0,-.024),(6.2,50.4,.048),'asphalt',.008)
    for side in (-1,1):
        box('edge_girder',(side*3.35,0,-.67),(.48,50.4,.62),'ivory' if style=='A' else 'navy',.07)
        box('curb',(side*3.22,0,.10),(.24,50.4,.20),'ivory',.025)
        box('maintenance_walk',(side*3.52,0,.06),(.36,50.4,.12),'pave_light',.015)
        box('edge_line',(side*2.98,0,.009),(.08,50.1,.012),'mark',.003)
        for j in range(28):
            y=-24.9+j*1.84
            box('walk_joint',(side*3.52,y,.124),(.35,.018,.008),'pave',.001)
    for y in range(-24,25,4):box('center_dash',(0,y,.011),(.10,1.8,.016),'yellow',.003)
    for y in (-24.95,-8.4,8.4,24.95):box('expansion_joint',(0,y,.004),(6.18,.07,.016),'silver',.005)
    for y in range(-23,25,4):box('cross_diaphragm',(0,y,-.85),(6.6,.26,.42),'concrete',.04)
    for side in (-1,1):
        box('bridge_led_recess',(side*3.57,0,-.20),(.04,49.8,.055),'warm',.008)
    # Bridge end foundations; all study geometry is isolated from the game.
    for y in (-25.4,25.4):
        box('abutment',(0,y,-3.8),(8.1,1.4,7.2),'stone',.16)
        box('bearing_cap',(0,y,-.95),(7.5,1.6,.5),'concrete',.06)
        for x in (-2.5,0,2.5):box('bearing_pad',(x,y,-.66),(.58,.5,.14),'navy',.025)

def metal_rails(color):
    for side in (-1,1):
        x=side*3.6
        box('top_handrail',(x,0,1.2),(.105,50.4,.1),color,.035)
        box('bottom_rail',(x,0,.36),(.07,50.4,.08),color)
        for j in range(43):
            y=-25.05+j*1.193
            box('rail_post',(x,y,.76),(.10,.10,.92),color,.018)
            box('post_shoe',(x,y,.25),(.22,.22,.15),'silver',.025)
        for j in range(84):
            y=-24.85+j*.596
            box('infill_bar',(x,y,.75),(.045,.045,.73),color,.01)

STUDIES=[]
def create(code,title):
    global C,ROOT
    scene=bpy.data.scenes.new(PREFIX+code+'_'+title)
    scene.render.engine=ENGINE;scene.view_settings.view_transform=VIEW
    scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
    scene.render.image_settings.file_format=FMT
    scene.world=bpy.data.worlds.new(PREFIX+code+'_sky');scene.world.use_nodes=True
    bg=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs['Color'].default_value=(.55,.7,.86,1);bg.inputs['Strength'].default_value=.65
    C=bpy.data.collections.new(PREFIX+code+'_BRIDGE_ONLY');scene.collection.children.link(C)
    ROOT=bpy.data.objects.new(PREFIX+code+'_Bridge',None);C.objects.link(ROOT)
    ROOT['study_only']=True;ROOT['span_m']=50.4;ROOT['driving_width_m']=6.2;ROOT['approval_required_before_Unity']=True
    STUDIES.append({'code':code,'title':title,'scene':scene.name,'collection':C.name,'root':ROOT.name})
    return scene

def stage(scene):
    global C,ROOT
    C=bpy.data.collections.new(scene.name+'_PRESENTATION_NOT_FOR_GAME');scene.collection.children.link(C);ROOT=None
    box('water_backdrop',(0,0,-5.08),(150,90,.12),'water',0,False)
    for y in (-31.8,31.8):
        box('neutral_bank_context',(0,y,-2.7),(150,12,5.4),'concrete',.12,False)
        box('approach_context',(0,y,-.04),(6.2,12,.08),'asphalt',.01,False)
        for x in (-3.35,3.35):box('approach_paving',(x,y,.05),(.5,12,.1),'pave_light',.01,False)
    sun=bpy.data.lights.new(PREFIX+'sun','SUN');sun.energy=2.5;sun.angle=.12
    ob=link(bpy.data.objects.new(PREFIX+'sun',sun),'sun',None,False);ob.rotation_euler=(math.radians(25),math.radians(-25),math.radians(-30))
    fill=bpy.data.lights.new(PREFIX+'softbox','AREA');fill.energy=1800;fill.shape='DISK';fill.size=40
    ob=link(bpy.data.objects.new(PREFIX+'softbox',fill),'softbox',None,False);ob.location=(18,-12,30)
    ob.rotation_euler=(Vector((0,0,0))-ob.location).to_track_quat('-Z','Y').to_euler()
    for name,pos,target,scale in [('overview',(42,-60,31),(0,0,1.5),66),('street',(4.5,-37,5.3),(0,8,2.8),52),('detail',(15,-23,9),(0,-7,0),28)]:
        data=bpy.data.cameras.new(PREFIX+name);cam=link(bpy.data.objects.new(PREFIX+name,data),name,None,False)
        cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
        if name!='street':data.type=ORTHO;data.ortho_scale=scale
        else:data.lens=29
        data.clip_end=300;cam['view_role']=name
        if name=='overview':scene.camera=cam
    scene['reference_only']='Bridge design study, not installed in Unity'
    # Neutral reference ruler on the road: no vehicle asset is invented/replaced.
    return scene

# A — cream concrete, genuine three-span arches, bronze detailing.
s=create('A','Song_Viet_Classic');common_deck('A')
for y in (-8.4,8.4):
    box('pier_rounded',(0,y,-4.1),(6.6,1.35,6.5),'stone',.32)
    box('pier_coping',(0,y,-1.04),(7.05,1.7,.36),'ivory',.09)
for i in range(3):
    for x in (-2.65,2.65):arch('concrete_arch',x,-25.2+i*16.8,-8.4+i*16.8,-4.0,3.15,.55,.48,'ivory',32)
for side in (-1,1):
    x=side*3.59
    box('stone_footrail',(x,0,.34),(.21,50.4,.22),'ivory',.035)
    box('stone_coping',(x,0,1.24),(.26,50.4,.15),'ivory',.045)
    box('bronze_inlay',(side*3.725,0,1.20),(.018,50,.028),'bronze',.004)
    for j in range(22):
        y=-25.1+j*(50.2/21)
        box('masonry_post',(x,y,.78),(.31,.34,.95),'ivory',.04)
        box('post_cap',(x,y,1.33),(.40,.43,.12),'stone',.035)
        if j in (3,10,18):planter(x,y)
    for j in range(21):
        y=-23.9+j*(50.2/21)
        for k in (-.7,-.35,0,.35,.7):box('bronze_baluster',(x,y+k,.79),(.055,.055,.74),'bronze',.012)
        # Rhombus/petal medallion makes this a designed balustrade, not a stock fence.
        pts=[(x,y-.23,.75),(x,y,1.02),(x,y+.23,.75),(x,y,.48)]
        for k in range(4):rod('lotus_medallion',pts[k],pts[(k+1)%4],.025,'bronze',6)
    for y in (-20,-6.7,6.7,20):lamp(x,y,True)
    star_flag(x,-13 if side<0 else 13)
stage(s)

# B — tied steel bows; no cross-member crosses the driving/jumping corridor.
s=create('B','Blue_Steel_Bow');common_deck('B');metal_rails('blue')
for side in (-1,1):
    x=side*3.58
    arch('main_steel_bow',x,-25.0,25.0,.64,7.7,.40,.66,'blue')
    arch('bow_edge_trim',x+side*.215,-25.0,25.0,.78,7.7,.035,.10,'blue_edge')
    for j in range(1,12):
        y=-25+j*50/12;z=.64+7.7*sin(pi*j/12)
        rod('tension_hanger',(x,y,.66),(x,y,z-.28),.045,'silver')
        box('hanger_anchor',(x,y,.60),(.48,.40,.25),'blue',.04)
        for dx in (-.15,.15):rod('anchor_pin',(x+dx,y-.13,.75),(x+dx,y+.13,.75),.055,'silver')
    for y in (-24.3,24.3):
        box('bow_footplate',(x,y,.25),(.65,1.2,.18),'navy',.035)
        for dx in (-.22,.22):
            for dy in (-.4,.4):rod('foot_bolt',(x+dx,y+dy,.34),(x+dx,y+dy,.40),.045,'silver',6)
    for y in (-18,0,18):lamp(x,y)
    for y in (-11,11):planter(x,y)
    star_flag(x,-17 if side<0 else 17)
stage(s)

# C — paired outboard needles with a fan of stay cables, clear road centre.
s=create('C','River_Sail_Stayed');common_deck('C');metal_rails('silver')
for side in (-1,1):
    x=side*3.58;y=-5.2
    box('tower_foundation',(side*2.7,y,-4.1),(2.3,2.4,7),'concrete',.25)
    box('tower_shoe',(x,y,.32),(.72,1.38,.64),'navy',.08)
    # Tapering pylon with a restrained outward lean.
    vs=[]
    for z,w,d,lean in [(0,.62,1.1,0),(12.8,.24,.45,.48)]:
        vs.extend([(x+side*lean+a*w/2,y+b*d/2,z) for a,b in [(-1,-1),(1,-1),(1,1),(-1,1)]])
    mesh('tapered_pylon',vs,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],'ivory',.045)
    rod('pylon_blue_insert',(x+side*.33,y-.57,.8),(x+side*.59,y-.25,12.15),.048,'blue_edge')
    for k,anchor in enumerate([-24,-20,-16,-12,0,5,10,15,20,24]):
        top=12.25-(k%5)*.30
        rod('stay_cable',(x,anchor,.4),(x+side*.45,y,top),.035,'silver')
        box('stay_anchor',(x,anchor,.34),(.40,.46,.28),'blue',.035)
    for yy in (-21,8,22):lamp(x,yy)
    for yy in (-15,16):planter(x,yy)
    star_flag(x,12 if side<0 else -18)
stage(s)

exec(compile(open(os.path.join(OUT,"refine_bridge_concepts.py"),encoding="utf-8").read(),"refine_bridge_concepts.py","exec"))

# Set the initial presentation to A and save a new study file; original data is retained.
bpy.context.window.scene=bpy.data.scenes[STUDIES[0]['scene']]
for d in STUDIES:
    root=bpy.data.objects[d['root']]
    objs=list(root.children_recursive)
    d['mesh_objects']=sum(o.type=='MESH' for o in objs)
    d['base_triangles']=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objs if o.type=='MESH')
    d['day_render']=os.path.join(OUT,d['code']+'-day.png')
    d['street_render']=os.path.join(OUT,d['code']+'-street.png')
    d['span_m']=50.4;d['road_width_m']=6.2;d['deck_width_m']=7.4
with open(os.path.join(OUT,'concepts.json'),'w',encoding='utf-8') as f:json.dump({'reference_only':True,'approval_required':True,'studies':STUDIES},f,indent=2)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'PhoViet_Bridge_Concepts.blend'),copy=True)
print(json.dumps({'success':True,'original_scene_preserved':ORIGINAL.name,'original_filepath':bpy.data.filepath,'studies':STUDIES},ensure_ascii=False))
