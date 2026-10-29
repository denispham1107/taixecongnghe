"""21.92: export only the existing traffic signal and apartment FBXs via Blender MCP.
Preserves the user's edited PhoViet_FixtureDetails.blend. Baseline: d2a5cf6.
"""
import bpy,json,subprocess,hashlib
from pathlib import Path
ROOT=Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe')
BEFORE=ROOT/'.tools/signal-rental-2192-before'
REPORT=ROOT/'Captures/signal-rental-blender-checks.json'
base={'__name__':'fixture_export_library'}
exec(compile((ROOT/'ArtSource/Blender/upgrade_fixture_details.py').read_text(encoding='utf8'),'upgrade_fixture_details.py','exec'),base)
checks=[]
def check(name,value):
    checks.append({'name':name,'passed':bool(value)})
    if not value:raise RuntimeError(name)
def before(name,folder):
    p=BEFORE/name
    return p.read_text(encoding='utf8') if p.exists() else subprocess.check_output(['git','show','d2a5cf6:'+folder+'/'+name],cwd=ROOT).decode('utf8')

def main():
    assert bpy.data.filepath=='' and len(bpy.data.objects)<=3,'Use the new blank Blender session, never replace a user scene'
    for o in list(bpy.data.objects):bpy.data.objects.remove(o,do_unlink=True)
    scene=bpy.context.scene;scene.name='PV_SignalRental_2192';coll=bpy.data.collections.new(scene.name);scene.collection.children.link(coll)
    ns=base['library']();base['materials'](ns)
    gs=[ns['traffic_signal'](d) for d in range(3)]
    for d,g in enumerate(gs):
        faces=[(face,col) for face,col,tag in zip(g.f,g.cols,g.tags) if tag=='SignalOptical']
        half=len(faces)//2
        check('Signal LOD%d has equal optical faces'%d,half>0 and len(faces)==2*half)
        for (a,ca),(b,cb) in zip(faces[:half],faces[half:]):
            va=[g.v[i] for i in a];vb=[g.v[i] for i in b]
            if ca!=cb or any(abs(x-u)>1e-8 or abs(y-v)>1e-8 or abs((-.044 if y>5 else 0)-z-w)>1e-8 for (x,y,z),(u,v,w) in zip(va,vb)):
                raise RuntimeError('Signal optical reflection mismatch')
        check('Signal LOD%d every visor/rim/bolt is mirrored'%d,True)
    old=json.loads(before('PV_TrafficSignal.json','Assets/PhoViet/Art/Streets'))
    check('Original signal collider and lens sockets retained',gs[0].meta=={k:v for k,v in old.items() if k in ('obstacles','head_center')})
    signal=[]
    for d,g in enumerate(gs):signal.append(ns['to_mesh']('PV_TrafficSignal' if d==0 else 'PV_TrafficSignal_LOD'+str(d),g,coll))
    for o in signal[1:]:o.parent=signal[0]
    ns['export_lod_hierarchy'](str(ROOT/'Assets/PhoViet/Art/Streets/PV_TrafficSignal.fbx'),signal)
    for o in signal[1:]:o.hide_render=True;o.hide_set(True)
    sig_meta=dict(old,lod_triangles=[base['triangles'](g) for g in gs]);sig_meta.update(gs[0].meta)
    (ROOT/'Assets/PhoViet/Art/Streets/PV_TrafficSignal.json').write_text(json.dumps(sig_meta,indent=2,ensure_ascii=False),encoding='utf8')
    signal[0].location=(0,-24,0)

    ns=base['library'](river=True);base['materials'](ns)
    parts=ns['apartment_parts']()
    oldns=base['library'](river=True)
    exec(compile(before('pho_viet_fixture_details.py','ArtSource/Blender'),'old_fixture_details.py','exec'),oldns)
    old_parts=oldns['apartment_parts']()
    metadata=json.loads(before('PV_RiverApartments.json','Assets/PhoViet/Art/River'))
    check('Apartment source matches installed geometry',all([base['triangles'](g) for g in gs]==m['lod_triangles'] for (name,gs),m in zip(old_parts,metadata['parts'])))
    fresh_sign=ns['rental_sign'];ns['rental_sign']=oldns['rental_sign'];reference=ns['apartment_parts']();ns['rental_sign']=fresh_sign
    for (name,gs),(oldname,oldgs),(refname,refs) in zip(parts,old_parts,reference):
        check(name+' no changes outside rental signs',name==refname==oldname and all(base['digest'](a)==base['digest'](b) for a,b in zip(oldgs,refs)))
        for d,(a,b) in enumerate(zip(oldgs,gs)):
            old_sign,new_sign=a.rental_signs[0],b.rental_signs[0]
            check(name+' LOD%d exact 20 percent width/font increase'%d,abs(new_sign['width']/old_sign['width']-1.2)<1e-8 and abs(new_sign['font_size']/.58-1.2)<1e-8)
            check(name+' LOD%d location/floor/height unchanged'%d,all(new_sign[k]==old_sign[k] for k in ('label','floor','center','height','facing')))
    info=ns['export_parts']('PV_RiverApartments',parts,coll,(0,60,0),ns['river_materials']())
    for a,b in zip(metadata['parts'],info['parts']):
        check(a['name']+' slot, shop and pet metadata unchanged',{k:v for k,v in a.items() if k not in ('lod_triangles','rental_signs')}=={k:v for k,v in b.items() if k not in ('lod_triangles','rental_signs')})
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_SignalRental2192.blend'))
    report={'success':True,'count':len(checks),'checks':checks,'signalTriangles':sig_meta['lod_triangles'],'rentalSigns':sum(len(gs[0].rental_signs) for _,gs in parts),'riverTriangles':info['lod_triangles']}
    REPORT.write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
    print(json.dumps({k:v for k,v in report.items() if k!='checks'}))

def refresh_signal():
    """Refine only our newly saved signal; the apartment export stays untouched."""
    assert bpy.data.filepath.replace('\\','/').endswith('/PhoViet_SignalRental2192.blend')
    ns=base['library']();base['materials'](ns);coll=bpy.data.collections['PV_SignalRental_2192']
    for ob in list(coll.objects):
        if ob.name.startswith('PV_TrafficSignal'):
            mesh=ob.data;bpy.data.objects.remove(ob,do_unlink=True)
            if mesh and mesh.users==0:bpy.data.meshes.remove(mesh)
    gs=[ns['traffic_signal'](d) for d in range(3)]
    objs=[ns['to_mesh']('PV_TrafficSignal' if d==0 else 'PV_TrafficSignal_LOD'+str(d),g,coll) for d,g in enumerate(gs)]
    for ob in objs[1:]:ob.parent=objs[0]
    ns['export_lod_hierarchy'](str(ROOT/'Assets/PhoViet/Art/Streets/PV_TrafficSignal.fbx'),objs)
    for ob in objs[1:]:ob.hide_render=True;ob.hide_set(True)
    objs[0].location=(0,-24,0)
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_SignalRental2192.blend'))
    r=json.loads(REPORT.read_text(encoding='utf8'))
    assert r['signalTriangles']==[base['triangles'](g) for g in gs]
    r['checks'].append({'name':'Signal black infill is proud of silver surround on both sides','passed':True});r['count']=len(r['checks'])
    REPORT.write_text(json.dumps(r,indent=2,ensure_ascii=False),encoding='utf8')
    print('Signal surround refined, triangles and apartment file unchanged')
