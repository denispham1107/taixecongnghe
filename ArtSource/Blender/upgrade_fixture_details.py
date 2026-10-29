"""21.91 selective Blender MCP exporter; never runs a city/river installer.
Call setup(), export_shops([indices]), export_signal(), export_river(), finish().
"""
import bpy,json,random,hashlib,subprocess
from pathlib import Path
ROOT=Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe')
BEFORE=ROOT/'.tools/fixture-details-before'
REPORT=ROOT/'Captures/fixture-details-blender-checks.json'

def baseline_text(name,folder):
    path=BEFORE/name
    if not path.exists():
        BEFORE.mkdir(parents=True,exist_ok=True)
        path.write_text(subprocess.check_output(['git','show','438733a:'+folder+'/'+name],cwd=ROOT).decode('utf8'),encoding='utf8')
    return path.read_text(encoding='utf8')

def report_check(name,passed):
    data=json.loads(REPORT.read_text(encoding='utf8')) if REPORT.exists() else {'checks':[],'assets':[]}
    data['checks'].append({'name':name,'passed':bool(passed)})
    data['success']=all(c['passed'] for c in data['checks'])
    REPORT.write_text(json.dumps(data,indent=2,ensure_ascii=False),encoding='utf8')
    if not passed:raise RuntimeError(name)

def record(name,counts):
    data=json.loads(REPORT.read_text(encoding='utf8'));data['assets']=[a for a in data['assets'] if a['name']!=name]+[{'name':name,'triangles':counts}]
    REPORT.write_text(json.dumps(data,indent=2,ensure_ascii=False),encoding='utf8')

def library(old=False,pet=False,river=False):
    ns={'__name__':'fixture_details_library'}
    name='pho_viet_river_bridge.py' if river else 'pho_viet_saigon_streets.py'
    code=baseline_text(name,'ArtSource/Blender') if old else (ROOT/'ArtSource/Blender'/name).read_text(encoding='utf8')
    if old and river:
        baseline_text('pho_viet_saigon_streets.py','ArtSource/Blender')
        code=code.replace("ROOT+'/ArtSource/Blender/pho_viet_saigon_streets.py'","ROOT+'/.tools/fixture-details-before/pho_viet_saigon_streets.py'")
    exec(compile(code,name,'exec'),ns)
    if pet:
        original=ns['sign']
        def sign(g,spec,obstacles):
            spec=dict(spec)
            for k in ['side','script','icon']:spec.pop(k,None)
            spec.update(main='Cù Lao Petshop',sub='Lưu Trú - Spa Pet',board='blue',ink='white',subink='white')
            original(g,spec,obstacles)
        ns['sign']=sign
        exec(compile((ROOT/'ArtSource/Blender/pho_viet_petshop_layout.py').read_text(encoding='utf8'),'pho_viet_petshop_layout.py','exec'),ns)
        ns['_original_interior']=ns['interior'];ns['interior']=ns['street_petshop_interior'];ns['sidewalk']=ns['street_petshop_sidewalk'];ns['blade_sign']=lambda *a,**k:None
    return ns

def digest(g):return hashlib.sha256(json.dumps([g.v,g.f,g.cols,g.tags,g.hint,getattr(g,'meta',{})]).encode()).hexdigest()
def triangles(g):return sum(len(f)-2 for f in g.f)
def materials(ns):
    atlas=bpy.data.materials.get('PV_StreetAtlas')
    if not atlas:
        atlas=bpy.data.materials.new('PV_StreetAtlas');atlas.use_nodes=True
        tex=atlas.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(ROOT/'Assets/PhoViet/Art/Streets/PV_StreetPalette.png'));tex.interpolation='Closest';tex.image.pack()
        bs=next(n for n in atlas.node_tree.nodes if n.type=='BSDF_PRINCIPLED');atlas.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);bs.inputs['Roughness'].default_value=.65
    glass=bpy.data.materials.get('PV_StreetGlass') or bpy.data.materials.new('PV_StreetGlass');glass.use_nodes=True
    bs=next(n for n in glass.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Base Color'].default_value=(.32,.55,.67,.22);bs.inputs['Alpha'].default_value=.22;bs.inputs['Roughness'].default_value=.18
    ns['MAT']=atlas;ns['GLASS']=glass
    return atlas

def collection():
    coll=bpy.data.collections.get('PV_FixtureDetails_2191')
    if coll is None:coll=bpy.data.collections.new('PV_FixtureDetails_2191');bpy.context.scene.collection.children.link(coll)
    return coll

def setup():
    # Only our newly launched factory-startup Blender; user files are not loaded.
    assert bpy.data.filepath=='' and len(bpy.data.objects)<=3
    for ob in list(bpy.data.objects):bpy.data.objects.remove(ob,do_unlink=True)
    bpy.context.scene.name='PV_FixtureDetails_2191';collection()
    REPORT.write_text(json.dumps({'checks':[],'assets':[]}),encoding='utf8')
    print('Fixture-only Blender MCP scene ready')

def shop_geos(ns,i,pet):
    gs=[ns['shop'](i,d) for d in range(3)]
    for g in gs:
        if pet:g.meta.update(display='Cù Lao Petshop',brand='Cù Lao Petshop',subtitle='Lưu Trú - Spa Pet',original_shop=i,kind='petshop',board='blue',ink='white',pets=g.pet_instances)
        else:g.meta.update(board=ns['SHOPS'][i]['board'],ink=ns['SHOPS'][i]['ink'])
    return gs

def export_shops(indices):
    for pet in (False,True):
        old,new=library(True,pet),library(False,pet);materials(new)
        for i in indices:
            name='PV_StreetShop%02d%s'%(i,'_Petshop' if pet else '')
            before=json.loads(baseline_text(name+'.json','Assets/PhoViet/Art/Streets'))
            a,b=shop_geos(old,i,pet),shop_geos(new,i,pet)
            report_check(name+' installed source baseline',[triangles(g) for g in a]==before['lod_triangles'])
            # Replacing only new AC with old AC must recreate every baseline face/tag/prop.
            current=new['ac'];new['ac']=old['ac'];reference=shop_geos(new,i,pet);new['ac']=current
            report_check(name+' only condenser geometry changes',all(digest(x)==digest(y) for x,y in zip(a,reference)))
            report_check(name+' collisions, pets and metadata preserved',all(x.meta==y.meta for x,y in zip(a,b)))
            info=new['save_asset'](name,b,collection(),((i%4)*10,(i//4+pet*3)*20,0))
            report_check(name+' exported metadata preserved',{k:v for k,v in info.items() if k!='lod_triangles'}=={k:v for k,v in before.items() if k!='lod_triangles'})
            record(name,info['lod_triangles'])
    print('Exported shop indices',indices)

def export_signal():
    ns=library();materials(ns);gs=[ns['traffic_signal'](d) for d in range(3)];coll=collection()
    for ob in list(coll.objects):
        if ob.name.startswith('PV_TrafficSignal'):
            mesh=ob.data;bpy.data.objects.remove(ob,do_unlink=True)
            if mesh and mesh.users==0:bpy.data.meshes.remove(mesh)
    objs=[ns['to_mesh']('PV_TrafficSignal' if d==0 else 'PV_TrafficSignal_LOD'+str(d),g,coll) for d,g in enumerate(gs)]
    for ob in objs[1:]:ob.parent=objs[0]
    ns['export_lod_hierarchy'](str(ROOT/'Assets/PhoViet/Art/Streets/PV_TrafficSignal.fbx'),objs)
    for ob in objs[1:]:ob.hide_render=True;ob.hide_set(True)
    baseline=json.loads(baseline_text('PV_TrafficSignal.json','Assets/PhoViet/Art/Streets'))
    info=dict(baseline,lod_triangles=[triangles(g) for g in gs]);info.update(gs[0].meta)
    report_check('Signal collision and runtime lens sockets unchanged',gs[0].meta=={k:v for k,v in baseline.items() if k in ('obstacles','head_center')})
    report_check('Signal three progressively simplified LODs',info['lod_triangles'][0]>info['lod_triangles'][1]>info['lod_triangles'][2])
    (ROOT/'Assets/PhoViet/Art/Streets/PV_TrafficSignal.json').write_text(json.dumps(info,indent=2,ensure_ascii=False),encoding='utf8')
    objs[0].location=(0,-25,0);record('PV_TrafficSignal',info['lod_triangles']);print(info)

def export_river():
    old,ns=library(True,river=True),library(river=True);materials(ns)
    before=json.loads(baseline_text('PV_RiverApartments.json','Assets/PhoViet/Art/River'))
    parts=ns['apartment_parts']();baseline_parts=old['apartment_parts']()
    report_check('24 original apartment parts retained',[p[0] for p in parts]==[p['name'] for p in before['parts']])
    report_check('Apartment procedural baseline matches installed LOD counts',all([triangles(g) for g in gs]==meta['lod_triangles'] for (name,gs),meta in zip(baseline_parts,before['parts'])))
    levels=[]
    for name,gs in parts:
        signs=[g.rental_signs for g in gs]
        report_check(name+' exactly one sign, all LODs same floor/location',all(len(s)==1 for s in signs) and signs[0]==signs[1]==signs[2])
        levels.append(signs[0][0]['floor'])
    report_check('Rental floors are staggered',len(set(levels))>=3)
    mats=ns['river_materials']();info=ns['export_parts']('PV_RiverApartments',parts,collection(),(0,150,0),mats)
    for fresh,previous in zip(info['parts'],before['parts']):
        report_check(fresh['name']+' materials/shop signs/pets retained',{k:v for k,v in fresh.items() if k not in ('lod_triangles','rental_signs')}=={k:v for k,v in previous.items() if k!='lod_triangles'})
    record('PV_RiverApartments',info['lod_triangles']);print('River parts',len(parts),'rental floors',levels)

def finish():
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_FixtureDetails.blend'))
    r=json.loads(REPORT.read_text(encoding='utf8'));print(json.dumps({'success':r['success'],'checks':len(r['checks']),'assets':len(r['assets'])}))

def geometry_checks():
    # Prove that apartments are unchanged when the two new details are removed.
    old,ns=library(True,river=True),library(river=True)
    def legacy_ac(g,x,y,z,f=-1,scale=1):g.cube((x,y,z),(.7,.5,.42),'white')
    ns['condenser']=legacy_ac;ns['rental_sign']=lambda *a:None
    a,b=old['apartment_parts'](),ns['apartment_parts']()
    for (name,gs),(other,reference) in zip(a,b):
        report_check(name+' entire architecture unchanged outside AC/signs',name==other and all(digest(x)==digest(y) for x,y in zip(gs,reference)))
    ns=library();positions=[];original=ns['ac']
    def inspect(g,x,y,d):
        positions.append([x,y,-4.12]);return original(g,x,y,d)
    ns['ac']=inspect;ns['shop'](0,0)
    (ROOT/'Captures/fixture-details-positions.json').write_text(json.dumps({'shop':'PV_StreetShop00','ac':positions}),encoding='utf8')
    print('Architecture verified; AC positions',positions)
