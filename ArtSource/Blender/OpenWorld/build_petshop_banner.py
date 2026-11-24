# 21.384 — Banner quảng cáo «Cù Lao Petshop» dán lên 2 mảng tường trống ở ngã 3 nơi xe xuất phát.
# Cùng phong cách biển Cù Lao Petshop (build_petshop_signs.py): nền xanh ngọc #16808A, viền trắng, đĩa trắng có dấu
# chân cam #F28C28, chữ trắng Arial Bold; thêm dải cam «Spa - Lưu Trú - Thú y», vệt dấu chân nhỏ, khung nhôm tối và
# đèn rọi trên đỉnh.
#   OW_BangRon_Petshop_Ngang  13 × 9,5 m  (tường hồng, mặt hông bắc nhà góc nam)
#   OW_BangRon_Petshop_Doc     7 × 14 m   (tường kem, mặt hông đông nhà góc bắc)
# Hệ toạ độ cục bộ: x = bề ngang, z = lên, mặt đọc quay −y; gốc = giữa mép dưới, nằm trên mặt tường (y = 0).
# Chạy qua Blender MCP: ns = {'OW_DIR': ...}; exec(open(.../build_petshop_banner.py).read(), ns); ns['build_all']()
import bpy, math, os
OW_DIR = globals().get('OW_DIR') or os.path.dirname(os.path.abspath(__file__))
_lib = os.path.join(OW_DIR, 'ow_lib.py')
exec(compile(open(_lib, encoding='utf-8').read(), _lib, 'exec'), globals())

LIBDIR = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/Assets/PhoViet/OpenWorld/Library'
FBX = LIBDIR + '/OW_BangRon_Petshop.fbx'
TITLE1, TITLE2, SUB = 'Cù Lao', 'Petshop', 'Spa - Lưu Trú - Thú y'


def mats():
    return dict(
        bg=mat('OW_SignBG_16808A', '#16808A', 0.55),
        fr=mat('OW_FrameWhite', '#E9E6DE', 0.5),
        tx=mat('OW_SignTextWhite', '#F7F4EC', 0.4, emit='#FFFFFF', emit_strength=0.3),
        paw=mat('OW_PetshopPaw', '#F28C28', 0.45),
        alu=mat('OW_AluCharcoal', '#3A3D40', 0.4, metal=0.6),
        bulb=mat('OW_LampBulb', '#FFF3D6', 0.2, emit='#FFF1CF', emit_strength=4.0),
    )


def paw(sb, cx, cz, s, y, m, depth=0.03):
    """Dấu chân: đệm bầu dục + 4 ngón, mặt trước ở y, nổi ra −y. s = bán kính đĩa nền tương ứng."""
    t = depth / 2
    sb.sphere((cx, y - t, cz - 0.2 * s), 0.42 * s, m, seg=28, rings=8, scale=(1.12, t / (0.42 * s), 0.92))
    for dx, dz, r in ((-0.44, 0.22, 0.155), (-0.17, 0.47, 0.165), (0.17, 0.47, 0.165), (0.44, 0.22, 0.155)):
        sb.sphere((cx + dx * s, y - t, cz + dz * s), r * s, m, seg=18, rings=6, scale=(0.9, t / (r * s), 1.15))


def pill(sb, cx, cz, w, h, y, d, m):
    """Dải bo tròn hai đầu (mặt phẳng XZ), dày d từ y lùi về −y."""
    r = h / 2
    sb.box((cx, y - d / 2, cz), (w - h, d, h), m)
    for sx in (-1, 1):
        sb.cyl((cx + sx * (w / 2 - r), y - d, cz), r, d, m, seg=20, axis='Y')


def frame_and_panel(sb, W, H, M):
    """Khung nhôm tối bắt vào tường + tấm nền xanh ngọc + viền trắng trong. Trả về y mặt trước nền."""
    sb.box((0, -0.07, H / 2), (W + 0.3, 0.14, H + 0.3), M['alu'])            # khung hộp
    sb.box((0, -0.16, H / 2), (W, 0.04, H), M['bg'])                          # bạt nền
    yf = -0.18
    ins, bw = 0.28, 0.12                                                       # viền trắng trong
    for z in (ins, H - ins):
        sb.box((0, yf - 0.01, z), (W - 2 * ins + bw, 0.02, bw), M['fr'])
    for x in (-W / 2 + ins, W / 2 - ins):
        sb.box((x, yf - 0.01, H / 2), (bw, 0.02, H - 2 * ins), M['fr'])
    for x in (-W / 2 - 0.05, W / 2 + 0.05):                                    # bu lông bốn góc khung
        for z in (0.12, H - 0.12):
            sb.cyl((x, -0.2, z), 0.06, 0.06, M['fr'], seg=8, axis='Y')
    return yf


def spotlights(sb, W, H, n, M):
    """n đèn rọi trên đỉnh: tay đòn từ tường chìa ra 1,1 m, chao đèn chúc xuống mặt bạt."""
    for i in range(n):
        x = -W / 2 + W * (i + 0.5) / n
        z0 = H + 0.15
        sb.box((x, -0.1, z0 + 0.25), (0.22, 0.2, 0.5), M['alu'])               # đế bắt tường
        sb.rod((x, -0.15, z0 + 0.4), (x, -1.1, z0 + 0.75), 0.045, M['alu'])
        sb.box((x, -1.2, z0 + 0.7), (0.62, 0.36, 0.22), M['alu'], rx=math.radians(-35))
        sb.box((x, -1.18, z0 + 0.6), (0.52, 0.26, 0.04), M['bulb'], rx=math.radians(-35))


def paw_trail(sb, pts, s, y, m):
    for (x, z, a) in pts:
        paw(sb, x, z, s, y, m, depth=0.02)


def _fit(ob, max_w, max_h=None):
    bpy.context.view_layer.update()
    k = 1.0
    if ob.dimensions.x > max_w: k = max_w / ob.dimensions.x
    if max_h and ob.dimensions.y * k > max_h: k = max_h / ob.dimensions.y
    ob.scale = (k, k, k)


def _txt(C, body, x, z, size, m, max_w, y, max_h=None):
    t = text(C, body, (x, y - 0.012, z), size, m, (math.pi / 2, 0, 0), 'arialbd.ttf', extrude=0.012, name='Chữ • ' + body)
    _fit(t, max_w, max_h)
    return t


def banner_ngang(C, M, W=13.0, H=9.5):
    sb = MB('OW_BangRon_Petshop_Ngang_Khung')
    yf = frame_and_panel(sb, W, H, M)
    # đĩa trắng + dấu chân lớn bên trái
    R = 3.0; dx = -W / 2 + 0.75 + R; dz = H * 0.56
    sb.cyl((dx, yf - 0.03, dz), R, 0.03, M['fr'], seg=48, axis='Y')
    sb.cyl((dx, yf - 0.035, dz), R - 0.18, 0.005, M['bg'], seg=48, axis='Y')   # vòng chỉ xanh trong đĩa
    sb.cyl((dx, yf - 0.04, dz), R - 0.3, 0.005, M['fr'], seg=48, axis='Y')
    paw(sb, dx, dz, R * 0.95, yf - 0.045, M['paw'], depth=0.03)
    # dải cam phụ đề
    tx0 = dx + R + 0.6; tx1 = W / 2 - 0.9; tcx = (tx0 + tx1) / 2; tw = tx1 - tx0
    pill(sb, tcx, 2.15, tw, 1.25, yf, 0.03, M['paw'])
    # vệt dấu chân trắng nhỏ góc trên phải
    paw_trail(sb, [(W / 2 - 1.0, H - 0.95, 0), (W / 2 - 1.75, H - 1.5, 0), (W / 2 - 2.5, H - 0.95, 0)], 0.32, yf, M['fr'])
    spotlights(sb, W, H, 3, M)
    sb.finish(C)
    _txt(C, TITLE1, tcx, 6.55, 2.3, M['tx'], tw, yf, 2.0)
    _txt(C, TITLE2, tcx, 4.15, 2.3, M['tx'], tw, yf, 2.0)
    sb2 = MB('OW_BangRon_Petshop_Ngang_Gach')                                  # gạch chân chữ
    sb2.box((tcx, yf - 0.015, 3.05), (tw * 0.85, 0.02, 0.1), M['fr'])
    sb2.finish(C)
    _txt(C, SUB, tcx, 2.15, 0.8, M['tx'], tw - 1.1, yf - 0.03, 0.75)
    return 'OW_BangRon_Petshop_Ngang'


def banner_doc(C, M, W=7.0, H=14.0):
    sb = MB('OW_BangRon_Petshop_Doc_Khung')
    yf = frame_and_panel(sb, W, H, M)
    R = 2.2; dz = H - 0.7 - R
    sb.cyl((0, yf - 0.03, dz), R, 0.03, M['fr'], seg=48, axis='Y')
    sb.cyl((0, yf - 0.035, dz), R - 0.16, 0.005, M['bg'], seg=48, axis='Y')
    sb.cyl((0, yf - 0.04, dz), R - 0.26, 0.005, M['fr'], seg=48, axis='Y')
    paw(sb, 0, dz, R * 0.95, yf - 0.045, M['paw'], depth=0.03)
    tw = W - 1.3
    sb.box((0, yf - 0.015, 4.6), (tw * 0.8, 0.02, 0.1), M['fr'])               # gạch chân chữ
    pill(sb, 0, 3.3, tw, 1.05, yf, 0.03, M['paw'])
    paw_trail(sb, [(-1.6, 1.25, 0), (-0.55, 1.75, 0), (0.55, 1.25, 0), (1.6, 1.75, 0)], 0.3, yf, M['fr'])
    spotlights(sb, W, H, 2, M)
    sb.finish(C)
    _txt(C, TITLE1, 0, 7.6, 1.9, M['tx'], tw, yf, 1.7)
    _txt(C, TITLE2, 0, 5.6, 1.9, M['tx'], tw, yf, 1.7)
    _txt(C, SUB, 0, 3.3, 0.62, M['tx'], tw - 0.8, yf - 0.03, 0.6)
    return 'OW_BangRon_Petshop_Doc'


def _font_to_mesh(ob):
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
    n = bpy.data.objects.new(ob.name + '_Mesh', me)
    for c in ob.users_collection: c.objects.link(n)
    n.matrix_basis = ob.matrix_basis.copy()
    bpy.data.objects.remove(ob, do_unlink=True)
    return n


def join_coll(C, name):
    """Gộp mọi khối + chữ trong collection thành MỘT mesh (1 renderer trong Unity)."""
    for o in list(C.objects):
        if o.type == 'FONT': _font_to_mesh(o)
    obs = [o for o in C.objects if o.type == 'MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for o in obs: o.select_set(True)
    bpy.context.view_layer.objects.active = obs[0]
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    bpy.ops.object.join()
    ob = bpy.context.view_layer.objects.active
    ob.name = name; ob.data.name = name
    return ob


def build_all(export=True):
    M = mats()
    out = []
    for i, fn in enumerate((banner_ngang, banner_doc)):
        cname = 'BangRon_Petshop_%d' % i
        clear_coll(cname)
        C = coll(cname)
        name = fn(C, M)
        ob = join_coll(C, name)
        ob.location.x = i * 16.0           # đặt cạnh nhau để xem; xuất ở gốc
        out.append((name, len(ob.data.polygons), tuple(round(v, 2) for v in ob.dimensions)))
    if export:
        bpy.ops.object.select_all(action='DESELECT')
        obs = [bpy.data.objects['OW_BangRon_Petshop_Ngang'], bpy.data.objects['OW_BangRon_Petshop_Doc']]
        saved = [o.location.copy() for o in obs]
        for o in obs: o.location = (0, 0, 0); o.select_set(True)
        bpy.context.view_layer.objects.active = obs[0]
        bpy.ops.export_scene.fbx(filepath=FBX, use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y',
                                 apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False,
                                 use_mesh_modifiers=True, mesh_smooth_type='FACE', path_mode='STRIP')
        for o, l in zip(obs, saved): o.location = l
    return out
