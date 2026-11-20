# 21.272 — nội thất tầng trệt nhà phố bản đồ mở (14 loại cửa hàng theo biển hiệu + 4 kiểu nhà ở).
# Dùng cùng ow_lib (MB, mat, text). Hệ toạ độ CỤC BỘ của một căn nhà (giống shophouse(x_face=0) trong build_ref3_segment):
#   mặt tiền x = 0, nhà chạy vào trong theo −x, ngang nhà y ∈ [−w/2, w/2], z lên. Kính mặt tiền ở x = −0,40.
#   Lòng phòng: x ∈ [BACK, FRONT], y ∈ [−w/2 + 0,23, w/2 − 0,23], sàn z = FLOOR, trần z = CEIL.
# Mỗi hàm furnish_<loại>(mb, col, w, rnd) dựng đồ đạc vào MB (một mesh nhiều vật liệu) + chữ (đối tượng text riêng).
# Vật liệu tên 'NT_*' — khi xuất sang Unity màu gốc được ghi vào màu đỉnh (xem export trong build_interiors.py).
import math, random
from mathutils import Vector

FRONT, BACK, FLOOR, CEIL = -0.62, -8.2, 0.27, 4.05
SHOP_TYPES = ['PhoBo', 'ComTam', 'BanhMi', 'CaPhe', 'TraSua', 'TapHoa', 'NhaThuoc', 'SuaXe', 'HuTieu', 'TiemVang',
              'DienThoai', 'BunBoHue', 'ThoiTrang', 'BachHoa']          # đúng thứ tự SIGN_SET (OW_Mau_Bang_<w>_<i>)
SHOP_NAMES = ['Phở Bò', 'Cơm Tấm', 'Bánh Mì', 'Cà Phê', 'Trà Sữa', 'Tạp Hóa', 'Nhà Thuốc', 'Sửa Xe', 'Hủ Tiếu', 'Tiệm Vàng',
              'Điện Thoại', 'Bún Bò Huế', 'Thời Trang', 'Bách Hóa']
HOME_TYPES = ['NhaO_Salon', 'NhaO_XeMay', 'NhaO_PhongKhach', 'NhaO_ViVong']

_M = {}
def P(lib, name, hexc, rough=0.6, metal=0.0, emit=0.0, alpha=1.0):
    """Vật liệu bảng màu nội thất (cache)."""
    key = 'NT_' + name
    if key not in _M:
        m = lib.mat(key, hexc, rough=rough, metal=metal, emit=hexc if emit else None, emit_strength=emit)
        if alpha < 1.0:
            b = lib._bsdf(m); b.inputs['Alpha'].default_value = alpha
            try: m.surface_render_method = 'BLENDED'
            except Exception: pass
            m.use_backface_culling = False
        m['nt_emit'] = float(emit); m['nt_alpha'] = float(alpha)
        _M[key] = m
    return _M[key]

class Ctx:
    """Gói thư viện + vật liệu hay dùng cho các hàm dựng đồ."""
    def __init__(self, lib, mb, col, w, rnd):
        self.L, self.mb, self.col, self.w, self.r = lib, mb, col, w, rnd
        self.title, self.sub = None, None          # nhà nổi bật: tên tiệm thật trên biển (bảng tường hậu, bảng món)
        self.yl, self.yr = -w / 2 + 0.23, w / 2 - 0.23           # mép trong hai tường bên
        self.inner = self.yr - self.yl
        c = lambda *a, **k: P(lib, *a, **k)
        self.c = c
        self.steel = c('Inox', '#C9CED3', rough=0.25, metal=0.9)
        self.steelD = c('InoxToi', '#8C939A', rough=0.35, metal=0.8)
        self.black = c('DenNham', '#1E2124', rough=0.7)
        self.white = c('TrangSu', '#F2F1EC', rough=0.35)
        self.wood = c('GoNau', '#7A4E2C', rough=0.55)
        self.woodL = c('GoSang', '#B8875A', rough=0.55)
        self.woodD = c('GoToi', '#4A2E1A', rough=0.5)
        self.red = c('DoTuoi', '#C8262A', rough=0.45)
        self.gold = c('VangKim', '#D9A93A', rough=0.3, metal=0.85)
        self.glass = c('KinhTu', '#BFD8E2', rough=0.05, alpha=0.28)
        self.lamp = c('DenTran', '#FFF6E2', emit=6.0)
        self.lampCool = c('DenTranLanh', '#EAF4FF', emit=7.0)
        self.redLamp = c('DenDoTho', '#FF3A2A', emit=4.0)

    def box(self, c, s, m, **k): self.mb.box(c, s, m, **k)
    def floor_quad(self, x0, y0, x1, y1, z, m):
        """Ô gạch sàn: một mặt phẳng hướng lên (nhẹ hơn hộp 6 mặt)."""
        self.mb.quad([(x0, y0, z), (x1, y0, z), (x1, y1, z), (x0, y1, z)], m)
    def wall_quad(self, x0, x1, y, z0, z1, side, m):
        """Ô gạch ốp tường bên: mặt phẳng quay vào phòng (side +1 tường phải → pháp tuyến −y)."""
        pts = [(x0, y, z0), (x0, y, z1), (x1, y, z1), (x1, y, z0)]
        self.mb.quad(list(reversed(pts)) if side > 0 else pts, m)   # thứ tự gốc cho pháp tuyến +y
    def ab(self, a, b, m): self.mb.boxab(a, b, m)
    # 21.272: số cạnh vật tròn giảm ~½ cho nhẹ bản web (đủ tròn ở khoảng cách nhìn qua kính)
    LOW = True
    def cyl(self, c, r, h, m, seg=12, r2=None, axis='Z'): self.mb.cyl(c, r, h, m, seg=max(5, seg // 2 + (1 if r > 0.15 else 0)) if self.LOW else seg, r2=r2, axis=axis)
    def rod(self, a, b, r, m, seg=6): self.mb.rod(a, b, r, m, seg=max(3, min(seg, 5)) if self.LOW else seg)
    def sph(self, c, r, m, seg=10, rings=6, scale=(1, 1, 1)):
        if self.LOW: seg, rings = max(5, seg * 3 // 5), max(3, rings - 2)
        self.mb.sphere(c, r, m, seg=seg, rings=rings, scale=scale)
    def text(self, body, loc, size, m, rot=(math.pi / 2, 0, math.pi / 2), align='CENTER', extrude=0.004):
        return self.L.text(self.col, body, loc, size, m, rot=rot, align=align, extrude=extrude)

# ------------------------------------------------------------------------------------------------ vỏ phòng
STYLE = {   # (tường, chân tường, gạch sàn 1, gạch sàn 2, kiểu đèn)
    'PhoBo': ('#E9E2CF', '#7E2A1E', '#C9C2B3', '#B5AD9C', 'warm'), 'ComTam': ('#EFE3C8', '#8A2B2B', '#CBBFA6', '#B9AC92', 'warm'),
    'BanhMi': ('#F3E6C4', '#B9822A', '#D8CDB6', '#C6B99E', 'warm'), 'CaPhe': ('#5A4636', '#2E2018', '#7E6A58', '#6E5A48', 'warm'),
    'TraSua': ('#F6E3E8', '#7DB7C9', '#F2F2F2', '#E3E3E6', 'cool'), 'TapHoa': ('#E8E6D8', '#3E6E3A', '#B9B6A8', '#A8A596', 'cool'),
    'NhaThuoc': ('#F4F7F5', '#1F7A4A', '#ECEEEC', '#DCE0DE', 'cool'), 'SuaXe': ('#9DA3A0', '#3B3F42', '#6E6F6A', '#626359', 'cool'),
    'HuTieu': ('#EFE6CF', '#9A3A22', '#CFC5AE', '#BDB29A', 'warm'), 'TiemVang': ('#7A1416', '#C9952A', '#E6DCCB', '#D3C5AE', 'warm'),
    'DienThoai': ('#F2F5F8', '#1F5FA8', '#E8EBEF', '#D9DEE4', 'cool'), 'BunBoHue': ('#EADFC8', '#7E2A1E', '#C8BEA8', '#B5AA93', 'warm'),
    'ThoiTrang': ('#EDE6EE', '#5B3A7A', '#E9E4DE', '#DAD3CB', 'warm'), 'BachHoa': ('#F1F4EE', '#2B8A3E', '#E6E8E2', '#D5D9D0', 'cool'),
    'NhaO_Salon': ('#EADCC0', '#6B4A2C', '#C7A98A', '#B8987A', 'warm'), 'NhaO_XeMay': ('#DCE3E6', '#4E5B63', '#BFC6C2', '#AEB6B1', 'cool'),
    'NhaO_PhongKhach': ('#E6E0D2', '#5E4632', '#D2C6B2', '#C2B49E', 'warm'), 'NhaO_ViVong': ('#E3D7B6', '#6B5A3A', '#B9A07A', '#A88F6A', 'warm'),
}

def _mix(a, b, t):
    a = a.lstrip('#'); b = b.lstrip('#')
    ca = [int(a[i:i + 2], 16) for i in (0, 2, 4)]; cb = [int(b[i:i + 2], 16) for i in (0, 2, 4)]
    return '#%02X%02X%02X' % tuple(int(ca[k] + (cb[k] - ca[k]) * t) for k in range(3))

def food_frames(X, y_wall, side, x0, n, cols):
    """Tranh ảnh món ăn đóng khung trên tường: khung gỗ, nền màu, tô tròn + món."""
    for k in range(n):
        x = x0 - k * 1.1; yb = y_wall - side * 0.015
        X.box((x, yb, 2.2), (0.8, 0.02, 0.6), X.woodD)
        X.box((x, yb - side * 0.012, 2.2), (0.72, 0.006, 0.52), X.c('NenAnh_' + cols[k % len(cols)], cols[k % len(cols)]))
        X.cyl((x, yb - side * 0.016, 2.18), 0.18, 0.006, X.white, seg=16, axis='Y')
        X.cyl((x, yb - side * 0.02, 2.19), 0.14, 0.006, X.c('MonTrongAnh_%d' % k, ['#B5652A', '#E8C53A', '#C8262A', '#8A3A18'][k % 4]), seg=14, axis='Y')
        X.box((x + 0.05, yb - side * 0.024, 2.22), (0.12, 0.004, 0.04), X.c('RauTrongAnh', '#4F9A3A'))

def wall_shelf(X, x0, x1, y_wall, side, z, items):
    """Kệ gỗ treo tường + đồ bày (tô xếp, chai gia vị, hũ)."""
    X.ab((x0, min(y_wall, y_wall - side * 0.3), z - 0.03), (x1, max(y_wall, y_wall - side * 0.3), z), X.wood)
    for xx in (x0 + 0.1, x1 - 0.1): X.box((xx, y_wall - side * 0.12, z - 0.12), (0.03, 0.22, 0.18), X.woodD)
    xx = x0 + 0.12; k = 0
    while xx < x1 - 0.1:
        it = items[k % len(items)]
        if it == 'bowls': bowls_stack(X, xx + 0.05, y_wall - side * 0.15, z, n=5); xx += 0.22
        elif it == 'bottle':
            X.cyl((xx, y_wall - side * 0.15, z), 0.035, 0.22, X.c('ChaiGiaVi_%d' % (k % 3), ['#D23A1E', '#3A2416', '#E5A321'][k % 3]), seg=8)
            X.cyl((xx, y_wall - side * 0.15, z + 0.22), 0.015, 0.05, X.white, seg=6); xx += 0.1
        else:
            X.cyl((xx + 0.03, y_wall - side * 0.15, z), 0.07, 0.16, X.c('HuThuyTinh', '#D9E8EC', rough=0.05, alpha=0.5), seg=10)
            X.cyl((xx + 0.03, y_wall - side * 0.15, z + 0.01), 0.06, 0.1, X.c('TrongHu_%d' % (k % 3), ['#C98A5A', '#E8C53A', '#7DB33A'][k % 3]), seg=10); xx += 0.18
        k += 1

def shell(X, kind):
    """Sàn gạch ô 0,6 m hai màu, tường hai bên + tường hậu có cửa ra sau, chân tường, trần + đèn."""
    wall, skirt, t1, t2, light = STYLE[kind]
    c = X.c; mWall = c('Tuong_' + kind, wall, rough=0.85); mSk = c('ChanTuong_' + kind, skirt, rough=0.6)
    m1 = c('Gach1_' + kind, t1, rough=0.35); m2 = c('Gach2_' + kind, t2, rough=0.35)
    # sàn
    nx = int((FRONT - BACK) / 0.6) + 1; ny = int(X.inner / 0.6) + 1
    for i in range(nx):
        for j in range(ny):
            x0 = BACK + i * 0.6; y0 = X.yl + j * 0.6
            x1 = min(x0 + 0.6, FRONT + 0.2); y1 = min(y0 + 0.6, X.yr)
            if x1 <= x0 or y1 <= y0: continue
            X.floor_quad(x0 + 0.004, y0 + 0.004, x1 - 0.004, y1 - 0.004, FLOOR, m1 if (i + j) % 2 == 0 else m2)
    X.floor_quad(BACK, X.yl, FRONT + 0.2, X.yr, FLOOR - 0.003, c('VuaGach', '#7E7A70'))
    # tường bên + chân tường
    for y0, y1 in ((X.yl - 0.04, X.yl), (X.yr, X.yr + 0.04)):
        X.ab((BACK, y0, FLOOR), (FRONT + 0.2, y1, CEIL), mWall)
    for y0, y1 in ((X.yl, X.yl + 0.015), (X.yr - 0.015, X.yr)):
        X.ab((BACK, y0, FLOOR), (FRONT + 0.2, y1, FLOOR + 0.12), mSk)
    # ốp gạch men chân tường cao 1,1 m (ô 0,3 m hai sắc) + nẹp viền trên — đúng kiểu quán/nhà phố Sài Gòn
    if kind not in ('CaPhe', 'TiemVang', 'SuaXe'):
        wt1 = c('OpGach1_' + kind, _mix(wall, '#FFFFFF', 0.55), rough=0.25); wt2 = c('OpGach2_' + kind, _mix(wall, '#B8B8B0', 0.35), rough=0.25)
        for side, yw in ((-1, X.yl), (1, X.yr)):
            for i in range(int((FRONT + 0.2 - BACK) / 0.3)):
                for j in range(3):
                    x0 = BACK + i * 0.3; z0 = FLOOR + 0.13 + j * 0.3
                    X.wall_quad(x0 + 0.005, x0 + 0.295, yw - side * 0.006, z0 + 0.005, z0 + 0.295, side, wt1 if (i + j) % 2 == 0 else wt2)
            X.box(((BACK + FRONT + 0.2) / 2, yw - side * 0.02, FLOOR + 1.06), (FRONT + 0.2 - BACK, 0.03, 0.05), mSk)
    # tường hậu có ô cửa (tối) + khung cửa gỗ
    dw = min(1.0, X.inner * 0.22); dy = X.yr - 0.45 - dw / 2
    X.ab((BACK - 0.04, X.yl, FLOOR), (BACK, dy - dw / 2, CEIL), mWall)
    X.ab((BACK - 0.04, dy + dw / 2, FLOOR), (BACK, X.yr, CEIL), mWall)
    X.ab((BACK - 0.04, dy - dw / 2, FLOOR + 2.15), (BACK, dy + dw / 2, CEIL), mWall)
    X.ab((BACK - 0.5, dy - dw / 2, FLOOR), (BACK - 0.45, dy + dw / 2, FLOOR + 2.15), c('PhongSau', '#1A1714', rough=1))
    for yy in (dy - dw / 2, dy + dw / 2):
        X.box((BACK + 0.01, yy, FLOOR + 1.08), (0.06, 0.07, 2.16), X.woodD)
    X.box((BACK + 0.01, dy, FLOOR + 2.18), (0.06, dw + 0.14, 0.07), X.woodD)
    X.ab((BACK + 0.0, X.yl, FLOOR), (BACK + 0.015, dy - dw / 2, FLOOR + 0.12), mSk)
    # trần + đèn
    X.ab((BACK, X.yl, CEIL), (FRONT + 0.2, X.yr, CEIL + 0.04), c('Tran', '#F4F2EC', rough=0.9))
    lm = X.lamp if light == 'warm' else X.lampCool
    for xx in (-1.8, -4.4, -7.0):
        if X.inner > 4.2:
            for yy in (X.yl + X.inner * 0.3, X.yl + X.inner * 0.7):
                X.box((xx, yy, CEIL - 0.015), (0.6, 0.6, 0.02), lm)
        else:
            X.box((xx, (X.yl + X.yr) / 2, CEIL - 0.015), (1.2, 0.3, 0.02), lm)
    return mWall

# ------------------------------------------------------------------------------------------------ đồ dùng chung
def stool(X, x, y, m, h=0.45):
    """Ghế nhựa đỏ/xanh thấp kiểu vỉa hè Sài Gòn: mặt vuông có lỗ, 4 chân loe."""
    X.box((x, y, FLOOR + h), (0.3, 0.3, 0.03), m)
    X.box((x, y, FLOOR + h - 0.05), (0.27, 0.27, 0.07), m)
    for sx in (-1, 1):
        for sy in (-1, 1):
            X.rod((x + sx * 0.11, y + sy * 0.11, FLOOR + h - 0.06), (x + sx * 0.15, y + sy * 0.15, FLOOR), 0.018, m, seg=5)
    X.box((x, y, FLOOR + 0.12), (0.27, 0.27, 0.025), m)

def chair(X, x, y, yaw, m, back=True):
    """Ghế tựa (gỗ/nhựa) mặt ghế + 4 chân + lưng."""
    ca, sa = math.cos(yaw), math.sin(yaw)
    R = lambda dx, dy: (x + dx * ca - dy * sa, y + dx * sa + dy * ca)
    X.box((x, y, FLOOR + 0.45), (0.42, 0.42, 0.04), m, rz=yaw)
    for sx in (-1, 1):
        for sy in (-1, 1):
            px, py = R(sx * 0.18, sy * 0.18); X.box((px, py, FLOOR + 0.22), (0.035, 0.035, 0.45), m, rz=yaw)
    if back:
        px, py = R(-0.19, 0)
        X.box((px, py, FLOOR + 0.72), (0.03, 0.42, 0.42), m, rz=yaw)
        for sy in (-1, 1):
            qx, qy = R(-0.19, sy * 0.18); X.box((qx, qy, FLOOR + 0.66), (0.035, 0.035, 0.42), m, rz=yaw)

def condiments(X, x, y, rnd):
    """Bộ gia vị trên bàn quán: ống đũa, tương ớt đỏ, tương đen, hộp khăn giấy, chén chanh ớt."""
    z = FLOOR + 0.76
    X.cyl((x, y - 0.12, z), 0.04, 0.16, X.c('OngDua', '#3E6E9A'))
    for k in range(5): X.rod((x - 0.015 + k * 0.007, y - 0.12, z + 0.1), (x - 0.02 + k * 0.01, y - 0.12, z + 0.26), 0.004, X.c('Dua', '#E8D8B0'), seg=4)
    X.cyl((x + 0.06, y, z), 0.03, 0.17, X.c('TuongOt', '#D23A1E')); X.cyl((x + 0.06, y, z + 0.17), 0.012, 0.04, X.c('NapTrang', '#F5F5F0'))
    X.cyl((x - 0.03, y + 0.02, z), 0.03, 0.17, X.c('TuongDen', '#3A2416')); X.cyl((x - 0.03, y + 0.02, z + 0.17), 0.012, 0.04, X.c('NapDo', '#C8262A'))
    X.box((x, y + 0.14, z + 0.045), (0.12, 0.2, 0.09), X.c('HopKhan', '#E8E4DA'))
    X.box((x, y + 0.14, z + 0.095), (0.04, 0.07, 0.012), X.white)
    X.cyl((x + 0.1, y + 0.13, z), 0.05, 0.03, X.white, r2=0.06)
    for k in range(3): X.sph((x + 0.09 + (k - 1) * 0.025, y + 0.13, z + 0.04), 0.017, X.c('Chanh', '#7DB33A'), seg=6, rings=4)

def steel_table(X, x, y, ly=1.0, lx=0.6, h=0.72, top=None):
    top = top or X.steel
    X.box((x, y, FLOOR + h), (lx, ly, 0.03), top)
    X.box((x, y, FLOOR + h - 0.05), (lx - 0.06, ly - 0.06, 0.06), X.steelD)
    for sx in (-1, 1):
        for sy in (-1, 1): X.box((x + sx * (lx / 2 - 0.05), y + sy * (ly / 2 - 0.05), FLOOR + h / 2), (0.035, 0.035, h), X.steelD)
    X.box((x, y, FLOOR + 0.18), (lx - 0.1, ly - 0.1, 0.015), X.steelD)

def wall_fan(X, x, y_wall, z, side):
    """Quạt treo tường (lồng sắt + cánh) — side = +1 tường phải, −1 tường trái."""
    yb = y_wall - side * 0.06
    X.box((x, yb, z), (0.12, 0.1, 0.16), X.white)
    X.rod((x, yb, z), (x, yb - side * 0.18, z - 0.1), 0.02, X.white)
    c = (x, yb - side * 0.26, z - 0.14)
    X.cyl(c, 0.2, 0.08, X.c('LongQuat', '#4A6E8A', rough=0.4), seg=16, axis='Y')
    for k in range(3):
        a = k * 2 * math.pi / 3
        X.box((c[0] + math.cos(a) * 0.09, c[1], c[2] + math.sin(a) * 0.09), (0.15, 0.02, 0.07), X.c('CanhQuat', '#7FB3D5'), rz=0, ry=-a)

def ceiling_fan(X, x, y):
    X.rod((x, y, CEIL), (x, y, CEIL - 0.45), 0.02, X.white)
    X.cyl((x, y, CEIL - 0.55), 0.12, 0.12, X.c('MoQuat', '#D8D2C4'))
    for k in range(3):
        a = k * 2 * math.pi / 3 + 0.3
        X.box((x + math.cos(a) * 0.45, y + math.sin(a) * 0.45, CEIL - 0.5), (0.7, 0.13, 0.015), X.woodL, rz=a)

def ac_unit(X, y_wall, side, x=-4.0):
    yb = y_wall - side * 0.13
    X.box((x, yb, CEIL - 0.55), (0.95, 0.24, 0.3), X.white)
    X.box((x, yb - side * 0.122, CEIL - 0.64), (0.85, 0.01, 0.06), X.c('KheMayLanh', '#9AA3A8'))
    X.box((x + 0.36, yb - side * 0.122, CEIL - 0.5), (0.06, 0.01, 0.02), X.c('DenXanhMay', '#3BE07A', emit=3))

def clock(X, x, y_wall, side, z=3.2):
    X.cyl((x, y_wall - side * 0.02, z), 0.18, 0.03, X.c('VienDongHo', '#2B2B2B'), seg=20, axis='Y')
    X.cyl((x, y_wall - side * 0.045, z), 0.16, 0.005, X.white, seg=20, axis='Y')
    X.box((x, y_wall - side * 0.05, z + 0.05), (0.015, 0.004, 0.1), X.black); X.box((x + 0.04, y_wall - side * 0.05, z), (0.08, 0.004, 0.012), X.black)

def calendar(X, x, y_wall, side, z=2.3):
    X.box((x, y_wall - side * 0.01, z), (0.42, 0.012, 0.6), X.c('LichDo', '#C8262A'))
    X.box((x, y_wall - side * 0.018, z - 0.08), (0.34, 0.006, 0.36), X.white)
    X.box((x, y_wall - side * 0.022, z - 0.08), (0.18, 0.004, 0.16), X.c('SoLich', '#C8262A'))

def altar_than_tai(X, x, y, face_y):
    """Bàn thờ Thần Tài – Ông Địa: tủ gỗ đỏ son thấp sát chân tường, tượng, đèn đỏ, nải chuối, lư hương."""
    d = -face_y   # mở về phía giữa phòng
    X.ab((x - 0.32, y - 0.25, FLOOR), (x + 0.32, y + 0.25, FLOOR + 0.06), X.woodD)
    X.ab((x - 0.3, y - 0.23, FLOOR + 0.06), (x + 0.3, y + 0.23, FLOOR + 0.62), X.c('SonDo', '#9E1B1B', rough=0.4))
    X.ab((x - 0.33, y - 0.26, FLOOR + 0.62), (x + 0.33, y + 0.26, FLOOR + 0.66), X.gold)
    X.ab((x - 0.3, y - 0.23, FLOOR + 0.66), (x + 0.3, y - 0.2, FLOOR + 0.95), X.c('SonDo', '#9E1B1B'))
    X.ab((x - 0.3, y + 0.2, FLOOR + 0.66), (x + 0.3, y + 0.23, FLOOR + 0.95), X.c('SonDo', '#9E1B1B'))
    X.ab((x - 0.33, y - 0.26, FLOOR + 0.95), (x + 0.33, y + 0.26, FLOOR + 1.0), X.gold)
    for k, col in enumerate(('#E2B13C', '#3A6E3A')):
        cx = x + (k - 0.5) * 0.26
        X.cyl((cx, y + d * 0.05, FLOOR + 0.66), 0.06, 0.12, X.c('ThanTaiAo%d' % k, col))
        X.sph((cx, y + d * 0.05, FLOOR + 0.84), 0.06, X.c('MatTuong', '#F0C9A0'), seg=8, rings=5)
    X.cyl((x, y + d * 0.15, FLOOR + 0.66), 0.05, 0.05, X.gold)
    for k in range(3): X.rod((x - 0.01 + k * 0.01, y + d * 0.15, FLOOR + 0.7), (x - 0.02 + k * 0.02, y + d * 0.15, FLOOR + 0.9), 0.003, X.red, seg=3)
    for sx in (-1, 1): X.cyl((x + sx * 0.24, y + d * 0.12, FLOOR + 0.66), 0.025, 0.12, X.redLamp)
    for k in range(4): X.sph((x + 0.12 + k * 0.03, y + d * 0.14, FLOOR + 0.69), 0.025, X.c('Chuoi', '#E8C53A'), seg=6, rings=4, scale=(0.6, 0.6, 1.4))

def plant(X, x, y, s=1.0, pot='#A0522D'):
    X.cyl((x, y, FLOOR), 0.18 * s, 0.34 * s, X.c('ChauCay_' + pot, pot), r2=0.22 * s)
    X.cyl((x, y, FLOOR + 0.33 * s), 0.19 * s, 0.02, X.c('DatCay', '#3B2A1E'))
    lc = [X.c('La1', '#2F6B2A'), X.c('La2', '#4E8F35'), X.c('La3', '#3E7A30')]
    rnd = random.Random(int(x * 100 + y * 37))
    for k in range(9):
        a = k * 2.4; r = 0.08 + 0.05 * (k % 3); z = FLOOR + (0.55 + 0.12 * (k % 4)) * s
        X.sph((x + math.cos(a) * r * s, y + math.sin(a) * r * s, z), 0.13 * s, lc[k % 3], seg=7, rings=5, scale=(1.0, 0.55, 1.3))
    X.rod((x, y, FLOOR + 0.3 * s), (x, y, FLOOR + 0.6 * s), 0.02 * s, X.c('ThanCay', '#5A3E22'))

def menu_board(X, x, y_wall, side, title, lines, bg='#2B2B2B', fg='#F7F1DE', acc='#F2C14E', w=1.4, h=0.95, z=2.75):
    """Bảng thực đơn treo tường: nền + viền + chữ (tên món, giá)."""
    yb = y_wall - side * 0.03
    X.box((x, yb, z), (w + 0.06, 0.03, h + 0.06), X.c('VienBang', '#1A1A1A'))
    X.box((x, yb - side * 0.02, z), (w, 0.01, h), X.c('NenBang_' + bg, bg, rough=0.8))
    rot = (math.pi / 2, 0, 0 if side > 0 else math.pi)   # tường phải (+1) mặt quay −y; tường trái quay +y
    yt = yb - side * 0.03
    X.L.text(X.col, title, (x, yt, z + h / 2 - 0.17), min(0.22, h * 0.17), X.c('ChuNhan_' + acc, acc), rot=rot, extrude=0.003)
    for i, ln in enumerate(lines[:5]):
        X.L.text(X.col, ln, (x, yt, z + h / 2 - 0.4 - i * (h - 0.5) / 4.6), min(0.11, h * 0.085), X.c('ChuBang_' + fg, fg), rot=rot, extrude=0.002)

def back_sign(X, title, sub, bg, fg, z=2.95):
    """Bảng tên tiệm trên tường hậu, nhìn thẳng từ ngoài vào (mặt quay +x). Nhà nổi bật dùng tên thật (X.title/X.sub)."""
    title = X.title or title
    if X.sub is not None: sub = X.sub
    wv = min(2.6, X.inner * 0.62); cy = X.yl + X.inner * 0.42
    X.box((BACK + 0.04, cy, z), (0.04, wv, 0.62), X.c('NenTen_' + bg, bg, rough=0.5))
    X.box((BACK + 0.03, cy, z), (0.03, wv + 0.08, 0.7), X.c('VienTen', '#1E1E1E'))
    rot = (math.pi / 2, 0, math.pi / 2)          # mặt chữ quay +x (ra đường)
    X.L.text(X.col, title, (BACK + 0.07, cy, z + 0.06), 0.24, X.c('ChuTen_' + fg, fg), rot=rot, extrude=0.004)
    if sub: X.L.text(X.col, sub, (BACK + 0.07, cy, z - 0.2), 0.09, X.c('ChuTen_' + fg, fg), rot=rot, extrude=0.003)
    return cy

def products_shelf(X, x0, x1, y_wall, side, levels, h, depth, rnd, palette, kind='box', frame=None):
    """Kệ dọc tường bên: khung + tầng + hàng hoá xếp sát (hộp/chai/lon/túi) màu theo bảng."""
    frame = frame or X.c('KeTrang', '#E9E9E4', rough=0.5)
    yi = y_wall - side * depth
    for xx in (x0, x1):
        X.ab((xx - 0.02, min(y_wall, yi), FLOOR), (xx + 0.02, max(y_wall, yi), FLOOR + h), frame)
    X.ab((x0, min(y_wall, y_wall - side * 0.02), FLOOR), (x1, max(y_wall, y_wall - side * 0.02), FLOOR + h), frame)
    mats = [X.c('Hang_' + p, p, rough=0.45) for p in palette]
    for lv in range(levels):
        z = FLOOR + 0.12 + lv * (h - 0.15) / levels
        X.ab((x0, min(y_wall, yi), z - 0.02), (x1, max(y_wall, yi), z), frame)
        xx = x0 + 0.04; gap = (h - 0.15) / levels - 0.06
        while xx < x1 - 0.06:
            m = mats[rnd.randrange(len(mats))]
            k = kind if kind != 'mix' else rnd.choice(['box', 'bottle', 'can', 'bag'])
            if k == 'box':
                bw = rnd.uniform(0.08, 0.2); bh = min(gap, rnd.uniform(0.12, 0.3))
                X.box((xx + bw / 2, y_wall - side * depth * 0.5, z + bh / 2), (bw - 0.01, depth * 0.85, bh), m)
                X.box((xx + bw / 2, y_wall - side * (depth * 0.93), z + bh * 0.55), (bw * 0.6, 0.004, bh * 0.35), X.white)
                xx += bw
            elif k == 'bottle':
                bh = min(gap - 0.02, rnd.uniform(0.22, 0.32))
                for dd in (0.25, 0.6):
                    X.cyl((xx + 0.04, y_wall - side * depth * dd, z), 0.035, bh * 0.7, m, seg=8)
                    X.cyl((xx + 0.04, y_wall - side * depth * dd, z + bh * 0.7), 0.035, bh * 0.25, m, seg=8, r2=0.013)
                X.cyl((xx + 0.04, y_wall - side * depth * 0.6, z + bh * 0.95), 0.015, 0.03, X.white, seg=6)
                xx += 0.085
            elif k == 'can':
                for dd in (0.25, 0.6):
                    for st in range(2): X.cyl((xx + 0.035, y_wall - side * depth * dd, z + st * 0.125), 0.032, 0.12, m, seg=8)
                xx += 0.072
            else:
                bw = rnd.uniform(0.16, 0.26); bh = min(gap, rnd.uniform(0.2, 0.3))
                X.box((xx + bw / 2, y_wall - side * depth * 0.45, z + bh / 2), (bw - 0.015, depth * 0.6, bh), m, rx=0.08 * side)
                xx += bw

def glass_cabinet(X, x, y, lx, ly, h, top_items=None, frame=None):
    """Tủ kính trưng bày: chân/khung kim loại, 3 mặt kính, kệ kính trong."""
    frame = frame or X.steel
    X.ab((x - lx / 2, y - ly / 2, FLOOR), (x + lx / 2, y + ly / 2, FLOOR + 0.45), frame)
    for sx in (-1, 1):
        for sy in (-1, 1):
            X.box((x + sx * lx / 2, y + sy * ly / 2, FLOOR + 0.45 + h / 2), (0.025, 0.025, h), frame)
    X.ab((x - lx / 2, y - ly / 2, FLOOR + 0.45 + h), (x + lx / 2, y + ly / 2, FLOOR + 0.48 + h), frame)
    X.ab((x + lx / 2 - 0.01, y - ly / 2, FLOOR + 0.45), (x + lx / 2, y + ly / 2, FLOOR + 0.45 + h), X.glass)
    for sy in (-1, 1): X.ab((x - lx / 2, y + sy * ly / 2 - 0.005, FLOOR + 0.45), (x + lx / 2, y + sy * ly / 2 + 0.005, FLOOR + 0.45 + h), X.glass)
    X.ab((x - lx / 2, y - ly / 2, FLOOR + 0.45 + h * 0.5), (x + lx / 2, y + ly / 2, FLOOR + 0.46 + h * 0.5), X.glass)
    if top_items: top_items(FLOOR + 0.46, FLOOR + 0.47 + h * 0.5)

def stock_pot(X, x, y, z, r=0.22, h=0.4, broth='#B5652A'):
    X.cyl((x, y, z), r, h, X.steel, seg=16)
    X.cyl((x, y, z + h - 0.03), r - 0.01, 0.02, X.c('NuocLeo_' + broth, broth, rough=0.15), seg=16)
    for sx in (-1, 1): X.box((x, y + sx * (r + 0.03), z + h - 0.06), (0.08, 0.05, 0.03), X.steelD)
    X.rod((x + r * 0.4, y, z + h), (x + r * 0.9, y + r * 0.3, z + h + 0.3), 0.012, X.steelD)

def gas_stove(X, x, y, z):
    X.box((x, y, z + 0.06), (0.5, 0.35, 0.12), X.c('BepGa', '#2E3236', metal=0.4))
    for sy in (-0.08, 0.08): X.cyl((x, y + sy, z + 0.12), 0.07, 0.015, X.black)
    X.cyl((x - 0.5, y, z - 0.45), 0.13, 0.42, X.c('BinhGa', '#2A6EB5'))

def bowls_stack(X, x, y, z, n=6, col='#F4F2EA'):
    for k in range(n): X.cyl((x, y, z + k * 0.03), 0.07, 0.035, X.c('To_' + col, col), seg=12, r2=0.09)

def noodle_cart(X, x, y, ly, color, title, rnd, items='noodle'):
    """Xe đẩy/tủ kính bán món nước kiểu Sài Gòn đặt sát kính mặt tiền: thân inox + mặt kính có chữ, nồi nước lèo bên cạnh."""
    body = X.c('XeDay_' + color, color, rough=0.4)
    X.ab((x - 0.35, y - ly / 2, FLOOR + 0.1), (x + 0.35, y + ly / 2, FLOOR + 0.85), X.steel)
    X.ab((x + 0.35, y - ly / 2, FLOOR + 0.12), (x + 0.36, y + ly / 2, FLOOR + 0.8), body)
    for sy in (-1, 1):
        X.cyl((x + 0.2, y + sy * (ly / 2 - 0.12), FLOOR + 0.08), 0.08, 0.05, X.black, axis='Y', seg=10)
    def top(z0, z1):
        if items == 'noodle':
            for k in range(int(ly / 0.22)):
                yy = y - ly / 2 + 0.15 + k * 0.22
                X.box((x, yy, z0 + 0.04), (0.3, 0.18, 0.08), X.c('BanhPho', '#F3EEDF', rough=0.7))
                X.box((x + 0.1, yy, z1 + 0.05), (0.18, 0.18, 0.1), X.c('RauThom', '#4F9A3A', rough=0.8))
            for k in range(3): X.box((x - 0.12, y - ly / 2 + 0.3 + k * 0.32, z1 + 0.04), (0.2, 0.26, 0.06), X.c('ThitBo', '#9A3B2E', rough=0.7))
        elif items == 'banhmi':
            for k in range(int(ly / 0.12)):
                yy = y - ly / 2 + 0.08 + k * 0.12
                X.sph((x - 0.02, yy, z0 + 0.06), 0.05, X.c('VoBanh', '#D9973F', rough=0.6), seg=8, rings=5, scale=(3.4, 1.0, 0.9))
                X.sph((x + 0.02, yy + 0.05, z1 + 0.06), 0.05, X.c('VoBanh', '#D9973F'), seg=8, rings=5, scale=(3.4, 1.0, 0.9))
        elif items == 'comtam':
            for k in range(int(ly / 0.3)):
                yy = y - ly / 2 + 0.2 + k * 0.3
                X.box((x, yy, z0 + 0.03), (0.32, 0.26, 0.05), X.steel)
                for j in range(3): X.box((x - 0.08 + j * 0.08, yy, z0 + 0.07), (0.07, 0.2, 0.025), X.c('SuonNuong', '#8A3A18', rough=0.6))
                X.box((x, yy, z1 + 0.04), (0.3, 0.24, 0.06), X.c('ChaTrung', '#E5B84A'))
    glass_cabinet(X, x, y, 0.62, ly - 0.05, 0.7, top_items=top)
    # chữ trên mặt kính hướng ra đường
    X.L.text(X.col, title, (x + 0.37, y, FLOOR + 1.0), min(0.16, ly / max(4, len(title)) * 1.5), X.c('ChuXe', '#C8262A'), rot=(math.pi / 2, 0, math.pi / 2), extrude=0.002)

def motorbike(X, x, y, yaw, color, rnd=None, z0=0.0):
    """Xe tay ga kiểu Vision/Air Blade dựng chân chống (dài 1,85 m): lốp + mâm + đùm, phuộc trước, sàn để chân, thân sau
    bo tròn, yên, ốp đầu + đèn pha + xi-nhan, tay lái + tay nắm + gương, đèn hậu, biển số, ống xả, chân chống."""
    ca, sa = math.cos(yaw), math.sin(yaw)
    def P(dx, dy, dz): return (x + dx * ca - dy * sa, y + dx * sa + dy * ca, FLOOR + z0 + dz)
    body = X.c('ThanXe_' + color, color, rough=0.25, metal=0.25)
    tire = X.c('LopXe', '#151515', rough=0.9); rim = X.c('MamXe', '#A7ADB3', metal=0.75, rough=0.3)
    black = X.c('NhuaDenXe', '#1C1D1F', rough=0.55)
    def wheel(dx):
        # bánh: lốp (vòng ngoài) + mặt lốp hai bên + mâm 5 chấu + đùm
        c = P(dx, 0, 0.27)
        X.mb.cyl((c[0] - 0.05 * -sa, c[1] - 0.05 * ca, c[2]), 0.27, 0.1, tire, seg=20, axis='Y') if abs(sa) < 0.5 else X.mb.cyl((c[0] - 0.05 * ca, c[1] - 0.05 * sa, c[2]), 0.27, 0.1, tire, seg=20, axis='X')
        for k in range(5):
            a = k * 2 * math.pi / 5
            p0 = P(dx, 0, 0.27); p1 = P(dx + math.cos(a) * 0.16, 0, 0.27 + math.sin(a) * 0.16)
            for side in (-0.052, 0.052):
                X.rod((p0[0] - side * sa, p0[1] + side * ca, p0[2]), (p1[0] - side * sa, p1[1] + side * ca, p1[2]), 0.018, rim, seg=4)
        for side in (-0.053, 0.053):
            cc = P(dx, side, 0.27)
            X.sph(cc, 0.17, rim, seg=14, rings=3, scale=(1, 0.05, 1)) if abs(sa) < 0.5 else X.sph(cc, 0.17, rim, seg=14, rings=3, scale=(0.05, 1, 1))
            X.sph(cc, 0.05, X.steelD, seg=8, rings=4, scale=(1, 0.6, 1))
    wheel(-0.64); wheel(0.66)
    # phuộc trước + chắn bùn trước
    for side in (-0.07, 0.07): X.rod(P(0.66, side, 0.27), P(0.52, side, 0.82), 0.022, X.steelD)
    X.sph(P(0.68, 0, 0.5), 0.3, body, seg=12, rings=6, scale=(0.75, 0.32, 0.18))
    # sàn để chân + yếm trước
    X.box(P(0.06, 0, 0.3), (0.62, 0.34, 0.05), black, rz=yaw)
    X.sph(P(0.42, 0, 0.62), 0.4, body, seg=14, rings=8, scale=(0.32, 0.5, 1.0))
    X.sph(P(0.36, 0, 0.6), 0.36, black, seg=12, rings=6, scale=(0.3, 0.42, 0.9))
    # thân sau bo + yên + tay dắt + đèn hậu + biển số
    X.sph(P(-0.42, 0, 0.6), 0.5, body, seg=16, rings=8, scale=(1.0, 0.38, 0.42))
    X.sph(P(-0.4, 0, 0.82), 0.44, X.c('YenXe', '#2A2420', rough=0.8), seg=14, rings=6, scale=(1.0, 0.36, 0.17))
    X.rod(P(-0.82, -0.14, 0.86), P(-0.82, 0.14, 0.86), 0.018, black)
    X.sph(P(-0.9, 0, 0.66), 0.09, X.c('DenHau', '#D0262A', emit=0.6), seg=10, rings=5, scale=(0.5, 1.4, 0.6))
    X.box(P(-0.93, 0, 0.42), (0.02, 0.19, 0.13), X.white, rz=yaw)
    X.box(P(-0.94, 0, 0.42), (0.005, 0.15, 0.04), X.c('SoBien', '#1E1E1E'), rz=yaw)
    X.rod(P(-0.3, 0.17, 0.33), P(-0.86, 0.19, 0.38), 0.045, X.c('OngXa', '#3A3C3E', metal=0.6, rough=0.35))
    # ốp đầu, đèn pha, xi-nhan, tay lái, gương
    X.sph(P(0.52, 0, 1.02), 0.22, body, seg=12, rings=6, scale=(0.75, 1.15, 0.6))
    X.sph(P(0.64, 0, 1.0), 0.09, X.c('DenPha', '#F7F3DE', emit=0.4), seg=10, rings=5, scale=(0.5, 1.5, 0.7))
    for side in (-1, 1):
        X.sph(P(0.6, side * 0.2, 0.98), 0.035, X.c('XiNhan', '#F2A33A', emit=0.4), seg=8, rings=4)
        X.rod(P(0.5, side * 0.14, 1.08), P(0.48, side * 0.36, 1.1), 0.016, black)
        X.cyl(P(0.48, side * 0.36, 1.1), 0.022, 0.12, X.c('TayNam', '#2B2B2B', rough=0.8), seg=8, axis='Y') if abs(sa) < 0.5 else None
        X.rod(P(0.48, side * 0.2, 1.12), P(0.44, side * 0.27, 1.36), 0.007, black, seg=4)
        X.sph(P(0.44, side * 0.28, 1.39), 0.05, black, seg=8, rings=4, scale=(0.4, 1.0, 0.7))
    # chân chống
    X.rod(P(-0.12, 0.1, 0.28), P(-0.16, 0.18, 0.0), 0.014, X.steelD, seg=5)

def hanging_clothes(X, x0, x1, y, z, rnd, palette):
    """Sào treo quần áo: thanh inox + móc + áo (thân + tay) xếp sát."""
    X.rod((x0, y, z), (x1, y, z), 0.015, X.steel)
    for xx in (x0, x1): X.rod((xx, y, z), (xx, y, FLOOR), 0.018, X.steel)
    xx = x0 + 0.08
    while xx < x1 - 0.06:
        m = X.c('Ao_' + palette[rnd.randrange(len(palette))], palette[rnd.randrange(len(palette))], rough=0.85)
        X.box((xx, y, z - 0.38), (0.04, 0.42, 0.62), m)
        X.box((xx, y, z - 0.14), (0.04, 0.6, 0.14), m)
        X.rod((xx, y, z), (xx, y, z - 0.06), 0.004, X.steelD, seg=4)
        xx += 0.07

def mannequin(X, x, y, m):
    X.cyl((x, y, FLOOR), 0.15, 0.02, X.black)
    X.rod((x, y, FLOOR), (x, y, FLOOR + 0.9), 0.015, X.black)
    X.cyl((x, y, FLOOR + 0.85), 0.14, 0.25, m, r2=0.17)
    X.cyl((x, y, FLOOR + 1.1), 0.17, 0.35, m, r2=0.19)
    X.cyl((x, y, FLOOR + 1.45), 0.07, 0.08, X.c('Manocanh', '#E8E2D8'))
    X.sph((x, y, FLOOR + 1.62), 0.1, X.c('Manocanh', '#E8E2D8'), scale=(0.9, 0.9, 1.15))
    for sy in (-1, 1): X.rod((x, y + sy * 0.2, FLOOR + 1.42), (x + 0.05, y + sy * 0.24, FLOOR + 1.0), 0.04, m)

def counter(X, x, y, ly, lx=0.6, h=1.0, m=None, top=None):
    m = m or X.wood; top = top or X.c('MatDa', '#3A3A3C', rough=0.25)
    X.ab((x - lx / 2, y - ly / 2, FLOOR), (x + lx / 2, y + ly / 2, FLOOR + h), m)
    X.ab((x - lx / 2 - 0.03, y - ly / 2 - 0.03, FLOOR + h), (x + lx / 2 + 0.03, y + ly / 2 + 0.03, FLOOR + h + 0.04), top)
    X.ab((x + lx / 2, y - ly / 2, FLOOR), (x + lx / 2 + 0.01, y + ly / 2, FLOOR + 0.08), X.black)

def register(X, x, y):
    z = FLOOR + 1.04
    X.box((x, y, z + 0.06), (0.3, 0.32, 0.12), X.black)
    X.box((x + 0.02, y, z + 0.22), (0.04, 0.26, 0.2), X.black, ry=-0.3)
    X.box((x + 0.045, y, z + 0.22), (0.005, 0.22, 0.15), X.c('ManHinh', '#5FB8E8', emit=1.5), ry=-0.3)

def fridge(X, x, y_wall, side, w=0.7, h=1.9, brand='#C8262A', rnd=None, drinks=None):
    """Tủ mát cửa kính có đèn trong, 5 tầng chai/lon nhiều màu, đầu tủ in hiệu."""
    rnd = rnd or random.Random(1)
    d = 0.65; yi = y_wall - side * d
    X.ab((x - w / 2, min(y_wall, yi), FLOOR), (x + w / 2, max(y_wall, yi), FLOOR + h), X.white)
    X.ab((x - w / 2 + 0.04, min(y_wall, yi) + 0.02, FLOOR + 0.12), (x + w / 2 - 0.04, max(y_wall, yi) - 0.02, FLOOR + h - 0.25), X.c('TrongTuMat', '#EEF6FA', emit=1.2))
    X.ab((x - w / 2, yi - side * 0.005, FLOOR + h - 0.22), (x + w / 2, yi + side * 0.005, FLOOR + h), X.c('HieuTu_' + brand, brand, emit=0.6))
    drinks = drinks or ['#C8262A', '#2E7D32', '#F2A900', '#1565C0', '#E65100', '#6A1B9A', '#F5F5F5']
    for lv in range(5):
        z = FLOOR + 0.15 + lv * 0.3
        X.ab((x - w / 2 + 0.04, min(y_wall, yi) + 0.03, z - 0.012), (x + w / 2 - 0.04, max(y_wall, yi) - 0.03, z), X.glass)
        for k in range(int((w - 0.1) / 0.075)):
            m = X.c('Nuoc_' + drinks[(lv * 3 + k) % len(drinks)], drinks[(lv * 3 + k) % len(drinks)], rough=0.2)
            X.cyl((x - w / 2 + 0.09 + k * 0.075, yi + side * 0.12, z), 0.03, 0.2 if lv % 2 else 0.12, m, seg=8)
    X.ab((x - w / 2, yi - side * 0.01, FLOOR + 0.1), (x + w / 2, yi + side * 0.0, FLOOR + h - 0.22), X.glass)
    X.box((x + w / 2 - 0.06, yi - side * 0.03, FLOOR + 1.0), (0.03, 0.03, 0.5), X.steel)

def poster(X, x, y_wall, side, w, h, z, col, accent):
    yb = y_wall - side * 0.008
    X.box((x, yb, z), (w, 0.006, h), X.c('Poster_' + col, col, rough=0.6))
    X.box((x, yb - side * 0.004, z + h * 0.18), (w * 0.8, 0.004, h * 0.3), X.c('Poster_' + accent, accent))

# ------------------------------------------------------------------------------------------------ 14 cửa hàng
def _food_shop(X, rnd, title, menu, cart_col, broth, items='noodle', stool_cols=('#C8262A', '#1F6FB5')):
    """Quán ăn vỉa hè trong nhà: xe đẩy tủ kính sát kính mặt tiền bên trái, nồi nước lèo/bếp, 2–4 bàn inox + ghế nhựa, bảng thực đơn."""
    shell(X, X.kind)
    ly = min(1.6, X.inner * 0.32)
    cy = X.yl + ly / 2 + 0.1
    noodle_cart(X, -1.05, cy, ly, cart_col, title, rnd, items=items)
    steel_table(X, -1.85, cy, ly=ly, lx=0.6, h=0.82)
    if items == 'comtam':
        X.box((-1.85, cy - 0.3, FLOOR + 0.9), (0.45, 0.35, 0.12), X.c('LoNuong', '#3A2C22'))
        for k in range(5): X.rod((-2.05, cy - 0.45 + k * 0.07, FLOOR + 0.97), (-1.65, cy - 0.45 + k * 0.07, FLOOR + 0.97), 0.006, X.steelD, seg=4)
        X.cyl((-1.85, cy + 0.35, FLOOR + 0.85), 0.2, 0.32, X.c('NoiCom', '#E8E6E0'), seg=14)
    else:
        stock_pot(X, -1.85, cy - ly * 0.22, FLOOR + 0.85, broth=broth)
        bowls_stack(X, -1.85, cy + ly * 0.25, FLOOR + 0.85, n=7)
    gas_stove(X, -2.6, cy, FLOOR + 0.0) if items != 'banhmi' else None
    # bàn khách
    xs = [-3.0, -4.45, -5.9]
    cols = [X.yl + X.inner * 0.66] if X.inner < 4.4 else [X.yl + X.inner * 0.32, X.yl + X.inner * 0.74]
    seats = [(tx, yy) for tx in xs for yy in cols if not (yy < X.yl + ly + 0.6 and tx > -3.4)]
    for i, (tx, yy) in enumerate(seats):
        steel_table(X, tx, yy, ly=0.75, lx=1.0, h=0.72)
        condiments(X, tx, yy, rnd)
        for dx in (-0.32, 0.32):
            for sy in (-1, 1):
                if abs(sy) and yy + sy * 0.6 < X.yr - 0.2 and yy + sy * 0.6 > X.yl + 0.2:
                    stool(X, tx + dx, yy + sy * 0.6, X.c('Ghe_' + stool_cols[(i + (dx > 0)) % 2], stool_cols[(i + (dx > 0)) % 2], rough=0.5))
        if i == 0: bowls_stack(X, tx + 0.25, yy - 0.2, FLOOR + 0.74, n=1)
    # tường: thực đơn, quạt, đồng hồ, lịch, máy lạnh, bàn thờ
    menu_board(X, -4.6, X.yr, 1, title.upper(), menu, w=2.1, h=1.25, z=2.75)
    food_frames(X, X.yl, -1, -3.4, 3, ['#F2E2C0', '#E8D2B0', '#F0E6D0'])
    wall_shelf(X, -2.9, -1.3, X.yr, 1, 1.75, ['bowls', 'bottle', 'bottle', 'jar', 'bottle'])
    ceiling_fan(X, -3.6, X.yl + X.inner * 0.5); ceiling_fan(X, -6.0, X.yl + X.inner * 0.5)
    fridge(X, -7.3, X.yr, 1, w=0.7, h=1.85, brand='#C8262A', rnd=rnd)
    X.cyl((-2.4, X.yr - 0.25, FLOOR), 0.17, 0.5, X.c('ThungRac', '#2E7D32'), seg=12)
    wall_fan(X, -3.2, X.yr, 1, 3.3); wall_fan(X, -6.4, X.yl, -1, 3.3)
    clock(X, -6.6, X.yr, 1); calendar(X, -2.6, X.yl, -1)
    altar_than_tai(X, -7.6, X.yl + 0.3, -1)
    X.ab((BACK + 0.02, X.yl + 0.6, FLOOR), (BACK + 0.62, X.yl + 1.6, FLOOR + 0.9), X.steel)       # bàn sơ chế sau
    for k in range(4): X.box((BACK + 0.3, X.yl + 0.7 + k * 0.22, FLOOR + 0.95), (0.25, 0.18, 0.1), X.c('RoNhua', '#2E8B57'))
    back_sign(X, title, 'Quán ngon Sài Gòn', '#7E2A1E' if broth != '#D24A1E' else '#9E1B1B', '#F7E7B8')

def furnish_PhoBo(X, rnd):
    _food_shop(X, rnd, 'Phở Bò', ['Phở tái ........ 50k', 'Phở chín ....... 50k', 'Phở đặc biệt ... 70k', 'Phở gà ......... 45k', 'Trà đá ......... 5k'], '#C8262A', '#B5652A')

def furnish_ComTam(X, rnd):
    _food_shop(X, rnd, 'Cơm Tấm', ['Sườn bì chả ... 55k', 'Sườn nướng .... 45k', 'Sườn trứng .... 50k', 'Canh khổ qua . 15k', 'Trà đá ......... 5k'], '#E5A321', '#B5652A', items='comtam', stool_cols=('#1F6FB5', '#2E8B57'))

def furnish_HuTieu(X, rnd):
    _food_shop(X, rnd, 'Hủ Tiếu', ['Hủ tiếu Nam Vang 55k', 'Hủ tiếu khô .... 50k', 'Hủ tiếu bò kho . 50k', 'Mì xá xíu ...... 45k', 'Sâm lạnh ...... 10k'], '#E8C53A', '#C99A5A', stool_cols=('#C8262A', '#E5A321'))

def furnish_BunBoHue(X, rnd):
    _food_shop(X, rnd, 'Bún Bò Huế', ['Bún bò giò heo . 55k', 'Bún bò đặc biệt  70k', 'Bún bò chả cua . 55k', 'Thêm huyết ..... 10k', 'Trà đá ......... 5k'], '#9E1B1B', '#D24A1E', stool_cols=('#1F6FB5', '#C8262A'))
    for k in range(3):   # đèn lồng Huế
        X.rod((-2.8 - k * 1.6, X.yl + X.inner * 0.5, CEIL), (-2.8 - k * 1.6, X.yl + X.inner * 0.5, CEIL - 0.4), 0.006, X.black, seg=4)
        X.sph((-2.8 - k * 1.6, X.yl + X.inner * 0.5, CEIL - 0.62), 0.2, X.c('DenLong', '#E8402A', emit=1.2), seg=10, rings=6, scale=(1, 1, 1.25))

def furnish_BanhMi(X, rnd):
    shell(X, X.kind)
    ly = min(1.8, X.inner * 0.36); cy = X.yl + ly / 2 + 0.1
    noodle_cart(X, -1.05, cy, ly, '#E0A321', 'Bánh Mì', rnd, items='banhmi')
    counter(X, -1.9, cy, ly, lx=0.6, h=0.9, m=X.c('QuayBanhMi', '#E9D9B5'))
    cols = ['#8A5A3A', '#E8D8C0', '#E58A3A', '#4F9A3A', '#C8262A']
    for k in range(5):   # khay pa-tê, chả lụa, dưa chua, rau, ớt
        X.box((-1.9, cy - ly / 2 + 0.2 + k * (ly - 0.3) / 4, FLOOR + 0.97), (0.3, 0.22, 0.06), X.c('Khay_' + cols[k], cols[k], rough=0.7))
    X.box((-2.0, cy + ly / 2 - 0.15, FLOOR + 1.0), (0.35, 0.25, 0.04), X.c('ThotGo', '#B8875A'))
    # lò nướng inox 3 tầng có cửa kính, ánh lửa trong
    ox = -3.3
    X.ab((ox - 0.4, X.yl + 0.05, FLOOR), (ox + 0.4, X.yl + 0.85, FLOOR + 1.75), X.steel)
    for k in range(3):
        z = FLOOR + 0.3 + k * 0.48
        X.ab((ox - 0.36, X.yl + 0.1, z), (ox + 0.36, X.yl + 0.8, z + 0.36), X.c('LoNoiThat', '#4A2A12', emit=0.5))
        X.ab((ox + 0.4, X.yl + 0.12, z + 0.02), (ox + 0.41, X.yl + 0.78, z + 0.34), X.glass)
        for j in range(5): X.sph((ox, X.yl + 0.18 + j * 0.13, z + 0.08), 0.05, X.c('VoBanh', '#D9973F'), seg=8, rings=5, scale=(1.0, 3.2, 0.9))
        X.rod((ox + 0.43, X.yl + 0.25, z + 0.3), (ox + 0.43, X.yl + 0.65, z + 0.3), 0.012, X.black, seg=5)
    # giá bánh 5 tầng giữa tiệm, nhìn rõ từ kính
    rx0, rx1 = -5.6, -4.2; ry = X.yl + X.inner * 0.5
    for lv in range(5):
        z = FLOOR + 0.25 + lv * 0.36
        X.ab((rx0, ry - 0.3, z - 0.02), (rx1, ry + 0.3, z), X.steelD)
        for k in range(10):
            X.sph((rx0 + 0.08 + k * 0.135, ry - 0.12, z + 0.05), 0.05, X.c('VoBanh', '#D9973F'), seg=8, rings=5, scale=(1.0, 3.4, 0.9), ) if rnd.random() < 0.9 else None
            X.sph((rx0 + 0.08 + k * 0.135, ry + 0.14, z + 0.05), 0.05, X.c('VoBanhSam', '#C9822F'), seg=8, rings=5, scale=(1.0, 3.4, 0.9)) if rnd.random() < 0.8 else None
    for xx in (rx0, rx1):
        for yy in (ry - 0.3, ry + 0.3): X.rod((xx, yy, FLOOR), (xx, yy, FLOOR + 1.9), 0.014, X.steelD, seg=5)
    for yy in (ry - 0.3, ry + 0.3): X.cyl((rx0 + 0.05, yy, FLOOR), 0.04, 0.04, X.black, seg=8)
    # rổ mây bánh mì trên bàn gỗ + bao bột + két nước
    X.ab((-7.2, X.yl + 0.1, FLOOR), (-5.9, X.yl + 0.8, FLOOR + 0.8), X.woodL)
    for k in range(3):
        bx = -7.0 + k * 0.42
        X.cyl((bx, X.yl + 0.45, FLOOR + 0.8), 0.18, 0.14, X.c('RoMay', '#B8864A', rough=0.95), r2=0.21, seg=14)
        for j in range(4): X.sph((bx - 0.06 + (j % 2) * 0.12, X.yl + 0.4 + (j // 2) * 0.1, FLOOR + 0.98 + (j % 2) * 0.03), 0.05, X.c('VoBanh', '#D9973F'), seg=8, rings=5, scale=(3.2, 1.0, 0.9))
    for k in range(4): X.box((-7.4 + (k % 2) * 0.48, X.yl + 1.25 + (k // 2) * 0.0, FLOOR + 0.17 + (k // 2) * 0.33), (0.45, 0.55, 0.33), X.c('BaoBot', '#EDE3CE', rough=0.95))
    X.L.text(X.col, 'BỘT MÌ', (-7.16, X.yl + 0.97, FLOOR + 0.25), 0.08, X.c('ChuBao', '#C8262A'), rot=(math.pi / 2, 0, 0), extrude=0.002)
    # bàn ăn tại chỗ + ghế nhựa + quầy đứng bên phải
    X.ab((-6.8, X.yr - 0.35, FLOOR + 1.0), (-2.6, X.yr - 0.02, FLOOR + 1.04), X.woodL)
    for k in range(4):
        sx = -6.3 + k * 1.0
        X.cyl((sx, X.yr - 0.6, FLOOR), 0.16, 0.03, X.black); X.rod((sx, X.yr - 0.6, FLOOR), (sx, X.yr - 0.6, FLOOR + 0.72), 0.025, X.black)
        X.cyl((sx, X.yr - 0.6, FLOOR + 0.72), 0.17, 0.05, X.c('NemGhe', '#C8262A'))
        X.box((sx, X.yr - 0.18, FLOOR + 1.07), (0.22, 0.14, 0.04), X.c('GiayGoi', '#F2E2C0'))
        X.cyl((sx + 0.15, X.yr - 0.2, FLOOR + 1.04), 0.04, 0.13, X.c('LySua', '#F5F2E8'), seg=8)
    if X.inner > 4.2:
        steel_table(X, -3.0, X.yl + X.inner * 0.62, ly=0.7, lx=0.9, h=0.72); condiments(X, -3.0, X.yl + X.inner * 0.62, rnd)
        for dx in (-0.3, 0.3):
            for sy in (-1, 1): stool(X, -3.0 + dx, X.yl + X.inner * 0.62 + sy * 0.55, X.c('Ghe_#1F6FB5', '#1F6FB5'))
    fridge(X, -7.4, X.yr, 1, w=0.7, h=1.85, brand='#1565C0', rnd=rnd)
    menu_board(X, -3.6, X.yr, 1, 'BÁNH MÌ', ['Bánh mì thịt ..... 25k', 'Bánh mì pa-tê ... 20k', 'Bánh mì xíu mại . 25k', 'Bánh mì ốp la ... 20k', 'Sữa đậu nành ..... 8k'], bg='#E0A321', fg='#2B1A0E', acc='#7E2A1E', w=2.0, h=1.2, z=2.8)
    food_frames(X, X.yl, -1, -4.2, 2, ['#F3E2B8', '#EED7A8'])
    wall_fan(X, -5.8, X.yr, 1, 3.4); clock(X, -1.6, X.yr, 1, z=3.4); calendar(X, -6.2, X.yl, -1, z=2.6)
    ceiling_fan(X, -4.9, X.yl + X.inner * 0.5)
    altar_than_tai(X, -7.8, X.yl + X.inner * 0.75, 0)
    back_sign(X, 'Bánh Mì', 'Nóng giòn mỗi ngày', '#E0A321', '#4A1E0E')

def furnish_CaPhe(X, rnd):
    shell(X, X.kind)
    # quầy bar gỗ bên phải phía sau, máy pha, xay, phin, kệ ly
    by = X.yr - 0.4
    counter(X, -5.6, by, 0.7, lx=2.6, h=1.05, m=X.c('QuayGoCaPhe', '#3A2418'), top=X.woodL)
    X.box((-5.0, by, FLOOR + 1.28), (0.45, 0.4, 0.4), X.steel)
    X.box((-4.8, by, FLOOR + 1.16), (0.1, 0.3, 0.08), X.black)
    X.cyl((-5.6, by, FLOOR + 1.09), 0.09, 0.32, X.black); X.cyl((-5.6, by, FLOOR + 1.41), 0.07, 0.12, X.c('PheuXay', '#D7E0E4', rough=0.1), r2=0.1)
    for k in range(6): X.cyl((-6.4 + k * 0.1, by - 0.1, FLOOR + 1.09), 0.035, 0.05, X.steel, seg=8); X.cyl((-6.4 + k * 0.1, by - 0.1, FLOOR + 1.14), 0.04, 0.06, X.steelD, seg=8)
    for lv in range(3):
        X.ab((-7.0, X.yr - 0.25, FLOOR + 1.7 + lv * 0.42), (-4.4, X.yr - 0.02, FLOOR + 1.73 + lv * 0.42), X.woodD)
        for k in range(12):
            if lv == 2: X.cyl((-6.9 + k * 0.21, X.yr - 0.13, FLOOR + 1.73 + lv * 0.42), 0.07, 0.24, X.c('HuCaPhe', '#C9A97A', rough=0.15), seg=10)
            else: X.cyl((-6.9 + k * 0.21, X.yr - 0.13, FLOOR + 1.73 + lv * 0.42), 0.045, 0.1, X.white if k % 3 else X.c('LyNau', '#6B4A33'), seg=8)
    # bàn gỗ tròn + ghế gỗ, đèn thả
    pts = [(-1.6, X.yl + X.inner * 0.3), (-1.6, X.yl + X.inner * 0.72), (-3.2, X.yl + X.inner * 0.35), (-3.4, X.yl + X.inner * 0.72 if X.inner > 4.4 else -99)]
    for i, (tx, ty) in enumerate(pts):
        if ty < -50: continue
        X.cyl((tx, ty, FLOOR), 0.25, 0.03, X.black); X.rod((tx, ty, FLOOR), (tx, ty, FLOOR + 0.7), 0.03, X.black)
        X.cyl((tx, ty, FLOOR + 0.7), 0.38, 0.04, X.woodL, seg=18)
        X.cyl((tx + 0.1, ty, FLOOR + 0.74), 0.045, 0.1, X.white, seg=8); X.cyl((tx + 0.1, ty, FLOOR + 0.84), 0.05, 0.05, X.steel, seg=8)
        for a in (0.4, 2.6):
            chair(X, tx + math.cos(a) * 0.6, ty + math.sin(a) * 0.6, a + math.pi, X.c('GheGoCaPhe', '#6B4227'))
        X.rod((tx, ty, CEIL), (tx, ty, CEIL - 1.1), 0.006, X.black, seg=4)
        X.cyl((tx, ty, CEIL - 1.35), 0.04, 0.25, X.c('ChaoDen', '#1E1E1E'), r2=0.2, seg=14)
        X.sph((tx, ty, CEIL - 1.33), 0.06, X.c('BongDenAm', '#FFD58A', emit=8.0), seg=8, rings=5)
    # tường gạch thẻ trang trí + tranh + cây
    for i in range(14):
        for j in range(int(X.inner / 0.25)):
            if (i + j) % 2: continue
            X.box((-1.2 - i * 0.26 - (j % 2) * 0.13, X.yl + 0.012, FLOOR + 1.0 + (j % 8) * 0.0), (0.24, 0.02, 0.08), X.c('GachThe', '#9A4A2E'))
    for k in range(8):
        for i in range(10):
            X.box((-1.25 - i * 0.26 - (k % 2) * 0.13, X.yl + 0.014, FLOOR + 0.9 + k * 0.1), (0.24, 0.02, 0.08), X.c('GachThe', '#9A4A2E'))
    for k, col in enumerate(('#C9A35A', '#4E7A6A', '#B2563A')):
        X.box((-4.0 - k * 0.9, X.yl + 0.02, 2.6), (0.7, 0.02, 0.9), X.c('KhungTranh', '#1E1E1E'))
        X.box((-4.0 - k * 0.9, X.yl + 0.032, 2.6), (0.6, 0.01, 0.8), X.c('Tranh_' + col, col))
    plant(X, -0.95, X.yl + 0.35, 1.2, '#E8E2D2'); plant(X, -7.6, X.yl + 0.35, 1.4, '#3A3A3A'); plant(X, -0.95, X.yr - 0.35, 1.0, '#E8E2D2')
    menu_board(X, -2.4, X.yr, 1, 'CÀ PHÊ', ['Cà phê sữa đá .. 25k', 'Bạc xỉu ......... 30k', 'Cà phê muối ..... 35k', 'Trà đào cam sả . 40k', 'Sinh tố bơ ...... 40k'], bg='#1F2A22', fg='#EDE6D2', acc='#E2B65C', z=2.6)
    altar_than_tai(X, -7.6, X.yr - 0.9, 1)
    back_sign(X, 'Cà Phê', 'Rang xay mộc', '#2E2018', '#E2B65C')

def furnish_TraSua(X, rnd):
    shell(X, X.kind)
    cy = X.yl + X.inner * 0.5
    counter(X, -3.6, cy, min(3.2, X.inner - 1.2), lx=0.7, h=1.05, m=X.c('QuayTraSua', '#F7C6D0'), top=X.white)
    X.ab((-3.25, cy - 1.2, FLOOR + 0.2), (-3.24, cy + 1.2, FLOOR + 0.9), X.c('VachQuay', '#7DB7C9'))
    for k in range(3): X.cyl((-3.7, cy - 0.8 + k * 0.5, FLOOR + 1.09), 0.15, 0.25, X.steel, seg=14)
    for k in range(3): X.cyl((-3.7, cy - 0.8 + k * 0.5, FLOOR + 1.3), 0.13, 0.02, X.c('TranChau', '#2A1A12'), seg=14)
    X.box((-3.6, cy + 0.9, FLOOR + 1.25), (0.35, 0.3, 0.4), X.white)   # máy ép nắp
    for k in range(10): X.cyl((-3.3, cy - 1.1 + k * 0.22, FLOOR + 1.09), 0.04, 0.16, X.c('LyTra_%d' % (k % 4), ['#E8C9A0', '#C98A5A', '#F2B5C6', '#9BD3B0'][k % 4], rough=0.2), r2=0.05, seg=8)
    # kệ siro + bảng menu đèn hộp trên tường sau quầy
    for lv in range(2):
        X.ab((-5.4, cy - 1.4, FLOOR + 1.6 + lv * 0.4), (-5.1, cy + 1.4, FLOOR + 1.63 + lv * 0.4), X.white)
        for k in range(12): X.cyl((-5.25, cy - 1.3 + k * 0.23, FLOOR + 1.63 + lv * 0.4), 0.04, 0.26, X.c('Siro_%d' % (k % 6), ['#E53935', '#FB8C00', '#FDD835', '#43A047', '#8E24AA', '#6D4C41'][k % 6], rough=0.15), seg=8)
    for k in range(3):
        X.box((BACK + 0.06, cy - 1.1 + k * 1.1, 3.1), (0.05, 1.0, 0.75), X.c('HopDenMenu', '#FFF8EE', emit=2.0))
        X.box((BACK + 0.09, cy - 1.1 + k * 1.1, 3.25), (0.01, 0.8, 0.22), X.c('AnhTra_%d' % k, ['#C98A5A', '#F2B5C6', '#9BD3B0'][k]))
    # ghế cao + bàn nhỏ phía trước, đèn neon
    for k in range(int(X.inner / 1.1)):
        yy = X.yl + 0.6 + k * 1.1
        X.cyl((-1.3, yy, FLOOR), 0.2, 0.03, X.white); X.rod((-1.3, yy, FLOOR), (-1.3, yy, FLOOR + 0.75), 0.025, X.white)
        X.cyl((-1.3, yy, FLOOR + 0.75), 0.3, 0.03, X.c('BanTron', '#F7C6D0'), seg=16)
        for sy in (-1, 1): X.cyl((-1.75, yy + sy * 0.2, FLOOR), 0.15, 0.65, X.c('GheTraSua', '#7DB7C9'), r2=0.17, seg=10)
    X.L.text(X.col, 'Trà Sữa', (-6.0, X.yr - 0.03, 2.9), 0.32, X.c('Neon', '#FF5FA2', emit=6.0), rot=(math.pi / 2, 0, 0), extrude=0.01)
    plant(X, -0.95, X.yr - 0.35, 1.0, '#FFFFFF'); plant(X, -7.6, X.yl + 0.35, 1.2, '#F7C6D0')
    altar_than_tai(X, -7.6, X.yr - 0.3, 1)
    back_sign(X, 'Trà Sữa', 'Trân châu đường đen', '#7DB7C9', '#FFFFFF', z=3.75)

def _grocery(X, rnd, palette_sets, title, sub, bg, fg, gondolas=True, fridges=2):
    shell(X, X.kind)
    products_shelf(X, -7.6, -1.6, X.yl, -1, 5, 2.1, 0.45, rnd, palette_sets[0], kind='mix')
    products_shelf(X, -7.6, -3.2, X.yr, 1, 5, 2.1, 0.45, rnd, palette_sets[1], kind='mix')
    for k in range(fridges): fridge(X, -2.0 - k * 0.75, X.yr, 1, rnd=rnd)
    if gondolas and X.inner > 4.0:
        gy = X.yl + X.inner * 0.5
        for xx0, xx1 in ((-6.8, -4.4),):
            for side in (-1, 1):
                products_shelf(X, xx0, xx1, gy, side, 4, 1.5, 0.35, rnd, palette_sets[2], kind='mix', frame=X.c('KeGiua', '#DADDE0'))
    # quầy thu ngân gần cửa + két + kẹo
    counter(X, -1.5, X.yl + 0.45, 0.8, lx=0.7, h=0.95, m=X.c('QuayThuNgan', bg))
    register(X, -1.55, X.yl + 0.45)
    for k in range(6): X.cyl((-1.25, X.yl + 0.15 + k * 0.1, FLOOR + 0.99), 0.04, 0.12, X.c('HuKeo_%d' % (k % 3), ['#E53935', '#FDD835', '#43A047'][k % 3], rough=0.1), seg=8)
    # thùng nước + bao gạo + két bia dưới đất
    for k in range(3):
        X.box((-3.0 - k * 0.42, X.yl + 0.9, FLOOR + 0.17), (0.4, 0.3, 0.34), X.c('ThungNuoc', '#5FA8D3', rough=0.3))
        X.box((-3.0 - k * 0.42, X.yl + 0.9, FLOOR + 0.5), (0.4, 0.3, 0.3), X.c('ThungNuoc2', '#3F8FBF', rough=0.3))
    for k in range(2): X.sph((-7.0 + k * 0.6, X.yr - 0.9, FLOOR + 0.3), 0.3, X.c('BaoGao', '#EFE8D6', rough=0.95), seg=8, rings=5, scale=(0.9, 0.7, 1.1))
    X.box((-6.0, X.yr - 0.9, FLOOR + 0.15), (0.45, 0.32, 0.3), X.c('KetBia', '#2E7D32'))
    X.box((-6.0, X.yr - 0.9, FLOOR + 0.45), (0.45, 0.32, 0.3), X.c('KetBia2', '#C62828'))
    ceiling_fan(X, -3.5, X.yl + X.inner * 0.5); clock(X, -1.6, X.yr, 1, z=3.4)
    altar_than_tai(X, -7.8, X.yl + X.inner * 0.5, 0)
    back_sign(X, title, sub, bg, fg, z=3.55)

def furnish_TapHoa(X, rnd):
    P1 = ['#E53935', '#FDD835', '#1E88E5', '#43A047', '#FB8C00', '#8E24AA', '#F5F5F5', '#6D4C41']
    _grocery(X, rnd, [P1, P1, P1], 'Tạp Hóa', 'Bán sỉ & lẻ', '#2B7A3B', '#FFFFFF')

def furnish_BachHoa(X, rnd):
    P1 = ['#2E7D32', '#66BB6A', '#FDD835', '#F5F5F5', '#E53935', '#1E88E5', '#FFB74D']
    _grocery(X, rnd, [P1, P1, P1], 'Bách Hóa', 'Tươi ngon mỗi ngày', '#2B8A3E', '#FFFFFF', fridges=3)
    # kệ rau củ trái cây gần cửa
    gx = -2.6; gy = X.yl + X.inner * 0.5
    X.ab((gx - 0.6, gy - 0.6, FLOOR), (gx + 0.6, gy + 0.6, FLOOR + 0.7), X.c('KeRau', '#8D6E63'))
    cols = ['#E53935', '#FB8C00', '#7CB342', '#FDD835', '#6D4C41', '#43A047']
    for i in range(4):
        for j in range(4):
            c = cols[(i * 4 + j) % len(cols)]
            X.sph((gx - 0.45 + i * 0.3, gy - 0.45 + j * 0.3, FLOOR + 0.78), 0.1, X.c('Trai_' + c, c, rough=0.5), seg=8, rings=5)

def furnish_NhaThuoc(X, rnd):
    shell(X, X.kind)
    pal = ['#FFFFFF', '#E3F2FD', '#1E88E5', '#43A047', '#E53935', '#FDD835', '#B3E5FC', '#C8E6C9']
    products_shelf(X, -7.6, -1.4, X.yl, -1, 6, 2.3, 0.35, rnd, pal, kind='box', frame=X.c('KeThuoc', '#F7F7F7'))
    products_shelf(X, -7.6, -4.6, X.yr, 1, 6, 2.3, 0.35, rnd, pal, kind='box', frame=X.c('KeThuoc', '#F7F7F7'))
    # quầy kính chữ L, dược sĩ, máy tính
    cy = X.yl + X.inner * 0.55
    glass_cabinet(X, -3.0, cy, 0.6, min(2.6, X.inner - 1.4), 0.55, frame=X.white,
                  top_items=lambda z0, z1: [X.box((-3.0, cy - 1.0 + k * 0.25, z0 + 0.05), (0.2, 0.18, 0.1), X.c('HopThuoc_%d' % (k % 3), pal[2 + k % 3])) for k in range(int(min(2.6, X.inner - 1.4) / 0.25))])
    register(X, -3.0, cy + 0.6)
    X.cyl((-3.6, cy, FLOOR), 0.17, 0.7, X.c('AoBlouse', '#FFFFFF'), r2=0.2)
    X.cyl((-3.6, cy, FLOOR + 0.7), 0.2, 0.55, X.c('AoBlouse', '#FFFFFF'), r2=0.18)
    X.sph((-3.6, cy, FLOOR + 1.42), 0.11, X.c('Da', '#E9C4A0'), scale=(0.9, 0.9, 1.15)); X.sph((-3.62, cy, FLOOR + 1.47), 0.11, X.black, scale=(0.95, 0.95, 0.8))
    # chữ thập xanh phát sáng + áp phích sức khoẻ
    for (dx, dz, sx, sz) in ((0, 0, 0.18, 0.55), (0, 0, 0.55, 0.18)):
        X.box((BACK + 0.07, X.yl + X.inner * 0.42 + dx, 3.55 + dz), (0.04, sx, sz), X.c('ChuThapXanh', '#25C26E', emit=5.0))
    poster(X, -5.5, X.yr, 1, 0.6, 0.85, 2.75, '#E3F2FD', '#1E88E5'); poster(X, -6.3, X.yr, 1, 0.6, 0.85, 2.75, '#E8F5E9', '#43A047')
    X.ab((-1.3, X.yr - 0.5, FLOOR), (-0.9, X.yr - 0.1, FLOOR + 1.1), X.white)   # máy đo huyết áp/cân
    X.box((-1.1, X.yr - 0.3, FLOOR + 1.2), (0.3, 0.3, 0.2), X.c('MayDo', '#1E88E5'))
    plant(X, -0.95, X.yl + 0.35, 1.0, '#FFFFFF')
    back_sign(X, 'Nhà Thuốc', 'Tư vấn miễn phí', '#1F7A4A', '#FFFFFF', z=2.75)

def tire(X, x, y, z, r=0.29, w=0.1, axis='Z'):
    """Lốp rời: vành cao su ngoài + lòng tối, nhìn ra lỗ tròn."""
    X.cyl((x, y, z), r, w, X.c('LopXe', '#151515', rough=0.9), seg=18, axis=axis)
    X.cyl((x, y, z - 0.002 if axis == 'Z' else z), r * 0.55, w + 0.004, X.c('LongLop', '#050505'), seg=14, axis=axis)

def furnish_SuaXe(X, rnd):
    shell(X, X.kind)
    # vách treo dụng cụ (pegboard) + cờ lê/búa/tua vít/kìm thật hình
    X.ab((-7.4, X.yl + 0.0, FLOOR + 1.0), (-2.4, X.yl + 0.03, FLOOR + 2.5), X.c('Pegboard', '#C9B48A'))
    for i in range(16):
        for j in range(5):
            X.cyl((-7.3 + i * 0.31, X.yl + 0.031, FLOOR + 1.1 + j * 0.3), 0.012, 0.004, X.c('LoPeg', '#8A7A5A'), seg=6, axis='Y')
    metal = [X.c('DungCu_%d' % k, c, metal=0.6, rough=0.3) for k, c in enumerate(('#B0B6BC', '#C62828', '#1565C0', '#F9A825'))]
    for k in range(13):   # cờ lê to → nhỏ
        L_ = 0.34 - k * 0.018; tx = -7.2 + k * 0.17
        X.box((tx, X.yl + 0.05, FLOOR + 2.05), (0.025, 0.012, L_), metal[0])
        X.cyl((tx, X.yl + 0.05, FLOOR + 2.05 + L_ / 2), 0.035 - k * 0.0015, 0.012, metal[0], seg=8, axis='Y')
    for k in range(6):    # tua vít cán màu
        tx = -4.9 + k * 0.16
        X.cyl((tx, X.yl + 0.05, FLOOR + 1.95), 0.02, 0.12, metal[1 + k % 3], seg=8); X.rod((tx, X.yl + 0.05, FLOOR + 1.95), (tx, X.yl + 0.05, FLOOR + 1.72), 0.005, metal[0], seg=4)
    for k in range(4):    # búa + kìm
        tx = -3.7 + k * 0.32
        X.box((tx, X.yl + 0.05, FLOOR + 1.45), (0.03, 0.02, 0.3), X.woodL); X.box((tx, X.yl + 0.05, FLOOR + 1.62), (0.13, 0.03, 0.05), metal[0])
    X.L.text(X.col, 'ĐỒ NGHỀ', (-4.9, X.yl + 0.04, FLOOR + 2.38), 0.12, X.c('ChuDo', '#C62828'), rot=(math.pi / 2, 0, math.pi), extrude=0.003)
    # bàn nguội + ê tô + thùng đồ nghề đỏ nhiều ngăn
    X.ab((-6.6, X.yl + 0.05, FLOOR + 0.85), (-3.0, X.yl + 0.75, FLOOR + 0.9), X.c('MatBanNguoi', '#5A4636'))
    for xx in (-6.5, -3.1):
        for yy in (X.yl + 0.1, X.yl + 0.7): X.box((xx, yy, FLOOR + 0.42), (0.05, 0.05, 0.85), X.steelD)
    X.box((-3.4, X.yl + 0.45, FLOOR + 1.0), (0.22, 0.16, 0.18), X.c('ETo', '#2A5E9A'))
    for k in range(5): X.box((-5.2 + k * 0.25, X.yl + 0.4, FLOOR + 0.95), (0.18, 0.12, 0.08), X.c('LinhKien_%d' % (k % 3), ['#9AA0A6', '#3A3C3E', '#C62828'][k % 3], metal=0.5))
    X.ab((-6.45, X.yl + 0.1, FLOOR), (-5.55, X.yl + 0.7, FLOOR + 0.8), X.c('TuDoNghe', '#C62828', metal=0.3, rough=0.4))
    for k in range(5): X.box((-6.0, X.yl + 0.705, FLOOR + 0.1 + k * 0.15), (0.84, 0.01, 0.02), X.steel)
    # bàn nâng giữa xưởng + xe đang sửa (bánh trước tháo ra dựng bên cạnh), xe chờ sửa
    cy = X.yl + X.inner * 0.52
    X.ab((-4.7, cy - 0.38, FLOOR), (-2.5, cy + 0.38, FLOOR + 0.06), X.c('DeBanNang', '#1E4E8C'))
    X.ab((-4.6, cy - 0.33, FLOOR + 0.06), (-2.6, cy + 0.33, FLOOR + 0.38), X.c('BanNang', '#2E6EB5', metal=0.3, rough=0.4))
    X.ab((-4.65, cy - 0.36, FLOOR + 0.38), (-2.55, cy + 0.36, FLOOR + 0.41), X.c('MatBanNang', '#3A3C3E'))
    motorbike(X, -3.6, cy, 0.0, '#E8E8E8', z0=0.41)
    tire(X, -2.2, cy + 0.65, FLOOR + 0.29, axis='Y')
    if X.inner > 4.0: motorbike(X, -6.6, X.yr - 0.65, math.pi, '#B71C1C')
    # chồng lốp mới, ruột xe treo tường, can nhớt, thùng phuy, máy nén khí, xô nước vá xe
    X.ab((-2.0, X.yr - 0.42, FLOOR), (-0.9, X.yr - 0.36, FLOOR + 0.05), X.steelD)          # giá lốp dựng đứng dọc tường
    for k in range(7): tire(X, -1.9 + k * 0.155, X.yr - 0.45, FLOOR + 0.3, r=0.29 - (k % 2) * 0.02, w=0.1, axis='X')
    for k in range(3): tire(X, -1.4, X.yr - 1.2, FLOOR + k * 0.115, r=0.27)
    for k in range(4):
        X.cyl((-2.3 - k * 0.5, X.yr - 0.03, FLOOR + 2.2), 0.22, 0.03, X.c('RuotXe', '#2A2A2A', rough=0.6), seg=16, axis='Y')
        X.cyl((-2.3 - k * 0.5, X.yr - 0.025, FLOOR + 2.2), 0.17, 0.04, X.c('Tuong_' + X.kind, '#9DA3A0'), seg=14, axis='Y')
    for k in range(9): X.box((-2.4 + (k % 3) * 0.17, X.yr - 0.18, FLOOR + 0.13 + (k // 3) * 0.27), (0.14, 0.09, 0.25), X.c('CanNhot_%d' % (k % 3), ['#F9A825', '#1565C0', '#C62828'][k % 3]))
    X.ab((-2.6, X.yr - 0.35, FLOOR + 0.0), (-1.85, X.yr - 0.02, FLOOR + 0.02), X.steelD)
    X.cyl((-5.2, X.yr - 0.4, FLOOR), 0.3, 0.9, X.c('ThungPhuy', '#1F5FA8', metal=0.3), seg=16)
    for zz in (0.25, 0.65): X.cyl((-5.2, X.yr - 0.4, FLOOR + zz), 0.31, 0.03, X.c('DaiPhuy', '#164A85'), seg=16)
    X.cyl((-7.5, X.yr - 0.5, FLOOR + 0.25), 0.22, 0.75, X.c('MayNen', '#C62828'), axis='X')
    X.box((-7.25, X.yr - 0.5, FLOOR + 0.62), (0.32, 0.3, 0.22), X.black)
    X.rod((-7.25, X.yr - 0.5, FLOOR + 0.7), (-6.6, X.yr - 0.9, FLOOR + 0.02), 0.012, X.c('DayHoi', '#F2C14E'), seg=5)
    X.cyl((-1.9, X.yl + 1.2, FLOOR), 0.17, 0.28, X.c('XoNuoc', '#2E7D32'), r2=0.2, seg=12)
    X.cyl((-1.9, X.yl + 1.2, FLOOR + 0.26), 0.17, 0.01, X.c('NuocXo', '#6FA8C8', rough=0.05), seg=12)
    for k in range(2): stool(X, -2.7 + k * 1.6, X.yl + 1.25, X.c('Ghe_#1F6FB5', '#1F6FB5'), h=0.3)
    X.cyl((-4.0, cy + 0.2, FLOOR + 0.001), 0.6, 0.004, X.c('VetDau', '#2C2A26', rough=0.1), seg=16)
    poster(X, -4.6, X.yr, 1, 1.0, 0.65, 2.75, '#F5F5F5', '#C62828'); poster(X, -6.0, X.yr, 1, 0.7, 0.9, 2.7, '#1565C0', '#F9A825')
    calendar(X, -3.4, X.yr, 1, z=2.6); wall_fan(X, -1.8, X.yl, -1, 3.2); clock(X, -7.0, X.yr, 1, z=3.3)
    X.ab((-1.2, X.yl + 0.02, FLOOR + 2.6), (-0.8, X.yl + 0.5, FLOOR + 2.63), X.wood)
    for k in range(2): X.sph((-1.0, X.yl + 0.15 + k * 0.22, FLOOR + 2.75), 0.12, X.c('NonBH_%d' % k, ['#C62828', '#F5F5F5'][k], rough=0.3), seg=10, rings=6, scale=(1, 1, 0.85))
    altar_than_tai(X, -7.8, X.yl + X.inner * 0.5, 0)
    back_sign(X, 'Sửa Xe', 'Thay nhớt – Vá xe', '#2B4A7A', '#FFFFFF', z=3.0)

def furnish_TiemVang(X, rnd):
    shell(X, X.kind)
    gold = X.gold; red = X.c('NhungDo', '#8E0E12', rough=0.9)
    # 3 tủ kính chữ U bày vàng, bên trong nhung đỏ
    runs = [(-2.0, X.yl + 0.55, 'y', X.inner - 1.1), (-5.0, X.yl + 0.5, 'x', 2.6), (-5.0, X.yr - 0.5, 'x', 2.6)]
    for (x, y, ax, L) in runs:
        if ax == 'y':
            cx, cy, lx, ly = x, y + L / 2 - 0.0, 0.6, L
        else:
            cx, cy, lx, ly = x, y, L, 0.6
        X.ab((cx - lx / 2, cy - ly / 2, FLOOR), (cx + lx / 2, cy + ly / 2, FLOOR + 0.85), X.c('ThanTuVang', '#2A1A12', rough=0.3))
        X.ab((cx - lx / 2 + 0.03, cy - ly / 2 + 0.03, FLOOR + 0.85), (cx + lx / 2 - 0.03, cy + ly / 2 - 0.03, FLOOR + 0.88), red)
        X.ab((cx - lx / 2, cy - ly / 2, FLOOR + 1.12), (cx + lx / 2, cy + ly / 2, FLOOR + 1.13), X.glass)
        for sx in (-1, 1): X.ab((cx + sx * lx / 2 - 0.005, cy - ly / 2, FLOOR + 0.85), (cx + sx * lx / 2 + 0.005, cy + ly / 2, FLOOR + 1.12), X.glass)
        for sy in (-1, 1): X.ab((cx - lx / 2, cy + sy * ly / 2 - 0.005, FLOOR + 0.85), (cx + lx / 2, cy + sy * ly / 2 + 0.005, FLOOR + 1.12), X.glass)
        n = int(max(lx, ly) / 0.14)
        for k in range(n):
            t = -0.5 + (k + 0.5) / n
            px = cx + (t * lx if ax == 'x' else 0); py = cy + (t * ly if ax == 'y' else 0)
            kk = k % 4
            if kk == 0: X.cyl((px, py, FLOOR + 0.89), 0.04, 0.012, gold, seg=12)
            elif kk == 1: X.box((px, py, FLOOR + 0.9), (0.08, 0.05, 0.02), gold)
            elif kk == 2: X.cyl((px, py, FLOOR + 0.89), 0.05, 0.15, X.c('GiaDayChuyen', '#F2EDE4'), r2=0.01, seg=8); X.cyl((px, py, FLOOR + 0.99), 0.03, 0.01, gold, seg=10)
            else: X.sph((px, py, FLOOR + 0.9), 0.025, gold, seg=8, rings=5)
        X.ab((cx - lx / 2, cy - ly / 2, FLOOR + 0.2), (cx + lx / 2, cy + ly / 2, FLOOR + 0.22), gold)
    # tường sau: gương + kệ vàng miếng, két sắt, chữ Phúc/Lộc, đèn chùm
    X.ab((BACK + 0.02, X.yl + 0.6, FLOOR + 1.3), (BACK + 0.05, X.yr - 1.3, FLOOR + 2.9), X.c('Guong', '#C9D6DE', rough=0.02, metal=0.9))
    X.ab((BACK + 0.02, X.yr - 1.1, FLOOR), (BACK + 0.8, X.yr - 0.2, FLOOR + 1.3), X.c('KetSat', '#3A3F44', metal=0.6, rough=0.4))
    X.cyl((BACK + 0.81, X.yr - 0.65, FLOOR + 0.8), 0.1, 0.03, X.steel, axis='X')
    for k in range(3):
        X.rod((-2.5 - k * 2.0, X.yl + X.inner * 0.5, CEIL), (-2.5 - k * 2.0, X.yl + X.inner * 0.5, CEIL - 0.35), 0.008, gold, seg=4)
        for j in range(8):
            a = j * math.pi / 4
            X.sph((-2.5 - k * 2.0 + math.cos(a) * 0.3, X.yl + X.inner * 0.5 + math.sin(a) * 0.3, CEIL - 0.55), 0.05, X.c('PhaLe', '#FFF7E0', emit=4.0), seg=8, rings=5)
        X.cyl((-2.5 - k * 2.0, X.yl + X.inner * 0.5, CEIL - 0.5), 0.33, 0.04, gold, seg=16)
    X.L.text(X.col, 'PHÚC', (-6.2, X.yr - 0.03, 3.2), 0.36, gold, rot=(math.pi / 2, 0, 0), extrude=0.01)
    X.L.text(X.col, 'LỘC', (-6.2, X.yl + 0.03, 3.2), 0.36, gold, rot=(math.pi / 2, 0, math.pi), extrude=0.01)
    X.L.text(X.col, 'GIÁ VÀNG HÔM NAY', (-3.0, X.yr - 0.03, 3.4), 0.12, X.c('ChuDenLed', '#FF4A2A', emit=4.0), rot=(math.pi / 2, 0, 0), extrude=0.003)
    X.box((-3.0, X.yr - 0.02, 3.0), (1.3, 0.03, 0.7), X.c('BangLed', '#141414'))
    for k in range(4): X.box((-3.0, X.yr - 0.04, 3.2 - k * 0.14), (1.1, 0.005, 0.06), X.c('SoLed', '#FFC14A', emit=3.0))
    altar_than_tai(X, -7.8, X.yl + 0.35, -1)
    back_sign(X, 'Tiệm Vàng', 'Vàng bạc – Đá quý', '#7A1416', '#F2C14E', z=3.45)

def furnish_DienThoai(X, rnd):
    shell(X, X.kind)
    # bàn trải nghiệm giữa + điện thoại dựng, quầy kính hai bên, tường phụ kiện, áp phích đèn hộp
    ty = X.yl + X.inner * 0.5
    X.ab((-4.6, ty - 0.45, FLOOR), (-2.2, ty + 0.45, FLOOR + 0.9), X.c('BanTraiNghiem', '#E7EAEE'))
    X.ab((-4.65, ty - 0.5, FLOOR + 0.9), (-2.15, ty + 0.5, FLOOR + 0.94), X.woodL)
    for k in range(10):
        px = -4.4 + (k % 5) * 0.48; py = ty - 0.22 + (k // 5) * 0.44
        X.box((px, py, FLOOR + 0.99), (0.05, 0.03, 0.08), X.black)
        X.box((px + 0.02, py, FLOOR + 1.05), (0.012, 0.08, 0.16), X.c('ThanDT_%d' % (k % 4), ['#1E1E1E', '#C9CED3', '#D4AF7A', '#3E5C8A'][k % 4], metal=0.5, rough=0.2), ry=-0.35)
        X.box((px + 0.03, py, FLOOR + 1.05), (0.004, 0.07, 0.15), X.c('ManDT', '#4FA3E0', emit=2.0), ry=-0.35)
    for side, y_wall in ((-1, X.yl), (1, X.yr)):
        glass_cabinet(X, -4.0, y_wall - side * 0.35, 3.6, 0.55, 0.45, frame=X.white,
                      top_items=lambda z0, z1, yw=y_wall, sd=side: [X.box((-5.6 + k * 0.32, yw - sd * 0.35, z0 + 0.01), (0.09, 0.17, 0.012), X.c('ThanDT_%d' % (k % 4), ['#1E1E1E', '#C9CED3', '#D4AF7A', '#3E5C8A'][k % 4])) for k in range(11)])
        for k in range(14):   # tường ốp lưng/tai nghe/sạc
            X.box((-6.9 + (k % 7) * 0.5, y_wall - side * 0.04, FLOOR + 1.5 + (k // 7) * 0.4), (0.12, 0.04, 0.22), X.c('PhuKien_%d' % (k % 5), ['#E53935', '#1E88E5', '#FDD835', '#212121', '#F5F5F5'][k % 5]))
        X.box((-4.0, y_wall - side * 0.015, FLOOR + 2.85), (2.4, 0.03, 0.9), X.c('Lightbox_%d' % side, '#FFFFFF', emit=1.8))
        X.box((-4.0, y_wall - side * 0.032, FLOOR + 2.85), (1.4, 0.005, 0.7), X.c('AnhQC_%d' % side, '#3E5C8A' if side < 0 else '#D4AF7A'))
    counter(X, -6.8, ty, min(2.2, X.inner - 1.6), lx=0.7, h=1.0, m=X.c('QuayDT', '#1F5FA8'), top=X.white)
    register(X, -6.8, ty + 0.4)
    X.L.text(X.col, 'Trả góp 0%', (BACK + 0.08, X.yl + X.inner * 0.5, 2.1), 0.16, X.c('ChuDo', '#E53935'), rot=(math.pi / 2, 0, math.pi / 2), extrude=0.003)
    altar_than_tai(X, -7.8, X.yr - 0.35, 1)
    back_sign(X, 'Điện Thoại', 'Chính hãng – Bảo hành 12 tháng', '#1F5FA8', '#FFFFFF', z=3.0)

def furnish_ThoiTrang(X, rnd):
    shell(X, X.kind)
    pal = ['#1E1E1E', '#F5F5F5', '#C62828', '#1565C0', '#F8BBD0', '#A1887F', '#FFF59D', '#80CBC4', '#6A1B9A']
    hanging_clothes(X, -7.0, -2.6, X.yl + 0.35, FLOOR + 1.75, rnd, pal)
    hanging_clothes(X, -7.0, -4.0, X.yr - 0.35, FLOOR + 1.75, rnd, pal)
    # bàn gỗ giữa xếp áo gấp, ma-nơ-canh sát kính
    ty = X.yl + X.inner * 0.5
    X.ab((-4.2, ty - 0.45, FLOOR), (-2.8, ty + 0.45, FLOOR + 0.8), X.woodL)
    for k in range(12):
        c = pal[k % len(pal)]
        X.box((-4.0 + (k % 4) * 0.33, ty - 0.25 + (k // 4) * 0.25, FLOOR + 0.84 + (k % 2) * 0.04), (0.3, 0.22, 0.08), X.c('AoGap_' + c, c, rough=0.85))
    mannequin(X, -1.1, X.yl + X.inner * 0.3, X.c('VayDo', '#C62828', rough=0.8))
    mannequin(X, -1.1, X.yl + X.inner * 0.7, X.c('AoKhoac', '#1565C0', rough=0.8))
    # phòng thử rèm + gương đứng + kệ giày
    X.ab((-7.9, X.yr - 1.3, FLOOR), (-7.85, X.yr, FLOOR + 2.3), X.c('VachThu', '#5B3A7A'))
    X.ab((-6.6, X.yr - 1.3, FLOOR + 0.1), (-6.58, X.yr - 0.05, FLOOR + 2.2), X.c('Rem', '#B39DDB', rough=0.95))
    X.rod((-6.6, X.yr - 1.3, FLOOR + 2.25), (-6.6, X.yr, FLOOR + 2.25), 0.012, X.steel)
    X.ab((-3.6, X.yr - 0.1, FLOOR + 0.1), (-2.6, X.yr - 0.05, FLOOR + 2.0), X.c('Guong', '#C9D6DE', rough=0.02, metal=0.9))
    X.ab((-3.62, X.yr - 0.12, FLOOR + 0.05), (-2.58, X.yr - 0.03, FLOOR + 2.05), X.woodD)
    for lv in range(3):
        X.ab((-2.4, X.yr - 0.35, FLOOR + 0.15 + lv * 0.35), (-1.3, X.yr - 0.02, FLOOR + 0.18 + lv * 0.35), X.woodL)
        for k in range(3): X.box((-2.2 + k * 0.35, X.yr - 0.18, FLOOR + 0.23 + lv * 0.35), (0.26, 0.1, 0.09), X.c('Giay_%d' % ((k + lv) % 3), ['#212121', '#F5F5F5', '#8D6E63'][(k + lv) % 3]))
    counter(X, -7.0, X.yl + 1.0, 1.2, lx=0.6, h=1.0, m=X.c('QuayTT', '#F5F5F5'), top=X.woodL)
    register(X, -7.0, X.yl + 1.2)
    for k in range(3): X.rod((-2.5 - k * 1.8, ty, CEIL), (-2.5 - k * 1.8, ty, CEIL - 0.7), 0.005, X.black, seg=4); X.sph((-2.5 - k * 1.8, ty, CEIL - 0.8), 0.12, X.c('DenCau', '#FFE9B8', emit=5.0), seg=10, rings=6)
    plant(X, -0.95, X.yr - 0.35, 1.1, '#E8E2D2')
    back_sign(X, 'Thời Trang', 'New Collection', '#5B3A7A', '#FFFFFF', z=3.2)

# ------------------------------------------------------------------------------------------------ 4 kiểu nhà ở
def _home_common(X, rnd, altar=True):
    shell(X, X.kind)
    if altar:   # tủ thờ gia tiên ở tường hậu: tủ gỗ cao, ảnh, đèn đỏ, lư hương, mâm quả
        cy = X.yl + X.inner * 0.4
        X.ab((BACK + 0.05, cy - 0.8, FLOOR), (BACK + 0.65, cy + 0.8, FLOOR + 0.95), X.woodD)
        for k in range(4): X.box((BACK + 0.66, cy - 0.6 + k * 0.4, FLOOR + 0.5), (0.01, 0.34, 0.7), X.c('CuaTuTho', '#5A3418'))
        X.ab((BACK + 0.05, cy - 0.85, FLOOR + 0.95), (BACK + 0.7, cy + 0.85, FLOOR + 1.0), X.c('MatTuTho', '#3A1E0E'))
        for k, col in enumerate(('#7A7A7A', '#5A5A5A')):
            X.box((BACK + 0.12, cy - 0.3 + k * 0.6, FLOOR + 1.3), (0.03, 0.3, 0.4), X.gold)
            X.box((BACK + 0.14, cy - 0.3 + k * 0.6, FLOOR + 1.3), (0.01, 0.24, 0.32), X.c('AnhTho_' + col, col))
        X.cyl((BACK + 0.4, cy, FLOOR + 1.0), 0.09, 0.1, X.c('LuHuong', '#B8862F', metal=0.7))
        for sy in (-1, 1): X.cyl((BACK + 0.35, cy + sy * 0.5, FLOOR + 1.0), 0.03, 0.25, X.redLamp)
        X.cyl((BACK + 0.45, cy + 0.25, FLOOR + 1.0), 0.14, 0.05, X.gold, r2=0.17)
        for k in range(5): X.sph((BACK + 0.45 + math.cos(k) * 0.07, cy + 0.25 + math.sin(k) * 0.07, FLOOR + 1.1), 0.05, X.c('Trai_%d' % (k % 3), ['#E53935', '#FB8C00', '#7CB342'][k % 3]), seg=7, rings=4)
        X.ab((BACK + 0.04, cy - 0.9, FLOOR + 2.0), (BACK + 0.08, cy + 0.9, FLOOR + 2.1), X.woodD)
        X.L.text(X.col, 'Phúc Lộc Thọ', (BACK + 0.09, cy, 2.55), 0.18, X.gold, rot=(math.pi / 2, 0, math.pi / 2), extrude=0.004)
    clock(X, -5.0, X.yr, 1, z=3.1); calendar(X, -3.8, X.yr, 1, z=2.4)

def furnish_NhaO_Salon(X, rnd):
    """Bộ salon gỗ chạm (trường kỷ + 2 ghế bành + bàn trà), tủ TV, 2 xe máy dựng sát cửa, kệ dép, chậu cây."""
    _home_common(X, rnd)
    sy = X.yl + X.inner * 0.62
    wd = X.c('GoSalon', '#5A2E14', rough=0.45)
    X.ab((-5.4, sy + 0.5, FLOOR + 0.2), (-3.6, sy + 1.05, FLOOR + 0.48), wd)
    X.ab((-5.4, sy + 0.95, FLOOR + 0.48), (-3.6, sy + 1.05, FLOOR + 1.05), wd)
    for xx in (-5.4, -3.6): X.ab((xx - 0.05, sy + 0.5, FLOOR + 0.48), (xx + 0.05, sy + 1.05, FLOOR + 0.75), wd)
    X.ab((-5.35, sy + 0.55, FLOOR + 0.48), (-3.65, sy + 0.93, FLOOR + 0.56), X.c('NemSalon', '#C9A35A', rough=0.9))
    for dx in (-5.9, -3.1):
        X.ab((dx - 0.32, sy - 0.3, FLOOR + 0.2), (dx + 0.32, sy + 0.3, FLOOR + 0.48), wd)
        X.ab((dx - 0.32 if dx < -4 else dx + 0.22, sy - 0.3, FLOOR + 0.48), (dx - 0.22 if dx < -4 else dx + 0.32, sy + 0.3, FLOOR + 1.0), wd)
    for xx in (-5.3, -3.7):
        for yy in (sy + 0.55, sy + 1.0): X.box((xx, yy, FLOOR + 0.1), (0.06, 0.06, 0.2), wd)
    X.ab((-5.0, sy - 0.35, FLOOR + 0.4), (-4.0, sy + 0.25, FLOOR + 0.45), wd)
    X.ab((-4.95, sy - 0.3, FLOOR + 0.45), (-4.05, sy + 0.2, FLOOR + 0.47), X.glass)
    X.cyl((-4.5, sy - 0.05, FLOOR + 0.47), 0.12, 0.02, X.c('KhayTra', '#B8862F', metal=0.5))
    for k in range(4): X.cyl((-4.55 + (k % 2) * 0.1, sy - 0.1 + (k // 2) * 0.1, FLOOR + 0.49), 0.025, 0.04, X.white, seg=8)
    X.sph((-4.38, sy - 0.05, FLOOR + 0.55), 0.06, X.c('AmTra', '#E8E2D2'), seg=10, rings=6)
    # tủ TV + TV đối diện
    X.ab((-5.6, X.yl + 0.05, FLOOR), (-3.4, X.yl + 0.5, FLOOR + 0.5), X.woodD)
    X.box((-4.5, X.yl + 0.12, FLOOR + 1.15), (1.3, 0.05, 0.75), X.black)
    X.box((-4.5, X.yl + 0.15, FLOOR + 1.15), (1.24, 0.005, 0.69), X.c('ManTV', '#2A4A6A', emit=0.8))
    X.cyl((-3.6, X.yl + 0.3, FLOOR + 0.5), 0.08, 0.3, X.c('BinhHoa', '#3E6E9A'), seg=10)
    for k in range(5): X.sph((-3.6 + math.cos(k) * 0.06, X.yl + 0.3 + math.sin(k) * 0.06, FLOOR + 0.9), 0.06, X.c('Hoa_%d' % (k % 2), ['#F06292', '#FFEB3B'][k % 2]), seg=6, rings=4)
    motorbike(X, -1.6, X.yl + 0.75, 0.15, '#B71C1C')
    if X.inner > 4.3: motorbike(X, -1.9, X.yl + 1.75, -0.1, '#1565C0')
    X.ab((-2.6, X.yr - 0.32, FLOOR), (-1.2, X.yr - 0.02, FLOOR + 0.6), X.woodL)
    for k in range(6): X.box((-2.45 + k * 0.22, X.yr - 0.17, FLOOR + 0.62 + (k % 2) * 0.0), (0.1, 0.24, 0.05), X.c('Dep_%d' % (k % 3), ['#E53935', '#1E88E5', '#212121'][k % 3]))
    plant(X, -0.95, X.yr - 0.35, 1.2); ceiling_fan(X, -4.5, sy)

def furnish_NhaO_XeMay(X, rnd):
    """Nhà để 3 xe máy ở phòng trước, bàn ăn gia đình + ghế nhựa, tủ lạnh, cầu thang lên lầu ở cuối."""
    _home_common(X, rnd, altar=False)
    cols = ['#B71C1C', '#212121', '#ECEFF1', '#1565C0', '#F9A825']
    n = 3 if X.inner > 4.4 else 2
    for k in range(n): motorbike(X, -1.8 - (k % 2) * 0.2, X.yl + 0.65 + k * (X.inner - 1.0) / max(1, n - 1) * 0.92, 0.1 * (k - 1), cols[(k * 2 + 1) % 5])
    ty = X.yl + X.inner * 0.45
    X.ab((-5.0, ty - 0.45, FLOOR + 0.72), (-3.6, ty + 0.45, FLOOR + 0.76), X.c('MatBanAn', '#C9A97A'))
    for xx in (-4.9, -3.7):
        for yy in (ty - 0.4, ty + 0.4): X.box((xx, yy, FLOOR + 0.36), (0.05, 0.05, 0.72), X.woodD)
    for k, (dx, dy, a) in enumerate(((-4.6, -0.75, math.pi / 2), (-4.0, -0.75, math.pi / 2), (-4.6, 0.75, -math.pi / 2), (-4.0, 0.75, -math.pi / 2))):
        chair(X, dx, ty + dy, a, X.c('GheNhua_%d' % (k % 2), ['#E53935', '#1E88E5'][k % 2], rough=0.5))
    X.cyl((-4.3, ty, FLOOR + 0.76), 0.18, 0.06, X.c('LongBan', '#D9C9A0'), r2=0.2)
    bowls_stack(X, -4.7, ty - 0.2, FLOOR + 0.76, n=3)
    X.ab((-6.1, X.yr - 0.7, FLOOR), (-5.4, X.yr - 0.05, FLOOR + 1.75), X.c('TuLanh', '#D8DDE0', rough=0.3, metal=0.3))
    X.box((-5.38, X.yr - 0.6, FLOOR + 1.2), (0.03, 0.03, 0.4), X.steelD)
    for k in range(4): X.box((-5.8 + k * 0.15, X.yr - 0.38, FLOOR + 1.8), (0.12, 0.12, 0.1), X.c('Nam_%d' % k, ['#E53935', '#FDD835', '#43A047', '#1E88E5'][k]))
    # cầu thang gạch lên lầu
    for k in range(12):
        X.ab((BACK + 0.05 + k * 0.26, X.yl + 0.05, FLOOR + k * 0.3), (BACK + 0.32 + k * 0.26, X.yl + 0.95, FLOOR + (k + 1) * 0.3), X.c('BacThang', '#C9B89A'))
    X.rod((BACK + 0.1, X.yl + 0.98, FLOOR + 1.0), (BACK + 3.2, X.yl + 0.98, FLOOR + 4.3), 0.025, X.woodD)
    ceiling_fan(X, -4.3, ty); wall_fan(X, -2.8, X.yr, 1, 3.0)
    plant(X, -0.95, X.yr - 0.35, 1.0, '#6D4C41')

def furnish_NhaO_PhongKhach(X, rnd):
    """Phòng khách hiện đại: sofa vải chữ L, thảm, bàn kính, kệ TV ốp gỗ, bể cá, đèn cây."""
    _home_common(X, rnd, altar=False)
    sy = X.yr - 0.5
    sofa = X.c('SofaVai', '#8A9BA8', rough=0.95)
    X.ab((-6.4, sy - 0.45, FLOOR + 0.1), (-3.6, sy + 0.4, FLOOR + 0.45), sofa)
    X.ab((-6.4, sy + 0.2, FLOOR + 0.45), (-3.6, sy + 0.4, FLOOR + 0.9), sofa)
    X.ab((-6.4, sy - 1.6, FLOOR + 0.1), (-5.6, sy - 0.45, FLOOR + 0.45), sofa)
    X.ab((-6.4, sy - 1.6, FLOOR + 0.45), (-6.2, sy - 0.45, FLOOR + 0.9), sofa)
    for k in range(3): X.box((-5.8 + k * 0.8, sy + 0.1, FLOOR + 0.65), (0.4, 0.15, 0.4), X.c('Goi_%d' % k, ['#F2C14E', '#E8E2D2', '#C0645A'][k]), rx=0.2)
    X.ab((-5.4, sy - 1.6, FLOOR + 0.001), (-3.4, sy - 0.2, FLOOR + 0.008), X.c('Tham', '#C9B79C', rough=1))
    X.ab((-5.0, sy - 1.2, FLOOR + 0.38), (-3.8, sy - 0.6, FLOOR + 0.4), X.glass)
    for xx in (-4.95, -3.85):
        for yy in (sy - 1.15, sy - 0.65): X.box((xx, yy, FLOOR + 0.19), (0.03, 0.03, 0.38), X.black)
    X.ab((-6.6, X.yl + 0.02, FLOOR), (-2.8, X.yl + 0.06, FLOOR + 2.6), X.c('OpGo', '#8A6242'))
    X.ab((-6.0, X.yl + 0.06, FLOOR), (-3.4, X.yl + 0.45, FLOOR + 0.42), X.c('KeTVTrang', '#F0EDE6'))
    X.box((-4.7, X.yl + 0.1, FLOOR + 1.35), (1.55, 0.05, 0.88), X.black); X.box((-4.7, X.yl + 0.13, FLOOR + 1.35), (1.5, 0.004, 0.83), X.c('ManTV2', '#3A7AB0', emit=0.9))
    X.ab((-2.6, X.yl + 0.05, FLOOR), (-1.4, X.yl + 0.5, FLOOR + 0.75), X.woodD)
    X.ab((-2.55, X.yl + 0.08, FLOOR + 0.75), (-1.45, X.yl + 0.47, FLOOR + 1.35), X.c('NuocBe', '#7FC8E8', alpha=0.45, rough=0.05))
    for k in range(5): X.sph((-2.4 + k * 0.22, X.yl + 0.28, FLOOR + 0.95 + (k % 3) * 0.1), 0.03, X.c('Ca_%d' % (k % 2), ['#FF7043', '#FFD54F'][k % 2]), seg=6, rings=4, scale=(1.8, 0.6, 1))
    X.cyl((-6.6, sy - 0.2, FLOOR), 0.15, 0.03, X.black); X.rod((-6.6, sy - 0.2, FLOOR), (-6.6, sy - 0.2, FLOOR + 1.6), 0.015, X.black)
    X.cyl((-6.6, sy - 0.2, FLOOR + 1.55), 0.18, 0.28, X.c('ChaoDenCay', '#F5E9D0', emit=1.5), r2=0.12, seg=14)
    motorbike(X, -1.6, X.yr - 0.7, math.pi + 0.1, '#ECEFF1')
    plant(X, -0.95, X.yl + 0.35, 1.3, '#3A3A3A'); clock(X, -4.7, X.yl, -1, z=3.4)

def furnish_NhaO_ViVong(X, rnd):
    """Nhà ông bà: võng xếp, bộ bàn ghế may_, chậu bonsai, lồng chim, tủ gỗ cũ, xe đạp."""
    _home_common(X, rnd)
    hy = X.yl + X.inner * 0.6
    st = X.c('KhungVong', '#3E6E9A', metal=0.4)
    for xx in (-5.6, -3.0):
        X.rod((xx, hy - 0.35, FLOOR), (xx, hy, FLOOR + 1.35), 0.025, st); X.rod((xx, hy + 0.35, FLOOR), (xx, hy, FLOOR + 1.35), 0.025, st)
    X.rod((-5.6, hy, FLOOR + 0.05), (-3.0, hy, FLOOR + 0.05), 0.025, st)
    pts = [(-5.5 + k * 0.26, hy, FLOOR + 1.3 - 0.75 * math.sin(math.pi * k / 10)) for k in range(11)]
    for a, b in zip(pts, pts[1:]):
        X.box(((a[0] + b[0]) / 2, hy, (a[2] + b[2]) / 2), (0.28, 0.65, 0.02), X.c('LuoiVong', '#E57373', rough=0.95), ry=math.atan2(b[2] - a[2], b[0] - a[0]) * -1)
    may_ = X.c('May', '#C9A46A', rough=0.9)
    X.cyl((-2.2, X.yl + 0.9, FLOOR), 0.35, 0.55, may_, seg=14, r2=0.4); X.cyl((-2.2, X.yl + 0.9, FLOOR + 0.55), 0.42, 0.04, X.glass, seg=14)
    for a in (0.6, 2.5, 4.4):
        cx, cy = -2.2 + math.cos(a) * 0.7, X.yl + 0.9 + math.sin(a) * 0.7
        if cy < X.yl + 0.25: continue
        X.cyl((cx, cy, FLOOR), 0.25, 0.42, may_, seg=12, r2=0.28); X.cyl((cx - math.cos(a) * 0.15, cy - math.sin(a) * 0.15, FLOOR + 0.42), 0.27, 0.45, may_, seg=12, r2=0.3)
    for k in range(3):
        bx = -6.8 + k * 0.6
        X.cyl((bx, X.yr - 0.4, FLOOR), 0.2, 0.18, X.c('ChauSu', '#3E6E9A'), r2=0.24, seg=12)
        X.rod((bx, X.yr - 0.4, FLOOR + 0.18), (bx + 0.1, X.yr - 0.4, FLOOR + 0.55), 0.03, X.c('ThanBonsai', '#5A3E22'))
        for j in range(4): X.sph((bx + 0.1 + (j - 1.5) * 0.12, X.yr - 0.4, FLOOR + 0.62 + (j % 2) * 0.08), 0.1, X.c('La2', '#4E8F35'), seg=7, rings=4, scale=(1.3, 1.0, 0.6))
    X.rod((-1.6, X.yr - 0.5, CEIL), (-1.6, X.yr - 0.5, CEIL - 1.3), 0.005, X.black, seg=4)
    for k in range(12):
        a = k * math.pi / 6; X.rod((-1.6 + math.cos(a) * 0.17, X.yr - 0.5 + math.sin(a) * 0.17, CEIL - 1.9), (-1.6 + math.cos(a) * 0.05, X.yr - 0.5 + math.sin(a) * 0.05, CEIL - 1.35), 0.004, X.c('LongChim', '#B08A4A'), seg=3)
    X.cyl((-1.6, X.yr - 0.5, CEIL - 1.92), 0.18, 0.03, X.c('LongChim', '#B08A4A'), seg=12)
    X.sph((-1.6, X.yr - 0.5, CEIL - 1.75), 0.04, X.c('Chim', '#F2C14E'), seg=6, rings=4)
    X.ab((-7.6, X.yr - 0.5, FLOOR), (-6.8, X.yr - 0.05, FLOOR + 1.9), X.c('TuGoCu', '#6B3E1E'))
    for k in range(2): X.ab((-7.55 + k * 0.4, X.yr - 0.51, FLOOR + 0.1), (-7.2 + k * 0.4, X.yr - 0.5, FLOOR + 1.8), X.c('CuaTuCu', '#5A3418'))
    for dx in (-0.5, 0.5): X.cyl((-1.4 + dx, X.yl + 2.3 if X.inner > 3.6 else X.yl + 1.6, FLOOR + 0.33), 0.33, 0.04, X.black, seg=16, axis='Y')
    X.rod((-1.9, X.yl + 2.3 if X.inner > 3.6 else X.yl + 1.6, FLOOR + 0.33), (-1.1, X.yl + 2.3 if X.inner > 3.6 else X.yl + 1.6, FLOOR + 0.75), 0.02, X.c('KhungXeDap', '#2E7D32'))
    ceiling_fan(X, -4.3, hy)


# ------------------------------------------------------------------------------------------------ 4 loại riêng cho nhà nổi bật (21.272)
EXTRA_TYPES = ['HaiSan', 'NhaSach', 'DienMay', 'LauBo']
STYLE.update({
    'HaiSan': ('#E6EEF0', '#1D4FA0', '#CDD6D8', '#BAC4C6', 'cool'), 'NhaSach': ('#F1EBDD', '#8A5A2B', '#D9CDB4', '#C8BB9E', 'warm'),
    'DienMay': ('#F2F5F8', '#2154A6', '#E7EAEE', '#D7DCE2', 'cool'), 'LauBo': ('#EFE2CC', '#8A2B2B', '#CFC2A8', '#BDAE92', 'warm'),
})

def aquarium(X, x0, x1, y_wall, side, z0, h, rnd, kind='fish'):
    """Bể kính hải sản sống dọc tường: khung inox, nước xanh trong, sủi bọt, cá/cua/tôm/ốc."""
    d = 0.55; yi = y_wall - side * d
    X.ab((x0, min(y_wall, yi), FLOOR), (x1, max(y_wall, yi), z0), X.steel)
    X.ab((x0 + 0.03, min(y_wall, yi) + 0.03, z0), (x1 - 0.03, max(y_wall, yi) - 0.03, z0 + h * 0.85), X.c('NuocBeHaiSan', '#5FB3D9', rough=0.05, alpha=0.45))
    X.ab((x0, min(y_wall, yi), z0 + h), (x1, max(y_wall, yi), z0 + h + 0.04), X.steel)
    for xx in (x0, x1): X.ab((xx - 0.02, min(y_wall, yi), z0), (xx + 0.02, max(y_wall, yi), z0 + h), X.steel)
    X.ab((x0, yi - side * 0.005, z0 + h * 0.86), (x1, yi + side * 0.005, z0 + h * 0.9), X.c('DenBe', '#BFF0FF', emit=3.0))
    n = int((x1 - x0) / 0.22)
    for k in range(n):
        xx = x0 + 0.15 + k * (x1 - x0 - 0.3) / max(1, n - 1)
        z = z0 + 0.06 + (k % 3) * h * 0.22
        if kind == 'fish':
            X.sph((xx, y_wall - side * d * 0.5, z + 0.1), 0.07, X.c('CaBe_%d' % (k % 3), ['#E57373', '#9E9E9E', '#FFB74D'][k % 3]), seg=8, rings=5, scale=(2.2, 0.6, 0.9))
        elif kind == 'crab':
            X.sph((xx, y_wall - side * d * 0.5, z0 + 0.06), 0.08, X.c('Cua', '#C0392B'), seg=8, rings=4, scale=(1.3, 1.0, 0.45))
            for s2 in (-1, 1): X.rod((xx + s2 * 0.08, y_wall - side * d * 0.5, z0 + 0.06), (xx + s2 * 0.16, y_wall - side * d * 0.4, z0 + 0.03), 0.012, X.c('Cua', '#C0392B'), seg=3)
        else:
            X.sph((xx, y_wall - side * d * 0.5, z0 + 0.04), 0.045, X.c('Oc_%d' % (k % 2), ['#8D6E63', '#D7CCC8'][k % 2]), seg=7, rings=4, scale=(1.0, 1.0, 1.3))
    for k in range(6): X.sph((x0 + 0.2 + k * (x1 - x0 - 0.4) / 5, y_wall - side * d * 0.8, z0 + h * (0.3 + 0.1 * (k % 4))), 0.015, X.c('BongBong', '#E8FBFF', emit=0.5), seg=5, rings=3)

def furnish_HaiSan(X, rnd):
    """Quán hải sản/ốc: bể kính hải sản sống dọc tường trái, quầy đá bày hải sản sát kính, bàn tròn inox + ghế nhựa,
    bếp nướng than, thùng đá bia, bảng giá theo ký."""
    shell(X, X.kind)
    aquarium(X, -6.8, -4.6, X.yl, -1, FLOOR + 0.7, 0.75, rnd, 'fish')
    aquarium(X, -4.4, -2.8, X.yl, -1, FLOOR + 0.7, 0.75, rnd, 'crab')
    aquarium(X, -6.8, -4.6, X.yl, -1, FLOOR + 1.6, 0.55, rnd, 'snail')
    # quầy đá bày hải sản sát kính (nghiêng ra đường)
    qy = X.yl + 0.9; ql = min(2.0, X.inner * 0.4)
    X.ab((-1.6, qy - ql / 2, FLOOR), (-0.85, qy + ql / 2, FLOOR + 0.8), X.steel)
    X.box((-1.2, qy, FLOOR + 0.86), (0.72, ql, 0.12), X.c('DaVien', '#EAF6FB', rough=0.2), ry=0.18)
    cols = ['#E57373', '#FF8A65', '#BCAAA4', '#90A4AE', '#F06292']
    for k in range(int(ql / 0.18)):
        yy = qy - ql / 2 + 0.12 + k * 0.18
        X.sph((-1.25, yy, FLOOR + 0.95), 0.06, X.c('HaiSan_%d' % (k % 5), cols[k % 5]), seg=7, rings=4, scale=(1.6, 0.8, 0.5))
        X.sph((-1.0, yy + 0.05, FLOOR + 0.92), 0.04, X.c('Tom', '#FF7043'), seg=6, rings=3, scale=(1.8, 0.6, 0.6))
    X.ab((-2.6, X.yl + 0.05, FLOOR), (-1.9, X.yl + 0.6, FLOOR + 0.85), X.c('BepNuong', '#3A2C22'))
    for k in range(5): X.rod((-2.55, X.yl + 0.1 + k * 0.1, FLOOR + 0.9), (-1.95, X.yl + 0.1 + k * 0.1, FLOOR + 0.9), 0.006, X.steelD, seg=3)
    X.ab((-2.55, X.yl + 0.1, FLOOR + 0.82), (-1.95, X.yl + 0.55, FLOOR + 0.86), X.c('ThanHong', '#FF5A1F', emit=2.5))
    # bàn tròn inox + ghế nhựa
    cols2 = [X.yl + X.inner * 0.55, X.yl + X.inner * 0.82] if X.inner > 5.0 else [X.yl + X.inner * 0.66]
    for i, tx in enumerate((-2.9, -4.4, -5.9)):
        for ty in cols2:
            X.cyl((tx, ty, FLOOR), 0.25, 0.03, X.steelD); X.rod((tx, ty, FLOOR), (tx, ty, FLOOR + 0.7), 0.03, X.steelD)
            X.cyl((tx, ty, FLOOR + 0.7), 0.5, 0.03, X.steel, seg=16)
            X.cyl((tx, ty, FLOOR + 0.73), 0.16, 0.03, X.white, r2=0.2, seg=12)
            X.sph((tx, ty, FLOOR + 0.77), 0.09, X.c('Oc', '#8D6E63'), seg=7, rings=4, scale=(1.3, 1.3, 0.6))
            condiments(X, tx + 0.25, ty - 0.1, rnd)
            for a in (0.3, 1.9, 3.5, 5.0):
                sx, sy = tx + math.cos(a) * 0.72, ty + math.sin(a) * 0.72
                if X.yl + 0.25 < sy < X.yr - 0.25: stool(X, sx, sy, X.c('Ghe_#1F6FB5' if (i + int(a)) % 2 else 'Ghe_#C8262A', '#1F6FB5' if (i + int(a)) % 2 else '#C8262A'))
    X.ab((-7.6, X.yr - 0.6, FLOOR), (-6.8, X.yr - 0.05, FLOOR + 0.6), X.c('ThungDa', '#1565C0'))
    for k in range(6): X.cyl((-7.5 + (k % 3) * 0.25, X.yr - 0.3, FLOOR + 0.6), 0.035, 0.24, X.c('ChaiBia', '#2E7D32'), seg=6)
    menu_board(X, -4.6, X.yr, 1, (X.title or 'HẢI SẢN').upper(), ['Ốc hương ..... 250k/kg', 'Cua gạch ..... 650k/kg', 'Tôm sú ........ 450k/kg', 'Nghêu hấp ..... 60k', 'Bia lạnh ........ 20k'], bg='#1D4FA0', fg='#FFFFFF', acc='#FFD54F', w=2.2, h=1.2, z=2.8)
    wall_fan(X, -6.6, X.yr, 1, 3.3); clock(X, -2.4, X.yr, 1, z=3.4); ceiling_fan(X, -4.4, X.yl + X.inner * 0.6)
    altar_than_tai(X, -7.8, X.yl + X.inner * 0.5, 0)
    back_sign(X, X.title or 'Hải Sản', 'Tươi sống mỗi ngày', '#1D4FA0', '#FFFFFF', z=3.1)

def furnish_NhaSach(X, rnd):
    """Nhà sách: kệ gỗ cao kín sách hai tường (gáy nhiều màu), bàn trưng sách mới giữa, ghế đọc, quầy thu ngân, đèn thả."""
    shell(X, X.kind)
    pal = ['#C62828', '#1565C0', '#2E7D32', '#F9A825', '#6A1B9A', '#EF6C00', '#00838F', '#ECEFF1', '#4E342E', '#AD1457']
    for side, yw, x0 in ((-1, X.yl, -7.8), (1, X.yr, -7.8)):
        x1 = -1.2 if side < 0 else -2.4
        d = 0.38; yi = yw - side * d
        X.ab((x0, min(yw, yw - side * 0.025), FLOOR), (x1, max(yw, yw - side * 0.025), FLOOR + 2.6), X.c('LongKe', '#5A3A1C'))     # tấm hậu mỏng sát tường
        X.ab((x0, min(yw, yi), FLOOR + 2.58), (x1, max(yw, yi), FLOOR + 2.64), X.c('GoKeSach', '#8A5A2B'))                       # nóc kệ
        for lv in range(6):
            z = FLOOR + 0.12 + lv * 0.42
            X.ab((x0, min(yw, yi), z - 0.025), (x1, max(yw, yi), z), X.c('GoKeSach', '#8A5A2B'))
            xx = x0 + 0.06
            while xx < x1 - 0.06:
                bw = rnd.uniform(0.03, 0.06); bh = rnd.uniform(0.22, 0.34)
                X.box((xx + bw / 2, yw - side * d * 0.5, z + bh / 2), (bw - 0.004, d * 0.8, bh), X.c('GaySach_%d' % (int(xx * 37) % len(pal)), pal[int(xx * 37) % len(pal)], rough=0.7))
                xx += bw
                if rnd.random() < 0.03: xx += 0.12
        for xx in range(int((x1 - x0) / 0.9) + 1): X.ab((x0 + xx * 0.9 - 0.02, min(yw, yi), FLOOR), (x0 + xx * 0.9 + 0.02, max(yw, yi), FLOOR + 2.6), X.c('GoKeSach', '#8A5A2B'))
    ty = X.yl + X.inner * 0.5
    for tx in (-3.2, -5.4):
        X.ab((tx - 0.7, ty - 0.45, FLOOR), (tx + 0.7, ty + 0.45, FLOOR + 0.75), X.woodL)
        for k in range(10):
            c = pal[(k * 3) % len(pal)]
            X.box((tx - 0.55 + (k % 5) * 0.27, ty - 0.2 + (k // 5) * 0.4, FLOOR + 0.79 + (k % 2) * 0.03), (0.22, 0.3, 0.06 + (k % 3) * 0.03), X.c('BiaSach_' + c, c, rough=0.6))
        X.box((tx, ty, FLOOR + 1.0), (0.04, 0.3, 0.4), X.c('BangSachMoi', '#F9A825'))
    for k in range(2):
        chair(X, -1.6, X.yr - 0.7 - k * 0.8, math.pi, X.c('GheDoc', '#2E7D32', rough=0.8))
    counter(X, -7.0, X.yr - 0.9, 1.2, lx=0.7, h=1.0, m=X.c('QuaySach', '#5A3A1C'), top=X.woodL)
    register(X, -7.0, X.yr - 0.7)
    for k in range(3): X.rod((-2.8 - k * 1.8, ty, CEIL), (-2.8 - k * 1.8, ty, CEIL - 0.6), 0.005, X.black, seg=3); X.cyl((-2.8 - k * 1.8, ty, CEIL - 0.8), 0.04, 0.22, X.c('ChaoDenSach', '#2E7D32'), r2=0.24, seg=12); X.sph((-2.8 - k * 1.8, ty, CEIL - 0.78), 0.06, X.c('BongDenAm', '#FFD58A', emit=8.0), seg=6, rings=4)
    poster(X, -3.6, X.yr, 1, 0.8, 1.0, 3.2, '#ECEFF1', '#C62828'); plant(X, -0.95, X.yr - 0.35, 1.1, '#E8E2D2')
    back_sign(X, X.title or 'Nhà Sách', 'Sách – Văn phòng phẩm', '#E3B323', '#2B1A0E', z=3.3)

def furnish_DienMay(X, rnd):
    """Điện máy: tường TV đủ cỡ (màn sáng), hàng tủ lạnh + máy giặt trưng dưới sàn, máy lạnh treo tường, quạt cây, quầy tư vấn."""
    shell(X, X.kind)
    for k, (sz, xx) in enumerate(((1.4, -6.6), (1.1, -5.0), (0.9, -3.7), (1.2, -2.4))):
        h = sz * 0.58
        X.box((xx, X.yl + 0.06, 2.3), (sz + 0.06, 0.06, h + 0.06), X.black)
        X.box((xx, X.yl + 0.095, 2.3), (sz, 0.006, h), X.c('ManTV_%d' % k, ['#2B6CB0', '#38A169', '#D69E2E', '#805AD5'][k], emit=1.4))
        X.box((xx, X.yl + 0.1, 1.85 - h / 2 + 0.6), (sz * 0.9, 0.005, 0.03), X.c('NhanGia', '#E53935'))
    for k in range(int(4.6 / 0.8)):
        xx = -6.8 + k * 0.8
        X.ab((xx - 0.33, X.yl + 0.15, FLOOR), (xx + 0.33, X.yl + 0.8, FLOOR + (1.7 if k % 2 == 0 else 0.9)), X.c('ThanTu_%d' % (k % 3), ['#E9ECEF', '#B0B7BF', '#2F3438'][k % 3], metal=0.3, rough=0.3))
        if k % 2 == 1: X.cyl((xx, X.yl + 0.805, FLOOR + 0.55), 0.22, 0.02, X.c('CuaMayGiat', '#9AD1E8', rough=0.05), seg=14, axis='Y')
        else: X.box((xx + 0.25, X.yl + 0.81, FLOOR + 1.1), (0.03, 0.02, 0.4), X.steel)
    for k in range(3): ac_unit(X, X.yr, 1, x=-6.6 + k * 1.3)
    for k in range(3):
        fx = -2.6 - k * 1.1; fy = X.yr - 0.6
        X.cyl((fx, fy, FLOOR), 0.2, 0.03, X.white); X.rod((fx, fy, FLOOR), (fx, fy, FLOOR + 1.1), 0.02, X.white)
        X.cyl((fx, fy - 0.02, FLOOR + 1.25), 0.24, 0.07, X.c('LongQuat', '#4A6E8A'), seg=14, axis='Y')
    counter(X, -7.2, X.yr - 1.4, 1.4, lx=0.7, h=1.0, m=X.c('QuayDienMay', '#2154A6'), top=X.white)
    register(X, -7.2, X.yr - 1.2)
    X.box((-4.4, X.yr - 0.02, 3.2), (2.6, 0.03, 0.7), X.c('BangKhuyenMai', '#E53935', emit=0.8))
    X.L.text(X.col, 'GIẢM GIÁ ĐẾN 50%', (-4.4, X.yr - 0.04, 3.2), 0.2, X.c('ChuVang', '#FFEB3B'), rot=(math.pi / 2, 0, 0), extrude=0.002)
    back_sign(X, X.title or 'Điện Máy', 'Trả góp 0% – Giao hàng tận nơi', '#2154A6', '#FFFFFF', z=3.4)

def furnish_LauBo(X, rnd):
    """Quán lẩu: mỗi bàn có bếp ga mini + nồi lẩu bốc màu nước dùng, đĩa thịt bò, rổ rau, ghế đẩu; tủ bia, bảng món."""
    shell(X, X.kind)
    cols = [X.yl + X.inner * 0.3, X.yl + X.inner * 0.72] if X.inner > 4.6 else [X.yl + X.inner * 0.55]
    for i, tx in enumerate((-1.9, -3.5, -5.1, -6.6)):
        for ty in cols:
            steel_table(X, tx, ty, ly=0.8, lx=1.0, h=0.72, top=X.c('MatBanLau', '#8D6E63', rough=0.5))
            X.box((tx, ty, FLOOR + 0.78), (0.32, 0.32, 0.1), X.c('BepMini', '#2E3236', metal=0.4))
            X.cyl((tx, ty, FLOOR + 0.83), 0.17, 0.14, X.steel, seg=12)
            X.cyl((tx, ty, FLOOR + 0.95), 0.16, 0.015, X.c('NuocLau', '#D84315', rough=0.15), seg=12)
            X.box((tx - 0.3, ty - 0.25, FLOOR + 0.76), (0.22, 0.16, 0.025), X.white)
            X.box((tx - 0.3, ty - 0.25, FLOOR + 0.78), (0.18, 0.12, 0.015), X.c('ThitBo', '#9A3B2E'))
            X.cyl((tx + 0.3, ty + 0.25, FLOOR + 0.74), 0.11, 0.08, X.c('RoRau', '#4CAF50'), r2=0.13, seg=8)
            for dx in (-0.3, 0.3):
                for sy in (-1, 1):
                    yy = ty + sy * 0.62
                    if X.yl + 0.25 < yy < X.yr - 0.25: stool(X, tx + dx, yy, X.c('Ghe_#C8262A', '#C8262A'))
    fridge(X, -7.4, X.yr, 1, w=0.7, h=1.85, brand='#2E7D32', rnd=rnd, drinks=['#2E7D32', '#F2A900', '#C8262A'])
    menu_board(X, -4.4, X.yr, 1, (X.title or 'LẨU BÒ').upper(), ['Lẩu bò nhỏ ..... 199k', 'Lẩu bò lớn ...... 299k', 'Bò thêm ........... 80k', 'Rau nấm ............ 30k', 'Bia lạnh ........... 20k'], bg='#3A1A12', fg='#F7E7B8', acc='#FFB74D', w=2.1, h=1.2, z=2.8)
    food_frames(X, X.yl, -1, -3.0, 3, ['#F2E2C0', '#E8D2B0', '#F0E6D0'])
    ceiling_fan(X, -3.0, X.yl + X.inner * 0.5); ceiling_fan(X, -5.6, X.yl + X.inner * 0.5); wall_fan(X, -6.6, X.yl, -1, 3.3)
    altar_than_tai(X, -7.8, X.yl + X.inner * 0.3, 0)
    back_sign(X, X.title or 'Lẩu Bò', 'Nước dùng ninh xương', '#8A2B2B', '#F7E7B8', z=3.2)


# ------------------------------------------------------------------------------------------------ 21.285: Cù Lao Petshop
# Biển «Cù Lao Petshop» + «Spa-Lưu Trú-Thú y» (50 % nhà không biển). Từ ngoài vào: khu spa (bàn cắt tỉa + chó, máy sấy,
# bồn tắm) và kệ thức ăn/đồ chơi + nhà cây cho mèo; giữa: quầy thu ngân + nhân viên; trong: dãy chuồng lưu trú hai tầng
# (có chó/mèo), góc thú y (bàn khám inox, tủ thuốc, áp phích); tường hậu: bảng tên tiệm. Dấu chân thú trên sàn.
PETSHOP_TEAL, PETSHOP_ORANGE = '#16808A', '#F28C28'
STYLE['PetShop'] = ('#EAF6F4', PETSHOP_TEAL, '#F4F1EA', '#E4DED2', 'cool')

def pet(X, x, y, z, kind, col, yaw=0.0, s=1.0, sit=False):
    """Chó/mèo khối tròn: thân bầu dục, đầu, mõm, tai, 4 chân, đuôi. yaw: hướng đầu (rad, 0 = +x ra đường)."""
    ca, sa = math.cos(yaw), math.sin(yaw)
    def Pt(dx, dy, dz): return (x + (dx * ca - dy * sa) * s, y + (dx * sa + dy * ca) * s, z + dz * s)
    m = X.c('Long_' + col, col, rough=0.9); dark = X.c('MuiThu', '#2A2522', rough=0.6)
    cat = kind == 'cat'
    bl, bh = (0.30, 0.13) if cat else (0.36, 0.17)
    if sit:
        X.sph(Pt(-0.02, 0, bh * 1.6), bh * 1.15 * s, m, scale=(0.9, 0.85, 1.25))
        head = Pt(0.06, 0, bh * 3.0)
    else:
        X.sph(Pt(0, 0, bh * 1.75), bh * s, m, scale=(bl / bh, 0.85, 0.85))
        for dx in (bl * 0.6, -bl * 0.6):
            for dy in (-bh * 0.45, bh * 0.45):
                X.cyl(Pt(dx, dy, 0), 0.035 * s, bh * 1.3 * s, m, seg=6)
        head = Pt(bl * 0.95, 0, bh * 2.6)
    hr = (0.10 if cat else 0.12) * s
    X.sph(head, hr, m, scale=(1.05, 1, 0.95))
    hx, hy, hz = head
    X.sph((hx + ca * hr * 0.85, hy + sa * hr * 0.85, hz - hr * 0.25), hr * (0.38 if cat else 0.5), m, scale=(1.2, 1, 0.8))   # mõm
    X.sph((hx + ca * hr * 1.25, hy + sa * hr * 1.25, hz - hr * 0.15), hr * 0.13, dark)                                     # mũi
    for sd in (-1, 1):
        ex, ey = hx + (-sa) * sd * hr * 0.55, hy + ca * sd * hr * 0.55
        if cat: X.cyl((ex, ey, hz + hr * 0.7), hr * 0.32, hr * 0.5, m, seg=4, r2=0.005)                                   # tai nhọn
        else: X.sph((ex - ca * hr * 0.1, ey - sa * hr * 0.1, hz + hr * 0.35), hr * 0.38, m, scale=(0.5, 0.6, 1.3))          # tai cụp
        X.sph((hx + ca * hr * 0.8 + (-sa) * sd * hr * 0.35, hy + sa * hr * 0.8 + ca * sd * hr * 0.35, hz + hr * 0.2), hr * 0.12, dark)  # mắt
    tail = Pt(-bl * 1.0, 0, bh * (1.6 if sit else 2.0)); tip = Pt(-bl * 1.5, 0, bh * (2.3 if cat else 2.8))
    X.rod(tail, tip, 0.03 * s, m)

def cage_row(X, x0, x1, y_wall, side, z0, h, n, rnd, pets=True):
    """Dãy chuồng lưu trú dọc tường bên: khung trắng, song inox mặt trước, khay đáy, thú bên trong (có ô trống)."""
    frame = X.c('KhungChuong', '#F2F4F3', rough=0.4); bar = X.c('SongChuong', '#B9C1C6', rough=0.3, metal=0.8)
    tray = X.c('KhayChuong', '#7FB8C2', rough=0.5); d = 0.62
    yi = y_wall - side * d; cw = (x1 - x0) / n
    for k in range(n):
        a = x0 + k * cw; b = a + cw
        X.ab((a, min(y_wall, yi), z0), (b, max(y_wall, yi), z0 + 0.04), tray)                                  # đáy
        X.ab((a, min(y_wall, yi), z0 + h - 0.03), (b, max(y_wall, yi), z0 + h), frame)                         # nóc
        X.box((a, (y_wall + yi) / 2, z0 + h / 2), (0.04, d, h), frame)                                          # vách ngăn
        nb = max(5, int(cw / 0.09))
        for j in range(1, nb):
            xx = a + (b - a) * j / nb
            X.rod((xx, yi, z0 + 0.05), (xx, yi, z0 + h - 0.04), 0.008, bar, seg=4)
        X.box(((a + b) / 2, yi, z0 + h * 0.55), (cw - 0.06, 0.02, 0.03), bar)                                   # chốt ngang
        if pets and rnd.random() < 0.75:
            kind = 'cat' if rnd.random() < 0.4 else 'dog'
            col = rnd.choice(['#F2E2C4', '#C98A4B', '#3A3230', '#FFFFFF', '#9C9C9C', '#E0A35C'])
            pet(X, (a + b) / 2, (y_wall + yi) / 2, z0 + 0.04, kind, col, yaw=-side * math.pi / 2, s=0.72, sit=rnd.random() < 0.5)
        X.box(((a + b) / 2, yi - side * 0.01, z0 + h - 0.1), (0.16, 0.01, 0.07), X.c('TheTen', '#FFF8E1'))      # thẻ tên
    X.box((x1, (y_wall + yi) / 2, z0 + h / 2), (0.04, d, h), frame)

def paw(X, x, y, m, s=0.12):
    """Dấu chân thú dán sàn."""
    X.cyl((x, y, FLOOR + 0.002), s * 0.5, 0.004, m, seg=10)
    for dx, dy in ((0.55, -0.45), (0.75, -0.15), (0.75, 0.15), (0.55, 0.45)):
        X.cyl((x + dx * s, y + dy * s, FLOOR + 0.002), s * 0.18, 0.004, m, seg=8)

def furnish_PetShop(X, rnd):
    shell(X, 'PetShop')
    teal = X.c('XanhPetshop', PETSHOP_TEAL, rough=0.45); orange = X.c('CamPetshop', PETSHOP_ORANGE, rough=0.45)
    yl, yr, inner = X.yl, X.yr, X.inner
    rotL = (math.pi / 2, 0, math.pi); rotR = (math.pi / 2, 0, 0)          # chữ trên tường trái (mặt +y) / phải (mặt −y)
    # --- khu spa (trước, bên trái): bàn cắt tỉa inox + cần treo, chó đứng trên bàn, máy sấy đứng, bồn tắm ốp gạch
    gx, gy = -1.55, yl + 0.55
    X.ab((gx - 0.45, gy - 0.3, FLOOR + 0.82), (gx + 0.45, gy + 0.3, FLOOR + 0.86), X.steel)
    X.ab((gx - 0.42, gy - 0.27, FLOOR + 0.86), (gx + 0.42, gy + 0.27, FLOOR + 0.875), X.c('ThamCaoSu', '#2F3A44', rough=0.9))
    for dx in (-0.35, 0.35): X.rod((gx + dx, gy, FLOOR), (gx + dx, gy, FLOOR + 0.82), 0.025, X.steelD)
    X.ab((gx - 0.4, gy - 0.25, FLOOR + 0.02), (gx + 0.4, gy + 0.25, FLOOR + 0.06), X.steelD)
    X.rod((gx - 0.42, gy - 0.25, FLOOR + 0.86), (gx - 0.42, gy - 0.25, FLOOR + 1.85), 0.02, X.steel)
    X.rod((gx - 0.42, gy - 0.25, FLOOR + 1.85), (gx, gy - 0.25, FLOOR + 1.85), 0.02, X.steel)
    pet(X, gx, gy, FLOOR + 0.875, 'dog', '#FFFFFF', yaw=0.0, s=1.0)                                       # chó trắng đang spa
    X.rod((gx, gy - 0.25, FLOOR + 1.85), (gx + 0.33, gy, FLOOR + 1.36), 0.006, orange)                    # dây giữ
    sx, sy = -1.0, yl + 1.25                                                                               # máy sấy đứng
    X.cyl((sx, sy, FLOOR), 0.22, 0.04, X.steelD, seg=10); X.rod((sx, sy, FLOOR), (sx, sy, FLOOR + 1.4), 0.025, X.steel)
    X.box((sx, sy - 0.12, FLOOR + 1.45), (0.2, 0.3, 0.22), X.c('MaySay', '#E9EEF2', rough=0.4))
    X.cyl((sx, sy - 0.35, FLOOR + 1.45), 0.07, 0.25, teal, seg=8, axis='Y')
    bx = -2.85                                                                                             # bồn tắm thú
    X.ab((bx - 0.55, yl, FLOOR), (bx + 0.55, yl + 0.7, FLOOR + 0.85), X.c('OpBon', '#DDEFF1', rough=0.3))
    X.ab((bx - 0.48, yl + 0.07, FLOOR + 0.6), (bx + 0.48, yl + 0.63, FLOOR + 0.86), X.c('LongBon', '#B9DDE2', rough=0.15))
    X.rod((bx, yl + 0.04, FLOOR + 0.86), (bx, yl + 0.04, FLOOR + 1.6), 0.015, X.steel)
    X.cyl((bx + 0.05, yl + 0.18, FLOOR + 1.55), 0.05, 0.03, X.steel, seg=8, axis='Y')
    for k in range(3):                                                                                     # chai dầu tắm
        X.cyl((bx - 0.35 + k * 0.12, yl + 0.05, FLOOR + 0.86), 0.035, 0.18, X.c('ChaiSpa_%d' % k, [PETSHOP_ORANGE, '#7FC97F', '#F7A6C2'][k]), seg=8)
    poster(X, -1.9, yl, -1, 0.7, 0.9, 2.55, '#FFF3E0', PETSHOP_ORANGE)                                    # áp phích SPA
    X.text('SPA', (-1.9, yl + 0.035, 2.95), 0.16, orange, rot=rotL)
    # --- kệ thức ăn/đồ chơi (trước, bên phải) + nhà cây cho mèo
    pal = ['#F28C28', '#16808A', '#E53935', '#FDD835', '#8E24AA', '#43A047', '#FFFFFF', '#6D4C41']
    products_shelf(X, -4.6, -1.3, yr, 1, 5, 2.1, 0.4, rnd, pal, kind='box', frame=X.c('KeGoPet', '#D9C2A0'))
    tx, ty = -1.0, yr - 0.75
    X.ab((tx - 0.3, ty - 0.3, FLOOR), (tx + 0.3, ty + 0.3, FLOOR + 0.08), X.c('ThamMeo', '#C9B79C', rough=0.95))
    X.cyl((tx, ty, FLOOR + 0.08), 0.07, 1.5, X.c('CotCaoMong', '#CDB48C', rough=0.95), seg=8)
    for zz, dd in ((0.55, 0.0), (1.05, 0.12), (1.55, -0.05)):
        X.ab((tx - 0.24 + dd, ty - 0.24, FLOOR + zz), (tx + 0.24 + dd, ty + 0.24, FLOOR + zz + 0.05), X.c('ThamMeo', '#C9B79C', rough=0.95))
    pet(X, tx, ty, FLOOR + 1.6, 'cat', '#E0A35C', yaw=0.2, s=0.7, sit=True)                               # mèo cam ngồi trên cây
    # --- quầy thu ngân giữa phòng (song song mặt tiền) + nhân viên áo xanh
    cy = yl + inner * 0.62; cl = max(1.0, min(2.2, inner - 1.9))
    X.ab((-4.05, cy - cl / 2, FLOOR), (-3.45, cy + cl / 2, FLOOR + 1.0), X.c('QuayGo', '#E7D6BC'))
    X.ab((-3.46, cy - cl / 2, FLOOR + 0.25), (-3.44, cy + cl / 2, FLOOR + 0.75), teal)                     # dải màu mặt quầy
    X.ab((-4.1, cy - cl / 2 - 0.05, FLOOR + 1.0), (-3.4, cy + cl / 2 + 0.05, FLOOR + 1.04), X.white)
    register(X, -3.75, cy + cl / 2 - 0.35)
    for k in range(4):                                                                                     # hũ bánh thưởng
        X.cyl((-3.7, cy - cl / 2 + 0.25 + k * 0.18, FLOOR + 1.04), 0.06, 0.16, X.c('HuBanh', '#E3F1F2', rough=0.05, alpha=0.5), seg=8)
        X.cyl((-3.7, cy - cl / 2 + 0.25 + k * 0.18, FLOOR + 1.05), 0.05, 0.08, X.c('BanhThuong_%d' % (k % 2), ['#C98A4B', '#F2C14E'][k % 2]), seg=8)
    px = -4.5
    X.cyl((px, cy, FLOOR), 0.16, 0.75, X.c('QuanNV', '#2E3B4E'), r2=0.19)
    X.cyl((px, cy, FLOOR + 0.75), 0.2, 0.55, teal, r2=0.17)
    X.sph((px, cy, FLOOR + 1.45), 0.11, X.c('Da', '#E9C4A0'), scale=(0.9, 0.9, 1.15)); X.sph((px - 0.02, cy, FLOOR + 1.5), 0.115, X.black, scale=(0.95, 0.95, 0.8))
    pet(X, -3.0, cy - cl / 2 - 0.45, FLOOR, 'dog', '#C98A4B', yaw=0.9, s=1.0)                              # chó vàng dắt dây
    # --- lưu trú: dãy chuồng hai tầng dọc tường trái (phía trong)
    cage_row(X, -7.7, -4.9, yl, -1, FLOOR + 0.02, 0.72, 4, rnd)
    cage_row(X, -7.7, -4.9, yl, -1, FLOOR + 0.78, 0.72, 4, rnd)
    X.box((-6.3, yl + 0.012, 2.45), (2.4, 0.02, 0.32), teal)
    X.text('LƯU TRÚ', (-6.3, yl + 0.03, 2.45), 0.15, X.white, rot=rotL)
    # --- thú y: bàn khám inox, tủ thuốc kính, đèn khám, áp phích (tường phải, phía trong)
    vx, vy = -6.4, yr - 0.6
    X.ab((vx - 0.55, vy - 0.32, FLOOR + 0.85), (vx + 0.55, vy + 0.32, FLOOR + 0.9), X.steel)
    X.ab((vx - 0.5, vy - 0.28, FLOOR), (vx + 0.5, vy + 0.28, FLOOR + 0.85), X.steelD)
    pet(X, vx, vy, FLOOR + 0.9, 'cat', '#9C9C9C', yaw=math.pi / 2, s=1.0)
    X.rod((vx + 0.3, yr - 0.05, FLOOR + 2.3), (vx, vy, FLOOR + 1.8), 0.02, X.steel)
    X.cyl((vx, vy, FLOOR + 1.72), 0.16, 0.08, X.c('DenKham', '#FFFFFF', emit=4.0), seg=12)
    glass_cabinet(X, -7.55, yr - 0.35, 0.5, 0.6, 1.7, frame=X.white)
    for k in range(6):
        X.box((-7.55, yr - 0.5 + (k % 3) * 0.15, FLOOR + 0.9 + (k // 3) * 0.4), (0.12, 0.1, 0.14), X.c('HopThuocThuY_%d' % (k % 3), ['#FFFFFF', '#1E88E5', '#43A047'][k % 3]))
    poster(X, -5.4, yr, 1, 0.65, 0.9, 2.5, '#E3F2FD', '#1E88E5'); poster(X, -6.2, yr, 1, 0.65, 0.9, 2.5, '#E8F5E9', '#43A047')
    X.box((-5.8, yr - 0.012, 3.15), (1.7, 0.02, 0.3), X.c('BangThuY', '#1E88E5'))
    X.text('THÚ Y', (-5.8, yr - 0.03, 3.15), 0.15, X.white, rot=rotR)
    for k in range(3):                                                                                     # bao thức ăn lớn dựng góc
        X.ab((-4.85 + k * 0.38, yr - 0.45, FLOOR), (-4.55 + k * 0.38, yr - 0.2, FLOOR + 0.62), X.c('BaoThucAn_%d' % k, ['#F28C28', '#16808A', '#E53935'][k]))
    # --- dấu chân trên sàn, cây xanh, tên tiệm tường hậu
    pm = X.c('DauChan', PETSHOP_ORANGE, rough=0.6)
    for k in range(6): paw(X, -1.0 - k * 0.55, yl + inner * (0.42 if k % 2 == 0 else 0.5), pm)
    plant(X, -0.95, yl + inner * 0.5 - 0.6, 0.9, '#FFFFFF')
    back_sign(X, 'Cù Lao Petshop', 'Spa - Lưu Trú - Thú y', PETSHOP_TEAL, '#FFFFFF', z=2.95)

EXTRA_TYPES = EXTRA_TYPES + ['PetShop']

FURNISH = {k: globals()['furnish_' + k] for k in SHOP_TYPES + HOME_TYPES + EXTRA_TYPES}
