"""Emerald Tyrant: preserve Meshy skin/UVs; author a weighted rig, walk and tail sweep.
Run through Blender MCP. Coordinates here are Blender Z-up, facing -Y.
"""
import bpy, math, json
from pathlib import Path
from mathutils import Vector, Matrix, Quaternion

ROOT=Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe')
OUT=ROOT/'Assets/PhoViet/Art/Dinosaur'
OUT.mkdir(parents=True,exist_ok=True)
SOURCE=ROOT/'Assets/MeshyImports/Emerald Tyrant_20261026_221137'
COL='PV_Dinosaur_Authoring'
if bpy.data.collections.get(COL):
    for ob in list(bpy.data.collections[COL].objects): bpy.data.objects.remove(ob,do_unlink=True)
    bpy.data.collections.remove(bpy.data.collections[COL])
col=bpy.data.collections.new(COL);bpy.context.scene.collection.children.link(col)
for action in list(bpy.data.actions):
    if action.name.startswith(('Walk','TailSweep')):bpy.data.actions.remove(action)
for data in list(bpy.data.armatures):
    if data.name.startswith('PV_EmeraldRig') and not data.users:bpy.data.armatures.remove(data)
before=set(bpy.data.objects)
bpy.ops.import_scene.fbx(filepath=str(SOURCE/'Meshy_AI_Emerald_Tyrant_20261025_texture.fbx'))
mesh=next(o for o in set(bpy.data.objects)-before if o.type=='MESH')
for c in list(mesh.users_collection): c.objects.unlink(mesh)
col.objects.link(mesh)
bpy.context.view_layer.objects.active=mesh
bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True)
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
points=[v.co.copy() for v in mesh.data.vertices]
minimum=Vector(tuple(min(v[i] for v in points) for i in range(3)))
maximum=Vector(tuple(max(v[i] for v in points) for i in range(3)))
factor=4.65/(maximum.x-minimum.x)
for v in mesh.data.vertices:
    v.co=(v.co-Vector(((minimum.x+maximum.x)*.5,0,minimum.z)))*factor
mesh.name='PV_EmeraldTyrant_Skin'
mat=bpy.data.materials.new('PV_EmeraldSkin');mat.use_nodes=True
bs=mat.node_tree.nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=.78
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(SOURCE/'meshy_basecolor.png'),check_existing=True)
mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
mesh.data.materials.clear();mesh.data.materials.append(mat)

arm=bpy.data.armatures.new('PV_EmeraldRig');rig=bpy.data.objects.new('PV_EmeraldRig',arm);col.objects.link(rig)
bpy.context.view_layer.objects.active=rig;mesh.select_set(False);rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
spec={}
def bone(name,head,tail,parent=None):
    b=arm.edit_bones.new(name);b.head=head;b.tail=tail
    if parent:b.parent=arm.edit_bones[parent]
    spec[name]=(Vector(head),Vector(tail),parent)
bone('Root',(0,0,0),(0,0,.5))
bone('Pelvis',(-.55,.1,2.8),(-.55,-.7,3.4),'Root')
bone('Spine',(-.55,-.7,3.4),(-.55,-1.7,4.3),'Pelvis')
bone('Neck',(-.55,-1.7,4.3),(-.55,-2.5,5.1),'Spine')
bone('Head',(-.55,-2.5,5.1),(-.55,-3.6,5.25),'Neck')
tail=[(-.55,.55,3.2),(-.6,1.45,3.05),(-.45,2.3,2.9),(.1,3.1,2.6),(.95,3.85,2.2),(1.9,4.25,1.25)]
for i in range(5):bone('Tail'+str(i),tail[i],tail[i+1],'Pelvis' if i==0 else 'Tail'+str(i-1))
bone('TailTip',tail[-1],(1.98,4.27,1.10),'Tail4')
for side,x in [('L',-1.55),('R',.55)]:
    bone('Thigh'+side,(x,.05,2.75),(x,-.1,1.65),'Pelvis')
    bone('Shin'+side,(x,-.1,1.65),(x,.05,.52),'Thigh'+side)
    bone('Foot'+side,(x,.05,.52),(x,-.95,.20),'Shin'+side)
    ax=-1.25 if side=='L' else .15
    bone('Arm'+side,(ax,-1.5,3.7),(ax,-1.8,3.0),'Spine')
    bone('Hand'+side,(ax,-1.8,3.0),(ax,-2.2,2.85),'Arm'+side)
bpy.ops.object.mode_set(mode='OBJECT')
arm.bones['Root'].use_deform=False
arm.bones['TailTip'].use_deform=False

# Smooth distance weights with anatomical regions prevent tail/legs pulling the torso.
groups={n:mesh.vertex_groups.new(name=n) for n in spec if n!='Root'}
def distance(p,a,b):
    d=b-a;t=max(0,min(1,(p-a).dot(d)/d.length_squared));return (p-a-d*t).length
for v in mesh.data.vertices:
    p=v.co
    if p.y>.8: candidates=['Pelvis']+['Tail'+str(i) for i in range(5)]+['TailTip']
    elif p.z<2.7 and p.y>-1.6:
        side='L' if p.x<-.55 else 'R';candidates=['Thigh'+side,'Shin'+side,'Foot'+side,'Pelvis']
    elif p.y<-2.15:candidates=['Neck','Head','Spine']
    elif p.z<3.5 and p.y<-1.2:
        side='L' if p.x<-.55 else 'R';candidates=['Arm'+side,'Hand'+side,'Spine','Neck']
    else:candidates=['Pelvis','Spine','Neck','Head','Tail0']
    weighted=sorted([(1/(.13+distance(p,*spec[n][:2]))**4,n) for n in candidates],reverse=True)[:4]
    total=sum(w for w,n in weighted)
    for w,n in weighted:groups[n].add([v.index],w/total,'REPLACE')
mesh.parent=rig;mod=mesh.modifiers.new('Emerald deformation','ARMATURE');mod.object=rig;mod.use_deform_preserve_volume=True
# Bone-heat diffusion keeps the belly/hip transitions continuous when crouching.
# Retain anatomical distance weights for isolated teeth/claws if heat solving fails.
fallback=[[(mesh.vertex_groups[w.group].name,w.weight) for w in v.groups] for v in mesh.data.vertices]
bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
try:bpy.ops.object.parent_set(type='ARMATURE_AUTO')
except RuntimeError:pass
for v,weights in zip(mesh.data.vertices,fallback):
    if sum(w.weight for w in v.groups)<.001:
        for name,weight in weights:mesh.vertex_groups[name].add([v.index],weight,'REPLACE')
for modifier in mesh.modifiers:
    if modifier.type=='ARMATURE':modifier.use_deform_preserve_volume=True
rig.animation_data_create()
for pb in rig.pose.bones:pb.rotation_mode='QUATERNION'

def orient(name,head,target):
    pb=rig.pose.bones[name];rest=arm.bones[name]
    q=(rest.tail_local-rest.head_local).rotation_difference(Vector(target)-Vector(head))@rest.matrix_local.to_quaternion()
    pb.matrix=Matrix.Translation(Vector(head))@q.to_matrix().to_4x4()
    bpy.context.view_layer.update()
    return pb.tail.copy()
def smooth(t):t=max(0,min(1,t));return t*t*t*(10+t*(-15+6*t))
def pose(t,whip):
    for pb in rig.pose.bones:pb.matrix_basis=Matrix.Identity(4)
    phase=t*2*math.pi/1.4
    # Three phases: load the rear leg, low accelerating sweep, damped recovery.
    load=smooth(t/.85);strike=smooth((t-.85)/.70);recover=smooth((t-1.55)/1.25)
    envelope=(load*(1-recover)) if whip else 0
    angle=math.radians(-100-160*strike)
    # Pivot the animal, not the tail through its stomach: a planted loaded step,
    # hips lead the sweep, then head/torso follow through and settle back to walking.
    yaw=math.radians(100*load+80*strike+180*recover) if whip else 0
    spin=Quaternion((0,0,1),yaw);shift=Vector((2.4*math.sin(angle)*envelope,0,-1.0*envelope))
    root=rig.pose.bones['Root'];root.matrix=Matrix.Translation(shift)@spin.to_matrix().to_4x4()@arm.bones['Root'].matrix_local
    hip=rig.pose.bones['Pelvis'];hip.location=(0,0,.045*math.cos(phase*2))
    bpy.context.view_layer.update()
    # Independent feet: stance planted relative to the moving road; smooth lifted return.
    for side,offset in [('L',0),('R',.5)]:
        u=(t/1.4+offset)%1;stance=u<.60
        fy=(-1.25+2.5*(u/.6)) if stance else (1.25-2.5*smooth((u-.6)/.4))
        lift=0 if stance else .40*math.sin(math.pi*(u-.6)/.4)**2
        a=rig.pose.bones['Thigh'+side].head.copy();x=spec['Thigh'+side][0].x
        target=spin@Vector((x,fy,.52+lift))+Vector((shift.x,shift.y,0));d=target-a;l1=(spec['Thigh'+side][1]-spec['Thigh'+side][0]).length;l2=(spec['Shin'+side][1]-spec['Shin'+side][0]).length
        length=min(d.length,l1+l2-.02);unit=d.normalized();along=(l1*l1-l2*l2+length*length)/(2*length)
        pole=spin@Vector((0,-1,0));pole=(pole-unit*pole.dot(unit)).normalized()
        knee=a+unit*along+pole*math.sqrt(max(0,l1*l1-along*along))
        knee=orient('Thigh'+side,a,knee);ankle=orient('Shin'+side,knee,target)
        orient('Foot'+side,ankle,ankle+spin@Vector((0,-1,-.32)))
    if whip:
        head=rig.pose.bones['Tail0'].head.copy()
        # Lower distal tail while loading; its long physical arc reaches both road edges.
        direction=Vector((math.sin(angle),math.cos(angle),-.29)).normalized()
        for i in range(5):
            n='Tail'+str(i);pb=rig.pose.bones[n];rest=spin@(spec[n][1]-spec[n][0]).normalized()
            # Pose in armature coordinates; mix rest curvature into the recovering sweep.
            desired=(rest*(1-envelope)+direction*envelope).normalized()
            head=orient(n,pb.head.copy(),pb.head+desired)
    else:
        for i in range(5):rig.pose.bones['Tail'+str(i)].rotation_quaternion=Vector((0,1,0)).rotation_difference(Vector((.035*math.sin(phase-i*.35),1,0)).normalized())
    if whip:
        neck=rig.pose.bones['Neck'];direction=spin@(Quaternion((1,0,0),math.radians(75)*envelope)@(spec['Neck'][1]-spec['Neck'][0]))
        orient('Neck',neck.head.copy(),neck.head+direction)
        head=rig.pose.bones['Head'];direction=spin@(Quaternion((1,0,0),math.radians(35)*envelope)@(spec['Head'][1]-spec['Head'][0]))
        orient('Head',head.head.copy(),head.head+direction)
    else:
        rig.pose.bones['Neck'].rotation_quaternion=Vector((0,1,0)).rotation_difference(Vector((0,1,.012*math.sin(phase))))
    bpy.context.view_layer.update()

scene=bpy.context.scene;scene.render.fps=60
for name,duration,whip in [('Walk',1.4,False),('TailSweep',2.8,True)]:
    action=bpy.data.actions.new(name);rig.animation_data.action=action
    for frame in range(round(duration*60)+1):
        scene.frame_set(frame);pose(frame/60,whip)
        for pb in rig.pose.bones:
            pb.keyframe_insert('location',frame=frame,group=pb.name)
            pb.keyframe_insert('rotation_quaternion',frame=frame,group=pb.name)
    action.use_fake_user=True
rig.animation_data.action=bpy.data.actions['Walk'];scene.frame_set(0)
bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(OUT/'PV_EmeraldTyrant.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,apply_scale_options='FBX_SCALE_UNITS',use_mesh_modifiers=True)
report={'source':str(SOURCE),'width':4.65,'height':(maximum.z-minimum.z)*factor,'length':(maximum.y-minimum.y)*factor,'vertices':len(mesh.data.vertices),'bones':len(spec),'clips':{'Walk':1.4,'TailSweep':2.8},'walkSpeed':3,'tailPoints':tail}
(OUT/'PV_EmeraldTyrant.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_Dinosaur.blend'))
print(json.dumps(report))
