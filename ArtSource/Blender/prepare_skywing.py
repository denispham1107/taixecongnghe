"""Split the Meshy Crimson Skywing into an airframe and a spinning propeller."""
from pathlib import Path
import bmesh
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Assets/MeshyImports/Crimson_Skywing_20261110_013834"
OUTPUT = ROOT / "Assets/PhoViet/Art/Skywing"

assert not bpy.data.filepath, "Run with --background --factory-startup"
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(next(SOURCE.glob("*.fbx"))))
source, = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
bpy.context.view_layer.objects.active = source
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
mesh = source.data
neighbors = [[] for _ in mesh.vertices]
for edge in mesh.edges:
    a, b = edge.vertices
    neighbors[a].append(b)
    neighbors[b].append(a)

seen = set()
propeller = set()
for start in range(len(neighbors)):
    if start in seen:
        continue
    stack = [start]
    seen.add(start)
    group = []
    while stack:
        vertex = stack.pop()
        group.append(vertex)
        for adjacent in neighbors[vertex]:
            if adjacent not in seen:
                seen.add(adjacent)
                stack.append(adjacent)
    # The scan's two blades and spinner are a single component ahead of the fixed engine.
    points = [mesh.vertices[i].co for i in group]
    if min(p.x for p in points) < -.30 and max(p.x for p in points) < -.225 and max(p.y for p in points) > .15:
        propeller.update(group)

assert len(propeller) == 592, "Unexpected source propeller topology"
scale = 6.0 / (max(v.co.y for v in mesh.vertices) - min(v.co.y for v in mesh.vertices))
spinner = Vector((-.289, .004, .021))
for vertex in mesh.vertices:
    x, y, z = vertex.co
    vertex.co = Vector((-y, x, z - spinner.z)) * scale

parts = []
for name, moving in (("Airframe", False), ("Propeller", True)):
    part = mesh.copy()
    work = bmesh.new()
    work.from_mesh(part)
    work.verts.ensure_lookup_table()
    bmesh.ops.delete(work, geom=[v for v in work.verts if (v.index in propeller) != moving], context="VERTS")
    work.to_mesh(part)
    work.free()
    if moving:
        pivot = Vector((-spinner.y, spinner.x, 0)) * scale
        for vertex in part.vertices:
            vertex.co -= pivot
        print("Unity propeller pivot:", (pivot.x, pivot.z, -pivot.y))
    for level, ratio in enumerate((.70, .28, .11)):
        obj = bpy.data.objects.new(f"Skywing_{name}_LOD{level}", part.copy())
        bpy.context.collection.objects.link(obj)
        bpy.context.view_layer.objects.active = obj
        modifier = obj.modifiers.new("Distance detail", "DECIMATE")
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.data.calc_loop_triangles()
        print(obj.name, len(obj.data.loop_triangles))
        parts.append(obj)

bpy.data.objects.remove(source, do_unlink=True)
for obj in parts:
    obj.select_set(True)
bpy.context.view_layer.objects.active = parts[0]
OUTPUT.mkdir(parents=True, exist_ok=True)
bpy.ops.export_scene.fbx(
    filepath=str(OUTPUT / "PV_Skywing.fbx"), use_selection=True, object_types={"MESH"},
    axis_forward="-Z", axis_up="Y", apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS",
    bake_anim=False, add_leaf_bones=False, mesh_smooth_type="FACE",
)
