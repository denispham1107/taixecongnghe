# Đợt 6 (21.249): xuất bản đồ mở từ PhoViet_OW_Map.blend sang Unity → Assets/PhoViet/OpenWorld/.
# Chạy trong Blender đã dựng bản đồ (build_world_map.py) qua run_in_blender.py (TARGET='export_world_unity.py').
# Quy ước trục dự án: Unity (x, y, z) ↔ Blender (−x, −z, y). Unity đặt mọi thứ dưới gốc xoay 180° quanh Y
# nên Unity x/z = map X/Z. Mesh chưa có UV → vật liệu thủ tục được NƯỚNG thành texture lặp, Unity chiếu theo
# tọa độ thế giới (shader PhoViet/OpenWorld/Triplanar) với kích thước ô lặp ghi trong materials.json.
import bpy, os, json, math
from mathutils import Matrix

OW_DIR = globals().get('OW_DIR') or os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.normpath(os.path.join(OW_DIR, '..', '..', '..'))
OUT = os.path.join(PROJECT, 'Assets', 'PhoViet', 'OpenWorld')
EXP_PARTS = globals().get('EXP_PARTS') or ['bake', 'materials', 'library', 'chunks', 'placements', 'colliders']
for d in ('Library', 'Chunks', 'Textures', 'Data'): os.makedirs(os.path.join(OUT, d), exist_ok=True)
sc = bpy.context.scene
REPORT_LINES = []
def log(*a): REPORT_LINES.append(' '.join(str(x) for x in a)); print(*a)

# ---------------------------------------------------------------- tiện ích
def layer_coll(name, lc=None):
    lc = lc or bpy.context.view_layer.layer_collection
    if lc.collection.name == name: return lc
    for ch in lc.children:
        r = layer_coll(name, ch)
        if r: return r
    return None

def set_excluded(name, val):
    lc = layer_coll(name)
    if lc: lc.exclude = val

def all_objects(coll):
    out = list(coll.objects)
    for ch in coll.children: out += all_objects(ch)
    return out

PROC_NODES = {'TEX_NOISE', 'TEX_BRICK', 'TEX_WAVE', 'TEX_VORONOI'}
def is_procedural(m):
    return m and m.use_nodes and any(n.type in PROC_NODES for n in m.node_tree.nodes)

VERTICAL = ('OW_Facade_', 'OW_MarketWall', 'OW_TowerGrid_', 'OW_RoofBarrelTiles', 'OW_RoofTileRed', 'OW_BankStone', 'OW_Curb')
def tile_size(name):
    """(ngang, dọc) mét — bội số chu kỳ hoa văn để texture lặp không lộ mối."""
    if name.startswith('OW_TowerGrid_'): return (9.6, 9.9)
    if name == 'OW_Paving': return (4.8, 4.8)
    if name == 'OW_PavingBig': return (4.0, 4.0)
    if name == 'OW_PavingBrickRed': return (4.5, 4.5)
    if name == 'OW_RoofBarrelTiles': return (4.4, 4.16)
    return (4.0, 4.0)

# ---------------------------------------------------------------- 1. nướng texture
def used_materials():
    mats = set()
    roots = [bpy.data.collections.get('OW_Map'), bpy.data.collections.get('OW_ThuVienCoSan')]
    for r in roots:
        if not r: continue
        for o in all_objects(r):
            if o.type in ('MESH', 'FONT', 'CURVE'):
                for s in o.material_slots:
                    if s.material: mats.add(s.material)
    return sorted(mats, key=lambda m: m.name)

def bake_all():
    try: sc.render.engine = 'CYCLES'
    except TypeError as e: log('engine', e)
    sc.cycles.samples = 1
    sc.cycles.bake_type = 'DIFFUSE'
    sc.render.bake.use_pass_direct = False; sc.render.bake.use_pass_indirect = False; sc.render.bake.use_pass_color = True
    sc.render.bake.margin = 0
    baked = {}
    for m in used_materials():
        if not m.name.startswith('OW_') or not is_procedural(m) or m.name == 'OW_RiverWater': continue
        th, tv = tile_size(m.name); vert = m.name.startswith(VERTICAL)
        res = 1024 if max(th, tv) > 5 else 512
        me = bpy.data.meshes.new('OW_BakePlane')
        if vert: verts = [(0, 0, 0), (th, 0, 0), (th, 0, tv), (0, 0, tv)]
        else: verts = [(0, 0, 0), (th, 0, 0), (th, tv, 0), (0, tv, 0)]
        me.from_pydata(verts, [], [(0, 1, 2, 3)])
        uv = me.uv_layers.new(name='UV')
        for li, l in enumerate(me.loops): uv.data[li].uv = [(0, 0), (1, 0), (1, 1), (0, 1)][l.vertex_index]
        me.materials.append(m)
        ob = bpy.data.objects.new('OW_BakePlane', me); sc.collection.objects.link(ob)
        img = bpy.data.images.new('bake_' + m.name, res, res, alpha=False)
        nt = m.node_tree; tn = nt.nodes.new('ShaderNodeTexImage'); tn.image = img
        for n in nt.nodes: n.select = False
        tn.select = True; nt.nodes.active = tn
        bpy.ops.object.select_all(action='DESELECT'); ob.select_set(True); bpy.context.view_layer.objects.active = ob
        try:
            bpy.ops.object.bake(type='DIFFUSE', pass_filter={'COLOR'}, margin=0)
            path = os.path.join(OUT, 'Textures', m.name + '.png')
            img.filepath_raw = path; img.file_format = 'PNG'; img.save()
            baked[m.name] = dict(tex='Textures/' + m.name + '.png', tileH=th, tileV=tv)
        except Exception as e:
            log('bake lỗi', m.name, e)
        nt.nodes.remove(tn); bpy.data.images.remove(img)
        bpy.data.objects.remove(ob, do_unlink=True); bpy.data.meshes.remove(me)
    try: sc.render.engine = 'BLENDER_EEVEE'
    except TypeError: pass
    json.dump(baked, open(os.path.join(OUT, 'Data', 'baked.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
    log('nướng', len(baked), 'vật liệu')

# ---------------------------------------------------------------- 2. bảng vật liệu
def lin(c): return [round(float(c[0]), 4), round(float(c[1]), 4), round(float(c[2]), 4)]
def material_table():
    baked = json.load(open(os.path.join(OUT, 'Data', 'baked.json'), encoding='utf-8')) if os.path.exists(os.path.join(OUT, 'Data', 'baked.json')) else {}
    table = {}
    for m in used_materials():
        if not m.name.startswith('OW_'): continue          # vật liệu asset có sẵn (PV_StreetAtlas…) Unity đã có
        b = next((n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED'), None) if m.use_nodes else None
        e = dict(name=m.name, color=lin(m.diffuse_color), rough=0.6, metal=0.0, emit=[0, 0, 0], emitStrength=0.0)
        if b:
            bc = b.inputs['Base Color']
            if not bc.is_linked: e['color'] = lin(bc.default_value)
            e['rough'] = round(float(b.inputs['Roughness'].default_value), 3)
            e['metal'] = round(float(b.inputs['Metallic'].default_value), 3)
            es = float(b.inputs['Emission Strength'].default_value)
            if es > 0:
                ec = b.inputs['Emission Color']
                col = ec.default_value
                if ec.is_linked:   # kính tiệm: lấy màu sáng của dải màu
                    ramp = next((n for n in m.node_tree.nodes if n.type == 'VALTORGB'), None)
                    if ramp: col = ramp.color_ramp.elements[-1].color
                e['emit'] = lin(col); e['emitStrength'] = round(es, 3)
        if m.name in baked: e.update(baked[m.name]); e['color'] = [1, 1, 1]      # màu nằm trong texture nướng
        if m.name == 'OW_RiverWater': e['water'] = True
        table[m.name] = e
    json.dump(dict(materials=list(table.values())), open(os.path.join(OUT, 'Data', 'materials.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
    log('bảng vật liệu', len(table))

# ---------------------------------------------------------------- 3–4. xuất FBX
def font_to_mesh(ob):
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
    n = bpy.data.objects.new(ob.name + '_Mesh', me)
    for c in ob.users_collection: c.objects.link(n)
    n.parent = ob.parent; n.matrix_parent_inverse = ob.matrix_parent_inverse.copy(); n.matrix_basis = ob.matrix_basis.copy()
    return n

def export_fbx(objs, path):
    temp = []
    sel = []
    for o in objs:
        if o.type == 'FONT':
            t = font_to_mesh(o); temp.append(t); sel.append(t)
        elif o.type in ('MESH', 'EMPTY') and not (o.type == 'EMPTY' and o.instance_type == 'COLLECTION'):
            sel.append(o)
    bpy.ops.object.select_all(action='DESELECT')
    for o in sel:
        try: o.select_set(True)
        except RuntimeError: pass
    meshes = [o for o in sel if o.type == 'MESH']
    if meshes: bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'EMPTY', 'MESH'}, axis_forward='-Z', axis_up='Y',
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False,
                             use_mesh_modifiers=True, mesh_smooth_type='FACE', path_mode='STRIP')
    for t in temp:
        me = t.data; bpy.data.objects.remove(t, do_unlink=True); bpy.data.meshes.remove(me)
    return len(meshes)

def export_library():
    set_excluded('OW_ThuVienCoSan', False)
    n = 0
    for c in bpy.data.collections:
        if c.name.startswith('OW_Mau_') or c.name == 'OW_Src_Planter':
            set_excluded(c.name, False)
            name = 'PV_ChauCayLon' if c.name == 'OW_Src_Planter' else c.name
            export_fbx(all_objects(c), os.path.join(OUT, 'Library', name + '.fbx')); n += 1
    set_excluded('OW_ThuVienCoSan', True)
    log('thư viện', n, 'FBX')

CHUNKS = ['ground', 'roads', 'river', 'bridges', 'showcase', 'blocks', 'street']
def export_chunks():
    for k in CHUNKS:
        c = bpy.data.collections.get('OW_Map_' + k)
        if not c: continue
        cnt = export_fbx(all_objects(c), os.path.join(OUT, 'Chunks', 'OW_Map_' + k + '.fbx'))
        log('khối', k, cnt, 'mesh')

# ---------------------------------------------------------------- 5. vị trí instance
def asset_key(coll):
    n = coll.name
    if n.startswith('OW_Mau_'): return 'Library/' + n
    if n == 'OW_Src_Planter': return 'Library/PV_ChauCayLon'
    if n.startswith('OW_Src_Tree'): return 'Tree' + n[-1]
    if n == 'OW_Src_FishingBoat': return 'FishingBoat'
    return 'Unknown/' + n

def export_placements():
    items = []
    for o in all_objects(bpy.data.collections['OW_Map']):
        if o.type != 'EMPTY' or o.instance_type != 'COLLECTION' or not o.instance_collection: continue
        mw = o.matrix_world; loc = mw.to_translation(); rot = mw.to_euler(); s = mw.to_scale()
        items.append(dict(a=asset_key(o.instance_collection), p=[round(loc.x, 3), round(loc.y, 3), round(loc.z, 3)], yaw=round(math.degrees(rot.z), 2), s=round(s.x, 4)))
    json.dump(dict(note='Tọa độ Blender (x = map X, y = map Z, z = lên); yaw độ quanh trục z Blender. Unity: con của gốc xoay 180°, vị trí cục bộ (−x, z, −y), yaw cục bộ = −yaw.', items=items),
              open(os.path.join(OUT, 'Data', 'placements.json'), 'w', encoding='utf-8'), ensure_ascii=False)
    from collections import Counter
    log('instance', len(items), dict(Counter(i['a'].split('/')[0] for i in items)))

# ---------------------------------------------------------------- 6. va chạm
def export_colliders():
    land = [(-600, -600, 234.5, 228.5), (400, -600, 700, 700), (-600, 400, 400, 700),
            (-1400, -1400, 234.5, -600), (400, -1400, 2100, -600), (-1400, 700, 2100, 2100), (700, -600, 2100, 700), (-1400, -600, -600, 228.5), (-1400, 400, -600, 700)]
    decks = [(229.0, 203.6, 406.0, 226.4), (-161.4, 223.5, -138.6, 405.0)]
    R = lambda r: dict(x0=r[0], z0=r[1], x1=r[2], z1=r[3])        # dạng đối tượng để Unity JsonUtility đọc được
    land = [R(r) for r in land]; decks = [R(r) for r in decks]
    solids = [dict(name='ChoBenThanh', x0=-372, z0=-130, x1=-242, z1=-64, top=12.0)]
    json.dump(dict(note='Hình chữ nhật mặt bằng (x0, z0, x1, z1) theo tọa độ bản đồ; mặt trên ở y = 0.', land=land, bridgeDecks=decks, solids=solids,
                   meshColliderChunks=['OW_Map_blocks', 'OW_Map_bridges', 'OW_Map_showcase'], spawn=dict(p=[218.5, 0.3, -400.0], yaw=0.0)),
              open(os.path.join(OUT, 'Data', 'colliders.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
    log('va chạm', len(land), 'mảng đất,', len(decks), 'mặt cầu')

STEPS = [('bake', bake_all), ('materials', material_table), ('library', export_library), ('chunks', export_chunks),
         ('placements', export_placements), ('colliders', export_colliders)]
for k, fn in STEPS:
    if k in EXP_PARTS: fn()
