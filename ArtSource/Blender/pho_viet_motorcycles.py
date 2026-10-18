"""Phố Việt motorcycles and riders, version 2 (near-realistic). Run inside Blender via Blender MCP.

    exec(compile(open(ROOT+'/ArtSource/Blender/pho_viet_motorcycles.py',encoding='utf-8').read(),'pho_viet_motorcycles.py','exec'),globals())
    build_motorcycles()

Authoring coordinates are metres: X = rider's right, Y = up, Z = road forward (same as
pho_viet_bikes.py, which other art scripts still use as a helper library and is not changed).
The world is authored at the game's 1.2x vehicle scale: wheel centres (0,.34,-.73)/(0,.34,.78).

Unity contract kept: one skinned mesh + _LOD1 + _LOD2, one 128 px palette atlas, 17 bones with
identical names and identity rest rotations, 7 Legacy clips (Idle, Ride, KickLeft, KickRight,
FallLeft, FallRight, Recover). Bike parts are rigid; the rider body is one continuous voxel-
remeshed surface with smooth two-bone weights computed from anatomical segments.
"""
import bpy, bmesh, math, json, os
from mathutils import Vector, Matrix, Quaternion, Euler
from mathutils.bvhtree import BVHTree
from math import sin, cos, pi, radians

ROOT = r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT = ROOT + '/Assets/PhoViet/Art/Bikes'
SOURCE = ROOT + '/ArtSource/Blender/PhoViet_Motorcycles.blend'
FRONT = Vector((0, .34, .78)); REAR = Vector((0, .34, -.73)); WHEEL_R = .335

def V(*a): return Vector(a[0] if len(a) == 1 else a)
def B(p): return Vector((-p[0], -p[2], p[1]))
TO_BLENDER = Matrix(((-1, 0, 0, 0), (0, 0, -1, 0), (0, 1, 0, 0), (0, 0, 0, 1)))
def hexcol(h): return tuple((int(h[i:i + 2], 16) / 255) ** 2.2 for i in (0, 2, 4)) + (1,)
def lerp(a, b, t): return a + (b - a) * t
def smooth01(t): t = max(0., min(1., t)); return t * t * (3 - 2 * t)

# ---------------------------------------------------------------- palette slots
SLOTS = ['paint', 'trim', 'plastic', 'rubber', 'chrome', 'alloy', 'metal', 'seat',
         'skin', 'skinShade', 'shirt', 'shirtDark', 'pants', 'pantsDark', 'shoe', 'sole',
         'star', 'helmet', 'helmetTrim', 'visor', 'lamp', 'tail', 'amber', 'plate',
         'ink', 'eyeWhite', 'iris', 'hair', 'lip', 'accent', 'khaki', 'blue']
S = {n: i for i, n in enumerate(SLOTS)}
# metallic, smoothness per slot (written into the _Surface map: R metal, A smoothness)
# Kept low on metals: the Unity scene has no reflection probes, so high metallic reads as black.
SURFACE = {'paint': (0, .82), 'trim': (.12, .6), 'plastic': (0, .34), 'rubber': (0, .12), 'chrome': (.45, .86),
           'alloy': (.3, .6), 'metal': (.25, .42), 'seat': (0, .38), 'skin': (0, .32), 'skinShade': (0, .28),
           'shirt': (0, .1), 'shirtDark': (0, .1), 'pants': (0, .12), 'pantsDark': (0, .12), 'shoe': (0, .3),
           'sole': (0, .2), 'star': (0, .18), 'helmet': (0, .78), 'helmetTrim': (0, .4), 'visor': (.2, .9),
           'lamp': (.3, .92), 'tail': (.1, .88), 'amber': (.1, .88), 'plate': (0, .45), 'ink': (0, .3),
           'eyeWhite': (0, .7), 'iris': (0, .8), 'hair': (0, .25), 'lip': (0, .35), 'accent': (0, .7),
           'khaki': (0, .12), 'blue': (0, .7)}

def slot_materials(name, colors):
    mats = []
    for i, slot in enumerate(SLOTS):
        m = bpy.data.materials.get(name + '_' + slot) or bpy.data.materials.new(name + '_' + slot)
        m.use_nodes = True; bsdf = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
        col = hexcol(colors[slot]); metal, gloss = SURFACE[slot]
        bsdf.inputs['Base Color'].default_value = col; bsdf.inputs['Metallic'].default_value = metal
        bsdf.inputs['Roughness'].default_value = 1 - gloss; m.diffuse_color = col; m.roughness = 1 - gloss; m.metallic = metal
        mats.append(m)
    return mats

def palette_material(name, colors):
    size = 128; pixels = []; masks = []
    for y in range(size):
        for x in range(size):
            idx = (y // 32) * 8 + x // 16; slot = SLOTS[idx]
            pixels.extend(hexcol(colors[slot])); metal, gloss = SURFACE[slot]; masks.extend((metal, metal, metal, gloss))
    for suffix, data, colour in [('_Palette', pixels, 'sRGB'), ('_Surface', masks, 'Non-Color')]:
        old = bpy.data.images.get(name + suffix)
        if old: bpy.data.images.remove(old)
        image = bpy.data.images.new(name + suffix, width=size, height=size, alpha=True); image.pixels = data
        image.filepath_raw = OUT + '/' + name + suffix + '.png'; image.file_format = 'PNG'; image.save()
        if colour != 'sRGB': image.colorspace_settings.name = colour  # reloads the saved file, so only after save()
    mat = bpy.data.materials.get(name + '_Atlas') or bpy.data.materials.new(name + '_Atlas'); mat.use_nodes = True
    nodes = mat.node_tree.nodes; links = mat.node_tree.links
    for n in list(nodes):
        if n.type not in ('BSDF_PRINCIPLED', 'OUTPUT_MATERIAL'): nodes.remove(n)
    bsdf = next(n for n in nodes if n.type == 'BSDF_PRINCIPLED')
    tex = nodes.new('ShaderNodeTexImage'); tex.image = bpy.data.images[name + '_Palette']; tex.interpolation = 'Closest'
    links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
    surf = nodes.new('ShaderNodeTexImage'); surf.image = bpy.data.images[name + '_Surface']; surf.interpolation = 'Closest'
    sep = nodes.new('ShaderNodeSeparateColor'); links.new(surf.outputs['Color'], sep.inputs['Color']); links.new(sep.outputs['Red'], bsdf.inputs['Metallic'])
    inv = nodes.new('ShaderNodeMath'); inv.operation = 'SUBTRACT'; inv.inputs[0].default_value = 1
    links.new(surf.outputs['Alpha'], inv.inputs[1]); links.new(inv.outputs[0], bsdf.inputs['Roughness'])
    return mat

# ---------------------------------------------------------------- geometry accumulation
class Part:
    """Geometry in authoring space. bone=None marks rider body pieces for the skinned remesh."""
    def __init__(self, bone='Frame', sub=0, sharp=40):
        self.V = []; self.F = []; self.S = []; self.bone = bone; self.sub = sub; self.sharp = sharp; self.tag = ''
    def add(self, verts, faces, slot):
        base = len(self.V); self.V.extend(Vector(v) for v in verts)
        self.F.extend(tuple(base + i for i in f) for f in faces); self.S.extend([S[slot] if isinstance(slot, str) else slot] * len(faces))
        return self

def two_sided(part, verts, faces, slot, toward, gap=.0008):
    """Open patches (decals, visors, masks, brims) get a back copy nudged toward a point, so they
    stay visible whichever way normal recalculation orients the island (Unity culls back faces)."""
    # Built as a thin closed slab (front, back and boundary walls) so normal recalculation is exact.
    t = Vector(toward); n = len(verts)
    back = [Vector(v) + (t - Vector(v)).normalized() * gap for v in verts]
    F = [tuple(f) for f in faces] + [tuple(n + i for i in reversed(f)) for f in faces]
    uses = {}
    for f in faces:
        for a, b in zip(f, f[1:] + f[:1]): uses[(a, b)] = uses.get((a, b), 0) + 1
    for (a, b) in uses:
        if (b, a) not in uses: F.append((b, a, n + a, n + b))
    part.add(list(verts) + back, F, slot)

def se(t, e):
    c, s = cos(t), sin(t)
    return math.copysign(abs(c) ** (2 / e), c), math.copysign(abs(s) ** (2 / e), s)

def grid_faces(rings, n, closed=True, cap0=True, cap1=True):
    F = []
    for j in range(rings - 1):
        for i in range(n if closed else n - 1):
            F.append((j * n + i, j * n + (i + 1) % n, (j + 1) * n + (i + 1) % n, (j + 1) * n + i))
    if closed and cap0: F.append(tuple(range(n - 1, -1, -1)))
    if closed and cap1: F.append(tuple(range((rings - 1) * n, rings * n)))
    return F

def interp_sections(sections, k):
    """Catmull-Rom between numeric section tuples, so panels curve smoothly without subdivision."""
    if k <= 1 or len(sections) < 3: return sections
    width = max(len(s) for s in sections)
    def pad(sec):
        sec = list(sec)
        if width > 5 and len(sec) < 6: sec.append(0)
        if width > 6 and len(sec) < 7: sec.append(sec[4])
        return sec
    rows = [Vector(pad(sec)) for sec in sections]; return [tuple(r) for r in catmull(rows, k)]

def loft_z(part, sections, slot, n=18, caps=(True, True), interp=2):
    """Sections (z, ybot, ytop, halfwidth, exponent[, x, exponent_bottom]) - rounded panels along Z."""
    sections = interp_sections(sections, interp); vs = []
    for sec in sections:
        z, yb, yt, w, e = sec[:5]; x0 = sec[5] if len(sec) > 5 else 0; eb = sec[6] if len(sec) > 6 else e
        cy = (yb + yt) / 2; h = (yt - yb) / 2
        for i in range(n):
            t = 2 * pi * i / n; px, py = se(t, e if sin(t) >= 0 else eb)
            vs.append((x0 + w * px, cy + h * py, z))
    return part.add(vs, grid_faces(len(sections), n, True, *caps), slot)

def loft_y(part, sections, slot, n=18, caps=(True, True), interp=2):
    """Sections (y, zback, zfront, halfwidth, exponent[, x]) - vertical panels such as leg shields."""
    sections = interp_sections(sections, interp); vs = []
    for sec in sections:
        y, zb, zf, w, e = sec[:5]; x0 = sec[5] if len(sec) > 5 else 0; cz = (zb + zf) / 2; d = (zf - zb) / 2
        for i in range(n):
            t = 2 * pi * i / n; px, pz = se(t, e); vs.append((x0 + w * px, y, cz + d * pz))
    return part.add(vs, grid_faces(len(sections), n, True, *caps), slot)

def catmull(points, samples=6):
    pts = [p.copy() if isinstance(p, Vector) else Vector(p) for p in points]
    if len(pts) < 3: return pts
    out = []
    ext = [pts[0] * 2 - pts[1]] + pts + [pts[-1] * 2 - pts[-2]]
    for i in range(1, len(ext) - 2):
        p0, p1, p2, p3 = ext[i - 1], ext[i], ext[i + 1], ext[i + 2]
        for k in range(samples):
            t = k / samples; t2 = t * t; t3 = t2 * t
            out.append(.5 * (2 * p1 + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3))
    out.append(pts[-1]); return out

def frames(points, up_hint=(0, 1, 0)):
    """Parallel-transport frames along a polyline: returns (tangent, side, up) per point."""
    pts = [Vector(p) for p in points]; result = []
    hint = Vector(up_hint)
    t0 = (pts[1] - pts[0]).normalized()
    side = t0.cross(hint)
    if side.length < 1e-4: side = t0.cross(Vector((1, 0, 0)))
    side.normalize()
    for i, p in enumerate(pts):
        a = pts[max(0, i - 1)]; b = pts[min(len(pts) - 1, i + 1)]; t = (b - a).normalized()
        side = (side - t * side.dot(t)).normalized(); up = side.cross(t).normalized()
        result.append((t, side, up))
    return result

def sweep(part, points, radii, slot, n=12, e=2.0, up_hint=(0, 1, 0), caps=(True, True), smooth=0):
    """Tube through points; radii is one value, a list per point, or (a,b) pairs (side, up)."""
    pts = catmull(points, smooth) if smooth else [Vector(p) for p in points]
    m = len(pts)
    if not isinstance(radii, (list, tuple)): radii = [radii] * m
    elif len(radii) != m:
        src = list(radii); radii = []
        for i in range(m):
            f = i / (m - 1) * (len(src) - 1); k = min(int(f), len(src) - 2); u = f - k
            a, b = src[k], src[k + 1]
            radii.append(tuple(lerp(a[j], b[j], u) for j in range(2)) if isinstance(a, tuple) else lerp(a, b, u))
    vs = []
    for p, (t, side, up), r in zip(pts, frames(pts, up_hint), radii):
        ra, rb = r if isinstance(r, tuple) else (r, r)
        for i in range(n):
            px, py = se(2 * pi * i / n, e); vs.append(p + side * px * ra + up * py * rb)
    return part.add(vs, grid_faces(m, n, True, *caps), slot)

def revolve(part, profile, centre, axis, slot, n=32, ref=None, loop=False):
    """profile [(radius, along)] revolved about axis through centre; zero radius closes the end.
    loop=True treats the profile as a closed cross-section (a ring such as a rim bed or brake disc)."""
    c = Vector(centre); ax = Vector(axis).normalized()
    ref = Vector(ref) if ref else (Vector((0, 1, 0)) if abs(ax.y) < .9 else Vector((1, 0, 0)))
    u = (ref - ax * ref.dot(ax)).normalized(); v = ax.cross(u)
    vs = []; F = []
    for r, a in profile:
        for i in range(n):
            t = 2 * pi * i / n; vs.append(c + ax * a + (u * cos(t) + v * sin(t)) * r)
    rings = len(profile)
    if loop:
        F = [(j * n + i, j * n + (i + 1) % n, ((j + 1) % rings) * n + (i + 1) % n, ((j + 1) % rings) * n + i) for j in range(rings) for i in range(n)]
    else:
        F = grid_faces(rings, n, True, profile[0][0] > 1e-6, profile[-1][0] > 1e-6)
    return part.add(vs, F, slot)

def ellipsoid(part, centre, radii, slot, rot=None, n=14, m=8):
    c = Vector(centre); R = (rot or Matrix.Identity(3))
    vs = []
    for j in range(m + 1):
        a = pi * j / m
        for i in range(n):
            t = 2 * pi * i / n
            p = Vector((radii[0] * sin(a) * cos(t), radii[1] * cos(a), radii[2] * sin(a) * sin(t)))
            vs.append(c + R @ p)
    F = []
    for j in range(m):
        for i in range(n): F.append((j * n + i, j * n + (i + 1) % n, (j + 1) * n + (i + 1) % n, (j + 1) * n + i))
    return part.add(vs, F, slot)

def rbox(part, centre, size, slot, bevel=.01, rot=None, segments=1):
    bm = bmesh.new(); bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts: v.co = Vector((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
    b = min(bevel, min(size) * .45)
    if b > 0: bmesh.ops.bevel(bm, geom=list(bm.edges), offset=b, segments=segments, affect='EDGES', profile=.5)
    R = rot or Matrix.Identity(3); c = Vector(centre)
    vs = [c + R @ v.co for v in bm.verts]; idx = {v: i for i, v in enumerate(bm.verts)}
    fs = [tuple(idx[v] for v in f.verts) for f in bm.faces]; bm.free()
    return part.add(vs, fs, slot)

def disc(part, centre, axis, r_out, r_in, thick, slot, n=32):
    return revolve(part, [(r_in, -thick / 2), (r_out, -thick / 2), (r_out, thick / 2), (r_in, thick / 2)], centre, axis, slot, n, loop=True) if r_in > 0 else \
        revolve(part, [(0, -thick / 2), (r_out, -thick / 2), (r_out, thick / 2), (0, thick / 2)], centre, axis, slot, n)

def label(part, value, position, size, slot, facing=-1):
    """Text strokes flat in an XY plane facing -Z (rear) or +Z (front)."""
    curve = bpy.data.curves.new('PVLabel', 'FONT'); curve.body = value; curve.size = size
    curve.align_x = 'CENTER'; curve.align_y = 'CENTER'; curve.resolution_u = 1
    obj = bpy.data.objects.new('PVLabel', curve); bpy.context.scene.collection.objects.link(obj)
    deps = bpy.context.evaluated_depsgraph_get(); mesh = obj.evaluated_get(deps).to_mesh()
    p0 = Vector(position); vs = []
    for v in mesh.vertices:
        x, y = v.co.x, v.co.y; vs.append(p0 + Vector((x if facing < 0 else -x, y, 0)))
    fs = [tuple(p.vertices) for p in mesh.polygons]
    obj.evaluated_get(deps).to_mesh_clear(); bpy.data.objects.remove(obj, do_unlink=True); bpy.data.curves.remove(curve)
    two_sided(part, vs, fs, slot, p0 + Vector((0, 0, 1 if facing < 0 else -1)), gap=.0012)
    return part

def star_points(r, inner=.382, rot=0):
    return [(r * (1 if i % 2 == 0 else inner) * cos(pi / 2 + rot + i * pi / 5), r * (1 if i % 2 == 0 else inner) * sin(pi / 2 + rot + i * pi / 5)) for i in range(10)]

# ---------------------------------------------------------------- realisation in Blender
def link(obj, collection):
    collection.objects.link(obj); return obj

def part_object(part, name, collection, mats):
    me = bpy.data.meshes.new(name); bm = bmesh.new()
    verts = [bm.verts.new(TO_BLENDER @ v) for v in part.V]; bm.verts.ensure_lookup_table()
    for f, s in zip(part.F, part.S):
        try: face = bm.faces.new([verts[i] for i in f]); face.material_index = s
        except ValueError: pass
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-6)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces); bm.to_mesh(me); bm.free()
    for m in mats: me.materials.append(m)
    obj = link(bpy.data.objects.new(name, me), collection)
    if part.sub:
        mod = obj.modifiers.new('Sub', 'SUBSURF'); mod.levels = part.sub; mod.render_levels = part.sub
        mod.use_limit_surface = False
        deps = bpy.context.evaluated_depsgraph_get(); new = bpy.data.meshes.new_from_object(obj.evaluated_get(deps))
        obj.modifiers.clear(); old = obj.data; obj.data = new; bpy.data.meshes.remove(old)
        for m in mats:
            if m.name not in obj.data.materials: obj.data.materials.append(m)
    obj.data.shade_smooth()
    if part.sharp: obj.data.set_sharp_from_angle(angle=radians(part.sharp))
    return obj

def apply_modifiers(obj):
    deps = bpy.context.evaluated_depsgraph_get(); new = bpy.data.meshes.new_from_object(obj.evaluated_get(deps))
    old = obj.data; obj.modifiers.clear(); obj.data = new
    if old.users == 0: bpy.data.meshes.remove(old)
    return obj

# ---------------------------------------------------------------- rider anatomy
def ik(a, c, l1, l2, pole):
    """Two-bone IK: joint position between a and c with segment lengths l1,l2 bending toward pole."""
    a, c, pole = Vector(a), Vector(c), Vector(pole)
    d = c - a; dist = min(d.length, (l1 + l2) * .995); dirn = d.normalized()
    x = (l1 * l1 - l2 * l2 + dist * dist) / (2 * dist); h = math.sqrt(max(0, l1 * l1 - x * x))
    p = (pole - dirn * pole.dot(dirn)).normalized()
    return a + dirn * x + p * h

def mirror(v, s): return Vector((v[0] * s, v[1], v[2]))

def rider_joints(pose):
    k = pose.get('scale', 1.0); L = radians(pose.get('lean', 15)); female = pose.get('female', False)
    u = Vector((0, cos(L), sin(L))); f = Vector((0, -sin(L), cos(L))); X = Vector((1, 0, 0))
    hip = Vector(pose['hip']); J = {'u': u, 'f': f, 'k': k, 'hipC': hip}
    hw = (.105 if female else .095) * k; sw = (.188 if female else .205) * k
    J['waist'] = hip + u * .13 * k; J['neck'] = hip + u * .555 * k - f * .012 * k
    J['head'] = J['neck'] + Vector((0, .178 * k, .05 * k + pose.get('head_forward', 0)))
    lengths = {'upper': .35 * k, 'fore': .268 * k, 'thigh': .46 * k, 'shin': .455 * k}
    J.update(lengths)
    for side, s in (('L', -1), ('R', 1)):
        J[side + 'Hip'] = hip + X * s * hw
        J[side + 'Shoulder'] = hip + u * .512 * k + X * s * sw + f * pose.get('shoulder_forward', .02) * k
        grip = mirror(pose['grip'], s); J[side + 'Grip'] = grip
        J[side + 'Wrist'] = grip + Vector((-s * .012, .032, -.082)) * k
        pole = Vector((s * .55, -.7, -.45)) + mirror(pose.get('elbow_pole', (0, 0, 0)), s)
        J[side + 'Elbow'] = ik(J[side + 'Shoulder'], J[side + 'Wrist'], lengths['upper'], lengths['fore'], pole)
        ankle = mirror(pose['ankle'], s); J[side + 'Ankle'] = ankle
        J[side + 'Knee'] = ik(J[side + 'Hip'], ankle, lengths['thigh'], lengths['shin'], Vector((s * pose.get('knee_out', .3), .45, 1)))
        toe = mirror(pose.get('toe', (0, -.05, 1)), s).normalized()
        J[side + 'Toe'] = ankle + toe * .2 * k
        reach = (J[side + 'Wrist'] - J[side + 'Shoulder']).length; leg = (ankle - J[side + 'Hip']).length
        J[side + 'Reach'] = (round(reach / (lengths['upper'] + lengths['fore']), 3), round(leg / (lengths['thigh'] + lengths['shin']), 3))
    return J

def body_primitives(J, outfit):
    """Overlapping clothed body volumes; remeshed into one continuous skin afterwards.
    Returns list of (Part, slot, priority) - priority lets outer clothing win the surface colour."""
    k = J['k']; u = J['u']; f = J['f']; hip = J['hipC']; prims = []
    female = outfit.get('female', False); top = outfit.get('top', 'tee'); bottom = outfit.get('bottom', 'jeans')
    long_arm = top in ('jacket', 'long', 'police')
    def P(): return Part(None)
    def spine(h, fwd=0): return hip + u * h * k + f * fwd * k
    # Pelvis / trousers seat. Sitting compresses the buttocks onto the seat.
    pel = P(); wf = 1.06 if female else 1.0
    secs = [(-.112, .085, .05, -.065), (-.095, .15, .1, -.04), (-.06, .174, .124, -.016), (0, .174, .124, -.004), (.07, .162, .114, .002)]
    sweep(pel, [spine(h, o) for h, w, d, o in secs], [(w * k * wf, d * k) for h, w, d, o in secs], 'pants', n=20, up_hint=(0, 0, 1))
    pel.tag = 'pelvis'; prims.append((pel, 'pants', 0))
    # Shirt / jacket torso: chest, shoulder blades and a hem that overhangs the waistband.
    tor = P(); chest = .118 if female else .116
    secs = [(.0, .178, .127, -.008), (.04, .171, .121, 0), (.11, .153, .107, .006), (.21, .142 if female else .150, .107, .012),
            (.31, .163, chest, .018), (.39, .166 if female else .178, .119, .014), (.46, .172 if female else .188, .108, .006),
            (.51, .17, .095, -.002), (.545, .115, .078, -.008), (.565, .064, .056, -.012)]
    if top == 'jacket': secs = [(h, w + .01, d + .01, o) for h, w, d, o in secs]
    sweep(tor, [spine(h, o) for h, w, d, o in secs], [(w * k, d * k) for h, w, d, o in secs], 'shirt', n=24, up_hint=(0, 0, 1))
    if female and top != 'jacket':
        for s in (-1, 1): ellipsoid(tor, spine(.33, .09) + Vector((s * .062 * k, 0, 0)), (.055 * k, .05 * k, .045 * k), 'shirt')
    tor.tag = 'torso'; prims.append((tor, 'shirt', 2))
    neck = P(); sweep(neck, [J['neck'] - u * .04 * k, J['neck'] + Vector((0, .06, .012)) * k, J['head'] + Vector((0, -.09, -.01)) * k],
                       [.058 * k, .053 * k, .05 * k], 'skin', n=14, up_hint=(0, 0, 1)); neck.tag = 'neck'; prims.append((neck, 'skin', 1))
    col = P(); ring = []
    for i in range(17):
        t = 2 * pi * i / 16; ring.append(J['neck'] - u * .005 * k + (Vector((1, 0, 0)) * cos(t) * .066 + f * sin(t) * .06) * k)
    sweep(col, ring, .011 * k, 'shirtDark', n=8, caps=(False, False)); col.tag = 'collar'; prims.append((col, 'shirtDark', 3))
    for side, s in (('L', -1), ('R', 1)):
        sh, el, wr = J[side + 'Shoulder'], J[side + 'Elbow'], J[side + 'Wrist']
        arm_slot = 'shirt' if long_arm else 'skin'
        dl = P(); ellipsoid(dl, sh + Vector((s * .008, -.004, 0)) * k, (.06 * k, .063 * k, .066 * k), 'shirt', n=14, m=9)
        dl.tag = 'delt' + side; prims.append((dl, 'shirt', 2))
        up = P(); sweep(up, [sh, sh.lerp(el, .45), el], [.057 * k, .058 * k, .046 * k] if not long_arm else [.062 * k, .062 * k, .054 * k], arm_slot, n=14, smooth=3)
        up.tag = 'upper' + side; prims.append((up, arm_slot, 1))
        if top in ('tee', 'polo'):
            sl = P(); a = sh + (sh - el).normalized() * .02; b = sh.lerp(el, .52 if top == 'tee' else .45)
            sweep(sl, [a, sh.lerp(el, .25), b], [.07 * k, .067 * k, .064 * k], 'shirt', n=14); sl.tag = 'sleeve' + side; prims.append((sl, 'shirt', 3))
        ext = wr + (wr - el).normalized() * .02
        fa = P(); sweep(fa, [el, el.lerp(wr, .3), el.lerp(wr, .75), ext],
                        [.05 * k, .05 * k, .043 * k, .04 * k] if long_arm else [.043 * k, .044 * k, .034 * k, .029 * k], arm_slot, n=14, smooth=2)
        prims.append((fa, arm_slot, 1))
        if long_arm:
            cuff = P(); sweep(cuff, [wr - (wr - el).normalized() * .035, wr + (wr - el).normalized() * .01], .044 * k, 'shirtDark', n=14)
            prims.append((cuff, 'shirtDark', 3))
        elb = P(); ellipsoid(elb, el, (.044 * k, .044 * k, .044 * k) if not long_arm else (.054 * k, .054 * k, .054 * k), arm_slot, n=12, m=8); prims.append((elb, arm_slot, 1))
        hp, kn, an = J[side + 'Hip'], J[side + 'Knee'], J[side + 'Ankle']
        th = P(); sweep(th, [hp - (kn - hp).normalized() * .03, hp.lerp(kn, .35), hp.lerp(kn, .75), kn],
                        [(.094 * k, .088 * k), (.086 * k, .08 * k), (.072 * k, .068 * k), (.063 * k, .062 * k)], 'pants', n=16, smooth=2, up_hint=(0, 1, 0))
        th.tag = 'thigh' + side; prims.append((th, 'pants', 0))
        kp = P(); ellipsoid(kp, kn, (.064 * k, .062 * k, .066 * k), 'pants', n=12, m=8); prims.append((kp, 'pants', 0))
        sn = P(); hem = .052 if bottom in ('jeans', 'trousers') else .045
        sweep(sn, [kn, kn.lerp(an, .3), kn.lerp(an, .7), an + (an - kn).normalized() * .01], [.058 * k, .061 * k, .05 * k, hem * k], 'pants', n=14, smooth=2)
        prims.append((sn, 'pants', 0))
        if bottom == 'jeans':
            seam = P(); sweep(seam, [hp.lerp(kn, .15) + Vector((s * .084, 0, 0)) * k, kn + Vector((s * .06, 0, 0)) * k], .004 * k, 'pantsDark', n=6)
            prims.append((seam, 'pantsDark', 4))
    if top == 'jacket':
        hood = P(); ellipsoid(hood, J['neck'] - f * .065 * k + u * .01 * k, (.12 * k, .055 * k, .06 * k), 'shirt', n=14, m=8)
        prims.append((hood, 'shirt', 2))
    return prims

def remesh_object(name, parts, collection, mats, voxel, smooth_iter=6, target_tris=None):
    """Union overlapping volumes into one continuous manifold surface."""
    big = Part(None, sharp=0)
    for p in parts: big.add(p.V, p.F, 0)
    obj = part_object(big, name, collection, mats)
    m = obj.modifiers.new('Union', 'REMESH'); m.mode = 'VOXEL'; m.voxel_size = voxel; m.adaptivity = 0; m.use_smooth_shade = True
    sm = obj.modifiers.new('Soften', 'SMOOTH'); sm.factor = .6; sm.iterations = smooth_iter
    apply_modifiers(obj)
    if target_tris:
        obj.data.calc_loop_triangles(); tris = len(obj.data.loop_triangles)
        if tris > target_tris:
            d = obj.modifiers.new('Budget', 'DECIMATE'); d.ratio = target_tris / tris; d.use_collapse_triangulate = True
            apply_modifiers(obj)
    for mm in mats:
        if mm.name not in obj.data.materials: obj.data.materials.append(mm)
    obj.data.shade_smooth()
    return obj

def colour_by_nearest(obj, prims):
    """Each remeshed face takes the slot of the volume whose surface it lies on (clothing wins ties)."""
    trees = []
    for part, slot, priority in prims:
        vs = [TO_BLENDER @ v for v in part.V]; trees.append((BVHTree.FromPolygons(vs, part.F, all_triangles=False, epsilon=0), S[slot], priority))
    tags = [p.tag for p, _, _ in prims]; attr = obj.data.attributes.get('prim') or obj.data.attributes.new('prim', 'INT', 'FACE')
    for poly in obj.data.polygons:
        c = poly.center; best = None
        for idx, (tree, slot, priority) in enumerate(trees):
            hit = tree.find_nearest(c)
            if hit[0] is None: continue
            score = hit[3] - priority * .0012
            if best is None or score < best[0]: best = (score, slot, idx)
        poly.material_index = best[1] if best else 0; attr.data[poly.index].value = best[2] if best else -1
    obj['prim_tags'] = tags

def head_geometry(part, J, outfit):
    """Sculpted head: jaw taper, brow, sockets, nose, cheekbones, lips, chin, ears, eyes and brows."""
    k = J['k']; c = J['head']; female = outfit.get('female', False)
    rx, ry, rz = .081 * k, .12 * k, .101 * k
    if female: rx *= .95; ry *= .955; rz *= .96
    n, m = 32, 24; vs = []; regions = []
    def g(x, y, cx, cy, sx, sy): return math.exp(-((x - cx) / sx) ** 2 - ((y - cy) / sy) ** 2)
    for j in range(m + 1):
        a = pi * j / m
        for i in range(n):
            t = 2 * pi * i / n; nx, ny, nz = sin(a) * sin(t), cos(a), sin(a) * cos(t)
            x, y, z = nx * rx, ny * ry, nz * rz
            if ny < 0: x *= 1 - .25 * (-ny) ** 1.7
            if nz < 0 and ny > -.35: z *= 1.06
            fr = max(0, nz)
            if nz > 0:
                yy = y / k
                if yy > .03: z *= 1 - .12 * ((yy - .03) / .09) ** 2 * fr
                if yy < -.03: z *= 1 - .16 * min(1, (-yy - .03) / .085) * fr
            yy = y / k; xx = x / k; dz = 0
            # nose: bridge rises to a tip, then drops sharply to the upper lip
            tn = max(0, min(1, (.012 - yy) / .047))
            prof = (.2 + .8 * tn) if yy > -.035 else math.exp(-((yy + .035) / .007) ** 2)
            dz += .021 * prof * math.exp(-(xx / (.0065 + .008 * tn)) ** 2) * fr ** 3
            dz += .005 * g(xx, yy, 0, .03, .05, .011) * fr ** 2
            dz -= .007 * (g(xx, yy, .032, .011, .016, .012) + g(xx, yy, -.032, .011, .016, .012)) * fr ** 2
            dz += .005 * (g(xx, yy, .05, -.02, .02, .02) + g(xx, yy, -.05, -.02, .02, .02)) * fr
            dz += .006 * g(xx, yy, 0, -.062, .021, .008) * fr ** 3
            dz += .009 * g(xx, yy, 0, -.1, .024, .016) * fr ** 2
            dz *= k
            vs.append(c + Vector((x, y, z + dz)))
            if fr > .55 and abs(x) < .021 * k and -.073 * k < y < -.056 * k: regions.append('lip')
            elif y > .05 * k or (nz < -.2 and y > -.065 * k) or (abs(nx) > .8 and y > .015 * k and nz < .25): regions.append('hair' if not outfit.get('bald') else 'skin')
            else: regions.append('skin')
    for j in range(m):
        for i in range(n):
            q = (j * n + i, j * n + (i + 1) % n, (j + 1) * n + (i + 1) % n, (j + 1) * n + i)
            slots = [regions[v] for v in q]; slot = max(set(slots), key=slots.count)
            part.add([vs[v] for v in q], [(0, 1, 2, 3)], slot)
    for s in (-1, 1):
        R = Euler((0, s * radians(12), 0)).to_matrix()
        ellipsoid(part, c + Vector((s * (rx - .003 * k), -.006 * k, -.012 * k)), (.011 * k, .03 * k, .019 * k), 'skin', rot=R, n=10, m=8)
        ellipsoid(part, c + Vector((s * .033 * k, .011 * k, .079 * k)), (.0125 * k, .0085 * k, .009 * k), 'eyeWhite', n=10, m=6)
        ellipsoid(part, c + Vector((s * .033 * k, .011 * k, .0875 * k)), (.0058 * k, .0058 * k, .0016 * k), 'iris', n=10, m=4)
        sweep(part, [c + Vector((s * .016, .03, .092)) * k, c + Vector((s * .034, .035, .091)) * k, c + Vector((s * .052, .03, .082)) * k],
              [.0035 * k, .004 * k, .0025 * k], 'hair', n=6)
    if outfit.get('mask'):
        mask = []
        for j in range(9):
            a = radians(97 + j * 6.4)
            for i in range(15):
                t = radians(-78 + i * (156 / 14)); mask.append(c + Vector((sin(a) * sin(t) * rx * 1.06, cos(a) * ry * 1.02 - .004 * k, sin(a) * cos(t) * rz * 1.08 + .012 * k)))
        F = [(j * 15 + i, j * 15 + i + 1, (j + 1) * 15 + i + 1, (j + 1) * 15 + i) for j in range(8) for i in range(14)]
        two_sided(part, mask, F, outfit.get('mask_slot', 'accent'), c)
    if outfit.get('ponytail'):
        sweep(part, [c + Vector((0, .0, -.09)) * k, c + Vector((0, -.06, -.125)) * k, c + Vector((0, -.15, -.12)) * k, c + Vector((0, -.22, -.1)) * k],
              [.032 * k, .034 * k, .026 * k, .012 * k], 'hair', n=10, smooth=3)
    return rx, ry, rz

def shell_grid(part, centre, radii, amax, slot, n=36, m=12, thickness=.012, rim_slot=None, inner=True):
    """Helmet-like shell: ellipsoid cap from the crown down to amax(theta) (radians from top)."""
    c = Vector(centre); outer = []; inner_v = []
    for j in range(m + 1):
        for i in range(n):
            t = 2 * pi * i / n; a = amax(t) * j / m
            d = Vector((sin(a) * sin(t), cos(a), sin(a) * cos(t)))
            outer.append(c + Vector((d.x * radii[0], d.y * radii[1], d.z * radii[2])))
            inner_v.append(c + Vector((d.x * (radii[0] - thickness), d.y * (radii[1] - thickness), d.z * (radii[2] - thickness))))
    F = [(j * n + i, j * n + (i + 1) % n, (j + 1) * n + (i + 1) % n, (j + 1) * n + i) for j in range(m) for i in range(n)]
    part.add(outer, F[:-n], slot); part.add(outer, F[-n:], rim_slot or slot)
    if inner:
        part.add(inner_v, [tuple(reversed(f)) for f in F], 'plastic')
        rim = [outer[m * n + i] for i in range(n)] + [inner_v[m * n + i] for i in range(n)]
        part.add(rim, [(i, n + i, n + (i + 1) % n, (i + 1) % n) for i in range(n)], rim_slot or slot)
    return outer

def helmet_geometry(part, J, outfit, rxyz):
    k = J['k']; c = J['head']; rx, ry, rz = rxyz; kind = outfit.get('helmet', 'half')
    if kind == 'full':
        hc = c + Vector((0, .006, .012)) * k; R = (rx + .046 * k, ry + .034 * k, rz + .05 * k)
        shell_grid(part, hc, R, lambda t: radians(132 + 20 * (1 - cos(t)) / 2), 'helmet', n=32, m=12, thickness=.02 * k, rim_slot='plastic')
        vis = []
        for j in range(7):
            a = radians(62 + j * 6.2)
            for i in range(17):
                t = radians(-62 + i * (124 / 16)); vis.append(hc + Vector((sin(a) * sin(t) * (R[0] + .003), cos(a) * (R[1] + .003), sin(a) * cos(t) * (R[2] + .006))))
        two_sided(part, vis, [(j * 17 + i, j * 17 + i + 1, (j + 1) * 17 + i + 1, (j + 1) * 17 + i) for j in range(6) for i in range(16)], 'visor', hc)
        sweep(part, [hc + Vector((0, R[1] + .002, .02)), hc + Vector((0, R[1] * .55, -R[2] * .85)), hc + Vector((0, -.02, -R[2] - .002))], .012 * k, 'helmetTrim', n=8, e=4)
        for s in (-1, 1): rbox(part, hc + Vector((s * (R[0] - .006), .002, .03 * k)), (.012 * k, .028 * k, .05 * k), 'plastic', .004)
        return
    deep = kind == 'police'
    hc = c + Vector((0, .02, -.004)) * k; R = (rx + .026 * k, ry * .93 + .02 * k, rz + .03 * k)
    front, side, back = (radians(60), radians(98 if deep else 90), radians(112 if deep else 104))
    def amax(t):
        w = (1 - cos(t)) / 2; sidew = sin(t) ** 2
        return front + (back - front) * w ** 1.2 + (side - (front + back) / 2) * sidew * .6
    shell_grid(part, hc, R, amax, 'helmet', n=28, m=8, thickness=.013 * k, rim_slot='helmetTrim')
    # front brim: a moulded peak continuing the shell line
    pts_in = []; pts_out = []
    for i in range(13):
        t = radians(-58 + i * (116 / 12)); a = amax(t)
        d = Vector((sin(a) * sin(t) * R[0], cos(a) * R[1], sin(a) * cos(t) * R[2])); p = hc + d
        out = Vector((sin(t), 0, cos(t))) * (.052 * k * cos(t * .9) ** 1.5)
        pts_in.append(p); pts_out.append(p + out + Vector((0, -.012 * k, 0)))
    top = pts_in + pts_out; th = Vector((0, -.006 * k, 0))
    bottom = [p + th for p in top]; nn = 13
    F = [(i, i + 1, nn + i + 1, nn + i) for i in range(nn - 1)]
    two_sided(part, top, F, 'helmet', hc - Vector((0, .2, 0))); two_sided(part, bottom, [tuple(reversed(f)) for f in F], 'plastic', hc + Vector((0, .3, 0)))
    edge = pts_out + [p + th for p in pts_out]; part.add(edge, [(i, nn + i, nn + i + 1, i + 1) for i in range(nn - 1)], 'helmetTrim')
    for s in (-1, 1):
        a = amax(pi / 2); rim = hc + Vector((s * sin(a) * R[0] * .97, cos(a) * R[1], 0))
        sweep(part, [rim, c + Vector((s * rx * .86, -.075 * k, .012 * k)), c + Vector((s * .03 * k, -.122 * k, .035 * k)), c + Vector((0, -.126 * k, .037 * k))],
              [(.0065 * k, .0025 * k)] * 4, 'ink', n=8, e=3, smooth=3)
    if deep:
        disc(part, hc + Vector((0, R[1] * .42, R[2] * .92)), (0, .38, 1), .026 * k, 0, .006 * k, 'tail', n=20)
        pts = star_points(.02 * k); cc = hc + Vector((0, R[1] * .42, R[2] * .92 + .0045 * k))
        ax = Vector((0, .38, 1)).normalized(); uu = Vector((1, 0, 0)); vv = ax.cross(uu)
        part.add([cc] + [cc + uu * x + vv * y for x, y in pts], [(0, i + 1, (i + 1) % 10 + 1) for i in range(10)], 'star')

def hand_parts(J, side, glove=False):
    """Gripping hand around a handlebar along X: palm, four curled fingers and thumb."""
    s = -1 if side == 'L' else 1; k = J['k']; G = J[side + 'Grip']; W = J[side + 'Wrist']; slot = glove if glove else 'skin'
    parts = []
    def P(): p = Part(None); parts.append(p); return p
    pa = P(); rbox(pa, G + Vector((s * .004, .024, -.03)) * 1, (.084 * k, .03 * k, .074 * k), slot, .012 * k, rot=Euler((radians(-28), 0, 0)).to_matrix(), segments=3)
    wr = P(); ellipsoid(wr, W + (G - W) * .15, (.037 * k, .024 * k, .034 * k), slot, n=12, m=8)
    R = .0285 * k
    for i, (off, rad) in enumerate(zip((-.03, -.01, .01, .029), (.0098, .0102, .0098, .0085))):
        f = P(); pts = []
        for phi in (-28, 15, 70, 125, 175, 205)[:6 if i < 3 else 5]:
            p = radians(phi); pts.append(G + Vector((s * off * k, R * cos(p) + (.004 * k if phi < 0 else 0), R * sin(p) - (.012 * k if phi < 0 else 0))))
        sweep(f, pts, rad * k, slot, n=8, smooth=3)
    th = P(); sweep(th, [G + Vector((s * -.03, .012, -.045)) * k, G + Vector((s * -.05, -.012, -.03)) * k, G + Vector((s * -.05, -.03, -.006)) * k,
                         G + Vector((s * -.042, -.032, .014)) * k], [.013 * k, .011 * k, .0095 * k, .0085 * k], slot, n=8, smooth=3)
    return parts, slot

def shoe_geometry(part, J, side, style='sneaker'):
    k = J['k']; A = J[side + 'Ankle']; td = (J[side + 'Toe'] - A); td.y = 0; td.normalize()
    up = Vector((0, 1, 0)); sx = up.cross(td).normalized()
    base = A - up * .085 * k
    def P(l, x, y): return base + td * l * k + sx * x * k + up * y * k
    if style == 'boot':
        secs = [(-.07, .04, .3), (-.05, .047, .31), (0, .052, .3), (.06, .053, .12), (.12, .052, .08), (.17, .048, .06), (.2, .038, .045), (.215, .02, .03)]
    elif style == 'sandal':
        secs = [(-.06, .036, .012), (0, .046, .012), (.1, .05, .012), (.17, .048, .012), (.205, .03, .012)]
    else:
        secs = [(-.066, .035, .078), (-.052, .043, .094), (0, .048, .1), (.05, .051, .088), (.1, .053, .072), (.15, .05, .056), (.185, .043, .043), (.206, .029, .033), (.214, .013, .023)]
    upper_slot = 'ink' if style == 'boot' else 'shoe'
    n = 16; vs = []
    for l, w, h in secs:
        for i in range(n):
            px, py = se(2 * pi * i / n, 2.6 if style != 'sandal' else 6); vs.append(P(l, px * w, .012 + (py + 1) * h / 2))
    if style != 'sandal': part.add(vs, grid_faces(len(secs), n), upper_slot)
    sole = []
    for l, w, h in [(secs[0][0] - .004, secs[0][1] + .003, 0), *[(l, w + .005, 0) for l, w, h in secs[1:-1]], (secs[-1][0] + .006, secs[-1][1] + .002, 0)]:
        for i in range(n):
            px, py = se(2 * pi * i / n, 5); sole.append(P(l, px * w, .013 + py * .013))
    part.add(sole, grid_faces(len(secs), n), 'sole' if style == 'sneaker' else 'ink')
    if style == 'sneaker':
        for i in range(4):
            l = .02 + i * .026; sweep(part, [P(l, -.022, .0), P(l, 0, 0), P(l, .022, 0)], .0035 * k, 'sole', n=5)
        for i, p in enumerate(part.V[-20 * 4:]): pass
        # lift lace bars onto the upper surface
        for j in range(len(part.V) - 4 * 3 * 5, len(part.V)):
            q = part.V[j]; l = (q - base).dot(td) / k
            h = next((hh for ll, ww, hh in secs if ll >= l), .07)
            part.V[j] = q + up * (.012 + h - .004) * k
    if style == 'sandal':
        for l in (.02, .12):
            sweep(part, [P(l, -.05, .012), P(l, -.03, .045), P(l, .03, .045), P(l, .05, .012)], [(.004 * k, .012 * k)] * 4, 'ink', n=6, smooth=2)
        toes = []
        ellipsoid(part, P(.17, 0, .03), (.042 * k, .02 * k, .04 * k), 'skin', n=12, m=6)
        ellipsoid(part, P(.06, 0, .035), (.045 * k, .028 * k, .07 * k), 'skin', n=12, m=6)

def surface_decal(part, body, points2d, centre, normal, up, slot, rings=8, lift=.005, subdivide=3):
    """Projects a flat outline (fan around its centre) onto the remeshed body surface.
    The outline is densified so the decal follows chest curvature instead of cutting under it."""
    dense = []
    for i, a in enumerate(points2d):
        b = points2d[(i + 1) % len(points2d)]
        for t in range(subdivide): dense.append((lerp(a[0], b[0], t / subdivide), lerp(a[1], b[1], t / subdivide)))
    points2d = dense
    me = body.data; tree = BVHTree.FromPolygons([v.co.copy() for v in me.vertices], [tuple(p.vertices) for p in me.polygons])
    n = Vector(normal).normalized(); upv = (Vector(up) - n * Vector(up).dot(n)).normalized(); side = upv.cross(n)
    c = Vector(centre); flat = [c]
    for j in range(1, rings + 1):
        for x, y in points2d: flat.append(c + (side * x + upv * y) * (j / rings))
    vs = []
    for p in flat:
        # cast outward from inside the torso so arms in front of the chest are never hit first
        o = TO_BLENDER @ (p - n * .24); d = (TO_BLENDER.to_3x3() @ n).normalized()
        hit = tree.ray_cast(o, d, .5)
        q = hit[0] if hit[0] is not None else TO_BLENDER @ p
        vs.append(TO_BLENDER.inverted() @ (q + (TO_BLENDER.to_3x3() @ n) * lift))
    m = len(points2d); F = [(0, 1 + i, 1 + (i + 1) % m) for i in range(m)]
    for j in range(rings - 1):
        a = 1 + j * m; b = 1 + (j + 1) * m
        for i in range(m): F.append((a + i, b + i, b + (i + 1) % m, a + (i + 1) % m))
    two_sided(part, vs, F, slot, Vector(centre) - n * .3, gap=.0015)

# ---------------------------------------------------------------- motorcycle components (authoring space)
def torus_se(part, centre, R, a, b, e, slot, n=40, m=14):
    """Ring about the X axis with a superellipse section (a radial, b axial): tyres and rims."""
    c = Vector(centre); vs = []
    for i in range(n):
        t = 2 * pi * i / n
        for j in range(m):
            px, py = se(2 * pi * j / m, e); r = R + a * px
            vs.append(c + Vector((b * py, r * sin(t), r * cos(t))))
    F = [(i * m + j, ((i + 1) % n) * m + j, ((i + 1) % n) * m + (j + 1) % m, i * m + (j + 1) % m) for i in range(n) for j in range(m)]
    return part.add(vs, F, slot)

def wheel(parts, centre, bone, width, front, spokes='split', drum=False):
    c = Vector(centre); w = Part(bone, sharp=50); parts.append(w)
    torus_se(w, c, .287, .048, width / 2, 2.6, 'rubber', n=34, m=10)
    torus_se(w, c, .243, .011, width / 2 + .004, 3.0, 'alloy', n=32, m=6)
    revolve(w, [(.236, -width / 2 + .006), (.228, -width / 2 + .016), (.228, width / 2 - .016), (.236, width / 2 - .006)], c, (1, 0, 0), 'metal', n=40, loop=True)
    revolve(w, [(0, -.062), (.03, -.062), (.05, -.05), (.052, .05), (.03, .062), (0, .062)], c, (1, 0, 0), 'alloy', n=20)
    disc(w, c + Vector((.064, 0, 0)), (1, 0, 0), .022, 0, .006, 'chrome', n=16); disc(w, c + Vector((-.064, 0, 0)), (1, 0, 0), .022, 0, .006, 'chrome', n=16)
    count = 5 if spokes == 'split' else 6
    for i in range(count):
        t = 2 * pi * i / count + .3
        def P(r, dt=0, x=0): return c + Vector((x, r * sin(t + dt), r * cos(t + dt)))
        if spokes == 'split':
            sweep(w, [P(.045), P(.1), P(.15)], [(.011, .006)] * 3, 'alloy', n=6, e=3, up_hint=(1, 0, 0))
            for dt in (-.11, .11): sweep(w, [P(.15), P(.19, dt * .55), P(.232, dt)], [(.0085, .0055)] * 3, 'alloy', n=6, e=3, up_hint=(1, 0, 0), smooth=2)
        else:
            sweep(w, [P(.045), P(.14, .06), P(.232, .12)], [(.012, .007)] * 3, 'alloy', n=6, e=3, up_hint=(1, 0, 0), smooth=2)
    if front:
        disc(w, c + Vector((-.058, 0, 0)), (1, 0, 0), .118, .068, .005, 'metal', n=36)
        for i in range(10):
            t = 2 * pi * i / 10; disc(w, c + Vector((-.0605, .093 * sin(t), .093 * cos(t))), (1, 0, 0), .007, 0, .0012, 'ink', n=8)
        for i in range(5):
            t = 2 * pi * i / 5; sweep(w, [c + Vector((-.058, .05 * sin(t), .05 * cos(t))), c + Vector((-.058, .072 * sin(t + .2), .072 * cos(t + .2)))], .006, 'alloy', n=6)
    if drum: revolve(w, [(0, -.075), (.085, -.075), (.092, -.06), (.092, -.03), (0, -.03)], c, (1, 0, 0), 'metal', n=28)
    return w

def mirror_assembly(p, s, base, head, slot='plastic'):
    sweep(p, [base, base + (head - base) * .5 + Vector((s * .01, .02, 0)), head - Vector((s * .03, .02, 0))], .0085, 'plastic', n=8, smooth=3)
    R = Euler((0, s * radians(-12), s * radians(-8))).to_matrix()
    ellipsoid(p, head, (.074, .046, .026), slot, rot=R, n=12, m=8)
    ellipsoid(p, head + R @ Vector((0, 0, -.017)), (.064, .038, .01), 'chrome', rot=R, n=12, m=5)

def handlebar(p, grip, lever_slot='trim', bar_slot='chrome', exposed=True):
    for s in (-1, 1):
        g = mirror(grip, s)
        if exposed: sweep(p, [Vector((0, grip[1] - .025, grip[2] + .03)), Vector((s * .16, grip[1] - .012, grip[2] + .02)), g - Vector((s * .1, 0, 0))], .012, bar_slot, n=10, smooth=3)
        revolve(p, [(0, -.07), (.017, -.07), (.019, -.05), (.019, .05), (.018, .062), (0, .062)], g + Vector((s * .01, 0, 0)), (s, 0, 0), 'rubber', n=14)
        revolve(p, [(0, .062), (.015, .062), (.016, .078), (0, .082)], g + Vector((s * .01, 0, 0)), (s, 0, 0), 'chrome', n=12)
        rbox(p, g - Vector((s * .09, -.004, .0)), (.045, .04, .05), 'plastic', .012)
        sweep(p, [g - Vector((s * .075, -.005, -.025)), g + Vector((s * -.02, -.008, .06)), g + Vector((s * .06, -.014, .07))], [(.006, .002)] * 3, lever_slot, n=8, e=3, smooth=3)

def plate(p, centre, number=('59-V1', '268.79')):
    rbox(p, centre, (.235, .168, .007), 'plate', .006)
    sweep(p, [centre + Vector((-.113, .08, -.005)), centre + Vector((.113, .08, -.005)), centre + Vector((.113, -.08, -.005)), centre + Vector((-.113, -.08, -.005)), centre + Vector((-.113, .08, -.005))],
          .0028, 'ink', n=5, caps=(False, False))
    label(p, number[0], centre + Vector((0, .038, -.0045)), .066, 'ink'); label(p, number[1], centre + Vector((0, -.036, -.0045)), .064, 'ink')

def scooter(parts, look):
    """Small Vietnamese city scooter: flat floor, leg shield, under-seat body, CVT swingarm engine."""
    F = Part('Frame', sharp=0); ST = Part('Steering', sharp=0); M = Part('Frame', sharp=38); SM = Part('Steering', sharp=38)
    parts.extend([F, ST, M, SM]); paint = 'paint'
    wheel(parts, FRONT, 'WheelFront', .096, True); wheel(parts, REAR, 'WheelRear', .112, False, drum=True)
    ax = Vector((0, .899, -.438))  # steering axis, axle up to head
    # fork legs and steering stem
    for s in (-1, 1):
        sweep(SM, [FRONT + Vector((s * .074, 0, 0)), FRONT + Vector((s * .074, 0, 0)) + ax * .26], [.03, .027], 'plastic', n=14)
        sweep(SM, [FRONT + Vector((s * .074, 0, 0)) + ax * .25, FRONT + Vector((s * .07, 0, 0)) + ax * .5], .019, 'chrome', n=12)
        rbox(SM, FRONT + Vector((s * .074, -.015, -.02)), (.034, .07, .05), 'plastic', .012)
    rbox(SM, FRONT + Vector((-.072, .085, -.075)), (.03, .07, .055), 'metal', .01, rot=Euler((radians(-40), 0, 0)).to_matrix())
    sweep(SM, [FRONT + ax * .5, FRONT + ax * .82], .028, 'metal', n=12)
    # front fender following the tyre
    pts = [FRONT + Vector((0, .378 * sin(radians(a)), .378 * cos(radians(a)))) for a in range(28, 134, 7)]
    sweep(ST, pts, [(.064, .012)] * len(pts), paint, n=14, e=5, up_hint=(1, 0, 0))
    # leg shield / front apron: one sculpted volume behind the wheel up to the handlebar
    loft_y(F, [(.40, .27, .40, .17, 3.2), (.47, .28, .44, .19, 3.2), (.56, .285, .48, .215, 3.4), (.66, .29, .53, .235, 3.4), (.76, .30, .60, .245, 3.4),
               (.86, .32, .62, .24, 3.2), (.95, .35, .615, .215, 3.0), (1.02, .38, .58, .17, 2.8), (1.06, .41, .54, .12, 2.6)], paint, n=20)
    # front face garnish, indicators and lower vent
    loft_y(F, [(.62, .505, .515, .05, 4), (.74, .58, .595, .085, 4), (.86, .605, .622, .11, 4), (.92, .608, .626, .105, 4)], 'trim', n=16)
    for s in (-1, 1):
        ellipsoid(F, Vector((s * .175, .905, .585)), (.045, .022, .028), 'amber', rot=Euler((0, s * radians(32), 0)).to_matrix(), n=12, m=8)
    rbox(F, Vector((0, .49, .505)), (.16, .045, .03), 'plastic', .01, rot=Euler((radians(-28), 0, 0)).to_matrix())
    # inner leg-shield pocket and ignition
    rbox(F, Vector((0, .80, .30)), (.2, .12, .03), 'plastic', .012, rot=Euler((radians(-12), 0, 0)).to_matrix())
    disc(F, Vector((.12, .93, .345)), (0, .3, -1), .022, 0, .01, 'chrome', n=14)
    # floorboard with rubber mats and side skirts
    loft_z(F, [(.40, .23, .405, .14, 3.4), (.34, .22, .41, .175, 4), (.10, .22, .41, .19, 4.5), (-.08, .22, .41, .19, 4.5), (-.12, .25, .41, .18, 4)], paint, n=20)
    rbox(F, Vector((0, .415, .15)), (.31, .012, .40), 'plastic', .004)
    for i in range(7): rbox(F, Vector((0, .422, -.02 + i * .055)), (.28, .004, .012), 'rubber', 0)
    # under-seat body sweeping from the floor to a tapered tail
    loft_z(F, [(.0, .40, .52, .12, 3.0), (-.06, .40, .72, .162, 3.2), (-.14, .40, .80, .176, 3.4), (-.28, .42, .812, .184, 3.5), (-.40, .52, .818, .188, 3.5),
               (-.50, .63, .822, .188, 3.4), (-.62, .70, .828, .183, 3.3), (-.76, .725, .835, .174, 3.2), (-.90, .72, .842, .155, 3.0), (-1.0, .735, .846, .122, 2.8),
               (-1.065, .765, .842, .08, 2.6), (-1.095, .785, .83, .035, 2.4)], paint, n=20)
    # character line and lower trim on the side covers
    for s in (-1, 1):
        sweep(M, [Vector((s * .184, .70, -.16)), Vector((s * .19, .735, -.45)), Vector((s * .182, .76, -.75)), Vector((s * .15, .785, -.98))], [(.004, .006)] * 4, 'trim', n=6, smooth=3)
        sweep(M, [Vector((s * .178, .455, -.10)), Vector((s * .187, .56, -.42)), Vector((s * .184, .70, -.63))], .006, 'plastic', n=6, smooth=3)
    # seat: rider and pillion, flat underside, stitched seam
    loft_z(F, [(-.04, .79, .862, .11, 2.4, 0, 6), (-.10, .79, .895, .162, 2.6, 0, 6), (-.25, .79, .902, .178, 2.8, 0, 6), (-.42, .795, .907, .176, 2.8, 0, 6),
               (-.55, .80, .932, .166, 2.8, 0, 6), (-.72, .805, .95, .156, 2.8, 0, 6), (-.88, .81, .948, .136, 2.6, 0, 6), (-.98, .82, .933, .10, 2.4, 0, 6),
               (-1.03, .83, .912, .055, 2.2, 0, 6)], 'seat', n=20)
    sweep(M, [Vector((-.17, .87, -.18)), Vector((-.175, .88, -.5)), Vector((-.16, .91, -.8)), Vector((-.09, .905, -1.0)), Vector((0, .9, -1.03)),
              Vector((.09, .905, -1.0)), Vector((.16, .91, -.8)), Vector((.175, .88, -.5)), Vector((.17, .87, -.18))], .0035, 'trim', n=5, smooth=2, caps=(False, False))
    # grab rail
    sweep(M, [Vector((-.185, .842, -.56)), Vector((-.19, .86, -.84)), Vector((-.14, .875, -1.0)), Vector((0, .88, -1.045)), Vector((.14, .875, -1.0)),
              Vector((.19, .86, -.84)), Vector((.185, .842, -.56))], [(.016, .012)] * 7, 'plastic', n=8, smooth=3)
    # tail light, indicators, reflector, hugger and plate
    ellipsoid(M, Vector((0, .805, -1.083)), (.082, .03, .028), 'tail', n=18, m=10)
    for s in (-1, 1): ellipsoid(M, Vector((s * .098, .79, -1.06)), (.026, .018, .02), 'amber', n=10, m=6)
    sweep(M, [Vector((0, .74, -1.02)), Vector((0, .68, -1.09)), Vector((0, .62, -1.13))], [(.07, .014)] * 3, 'plastic', n=12, e=4, up_hint=(1, 0, 0))
    plate(M, Vector((0, .555, -1.142)))
    disc(M, Vector((0, .44, -1.13)), (0, 0, -1), .022, 0, .008, 'tail', n=12)
    # engine, CVT case, air box, single rear shock (left), muffler (right)
    ellipsoid(M, Vector((.0, .30, -.12)), (.12, .1, .14), 'metal', n=14, m=8)
    loft_z(F, [(-.06, .27, .44, .05, 3, -.163), (-.16, .245, .49, .066, 3.3, -.165), (-.38, .245, .495, .07, 3.4, -.165), (-.58, .262, .46, .066, 3.2, -.165),
               (-.73, .285, .405, .055, 3, -.163), (-.79, .31, .375, .035, 2.6, -.16)], 'trim', n=18)
    for i in range(4): sweep(M, [Vector((-.233, .30 + i * .04, -.14)), Vector((-.236, .30 + i * .04, -.5))], .004, 'metal', n=5)
    rbox(M, Vector((-.13, .53, -.40)), (.09, .1, .3), 'plastic', .03)
    sh0, sh1 = Vector((-.165, .43, -.66)), Vector((-.15, .76, -.5))
    sweep(M, [sh0, sh0.lerp(sh1, .45)], .022, 'metal', n=12); sweep(M, [sh0.lerp(sh1, .4), sh1], .011, 'chrome', n=8)
    helix = []; axis = (sh1 - sh0).normalized(); uu = axis.cross(Vector((1, 0, 0))).normalized(); vv = axis.cross(uu)
    for i in range(70):
        t = i / 69; a = t * 2 * pi * 8; helix.append(sh0.lerp(sh1, .12 + t * .7) + (uu * cos(a) + vv * sin(a)) * .03)
    sweep(M, helix, .0055, 'accent' if look.get('red_spring') else 'metal', n=4)
    loft_z(F, [(-.42, .335, .425, .04, 2.2, .19), (-.50, .315, .445, .06, 2.3, .195), (-.80, .315, .452, .062, 2.3, .2), (-.93, .33, .44, .05, 2.2, .2), (-.98, .35, .42, .03, 2, .2)], 'plastic', n=16)
    loft_z(F, [(-.50, .39, .455, .04, 3, .228), (-.62, .392, .462, .046, 3, .236), (-.80, .39, .46, .042, 3, .236), (-.86, .395, .45, .026, 3, .23)], 'trim', n=14)
    revolve(M, [(0, -.01), (.026, -.01), (.028, .03), (.02, .032), (.016, 0)], Vector((.2, .37, -1.0)), (0, 0, -1), 'chrome', n=16)
    sweep(M, [Vector((.04, .25, -.03)), Vector((.12, .25, -.18)), Vector((.18, .30, -.34)), Vector((.19, .34, -.43))], .021, 'metal', n=10, smooth=3)
    # stands and pillion pegs (folded)
    for s in (-1, 1):
        sweep(M, [Vector((s * .11, .22, -.24)), Vector((s * .115, .19, -.5))], .012, 'plastic', n=8)
        rbox(M, Vector((s * .205, .43, -.36)), (.03, .02, .07), 'plastic', .008)
    sweep(M, [Vector((-.13, .24, -.1)), Vector((-.2, .23, -.32))], .01, 'plastic', n=8)
    # handlebar cover with headlight, instrument and mirrors (all steer)
    grip = Vector((.39, 1.19, .44))
    loft_z(ST, [(.30, 1.10, 1.15, .09, 2.6), (.36, 1.075, 1.185, .175, 3.0), (.46, 1.07, 1.205, .205, 3.2), (.56, 1.075, 1.19, .17, 3.0), (.625, 1.09, 1.165, .12, 2.6),
                (.655, 1.105, 1.15, .07, 2.2)], paint, n=20)
    loft_z(ST, [(.38, 1.035, 1.09, .12, 3), (.5, 1.025, 1.09, .14, 3), (.6, 1.045, 1.1, .1, 3)], 'plastic', n=16)
    ellipsoid(SM, Vector((0, 1.13, .642)), (.1, .036, .018), 'chrome', n=20, m=8)
    ellipsoid(SM, Vector((0, 1.13, .652)), (.088, .028, .012), 'lamp', n=20, m=8)
    ellipsoid(SM, Vector((0, 1.205, .43)), (.105, .012, .055), 'visor', rot=Euler((radians(-18), 0, 0)).to_matrix(), n=18, m=6)
    handlebar(SM, grip)
    for s in (-1, 1): mirror_assembly(SM, s, Vector((s * .25, 1.16, .43)), Vector((s * .36, 1.43, .425)))
    return F, ST, M, SM

def wire_wheel(parts, centre, bone, width, front):
    """Classic wire-spoke wheel with drum hub (Vietnamese underbone commuter)."""
    c = Vector(centre); w = Part(bone, sharp=50); parts.append(w)
    torus_se(w, c, .287, .046, width / 2, 2.6, 'rubber', n=34, m=10)
    torus_se(w, c, .245, .012, width / 2 + .002, 3.4, 'chrome', n=32, m=6)
    revolve(w, [(0, -.07), (.07, -.07), (.075, -.05), (.05, -.035), (.05, .035), (.07, .05), (.07, .065), (0, .065)], c, (1, 0, 0), 'alloy', n=20)
    for i in range(32):
        t = 2 * pi * i / 32; side = -1 if i % 2 else 1; lean = .14 * (1 if (i // 2) % 2 else -1)
        a = c + Vector((side * .045, .062 * sin(t), .062 * cos(t))); b = c + Vector((side * .012, .238 * sin(t + lean), .238 * cos(t + lean)))
        sweep(w, [a, b], .0028, 'chrome', n=4)
    disc(w, c + Vector((-.072, 0, 0)), (1, 0, 0), .078, 0, .012, 'metal', n=20)
    return w

def exposed_fork(SM, steering_top, legs_slot='plastic', tube_slot='chrome', gap=.075, length=.62):
    ax = Vector((0, .899, -.438))
    for s in (-1, 1):
        base = FRONT + Vector((s * gap, 0, 0))
        sweep(SM, [base - ax * .03, base + ax * .27], [.032, .029], legs_slot, n=12)
        sweep(SM, [base + ax * .26, base + ax * length], .02, tube_slot, n=10)
        rbox(SM, base + Vector((0, -.012, -.02)), (.036, .07, .05), legs_slot, .012)
    top = FRONT + ax * length
    rbox(SM, top + Vector((0, .02, -.005)), (gap * 2 + .07, .03, .07), 'alloy', .012, rot=Euler((radians(-26), 0, 0)).to_matrix())
    rbox(SM, FRONT + ax * (length - .2), (gap * 2 + .06, .03, .06), 'alloy', .012, rot=Euler((radians(-26), 0, 0)).to_matrix())
    return top

def engine_block(M, centre, cylinder_axis, finned=True, slot='metal', case_slot='alloy'):
    c = Vector(centre); ax = Vector(cylinder_axis).normalized()
    ellipsoid(M, c, (.13, .12, .15), case_slot, n=14, m=8)
    for s in (-1, 1): ellipsoid(M, c + Vector((s * .1, -.01, -.02)), (.04, .1, .12), case_slot, n=12, m=8)
    cyl = c + ax * .12
    sweep(M, [c + ax * .04, cyl + ax * .1], .06, slot, n=12)
    if finned:
        for i in range(6): disc(M, c + ax * (.1 + i * .022), ax, .075, .05, .008, slot, n=16)
    rbox(M, cyl + ax * .14, (.13, .1, .1), slot, .02, rot=Matrix((Vector((1, 0, 0)), ax.cross(Vector((1, 0, 0))), ax)).transposed())

def chain_drive(M, z_front, side=-1):
    for y in (.37, .31):
        sweep(M, [Vector((side * .1, y, z_front)), Vector((side * .1, y + (.0 if y > .34 else .0), -.73))], [(.006, .01)] * 2, 'metal', n=6, e=4)
    disc(M, Vector((side * .1, .34, -.73)), (1, 0, 0), .1, .03, .008, 'metal', n=24)

def tail_plate(M, z, y):
    sweep(M, [Vector((0, y + .2, z + .1)), Vector((0, y + .1, z + .03)), Vector((0, y, z))], [(.06, .012)] * 3, 'plastic', n=12, e=4, up_hint=(1, 0, 0))
    plate(M, Vector((0, y - .05, z - .015)))
    for s in (-1, 1):
        sweep(M, [Vector((s * .03, y + .16, z + .08)), Vector((s * .1, y + .15, z + .05))], .006, 'plastic', n=6)
        ellipsoid(M, Vector((s * .12, y + .15, z + .045)), (.028, .02, .03), 'amber', n=10, m=6)

def underbone(parts, look):
    """Vietnamese underbone commuter (Wave-type): wire wheels, exposed engine, step-through frame."""
    F = Part('Frame', sharp=0); ST = Part('Steering', sharp=0); M = Part('Frame', sharp=38); SM = Part('Steering', sharp=38)
    parts.extend([F, ST, M, SM]); paint = 'paint'
    wire_wheel(parts, FRONT, 'WheelFront', .09, True); wire_wheel(parts, REAR, 'WheelRear', .1, False)
    top = exposed_fork(SM, None)
    pts = [FRONT + Vector((0, .372 * sin(radians(a)), .372 * cos(radians(a)))) for a in range(20, 141, 8)]
    sweep(ST, pts, [(.06, .011)] * len(pts), paint, n=12, e=5, up_hint=(1, 0, 0))
    # narrow leg shield and step-through backbone cover
    loft_y(F, [(.56, .33, .41, .13, 3), (.66, .34, .46, .18, 3.2), (.80, .35, .50, .2, 3.2), (.92, .37, .5, .18, 3), (1.0, .39, .48, .13, 2.6)], paint, n=18)
    loft_z(F, [(.42, .62, .80, .07, 2.6), (.30, .5, .66, .085, 2.8), (.14, .43, .54, .085, 3), (.0, .44, .56, .1, 3), (-.12, .5, .66, .13, 3)], 'plastic', n=16)
    loft_z(F, [(.40, .70, .82, .06, 2.4), (.28, .58, .70, .07, 2.6), (.16, .5, .6, .075, 2.8)], paint, n=14)
    # engine, carburettor side, kick-start and shift levers
    engine_block(M, (0, .30, .02), (0, .2, 1))
    rbox(M, Vector((.0, .44, -.12)), (.1, .08, .12), 'plastic', .02)
    sweep(M, [Vector((.16, .30, -.08)), Vector((.17, .34, -.25))], .012, 'chrome', n=8)
    sweep(M, [Vector((-.15, .30, .0)), Vector((-.2, .33, .1))], .01, 'chrome', n=8)
    # side covers, under-seat tank, seat, rear fender
    loft_z(F, [(-.08, .52, .76, .13, 3), (-.2, .53, .82, .158, 3.3), (-.42, .58, .83, .164, 3.4), (-.6, .66, .835, .155, 3.2), (-.78, .70, .84, .14, 3), (-.92, .71, .85, .11, 2.8), (-.99, .74, .84, .06, 2.4)], paint, n=20)
    loft_z(F, [(-.08, .80, .875, .11, 2.4, 0, 6), (-.16, .80, .905, .158, 2.6, 0, 6), (-.40, .805, .915, .168, 2.8, 0, 6), (-.62, .81, .935, .16, 2.8, 0, 6),
               (-.84, .82, .945, .14, 2.6, 0, 6), (-.97, .83, .93, .1, 2.4, 0, 6), (-1.02, .84, .905, .05, 2.2, 0, 6)], 'seat', n=20)
    pts = [REAR + Vector((0, .37 * sin(radians(a)), .37 * cos(radians(a)))) for a in range(60, 171, 10)]
    sweep(M, pts, [(.065, .01)] * len(pts), paint, n=12, e=5, up_hint=(1, 0, 0))
    ellipsoid(M, Vector((0, .80, -1.06)), (.07, .045, .04), 'tail', n=14, m=8)
    tail_plate(M, -1.11, .66)
    sweep(M, [Vector((-.15, .86, -.7)), Vector((-.15, .88, -.95)), Vector((0, .89, -1.02)), Vector((.15, .88, -.95)), Vector((.15, .86, -.7))], .012, 'plastic', n=8, smooth=3)
    # swingarm, dual shocks, chain case, exhaust, pegs
    for s in (-1, 1):
        sweep(M, [Vector((s * .1, .34, -.12)), Vector((s * .1, .34, -.73))], [(.02, .03)] * 2, 'metal', n=8, e=3)
        a, b = Vector((s * .135, .38, -.68)), Vector((s * .135, .80, -.56))
        sweep(M, [a, a.lerp(b, .45)], .02, 'plastic', n=10); sweep(M, [a.lerp(b, .4), b], .011, 'chrome', n=8)
        helix = []; d = (b - a).normalized(); uu = d.cross(Vector((1, 0, 0))).normalized(); vv = d.cross(uu)
        for i in range(50): t = i / 49; ang = t * 2 * pi * 6; helix.append(a.lerp(b, .15 + t * .65) + (uu * cos(ang) + vv * sin(ang)) * .026)
        sweep(M, helix, .005, 'chrome', n=4)
        rbox(M, Vector((s * .19, .36, -.06)), (.1, .02, .03), 'rubber', .006)
        rbox(M, Vector((s * .2, .43, -.56)), (.08, .018, .028), 'rubber', .006)
    loft_z(F, [(-.06, .30, .37, .03, 3, -.105), (-.4, .29, .40, .035, 3, -.105), (-.72, .30, .39, .03, 3, -.105)], 'plastic', n=12)
    sweep(M, [Vector((.03, .2, .2)), Vector((.1, .16, .05)), Vector((.17, .2, -.3)), Vector((.2, .3, -.5))], .02, 'chrome', n=10, smooth=3)
    loft_z(F, [(-.45, .29, .38, .04, 2.2, .2), (-.55, .27, .41, .058, 2.3, .205), (-.86, .29, .42, .055, 2.3, .21), (-.98, .33, .40, .03, 2, .21)], 'chrome', n=16)
    loft_z(F, [(-.55, .36, .43, .04, 3, .235), (-.82, .37, .44, .04, 3, .24)], 'plastic', n=12)
    # handlebar cover with headlight and speedo
    grip = Vector((.37, 1.16, .43))
    loft_z(ST, [(.33, 1.07, 1.12, .08, 2.6), (.38, 1.04, 1.16, .17, 3), (.48, 1.03, 1.17, .19, 3.2), (.58, 1.035, 1.15, .14, 3), (.64, 1.05, 1.12, .09, 2.6)], paint, n=20)
    ellipsoid(SM, Vector((0, 1.09, .646)), (.085, .045, .02), 'chrome', n=16, m=8); ellipsoid(SM, Vector((0, 1.09, .655)), (.075, .038, .012), 'lamp', n=16, m=6)
    ellipsoid(SM, Vector((0, 1.17, .44)), (.075, .012, .05), 'visor', rot=Euler((radians(-20), 0, 0)).to_matrix(), n=14, m=6)
    for s in (-1, 1): ellipsoid(SM, Vector((s * .17, 1.09, .6)), (.03, .02, .026), 'amber', n=10, m=6)
    handlebar(SM, grip)
    for s in (-1, 1): mirror_assembly(SM, s, Vector((s * .24, 1.14, .43)), Vector((s * .34, 1.40, .42)))
    return F, ST, M, SM

def sportbike(parts, look):
    """Vietnamese sport underbone (Exciter/Winner type): fairing, twin lamps, tank shroud, upswept tail."""
    F = Part('Frame', sharp=0); ST = Part('Steering', sharp=0); M = Part('Frame', sharp=30); SM = Part('Steering', sharp=38)
    parts.extend([F, ST, M, SM]); paint = 'paint'; acc = 'accent'
    wheel(parts, FRONT, 'WheelFront', .1, True, spokes='straight'); wheel(parts, REAR, 'WheelRear', .13, False, spokes='straight')
    exposed_fork(SM, None, tube_slot='star' if look.get('gold_fork') else 'chrome', gap=.078, length=.66)
    pts = [FRONT + Vector((0, .365 * sin(radians(a)), .365 * cos(radians(a)))) for a in range(40, 121, 8)]
    sweep(SM, pts, [(.06, .01)] * len(pts), paint, n=12, e=6, up_hint=(1, 0, 0))
    # front cowl with twin headlamps, screen and indicators (frame-mounted)
    loft_y(F, [(.80, .44, .58, .12, 4), (.90, .44, .66, .17, 4.2), (1.0, .45, .69, .18, 4.4), (1.08, .47, .68, .15, 4), (1.13, .5, .64, .1, 3.4)], paint, n=18)
    for s in (-1, 1):
        ellipsoid(F, Vector((s * .07, .97, .685)), (.055, .028, .02), 'lamp', rot=Euler((radians(-30), 0, s * radians(-12))).to_matrix(), n=12, m=6)
        loft_y(F, [(.86, .52, .6, .03, 5, s * .175), (1.02, .53, .66, .025, 5, s * .175)], acc, n=12)
        ellipsoid(F, Vector((s * .17, .92, .64)), (.025, .016, .032), 'amber', n=8, m=6)
    loft_y(F, [(1.06, .66, .675, .13, 3), (1.14, .615, .627, .12, 3.2), (1.2, .575, .585, .09, 2.8)], 'visor', n=14)
    # tank shroud and knee recesses, frame spine, radiator
    loft_z(F, [(.46, .74, .92, .1, 3, 0, 4), (.35, .68, .93, .15, 3.6, 0, 4), (.18, .62, .90, .19, 4, 0, 4), (.02, .64, .88, .2, 4, 0, 4), (-.12, .70, .86, .17, 3.6, 0, 4)], paint, n=20)
    for s in (-1, 1):
        loft_z(F, [(.36, .64, .76, .02, 5, s * .17), (.2, .58, .74, .024, 5, s * .2), (.04, .6, .72, .02, 5, s * .2)], acc, n=12)
        sweep(M, [Vector((s * .16, .78, .44)), Vector((s * .19, .82, .2)), Vector((s * .2, .8, -.02))], [(.004, .006)] * 3, 'ink', n=6, smooth=2)
    rbox(M, Vector((-.12, .52, .28)), (.03, .2, .2), 'plastic', .015)
    for i in range(6): rbox(M, Vector((-.137, .44 + i * .03, .28)), (.004, .012, .18), 'metal', 0)
    engine_block(M, (0, .34, .02), (0, .35, 1), finned=False)
    # under-seat, stepped seat, upswept tail, LED light
    loft_z(F, [(-.10, .58, .84, .15, 3.6, 0, 4), (-.3, .64, .87, .16, 4, 0, 4), (-.55, .72, .9, .15, 4, 0, 4), (-.8, .8, .95, .12, 3.6, 0, 4), (-.98, .86, .99, .08, 3.2, 0, 4), (-1.08, .9, 1.0, .03, 2.6, 0, 3)], paint, n=20)
    loft_z(F, [(-.08, .84, .9, .12, 2.6, 0, 6), (-.16, .85, .93, .155, 2.8, 0, 6), (-.42, .86, .935, .15, 2.8, 0, 6), (-.52, .9, .99, .12, 2.6, 0, 6), (-.82, .95, 1.01, .1, 2.6, 0, 6), (-.95, .97, 1.0, .07, 2.4, 0, 6)], 'seat', n=18)
    ellipsoid(M, Vector((0, .955, -1.08)), (.055, .022, .03), 'tail', n=12, m=6)
    for s in (-1, 1): sweep(M, [Vector((s * .15, .8, -.62)), Vector((s * .17, .9, -.86))], [(.004, .03)] * 2, acc, n=6, e=4)
    tail_plate(M, -1.12, .62)
    # swingarm, chain, mono shock, upswept exhaust, rear sets
    for s in (-1, 1): sweep(M, [Vector((s * .105, .36, -.1)), Vector((s * .105, .34, -.73))], [(.02, .038)] * 2, 'alloy', n=8, e=4)
    chain_drive(M, -.08)
    sweep(M, [Vector((0, .5, -.12)), Vector((0, .74, -.3))], .024, 'star' if look.get('gold_fork') else 'accent', n=10)
    sweep(M, [Vector((.03, .2, .2)), Vector((.12, .16, .04)), Vector((.17, .22, -.25)), Vector((.2, .36, -.48))], .022, 'metal', n=10, smooth=3)
    loft_z(F, [(-.42, .38, .46, .04, 2.6, .19), (-.5, .37, .51, .065, 3, .2), (-.78, .43, .57, .062, 3, .21), (-.86, .46, .56, .04, 2.6, .21)], 'plastic', n=16)
    loft_z(F, [(-.5, .47, .53, .03, 4, .25), (-.76, .52, .58, .03, 4, .255)], 'chrome', n=10)
    revolve(M, [(0, 0), (.03, 0), (.03, .03), (0, .03)], Vector((.21, .5, -.86)), (0, .3, -1), 'chrome', n=12)
    for s in (-1, 1):
        rbox(M, Vector((s * .2, .38, -.12)), (.09, .022, .03), 'rubber', .006)
        sweep(M, [Vector((s * .13, .42, -.06)), Vector((s * .2, .38, -.13))], .01, 'alloy', n=6)
    grip = Vector((.36, 1.12, .44))
    sweep(SM, [Vector((0, 1.06, .47)), Vector((0, 1.1, .45))], .03, 'alloy', n=10)
    ellipsoid(SM, Vector((0, 1.13, .47)), (.075, .03, .05), 'plastic', n=12, m=6)
    ellipsoid(SM, Vector((0, 1.16, .47)), (.06, .006, .04), 'visor', n=12, m=4)
    handlebar(SM, grip)
    for s in (-1, 1): mirror_assembly(SM, s, Vector((s * .22, 1.1, .44)), Vector((s * .32, 1.36, .43)))
    return F, ST, M, SM

def tourer(parts, look):
    """Traffic police motorcycle: fairing and screen, big tank, panniers, top box, beacon bars."""
    F = Part('Frame', sharp=0); ST = Part('Steering', sharp=0); M = Part('Frame', sharp=32); SM = Part('Steering', sharp=38)
    parts.extend([F, ST, M, SM]); paint = 'paint'
    wheel(parts, FRONT, 'WheelFront', .12, True, spokes='straight'); wheel(parts, REAR, 'WheelRear', .15, False, spokes='straight')
    exposed_fork(SM, None, gap=.085, length=.64)
    pts = [FRONT + Vector((0, .37 * sin(radians(a)), .37 * cos(radians(a)))) for a in range(30, 131, 8)]
    sweep(SM, pts, [(.068, .011)] * len(pts), paint, n=12, e=6, up_hint=(1, 0, 0))
    loft_y(F, [(.66, .40, .62, .2, 3.4), (.80, .40, .70, .27, 3.8), (.96, .42, .74, .28, 4), (1.1, .45, .72, .24, 3.6), (1.2, .5, .66, .16, 3)], paint, n=20)
    loft_y(F, [(.70, .62, .655, .21, 4), (.86, .69, .725, .25, 4)], 'blue', n=16)
    ellipsoid(F, Vector((0, .95, .745)), (.13, .05, .02), 'lamp', n=16, m=6)
    for s in (-1, 1): ellipsoid(F, Vector((s * .26, .9, .69)), (.03, .02, .03), 'amber', n=8, m=6)
    loft_y(F, [(1.08, .705, .72, .25, 3.2), (1.22, .655, .668, .25, 3.6), (1.36, .6, .612, .23, 3.6), (1.47, .555, .565, .19, 3)], 'khaki', n=16)
    sweep(M, [Vector((-.25, 1.1, .715)), Vector((-.25, 1.36, .61)), Vector((-.19, 1.47, .56)), Vector((.19, 1.47, .56)), Vector((.25, 1.36, .61)), Vector((.25, 1.1, .715))], .006, 'plastic', n=5, smooth=2)
    loft_z(F, [(.44, .78, .98, .14, 3), (.30, .72, 1.02, .22, 3.4), (.10, .70, 1.0, .25, 3.6), (-.08, .74, .96, .21, 3.4), (-.16, .8, .93, .14, 3)], paint, n=20)
    sweep(M, [Vector((-.18, .83, .2)), Vector((-.24, .86, .0))], [(.003, .06)] * 2, 'blue', n=6, e=4)
    sweep(M, [Vector((.18, .83, .2)), Vector((.24, .86, .0))], [(.003, .06)] * 2, 'blue', n=6, e=4)
    engine_block(M, (0, .38, .12), (0, .5, 1), finned=False)
    for s in (-1, 1): ellipsoid(M, Vector((s * .17, .36, .06)), (.06, .09, .1), 'alloy', n=12, m=8)
    for s in (-1, 1):
        sweep(M, [Vector((s * .2, .78, .42)), Vector((s * .33, .55, .38)), Vector((s * .3, .32, .2))], .016, 'chrome', n=8, smooth=3)
        sweep(M, [Vector((s * .22, .9, .62)), Vector((s * .29, 1.1, .7)), Vector((s * .29, 1.31, .71))], .013, 'chrome', n=8, smooth=2)
        disc(M, Vector((s * .29, 1.31, .71)), (0, 1, 0), .03, 0, .02, 'metal', n=12)
    loft_z(F, [(-.12, .6, .9, .17, 3.4), (-.35, .64, .9, .18, 3.6), (-.65, .74, .92, .16, 3.4), (-.9, .8, .95, .12, 3), (-1.0, .84, .95, .06, 2.6)], paint, n=18)
    loft_z(F, [(-.06, .9, .96, .14, 2.6, 0, 6), (-.16, .9, 1.0, .19, 2.8, 0, 6), (-.42, .91, 1.0, .18, 2.8, 0, 6), (-.6, .92, 1.02, .15, 2.6, 0, 6), (-.78, .93, 1.0, .1, 2.4, 0, 6)], 'seat', n=18)
    # panniers with stripes, top box with C.S.G.T plate, beacons rail
    for s in (-1, 1):
        rbox(F, Vector((s * .43, .72, -.72)), (.25, .34, .5), paint, .07, segments=2)
        rbox(M, Vector((s * .556, .74, -.72)), (.006, .07, .46), 'blue', .002)
        rbox(M, Vector((s * .43, .9, -.72)), (.255, .03, .51), 'metal', .012)
        ellipsoid(M, Vector((s * .43, .76, -.975)), (.07, .02, .01), 'tail', n=10, m=4)
    rbox(F, Vector((0, 1.13, -1.02)), (.52, .34, .38), paint, .07, segments=2)
    rbox(M, Vector((0, 1.31, -1.02)), (.53, .03, .39), 'metal', .012)
    rbox(M, Vector((0, 1.12, -1.213)), (.4, .1, .01), 'blue', .006)
    label(M, 'C.S.G.T', Vector((0, 1.12, -1.22)), .07, 'plate')
    ellipsoid(M, Vector((0, 1.0, -1.205)), (.08, .03, .01), 'tail', n=12, m=4)
    ellipsoid(M, Vector((0, .86, -1.02)), (.09, .03, .03), 'tail', n=12, m=6)
    tail_plate(M, -1.1, .62)
    for s in (-1, 1): sweep(M, [Vector((s * .11, .38, -.1)), Vector((s * .11, .34, -.73))], [(.025, .045)] * 2, 'metal', n=8, e=4)
    chain_drive(M, -.05)
    sweep(M, [Vector((.05, .22, .26)), Vector((.14, .2, .0)), Vector((.2, .3, -.4))], .028, 'chrome', n=10, smooth=3)
    revolve(M, [(.0, 0), (.055, 0), (.065, .05), (.065, .32), (.05, .36), (0, .36)], Vector((.24, .38, -.44)), (0, .2, -1), 'chrome', n=16)
    for s in (-1, 1): rbox(M, Vector((s * .22, .38, -.02)), (.1, .025, .04), 'rubber', .008)
    grip = Vector((.36, 1.2, .42))
    handlebar(SM, grip)
    loft_z(SM, [(.36, 1.12, 1.17, .1, 3), (.44, 1.11, 1.2, .16, 3.2), (.52, 1.12, 1.18, .12, 3)], 'plastic', n=14)
    ellipsoid(SM, Vector((0, 1.2, .44)), (.1, .008, .05), 'visor', rot=Euler((radians(-25), 0, 0)).to_matrix(), n=12, m=4)
    for s in (-1, 1): mirror_assembly(SM, s, Vector((s * .24, 1.16, .44)), Vector((s * .36, 1.42, .45)))
    return F, ST, M, SM

def crisp_boundaries(obj, J, outfit):
    """Cut the decimated body along real garment edges (hem, sleeve ends, collar) and recolour
    each side, so clothing borders are straight lines instead of triangle staircases."""
    k = J['k']; u = J['u']; top = outfit.get('top', 'tee')
    tags = list(obj.get('prim_tags', []))
    fwd = J['f']; hip = J['hipC']
    # The hem follows one plane through every volume meeting at the waist; thigh volumes only behind the lap.
    hem_ok = lambda c, tag: tag in ('pelvis', 'torso') or (tag.startswith('thigh') and (c - hip).dot(fwd) < -.045 * k)
    cuts = [(hip + u * .004 * k, u, hip + u * .02 * k, .3 * k, {'torso', 'pelvis', 'thighL', 'thighR'}, lambda d: 'shirt' if d > 0 else 'pants', hem_ok)]
    neck = J['neck'] - u * .005 * k
    if top != 'jacket':
        cuts.append((neck, u, neck, .13 * k, {'torso', 'neck', 'collar'}, lambda d: 'skin' if d > .0 else 'shirtDark' if d > -.016 * k else 'shirt', None))
    if top in ('tee', 'polo'):
        for side in 'LR':
            sh, el = J[side + 'Shoulder'], J[side + 'Elbow']; n = (el - sh).normalized(); p = sh.lerp(el, .52 if top == 'tee' else .45)
            cuts.append((p, n, p, .12 * k, {'sleeve' + side, 'upper' + side, 'delt' + side}, lambda d: 'skin' if d > 0 else 'shirt', None))
    bm = bmesh.new(); bm.from_mesh(obj.data); M = TO_BLENDER.to_3x3(); layer = bm.faces.layers.int.get('prim')
    back = TO_BLENDER.inverted()
    for point, normal, centre, radius, allowed, rule, extra in cuts:
        P = TO_BLENDER @ point; N = (M @ normal).normalized(); C = TO_BLENDER @ centre
        ok = {i for i, t in enumerate(tags) if t in allowed}
        def inside(face):
            if face[layer] not in ok: return False
            c = face.calc_center_median()
            return (c - C).length < radius and (extra is None or extra(back @ c, tags[face[layer]]))
        geom = [f for f in bm.faces if inside(f)]
        if not geom: continue
        edges = list({e for f in geom for e in f.edges}); verts = list({v for f in geom for v in f.verts})
        bmesh.ops.bisect_plane(bm, geom=geom + edges + verts, plane_co=P, plane_no=N, dist=1e-5)
        for f in bm.faces:
            if inside(f): f.material_index = S[rule((f.calc_center_median() - P).dot(N))]
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces); bm.to_mesh(obj.data); bm.free()

HUMAN_PARENT = {'Torso': 'Pelvis', 'Head': 'Torso', 'LUpperArm': 'Torso', 'RUpperArm': 'Torso', 'LForearm': 'LUpperArm', 'RForearm': 'RUpperArm',
                'LThigh': 'Pelvis', 'RThigh': 'Pelvis', 'LShin': 'LThigh', 'RShin': 'RThigh', 'LFoot': 'LShin', 'RFoot': 'RShin'}
HUMAN_CHILD = {'Pelvis': 'Torso', 'Torso': 'Head', 'LUpperArm': 'LForearm', 'RUpperArm': 'RForearm', 'LThigh': 'LShin', 'RThigh': 'RShin', 'LShin': 'LFoot', 'RShin': 'RFoot'}

def segments(J):
    k = J['k']; u = J['u']
    seg = {'Pelvis': (J['hipC'] - u * .09 * k, J['waist'], .15 * k, .07), 'Torso': (J['waist'], J['neck'], .155 * k, .07),
           'Head': (J['neck'], J['neck'] + Vector((0, .14, .02)) * k, .05 * k, .045)}
    for side in 'LR':
        seg[side + 'UpperArm'] = (J[side + 'Shoulder'], J[side + 'Elbow'], .055 * k, .05)
        seg[side + 'Forearm'] = (J[side + 'Elbow'], J[side + 'Wrist'], .045 * k, .045)
        seg[side + 'Thigh'] = (J[side + 'Hip'], J[side + 'Knee'], .08 * k, .07)
        seg[side + 'Shin'] = (J[side + 'Knee'], J[side + 'Ankle'], .057 * k, .05)
        seg[side + 'Foot'] = (J[side + 'Ankle'], J[side + 'Toe'], .045 * k, .035)
    return seg

def skin_weights(obj, J):
    seg = {n: (TO_BLENDER @ a, TO_BLENDER @ b, r, w) for n, (a, b, r, w) in segments(J).items()}
    groups = {n: obj.vertex_groups.get(n) or obj.vertex_groups.new(name=n) for n in seg}
    for v in obj.data.vertices:
        p = v.co; best = None
        for n, (a, b, r, bw) in seg.items():
            ab = b - a; L = ab.length; s = (p - a).dot(ab) / L; t = max(0, min(L, s))
            d = (p - (a + ab * (t / L))).length - r
            if best is None or d < best[0]: best = (d, n, s, L, bw)
        _, n, s, L, bw = best; weights = {n: 1.0}
        if n in HUMAN_PARENT and s < bw:
            wp = 1 - smooth01(.5 + s / (2 * bw)); weights[HUMAN_PARENT[n]] = wp; weights[n] = 1 - wp
        elif n in HUMAN_CHILD and s > L - bw:
            wc = smooth01(.5 + (s - L) / (2 * bw)); weights[HUMAN_CHILD[n]] = wc; weights[n] = 1 - wc
        for g, w in weights.items():
            if w > .001: groups[g].add([v.index], w, 'REPLACE')

# ---------------------------------------------------------------- rig, animation, assembly, export
BONES = ['Frame', 'Steering', 'WheelRear', 'WheelFront', 'Pelvis', 'Torso', 'Head',
         'LUpperArm', 'LForearm', 'LThigh', 'LShin', 'LFoot', 'RUpperArm', 'RForearm', 'RThigh', 'RShin', 'RFoot']
BONE_PARENT = {'Steering': 'Frame', 'WheelRear': 'Frame', 'WheelFront': 'Steering', 'Pelvis': 'Frame', 'Torso': 'Pelvis', 'Head': 'Torso',
               'LUpperArm': 'Torso', 'LForearm': 'LUpperArm', 'LThigh': 'Pelvis', 'LShin': 'LThigh', 'LFoot': 'LShin',
               'RUpperArm': 'Torso', 'RForearm': 'RUpperArm', 'RThigh': 'Pelvis', 'RShin': 'RThigh', 'RFoot': 'RShin'}

def bone_heads(J, steering):
    h = {'Frame': Vector((0, 0, 0)), 'Steering': Vector(steering), 'WheelRear': REAR.copy(), 'WheelFront': FRONT.copy(),
         'Pelvis': J['hipC'], 'Torso': J['waist'], 'Head': J['neck']}
    for s in 'LR':
        h[s + 'UpperArm'] = J[s + 'Shoulder']; h[s + 'Forearm'] = J[s + 'Elbow']; h[s + 'Thigh'] = J[s + 'Hip']
        h[s + 'Shin'] = J[s + 'Knee']; h[s + 'Foot'] = J[s + 'Ankle']
    return h

def skeleton(name, heads, collection):
    data = bpy.data.armatures.new(name + '_Skeleton'); rig = bpy.data.objects.new(name + '_Rig', data); collection.objects.link(rig)
    bpy.context.view_layer.objects.active = rig; rig.select_set(True); bpy.ops.object.mode_set(mode='EDIT')
    # Every bone points up with zero roll: identity rest rotations in Unity, as the game expects.
    for n in BONES:
        b = data.edit_bones.new(n); b.head = B(heads[n]); b.tail = B(heads[n]) + Vector((0, 0, .12)); b.use_deform = True
    for n, p in BONE_PARENT.items(): data.edit_bones[n].parent = data.edit_bones[p]
    bpy.ops.object.mode_set(mode='OBJECT'); rig.select_set(False); rig.show_in_front = True
    return rig

def pose_rotation(rig, name, xyz):
    p = rig.pose.bones[name]; p.rotation_mode = 'QUATERNION'
    basis = p.bone.matrix_local.to_quaternion(); q = Euler(tuple(radians(a) for a in (xyz[0], -xyz[1], -xyz[2])), 'XYZ').to_quaternion()
    p.rotation_quaternion = basis.inverted() @ q @ basis

def pose_offset(rig, name, offset):
    p = rig.pose.bones[name]; p.location = p.bone.matrix_local.to_quaternion().inverted() @ B(offset)

CLIPS = ['Idle', 'Ride', 'KickLeft', 'KickRight', 'FallLeft', 'FallRight', 'Recover']

def animate(rig, style):
    """Seven semantic clips. The game samples them at normalized times; physics moves the vehicle root."""
    sport = style in ('sport', 'police'); clips = []
    for name in CLIPS:
        rig.animation_data_clear()
        frames = 25 if name in ('Idle', 'Ride') else 21 if 'Kick' in name else 41 if 'Fall' in name else 45
        action = bpy.data.actions.new(rig.name.replace('_Rig', '') + '_' + name)
        rig.animation_data_create(); rig.animation_data.action = action
        keys = sorted(set([1, frames] + [1 + round((frames - 1) * p) for p in (.12, .25, .38, .5, .62, .75, .88)]))
        for frame in keys:
            t = (frame - 1) / (frames - 1)
            for p in rig.pose.bones: p.rotation_mode = 'QUATERNION'; p.rotation_quaternion = Quaternion(); p.location = (0, 0, 0); p.scale = (1, 1, 1)
            if name in ('Idle', 'Ride'):
                w = sin(t * 2 * pi); w2 = sin(t * 4 * pi)
                pose_offset(rig, 'Pelvis', (0, .004 * w2, 0))
                pose_rotation(rig, 'Torso', (w * .9, w * .5, w * .6)); pose_rotation(rig, 'Head', (-w * .6, -w * 1.6, w * .8))
                for side, s in (('L', -1), ('R', 1)):
                    pose_rotation(rig, side + 'UpperArm', (w * .6, 0, -s * w * .5)); pose_rotation(rig, side + 'Forearm', (-w * .8, 0, s * w * .4))
                if name == 'Ride':
                    pose_rotation(rig, 'Steering', (0, w * 1.5, 0))
                    for b in ('WheelRear', 'WheelFront'): pose_rotation(rig, b, (t * 360, 0, 0))
                else:
                    pose_rotation(rig, 'Head', (-w * .6, w * 6 * (1 - cos(t * 2 * pi)) * .5, 0))
            elif 'Kick' in name:
                s = -1 if 'Left' in name else 1; side = 'L' if s < 0 else 'R'
                pulse = max(0, sin(pi * min(1, max(0, (t - .1) / .78))))
                pose_rotation(rig, side + 'Thigh', (pulse * -14, pulse * -s * 74, pulse * s * 28))
                pose_rotation(rig, side + 'Shin', (pulse * -38, 0, 0)); pose_rotation(rig, side + 'Foot', (pulse * 20, 0, 0))
                pose_rotation(rig, 'Torso', (pulse * -5, pulse * s * 8, pulse * -s * 12))
                pose_rotation(rig, 'Head', (pulse * 4, pulse * s * 22, pulse * s * 6))
                pose_rotation(rig, side + 'UpperArm', (pulse * 2, pulse * s * 6, pulse * -s * 8))
                pose_offset(rig, 'Pelvis', (s * .03 * pulse, .015 * pulse, 0))
            else:
                s = -1 if name == 'FallLeft' else 1
                fall = min(1, t / .55)
                if name == 'Recover': fall = 1 - smooth01((t - .15) / .8)
                flail = sin(min(1, t / .65) * pi) * (.5 if name == 'Recover' else 1)
                pose_offset(rig, 'Pelvis', (s * .09 * fall, -.04 * fall, .03 * fall))
                pose_rotation(rig, 'Torso', (-16 * fall, s * 12 * fall, s * -15 * fall))
                pose_rotation(rig, 'Head', (14 * fall, s * 10 * fall, -s * 14 * fall))
                for side, ss in (('L', -1), ('R', 1)):
                    pose_rotation(rig, side + 'UpperArm', (-28 * fall, ss * (30 * fall + 35 * flail), ss * -14 * fall))
                    pose_rotation(rig, side + 'Forearm', (-38 * fall, ss * 16 * flail, 0))
                    pose_rotation(rig, side + 'Thigh', (-10 * fall, ss * -20 * fall, ss * 12 * fall))
                    pose_rotation(rig, side + 'Shin', (-28 * flail, 0, 0))
            for p in rig.pose.bones:
                p.keyframe_insert(data_path='rotation_quaternion', frame=frame, group=p.name)
                p.keyframe_insert(data_path='location', frame=frame, group=p.name)
        action.use_fake_user = True; clips.append((name, action, rig.animation_data.action_slot))
        rig.animation_data.action = None
    rig.animation_data_clear(); rig.animation_data_create()
    for name, action, slot in clips:
        track = rig.animation_data.nla_tracks.new(); track.name = name
        strip = track.strips.new(name, 1, action); strip.action_slot = slot; strip.name = name; track.mute = True
    rig.animation_data.action = None
    for p in rig.pose.bones: p.rotation_quaternion = Quaternion(); p.location = (0, 0, 0)
    return clips

def preview_clip(rig, clip, frame=1):
    for track in rig.animation_data.nla_tracks: track.mute = True
    action = bpy.data.actions[rig.name.replace('_Rig', '') + '_' + clip]
    rig.animation_data.action = action; rig.animation_data.action_slot = action.slots[0]
    bpy.context.scene.frame_set(frame)

def rigid(obj, bone):
    g = obj.vertex_groups.new(name=bone); g.add(list(range(len(obj.data.vertices))), 1.0, 'REPLACE'); return obj

def palette_uv(obj, atlas):
    me = obj.data; uv = me.uv_layers.get('PaletteUV') or me.uv_layers.new(name='PaletteUV')
    for poly in me.polygons:
        slot = poly.material_index; u = (slot % 8 + .5) / 8; v = (slot // 8 + .5) / 4
        for li in poly.loop_indices: uv.data[li].uv = (u, v)
    me.materials.clear(); me.materials.append(atlas)
    for poly in me.polygons: poly.material_index = 0

MODELS = {}

def remove_owned(name):
    MODELS.pop(name, None); col = bpy.data.collections.get(name)
    if col:
        for o in list(col.objects): bpy.data.objects.remove(o, do_unlink=True)
        bpy.data.collections.remove(col)
    for a in list(bpy.data.actions):
        if a.name.startswith(name + '_'): bpy.data.actions.remove(a)
    for m in list(bpy.data.meshes):
        if m.users == 0: bpy.data.meshes.remove(m)
    for arm in list(bpy.data.armatures):
        if arm.users == 0: bpy.data.armatures.remove(arm)

def build_rider(J, outfit, col, mats, name):
    objs = []
    prims = body_primitives(J, outfit)
    body = remesh_object(name + '_RiderBody', [p for p, _, _ in prims], col, mats, .0065, 6, outfit.get('body_tris', 6200))
    colour_by_nearest(body, prims); crisp_boundaries(body, J, outfit)
    if outfit.get('star'):
        st = Part('Torso', sharp=0); k = J['k']; u, f = J['u'], J['f']
        surface_decal(st, body, star_points(.122 * k), J['hipC'] + u * .33 * k - f * .2, -f, u, 'star')
        surface_decal(st, body, star_points(.098 * k), J['hipC'] + u * .32 * k + f * .2, f, u, 'star')
        objs.append(rigid(part_object(st, name + '_Star', col, mats), 'Torso'))
    skin_weights(body, J); objs.append(body)
    if outfit.get('top') == 'police':
        k = J['k']; u, f = J['u'], J['f']; tp = Part('Torso', sharp=30)
        for side in 'LR':
            sh = J[side + 'Shoulder']; d = (sh - J['neck']).normalized()
            rbox(tp, sh - d * .045 + u * .06 * k, (.11 * k, .012 * k, .06 * k), 'tail', .004, rot=Matrix((d, u, d.cross(u))).transposed())
            for i in range(2): disc(tp, sh - d * (.02 + i * .04) + u * .068 * k, u, .009 * k, 0, .004 * k, 'star', n=8)
            rbox(tp, J['hipC'] + u * .33 * k + f * .118 * k + Vector(((-1 if side == 'L' else 1) * .07 * k, 0, 0)), (.07 * k, .075 * k, .01 * k), 'shirtDark', .006, rot=Matrix((Vector((1, 0, 0)), u, f)).transposed())
        objs.append(rigid(part_object(tp, name + '_Uniform', col, mats), 'Torso'))
        bp = Part('Pelvis', sharp=0); ring = []
        for i in range(25):
            t = 2 * pi * i / 24; ring.append(J['hipC'] + u * .075 * k + (Vector((1, 0, 0)) * cos(t) * .168 + f * sin(t) * .12) * k)
        sweep(bp, ring, [(.007 * k, .02 * k)] * 25, 'ink', n=6, e=4, up_hint=(0, 1, 0), caps=(False, False))
        rbox(bp, J['hipC'] + u * .075 * k + f * .125 * k, (.05 * k, .035 * k, .012 * k), 'star', .004, rot=Matrix((Vector((1, 0, 0)), u, f)).transposed())
        objs.append(rigid(part_object(bp, name + '_Belt', col, mats), 'Pelvis'))
    hd = Part('Head', sharp=0); r = head_geometry(hd, J, outfit); helmet_geometry(hd, J, outfit, r)
    objs.append(rigid(part_object(hd, name + '_Head', col, mats), 'Head'))
    for side in 'LR':
        hp, slot = hand_parts(J, side, outfit.get('gloves', False))
        hand = remesh_object(name + '_' + side + 'Hand', hp, col, mats, .0034, 4, 620)
        for poly in hand.data.polygons: poly.material_index = S[slot]
        objs.append(rigid(hand, side + 'Forearm'))
        sh = Part(side + 'Foot', sharp=35); shoe_geometry(sh, J, side, outfit.get('shoes', 'sneaker'))
        objs.append(rigid(part_object(sh, name + '_' + side + 'Shoe', col, mats), side + 'Foot'))
    return objs

def build_model(name, bike_fn, look, pose, outfit, steering, style):
    remove_owned(name)
    col = bpy.data.collections.new(name); bpy.context.scene.collection.children.link(col)
    mats = slot_materials(name, look['colors'])
    parts = []; bike_fn(parts, look)
    objs = [rigid(part_object(p, name + '_Part%02d' % i, col, mats), p.bone) for i, p in enumerate(parts) if p.F]
    J = rider_joints(pose); objs += build_rider(J, outfit, col, mats, name)
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]; bpy.ops.object.join(); mesh = bpy.context.view_layer.objects.active
    mesh.name = name + '_Body'; mesh.data.name = name + '_Mesh'
    rig = skeleton(name, bone_heads(J, steering), col)
    for n in BONES:
        if n not in mesh.vertex_groups: raise RuntimeError(name + ' missing weights for ' + n)
    atlas = palette_material(name, look['colors']); palette_uv(mesh, atlas)
    mesh.parent = rig; mod = mesh.modifiers.new('VehicleAndRiderRig', 'ARMATURE'); mod.object = rig
    clips = animate(rig, style); rig['asset_kind'] = style; rig['animation_contract'] = ','.join(CLIPS)
    mesh.data.calc_loop_triangles()
    info = {'name': name, 'kind': style, 'vertices': len(mesh.data.vertices), 'triangles': len(mesh.data.loop_triangles), 'bones': len(rig.data.bones),
            'materials': 1, 'clips': CLIPS, 'reach': {'L': J['LReach'], 'R': J['RReach']}}
    MODELS[name] = (rig, mesh, info, J)
    return rig, mesh, info

def make_lods(name):
    rig, mesh, info, J = MODELS[name]; counts = [info['triangles']]
    for level, ratio in ((1, .45), (2, .17)):
        obj = mesh.copy(); obj.data = mesh.data.copy(); obj.name = name + '_LOD' + str(level); mesh.users_collection[0].objects.link(obj)
        obj.modifiers.clear(); d = obj.modifiers.new('MobileLOD', 'DECIMATE'); d.ratio = ratio; d.use_collapse_triangulate = True
        apply_modifiers(obj); a = obj.modifiers.new('VehicleAndRiderRig', 'ARMATURE'); a.object = rig
        obj.data.calc_loop_triangles(); counts.append(len(obj.data.loop_triangles)); obj.hide_set(True); obj.hide_render = True
    info['lod_triangles'] = counts; return counts

def export_model(name):
    rig, mesh, info, J = MODELS[name]; bpy.ops.object.select_all(action='DESELECT'); rig.animation_data.action = None
    loc = rig.location.copy(); rig.location = (0, 0, 0)
    lods = [o for o in rig.children if '_LOD' in o.name]
    rig.select_set(True); mesh.select_set(True); bpy.context.view_layer.objects.active = rig
    for o in lods: o.hide_set(False); o.hide_render = False; o.select_set(True)
    for track in rig.animation_data.nla_tracks: track.mute = False
    bpy.ops.export_scene.fbx(filepath=OUT + '/' + name + '.fbx', use_selection=True, object_types={'ARMATURE', 'MESH'},
        axis_forward='-Z', axis_up='Y', global_scale=1, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
        add_leaf_bones=False, use_armature_deform_only=True, mesh_smooth_type='FACE', use_mesh_modifiers=True,
        bake_anim=True, bake_anim_use_all_actions=False, bake_anim_use_nla_strips=True, bake_anim_simplify_factor=.35,
        path_mode='RELATIVE', embed_textures=False)
    for track in rig.animation_data.nla_tracks: track.mute = True
    rig.location = loc
    for o in lods: o.hide_set(True); o.hide_render = True
    with open(OUT + '/' + name + '.json', 'w', encoding='utf-8') as fh: json.dump(info, fh, indent=2)
    print('Exported ' + name, json.dumps(info))

# ---------------------------------------------------------------- designs
BASE = {'paint': 'F2F2EE', 'trim': 'A9AEB2', 'plastic': '26292C', 'rubber': '1C1E20', 'chrome': 'D8DDE0', 'alloy': '9DA3A8', 'metal': '4A4F54', 'seat': '2A2522',
        'skin': 'D9A47F', 'skinShade': 'B98262', 'shirt': 'C8211E', 'shirtDark': '9E1715', 'pants': '3E5677', 'pantsDark': '2C3E58', 'shoe': 'EDEDE8', 'sole': 'D9D6CC',
        'star': 'FFD21F', 'helmet': 'F4F3EE', 'helmetTrim': '8A9096', 'visor': '2A3238', 'lamp': 'EEF5F8', 'tail': 'C4161C', 'amber': 'F29A1E', 'plate': 'F3F3EE',
        'ink': '15181A', 'eyeWhite': 'F2EEE6', 'iris': '2B1D16', 'hair': '1A1512', 'lip': 'A8625A', 'accent': '7D858C', 'khaki': 'C9B27E', 'blue': '2F6FD6'}
def colours(**over): c = dict(BASE); c.update(over); return c
SCOOTER_POSE = {'hip': (0, .99, -.28), 'lean': 18, 'grip': (.39, 1.19, .44), 'ankle': (.17, .47, .17), 'toe': (0, -.1, 1)}
DESIGNS = {
    'PV_HeroScooter': dict(bike='scooter', look={'colors': colours()}, pose=SCOOTER_POSE,
        outfit={'top': 'tee', 'bottom': 'jeans', 'star': True, 'helmet': 'half'}, steering=(0, 1.0, .455), style='scooter'),
    'PV_CityScooter': dict(bike='scooter', look={'colors': colours(paint='3C9C99', seat='3A2E2A', skin='E2B394', skinShade='C79677', shirt='9CC8E6', shirtDark='7EAACB',
            pants='34343E', pantsDark='26262E', helmet='E68FA8', helmetTrim='F3E7EA', accent='E9EFF2', hair='231A16', lip='B86A6A')},
        pose={'hip': (0, .985, -.2), 'lean': 12, 'grip': (.39, 1.19, .44), 'ankle': (.14, .47, .15), 'toe': (0, -.1, 1), 'scale': .94, 'female': True, 'knee_out': .1},
        outfit={'female': True, 'top': 'jacket', 'bottom': 'trousers', 'mask': True, 'ponytail': True, 'helmet': 'half', 'shoes': 'sandal'}, steering=(0, 1.0, .455), style='scooter'),
    'PV_Underbone': dict(bike='underbone', look={'colors': colours(paint='24468F', trim='B9BEC2', skin='BF8763', skinShade='9F6E4F', shirt='D8CDB2', shirtDark='BFB295',
            pants='5E5546', pantsDark='4A4337', helmet='2F6B4F', helmetTrim='D9D5C8', hair='3A3532')},
        pose={'hip': (0, 1.0, -.25), 'lean': 13, 'grip': (.37, 1.16, .43), 'ankle': (.19, .45, -.07), 'toe': (0, -.25, 1), 'knee_out': .35},
        outfit={'top': 'polo', 'bottom': 'trousers', 'helmet': 'half', 'shoes': 'sandal'}, steering=(0, 1.0, .455), style='underbone'),
    'PV_SportBike': dict(bike='sport', look={'colors': colours(paint='E3B21E', accent='202327', seat='202022', shirt='3B4048', shirtDark='2A2E34', pants='2B3B55',
            pantsDark='1F2B40', helmet='202326', helmetTrim='E3B21E', visor='1C2328', skin='CF9873')},
        pose={'hip': (0, 1.05, -.33), 'lean': 27, 'grip': (.36, 1.12, .44), 'ankle': (.2, .47, -.15), 'toe': (0, -.35, 1), 'knee_out': .18, 'head_forward': .01},
        outfit={'top': 'jacket', 'bottom': 'jeans', 'helmet': 'full', 'shoes': 'sneaker'}, steering=(0, 1.02, .47), style='sport'),
    'PV_Racer': dict(bike='sport', look={'colors': colours(paint='1E5ED2', accent='EDEFF2', star='D9A41C', seat='1B1C1F', shirt='17181B', shirtDark='0F1012',
            pants='7189AC', pantsDark='5A7194', helmet='1E5ED2', helmetTrim='F2F2F2', visor='15191D', skin='C48A62', shoe='E9E9E4'), 'gold_fork': True, 'red_spring': True},
        pose={'hip': (0, 1.05, -.34), 'lean': 32, 'grip': (.36, 1.12, .44), 'ankle': (.21, .48, -.17), 'toe': (0, -.35, 1), 'knee_out': .25, 'elbow_pole': (.25, .1, 0)},
        outfit={'top': 'tee', 'bottom': 'jeans', 'helmet': 'full', 'shoes': 'sneaker', 'gloves': 'plastic'}, steering=(0, 1.02, .47), style='sport'),
    'PV_TrafficPolice': dict(bike='tourer', look={'colors': colours(paint='F1EFE6', blue='1D4FA8', seat='1E1F22', shirt='CDB57F', shirtDark='AE955F', pants='BFA56F', pantsDark='A38B58',
            helmet='F3F0E6', helmetTrim='1D4FA8', sole='F4F4F2', tail='C8202A', skin='C99068', khaki='A9C2CE')},
        pose={'hip': (0, 1.09, -.17), 'lean': 7, 'grip': (.36, 1.2, .42), 'ankle': (.22, .46, -.03), 'toe': (0, -.2, 1), 'knee_out': .3},
        outfit={'top': 'police', 'bottom': 'trousers', 'helmet': 'police', 'shoes': 'boot', 'gloves': 'sole'}, steering=(0, 1.02, .47), style='police'),
}
BIKES = {'scooter': scooter, 'underbone': underbone, 'sport': sportbike, 'tourer': tourer}

def build_design(name):
    d = DESIGNS[name]
    return build_model(name, BIKES[d['bike']], d['look'], d['pose'], d['outfit'], d['steering'], d['style'])

ORDER = ['PV_HeroScooter', 'PV_CityScooter', 'PV_Underbone', 'PV_SportBike', 'PV_Racer', 'PV_TrafficPolice']

def presentation():
    old = bpy.data.collections.get('PV_Presentation')
    if old:
        for o in list(old.objects): bpy.data.objects.remove(o, do_unlink=True)
        bpy.data.collections.remove(old)
    col = bpy.data.collections.new('PV_Presentation'); bpy.context.scene.collection.children.link(col)
    for o in list(bpy.data.objects):
        if o.name.startswith('PV_LookHead'): bpy.data.objects.remove(o, do_unlink=True)
    mat = bpy.data.materials.get('PV_StudioFloor') or bpy.data.materials.new('PV_StudioFloor'); mat.use_nodes = True
    bsdf = next(n for n in mat.node_tree.nodes if n.type == 'BSDF_PRINCIPLED'); bsdf.inputs['Base Color'].default_value = (.36, .4, .41, 1); bsdf.inputs['Roughness'].default_value = .85
    me = bpy.data.meshes.new('PV_StudioFloor'); bm = bmesh.new(); bmesh.ops.create_grid(bm, x_segments=1, y_segments=1, size=1); bm.to_mesh(me); bm.free(); me.materials.append(mat)
    floor = bpy.data.objects.new('PV_StudioFloor', me); col.objects.link(floor); floor.location = (-4, 0, -.005); floor.scale = (8, 4, 1)
    for nm, loc, energy, size in [('PV_Key', (2, -5, 7), 2200, 7), ('PV_Fill', (-10, -4, 5), 1400, 6), ('PV_Rim', (-4, 5, 6), 1600, 5)]:
        data = bpy.data.lights.new(nm, 'AREA'); data.energy = energy; data.size = size
        obj = bpy.data.objects.new(nm, data); col.objects.link(obj); obj.location = loc
        obj.rotation_euler = (Vector((-4, 0, 1)) - obj.location).to_track_quat('-Z', 'Y').to_euler()
    for nm, loc, target, scale in [('PV_LineupCamera', (-1.5, -13, 6.5), (-4, 0, 1), 10.5), ('PV_HeroCamera', (2.4, -3.6, 2.6), (0, 0, 1.0), 3.2),
                                   ('PV_ChaseCamera', (0, 6.2, 4.8), (0, -1.5, 1.0), 3.6)]:
        data = bpy.data.cameras.new(nm); data.type = 'ORTHO'; data.ortho_scale = scale
        obj = bpy.data.objects.new(nm, data); col.objects.link(obj); obj.location = loc
        obj.rotation_euler = (Vector(target) - obj.location).to_track_quat('-Z', 'Y').to_euler()
    scene = bpy.context.scene; scene.camera = bpy.data.objects['PV_LineupCamera']
    scene.render.resolution_x = 1800; scene.render.resolution_y = 1000; scene.render.fps = 30; scene.frame_start = 1; scene.frame_end = 25
    for i, n in enumerate(ORDER):
        if n in MODELS: MODELS[n][0].location.x = -i * 1.6; preview_clip(MODELS[n][0], 'Ride', 1)

def save_source():
    for image in bpy.data.images:
        if image.name.startswith('PV_'):
            try: image.pack()
            except RuntimeError: pass
    bpy.ops.wm.save_as_mainfile(filepath=SOURCE)

def build_motorcycles(names=None, export=True):
    """Rebuild every motorcycle + rider, make mobile LODs, export FBX/JSON for Unity, save the .blend."""
    bpy.context.scene.render.fps = 30; report = []
    for i, n in enumerate(names or ORDER):
        rig, mesh, info = build_design(n); make_lods(n); rig.location.x = -ORDER.index(n) * 1.6
        if export: export_model(n)
        report.append({'name': n, 'lod_triangles': info['lod_triangles'], 'reach': info['reach']})
    presentation()
    if export: save_source()
    print(json.dumps(report)); return report
