# 21.289 — Mặt tiền nhà phố chi tiết, MỘT phong cách hiện đại đồng nhất kiểu shophouse (thay mặt tiền khối hộp của shophouse()).
# Người dùng (20/11/2026): «mặt tiền các căn nhà còn quá thô và sơ sài, nhất là cửa sổ và ban công — dựng lại thật chi tiết, đẹp,
# giống ngoài đời thật» (Blender MCP); sau bản thử 3 kiểu: «Chỉ dựng phong cách hiện đại đồng nhất và phải phù hợp với mặt tiền
# dạng shop house».
# Nạp:  ns = {'OW_DIR': ..., 'NO_RUN': True}; exec(build_ref3_segment.py, ns); ns['mats'](); exec(ow_facade.py, ns); ns['facade_mats']()
# Hệ trục như shophouse(): mặt tiền quay +x tại x = xf, nhà ở x < xf, y dọc phố [y0, y0 + w], z lên.
# Giữ nguyên: số tầng + màu tường (hai lần rút ngẫu nhiên đầu như shophouse cũ → chiều cao không đổi), tầng trệt (mặt kính tiệm,
# trụ, bậc, mái hiên — Unity menu 10 thay bằng kính + nội thất), chừa chỗ bảng hiệu tầng 2 (không ban công, khung bắt đầu trên biển).
# Ngôn ngữ thiết kế (mọi căn giống nhau, chỉ khác bề ngang/số tầng/màu tường/vị trí ban công):
#  • Khung bao bê tông sáng nhô 1,2 m: hai cột 0,3 m chạy suốt các tầng trên + ô mái đỉnh dày, tường mái lùi phía sau.
#  • Tầng ban công: sàn mỏng có gờ mép, gạch lát, cửa kính trượt sát trần khung nhôm xám than, lan can
#    thanh dẹt đứng + tay vịn nhôm phẳng, bồn cây dài sau lan can, đèn ốp trần.
#  • Tầng cửa sổ: tường có lỗ thật (má cửa dày 0,22 m), cửa sổ kính lớn chia 2–4 ô + ô thoáng, bậu nhôm, ô văng bê tông phẳng
#    trên đầu cửa, ốp lam gỗ ngang dưới bậu, bồn cây treo ngoài bậu; căn rộng ≥ 6,9 m có màn lam nhôm đứng một bên.
import math, random
from mathutils import Vector

FM = {}
def facade_mats():
    FM['alu'] = mat('OW_AluCharcoal', '#2F3336', 0.35, 0.5)                 # nhôm sơn tĩnh điện xám than
    FM['glass'] = mat('OW_GlassBlue', '#2A4652', 0.05, 0.35)
    FM['glass_sky'] = mat('OW_GlassSky', '#6F8C9C', 0.04, 0.5)              # ô thoáng phản trời
    FM['frame'] = mat_noise('OW_FrameConcrete', '#E7E5DF', '#D2CFC7', scale=5, rough=0.7, bump=0.04)   # khung bê tông sơn sáng
    FM['wood'] = mat_noise('OW_WoodSlat', '#8A5A36', '#6E4428', scale=12, rough=0.6, bump=0.08)       # lam gỗ / nhôm vân gỗ
    FM['interior'] = mat('OW_InteriorDark', '#24211E', 0.95)
    FM['tile_floor'] = mat_tiles('OW_BalconyTile', '#9C5C40', '#8A4F37', '#5A463C', size=0.3)   # đất nung (như ảnh duyệt)
    FM['planter'] = mat('OW_PlanterGrey', '#5B5F61', 0.6)
    FM['iron'] = M['iron']; FM['leaves'] = M['leaves']; FM['leaves2'] = M['leaves2']; FM['concrete'] = M['concrete']
    FM['steel'] = M['metal_gray']; FM['lamp'] = M['lamp']; FM['soil'] = M['soil']

# ---------------------------------------------------------------- hình học cơ bản
def skin_with_openings(mb, xf, ya, yb, z0, z1, holes, m, t=0.22):
    """Tường dày t (x: xf−t..xf) phủ [ya, yb]×[z0, z1], chừa các lỗ (y0, y1, za, zb) không chồng nhau theo y."""
    holes = sorted(holes); y = ya
    for (hy0, hy1, za, zb) in holes:
        if hy0 > y + 1e-4: mb.boxab((xf - t, y, z0), (xf, hy0, z1), m)
        if za > z0 + 1e-4: mb.boxab((xf - t, hy0, z0), (xf, hy1, za), m)
        if zb < z1 - 1e-4: mb.boxab((xf - t, hy0, zb), (xf, hy1, z1), m)
        y = hy1
    if yb > y + 1e-4: mb.boxab((xf - t, y, z0), (xf, yb, z1), m)

def frame_ring(mb, x0, x1, ya, yb, za, zb, s, m):
    """Khung chữ nhật 4 thanh tiết diện s (mép trong bao [ya, yb]×[za, zb]) trong lớp x0..x1."""
    mb.boxab((x0, ya - s, za - s), (x1, ya, zb + s), m)
    mb.boxab((x0, yb, za - s), (x1, yb + s, zb + s), m)
    mb.boxab((x0, ya, za - s), (x1, yb, za), m)
    mb.boxab((x0, ya, zb), (x1, yb, zb + s), m)

def glazing(mb, xw, ya, yb, za, zb, n, transom, r, sliding=False):
    """Bộ khung nhôm xám than mặt ngoài tại xw: khung bao 0,06, n ô kính (đố 0,05), ô thoáng trên; sliding: cánh lùa so le."""
    A = FM['alu']; d = 0.08; xb = xw - d
    frame_ring(mb, xb, xw, ya + 0.06, yb - 0.06, za + 0.06, zb - 0.06, 0.06, A)
    ya, yb, za, zb = ya + 0.06, yb - 0.06, za + 0.06, zb - 0.06
    if transom:
        zt = zb - min(0.42, (zb - za) * 0.2)
        mb.boxab((xb, ya, zt - 0.03), (xw, yb, zt + 0.03), A)
        for i in range(n):
            a = ya + (yb - ya) * i / n; b = ya + (yb - ya) * (i + 1) / n
            if i: mb.boxab((xb, a - 0.02, zt), (xw, a + 0.02, zb), A)
            mb.boxab((xw - 0.05, a + 0.02, zt + 0.03), (xw - 0.04, b - 0.02, zb), FM['glass_sky'])
        zb = zt - 0.03
    for i in range(n):
        a = ya + (yb - ya) * i / n; b = ya + (yb - ya) * (i + 1) / n
        off = (-0.035 if i % 2 else 0.0) if sliding else 0.0                  # cánh lùa nằm hai ray so le
        if i: mb.boxab((xb + off, a - 0.025, za), (xw + off, a + 0.025, zb), A)
        a2, b2 = a + (0.025 if i else 0), b - (0.025 if i < n - 1 else 0)
        if sliding:
            frame_ring(mb, xw - 0.045 + off, xw - 0.01 + off, a2 + 0.035, b2 - 0.035, za + 0.035, zb - 0.035, 0.035, A)
        mb.boxab((xw - 0.035 + off, a2 + 0.03, za + 0.03), (xw - 0.025 + off, b2 - 0.03, zb - 0.03), FM['glass'])
        if sliding or i % 2 == 0:                                              # tay nắm đứng
            hy = b2 - 0.09 if i % 2 == 0 else a2 + 0.09
            mb.box((xw + 0.005 + off, hy, za + (zb - za) * 0.48), (0.025, 0.02, 0.32 if sliding else 0.14), FM['steel'])

def curtain(mb, x, ya, yb, za, zb, r):
    """Rèm sau kính: hai tấm kéo về hai bên, nếp gấp so le (nhìn qua kính tối thấy sâu hơn)."""
    mb.boxab((x - 0.02, ya, za), (x, yb, zb), FM['interior'])
    for a, b in ((ya, ya + (yb - ya) * r.uniform(0.18, 0.32)), (yb - (yb - ya) * r.uniform(0.18, 0.32), yb)):
        n = max(2, int((b - a) / 0.12))
        for i in range(n):
            y0 = a + (b - a) * i / n; y1 = a + (b - a) * (i + 1) / n
            mb.boxab((x + (0.03 if i % 2 else 0.0), y0, za), (x + (0.06 if i % 2 else 0.03), y1, zb), FM['curtain'])

def leaf(mb, base, az, elev, L, W, droop, m, shape='oval', segs=3, roll=0.0):
    """Một lá thật (hai mặt): gân giữa cong — góc nâng elev giảm dần droop·s^1,5 theo chiều dài (vươn ra rồi rủ xuống),
    phiến lá gập nhẹ theo gân (mặt cắt chữ V); shape: 'oval' (trầu bà), 'strap' (lá dải dài), 'heart' (lá tim nhỏ)."""
    mi = mb._mi(m); p = Vector(base); dirs = []; pts = [p.copy()]
    for k in range(segs):
        s = (k + 0.5) / segs; e = elev - droop * s ** 1.5
        d = Vector((math.cos(e) * math.cos(az), math.cos(e) * math.sin(az), math.sin(e)))
        p = p + d * (L / segs); pts.append(p.copy()); dirs.append(d)
    dirs.append(dirs[-1])
    side0 = Vector((-math.sin(az), math.cos(az), 0.0))
    rows = []
    for k, c in enumerate(pts):
        s = k / segs
        if shape == 'strap': wv = W * (0.55 + 0.45 * math.sin(math.pi * min(1.0, s * 1.4 + 0.15))) * (1 - s ** 3)
        elif shape == 'heart': wv = W * (0.6 if k == 0 else math.sin(math.pi * (0.25 + 0.75 * s)) ** 0.8) * (1 - s ** 4)
        else: wv = W * (0.15 if k == 0 else math.sin(math.pi * s) ** 0.7)
        d = dirs[min(k, len(dirs) - 1)]
        side = (side0 * math.cos(roll) + d.cross(side0).normalized() * math.sin(roll)).normalized()
        nrm = side.cross(d).normalized()
        if nrm.z < 0: nrm = -nrm
        fold = nrm * (wv * 0.18)
        rows.append((c - side * (wv / 2) - fold * 0.0, c + fold * 0.6, c + side * (wv / 2), nrm))
    for off, flip in ((0.0, False), (-0.004, True)):
        V = [[mb.bm.verts.new(q + r_[3] * off) for q in r_[:3]] for r_ in rows]
        for k in range(segs):
            a, b = V[k], V[k + 1]
            for f in ((a[0], b[0], b[1], a[1]), (a[1], b[1], b[2], a[2])):
                f = tuple(reversed(f)) if flip else f
                face = mb.bm.faces.new(f); face.material_index = mi; face.smooth = True

def plant_broad(mb, x, y, z, r, m):
    """Trầu bà / lá rộng: 7–9 lá bầu dục mọc từ gốc, lá trong dựng, lá ngoài vươn xa rủ xuống."""
    n = r.randint(9, 11)
    for k in range(n):
        az = 2 * math.pi * k / n + r.uniform(-0.3, 0.3); inner = k % 3 == 0
        h = r.uniform(0.02, 0.16) if not inner else r.uniform(0.14, 0.26)
        leaf(mb, (x + math.cos(az) * 0.02, y + math.sin(az) * 0.02, z + h), az, math.radians(r.uniform(55, 72) if inner else r.uniform(25, 45)),
             r.uniform(0.26, 0.38), r.uniform(0.14, 0.2), r.uniform(0.5, 1.1), m, 'oval', roll=r.uniform(-0.3, 0.3))

def plant_strap(mb, x, y, z, r, m):
    """Lan ý / cỏ lá dải: 10–13 lá dài hẹp vươn cao rồi cong rủ."""
    n = r.randint(12, 15)
    for k in range(n):
        az = 2 * math.pi * k / n + r.uniform(-0.25, 0.25)
        leaf(mb, (x, y, z + r.uniform(0.0, 0.05)), az, math.radians(r.uniform(58, 84)), r.uniform(0.4, 0.6),
             r.uniform(0.045, 0.06), r.uniform(1.3, 2.0), m, 'strap', segs=3, roll=r.uniform(-0.4, 0.4))

def trailing(mb, x_edge, y, z_top, r, m):
    """Dây trầu bà rủ qua mép chậu: thân dây mảnh + 5–7 lá tim so le hai bên."""
    n = r.randint(5, 7); Ld = r.uniform(0.45, 0.75); pts = []
    for k in range(n + 1):
        s = k / n
        pts.append(Vector((x_edge + 0.06 * math.sin(min(1, s * 3) * math.pi / 2) + r.uniform(-0.01, 0.01),
                           y + r.uniform(-0.02, 0.02) + s * r.uniform(-0.06, 0.06), z_top + 0.03 - Ld * s ** 1.3)))
    for a, b in zip(pts, pts[1:]): mb.rod(a, b, 0.005, m, seg=3)
    for k in range(1, n + 1):
        side = 1 if k % 2 else -1
        az = math.radians(side * r.uniform(50, 80))                                         # lá chĩa ra phố (+x) lệch hai bên
        leaf(mb, pts[k], az, math.radians(r.uniform(-20, 15)), r.uniform(0.07, 0.11), r.uniform(0.06, 0.08), 0.5, m, 'heart', segs=2,
             roll=side * r.uniform(0.2, 0.6))

def shrubs(mb, x0, x1, ya, yb, z, r, dens=0.26, trail=True):
    """Cây trong bồn: xen trầu bà lá rộng và lan ý lá dải (hai sắc xanh), dây trầu bà rủ qua mép trước (x1)."""
    n = max(2, int((yb - ya) / dens))
    for i in range(n):
        y = ya + (yb - ya) * (i + 0.5) / n + r.uniform(-0.04, 0.04)
        xc = (x0 + x1) / 2 + r.uniform(-0.03, 0.03)
        m = FM['leaves'] if r.random() < 0.55 else FM['leaves2']
        (plant_broad if r.random() < 0.6 else plant_strap)(mb, xc, y, z, r, m)
    k = int((yb - ya) / 0.42) if trail else 0
    for i in range(k):
        if r.random() < 0.7:
            trailing(mb, x1 + 0.02, ya + (yb - ya) * (i + 0.5) / k, z, r, FM['leaves2'] if i % 2 else FM['leaves'])

def planter_box(mb, x0, x1, ya, yb, z, h, r, trail=True):
    mb.boxab((x0, ya, z), (x1, yb, z + h), FM['planter'])
    mb.boxab((x0 + 0.03, ya + 0.03, z + h - 0.02), (x1 - 0.03, yb - 0.03, z + h - 0.005), FM['soil'])
    shrubs(mb, x0 + 0.03, x1 - 0.03, ya + 0.1, yb - 0.1, z + h - 0.03, r, trail=trail)

def flat_railing(mb, x, ya, yb, z):
    """Lan can hiện đại: tay vịn nhôm phẳng 0,07 × 0,045, thanh dưới, thanh dẹt đứng 0,012 × 0,04 cách 0,1 m."""
    A = FM['alu']
    mb.boxab((x - 0.035, ya, z + 1.03), (x + 0.035, yb, z + 1.075), A)
    mb.boxab((x - 0.02, ya, z + 0.08), (x + 0.02, yb, z + 0.12), A)
    n = max(2, int((yb - ya) / 0.1))
    for i in range(1, n):
        y = ya + (yb - ya) * i / n
        mb.boxab((x - 0.02, y - 0.006, z + 0.12), (x + 0.02, y + 0.006, z + 1.03), A)

def louvre_screen(mb, x, ya, yb, za, zb):
    """Màn lam nhôm đứng (vân gỗ) trước tường: thanh 0,05 × 0,2 cách 0,18 m, khung trên dưới."""
    n = max(2, int((yb - ya) / 0.18))
    for i in range(n + 1):
        y = ya + (yb - ya) * i / n
        mb.boxab((x, y - 0.025, za), (x + 0.2, y + 0.025, zb), FM['wood'])
    for zz in (za - 0.05, zb):
        mb.boxab((x - 0.02, ya - 0.03, zz), (x + 0.22, yb + 0.03, zz + 0.05), FM['alu'])

# ---------------------------------------------------------------- căn nhà
def shophouse2(C, y0, w, seed, sign=None, depth=16.0, x_face=None, sign_slot=False):
    r = random.Random(seed)
    xf = (-ROAD_HW - WALK) if x_face is None else x_face
    floors = r.choice([3, 4, 4, 5, 5, 6])                                   # như shophouse(): giữ chiều cao + màu tường
    gh, fh = 4.2, 3.3
    H = gh + fh * (floors - 1)
    fac = M['facade_' + r.choice(['cream', 'gray', 'yellowish', 'offwhite', 'olive', 'pinkish'])]
    rs = random.Random(seed * 7919 + 13)                                     # ngẫu nhiên riêng cho chi tiết mặt tiền
    mb = MB('OW_NhaPho_%03d' % seed)
    yc = y0 + w / 2; t = 0.22; DF = 1.2; PW = 0.3
    F = FM['frame']; A = FM['alu']
    has_sign = bool(sign or sign_slot)
    featured = bool(sign) and (sign[3] is not None or chr(10) in sign[2] or (len(sign) > 7 and bool(sign[7])))   # biển lớn 2,3 m (tới z 6,0)
    # khối nhà: tầng trệt lùi 0,35 như cũ; tầng trên lùi t (mặt sau má cửa)
    mb.boxab((xf - depth, y0 + 0.03, 0.15), (xf - 0.35, y0 + w - 0.03, gh), fac)
    mb.boxab((xf - depth, y0 + 0.03, gh), (xf - t, y0 + w - 0.03, H), fac)
    # ---- tầng trệt (giữ như cũ)
    for yy in (y0 + 0.2, y0 + w - 0.2):
        mb.box((xf - 0.2, yy, (gh + 0.15) / 2), (0.4, 0.4, gh - 0.15), fac)
    glow = M['shop'] if r.random() < 0.7 else M['shop_cool']
    mb.boxab((xf - 0.42, y0 + 0.4, 0.15), (xf - 0.38, y0 + w - 0.4, 3.4), glow)
    nmull = max(2, int((w - 0.8) / 1.3))
    for i in range(nmull + 1):
        yy = y0 + 0.4 + i * (w - 0.8) / nmull
        mb.box((xf - 0.36, yy, 1.78), (0.06, 0.07, 3.25), M['frame_dark'])
    mb.box((xf - 0.36, yc, 2.6), (0.06, w - 0.8, 0.06), M['frame_dark'])
    mb.box((xf - 0.36, yc, 3.42), (0.08, w - 0.8, 0.1), M['frame_dark'])
    mb.boxab((xf - 0.4, y0 + 0.1, 0.15), (xf + 0.1, y0 + w - 0.1, 0.27), M['concrete'])
    mb.boxab((xf - 0.35, y0 + 0.03, gh - 0.3), (xf, y0 + w - 0.03, gh), fac)            # dầm ô văng dưới tầng 2
    # ---- khung bao nhô DF: hai cột + ô mái đỉnh (căn có biển: khung bắt đầu trên mép biển, có tấm đáy)
    zf0 = gh + 1.9 if featured else (gh + 1.0 if has_sign else gh - 0.3)
    ya_in, yb_in = y0 + 0.03 + PW, y0 + w - 0.03 - PW
    for a, b in ((y0 + 0.03, ya_in), (yb_in, y0 + w - 0.03)):
        mb.boxab((xf - t, a, zf0), (xf + DF, b, H + 0.95), F)
        mb.boxab((xf + DF, a + 0.08, zf0 + 0.25), (xf + DF + 0.012, b - 0.08, H + 0.3), A)   # nẹp nhôm mặt cột
    mb.boxab((xf - t, y0 + 0.03, H + 0.35), (xf + DF, y0 + w - 0.03, H + 0.95), F)                  # ô mái đỉnh
    mb.boxab((xf + DF, y0 + 0.03, H + 0.55), (xf + DF + 0.012, y0 + w - 0.03, H + 0.6), A)          # chỉ nhôm tối
    if has_sign: mb.boxab((xf, ya_in, zf0), (xf + DF, yb_in, zf0 + 0.16), F)
    for k in range(3):                                                                                 # đèn hắt dưới ô mái
        mb.cyl((xf + DF * 0.55, ya_in + (yb_in - ya_in) * (k + 0.5) / 3, H + 0.33), 0.06, 0.02, FM['lamp'], seg=8)
    # ---- các tầng trên
    bal = [False] + [rs.random() < 0.62 and not (has_sign and f == 1) for f in range(1, floors)]
    if floors > 2 and not any(bal): bal[floors - 1] = True                                          # mỗi căn có ít nhất một ban công
    louvre = w >= 6.9 and rs.random() < 0.7
    for f in range(1, floors):
        z = gh + (f - 1) * fh; z1 = z + fh
        if bal[f]:
            # sàn ban công trong khung: mép sàn, gạch lát
            mb.boxab((xf, ya_in, z - 0.02), (xf + DF - 0.04, yb_in, z + 0.14), FM['concrete'])
            mb.boxab((xf + DF - 0.06, ya_in, z - 0.12), (xf + DF, yb_in, z + 0.16), F)
            mb.boxab((xf, ya_in + 0.02, z + 0.14), (xf + DF - 0.07, yb_in - 0.02, z + 0.15), FM['tile_floor'])
            # cửa kính trượt sát trần
            da, db = ya_in + 0.12, yb_in - 0.12
            skin_with_openings(mb, xf, ya_in, yb_in, z, z1, [(da, db, z + 0.15, z + 2.95)], fac, t)
            n = 4 if db - da > 2.6 else 2
            glazing(mb, xf - 0.06, da, db, z + 0.15, z + 2.95, n, False, rs, sliding=True)
            mb.boxab((xf - 0.08, da, z + 0.15), (xf + 0.02, db, z + 0.19), A)          # ray dưới
            # lan can + bồn cây dài sau lan can
            flat_railing(mb, xf + DF - 0.12, ya_in, yb_in, z + 0.15)
            pl0 = ya_in + 0.15; pl1 = pl0 + min(2.2, (yb_in - ya_in) * 0.55)
            if rs.random() < 0.5: pl0, pl1 = yb_in - 0.15 - (pl1 - pl0), yb_in - 0.15
            planter_box(mb, xf + DF - 0.55, xf + DF - 0.2, pl0, pl1, z + 0.15, 0.42, rs, trail=False)
            mb.cyl((xf + DF * 0.5, yc, z1 - 0.32), 0.1, 0.03, FM['lamp'], seg=10)          # đèn ốp trần (dưới sàn trên)
            if rs.random() < 0.45:                                                       # bàn trà nhỏ
                y = rs.uniform(ya_in + 0.6, yb_in - 0.6)
                mb.box((xf + 0.5, y, z + 0.6), (0.5, 0.5, 0.03), FM['wood'])
                mb.box((xf + 0.5, y, z + 0.38), (0.05, 0.05, 0.44), A)
        else:
            # cửa sổ kính lớn trong lỗ tường
            span0, span1 = ya_in + 0.15, yb_in - 0.15
            if louvre: span1 = ya_in + (yb_in - ya_in) * 0.62
            nwin = 1 if span1 - span0 < 3.4 else 2
            holes = []
            za = z + ((1.95 if featured else 1.05) if (f == 1 and has_sign) else 0.85); zb = z + 2.75
            # 21.289 sửa: tầng có biển — mọi chi tiết mặt tiền bắt đầu trên tấm đáy khung (zf0 + 0,16), không thò xuống vùng biển
            zlow = zf0 + 0.18 if (f == 1 and has_sign) else z
            for i in range(nwin):
                a = span0 + (span1 - span0) * i / nwin + (0.2 if nwin > 1 and i else 0)
                b = span0 + (span1 - span0) * (i + 1) / nwin - (0.2 if nwin > 1 and i < nwin - 1 else 0)
                holes.append((a, b, za, zb))
            skin_with_openings(mb, xf, ya_in, yb_in, z, z1, holes, fac, t)
            for a, b, za_, zb_ in holes:
                nn = 4 if b - a > 2.2 else (3 if b - a > 1.5 else 2)
                glazing(mb, xf - 0.07, a, b, za_, zb_, nn, True, rs)
                mb.boxab((xf - 0.2, a - 0.04, za_ - 0.05), (xf + 0.08, b + 0.04, za_), A)            # bậu nhôm
                mb.boxab((xf, a - 0.25, zb_ + 0.12), (xf + 0.7, b + 0.25, zb_ + 0.24), F)            # ô văng phẳng
                mb.boxab((xf + 0.66, a - 0.25, zb_ + 0.08), (xf + 0.7, b + 0.25, zb_ + 0.12), A)
                if za_ - z > 0.5:                                                                   # ốp lam gỗ dưới bậu
                    zz = max(z + 0.2, zlow)
                    while zz < za_ - 0.12:
                        mb.boxab((xf, a, zz), (xf + 0.03, b, zz + 0.07), FM['wood']); zz += 0.1
                if rs.random() < 0.55 and not (f == 1 and has_sign):                               # bồn cây treo ngoài bậu
                    planter_box(mb, xf + 0.08, xf + 0.42, a + 0.1, b - 0.1, za_ - 0.42, 0.34, rs)
                    for y in (a + 0.2, b - 0.2): mb.boxab((xf, y - 0.02, za_ - 0.48), (xf + 0.35, y + 0.02, za_ - 0.42), A)
            if louvre:
                louvre_screen(mb, xf + 0.02, span1 + 0.25, yb_in - 0.1, max(z + 0.3, zlow + 0.06), z1 - 0.3)
        if not (f == 1 and has_sign):                                                # dải sàn lộ ra mặt tiền (tầng có biển: biển che)
            mb.boxab((xf - 0.02, ya_in, z - 0.12), (xf + 0.08, yb_in, z + 0.04), F)
    # ---- tường mái lùi + bồn nước + mái tôn
    mb.boxab((xf - 0.6, y0 + 0.03, H), (xf - t, y0 + w - 0.03, H + 0.35), fac)
    if r.random() < 0.6: mb.boxab((xf - depth, y0 + 0.03, H), (xf - 0.6, y0 + w - 0.03, H + 0.9), fac)
    if r.random() < 0.6: mb.cyl((xf - 3.5, yc, H + 0.9), 0.6, 1.3, M['tank'], seg=16)
    if r.random() < 0.4:
        mb.box((xf - 6, yc, H + 2.1), (5.5, w - 0.4, 0.08), M['roof_tin'], ry=0.08)
        for yy in (y0 + 0.4, y0 + w - 0.4):
            mb.rod((xf - 3.6, yy, H + 0.9), (xf - 3.6, yy, H + 2.3), 0.05, M['iron'])
    aw = M['awning'] if r.random() < 0.65 else M['awning_dark']
    mb.box((xf + 0.85, yc, 3.25), (1.9, w - 0.35, 0.05), aw, ry=0.32)
    mb.box((xf + 1.75, yc, 2.88), (0.04, w - 0.35, 0.25), aw)
    ob = mb.finish(C)
    if sign:
        featured = sign[3] is not None or chr(10) in sign[2] or (len(sign) > 7 and sign[7])
        sign_board(C, xf, y0 + 0.15, w - 0.3, *sign[2:7], z=4.85 if featured else 4.3, h=2.3 if featured else 1.6)
    return H
