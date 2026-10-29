"""Idempotent corrections after visual inspection of the first high-res render."""
import bpy,json,math
from mathutils import Vector
from pathlib import Path
P='PV86_';OUT=Path(r'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/City-20261029')
for c in bpy.data.collections:
    if not c.name.startswith(P):continue
    for ob in c.objects:
        if ob.type!='FONT':continue
        ob.scale=(1,1,1)
        if c.name.startswith(P+'House_'):
            # Main shop names fit vertically within their own band; subtitles stay small.
            if abs(ob.location.z-4.36)<.01:ob.data.size=.82
            elif abs(ob.location.z-3.92)<.01:ob.data.size=.21
for matname in ['glass','glass2']:
    mat=bpy.data.materials[P+matname];n=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    n.inputs['Roughness'].default_value=.38;n.inputs['Specular IOR Level'].default_value=.12
for ld in bpy.data.lights:
    if ld.name.startswith(P) and 'FacadeFill' in ld.name:ld.energy=3500
stage=bpy.data.collections[P+'Street_Preview_PRESENTATION_ONLY']
buildings=bpy.data.collections[P+'Street_BUILDINGS_ONLY']
if bpy.data.objects.get(P+'Existing_Player_Car_Context'):bpy.data.objects[P+'Existing_Player_Car_Context'].location.y=-8
last=bpy.data.objects.get(P+'House_-1_14')
if last:last.instance_collection=bpy.data.collections[P+'House_12_petshop']
def instance(name,coll,loc,angle):
    if bpy.data.objects.get(P+name):return
    ob=bpy.data.objects.new(P+name,None);ob.instance_type='COLLECTION';ob.instance_collection=bpy.data.collections[P+coll];ob.location=loc;ob.rotation_euler.z=angle;buildings.objects.link(ob)
for side in [-1,1]:
    for j in range(2):
        col=('House_04_pharmacy' if j==0 else 'House_06_coffee_Corner') if side<0 else ('House_12_petshop_Corner' if j==0 else 'House_05_garage')
        instance('Foreground_%d_%d'%(side,j),col,(side*10.6,-4-(j+1)*8.36,.16),-side*math.pi/2)
for side in [-1,1]:
    for j in range(2):instance('Skyline_%d_%d'%(side,j),'River_Apartment_%02d'%j,(side*(22+j*17),151+j*8,.16),0)
# Lower view direction to match the reference's elevated street view and show the car.
for name,target in [('Street_Wide',(0,37,0)),('Street_Portrait',(0,25,0))]:
    ob=bpy.data.objects[P+name];ob.rotation_euler=(Vector(target)-ob.location).to_track_quat('-Z','Y').to_euler()
for sc in bpy.data.scenes:
    if sc.name.startswith(P):
        sc.render.resolution_percentage=100
        sc.eevee.taa_render_samples=32
meta=json.loads((OUT/'study.json').read_text(encoding='utf8'))
meta['refinement']='Font vertical fit; glass highlight suppression; reduced facade fill; original player car visible; foreground buildings close blank row ends.'
meta['main_row_petshops']=9;meta['main_row_shops']=30
(OUT/'study.json').write_text(json.dumps(meta,indent=2,ensure_ascii=False),encoding='utf8')
bpy.context.window.scene=bpy.data.scenes[P+'Street_Preview']
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'PhoViet_NewCity_Approval.blend'),copy=True)
print('Refinements applied; game/source tree meshes remain unchanged')
