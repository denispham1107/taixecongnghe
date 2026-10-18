"""Original Phố Việt vehicle/rider art. Run inside Blender through Blender MCP.
Authoring coordinates are metres: X right, Y up, Z road-forward; B() converts to Blender.
Each vehicle is one atlas material, one skinned mesh and a small rigid/deform bone rig.
"""
import bpy, math, json, os
from mathutils import Vector, Quaternion, Euler
from math import sin, cos, pi

ROOT = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT = ROOT + '/Assets/PhoViet/Art/Bikes'
os.makedirs(OUT, exist_ok=True)

def B(p): return Vector((-p[0], -p[2], p[1]))
def hexcol(h):
    return tuple((int(h[i:i+2],16)/255)**2.2 for i in (0,2,4))+(1,)

class Geometry:
    def __init__(self): self.v=[]; self.f=[]; self.colors=[]; self.bones=[]
    def add(self, verts, faces, color, bone='Frame', smooth=True):
        start=len(self.v);self.v.extend([B(p) for p in verts]);self.bones.extend([bone]*len(verts))
        for face in faces:self.f.append(tuple(start+i for i in face));self.colors.append((color,smooth))
    def ell(self,c,s,col,bone='Frame',n=16,m=10):
        vs=[]
        for j in range(m+1):
            a=pi*j/m
            for i in range(n):
                t=2*pi*i/n;vs.append((c[0]+s[0]*sin(a)*cos(t),c[1]+s[1]*cos(a),c[2]+s[2]*sin(a)*sin(t)))
        fs=[]
        for j in range(m):
            for i in range(n):fs.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
        self.add(vs,fs,col,bone)
    def rod(self,a,b,r,col,bone='Frame',n=8,r2=None):
        a,b=Vector(a),Vector(b);axis=(b-a).normalized();u=axis.cross(Vector((0,1,0)))
        if u.length<.01:u=axis.cross(Vector((1,0,0)))
        u.normalize();v=axis.cross(u).normalized();r2=r if r2 is None else r2
        vs=[p+(u*cos(i*2*pi/n)+v*sin(i*2*pi/n))*radius for p,radius in [(a,r),(b,r2)] for i in range(n)]
        fs=[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
        fs.extend([tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]);self.add(vs,fs,col,bone)
        self.colors[-1]=(col,False);self.colors[-2]=(col,False)
    def box(self,c,s,col,bone='Frame',bevel=.025):
        # Rounded rectangular cross sections and end caps, rather than sharp primitive cubes.
        x,y,z=c;hx,hy,hz=[a*.5 for a in s];r=min(bevel,hx*.8,hy*.8,hz*.8)
        vs=[];rings=[(-hz,.92),(-hz+r,1),(hz-r,1),(hz,.92)]
        for zz,fac in rings:
            for cx,cy,start in [(hx-r,hy-r,0),(-hx+r,hy-r,90),(-hx+r,-hy+r,180),(hx-r,-hy+r,270)]:
                for k in range(4):
                    a=math.radians(start+k*30);xx=cx+r*cos(a);yy=cy+r*sin(a)
                    vs.append((x+xx*fac,y+yy*fac,z+zz))
        fs=[]
        for j in range(3):
            for i in range(16):fs.append((j*16+i,j*16+(i+1)%16,(j+1)*16+(i+1)%16,(j+1)*16+i))
        fs.extend([tuple(range(15,-1,-1)),tuple(range(48,64))]);self.add(vs,fs,col,bone)
    def loft(self,sections,col,bone='Frame',vertical=False,n=16):
        vs=[]
        for along,width,depth,centre in sections:
            for i in range(n):
                a=2*pi*i/n
                vs.append((width*cos(a),along,centre+depth*sin(a)) if vertical else (width*cos(a),centre+depth*sin(a),along))
        fs=[]
        for j in range(len(sections)-1):
            for i in range(n):fs.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
        fs.extend([tuple(range(n-1,-1,-1)),tuple(range((len(sections)-1)*n,len(sections)*n))]);self.add(vs,fs,col,bone)
    def curve(self,points,r,col,bone='Frame',n=6):
        for a,b in zip(points,points[1:]):self.rod(a,b,r,col,bone,n)
    def torus(self,c,r,minor,col,bone='Frame',axis='x',n=28,m=6):
        vs=[]
        for i in range(n):
            a=2*pi*i/n
            for j in range(m):
                b=2*pi*j/m;rr=r+minor*cos(b);ax=minor*sin(b)
                d=(ax,rr*sin(a),rr*cos(a)) if axis=='x' else (rr*cos(a),ax,rr*sin(a))
                vs.append(tuple(c[k]+d[k] for k in range(3)))
        fs=[]
        for i in range(n):
            for j in range(m):fs.append((i*m+j,((i+1)%n)*m+j,((i+1)%n)*m+(j+1)%m,i*m+(j+1)%m))
        self.add(vs,fs,col,bone)
    def star(self,c,r,col,bone='Torso',front=False):
        vs=[c]
        for i in range(10):
            a=pi*.5+i*pi/5;rad=r if i%2==0 else r*.382
            vs.append((c[0]+rad*cos(a),c[1]+rad*sin(a),c[2]))
        fs=[(0,i+1,(i+1)%10+1) if front else (0,(i+1)%10+1,i+1) for i in range(10)]
        self.add(vs,fs,col,bone,False)
    def shirt_star(self,r,front):
        sections=[(1.13,.18,.115,-.25),(1.25,.215,.13,-.23),(1.46,.235,.135,-.17),(1.62,.255,.14,-.12),(1.69,.16,.095,-.10)]
        def surface(x,y):
            for a,b in zip(sections,sections[1:]):
                if a[0]<=y<=b[0]:
                    t=(y-a[0])/(b[0]-a[0]);w=a[1]+(b[1]-a[1])*t;d=a[2]+(b[2]-a[2])*t;z=a[3]+(b[3]-a[3])*t
                    return z+(1 if front else -1)*(d*math.sqrt(max(0,1-(x/w)**2))+.008)
            return -.02 if front else -.34
        vs=[(0,1.47,surface(0,1.47))]
        for i in range(10):
            a=pi*.5+i*pi/5;rad=r if i%2==0 else r*.382;x=rad*cos(a);y=1.47+rad*sin(a);vs.append((x,y,surface(x,y)))
        self.add(vs,[(0,i+1,(i+1)%10+1) if front else (0,(i+1)%10+1,i+1) for i in range(10)],C['gold'],'Torso',False)
    def text(self,value,position,size,col,bone='Frame',front=False):
        curve=bpy.data.curves.new('Label','FONT');curve.body=value;curve.size=size;curve.align_x='CENTER';curve.align_y='CENTER';curve.resolution_u=2
        obj=bpy.data.objects.new('TemporaryLabel',curve);bpy.context.collection.objects.link(obj);obj.location=B(position)
        obj.rotation_euler=(pi/2,0,0 if front else pi)
        deps=bpy.context.evaluated_depsgraph_get();evaluated=obj.evaluated_get(deps);mesh=evaluated.to_mesh()
        points=[obj.matrix_world@v.co for v in mesh.vertices]
        # Convert temporary Blender geometry back to authoring coordinates for the mesh builder.
        vs=[(-p.x,p.z,-p.y) for p in points];fs=[tuple(p.vertices) for p in mesh.polygons]
        self.add(vs,fs,col,bone,False);evaluated.to_mesh_clear();bpy.data.objects.remove(obj,do_unlink=True);bpy.data.curves.remove(curve)

# Palette indices are semantic and stable across every model; shared UV layout permits recolouring.
C={'paint':0,'paintDark':1,'cream':2,'rubber':3,'chrome':4,'steel':5,'skin':6,'skinDark':7,
   'shirt':8,'shirtDark':9,'pants':10,'shoe':11,'white':12,'gold':13,'lamp':14,'red':15,
   'glass':16,'black':17,'orange':18,'eye':19,'hair':20,'lip':21,'helmet':22,'stripe':23,
   'blue':24,'khaki':25,'clothLight':26,'sole':27,'bolt':28,'visor':29,'ear':30,'badge':31}

def palette(name,paint,shirt,helmet,police=False):
    cols=[paint,'762D2A','F4E9D2','20282D','B9D1D4','52656C','DBA074','AE7057',shirt,'9A3635',
          '283D56','30404B','FFF6E3','FFC846','FFF3AD','E43A36','72979F','18242C','F5A343',
          '24333C','332C2A','A35851',helmet,'EA4A39','387EDA','D7B97A','F0C9A4','DED8C5','82959B','31505E','BD8065','F8C84C']
    if police: cols[8]='D7B97A';cols[9]='B4945E';cols[10]='C6A870';cols[11]='242D31';cols[23]='B9272E'
    else: cols[9]=('155B61' if shirt!='DB3C34' else 'A62A2B')
    size=128; pixels=[]; masks=[]
    for y in range(size):
        for x in range(size):
            idx=(y//32)*8+x//16;pixels.extend(hexcol(cols[idx]));metal=1 if idx in (4,5,28) else 0
            gloss=.72 if idx in (0,4,14,16,22,29) else .2
            masks.extend((metal,metal,metal,gloss))
    image=bpy.data.images.new(name+'_Palette',width=size,height=size,alpha=True);image.pixels=pixels
    image.filepath_raw=OUT+'/'+name+'_Palette.png';image.file_format='PNG';image.save()
    mask=bpy.data.images.new(name+'_Surface',width=size,height=size,alpha=True);mask.pixels=masks
    mask.filepath_raw=OUT+'/'+name+'_Surface.png';mask.file_format='PNG';mask.save()
    mat=bpy.data.materials.new(name+'_Atlas');mat.use_nodes=True
    nodes=mat.node_tree.nodes;bsdf=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
    tex=nodes.new('ShaderNodeTexImage');tex.image=image;tex.interpolation='Closest'
    mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
    mask.colorspace_settings.name='Non-Color';surface=nodes.new('ShaderNodeTexImage');surface.image=mask;surface.interpolation='Closest'
    separate=nodes.new('ShaderNodeSeparateColor');mat.node_tree.links.new(surface.outputs['Color'],separate.inputs['Color'])
    mat.node_tree.links.new(separate.outputs['Red'],bsdf.inputs['Metallic'])
    inv=nodes.new('ShaderNodeMath');inv.operation='SUBTRACT';inv.inputs[0].default_value=1
    mat.node_tree.links.new(surface.outputs['Alpha'],inv.inputs[1]);mat.node_tree.links.new(inv.outputs[0],bsdf.inputs['Roughness'])
    return mat

def skeleton(name):
    data=bpy.data.armatures.new(name+'_Skeleton');rig=bpy.data.objects.new(name+'_Rig',data);bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
    definitions=[('Frame',None,(0,0,0)),('Steering','Frame',(0,1.12,.53)),('WheelRear','Frame',(0,.34,-.73)),
                 ('WheelFront','Steering',(0,.34,.78)),('Pelvis','Frame',(0,1.08,-.25)),('Torso','Pelvis',(0,1.21,-.23)),('Head','Torso',(0,1.77,-.1))]
    for side,s in [('L',-1),('R',1)]:
        definitions.extend([(side+'UpperArm','Torso',(s*.245,1.61,-.11)),(side+'Forearm',side+'UpperArm',(s*.36,1.34,.16)),
                            (side+'Thigh','Pelvis',(s*.17,1.08,-.25)),(side+'Shin',side+'Thigh',(s*.3,.72,.24)),(side+'Foot',side+'Shin',(s*.30,.43,.13))])
    for n,parent,p in definitions:
        bone=data.edit_bones.new(n);bone.head=B(p);bone.tail=B(p)+Vector((0,0,.12));bone.use_deform=True
        if parent:bone.parent=data.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False);rig.show_in_front=True
    return rig

def finish_mesh(name,g,rig,material):
    mesh=bpy.data.meshes.new(name+'_Mesh');mesh.from_pydata(g.v,[],g.f);mesh.update()
    obj=bpy.data.objects.new(name+'_Body',mesh);bpy.context.collection.objects.link(obj);obj.parent=rig;mesh.materials.append(material)
    uv=mesh.uv_layers.new(name='PaletteUV')
    for poly,(col,smooth) in zip(mesh.polygons,g.colors):
        poly.use_smooth=smooth;u=(col%8+.5)/8;v=(col//8+.5)/4
        for loop in poly.loop_indices:uv.data[loop].uv=(u,v)
    groups={bone:obj.vertex_groups.new(name=bone) for bone in set(g.bones)}
    indices={bone:[] for bone in groups}
    for i,bone in enumerate(g.bones):indices[bone].append(i)
    for bone,ids in indices.items():groups[bone].add(ids,1,'REPLACE')
    modifier=obj.modifiers.new('VehicleAndRiderRig','ARMATURE');modifier.object=rig
    # Recalculate winding after the coordinate conversion; preserve smooth curved panels.
    import bmesh
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free()
    return obj

MODELS={}

def motorcycle(g,kind):
    c=C; sporty=kind in ('Sport','Racer');police=kind=='Police';under=kind=='Underbone'
    # Wheels have actual rounded tyres, raised sidewalls, alloy spokes and drilled rotors.
    for z,bone in [(-.73,'WheelRear'),(.78,'WheelFront')]:
        g.torus((0,.34,z),.262,.078,c['rubber'],bone,n=32,m=8)
        for side in [-1,1]:
            x=side*.065
            g.torus((x,.34,z),.248,.012,c['steel'],bone,n=28,m=5)
            g.torus((x*.68,.34,z),.202,.015,c['chrome'],bone,n=28,m=5)
            for i in range(6):
                a=i*pi/3; b=a+.16
                g.rod((x,.34+.065*sin(a),z+.065*cos(a)),(x,.34+.197*sin(b),z+.197*cos(b)),.016,c['chrome'],bone,6)
            for i in range(20):
                a=i*pi/10
                g.curve([(side*.02,.34+.338*sin(a),z+.338*cos(a)),(side*.045,.34+.327*sin(a+.02),z+.327*cos(a+.02)),
                    (side*.072,.34+.293*sin(a+.035),z+.293*cos(a+.035))],.002,c['black'],bone,4)
        g.rod((-.09,.34,z),(.09,.34,z),.055,c['steel'],bone,12)
        g.rod((-.095,.34,z),(-.085,.34,z),.135,c['chrome'],bone,24)
        for i in range(12):
            a=i*pi/6;g.ell((-.101,.34+.105*sin(a),z+.105*cos(a)),(.004,.012,.012),c['black'],bone,6,4)
        g.box((-.122,.40,z+.105),(.065,.12,.065),c['red'],'Steering' if z>0 else 'Frame',.016)
    # Visible swingarm, fork tubes, springs, engine cooling fins and exhaust.
    for s in [-1,1]:
        g.rod((s*.13,.43,-.1),(s*.13,.34,-.73),.035,c['steel'])
        g.rod((s*.13,.34,.78),(s*.13,1.14,.53),.028,c['chrome'],'Steering')
        g.rod((s*.13,.48,.735),(s*.13,.75,.65),.043,c['steel'],'Steering')
        g.rod((s*.19,.39,-.65),(s*.19,.83,-.44),.027,c['chrome'])
        spring=[]
        a=Vector((s*.19,.45,-.62));b=Vector((s*.19,.78,-.46));axis=(b-a).normalized();u=Vector((1,0,0));v=axis.cross(u)
        for i in range(51):
            p=a+(b-a)*(i/50)+.042*(u*cos(i/50*12*pi)+v*sin(i/50*12*pi));spring.append(tuple(p))
        g.curve(spring,.008,c['gold'] if sporty else c['steel'],n=5)
    g.ell((0,.48,-.16),(.23,.19,.36),c['steel'])
    g.ell((.225,.48,-.16),(.035,.13,.17),c['chrome'],n=16,m=8)
    for i in range(5):g.box((0,.46+i*.033,.12),(.36,.016,.23),c['bolt'],bevel=.007)
    g.curve([(.17,.44,.22),(.23,.29,.2),(.30,.24,-.14),(.34,.34,-.63)],.025,c['chrome'])
    g.rod((.34,.34,-.93),(.34,.40,-.38),.068,c['steel'],n=12,r2=.075)
    g.rod((.34,.34,-.955),(.34,.34,-.933),.059,c['chrome'],n=12)
    g.rod((.34,.34,-.966),(.34,.34,-.955),.037,c['black'],n=10)
    g.box((.357,.425,-.64),(.05,.07,.39),c['chrome'],bevel=.018)
    for z in [-.55,-.72]:g.ell((.388,.43,z),(.009,.012,.012),c['black'],n=6,m=4)
    # A shaped rear fuselage narrows to the tail, with a separate upholstered seat.
    if sporty or under or police:
        g.loft([(-1.0,.07,.025,.83),(-.86,.18,.08,.79),(-.5,.24,.13,.74),(-.13,.21,.14,.70),(.14,.15,.1,.67)],c['paint'])
        g.loft([(-.87,.12,.025,.94),(-.65,.225,.045,.92),(-.25,.22,.045,.91),(-.05,.16,.035,.87)],c['rubber'])
        g.loft([(-.12,.12,.10,.86),(.13,.22,.17,.87),(.4,.18,.12,.88),(.51,.11,.055,.79)],c['paint'])
        for s in [-1,1]:
            g.curve([(s*.19,.79,-.4),(s*.21,.68,-.05),(s*.16,.70,.34)],.025,c['steel'])
            g.curve([(s*.18,.83,.03),(s*.23,.87,.24),(s*.16,.81,.5)],.02,c['gold'])
            g.rod((s*.18,.47,-.13),(s*.34,.47,-.13),.025,c['steel'])
            g.rod((s*.34,.47,-.21),(s*.34,.47,-.05),.023,c['rubber'])
    else:
        g.loft([(-1.0,.06,.035,.65),(-.85,.24,.17,.68),(-.52,.28,.22,.67),(-.15,.26,.19,.65),(.1,.19,.1,.57)],c['paint'])
        g.loft([(-.87,.13,.025,.94),(-.67,.245,.055,.93),(-.3,.235,.055,.92),(-.03,.17,.035,.89)],c['rubber'])
        g.box((0,.45,.24),(.54,.065,.68),c['rubber'],bevel=.022)
        for s in [-1,1]:g.curve([(s*.25,.48,-.02),(s*.24,.48,.44)],.014,c['chrome'])
    # The scooter leg shield is sculpted through elliptical sections, never a flat block.
    if not (sporty or police):
        g.loft([(.43,.15,.07,.52),(.58,.24,.09,.49),(.78,.29,.095,.49),(1.0,.27,.075,.56),(1.19,.19,.065,.60)],c['paint'],vertical=True,bone='Steering')
        g.loft([(.55,.07,.026,.59),(.77,.115,.035,.595),(.98,.12,.035,.645),(1.13,.07,.023,.67)],c['cream'],vertical=True,bone='Steering')
        for s in [-1,1]:
            g.curve([(s*.23,.7,.54),(s*.235,.9,.60),(s*.16,1.1,.66)],.006,c['chrome'],'Steering')
    else:
        for s in [-1,1]:
            g.ell((s*.18,.79,.43),(.11,.22,.25),c['paint'],'Steering',n=12,m=8)
            g.curve([(s*.21,.7,.28),(s*.27,.82,.43),(s*.15,.95,.59)],.013,c['gold'],'Steering')
    # Curved fender caps over each tyre.
    for z,bone in [(-.73,'Frame'),(.78,'Steering')]:
        verts=[]
        for a in range(11):
            t=pi*.15+a/10*pi*.7
            for x in [-.105,.105]:verts.append((x,.34+.375*sin(t),z+.375*cos(t)))
        g.add(verts,[(i*2,i*2+1,i*2+3,i*2+2) for i in range(10)],c['paint'],bone)
    g.ell((0,1.23,.6),(.32,.13,.135),c['paint'],'Steering')
    if sporty:
        for s in [-1,1]:g.ell((s*.115,1.235,.72),(.105,.047,.025),c['lamp'],'Steering',12,6)
    else:
        g.ell((0,1.235,.724),(.185,.068,.025),c['chrome'],'Steering',20,8)
        g.ell((0,1.235,.745),(.16,.052,.017),c['lamp'],'Steering',20,8)
        for x in [-.08,0,.08]:g.ell((x,1.24,.76),(.028,.03,.008),c['white'],'Steering',8,6)
    g.ell((0,1.305,.54),(.16,.018,.075),c['black'],'Steering',14,6)
    g.ell((0,1.321,.54),(.118,.005,.051),c['glass'],'Steering',14,6)
    for s in [-1,1]:
        g.curve([(0,1.25,.56),(s*.22,1.27,.55),(s*.43,1.28,.49)],.022,c['steel'],'Steering')
        g.rod((s*.34,1.28,.5),(s*.47,1.28,.47),.03,c['rubber'],'Steering')
        g.curve([(s*.31,1.29,.59),(s*.40,1.30,.57),(s*.48,1.29,.53)],.011,c['chrome'],'Steering')
        g.curve([(s*.29,1.29,.57),(s*.36,1.49,.55),(s*.47,1.55,.54)],.011,c['chrome'],'Steering')
        g.ell((s*.48,1.56,.54),(.10,.064,.028),c['black'],'Steering',12,8)
        g.ell((s*.48,1.56,.52),(.084,.05,.008),c['chrome'],'Steering',12,8)
        g.ell((s*.25,1.21,.70),(.032,.026,.034),c['orange'],'Steering',8,6)
    g.ell((0,.79,-.985),(.18,.055,.025),c['red'],n=16,m=6)
    g.box((0,.57,-1.015),(.265,.18,.028),c['white'],bevel=.012)
    for s in [-1,1]:g.ell((s*.10,.64,-1.036),(.008,.008,.004),c['bolt'],n=6,m=4)
    # Tiny registration strokes read as a real plate at game distance without font draw calls.
    g.text('59-V1',(0,.61,-1.035),.055,c['black']);g.text('268.79',(0,.545,-1.035),.048,c['black'])
    g.curve([(-.22,.96,-.73),(-.24,.99,-.94),(0,1.0,-1.01),(.24,.99,-.94),(.22,.96,-.73)],.016,c['chrome'])
    # Seam piping and vent slots give the close following camera useful rear detail.
    for s in [-1,1]:
        g.curve([(s*.21,.93,-.16),(s*.235,.97,-.4),(s*.20,.97,-.71)],.005,c['clothLight'])
        for i in range(4):g.rod((s*.255,.68+i*.027,-.43),(s*.255,.69+i*.027,-.65),.006,c['paintDark'],n=4)
    if police:
        for s in [-1,1]:
            g.box((s*.43,.76,-.67),(.30,.35,.56),c['cream'],bevel=.065)
            g.box((s*.43,.92,-.67),(.31,.04,.57),c['steel'],bevel=.016)
            g.box((s*.43,.79,-.963),(.21,.09,.015),c['blue'],bevel=.008)
            g.rod((s*.29,1.28,.71),(s*.29,1.36,.71),.055,c['steel'],'Steering',12)
            g.ell((s*.29,1.39,.71),(.06,.07,.06),c['red'] if s<0 else c['blue'],'Steering',12,8)
        g.loft([(1.30,.24,.018,.70),(1.51,.26,.018,.64),(1.69,.20,.018,.61)],c['glass'],vertical=True,bone='Steering')
        g.box((0,1.10,-1.0),(.58,.38,.39),c['cream'],bevel=.06)
        g.box((0,1.28,-1.0),(.60,.035,.40),c['steel'],bevel=.015)
        g.box((0,1.13,-1.20),(.42,.105,.014),c['blue'],bevel=.009)
        g.text('C.S.G.T',(0,1.13,-1.213),.065,c['white'])

def rider(g,kind):
    c=C;hero=kind=='Hero';police=kind=='Police';full=kind in ('Sport','Racer')
    g.ell((0,1.07,-.25),(.235,.145,.22),c['pants'],'Pelvis',16,8)
    g.loft([(1.13,.18,.115,-.25),(1.25,.215,.13,-.23),(1.46,.235,.135,-.17),(1.62,.255,.14,-.12),(1.69,.16,.095,-.10)],c['shirt'],'Torso',vertical=True,n=20)
    # Waistband, pocket seams, natural clothing folds and collar.
    g.loft([(1.14,.205,.125,-.24),(1.19,.208,.127,-.235)],c['shirtDark'],'Torso',vertical=True)
    g.torus((0,1.695,-.10),.09,.014,c['shirtDark'],'Torso',axis='y',n=16,m=5)
    g.rod((0,1.68,-.1),(0,1.80,-.08),.068,c['skin'],'Head',n=12)
    for s,side in [(-1,'L'),(1,'R')]:
        shoulder=(s*.245,1.61,-.11);elbow=(s*.36,1.34,.16);wrist=(s*.41,1.285,.49)
        g.ell(shoulder,(.095,.105,.11),c['shirt'],side+'UpperArm',12,8)
        upperEnd=Vector(shoulder).lerp(Vector(elbow),.72)
        g.rod(shoulder,tuple(upperEnd),.09,c['shirt'],side+'UpperArm',12,r2=.075)
        g.rod(tuple(upperEnd),elbow,.076,c['skin'] if not full else c['shirtDark'],side+'UpperArm',12,r2=.065)
        g.ell(elbow,(.066,.064,.067),c['skin'] if not full else c['shirtDark'],side+'Forearm',10,6)
        g.rod(elbow,wrist,.065,c['skin'] if not full else c['shirt'],side+'Forearm',12,r2=.043)
        g.ell((s*.425,1.28,.50),(.052,.044,.075),c['white'] if police else c['skin'],side+'Forearm',12,8)
        for finger in range(4):
            g.rod((s*(.398+finger*.015),1.278,.525),(s*(.398+finger*.015),1.25,.546),.009,c['white'] if police else c['skin'],side+'Forearm',6)
        g.rod((s*.39,1.3,.49),(s*.378,1.27,.515),.016,c['white'] if police else c['skin'],side+'Forearm',7)
        hip=(s*.17,1.08,-.25);knee=(s*.30,.72,.24);ankle=(s*.30,.43,.13)
        g.rod(hip,knee,.117,c['pants'],side+'Thigh',12,r2=.095);g.ell(knee,(.096,.09,.10),c['pants'],side+'Shin',12,8)
        g.rod(knee,ankle,.091,c['pants'],side+'Shin',12,r2=.065)
        g.curve([(s*.255,.94,-.08),(s*.33,.75,.20)],.005,c['steel'],side+'Thigh',5)
        g.ell((s*.30,.40,.21),(.088,.07,.17),c['shoe'],side+'Foot',14,8)
        g.box((s*.30,.355,.23),(.18,.035,.31),c['black'] if police else c['sole'],side+'Foot',.012)
        g.ell((s*.30,.40,.34),(.088,.052,.065),c['black'] if police else c['cream'],side+'Foot',12,6)
        for i in range(3):g.rod((s*.30-.045,.459,.20+i*.03),(s*.30+.045,.459,.20+i*.03),.004,c['black'] if police else c['white'],side+'Foot',4)
        # Sleeve edging and shoulder folds remain attached to the upper-arm bone.
        g.rod(tuple(Vector(shoulder).lerp(Vector(elbow),.65)),tuple(upperEnd),.078,c['shirtDark'],side+'UpperArm',12)
        g.curve([(s*.08,1.23,-.362),(s*.19,1.25,-.34)],.004,c['shirtDark'],'Torso',5)
    # Adult proportions, rounded jaw, ears, nose, eyes, brows and a relaxed smile.
    g.ell((0,1.94,-.085),(.15,.195,.159),c['skin'],'Head',20,14)
    g.ell((0,1.84,-.038),(.116,.075,.12),c['skin'],'Head',16,8)
    for s in [-1,1]:
        g.ell((s*.15,1.94,-.072),(.028,.057,.034),c['skin'],'Head',10,8)
        g.ell((s*.169,1.94,-.052),(.008,.029,.016),c['ear'],'Head',8,6)
        g.ell((s*.061,1.974,.054),(.037,.021,.012),c['white'],'Head',12,8)
        g.ell((s*.06,1.973,.065),(.013,.016,.007),c['eye'],'Head',10,8)
        g.ell((s*.064-.004,1.979,.071),(.004,.004,.003),c['white'],'Head',6,4)
        g.curve([(s*.025,2.012,.057),(s*.06,2.022,.056),(s*.098,2.012,.044)],.006,c['hair'],'Head')
        g.ell((s*.09,1.907,.045),(.032,.016,.010),c['skinDark'],'Head',10,6)
    g.ell((0,1.932,.079),(.027,.042,.033),c['skin'],'Head',12,8)
    g.curve([(-.043,1.874,.063),(-.021,1.865,.078),(0,1.861,.083),(.021,1.865,.078),(.043,1.874,.063)],.004,c['lip'],'Head')
    # Open helmet shell leaves the face readable; rear strap/vent details face the chase camera.
    vs=[];n=24;m=9
    for j in range(m+1):
        a=j/m*pi*.63
        for i in range(n):
            t=i*2*pi/n
            # Higher cut at the face, lower protection around the back and ears.
            aa=a*(.80 if sin(t)>.25 else 1)
            vs.append((.185*sin(aa)*cos(t),2.045+.188*cos(aa),-.10+.191*sin(aa)*sin(t)))
    g.add(vs,[(j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i) for j in range(m) for i in range(n)],c['helmet'],'Head')
    g.ell((0,2.105,-.1),(.045,.133,.192),c['stripe'],'Head',12,10)
    for s in [-1,1]:
        g.curve([(s*.16,1.99,-.04),(s*.115,1.83,.015),(0,1.802,.025)],.008,c['rubber'],'Head')
        for i in range(3):g.box((s*(.06+i*.028),2.126,-.247),(.015,.035,.008),c['black'],'Head',.003)
    g.box((0,2.025,.09),(.27,.035,.065),c['helmet'],'Head',.012)
    if full:
        g.ell((0,1.985,.065),(.155,.074,.053),c['visor'],'Head',20,10)
        g.curve([(-.14,1.99,.091),(0,2.022,.12),(.14,1.99,.091)],.006,c['chrome'],'Head')
        g.ell((0,1.85,.09),(.114,.07,.055),c['helmet'],'Head',16,8)
        g.box((0,1.86,.141),(.06,.018,.008),c['black'],'Head',.004)
    if hero:
        g.shirt_star(.158,False);g.shirt_star(.14,True)
        g.star((0,2.115,-.291),.042,c['gold'],'Head')
    elif police:
        for s in [-1,1]:
            g.box((s*.13,1.49,-.015),(.12,.13,.018),c['khaki'],'Torso',.008)
            shoulderBone=('L' if s<0 else 'R')+'UpperArm'
            g.box((s*.245,1.722,-.11),(.085,.02,.15),c['red'],shoulderBone,.006)
            for i in range(2):g.ell((s*.245,1.739,-.14+i*.05),(.018,.006,.018),c['gold'],shoulderBone,6,4)
        g.loft([(1.12,.228,.14,-.25),(1.17,.23,.141,-.25)],c['black'],'Pelvis',vertical=True)
        g.box((0,1.15,-.104),(.06,.04,.012),c['gold'],'Pelvis',.005)
        g.box((0,1.51,-.321),(.29,.08,.012),c['khaki'],'Torso',.008)
        g.text('C.S.G.T',(0,1.51,-.335),.054,c['gold'],'Torso')
        g.star((0,2.095,.093),.035,c['badge'],'Head',True)
        g.text('CSGT',(0,2.064,-.294),.044,c['red'],'Head')
    else:
        g.curve([(0,1.24,-.095),(0,1.48,-.025),(0,1.64,-.03)],.005,c['cream'],'Torso',5)
        g.box((-.11,1.46,-.019),(.10,.095,.01),c['shirtDark'],'Torso',.009)

def pose_rotation(rig,name,xyz):
    p=rig.pose.bones[name];p.rotation_mode='QUATERNION'
    basis=p.bone.matrix_local.to_quaternion();q=Euler(tuple(math.radians(a) for a in (xyz[0],-xyz[1],-xyz[2])),'XYZ').to_quaternion()
    p.rotation_quaternion=basis.inverted()@q@basis

def pose_offset(rig,name,offset):
    p=rig.pose.bones[name];p.location=p.bone.matrix_local.to_quaternion().inverted()@B(offset)

def animate(rig):
    names=['Idle','Ride','KickLeft','KickRight','FallLeft','FallRight','Recover']
    human=[p.name for p in rig.pose.bones if p.name not in ('Frame','Steering','WheelRear','WheelFront')]
    for name in names:
        rig.animation_data_clear()
        frames=25 if name in ('Idle','Ride') else 21 if 'Kick' in name else 41 if 'Fall' in name else 45
        action=bpy.data.actions.new(rig.name.replace('_Rig','')+'_'+name)
        rig.animation_data_create();rig.animation_data.action=action
        keyframes=sorted(set([1,frames]+[1+round((frames-1)*p) for p in [.16,.28,.42,.58,.76,.9]]))
        for frame in keyframes:
            t=(frame-1)/(frames-1)
            for p in rig.pose.bones:p.rotation_mode='QUATERNION';p.rotation_quaternion=Quaternion();p.location=(0,0,0);p.scale=(1,1,1)
            if name in ('Idle','Ride'):
                wave=sin(t*2*pi);pose_offset(rig,'Pelvis',(0,.0035*wave,0))
                pose_rotation(rig,'Torso',(wave*.65,0,wave*.4));pose_rotation(rig,'Head',(wave*.8,0,wave*1.2))
                if name=='Ride':
                    pose_rotation(rig,'Steering',(0,0,wave*1.3))
                    for bone in ['WheelRear','WheelFront']:pose_rotation(rig,bone,(t*360,0,0))
                    for side,s in [('L',-1),('R',1)]:pose_rotation(rig,side+'Forearm',(wave*.8,0,wave*s*.7))
            elif 'Kick' in name:
                s=-1 if 'Left' in name else 1;side='L' if s<0 else 'R'
                pulse=max(0,sin(pi*min(1,max(0,(t-.12)/.76))))
                pose_rotation(rig,side+'Thigh',(pulse*-12,pulse*-s*68,pulse*s*8))
                pose_rotation(rig,side+'Shin',(pulse*-33,0,0));pose_rotation(rig,side+'Foot',(pulse*12,0,0))
                pose_rotation(rig,'Torso',(pulse*-4,pulse*s*6,pulse*s*15))
                pose_rotation(rig,'Head',(0,pulse*s*5,pulse*s*18))
                pose_rotation(rig,side+'UpperArm',(pulse*2,pulse*s*8,pulse*-s*12))
                pose_rotation(rig,side+'Forearm',(pulse*12,0,0));pose_offset(rig,'Pelvis',(s*.035*pulse,.018*pulse,0))
            else:
                s=-1 if name=='FallLeft' else 1
                # In-game vehicle physics supplies the bike's fall; this clip animates its rider.
                fall=min(1,t/.55)
                if name=='Recover':fall=1-min(1,max(0,(t-.2)/.8))
                flail=sin(min(1,t/.65)*pi)*(.5 if name=='Recover' else 1)
                pose_offset(rig,'Pelvis',(s*.09*fall,-.04*fall,.035*fall))
                pose_rotation(rig,'Torso',(-14*fall,s*12*fall,s*-13*fall))
                pose_rotation(rig,'Head',(12*fall,s*8*fall,-s*12*fall))
                for side,sideSign in [('L',-1),('R',1)]:
                    pose_rotation(rig,side+'UpperArm',(-25*fall,sideSign*(30*fall+35*flail),sideSign*-12*fall))
                    pose_rotation(rig,side+'Forearm',(-35*fall,sideSign*15*flail,0))
                    pose_rotation(rig,side+'Thigh',(-8*fall,sideSign*-20*fall,sideSign*10*fall))
                    pose_rotation(rig,side+'Shin',(-25*flail,0,0))
            for p in rig.pose.bones:
                p.keyframe_insert(data_path='rotation_quaternion',frame=frame,group=p.name)
                p.keyframe_insert(data_path='location',frame=frame,group=p.name)
        action.use_fake_user=True
        # Keep a separate NLA strip per semantic clip so FBX exports actual editable takes.
        slot=rig.animation_data.action_slot
        rig.animation_data.action=None
        track=rig.animation_data.nla_tracks.new();track.name=name
        strip=track.strips.new(name,1,action);strip.action_slot=slot;strip.name=name
        track.mute=True
        # animation_data_clear on the next iteration would discard tracks; retain descriptors.
        if not hasattr(animate,'clips'):animate.clips={}
        animate.clips.setdefault(rig.name,[]).append((name,action,slot))
    rig.animation_data_clear();rig.animation_data_create()
    for name,action,slot in animate.clips[rig.name]:
        track=rig.animation_data.nla_tracks.new();track.name=name
        strip=track.strips.new(name,1,action);strip.action_slot=slot;strip.name=name;track.mute=True
    rig.animation_data.action=None
    for p in rig.pose.bones:p.rotation_quaternion=Quaternion();p.location=(0,0,0)
    return names

def build_model(name,kind,paint,shirt,helmet):
    bpy.ops.object.select_all(action='DESELECT')
    collection=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(collection)
    layer=bpy.context.view_layer.layer_collection.children.get(collection.name);bpy.context.view_layer.active_layer_collection=layer
    rig=skeleton(name);g=Geometry();motorcycle(g,kind);rider(g,kind)
    material=palette(name,paint,shirt,helmet,kind=='Police');mesh=finish_mesh(name,g,rig,material)
    clips=animate(rig);rig['asset_kind']=kind;rig['animation_contract']=','.join(clips)
    mesh.data.calc_loop_triangles()
    info={'name':name,'kind':kind,'vertices':len(mesh.data.vertices),'triangles':len(mesh.data.loop_triangles),'bones':len(rig.data.bones),'materials':1,'clips':clips}
    MODELS[name]=(rig,mesh,info);bpy.context.scene.frame_set(1);print(json.dumps(info))
    return rig,mesh

def export_model(name):
    rig,mesh,info=MODELS[name];bpy.ops.object.select_all(action='DESELECT');rig.animation_data.action=None
    location=rig.location.copy();rig.location=(0,0,0)
    rig.select_set(True);mesh.select_set(True);bpy.context.view_layer.objects.active=rig
    lods=[o for o in rig.children if '_LOD' in o.name]
    for obj in lods:obj.hide_set(False);obj.hide_render=False;obj.select_set(True)
    for track in rig.animation_data.nla_tracks:track.mute=False
    bpy.ops.export_scene.fbx(filepath=OUT+'/'+name+'.fbx',use_selection=True,object_types={'ARMATURE','MESH'},
        axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',
        add_leaf_bones=False,use_armature_deform_only=True,mesh_smooth_type='FACE',use_mesh_modifiers=True,
        bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=True,bake_anim_simplify_factor=.35,
        path_mode='RELATIVE',embed_textures=False)
    for track in rig.animation_data.nla_tracks:track.mute=True
    rig.location=location
    for obj in lods:obj.hide_set(True);obj.hide_render=True
    with open(OUT+'/'+name+'.json','w',encoding='utf-8') as f:json.dump(info,f,indent=2)
    bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);bpy.context.view_layer.objects.active=rig
    print('Exported '+name)

def preview_action(name,clip,frame=1):
    rig=MODELS[name][0]
    for track in rig.animation_data.nla_tracks:track.mute=True
    descriptor=next(x for x in animate.clips[rig.name] if x[0]==clip)
    rig.animation_data.action=descriptor[1];rig.animation_data.action_slot=descriptor[2]
    bpy.context.scene.frame_set(frame)

def save_source():
    for image in bpy.data.images:
        if image.name.startswith('PV_'):
            try:image.pack()
            except RuntimeError:pass
    bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Blender/PhoViet_Motorcycles.blend')

def presentation():
    from mathutils import Vector
    old=bpy.data.collections.get('PV_Presentation')
    if old:
        for obj in list(old.objects):bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.collections.remove(old)
    collection=bpy.data.collections.new('PV_Presentation');bpy.context.scene.collection.children.link(collection)
    bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[collection.name]
    mat=bpy.data.materials.new('PV_StudioFloor');mat.use_nodes=True
    node=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');node.inputs['Base Color'].default_value=(.36,.43,.43,1);node.inputs['Roughness'].default_value=.8
    bpy.ops.mesh.primitive_plane_add(size=1,location=(5,0,-.014));floor=bpy.context.object;floor.name='PV_StudioFloor';floor.scale=(14,5,1);floor.data.materials.append(mat)
    def area(name,location,energy,size):
        data=bpy.data.lights.new(name,'AREA');data.energy=energy;data.shape='DISK';data.size=size
        obj=bpy.data.objects.new(name,data);collection.objects.link(obj);obj.location=location;obj.rotation_euler=(Vector((5,0,1))-obj.location).to_track_quat('-Z','Y').to_euler()
    area('PV_Key',(1,-4,7),1800,7);area('PV_Fill',(9,-2,5),1100,6);area('PV_Rim',(4,3,6),1400,5)
    def camera(name,position,target,scale):
        data=bpy.data.cameras.new(name);obj=bpy.data.objects.new(name,data);collection.objects.link(obj);obj.location=position
        obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=scale;data.lens=50;return obj
    lineup=camera('PV_LineupCamera',(5,-12,7),(5,0,1),13.7)
    camera('PV_HeroCamera',(2.8,-4.6,3.0),(0,0,1.08),3.35)
    camera('PV_KickCamera',(-.8,4,2.8),(0,0,1.05),3.9)
    scene=bpy.context.scene;scene.camera=lineup;scene.render.resolution_x=1800;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
    scene.frame_start=1;scene.frame_end=25;scene.render.fps=30;scene.world.use_nodes=True
    background=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND');background.inputs['Color'].default_value=(.14,.19,.23,1);background.inputs['Strength'].default_value=.4
    titles=['TÀI XẾ PHỐ VIỆT','XE TAY GA','XE SỐ','XE THỂ THAO','QUÁI XẾ','MÔ TÔ CSGT']
    font=bpy.data.fonts.load(ROOT+'/Assets/PhoViet/Fonts/BeVietnamPro-SemiBold.ttf',check_existing=True)
    try:font.pack()
    except RuntimeError:pass
    for i,title in enumerate(titles):
        curve=bpy.data.curves.new('PV_Label','FONT');curve.body=title;curve.font=font;curve.size=.115;curve.align_x='CENTER';curve.align_y='CENTER'
        label=bpy.data.objects.new('PV_Label_'+str(i),curve);collection.objects.link(label);label.location=(i*2,-1.45,.002)
    for name in MODELS:preview_action(name,'Ride',1)
    bpy.ops.object.select_all(action='DESELECT');hero=MODELS['PV_HeroScooter'][0];hero.select_set(True);bpy.context.view_layer.objects.active=hero
    for screen in bpy.data.screens:
        for area in screen.areas:
            if area.type=='VIEW_3D':
                area.spaces.active.shading.type='MATERIAL';area.spaces.active.region_3d.view_perspective='CAMERA'
    print('Presentation scene and cameras ready')

def make_lods(name):
    rig,mesh,info=MODELS[name];lods=[]
    for level,ratio in [(1,.44),(2,.16)]:
        obj=mesh.copy();obj.data=mesh.data.copy();obj.name=name+'_LOD'+str(level)
        mesh.users_collection[0].objects.link(obj)
        # Decimate the bound rest mesh before its armature modifier; weights/UVs are retained.
        bpy.context.view_layer.objects.active=obj;obj.select_set(True)
        arm=obj.modifiers.get('VehicleAndRiderRig');obj.modifiers.remove(arm)
        dec=obj.modifiers.new('MobileLOD','DECIMATE');dec.ratio=ratio;dec.use_collapse_triangulate=True
        bpy.ops.object.modifier_apply(modifier=dec.name)
        arm=obj.modifiers.new('VehicleAndRiderRig','ARMATURE');arm.object=rig
        obj.data.calc_loop_triangles();lods.append(len(obj.data.loop_triangles));obj.hide_set(True);obj.hide_render=True;obj.select_set(False)
    info['lod_triangles']=[info['triangles']]+lods;print(json.dumps({'name':name,'lod_triangles':info['lod_triangles']}))

def remove_owned_model(name):
    MODELS.pop(name,None);animate.clips.pop(name+'_Rig',None)
    collection=bpy.data.collections.get(name)
    if collection:
        for obj in list(collection.objects):bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.collections.remove(collection)
    for action in list(bpy.data.actions):
        if action.name.startswith(name+'_'):bpy.data.actions.remove(action)

def rebuild_all():
    configs=[('PV_HeroScooter','Hero','D83C35','DB3C34','FFF1CF'),('PV_CityScooter','Scooter','168F98','23878D','F2C94C'),
        ('PV_Underbone','Underbone','F0DFB0','5275A7','EEE7D7'),('PV_SportBike','Sport','D5A638','78609A','EEE8DA'),
        ('PV_Racer','Racer','286ADA','343E55','286ADA'),('PV_TrafficPolice','Police','F1E8CC','D7B97A','F4E8CF')]
    bpy.context.scene.render.fps=30
    for i,config in enumerate(configs):
        remove_owned_model(config[0]);build_model(*config);make_lods(config[0]);export_model(config[0]);MODELS[config[0]][0].location.x=i*2
    presentation();save_source()

# MCP execution scopes are separate. Re-discover our existing assets for every small step.
animate.clips={}
for _rig in list(bpy.data.objects):
    if _rig.type!='ARMATURE' or not _rig.name.startswith('PV_') or not _rig.name.endswith('_Rig'):continue
    _name=_rig.name[:-4];_mesh=next((o for o in _rig.children if o.type=='MESH' and o.name.endswith('_Body')),None)
    if not _mesh:continue
    _mesh.data.calc_loop_triangles()
    _clips=[]
    for _action in bpy.data.actions:
        if _action.name.startswith(_name+'_'):
            _clip=_action.name[len(_name)+1:]
            if len(_action.slots):_clips.append((_clip,_action,_action.slots[0]))
    animate.clips[_rig.name]=_clips
    _lods=sorted([o for o in _rig.children if o.type=='MESH' and '_LOD' in o.name],key=lambda o:o.name)
    _counts=[len(_mesh.data.loop_triangles)]
    for _obj in _lods:_obj.data.calc_loop_triangles();_counts.append(len(_obj.data.loop_triangles))
    MODELS[_name]=(_rig,_mesh,{'name':_name,'kind':_rig.get('asset_kind',''),'vertices':len(_mesh.data.vertices),'triangles':_counts[0],
        'bones':len(_rig.data.bones),'materials':1,'clips':[x[0] for x in _clips],'lod_triangles':_counts})
