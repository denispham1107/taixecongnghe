"""Street-facing fixture sets for the riverside shop bays. Uses the street atlas."""

RIVER_FOOD = {'CƠM BÌNH DÂN', 'BÚN RIÊU', 'BÁNH CUỐN', 'XÔI', 'ỐC'}
RIVER_DRINKS = {'CÀ PHÊ', 'NƯỚC MÍA', 'TRÀ SỮA'}


def river_shop_planter(g, x, z, seed, flowers=False, scale=1):
    # Terracotta pot with a flared rim, visible soil and individually folded leaves.
    s = scale
    n = (10, 7, 5)[g.d]
    g.cyl((x, .02*s, z), (x, .30*s, z), .15*s, 'tileDark', n=n, r2=.21*s)
    g.cyl((x, .29*s, z), (x, .34*s, z), .225*s, 'tile', n=n)
    g.disc((x, .344*s, z), .19*s, 'soil', facing=(0, 1, 0), n=n)
    rng = random.Random(seed)
    for i in range((10, 6, 3)[g.d]):
        a = 2*pi*i/(10, 6, 3)[g.d]+rng.uniform(-.21, .21)
        length = rng.uniform(.23, .43)*s
        tip = (x+cos(a)*length*.55, (.52+length*.36)*s, z+sin(a)*length*.55)
        if g.d < 2:
            g.cyl((x, .34*s, z), tip, .008*s, 'leaf5', n=4, caps=False)
        g.leaf(tip, (cos(a), .66, sin(a)), length*.52, .054*s,
               ('leaf2', 'leaf3', 'leaf4')[i % 3], folded=g.d == 0)
        if flowers and i % 2 == 0:
            g.blob(tip, (.043*s, .032*s, .04*s), ('pink', 'pinkLight', 'white')[i % 3], n=5, m=2)


def river_shop_food(g, x, z, label, spec):
    # Stainless serving trolley, glass sneeze guard, trays, lidded stock pot and rubber casters.
    w = 1.32
    g.cube((x, .47, z), (w, .76, .47), 'steel', skip=('-y',))
    name_panel(g, spec, x, .52, z-.236, w-.07, .43)
    g.cube((x, .88, z), (w+.04, .05, .53), 'chrome')
    for side in (-1, 1):
        px = x+side*.63
        g.cube((px, 1.13, z-.23), (.026, .48, .026), 'chrome')
        if g.d < 2:
            g.cube((px, 1.13, z+.21), (.026, .48, .026), 'chrome')
            g.cyl((px-side*.11, .07, z-.19), (px-side*.11, .07, z+.19), .067, 'rubber', n=8)
    g.cube((x, 1.38, z), (w, .029, .48), 'chrome')
    g.panel((x, 1.128, z-.235), w-.05, .48, 'glassLight', tag='Glass')
    colors = {'BÚN RIÊU': ('redDark', 'leaf3', 'white'), 'ỐC': ('woodDark', 'leaf2', 'orange'),
              'XÔI': ('awnYellow', 'woodLight', 'leaf3'), 'BÁNH CUỐN': ('white', 'leaf3', 'woodLight')}
    for k, color in enumerate(colors.get(label, ('white', 'orange', 'leaf3'))):
        bx = x-.38+k*.34
        g.cube((bx, .925, z-.02), (.26, .038, .27), 'chrome')
        if g.d < 2:
            for q in range(3 if g.d == 0 else 1):
                g.blob((bx-.065+q*.064, .95, z-.02+(.054 if q == 1 else -.045)),
                       (.064, .042, .077), color, n=6, m=2)
    if label in ('BÚN RIÊU', 'ỐC'):
        bx = x+.47
        g.cyl((bx, .905, z), (bx, 1.17, z), .13, 'steel', n=10)
        g.cyl((bx, 1.17, z), (bx, 1.194, z), .143, 'chrome', n=10)
        g.cyl((bx, 1.194, z), (bx, 1.225, z), .028, 'iron', n=5)


def river_shop_drinks(g, x, z, label, spec):
    g.cube((x, .46, z), (1.28, .77, .50), 'wood', skip=('-y',))
    name_panel(g, spec, x, .48, z-.251, 1.21, .43)
    g.cube((x, .865, z), (1.34, .054, .55), 'chrome' if label == 'NƯỚC MÍA' else 'woodLight')
    if label == 'NƯỚC MÍA':
        # Roller press, drip tray and a bundle of cane over the loading end.
        g.cube((x+.17, 1.085, z+.04), (.56, .38, .34), 'steel')
        for y in (1.04, 1.16):
            g.cyl((x-.07, y, z-.15), (x+.39, y, z-.15), .048, 'chrome', n=8)
        g.cube((x+.17, .922, z-.14), (.51, .047, .26), 'chrome')
        for i in range((5, 3, 2)[g.d]):
            g.cyl((x-.56+i*.055, .91, z-.18), (x-.49+i*.055, 1.48, z+.15), .023, 'leaf3', n=5)
    else:
        for i in range((4, 3, 2)[g.d]):
            bx = x-.46+i*.25
            col = ('woodDark', 'interior', 'pinkLight', 'leaf3')[i]
            g.cyl((bx, .9, z-.065), (bx, 1.10, z-.065), .067, col, r2=.08, n=8)
            if g.d < 2:
                g.cyl((bx, 1.10, z-.065), (bx, 1.127, z-.065), .082, 'cream', n=8)
                g.cyl((bx+.018, 1.127, z-.065), (bx+.024, 1.233, z-.07), .008, 'awnWhite', n=4)
        if label == 'CÀ PHÊ':
            g.cyl((x+.49, .89, z+.05), (x+.49, 1.09, z+.05), .076, 'chrome', n=8)


def river_shop_tailor(g, x, z):
    g.cube((x, .66, z), (1.20, .055, .48), 'woodLight')
    for side in (-1, 1):
        g.cube((x+side*.49, .33, z), (.045, .66, .39), 'ironGrey')
    g.cube((x+.24, .72, z), (.54, .065, .26), 'iron')
    g.cube((x+.43, .84, z), (.13, .23, .17), 'iron')
    g.cube((x+.25, .967, z), (.47, .11, .17), 'iron')
    g.cube((x+.06, .885, z), (.095, .13, .12), 'iron')
    g.cyl((x+.49, .962, z), (x+.53, .962, z), .081, 'steel', n=10)
    for i, color in enumerate(('blue', 'pink', 'awnYellow')):
        xx = x-.48+i*.10
        g.cyl((xx, .685, z-.16), (xx, .685, z+.17), .046, color, n=8)
    if g.d == 0:
        g.cyl((x+.05, .77, z-.044), (x+.05, .85, z-.044), .009, 'chrome', n=5)
        g.cyl((x+.34, 1.025, z), (x+.34, 1.111, z), .026, 'red', n=6)


def river_shop_service(g, x, z, label, spec):
    if label in ('SỬA ĐIỆN', 'KHÓA - CHÌA'):
        toolbox(g, x, z, [])
        g.cube((x-.23, .996, z+.06), (.32, .11, .27), 'steel')
        for i in range((5, 3, 2)[g.d]):
            bx = x+.04+i*.075
            g.cyl((bx, .947, z-.15), (bx, .947, z+.08), .014, 'chrome', n=5)
            g.cyl((bx, .953, z-.15), (bx, .953, z-.072), .024, 'yellow' if label == 'KHÓA - CHÌA' else 'red', n=5)
    elif label == 'NHÀ MAY':
        river_shop_tailor(g, x, z)
    elif label in ('HỚT TÓC', 'SPA'):
        accent = 'redDark' if label == 'HỚT TÓC' else 'greenDark'
        g.cyl((x, .012, z), (x, .05, z), .26, 'iron', n=10)
        g.cyl((x, .05, z), (x, .48, z), .08, 'chrome', n=10)
        g.cube((x, .50, z), (.54, .09, .54), accent)
        g.cube((x, .725, z+.22), (.53, .49, .10), accent)
        for side in (-1, 1):
            bx = x+side*.28
            g.cyl((bx, .50, z), (bx, .68, z), .023, 'chrome', n=5)
            g.cube((bx, .70, z), (.065, .052, .42), 'iron')
        g.cube((x, .25, z-.23), (.37, .037, .20), 'chrome')
        if label == 'HỚT TÓC':
            g.cyl((x-.44, .08, z), (x-.44, 1.54, z), .037, 'chrome', n=8)
            for k in range(6):
                y = .98+k*.083
                g.cyl((x-.44, y, z), (x-.44, y+.083, z), .085, ('awnWhite', 'red', 'awnBlue')[k % 3], n=8)
        elif g.d < 2:
            for n in range(3):
                g.cyl((x-.18+n*.15, .56, z-.05), (x-.18+n*.15, .56, z+.16), .052, 'cream', n=7)
    else:
        # Lockable display cases and stands for retail and professional services.
        g.cube((x, .43, z), (1.32, .73, .49), 'ironGrey' if label == 'TIỆM VÀNG' else 'wood')
        name_panel(g, spec, x, .46, z-.246, 1.25, .40)
        g.cube((x, .817, z), (1.36, .046, .53), 'chrome')
        top = 1.18
        for side in (-1, 1):
            g.cube((x+side*.645, 1.0, z), (.028, .37, .51), 'chrome')
        g.panel((x, 1.006, z-.239), 1.26, .35, 'glassLight', tag='Glass')
        g.quad([(x-.64, top, z-.24), (x+.64, top, z-.24), (x+.64, top, z+.24), (x-.64, top, z+.24)],
               'glassLight', (0, 1, 0), 'Glass')
        for k in range((5, 4, 3)[g.d]):
            bx = x-.46+k*(.92/((5, 4, 3)[g.d]-1))
            if label == 'TIỆM VÀNG':
                g.cube((bx, .85, z), (.15, .038, .22), 'redDark')
                g.torus((bx, .907, z-.014), .044, .011, 'flagGold', axis='x', n=10, m=4)
            elif label == 'ĐIỆN THOẠI':
                g.cube((bx, .86, z), (.10, .033, .20), 'iron')
                g.cube((bx, .879, z-.007), (.083, .005, .168), ('goodsB', 'sky', 'glass')[k % 3])
            elif label == 'NHÀ THUỐC':
                g.cube((bx, .92, z), (.145, .18, .15), 'white')
                g.cube((bx, .914, z-.077), (.136, .043, .008), ('green', 'blue', 'red')[k % 3])
            else:
                for layer in range(2):
                    g.cube((bx+.005*layer, .85+.023*layer, z), (.153, .022, .23),
                           ('cream', 'sky', 'woodLight', 'pinkLight')[k % 4] if layer else 'white')
        if label == 'NHÀ THUỐC':
            g.cube((x+.44, 1.35, z+.13), (.30, .29, .045), 'white')
            g.cube((x+.44, 1.35, z+.102), (.21, .06, .012), 'green')
            g.cube((x+.44, 1.35, z+.101), (.06, .21, .012), 'green')


def river_shop_sidewalk(g, sign):
    """Build in street fixture axes (-Z out) then rotate to face the road."""
    label, facing = sign['label'], sign['f']
    center = (sign['x0']+sign['x1'])*.5
    width = sign['x1']-sign['x0']
    fixture = Geo(g.d)
    offset = width*.33
    seed = round(center*137+sign['zf']*211)
    spec = dict(main=label, board=sign['board'], ink=sign['ink'])
    if sign['petshop']:
        # Keep the existing pets and add only water/food bowls and plants at the jambs.
        for side in (-1, 1):
            bx = side*width*.405
            river_shop_planter(fixture, bx, -.36, seed+side, flowers=True, scale=.75)
            fixture.cyl((bx, .019, -.94), (bx, .075, -.94), .105, 'chrome', n=10, r2=.14)
            fixture.disc((bx, .077, -.94), .107, 'goodsB' if side < 0 else 'woodLight', (0, 1, 0), n=10)
    elif label in RIVER_FOOD or label in RIVER_DRINKS:
        if label in RIVER_FOOD:
            river_shop_food(fixture, -offset, -.59, label, spec)
            pho_table(fixture, offset, -.78)
            stool(fixture, offset-.39, -.43, .36)
            stool(fixture, offset+.39, -.43, .36)
        else:
            river_shop_drinks(fixture, -offset, -.60, label, spec)
            cafe_table(fixture, offset, -.78)
            cafe_chair(fixture, offset+.39, -.40, 1)
        river_shop_planter(fixture, -offset+.29, -1.06, seed, scale=.56)
    elif label in ('TẠP HÓA', 'MINI MART'):
        outdoor_shelf(fixture, -offset, -.53, [], w=1.24)
        drink_fridge(fixture, offset-.24 if width > 4.6 else offset, -.57, [])
        if width > 4.6:
            crates(fixture, width*.5-.36, -.55, [])
        else:
            river_shop_planter(fixture, -offset, -1.00, seed, scale=.58)
    elif label == 'NGÂN HÀNG':
        x = -offset
        fixture.cube((x, .79, -.57), (.68, 1.55, .47), 'steel')
        fixture.cube((x, 1.27, -.809), (.50, .53, .045), 'navy')
        fixture.cube((x, 1.28, -.839), (.35, .20, .013), 'goodsB')
        fixture.cube((x, 1.01, -.84), (.29, .11, .12), 'chrome')
        fixture.cube((x, .79, -.814), (.31, .04, .011), 'iron')
        fixture.text('ATM', (x, 1.49, -.837), .12, 'white', .40, bold=.02)
        river_shop_planter(fixture, offset, -.58, seed, flowers=True, scale=1.20)
    elif label == 'BẤT ĐỘNG SẢN':
        a_frame(fixture, -offset, -1.05, 'NHÀ ĐẤT', ('MUA BÁN', 'CHO THUÊ'), [])
        river_shop_planter(fixture, offset, -.58, seed, flowers=True, scale=1.20)
    else:
        river_shop_service(fixture, -offset, -.58, label, spec)
        river_shop_planter(fixture, offset, -.57, seed, flowers=seed % 3 != 0)
        if label in ('VĂN PHÒNG PHẨM', 'IN ẤN'):
            for k in range(3):
                fixture.cube((width*.5-.23, .06+k*.081, -.51), (.37, .075, .31), ('paveCream', 'sky', 'white')[k])

    sign_y = sign['base']+.016
    def rotate(p): return (-facing*p[0], p[1], -facing*p[2])
    def place(p):
        x, y, z = rotate(p)
        return (center+x, sign_y+y, sign['zf']+z)

    start = len(g.v)
    g.v.extend(place(p) for p in fixture.v)
    g.f.extend(tuple(start+n for n in face) for face in fixture.f)
    g.cols.extend(fixture.cols)
    g.tags.extend(fixture.tags)
    g.hint.extend((kind, place(p) if kind == 'c' else rotate(p)) for kind, p in fixture.hint)


def river_shop_apron(g, sign):
    """Continue the street's limestone and mauve pavers below the awning."""
    left, right, z, facing = sign['x0'], sign['x1'], sign['zf'], sign['f']
    width = right-left
    depth = 1.44
    g.cube(((left+right)*.5, .113, z+facing*depth*.5), (width-.02, .226, depth),
           'pavingJoint', skip=('-y',))
    n = max(1, round(width/(.95 if g.d == 2 else .55)))
    for column in range(n):
        xa, xb = left+column*width/n+.019, left+(column+1)*width/n-.019
        for row in range(3):
            za = z+facing*(row*.46+.02)
            zb = z+facing*((row+1)*.46-.02)
            material = 'pavingBeige' if (column+row+round(left*100)) % 2 == 0 else 'pavingMauve'
            g.quad([(xa,.229,za),(xb,.229,za),(xb,.229,zb),(xa,.229,zb)], material, (0,1,0))
    g.cube(((left+right)*.5, .166, z+facing*1.422), (width-.025, .126, .058),
           'curbLimestoneShade', skip=('-y',))
