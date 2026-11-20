# 21.285 — biển «Cù Lao Petshop» cho bản đồ mở: 6 bề ngang (4,5/5/5,5/6/7/8 m) → Library/OW_Mau_Bang_<w>_14.fbx
# (chỉ số 14 = sau 14 loại SIGN_SET). Cùng kiểu sign_board (build_ref3_segment): tấm nền 0,2 m sâu, cao 1,6 m, tâm z 4,3 m,
# viền trắng trên/dưới; mặt quay +x (ra đường). Chữ lớn «Cù Lao Petshop», dòng nhỏ «Spa-Lưu Trú-Thú y», icon dấu chân cam
# bên trái. Chữ tự co cho vừa bảng. Vật liệu đặt tên như các biển cũ để Unity gán: OW_SignBG_16808A, OW_FrameWhite,
# OW_SignTextWhite, OW_PetshopPaw.
# Chạy qua Blender MCP: ns = {'OW_DIR': ...}; exec(open(.../build_petshop_signs.py).read(), ns); ns['build_all']()
import bpy, math, os
OW_DIR = globals().get('OW_DIR') or os.path.dirname(os.path.abspath(__file__))
_lib = os.path.join(OW_DIR, 'ow_lib.py')
exec(compile(open(_lib, encoding='utf-8').read(), _lib, 'exec'), globals())

LIBDIR = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/Assets/PhoViet/OpenWorld/Library'
WIDTHS = [4.5, 5.0, 5.5, 6.0, 7.0, 8.0]
TITLE, SUB = 'Cù Lao Petshop', 'Spa-Lưu Trú-Thú y'
INDEX = 14

def _fit(ob, max_w):
    bpy.context.view_layer.update()
    w = ob.dimensions.x
    if w > max_w: s = max_w / w; ob.scale = (s, s, s)

def petshop_sign(C, w, x_face=0.0, z=4.3, h=1.6):
    y0 = -w / 2 + 0.15; bw = w - 0.3; yc = y0 + bw / 2
    bg = mat('OW_SignBG_16808A', '#16808A', 0.55)
    fr = mat('OW_FrameWhite', '#E9E6DE', 0.5)
    tx = mat('OW_SignTextWhite', '#F7F4EC', 0.4, emit='#FFFFFF', emit_strength=0.3)
    paw = mat('OW_PetshopPaw', '#F28C28', 0.45)
    sb = MB('OW_Bang_CuLaoPetshop_%s' % str(w).replace('.', '_'))
    sb.box((x_face + 0.12, yc, z), (0.2, bw - 0.3, h), bg)
    sb.box((x_face + 0.23, yc, z + h / 2 - 0.04), (0.04, bw - 0.3, 0.08), fr)
    sb.box((x_face + 0.23, yc, z - h / 2 + 0.04), (0.04, bw - 0.3, 0.08), fr)
    # icon dấu chân: đệm tròn + 4 ngón, trên nền tròn trắng
    py = y0 + 0.15 + 0.5; pz = z + 0.05
    sb.cyl((x_face + 0.235, py, pz), 0.48, 0.03, fr, seg=24, axis='X')
    sb.cyl((x_face + 0.255, py, pz - 0.1), 0.2, 0.02, paw, seg=18, axis='X')
    for dy, dz in ((-0.2, 0.12), (-0.08, 0.24), (0.08, 0.24), (0.2, 0.12)):
        sb.cyl((x_face + 0.255, py + dy, pz + dz), 0.075, 0.02, paw, seg=12, axis='X')
    sb.finish(C)
    rot = (math.pi / 2, 0, math.pi / 2)
    ty0 = py + 0.55; ty1 = y0 + bw - 0.3; tyc = (ty0 + ty1) / 2; tw = ty1 - ty0
    t = text(C, TITLE, (x_face + 0.25, tyc, z + 0.2), 0.8, tx, rot, 'arialbd.ttf', extrude=0.01, name='Chữ • ' + TITLE)
    _fit(t, tw)
    s = text(C, SUB, (x_face + 0.25, tyc, z - 0.47), 0.34, tx, rot, 'arialbd.ttf', extrude=0.01, name='Chữ • ' + SUB)
    _fit(s, tw * 0.9)
    return t, s

def _font_to_mesh(ob):
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
    n = bpy.data.objects.new(ob.name + '_Mesh', me)
    for c in ob.users_collection: c.objects.link(n)
    n.matrix_basis = ob.matrix_basis.copy()
    return n

def export_coll(C, path):
    temp, sel = [], []
    for o in C.objects:
        if o.type == 'FONT': t = _font_to_mesh(o); temp.append(t); sel.append(t)
        elif o.type == 'MESH': sel.append(o)
    bpy.ops.object.select_all(action='DESELECT')
    for o in sel: o.select_set(True)
    bpy.context.view_layer.objects.active = sel[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'EMPTY', 'MESH'}, axis_forward='-Z', axis_up='Y',
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False,
                             use_mesh_modifiers=True, mesh_smooth_type='FACE', path_mode='STRIP')
    for t in temp:
        me = t.data; bpy.data.objects.remove(t, do_unlink=True); bpy.data.meshes.remove(me)
    return len(sel)

def build_all(export=True):
    out = []
    for w in WIDTHS:
        name = 'OW_Mau_Bang_%s_%d' % (str(w).replace('.', '_'), INDEX)
        C = bpy.data.collections.get(name)
        if C is None: C = bpy.data.collections.new(name); bpy.context.scene.collection.children.link(C)
        for o in list(C.objects): bpy.data.objects.remove(o, do_unlink=True)
        petshop_sign(C, w)
        if export: out.append((name, export_coll(C, LIBDIR + '/' + name + '.fbx')))
    return out
