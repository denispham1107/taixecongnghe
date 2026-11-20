# 21.289 — dựng lại 48 nhà mẫu OW_Mau_NhaPho_<rộng>_<0..3>[_B] bằng shophouse2 (ow_facade.py, mặt tiền shophouse hiện đại đã duyệt)
# và xuất đè Assets/PhoViet/OpenWorld/Library/*.fbx (cùng tên, cùng seed 9000–9047, cùng thông số xuất như export_world_unity.py)
# + nướng texture các vật liệu thủ tục MỚI và gộp vào Data/baked.json, Data/materials.json (không đụng mục cũ).
# Sau đó chạy build_interiors.py MODE='export_houses' (gỡ kính/tường tầng trệt cũ, gắn OW_MatTienKinh — tầng trệt như cũ).
# Chạy NỀN (không mở/lưu tệp .blend nào):
#   blender -b --factory-startup --python build_facade_library.py [-- only=4_5_0,6_0_1_B]
import bpy, os, sys, json, math, time
OW = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld'
T0 = time.time()
def log(*a): print('[mattien %5.1fs]' % (time.time() - T0), *a, flush=True)
args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
only = None
for a in args:
    if a.startswith('only='): only = set(a[5:].split(','))

ns = {'OW_DIR': OW, 'NO_RUN': True}
exec(compile(open(OW + '/build_ref3_segment.py', encoding='utf-8').read(), OW + '/build_ref3_segment.py', 'exec'), ns)
ns['mats']()
exec(compile(open(OW + '/ow_facade.py', encoding='utf-8').read(), OW + '/ow_facade.py', 'exec'), ns)
ns['facade_mats']()
ex = {'OW_DIR': OW, 'EXP_PARTS': ['none']}                  # chỉ lấy hàm tiện ích (is_procedural, tile_size, lin, export_fbx…)
exec(compile(open(OW + '/export_world_unity.py', encoding='utf-8').read(), OW + '/export_world_unity.py', 'exec'), ex)
OUT = ex['OUT']; sc = bpy.context.scene
for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)

WIDTHS = [4.5, 5.0, 5.5, 6.0, 7.0, 8.0]
used = set(); seed = 9000; stats = []
for w in WIDTHS:
    for v in range(4):
        for slot in (False, True):
            name = 'OW_Mau_NhaPho_%s_%d%s' % (str(w).replace('.', '_'), v, '_B' if slot else '')
            key = name[len('OW_Mau_NhaPho_'):]
            if only is None or key in only:
                c = bpy.data.collections.new(name); sc.collection.children.link(c)
                ns['shophouse2'](c, -w / 2, w, seed, x_face=0.0, sign_slot=slot)
                obs = list(c.objects)
                for o in obs:
                    for s in o.material_slots:
                        if s.material: used.add(s.material)
                tris = sum(sum(len(p.vertices) - 2 for p in o.data.polygons) for o in obs if o.type == 'MESH')
                if slot:   # kiểm tra: không đỉnh nào thò vào vùng biển hiệu (trước tường x 0,03–0,3; cao 3,6–5,08; trong bề ngang biển)
                    bad = sum(1 for o in obs if o.type == 'MESH' for v in o.data.vertices
                              if 0.03 < v.co.x < 0.3 and 3.6 < v.co.z < 5.08 and abs(v.co.y) < w / 2 - 0.2)
                    if bad: log('LỖI vùng biển', name, bad, 'đỉnh')
                ex['export_fbx'](obs, os.path.join(OUT, 'Library', name + '.fbx'))
                stats.append((name, seed, tris))
                for o in obs: me = o.data; bpy.data.objects.remove(o, do_unlink=True); bpy.data.meshes.remove(me)
                bpy.data.collections.remove(c)
            seed += 1
for s in stats: log('mẫu', *s)

# ---- vật liệu mới: nướng (nếu thủ tục) + bảng
bpath = os.path.join(OUT, 'Data', 'baked.json'); mpath = os.path.join(OUT, 'Data', 'materials.json')
baked = json.load(open(bpath, encoding='utf-8')); table = json.load(open(mpath, encoding='utf-8'))
have = {e['name'] for e in table['materials']}
new = sorted([m for m in used if m.name.startswith('OW_') and m.name not in have], key=lambda m: m.name)
log('vật liệu mới', [m.name for m in new])
try: sc.render.engine = 'CYCLES'
except TypeError as e: log('engine', e)
sc.cycles.samples = 1
for m in new:
    if not ex['is_procedural'](m): continue
    th, tv = ex['tile_size'](m.name); vert = m.name.startswith(ex['VERTICAL'])
    res = 1024 if max(th, tv) > 5 else 512
    me = bpy.data.meshes.new('OW_BakePlane')
    verts = [(0, 0, 0), (th, 0, 0), (th, 0, tv), (0, 0, tv)] if vert else [(0, 0, 0), (th, 0, 0), (th, tv, 0), (0, tv, 0)]
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
    bpy.ops.object.bake(type='DIFFUSE', pass_filter={'COLOR'}, margin=0)
    path = os.path.join(OUT, 'Textures', m.name + '.png'); img.filepath_raw = path; img.file_format = 'PNG'; img.save()
    baked[m.name] = dict(tex='Textures/' + m.name + '.png', tileH=th, tileV=tv)
    nt.nodes.remove(tn); bpy.data.images.remove(img); bpy.data.objects.remove(ob, do_unlink=True); bpy.data.meshes.remove(me)
    log('nướng', m.name, th, tv, res)
lin = ex['lin']
for m in new:
    b = next((n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED'), None) if m.use_nodes else None
    e = dict(name=m.name, color=lin(m.diffuse_color), rough=0.6, metal=0.0, emit=[0, 0, 0], emitStrength=0.0)
    if b:
        bc = b.inputs['Base Color']
        if not bc.is_linked: e['color'] = lin(bc.default_value)
        e['rough'] = round(float(b.inputs['Roughness'].default_value), 3)
        e['metal'] = round(float(b.inputs['Metallic'].default_value), 3)
        es = float(b.inputs['Emission Strength'].default_value)
        if es > 0: e['emit'] = lin(b.inputs['Emission Color'].default_value); e['emitStrength'] = round(es, 3)
    if m.name in baked: e.update(baked[m.name]); e['color'] = [1, 1, 1]
    table['materials'].append(e)
json.dump(baked, open(bpath, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
json.dump(table, open(mpath, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
log('bảng vật liệu', len(table['materials']), 'nướng', len(baked))
