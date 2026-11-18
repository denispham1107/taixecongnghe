# Thư viện dựng bản đồ mở Phố Việt (21.239). Chạy bên trong Blender qua Blender MCP:
#   exec(compile(open(p, encoding='utf-8').read(), p, 'exec'), ns)
# Hệ toạ độ cục bộ: x = đông, y = bắc, z = lên (Blender). Đổi sang Unity: Unity x = x, Unity z = y.
import bpy, bmesh, math, random
from mathutils import Vector, Matrix

FONT_DIR = 'C:/Windows/Fonts/'

# ---------------------------------------------------------------- collections
def coll(name, parent=None):
    c = bpy.data.collections.get(name)
    if c is None:
        c = bpy.data.collections.new(name)
        (parent or bpy.context.scene.collection).children.link(c)
    return c

def clear_coll(name):
    c = bpy.data.collections.get(name)
    if not c: return
    for ch in list(c.children): clear_coll(ch.name)
    for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
    bpy.data.collections.remove(c)

def purge():
    for _ in range(3):
        try: bpy.ops.outliner.orphans_purge(do_recursive=True)
        except Exception: pass

# ---------------------------------------------------------------- materials
def _hex(h):
    h = h.lstrip('#'); c = [int(h[i:i+2], 16) / 255 for i in (0, 2, 4)]
    return tuple(((x + 0.055) / 1.055) ** 2.4 if x > 0.04045 else x / 12.92 for x in c) + (1.0,)

def _new_mat(name):
    m = bpy.data.materials.get(name)
    if m: return m, False
    m = bpy.data.materials.new(name); m.use_nodes = True
    return m, True

def _bsdf(m):
    return next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')

def mat(name, color, rough=0.6, metal=0.0, emit=None, emit_strength=0.0, alpha=1.0):
    m, new = _new_mat(name)
    if not new: return m
    b = _bsdf(m)
    b.inputs['Base Color'].default_value = _hex(color) if isinstance(color, str) else color
    b.inputs['Roughness'].default_value = rough
    b.inputs['Metallic'].default_value = metal
    if emit:
        b.inputs['Emission Color'].default_value = _hex(emit)
        b.inputs['Emission Strength'].default_value = emit_strength
    m.diffuse_color = _hex(color) if isinstance(color, str) else color
    return m

def _coord(nt, kind='Object'):
    tc = nt.nodes.new('ShaderNodeTexCoord'); return tc.outputs[kind]

def mat_noise(name, c1, c2, scale=3.0, rough=0.8, bump=0.15, detail=8.0, kind='Object', rough2=None):
    """Hai màu trộn theo nhiễu + gồ ghề: nhựa đường, bê tông, tường cũ."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, kind)
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = scale; nz.inputs['Detail'].default_value = detail
    nt.links.new(co, nz.inputs['Vector'])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color = _hex(c1); ramp.color_ramp.elements[1].color = _hex(c2)
    ramp.color_ramp.elements[0].position = 0.35; ramp.color_ramp.elements[1].position = 0.7
    nt.links.new(nz.outputs['Fac'], ramp.inputs['Fac']); nt.links.new(ramp.outputs['Color'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = rough
    if bump:
        bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = bump
        nz2 = nt.nodes.new('ShaderNodeTexNoise'); nz2.inputs['Scale'].default_value = scale * 20
        nt.links.new(co, nz2.inputs['Vector']); nt.links.new(nz2.outputs['Fac'], bp.inputs['Height'])
        nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    m.diffuse_color = _hex(c1)
    return m

def mat_weathered(name, base, stain='#6F6B60', scale=0.35):
    """Tường sơn cũ có vệt ố chảy dọc (giống mặt nhà ảnh 3)."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    # Kéo dài nhiễu theo trục đứng để thành vệt ố chảy xuống.
    mp = nt.nodes.new('ShaderNodeMapping'); mp.inputs['Scale'].default_value = (scale * 3, scale * 3, scale * 0.6)
    nt.links.new(co, mp.inputs['Vector'])
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = 2.5; nz.inputs['Detail'].default_value = 10; nz.inputs['Roughness'].default_value = 0.65
    nt.links.new(mp.outputs['Vector'], nz.inputs['Vector'])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].position = 0.40; ramp.color_ramp.elements[0].color = _hex(base)
    ramp.color_ramp.elements[1].position = 0.72; ramp.color_ramp.elements[1].color = _hex(stain)
    nt.links.new(nz.outputs['Fac'], ramp.inputs['Fac']); nt.links.new(ramp.outputs['Color'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = 0.9
    bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = 0.08
    nz2 = nt.nodes.new('ShaderNodeTexNoise'); nz2.inputs['Scale'].default_value = 60
    nt.links.new(co, nz2.inputs['Vector']); nt.links.new(nz2.outputs['Fac'], bp.inputs['Height']); nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    m.diffuse_color = _hex(base)
    return m

def mat_tiles(name, c1, c2, mortar='#5E5E5A', size=0.6, rough=0.85):
    """Gạch lát vỉa hè vuông."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    br = nt.nodes.new('ShaderNodeTexBrick')
    br.inputs['Scale'].default_value = 1.0 / size; br.inputs['Mortar Size'].default_value = 0.012 / size * 0.6
    br.inputs['Brick Width'].default_value = 1.0; br.inputs['Row Height'].default_value = 1.0
    br.offset = 0.0
    br.inputs['Color1'].default_value = _hex(c1); br.inputs['Color2'].default_value = _hex(c2); br.inputs['Mortar'].default_value = _hex(mortar)
    nt.links.new(co, br.inputs['Vector'])
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = 4
    nt.links.new(co, nz.inputs['Vector'])
    mix = nt.nodes.new('ShaderNodeMix'); mix.data_type = 'RGBA'; mix.blend_type = 'MULTIPLY'
    mix.inputs['Factor'].default_value = 0.35
    nt.links.new(br.outputs['Color'], mix.inputs['A']); nt.links.new(nz.outputs['Color'], mix.inputs['B'])
    nt.links.new(mix.outputs['Result'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = rough
    bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = 0.35; bp.invert = True
    nt.links.new(br.outputs['Fac'], bp.inputs['Height']); nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    m.diffuse_color = _hex(c1)
    return m

def mat_shopglass(name, warm='#FFB867', strength=3.5):
    """Mặt kính tiệm tầng trệt sáng đèn vàng ấm bên trong."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = 1.6
    nt.links.new(co, nz.inputs['Vector'])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color = _hex('#6B3F1E'); ramp.color_ramp.elements[1].color = _hex(warm)
    nt.links.new(nz.outputs['Fac'], ramp.inputs['Fac'])
    nt.links.new(ramp.outputs['Color'], b.inputs['Emission Color'])
    b.inputs['Emission Strength'].default_value = strength
    b.inputs['Base Color'].default_value = _hex('#1E1A16'); b.inputs['Roughness'].default_value = 0.08
    m.diffuse_color = _hex(warm)
    return m

def mat_roof_tiles(name, c1='#A8432B', c2='#7E2E1D', rows=4.0):
    """Ngói đỏ: sọc hàng ngói theo trục Z của object + nhiễu loang màu."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    wv = nt.nodes.new('ShaderNodeTexWave'); wv.wave_type = 'BANDS'; wv.bands_direction = 'Z'
    wv.inputs['Scale'].default_value = rows / 6.28 * 6; wv.wave_profile = 'SAW'
    nt.links.new(co, wv.inputs['Vector'])
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = 1.5
    nt.links.new(co, nz.inputs['Vector'])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color = _hex(c2); ramp.color_ramp.elements[1].color = _hex(c1)
    nt.links.new(nz.outputs['Fac'], ramp.inputs['Fac'])
    mix = nt.nodes.new('ShaderNodeMix'); mix.data_type = 'RGBA'; mix.blend_type = 'MULTIPLY'; mix.inputs['Factor'].default_value = 0.35
    nt.links.new(ramp.outputs['Color'], mix.inputs['A']); nt.links.new(wv.outputs['Color'], mix.inputs['B'])
    nt.links.new(mix.outputs['Result'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = 0.75
    bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = 0.5
    nt.links.new(wv.outputs['Fac'], bp.inputs['Height']); nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    m.diffuse_color = _hex(c1)
    return m

def mat_barrel_tiles(name, c1='#B4492C', c2='#7A2B1A', row=0.13, col=0.22):
    """Ngói ống đỏ: hàng ngói theo cao độ Z (mái dốc → hàng nằm ngang) × rãnh ống theo X và Y, loang màu."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    sep = nt.nodes.new('ShaderNodeSeparateXYZ'); nt.links.new(co, sep.inputs['Vector'])
    def wave(src, period):
        mth = nt.nodes.new('ShaderNodeMath'); mth.operation = 'MULTIPLY'; mth.inputs[1].default_value = 6.2832 / period
        nt.links.new(src, mth.inputs[0])
        sn = nt.nodes.new('ShaderNodeMath'); sn.operation = 'SINE'; nt.links.new(mth.outputs[0], sn.inputs[0])
        ab = nt.nodes.new('ShaderNodeMath'); ab.operation = 'ABSOLUTE'; nt.links.new(sn.outputs[0], ab.inputs[0])
        return ab.outputs[0]
    rz = wave(sep.outputs['Z'], row * 2)
    cx = wave(sep.outputs['X'], col * 2); cy = wave(sep.outputs['Y'], col * 2)
    mx = nt.nodes.new('ShaderNodeMath'); mx.operation = 'MAXIMUM'; nt.links.new(cx, mx.inputs[0]); nt.links.new(cy, mx.inputs[1])
    pw = nt.nodes.new('ShaderNodeMath'); pw.operation = 'POWER'; pw.inputs[1].default_value = 0.35; nt.links.new(rz, pw.inputs[0])
    h = nt.nodes.new('ShaderNodeMath'); h.operation = 'MULTIPLY'; nt.links.new(pw.outputs[0], h.inputs[0]); nt.links.new(mx.outputs[0], h.inputs[1])
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = 0.8; nz.inputs['Detail'].default_value = 6
    nt.links.new(co, nz.inputs['Vector'])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color = _hex(c2); ramp.color_ramp.elements[1].color = _hex(c1)
    ramp.color_ramp.elements[0].position = 0.3; ramp.color_ramp.elements[1].position = 0.75
    nt.links.new(nz.outputs['Fac'], ramp.inputs['Fac'])
    shade = nt.nodes.new('ShaderNodeMapRange'); shade.inputs['To Min'].default_value = 0.55; shade.inputs['To Max'].default_value = 1.0
    nt.links.new(h.outputs[0], shade.inputs['Value'])
    mix = nt.nodes.new('ShaderNodeMix'); mix.data_type = 'RGBA'; mix.blend_type = 'MULTIPLY'; mix.inputs['Factor'].default_value = 1.0
    nt.links.new(ramp.outputs['Color'], mix.inputs['A']); nt.links.new(shade.outputs['Result'], mix.inputs['B'])
    nt.links.new(mix.outputs['Result'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = 0.7
    bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = 0.8; bp.inputs['Distance'].default_value = 0.05
    nt.links.new(h.outputs[0], bp.inputs['Height']); nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    m.diffuse_color = _hex(c1)
    return m

def mat_glass_tower(name, base='#5D7486', stripes=60.0):
    """Kính cao ốc có sọc khung đứng."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    wv = nt.nodes.new('ShaderNodeTexWave'); wv.wave_type = 'BANDS'; wv.bands_direction = 'Z'
    wv.inputs['Scale'].default_value = stripes / 10
    nt.links.new(co, wv.inputs['Vector'])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color = _hex(base); ramp.color_ramp.elements[1].color = _hex('#C9D4DC')
    ramp.color_ramp.elements[0].position = 0.85; ramp.color_ramp.elements[1].position = 0.95
    nt.links.new(wv.outputs['Fac'], ramp.inputs['Fac']); nt.links.new(ramp.outputs['Color'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = 0.12; b.inputs['Metallic'].default_value = 0.55
    m.diffuse_color = _hex(base)
    return m

def mat_windows_grid(name, wall='#D9D4C8', glass='#3E5466', fx=1.0, fz=1.0):
    """Lưới cửa sổ cho cao ốc xa (rẻ): gạch -> ô kính."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    sep = nt.nodes.new('ShaderNodeSeparateXYZ'); nt.links.new(co, sep.inputs['Vector'])
    comb = nt.nodes.new('ShaderNodeCombineXYZ')
    add = nt.nodes.new('ShaderNodeMath'); add.operation = 'ADD'
    nt.links.new(sep.outputs['X'], add.inputs[0]); nt.links.new(sep.outputs['Y'], add.inputs[1])
    nt.links.new(add.outputs['Value'], comb.inputs['X']); nt.links.new(sep.outputs['Z'], comb.inputs['Y'])
    br = nt.nodes.new('ShaderNodeTexBrick'); br.offset = 0.0
    br.inputs['Scale'].default_value = 1.0
    br.inputs['Brick Width'].default_value = 1.6 * fx; br.inputs['Row Height'].default_value = 3.3 * fz
    br.inputs['Mortar Size'].default_value = 0.35
    br.inputs['Color1'].default_value = _hex(glass); br.inputs['Color2'].default_value = _hex(glass); br.inputs['Mortar'].default_value = _hex(wall)
    nt.links.new(comb.outputs['Vector'], br.inputs['Vector'])
    nt.links.new(br.outputs['Color'], b.inputs['Base Color'])
    rr = nt.nodes.new('ShaderNodeMapRange'); rr.inputs['To Min'].default_value = 0.85; rr.inputs['To Max'].default_value = 0.15
    nt.links.new(br.outputs['Fac'], rr.inputs['Value']); nt.links.new(rr.outputs['Result'], b.inputs['Roughness'])
    m.diffuse_color = _hex(wall)
    return m

def mat_water(name):
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    # Màu lấy từ PV_RiverWater.mat (_DeepColor/_ShallowColor). Trong Unity dùng lại RiverWater.shader có sẵn.
    b.inputs['Base Color'].default_value = (0.03, 0.11, 0.16, 1); b.inputs['Roughness'].default_value = 0.05
    b.inputs['Specular IOR Level'].default_value = 0.6
    mp = nt.nodes.new('ShaderNodeMapping'); mp.inputs['Scale'].default_value = (1.0, 0.35, 1.0)
    nt.links.new(co, mp.inputs['Vector'])
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = 0.9; nz.inputs['Detail'].default_value = 6
    nt.links.new(mp.outputs['Vector'], nz.inputs['Vector'])
    bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = 0.12
    nt.links.new(nz.outputs['Fac'], bp.inputs['Height']); nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    m.diffuse_color = _hex('#2F5560')
    return m

def mat_leaves(name, c1='#2F5A22', c2='#6E8F2E', translucent=True):
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    vor = nt.nodes.new('ShaderNodeTexVoronoi'); vor.inputs['Scale'].default_value = 9
    nt.links.new(co, vor.inputs['Vector'])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color = _hex(c1); ramp.color_ramp.elements[1].color = _hex(c2)
    nt.links.new(vor.outputs['Distance'], ramp.inputs['Fac']); nt.links.new(ramp.outputs['Color'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = 0.65
    try:
        b.inputs['Subsurface Weight'].default_value = 0.15 if translucent else 0
    except KeyError: pass
    bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = 0.9
    nt.links.new(vor.outputs['Distance'], bp.inputs['Height']); nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    m.diffuse_color = _hex(c1)
    return m

def mat_bougainvillea(name):
    """Hoa giấy hồng xen lá xanh."""
    m, new = _new_mat(name)
    if not new: return m
    nt = m.node_tree; b = _bsdf(m)
    co = _coord(nt, 'Object')
    vor = nt.nodes.new('ShaderNodeTexVoronoi'); vor.inputs['Scale'].default_value = 14
    nt.links.new(co, vor.inputs['Vector'])
    nz = nt.nodes.new('ShaderNodeTexNoise'); nz.inputs['Scale'].default_value = 3
    nt.links.new(co, nz.inputs['Vector'])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    el = ramp.color_ramp.elements
    el[0].position = 0.40; el[0].color = _hex('#2E5B26')
    el[1].position = 0.52; el[1].color = _hex('#D0247E')
    e3 = el.new(0.8); e3.color = _hex('#E852A5')
    nt.links.new(nz.outputs['Fac'], ramp.inputs['Fac']); nt.links.new(ramp.outputs['Color'], b.inputs['Base Color'])
    b.inputs['Roughness'].default_value = 0.6
    bp = nt.nodes.new('ShaderNodeBump'); bp.inputs['Strength'].default_value = 1.0
    nt.links.new(vor.outputs['Distance'], bp.inputs['Height']); nt.links.new(bp.outputs['Normal'], b.inputs['Normal'])
    m.diffuse_color = _hex('#D0247E')
    return m

# ---------------------------------------------------------------- mesh builder
class MB:
    """Gom nhiều khối vào MỘT mesh (nhiều slot vật liệu) để nhẹ khi xuất sang Unity."""
    def __init__(self, name):
        self.name = name; self.bm = bmesh.new(); self.mats = []
    def _mi(self, m):
        if m not in self.mats: self.mats.append(m)
        return self.mats.index(m)
    def box(self, c, s, m, rz=0.0, rx=0.0, ry=0.0):
        mi = self._mi(m)
        r = (Matrix.Rotation(rz, 4, 'Z') @ Matrix.Rotation(ry, 4, 'Y') @ Matrix.Rotation(rx, 4, 'X'))
        hx, hy, hz = s[0] / 2, s[1] / 2, s[2] / 2
        vs = [self.bm.verts.new(Vector(c) + (r @ Vector((x * hx, y * hy, z * hz, 0))).to_3d())
              for x in (-1, 1) for y in (-1, 1) for z in (-1, 1)]
        for f in ((0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)):
            face = self.bm.faces.new([vs[i] for i in f]); face.material_index = mi; face.smooth = False
    def boxab(self, a, b, m):
        """Hộp theo hai góc a, b."""
        self.box(((a[0] + b[0]) / 2, (a[1] + b[1]) / 2, (a[2] + b[2]) / 2), (abs(b[0] - a[0]), abs(b[1] - a[1]), abs(b[2] - a[2])), m)
    def cyl(self, c, r, h, m, seg=10, r2=None, axis='Z'):
        mi = self._mi(m); r2 = r if r2 is None else r2
        rot = Matrix.Identity(3)
        if axis == 'X': rot = Matrix.Rotation(math.pi / 2, 3, 'Y')
        if axis == 'Y': rot = Matrix.Rotation(-math.pi / 2, 3, 'X')
        bot, top = [], []
        for i in range(seg):
            a = 2 * math.pi * i / seg
            bot.append(self.bm.verts.new(Vector(c) + rot @ Vector((math.cos(a) * r, math.sin(a) * r, 0))))
            top.append(self.bm.verts.new(Vector(c) + rot @ Vector((math.cos(a) * r2, math.sin(a) * r2, h))))
        for i in range(seg):
            j = (i + 1) % seg
            f = self.bm.faces.new((bot[i], bot[j], top[j], top[i])); f.material_index = mi; f.smooth = True
        f = self.bm.faces.new(list(reversed(bot))); f.material_index = mi
        f = self.bm.faces.new(top); f.material_index = mi
    def rod(self, a, b, r, m, seg=8):
        a, b = Vector(a), Vector(b); d = b - a
        mi = self._mi(m)
        q = Vector((0, 0, 1)).rotation_difference(d.normalized())
        bot, top = [], []
        for i in range(seg):
            t = 2 * math.pi * i / seg; p = q @ Vector((math.cos(t) * r, math.sin(t) * r, 0))
            bot.append(self.bm.verts.new(a + p)); top.append(self.bm.verts.new(b + p))
        for i in range(seg):
            j = (i + 1) % seg
            f = self.bm.faces.new((bot[i], bot[j], top[j], top[i])); f.material_index = mi; f.smooth = True
    def star(self, c, radius, m, normal_y=-1.0, thick=0.004):
        """Sao vàng 5 cánh nằm trong mặt phẳng XZ, mũi trên hướng lên, mặt quay về phía normal_y (±y)."""
        mi = self._mi(m); c = Vector(c)
        ring = []
        for i in range(10):
            a = math.radians(90 + i * 36); r = radius if i % 2 == 0 else radius * 0.382
            ring.append(Vector((math.cos(a) * r, 0, math.sin(a) * r)))
        y = normal_y * thick
        ctr = self.bm.verts.new(c + Vector((0, y, 0)))
        vs = [self.bm.verts.new(c + p + Vector((0, y, 0))) for p in ring]
        for i in range(10):
            a, b = vs[i], vs[(i + 1) % 10]
            # (ctr, a, b) có pháp tuyến −y vì vòng điểm đi ngược chiều kim đồng hồ trong mặt XZ.
            f = self.bm.faces.new((ctr, a, b) if normal_y < 0 else (ctr, b, a)); f.material_index = mi
    def sphere(self, c, r, m, seg=10, rings=6, scale=(1.0, 1.0, 1.0), smooth=True):
        """Cầu UV (có thể kéo dãn theo trục) — trái cây, bông hoa, đầu bông."""
        mi = self._mi(m); c = Vector(c); sx, sy, sz = scale
        top = self.bm.verts.new(c + Vector((0, 0, r * sz))); bot = self.bm.verts.new(c - Vector((0, 0, r * sz)))
        rows = []
        for j in range(1, rings):
            phi = math.pi * j / rings; z = math.cos(phi) * r * sz; rr = math.sin(phi) * r
            rows.append([self.bm.verts.new(c + Vector((math.cos(2 * math.pi * i / seg) * rr * sx, math.sin(2 * math.pi * i / seg) * rr * sy, z))) for i in range(seg)])
        for i in range(seg):
            j = (i + 1) % seg
            f = self.bm.faces.new((top, rows[0][i], rows[0][j])); f.material_index = mi; f.smooth = smooth
            f = self.bm.faces.new((bot, rows[-1][j], rows[-1][i])); f.material_index = mi; f.smooth = smooth
            for k in range(len(rows) - 1):
                f = self.bm.faces.new((rows[k][i], rows[k + 1][i], rows[k + 1][j], rows[k][j])); f.material_index = mi; f.smooth = smooth
    def sweep_rect(self, pts, hx, hn, m, side=Vector((1, 0, 0))):
        """Thanh tiết diện chữ nhật chạy theo đường cong pts (vòm cầu). hx: nửa rộng theo trục side, hn: nửa cao theo pháp tuyến."""
        mi = self._mi(m); pts = [Vector(p) for p in pts]; rings = []
        for i, p in enumerate(pts):
            t = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
            n = side.cross(t).normalized()
            rings.append([self.bm.verts.new(p + sx * hx * side + sn * hn * n) for sx, sn in ((-1, -1), (1, -1), (1, 1), (-1, 1))])
        for a, b in zip(rings, rings[1:]):
            for k in range(4):
                j = (k + 1) % 4
                f = self.bm.faces.new((a[k], a[j], b[j], b[k])); f.material_index = mi; f.smooth = False
        for ring in (rings[0], list(reversed(rings[-1]))):
            f = self.bm.faces.new(ring); f.material_index = mi
    def profile(self, pts, axis, w0, w1, m):
        """Đùn một đa giác phẳng (u, v) theo chiều sâu w0→w1.
        axis 'x': điểm 3D = (w, u, v) — mặt tường vuông góc trục x; axis 'y': (u, w, v)."""
        mi = self._mi(m)
        P = (lambda u, v, w: Vector((w, u, v))) if axis == 'x' else (lambda u, v, w: Vector((u, w, v)))
        a = [self.bm.verts.new(P(u, v, w0)) for u, v in pts]
        b = [self.bm.verts.new(P(u, v, w1)) for u, v in pts]
        for ring in (a, list(reversed(b))):
            f = self.bm.faces.new(ring); f.material_index = mi
        n = len(pts)
        for i in range(n):
            j = (i + 1) % n
            f = self.bm.faces.new((a[i], a[j], b[j], b[i])); f.material_index = mi
    def quad(self, pts, m):
        f = self.bm.faces.new([self.bm.verts.new(Vector(p)) for p in pts]); f.material_index = self._mi(m)
    def finish(self, col, loc=(0, 0, 0)):
        me = bpy.data.meshes.new(self.name)
        self.bm.to_mesh(me); self.bm.free()
        for m in self.mats: me.materials.append(m)
        ob = bpy.data.objects.new(self.name, me); ob.location = loc
        col.objects.link(ob)
        return ob

# ---------------------------------------------------------------- text
_fonts = {}
def font(file):
    if file not in _fonts:
        _fonts[file] = bpy.data.fonts.load(FONT_DIR + file, check_existing=True)
    return _fonts[file]

def text(col, body, loc, size, m, rot=(math.pi / 2, 0, 0), fontfile='arialbd.ttf', align='CENTER', extrude=0.01, name=None):
    cu = bpy.data.curves.new(name or ('Chữ • ' + body[:20]), 'FONT')
    cu.body = body; cu.font = font(fontfile); cu.size = size; cu.extrude = extrude
    cu.resolution_u = 3          # 21.250: chữ mịn vừa đủ (mặc định 12 làm mỗi bảng ~4.800 tam giác)
    cu.align_x = align; cu.align_y = 'CENTER'
    ob = bpy.data.objects.new(cu.name, cu); ob.location = loc; ob.rotation_euler = rot
    cu.materials.append(m); col.objects.link(ob)
    return ob

# ---------------------------------------------------------------- organic
def blob(col, name, center, radius, m, n=6, spread=0.8, seed=0, disp=0.5, subdiv=2, squash=0.8):
    """Khối tán lá/bụi hoa: vài cầu ico + Displace mây."""
    rnd = random.Random(seed)
    bm = bmesh.new()
    for i in range(n):
        r = radius * rnd.uniform(0.55, 0.9)
        off = Vector((rnd.uniform(-1, 1), rnd.uniform(-1, 1), rnd.uniform(-0.4, 0.6))) * radius * spread
        bmesh.ops.create_icosphere(bm, subdivisions=subdiv, radius=r, matrix=Matrix.Translation(off) @ Matrix.Diagonal((1, 1, squash, 1)))
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = True
    me.materials.append(m)
    ob = bpy.data.objects.new(name, me); ob.location = center; col.objects.link(ob)
    tex = bpy.data.textures.get('OW_LeafClouds') or bpy.data.textures.new('OW_LeafClouds', 'CLOUDS')
    tex.noise_scale = 0.45
    md = ob.modifiers.new('Displace', 'DISPLACE'); md.texture = tex; md.strength = disp; md.texture_coords = 'GLOBAL'
    return ob

def instance(col, src, loc, rz=0.0, s=1.0, name=None):
    ob = bpy.data.objects.new(name or src.name + '.i', src.data)
    ob.location = loc; ob.rotation_euler = (0, 0, rz); ob.scale = (s, s, s)
    for md in src.modifiers:
        n = ob.modifiers.new(md.name, md.type)
        if md.type == 'DISPLACE': n.texture = md.texture; n.strength = md.strength; n.texture_coords = 'LOCAL'
    col.objects.link(ob)
    return ob
