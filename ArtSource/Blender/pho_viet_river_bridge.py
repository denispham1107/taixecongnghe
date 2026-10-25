"""Cầu vượt sông theo ảnh mẫu ArtSource/Reference/River/river_bridge_reference.png (24/10/2026).
Chạy trong Blender thật qua Blender MCP: exec file này rồi gọi build_river_bridge().
Dùng chung bảng màu/atlas và lớp Geo của pho_viet_saigon_streets.py để cùng phong cách phố.

Tọa độ tác giả = tọa độ cục bộ một khối phố Unity (VietnamStreet, dài 80 m):
x ngang đường (làn ở ±1,55/±4,65), y lên, z dọc đường; giao lộ ở z 63–77 (vạch mũi tên 55,4–84,6).
Khối cầu: mặt sông z 4,8–55,2 = 50,4 m (25/10/2026: dài gấp đôi bản 16,8–42,0 theo yêu cầu người dùng, chạy từ
ngay sau vạch mũi tên của giao lộ trước tới sát vạch mũi tên giao lộ sau), cầu chỉ 2 làn giữa |x| ≤ 3,1.
Hai bờ ngắn còn lại (0–4,8 và 55,2–58,8): vỉa hè đường chính lát lại và một dãy nhà ống ven sông sâu 3 m
(21.48) quay mặt ra đường ngang, lưng ra sông, kéo tới hết sông |x| 95 — thay dãy nhà đường ngang bị ẩn vì
đứng trên mặt sông; không phần nào lấn ra mặt nước. Từ 21.49 là khối căn hộ cao cấp 6–9 tầng: mặt hướng sông có
ban công lõm lan can kính, vách kính trong suốt sàn–trần (submesh PV_StreetGlass) nhìn thấy phòng sáng đèn.
Hai làn ngoài kết thúc ở mép kè; ra ngoài mặt cầu trên mặt sông là rơi xuống sông (luật trong Unity).
Xuất: PV_RiverBridge.fbx (3 LOD, vật liệu: atlas + nước + mặt nạ nền + nhựa đường + vạch trắng/vàng)
và PV_RiverBoat0/1.fbx (thuyền neo, Unity cho nhấp nhô), kèm JSON số đo cho trình cài Unity.
"""
import bpy, bmesh, math, os, json, random
from math import sin, cos, pi
from mathutils import Vector

ROOT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
exec(compile(open(ROOT+'/ArtSource/Blender/pho_viet_saigon_streets.py', encoding='utf8').read(), 'pho_viet_saigon_streets.py', 'exec'))
OUT = ROOT+'/Assets/PhoViet/Art/River'
SOURCE = ROOT+'/ArtSource/Blender/PhoViet_RiverBridge.blend'
SCENE = 'PV_RiverBridge'

W0, W1 = 4.8, 55.2           # mặt nước giữa hai bờ kè, 50,4 m (giữa vạch mũi tên hai giao lộ)
PAVE_END = 58.8              # tấm vỉa hè đầu tiên còn hiện sau bờ xa (58,8–63)
WATER_Y = -5.0               # mặt nước thấp hơn mặt đường 5 m (kè cao, thấy rõ sông nằm sâu dưới đường/cầu)
WALL_BOTTOM = -9.8           # kè xuống tới đáy sông
BED_Y = -9.5                 # lòng sông
MURK = [-8.7, -7.6, -6.6, -5.7]  # các lớp nước đục trong suốt (sâu trước): vật chìm càng sâu càng mờ
GROUND_TOP = -.25            # mặt trên "Nền thành phố" (hộp 600×800, tâm y -0,4, dày 0,3)
SIDE = 95.0                  # sông kéo dài sang hai bên, sương che phần xa
ROAD = 3.1                   # mặt cầu = hai làn giữa (LaneX ±1,55 ± 1,55)
CURB = 3.4                   # gờ bó trắng, sau đó lan can
RAIL = 3.7                   # mép ngoài lan can/mặt cầu
DECK_BOTTOM = -.9
LAMPS = [10.0, 23.4, 36.6, 50.0]  # đèn LED (Unity đặt mẫu PV_LEDStreetLamp) ở x = ±3,55
PIERS = [W0+16.8, W1-16.8]   # hai trụ cầu chia 3 nhịp 16,8 m
LAMP_X = 3.55
BOATS = [  # (asset, x, z, yaw độ) — thuyền neo dọc kè, mũi theo trục x
    ('PV_RiverBoat0', -31.0, W1-3.3, 0.0),
    ('PV_RiverBoat1', 27.0, W0+3.2, 180.0),
    ('PV_RiverBoat1', -58.0, W0+3.0, 0.0),
    ('PV_RiverBoat0', 62.0, W1-3.4, 180.0),
]
EXTRA = ['Water', 'Murk', 'Mask', 'Asphalt', 'LineWhite', 'LineYellow', 'Room']
EXTRA_MAT = {'Water': 'PV_RiverWater', 'Murk': 'PV_RiverMurk', 'Mask': 'PV_RiverGroundMask', 'Asphalt': 'PV_RiverAsphalt',
             'LineWhite': 'PV_RiverLineWhite', 'LineYellow': 'PV_RiverLineYellow', 'Room': 'PV_RiverRoomLight'}

# ---------------------------------------------------------------- nước, kè, lối dạo
def water(g):
    # Lòng sông tối, các lớp nước đục (sâu trước, để vẽ đúng thứ tự trong suốt) rồi lưới mặt nước
    # đủ mịn cho sóng nhấp nhô dịch đỉnh trong shader (LOD0 ô ~1,25 m).
    g.quad([(-SIDE, BED_Y, W0), (SIDE, BED_Y, W0), (SIDE, BED_Y, W1), (-SIDE, BED_Y, W1)], 'navy', (0, 1, 0))
    for y in MURK:
        g.quad([(-SIDE, y, W0), (SIDE, y, W0), (SIDE, y, W1), (-SIDE, y, W1)], 'glass', (0, 1, 0), 'Murk')
    nx, nz = ((152, 40), (50, 12), (10, 4))[g.d]
    for i in range(nx):
        x0 = -SIDE+2*SIDE*i/nx; x1 = -SIDE+2*SIDE*(i+1)/nx
        for j in range(nz):
            z0 = W0+(W1-W0)*j/nz; z1 = W0+(W1-W0)*(j+1)/nz
            g.quad([(x0, WATER_Y, z0), (x1, WATER_Y, z0), (x1, WATER_Y, z1), (x0, WATER_Y, z1)], 'glass', (0, 1, 0), 'Water')
    # Mặt nạ stencil ngay trên mặt nền thành phố: nền không vẽ đè lên lòng sông.
    g.quad([(-SIDE, GROUND_TOP+.012, W0), (SIDE, GROUND_TOP+.012, W0), (SIDE, GROUND_TOP+.012, W1), (-SIDE, GROUND_TOP+.012, W1)], 'glass', (0, 1, 0), 'Mask')

def wall_face(g, z, facing):
    """Kè đá xám xếp khối so le, vệt ẩm sẫm sát mặt nước, gờ đá trên cùng."""
    d = g.d; top = 0.0
    if d == 2:
        g.quad([(-SIDE, WALL_BOTTOM, z), (SIDE, WALL_BOTTOM, z), (SIDE, top, z), (-SIDE, top, z)], 'potGrey', (0, 0, facing))
    else:
        # Phần dưới nước: đá sẫm phủ rêu (chỉ thấy mờ qua nước), phần trên xếp khối so le.
        base = WATER_Y-.35
        g.quad([(-SIDE, WALL_BOTTOM, z), (SIDE, WALL_BOTTOM, z), (SIDE, base-1.4, z), (-SIDE, base-1.4, z)], 'ironGrey', (0, 0, facing))
        g.quad([(-SIDE, base-1.4, z), (SIDE, base-1.4, z), (SIDE, base, z), (-SIDE, base, z)], 'leaf5', (0, 0, facing))
        rows = 8 if d == 0 else 4; h = (top-base)/rows; blk = 1.25 if d == 0 else 3.2
        for r in range(rows):
            y0 = base+r*h; y1 = y0+h-(.035 if d == 0 else 0); x = -SIDE-(blk/2 if r % 2 else 0); k = 0
            while x < SIDE:
                x1 = min(SIDE, x+blk-(.04 if d == 0 else 0)); xs = max(-SIDE, x)
                col = ('potGrey', 'stone', 'steel', 'potGrey', 'trunk')[(k*7+r*3) % 5] if d == 0 else ('potGrey', 'stone')[(k+r) % 2]
                if y1 < WATER_Y+.45: col = 'ironGrey' if (k+r) % 2 else 'trunk'
                g.quad([(xs, y0, z), (x1, y0, z), (x1, y1, z), (xs, y1, z)], col, (0, 0, facing))
                x += blk; k += 1
        # Mạch vữa phía sau khối (một tấm sẫm liền để khe giữa khối không thủng).
        g.quad([(-SIDE, WALL_BOTTOM, z-facing*.02), (SIDE, WALL_BOTTOM, z-facing*.02), (SIDE, top, z-facing*.02), (-SIDE, top, z-facing*.02)], 'ironGrey', (0, 0, facing))
    # Gờ đá đỉnh kè nhô ra mặt nước.
    g.cube((0, -.06, z+facing*.12), (2*SIDE, .16, .34), 'stone')

def hanging_green(g, x, z, facing, seed, length=1.7):
    """Dây leo xanh rủ từ đỉnh kè xuống mặt sông như ảnh mẫu."""
    if g.d == 2: return
    r = random.Random(seed); strands = 5 if g.d == 0 else 3; per = 7 if g.d == 0 else 4
    for s in range(strands):
        sx = x+r.uniform(-1.2, 1.2); ln = length*r.uniform(.6, 1.2)
        for j in range(per):
            yy = -.05-j*ln/per; jitter = r.uniform(-.15, .15)
            for c in range(2):
                a = r.uniform(0, 2*pi); col = ('leaf2', 'leaf3', 'leaf1', 'leaf4')[(j+c+s) % 4]
                g.leaf((sx+jitter+r.uniform(-.14, .14), yy+r.uniform(-.06, .06), z+facing*r.uniform(.03, .2)),
                       (cos(a)*.7, sin(a)*.5-.35, facing*.55), .2, .11, col, roll=a, face=(0, .2, facing))
    # Bụi lá trên đỉnh kè, nơi dây leo bắt đầu.
    for k in range(10 if g.d == 0 else 4):
        a = k*2.1+seed; q = (x+cos(a)*.9, .12+abs(sin(a))*.25, z-facing*(.25+.2*abs(cos(a))))
        g.leaf(q, (cos(a)*.6, .7, sin(a)*.6), .32, .16, ('leaf3', 'leaf2', 'leaf4')[k % 3], roll=a)

def promenade(g, z0, z1, rail_z, facing):
    """Lối dạo lát gạch ven sông (ngoài phạm vi nhà phố) và lan can sắt trên đỉnh kè."""
    d = g.d
    for side in (-1, 1):
        xa, xb = (26.0, SIDE) if side > 0 else (-SIDE, -26.0)
        g.quad([(xa, .02, z0), (xb, .02, z0), (xb, .02, z1), (xa, .02, z1)], 'paveCream', (0, 1, 0))
        if d == 0:
            step = 2.0; x = xa
            while x < xb-.01:
                g.cube((x+1.0, .03, (z0+z1)/2), (.04, .012, abs(z1-z0)), 'grout'); x += step
        # Lan can sắt từ vỉa hè phố (|x| 7,1) ra xa; làn ngoài của đường KHÔNG có lan can để xe rơi được.
        ra, rb = (7.15, SIDE) if side > 0 else (-SIDE, -7.15)
        g.cube(((ra+rb)/2, .06, rail_z), (rb-ra, .12, .28), 'stone')
        if d < 2:
            g.cube(((ra+rb)/2, 1.0, rail_z), (rb-ra, .06, .07), 'iron')
            g.cube(((ra+rb)/2, .55, rail_z), (rb-ra, .04, .04), 'iron')
            n = int((rb-ra)/(1.6 if d == 0 else 4.0))
            for k in range(n+1): g.cube((ra+k*(rb-ra)/n, .55, rail_z), (.07, 1.0, .07), 'iron')
        else:
            g.panel(((ra+rb)/2, .55, rail_z), rb-ra, .9, 'iron', z_facing=facing)
        # Chậu cây lớn dọc lối dạo (dùng đúng chậu/cây mẫu của phố).
        if d < 2:
            for xx in range(32, 95, 16): river_planter(g, side*xx, (z0+z1)/2-facing*.4)

def river_planter(g, x, z, s=1.1):
    """Chậu vuông trắng + cây chậu kiểu cây mẫu (bản nhẹ hơn chậu vỉa hè để khối cầu không quá nặng)."""
    h = .62*s
    g.cube((x, h/2, z), (.5*s, h, .5*s), 'white'); g.cube((x, h+.02, z), (.56*s, .05, .56*s), 'stone')
    g.cube((x, h+.03, z), (.44*s, .02, .44*s), 'soil', skip=('-y',))
    mini_tree(g, (x, h+.03, z), 1.3*s, g.d, int(x*131+z*17), clusters=(5, 3, 0), per=(14, 8, 0))

# ---------------------------------------------------------------- cầu
def baluster_run(g, x, z0, z1, side):
    """Lan can trắng: cột vuông, con tiện, tay vịn có gờ; chậu hoa hồng trên đầu cột."""
    d = g.d; y0 = .15; top = 1.12
    g.cube((x, y0+.06, (z0+z1)/2), (.34, .12, z1-z0), 'trim')               # đế lan can
    g.cube((x, top, (z0+z1)/2), (.36, .1, z1-z0), 'white')                    # tay vịn
    if d < 2: g.cube((x, top+.065, (z0+z1)/2), (.26, .03, z1-z0), 'trim')
    post = 2.1; n = int(round((z1-z0)/post))
    for k in range(n+1):
        z = z0+k*(z1-z0)/n
        g.cube((x, (y0+top)/2+.02, z), (.36, top-y0+.12, .36), 'white')        # cột vuông
        if d == 0: g.cube((x, top+.11, z), (.42, .06, .42), 'trim')
        if k < n:
            if d == 2:
                g.cube((x, (y0+top)/2, z+(z1-z0)/n/2), (.06, .8, (z1-z0)/n), 'white')
                continue
            m = 6 if d == 0 else 3
            for j in range(1, m+1):
                zz = z+j*(z1-z0)/n/(m+1)
                if d == 0:
                    g.cyl((x, y0+.12, zz), (x, .48, zz), .045, 'white', n=6, r2=.075)
                    g.cyl((x, .48, zz), (x, top-.05, zz), .075, 'white', n=6, r2=.04)
                else:
                    g.cube((x, (y0+top)/2, zz), (.09, top-y0-.1, .09), 'white')

def flower_box(g, x, y, z, seed):
    """Hộp hoa trắng trên đầu cột lan can, bụi hoa giấy hồng như ảnh mẫu."""
    d = g.d
    g.cube((x, y+.2, z), (.56, .4, .7), 'white'); g.cube((x, y+.41, z), (.5, .03, .64), 'soil', skip=('-y',))
    if d == 2:
        g.blob((x, y+.62, z), (.36, .26, .42), 'pink', n=5, m=3); return
    r = random.Random(seed)
    for k in range(9 if d == 0 else 4):
        c = (x+r.uniform(-.2, .2), y+.55+r.uniform(0, .25), z+r.uniform(-.28, .28))
        g.blob(c, (.17, .14, .17), ('pink', 'pinkLight', 'leaf3', 'pink')[k % 4], n=6 if d == 0 else 4, m=3)
    if d == 0:
        for k in range(10):
            a = k*.63+seed; g.leaf((x+cos(a)*.25, y+.45, z+sin(a)*.32), (cos(a)*.6, .5, sin(a)*.6), .26, .13, ('leaf2', 'leaf3')[k % 2], roll=a)

def lamp_flag(g, side, z):
    """Cờ đỏ sao vàng treo trên cột đèn cầu (cột do Unity đặt mẫu đèn LED phố)."""
    x = side*(LAMP_X+.12)
    g.cyl((x, 4.4, z), (x+side*.95, 4.95, z), .022, 'iron', n=5)
    w, h = .95, .64; cx = x+side*.55; cy = 4.6
    g.cube((cx, cy, z), (w, h, .02), 'flagRed')
    g.star((cx, cy, z-.012), .19, 'flagGold'); g.star((cx, cy, z+.012), .19, 'flagGold', facing=1)

def deck(g):
    d = g.d
    # Mặt đường hai làn (vật liệu nhựa đường của phố) và vạch kẻ.
    g.quad([(-ROAD, 0, W0), (ROAD, 0, W0), (ROAD, 0, W1), (-ROAD, 0, W1)], 'rubber', (0, 1, 0), 'Asphalt')
    for side in (-1, 1):
        x = side*(ROAD-.2)
        g.quad([(x-.06, .006, W0), (x+.06, .006, W0), (x+.06, .006, W1), (x-.06, .006, W1)], 'white', (0, 1, 0), 'LineWhite')
    for zc in range(6, 55, 4):  # cùng nhịp vạch vàng đứt của phố: tâm 2 + 4k
        g.quad([(-.085, .007, zc-1.125), (.085, .007, zc-1.125), (.085, .007, zc+1.125), (-.085, .007, zc+1.125)], 'yellow', (0, 1, 0), 'LineYellow')
    for side in (-1, 1):
        # Gờ bó trắng, lan can, mặt ngoài dầm cầu.
        g.cube((side*(ROAD+CURB)/2, .075, (W0+W1)/2), (CURB-ROAD, .15, W1-W0), 'trim')
        baluster_run(g, side*(CURB+RAIL)/2, W0+.2, W1-.2, side)
        g.quad([(side*RAIL, DECK_BOTTOM, W0), (side*RAIL, DECK_BOTTOM, W1), (side*RAIL, .15, W1), (side*RAIL, .15, W0)], 'stone', (side, 0, 0))
        g.cube((side*(RAIL+.04), -.12, (W0+W1)/2), (.1, .22, W1-W0), 'trim')
        for z in LAMPS: lamp_flag(g, side, z)
        for k, z in enumerate([W0+2.3+k2*(W1-W0-4.6)/5 for k2 in range(6)]):
            flower_box(g, side*(CURB+RAIL)/2, 1.17, z, k*17+(3 if side > 0 else 0))
    # Mặt dưới dầm và trụ cầu chìm trong nước.
    g.quad([(-RAIL, DECK_BOTTOM, W0), (-RAIL, DECK_BOTTOM, W1), (RAIL, DECK_BOTTOM, W1), (RAIL, DECK_BOTTOM, W0)], 'potGrey', (0, -1, 0))
    for zp in PIERS:
        g.cube((0, (WALL_BOTTOM+DECK_BOTTOM)/2, zp), (RAIL*2-.8, DECK_BOTTOM-WALL_BOTTOM, 1.2), 'stone')
        g.cube((0, DECK_BOTTOM-.15, zp), (RAIL*2-.4, .3, 1.6), 'trim')
        if d == 0:
            for sx in (-1, 1): g.cyl((sx*(RAIL-.4), WALL_BOTTOM, zp), (sx*(RAIL-.4), DECK_BOTTOM-.3, zp), .62, 'stone', n=10)
    # Mố cầu: khối đá dưới hai đầu cầu nối vào kè.
    for zb, f in ((W0, 1), (W1, -1)):
        g.cube((0, (WALL_BOTTOM+DECK_BOTTOM)/2, zb+f*.6), (RAIL*2, DECK_BOTTOM-WALL_BOTTOM, 1.2), 'stone')

def bank_marks(g):
    """Làn ngoài sắp hết đường: vạch chéo trắng thu hẹp, gờ sơn vàng–đen ở mép kè, biển CẦU HẸP."""
    d = g.d
    # Cầu dài 50,4 m chiếm trọn đoạn giữa hai giao lộ nên không còn chỗ kẻ vạch thu hẹp làn (25/10/2026):
    # chỉ giữ gờ cảnh báo vàng–đen ở mép kè của hai làn ngoài và biển CẦU HẸP trên vỉa hè hai bờ.
    for toward in (1, -1):
        for side in (-1, 1):
            # Gờ cảnh báo vàng–đen trên mép kè của làn ngoài (không chắn: xe đi thẳng vẫn rơi).
            zw = W0 if toward > 0 else W1
            stripes = 8 if d < 2 else 2
            for k in range(stripes):
                xa2 = side*(RAIL+k*(7.0-RAIL)/stripes); xb2 = side*(RAIL+(k+1)*(7.0-RAIL)/stripes)
                g.cube(((xa2+xb2)/2, .04, zw-toward*.12), (abs(xb2-xa2), .08, .24), 'yellow' if k % 2 == 0 else 'iron')
    # Biển "CẦU HẸP" (tam giác vàng viền đỏ, ký hiệu đường hẹp hai bên) cho cả hai chiều.
    for x, z, face in ((7.75, W0-1.9, -1), (-7.75, W1+2.0, 1)):
        g.cyl((x, .25, z), (x, 2.55, z), .045, 'steel', n=6)
        h = .95; cy = 2.95; zz = z+face*.06
        tri = [(x-h*.58, cy-h*.42, zz), (x+h*.58, cy-h*.42, zz), (x, cy+h*.58, zz)]
        g.add(tri, [(0, 1, 2)], 'red', ('d', (0, 0, face)))
        s = .78; inner = [(x-h*.58*s, cy-h*.42*s+.02, zz+face*.005), (x+h*.58*s, cy-h*.42*s+.02, zz+face*.005), (x, cy+h*.58*s-.03, zz+face*.005)]
        g.add(inner, [(0, 1, 2)], 'yellow', ('d', (0, 0, face)))
        for sx in (-1, 1):  # hai vạch hẹp dần
            g.quad([(x+sx*.2, cy-.22, zz+face*.01), (x+sx*.14, cy-.22, zz+face*.01), (x+sx*.07, cy+.2, zz+face*.01), (x+sx*.11, cy+.2, zz+face*.01)], 'iron', (0, 0, face))
        g.cube((x, 2.25, z), (1.05, .36, .03), 'white')
        g.text('CẦU HẸP 2 LÀN', (x, 2.25, z+face*.02), .2, 'navy', .95, bold=.02, facing=face)
        if d == 0:  # mặt sau biển màu kim loại
            g.add([(p[0], p[1], p[2]-face*.012) for p in tri], [(0, 2, 1)], 'steel', ('d', (0, 0, -face)))

def bank_land(g, z0, z1, facing):
    """Bờ ngắn giữa giao lộ và kè: vỉa hè caro đường chính (|x| 7,0–11,2) giống PV_StreetPavement (cùng lưới
    gạch 0,6 m, gờ bó vỉa) và nền lát dưới dãy nhà ven sông (|x| 11,2–95)."""
    d = g.d; top = .2
    for side in (-1, 1):
        lo, hi = sorted((side*6.99, side*11.2))
        g.cube(((lo+hi)/2, top/2, (z0+z1)/2), (hi-lo, top, z1-z0), 'grout', skip=('-y',))
        zf = z1 if facing > 0 else z0
        g.quad([(lo, -.06, zf), (hi, -.06, zf), (hi, top+.03, zf), (lo, top+.03, zf)], 'stone', (0, 0, facing))
        k0 = math.ceil((z0-.3)/.6-1e-6); k1 = math.floor((z1-.3)/.6+1e-6)
        for xi in range(7):
            x = side*(9.1+(-1.8+xi*.6)*side)
            for k in range(k0, k1+1):
                z = .3+.6*k; za, zb = max(z0, z-.28), min(z1, z+.28)
                if zb-za < .05 or d == 2: continue
                col = 'paveCream' if (xi+k) % 2 == 0 else 'paveBrown'
                g.quad([(x-.28, top+.015, za), (x+.28, top+.015, za), (x+.28, top+.015, zb), (x-.28, top+.015, zb)], col, (0, 1, 0))
        if d == 2:
            g.quad([(side*7.0, top+.015, z0), (side*11.2, top+.015, z0), (side*11.2, top+.015, z1), (side*7.0, top+.015, z1)], 'paveBrown', (0, 1, 0))
        n = max(1, int(round((z1-z0)/.7)))
        for k in range(n):
            za = z0+k*(z1-z0)/n; zb = za+(z1-z0)/n
            g.cube((side*7.1, .17, (za+zb)/2), (.22, .36, zb-za-.02), 'curb' if k % 2 == 0 else 'stone')
        # Nền lát dưới dãy nhà ven sông + mép đứng nhìn ra sông.
        lo, hi = sorted((side*11.2, side*SIDE))
        g.quad([(lo, top, z0), (hi, top, z0), (hi, top, z1), (lo, top, z1)], 'paveCream', (0, 1, 0))
        g.quad([(lo, -.06, zf), (hi, -.06, zf), (hi, top, zf), (lo, top, zf)], 'stone', (0, 0, facing))
        # Lan can sắt ngắn ở góc đường chính (từ vỉa hè tới nhà đầu dãy), không chắn đường ngang.
        rz = zf-facing*.12
        g.cube((side*9.3, .06, rz), (4.3, .12, .2), 'stone')
        if d < 2:
            g.cube((side*9.3, 1.0, rz), (4.3, .06, .07), 'iron'); g.cube((side*9.3, .55, rz), (4.3, .04, .04), 'iron')
            for k in range(4): g.cube((side*(7.2+k*1.38), .55, rz), (.07, 1.0, .07), 'iron')

# ---------------------------------------------------------------- căn hộ ven sông (21.48–21.49)
HOUSE_DEPTH = 3.0
LOGGIA = 1.0          # ban công lõm: lan can kính ở mặt lưng nhà, vách kính lùi vào 1 m (không nhô ra mặt sông)
ROOM = 1.25           # phòng nhìn thấy sau vách kính
SHELLS = ['white', 'stone', 'cream', 'sky', 'mint', 'sand', 'white', 'stone']
FRAMES = ['darkFrame', 'white', 'steel', 'ironGrey']
INTERIORS = ['interiorWall', 'trim', 'sky', 'woodLight', 'cream', 'beige']
LIT_INTERIORS = ['interiorWall', 'interior', 'cream', 'woodLight', 'peach', 'trim']   # tông ấm cho phòng bật đèn ban đêm (21.51)
HOUSE_BOARDS = [('red', 'white'), ('navy', 'yellow'), ('green', 'white'), ('white', 'red'), ('yellow', 'redDark'), ('blue', 'white'), ('orange', 'white')]
HOUSE_LABELS = ['TẠP HÓA', 'NHÀ THUỐC', 'CƠM BÌNH DÂN', 'SỬA ĐIỆN', 'TIỆM VÀNG', 'BÚN RIÊU', 'IN ẤN', 'NHÀ MAY', 'CÀ PHÊ', 'HỚT TÓC', 'ĐIỆN THOẠI', 'BÁNH CUỐN',
                'NƯỚC MÍA', 'TRÀ SỮA', 'MINI MART', 'KHÓA - CHÌA', 'VĂN PHÒNG PHẨM', 'XÔI', 'SPA', 'ỐC', 'NGÂN HÀNG', 'BẤT ĐỘNG SẢN']

def shop_unit(g, x0, x1, base, out, f, rng):
    """Một gian cửa hàng tầng trệt mặt đường ngang: kính khung trắng có dải đèn hoặc cửa cuốn, hộp cuốn, biển chữ, mái hiên."""
    d = g.d; w = x1-x0; cx = (x0+x1)/2; gw = w-.6
    board, ink = rng.choice(HOUSE_BOARDS); label = rng.choice(HOUSE_LABELS); lit = rng.random() < .65
    awn = rng.random() < .45; awn_cols = rng.choice([('awnRed', 'awnWhite'), ('awnBlue', 'awnWhite'), ('awnGreen', 'awnWhite'), ('awnYellow', 'awnWhite')])
    g.quad([(cx-gw/2, base+.05, out(.01)), (cx+gw/2, base+.05, out(.01)), (cx+gw/2, base+2.75, out(.01)), (cx-gw/2, base+2.75, out(.01))], 'glass' if lit else 'steel', (0, 0, f))
    g.cube((cx, base+2.62, out(.04)), (gw, .26, .08), 'ironGrey')
    if lit:
        g.cube((cx, base+2.2, out(.03)), (gw, .07, .05), 'frame')
        if d == 0:
            n = max(2, int(gw/1.3))
            for j in range(n+1): g.cube((cx-gw/2+j*gw/n, base+1.3, out(.03)), (.07, 2.5, .05), 'frame')
            g.quad([(cx-gw/2+.1, base+2.27, out(.02)), (cx+gw/2-.1, base+2.27, out(.02)), (cx+gw/2-.1, base+2.48, out(.02)), (cx-gw/2+.1, base+2.48, out(.02))], 'lamp', (0, 0, f))
    elif d == 0:
        for k in range(9): g.cube((cx, base+.3+k*.26, out(.02)), (gw, .03, .02), 'ironGrey')
    g.cube((cx, base+3.15, out(.12)), (w-.4, .72, .16), board)
    if d == 0: g.text(label, (cx, base+3.15, out(.21)), .34, ink, w-.9, bold=.02, facing=f)
    if awn and d < 2:
        n = 4 if d == 0 else 2; sw = (w-.4)/n
        for i in range(n):
            xa = x0+.2+i*sw
            g.wedge(xa, xa+sw, base+2.78, out(.02), base+2.4, out(1.15), awn_cols[i % 2], .04, 'Awning')

def apartment_block(g, x0, x1, zf, f, rng, obstacles):
    """Khối căn hộ cao cấp ven sông rộng x0–x1, 6–9 tầng, sâu 3 m (zf = mặt đường ngang, hướng f).
    Mặt đường ngang: dãy cửa hàng tầng trệt + cửa sổ/ban công sắt các tầng trên.
    Mặt hướng sông: mỗi tầng có gờ sàn + lan can kính trong suốt ở mặt lưng nhà, ban công lõm 1 m, vách kính
    trong suốt từ sàn tới trần (submesh PV_StreetGlass) chia ô bằng đố, nhìn thấy phòng sáng đèn (sàn, trần có
    đèn, vách sau, sofa, cây). Ba kiểu: tường kính đố tối, lam đứng, khung trắng. Không phần nào vươn quá 0,12 m
    khỏi lưng nhà nên không lấn mặt sông (kè cách lưng nhà 0,3 m)."""
    d = g.d; w = x1-x0; cx = (x0+x1)/2; zb = zf-f*HOUSE_DEPTH; base = .2
    floors = rng.choice([6, 7, 7, 8, 8, 9]); gh = 3.6; fh = 3.0; h = base+gh+(floors-1)*fh
    shell = rng.choice(SHELLS); frame = rng.choice(FRAMES); style = rng.randrange(3)
    def out(o): return zf+f*o          # ra phía đường ngang
    def back(o): return zb-f*o         # ra phía sông (≤ 0,12)
    def inner(o): return zb+f*o        # vào trong nhà tính từ lưng nhà
    zone = LOGGIA+ROOM
    # Lõi đặc từ mặt đường tới sau phòng; hai vách đầu hồi bao vùng ban công + phòng.
    g.cube((cx, (base+h)/2, (zf+inner(zone))/2), (w-.04, h-base, HOUSE_DEPTH-zone), shell, skip=('-y',))
    for xe in (x0+.12, x1-.12): g.cube((xe, (base+h)/2, inner(zone/2)), (.24, h-base, zone), shell, skip=('-y',))
    obstacles.append(dict(kind='Nhà ven sông', center=[cx, (base+h)/2, (zf+zb)/2], size=[w, h-base, HOUSE_DEPTH]))
    rngs = random.Random(rng.randrange(1 << 30))
    # ---- mặt đường ngang
    for xe in (x0+.12, x1-.12): g.cube((xe, (base+h)/2, out(.06)), (.24, h-base, .12), 'trim')
    units = max(2, int(round(w/5.0))); uw = w/units
    for u in range(units): shop_unit(g, x0+u*uw, x0+(u+1)*uw, base, out, f, rngs)
    nwin = max(2, int(w/2.6)); pitch = w/nwin
    for k in range(1, floors):
        y0 = base+gh+(k-1)*fh
        g.cube((cx, y0+.05, out(.08)), (w, .14, .16), 'trim')
        if d == 2: continue
        for j in range(nwin):
            xw = x0+(j+.5)*pitch
            g.cube((xw, y0+1.45, out(.04)), (1.36, 1.95, .08), 'frame')
            g.quad([(xw-.6, y0+.55, out(.085)), (xw+.6, y0+.55, out(.085)), (xw+.6, y0+2.35, out(.085)), (xw-.6, y0+2.35, out(.085))], 'glass', (0, 0, f))
        if (k+int(abs(cx))) % 2 == 0:
            g.cube((cx, y0+.12, out(.45)), (w-.6, .14, .9), 'trim')
            g.cube((cx, y0+1.02, out(.88)), (w-.6, .05, .05), 'iron'); g.cube((cx, y0+.55, out(.88)), (w-.6, .04, .04), 'iron')
            if d == 0:
                n = int((w-.6)/.5)
                for j in range(n+1): g.cube((x0+.3+j*(w-.6)/n, y0+.6, out(.88)), (.025, .85, .025), 'iron')
        elif d == 0 and rngs.random() < .6:
            g.cube((x0+.7 if rngs.random() < .5 else x1-.7, y0+.9, out(.22)), (.7, .5, .42), 'white')
    # ---- mặt hướng sông: căn hộ kính
    glass_z = inner(LOGGIA); wall_z = inner(zone); mpitch = (1.5 if style != 1 else 1.25)
    nm = max(2, int((w-.5)/mpitch))
    lit_seed = int(abs(cx)*13+abs(zf)*7)
    for k in range(floors):
        lit = (k*7+lit_seed) % 10 < 7          # ~70% phòng sáng đèn ban đêm (vách ấm + máng đèn trần phát sáng)
        room_tag = 'Room' if lit else ''
        room_col = LIT_INTERIORS[(k+lit_seed) % len(LIT_INTERIORS)] if lit else INTERIORS[(k+lit_seed) % len(INTERIORS)]
        y0 = base if k == 0 else base+gh+(k-1)*fh
        yt = base+gh if k == 0 else y0+fh
        # Sàn (là trần tầng dưới) qua vùng ban công + phòng; gờ sàn ở lưng nhà.
        g.cube((cx, y0+.1, inner(zone/2)), (w-.48, .2, zone), shell, skip=('-y',) if k == 0 else ())
        g.cube((cx, y0+.15, back(.05)), (w, .3, .1), frame if style != 2 else 'white')
        if d == 2:
            g.quad([(x0+.25, y0+.3, glass_z), (x1-.25, y0+.3, glass_z), (x1-.25, yt, glass_z), (x0+.25, yt, glass_z)], 'glassLight', (0, 0, -f), 'Glass')
            g.panel((cx, (y0+yt)/2, wall_z), w-.5, yt-y0-.2, room_col, z_facing=-f, tag=room_tag)
            continue
        # Lan can kính trong suốt + tay vịn inox (không ở tầng trệt).
        if k:
            g.quad([(x0+.25, y0+.3, back(.0)), (x1-.25, y0+.3, back(.0)), (x1-.25, y0+1.3, back(.0)), (x0+.25, y0+1.3, back(.0))], 'glassLight', (0, 0, -f), 'Glass')
            g.cube((cx, y0+1.33, back(.0)), (w-.5, .05, .07), 'chrome')
        # Vách kính sàn–trần lùi vào, chia ô bằng đố.
        g.quad([(x0+.25, y0+.2, glass_z), (x1-.25, y0+.2, glass_z), (x1-.25, yt, glass_z), (x0+.25, yt, glass_z)], 'glassLight', (0, 0, -f), 'Glass')
        for m in range(nm+1):
            xm = x0+.25+m*(w-.5)/nm
            if style == 1:   # lam đứng nhô ra tới mép ban công
                g.cube((xm, (y0+yt)/2, inner(LOGGIA/2)), (.1, yt-y0-.2, LOGGIA-.05), frame)
            else:
                g.cube((xm, (y0+.2+yt)/2, glass_z-f*.03), (.07, yt-y0-.2, .06), 'white' if style == 2 else frame)
        if d == 0: g.cube((cx, y0+2.45 if k else y0+2.9, glass_z-f*.03), (w-.5, .05, .06), 'white' if style == 2 else frame)
        # Phòng phía sau kính: trần có đèn, vách sau màu ấm, đồ đạc.
        g.panel((cx, (y0+.2+yt)/2, wall_z-f*.005), w-.5, yt-y0-.2, room_col, z_facing=-f, tag=room_tag)
        for lx in [x0+(j+.5)*(w/max(2, int(w/4))) for j in range(max(2, int(w/4)))]:
            g.quad([(lx-.6, yt-.03, inner(LOGGIA+.3)), (lx+.6, yt-.03, inner(LOGGIA+.3)), (lx+.6, yt-.03, inner(LOGGIA+.55)), (lx-.6, yt-.03, inner(LOGGIA+.55))], 'lamp', (0, -1, 0), room_tag)
        if d == 0:
            sx = cx+rngs.uniform(-w*.25, w*.25)
            g.cube((sx, y0+.45, inner(zone-.35)), (1.8, .5, .6), rngs.choice(['woodLight', 'navy', 'stone', 'awnGreen']))   # sofa
            g.cube((sx, y0+.85, inner(zone-.12)), (1.8, .5, .16), rngs.choice(['woodLight', 'navy', 'stone', 'awnGreen']))
            px = x0+.8 if rngs.random() < .5 else x1-.8
            g.cube((px, y0+.5, inner(LOGGIA+.5)), (.4, .6, .4), 'potGrey'); g.blob((px, y0+1.2, inner(LOGGIA+.5)), (.35, .5, .35), 'leaf3', n=6, m=3)
            if k and rngs.random() < .5:   # chậu cây ngoài ban công
                bx = cx+rngs.uniform(-w*.35, w*.35)
                g.cube((bx, y0+.45, inner(.45)), (.5, .5, .5), 'white'); g.blob((bx, y0+1.0, inner(.45)), (.4, .4, .4), rngs.choice(['leaf2', 'leaf3', 'pink']), n=6, m=3)
    # ---- mái: sân thượng vườn, lan can kính, phòng kỹ thuật
    g.cube((cx, h+.1, (zf+zb)/2), (w, .2, HOUSE_DEPTH), shell)
    g.cube((cx, h+.7, out(-.08)), (w, 1.0, .16), shell)
    if d < 2:
        g.quad([(x0+.1, h+.2, back(.0)), (x1-.1, h+.2, back(.0)), (x1-.1, h+1.25, back(.0)), (x0+.1, h+1.25, back(.0))], 'glassLight', (0, 0, -f), 'Glass')
        g.cube((cx, h+1.28, back(.0)), (w-.2, .06, .08), 'chrome')
        g.cube((cx-w*.25, h+1.2, out(-.9)), (w*.3, 2.0, 1.2), shell, skip=('-y',))
        if d == 0:
            for j in range(int(w/3)):
                bx = x0+1.5+j*3.0
                g.cube((bx, h+.45, inner(.7)), (1.0, .5, .6), 'woodDark'); g.blob((bx, h+.95, inner(.7)), (.5, .4, .35), ('leaf2', 'leaf3', 'leaf4')[j % 3], n=6, m=3)
    else:
        g.cube((cx, h+.7, back(.08)), (w, 1.0, .16), shell)

def riverside_row(g, zf, f, seed, obstacles):
    """Dãy khối căn hộ hai bên đường chính (|x| 11,4 → 95) trên bờ ngắn, rộng 10–17 m có hạt giống
    (cùng hạt cho mọi LOD nên các LOD khớp nhau); khối cuối nhận phần dư để dãy chạy tới đúng hết sông."""
    for side in (-1, 1):
        rng = random.Random(seed+(7 if side > 0 else 0)); x = 11.4
        while x < SIDE-.01:
            w = rng.choice([10.0, 11.5, 12.6, 14.0, 15.4, 16.8])
            if SIDE-x-w < 8.0: w = SIDE-x
            a, b = sorted((side*x, side*(x+w)))
            apartment_block(g, a, b, zf, f, random.Random(rng.randrange(1 << 30)), obstacles)
            x += w

def jetty(g, x, z, facing):
    """Cầu tàu gỗ nhỏ và cọc neo cạnh thuyền."""
    d = g.d; y = WATER_Y+.55
    g.cube((x, y, z+facing*1.6), (5.0, .14, 3.2), 'wood')
    if d == 0:
        for k in range(12): g.cube((x-2.3+k*.42, y+.075, z+facing*1.6), (.04, .01, 3.2), 'woodDark')
    for sx in (-2.3, 0, 2.3):
        for sz in (.2, 3.0):
            g.cyl((x+sx, WALL_BOTTOM, z+facing*sz), (x+sx, y+.6, z+facing*sz), .11, 'woodDark', n=6 if d == 0 else 4)

def bridge_tile(d):
    g = Geo(d)
    water(g)
    wall_face(g, W0, 1); wall_face(g, W1, -1)
    for x in (-SIDE, SIDE):  # đầu sông xa hai bên, chìm trong sương
        g.quad([(x, WALL_BOTTOM, W0), (x, WALL_BOTTOM, W1), (x, GROUND_TOP, W1), (x, GROUND_TOP, W0)], 'potGrey', (-x, 0, 0))
    bank_land(g, 0.0, W0-.02, 1); bank_land(g, W1+.02, PAVE_END, -1)
    obstacles = []
    riverside_row(g, W0-.3-HOUSE_DEPTH, -1, 11, obstacles)
    riverside_row(g, W1+.3+HOUSE_DEPTH, 1, 23, obstacles)
    for k, x in enumerate([-80, -66, -51, -39, -22, -12, 12, 20, 34, 47, 70, 86]):
        hanging_green(g, x, W0, 1, 100+k, 2.2+(k % 3)*.6)
        hanging_green(g, x+5, W1, -1, 300+k, 2.0+(k % 2)*.8)
    deck(g)
    bank_marks(g)
    for name, x, z, yaw in BOATS:
        jetty(g, x, (W1 if z > (W0+W1)/2 else W0), -1 if z > (W0+W1)/2 else 1)
    g.meta = dict(obstacles=obstacles, water=[W0, W1], water_y=WATER_Y, bed_y=BED_Y, murk=MURK, deck_half=ROAD, curb=CURB, rail=RAIL, side=SIDE, ground_top=GROUND_TOP,
                  lamps=LAMPS, lamp_x=LAMP_X, piers=PIERS, boats=[dict(asset=a, x=x, z=z, yaw=y) for a, x, z, y in BOATS],
                  materials=['PV_StreetAtlas']+[EXTRA_MAT[t] for t in EXTRA])
    return g

# ---------------------------------------------------------------- thuyền
def boat(kind, d):
    """Thuyền gỗ chở hàng neo dọc kè: thân gỗ thon hai đầu, mái che xanh, hàng hóa nhiều màu.
    Trục dài theo x (song song bờ), mặt nước ở y = 0 (Unity đặt ở WATER_Y)."""
    g = Geo(d); L = 9.0 if kind == 0 else 6.4; Wd = 1.9 if kind == 0 else 1.5; H = .95
    seg = 10 if d == 0 else 5
    # Thân: các khoang hình thang theo x, hai đầu thu nhọn và cong lên.
    def half_width(t): return Wd/2*(1-abs(t)**2.2)
    def rise(t): return .35*abs(t)**3
    for i in range(seg):
        t0 = -1+2*i/seg; t1 = -1+2*(i+1)/seg; x0 = t0*L/2; x1 = t1*L/2
        w0, w1 = max(.04, half_width(t0)), max(.04, half_width(t1)); r0, r1 = rise(t0), rise(t1)
        for s in (-1, 1):
            g.quad([(x0, -.45+r0, s*w0*.55), (x1, -.45+r1, s*w1*.55), (x1, H*.55+r1, s*w1), (x0, H*.55+r0, s*w0)], 'woodDark' if s < 0 else 'wood', (0, 0, s))
        g.quad([(x0, -.45+r0, -w0*.55), (x1, -.45+r1, -w1*.55), (x1, -.45+r1, w1*.55), (x0, -.45+r0, w0*.55)], 'woodDark', (0, -1, 0))
        g.quad([(x0, H*.5+r0, -w0*.92), (x1, H*.5+r1, -w1*.92), (x1, H*.5+r1, w1*.92), (x0, H*.5+r0, w0*.92)], 'woodLight', (0, 1, 0))
        # Mép mạn sơn xanh/đỏ quanh thuyền
        for s in (-1, 1):
            g.quad([(x0, H*.55+r0, s*w0*1.01), (x1, H*.55+r1, s*w1*1.01), (x1, H*.55+r1-.12, s*w1*.98), (x0, H*.55+r0-.12, s*w0*.98)], 'awnBlue' if kind == 0 else 'red', (0, 0, s))
    # Mái che vòm trên khoang giữa.
    roof_col = 'awnGreen' if kind == 0 else 'awnBlue'; rl = L*(.55 if kind == 0 else .5); x0 = -rl/2+(.6 if kind == 0 else 0)
    arcs = 6 if d < 2 else 3
    for k in range(arcs):
        a0 = pi*k/arcs; a1 = pi*(k+1)/arcs; rr = Wd*.52
        p = lambda a, x: (x, H*.55+.95+sin(a)*.55, cos(a)*rr)
        g.add([p(a0, x0), p(a0, x0+rl), p(a1, x0+rl), p(a1, x0)], [(0, 1, 2, 3)], roof_col, ('c', (x0+rl/2, H*.55+.6, 0)))
        g.add([p(a0, x0), p(a1, x0), p(a1, x0+rl), p(a0, x0+rl)], [(0, 1, 2, 3)], 'woodDark', ('c', (x0+rl/2, H*.55+3, 0)))
    for x in (x0, x0+rl):
        for s in (-1, 1): g.cube((x, H*.55+.5, s*Wd*.5), (.07, 1.0, .07), 'woodDark')
    if kind == 0:
        # Ca bin nhỏ phía đuôi, cửa sổ, cờ đỏ.
        g.cube((L*.3, H*.55+.55, 0), (1.3, 1.1, Wd*.8), 'white')
        for s in (-1, 1): g.panel((L*.3, H*.55+.75, s*Wd*.401), .8, .4, 'glass', z_facing=s)
        g.cube((L*.3, H*.55+1.15, 0), (1.45, .08, Wd*.9), 'awnBlue')
        g.cyl((L*.38, H*.55+1.2, 0), (L*.38, H*.55+2.4, 0), .025, 'iron', n=5)
        g.cube((L*.38+.32, H*.55+2.15, 0), (.6, .4, .02), 'flagRed'); g.star((L*.38+.32, H*.55+2.15, -.012), .12, 'flagGold')
        g.star((L*.38+.32, H*.55+2.15, .012), .12, 'flagGold', facing=1)
    # Hàng hóa: két, bao, rổ trái cây nhiều màu dưới mái và mũi thuyền.
    if d < 2:
        r = random.Random(kind*31+7)
        for k in range(14 if d == 0 else 6):
            x = r.uniform(-L*.36, L*.18); z = r.uniform(-Wd*.3, Wd*.3)
            col = ('red', 'yellow', 'orange', 'green', 'goodsB', 'white', 'awnYellow')[k % 7]
            if k % 3 == 0: g.blob((x, H*.55+.25, z), (.28, .2, .28), ('orange', 'yellow', 'leaf3')[k % 3], n=6, m=3)
            else: g.cube((x, H*.55+.18+(k % 2)*.34, z), (.45, .34, .38), col)
    return g

# ---------------------------------------------------------------- xuất
def river_materials():
    mats = {}
    for tag, name in EXTRA_MAT.items():
        m = bpy.data.materials.get(name) or bpy.data.materials.new(name); m.use_nodes = True
        b = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
        b.inputs['Base Color'].default_value = {'Water': hexcol('2E8FB5'), 'Murk': hexcol('0E4F66'), 'Mask': hexcol('92A68A'), 'Asphalt': hexcol('555D5D'),
                                                'LineWhite': hexcol('FFF1CF'), 'LineYellow': hexcol('FFD35A'), 'Room': hexcol('F7D58A')}[tag]
        b.inputs['Roughness'].default_value = .08 if tag == 'Water' else .85
        mats[tag] = m
    return mats

def river_mesh(name, g, coll, mats):
    ob = to_mesh(name, g, coll); me = ob.data
    # to_mesh (script phố) đã thêm slot kính trong suốt PV_StreetGlass cho mặt tag 'Glass' (21.49): slot thêm nối tiếp sau.
    used = []; first = len(me.materials)
    for t in EXTRA:
        if any(tag == t for tag in g.tags): used.append(t); me.materials.append(mats[t])
    if used:
        # to_mesh đảo một số mặt nhưng giữ thứ tự đa giác: gán slot theo tag.
        for poly, t in zip(me.polygons, g.tags):
            if t in used: poly.material_index = first+used.index(t)
    return ob

def export(name, geos, coll, offset, mats):
    root = bpy.data.objects.new(name, None); coll.objects.link(root); objs = []
    for level, g in enumerate(geos):
        ob = river_mesh(name+'_LOD'+str(level), g, coll, mats); ob.parent = root; objs.append(ob)
    bpy.ops.object.select_all(action='DESELECT'); root.select_set(True)
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.export_scene.fbx(filepath=OUT+'/'+name+'.fbx', use_selection=True, object_types={'EMPTY', 'MESH'}, axis_forward='-Z', axis_up='Y',
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False, use_mesh_modifiers=True, mesh_smooth_type='FACE')
    counts = []
    for o in objs: o.data.calc_loop_triangles(); counts.append(len(o.data.loop_triangles))
    # Unity tách submesh theo thứ tự vật liệu xuất hiện lần đầu trên các mặt (không theo thứ tự slot Blender).
    def first_use(me):
        order = []
        for poly in me.polygons:
            n = me.materials[poly.material_index].name
            if n not in order: order.append(n)
        return order
    orders = [first_use(o.data) for o in objs]
    assert all(o == orders[0] for o in orders), orders
    info = dict(name=name, generator='pho_viet_river_bridge.py', reference='ArtSource/Reference/River/river_bridge_reference.png',
                lod_triangles=counts, slots=orders[0], blender_slots=[m.name for m in objs[0].data.materials])
    info.update(getattr(geos[0], 'meta', {}))
    with open(OUT+'/'+name+'.json', 'w', encoding='utf8') as f: json.dump(info, f, ensure_ascii=False, indent=2)
    root.location = offset
    for o in objs[1:]: o.hide_set(True); o.hide_render = True
    return info

def build_river_bridge():
    global MAT, OUT
    os.makedirs(OUT, exist_ok=True)
    sc = bpy.data.scenes.get(SCENE) or bpy.data.scenes.new(SCENE)
    bpy.context.window.scene = sc
    coll = bpy.data.collections.get(SCENE) or bpy.data.collections.new(SCENE)
    if coll.name not in sc.collection.children: sc.collection.children.link(coll)
    for o in list(coll.objects):
        data = o.data; bpy.data.objects.remove(o, do_unlink=True)
        if isinstance(data, bpy.types.Mesh) and data.users == 0: bpy.data.meshes.remove(data)
    # Cùng atlas PV_StreetAtlas/PV_StreetPalette.png của phố (không ghi lại PNG bảng màu).
    MAT = bpy.data.materials.get('PV_StreetAtlas')
    global GLASS
    GLASS = bpy.data.materials.get('PV_StreetGlass') or bpy.data.materials.new('PV_StreetGlass')
    if MAT is None:
        river_out = OUT; OUT = ROOT+'/Assets/PhoViet/Art/Streets'; MAT = palette_material(); OUT = river_out
    mats = river_materials(); results = []
    results.append(export('PV_RiverBridge', [bridge_tile(d) for d in range(3)], coll, (0, 0, 0), mats))
    for kind in (0, 1):
        results.append(export('PV_RiverBoat'+str(kind), [boat(kind, d) for d in range(3)], coll, (0, -20-kind*12, 0), mats))
    # Lưu nguồn riêng chỉ gồm scene cầu (không ghi đè PhoViet_SaigonStreets.blend đang mở).
    ids = {sc, coll}|set(coll.objects)|{o.data for o in coll.objects if o.data}|set(mats.values())|{MAT}
    bpy.data.libraries.write(SOURCE, ids, fake_user=True)
    return dict(success=True, results=[(r['name'], r['lod_triangles'], r['slots']) for r in results])
