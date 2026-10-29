"""Approval study only; run in live Blender via MCP. Never writes Unity assets.
Local building coordinates: X frontage, Y towards back, Z up; facade Y=0.
Existing trees/pets are appended as immutable context meshes, not redesigned.
"""
import bpy, bmesh, math, json, os, random
from mathutils import Vector
from math import sin, cos, pi
from pathlib import Path
BASE=Path(r'C:/Users/HP/Documents/GameUnity/Taixecongnghe')
OUT=BASE/'ArtSource/Concepts/City-20261029'
P='PV86_'
if any(s.name.startswith(P) for s in bpy.data.scenes):
    raise RuntimeError('PV86 study already exists; inspect, do not overwrite it')
ORIGINAL=bpy.context.scene
ORIGINAL_INFO={'scene':ORIGINAL.name,'objects':len(ORIGINAL.objects),'filepath':bpy.data.filepath}
ENGINE=ORIGINAL.render.engine
BEVEL=next(i.identifier for i in bpy.types.Modifier.bl_rna.properties['type'].enum_items if i.identifier=='BEVEL')
ORTHO=next(i.identifier for i in bpy.types.Camera.bl_rna.properties['type'].enum_items if i.identifier=='ORTHO')
PNG=next(i.identifier for i in bpy.types.ImageFormatSettings.bl_rna.properties['file_format'].enum_items if i.identifier=='PNG')
FONT=bpy.data.fonts.load(str(BASE/'Assets/PhoViet/Fonts/BeVietnamPro-SemiBold.ttf'))
M={}
def rgb(h):
    vals=[int(h[i:i+2],16)/255 for i in (0,2,4)]
    return tuple(x/12.92 if x<=.04045 else ((x+.055)/1.055)**2.4 for x in vals)
def material(name,h,metal=0,rough=.48,glow=0,alpha=1):
    m=bpy.data.materials.new(P+name);m.use_nodes=True;c=rgb(h)
    n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    n.inputs['Base Color'].default_value=(*c,1);n.inputs['Metallic'].default_value=metal
    n.inputs['Roughness'].default_value=rough
    if glow:n.inputs['Emission Color'].default_value=(*c,1);n.inputs['Emission Strength'].default_value=glow
    if alpha<1:n.inputs['Alpha'].default_value=alpha
    m.diffuse_color=(*c,alpha);M[name]=m;return m
for n,h in {'porcelain':'EDF0F4','concrete':'ADB5C4','slate':'586079','navy':'24304E','iron':'1C2537','silver':'B4C6D7','glass':'294C71','glass2':'426F97','interior':'FAE4AF','floor':'E2CDB0','wood':'B87945','woodDark':'553C36','road':'333C50','white':'FFF9EE','yellow':'FFD24B','black':'141F31','cream':'FFF1D6','tile1':'ECDCC2','tile2':'CC775C','curb':'F6BE45','water':'42868F','cyan':'32CBF0','sky':'67B8F3','red':'F05251','pink':'F060A5','violet':'8262CC','teal':'259D99','orange':'F89334','cobalt':'2054B0','mustard':'DEA52C','green':'348A60','soil':'42322E'}.items():
    material(n,h,metal=.35 if n in ('silver','iron') else 0,rough=.27 if n.startswith('glass') else .5)
material('windowWarm','FFE4A1',glow=.38)
material('light','FFF0C5',glow=2.2)
material('led','84E3FF',glow=2)
material('clear','B6D4EA',rough=.12,alpha=.035)
material('interiorGlow','FFE2A0',glow=.65)
FACADE=['slate','porcelain','navy','concrete','slate','navy','porcelain','slate','navy','concrete','porcelain','slate','navy']
SIGN=['red','teal','orange','pink','green','violet','mustard','cyan','red','orange','pink','violet','cobalt']
SHOP=[('PHỞ VIỆT','PHỞ BÒ • PHỞ GÀ','pho'),('TIỆM TRÀ','TRÀ SỮA • TRÁI CÂY','tea'),('BÁNH MÌ','NÓNG GIÒN MỖI NGÀY','bread'),('TẠP HÓA','GÓC NHỎ PHỐ QUEN','grocery'),('NHÀ THUỐC','CHĂM SÓC SỨC KHỎE','pharmacy'),('GARA PHỐ','RỬA XE • CHĂM SÓC XE','garage'),('CÀ PHÊ','CÀ PHÊ • BÁNH NGỌT','coffee'),('CƠM TẤM','SƯỜN NƯỚNG • BÌ CHẢ','rice'),('BÚN BÒ HUẾ','HƯƠNG VỊ CỐ ĐÔ','noodle'),('HỦ TIẾU','NAM VANG • MÌ TƯƠI','soup'),('SINH TỐ','TƯƠI MỖI NGÀY','juice'),('TIỆM BÁNH','BÁNH NGỌT • BÁNH KEM','bakery'),('Cù Lao Petshop','Lưu Trú - Spa Pet','petshop')]
WIDTH=8.36
class Geo:
    def __init__(self,c):self.c=c;self.v={};self.f={}
    def add(self,verts,faces,m):
        v=self.v.setdefault(m,[]);f=self.f.setdefault(m,[]);s=len(v);v.extend(verts);f.extend(tuple(s+i for i in p) for p in faces)
    def box(self,loc,dims,m):
        x,y,z=(a/2 for a in dims);a,b,c=loc
        v=[(a+i*x,b+j*y,c+k*z) for i,j,k in [(-1,-1,-1),(-1,-1,1),(-1,1,-1),(-1,1,1),(1,-1,-1),(1,-1,1),(1,1,-1),(1,1,1)]]
        self.add(v,[(0,4,6,2),(1,3,7,5),(0,1,5,4),(2,6,7,3),(0,2,3,1),(4,5,7,6)],m)
    def rod(self,a,b,r,m,n=10):
        a,b=Vector(a),Vector(b);d=b-a;q=Vector((0,0,1)).rotation_difference(d.normalized())
        v=[tuple(a+q@Vector((r*cos(2*pi*i/n),r*sin(2*pi*i/n),z))) for z in (0,d.length) for i in range(n)]
        self.add(v,[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],m)
    def disc(self,x,y,z,rx,rz,m,n=40):
        self.add([(x,y,z)]+[(x+rx*cos(2*pi*i/n),y,z+rz*sin(2*pi*i/n)) for i in range(n)],[(0,i+1,(i+1)%n+1) for i in range(n)],m)
    def poly(self,pts,y,m):self.add([(x,y,z) for x,z in pts],[tuple(range(len(pts)-1,-1,-1))],m)
    def finish(self,name,bevel=.018):
        objs=[]
        for k,v in self.v.items():
            me=bpy.data.meshes.new(P+name+'_'+k);me.from_pydata(v,[],self.f[k]);me.update()
            bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
            ob=bpy.data.objects.new(me.name,me);self.c.objects.link(ob);me.materials.append(M[k]);objs.append(ob)
            if bevel and k not in ('clear','light','led'):
                mod=ob.modifiers.new('Soft manufactured edges',BEVEL);mod.width=bevel;mod.segments=2
        return objs
def collection(name):return bpy.data.collections.new(P+name)
def text(c,word,loc,size,m,maxw=None,angle=0):
    cu=bpy.data.curves.new(P+'Lettering','FONT');cu.body=word;cu.font=FONT;cu.size=size;cu.align_x='CENTER';cu.align_y='CENTER';cu.extrude=.009;cu.bevel_depth=.002;cu.bevel_resolution=1
    ob=bpy.data.objects.new(P+word,cu);c.objects.link(ob);ob.location=loc;ob.rotation_euler=(pi/2,0,angle);cu.materials.append(M[m])
    # Bound by evaluated text metrics, including Vietnamese diacritics.
    bpy.context.view_layer.update()
    width=sum(cu.size*(.3 if ch in ' .•-ilI' else .68) for ch in word)
    if maxw and width>maxw:ob.scale*=maxw/width
    return ob
def inst(c,coll,loc,angle=0,name='Building'):
    ob=bpy.data.objects.new(P+name,None);ob.instance_type='COLLECTION';ob.instance_collection=coll;ob.location=loc;ob.rotation_euler.z=angle;c.objects.link(ob);return ob
def paw(g,x,y,z,s,m='white'):
    g.disc(x,y,z-.05*s,.27*s,.23*s,m)
    for dx,dz,r in [(-.29,.28,.11),(-.1,.42,.12),(.13,.42,.12),(.32,.25,.11)]:g.disc(x+dx*s,y,z+dz*s,r*s,r*1.25*s,m)
def cat_art(g,x,y,z,s,bg):
    g.box((x,y+.035,z),(s*1.8,.08,s*2.05),bg)
    g.disc(x,y-.025,z,.61*s,.6*s,'white')
    g.poly([(x-.58*s,z+.22*s),(x-.51*s,z+.94*s),(x-.12*s,z+.52*s)],y-.025,'white')
    g.poly([(x+.58*s,z+.22*s),(x+.51*s,z+.94*s),(x+.12*s,z+.52*s)],y-.025,'white')
    for dx in [-.23,.23]:
        g.disc(x+dx*s,y-.04,z+.04*s,.1*s,.16*s,'navy');g.disc(x+(dx-.025)*s,y-.045,z+.09*s,.028*s,.038*s,'white')
    g.poly([(x-.06*s,z-.13*s),(x+.06*s,z-.13*s),(x,z-.21*s)],y-.04,'pink')
    for side in [-1,1]:
        for dz in [-.18,0,.18]:g.rod((x+side*.42*s,y-.04,z-.19*s),(x+side*.78*s,y-.04,z+dz*s),.016*s,'navy',6)
def rail(g,x,y,z,w,style=0):
    g.box((x,y,z+.51),(w,.07,.065),'iron');g.box((x,y,z-.02),(w,.09,.06),'iron')
    for j in range(int(w/.25)+1):g.box((x-w/2+j*w/int(w/.25),y,z+.25),(.035,.045,.52),'iron')
def awning(g,w,col,z=3.35,y=0):
    n=14;sw=w/n
    for i in range(n):
        x=-w/2+(i+.5)*sw;m=col if i%2==0 else 'white'
        # Sloping fabric, substantial scalloped valance and metal frame.
        pts=[(x-sw/2,y-.03,z+.38),(x+sw/2,y-.03,z+.38),(x+sw/2,y-1.55,z),(x-sw/2,y-1.55,z)]
        g.add(pts,[(0,3,2,1)],m);g.box((x,y-1.55,z-.13),(sw,.075,.29),m)
        # Rounded lower valance contour.
        g.disc(x,y-1.59,z-.26,sw*.47,.085,m,14)
    for x in [-w/2,w/2]:g.rod((x,y-.05,z+.38),(x,y-1.55,z-.05),.032,'iron')
    g.rod((-w/2,y-1.56,z+.01),(w/2,y-1.56,z+.01),.035,'iron')
def shop_interior(g,w,kind):
    # Open volume; no solid wall hiding the glazed storefront.
    g.box((0,2,.18),(w-.35,4,.16),'floor');g.box((0,3.9,1.8),(w-.35,.15,3.4),'interiorGlow')
    g.box((0,1.9,3.35),(w-.35,4,.12),'cream')
    for x in [-w/2+.15,w/2-.15]:g.box((x,2,1.8),(.18,4,3.4),'porcelain')
    for x in [-w*.27,w*.27]:
        g.box((x,3.45,1.35),(w*.32,.5,2.25),'woodDark')
        for z in [.6,1.2,1.8,2.4]:
            g.box((x,3.1,z),(w*.32,.7,.07),'wood')
            for j in range(5):g.box((x+(j-2)*.37,3.07,z+.19),(.24,.24,.31),['cream','orange','teal','pink','white'][j])
    for x in [-w*.24,w*.24]:g.box((x,1.4,3.25),(1.15,.23,.035),'light')
    if kind=='petshop':
        # Pet display/activity furniture, no restaurant tables or glass food cart.
        for x in [-2.8,2.8]:
            g.rod((x,1.7,.3),(x,1.7,1.5),.08,'wood');g.box((x,1.7,1.55),(.7,.7,.09),'cream')
        paw(g,0,3.78,2.35,1.25,'cobalt')
    else:
        g.box((2.3,1.8,.75),(1.4,.75,1.1),'wood');g.box((2.3,1.8,1.33),(1.55,.86,.1),'cream')
    # Glass panes and metal mullions/door handles.
    g.box((0,-.015,1.63),(w-.55,.022,2.85),'clear')
    for x in [-w/2+.18,-w/6,w/6,w/2-.18]:g.box((x,-.04,1.65),(.08,.1,3.05),'iron')
    for z in [.24,3.12]:g.box((0,-.05,z),(w-.3,.11,.1),'iron')
    for x in [-.27,.27]:g.rod((x,-.14,1.15),(x,-.14,1.67),.028,'silver')
def upper(g,w,floors,fac,accent,seed,depth=7.8):
    rng=random.Random(seed)
    h=4.9+floors*3
    g.box((0,depth/2,h/2+2.4),(w-.05,depth,h-4.8),fac)
    # Discrete structural columns with continuous slab edges.
    for x in [-w/2+.13,w/2-.13]:g.box((x,-.03,h/2+2.4),(.27,.32,h-4.8),'porcelain')
    for k in range(floors):
        z=4.9+k*3
        g.box((0,-.13,z+.02),(w+.08,.56,.22),'porcelain')
        for b in range(2):
            x=(-1 if b==0 else 1)*w*.236
            g.box((x,-.025,z+1.43),(w*.397,.06,2.48),'glass' if (k+b+seed)%3 else 'glass2')
            # alternate warm rooms visible behind the glazed panels
            if (k+b+seed)%3==0:g.box((x+.22,-.068,z+1.4),(w*.11,.025,2.1),'windowWarm')
            for dx in [-w*.2,0,w*.2]:g.box((x+dx,-.11,z+1.41),(.065,.1,2.55),'iron')
            g.box((x,-.12,z+1.3),(w*.41,.12,.065),'iron')
        if (k+seed)%3!=1:
            # Recessed-feeling balcony with dark metal railings.
            g.box((0,-.59,z+.13),(w-.2,1.12,.18),'porcelain');rail(g,0,-1.1,z+.28,w-.4)
            for x in [-w*.31,w*.31]:
                g.box((x,-.65,z+.38),(1.16,.46,.4),'concrete')
        if (k+seed)%3==1:
            # Colour-blocked cornice with energetic diagonal insert.
            g.box((0,-.29,z+.27),(w-.2,.18,.47),accent)
            for x in [-2.8,0,2.8]:g.rod((x-.18,-.4,z+.1),(x+.18,-.4,z+.42),.055,'yellow',6)
    g.box((0,depth/2,h+.08),(w+.24,depth+.2,.22),'porcelain')
    for x in [-w/2+.1,w/2-.1]:g.box((x,depth/2,h+.49),(.16,depth,.7),'slate')
    g.box((0,depth-.1,h+.49),(w,.18,.7),'slate')
    g.box((0,.12,h+.45),(w,.22,.64),'slate')
    # Set-back rooftop volume, vents and condenser fins.
    g.box((.8,4.9,h+1.14),(w*.65,3.4,2.05),fac)
    g.box((.8,4.9,h+2.22),(w*.68,3.6,.16),'porcelain')
    for x in [-2.4,-1.1]:
        g.box((x,5.8,h+.53),(.96,1.05,.75),'silver')
        for j in range(5):g.box((x-.38+j*.18,5.25,h+.52),(.025,.02,.46),'iron')
    return h
def billboard(g,c,w,h,seed,col):
    bw=min(w*.79,6.6);bh=4.7;z=h-2.3;x=.05
    g.box((x,-.33,z),(bw+.24,.32,bh+.24),'iron')
    g.box((x,-.53,z),(bw,.06,bh),col)
    for dx in [-bw/2-.04,bw/2+.04]:g.box((x+dx,-.58,z),(.055,.055,bh+.1),'led')
    if seed%3==0:cat_art(g,x,-.58,z+.22,1.9,col)
    elif seed%3==1:
        paw(g,x,-.59,z+.3,3.8,'white')
        for dx in [-2.4,2.4]:g.disc(x+dx,-.6,z+1.4,.13,.13,'yellow')
    else:
        for j,m in enumerate(['white','yellow','pink']):
            xx=x-2+j*1.5
            g.poly([(xx-.5,z-1.6),(xx+.3,z-1.6),(xx+1.4,z+.1),(xx+.7,z+1.7),(xx-.1,z+1.7),(xx+.7,z+.1)],-.59,m)
    # Lettering caption with real dimensional letters.
    text(c,['PHỐ VUI','GÓC PHỐ MỚI','CHẠM NIỀM VUI'][seed%3],(x,-.66,z-bh/2+.35),.22,'white',bw-.4)
def make_shop(i,corner=False):
    name,sub,kind=SHOP[i];col=SIGN[i];fac=FACADE[i];w=WIDTH if not corner else 10.8
    c=collection('House_%02d_%s'%(i,kind)+('_Corner' if corner else ''))
    c['asset_role']='buildings_only';c['business']=kind;c['frontage_m']=w;c['depth_m']=7.8;c['approval']='pending'
    g=Geo(c);floors=[4,5,4,3,5,3,4,4,5,3,4,5,4][i]
    h=upper(g,w,floors,fac,col,i+10,7.8)
    shop_interior(g,w,kind)
    for x in [-w/2+.13,w/2-.13]:g.box((x,0,2.4),(.28,.38,4.8),'porcelain')
    # Ground signage belongs to each house, not a shared generic strip.
    g.box((0,-.19,4.15),(w-.34,.4,1.35),'iron')
    g.box((0,-.43,4.16),(w-.48,.08,1.16),col)
    text(c,name,(0,-.51,4.36),.82,'white',w-1.2)
    text(c,sub,(0,-.515,3.92),.21,'white',(w-1.05)*.78)
    if i==12:paw(g,w*.405,-.53,4.3,.64)
    awning(g,w-.25,'teal' if i==12 else col,3.35)
    # Wall lamps, street address plates and door side texture relief.
    for x in [-w/2+.15,w/2-.15]:
        g.rod((x,-.35,2.5),(x,-.35,2.9),.055,'iron');g.box((x,-.36,2.78),(.21,.19,.35),'light')
    g.box((-w/2+.5,-.25,3.16),(.54,.08,.34),'cobalt');text(c,str(18+i*7),(-w/2+.5,-.31,3.16),.17,'white')
    # Vertical illuminated blade sign, anchored to wall, never across the road.
    x=w/2-.65
    g.box((x,-.81,6.6),(1.02,.24,2.54),col)
    for j,ch in enumerate(['PHỐ','VIỆT'] if i!=12 else ['PET','SPA']):text(c,ch,(x,-.96,7.1-j*.75),.27,'white',.88)
    g.rod((x,.1,7.7),(x,-.8,7.7),.055,'iron')
    if i in [0,2,4,6,8,11,12] or corner:billboard(g,c,w,h,i,'cobalt' if i==12 else ['pink','cyan','orange','violet'][i%4])
    else:
        g.box((-w*.26,-.23,h-2.4),(1.4,.16,4.35),col)
        for j,word in enumerate(['PHỐ','VIỆT','24H']):text(c,word,(-w*.26,-.36,h-1.18-j*1.1),.34,'white',1.18)
    if corner:
        # Return facade: side-facing windows wrap around the corner.
        for k in range(floors):
            for yy in [1.5,4,6.5]:
                g.box((w/2+.01,yy,6.2+k*3),(.055,1.8,2.2),'glass2')
                g.box((w/2+.06,yy,5.6+k*3),(.1,1.83,.06),'silver')
        g.box((w/2+.1,3.9,4.15),(.18,7.8,1.15),col)
    g.finish('Shop%02d'%i)
    ld=bpy.data.lights.new(P+'Shop_%02d_Interior'%i,'AREA');ld.energy=170;ld.color=(1,.68,.35);ld.shape='DISK';ld.size=3.5
    light=bpy.data.objects.new(ld.name,ld);c.objects.link(light);light.location=(0,1.9,3.05)
    # Existing Blender pets as context for the Petshop, never reconstructed.
    if kind=='petshop':
        for j,(sp,x,y) in enumerate([('PV_PetDog_LOD0',-2.2,-.72),('PV_PetCat_LOD0',2.4,-.65),('PV_PetRabbit_LOD0',1.5,1.5),('PV_PetDog_LOD0',-1.9,2.3),('PV_PetCat_LOD0',0,2)]):
            src=bpy.data.objects.get(sp)
            if src:
                ob=src.copy();ob.data=src.data;ob.parent=None;ob.location=(x,y,.27);ob.rotation_euler=(0,0,pi);ob.scale=(1.3,)*3;c.objects.link(ob)
    c['height_m']=round(h+2.3,3)
    return c
def append_objects(path,names):
    with bpy.data.libraries.load(str(path),link=False) as (src,dst):dst.objects=[n for n in names if n in src.objects]
    return dst.objects
PETS=append_objects(BASE/'ArtSource/Blender/PhoViet_PetAnimals.blend',['PV_PetDog_LOD0','PV_PetCat_LOD0','PV_PetRabbit_LOD0'])
TREES=[bpy.data.objects.get('PV_StreetTree%d_LOD0'%i) for i in range(3)]
if not all(TREES):TREES=append_objects(BASE/'ArtSource/Blender/PhoViet_SaigonStreets.blend',['PV_StreetTree%d_LOD0'%i for i in range(3)])
TREE_SHA={o.name:len(o.data.vertices) for o in TREES}
SHOPS=[make_shop(i) for i in range(13)]
CORNERS=[make_shop(i,True) for i in [6,12]]
def make_river(i):
    c=collection('River_Apartment_%02d'%i);c['asset_role']='buildings_only';c['approval']='pending'
    w=16.72;depth=3;floors=6+i;h=3.6+floors*3
    c['frontage_m']=w;c['depth_m']=depth;c['height_m']=h+.9
    g=Geo(c);fac=['porcelain','slate','navy','concrete'][i];accent=['teal','pink','orange','violet'][i]
    # Three-metre river envelope, recessed loggias, detail on BOTH faces.
    g.box((0,1.5,h/2),(w,3,h),fac)
    for side in [0,1]:
        y=0 if side==0 else depth;d=-1 if side==0 else 1
        for k in range(floors):
            z=3.6+k*3
            g.box((0,y+d*.035,z),(w,.12,.2),'porcelain')
            for j in range(6):
                x=-w/2+(j+.5)*w/6
                g.box((x,y+d*.066,z+1.4),(2.27,.035,2.4),'glass2' if (j+k)%3==0 else 'glass')
                if (j+k+i)%4==0:g.box((x+.3,y+d*.094,z+1.4),(.6,.02,2.1),'windowWarm')
                g.box((x,y+d*.1,z+1.4),(.055,.07,2.4),'iron')
                for dz in [.3,1.9]:g.box((x,y+d*.115,z+dz),(2.28,.04,.045),'silver')
                # Balconies entirely within 0.12m outer envelope.
                if (j+i)%2==0:
                    g.box((x,y+d*.08,z+.62),(2.4,.055,.07),'iron')
                    for dx in [-1.1,-.55,0,.55,1.1]:g.box((x+dx,y+d*.09,z+.38),(.03,.035,.5),'iron')
            if i%2:
                for x in [-6.7,-4,4,6.7]:g.box((x,y+d*.09,z+1.5),(.11,.06,2.7),accent)
        for x in [-w/2+.13,w/2-.13]:g.box((x,y,h/2),(.28,.22,h),'porcelain')
        for j in range(3):
            x=(j-1)*w/3;col='cobalt' if j==1 and i%2==0 else ['teal','orange','pink','violet'][(j+i)%4]
            g.box((x,y+d*.04,1.6),(w/3-.2,.045,2.5),'windowWarm')
            g.box((x,y+d*.085,3.02),(w/3-.15,.09,.8),col)
            word='Cù Lao Petshop' if col=='cobalt' else ['GÓC CÀ PHÊ','TIỆM TRÀ','PHỐ BÁNH'][j]
            if side==0:text(c,word,(x,y-.155,3.1),.28,'white',w/3-.6)
            else:text(c,word,(x,y+.155,3.1),.28,'white',w/3-.6,pi)
            for dx in [-w/6+.2,0,w/6-.2]:g.box((x+dx,y+d*.11,1.53),(.07,.05,2.48),'iron')
        g.box((0,y,h+.25),(w,.2,.5),accent)
    g.box((0,1.5,h+.55),(w*.6,1.7,.9),'slate')
    g.finish('River%02d'%i,.012)
    return c
RIVERS=[make_river(i) for i in range(4)]
def new_scene(name):
    sc=bpy.data.scenes.new(P+name);sc.render.engine=ENGINE;sc.render.image_settings.file_format=PNG
    sc.render.resolution_percentage=100;sc.render.image_settings.color_mode='RGB'
    sc.render.resolution_x=1920;sc.render.resolution_y=1080
    sc.render.film_transparent=False
    if 'AgX' in [i.identifier for i in sc.view_settings.bl_rna.properties['view_transform'].enum_items]:sc.view_settings.view_transform='AgX'
    sc.view_settings.view_transform='Standard';sc.view_settings.exposure=-.25
    sc.world=bpy.data.worlds.new(P+name+'_Sky');sc.world.use_nodes=True
    nt=sc.world.node_tree;bg=next(n for n in nt.nodes if n.type=='BACKGROUND');bg.inputs['Color'].default_value=(*rgb('DBECFF'),1);bg.inputs['Strength'].default_value=.65
    camera_bg=nt.nodes.new('ShaderNodeBackground');camera_bg.inputs['Color'].default_value=(*rgb('53A8EB'),1);camera_bg.inputs['Strength'].default_value=1
    path=nt.nodes.new('ShaderNodeLightPath');mix=nt.nodes.new('ShaderNodeMixShader');output=next(n for n in nt.nodes if n.type=='OUTPUT_WORLD')
    nt.links.new(path.outputs['Is Camera Ray'],mix.inputs[0]);nt.links.new(bg.outputs[0],mix.inputs[1]);nt.links.new(camera_bg.outputs[0],mix.inputs[2]);nt.links.new(mix.outputs[0],output.inputs[0])
    # Uniform blue sky, lit separately; do not rely on version-specific sky enums.
    c=collection(name+'_PRESENTATION_ONLY');sc.collection.children.link(c)
    ld=bpy.data.lights.new(P+name+'Sun',next(i.identifier for i in bpy.types.Light.bl_rna.properties['type'].enum_items if i.identifier=='SUN'));ld.energy=2.5;ld.angle=.08
    ob=bpy.data.objects.new(ld.name,ld);c.objects.link(ob);ob.rotation_euler=(.45,-.52,-.55)
    ld=bpy.data.lights.new(P+name+'Fill',next(i.identifier for i in bpy.types.Light.bl_rna.properties['type'].enum_items if i.identifier=='AREA'));ld.energy=1800;ld.shape=next(i.identifier for i in ld.bl_rna.properties['shape'].enum_items if i.identifier=='DISK');ld.size=32
    ob=bpy.data.objects.new(ld.name,ld);c.objects.link(ob);ob.location=(0,4,30)
    for side in [-1,1]:
        ld=bpy.data.lights.new(P+name+'FacadeFill'+str(side),'AREA');ld.energy=3500;ld.shape='RECTANGLE';ld.size=55;ld.size_y=14
        ob=bpy.data.objects.new(ld.name,ld);c.objects.link(ob);ob.location=(-side*2,28,15);ob.rotation_euler=(Vector((side*12,28,7))-ob.location).to_track_quat('-Z','Y').to_euler()
    return sc,c
def cam(c,name,loc,target,lens=32,ortho=None):
    cu=bpy.data.cameras.new(P+name);ob=bpy.data.objects.new(P+name,cu);c.objects.link(ob);ob.location=loc
    ob.rotation_euler=(Vector(target)-Vector(loc)).to_track_quat('-Z','Y').to_euler();cu.lens=lens;cu.clip_end=600
    if ortho:cu.type=ORTHO;cu.ortho_scale=ortho
    return ob
def tree(c,x,y,i=0,scale=1,z=.22):
    src=TREES[i%3];o=src.copy();o.data=src.data;o.parent=None;o.name=P+'Existing_Tree_Context';c.objects.link(o);o.location=(x,y,z);o.scale=(scale,)*3;o.rotation_euler=(0,0,i*.7);o['context_only']='original tree geometry unchanged';return o
def road(g,length,start=-28):
    end=start+length
    g.box((0,(start+end)/2,-.19),(12.8,length,.38),'road')
    for sign in [-1,1]:
        g.box((sign*8.6,(start+end)/2,-.03),(4.2,length,.25),'tile1')
        # Checker pavement has real grout-separated squares.
        for y in range(int(start),int(end),2):
            for j in range(2):g.box((sign*(7.4+j*1.45),y,.115),(1.42,1.96,.06),'tile2' if (j+y//2)%2 else 'tile1')
        for y in range(int(start),int(end)):
            g.box((sign*6.6,y,.13),(.25,.97,.28),'curb' if y%2 else 'white')
    for y in range(int(start),int(end),6):
        g.box((0,y,.014),(.13,2.5,.018),'yellow')
        for x in [-3.1,3.1]:g.box((x,y,.016),(.09,2.15,.018),'white')
    for y in [13,64]:
        for x in [-5.8,-4.5,-3.2,-1.9,-.6,.7,2,3.3,4.6,5.9]:g.box((x,y,.02),(.7,2.0,.023),'white')
    # Direction markings formed as coherent painted arrows.
    for x in [-4.65,-1.55,1.55,4.65]:
        g.box((x,-10,.027),(.16,2.45,.02),'white')
        g.add([(x-.58,-9,.04),(x+.58,-9,.04),(x,-7.9,.04)],[(0,1,2)],'white')
def street_lamps(g):
    for side in [-1,1]:
        for y in [-5,20,47,75]:
            x=side*6.9
            g.rod((x,y,.2),(x,y,7.9),.07,'iron')
            g.rod((x,y,7.9),(x-side*1.5,y,8.35),.06,'iron')
            g.box((x-side*1.6,y,8.35),(.65,.27,.1),'silver');g.box((x-side*1.6,y,8.29),(.57,.22,.025),'light')
            g.box((x,y,5.4),(.55,.22,1.1),'iron')
            for j in range(3):g.disc(x,y-.14,5.75-j*.35,.13,.13,'teal' if j==2 else 'glass')
def street_context():
    sc,c=new_scene('Street_Preview');b=collection('Street_BUILDINGS_ONLY');sc.collection.children.link(b);g=Geo(c);road(g,148);street_lamps(g)
    # 30 storefronts, 9 Petshops = 30%; shop positions only for presentation.
    pattern=[12,3,11,0,12,4,7,12,1,5,12,8,2,12,10]
    for side in [-1,1]:
        for j in range(15):
            index=pattern[j] if side==1 else ([11,12,0,2,4,12,6,7,9,12,10,1,5,8,12][j])
            loc=(side*10.6,-4+j*WIDTH,.16);rot=-side*pi/2
            ob=inst(b,SHOPS[index],loc,rot,'House_%s_%02d'%(side,j));ob['study_business']=SHOP[index][2]
            tree(c,side*8.55,.18+j*WIDTH,j)
    # Tall double-sided apartments as distant continuation, not new road objects.
    for side in [-1,1]:
        for j in range(3):inst(b,RIVERS[j%4],(side*13,126+j*17,.1),-side*pi/2,'DistantRiverBlock')
    for side in [-1,1]:
        for y in range(-15,114,4):
            g.rod((side*6.98,y,.2),(side*6.98,y,.85),.07,'iron');g.rod((side*6.98,y,.58),(side*6.98,y,.66),.073,'yellow')
    g.finish('StreetStage',.007)
    wide=cam(c,'Street_Wide',(0,-30,9.2),(0,37,6.2),29)
    portrait=cam(c,'Street_Portrait',(0,-32,10.5),(0,34,6.4),24)
    detail=cam(c,'Petshop_Detail',(0,-17,5.8),(10.6,-4,3.2),39)
    corner=cam(c,'Architecture_Close',(1.7,18,8.6),(-10.6,28,7.8),32)
    sc.camera=wide
    return sc,c,{'street-wide':wide,'street-portrait':portrait,'petshop-detail':detail,'architecture-close':corner}
STREET,STAGE,CAMS=street_context()
# Existing player car copied into the study only, to judge building scale.
carobjs=append_objects(BASE/'ArtSource/Blender/PhoViet_BlueSupercar.blend',['PV_BlueSupercar_Rig','PV_BlueSupercar_Body'])
carcoll=collection('EXISTING_CAR_CONTEXT_ONLY')
for ob in carobjs:
    carcoll.objects.link(ob)
    if ob.type=='ARMATURE':
        ob.animation_data_clear();ob.location=(0,0,0)
        for bone in ob.pose.bones:bone.matrix_basis.identity()
    if ob.type=='MESH':
        for mod in ob.modifiers:
            if mod.type=='ARMATURE':mod.object=next(o for o in carobjs if o.type=='ARMATURE')
car=inst(STAGE,carcoll,(1.55,-8,.03),pi,'Existing_Player_Car_Context');car.scale=(.85,)*3
# Gallery of all 19 prototypes; stages and numbering are separate from assets.
GALLERY,GC=new_scene('Building_Catalogue');gg=Geo(GC)
LIB=SHOPS+CORNERS+RIVERS
for i,col in enumerate(LIB):
    row=i//7;idx=i%7;x=(idx-3)*19;y=row*45
    inst(GC,col,(x,y,0),0,'Catalogue_%02d'%(i+1))
    gg.box((x,2,-.25),(18,12,.45),'concrete')
    text(GC,('%02d  '%(i+1))+(SHOP[i][0] if i<13 else 'GÓC PHỐ' if i<15 else 'CĂN HỘ VEN SÔNG'),(x,-2.4,.42),.45,'navy',16)
gg.finish('CatalogueStage',.02)
GALLERY.camera=cam(GC,'Catalogue',(77,-113,105),(0,42,10),ortho=155)
RIVER,RC=new_scene('Riverside_Preview');rg=Geo(RC)
for i in range(4):inst(RC,RIVERS[i],((i-1.5)*16.72,0,.25),0,'River_%d'%i)
rg.box((0,-6,-.15),(83,12,.4),'road');rg.box((0,-1.3,.08),(83,2.6,.22),'tile1')
rg.box((0,4.8,-1),(83,3.5,2.3),'concrete');rg.box((0,17,-1.3),(170,23,.2),'water')
for x in range(-36,38,6):tree(RC,x,-1.4,x)
rg.finish('RiverStage',.01)
RIVER.camera=cam(RC,'River',(53,55,26),(0,0,12),40)
META={'approval_required':True,'approved':False,'unity_installed':False,'prefix':P,'original':ORIGINAL_INFO,
      'style':'colourful modern shop-houses, navy structural frames, illuminated warm shop windows, striped awnings, playful pet billboards',
      'building_families':[{'collection':c.name,'business':c.get('business','corner_or_river'),'width':c.get('frontage_m'),'depth':c.get('depth_m'),'height':c.get('height_m'),'base_triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in c.objects if o.type=='MESH')} for c in LIB],
      'trees':{'source':'ArtSource/Blender/PhoViet_SaigonStreets.blend','geometry_unchanged':True,'sources':TREE_SHA},
      'renders':[
          {'name':'street-wide','scene':STREET.name,'camera':CAMS['street-wide'].name,'size':[1920,1080]},
          {'name':'street-portrait','scene':STREET.name,'camera':CAMS['street-portrait'].name,'size':[1080,1600]},
          {'name':'petshop-detail','scene':STREET.name,'camera':CAMS['petshop-detail'].name,'size':[1600,1200]},
          {'name':'architecture-close','scene':STREET.name,'camera':CAMS['architecture-close'].name,'size':[1600,1200]},
          {'name':'building-catalogue','scene':GALLERY.name,'camera':GALLERY.camera.name,'size':[2100,1500]},
          {'name':'riverside','scene':RIVER.name,'camera':RIVER.camera.name,'size':[1920,1080]}]}
for sc in [STREET,GALLERY,RIVER]:
    # Keep all images high resolution; this is visual approval, not a performance claim.
    sc.render.resolution_percentage=100
for im in bpy.data.images:
    if im.source=='FILE' and not im.packed_file and im.filepath:
        try:im.pack()
        except Exception:pass
bpy.context.window.scene=STREET
for a in bpy.context.screen.areas:
    if a.type=='VIEW_3D':a.spaces.active.region_3d.view_perspective='CAMERA'
OUT.mkdir(parents=True,exist_ok=True)
(OUT/'study.json').write_text(json.dumps(META,indent=2,ensure_ascii=False),encoding='utf8')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'PhoViet_NewCity_Approval.blend'),copy=True)
exec(compile((OUT/'refine_city_study.py').read_text(encoding='utf8'),'refine_city_study.py','exec'))
print(json.dumps({'scenes':[(s.name,len(s.objects)) for s in bpy.data.scenes], 'families':len(LIB),'triangles':sum(x['base_triangles'] for x in META['building_families']), 'original_scene_objects':len(ORIGINAL.objects),'original_filepath':bpy.data.filepath,'out':str(OUT)},ensure_ascii=False))
