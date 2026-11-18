# Đợt 5 (21.248): dựng TOÀN BỘ bản đồ mở 1,3 × 1,3 km theo layout.json.
# Tọa độ thế giới = tọa độ bản đồ: Blender x = map X (đông), Blender y = map Z (bắc), z = lên. Unity: x = X, z = Z.
# Nạp build_ref1_segment.py (kéo theo ảnh 2, ảnh 3, ow_benthanh, ow_stalls, ow_streetcafe) làm thư viện.
# Mỗi tuyến đường (đều nằm ngang/dọc) dựng trong hệ cục bộ riêng: x = ngang đường (+x = bên phải khi đi theo chiều tăng),
# y = dọc đường; xong xoay vào thế giới. Nhà phố/bảng hiệu/cao ốc là thư viện mẫu dựng một lần rồi đặt instance.
import bpy, math, random, os, json
from mathutils import Vector, Matrix

OW_DIR = globals().get('OW_DIR') or os.path.dirname(os.path.abspath(__file__))
NO_RUN1 = True
_base = os.path.join(OW_DIR, 'build_ref1_segment.py')
exec(compile(open(_base, encoding='utf-8').read(), _base, 'exec'), globals())
NO_RUN1 = False

PROJECT = os.path.normpath(os.path.join(OW_DIR, '..', '..', '..'))
LAYOUT = json.load(open(os.path.join(PROJECT, 'ArtSource', 'Concepts', 'OpenWorld-20261117', 'layout.json'), encoding='utf-8'))
PARTSW = globals().get('PARTSW') or ['scene', 'ground', 'roads', 'river', 'bridges', 'library', 'buildings', 'showcase', 'blocks', 'street', 'cameras']
ROOT = 'OW_Map'
WR = random.Random(20261118)

# ---------------------------------------------------------------- mạng đường
class Road:
    def __init__(self, d):
        self.id = d['id']; self.name = d['name']; p = LAYOUT['roadProfiles'][d['profile']]
        self.main = d['profile'] == 'main'; self.cw = p['carriageway']; self.walk = p['sidewalk']
        (x0, z0), (x1, z1) = d['from'], d['to']
        self.ns = abs(x0 - x1) < 1e-6
        self.c = x0 if self.ns else z0
        self.a, self.b = (min(z0, z1), max(z0, z1)) if self.ns else (min(x0, x1), max(x0, x1))
        self.L = self.b - self.a
        self.yaw = 0.0 if self.ns else -math.pi / 2
    def W(self, lx, ly):
        """cục bộ → thế giới."""
        return (self.c + lx, self.a + ly) if self.ns else (self.a + ly, self.c - lx)
    def half(self): return self.cw / 2 + self.walk

ROADS = [Road(d) for d in LAYOUT['roads']]
RD = {r.id: r for r in ROADS}

def crossings(r):
    """[(along, nửa bề rộng đường cắt, road)] — đường vuông góc cắt/chạm tuyến r."""
    out = []
    for q in ROADS:
        if q is r or q.ns == r.ns: continue
        if r.a - 1 <= q.c <= r.b + 1 and q.a - 1 <= r.c <= q.b + 1:
            out.append((q.c - r.a, q.half(), q))
    return sorted(out, key=lambda t: t[0])

def on_road_corridor(q, wx, wz):
    if q.ns: return abs(wx - q.c) <= q.half() and q.a - 0.5 <= wz <= q.b + 0.5
    return abs(wz - q.c) <= q.half() and q.a - 0.5 <= wx <= q.b + 0.5

# Nhịp cầu (theo trục dọc của tuyến): mặt đất đường/vỉa hè bỏ qua đoạn này.
BRIDGE_SPANS = {'R3': (229.0 - (-560), 406.0 - (-560)), 'R4': (223.5 - (-560), 405.0 - (-560))}
# Phía giáp sông (không dựng nhà): (road, side) → lối ven sông.
RIVER_SIDES = {('R2', 1), ('R3', -1), ('E1', -1), ('N1', 1)}
# Mép bản đồ: không dựng nhà phía ngoài.
EDGE_SIDES = {('P1', -1), ('P2', 1), ('E3', 1), ('E4', 1), ('N2', -1), ('N3', -1)}

def subtract(runs, a, b):
    out = []
    for s0, s1 in runs:
        if b <= s0 or a >= s1: out.append((s0, s1)); continue
        if a > s0: out.append((s0, a))
        if b < s1: out.append((b, s1))
    return out

def side_runs(r, s, pad=0.0):
    """Các đoạn dọc tuyến r phía s không bị đường cắt (kể cả ngã ba chỉ một phía) và không trên cầu."""
    runs = [(0.0, r.L)]
    lx = s * (r.cw / 2 + r.walk / 2)
    for along, hq, q in crossings(r):
        wx, wz = r.W(lx, along)
        if on_road_corridor(q, wx, wz): runs = subtract(runs, along - hq - pad, along + hq + pad)
    if r.id in BRIDGE_SPANS: runs = subtract(runs, *BRIDGE_SPANS[r.id])
    return [(a, b) for a, b in runs if b - a > 0.5]

def road_obj(mb, C, r):
    ob = mb.finish(C, loc=(r.W(0, 0)[0], r.W(0, 0)[1], 0)); ob.rotation_euler = (0, 0, r.yaw); return ob

# ---------------------------------------------------------------- 1. nền
def build_ground_w(C):
    """Nền chừa lòng sông: đất khu phố (bê tông, mặt −0,08) + cỏ ngoài mép bản đồ (−0,15); sông kéo dài ra ngoài mép."""
    g = MB('OW_Map_Nen')
    for x0, z0, x1, z1 in ((-600, -600, 234.5, 228.5), (400, -600, 700, 700), (-600, 400, 400, 700)):
        g.boxab((x0, z0, -0.6), (x1, z1, -0.08), M['concrete'])
    for x0, z0, x1, z1 in ((-1400, -1400, 234.5, -600), (400, -1400, 2100, -600), (-1400, 700, 2100, 2100), (700, -600, 2100, 700),
                           (-1400, -600, -600, 228.5), (-1400, 400, -600, 700)):
        g.boxab((x0, z0, -0.7), (x1, z1, -0.15), M['grass'])
    g.finish(C)
    w = MB('OW_Map_SongNgoaiMep')
    for x0, z0, x1, z1 in ((234.5, -1400, 400, -600), (-1400, 228.5, -600, 400)):
        w.boxab((x0, z0, WATER_Z - 3), (x1, z1, WATER_Z), M['water'])
    w.finish(C)

# ---------------------------------------------------------------- 2. đường, vỉa hè, vạch, vạch qua đường
def build_roads_w(C):
    for r in ROADS:
        hw = r.cw / 2
        cr = crossings(r)
        a_ = MB('OW_Duong_%s_MatDuong' % r.id)
        runs = [(0.0, r.L)]
        if not r.ns:      # đường đông–tây nhường ô giao lộ cho đường bắc–nam để khỏi chồng mặt
            for along, hq, q in cr: runs = subtract(runs, along - q.cw / 2, along + q.cw / 2)
        if r.id in BRIDGE_SPANS: runs = subtract(runs, *BRIDGE_SPANS[r.id])
        for s0, s1 in runs: a_.boxab((-hw, s0, -0.3), (hw, s1, 0.0), M['asphalt'])
        for i in range(int(r.L / 40)):
            y = WR.uniform(0, r.L)
            if any(s0 < y < s1 for s0, s1 in runs):
                a_.box((WR.uniform(-hw + 1, hw - 1), y, 0.004), (WR.uniform(0.8, 2.4), WR.uniform(1, 4), 0.006), M['asphalt_patch'], rz=WR.uniform(-0.2, 0.2))
        road_obj(a_, C, r)
        # Vạch sơn (đi qua cầu, ngắt ở giao lộ).
        lm = MB('OW_Duong_%s_VachSon' % r.id)
        mruns = [(0.0, r.L)]
        for along, hq, q in cr: mruns = subtract(mruns, along - q.cw / 2 - 2, along + q.cw / 2 + 2)
        for s0, s1 in mruns:
            if r.main:
                for x in (-0.13, 0.13): lm.box((x, (s0 + s1) / 2, 0.006), (0.12, s1 - s0, 0.01), M['yellow'])
                for x in (-3.5, 3.5):
                    y = s0 + 1
                    while y < s1 - 3: lm.box((x, y + 1.5, 0.006), (0.13, 3.0, 0.01), M['white']); y += 9
            else:
                y = s0 + 1
                while y < s1 - 3: lm.box((0, y + 1.5, 0.006), (0.12, 3.0, 0.01), M['yellow']); y += 7
            for x in (-(hw - 0.25), hw - 0.25): lm.box((x, (s0 + s1) / 2, 0.006), (0.15, s1 - s0, 0.01), M['white'])
        for along, hq, q in cr:          # vạch người đi bộ hai phía giao lộ
            for d in (-1, 1):
                y = along + d * (q.cw / 2 + 1.6)
                if 0 < y < r.L:
                    n = int(r.cw / 1.1)
                    for k in range(n): lm.box((-hw + 0.55 + k * 1.1, y, 0.006), (0.55, 2.6, 0.01), M['white'])
        road_obj(lm, C, r)
        # Vỉa hè + bó vỉa (R3 sơn đỏ–trắng như ảnh 1).
        sw = MB('OW_Duong_%s_ViaHe' % r.id)
        for s in (-1, 1):
            pav = M['brick_red'] if (r.id == 'R3' and s < 0) else M['paving']
            for s0, s1 in side_runs(r, s):
                lo, hi = sorted((s * (hw + 0.25), s * (hw + r.walk)))
                sw.boxab((lo, s0, 0.0), (hi, s1, 0.15), pav)
                if r.id == 'R3': striped_curb(sw, s * hw, s0, s1, s)
                else:
                    lo, hi = sorted((s * hw, s * (hw + 0.25)))
                    sw.boxab((lo, s0, -0.05), (hi, s1, 0.17), M['curb'])
        road_obj(sw, C, r)

# ---------------------------------------------------------------- 3. sông, kè, lan can, lối ven sông
def build_river_w(C):
    pts = [Vector((p[0], p[1], WATER_Z)) for p in LAYOUT['river']['outline']]
    import bmesh
    bm = bmesh.new(); vs = [bm.verts.new(p) for p in pts]; bm.faces.new(vs)
    me = bpy.data.meshes.new('OW_Map_Song'); bm.to_mesh(me); bm.free(); me.materials.append(M['water'])
    ob = bpy.data.objects.new('OW_Map_Song', me); C.objects.link(ob)
    # Lòng sông (đáy) để nhìn xuyên nước không thấy trống.
    bm = bmesh.new(); vs = [bm.verts.new(p - Vector((0, 0, 4))) for p in pts]; bm.faces.new(vs)
    me = bpy.data.meshes.new('OW_Map_DaySong'); bm.to_mesh(me); bm.free(); me.materials.append(M['bank'])
    C.objects.link(bpy.data.objects.new('OW_Map_DaySong', me))
    b = LAYOUT['bounds']
    k = MB('OW_Map_KeSong'); seg_pieces = []
    gaps = [((234.5, 215.0), 12.5), ((400.0, 215.0), 12.5), ((-150.0, 228.5), 12.5), ((-150.0, 400.0), 12.5)]   # chân cầu
    n = len(pts)
    for i in range(n):
        p, q = pts[i], pts[(i + 1) % n]
        if (abs(p.x - q.x) < 1e-3 and abs(abs(p.x) - 600) < 1e-3) or (abs(p.y - q.y) < 1e-3 and abs(abs(p.y) - 600) < 1e-3):
            continue      # mép bản đồ
        d = q - p; L = d.length
        if L < 0.1: continue
        t = d.normalized(); nrm = Vector((t.y, -t.x, 0))   # pháp tuyến hướng ra bờ (đa giác đi ngược chiều kim đồng hồ? kiểm tra dưới)
        # Thân kè + mũ đá theo cạnh.
        seg_runs = [(0.0, L)]
        for (gx, gz), gr in gaps:
            g = Vector((gx, gz, WATER_Z)); u = (g - p).dot(t)
            if 0 - gr < u < L + gr and abs((g - p).dot(nrm)) < 3: seg_runs = subtract(seg_runs, u - gr, u + gr)
        for s0, s1 in [(0.0, L)]:
            c0 = p + t * s0; c1 = p + t * s1
            for (z0, z1, wdt, m) in ((WATER_Z - 1, 0.15, 0.6, M['bank']), (0.15, 0.3, 0.8, M['granite'])):
                quad_wall(k, c0, c1, nrm, wdt, z0, z1, m)
        for s0, s1 in seg_runs:
            if s1 - s0 < 2: continue
            # 21.250: mỗi đoạn lan can ≤ 60 m là một lưới riêng → Unity ẩn đoạn xa/ngoài khung nhìn.
            npc = max(1, int(math.ceil((s1 - s0) / 60.0)))
            for kk in range(npc):
                a_, b_ = s0 + (s1 - s0) * kk / npc, s0 + (s1 - s0) * (kk + 1) / npc
                piece = MB('OW_Map_LanCanSong_%d_%d' % (i, len(seg_pieces)))
                balustrade_seg(piece, p + t * a_, p + t * b_, nrm)
                seg_pieces.append(piece)
    k.finish(C)
    for piece in seg_pieces: piece.finish(C)
    # Lối ven sông lát gạch giữa đường và kè.
    pv = MB('OW_Map_LoiVenSong')
    pv.boxab((228.0, -560, 0.0), (234.5, 208, 0.15), M['paving_big'])            # R2 bờ tây nhánh dọc
    pv.boxab((400.0, -560, 0.0), (417.5, 300, 0.15), M['paving_big'])            # E1 bờ đông
    pv.boxab((-560, 400.0, 0.0), (300, 417.5, 0.15), M['paving_big'])            # N1 bờ bắc
    pv.boxab((300, 400, 0.0), (417.5, 417.5, 0.15), M['paving_big'])
    pv.boxab((222, 228.0, 0.0), (234.5, 228.5, 0.15), M['paving_big'])
    pv.finish(C)

def quad_wall(mb, c0, c1, nrm, wdt, z0, z1, m):
    """Hộp dọc cạnh c0→c1, lệch về phía bờ (−nrm) một nửa bề dày."""
    d = c1 - c0; L = d.length; mid = (c0 + c1) / 2 - nrm * (wdt / 2 - 0.0)
    ang = math.atan2(d.y, d.x)
    mb.box((mid.x, mid.y, (z0 + z1) / 2), (L + 0.05, wdt, z1 - z0), m, rz=ang)

def balustrade_seg(mb, c0, c1, nrm, step=3.6):
    """Lan can trụ đá + song sắt (ảnh 1) theo cạnh bất kỳ, đặt lùi 0,35 m vào bờ."""
    d = c1 - c0; L = d.length; t = d.normalized(); ang = math.atan2(d.y, d.x)
    off = nrm * 0.35          # lùi vào phía bờ
    n = max(1, int(L / step)); st = L / n
    for i in range(n + 1):
        p = c0 + t * (i * st) + off
        mb.box((p.x, p.y, 0.25), (0.56, 0.56, 0.2), M['granite'], rz=ang)
        mb.box((p.x, p.y, 0.75), (0.44, 0.44, 0.8), M['granite'], rz=ang)
        mb.box((p.x, p.y, 1.21), (0.56, 0.56, 0.12), M['granite'], rz=ang)
        if i < n:
            a = c0 + t * (i * st + 0.22) + off; b = c0 + t * ((i + 1) * st - 0.22) + off
            m_ = (a + b) / 2; l = (b - a).length
            mb.box((m_.x, m_.y, 1.05), (l, 0.06, 0.06), M['iron'], rz=ang)
            mb.box((m_.x, m_.y, 0.35), (l, 0.05, 0.05), M['iron'], rz=ang)
            k = int(l / 0.13)
            for j in range(1, k):
                q = a + (b - a) * (j / k)
                mb.box((q.x, q.y, 0.7), (0.02, 0.02, 0.7), M['iron'])

# ---------------------------------------------------------------- 4. hai cầu vòm
def framed(C, name, origin, yaw):
    """Collection tạm: dựng đồ theo hệ cục bộ rồi gắn vào empty mang phép biến đổi."""
    c = bpy.data.collections.new(name); C.children.link(c)
    e = bpy.data.objects.new(name + '_Goc', None); c.objects.link(e)
    e.location = (origin[0], origin[1], 0); e.rotation_euler = (0, 0, yaw)
    return c, e

def attach(c, e):
    for o in c.objects:
        if o is not e and o.parent is None: o.parent = e

def build_bridges_w(C):
    g = globals()
    # B1: hệ ảnh 1 (map X = y + 125, map Z = 215 − x) = xoay −90° + dời (125; 215).
    OBST.clear(); OBST.append((1e6, 1e6, 0, 0, 'tree'))
    c, e = framed(C, 'OW_Cau_B1_AnhDuong', (125.0, 215.0), -math.pi / 2)
    build_bridge(c); attach(c, e)
    # B2: Hai Bà Trưng qua nhánh ngang, cùng kiểu nhỏ hơn. Hệ cục bộ: map X = x − 150, map Z = y.
    saved = {k: g[k] for k in ('BR0', 'BR1', 'SPAN0', 'SPAN1', 'ARCH_H', 'RIV0', 'RIV1')}
    g.update(BR0=223.5, BR1=405.0, SPAN0=262.0, SPAN1=336.0, ARCH_H=26.0, RIV0=228.5, RIV1=400.0)
    OBST.clear(); OBST.append((1e6, 1e6, 0, 0, 'tree'))
    c, e = framed(C, 'OW_Cau_B2_PhiaBac', (-150.0, 0.0), 0.0)
    build_bridge(c); attach(c, e)
    g.update(saved)

# ---------------------------------------------------------------- 5. thư viện mẫu
WIDTHS = [4.5, 5.0, 5.5, 6.0, 7.0, 8.0]
SIGN_SET = [('Phở Bò', '#7E2A1E'), ('Cơm Tấm', '#8A2B2B'), ('Bánh Mì', '#E0A321'), ('Cà Phê', '#4E3628'), ('Trà Sữa', '#3E6E9A'),
            ('Tạp Hóa', '#2B7A3B'), ('Nhà Thuốc', '#1F7A4A'), ('Sửa Xe', '#2B4A7A'), ('Hủ Tiếu', '#9A3A22'), ('Tiệm Vàng', '#C9952A'),
            ('Điện Thoại', '#1F5FA8'), ('Bún Bò Huế', '#7E2A1E'), ('Thời Trang', '#5B3A7A'), ('Bách Hóa', '#2B8A3E')]
LIB = {}

def lib_coll(name):
    lib = _lib()
    c = bpy.data.collections.get(name)
    if c is None: c = bpy.data.collections.new(name); lib.children.link(c)
    return c

def build_library(C):
    seed = 9000
    for w in WIDTHS:
        for v in range(4):
            for slot in (False, True):
                name = 'OW_Mau_NhaPho_%s_%d%s' % (str(w).replace('.', '_'), v, '_B' if slot else '')
                c = lib_coll(name)
                for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
                shophouse(c, -w / 2, w, seed, x_face=0.0, sign_slot=slot); seed += 1
                LIB[(w, v, slot)] = c
    for w in WIDTHS:
        for i, (t, col) in enumerate(SIGN_SET):
            name = 'OW_Mau_Bang_%s_%d' % (str(w).replace('.', '_'), i)
            c = lib_coll(name)
            for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
            sign_board(c, 0.0, -w / 2 + 0.15, w - 0.3, t, None, col, 'white' if col not in ('#E0A321', '#C9952A') else 'dark', 'arialbd.ttf')
            LIB[('sign', w, i)] = c
    # Cột đèn + cờ (tay đèn hướng −x cục bộ).
    c = lib_coll('OW_Mau_CotDen_Co')
    for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
    OBST.clear(); OBST.append((1e6, 1e6, 0, 0, 'tree'))
    mb = MB('OW_Mau_CotDen_Co'); street_lamp(mb, 0, 0, 1); banner(mb, 0, 0, 1); mb.finish(c); LIB['lamp_flag'] = c
    c = lib_coll('OW_Mau_CotDen')
    for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
    OBST.clear(); OBST.append((1e6, 1e6, 0, 0, 'tree'))
    mb = MB('OW_Mau_CotDen'); street_lamp(mb, 0, 0, 1); mb.finish(c); LIB['lamp'] = c

def inst(C, coll, wx, wz, yaw, name, z=0.0, s=1.0):
    e = bpy.data.objects.new(name, None); e.instance_type = 'COLLECTION'; e.instance_collection = coll
    e.location = (wx, wz, z); e.rotation_euler = (0, 0, yaw); e.scale = (s, s, s); C.objects.link(e)
    return e

# ---------------------------------------------------------------- 6. nhà mặt tiền theo tuyến
def featured_list():
    """Nhà có bảng nổi bật đúng chỗ 3 ảnh: (road, side, along_đầu, w, sign_tuple)."""
    F = []
    for s in SIGNS_WEST:                                     # ảnh 3: R2 phía tây, along = y_ref3 + 80
        F.append(('R2', -1, s[0] + 80.0, s[1], s))
    for s in SIGNS_RIGHT:                                    # ảnh 2: R1 phía nam, along = y_ref2 + 140
        F.append(('R1', 1, s[0] + 140.0, s[1], s))
    for s in SIGNS_R1:                                       # ảnh 1: R3 phía nam, along = y_ref1 + 685
        F.append(('R3', 1, s[0] + 685.0, s[1], s))
    return F

MARKET_ZONE = (-452.0, -146.0, -226.0, -40.0)              # không dựng nhà mặt tiền quanh khối chợ (x0, z0, x1, z1)

def in_zone(wx, wz, z):
    return z[0] <= wx <= z[2] and z[1] <= wz <= z[3]

def place_building(C, r, s, along_c, w, variant, slot):
    lx = s * (r.cw / 2 + r.walk)
    wx, wz = r.W(lx, along_c)
    yaw = r.yaw + (math.pi if s > 0 else 0.0)
    inst(C, LIB[(w, variant, slot)], wx, wz, yaw, 'OW_Nha_%s' % r.id)
    return wx, wz, yaw

def build_buildings_w(C):
    if not LIB: build_library(C)
    F = featured_list()
    for r in ROADS:
        for s in (-1, 1):
            if (r.id, s) in RIVER_SIDES or (r.id, s) in EDGE_SIDES: continue
            res = [(f[2], f[3], f[4]) for f in F if f[0] == r.id and f[1] == s]
            for a, b in side_runs(r, s, pad=1.5):
                y = a + 0.2
                while y < b - 4.4:
                    hit = next((t for t in res if t[0] - 0.01 <= y < t[0] + t[1]), None)
                    if hit: y = hit[0] + hit[1]; continue
                    nxt = min([t[0] for t in res if t[0] > y] + [b - 0.2])
                    w = WR.choice(WIDTHS)
                    while w > nxt - y and w > WIDTHS[0]: w = WIDTHS[WIDTHS.index(w) - 1]
                    if w > nxt - y: y = nxt if nxt < b - 0.2 else b; continue
                    lx = s * (r.cw / 2 + r.walk + 8)
                    wx, wz = r.W(lx, y + w / 2)
                    if in_zone(wx, wz, MARKET_ZONE): y += w; continue
                    signed = WR.random() < 0.5
                    v = WR.randrange(4)
                    bx, bz, yaw = place_building(C, r, s, y + w / 2, w, v, signed)
                    if signed:
                        i = WR.randrange(len(SIGN_SET))
                        inst(C, LIB[('sign', w, i)], bx, bz, yaw, 'OW_Bang_%s' % r.id)
                    y += w

# ---------------------------------------------------------------- 7. trưng bày: nhà bảng nổi bật, chợ, quầy, quán, xe bánh mì, tháp
def build_showcase_w(C):
    F = featured_list(); seed = 4000
    for rid, s, a, w, sign in F:
        r = RD[rid]
        lx = s * (r.cw / 2 + r.walk); wx, wz = r.W(lx, a + w / 2)
        yaw = r.yaw + (math.pi if s > 0 else 0.0)
        c, e = framed(C, 'OW_NhaNoiBat_%s_%d' % (rid, seed), (wx, wz), yaw)
        s2 = (-w / 2,) + tuple(sign[1:])
        shophouse(c, -w / 2, w, seed, sign=s2, x_face=0.0); attach(c, e); seed += 1
    # Hệ ảnh 2 (map X = y − 420, map Z = −x − 150): xoay −90° + dời (−420; −150).
    c, e = framed(C, 'OW_ChoBenThanh_The', (-420.0, -150.0), -math.pi / 2)
    mats2(); build_ben_thanh(c)
    t0, t1 = BT['YC'] - BT['TW'] / 2 - 1.2, BT['YC'] + BT['TW'] / 2 + 1.2
    order = ['hoa_cuc', 'trai_cay', 'hoa_bo', 'luu_niem', 'hoa_cuc', 'do_kho', 'hoa_bo', 'trai_cay', 'luu_niem', 'do_kho']
    spots = []; k = 0; y = 16.0
    while y < 176.0:
        if not (t0 - 1.8 < y < t1 + 1.8): spots.append((y, 3.4, order[k % len(order)])); k += 1
        y += 4.0
    build_stalls(c, -18.6, spots)
    cf = MB('OW_QuanViaHe_LeLoi'); CM = cafe_mats(); rr = random.Random(21)
    for yy, dish, chair, stools in ((59.8, 'com_tam', 'plastic_red', False), (64.4, 'com_tam', 'plastic_red', False),
                                    (70.5, 'com_tam', 'plastic_blue', True), (77.0, 'pho', 'plastic_red', False), (83.2, 'pho', 'plastic_green', True)):
        street_cafe_set(cf, CM, 10.9, yy, rr, dish, chair, stools)
    umbrella_pro(cf, CM, 10.9, 62.1, 1.45, 2.55, 'fab_red', 'fab_cream', 'fab_red')
    umbrella_pro(cf, CM, 10.9, 80.1, 1.45, 2.55, 'fab_green', 'fab_cream', 'fab_green')
    cf.finish(c); attach(c, e)
    # Hệ ảnh 3 (map X = x + 215, map Z = y − 480): nhà thuốc An Khang, quán vỉa hè, xe bánh mì Cô Lan.
    c, e = framed(C, 'OW_TonDucThang_The', (215.0, -480.0), 0.0)
    build_east(c)
    cf = MB('OW_QuanViaHe_TDT'); rr = random.Random(33)
    for yy, dish, chair, stools in ((7.2, 'pho', 'plastic_red', False), (11.8, 'pho', 'plastic_blue', True), (27.0, 'com_tam', 'plastic_red', False)):
        street_cafe_set(cf, CM, -10.9, yy, rr, dish, chair, stools)
    umbrella_pro(cf, CM, -10.9, 9.5, 1.45, 2.55, 'fab_red', 'fab_cream', 'fab_red')
    umbrella_pro(cf, CM, -10.9, 29.4, 1.45, 2.55, 'fab_green', 'fab_cream', 'fab_green')
    cx, cy = 11.5, 52.0
    cf.boxab((cx - 0.7, cy - 1.2, 0.55), (cx + 0.7, cy + 1.2, 1.25), M['cart_yellow'])
    cf.boxab((cx - 0.6, cy - 1.1, 1.25), (cx + 0.6, cy + 1.1, 1.85), M['window'])
    cf.boxab((cx - 0.7, cy - 1.2, 1.85), (cx + 0.7, cy + 1.2, 1.95), M['cart_yellow'])
    for dy in (-0.9, 0.9):
        cf.cyl((cx - 0.75, cy + dy, 0.35), 0.2, 0.08, M['iron'], seg=12, axis='X'); cf.cyl((cx + 0.67, cy + dy, 0.35), 0.2, 0.08, M['iron'], seg=12, axis='X')
    umbrella_pro(cf, CM, cx + 1.1, cy, 1.5, 2.65, 'fab_red', 'fab_red', 'fab_cream')
    cf.box((cx - 0.72, cy, 1.0), (0.03, 2.2, 0.45), M['red'])
    cf.finish(c)
    text(c, 'Bánh Mì Cô Lan', (cx - 0.75, cy, 1.0), 0.26, M['sign_white'], (math.pi / 2, 0, -math.pi / 2))
    attach(c, e)
    # Hệ ảnh 1 (map X = y + 125, map Z = 215 − x): quán vỉa hè trước Hải Sản / Cà Phê Cù Lao.
    c, e = framed(C, 'OW_BachDang_The', (125.0, 215.0), -math.pi / 2)
    cf = MB('OW_QuanViaHe_BD'); rr = random.Random(41)
    for yy, dish in ((53.4, 'com_tam'), (57.0, 'pho'), (62.4, 'pho'), (66.0, 'com_tam'), (70.6, 'pho')):
        street_cafe_set(cf, CM, 10.9, yy, rr, dish, 'wood_chair')
    umbrella_pro(cf, CM, 10.9, 74.2, 1.45, 2.55, 'fab_red', 'fab_red', 'fab_cream')
    cf.finish(c); attach(c, e)
    # Tháp Tài Chính (160; −60) + tàu du lịch.
    lotus_tower(C, (160.0, -60.0, 0))
    c, e = framed(C, 'OW_TauDuLich_The', (125.0, 215.0), -math.pi / 2)
    bt = MB('OW_TauDuLich'); bx, by = -58.0, 88.0
    bt.boxab((bx - 5, by - 18, WATER_Z - 0.6), (bx + 5, by + 18, WATER_Z + 1.6), M['red'])
    bt.boxab((bx - 4.8, by - 17.5, WATER_Z + 1.6), (bx + 4.8, by + 17.5, WATER_Z + 3.6), M['frame_white'])
    for kk in range(12):
        for sx in (4.82, -4.82): bt.box((bx + sx, by - 15 + kk * 2.6, WATER_Z + 2.6), (0.04, 1.6, 1.1), M['window'])
    bt.boxab((bx - 4.2, by - 12, WATER_Z + 3.6), (bx + 4.2, by + 10, WATER_Z + 5.4), M['frame_white'])
    bt.boxab((bx - 4.4, by - 12.4, WATER_Z + 5.4), (bx + 4.4, by + 10.4, WATER_Z + 5.6), M['red'])
    bt.finish(c); attach(c, e)
    for bx_, bz_, rz in ((320.0, -380.0, 1.6), (300.0, -120.0, -1.5), (340.0, 90.0, 1.4), (-60.0, 330.0, 0.1), (-380.0, 300.0, 3.1), (100.0, 350.0, 0.2)):
        place(C, boat_src(), (bx_, bz_, WATER_Z + 0.1), rz, 1.0, 'OW_TauDanhCa')

# ---------------------------------------------------------------- 8. lõi khối nhà (trung tâm: nhà cao 4–9 tầng; khu tài chính: cao ốc; bờ đông: căn hộ; bờ bắc: thấp + công viên)
def build_blocks_w(C):
    hi = MB('OW_Map_LoiKhoi')
    grids = [mat_windows_grid('OW_TowerGrid_%d' % i, w, g) for i, (w, g) in enumerate([('#D8D4CA', '#41566A'), ('#E4E1DA', '#4C6377'), ('#C9CDD0', '#3A4C5C'), ('#DCD3C2', '#4A5A66')])]
    facades = [M['facade_' + k] for k in ('gray', 'offwhite', 'olive', 'cream', 'yellowish')]
    def box(cx, cz, w, d, h, m, roof=True):
        hi.boxab((cx - w / 2, cz - d / 2, 0), (cx + w / 2, cz + d / 2, h), m)
        if roof: hi.boxab((cx - w / 2 - 0.3, cz - d / 2 - 0.3, h), (cx + w / 2 + 0.3, cz + d / 2 + 0.3, h + 1.0), M['concrete'])
    def fill(x0, z0, x1, z1, kind):
        if x1 - x0 < 12 or z1 - z0 < 12: return
        x = x0
        while x < x1 - 10:
            w = WR.uniform(14, 26) if kind != 'tower' else WR.uniform(22, 32)
            w = min(w, x1 - x)
            z = z0
            while z < z1 - 10:
                d = min(WR.uniform(14, 26) if kind != 'tower' else WR.uniform(20, 30), z1 - z)
                cx, cz = x + w / 2, z + d / 2
                if (cx - 160) ** 2 + (cz + 60) ** 2 < 45 ** 2: z += d + 6; continue          # Tháp Tài Chính
                if kind == 'mid': box(cx, cz, w - 2, d - 2, WR.uniform(12, 30), WR.choice(facades))
                elif kind == 'tower': box(cx, cz, w - 3, d - 3, WR.uniform(50, 160), WR.choice(grids))
                elif kind == 'apt':
                    if WR.random() < 0.55: box(cx, cz, w - 6, d - 6, WR.uniform(55, 120), WR.choice(grids))
                elif kind == 'apt_low':   # bờ bắc: căn hộ thấp vừa như ảnh 1
                    if WR.random() < 0.6: box(cx, cz, w - 6, d - 6, WR.uniform(22, 48), WR.choice(grids))
                else: box(cx, cz, w - 2, d - 2, WR.uniform(8, 16), WR.choice(facades))
                z += d + (6 if kind not in ('apt', 'apt_low') else 14)
            x += w + (6 if kind not in ('apt', 'apt_low') else 14)
    X = [-560, -450, -228, -150, 0, 110, 215]; Z = [-560, -430, -300, -150, 0, 100, 215]
    def inset(v, sgn):   # lùi khỏi tim đường: nửa đường + vỉa hè + nhà phố 16 m + 2 m
        return v + sgn * 29
    for i in range(len(X) - 1):
        for j in range(len(Z) - 1):
            x0, x1, z0, z1 = inset(X[i], 1), inset(X[i + 1], -1), inset(Z[j], 1), inset(Z[j + 1], -1)
            cx, cz = (X[i] + X[i + 1]) / 2, (Z[j] + Z[j + 1]) / 2
            if -452 < cx < -226 and -150 < cz < 0: continue                   # khối chợ Bến Thành
            kind = 'tower' if (0 <= cx <= 215 and -430 <= cz <= 100) else 'mid'
            fill(x0, z0, x1, z1, kind)
    for (x0, z0, x1, z1) in ((425, -560, 680, -150), (425, -150, 680, 425)):  # bờ đông: căn hộ trong công viên
        fill(x0 + 30, z0 + 29, x1 - 22, z1 - 29, 'apt')
    for (x0, z0, x1, z1) in ((-560, 425, -150, 680), (-150, 425, 680, 680)):  # bờ bắc
        fill(x0 + 22, z0 + 22, x1 - 22, z1 - 22, 'low' if x0 < -200 else 'apt_low')
    hi.finish(C)
    # Cây công viên bờ đông + bờ bắc.
    for k in range(260):
        if k % 2: x, z = WR.uniform(445, 665), WR.uniform(-540, 400)
        else: x, z = WR.uniform(-130, 660), WR.uniform(445, 665)
        place_tree(C, 'big', WR.choice([1, 2, 3]), (x, z, 0.0), WR.uniform(0, 6.28), WR.uniform(0.85, 1.2))
    # Vành cao ốc ngoài mép bản đồ (đường chân trời, không vào được).
    ring = MB('OW_Map_ChanTroi')
    for k in range(70):
        a = WR.uniform(0, 2 * math.pi); rr_ = WR.uniform(900, 1300)
        cx, cz = 70 + math.cos(a) * rr_, 50 + math.sin(a) * rr_
        w = WR.uniform(25, 45)
        ring.boxab((cx - w / 2, cz - w / 2, 0), (cx + w / 2, cz + w / 2, WR.uniform(40, 150)), WR.choice(grids))
    ring.finish(C)

# ---------------------------------------------------------------- 9. đồ đường phố mỗi tuyến: cây, cột đèn (+cờ đường chính), hàng rào đường chính
def build_street_w(C):
    for r in ROADS:
        OBST.clear(); OBST.append((1e6, 1e6, 0, 0, 'tree'))
        mb = MB('OW_Duong_%s_DoViaHe' % r.id); fences = {}
        hw = r.cw / 2
        for s in (-1, 1):
            edge_side = (r.id, s) in EDGE_SIDES
            for a, b in side_runs(r, s, pad=2.0):
                y = a + WR.uniform(3, 8)
                while y < b - 3:
                    sc = WR.uniform(0.9, 1.15) if r.main else WR.uniform(0.75, 0.95)
                    hwb = TREE_BASE_HALF * sc * 1.55
                    if r.walk >= 2 * hwb + 0.6 or r.main:
                        lx = s * (hw + 0.35 + hwb)
                        OBST.append((lx, y, hwb, hwb, 'tree'))
                        wx, wz = r.W(lx, y)
                        place(C, tree_src(WR.choice([1, 2, 0])), (wx, wz, 0.15), r.yaw + round(WR.uniform(0, 6.28) / (math.pi / 2)) * (math.pi / 2), sc * 1.55, 'OW_Cay_PV_StreetTree')
                    y += WR.uniform(12, 17) if not edge_side else WR.uniform(9, 12)
            for a, b in side_runs(r, s, pad=2.0):
                y = a + 4
                while y < b - 2:
                    yl = free_y(s * (hw + 0.7), y, 0.25); OBST.append((s * (hw + 0.7), yl, 0.25, 0.25, 'lamp'))
                    wx, wz = r.W(s * (hw + 0.7), yl)
                    inst(C, LIB['lamp_flag' if r.main else 'lamp'], wx, wz, r.yaw + (0.0 if s > 0 else math.pi), 'OW_CotDen_%s' % r.id)
                    y += 30 if r.main else 36
            if r.main and (r.id, s) not in RIVER_SIDES:
                for a, b in side_runs(r, s, pad=2.0):
                    y = a + 1
                    while y < b - 3.5:
                        # 21.250: hàng rào chia đoạn 80 m (mỗi đoạn một lưới) để Unity ẩn đoạn xa.
                        key = (s, int(y // 80))
                        if key not in fences: fences[key] = MB('OW_Duong_%s_HangRao_%d_%d' % (r.id, 0 if s < 0 else 1, key[1]))
                        short_fence(fences[key], s * (hw + 0.55), y, 3.0); y += 3.6
        road_obj(mb, C, r)
        for f in fences.values(): road_obj(f, C, r)

# ---------------------------------------------------------------- 10. camera kiểm tra
def build_cameras_w(C):
    cams = {
        'OW_Cam_Map_Ref3': ((216.4, -492.0, 6.6), (215.0, -340.0, -18.0), 22, (1672, 941)),
        'OW_Cam_Map_Ref2': ((-375.0, -151.4, 6.4), (-223.0, -150.0, 11.5), 18, (1672, 460)),
        'OW_Cam_Map_Ref1': ((155.0, 213.6, 6.2), (321.0, 215.0, 10.0), 18, (1672, 475)),
        'OW_Cam_Map_TrenCao': ((70.0, -700.0, 520.0), (40.0, 0.0, 0.0), 30, (1600, 1000)),
    }
    for name, (loc, tgt, lens, res) in cams.items():
        cam = bpy.data.objects.get(name)
        if cam is None:
            cam = bpy.data.objects.new(name, bpy.data.cameras.new(name)); C.objects.link(cam)
        cam.data.lens = lens; cam.data.clip_end = 5000; cam.location = loc
        cam.rotation_euler = (Vector(tgt) - Vector(loc)).to_track_quat('-Z', 'Y').to_euler()
        cam['res'] = list(res)
    bpy.context.scene.camera = bpy.data.objects['OW_Cam_Map_TrenCao']

# ---------------------------------------------------------------- chạy
def runw():
    if 'scene' in PARTSW:
        clear_coll(ROOT)
        setup_scene()
    mats(); mats2(); mats1()
    root = coll(ROOT); _lib()
    steps = [('ground', build_ground_w), ('roads', build_roads_w), ('river', build_river_w), ('bridges', build_bridges_w),
             ('library', build_library), ('buildings', build_buildings_w), ('showcase', build_showcase_w), ('blocks', build_blocks_w),
             ('street', build_street_w), ('cameras', build_cameras_w)]
    for key, fn in steps:
        if key in PARTSW:
            if key == 'library': fn(None); print('built library', len(LIB)); continue
            c = coll(ROOT + '_' + key, root)
            for ch in list(c.children): clear_coll(ch.name)
            for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
            fn(c)
            print('built', key, len(c.all_objects))

if not globals().get('NO_RUNW'):
    runw()
