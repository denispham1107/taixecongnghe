"""Two original passengers, authored via the live Blender MCP. Metres, Y up."""
import bpy, math, os, json
from mathutils import Vector,Quaternion
ROOT='C:/Users/HP/Documents/GameUnity/Taixecongnghe'
exec(compile(open(ROOT+'/ArtSource/Blender/pho_viet_bikes.py',encoding='utf-8').read(),'pho_viet_bikes.py','exec'),globals())
OUT=ROOT+'/Assets/PhoViet/Art/Passengers';os.makedirs(OUT,exist_ok=True)
def passenger_rig(name):
 data=bpy.data.armatures.new(name+'_Skeleton');rig=bpy.data.objects.new(name+'_Rig',data);bpy.context.collection.objects.link(rig)
 bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
 defs=[('Frame',None,(0,0,0)),('Pelvis','Frame',(0,.94,0)),('Torso','Pelvis',(0,1.06,0)),('Head','Torso',(0,1.45,0))]
 for s,side in [(-1,'L'),(1,'R')]:
  defs.extend([(side+'UpperArm','Torso',(s*.22,1.38,0)),(side+'Forearm',side+'UpperArm',(s*.27,1.10,.015)),
   (side+'Thigh','Pelvis',(s*.12,.94,0)),(side+'Shin',side+'Thigh',(s*.13,.52,.02)),(side+'Foot',side+'Shin',(s*.14,.11,.02))])
 for n,parent,p in defs:
  bone=data.edit_bones.new(n);bone.head=B(p);bone.tail=B(p)+Vector((0,0,.12))
  if parent:bone.parent=data.edit_bones[parent]
 bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False);return rig
def passenger(g,female):
 c=C;hip=.94
 g.ell((0,.95,0),(.19,.13,.12),c['pants'],'Pelvis',16,8)
 g.loft([(1.0,.17,.12,0),(1.15,.185,.115,0),(1.37,.21,.12,0),(1.44,.15,.09,0)],c['shirt'],'Torso',vertical=True,n=16)
 g.torus((0,1.45,0),.08,.016,c['shirtDark'],'Torso',axis='y',n=12,m=4)
 g.rod((0,1.42,0),(0,1.51,.005),.06,c['skin'],'Head',n=10)
 for s,side in [(-1,'L'),(1,'R')]:
  shoulder=(s*.22,1.38,0);elbow=(s*.27,1.1,.015);wrist=(s*.28,.91,.035)
  g.ell(shoulder,(.08,.09,.09),c['shirt'],side+'UpperArm',10,6)
  g.rod(shoulder,elbow,.075,c['shirt'],side+'UpperArm',10,r2=.057)
  g.rod(elbow,wrist,.05,c['skin'],side+'Forearm',10,r2=.035)
  g.ell((s*.28,.87,.035),(.044,.075,.035),c['skin'],side+'Forearm',10,6)
  for finger in range(4):g.rod((s*(.25+finger*.017),.87,.05),(s*(.25+finger*.017),.82,.05),.009,c['skin'],side+'Forearm',5)
  g.rod((s*.12,.94,0),(s*.13,.52,.02),.092,c['pants'],side+'Thigh',12,r2=.075)
  g.rod((s*.13,.52,.02),(s*.14,.11,.02),.074,c['pants'],side+'Shin',10,r2=.05)
  g.box((s*.14,.08,.09),(.15,.12,.29),c['shoe'],side+'Foot',.025)
  g.box((s*.14,.025,.09),(.16,.035,.3),c['sole'],side+'Foot',.008)
  for k in range(3):g.rod((s*.14-.05,.145,.05+k*.035),(s*.14+.05,.145,.05+k*.035),.003,c['cream'],side+'Foot',4)
  g.ell((s*.132,1.64,0),(.025,.045,.027),c['skin'],'Head',10,6)
  g.ell((s*.052,1.674,.123),(.029,.019,.011),c['white'],'Head',10,6)
  g.ell((s*.052,1.674,.132),(.012,.014,.006),c['eye'],'Head',8,6)
  g.curve([(s*.025,1.71,.12),(s*.055,1.716,.116),(s*.08,1.706,.108)],.005,c['hair'],'Head',5)
 g.ell((0,1.63,0),(.13 if female else .14,.17,.13),c['skin'],'Head',18,12)
 # Facial detail is placed on the front surface after the main head.
 for s in [-1,1]:
  g.ell((s*.052,1.674,.123),(.029,.019,.011),c['white'],'Head',10,6)
  g.ell((s*.052,1.674,.132),(.012,.014,.006),c['eye'],'Head',8,6)
 g.ell((0,1.635,.137),(.023,.031,.024),c['skin'],'Head',10,6)
 g.curve([(-.035,1.576,.113),(0,1.564,.127),(.035,1.576,.113)],.004,c['lip'],'Head',6)
 g.ell((0,1.78,-.015),(.17,.105,.17),c['helmet'],'Head',20,10)
 g.box((0,1.755,.14),(.26,.025,.08),c['helmet'],'Head',.009)
 g.ell((0,1.79,-.015),(.025,.104,.171),c['cream'],'Head',12,8)
 for s in [-1,1]:g.curve([(s*.145,1.73,.01),(s*.09,1.53,.03),(0,1.515,.045)],.007,c['rubber'],'Head',6)
 if female:
  g.ell((0,1.64,-.12),(.13,.13,.055),c['hair'],'Head',14,8)
  g.curve([(0,1.67,-.15),(.04,1.55,-.2),(.05,1.42,-.18)],.045,c['hair'],'Head',10)
  g.torus((.02,1.59,-.18),.042,.008,c['shirtDark'],'Head',axis='y',n=10,m=4)
  g.curve([(-.12,1.35,.12),(.08,1.10,.13)],.012,c['hair'],'Torso',6)
  g.box((.16,1.05,.07),(.12,.19,.12),c['shirtDark'],'Pelvis',.025)
 else:
  g.box((-.1,1.27,.123),(.09,.1,.014),c['shirtDark'],'Torso',.006)
 g.curve([(0,1.06,.123),(0,1.36,.123)],.004,c['cream'],'Torso',5)
def clips(rig):
 descriptions=[]
 for name in ['Idle','Walk','Board','Sit']:
  rig.animation_data_clear();rig.animation_data_create();action=bpy.data.actions.new(rig.name[:-4]+'_'+name);rig.animation_data.action=action
  for frame in [1,7,13,19,25,31,37]:
   t=(frame-1)/36
   for p in rig.pose.bones:p.rotation_mode='QUATERNION';p.rotation_quaternion=Quaternion();p.location=(0,0,0)
   sit=(t if name=='Board' else 1 if name=='Sit' else 0)
   pose_rotation(rig,'Torso',(-6*sit,0,0))
   for s,side in [(-1,'L'),(1,'R')]:
    wave=math.sin(t*math.tau)*s if name=='Walk' else 0
    pose_rotation(rig,side+'Thigh',(-78*sit+25*wave,0,-s*22*sit))
    pose_rotation(rig,side+'Shin',(78*sit+max(0,-wave)*32,0,0))
    pose_rotation(rig,side+'UpperArm',(-30*sit-20*wave,0,-s*6*sit))
    pose_rotation(rig,side+'Forearm',(-35*sit,0,0))
   if name=='Walk':pose_offset(rig,'Pelvis',(0,.015*abs(math.sin(t*math.tau)),0))
   for p in rig.pose.bones:p.keyframe_insert(data_path='rotation_quaternion',frame=frame);p.keyframe_insert(data_path='location',frame=frame)
  slot=rig.animation_data.action_slot;action.use_fake_user=True;descriptions.append((name,action,slot))
 rig.animation_data_clear();rig.animation_data_create()
 for name,action,slot in descriptions:
  track=rig.animation_data.nla_tracks.new();track.name=name;strip=track.strips.new(name,1,action);strip.action_slot=slot;track.mute=False
 rig.animation_data.action=None
 for p in rig.pose.bones:p.rotation_quaternion=Quaternion();p.location=(0,0,0)
 return descriptions
def build_passengers():
 results=[]
 for obj in list(bpy.data.objects):
  if obj.name.startswith('PV_Passenger'):bpy.data.objects.remove(obj,do_unlink=True)
 for i,female in enumerate([False,True]):
  name='PV_PassengerFemale' if female else 'PV_PassengerMale'
  mat=palette(name,'D85C64' if female else '197F86','D85C64' if female else '197F86','33A6A0' if female else 'F5E9CB')
  rig=passenger_rig(name);g=Geometry();passenger(g,female);mesh=finish_mesh(name,g,rig,mat);objects=[mesh]
  for level,ratio in [(1,.5),(2,.22)]:
   obj=mesh.copy();obj.data=mesh.data.copy();obj.name=name+'_LOD'+str(level);bpy.context.collection.objects.link(obj);obj.parent=rig
   bpy.context.view_layer.objects.active=obj;mod=obj.modifiers.new('LOD detail','DECIMATE');mod.ratio=ratio;bpy.ops.object.modifier_apply(modifier=mod.name);objects.append(obj)
  actions=clips(rig);bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
  for obj in objects:obj.select_set(True)
  bpy.context.view_layer.objects.active=rig
  bpy.ops.export_scene.fbx(filepath=OUT+'/'+name+'.fbx',use_selection=True,object_types={'ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=True,bake_anim_simplify_factor=.1)
  for track in rig.animation_data.nla_tracks:track.mute=True
  counts=[]
  for obj in objects:obj.data.calc_loop_triangles();counts.append(len(obj.data.loop_triangles))
  results.append({'name':name,'bones':len(rig.data.bones),'clips':[a[0] for a in actions],'lod_triangles':counts})
  rig.location.x=i*1.2
  for obj in objects[1:]:obj.hide_set(True);obj.hide_render=True
 with open(OUT+'/catalogue.json','w',encoding='utf-8') as f:json.dump(results,f,indent=2)
 bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Blender/PhoViet_Passengers.blend')
 print(json.dumps(results))
