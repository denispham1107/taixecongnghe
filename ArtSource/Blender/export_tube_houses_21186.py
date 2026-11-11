"""Export only the approved tube-house models to .tools/tube-houses-21186."""

import json
import random
from pathlib import Path

import bpy


SOURCE = Path(__file__).resolve().parent
ROOT = SOURCE.parent.parent
STAGE = ROOT / '.tools' / 'tube-houses-21186'


def library():
    module = {'__name__': 'tube_house_export'}
    path = SOURCE / 'upgrade_fixture_details.py'
    exec(compile(path.read_text(encoding='utf-8'), str(path), 'exec'), module)
    return module


def verify(filename, folder, info):
    destination = ROOT / 'Assets' / 'PhoViet' / 'Art' / folder / (filename + '.json')
    installed = json.loads(destination.read_text(encoding='utf-8'))
    fresh = json.loads(json.dumps(info, ensure_ascii=False))
    for prior, current in zip(installed.get('parts', [installed]), fresh.get('parts', [fresh]), strict=True):
        for key, value in prior.items():
            if key != 'lod_triangles' and current[key] != value:
                raise RuntimeError(f'{filename} changed installed {prior.get("name")} {key}')
        counts = current['lod_triangles']
        if not counts[0] > counts[1] > counts[2] > 0:
            raise RuntimeError(f'{filename} invalid LOD triangle counts {counts}')
    return fresh


def main():
    if not bpy.app.background or bpy.data.filepath:
        raise RuntimeError('Run in a fresh background Blender session')
    STAGE.mkdir(parents=True, exist_ok=True)
    for obj in list(bpy.context.scene.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    collection = bpy.data.collections.new('PV_TubeHouses_21186')
    bpy.context.scene.collection.children.link(collection)
    base = library()
    results = {}
    for petshop in (False, True):
        street = base['library'](pet=petshop)
        street['OUT'] = str(STAGE)
        base['materials'](street)
        for i in range(len(street['SHOPS'])):
            name = f'PV_StreetShop{i:02d}' + ('_Petshop' if petshop else '')
            lods = base['shop_geos'](street, i, petshop)
            info = street['save_asset'](name, lods, collection, ((i % 6)*10, (i // 6+petshop*2)*13, 0))
            results[name] = verify(name, 'Streets', info)['lod_triangles']

    river = base['library'](river=True)
    river['OUT'] = str(STAGE)
    base['materials'](river)
    mats = river['river_materials']()
    parts = river['apartment_parts']()
    name = 'PV_RiverApartments'
    info = river['export_parts'](name, parts, collection, (0, 150, 0), mats)
    results[name] = verify(name, 'River', info)['lod_triangles']

    name = 'PV_RiverPetshopExtra'
    installed = json.loads((ROOT / 'Assets/PhoViet/Art/River' / (name+'.json')).read_text(encoding='utf-8'))
    extra_name = installed['parts'][0]['name']
    index = next(i for i, (part, _) in enumerate(parts) if extra_name == part+'_PetshopExtra')
    a, b, z, facing, seed = river['riverside_specs']()[index]
    river['PETSHOP_EXTRA'] = True
    if 'rental_signs' not in installed['parts'][0]:
        # This alternate storefront predates the rental-plaque pass.
        def no_rental_signs(geo, *args):
            geo.rental_signs = []
        river['rental_sign'] = no_rental_signs
    lods = [river['Geo'](detail) for detail in range(3)]
    for geo in lods:
        river['apartment_block'](geo, a, b, z, facing, random.Random(seed), [])
        if not geo.rental_signs:
            del geo.rental_signs
    info = river['export_parts'](name, [(extra_name, lods)], collection, (0, 280, 0), mats)
    results[name] = verify(name, 'River', info)['lod_triangles']
    (STAGE / 'verified.json').write_text(json.dumps(results, indent=2), encoding='utf-8')
    print(f'Verified metadata and LODs for {len(results)} tube-house models.')


if __name__ == '__main__':
    main()
