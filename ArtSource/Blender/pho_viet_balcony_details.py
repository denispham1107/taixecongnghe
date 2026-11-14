"""Cửa chớp gỗ, lan can sắt và mái ngói của nhà phố Việt. Dùng chung Geo và atlas phố."""


def louver_window(g, x, y, z, w, h, facing, shutters, glazing=True):
    tag = 'FacadeWindow'
    depth = lambda offset: z+facing*offset
    head, sill = y+h*.5, y-h*.5
    if glazing:
        g.panel((x, y, depth(.045)), w+.16, h+.15, 'trim', facing, tag)
        g.panel((x, y, depth(.047)), w, h, 'glass', facing, tag)
    for side in (-1, 1):
        g.panel((x+side*(w*.5+.031), y, depth(.063)), .072, h+.11, 'trim', facing, tag)
    g.panel((x, head+.029, depth(.063)), w+.10, .075, 'trim', facing, tag)
    if g.d == 2:
        g.panel((x, sill-.055, depth(.125)), w+.22, .065, 'trim', facing, tag)
    else:
        g.cube((x, sill-.055, depth(.058)), (w+.22, .065, .15), 'trim', tag, skip=('-z' if facing > 0 else '+z',))
    if g.d < 2:
        g.panel((x, sill-.096, depth(.12)), w+.16, .025, 'stone', facing, tag)
        g.panel((x, y, depth(.068)), .049, h, 'woodDark', facing, tag)
        g.panel((x, y+h*.25, depth(.068)), w*.45, .052, 'woodDark', facing, tag)
        g.panel((x, y-h*.30, depth(.068)), w*.45, .038, 'woodDark', facing, tag)
    else:
        g.panel((x, y, depth(.068)), .048, h, 'woodDark', facing, tag)

    leaf_width = w*.285
    slats = (9, 5, 2)[g.d]
    slat_colour = 'green' if shutters == 'greenDark' else shutters
    rail_width = min(.073, leaf_width*.14)
    for side in (-1, 1):
        sx = x+side*(w-leaf_width)*.5
        g.panel((sx, y, depth(.083)), leaf_width, h, 'greenDark', facing, tag)
        if g.d == 0:
            outer = sx+side*leaf_width*.5
            g.quad([(outer, sill, depth(.061)), (outer, sill, depth(.126)),
                    (outer, head, depth(.126)), (outer, head, depth(.061))], shutters, (side, 0, 0), tag)
        if g.d < 2:
            for edge in (-1, 1):
                g.panel((sx+edge*(leaf_width-rail_width)*.5, y, depth(.126)), rail_width, h, shutters, facing, tag)
                if g.d == 0:
                    g.panel((sx, y+edge*(h*.5-.037), depth(.127)), leaf_width-rail_width*2, .074, shutters, facing, tag)
        inner_half = (leaf_width-rail_width*2)*.5
        pitch = (h-.15)/slats
        for index in range(slats):
            yy = sill+.075+(index+.5)*pitch
            g.quad([(sx-inner_half, yy-pitch*.30, depth(.118)), (sx+inner_half, yy-pitch*.30, depth(.118)),
                    (sx+inner_half, yy+pitch*.33, depth(.091)), (sx-inner_half, yy+pitch*.33, depth(.091))],
                   slat_colour, (0, .25, facing), tag)
        if g.d == 0:
            for hinge_y in (y-h*.34, y+h*.34):
                g.panel((sx+side*(leaf_width*.5-.035), hinge_y, depth(.132)), .085, .043, 'ironGrey', facing, tag)
            g.panel((sx-side*(leaf_width*.5-.092), y-.07, depth(.132)), .027, .12, 'woodLight', facing, tag)


def wrought_balcony(g, x, y, z, width, facing, reach):
    tag = 'FacadeBalcony'
    edge = z+facing*reach
    middle = (z+edge)*.5
    half = width*.5
    wall_face = '-z' if facing > 0 else '+z'
    g.cube((x, y-.065, middle), (width, .13, reach), 'trim', tag, skip=(wall_face,))
    if g.d == 0:
        g.cube((x, y-.154, edge-facing*.046), (width+.035, .048, .125), 'stone', tag, skip=('+y',))
    front = edge-facing*.035
    for level, thick in ((.13, .038), (.94, .062)):
        if g.d == 0:
            g.cube((x, y+level, front), (width-.042, thick, .065), 'iron', tag)
        else:
            g.panel((x, y+level, front), width-.042, thick, 'iron', facing, tag)
        for side in ((-1, 1) if g.d < 2 or level > .5 else ()):
            end = x+side*(half-.026)
            g.quad([(end, y+level-thick*.5, z), (end, y+level-thick*.5, front),
                    (end, y+level+thick*.5, front), (end, y+level+thick*.5, z)], 'iron', (side, 0, 0), tag)
    divisions = max(4, round(width/(.45, .60, .88)[g.d]))
    step = (width-.10)/divisions
    for index in range(divisions+1):
        xx = x+(index-divisions*.5)*step
        if g.d == 0 and index in (0, divisions):
            g.cube((xx, y+.48, front), (.065, .96, .065), 'iron', tag, skip=('-y',))
            g.cube((xx, y+.99, front), (.092, .038, .085), 'ironGrey', tag, skip=('-y',))
        else:
            g.panel((xx, y+.52, front), .043 if g.d else .029, .79, 'iron', facing, tag)
    if g.d == 2:
        return
    for side in (-1, 1):
        end = x+side*(half-.026)
        for fraction in ((.32, .65) if g.d == 0 else (.5,)):
            bar = z+(front-z)*fraction
            g.quad([(end, y+.13, bar-.018), (end, y+.13, bar+.018),
                    (end, y+.94, bar+.018), (end, y+.94, bar-.018)], 'iron', (side, 0, 0), tag)
    # A row of forged lozenges between the upright bars. Narrow ribbons keep the rail open.
    z_detail = front+facing*.002
    for index in range(0, divisions, 1 if g.d == 0 else 2):
        cx = x+(index+.5-divisions*.5)*step
        cy = y+.57
        hw, hh, band = step*.43, .22, .038
        outer = [(cx, cy+hh), (cx+hw, cy), (cx, cy-hh), (cx-hw, cy)]
        inner = [(cx, cy+hh-band), (cx+hw-band, cy), (cx, cy-hh+band), (cx-hw+band, cy)]
        for j in range(4):
            k = (j+1)%4
            g.quad([(a, b, z_detail) for a, b in (outer[j], outer[k], inner[k], inner[j])],
                   'greenDark', (0, 0, facing), tag)


def timber_glazed_window(g, x, y, z, width, height, facing=-1, *, see_through=False, compact=False):
    """Four timber leaves with a continuous, recessed ventilation header."""
    tag = 'FacadeWindow'
    sill, head = y-height*.5, y+height*.5
    def depth(offset): return z+facing*offset
    def panel(cx, cy, offset, w, h, color, material_tag=tag):
        g.panel((cx, cy, depth(offset)), w, h, color, facing, material_tag)
    wall_face = '+z' if facing < 0 else '-z'
    if see_through:
        for side in (-1, 1):
            panel(x+side*(width*.5+.041), y, .060, .099, height+.16, 'trim')
        panel(x, head+.025, .060, width, .044, 'trim')
    else:
        g.cube((x, y, depth(.029)), (width+.18, height+.16, .062), 'trim', tag, skip=(wall_face,))
        panel(x, y, .061, width+.024, height+.018, 'woodDark')
    g.cube((x, sill-.047, depth(.083)), (width+.23, .085, .21), 'trim', tag, skip=(wall_face,))
    casing = .063
    for side in (-1, 1):
        stile_x = x+side*(width-casing)*.5
        if compact and g.d > 0:
            panel(stile_x, y, .138, casing, height, 'wood')
        else:
            g.cube((stile_x, y, depth(.099)), (casing, height, .078), 'wood', tag, skip=(wall_face,))
    aperture = width-2*casing
    vent_h, lower_h = height*.14, height*.17
    vent_y = head-vent_h*.5
    glass_low, glass_high = sill+lower_h, head-vent_h
    vents = (9, 7, 5)[g.d]
    vent_pitch = aperture/vents
    hole = min(.11, vent_pitch*.48)
    strip = vent_h*.285
    flat_vents = compact and g.d > 0
    if flat_vents:
        panel(x, vent_y, .104, aperture, vent_h, 'wood')
    else:
        for side in (-1, 1):
            panel(x, vent_y+side*(vent_h-strip)*.5, .104, aperture, strip, 'wood')
    for i in range(vents):
        vx = x+(i+.5-vents*.5)*vent_pitch
        panel(vx, vent_y, .107 if flat_vents else .079, hole, vent_h*.43, 'darkFrame')
        if not flat_vents:
            gap = (vent_pitch-hole)*.5
            for side in (-1, 1):
                panel(vx+side*(hole+gap)*.5, vent_y, .104, gap, vent_h*.43, 'wood')
        if g.d == 0:
            panel(vx, vent_y-vent_h*.19, .117, hole, .021, 'woodLight')

    span = aperture/4
    for i in range(4):
        cx = x+(i-1.5)*span
        panel(cx, (glass_low+glass_high)*.5, .077, span, glass_high-glass_low,
              'glass' if i in (0, 3) else 'glassLight', 'Glass' if see_through else tag)
        panel(cx, sill+lower_h*.5, .107, span if see_through else span-.024, lower_h, 'wood')
        if g.d == 0:
            panel(cx, sill+lower_h*.5, .115, span-.085, lower_h*.59, 'woodLight')
    for index in (1, 2, 3):
        xx = x-aperture*.5+index*span
        panel(xx, y-vent_h*.5, .118, .045, height-vent_h, 'wood')
    for part in (.0, .36, .72, 1.0):
        panel(x, glass_low+(glass_high-glass_low)*part, .115, aperture, .041, 'woodLight')
    if g.d < 2:
        for side in (-1, 1):
            g.cube((x+side*.074, y-.058, depth(.134)), (.024, .127, .036), 'woodLight', tag, skip=(wall_face,))


def terracotta_balcony(g, x, y, z, width, reach, facing=-1, *, compact=False):
    """Two rows of open diagonal clay blocks, between plastered returns."""
    tag = 'FacadeBalcony'
    edge = z+facing*reach
    middle = (z+edge)*.5
    wall_face = '+z' if facing < 0 else '-z'
    g.cube((x, y-.069, middle), (width, .139, reach), 'trim', tag, skip=(wall_face,))
    g.cube((x, y-.148, edge-facing*.080), (width+.032, .040, .184), 'stone', tag, skip=('+y',))
    if g.d < 2:
        for side in (-1, 1):
            g.cube((x+side*width*.34, y-.227, z+facing*.17), (.22, .161, .31), 'stone', tag, skip=('+y', wall_face))
    how_many = (11, 8, 6)[g.d]
    gable = .14
    span = (width-2*gable)/how_many
    rail_z = edge-facing*.077
    thickness = .132
    block_height = .313
    front = rail_z+facing*thickness*.5
    rib_front = front+facing*(.002 if compact and g.d > 0 else 0)
    for side in (-1, 1):
        g.cube((x+side*(width-gable)*.5, y+.329, middle),
               (gable, .658, reach-.045), 'cream', tag, skip=('-y', wall_face))
    g.cube((x, y+.684, rail_z), (width+.038, .059, .175), 'wood', tag, skip=('-y',))
    for row in range(3):
        yy = y+.027+row*block_height
        if compact and g.d > 0:
            g.panel((x, yy, front), width-2*gable, .044, 'tile', facing, tag)
        else:
            g.cube((x, yy, rail_z), (width-2*gable, .044, thickness), 'tile', tag)
    for i in range(how_many+1):
        xx = x+(i-how_many*.5)*span
        if compact and g.d > 0:
            g.panel((xx, y+.339, front), .041, .605, 'tile', facing, tag)
        else:
            g.cube((xx, y+.339, rail_z), (.041, .605, thickness), 'tile', tag, skip=('-y', '+y'))
    for row in range(2):
        cy = y+.027+(row+.5)*block_height
        for i in range(how_many):
            cx = x+(i+.5-how_many*.5)*span
            dx, dy = span*.50, block_height*.46
            direction = 1 if (row+i)%2 == 0 else -1
            corners = [
                (cx-dx, cy-direction*dy-.028, rib_front), (cx+dx, cy+direction*dy-.028, rib_front),
                (cx+dx, cy+direction*dy+.028, rib_front), (cx-dx, cy-direction*dy+.028, rib_front),
            ]
            g.quad(corners, 'woodLight', (0, 0, facing), tag)
            if g.d == 0:
                back = [(a, b, c-facing*thickness) for a, b, c in corners]
                g.quad(back, 'tile', (0, 0, -facing), tag)
                for a, b, normal_y in ((0, 1, -1), (3, 2, 1)):
                    g.quad([corners[a], corners[b], back[b], back[a]], 'tile', (0, normal_y, 0), tag)


def tile_canopy(g, x, w, y, z, reach, facing, *, drop=.26, compact=False, rows=None, tag='FacadeTiles'):
    end = z+reach*facing
    g.wedge(x-w*.5, x+w*.5, y-.051, z, y-.051-drop, end, 'woodDark', .035, tag)
    courses = rows if rows is not None else ((2, 1, 1)[g.d] if not compact else 1)
    spacing = (.48, .92, 1.45) if compact else (.27, .68, 1.34)
    tiles = max(2, round(w/spacing[g.d]))
    width = w/tiles
    profile = ((0, 0), (.28, .048), (.62, .063), (1, 0)) if g.d == 0 and not compact else ((0, 0), (.50, .06), (1, 0))
    for tile in range(tiles):
        x0 = x-w*.5+tile*width
        for course in range(courses):
            back = course/courses
            edge = (course+1)/courses
            for (a, rise_a), (b, rise_b) in zip(profile, profile[1:]):
                xa, xb = x0+width*a, x0+width*b
                color = 'tileDark' if (a >= .50 or (tile+course*3)%11 == 7) else 'tile'
                g.quad([(xa, y-.051-drop*back+rise_a, z+facing*reach*back),
                        (xb, y-.051-drop*back+rise_b, z+facing*reach*back),
                        (xb, y-.040-drop*edge+rise_b, z+facing*reach*edge),
                        (xa, y-.040-drop*edge+rise_a, z+facing*reach*edge)], color, (0, 1, facing*.3), tag)
                if g.d == 0 and not compact and course == courses-1:
                    g.quad([(xa, y-.040-drop+rise_a, end), (xb, y-.040-drop+rise_b, end),
                            (xb, y-.077-drop, end), (xa, y-.077-drop, end)], 'tileDark', (0, 0, facing), tag)
        if compact or g.d > 0:
            lip = y-.051-drop
            g.add([(x0,lip,end), (x0+width*.5,lip+.071,end), (x0+width,lip,end)],
                  [(0,1,2)], 'tileDark', ('d',(0,0,facing)), tag)
    if g.d == 0 and not compact and tag == 'FacadeTiles':
        for side in (-1, 1):
            px = x+side*(w*.5-.13)
            g.cyl((px, y-.49, z+facing*.035), (px, y-.078-drop*.83, z+facing*reach*.83), .026, 'woodDark', n=4, caps=False, tag=tag)
