"""21.87: export only the CURRENT bridge's three LODs. Never run the river installer.
Run in a separate Blender --background --factory-startup process, not a user's live scene.
Preserves banks/water/houses/boats/palette, FBX hierarchy/material order and scene GUIDs.
"""
import bpy,json,hashlib,traceback,subprocess
from pathlib import Path
ROOT=Path(r'C:/Users/HP/Documents/GameUnity/Taixecongnghe')
CAP=ROOT/'Captures';CAP.mkdir(exist_ok=True)
REPORT=CAP/'bridge-fixtures-blender-checks.json'
CHECKS=[]
def check(name,value):
    CHECKS.append({'name':name,'passed':bool(value)})
    if not value:raise RuntimeError(name)
def library(path):
    ns={'__name__':'bridge_fixture_library'}
    code=path.read_text(encoding='utf8') if path.exists() else subprocess.check_output(['git','show','92c4348:ArtSource/Blender/pho_viet_river_bridge.py'],cwd=ROOT).decode('utf8')
    exec(compile(code,str(path),'exec'),ns)
    return ns
def digest(g,filter_col=None):
    faces=[]
    for f,col,tag,hint in zip(g.f,g.cols,g.tags,g.hint):
        if filter_col is not None and col not in filter_col:continue
        faces.append((tuple(tuple(round(float(v),7) for v in g.v[i]) for i in f),col,tag,hint))
    return hashlib.sha256(json.dumps(faces,sort_keys=True).encode()).hexdigest()
def tris(g):return sum(len(f)-2 for f in g.f)
try:
    old=library(ROOT/'.tools/bridge-fixtures-before-generator.py')
    new=library(ROOT/'ArtSource/Blender/pho_viet_river_bridge.py')
    baseline=ROOT/'.tools/bridge-fixtures-before.json'
    installed=json.loads(baseline.read_text(encoding='utf8') if baseline.exists() else subprocess.check_output(['git','show','92c4348:Assets/PhoViet/Art/River/PV_RiverBridge.json'],cwd=ROOT).decode('utf8'))
    old_geos=[old['bridge_tile'](d) for d in range(3)]
    check('Source baseline matches installed three LOD triangle counts',[tris(g) for g in old_geos]==installed['lod_triangles'])
    # Prove that water/banks/walls/pavement and collision metadata are byte-equivalent.
    old_deck,new_deck=old['deck'],new['deck']
    old['deck']=lambda g:None;new['deck']=lambda g:None
    for d in range(3):
        a,b=old['bridge_tile'](d),new['bridge_tile'](d)
        check('All non-deck geometry unchanged LOD'+str(d),digest(a)==digest(b))
        check('Non-deck metadata unchanged LOD'+str(d),a.meta==b.meta)
    old['deck'],new['deck']=old_deck,new_deck
    # Rail/deck/piers also stay exact; ignore just the removed flowers and replaced flag fixtures.
    old_flag,new_flag=old['lamp_flag'],new['lamp_flag']
    old['lamp_flag']=lambda *args:None;new['lamp_flag']=lambda *args:None
    old['flower_box']=lambda *args:None
    new['flower_box']=lambda *args:(_ for _ in ()).throw(RuntimeError('Bridge flowers must never be generated'))
    for d in range(3):
        a,b=old['Geo'](d),new['Geo'](d);old_deck(a);new_deck(b)
        check('Deck/rails/piers exact apart from fixtures LOD'+str(d),digest(a)==digest(b))
    old['lamp_flag'],new['lamp_flag']=old_flag,new_flag
    for d in range(3):
        for side in [-1,1]:
            a,b=old['Geo'](d),new['Geo'](d);old_flag(a,side,10)
            calls=[];cyl=b.cyl
            def record(start,end,r,col,**kw):calls.append((start,end,r,col));return cyl(start,end,r,col,**kw)
            b.cyl=record;new_flag(b,side,10)
            red_gold={new['CI']['flagRed'],new['CI']['flagGold']}
            check('Flag and both golden stars preserved LOD%d side%d'%(d,side),digest(a,red_gold)==digest(b,red_gold))
            arms=[x for x in calls if abs(x[0][0]-x[1][0])>.5]
            check('Exactly two green horizontal arms LOD%d side%d'%(d,side),len(arms)==2 and all(abs(x[0][1]-x[1][1])<1e-6 and x[3]=='awnGreen' for x in arms))
            check('Arms do not cross red cloth LOD%d side%d'%(d,side),all(x[0][1]+x[2]<4.28 or x[0][1]-x[2]>4.92 for x in arms))
    geos=[new['bridge_tile'](d) for d in range(3)]
    for key,value in old_geos[0].meta.items():check('Installed metadata retained: '+key,installed.get(key)==value)
    # This process owns a factory-startup scene; no user .blend is opened or overwritten.
    for ob in list(bpy.data.objects):bpy.data.objects.remove(ob,do_unlink=True)
    sc=bpy.context.scene;sc.name='PV_BridgeFixtures_2187'
    coll=bpy.data.collections.new('PV_BridgeFixtures_2187');sc.collection.children.link(coll)
    mat=bpy.data.materials.new('PV_StreetAtlas');mat.use_nodes=True
    bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Roughness'].default_value=.78
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(ROOT/'Assets/PhoViet/Art/Streets/PV_StreetPalette.png'));tex.interpolation='Closest'
    mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);tex.image.pack()
    new['MAT']=mat;new['GLASS']=bpy.data.materials.new('PV_StreetGlass')
    info=new['export']('PV_RiverBridge',geos,coll,(0,0,0),new['river_materials']())
    check('Material slot order preserved',info['slots']==installed['slots'] and info['blender_slots']==installed['blender_slots'])
    check('Exporter triangle counts match geometry',info['lod_triangles']==[tris(g) for g in geos])
    info['bridge_fixtures']={'flower_boxes':0,'flags':8,'supports_per_flag':2,'support_material':'awnGreen','style':'horizontal arms above and below cloth, clamped to lamp pole','revision':'21.87'}
    (ROOT/'Assets/PhoViet/Art/River/PV_RiverBridge.json').write_text(json.dumps(info,ensure_ascii=False,indent=2),encoding='utf8')
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_BridgeFixtures.blend'))
    report={'success':True,'checks':CHECKS,'passed':len(CHECKS),'failed':0,'old_triangles':installed['lod_triangles'],'new_triangles':info['lod_triangles'],'fixtures':info['bridge_fixtures']}
    REPORT.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
    print(json.dumps({'success':True,'checks':len(CHECKS),'old':installed['lod_triangles'],'new':info['lod_triangles']}))
except Exception:
    REPORT.write_text(json.dumps({'success':False,'checks':CHECKS,'error':traceback.format_exc()},ensure_ascii=False,indent=2),encoding='utf8')
    raise
