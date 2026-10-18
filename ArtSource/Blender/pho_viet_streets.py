"""Original Vietnamese streets, authored in the live Blender through MCP.
Metres: author coordinates X across facade, Y up, Z into the shop.
Each asset exports three shared-atlas LOD meshes; signs are flush with the wall.
"""
import bpy, bmesh, math, os, json, random
from mathutils import Vector
ROOT='C:/Users/HP/Documents/GameUnity/Taixecongnghe'
exec(compile(open(ROOT+'/ArtSource/Blender/pho_viet_bikes.py',encoding='utf-8').read(),'pho_viet_bikes.py','exec'))
OUT=ROOT+'/Assets/PhoViet/Art/Streets';os.makedirs(OUT,exist_ok=True)
COLORS=['E7C48C','A6C9C2','E4AA94','D8D4BD','A3B9CD','D7BC7F','B5C4A0','FFF1CF',
 '203C43','A9382E','196E72','326A8C','684B39','D83833','FFD35A','507478',
 'B7C4C1','697B79','C7BAA5','A58D72','725844','516C43','6D985B','8EB471',
 'D29262','8A6040','D9C6A2','E8DFCE','466143','376E58','A8B6B2','DDD6BE']
SHOPS=['QUÁN PHỞ','HỦ TIẾU','MÌ GÕ','TẠP HÓA CÔ BA','BÁCH HÓA AN TÂM','SỬA XE TẤN PHÁT',
 'CÀ PHÊ VỈA HÈ','BÁNH MÌ SÀI GÒN','CƠM TẤM','TIỆM SỬA XE HAI LÚA','BÚN BÒ HUẾ','TẠP HÓA MINH CHÂU']
SUBS=['BÒ TÁI - NẠM - GẦU','NƯỚC LÈO THANH - SỢI DAI','NÓNG HỔI MỖI NGÀY','GẠO - SỮA - BÁNH KẸO',
 'ĐỦ ĐẦY MỖI NGÀY','VÁ VỎ - THAY NHỚT','CÀ PHÊ SỮA ĐÁ - TRÀ TẮC','GIÒN THƠM - ĐẬM VỊ VIỆT',
 'SƯỜN - BÌ - CHẢ - TRỨNG','SỬA CHỮA - BẢO DƯỠNG','ĐẬM ĐÀ HƯƠNG VỊ CỐ ĐÔ','MỞ CỬA TỪ 6:00']
def atlas():
 pixels=[]
 for y in range(128):
  for x in range(128):pixels.extend(hexcol(COLORS[(y//32)*8+x//16]))
 im=bpy.data.images.new('PV_StreetPalette',128,128);im.pixels=pixels;im.filepath_raw=OUT+'/PV_StreetPalette.png';im.file_format='PNG';im.save()
 mat=bpy.data.materials.new('PV_StreetAtlas');mat.use_nodes=True
 bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bsdf.inputs['Roughness'].default_value=.78
 tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;tex.interpolation='Closest';mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);return mat
MAT=None
def label(g,text,pos,size,col,maxwidth=6.5):
 curve=bpy.data.curves.new('Shop lettering','FONT');curve.body=text;curve.size=size;curve.align_x='CENTER';curve.align_y='CENTER';curve.resolution_u=1
 curve.font=bpy.data.fonts.load(ROOT+'/Assets/PhoViet/Fonts/BeVietnamPro-SemiBold.ttf',check_existing=True)
 obj=bpy.data.objects.new('Lettering',curve);bpy.context.collection.objects.link(obj);obj.location=B(pos);obj.rotation_euler=(math.pi/2,0,math.pi)
 bpy.context.view_layer.update()
 if obj.dimensions.x>maxwidth:obj.scale*=maxwidth/obj.dimensions.x
 deps=bpy.context.evaluated_depsgraph_get();e=obj.evaluated_get(deps);m=e.to_mesh()
 pts=[obj.matrix_world@v.co for v in m.vertices];g.add([(-p.x,p.z,-p.y) for p in pts],[tuple(f.vertices) for f in m.polygons],col,bone='Lettering',smooth=False)
 e.to_mesh_clear();bpy.data.objects.remove(obj,do_unlink=True);bpy.data.curves.remove(curve)
def pot(g,x,y,z,seed):
 g.rod((x,y,z),(x,y+.28,z),.17,24,n=8,r2=.23)
 for k in range(5):
  a=k*1.256+seed;g.ell((x+.16*math.cos(a),y+.5,z+.16*math.sin(a)),(.15,.3,.12),21+k%3,n=6,m=4)
def shop(seed):
 g=Geometry();r=random.Random(seed);height=8.4+(seed%4)*2.65;paint=seed%7;sign=[9,10,11,10,11,9,12][seed%7]
 g.box((0,height/2,0),(7.7,height,7.8),paint,bevel=.09)
 # Recessed entrance, columns, tiled plinth and metal shutter rails.
 g.box((0,.23,-.05),(7.85,.46,8),18)
 g.box((0,1.65,-3.94),(6.7,2.75,.07),8)
 for x in [-3.5,3.5]:
  g.box((x,1.65,-4.0),(.3,2.85,.23),7)
  for h in range(7):g.box((x,1.65,-4.14),( .08,2.7,.055),17)
 g.box((0,3.05,-3.99),(7.75,.22,.3),7)
 # Wall-mounted board: rear surface exactly touches z=-3.90 wall.
 g.box((0,3.86,-4.01),(7.1,1.28,.22),7,bevel=.035)
 g.box((0,3.86,-4.135),(6.94,1.1,.03),sign,bevel=.01)
 label(g,SHOPS[seed],(0,3.80,-4.158),.51,7,6.65)
 label(g,SUBS[seed],(0,3.53,-4.16),.17,7,6.45)
 # Retractable striped canvas, front valance, and visible triangular brackets.
 for i in range(12):
  x=-3.3+i*.6
  g.add([(x-.29,3.13,-4.1),(x+.29,3.13,-4.1),(x+.29,2.83,-5.35),(x-.29,2.83,-5.35)],[(0,1,2,3)],sign if i%2==0 else 7,smooth=False)
  g.box((x,2.75,-5.35),(.58,.17,.035),sign if i%2==0 else 7,bevel=.008)
 for x in [-3.2,3.2]:
  g.rod((x,2.2,-4.1),(x,2.84,-5.3),.028,17,n=6);g.rod((x,2.84,-5.3),(x,3.13,-4.1),.026,17,n=6)
 # Floors with framed glazing, shutters, balconies, rail detail and plants.
 floors=int((height-4.4)/2.65)+1
 for f in range(floors):
  y=5.35+f*2.65
  if y+1>height:continue
  g.box((0,y-1.1,-3.94),(7.9,.16,.22),7)
  for x in [-2.35,0,2.35]:
   g.box((x,y,-3.96),(1.77,1.87,.19),7)
   g.box((x,y,-4.075),(1.52,1.62,.055),15)
   g.box((x,y,-4.12),(.055,1.65,.07),7)
   g.box((x,y-.1,-4.12),(1.56,.045,.07),7)
   if (seed+f)%3==1:
    for s in [-1,1]:
     g.box((x+s*.97,y,-4.1),(.24,1.7,.1),paint)
     for slat in range(7):g.box((x+s*.97,y-.65+slat*.21,-4.17),(.26,.06,.08),17)
  if (seed+f)%2==0:
   g.box((0,y-1.0,-4.4),(6.4,.18,1.1),7)
   for rail in range(21):g.rod((-3.0+rail*.3,y-.9,-4.91),(-3.0+rail*.3,y-.12,-4.91),.021,8,n=5)
   g.rod((-3.1,y-.12,-4.91),(3.1,y-.12,-4.91),.035,8,n=6)
   for x in [-2.5,2.5]:pot(g,x,y-.9,-4.5,seed+f)
  if f==0:
   g.box((3.14,y+.92,-4.2),(.95,.5,.45),16)
   for k in range(5):g.box((2.8+k*.16,y+.92,-4.45),(.055,.35,.025),17)
 # Roof parapet, terracotta trim, pipes, solar heater/tank.
 g.box((0,height+.1,0),(8.05,.2,8.1),7)
 for x in [-3.8,3.8]:g.box((x,height+.42,0),(.2,.55,7.95),paint)
 g.box((0,height+.42,-3.9),(7.8,.55,.16),paint)
 g.rod((3.4,.4,-4.07),(3.4,height,-4.07),.05,17,n=6)
 for z in [-2.8,2.8]:g.rod((-1.1,height+.2,z),(-1.1,height+1,z),.045,16,n=6)
 g.rod((-1.1,height+.95,-1),(-1.1,height+.95,1),.57,16,n=16)
 for z in [-1.1,1.1]:g.torus((-1.1,height+.95,z),.57,.025,7,axis='x',n=16,m=4)
 if seed%3==1:
  for x in [-2.8,-1.4,0]:g.box((x,height+.45,2.5),(1.18,.13,1.6),11)
 # Open shop frontage with real function-specific silhouettes.
 if seed in [3,4,11]:
  for x in [-2.45,1.8]:
   for y in [.55,1.15,1.75]:
    g.box((x,y,-4.25),(1.8,.09,.55),16)
    for j in range(6):g.box((x-.72+j*.28,y+.22,-4.38),(.21,.34,.25),[9,10,11,14,24,7][j],bevel=.018)
  for x in [-2.8,-2.1,-1.4]:g.ell((x,.48,-4.95),(.3,.36,.25),26,n=8,m=5)
 elif seed in [5,9]:
  g.box((-2.2,.88,-4.45),(1.35,1.15,.65),13)
  for y in [.48,.75,1.02,1.28]:g.box((-2.2,y,-4.79),(1.17,.035,.025),16)
  for j in range(3):g.torus((1.6,.55+j*.18,-4.45),.41,.095,8,axis='y',n=16,m=5)
  g.box((1.65,1.85,-3.99),(2,.8,.05),17)
  for j in range(6):g.rod((.9+j*.28,1.65,-4.06),(.9+j*.28,2.05,-4.06),.035,16,n=5)
 else:
  g.box((1.8,.94,-4.36),(2.2,1.2,.75),16)
  g.box((1.8,1.63,-4.35),(2.1,.06,.72),7)
  g.rod((1.6,1.66,-4.35),(1.6,2.12,-4.35),.31,16,n=12)
  g.ell((1.6,2.13,-4.35),(.32,.07,.32),7,n=12,m=4)
  for x in [-2.2,-.5]:
   g.box((x,.91,-4.95),(1.15,.08,1.1),16)
   for sx in [-.42,.42]:
    for sz in [-.4,.4]:g.rod((x+sx,.25,-4.95+sz),(x+sx,.88,-4.95+sz),.027,17,n=5)
   g.ell((x,.99,-4.95),(.14,.055,.14),7,n=8,m=4)
   for z in [-5.65,-4.2]:g.box((x,.5,z),(.42,.43,.42),13)
 # Small ceramic number plate, wall light, potted corner plant.
 g.box((-3.2,2.4,-4.05),(.38,.28,.03),11)
 label(g,str(18+seed*7),(-3.2,2.4,-4.075),.12,7,.32)
 g.box((2.9,4.68,-4.12),(.26,.12,.38),7);pot(g,3,.24,-4.8,seed)
 if seed%2==0:
  g.rod((3.25,4.65,-4.0),(3.25,5.65,-4.9),.025,17,n=6)
  g.box((2.65,5.5,-4.9),(1.2,.8,.025),13)
  g.star((2.65,5.5,-4.921),.25,14)
 return g
def tree(seed):
 g=Geometry();r=random.Random(400+seed)
 g.box((0,.16,0),(1.45,.32,1.45),18)
 g.box((0,.33,0),(1.22,.06,1.22),20)
 for x in [-.65,.65]:g.box((x,.38,0),(.12,.15,1.5),26)
 for z in [-.65,.65]:g.box((0,.38,z),(1.45,.15,.12),26)
 g.rod((0,.32,0),(.13,3.8,0),.18,20,n=10,r2=.1)
 for k in range(9):
  a=k*2.399+seed;reach=.7+r.random()*.6;y=3.2+r.random()*1.2
  end=(math.cos(a)*reach,y,math.sin(a)*reach)
  g.rod((.08,2.2+k*.14,0),end,.08,20,n=6,r2=.028)
  g.ell((end[0],y+.38,end[2]),(.85,.7,.78),21+k%3,n=9,m=6)
  for j in range(3):
   aa=a+j*2.09;g.ell((end[0]+.42*math.cos(aa),y+.7,end[2]+.42*math.sin(aa)),(.52,.4,.48),22+j%2,n=7,m=4)
 for k in range(5):
  a=k*1.25;g.rod((.01,.55,0),(.48*math.cos(a),.34,.48*math.sin(a)),.065,20,n=6,r2=.025)
 return g
def pavement():
 g=Geometry();g.box((0,.1,0),(4.2,.2,8.4),18)
 for x in range(7):
  for z in range(14):
   g.box((-1.8+x*.6,.245,-3.9+z*.6),(.575,.075,.575),18+(x+z)%2,bevel=.008)
 for z in range(12):
  g.box((-2.0,.15,-3.85+z*.7),(.19,.32,.675),26 if z%3 else 7,bevel=.018)
 for z in [-2.9,2.9]:
  g.box((-1.72,.29,z),(.38,.025,.65),17)
  for j in range(6):g.box((-1.72,.31,z-.25+j*.1),(.32,.008,.037),8,bevel=.002)
 return g
def save_asset(name,g,offset):
 mesh=bpy.data.meshes.new(name+'_LOD0');mesh.from_pydata(g.v,[],g.f);mesh.update();mesh.materials.append(MAT)
 uv=mesh.uv_layers.new(name='StreetAtlasUV')
 for poly,(col,smooth) in zip(mesh.polygons,g.colors):
  poly.use_smooth=smooth
  for loop in poly.loop_indices:uv.data[loop].uv=((col%8+.5)/8,(col//8+.5)/4)
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free()
 root=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(root)
 ob=bpy.data.objects.new(name+'_LOD0',mesh);bpy.context.collection.objects.link(ob);ob.parent=root;objs=[ob]
 label_faces={i for i,f in enumerate(g.f) if g.bones[f[0]]=='Lettering'}
 def strip_faces(target,remove_labels):
  bm=bmesh.new();bm.from_mesh(target);bm.faces.ensure_lookup_table()
  doomed=[f for f in bm.faces if (f.index in label_faces)==remove_labels]
  bmesh.ops.delete(bm,geom=doomed,context='FACES')
  bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS');bm.to_mesh(target);bm.free()
 letters=mesh.copy();strip_faces(letters,False)
 for level,ratio in [(1,.48),(2,.18)]:
  child=ob.copy();child.data=mesh.copy();child.name=name+'_LOD'+str(level);bpy.context.collection.objects.link(child);child.parent=root
  strip_faces(child.data,True)
  bpy.context.view_layer.objects.active=child;mod=child.modifiers.new('Distance simplification','DECIMATE');mod.ratio=ratio
  bpy.ops.object.modifier_apply(modifier=mod.name)
  if len(letters.polygons):
   lettering=bpy.data.objects.new('Protected Vietnamese lettering',letters.copy());bpy.context.collection.objects.link(lettering)
   bpy.ops.object.select_all(action='DESELECT');child.select_set(True);lettering.select_set(True);bpy.context.view_layer.objects.active=child;bpy.ops.object.join()
  objs.append(child)
 bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
 for o in objs:o.select_set(True)
 bpy.context.view_layer.objects.active=ob
 bpy.ops.export_scene.fbx(filepath=OUT+'/'+name+'.fbx',use_selection=True,object_types={'EMPTY','MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,add_leaf_bones=False,use_mesh_modifiers=True,mesh_smooth_type='FACE')
 counts=[]
 for o in objs:o.data.calc_loop_triangles();counts.append(len(o.data.loop_triangles))
 info={'name':name,'lod_triangles':counts,'materials':1,'sign_front_z':-4.158 if 'Shop' in name else None,'wall_front_z':-3.9 if 'Shop' in name else None}
 with open(OUT+'/'+name+'.json','w',encoding='utf8') as f:json.dump(info,f,ensure_ascii=False,indent=2)
 root.location=offset
 for o in objs[1:]:o.hide_set(True);o.hide_render=True
 return info
def build_streets():
 global MAT
 owned=[o for o in bpy.data.objects if o.name.startswith('PV_Street')]
 for o in owned:bpy.data.objects.remove(o,do_unlink=True)
 MAT=atlas();results=[]
 for i in range(12):results.append(save_asset('PV_StreetShop'+str(i).zfill(2),shop(i),((i%4)*10,(i//4)*13,0)))
 for i in range(3):results.append(save_asset('PV_StreetTree'+str(i),tree(i),(i*5,-17,0)))
 results.append(save_asset('PV_StreetPavement',pavement(),(18,-17,0)))
 with open(OUT+'/catalogue.json','w',encoding='utf8') as f:json.dump(results,f,indent=2)
 bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Blender/PhoViet_Streets.blend')
 print(json.dumps({'success':True,'assets':len(results),'triangles':sum(r['lod_triangles'][0] for r in results),'results':results}))
