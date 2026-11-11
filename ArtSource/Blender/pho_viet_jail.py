"""Build the Phố Việt jail shot in the separate scene prepared by Blender MCP."""
import math
import random
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(r"C:\Users\HP\Documents\GameUnity\Taixecongnghe")
OUT = ROOT / "Assets/PhoViet/Resources/Jail"
OUT.mkdir(parents=True, exist_ok=True)
scene = bpy.context.scene
arm = next(o for o in scene.objects if o.type == 'ARMATURE')
actor = next(o for o in scene.objects if o.type == 'MESH')
arm.animation_data_clear()
for bone in arm.pose.bones:
    bone.matrix_basis.identity()
bpy.context.view_layer.update()


def pose(name, direction, use_rest=False):
    bone = arm.pose.bones['smartrig:' + name]
    current = arm.matrix_world @ bone.matrix
    rest = arm.matrix_world @ bone.bone.matrix_local
    rotation = (rest if use_rest else current).to_quaternion()
    if direction is not None:
        rotation = (rotation @ Vector((0, 1, 0))).rotation_difference(Vector(direction).normalized()) @ rotation
    matrix = rotation.to_matrix().to_4x4()
    matrix.translation = current.translation
    bone.matrix = arm.matrix_world.inverted() @ matrix
    bpy.context.view_layer.update()


pose('Spine', (0, -.22, 1))
pose('Neck', (0, -.24, 1))
pose('Head', (.04, -.23, 1))
for side, sign in [('Left', 1), ('Right', -1)]:
    pose(side + 'UpLeg', (sign * .20, -1, -.10))
    pose(side + 'Leg', (sign * .07, -.14, -1))
    pose(side + 'Foot', None, use_rest=True)
    pose(side + 'Arm', (sign * .08, -.44, -1))
    pose(side + 'ForeArm', (-sign * .42, -.72, -.75))
    pose(side + 'Hand', (-sign * .26, -.9, -.22))
arm.location = (-.47, .69, -.37)
actor.name = 'Golden Star Stance — seated original Meshy model'
base = ROOT / 'Assets/MeshyImports/Golden Star Stance_20261023_150430/meshy_basecolor.png'
skin = actor.data.materials[0]
skin.use_nodes = True
skin_node = next(n for n in skin.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
texture = skin.node_tree.nodes.new('ShaderNodeTexImage')
texture.image = bpy.data.images.load(str(base), check_existing=True)
skin.node_tree.links.new(texture.outputs['Color'], skin_node.inputs['Base Color'])
skin_node.inputs['Roughness'].default_value = .76


def apply_prison_uniform(actor):
    """Replace the clothing colors on the jail-only copy of the Meshy character."""
    mesh = actor.data
    rest = mesh.attributes.get('Uniform Rest Position')
    if rest is None:
        rest = mesh.attributes.new('Uniform Rest Position', 'FLOAT_VECTOR', 'POINT')
    rest.data.foreach_set('vector', [axis for vertex in mesh.vertices for axis in vertex.co])

    mat = mesh.materials[0]
    nodes, links = mat.node_tree.nodes, mat.node_tree.links
    shader = next(node for node in nodes if node.type == 'BSDF_PRINCIPLED')
    if any(node.label == 'Prison uniform over Meshy base color' for node in nodes):
        return
    base_color = shader.inputs['Base Color'].links[0].from_socket

    def connect(value, socket):
        if isinstance(value, (float, int)):
            socket.default_value = value
        else:
            links.new(value, socket)

    def calc(operation, a, b):
        node = nodes.new('ShaderNodeMath')
        node.operation = operation
        connect(a, node.inputs[0])
        connect(b, node.inputs[1])
        return node.outputs[0]

    def separate(source):
        node = nodes.new('ShaderNodeSeparateXYZ')
        links.new(source, node.inputs[0])
        return node.outputs

    attribute = nodes.new('ShaderNodeAttribute')
    attribute.attribute_name = rest.name
    x, _, z = separate(attribute.outputs['Vector'])
    red, green, blue = separate(base_color)
    ax = calc('ABSOLUTE', x, 0)
    upper = calc('MULTIPLY', calc('GREATER_THAN', z, .80), calc('LESS_THAN', z, 1.46))
    red_shirt = calc('MULTIPLY', calc('GREATER_THAN', red, calc('MULTIPLY', green, 4.5)),
                    calc('GREATER_THAN', calc('SUBTRACT', red, blue), .13))
    shirt = calc('MULTIPLY', upper, calc('MULTIPLY', red_shirt, calc('LESS_THAN', ax, .46)))
    # The chest print includes yellow and metallic colors. Cover that part as fabric as well.
    chest = calc('MULTIPLY', calc('LESS_THAN', ax, .18),
                 calc('MULTIPLY', calc('GREATER_THAN', z, .89), calc('LESS_THAN', z, 1.33)))
    denim = calc('MULTIPLY', calc('GREATER_THAN', blue, calc('MULTIPLY', green, 1.025)),
                 calc('GREATER_THAN', calc('SUBTRACT', blue, red), .008))
    pants = calc('MULTIPLY', calc('LESS_THAN', z, .89), denim)
    clothing = calc('MAXIMUM', calc('MAXIMUM', shirt, chest), pants)

    # Coordinates are stored on the undeformed mesh, so the bands follow the bent knees.
    band = calc('FRACT', calc('MULTIPLY', calc('ADD', z, .028), 9.5), 0)
    edge = nodes.new('ShaderNodeMapRange')
    edge.interpolation_type = 'SMOOTHSTEP'
    links.new(calc('ABSOLUTE', calc('SUBTRACT', band, .5), 0), edge.inputs['Value'])
    edge.inputs['From Min'].default_value = .22
    edge.inputs['From Max'].default_value = .26
    stripes = nodes.new('ShaderNodeMixRGB')
    stripes.inputs[1].default_value = (.055, .075, .082, 1)
    stripes.inputs[2].default_value = (.66, .63, .55, 1)
    links.new(edge.outputs['Result'], stripes.inputs[0])
    grain = nodes.new('ShaderNodeTexNoise')
    grain.inputs['Scale'].default_value = 510
    links.new(attribute.outputs['Vector'], grain.inputs['Vector'])
    fabric = nodes.new('ShaderNodeMixRGB')
    fabric.blend_type = 'MULTIPLY'
    fabric.inputs[0].default_value = .18
    links.new(stripes.outputs[0], fabric.inputs[1])
    links.new(grain.outputs['Fac'], fabric.inputs[2])
    uniform = nodes.new('ShaderNodeMixRGB')
    uniform.label = 'Prison uniform over Meshy base color'
    links.new(clothing, uniform.inputs[0])
    links.new(base_color, uniform.inputs[1])
    links.new(fabric.outputs[0], uniform.inputs[2])
    links.new(uniform.outputs[0], shader.inputs['Base Color'])


apply_prison_uniform(actor)


def surface(name, low, high=None, scale=12, rough=.8, metal=0, bump=.15):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*low, 1)
    mat.use_nodes = True
    n, l = mat.node_tree.nodes, mat.node_tree.links
    shader = next(o for o in n if o.type == 'BSDF_PRINCIPLED')
    shader.inputs['Base Color'].default_value = (*low, 1)
    shader.inputs['Roughness'].default_value = rough
    shader.inputs['Metallic'].default_value = metal
    if high:
        coord = n.new('ShaderNodeTexCoord')
        noise = n.new('ShaderNodeTexNoise')
        noise.inputs['Scale'].default_value = scale
        noise.inputs['Detail'].default_value = 4
        l.new(coord.outputs['Object'], noise.inputs['Vector'])
        ramp = n.new('ShaderNodeValToRGB')
        ramp.color_ramp.elements[0].position = .23
        ramp.color_ramp.elements[0].color = (*low, 1)
        ramp.color_ramp.elements[1].position = .79
        ramp.color_ramp.elements[1].color = (*high, 1)
        l.new(noise.outputs['Fac'], ramp.inputs['Fac'])
        l.new(ramp.outputs['Color'], shader.inputs['Base Color'])
        fine = n.new('ShaderNodeTexNoise')
        fine.inputs['Scale'].default_value = scale * 5
        l.new(coord.outputs['Object'], fine.inputs['Vector'])
        relief = n.new('ShaderNodeBump')
        relief.inputs['Strength'].default_value = bump
        relief.inputs['Distance'].default_value = .022
        l.new(fine.outputs['Fac'], relief.inputs['Height'])
        l.new(relief.outputs['Normal'], shader.inputs['Normal'])
    return mat


def box(name, at, size, material, bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=at)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    if bevel:
        edge = obj.modifiers.new('Worn rounded edges', 'BEVEL')
        edge.width = bevel
        edge.segments = 3
        obj.modifiers.new('Face normals', 'WEIGHTED_NORMAL')
    return obj


def rod(name, at, radius, length, material, vertices=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=length, location=at)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    for polygon in obj.data.polygons:
        polygon.use_smooth = len(polygon.vertices) == 4
    return obj


plaster = surface('Weathered lime plaster', (.12, .145, .13), (.41, .39, .31), 5, bump=.24)
paint = surface('Aged blue green oil paint', (.033, .09, .087), (.14, .24, .2), 7, bump=.19)
floor = surface('Pitted concrete tiles', (.061, .074, .075), (.18, .172, .146), 9, bump=.22)
iron = surface('Painted steel', (.032, .044, .043), (.18, .204, .181), 14, .49, .58)
warm_metal = surface('Weathered warm fasteners', (.09, .045, .022), (.37, .22, .089), 19, .58, .44)
wood = surface('Old hardwood bench', (.078, .041, .022), (.29, .164, .072), 18, .72)
cloth = surface('Muted canvas blanket', (.14, .16, .09), (.28, .27, .16), 28, .98)
black = surface('Deep seams', (.019, .027, .026))
box('Floor grout slab', (0, .6, -.11), (5.8, 6.2, .16), black)
for x in range(-5, 6):
    for y in range(-5, 6):
        box('Cast concrete tile', (x * .48, y * .48 + .45, -.018), (.473, .473, .036), floor, .004)
for material, z, height in [(paint, .56, 1.12), (plaster, 2.1, 1.96)]:
    box('Far wall', (0, 1.46, z), (4.66, .24, height), material)
    for side in (-1, 1):
        box('Side wall', (side * 2.26, .22, z), (.24, 2.72, height), material)
box('Ceiling', (0, .22, 3.06), (4.66, 2.72, .18), plaster)
box('Foot trim', (0, 1.31, .057), (4.55, .085, .115), floor, .008)
box('Bench rear beam', (-.47, .99, .45), (1.45, .13, .11), iron, .009)
for dx in (-.6, .59):
    box('Bench trestle', (-.47 + dx, .87, .22), (.085, .34, .44), iron, .012)
for y in (.62, .78, .94, 1.10):
    box('Rounded timber seat', (-.47, y, .457), (1.52, .15, .065), wood, .014)
for layer in range(3):
    box('Folded canvas bedding', (.68, 1.02, .095 + .073 * layer), (.54, .44, .074), cloth, .034)
rod('Enamel water pail', (-1.69, .94, .14), .17, .28, iron, 24)
rod('Rim of pail', (-1.69, .94, .288), .18, .027, warm_metal, 24)
rod('Pail interior', (-1.69, .94, .300), .153, .007, black, 24)
box('Recessed dark window backing', (-.36, 1.329, 2.26), (1.29, .035, .74), black, .025)
glass = surface('Frosted window', (.25, .39, .41), (.47, .66, .71), 6, .57)
for x in (-.78, -.36, .06):
    box('Clouded clerestory glass', (x, 1.299, 2.26), (.39, .019, .68), glass)
for x in (-.99, -.57, -.15, .27):
    box('Window mullion', (x, 1.275, 2.26), (.041, .06, .74), iron, .006)
for z in (1.88, 2.25, 2.64):
    box('Window cross frame', (-.36, 1.274, z), (1.33, .065, .039), iron, .008)
box('Deep concrete sill', (-.36, 1.226, 1.86), (1.44, .22, .11), plaster, .018)
for i in range(15):
    x = -2.08 + .30 * i
    rod('Painted iron cell bar', (x, -1.25, 1.47), .024, 2.86, iron)
for z in (.075, 2.82):
    box('Cell front rail', (0, -1.25, z), (4.44, .098, .093), iron, .009)
for x in (-2.15, .80, 2.15):
    box('Gate jamb', (x, -1.25, 1.45), (.073, .11, 2.9), iron, .011)
box('Gate latch plate', (.80, -1.31, 1.05), (.19, .038, .26), iron, .016)
for z in (.96, 1.13):
    bolt = rod('Gate plate bolt', (.80, -1.334, z), .016, .018, warm_metal, 8)
    bolt.rotation_euler.x = math.pi / 2
box('Door identification plate', (1.44, -1.32, 2.16), (.47, .045, .21), iron, .018)
bpy.ops.object.text_add(location=(1.44, -1.346, 2.16), rotation=(math.pi / 2, 0, 0))
plate = bpy.context.object
plate.name = 'Cell ID'
plate.data.body = 'G - 03'
plate.data.align_x = 'CENTER'
plate.data.align_y = 'CENTER'
plate.data.size = .108
plate.data.extrude = .001
plate.data.materials.append(surface('Worn white lettering', (.72, .67, .53)))


def aim(obj, point):
    obj.rotation_euler = (Vector(point) - obj.location).to_track_quat('-Z', 'Y').to_euler()


def light(name, at, target, energy, color, size):
    data = bpy.data.lights.new(name, 'AREA')
    data.energy = energy
    data.color = color
    data.shape = 'DISK'
    data.size = size
    obj = bpy.data.objects.new(name, data)
    scene.collection.objects.link(obj)
    obj.location = at
    aim(obj, target)


light('Cold window bounce', (-.35, 1.13, 2.40), (-.65, .0, .35), 160, (.55, .77, 1), .85)
light('Warm overhead fluorescent', (.1, .15, 2.82), (-.34, .64, .54), 140, (1, .78, .54), 1.1)
light('Soft corridor fill', (-1.5, -2.4, 2.25), (-.50, .64, .95), 110, (.68, .83, 1), 2)
box('Fluorescent housing', (.05, .16, 2.87), (1.03, .19, .10), iron, .025)
lamp = surface('Lit opal diffuser', (.95, .75, .41))
node = next(n for n in lamp.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
node.inputs['Emission Color'].default_value = (1, .78, .54, 1)
node.inputs['Emission Strength'].default_value = 2
box('Opal tube cover', (.05, .16, 2.81), (.90, .12, .05), lamp, .017)
scene.world = bpy.data.worlds.new('Dark corridor ambient')
scene.world.use_nodes = True
next(n for n in scene.world.node_tree.nodes if n.type == 'BACKGROUND').inputs['Strength'].default_value = .20


def camera(name, at, target, lens, shift_x=0, shift_y=0):
    data = bpy.data.cameras.new(name)
    obj = bpy.data.objects.new(name, data)
    scene.collection.objects.link(obj)
    obj.location = at
    data.lens = lens
    data.shift_x, data.shift_y = shift_x, shift_y
    aim(obj, target)
    return obj


wide = camera('Desktop jail shot', (.24, -3.12, 1.68), (-.13, .58, 1.13), 29, .095)
tall = camera('Portrait jail shot', (.18, -3.2, 1.66), (-.47, .60, 1.02), 32, 0, -.10)
scene.camera = wide
scene.render.engine = 'BLENDER_EEVEE'
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'JPEG'
scene.render.image_settings.quality = 91
bpy.context.view_layer.update()
print('Seated actor bounds', [tuple(round(v, 3) for v in p) for p in actor.bound_box[:2]])
for cam, w, h, label in ((wide, 1440, 900, 'JailWide'), (tall, 810, 1200, 'JailTall')):
    scene.camera = cam
    scene.render.resolution_x, scene.render.resolution_y = w, h
    scene.render.filepath = str(OUT / (label + '.jpg'))
    bpy.ops.render.render(write_still=True)
scene.camera = wide
scene.render.resolution_x, scene.render.resolution_y = 1440, 900
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT / 'ArtSource/Blender/PhoViet_Jail.blend'))
print('Rendered both jail shots and saved the Blender source')
