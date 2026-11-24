# 21.389 — Banner «Cù Lao Petshop» mẫu 2 «Ấm áp» (người dùng duyệt từ 4 mẫu 21.388).
# Mặt banner là ẢNH in (texture vẽ SVG ở ArtSource/Concepts/BangRonPetshop-21388/banner.html ?c=2) dán trên
# tấm phẳng có UV, thay cho chữ/đĩa/dấu chân 3D của 21.384. Giữ khung nhôm, bu lông và đèn rọi của 21.384.
#   OW_BangRon_Petshop_Ngang 13 × 9,5 m → vật liệu OW_BangRonPetshop_Ngang (ảnh 2048×2048 kéo giãn, UV co lại đúng tỉ lệ)
#   OW_BangRon_Petshop_Doc    7 × 14 m  → vật liệu OW_BangRonPetshop_Doc (ảnh 1024×2048)
# Cùng tên mesh + cùng FBX (Library/OW_BangRon_Petshop.fbx) nên InstallPetshopBanner21384.Run đặt lại được ngay.
# Chạy: ns = {'OW_DIR': ...}; exec(open(.../build_petshop_banner_anh.py).read(), ns); ns['build_all']()
import bpy, os
OW_DIR = globals().get('OW_DIR') or os.path.dirname(os.path.abspath(__file__))
_base = os.path.join(OW_DIR, 'build_petshop_banner.py')
exec(compile(open(_base, encoding='utf-8').read(), _base, 'exec'), globals())   # MB, mat, frame/spotlights, join_coll, FBX…

TEXDIR = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/Assets/PhoViet/OpenWorld/Textures'


def mat_image(name, path):
    """Vật liệu ảnh (Blender chỉ để xem; Unity remap sang .mat cùng tên)."""
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    bsdf = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    img = bpy.data.images.load(path, check_existing=True)
    tex = next((n for n in nt.nodes if n.type == 'TEX_IMAGE'), None) or nt.nodes.new('ShaderNodeTexImage')
    tex.image = img
    nt.links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
    bsdf.inputs['Roughness'].default_value = 0.6
    return m


def image_panel(C, name, W, H, y, m):
    """Tấm phẳng W×H (x giữa 0, z 0…H) ở mặt y, pháp tuyến −y, UV phủ trọn ảnh (u theo x, v theo z)."""
    me = bpy.data.meshes.new(name)
    me.from_pydata([(-W / 2, y, 0), (W / 2, y, 0), (W / 2, y, H), (-W / 2, y, H)], [], [(0, 1, 2, 3)])
    uv = me.uv_layers.new(name='UVMap')
    for li, (u, v) in zip(range(4), ((0, 0), (1, 0), (1, 1), (0, 1))):
        uv.data[li].uv = (u, v)
    me.materials.append(m)
    me.update()
    ob = bpy.data.objects.new(name, me)
    C.objects.link(ob)
    return ob


def frame_only(sb, W, H, M):
    """Khung nhôm hộp + tấm lót + bu lông (như 21.384, bỏ viền trắng trong vì ảnh đã có viền)."""
    sb.box((0, -0.07, H / 2), (W + 0.3, 0.14, H + 0.3), M['alu'])
    sb.box((0, -0.16, H / 2), (W, 0.04, H), M['alu'])
    for x in (-W / 2 - 0.05, W / 2 + 0.05):
        for z in (0.12, H - 0.12):
            sb.cyl((x, -0.2, z), 0.06, 0.06, M['fr'], seg=8, axis='Y')
    return -0.185


def banner_ngang_anh(C, M, W=13.0, H=9.5):
    sb = MB('OW_BangRon_Petshop_Ngang_Khung')
    yf = frame_only(sb, W, H, M)
    spotlights(sb, W, H, 3, M)
    sb.finish(C)
    image_panel(C, 'OW_BangRon_Petshop_Ngang_Anh', W, H, yf, mat_image('OW_BangRonPetshop_Ngang', TEXDIR + '/OW_BangRonPetshop_Ngang.png'))
    return 'OW_BangRon_Petshop_Ngang'


def banner_doc_anh(C, M, W=7.0, H=14.0):
    sb = MB('OW_BangRon_Petshop_Doc_Khung')
    yf = frame_only(sb, W, H, M)
    spotlights(sb, W, H, 2, M)
    sb.finish(C)
    image_panel(C, 'OW_BangRon_Petshop_Doc_Anh', W, H, yf, mat_image('OW_BangRonPetshop_Doc', TEXDIR + '/OW_BangRonPetshop_Doc.png'))
    return 'OW_BangRon_Petshop_Doc'


def build_all(export=True):
    M = mats()
    out = []
    for i, fn in enumerate((banner_ngang_anh, banner_doc_anh)):
        cname = 'BangRon_Petshop_%d' % i
        clear_coll(cname)
        C = coll(cname)
        name = fn(C, M)
        ob = join_coll(C, name)
        ob.location.x = i * 16.0
        out.append((name, len(ob.data.polygons), tuple(round(v, 2) for v in ob.dimensions), [s.material.name for s in ob.material_slots], [u.name for u in ob.data.uv_layers]))
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
