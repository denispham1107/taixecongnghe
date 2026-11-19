# 21.271 — lưới mặt sông bản đồ mở (thay mặt phẳng ô 5 m «MatSong» + hộp nước ngoài mép).
# Chạy trong Blender qua MCP:
#   p='C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld/run_in_blender.py'
#   ns={'TARGET':'build_river_surface.py'}; exec(open(p,encoding='utf-8').read(), ns); print(ns['REPORT'])
#
# Lưới dựng theo toạ độ dòng chảy: tim sông = nhánh ngang (z 314,25, từ x −1400 chảy sang đông) → cung rẽ phải tâm
# (234,5; 231,5) bán kính 82,75 → nhánh dọc (x 317,25, chảy về nam tới z −1400). Mỗi hàng là một mặt cắt ngang vuông
# góc tim sông (ở khúc cua là tia từ tâm), cắt với đường bờ trong layout.json để biết hai mép kè → mép lưới khớp đúng kè,
# kể cả góc bo bờ trong và cung bờ ngoài. Toạ độ Blender (x, y) = thế giới Unity (x, z) (gốc BanDoMo xoay 180°).
# Thuộc tính đỉnh (shader PhoViet/OpenWorld/River):
#   UV      = (s, d) mét: s dọc tim sông theo chiều chảy, d ngang (dương = bên trái chiều chảy)
#   Màu     = r: gần kè (1 sát kè → 0 khi cách ≥ 10 m), g/b: hướng chảy (x, z)·0,5 + 0,5, a: 1  (FLOAT_COLOR, xuất LINEAR)
# Chia khúc ~160 m dọc tim sông (mỗi khúc một renderer để Unity chọn Reflection Probe gần nhất và cắt khi ngoài tầm nhìn).
import bpy, bmesh, json, math, os

ROOT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
LAYOUT = json.load(open(ROOT + '/ArtSource/Concepts/OpenWorld-20261117/layout.json', encoding='utf-8'))
WATER_Z = LAYOUT['river']['waterLevel']            # −2,2
FAR = -1400.0                                       # sông kéo ra ngoài mép bản đồ (sương che)
OUT_FBX = ROOT + '/Assets/PhoViet/OpenWorld/River/OW_MatSong.fbx'
OUT_BLEND = ROOT + '/ArtSource/Blender/OpenWorld/PhoViet_OW_River.blend'
CX, CY, R = 234.5, 231.5, 82.75                     # tâm + bán kính cung tim sông ở khúc cua
HZ, VX = 314.25, 317.25                             # tim nhánh ngang (y) và nhánh dọc (x)
ACROSS = 44                                         # số ô ngang sông (≈ 4 m)
CHUNK = 160.0

# đường bờ: nối dài hai đầu ra FAR
outline = [tuple(p) for p in LAYOUT['river']['outline']]
ext = []
for x, y in outline:
    if y == -600: y = FAR
    if x == -600: x = FAR
    ext.append((x, y))
EDGES = [(ext[i], ext[(i + 1) % len(ext)]) for i in range(len(ext))]


def interval(px, py, nx, ny):
    """Mặt cắt P + t·n: trả (t_min, t_max) của đoạn nằm trong lòng sông chứa P (t = 0)."""
    ts = []
    for (ax, ay), (bx, by) in EDGES:
        ex, ey = bx - ax, by - ay
        den = nx * ey - ny * ex
        if abs(den) < 1e-9: continue
        t = ((ax - px) * ey - (ay - py) * ex) / den
        u = ((ax - px) * ny - (ay - py) * nx) / den
        if -1e-6 <= u <= 1 + 1e-6: ts.append(t)
    lo = max([t for t in ts if t < 0], default=-82.75)
    hi = min([t for t in ts if t > 0], default=82.75)
    return lo, hi


def rows():
    """Các mặt cắt dọc tim sông: (s, tâm P, hướng chảy f, hướng trái n). Bước 4 m trong bản đồ, 12 m ngoài mép."""
    out = []; s = 0.0
    def step(v): return 12.0 if v < -640 else 4.0
    # 1) nhánh ngang: x từ FAR tới 234,5 (chảy +x), trái = +y
    x = FAR
    while x < CX - 1e-6:
        out.append((s, (x, HZ), (1.0, 0.0), (0.0, 1.0), -1e9)); d = min(step(x), CX - x); x += d; s += d
    # 2) khúc cua: θ từ 90° (đỉnh) xuống 0° (rẽ phải), trái = hướng ra ngoài tâm
    arc = R * math.pi / 2; n = int(math.ceil(arc / 2.0))        # bước 2 m trên tim → ≈ 4,8 m ở bờ ngoài
    for k in range(n):
        th = math.pi / 2 - (math.pi / 2) * k / n
        c, si = math.cos(th), math.sin(th)
        out.append((s, (CX + R * c, CY + R * si), (si, -c), (c, si), -R)); s += arc / n   # tia dừng ở tâm (sau tâm là góc phố)
    # 3) nhánh dọc: y từ 231,5 xuống FAR (chảy −y), trái = +x
    y = CY
    while y > FAR - 1e-6:
        out.append((s, (VX, y), (0.0, -1.0), (1.0, 0.0), 222.0 - VX)); d = step(y); y -= d; s += d   # mép trái không vượt góc bo bờ trong (x 222)
    return out


def build():
    for o in [o for o in bpy.data.objects if o.name.startswith('OW_MatSong')]: bpy.data.objects.remove(o, do_unlink=True)
    col = bpy.data.collections.get('MatSong') or bpy.data.collections.new('MatSong')
    if col.name not in bpy.context.scene.collection.children: bpy.context.scene.collection.children.link(col)
    mat = bpy.data.materials.get('OW_River') or bpy.data.materials.new('OW_River')
    mat.diffuse_color = (0.05, 0.14, 0.13, 1)
    R_ = rows()
    grid = []                                   # mỗi hàng: list (x, y, s, d, shore, fx, fy)
    for s, (px, py), (fx, fy), (nx, ny), cap in R_:
        lo, hi = interval(px, py, nx, ny); lo = max(lo, cap)
        row = []
        for j in range(ACROSS + 1):
            t = lo + (hi - lo) * j / ACROSS
            shore = 1 - min(min(t - lo, hi - t) / 10.0, 1.0)
            row.append((px + nx * t, py + ny * t, s, t, shore, fx, fy))
        grid.append(row)
    # chia khúc theo s
    total = R_[-1][0]; nch = int(math.ceil(total / CHUNK)); objs = []
    for ci in range(nch):
        s0, s1 = ci * CHUNK, (ci + 1) * CHUNK
        idx = [i for i, r in enumerate(R_) if s0 <= r[0] <= s1 + 1e-6]
        if idx and idx[-1] + 1 < len(R_): idx.append(idx[-1] + 1)          # khép mép với khúc sau
        if len(idx) < 2: continue
        bm = bmesh.new()
        uv = bm.loops.layers.uv.new('UVMap')
        colr = bm.verts.layers.float_color.new('Col')
        vs = []
        for i in idx:
            rv = []
            for (x, y, s, d, sh, fx, fy) in grid[i]:
                v = bm.verts.new((x, y, WATER_Z)); v[colr] = (sh, fx * .5 + .5, fy * .5 + .5, 1.0); rv.append((v, s, d))
            vs.append(rv)
        for a in range(len(vs) - 1):
            for j in range(ACROSS):
                q = [vs[a][j], vs[a][j + 1], vs[a + 1][j + 1], vs[a + 1][j]]
                try: f = bm.faces.new([e[0] for e in q])
                except ValueError: continue
                for loop, e in zip(f.loops, q): loop[uv].uv = (e[1], e[2])
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        for f in bm.faces:
            if f.normal.z < 0: f.normal_flip()
        me = bpy.data.meshes.new('OW_MatSong_%02d' % ci); bm.to_mesh(me); bm.free()
        me.materials.append(mat)
        ob = bpy.data.objects.new(me.name, me); col.objects.link(ob); objs.append(ob)
    return objs


def export(objs):
    os.makedirs(os.path.dirname(OUT_FBX), exist_ok=True)
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.export_scene.fbx(filepath=OUT_FBX, use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y',
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False,
                             mesh_smooth_type='OFF', colors_type='LINEAR', use_mesh_modifiers=False)


objs = build()
nv = sum(len(o.data.vertices) for o in objs); nf = sum(len(o.data.polygons) for o in objs)
export(objs)
bpy.ops.wm.save_as_mainfile(filepath=OUT_BLEND, copy=False)
print('khúc', len(objs), 'đỉnh', nv, 'mặt', nf, '→', OUT_FBX)
