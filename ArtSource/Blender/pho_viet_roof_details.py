"""Mái dốc, sân thượng và hạ tầng mái của dãy nhà Phố Việt."""


def pitched_roof(g, y, facade, width):
    ridge_z, front_z, rear_z = -.42, -4.24, 4.12
    eave, ridge = y+.33, y+1.82
    courses = (6, 3, 1)[g.d]
    for end, direction in ((front_z, -1), (rear_z, 1)):
        tile_canopy(g, 0, width+.13, ridge, ridge_z, abs(end-ridge_z), direction,
                    drop=ridge-eave, rows=courses, tag='RoofTiles')
        g.cube((0, eave-.067, end), (width+.18, .105, .14), 'tileDark', 'RoofTiles')
    g.cyl((-width*.5-.07, ridge+.016, ridge_z), (width*.5+.07, ridge+.016, ridge_z), .11,
          'tileDark', n=(8, 6, 4)[g.d], tag='RoofTiles')
    front_wall = ridge-.081-(ridge-eave)*abs(-3.90-ridge_z)/abs(front_z-ridge_z)
    rear_wall = ridge-.081-(ridge-eave)*abs(3.90-ridge_z)/abs(rear_z-ridge_z)
    for edge, top, facing in ((-3.90, front_wall, -1), (3.90, rear_wall, 1)):
        g.panel((0,(y+.18+top)*.5,edge),width,top-y-.18,facade,facing,'RoofTiles')
    for side in (-1, 1):
        x = side*width*.5
        g.add([(x,y+.18,-3.90), (x,front_wall,-3.90), (x,ridge-.081,ridge_z),
               (x,rear_wall,3.90), (x,y+.18,3.90)], [(0,1,2,3,4)], facade, ('d',(side,0,0)), 'RoofTiles')
        if g.d < 2:
            g.cube((x+side*.016, y+.76, ridge_z), (.032, .50, .89), 'trim', 'RoofTiles')
            for slat in range(4):
                g.quad([(x+side*.036, y+.59+slat*.105, ridge_z-.35),
                        (x+side*.036, y+.64+slat*.105, ridge_z-.35),
                        (x+side*.036, y+.64+slat*.105, ridge_z+.35),
                        (x+side*.036, y+.59+slat*.105, ridge_z+.35)], 'darkFrame', (side,0,0), 'RoofTiles')


def roof_terrace(g, x, y, z, width, depth, facade, variant, *, compact=False):
    tag = 'RoofTerrace'
    left, right = x-width*.5, x+width*.5
    front, back = z-depth*.5, z+depth*.5
    parapet = .47 if compact else .59
    for edge, facing in ((front, -1), (back, 1)):
        g.cube((x, y+parapet*.5, edge), (width, parapet, .12), facade, tag, skip=('-y',))
        g.cube((x, y+parapet, edge), (width+.025, .069, .19), 'trim', tag, skip=('-y',))
        if g.d < 2:
            g.panel((x, y+parapet+.30, edge), width-.16, .054, 'iron', facing, tag)
            posts = max(3, round(width/(.48 if g.d == 0 else .85)))
            for index in range(posts+1):
                px = left+.08+(width-.16)*index/posts
                g.panel((px, y+parapet+.153, edge), .033, .30, 'iron', facing, tag)
    for xx in (left+.06, right-.06):
        g.cube((xx, y+parapet*.5, z), (.12, parapet, depth-.12), facade, tag, skip=('-y',))
        if not compact:
            g.cube((xx, y+parapet, z), (.19, .069, depth-.1), 'trim', tag, skip=('-y',))

    # Phòng thang lùi sau mép mái; lối mở nhìn ra sân, phía kia dành cho bồn nước.
    direction = 1 if variant%2 else -1
    room_x = x-direction*width*.23
    room_z = z+depth*.22
    rw, rd = min(2.35,width*.42), min(2.18,depth*.42)
    room_h = 1.30 if compact else 2.08
    if not compact or variant%3 != 1:
        g.cube((room_x, y+room_h*.5, room_z), (rw, room_h, rd), facade, tag, skip=('-y',))
        g.panel((room_x, y+(room_h-.16)*.5, room_z-rd*.5-.005), rw*.41, room_h-.16, 'greenDark', -1, tag)
        if g.d == 0:
            g.panel((room_x+rw*.12, y+room_h*.46, room_z-rd*.5-.009), .075, .055, 'chrome', -1, tag)
        roof_y, roof_z, roof_depth = y+room_h+.38, room_z+rd*.55, rd+.25
        back_y = roof_y-.077-.27*(roof_z-room_z-rd*.5)/roof_depth
        front_y = roof_y-.077-.27*(roof_z-room_z+rd*.5)/roof_depth
        g.wedge(room_x-rw*.5,room_x+rw*.5,back_y,room_z+rd*.5,front_y,room_z-rd*.5,facade,.36,tag)
        tile_canopy(g, room_x, rw+.26, roof_y, roof_z, roof_depth, -1,
                    drop=.27, compact=True, tag='RoofTiles')

    tank_x, tank_z = x+direction*width*.24, z+depth*.21
    if not compact or variant%2 == 0:
        radius = .31 if compact else .49
        bottom, top = y+.25, y+(1.03 if compact else 1.42)
        g.cube((tank_x, y+.125, tank_z), (radius*1.75,.25,radius*1.7), 'ironGrey', tag, skip=('-y',))
        g.cyl((tank_x,bottom,tank_z), (tank_x,top,tank_z), radius, 'steel', n=(10,8,6)[g.d], tag=tag)
        if g.d < 2:
            for fraction in ((.23,.52,.80) if g.d == 0 else (.52,)):
                band = bottom+(top-bottom)*fraction
                g.cyl((tank_x,band-.018,tank_z), (tank_x,band+.018,tank_z), radius+.011, 'chrome',
                      n=10 if g.d == 0 else 8, caps=False, tag=tag)
            g.cyl((tank_x,top,tank_z), (tank_x,top+.075,tank_z), radius*.42, 'chrome', n=8, tag=tag)
        if g.d == 0 and not compact:
            g.cyl((tank_x+radius*.81,bottom,tank_z), (tank_x+radius*.81,y+.035,tank_z), .032, 'ironGrey', n=5, tag=tag)

    if compact:
        return
    # Sân lát gạch, giàn che sắt một số căn và dải chậu cây ven lan can.
    g.cube((x, y+.013, z), (width-.31,.026,depth-.31), 'pavingBeige', tag, skip=('-y',))
    if g.d < 2:
        for j in range(-2,3):
            g.cube((x+j*width/6,y+.029,z), (.019,.009,depth-.36), 'pavingJoint', tag, skip=('-y',))
        for j in range(-2,3):
            g.cube((x,y+.03,z+j*depth/6), (width-.36,.009,.019), 'pavingJoint', tag, skip=('-y',))
    for index, offset in enumerate((-.30, 0, .30) if g.d == 0 else (-.28,.28)):
        px, pz = x+offset*width, front+.56
        g.cube((px,y+.17,pz), (.53,.34,.43), 'tileDark', tag, skip=('-y',))
        if g.d == 0:
            mini_tree(g, (px,y+.33,pz), .68, g.d, variant*71+index, clusters=(4,3,2), per=(8,5,3), folded_leaves=False)
        else:
            g.blob((px,y+.47,pz), (.28,.27,.26), ('leaf2','leaf3','leaf4')[index], n=4, m=2, tag=tag)
    if variant%3 == 0:
        cover_x, cover_z = x+direction*width*.23, z-depth*.14
        canopy_w, canopy_d = min(2.65,width*.36), min(2.38,depth*.41)
        for sx in (-1,1):
            for sz in (-1,1):
                g.cube((cover_x+sx*canopy_w*.43,y+1.035,cover_z+sz*canopy_d*.43), (.065,2.07,.065), 'iron', tag)
        tile_canopy(g,cover_x,canopy_w,y+2.22,cover_z+canopy_d*.5,canopy_d,-1,
                    drop=.19,compact=True,tag='RoofTiles')
        if g.d < 2:
            g.cube((cover_x,y+.39,cover_z), (canopy_w*.61,.075,.51), 'woodLight', tag)
            for side in (-1,1):
                g.cube((cover_x+side*canopy_w*.23,y+.18,cover_z), (.077,.36,.44), 'ironGrey', tag)
    elif variant%3 == 1:
        panel_x, panel_z = x+direction*width*.23, z-depth*.12
        for dx in (-.51,.51):
            px = panel_x+dx
            g.wedge(px-.48,px+.48,y+.61,panel_z+.54,y+.23,panel_z-.54,'steel',.06,tag)
            for fraction in (0,1):
                g.cube((px,y+(.18+.19*fraction),panel_z+(.42 if fraction else -.42)),
                       (.78,.36+.38*fraction,.053), 'ironGrey', tag, skip=('-y',))
            g.quad([(px-.43,y+.252,panel_z-.49),(px+.43,y+.252,panel_z-.49),
                    (px+.43,y+.594,panel_z+.49),(px-.43,y+.594,panel_z+.49)], 'navy', (0,1,0), tag)
            if g.d == 0:
                for t in (.25,.50,.75):
                    zz = panel_z-.49+.98*t
                    yy = y+.254+.342*t
                    g.quad([(px-.43,yy,zz-.006),(px+.43,yy,zz-.006),(px+.43,yy+.004,zz+.006),
                            (px-.43,yy+.004,zz+.006)], 'steel', (0,1,0), tag)
