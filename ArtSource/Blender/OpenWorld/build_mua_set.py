"""21.317 — Thời tiết mưa giông: hạt mưa, màn mưa, giọt bắn, vết cháy và tia sét (dựng bằng Blender MCP, người dùng 22/11/2026).

Chạy trong Blender (MCP execute_blender_code):  exec(open(r'<repo>/ArtSource/Blender/OpenWorld/build_mua_set.py', encoding='utf-8').read())
Dựng trong scene riêng «ThoiTiet» (không đụng scene khác), lưu file mới PhoViet_OW_ThoiTiet.blend.
Ảnh (render EEVEE nền trong suốt, Standard):
  Resources/ThoiTiet/PV_Mua_Hat.png    64×512  — một hạt mưa rơi nhanh (vệt dài, đầu giọt sáng hơn, đuôi mờ dần)
  Resources/ThoiTiet/PV_Mua_Man.png   512×1024 — màn mưa lặp liền hai chiều (≈260 vệt dài ngắn, gần rõ / xa nhòe)
  Resources/ThoiTiet/PV_Mua_Toe.png   512×128  — 4 khung giọt bắn hình vương miện trên mặt đường (4×1)
  Resources/ThoiTiet/PV_Set_VetChay.png 256×256 — vết cháy sém mặt đường nơi sét đánh
Lưới (FBX):
  Resources/ThoiTiet/PV_Mua.fbx  — Mua_OngGan (ống r 8 m, cao 26 m) + Mua_OngXa (ống r 20 m, cao 40 m): màn mưa quanh camera
  Resources/ThoiTiet/PV_Set.fbx  — Set_0..7: 8 tia sét cao 150 m (kênh chính gấp khúc fractal + 6–10 nhánh + nhánh con
                                   + 2–3 nhánh bò ngang dưới mây). Mỗi nhánh là dải 2 đỉnh/điểm nằm trên đường tâm;
                                   shader Unity nở dải theo hướng nhìn camera.
Quy ước UV tia sét (đọc trong shader PhoViet/OpenWorld/Lightning):
  UV0 = (bên −1/+1, thời điểm tới t: 0 ở đỉnh mây → 1 ở đất theo kênh chính; nhánh tính theo chiều dài thật)
  UV1 = (hướng.x, hướng.y) — hướng tiếp tuyến đã đổi sang trục Unity (−x, z, −y)
  UV2 = (hướng.z, bề rộng tương đối)  UV3 = (độ sáng nhánh, 0..1 từ gốc nhánh → đầu nhánh)
Trục: Blender Z = lên. Gốc tia = điểm chạm đất (0,0,0).
"""
import bpy, bmesh, math, random, os
from mathutils import Vector

REPO = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT = REPO + '/Assets/PhoViet/Resources/ThoiTiet'
BLEND = REPO + '/ArtSource/Blender/OpenWorld/PhoViet_OW_ThoiTiet.blend'
os.makedirs(OUT, exist_ok=True)

sc = bpy.data.scenes.get('ThoiTiet') or bpy.data.scenes.new('ThoiTiet')
bpy.context.window.scene = sc
for o in list(sc.collection.all_objects):
    bpy.data.objects.remove(o, do_unlink=True)
for c in list(sc.collection.children):
    sc.collection.children.unlink(c)

def coll(name):
    c = bpy.data.collections.get(name)
    if not c: c = bpy.data.collections.new(name)
    if c.name not in sc.collection.children: sc.collection.children.link(c)
    for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
    return c

# ---------------------------------------------------------------- vật liệu phát sáng trong suốt (để render ảnh)
def emat(name, rgb, strength=1.0, alpha_from='gradZ'):
    """Emission + Transparent, độ đục = (gradient/fresnel/…) × Object Info Color.r (mỗi vệt một độ đục)."""
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True; nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new('ShaderNodeOutputMaterial')
    em = nt.nodes.new('ShaderNodeEmission'); em.inputs['Color'].default_value = (*rgb, 1); em.inputs['Strength'].default_value = strength
    tr = nt.nodes.new('ShaderNodeBsdfTransparent')
    mix = nt.nodes.new('ShaderNodeMixShader')
    info = nt.nodes.new('ShaderNodeObjectInfo')
    mul = nt.nodes.new('ShaderNodeMath'); mul.operation = 'MULTIPLY'; mul.use_clamp = True
    nt.links.new(info.outputs['Color'], mul.inputs[1])
    if alpha_from == 'gradZ':        # vệt mưa: đầu giọt (z thấp) đặc, đuôi (z cao) mờ dần; mép mờ theo fresnel
        tc = nt.nodes.new('ShaderNodeTexCoord'); sep = nt.nodes.new('ShaderNodeSeparateXYZ')
        nt.links.new(tc.outputs['Generated'], sep.inputs[0])
        ramp = nt.nodes.new('ShaderNodeValToRGB')
        ramp.color_ramp.elements[0].position = 0.0; ramp.color_ramp.elements[0].color = (1, 1, 1, 1)
        ramp.color_ramp.elements[1].position = 1.0; ramp.color_ramp.elements[1].color = (0, 0, 0, 1)
        e = ramp.color_ramp.elements.new(0.12); e.color = (0.85, 0.85, 0.85, 1)
        nt.links.new(sep.outputs['Z'], ramp.inputs['Fac'])
        lw = nt.nodes.new('ShaderNodeLayerWeight'); lw.inputs['Blend'].default_value = 0.35
        inv = nt.nodes.new('ShaderNodeMath'); inv.operation = 'SUBTRACT'; inv.inputs[0].default_value = 1.0
        nt.links.new(lw.outputs['Facing'], inv.inputs[1])
        m2 = nt.nodes.new('ShaderNodeMath'); m2.operation = 'MULTIPLY'
        nt.links.new(ramp.outputs['Color'], m2.inputs[0]); nt.links.new(inv.outputs[0], m2.inputs[1])
        nt.links.new(m2.outputs[0], mul.inputs[0])
    elif alpha_from == 'fresnel':    # nước bắn: viền sáng, giữa trong
        lw = nt.nodes.new('ShaderNodeLayerWeight'); lw.inputs['Blend'].default_value = 0.55
        add = nt.nodes.new('ShaderNodeMath'); add.operation = 'ADD'; add.inputs[1].default_value = 0.35
        nt.links.new(lw.outputs['Facing'], add.inputs[0]); nt.links.new(add.outputs[0], mul.inputs[0])
    elif alpha_from == 'scorch':     # vết cháy: tối giữa, loang ra mép theo nhiễu + tia xém hướng tâm
        tc = nt.nodes.new('ShaderNodeTexCoord')
        sub = nt.nodes.new('ShaderNodeVectorMath'); sub.operation = 'SUBTRACT'; sub.inputs[1].default_value = (0.5, 0.5, 0.5)
        nt.links.new(tc.outputs['Generated'], sub.inputs[0])
        ln = nt.nodes.new('ShaderNodeVectorMath'); ln.operation = 'LENGTH'; nt.links.new(sub.outputs[0], ln.inputs[0])
        r = nt.nodes.new('ShaderNodeMath'); r.operation = 'MULTIPLY'; r.inputs[1].default_value = 2.0; nt.links.new(ln.outputs['Value'], r.inputs[0])
        noise = nt.nodes.new('ShaderNodeTexNoise'); noise.inputs['Scale'].default_value = 5.0; noise.inputs['Detail'].default_value = 9.0
        nt.links.new(tc.outputs['Generated'], noise.inputs['Vector'])
        # tia xém: nhiễu theo góc (tọa độ cực)
        sep = nt.nodes.new('ShaderNodeSeparateXYZ'); nt.links.new(sub.outputs[0], sep.inputs[0])
        at = nt.nodes.new('ShaderNodeMath'); at.operation = 'ARCTAN2'; nt.links.new(sep.outputs['Y'], at.inputs[0]); nt.links.new(sep.outputs['X'], at.inputs[1])
        comb = nt.nodes.new('ShaderNodeCombineXYZ'); nt.links.new(at.outputs[0], comb.inputs['X'])
        rn = nt.nodes.new('ShaderNodeTexNoise'); rn.inputs['Scale'].default_value = 3.0; rn.inputs['Detail'].default_value = 4.0
        sc2 = nt.nodes.new('ShaderNodeVectorMath'); sc2.operation = 'SCALE'; sc2.inputs['Scale'].default_value = 2.5
        nt.links.new(comb.outputs[0], sc2.inputs[0]); nt.links.new(sc2.outputs[0], rn.inputs['Vector'])
        # độ đục = (1 − r^1.2 × (0.75 + tia)) × (0.55 + nhiễu)
        pw = nt.nodes.new('ShaderNodeMath'); pw.operation = 'POWER'; pw.inputs[1].default_value = 1.3; nt.links.new(r.outputs[0], pw.inputs[0])
        rays = nt.nodes.new('ShaderNodeMath'); rays.operation = 'MULTIPLY_ADD'; rays.inputs[1].default_value = 0.5; rays.inputs[2].default_value = 0.7
        nt.links.new(rn.outputs['Fac'], rays.inputs[0])
        pm = nt.nodes.new('ShaderNodeMath'); pm.operation = 'MULTIPLY'; nt.links.new(pw.outputs[0], pm.inputs[0]); nt.links.new(rays.outputs[0], pm.inputs[1])
        one = nt.nodes.new('ShaderNodeMath'); one.operation = 'SUBTRACT'; one.inputs[0].default_value = 1.0; one.use_clamp = True
        nt.links.new(pm.outputs[0], one.inputs[1])
        nz = nt.nodes.new('ShaderNodeMath'); nz.operation = 'ADD'; nz.inputs[1].default_value = 0.45; nt.links.new(noise.outputs['Fac'], nz.inputs[0])
        a = nt.nodes.new('ShaderNodeMath'); a.operation = 'MULTIPLY'; nt.links.new(one.outputs[0], a.inputs[0]); nt.links.new(nz.outputs[0], a.inputs[1])
        nt.links.new(a.outputs[0], mul.inputs[0])
    nt.links.new(mul.outputs[0], mix.inputs['Fac'])
    nt.links.new(tr.outputs[0], mix.inputs[1]); nt.links.new(em.outputs[0], mix.inputs[2])
    nt.links.new(mix.outputs[0], out.inputs['Surface'])
    try: m.surface_render_method = 'BLENDED'
    except Exception: pass
    return m

M_DROP = emat('PV_Mua_HatVat', (1, 1, 1), 1.0, 'gradZ')
M_SPLASH = emat('PV_Mua_ToeVat', (1, 1, 1), 1.0, 'fresnel')
M_SCORCH = emat('PV_Set_VetChayVat', (0.055, 0.045, 0.038), 1.0, 'scorch')

def drop_mesh(name, length, radius, col, loc, alpha):
    """Vệt hạt mưa: hình thoi kéo dài (đầu giọt tròn ở dưới, đuôi nhọn ở trên)."""
    bm = bmesh.new()
    seg, rings = 8, 14
    rows = []
    for j in range(rings + 1):
        t = j / rings                                   # 0 = đầu giọt (dưới), 1 = đuôi (trên)
        z = t * length
        r = radius * (math.sin(min(1, t * 6) * math.pi / 2) * (1 - t) ** 0.6 if t > 0 else 0.0)
        row = [bm.verts.new((r * math.cos(2 * math.pi * k / seg), r * math.sin(2 * math.pi * k / seg), z)) for k in range(seg)]
        rows.append(row)
    for j in range(rings):
        for k in range(seg):
            a, b = rows[j][k], rows[j][(k + 1) % seg]; c, d = rows[j + 1][(k + 1) % seg], rows[j + 1][k]
            try: bm.faces.new((a, b, c, d))
            except ValueError: pass
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-6)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    me.materials.append(M_DROP)
    o = bpy.data.objects.new(name, me); col.objects.link(o); o.location = loc; o.color = (alpha, alpha, alpha, 1)
    return o

# ---------------------------------------------------------------- camera + render
def cam_ortho(name, loc, rot, scale):
    cd = bpy.data.cameras.get(name) or bpy.data.cameras.new(name)
    cd.type = 'ORTHO'; cd.ortho_scale = scale; cd.clip_start = 0.01; cd.clip_end = 100
    o = bpy.data.objects.new(name, cd); sc.collection.objects.link(o); o.location = loc; o.rotation_euler = rot
    return o

def render(cam, col, path, w, h):
    for c in sc.collection.children: c.hide_render = (c != col)
    sc.camera = cam
    r = sc.render; r.resolution_x = w; r.resolution_y = h; r.resolution_percentage = 100
    r.film_transparent = True; r.image_settings.file_format = 'PNG'; r.image_settings.color_mode = 'RGBA'
    r.image_settings.color_depth = '8'; r.filepath = path
    try: sc.view_settings.view_transform = 'Standard'
    except TypeError: pass
    sc.view_settings.look = 'None'; sc.view_settings.exposure = 0; sc.view_settings.gamma = 1
    try: sc.eevee.taa_render_samples = 32
    except Exception: pass
    if sc.world is None: sc.world = bpy.data.worlds.new('ThoiTiet_World')
    sc.world.use_nodes = True
    bg = next((n for n in sc.world.node_tree.nodes if n.type == 'BACKGROUND'), None)
    if bg: bg.inputs['Strength'].default_value = 0.0
    bpy.ops.render.render(write_still=True)

rng = random.Random(21317)

# ---- A. một hạt mưa (camera nhìn ngang, khung 0,125 × 1 m)
colA = coll('Mua_HatRender')
drop_mesh('Hat', 0.92, 0.022, colA, (0, 0, -0.46), 1.0)
camA = cam_ortho('CamHat', (0, -5, 0), (math.radians(90), 0, 0), 1.0)
render(camA, colA, OUT + '/PV_Mua_Hat.png', 64, 512)

# ---- B. màn mưa lặp liền (khung 1 × 2 m; vệt chạm mép được nhân bản sang phía đối diện)
colB = coll('Mua_ManRender')
W, H = 1.0, 2.0
for i in range(260):
    depth = rng.random()                                # 0 gần (rõ, dày), 1 xa (mảnh, mờ)
    L = rng.uniform(0.08, 0.42) * (1.2 - 0.5 * depth)
    rad = (0.0042 - 0.0026 * depth) * rng.uniform(0.8, 1.25)
    a = rng.uniform(0.35, 0.95) * (1 - 0.6 * depth)
    x = rng.uniform(-W / 2, W / 2); z = rng.uniform(-H / 2, H / 2)
    for dx in (-W, 0, W):
        for dz in (-H, 0, H):
            px, pz = x + dx, z + dz
            if -W / 2 - 0.02 < px < W / 2 + 0.02 and -H / 2 - L < pz < H / 2 + 0.01:
                drop_mesh('Man%03d' % i, L, rad, colB, (px, depth * 2, pz), a)
camB = cam_ortho('CamMan', (0, -5, 0), (math.radians(90), 0, 0), 2.0)
render(camB, colB, OUT + '/PV_Mua_Man.png', 512, 1024)

# ---- C. giọt bắn vương miện, 4 khung (mỗi khung 0,5 × 0,5 m, đặt cạnh nhau theo trục X)
colC = coll('Mua_ToeRender')
def crown(cx, stage):
    """stage 0..3: vương miện nhú → loe rộng có giọt văng → vỡ → gợn tan."""
    bm = bmesh.new()
    n = 14; rad = [0.06, 0.105, 0.15, 0.2][stage]; hgt = [0.08, 0.15, 0.105, 0.018][stage]
    wall = [0.018, 0.015, 0.011, 0.006][stage]
    inner, outer, top = [], [], []
    for k in range(n * 2):
        ang = math.pi * k / n
        spike = (1.0 if k % 2 == 0 else 0.55) if stage < 3 else 1.0
        r0 = rad * (0.75 if stage < 3 else 0.92)
        inner.append(bm.verts.new((cx + r0 * math.cos(ang), r0 * math.sin(ang), 0)))
        outer.append(bm.verts.new((cx + (rad + wall) * math.cos(ang), (rad + wall) * math.sin(ang), 0)))
        tr = rad * (1.12 if stage < 3 else 1.0)
        top.append(bm.verts.new((cx + tr * math.cos(ang), tr * math.sin(ang), hgt * spike)))
    m = len(top)
    for k in range(m):
        k2 = (k + 1) % m
        bm.faces.new((inner[k], inner[k2], top[k2], top[k]))
        bm.faces.new((top[k], top[k2], outer[k2], outer[k]))
    me = bpy.data.meshes.new('Toe%d' % stage); bm.to_mesh(me); bm.free(); me.materials.append(M_SPLASH)
    o = bpy.data.objects.new('Toe%d' % stage, me); colC.objects.link(o); o.color = ((1.0, 0.95, 0.8, 0.45)[stage],) * 3 + (1,)
    if stage in (1, 2):                                 # giọt văng trên đỉnh gai
        for k in range(0, n * 2, 2):
            ang = math.pi * k / n; up = hgt * (1.35 if stage == 1 else 1.8); rr = rad * (1.18 if stage == 1 else 1.45)
            bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=6, radius=0.011 if stage == 1 else 0.009,
                                                 location=(cx + rr * math.cos(ang), rr * math.sin(ang), up))
            s = bpy.context.active_object
            for c in s.users_collection: c.objects.unlink(s)
            colC.objects.link(s); s.data.materials.append(M_SPLASH); s.color = (0.9, 0.9, 0.9, 1)
for st in range(4): crown(st * 0.5 - 0.75, st)
camC = cam_ortho('CamToe', (0, -3, 1.02), (math.radians(73), 0, 0), 2.0)
render(camC, colC, OUT + '/PV_Mua_Toe.png', 512, 128)

# ---- D. vết cháy sém (nhìn thẳng xuống)
colD = coll('Set_VetChayRender')
bpy.ops.mesh.primitive_plane_add(size=1.0, location=(0, 0, 0))
pl = bpy.context.active_object
for c in pl.users_collection: c.objects.unlink(pl)
colD.objects.link(pl); pl.name = 'VetChay'; pl.data.materials.append(M_SCORCH); pl.color = (1, 1, 1, 1)
camD = cam_ortho('CamVetChay', (0, 0, 5), (0, 0, 0), 1.0)
render(camD, colD, OUT + '/PV_Set_VetChay.png', 256, 256)

# ---------------------------------------------------------------- E. ống màn mưa (FBX)
colE = coll('Mua')
def tube(name, radius, height, tiles_u, tiles_v, seg=40, rows=6):
    bm = bmesh.new(); uv = bm.loops.layers.uv.new('UVMap')
    vs = [[bm.verts.new((radius * math.cos(2 * math.pi * k / seg), radius * math.sin(2 * math.pi * k / seg), height * j / rows - height * 0.3))
           for k in range(seg + 1)] for j in range(rows + 1)]
    for j in range(rows):
        for k in range(seg):
            f = bm.faces.new((vs[j][k], vs[j][k + 1], vs[j + 1][k + 1], vs[j + 1][k]))
            for lp, (kk, jj) in zip(f.loops, ((k, j), (k + 1, j), (k + 1, j + 1), (k, j + 1))):
                lp[uv].uv = (kk / seg * tiles_u, jj / rows * tiles_v)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); colE.objects.link(o); return o
tube('Mua_OngGan', 8.0, 26.0, 10, 4)
tube('Mua_OngXa', 20.0, 40.0, 18, 4)

# ---------------------------------------------------------------- F. tia sét (FBX)
colF = coll('Set')
def to_unity(v):                                        # Blender (x, y, z) → Unity (−x, z, −y) (xuất forward −Z, up Y)
    return Vector((-v.x, v.z, -v.y))

def fractal(p0, p1, levels, amp, r):
    pts = [p0, p1]
    for lv in range(levels):
        out = [pts[0]]
        for a, b in zip(pts, pts[1:]):
            d = b - a; L = d.length
            perp = d.orthogonal().normalized(); perp.rotate(__import__('mathutils').Quaternion(d.normalized(), r.uniform(0, 2 * math.pi)))
            mid = (a + b) / 2 + perp * L * amp * r.uniform(-1, 1)
            out += [mid, b]
        pts = out
        amp *= 0.62
    return pts

def add_strip(bm, L, pts, t0, t_scale, width, bright):
    uv0, uv1, uv2, uv3 = L
    n = len(pts); total = sum((pts[i + 1] - pts[i]).length for i in range(n - 1)) or 1
    acc = 0.0; prev = None; rows = []
    for i, p in enumerate(pts):
        if i > 0: acc += (pts[i] - pts[i - 1]).length
        d = (pts[min(i + 1, n - 1)] - pts[max(i - 1, 0)]).normalized()
        du = to_unity(d)
        along = acc / total
        w = width * (1 - 0.55 * along) if bright < 0.99 else width * (1 - 0.2 * along)
        rows.append((bm.verts.new(p), bm.verts.new(p), t0 + acc * t_scale, du, w, along))
    for i in range(n - 1):
        a, b = rows[i], rows[i + 1]
        f = bm.faces.new((a[0], a[1], b[1], b[0]))
        for lp, (row, side) in zip(f.loops, ((a, -1), (a, 1), (b, 1), (b, -1))):
            lp[uv0].uv = (side, row[2]); lp[uv1].uv = (row[3].x, row[3].y); lp[uv2].uv = (row[3].z, row[4]); lp[uv3].uv = (bright, row[5])
    return total

def bolt(idx, r):
    bm = bmesh.new(); L = tuple(bm.loops.layers.uv.new(nm) for nm in ('UVMap', 'Huong', 'HuongRong', 'Sang'))
    top = Vector((r.uniform(-28, 28), r.uniform(-28, 28), 150))
    main = fractal(top, Vector((0, 0, 0)), 7, 0.16, r)
    # gấp khúc nhỏ thêm (đoạn 1–3 m) để kênh sét lởm chởm như thật
    fine = [main[0]]
    for a, b in zip(main, main[1:]):
        fine += fractal(a, b, 2, 0.35, r)[1:]
    main = fine
    lengths = [0]
    for a, b in zip(main, main[1:]): lengths.append(lengths[-1] + (b - a).length)
    total = lengths[-1]; ts = 1.0 / total
    add_strip(bm, L, main, 0.0, ts, 1.0, 1.0)
    nb = r.randint(6, 10)
    for k in range(nb):
        i = int(len(main) * r.uniform(0.08, 0.78)); p = main[i]
        down = (main[min(i + 3, len(main) - 1)] - p).normalized()
        side = Vector((r.uniform(-1, 1), r.uniform(-1, 1), 0)).normalized()
        dirb = (down * math.cos(math.radians(r.uniform(28, 60))) + side * math.sin(math.radians(r.uniform(28, 60)))).normalized()
        blen = r.uniform(14, 55) * (1 - lengths[i] / total * 0.5)
        bp = fractal(p, p + dirb * blen, 5, 0.22, r)
        t0 = lengths[i] * ts
        add_strip(bm, L, bp, t0, ts, 0.5, 0.62)
        for s in range(r.randint(0, 2)):                 # nhánh con
            j = int(len(bp) * r.uniform(0.3, 0.7)); q = bp[j]
            d2 = (dirb + Vector((r.uniform(-.8, .8), r.uniform(-.8, .8), -0.3))).normalized()
            sp = fractal(q, q + d2 * blen * r.uniform(0.25, 0.45), 4, 0.25, r)
            add_strip(bm, L, sp, t0 + (q - p).length * ts, ts, 0.28, 0.38)
    for k in range(r.randint(2, 3)):                     # nhánh bò ngang dưới chân mây
        p = main[int(len(main) * r.uniform(0.0, 0.06))]
        ang = r.uniform(0, 2 * math.pi)
        end = p + Vector((math.cos(ang), math.sin(ang), r.uniform(-0.12, 0.05))) * r.uniform(40, 85)
        cp = fractal(p, end, 6, 0.2, r)
        add_strip(bm, L, cp, 0.0, ts * 0.6, 0.55, 0.5)
    me = bpy.data.meshes.new('Set_%d' % idx); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new('Set_%d' % idx, me); colF.objects.link(o); return o

for i in range(8): bolt(i, random.Random(31700 + i * 77))

# ---------------------------------------------------------------- xuất + lưu
def export(col, path):
    bpy.ops.object.select_all(action='DESELECT')
    for o in col.objects: o.select_set(True)
    bpy.context.view_layer.objects.active = col.objects[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
                             axis_forward='-Z', axis_up='Y', bake_space_transform=True, object_types={'MESH'},
                             mesh_smooth_type='FACE', use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False)
for c in sc.collection.children: c.hide_render = False
export(colE, OUT + '/PV_Mua.fbx')
export(colF, OUT + '/PV_Set.fbx')
bpy.ops.wm.save_as_mainfile(filepath=BLEND, copy=False)
REPORT = {o.name: (len(o.data.vertices), len(o.data.polygons)) for o in list(colE.objects) + list(colF.objects)}
print('REPORT', REPORT)
