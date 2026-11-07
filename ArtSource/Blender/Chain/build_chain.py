"""Build the chain meshes and a short cast/latch/retract study in Blender."""
import bpy
import json
from math import cos, pi, sin
from pathlib import Path
from mathutils import Vector

root = Path(__file__).resolve().parents[3]
output = root / "ArtSource/Blender/Chain"
scene_before = bpy.context.window.scene
scene = bpy.data.scenes.new("Pho Viet | Chain grapple")
bpy.context.window.scene = scene


def material(name, color, metallic):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = 0.3
    return mat


def tube(name, points, radii, sides, closed, mat):
    vertices, faces = [], []
    for i, (point, radius) in enumerate(zip(points, radii)):
        before = points[(i - 1) % len(points)] if closed or i else point
        after = points[(i + 1) % len(points)] if closed or i < len(points) - 1 else point
        tangent = (after - before).normalized()
        side = tangent.cross(Vector((0, 0, 1))).normalized()
        for j in range(sides):
            angle = 2 * pi * j / sides
            vertices.append(point + radius * (side * cos(angle) + Vector((0, 0, sin(angle)))))
    for i in range(len(points) if closed else len(points) - 1):
        row, next_row = i * sides, ((i + 1) % len(points)) * sides
        for j in range(sides):
            next_j = (j + 1) % sides
            faces.append((row + j, next_row + j, next_row + next_j, row + next_j))
    if not closed:
        faces.extend((tuple(range(sides)), tuple(range(len(vertices) - 1, len(vertices) - sides - 1, -1))))
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    mesh.materials.append(mat)
    for face in mesh.polygons:
        face.use_smooth = len(face.vertices) == 4
    return mesh


steel = material("Brushed blue steel", (0.30, 0.49, 0.60), 0.66)
brass = material("Forged amber hook", (0.88, 0.35, 0.055), 0.64)
link_points = []
for i in range(20):
    angle = 2 * pi * i / 20
    link_points.append(Vector((0.062 * cos(angle), 0.062 * sin(angle) + (0.069 if i <= 10 else -0.069), 0)))
# Two semicircles joined by straight rails.
link = tube("Chain link", link_points, [0.021] * 20, 8, True, steel)
hook_points = [Vector((x, y, 0)) for x, y in [
    (0, -.19), (0, -.08), (-.012, .015), (-.042, .10), (-.04, .17),
    (.007, .224), (.072, .236), (.133, .202), (.159, .145), (.149, .091),
    (.113, .066), (.069, .068)]]
hook = tube("Forged hook", hook_points, [.036,.038,.044,.046,.046,.044,.042,.037,.032,.026,.019,.007], 10, False, brass)


def export(mesh):
    mesh.calc_loop_triangles()
    return {
        "vertices": [dict(x=round(v.co.x, 6), y=round(v.co.z, 6), z=round(v.co.y, 6)) for v in mesh.vertices],
        "triangles": [index for tri in mesh.loop_triangles for index in reversed(tri.vertices)],
    }


scene.render.fps = 60
scene.frame_end = 23
controller = bpy.data.objects.new("Chain timing", None)
scene.collection.objects.link(controller)
poses = [(1, .14, .11), (4, .48, .16), (7, 1, .055), (13, 1, .055), (17, .63, .11), (23, 0, 0)]
for frame, extension, sag in poses:
    controller["extension"] = float(extension)
    controller.keyframe_insert(data_path='["extension"]', frame=frame)


def sample(first, last):
    result = []
    for frame in range(first, last):
        for subframe in (0, .5):
            scene.frame_set(frame, subframe=subframe)
            result.append(round(controller["extension"], 6))
    scene.frame_set(last)
    return result + [round(controller["extension"], 6)]


asset = root / "Assets/PhoViet/Resources/Chain/PV_Chain.json"
data = {"link": export(link), "hook": export(hook), "cast": sample(1, 7), "retract": sample(13, 23)}
asset.write_text(json.dumps(data, separators=(",", ":")) + "\n", encoding="utf-8")
for i in range(25):
    is_hook = i == 24
    obj = bpy.data.objects.new(f"{'Hook' if is_hook else 'Link'} {i:02}", hook if is_hook else link)
    scene.collection.objects.link(obj)
    for frame, normalized, sag in poses:
        extension = 3.8 * normalized
        t = 1 if is_hook else (i + .5) / 24
        obj.location = (0, extension * t, 0.7 - sag * sin(pi * t))
        obj.rotation_euler = (0, (i % 2) * pi / 2, 0)
        obj.scale = (1, 1, 1) if is_hook else (1, min(1, extension / 2.8), 1)
        for prop in ("location", "rotation_euler", "scale"):
            obj.keyframe_insert(data_path=prop, frame=frame)
camera_data = bpy.data.cameras.new("Chain study")
camera = bpy.data.objects.new("Chain study", camera_data)
scene.collection.objects.link(camera)
scene.camera = camera
camera.location = (4.5, 3.3, 3.9)
camera.rotation_euler = (Vector((0, 1.9, .65)) - camera.location).to_track_quat("-Z", "Y").to_euler()
camera_data.type = "ORTHO"
camera_data.ortho_scale = 4.7
scene.render.engine = "BLENDER_WORKBENCH"
scene.display.shading.color_type = "MATERIAL"
scene.display.shading.show_cavity = True
scene.display.shading.background_type = "WORLD"
scene.world = bpy.data.worlds.new("Chain study backdrop")
scene.world.color = (.035, .05, .065)
scene.render.resolution_x, scene.render.resolution_y = 1100, 550
scene.frame_set(7)
bpy.ops.wm.save_as_mainfile(filepath=str(output / "PV_Chain.blend"), copy=True)
bpy.context.window.scene = scene_before
bpy.data.scenes.remove(scene)
print(f"Exported {len(link.vertices)} link vertices, {len(hook.vertices)} hook vertices, and 23-frame Blender study.")
