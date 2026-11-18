# Quầy hàng trước chợ Bến Thành (21.243) — hàng hóa có hình khối rõ, đa dạng.
# Nạp trong build_ref2_segment.py (cùng namespace: MB, M, mat, mat_noise, text…).
# Hệ cục bộ: quầy dài theo y, mặt quầy quay ra đường (+x). Mỗi quầy: bàn gỗ + khăn phủ, kệ bậc phía sau,
# mái bạt sọc trên 4 cột, bảng tên; hàng hóa theo loại: hoa cúc, hoa bó, trái cây, đồ khô, lưu niệm/áo dài–nón lá.
import math, random

def stall_mats():
    S = {}
    def m(k, c, rough=0.6, metal=0.0, emit=None, es=0.0): S[k] = mat('OW_St_' + k, c, rough, metal, emit, es)
    def n(k, c1, c2, sc=8.0, rough=0.7, bump=0.3): S[k] = mat_noise('OW_St_' + k, c1, c2, scale=sc, rough=rough, bump=bump)
    n('wood', '#8A5A34', '#6B4426', 5, 0.75, 0.25); n('wood_dark', '#5E3E26', '#4A2F1C', 5, 0.75, 0.25)
    for k, c in (('cloth_red', '#B8322A'), ('cloth_blue', '#2F5D9A'), ('cloth_green', '#3E7A3E'), ('cloth_cream', '#E8DFC6')): m(k, c, 0.85)
    for k, c in (('stripe_red', '#C23A2E'), ('stripe_blue', '#2D63A8'), ('stripe_green', '#3A8A4A'), ('stripe_white', '#F2EEE4')): m(k, c, 0.8)
    m('terracotta', '#A85A36', 0.8)
    for k, c in (('bucket_blue', '#2F7BC0'), ('bucket_red', '#C8342C'), ('bucket_green', '#3E9A4E')): m(k, c, 0.4)
    n('wicker', '#B88A4E', '#8E6532', 30, 0.85, 0.6)
    n('chrys_yellow', '#F6BE16', '#E08A08', 70, 0.7, 0.9); n('chrys_white', '#F4F0E4', '#DCD2BC', 70, 0.7, 0.9)
    n('chrys_orange', '#F28A1E', '#D45E10', 70, 0.7, 0.9)
    m('rose_red', '#C0182B', 0.5); m('rose_pink', '#E8689A', 0.5); m('lily_white', '#F8F4EA', 0.4)
    m('orchid', '#A24FBF', 0.45); m('sun_petal', '#F7C412', 0.55); n('sun_center', '#4A2E14', '#2E1C0C', 60, 0.8, 0.8)
    n('leaf', '#3F7A2C', '#2C5A1E', 25, 0.6, 0.5); m('stem', '#4E7A2E', 0.6)
    m('orange', '#F0861A', 0.45); m('mango', '#E8C23A', 0.4); m('dragon', '#D2306B', 0.45); m('dragon_tip', '#86B43A', 0.5)
    S['melon'] = mat_noise('OW_St_melon', '#24561F', '#5E9A3A', scale=3.5, rough=0.4, bump=0.1)
    m('banana', '#F2D032', 0.5); m('grape', '#5B2A6E', 0.25); n('coconut', '#6B4A2A', '#4A321C', 20, 0.9, 0.6)
    n('pineapple', '#C7962E', '#8A6A1E', 40, 0.7, 0.9); m('apple', '#C42C2A', 0.3); m('lime', '#7DB33A', 0.4)
    n('rice', '#EFE9D8', '#D8CFB8', 80, 0.9, 0.6); n('chili', '#B3241C', '#7E140E', 60, 0.6, 0.7)
    n('bean', '#2A2420', '#3E342C', 90, 0.5, 0.7); n('peanut', '#C9A16A', '#A8814E', 70, 0.8, 0.6)
    n('coffee', '#4A2E1B', '#2E1C10', 90, 0.5, 0.7); n('burlap', '#A58A5E', '#8A7048', 45, 0.95, 0.6)
    n('straw', '#DCC48C', '#BFA266', 40, 0.85, 0.4)
    m('lacquer_red', '#9E1F1A', 0.15); m('lacquer_black', '#1A1414', 0.12); m('gold', '#D4A63A', 0.25, 0.9)
    for k, c in (('silk_red', '#C8202A'), ('silk_blue', '#2A5CB8'), ('silk_white', '#F4F1EA'), ('silk_yellow', '#E8B820'), ('silk_pink', '#E87AA8'), ('tee_green', '#3A9A5A'), ('tee_gray', '#8A8E92')): m(k, c, 0.55)
    m('glass', '#D8E8EA', 0.05); m('candy_a', '#E8405A', 0.3); m('candy_b', '#F2B620', 0.3); m('candy_c', '#4AAE5A', 0.3)
    m('tag', '#FBF8EE', 0.6); m('tag_text', '#8E1B16', 0.5)
    return S

# ---------------------------------------------------------------- hàng hóa
def chrys_pot(mb, S, x, y, z, color, r):
    mb.cyl((x, y, z), 0.1, 0.17, S['terracotta'], seg=12, r2=0.135)
    mb.sphere((x, y, z + 0.24), 0.17, S['leaf'], seg=10, rings=5, scale=(1, 1, 0.6))
    for k in range(8):
        a = k * 2.4 + r.uniform(-0.2, 0.2); d = 0.04 + 0.075 * math.sqrt(k / 8)
        mb.sphere((x + math.cos(a) * d, y + math.sin(a) * d, z + 0.33 - d * 0.35), 0.052, S[color], seg=8, rings=5, scale=(1, 1, 0.75))

def bucket_bouquet(mb, S, x, y, z, kind, r):
    mb.cyl((x, y, z), 0.12, 0.3, r.choice([S['bucket_blue'], S['bucket_red'], S['bucket_green']]), seg=14, r2=0.15)
    for k in range(9):
        a = k * 0.7 + r.uniform(-0.2, 0.2); d = r.uniform(0.0, 0.1)
        top = (x + math.cos(a) * (d + 0.08), y + math.sin(a) * (d + 0.08), z + 0.62 + r.uniform(-0.06, 0.08))
        mb.rod((x + math.cos(a) * d * 0.4, y + math.sin(a) * d * 0.4, z + 0.25), top, 0.008, S['stem'], seg=4)
        if kind == 'rose':
            mb.sphere(top, 0.042, r.choice([S['rose_red'], S['rose_red'], S['rose_pink']]), seg=8, rings=5, scale=(1, 1, 0.85))
            mb.box((top[0], top[1], top[2] - 0.05), (0.07, 0.03, 0.012), S['leaf'], rz=a)
        elif kind == 'lily':
            mb.cyl((top[0], top[1], top[2] - 0.05), 0.02, 0.1, S['lily_white'], seg=6, r2=0.075)
            mb.cyl((top[0], top[1], top[2] + 0.03), 0.008, 0.04, S['sun_petal'], seg=4)
        else:   # hướng dương quay ra đường
            mb.cyl(top, 0.1, 0.025, S['sun_petal'], seg=14, axis='X')
            mb.cyl((top[0] + 0.022, top[1], top[2]), 0.045, 0.02, S['sun_center'], seg=12, axis='X')

def orchid_pot(mb, S, x, y, z, r):
    mb.cyl((x, y, z), 0.08, 0.14, S['lacquer_red'] if r.random() < 0.5 else S['glass'], seg=12, r2=0.1)
    for k in range(3):
        mb.box((x, y, z + 0.17), (0.28, 0.06, 0.02), S['leaf'], rz=k * 1.05)
    for s in (-1, 1):
        prev = (x, y, z + 0.15)
        for k in range(6):
            p = (x + s * 0.03 * k, y + 0.01 * k, z + 0.15 + 0.08 * k - (0.012 * k * k if k > 3 else 0))
            mb.rod(prev, p, 0.006, S['stem'], seg=4); prev = p
            if k >= 2: mb.sphere(p, 0.035, S['orchid'], seg=8, rings=4, scale=(1.2, 0.5, 1))

def fruit_pyramid(mb, S, x, y, z, w, d, rad, key, layers=3, scale=(1, 1, 1), tray=True, tip=None):
    if tray:
        mb.boxab((x - d / 2 - 0.03, y - w / 2 - 0.03, z), (x + d / 2 + 0.03, y + w / 2 + 0.03, z + 0.05), S['wicker'])
        mb.boxab((x - d / 2 - 0.03, y - w / 2 - 0.03, z + 0.05), (x - d / 2, y + w / 2 + 0.03, z + 0.1), S['wicker'])
        z += 0.05
    for L in range(layers):
        nx = max(1, int(d / (2 * rad)) - L); ny = max(1, int(w / (2 * rad)) - L)
        for i in range(nx):
            for j in range(ny):
                cx = x - (nx - 1) * rad + i * 2 * rad; cy = y - (ny - 1) * rad + j * 2 * rad
                cz = z + rad * scale[2] + L * rad * 1.5 * scale[2]
                mb.sphere((cx, cy, cz), rad, S[key], seg=10, rings=6, scale=scale)
                if tip: mb.box((cx, cy, cz + rad * scale[2]), (0.025, 0.025, 0.04), S[tip])

def banana_bunch(mb, S, x, y, z, hang=True):
    mb.rod((x, y, z), (x, y, z - 0.12), 0.015, S['wood_dark'], seg=5)
    for k in range(7):
        a = -0.9 + k * 0.3
        p0 = (x, y + math.sin(a) * 0.05, z - 0.1)
        p1 = (x + 0.12, y + math.sin(a) * 0.12, z - 0.16)
        p2 = (x + 0.2, y + math.sin(a) * 0.16, z - 0.08)
        mb.rod(p0, p1, 0.02, S['banana'], seg=6); mb.rod(p1, p2, 0.02, S['banana'], seg=6)

def pineapple(mb, S, x, y, z):
    mb.sphere((x, y, z + 0.13), 0.08, S['pineapple'], seg=10, rings=6, scale=(1, 1, 1.6))
    for k in range(7):
        a = k * 0.9
        mb.box((x + math.cos(a) * 0.02, y + math.sin(a) * 0.02, z + 0.3), (0.018, 0.05, 0.16), S['leaf'], rx=math.cos(a) * 0.4, ry=math.sin(a) * 0.4, rz=a)

def grape_bunch(mb, S, x, y, z):
    for k in range(14):
        layer = int(math.sqrt(k)); a = k * 2.2
        mb.sphere((x + math.cos(a) * 0.025 * (3 - layer), y + math.sin(a) * 0.025 * (3 - layer), z + 0.16 - layer * 0.04), 0.022, S['grape'], seg=6, rings=4)
    mb.rod((x, y, z + 0.17), (x, y, z + 0.21), 0.006, S['stem'], seg=4)

def sack(mb, S, x, y, z, content):
    mb.cyl((x, y, z), 0.19, 0.34, S['burlap'], seg=14, r2=0.2)
    mb.cyl((x, y, z + 0.32), 0.23, 0.06, S['burlap'], seg=14)          # miệng bao xắn
    mb.sphere((x, y, z + 0.36), 0.2, S[content], seg=12, rings=5, scale=(1, 1, 0.35))
    mb.cyl((x + 0.05, y + 0.04, z + 0.38), 0.035, 0.02, S['gold'], seg=8)    # muỗng xúc
    mb.box((x + 0.12, y + 0.08, z + 0.42), (0.16, 0.02, 0.02), S['gold'], ry=-0.4)

def jar(mb, S, x, y, z, candy):
    mb.cyl((x, y, z), 0.07, 0.18, S['glass'], seg=12)
    mb.cyl((x, y, z + 0.01), 0.06, 0.13, S[candy], seg=10)
    mb.cyl((x, y, z + 0.18), 0.075, 0.03, S['lacquer_red'], seg=12)

def hat_stack(mb, S, x, y, z, n=5):
    for k in range(n):
        mb.cyl((x, y, z + k * 0.03), 0.21, 0.13, S['straw'], seg=18, r2=0.012)

def vase(mb, S, x, y, z, key):
    mb.cyl((x, y, z), 0.05, 0.04, S[key], seg=12, r2=0.07)
    mb.sphere((x, y, z + 0.14), 0.09, S[key], seg=12, rings=6, scale=(1, 1, 1.3))
    mb.cyl((x, y, z + 0.24), 0.035, 0.12, S[key], seg=10)
    mb.cyl((x, y, z + 0.35), 0.05, 0.02, S['gold'], seg=10)
    mb.cyl((x, y, z + 0.12), 0.092, 0.02, S['gold'], seg=12)

def garment(mb, S, x, y, z, key, long=False):
    """Áo treo trên sào, mặt áo quay ra đường (+x)."""
    mb.box((x, y, z + 0.02), (0.02, 0.36, 0.02), S['wood_dark'])                   # móc
    if long:   # áo dài: thân dài, vạt xòe, tay áo
        mb.box((x, y, z - 0.35), (0.03, 0.34, 0.6), S[key])
        mb.box((x, y, z - 0.95), (0.03, 0.46, 0.6), S[key])
        for s in (-1, 1): mb.box((x, y + s * 0.26, z - 0.28), (0.03, 0.12, 0.5), S[key], rx=s * 0.25)
    else:
        mb.box((x, y, z - 0.3), (0.03, 0.44, 0.55), S[key])
        for s in (-1, 1): mb.box((x, y + s * 0.29, z - 0.1), (0.03, 0.18, 0.16), S[key], rx=s * 0.6)

# ---------------------------------------------------------------- khung quầy
def stall_frame(mb, S, xs, ys, L, cloth, stripe, r):
    D = 1.3; y0, y1 = ys - L / 2, ys + L / 2
    mb.boxab((xs - D / 2, y0, 0.76), (xs + D / 2, y1, 0.82), S['wood'])
    for dx in (-D / 2 + 0.06, D / 2 - 0.06):
        for yy in (y0 + 0.06, y1 - 0.06):
            mb.boxab((dx + xs - 0.03, yy - 0.03, 0.15), (dx + xs + 0.03, yy + 0.03, 0.76), S['wood_dark'])
    mb.boxab((xs + D / 2, y0, 0.32), (xs + D / 2 + 0.02, y1, 0.8), S[cloth])          # khăn phủ trước
    mb.boxab((xs + D / 2 - 0.01, y0, 0.74), (xs + D / 2 + 0.03, y1, 0.82), S['cloth_cream'])
    # Kệ bậc phía sau.
    mb.boxab((xs - D / 2, y0, 0.82), (xs - D / 2 + 0.45, y1, 1.12), S['wood'])
    mb.boxab((xs - D / 2, y0, 1.12), (xs - D / 2 + 0.22, y1, 1.42), S['wood'])
    # Cột + mái bạt sọc dốc ra đường + diềm rủ.
    for xx in (xs - D / 2 - 0.05, xs + D / 2 + 0.35):
        for yy in (y0 + 0.04, y1 - 0.04):
            mb.cyl((xx, yy, 0.15), 0.03, 2.45 if xx < xs else 2.2, S['wood_dark'], seg=6)
    n = max(4, int(L / 0.42)); sw = L / n
    for i in range(n):
        m = S[stripe] if i % 2 == 0 else S['stripe_white']
        yy = y0 + (i + 0.5) * sw
        mb.quad([(xs - D / 2 - 0.25, yy - sw / 2, 2.62), (xs - D / 2 - 0.25, yy + sw / 2, 2.62), (xs + D / 2 + 0.9, yy + sw / 2, 2.32), (xs + D / 2 + 0.9, yy - sw / 2, 2.32)], m)
        mb.quad([(xs + D / 2 + 0.9, yy - sw / 2, 2.32), (xs + D / 2 + 0.9, yy + sw / 2, 2.32), (xs + D / 2 + 0.9, yy + sw / 2, 2.08), (xs + D / 2 + 0.9, yy - sw / 2, 2.08)], m)
    mb.boxab((xs + D / 2 + 0.3, y0, 2.0), (xs + D / 2 + 0.36, y1, 2.05), S['wood_dark'])   # xà treo hàng

def stall_sign(C, mb, S, xs, ys, L, title):
    x = xs + 1.3 / 2 + 0.92
    mb.boxab((x, ys - L * 0.32, 1.72), (x + 0.03, ys + L * 0.32, 2.04), S['tag'])
    text(C, title, (x + 0.04, ys, 1.88), 0.17, S['tag_text'], (math.pi / 2, 0, math.pi / 2), 'arialbd.ttf', extrude=0.004, name='OW_BangQuay_' + title)

def price_tag(C, mb, S, x, y, z, txt):
    mb.box((x, y, z + 0.07), (0.015, 0.16, 0.1), S['tag'])
    mb.rod((x, y, z), (x, y, z + 0.03), 0.004, S['wood_dark'], seg=4)
    text(C, txt, (x + 0.01, y, z + 0.07), 0.045, S['tag_text'], (math.pi / 2, 0, math.pi / 2), 'arialbd.ttf', extrude=0.001, name='OW_GiaTien')

# ---------------------------------------------------------------- loại quầy
STALL_KINDS = {
    'hoa_cuc': ('Hoa Cúc Tết', 'cloth_red', 'stripe_red'),
    'hoa_bo': ('Hoa Tươi', 'cloth_green', 'stripe_green'),
    'trai_cay': ('Trái Cây Tươi', 'cloth_blue', 'stripe_blue'),
    'do_kho': ('Đặc Sản Khô', 'cloth_red', 'stripe_red'),
    'luu_niem': ('Áo Dài - Nón Lá', 'cloth_blue', 'stripe_blue'),
}

def stall(C, st, gd, S, xs, ys, L, kind, r):
    title, cloth, stripe = STALL_KINDS[kind]
    stall_frame(st, S, xs, ys, L, cloth, stripe, r)
    stall_sign(C, st, S, xs, ys, L, title)
    D = 1.3; y0, y1 = ys - L / 2 + 0.2, ys + L / 2 - 0.2
    zt, z1, z2 = 0.82, 1.12, 1.42
    xb1, xb2 = xs - D / 2 + 0.33, xs - D / 2 + 0.11
    if kind == 'hoa_cuc':
        for row, xx in enumerate((xs + 0.42, xs + 0.12, xs - 0.18)):
            y = y0 + 0.14
            while y < y1 - 0.1:
                chrys_pot(gd, S, xx, y + (0.14 if row % 2 else 0), zt, r.choice(['chrys_yellow', 'chrys_yellow', 'chrys_white', 'chrys_orange']), r); y += 0.3
        y = y0 + 0.15
        while y < y1:
            chrys_pot(gd, S, xb1, y, z1, 'chrys_yellow', r); chrys_pot(gd, S, xb2, y + 0.15, z2, r.choice(['chrys_white', 'chrys_orange']), r); y += 0.32
        for yy in (y0, y1):   # chậu lớn dưới đất trước quầy
            chrys_pot(gd, S, xs + 0.95, yy, 0.15, 'chrys_yellow', r)
    elif kind == 'hoa_bo':
        y = y0 + 0.15; kinds = ['rose', 'lily', 'sun']
        i = 0
        while y < y1:
            bucket_bouquet(gd, S, xs + 0.35, y, zt, kinds[i % 3], r)
            bucket_bouquet(gd, S, xs - 0.05, y + 0.17, zt, kinds[(i + 1) % 3], r)
            y += 0.36; i += 1
        y = y0 + 0.2
        while y < y1:
            orchid_pot(gd, S, xb1, y, z1, r); y += 0.4
        for yy in (y0 + 0.2, ys, y1 - 0.2):
            bucket_bouquet(gd, S, xs + 1.0, yy, 0.15, 'rose', r)
    elif kind == 'trai_cay':
        segs = [('orange', 0.055, (1, 1, 1), None), ('mango', 0.06, (1, 0.8, 1.35), None), ('dragon', 0.07, (1, 1, 1.25), 'dragon_tip'), ('apple', 0.05, (1, 1, 0.95), None), ('lime', 0.04, (1, 1, 1), None)]
        n = len(segs); w = (y1 - y0) / n
        for i, (k, rad, sc, tip) in enumerate(segs):
            fruit_pyramid(gd, S, xs + 0.15, y0 + (i + 0.5) * w, zt, w - 0.08, 0.8, rad, k, 3, sc, True, tip)
            price_tag(C, gd, S, xs + 0.6, y0 + (i + 0.5) * w, zt + 0.02, r.choice(['25K', '30K', '45K', '60K']))
        y = y0 + 0.2
        while y < y1:
            pineapple(gd, S, xb1, y, z1); grape_bunch(gd, S, xb2, y + 0.18, z2); y += 0.42
        y = y0 + 0.2
        while y < y1:
            banana_bunch(gd, S, xs + 0.95, y, 2.0); y += 0.55
        for yy in (y0 + 0.25, y0 + 0.65, y1 - 0.25):
            gd.sphere((xs + 0.95, yy, 0.33), 0.18, S['melon'], seg=12, rings=8, scale=(1, 1, 0.85))
        gd.sphere((xs + 0.95, (y0 + y1) / 2, 0.3), 0.12, S['coconut'], seg=10, rings=6)
    elif kind == 'do_kho':
        contents = ['rice', 'chili', 'bean', 'peanut', 'coffee', 'rice']
        y = y0 + 0.22; i = 0
        while y < y1 - 0.1:
            sack(gd, S, xs + 0.25, y, zt, contents[i % len(contents)])
            price_tag(C, gd, S, xs + 0.5, y, zt + 0.36, r.choice(['20K/kg', '35K/kg', '90K/kg']))
            y += 0.46; i += 1
        y = y0 + 0.12
        while y < y1:
            jar(gd, S, xb1, y, z1, r.choice(['candy_a', 'candy_b', 'candy_c'])); jar(gd, S, xb2, y + 0.1, z2, r.choice(['candy_a', 'candy_b', 'candy_c'])); y += 0.2
        for yy in (y0 + 0.1, y1 - 0.1):
            sack(gd, S, xs + 0.95, yy, 0.15, r.choice(contents))
    else:   # lưu niệm: nón lá, bình sơn mài, áo dài + áo thun treo
        y = y0 + 0.25
        while y < y1:
            hat_stack(gd, S, xs + 0.3, y, zt, r.randint(3, 6)); y += 0.48
        y = y0 + 0.15
        while y < y1:
            vase(gd, S, xb1, y, z1, r.choice(['lacquer_red', 'lacquer_black'])); vase(gd, S, xb2, y + 0.12, z2, r.choice(['lacquer_red', 'lacquer_black'])); y += 0.26
        y = y0 + 0.2; keys = ['silk_red', 'silk_blue', 'silk_white', 'silk_yellow', 'silk_pink', 'tee_green', 'tee_gray']
        i = 0
        while y < y1:
            garment(gd, S, xs + 0.95, y, 2.0, keys[i % len(keys)], long=(i % 3 != 2)); y += 0.42; i += 1

def build_stalls(C, xs, spots, seed=11):
    """spots: danh sách (y tâm, chiều dài, loại)."""
    S = stall_mats()
    r = random.Random(seed)
    # 21.250: mỗi quầy một cặp lưới (khung + hàng hóa) để Unity ẩn quầy xa; trước đây gộp 430 nghìn tam giác một lưới.
    for i, (ys, L, kind) in enumerate(spots):
        st = MB('OW_QuayHang_%02d_Khung' % i); gd = MB('OW_QuayHang_%02d_HangHoa' % i)
        stall(C, st, gd, S, xs, ys, L, kind, r)
        st.finish(C); gd.finish(C)
