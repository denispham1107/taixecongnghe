"""Author the shop and recovery truck in a dedicated Blender MCP scene."""
import math
import random
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(r"C:/Users/HP/Documents/GameUnity/Taixecongnghe")
OUT = ROOT / "Assets/PhoViet/Resources/ShopHideout"
OUT.mkdir(parents=True, exist_ok=True)
SCENE = "Phố Việt · Cửa hàng"
scene = bpy.data.scenes.get(SCENE) or bpy.data.scenes.new(SCENE)
if scene.objects:
    raise RuntimeError("Open a fresh scene before rebuilding the shop source.")
bpy.context.window.scene = scene
rng = random.Random(21197)

SWATCHES = {
    "ivory": "EAD9B9", "plaster": "D0BA95", "tile": "B2775D", "sand": "E2CCAA",
    "olive": "627E65", "jade": "3F6655", "trim": "B3BE96", "cream": "FAE9C8",
    "oak": "AE7650", "wood": "654533", "ink": "303A3A", "metal": "708387",
    "scarlet": "A9473E", "ochre": "D1A34C", "navy": "3F667A", "pale": "D8DFD4",
    "leaf": "658354", "mint": "A9C8A8", "amber": "EDB86A", "lime": "98A968",
    "blue": "26496B", "glass": "405964", "rubber": "292B2B", "lamp": "FFE2A6",
}
materials = {}
for name, color in SWATCHES.items():
    channels = [int(color[i:i+2], 16) / 255 for i in (0, 2, 4)]
    rgb = tuple(c / 12.92 if c < .04045 else ((c + .055) / 1.055) ** 2.4 for c in channels)
    material = bpy.data.materials.new("PV Shop " + name)
    material.diffuse_color = (*rgb, 1)
    material.use_nodes = True
    node = next(n for n in material.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    node.inputs["Base Color"].default_value = (*rgb, 1)
    node.inputs["Roughness"].default_value = .7
    if name == "lamp":
        node.inputs["Emission Color"].default_value = (*rgb, 1)
        node.inputs["Emission Strength"].default_value = 1.15
    materials[name] = material


def to_blender(x, y, z):
    return (-x, -z, y)


class Model:
    def __init__(self):
        self.vertices = []
        self.faces = []
        self.colors = []

    def add(self, vertices, faces, color):
        offset = len(self.vertices)
        self.vertices.extend(to_blender(*v) for v in vertices)
        self.faces.extend(tuple(offset + i for i in reversed(f)) for f in faces)
        self.colors.extend([color] * len(faces))

    def box(self, x, y, z, width, height, length, color):
        vertices = [(x + i*width/2, y + j*height/2, z + k*length/2)
                    for i in (-1, 1) for j in (-1, 1) for k in (-1, 1)]
        faces = [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1),
                 (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]
        self.add(vertices, faces, color)

    def round(self, x, y, z, radius, depth, color, axis="y", sides=12, taper=1):
        verts = []
        for cap in (0, 1):
            for i in range(sides):
                angle = i * math.tau / sides
                a, b = math.cos(angle)*radius, math.sin(angle)*radius
                if cap:
                    a, b = a*taper, b*taper
                t = (cap - .5)*depth
                verts.append((x+a,y+t,z+b) if axis == "y" else (x+t,y+a,z-b))
        faces = [tuple(range(sides)), tuple(range(2*sides-1, sides-1, -1))]
        faces += [(i, sides+i, sides+(i+1)%sides, (i+1)%sides) for i in range(sides)]
        self.add(verts, faces, color)

    def mesh(self, name):
        mesh = bpy.data.meshes.new(name)
        mesh.from_pydata(self.vertices, [], self.faces)
        mesh.update()
        obj = bpy.data.objects.new(name, mesh)
        scene.collection.objects.link(obj)
        used = list(dict.fromkeys(self.colors))
        for color in used:
            mesh.materials.append(materials[color])
        for poly, color in zip(mesh.polygons, self.colors):
            poly.material_index = used.index(color)
        return obj


def shop():
    m = Model()
    m.box(0,-.10,4.2,6.6,.20,8.6,"wood")
    for ix in range(12):
        for iz in range(15):
            color = "tile" if (ix+iz)%2 else "sand"
            m.box((ix-5.5)*.54,-.025,.30+iz*.55,.530,.055,.54,color)
    for x in (-3.27,3.27):
        m.box(x,1.65,4.25,.22,3.3,8.6,"ivory")
        m.box(x*.974,.47,4.25,.11,.93,8.5,"olive")
        m.box(x*.96,.95,4.25,.075,.055,8.5,"trim")
        m.box(x*.95,.07,4.25,.10,.15,8.5,"jade")
    m.box(0,1.65,8.44,6.6,3.3,.20,"ivory")
    m.box(0,.47,8.31,6.44,.93,.11,"olive")
    m.box(0,3.31,4.24,6.75,.16,8.65,"cream")
    for x in (-2.30,2.30):
        m.box(x,1.2,.12,1.80,2.4,.16,"pale")
        m.box(x,.32,.034,1.80,.64,.15,"jade")
        m.box(x,1.68,.019,1.62,1.48,.03,"mint")
        for dx in (-.79,0,.79):
            m.box(x+dx,1.68,-.005,.04,1.52,.04,"wood")
    for x in (-1.22,1.22):
        m.box(x,1.37,.13,.14,2.74,.24,"wood")
    m.box(0,2.76,.13,2.56,.16,.24,"wood")
    m.box(0,.015,.26,2.22,.04,.52,"pale")
    m.box(0,.041,.87,1.95,.035,.54,"jade")
    for z in (2.3,5.2,7.3):
        m.box(0,3.16,z,2.05,.085,.22,"ink")
        m.box(0,3.11,z,1.9,.025,.15,"lamp")
    goods = ("scarlet", "ochre", "navy", "mint", "ivory", "lime")
    for side in (-1,1):
        for z in (2.15,4.3,6.45):
            if side < 0 and z > 6:
                continue
            x = side*2.64
            m.box(x,1.21,z,.52,2.42,1.97,"wood")
            for dy in (.23,.72,1.2,1.68,2.14):
                m.box(x-side*.16,dy,z,.82,.065,2,"oak")
                for n in range(8):
                    zz = z-.82+n*.23
                    h = rng.uniform(.16,.34)
                    col = rng.choice(goods)
                    m.box(x-side*.33,dy+.038+h/2,zz,.23,h,.163,col)
                    m.box(x-side*.45,dy+.085+h*.34,zz,.014,.066,.131,"cream")
                m.box(x-side*.58,dy-.017,z,.026,.045,2.02,"ink")
    m.box(1.38,.56,4.46,1.0,1.10,1.94,"olive")
    m.box(1.38,1.14,4.46,1.12,.10,2.04,"oak")
    for j in range(6):
        for i in range(2):
            x,z = 1.14+i*.45,3.69+j*.30
            m.round(x,1.29,z,.109,.23,goods[(i+j)%len(goods)])
            m.round(x,1.416,z,.08,.037,"cream")
    m.box(-1.64,.54,6.43,2.22,1.08,.88,"jade")
    m.box(-1.64,1.105,6.43,2.30,.11,.98,"oak")
    for i in range(14):
        m.box(-2.67+i*.158,.50,5.979,.030,.90,.032,"trim")
    m.box(-1.40,1.19,6.31,.41,.09,.30,"ink")
    m.box(-1.40,1.38,6.49,.37,.24,.065,"ink")
    m.box(-1.40,1.38,6.45,.30,.17,.018,"mint")
    for x in (-2.46,2.46):
        m.round(x,.25,.82,.31,.5,"tile",taper=1.24)
        for i in range(7):
            a=i*math.tau/7
            m.round(x+.20*math.cos(a),.59+.10*(i%3),.82+.20*math.sin(a),.17,.31,
                    "leaf" if i%2 else "lime",sides=7,taper=.40)
    m.box(0,2.48,8.22,3.52,.67,.12,"jade")
    m.box(0,2.115,8.23,3.52,.03,.14,"ochre")
    return m.mesh("PV_ShopInterior")


def recovery_truck():
    m = Model()
    m.box(0,.45,.04,2.15,.29,5.75,"blue")
    m.box(0,.78,-.63,2.35,.19,4.55,"metal")
    for x in (-1.04,1.04):
        m.box(x,.89,-.63,.14,.055,4.55,"pale")
        for i in range(9):
            m.box(x,.885,-2.65+i*.49,.16,.035,.18,"amber")
    m.box(0,1.24,2.02,2.13,1.07,1.76,"pale")
    m.box(0,1.57,2.77,1.82,.55,.06,"glass")
    m.box(0,1.85,2.02,2.14,.12,1.77,"pale")
    m.box(0,1.94,2.08,1.11,.10,.22,"amber")
    m.box(0,.57,2.94,2.23,.20,.16,"blue")
    m.box(0,.77,2.91,.91,.20,.06,"ink")
    for s in (-1,1):
        m.box(s*.81,.86,2.93,.36,.16,.09,"lamp")
        m.box(s*.98,1.51,2.19,.17,.45,.94,"glass")
        m.box(s*.88,.72,-2.96,.35,.14,.07,"scarlet")
        for z in (-1.85,1.80):
            m.round(s*1.02,.37,z,.37,.25,"rubber",axis="x",sides=16)
            m.round(s*1.16,.37,z,.22,.04,"metal",axis="x",sides=12)
    return m.mesh("PV_PoliceTow")


def export(obj):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(filepath=str(OUT / (obj.name + ".fbx")), use_selection=True,
                             object_types={"MESH"}, axis_forward="-Z", axis_up="Y",
                             bake_space_transform=True, add_leaf_bones=False)


def preview(interior, truck):
    truck.location.x = 11
    font_path = ROOT / "Assets/PhoViet/Fonts/BeVietnamPro-SemiBold.ttf"
    if font_path.exists():
        title = bpy.data.curves.new("Tạp hóa phố", "FONT")
        title.font = bpy.data.fonts.load(str(font_path))
        title.body, title.align_x, title.align_y, title.size = "TẠP HÓA PHỐ", "CENTER", "CENTER", .30
        obj = bpy.data.objects.new("Bảng nội thất · bản xem Blender", title)
        scene.collection.objects.link(obj)
        obj.location = to_blender(0,2.47,8.14)
        obj.rotation_euler = (math.pi/2,0,math.pi)
        obj.data.materials.append(materials["cream"])
    for i, pos in enumerate(((0,3.03,2.6), (0,3.04,5.85), (0,2.8,.55))):
        light = bpy.data.lights.new(f"Shop light {i}", "AREA")
        light.energy = (165,190,115)[i]
        light.shape, light.size = "DISK", 2.0
        light.color = (1.0,.88,.73)
        obj = bpy.data.objects.new(light.name,light)
        scene.collection.objects.link(obj)
        obj.location = to_blender(*pos)
        direction = Vector(to_blender(0,0,pos[2]+1.3)) - obj.location
        obj.rotation_euler = direction.to_track_quat("-Z","Y").to_euler()
    lens = bpy.data.cameras.new("Cửa hàng · góc nhìn người chơi")
    camera = bpy.data.objects.new(lens.name,lens)
    scene.collection.objects.link(camera)
    camera.location = to_blender(.12,2.35,.50)
    camera.rotation_euler = (Vector(to_blender(0,1.23,6.7)) - camera.location).to_track_quat("-Z","Y").to_euler()
    lens.lens = 18
    scene.camera = camera
    scene.world = bpy.data.worlds.new("Ánh sáng cửa hàng")
    scene.world.use_nodes = True
    background = next(n for n in scene.world.node_tree.nodes if n.type == "BACKGROUND")
    background.inputs["Color"].default_value = (.16,.22,.25,1)
    background.inputs["Strength"].default_value = .45
    scene.render.resolution_x, scene.render.resolution_y = 1280, 800
    scene.render.resolution_percentage = 100
    bpy.ops.object.select_all(action="DESELECT")
    interior.select_set(True)


interior, truck = shop(), recovery_truck()
export(interior)
export(truck)
preview(interior, truck)
print({obj.name: (len(obj.data.vertices), len(obj.data.polygons)) for obj in (interior,truck)})
