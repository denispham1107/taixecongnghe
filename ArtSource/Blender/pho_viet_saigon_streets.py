"""Phố Sài Gòn theo ảnh mẫu ArtSource/Reference/Street/saigon_street_reference.webp.
Chạy trong Blender thật qua Blender MCP; dựng trong scene riêng `PV_SaigonStreets`,
lưu nguồn mới PhoViet_SaigonStreets.blend (không ghi đè PhoViet_Streets.blend).

Tọa độ tác giả giống bộ phố cũ để Unity giữ nguyên vị trí/hướng/vật cản:
X dọc mặt tiền, Y lên, Z vào nhà (âm = phía đường). Tường mặt tiền z=-3.9.
Xuất lại đúng 16 tên asset cũ (12 shop, 3 cây, 1 vỉa hè), mỗi asset 3 LOD dùng
chung một material atlas; Unity cập nhật mọi khối phố qua GUID FBX sẵn có.
LOD được dựng theo mức chi tiết (không decimate), chữ/mái hiên giữ ở mọi LOD.
"""
import bpy, bmesh, math, os, json, random
from math import sin, cos, pi
from mathutils import Vector, Quaternion

ROOT = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT = ROOT + '/Assets/PhoViet/Art/Streets'
SOURCE = ROOT + '/ArtSource/Blender/PhoViet_SaigonStreets.blend'
FONT = ROOT + '/Assets/PhoViet/Fonts/BeVietnamPro-SemiBold.ttf'
SCENE = 'PV_SaigonStreets'

def B(p): return Vector((-p[0], -p[2], p[1]))
def hexcol(h): return tuple((int(h[i:i+2], 16)/255)**2.2 for i in (0, 2, 4))+(1,)

# 64 màu = lưới 8x8 ô 16 px trong PNG 128 px (Point filter, không mip).
PAL = {
 # Mặt tiền nhiều màu (21.43: trước đây kem/vàng/be gần như toàn vàng, nay mỗi tiệm một màu, chỉ một tiệm vàng):
 # cream=trắng ngà, butter=vàng, beige=hồng phấn, peach=cam san hô, mint=xanh bạc hà, sky=xanh da trời, sand=tím oải hương
 'cream':'F3EADB','butter':'F5D06B','beige':'EDB3BC','peach':'F2A487','mint':'A9DCC4','sky':'A7CBEA','sand':'C8B7E6','stone':'D9D4C7',
 # Viền/cửa/kính
 'white':'FBF8F0','trim':'EFE6D2','glass':'3E5A66','glassLight':'8FB3C0','frame':'F6F3EA','darkFrame':'2B2F33','iron':'1E2124','ironGrey':'4A4F55',
 # Mái ngói, gỗ, tường trong
 'tile':'C2502F','tileDark':'93391F','wood':'8A5A36','woodLight':'B07A4A','woodDark':'5C3A22','interior':'FFE3A0','interiorWall':'F7D58A','lamp':'FFF4C8',
 # Biển hiệu
 'red':'D6312B','redDark':'A6211D','green':'1E6A4A','greenDark':'134A33','yellow':'FFD23F','blue':'2B66B0','orange':'F08A24','navy':'213A63',
 # Mái hiên sọc
 'awnRed':'D93A33','awnGreen':'2E7D55','awnBlue':'2F6FB5','awnYellow':'F2B632','awnWhite':'FAF7EE','pink':'E24C8E','pinkLight':'F28CBB','purple':'9B4BA0',
 # Lá cây, chậu
 'leaf1':'4C8E2A','leaf2':'5FA834','leaf3':'78BE42','leaf4':'9CD25C','leaf5':'2F5F1F','trunk':'8E8B86','soil':'4E3B2A','potGrey':'B9B6AE',
 # Vỉa hè, bó vỉa, kim loại
 'paveCream':'EBD7AE','paveBrown':'B97B58','grout':'BFAE92','curb':'F0CF6E','curbDark':'B8A46E','steel':'A9B0B3','chrome':'D7DDE0','rubber':'26282A',
 # Đồ vật, cờ, hàng hóa
 'stool':'E23B2F','fridge':'D8262B','binGreen':'2F8A4C','chalk':'2A2E2B','chalkText':'F3F1EA','flagRed':'DA251D','flagGold':'FFDD00','goodsB':'3B8FD9',
}
NAMES = list(PAL.keys())
assert len(NAMES) == 64, len(NAMES)
CI = {n: i for i, n in enumerate(NAMES)}
def uv_of(col): return ((col % 8 + .5)/8, (col//8 + .5)/8)

class Geo:
    """Hình học tác giả: mỗi mặt mang màu palette và gợi ý pháp tuyến.
    B() là phép đối xứng nên chiều quay mặt được sửa theo gợi ý sau khi dựng mesh."""
    def __init__(self, detail=0):
        self.v = []; self.f = []; self.cols = []; self.hint = []; self.tags = []; self.d = detail
    def add(self, verts, faces, col, hint, tag=''):
        s = len(self.v); self.v.extend(verts); c = CI[col] if isinstance(col, str) else col
        for i, face in enumerate(faces):
            self.f.append(tuple(s+k for k in face)); self.cols.append(c); self.tags.append(tag)
            self.hint.append(hint[i] if isinstance(hint, list) else hint)
    def cube(self, c, s, col, tag='', skip=()):
        x, y, z = c; hx, hy, hz = s[0]/2, s[1]/2, s[2]/2
        vs = [(x+a*hx, y+b*hy, z+e*hz) for a in (-1, 1) for b in (-1, 1) for e in (-1, 1)]
        faces = {'-x': (0, 1, 3, 2), '+x': (4, 6, 7, 5), '-y': (0, 4, 5, 1), '+y': (2, 3, 7, 6), '-z': (0, 2, 6, 4), '+z': (1, 5, 7, 3)}
        fs = [f for k, f in faces.items() if k not in skip]
        self.add(vs, fs, col, ('c', (x, y, z)), tag)
    def quad(self, pts, col, facing, tag=''):
        self.add(list(pts), [(0, 1, 2, 3)], col, ('d', facing), tag)
    def panel(self, c, w, h, col, z_facing=-1, tag=''):
        # Tấm phẳng song song mặt tiền, quay về phía đường (-z).
        x, y, z = c
        self.quad([(x-w/2, y-h/2, z), (x+w/2, y-h/2, z), (x+w/2, y+h/2, z), (x-w/2, y+h/2, z)], col, (0, 0, z_facing), tag)
    def cyl(self, a, b, r, col, n=8, r2=None, caps=True, tag=''):
        a, b = Vector(a), Vector(b); axis = (b-a).normalized(); u = axis.cross(Vector((0, 1, 0)))
        if u.length < .01: u = axis.cross(Vector((1, 0, 0)))
        u.normalize(); w = axis.cross(u).normalized(); r2 = r if r2 is None else r2
        vs = [tuple(p+(u*cos(i*2*pi/n)+w*sin(i*2*pi/n))*rad) for p, rad in ((a, r), (b, r2)) for i in range(n)]
        fs = [(i, (i+1) % n, (i+1) % n+n, i+n) for i in range(n)]
        if caps: fs += [tuple(range(n-1, -1, -1)), tuple(range(n, 2*n))]
        self.add(vs, fs, col, ('c', tuple((a+b)/2)), tag)
    def torus(self, c, r, minor, col, axis='y', n=12, m=5, tag=''):
        vs = []
        for i in range(n):
            a = 2*pi*i/n
            for j in range(m):
                b = 2*pi*j/m; rr = r+minor*cos(b); ax = minor*sin(b)
                dd = (rr*cos(a), ax, rr*sin(a)) if axis == 'y' else (ax, rr*sin(a), rr*cos(a))
                vs.append((c[0]+dd[0], c[1]+dd[1], c[2]+dd[2]))
        fs = [(i*m+j, ((i+1) % n)*m+j, ((i+1) % n)*m+(j+1) % m, i*m+(j+1) % m) for i in range(n) for j in range(m)]
        # Pháp tuyến hướng ra khỏi lõi ống: gợi ý theo từng mặt bằng tâm vòng tròn lõi gần nhất.
        hints = []
        for i in range(n):
            a = 2*pi*(i+.5)/n
            core = (c[0]+r*cos(a), c[1], c[2]+r*sin(a)) if axis == 'y' else (c[0], c[1]+r*sin(a), c[2]+r*cos(a))
            hints += [('c', core)]*m
        self.add(vs, fs, col, hints, tag)
    def blob(self, c, s, col, n=6, m=4, tag=''):
        vs = [(c[0], c[1]+s[1], c[2])]
        for j in range(1, m):
            a = pi*j/m
            for i in range(n):
                t = 2*pi*i/n+(j % 2)*pi/n; vs.append((c[0]+s[0]*sin(a)*cos(t), c[1]+s[1]*cos(a), c[2]+s[2]*sin(a)*sin(t)))
        vs.append((c[0], c[1]-s[1], c[2])); last = len(vs)-1; fs = []
        for i in range(n): fs.append((0, 1+(i+1) % n, 1+i))
        for j in range(m-2):
            for i in range(n):
                a0 = 1+j*n; a1 = 1+(j+1)*n; fs.append((a0+i, a0+(i+1) % n, a1+(i+1) % n, a1+i))
        b0 = 1+(m-2)*n
        for i in range(n): fs.append((last, b0+i, b0+(i+1) % n))
        self.add(vs, fs, col, ('c', c), tag)
    def disc(self, c, r, col, facing=(0, 0, -1), n=10, tag=''):
        f = Vector(facing).normalized(); u = f.cross(Vector((0, 1, 0)))
        if u.length < .01: u = f.cross(Vector((1, 0, 0)))
        u.normalize(); w = f.cross(u).normalized(); c = Vector(c)
        vs = [tuple(c)]+[tuple(c+(u*cos(i*2*pi/n)+w*sin(i*2*pi/n))*r) for i in range(n)]
        self.add(vs, [(0, 1+i, 1+(i+1) % n) for i in range(n)], col, ('d', tuple(f)), tag)
    def leaf(self, base, direction, length, width, col, roll=0, tag='Leaf', face=None, wpos=.35):
        base = Vector(base); axis = Vector(direction).normalized()
        if face is not None:
            # Mặt lá quay theo `face` (vd. pháp tuyến ra ngoài tán) thay vì để mép lá hướng về người nhìn.
            side = Vector(face).cross(axis)
            if side.length < .05: side = axis.cross(Vector((0, 1, .2)))
            side.normalize(); side = Quaternion(axis, roll*.35)@side
        else:
            side = axis.cross(Vector((0, 1, .2)))
            if side.length < .1: side = axis.cross(Vector((1, 0, 0)))
            side.normalize(); side = Quaternion(axis, roll)@side
        normal = side.cross(axis).normalized()
        mid = base+axis*length*.42+normal*width*.25
        pts = [base, base+axis*length*wpos+side*width, base+axis*length, base+axis*length*wpos-side*width, mid]
        vs = [tuple(p) for p in pts]
        # Hai mặt: mặt trước theo normal, mặt sau ngược lại (Standard culling).
        self.add(vs, [(4, 0, 1), (4, 1, 2), (4, 2, 3), (4, 3, 0)], col, ('d', tuple(normal)), tag)
        self.add([tuple(p-normal*.004) for p in pts], [(4, 1, 0), (4, 2, 1), (4, 3, 2), (4, 0, 3)], col, ('d', tuple(-normal)), tag)
    def wedge(self, x0, x1, y0, z0, y1, z1, col, thick=.08, tag=''):
        # Mái dốc: tấm có độ dày từ (y0,z0) phía tường tới (y1,z1) phía ngoài.
        pts = [(x0, y0, z0), (x1, y0, z0), (x1, y1, z1), (x0, y1, z1)]
        bot = [(a, b-thick, c) for a, b, c in pts]; vs = pts+bot
        fs = [(0, 1, 2, 3), (7, 6, 5, 4), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)]
        cen = (sum(p[0] for p in vs)/8, sum(p[1] for p in vs)/8, sum(p[2] for p in vs)/8)
        self.add(vs, fs, col, ('c', cen), tag)
    def text(self, value, pos, size, col, maxwidth=7.0, bold=.0, shear=0., facing=-1, align='CENTER', tag='Lettering'):
        curve = bpy.data.curves.new('PV_Lettering', 'FONT'); curve.body = value; curve.size = size
        curve.align_x = align; curve.align_y = 'CENTER'; curve.resolution_u = 1; curve.shear = shear
        curve.offset = bold*size; curve.font = bpy.data.fonts.load(FONT, check_existing=True)
        obj = bpy.data.objects.new('PV_Lettering', curve); bpy.context.scene.collection.objects.link(obj)
        obj.location = B(pos); obj.rotation_euler = (pi/2, 0, pi if facing < 0 else 0)
        bpy.context.view_layer.update()
        if obj.dimensions.x > maxwidth: obj.scale *= maxwidth/obj.dimensions.x
        bpy.context.view_layer.update()
        deps = bpy.context.evaluated_depsgraph_get(); e = obj.evaluated_get(deps); m = e.to_mesh()
        pts = [obj.matrix_world@v.co for v in m.vertices]
        vs = [(-p.x, p.z, -p.y) for p in pts]; fs = [tuple(f.vertices) for f in m.polygons]
        width = obj.dimensions.x
        self.add(vs, fs, col, ('d', (0, 0, facing)), tag)
        e.to_mesh_clear(); bpy.data.objects.remove(obj, do_unlink=True); bpy.data.curves.remove(curve)
        return width
    def star(self, c, r, col, facing=-1, tag='Lettering'):
        vs = [c]
        for i in range(10):
            a = pi*.5+i*pi/5; rad = r if i % 2 == 0 else r*.382; vs.append((c[0]+rad*cos(a), c[1]+rad*sin(a), c[2]))
        self.add(vs, [(0, i+1, (i+1) % 10+1) for i in range(10)], col, ('d', (0, 0, facing)), tag)

# ---------------------------------------------------------------- shop catalogue
# Tên hiển thị trùng VietnamStreet.Shops để vật cản/kiểm tra nhận diện đúng.
SHOPS = [
 dict(name='PHỞ BÒ - GÀ', main='PHỞ', side='BÒ - GÀ', sub='', board='white', ink='red', subink='red', stripe=('awnRed', 'awnWhite'), facade='cream', kind='pho', floors=3, roof='tile', menu=('PHỞ', ['Tái - Nạm  40K', 'Gầu  40K', 'Bò viên  35K', 'Gà  35K'])),
 dict(name='TIỆM TRÀ', main='TIỆM TRÀ', sub='TRÀ SỮA - TRÀ TRÁI CÂY', board='green', ink='white', subink='white', stripe=('awnGreen', 'awnWhite'), facade='mint', kind='tea', floors=4, roof='flat', icon='leaf'),
 dict(name='BÁNH MÌ', main='BÁNH MÌ', sub='CHẢ CÁ - PATE - XÍU MẠI', board='white', ink='red', subink='redDark', stripe=('awnRed', 'awnWhite'), facade='peach', kind='banhmi', floors=2, roof='tile'),
 dict(name='TẠP HÓA', main='TẠP HÓA', sub='BÁCH HÓA TỔNG HỢP', board='red', ink='white', subink='white', stripe=('awnRed', 'awnWhite'), facade='sky', kind='grocery', floors=3, roof='flat', icon='star'),
 dict(name='THUỐC TÂY', main='THUỐC TÂY', sub='NHÀ THUỐC - MỞ CỬA 24H', board='white', ink='blue', subink='green', stripe=('awnBlue', 'awnWhite'), facade='beige', kind='pharmacy', floors=4, roof='tile', icon='cross'),
 dict(name='SỬA XE', main='SỬA XE', sub='VÁ VỎ - THAY NHỚT - RỬA XE', board='navy', ink='yellow', subink='white', stripe=('awnBlue', 'awnWhite'), facade='stone', kind='repair', floors=2, roof='flat'),
 dict(name='CÀ PHÊ SÀI GÒN', main='CÀ PHÊ', script='Sài Gòn', sub='', board='green', ink='white', subink='yellow', stripe=('awnGreen', 'awnWhite'), facade='sand', kind='cafe', floors=3, roof='tile', menu=('CÀ PHÊ', ['Cà phê sữa  20K', 'Cà phê đá  18K', 'Bạc xỉu  25K', 'Trà tắc  15K'])),
 dict(name='CƠM TẤM', main='CƠM TẤM', sub='SƯỜN - BÌ - CHẢ', board='yellow', ink='red', subink='redDark', stripe=('awnYellow', 'awnWhite'), facade='mint', kind='food', floors=4, roof='flat'),
 dict(name='BÚN BÒ HUẾ', main='BÚN BÒ HUẾ', sub='ĐẬM ĐÀ HƯƠNG VỊ CỐ ĐÔ', board='red', ink='yellow', subink='white', stripe=('awnRed', 'awnWhite'), facade='sky', kind='food', floors=3, roof='tile'),
 dict(name='HỦ TIẾU NAM VANG', main='HỦ TIẾU', sub='NAM VANG - MÌ - SA TẾ', board='orange', ink='white', subink='white', stripe=('awnYellow', 'awnWhite'), facade='butter', kind='food', floors=2, roof='flat'),
 dict(name='SINH TỐ', main='SINH TỐ', sub='NƯỚC ÉP - TRÁI CÂY TƯƠI', board='pink', ink='white', subink='white', stripe=('awnGreen', 'awnWhite'), facade='sand', kind='tea', floors=3, roof='tile', icon='leaf'),
 dict(name='TIỆM BÁNH', main='TIỆM BÁNH', sub='BÁNH NGỌT - BÁNH KEM', board='blue', ink='white', subink='yellow', stripe=('awnBlue', 'awnWhite'), facade='peach', kind='bakery', floors=4, roof='flat'),
]
WIDTH = 8.36; HX = WIDTH/2; WALL = -3.9; GROUND = 4.9; STOREY = 3.0

def shop_height(spec): return GROUND+spec['floors']*STOREY

def awning(g, spec, obstacles):
    a, b = spec['stripe']; n = 14; w = 7.36/n
    y0, z0, y1, z1 = 3.22, -4.02, 2.8, -5.4
    for i in range(n):
        x = -3.68+i*w
        g.wedge(x, x+w, y0, z0, y1, z1, a if i % 2 == 0 else b, .04, 'Awning')
        # Riềm lượn sóng phía trước, mỗi dải một vạt.
        col = a if i % 2 == 0 else b; drop = .34
        pts = [(x, y1, z1-.005), (x+w, y1, z1-.005), (x+w*.85, y1-drop, z1-.01), (x+w*.5, y1-drop-.07, z1-.012), (x+w*.15, y1-drop, z1-.01)]
        g.add(pts, [(0, 1, 2, 3, 4)], col, ('d', (0, .15, -1)), 'Awning')
        g.add([(p[0], p[1], p[2]+.012) for p in pts], [(4, 3, 2, 1, 0)], col, ('d', (0, -.15, 1)), 'Awning')
    for x in (-3.55, 3.55):
        g.cyl((x, 2.35, WALL-.05), (x, y1-.02, z1+.05), .025, 'iron', n=5)
    obstacles.append(('Mái hiên', (0, 2.95, -4.7), (7.4, .5, 1.4)))

def sign(g, spec, obstacles):
    board = spec['board']; trim = 'white' if board != 'white' else spec['ink']
    g.cube((0, 4.1, WALL-.1), (7.6, 1.55, .2), trim)
    g.cube((0, 4.1, WALL-.205), (7.36, 1.36, .02), board, skip=('+z',))
    g.cube((0, 4.86, WALL-.12), (7.7, .08, .26), 'darkFrame')  # nẹp trên
    z = WALL-.218; ink = spec['ink']
    if spec.get('side'):
        wmain = g.text(spec['main'], (-1.0, 4.12, z), 1.05, ink, 3.6, bold=.03)
        g.text(spec['side'], (2.15, 4.02, z), .6, ink, 2.7, bold=.03)
        g.cube((0, 4.72, WALL-.21), (7.2, .08, .02), ink, skip=('+z',))
    elif spec.get('script'):
        g.text(spec['main'], (-.35, 4.3, z), 1.02, ink, 6.0, bold=.035)
        g.text(spec['script'], (1.25, 3.68, z), .62, spec['subink'], 3.2, bold=.025, shear=.3)
    else:
        g.text(spec['main'], (0, 4.27 if spec['sub'] else 4.1, z), .92, ink, 6.5, bold=.035)
        if spec['sub']: g.text(spec['sub'], (0, 3.66, z), .27, spec['subink'], 6.2, bold=.025)
    icon = spec.get('icon')
    if icon == 'star': g.star((-3.25, 4.1, z), .3, 'yellow')
    if icon == 'cross':
        g.cube((-3.25, 4.1, z), (.42, .14, .02), 'green', skip=('+z',)); g.cube((-3.25, 4.1, z), (.14, .42, .02), 'green', skip=('+z',))
    if icon == 'leaf':
        g.leaf((-3.35, 3.9, z), (.6, 1, 0), .45, .14, 'leaf4', roll=0, tag='Lettering')
    obstacles.append(('Bảng hiệu', (0, 4.1, -4.0), (7.6, 1.6, .26)))

def storefront(g, spec, obstacles):
    d = g.d; fac = spec['facade']
    # Cột hai bên, dầm trên, nền bậc.
    for x in (-3.95, 3.95): g.cube((x, 1.65, WALL-.02), (.46, 3.3, .3), fac)
    g.cube((0, .12, WALL-.25), (8.3, .24, .6), 'stone')
    # Đèn tường hai bên cột như ảnh mẫu.
    for x in (-3.95, 3.95):
        g.cube((x, 2.55, WALL-.25), (.2, .3, .16), 'lamp'); g.cube((x, 2.74, WALL-.25), (.26, .05, .2), 'iron')
        if d < 2: g.cube((x, 2.55, WALL-.18), (.04, .3, .04), 'iron')
    interior(g, spec)
    if spec['kind'] != 'repair': glass_front(g, spec)
    sidewalk(g, spec, obstacles)

DEPTH = 3.4   # chiều sâu lòng cửa hàng nhìn qua vách kính (tường trong ở z = -0,5)
GOODS = ['red', 'yellow', 'goodsB', 'green', 'orange', 'white', 'pink', 'awnBlue']
DOOR_X = {'grocery': .45, 'pharmacy': .4, 'bakery': .4}

def shelf_wall(g, x, z, w, h, rows, d, cols=GOODS, frame='woodLight', depth=.38, facing=-1):
    # Kệ áp tường, mặt kệ quay về phía kính (facing -1 = về đường).
    g.cube((x, .25+h/2, z), (w, h, depth), frame)
    n = max(2, int(w/.32)) if d == 0 else max(2, int(w/.6))
    for row in range(rows):
        y = .45+row*(h-.3)/max(1, rows-1) if rows > 1 else .6
        g.cube((x, y, z+facing*(depth/2+.02)), (w, .04, .12), 'white')
        if d < 2:
            for k in range(n):
                g.cube((x-w/2+.16+k*(w-.32)/max(1, n-1), y+.14, z+facing*(depth/2+.06)), (.2, .24, .16), cols[(k+row*3) % len(cols)])

def menu_board(g, x, y, z, w, h, d, board='chalk', items=('red', 'yellow', 'green', 'orange')):
    g.cube((x, y, z), (w+.08, h+.08, .04), 'woodDark'); g.panel((x, y, z-.025), w, h, board)
    if d < 2:
        for k in range(4):
            ix = x-w/2+.25+(k % 2)*(w/2); iy = y+h/4-(k//2)*(h/2)
            g.panel((ix, iy, z-.03), .3, .22, items[k % len(items)])
            g.panel((ix+.42, iy, z-.03), .3, .03, 'chalkText')

def interior(g, spec):
    """Lòng cửa hàng sâu 3,4 m, sáng đèn ấm, đồ đạc theo loại tiệm (nhìn thấy qua vách kính)."""
    d = g.d; kind = spec['kind']; back = WALL+DEPTH
    g.cube((0, 3.2, WALL+DEPTH/2), (7.5, .12, DEPTH), 'interiorWall', skip=('-z',))
    g.panel((0, 1.7, back), 7.5, 3.0, 'interiorWall')
    for x in (-3.72, 3.72):
        g.quad([(x, .24, WALL), (x, .24, back), (x, 3.2, back), (x, 3.2, WALL)], 'interior', (-x, 0, 0))
    if d < 2:
        for xi in range(12):
            for zi in range(5):
                x = -3.42+xi*.62; z = WALL+.34+zi*.68
                g.quad([(x-.31, .25, z-.34), (x+.31, .25, z-.34), (x+.31, .25, z+.34), (x-.31, .25, z+.34)], 'trim' if (xi+zi) % 2 else 'paveCream', (0, 1, 0))
    else:
        g.quad([(-3.72, .25, WALL), (3.72, .25, WALL), (3.72, .25, back), (-3.72, .25, back)], 'paveCream', (0, 1, 0))
    for x in (-2.4, 0, 2.4):
        for z in ((WALL+1.0, WALL+2.4) if d == 0 else (WALL+1.6,)):
            g.cube((x, 3.12, z), (.4, .06, .4), 'lamp')
            if d < 2: g.cyl((x, 3.14, z), (x, 2.8, z), .012, 'iron', n=4, caps=False); g.blob((x, 2.72, z), (.13, .1, .13), 'lamp', n=6, m=3)
    if kind == 'cafe':
        g.cube((-1.6, .75, back-.45), (3.6, 1.0, .6), 'wood'); g.cube((-1.6, 1.27, back-.45), (3.7, .06, .66), 'woodLight')
        g.cube((-2.6, 1.55, back-.5), (.6, .5, .4), 'steel'); g.cube((-2.6, 1.82, back-.5), (.5, .05, .35), 'chrome')
        shelf_wall(g, -1.6, back-.15, 3.4, 1.0, 2, d, cols=['white', 'woodLight', 'white', 'leaf3'], frame='wood', depth=.25)
        g.cube((-1.6, 2.2, back-.2), (3.4, .2, .3), 'wood')
        menu_board(g, 2.0, 2.3, back-.03, 2.2, 1.0, d)
        for tx in (1.0, 2.7):
            cafe_table(g, tx, WALL+1.5)
            for cz, bz in ((-.55, -1), (.55, 1)): cafe_chair(g, tx, WALL+1.5+cz, bz)
        if d < 2:
            # Hũ cà phê/ly trên kệ và đèn thả bóng tròn trên quầy.
            for k in range(6): g.cyl((-3.0+k*.5, 1.31, back-.35), (-3.0+k*.5, 1.5, back-.35), .07, ('woodDark', 'white', 'awnYellow')[k % 3], n=6)
        if d < 2:
            for j in range(8): g.leaf((3.35, .9, back-.4), (cos(j*.8)*.5, 1, sin(j*.8)*.5), .55, .16, ('leaf2', 'leaf3')[j % 2])
            g.cube((3.35, .55, back-.4), (.4, .6, .4), 'potGrey')
    elif kind in ('pho', 'food', 'banhmi'):
        g.cube((0, .8, back-.4), (6.8, 1.1, .6), 'steel'); g.cube((0, 1.37, back-.4), (6.9, .05, .66), 'chrome')
        for k, x in enumerate((-2.4, -1.4, 1.6)):
            g.cyl((x, 1.4, back-.4), (x, 1.85, back-.4), .25 if k < 2 else .2, 'chrome', n=10 if d == 0 else 6)
        if kind == 'banhmi':
            for k in range(10 if d == 0 else 4): g.blob((-.4+k*.24, 1.48, back-.45), (.1, .06, .2), 'woodLight', n=6, m=3)
        menu_board(g, -1.5, 2.45, back-.03, 2.6, .9, d, board=('red' if kind != 'banhmi' else 'yellow'), items=('orange', 'yellow', 'white', 'green'))
        menu_board(g, 1.8, 2.45, back-.03, 2.2, .9, d, board='chalk')
        for tx in (-1.6, 1.4):
            pho_table(g, tx, WALL+1.5)
            for sx in ((-.55, .55) if d < 2 else ()): stool(g, tx+sx, WALL+1.5)
        if d < 2:
            # Chồng tô, rổ rau trên bếp và đèn lồng đỏ treo trần.
            for k in range(3):
                for j in range(4 if d == 0 else 2): g.cyl((.2+k*.32, 1.4+j*.06, back-.45), (.2+k*.32, 1.46+j*.06, back-.45), .1, 'white', n=8, r2=.12)
            g.cube((-.5, 1.45, back-.45), (.4, .1, .3), 'woodLight'); g.blob((-.5, 1.53, back-.45), (.17, .06, .12), 'leaf3', n=6, m=3)
            for lx in (-1.2, 1.2):
                g.cyl((lx, 3.14, WALL+.9), (lx, 2.75, WALL+.9), .01, 'iron', n=3, caps=False)
                g.blob((lx, 2.55, WALL+.9), (.17, .21, .17), 'red', n=8, m=4); g.cyl((lx, 2.33, WALL+.9), (lx, 2.36, WALL+.9), .1, 'yellow', n=6)
    elif kind == 'tea':
        top = spec['board'] if spec['board'] in ('green', 'pink') else 'leaf3'
        g.cube((0, .75, WALL+1.6), (3.6, 1.0, .6), 'white'); g.cube((0, 1.27, WALL+1.6), (3.7, .05, .66), top)
        if d < 2:
            for k in range(8): g.cyl((-1.4+k*.4, 1.3, WALL+1.55), (-1.4+k*.4, 1.55, WALL+1.55), .07, ('orange', 'pink', 'leaf4', 'yellow')[k % 4], n=6, r2=.09)
        shelf_wall(g, 0, back-.15, 5.6, 1.2, 2, d, cols=['yellow', 'orange', 'pink', 'leaf4', 'white'], frame='white', depth=.25)
        menu_board(g, 0, 2.45, back-.03, 4.6, .9, d, board=top if top != 'leaf3' else 'green', items=('orange', 'pink', 'yellow', 'leaf4'))
    elif kind in ('grocery', 'pharmacy', 'bakery'):
        frame = 'white' if kind == 'pharmacy' else 'woodLight'
        cols = ['white', 'green', 'awnBlue', 'white', 'pink'] if kind == 'pharmacy' else GOODS
        shelf_wall(g, 0, back-.2, 7.2, 2.3, 5, d, cols=cols, frame=frame)
        for x in (-3.5, 3.5):
            g.cube((x, 1.4, WALL+1.9), (.4, 2.3, 2.4), frame)
            if d < 2:
                for row in range(4):
                    for k in range(5 if d == 0 else 2):
                        g.cube((x-(.22 if x > 0 else -.22), .55+row*.5, WALL+.95+k*(1.9/(4 if d == 0 else 1))), (.06, .24, .2), cols[(k+row) % len(cols)])
        if kind == 'grocery':
            shelf_wall(g, -.6, WALL+1.7, 3.0, 1.1, 3, d)
            g.cube((2.2, 1.05, WALL+1.5), (.8, 1.6, .6), 'glassLight'); g.cube((2.2, 1.05, WALL+1.2), (.85, 1.65, .04), 'fridge')
        elif kind == 'pharmacy':
            g.cube((-.4, .7, WALL+1.5), (3.2, .9, .6), 'white'); g.panel((-.4, .75, WALL+1.19), 3.0, .6, 'sky')
            g.cube((-.4, .75, WALL+1.18), (.4, .12, .01), 'green'); g.cube((-.4, .75, WALL+1.18), (.12, .4, .01), 'green')
        else:
            g.cube((-.5, .7, WALL+1.5), (3.4, .9, .65), 'pinkLight'); g.cube((-.5, 1.2, WALL+1.5), (3.4, .1, .65), 'glassLight')
            if d < 2:
                for k in range(8): g.cyl((-1.9+k*.4, 1.25, WALL+1.5), (-1.9+k*.4, 1.4, WALL+1.5), .13, ('pink', 'yellow', 'white', 'woodLight')[k % 4], n=8)
    elif kind == 'repair':
        g.panel((0, 2.0, back-.02), 6.8, 1.6, 'ironGrey')
        if d < 2:
            for k in range(12): g.cube((-3.0+k*.55, 2.0, back-.06), (.06, .5 if k % 2 else .7, .04), ('steel', 'red', 'chrome')[k % 3])
        g.cube((-1.8, .7, back-.5), (2.6, .9, .7), 'red'); g.cube((-1.8, 1.17, back-.5), (2.7, .05, .75), 'steel')
        for j in range(3 if d < 2 else 1): g.cyl((2.2, .25+j*.2, back-.7), (2.2, .43+j*.2, back-.7), .4, 'rubber', n=10 if d == 0 else 6)
        # Cửa cuốn kéo lên hết, hộp cuốn trên đầu.
        g.cube((0, 3.08, WALL-.1), (7.4, .22, .26), 'steel')

def glass_front(g, spec):
    """Vách kính khung tối/gỗ, cửa kính hai cánh có tay nắm như ảnh mẫu; kính dùng vật liệu trong suốt riêng."""
    d = g.d; kind = spec['kind']; frame = 'wood' if kind in ('cafe', 'tea', 'bakery') else 'darkFrame'
    x0, x1 = -3.72, 3.72; dx = DOOR_X.get(kind, .65); dl, dr = dx-.66, dx+.66; zf = WALL-.035
    top = 3.16; sill = .62; transom = 2.62
    # Chân vách (không có ở ô cửa đi), dầm trên và dầm ngang.
    for a, b in ((x0, dl), (dr, x1)): g.cube(((a+b)/2, .43, zf), (b-a, .38, .07), frame)
    g.cube((0, top, zf), (x1-x0, .09, .08), frame); g.cube((0, transom, zf), (x1-x0, .07, .07), frame)
    for a, b in ((x0, dl), (dr, x1)):
        n = max(0, int((b-a)/1.6))
        for x in [a, b]+[a+(b-a)*k/(n+1) for k in range(1, n+1)]:
            door_post = x in (dl, dr)
            y0 = .25 if door_post else sill
            g.cube((x, (y0+top)/2, zf), (.07, top-y0, .08), frame)
    # Hai cánh cửa: khung mảnh, tay nắm inox, chữ MỞ CỬA trên kính.
    g.cube((dx, (.25+transom)/2, zf-.01), (.05, transom-.25, .06), frame)
    g.cube((dx, .3, zf-.01), (dr-dl, .1, .06), frame)
    for s in (-1, 1):
        g.cyl((dx+s*.14, .95, zf-.07), (dx+s*.14, 1.45, zf-.07), .018, 'chrome', n=6)
    if d < 2: g.text('MỞ CỬA', (dx, 1.75, zf-.02), .1, 'white', .9)
    # Mặt kính: ba tấm (hai bên + ô cửa), quay ra đường.
    for a, b, y0 in ((x0, dl, sill+.19), (dl, dr, .35), (dr, x1, sill+.19)):
        g.quad([(a, y0, WALL), (b, y0, WALL), (b, top, WALL), (a, top, WALL)], 'glassLight', (0, 0, -1), 'Glass')

# ---------------------------------------------------------------- detailed street furniture (21.23)
# Đồ vật vỉa hè và nội thất theo ảnh mẫu Sài Gòn: ghế nhựa đỏ chân loe, bàn gỗ nhỏ có khay gia vị,
# bàn ghế gỗ cà phê tựa nan, bảng đen chữ A, chậu cây lá to, tủ kính đồ ăn, tủ nước cửa kính,
# kệ sắt bày chai, két nhựa, dù riềm, thùng rác có nắp/bánh. Mọi đồ vật dựng theo mức chi tiết d.

def stool(g, x, z, h=.46, col='stool'):
    d = g.d
    g.cube((x, h-.02, z), (.28, .04, .28), col)                 # mặt ngồi
    if d < 2: g.cube((x, h-.065, z), (.25, .05, .25), col, skip=('+y', '-y'))  # váy quanh mặt
    for sx in (-1, 1):
        for sz in (-1, 1):
            g.cyl((x+sx*.1, h-.08, z+sz*.1), (x+sx*.15, 0, z+sz*.15), .026, col, n=4)
    if d == 0:
        for k, (a, b) in enumerate((((-.14, -.14), (.14, -.14)), ((.14, -.14), (.14, .14)), ((.14, .14), (-.14, .14)), ((-.14, .14), (-.14, -.14)))):
            g.cyl((x+a[0], .13, z+a[1]), (x+b[0], .13, z+b[1]), .012, col, n=4)

def condiments(g, x, y, z):
    if g.d > 0: return
    g.cube((x, y+.01, z), (.26, .02, .14), 'woodDark')
    for k, (col, cap) in enumerate((('red', 'white'), ('woodDark', 'red'), ('yellow', 'green'))):
        bx = x-.08+k*.08; g.cyl((bx, y+.02, z), (bx, y+.15, z), .025, col, n=6); g.cyl((bx, y+.15, z), (bx, y+.19, z), .012, cap, n=5)
    g.cyl((x+.17, y, z+.02), (x+.17, y+.1, z+.02), .03, 'chrome', n=6)
    for k in range(3): g.cyl((x+.16+k*.01, y+.08, z+.02), (x+.16+k*.01, y+.2, z+.02), .004, 'woodLight', n=3)
    g.cube((x-.2, y+.04, z+.03), (.08, .07, .08), 'white')

def pho_table(g, x, z, top='woodLight'):
    g.cube((x, .66, z), (.72, .04, .52), top)
    if g.d < 2: g.cube((x, .61, z), (.66, .06, .46), 'wood', skip=('+y', '-y'))
    for sx in (-.31, .31):
        for sz in (-.21, .21): g.cube((x+sx, .32, z+sz), (.04, .64, .04), 'woodDark')
    condiments(g, x-.12, .68, z)
    if g.d == 0:
        for k in range(2): g.cyl((x+.18-k*.1, .68, z+.12), (x+.18-k*.1, .74, z+.12), .07, 'white', n=8, r2=.085)

def cafe_table(g, x, z):
    g.cube((x, .74, z), (.78, .05, .66), 'woodLight')
    for sx in (-.33, .33):
        for sz in (-.27, .27): g.cube((x+sx, .36, z+sz), (.05, .72, .05), 'woodDark')
    if g.d == 0:
        g.cyl((x+.15, .765, z), (x+.15, .85, z), .04, 'white', n=8); g.cyl((x-.12, .765, z+.1), (x-.12, .78, z+.1), .07, 'white', n=8)

def cafe_chair(g, x, z, back_z):
    d = g.d; g.cube((x, .46, z), (.42, .05, .42), 'wood')
    for sx in (-.18, .18):
        for sz in (-.18, .18): g.cube((x+sx, .23, z+sz), (.04, .46, .04), 'woodDark')
    by = z+back_z*.19
    for sx in (-.18, .18): g.cube((x+sx, .72, by), (.04, .52, .04), 'woodDark')
    for k in range(3 if d < 2 else 1): g.cube((x, .62+k*.13, by), (.36, .06, .025), 'wood')

def a_frame(g, x, z, title, lines, obstacles):
    # Bảng đen chữ A khung gỗ, mặt trước nghiêng về đường như ảnh mẫu.
    d = g.d; lean = .18; top, bot = 1.32, .05
    for side, zz in ((-1, z), (1, z+.5)):
        zt = z+.25 if side < 0 else z+.25
        zb = z if side < 0 else z+.5
        for sx in (-.36, .36): g.cyl((x+sx, bot, zb), (x+sx, top, zt), .025, 'woodDark', n=4)
    g.cyl((x-.36, top, z+.25), (x+.36, top, z+.25), .03, 'woodDark', n=4)
    # Mặt bảng trước: tứ giác nghiêng giữa hai chân.
    p = lambda yy: z+.25*(yy-bot)/(top-bot)
    y0, y1 = .25, 1.18
    g.add([(x-.33, y0, p(y0)-.012), (x+.33, y0, p(y0)-.012), (x+.33, y1, p(y1)-.012), (x-.33, y1, p(y1)-.012)], [(0, 1, 2, 3)], 'chalk', ('d', (0, .25, -1)))
    for yy in (y0, y1): g.cyl((x-.34, yy, p(yy)-.02), (x+.34, yy, p(yy)-.02), .018, 'wood', n=4)
    sub = Geo(d); sub.text(title, (0, 0, 0), .13, 'chalkText', .55, bold=.03)
    if d == 0:
        for k, line in enumerate(lines):
            sub.text(line, (0, -.17-k*.14, 0), .06, 'chalkText', .56)
    # Chữ đặt lên mặt nghiêng (dời theo y, lùi theo độ nghiêng).
    for i, (vx, vy, vz) in enumerate(sub.v):
        yy = 1.02+vy; sub.v[i] = (x+vx, yy, p(yy)-.022)
    g.add(sub.v, sub.f, 'chalkText', ('d', (0, .25, -1)), 'Lettering')
    obstacles.append(('Bảng thực đơn', (x, .66, z+.25), (.8, 1.3, .6)))

def planter(g, x, z, obstacles, scale=1.0, pot='white'):
    # Chậu vuông cao màu sáng, cây nhiệt đới lá to bản (không phải lá nhọn kiểu lô hội).
    d = g.d; s = scale; h = .62*s
    g.cube((x, h/2, z), (.5*s, h, .5*s), pot); g.cube((x, h+.02, z), (.56*s, .05, .56*s), 'stone')
    g.cube((x, h+.03, z), (.44*s, .02, .44*s), 'soil', skip=('-y',))
    # Cây chậu kiểu cây mẫu (21.25): thân xám, cành chẻ, tán cụm lá bầu dục.
    mini_tree(g, (x, h+.03, z), 1.3*s, d, int(x*131+z*17))
    obstacles.append(('Chậu cây lớn', (x, .95*s, z), (.6*s, 1.9*s, .6*s)))

def trash_bin(g, x, z, obstacles):
    d = g.d
    g.cyl((x, .08, z), (x, .9, z), .26, 'binGreen', n=4 if d == 2 else 4, r2=.29)  # thân vuông loe
    g.cube((x, .94, z+.02), (.48, .06, .5), 'leaf5')
    if d < 2:
        g.cube((x, .96, z+.26), (.4, .04, .05), 'leaf5'); g.cube((x, .7, z-.22), (.18, .1, .04), 'leaf5')
        for sx in (-.18, .18): g.cyl((x+sx, .08, z+.2), (x+sx*1.3, .08, z+.2), .07, 'rubber', n=8)
    obstacles.append(('Thùng rác', (x, .48, z), (.55, .95, .6)))

def umbrella(g, x, z, cols, obstacles, r=1.2, h=2.3):
    d = g.d
    g.cube((x, .05, z), (.4, .1, .4), 'ironGrey'); g.cyl((x, .1, z), (x, h+.18, z), .028, 'white', n=6)
    seg = 12 if d < 2 else 8
    for k in range(seg):
        a0 = k*2*pi/seg; a1 = (k+1)*2*pi/seg; col = cols[k % 2]
        pts = [(x, h+.18, z), (x+r*cos(a0), h-.28, z+r*sin(a0)), (x+r*cos(a1), h-.28, z+r*sin(a1))]
        g.add(pts, [(0, 1, 2)], col, ('d', (0, 1, 0))); g.add(pts, [(0, 2, 1)], col, ('d', (0, -1, 0)))
        if d < 2:
            # Riềm rủ quanh mép dù.
            q = [(x+r*cos(a0), h-.28, z+r*sin(a0)), (x+r*cos(a1), h-.28, z+r*sin(a1)), (x+r*cos((a0+a1)/2)*.98, h-.42, z+r*sin((a0+a1)/2)*.98)]
            g.add(q, [(0, 1, 2)], col, ('c', (x, h, z))); g.add(q, [(0, 2, 1)], col, ('c', (x, h-2, z)))
        if d == 0: g.cyl((x, h+.12, z), (x+r*.95*cos(a0), h-.27, z+r*.95*sin(a0)), .008, 'white', n=3, caps=False)
    obstacles.append(('Cán dù', (x, 1.15, z), (.4, 2.3, .4)))

def name_panel(g, spec, x, y, z, w, h):
    """Bảng tên tiệm trên thân tủ kính (21.25): đúng chữ và màu của biển hiệu (spec main + side/script/sub,
    board/ink/subink, viền như biển), căn giữa theo cả hai trục, chữ tự thu nhỏ để không tràn khỏi bảng."""
    board = spec['board']; trim = 'white' if board != 'white' else spec['ink']; ink = spec['ink']
    g.cube((x, y, z-.006), (w, h, .012), trim)
    g.cube((x, y, z-.014), (w-.06, h-.06, .012), board)
    tz = z-.022
    second = spec.get('side') or spec.get('script') or spec.get('sub') or ''
    if second:
        g.text(spec['main'], (x, y+h*.14, tz), h*.36, ink, w-.18, bold=.03)
        g.text(second, (x, y-h*.24, tz), h*.19, ink if spec.get('side') else spec['subink'], w-.22, bold=.025, shear=.3 if spec.get('script') else 0)
    else:
        g.text(spec['main'], (x, y, tz), h*.42, ink, w-.18, bold=.03)

def mini_tree(g, base, height, d, seed, clusters=(8, 5, 3), per=(24, 16, 12)):
    """Cây chậu thu nhỏ cùng kiểu cây mẫu (21.24): thân xám mảnh, cành chẻ, cụm lá bầu dục xanh tươi."""
    nc = clusters[d]; nl = per[d]
    if nc == 0: return
    r = random.Random(seed); base = Vector(base); s = height/1.6
    top = base+Vector((r.uniform(-.03, .03), height*.55, r.uniform(-.03, .03)))
    g.cyl(tuple(base), tuple(top), .03*s, 'trunk', n=6 if d == 0 else 4, r2=.02*s)
    cc = top+Vector((0, height*.2, 0)); rx = height*.3; ry = height*.24
    for k in range(nc):
        u = r.uniform(-.45, 1); t = r.uniform(0, 2*pi)+k*.7; sq = math.sqrt(max(0, 1-u*u))
        c = cc+Vector((sq*cos(t)*rx*.8, u*ry*.8, sq*sin(t)*rx*.8))
        if d < 2: g.cyl(tuple(top), tuple(c), .013*s, 'trunk', n=4, r2=.008*s, caps=False)
        cr = height*r.uniform(.1, .15); oc = c-cc
        oc = oc.normalized() if oc.length > 1e-3 else Vector((0, 1, 0))
        for j in range(nl):
            v = Vector((r.uniform(-1, 1), r.uniform(-.6, 1), r.uniform(-1, 1)))
            if v.length < .05: v = Vector((0, 1, 0))
            v = (v.normalized()+oc*.6).normalized(); q = c+v*cr*r.uniform(.25, 1.0)
            axis = (v+Vector((r.uniform(-.4, .4), -.35, r.uniform(-.4, .4)))).normalized()
            face = (v+Vector((0, .5, 0))).normalized()
            light = (q.y-cc.y)/ry*.5+(q-cc).length/rx*.6+r.uniform(-.25, .25)+.18
            col = 'leaf4' if light > .95 else 'leaf3' if light > .6 else 'leaf2' if light > .3 else 'leaf1' if light > 0 else 'leaf5'
            ln = height*(.075+r.random()*.03)*(1+.3*d)
            g.leaf(tuple(q), tuple(axis), ln, ln*.5, col, roll=r.uniform(-1, 1), face=tuple(face), wpos=.48)

def food_cabinet(g, x, z, kind, obstacles, spec):
    # Tủ kính bày đồ ăn trên thân inox có bánh xe, nồi nước dùng bên cạnh (phở/bún/hủ tiếu/cơm).
    d = g.d; w = 1.5
    g.cube((x, .52, z), (w, .8, .62), 'steel')
    if d < 2:
        for sx in (-.65, .65): g.cyl((x+sx, .06, z-.2), (x+sx, .06, z+.2), .06, 'rubber', n=6)
    # Nửa dưới tủ: bảng tên tiệm trùng chữ/màu biển hiệu.
    name_panel(g, spec, x, .52, z-.31, w-.08, .62)
    g.cube((x, .94, z), (w+.06, .04, .68), 'chrome')
    gy0, gy1 = .96, 1.55; zf, zb = z-.3, z+.3; x0, x1 = x-w/2, x+w/2
    for sx in (x0, x1):
        for sz in (zf, zb): g.cube((sx, (gy0+gy1)/2, sz), (.03, gy1-gy0, .03), 'chrome')
    g.cube((x, gy1, z), (w+.04, .04, .64), 'chrome')
    # Kính trước/hai bên/trên (vật liệu trong suốt).
    g.quad([(x0, gy0, zf), (x1, gy0, zf), (x1, gy1, zf), (x0, gy1, zf)], 'glassLight', (0, 0, -1), 'Glass')
    for sx, f in ((x0, -1), (x1, 1)): g.quad([(sx, gy0, zf), (sx, gy1, zf), (sx, gy1, zb), (sx, gy0, zb)], 'glassLight', (f, 0, 0), 'Glass')
    g.quad([(x0, gy1+.021, zf), (x1, gy1+.021, zf), (x1, gy1+.021, zb), (x0, gy1+.021, zb)], 'glassLight', (0, 1, 0), 'Glass')
    if d < 2:
        g.cube((x, 1.22, z), (w-.06, .02, .55), 'chrome')
        foods = ('leaf3', 'white', 'orange', 'leaf4', 'redDark', 'awnYellow') if kind != 'banhmi' else ('woodLight', 'woodLight', 'leaf3', 'redDark', 'woodLight', 'orange')
        for k in range(6):
            fx = x0+.18+k*(w-.36)/5
            for yy in (.98, 1.24):
                if kind == 'banhmi': g.blob((fx, yy+.06, z), (.1, .05, .2), foods[(k+int(yy*3)) % 6], n=6, m=3)
                else:
                    g.cyl((fx, yy, z-.08), (fx, yy+.06, z-.08), .07, 'white', n=8, r2=.09)
                    if d == 0: g.cyl((fx, yy+.055, z-.08), (fx, yy+.065, z-.08), .075, foods[(k+int(yy*3)) % 6], n=8)
    # Nồi nước dùng inox có nắp, vá.
    if kind in ('pho', 'food'):
        px = x+w/2+.32
        g.cyl((px, .02, z), (px, .7, z), .18, 'ironGrey', n=8)
        g.cyl((px, .7, z), (px, 1.12, z), .26, 'chrome', n=12 if d == 0 else 8); g.cyl((px, 1.12, z), (px, 1.15, z), .27, 'steel', n=12 if d == 0 else 8)
        if d < 2: g.cyl((px, 1.15, z), (px, 1.22, z), .03, 'iron', n=6); g.cyl((px+.12, 1.0, z), (px+.32, 1.45, z), .012, 'chrome', n=4)
        obstacles.append(('Nồi nước dùng', (px, .6, z), (.56, 1.2, .56)))
    obstacles.append(('Quầy thức ăn', (x, .78, z), (w+.06, 1.56, .68)))

def drink_fridge(g, x, z, obstacles):
    d = g.d; w, h, dep = .82, 1.9, .7
    g.cube((x, h/2+.05, z), (w, h, dep), 'fridge', skip=('-z',))
    zf = z-dep/2
    g.cube((x, h-.08, zf-.01), (w, .2, .02), 'white')                         # băng đầu tủ (không logo)
    g.cube((x, .12, zf-.01), (w, .2, .02), 'fridge')
    for sx in (x-w/2+.03, x+w/2-.03): g.cube((sx, h/2+.05, zf-.01), (.06, h-.2, .02), 'fridge')
    zb = z+dep/2-.04  # vách trong sáng phía sau các chai, nhìn qua cửa kính
    g.quad([(x-w/2+.06, .22, zb), (x+w/2-.06, .22, zb), (x+w/2-.06, h-.18, zb), (x-w/2+.06, h-.18, zb)], 'interior', (0, 0, -1))
    g.quad([(x-w/2+.06, .22, zf-.012), (x+w/2-.06, .22, zf-.012), (x+w/2-.06, h-.18, zf-.012), (x-w/2+.06, h-.18, zf-.012)], 'glassLight', (0, 0, -1), 'Glass')
    for row in range(4):
        y = .3+row*.36; g.cube((x, y, z), (w-.1, .02, dep-.1), 'chrome')
        if d < 2:
            for k in range(5 if d == 0 else 3):
                bx = x-w/2+.14+k*(w-.28)/(4 if d == 0 else 2)
                g.cyl((bx, y+.01, zf+.18), (bx, y+.25, zf+.18), .035, ('red', 'orange', 'leaf3', 'goodsB', 'yellow')[(k+row) % 5], n=6, r2=.03)
    if d < 2: g.cyl((x+w/2-.1, .7, zf-.04), (x+w/2-.1, 1.2, zf-.04), .015, 'chrome', n=4)
    obstacles.append(('Tủ nước', (x, 1.0, z), (w, h+.05, dep)))

def outdoor_shelf(g, x, z, obstacles, w=1.6):
    d = g.d
    for sx in (-w/2, w/2):
        for sz in (-.22, .22): g.cube((x+sx, .8, z+sz), (.035, 1.6, .035), 'ironGrey')
    for row in range(4):
        y = .25+row*.42; g.cube((x, y, z), (w, .03, .48), 'white')
        if d < 2:
            n = 8 if d == 0 else 4
            for k in range(n):
                bx = x-w/2+.12+k*(w-.24)/(n-1); col = ('red', 'yellow', 'goodsB', 'green', 'orange', 'white', 'pink', 'awnBlue')[(k+row*3) % 8]
                if (k+row) % 3: g.cyl((bx, y+.015, z-.08), (bx, y+.27, z-.08), .045, col, n=6, r2=.035)
                else: g.cube((bx, y+.13, z-.06), (.14, .24, .14), col)
    obstacles.append(('Kệ tạp hóa', (x, .8, z), (w+.05, 1.6, .5)))

def crates(g, x, z, obstacles):
    d = g.d
    for k, (col, dy) in enumerate((('yellow', .16), ('yellow', .48), ('red', .8))):
        g.cube((x, dy, z), (.62, .3, .42), col)
        if d < 2:
            g.cube((x, dy+.04, z-.215), (.5, .08, .01), 'awnYellow', skip=('+z',))
            for sx in (-.2, 0, .2): g.cube((x+sx, dy-.05, z-.215), (.12, .1, .01), 'awnYellow', skip=('+z',))
    if d == 0:
        for k in range(6): g.cyl((x-.22+(k % 3)*.22, .95, z-.09+(k//3)*.18), (x-.22+(k % 3)*.22, 1.12, z-.09+(k//3)*.18), .035, 'leaf3', n=5)
    obstacles.append(('Két hàng', (x, .5, z), (.65, 1.0, .45)))

def display_case(g, x, z, kind, obstacles, spec):
    d = g.d; w = 1.4
    g.cube((x, .45, z), (w, .7, .55), 'white' if kind == 'pharmacy' else 'pinkLight')
    g.cube((x, .82, z), (w+.04, .04, .6), 'chrome')
    name_panel(g, spec, x, .45, z-.275, w-.08, .56)
    x0, x1, zf, zb, y0, y1 = x-w/2, x+w/2, z-.27, z+.27, .84, 1.28
    g.quad([(x0, y0, zf), (x1, y0, zf), (x1, y1, zf), (x0, y1, zf)], 'glassLight', (0, 0, -1), 'Glass')
    g.quad([(x0, y1, zf), (x1, y1, zf), (x1, y1, zb), (x0, y1, zb)], 'glassLight', (0, 1, 0), 'Glass')
    for sx in (x0, x1): g.cube((sx, (y0+y1)/2, z), (.03, y1-y0, .55), 'chrome')
    if d < 2:
        for k in range(6):
            fx = x0+.16+k*(w-.32)/5
            if kind == 'pharmacy': g.cube((fx, .92, z), (.16, .14, .12), ('white', 'green', 'awnBlue')[k % 3])
            else:
                g.cyl((fx, .86, z), (fx, 1.0, z), .09, ('pink', 'yellow', 'white', 'woodLight')[k % 4], n=8)
                if d == 0: g.cyl((fx, 1.0, z), (fx, 1.03, z), .05, 'red', n=6)
    if kind == 'pharmacy':
        g.cube((x, 1.55, z+.2), (.42, .42, .06), 'white'); g.cube((x, 1.55, z+.16), (.3, .1, .02), 'green'); g.cube((x, 1.55, z+.16), (.1, .3, .02), 'green')
        g.cyl((x, .84, z+.2), (x, 1.34, z+.2), .02, 'chrome', n=4)
    obstacles.append(('Tủ trưng bày', (x, .64, z), (w+.04, 1.28, .6)))

def toolbox(g, x, z, obstacles):
    d = g.d; g.cube((x, .5, z), (1.0, .8, .5), 'red')
    if d < 2:
        for k in range(4): g.cube((x, .25+k*.18, z-.255), (.9, .02, .01), 'redDark', skip=('+z',)); g.cube((x, .33+k*.18, z-.26), (.3, .03, .02), 'chrome')
        for sx in (-.4, .4): g.cyl((x+sx, .05, z-.15), (x+sx, .05, z+.15), .05, 'rubber', n=6)
    g.cube((x, .92, z), (1.04, .04, .54), 'ironGrey')
    obstacles.append(('Tủ dụng cụ sửa xe', (x, .47, z), (1.05, .95, .55)))

def tyre_stack(g, x, z, obstacles):
    d = g.d
    for j in range(4 if d == 0 else 2):
        g.torus((x, .14+j*.2, z), .28, .1, 'rubber', axis='y', n=14 if d == 0 else 8, m=5 if d == 0 else 4)
        if d == 0: g.disc((x, .14+j*.2, z), .2, 'ironGrey', facing=(0, 1, 0), n=8)
    obstacles.append(('Chồng lốp xe', (x, .45, z), (.8, .9, .8)))

def sidewalk(g, spec, obstacles):
    """Bố trí đồ vật vỉa hè trước từng loại tiệm (dải z -4,05 … -5,95; lối vào cửa để trống)."""
    d = g.d; kind = spec['kind']; door = DOOR_X.get(kind, .65)
    if kind == 'cafe':
        for tx in (-2.55, 2.45):
            cafe_table(g, tx, -4.95)
            for cz, bz in ((-5.55, -1), (-4.35, 1)): cafe_chair(g, tx, cz, bz)
            obstacles.append(('Bàn ăn', (tx, .4, -4.95), (.8, .8, .68)))
            for cz in (-5.55, -4.35): obstacles.append(('Ghế gỗ', (tx, .5, cz), (.45, 1.0, .45)))
        planter(g, -.9, -5.6, obstacles, .75, 'potGrey')
    elif kind in ('pho', 'food', 'banhmi', 'tea'):
        if kind == 'tea':
            umbrella(g, -1.78, -5.6, ('awnYellow', 'awnWhite'), obstacles)
            g.cube((2.45, .55, -4.35), (1.5, .9, .55), 'white'); g.cube((2.45, 1.02, -4.35), (1.56, .04, .6), spec['board'] if spec['board'] in ('green', 'pink') else 'leaf3')
            if d < 2:
                for k in range(5): g.cyl((1.95+k*.25, 1.04, -4.4), (1.95+k*.25, 1.24, -4.4), .05, ('orange', 'pink', 'leaf4', 'yellow', 'white')[k], n=6, r2=.065)
            obstacles.append(('Quầy thức ăn', (2.45, .55, -4.35), (1.56, 1.1, .6)))
        else:
            food_cabinet(g, 2.3, -4.38, kind, obstacles, spec)
        for tx in (-2.6, -.95):
            pho_table(g, tx, -4.98, top='woodLight' if kind != 'tea' else 'white')
            obstacles.append(('Bàn ăn', (tx, .34, -4.98), (.74, .7, .54)))
            for sx, sz in ((-.55, 0), (.55, 0), (0, -.52), (0, .5)):
                if d == 2 and sz: continue
                stool(g, tx+sx, -4.98+sz); obstacles.append(('Ghế nhựa', (tx+sx, .23, -4.98+sz), (.42, .46, .42)))
    elif kind == 'grocery':
        drink_fridge(g, -2.75, -4.42, obstacles)
        outdoor_shelf(g, 2.45, -4.4, obstacles)
        crates(g, -1.55, -4.5, obstacles)
    elif kind in ('pharmacy', 'bakery'):
        display_case(g, 2.5, -4.4, kind, obstacles, spec)
    elif kind == 'repair':
        tyre_stack(g, 2.5, -4.55, obstacles); toolbox(g, -2.2, -4.4, obstacles)
        if d < 2:
            g.cube((.2, .06, -4.7), (.9, .08, .5), 'ironGrey'); g.cyl((.0, .1, -4.7), (.4, .1, -4.7), .05, 'chrome', n=6)
    if spec.get('menu'):
        title, lines = spec['menu']; a_frame(g, 3.4, -5.75, title, lines, obstacles)

def window(g, x, y, w, h, d, shutters=None):
    g.cube((x, y, WALL-.05), (w+.2, h+.2, .1), 'frame')
    g.panel((x, y, WALL-.102), w, h, 'glass')
    if d < 2:
        g.cube((x, y, WALL-.11), (.05, h, .02), 'frame', skip=('+z',)); g.cube((x, y+h*.2, WALL-.11), (w, .05, .02), 'frame', skip=('+z',))
        g.cube((x, y-h/2-.12, WALL-.12), (w+.3, .07, .2), 'trim')
    if shutters and d < 2:
        for s in (-1, 1): g.cube((x+s*(w/2+.22), y, WALL-.08), (.26, h, .05), shutters)

def railing(g, x0, x1, y, z, d, col='iron'):
    g.cube(((x0+x1)/2, y+.95, z), (x1-x0, .05, .06), col)
    g.cube(((x0+x1)/2, y+.12, z), (x1-x0, .05, .05), col)
    if d == 2:
        g.panel(((x0+x1)/2, y+.53, z), x1-x0, .78, 'ironGrey'); return
    step = .14 if d == 0 else .28; n = int((x1-x0)/step)
    for k in range(n+1):
        g.cube((x0+k*(x1-x0)/n, y+.53, z), (.025, .8, .025), col)

def upper_floor(g, spec, k, top, obstacles, r):
    d = g.d; y0 = GROUND+(k-1)*STOREY; fac = spec['facade']
    g.cube((0, y0+.05, WALL-.08), (WIDTH+.02, .16, .2), 'trim')  # gờ sàn
    style = (k+len(spec['name'])) % 3
    if style != 1 or top:
        # Ban công sâu, cửa đi kính khung trắng, lan can sắt đen.
        g.cube((0, y0+.12, -4.4), (7.4, .16, 1.0), 'trim')
        for x in (-2.4, 0, 2.4): window(g, x, y0+1.35, 1.4, 2.1, d)
        railing(g, -3.65, 3.65, y0+.2, -4.85, d)
        for x in (-3.65, 3.65):
            # Lan can hai đầu ban công cũng là song sắt thoáng, không phải tấm đen đặc.
            for yy in (y0+.32, y0+1.15): g.cube((x, yy, -4.4), (.04, .05, .95), 'iron')
            for zz in range(4 if d < 2 else 2): g.cube((x, y0+.73, -3.98-zz*(.85/(3 if d < 2 else 1))), (.025, .8, .025), 'iron')
        # Cây ban công, hoa giấy rủ.
        if d < 2:
            for px in (-3.0, 3.0):
                g.cube((px, y0+.42, -4.55), (.5, .36, .38), 'tileDark')
                mini_tree(g, (px, y0+.6, -4.55), .95, d, int(px*37+y0*11), clusters=(5, 3, 0), per=(18, 12, 0))
        if (k+r.randrange(3)) % 2 == 0:
            # Tầng 1 rủ ngắn để hoa giấy không che chữ biển hiệu (đỉnh biển 4,875 m).
            vine(g, r.uniform(-3, -1), y0+1.1, -4.9, 1.8 if k > 1 else .85, d, flowers=(spec['floors'] >= 3))
        if top and spec['roof'] == 'tile':
            g.wedge(-3.9, 3.9, y0+2.9, WALL+.2, y0+2.35, -5.0, 'tile', .1)
            if d == 0:
                for kk in range(13): g.cube((-3.6+kk*.6, y0+2.66, -4.3), (.05, .05, 1.2), 'tileDark')
        obstacles.append(('Ban công', (0, y0+.6, -4.4), (7.4, 1.0, 1.0)))
    else:
        # Hai cửa sổ có cửa chớp và cục nóng điều hòa.
        for x in (-1.8, 1.8): window(g, x, y0+1.5, 1.5, 1.6, d, shutters='glassLight' if k % 2 else 'green')
        for x in (-1.8, 1.8): g.cube((x, y0+.62, -4.12), (1.7, .1, .35), 'trim')
        if d < 2:
            for x in (-1.8, 1.8):
                g.cube((x, y0+.75, -4.12), (1.5, .2, .3), 'tileDark')
                for j in range(10 if d == 0 else 4):
                    a = j*2.1; g.leaf((x-.65+j*(1.3/(9 if d == 0 else 3)), y0+.84, -4.16), (cos(a)*.5, .8, -.5+sin(a)*.3), .22, .1, ('pink', 'leaf3', 'pinkLight', 'leaf2')[j % 4], roll=a)
        ac(g, 3.25, y0+1.1, d)
    if style == 2 and d < 2: ac(g, -3.3, y0+2.35, d)

def ac(g, x, y, d):
    g.cube((x, y, -4.12), (.75, .55, .42), 'white')
    if d < 2: g.disc((x-.1, y, -4.335), .19, 'ironGrey', n=10 if d == 0 else 6); g.cube((x+.27, y, -4.335), (.12, .4, .01), 'stone', skip=('+z',))
    g.cube((x, y-.32, -4.12), (.6, .04, .5), 'ironGrey')

def vine(g, x, y, z, length, d, flowers=False):
    if d == 2: return
    r = random.Random(int(x*77+y*13))
    for strand in range(4 if d == 0 else 2):
        sx = x+strand*.3
        n = 7 if d == 0 else 3
        for j in range(n):
            yy = y-j*length/n*(.6+.4*r.random())
            cluster = 4 if d == 0 else 2
            for c in range(cluster):
                a = r.uniform(0, 2*pi)
                col = ('pink', 'pinkLight', 'pink', 'leaf3')[(j+c) % 4] if flowers else ('leaf2', 'leaf3', 'leaf1')[(j+c) % 3]
                g.leaf((sx+r.uniform(-.12, .12), yy+r.uniform(-.08, .08), z-r.uniform(0, .1)), (cos(a)*.8, sin(a)*.6-.2, -.5), .17, .1, col, roll=a)

def roof(g, spec, obstacles, r):
    d = g.d; h = shop_height(spec); fac = spec['facade']
    g.cube((0, h+.1, 0), (WIDTH+.06, .2, 7.86), 'trim')
    if spec['roof'] == 'tile':
        # Mái ngói đỏ dốc về đường, đầu hồi tường.
        g.wedge(-HX-.05, HX+.05, h+1.6, 1.2, h+.25, WALL-.5, 'tile', .14)
        if d == 0:
            for k in range(14): g.cube((-3.9+k*.6, h+.93, -1.4), (.06, .05, 5.3), 'tileDark')
        g.cube((0, h+.8, 2.6), (WIDTH, 1.6, 2.6), fac)
        for x in (-HX+.1, HX-.1): g.cube((x, h+.7, -1.2), (.2, 1.3, 5.2), fac)
    else:
        g.cube((0, h+.55, WALL+.1), (WIDTH, .9, .2), fac); g.cube((0, h+1.03, WALL+.1), (WIDTH+.1, .06, .3), 'trim')
        if d < 2: railing(g, -3.6, 3.6, h+1.06, WALL+.1, d, 'ironGrey')
        # Bồn nước inox, cây trên sân thượng.
        g.cyl((-2.2, h+.3, 1.5), (-2.2, h+1.6, 1.5), .55, 'chrome', n=12 if d == 0 else 8)
        if d < 2:
            for px in (1.2, 2.6):
                g.cube((px, h+.45, -2.6), (.6, .5, .6), 'tileDark')
                for j in range(8 if d == 0 else 4):
                    a = j*2.3+px; g.leaf((px, h+.7, -2.6), (cos(a)*.5, .9, sin(a)*.5), .6, .15, ('leaf3', 'leaf2', 'leaf4')[j % 3], roll=.2)
    # Cột tiếp giáp hai bên và ống thoát nước.
    for x in (-HX+.12, HX-.12): g.cube((x, h/2+2.4, WALL-.06), (.24, h-4.6, .14), 'trim')
    g.cyl((HX-.35, .3, WALL-.1), (HX-.35, h, WALL-.1), .045, 'ironGrey', n=6)

def flag(g, x, y):
    g.cyl((x, y, WALL-.05), (x, y+.6, WALL-1.3), .025, 'iron', n=5)
    g.cube((x+.55, y+.25, WALL-1.0), (1.05, .7, .02), 'flagRed')
    g.star((x+.55, y+.25, WALL-1.012), .2, 'flagGold')
    g.star((x+.55, y+.25, WALL-.988), .2, 'flagGold', facing=1)

def blade_sign(g, text, color, x, y):
    # Biển dọc vuông góc mặt tiền nằm trong bề rộng tiệm, không lấn nhà bên.
    g.cube((x, y, -4.45), (.12, 1.78, .99), 'white')
    g.cube((x, y, -4.45), (.16, 1.66, .87), color)
    for side in (-1, 1):
        for k, word in enumerate(text.split()):
            ty = y+.4-k*.5
            # Dựng chữ phẳng quay về -z rồi xoay quanh trục đứng sang hai mặt bên.
            gg = Geo(g.d); gg.text(word, (0, 0, 0), .3, 'white', .8, bold=.03)
            for i, (vx, vy, vz) in enumerate(gg.v): gg.v[i] = (x+side*.083, ty+vy, -4.45+vx*side)
            g.add(gg.v, gg.f, 'white', ('d', (side, 0, 0)), 'Lettering')

def back_tower(g, seed):
    """Nhà cao tầng phía sau dãy phố (21.27): mặt chính diện quay ra đường là vách kính lớn trong suốt
    (submesh PV_StreetGlass) chia ô bằng đố, dải sàn từng tầng; nhìn xuyên thấy văn phòng sáng đèn
    (sàn, trần, máng đèn, bàn, vách sau). Hai hông có cửa sổ theo tầng, mái có lan can, phòng kỹ thuật,
    bồn nước, cục lạnh, ăng-ten. Ba kiểu theo seed: tường kính đố tối, khung bê tông + ban công kính, lam đứng."""
    d = g.d; style = seed % 3
    floors = 8+(seed*3) % 4; fh = 3.2; h = floors*fh+.8
    shell = ('stone', 'white', 'sky', 'mint')[seed % 4]
    frame = ('darkFrame', 'white', 'steel')[style]
    spandrel = ('glass', shell, 'ironGrey')[style]
    x0, x1, zf, zb = -3.6, 3.6, 5.5, 12.5; inner = 2.0
    # Lõi đặc phía sau phần văn phòng, hai hông bao phần văn phòng, đáy không cần.
    g.cube((0, h/2, (zf+inner+zb)/2), (x1-x0, h, zb-zf-inner), shell, skip=('-y', '-z'))
    for x in (x0+.06, x1-.06): g.cube((x, h/2, zf+inner/2), (.12, h, inner), shell, skip=('-y',))
    # Cột góc mặt tiền.
    for x in (x0, x1): g.cube((x, h/2, zf-.08), (.36, h, .32), shell, skip=('-y',))
    pitch = 1.2 if style != 2 else 1.8
    nm = int((x1-x0)/pitch)
    for k in range(floors):
        y0 = k*fh; yt = y0+fh
        # Dải sàn/tường lửng ngoài mặt kính.
        g.cube((0, y0+.28, zf-.06), (x1-x0, .56, .2), spandrel)
        if style == 1 and d < 2:
            # Ban công kính: sàn nhô + lan can kính + tay vịn.
            g.cube((0, y0+.62, zf-.55), (x1-x0-.4, .12, .9), shell)
            g.quad([(x0+.25, y0+.68, zf-.98), (x1-.25, y0+.68, zf-.98), (x1-.25, y0+1.62, zf-.98), (x0+.25, y0+1.62, zf-.98)], 'glassLight', (0, 0, -1), 'Glass')
            g.cube((0, y0+1.64, zf-.98), (x1-x0-.5, .05, .06), 'chrome')
        # Mặt kính lớn trong suốt (một tấm/tầng, chia ô bằng đố).
        g.quad([(x0+.18, y0+.56, zf), (x1-.18, y0+.56, zf), (x1-.18, yt, zf), (x0+.18, yt, zf)], 'glassLight', (0, 0, -1), 'Glass')
        if d < 2:
            for m in range(1, nm):
                x = x0+m*(x1-x0)/nm
                if style == 2: g.cube((x, (y0+.56+yt)/2, zf-.25), (.1, yt-y0-.56, .5), frame)   # lam đứng
                else: g.cube((x, (y0+.56+yt)/2, zf-.04), (.07, yt-y0-.56, .1), frame)
            if d == 0: g.cube((0, y0+2.5, zf-.04), (x1-x0-.36, .05, .08), frame)          # đố ngang ô thoáng
        # Văn phòng phía sau kính: sàn, trần có máng đèn, vách sau, bàn làm việc.
        g.quad([(x0+.12, y0+.56, zf), (x1-.12, y0+.56, zf), (x1-.12, y0+.56, zf+inner), (x0+.12, y0+.56, zf+inner)], 'stone', (0, 1, 0))
        g.quad([(x0+.12, yt-.02, zf), (x1-.12, yt-.02, zf), (x1-.12, yt-.02, zf+inner), (x0+.12, yt-.02, zf+inner)], 'white', (0, -1, 0))
        g.panel((0, (y0+.56+yt)/2, zf+inner), x1-x0-.24, yt-y0-.56, ('interiorWall', 'sky', 'trim')[(k+seed) % 3])
        for lx in ((-2.2, 0, 2.2) if d < 2 else (0,)):
            g.quad([(lx-.7, yt-.04, zf+.7), (lx+.7, yt-.04, zf+.7), (lx+.7, yt-.04, zf+.95), (lx-.7, yt-.04, zf+.95)], 'lamp', (0, -1, 0))
        if d == 0:
            for bx in (-2.1, .9):
                g.cube((bx, y0+1.3, zf+1.1), (1.3, .05, .6), 'woodLight'); g.cube((bx, y0+.92, zf+1.38), (1.3, .72, .04), 'woodLight')
                g.cube((bx+.2, y0+1.55, zf+1.25), (.42, .3, .03), 'darkFrame')
            if (k+seed) % 2: g.cube((3.0, y0+1.05, zf+1.6), (.4, .9, .4), 'potGrey'); g.blob((3.0, y0+1.75, zf+1.6), (.32, .4, .32), 'leaf3', n=6, m=3)
        # Hông nhà (thấy khi nhìn dọc trục đường): gờ tầng, cửa sổ lớn có khung, đố đứng/ngang, bậu.
        for side in (-1, 1):
            xs = side*(x1+.012)
            g.cube((side*(x1+.05), y0+.08, 9.0), (.1, .16, zb-zf), 'trim' if style != 1 else frame)
            if d == 2: continue
            for wz in (7.25, 9.25, 11.25):
                g.quad([(xs, y0+.75, wz-.75), (xs, y0+.75, wz+.75), (xs, y0+2.75, wz+.75), (xs, y0+2.75, wz-.75)], 'glassLight' if style == 0 else 'glass', (side, 0, 0))
                fx = side*(x1+.04)
                for zz in (wz-.78, wz+.78): g.cube((fx, y0+1.75, zz), (.07, 2.06, .07), frame if style != 1 else 'white')
                for yy in (y0+.72, y0+2.78): g.cube((fx, yy, wz), (.07, .07, 1.62), frame if style != 1 else 'white')
                if d == 0:
                    g.cube((fx, y0+1.75, wz), (.05, 2.0, .05), frame if style != 1 else 'white')
                    g.cube((fx, y0+2.15, wz), (.05, .05, 1.5), frame if style != 1 else 'white')
                    g.cube((side*(x1+.1), y0+.66, wz), (.14, .05, 1.7), 'trim')
    # Mái: lan can, đỉnh mặt tiền, phòng kỹ thuật, bồn nước, cục lạnh, ăng-ten.
    g.cube((0, h+.05, 9.0), (x1-x0+.1, .1, zb-zf+.1), shell)
    g.cube((0, h+.55, zf+.05), (x1-x0+.1, 1.0, .2), shell)
    g.cube((0, h+1.08, zf-.02), (x1-x0+.2, .1, .3), frame)
    for x in (x0+.05, x1-.05): g.cube((x, h+.5, 9.0), (.12, .9, zb-zf), shell)
    g.cube((0, h+.5, zb-.05), (x1-x0, .9, .12), shell)
    g.cube((-1.2, h+1.25, 10.4), (2.6, 2.4, 2.6), 'stone', skip=('-y',))
    if d < 2:
        g.cube((-1.2, h+.9, 9.08), (.9, 1.7, .04), 'ironGrey', skip=('+z',))
        g.cyl((1.8, h+.1, 10.6), (1.8, h+1.5, 10.6), .55, 'chrome', n=10 if d == 0 else 6)
        for ax in (1.0, 2.4): g.cube((ax, h+.45, 7.4), (.9, .7, .6), 'white')
        g.cyl((-1.8, h+2.45, 10.4), (-1.8, h+5.0, 10.4), .03, 'steel', n=4)
        if d == 0:
            g.cyl((-1.8, h+3.6, 10.4), (-1.3, h+3.6, 10.4), .015, 'steel', n=3)
            for ax in (1.0, 2.4): g.disc((ax-.1, h+.45, 7.095), .22, 'ironGrey', n=8)

def shop(i, detail):
    spec = SHOPS[i]; g = Geo(detail); obstacles = []; r = random.Random(500+i)
    h = shop_height(spec); fac = spec['facade']
    # Khối nhà: tường trước tách riêng để mở lòng cửa hàng tầng trệt.
    g.cube((0, (h+GROUND-1.7)/2, 0), (WIDTH, h-GROUND+1.7, 7.8), fac, skip=('-y',))
    g.cube((0, 1.7, 1.7), (WIDTH, 3.4, 4.4), fac, skip=('-z', '-y'))
    for x in (-HX+.01, HX-.01): g.cube((x, 1.7, WALL+DEPTH/2), (.02, 3.4, DEPTH), fac)
    g.cube((0, 3.42, WALL+.05), (WIDTH, .55, .1), fac)
    storefront(g, spec, obstacles); awning(g, spec, obstacles); sign(g, spec, obstacles)
    for k in range(1, spec['floors']+1): upper_floor(g, spec, k, k == spec['floors'], obstacles, r)
    roof(g, spec, obstacles, r)
    # Cột nhà/biển nhỏ số nhà, cờ, biển dọc, chậu cây và thùng rác vỉa hè.
    g.cube((-3.55, 2.55, WALL-.06), (.42, .3, .03), 'navy'); g.text(str(18+i*7), (-3.55, 2.55, WALL-.08), .17, 'white', .36)
    if i % 3 == 0: flag(g, -3.6, GROUND+1.0)
    if i in (4, 7, 10): blade_sign(g, {4: 'NHA KHOA', 7: 'NHÀ NGHỈ', 10: 'GIẶT SẤY'}[i], {4: 'blue', 7: 'red', 10: 'awnBlue'}[i], 3.55, GROUND+2.2)
    if spec['kind'] not in ('grocery', 'repair'): planter(g, -3.5, -5.6, obstacles, 1.0, ('white', 'potGrey')[i % 2])
    if i % 4 == 1: planter(g, 3.45, -5.6, obstacles, 1.15, 'potGrey')
    if i % 3 == 2: trash_bin(g, 3.55 if i % 4 != 1 else -1.6, -5.65, obstacles)
    if i % 3 == 1: back_tower(g, i)
    obstacles.insert(0, ('Nhà cửa', (0, h/2, 0), (WIDTH, h, 7.8)))
    g.meta = dict(height=h, display=spec['name'], kind=spec['kind'], floors=spec['floors']+1,
                  obstacles=[dict(kind=k, center=list(c), size=list(s)) for k, c, s in obstacles])
    return g

# ---------------------------------------------------------------- tree / pavement
def tree(seed, detail):
    """Cây theo ảnh mẫu ArtSource/Reference/Street/tree_reference.webp (24/10/2026):
    thân xám mảnh thẳng ~40% chiều cao, cành xám chia lên, tán tròn không đều gồm nhiều cụm lá
    bầu dục to bản, có khe hở giữa các cụm; xanh tươi ngả vàng, sáng ở ngọn/ngoài, đậm ở trong/dưới."""
    g = Geo(detail); r = random.Random(900+seed)
    g.cube((0, .16, 0), (1.45, .32, 1.45), 'grout'); g.cube((0, .33, 0), (1.2, .04, 1.2), 'soil', skip=('-y',))
    for x in (-.66, .66): g.cube((x, .37, 0), (.12, .1, 1.45), 'ironGrey')
    for z in (-.66, .66): g.cube((0, .37, z), (1.2, .1, .12), 'ironGrey')
    n = (10, 7, 5)[detail]
    lean = Vector((.04+seed*.02, 0, .03))
    top = Vector((0, 3.7+seed*.15, 0))+lean*3.4
    g.cyl((0, .3, 0), tuple(top), .1, 'trunk', n=n, r2=.065)
    # Tán: tâm, bán kính (hơi rộng hơn cao như ảnh mẫu).
    cc = Vector((0, top.y+1.3, 0))+lean*1.5; rx, ry, rz = 1.68, 1.6, 1.8
    # Cành chính tỏa lên từ đỉnh thân và một điểm chẻ thấp hơn.
    primaries = []
    for k in range(5):
        a = k*2*pi/5+seed*.7+r.uniform(-.25, .25)
        start = Vector((0, top.y-(.55 if k % 2 else 0), 0))+lean*(top.y-(.55 if k % 2 else 0))
        node = cc+Vector((cos(a)*rx*.45, -ry*.15+r.uniform(-.1, .35), sin(a)*rz*.45))
        g.cyl(tuple(start), tuple(node), .055, 'trunk', n=max(5, n-3), r2=.032); primaries.append(node)
    # Cụm lá ở đầu các cành con, phân bố trên vỏ tán gồ ghề.
    clusters = []
    count = (40, 30, 20)[detail]
    for k in range(count):
        u = r.uniform(-.72, 1); t = r.uniform(0, 2*pi); s = math.sqrt(max(0, 1-u*u))
        dirv = Vector((s*cos(t), u, s*sin(t)))
        rad = r.uniform(.6, 1.05)
        c = cc+Vector((dirv.x*rx*rad, dirv.y*ry*rad, dirv.z*rz*rad))
        clusters.append((c, dirv, r.uniform(.36, .55)))
    if detail < 2:
        for c, dirv, cr in clusters:
            node = min(primaries, key=lambda p: (p-c).length)
            mid = node.lerp(c, .55)+Vector((0, .12, 0))
            g.cyl(tuple(node), tuple(mid), .028, 'trunk', n=4, r2=.02, caps=False)
            if detail == 0: g.cyl(tuple(mid), tuple(c), .018, 'trunk', n=4, r2=.01, caps=False)
    per = (88, 50, 33)[detail]; scale = (1.0, 1.3, 1.6)[detail]; total = 0
    for ci, (c, dirv, cr) in enumerate(clusters):
        out_c = (c-cc).normalized()
        for k in range(per):
            # Lá xếp thành chùm quanh tâm cụm, mặt lá quay ra ngoài và hơi lên trên.
            v = Vector((r.uniform(-1, 1), r.uniform(-.6, 1), r.uniform(-1, 1)))
            if v.length < .05: v = Vector((0, 1, 0))
            v.normalize(); v = (v+out_c*.6).normalized()
            p = c+v*cr*r.uniform(.25, 1.0)
            axis = (v+Vector((r.uniform(-.4, .4), -.35, r.uniform(-.4, .4)))).normalized()
            face = (v+Vector((0, .5, 0))).normalized()
            # Màu: ngọn/ngoài sáng vàng-xanh, trong/dưới đậm.
            light = (p.y-cc.y)/ry*.5+(p-cc).length/max(rx, rz)*.6+r.uniform(-.25, .25)+.18
            col = 'leaf4' if light > .95 else 'leaf3' if light > .6 else 'leaf2' if light > .3 else 'leaf1' if light > 0 else 'leaf5'
            ln = (.21+r.random()*.09)*scale
            g.leaf(tuple(p), tuple(axis), ln, ln*.5, col, roll=r.uniform(-1, 1), face=tuple(face), wpos=.48); total += 1
    g.leaf_count = total
    g.meta = dict(obstacles=[dict(kind='Thân cây', center=[.03, 2.0, 0], size=[.42, 4.0, .42]), dict(kind='Bồn gốc cây', center=[0, .25, 0], size=[1.45, .5, 1.45])],
                  reference='ArtSource/Reference/Street/tree_reference.webp')
    return g

def pavement(detail):
    g = Geo(detail)
    g.cube((0, .1, 0), (4.2, .2, 8.4), 'grout', skip=('-y',))
    if detail < 2:
        # Gạch caro kem–đất nung 0,6 m giống ảnh mẫu, nổi nhẹ trên mạch vữa.
        for xi in range(7):
            for zi in range(14):
                x = -1.8+xi*.6; z = -3.9+zi*.6; col = 'paveCream' if (xi+zi) % 2 == 0 else 'paveBrown'
                if detail == 0: g.cube((x, .213, z), (.56, .03, .56), col, skip=('-y',))
                else: g.quad([(x-.28, .215, z-.28), (x+.28, .215, z-.28), (x+.28, .215, z+.28), (x-.28, .215, z+.28)], col, (0, 1, 0))
    else:
        g.quad([(-2.1, .215, -4.2), (2.1, .215, -4.2), (2.1, .215, 4.2), (-2.1, .215, 4.2)], 'paveBrown', (0, 1, 0))
    for zi in range(12):
        g.cube((-2.0, .17, -3.85+zi*.7), (.22, .36, .68), 'curb' if zi % 2 == 0 else 'stone')
    if detail < 2:
        for z in (-2.9, 2.9):
            g.cube((-1.7, .22, z), (.4, .02, .7), 'iron')
            for j in range(5): g.cube((-1.7, .235, z-.24+j*.12), (.34, .01, .04), 'ironGrey', skip=('-y',))
    g.meta = dict(obstacles=[])
    return g

# ---------------------------------------------------------------- street lamp / traffic signal
# Cột đèn đường cong và cột đèn tín hiệu cần vươn như ảnh mẫu (24/10/2026, yêu cầu thứ hai).
# Đèn đường thay PV_LEDStreetLamp.fbx (giữ 3 slot vật liệu metal/silver/lens của hệ ngày–đêm);
# cột tín hiệu là PV_TrafficSignal.fbx dùng atlas phố. Bóng đèn tín hiệu runtime vẫn là "Tín hiệu 0..2".
LAMP_REACH = 2.35; LAMP_HEIGHT = 8.48   # tâm quang học trong hệ "Đèn LED • ±1" (x hướng ra đường)
SIGNAL_REACH = 3.0; SIGNAL_HEAD_Y = 6.82 # tâm "Hộp đèn" so với gốc cột tín hiệu
LAMP_SLOT = {'steel': 0, 'chrome': 1, 'white': 2}

def street_lamp(detail):
    g = Geo(detail); n = (14, 9, 6)[detail]
    g.cube((0, .16, 0), (.38, .32, .38), 'steel'); g.cube((0, .36, 0), (.26, .1, .26), 'chrome')
    g.cyl((0, .3, 0), (0, 7.55, 0), .11, 'steel', n=n, r2=.07)
    for y in (2.6, 5.2):
        if detail < 2: g.cyl((0, y, 0), (0, y+.08, 0), .1, 'chrome', n=n)
    # Cần cong kiểu cổ ngỗng: cung 1,05 m rồi vươn gần ngang tới bầu đèn.
    pts = []; seg = (10, 6, 4)[detail]
    for k in range(seg+1):
        t = (pi/2)*k/seg; pts.append((1.05-1.05*cos(t), 7.55+1.05*sin(t), 0))
    pts += [(1.75, 8.64, 0), (LAMP_REACH-.35, 8.62, 0)]
    for a, b in zip(pts, pts[1:]): g.cyl(a, b, .062, 'steel', n=max(6, n-4))
    # Bầu đèn dẹt thuôn, mặt kính trắng phía dưới.
    hx = LAMP_REACH+.12
    g.blob((hx, 8.62, 0), (.62, .12, .24), 'steel', n=(12, 8, 6)[detail], m=(5, 4, 3)[detail])
    g.blob((hx+.05, 8.6, 0), (.5, .04, .19), 'chrome', n=(12, 8, 6)[detail], m=3)
    g.disc((LAMP_REACH, LAMP_HEIGHT+.03, 0), .17, 'white', facing=(0, -1, 0), n=(12, 8, 6)[detail])
    g.cube((LAMP_REACH, LAMP_HEIGHT+.06, 0), (.62, .02, .26), 'white', skip=('+y',))
    g.meta = dict(obstacles=[dict(kind='Cột đèn đường', center=[0, 3.8, 0], size=[.25, 7.6, .25])],
                  optical_center=[LAMP_REACH, LAMP_HEIGHT, 0])
    return g

def traffic_signal(detail):
    g = Geo(detail); n = (12, 8, 6)[detail]
    g.cube((0, .18, 0), (.46, .36, .46), 'steel')
    g.cyl((0, .3, 0), (0, 7.75, 0), .12, 'steel', n=n, r2=.095)
    g.cyl((0, 7.75, 0), (0, 7.9, 0), .1, 'chrome', n=n)
    # Cần vươn ra lòng đường có thanh giằng.
    g.cyl((0, 7.6, 0), (SIGNAL_REACH+.35, 7.6, 0), .075, 'steel', n=n, r2=.055)
    if detail < 2: g.cyl((0, 6.75, 0), (1.35, 7.58, 0), .04, 'steel', n=6)
    g.cyl((SIGNAL_REACH, 7.58, 0), (SIGNAL_REACH, SIGNAL_HEAD_Y+.72, 0), .03, 'iron', n=6)
    # Hộp đèn treo đối xứng trước/sau; mặt trước chừa chỗ ba bóng runtime ở z = -0,23.
    hx, hy = SIGNAL_REACH, SIGNAL_HEAD_Y
    g.cube((hx, hy, 0), (.52, 1.42, .34), 'darkFrame')
    g.cube((hx, hy, 0), (.64, 1.54, .06), 'iron')   # tấm viền sau hộp
    for k in range(3):
        y = hy+.43-k*.43
        for side in (-1, 1):
            if detail < 2: g.cube((hx, y+.19, side*.27), (.4, .035, .2), 'iron')
            if side > 0: g.disc((hx, y, .172), .15, 'ironGrey', facing=(0, 0, 1), n=(10, 8, 6)[detail])
    # Hộp phụ trên thân cột và hộp điện.
    g.cube((.0, 3.55, -.2), (.36, 1.0, .26), 'darkFrame')
    for k in range(3):
        if detail < 2: g.disc((0, 3.85-k*.3, -.335), .1, 'ironGrey', facing=(0, 0, -1), n=8)
    g.cube((-.2, 1.25, 0), (.18, .5, .32), 'steel')
    g.meta = dict(obstacles=[dict(kind='Cột đèn tín hiệu', center=[0, 3.9, 0], size=[.3, 7.8, .3])],
                  head_center=[SIGNAL_REACH, SIGNAL_HEAD_Y, 0])
    return g

def lamp_mesh(name, g, coll, mats):
    ob = to_mesh(name, g, coll); me = ob.data; me.materials.clear()
    for m in mats: me.materials.append(m)
    for poly, col in zip(me.polygons, g.cols): poly.material_index = LAMP_SLOT.get(NAMES[col], 0)
    return ob

def lamp_materials():
    out = []
    for name, hx, rough, em in (('PV_LampMetal', '8C959C', .4, 0), ('PV_LampSilver', 'C9D0D5', .3, 0), ('PV_LampLens', 'F4FAFF', .18, 2)):
        m = bpy.data.materials.get(name) or bpy.data.materials.new(name); m.use_nodes = True
        b = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED'); b.inputs['Base Color'].default_value = hexcol(hx); b.inputs['Roughness'].default_value = rough
        out.append(m)
    return out

def export_lod_hierarchy(path, objs):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y', global_scale=1,
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False, mesh_smooth_type='FACE')

def build_saigon_fixtures(clear=True):
    """Xuất PV_LEDStreetLamp.fbx (Art/Lighting) và PV_TrafficSignal.fbx (Art/Streets)."""
    global MAT
    if clear: sc, coll = owned_collection(); MAT = palette_material()
    else: coll = bpy.data.collections[SCENE]
    results = {}
    mats = lamp_materials()
    # Đèn đường: gốc là LOD0, hai LOD con như FBX cũ để giữ fileID/prefab instance.
    lamps = [lamp_mesh('PV_LEDStreetLamp' if d == 0 else 'PV_LEDStreetLamp_LOD'+str(d), street_lamp(d), coll, mats) for d in range(3)]
    for o in lamps[1:]: o.parent = lamps[0]
    export_lod_hierarchy(ROOT+'/Assets/PhoViet/Art/Lighting/PV_LEDStreetLamp.fbx', lamps)
    sig = [to_mesh('PV_TrafficSignal' if d == 0 else 'PV_TrafficSignal_LOD'+str(d), traffic_signal(d), coll) for d in range(3)]
    for o in sig[1:]: o.parent = sig[0]
    export_lod_hierarchy(OUT+'/PV_TrafficSignal.fbx', sig)
    for name, objs, g in (('PV_LEDStreetLamp', lamps, street_lamp(0)), ('PV_TrafficSignal', sig, traffic_signal(0))):
        counts = []
        for o in objs: o.data.calc_loop_triangles(); counts.append(len(o.data.loop_triangles))
        info = dict(name=name, generator='pho_viet_saigon_streets.py', lod_triangles=counts); info.update(g.meta); results[name] = info
    lamps[0].location = (0, -24, 0); sig[0].location = (6, -24, 0)
    for o in lamps[1:]+sig[1:]: o.hide_set(True); o.hide_render = True
    with open(ROOT+'/Assets/PhoViet/Art/Lighting/PV_LEDStreetLamp.json', 'w', encoding='utf8') as f: json.dump(results['PV_LEDStreetLamp'], f, ensure_ascii=False, indent=2)
    with open(OUT+'/PV_TrafficSignal.json', 'w', encoding='utf8') as f: json.dump(results['PV_TrafficSignal'], f, ensure_ascii=False, indent=2)
    return results

# ---------------------------------------------------------------- Blender output
MAT = None
GLASS = None
def palette_material():
    pixels = []
    for y in range(128):
        for x in range(128): pixels.extend(hexcol(PAL[NAMES[(y//16)*8+x//16]]))
    im = bpy.data.images.get('PV_StreetPalette') or bpy.data.images.new('PV_StreetPalette', 128, 128)
    if im.size[0] != 128: im.scale(128, 128)
    im.pixels = pixels; im.filepath_raw = OUT+'/PV_StreetPalette.png'; im.file_format = 'PNG'; im.save()
    mat = bpy.data.materials.get('PV_StreetAtlas') or bpy.data.materials.new('PV_StreetAtlas'); mat.use_nodes = True
    nt = mat.node_tree; bsdf = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED'); bsdf.inputs['Roughness'].default_value = .78
    tex = next((n for n in nt.nodes if n.type == 'TEX_IMAGE'), None) or nt.nodes.new('ShaderNodeTexImage')
    tex.image = im; tex.interpolation = 'Closest'; nt.links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
    # Slot 2 của shop: kính trong suốt (Unity gán PV_StreetGlass.mat).
    global GLASS
    GLASS = bpy.data.materials.get('PV_StreetGlass') or bpy.data.materials.new('PV_StreetGlass'); GLASS.use_nodes = True
    gb = next(n for n in GLASS.node_tree.nodes if n.type == 'BSDF_PRINCIPLED'); gb.inputs['Base Color'].default_value = hexcol('BFDDEA'); gb.inputs['Alpha'].default_value = .25
    return mat

def to_mesh(name, g, coll):
    me = bpy.data.meshes.new(name); me.from_pydata([B(p) for p in g.v], [], g.f); me.update(); me.materials.append(MAT)
    uv = me.uv_layers.new(name='StreetAtlasUV')
    flips = []
    for poly, col, hint in zip(me.polygons, g.cols, g.hint):
        poly.use_smooth = False
        for loop in poly.loop_indices: uv.data[loop].uv = uv_of(col)
        if hint[0] == 'c': want = Vector(poly.center)-B(hint[1])
        else: want = B(hint[1])
        flips.append(want.length > 1e-6 and poly.normal.dot(want) < 0)
    bm = bmesh.new(); bm.from_mesh(me); bm.faces.ensure_lookup_table()
    bad = [bm.faces[i] for i, f in enumerate(flips) if f]
    if bad: bmesh.ops.reverse_faces(bm, faces=bad, flip_multires=False)
    bm.to_mesh(me); bm.free(); me.update()
    if any(t == 'Glass' for t in g.tags):
        me.materials.append(GLASS)
        for poly, t in zip(me.polygons, g.tags): poly.material_index = 1 if t == 'Glass' else 0
    ob = bpy.data.objects.new(name, me); coll.objects.link(ob); return ob

def owned_collection():
    sc = bpy.data.scenes.get(SCENE) or bpy.data.scenes.new(SCENE)
    bpy.context.window.scene = sc
    coll = bpy.data.collections.get(SCENE) or bpy.data.collections.new(SCENE)
    if coll.name not in sc.collection.children: sc.collection.children.link(coll)
    for o in list(coll.objects):
        data = o.data; bpy.data.objects.remove(o, do_unlink=True)
        if isinstance(data, bpy.types.Mesh) and data.users == 0: bpy.data.meshes.remove(data)
        elif isinstance(data, bpy.types.Camera) and data.users == 0: bpy.data.cameras.remove(data)
    return sc, coll

def save_asset(name, geos, coll, offset):
    root = bpy.data.objects.new(name, None); coll.objects.link(root); objs = []
    for level, g in enumerate(geos):
        ob = to_mesh(name+'_LOD'+str(level), g, coll); ob.parent = root; objs.append(ob)
    bpy.ops.object.select_all(action='DESELECT'); root.select_set(True)
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.export_scene.fbx(filepath=OUT+'/'+name+'.fbx', use_selection=True, object_types={'EMPTY', 'MESH'}, axis_forward='-Z', axis_up='Y',
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, add_leaf_bones=False, use_mesh_modifiers=True, mesh_smooth_type='FACE')
    counts = []
    for o in objs: o.data.calc_loop_triangles(); counts.append(len(o.data.loop_triangles))
    meta = geos[0].meta
    info = dict(name=name, generator='pho_viet_saigon_streets.py', reference='ArtSource/Reference/Street/saigon_street_reference.webp',
        lod_triangles=counts, materials=1, palette_grid=[8, 8],
        sign_front_z=-4.118 if 'Shop' in name else None, wall_front_z=WALL if 'Shop' in name else None,
        protected_awning_strips=14 if 'Shop' in name else 0, leaf_counts=[getattr(g, 'leaf_count', 0) for g in geos] if 'Tree' in name else [])
    info.update(meta)
    with open(OUT+'/'+name+'.json', 'w', encoding='utf8') as f: json.dump(info, f, ensure_ascii=False, indent=2)
    root.location = offset
    for o in objs[1:]: o.hide_set(True); o.hide_render = True
    return info

def build_saigon_streets(only=None):
    global MAT
    sc, coll = owned_collection(); MAT = palette_material(); results = []
    names = []
    for i in range(12): names.append(('PV_StreetShop'+str(i).zfill(2), lambda d, i=i: shop(i, d), ((i % 6)*9.5, (i//6)*14, 0)))
    for i in range(3): names.append(('PV_StreetTree'+str(i), lambda d, i=i: tree(i, d), (i*6, -16, 0)))
    names.append(('PV_StreetPavement', lambda d: pavement(d), (22, -16, 0)))
    for name, make, offset in names:
        if only and name not in only: continue
        results.append(save_asset(name, [make(d) for d in range(3)], coll, offset))
    if not only:
        with open(OUT+'/catalogue.json', 'w', encoding='utf8') as f: json.dump(results, f, ensure_ascii=False, indent=2)
        fixtures = build_saigon_fixtures(clear=False)
        bpy.ops.wm.save_as_mainfile(filepath=SOURCE)
        results = results+list(fixtures.values())
    return dict(success=True, assets=len(results), lod0=sum(r['lod_triangles'][0] for r in results),
                results=[(r['name'], r['lod_triangles']) for r in results])
