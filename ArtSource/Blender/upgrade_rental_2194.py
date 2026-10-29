"""21.94: enlarge only existing rental plaques, through a separate Blender MCP scene."""
import bpy, json, subprocess
from pathlib import Path
ROOT=Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe')
BEFORE=ROOT/'.tools/tuning2194-before'

def baseline(name,folder):
    p=BEFORE/name
    return p.read_text(encoding='utf8') if p.exists() else subprocess.check_output(['git','show','a8fc029:'+folder+'/'+name],cwd=ROOT).decode('utf8')

def main():
    assert bpy.data.filepath=='' and len(bpy.data.objects)<=3, 'A fresh Blender session is required'
    base={'__name__':'fixture_export_library'}
    exec(compile((ROOT/'ArtSource/Blender/upgrade_fixture_details.py').read_text(encoding='utf8'),'upgrade_fixture_details.py','exec'),base)
    ns=base['library'](river=True);base['materials'](ns)
    old=base['library'](river=True)
    exec(compile(baseline('pho_viet_fixture_details.py','ArtSource/Blender'),'baseline_fixture.py','exec'),old)
    parts=ns['apartment_parts']();previous=old['apartment_parts']()
    metadata=json.loads(baseline('PV_RiverApartments.json','Assets/PhoViet/Art/River'))
    checks=[]
    def check(name,ok):
        if not ok:raise RuntimeError(name)
        checks.append(name)
    check('Installed apartment baseline matches source',all([base['triangles'](g) for g in gs]==m['lod_triangles'] for (_,gs),m in zip(previous,metadata['parts'])))
    fresh=ns['rental_sign'];ns['rental_sign']=old['rental_sign'];reference=ns['apartment_parts']();ns['rental_sign']=fresh
    for (name,gs),(oldname,oldgs),(refname,refs) in zip(parts,previous,reference):
        check(name+' only rental sign geometry changes',name==oldname==refname and all(base['digest'](a)==base['digest'](b) for a,b in zip(oldgs,refs)))
        for d,(a,b) in enumerate(zip(oldgs,gs)):
            x,y=a.rental_signs[0],b.rental_signs[0]
            check(name+' LOD'+str(d)+' width and font grow another 20 percent',all(abs(y[k]/x[k]-1.2)<1e-8 for k in ('width','font_size')))
            check(name+' LOD'+str(d)+' location floor facing and plaque height retained',all(y[k]==x[k] for k in ('label','floor','center','height','facing')))
    for o in list(bpy.data.objects):bpy.data.objects.remove(o,do_unlink=True)
    scene=bpy.context.scene;scene.name='PV_Rental_2194'
    coll=bpy.data.collections.new(scene.name);scene.collection.children.link(coll)
    info=ns['export_parts']('PV_RiverApartments',parts,coll,(0,60,0),ns['river_materials']())
    for a,b in zip(metadata['parts'],info['parts']):
        check(a['name']+' architecture collision and shop metadata retained',{k:v for k,v in a.items() if k not in ('lod_triangles','rental_signs')}=={k:v for k,v in b.items() if k not in ('lod_triangles','rental_signs')})
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_Rental2194.blend'))
    report=dict(success=True,count=len(checks),checks=checks,parts=len(parts),signs=len(parts)*3,source=bpy.data.filepath)
    (ROOT/'Captures/tuning2194-blender-checks.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
    print(json.dumps({k:v for k,v in report.items() if k!='checks'}))
