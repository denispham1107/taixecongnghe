# Đợt 2 (21.239): đoạn mẫu đường Tôn Đức Thắng theo ảnh tham chiếu 3 (ref-3-pho-ven-song.webp).
# Gốc toạ độ cục bộ = điểm bản đồ (x 215, z −480) trong layout.json. x = đông, y = bắc (Unity z), z = lên.
# Đường R2 chạy dọc +y, nhà phố bên trái (x < −13), sông bên phải (x > 25).
# Chạy trong Blender:  ns={'OW_DIR': r'...\ArtSource\Blender\OpenWorld'}; exec(open(...).read(), ns)
import bpy, math, random, os
from mathutils import Vector

OW_DIR = globals().get('OW_DIR') or os.path.dirname(os.path.abspath(__file__))
_lib = os.path.join(OW_DIR, 'ow_lib.py')
exec(compile(open(_lib, encoding='utf-8').read(), _lib, 'exec'), globals())

PARTS = globals().get('PARTS') or ['scene', 'ground', 'river', 'west', 'east', 'trees', 'props', 'skyline', 'traffic', 'camera']
ROOT = 'OW_Ref3_TonDucThang'

Y0, Y1 = -40.0, 720.0
ROAD_HW = 7.0          # nửa lòng đường chính 14 m
WALK = 6.0             # vỉa hè chính
BANK_X = 19.5          # mép kè sông (map x 234,5 — hẹp lại cho giống ảnh 3)
FAR_BANK_X = 185.0     # bờ đông (map x 400)
NORTH_BANK_Y = 720.0   # bờ nam nhánh sông ngang (map z 240)
NORTH_FAR_Y = 880.0    # bờ bắc (map z 400)
WATER_Z = -2.2
# Đường nhánh phía tây (tâm y, nửa bề rộng gồm vỉa hè)
CROSS_W = [(180.0, 7.5), (330.0, 13.0), (480.0, 7.5), (580.0, 7.5), (695.0, 13.0)]

rnd = random.Random(20261117)

# ---------------------------------------------------------------- vật liệu dùng chung
M = {}
def mats():
    M['asphalt'] = mat_noise('OW_Asphalt', '#2C2E30', '#3E4042', scale=1.2, rough=0.88, bump=0.25, kind='Object')
    M['asphalt_patch'] = mat_noise('OW_AsphaltPatch', '#26282A', '#33363A', scale=0.4, rough=0.9, bump=0.2)
    M['white'] = mat('OW_PaintWhite', '#E8E6DF', 0.55)
    M['yellow'] = mat('OW_PaintYellow', '#E6B21C', 0.55)
    M['curb'] = mat_noise('OW_Curb', '#9A9890', '#7C7A73', scale=4, rough=0.85, bump=0.2)
    M['paving'] = mat_tiles('OW_Paving', '#8D8B85', '#7E7C76', '#55534E', size=0.6)
    M['paving_big'] = mat_tiles('OW_PavingBig', '#A3A19A', '#8F8D86', '#5E5C57', size=1.0)
    M['concrete'] = mat_noise('OW_Concrete', '#8B8780', '#6F6B64', scale=2.5, rough=0.9, bump=0.2)
    M['bank'] = mat_noise('OW_BankStone', '#6E6A60', '#4D4A43', scale=3, rough=0.95, bump=0.4)
    M['water'] = mat_water('OW_RiverWater')
    M['iron'] = mat('OW_IronBlack', '#141617', 0.45, 0.6)
    M['metal_gray'] = mat('OW_MetalGray', '#6E7274', 0.4, 0.7)
    M['soil'] = mat_noise('OW_Soil', '#3B2E22', '#2A2018', scale=6, rough=1.0, bump=0.3)
    M['grass'] = mat_noise('OW_Grass', '#3E5F25', '#5B7A2F', scale=8, rough=0.9, bump=0.3)
    M['bark'] = mat_noise('OW_Bark', '#3A3128', '#5A4D3E', scale=12, rough=0.95, bump=0.6)
    M['leaves'] = mat_leaves('OW_LeavesTamarind', '#2B4F1C', '#6A8C2B')
    M['leaves2'] = mat_leaves('OW_LeavesDark', '#24451A', '#4F7426')
    M['shop'] = mat_shopglass('OW_ShopGlow')
    M['shop_cool'] = mat_shopglass('OW_ShopGlowCool', '#FFE2B0', 2.8)
    M['window'] = mat('OW_WindowGlass', '#1C2A30', 0.08, 0.3)
    M['frame_white'] = mat('OW_FrameWhite', '#E9E6DE', 0.5)
    M['frame_dark'] = mat('OW_FrameDark', '#2B2B28', 0.5, 0.3)
    M['awning'] = mat_noise('OW_AwningCanvas', '#D8D3C6', '#B9B3A4', scale=3, rough=0.85, bump=0.05)
    M['awning_dark'] = mat_noise('OW_AwningGrey', '#8F918C', '#77797A', scale=3, rough=0.85, bump=0.05)
    M['ac'] = mat('OW_AirCon', '#D9D9D2', 0.45)
    M['tank'] = mat('OW_SteelTank', '#B9BEBF', 0.25, 0.9)
    M['roof_tin'] = mat_noise('OW_RoofTin', '#6F6458', '#8A7A66', scale=2, rough=0.6, bump=0.2)
    M['pot'] = mat_noise('OW_PlanterConcrete', '#A39E92', '#87827A', scale=3, rough=0.9, bump=0.2)
    M['pot_terra'] = mat('OW_PotTerracotta', '#9E5A3A', 0.8)
    M['sign_white'] = mat('OW_SignTextWhite', '#F7F4EC', 0.4, emit='#FFFFFF', emit_strength=0.3)
    M['sign_dark'] = mat('OW_SignTextDark', '#2A1A12', 0.5)
    M['lamp'] = mat('OW_LampBulb', '#FFF2D0', 0.3, emit='#FFE3A8', emit_strength=6.0)
    M['chair'] = mat('OW_CafeChairWood', '#6B4A30', 0.7)
    M['table'] = mat('OW_CafeTable', '#3E3A36', 0.5, 0.4)
    M['red'] = mat('OW_UmbrellaRed', '#B8231F', 0.7)
    M['cart_yellow'] = mat('OW_CartYellow', '#E8B92A', 0.5)
    M['flag_red'] = mat('OW_FlagRed', '#DA251D', 0.7, emit='#DA251D', emit_strength=0.35)  # tự sáng nhẹ để đỏ tươi cả khi khuất nắng
    M['flag_star'] = mat('OW_FlagStar', '#FFCD00', 0.5, emit='#FFCD00', emit_strength=0.25)
    M['chevron_bg'] = mat('OW_ChevronBlack', '#121212', 0.5)
    M['chevron'] = mat('OW_ChevronYellow', '#F2C21A', 0.4, emit='#F2C21A', emit_strength=0.4)
    M['sign_blue'] = mat('OW_RoadSignBlue', '#1D4E9E', 0.5)
    for k, hx in {'cream': '#D9CDB3', 'gray': '#B9B5AB', 'yellowish': '#D6C38E', 'offwhite': '#D4D0C6', 'olive': '#AAA48E', 'pinkish': '#CBB19F'}.items():
        M['facade_' + k] = mat_weathered('OW_Facade_' + k, hx)

# ---------------------------------------------------------------- 1. cảnh, ánh sáng
def setup_scene():
    sc = bpy.context.scene
    for name in ('Cube', 'Light', 'Camera'):
        ob = bpy.data.objects.get(name)
        if ob: bpy.data.objects.remove(ob, do_unlink=True)
    try: sc.render.engine = 'BLENDER_EEVEE'
    except TypeError as e: print(e)
    ee = sc.eevee
    for attr, val in (('use_raytracing', True), ('use_shadows', True), ('shadow_ray_count', 2), ('use_volumetric_shadows', False)):
        try: setattr(ee, attr, val)
        except Exception: pass
    try: sc.view_settings.view_transform = 'AgX'
    except TypeError: pass
    try: sc.view_settings.look = 'AgX - Medium High Contrast'
    except TypeError: pass
    sc.view_settings.exposure = 0.25
    sc.render.resolution_x, sc.render.resolution_y = 1672, 941
    sc.render.film_transparent = False
    sc.unit_settings.system = 'METRIC'
    # Trời chiều: mặt trời thấp phía đông nam, sau lưng bên phải camera (bóng cây đổ chéo qua đường như ảnh 3).
    w = sc.world or bpy.data.worlds.new('World'); sc.world = w; w.use_nodes = True
    nt = w.node_tree; nt.nodes.clear()
    out = nt.nodes.new('ShaderNodeOutputWorld'); bg = nt.nodes.new('ShaderNodeBackground')
    sky = nt.nodes.new('ShaderNodeTexSky')
    try: sky.sky_type = 'MULTIPLE_SCATTERING'
    except TypeError: pass
    for attr, val in (('sun_elevation', math.radians(16)), ('sun_rotation', math.radians(-60)), ('altitude', 50.0), ('air_density', 1.2), ('dust_density', 1.6), ('sun_disc', False)):
        try: setattr(sky, attr, val)
        except Exception: pass
    bg.inputs['Strength'].default_value = 0.06
    nt.links.new(sky.outputs['Color'], bg.inputs['Color']); nt.links.new(bg.outputs['Background'], out.inputs['Surface'])
    sun = bpy.data.objects.get('OW_Sun')
    if sun is None:
        ld = bpy.data.lights.new('OW_Sun', 'SUN'); sun = bpy.data.objects.new('OW_Sun', ld); sc.collection.objects.link(sun)
    sun.data.energy = 7.0; sun.data.color = (1.0, 0.74, 0.46); sun.data.angle = math.radians(1.2)
    # Hướng chiếu: từ đông nam (x+, y−) xuống tây bắc.
    # Ảnh 3: nắng chiều thấp ở phía trước bên phải (đông bắc), mặt tiền dãy trái được nắng, bóng cây đổ chéo qua đường.
    d = Vector((-0.80, -0.42, -0.30)).normalized()
    sun.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()

# ---------------------------------------------------------------- 2. mặt đường, vỉa hè
def in_cross(y, pad=0.0):
    for cy, hw in CROSS_W:
        if abs(y - cy) < hw + pad: return (cy, hw)
    return None

def west_runs(y0, y1, pad=0.0):
    """Các đoạn y liên tục bên tây, bỏ qua miệng đường nhánh."""
    runs, cur = [], y0
    for cy, hw in sorted(CROSS_W):
        a, b = cy - hw - pad, cy + hw + pad
        if b <= y0 or a >= y1: continue
        if a > cur: runs.append((cur, a))
        cur = max(cur, b)
    if cur < y1: runs.append((cur, y1))
    return runs

def build_ground(C):
    mb = MB('OW_R2_MatDuong')
    mb.boxab((-ROAD_HW, Y0, -0.3), (ROAD_HW, Y1 + 30, 0.0), M['asphalt'])
    # Miệng đường nhánh phía tây + R3 ở cuối.
    for cy, hw in CROSS_W:
        cw = hw - (WALK if hw > 10 else 4.0)
        mb.boxab((-90, cy - cw, -0.3), (-ROAD_HW, cy + cw, 0.0), M['asphalt'])
    cy, hw = CROSS_W[-1]; cw = hw - WALK
    mb.boxab((ROAD_HW, cy - cw, -0.3), (BANK_X + 2, cy + cw, 0.0), M['asphalt'])
    # Vá nhựa đường sẫm màu rải rác.
    for i in range(26):
        y = rnd.uniform(Y0, Y1); x = rnd.uniform(-6, 6)
        mb.box((x, y, 0.004), (rnd.uniform(0.8, 2.5), rnd.uniform(1.0, 4.0), 0.006), M['asphalt_patch'], rz=rnd.uniform(-0.2, 0.2))
    mb.finish(C)

    lm = MB('OW_R2_VachSon')
    y = Y0
    while y < Y1:
        if not in_cross(y, 2.0):
            for x in (-0.13, 0.13): lm.box((x, y + 1.5, 0.006), (0.12, 3.0, 0.01), M['yellow'])
        y += 3.0
    # Vạch đôi vàng liền: nối bằng dải dài cho phần ngoài giao lộ.
    for a, b in west_runs(Y0, Y1, 2.0):
        for x in (-0.13, 0.13): lm.box((x, (a + b) / 2, 0.006), (0.12, b - a, 0.01), M['yellow'])
    for x in (-3.5, 3.5):
        y = Y0
        while y < Y1:
            if not in_cross(y, 3.0): lm.box((x, y + 1.5, 0.006), (0.13, 3.0, 0.01), M['white'])
            y += 9.0
    for a, b in west_runs(Y0, Y1, 1.0):
        lm.box((-6.75, (a + b) / 2, 0.006), (0.15, b - a, 0.01), M['white'])
    lm.box((6.75, (Y0 + 682) / 2, 0.006), (0.15, 682 - Y0, 0.01), M['white'])
    # Vạch người đi bộ ở hai giao lộ lớn.
    for cy, hw in CROSS_W:
        if hw < 10: continue
        for k in range(12):
            lm.box((-6.0 + k * 1.1, cy - hw + 1.6, 0.006), (0.55, 2.6, 0.01), M['white'])
    lm.finish(C)

    sw = MB('OW_R2_ViaHe')
    for a, b in west_runs(Y0, Y1):
        sw.boxab((-ROAD_HW - WALK, a, 0.0), (-ROAD_HW - 0.25, b, 0.15), M['paving'])
        sw.boxab((-ROAD_HW - 0.25, a, -0.05), (-ROAD_HW, b, 0.17), M['curb'])
        # Bồn cây vuông trên vỉa hè.
    # Vỉa hè + lối đi ven sông phía đông.
    sw.boxab((ROAD_HW + 0.25, Y0, 0.0), (BANK_X, 682, 0.15), M['paving_big'])
    sw.boxab((ROAD_HW, Y0, -0.05), (ROAD_HW + 0.25, 682, 0.17), M['curb'])
    # Dải cây xanh/hoa giữa vỉa hè và lối đi ven sông (x 14..17), có gờ bê tông.
    y = Y0 + 4
    while y < 676:
        L = rnd.uniform(14, 22)
        if y + L > 676: break
        sw.boxab((14.0, y, 0.15), (16.4, y + L, 0.32), M['concrete'])
        sw.boxab((14.15, y + 0.15, 0.32), (16.25, y + L - 0.15, 0.62), M['leaves2'])   # bờ rào lá thấp
        y += L + 4.0
    # Vỉa hè phía bắc R3 (cuối tầm nhìn) + dải kè.
    sw.boxab((-90, 702, 0.0), (BANK_X + 2, 708, 0.15), M['paving'])
    sw.boxab((-90, 708, 0.0), (BANK_X + 2, NORTH_BANK_Y, 0.15), M['paving_big'])
    sw.finish(C)

    # Nền đất phía sau nhà (để không thấy khoảng trống) .
    g = MB('OW_R2_NenPhiaSau')
    g.boxab((-140, Y0 - 40, -0.4), (-ROAD_HW - WALK, NORTH_BANK_Y, 0.0), M['concrete'])
    g.finish(C)

# ---------------------------------------------------------------- 3. sông, kè, lan can
def build_river(C):
    mb = MB('OW_Song')
    mb.boxab((BANK_X, Y0 - 300, WATER_Z - 3), (FAR_BANK_X, NORTH_FAR_Y + 400, WATER_Z), M['water'])
    mb.boxab((-700, NORTH_BANK_Y, WATER_Z - 3), (BANK_X, NORTH_FAR_Y, WATER_Z), M['water'])
    mb.finish(C)
    k = MB('OW_KeSong')
    k.boxab((BANK_X - 0.6, Y0 - 300, WATER_Z - 1), (BANK_X, NORTH_BANK_Y, 0.15), M['bank'])
    k.boxab((-700, NORTH_BANK_Y - 0.6, WATER_Z - 1), (BANK_X, NORTH_BANK_Y, 0.15), M['bank'])
    # Gờ đá đỉnh kè.
    k.boxab((BANK_X - 0.7, Y0 - 300, 0.15), (BANK_X + 0.1, NORTH_BANK_Y, 0.3), M['concrete'])
    k.boxab((-700, NORTH_BANK_Y - 0.7, 0.15), (BANK_X + 0.1, NORTH_BANK_Y + 0.1, 0.3), M['concrete'])
    k.finish(C)
    # Lan can sắt ven sông: trụ 2,5 m, tay vịn trên/dưới, song đứng 0,14 m.
    rl = MB('OW_LanCanSong')
    def railing(a, b):
        a, b = Vector(a), Vector(b); d = b - a; L = d.length; t = d.normalized()
        n = int(L / 2.5)
        for i in range(n + 1):
            p = a + t * (i * L / n)
            rl.box((p.x, p.y, 0.3 + 0.6), (0.09, 0.09, 1.2), M['iron'])
            rl.box((p.x, p.y, 1.52), (0.13, 0.13, 0.05), M['iron'])
        for z in (0.42, 1.12):
            c = (a + b) / 2; rl.box((c.x, c.y, z), (0.05 if abs(t.y) > 0.5 else L, L if abs(t.y) > 0.5 else 0.05, 0.05), M['iron'])
        rl.box(((a + b) / 2 + Vector((0, 0, 1.47))), (0.08 if abs(t.y) > 0.5 else L, L if abs(t.y) > 0.5 else 0.08, 0.06), M['iron'])
        m = int(L / 0.14)
        for i in range(m):
            p = a + t * (i * L / m)
            rl.box((p.x, p.y, 0.77), (0.022, 0.022, 0.7), M['iron'])
    railing((BANK_X - 0.3, Y0 - 60, 0), (BANK_X - 0.3, 682, 0))
    railing((-90, NORTH_BANK_Y - 0.3, 0), (BANK_X - 0.3, NORTH_BANK_Y - 0.3, 0))
    rl.finish(C)

# ---------------------------------------------------------------- 4. nhà phố phía tây
SIGNS_WEST = [
    # (y bắt đầu, bề ngang, tên, phụ đề, màu nền, màu chữ)
    (6.0, 8.0, 'Phở Thịnh', 'Phở Bò - Phở Gà', '#6A2232', 'white', 'timesbd.ttf'),
    (14.0, 6.0, 'Nhà Sách\nMinh Tâm', None, '#E3B323', 'dark', 'arialbd.ttf'),
    (20.0, 5.5, 'Điện Máy\nHải Đăng', None, '#2154A6', 'white', 'arialbd.ttf'),
    (25.5, 5.5, 'Cà Phê\nSài Gòn', None, '#2B7A3B', 'white', 'arialbd.ttf'),
    (31.0, 6.0, 'Tạp Hóa\nThanh Hương', None, '#B3362C', 'white', 'arialbd.ttf'),
    (37.0, 5.0, 'Tiệm Vàng\nKim Phát', None, '#C9952A', 'dark', 'arialbd.ttf'),
]

def sign_board(C, x_face, y0, w, title, sub, bg, fg, fontfile, z=4.3, h=1.6):
    """Bảng hiệu ngang trên mái hiên, mặt quay về đường (+x)."""
    sb = MB('OW_Bang_' + title.replace('\n', ' '))
    mbg = mat('OW_SignBG_' + bg.strip('#'), bg, 0.55)
    yc = y0 + w / 2
    sb.box((x_face + 0.12, yc, z), (0.2, w - 0.3, h), mbg)
    sb.box((x_face + 0.23, yc, z + h / 2 - 0.04), (0.04, w - 0.3, 0.08), M['frame_white'])
    sb.box((x_face + 0.23, yc, z - h / 2 + 0.04), (0.04, w - 0.3, 0.08), M['frame_white'])
    sb.finish(C)
    tm = M['sign_white'] if fg == 'white' else M['sign_dark']
    rot = (math.pi / 2, 0, math.pi / 2)
    lines = title.count('\n') + 1
    k = h / 1.6
    if sub:
        text(C, title, (x_face + 0.25, yc + 0.5 * k, z + 0.22 * k), 0.72 * k, tm, rot, fontfile)
        text(C, sub, (x_face + 0.25, yc + 0.5 * k, z - 0.5 * k), 0.28 * k, tm, rot, 'arialbd.ttf')
        # Biểu tượng tô phở trắng bên trái chữ (ảnh 3).
        ic = MB('OW_IconBowl_' + title)
        ic.cyl((x_face + 0.24, y0 + 1.1 * k, z), 0.5 * k, 0.04, M['frame_white'], seg=24, axis='X')
        ic.finish(C)
    else:
        text(C, title, (x_face + 0.25, yc, z), (0.52 if lines > 1 else 0.7) * k, tm, rot, fontfile)

def shophouse(C, y0, w, seed, sign=None, depth=16.0, x_face=None, sign_slot=False):
    """sign_slot=True: chừa chỗ bảng hiệu (không ban công tầng 2) dù không dựng bảng — dùng cho thư viện mẫu bản đồ (21.248)."""
    r = random.Random(seed)
    xf = (-ROAD_HW - WALK) if x_face is None else x_face
    floors = r.choice([3, 4, 4, 5, 5, 6])
    gh, fh = 4.2, 3.3
    H = gh + fh * (floors - 1)
    fac = M['facade_' + r.choice(['cream', 'gray', 'yellowish', 'offwhite', 'olive', 'pinkish'])]
    mb = MB('OW_NhaPho_%03d' % seed)
    yc = y0 + w / 2
    # Khối chính lùi 0,05 để tường các nhà kề nhau không z-fight.
    mb.boxab((xf - depth, y0 + 0.03, 0.15), (xf - 0.35, y0 + w - 0.03, H), fac)
    # Tầng trên nhô ra 0,35 (ô văng) — mặt tiền thật.
    mb.boxab((xf - 0.35, y0 + 0.03, gh), (xf, y0 + w - 0.03, H), fac)
    # Trụ hai bên tầng trệt.
    for yy in (y0 + 0.2, y0 + w - 0.2):
        mb.box((xf - 0.2, yy, (gh + 0.15) / 2), (0.4, 0.4, gh - 0.15), fac)
    # Mặt kính tiệm sáng đèn + khung.
    glow = M['shop'] if r.random() < 0.7 else M['shop_cool']
    mb.boxab((xf - 0.42, y0 + 0.4, 0.15), (xf - 0.38, y0 + w - 0.4, 3.4), glow)
    nmull = max(2, int((w - 0.8) / 1.3))
    for i in range(nmull + 1):
        yy = y0 + 0.4 + i * (w - 0.8) / nmull
        mb.box((xf - 0.36, yy, 1.78), (0.06, 0.07, 3.25), M['frame_dark'])
    mb.box((xf - 0.36, yc, 2.6), (0.06, w - 0.8, 0.06), M['frame_dark'])
    mb.box((xf - 0.36, yc, 3.42), (0.08, w - 0.8, 0.1), M['frame_dark'])
    # Bậc thềm.
    mb.boxab((xf - 0.4, y0 + 0.1, 0.15), (xf + 0.1, y0 + w - 0.1, 0.27), M['concrete'])
    # Gờ ngăn tầng.
    for f in range(1, floors):
        z = gh + (f - 1) * fh
        mb.boxab((xf - 0.02, y0 + 0.03, z - 0.08), (xf + 0.12, y0 + w - 0.03, z + 0.08), M['frame_white'])
    # Cửa sổ, ban công, máy lạnh.
    nwin = max(1, int(w / 2.4))
    for f in range(1, floors):
        z = gh + (f - 1) * fh
        balcony = r.random() < 0.6 and not ((sign or sign_slot) and f == 1)   # bảng hiệu lớn chiếm tầng 2: không có ban công che chữ
        if balcony:
            mb.boxab((xf, y0 + 0.25, z), (xf + 1.05, y0 + w - 0.25, z + 0.14), M['concrete'])
            # Lan can sắt ban công.
            mb.boxab((xf + 1.0, y0 + 0.25, z + 1.0), (xf + 1.06, y0 + w - 0.25, z + 1.05), M['iron'])
            mb.boxab((xf + 1.0, y0 + 0.25, z + 0.2), (xf + 1.06, y0 + w - 0.25, z + 0.24), M['iron'])
            nb = int((w - 0.5) / 0.13)
            for i in range(nb):
                mb.box((xf + 1.03, y0 + 0.3 + i * 0.13, z + 0.6), (0.02, 0.02, 0.85), M['iron'])
            for yy in (y0 + 0.25, y0 + w - 0.25):
                mb.boxab((xf, yy - 0.02, z + 0.14), (xf + 1.05, yy + 0.02, z + 1.05), M['iron'])
        for i in range(nwin):
            wy = y0 + (i + 0.5) * w / nwin
            ww = min(1.5, w / nwin - 0.7); wh = 2.0 if balcony else 1.55
            wz = z + 0.25 + wh / 2 if balcony else z + 0.95 + wh / 2
            mb.box((xf + 0.02, wy, wz), (0.08, ww + 0.2, wh + 0.2), M['frame_white'])
            mb.box((xf + 0.04, wy, wz), (0.06, ww, wh), M['window'])
            mb.box((xf + 0.07, wy, wz), (0.04, 0.05, wh), M['frame_white'])
            if not balcony and r.random() < 0.5:   # gờ mái che cửa sổ
                mb.box((xf + 0.25, wy, wz + wh / 2 + 0.18), (0.5, ww + 0.4, 0.08), M['concrete'], ry=-0.25)
        if r.random() < 0.45:
            ay = y0 + r.uniform(0.6, w - 0.6)
            mb.box((xf + (1.25 if balcony else 0.3), ay, z + (0.45 if balcony else 0.6)), (0.32, 0.8, 0.58), M['ac'])
    # Mái: lan can mái, bồn nước, mái tôn che.
    mb.boxab((xf - depth, y0 + 0.03, H), (xf, y0 + w - 0.03, H + 0.9), fac) if r.random() < 0.6 else None
    mb.boxab((xf - 0.05, y0 + 0.03, H + 0.9), (xf + 0.08, y0 + w - 0.03, H + 1.0), M['frame_white'])
    if r.random() < 0.6:
        mb.cyl((xf - 3.5, yc, H + 0.9), 0.6, 1.3, M['tank'], seg=16)
    if r.random() < 0.4:
        mb.box((xf - 6, yc, H + 2.1), (5.5, w - 0.4, 0.08), M['roof_tin'], ry=0.08)
        for yy in (y0 + 0.4, y0 + w - 0.4):
            mb.rod((xf - 3.6, yy, H + 0.9), (xf - 3.6, yy, H + 2.3), 0.05, M['iron'])
    # Mái hiên vải dốc ra đường.
    aw = M['awning'] if r.random() < 0.65 else M['awning_dark']
    mb.box((xf + 0.85, yc, 3.25), (1.9, w - 0.35, 0.05), aw, ry=0.32)
    mb.box((xf + 1.75, yc, 2.88), (0.04, w - 0.35, 0.25), aw)
    ob = mb.finish(C)
    if sign:
        featured = sign[3] is not None or chr(10) in sign[2] or (len(sign) > 7 and sign[7])
        sign_board(C, xf, y0 + 0.15, w - 0.3, *sign[2:7], z=4.85 if featured else 4.3, h=2.3 if featured else 1.6)
    # Không đặt hoa giấy trên nhà (người dùng bỏ 17/11/2026).
    return H

def build_west(C):
    used = []
    # 6 tiệm có bảng như ảnh 3, sau đó nhà phố ngẫu nhiên.
    for s in SIGNS_WEST:
        shophouse(C, s[0], s[1], int(s[0] * 10) + 7, sign=s)
    y = 42.0; seed = 100
    other_signs = [('Bún Bò Huế', '#7E2A1E', 'white'), ('Bánh Mì Sài Gòn', '#E0A321', 'dark'), ('Nhà Thuốc Tây', '#1F7A4A', 'white'),
                   ('Cơm Tấm', '#8A2B2B', 'white'), ('Trà Sữa', '#3E6E9A', 'white'), ('Hủ Tiếu Nam Vang', '#9A3A22', 'white'),
                   ('Sửa Xe Máy', '#2B4A7A', 'white'), ('Cà Phê Muối', '#4E3628', 'white'), ('Bách Hóa Xanh', '#2B8A3E', 'white')]
    k = 0
    for a, b in west_runs(42.0, Y1 - 40):
        y = a + 0.2
        while y < b - 4.5:
            w = rnd.choice([4.5, 5.0, 5.5, 6.0, 7.0, 8.0])
            if y + w > b - 0.2: w = b - 0.2 - y
            if w < 4.0: break
            s = None
            if rnd.random() < 0.55:
                t = other_signs[k % len(other_signs)]; k += 1
                s = (y, w, t[0], None, t[1], t[2], 'arialbd.ttf')
            shophouse(C, y, w, seed, sign=s); seed += 1
            y += w
        # Nhà góc phố nhìn ra đường nhánh: thêm khối cao hơn phía sau.
    # Hàng nhà thứ hai cao hơn phía sau (tạo đường chân trời).
    back = MB('OW_NhaPhiaSau')
    for a, b in west_runs(Y0, Y1):
        y = a
        while y < b - 6:
            w = rnd.uniform(8, 16); h = rnd.uniform(10, 26)
            back.boxab((-ROAD_HW - WALK - 36, y, 0), (-ROAD_HW - WALK - 17, min(b, y + w), h), M['facade_' + rnd.choice(['gray', 'offwhite', 'olive'])])
            y += w + rnd.uniform(0, 3)
    back.finish(C)

# ---------------------------------------------------------------- 5. phía đông: nhà thuốc đầu đoạn + lối ven sông
def build_east(C):
    # Nhà thuốc An Khang ở mép phải gần camera (ảnh 3).
    xf = ROAD_HW + WALK
    mb = MB('OW_NhaThuocAnKhang')
    fac = M['facade_gray']
    # Mặt tiền quay về đường (−x). Tầng trệt lùi 0,45 m, kính tiệm sáng đèn.
    mb.boxab((xf, -30, 4.0), (xf + 6, 4, 12.4), fac)          # sâu 6 m: nằm gọn trên lối ven sông (21.248)
    mb.boxab((xf + 0.45, -30, 0.15), (xf + 6, 4, 4.0), fac)
    mb.boxab((xf + 0.40, -29.6, 0.15), (xf + 0.44, 3.6, 3.6), M['shop_cool'])
    for i in range(10):
        mb.box((xf + 0.37, -29.6 + i * 3.68, 1.88), (0.06, 0.08, 3.45), M['frame_dark'])
    for f in range(2):
        z = 4.2 + f * 3.4
        for i in range(7):
            wy = -27.5 + i * 4.6
            mb.box((xf - 0.02, wy, z + 1.5), (0.08, 1.9, 2.0), M['frame_white'])
            mb.box((xf - 0.04, wy, z + 1.5), (0.06, 1.6, 1.7), M['window'])
            if (i + f) % 2 == 0:
                mb.box((xf - 0.35, wy + 1.4, z + 0.6), (0.6, 0.8, 0.55), M['ac'])
        mb.box((xf - 0.5, -13, z + 0.05), (1.0, 34, 0.14), M['concrete'])
    mb.finish(C)
    sb = MB('OW_Bang_NhaThuocAnKhang')
    green = mat('OW_SignBG_1F7A4A', '#1F7A4A', 0.5)
    sb.box((xf - 0.12, -1.5, 5.0), (0.2, 11.0, 2.2), green)
    sb.box((xf - 0.24, -5.6, 5.0), (0.04, 0.45, 1.5), M['sign_white'])
    sb.box((xf - 0.24, -5.6, 5.0), (0.04, 1.5, 0.45), M['sign_white'])
    sb.finish(C)
    rot = (math.pi / 2, 0, -math.pi / 2)
    text(C, 'Nhà Thuốc', (xf - 0.26, -0.6, 5.5), 0.6, M['sign_white'], rot)
    text(C, 'An Khang', (xf - 0.26, -0.6, 4.6), 0.85, M['sign_white'], rot)

# ---------------------------------------------------------------- 6. cây, chậu cây, tàu: DÙNG ASSET CÓ SẴN
# Theo yêu cầu người dùng 17/11/2026: không dựng cây/chậu cây/tàu đánh cá mới. Lấy LOD0 của
# PV_StreetTree0/1/2.fbx, mẫu «Chậu cây lớn» (planter() trong pho_viet_saigon_streets.py) và PV_FishingBoat.fbx.
PROJECT = os.path.normpath(os.path.join(OW_DIR, '..', '..', '..'))
STREETS = os.path.join(PROJECT, 'Assets', 'PhoViet', 'Art', 'Streets')
_src = {}

def _lib():
    lib = coll('OW_ThuVienCoSan')
    try: bpy.context.view_layer.layer_collection.children[lib.name].exclude = True
    except Exception: pass
    return lib

def _import_fbx_lod0(key, path):
    """Nhập FBX vào collection riêng, chỉ giữ LOD0 làm nguồn instance."""
    if key in _src: return _src[key]
    c = bpy.data.collections.get('OW_Src_' + key)
    if c is None or not c.objects:
        if c is None:
            c = bpy.data.collections.new('OW_Src_' + key); _lib().children.link(c)
        before = set(bpy.data.objects)
        bpy.ops.import_scene.fbx(filepath=path)
        for o in [o for o in bpy.data.objects if o not in before]:
            for uc in list(o.users_collection): uc.objects.unlink(o)
            c.objects.link(o)
        for o in list(c.objects):   # xe: _Body là LOD0; bỏ LOD1/LOD2 chồng lên
            if o.name.split('.')[0].endswith(('LOD1', 'LOD2')) and any(x.name.split('.')[0].endswith(('LOD0', '_Body')) for x in c.objects):
                bpy.data.objects.remove(o, do_unlink=True)
        objs = list(c.objects)
        if any(o.name.split('.')[0].endswith('LOD0') for o in objs):
            for o in objs:
                if o.type == 'MESH' and o.name.split('.')[0].endswith('LOD0'):
                    o.parent = None; o.location = (0, 0, 0)
            for o in objs:
                if not (o.type == 'MESH' and o.name.split('.')[0].endswith('LOD0')):
                    bpy.data.objects.remove(o, do_unlink=True)
    _src[key] = c
    return c

def tree_src(i):
    return _import_fbx_lod0('Tree%d' % i, os.path.join(STREETS, 'PV_StreetTree%d.fbx' % i))

def boat_src():
    rel = os.path.join(PROJECT, 'Assets', 'PhoViet', 'Art', 'River', 'FishingBoat', 'PV_FishingBoat.fbx')
    c = _import_fbx_lod0('FishingBoat', rel)
    attach_textures(c, rel)
    return c

def planter_src():
    if 'Planter' in _src: return _src['Planter']
    c = bpy.data.collections.get('OW_Src_Planter')
    if c is None or not c.objects:
        if c is None:
            c = bpy.data.collections.new('OW_Src_Planter'); _lib().children.link(c)
        p = os.path.join(PROJECT, 'ArtSource', 'Blender', 'pho_viet_saigon_streets.py')
        ns = {'__name__': 'pv_streets_lib', '__file__': p}
        exec(compile(open(p, encoding='utf-8').read(), p, 'exec'), ns)
        ns['MAT'] = ns['palette_material']()
        g = ns['Geo'](0); ns['planter'](g, 0, 0, [], scale=1.0, pot='white')
        ns['to_mesh']('PV_ChauCayLon_LOD0', g, c)
    _src['Planter'] = c
    return c

def place(C, src, loc, rz=0.0, s=1.0, name='OW_Inst'):
    e = bpy.data.objects.new(name, None); e.instance_type = 'COLLECTION'; e.instance_collection = src
    e.location = loc; e.rotation_euler = (0, 0, rz); e.scale = (s, s, s); C.objects.link(e)
    return e

def place_tree(C, kind, seed, loc, rz, s):
    """kind/seed giữ chữ ký cũ; cây lớn phóng 1,25–1,45 lần cho cao như ảnh 3 (cây mẫu cao 7,2 m)."""
    k = 1.55 if kind == 'big' else 1.15
    return place(C, tree_src(seed % 3), loc, rz, s * k, 'OW_Cay_PV_StreetTree%d' % (seed % 3))

# 21.246: vật cản trên vỉa hè (x, y, nửa rộng x, nửa rộng y, loại) để hàng rào/cột đèn không xuyên bồn cây.
OBST = []
TREE_BASE_HALF = 0.73          # nửa cạnh bồn gốc cây PV_StreetTree* trước khi phóng

def curb_tree(C, side, y, seed, s, rz):
    """Cây lớn mép vỉa hè: bồn gốc vuông theo mép đường (xoay bội 90°) và nằm gọn trong vỉa hè, cách bó vỉa 0,35 m."""
    hw = TREE_BASE_HALF * s * 1.55
    x = side * (ROAD_HW + 0.35 + hw)
    rz = round(rz / (math.pi / 2)) * (math.pi / 2)
    place_tree(C, 'big', seed, (x, y, 0.15), rz, s)
    OBST.append((x, y, hw, hw, 'tree'))

def ensure_tree_obstacles():
    """Khi chỉ dựng lại phần 'props', lấy vị trí cây đã có trong cảnh."""
    if any(o[4] == 'tree' for o in OBST): return
    for ob in bpy.data.objects:
        if not ob.name.startswith('OW_Cay_PV_StreetTree') or not ob.users_collection: continue
        if not ob.users_collection[0].name.startswith(ROOT) or abs(ob.location.x) > 12: continue
        hw = TREE_BASE_HALF * ob.scale.x
        OBST.append((ob.location.x, ob.location.y, hw, hw, 'tree'))

def free_y(x, y, half, pad=0.3):
    """Dời y ra khỏi mọi vật cản cùng làn x (dùng cho cột đèn)."""
    ensure_tree_obstacles()
    for _ in range(6):
        hit = next((o for o in OBST if abs(x - o[0]) < o[2] + half + 0.05 and abs(y - o[1]) < o[3] + half + pad), None)
        if not hit: return y
        y = hit[1] + (hit[3] + half + pad + 0.05) * (1 if y >= hit[1] else -1)
    return y

def build_trees(C):
    # Cây lớn hai bên vỉa hè, ~14 m một cây (ảnh 3: tán rộng, thân sẫm).
    for side in (-1, 1):
        y = Y0 + 6
        while y < 676:
            if side < 0 and in_cross(y, 3): y += 4; continue
            seed = rnd.choice([1, 2, 3]); rz = rnd.uniform(0, 6.28); sc = rnd.uniform(0.9, 1.2)
            curb_tree(C, side, y, seed, sc, rz)
            y += rnd.uniform(12, 16)
    # Cây nhỏ có sẵn trên dải xanh ven sông; không có bụi hoa giấy trên lề (người dùng bỏ 17/11/2026).
    y = Y0 + 10
    while y < 670:
        if rnd.random() < 0.55:
            place_tree(C, 'small', rnd.choice([4, 5]), (15.2, y, 0.36), rnd.uniform(0, 6.28), rnd.uniform(0.9, 1.15))
        y += rnd.uniform(7, 11)

# ---------------------------------------------------------------- 7. đèn, rào, bàn ghế cà phê, xe bánh mì, chậu cây
def street_lamp(mb, x, y, side):
    """Cột đèn cao một cần, xám (ảnh 3). Tự dời khỏi bồn cây; trả về y thực tế."""
    y = free_y(x, y, 0.25); OBST.append((x, y, 0.25, 0.25, 'lamp'))
    mb.cyl((x, y, 0.15), 0.14, 0.6, M['metal_gray'], seg=12, r2=0.12)
    mb.cyl((x, y, 0.75), 0.11, 8.6, M['metal_gray'], seg=12, r2=0.075)
    hx = x - side * 2.2
    mb.rod((x, y, 9.1), (x - side * 1.0, y, 9.7), 0.06, M['metal_gray'])
    mb.rod((x - side * 1.0, y, 9.7), (hx, y, 9.75), 0.06, M['metal_gray'])
    mb.box((hx, y, 9.65), (0.9, 0.36, 0.16), M['metal_gray'])
    mb.box((hx, y, 9.55), (0.75, 0.28, 0.04), M['lamp'])
    return y

def banner(mb, x, y, side):
    """Cờ Việt Nam treo dọc trên cột đèn (yêu cầu 17/11/2026 thay banner xanh): nền đỏ 1,0 × 1,5 m,
    sao vàng 5 cánh ở giữa, bán kính ngoại tiếp = 1/5 chiều dài cờ (0,3 m), có sao ở cả hai mặt."""
    z0, W, H = 5.6, 1.0, 1.5
    mb.rod((x, y, z0 + H + 0.05), (x - side * (W + 0.15), y, z0 + H + 0.05), 0.03, M['metal_gray'])
    mb.rod((x, y, z0 - 0.05), (x - side * (W + 0.15), y, z0 - 0.05), 0.03, M['metal_gray'])
    fx = x - side * (W / 2 + 0.12)
    mb.box((fx, y, z0 + H / 2), (W, 0.02, H), M['flag_red'])
    for ny in (-1.0, 1.0):
        mb.star((fx, y + ny * 0.011, z0 + H / 2), 0.3, M['flag_star'], normal_y=ny)

# 21.245: bàn ghế/dù kiểu thô cũ đã thay bằng mẫu chi tiết trong ow_streetcafe.py (street_cafe_set, umbrella_pro).
_cafe = os.path.join(OW_DIR, 'ow_streetcafe.py')
exec(compile(open(_cafe, encoding='utf-8').read(), _cafe, 'exec'), globals())

def short_fence(mb, x, y0, L, gap=0.25):
    """Rào sắt thấp mép vỉa hè; tự ngắt đoạn chừa khe quanh bồn cây, chân cột đèn (21.246)."""
    ensure_tree_obstacles()
    runs = [(y0, y0 + L)]
    for ox, oy, hx, hy, kind in OBST:
        if abs(x - ox) >= hx + 0.06: continue
        a, b = oy - hy - gap, oy + hy + gap; nr = []
        for s0, s1 in runs:
            if b <= s0 or a >= s1: nr.append((s0, s1)); continue
            if a > s0: nr.append((s0, a))
            if b < s1: nr.append((b, s1))
        runs = nr
    for s0, s1 in runs:
        if s1 - s0 >= 0.7: _fence_segment(mb, x, s0, s1 - s0)

def _fence_segment(mb, x, y0, L):
    mb.box((x, y0 + L / 2, 1.0), (0.05, L, 0.05), M['iron'])
    mb.box((x, y0 + L / 2, 0.35), (0.05, L, 0.04), M['iron'])
    for i in range(int(L / 0.12) + 1):
        mb.box((x, y0 + i * L / int(L / 0.12), 0.65), (0.02, 0.02, 0.7), M['iron'])
    for yy in (y0, y0 + L):
        mb.box((x, yy, 0.6), (0.07, 0.07, 0.9), M['iron'])

def build_props(C):
    mb = MB('OW_DoViaHe')
    r = random.Random(7)
    # Đèn đường: phía đông mỗi 30 m, phía tây so le.
    y = Y0 + 12
    while y < 676:
        yl = street_lamp(mb, ROAD_HW + 0.7, y, 1)
        if int(y) % 2 == 0: banner(mb, ROAD_HW + 0.7, yl, 1)
        y += 30
    y = Y0 + 27
    while y < Y1 - 30:
        if not in_cross(y, 2):
            yl = street_lamp(mb, -ROAD_HW - 0.7, y, -1)
            if int(y) % 3 == 0: banner(mb, -ROAD_HW - 0.7, yl, -1)
        y += 30
    # Rào sắt thấp đoạn tiền cảnh hai bên.
    for yy in (2.0, 8.5, 34.0, 60.0):
        short_fence(mb, -ROAD_HW - 0.55, yy, 2.6)
    for yy in (-4.0, 14.0, 30.0, 47.0, 70.0, 95.0, 120.0):
        short_fence(mb, ROAD_HW + 0.55, yy, 3.2)
    # Bàn ghế quán vỉa hè chi tiết trước Phở Thịnh / Cà Phê Sài Gòn và rải rác; chỉ 2 dù (21.245).
    cf = MB('OW_QuanViaHe'); CM = cafe_mats(); rr = random.Random(33)
    xl = -(ROAD_HW + WALK) + 2.1
    for yy, dish, chair, stools in ((7.2, 'pho', 'plastic_red', False), (11.8, 'pho', 'plastic_blue', True),
                                    (27.0, 'com_tam', 'plastic_red', False), (44.0, 'com_tam', 'plastic_green', True),
                                    (78.0, 'pho', 'plastic_red', False), (140.0, 'pho', 'plastic_blue', True), (210.0, 'com_tam', 'plastic_red', False)):
        if not in_cross(yy, 2.5): street_cafe_set(cf, CM, xl, yy, rr, dish, chair, stools)
    umbrella_pro(cf, CM, xl, 9.5, 1.45, 2.55, 'fab_red', 'fab_cream', 'fab_red')
    umbrella_pro(cf, CM, xl, 29.4, 1.45, 2.55, 'fab_green', 'fab_cream', 'fab_green')
    # Chậu cây bê tông vuông + cây cảnh.
    for yy in (0.5, 13.2, 19.6, 37.5, 58.0, 86.0, 120.0, 160.0):
        if in_cross(yy, 2): continue
        x = -ROAD_HW - WALK + 0.8
        place(C, planter_src(), (x, yy, 0.15), 0.0, 1.0, 'OW_ChauCayLon')
    # Xe bánh mì Cô Lan + dù đỏ (lối ven sông, ảnh 3).
    cx, cy = 11.5, 52.0
    mb.boxab((cx - 0.7, cy - 1.2, 0.55), (cx + 0.7, cy + 1.2, 1.25), M['cart_yellow'])
    mb.boxab((cx - 0.6, cy - 1.1, 1.25), (cx + 0.6, cy + 1.1, 1.85), M['window'])
    mb.boxab((cx - 0.7, cy - 1.2, 1.85), (cx + 0.7, cy + 1.2, 1.95), M['cart_yellow'])
    for dy in (-0.9, 0.9):
        mb.cyl((cx - 0.75, cy + dy, 0.35), 0.2, 0.08, M['iron'], seg=12, axis='X')
        mb.cyl((cx + 0.67, cy + dy, 0.35), 0.2, 0.08, M['iron'], seg=12, axis='X')
    # Dù đỏ của xe bánh mì: cùng mẫu dù chi tiết (21.245).
    umbrella_pro(cf, CM, cx + 1.1, cy, 1.5, 2.65, 'fab_red', 'fab_red', 'fab_cream')
    mb.finish(C); cf.finish(C)
    bs = MB('OW_Bang_BanhMiCoLan')
    bs.box((cx - 0.72, cy, 1.0), (0.03, 2.2, 0.45), M['red'])
    bs.finish(C)
    text(C, 'Bánh Mì Cô Lan', (cx - 0.75, cy, 1.0), 0.26, M['sign_white'], (math.pi / 2, 0, -math.pi / 2))
    # Biển mũi tên vàng cuối đường (giao R3) + hai biển xanh.
    cv = MB('OW_BienMuiTen')
    by = 709.0
    cv.box((0, by, 2.4), (7.0, 0.15, 1.3), M['chevron_bg'])
    for i in range(3):
        x = -1.8 + i * 1.8
        cv.box((x + 0.25, by - 0.09, 2.62), (0.95, 0.03, 0.22), M['chevron'], ry=0.0, rx=0.0, rz=0.0)
    for x in (-3.2, 3.2):
        cv.cyl((x, by, 0.15), 0.06, 1.75, M['metal_gray'])
    for x in (-6.5, 6.5):
        cv.cyl((x, by, 0.15), 0.06, 3.2, M['metal_gray'])
        cv.box((x, by - 0.05, 3.6), (0.8, 0.06, 1.4), M['sign_blue'])
    cv.finish(C)
    # Chữ mũi tên «« bằng font cho rõ hình chevron.
    text(C, '‹‹‹', (0, by - 0.12, 2.4), 1.6, M['chevron'], (math.pi / 2, 0, 0), 'arialbd.ttf')

# ---------------------------------------------------------------- 8. bờ xa, đường chân trời, Tháp Tài Chính
def lotus_tower(C, loc, H=262.0):
    """Tháp dáng búp sen: tiết diện elip thuôn dần, sân trực thăng nhô ra ở 2/3 chiều cao."""
    import bmesh
    bm = bmesh.new(); rings = []; seg = 32; n = 40
    for i in range(n + 1):
        t = i / n
        r = 22.0 * (0.85 + 0.35 * math.sin(math.pi * min(1, t * 1.25))) * (1 - t ** 2.2) + 0.6
        sk = 0.72  # dẹt theo một trục
        ring = [bm.verts.new((math.cos(a) * r, math.sin(a) * r * sk + t * t * 6, t * H)) for a in (2 * math.pi * k / seg for k in range(seg))]
        rings.append(ring)
    for i in range(n):
        for k in range(seg):
            bm.faces.new((rings[i][k], rings[i][(k + 1) % seg], rings[i + 1][(k + 1) % seg], rings[i + 1][k]))
    bm.faces.new(list(reversed(rings[0])))
    me = bpy.data.meshes.new('OW_ThapTaiChinh'); bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = True
    me.materials.append(mat_glass_tower('OW_TowerGlass', '#617C8E', 40))
    ob = bpy.data.objects.new('OW_ThapTaiChinh', me); ob.location = loc; C.objects.link(ob)
    hp = MB('OW_ThapTaiChinh_SanTrucThang')
    hp.cyl((loc[0] + 10, loc[1] - 14, loc[2] + H * 0.6), 11, 1.2, M['concrete'], seg=24)
    hp.boxab((loc[0] + 2, loc[1] - 8, loc[2] + H * 0.6 - 3), (loc[0] + 8, loc[1] - 2, loc[2] + H * 0.6 + 1.2), M['concrete'])
    hp.cyl((loc[0], loc[1] + 6, loc[2] + H * 0.97), 0.4, 18, M['metal_gray'])
    hp.finish(C)

def build_skyline(C):
    # Bờ đông: kè, hàng cây, cao ốc căn hộ.
    fb = MB('OW_BoDong')
    fb.boxab((FAR_BANK_X, Y0 - 300, WATER_Z - 1), (FAR_BANK_X + 400, NORTH_FAR_Y + 400, 0.3), M['grass'])
    fb.boxab((-700, NORTH_FAR_Y, WATER_Z - 1), (FAR_BANK_X, NORTH_FAR_Y + 400, 0.3), M['grass'])
    fb.boxab((FAR_BANK_X - 0.5, Y0 - 300, WATER_Z - 1), (FAR_BANK_X + 0.5, NORTH_FAR_Y, 0.6), M['bank'])
    fb.finish(C)
    y = Y0 - 200
    while y < NORTH_FAR_Y + 300:
        place_tree(C, rnd.choice(['big', 'small']), rnd.choice([1, 2, 3, 4, 5]), (FAR_BANK_X + rnd.uniform(4, 14), y, 0.3), rnd.uniform(0, 6.28), rnd.uniform(1.0, 1.5))
        y += rnd.uniform(6, 12)
    x = -500
    while x < FAR_BANK_X:
        place_tree(C, rnd.choice(['big', 'small']), rnd.choice([1, 2, 3, 4, 5]), (x, NORTH_FAR_Y + rnd.uniform(4, 14), 0.3), rnd.uniform(0, 6.28), rnd.uniform(1.0, 1.5))
        x += rnd.uniform(8, 14)
    hi = MB('OW_CaoOcXa')
    grids = [mat_windows_grid('OW_TowerGrid_%d' % i, w, g) for i, (w, g) in enumerate([('#D8D4CA', '#41566A'), ('#E4E1DA', '#4C6377'), ('#C9CDD0', '#3A4C5C'), ('#DCD3C2', '#4A5A66')])]
    def tower(cx, cy, w, d, h):
        m = rnd.choice(grids)
        hi.boxab((cx - w / 2, cy - d / 2, 0), (cx + w / 2, cy + d / 2, h), m)
        hi.boxab((cx - w / 2 - 0.3, cy - d / 2 - 0.3, h), (cx + w / 2 + 0.3, cy + d / 2 + 0.3, h + 1.2), M['concrete'])
        if rnd.random() < 0.5: hi.boxab((cx - w / 4, cy - d / 4, h + 1.2), (cx + w / 4, cy + d / 4, h + 5), M['concrete'])
    # Bờ đông Thủ Thiêm (căn hộ 20–35 tầng).
    for i in range(26):
        tower(rnd.uniform(FAR_BANK_X + 40, FAR_BANK_X + 380), rnd.uniform(Y0 - 100, NORTH_FAR_Y + 200), rnd.uniform(22, 34), rnd.uniform(18, 26), rnd.uniform(60, 120))
    # Bờ bắc + xa phía trước.
    for i in range(30):
        tower(rnd.uniform(-450, FAR_BANK_X + 300), rnd.uniform(NORTH_FAR_Y + 60, NORTH_FAR_Y + 600), rnd.uniform(20, 32), rnd.uniform(18, 26), rnd.uniform(40, 130))
    # Cụm cao ốc trung tâm phía tây (sau dãy nhà phố).
    for i in range(22):
        tower(rnd.uniform(-480, -170), rnd.uniform(120, 760), rnd.uniform(24, 36), rnd.uniform(20, 30), rnd.uniform(45, 140))
    hi.finish(C)
    # Tháp Tài Chính: bản đồ (160, −60) khu tài chính → cục bộ (−55, 420) (người dùng chọn 17/11/2026 để thấy ở cả ảnh 2 và 3).
    lotus_tower(C, (-55, 420, 0))
    # Tàu đánh cá có sẵn (PV_FishingBoat) trên sông.
    for bx, by, rz in ((48.0, 150.0, 1.45), (120.0, 420.0, -1.7), (70.0, 610.0, 1.6)):
        place(C, boat_src(), (bx, by, WATER_Z + 0.1), rz, 1.0, 'OW_TauDanhCa')
    # Tàu du lịch trên sông.
    bt = MB('OW_TauDuLich')
    bx, by = 95.0, 260.0
    bt.boxab((bx - 5, by - 22, WATER_Z - 0.5), (bx + 5, by + 22, WATER_Z + 2.2), M['frame_white'])
    bt.boxab((bx - 4.6, by - 18, WATER_Z + 2.2), (bx + 4.6, by + 16, WATER_Z + 4.6), M['window'])
    bt.boxab((bx - 4.8, by - 18.5, WATER_Z + 4.6), (bx + 4.8, by + 16.5, WATER_Z + 4.9), M['red'])
    bt.boxab((bx - 4.2, by - 12, WATER_Z + 4.9), (bx + 4.2, by + 8, WATER_Z + 6.8), M['frame_white'])
    bt.finish(C)

# ---------------------------------------------------------------- 8b. xe có sẵn của game (giữ lại theo yêu cầu)
ART = os.path.join(PROJECT, 'Assets', 'PhoViet', 'Art')
def veh_src(key, rel):
    c = _import_fbx_lod0(key, os.path.join(ART, rel))
    attach_textures(c, os.path.join(ART, rel))
    return c

def attach_textures(c, fbx):
    # FBX của game không nhúng ảnh: gắn lại <Tên>_Palette.png (màu) và _Normal.png nếu có, cùng thư mục FBX.
    folder = os.path.dirname(fbx); base = os.path.splitext(os.path.basename(fbx))[0]
    pal = os.path.join(folder, base + '_Palette.png'); nrm = os.path.join(folder, base + '_Normal.png')
    for o in c.objects:
        if o.type != 'MESH': continue
        for m in o.data.materials:
            if m is None or not m.use_nodes: continue
            for n in [n for n in m.node_tree.nodes if n.type == 'TEX_IMAGE' and (n.image is None or not n.image.has_data)]:
                m.node_tree.nodes.remove(n)   # ảnh thiếu → hiện tím; gắn lại bên dưới
            if any(n.type == 'TEX_IMAGE' for n in m.node_tree.nodes): continue
            if not os.path.exists(pal): continue
            nt = m.node_tree; b = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
            ti = nt.nodes.new('ShaderNodeTexImage'); ti.image = bpy.data.images.load(pal, check_existing=True)
            nt.links.new(ti.outputs['Color'], b.inputs['Base Color'])
            if os.path.exists(nrm):
                tn = nt.nodes.new('ShaderNodeTexImage'); tn.image = bpy.data.images.load(nrm, check_existing=True)
                tn.image.colorspace_settings.name = 'Non-Color'
                nm = nt.nodes.new('ShaderNodeNormalMap'); nt.links.new(tn.outputs['Color'], nm.inputs['Color'])
                nt.links.new(nm.outputs['Normal'], b.inputs['Normal'])
            b.inputs['Roughness'].default_value = 0.35

def fit_ground(src):
    """Độ cao đáy model (để đặt bánh xe chạm mặt đường)."""
    zs = [ (o.matrix_world @ Vector(v)).z for o in src.all_objects if o.type == 'MESH' for v in o.bound_box]
    return -min(zs) if zs else 0.0

def build_traffic(C):
    # Siêu xe xanh người chơi ngay trước camera, làn trong hướng bắc (ảnh 3).
    car = veh_src('BlueSupercar', 'PlayerCar/PV_BlueSupercar.fbx')
    place(C, car, (1.8, 9.0, fit_ground(car)), 0.0, 0.85, 'OW_Xe_NguoiChoi')
    # Xe trắng, vàng, đỏ phía trước như ảnh.
    for key, rel, x, y, rz in (('Sedan', 'Cars/PV_Sedan.fbx', -1.9, 38.0, math.pi), ('Taxi', 'Cars/PV_Taxi.fbx', 5.2, 44.0, 0.0),
                               ('Hatchback', 'Cars/PV_Hatchback.fbx', 1.8, 95.0, 0.0), ('Sedan', 'Cars/PV_Sedan.fbx', -5.2, 160.0, math.pi),
                               ('DeliveryVan', 'Cars/PV_DeliveryVan.fbx', 5.2, 230.0, 0.0)):
        src = veh_src(key, rel)
        place(C, src, (x, y, fit_ground(src)), rz, 1.0, 'OW_Xe_' + key)
    # Xe máy đậu trên vỉa hè hai bên.
    for key, rel, x, y, rz in (('Underbone', 'Bikes/PV_Underbone.fbx', -11.6, 17.0, 1.2), ('CityScooter', 'Bikes/PV_CityScooter.fbx', -11.4, 33.0, 1.3),
                               ('Underbone', 'Bikes/PV_Underbone.fbx', 12.2, 80.0, -1.9), ('CityScooter', 'Bikes/PV_CityScooter.fbx', -11.5, 66.0, 1.1)):
        src = veh_src(key, rel)
        place(C, src, (x, y, 0.15 + fit_ground(src)), rz, 1.0, 'OW_XeMayDau')

# ---------------------------------------------------------------- 9. camera như ảnh 3
def build_camera(C):
    cam = bpy.data.objects.get('OW_Cam_Ref3')
    if cam is None:
        cd = bpy.data.cameras.new('OW_Cam_Ref3'); cam = bpy.data.objects.new('OW_Cam_Ref3', cd); C.objects.link(cam)
    cam.data.lens = 22; cam.data.sensor_width = 36; cam.data.clip_end = 3000
    cam.location = (1.4, -12.0, 6.6)
    target = Vector((0.0, 140.0, -18.0))  # chân trời ~32% từ trên xuống như ảnh 3
    cam.rotation_euler = (target - cam.location).to_track_quat('-Z', 'Y').to_euler()
    bpy.context.scene.camera = cam

# ---------------------------------------------------------------- chạy
def run():
    OBST.clear()
    if 'scene' in PARTS:   # dựng lại vật liệu OW_* để áp thay đổi tham số
        for m in [m for m in bpy.data.materials if m.name.startswith('OW_')]: bpy.data.materials.remove(m)
    mats()
    if 'scene' in PARTS:
        clear_coll(ROOT); clear_coll('OW_ThuVienCay')
        setup_scene()
    root = coll(ROOT)
    _lib()
    steps = [('ground', build_ground), ('river', build_river), ('west', build_west), ('east', build_east),
             ('trees', build_trees), ('props', build_props), ('skyline', build_skyline), ('traffic', build_traffic), ('camera', build_camera)]
    for key, fn in steps:
        if key in PARTS:
            c = coll(ROOT + '_' + key, root)
            for o in list(c.objects): bpy.data.objects.remove(o, do_unlink=True)
            fn(c)
            print('built', key, len(c.objects))

if not globals().get('NO_RUN'):   # build_ref2_segment.py nạp file này làm thư viện
    run()
