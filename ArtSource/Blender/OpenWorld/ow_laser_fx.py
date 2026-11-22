"""21.300 — hiệu ứng tia LAZER (xe người chơi phóng 2 tia từ 2 đèn pha) dựng bằng Blender, chạy nền (không mở/ghi đè .blend có sẵn):
  blender -b --factory-startup --python ArtSource/Blender/OpenWorld/ow_laser_fx.py
Kết quả (nền đen, Unity cộng sáng — đen = không cộng):
  Assets/PhoViet/Resources/Laser/PV_LazerTia.png   1024×128  thân tia: lõi trắng nóng + 3 sợi năng lượng đỏ xoắn quanh lõi (3 vòng/khung,
                                                    lặp liền mạch theo chiều dài để Unity cuộn chạy dọc tia) + quầng đỏ nhiều lớp
  Assets/PhoViet/Resources/Laser/PV_LazerNong.png  256²  chớp đầu nòng (đèn pha): lõi tròn + 2 vệt sao chữ thập + 6 tia mảnh + quầng
  Assets/PhoViet/Resources/Laser/PV_LazerTrung.png 256²  chớp chỗ tia trúng: lõi + vòng sóng + 10 tia lửa tỏa + quầng
Hình học phát sáng (Emission) render Eevee chiếu thẳng; quầng sáng (bloom) cộng bằng numpy nhiều lớp mờ Gauss để không phụ thuộc
nút Compositor (API đổi ở Blender 5). Cảnh lưu ArtSource/Blender/OpenWorld/PhoViet_OW_Lazer_21300.blend (file mới).
"""
import bpy, math, os, random
import numpy as np

ROOT = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT = ROOT + '/Assets/PhoViet/Resources/Laser'
TMP = ROOT + '/ArtSource/Concepts/Laser-21300'
BLEND = ROOT + '/ArtSource/Blender/OpenWorld/PhoViet_OW_Lazer_21300.blend'
os.makedirs(OUT, exist_ok=True); os.makedirs(TMP, exist_ok=True)
RED = (1.0, 0.10, 0.06)

bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene
for eng in ('BLENDER_EEVEE', 'BLENDER_EEVEE_NEXT', 'CYCLES'):
    try:
        sc.render.engine = eng; break
    except TypeError:
        pass
if sc.render.engine == 'CYCLES':
    sc.cycles.samples = 32
sc.render.film_transparent = False
sc.render.image_settings.file_format = 'PNG'; sc.render.image_settings.color_mode = 'RGB'
sc.render.image_settings.color_depth = '16'
sc.view_settings.view_transform = 'Standard'
world = bpy.data.worlds.new('Den'); world.use_nodes = True
bgn = next(n for n in world.node_tree.nodes if n.type == 'BACKGROUND'); bgn.inputs[0].default_value = (0, 0, 0, 1); bgn.inputs[1].default_value = 0
sc.world = world

cam = bpy.data.objects.new('Cam', bpy.data.cameras.new('Cam')); cam.data.type = 'ORTHO'
cam.location = (0, 0, 10); cam.rotation_euler = (0, 0, 0)
sc.collection.objects.link(cam); sc.camera = cam


def emit(name, col, strength):
    m = bpy.data.materials.new(name); m.use_nodes = True
    nt = m.node_tree; nt.nodes.clear()
    o = nt.nodes.new('ShaderNodeOutputMaterial'); e = nt.nodes.new('ShaderNodeEmission')
    e.inputs['Color'].default_value = (*col, 1); e.inputs['Strength'].default_value = strength
    nt.links.new(e.outputs[0], o.inputs['Surface']); return m


def tube(name, pts, radius, mat):
    cu = bpy.data.curves.new(name, 'CURVE'); cu.dimensions = '3D'; cu.bevel_depth = radius; cu.bevel_resolution = 3
    sp = cu.splines.new('POLY'); sp.points.add(len(pts) - 1)
    for i, p in enumerate(pts): sp.points[i].co = (*p, 1)
    ob = bpy.data.objects.new(name, cu); ob.data.materials.append(mat); sc.collection.objects.link(ob); return ob


def clear_geo():
    for ob in list(sc.collection.objects):
        if ob.type != 'CAMERA': bpy.data.objects.remove(ob, do_unlink=True)


def render(path, w, h, ortho):
    sc.render.resolution_x, sc.render.resolution_y = w, h; sc.render.resolution_percentage = 100
    cam.data.ortho_scale = ortho
    sc.render.filepath = path; bpy.ops.render.render(write_still=True)
    img = bpy.data.images.load(path)
    px = np.array(img.pixels[:], np.float32).reshape(h, w, 4)[..., :3]; bpy.data.images.remove(img)
    return px                                                   # gốc dưới-trái


def blur(a, s, wrap_x=False):
    """Gauss xấp xỉ bằng 3 lượt hộp (tách trục); wrap_x: lặp theo x (thân tia liền mạch)."""
    r = max(1, int(s))
    for axis in (0, 1):
        for _ in range(3):
            mode = 'wrap' if (wrap_x and axis == 1) else 'edge'
            pad = [(0, 0)] * a.ndim; pad[axis] = (r, r)
            p = np.pad(a, pad, mode=mode); c = np.cumsum(p, axis=axis)
            c = np.concatenate([np.zeros_like(np.take(c, [0], axis=axis)), c], axis=axis)
            hi = np.take(c, range(2 * r + 1, c.shape[axis]), axis=axis); lo = np.take(c, range(0, c.shape[axis] - 2 * r - 1), axis=axis)
            a = (hi - lo) / (2 * r + 1)
    return a


def bloom(px, sigmas, gains, wrap_x=False):
    out = px.copy()
    for s, g in zip(sigmas, gains): out += blur(px, s, wrap_x) * g
    # tông đỏ cho quầng (phần lõi đã trắng): đẩy kênh đỏ ở vùng mờ
    return np.clip(out, 0, 1)


def save(px, path):
    h, w, _ = px.shape
    img = bpy.data.images.new(os.path.basename(path), w, h, alpha=True)
    rgba = np.concatenate([px, np.ones((h, w, 1), np.float32)], axis=2)
    img.pixels = rgba.ravel(); img.filepath_raw = path; img.file_format = 'PNG'; img.save()
    bpy.data.images.remove(img)


core = emit('Loi trang nong', (1.0, 0.95, 0.95), 6.0)
strand = emit('Soi nang luong do', RED, 5.0)
inner = emit('Ao do', (1.0, 0.25, 0.18), 2.2)

# ---------------------------------------------------------------- 1) thân tia: dài 8 đơn vị theo x, cao 1 → 1024×128
clear_geo()
L = 8.0
tube('Loi', [(-L / 2 - .2, 0, 0), (L / 2 + .2, 0, 0)], 0.032, core)
tube('Ao', [(-L / 2 - .2, 0, -0.05), (L / 2 + .2, 0, -0.05)], 0.07, inner)
random.seed(300)
for k in range(3):
    pts = []; ph = k * 2 * math.pi / 3
    for i in range(0, 241):
        t = i / 240.0; x = -L / 2 + t * L
        a = ph + t * 3 * 2 * math.pi                           # 3 vòng / khung → lặp liền mạch
        rr = 0.16 + 0.03 * math.sin(t * 9 * 2 * math.pi + k)
        pts.append((x, rr * math.cos(a), rr * math.sin(a) * 0.6))
    tube('Soi%d' % k, pts, 0.014, strand)
beam = render(TMP + '/beam_raw.png', 1024, 128, L)
# quầng: hẹp đỏ rực + rộng đỏ thẫm; lặp theo x cho liền mạch
g1 = blur(beam, 3, True); g2 = blur(beam, 9, True); g3 = blur(beam, 22, True)
tint = np.array(RED, np.float32)
beam_out = beam + g1 * 0.9 + (g2.mean(axis=2, keepdims=True) * tint) * 1.6 + (g3.mean(axis=2, keepdims=True) * tint) * 1.4
# nhịp sáng mờ chạy dọc tia (xung năng lượng), lặp 2 lần theo x
xs = np.linspace(0, 1, 1024, endpoint=False)
beam_out *= (0.85 + 0.15 * np.sin(xs * 2 * 2 * math.pi))[None, :, None]
save(np.clip(beam_out, 0, 1), OUT + '/PV_LazerTia.png')
print('XONG thân tia', flush=True)


def flare(name, rays, ring, seed):
    clear_geo(); random.seed(seed)
    # cầu lõi
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.16, location=(0, 0, 0)); s = bpy.context.object; s.data.materials.append(core)
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.3, location=(0, 0, -0.4)); s2 = bpy.context.object; s2.data.materials.append(inner)
    # vệt sao chữ thập (ngang dài, dọc ngắn)
    tube('NgangSao', [(-1.9, 0, 0), (1.9, 0, 0)], 0.022, strand)
    tube('DocSao', [(0, -1.1, 0), (0, 1.1, 0)], 0.018, strand)
    for i in range(rays):
        a = random.uniform(0, 2 * math.pi); ln = random.uniform(0.55, 1.35)
        tube('Tia%d' % i, [(0.12 * math.cos(a), 0.12 * math.sin(a), 0), (ln * math.cos(a), ln * math.sin(a), 0)], random.uniform(0.006, 0.013), strand)
    if ring:
        pts = [(0.62 * math.cos(t), 0.62 * math.sin(t), 0) for t in np.linspace(0, 2 * math.pi, 97)]
        tube('Vong', pts, 0.012, inner)
    px = render(TMP + '/%s_raw.png' % name, 256, 256, 4.2)
    out = px + blur(px, 2) * 0.8 + blur(px, 6).mean(axis=2, keepdims=True) * tint * 1.6 + blur(px, 16).mean(axis=2, keepdims=True) * tint * 1.8
    # giảm dần ra mép (không lộ mép vuông khi billboard)
    yy, xx = np.mgrid[0:256, 0:256]; r = np.sqrt((xx - 127.5) ** 2 + (yy - 127.5) ** 2) / 128.0
    out *= np.clip(1.0 - (r - 0.75) / 0.25, 0, 1)[..., None]
    save(np.clip(out, 0, 1), OUT + '/%s.png' % name)
    print('XONG', name, flush=True)


flare('PV_LazerNong', 6, False, 11)
flare('PV_LazerTrung', 10, True, 23)


# ---------------------------------------------------------------- 21.300b: thêm ảnh cho tia đẹp hơn (người dùng: «còn đơn giản, xấu»)
def tube_r(name, pts, radii, mat):
    """Ống cong có bán kính thay đổi theo từng điểm (thon dần)."""
    cu = bpy.data.curves.new(name, 'CURVE'); cu.dimensions = '3D'; cu.bevel_depth = 1.0; cu.bevel_resolution = 3
    sp = cu.splines.new('POLY'); sp.points.add(len(pts) - 1)
    for i, (p, r) in enumerate(zip(pts, radii)): sp.points[i].co = (*p, 1); sp.points[i].radius = r
    ob = bpy.data.objects.new(name, cu); ob.data.materials.append(mat); sc.collection.objects.link(ob); return ob


# (a) NGỌN TIA (512×128, x: −4 → 4): nửa trái giống thân tia (nối liền), từ x = 0,6 thon dần tới mũi nhọn x = 3,3, 3 sợi năng lượng
#     xoắn thu về mũi; sau mũi là chùm vệt năng lượng tỏa ra rồi tan. Unity đặt ở 2 m cuối tia khi bắn trượt.
clear_geo(); random.seed(77)
N = 160; xs_ = [-4.2 + 7.5 * i / N for i in range(N + 1)]
taper = [1.0 if x < 0.6 else max(0.0, 1 - (x - 0.6) / 2.7) ** 1.4 for x in xs_]
tube_r('LoiNgon', [(x, 0, 0) for x in xs_], [0.032 * t for t in taper], core)
tube_r('AoNgon', [(x, 0, -0.05) for x in xs_], [0.07 * t for t in taper], inner)
for k in range(3):
    pts = []; rads = []; ph = k * 2 * math.pi / 3
    for x, t in zip(xs_, taper):
        a = ph + (x + 4.2) / 8.0 * 3 * 2 * math.pi
        rr = 0.16 * (t ** 0.7)
        pts.append((x, rr * math.cos(a), rr * math.sin(a) * 0.6)); rads.append(0.014 * max(t, 0.25))
    tube_r('SoiNgon%d' % k, pts, rads, strand)
for i in range(9):                                            # vệt năng lượng tỏa ra sau mũi
    a = random.uniform(-0.5, 0.5); ln = random.uniform(0.35, 0.8)
    tube('Toa%d' % i, [(3.25, 0, 0), (3.25 + ln * math.cos(a), ln * math.sin(a) * 0.55, 0)], random.uniform(0.005, 0.01), strand)
bpy.ops.mesh.primitive_uv_sphere_add(radius=0.07, location=(3.3, 0, 0)); bpy.context.object.data.materials.append(core)
px = render(TMP + '/ngon_raw.png', 1024, 128, 8.0)
out = px + blur(px, 3) * 0.9 + blur(px, 9).mean(axis=2, keepdims=True) * tint * 1.6 + blur(px, 22).mean(axis=2, keepdims=True) * tint * 1.4
fade = np.clip((1024 - np.arange(1024)) / 110.0, 0, 1) ** 1.5                 # tan hết ở mép phải
out *= fade[None, :, None]
save(np.clip(out, 0, 1), OUT + '/PV_LazerNgon.png'); print('XONG ngọn tia', flush=True)

# (b) VÒNG SÓNG XUNG KÍCH (256²): vòng sáng mảnh + vòng mờ trong + 8 vệt ngắn hướng tâm, quầng đỏ — nở ra ở chỗ trúng.
clear_geo(); random.seed(5)
tube('VongNgoai', [(1.45 * math.cos(t), 1.45 * math.sin(t), 0) for t in np.linspace(0, 2 * math.pi, 161)], 0.035, core)
tube('VongTrong', [(1.15 * math.cos(t), 1.15 * math.sin(t), 0) for t in np.linspace(0, 2 * math.pi, 161)], 0.018, strand)
for i in range(8):
    a = i * math.pi / 4 + random.uniform(-.15, .15)
    tube('Vach%d' % i, [(1.5 * math.cos(a), 1.5 * math.sin(a), 0), (1.78 * math.cos(a), 1.78 * math.sin(a), 0)], 0.012, strand)
px = render(TMP + '/vong_raw.png', 256, 256, 4.2)
out = px + blur(px, 2) * 0.8 + blur(px, 6).mean(axis=2, keepdims=True) * tint * 1.5 + blur(px, 14).mean(axis=2, keepdims=True) * tint * 1.2
yy, xx = np.mgrid[0:256, 0:256]; r = np.sqrt((xx - 127.5) ** 2 + (yy - 127.5) ** 2) / 128.0
out *= np.clip(1.0 - (r - 0.85) / 0.15, 0, 1)[..., None]
save(np.clip(out, 0, 1), OUT + '/PV_LazerVong.png'); print('XONG vòng sóng', flush=True)

# (c) QUẦNG RỘNG (256×64, lặp theo x): dải sáng mềm Gauss theo bề ngang + gợn rất nhẹ theo chiều dài — lớp ngoài cùng của tia.
yy = np.linspace(-1, 1, 64)[:, None]; xx = np.linspace(0, 1, 256, endpoint=False)[None, :]
prof = 0.65 * np.exp(-(yy / 0.42) ** 2) + 0.35 * np.exp(-(yy / 0.14) ** 2)
ripple = 0.9 + 0.1 * np.sin(xx * 2 * math.pi * 3 + yy * 2)
g = np.clip(prof * ripple, 0, 1)
save(np.repeat(g[..., None], 3, axis=2).astype(np.float32), OUT + '/PV_LazerQuang.png'); print('XONG quầng', flush=True)

# (d) CHẤM SÁNG (64²) mềm — xung năng lượng chạy dọc tia, đốm bay, vết cháy.
yy, xx = np.mgrid[0:64, 0:64]; r = np.sqrt((xx - 31.5) ** 2 + (yy - 31.5) ** 2) / 32.0
dot = np.clip(np.exp(-(r / 0.38) ** 2) * 1.1 + np.exp(-(r / 0.12) ** 2) * 0.6, 0, 1) * np.clip(1 - (r - .85) / .15, 0, 1)
save(np.repeat(dot[..., None], 3, axis=2).astype(np.float32), OUT + '/PV_LazerHat.png'); print('XONG chấm sáng', flush=True)

# (e) TIA LỬA NÓNG CHẢY (128×32): vệt dài đầu trắng (phải) đuôi cam đỏ mờ dần (trái) — hạt kéo dài theo vận tốc.
yy = np.linspace(-1, 1, 32)[:, None]; xx = np.linspace(0, 1, 128)[None, :]
body = np.exp(-(yy / (0.18 + 0.22 * xx)) ** 2) * (xx ** 1.6)
head = np.exp(-(yy / 0.3) ** 2) * np.exp(-((xx - 0.9) / 0.08) ** 2)
sp_ = np.zeros((32, 128, 3), np.float32)
hot = np.clip(body + head, 0, 1)
sp_[..., 0] = np.clip(hot * 1.2, 0, 1); sp_[..., 1] = np.clip(hot * (0.35 + 0.65 * xx ** 2), 0, 1); sp_[..., 2] = np.clip(hot * 0.55 * xx ** 3, 0, 1)
save(sp_, OUT + '/PV_LazerTiaLua.png'); print('XONG tia lửa', flush=True)

bpy.ops.wm.save_as_mainfile(filepath=BLEND)
print('XONG TẤT CẢ', flush=True)
