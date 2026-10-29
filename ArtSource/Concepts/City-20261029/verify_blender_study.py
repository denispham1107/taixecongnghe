import bpy,json,hashlib
from pathlib import Path
OUT=Path(r'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/City-20261029')
P='PV86_';meta=json.loads((OUT/'study.json').read_text(encoding='utf8'))
checks=[]
def check(name,value,detail=None):checks.append({'name':name,'passed':bool(value),'detail':detail})
check('Original scene still empty',len(bpy.data.scenes['Scene'].objects)==0)
check('Original unsaved filepath retained',bpy.data.filepath=='')
check('Three isolated scenes',len([s for s in bpy.data.scenes if s.name.startswith(P)])==3)
for d in meta['building_families']:
    c=bpy.data.collections[d['collection']]
    check('Collection '+c.name,c.get('asset_role')=='buildings_only' and len(c.objects)>10)
    for o in c.objects:
        if o.type=='FONT' and c.name.startswith(P+'House_') and abs(o.location.z-4.36)<.01:
            check('Main name vertical fit '+c.name,o.dimensions.z<.72,{'height':o.dimensions.z,'scale':list(o.scale)})
for i in range(3):
    src=bpy.data.objects['PV_StreetTree%d_LOD0'%i]
    clones=[o for o in bpy.data.objects if o.name.startswith(P+'Existing_Tree_Context') and o.data==src.data]
    check('Existing tree shared mesh '+str(i),len(clones)>0,{'clones':len(clones),'vertices':len(src.data.vertices)})
shops=bpy.data.collections[P+'Street_BUILDINGS_ONLY']
main=[o for o in shops.objects if o.name.startswith(P+'House_')]
check('Main rows 9 of 30 Petshops',len(main)==30 and sum(o.instance_collection.get('business')=='petshop' for o in main)==9)
check('All file textures packed',all(im.packed_file for im in bpy.data.images if im.source=='FILE' and im.users))
sc=bpy.data.scenes[P+'Street_Preview']
settings={x.identifier:getattr(sc.eevee,x.identifier) for x in sc.eevee.bl_rna.properties if 'sample' in x.identifier and x.type in ('INT','FLOAT','BOOLEAN')}
report={'passed':sum(c['passed'] for c in checks),'failed':sum(not c['passed'] for c in checks),'checks':checks,'render_settings':settings,'active_scene':bpy.context.scene.name}
(OUT/'blender-verification.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
print(json.dumps({'passed':report['passed'],'failed':report['failed'],'settings':settings}))
