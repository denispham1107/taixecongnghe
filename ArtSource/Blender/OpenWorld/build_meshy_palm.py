# 21.286 — cây dừa Meshy (người dùng gửi qua Meshy Bridge, 7,5 triệu tam giác) + bệ P2 cũ (build_coconut_palm.planter_p2_aged)
# → mẫu cây cho bản đồ mở.
# Nhập FBX 309 MB trong Blender hết bộ nhớ (ArrayMemoryError) nên lưới được Unity xuất ra tệp nhị phân gọn
# (ArtSource/MeshyRaw/<thư mục>/mesh_unity.bin: int32 nv, nt, nuv; float32 vị trí (Unity, Y lên, đã nhân xoay/tỉ lệ của FBX);
# float32 UV; int32 tam giác). Thư mục MeshyRaw bị .gitignore — giữ cục bộ.
# Chạy NỀN:  blender -b --factory-startup --python build_meshy_palm.py -- [inspect|build]
# Ra:  Assets/PhoViet/Art/Streets/CayDua/PV_CayDua.fbx  (CayDua_LOD0..2 = thân + tán có UV, BeCay_LOD0..2 = bệ màu đỉnh)
#      + PV_CayDua_Albedo.png (texture Meshy 2048).
# Bước: (1) đọc lưới bằng numpy; (2) cắt bệ đất dưới gốc (người dùng khoanh đỏ): bỏ mọi tam giác có tâm thấp hơn mặt trên
#       của bệ đất (đỉnh cao nhất của phần ở xa thân trong 20 % chiều cao dưới); (3) gốc về (0,0), cao CAO_CAY m;
#       (4) giảm lưới theo từng khối (thân + 8 múi tán) cho đỡ tốn bộ nhớ, ghép lại, rồi tạo LOD; (5) bệ P2 cũ + bản giảm; (6) xuất.
import bpy, sys, os, math, time
import numpy as np
ROOT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
RAW = ROOT + '/ArtSource/MeshyRaw/Golden_Palm_Tree_20261120_215552'
OUTDIR = ROOT + '/Assets/PhoViet/Art/Streets/CayDua'
CAO_CAY = 9.6
PRE = 400000
TARGETS = (60000, 16000, 4000)            # tổng mỗi LOD; thân giữ riêng TRUNK_T, tán phần còn lại
TRUNK_T = (6000, 1800, 500)
args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else ['inspect']
T0 = time.time()
def log(*a): print('[caydua %5.1fs]' % (time.time() - T0), *a, flush=True)

with open(RAW + '/mesh_unity.bin', 'rb') as f:
    nv, nt, nuv = (int(x) for x in np.frombuffer(f.read(12), np.int32))
    P = np.frombuffer(f.read(nv * 12), np.float32).reshape(-1, 3)
    UV = np.frombuffer(f.read(nuv * 8), np.float32).reshape(-1, 2)
    TRI = np.frombuffer(f.read(nt * 12), np.int32).reshape(-1, 3)
# Unity (x, y lên, z) → Blender (x, z, y lên): phép đối xứng gương bù đúng cho hệ tay trái → tay phải, giữ chiều quấn tam giác
co = np.stack([P[:, 0], P[:, 2], P[:, 1]], 1).astype(np.float32); del P
zmin = float(co[:, 2].min()); zmax = float(co[:, 2].max()); H = zmax - zmin
band = co[(co[:, 2] > zmin + 0.06 * H) & (co[:, 2] < zmin + 0.10 * H)]
cx, cy = float(np.median(band[:, 0])), float(np.median(band[:, 1]))
r_trunk = float(np.percentile(np.hypot(band[:, 0] - cx, band[:, 1] - cy), 90))
rad = np.hypot(co[:, 0] - cx, co[:, 1] - cy)
far = co[(rad > 1.45 * r_trunk) & (co[:, 2] < zmin + 0.2 * H)]
z_cut = float(far[:, 2].max()) + 0.002 * H
log('nv', nv, 'nt', nt, 'cao', round(H, 3), 'thân tâm', round(cx, 3), round(cy, 3), 'r', round(r_trunk, 3),
    'bệ đất tới', round((z_cut - zmin) / H, 4), 'H')
if args[0] == 'inspect':
    sys.exit(0)

# (2) cắt bệ đất: bỏ tam giác có tâm dưới z_cut
cz = co[TRI, 2].mean(1)
TRI = TRI[cz > z_cut]; del cz
log('cắt bệ đất: còn', len(TRI), 'tam giác (bỏ', nt - len(TRI), ')')
# (3) gốc về (0,0), z=0 tại mặt cắt, cao CAO_CAY
s = CAO_CAY / (zmax - z_cut)
co[:, 0] -= cx; co[:, 1] -= cy; co[:, 2] -= z_cut; co *= s
rad = np.hypot(co[:, 0], co[:, 1]); r_trunk *= s

def build(name, tri):
    """Lưới Blender từ mảng numpy (chỉ giữ đỉnh dùng tới)."""
    used, inv = np.unique(tri.ravel(), return_inverse=True)
    me = bpy.data.meshes.new(name)
    me.vertices.add(len(used)); me.vertices.foreach_set('co', co[used].ravel())
    me.loops.add(len(tri) * 3); me.loops.foreach_set('vertex_index', inv.astype(np.int32))
    me.polygons.add(len(tri)); me.polygons.foreach_set('loop_start', np.arange(0, len(tri) * 3, 3, dtype=np.int32))
    me.update(calc_edges=True)
    uvl = me.uv_layers.new(name='UVMap'); uvl.data.foreach_set('uv', UV[tri.ravel()].ravel())
    me.polygons.foreach_set('use_smooth', np.ones(len(tri), bool))
    ob = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(ob)
    return ob

def decimate(o, target, uv_delimit=True):
    n = len(o.data.polygons)
    if n > target:
        md = o.modifiers.new('Giam', 'DECIMATE'); md.decimate_type = 'COLLAPSE'; md.ratio = max(0.0005, target / n)
        md.use_collapse_triangulate = True
        if uv_delimit:
            try: md.delimit = {'UV'}
            except Exception: pass
        bpy.context.view_layer.objects.active = o
        bpy.ops.object.select_all(action='DESELECT'); o.select_set(True)
        bpy.ops.object.modifier_apply(modifier=md.name)
    return o

# (4) giảm theo khối: thân (gần trục, dưới 45 % chiều cao) + 8 múi tán theo góc
tc = co[TRI].mean(1)
is_trunk = (np.hypot(tc[:, 0], tc[:, 1]) < 2.2 * r_trunk) & (tc[:, 2] < 0.45 * CAO_CAY)
ang = ((np.arctan2(tc[:, 1], tc[:, 0]) + math.pi) / (2 * math.pi) * 8).astype(np.int32) % 8
ang[is_trunk] = -1; del tc
ratio = PRE / len(TRI); parts = []; trunk_o = None
for k in range(-1, 8):
    sub = TRI[ang == k]
    if not len(sub): continue
    o = build('Khoi_%d' % k, sub)
    decimate(o, len(sub) if k < 0 else max(200, int(len(sub) * ratio)))     # thân giữ nguyên ở bước này
    log('khối', k, len(sub), '→', len(o.data.polygons))
    if k < 0: trunk_o = o
    else: parts.append(o)
del TRI, ang
def join(objs, name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]; bpy.ops.object.join()
    o = bpy.context.view_layer.objects.active; o.name = name
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.remove_doubles(threshold=0.0005); bpy.ops.object.mode_set(mode='OBJECT')
    return o
crown = join(parts, 'Tan_Tam')
log('tán', len(crown.data.polygons), 'thân', len(trunk_o.data.polygons))
# kiểm chiều pháp tuyến bằng thân cây: phần lớn mặt thân phải hướng ra ngoài trục
me = trunk_o.data; n = len(me.polygons)
cen = np.empty(n * 3, np.float32); me.polygons.foreach_get('center', cen); cen = cen.reshape(-1, 3)
nor = np.empty(n * 3, np.float32); me.polygons.foreach_get('normal', nor); nor = nor.reshape(-1, 3)
m = (cen[:, 2] > 0.1 * CAO_CAY) & (cen[:, 2] < 0.4 * CAO_CAY)
out = float(((cen[m, 0] * nor[m, 0] + cen[m, 1] * nor[m, 1]) > 0).mean()) if m.any() else 1.0
log('thân: tỉ lệ mặt hướng ra ngoài', round(out, 3))
if out < 0.5:
    for o in (trunk_o, crown):
        bpy.context.view_layer.objects.active = o; bpy.ops.object.select_all(action='DESELECT'); o.select_set(True)
        bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT'); bpy.ops.mesh.flip_normals()
        bpy.ops.object.mode_set(mode='OBJECT')
    log('đã lật pháp tuyến')

def dup(src, name):
    o = src.copy(); o.data = src.data.copy(); o.name = name; bpy.context.scene.collection.objects.link(o); return o
lods = []
for i, (t, tt) in enumerate(zip(TARGETS, TRUNK_T)):
    a = decimate(dup(trunk_o, 'th%d' % i), tt, uv_delimit=i < 2)
    b = decimate(dup(crown, 'ta%d' % i), t - tt, uv_delimit=i < 2)
    o = join([a, b], 'CayDua_LOD%d' % i); log(o.name, len(o.data.polygons)); lods.append(o)
bpy.data.objects.remove(trunk_o, do_unlink=True); bpy.data.objects.remove(crown, do_unlink=True)

# texture
os.makedirs(OUTDIR, exist_ok=True)
import shutil
shutil.copyfile(RAW + '/meshy_basecolor.png', OUTDIR + '/PV_CayDua_Albedo.png')
mat = bpy.data.materials.new('PV_CayDua')
for o in lods: o.data.materials.append(mat)

# (5) bệ P2 cũ
ns = {'__name__': 'caydua'}
exec(open(ROOT + '/ArtSource/Blender/OpenWorld/build_coconut_palm.py', encoding='utf-8').read(), ns)
be, soil = ns['planter_p2_aged'](loc=(0, 0, 0), seed=7)
be.name = 'BeCay_LOD0'
bpy.context.view_layer.objects.active = be
for o in lods: o.location.z = soil - 0.05      # cắm gốc xuống lòng bệ 5 cm (giấu mép cắt)
def copy_dec(src, t, name):
    o = src.copy(); o.data = src.data.copy(); o.name = name; bpy.context.scene.collection.objects.link(o)
    return decimate(o, t, uv_delimit=False)
b1 = copy_dec(be, 2600, 'BeCay_LOD1'); b2 = copy_dec(be, 700, 'BeCay_LOD2')
log('bệ', len(be.data.polygons), len(b1.data.polygons), len(b2.data.polygons), 'đất z', round(soil, 3))

# (6) xuất
objs = lods + [be, b1, b2]
bpy.ops.object.select_all(action='DESELECT')
for o in objs: o.select_set(True)
bpy.context.view_layer.objects.active = objs[0]
bpy.ops.export_scene.fbx(filepath=OUTDIR + '/PV_CayDua.fbx', use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y',
                         apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False,
                         mesh_smooth_type='OFF', colors_type='LINEAR', path_mode='STRIP')
log('xuất', OUTDIR + '/PV_CayDua.fbx')
_blend = ROOT + '/ArtSource/Blender/OpenWorld/PhoViet_OW_CayDuaMeshy_21286.blend'
if not os.path.exists(_blend): bpy.ops.wm.save_as_mainfile(filepath=_blend)     # không ghi đè tệp .blend đã có
