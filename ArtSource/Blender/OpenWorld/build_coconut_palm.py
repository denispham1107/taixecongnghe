# 21.286 — cây dừa + bệ cây mới cho bản đồ mở (thay PV_StreetTree0..2 sau khi người dùng duyệt).
# Ảnh mẫu: ArtSource/Concepts/OpenWorld-20261117/CayDua-21286/anh-mau-nguoi-dung.png — một thân thon cao cong nhẹ (gốc lệch phải,
# ngọn nghiêng trái rồi thẳng), gốc phình trên ụ đất; thân xám-nâu nhạt có vòng sẹo; tán ~45 % chiều cao trên, ~26 tàu lá:
# tàu trên vươn cong, tàu dưới rủ sâu; lá chét dài mảnh rủ; màu: trái/dưới vàng cam (nắng), đỉnh xanh lá, phải xanh ngọc/đậm,
# lõi tán nâu cam tối. Toàn bộ là lưới có màu đỉnh («Col», rgb + a = 0) — không texture — để Unity vẽ bằng 1 vật liệu.
# Hệ toạ độ: gốc cây tại (0,0,0), z lên. Chạy qua Blender MCP:
#   ns = {'OW_DIR': ...}; exec(open(.../build_coconut_palm.py).read(), ns); ns['build_palm']('A', seed=1)
import bpy, bmesh, math, random, os
from mathutils import Vector, Matrix

OUT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/OpenWorld-20261117/CayDua-21286'

def hexrgb(h):
    h = h.lstrip('#'); return tuple(int(h[i:i + 2], 16) / 255.0 for i in (0, 2, 4))
def mixc(a, b, t): return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))
def jit(c, r, k=0.06): return tuple(max(0, min(1, v * (1 + r.uniform(-k, k)))) for v in c)

class VC:
    """Gom lưới + màu đỉnh theo góc mặt (Col)."""
    def __init__(self, name):
        self.name = name; self.bm = bmesh.new(); self.col = self.bm.loops.layers.color.new('Col')
    def face(self, pts, cols):
        vs = [self.bm.verts.new(p) for p in pts]
        try: f = self.bm.faces.new(vs)
        except ValueError: return None
        for l, c in zip(f.loops, cols): l[self.col] = (c[0], c[1], c[2], 1.0)
        return f
    def quad(self, a, b, c, d, ca, cb=None, cc=None, cd=None):
        return self.face([a, b, c, d], [ca, cb or ca, cc or ca, cd or ca])
    def tri(self, a, b, c, ca, cb=None, cc=None):
        return self.face([a, b, c], [ca, cb or ca, cc or ca])
    def finish(self, coll, mat):
        bmesh.ops.remove_doubles(self.bm, verts=self.bm.verts, dist=1e-5)
        me = bpy.data.meshes.new(self.name); self.bm.to_mesh(me); self.bm.free()
        me.materials.append(mat)
        for p in me.polygons: p.use_smooth = True
        ob = bpy.data.objects.new(self.name, me); coll.objects.link(ob); return ob

def vc_material():
    m = bpy.data.materials.get('CD_MauDinh')
    if m: return m
    m = bpy.data.materials.new('CD_MauDinh'); m.use_nodes = True
    nt = m.node_tree; b = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    at = nt.nodes.new('ShaderNodeVertexColor'); at.layer_name = 'Col'
    nt.links.new(at.outputs['Color'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = 0.62
    try:
        b.inputs['Subsurface Weight'].default_value = 0.0
        b.inputs['Transmission Weight'].default_value = 0.0
    except Exception: pass
    m.use_backface_culling = False
    return m

# ------------------------------------------------------------------------------------------------ thân
def trunk(V, r, H, base_r, top_r, lean, bend, rings=True, seg=14, steps=46):
    """Thân cong chữ S nhẹ: x(t) = lean·t² − bend·sin(πt)·0,35; gốc phình mũ (×1,9 ở t=0). Vòng sẹo mỗi ~0,16 m (gờ + vạch tối)."""
    def C(t): return Vector((lean * t * t - bend * math.sin(math.pi * t) * 0.35, 0.12 * math.sin(2.1 * math.pi * t) * bend, H * t))
    def R(t):
        flare = 1 + 0.95 * math.exp(-t * 26) + 0.25 * math.exp(-t * 7)
        return (top_r + (base_r - top_r) * (1 - t) ** 1.4) * flare
    cBase = hexrgb('#7A6A58'); cMid = hexrgb('#A79A88'); cTop = hexrgb('#B9A58C'); cRing = hexrgb('#5E5145'); cWarm = hexrgb('#C79A64')
    rings_n = int(H / 0.16)
    steps = rings_n * 2                       # 2 hàng đỉnh mỗi vòng sẹo: hàng chẵn là gờ sẫm
    prev = None
    for i in range(steps + 1):
        t = i / steps; c = C(t); tan = (C(min(1, t + 0.01)) - C(max(0, t - 0.01))).normalized()
        side = tan.cross(Vector((0, 1, 0))).normalized(); up2 = tan.cross(side).normalized()
        ring = []
        for k in range(seg):
            a = 2 * math.pi * k / seg
            rr = R(t) * (1 + 0.035 * math.sin(a * 3 + t * 9)) * (1.025 if (rings and i % 2 == 0) else 1.0)   # gờ vòng sẹo
            ring.append(c + side * math.cos(a) * rr + up2 * math.sin(a) * rr)
        # màu: gốc sẫm → giữa xám → ngọn tan ấm; phía nắng (−x) ấm hơn
        base = mixc(cBase, cMid, min(1, t * 2.2)) if t < 0.45 else mixc(cMid, cTop, (t - 0.45) / 0.55)
        cols = []
        for k in range(seg):
            a = 2 * math.pi * k / seg; sun = max(0, -math.cos(a)) * 0.35
            band = 0.0
            if rings and i % 2 == 0 and 0.02 < t < 0.97:
                band = 0.38 + 0.12 * math.sin(a * 2 + i)              # vòng sẹo sẫm, đậm nhạt quanh thân
            cc = mixc(mixc(base, cWarm, sun), cRing, band)
            cols.append(jit(cc, r, 0.04))
        if prev:
            pr, pc = prev
            for k in range(seg):
                k2 = (k + 1) % seg
                V.quad(pr[k], pr[k2], ring[k2], ring[k], pc[k], pc[k2], cols[k2], cols[k])
        prev = (ring, cols)
    return C(1.0), (C(1.0) - C(0.97)).normalized(), R(1.0)

def mound(V, r, rad=0.62):
    """Ụ đất nhỏ quanh gốc (như ảnh) + vài cụm cỏ."""
    soil = hexrgb('#6B4A30'); grass = hexrgb('#5E7A2E'); seg = 18
    rings = [(rad, 0.0), (rad * 0.75, 0.07), (rad * 0.45, 0.12)]
    for j in range(len(rings) - 1):
        (r0, z0), (r1, z1) = rings[j], rings[j + 1]
        for k in range(seg):
            a0 = 2 * math.pi * k / seg; a1 = 2 * math.pi * (k + 1) / seg
            V.quad((r0 * math.cos(a0), r0 * math.sin(a0), z0), (r0 * math.cos(a1), r0 * math.sin(a1), z0),
                   (r1 * math.cos(a1), r1 * math.sin(a1), z1), (r1 * math.cos(a0), r1 * math.sin(a0), z1), jit(soil, r, 0.12))
    for k in range(9):
        a = r.uniform(0, 2 * math.pi); d = r.uniform(rad * 0.45, rad * 0.95); x, y = d * math.cos(a), d * math.sin(a)
        for b in range(4):
            bb = a + b * 1.6 + r.uniform(-0.4, 0.4); h = r.uniform(0.08, 0.16)
            V.tri((x - 0.012 * math.sin(bb), y + 0.012 * math.cos(bb), 0.02), (x + 0.012 * math.sin(bb), y - 0.012 * math.cos(bb), 0.02),
                  (x + math.cos(bb) * 0.05, y + math.sin(bb) * 0.05, h), jit(grass, r, 0.15), jit(grass, r, 0.15), mixc(grass, hexrgb('#A9B44E'), 0.5))

# ------------------------------------------------------------------------------------------------ tán lá
PAL = {   # bảng màu tán theo ảnh: vàng cam nắng / xanh lá / xanh ngọc / nâu khô
    'gold': ('#F2B238', '#E58A1E', '#FFD36A'), 'amber': ('#D9881F', '#B5651A', '#F0A43A'),
    'green': ('#5E9A3A', '#3F7A2E', '#9CC25A'), 'teal': ('#2F7F78', '#1E5E62', '#4FA59A'),
    'olive': ('#8A9A3A', '#6B7A2A', '#B8B85A'), 'dry': ('#9A6A34', '#7A4E26', '#C08A48'),
}

def frond_colour(az, elev, r, style):
    """Màu tàu theo hướng như ảnh: phía trái (−x, hướng nắng) vàng cam, phía phải (+x) xanh ngọc/xanh đậm, đỉnh xanh lá."""
    sx = -math.cos(az)                     # 1 = hướng −x (trái ảnh)
    if style == 'B':                        # biến thể xanh tươi
        k = 'green' if r.random() < 0.6 else ('olive' if r.random() < 0.6 else 'teal')
        return PAL[k]
    if style == 'C' and r.random() < 0.15: return PAL['dry']
    if sx > 0.1: return PAL['gold'] if r.random() < (0.55 if elev > 0.6 else 0.72) else (PAL['olive'] if elev > 0.6 else PAL['amber'])
    if elev > 0.55: return PAL['green'] if r.random() < 0.7 else PAL['olive']
    if sx < -0.3: return PAL['teal'] if r.random() < 0.55 else PAL['green']
    return [PAL['olive'], PAL['gold'], PAL['green']][r.randrange(3)]

def frond(V, r, top, az, elev, L, droop, cols, n_pairs=34, leaf_len=1.15, twist=0.0, hang=0.0):
    """Một tàu lá: cuống (rachis) cong theo trọng lực + các cặp lá chét dài rủ, xếp chữ V, ngắn dần về hai đầu."""
    cR, cD, cL = (hexrgb(c) for c in cols)
    d0 = Vector((math.cos(az) * math.cos(elev), math.sin(az) * math.cos(elev), math.sin(elev)))
    def P(s):
        return top + d0 * (L * s) + Vector((0, 0, -droop * s * s * L))
    def T(s): return (P(min(1, s + 0.01)) - P(max(0, s - 0.01))).normalized()
    # cuống: dải 3 cạnh nhỏ dần
    stp = 14; rib = hexrgb('#C8A060')
    for i in range(stp):
        s0, s1 = i / stp, (i + 1) / stp; p0, p1 = P(s0), P(s1); t0 = T(s0)
        side = t0.cross(Vector((0, 0, 1))).normalized(); upv = side.cross(t0).normalized()
        w0 = 0.055 * (1 - s0 * 0.85); w1 = 0.055 * (1 - s1 * 0.85)
        V.quad(p0 - side * w0, p0 + side * w0, p1 + side * w1, p1 - side * w1, jit(rib, r, 0.08))
        V.quad(p0 + upv * w0 * 0.8, p0 - side * w0, p1 - side * w1, p1 + upv * w1 * 0.8, jit(mixc(rib, cD, 0.3), r, 0.08))
    # lá chét
    for i in range(n_pairs):
        s = 0.12 + 0.86 * (i / (n_pairs - 1))
        p = P(s); t = T(s); side = t.cross(Vector((0, 0, 1))).normalized(); dn = Vector((0, 0, -1))
        env = math.sin(math.pi * min(1, (s - 0.06) / 0.94)) ** 0.7       # dài ở giữa, ngắn hai đầu
        ln = leaf_len * (0.35 + 0.65 * env) * r.uniform(0.85, 1.1)
        for sd in (-1, 1):
            # hướng lá chét: ra hai bên + nghiêng về ngọn + rủ xuống (chữ V)
            d = (side * sd * 0.8 + t * 0.4 + dn * (0.3 + 0.3 * s + hang)).normalized()
            d.rotate(Matrix.Rotation(r.uniform(-0.3, 0.3) + twist * sd, 3, t))      # lá chét lệch góc tự nhiên (xù)
            ln *= r.uniform(0.75, 1.12)
            w = 0.05 * (0.6 + 0.4 * env)
            nseg = 3; pts = []; sag = 0.3 + hang * 0.4 + r.uniform(0, 0.15)
            for k in range(nseg + 1):
                u = k / nseg
                q = p + d * (ln * u) + Vector((0, 0, -sag * ln * u * u))   # đầu lá rủ thêm
                pts.append(q)
            nrm = d.cross(t).normalized()
            base_c = mixc(cD, cR, 0.35 + 0.4 * s)
            for k in range(nseg):
                u0, u1 = k / nseg, (k + 1) / nseg
                ww0 = w * (1 - u0 * 0.85); ww1 = w * (1 - u1 * 0.85)
                c0 = jit(mixc(base_c, cL, u0 * 0.6), r, 0.07); c1 = jit(mixc(base_c, cL, u1 * 0.75), r, 0.07)
                a0, a1 = pts[k], pts[k + 1]
                wv0 = t * ww0; wv1 = t * ww1
                if k < nseg - 1:
                    V.quad(a0 - wv0, a0 + wv0, a1 + wv1, a1 - wv1, c0, c0, c1, c1)
                else:
                    V.tri(a0 - wv0, a0 + wv0, a1, c0, c0, c1)

def crown_core(V, r, top, R0, style):
    """Bẹ lá + chùm dừa ở ngọn: búp bẹ nâu, 3 cụm dừa (xanh/vàng/nâu), cuống bẹ cũ."""
    sheath = hexrgb('#8A6A40'); nut = [hexrgb('#5E7A2E'), hexrgb('#9A8A2A'), hexrgb('#7A5A30')]
    seg = 12
    for j in range(6):
        z0 = -0.15 + j * 0.12; z1 = z0 + 0.12; rr0 = R0 * (1.25 + 0.2 * math.sin(j)); rr1 = R0 * (1.2 - j * 0.12)
        for k in range(seg):
            a0 = 2 * math.pi * k / seg; a1 = 2 * math.pi * (k + 1) / seg
            V.quad(top + Vector((rr0 * math.cos(a0), rr0 * math.sin(a0), z0)), top + Vector((rr0 * math.cos(a1), rr0 * math.sin(a1), z0)),
                   top + Vector((rr1 * math.cos(a1), rr1 * math.sin(a1), z1)), top + Vector((rr1 * math.cos(a0), rr1 * math.sin(a0), z1)), jit(sheath, r, 0.1))
    for c in range(3 if style != 'B' else 4):
        a = c * 2.1 + r.uniform(-0.3, 0.3)
        for k in range(r.randint(3, 5)):
            ctr = top + Vector((math.cos(a) * (R0 + 0.12) + r.uniform(-0.06, 0.06), math.sin(a) * (R0 + 0.12) + r.uniform(-0.06, 0.06), -0.25 - k * 0.07 + r.uniform(-0.04, 0.04)))
            sphere(V, ctr, 0.13 * r.uniform(0.9, 1.1), jit(nut[r.randrange(3)], r, 0.1))

def sphere(V, c, rad, col, seg=8, rings=5):
    pts = [[c + Vector((rad * math.sin(math.pi * i / rings) * math.cos(2 * math.pi * k / seg), rad * math.sin(math.pi * i / rings) * math.sin(2 * math.pi * k / seg), rad * 1.1 * math.cos(math.pi * i / rings))) for k in range(seg)] for i in range(rings + 1)]
    for i in range(rings):
        for k in range(seg):
            k2 = (k + 1) % seg
            V.quad(pts[i][k], pts[i][k2], pts[i + 1][k2], pts[i + 1][k], col)

# ------------------------------------------------------------------------------------------------ cây hoàn chỉnh
STYLES = {   # (cao thân, nghiêng ngọn, cong S, số tàu, dài tàu, độ rủ)
    'A': dict(H=7.0, lean=-0.6, bend=0.55, n=48, L=4.8, droop=0.6, pairs=48, leaf=1.4, hang=0.55),    # sát ảnh mẫu
    'B': dict(H=6.8, lean=-0.3, bend=0.25, n=46, L=4.9, droop=0.55, pairs=48, leaf=1.45, hang=0.45),  # tán xanh tươi, thân thẳng hơn
    'C': dict(H=8.0, lean=-1.05, bend=0.75, n=44, L=5.1, droop=0.65, pairs=46, leaf=1.4, hang=0.6),   # cao, nghiêng mạnh, vài tàu khô
}

def build_palm(style='A', seed=1, loc=(0, 0, 0), coll=None, mound_on=True):
    P = STYLES[style]; r = random.Random(seed)
    coll = coll or bpy.context.scene.collection
    V = VC('CayDua_%s' % style); mat = vc_material()
    top, tdir, rtop = trunk(V, r, P['H'], 0.24, 0.15, P['lean'], P['bend'])
    if mound_on: mound(V, r)
    crown_core(V, r, top, rtop, style)
    n = P['n']; golden = 2.39996
    for i in range(n):
        az = i * golden + r.uniform(-0.3, 0.3)
        # tầng: tàu non đứng (elev cao), tàu già rủ (elev âm)
        layer = i / (n - 1)
        elev = math.radians(min(42, 52 - 80 * layer + r.uniform(-10, 10)))  # tàu non vươn cao (≤ 42°, không dựng như ngọn thông) → tàu già chúc xuống
        L = P['L'] * (0.75 + 0.35 * math.sin(math.pi * min(1, 0.25 + layer))) * r.uniform(0.9, 1.08)
        droop = P['droop'] * (0.5 + P['hang'] * layer ** 1.5)             # cong vòm; tàu già rủ thõng (ảnh: rủ tới ~70 % chiều cao)
        cols = frond_colour(az, math.sin(elev), r, style)
        if style == 'A' and layer > 0.8 and -math.cos(az) > -0.2 and r.random() < 0.6: cols = PAL['dry']     # tàu già nâu cam rủ dưới (như ảnh)
        frond(V, r, top + Vector((0, 0, 0.1)), az, elev, L, droop, cols, n_pairs=P['pairs'], leaf_len=P['leaf'] * (0.8 + 0.35 * layer), twist=0.25, hang=0.45 * layer)
    ob = V.finish(coll, mat); ob.location = loc
    return ob

# ------------------------------------------------------------------------------------------------ bệ cây
def planter(style='P1', loc=(0, 0, 0), coll=None, seed=3, size=2.6):
    """P1: bệ đá granite vuông bo góc vát, gờ ngồi ốp đá mài, lòng đất phủ vỏ cây nâu + sỏi trắng + cỏ lá gừng/cỏ lông heo.
    P2: bệ bát giác gạch nung đỏ + nắp đá xám, đèn hắt chân cây, viền sỏi đen + cỏ nhật."""
    r = random.Random(seed); coll = coll or bpy.context.scene.collection
    V = VC('BeCay_%s' % style); mat = vc_material()
    h = 0.48; wall = 0.2; half = size / 2
    if style == 'P1':
        body = hexrgb('#B9B2A6'); cap = hexrgb('#D8D2C6'); dark = hexrgb('#8C857A'); edge = hexrgb('#6E675E')
        sides = 4; R = half * math.sqrt(2); rot0 = math.pi / 4
    else:
        body = hexrgb('#A9543A'); cap = hexrgb('#9A9A96'); dark = hexrgb('#7A3A28'); edge = hexrgb('#5A5A56')
        sides = 8; R = half / math.cos(math.pi / 8); rot0 = math.pi / 8
    def ring(rad, z): return [Vector((rad * math.cos(rot0 + 2 * math.pi * k / sides), rad * math.sin(rot0 + 2 * math.pi * k / sides), z)) for k in range(sides)]
    def band(a, b, ca, cb=None):
        for k in range(sides):
            k2 = (k + 1) % sides; V.quad(a[k], a[k2], b[k2], b[k], ca, ca, cb or ca, cb or ca)
    def cap_ring(a, b, c):   # mặt nằm giữa hai vòng
        for k in range(sides):
            k2 = (k + 1) % sides; V.quad(a[k], a[k2], b[k2], b[k], c)
    Ro, Ri = R, R - wall / math.cos(math.pi / sides) * 1.0
    # chân đế lùi vào + thân + gờ vát + nắp chìa ra
    band(ring(Ro - 0.04, 0.0), ring(Ro - 0.04, 0.06), edge)
    band(ring(Ro - 0.04, 0.06), ring(Ro, 0.1), dark)
    if style == 'P2':     # gạch: các hàng gạch xen màu
        for j in range(7):
            z0 = 0.1 + j * 0.04; band(ring(Ro, z0), ring(Ro, z0 + 0.036), jit(body if j % 2 == 0 else mixc(body, dark, 0.35), r, 0.06))
            band(ring(Ro - 0.006, z0 + 0.036), ring(Ro - 0.006, z0 + 0.04), hexrgb('#CFC6B6'))
        zt = 0.1 + 7 * 0.04
    else:
        band(ring(Ro, 0.1), ring(Ro, h - 0.08), body, mixc(body, cap, 0.25))
        # rãnh trang trí ngang
        band(ring(Ro + 0.004, 0.24), ring(Ro + 0.004, 0.26), dark)
        zt = h - 0.08
    # nắp chìa 0,05 m, vát mép
    band(ring(Ro, zt), ring(Ro + 0.05, zt + 0.02), cap)
    band(ring(Ro + 0.05, zt + 0.02), ring(Ro + 0.05, zt + 0.07), cap)
    band(ring(Ro + 0.05, zt + 0.07), ring(Ro + 0.02, zt + 0.1), mixc(cap, hexrgb('#FFFFFF'), 0.2))
    cap_ring(ring(Ro + 0.02, zt + 0.1), ring(Ri - 0.02, zt + 0.1), mixc(cap, hexrgb('#FFFFFF'), 0.12))
    band(ring(Ri - 0.02, zt + 0.1), ring(Ri, zt - 0.02), mixc(cap, dark, 0.3))
    band(ring(Ri, zt - 0.02), ring(Ri, h - 0.16), dark)
    # lòng đất: phủ vỏ thông nâu (mảnh nhỏ), sỏi viền, cỏ bụi — rải theo đúng đa giác lòng bệ (bán kính theo góc)
    ap = Ri * math.cos(math.pi / sides)
    def rad_at(a): return ap / math.cos(((a - rot0) % (2 * math.pi / sides)) - math.pi / sides)
    soil = h - 0.16; mulch = [hexrgb('#7A4A28'), hexrgb('#5E3A20'), hexrgb('#94603A')]
    cap_ring(ring(Ri, soil), ring(0.02, soil + 0.04), hexrgb('#4A3020'))
    for k in range(420 if style == 'P1' else 300):
        a = r.uniform(0, 2 * math.pi); d = math.sqrt(r.random()) * (rad_at(a) - 0.12); x, y = d * math.cos(a), d * math.sin(a)
        z = soil + 0.03 + 0.04 * (1 - d / Ri); s = r.uniform(0.03, 0.06); b = r.uniform(0, math.pi)
        c, sn = math.cos(b), math.sin(b)
        V.quad((x - c * s, y - sn * s, z), (x - sn * s * 0.5, y + c * s * 0.5, z + 0.01), (x + c * s, y + sn * s, z), (x + sn * s * 0.5, y - c * s * 0.5, z - 0.005), jit(mulch[r.randrange(3)], r, 0.1))
    peb = hexrgb('#E8E4DA') if style == 'P1' else hexrgb('#3A3A38')
    for k in range(60):
        a = 2 * math.pi * k / 60 + r.uniform(-0.03, 0.03); d = rad_at(a) - r.uniform(0.06, 0.12)
        sphere(V, Vector((d * math.cos(a), d * math.sin(a), soil + 0.045)), r.uniform(0.03, 0.05), jit(peb, r, 0.08), seg=5, rings=3)
    # cụm cỏ bốn góc / tám góc
    gc = [hexrgb('#4E7A2A'), hexrgb('#6E9A3A'), hexrgb('#3E6A2A')]
    for k in range(sides):
        a = rot0 + 2 * math.pi * k / sides; d = rad_at(a) - 0.32; cx, cy = d * math.cos(a), d * math.sin(a)
        for b in range(18):
            bb = r.uniform(0, 2 * math.pi); hh = r.uniform(0.22, 0.42); lean = r.uniform(0.06, 0.16)
            base = Vector((cx + r.uniform(-0.05, 0.05), cy + r.uniform(-0.05, 0.05), soil + 0.04))
            tip = base + Vector((math.cos(bb) * lean, math.sin(bb) * lean, hh))
            w = Vector((-math.sin(bb), math.cos(bb), 0)) * 0.012
            V.tri(base - w, base + w, tip, jit(gc[b % 3], r, 0.1), jit(gc[b % 3], r, 0.1), mixc(gc[b % 3], hexrgb('#C8C86A'), 0.4))
    ob = V.finish(coll, mat); ob.location = loc
    return ob, soil

def scene_setup(cam_loc, target, lens=50, res=(1400, 1000)):
    sc = bpy.context.scene
    for eng in ('BLENDER_EEVEE_NEXT', 'BLENDER_EEVEE'):
        try: sc.render.engine = eng; break
        except TypeError: pass
    sc.render.resolution_x, sc.render.resolution_y = res
    try: sc.eevee.taa_render_samples = 32
    except Exception: pass
    sc.view_settings.view_transform = 'Standard'; sc.view_settings.exposure = 0.0
    w = sc.world or bpy.data.worlds.new('W'); sc.world = w; w.use_nodes = True
    bg = next(n for n in w.node_tree.nodes if n.type == 'BACKGROUND'); bg.inputs[0].default_value = (0.78, 0.85, 0.95, 1); bg.inputs[1].default_value = 0.55
    cam = bpy.data.objects.get('CamCayDua') or bpy.data.objects.new('CamCayDua', bpy.data.cameras.new('CamCayDua'))
    if cam.name not in sc.collection.objects: sc.collection.objects.link(cam)
    cam.data.lens = lens; cam.location = cam_loc
    cam.rotation_euler = (Vector(target) - Vector(cam_loc)).to_track_quat('-Z', 'Y').to_euler(); sc.camera = cam
    sun = bpy.data.objects.get('NangCayDua') or bpy.data.objects.new('NangCayDua', bpy.data.lights.new('NangCayDua', 'SUN'))
    if sun.name not in sc.collection.objects: sc.collection.objects.link(sun)
    sun.data.energy = 4.6; sun.data.color = (1.0, 0.88, 0.7); sun.rotation_euler = (math.radians(55), 0, math.radians(-75))   # nắng chiều từ trái (−x)
    gnd = bpy.data.objects.get('NenDuyet')
    if not gnd:
        me = bpy.data.meshes.new('NenDuyet'); bm = bmesh.new(); bmesh.ops.create_grid(bm, x_segments=1, y_segments=1, size=30); bm.to_mesh(me); bm.free()
        gnd = bpy.data.objects.new('NenDuyet', me); sc.collection.objects.link(gnd)
        gm = bpy.data.materials.new('NenDuyet'); gm.use_nodes = True; next(n for n in gm.node_tree.nodes if n.type == 'BSDF_PRINCIPLED').inputs['Base Color'].default_value = (0.55, 0.55, 0.53, 1); me.materials.append(gm)


# ================================================================================================ 21.286 vòng 2 (góp ý người dùng)
# • Thân THẲNG ĐỨNG (chỉ lượn ±3 cm), SẬM như ảnh (nâu xám, gốc sẫm), NHÁM: vòng sẹo nổi, vết nứt dọc, mảng loang, không bóng.
# • Tàu lá XÒE RỒI RŨ theo đường cong: góc cuống giảm dần dọc tàu (θ(s) = e0 − uốn·s^1,35) + cuộn ngang nhẹ, lá chét rủ cong.
# • Bệ P2 CŨ: từng viên gạch lệch màu/lồi lõm, mạch vữa bẩn, ố nước chảy dưới nắp, đất bắn + rêu chân bệ, nắp đá 8 tấm có
#   khe, loang ố, vết nứt, vết xước, sứt mép.
def _h(*a):
    """nhiễu giá trị 0..1 theo toạ độ (băm)."""
    x = math.sin(sum(v * k for v, k in zip(a, (12.9898, 78.233, 37.719, 53.11)))) * 43758.5453
    return x - math.floor(x)

def vnoise(x, y, z=0.0):
    xi, yi, zi = math.floor(x), math.floor(y), math.floor(z); xf, yf, zf = x - xi, y - yi, z - zi
    def L(a, b, t): return a + (b - a) * (t * t * (3 - 2 * t))
    c = [[[_h(xi + i, yi + j, zi + k) for k in (0, 1)] for j in (0, 1)] for i in (0, 1)]
    return L(L(L(c[0][0][0], c[0][0][1], zf), L(c[0][1][0], c[0][1][1], zf), yf),
             L(L(c[1][0][0], c[1][0][1], zf), L(c[1][1][0], c[1][1][1], zf), yf), xf)

def fbm(x, y, z=0.0, o=3):
    s, a, f, n = 0.0, 0.5, 1.0, 0.0
    for _ in range(o):
        s += a * vnoise(x * f, y * f, z * f); n += a; a *= 0.5; f *= 2.1
    return s / n

def mul(c, k): return (c[0] * k, c[1] * k, c[2] * k)

def rough_material(name, rough=0.92, spec=0.12):
    m = bpy.data.materials.get(name)
    if m: return m
    m = bpy.data.materials.new(name); m.use_nodes = True
    nt = m.node_tree; b = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    at = nt.nodes.new('ShaderNodeVertexColor'); at.layer_name = 'Col'
    nt.links.new(at.outputs['Color'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = rough
    for key in ('Specular IOR Level', 'Specular'):
        if key in b.inputs:
            b.inputs[key].default_value = spec; break
    m.use_backface_culling = False
    return m

def trunk2(V, r, H, base_r=0.25, top_r=0.155, seg=18):
    """Thân thẳng đứng, gốc phình, vòng sẹo nổi mỗi ~0,11 m, nứt dọc, loang sẫm — màu nâu xám sậm như ảnh."""
    ring_h = 0.11; rows_per = 3; n_rings = int(H / ring_h); steps = n_rings * rows_per
    ox, oy = r.uniform(0, 50), r.uniform(0, 50)
    def C(t): return Vector((0.03 * math.sin(t * 5.1 + ox), 0.03 * math.sin(t * 4.3 + oy), H * t))
    def R(t): return (top_r + (base_r - top_r) * (1 - t) ** 1.25) * (1 + 0.9 * math.exp(-t * 24) + 0.22 * math.exp(-t * 6))
    cBase = hexrgb('#4E3E31'); cMid = hexrgb('#6E5E4E'); cTop = hexrgb('#7E6A54'); cScar = hexrgb('#33281F')
    cHi = hexrgb('#9C8468'); cFis = hexrgb('#2B2219')
    fis = [r.uniform(0, 2 * math.pi) for _ in range(7)]
    fis_span = [(r.uniform(0, 0.6), r.uniform(0.15, 0.4)) for _ in fis]
    prev = None
    for i in range(steps + 1):
        t = i / steps; c = C(t); row = i % rows_per
        ring = []; cols = []
        for k in range(seg):
            a = 2 * math.pi * k / seg
            ridge = (1.0 + 0.012 * (0.4 + 0.6 * _h(i, k * 3))) if row == 0 else (1.0 if row == 1 else 0.995)  # gờ sẹo không đều, nhẹ
            bump = 1 + 0.04 * (fbm(math.cos(a) * 2 + ox, math.sin(a) * 2, t * 18) - 0.5)
            rr = R(t) * ridge * bump
            ring.append(c + Vector((math.cos(a) * rr, math.sin(a) * rr, 0)))
            base = mixc(cBase, cMid, min(1, t * 2.4)) if t < 0.42 else mixc(cMid, cTop, (t - 0.42) / 0.58)
            n = fbm(math.cos(a) * 3 + ox, math.sin(a) * 3 + oy, t * 25)
            fib = fbm(math.cos(a) * 9 + ox, math.sin(a) * 9 + oy, t * 3)                       # thớ dọc
            col = mul(base, 0.72 + 0.32 * n + 0.22 * fib)
            sun = max(0, -math.cos(a - 0.3)) * 0.18; col = mixc(col, cHi, sun)
            if row == 0 and fbm(math.cos(a) * 4 + i * 0.37, math.sin(a) * 4, ox) > 0.4: col = mixc(col, cScar, 0.22 + 0.22 * _h(i, k))   # sẹo đứt quãng, nhạt
            for (fa, (f0, fl)) in zip(fis, fis_span):
                if f0 < t < f0 + fl and abs(((a - fa + math.pi) % (2 * math.pi)) - math.pi) < 0.09: col = mixc(col, cFis, 0.7)
            if t < 0.06: col = mixc(col, hexrgb('#3E3226'), 0.4)
            cols.append(col)
        if prev:
            pr, pc = prev
            for k in range(seg):
                k2 = (k + 1) % seg; V.quad(pr[k], pr[k2], ring[k2], ring[k], pc[k], pc[k2], cols[k2], cols[k])
        prev = (ring, cols)
    return C(1.0), R(1.0)

def frond2(V, r, top, az, e0, bend, L, cols, n_pairs=48, leaf_len=1.4, hang=0.5, curl=0.0):
    """Tàu lá cong tự nhiên: hướng cuống quay dần xuống dọc tàu (vươn lên → xòe ngang → rũ), cuộn ngang nhẹ; lá chét hai bên
    mọc chữ V, rủ cong, ngắn dần về hai đầu; gốc tàu (lõi tán) sẫm, đầu lá sáng."""
    cR, cD, cL = (hexrgb(c) for c in cols)
    N = 22; pts = [top.copy()]; tans = []
    p = top.copy()
    for i in range(N):
        s = (i + 0.5) / N
        th = math.radians(e0 - bend * s ** 1.9); ph = az + curl * s * s      # vươn xa trước, uốn rũ dồn về nửa ngoài
        d = Vector((math.cos(ph) * math.cos(th), math.sin(ph) * math.cos(th), math.sin(th)))
        p = p + d * (L / N); pts.append(p.copy()); tans.append(d)
    tans.append(tans[-1])
    def P(s):
        f = s * N; i = min(N - 1, int(f)); u = f - i
        return pts[i].lerp(pts[i + 1], u), tans[i].lerp(tans[i + 1], u).normalized()
    rib = hexrgb('#B89058')
    for i in range(N):
        p0, p1, t0 = pts[i], pts[i + 1], tans[i]
        side = t0.cross(Vector((0, 0, 1))); side = side.normalized() if side.length > 1e-4 else Vector((1, 0, 0))
        upv = side.cross(t0).normalized(); w0 = 0.06 * (1 - i / N * 0.85); w1 = 0.06 * (1 - (i + 1) / N * 0.85)
        V.quad(p0 - side * w0, p0 + side * w0, p1 + side * w1, p1 - side * w1, jit(rib, r, 0.08))
        V.quad(p0 + upv * w0, p0 - side * w0, p1 - side * w1, p1 + upv * w1, jit(mixc(rib, cD, 0.4), r, 0.08))
    for i in range(n_pairs):
        s = 0.1 + 0.88 * (i / (n_pairs - 1)); p, t = P(s)
        side = t.cross(Vector((0, 0, 1))); side = side.normalized() if side.length > 1e-4 else Vector((1, 0, 0))
        env = math.sin(math.pi * min(1, (s - 0.05) / 0.95)) ** 0.65
        core = 0.62 + 0.38 * min(1, s * 2.2)
        for sd in (-1, 1):
            ln = leaf_len * (0.32 + 0.68 * env) * r.uniform(0.78, 1.12)
            d = (side * sd * 0.82 + t * 0.38 + Vector((0, 0, -1)) * (0.15 + 0.25 * s + hang * 0.35)).normalized()
            d.rotate(Matrix.Rotation(r.uniform(-0.28, 0.28) + 0.22 * sd, 3, t))
            sag = 0.35 + hang * 0.45 + r.uniform(0, 0.2); w = 0.052 * (0.6 + 0.4 * env)
            nseg = 4; q = [p + d * (ln * k / nseg) + Vector((0, 0, -sag * ln * (k / nseg) ** 2)) for k in range(nseg + 1)]
            base_c = mul(mixc(cD, cR, 0.3 + 0.45 * s), core)
            for k in range(nseg):
                u0, u1 = k / nseg, (k + 1) / nseg
                c0 = jit(mixc(base_c, cL, u0 * 0.55), r, 0.08); c1 = jit(mixc(base_c, cL, u1 * 0.7), r, 0.08)
                ww0 = t * (w * (1 - u0 * 0.8)); ww1 = t * (w * (1 - u1 * 0.8))
                if k < nseg - 1: V.quad(q[k] - ww0, q[k] + ww0, q[k + 1] + ww1, q[k + 1] - ww1, c0, c0, c1, c1)
                else: V.tri(q[k] - ww0, q[k] + ww0, q[k + 1], c0, c0, c1)

def build_palm2(seed=1, loc=(0, 0, 0), coll=None, H=6.4, n=52, name='CayDua_V2'):
    r = random.Random(seed); coll = coll or bpy.context.scene.collection
    Vt = VC(name + '_Than'); Vl = VC(name + '_La')
    top, rtop = trunk2(Vt, r, H)
    crown_core(Vl, r, top, rtop, 'A')
    golden = 2.39996
    for i in range(n):
        az = i * golden + r.uniform(-0.3, 0.3); layer = i / (n - 1)
        e0 = min(55, 56 - 46 * layer + r.uniform(-8, 8))
        bend = 58 + 82 * layer + r.uniform(-10, 10)
        L = (4.6 + 1.0 * math.sin(math.pi * min(1, 0.3 + layer))) * r.uniform(0.9, 1.08)
        cols = frond_colour(az, math.sin(math.radians(e0 - bend * 0.5)), r, 'A')
        if layer > 0.78 and -math.cos(az) > -0.2 and r.random() < 0.6: cols = PAL['dry']
        frond2(Vl, r, top + Vector((0, 0, 0.08)), az, e0, bend, L, cols, n_pairs=46,
               leaf_len=1.35 * (0.85 + 0.3 * layer), hang=0.15 + 0.35 * layer, curl=r.uniform(-0.25, 0.25))
    ot = Vt.finish(coll, rough_material('CD_Than', 0.95, 0.08)); ol = Vl.finish(coll, rough_material('CD_La', 0.7, 0.25))
    for o in (ot, ol): o.location = loc
    return ot, ol

def planter_p2_aged(loc=(0, 0, 0), coll=None, seed=7, size=2.6):
    """Bệ bát giác gạch nung CŨ: chân bê tông bẩn; 5 hàng gạch so le (lệch màu, lồi/lõm, vài viên sứt/nứt), mạch vữa bẩn;
    ố nước chảy thành vệt dưới nắp; đất bắn + rêu chân bệ; nắp đá xám 8 tấm có khe, loang ố, địa y, nứt, xước, sứt mép;
    lòng: đất, vỏ thông, sỏi đen, cỏ, lá dừa khô rụng."""
    r = random.Random(seed); coll = coll or bpy.context.scene.collection
    V = VC('BeCay_P2_Cu'); mat = rough_material('CD_Be', 0.96, 0.06)
    sides = 8; half = size / 2; R = half / math.cos(math.pi / sides); rot0 = math.pi / 8
    def corner(rad, k, z):
        a = rot0 + 2 * math.pi * k / sides; return Vector((rad * math.cos(a), rad * math.sin(a), z))
    def edge_pt(rad, k, u, z): return corner(rad, k, z).lerp(corner(rad, k + 1, z), u)
    mortar = hexrgb('#8A8174'); brick = [hexrgb('#8E4A36'), hexrgb('#7E4232'), hexrgb('#9A5A42'), hexrgb('#6E3A2C'), hexrgb('#8A5640')]
    dirt = hexrgb('#3E3026'); moss = hexrgb('#4A5A22'); stain = hexrgb('#2E2620'); grime = hexrgb('#5A4A3E'); salt = hexrgb('#D8D0C0')
    z0, z1 = 0.1, 0.4; Ro = R
    STREAKS = [(r.randrange(sides), r.uniform(0.08, 0.92), r.uniform(0.14, 0.3)) for _ in range(22)]
    def weather(col, pos, k, u, z):
        n = fbm(pos.x * 3.1, pos.y * 3.1, z * 6)
        col = mul(col, 0.68 + 0.42 * n)                                                        # loang bụi
        col = mixc(col, grime, 0.25 + 0.15 * fbm(pos.x * 0.9 + 4, pos.y * 0.9, 5))               # xỉn màu chung
        if fbm(pos.x * 4.2 + 11, pos.y * 4.2, z * 9) > 0.68: col = mixc(col, salt, 0.35)       # vệt vôi/muối trắng
        foot = max(0.0, 1 - z / 0.22); col = mixc(col, dirt, 0.8 * foot * (0.55 + 0.45 * n))    # đất bắn chân bệ
        mo = fbm(pos.x * 1.3 + 7, pos.y * 1.3, 2)
        if mo > 0.55 and z < 0.28: col = mixc(col, moss, 0.75 * (1 - z / 0.28) * min(1, (mo - 0.55) * 6))   # rêu từng mảng
        st = 0.0
        for (sk, su, sl) in STREAKS:
            if sk == k: st = max(st, math.exp(-((u - su) / 0.04) ** 2) * max(0, (z - (z1 - sl)) / sl))
        return mixc(col, stain, 0.75 * st)                                                    # vệt nước chảy dưới nắp
    # chân đế bê tông
    for k in range(sides):
        for (ra, za, rb, zb) in ((Ro - 0.05, 0.0, Ro - 0.05, 0.07), (Ro - 0.05, 0.07, Ro, 0.1)):
            for j in range(6):
                u0, u1 = j / 6, (j + 1) / 6
                a0, a1 = edge_pt(ra, k, u0, za), edge_pt(ra, k, u1, za); b1, b0 = edge_pt(rb, k, u1, zb), edge_pt(rb, k, u0, zb)
                base = hexrgb('#6E6A62')
                V.quad(a0, a1, b1, b0, weather(base, a0, k, u0, za), weather(base, a1, k, u1, za), weather(base, b1, k, u1, zb), weather(base, b0, k, u0, zb))
    # mạch vữa (mặt nền lùi 0,012) + gạch so le nổi lên
    rows = 5; rh = (z1 - z0) / rows
    for k in range(sides):
        for j in range(4):
            for m in range(3):
                u0, u1 = j / 4, (j + 1) / 4; za, zb = z0 + (z1 - z0) * m / 3, z0 + (z1 - z0) * (m + 1) / 3
                a0, a1, b1, b0 = edge_pt(Ro - 0.012, k, u0, za), edge_pt(Ro - 0.012, k, u1, za), edge_pt(Ro - 0.012, k, u1, zb), edge_pt(Ro - 0.012, k, u0, zb)
                V.quad(a0, a1, b1, b0, weather(mortar, a0, k, u0, za), weather(mortar, a1, k, u1, za), weather(mortar, b1, k, u1, zb), weather(mortar, b0, k, u0, zb))
        nb = 5
        for row in range(rows):
            off = 0.5 / nb if row % 2 else 0.0
            for b in range(nb + 1):
                u0 = max(0.0, b / nb - off) + 0.004; u1 = min(1.0, (b + 1) / nb - off) - 0.004
                if u1 - u0 < 0.03: continue
                za = z0 + row * rh + 0.004; zb = za + rh - 0.008
                pop = r.uniform(-0.004, 0.006); chipped = r.random() < 0.12; cracked = r.random() < 0.1
                col = jit(brick[r.randrange(len(brick))], r, 0.08)
                if r.random() < 0.08: col = mixc(col, hexrgb('#5A2A1E'), 0.5)
                rr = Ro + pop
                for gx in range(3):
                    for gz in range(2):
                        ua, ub = u0 + (u1 - u0) * gx / 3, u0 + (u1 - u0) * (gx + 1) / 3
                        zc, zd = za + (zb - za) * gz / 2, za + (zb - za) * (gz + 1) / 2
                        c4 = []
                        for (uu, zz) in ((ua, zc), (ub, zc), (ub, zd), (ua, zd)):
                            q = edge_pt(rr, k, uu, zz); cc = weather(col, q, k, uu, zz)
                            if chipped and gx == 2 and gz == 1: cc = mixc(cc, hexrgb('#C9A68A'), 0.45)
                            c4.append(cc)
                        V.quad(edge_pt(rr, k, ua, zc), edge_pt(rr, k, ub, zc), edge_pt(rr, k, ub, zd), edge_pt(rr, k, ua, zd), *c4)
                V.quad(edge_pt(Ro - 0.012, k, u0, zb), edge_pt(Ro - 0.012, k, u1, zb), edge_pt(rr, k, u1, zb), edge_pt(rr, k, u0, zb), mul(col, 0.85))
                V.quad(edge_pt(Ro - 0.012, k, u0, za), edge_pt(rr, k, u0, za), edge_pt(rr, k, u0, zb), edge_pt(Ro - 0.012, k, u0, zb), mul(col, 0.7))
                if cracked:
                    pa = edge_pt(rr + 0.001, k, u0 + (u1 - u0) * 0.3, za + 0.01); pb = edge_pt(rr + 0.001, k, u0 + (u1 - u0) * 0.65, zb - 0.01)
                    dv = (pb - pa); tw = Vector((0, 0, 1)).cross(dv).normalized() * 0.0035
                    mid = pa.lerp(pb, 0.5) + tw * 2.2
                    V.quad(pa - tw, pa + tw, mid + tw, mid - tw, hexrgb('#2A1E16')); V.quad(mid - tw, mid + tw, pb + tw, pb - tw, hexrgb('#2A1E16'))
    # nắp đá: 8 tấm, khe 6 mm; mặt trên lưới 6×3 loang ố / địa y; mép vát; nứt, xước, sứt
    capc = hexrgb('#7E7C76'); lichen = hexrgb('#A8A266'); dark = hexrgb('#46433E')
    zt = z1; Rin = Ro - 0.34; zt1 = zt + 0.09
    def capcol(q):
        n = fbm(q.x * 2.4 + 3, q.y * 2.4, 1.7); c = mul(capc, 0.72 + 0.45 * n)
        if fbm(q.x * 5 + 9, q.y * 5, 3) > 0.62: c = mixc(c, lichen, 0.45)
        dk = fbm(q.x * 1.6 + 2, q.y * 1.6, 8)
        if dk < 0.4: c = mixc(c, dark, 0.55 * (0.4 - dk) / 0.4 + 0.15)                       # ố đen loang
        c = mixc(c, hexrgb('#5E5850'), 0.2)
        return c
    for k in range(sides):
        ga, gb = 0.006, 0.994
        def P(rad, u, z): return edge_pt(rad, k, u, z)
        nx, ny = 6, 3
        grid = [[P(Rin + (Ro + 0.05 - Rin) * (j / ny), ga + (gb - ga) * (i / nx), zt1) for i in range(nx + 1)] for j in range(ny + 1)]
        for j in range(ny):
            for i in range(nx):
                a, b, c_, d = grid[j][i], grid[j][i + 1], grid[j + 1][i + 1], grid[j + 1][i]
                V.quad(a, b, c_, d, capcol(a), capcol(b), capcol(c_), capcol(d))
        for i in range(nx):
            u0, u1 = ga + (gb - ga) * i / nx, ga + (gb - ga) * (i + 1) / nx
            o0, o1 = P(Ro + 0.05, u0, zt1), P(Ro + 0.05, u1, zt1)
            m0, m1 = P(Ro + 0.07, u0, zt1 - 0.025), P(Ro + 0.07, u1, zt1 - 0.025)
            l0, l1 = P(Ro + 0.07, u0, zt), P(Ro + 0.07, u1, zt)
            V.quad(o0, o1, m1, m0, capcol(o0), capcol(o1), mul(capcol(m1), 0.9), mul(capcol(m0), 0.9))
            V.quad(m0, m1, l1, l0, mul(capcol(m0), 0.82), mul(capcol(m1), 0.82), mul(mixc(capc, stain, 0.4), 0.8), mul(mixc(capc, stain, 0.4), 0.8))
            i0, i1 = P(Rin, u0, zt1), P(Rin, u1, zt1); j0, j1 = P(Rin, u0, zt - 0.05), P(Rin, u1, zt - 0.05)
            V.quad(i1, i0, j0, j1, mul(capc, 0.6), mul(capc, 0.6), mul(dark, 0.9), mul(dark, 0.9))
        if r.random() < 0.55:
            u = r.uniform(0.2, 0.8); pts = [P(Rin + 0.02, u, zt1 + 0.0015)]
            for s in range(1, 5): pts.append(P(Rin + (Ro + 0.04 - Rin) * s / 4, u + r.uniform(-0.05, 0.05), zt1 + 0.0015))
            for a, b in zip(pts, pts[1:]):
                tw = (b - a).cross(Vector((0, 0, 1))).normalized() * 0.003; V.quad(a - tw, a + tw, b + tw, b - tw, hexrgb('#2E2B27'))
        for _ in range(r.randint(2, 5)):
            u = r.uniform(0.05, 0.95); rr = r.uniform(Rin + 0.05, Ro)
            a = P(rr, u, zt1 + 0.0012); b = P(rr + r.uniform(-0.08, 0.08), u + r.uniform(-0.08, 0.08), zt1 + 0.0012)
            tw = (b - a).cross(Vector((0, 0, 1))).normalized() * 0.0016; V.quad(a - tw, a + tw, b + tw, b - tw, hexrgb('#C8C4BA'))
        if r.random() < 0.4:
            u = r.uniform(0.1, 0.9)
            V.tri(P(Ro + 0.05, u - 0.04, zt1 + 0.001), P(Ro + 0.05, u + 0.04, zt1 + 0.001), P(Ro - 0.02, u, zt1 + 0.001), hexrgb('#6A665E'))
    # lòng bệ
    ap = Rin * math.cos(math.pi / sides)
    def rad_at(a): return ap / math.cos(((a - rot0) % (2 * math.pi / sides)) - math.pi / sides)
    soil = zt1 - 0.08
    for k in range(sides):
        V.tri(Vector((0, 0, soil + 0.03)), corner(Rin, k, soil), corner(Rin, k + 1, soil), hexrgb('#3E2A1C'), hexrgb('#4A3222'), hexrgb('#4A3222'))
    mulch = [hexrgb('#6A4226'), hexrgb('#4E3220'), hexrgb('#84583A')]
    for _ in range(320):
        a = r.uniform(0, 2 * math.pi); d = math.sqrt(r.random()) * (rad_at(a) - 0.1); x, y = d * math.cos(a), d * math.sin(a)
        z = soil + 0.02 + 0.03 * (1 - d / Rin); s = r.uniform(0.03, 0.06); b = r.uniform(0, math.pi); c, sn = math.cos(b), math.sin(b)
        V.quad((x - c * s, y - sn * s, z), (x - sn * s * 0.5, y + c * s * 0.5, z + 0.01), (x + c * s, y + sn * s, z), (x + sn * s * 0.5, y - c * s * 0.5, z - 0.005), jit(mulch[r.randrange(3)], r, 0.1))
    for k in range(70):
        a = 2 * math.pi * k / 70 + r.uniform(-0.03, 0.03); d = rad_at(a) - r.uniform(0.05, 0.11)
        sphere(V, Vector((d * math.cos(a), d * math.sin(a), soil + 0.035)), r.uniform(0.028, 0.045), jit(hexrgb('#34322F'), r, 0.15), seg=5, rings=3)
    gc = [hexrgb('#4E7A2A'), hexrgb('#6E8A3A'), hexrgb('#3E6A2A'), hexrgb('#8A8A4A')]
    for k in range(sides):
        a = rot0 + 2 * math.pi * k / sides + 0.2; d = rad_at(a) - 0.3; cx, cy = d * math.cos(a), d * math.sin(a)
        for b in range(16):
            bb = r.uniform(0, 2 * math.pi); hh = r.uniform(0.18, 0.38); ln = r.uniform(0.06, 0.16)
            base = Vector((cx + r.uniform(-0.05, 0.05), cy + r.uniform(-0.05, 0.05), soil + 0.03))
            tip = base + Vector((math.cos(bb) * ln, math.sin(bb) * ln, hh)); w = Vector((-math.sin(bb), math.cos(bb), 0)) * 0.012
            V.tri(base - w, base + w, tip, jit(gc[b % 4], r, 0.12), jit(gc[b % 4], r, 0.12), mixc(gc[b % 4], hexrgb('#C8C06A'), 0.45))
    for _ in range(6):
        a = r.uniform(0, 2 * math.pi); d = r.uniform(0.35, max(0.4, rad_at(a) - 0.25)); x, y = d * math.cos(a), d * math.sin(a); b = r.uniform(0, math.pi)
        dv = Vector((math.cos(b), math.sin(b), 0)) * r.uniform(0.25, 0.45); tw = Vector((-math.sin(b), math.cos(b), 0)) * 0.025
        p0 = Vector((x, y, soil + 0.05)); V.quad(p0 - dv - tw, p0 - dv + tw, p0 + dv + tw * 0.3, p0 + dv - tw * 0.3, jit(hexrgb('#A07A44'), r, 0.15))
    ob = V.finish(coll, mat); ob.location = loc
    return ob, soil
