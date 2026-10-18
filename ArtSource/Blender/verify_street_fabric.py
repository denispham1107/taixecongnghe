"""Run via Blender MCP: check actual exported source mesh canopy coverage."""
import bpy, json
from mathutils import Vector
from mathutils.bvhtree import BVHTree
ROOT='C:/Users/HP/Documents/GameUnity/Taixecongnghe'
checks=[]
for seed in range(12):
 name='PV_StreetShop'+str(seed).zfill(2)
 for level in range(3):
  ob=next(o for o in bpy.data.objects[name].children if o.name.endswith('_LOD'+str(level)))
  mesh=ob.data;canopy_bvh=BVHTree.FromPolygons([v.co for v in mesh.vertices],[p.vertices[:] for p in mesh.polygons],all_triangles=False)
  for ix in range(73):
   x=-3.59+ix*7.18/72
   for iz in range(8):
    z=-5.30+iz*1.12/7
    # Author X,Y,Z -> Blender -X,-Z,Y; test top and underside.
    cursor=Vector((-x,-z,3.6));covered=False
    for step in range(12):
     hit,normal,_,_=canopy_bvh.ray_cast(cursor,Vector((0,0,-1)),max(0,cursor.z-2.78))
     if hit is None:break
     if 2.8<hit.z<3.2 and normal.z>.6:covered=True;break
     cursor=hit-Vector((0,0,.001))
    assert covered,(name,level,x,z,'top')
    cursor=Vector((-x,-z,2.5));covered=False
    for step in range(12):
     hit,normal,_,_=canopy_bvh.ray_cast(cursor,Vector((0,0,1)),max(0,3.2-cursor.z))
     if hit is None:break
     if 2.78<hit.z<3.2 and normal.z<-.6:covered=True;break
     cursor=hit+Vector((0,0,.001)) # Pass genuine support brackets before reaching fabric.
    assert covered,(name,level,x,z,'underside')
  checks.append(name+' LOD'+str(level)+': 1168 roof/underside rays have complete fabric coverage and outward normals')
catalogue=json.load(open(ROOT+'/Assets/PhoViet/Art/Streets/catalogue.json',encoding='utf-8'))
for item in catalogue:
 assert item['decorative_pots']==0,item['name']
checks.append('No decorative pots in any regenerated source asset')
for item in catalogue:
 if item['name'].startswith('PV_StreetTree'):
  assert item['leaf_counts']==[864,432,216],item['name']
  assert item['lod_triangles'][0]>item['lod_triangles'][1]>item['lod_triangles'][2],item['name']
  checks.append(item['name']+': complete individual leaves retained at all three LODs with reducing triangle counts')
result={'success':True,'count':len(checks),'checks':checks,'fabric_rays':12*3*73*8*2}
with open(ROOT+'/Captures/fabric-leaves-blender-results.json','w',encoding='utf-8') as f:json.dump(result,f,indent=2)
print(json.dumps({'success':True,'count':len(checks),'fabric_rays':result['fabric_rays']}))
