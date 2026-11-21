"""21.295 — khói trắng phản lực khi xe nhảy (4 cột khói dưới gầm), dựng lại thật hơn.
Chạy Blender nền (không qua MCP vì render lâu):
  blender -b --factory-startup --python-expr "exec(open(r'.../ow_jump_smoke.py',encoding='utf-8').read())"
Kết quả: ArtSource/Concepts/JumpSmoke-21295/frame_XX.png (16 khung 256²) → ghép bằng numpy thành
Assets/PhoViet/Resources/Boost/PV_JumpSmoke4x4.png (1024², RGBA, 4×4 khung): một cụm khói thể tích (Cycles volume) được chiếu
nắng từ trên-trước nên có khối sáng/tối, theo thời gian nở ra, rách dần thành sợi mỏng rồi tan — Unity chạy khung theo tuổi hạt.
Không mở / ghi đè file .blend có sẵn; lưu cảnh vào PhoViet_OW_KhoiNhay_21295.blend (mới).
"""
import bpy, math, os
import numpy as np

ROOT = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
FR = ROOT + '/ArtSource/Concepts/JumpSmoke-21295'
OUT = ROOT + '/Assets/PhoViet/Resources/Boost/PV_JumpSmoke4x4.png'
BLEND = ROOT + '/ArtSource/Blender/OpenWorld/PhoViet_OW_KhoiNhay_21295.blend'
N, RES = 16, 256
os.makedirs(FR, exist_ok=True)
try:
    MODE
except NameError:
    MODE = 'all'          # 'all' = render 16 khung + ghép; 'sheet' = chỉ ghép lại từ khung đã render (sửa hậu kỳ)

bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene
sc.render.engine = 'CYCLES'
try:
    sc.cycles.device = 'CPU'
except Exception:
    pass
sc.cycles.samples = 96
sc.cycles.use_denoising = True
sc.cycles.volume_bounces = 2
sc.cycles.volume_step_rate = 0.5
sc.render.resolution_x = sc.render.resolution_y = RES
sc.render.resolution_percentage = 100
sc.render.film_transparent = True
sc.render.image_settings.file_format = 'PNG'
sc.render.image_settings.color_mode = 'RGBA'
sc.view_settings.view_transform = 'Standard'

world = bpy.data.worlds.new('Troi sang dịu'); world.use_nodes = True
bg = next(n for n in world.node_tree.nodes if n.type == 'BACKGROUND')
bg.inputs[0].default_value = (0.78, 0.86, 1.0, 1); bg.inputs[1].default_value = 0.55
sc.world = world

# nắng chéo từ trên-trước → đỉnh cụm khói sáng, bụng hơi xám xanh (như khói thật dưới trời)
sun = bpy.data.objects.new('Nang', bpy.data.lights.new('Nang', 'SUN'))
sun.data.energy = 4.2; sun.data.angle = math.radians(8); sun.data.color = (1.0, 0.97, 0.92)
sun.rotation_euler = (math.radians(35), math.radians(-20), math.radians(30))
sc.collection.objects.link(sun)

cam = bpy.data.objects.new('Cam', bpy.data.cameras.new('Cam'))
cam.data.type = 'ORTHO'; cam.data.ortho_scale = 2.3
cam.location = (0, -6, 0); cam.rotation_euler = (math.radians(90), 0, 0)
sc.collection.objects.link(cam); sc.camera = cam

mat = bpy.data.materials.new('Khoi the tich'); mat.use_nodes = True
nt = mat.node_tree; nd = nt.nodes; ln = nt.links; nd.clear()
out = nd.new('ShaderNodeOutputMaterial')
vol = nd.new('ShaderNodeVolumePrincipled')
vol.inputs['Color'].default_value = (0.96, 0.97, 1.0, 1)
vol.inputs['Anisotropy'].default_value = 0.25
tc = nd.new('ShaderNodeTexCoord')
# vị trí gốc tâm 0 (Object), lệch nhẹ bằng noise lớn để mép cụm nhấp nhô
warp = nd.new('ShaderNodeTexNoise'); warp.noise_dimensions = '4D'
warp.inputs['Scale'].default_value = 1.6; warp.inputs['Detail'].default_value = 2
mixw = nd.new('ShaderNodeVectorMath'); mixw.operation = 'SCALE'
sub = nd.new('ShaderNodeVectorMath'); sub.operation = 'SUBTRACT'; sub.inputs[1].default_value = (0.5, 0.5, 0.5)
addw = nd.new('ShaderNodeVectorMath'); addw.operation = 'ADD'
ln.new(tc.outputs['Object'], warp.inputs['Vector'])
ln.new(warp.outputs['Color'], sub.inputs[0])
ln.new(sub.outputs['Vector'], mixw.inputs[0])
ln.new(tc.outputs['Object'], addw.inputs[0]); ln.new(mixw.outputs['Vector'], addw.inputs[1])
length = nd.new('ShaderNodeVectorMath'); length.operation = 'LENGTH'; ln.new(addw.outputs['Vector'], length.inputs[0])
fall = nd.new('ShaderNodeMapRange'); fall.inputs['To Min'].default_value = 1; fall.inputs['To Max'].default_value = 0
ln.new(length.outputs['Value'], fall.inputs['Value'])
# chi tiết cuộn khói (2 tầng noise)
n1 = nd.new('ShaderNodeTexNoise'); n1.noise_dimensions = '4D'
n1.inputs['Scale'].default_value = 3.2; n1.inputs['Detail'].default_value = 6; n1.inputs['Roughness'].default_value = 0.62
ln.new(addw.outputs['Vector'], n1.inputs['Vector'])
thr = nd.new('ShaderNodeMapRange'); thr.clamp = True; thr.inputs['To Min'].default_value = 0; thr.inputs['To Max'].default_value = 1
ln.new(n1.outputs['Fac'], thr.inputs['Value'])
mul = nd.new('ShaderNodeMath'); mul.operation = 'MULTIPLY'; ln.new(fall.outputs['Result'], mul.inputs[0]); ln.new(thr.outputs['Result'], mul.inputs[1])
gain = nd.new('ShaderNodeMath'); gain.operation = 'MULTIPLY'; ln.new(mul.outputs[0], gain.inputs[0])
ln.new(gain.outputs[0], vol.inputs['Density']); ln.new(vol.outputs['Volume'], out.inputs['Volume'])

bpy.ops.mesh.primitive_cube_add(size=2.2)
dom = bpy.context.object; dom.name = 'Mien khoi'; dom.data.materials.append(mat)

frames = [FR + '/frame_%02d.png' % k for k in range(N)] if MODE == 'sheet' else []
for k in (range(N) if MODE != 'sheet' else []):
    t = k / (N - 1)
    # nở to dần (bán kính), rách dần (ngưỡng noise tăng), loãng dần (mật độ giảm), cuộn xoáy (W đổi)
    radius = 0.55 + 0.42 * t
    fall.inputs['From Min'].default_value = radius * 0.35
    fall.inputs['From Max'].default_value = radius
    thr.inputs['From Min'].default_value = 0.30 + 0.20 * t
    thr.inputs['From Max'].default_value = 0.62 + 0.12 * t
    gain.inputs[1].default_value = 26.0 * (1.0 - 0.72 * t)
    mixw.inputs['Scale'].default_value = 0.35 + 0.35 * t
    warp.inputs['W'].default_value = 0.6 * t
    n1.inputs['W'].default_value = 1.3 * t
    n1.inputs['Scale'].default_value = 3.2 - 0.8 * t
    fp = FR + '/frame_%02d.png' % k
    sc.render.filepath = fp
    bpy.ops.render.render(write_still=True)
    frames.append(fp)
    print('khung', k, 'xong', flush=True)

sheet = np.zeros((RES * 4, RES * 4, 4), np.float32)
for k, fp in enumerate(frames):
    img = bpy.data.images.load(fp)
    px = np.array(img.pixels[:], np.float32).reshape(RES, RES, 4)      # gốc dưới-trái
    row, col = k // 4, k % 4
    # Unity: khung 0 ở hàng TRÊN cùng, trái → phải; ảnh Blender gốc dưới → hàng trên = y lớn
    y0 = (3 - row) * RES
    sheet[y0:y0 + RES, col * RES:(col + 1) * RES] = px
    bpy.data.images.remove(img)
# hậu kỳ: mép cụm khói mềm hơn (làm mờ alpha 2 lượt hộp 7×7 trong từng khung, không tràn sang khung bên) và sáng hơn chút
# (khói trắng dưới nắng) → các cụm hòa vào nhau thành cột khói thay vì từng «cục bông» tròn rời.
def box_blur(a, r):
    k = 2 * r + 1
    p = np.pad(a, r, mode='edge'); c = p.cumsum(0).cumsum(1)
    c = np.pad(c, ((1, 0), (1, 0)))
    return (c[k:, k:] - c[:-k, k:] - c[k:, :-k] + c[:-k, :-k]) / (k * k)
for k in range(N):
    y0 = (3 - k // 4) * RES; x0 = (k % 4) * RES
    t = sheet[y0:y0 + RES, x0:x0 + RES]
    a = t[..., 3].copy()
    soft = box_blur(box_blur(a, 3), 3)
    t[..., 3] = np.clip(np.minimum(a, soft) * 0.6 + soft * 0.4, 0, 1)
    t[..., :3] = np.clip(t[..., :3] * 1.1 + 0.03, 0, 1)
img = bpy.data.images.new('PV_JumpSmoke4x4', RES * 4, RES * 4, alpha=True)
img.pixels = sheet.ravel()
img.filepath_raw = OUT; img.file_format = 'PNG'; img.save()
if MODE != 'sheet':
    bpy.ops.wm.save_as_mainfile(filepath=BLEND)
print('XONG', OUT, flush=True)
