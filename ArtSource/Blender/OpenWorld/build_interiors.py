# 21.272 — mặt tiền kính + nội thất tầng trệt. Chế độ (biến toàn cục MODE trước khi exec):
#   'preview' : dựng 18 căn mẫu (14 cửa hàng + 4 nhà ở) bề ngang PREVIEW_W, render ảnh duyệt vào ArtSource/Concepts/OpenWorld-20261117/NoiThat-21272/
# Chạy nền:  blender -b --factory-startup --python <runner>  (runner exec run_in_blender.py với TARGET='build_interiors.py')
import bpy, math, os, random, sys
OW = globals().get('OW_DIR', 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld')
sys.path.insert(0, OW)
import importlib
import ow_lib as L; importlib.reload(L)
import ow_interiors as I; importlib.reload(I)

ROOT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
MODE = globals().get('MODE', 'preview')
PREVIEW_W = globals().get('PREVIEW_W', 6.0)
ONLY = globals().get('ONLY', None)
OUT = ROOT + '/ArtSource/Concepts/OpenWorld-20261117/NoiThat-21272'
GH = 4.2

def storefront(mb, w, shop, P):
    """Mặt tiền kính tầng trệt (hệ cục bộ nhà): khung nhôm, kính, cửa (tiệm: 2 cánh kính tay nắm inox; nhà ở: 4 cánh nhôm kính
    + hộp cửa cuốn đã kéo lên), ô kính trên cửa, tấm chân."""
    frame = P('KhungNhom', '#2F3438', rough=0.35, metal=0.6)
    glass = P('KinhMatTien', '#CFE3EA', rough=0.03, alpha=0.09)
    steel = P('Inox', '#C9CED3', rough=0.25, metal=0.9)
    y0, y1 = -w / 2 + 0.4, w / 2 - 0.4; x = -0.40; z0, z1, zt = 0.27, 2.75, 3.42
    mb.boxab((x - 0.03, y0, z0), (x + 0.03, y1, z0 + 0.14), frame)                     # tấm chân
    mb.boxab((x - 0.03, y0, z1 - 0.04), (x + 0.03, y1, z1 + 0.04), frame)              # đố ngang trên cửa
    mb.boxab((x - 0.04, y0, zt - 0.06), (x + 0.04, y1, zt + 0.04), frame)
    for yy in (y0, y1): mb.box((x, yy, (z0 + zt) / 2), (0.08, 0.08, zt - z0), frame)
    if shop:
        dw = min(1.8, (y1 - y0) * 0.42); dc = y0 + (y1 - y0) * 0.62
        for yy in (dc - dw / 2, dc + dw / 2): mb.box((x, yy, (z0 + zt) / 2), (0.07, 0.07, zt - z0), frame)
        for yy in sorted(set([y0 + (dc - dw / 2 - y0) * k / max(1, int((dc - dw / 2 - y0) / 1.4)) for k in range(1, max(1, int((dc - dw / 2 - y0) / 1.4)))])):
            mb.box((x, yy, (z0 + z1) / 2), (0.05, 0.05, z1 - z0), frame)
        for yy in sorted(set([dc + dw / 2 + (y1 - dc - dw / 2) * k / max(1, int((y1 - dc - dw / 2) / 1.4)) for k in range(1, max(1, int((y1 - dc - dw / 2) / 1.4)))])):
            mb.box((x, yy, (z0 + z1) / 2), (0.05, 0.05, z1 - z0), frame)
        mb.box((x, dc, (z0 + z1) / 2), (0.05, 0.03, z1 - z0 - 0.08), frame)             # khe giữa hai cánh
        for sy in (-1, 1):
            mb.rod((x + 0.06, dc + sy * 0.09, 0.95), (x + 0.06, dc + sy * 0.09, 1.65), 0.016, steel)
            for zz in (0.97, 1.63): mb.rod((x + 0.0, dc + sy * 0.09, zz), (x + 0.06, dc + sy * 0.09, zz), 0.01, steel)
        mb.boxab((x - 0.004, y0 + 0.04, z0 + 0.14), (x + 0.004, y1 - 0.04, zt - 0.06), glass)
    else:
        n = 4; seg = (y1 - y0) / n
        for k in range(n + 1): mb.box((x, y0 + k * seg, (z0 + z1) / 2), (0.06, 0.06, z1 - z0), frame)
        for k in range(n):
            yy = y0 + (k + 0.5) * seg
            mb.box((x, yy, 1.1), (0.05, seg - 0.06, 0.05), frame)                        # nẹp ngang cánh
            mb.box((x + 0.035, yy + (seg / 2 - 0.12) * (1 if k % 2 == 0 else -1), 1.15), (0.03, 0.03, 0.22), steel)
        mb.boxab((x - 0.004, y0 + 0.04, z0 + 0.14), (x + 0.004, y1 - 0.04, zt - 0.06), glass)
        mb.boxab((x - 0.02, y0 - 0.1, zt + 0.04), (x + 0.32, y1 + 0.1, zt + 0.36), P('HopCuaCuon', '#9AA0A6', metal=0.5, rough=0.4))

def preview_house(col, w, kind, shop, rnd, sign=None):
    """Căn mẫu: vỏ nhà quanh phòng (tường hai bên, tầng trên, ô văng, trụ), vỉa hè, mái hiên, biển hiệu."""
    P = lambda *a, **k: I.P(L, *a, **k)
    fac = P('MatTienKem', '#E8DEC4', rough=0.85)
    mb = L.MB('NhaMau_' + kind)
    mb.boxab((-9.0, -w / 2, 0.0), (0.0, -w / 2 + 0.19, GH + 3.3), fac)
    mb.boxab((-9.0, w / 2 - 0.19, 0.0), (0.0, w / 2, GH + 3.3), fac)
    mb.boxab((-9.0, -w / 2, I.CEIL + 0.04), (-0.35, w / 2, GH + 3.3), fac)
    mb.boxab((-0.35, -w / 2, GH), (0.0, w / 2, GH + 3.3), fac)
    mb.boxab((-0.62, -w / 2 + 0.19, I.CEIL + 0.04), (-0.35, w / 2 - 0.19, GH), fac)
    for yy in (-w / 2 + 0.2, w / 2 - 0.2): mb.box((-0.2, yy, GH / 2), (0.4, 0.4, GH), fac)
    mb.boxab((-0.02, -w / 2, GH - 0.08), (0.12, w / 2, GH + 0.08), P('GoTrang', '#F4F2EC'))
    for k in range(2):   # cửa sổ tầng 2
        wy = -w / 4 + k * w / 2
        mb.box((0.02, wy, GH + 1.7), (0.08, 1.6, 1.75), P('KhungCuaSo', '#F4F2EC'))
        mb.box((0.05, wy, GH + 1.7), (0.05, 1.4, 1.55), P('KinhCuaSo', '#3E5466', rough=0.1))
    mb.boxab((-0.4, -w / 2, 0.0), (0.1, w / 2, 0.27), P('BeTong', '#B9B4AA'))
    mb.boxab((0.1, -w / 2 - 3, -0.02), (3.5, w / 2 + 3, 0.15), P('GachViaHe', '#A9A49A', rough=0.8))
    mb.boxab((3.5, -w / 2 - 3, -0.2), (9.0, w / 2 + 3, -0.02), P('NhuaDuong', '#3A3B3D', rough=0.9))
    aw = P('MaiHien', '#B23A2E' if shop else '#2E6B5A', rough=0.9)
    mb.box((0.85, 0, 3.25), (1.9, w - 0.35, 0.05), aw, ry=0.32)
    storefront(mb, w, shop, P)
    mb.finish(col)
    if sign:
        title, colr = sign
        b = L.MB('BangHieu_' + kind)
        b.box((0.12, 0, GH + 0.55), (0.12, w - 0.3, 1.0), P('Bang_' + colr, colr, rough=0.5))
        b.box((0.06, 0, GH + 0.55), (0.06, w - 0.2, 1.1), P('VienBang', '#F4F2EC'))
        b.finish(col)
        L.text(col, title, (0.19, 0, GH + 0.55), 0.5, P('ChuBang', '#FFFFFF' if colr not in ('#E0A321', '#C9952A') else '#2B1A0E', rough=0.5), rot=(math.pi / 2, 0, math.pi / 2), extrude=0.02)

def furnish(col, w, kind, seed, title=None, sub=None):
    rnd = random.Random(seed)
    mb = L.MB('NoiThat_' + kind)
    X = I.Ctx(L, mb, col, w, rnd); X.kind = kind; X.title = title; X.sub = sub
    I.FURNISH[kind](X, rnd)
    return mb.finish(col)

def setup_render(cam_loc, target):
    sc = bpy.context.scene
    for eng in ('BLENDER_EEVEE_NEXT', 'BLENDER_EEVEE'):
        try: sc.render.engine = eng; break
        except TypeError: pass
    sc.render.resolution_x, sc.render.resolution_y = 1200, 760
    sc.render.film_transparent = False
    try: sc.eevee.taa_render_samples = 48
    except Exception: pass
    try: sc.eevee.use_raytracing = True
    except Exception: pass
    sc.view_settings.view_transform = 'Standard'; sc.view_settings.exposure = -0.35
    w = sc.world or bpy.data.worlds.new('W'); sc.world = w; w.use_nodes = True
    bg = next(n for n in w.node_tree.nodes if n.type == 'BACKGROUND'); bg.inputs[0].default_value = (0.62, 0.75, 0.95, 1); bg.inputs[1].default_value = 0.9
    cam = bpy.data.objects.get('CamDuyet')
    if not cam:
        cam = bpy.data.objects.new('CamDuyet', bpy.data.cameras.new('CamDuyet')); sc.collection.objects.link(cam)
    cam.data.lens = 20
    cam.location = cam_loc
    d = (target[0] - cam_loc[0], target[1] - cam_loc[1], target[2] - cam_loc[2])
    import mathutils
    cam.rotation_euler = mathutils.Vector(d).to_track_quat('-Z', 'Y').to_euler()
    sc.camera = cam
    sun = bpy.data.objects.get('NangChieu')
    if not sun:
        sun = bpy.data.objects.new('NangChieu', bpy.data.lights.new('NangChieu', 'SUN')); sc.collection.objects.link(sun)
    sun.data.energy = 3.2; sun.rotation_euler = (math.radians(50), 0, math.radians(120))

def room_lights(col, w, warm=True):
    """Đèn vùng trong phòng (chỉ để render duyệt — trong game là đèn giả trong shader + đèn trần phát sáng)."""
    for k, xx in enumerate((-2.2, -5.4)):
        ld = bpy.data.lights.new('DenPhong%d' % k, 'AREA'); ld.energy = 140 * (w / 6.0); ld.size = 2.0
        ld.color = (1.0, 0.9, 0.78) if warm else (0.92, 0.96, 1.0)
        ob = bpy.data.objects.new(ld.name, ld); ob.location = (xx, 0, I.CEIL - 0.1); col.objects.link(ob)

def preview():
    os.makedirs(OUT, exist_ok=True)
    for n in ('Cube', 'Light', 'Camera'):          # cảnh mặc định của --factory-startup
        o = bpy.data.objects.get(n)
        if o: bpy.data.objects.remove(o, do_unlink=True)
    kinds = I.SHOP_TYPES + I.HOME_TYPES
    if ONLY: kinds = [k for k in kinds if k in ONLY]
    done = []
    for i, kind in enumerate(kinds):
        for c in list(bpy.data.collections):
            if c.name.startswith('Duyet_'): L.clear_coll(c.name)
        for o in [o for o in bpy.data.objects if o.type == 'LIGHT' and o.name.startswith('DenPhong')]: bpy.data.objects.remove(o)
        col = L.coll('Duyet_' + kind)
        shop = kind in I.SHOP_TYPES
        sign = (I.SHOP_NAMES[I.SHOP_TYPES.index(kind)], ['#7E2A1E', '#8A2B2B', '#E0A321', '#4E3628', '#3E6E9A', '#2B7A3B', '#1F7A4A', '#2B4A7A', '#9A3A22', '#C9952A', '#1F5FA8', '#7E2A1E', '#5B3A7A', '#2B8A3E'][I.SHOP_TYPES.index(kind)]) if shop else None
        preview_house(col, PREVIEW_W, kind, shop, random.Random(i), sign)
        furnish(col, PREVIEW_W, kind, 1000 + i)
        room_lights(col, PREVIEW_W, I.STYLE[kind][4] == 'warm')
        setup_render((2.7, PREVIEW_W * 0.1, 1.6), (-5.5, -PREVIEW_W * 0.04, 1.2))
        bpy.context.scene.render.filepath = OUT + '/%02d_%s.png' % ((I.SHOP_TYPES + I.HOME_TYPES).index(kind), kind)
        bpy.ops.render.render(write_still=True)
        done.append(kind)
        print('render', kind)
    return done


# ================================================================================================ xuất sang Unity
LIBDIR = ROOT + '/Assets/PhoViet/OpenWorld/Library'
WIDTHS = [4.5, 5.0, 5.5, 6.0, 7.0, 8.0]

def _fbx_import(path):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=path, axis_forward='-Z', axis_up='Y')
    return [o for o in bpy.data.objects if o not in before]

def _fbx_export(objs, path):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'EMPTY', 'MESH'}, axis_forward='-Z', axis_up='Y',
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False,
                             mesh_smooth_type='FACE', colors_type='LINEAR')

def _clear():
    for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)

def open_house(path, w, shop):
    """Nhà mẫu FBX → gỡ mặt tường tầng trệt (mặt trước khối thân ở x = −0,35), tấm kính đèn + đố cũ; gắn mặt tiền kính
    + lanh tô trên kính; xuất đè đúng file (Unity cập nhật mọi bản đặt)."""
    import bmesh
    _clear(); objs = _fbx_import(path)
    meshes = [o for o in objs if o.type == 'MESH']
    removed = 0
    for ob in meshes:
        me = ob.data; mw = ob.matrix_world; nm = mw.to_3x3().inverted().transposed()
        bm = bmesh.new(); bm.from_mesh(me)
        kill = []
        for f in bm.faces:
            mname = me.materials[f.material_index].name if f.material_index < len(me.materials) and me.materials[f.material_index] else ''
            c = mw @ f.calc_center_median(); n = (nm @ f.normal).normalized()
            zs = [(mw @ v.co).z for v in f.verts]
            base = mname.split('.')[0]
            if base in ('OW_ShopGlow', 'OW_ShopGlowCool'):                                          # tấm kính đèn cũ
                kill.append(f); continue
            if abs(c.x + 0.35) < 0.03 and n.x > 0.9 and min(zs) < 0.3 and max(zs) > 3.0:          # mặt trước khối thân (che tầng trệt)
                kill.append(f); continue
            if base == 'OW_FrameDark' and -0.5 < c.x < -0.3 and max(zs) < 3.6:                       # đố/khung tấm kính đèn cũ
                kill.append(f); continue
        removed += len(kill)
        bmesh.ops.delete(bm, geom=kill, context='FACES')
        bm.to_mesh(me); bm.free()
    P = lambda *a, **k: I.P(L, *a, **k)
    col = L.coll('MatTien')
    mb = L.MB('OW_MatTienKinh')
    storefront(mb, w, shop, P)
    fac = P('LanhTo', '#E8DEC4', rough=0.85)
    mb.boxab((-0.62, -w / 2 + 0.19, 3.46), (-0.35, w / 2 - 0.19, GH), fac)        # lanh tô che khe trên kính
    ob = mb.finish(col)
    ob.parent = meshes[0]; ob.matrix_parent_inverse = meshes[0].matrix_world.inverted()
    _fbx_export(objs + [ob], path)
    return removed

def _vc_materials():
    """Vật liệu chung đọc màu đỉnh 'Col' (để render ảnh xa đúng màu như trong Unity): màu gốc + phát sáng theo alpha."""
    opaque = bpy.data.materials.get('NT_NoiThat')
    if opaque is None:
        opaque = bpy.data.materials.new('NT_NoiThat'); opaque.use_nodes = True
        nt = opaque.node_tree; b = L._bsdf(opaque)
        at = nt.nodes.new('ShaderNodeAttribute'); at.attribute_name = 'Col'
        nt.links.new(at.outputs['Color'], b.inputs['Base Color'])
        nt.links.new(at.outputs['Color'], b.inputs['Emission Color'])
        mul = nt.nodes.new('ShaderNodeMath'); mul.operation = 'MULTIPLY'; mul.inputs[1].default_value = 6.0
        nt.links.new(at.outputs['Alpha'], mul.inputs[0]); nt.links.new(mul.outputs[0], b.inputs['Emission Strength'])
        b.inputs['Roughness'].default_value = 0.6
    glass = bpy.data.materials.get('NT_NoiThatKinh')
    if glass is None:
        glass = bpy.data.materials.new('NT_NoiThatKinh'); glass.use_nodes = True
        b = L._bsdf(glass); b.inputs['Base Color'].default_value = (0.75, 0.85, 0.9, 1); b.inputs['Alpha'].default_value = 0.25; b.inputs['Roughness'].default_value = 0.05
        try: glass.surface_render_method = 'BLENDED'
        except Exception: pass
    return opaque, glass

def bake_vertex_colors(ob):
    """Màu vật liệu → màu đỉnh (góc mặt): rgb = màu gốc, a = phát sáng (0…1). Gom về 2 vật liệu: NT_NoiThat, NT_NoiThatKinh."""
    me = ob.data
    if 'Col' not in me.color_attributes: me.color_attributes.new('Col', 'FLOAT_COLOR', 'CORNER')
    ca = me.color_attributes['Col']
    info = []
    for m in me.materials:
        if m is None: info.append(((0.8, 0.8, 0.8), 0.0, 1.0)); continue
        b = L._bsdf(m); bc = b.inputs['Base Color'].default_value
        info.append(((bc[0], bc[1], bc[2]), min(1.0, float(m.get('nt_emit', 0.0)) / 8.0), float(m.get('nt_alpha', 1.0))))
    for poly in me.polygons:
        rgb, e, al = info[poly.material_index]
        for li in poly.loop_indices: ca.data[li].color = (rgb[0], rgb[1], rgb[2], e if al >= 0.99 else al)
    opaque, glass = _vc_materials()
    idx = [1 if info[p.material_index][2] < 0.99 else 0 for p in me.polygons]
    me.materials.clear(); me.materials.append(opaque); me.materials.append(glass)
    for p, k in zip(me.polygons, idx): p.material_index = k

def build_interior_object(kind, w, seed, title=None, sub=None, name=None):
    col = L.coll('NT_%s_%s' % (name or kind, str(w).replace('.', '_')))
    ob = furnish(col, w, kind, seed, title, sub)
    texts = [o for o in col.objects if o.type == 'FONT']
    for t in texts:
        bpy.context.view_layer.objects.active = t
        bpy.ops.object.select_all(action='DESELECT'); t.select_set(True)
        bpy.ops.object.convert(target='MESH')
    meshes = [o for o in col.objects if o.type == 'MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes: o.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.join()
    ob.name = 'NT_' + (name or kind)
    bake_vertex_colors(ob)
    return ob

# 21.372 (người dùng: «trên mobile dù để Rất cao mặt tiền cửa hàng vẫn rất mờ»): gói điện thoại chỉ có ảnh xa nội thất, ô cũ
# 192 × 144 (texture web 1024 px → ~102 × 77 px/căn). Ảnh render 1168 px (CARD_RES) vào thư mục *HD, ô atlas 584 × 438, lưới
# 7 cột × 8 hàng (4088 × 3504, ≤ 4096) — ghép bằng ArtSource/Blender/OpenWorld/ow_interior_atlas_hd.py (Python ngoài, PIL).
# FBX cũ (UV lưới 10 × 6) được Unity đổi UV sang lưới mới khi nhập (PhoVietInteriorMeshPostprocessor.RemapCardUv).
CARD_DIR = ROOT + '/ArtSource/Concepts/OpenWorld-20261117/NoiThat-21272/AnhXaHD'
CARD_RES = globals().get('CARD_RES', 1168)
CELL = (584, 438); ATLAS_COLS = 7
ALL_KINDS = I.SHOP_TYPES + I.HOME_TYPES

NT_WIDTHS = [5.0, 6.0, 8.0]      # 21.272: chỉ 3 bộ nội thất (bề ngang khác co giãn ngang trong Unity) cho nhẹ bản web
def card_index(kind, w): return NT_WIDTHS.index(w) * len(ALL_KINDS) + ALL_KINDS.index(kind)

def render_card(kind, w, show):
    """Ảnh nội thất nhìn từ vỉa hè (phối cảnh, vừa khít ô kính) cho LOD xa. Chỉ hiện các đối tượng trong show."""
    for o in bpy.data.objects:
        if o.type in ('MESH', 'FONT'): o.hide_render = o not in show
    os.makedirs(CARD_DIR, exist_ok=True)
    W = w - 0.8; H = 3.15
    for o in [o for o in bpy.data.objects if o.type == 'LIGHT' and o.name.startswith('DenPhong')]: bpy.data.objects.remove(o)
    col = L.coll('DenAnhXa'); room_lights(col, w, I.STYLE[kind][4] == 'warm')
    setup_render((2.6, 0.0, 1.85), (-5.0, 0.0, 1.85))
    sc = bpy.context.scene; cam = sc.camera
    cam.data.sensor_fit = 'HORIZONTAL'; cam.data.sensor_width = 36
    import math as _m
    cam.data.lens = 36 / (2 * (W / 2) / (2.6 + 0.4))          # khung ngang = ô kính tại mặt kính (x = −0,4)
    sc.render.resolution_x = CARD_RES; sc.render.resolution_y = int(CARD_RES * H / W)
    try: sc.eevee.taa_render_samples = 32
    except Exception: pass
    sc.render.filepath = CARD_DIR + '/%03d_%s_%s.png' % (card_index(kind, w), kind, str(w).replace('.', '_'))
    bpy.ops.render.render(write_still=True)
    for o in bpy.data.objects:
        if o.type in ('MESH', 'FONT'): o.hide_render = False
    return sc.render.filepath

def card_object(kind, w):
    """Tấm ảnh xa đặt ngay sau kính, UV trỏ vào ô của atlas."""
    import bmesh
    idx = card_index(kind, w); cx, cy = idx % ATLAS_COLS, idx // ATLAS_COLS
    rows = (len(NT_WIDTHS) * len(ALL_KINDS) + ATLAS_COLS - 1) // ATLAS_COLS
    u0, u1 = cx / ATLAS_COLS, (cx + 1) / ATLAS_COLS; v1, v0 = 1 - cy / rows, 1 - (cy + 1) / rows
    y0, y1, z0, z1, x = -w / 2 + 0.42, w / 2 - 0.42, 0.29, 3.4, -0.46
    me = bpy.data.meshes.new('NT_%s_Xa' % kind); bm = bmesh.new(); uv = bm.loops.layers.uv.new('UVMap')
    vs = [bm.verts.new(p) for p in ((x, y1, z0), (x, y0, z0), (x, y0, z1), (x, y1, z1))]   # pháp tuyến +x (ra đường)
    f = bm.faces.new(vs)
    for loop, (u, v) in zip(f.loops, ((u0, v0), (u1, v0), (u1, v1), (u0, v1))): loop[uv].uv = (u, v)
    f.normal_update()
    if f.normal.x < 0:
        f.normal_flip()
    bm.to_mesh(me); bm.free()
    mat = bpy.data.materials.get('NT_NoiThatXa') or bpy.data.materials.new('NT_NoiThatXa'); me.materials.append(mat)
    ob = bpy.data.objects.new(me.name, me); bpy.context.scene.collection.objects.link(ob)
    return ob

def build_atlas():
    from PIL import Image
    n = len(NT_WIDTHS) * len(ALL_KINDS); rows = (n + ATLAS_COLS - 1) // ATLAS_COLS
    at = Image.new('RGB', (CELL[0] * ATLAS_COLS, CELL[1] * rows), (40, 40, 40))
    import glob
    for f in glob.glob(CARD_DIR + '/*.png'):
        i = int(os.path.basename(f)[:3]); im = Image.open(f).convert('RGB').resize(CELL, Image.LANCZOS)
        at.paste(im, ((i % ATLAS_COLS) * CELL[0], (i // ATLAS_COLS) * CELL[1]))
    out = LIBDIR + '/NoiThat/OW_NoiThat_AnhXa.png'; at.save(out); return out

def export_interiors(widths=None, kinds=None):
    os.makedirs(LIBDIR + '/NoiThat', exist_ok=True)
    stats = []
    for w in widths or NT_WIDTHS:
        _clear()
        objs = []
        for kind in kinds or ALL_KINDS:
            i = ALL_KINDS.index(kind)
            ob = build_interior_object(kind, w, 1000 + i)
            if globals().get('CARDS', True): render_card(kind, w, [ob])
            objs.append(ob); objs.append(card_object(kind, w))
            stats.append((kind, w, sum(len(p.vertices) - 2 for p in ob.data.polygons)))
        for o in [o for o in bpy.data.objects if o.type == 'LIGHT' or o.type == 'CAMERA']: bpy.data.objects.remove(o)
        _fbx_export(objs, LIBDIR + '/NoiThat/OW_NoiThat_%s.fbx' % str(w).replace('.', '_'))
    return stats

def export_houses():
    out = []
    for w in WIDTHS:
        for v in range(4):
            for slot in (False, True):
                path = LIBDIR + '/OW_Mau_NhaPho_%s_%d%s.fbx' % (str(w).replace('.', '_'), v, '_B' if slot else '')
                out.append((os.path.basename(path), open_house(path, w, slot)))
    return out

if MODE == 'preview':
    res = preview()
    print('xong', len(res))
elif MODE == 'export_interiors':
    st = export_interiors(globals().get('ONLY_W'), ONLY)
    for k in st: print('noithat', *k)
    try: print('atlas', build_atlas())
    except ImportError: print('atlas: ghép bằng Python ngoài (Blender không có PIL)')
elif MODE == 'export_houses':
    for k in export_houses(): print('nha', *k)

# ------------------------------------------------------------------------------------------------ 14 nhà nổi bật (tên thật, bề ngang thật)
FEATURED = [   # (id, bề ngang, loại, tên trên bảng trong tiệm, dòng phụ)
    (4000, 8.0, 'PhoBo', 'Phở Thịnh', 'Phở bò – Phở gà'), (4001, 6.0, 'NhaSach', 'Nhà Sách Minh Tâm', None),
    (4002, 5.5, 'DienMay', 'Điện Máy Hải Đăng', None), (4003, 5.5, 'CaPhe', 'Cà Phê Sài Gòn', None),
    (4004, 6.0, 'TapHoa', 'Tạp Hóa Thanh Hương', None), (4005, 5.0, 'TiemVang', 'Tiệm Vàng Kim Phát', 'Vàng bạc – Đá quý'),
    (4006, 9.0, 'ComTam', 'Cơm Tấm Sài Gòn', None), (4007, 7.0, 'BanhMi', 'Bánh Mì Huế', None),
    (4008, 6.0, 'PhoBo', 'Phở Hà Nội', None), (4009, 6.0, 'TraSua', 'Trà Sữa', None),
    (4010, 7.0, 'HaiSan', 'Ốc Đêm', 'Ốc – Hải sản tươi'), (4011, 9.0, 'HaiSan', 'Hải Sản Tươi Sống', None),
    (4012, 8.5, 'CaPhe', 'Cà Phê Cù Lao', None), (4013, 7.5, 'LauBo', 'Lẩu Bò Cù Lao', None),
]
FCARD_DIR = ROOT + '/ArtSource/Concepts/OpenWorld-20261117/NoiThat-21272/AnhXaNoiBatHD'      # 21.372: bản HD (cũ: AnhXaNoiBat, 384 px)
FCOLS, FROWS = 5, 3

def featured_card_object(fid, i, w):
    import bmesh
    cx, cy = i % FCOLS, i // FCOLS
    u0, u1 = cx / FCOLS, (cx + 1) / FCOLS; v1, v0 = 1 - cy / FROWS, 1 - (cy + 1) / FROWS
    y0, y1, z0, z1, x = -w / 2 + 0.42, w / 2 - 0.42, 0.29, 3.4, -0.46
    me = bpy.data.meshes.new('NT_F_%d_Xa' % fid); bm = bmesh.new(); uv = bm.loops.layers.uv.new('UVMap')
    vs = [bm.verts.new(p) for p in ((x, y1, z0), (x, y0, z0), (x, y0, z1), (x, y1, z1))]
    f = bm.faces.new(vs)
    for loop, (u, v) in zip(f.loops, ((u0, v0), (u1, v0), (u1, v1), (u0, v1))): loop[uv].uv = (u, v)
    f.normal_update()
    if f.normal.x < 0: f.normal_flip()          # mặt ảnh quay ra đường (+x)
    bm.to_mesh(me); bm.free()
    mat = bpy.data.materials.get('NT_NoiThatXaNoiBat') or bpy.data.materials.new('NT_NoiThatXaNoiBat'); me.materials.append(mat)
    ob = bpy.data.objects.new(me.name, me); bpy.context.scene.collection.objects.link(ob)
    return ob

def export_featured():
    global CARD_DIR
    _clear(); objs = []; stats = []
    old = CARD_DIR; CARD_DIR = FCARD_DIR
    for i, (fid, w, kind, title, sub) in enumerate(FEATURED):
        ob = build_interior_object(kind, w, 5000 + fid, title, sub, name='F_%d' % fid)
        global card_index
        _ci = card_index; card_index = lambda k, ww, _i=i: _i
        render_card(kind, w, [ob])
        card_index = _ci
        objs.append(ob); objs.append(featured_card_object(fid, i, w))
        stats.append((fid, kind, title, sum(len(p.vertices) - 2 for p in ob.data.polygons)))
    CARD_DIR = old
    for o in [o for o in bpy.data.objects if o.type in ('LIGHT', 'CAMERA')]: bpy.data.objects.remove(o)
    _fbx_export(objs, LIBDIR + '/NoiThat/OW_NoiThat_NoiBat.fbx')
    return stats

if MODE == 'export_featured':
    for k in export_featured(): print('noibat', *k)

# ------------------------------------------------------------------------------------------------ 21.285: Cù Lao Petshop
# MODE 'export_petshop': nội thất PetShop 3 bề ngang (5/6/8 m) → Library/NoiThat/OW_NoiThat_PetShop.fbx gồm NT_PetShop_<w>
# (3D, màu đỉnh) + NT_PetShop_<w>_Xa (tấm ảnh xa, UV ô thứ i của atlas 3 × 1 OW_NoiThat_PetShop_AnhXa.png — ghép bằng Python
# ngoài vì Blender không có PIL). Có thể gọi từng bề ngang qua Blender MCP: petshop_width(w) rồi petshop_export().
PCARD_DIR = ROOT + '/ArtSource/Concepts/OpenWorld-20261117/Petshop-21285/AnhXaHD'            # 21.372: bản HD (cũ: AnhXa, 384 px)
PET_WIDTHS = [5.0, 6.0, 8.0]

def petshop_card_object(i, w):
    import bmesh
    u0, u1, v0, v1 = i / 3.0, (i + 1) / 3.0, 0.0, 1.0
    y0, y1, z0, z1, x = -w / 2 + 0.42, w / 2 - 0.42, 0.29, 3.4, -0.46
    key = str(w).replace('.', '_')
    me = bpy.data.meshes.new('NT_PetShop_%s_Xa' % key); bm = bmesh.new(); uv = bm.loops.layers.uv.new('UVMap')
    vs = [bm.verts.new(p) for p in ((x, y1, z0), (x, y0, z0), (x, y0, z1), (x, y1, z1))]
    f = bm.faces.new(vs)
    for loop, (u, v) in zip(f.loops, ((u0, v0), (u1, v0), (u1, v1), (u0, v1))): loop[uv].uv = (u, v)
    f.normal_update()
    if f.normal.x < 0: f.normal_flip()
    bm.to_mesh(me); bm.free()
    mat = bpy.data.materials.get('NT_NoiThatXaPetShop') or bpy.data.materials.new('NT_NoiThatXaPetShop'); me.materials.append(mat)
    ob = bpy.data.objects.new(me.name, me); bpy.context.scene.collection.objects.link(ob)
    return ob

def petshop_width(w):
    """Dựng NT_PetShop_<w> + render ảnh xa + tấm ảnh xa (giữ trong cảnh để xuất chung)."""
    global CARD_DIR, card_index
    i = PET_WIDTHS.index(w); key = str(w).replace('.', '_')
    ob = build_interior_object('PetShop', w, 2100 + i, name='PetShop_' + key)
    old_dir, old_ci = CARD_DIR, card_index
    CARD_DIR = PCARD_DIR; card_index = lambda k, ww, _i=i: _i
    try: render_card('PetShop', w, [ob])
    finally: CARD_DIR, card_index = old_dir, old_ci
    card = petshop_card_object(i, w)
    return ob, card, sum(len(p.vertices) - 2 for p in ob.data.polygons)

def petshop_export():
    objs = [o for o in bpy.data.objects if o.type == 'MESH' and o.name.startswith('NT_PetShop_')]
    for o in [o for o in bpy.data.objects if o.type in ('LIGHT', 'CAMERA')]: bpy.data.objects.remove(o)
    _fbx_export(objs, LIBDIR + '/NoiThat/OW_NoiThat_PetShop.fbx')
    return [o.name for o in objs]

if MODE == 'export_petshop':
    _clear()
    for w in PET_WIDTHS: print('petshop', w, petshop_width(w)[2])
    print('fbx', petshop_export())

# ------------------------------------------------------------------------------------------------ 21.372: chỉ render lại ảnh xa HD
# MODE 'render_cards_hd': dựng lại đúng như export_interiors / export_featured / petshop_width (cùng hạt giống, cùng thứ tự) và chỉ
# render ảnh (CARD_RES px) vào các thư mục *HD — KHÔNG xuất FBX (mesh 3D/UV trong Unity giữ nguyên). PART = 'main'|'featured'|'petshop'|None.
def render_cards_hd(part=None):
    out = []
    if part in (None, 'main'):
        for w in NT_WIDTHS:
            _clear()
            for i, kind in enumerate(ALL_KINDS):
                ob = build_interior_object(kind, w, 1000 + i); out.append(render_card(kind, w, [ob]))
                print('card', kind, w, flush=True)
    if part in (None, 'featured'):
        global CARD_DIR, card_index
        _clear(); old, _ci = CARD_DIR, card_index; CARD_DIR = FCARD_DIR
        try:
            for i, (fid, w, kind, title, sub) in enumerate(FEATURED):
                ob = build_interior_object(kind, w, 5000 + fid, title, sub, name='F_%d' % fid)
                card_index = lambda k, ww, _i=i: _i
                out.append(render_card(kind, w, [ob])); print('featured', fid, flush=True)
        finally: CARD_DIR, card_index = old, _ci
    if part in (None, 'petshop'):
        _clear()
        for w in PET_WIDTHS: petshop_width(w); print('petshop', w, flush=True)
    return out

if MODE == 'render_cards_hd':
    print('xong', len(render_cards_hd(globals().get('PART'))))
