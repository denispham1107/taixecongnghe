# Phố Việt 21.288 — thay hộp cao ốc cũ bằng cao ốc thư viện ow_towers.py.
# Chạy Blender nền trên bản đồ:  blender -b PhoViet_OW_Map.blend --python ow_towers_apply_map.py
# 1) Tìm từng hộp cao ốc trong OW_Map_LoiKhoi (vật liệu OW_TowerGrid_*) + tấm mái bê tông của nó, và 70 hộp vành OW_Map_ChanTroi.
# 2) Chọn mẫu + chiều cao + tỉ lệ cho từng chỗ, ghi Assets/PhoViet/OpenWorld/Data/towers.json (toạ độ Blender: x = map X, y = map Z).
# 3) Xoá các hộp đó khỏi lưới, xoá OW_Map_ChanTroi, lưu .blend và xuất lại riêng Chunks/OW_Map_blocks.fbx (hộp trung tầng giữ nguyên).
# Chạy lần hai: không còn hộp cao ốc để tìm -> giữ towers.json cũ (không ghi đè).
import bpy, bmesh, json, math, os, random

OW_DIR = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld'
OUT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/Assets/PhoViet/OpenWorld'
models = json.load(open(OUT + '/Towers/towers_models.json', encoding='utf-8'))['models']
rnd = random.Random(21288)

# ---------------------------------------------------------------- 1. đảo lưới
def islands(ob):
    me = ob.data; mw = ob.matrix_world
    bm = bmesh.new(); bm.from_mesh(me); bm.faces.ensure_lookup_table()
    seen = set(); out = []
    for f in bm.faces:
        if f.index in seen: continue
        stack = [f]; isl = []; seen.add(f.index)
        while stack:
            g = stack.pop(); isl.append(g.index)
            for e in g.edges:
                for h in e.link_faces:
                    if h.index not in seen: seen.add(h.index); stack.append(h)
        vs = {v for i in isl for v in bm.faces[i].verts}
        co = [mw @ v.co for v in vs]
        mat = me.materials[bm.faces[isl[0]].material_index].name if me.materials else ''
        out.append(dict(faces=isl, mat=mat, x0=min(c.x for c in co), x1=max(c.x for c in co), y0=min(c.y for c in co),
                        y1=max(c.y for c in co), z0=min(c.z for c in co), z1=max(c.z for c in co)))
    return bm, out

lk = bpy.data.objects['OW_Map_LoiKhoi']
bm, isl = islands(lk)
towers = [i for i in isl if i['mat'].startswith('OW_TowerGrid')]
solids = [i for i in isl if not i['mat'].startswith('OW_TowerGrid') and i['mat'] != 'OW_Concrete']
roofs = [i for i in isl if i['mat'] == 'OW_Concrete']
ring_ob = bpy.data.objects.get('OW_Map_ChanTroi')
ring = []
if ring_ob:
    rbm, ring = islands(ring_ob); rbm.free()
print('hộp cao ốc', len(towers), 'vành', len(ring), 'trung tầng', len(solids), 'mái', len(roofs))

if towers:
    # ---------------------------------------------------------------- 2. chọn mẫu
    def zone(t):
        cx, cy = (t['x0'] + t['x1']) / 2, (t['y0'] + t['y1']) / 2
        if cx > 420: return 'east'
        if cy > 420: return 'north'
        return 'fin'
    POOLS = {'fin': ['VP_VayDoc', 'VP_VatGoc', 'VP_KhungDa', 'VP_KhoiLech', 'VP_BoTron'],
             'east': ['CH_BanCong', 'CH_ChuThap', 'CH_GocLom', 'CH_ZigZag'],
             'east_low': ['TT_ChungCu', 'TT_ChungCu', 'TT_VanPhong'],
             'north': ['TT_ChungCu', 'TT_ChungCu', 'TT_VanPhong'],
             'north_hi': ['CH_BanCong', 'CH_ZigZag'],
             'ring': ['VP_VayDoc', 'VP_VatGoc', 'VP_KhungDa', 'VP_KhoiLech', 'VP_BoTron', 'CH_BanCong', 'CH_ChuThap', 'CH_GocLom', 'CH_ZigZag']}
    by_spec = {}
    for m in models: by_spec.setdefault(m['spec'], []).append(m)

    def pick_variant(spec, h):
        best = min(by_spec[spec], key=lambda m: abs(math.log(h / m['h'])))
        return best, max(0.8, min(1.25, h / best['h']))

    others = towers + solids            # vật cản khi tính chỗ trống quanh mỗi hộp
    def room(t, d, default):
        """Khoảng trống từ mặt hộp t theo hướng d (+x, -x, +y, -y) tới hộp gần nhất cùng dải; không có -> default."""
        best = None
        for o in others:
            if o is t: continue
            if d in ('+x', '-x'):
                if o['y1'] < t['y0'] - 1 or o['y0'] > t['y1'] + 1: continue
                g = o['x0'] - t['x1'] if d == '+x' else t['x0'] - o['x1']
            else:
                if o['x1'] < t['x0'] - 1 or o['x0'] > t['x1'] + 1: continue
                g = o['y0'] - t['y1'] if d == '+y' else t['y0'] - o['y1']
            if g < -0.5: continue
            if best is None or g < best: best = g
        if best is None or best > 60: return default
        return max(0.0, best / 2 - 1.0)

    items = []; prev = None; squeezed = 0
    for t in sorted(towers, key=lambda t: (round(t['x0'] / 40), t['y0'])):
        z = zone(t); h = t['z1']
        cx, cy = (t['x0'] + t['x1']) / 2, (t['y0'] + t['y1']) / 2
        pool = POOLS[z]
        if z == 'east' and h < 45: pool = POOLS['east_low']
        if z == 'north' and h > 44: pool = POOLS['north_hi']
        cands = [p for p in pool if p != prev] or pool
        spec = rnd.choice(cands); prev = spec
        m, sz = pick_variant(spec, h)
        mx = max(abs(m['x0']), abs(m['x1'])); my = max(abs(m['y0']), abs(m['y1']))
        dflt = 1.5 if z == 'fin' else 4.0
        ax = (t['x1'] - t['x0']) / 2 + min(room(t, '+x', dflt), room(t, '-x', dflt))
        ay = (t['y1'] - t['y0']) / 2 + min(room(t, '+y', dflt), room(t, '-y', dflt))
        s0 = min(ax / mx, ay / my); s90 = min(ax / my, ay / mx)
        if s90 > s0 * 1.02: s, yaw = s90, rnd.choice([90, 270])
        else: s, yaw = s0, rnd.choice([0, 180])
        if s < 0.55: squeezed += 1
        s = max(0.55, min(1.1, s))
        items.append(dict(m=m['name'], zone=z, p=[round(cx, 2), round(cy, 2), 0.0], yaw=yaw, sxy=round(s, 3), sz=round(sz, 3),
                          old=[round(t['x1'] - t['x0'], 1), round(t['y1'] - t['y0'], 1), round(h, 1)]))
    for t in ring:
        cx, cy = (t['x0'] + t['x1']) / 2, (t['y0'] + t['y1']) / 2; h = t['z1']
        spec = rnd.choice(POOLS['ring']); m, sz = pick_variant(spec, h)
        mx = max(abs(m['x0']), abs(m['x1'])); my = max(abs(m['y0']), abs(m['y1']))
        s = max(0.9, min(1.5, (t['x1'] - t['x0']) / 2 / max(mx, my) * 1.15))
        items.append(dict(m=m['name'], zone='ring', p=[round(cx, 2), round(cy, 2), 0.0], yaw=rnd.choice([0, 90, 180, 270]),
                          sxy=round(s, 3), sz=round(max(0.8, min(1.4, h / m['h'])), 3), old=[round(t['x1'] - t['x0'], 1), round(t['y1'] - t['y0'], 1), round(h, 1)]))
    from collections import Counter
    print('chọn', len(items), Counter(i['m'].split('_')[2] + '_' + i['m'].split('_')[3] for i in items).most_common(), 'bị ép <0,55:', squeezed)
    json.dump(dict(note='21.288 — cao ốc thư viện (Assets/PhoViet/OpenWorld/Towers/Models/<m>.fbx). Toạ độ Blender: x = map X, y = map Z, yaw độ quanh trục đứng Blender. '
                        'Unity: con của gốc BanDoMo xoay 180°, vị trí cục bộ (−x, 0, −y), yaw cục bộ = −yaw, tỉ lệ (sxy, sz, sxy). old = hộp cũ (rộng x, sâu y, cao).',
                   items=items), open(OUT + '/Data/towers.json', 'w', encoding='utf-8'), ensure_ascii=False, indent=0)

    # ---------------------------------------------------------------- 3. xoá hộp cao ốc + mái
    kill = set()
    for t in towers:
        for i in t['faces']: kill.add(i)
        for r in roofs:
            if abs(r['z0'] - t['z1']) < 0.05 and r['x0'] < t['x0'] and r['x1'] > t['x1'] and r['y0'] < t['y0'] and r['y1'] > t['y1'] and r['x1'] - r['x0'] < t['x1'] - t['x0'] + 1.0:
                for i in r['faces']: kill.add(i)
                r['used'] = True
    nroof = sum(1 for r in roofs if r.get('used'))
    bm.faces.ensure_lookup_table()
    bmesh.ops.delete(bm, geom=[bm.faces[i] for i in kill], context='FACES')
    bm.to_mesh(lk.data); lk.data.update()
    print('đã xoá mặt', len(kill), 'mái khớp', nroof, '/', len(towers))
    if ring_ob:
        me = ring_ob.data; colls = list(ring_ob.users_collection)
        bpy.data.objects.remove(ring_ob, do_unlink=True); bpy.data.meshes.remove(me)
        # GIỮ một đối tượng rỗng cùng tên: FBX một mesh thì Unity gộp gốc với mesh và mất góc xoay −90° X (lưới nằm ngang, hộp lơ lửng)
        e = bpy.data.objects.new('OW_Map_ChanTroi', None)
        for cc in colls: cc.objects.link(e)
    bm.free()
    bpy.ops.wm.save_mainfile()
    # ---------------------------------------------------------------- xuất riêng chunk blocks (cùng hàm export_fbx của export_world_unity.py)
    ns = {'OW_DIR': OW_DIR, 'EXP_PARTS': ['none']}
    p = OW_DIR + '/export_world_unity.py'
    exec(compile(open(p, encoding='utf-8').read(), p, 'exec'), ns)
    c = bpy.data.collections['OW_Map_blocks']
    n = ns['export_fbx'](ns['all_objects'](c), os.path.join(ns['OUT'], 'Chunks', 'OW_Map_blocks.fbx'))
    print('xuất OW_Map_blocks.fbx', n, 'mesh', [o.name for o in ns['all_objects'](c)])
else:
    bm.free(); print('không còn hộp cao ốc — bỏ qua')
