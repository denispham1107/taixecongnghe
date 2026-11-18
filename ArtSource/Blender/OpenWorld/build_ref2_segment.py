# Đợt 3 (21.240): Đại lộ Lê Lợi + chợ Bến Thành theo ảnh tham chiếu 2 (ref-2-cho-ben-thanh.webp).
# Nạp build_ref3_segment.py làm thư viện (NO_RUN) rồi dựng đoạn mới trong collection riêng.
# Hệ cục bộ đoạn này: đường chạy dọc +y (= hướng ĐÔNG trên bản đồ), bên phải (+x) = phía NAM.
#   map X = y − 420 ; map Z = −x − 150     (camera ảnh 2 ở bản đồ (−420; −151,75))
# Chợ ở bên trái (bắc), dãy nhà phố bên phải (nam), Tháp Tài Chính phía trước hơi lệch trái.
import bpy, math, random, os
from mathutils import Vector

OW_DIR = globals().get('OW_DIR') or os.path.dirname(os.path.abspath(__file__))
NO_RUN = True
_base = os.path.join(OW_DIR, 'build_ref3_segment.py')
exec(compile(open(_base, encoding='utf-8').read(), _base, 'exec'), globals())
NO_RUN = False

PARTS2 = globals().get('PARTS2') or ['scene', 'ground', 'market', 'left', 'right', 'trees', 'props', 'skyline', 'traffic', 'camera']
ROOT = 'OW_Ref2_LeLoi'
Y0, Y1 = -40.0, 660.0
# Đường ngang (tâm y, nửa bề rộng gồm vỉa hè): L2 Trần Hưng Đạo (21.241 dời tới map x −228 để chợ liền một khối), R4 Hai Bà Trưng, L3 Quang Trung, L4 Lý Nam Đế, R2 Tôn Đức Thắng.
CROSS_W = [(192.0, 7.5), (270.0, 13.0), (420.0, 7.5), (530.0, 7.5), (635.0, 13.0)]
RIVER_Y = 654.5            # mép kè nhánh dọc (map x 234,5)
MARKET = dict(x0=-84.0, x1=-22.0, y0=50.0, y1=178.0)     # 21.241: map x −370…−242, z −128…−66; mặt dãy vòm dọc Lê Lợi ngay đầu đoạn như ảnh 2
CAM_Y = 45.0             # camera ảnh 2: bản đồ (−375; −151,75)

def to_map(x, y): return (y - 420.0, -x - 150.0)

# ---------------------------------------------------------------- vật liệu thêm cho chợ
def mats2():
    M['market_wall'] = mat_weathered('OW_MarketWall', '#E3CF96', '#A99A70')
    M['market_trim'] = mat('OW_MarketTrim', '#F1E7CC', 0.6)
    M['market_door'] = mat('OW_MarketDoorGreen', '#2F4A3A', 0.55)
    M['market_dark'] = mat('OW_MarketArchDark', '#2A2420', 0.8)
    M['roof_tile'] = mat_roof_tiles('OW_RoofTileRed')
    M['clock_face'] = mat('OW_ClockFace', '#F6F2E6', 0.4, emit='#FFF6DD', emit_strength=0.2)
    M['clock_hand'] = mat('OW_ClockHand', '#151515', 0.4)
    M['sign_red'] = mat('OW_MarketLetterRed', '#C8221C', 0.4, emit='#C8221C', emit_strength=0.45)   # đỏ tươi nổi bật cả trong bóng
    M['flower_y'] = mat_noise('OW_FlowerYellow', '#F2B90F', '#E58A0A', scale=18, rough=0.7, bump=0.4)
    M['flower_w'] = mat_noise('OW_FlowerWhite', '#F3EEDF', '#E2D5B8', scale=18, rough=0.7, bump=0.4)
    M['wood'] = mat_noise('OW_StallWood', '#7A5634', '#5C3E22', scale=6, rough=0.8, bump=0.2)
    M['umb_blue'] = mat('OW_UmbrellaBlue', '#245BA8', 0.7)
    M['chair_red'] = mat('OW_PlasticChairRed', '#C9302C', 0.5)

# ---------------------------------------------------------------- hàm phụ: mái, vòm
def gable(mb, xa, xb, ya, yb, ze, zr, m, ridge='y', over=0.6):
    """Mái hai mái dốc. ridge='y': nóc chạy dọc y; ridge='x': nóc chạy dọc x (đầu hồi quay ±y)."""
    xa -= over; xb += over; ya -= over; yb += over
    if ridge == 'y':
        xm = (xa + xb) / 2
        mb.quad([(xa, ya, ze), (xa, yb, ze), (xm, yb, zr), (xm, ya, zr)], m)
        mb.quad([(xb, yb, ze), (xb, ya, ze), (xm, ya, zr), (xm, yb, zr)], m)
        mb.quad([(xa, ya, ze), (xm, ya, zr), (xb, ya, ze)], m)
        mb.quad([(xb, yb, ze), (xm, yb, zr), (xa, yb, ze)], m)
    else:
        ym = (ya + yb) / 2
        mb.quad([(xb, ya, ze), (xa, ya, ze), (xa, ym, zr), (xb, ym, zr)], m)
        mb.quad([(xa, yb, ze), (xb, yb, ze), (xb, ym, zr), (xa, ym, zr)], m)
        mb.quad([(xa, ya, ze), (xa, yb, ze), (xa, ym, zr)], m)
        mb.quad([(xb, yb, ze), (xb, ya, ze), (xb, ym, zr)], m)

def arch_x(mb, xf, yc, w, h, z0, m, out=1.0):
    """Ô vòm tối trên mặt tường vuông góc trục x (mặt quay về +x khi out=1)."""
    r = w / 2
    mb.box((xf + out * 0.03, yc, z0 + (h - r) / 2), (0.06, w, h - r), m)
    mb.cyl((xf + out * 0.0, yc, z0 + h - r), r, 0.06, m, seg=20, axis='X')

def arch_y(mb, yf, xc, w, h, z0, m):
    """Ô vòm trên mặt tường vuông góc trục y, quay về −y."""
    r = w / 2
    mb.box((xc, yf - 0.03, z0 + (h - r) / 2), (w, 0.06, h - r), m)
    mb.cyl((xc, yf - 0.06, z0 + h - r), r, 0.06, m, seg=20, axis='Y')

# ---------------------------------------------------------------- 1. mặt đường hai bên có đường ngang
def build_ground2(C):
    mb = MB('OW_LL_MatDuong')
    mb.boxab((-ROAD_HW, Y0, -0.3), (ROAD_HW, RIVER_Y - 8, 0.0), M['asphalt'])
    for cy, hw in CROSS_W:
        cw = hw - (WALK if hw > 10 else 4.0)
        mb.boxab((-120, cy - cw, -0.3), (120, cy + cw, 0.0), M['asphalt'])
    for i in range(30):
        mb.box((rnd.uniform(-6, 6), rnd.uniform(Y0, Y1 - 40), 0.004), (rnd.uniform(0.8, 2.5), rnd.uniform(1.0, 4.0), 0.006), M['asphalt_patch'], rz=rnd.uniform(-0.2, 0.2))
    mb.finish(C)
    lm = MB('OW_LL_VachSon')
    for a, b in west_runs(Y0, RIVER_Y - 22, 2.0):
        for x in (-0.13, 0.13): lm.box((x, (a + b) / 2, 0.006), (0.12, b - a, 0.01), M['yellow'])
        lm.box((-6.75, (a + b) / 2, 0.006), (0.15, b - a, 0.01), M['white'])
        lm.box((6.75, (a + b) / 2, 0.006), (0.15, b - a, 0.01), M['white'])
    for x in (-3.5, 3.5):
        y = Y0
        while y < RIVER_Y - 22:
            if not in_cross(y, 3.0): lm.box((x, y + 1.5, 0.006), (0.13, 3.0, 0.01), M['white'])
            y += 9.0
    for cy, hw in CROSS_W:
        for k in range(12):
            lm.box((-6.0 + k * 1.1, cy - hw + 1.6, 0.006), (0.55, 2.6, 0.01), M['white'])
    lm.finish(C)
    sw = MB('OW_LL_ViaHe')
    for a, b in west_runs(Y0, RIVER_Y - 8):
        for s in (-1, 1):
            sw.boxab((s * (ROAD_HW + 0.25), a, 0.0), (s * (ROAD_HW + WALK), b, 0.15), M['paving'])
            sw.boxab((s * ROAD_HW, a, -0.05), (s * (ROAD_HW + 0.25), b, 0.17), M['curb'])
    # Quảng trường lát gạch lớn trước chợ và chợ hoa đầu đoạn (bên trái, x −25…−13).
    for a, b in west_runs(Y0, RIVER_Y - 8):
        sw.boxab((-140, a, 0.0), (-ROAD_HW - WALK, b, 0.15), M['paving_big'])
    # Lối ven sông cuối tầm nhìn.
    sw.boxab((-140, RIVER_Y - 8, 0.0), (140, RIVER_Y, 0.15), M['paving_big'])
    sw.finish(C)
    g = MB('OW_LL_NenPhiaSau')
    g.boxab((ROAD_HW + WALK, Y0 - 40, -0.4), (160, RIVER_Y, 0.0), M['concrete'])
    g.finish(C)
    # Sông nhánh dọc phía trước + bờ đông.
    w = MB('OW_LL_Song')
    w.boxab((-700, RIVER_Y, WATER_Z - 3), (700, RIVER_Y + 165.5, WATER_Z), M['water'])
    w.boxab((-700, RIVER_Y - 0.6, WATER_Z - 1), (700, RIVER_Y, 0.15), M['bank'])
    w.boxab((-700, RIVER_Y + 165.5, WATER_Z - 1), (700, RIVER_Y + 700, 0.3), M['grass'])
    w.finish(C)

# ---------------------------------------------------------------- 2. chợ Bến Thành (21.241: dựng lại chi tiết trong ow_benthanh.py)
_bt = os.path.join(OW_DIR, 'ow_benthanh.py')
exec(compile(open(_bt, encoding='utf-8').read(), _bt, 'exec'), globals())

def build_market(C):
    build_ben_thanh(C)

# ---------------------------------------------------------------- 3. bên trái: chợ hoa đầu đoạn + sạp trước chợ
def flower_stall(mb, x, y, w, r, kinds):
    mb.boxab((x - 0.9, y - w / 2, 0.15), (x + 0.9, y + w / 2, 0.85), M['wood'])
    for row in range(3):
        for k in range(int(w / 0.45)):
            fx = x - 0.6 + row * 0.6; fy = y - w / 2 + 0.25 + k * 0.45
            m = M[r.choice(kinds)]
            mb.cyl((fx, fy, 0.85 + row * 0.12), 0.13, 0.18, M['pot_terra'], seg=8, r2=0.16)
            mb.cyl((fx, fy, 1.03 + row * 0.12), 0.24, 0.22, m, seg=10, r2=0.12)

_cafe = os.path.join(OW_DIR, 'ow_streetcafe.py')
exec(compile(open(_cafe, encoding='utf-8').read(), _cafe, 'exec'), globals())
_stalls = os.path.join(OW_DIR, 'ow_stalls.py')
exec(compile(open(_stalls, encoding='utf-8').read(), _stalls, 'exec'), globals())

def build_left(C):
    # 21.243: quầy hàng có mái bạt, bảng tên, hàng hóa rõ hình (hoa cúc, hoa bó, trái cây, đồ khô, áo dài–nón lá)
    # xếp dọc dải trước dãy vòm chợ (x ≈ −19,3…−17) và vài quầy trước đầu chợ, chừa khối cổng chính.
    t0, t1 = BT['YC'] - BT['TW'] / 2 - 1.2, BT['YC'] + BT['TW'] / 2 + 1.2
    order = ['hoa_cuc', 'trai_cay', 'hoa_bo', 'luu_niem', 'hoa_cuc', 'do_kho', 'hoa_bo', 'trai_cay', 'luu_niem', 'do_kho']
    spots = []; k = 0; y = 16.0
    while y < 176.0:
        if not (t0 - 1.8 < y < t1 + 1.8) and not in_cross(y, 2.5):
            spots.append((y, 3.4, order[k % len(order)])); k += 1
        y += 4.0
    build_stalls(C, -18.6, spots)
    for y in (16.0, 30.0, 46.0, 72.0, 98.0, 132.0, 160.0):
        place(C, planter_src(), (-ROAD_HW - WALK + 0.9, y, 0.15), 0.0, 1.0, 'OW_ChauCayLon')
    # Dãy nhà phố bên trái sau chợ và sau đường L2.
    for a, b in west_runs(MARKET['y1'] + 22, RIVER_Y - 22):
        y = a + 0.2; seed = 500
        while y < b - 4.5:
            w = rnd.choice([5.0, 6.0, 7.0, 8.0]); w = min(w, b - 0.2 - y)
            if w < 4: break
            shophouse(C, y, w, seed, sign=(y, w, rnd.choice(['Cà Phê Sữa Đá', 'Bún Thịt Nướng', 'Sửa Xe', 'Tạp Hóa', 'Nhà Thuốc']), None, rnd.choice(['#2B7A3B', '#7E2A1E', '#2154A6', '#C9952A']), 'white', 'arialbd.ttf') if rnd.random() < 0.5 else None)
            seed += 1; y += w

# ---------------------------------------------------------------- 4. bên phải: dãy nhà phố (xoay 180° bộ dựng nhà bên trái)
SIGNS_RIGHT = [
    (58.0, 9.0, 'Cơm Tấm Sài Gòn', None, '#7A1F2B', 'white', 'timesbd.ttf', True),
    (67.0, 7.0, 'Bánh Mì Huế', None, '#E2B21C', 'dark', 'timesbd.ttf', True),
    (74.0, 6.0, 'Phở Hà Nội', None, '#2B5E3A', 'white', 'arialbd.ttf', True),
    (80.0, 6.0, 'Trà Sữa', None, '#3E6E9A', 'white', 'arialbd.ttf'),
]

def shophouse_right(C, y0, w, seed, sign=None):
    before = set(C.objects)
    s2 = None
    if sign: s2 = (-w / 2,) + tuple(sign[1:])
    shophouse(C, -w / 2, w, seed, sign=s2)
    new = [o for o in C.objects if o not in before]
    e = bpy.data.objects.new('OW_NhaPhai_%03d' % seed, None); C.objects.link(e)
    e.location = (0, y0 + w / 2, 0); e.rotation_euler = (0, 0, math.pi)
    for o in new: o.parent = e
    return e

def build_right(C):
    for s in SIGNS_RIGHT:
        shophouse_right(C, s[0], s[1], int(s[0] * 10) + 3, s)
    seed = 600
    names = [('Hủ Tiếu', '#9A3A22'), ('Cà Phê', '#4E3628'), ('Tiệm Vàng', '#C9952A'), ('Điện Thoại', '#1F5FA8'), ('Bánh Cuốn', '#8A2B2B'), ('Thời Trang', '#5B3A7A')]
    k = 0
    taken = [(s[0], s[0] + s[1]) for s in SIGNS_RIGHT]
    for a, b in west_runs(Y0 + 30, RIVER_Y - 22):
        y = a + 0.2
        while y < b - 4.5:
            hit = next((t for t in taken if t[0] - 4.0 < y < t[1]), None)
            if hit: y = hit[1]; continue
            w = rnd.choice([4.5, 5.0, 5.5, 6.0, 7.0]); w = min(w, b - 0.2 - y)
            nxt = min([t[0] for t in taken if t[0] >= y] + [1e9]); w = min(w, nxt - y)
            if w < 4: y = nxt if nxt < 1e9 else b; continue
            sg = None
            if rnd.random() < 0.55:
                t = names[k % len(names)]; k += 1
                sg = (y, w, t[0], None, t[1], 'white', 'arialbd.ttf')
            shophouse_right(C, y, w, seed, sg); seed += 1; y += w

# ---------------------------------------------------------------- 5. cây, đèn, cờ, rào, bàn ghế
def build_trees2(C):
    for side in (-1, 1):
        y = Y0 + 8
        while y < RIVER_Y - 25:
            sight = side < 0 and CAM_Y + 6 < y < BT['YC'] + 2      # chừa tầm nhìn tới tháp đồng hồ chợ (ảnh 2)
            if not in_cross(y, 3) and not sight:
                seed = rnd.choice([1, 2, 3]); rz = rnd.uniform(0, 6.28); sc = rnd.uniform(0.9, 1.2)
                curb_tree(C, side, y, seed, sc, rz)
            y += rnd.uniform(12, 16)

def build_props2(C):
    mb = MB('OW_LL_DoViaHe')
    r = random.Random(9)
    for side in (-1, 1):
        y = Y0 + (14 if side > 0 else 29)
        while y < RIVER_Y - 25:
            if not in_cross(y, 2):
                yl = street_lamp(mb, side * (ROAD_HW + 0.7), y, side)
                banner(mb, side * (ROAD_HW + 0.7), yl, side)       # cờ đỏ sao vàng
            y += 30
    # Rào sắt thấp liền mạch mép vỉa hè bên phải (ảnh 2) và đoạn trước chợ bên trái.
    for a, b in west_runs(Y0, RIVER_Y - 25, 1.0):
        y = a + 1
        while y < b - 3.5:
            short_fence(mb, ROAD_HW + 0.55, y, 3.0)
            if MARKET['y0'] - 40 < y < MARKET['y0']: short_fence(mb, -ROAD_HW - 0.55, y, 3.0)
            y += 3.6
    mb.finish(C)
    # 21.244: bàn ghế quán vỉa hè chi tiết (ow_streetcafe.py) trước Cơm Tấm / Bánh Mì / Phở / Trà Sữa; chỉ 2 dù.
    cf = MB('OW_LL_QuanViaHe'); CM = cafe_mats(); rr = random.Random(21)
    x = ROAD_HW + WALK - 2.1
    for yy, dish, chair, stools in ((59.8, 'com_tam', 'plastic_red', False), (64.4, 'com_tam', 'plastic_red', False),
                                    (70.5, 'com_tam', 'plastic_blue', True), (77.0, 'pho', 'plastic_red', False),
                                    (83.2, 'pho', 'plastic_green', True)):
        street_cafe_set(cf, CM, x, yy, rr, dish, chair, stools)
    umbrella_pro(cf, CM, x, 62.1, 1.45, 2.55, 'fab_red', 'fab_cream', 'fab_red')
    umbrella_pro(cf, CM, x, 80.1, 1.45, 2.55, 'fab_green', 'fab_cream', 'fab_green')
    # Lề trái phía sau chợ (sau đường L2): vài bộ bàn ghế + 2 dù cùng mẫu mới (yêu cầu bổ sung 17/11/2026).
    xl = -(ROAD_HW + WALK - 2.1)
    for yy, dish, chair, stools in ((MARKET['y1'] + 26.0, 'pho', 'plastic_blue', False), (MARKET['y1'] + 30.6, 'com_tam', 'plastic_red', True),
                                    (MARKET['y1'] + 44.0, 'pho', 'plastic_red', False), (MARKET['y1'] + 48.6, 'com_tam', 'plastic_green', True)):
        if not in_cross(yy, 2.5): street_cafe_set(cf, CM, xl, yy, rr, dish, chair, stools)
    umbrella_pro(cf, CM, xl, MARKET['y1'] + 28.3, 1.45, 2.55, 'fab_blue', 'fab_cream', 'fab_blue')
    umbrella_pro(cf, CM, xl, MARKET['y1'] + 46.3, 1.45, 2.55, 'fab_red', 'fab_cream', 'fab_red')
    cf.finish(C)
    for y in (50.0, 64.0, 77.0, 90.0, 108.0):
        place(C, planter_src(), (ROAD_HW + WALK - 0.8, y, 0.15), 0.0, 1.0, 'OW_ChauCayLon')

# ---------------------------------------------------------------- 6. đường chân trời: khu tài chính + Tháp Tài Chính
def build_skyline2(C):
    hi = MB('OW_LL_CaoOc')
    grids = [mat_windows_grid('OW_TowerGrid_%d' % i, w, g) for i, (w, g) in enumerate([('#D8D4CA', '#41566A'), ('#E4E1DA', '#4C6377'), ('#C9CDD0', '#3A4C5C'), ('#DCD3C2', '#4A5A66')])]
    def tower(cx, cy, w, d, h):
        hi.boxab((cx - w / 2, cy - d / 2, 0), (cx + w / 2, cy + d / 2, h), rnd.choice(grids))
        hi.boxab((cx - w / 2 - 0.3, cy - d / 2 - 0.3, h), (cx + w / 2 + 0.3, cy + d / 2 + 0.3, h + 1.2), M['concrete'])
    # Khu tài chính: bản đồ x 0…215, z −450…100 → cục bộ y 420…635, x −250…300 (chừa lòng đường).
    for i in range(26):
        x = rnd.choice([rnd.uniform(-160, -40), rnd.uniform(40, 220)]); y = rnd.uniform(440, 625)
        if (x + 90) ** 2 + (y - 580) ** 2 < 45 ** 2 or (-150 < x < -30 and y < 600): continue   # chừa chỗ và tầm nhìn tới Tháp Tài Chính
        tower(x, y, rnd.uniform(22, 32), rnd.uniform(20, 28), rnd.uniform(50, 150))
    # Bờ đông xa.
    for i in range(30):
        tower(rnd.uniform(-450, 450), rnd.uniform(RIVER_Y + 200, RIVER_Y + 650), rnd.uniform(22, 34), rnd.uniform(18, 26), rnd.uniform(60, 120))
    # Nhà 4–8 tầng phía sau dãy phố hai bên.
    for side in (-1, 1):
        for a, b in west_runs(Y0, 420):
            y = a
            while y < b - 6:
                w = rnd.uniform(8, 16); h = rnd.uniform(12, 26)
                xa = side * (ROAD_HW + WALK + 18); xb = side * (ROAD_HW + WALK + 40)
                if side < 0 and y < MARKET['y1'] + 5: y += w; continue   # bên trái đầu đoạn là quảng trường + chợ, không có khối che
                hi.boxab((min(xa, xb), y, 0), (max(xa, xb), min(b, y + w), h), M['facade_' + rnd.choice(['gray', 'offwhite', 'olive'])])
                y += w + rnd.uniform(0, 3)
    hi.finish(C)
    # Tháp Tài Chính: bản đồ (160; −60) → cục bộ (−90; 580).
    lotus_tower(C, (-90, 580, 0))

# ---------------------------------------------------------------- 7. xe
def build_traffic2(C):
    car = veh_src('BlueSupercar', 'PlayerCar/PV_BlueSupercar.fbx')
    place(C, car, (1.8, CAM_Y + 30.0, fit_ground(car)), 0.0, 0.85, 'OW_Xe_NguoiChoi')
    for key, rel, x, y, rz in (('Sedan', 'Cars/PV_Sedan.fbx', -1.9, 36.0, math.pi), ('Taxi', 'Cars/PV_Taxi.fbx', 5.2, 40.0, 0.0),
                               ('Hatchback', 'Cars/PV_Hatchback.fbx', 1.8, 80.0, 0.0), ('Sedan', 'Cars/PV_Sedan.fbx', -5.2, 150.0, math.pi),
                               ('DeliveryVan', 'Cars/PV_DeliveryVan.fbx', 5.2, 210.0, 0.0)):
        src = veh_src(key, rel)
        place(C, src, (x, y + CAM_Y, fit_ground(src)), rz, 1.0, 'OW_Xe_' + key)
    for key, rel, x, y, rz in (('Underbone', 'Bikes/PV_Underbone.fbx', 11.6, 17.0, -1.9), ('CityScooter', 'Bikes/PV_CityScooter.fbx', 11.4, 28.0, -1.8),
                               ('Underbone', 'Bikes/PV_Underbone.fbx', 12.0, 52.0, -2.0), ('CityScooter', 'Bikes/PV_CityScooter.fbx', -11.6, 120.5 + 20, 1.2)):
        src = veh_src(key, rel)
        place(C, src, (x, y, 0.15 + fit_ground(src)), rz, 1.0, 'OW_XeMayDau')

def build_camera2(C):
    cam = bpy.data.objects.get('OW_Cam_Ref2')
    if cam is None:
        cd = bpy.data.cameras.new('OW_Cam_Ref2'); cam = bpy.data.objects.new('OW_Cam_Ref2', cd); C.objects.link(cam)
    cam.data.lens = 18; cam.data.sensor_width = 36; cam.data.clip_end = 3000
    cam.location = (1.4, CAM_Y, 6.4)
    # Ngẩng ~2° để thấy trọn tháp đồng hồ.
    cam.rotation_euler = (Vector((0.0, CAM_Y + 152.0, 11.5)) - cam.location).to_track_quat('-Z', 'Y').to_euler()

# ---------------------------------------------------------------- chạy
def run2():
    OBST.clear()
    if 'scene' in PARTS2:
        clear_coll(ROOT)
        setup_scene()
        # Nắng riêng cho góc ảnh 2: thấp phía trước bên phải, rọi mặt dãy vòm chợ (+x) và mặt tháp quay về camera (−y).
        sun = bpy.data.objects['OW_Sun']
        sun.rotation_euler = Vector((-0.45, -0.80, -0.40)).normalized().to_track_quat('-Z', 'Y').to_euler()
    mats(); mats2()
    root = coll(ROOT); _lib()
    steps = [('ground', build_ground2), ('market', build_market), ('left', build_left), ('right', build_right), ('trees', build_trees2),
             ('props', build_props2), ('skyline', build_skyline2), ('traffic', build_traffic2), ('camera', build_camera2)]
    for key, fn in steps:
        if key in PARTS2:
            c = coll(ROOT + '_' + key, root)
            for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
            fn(c)
            print('built', key, len(c.objects))
    # Ẩn đoạn ảnh 3 khi xem đoạn này (hai đoạn dùng hệ cục bộ riêng, chồng lên nhau quanh gốc).
    for name in ('OW_Ref3_TonDucThang',):
        lc = bpy.context.view_layer.layer_collection.children.get(name)
        if lc: lc.exclude = True
    lc = bpy.context.view_layer.layer_collection.children.get(ROOT)
    if lc: lc.exclude = False
    bpy.context.scene.camera = bpy.data.objects['OW_Cam_Ref2']
    bpy.context.scene.render.resolution_x, bpy.context.scene.render.resolution_y = 1672, 460

if not globals().get('NO_RUN2'):   # build_ref1_segment.py nạp file này làm thư viện
    run2()
