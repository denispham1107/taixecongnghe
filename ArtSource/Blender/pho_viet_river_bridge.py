"""Cầu vượt sông theo ảnh mẫu ArtSource/Reference/River/river_bridge_reference.png (24/10/2026).
Chạy trong Blender thật qua Blender MCP: exec file này rồi gọi build_river_bridge().
Dùng chung bảng màu/atlas và lớp Geo của pho_viet_saigon_streets.py để cùng phong cách phố.

Tọa độ tác giả = tọa độ cục bộ một khối phố Unity (VietnamStreet, dài 80 m):
x ngang đường (làn ở ±1,55/±4,65), y lên, z dọc đường; giao lộ ở z 63–77.
Khối cầu: mặt sông z 16,8–42,0 (đúng mép các tấm vỉa hè 8,4 m), cầu chỉ 2 làn giữa |x| ≤ 3,1.
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

W0, W1 = 16.8, 42.0          # mặt nước giữa hai bờ kè (khớp mép tấm vỉa hè 16,8 và 42,0)
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
LAMPS = [22.0, 36.8]         # đèn LED (Unity đặt mẫu PV_LEDStreetLamp) ở x = ±3,55
LAMP_X = 3.55
BOATS = [  # (asset, x, z, yaw độ) — thuyền neo dọc kè, mũi theo trục x
    ('PV_RiverBoat0', -31.0, W1-3.3, 0.0),
    ('PV_RiverBoat1', 27.0, W0+3.2, 180.0),
    ('PV_RiverBoat1', -58.0, W0+3.0, 0.0),
    ('PV_RiverBoat0', 62.0, W1-3.4, 180.0),
]
EXTRA = ['Water', 'Murk', 'Mask', 'Asphalt', 'LineWhite', 'LineYellow']
EXTRA_MAT = {'Water': 'PV_RiverWater', 'Murk': 'PV_RiverMurk', 'Mask': 'PV_RiverGroundMask', 'Asphalt': 'PV_RiverAsphalt',
             'LineWhite': 'PV_RiverLineWhite', 'LineYellow': 'PV_RiverLineYellow'}

# ---------------------------------------------------------------- nước, kè, lối dạo
def water(g):
    # Lòng sông tối, các lớp nước đục (sâu trước, để vẽ đúng thứ tự trong suốt) rồi lưới mặt nước
    # đủ mịn cho sóng nhấp nhô dịch đỉnh trong shader (LOD0 ô ~1,25 m).
    g.quad([(-SIDE, BED_Y, W0), (SIDE, BED_Y, W0), (SIDE, BED_Y, W1), (-SIDE, BED_Y, W1)], 'navy', (0, 1, 0))
    for y in MURK:
        g.quad([(-SIDE, y, W0), (SIDE, y, W0), (SIDE, y, W1), (-SIDE, y, W1)], 'glass', (0, 1, 0), 'Murk')
    nx, nz = ((152, 20), (50, 6), (10, 2))[g.d]
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
    for zc in range(18, 42, 4):  # cùng nhịp vạch vàng đứt của phố: tâm 2 + 4k
        g.quad([(-.085, .007, zc-1.125), (.085, .007, zc-1.125), (.085, .007, zc+1.125), (-.085, .007, zc+1.125)], 'yellow', (0, 1, 0), 'LineYellow')
    for side in (-1, 1):
        # Gờ bó trắng, lan can, mặt ngoài dầm cầu.
        g.cube((side*(ROAD+CURB)/2, .075, (W0+W1)/2), (CURB-ROAD, .15, W1-W0), 'trim')
        baluster_run(g, side*(CURB+RAIL)/2, W0+.2, W1-.2, side)
        g.quad([(side*RAIL, DECK_BOTTOM, W0), (side*RAIL, DECK_BOTTOM, W1), (side*RAIL, .15, W1), (side*RAIL, .15, W0)], 'stone', (side, 0, 0))
        g.cube((side*(RAIL+.04), -.12, (W0+W1)/2), (.1, .22, W1-W0), 'trim')
        for z in LAMPS: lamp_flag(g, side, z)
        for k, z in enumerate([W0+2.3, 26.3, 32.5, W1-2.3]):
            flower_box(g, side*(CURB+RAIL)/2, 1.17, z, k*17+(3 if side > 0 else 0))
    # Mặt dưới dầm và trụ cầu chìm trong nước.
    g.quad([(-RAIL, DECK_BOTTOM, W0), (-RAIL, DECK_BOTTOM, W1), (RAIL, DECK_BOTTOM, W1), (RAIL, DECK_BOTTOM, W0)], 'potGrey', (0, -1, 0))
    for zp in (W0+8.4, W1-8.4):
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
    for z0, z1, toward in ((5.0, W0-.15, 1), (W1+.15, 48.0, -1)):
        for side in (-1, 1):
            # Đường thu hẹp: từ mép làn ngoài (|x| 6,2) chéo vào mép cầu (|x| 3,2) ở mép nước.
            za, zb = (z0, z1) if toward > 0 else (z1, z0)
            xa, xb = side*6.2, side*3.25
            dz = zb-za; dx = xb-xa; L = math.hypot(dx, dz); nx, nz = -dz/L*.07, dx/L*.07
            g.quad([(xa-nx, .011, za-nz), (xb-nx, .011, zb-nz), (xb+nx, .011, zb+nz), (xa+nx, .011, za+nz)], 'white', (0, 1, 0), 'LineWhite')
            n = 6 if d < 2 else 3
            for k in range(n):
                zz = z0+(k+.5)*(z1-z0)/n; t = (zz-za)/dz; xe = xa+dx*t
                inner, outer = xe, side*6.55
                # vạch chéo trong vùng cấm (giữa đường thu hẹp và mép ngoài)
                g.quad([(inner, .012, zz-.25), (outer, .012, zz+.45), (outer, .012, zz+.75), (inner, .012, zz+.05)], 'white', (0, 1, 0), 'LineWhite')
            # Gờ cảnh báo vàng–đen trên mép kè của làn ngoài (không chắn: xe đi thẳng vẫn rơi).
            zw = W0 if toward > 0 else W1
            stripes = 8 if d < 2 else 2
            for k in range(stripes):
                xa2 = side*(RAIL+k*(7.0-RAIL)/stripes); xb2 = side*(RAIL+(k+1)*(7.0-RAIL)/stripes)
                g.cube(((xa2+xb2)/2, .04, zw-toward*.12), (abs(xb2-xa2), .08, .24), 'yellow' if k % 2 == 0 else 'iron')
    # Biển "CẦU HẸP" (tam giác vàng viền đỏ, ký hiệu đường hẹp hai bên) cho cả hai chiều.
    for x, z, face in ((7.55, 3.0, -1), (-7.55, W1+6.0, 1)):
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
    promenade(g, W0-3.0, W0, W0-.1, 1)
    promenade(g, W1, W1+2.0, W1+.1, -1)
    for k, x in enumerate([-80, -66, -51, -39, -22, -12, 12, 20, 34, 47, 70, 86]):
        hanging_green(g, x, W0, 1, 100+k, 2.2+(k % 3)*.6)
        hanging_green(g, x+5, W1, -1, 300+k, 2.0+(k % 2)*.8)
    deck(g)
    bank_marks(g)
    for name, x, z, yaw in BOATS:
        jetty(g, x, (W1 if z > (W0+W1)/2 else W0), -1 if z > (W0+W1)/2 else 1)
    g.meta = dict(water=[W0, W1], water_y=WATER_Y, bed_y=BED_Y, murk=MURK, deck_half=ROAD, curb=CURB, rail=RAIL, side=SIDE, ground_top=GROUND_TOP,
                  lamps=LAMPS, lamp_x=LAMP_X, boats=[dict(asset=a, x=x, z=z, yaw=y) for a, x, z, y in BOATS],
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
                                                'LineWhite': hexcol('FFF1CF'), 'LineYellow': hexcol('FFD35A')}[tag]
        b.inputs['Roughness'].default_value = .08 if tag == 'Water' else .85
        mats[tag] = m
    return mats

def river_mesh(name, g, coll, mats):
    ob = to_mesh(name, g, coll); me = ob.data
    used = []
    for t in EXTRA:
        if any(tag == t for tag in g.tags): used.append(t); me.materials.append(mats[t])
    if used:
        # to_mesh đảo một số mặt nhưng giữ thứ tự đa giác: gán slot theo tag.
        for poly, t in zip(me.polygons, g.tags):
            if t in used: poly.material_index = 1+used.index(t)
            elif t == 'Glass': poly.material_index = 0
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
