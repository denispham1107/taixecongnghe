"""Fit the received Meshy person to the existing motorcycle/animation contract.

Run in an isolated Blender background process with -- --phase prepare, then run
prepare_meshy_rider_atlas.py with Python/Pillow, then -- --phase build.
Never edits the received source or the user's previous Blender projects.
"""
import bpy,bmesh,numpy as np,json,math,sys
from pathlib import Path
from mathutils import Vector,Matrix

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Assets/MeshyImports/Golden Star Stance_20261023_150430/Meshy_AI_Golden_Star_Stance_biped_Character_output.fbx'
BIKES=ROOT/'Assets/PhoViet/Art/Bikes/Meshy'
OUT=ROOT/'Assets/PhoViet/Art/Bikes/MeshyRiders'
MASK=ROOT/'ArtSource/Blender/MeshyRider_ShirtMask.png'
NAMES=['PV_HeroScooter','PV_CityScooter','PV_Underbone','PV_SportBike','PV_Racer']
CLIPS=['Idle','Ride','KickLeft','KickRight','FallLeft','FallRight','Recover']
BIKE_BONES={'Frame','Steering','WheelFront','WheelRear'}
FINGER_ARC_SCALE=.80
PARENTS={'Torso':'Pelvis','Head':'Torso','LUpperArm':'Torso','RUpperArm':'Torso','LForearm':'LUpperArm','RForearm':'RUpperArm','LThigh':'Pelvis','RThigh':'Pelvis','LShin':'LThigh','RShin':'RThigh','LFoot':'LShin','RFoot':'RShin'}
CHILDREN={'Pelvis':'Torso','Torso':'Head','LUpperArm':'LForearm','RUpperArm':'RForearm','LThigh':'LShin','RThigh':'RShin','LShin':'LFoot','RShin':'RFoot'}

def clear():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 for a in list(bpy.data.actions):bpy.data.actions.remove(a)

def B(p):return Vector((-p[0],-p[2],p[1]))
def G(p):return Vector((-p[0],p[2],-p[1]))
def smooth(v):v=max(0,min(1,v));return v*v*(3-2*v)

def source():
 clear();bpy.ops.import_scene.fbx(filepath=str(SOURCE))
 obj=next(o for o in bpy.context.scene.objects if o.type=='MESH')
 raw=np.array([list(obj.matrix_world@v.co) for v in obj.data.vertices])
 scale=1.75/(raw[:,2].max()-raw[:,2].min())
 # Meshy faces Blender -Y. This is the same left-handed game conversion as the bike rig.
 points=np.c_[-raw[:,0]*scale,(raw[:,2]-raw[:,2].min())*scale,-raw[:,1]*scale]
 global SOURCE_WEIGHTS,SOURCE_SEGMENTS,SOURCE_DETAIL
 rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
 def joint(n):
  q=rig.matrix_world@rig.data.bones['smartrig:'+n].head_local
  return Vector((-q.x*scale,(q.z-raw[:,2].min())*scale,-q.y*scale))
 SOURCE_SEGMENTS={'Pelvis':(joint('Hips'),joint('Spine')),'Torso':(joint('Spine'),joint('Neck')),'Head':(joint('Head'),joint('HeadTop_End'))}
 def semantic(n):
  n=n.split(':')[-1]
  for side in ['Left','Right']:
   if n.startswith(side):
    tail=n[len(side):];prefix=side[0]
    if tail=='Shoulder':return 'Torso'
    if tail=='Arm':return prefix+'UpperArm'
    if tail=='ForeArm' or tail.startswith('Hand'):return prefix+'Forearm'
    if tail=='UpLeg':return prefix+'Thigh'
    if tail=='Leg':return prefix+'Shin'
    if tail.startswith(('Foot','Toe')):return prefix+'Foot'
  if n=='Hips':return 'Pelvis'
  if n.startswith('Spine'):return 'Torso'
  if n=='Neck':return 'Torso'
  if n in ['Head','HeadTop_End','headfront']:return 'Head'
  return 'LForearm' if joint('LeftHand').x<0 and n.startswith('Bone_') else 'Torso'
 for side,prefix in [('Left','L'),('Right','R')]:
  for name,a,b in [('UpperArm','Arm','ForeArm'),('Forearm','ForeArm','Hand'),('Thigh','UpLeg','Leg'),('Shin','Leg','Foot'),('Foot','Foot','ToeBase')]:SOURCE_SEGMENTS[prefix+name]=(joint(side+a),joint(side+b))
 SOURCE_WEIGHTS=[];SOURCE_DETAIL=[]
 for v in obj.data.vertices:
  w={}
  SOURCE_DETAIL.append(obj.vertex_groups[max(v.groups,key=lambda g:g.weight).group].name if v.groups else '')
  for g in v.groups:
   n=semantic(obj.vertex_groups[g.group].name);w[n]=w.get(n,0)+g.weight
  top=sorted(w.items(),key=lambda t:-t[1])[:2];total=sum(v for _,v in top)
  SOURCE_WEIGHTS.append({n:v/total for n,v in top} if total else {'Torso':1})
 faces=[list(p.vertices) for p in obj.data.polygons]
 uv=[[tuple(obj.data.uv_layers.active.data[i].uv) for i in p.loop_indices] for p in obj.data.polygons]
 obj.data.calc_loop_triangles();triangles=len(obj.data.loop_triangles)
 image=bpy.data.images.load(str(SOURCE.parent/'meshy_basecolor.png'),check_existing=True)
 pixels=np.asarray(image.pixels[:]).reshape(image.size[1],image.size[0],4)
 colours=np.zeros((len(points),3));samples=np.zeros(len(points))
 for ids,coords in zip(faces,uv):
  for i,(u,v) in zip(ids,coords):
   colours[i]+=pixels[min(image.size[1]-1,int(v*image.size[1])),min(image.size[0]-1,int(u*image.size[0])),:3];samples[i]+=1
 colours/=np.maximum(1,samples[:,None])
 return points,faces,uv,triangles,colours

def prepare():
 points,faces,uv,triangles,colours=source();OUT.mkdir(parents=True,exist_ok=True)
 mask=np.zeros((2048,2048,4),dtype=np.float32);mask[:,:,3]=1
 for ids,coords in zip(faces,uv):
  shirt_bones={'Torso','Pelvis','LUpperArm','RUpperArm'}
  if not any(sum(w for n,w in SOURCE_WEIGHTS[i].items() if n in shirt_bones)>.15 for i in ids):continue
  coords=np.array(coords)*2047
  for i in range(1,len(coords)-1):
   a,b,c=coords[0],coords[i],coords[i+1];lo=np.maximum(0,np.floor(np.minimum(np.minimum(a,b),c)).astype(int));hi=np.minimum(2047,np.ceil(np.maximum(np.maximum(a,b),c)).astype(int))
   xx,yy=np.meshgrid(np.arange(lo[0],hi[0]+1),np.arange(lo[1],hi[1]+1));p=np.stack((xx,yy),axis=-1)
   def cross(v,w):return v[...,0]*w[...,1]-v[...,1]*w[...,0]
   a1=cross(b-a,p-a);a2=cross(c-b,p-b);a3=cross(a-c,p-c)
   inside=((a1>=0)&(a2>=0)&(a3>=0))|((a1<=0)&(a2<=0)&(a3<=0))
   mask[yy[inside],xx[inside],:3]=1
 img=bpy.data.images.new('MeshyRider_ShirtMask',width=2048,height=2048,alpha=True);img.pixels.foreach_set(mask.ravel());img.filepath_raw=str(MASK);img.file_format='PNG';img.save()
 (OUT/'source.json').write_text(json.dumps({'sourceRider':str(SOURCE.relative_to(ROOT)).replace('\\','/'),'sourceVertices':len(points),'sourceTriangles':triangles,'normalisedStandingHeight':1.75,'helmet':False,'policeUnchanged':True,'atlasSize':2048,'bikeUvY':[.5,1],'riderUvY':[0,.5]},indent=2))
 print('SOURCE',json.dumps({'vertices':len(points),'triangles':triangles,'min':points.min(axis=0).tolist(),'max':points.max(axis=0).tolist()}),flush=True)

def standing_segments():
 return {n:(a,b,.05,.04) for n,(a,b) in SOURCE_SEGMENTS.items()}

def basis(a,b):
 y=(b-a).normalized();forward=Vector((0,0,1));x=y.cross(forward).normalized();z=x.cross(y).normalized()
 return Matrix((x,y,z)).transposed()

def targets(rig):
 heads={b.name:G(b.head_local) for b in rig.data.bones};segments={}
 for name in standing_segments():
  if name=='Pelvis':a=heads['Pelvis']-Vector((0,.075,0));b=heads['Torso']
  elif name=='Torso':a=heads['Torso'];b=Vector((0,1.415,-.115))
  elif name=='Head':a=heads['Head']-Vector((0,.065,0));b=a+Vector((0,.235,.005))
  elif name.endswith('UpperArm'):
   s=-1 if name[0]=='L' else 1;a=Vector((s*.205,1.365,-.10));b=Vector((s*.305,1.195,.055))
  elif name.endswith('Forearm'):
   s=-1 if name[0]=='L' else 1;a=Vector((s*.305,1.195,.055));b=Vector((s*.395,1.261,.23))
  elif name.endswith('Foot'):
   a=heads[name];b=a+Vector((0,-.035,.205))
  else:a=heads[name];b=heads[CHILDREN[name]]
  segments[name]=(a,b)
 return segments

def bind_person(rig,prototype):
 points,faces,uv,triangles,colours=prototype;rest=standing_segments();target=targets(rig)
 matrices={};weights=[];posed=[];counts={n:0 for n in rest}
 for n,(a,b,r,bw) in rest.items():
  ta,tb=target[n];length=(b-a).length;stretch=(tb-ta).length/length
  matrices[n]=(ta,basis(ta,tb)@Matrix.Diagonal((1,stretch,1))@basis(a,b).transposed(),a)
 for index,p0 in enumerate(points):
  p=Vector(p0);w=SOURCE_WEIGHTS[index];position=Vector((0,0,0))
  for k,value in w.items():
   ta,m,a=matrices[k];position+=(ta+m@(p-a))*value
  n=max(w,key=w.get);counts[n]+=1
  if n.endswith('Forearm'):
   sign=-1 if n[0]=='L' else 1;wrist=rest[n][1]
   t=max(0,min(.225,(p.x-wrist.x)*sign));blend=smooth(t/.025)
   if t>0:
    centre_y,centre_z,radius=1.237,.315,.033
    width=-sign*(p.z-wrist.z)*.9
    thickness=max(-.007,min(.007,(p.y-wrist.y)*.45))
    if 'Thumb' in SOURCE_DETAIL[index]:
     u=max(0,min(1,(t-.025)/.10));angle=-math.pi*.5-u*math.pi*.80*FINGER_ARC_SCALE
     radial=radius+thickness
     y=centre_y+radial*math.cos(angle);z=centre_z+radial*math.sin(angle)
    elif t<.08:
     u=t/.08;y=1.261+.009*smooth(u)+thickness;z=.230+.085*u
    else:
     # Roll every distal phalanx around the rubber, returning the fingertips
     # behind/under the grip rather than leaving them dangling by the brake.
     angle=min(1,(t-.08)/.12)*math.pi*1.65*FINGER_ARC_SCALE
     radial=radius+thickness
     y=centre_y+radial*math.cos(angle);z=centre_z+radial*math.sin(angle)
    hand=Vector((sign*.395+width,y,z))
    position=position.lerp(hand,blend)
  posed.append(B(position));weights.append(w)
 kept=faces;kept_uv=uv
 mesh=bpy.data.meshes.new('Meshy_Rider_Mesh');mesh.from_pydata(posed,[],kept);mesh.update()
 person=bpy.data.objects.new('Meshy_Rider',mesh);bpy.context.scene.collection.objects.link(person)
 layer=mesh.uv_layers.new(name='PaletteUV')
 for polygon,coords in zip(mesh.polygons,kept_uv):
  polygon.use_smooth=True
  for li,(u,v) in zip(polygon.loop_indices,coords):layer.data[li].uv=(u,v*.5)
 for n in rest:
  group=person.vertex_groups.new(name=n)
  for i,w in enumerate(weights):
   if n in w:group.add([i],w[n],'REPLACE')
 return person,counts

def material(name):
 m=bpy.data.materials.new(name+'_Atlas');m.use_nodes=True;nodes=m.node_tree.nodes;links=m.node_tree.links;bsdf=nodes.get('Principled BSDF')
 tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(OUT/f'{name}_Palette.png'),check_existing=True);links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);bsdf.inputs['Roughness'].default_value=.65
 return m

def build(name,prototype):
 clear();bpy.ops.import_scene.fbx(filepath=str(BIKES/'PV_HeroScooter.fbx'))
 rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');rig.animation_data.action=None
 for track in list(rig.animation_data.nla_tracks):rig.animation_data.nla_tracks.remove(track)
 for pb in rig.pose.bones:pb.rotation_quaternion=(1,0,0,0);pb.location=(0,0,0);pb.scale=(1,1,1)
 bike=next(o for o in rig.children if o.type=='MESH' and o.name.endswith('_Body'))
 for o in list(rig.children):
  if o.type=='MESH' and o!=bike:bpy.data.objects.remove(o,do_unlink=True)
 bm=bmesh.new();bm.from_mesh(bike.data);deform=bm.verts.layers.deform.active;indices={g.index for g in bike.vertex_groups if g.name in BIKE_BONES}
 remove=[v for v in bm.verts if sum(w for i,w in v[deform].items() if i in indices)<.5]
 bmesh.ops.delete(bm,geom=remove,context='VERTS');bm.to_mesh(bike.data);bm.free()
 for point in bike.data.uv_layers.active.data:point.uv.y=.5+(point.uv.y-.0625)*(.5/.9375)
 bike.data.calc_loop_triangles();bike_triangles=len(bike.data.loop_triangles)
 person,counts=bind_person(rig,prototype);mat=material(name)
 bike.data.materials.clear();bike.data.materials.append(mat);person.data.materials.append(mat)
 bpy.ops.object.select_all(action='DESELECT');bike.select_set(True);person.select_set(True);bpy.context.view_layer.objects.active=bike;bpy.ops.object.join()
 bike.name=name+'_Body';bike.data.name=name+'_Mesh';bike.data.materials.clear();bike.data.materials.append(mat)
 for p in bike.data.polygons:p.material_index=0
 rig.name=name+'_Rig';rig.data.name=name+'_Skeleton';bike.data.calc_loop_triangles();lods=[len(bike.data.loop_triangles)]
 for level,budget in [(1,11000),(2,5000)]:
  o=bike.copy();o.data=bike.data.copy();o.name=name+'_LOD'+str(level);bpy.context.scene.collection.objects.link(o);o.modifiers.clear()
  decimate=o.modifiers.new('MobileLOD','DECIMATE');decimate.ratio=budget/lods[0];decimate.use_collapse_triangulate=True
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=decimate.name)
  arm=o.modifiers.new('RiderAndMotorcycle','ARMATURE');arm.object=rig;o.data.calc_loop_triangles();lods.append(len(o.data.loop_triangles))
 for clip in CLIPS:
  action=next(a for a in bpy.data.actions if a.name.endswith('|'+clip));action.name=name+'_'+clip
  track=rig.animation_data.nla_tracks.new();track.name=clip;track.strips.new(clip,int(action.frame_range[0]),action)
 bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
 for o in rig.children:
  if o.type=='MESH':o.select_set(True)
 bpy.context.view_layer.objects.active=rig
 bpy.ops.export_scene.fbx(filepath=str(OUT/f'{name}.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,use_armature_deform_only=True,mesh_smooth_type='FACE',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=True,bake_anim_simplify_factor=.35,path_mode='RELATIVE',embed_textures=False)
 old=json.loads((BIKES/f'{name}.json').read_text());info=dict(old)
 info.update(riderDesign='meshy-golden-star-rigged',sourceSkinWeights=True,sourceRider=str(SOURCE.relative_to(ROOT)).replace('\\','/'),sourceRiderTriangles=prototype[3],sourceRiderVertices=len(prototype[0]),bikeTriangles=bike_triangles,originalRiderPreserved=False,originalMotorcyclePreserved=True,helmet=False,shirtStar=name=='PV_HeroScooter',ridingPose='relaxed-shoulders-closed-grip-v4',fingerArcScale=FINGER_ARC_SCALE,shoulderHeight=1.365,gripCentre=[.395,1.237,.315],lod_triangles=lods,binding=counts,atlasLayout={'bike':[.5,1],'rider':[0,.5]})
 (OUT/f'{name}.json').write_text(json.dumps(info,indent=2));print('EXPORTED',json.dumps(info),flush=True)
 return info

def main():
 phase=sys.argv[sys.argv.index('--phase')+1] if '--phase' in sys.argv else 'build'
 if phase=='prepare':prepare();return
 OUT.mkdir(parents=True,exist_ok=True);prototype=source();report=[build(name,prototype) for name in NAMES]
 clear()
 for i,name in enumerate(NAMES):
  before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(OUT/f'{name}.fbx'))
  for o in set(bpy.context.scene.objects)-before:
   if o.parent is None:o.location.x+=i*1.6
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_MeshyRiders.blend'))
 (ROOT/'Captures/meshy-riders-export.json').write_text(json.dumps(report,indent=2))
if __name__=='__main__':main()
