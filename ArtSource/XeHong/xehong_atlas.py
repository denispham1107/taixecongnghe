# 21.324 — atlas vết hỏng xe 1024x1024 (RGBA): [trên-trái] kính nứt, [trên-phải] đèn vỡ, [dưới-trái] xước A, [dưới-phải] xước B.
# Unity: v=0 ở đáy ảnh → hàng dưới = v 0..0.5. Viền trong suốt 16 px mỗi ô.
import numpy as np, math, random, sys
from PIL import Image, ImageDraw, ImageFilter
Q = 512; SS = 2
out = sys.argv[1]

def vnoise(shape, scale, seed):
    rng = np.random.default_rng(seed)
    h, w = shape; gh, gw = int(h/scale)+2, int(w/scale)+2
    g = rng.random((gh, gw))
    y = np.arange(h)/scale; x = np.arange(w)/scale
    yi = y.astype(int); xi = x.astype(int); fy = y-yi; fx = x-xi
    fy = fy*fy*(3-2*fy); fx = fx*fx*(3-2*fx)
    a = g[yi][:, xi]; b = g[yi][:, xi+1]; c = g[yi+1][:, xi]; d = g[yi+1][:, xi+1]
    fx = fx[None, :]; fy = fy[:, None]
    return (a*(1-fx)+b*fx)*(1-fy)+(c*(1-fx)+d*fx)*fy

def fbm(shape, scale, seed, oct=4):
    s = 0; amp = 1; tot = 0
    for o in range(oct):
        s = s + vnoise(shape, scale/(2**o), seed+o*17)*amp; tot += amp; amp *= .5
    return s/tot

def smooth(e0, e1, x):
    t = np.clip((x-e0)/(e1-e0), 0, 1); return t*t*(3-2*t)

def scratch(seed, elong):
    rng = random.Random(seed)
    yy, xx = np.mgrid[0:Q, 0:Q].astype(float)
    cx = cy = Q/2
    dx = (xx-cx)/(Q*0.40*elong); dy = (yy-cy)/(Q*0.40)
    r = np.sqrt(dx*dx+dy*dy)
    n1 = fbm((Q, Q), 70, seed); n2 = fbm((Q, Q), 22, seed+5); n3 = fbm((Q, Q), 8, seed+9)
    edge = r + (n1-0.5)*0.55 + (n2-0.5)*0.18
    rgb = np.zeros((Q, Q, 3))
    soot = (1-smooth(0.55, 1.0, edge)) * (0.35+0.4*n2)
    rgb[:] = np.array([.10, .095, .09]); a = soot*0.55
    primer = 1-smooth(0.58, 0.66, edge + (n3-0.5)*0.12)
    pc = np.array([.47, .48, .47])[None, None, :]*(0.85+0.3*n2[..., None])
    rgb = rgb*(1-primer[..., None]) + pc*primer[..., None]; a = np.maximum(a, primer*0.93)
    core = 1-smooth(0.36, 0.46, edge + (n3-0.5)*0.16)
    streak = 0.5+0.5*np.sin(yy*0.9 + n1*30 + n3*6)
    mc = np.array([.62, .64, .67])[None, None, :]*(0.72+0.38*streak[..., None])*(0.85+0.3*n2[..., None])
    rgb = rgb*(1-core[..., None]) + mc*core[..., None]; a = np.maximum(a, core*0.97)
    crease = np.exp(-((n1-0.5)*40)**2) * core
    rgb = rgb*(1-0.65*crease[..., None])
    img = Image.fromarray(np.dstack([np.clip(rgb, 0, 1)*255, np.clip(a, 0, 1)*255]).astype(np.uint8), 'RGBA')
    big = img.resize((Q*SS, Q*SS), Image.BICUBIC); d = ImageDraw.Draw(big)
    for i in range(70):
        ang = rng.gauss(0, 0.12); L = rng.uniform(40, 220)*SS*elong
        x0 = cx*SS + rng.gauss(0, 70*SS*elong); y0 = cy*SS + rng.gauss(0, 60*SS)
        if math.hypot((x0-cx*SS)/(Q*0.46*SS*elong), (y0-cy*SS)/(Q*0.46*SS)) > 0.95: continue
        pts = []; x, y = x0, y0
        for k in range(8):
            pts.append((x, y)); x += math.cos(ang)*L/8; y += math.sin(ang)*L/8 + rng.uniform(-2, 2)*SS
        light = rng.random() < 0.7
        col = (int(205+rng.uniform(-20, 30)), int(208+rng.uniform(-20, 30)), int(212+rng.uniform(-20, 30)), int(rng.uniform(120, 210))) if light else (25, 24, 22, int(rng.uniform(90, 160)))
        d.line(pts, fill=col, width=max(1, int(rng.uniform(1, 3.2)*SS)))
    img = big.resize((Q, Q), Image.LANCZOS)
    rx = min(0.47, 0.40*elong*1.12)*Q; ry = 0.44*Q
    m = Image.new('L', (Q, Q), 0); ImageDraw.Draw(m).ellipse([Q/2-rx, Q/2-ry, Q/2+rx, Q/2+ry], fill=255)
    m = m.filter(ImageFilter.GaussianBlur(14))
    img.putalpha(Image.fromarray((np.asarray(img.getchannel('A'), float)*np.asarray(m, float)/255).astype(np.uint8)))
    return img

def crack(seed):
    rng = random.Random(seed)
    C = Q*SS/2
    fro = Image.new('L', (Q*SS, Q*SS), 0); fd = ImageDraw.Draw(fro)
    for rr in range(130*SS, 0, -4*SS): fd.ellipse([C-rr, C-rr, C+rr, C+rr], fill=int(70*(1-rr/(130*SS))**1.4))
    big = Image.merge('RGBA', [Image.new('L', fro.size, 225), Image.new('L', fro.size, 232), Image.new('L', fro.size, 240), fro])
    d = ImageDraw.Draw(big)
    rays = []; nray = 17
    for i in range(nray):
        a0 = i*2*math.pi/nray + rng.uniform(-0.15, 0.15)
        pts = [(C, C)]; r = 0; a = a0
        while r < 236*SS:
            r += rng.uniform(14, 30)*SS; a += rng.uniform(-0.09, 0.09)
            pts.append((C+math.cos(a)*r, C+math.sin(a)*r))
        rays.append(pts)
        for k in range(len(pts)-1):
            fade = 1 - k/len(pts)
            d.line([pts[k], pts[k+1]], fill=(236, 243, 255, int(235*(0.45+0.55*fade))), width=max(2, int((3.2*fade+1.2)*SS)))
        for k in range(2, len(pts)-1):
            if rng.random() < 0.35:
                bx, by = pts[k]; ba = a0 + rng.choice([-1, 1])*rng.uniform(0.4, 0.9); bl = rng.uniform(20, 60)*SS
                d.line([(bx, by), (bx+math.cos(ba)*bl, by+math.sin(ba)*bl)], fill=(230, 238, 255, 170), width=max(1, int(1.4*SS)))
    def at(ray, R):
        for j in range(1, len(ray)):
            if math.hypot(ray[j][0]-C, ray[j][1]-C) >= R*SS: return ray[j]
        return ray[-1]
    for rr in (34, 72, 120, 178):
        for i in range(nray):
            if rng.random() < 0.72:
                a1, a2 = at(rays[i], rr+rng.uniform(-6, 6)), at(rays[(i+1) % nray], rr+rng.uniform(-6, 6))
                mid = ((a1[0]+a2[0])/2 + rng.uniform(-6, 6)*SS, (a1[1]+a2[1])/2 + rng.uniform(-6, 6)*SS)
                d.line([a1, mid, a2], fill=(232, 240, 255, int(200 - rr*0.5)), width=max(1, int(2.0*SS)))
    for k in range(90):
        rr = abs(rng.gauss(0, 13))*SS; a = rng.uniform(0, 2*math.pi)
        x, y = C+math.cos(a)*rr, C+math.sin(a)*rr; s = rng.uniform(2, 7)*SS
        d.polygon([(x+rng.uniform(-s, s), y+rng.uniform(-s, s)) for _ in range(3)], fill=(245, 248, 255, int(rng.uniform(150, 240))))
    img = big.resize((Q, Q), Image.LANCZOS)
    m = Image.new('L', (Q, Q), 0); ImageDraw.Draw(m).ellipse([12, 12, Q-12, Q-12], fill=255)
    m = m.filter(ImageFilter.GaussianBlur(10))
    img.putalpha(Image.fromarray((np.asarray(img.getchannel('A'), float)*np.asarray(m, float)/255).astype(np.uint8)))
    return img

def lamp(seed):
    rng = random.Random(seed)
    big = Image.new('RGBA', (Q*SS, Q*SS), (0, 0, 0, 0)); d = ImageDraw.Draw(big)
    C = Q*SS/2; RX, RY = 225*SS, 150*SS
    pts = []
    for i in range(64):
        a = i*2*math.pi/64; k = rng.uniform(0.86, 1.0)
        pts.append((C+math.cos(a)*RX*k, C+math.sin(a)*RY*k))
    d.polygon(pts, fill=(14, 14, 16, 238))
    for k in range(26):
        a = rng.uniform(0, 2*math.pi); rr = rng.uniform(0.35, 0.92)
        x, y = C+math.cos(a)*RX*rr, C+math.sin(a)*RY*rr; s = rng.uniform(14, 40)*SS
        tone = int(rng.uniform(40, 110))
        d.polygon([(x+rng.uniform(-s, s), y+rng.uniform(-s*0.7, s*0.7)) for _ in range(rng.choice([3, 4]))], fill=(tone, tone, tone+8, int(rng.uniform(150, 230))))
    for k in range(22):
        a = rng.uniform(0, 2*math.pi); x0, y0 = C+math.cos(a)*RX*rng.uniform(0, .3), C+math.sin(a)*RY*rng.uniform(0, .3)
        L = rng.uniform(0.5, 1.0); seg = [(x0, y0)]
        for j in range(5): seg.append((seg[-1][0]+math.cos(a)*RX*L/5+rng.uniform(-6, 6)*SS, seg[-1][1]+math.sin(a)*RY*L/5+rng.uniform(-6, 6)*SS))
        d.line(seg, fill=(210, 214, 222, int(rng.uniform(120, 200))), width=max(1, int(rng.uniform(1.2, 2.6)*SS)))
    d.ellipse([C-40*SS, C-30*SS, C+40*SS, C+30*SS], fill=(6, 6, 7, 245))
    d.ellipse([C-18*SS, C-22*SS, C-2*SS, C-12*SS], fill=(150, 150, 160, 160))
    m = Image.new('L', big.size, 0); ImageDraw.Draw(m).polygon(pts, fill=255)
    big.putalpha(Image.fromarray(np.minimum(np.asarray(big.getchannel('A')), np.asarray(m)).astype(np.uint8)))
    return big.resize((Q, Q), Image.LANCZOS)

atlas = Image.new('RGBA', (2*Q, 2*Q), (0, 0, 0, 0))
atlas.paste(crack(11), (0, 0)); atlas.paste(lamp(23), (Q, 0))
atlas.paste(scratch(37, 1.0), (0, Q)); atlas.paste(scratch(59, 1.35), (Q, Q))
A = np.asarray(atlas).copy()
for ox in (0, Q):
    for oy in (0, Q):
        A[oy:oy+16, ox:ox+Q, 3] = 0; A[oy+Q-16:oy+Q, ox:ox+Q, 3] = 0
        A[oy:oy+Q, ox:ox+16, 3] = 0; A[oy:oy+Q, ox+Q-16:ox+Q, 3] = 0
Image.fromarray(A, 'RGBA').save(out)
bg = Image.new('RGBA', atlas.size, (40, 90, 200, 255)); prev = Image.alpha_composite(bg, Image.fromarray(A, 'RGBA'))
prev.convert('RGB').save(sys.argv[2], quality=88)
print('ok', out)
