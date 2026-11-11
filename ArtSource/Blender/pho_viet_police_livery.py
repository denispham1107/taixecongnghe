"""Project the corrected Vietnamese side lettering onto the Meshy UV atlas."""

import bpy
import numpy as np


def paint_livery(car, original, lettering, output):
    def pixels(image):
        width, height = image.size
        data = np.empty(width * height * 4, dtype=np.float32)
        image.pixels.foreach_get(data)
        return data.reshape(height, width, 4)

    atlas = pixels(original)
    glyph = pixels(lettering)[:, :, 3]
    height, width = atlas.shape[:2]
    mesh = car.data
    mesh.calc_loop_triangles()
    uv_layer = mesh.uv_layers.active.uv
    vertices = np.asarray([tuple(car.matrix_world @ vertex.co) for vertex in mesh.vertices])
    mask = np.zeros((height, width), dtype=np.float32)
    paint = np.zeros((height, width, 3), dtype=np.float32)

    # Each orthographic side is 1200 px across 1.04 Blender units.
    scale = 1200 / 1.04
    for tri in mesh.loop_triangles:
        xyz = vertices[list(tri.vertices)]
        normal = np.cross(xyz[1] - xyz[0], xyz[2] - xyz[0])
        side = np.sign(xyz[:, 1].mean())
        if abs(xyz[:, 1].mean()) < .12 or side * normal[1] < .45 * np.linalg.norm(normal):
            continue

        screen_x = 600 - side * xyz[:, 0] * scale
        screen_y = 300 - xyz[:, 2] * scale
        across = screen_y - (309.2 if side > 0 else 308.0) - side * .035 * (screen_x - 600)
        upper_edge = 20 if side > 0 else 17.5
        if screen_x.max() < 345 or screen_x.min() > 855 or across.min() > 17.5 or across.max() < -upper_edge:
            continue

        uv = np.asarray([tuple(uv_layer[i].vector) for i in tri.loops]) * (width, height)
        lo = np.maximum(np.floor(uv.min(axis=0)).astype(int) - 2, 0)
        hi = np.minimum(np.ceil(uv.max(axis=0)).astype(int) + 2, (width, height))
        yy, xx = np.mgrid[lo[1]:hi[1], lo[0]:hi[0]]
        a, b = uv[1:] - uv[0]
        det = a[0] * b[1] - a[1] * b[0]
        if abs(det) < .001:
            continue
        dx, dy = xx + .5 - uv[0, 0], yy + .5 - uv[0, 1]
        u, v = (dx * b[1] - dy * b[0]) / det, (a[0] * dy - a[1] * dx) / det
        x = screen_x[0] + u * (screen_x[1] - screen_x[0]) + v * (screen_x[2] - screen_x[0])
        t = across[0] + u * (across[1] - across[0]) + v * (across[2] - across[0])
        feather = np.minimum(np.minimum((x - 345) / 5, (855 - x) / 5), np.minimum(t + upper_edge, 17.5 - t) / 2)
        feather = np.clip(feather, 0, 1).astype(np.float32)
        valid = (u >= 0) & (v >= 0) & (u + v <= 1) & (feather > 0)
        if not np.any(valid):
            continue

        # The same reading direction is used for both outward views.
        gx, gy = (x[valid] - 381) / 438, (.5 - t[valid] / 29)
        inside = (gx >= 0) & (gx < 1) & (gy >= 0) & (gy < 1)
        label = np.zeros(gx.shape, dtype=np.float32)
        label[inside] = glyph[(gy[inside] * glyph.shape[0]).astype(int), (gx[inside] * glyph.shape[1]).astype(int)]
        blue = np.asarray((11, 61, 157) if side > 0 else (11, 53, 137), dtype=np.float32) / 255
        rows, cols = yy[valid], xx[valid]
        mask[rows, cols] = feather[valid]
        paint[rows, cols] = blue + (.985 - blue) * label[:, None]

    # Cover bilinear sampling at the UV island borders without disturbing the original atlas.
    for _ in range(2):
        before = mask.copy()
        for shift, axis in ((1, 0), (-1, 0), (1, 1), (-1, 1)):
            neighbor = np.roll(before, shift, axis)
            fill = (mask == 0) & (neighbor > 0)
            paint[fill] = np.roll(paint, shift, axis)[fill]
            mask[fill] = neighbor[fill]
    atlas[:, :, :3] += (paint - atlas[:, :, :3]) * mask[:, :, None]
    image = bpy.data.images.new('CSGT - corrected Vietnamese livery', width, height, alpha=False)
    image.colorspace_settings.name = original.colorspace_settings.name
    image.pixels.foreach_set(atlas.ravel())
    image.filepath_raw = str(output)
    image.file_format = 'PNG'
    image.save()
    return image
