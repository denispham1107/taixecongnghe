# Phố Việt 21.293 — Tháp Tài Chính dựng lại theo ảnh mẫu người dùng gửi 21/11/2026 (tháp búp sen có sân trực thăng nhô ra).
# Chạy Blender nền (KHÔNG qua MCP):
#   blender -b --factory-startup --python-expr "MODE='preview';exec(open(r'.../ow_thap_taichinh.py',encoding='utf-8').read())"
# MODE: 'tex' (chỉ texture) | 'preview' (texture + dựng LOD0 + render ảnh so với ảnh mẫu + lưu .blend) | 'export' (FBX 3 LOD + va chạm).
# Hệ toạ độ Blender: x sang phải (nhìn chính diện), y vào trong (mặt chính diện quay về −y), z lên; gốc ở tâm chân tháp, đất z = 0.
# Số đo lấy từ ảnh mẫu (ảnh 1088×1450): x_m = (px − 534)·0,135, z_m = (1432 − py)·0,185 — chiều cao giữ đúng 262 m như tháp
# thật (mái 255 m + cột ăng-ten), bề ngang chân thân ~51 m (ảnh chụp từ dưới nên trong ảnh trông bè hơn).
import bpy, bmesh, math, os, json
import numpy as np
from mathutils import Vector, Matrix

ROOT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OW_DIR = ROOT + '/ArtSource/Blender/OpenWorld'
TEX_DIR = OW_DIR + '/thaptc_tex'
OUT_DIR = ROOT + '/ArtSource/Concepts/ThapTaiChinh-21293'
UNITY_DIR = ROOT + '/Assets/PhoViet/OpenWorld/ThapTaiChinh'
MODE = globals().get('MODE', 'preview')
os.makedirs(TEX_DIR, exist_ok=True); os.makedirs(OUT_DIR, exist_ok=True)
NAME = 'OW_ThapTaiChinh'

# ================================================================ texture (numpy, sRGB 0..1, gốc ảnh dưới-trái)
def srgb(h):
    h = h.lstrip('#'); return np.array([int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)], dtype=np.float32)

class Canvas:
    def __init__(self, w, h, seed):
        self.w, self.h = w, h
        self.c = np.zeros((h, w, 3), np.float32)
        self.ms = np.zeros((h, w, 2), np.float32)
        self.r = np.random.default_rng(seed)
        yy, xx = np.mgrid[0:h, 0:w]; self.X = xx / w; self.Y = yy / h
    def rect(self, x0, y0, x1, y1, col, metal=None, smooth=None):
        x0, x1 = int(round(x0)), int(round(x1)); y0, y1 = int(round(y0)), int(round(y1))
        x0, y0 = max(0, x0), max(0, y0); x1, y1 = min(self.w, x1), min(self.h, y1)
        if x1 <= x0 or y1 <= y0: return
        self.c[y0:y1, x0:x1] = col
        if metal is not None: self.ms[y0:y1, x0:x1, 0] = metal
        if smooth is not None: self.ms[y0:y1, x0:x1, 1] = smooth
    def mix(self, x0, y0, x1, y1, col, k):
        x0, x1 = int(round(x0)), int(round(x1)); y0, y1 = int(round(y0)), int(round(y1))
        x0, y0 = max(0, x0), max(0, y0); x1, y1 = min(self.w, x1), min(self.h, y1)
        if x1 <= x0 or y1 <= y0: return
        self.c[y0:y1, x0:x1] = self.c[y0:y1, x0:x1] * (1 - k) + col * k
    def grad(self, x0, y0, x1, y1, c0, c1, metal=None, smooth=None):
        x0, x1 = int(round(x0)), int(round(x1)); y0, y1 = int(round(y0)), int(round(y1))
        x0, y0 = max(0, x0), max(0, y0); x1, y1 = min(self.w, x1), min(self.h, y1)
        if x1 <= x0 or y1 <= y0: return
        t = np.linspace(0, 1, y1 - y0, dtype=np.float32)[:, None, None]
        self.c[y0:y1, x0:x1] = c0 * (1 - t) + c1 * t
        if metal is not None: self.ms[y0:y1, x0:x1, 0] = metal
        if smooth is not None: self.ms[y0:y1, x0:x1, 1] = smooth
    def lowfreq(self, amp, n=5, fx=(1, 4), fy=(1, 4)):
        f = np.zeros((self.h, self.w), np.float32)
        for _ in range(n):
            a, b = self.r.integers(fx[0], fx[1] + 1), self.r.integers(fy[0], fy[1] + 1)   # số nguyên → lặp liền mạch
            f += np.sin(self.X * a * 6.2832 + self.Y * b * 6.2832 + self.r.uniform(0, 6.28))
        return f / n * amp
    def save(self, name):
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
        ms = np.zeros((self.h, self.w, 4), np.float32)
        ms[..., 0] = ms[..., 1] = ms[..., 2] = self.ms[..., 0]; ms[..., 3] = self.ms[..., 1]
        return a, put(ms, name + '_MS', True)

# ---------------------------------------------------------------- 1) kính mặt dựng thân tháp: ô lặp 8 tấm × 6 tầng (tấm ~1,5 m, tầng 3,75 m)
def tex_kinh():
    # Ảnh mẫu: kính xanh dương đều màu, vạch dầm sàn sáng mảnh rõ từng tầng, đố dọc mảnh; ít đèn phòng (ban ngày).
    W, H = 512, 1024; cv = Canvas(W, H, 2931); r = cv.r
    npn, nf = 8, 6; pw, fh = W / npn, H / nf
    gd, gl = srgb('#2C5070'), srgb('#5C87AC')               # tối dưới → sáng trên (phản chiếu trời) trong mỗi tầng
    sp, mu = srgb('#B3C3CF'), srgb('#9DB0C0')
    sky = cv.lowfreq(1.0, 6, (1, 2), (1, 3))
    for f in range(nf):
        y0 = f * fh; sh = fh * 0.10
        for p in range(npn):
            x0 = p * pw; t = r.uniform(-0.018, 0.018)
            cv.grad(x0, y0 + sh, x0 + pw, y0 + fh, gd + t, gl + t, 0.4, 0.93)
            k = r.random(); gy0, gy1 = y0 + sh, y0 + fh
            if k < 0.14:                                      # đèn trần (dải sáng ấm mờ sát trần)
                cv.mix(x0 + 2, gy1 - 8, x0 + pw - 2, gy1 - 3, srgb('#E9E2CC'), 0.4)
            elif k < 0.22:
                cv.mix(x0, gy0, x0 + pw, gy1, srgb('#1A3047'), 0.22)   # phòng tối
        cv.rect(0, y0, W, y0 + sh, sp, 0.55, 0.62)
        cv.mix(0, y0 + sh - 2, W, y0 + sh, srgb('#E8EEF2'), 0.6)       # gờ sáng trên dầm
        cv.mix(0, y0, W, y0 + 2, srgb('#5E7385'), 0.5)                 # bóng dưới dầm
    cv.c += (sky[..., None] * np.array([0.06, 0.075, 0.09], np.float32)) * (cv.ms[..., 0:1] < 0.5)
    for p in range(npn + 1):                                  # đố dọc mảnh, đậm hơn mỗi 2 tấm
        x = p * pw; w = 2.2 if p % 2 == 0 else 1.2
        cv.rect(x - w, 0, x + w, H, mu, 0.85, 0.55)
    return cv.save('OW_ThapTC_Kinh')

# ---------------------------------------------------------------- 2) khối đế: A (trên) mặt kính cửa hàng 2 tầng cao; B (dưới-trái) ốp nhôm bạc;
#                                                                     C (dưới-phải) sảnh kính lối vào có cửa xoay
def tex_de():
    W, H = 1024, 1024; cv = Canvas(W, H, 2932); r = cv.r
    # --- A: y 512..1024 = cao 0..19 m (2 tầng cao 9,5 m), x 0..1024 = 24 m (16 tấm 1,5 m)
    y0 = 512; fh = 256
    for lv in range(2):
        ly = y0 + lv * fh
        for p in range(16):
            x0 = p * 64; t = r.uniform(-0.03, 0.03)
            if lv == 0:   # tầng trệt: kính trong, thấy trong sảnh/cửa hàng sáng đèn
                cv.grad(x0, ly + 10, x0 + 64, ly + fh, srgb('#8E9A9A') + t, srgb('#4F6E86') + t, 0.25, 0.9)
                cv.mix(x0 + 4, ly + 10, x0 + 60, ly + 70, srgb('#E2D2AE'), 0.45)       # hàng hoá/kệ sáng
                cv.mix(x0 + 2, ly + fh - 26, x0 + 62, ly + fh - 18, srgb('#FFF4D8'), 0.8)  # đèn trần
                if r.random() < 0.4: cv.mix(x0 + 18, ly + 12, x0 + 28, ly + 70, srgb('#2B2B30'), 0.5)  # bóng người
            else:
                cv.grad(x0, ly + 14, x0 + 64, ly + fh, srgb('#3D5F7E') + t, srgb('#86A8C4') + t, 0.4, 0.93)
                if r.random() < 0.35: cv.mix(x0 + 3, ly + fh - 22, x0 + 61, ly + fh - 16, srgb('#F3E9CF'), 0.7)
        cv.rect(0, ly, W, ly + (10 if lv == 0 else 14), srgb('#B8C2CA'), 0.8, 0.55)  # dầm ngang
    for p in range(17):
        x = p * 64; cv.rect(x - 2.5, 512, x + 2.5, 1024, srgb('#C9D2D9'), 0.85, 0.6)
        cv.rect(x + 31, 512, x + 33, 1024, srgb('#BAC4CC'), 0.85, 0.6)
    # --- B: x 0..512, y 0..512 ốp nhôm bạc 4×6 tấm, có vệt chải và mạch ghép
    cv.grad(0, 0, 512, 512, srgb('#9EA5AB'), srgb('#BCC2C7'), 0.55, 0.42)
    cv.c[0:512, 0:512] += cv.lowfreq(0.03, 4)[0:512, 0:512, None]
    for i in range(1, 4): cv.rect(i * 128 - 1.5, 0, i * 128 + 1.5, 512, srgb('#8D9398'), 0.8, 0.4)
    for j in range(1, 6): cv.rect(0, j * 85.3 - 1.5, 512, j * 85.3 + 1.5, srgb('#8D9398'), 0.8, 0.4)
    # --- C: x 512..1024, y 0..512 sảnh kính lối vào (17 m × 13,3 m): cửa xoay giữa, đố dày, sàn sáng
    cv.grad(512, 0, 1024, 512, srgb('#8B9697'), srgb('#3E5C76'), 0.3, 0.9)
    cv.mix(512, 0, 1024, 150, srgb('#E5D6B4'), 0.4)
    cv.mix(512, 440, 1024, 470, srgb('#FFF4D8'), 0.6)
    for p in range(9): cv.rect(512 + p * 64 - 3, 0, 512 + p * 64 + 3, 512, srgb('#C9D2D9'), 0.85, 0.6)
    cv.rect(512, 300, 1024, 310, srgb('#C9D2D9'), 0.85, 0.6)
    cx = 768; cv.rect(cx - 70, 0, cx + 70, 240, srgb('#5B6E7C'), 0.4, 0.9)         # cửa xoay
    cv.rect(cx - 74, 236, cx + 74, 252, srgb('#C2CBD2'), 0.85, 0.6)
    for d in (-35, 0, 35): cv.rect(cx + d - 2, 0, cx + d + 2, 236, srgb('#D5DCE1'), 0.85, 0.6)
    cv.mix(cx - 70, 0, cx + 70, 236, srgb('#F0E2C0'), 0.15)
    return cv.save('OW_ThapTC_De')

# ---------------------------------------------------------------- 3) phụ: bảng màu 4×4 ô (x 0..256) + ô lưới tấm trần đáy sân trực thăng (x 256..512)
PAL = ['silver', 'darkmetal', 'concrete', 'red', 'railglass', 'roof', 'text', 'white',
       'deck', 'seam', 'lightgrey', 'antenna', 'yellow', 'glassdark', 'steel', 'black']
PAL_C = dict(silver='#C9CED2', darkmetal='#3A4652', concrete='#CFCFCB', red='#E0241C', railglass='#A9C0CC', roof='#6E7378',
             text='#24262A', white='#EDEDEA', deck='#5A5F63', seam='#41505E', lightgrey='#DADDDF', antenna='#9AA0A6',
             yellow='#E6C229', glassdark='#2C4560', steel='#7F878E', black='#141618')
PAL_MS = dict(silver=(0.6, 0.45), darkmetal=(0.8, 0.5), seam=(0.8, 0.5), steel=(0.9, 0.5), antenna=(0.9, 0.5), railglass=(0.4, 0.92),
              glassdark=(0.4, 0.92), red=(0.0, 0.5))
def tex_phu():
    cv = Canvas(512, 256, 2933)
    for i, k in enumerate(PAL):
        x, y = (i % 4) * 64, (i // 4) * 64
        m, s = PAL_MS.get(k, (0.0, 0.25)); cv.rect(x, y, x + 64, y + 64, srgb(PAL_C[k]), m, s)
    cv.grad(256, 0, 512, 256, srgb('#CDD1D3'), srgb('#E3E6E7'), 0.3, 0.45)
    for i in range(9):
        cv.rect(256 + i * 32 - 1.5, 0, 256 + i * 32 + 1.5, 256, srgb('#9AA1A6'), 0.6, 0.4)
        cv.rect(256, i * 32 - 1.5, 512, i * 32 + 1.5, srgb('#9AA1A6'), 0.6, 0.4)
    return cv.save('OW_ThapTC_Phu')

def pal_uv(k):
    i = PAL.index(k); return ((i % 4) * 64 + 32) / 512.0, ((i // 4) * 64 + 32) / 256.0

def make_textures():
    return dict(Kinh=tex_kinh(), De=tex_de(), Phu=tex_phu())

def img_mat(name, img, ms):
    m = bpy.data.materials.get(name)
    if m: return m
    m = bpy.data.materials.new(name); m.use_nodes = True
    nt = m.node_tree; b = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    t = nt.nodes.new('ShaderNodeTexImage'); t.image = img
    nt.links.new(t.outputs['Color'], b.inputs['Base Color'])
    t2 = nt.nodes.new('ShaderNodeTexImage'); t2.image = ms
    sep = nt.nodes.new('ShaderNodeSeparateColor'); nt.links.new(t2.outputs['Color'], sep.inputs['Color'])
    nt.links.new(sep.outputs['Red'], b.inputs['Metallic'])
    inv = nt.nodes.new('ShaderNodeMath'); inv.operation = 'SUBTRACT'; inv.inputs[0].default_value = 1.0
    nt.links.new(t2.outputs['Alpha'], inv.inputs[1]); nt.links.new(inv.outputs['Value'], b.inputs['Roughness'])
    return m

# ================================================================ số đo theo ảnh mẫu
def px2x(px): return (px - 534.0) * 0.135
def py2z(py): return (1432.0 - py) * 0.185
def interp(tab, z):
    if z <= tab[0][0]: return tab[0][1]
    for (z0, v0), (z1, v1) in zip(tab, tab[1:]):
        if z <= z1: return v0 + (v1 - v0) * (z - z0) / (z1 - z0)
    return tab[-1][1]
Z_PETAL = 188.0        # đỉnh cánh trái (cánh ôm xoắn) — ngay dưới sân trực thăng
Z_DECK = 204.6         # mặt sàn sân trực thăng (~tầng 52)
TAB_L = [(0, -25.3), (py2z(1300), px2x(348)), (py2z(1100), px2x(360)), (py2z(900), px2x(372)), (py2z(700), px2x(385)),
         (py2z(500), px2x(400)), (Z_PETAL, px2x(411))]
TAB_R = [(0, px2x(722)), (py2z(1100), px2x(722)), (py2z(900), px2x(718)), (py2z(700), px2x(712)), (py2z(500), px2x(700)),
         (py2z(420), px2x(692)), (py2z(340), px2x(680)), (py2z(300), px2x(668)), (py2z(200), px2x(660)), (py2z(100), px2x(650)),
         (py2z(55), px2x(640))]
TAB_S = [(0, px2x(351)), (py2z(1300), px2x(353)), (py2z(1100), px2x(375)), (py2z(900), px2x(405)), (py2z(700), px2x(432)),
         (py2z(500), px2x(474)), (Z_PETAL, px2x(500))]
TAB_LB = [(Z_PETAL, -10.0), (Z_DECK, -5.6), (py2z(250), px2x(494)), (py2z(200), px2x(503)), (py2z(150), px2x(524)),
          (py2z(100), px2x(551)), (py2z(55), px2x(576))]
DEPTH = 0.80           # tiết diện elip: sâu = 0,8 × ngang
PETAL_OFF = 0.9        # cánh trái nổi ra khỏi thân 0,9 m (đường xoắn chéo thấy rõ từ xa)
PETAL_SPAN = math.radians(170)
Z_ROOF0 = 252.0        # mái vát: thấp 252 m bên trái → 255,2 m bên phải
ANT_X, ANT_TOP = px2x(628), 262.5
CYL = (-6.6, 0.0, 8.8)                                       # trụ kính dưới sân trực thăng (tâm x, y, bán kính)
DISK = (-16.0, -1.5, 18.0)                                   # sân trực thăng (tâm x, y, bán kính)

def sect(z, upper=False):
    L = interp(TAB_LB if upper else TAB_L, z); R = interp(TAB_R, z)
    cx = (L + R) / 2; a = (R - L) / 2
    return cx, a, a * DEPTH

# phân số chu vi theo góc (elip tỉ lệ cố định → một bảng dùng chung) để u đều và đố kính thẳng đứng
_TH = np.linspace(0, 2 * math.pi, 2049)
_seg = np.hypot(np.diff(np.cos(_TH)), np.diff(DEPTH * np.sin(_TH)))
_FR = np.concatenate([[0], np.cumsum(_seg)]) / np.sum(_seg)
def ufrac(th):
    k = math.floor(th / (2 * math.pi)); t = th - k * 2 * math.pi
    return k + float(np.interp(t, _TH, _FR))

def epos(cx, a, b, th, z, off=0.0): return (cx + (a + off) * math.cos(th), (b + off) * math.sin(th), z)
def etan(a, b, th): return Vector((-a * math.sin(th), b * math.cos(th), 0)).normalized()

def seam_theta(z):
    cx, a, b = sect(z); c = max(-1.0, min(1.0, (interp(TAB_S, z) - cx) / a))
    return 2 * math.pi - math.acos(c)

def roof_z(x):
    L, R = interp(TAB_LB, 254.0), interp(TAB_R, 254.0)
    return Z_ROOF0 + 3.2 * max(0.0, min(1.0, (x - L) / (R - L)))

# ================================================================ lưới
class B:
    def __init__(s, name, mats):
        s.name = name; s.bm = bmesh.new(); s.uv = s.bm.loops.layers.uv.verify(); s.mats = mats
    def mi(s, k): return s.mats.index(k)
    def face(s, pts, uvs, mat, want=None, smooth=False):
        if want is not None:
            P = [Vector(p) for p in pts]; n = Vector((0, 0, 0))
            for i in range(len(P)): n += P[i].cross(P[(i + 1) % len(P)])
            if n.dot(Vector(want)) < 0: pts, uvs = pts[::-1], uvs[::-1]
        vs = [s.bm.verts.new(p) for p in pts]
        try: f = s.bm.faces.new(vs)
        except ValueError: return None
        f.material_index = s.mi(mat); f.smooth = smooth
        for l, uv in zip(f.loops, uvs): l[s.uv].uv = uv
        return f
    def quad_pal(s, pts, key, want=None):
        u = pal_uv(key); return s.face(pts, [u] * len(pts), 'Phu', want)
    def grid(s, rows, uvs, mat, closed, want, smooth=True):
        """rows: các vòng điểm; uvs: cùng số hàng, mỗi hàng n+1 (closed) hoặc n giá trị. want(center)->hướng pháp tuyến mong muốn."""
        vr = [[s.bm.verts.new(p) for p in r] for r in rows]; n = len(rows[0]); faces = []
        for i in range(len(rows) - 1):
            for k in range(n if closed else n - 1):
                k2 = (k + 1) % n
                try: f = s.bm.faces.new([vr[i][k], vr[i][k2], vr[i + 1][k2], vr[i + 1][k]])
                except ValueError: continue
                f.material_index = s.mi(mat); f.smooth = smooth
                for l, u in zip(f.loops, [uvs[i][k], uvs[i][k + 1], uvs[i + 1][k + 1], uvs[i + 1][k]]): l[s.uv].uv = u
                faces.append(f)
        s.bm.normal_update()
        bad = [f for f in faces if f.normal.dot(Vector(want(f.calc_center_median()))) < 0]
        if bad: bmesh.ops.reverse_faces(s.bm, faces=bad)
        return vr
    def box(s, x0, y0, z0, x1, y1, z1, key, bottom=False):
        c = [(x0, y0, z0), (x1, y0, z0), (x1, y1, z0), (x0, y1, z0), (x0, y0, z1), (x1, y0, z1), (x1, y1, z1), (x0, y1, z1)]
        F = [((0, 1, 5, 4), (0, -1, 0)), ((1, 2, 6, 5), (1, 0, 0)), ((2, 3, 7, 6), (0, 1, 0)), ((3, 0, 4, 7), (-1, 0, 0)), ((4, 5, 6, 7), (0, 0, 1))]
        if bottom: F.append(((0, 3, 2, 1), (0, 0, -1)))
        for idx, w in F: s.quad_pal([c[i] for i in idx], key, w)
    def cyl(s, cx, cy, r0, r1, z0, z1, seg, key, top=True):
        rows = [[(cx + r0 * math.cos(2 * math.pi * k / seg), cy + r0 * math.sin(2 * math.pi * k / seg), z0) for k in range(seg)],
                [(cx + r1 * math.cos(2 * math.pi * k / seg), cy + r1 * math.sin(2 * math.pi * k / seg), z1) for k in range(seg)]]
        u = pal_uv(key); s.grid(rows, [[u] * (seg + 1)] * 2, 'Phu', True, lambda c: (c.x - cx, c.y - cy, 0))
        if top: s.face([p for p in rows[1]], [u] * seg, 'Phu', (0, 0, 1))
    def ntris(s): return sum(len(f.verts) - 2 for f in s.bm.faces)
    def finish(s, coll, matobjs):
        me = bpy.data.meshes.new(s.name); s.bm.to_mesh(me); s.bm.free()
        for k in s.mats: me.materials.append(matobjs[k])
        ob = bpy.data.objects.new(s.name, me); coll.objects.link(ob)
        return ob

def zlist(z0, z1, step):
    n = max(1, int(math.ceil((z1 - z0) / step - 1e-6)))
    return [z0 + (z1 - z0) * i / n for i in range(n + 1)]

LODP = {0: dict(n=64, step=7.5, m=40, nb=48, bstep=3.75, ds=48, wall=28, detail=2),
        1: dict(n=32, step=15.0, m=18, nb=24, bstep=7.5, ds=24, wall=12, detail=1),
        2: dict(n=16, step=31.3, m=8, nb=12, bstep=16.0, ds=12, wall=6, detail=0)}
NREP_BODY, NREP_BUD = 12, 8           # số ô lặp 12 m quanh chu vi (đố dọc liền nhau, tấm thu nhỏ dần theo độ thuôn)
TV = 22.5                             # v: 6 tầng × 3,75 m

def build(lod, coll, matobjs):
    P = LODP[lod]; b = B('%s_LOD%d' % (NAME, lod), ['Kinh', 'De', 'Phu'])
    axis = lambda z, up=False: sect(z, up)[0]
    # ---------------- thân dưới (0 → Z_PETAL), tiết diện elip thuôn dần
    zs = zlist(0.0, Z_PETAL, P['step']); n = P['n']
    ths = [2 * math.pi * k / n for k in range(n)]
    rows, uvs = [], []
    for z in zs:
        cx, a, bb = sect(z); rows.append([epos(cx, a, bb, t, z) for t in ths])
        uvs.append([(ufrac(t) * NREP_BODY, z / TV) for t in ths] + [(NREP_BODY, z / TV)])
    b.grid(rows, uvs, 'Kinh', True, lambda c: (c.x - axis(c.z), c.y, 0))
    cx, a, bb = sect(Z_PETAL)
    b.face([epos(cx, a, bb, t, Z_PETAL) for t in ths], [pal_uv('roof')] * n, 'Phu', (0, 0, 1))
    # ---------------- cánh trái ôm xoắn (nổi PETAL_OFF), mép trước = đường xoắn chéo trong ảnh
    m = P['m']; prow, puv, edgeA, edgeB = [], [], [], []
    for z in zs:
        cx, a, bb = sect(z); ts = seam_theta(z); th = [ts - PETAL_SPAN + PETAL_SPAN * j / m for j in range(m + 1)]
        prow.append([epos(cx, a, bb, t, z, PETAL_OFF) for t in th])
        puv.append([(ufrac(t) * NREP_BODY * (a + PETAL_OFF) / a, z / TV) for t in th])
        edgeA.append((epos(cx, a, bb, ts, z), epos(cx, a, bb, ts, z, PETAL_OFF), etan(a, bb, ts)))
        edgeB.append((epos(cx, a, bb, th[0], z), epos(cx, a, bb, th[0], z, PETAL_OFF), -etan(a, bb, th[0])))
    b.grid(prow, puv, 'Kinh', False, lambda c: (c.x - axis(c.z), c.y, 0))
    for edge, key in ((edgeA, 'seam'), (edgeB, 'seam')):
        for i in range(len(edge) - 1):
            (p0, q0, w0), (p1, q1, w1) = edge[i], edge[i + 1]
            b.quad_pal([p0, q0, q1, p1], key, w0 + w1)
    if P['detail'] >= 1:                                    # nẹp nhôm sáng dọc mép cánh (vạch sáng cạnh đường xoắn)
        for i in range(len(zs) - 1):
            pts = []
            for z in (zs[i], zs[i + 1]):
                cx, a, bb = sect(z); ts = seam_theta(z); d = 0.35 / a
                pts.append((epos(cx, a, bb, ts - d, z, PETAL_OFF + 0.06), epos(cx, a, bb, ts, z, PETAL_OFF + 0.06)))
            b.quad_pal([pts[0][0], pts[0][1], pts[1][1], pts[1][0]], 'silver', Vector(pts[0][1]) - Vector((axis(zs[i]), 0, zs[i])))
    if P['detail'] >= 1:                                    # dải kính tối sát mép cánh trên thân (đường xoắn đậm như ảnh)
        for i in range(len(zs) - 1):
            pts = []
            for z in (zs[i], zs[i + 1]):
                cx, a, bb = sect(z); ts = seam_theta(z); d = 0.6 / a
                pts.append((epos(cx, a, bb, ts, z, 0.05), epos(cx, a, bb, ts + d, z, 0.05)))
            b.quad_pal([pts[0][0], pts[0][1], pts[1][1], pts[1][0]], 'seam', Vector(pts[0][1]) - Vector((axis(zs[i]), 0, zs[i])))
    cx, a, bb = sect(Z_PETAL); ts = seam_theta(Z_PETAL)
    th = [ts - PETAL_SPAN + PETAL_SPAN * j / m for j in range(m + 1)]
    for j in range(m):                                      # mép trên cánh
        t0, t1 = th[j], th[j + 1]
        b.quad_pal([epos(cx, a, bb, t0, Z_PETAL), epos(cx, a, bb, t1, Z_PETAL), epos(cx, a, bb, t1, Z_PETAL, PETAL_OFF),
                    epos(cx, a, bb, t0, Z_PETAL, PETAL_OFF)], 'silver', (0, 0, 1))
    # ---------------- phần ngọn (búp) Z_PETAL → mái vát, lệch phải
    nb = P['nb']; thb = [2 * math.pi * k / nb for k in range(nb)]
    zb = zlist(Z_PETAL, Z_ROOF0 - 2.0, P['bstep']); rows, uvs = [], []
    for z in zb:
        cx, a, bb = sect(z, True); rows.append([epos(cx, a, bb, t, z) for t in thb])
        uvs.append([(ufrac(t) * NREP_BUD, z / TV) for t in thb] + [(NREP_BUD, z / TV)])
    cx, a, bb = sect(254.0, True); top = []
    for t in thb:
        x, y, _ = epos(cx, a, bb, t, 0); top.append((x, y, roof_z(x)))
    rows.append(top); uvs.append([(ufrac(t) * NREP_BUD, p[2] / TV) for t, p in zip(thb, top)] + [(NREP_BUD, top[0][2] / TV)])
    b.grid(rows, uvs, 'Kinh', True, lambda c: (c.x - axis(min(c.z, 254.0), True), c.y, 0))
    b.face(top, [pal_uv('roof')] * nb, 'Phu', (0, 0, 1))
    # ---------------- trụ kính dưới sân trực thăng
    ccx, ccy, cr = CYL; ds = P['ds']; thc = [2 * math.pi * k / ds for k in range(ds)]
    zc = [Z_PETAL - 2.0, Z_DECK - 2.0]
    b.grid([[(ccx + cr * math.cos(t), ccy + cr * math.sin(t), z) for t in thc] for z in zc],
           [[(k / ds * 5, z / TV) for k in range(ds + 1)] for z in zc], 'Kinh', True, lambda c: (c.x - ccx, c.y - ccy, 0))
    # ---------------- sân trực thăng: đáy chén (lưới tấm trần), viền nhôm, sàn, lan can kính
    dx, dy, dr = DISK; thd = [2 * math.pi * k / ds for k in range(ds + 1)]
    z_rim0, z_rim1 = Z_DECK - 1.4, Z_DECK
    rin, zin = 10.0, Z_DECK - 6.0
    def ring(r, z): return [(dx + r * math.cos(t), dy + r * math.sin(t), z) for t in thd[:-1]]
    gu = lambda k, v: (0.5 + 0.5 * ((k * 8.0 / ds) % 1.0001), v)   # nửa phải texture Phu: ô lưới tấm trần
    R3 = [(rin, zin), ((rin + dr) / 2, (zin + z_rim0) / 2 - 0.6), (dr, z_rim0)]
    for j in range(2):
        (ra, za), (rb, zb_) = R3[j], R3[j + 1]
        for k in range(ds):
            t0, t1 = thd[k], thd[k + 1]
            pts = [(dx + ra * math.cos(t0), dy + ra * math.sin(t0), za), (dx + ra * math.cos(t1), dy + ra * math.sin(t1), za),
                   (dx + rb * math.cos(t1), dy + rb * math.sin(t1), zb_), (dx + rb * math.cos(t0), dy + rb * math.sin(t0), zb_)]
            v0, v1 = 0.02 + 0.48 * j, 0.5 + 0.48 * j
            b.face(pts, [(0.5, v0), (0.625, v0), (0.625, v1), (0.5, v1)], 'Phu', (0, 0, -1))
    b.face(ring(rin, zin)[::-1], [pal_uv('concrete')] * ds, 'Phu', (0, 0, -1))
    u = pal_uv('silver')
    b.grid([ring(dr, z_rim0), ring(dr, z_rim1)], [[u] * (ds + 1)] * 2, 'Phu', True, lambda c: (c.x - dx, c.y - dy, 0))
    b.face(ring(dr, z_rim1), [pal_uv('deck')] * ds, 'Phu', (0, 0, 1))
    if P['detail'] >= 1:
        ug = pal_uv('railglass')
        b.grid([ring(dr - 0.15, z_rim1), ring(dr - 0.15, z_rim1 + 1.2)], [[ug] * (ds + 1)] * 2, 'Phu', True, lambda c: (c.x - dx, c.y - dy, 0))
        b.grid([ring(dr - 0.15, z_rim1 + 1.2), ring(dr - 0.15, z_rim1)], [[ug] * (ds + 1)] * 2, 'Phu', True, lambda c: (dx - c.x, dy - c.y, 0))
        for k in range(0, ds, 2 if P['detail'] >= 2 else 4):   # cột lan can
            t = 2 * math.pi * k / ds; x, y = dx + (dr - 0.15) * math.cos(t), dy + (dr - 0.15) * math.sin(t)
            b.box(x - 0.07, y - 0.07, z_rim1, x + 0.07, y + 0.07, z_rim1 + 1.3, 'steel')
        t = math.radians(150); x, y = dx + (dr - 0.6) * math.cos(t), dy + (dr - 0.6) * math.sin(t)
        b.box(x - 0.3, y - 0.3, z_rim1, x + 0.3, y + 0.3, z_rim1 + 0.9, 'red')                   # đèn hiệu đỏ
    if P['detail'] >= 2:                                     # gân đáy chén
        for k in range(0, ds, 3):
            t = 2 * math.pi * k / ds; c, s_ = math.cos(t), math.sin(t)
            p0 = Vector((dx + rin * c, dy + rin * s_, zin)); p1 = Vector((dx + dr * c, dy + dr * s_, z_rim0))
            side = Vector((-s_, c, 0)) * 0.12; dn = Vector((0, 0, -0.25))
            b.quad_pal([p0 - side + dn, p1 - side + dn, p1 + side + dn, p0 + side + dn], 'lightgrey', (0, 0, -1))
            b.quad_pal([p0 - side, p1 - side, p1 - side + dn, p0 - side + dn], 'lightgrey', -side)
            b.quad_pal([p0 + side + dn, p1 + side + dn, p1 + side, p0 + side], 'lightgrey', side)
    # ---------------- mái: thiết bị + cột ăng-ten đỉnh đỏ
    if P['detail'] >= 1:
        zr = roof_z(9.0); b.box(7.0, -2.2, zr - 0.5, 10.5, 1.8, zr + 1.9, 'steel')
        zr = roof_z(ANT_X); b.box(ANT_X - 1.2, -1.2, zr - 0.5, ANT_X + 1.2, 1.2, zr + 1.2, 'roof')
        b.cyl(ANT_X, 0.0, 0.42, 0.22, zr + 1.2, ANT_TOP - 0.6, 8, 'antenna')
        b.box(ANT_X - 0.3, -0.3, ANT_TOP - 0.6, ANT_X + 0.3, 0.3, ANT_TOP, 'red')
        if P['detail'] >= 2:
            b.cyl(ANT_X - 1.6, 0.6, 0.1, 0.06, zr + 1.2, zr + 4.5, 6, 'antenna')
    # ---------------- khối đế: vách kính cong ôm chân tháp, mép trên vát lên phải (dải nhôm bạc) + khối ốp nhôm có biển tên
    cx0, a0, b0 = sect(0.0)
    def off(t): s_ = max(0.0, min(1.0, (t - math.radians(185)) / math.radians(55))); return 1.2 + 5.8 * s_ * s_ * (3 - 2 * s_)
    def hx(x): return py2z(1302) + (py2z(1236) - py2z(1302)) * max(0.0, min(1.0, (x - px2x(345)) / (BOX_X0 - px2x(345))))
    BOX_X0, BOX_X1 = px2x(620), px2x(742)
    lo, hi = math.radians(270), math.radians(359)            # tìm góc vách chạm mép trái khối nhôm
    for _ in range(40):
        mid = (lo + hi) / 2
        if epos(cx0, a0, b0, mid, 0, off(mid))[0] < BOX_X0 + 0.4: lo = mid
        else: hi = mid
    tA, tB = math.radians(160), lo
    wn = P['wall']; tw = [tA + (tB - tA) * k / wn for k in range(wn + 1)]
    wp = [epos(cx0, a0, b0, t, 0, off(t)) for t in tw]
    sacc = [0.0]
    for i in range(1, len(wp)): sacc.append(sacc[-1] + math.hypot(wp[i][0] - wp[i - 1][0], wp[i][1] - wp[i - 1][1]))
    Z_SHOP = py2z(1330)
    for i in range(wn):
        (x0, y0, _), (x1, y1, _) = wp[i], wp[i + 1]; h0, h1 = hx(x0), hx(x1)
        nrm = (y1 - y0, -(x1 - x0), 0)
        n2 = Vector(nrm)
        if n2.dot(Vector((x0 + x1 - 2 * cx0, y0 + y1, 0))) < 0: nrm = (-nrm[0], -nrm[1], 0)
        u0, u1 = sacc[i] / 24.0, sacc[i + 1] / 24.0
        b.face([(x0, y0, 0), (x1, y1, 0), (x1, y1, Z_SHOP), (x0, y0, Z_SHOP)], [(u0, 0.5), (u1, 0.5), (u1, 1.0), (u0, 1.0)], 'De', nrm)
        k0, k1 = sacc[i] / 12.0, sacc[i + 1] / 12.0
        b.face([(x0, y0, Z_SHOP), (x1, y1, Z_SHOP), (x1, y1, h1 - 1.6), (x0, y0, h0 - 1.6)],
               [(k0, Z_SHOP / TV), (k1, Z_SHOP / TV), (k1, (h1 - 1.6) / TV), (k0, (h0 - 1.6) / TV)], 'Kinh', nrm)
        b.quad_pal([(x0, y0, h0 - 1.6), (x1, y1, h1 - 1.6), (x1, y1, h1), (x0, y0, h0)], 'silver', nrm)
        # mái đế: từ mép vách vào thân tháp (cùng góc)
        ia = epos(cx0, a0, b0, tw[i], h0); ib = epos(cx0, a0, b0, tw[i + 1], h1)
        b.quad_pal([(x0, y0, h0), (x1, y1, h1), ib, ia], 'concrete', (0, 0, 1))
    x0, y0, _ = wp[0]; ia = epos(cx0, a0, b0, tw[0], 0); h0 = hx(x0)
    b.quad_pal([(x0, y0, 0), ia, (ia[0], ia[1], h0), (x0, y0, h0)], 'silver', etan(a0, b0, tw[0]) * -1)
    # khối nhôm bạc + sảnh kính lối vào
    yF = wp[-1][1] - 0.6; yB = -6.0; zG, zT = py2z(1360), py2z(1232)
    bw = BOX_X1 - BOX_X0
    b.face([(BOX_X0, yF, 0), (BOX_X1, yF, 0), (BOX_X1, yF, zG), (BOX_X0, yF, zG)], [(0.5, 0), (1, 0), (1, 0.5), (0.5, 0.5)], 'De', (0, -1, 0))
    b.face([(BOX_X0, yF, zG), (BOX_X1, yF, zG), (BOX_X1, yF, zT), (BOX_X0, yF, zT)], [(0, 0), (0.5, 0), (0.5, 0.5), (0, 0.5)], 'De', (0, -1, 0))
    dep = yB - yF
    b.face([(BOX_X1, yF, 0), (BOX_X1, yB, 0), (BOX_X1, yB, zG), (BOX_X1, yF, zG)], [(0, 0.5), (dep / 24, 0.5), (dep / 24, 0.5 + 0.5 * zG / 19), (0, 0.5 + 0.5 * zG / 19)], 'De', (1, 0, 0))
    b.face([(BOX_X1, yF, zG), (BOX_X1, yB, zG), (BOX_X1, yB, zT), (BOX_X1, yF, zT)], [(0, 0), (0.5, 0), (0.5, 0.5), (0, 0.5)], 'De', (1, 0, 0))
    b.face([(BOX_X0, yF, zG), (BOX_X0, yB, zG), (BOX_X0, yB, zT), (BOX_X0, yF, zT)], [(0, 0), (0.5, 0), (0.5, 0.5), (0, 0.5)], 'De', (-1, 0, 0))
    b.quad_pal([(BOX_X0, yF, zT), (BOX_X1, yF, zT), (BOX_X1, yB, zT), (BOX_X0, yB, zT)], 'silver', (0, 0, 1))
    if P['detail'] >= 1:                                     # mái che lối vào
        b.box(BOX_X0 + 3.0, yF - 3.0, 9.6, BOX_X1 - 3.0, yF, 10.2, 'silver', bottom=True)
    if P['detail'] >= 2:                                     # biển tên trên mặt khối nhôm (chữ có gờ nổi)
        add_sign(b, px2x(650), yF, py2z(1283))
    return b

def add_sign(b, x0, yF, z0):
    font = None
    for f in ('C:/Windows/Fonts/timesbd.ttf', 'C:/Windows/Fonts/times.ttf'):
        if os.path.exists(f): font = bpy.data.fonts.load(f, check_existing=True); break
    cu = bpy.data.curves.new('Bien', 'FONT'); cu.body = 'BITEXCO\nFINANCIAL\nTOWER'
    if font: cu.font = font
    cu.size = 4.0; cu.space_line = 0.92; cu.extrude = 0.06; cu.resolution_u = 3; cu.align_x = 'LEFT'
    ob = bpy.data.objects.new('Bien', cu); bpy.context.scene.collection.objects.link(ob)
    dg = bpy.context.evaluated_depsgraph_get(); me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
    M = Matrix.Translation((x0, yF - 0.07, z0)) @ Matrix.Rotation(math.radians(90), 4, 'X') @ Matrix.Diagonal((0.36, 1.0, 1.0, 1.0))
    me.transform(M)
    off = len(b.bm.verts); vmap = [b.bm.verts.new(v.co) for v in me.vertices]
    u = pal_uv('text')
    for p in me.polygons:
        try: f = b.bm.faces.new([vmap[i] for i in p.vertices])
        except ValueError: continue
        f.material_index = b.mi('Phu')
        for l in f.loops: l[b.uv].uv = u
    bpy.data.objects.remove(ob); bpy.data.curves.remove(cu); bpy.data.meshes.remove(me)

def build_collider(coll, matobjs):
    """Lăng trụ lồi thấp (chân tháp + đế + khối nhôm) cho MeshCollider convex trong Unity."""
    b = B(NAME + '_VaCham', ['Phu']); cx0, a0, b0 = sect(0.0)
    pts = [epos(cx0, a0 + 1.2, b0 + 1.2, 2 * math.pi * k / 20, 0)[:2] for k in range(20)]
    pts += [(px2x(620), -27.5), (px2x(742), -27.5), (px2x(742), -6.0)]
    from mathutils.geometry import convex_hull_2d
    hull = [pts[i] for i in convex_hull_2d(pts)]
    u = pal_uv('concrete')
    for i in range(len(hull)):
        (x0, y0), (x1, y1) = hull[i], hull[(i + 1) % len(hull)]
        b.face([(x0, y0, 0), (x1, y1, 0), (x1, y1, 40), (x0, y0, 40)], [u] * 4, 'Phu', (y1 - y0, x0 - x1, 0) if True else None)
    b.face([(x, y, 40) for x, y in hull], [u] * len(hull), 'Phu', (0, 0, 1))
    b.face([(x, y, 0) for x, y in hull], [u] * len(hull), 'Phu', (0, 0, -1))
    return b.finish(coll, matobjs)

# ================================================================ cảnh render duyệt
def setup_scene():
    sc = bpy.context.scene
    for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
    try: sc.render.engine = 'CYCLES'
    except TypeError: pass
    sc.cycles.samples = 40; sc.cycles.use_denoising = True
    try: sc.cycles.device = 'CPU'
    except Exception: pass
    sc.view_settings.view_transform = 'Standard'; sc.view_settings.exposure = -1.6
    w = bpy.data.worlds.new('Sky'); sc.world = w; w.use_nodes = True
    nt = w.node_tree; bg = next(n for n in nt.nodes if n.type == 'BACKGROUND')
    sky = nt.nodes.new('ShaderNodeTexSky')
    for st in ('MULTIPLE_SCATTERING', 'NISHITA'):
        try: sky.sky_type = st; break
        except Exception: pass
    try: sky.sun_elevation = math.radians(40); sky.sun_rotation = math.radians(200)
    except Exception: pass
    nt.links.new(sky.outputs['Color'], bg.inputs['Color']); bg.inputs['Strength'].default_value = 0.25
    sun = bpy.data.lights.new('Sun', 'SUN'); sun.energy = 2.4; sun.angle = math.radians(1.5)
    so = bpy.data.objects.new('Sun', sun); sc.collection.objects.link(so); so.rotation_euler = (math.radians(50), 0, math.radians(200))
    me = bpy.data.meshes.new('Ground'); s = 2000
    me.from_pydata([(-s, -s, 0), (s, -s, 0), (s, s, 0), (-s, s, 0)], [], [(0, 1, 2, 3)])
    m = bpy.data.materials.new('Ground'); m.use_nodes = True
    next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED').inputs['Base Color'].default_value = (0.35, 0.35, 0.34, 1)
    me.materials.append(m); g = bpy.data.objects.new('Ground', me); sc.collection.objects.link(g)
    return sc

def camera(name, loc, target, lens=35):
    cd = bpy.data.cameras.new(name); cd.lens = lens; cd.clip_end = 6000
    co = bpy.data.objects.new(name, cd); bpy.context.scene.collection.objects.link(co)
    co.location = loc; co.rotation_euler = (Vector(target) - Vector(loc)).to_track_quat('-Z', 'Y').to_euler()
    return co

def render(cam, path, rx, ry):
    sc = bpy.context.scene; sc.camera = cam; sc.render.resolution_x, sc.render.resolution_y = rx, ry
    sc.render.filepath = path; bpy.ops.render.render(write_still=True); print('RENDER', path)

def materials(imgs): return {k: img_mat('OW_ThapTC_' + k, imgs[k][0], imgs[k][1]) for k in imgs}

def run_preview():
    imgs = make_textures(); setup_scene(); mats = materials(imgs)
    coll = bpy.data.collections.new('ThapTaiChinh'); bpy.context.scene.collection.children.link(coll)
    rep = []
    for lod in range(3):
        bb = build(lod, coll, mats); rep.append(bb.ntris()); ob = bb.finish(coll, mats)
        if lod: ob.location.x = 90 * lod; ob.hide_render = True
    print('TRIS', rep)
    render(camera('Cam_Mau', (0, -330, 8), (0, 0, 128), 40), OUT_DIR + '/thap-chinh-dien.png', 1000, 1350)
    render(camera('Cam_Goc', (-190, -230, 30), (0, 0, 120), 26), OUT_DIR + '/thap-goc-trai.png', 1000, 1350)
    render(camera('Cam_De', (35, -95, 6), (0, -10, 22), 28), OUT_DIR + '/thap-de.png', 1400, 900)
    render(camera('Cam_San', (-60, -90, 170), (-10, 0, 205), 30), OUT_DIR + '/thap-san-truc-thang.png', 1400, 900)
    bpy.ops.wm.save_as_mainfile(filepath=OW_DIR + '/PhoViet_OW_ThapTaiChinh_21293.blend')

def run_export():
    import shutil
    imgs = make_textures()
    for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
    mats = materials(imgs)
    coll = bpy.data.collections.new('X'); bpy.context.scene.collection.children.link(coll)
    for d in ('Models', 'Textures'): os.makedirs(UNITY_DIR + '/' + d, exist_ok=True)
    for f in os.listdir(TEX_DIR):
        if f.endswith('.png'): shutil.copyfile(TEX_DIR + '/' + f, UNITY_DIR + '/Textures/' + f)
    obs, tris = [], []
    for lod in range(3):
        bb = build(lod, coll, mats); tris.append(bb.ntris()); obs.append(bb.finish(coll, mats))
    obs.append(build_collider(coll, mats))
    bpy.ops.object.select_all(action='DESELECT')
    for ob in obs: ob.select_set(True)
    bpy.context.view_layer.objects.active = obs[0]
    bpy.ops.export_scene.fbx(filepath=UNITY_DIR + '/Models/' + NAME + '.fbx', use_selection=True, object_types={'MESH'},
                             axis_forward='-Z', axis_up='Y', apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                             bake_anim=False, add_leaf_bones=False, use_mesh_modifiers=True, mesh_smooth_type='FACE', path_mode='STRIP')
    print('EXPORT', NAME, tris)

if MODE == 'export': run_export()
elif MODE == 'tex': make_textures()
elif MODE == 'preview': run_preview()
