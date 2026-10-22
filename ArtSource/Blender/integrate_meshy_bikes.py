"""Offline Blender integration: original Meshy mesh + existing animated riders.
Use a separate Blender background process; does not touch the user's live Blender.
Exports into Bikes/Meshy, leaving the received FBX and previous art intact.
"""
import bpy,bmesh,json,math,random
import numpy as np
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/PhoViet/Art/Bikes/Meshy'
SOURCE=next((ROOT/'Assets/MeshyImports/Red Honda Scooter Front View_20261023_023822').glob('*.fbx'))
NAMES=['PV_HeroScooter','PV_CityScooter','PV_Underbone','PV_SportBike','PV_Racer']
CLIPS=['Idle','Ride','KickLeft','KickRight','FallLeft','FallRight','Recover']
BIKE_BONES={'Frame','Steering','WheelFront','WheelRear'}

def clear():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 for a in list(bpy.data.actions):bpy.data.actions.remove(a)

def circle(points,front,front_fit=None):
 pts=points[(points[:,0]<-.18 if front else points[:,0]>.18)&(points[:,2]<-.19)&(np.abs(points[:,1])<.08)][:,[0,2]]
 rng=np.random.default_rng(791 if front else 93);best=None;best_score=-1
 for _ in range(3500):
  p=pts[rng.choice(len(pts),3,replace=False)];a=2*(p[1:]-p[0]);b=(p[1:]**2).sum(axis=1)-(p[0]**2).sum()
  try:c=np.linalg.solve(a,b)
  except np.linalg.LinAlgError:continue
  r=np.linalg.norm(p[0]-c)
  if not(.105<r<.17 and -.205<c[1]<-.145 and (.27<abs(c[0])<.35)):continue
  # The rear engine cover is also circular. Require the rear tyre to match the
  # front tyre height/radius instead of fitting the larger, higher engine hub.
  if front_fit is not None and (abs(c[1]-front_fit[0][1])>.018 or abs(r-front_fit[1])>.018):continue
  error=np.abs(np.linalg.norm(pts-c,axis=1)-r);score=(error<.0035).sum()
  if score>best_score:best=(c,r);best_score=score
 c,r=best
 for _ in range(3):
  q=pts[np.abs(np.linalg.norm(pts-c,axis=1)-r)<.005]
  fit=np.linalg.lstsq(np.c_[2*q,np.ones(len(q))],(q*q).sum(axis=1),rcond=None)[0];c=fit[:2];r=math.sqrt(max(.001,fit[2]+c@c))
 return c,r

def source_mesh():
 clear();bpy.ops.import_scene.fbx(filepath=str(SOURCE))
 source=next(o for o in bpy.context.scene.objects if o.type=='MESH')
 pts=np.array([list(source.matrix_world@v.co) for v in source.data.vertices]);front,fr=circle(pts,True);rear,rr=circle(pts,False,(front,fr))
 scale=1.51/(rear[0]-front[0]);ground=pts[:,2].min();vertical=.34/((front[1]+rear[1])/2-ground);mid=(front[0]+rear[0])/2
 # Imported Meshy faces left along -X, Z-up. B coordinates: -gameX,-gameZ,gameY.
 game=np.c_[pts[:,1]*scale,(pts[:,2]-ground)*vertical,-(pts[:,0]-mid)*scale+.025]
 # Align both axles with the retained animation rig, correcting the small source asymmetry.
 center_y=np.interp(pts[:,0],[front[0],rear[0]],[front[1],rear[1]])
 game[:,1]=(pts[:,2]-center_y)*vertical+.34
 verts=[];faces=[];uvs=[];bones=[];lookup={};uv=source.data.uv_layers.active.data
 for poly in source.data.polygons:
  x,y,z=game[list(poly.vertices)].mean(axis=0);bone='Frame'
  for wz,name in [(.78,'WheelFront'),(-.73,'WheelRear')]:
   radius=math.hypot(y-.34,z-wz)
   # Keep the fork legs and the rear engine/muffler out of the rotating tyre/rim.
   width=.12 if name=='WheelFront' else .085
   if abs(x)<width and radius<.355 and (y<.39 or radius>.26 or abs(x)<.045):bone=name;break
  if bone=='Frame' and z>.30 and y>1.05:bone='Steering'
  if bone=='Frame' and z>.58 and y<1.05:bone='Steering'
  face=[];faceuv=[]
  for li in poly.loop_indices:
   vi=source.data.loops[li].vertex_index;key=(vi,bone)
   if key not in lookup:
    lookup[key]=len(verts);v=game[vi];verts.append((-v[0],-v[2],v[1]));bones.append(bone)
   face.append(lookup[key]);u,v=uv[li].uv;faceuv.append((u,.0625+v*.9375))
  faces.append(face);uvs.append(faceuv)
 tree=BVHTree.FromPolygons([Vector(p) for p in game],[list(p.vertices) for p in source.data.polygons])
 def socket(y,direction):
  hit=tree.ray_cast(Vector((0,y,3*direction)),Vector((0,0,-direction)),6)[0]
  return list(hit+Vector((0,0,.025*direction))) if hit else [0,y,.6*direction]
 sockets={'headlight':socket(1.25,1),'taillight':socket(.995,-1),'exhaust_tip':[.23,.40,-1.0]}
 # Rasterise actual lamp faces into the shared UV emission map (not the rider's
 # old 128px palette coordinates). Other body panels stay black at night.
 emission=np.zeros((2048,2048,4),dtype=np.float32);emission[:,:,3]=1
 source_image=bpy.data.images.load(str(SOURCE.parent/'meshy_basecolor.png'),check_existing=True)
 pixels=np.asarray(source_image.pixels[:]).reshape(source_image.size[1],source_image.size[0],4)
 lamp_faces={'head':0,'tail':0}
 for poly in source.data.polygons:
  x,y,z=game[list(poly.vertices)].mean(axis=0)
  head=abs(x)<.17 and 1.17<y<1.335 and z>.49
  tail=abs(x)<.15 and .94<y<1.045 and z<-.78
  if not(head or tail):continue
  coords=[uv[li].uv[:] for li in poly.loop_indices];u,v=np.mean(coords,axis=0)
  color=pixels[min(int(v*source_image.size[1]),source_image.size[1]-1),min(int(u*source_image.size[0]),source_image.size[0]-1),:3]
  if head and color[0]>color[1]*1.3 and color[0]>color[2]*1.3:continue
  if tail and not(color[0]>color[1]*1.1 and color[0]>color[2]*1.1):continue
  lamp_faces['head' if head else 'tail']+=1
  coords=np.array([(u*2047,(.0625+v*.9375)*2047) for u,v in coords])
  for i in range(1,len(coords)-1):
   a,b,c=coords[0],coords[i],coords[i+1];lo=np.maximum(0,np.floor(np.minimum(np.minimum(a,b),c)).astype(int));hi=np.minimum(2047,np.ceil(np.maximum(np.maximum(a,b),c)).astype(int))
   xx,yy=np.meshgrid(np.arange(lo[0],hi[0]+1),np.arange(lo[1],hi[1]+1));p=np.stack((xx,yy),axis=-1)
   def cross2(v,w):return v[...,0]*w[...,1]-v[...,1]*w[...,0]
   e1=cross2(b-a,p-a);e2=cross2(c-b,p-b);e3=cross2(a-c,p-c)
   inside=((e1>=0)&(e2>=0)&(e3>=0))|((e1<=0)&(e2<=0)&(e3<=0))
   emission[yy[inside],xx[inside],:3]=(.72,.85,1) if head else (1,.015,.008)
 img=bpy.data.images.new('Meshy_Emission',width=2048,height=2048,alpha=True);img.pixels.foreach_set(emission.ravel());img.filepath_raw=str(OUT/'Meshy_Emission.png');img.file_format='PNG';img.save()
 print('LAMP_FACES',json.dumps(lamp_faces),flush=True)
 print('Meshy normalization',json.dumps({'front':front.tolist(),'rear':rear.tolist(),'scale':scale,'verticalScale':vertical,'sockets':sockets}),flush=True)
 return verts,faces,uvs,bones,sockets

def image_material(name):
 m=bpy.data.materials.new(name+'_Atlas');m.use_nodes=True
 img=bpy.data.images.load(str(OUT/f'{name}_Palette.png'),check_existing=True)
 tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=img
 m.node_tree.links.new(tex.outputs['Color'],m.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
 m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.5
 return m

def build(name,prototype):
 clear();bpy.ops.import_scene.fbx(filepath=str(ROOT/f'Assets/PhoViet/Art/Bikes/{name}.fbx'))
 rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');rig.animation_data.action=None
 for t in list(rig.animation_data.nla_tracks):rig.animation_data.nla_tracks.remove(t)
 for pb in rig.pose.bones:pb.rotation_quaternion=(1,0,0,0);pb.location=(0,0,0);pb.scale=(1,1,1)
 rider=next(o for o in rig.children if o.type=='MESH' and o.name.endswith('_Body'))
 for o in list(rig.children):
  if o.type=='MESH' and o!=rider:bpy.data.objects.remove(o,do_unlink=True)
 bm=bmesh.new();bm.from_mesh(rider.data);layer=bm.verts.layers.deform.active
 indices={g.index for g in rider.vertex_groups if g.name in BIKE_BONES}
 delete=[v for v in bm.verts if sum(w for group,w in v[layer].items() if group in indices)>.5]
 bmesh.ops.delete(bm,geom=delete,context='VERTS');bm.to_mesh(rider.data);bm.free()
 # Retain the same animated rider, fitting only the distal forearms/hands to the
 # scooter's slightly higher grips. Legs/foot contact trajectories stay unchanged.
 forearms={g.index for g in rider.vertex_groups if g.name in {'LForearm','RForearm'}}
 for v in rider.data.vertices:
  weight=sum(g.weight for g in v.groups if g.group in forearms)
  if weight:
   game_y=v.co.z;blend=max(0,min(1,(1.38-game_y)/.17))*weight
   v.co+=Vector((0,.035,.065))*blend
 for point in rider.data.uv_layers.active.data:point.uv.y*=.0625
 mat=image_material(name);rider.data.materials.clear();rider.data.materials.append(mat)
 verts,faces,uvs,bones,sockets=prototype
 mesh=bpy.data.meshes.new(name+'_MeshySource');mesh.from_pydata(verts,[],faces);mesh.update()
 bike=bpy.data.objects.new(name+'_Meshy',mesh);bpy.context.scene.collection.objects.link(bike)
 layer=mesh.uv_layers.new(name='PaletteUV')
 for poly,coords in zip(mesh.polygons,uvs):
  poly.use_smooth=True
  for li,coord in zip(poly.loop_indices,coords):layer.data[li].uv=coord
 for bone in BIKE_BONES:
  g=bike.vertex_groups.new(name=bone);g.add([i for i,b in enumerate(bones) if b==bone],1,'REPLACE')
 mesh.materials.append(mat)
 bpy.ops.object.select_all(action='DESELECT');rider.select_set(True);bike.select_set(True);bpy.context.view_layer.objects.active=rider;bpy.ops.object.join()
 rider.name=name+'_Body';rider.data.name=name+'_Mesh';rider.data.materials.clear();rider.data.materials.append(mat)
 for poly in rider.data.polygons:poly.material_index=0
 rider.data.calc_loop_triangles();counts=[len(rider.data.loop_triangles)]
 for level,target in [(1,11000),(2,5000)]:
  o=rider.copy();o.data=rider.data.copy();o.name=name+'_LOD'+str(level);bpy.context.scene.collection.objects.link(o)
  o.modifiers.clear();d=o.modifiers.new('MobileLOD','DECIMATE');d.ratio=min(.95,target/counts[0]);d.use_collapse_triangulate=True
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=d.name)
  a=o.modifiers.new('VehicleAndRiderRig','ARMATURE');a.object=rig
  o.data.calc_loop_triangles();counts.append(len(o.data.loop_triangles))
 actions=[]
 for clip in CLIPS:
  action=next(a for a in bpy.data.actions if a.name.endswith('|'+clip));action.name=name+'_'+clip
  track=rig.animation_data.nla_tracks.new();track.name=clip;strip=track.strips.new(clip,int(action.frame_range[0]),action);actions.append(action)
 # Empty sockets are installed in Unity from metadata, so the rig remains exactly 17 bones.
 bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
 for o in rig.children:
  if o.type=='MESH':o.select_set(True)
 bpy.context.view_layer.objects.active=rig
 bpy.ops.export_scene.fbx(filepath=str(OUT/f'{name}.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,use_armature_deform_only=True,mesh_smooth_type='FACE',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=True,bake_anim_simplify_factor=.35,path_mode='RELATIVE',embed_textures=False)
 info={'name':name,'design':'meshy-scooter','source':str(SOURCE.relative_to(ROOT)),'source_triangles':15739,'bones':17,'materials':1,'clips':CLIPS,'lod_triangles':counts,'anchors':sockets,'originalRiderPreserved':True}
 (OUT/f'{name}.json').write_text(json.dumps(info,indent=2));print('EXPORTED',json.dumps(info),flush=True)
 return info

def main():
 OUT.mkdir(parents=True,exist_ok=True);prototype=source_mesh();report=[]
 for name in NAMES:report.append(build(name,prototype))
 # Keep all five exported rigs together in an independent editable source file.
 clear()
 for i,name in enumerate(NAMES):
  before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(OUT/f'{name}.fbx'))
  roots=[o for o in set(bpy.context.scene.objects)-before if o.parent is None]
  for o in roots:o.location.x+=i*1.6
 # Independent editable source, never overwrites any previous user's .blend.
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_MeshyMotorcycles.blend'))
 (ROOT/'Captures/meshy-bikes-export.json').write_text(json.dumps(report,indent=2))
if __name__=='__main__':main()
