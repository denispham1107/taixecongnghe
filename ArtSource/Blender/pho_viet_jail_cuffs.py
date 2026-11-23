"""21.350 — còng tay xích to cho nhân vật trong ảnh phòng giam.
Chạy nền:  blender -b ArtSource/Blender/PhoViet_Jail.blend --python ArtSource/Blender/pho_viet_jail_cuffs.py -- <thư mục ra> [save]
Không ghi đè PhoViet_Jail.blend: khi có 'save' thì lưu bản mới PhoViet_Jail_CongTay.blend.
Đưa hai cổ tay về sát nhau trước bụng (xoay xương ForeArm/Hand), đặt 2 vòng còng ôm vừa cổ tay (đo bán kính trên lưới) + ổ khóa + xích 4 mắt nối ngang giữa hai còng (21.350b theo nét người dùng vẽ)."""
import bpy, bmesh, math, sys
from mathutils import Vector, Matrix, Quaternion

argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
OUT = argv[0] if argv else '.'
SAVE = 'save' in argv
ROOT = r"C:\Users\HP\Documents\GameUnity\Taixecongnghe"
scene = bpy.data.scenes['Pho Viet - Trai tam giam']
bpy.context.window.scene = scene if bpy.context.window else scene
arm = next(o for o in scene.objects if o.type == 'ARMATURE')

def pb(n): return arm.pose.bones['smartrig:' + n]

def aim(name, direction):
    bone = pb(name)
    cur = arm.matrix_world @ bone.matrix
    rot = cur.to_quaternion()
    rot = (rot @ Vector((0, 1, 0))).rotation_difference(Vector(direction).normalized()) @ rot
    m = rot.to_matrix().to_4x4(); m.translation = cur.translation
    bone.matrix = arm.matrix_world.inverted() @ m
    bpy.context.view_layer.update()

def head(n): return arm.matrix_world @ pb(n).head
def tail(n): return arm.matrix_world @ pb(n).tail

CX = (head('LeftHand').x + head('RightHand').x) / 2
GAP = 0.085            # nửa khoảng cách hai cổ tay (m)
WY, WZ = 0.43, 0.655   # cổ tay đặt trước bụng, trên đùi
for side, sign in [('Left', 1), ('Right', -1)]:
    elbow = head(side + 'ForeArm')
    target = Vector((CX + sign * GAP, WY, WZ))
    aim(side + 'ForeArm', target - elbow)
    aim(side + 'Hand', (-sign * .18, -.80, -.55))

steel = bpy.data.materials.new('Thép còng tay')
steel.use_nodes = True
bs = next(n for n in steel.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
bs.inputs['Base Color'].default_value = (.80, .82, .85, 1)
bs.inputs['Metallic'].default_value = .75   # EEVEE không có probe → kim loại 1,0 phản chiếu phòng tối thành đen
bs.inputs['Roughness'].default_value = .3
dark = bpy.data.materials.new('Thép bản lề tối')
dark.use_nodes = True
bd = next(n for n in dark.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
bd.inputs['Base Color'].default_value = (.30, .31, .34, 1)
bd.inputs['Metallic'].default_value = .6
bd.inputs['Roughness'].default_value = .38

col = bpy.data.collections.new('Còng tay xích 21.350')
scene.collection.children.link(col)

def torus(name, R, r, mat, mtx, seg=40, rseg=14):
    me = bpy.data.meshes.new(name); bm = bmesh.new()
    bmesh.ops.create_circle(bm, segments=rseg, radius=r)  # placeholder to keep API simple
    bm.free()
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, major_segments=seg, minor_segments=rseg)
    o = bpy.context.active_object; o.name = name
    for c in o.users_collection: c.objects.unlink(o)
    col.objects.link(o)
    o.matrix_world = mtx
    o.data.materials.append(mat)
    for p in o.data.polygons: p.use_smooth = True
    return o

def box(name, size, mat, mtx):
    bpy.ops.mesh.primitive_cube_add(size=1)
    o = bpy.context.active_object; o.name = name
    for c in o.users_collection: c.objects.unlink(o)
    col.objects.link(o)
    o.scale = size; bpy.ops.object.transform_apply(scale=True) if False else None
    o.matrix_world = mtx @ Matrix.Diagonal((*size, 1))
    o.data.materials.append(mat)
    bev = o.modifiers.new('Bevel', 'BEVEL'); bev.width = .003; bev.segments = 2
    return o

def frame(origin, z_axis, x_hint):
    z = z_axis.normalized(); x = (x_hint - x_hint.dot(z) * z).normalized(); y = z.cross(x)
    m = Matrix((x, y, z)).transposed().to_4x4(); m.translation = origin
    return m

def wrist_radius(side, c, axis):
    """Bán kính cổ tay đo trên lưới đã biến dạng: điểm trong lát dày 2 cm quanh tâm còng, khoảng cách tới trục cẳng tay."""
    actor = next(o for o in scene.objects if o.type == 'MESH' and o.find_armature() == arm)
    dg = bpy.context.evaluated_depsgraph_get(); ev = actor.evaluated_get(dg); me = ev.to_mesh()
    mw = actor.matrix_world; ds = []
    for v in me.vertices:
        p = mw @ v.co; d = p - c; t = d.dot(axis)
        if abs(t) < .006:
            r = (d - t * axis).length
            if r < .06: ds.append(r)
    ev.to_mesh_clear(); ds.sort()
    return ds[int(len(ds) * .7)] if ds else .03   # phân vị 70 %: bỏ điểm mu bàn tay/ống tay lọt vào lát đo

cuffs = {}
for side, sign in [('Left', 1), ('Right', -1)]:
    fa_h, fa_t = head(side + 'ForeArm'), tail(side + 'ForeArm')
    axis = (fa_t - fa_h).normalized()
    c = fa_t - axis * .022                     # vòng còng ngay trên khớp cổ tay
    inward = Vector((-sign, 0, 0))
    m = frame(c, axis, inward)
    wr = wrist_radius(side, c, axis)
    R, r = wr + .003 + .0055, .0055            # ôm sát cổ tay: khe 3 mm + nửa dày bản thép 11 mm
    print('WRIST', side, round(wr, 4), 'cuff R', round(R, 4))
    torus(side + ' cuff ring', R, r, steel, m, seg=48, rseg=12)
    torus(side + ' cuff inner band', R - .004, .0035, dark, m, seg=48, rseg=8)
    # ổ khóa nhỏ phía trong (hướng về tay kia), ở nửa trên vòng — chỗ móc xích
    up_in = (inward * .8 + Vector((0, 0, .6))).normalized()
    lock = c + up_in * (R + .008)
    box(side + ' cuff lock', (.020, .018, .016), dark, frame(lock, axis, up_in))
    cuffs[side] = c + up_in * (R + .019)       # khoen nối xích

# xích: 4 mắt to nối ngang giữa hai ổ khóa (nét xanh lá của người dùng), võng rất nhẹ
a, b = cuffs['Left'], cuffs['Right']
n = 4
for i in range(n):
    t0, t1 = i / n, (i + 1) / n
    p0 = a.lerp(b, t0) + Vector((0, 0, -.012 * math.sin(math.pi * t0)))
    p1 = a.lerp(b, t1) + Vector((0, 0, -.012 * math.sin(math.pi * t1)))
    mid, d = (p0 + p1) / 2, (p1 - p0)
    L = d.length
    up = Vector((0, 0, 1)) if i % 2 == 0 else Vector((0, 1, 0))
    m = frame(mid, up.cross(d).normalized() if up.cross(d).length > 1e-6 else Vector((0, -1, 0)), d)
    o = torus('Chain link %d' % i, .5, .2, steel, Matrix.Identity(4), seg=28, rseg=10)
    sx = 1.38 * L / 1.4; sy = .6 * sx      # mắt xích dài hơn bước 38 % để móc chồng vào nhau
    o.matrix_world = m @ Matrix.Diagonal((sx, sy, sy, 1))
    o.data.materials.clear(); o.data.materials.append(steel)
print('CHAIN span', round((b - a).length, 4))

def render(cam, w, h, path):
    scene.camera = bpy.data.objects[cam]
    scene.render.resolution_x, scene.render.resolution_y = w, h
    scene.render.image_settings.file_format = 'PNG'
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)

import os
render('Desktop jail shot', 1440, 900, os.path.join(OUT, 'cuffs_wide.png'))
render('Portrait jail shot', 810, 1200, os.path.join(OUT, 'cuffs_tall.png'))
scene.camera = bpy.data.objects['Desktop jail shot']; scene.render.resolution_x, scene.render.resolution_y = 1440, 900
if SAVE:
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT, 'ArtSource', 'Blender', 'PhoViet_Jail_CongTay.blend'), copy=True)
print('DONE', CX)
