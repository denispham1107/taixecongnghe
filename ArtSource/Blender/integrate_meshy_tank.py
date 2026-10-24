"""Xe tăng Meshy "Iron Sentinel" (24/10/2026) thành mẫu giao thông PV_MeshyTank.
Chạy Blender nền: blender -b --factory-startup -P integrate_meshy_tank.py
Giữ nguyên mọi mặt/UV/texture nguồn (không sửa file trong Assets/MeshyImports); dựng rig 15 xương,
7 clip Legacy và 3 LOD cùng chuẩn ô tô Meshy (integrate_meshy_cars.py). Xe tăng không có bánh xe
thật: hai trục "bánh" chỉ là xương ảo không gắn mặt nào để CarVisualAnimator dùng chung hợp đồng.
Mặt nạ sơn đen hoàn toàn → luôn giữ đúng một màu gốc; không có vùng đèn phát sáng.
"""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
exec(compile((ROOT/'ArtSource/Blender/pho_viet_cars.py').read_text(encoding='utf-8'),'pho_viet_cars.py','exec'))
ROOT=Path(__file__).resolve().parents[2]
import numpy as np,json,shutil
from mathutils.bvhtree import BVHTree
OUT=str(ROOT/'Assets/PhoViet/Art/Cars/Meshy');Path(OUT).mkdir(parents=True,exist_ok=True)
NAME='PV_MeshyTank';FOLDER='Iron Sentinel_20261024_170520'
LENGTH=7.0   # tổng chiều dài kể cả nòng pháo; bề rộng tỉ lệ theo nguồn (~2,5 m, vừa một làn 3,1 m)

def clear():
 for o in list(bpy.context.scene.objects):bpy.data.objects.remove(o,do_unlink=True)
 for a in list(bpy.data.actions):bpy.data.actions.remove(a)

def black_png(path,size):
 img=bpy.data.images.new(Path(path).stem,width=size[0],height=size[1],alpha=False)
 img.pixels.foreach_set(np.tile(np.array([0,0,0,1],dtype=np.float32),size[0]*size[1]))
 img.filepath_raw=path;img.file_format='PNG';img.save();bpy.data.images.remove(img)

def build():
 clear();folder=ROOT/'Assets/MeshyImports'/FOLDER;sourcepath=next(folder.glob('*.fbx'))
 bpy.ops.import_scene.fbx(filepath=str(sourcepath));source=next(o for o in bpy.context.scene.objects if o.type=='MESH')
 mesh=source.data;raw=np.array([tuple(source.matrix_world@v.co) for v in mesh.vertices]);lo=raw.min(axis=0);hi=raw.max(axis=0)
 scale=LENGTH/(hi[0]-lo[0]);mid=(hi[0]+lo[0])/2;ymid=(hi[1]+lo[1])/2
 # Nòng pháo nguồn hướng −X → +Z trong hệ tác giả (hướng chạy tới), cùng quy ước ô tô Meshy.
 points=np.c_[(raw[:,1]-ymid)*scale,(raw[:,2]-lo[2])*scale,-(raw[:,0]-mid)*scale]
 size=(points.max(axis=0)-points.min(axis=0));width,height=float(size[0]),float(size[1])
 # Thân xe tăng (không gồm nòng) để đặt đèn/đuôi: điểm thấp hơn 52% chiều cao.
 hull=points[points[:,1]<height*.52];front=float(hull[:,2].max());rear=float(hull[:,2].min())
 axles=[dict(x=width*.36,y=.45,z=front-1.3,radius=.45),dict(x=width*.36,y=.45,z=rear+1.3,radius=.45)]
 verts=[];faces=[];uvs=[];groups=[];normals=[];lookup={};uv=mesh.uv_layers.active.data
 for poly in mesh.polygons:
  pts=points[list(poly.vertices)];y=pts[:,1].mean()
  bone='Frame' if y<height*.22 else 'Body'   # xích/gầm cố định, thân trên nhún nhẹ theo clip
  face=[];fuv=[]
  for li in poly.loop_indices:
   vi=mesh.loops[li].vertex_index;key=(vi,bone)
   if key not in lookup:lookup[key]=len(verts);verts.append(tuple(B(points[vi])));groups.append(bone)
   face.append(lookup[key]);fuv.append(tuple(uv[li].uv))
   n=source.matrix_world.to_3x3()@mesh.corner_normals[li].vector;normals.append((-n.y,n.x,n.z))
  faces.append(face);uvs.append(fuv)
 data=bpy.data.meshes.new(NAME+'_Mesh');data.from_pydata(verts,[],faces);data.update();layer=data.uv_layers.new(name='UVMap')
 for poly,coords in zip(data.polygons,uvs):
  poly.use_smooth=True
  for li,coord in zip(poly.loop_indices,coords):layer.data[li].uv=coord
 data.normals_split_custom_set(normals)
 bpy.data.objects.remove(source,do_unlink=True)
 rig=skeleton(NAME,False);bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
 for j,ax in enumerate(axles):
  for side,s in [('L',-1),('R',1)]:
   suffix=('F' if j==0 else 'R')+side
   for bn,parent in [('Steer'+suffix,'Frame'),('Wheel'+suffix,'Steer'+suffix)]:
    b=rig.data.edit_bones[bn];b.head=B((s*ax['x'],ax['y'],ax['z']));b.tail=b.head+Vector((0,0,.12));b.parent=rig.data.edit_bones[parent]
 for bn,z in [('BumperFront',front-.1),('BumperRear',rear+.1)]:
  b=rig.data.edit_bones[bn];b.head=B((0,.7,z));b.tail=b.head+Vector((0,0,.12))
 bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
 obj=bpy.data.objects.new(NAME+'_Body',data);bpy.context.collection.objects.link(obj);obj.parent=rig
 for bn in rig.data.bones:obj.vertex_groups.new(name=bn.name)
 for idx,bn in enumerate(groups):obj.vertex_groups[bn].add([idx],1,'REPLACE')
 arm=obj.modifiers.new('VehicleAndRiderRig','ARMATURE');arm.object=rig
 data.materials.append(bpy.data.materials.new(NAME+'_Atlas'))
 data.calc_loop_triangles()
 info=dict(name=NAME,design='received-meshy-tank',source=str(sourcepath.relative_to(ROOT)).replace('\\','/'),sourceUnchanged=True,
  triangles=len(data.loop_triangles),bones=len(rig.data.bones),materials=1,clips=CLIPS,wheel_radius=.45,axles=axles,size=size.tolist(),
  armored=True,speedScale=.4,fixedPaint=True,hullFront=front,hullRear=rear,wheelBinding='virtual-axles-no-geometry')
 tree=BVHTree.FromPolygons([Vector(p) for p in points],[list(p.vertices) for p in mesh.polygons]);profile=[]
 for z in np.linspace(-LENGTH/2+.08,LENGTH/2-.08,49):
  heights=[]
  for x in [0,-width*.12,width*.12]:
   hit=tree.ray_cast(Vector((x,height+1,z)),Vector((0,-1,0)),height+2)[0]
   if hit is not None:heights.append(hit.y)
  profile.append([float(z),float(max(heights)) if heights else 0.0])
 info['roofProfile']=profile
 info['headlight']=[0,height*.36,front+.01];info['taillight']=[width*.34,height*.40,rear-.01]
 (Path(OUT)/(NAME+'_Roof.json')).write_text(json.dumps({'samples':[{'x':z,'y':y} for z,y in profile]}),encoding='utf-8')
 animate(rig);MODELS[NAME]=(rig,obj,info);make_lods(NAME);export_model(NAME)
 # Maps nguồn 2K giữ nguyên; không đổi sơn, không phát sáng.
 for old,new in [('meshy_basecolor','Palette'),('meshy_normal','Normal'),('meshy_metallic_smoothness','Surface')]:shutil.copyfile(folder/(old+'.png'),Path(OUT)/(NAME+'_'+new+'.png'))
 tex=bpy.data.images.load(str(folder/'meshy_basecolor.png'));wh=tuple(tex.size);bpy.data.images.remove(tex)
 black_png(OUT+'/'+NAME+'_PaintMask.png',wh);black_png(OUT+'/'+NAME+'_Emission.png',wh)
 return info

if __name__=='__main__':
 report=build()
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_MeshyTank.blend'))
 (ROOT/'Captures/meshy-tank-export.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
 print('TANK',json.dumps({k:report[k] for k in ('triangles','lod_triangles','size','hullFront','hullRear')}))
