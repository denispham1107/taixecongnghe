# Chợ Bến Thành chi tiết — bản 3 (21.242), nạp trong build_ref2_segment.py (cùng namespace: MB, M, mat, text, gable…).
# Hệ cục bộ đoạn Lê Lợi: đường dọc +y, chợ bên trái (x âm), mặt dãy vòm quay về +x (đường).
# Theo ảnh 2 phóng to: vòm bán nguyệt lớn sát nhau, viền vòm dày, hoa sắt rẻ quạt trong lòng vòm, chân trụ sơn xanh,
# trụ áp tường suốt chiều cao, dải đầu tường + gờ răng cưa, đầu hồi tam giác có huy hiệu, mái ngói ống vươn diềm;
# khối cổng chính cao có dải chữ đỏ CHỢ BẾN THÀNH, trụ góc có chóp trang trí; tháp đồng hồ lùi phía sau, số La Mã.
import math

BT = dict(
    XA=-22.0,        # mặt dãy vòm
    YA=50.0, YB=178.0,
    YC=104.0,        # tâm khối cổng + tháp (theo y)
    TW=14.0,         # chiều sâu khối cổng theo y
    TX=-14.5,        # mặt khối cổng phía đường
    AH=7.6,          # đỉnh gờ đầu tường dãy vòm
)
ROMAN = ['XII', 'I', 'II', 'III', 'IV', 'V', 'VI', 'VII', 'VIII', 'IX', 'X', 'XI']

def _arc(cu, sv, r, a0, a1, n=16):
    return [(cu + r * math.cos(math.radians(a0 + (a1 - a0) * i / n)), sv + r * math.sin(math.radians(a0 + (a1 - a0) * i / n))) for i in range(n + 1)]

def _box_axis(mb, axis, u, v, w, su, sv, sw, m, rot=0.0):
    """Hộp theo hệ (u dọc mặt tường, v cao, w pháp tuyến). rot: xoay trong mặt tường."""
    if axis == 'x': mb.box((w, u, v), (sw, su, sv), m, rx=rot)
    else: mb.box((u, w, v), (su, sw, sv), m, ry=-rot)

def _slab(mb, axis, w0, w1, pts, m):
    if w0 > w1: w0, w1 = w1, w0
    mb.profile(pts, axis, w0, w1, m)

def arched_wall(mb, axis, wf, depth, u0, u1, z0, z1, oc, ow, spring, m, sgn=1.0):
    """Mảng tường có lỗ vòm đục từ chân. sgn=+1: mặt trước ở wf quay về +w, tường lùi về −w."""
    r = ow / 2
    pts = [(u0, z0), (oc - r, z0), (oc - r, spring)] + _arc(oc, spring, r, 180, 0)[1:-1] + [(oc + r, spring), (oc + r, z0), (u1, z0), (u1, z1), (u0, z1)]
    _slab(mb, axis, wf, wf - sgn * depth, pts, m)

def half_ring(mb, axis, w0, w1, oc, spring, r_in, r_out, m, a0=180, a1=0):
    _slab(mb, axis, w0, w1, _arc(oc, spring, r_out, a0, a1) + _arc(oc, spring, r_in, a1, a0), m)

def archivolt(mb, axis, wf, oc, ow, spring, m, sgn=1.0, width=0.42):
    """Viền vòm hai lớp: lớp ngoài dày nổi 0,16, lớp trong mảnh nổi 0,08; đá khóa; gờ chân vòm."""
    r = ow / 2
    half_ring(mb, axis, wf, wf + sgn * 0.16, oc, spring, r + 0.14, r + width, m)
    half_ring(mb, axis, wf, wf + sgn * 0.08, oc, spring, r, r + 0.1, m)
    _box_axis(mb, axis, oc, spring + r + width / 2 + 0.08, wf + sgn * 0.12, 0.55, width + 0.45, 0.24, m)
    for s in (-1, 1):
        _box_axis(mb, axis, oc + s * (r + width / 2), spring - 0.1, wf + sgn * 0.1, width + 0.35, 0.28, 0.2, m)

def fan_grille(mb, axis, wf, oc, ow, spring, m, sgn=1.0, rays=11):
    """Hoa sắt rẻ quạt trong lòng vòm (ảnh 2): vành ngoài/giữa/trong, tia, vòng tròn nhỏ, hai thanh ngang."""
    r = ow / 2; w0, w1 = wf - sgn * 0.42, wf - sgn * 0.48
    for ri, ro in ((r - 0.1, r), (r * 0.62, r * 0.62 + 0.07), (r * 0.22, r * 0.22 + 0.07)):
        half_ring(mb, axis, w0, w1, oc, spring, ri, ro, m)
    wm = (w0 + w1) / 2
    for k in range(rays):
        a = math.pi * (k + 0.5) / rays; L = r * 0.78; rm = r * 0.22 + L / 2
        _box_axis(mb, axis, oc + math.cos(a) * rm, spring + math.sin(a) * rm, wm, 0.05, L, 0.06, m, rot=a - math.pi / 2)
    for k in range(rays - 1):
        a = math.pi * (k + 1) / rays; rc = r * 0.81
        cu, cv = oc + math.cos(a) * rc, spring + math.sin(a) * rc
        rr = min(0.24, r * 0.12)
        half_ring(mb, axis, w0, w1, cu, cv, rr - 0.045, rr, m, 180, 0)
        half_ring(mb, axis, w0, w1, cu, cv, rr - 0.045, rr, m, 360, 180)
    _box_axis(mb, axis, oc, spring, wm, ow, 0.16, 0.08, m)
    _box_axis(mb, axis, oc, spring - 0.45, wm, ow, 0.08, 0.06, m)

def pilaster(mb, axis, wf, u, z0, z1, m, sgn=1.0, wid=0.6):
    _box_axis(mb, axis, u, (z0 + z1) / 2, wf + sgn * 0.07, wid, z1 - z0, 0.14, m)
    _box_axis(mb, axis, u, z1 - 0.15, wf + sgn * 0.14, wid + 0.26, 0.3, 0.28, m)
    _box_axis(mb, axis, u, z1 - 0.38, wf + sgn * 0.1, wid + 0.12, 0.16, 0.2, m)
    _box_axis(mb, axis, u, z0 + 0.2, wf + sgn * 0.12, wid + 0.2, 0.4, 0.24, m)

def green_base(mb, axis, wf, u, m, sgn=1.0, wid=1.15, h=1.35):
    _box_axis(mb, axis, u, 0.15 + h / 2, wf + sgn * 0.13, wid, h, 0.26, m)
    _box_axis(mb, axis, u, 0.15 + h + 0.05, wf + sgn * 0.16, wid + 0.1, 0.1, 0.32, m)

def entablature(mb, axis, wf, u0, u1, z, mt, mw, sgn=1.0):
    """Gờ dưới, dải trơn, gờ mái nhô + răng cưa. Trả về cao độ đỉnh."""
    uc, L = (u0 + u1) / 2, u1 - u0
    _box_axis(mb, axis, uc, z + 0.15, wf + sgn * 0.09, L, 0.3, 0.18, mt)
    _box_axis(mb, axis, uc, z + 0.62, wf + sgn * 0.03, L, 0.64, 0.06, mw)
    _box_axis(mb, axis, uc, z + 1.02, wf + sgn * 0.12, L + 0.1, 0.16, 0.24, mt)
    _box_axis(mb, axis, uc, z + 1.22, wf + sgn * 0.3, L + 0.3, 0.24, 0.6, mt)
    n = int(L / 0.34)
    for i in range(n):
        _box_axis(mb, axis, u0 + 0.17 + i * 0.34, z + 1.0, wf + sgn * 0.2, 0.16, 0.14, 0.2, mt)
    return z + 1.34

def medallion(mb, axis, wf, u, v, r, m, sgn=1.0):
    if axis == 'x': mb.cyl((wf + (0 if sgn > 0 else -0.08), u, v), r, 0.08, m, seg=20, axis='X')
    else: mb.cyl((u, wf + (0 if sgn > 0 else -0.08), v), r, 0.08, m, seg=20, axis='Y')

def pediment(mb, axis, wf, u0, u1, z0, h, mw, mt, sgn=1.0, cartouche=True):
    uc = (u0 + u1) / 2
    _slab(mb, axis, wf - sgn * 0.6, wf, [(u0, z0), (u1, z0), (uc, z0 + h)], mw)
    L = math.hypot((u1 - u0) / 2, h); ang = math.atan2(h, (u1 - u0) / 2)
    for s in (-1, 1):
        cu = uc + s * (u1 - u0) / 4; cv = z0 + h / 2 + 0.12
        _box_axis(mb, axis, cu, cv, wf + sgn * 0.18, L + 0.5, 0.34, 0.4, mt, rot=-s * ang)
        _box_axis(mb, axis, cu, cv - 0.22, wf + sgn * 0.08, L + 0.2, 0.12, 0.2, mt, rot=-s * ang)
    if cartouche:
        if axis == 'x': mb.cyl((wf, uc, z0 + h * 0.4), 0.85, 0.14, mt, seg=24, axis='X')
        else: mb.cyl((uc, wf - 0.14, z0 + h * 0.4), 0.85, 0.14, mt, seg=24, axis='Y')
        medallion(mb, axis, wf + sgn * 0.12, uc, z0 + h * 0.4, 0.55, mw, sgn)
        _box_axis(mb, axis, uc, z0 + h * 0.4 + 0.95, wf + sgn * 0.1, 0.5, 0.35, 0.18, mt)

def hip_roof(mb, x0, x1, y0, y1, ze, pitch, m, mf, over=1.0):
    """Mái bốn mái, nóc dọc trục dài; diềm gỗ quanh mép mái. Trả về cao độ nóc."""
    x0 -= over; x1 += over; y0 -= over; y1 += over
    dx, dy = x1 - x0, y1 - y0
    xm, ym = (x0 + x1) / 2, (y0 + y1) / 2
    if dy >= dx:
        h = dx / 2; zr = ze + h * pitch
        R0, R1 = (xm, y0 + h, zr), (xm, y1 - h, zr)
        mb.quad([(x1, y0, ze), (x1, y1, ze), R1, R0], m); mb.quad([(x0, y1, ze), (x0, y0, ze), R0, R1], m)
        mb.quad([(x0, y0, ze), (x1, y0, ze), R0], m); mb.quad([(x1, y1, ze), (x0, y1, ze), R1], m)
    else:
        h = dy / 2; zr = ze + h * pitch
        R0, R1 = (x0 + h, ym, zr), (x1 - h, ym, zr)
        mb.quad([(x0, y0, ze), (x1, y0, ze), R1, R0], m); mb.quad([(x1, y1, ze), (x0, y1, ze), R0, R1], m)
        mb.quad([(x0, y1, ze), (x0, y0, ze), R0], m); mb.quad([(x1, y0, ze), (x1, y1, ze), R1], m)
    if R0 != R1: mb.rod(R0, R1, 0.12, m, seg=6)
    for (a, b) in (((x0, y0), (x1, y0)), ((x1, y0), (x1, y1)), ((x1, y1), (x0, y1)), ((x0, y1), (x0, y0))):
        mb.boxab((min(a[0], b[0]) - 0.04, min(a[1], b[1]) - 0.04, ze - 0.32), (max(a[0], b[0]) + 0.04, max(a[1], b[1]) + 0.04, ze + 0.02), mf)
    return zr

def roman_clock(C, mb, axis, wf, u, z, R, sgn=1.0):
    T, F, H = M['market_trim'], M['clock_face'], M['clock_hand']
    def disc(r, off, th, m):
        w = wf + sgn * off
        if axis == 'x': mb.cyl((w if sgn > 0 else w - th, u, z), r, th, m, seg=48, axis='X')
        else: mb.cyl((u, w if sgn > 0 else w - th, z), r, th, m, seg=48, axis='Y')
    disc(R + 0.38, 0.0, 0.16, T)
    disc(R + 0.1, 0.16, 0.03, H)
    disc(R, 0.19, 0.04, F)
    for k in range(60):
        if k % 5 == 0: continue
        a = math.radians(90 - k * 6)
        _box_axis(mb, axis, u + math.cos(a) * (R - 0.08), z + math.sin(a) * (R - 0.08), wf + sgn * 0.24, 0.025, 0.08, 0.02, H, rot=a - math.pi / 2)
    for ang, L, wdt in ((150, R * 0.5, 0.11), (52, R * 0.8, 0.07)):
        a = math.radians(ang); cu, cv = u + math.cos(a) * L / 2, z + math.sin(a) * L / 2
        _box_axis(mb, axis, cu, cv, wf + sgn * 0.27, wdt, L, 0.03, H, rot=a - math.pi / 2)
    disc(0.1, 0.25, 0.06, H)
    rot = (math.pi / 2, 0, math.pi / 2) if axis == 'x' else (math.pi / 2, 0, 0)
    for k, num in enumerate(ROMAN):
        a = math.radians(90 - k * 30); cu, cv = u + math.cos(a) * (R - 0.36), z + math.sin(a) * (R - 0.36)
        loc = (wf + sgn * 0.24, cu, cv) if axis == 'x' else (cu, wf + sgn * 0.24, cv)
        text(C, num, loc, R * 0.2, H, rot, 'timesbd.ttf', extrude=0.005, name='OW_SoLaMa_' + num)

def stall_interior(mb, x_back, y0, y1, z_top, r):
    mb.boxab((x_back - 0.1, y0, 0.15), (x_back, y1, z_top), M['shop'])
    goods = [M['flower_y'], M['red'], M['cart_yellow'], M['umb_blue'], M['frame_white'], M['chair_red']]
    y = y0 + 0.3
    while y < y1 - 0.6:
        mb.boxab((x_back + 1.2, y, 0.15), (x_back + 2.0, y + 1.1, 1.0), M['market_door'])
        for k in range(3):
            mb.box((x_back + 1.6, y + 0.25 + k * 0.3, 1.12), (0.5, 0.24, 0.24), r.choice(goods))
        for k in range(4):
            mb.box((x_back + 0.25, y + 0.15 + k * 0.25, 1.4 + (k % 2) * 0.5), (0.3, 0.2, 0.4), r.choice(goods))
        y += 1.4

def finial(mb, x, y, z, mt):
    mb.boxab((x - 0.5, y - 0.5, z), (x + 0.5, y + 0.5, z + 0.7), mt)
    mb.boxab((x - 0.6, y - 0.6, z + 0.7), (x + 0.6, y + 0.6, z + 0.85), mt)
    mb.cyl((x, y, z + 0.85), 0.32, 0.5, mt, seg=12, r2=0.18)
    mb.cyl((x, y, z + 1.35), 0.22, 0.35, mt, seg=12, r2=0.02)

def arcade(mb, ins, a, b, r):
    """Đoạn hành lang vòm dọc đường từ y=a tới y=b. Trả về (số khoang, bề khoang, cao độ đỉnh dải đầu tường)."""
    XA, AH = BT['XA'], BT['AH']
    W, T, G, GR = M['market_wall'], M['market_trim'], M['market_door'], M['grille']
    n = max(1, round((b - a) / 5.3)); bw = (b - a) / n
    OW, SP = bw - 1.15, 3.3
    top = AH - 1.4
    for i in range(n):
        yc = a + (i + 0.5) * bw
        arched_wall(mb, 'x', XA, 0.9, yc - bw / 2, yc + bw / 2, 0.15, top, yc, OW, SP, W)
        archivolt(mb, 'x', XA, yc, OW, SP, T)
        fan_grille(mb, 'x', XA, yc, OW, SP, GR)
        medallion(mb, 'x', XA, yc - bw / 2, SP + OW / 2 + 0.25, 0.26, T)
    for i in range(n + 1):
        u = a + i * bw
        pilaster(mb, 'x', XA, u, 0.15, top, T, wid=0.5)
        green_base(mb, 'x', XA, u, G)
    ztop = entablature(mb, 'x', XA, a, b, top, T, W)
    mb.boxab((XA - 5.0, a, top - 0.2), (XA - 0.9, b, top), T)
    mb.boxab((XA - 5.0, a, 0.0), (XA, b, 0.2), M['paving'])
    stall_interior(ins, XA - 5.0, a + 0.4, b - 0.4, top - 0.2, r)
    return n, bw, ztop

def build_ben_thanh(C):
    import random
    r = random.Random(1914)
    XA, YA, YB, YC, TW, TX, AH = BT['XA'], BT['YA'], BT['YB'], BT['YC'], BT['TW'], BT['TX'], BT['AH']
    M['grille'] = mat('OW_MarketGrille', '#E9E1C9', 0.45, 0.3)
    M['roof_barrel'] = mat_barrel_tiles('OW_RoofBarrelTiles')
    M['fascia'] = mat('OW_RoofFascia', '#5A3B26', 0.6)
    W, T, D, G, R, F = M['market_wall'], M['market_trim'], M['market_dark'], M['market_door'], M['roof_barrel'], M['fascia']
    mb = MB('OW_ChoBenThanh'); ins = MB('OW_ChoBenThanh_NoiThat'); rf = MB('OW_ChoBenThanh_Mai')
    t0, t1 = YC - TW / 2, YC + TW / 2
    PV = 10.0
    # ---- thân chợ phía sau + mái bốn mái lớn + mái lồng đèn
    mb.boxab((XA - 62, YA + 1, 0.15), (XA - 5.0, YB - 1, AH + 2.4), W)
    hip_roof(rf, XA - 62, XA - 5.0, YA + 1, YB - 1, AH + 2.4, 0.2, R, F, 1.0)      # mái thân chợ thấp, chỉ ló sau dãy vòm
    mb.boxab((XA - 38, YA + 18, AH + 6.5), (XA - 29, YB - 18, AH + 8.6), W)
    for y in range(int(YA + 20), int(YB - 19), 4):
        for xf, s in ((XA - 29, 1.0), (XA - 38, -1.0)):
            mb.box((xf + s * 0.04, y + 1, AH + 7.6), (0.08, 1.4, 1.2), D)
    hip_roof(rf, XA - 38, XA - 29, YA + 18, YB - 18, AH + 8.6, 0.45, R, F, 0.7)
    # ---- hai đoạn hành lang vòm + mái ngói vươn diềm + đầu hồi tam giác có huy hiệu ở giữa (3 khoang)
    for a, b in ((YA + PV, t0), (t1, YB - PV)):
        n, bw, z0 = arcade(mb, ins, a, b, r)
        hip_roof(rf, XA - 5.6, XA + 0.2, a, b, z0 + 0.15, 0.55, R, F, 0.9)
        mid = a + bw * (n // 2 + 0.5) if n % 2 else a + bw * (n / 2)
        p0, p1 = mid - bw * 1.5, mid + bw * 1.5
        mb.boxab((XA - 9, p0 + 0.3, z0), (XA - 0.2, p1 - 0.3, z0 + 0.9), W)
        pediment(mb, 'x', XA + 0.05, p0, p1, z0 + 0.9, 3.6, W, T)
        gable(rf, XA - 14, XA + 0.6, p0, p1, z0 + 1.0, z0 + 4.6, R, 'x', 0.5)
        for u in (p0, p1):
            finial(mb, XA - 0.4, u, z0 + 0.9, T)
    # ---- gian góc hai đầu: nhô 1 m, vòm lớn có hoa sắt, đầu hồi, mái hai dốc
    for p0 in (YA, YB - PV):
        p1 = p0 + PV; pc = (p0 + p1) / 2; xf = XA + 1.0
        mb.boxab((XA - 9.0, p0, 0.15), (xf - 0.9, p1, AH + 1.2), W)
        arched_wall(mb, 'x', xf, 0.9, p0, p1, 0.15, AH - 0.2, pc, 5.4, 3.8, W)
        archivolt(mb, 'x', xf, pc, 5.4, 3.8, T, width=0.5)
        fan_grille(mb, 'x', xf, pc, 5.4, 3.8, M['grille'], rays=13)
        mb.box((xf - 0.88, pc, 2.6), (0.05, 5.4, 5.0), M['shop'])
        for u in (p0 + 0.45, p1 - 0.45):
            pilaster(mb, 'x', xf, u, 0.15, AH - 0.2, T, wid=0.8)
            green_base(mb, 'x', xf, u, G, wid=1.3)
        z = entablature(mb, 'x', xf, p0, p1, AH - 0.2, T, W)
        pediment(mb, 'x', xf + 0.05, p0 - 0.1, p1 + 0.1, z, 3.2, W, T)
        gable(rf, XA - 9.5, xf + 0.7, p0 - 0.2, p1 + 0.2, z + 0.1, z + 3.3, R, 'x', 0.5)
        for u in (p0 + 0.2, p1 - 0.2):
            finial(mb, xf - 0.3, u, z, T)
    # Mặt đầu phía tây (quay về camera, y = YA + 1): cửa sổ vòm có hoa sắt + chân tường xanh.
    for k in range(6):
        xc = XA - 13 - k * 8.0
        if xc < XA - 60: break
        mb.box((xc, YA + 0.95, 3.0), (2.2, 0.06, 3.2), D)
        mb.cyl((xc, YA + 0.92, 4.6), 1.1, 0.06, D, seg=16, axis='Y')
        archivolt(mb, 'y', YA + 1.0, xc, 2.2, 4.6, T, sgn=-1.0, width=0.3)
        fan_grille(mb, 'y', YA + 0.45, xc, 2.2, 4.6, M['grille'], sgn=-1.0, rays=7)
    mb.boxab((XA - 62, YA + 0.84, 0.15), (XA - 9, YA + 1.0, 1.5), G)
    # ---- khối cổng chính (nhô tới TX), mặt chính quay về camera (−y)
    BH = AH + 0.6
    x0, x1 = XA - 5.0, TX
    cx0 = (x0 + x1) / 2
    ZA = BH + 1.34 + 1.9                                                        # đỉnh dải chữ
    mb.boxab((x0, t0 + 0.9, 0.15), (x1 - 0.9, t1, ZA), W)
    arched_wall(mb, 'y', t0, 0.9, x0, x1, 0.15, BH, cx0, 5.8, 4.4, W, sgn=-1.0)
    archivolt(mb, 'y', t0, cx0, 5.8, 4.4, T, sgn=-1.0, width=0.55)
    fan_grille(mb, 'y', t0, cx0, 5.8, 4.4, M['grille'], sgn=-1.0, rays=15)
    mb.box((cx0, t0 + 0.88, 2.4), (5.8, 0.05, 4.6), M['shop'])
    for s in (-1, 1):
        mb.box((cx0 + s * 2.2, t0 + 0.6, 2.2), (1.3, 0.08, 4.1), G)
    for u in (x0 + 0.7, x1 - 0.7, cx0 - 3.6, cx0 + 3.6):
        side = abs(u - cx0) > 4
        pilaster(mb, 'y', t0, u, 0.15, BH, T, sgn=-1.0, wid=0.85 if side else 0.6)
        green_base(mb, 'y', t0, u, G, sgn=-1.0, wid=1.3 if side else 1.0)
    z = entablature(mb, 'y', t0, x0, x1, BH, T, W, sgn=-1.0)
    mb.boxab((x0, t0 - 0.12, z), (x1, t0 + 0.9, z + 1.9), W)                  # dải chữ
    mb.boxab((x0 - 0.1, t0 - 0.3, z + 1.9), (x1 + 0.1, t0 + 0.9, z + 2.2), T)
    for u in (x0 + 0.5, x1 - 0.5):                                             # trụ góc + chóp trang trí
        mb.boxab((u - 0.6, t0 - 0.35, 0.15), (u + 0.6, t0 + 0.6, z + 2.2), T)
        finial(mb, u, t0 + 0.15, z + 2.2, T)
    ztext = z + 0.95
    # Mặt phía đường (+x) của khối cổng.
    arched_wall(mb, 'x', x1, 0.9, t0, t1, 0.15, BH, YC, 4.6, 4.0, W)
    archivolt(mb, 'x', x1, YC, 4.6, 4.0, T, width=0.5)
    fan_grille(mb, 'x', x1, YC, 4.6, 4.0, M['grille'], rays=13)
    mb.box((x1 - 0.88, YC, 2.3), (0.05, 4.6, 4.3), M['shop'])
    for u in (t0 + 0.6, t1 - 0.6):
        pilaster(mb, 'x', x1, u, 0.15, BH, T, wid=0.8)
        green_base(mb, 'x', x1, u, G, wid=1.2)
    z2 = entablature(mb, 'x', x1, t0, t1, BH, T, W)
    mb.boxab((x1 - 0.9, t0, z2), (x1 + 0.12, t1, z2 + 1.9), W)
    mb.boxab((x1 - 0.9, t0 - 0.1, z2 + 1.9), (x1 + 0.3, t1 + 0.1, z2 + 2.2), T)
    finial(mb, x1 - 0.3, t1 - 0.5, z2 + 2.2, T)
    mb.boxab((x0, t0 + 0.9, ZA - 0.1), (x1 - 0.9, t1, ZA + 0.05), M['concrete'])
    # ---- tháp đồng hồ lùi phía sau dải chữ
    tw = 7.0; tcx, tcy = cx0 - 0.5, t0 + 0.9 + 1.2 + tw / 2
    tx0, tx1, ty0, ty1 = tcx - tw / 2, tcx + tw / 2, tcy - tw / 2, tcy + tw / 2
    Z0 = z + 2.2; Z1 = Z0 + 0.9; Z2 = Z1 + 4.4; Z3 = Z2 + 0.6; Z4 = Z3 + 1.5
    mb.boxab((tx0, ty0, Z0 - 0.3), (tx1, ty1, Z4), W)
    mb.boxab((tx0 - 0.2, ty0 - 0.2, Z1 - 0.2), (tx1 + 0.2, ty1 + 0.2, Z1), T)
    for (axis, wf, sgn, u0, u1) in (('y', ty0, -1.0, tx0, tx1), ('x', tx1, 1.0, ty0, ty1)):
        for u in (u0 + 0.35, u1 - 0.35):
            pilaster(mb, axis, wf, u, Z1, Z2, T, sgn=sgn, wid=0.6)
        uc = (u0 + u1) / 2
        roman_clock(C, mb, axis, wf, uc, (Z1 + Z2) / 2, 1.55, sgn)
        _box_axis(mb, axis, uc, Z2 + 0.3, wf + sgn * 0.2, tw + 0.5, 0.6, 0.4, T)
        for k in (-1.8, 0.0, 1.8):
            _box_axis(mb, axis, uc + k, (Z3 + Z4) / 2, wf + sgn * 0.03, 0.9, 0.9, 0.06, D)
            _box_axis(mb, axis, uc + k, (Z3 + Z4) / 2, wf + sgn * 0.05, 1.15, 1.15, 0.03, T)
    zr = hip_roof(rf, tx0, tx1, ty0, ty1, Z4 + 0.1, 0.95, R, F, 1.1)
    mb.cyl((tcx, tcy, zr - 0.2), 0.1, 1.8, M['iron'])
    mb.cyl((tcx, tcy, zr + 0.6), 0.28, 0.2, M['iron'], seg=12)
    mb.finish(C); ins.finish(C); rf.finish(C)
    # Chữ đỏ nổi CHỢ BẾN THÀNH: dải chữ quay về camera (−y) và dải chữ mặt cổng quay ra đường (+x, yêu cầu 17/11/2026).
    text(C, 'CHỢ BẾN THÀNH', (cx0, t0 - 0.2, ztext), 1.25, M['sign_red'], (math.pi / 2, 0, 0), 'timesbd.ttf', extrude=0.08)
    text(C, 'CHỢ BẾN THÀNH', (x1 + 0.2, YC, z2 + 0.95), 1.3, M['sign_red'], (math.pi / 2, 0, math.pi / 2), 'timesbd.ttf', extrude=0.08)
    return dict(tower=(tcx, tcy, zr))
