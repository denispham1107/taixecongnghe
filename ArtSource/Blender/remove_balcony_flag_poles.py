"""21.88: remove only the black balcony flag rod; export eight existing shop FBXs.
Use a separate background Blender process; no live file or city installer is touched.
"""
import bpy,json,hashlib,subprocess,traceback
from pathlib import Path
ROOT=Path(r'C:/Users/HP/Documents/GameUnity/Taixecongnghe')
OUT=ROOT/'Assets/PhoViet/Art/Streets';BEFORE=ROOT/'.tools/balcony-flags-before'
REPORT=ROOT/'Captures/balcony-flags-blender-checks.json'
checks=[]
def check(s,b):
    checks.append({'name':s,'passed':bool(b)})
    if not b:raise RuntimeError(s)
def digest(g):
    data=[(tuple(g.v[i] for i in f),c,t,h) for f,c,t,h in zip(g.f,g.cols,g.tags,g.hint)]
    return hashlib.sha256(json.dumps(data).encode()).hexdigest()
def library(old,pet):
    ns={'__name__':'balcony_flag_library'}
    code=(BEFORE/'generator.py').read_text(encoding='utf8') if old and (BEFORE/'generator.py').exists() else subprocess.check_output(['git','show','b620b78:ArtSource/Blender/pho_viet_saigon_streets.py'],cwd=ROOT).decode('utf8') if old else (ROOT/'ArtSource/Blender/pho_viet_saigon_streets.py').read_text(encoding='utf8')
    exec(compile(code,'pho_viet_saigon_streets.py','exec'),ns)
    if pet:
        original=ns['sign']
        def sign(g,spec,obstacles):
            spec=dict(spec)
            for k in ['side','script','icon']:spec.pop(k,None)
            spec.update(main='Cù Lao Petshop',sub='Lưu Trú - Spa Pet',board='blue',ink='white',subink='white')
            original(g,spec,obstacles)
        ns['sign']=sign
        exec(compile((ROOT/'ArtSource/Blender/pho_viet_petshop_layout.py').read_text(encoding='utf8'),'pho_viet_petshop_layout.py','exec'),ns)
        ns['_original_interior']=ns['interior'];ns['interior']=ns['street_petshop_interior'];ns['sidewalk']=ns['street_petshop_sidewalk']
        ns['blade_sign']=lambda *args,**kw:None
    return ns
def geos(ns,i,pet):
    gs=[ns['shop'](i,d) for d in range(3)]
    for g in gs:
        if pet:g.meta.update(display='Cù Lao Petshop',brand='Cù Lao Petshop',subtitle='Lưu Trú - Spa Pet',original_shop=i,kind='petshop',board='blue',ink='white',pets=g.pet_instances)
        else:g.meta.update(board=ns['SHOPS'][i]['board'],ink=ns['SHOPS'][i]['ink'])
    return gs
try:
    pending=[]
    for pet in [False,True]:
        old,new=library(True,pet),library(False,pet)
        for i in [0,3,6,9]:
            name='PV_StreetShop%02d%s'%(i,'_Petshop' if pet else '')
            path=BEFORE/(name+'.json')
            baseline=json.loads(path.read_text(encoding='utf8') if path.exists() else subprocess.check_output(['git','show','b620b78:Assets/PhoViet/Art/Streets/'+name+'.json'],cwd=ROOT).decode('utf8'))
            a,b=geos(old,i,pet),geos(new,i,pet)
            check(name+' source matches installed mesh counts',[sum(len(f)-2 for f in g.f) for g in a]==baseline['lod_triangles'])
            flag=old['flag'];old['flag']=new['flag']
            reference=geos(old,i,pet);old['flag']=flag
            for d in range(3):
                check(name+' LOD%d only rod removed'%d,digest(reference[d])==digest(b[d]) and len(a[d].v)-len(b[d].v)==10 and sum(len(f)-2 for f in a[d].f)-sum(len(f)-2 for f in b[d].f)==16)
                check(name+' LOD%d collision/pets/details unchanged'%d,a[d].meta==b[d].meta)
            pending.append((name,new,b,baseline))
    for ob in list(bpy.data.objects):bpy.data.objects.remove(ob,do_unlink=True)
    scene=bpy.context.scene;scene.name='PV_BalconyFlags_2188'
    coll=bpy.data.collections.new('PV_BalconyFlags_2188');scene.collection.children.link(coll)
    atlas=bpy.data.materials.new('PV_StreetAtlas');atlas.use_nodes=True
    glass=bpy.data.materials.new('PV_StreetGlass');glass.use_nodes=True
    tex=atlas.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(OUT/'PV_StreetPalette.png'));tex.interpolation='Closest';tex.image.pack()
    bs=next(n for n in atlas.node_tree.nodes if n.type=='BSDF_PRINCIPLED');atlas.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
    results=[]
    for k,(name,ns,gs,before) in enumerate(pending):
        ns['MAT']=atlas;ns['GLASS']=glass
        info=ns['save_asset'](name,gs,coll,((k%4)*10,(k//4)*18,0))
        check(name+' metadata identical except triangle counts',{x:v for x,v in info.items() if x!='lod_triangles'}=={x:v for x,v in before.items() if x!='lod_triangles'})
        results.append({'name':name,'old':before['lod_triangles'],'new':info['lod_triangles']})
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_BalconyFlags.blend'))
    report={'success':True,'passed':len(checks),'checks':checks,'results':results,'scope':'8 shop variants, 3 LODs; only black flag rod removed'}
    REPORT.write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
    print(json.dumps({'success':True,'passed':len(checks),'assets':len(results)}))
except Exception:
    REPORT.write_text(json.dumps({'success':False,'checks':checks,'error':traceback.format_exc()},indent=2,ensure_ascii=False),encoding='utf8');raise
