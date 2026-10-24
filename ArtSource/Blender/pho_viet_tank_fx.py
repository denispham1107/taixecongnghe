"""Hiệu ứng xe tăng bắn đạn (25/10/2026, huongdan.md 21.38) — dựng và render trong Blender MCP.
exec file này trong Blender đang mở rồi gọi build_tank_fx(). Dựng trong scene riêng `PV_TankFx`
(không đụng scene khác), lưu nguồn PhoViet_TankFx.blend bằng bpy.data.libraries.write.

Xuất vào Assets/PhoViet/Art/TankFx/:
- PV_TankShell.fbx: viên đạn pháo (đầu nhọn thép, đai đồng, đuôi vạch sáng), 3 LOD, atlas phố.
- PV_TankFx_Muzzle.png: 4 khung (2×2) lửa đầu nòng — chùm tia lửa sao + lõi trắng + khói, nền trong suốt.
- PV_TankFx_Explosion.png: 8 khung (4×2) vụ nổ — lõi trắng-vàng, cầu lửa cam xoáy, khói đen lan, mảnh vụn.
- PV_TankFx_Warning.png: vòng cảnh báo đỏ nhìn từ trên (vòng kép, 4 mũi tên chỉ vào tâm, tâm ngắm).
Khung hình render EEVEE nền trong suốt, máy ảnh trực giao.
"""
import bpy, bmesh, math, os, json, random
import numpy as np
from math import sin, cos, pi
from mathutils import Vector

ROOT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
exec(compile(open(ROOT+'/ArtSource/Blender/pho_viet_saigon_streets.py', encoding='utf8').read(), 'pho_viet_saigon_streets.py', 'exec'))
OUT = ROOT+'/Assets/PhoViet/Art/TankFx'
SOURCE = ROOT+'/ArtSource/Blender/PhoViet_TankFx.blend'
SCENE = 'PV_TankFx'
FRAME = 256

def fx_scene():
    sc = bpy.data.scenes.get(SCENE) or bpy.data.scenes.new(SCENE)
    bpy.context.window.scene = sc
    coll = bpy.data.collections.get(SCENE) or bpy.data.collections.new(SCENE)
    if coll.name not in sc.collection.children: sc.collection.children.link(coll)
    for o in list(coll.objects):
        data = o.data; bpy.data.objects.remove(o, do_unlink=True)
        if isinstance(data, bpy.types.Mesh) and data.users == 0: bpy.data.meshes.remove(data)
    engines = [i.identifier for i in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items]
    sc.render.engine = next((e for e in ('BLENDER_EEVEE_NEXT', 'BLENDER_EEVEE') if e in engines), engines[0])
    sc.render.film_transparent = True
    sc.render.resolution_x = sc.render.resolution_y = FRAME; sc.render.resolution_percentage = 100
    sc.render.image_settings.file_format = 'PNG'; sc.render.image_settings.color_mode = 'RGBA'
    try: sc.view_settings.view_transform = 'Standard'
    except TypeError: pass
    if sc.world is None: sc.world = bpy.data.worlds.new(SCENE+'_World')
    sc.world.use_nodes = True
    bg = next(n for n in sc.world.node_tree.nodes if n.type == 'BACKGROUND'); bg.inputs['Strength'].default_value = 0.0
    cam = bpy.data.objects.get('PV_TankFxCam') or bpy.data.objects.new('PV_TankFxCam', bpy.data.cameras.new('PV_TankFxCam'))
    if cam.name not in coll.objects: coll.objects.link(cam)
    cam.data.type = 'ORTHO'; sc.camera = cam
    return sc, coll, cam

def emit_mat(name, color, strength, alpha=1.0, noise=None):
    """Vật liệu phát sáng; noise=(scale, detail) thì độ sáng/alpha loang theo nhiễu (lửa xoáy, khói)."""
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name); m.use_nodes = True
    nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new('ShaderNodeOutputMaterial'); em = nt.nodes.new('ShaderNodeEmission')
    em.inputs['Color'].default_value = color; em.inputs['Strength'].default_value = strength
    mix = nt.nodes.new('ShaderNodeMixShader'); tr = nt.nodes.new('ShaderNodeBsdfTransparent')
    if noise:
        tex = nt.nodes.new('ShaderNodeTexNoise'); tex.inputs['Scale'].default_value = noise[0]; tex.inputs['Detail'].default_value = noise[1]
        ramp = nt.nodes.new('ShaderNodeValToRGB'); ramp.color_ramp.elements[0].position = .35; ramp.color_ramp.elements[1].position = .7
        nt.links.new(tex.outputs['Fac'], ramp.inputs['Fac'])
        mathn = nt.nodes.new('ShaderNodeMath'); mathn.operation = 'MULTIPLY'; mathn.inputs[1].default_value = alpha
        nt.links.new(ramp.outputs['Color'], mathn.inputs[0]); nt.links.new(mathn.outputs[0], mix.inputs['Fac'])
    else:
        mix.inputs['Fac'].default_value = alpha
    nt.links.new(tr.outputs[0], mix.inputs[1]); nt.links.new(em.outputs[0], mix.inputs[2]); nt.links.new(mix.outputs[0], out.inputs['Surface'])
    try: m.surface_render_method = 'BLENDED'
    except Exception: pass
    return m

def blob_obj(coll, name, center, radius, seed, mat, rough=.35, subdiv=3):
    """Khối cầu lửa/khói gồ ghề (icosphere + dịch đỉnh nhiễu)."""
    bm = bmesh.new(); bmesh.ops.create_icosphere(bm, subdivisions=subdiv, radius=1.0)
    r = random.Random(seed)
    phases = [(r.uniform(0, 6.28), r.uniform(0, 6.28), r.uniform(1.5, 3.5)) for _ in range(4)]
    for v in bm.verts:
        n = v.co.normalized(); d = 0
        for a, b, f in phases: d += sin(n.x*f+a)*cos(n.y*f+b)*sin(n.z*f+a*.7)
        v.co = n*(1+rough*d/len(phases)*2)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = True
    ob = bpy.data.objects.new(name, me); coll.objects.link(ob); ob.location = center; ob.scale = (radius,)*3
    me.materials.append(mat); return ob

def render_frames(sc, coll, cam, frames, build, path, cols, rows, ortho):
    """Gọi build(i, t) cho từng khung, render, ghép thành atlas cols×rows."""
    cam.data.ortho_scale = ortho; cam.location = (0, -20, 0); cam.rotation_euler = (pi/2, 0, 0)
    atlas = np.zeros((rows*FRAME, cols*FRAME, 4), dtype=np.float32); tmp = OUT+'/_frame.png'
    for i in range(frames):
        for o in [o for o in coll.objects if o != cam]:
            data = o.data; bpy.data.objects.remove(o, do_unlink=True)
            if isinstance(data, bpy.types.Mesh) and data.users == 0: bpy.data.meshes.remove(data)
        build(i, i/max(1, frames-1))
        sc.render.filepath = tmp; bpy.ops.render.render(write_still=True, scene=sc.name)
        img = bpy.data.images.load(tmp); px = np.array(img.pixels[:], dtype=np.float32).reshape(FRAME, FRAME, 4); bpy.data.images.remove(img)
        cx, cy = i % cols, rows-1-i//cols   # pixels của Blender bắt đầu từ dưới
        atlas[cy*FRAME:(cy+1)*FRAME, cx*FRAME:(cx+1)*FRAME] = px
    os.remove(tmp)
    im = bpy.data.images.new(os.path.basename(path), cols*FRAME, rows*FRAME, alpha=True)
    im.pixels.foreach_set(atlas.ravel()); im.filepath_raw = path; im.file_format = 'PNG'; im.save(); bpy.data.images.remove(im)

# ---------------------------------------------------------------- lửa đầu nòng
def muzzle(coll):
    core = emit_mat('PV_FxMuzzleCore', (1, .9, .55, 1), 2.2)
    fire = emit_mat('PV_FxMuzzleFire', (1, .45, .08, 1), 1.6, 1.0, (6, 4))
    smoke = emit_mat('PV_FxMuzzleSmoke', (.32, .3, .28, 1), .6, .75, (4, 6))
    def build(i, t):
        r = random.Random(11+i); s = 1-t*.55
        # Chùm tia lửa hình sao (nhìn dọc nòng): các nón nhọn tỏa ra.
        for k in range(9):
            a = k*2*pi/9+r.uniform(-.15, .15); ln = (2.6+r.uniform(0, 1.2))*s
            bm = bmesh.new(); bmesh.ops.create_cone(bm, cap_ends=True, segments=6, radius1=.42*s, radius2=0, depth=ln)
            me = bpy.data.meshes.new('ray'); bm.to_mesh(me); bm.free(); ob = bpy.data.objects.new('ray', me); coll.objects.link(ob)
            ob.rotation_euler = (0, a+pi/2, 0); ob.location = (cos(a)*ln*.5, 0, sin(a)*ln*.5); me.materials.append(fire if k % 3 else core)
        blob_obj(coll, 'core', (0, 0, 0), 1.25*s, 7+i, core, .25)
        blob_obj(coll, 'flame', (0, .2, 0), 1.9*s, 9+i, fire, .45)
        if t > .3: blob_obj(coll, 'smoke', (0, .4, .3), 1.7*(.6+t), 13+i, smoke, .5)
    return build

# ---------------------------------------------------------------- vụ nổ
def explosion(coll):
    core = emit_mat('PV_FxBlastCore', (1, .88, .5, 1), 2.4)
    fire = emit_mat('PV_FxBlastFire', (1, .38, .05, 1), 1.7, 1.0, (3.5, 5))
    ember = emit_mat('PV_FxBlastEmber', (.8, .16, .03, 1), 1.2, .9, (5, 4))
    smoke = emit_mat('PV_FxBlastSmoke', (.16, .14, .13, 1), .35, .9, (2.5, 6))
    debris = emit_mat('PV_FxBlastDebris', (1, .6, .2, 1), 2)
    def build(i, t):
        r = random.Random(41+i)
        grow = .95+t*1.15; heat = max(0, 1-t*1.35)
        if heat > .15: blob_obj(coll, 'core', (0, -.5, 0), 1.6*grow*heat, 5, core, .3)
        if t < .85: blob_obj(coll, 'fire', (0, 0, .2+t*1.2), 2.6*grow*(1.1-t*.5), 8, fire if t < .45 else ember, .55)
        for k in range(5):   # cụm lửa/khói phụ cuộn lên
            a = k*1.26+t; rad = 2.2*grow
            c = (cos(a)*rad*.62, .3, sin(a)*rad*.4+t*1.5)
            blob_obj(coll, 'puff', c, (1.1+.5*k % 3)*grow*.6, 20+k, smoke if t > .35 or k % 2 else ember, .5, 2)
        if t > .25: blob_obj(coll, 'smoke', (0, .6, .9+t*1.6), 2.9*grow*.8, 3, smoke, .6)
        if t < .6:   # mảnh vụn bắn ra
            for k in range(14):
                a = r.uniform(0, 2*pi); d = (1.5+r.uniform(0, 3.5))*(t+.15)*3
                bm = bmesh.new(); bmesh.ops.create_cube(bm, size=.22); me = bpy.data.meshes.new('d'); bm.to_mesh(me); bm.free()
                ob = bpy.data.objects.new('d', me); coll.objects.link(ob); ob.location = (cos(a)*d, -1, sin(a)*d*.8+d*.3)
                ob.rotation_euler = (r.random()*3, r.random()*3, r.random()*3); me.materials.append(debris)
    return build

# ---------------------------------------------------------------- vòng cảnh báo
def warning_texture(sc, coll, cam):
    red = emit_mat('PV_FxWarnRed', (1, .08, .04, 1), 1.1)
    white = emit_mat('PV_FxWarnWhite', (1, .95, .9, 1), 1.0)
    def ring(r0, r1, mat, seg=96, gaps=0):
        bm = bmesh.new(); verts = []
        for k in range(seg+1):
            a = k*2*pi/seg; verts.append((bm.verts.new((cos(a)*r0, 0, sin(a)*r0)), bm.verts.new((cos(a)*r1, 0, sin(a)*r1))))
        for k in range(seg):
            if gaps and (k*gaps//seg) % 2: continue
            bm.faces.new((verts[k][0], verts[k][1], verts[k+1][1], verts[k+1][0]))
        me = bpy.data.meshes.new('ring'); bm.to_mesh(me); bm.free(); ob = bpy.data.objects.new('ring', me); coll.objects.link(ob); me.materials.append(mat)
    def build(i, t):
        ring(.92, 1.0, red); ring(.78, .84, red, gaps=24); ring(.12, .17, white)
        for k in range(4):   # 4 mũi tên tam giác chỉ vào tâm
            a = k*pi/2+pi/4; c = Vector((cos(a), 0, sin(a)))
            tip = c*.42; base = c*.68; side = Vector((-c.z, 0, c.x))*.14
            bm = bmesh.new(); vs = [bm.verts.new(tip), bm.verts.new(base+side), bm.verts.new(base-side)]; bm.faces.new(vs)
            me = bpy.data.meshes.new('arrow'); bm.to_mesh(me); bm.free(); ob = bpy.data.objects.new('arrow', me); coll.objects.link(ob); me.materials.append(red)
        for k in range(4):   # tâm ngắm
            a = k*pi/2; c = Vector((cos(a), 0, sin(a))); side = Vector((-c.z, 0, c.x))*.025
            bm = bmesh.new(); p0, p1 = c*.22, c*.38
            bm.faces.new([bm.verts.new(p0+side), bm.verts.new(p1+side), bm.verts.new(p1-side), bm.verts.new(p0-side)])
            me = bpy.data.meshes.new('tick'); bm.to_mesh(me); bm.free(); ob = bpy.data.objects.new('tick', me); coll.objects.link(ob); me.materials.append(white)
    render_frames(sc, coll, cam, 1, build, OUT+'/PV_TankFx_Warning.png', 1, 1, 2.05)

# ---------------------------------------------------------------- viên đạn
def shell(d):
    """Đạn pháo dài 0,9 m (phóng to để dễ thấy khi bay), trục +z là hướng bay."""
    g = Geo(d); n = (12, 8, 6)[d]
    g.cyl((0, 0, -.42), (0, 0, .12), .11, 'ironGrey', n=n)                 # thân
    g.cyl((0, 0, .12), (0, 0, .3), .11, 'steel', n=n, r2=.07, caps=False)   # vai thuôn
    g.cyl((0, 0, .3), (0, 0, .46), .07, 'steel', n=n, r2=.012, caps=False)  # đầu nhọn
    g.cyl((0, 0, -.2), (0, 0, -.12), .118, 'woodLight', n=n)                # đai đồng dẫn hướng
    g.cyl((0, 0, -.46), (0, 0, -.42), .09, 'lamp', n=n)                      # đuôi vạch sáng
    if d < 2:
        for k in range(4): g.cube((cos(k*pi/2)*.1, sin(k*pi/2)*.1, -.36), (.03, .03, .12), 'iron')
    g.meta = dict(length=.92, radius=.11, forward='+z')
    return g

def export_shell():
    global MAT
    MAT = bpy.data.materials.get('PV_StreetAtlas')
    sc = bpy.data.scenes[SCENE]; coll = bpy.data.collections[SCENE]
    root = bpy.data.objects.new('PV_TankShell', None); coll.objects.link(root); objs = []
    for d in range(3):
        g = shell(d); ob = to_mesh('PV_TankShell_LOD'+str(d), g, coll); ob.parent = root; objs.append(ob)
    bpy.ops.object.select_all(action='DESELECT'); root.select_set(True)
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.export_scene.fbx(filepath=OUT+'/PV_TankShell.fbx', use_selection=True, object_types={'EMPTY', 'MESH'}, axis_forward='-Z', axis_up='Y',
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False, use_mesh_modifiers=True, mesh_smooth_type='FACE')
    counts = []
    for o in objs: o.data.calc_loop_triangles(); counts.append(len(o.data.loop_triangles))
    return counts

def build_tank_fx():
    os.makedirs(OUT, exist_ok=True)
    sc, coll, cam = fx_scene()
    render_frames(sc, coll, cam, 4, muzzle(coll), OUT+'/PV_TankFx_Muzzle.png', 2, 2, 7.5)
    render_frames(sc, coll, cam, 8, explosion(coll), OUT+'/PV_TankFx_Explosion.png', 4, 2, 17)
    warning_texture(sc, coll, cam)
    for o in [o for o in coll.objects if o != cam]: bpy.data.objects.remove(o, do_unlink=True)
    tris = export_shell()
    info = dict(generator='pho_viet_tank_fx.py', shell_lod_triangles=tris, muzzle=dict(file='PV_TankFx_Muzzle.png', grid=[2, 2]),
                explosion=dict(file='PV_TankFx_Explosion.png', grid=[4, 2]), warning='PV_TankFx_Warning.png', frame=FRAME)
    with open(OUT+'/PV_TankFx.json', 'w', encoding='utf8') as f: json.dump(info, f, ensure_ascii=False, indent=2)
    mats = {m for m in bpy.data.materials if m.name.startswith('PV_Fx')}
    ids = {sc, coll}|set(coll.objects)|{o.data for o in coll.objects if o.data}|mats
    if sc.world: ids.add(sc.world)
    bpy.data.libraries.write(SOURCE, ids, fake_user=True)
    return dict(success=True, engine=sc.render.engine, shell=tris)
