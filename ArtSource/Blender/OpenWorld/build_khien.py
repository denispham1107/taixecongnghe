"""21.312 — Khiên năng lượng + hiệu ứng vỡ lưới điện CSGT (dựng bằng Blender MCP, người dùng 22/11/2026).

Chạy trong Blender (MCP execute_blender_code):  exec(open(r'<repo>/ArtSource/Blender/OpenWorld/build_khien.py', encoding='utf-8').read())
Dựng trong scene riêng «Khien» (không đụng scene khác), lưu file mới PhoViet_OW_Khien.blend, xuất 2 FBX:
  Assets/PhoViet/Resources/Khien/PV_Khien.fbx   — vỏ khiên (elip trơn), khung lục giác (viền ô), ô lục giác (mặt ô),
                                                  6 tia năng lượng (tia sét lởm chởm có nhánh), vành sáng dưới đất.
  Assets/PhoViet/Resources/Khien/PV_LuoiVo.fbx  — 10 mảnh lưới thoi rách (dây lõi + quầng, đầu dây đứt tua ra),
                                                  vòng sóng xung kích.
Quy ước UV cho shader Unity:
  • Tia/dây (ribbon chữ thập): UV0.x = chiều dài cộng dồn (m), UV0.y = 0..1 ngang dải (shader Electric mode 0).
  • Khung lục giác: UV0 = (0 ở mép ngoài ô → 1 ở mép trong, 0); UV1 = (số ngẫu nhiên của ô, độ cao 0..1).
  • Ô lục giác: UV0 = (khoảng cách tới tâm ô 0..1, 0); UV1 như trên.
Trục: Blender X = ngang xe, Y = đầu xe, Z = lên. Gốc = giữa đáy xe.
"""
import bpy, bmesh, math, random, os
from mathutils import Vector

REPO = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT_DIR = REPO + '/Assets/PhoViet/Resources/Khien'
BLEND = REPO + '/ArtSource/Blender/OpenWorld/PhoViet_OW_Khien.blend'

# ---------------------------------------------------------------- scene riêng
sc = bpy.data.scenes.get('Khien') or bpy.data.scenes.new('Khien')
bpy.context.window.scene = sc
for o in list(sc.collection.all_objects):
    bpy.data.objects.remove(o, do_unlink=True)

def coll(name):
    c = bpy.data.collections.get(name)
    if not c:
        c = bpy.data.collections.new(name); sc.collection.children.link(c)
    for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
    return c

def mat(name, rgb, emit=4.0, alpha=0.6):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    b = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    b.inputs['Base Color'].default_value = (*rgb, 1)
    for key in ('Emission Color', 'Emission'):
        if key in b.inputs: b.inputs[key].default_value = (*rgb, 1); break
    if 'Emission Strength' in b.inputs: b.inputs['Emission Strength'].default_value = emit
    b.inputs['Alpha'].default_value = alpha
    try: m.surface_render_method = 'BLENDED'
    except Exception: pass
    return m

TEAL = (0.20, 0.94, 0.82)
NETBLUE = (0.45, 0.80, 1.0)
M_SHELL = mat('PV_Khien_Vo', TEAL, 1.5, 0.15)
M_FRAME = mat('PV_Khien_Khung', TEAL, 6, 0.9)
M_CELL = mat('PV_Khien_O', TEAL, 2, 0.25)
M_BOLT = mat('PV_Khien_Tia', (0.75, 1.0, 0.95), 10, 1)
M_RING = mat('PV_Khien_Vanh', TEAL, 5, 0.7)
M_NCORE = mat('PV_LuoiVo_Loi', (0.85, 0.95, 1.0), 8, 1)
M_NHALO = mat('PV_LuoiVo_Quang', NETBLUE, 3, 0.5)
M_WAVE = mat('PV_LuoiVo_Song', NETBLUE, 5, 0.7)

def make_obj(name, bm, mats, col, uv_layers=('UVMap',)):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    for m in mats: me.materials.append(m)
    o = bpy.data.objects.new(name, me); col.objects.link(o)
    return o

# ---------------------------------------------------------------- khiên
A = Vector((1.55, 2.65, 1.30))     # bán trục: ngang, dọc xe, đứng
C = Vector((0, 0, 0.55))           # tâm (xe cao ~1,4 m, tâm thân 0,65)
def ell(p):                         # điểm trên mặt cầu đơn vị → mặt elip
    p = p.normalized(); return Vector((p.x * A.x, p.y * A.y, p.z * A.z)) + C
def ell_n(p):
    p = p.normalized(); n = Vector((p.x / A.x, p.y / A.y, p.z / A.z)); return n.normalized()

col_k = coll('Khien')
random.seed(312)

# vỏ trơn (fresnel ở Unity): UV sphere 64×32
bm = bmesh.new()
bmesh.ops.create_uvsphere(bm, u_segments=64, v_segments=32, radius=1.0)
for v in bm.verts: v.co = ell(v.co)
for f in bm.faces: f.smooth = True
o_shell = make_obj('Khien_Vo', bm, [M_SHELL], col_k)

# lưới lục giác: đối ngẫu của icosphere chia 3 lần (642 ô, 12 ngũ giác)
ico = bmesh.new()
bmesh.ops.create_icosphere(ico, subdivisions=3, radius=1.0)
ico.verts.ensure_lookup_table(); ico.faces.ensure_lookup_table()
cells = []
for v in ico.verts:
    n = v.co.normalized()
    t1 = n.orthogonal().normalized(); t2 = n.cross(t1)
    cs = [f.calc_center_median().normalized() for f in v.link_faces]
    cs.sort(key=lambda c: math.atan2(c.dot(t2), c.dot(t1)))
    cells.append((n, cs))
ico.free()

bmF = bmesh.new(); uvF = bmF.loops.layers.uv.new('UVMap'); uvF2 = bmF.loops.layers.uv.new('UV2')
bmC = bmesh.new(); uvC = bmC.loops.layers.uv.new('UVMap'); uvC2 = bmC.loops.layers.uv.new('UV2')
INNER = 0.80                         # viền mỗi ô = 20 % bán kính ô (hai ô kề → vạch đôi)
zmin = C.z - A.z; zmax = C.z + A.z
for (n, ring) in cells:
    rnd = random.random()
    cpos = ell(n); h = (cpos.z - zmin) / (zmax - zmin)
    outer = [ell(p) for p in ring]
    inner = [ell(n.lerp(p, INNER)) for p in ring]
    k = len(ring)
    vo = [bmF.verts.new(p) for p in outer]; vi = [bmF.verts.new(p) for p in inner]
    for i in range(k):
        j = (i + 1) % k
        f = bmF.faces.new((vo[i], vo[j], vi[j], vi[i]))
        for l, uvx in zip(f.loops, (0, 0, 1, 1)):
            l[uvF].uv = (uvx, 0); l[uvF2].uv = (rnd, h)
        f.smooth = True
    # mặt ô (quạt tam giác từ tâm)
    vc = bmC.verts.new(cpos); vr = [bmC.verts.new(p) for p in inner]
    for i in range(k):
        j = (i + 1) % k
        f = bmC.faces.new((vc, vr[i], vr[j]))
        for l, uvx in zip(f.loops, (0, 1, 1)):
            l[uvC].uv = (uvx, 0); l[uvC2].uv = (rnd, h)
for bmx in (bmF, bmC):
    bmesh.ops.recalc_face_normals(bmx, faces=bmx.faces)
o_frame = make_obj('Khien_Khung', bmF, [M_FRAME], col_k)
o_cell = make_obj('Khien_O', bmC, [M_CELL], col_k)

# tia năng lượng: tia sét lởm chởm dọc +Y (dài 1), chuẩn hóa để Unity co giãn; dải chữ thập, UV0.x = chiều dài, UV0.y = ngang
def bolt_path(p0, p1, segs, jag, rng):
    pts = [p0]
    d = (p1 - p0); L = d.length; dn = d.normalized()
    s1 = dn.orthogonal().normalized(); s2 = dn.cross(s1)
    for i in range(1, segs):
        t = i / segs
        amp = jag * L * math.sin(math.pi * t) ** 0.6
        pts.append(p0 + d * t + s1 * rng.uniform(-amp, amp) + s2 * rng.uniform(-amp, amp) * 0.6)
    pts.append(p1); return pts

def ribbon(bm, uvl, pts, w0, w1):
    along = 0.0
    for i in range(len(pts) - 1):
        a, b = pts[i], pts[i + 1]; d = b - a; L = d.length
        if L < 1e-5: continue
        dn = d / L
        up = Vector((0, 0, 1)) if abs(dn.z) < 0.9 else Vector((1, 0, 0))
        o1 = dn.cross(up).normalized(); o2 = dn.cross(o1).normalized()
        wa = w0 + (w1 - w0) * (i / (len(pts) - 1)); wb = w0 + (w1 - w0) * ((i + 1) / (len(pts) - 1))
        for off in (o1, o2):
            q = [bm.verts.new(a - off * wa), bm.verts.new(a + off * wa), bm.verts.new(b + off * wb), bm.verts.new(b - off * wb)]
            f = bm.faces.new(q)
            for l, uv in zip(f.loops, ((along, 0), (along, 1), (along + L, 1), (along + L, 0))):
                l[uvl].uv = uv
        along += L

for vi_ in range(6):
    rng = random.Random(1000 + vi_)
    bm = bmesh.new(); uvl = bm.loops.layers.uv.new('UVMap')
    main = bolt_path(Vector((0, 0, 0)), Vector((0, 1, 0)), 10 + vi_ % 3 * 2, 0.16, rng)
    ribbon(bm, uvl, main, 0.022, 0.010)
    for b in range(1 + vi_ % 2 + (1 if vi_ >= 4 else 0)):
        k = rng.randint(2, len(main) - 4); p0 = main[k]
        dirv = (Vector((rng.uniform(-1, 1), 0.6, rng.uniform(-1, 1)))).normalized()
        br = bolt_path(p0, p0 + dirv * rng.uniform(0.25, 0.45), 5, 0.22, rng)
        ribbon(bm, uvl, br, 0.012, 0.004)
    make_obj('Khien_Tia%d' % vi_, bm, [M_BOLT], col_k)

# vành sáng dưới đất (dải vòng elip phẳng, UV0.y = 0 ngoài → 1 trong)
bm = bmesh.new(); uvl = bm.loops.layers.uv.new('UVMap')
N = 96; outer = []; inner = []
for i in range(N):
    a = 2 * math.pi * i / N
    outer.append(bm.verts.new((math.cos(a) * A.x * 1.08, math.sin(a) * A.y * 1.05, 0.03)))
    inner.append(bm.verts.new((math.cos(a) * A.x * 0.78, math.sin(a) * A.y * 0.82, 0.03)))
for i in range(N):
    j = (i + 1) % N
    f = bm.faces.new((outer[i], outer[j], inner[j], inner[i]))
    for l, uv in zip(f.loops, ((i / N, 0), ((i + 1) / N, 0), ((i + 1) / N, 1), (i / N, 1))): l[uvl].uv = uv
make_obj('Khien_Vanh', bm, [M_RING], col_k)

# ---------------------------------------------------------------- mảnh lưới vỡ
col_n = coll('LuoiVo')
def net_piece(idx):
    rng = random.Random(2000 + idx)
    cell = 0.30; nu = rng.randint(2, 4); nv = rng.randint(2, 3)
    bend = rng.uniform(0.15, 0.45)
    def surf(u, v):                 # mảnh hơi cong như đang bọc thân xe
        x = (u - nu / 2) * cell; y = (v - nv / 2) * cell
        return Vector((x, y, -bend * (x * x + y * y)))
    bm = bmesh.new(); uvl = bm.loops.layers.uv.new('UVMap')
    core = []; halo = []
    def strand(p, q):
        pts = [p.lerp(q, t / 4) for t in range(5)]
        for i in range(1, 4): pts[i] += Vector((rng.uniform(-.01, .01), rng.uniform(-.01, .01), rng.uniform(-.015, .015)))
        core.append(pts); halo.append(pts)
    # dây chéo hai họ (mắt thoi), bỏ ngẫu nhiên vài đoạn = chỗ rách
    for i in range(nu + 1):
        for j in range(nv + 1):
            a = surf(i, j)
            for di, dj in ((1, 0), (0, 1)):
                if i + di > nu or j + dj > nv: continue
                if rng.random() < 0.14: continue
                strand(a, surf(i + di, j + dj))
    # tua dây đứt ở mép (cong rủ xuống)
    for _ in range(rng.randint(4, 7)):
        i = rng.choice([0, nu]); j = rng.randint(0, nv)
        if rng.random() < .5: i, j = rng.randint(0, nu), rng.choice([0, nv])
        a = surf(i, j); out = Vector((i - nu / 2, j - nv / 2, 0)).normalized() if (i, j) != (nu / 2, nv / 2) else Vector((1, 0, 0))
        L = rng.uniform(0.08, 0.2)
        core.append([a, a + out * L * .5 + Vector((0, 0, -L * .2)), a + out * L + Vector((0, 0, -L * .6))])
    for pts in core: ribbon(bm, uvl, pts, 0.016, 0.012)
    nc = len(bm.faces)
    for pts in halo: ribbon(bm, uvl, pts, 0.06, 0.05)
    bm.faces.ensure_lookup_table()
    for i, f in enumerate(bm.faces): f.material_index = 0 if i < nc else 1
    # quả nặng nhỏ ở 1 góc (mảnh có quả nặng)
    o = make_obj('LuoiVo_Manh%d' % idx, bm, [M_NCORE, M_NHALO], col_n)
    o.location = (idx % 5 * 1.2, idx // 5 * 1.2 + 6, 0)
    return o

for i in range(10): net_piece(i)

bm = bmesh.new(); uvl = bm.loops.layers.uv.new('UVMap')
N = 80; outer = []; inner = []
for i in range(N):
    a = 2 * math.pi * i / N
    outer.append(bm.verts.new((math.cos(a), math.sin(a), 0)))
    inner.append(bm.verts.new((math.cos(a) * .8, math.sin(a) * .8, 0)))
for i in range(N):
    j = (i + 1) % N
    f = bm.faces.new((outer[i], outer[j], inner[j], inner[i]))
    for l, uv in zip(f.loops, ((i / N * 6, 0), ((i + 1) / N * 6, 0), ((i + 1) / N * 6, 1), (i / N * 6, 1))): l[uvl].uv = uv
o = make_obj('LuoiVo_Song', bm, [M_WAVE], col_n); o.location = (0, 10, 0)

# xếp tia cạnh khiên để xem
for i in range(6):
    o = bpy.data.objects['Khien_Tia%d' % i]; o.location = (2.4 + i * 0.35, -1.5, 0.3); o.rotation_euler = (0, 0, 0)

# ---------------------------------------------------------------- xuất + lưu
os.makedirs(OUT_DIR, exist_ok=True)
def export(col, path, keep_loc=False):
    bpy.ops.object.select_all(action='DESELECT')
    saved = {}
    for o in col.objects:
        o.select_set(True)
        if not keep_loc: saved[o.name] = tuple(o.location); o.location = (0, 0, 0)
    bpy.context.view_layer.objects.active = col.objects[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
                             axis_forward='-Z', axis_up='Y', bake_space_transform=True, object_types={'MESH'},
                             mesh_smooth_type='FACE', use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False)
    for n, l in saved.items(): bpy.data.objects[n].location = l

export(col_k, OUT_DIR + '/PV_Khien.fbx')
export(col_n, OUT_DIR + '/PV_LuoiVo.fbx')
bpy.ops.wm.save_as_mainfile(filepath=BLEND, copy=False)
REPORT = {o.name: (len(o.data.vertices), len(o.data.polygons)) for o in sc.collection.all_objects}
print('REPORT', REPORT)
