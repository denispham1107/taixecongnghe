# 21.289 — 14 nhà nổi bật (OW_NhaNoiBat_*, seed 4000–4013, biển thật theo 3 ảnh mẫu) dựng lại bằng shophouse2 (mặt tiền shophouse
# hiện đại đã duyệt) → một FBX tạm Assets/PhoViet/OpenWorld/Library/NhaNoiBat/Src/OW_NhaNoiBat_MatTien21289.fbx (14 lưới
# OW_NhaPho_<seed> ở gốc, cùng hệ trục như lưới nhà trong khối showcase). Unity (AgentScripts/FacadeFeatured21289.cs) cắt tầng
# trệt như menu 10 (BuildFeatured) rồi chép đè Library/NhaNoiBat/OW_NhaPho_<seed>_Kinh.asset (giữ GUID). Biển giữ nguyên trong khối.
# Chạy NỀN:  blender -b --factory-startup --python build_facade_featured.py
import bpy, os, time
OW = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld'
T0 = time.time()
def log(*a): print('[noibat %5.1fs]' % (time.time() - T0), *a, flush=True)
g = {'OW_DIR': OW, 'NO_RUNW': True}
exec(compile(open(OW + '/build_world_map.py', encoding='utf-8').read(), OW + '/build_world_map.py', 'exec'), g)
g['mats']()
exec(compile(open(OW + '/ow_facade.py', encoding='utf-8').read(), OW + '/ow_facade.py', 'exec'), g)
g['facade_mats']()
ex = {'OW_DIR': OW, 'EXP_PARTS': ['none']}
exec(compile(open(OW + '/export_world_unity.py', encoding='utf-8').read(), OW + '/export_world_unity.py', 'exec'), ex)
sc = bpy.context.scene
for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
c = bpy.data.collections.new('NoiBat'); sc.collection.children.link(c)
seed = 4000; keep = []
for rid, s, a, w, sign in g['featured_list']():
    before = set(c.objects)
    s2 = (-w / 2,) + tuple(sign[1:])
    g['shophouse2'](c, -w / 2, w, seed, sign=s2, x_face=0.0)
    for o in set(c.objects) - before:
        if o.type == 'MESH' and o.name == 'OW_NhaPho_%03d' % seed:
            keep.append(o); log(rid, seed, w, repr(sign[2]), sum(len(p.vertices) - 2 for p in o.data.polygons), 'tam giác')
        else:
            bpy.data.objects.remove(o, do_unlink=True)
    seed += 1
out = os.path.join(ex['OUT'], 'Library', 'NhaNoiBat', 'Src'); os.makedirs(out, exist_ok=True)
ex['export_fbx'](keep, os.path.join(out, 'OW_NhaNoiBat_MatTien21289.fbx'))
log('xuất', len(keep), 'lưới')
