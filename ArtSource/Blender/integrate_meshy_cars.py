"""Build received textured cars into isolated game-ready rigs; never edit source FBX."""
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2]
exec(compile((ROOT/'ArtSource/Blender/pho_viet_cars.py').read_text(encoding='utf-8'),'pho_viet_cars.py','exec'))
ROOT=Path(__file__).resolve().parents[2]
import numpy as np
from mathutils.bvhtree import BVHTree
OUT=str(ROOT/'Assets/PhoViet/Art/Cars/Meshy');Path(OUT).mkdir(parents=True,exist_ok=True)
SOURCES=[('PV_MeshySport','Meshy_Model_20261024_022256',4.45,[(-.72,-.35),(.32,.73)],(.09,.19)),
 ('PV_MeshyPickup','Meshy_Model_20261024_022318',4.2,[(-.72,-.25),(.25,.75)],(.12,.25)),
 ('PV_MeshyCovered','Covered cargo truck_20261024_022230',6.4,[(-.40,-.18),(.18,.40)],(.035,.085)),
 ('PV_MeshyContainer','Meshy_Model_20261024_022335',12.8,[(-.90,-.65),(-.30,-.01),(.48,.72),(.70,.9)],(.025,.07))]

def clear():
 # Hidden LODs cannot be selected by select_all; remove every isolated-process object.
 for o in list(bpy.context.scene.objects):bpy.data.objects.remove(o,do_unlink=True)
 for a in list(bpy.data.actions):bpy.data.actions.remove(a)

def axle_fit(points,interval,radii):
 # The lower sidewall is separated from the bodywork; require centres near ground.
 ground=points[:,2].min();side=np.max(np.abs(points[:,1]))
 q=points[(points[:,0]>interval[0])&(points[:,0]<interval[1])&(points[:,2]<ground+radii[1]*1.8)&(np.abs(points[:,1])>side*.50)][:,[0,2]]
 rng=np.random.default_rng(839);best=None;score=-1
 for _ in range(4000):
  p=q[rng.choice(len(q),3,replace=False)];a=2*(p[1:]-p[0]);b=(p[1:]**2).sum(axis=1)-(p[0]**2).sum()
  try:c=np.linalg.solve(a,b)
  except np.linalg.LinAlgError:continue
  r=np.linalg.norm(p[0]-c)
  if not(radii[0]<r<radii[1] and interval[0]<c[0]<interval[1] and abs(c[1]-ground-r)<r*.2):continue
  errors=np.abs(np.linalg.norm(q-c,axis=1)-r);n=(errors<r*.045).sum()
  if n>score:best=(c,r);score=n
 if best is None:raise RuntimeError('Cannot fit axle '+str(interval))
 return best

def raster(target,coords,color):
 size=target.shape[0];coords=np.array(coords)*(size-1)
 for i in range(1,len(coords)-1):
  a,b,c=coords[0],coords[i],coords[i+1];lo=np.maximum(0,np.floor(np.minimum(np.minimum(a,b),c)).astype(int));hi=np.minimum(size-1,np.ceil(np.maximum(np.maximum(a,b),c)).astype(int))
  if np.any(hi<lo):continue
  xx,yy=np.meshgrid(np.arange(lo[0],hi[0]+1),np.arange(lo[1],hi[1]+1));p=np.stack((xx,yy),axis=-1)
  def cross(v,w):return v[...,0]*w[...,1]-v[...,1]*w[...,0]
  e1,e2,e3=cross(b-a,p-a),cross(c-b,p-b),cross(a-c,p-c);inside=((e1>=0)&(e2>=0)&(e3>=0))|((e1<=0)&(e2<=0)&(e3<=0))
  target[yy[inside],xx[inside]]=color

def image_save(name,data):
 h,w=data.shape[:2];img=bpy.data.images.new(name,width=w,height=h,alpha=True);img.pixels.foreach_set(data.astype(np.float32).ravel());img.filepath_raw=OUT+'/'+name+'.png';img.file_format='PNG';img.save();return img

def build(config):
 name,folder,length,intervals,radii=config;clear();folder=ROOT/'Assets/MeshyImports'/folder
 sourcepath=next(folder.glob('*.fbx'));bpy.ops.import_scene.fbx(filepath=str(sourcepath));source=next(o for o in bpy.context.scene.objects if o.type=='MESH')
 mesh=source.data;raw=np.array([tuple(source.matrix_world@v.co) for v in mesh.vertices]);lo=raw.min(axis=0);hi=raw.max(axis=0);scale=length/(hi[0]-lo[0]);mid=(hi[0]+lo[0])/2
 points=np.c_[raw[:,1]*scale,(raw[:,2]-lo[2])*scale,-(raw[:,0]-mid)*scale]
 fits=[axle_fit(raw,i,radii) for i in intervals];print('AXLES',name,[(c.tolist(),r) for c,r in fits],flush=True)
 axles=[]
 for j,(c,r) in enumerate(fits):
  gy=(c[1]-lo[2])*scale;gz=-(c[0]-mid)*scale;radius=r*scale
  section=points[(np.abs(points[:,2]-gz)<radius*.7)&(points[:,1]<gy+radius*.5)]
  width=float(np.percentile(np.abs(section[:,0]),92))-.035
  axles.append(dict(y=gy,z=gz,radius=radius,x=width))
 # Keep every original source face/UV. Split vertices only across rigid bone boundaries.
 verts=[];faces=[];uvs=[];groups=[];normals=[];lookup={};uv=mesh.uv_layers.active.data
 for poly in mesh.polygons:
  pts=points[list(poly.vertices)];x,y,z=pts.mean(axis=0);bone='Body'
  if name in ('PV_MeshyCovered','PV_MeshyContainer') and z<length*.18 and y>points[:,1].max()*.40:bone='Cargo'
  if y<.35:bone='Frame'
  if z>length*.5-.20 and y<1.2:bone='BumperFront'
  if z<-length*.5+.20 and y<1.2:bone='BumperRear'
  for j,ax in enumerate(axles):
   # Fused AI tyre/fender shell stays rigid; rotate the inner rim only.
   radial=np.linalg.norm(pts[:,1:]-[ax['y'],ax['z']],axis=1)
   if max(radial)<ax['radius']*.66 and min(np.abs(pts[:,0]))>ax['x']*.7:
    bone='Wheel'+('F' if j==0 else 'R' if j==1 else 'R'+str(j))+('R' if x>0 else 'L');break
  face=[];fuv=[]
  for li in poly.loop_indices:
   vi=mesh.loops[li].vertex_index;key=(vi,bone)
   if key not in lookup:lookup[key]=len(verts);p=points[vi];verts.append(tuple(B(p)));groups.append(bone)
   face.append(lookup[key]);fuv.append(tuple(uv[li].uv))
   n=source.matrix_world.to_3x3()@mesh.corner_normals[li].vector;normals.append((-n.y,n.x,n.z))
  faces.append(face);uvs.append(fuv)
 data=bpy.data.meshes.new(name+'_Mesh');data.from_pydata(verts,[],faces);data.update();layer=data.uv_layers.new(name='UVMap')
 for poly,coords in zip(data.polygons,uvs):
  poly.use_smooth=True
  for li,coord in zip(poly.loop_indices,coords):layer.data[li].uv=coord
 data.normals_split_custom_set(normals)
 bpy.data.objects.remove(source,do_unlink=True)
 rig=skeleton(name,name in ('PV_MeshyCovered','PV_MeshyContainer'));bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
 for j,ax in enumerate(axles):
  for side,s in [('L',-1),('R',1)]:
   suffix=('F' if j==0 else 'R' if j==1 else 'R'+str(j))+side;sn='Steer'+suffix;wn='Wheel'+suffix
   for bn,parent in [(sn,'Frame'),(wn,sn)]:
    b=rig.data.edit_bones.get(bn) or rig.data.edit_bones.new(bn);b.head=B((s*ax['x'],ax['y'],ax['z']));b.tail=b.head+Vector((0,0,.12));b.parent=rig.data.edit_bones[parent]
 for bn,z in [('BumperFront',length/2-.1),('BumperRear',-length/2+.1)]:
  b=rig.data.edit_bones[bn];b.head=B((0,.7,z));b.tail=b.head+Vector((0,0,.12))
 bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
 obj=bpy.data.objects.new(name+'_Body',data);bpy.context.collection.objects.link(obj);obj.parent=rig
 for bn in rig.data.bones:obj.vertex_groups.new(name=bn.name)
 for idx,bn in enumerate(groups):obj.vertex_groups[bn].add([idx],1,'REPLACE')
 arm=obj.modifiers.new('VehicleAndRiderRig','ARMATURE');arm.object=rig
 mat=bpy.data.materials.new(name+'_Atlas');data.materials.append(mat)
 data.calc_loop_triangles();info=dict(name=name,design='received-meshy-car',source=str(sourcepath.relative_to(ROOT)).replace('\\','/'),sourceUnchanged=True,triangles=len(data.loop_triangles),bones=len(rig.data.bones),materials=1,clips=CLIPS,wheel_radius=float(np.mean([a['radius'] for a in axles])),axles=axles,size=(points.max(axis=0)-points.min(axis=0)).tolist(),wheelBinding='stable-rim-only-v1')
 # Sample triangle surfaces, not vertex bins: container roof panels have large
 # triangles with no interior vertices, so vertex bins invent holes in flat roofs.
 tree=BVHTree.FromPolygons([Vector(p) for p in points],[list(p.vertices) for p in mesh.polygons])
 profile=[]
 for z in np.linspace(-length/2+.08,length/2-.08,49):
  heights=[]
  for x in [0,-info['size'][0]*.12,info['size'][0]*.12]:
   hit=tree.ray_cast(Vector((x,info['size'][1]+1,z)),Vector((0,-1,0)),info['size'][1]+2)[0]
   if hit is not None:heights.append(hit.y)
  y=float(max(heights)) if heights else 0
  profile.append([float(z),y])
 info['roofProfile']=profile
 info['headlight']=[0,float(info['size'][1]*(.47 if name=='PV_MeshyPickup' else .34 if name=='PV_MeshySport' else .27)),length/2+.01];info['taillight']=[float(info['size'][0]*.34),float(info['size'][1]*(.43 if name=='PV_MeshyPickup' else .37 if name=='PV_MeshySport' else .18)),-length/2-.01]
 (Path(OUT)/(name+'_Roof.json')).write_text(json.dumps({'samples':[{'x':z,'y':y} for z,y in profile]}),encoding='utf-8')
 # Export one material and three LODs. Source 2K PBR maps are retained unchanged.
 animate(rig);MODELS[name]=(rig,obj,info);make_lods(name);export_model(name)
 # Record geometric UV regions for selective paint/emission mask construction outside Blender.
 regions=[]
 for poly,coords in zip(data.polygons,uvs):
  p=np.array([(-data.vertices[i].co.x,data.vertices[i].co.z,-data.vertices[i].co.y) for i in poly.vertices]);regions.append(dict(uv=coords,vertices=p.tolist(),center=p.mean(axis=0).tolist(),normal=list(poly.normal)))
 (ROOT/f'.tools/{name}-uvregions.json').write_text(json.dumps(regions),encoding='utf-8')
 return info

if __name__=='__main__':
 reports=[]
 for config in SOURCES:reports.append(build(config))
 clear()
 for i,config in enumerate(SOURCES):
  before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=OUT+'/'+config[0]+'.fbx')
  for o in set(bpy.context.scene.objects)-before:
   if o.parent is None:o.location.x=i*4
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_MeshyCars.blend'))
 (ROOT/'Captures/meshy-cars-export.json').write_text(json.dumps(reports,indent=2),encoding='utf-8')
