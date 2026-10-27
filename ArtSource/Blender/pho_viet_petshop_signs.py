"""21.72: branded facade variants and complete riverside signs, no scene rebuild.
Run in a separate Blender background process; never saves the user's open file.
Only the 12 new shop FBXs, apartment FBX/JSON and one small apartment override
are exported. The shared palette, bridge, trees, pavement and boats are untouched.
"""
import bpy, json, os, random
from pathlib import Path

ROOT = Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe')
SOURCE = ROOT/'ArtSource/Blender'
if not bpy.app.background:
    raise RuntimeError('Run this generator in a separate background Blender process')
scene = bpy.context.scene
for obj in list(scene.objects): bpy.data.objects.remove(obj, do_unlink=True)
scene.name = 'PV_PetshopSigns'
collection = bpy.data.collections.new('PV_PetshopSigns')
scene.collection.children.link(collection)
atlas = bpy.data.materials.new('PV_StreetAtlas'); atlas.use_nodes = True
glass = bpy.data.materials.new('PV_StreetGlass'); glass.use_nodes = True
image = bpy.data.images.load(str(ROOT/'Assets/PhoViet/Art/Streets/PV_StreetPalette.png'))
node = atlas.node_tree.nodes.new('ShaderNodeTexImage'); node.image = image; node.interpolation = 'Closest'
bsdf = next(n for n in atlas.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
atlas.node_tree.links.new(node.outputs['Color'], bsdf.inputs['Base Color'])

street = {'__name__':'petshop_street'}
exec(compile((SOURCE/'pho_viet_saigon_streets.py').read_text(encoding='utf-8'), 'pho_viet_saigon_streets.py', 'exec'), street)
street['MAT'], street['GLASS'] = atlas, glass
original_sign = street['sign']
def petshop_sign(g, spec, obstacles):
    spec = dict(spec)
    for key in ('side', 'script', 'icon'): spec.pop(key, None)
    spec.update(main='Cù Lao Petshop', sub='Lưu Trú - Spa Pet', board='blue', ink='white', subink='white')
    original_sign(g, spec, obstacles)
street['sign'] = petshop_sign
exec(compile((SOURCE/'pho_viet_petshop_layout.py').read_text(encoding='utf-8'),'pho_viet_petshop_layout.py','exec'),street)
street['_original_interior']=street['interior']
street['interior']=street['street_petshop_interior'];street['sidewalk']=street['street_petshop_sidewalk']
# The former blade names (NHÀ NGHỈ/NHA KHOA…) do not belong to a pet shop.
street['blade_sign']=lambda *args,**kwargs:None
results = []
for i in range(12):
    geos = [street['shop'](i, detail) for detail in range(3)]
    for g in geos:g.meta.update(display='Cù Lao Petshop',brand='Cù Lao Petshop',subtitle='Lưu Trú - Spa Pet',original_shop=i,kind='petshop',pets=g.pet_instances)
    results.append(street['save_asset']('PV_StreetShop%02d_Petshop'%i, geos, collection, ((i%6)*9.5, (i//6)*14, 0)))

river = {'__name__':'petshop_river'}
exec(compile((SOURCE/'pho_viet_river_bridge.py').read_text(encoding='utf-8'), 'pho_viet_river_bridge.py', 'exec'), river)
river['MAT'], river['GLASS'] = atlas, glass
mats = river['river_materials']()
parts = river['apartment_parts']()
results.append(river['export_parts']('PV_RiverApartments', parts, collection, (0,60,0), mats))
order = river['petshop_order'](); base = int(len(order)*.3); extra_key = order[base]
part_index = None
for i, (a,b,zf,f,seed) in enumerate(river['riverside_specs']()):
    if round(zf,4)==extra_key[2] and f==extra_key[3] and a-.001<=extra_key[0] and b+.001>=extra_key[1]:
        part_index = i; break
assert part_index is not None
a,b,zf,f,seed = river['riverside_specs']()[part_index]
river['PETSHOP_EXTRA'] = True
extra_geos = []
for detail in range(3):
    g = river['Geo'](detail); river['apartment_block'](g,a,b,zf,f,random.Random(seed),[]); extra_geos.append(g)
extra_part = 'PV_RiverApt%02d'%part_index
results.append(river['export_parts']('PV_RiverPetshopExtra', [(extra_part+'_PetshopExtra',extra_geos)], collection, (0,120,0), mats))
report = dict(success=True,brand='Cù Lao Petshop',board='2B66B0',letters='FBF8F0',river_units=len(order),
              river_base_brands=base,river_extra_brands=base+1,river_extra_part=extra_part,
              river_faces=2,river_signs_per_tile=len(order)*2,
              river_high_blocks=round(len(order)*35*.3)-base*35,
              river_order=[list(k) for k in order],results=results)
(ROOT/'Assets/PhoViet/Art/River/PV_PetshopSigns.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
(ROOT/'Captures/petshop-sign-export.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'PhoViet_PetshopSigns.blend'))
print('PETSHOP_SIGNS_RESULT',json.dumps({k:v for k,v in report.items() if k not in ('results','river_order')},ensure_ascii=False))
