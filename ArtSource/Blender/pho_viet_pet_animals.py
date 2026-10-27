"""21.73 — dog, cat and rabbit authored in the live Blender through MCP.
The reusable palette geometry is also merged into Petshop facade LODs, avoiding
thousands of extra render objects. Never clears or saves the user's current file.
"""
import bpy, math, json
from pathlib import Path
ROOT = Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe')

def pet_geometry(ns, species, detail):
    g = ns['Geo'](detail); n,m = [(8,4),(6,3),(5,3)][detail]
    fur = {'dog':'woodLight','cat':'orange','rabbit':'white'}[species]
    def blob(p,s,col=fur): g.blob(p,s,col,n=n,m=m,tag='PetSmooth_'+species)
    def rod(a,b,r,col=fur,r2=None): g.cyl(a,b,r,col,n=max(5,n-2),r2=r2,tag='PetSmooth_'+species)
    if species == 'dog':
        blob((0,.37,.055),(.16,.19,.34)); blob((0,.48,-.23),(.12,.17,.14))
        blob((0,.63,-.32),(.16,.16,.15)); blob((0,.57,-.46),(.12,.085,.12),'cream')
        blob((0,.6,-.565),(.055,.039,.028),'rubber')
        blob((0,.43,.12),(.164,.145,.20),'wood')
        for side in (-1,1):
            for z in (-.22,.25):
                rod((side*.105,.35,z),(side*.11,.07,z-.015),.045,r2=.035)
                blob((side*.11,.05,z-.05),(.065,.048,.09),'cream')
            blob((side*.16,.58,-.27),(.057,.15,.085),'woodDark')
            blob((side*.105,.67,-.431),(.026,.03,.018),'rubber')
            if detail<2: blob((side*.103,.679,-.446),(.008,.008,.006),'white')
        blob((0,.48,-.22),(.127,.032,.143),'blue')
        tail=[(0,.43,.35),(.02,.52,.44),(.07,.65,.48),(.1,.72,.43)]
        if detail==2: tail=tail[::2]+[tail[-1]]
        for i in range(len(tail)-1): rod(tail[i],tail[i+1],.036-i*.007,r2=.025-i*.006)
    elif species == 'cat':
        blob((0,.23,.12),(.15,.23,.17)); blob((0,.41,.005),(.115,.20,.14))
        blob((0,.65,-.09),(.153,.14,.128))
        for side in (-1,1):
            rod((side*.074,.36,-.085),(side*.075,.065,-.16),.035,r2=.03)
            blob((side*.075,.044,-.19),(.06,.04,.085),'cream')
            vs=[(side*.035,.73,-.15),(side*.16,.72,-.12),(side*.12,.91,-.085),
                (side*.035,.73,-.025),(side*.16,.72,-.025)]
            g.add(vs,[(0,1,2),(3,2,4),(0,2,3),(1,4,2),(0,3,4,1)],fur,('c',(side*.10,.76,-.075)),'PetEar_cat')
            g.add([(side*.064,.752,-.155),(side*.137,.747,-.128),(side*.115,.868,-.10)],[(0,1,2)],'pinkLight',('d',(0,0,-1)),'PetEar_cat')
            blob((side*.085,.671,-.195),(.038,.028,.017),'leaf4')
            blob((side*.085,.671,-.21),(.01,.025,.007),'rubber')
            blob((side*.04,.6,-.205),(.046,.035,.03),'cream')
            if detail==0:
                for dy in (-.017,.015): rod((side*.045,.596+dy,-.23),(side*.225,.59+dy*2,-.225),.003,'white',r2=.001)
        blob((0,.62,-.237),(.019,.014,.01),'pink')
        blob((0,.494,-.06),(.119,.025,.133),'red')
        tail=[(0,.18,.25),(.16,.09,.34),(.255,.067,.21),(.24,.08,.015),(.19,.12,-.06)]
        if detail==2: tail=[tail[0],tail[2],tail[-1]]
        for i in range(len(tail)-1): rod(tail[i],tail[i+1],.035-i*.005,r2=.028-i*.005)
    else:
        blob((0,.2,.08),(.15,.18,.23)); blob((0,.36,-.16),(.125,.12,.115))
        for side in (-1,1):
            blob((side*.12,.048,.2),(.08,.044,.115),'cream')
            blob((side*.065,.04,-.18),(.055,.036,.085),'cream')
            blob((side*.065,.61,-.13),(.046,.20,.037))
            blob((side*.065,.615,-.165),(.021,.145,.012),'pinkLight')
            blob((side*.09,.393,-.238),(.023,.028,.017),'rubber')
            if detail<2: blob((side*.091,.402,-.25),(.007,.008,.005),'white')
            blob((side*.035,.32,-.259),(.042,.031,.033),'cream')
        blob((0,.337,-.287),(.018,.013,.01),'pink')
        blob((0,.255,.332),(.065,.066,.065))
    low=min(p[1] for p in g.v);g.v=[(x,y-low,z) for x,y,z in g.v]
    # Hint centres must follow the same tiny grounding shift.
    g.hint=[(kind,(p[0],p[1]-low,p[2]) if kind=='c' else p) for kind,p in g.hint]
    g.meta=dict(generator='pho_viet_pet_animals.py / Blender MCP',species=species,obstacles=[])
    return g

def build_pet_animals():
    ns={'__name__':'pet_animal_geometry'}
    exec(compile((ROOT/'ArtSource/Blender/pho_viet_saigon_streets.py').read_text(encoding='utf-8'),'pho_viet_saigon_streets.py','exec'),ns)
    ns['OUT']=str(ROOT/'Assets/PhoViet/Art/Pets');Path(ns['OUT']).mkdir(parents=True,exist_ok=True)
    scene=bpy.data.scenes.get('PV_PetAnimals_Work') or bpy.data.scenes.new('PV_PetAnimals_Work')
    bpy.context.window.scene=scene
    collection=bpy.data.collections.get('PV_PetAnimals_Work') or bpy.data.collections.new('PV_PetAnimals_Work')
    if collection.name not in scene.collection.children:scene.collection.children.link(collection)
    for obj in list(collection.objects):
        data=obj.data;bpy.data.objects.remove(obj,do_unlink=True)
        if isinstance(data,bpy.types.Mesh) and data.users==0:bpy.data.meshes.remove(data)
    atlas=bpy.data.materials.get('PV_PetAtlas') or bpy.data.materials.new('PV_PetAtlas');atlas.use_nodes=True
    node=next((n for n in atlas.node_tree.nodes if n.type=='TEX_IMAGE'),None) or atlas.node_tree.nodes.new('ShaderNodeTexImage')
    node.image=bpy.data.images.load(str(ROOT/'Assets/PhoViet/Art/Streets/PV_StreetPalette.png'),check_existing=True);node.interpolation='Closest'
    bsdf=next(n for n in atlas.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bsdf.inputs['Roughness'].default_value=.82
    atlas.node_tree.links.new(node.outputs['Color'],bsdf.inputs['Base Color']);ns['MAT']=atlas
    models={};counts={}
    for index,species in enumerate(('dog','cat','rabbit')):
        levels=[pet_geometry(ns,species,d) for d in range(3)]
        info=ns['save_asset']('PV_Pet'+species.title(),levels,collection,(index*1.2,0,0));counts[species]=info['lod_triangles']
        models[species]=[dict(v=g.v,f=g.f,cols=g.cols,hint=g.hint,tags=g.tags) for g in levels]
    data=dict(generator='Blender MCP / pho_viet_pet_animals.py',models=models,triangles=counts)
    (Path(ns['OUT'])/'PV_PetGeometry.json').write_text(json.dumps(data,separators=(',',':')),encoding='utf-8')
    ids={scene,collection,atlas}|set(collection.objects)|{o.data for o in collection.objects if o.data}
    bpy.data.libraries.write(str(ROOT/'ArtSource/Blender/PhoViet_PetAnimals.blend'),ids,fake_user=True)
    (ROOT/'Captures/pet-animals-blender.json').write_text(json.dumps(dict(success=True,via='Blender MCP',triangles=counts),indent=2),encoding='utf-8')
    print(json.dumps(dict(success=True,scene=scene.name,triangles=counts)))
