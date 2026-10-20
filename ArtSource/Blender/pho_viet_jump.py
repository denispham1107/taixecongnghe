"""Run in live Blender MCP after opening the motorcycle source.
Creates real keyed actions and a sampled Unity pose library, without changing the FBXs.
The original motorcycle source is never overwritten.
"""
import bpy, json, math, os
from mathutils import Euler, Vector, Quaternion
ROOT = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
rig = bpy.data.objects['PV_HeroScooter_Rig']
names = ['Pelvis','Torso','Head','LUpperArm','RUpperArm','LForearm','RForearm','LThigh','RThigh','LShin','RShin']
def smooth(t):
    t=max(0,min(1,t)); return t*t*(3-2*t)
def interpolate(keys,t):
    for i in range(1,len(keys)):
        if t<=keys[i][0]:
            a,x=keys[i-1]; b,y=keys[i]; return x+(y-x)*smooth((t-a)/(b-a))
    return keys[-1][1]
clips=[]
for name in ['JumpTakeoff','JumpAir','LandBoth','LandFront','LandRear']:
    action=bpy.data.actions.get(name)
    if action: bpy.data.actions.remove(action)
    action=bpy.data.actions.new(name); action.use_fake_user=True
    rig.animation_data_create()
    for track in rig.animation_data.nla_tracks: track.mute=True
    rig.animation_data.action=action
    samples=[]
    for frame in range(1,62):
        t=(frame-1)/60
        if name=='JumpTakeoff':
            pitch=interpolate([(0,0),(.28,4),(.70,-29),(1,-22)],t)
            crouch=interpolate([(0,0),(.3,1),(.75,.3),(1,.65)],t)
        elif name=='JumpAir':
            pitch=interpolate([(0,-22),(.4,-8),(.7,0),(1,9)],t)
            crouch=interpolate([(0,.65),(.4,1),(1,.4)],t)
        else:
            initial={'LandBoth':0,'LandFront':12,'LandRear':-16}[name]
            pitch=interpolate([(0,initial),(.23,-initial*.25),(.5,initial*.12),(1,0)],t)
            crouch=interpolate([(0,.35),(.22,1),(.58,.15),(1,0)],t)
        rig['jump_pitch']=pitch; rig['jump_crouch']=crouch
        rig.keyframe_insert(data_path='["jump_pitch"]',frame=frame)
        rig.keyframe_insert(data_path='["jump_crouch"]',frame=frame)
        for p in rig.pose.bones:
            p.rotation_mode='QUATERNION'; p.rotation_quaternion=Quaternion(); p.location=(0,0,0)
        # Body counterbalances the wheelie; elbows and knees absorb the landing.
        angles=[0, -10*crouch-pitch*.25, 6*crouch+pitch*.12,
                -8*crouch,-8*crouch,15*crouch,15*crouch,
                -14*crouch,-14*crouch,20*crouch,20*crouch]
        for n,a in zip(names,angles):
            p=rig.pose.bones[n]; basis=p.bone.matrix_local.to_quaternion()
            p.rotation_quaternion=basis.inverted() @ Euler((math.radians(a),0,0),'XYZ').to_quaternion() @ basis
            p.keyframe_insert(data_path='rotation_quaternion',frame=frame,group=n)
        pelvis=rig.pose.bones['Pelvis']; pelvis.location=pelvis.bone.matrix_local.to_quaternion().inverted() @ Vector((0,0,-.065*crouch))
        pelvis.keyframe_insert(data_path='location',frame=frame,group='Pelvis')
        # Preview front lift about the rear tyre; runtime physics owns world height.
        rig.rotation_euler=(math.radians(pitch),0,0)
        rig.keyframe_insert(data_path='rotation_euler',frame=frame,group='Motorcycle balance')
        samples.append({'pitch':pitch,'crouch':crouch,'angles':angles})
    clips.append({'name':name,'samples':samples})
os.makedirs(ROOT+'/Assets/PhoViet/Resources',exist_ok=True)
with open(ROOT+'/Assets/PhoViet/Resources/PhoVietJump.json','w',encoding='utf-8') as f:
    json.dump({'source':'Blender MCP keyed actions at 60 samples/phase','bones':names,'clips':clips},f,indent=2)
# Keep a compact editable source containing the hero only; leave original source files intact.
for obj in list(bpy.data.objects):
    if obj!=rig and obj.parent!=rig: bpy.data.objects.remove(obj,do_unlink=True)
bpy.context.scene.frame_start=1; bpy.context.scene.frame_end=61
bpy.context.scene.render.fps=60
# Full editable demonstration combines all phases with actual forward and vertical travel.
old=bpy.data.actions.get('JumpDemo')
if old: bpy.data.actions.remove(old)
demo=bpy.data.actions.new('JumpDemo');demo.use_fake_user=True;rig.animation_data.action=demo
for frame in range(1,113):
    t=(frame-1)/60
    if t<.24: name='JumpTakeoff'; u=t/.24; height=0
    elif t<.24+2*12.4/22:
        age=t-.24; name='JumpAir';u=age/1.12;height=12.4*age-11*age*age
    else: name='LandFront';u=(t-.24-2*12.4/22)/.48;height=0
    c=next(c for c in clips if c['name']==name);p=c['samples'][min(60,round(max(0,min(1,u))*60))]
    for n,a in zip(names,p['angles']):
        b=rig.pose.bones[n];basis=b.bone.matrix_local.to_quaternion()
        b.rotation_quaternion=basis.inverted() @ Euler((math.radians(a),0,0),'XYZ').to_quaternion() @ basis
        b.keyframe_insert(data_path='rotation_quaternion',frame=frame,group=n)
    b=rig.pose.bones['Pelvis'];b.location=b.bone.matrix_local.to_quaternion().inverted() @ Vector((0,0,-.065*p['crouch']))
    b.keyframe_insert(data_path='location',frame=frame,group='Pelvis')
    pitch=p['pitch'];rotation=Euler((math.radians(pitch),0,0),'XYZ').to_quaternion();pivot=Vector((0,.73,0))
    # Positive nose-down pitch needs front-tyre support at contact.
    lift=max(0,math.sin(math.radians(pitch))*1.51) if height==0 else 0
    rig.rotation_euler=(math.radians(pitch),0,0)
    rig.location=pivot-rotation@pivot+Vector((0,-17*t,height+lift))
    rig.keyframe_insert(data_path='rotation_euler',frame=frame,group='Trajectory')
    rig.keyframe_insert(data_path='location',frame=frame,group='Trajectory')
bpy.context.scene.frame_end=112
bpy.context.scene.frame_set(43)
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Blender/PhoViet_Jump.blend')
print(json.dumps({'success':True,'actions':[c['name'] for c in clips],'samples':sum(len(c['samples']) for c in clips),'source':bpy.data.filepath}))
