# Phố Việt 21.288 — thư viện cao ốc mới cho bản đồ mở (thay các hộp trơn + lưới cửa sổ của build_blocks_w).
# Chạy Blender nền (KHÔNG qua MCP):
#   blender -b --factory-startup --python-expr "MODE='preview';exec(open(r'.../ow_towers.py',encoding='utf-8').read())"
# MODE: 'tex' (chỉ sinh texture), 'preview' (texture + dựng mẫu + render ảnh duyệt), 'export' (FBX cho Unity — làm sau khi duyệt).
# Hệ toạ độ Blender: x đông, y bắc, z lên. Mỗi mẫu dựng tại gốc (0,0,0), mặt đất z = 0.
# Chi tiết bằng texture (cửa sổ, rèm, khung, máy lạnh, biển hiệu) + hình khối vừa (đế, bậc thu, vây, ban công, đỉnh).
import bpy, bmesh, math, random, os, json
import numpy as np
from mathutils import Vector

OW_DIR = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld'
TEX_DIR = OW_DIR + '/towers_tex'
OUT_DIR = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/CaoOc-21288'
MODE = globals().get('MODE', 'preview')
os.makedirs(TEX_DIR, exist_ok=True); os.makedirs(OUT_DIR, exist_ok=True)

# ================================================================ màu
def srgb(h):
    h = h.lstrip('#'); return np.array([int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)], dtype=np.float32)

def lin(c):
    c = np.asarray(c, dtype=np.float32)
    return np.where(c > 0.04045, ((c + 0.055) / 1.055) ** 2.4, c / 12.92)

# ================================================================ texture (numpy, sRGB 0..1, gốc ảnh ở dưới-trái như Blender)
class Canvas:
    def __init__(self, w, h, seed):
        self.w, self.h = w, h
        self.c = np.zeros((h, w, 3), np.float32)
        self.ms = np.zeros((h, w, 2), np.float32)       # kim loại, độ bóng (smoothness)
        self.r = np.random.default_rng(seed)
        yy, xx = np.mgrid[0:h, 0:w]
        self.X = xx / w; self.Y = yy / h
    def rect(self, x0, y0, x1, y1, col, metal=None, smooth=None):
        x0, x1 = int(round(x0)), int(round(x1)); y0, y1 = int(round(y0)), int(round(y1))
        x0, y0 = max(0, x0), max(0, y0); x1, y1 = min(self.w, x1), min(self.h, y1)
        if x1 <= x0 or y1 <= y0: return
        self.c[y0:y1, x0:x1] = col
        if metal is not None: self.ms[y0:y1, x0:x1, 0] = metal
        if smooth is not None: self.ms[y0:y1, x0:x1, 1] = smooth
    def grad(self, x0, y0, x1, y1, c0, c1, metal=None, smooth=None):
        """Dải chuyển màu dọc (c0 dưới -> c1 trên)."""
        x0, x1 = int(round(x0)), int(round(x1)); y0, y1 = int(round(y0)), int(round(y1))
        x0, y0 = max(0, x0), max(0, y0); x1, y1 = min(self.w, x1), min(self.h, y1)
        if x1 <= x0 or y1 <= y0: return
        t = np.linspace(0, 1, y1 - y0, dtype=np.float32)[:, None, None]
        self.c[y0:y1, x0:x1] = c0 * (1 - t) + c1 * t
        if metal is not None: self.ms[y0:y1, x0:x1, 0] = metal
        if smooth is not None: self.ms[y0:y1, x0:x1, 1] = smooth
    def lowfreq(self, amp, n=5):
        """Nhiễu tần số thấp (mảng loang) dạng tổng sin."""
        f = np.zeros((self.h, self.w), np.float32)
        for _ in range(n):
            a, b = self.r.uniform(1, 4), self.r.uniform(1, 4)
            ph = self.r.uniform(0, 6.28)
            f += np.sin(self.X * a * 6.28 + self.Y * b * 6.28 + ph)
        return f / n * amp
    def grain(self, amp):
        self.c += self.r.normal(0, amp, (self.h, self.w, 1)).astype(np.float32)
    def save(self, name, with_ms=True):
        def put(arr, nm, is_data):
            h, w = arr.shape[:2]
            img = bpy.data.images.get(nm) or bpy.data.images.new(nm, w, h, alpha=True)
            if img.size[0] != w or img.size[1] != h: img.scale(w, h)
            if is_data: img.colorspace_settings.name = 'Non-Color'
            rgba = np.ones((h, w, 4), np.float32); rgba[..., :arr.shape[2]] = arr
            img.pixels.foreach_set(np.clip(rgba, 0, 1).ravel())
            img.filepath_raw = TEX_DIR + '/' + nm + '.png'; img.file_format = 'PNG'; img.save()
            return img
        a = put(self.c, name, False)
        if not with_ms: return a, None
        ms = np.zeros((self.h, self.w, 4), np.float32)
        ms[..., 0] = self.ms[..., 0]; ms[..., 1] = self.ms[..., 0]; ms[..., 2] = self.ms[..., 0]; ms[..., 3] = self.ms[..., 1]
        return a, put(ms, name + '_MS', True)

# ---------------------------------------------------------------- mặt kính văn phòng
OFFICE_TILE = dict(bays=4, floors=6, bay=3.0, floor=4.0)      # ô lặp 12 x 24 m
def tex_office(name, glass_dark, glass_light, spandrel, mullion, seed, stone=None, band=0.24):
    W, H = 512, 1024
    cv = Canvas(W, H, seed); r = cv.r
    nb, nf = OFFICE_TILE['bays'], OFFICE_TILE['floors']
    bw, fh = W / nb, H / nf
    gd, gl, sp, mu = srgb(glass_dark), srgb(glass_light), srgb(spandrel), srgb(mullion)
    sky = cv.lowfreq(0.5, 6)                                     # mây phản chiếu loang trên cả mặt kính
    for f in range(nf):
        y0 = f * fh
        sh = fh * band
        for b in range(nb):
            x0 = b * bw
            for half in range(2):                                # mỗi bay 2 tấm kính 1,5 m
                px0 = x0 + half * bw / 2; px1 = px0 + bw / 2
                tint = r.uniform(-0.035, 0.035)
                cv.grad(px0, y0 + sh, px1, y0 + fh, gd + tint, gl + tint, 0.35, 0.92)
                k = r.random()
                if k < 0.35:                                     # rèm lá sách kéo xuống một phần
                    bh = r.uniform(0.15, 0.6) * (fh - sh)
                    yb = y0 + fh - bh
                    cv.c[int(yb):int(y0 + fh), int(px0):int(px1)] = cv.c[int(yb):int(y0 + fh), int(px0):int(px1)] * 0.55 + srgb('#C9CCC8') * 0.45
                    for yy in range(int(yb), int(y0 + fh), 4):
                        cv.c[yy, int(px0):int(px1)] *= 0.9
                elif k < 0.55:                                   # thấy đèn trần trong phòng
                    yl = y0 + fh - (fh - sh) * 0.14
                    cv.c[int(yl - 3):int(yl), int(px0 + 4):int(px1 - 4)] = cv.c[int(yl - 3):int(yl), int(px0 + 4):int(px1 - 4)] * 0.4 + srgb('#E8EEF0') * 0.6
                    cv.c[int(y0 + sh):int(y0 + sh + (fh - sh) * 0.35), int(px0):int(px1)] *= 0.82
                elif k < 0.62:                                   # tấm tối (phòng tắt đèn)
                    cv.c[int(y0 + sh):int(y0 + fh), int(px0):int(px1)] *= 0.7
        # tấm che dầm sàn (spandrel)
        cv.rect(0, y0, W, y0 + sh, sp, 0.35, 0.55)
        cv.c[int(y0):int(y0 + sh)] += r.uniform(-0.02, 0.02)
        if stone is not None:                                    # khung đá: dầm ngang dày hơn
            cv.rect(0, y0, W, y0 + sh * 0.55, srgb(stone), 0.0, 0.25)
    cv.c += sky[..., None] * np.array([0.07, 0.08, 0.09], np.float32) * (cv.ms[..., 0:1] > 0.3)
    # khung nhôm: đố dọc bay, đố giữa tấm, đố ngang trên/dưới tấm kính
    for b in range(nb + 1):
        x = b * bw
        cv.rect(x - 3, 0, x + 3, H, mu, 0.8, 0.5)
        if stone is not None: cv.rect(x - 7, 0, x + 7, H, srgb(stone), 0.0, 0.25)
        if b < nb: cv.rect(x + bw / 2 - 1.5, 0, x + bw / 2 + 1.5, H, mu, 0.8, 0.5)
    for f in range(nf + 1):
        y = f * fh
        cv.rect(0, y - 2, W, y + 2, mu, 0.8, 0.5)
        cv.rect(0, y + fh * band - 1.5, W, y + fh * band + 1.5, mu, 0.8, 0.5)
    cv.grain(0.008)
    return cv.save(name)

# ---------------------------------------------------------------- mặt chung cư
RES_TILE = dict(bays=4, floors=8, bay=3.6, floor=3.2)         # ô lặp 14,4 x 25,6 m
def tex_res(name, wall, accent, frame, glass, seed, door_cols=(1, 2), ac=True):
    W, H = 512, 1024
    cv = Canvas(W, H, seed); r = cv.r
    nb, nf = RES_TILE['bays'], RES_TILE['floors']
    bw, fh = W / nb, H / nf
    pm = bw / RES_TILE['bay']; pv = fh / RES_TILE['floor']    # px/m
    wl, ac_c, fr, gl = srgb(wall), srgb(accent), srgb(frame), srgb(glass)
    cv.c[:] = wl; cv.ms[..., 1] = 0.18
    cv.c += cv.lowfreq(0.035, 6)[..., None]
    curtains = [srgb(h) for h in ('#E9E2D0', '#D8CFC0', '#C9D3D8', '#E3D2C8', '#BFC6B8', '#F0EBE0', '#8E9AA2')]
    for f in range(nf):
        y0 = f * fh
        cv.rect(0, y0, W, y0 + 0.22 * pv, ac_c, 0.0, 0.25)       # gờ sàn
        cv.rect(0, y0 + 0.22 * pv, W, y0 + 0.28 * pv, wl * 0.86)  # bóng dưới gờ
        for b in range(nb):
            x0 = b * bw
            door = b in door_cols
            ww = (2.5 if door else 1.6) * pm
            sill = (0.25 if door else 0.95) * pv
            hh = (2.45 if door else 1.45) * pv
            wx0 = x0 + (bw - ww) / 2; wy0 = y0 + sill
            # khung + kính
            cv.rect(wx0 - 4, wy0 - 4, wx0 + ww + 4, wy0 + hh + 4, fr, 0.5, 0.4)
            cv.grad(wx0, wy0, wx0 + ww, wy0 + hh, gl * 0.8, gl * 1.25 + 0.05, 0.4, 0.88)
            k = r.random()
            if k < 0.55:                                      # rèm
                cc = curtains[r.integers(len(curtains))]
                side = r.random()
                cw = ww * r.uniform(0.3, 0.55)
                if side < 0.5: cv.rect(wx0, wy0, wx0 + cw, wy0 + hh, cc * 0.92, 0.0, 0.1)
                else: cv.rect(wx0 + ww - cw, wy0, wx0 + ww, wy0 + hh, cc * 0.92, 0.0, 0.1)
                if r.random() < 0.4:
                    cv.rect(wx0 + ww - cw * 0.8, wy0, wx0 + ww, wy0 + hh, cc * 0.88, 0.0, 0.1)
            elif k < 0.65:
                cv.rect(wx0, wy0, wx0 + ww, wy0 + hh, gl * 0.55, 0.3, 0.8)
            # đố cửa
            nseg = 3 if door else 2
            for s in range(1, nseg):
                xs = wx0 + ww * s / nseg
                cv.rect(xs - 2, wy0, xs + 2, wy0 + hh, fr, 0.5, 0.4)
            if not door:
                cv.rect(wx0, wy0 + hh * 0.72, wx0 + ww, wy0 + hh * 0.72 + 3, fr, 0.5, 0.4)
                cv.rect(wx0 - 6, wy0 - 9, wx0 + ww + 6, wy0 - 4, ac_c * 0.95)       # bậu cửa
                # vệt ố dưới bậu
                st = cv.r.uniform(0.02, 0.06)
                cv.c[max(0, int(wy0 - 9 - 0.9 * pv)):int(wy0 - 9), int(wx0):int(wx0 + ww)] *= (1 - st)
                if ac and r.random() < 0.45:                                       # cục nóng máy lạnh
                    ax = wx0 + ww + 0.25 * pm if r.random() < 0.5 else wx0 - 1.05 * pm
                    ay = wy0 + 0.1 * pv
                    cv.rect(ax, ay, ax + 0.8 * pm, ay + 0.55 * pv, srgb('#D9DAD6'), 0.2, 0.3)
                    for gy in range(int(ay + 3), int(ay + 0.55 * pv - 2), 3):
                        cv.rect(ax + 0.42 * pm, gy, ax + 0.76 * pm, gy + 1, srgb('#8D9091'))
                    cv.rect(ax, ay - 3, ax + 0.8 * pm, ay, wl * 0.7)
    # vệt nước mưa dọc tường
    for _ in range(26):
        x = r.uniform(0, W); w_ = r.uniform(2, 9); a = r.uniform(0.02, 0.06)
        y1 = r.uniform(H * 0.3, H)
        cv.c[0:int(y1), int(x):int(x + w_)] *= (1 - a)
    cv.grain(0.012)
    return cv.save(name)

# ---------------------------------------------------------------- đế thương mại (2 tầng: tầng trệt cửa hàng + tầng 2)
POD_TILE = dict(width=24.0, height=9.0)                       # ô lặp 24 x 9 m
def tex_podium(name, seed):
    W, H = 1024, 512
    cv = Canvas(W, H, seed); r = cv.r
    pm = W / POD_TILE['width']; pv = H / POD_TILE['height']
    wall = srgb('#D8D2C6'); dark = srgb('#2E3438')
    cv.c[:] = wall; cv.ms[..., 1] = 0.25
    cv.c += cv.lowfreq(0.03)[..., None]
    signs = ['#B5202A', '#1D5FA8', '#F2B705', '#1E8C4E', '#E85D1F', '#16808A', '#6A2C91', '#D9D9D9']
    for s in range(4):                                         # 4 cửa hàng x 6 m
        x0 = s * 6 * pm
        # khung cửa hàng kính trệt
        cv.rect(x0 + 0.3 * pm, 0, x0 + 5.7 * pm, 4.0 * pv, dark, 0.6, 0.5)
        cv.grad(x0 + 0.45 * pm, 0.1 * pv, x0 + 5.55 * pm, 3.85 * pv, srgb('#C7B48E'), srgb('#7F8E95'), 0.3, 0.85)
        # đồ trong tiệm: kệ, đèn
        for k in range(5):
            kx = x0 + r.uniform(0.6, 5.0) * pm
            cv.rect(kx, 0.1 * pv, kx + r.uniform(0.4, 1.2) * pm, r.uniform(0.9, 2.0) * pv, srgb('#6E5A48') * r.uniform(0.7, 1.2), 0.0, 0.2)
        cv.rect(x0 + 0.5 * pm, 3.55 * pv, x0 + 5.5 * pm, 3.65 * pv, srgb('#FFF4D8'))
        for d in (2.0, 3.5):
            cv.rect(x0 + d * pm - 2, 0, x0 + d * pm + 2, 4.0 * pv, dark, 0.6, 0.5)
        # biển hiệu
        sc = srgb(signs[r.integers(len(signs))])
        cv.rect(x0 + 0.3 * pm, 4.15 * pv, x0 + 5.7 * pm, 5.15 * pv, sc, 0.1, 0.5)
        tc = srgb('#FFFFFF') if sc.mean() < 0.6 else srgb('#1A1A1A')
        tx = x0 + 0.8 * pm
        while tx < x0 + 5.0 * pm:                              # khối chữ giả
            lw = r.uniform(0.18, 0.42) * pm
            cv.rect(tx, 4.4 * pv, tx + lw, 4.9 * pv, tc)
            tx += lw + r.uniform(0.06, 0.25) * pm
        # mái hiên nhỏ
        cv.rect(x0 + 0.2 * pm, 3.95 * pv, x0 + 5.8 * pm, 4.15 * pv, srgb('#3A3F42'))
    # tầng 2: dải cửa kính
    for b in range(8):
        x0 = b * 3 * pm
        cv.rect(x0 + 0.25 * pm, 5.8 * pv, x0 + 2.75 * pm, 8.3 * pv, dark, 0.6, 0.5)
        cv.grad(x0 + 0.35 * pm, 5.9 * pv, x0 + 2.65 * pm, 8.2 * pv, srgb('#3D5566'), srgb('#9DB3C0'), 0.5, 0.9)
    cv.rect(0, 8.55 * pv, W, 9.0 * pv, srgb('#C2BCB0'))
    cv.grain(0.01)
    return cv.save(name)

# ---------------------------------------------------------------- atlas màu phụ 4 x 4 ô (vây, sàn ban công, lan can kính, mái, khối kỹ thuật...)
TRIM = ['white', 'lightgray', 'midgray', 'darkmetal', 'alu', 'railglass', 'roof', 'roofdark',
        'black', 'redlight', 'stone', 'cream', 'teal', 'bronze', 'helipad', 'greenroof']
TRIM_COL = dict(white='#EEEDE8', lightgray='#C9CBC8', midgray='#8C9193', darkmetal='#3B4247', alu='#B9C0C4',
                railglass='#8FB3BE', roof='#A6A39B', roofdark='#6F6D68', black='#1C1E20', redlight='#D3261E',
                stone='#CDBFA6', cream='#E8DCC2', teal='#2F6E73', bronze='#6E5B45', helipad='#4C5156', greenroof='#5E7B48')
TRIM_MS = dict(alu=(0.85, 0.6), darkmetal=(0.7, 0.5), railglass=(0.3, 0.9), teal=(0.4, 0.8), bronze=(0.6, 0.6), black=(0.2, 0.4))
def tex_trim(name, seed=7):
    W = H = 512; cv = Canvas(W, H, seed); r = cv.r
    s = W // 4
    for i, k in enumerate(TRIM):
        cx, cy = (i % 4) * s, (i // 4) * s
        m, sm = TRIM_MS.get(k, (0.0, 0.2))
        cv.rect(cx, cy, cx + s, cy + s, srgb(TRIM_COL[k]), m, sm)
        n = r.normal(0, 0.025 if k.startswith('roof') or k in ('stone', 'helipad', 'greenroof') else 0.008, (s, s, 1))
        cv.c[cy:cy + s, cx:cx + s] += n
        if k == 'railglass':                                   # nẹp tay vịn trên
            cv.rect(cx, cy + s * 0.88, cx + s, cy + s, srgb('#D7DCDD'), 0.8, 0.5)
        if k == 'helipad':
            yy, xx = np.mgrid[0:s, 0:s]; d = np.hypot(xx - s / 2, yy - s / 2)
            ring = (d > s * 0.36) & (d < s * 0.42)
            cv.c[cy:cy + s, cx:cx + s][ring] = srgb('#F2C230')
            hb = np.zeros((s, s), bool); hb[int(s * .3):int(s * .7), int(s * .36):int(s * .43)] = True
            hb[int(s * .3):int(s * .7), int(s * .57):int(s * .64)] = True; hb[int(s * .47):int(s * .53), int(s * .36):int(s * .64)] = True
            cv.c[cy:cy + s, cx:cx + s][hb] = srgb('#F4F4F4')
        if k in ('roof', 'roofdark'):                         # vết loang mái
            for _ in range(6):
                x = r.integers(cx, cx + s - 20); y = r.integers(cy, cy + s - 20); w_ = r.integers(8, 40)
                cv.c[y:y + w_ // 2, x:x + w_] *= r.uniform(0.85, 0.97)
    return cv.save(name)

def trim_uv(key):
    i = TRIM.index(key); s = 0.25
    u0, v0 = (i % 4) * s, (i // 4) * s
    return (u0 + 0.03, v0 + 0.03, u0 + s - 0.03, v0 + s - 0.03)

# ================================================================ vật liệu Blender
FACADES = {
    # văn phòng
    'KinhXanh':  ('office', dict(glass_dark='#0F2C48', glass_light='#5B8DB5', spandrel='#1A2C3B', mullion='#AEB8C0', seed=11)),
    'KinhNgoc':  ('office', dict(glass_dark='#0C3E40', glass_light='#4F9893', spandrel='#173332', mullion='#95A7A5', seed=12)),
    'KinhBac':   ('office', dict(glass_dark='#2C3B47', glass_light='#8FA6B6', spandrel='#46525B', mullion='#C9CED2', seed=13, stone='#CFC5B0')),
    'KinhToi':   ('office', dict(glass_dark='#0A121A', glass_light='#3B5568', spandrel='#151D24', mullion='#4A545B', seed=14)),
    'KinhDong':  ('office', dict(glass_dark='#2A2419', glass_light='#8A7F69', spandrel='#3E3426', mullion='#9C8663', seed=15, band=0.3)),
    # chung cư
    'CcTrang':   ('res', dict(wall='#E2E0DA', accent='#8F9BA3', frame='#3E4448', glass='#3F5666', seed=21)),
    'CcKem':     ('res', dict(wall='#E0CFAE', accent='#AE8A58', frame='#F2EFE8', glass='#45606D', seed=22, door_cols=(0, 3))),
    'CcXamXanh': ('res', dict(wall='#C6CDD0', accent='#466173', frame='#2E3539', glass='#33495A', seed=23)),
    'CcHongDat': ('res', dict(wall='#D9B49B', accent='#9A5A3F', frame='#EDEBE6', glass='#3C505C', seed=24, door_cols=(1,))),
    'CcVang':    ('res', dict(wall='#E8D396', accent='#B99C62', frame='#FFFFFF', glass='#425B67', seed=25, door_cols=(0, 2))),
}

def make_textures():
    imgs = {}
    for k, (kind, kw) in FACADES.items():
        imgs[k] = (tex_office if kind == 'office' else tex_res)('OW_CaoOc_' + k, **kw)
    imgs['DeThuongMai'] = tex_podium('OW_CaoOc_DeThuongMai', 31)
    imgs['Phu'] = tex_trim('OW_CaoOc_Phu')
    return imgs

def img_mat(name, img, ms=None):
    m = bpy.data.materials.get(name)
    if m: return m
    m = bpy.data.materials.new(name); m.use_nodes = True
    nt = m.node_tree; b = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    t = nt.nodes.new('ShaderNodeTexImage'); t.image = img
    nt.links.new(t.outputs['Color'], b.inputs['Base Color'])
    if ms is not None:
        t2 = nt.nodes.new('ShaderNodeTexImage'); t2.image = ms
        sep = nt.nodes.new('ShaderNodeSeparateColor'); nt.links.new(t2.outputs['Color'], sep.inputs['Color'])
        nt.links.new(sep.outputs['Red'], b.inputs['Metallic'])
        inv = nt.nodes.new('ShaderNodeMath'); inv.operation = 'SUBTRACT'; inv.inputs[0].default_value = 1.0
        nt.links.new(t2.outputs['Alpha'], inv.inputs[1]); nt.links.new(inv.outputs['Value'], b.inputs['Roughness'])
    else:
        b.inputs['Roughness'].default_value = 0.8
    return m

# ================================================================ hình học
class MB:
    """Dựng lưới có UV, mặt phẳng (không chung đỉnh -> cạnh cứng)."""
    def __init__(self, name):
        self.name = name; self.bm = bmesh.new(); self.uv = self.bm.loops.layers.uv.verify()
        self.mats = []
    def mi(self, m):
        if m not in self.mats: self.mats.append(m)
        return self.mats.index(m)
    def face(self, pts, uvs, m):
        vs = [self.bm.verts.new(p) for p in pts]
        f = self.bm.faces.new(vs); f.material_index = self.mi(m)
        for l, uv in zip(f.loops, uvs): l[self.uv].uv = uv
        return f
    def ntris(self):
        return sum(len(f.verts) - 2 for f in self.bm.faces)
    def finish(self, coll):
        me = bpy.data.meshes.new(self.name); self.bm.to_mesh(me); self.bm.free()
        for m in self.mats: me.materials.append(m)
        ob = bpy.data.objects.new(self.name, me); coll.objects.link(ob)
        return ob

def walls(mb, poly, z0, z1, m, modw, modh, voff=0.0):
    """Mặt đứng của lăng trụ đa giác (đỉnh ngược chiều kim đồng hồ nhìn từ trên). u theo mét/modw từ góc mỗi mặt, v theo cao độ tuyệt đối/modh."""
    n = len(poly)
    for i in range(n):
        a, b = poly[i], poly[(i + 1) % n]
        L = math.hypot(b[0] - a[0], b[1] - a[1])
        if L < 1e-4: continue
        u1 = L / modw
        mb.face([(a[0], a[1], z0), (b[0], b[1], z0), (b[0], b[1], z1), (a[0], a[1], z1)],
                [(0, (z0 + voff) / modh), (u1, (z0 + voff) / modh), (u1, (z1 + voff) / modh), (0, (z1 + voff) / modh)], m)

def cap(mb, poly, z, m, key, down=False):
    u0, v0, u1, v1 = trim_uv(key)
    xs = [p[0] for p in poly]; ys = [p[1] for p in poly]
    ax, bx, ay, by = min(xs), max(xs), min(ys), max(ys)
    sx, sy = max(bx - ax, 1e-3), max(by - ay, 1e-3)
    pts = [(p[0], p[1], z) for p in poly]
    uvs = [(u0 + (p[0] - ax) / sx * (u1 - u0), v0 + (p[1] - ay) / sy * (v1 - v0)) for p in poly]
    if down: pts, uvs = pts[::-1], uvs[::-1]
    mb.face(pts, uvs, m)

def trim_prism(mb, poly, z0, z1, m, key, top=True, bottom=True):   # 21.288: có mặt đáy — Unity cull mặt sau, nhìn từ đường lên sàn ban công không bị rỗng
    u0, v0, u1, v1 = trim_uv(key)
    n = len(poly)
    for i in range(n):
        a, b = poly[i], poly[(i + 1) % n]
        mb.face([(a[0], a[1], z0), (b[0], b[1], z0), (b[0], b[1], z1), (a[0], a[1], z1)], [(u0, v0), (u1, v0), (u1, v1), (u0, v1)], m)
    if top: cap(mb, poly, z1, m, key)
    if bottom: cap(mb, poly, z0, m, key, down=True)

def rect(cx, cy, w, d):
    return [(cx - w / 2, cy - d / 2), (cx + w / 2, cy - d / 2), (cx + w / 2, cy + d / 2), (cx - w / 2, cy + d / 2)]

def chamfer(w, d, c):
    x, y = w / 2, d / 2
    return [(-x + c, -y), (x - c, -y), (x, -y + c), (x, y - c), (x - c, y), (-x + c, y), (-x, y - c), (-x, -y + c)]

def rounded(w, d, r, seg=3):
    x, y = w / 2 - r, d / 2 - r; pts = []
    for cx, cy, a0 in ((x, -y, -90), (x, y, 0), (-x, y, 90), (-x, -y, 180)):
        for k in range(seg + 1):
            a = math.radians(a0 + 90 * k / seg); pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts

def cross(w, d, arm):
    x, y, a = w / 2, d / 2, arm / 2
    return [(-a, -y), (a, -y), (a, -a), (x, -a), (x, a), (a, a), (a, y), (-a, y), (-a, a), (-x, a), (-x, -a), (-a, -a)]

def notch(w, d, n):
    x, y = w / 2, d / 2
    return [(-x + n, -y), (x - n, -y), (x - n, -y + n), (x, -y + n), (x, y - n), (x - n, y - n), (x - n, y), (-x + n, y), (-x + n, y - n), (-x, y - n), (-x, -y + n), (-x + n, -y + n)]

def scale_poly(poly, s):
    return [(p[0] * s, p[1] * s) for p in poly]

def edges_of(poly):
    n = len(poly)
    for i in range(n):
        a, b = poly[i], poly[(i + 1) % n]
        L = math.hypot(b[0] - a[0], b[1] - a[1])
        if L < 1e-4: continue
        t = ((b[0] - a[0]) / L, (b[1] - a[1]) / L); nrm = (t[1], -t[0])
        yield a, b, L, t, nrm

def box_on_edge(mb, a, t, nrm, s0, s1, out0, out1, z0, z1, m, key, top=True):
    """Hộp dọc theo mặt: từ s0..s1 theo cạnh, nhô out0..out1 ra ngoài."""
    def P(s, o): return (a[0] + t[0] * s + nrm[0] * o, a[1] + t[1] * s + nrm[1] * o)
    trim_prism(mb, [P(s0, out0), P(s0, out1), P(s1, out1), P(s1, out0)][::-1], z0, z1, m, key, top=top)

# ================================================================ mẫu cao ốc
class Spec:
    def __init__(self, key, kind, plan, facade, **kw):
        self.key, self.kind, self.plan, self.facade = key, kind, plan, facade
        self.__dict__.update(kw)

SPECS = [
    # --- khu tài chính: văn phòng
    Spec('VP_VayDoc', 'office', ('rect', 24, 24), 'KinhXanh', pod=(4, 3.0), setbacks=[(0.62, 0.84), (0.86, 0.66)], fins=3.0, crown='spire'),
    Spec('VP_VatGoc', 'office', ('chamfer', 28, 22, 5), 'KinhNgoc', pod=(3, 2.5), setbacks=[], bands=True, crown='pyramid'),
    Spec('VP_KhungDa', 'office', ('rect', 30, 21), 'KinhBac', pod=(4, 2.0), setbacks=[(0.8, 0.9)], pilasters=True, crown='helipad'),
    Spec('VP_KhoiLech', 'office', ('rect', 22, 16), 'KinhToi', pod=(3, 3.0), wing=(14, 18, 0.66), setbacks=[], fins=6.0, crown='frame'),
    Spec('VP_BoTron', 'office', ('rounded', 26, 26, 6), 'KinhDong', pod=(4, 2.5), setbacks=[(0.7, 0.88)], bands=True, crown='crownring'),
    # --- căn hộ
    Spec('CH_BanCong', 'res', ('rect', 32, 16), 'CcTrang', pod=(3, 2.0), setbacks=[], balcony='band', fins=8.0, fin_key='midgray', crown='tanks'),
    Spec('CH_ChuThap', 'res', ('cross', 30, 30, 11), 'CcKem', pod=(2, 3.0), setbacks=[], balcony='ends', crown='pergola'),
    Spec('CH_GocLom', 'res', ('notch', 24, 24, 4.5), 'CcXamXanh', pod=(3, 2.0), setbacks=[], balcony='notch', fins=7.2, crown='frame'),
    Spec('CH_ZigZag', 'res', ('rect', 26, 20), 'CcHongDat', pod=(3, 2.0), setbacks=[], balcony='zigzag', crown='fin'),
    # --- thấp tầng bờ bắc
    Spec('TT_ChungCu', 'res', ('rect', 30, 14), 'CcVang', pod=(1, 0.0), setbacks=[], balcony='band', crown='tanks'),
    Spec('TT_VanPhong', 'office', ('rect', 20, 20), 'KinhBac', pod=(2, 1.5), setbacks=[], bands=True, crown='tanks'),
]
HEIGHTS = {'VP': (64, 104, 152), 'CH': (52, 82, 116), 'TT': (26, 40)}

def plan_poly(p):
    k = p[0]
    if k == 'rect': return rect(0, 0, p[1], p[2])
    if k == 'chamfer': return chamfer(p[1], p[2], p[3])
    if k == 'rounded': return rounded(p[1], p[2], p[3])
    if k == 'cross': return cross(p[1], p[2], p[3])
    if k == 'notch': return notch(p[1], p[2], p[3])

def bbox(poly):
    xs = [p[0] for p in poly]; ys = [p[1] for p in poly]
    return min(xs), min(ys), max(xs), max(ys)

def build_tower(spec, height, mats, coll, lod):
    office = spec.kind == 'office'
    tile = OFFICE_TILE if office else RES_TILE
    fl = tile['floor']; modw = tile['bays'] * tile['bay']; modh = tile['floors'] * fl
    fac, pod, trim = mats[spec.facade], mats['DeThuongMai'], mats['Phu']
    name = 'OW_CaoOc_%s_%d_LOD%d' % (spec.key, height, lod)
    mb = MB(name)
    base = plan_poly(spec.plan)
    npod, margin = spec.pod
    pod_h = 0.0
    # ---- khối đế
    if npod > 0 and margin > 0:
        pod_h = 4.5 + (npod - 1) * 4.0
        x0, y0, x1, y1 = bbox(base)
        pp = rect((x0 + x1) / 2, (y0 + y1) / 2, x1 - x0 + 2 * margin, y1 - y0 + 2 * margin)
        walls(mb, pp, 0, min(pod_h, 9.0), pod, POD_TILE['width'], POD_TILE['height'])
        if pod_h > 9.0: walls(mb, pp, 9.0, pod_h, fac, modw, modh)
        cap(mb, pp, pod_h, trim, 'roof')
        if lod == 0:                                            # gờ mái đế + mái sảnh
            for a, b, L, t, n in edges_of(pp):
                box_on_edge(mb, a, t, n, -0.3, L + 0.3, -0.05, 0.35, pod_h, pod_h + 1.1, trim, 'white')
            a, b, L, t, n = next(edges_of(pp))
            box_on_edge(mb, a, t, n, L * 0.35, L * 0.65, 0.0, 3.2, 4.3, 4.6, trim, 'darkmetal')
    elif npod > 0:
        pod_h = 4.5
        walls(mb, base, 0, pod_h, pod, POD_TILE['width'], POD_TILE['height'])
    # ---- thân + bậc thu
    segs = []
    z = pod_h; scale = 1.0
    cuts = list(spec.setbacks) + [(1.0, None)]
    roof_h = height
    for frac, nxt in cuts:
        z1 = max(z + fl, round((height * frac - pod_h) / fl) * fl + pod_h) if frac < 1.0 else height
        segs.append((scale_poly(base, scale), z, z1))
        z = z1
        if nxt: scale = nxt
    if lod == 2:
        segs = [(segs[0][0], pod_h, height)]
    for poly, z0, z1 in segs:
        walls(mb, poly, z0, z1, fac, modw, modh)
        cap(mb, poly, z1, trim, 'roof')
        if lod == 0 and z1 < height:                           # gờ ở bậc thu
            for a, b, L, t, n in edges_of(poly):
                box_on_edge(mb, a, t, n, 0, L, -0.05, 0.25, z1 - 0.5, z1 + 1.1, trim, 'white' if not office else 'alu')
    # ---- cánh phụ (khối lệch)
    if getattr(spec, 'wing', None) and lod < 2:
        ww, wd, wf = spec.wing
        x0, y0, x1, y1 = bbox(base)
        wp = rect(x1 - ww * 0.25, y0 + wd * 0.35, ww, wd)
        wz = round((height * wf - pod_h) / fl) * fl + pod_h
        walls(mb, wp, pod_h, wz, mats['KinhBac'], modw, modh)
        cap(mb, wp, wz, trim, 'roofdark')
        if lod == 0:
            for a, b, L, t, n in edges_of(wp):
                box_on_edge(mb, a, t, n, 0, L, -0.05, 0.3, wz, wz + 1.6, trim, 'alu')
    top_poly = segs[-1][0]
    # ---- chi tiết mặt đứng (chỉ LOD0)
    if lod == 0:
        if getattr(spec, 'fins', None):
            step = spec.fins
            for poly, z0, z1 in segs:
                for a, b, L, t, n in edges_of(poly):
                    k = 1
                    while k * step < L - 0.5:
                        s = k * step
                        box_on_edge(mb, a, t, n, s - 0.15 - (0.15 if not office else 0), s + 0.15 + (0.15 if not office else 0), 0.0, 0.55 if office else 1.6, z0, z1 + (0.6 if office else 0), trim, getattr(spec, 'fin_key', 'alu' if office else 'white'))
                        k += 1
        if getattr(spec, 'bands', False):                      # dải ngang mỗi 2 tầng (văn phòng kính)
            for poly, z0, z1 in segs:
                zz = z0 + 2 * fl
                while zz < z1 - 1:
                    for a, b, L, t, n in edges_of(poly):
                        box_on_edge(mb, a, t, n, 0, L, 0.0, 0.3, zz - 0.25, zz + 0.15, trim, 'alu', top=False)
                    zz += 2 * fl
        if getattr(spec, 'pilasters', False):                  # trụ góc đá
            for poly, z0, z1 in segs:
                for a, b, L, t, n in edges_of(poly):
                    box_on_edge(mb, a, t, n, -0.6, 1.2, -0.6, 0.6, z0, z1 + 2.0, trim, 'stone')
        bal = getattr(spec, 'balcony', None)
        if bal:
            poly, z0, z1 = segs[0]
            edges = list(edges_of(poly))
            nfl = int((z1 - z0) / fl)
            for f in range(1, nfl):
                zf = z0 + f * fl
                for ei, (a, b, L, t, n) in enumerate(edges):
                    if bal == 'band':
                        if L < 20: continue
                        spans = [(1.2, L - 1.2)]
                    elif bal == 'ends':
                        if L > 12: continue
                        spans = [(0.6, L - 0.6)]
                    elif bal == 'zigzag':
                        h = L / 2
                        spans = [(0.8, h - 0.4)] if (f + ei) % 2 == 0 else [(h + 0.4, L - 0.8)]
                    elif bal == 'notch':
                        if L > 6: continue
                        spans = [(0.0, L)]
                    for s0, s1 in spans:
                        dep = 1.5 if bal != 'notch' else 0.0
                        o0 = 0.0 if bal != 'notch' else -0.01
                        if bal == 'notch':
                            continue
                        box_on_edge(mb, a, t, n, s0, s1, o0, dep, zf - 0.2, zf, trim, 'white')              # sàn
                        box_on_edge(mb, a, t, n, s0, s1, dep - 0.06, dep, zf, zf + 1.1, trim, 'railglass', top=False)
                if bal == 'notch':                              # sàn ban công trong góc lõm
                    w, d, nn = spec.plan[1], spec.plan[2], spec.plan[3]
                    for sx in (-1, 1):
                        for sy in (-1, 1):
                            cx, cy = sx * (w / 2 - nn / 2 + 0.05), sy * (d / 2 - nn / 2 + 0.05)
                            trim_prism(mb, rect(cx, cy, nn + 0.1, nn + 0.1), zf - 0.2, zf, trim, 'white')
                            # lan can kính hai cạnh ngoài
                            trim_prism(mb, rect(cx, sy * (d / 2 - 0.03), nn, 0.06), zf, zf + 1.1, trim, 'railglass', top=False)
                            trim_prism(mb, rect(sx * (w / 2 - 0.03), cy, 0.06, nn), zf, zf + 1.1, trim, 'railglass', top=False)
            if bal == 'notch':                                  # cột góc chịu lực
                w, d, nn = spec.plan[1], spec.plan[2], spec.plan[3]
                for sx in (-1, 1):
                    for sy in (-1, 1):
                        trim_prism(mb, rect(sx * (w / 2 - 0.3), sy * (d / 2 - 0.3), 0.6, 0.6), z0, z1, trim, 'white')
    # ---- đỉnh
    x0, y0, x1, y1 = bbox(top_poly)
    tw, td = x1 - x0, y1 - y0
    cr = spec.crown
    if lod < 2:
        # tường chắn mái (parapet)
        for a, b, L, t, n in edges_of(top_poly):
            box_on_edge(mb, a, t, n, 0, L, -0.3, 0.05, height, height + 1.2, trim, 'alu' if office else 'white')
        if cr == 'spire':
            sp = scale_poly(top_poly, 0.94)
            walls(mb, sp, height, height + 8.0, fac, modw, modh)       # màn kính đỉnh
            cap(mb, scale_poly(top_poly, 0.6), height + 8.0, trim, 'roofdark')
            trim_prism(mb, scale_poly(top_poly, 0.6), height, height + 8.0, trim, 'midgray', top=False)
            ring = [(math.cos(a) * 1.2, math.sin(a) * 1.2) for a in np.linspace(0, 2 * math.pi, 9)[:-1]]
            trim_prism(mb, ring, height + 8.0, height + 26.0, trim, 'alu')
            trim_prism(mb, scale_poly(ring, 0.4), height + 26.0, height + 32.0, trim, 'redlight')
        elif cr == 'pyramid':
            top = height + min(tw, td) * 0.55
            c = (0.0, 0.0)
            for a, b, L, t, n in edges_of(top_poly):
                mb.face([(a[0], a[1], height), (b[0], b[1], height), (b[0] * 0.25, b[1] * 0.25, top), (a[0] * 0.25, a[1] * 0.25, top)],
                        [(0, height / modh), (L / modw, height / modh), (L / modw * 0.75, top / modh), (L / modw * 0.25, top / modh)], fac)
            cap(mb, scale_poly(top_poly, 0.25), top, trim, 'alu')
            ring = [(math.cos(a) * 0.5, math.sin(a) * 0.5) for a in np.linspace(0, 2 * math.pi, 7)[:-1]]
            trim_prism(mb, ring, top, top + 12.0, trim, 'alu')
        elif cr == 'helipad':
            trim_prism(mb, rect(-tw * 0.15, 0, tw * 0.35, td * 0.45), height, height + 5.0, trim, 'lightgray')   # khối kỹ thuật
            pad = [(tw * 0.18 + math.cos(a) * min(tw, td) * 0.3, math.sin(a) * min(tw, td) * 0.3) for a in np.linspace(0, 2 * math.pi, 13)[:-1]]
            trim_prism(mb, pad, height + 5.0, height + 5.6, trim, 'midgray', top=False)
            cap(mb, pad, height + 5.6, trim, 'helipad')
            if lod == 0:
                for k in range(4):
                    a = k * math.pi / 2 + 0.6
                    px, py = tw * 0.18 + math.cos(a) * 3.0, math.sin(a) * 3.0
                    trim_prism(mb, rect(px, py * 0.5, 0.4, 0.4), height, height + 5.0, trim, 'darkmetal', top=False)
        elif cr in ('frame', 'pergola'):
            fh = 6.0 if cr == 'frame' else 4.0
            fp = scale_poly(top_poly, 0.9 if cr == 'frame' else 0.7)
            for a, b, L, t, n in edges_of(fp):                 # khung dầm trên mái
                box_on_edge(mb, a, t, n, 0, L, -0.5, 0.0, height + fh - 0.8, height + fh, trim, 'white' if cr == 'pergola' else 'darkmetal')
                k = 0
                while k * 4.0 <= L:
                    box_on_edge(mb, a, t, n, k * 4.0, k * 4.0 + 0.4, -0.5, -0.1, height, height + fh - 0.8, trim, 'white' if cr == 'pergola' else 'darkmetal', top=False)
                    k += 1
            trim_prism(mb, rect(0, 0, tw * 0.3, td * 0.3), height, height + 3.5, trim, 'lightgray')
            if cr == 'pergola':
                cap(mb, scale_poly(top_poly, 0.95), height + 0.05, trim, 'greenroof')
        elif cr == 'crownring':
            cp = scale_poly(top_poly, 1.04)
            trim_prism(mb, cp, height + 2.0, height + 7.0, trim, 'alu', top=False)
            k = 0
            for a, b, L, t, n in edges_of(top_poly):            # cột đỡ vành
                box_on_edge(mb, a, t, n, L * 0.5 - 0.3, L * 0.5 + 0.3, -0.3, 0.6, height, height + 2.0, trim, 'alu', top=False)
            trim_prism(mb, scale_poly(top_poly, 0.5), height, height + 4.0, trim, 'lightgray')
        elif cr == 'tanks':
            trim_prism(mb, rect(-tw * 0.2, 0, tw * 0.22, td * 0.35), height, height + 3.8, trim, 'lightgray')     # buồng thang
            for k in range(3 if lod == 0 else 1):
                cx = tw * 0.12 + k * 3.0
                ring = [(cx + math.cos(a) * 1.1, math.sin(a) * 1.1) for a in np.linspace(0, 2 * math.pi, 9)[:-1]]
                trim_prism(mb, ring, height + 0.6, height + 3.0, trim, 'alu' if k % 2 else 'white')
            if lod == 0:
                trim_prism(mb, rect(tw * 0.15, -td * 0.25, 6.0, 2.2), height, height + 1.8, trim, 'midgray')
                trim_prism(mb, rect(-tw * 0.3, -td * 0.3, 0.12, 0.12), height + 3.8, height + 9.0, trim, 'darkmetal', top=False)
        elif cr == 'fin':
            a, b, L, t, n = next(edges_of(top_poly))
            box_on_edge(mb, a, t, n, L * 0.2, L * 0.8, -0.3, 0.2, height, height + 9.0, trim, 'white')
            trim_prism(mb, rect(0, td * 0.15, tw * 0.4, td * 0.3), height, height + 4.0, trim, 'lightgray')
    else:
        if cr == 'spire':
            ring = [(math.cos(a) * 1.2, math.sin(a) * 1.2) for a in np.linspace(0, 2 * math.pi, 5)[:-1]]
            trim_prism(mb, ring, height, height + 30.0, trim, 'alu')
    tris = mb.ntris()
    ob = mb.finish(coll)
    return ob, tris

# ================================================================ cảnh xem trước
def setup_scene():
    sc = bpy.context.scene
    for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
    try: sc.render.engine = 'CYCLES'
    except TypeError: pass
    sc.cycles.samples = 48; sc.cycles.use_denoising = True
    try: sc.cycles.device = 'CPU'
    except Exception: pass
    sc.render.resolution_x, sc.render.resolution_y = 1800, 1000
    sc.view_settings.view_transform = 'Standard'; sc.view_settings.exposure = -0.6
    w = bpy.data.worlds.new('Sky'); sc.world = w; w.use_nodes = True
    nt = w.node_tree; bg = next(n for n in nt.nodes if n.type == 'BACKGROUND')
    sky = nt.nodes.new('ShaderNodeTexSky')
    try: sky.sky_type = 'MULTIPLE_SCATTERING'
    except Exception:
        try: sky.sky_type = 'NISHITA'
        except Exception: pass
    try:
        sky.sun_elevation = math.radians(32); sky.sun_rotation = math.radians(215)
    except Exception: pass
    nt.links.new(sky.outputs['Color'], bg.inputs['Color']); bg.inputs['Strength'].default_value = 0.22
    sun = bpy.data.lights.new('Sun', 'SUN'); sun.energy = 2.6; sun.angle = math.radians(1.5)
    so = bpy.data.objects.new('Sun', sun); sc.collection.objects.link(so)
    so.rotation_euler = (math.radians(58), 0, math.radians(215))
    return sc

def ground(coll, size, col='#8E8C86', z=0.0, name='Ground'):
    me = bpy.data.meshes.new(name)
    s = size
    me.from_pydata([(-s, -s, z), (s, -s, z), (s, s, z), (-s, s, z)], [], [(0, 1, 2, 3)])
    m = bpy.data.materials.new(name); m.use_nodes = True
    b = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    b.inputs['Base Color'].default_value = tuple(lin(srgb(col))) + (1,); b.inputs['Roughness'].default_value = 0.9
    me.materials.append(m)
    ob = bpy.data.objects.new(name, me); coll.objects.link(ob); return ob

def water(coll, x0, y0, x1, y1, z=-1.0):
    me = bpy.data.meshes.new('Song')
    me.from_pydata([(x0, y0, z), (x1, y0, z), (x1, y1, z), (x0, y1, z)], [], [(0, 1, 2, 3)])
    m = bpy.data.materials.new('Song'); m.use_nodes = True
    nt = m.node_tree; b = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    b.inputs['Base Color'].default_value = tuple(lin(srgb('#2E4A4C'))) + (1,); b.inputs['Roughness'].default_value = 0.08
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = 0.6
    bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = 0.08
    nt.links.new(nz.outputs['Fac'], bp.inputs['Height']); nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    me.materials.append(m)
    ob = bpy.data.objects.new('Song', me); coll.objects.link(ob)

def camera(name, loc, target, lens=35):
    cd = bpy.data.cameras.new(name); cd.lens = lens; cd.clip_end = 6000
    co = bpy.data.objects.new(name, cd); bpy.context.scene.collection.objects.link(co)
    co.location = loc
    d = Vector(target) - Vector(loc); co.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
    return co

def render(cam, path, samples=None):
    sc = bpy.context.scene; sc.camera = cam
    if samples: sc.cycles.samples = samples
    sc.render.filepath = path; bpy.ops.render.render(write_still=True)
    print('RENDER', path)

def run_preview():
    imgs = make_textures()
    sc = setup_scene()
    mats = {k: img_mat('OW_CaoOc_' + k, imgs[k][0], imgs[k][1]) for k in imgs}
    coll = bpy.data.collections.new('CaoOc'); sc.collection.children.link(coll)
    ground(coll, 3000)
    report = []
    # Hàng mẫu: mỗi mẫu cao nhất của nhóm, LOD0
    x = 0.0; lineup = []
    for spec in SPECS:
        hs = HEIGHTS[spec.key[:2]]
        ob, tris = build_tower(spec, hs[-1], mats, coll, 0)
        x0, y0, x1, y1 = bbox(plan_poly(spec.plan))
        wdt = (x1 - x0) + 2 * spec.pod[1] + 14
        ob.location = (x + wdt / 2, 0, 0); x += wdt
        lineup.append(ob)
        _, t1 = build_tower(spec, hs[-1], mats, bpy.data.collections.new('tmp'), 1)
        _, t2 = build_tower(spec, hs[-1], mats, bpy.data.collections.new('tmp'), 2)
        report.append((spec.key, hs[-1], tris, t1, t2))
    span = x
    # ảnh 1: toàn hàng
    c1 = camera('Cam_HangMau', (span / 2, -420, 55), (span / 2, 0, 70), 24)
    render(c1, OUT_DIR + '/caooc-hang-mau.png')
    # ảnh 2: cận cảnh 3 mẫu văn phòng
    c2 = camera('Cam_VanPhong', (60, -95, 18), (95, 0, 85), 28)
    render(c2, OUT_DIR + '/caooc-van-phong-can.png')
    # ảnh 3: cận cảnh chung cư
    xs = [o.location.x for o in lineup]
    c3 = camera('Cam_ChungCu', (xs[6] - 10, -90, 14), (xs[6] + 10, 0, 60), 28)
    render(c3, OUT_DIR + '/caooc-chung-cu-can.png')
    # ảnh 4: đường chân trời bên kia sông (giống góc ảnh 2 của người dùng)
    for o in lineup: o.hide_render = True
    bpy.data.objects['Ground'].hide_render = True
    sky = bpy.data.collections.new('ChanTroi'); sc.collection.children.link(sky)
    rr = random.Random(5)
    water(sky, -1200, -60, 1200, 262, -1.5)
    g2 = ground(sky, 1, name='BoBen'); g2.scale = (1400, 330, 1); g2.location = (0, 590, 0)
    gx = -700.0
    pool = [s for s in SPECS if s.kind == 'res' and not s.key.startswith('TT')] + [s for s in SPECS if s.kind == 'office']
    while gx < 700:
        spec = rr.choice(pool)
        hs = HEIGHTS[spec.key[:2]]
        h = rr.choice(hs)
        ob, _ = build_tower(spec, h, mats, sky, 0 if abs(gx) < 260 else 1)
        x0, y0, x1, y1 = bbox(plan_poly(spec.plan))
        ob.location = (gx + (x1 - x0) / 2, 300 + rr.uniform(0, 120), 0); ob.rotation_euler.z = rr.choice([0, math.pi / 2, math.pi]) if spec.plan[0] != 'rect' else rr.choice([0, math.pi])
        gx += (x1 - x0) + 2 * spec.pod[1] + rr.uniform(6, 24)
    for k in range(40):                                         # hàng sau xa hơn (LOD2)
        spec = rr.choice(pool); h = rr.choice(HEIGHTS[spec.key[:2]])
        ob, _ = build_tower(spec, h, mats, sky, 2)
        ob.location = (rr.uniform(-900, 900), rr.uniform(480, 800), 0); ob.rotation_euler.z = rr.choice([0, math.pi / 2])
    c4 = camera('Cam_BoSong', (0, -40, 6), (0, 400, 55), 24)
    render(c4, OUT_DIR + '/caooc-chan-troi-bo-song.png')
    bpy.ops.wm.save_as_mainfile(filepath=OW_DIR + '/PhoViet_OW_CaoOc_21288.blend')
    print('REPORT key,height,LOD0,LOD1,LOD2')
    for r_ in report: print('REPORT', r_)

# ================================================================ xuất cho Unity
UNITY_DIR = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/Assets/PhoViet/OpenWorld/Towers'
def run_export():
    import shutil
    imgs = make_textures()
    for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
    mats = {k: img_mat('OW_CaoOc_' + k, imgs[k][0], imgs[k][1]) for k in imgs}
    os.makedirs(UNITY_DIR + '/Models', exist_ok=True); os.makedirs(UNITY_DIR + '/Textures', exist_ok=True)
    for f in os.listdir(TEX_DIR):
        if f.endswith('.png'): shutil.copyfile(TEX_DIR + '/' + f, UNITY_DIR + '/Textures/' + f)
    models = []
    for spec in SPECS:
        for h in HEIGHTS[spec.key[:2]]:
            coll = bpy.data.collections.new('X_%s_%d' % (spec.key, h)); bpy.context.scene.collection.children.link(coll)
            obs = []; tris = []
            for lod in range(3):
                ob, t = build_tower(spec, h, mats, coll, lod); obs.append(ob); tris.append(t)
            # khung bao (gồm đế) theo LOD1 để đặt chỗ + va chạm
            xs = [v.co.x for v in obs[1].data.vertices]; ys = [v.co.y for v in obs[1].data.vertices]; zs = [v.co.z for v in obs[1].data.vertices]
            name = 'OW_CaoOc_%s_%d' % (spec.key, h)
            for lod, ob in enumerate(obs): ob.name = name + '_LOD%d' % lod; ob.data.name = ob.name
            bpy.ops.object.select_all(action='DESELECT')
            for ob in obs: ob.select_set(True)
            bpy.context.view_layer.objects.active = obs[0]
            bpy.ops.export_scene.fbx(filepath=UNITY_DIR + '/Models/' + name + '.fbx', use_selection=True, object_types={'MESH'},
                                     axis_forward='-Z', axis_up='Y', apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                                     bake_anim=False, add_leaf_bones=False, use_mesh_modifiers=True, mesh_smooth_type='FACE', path_mode='STRIP')
            models.append(dict(name=name, spec=spec.key, kind=spec.kind, h=h, top=round(max(zs), 2),
                               x0=round(min(xs), 2), x1=round(max(xs), 2), y0=round(min(ys), 2), y1=round(max(ys), 2), tris=tris))
            print('EXPORT', name, tris)
    json.dump(dict(note='Khung bao LOD1 theo toạ độ Blender của mẫu (x đông, y bắc, z lên); gốc ở tâm chân nhà.', models=models),
              open(UNITY_DIR + '/towers_models.json', 'w', encoding='utf-8'), indent=1)
    print('MODELS', len(models))

if MODE == 'export':
    run_export()
elif MODE == 'tex':
    make_textures()
elif MODE == 'preview':
    run_preview()
