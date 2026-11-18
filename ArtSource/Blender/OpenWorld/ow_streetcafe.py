# Dù và bàn ghế quán vỉa hè chi tiết (21.244) — nạp trong build_ref2_segment.py (cùng namespace: MB, mat, mat_noise…).
# Dù: đế gang, trụ hai đoạn có khớp + tay quay, 8 tấm bạt hai màu cong võng, diềm rủ có mũi nhọn, nan + chụp đầu nan,
# núm đỉnh. Ghế nhựa có tựa (chân loe, tựa có khe), ghế đẩu nhựa thấp, bàn nhôm vuông có viền + thanh giằng,
# đồ trên bàn: ống đũa, chai tương ớt/xì dầu, hộp giấy, tô/đĩa cơm tấm, ly trà đá có đá viên.
import math, random
from mathutils import Vector, Matrix

def cafe_mats():
    C = {}
    def m(k, c, rough=0.5, metal=0.0, emit=None, es=0.0): C[k] = mat('OW_Cf_' + k, c, rough, metal, emit, es)
    m('fab_red', '#C42F28', 0.75); m('fab_cream', '#F1EBDC', 0.75); m('fab_blue', '#2B62AE', 0.75); m('fab_green', '#2F7D4A', 0.75)
    m('pole', '#3A3D40', 0.35, 0.8); m('rib', '#C9CCCF', 0.3, 0.9); m('base', '#2A2C2E', 0.6, 0.3)
    m('plastic_red', '#D2392D', 0.32); m('plastic_blue', '#2F70C4', 0.32); m('plastic_green', '#2E9A55', 0.32)
    m('alu', '#C6CBCF', 0.28, 0.85); m('alu_dark', '#8E9398', 0.35, 0.85)
    m('ceramic', '#F6F4EE', 0.2); m('rice', '#FBF8F0', 0.6); m('pork', '#8A4A22', 0.5); m('egg', '#F2C230', 0.4); m('herb', '#4C8A2E', 0.6)
    m('broth', '#B8742E', 0.15); m('glass', '#DDEBEE', 0.05); m('tea', '#B0641E', 0.1); m('ice', '#EAF4F6', 0.05)
    m('chili_sauce', '#C2231A', 0.25); m('soy', '#2A160C', 0.2); m('cap', '#E8E2D2', 0.4); m('bamboo', '#C9A15A', 0.6)
    m('napkin', '#FFFFFF', 0.8)
    m('wood_chair', '#6E4529', 0.55)          # ghế gỗ quán cà phê (ảnh 1)
    return C

def _T(cx, cy, yaw):
    rz = Matrix.Rotation(yaw, 3, 'Z'); o = Vector((cx, cy, 0))
    return lambda x, y, z: tuple(o + rz @ Vector((x, y, z)))

# ---------------------------------------------------------------- dù
def umbrella_pro(mb, C, x, y, R=1.35, H=2.5, fa='fab_red', fb='fab_cream', trim='fab_red', panels=8):
    # Đế + trụ.
    mb.cyl((x, y, 0.15), 0.3, 0.07, C['base'], seg=20)
    mb.cyl((x, y, 0.22), 0.2, 0.05, C['base'], seg=20, r2=0.12)
    mb.cyl((x, y, 0.27), 0.035, 1.25, C['pole'], seg=10)
    mb.cyl((x, y, 1.5), 0.045, 0.08, C['rib'], seg=10)                       # khớp nối
    mb.box((x + 0.06, y, 1.4), (0.08, 0.03, 0.12), C['rib'])                # tay quay
    mb.cyl((x, y, 1.58), 0.026, H - 1.5, C['pole'], seg=10)
    mb.cyl((x, y, H - 0.42), 0.05, 0.1, C['rib'], seg=10)                    # con chạy
    apex = (x, y, H + 0.04)
    ring = lambda rr, z, a: (x + math.cos(a) * rr, y + math.sin(a) * rr, z)
    for i in range(panels):
        a0 = 2 * math.pi * i / panels; a1 = 2 * math.pi * (i + 1) / panels; am = (a0 + a1) / 2
        f = C[fa] if i % 2 == 0 else C[fb]
        # Tấm bạt cong võng: nan cao hơn, giữa tấm võng xuống.
        m0, m1, mm = ring(R * 0.5, H - 0.13, a0), ring(R * 0.5, H - 0.13, a1), ring(R * 0.48, H - 0.17, am)
        r0, r1, rm = ring(R, H - 0.42, a0), ring(R, H - 0.42, a1), ring(R * 0.94, H - 0.45, am)
        mb.quad([apex, m0, mm], f); mb.quad([apex, mm, m1], f)
        mb.quad([m0, r0, rm, mm], f); mb.quad([mm, rm, r1, m1], f)
        # Diềm rủ có mũi nhọn ở giữa tấm.
        d0, d1 = (r0[0], r0[1], r0[2] - 0.16), (r1[0], r1[1], r1[2] - 0.16)
        dm = (rm[0], rm[1], rm[2] - 0.24)
        mb.quad([r0, d0, dm, rm], C[trim]); mb.quad([rm, dm, d1, r1], C[trim])
        # Nan dù + chụp đầu nan.
        mb.rod((x, y, H - 0.37), (r0[0], r0[1], r0[2] - 0.02), 0.008, C['rib'], seg=4)
        mb.rod((x, y, H - 0.4), ring(R * 0.5, H - 0.2, a0), 0.006, C['rib'], seg=4)
        mb.sphere((r0[0], r0[1], r0[2] - 0.02), 0.018, C['pole'], seg=6, rings=4)
    mb.sphere((x, y, H + 0.09), 0.045, C['pole'], seg=8, rings=5)
    mb.cyl((x, y, H + 0.02), 0.06, 0.05, C['pole'], seg=10)

# ---------------------------------------------------------------- ghế, bàn
def plastic_chair(mb, C, x, y, yaw, key='plastic_red'):
    """Ghế nhựa có tựa (lưng quay về −x cục bộ trước khi xoay)."""
    T = _T(x, y, yaw); m = C[key]; s = 0.21; hz = 0.45
    mb.box(T(0, 0, hz), (0.44, 0.44, 0.035), m, rz=yaw)
    mb.box(T(0.215, 0, hz - 0.04), (0.02, 0.44, 0.07), m, rz=yaw)                 # mép trước
    for sx, sy in ((1, 1), (1, -1), (-1, 1), (-1, -1)):
        mb.rod(T(sx * s, sy * s, hz - 0.02), T(sx * (s + 0.05), sy * (s + 0.05), 0.15), 0.018, m, seg=6)
    mb.rod(T(0.24, -0.24, 0.3), T(0.24, 0.24, 0.3), 0.009, m, seg=4)              # thanh giằng
    mb.rod(T(-0.24, -0.24, 0.3), T(-0.24, 0.24, 0.3), 0.009, m, seg=4)
    # Tựa lưng hơi ngả: hai trụ + đỉnh tựa + 4 nan dọc (thấy khe).
    for sy in (-1, 1):
        mb.rod(T(-0.21, sy * 0.2, hz), T(-0.27, sy * 0.19, hz + 0.42), 0.016, m, seg=6)
    mb.box(T(-0.27, 0, hz + 0.4), (0.03, 0.42, 0.09), m, rz=yaw)
    mb.box(T(-0.235, 0, hz + 0.12), (0.025, 0.4, 0.05), m, rz=yaw)
    for k in range(4):
        yy = -0.13 + k * 0.087
        mb.rod(T(-0.235, yy, hz + 0.14), T(-0.265, yy, hz + 0.36), 0.012, m, seg=4)

def stool(mb, C, x, y, key='plastic_blue'):
    m = C[key]; h = 0.42
    mb.box((x, y, h), (0.3, 0.3, 0.035), m)
    mb.box((x, y, h - 0.05), (0.27, 0.27, 0.06), m)
    for sx, sy in ((1, 1), (1, -1), (-1, 1), (-1, -1)):
        mb.rod((x + sx * 0.12, y + sy * 0.12, h - 0.05), (x + sx * 0.17, y + sy * 0.17, 0.15), 0.016, m, seg=6)
    mb.cyl((x, y, h + 0.013), 0.04, 0.01, C['alu_dark'], seg=8)                    # lỗ tay nắm

def alu_table(mb, C, x, y, w=0.7, h=0.72):
    mb.box((x, y, h), (w, w, 0.025), C['alu'])
    for s in (-1, 1):
        mb.box((x + s * w / 2, y, h - 0.03), (0.02, w, 0.05), C['alu'])
        mb.box((x, y + s * w / 2, h - 0.03), (w, 0.02, 0.05), C['alu'])
    for sx, sy in ((1, 1), (1, -1), (-1, 1), (-1, -1)):
        mb.rod((x + sx * (w / 2 - 0.05), y + sy * (w / 2 - 0.05), h - 0.04), (x + sx * (w / 2 - 0.02), y + sy * (w / 2 - 0.02), 0.15), 0.016, C['alu'], seg=6)
        mb.cyl((x + sx * (w / 2 - 0.02), y + sy * (w / 2 - 0.02), 0.15), 0.022, 0.02, C['base'], seg=6)
    for s in (-1, 1):
        mb.rod((x - (w / 2 - 0.04), y + s * (w / 2 - 0.04), 0.3), (x + (w / 2 - 0.04), y + s * (w / 2 - 0.04), 0.3), 0.008, C['alu'], seg=4)

def table_items(mb, C, x, y, h, r, dish):
    z = h + 0.0125
    # Ống đũa tre + đũa.
    mb.cyl((x - 0.2, y - 0.2, z), 0.04, 0.12, C['bamboo'], seg=10)
    for k in range(6):
        a = k * 1.05
        mb.rod((x - 0.2 + math.cos(a) * 0.015, y - 0.2 + math.sin(a) * 0.015, z + 0.05), (x - 0.2 + math.cos(a) * 0.03, y - 0.2 + math.sin(a) * 0.03, z + 0.24), 0.004, C['bamboo'], seg=3)
    # Chai tương ớt + xì dầu.
    for k, sauce in enumerate(('chili_sauce', 'soy')):
        bx, by = x - 0.2 + 0.09 * (k + 1), y - 0.22
        mb.cyl((bx, by, z), 0.028, 0.13, C[sauce], seg=10)
        mb.cyl((bx, by, z + 0.13), 0.018, 0.035, C[sauce], seg=8, r2=0.01)
        mb.cyl((bx, by, z + 0.16), 0.012, 0.02, C['cap'], seg=6)
    mb.box((x + 0.2, y - 0.2, z + 0.04), (0.12, 0.09, 0.08), C['napkin'])             # hộp giấy
    # Món ăn + ly trà đá cho 2 người.
    for s in (-1, 1):
        px, py = x + s * 0.13, y + 0.1
        if dish == 'com_tam':
            mb.cyl((px, py, z), 0.12, 0.015, C['ceramic'], seg=20, r2=0.13)
            mb.sphere((px - 0.03, py, z + 0.03), 0.055, C['rice'], seg=10, rings=5, scale=(1.2, 1, 0.55))
            mb.box((px + 0.05, py + 0.02, z + 0.03), (0.09, 0.05, 0.02), C['pork'], rz=0.4)
            mb.cyl((px + 0.04, py - 0.05, z + 0.017), 0.03, 0.01, C['egg'], seg=10)
            mb.box((px - 0.07, py - 0.06, z + 0.022), (0.04, 0.03, 0.01), C['herb'])
        else:   # tô phở/bún
            mb.cyl((px, py, z), 0.045, 0.02, C['ceramic'], seg=16)
            mb.cyl((px, py, z + 0.02), 0.06, 0.07, C['ceramic'], seg=20, r2=0.095)
            mb.cyl((px, py, z + 0.075), 0.085, 0.005, C['broth'], seg=20)
            for k in range(4):
                mb.box((px + r.uniform(-0.04, 0.04), py + r.uniform(-0.04, 0.04), z + 0.082), (0.03, 0.015, 0.006), C['herb'], rz=r.uniform(0, 3))
            mb.rod((px + 0.04, py - 0.05, z + 0.09), (px + 0.1, py - 0.12, z + 0.11), 0.004, C['bamboo'], seg=3)
        gx, gy = px + s * 0.12, py - 0.12
        mb.cyl((gx, gy, z), 0.035, 0.11, C['glass'], seg=12, r2=0.04)
        mb.cyl((gx, gy, z + 0.005), 0.032, 0.085, C['tea'], seg=12, r2=0.036)
        for k in range(3):
            mb.box((gx + (k - 1) * 0.015, gy + (k % 2) * 0.01, z + 0.09), (0.022, 0.022, 0.022), C['ice'], rz=k)

def street_cafe_set(mb, C, x, y, r, dish, chair_key, stools=False):
    alu_table(mb, C, x, y)
    table_items(mb, C, x, y, 0.72, r, dish)
    for k, (dx, dy, yaw) in enumerate(((-0.62, 0.0, 0.0), (0.62, 0.0, math.pi), (0.0, -0.6, math.pi / 2), (0.0, 0.6, -math.pi / 2))):
        if stools and k >= 2: stool(mb, C, x + dx, y + dy, 'plastic_blue')
        else: plastic_chair(mb, C, x + dx + r.uniform(-0.04, 0.04), y + dy, yaw + r.uniform(-0.15, 0.15), chair_key)
