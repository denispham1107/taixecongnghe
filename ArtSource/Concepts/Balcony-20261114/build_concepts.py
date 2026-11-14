"""Editable Blender concepts for Phố Việt. Run outside Unity's Assets folder."""

from collections import defaultdict
from math import cos, pi, sin
from pathlib import Path
import runpy

import bpy
from mathutils import Vector


HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
PALETTE = {
    "paint": ("82B4A0", .79, 0), "cream": ("E5D8B8", .73, 0),
    "mortar": ("C8B898", .83, 0), "cut": ("F1E7CE", .71, 0),
    "stone": ("C6B993", .72, 0), "recess": ("34372D", .64, 0),
    "tile": ("B9593E", .84, 0), "tile_dark": ("87412E", .88, 0),
    "tile_light": ("C66F50", .84, 0),
    "iron": ("27322D", .41, .58), "bronze": ("966F39", .34, .65),
    "teal": ("19504A", .53, 0), "teal_edge": ("35776A", .50, 0),
    "blue": ("365961", .56, 0), "blue_edge": ("699097", .51, 0),
    "wood": ("945E35", .46, 0), "wood_edge": ("C38C56", .49, 0),
    "glass": ("38555D", .22, .29), "glass_light": ("57747B", .25, .27),
    "curtain": ("D5C7A3", .87, 0), "linen": ("EFE1BD", .89, 0),
    "brick": ("A65737", .82, 0), "brick_edge": ("C57C51", .78, 0),
    "soil": ("372F22", .98, 0), "stem": ("443B24", .90, 0),
    "leaf": ("487836", .80, 0), "leaf_light": ("729748", .78, 0),
    "rose": ("C13B69", .75, 0), "rose_light": ("EB7794", .77, 0),
}


def linear_color(rgb):
    channels = [int(rgb[i:i+2], 16)/255 for i in (0, 2, 4)]
    return tuple(c/12.92 if c < .04045 else ((c+.055)/1.055)**2.4 for c in channels)


def materials():
    result = {}
    for name, (rgb, roughness, metal) in PALETTE.items():
        mat = bpy.data.materials.new(name)
        color = (*linear_color(rgb), 1)
        mat.diffuse_color = color
        mat.use_nodes = True
        tree = mat.node_tree
        bs = next(node for node in tree.nodes if node.type == "BSDF_PRINCIPLED")
        bs.inputs["Base Color"].default_value = color
        bs.inputs["Roughness"].default_value = roughness
        bs.inputs["Metallic"].default_value = metal
        if name in {"paint", "tile", "tile_dark", "tile_light", "cream", "stone", "brick"}:
            tex = tree.nodes.new("ShaderNodeTexNoise")
            tex.inputs["Scale"].default_value = 88 if name != "paint" else 57
            tex.inputs["Detail"].default_value = 2.2
            tex_coord = tree.nodes.new("ShaderNodeTexCoord")
            tree.links.new(tex_coord.outputs["Object"], tex.inputs["Vector"])
            bump = tree.nodes.new("ShaderNodeBump")
            bump.inputs["Strength"].default_value = .28
            bump.inputs["Distance"].default_value = .015
            tree.links.new(tex.outputs["Fac"], bump.inputs["Height"])
            tree.links.new(bump.outputs["Normal"], bs.inputs["Normal"])
        if name.startswith("glass"):
            bs.inputs["Coat Weight"].default_value = .37
            bs.inputs["Coat Roughness"].default_value = .19
        result[name] = mat
    return result


class Geometry:
    def __init__(self):
        self.data = defaultdict(lambda: ([], []))

    def poly(self, material, vertices, faces):
        points, polys = self.data[material]
        base = len(points)
        points.extend(vertices)
        polys.extend(tuple(base+j for j in face) for face in faces)

    def box(self, material, center, dim, angle=0):
        x, y, z = center
        a, b, c = (d/2 for d in dim)
        points = []
        for u, v, w in ((-a,-b,-c),(a,-b,-c),(a,b,-c),(-a,b,-c),
                        (-a,-b,c),(a,-b,c),(a,b,c),(-a,b,c)):
            points.append((x+u*cos(angle)-v*sin(angle),y+u*sin(angle)+v*cos(angle),z+w))
        self.poly(material, points, ((3,2,1,0),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)))

    def tube(self, material, path, radius, sides=8, taper=1):
        points = [Vector(p) for p in path]
        verts = []
        for i, point in enumerate(points):
            axis = (points[min(i+1,len(points)-1)]-points[max(i-1,0)]).normalized()
            ref = Vector((0,1,0)) if abs(axis.y)<.92 else Vector((1,0,0))
            u = ref.cross(axis).normalized()
            v = axis.cross(u)
            r = radius*(1+(taper-1)*i/(len(points)-1))
            verts.extend(point+r*(cos(2*pi*j/sides)*u+sin(2*pi*j/sides)*v) for j in range(sides))
        last = len(verts)-sides
        faces = [tuple(reversed(range(sides))),tuple(range(last,last+sides))]
        for i in range(0,last,sides):
            faces.extend((i+j,i+(j+1)%sides,i+sides+(j+1)%sides,i+sides+j) for j in range(sides))
        self.poly(material, verts, faces)

    def shape(self, material, outline, back, front):
        """Extrude a CCW X/Z outline toward the street."""
        size = len(outline)
        pts = [(x,y,z) for y in (front,back) for x,z in outline]
        faces = [tuple(range(size)),tuple(reversed(range(size,size*2)))]
        faces.extend((i,i+size,(i+1)%size+size,(i+1)%size) for i in range(size))
        self.poly(material, pts, faces)

    def flush(self, collection, mats):
        for name,(vertices,faces) in self.data.items():
            mesh = bpy.data.meshes.new(collection.name + " / " + name)
            mesh.from_pydata(vertices, [], faces)
            mesh.update()
            assert not mesh.validate(), mesh.name
            mesh.materials.append(mats[name])
            obj = bpy.data.objects.new(collection.name + " / " + name, mesh)
            collection.objects.link(obj)
            if name not in {"leaf","leaf_light","rose","rose_light","glass","glass_light"}:
                bevel = obj.modifiers.new("Soft edges", "BEVEL")
                bevel.width = .007
                bevel.segments = 2
                bevel.angle_limit = .35
                normals = obj.modifiers.new("Corner normals", "WEIGHTED_NORMAL")
                normals.keep_sharp = True


def line(g, material, a, b, radius=.019):
    g.tube(material, [a,b], radius, 8)


def walls(g):
    for base in (0,3):
        g.box("paint",(0,.17,base+1.5),(8.16,.32,2.96))
        g.box("cream",(0,-.027,base+1.57),(.42,.15,2.70))
        g.box("stone",(0,-.066,base+.035),(8.32,.21,.115))
        g.box("cut",(0,-.088,base+.114),(8.32,.235,.048))
    for x in (-3.99,3.99):
        g.box("cream",(x,-.02,3.07),(.27,.17,6.12))
        g.box("cut",(x,-.123,3.07),(.035,.047,6.12))
    g.box("stone",(0,-.067,6.065),(8.32,.24,.15))
    g.box("cut",(0,-.09,6.164),(8.38,.29,.060))
    g.box("mortar",(0,-.82,-.11),(8.50,2,.18))


def canopy(g, x, z, width, reach):
    # Curved terracotta tiles with overlapping courses and dark fascia.
    g.box("wood",(x, -.53*reach,z-.15),(width-.03,reach,.075))
    columns = round(width/.17)
    step = width/columns
    rows = 4
    for row in range(rows):
        for col in range(columns):
            tx = x+(col+.5-columns/2)*step
            for idx in range(5):
                def point(a, front):
                    u = -reach*(row+front*1.12)/rows
                    return (tx+.58*step*cos(a),u,z+u*.27+.065*sin(a))
                a,b = idx*pi/5,(idx+1)*pi/5
                mat = ("tile_light","tile","tile","tile_dark")[(row*7+col*11)%4]
                g.poly(mat,[point(a,0),point(a,1),point(b,1),point(b,0)],[(0,1,2,3)])
    for dx in (-width/2+.15,width/2-.15):
        line(g,"wood",(x+dx,-.05,z-.35),(x+dx,-reach*.87,z-.21),.030)


def frame(g, x, bottom, w, h, material, side=.074, depth=-.17):
    for px in (x-w/2+side/2,x+w/2-side/2):
        g.box(material,(px,depth,bottom+h/2),(side,.095,h))
    for z in (bottom+side/2,bottom+h-side/2):
        g.box(material,(x,depth,z),(w,.095,side))


def rectangular_bay(g, x, bottom, width, height, kind, upper):
    wood, edge = ("teal","teal_edge") if kind=="A" else ("wood","wood_edge")
    g.box("recess",(x,-.023,bottom+height/2),(width,.08,height))
    frame(g,x,bottom,width+.20,height+.15,"cream",.079,-.098)
    frame(g,x,bottom,width+.04,height+.03,"cut",.035,-.153)
    frame(g,x,bottom+.039,width-.10,height-.08,wood,.062,-.19)
    if kind=="A":
        for side in (-1,1):
            shutter_x = x+side*(width*.337)
            sw = width*.27
            frame(g,shutter_x,bottom+.093,sw,height-.185,wood,.049,-.235)
            g.box("recess",(shutter_x,-.204,bottom+height/2),(sw-.055,.032,height-.22))
            for j in range(13 if upper else 9):
                z = bottom+.166+j*(height-.34)/(12 if upper else 8)
                g.poly(edge,[(shutter_x-sw*.43,-.263,z),
                             (shutter_x+sw*.43,-.263,z),
                             (shutter_x+sw*.43,-.211,z+.034),
                             (shutter_x-sw*.43,-.211,z+.034)],[(0,1,2,3)])
            g.box("bronze",(shutter_x-side*(sw/2-.019),-.28,bottom+.53),(.034,.024,.11))
        central = width*.46
        for i in (-1,1):
            g.box("glass_light" if i==1 else "glass",(x+i*central/4,-.151,bottom+height*.57),(central/2-.028,.027,height*.65))
            g.box(wood,(x+i*central/4,-.181,bottom+.23),(central/2-.025,.058,.34))
        g.box(edge,(x,-.205,bottom+height/2),(.041,.05,height-.11))
        g.box(wood,(x,-.202,bottom+height*.79),(central,.043,.037))
    else:
        # Sliding timber and glass leaves, with a separate row of square vents.
        glazing_bottom = bottom+.13
        glazing_h = height-.41
        for i in range(4):
            px=x+((i+.5)/4-.5)*(width-.17)
            g.box("glass_light" if i in (1,2) else "glass",(px,-.162,glazing_bottom+glazing_h*.55),
                  ((width-.17)/4-.029,.027,glazing_h*.9))
            g.box(wood,(px,-.197,bottom+.19),((width-.17)/4-.031,.044,.22))
            for bar_z in (.43,.75):
                g.box(edge,(px,-.202,glazing_bottom+glazing_h*bar_z),((width-.17)/4,.025,.025))
            if i:
                g.box(wood,(px-(width-.17)/8,-.203,glazing_bottom+glazing_h*.51),(.041,.049,glazing_h+.15))
        g.box("wood",(x,-.217,bottom+height-.19),(width-.11,.065,.26))
        for i in range(9):
            px=x+(i-4)*(width-.22)/9
            g.box("recess",(px,-.252,bottom+height-.175),(.107,.014,.11))
            g.box(edge,(px,-.267,bottom+height-.204),(.107,.014,.024))
        for s in (-1,1):
            g.box("bronze",(x+s*.051,-.246,bottom+height*.49),(.023,.025,.14))


def arched_bay(g, x, bottom, width, height):
    radius = width/2
    spring = bottom+height-.43

    def arch(r, rise):
        return [(x-r,bottom),(x+r,bottom)]+[
            (x+r*cos(i*pi/20),spring+rise*sin(i*pi/20)) for i in range(21)]

    g.shape("cream",arch(radius+.115,.52),-.018,-.147)
    g.shape("cut",arch(radius+.054,.468),-.143,-.18)
    g.shape("blue",arch(radius,.425),-.176,-.231)
    g.shape("glass_light",arch(radius-.085,.343),-.229,-.240)
    for side in (-1,1):
        # Curtains sit over the outer quarter of each opening with cloth folds.
        for i in range(6):
            px = x+side*(radius*.35+i*.065)
            top = spring+.30*(1-((px-x)/radius)**2)**.5
            g.poly("linen" if i%2 else "curtain",
                   [(px,-.255,bottom+.07),(px+side*.049,-.275,bottom+.07),
                    (px+side*.049,-.274,top-.018),(px,-.255,top)],[(0,1,2,3)])
        g.box("blue",(x+side*radius*.48,-.285,bottom+.16),(radius*.84,.079,.30))
        g.box("blue_edge",(x+side*radius*.48,-.333,bottom+.16),(radius*.72,.015,.212))
        g.box("blue",(x+side*radius*.46,-.289,(bottom+spring)/2),(.037,.04,spring-bottom))
        g.box("bronze",(x+side*.082,-.355,bottom+.57),(.038,.044,.13))
    g.box("blue",(x,-.292,(bottom+spring+.37)/2),(.055,.056,spring+.37-bottom))
    g.box("blue",(x,-.281,spring-.013),(width-.11,.059,.049))
    for i in range(1,6):
        t=i*pi/6
        line(g,"blue",(x,-.272,spring),(x+(radius-.06)*cos(t),-.273,spring+.346*sin(t)),.017)
    g.shape("stone",[(x-.066,spring+.438),(x+.066,spring+.438),(x+.100,spring+.558),(x-.100,spring+.558)],-.155,-.217)


def balcony(g,x,kind):
    z=3.28
    if kind=="B":
        # A bowed front and a matching balcony edge, sampled as a shallow arc.
        path=[(x+i*1.60/12,-1.005+.29*(i/12)**2) for i in range(-12,13)]
    else:
        path=[(x-1.57,-.87),(x+1.57,-.87)]
    outline = path+[(path[-1][0],-.015),(path[0][0],-.015)]
    pts=[(a,b,z+h) for h in (-.085,.085) for a,b in outline]
    n=len(outline)
    g.poly("stone",pts,[tuple(reversed(range(n))),tuple(range(n,n*2))]+[
        (j,(j+1)%n,(j+1)%n+n,j+n) for j in range(n)])
    line_path=[(a,b,z+.068) for a,b in outline+[outline[0]]]
    g.tube("cut",line_path,.033,6)
    rail_path=[(path[0][0],-.10)]+path+[(path[-1][0],-.10)]
    if kind=="C":
        # Glazed terracotta screen, with four separate sides around each aperture.
        for i in range(11):
            cx=x+(i-5)*.282
            for row in range(2):
                cz=z+.216+row*.287
                for side in (-1,1):
                    g.box("brick",(cx+side*.116,-.84,cz),(.051,.151,.277))
                    g.box("brick_edge",(cx,-.84,cz+side*.115),(.187,.151,.047))
                direction = 1 if (i+row)%2 == 0 else -1
                line(g,"brick_edge",(cx-.084,-.922,cz-.084*direction),
                     (cx+.084,-.922,cz+.084*direction),.023)
        for px in (x-1.50,x+1.50):
            g.box("cream",(px,-.49,z+.38),(.16,.77,.64))
        g.box("wood",(x,-.86,z+.723),(3.22,.185,.075))
        for px in (x-1.27,x+1.27):
            g.box("stone",(px,-.22,z-.23),(.24,.38,.26))
        return
    for zz,radius in ((z+.21,.016),(z+.796,.026)):
        g.tube("iron",[(a,b,zz) for a,b in rail_path],radius,8)
    for cx in (x-1.54,x+1.54):
        for yy in (-.11, -.72 if kind=="B" else -.86):
            g.box("iron",(cx,yy,z+.43),(.061,.061,.80))
            g.tube("bronze",[(cx,yy,z+.815),(cx,yy,z+.866)],.047,10,taper=.30)
    for idx in range(13):
        px=x+(idx-6)*.239
        yy=-1.005+.29*((px-x)/1.6)**2 if kind=="B" else -.87
        if kind=="A" and idx%2==1:
            for side in (-1,1):
                path=[(px+side*(.022+.084*sin(t*pi/10)),yy,z+.252+.49*t/10) for t in range(11)]
                g.tube("iron",path,.014,6)
            g.tube("iron",[(px+.068*cos(i*pi/9),yy-.008,z+.496+.068*sin(i*pi/9)) for i in range(19)],.011,6)
        else:
            g.tube("iron",[(px,yy,z+.21),(px,yy-(.085 if kind=="B" else 0),z+.40),(px,yy,z+.798)],.013,8)
    for px in (x-1.1,x+1.1):
        g.shape("stone",[(px-.11,z-.31),(px+.11,z-.31),(px+.18,z-.085),(px-.18,z-.085)],-.055,-.38)


def plant(g,x,y,z,flowers=False,shallow=False,seed=0):
    from random import Random
    rng=Random(seed)
    if shallow:
        g.box("brick",(x,y,z+.082),(.81,.27,.164))
        g.box("brick_edge",(x,y,z+.167),(.86,.297,.037))
        g.box("soil",(x,y,z+.188),(.77,.227,.027))
    else:
        g.tube("brick",[(x,y,z),(x,y,z+.24)],.118,14,taper=1.35)
        g.tube("brick_edge",[(x,y,z+.213),(x,y,z+.253)],.163,14)
        g.tube("soil",[(x,y,z+.252),(x,y,z+.26)],.135,14)
    for i in range(11 if flowers else 8):
        a=i*2.39
        reach=(.23 if shallow else .34)*(.5+rng.random()*.5)
        dx,dy=cos(a)*reach,sin(a)*reach
        dz=rng.uniform(.11,.27) if shallow else rng.uniform(.17,.47)
        if not shallow and flowers and i%3==0:
            dy-=.34
            dz*= -.86
        px=x+(i%5-2)*.133 if shallow else x
        origin=(px,y,z+(.19 if shallow else .27))
        tip=(px+dx,y+dy,origin[2]+dz)
        line(g,"stem",origin,tip,.007)
        for j in (1,2,3):
            f=j/3
            cx,cy,cz=(u+(v-u)*f for u,v in zip(origin,tip))
            for sign in (-1,1):
                leaf_dx=cos(a+sign*.86)*.090
                leaf_dy=sin(a+sign*.86)*.090
                g.poly("leaf" if j%2 else "leaf_light",
                       [(cx,cy,cz),(cx+leaf_dx*.60-.036,cy+leaf_dy*.45,cz+.027),
                        (cx+leaf_dx,cy+leaf_dy,cz+.043),
                        (cx+leaf_dx*.60+.035,cy+leaf_dy*.45,cz+.017)],[(0,1,2,3)])
        if flowers:
            cx,cy,cz=tip
            for j in range(4):
                a=j*pi/2+.25
                g.poly("rose_light" if (i+j)%3==0 else "rose",
                       [(cx,cy,cz),(cx+.075*cos(a-.52),cy+.047*sin(a),cz+.06*sin(a-.52)),
                        (cx+.091*cos(a),cy-.046,cz+.078*sin(a)),
                        (cx+.075*cos(a+.60),cy+.047*sin(a),cz+.06*sin(a+.60))],[(0,1,2,3)])


def collection(scene,name):
    coll=bpy.data.collections.new(name)
    scene.collection.children.link(coll)
    return coll


def keep_existing_lanterns():
    exporter=runpy.run_path(str(ROOT / "ArtSource/Blender/export_tube_houses_21186.py"))
    base=exporter["library"]()
    street=base["library"]()
    base["materials"](street)
    return street


def object_data(coll,name,data,location,target=None):
    obj=bpy.data.objects.new(name,data)
    coll.objects.link(obj)
    obj.location=location
    if target:
        obj.rotation_euler=(Vector(target)-obj.location).to_track_quat("-Z","Y").to_euler()
    return obj


def make_scene(kind,title,mats,street):
    scene=bpy.data.scenes.new(f"{kind} - {title}")
    wall=Geometry()
    walls(wall)
    wall.flush(collection(scene,kind+" / Walls"),mats)
    for upper in (True,False):
        g=Geometry()
        z=3 if upper else 0
        for i,x in enumerate((-1.94,1.94)):
            bottom=z+(.39 if upper else .76)
            width=2.44 if upper else 2.08
            height=2.10 if upper else 1.53
            if kind=="B":
                arched_bay(g,x,bottom,width,height)
            else:
                rectangular_bay(g,x,bottom,width,height,kind,upper)
            if upper:
                balcony(g,x,kind)
                plant(g,x-.94,-.68,3.373,flowers=i==0,seed=27+i)
                plant(g,x+.92,-.65,3.373,flowers=i==1,seed=82+i)
            else:
                g.box("stone",(x,-.218,bottom-.032),(width+.27,.50,.12))
                g.box("cut",(x,-.243,bottom+.039),(width+.31,.51,.051))
                plant(g,x,-.36,bottom+.064,flowers=True,shallow=True,seed=10+i*370)
            canopy(g,x,z+2.84,3.32 if upper else 2.52,.73 if upper else .61)
        g.flush(collection(scene,kind+(" / Balcony and French doors" if upper else " / Windows and sills")),mats)
    fixtures=collection(scene,kind+" / Existing lanterns")
    for level in (0,3):
        geo=street["Geo"](0)
        street["facade_lantern"](geo, level+2.39, 0)
        ob=street["to_mesh"](f"{kind} / Existing lantern {level}",geo,fixtures)
        ob.rotation_euler.z=pi
        ob.location.y=-.102
    stage=collection(scene,kind+" / Cameras and lighting")
    target=(.0,-.16,3.07)
    camera=object_data(stage,"Elevation / A-B-C",bpy.data.cameras.new(kind+" camera"),(.16,-19,3.22),target)
    camera.data.type="ORTHO"
    camera.data.ortho_scale=8.64
    scene.camera=camera
    detail=object_data(stage,"Detail / oblique",bpy.data.cameras.new(kind+" detail"),
                       (5.4,-13,5.5),(-.05,-.24,3.48))
    detail.data.type="ORTHO"
    detail.data.ortho_scale=8.54
    world=bpy.data.worlds.new(kind+" warm daylight")
    world.use_nodes=True
    bg=next(n for n in world.node_tree.nodes if n.type=="BACKGROUND")
    bg.inputs["Color"].default_value=(.56,.67,.77,1)
    bg.inputs["Strength"].default_value=.39
    scene.world=world
    sun=object_data(stage,"Afternoon sun",bpy.data.lights.new(kind+" sun","SUN"),(-4,-8,9),(0,0,2))
    sun.data.energy=2.36
    sun.data.angle=.17
    sun.data.color=(1,.88,.72)
    soft=object_data(stage,"Street fill",bpy.data.lights.new(kind+" fill","AREA"),(3,-7,7),(0,0,3))
    soft.data.energy=360
    soft.data.shape="DISK"
    soft.data.size=7
    scene.render.engine="CYCLES"
    scene.cycles.samples=22
    scene.cycles.use_denoising=True
    scene.render.resolution_x=1240
    scene.render.resolution_y=940
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.view_settings.view_transform="AgX"
    scene.render.filepath="//Mau-"+kind+".png"
    assert len(fixtures.objects)==2 and len(scene.objects)>30
    return scene


def main():
    assert not bpy.data.filepath, "Use a separate Blender session."
    mats=materials()
    street=keep_existing_lanterns()
    scenes=[make_scene(kind,title,mats,street) for kind,title in (
        ("A","Sat ren va cua chop"),("B","Vom cung va ban cong cong"),("C","Gach thong gio va go") )]
    if bpy.context.window:
        bpy.context.window.scene=scenes[0]
        for area in bpy.context.screen.areas:
            if area.type=="VIEW_3D":
                area.spaces.active.region_3d.view_perspective="CAMERA"
                area.spaces.active.shading.color_type="MATERIAL"
    bpy.ops.wm.save_as_mainfile(filepath=str(HERE / "PhoViet_BaMauBanCong.blend"))
    print("Created", [(scene.name,len(scene.objects)) for scene in scenes])


if __name__=="__main__":
    main()
