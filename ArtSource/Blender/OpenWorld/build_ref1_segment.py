# Đợt 4 (21.247): Đường Bạch Đằng ven sông + Cầu vòm Ánh Dương theo ảnh tham chiếu 1 (ref-1-cau-vom.webp).
# Nạp build_ref2_segment.py (kéo theo build_ref3_segment.py, ow_benthanh, ow_stalls, ow_streetcafe) làm thư viện.
# Hệ cục bộ: đường dọc +y = hướng ĐÔNG trên bản đồ, bên trái (−x) = BẮC (sông nhánh ngang), bên phải (+x) = NAM (dãy quán).
#   map X = y + 125 ; map Z = 215 − x
# Phía trước: đường R2 nhập từ bên phải (y 90), rồi sông nhánh dọc (y 109,5…275) có Cầu vòm Ánh Dương, bờ đông Thủ Thiêm.
import bpy, math, random, os
from mathutils import Vector

OW_DIR = globals().get('OW_DIR') or os.path.dirname(os.path.abspath(__file__))
NO_RUN2 = True
_base = os.path.join(OW_DIR, 'build_ref2_segment.py')
exec(compile(open(_base, encoding='utf-8').read(), _base, 'exec'), globals())
NO_RUN2 = False

PARTS1 = globals().get('PARTS1') or ['scene', 'ground', 'river', 'bridge', 'right', 'trees', 'props', 'skyline', 'traffic', 'camera']
ROOT = 'OW_Ref1_BachDang'
Y0, Y1 = -40.0, 700.0
CROSS_W = [(90.0, 13.0)]          # R2 Tôn Đức Thắng nhập từ phía nam (chỉ bên phải)
BANK_L = -13.5                    # mép kè trái (map z 228,5): lối đi ven sông hẹp như ảnh 1
RIV0, RIV1 = 109.5, 275.0         # sông nhánh dọc cắt ngang phía trước (map x 234,5…400)
FAR_N = -185.0                    # bờ bắc nhánh ngang (map z 400)
BR0, BR1 = 104.0, 281.0           # mố cầu
SPAN0, SPAN1 = 118.0, 192.0       # nhịp vòm chính (vòm dốc, thấp như ảnh 1); phần còn lại là nhịp dẫn
ARCH_H = 34.0
ARCH_X = 10.2                     # vòm ở mép ngoài, lề bộ hành nằm trong
CAM_Y = 36.0                      # camera ảnh 1: bản đồ (155; 213,6), cách chân vòm ~88 m

def to_map(x, y): return (y + 125.0, 215.0 - x)

def mats1():
    M['brick_red'] = mat_tiles('OW_PavingBrickRed', '#9A5A48', '#8A4E3E', '#5E3A30', size=0.45)
    M['granite'] = mat_noise('OW_Granite', '#B9B4AA', '#9C978D', scale=9, rough=0.7, bump=0.25)
    M['curb_red'] = mat('OW_CurbRed', '#B4322B', 0.6)
    M['curb_white'] = mat('OW_CurbWhite', '#ECE8DE', 0.6)
    M['steel_white'] = mat('OW_SteelWhite', '#EEF0EE', 0.3, 0.35)
    M['cable'] = mat('OW_BridgeCable', '#D7DADB', 0.3, 0.8)
    M['pier'] = mat_noise('OW_PierConcrete', '#B4B0A6', '#8E8A80', scale=4, rough=0.85, bump=0.2)

# ---------------------------------------------------------------- bó vỉa sơn đỏ–trắng
def striped_curb(mb, x, y0, y1, side, seg=1.0):
    y = y0; k = 0
    while y < y1 - 0.05:
        e = min(y1, y + seg)
        mb.boxab((x if side > 0 else x - 0.25, y, -0.05), (x + 0.25 if side > 0 else x, e, 0.18), M['curb_red'] if k % 2 else M['curb_white'])
        y = e; k += 1

# ---------------------------------------------------------------- lan can trụ đá + song sắt ven sông
def balustrade(mb, x, y0, y1, step=3.6):
    n = max(1, int((y1 - y0) / step)); st = (y1 - y0) / n
    for i in range(n + 1):
        y = y0 + i * st
        mb.boxab((x - 0.28, y - 0.28, 0.15), (x + 0.28, y + 0.28, 0.35), M['granite'])
        mb.boxab((x - 0.22, y - 0.22, 0.35), (x + 0.22, y + 0.22, 1.15), M['granite'])
        mb.boxab((x - 0.28, y - 0.28, 1.15), (x + 0.28, y + 0.28, 1.27), M['granite'])
        mb.boxab((x - 0.18, y - 0.18, 1.27), (x + 0.18, y + 0.18, 1.33), M['granite'])
        if i < n:
            a, b = y + 0.22, y + st - 0.22
            for z, t in ((1.02, 0.06), (0.32, 0.05)):
                mb.boxab((x - 0.03, a, z), (x + 0.03, b, z + t), M['iron'])
            k = int((b - a) / 0.13)
            for j in range(1, k):
                yy = a + j * (b - a) / k
                mb.box((x, yy, 0.68), (0.02, 0.02, 0.7), M['iron'])
            mb.cyl((x, (a + b) / 2, 0.68), 0.16, 0.025, M['iron'], seg=14, axis='X')   # vòng trang trí giữa ô

# ---------------------------------------------------------------- 1. mặt đất
def build_ground1(C):
    mb = MB('OW_BD_MatDuong')
    mb.boxab((-ROAD_HW, Y0, -0.3), (ROAD_HW, BR0, 0.0), M['asphalt'])
    mb.boxab((-ROAD_HW, BR1, -0.3), (ROAD_HW, Y1, 0.0), M['asphalt'])
    cy, hw = CROSS_W[0]
    mb.boxab((ROAD_HW, cy - 7, -0.3), (90, cy + 7, 0.0), M['asphalt'])
    for i in range(30):
        mb.box((rnd.uniform(-6, 6), rnd.uniform(Y0, BR0 - 4), 0.004), (rnd.uniform(0.8, 2.5), rnd.uniform(1.0, 4.0), 0.006), M['asphalt_patch'], rz=rnd.uniform(-0.2, 0.2))
    mb.finish(C)
    lm = MB('OW_BD_VachSon')
    for a, b in ((Y0, Y1),):
        for x in (-0.13, 0.13): lm.box((x, (a + b) / 2, 0.006), (0.12, b - a, 0.01), M['yellow'])
    for x in (-3.5, 3.5):
        y = Y0
        while y < Y1:
            if not (cy - hw - 2 < y < cy + hw + 2 and x > 0): lm.box((x, y + 1.5, 0.006), (0.13, 3.0, 0.01), M['white'])
            y += 9.0
    for x in (-6.75, 6.75):
        lm.box((x, (Y0 + Y1) / 2, 0.006), (0.15, Y1 - Y0, 0.01), M['white'])
    lm.finish(C)
    sw = MB('OW_BD_ViaHe')
    # Trái: lối gạch đỏ ven sông tới kè.
    sw.boxab((BANK_L, Y0, 0.0), (-ROAD_HW - 0.25, RIV0, 0.15), M['brick_red'])
    striped_curb(sw, -ROAD_HW, Y0, BR0, -1)
    # Phải: vỉa hè gạch xám + bó vỉa đỏ–trắng, chừa miệng R2.
    for a, b in ((Y0, cy - hw), (cy + hw, RIV0)):
        sw.boxab((ROAD_HW + 0.25, a, 0.0), (ROAD_HW + WALK, b, 0.15), M['paving'])
        striped_curb(sw, ROAD_HW, a, min(b, BR0), 1)
    sw.boxab((ROAD_HW + WALK, Y0 - 40, -0.4), (160, RIV0, 0.0), M['concrete'])
    # Bờ đông sau cầu.
    for s in (-1, 1):
        sw.boxab((s * (ROAD_HW + 0.25), BR1, 0.0), (s * (ROAD_HW + WALK), Y1, 0.15), M['paving'])
        striped_curb(sw, s * ROAD_HW, BR1, Y1, s)
    sw.boxab((-400, RIV1, -0.4), (400, Y1 + 400, 0.0), M['grass'])
    sw.finish(C)

def build_river1(C):
    w = MB('OW_BD_Song')
    w.boxab((FAR_N, Y0 - 500, WATER_Z - 3), (BANK_L, RIV1, WATER_Z), M['water'])
    w.boxab((BANK_L, RIV0, WATER_Z - 3), (400, RIV1, WATER_Z), M['water'])
    w.boxab((-700, Y0 - 500, WATER_Z - 3), (FAR_N, RIV1, WATER_Z), M['water'])
    w.finish(C)
    k = MB('OW_BD_Ke')
    k.boxab((BANK_L - 0.6, Y0 - 60, WATER_Z - 1), (BANK_L, RIV0, 0.15), M['bank'])
    k.boxab((BANK_L, RIV0 - 0.6, WATER_Z - 1), (400, RIV0, 0.15), M['bank'])
    k.boxab((-400, RIV1, WATER_Z - 1), (400, RIV1 + 0.6, 0.15), M['bank'])
    k.boxab((BANK_L - 0.7, Y0 - 60, 0.15), (BANK_L + 0.1, RIV0 - 0.6, 0.3), M['granite'])
    k.finish(C)
    rl = MB('OW_BD_LanCan')
    balustrade(rl, BANK_L + 0.35, Y0 - 50, RIV0 - 1.0)
    rl.finish(C)

# ---------------------------------------------------------------- 2. Cầu vòm Ánh Dương
def arch_z(y):
    t = (y - (SPAN0 + SPAN1) / 2) / ((SPAN1 - SPAN0) / 2)
    return 1.4 + ARCH_H * (1 - t * t)

def build_bridge(C):
    """Cầu vòm Ánh Dương (ảnh 1): hai vòm thép trắng dốc ở hai mép ngoài lề bộ hành, nhiều dây treo đứng,
    cổng vòm nhỏ bắc ngang mặt đường ở cuối cầu; lề bộ hành nằm giữa mặt đường và vòm."""
    mb = MB('OW_CauVomAnhDuong')
    W = 11.4
    mb.boxab((-W, BR0, -1.3), (W, BR1, -0.05), M['pier'])
    mb.boxab((-ROAD_HW, BR0, -0.05), (ROAD_HW, BR1, 0.0), M['asphalt'])
    for s in (-1, 1):
        lo, hi = sorted((s * (ROAD_HW + 0.25), s * 9.5))
        mb.boxab((lo, BR0, -0.05), (hi, BR1, 0.18), M['paving'])                     # lề bộ hành
        striped_curb(mb, s * ROAD_HW, BR0, BR1, s)
        lo, hi = sorted((s * 9.5, s * 10.9))
        mb.boxab((lo, BR0, -0.05), (hi, BR1, 0.3), M['concrete'])                     # dải đặt vòm
        mb.boxab((lo + 0.1, SPAN0 - 3, 0.3), (hi - 0.1, SPAN1 + 3, 1.5), M['steel_white'])   # dầm buộc vòm
        xr = s * (W - 0.15)
        y = BR0
        while y < BR1:
            mb.box((xr, y, 0.8), (0.08, 0.08, 1.1), M['steel_white']); y += 2.0
        for z in (1.3, 0.9, 0.5):
            mb.boxab((xr - 0.04, BR0, z), (xr + 0.04, BR1, z + 0.06), M['steel_white'])
        lo, hi = sorted((s * (W - 0.05), s * (W + 0.15)))
        mb.boxab((lo, BR0, -1.3), (hi, BR1, 0.3), M['steel_white'])                     # dầm biên sơn trắng
    for s in (-1, 1):
        x = s * ARCH_X
        pts = [Vector((x, y, arch_z(y))) for y in [SPAN0 + (SPAN1 - SPAN0) * i / 64 for i in range(65)]]
        mb.sweep_rect(pts, 0.9, 1.25, M['steel_white'])
        mb.sweep_rect([Vector((x, p.y, p.z + 1.31)) for p in pts], 0.65, 0.07, M['curb_white'])
        y = SPAN0 + 2.0
        while y < SPAN1 - 1.5:
            mb.rod((x, y, 1.5), (x, y, arch_z(y) - 1.2), 0.04, M['cable'], seg=6)
            y += 2.0
        for ys in (SPAN0, SPAN1):
            mb.boxab((x - 1.0, ys - 2.6, 0.3), (x + 1.0, ys + 2.6, 2.6), M['pier'])
    # Cổng vòm nhỏ bắc ngang mặt đường ở cuối cầu (thấy ở xa giữa ảnh 1).
    yp = BR1 - 14.0; R = ARCH_X
    pts = [Vector((R * math.cos(t), yp, 1.5 + 14.0 * math.sin(t) ** 0.85)) for t in [math.pi * i / 48 for i in range(49)]]
    mb.sweep_rect(pts, 0.55, 0.65, M['steel_white'], side=Vector((0, 1, 0)))
    for xx in (-R, R):
        mb.boxab((xx - 0.9, yp - 0.9, 0.3), (xx + 0.9, yp + 0.9, 1.8), M['pier'])
    # Trụ cầu trong sông + mố.
    for ypp in (SPAN0, SPAN1, (SPAN1 + BR1) / 2, (BR0 + SPAN0) / 2):
        mb.boxab((-W + 0.5, ypp - 2.5, WATER_Z - 2), (W - 0.5, ypp + 2.5, -1.3), M['pier'])
        mb.boxab((-W - 0.5, ypp - 3.2, WATER_Z - 2), (W + 0.5, ypp + 3.2, WATER_Z + 0.4), M['pier'])
    for yb, d in ((BR0, -1), (BR1, 1)):
        mb.boxab((-W - 1, min(yb, yb + d * 6), WATER_Z - 2), (W + 1, max(yb, yb + d * 6), 0.0), M['pier'])
    mb.finish(C)
    lp = MB('OW_CauVom_Den')
    for s in (-1, 1):
        y = BR0 + 8
        while y < BR1 - 5:
            if abs(y - SPAN0) > 4 and abs(y - SPAN1) > 4 and abs(y - yp) > 3:
                street_lamp(lp, s * 9.1, y, s)
            y += 22
    lp.finish(C)

# ---------------------------------------------------------------- 3. dãy quán bên phải
SIGNS_R1 = [
    (44.0, 7.0, 'Ốc Đêm', None, '#2B5E8A', 'white', 'arialbd.ttf'),
    (51.0, 9.0, 'Hải Sản Tươi Sống', 'Tươi Sống Mỗi Ngày', '#1D4FA0', 'white', 'arialbd.ttf', True),
    (60.0, 8.5, 'Cà Phê Cù Lao', 'Cà Phê • Sinh Tố', '#1F6B45', 'white', 'arialbd.ttf', True),
    (68.5, 7.5, 'Lẩu Bò Cù Lao', None, '#B42A26', 'white', 'arialbd.ttf', True),
]

def build_right1(C):
    for s in SIGNS_R1:
        shophouse_right(C, s[0], s[1], int(s[0] * 10) + 9, s)
    taken = [(s[0], s[0] + s[1]) for s in SIGNS_R1]
    names = [('Bia Hơi', '#C9952A'), ('Bánh Xèo', '#9A3A22'), ('Nhà Nghỉ', '#3E6E9A'), ('Tạp Hóa', '#2B7A3B'), ('Sinh Tố', '#C8322B')]
    seed = 700; k = 0
    cy, hw = CROSS_W[0]
    for a, b in ((Y0 + 2, cy - hw), (cy + hw, RIV0 - 2)):
        y = a + 0.2
        while y < b - 4.5:
            hit = next((t for t in taken if t[0] - 4.0 < y < t[1]), None)
            if hit: y = hit[1]; continue
            w = rnd.choice([4.5, 5.0, 5.5, 6.0, 7.0]); w = min(w, b - 0.2 - y)
            nxt = min([t[0] for t in taken if t[0] >= y] + [1e9]); w = min(w, nxt - y)
            if w < 4: y = nxt if nxt < 1e9 else b; continue
            sg = None
            if rnd.random() < 0.5:
                t = names[k % len(names)]; k += 1
                sg = (y, w, t[0], None, t[1], 'white', 'arialbd.ttf')
            shophouse_right(C, y, w, seed, sg); seed += 1; y += w
    # Bờ đông sau cầu: dãy nhà thấp hai bên.
    for side in (-1, 1):
        y = BR1 + 30
        while y < Y1 - 10:
            w = rnd.choice([6.0, 7.0, 8.0])
            if side > 0: shophouse_right(C, y, w, seed, None)
            else: shophouse(C, y, w, seed)
            seed += 1; y += w

# ---------------------------------------------------------------- 4. cây, đồ vỉa hè
def build_trees1(C):
    for side in (-1, 1):
        y = Y0 + 8
        while y < BR0 - 6:
            if not (side > 0 and in_cross(y, 3)):
                seed = rnd.choice([1, 2, 3]); rz = rnd.uniform(0, 6.28); sc = rnd.uniform(0.9, 1.2)
                curb_tree(C, side, y, seed, sc, rz)
            y += rnd.uniform(14, 18)
        y = BR1 + 8
        while y < Y1 - 10:
            seed = rnd.choice([1, 2, 3]); rz = rnd.uniform(0, 6.28); sc = rnd.uniform(0.9, 1.2)
            curb_tree(C, side, y, seed, sc, rz)
            y += rnd.uniform(12, 16)

def build_props1(C):
    mb = MB('OW_BD_DoViaHe')
    for side in (-1, 1):
        y = Y0 + 10
        while y < BR0 - 4:
            if not (side > 0 and in_cross(y, 2)):
                yl = street_lamp(mb, side * (ROAD_HW + 0.7), y, side)
                banner(mb, side * (ROAD_HW + 0.7), yl, side)
            y += 28
    mb.finish(C)
    # Chậu cây lớn có sẵn trên lối ven sông và trước quán.
    for y in (6.0, 22.0, 40.0, 58.0, 76.0, 96.0):
        if not any(abs(-11.6 - o[0]) < o[2] + 0.6 and abs(y - o[1]) < o[3] + 0.6 for o in OBST):
            place(C, planter_src(), (-11.6, y, 0.15), 0.0, 1.0, 'OW_ChauCayLon')
    for y in (50.4, 59.6, 68.0, 76.0):
        place(C, planter_src(), (ROAD_HW + WALK - 0.7, y, 0.15), 0.0, 1.0, 'OW_ChauCayLon')
    # Bàn ghế gỗ trước Hải Sản / Cà Phê Cù Lao, 1 dù đỏ phía xa (ảnh 1).
    cf = MB('OW_BD_QuanViaHe'); CM = cafe_mats(); rr = random.Random(41)
    x = ROAD_HW + WALK - 2.1
    for yy, dish in ((53.4, 'com_tam'), (57.0, 'pho'), (62.4, 'pho'), (66.0, 'com_tam'), (70.6, 'pho')):
        street_cafe_set(cf, CM, x, yy, rr, dish, 'wood_chair')
    umbrella_pro(cf, CM, x, 74.2, 1.45, 2.55, 'fab_red', 'fab_red', 'fab_cream')
    cf.finish(C)

# ---------------------------------------------------------------- 5. bờ xa, tàu, đường chân trời
def build_skyline1(C):
    fb = MB('OW_BD_BoBac')
    fb.boxab((-700, Y0 - 500, WATER_Z - 1), (FAR_N, RIV1, 0.3), M['grass'])
    fb.boxab((FAR_N - 0.5, Y0 - 500, WATER_Z - 1), (FAR_N + 0.5, RIV1, 0.6), M['bank'])
    fb.finish(C)
    y = Y0 - 300
    while y < RIV1:
        place_tree(C, rnd.choice(['big', 'small']), rnd.choice([1, 2, 3]), (FAR_N - rnd.uniform(4, 14), y, 0.3), rnd.uniform(0, 6.28), rnd.uniform(1.0, 1.4))
        y += rnd.uniform(7, 12)
    x = FAR_N
    while x < 400:
        place_tree(C, rnd.choice(['big', 'small']), rnd.choice([1, 2, 3]), (x, RIV1 + rnd.uniform(6, 16), 0.0), rnd.uniform(0, 6.28), rnd.uniform(1.0, 1.4))
        x += rnd.uniform(9, 14)
    hi = MB('OW_BD_CaoOc')
    grids = [mat_windows_grid('OW_TowerGrid_%d' % i, w, g) for i, (w, g) in enumerate([('#D8D4CA', '#41566A'), ('#E4E1DA', '#4C6377'), ('#C9CDD0', '#3A4C5C'), ('#DCD3C2', '#4A5A66')])]
    def tower(cx, cy, w, d, h):
        hi.boxab((cx - w / 2, cy - d / 2, 0), (cx + w / 2, cy + d / 2, h), rnd.choice(grids))
        hi.boxab((cx - w / 2 - 0.3, cy - d / 2 - 0.3, h), (cx + w / 2 + 0.3, cy + d / 2 + 0.3, h + 1.2), M['concrete'])
    for i in range(34):     # bờ bắc (trái, bên kia sông)
        tower(rnd.uniform(-700, FAR_N - 80), rnd.uniform(Y0 - 300, RIV1 + 200), rnd.uniform(22, 34), rnd.uniform(18, 26), rnd.uniform(25, 70))   # căn hộ bờ bắc thấp vừa như ảnh 1
    for i in range(30):     # bờ đông (sau cầu)
        x = rnd.uniform(-450, 450)
        if abs(x) < 40: continue
        tower(x, rnd.uniform(RIV1 + 60, RIV1 + 650), rnd.uniform(22, 34), rnd.uniform(18, 26), rnd.uniform(50, 140))
    for i in range(12):     # trung tâm phía nam (phải, sau dãy quán)
        tower(rnd.uniform(60, 260), rnd.uniform(-200, 90), rnd.uniform(24, 34), rnd.uniform(20, 28), rnd.uniform(45, 120))
    hi.finish(C)
    # Tháp Tài Chính: bản đồ (160; −60) → cục bộ (275; 35) — lệch phải ngoài khung ảnh 1 (đã báo người dùng).
    lotus_tower(C, (275.0, 35.0, 0))
    # Tàu du lịch trắng–đỏ trên sông bên trái + tàu đánh cá có sẵn.
    bt = MB('OW_BD_TauDuLich')
    bx, by = -58.0, CAM_Y + 52.0
    bt.boxab((bx - 5, by - 18, WATER_Z - 0.6), (bx + 5, by + 18, WATER_Z + 1.6), M['red'])
    bt.boxab((bx - 4.8, by - 17.5, WATER_Z + 1.6), (bx + 4.8, by + 17.5, WATER_Z + 3.6), M['frame_white'])
    for k in range(12):
        bt.box((bx + 4.82, by - 15 + k * 2.6, WATER_Z + 2.6), (0.04, 1.6, 1.1), M['window'])
        bt.box((bx - 4.82, by - 15 + k * 2.6, WATER_Z + 2.6), (0.04, 1.6, 1.1), M['window'])
    bt.boxab((bx - 4.2, by - 12, WATER_Z + 3.6), (bx + 4.2, by + 10, WATER_Z + 5.4), M['frame_white'])
    for k in range(8):
        bt.box((bx + 4.22, by - 10 + k * 2.6, WATER_Z + 4.5), (0.04, 1.6, 0.9), M['window'])
    bt.boxab((bx - 4.4, by - 12.4, WATER_Z + 5.4), (bx + 4.4, by + 10.4, WATER_Z + 5.6), M['red'])
    bt.finish(C)
    for bxx, byy, rz in ((-120.0, -30.0, 1.5), (-40.0, 180.0, 0.2), (60.0, 220.0, 3.0)):
        place(C, boat_src(), (bxx, byy, WATER_Z + 0.1), rz, 1.0, 'OW_TauDanhCa')

# ---------------------------------------------------------------- 6. xe + camera
def build_traffic1(C):
    car = veh_src('BlueSupercar', 'PlayerCar/PV_BlueSupercar.fbx')
    place(C, car, (1.8, CAM_Y + 18.0, fit_ground(car)), 0.0, 0.85, 'OW_Xe_NguoiChoi')
    for key, rel, x, y in (('Sedan', 'Cars/PV_Sedan.fbx', -1.8, CAM_Y + 36.0), ('Taxi', 'Cars/PV_Taxi.fbx', 5.2, CAM_Y + 38.0),
                           ('Hatchback', 'Cars/PV_Hatchback.fbx', 1.8, CAM_Y + 80.0), ('Sedan', 'Cars/PV_Sedan.fbx', -5.2, 170.0),
                           ('DeliveryVan', 'Cars/PV_DeliveryVan.fbx', 5.2, 205.0), ('Hatchback', 'Cars/PV_Hatchback.fbx', -1.8, 250.0)):
        src = veh_src(key, rel)
        place(C, src, (x, y, fit_ground(src)), 0.0, 1.0, 'OW_Xe_' + key)
    for key, rel, x, y, rz in (('Underbone', 'Bikes/PV_Underbone.fbx', 11.8, 64.2, -1.9), ('CityScooter', 'Bikes/PV_CityScooter.fbx', 11.6, 47.5, -1.8),
                               ('Underbone', 'Bikes/PV_Underbone.fbx', 12.0, 75.6, -2.0)):
        src = veh_src(key, rel)
        place(C, src, (x, y, 0.15 + fit_ground(src)), rz, 1.0, 'OW_XeMayDau')

def build_camera1(C):
    cam = bpy.data.objects.get('OW_Cam_Ref1')
    if cam is None:
        cam = bpy.data.objects.new('OW_Cam_Ref1', bpy.data.cameras.new('OW_Cam_Ref1')); C.objects.link(cam)
    cam.data.lens = 18; cam.data.sensor_width = 36; cam.data.clip_end = 3000
    cam.location = (1.4, CAM_Y - 6.0, 6.2)
    cam.rotation_euler = (Vector((0.0, CAM_Y + 160.0, 10.0)) - cam.location).to_track_quat('-Z', 'Y').to_euler()

def run1():
    OBST.clear()
    if 'scene' in PARTS1:
        clear_coll(ROOT)
        setup_scene()
        sun = bpy.data.objects['OW_Sun']
        # Ảnh 1: nắng chiều thấp phía trước bên trái, rọi mặt dãy quán bên phải và mặt vòm quay về camera.
        sun.rotation_euler = Vector((0.50, -0.75, -0.42)).normalized().to_track_quat('-Z', 'Y').to_euler()
    mats(); mats2(); mats1()
    root = coll(ROOT); _lib()
    steps = [('ground', build_ground1), ('river', build_river1), ('bridge', build_bridge), ('right', build_right1), ('trees', build_trees1),
             ('props', build_props1), ('skyline', build_skyline1), ('traffic', build_traffic1), ('camera', build_camera1)]
    for key, fn in steps:
        if key in PARTS1:
            c = coll(ROOT + '_' + key, root)
            for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
            fn(c)
            print('built', key, len(c.objects))
    bpy.context.scene.camera = bpy.data.objects['OW_Cam_Ref1']
    bpy.context.scene.render.resolution_x, bpy.context.scene.render.resolution_y = 1672, 475

if not globals().get('NO_RUN1'):   # build_world_map.py nạp file này làm thư viện
    run1()
