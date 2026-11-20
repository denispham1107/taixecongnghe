# 21.290 — chữ vẽ trên các mảng tường hông trống ở giao lộ (người dùng 21/11/2026). Chạy NỀN:
#   blender -b --factory-startup --python build_wall_texts.py
# Ra Assets/PhoViet/OpenWorld/Library/OW_ChuTuong.fbx: 7 lưới chữ 3D (font Arial Bold — đủ dấu tiếng Việt, như biển hiệu),
# mỗi lưới 2 slot vật liệu: 0 = OW_ChuTuong_Chinh (dòng chính), 1 = OW_ChuTuong_Nhan (dòng nhấn: giá / chữ CẤM…) để Unity tô
# hai màu khác nhau. Chữ đứng trong mặt phẳng XZ, đọc được từ phía −Y (Blender), tâm khối chữ ở gốc, dòng chính cao ~1 m.
# Unity (Editor/PhoVietOpenWorldWallTextBuilder.cs, menu 15) co giãn + đặt giữa vùng tường trống.
import bpy, math, os, unicodedata
ROOT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT = ROOT + '/Assets/PhoViet/OpenWorld/Library/OW_ChuTuong.fbx'
FONT = 'C:/Windows/Fonts/arialbd.ttf'
# (tên, [(dòng, cỡ chữ, slot)]) — nội dung đúng như người dùng yêu cầu, xuống dòng cho dễ đọc trên tường
PHRASES = [
    ('OW_ChuTuong_P1', [('Cù Lao Petshop', 1.0, 0), ('Tắm Làm Đẹp Thú Cưng', 0.78, 0), ('66K', 1.5, 1)]),
    ('OW_ChuTuong_P2', [('Cù Lao Petshop', 1.0, 0), ('Lưu Trú Thú Cưng', 0.82, 0), ('66K', 1.5, 1)]),
    ('OW_ChuTuong_P3', [('Cù Lao Petshop', 1.0, 0), ('Hỏa Táng Thú Cưng', 0.82, 0), ('300K', 1.5, 1)]),
    ('OW_ChuTuong_S1', [('CẤM', 1.5, 1), ('ĐÁI BẬY', 1.2, 0)]),
    ('OW_ChuTuong_S2', [('AI XE ÔM', 1.2, 0), ('KHÔNG', 1.5, 1)]),
    ('OW_ChuTuong_S3', [('CẤM', 1.5, 1), ('ĐỔ RÁC', 1.2, 0)]),
    ('OW_ChuTuong_S4', [('HÚT BỒN CẦU', 1.2, 0), ('ĐÂY', 1.5, 1)]),
    # bố cục DỌC (hẹp, nhiều dòng) cho vùng tường hẹp mà cao — Unity chọn bố cục cho chữ to nhất
    ('OW_ChuTuong_P1N', [('Cù Lao', 1.0, 0), ('Petshop', 1.0, 0), ('Tắm Làm Đẹp', 0.8, 0), ('Thú Cưng', 0.8, 0), ('66K', 1.5, 1)]),
    ('OW_ChuTuong_P2N', [('Cù Lao', 1.0, 0), ('Petshop', 1.0, 0), ('Lưu Trú', 0.85, 0), ('Thú Cưng', 0.85, 0), ('66K', 1.5, 1)]),
    ('OW_ChuTuong_P3N', [('Cù Lao', 1.0, 0), ('Petshop', 1.0, 0), ('Hỏa Táng', 0.85, 0), ('Thú Cưng', 0.85, 0), ('300K', 1.5, 1)]),
    ('OW_ChuTuong_S1N', [('CẤM', 1.5, 1), ('ĐÁI', 1.2, 0), ('BẬY', 1.2, 0)]),
    ('OW_ChuTuong_S2N', [('AI', 1.2, 0), ('XE ÔM', 1.2, 0), ('KHÔNG', 1.5, 1)]),
    ('OW_ChuTuong_S3N', [('CẤM', 1.5, 1), ('ĐỔ', 1.2, 0), ('RÁC', 1.2, 0)]),
    ('OW_ChuTuong_S4N', [('HÚT', 1.2, 0), ('BỒN CẦU', 1.2, 0), ('ĐÂY', 1.5, 1)]),
]
for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
font = bpy.data.fonts.load(FONT, check_existing=True)
mats = [bpy.data.materials.new('OW_ChuTuong_Chinh'), bpy.data.materials.new('OW_ChuTuong_Nhan')]
mats[0].diffuse_color = (0.6, 0.05, 0.05, 1); mats[1].diffuse_color = (0.05, 0.1, 0.5, 1)
sc = bpy.context.scene; out = []
for name, lines in PHRASES:
    parts = []; y = 0.0
    for k, (txt, size, slot) in enumerate(lines):
        txt = unicodedata.normalize('NFC', txt)                      # ký tự dựng sẵn (không dấu tổ hợp) → font hiển thị đúng
        cu = bpy.data.curves.new(name + '_%d' % k, 'FONT'); cu.body = txt; cu.font = font; cu.size = size
        cu.extrude = 0.012; cu.resolution_u = 4; cu.align_x = 'CENTER'; cu.align_y = 'CENTER'
        cu.materials.append(mats[slot])
        ob = bpy.data.objects.new(cu.name, cu); sc.collection.objects.link(ob)
        if k: y -= (lines[k - 1][1] * 0.62 + size * 0.62)               # khoảng cách dòng theo cỡ chữ
        ob.location = (0, y, 0)
        parts.append(ob)
    # đổi sang lưới, gộp, giữ 2 slot đúng thứ tự (Chinh, Nhan)
    meshes = []
    dg = bpy.context.evaluated_depsgraph_get()
    for ob in parts:
        me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg)); mo = bpy.data.objects.new(ob.name + '_m', me)
        mo.matrix_world = ob.matrix_world; sc.collection.objects.link(mo); meshes.append(mo)
        bpy.data.objects.remove(ob, do_unlink=True)
    for mo in meshes:
        me = mo.data; src = me.materials[0]; slot = mats.index(src)
        me.materials.clear(); me.materials.append(mats[0]); me.materials.append(mats[1])
        for p in me.polygons: p.material_index = slot
    bpy.ops.object.select_all(action='DESELECT')
    for mo in meshes: mo.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]; bpy.ops.object.join()
    ob = bpy.context.view_layer.objects.active; ob.name = name; ob.data.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    # tâm khối chữ về gốc, dựng đứng: mặt chữ (+Z cũ) quay về −Y
    import mathutils
    bb = [mathutils.Vector(c) for c in ob.bound_box]; ctr = sum(bb, mathutils.Vector()) / 8
    ob.data.transform(mathutils.Matrix.Translation(-ctr))
    ob.data.transform(mathutils.Matrix.Rotation(math.radians(90), 4, 'X'))
    ob.location = (0, 0, 0)
    dims = ob.dimensions
    print('[chutuong]', name, 'rộng %.2f cao %.2f' % (dims.x, dims.z), 'tam giác', sum(len(p.vertices) - 2 for p in ob.data.polygons), flush=True)
    out.append(ob)
bpy.ops.object.select_all(action='DESELECT')
for o in out: o.select_set(True)
bpy.context.view_layer.objects.active = out[0]
bpy.ops.export_scene.fbx(filepath=OUT, use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y',
                         apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False,
                         mesh_smooth_type='FACE', path_mode='STRIP')
print('[chutuong] xuất', OUT, flush=True)
