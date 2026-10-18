"""Original four-wheel traffic assets, authored through the live Blender MCP.
Run build_all() in Blender; shared mesh/atlas/FBX utilities are from our bike source.
Metres, author coordinates X right, Y up, Z forward. No gameplay collision changes.
"""
import os
ROOT=r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
exec(compile(open(ROOT+'/ArtSource/Blender/pho_viet_bikes.py',encoding='utf-8').read(),'pho_viet_bikes.py','exec'))
OUT=ROOT+'/Assets/PhoViet/Art/Cars';os.makedirs(OUT,exist_ok=True)
CONFIGS=[('PV_Sedan','Sedan','B74839'),('PV_Hatchback','Hatchback','E9B63F'),
 ('PV_Taxi','Taxi','168D88'),('PV_DeliveryVan','Van','5497B1'),
 ('PV_BoxTruck','Truck','268F89'),('PV_DrowsyTruck','Drowsy','DA8B35')]
CLIPS=['Idle','Drive','Brake','ImpactFront','ImpactRear','ImpactLeft','ImpactRight']
MODELS={k:v for k,v in MODELS.items() if k in [c[0] for c in CONFIGS]}

_BaseGeometry=Geometry
class Geometry(_BaseGeometry):
    def text(self,value,position,size,col,bone='Frame',front=False):
        curve=bpy.data.curves.new('Label','FONT');curve.body=value;curve.size=size;curve.align_x='CENTER';curve.align_y='CENTER';curve.resolution_u=2
        curve.font=bpy.data.fonts.load(ROOT+'/Assets/PhoViet/Fonts/BeVietnamPro-SemiBold.ttf',check_existing=True)
        obj=bpy.data.objects.new('TemporaryLabel',curve);bpy.context.collection.objects.link(obj);obj.location=B(position);obj.rotation_euler=(pi/2,0,0 if front else pi)
        bpy.context.view_layer.update();evaluated=obj.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=evaluated.to_mesh()
        pts=[obj.matrix_world@v.co for v in mesh.vertices];self.add([(-p.x,p.z,-p.y) for p in pts],[tuple(p.vertices) for p in mesh.polygons],col,bone,False)
        evaluated.to_mesh_clear();bpy.data.objects.remove(obj,do_unlink=True);bpy.data.curves.remove(curve)

def skeleton(name,truck=False):
    data=bpy.data.armatures.new(name+'_Skeleton');rig=bpy.data.objects.new(name+'_Rig',data);bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
    rad=.51 if truck else .38;x=1.02 if truck else .82;zf=2.12 if truck else 1.36;zr=-2.05 if truck else -1.36
    defs=[('Frame',None,(0,0,0)),('Body','Frame',(0,.75,0)),('Cargo','Body',(0,1.2,-.9)),
      ('BumperFront','Body',(0,.7,3.12 if truck else 2.02)),('BumperRear','Body',(0,.7,-3.1 if truck else -2.02)),
      ('MirrorL','Body',(-1.02,1.9 if truck else 1.27,2.7 if truck else .65)),('MirrorR','Body',(1.02,1.9 if truck else 1.27,2.7 if truck else .65))]
    for side,s in [('L',-1),('R',1)]:
        for end,z in [('F',zf),('R',zr)]:
            steer='Steer'+end+side;wheel='Wheel'+end+side
            defs.extend([(steer,'Frame',(s*x,rad,z)),(wheel,steer,(s*x,rad,z))])
    for n,parent,p in defs:
        b=data.edit_bones.new(n);b.head=B(p);b.tail=B(p)+Vector((0,0,.12))
        if parent:b.parent=data.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False);rig.show_in_front=True;return rig

def panel(g,points,col,bone='Body'):
    # A closed, thin glass pane gives deterministic outward normals in both FBX and
    # Unity's back-face-culling shader (an isolated polygon can be flipped by normal repair).
    normal=(Vector(points[1])-Vector(points[0])).cross(Vector(points[2])-Vector(points[0])).normalized()*.004
    n=len(points);verts=[Vector(p)+normal for p in points]+[Vector(p)-normal for p in points]
    faces=[tuple(range(n)),tuple(range(2*n-1,n-1,-1))]
    faces.extend((i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n));g.add(verts,faces,col,bone,False)

def shell(g,stations,col,bone='Body'):
    # A rounded eight-sided cross section; width, sill/shoulder and crown vary per station.
    verts=[]
    for z,w,bottom,top in stations:
        verts.extend([(w*.92,bottom,z),(w,bottom+.10,z),(w,top-.09,z),(w*.86,top,z),
                      (-w*.86,top,z),(-w,top-.09,z),(-w,bottom+.10,z),(-w*.92,bottom,z)])
    fs=[]
    for j in range(len(stations)-1):
        for k in range(8):fs.append((j*8+k,j*8+(k+1)%8,(j+1)*8+(k+1)%8,(j+1)*8+k))
    fs.extend([tuple(range(7,-1,-1)),tuple(range((len(stations)-1)*8,len(stations)*8))]);g.add(verts,fs,col,bone)

def wheels(g,truck):
    c=C;radius=.51 if truck else .38;x=1.02 if truck else .82
    for side,s in [('L',-1),('R',1)]:
        for end,z in [('F',2.12 if truck else 1.36),('R',-2.05 if truck else -1.36)]:
            bone='Wheel'+end+side;centre=(s*x,radius,z);minor=.115 if truck else .095;r=radius-minor
            g.torus(centre,r,minor,c['rubber'],bone,n=32,m=8)
            outside=s*(x+minor*.88)
            for rr,col in [(radius*.76,c['black']),(radius*.65,c['chrome'])]:
                g.torus((outside,radius,z),rr,.018,col,bone,n=28,m=5)
            g.rod((outside-s*.018,radius,z),(outside+s*.01,radius,z),radius*.57,c['steel'],bone,28)
            for i in range(6 if truck else 5):
                a=2*pi*i/(6 if truck else 5)
                g.rod((outside+s*.022,radius+.08*sin(a),z+.08*cos(a)),
                  (outside+s*.022,radius+radius*.58*sin(a+.08),z+radius*.58*cos(a+.08)),.027,c['chrome'],bone,6)
                g.ell((outside+s*.029,radius+.07*sin(a),z+.07*cos(a)),(.009,.014,.014),c['bolt'],bone,6,4)
            g.rod((outside,radius,z),(outside+s*.04,radius,z),.073,c['chrome'],bone,16)
            # Dark tread grooves remain flush with the crown, never floating outside tyres.
            for i in range(24):
                a=i*pi/12;g.rod((s*x-.045,radius+(radius-.004)*sin(a),z+(radius-.004)*cos(a)),
                 (s*x+.045,radius+(radius-.004)*sin(a+.018),z+(radius-.004)*cos(a+.018)),.004,c['black'],bone,4)
            # Steering knuckle and spring are visible through the wheel well.
            g.rod((s*(x-.14),radius,z),(s*(x-.14),radius+.32,z),.035,c['steel'],'Steer'+end+side,8)
            for j in range(5):g.torus((s*(x-.14),radius+.10+j*.035,z),.055,.009,c['gold'],'Steer'+end+side,axis='y',n=12,m=4)

def roadcar(g,kind):
    c=C;van=kind=='Van';hatch=kind=='Hatchback';taxi=kind=='Taxi';rear=-2.34 if van else -2.06;front=2.15
    # Sculpted unibody with wheel cut-outs: the low skirts stop at each circular wheel arch.
    shell(g,[(rear,.73,.84,1.03),(rear+.2,.88,.78,1.12),(-1.35,.89,.81,1.13),
             (.5,.89,.81,1.10),(1.45,.87,.81,1.04),(front,.72,.75,.96)],c['paint'])
    for s in [-1,1]:
        # Actual arched openings under shoulder line, outlined with flush body-colour trim.
        for z in [-1.36,1.36]:
            pts=[]
            for i in range(17):
                a=pi*i/16;pts.append((s*.888,.38+.455*sin(a),z+.455*cos(a)))
            g.curve(pts,.023,c['paint'])
        for a,b in [(rear+.05,-1.82),(-.9,.90),(1.82,front-.08)]:
            if b>a:g.box((s*.85,.67,(a+b)/2),(.10,.25,b-a),c['paint'],'Body',.025)
        g.box((s*.87,.52,0),(.08,.09,1.9),c['black'],'Body',.018)
        # Doors: contour seams, raised handles, rocker highlight, fuel flap.
        for z in [-1.02,.65]:g.curve([(s*.895,.84,z),(s*.895,1.1,z)],.007,c['paintDark'])
        g.curve([(s*.895,.76,-.88),(s*.895,.76,.88)],.007,c['paintDark'])
        for z in [-.65,.42]:g.box((s*.91,1.06,z),(.027,.035,.20),c['chrome'],'Body',.012)
        g.box((s*.895,1.02,-1.67),(.015,.13,.17),c['paintDark'],'Body',.018)
    # Cabin: raked windshield and curved roof; cargo van has tall square rear volume.
    cabinRear=-1.62 if hatch else -1.40;roofRear=-1.25 if hatch else -.88
    if van:
        shell(g,[(rear+.07,.82,1.04,2.0),(rear+.2,.86,1.04,2.13),(-.65,.86,1.04,2.13),(-.50,.81,1.04,2.0)],c['cream'],'Cargo')
        cabinRear=-.63;roofRear=-.50
    roofHeight=1.69 if hatch else 1.60
    if van:roofHeight=1.91
    shell(g,[(cabinRear,.80,1.07,1.15),(roofRear,.69,1.07,roofHeight),(.16,.68,1.06,roofHeight),(.91,.77,1.04,1.13)],c['paint'])
    # Four carefully fitted window panels sit on the actual sloped cabin surfaces.
    frontTop=roofHeight-(.04/.75)*(roofHeight-1.13)+.012
    frontBottom=roofHeight-(.67/.75)*(roofHeight-1.13)+.012
    panel(g,[(-.60,frontTop,.20),(.60,frontTop,.20),(.67,frontBottom,.83),(-.67,frontBottom,.83)],c['visor'])
    rearTop=roofHeight-(.035/(roofRear-cabinRear))*(roofHeight-1.15)+.012
    rearBottom=1.15+(.08/(roofRear-cabinRear))*(roofHeight-1.15)+.012
    panel(g,[(-.60,rearTop,roofRear-.035),(-.67,rearBottom,cabinRear+.08),(.67,rearBottom,cabinRear+.08),(.60,rearTop,roofRear-.035)],c['visor'])
    for s in [-1,1]:
        g.curve([(s*.60,frontTop+.002,.20),(s*.67,frontBottom+.002,.83)],.012,c['black'])
        g.rod((s*.28,frontBottom+.015,.81),(s*.52,frontBottom+.075,.71),.010,c['black'])
    for s in [-1,1]:
        # Side glass separated by a solid B-pillar; slightly smaller polygons expose painted seals.
        panel(g,[(s*.705,roofHeight-.08,.12),(s*.786,1.18,.76),(s*.803,1.18,-.18),(s*.711,roofHeight-.08,-.18)],c['glass'])
        if not van:panel(g,[(s*.711,roofHeight-.08,-.27),(s*.803,1.18,-.27),(s*.794,1.18,cabinRear+.13),(s*.711,roofHeight-.08,roofRear+.06)],c['glass'])
        g.curve([(s*.79,1.14,cabinRear+.12),(s*.80,1.14,.78)],.012,c['chrome'])
        g.rod((s*.8,1.24,.65),(s*.98,1.28,.63),.026,c['paint'],'MirrorL' if s<0 else 'MirrorR')
        g.box((s*1.0,1.29,.63),(.19,.14,.25),c['paint'],'MirrorL' if s<0 else 'MirrorR',.05)
        g.box((s*1.0,1.29,.492),(.14,.085,.018),c['chrome'],'MirrorL' if s<0 else 'MirrorR',.01)
    # Bonnet creases, chrome grille with individual slats, sculpted bumpers and plates.
    for s in [-1,1]:g.curve([(s*.46,1.08,.88),(s*.53,1.025,1.48),(s*.52,.995,1.94)],.008,c['paintDark'])
    g.box((0,.67,2.11),(1.62,.22,.18),c['paint'],'BumperFront',.065)
    g.box((0,.76,2.212),(.68,.22,.035),c['black'],'BumperFront',.04)
    for y in [.70,.76,.82]:g.box((0,y,2.235),(.57,.018,.015),c['chrome'],'BumperFront',.006)
    for s in [-1,1]:
        g.box((s*.60,.89,2.15),(.40,.16,.10),c['lamp'],'BumperFront',.04)
        g.box((s*.60,.92,2.209),(.33,.023,.01),c['white'],'BumperFront',.007)
        g.box((s*.76,.86,2.17),(.055,.09,.04),c['orange'],'BumperFront',.012)
        g.box((s*.68,.59,2.17),(.15,.07,.05),c['chrome'],'BumperFront',.02)
        g.box((s*.64,.94,rear-.03),(.34,.17,.075),c['red'],'BumperRear',.035)
        g.box((s*.64,.92,rear-.073),(.22,.034,.012),c['white'],'BumperRear',.008)
    g.box((0,.59,rear-.045),(1.69,.16,.17),c['paint'],'BumperRear',.04)
    for z,bone in [(2.233,'BumperFront'),(rear-.14,'BumperRear')]:
        g.box((0,.64 if z>0 else .79,z),(.44,.13,.016),c['white'],bone,.01)
        g.text('51A  268.89',(0,.64 if z>0 else .79,z+(.01 if z>0 else -.01)),.053,c['black'],bone,front=z>0)
    g.rod((.55,.46,rear-.08),(.55,.46,rear-.23),.041,c['chrome'],'BumperRear',12)
    if taxi:
        g.box((0,1.75,-.24),(.61,.23,.34),c['gold'],'Body',.045)
        g.text('TAXI',(0,1.76,-.42),.12,c['black']);g.text('TAXI',(0,1.76,-.06),.12,c['black'],front=True)
        for s in [-1,1]:
            for i in range(12):g.box((s*.899,.92,-.73+i*.12),(.012,.07,.105),c['white'] if i%2 else c['black'],'Body',.003)
    if van:
        # Rear double doors with perimeter seals, hinges, locking bars, cargo company lettering.
        for s in [-1,1]:
            g.box((s*.415,1.57,rear-.015),(.80,.95,.038),c['white'],'Cargo',.025)
            g.rod((s*.20,1.16,rear-.05),(s*.20,1.98,rear-.05),.016,c['chrome'],'Cargo',8)
            for y in [1.23,1.86]:g.box((s*.79,y,rear-.06),(.08,.08,.035),c['steel'],'Cargo',.012)
            g.box((s*.20,1.47,rear-.08),(.16,.035,.035),c['black'],'Cargo',.008)
            for z in [-1.9,-1.55,-1.2,-.85]:g.box((s*.865,1.57,z),(.025,.92,.025),c['chrome'],'Cargo',.006)
        g.text('GIAO HÀNG',(0,1.79,rear-.05),.135,c['paint'],'Cargo')
        g.text('PHỐ VIỆT',(0,1.57,rear-.05),.105,c['paint'],'Cargo')
    # Flat floor, chassis rails, independent axle bars; visible in collisions.
    g.box((0,.43,0),(1.48,.10,3.65),c['black'],'Frame',.03)
    for z in [-1.36,1.36]:g.rod((-.77,.38,z),(.77,.38,z),.05,c['steel'],'Frame',10)

def truck(g,drowsy):
    c=C
    g.box((0,.65,0),(1.72,.22,6.35),c['black'],'Frame',.045)
    for s in [-1,1]:
        g.box((s*.65,.86,-.55),(.11,.25,5.1),c['steel'],'Frame',.015)
        g.box((s*.88,.66,.65),(.28,.31,.86),c['steel'],'Frame',.05)
        for z in [-2.05,2.12]:g.rod((-1,.51,z),(1,.51,z),.08,c['steel'],'Frame',10)
    # Cab-over nose with curved corners and a raked brow, separate cargo body.
    vs=[]
    for y,w,front in [(.84,.99,3.18),(.98,1.05,3.21),(1.72,1.05,3.18),(2.58,.98,2.82),(2.65,.86,2.75)]:
        vs.extend([(-w,y,1.25),(-w,y,front),(w,y,front),(w,y,1.25)])
    fs=[]
    for j in range(4):
        for k in range(4):fs.append((j*4+k,j*4+(k+1)%4,(j+1)*4+(k+1)%4,(j+1)*4+k))
    fs.extend([(3,2,1,0),(16,17,18,19)]);g.add(vs,fs,c['paint'],'Body')
    panel(g,[(-.86,2.48,2.879),(.86,2.48,2.879),(.92,1.82,3.156),(-.92,1.82,3.156)],c['visor'])
    # Two wipers, centre badge and broad radiator mesh.
    for s in [-1,1]:
        g.curve([(s*.48,1.74,3.205),(s*.67,1.97,3.105),(s*.30,2.04,3.08)],.015,c['black'])
        panel(g,[(s*1.059,2.38,1.50),(s*1.059,1.85,1.50),(s*1.059,1.85,2.90),(s*1.059,2.39,2.65)],c['glass'])
        g.curve([(s*1.06,1.05,1.48),(s*1.07,1.05,2.85),(s*1.07,1.77,2.90)],.009,c['paintDark'])
        g.box((s*1.08,1.65,1.65),(.03,.055,.24),c['black'],'Body',.014)
        g.box((s*1.04,.81,1.10),(.28,.14,.49),c['chrome'],'Body',.03)
        bone='MirrorL' if s<0 else 'MirrorR'
        g.curve([(s*1.01,2.45,2.67),(s*1.26,2.43,2.94),(s*1.26,1.95,2.94)],.025,c['black'],bone)
        g.box((s*1.26,2.17,2.94),(.20,.43,.17),c['black'],bone,.05)
        g.box((s*1.26,2.17,2.84),(.15,.35,.025),c['chrome'],bone,.025)
        g.box((s*.77,1.14,3.22),(.39,.25,.07),c['lamp'],'BumperFront',.035)
        g.box((s*.94,1.14,3.23),(.07,.22,.045),c['orange'],'BumperFront',.015)
    g.box((0,1.21,3.23),(.93,.38,.04),c['black'],'BumperFront',.04)
    for y in [1.07,1.15,1.23,1.31]:g.box((0,y,3.26),(.82,.025,.022),c['chrome'],'BumperFront',.005)
    g.box((0,.76,3.23),(2.12,.23,.22),c['chrome'],'BumperFront',.05)
    g.ell((0,1.55,3.23),(.07,.07,.025),c['gold'],'BumperFront',12,6)
    g.box((0,.79,3.36),(.50,.16,.025),c['gold'],'BumperFront',.01)
    g.text('51C  548.26',(0,.79,3.38),.06,c['black'],'BumperFront',front=True)
    # Box body has corrugated panels, roof rails, reflectors, rear roll protection and locks.
    g.box((0,2.03,-1.0),(2.19,2.12,4.37),c['cream'],'Cargo',.06)
    g.box((0,3.12,-1.0),(2.25,.11,4.45),c['chrome'],'Cargo',.025)
    for s in [-1,1]:
        for z in [-3.12,-2.74,-2.36,-1.98,-1.60,-1.22,-.84,-.46,-.08,.30,.68,1.06]:
            g.box((s*1.105,2.03,z),(.038,2.0,.035),c['chrome'],'Cargo',.009)
        g.box((s*1.12,1.13,-1.0),(.028,.12,4.27),c['orange'],'Cargo',.008)
        g.box((s*1.09,.71,-.58),(.06,.15,2.0),c['chrome'],'Frame',.018)
        for z in [-1.5,-.4,.7]:g.box((s*1.13,1.11,z),(.025,.075,.16),c['red'],'Cargo',.006)
        g.box((s*.54,2.05,-3.20),(1.05,1.98,.055),c['steel'],'Cargo',.025)
        g.rod((s*.35,1.18,-3.25),(s*.35,2.94,-3.25),.021,c['chrome'],'Cargo',10)
        for y in [1.32,2.01,2.80]:
            g.box((s*1.01,y,-3.26),(.13,.12,.04),c['chrome'],'Cargo',.012)
            g.box((s*.35,y,-3.28),(.14,.08,.045),c['chrome'],'Cargo',.01)
        g.box((s*.35,1.55,-3.29),(.23,.045,.045),c['black'],'Cargo',.01)
        g.box((s*.77,.85,-3.21),(.36,.19,.055),c['red'],'BumperRear',.025)
        g.box((s*.80,.57,-2.5),(.42,.50,.06),c['rubber'],'Frame',.015)
    g.box((0,.56,-3.27),(2.14,.13,.15),c['chrome'],'BumperRear',.025)
    for i in range(9):g.box((-.88+i*.22,.56,-3.355),(.12,.09,.015),c['red'] if i%2 else c['white'],'BumperRear',.003)
    g.text('VẬN TẢI VIỆT',(0,2.58,-3.245),.19,c['black'],'Cargo')
    g.text('GIỮ KHOẢNG CÁCH',(0,2.29,-3.245),.105,c['black'],'Cargo')
    # Small comic personal touches; danger label remains a separate gameplay object.
    g.box((0,2.66,1.98),(.74,.10,.81),c['paint'],'Body',.04)
    for s in [-1,1]:g.box((s*.83,2.66,2.60),(.15,.08,.12),c['orange'],'Body',.025)
    if drowsy:
        g.text('CÀ PHÊ ĐÂU?',(0,1.52,3.235),.13,c['white'],'Body',front=True)
        # A lopsided sun visor and soft cargo sway make this truck recognisable, without fake collision motion.
        g.box((0,2.49,2.98),(1.87,.12,.24),c['paintDark'],'Body',.04)

def car_rotation(rig,bone,xyz):
    # Desired author-space XYZ angles; basis reflection is already baked into export axes.
    basis=rig.pose.bones[bone].bone.matrix_local.to_quaternion()
    # B is a reflection: pseudovectors transform with the opposite sign.
    v=B(tuple(math.radians(a) for a in xyz));v=-v
    q=Euler(tuple(v),'XYZ').to_quaternion();rig.pose.bones[bone].rotation_quaternion=basis.inverted()@q@basis

def animate(rig):
    records=[]
    for name in CLIPS:
        frames=31 if name in ('Idle','Drive') else 25
        action=bpy.data.actions.new(rig.name[:-4]+'_'+name);rig.animation_data_create();rig.animation_data.action=action
        for frame in range(1,frames+1,2):
            t=(frame-1)/(frames-1)
            for p in rig.pose.bones:p.rotation_mode='QUATERNION';p.rotation_quaternion=Quaternion();p.location=(0,0,0);p.scale=(1,1,1)
            if name=='Drive':
                pose_offset(rig,'Body',(0,.012*sin(4*pi*t),0));car_rotation(rig,'Body',(.28*sin(2*pi*t),0,.32*sin(4*pi*t)))
                car_rotation(rig,'Cargo',(0,0,.22*sin(2*pi*t)))
                for side in ['L','R']:
                    car_rotation(rig,'SteerF'+side,(0,1.2*sin(2*pi*t),0))
                    for end in ['F','R']:car_rotation(rig,'Wheel'+end+side,(360*t,0,0))
            elif name=='Brake':
                wave=sin(pi*t);car_rotation(rig,'Body',(2.3*wave,0,0));pose_offset(rig,'Body',(0,-.025*wave,.012*wave))
            elif name.startswith('Impact'):
                hit=sin(min(1,t/.23)*pi/2);rebound=sin(t*5*pi)*math.exp(-4*t)
                side=-1 if name=='ImpactLeft' else 1
                if name in ('ImpactLeft','ImpactRight'):
                    car_rotation(rig,'Body',(0,0,-side*(3.2*rebound+.8*hit)))
                    pose_offset(rig,'Body',(side*.035*rebound,0,0));car_rotation(rig,'MirrorL' if side<0 else 'MirrorR',(0,side*22*hit,0))
                    car_rotation(rig,'Cargo',(0,0,-side*1.3*rebound))
                else:
                    sign=1 if name=='ImpactFront' else -1
                    car_rotation(rig,'Body',(sign*3.5*rebound,0,0));pose_offset(rig,'Body',(0,-.025*abs(rebound),-sign*.035*rebound))
                    pose_offset(rig,'BumperFront' if sign>0 else 'BumperRear',(0,.035*hit,-sign*.11*hit))
                    car_rotation(rig,'BumperFront' if sign>0 else 'BumperRear',(sign*4*hit,0,0));car_rotation(rig,'Cargo',(sign*1.3*rebound,0,0))
            for p in rig.pose.bones:
                p.keyframe_insert(data_path='rotation_quaternion',frame=frame,group=p.name);p.keyframe_insert(data_path='location',frame=frame,group=p.name)
        action.use_fake_user=True;slot=rig.animation_data.action_slot;records.append((name,action,slot));rig.animation_data.action=None
    rig.animation_data_clear();rig.animation_data_create()
    for name,action,slot in records:
        track=rig.animation_data.nla_tracks.new();track.name=name;strip=track.strips.new(name,1,action);strip.action_slot=slot;strip.name=name;track.mute=True
    for p in rig.pose.bones:p.rotation_quaternion=Quaternion();p.location=(0,0,0)
    animate.clips[rig.name]=records;return CLIPS

animate.clips={}
for _name,_,_ in CONFIGS:
    _rig=bpy.data.objects.get(_name+'_Rig')
    if _rig:animate.clips[_rig.name]=[(n,a,a.slots[0]) for n in CLIPS for a in bpy.data.actions if a.name==_name+'_'+n and len(a.slots)]

def build_one(name,kind,paint):
    remove_owned_model(name);bpy.ops.object.select_all(action='DESELECT')
    collection=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(collection)
    bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[collection.name]
    heavy=kind in ('Truck','Drowsy');rig=skeleton(name,heavy);g=Geometry();wheels(g,heavy)
    if heavy:truck(g,kind=='Drowsy')
    else:roadcar(g,kind)
    # Geometry labels use the same embedded Vietnamese font as the city.
    material=palette(name,paint,'2B7D89','F0D8A0');mesh=finish_mesh(name,g,rig,material)
    clips=animate(rig);rig['asset_kind']=kind;rig['animation_contract']=','.join(clips);mesh.data.calc_loop_triangles()
    info={'name':name,'kind':kind,'vertices':len(mesh.data.vertices),'triangles':len(mesh.data.loop_triangles),'bones':len(rig.data.bones),'materials':1,'clips':clips,'wheel_radius':.51 if heavy else .38}
    MODELS[name]=(rig,mesh,info);make_lods(name);export_model(name);bpy.context.scene.frame_set(1)
    return info

def studio():
    collection=bpy.data.collections.get('PV_CarStudio')
    if collection:
        for obj in list(collection.objects):bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.collections.remove(collection)
    collection=bpy.data.collections.new('PV_CarStudio');bpy.context.scene.collection.children.link(collection)
    bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[collection.name]
    for i,(name,kind,paint) in enumerate(CONFIGS):
        MODELS[name][0].location=(i*3.6,0,0);preview_action(name,'Drive',1)
    mat=bpy.data.materials.new('PV_CarStudioFloor');mat.diffuse_color=(.18,.25,.25,1)
    bpy.ops.mesh.primitive_plane_add(size=1,location=(9,0,-.01));floor=bpy.context.object;floor.name='PV_CarStudioFloor';floor.scale=(30,16,1);floor.data.materials.append(mat)
    def cam(name,pos,target,scale):
        data=bpy.data.cameras.new(name);obj=bpy.data.objects.new(name,data);collection.objects.link(obj);obj.location=pos
        obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=scale;return obj
    scene=bpy.context.scene;scene.camera=cam('PV_CarLineupCamera',(11,-18,13),(9,0,1.0),25)
    cam('PV_SedanCamera',(-3,-5,3.4),(0,0,.85),5.4)
    cam('PV_TruckCamera',(10.5,-7.5,4.7),(14.4,0,1.5),8.5)
    for name,pos,power,size in [('Key',(1,-6,12),2800,10),('Fill',(18,-5,9),2400,9),('Rim',(10,7,11),3200,10)]:
        data=bpy.data.lights.new('PV_Car'+name,'AREA');data.energy=power;data.shape='DISK';data.size=size
        obj=bpy.data.objects.new(data.name,data);collection.objects.link(obj);obj.location=pos;obj.rotation_euler=(Vector((9,0,1))-obj.location).to_track_quat('-Z','Y').to_euler()
    scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=2000;scene.render.resolution_y=950;scene.render.resolution_percentage=100
    scene.world.use_nodes=True;bg=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs['Color'].default_value=(.16,.22,.26,1);bg.inputs['Strength'].default_value=.5
    scene.view_settings.view_transform='AgX';scene.frame_start=1;scene.frame_end=31;scene.render.fps=30
    for screen in bpy.data.screens:
        for area in screen.areas:
            if area.type=='VIEW_3D':area.spaces.active.region_3d.view_perspective='CAMERA';area.spaces.active.shading.type='MATERIAL'

def save_cars():
    for library in [bpy.data.meshes,bpy.data.armatures,bpy.data.materials,bpy.data.images]:
        for item in list(library):
            if item.users==0 and any(item.name.startswith(c[0]) for c in CONFIGS):library.remove(item)
    for font in bpy.data.fonts:
        if font.filepath and 'BeVietnam' in font.filepath:
            try:font.pack()
            except RuntimeError:pass
    for image in bpy.data.images:
        if image.name.startswith('PV_'):
            try:image.pack()
            except RuntimeError:pass
    bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Blender/PhoViet_Cars.blend')

def build_all():
    bpy.context.scene.render.fps=30
    for config in CONFIGS:print(json.dumps(build_one(*config)))
    studio();save_cars()
