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
