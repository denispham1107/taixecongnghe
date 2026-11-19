"""21.266 — vẽ vô lăng + 5 nút cảm ứng (Ga, Lùi, Stop, Nhảy, Tên lửa) cho bản đồ mở.

Phong cách theo bộ icon «B Arcade» của game (21.154, PV_FlightUp/PV_MissileHud): nét viền xanh đen đậm, màu tươi,
khối phẳng có bóng nhẹ. Vẽ ở 4× rồi thu nhỏ để khử răng cưa. Ảnh có kênh alpha; độ trong suốt khi hiển thị do Unity
quyết định (OpenWorldDriver: vô lăng 78 % khi để yên, 95 % khi đang giữ; nút 88 %).

Chạy:  python ArtSource/UI/build_touch_controls.py
Ra:    Assets/PhoViet/Resources/TouchControls/PV_Btn_{Left,Right,Gas,Reverse,Stop,Jump,Missile}.png (320²)
       (21.270: bỏ PV_Wheel.png — vô lăng thay bằng 2 nút mũi tên)
"""
import math
import os
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'PhoViet', 'Resources', 'TouchControls')
FONT = os.path.join(ROOT, 'Assets', 'PhoViet', 'Fonts', 'BeVietnamPro-SemiBold.ttf')
OUTLINE = (16, 38, 58, 255)
CREAM = (255, 244, 214, 255)


def hexc(h, a=255):
    h = h.lstrip('#'); return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(len(a)))


def vgrad(size, top, bottom):
    w, h = size
    t = np.linspace(0.0, 1.0, h, dtype=np.float32)[:, None, None]
    arr = np.array(top, np.float32)[None, None, :] * (1 - t) + np.array(bottom, np.float32)[None, None, :] * t
    return Image.fromarray(np.repeat(arr, w, axis=1).round().astype(np.uint8), 'RGBA')


def rgrad(size, center, inner, outer, radius):
    w, h = size; cx, cy = center
    yy, xx = np.mgrid[0:h, 0:w].astype(np.float32)
    t = np.clip(np.hypot(xx - cx, yy - cy) / radius, 0, 1)[..., None]
    arr = np.array(inner, np.float32) * (1 - t) + np.array(outer, np.float32) * t
    return Image.fromarray(arr.round().astype(np.uint8), 'RGBA')


def mask_rrect(size, box, radius):
    m = Image.new('L', size, 0); ImageDraw.Draw(m).rounded_rectangle(box, radius=radius, fill=255); return m


def mask_ellipse(size, box):
    m = Image.new('L', size, 0); ImageDraw.Draw(m).ellipse(box, fill=255); return m


def paste_masked(base, layer, mask):
    tmp = Image.new('RGBA', base.size, (0, 0, 0, 0)); tmp.paste(layer, (0, 0), mask)
    base.alpha_composite(tmp)


def shadow(base, mask, offset, blur, alpha):
    sh = Image.new('RGBA', base.size, (0, 0, 0, 0))
    m = Image.new('L', base.size, 0); m.paste(mask, offset)
    m = m.filter(ImageFilter.GaussianBlur(blur)).point(lambda v: int(v * alpha))
    sh.putalpha(m); base.alpha_composite(sh)


def rot(points, deg, c):
    a = math.radians(deg); ca, sa = math.cos(a), math.sin(a); cx, cy = c
    return [(cx + (x - cx) * ca - (y - cy) * sa, cy + (x - cx) * sa + (y - cy) * ca) for x, y in points]


def poly(d, pts, fill, w):
    d.polygon(pts, fill=fill)
    d.line(pts + [pts[0]], fill=OUTLINE, width=w, joint='curve')
    r = w / 2
    for x, y in pts: d.ellipse((x - r, y - r, x + r, y + r), fill=OUTLINE)


def rrect(d, box, radius, fill, w):
    d.rounded_rectangle(box, radius=radius, fill=fill, outline=OUTLINE if w else None, width=w)


def circle(d, c, r, fill, w=0, outline=OUTLINE):
    x, y = c; d.ellipse((x - r, y - r, x + r, y + r), fill=fill, outline=outline if w else None, width=w)


def star(c, r_out, r_in, rot_deg=-90):
    pts = []
    for i in range(10):
        a = math.radians(rot_deg + i * 36); r = r_out if i % 2 == 0 else r_in
        pts.append((c[0] + math.cos(a) * r, c[1] + math.sin(a) * r))
    return pts


def chevron(cx, cy, w, h, thick):
    """Mũi tên «^» dày, đỉnh ở (cx, cy-h/2)."""
    return [(cx - w / 2, cy + h / 2), (cx, cy - h / 2), (cx + w / 2, cy + h / 2),
            (cx + w / 2 - thick * 1.1, cy + h / 2 + thick * .05), (cx, cy - h / 2 + thick * 1.35), (cx - w / 2 + thick * 1.1, cy + h / 2 + thick * .05)]


# ---------------------------------------------------------------------------------------------------- vô lăng
def build_wheel(final=768):
    S = final * 3; c = (S / 2, S / 2); img = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    R, r = S * 0.475, S * 0.365                      # bán kính ngoài / trong của vành
    w = int(S * 0.012)
    ring = Image.new('L', (S, S), 0); dr = ImageDraw.Draw(ring)
    dr.ellipse((c[0] - R, c[1] - R, c[0] + R, c[1] + R), fill=255); dr.ellipse((c[0] - r, c[1] - r, c[0] + r, c[1] + r), fill=0)
    shadow(img, ring, (0, int(S * .012)), S * .012, .55)
    # vành da: chuyển màu dọc + ánh sáng chéo trên-trái
    leather = vgrad((S, S), hexc('#4a525c'), hexc('#121519'))
    hl = rgrad((S, S), (S * .33, S * .25), (255, 255, 255, 70), (255, 255, 255, 0), S * .55)
    leather.alpha_composite(hl)
    paste_masked(img, leather, ring)
    d = ImageDraw.Draw(img)
    # vùng cầm (9 giờ và 3 giờ): da đục lỗ
    for a0, a1 in ((150, 215), (-35, 30)):
        for k in range(int((a1 - a0) / 2.4)):
            a = math.radians(a0 + 1.2 + k * 2.4)
            for j, rr in enumerate((r + (R - r) * .28, r + (R - r) * .5, r + (R - r) * .72)):
                off = math.radians(1.2) if j % 2 else 0
                x, y = c[0] + math.cos(a + off) * rr, c[1] + math.sin(a + off) * rr
                circle(d, (x, y), S * .0035, (8, 9, 11, 200))
    # đường chỉ khâu dọc mép trong và mép ngoài
    for rr in (r + (R - r) * .14, R - (R - r) * .14):
        n = 150
        for k in range(n):
            a0 = 2 * math.pi * k / n; a1 = a0 + 2 * math.pi / n * .55
            d.line([(c[0] + math.cos(a0) * rr, c[1] + math.sin(a0) * rr), (c[0] + math.cos(a1) * rr, c[1] + math.sin(a1) * rr)],
                   fill=(214, 200, 170, 150), width=int(S * .0035))
    # viền vành
    d.ellipse((c[0] - R, c[1] - R, c[0] + R, c[1] + R), outline=OUTLINE, width=w)
    d.ellipse((c[0] - r, c[1] - r, c[0] + r, c[1] + r), outline=OUTLINE, width=w)
    # vạch vàng 12 giờ
    m = [(c[0] - S * .028, c[1] - R + w * .4), (c[0] + S * .028, c[1] - R + w * .4), (c[0] + S * .024, c[1] - r - w * .4), (c[0] - S * .024, c[1] - r - w * .4)]
    poly(d, m, hexc('#FFC93C'), int(w * .8))
    # nan: trái, phải (hơi chếch xuống) và nan dưới đôi
    metal_top, metal_bot = hexc('#e9eef3'), hexc('#7b8692')
    spokes = []
    for side in (-1, 1):
        x0, x1 = c[0] + side * S * .135, c[0] + side * (r + S * .02)
        y = c[1] + S * .03
        pts = [(x0, y - S * .055), (x1, y - S * .042), (x1, y + S * .062), (x0, y + S * .085)]
        spokes.append(pts)
    for side in (-1, 1):
        pts = [(c[0] + side * S * .045, c[1] + S * .11), (c[0] + side * S * .11, c[1] + S * .10),
               (c[0] + side * S * .075, c[1] + r + S * .02), (c[0] + side * S * .018, c[1] + r + S * .02)]
        spokes.append(pts)
    for pts in spokes:
        sm = Image.new('L', (S, S), 0); ImageDraw.Draw(sm).polygon(pts, fill=255)
        paste_masked(img, vgrad((S, S), metal_top, metal_bot), sm)
        d = ImageDraw.Draw(img); d.line(pts + [pts[0]], fill=OUTLINE, width=w, joint='curve')
    # nút điều khiển trên nan ngang
    for side in (-1, 1):
        for k, col in enumerate((hexc('#2a6fb8'), hexc('#d8413a'))):
            bx = c[0] + side * (S * .2 + k * S * .07); by = c[1] + S * .045
            circle(d, (bx, by), S * .026, hexc('#20262e'), int(w * .7))
            circle(d, (bx, by), S * .013, col)
    # ốp còi (túi khí) bo tròn + vòng bạc
    hub = Image.new('L', (S, S), 0); ImageDraw.Draw(hub).rounded_rectangle((c[0] - S * .165, c[1] - S * .13, c[0] + S * .165, c[1] + S * .15), radius=S * .09, fill=255)
    shadow(img, hub, (0, int(S * .008)), S * .01, .5)
    paste_masked(img, vgrad((S, S), hexc('#3a414b'), hexc('#14171b')), hub)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((c[0] - S * .165, c[1] - S * .13, c[0] + S * .165, c[1] + S * .15), radius=S * .09, outline=OUTLINE, width=w)
    d.rounded_rectangle((c[0] - S * .14, c[1] - S * .105, c[0] + S * .14, c[1] + S * .125), radius=S * .075, outline=(190, 198, 208, 160), width=int(S * .005))
    # biểu tượng Phố Việt: tròn đỏ + sao vàng
    em = (c[0], c[1] + S * .01)
    circle(d, em, S * .068, hexc('#E23D2E'), int(w * .8))
    poly(d, star(em, S * .048, S * .019), hexc('#FFD23F'), int(w * .45))
    # bóng sáng trên vành
    gl = Image.new('RGBA', (S, S), (0, 0, 0, 0)); gd = ImageDraw.Draw(gl)
    gd.arc((c[0] - R + w * 2, c[1] - R + w * 2, c[0] + R - w * 2, c[1] + R - w * 2), 200, 300, fill=(255, 255, 255, 60), width=int(S * .012))
    img.alpha_composite(gl.filter(ImageFilter.GaussianBlur(S * .003)))
    return img.resize((final, final), Image.LANCZOS)


# ---------------------------------------------------------------------------------------------------- nút
def plate(S, accent):
    img = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    m = S * .07; box = (m, m, S - m, S - m); rad = S * .2
    pm = mask_rrect((S, S), box, rad)
    shadow(img, pm, (0, int(S * .03)), S * .03, .55)
    paste_masked(img, vgrad((S, S), hexc('#24476a', 235), hexc('#0c1b2c', 235)), pm)
    # ánh màu nhấn dưới đáy
    glow = rgrad((S, S), (S / 2, S * .95), accent[:3] + (110,), accent[:3] + (0,), S * .55)
    paste_masked(img, glow, pm)
    # bóng kính phía trên
    gm = Image.new('L', (S, S), 0); ImageDraw.Draw(gm).rounded_rectangle((m + S * .05, m + S * .04, S - m - S * .05, S * .48), radius=rad * .8, fill=255)
    gm = Image.composite(gm, Image.new('L', (S, S), 0), pm)
    paste_masked(img, vgrad((S, S), (255, 255, 255, 46), (255, 255, 255, 0)), gm)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle(box, radius=rad, outline=OUTLINE, width=int(S * .028))
    i = S * .026
    d.rounded_rectangle((m + i, m + i, S - m - i, S - m - i), radius=rad - i, outline=accent, width=int(S * .03))
    i2 = S * .062
    d.rounded_rectangle((m + i2, m + i2, S - m - i2, S - m - i2), radius=rad - i2, outline=(255, 255, 255, 40), width=max(2, int(S * .008)))
    return img


def label(img, text, accent, size_frac=.15):
    S = img.size[0]; d = ImageDraw.Draw(img)
    f = ImageFont.truetype(FONT, int(S * size_frac))
    bb = d.textbbox((0, 0), text, font=f, stroke_width=int(S * .018))
    tw, th = bb[2] - bb[0], bb[3] - bb[1]
    d.text(((S - tw) / 2 - bb[0], S * .80 - th / 2 - bb[1]), text, font=f, fill=CREAM, stroke_width=int(S * .018), stroke_fill=OUTLINE)


def icon_gas(d, S):
    """Bàn đạp ga nhôm có gân cao su, cần đạp + bản lề dưới sàn, mũi tên tăng tốc."""
    w = int(S * .022); c = (S * .53, S * .39)
    rrect(d, (S * .33, S * .58, S * .74, S * .63), S * .02, hexc('#2b3440'), int(w * .7))
    circle(d, (S * .535, S * .585), S * .03, hexc('#9aa6b2'), int(w * .7))
    arm = rot([(c[0] - S * .025, c[1] + S * .1), (c[0] + S * .025, c[1] + S * .1), (c[0] + S * .02, c[1] + S * .2), (c[0] - S * .02, c[1] + S * .2)], -14, c)
    poly(d, arm, hexc('#6e7a87'), int(w * .7))
    pedal = rot([(c[0] - S * .085, c[1] - S * .2), (c[0] + S * .085, c[1] - S * .2), (c[0] + S * .1, c[1] + S * .13), (c[0] - S * .1, c[1] + S * .13)], -14, c)
    poly(d, pedal, hexc('#D9E0E7'), w)
    inner = rot([(c[0] - S * .062, c[1] - S * .17), (c[0] + S * .062, c[1] - S * .17), (c[0] + S * .075, c[1] + S * .1), (c[0] - S * .075, c[1] + S * .1)], -14, c)
    d.polygon(inner, fill=hexc('#1c232b'))
    for k in range(6):
        y = c[1] - S * .145 + k * S * .044
        seg = rot([(c[0] - S * .058 - k * S * .002, y), (c[0] + S * .058 + k * S * .002, y)], -14, c)
        d.line(seg, fill=hexc('#a8b4c0'), width=int(S * .016))
    poly(d, chevron(S * .27, S * .33, S * .1, S * .065, S * .03), hexc('#FFB259'), int(w * .7))
    poly(d, chevron(S * .27, S * .255, S * .1, S * .065, S * .03), hexc('#FFE08A'), int(w * .7))


def car_side(d, S, cx, cy, scale, angle=0.0, w=None, body_col='#3F9BF2', ys=1.35):
    """Xe thể thao xanh nhìn ngang, đầu xe bên phải (giống siêu xe của người chơi). Tọa độ đơn vị, y âm = lên."""
    w = w or int(S * .02)
    k = S * scale; a = math.radians(angle); ca, sa = math.cos(a), math.sin(a)
    def T(pts): return [(cx + (x * ca - y * ys * sa) * k, cy + (x * sa + y * ys * ca) * k) for x, y in pts]
    def P(pt): return T([pt])[0]
    # bánh (vẽ trước để thân phủ lên phần trên lốp)
    wheels = [P((-0.29, 0.07)), P((0.30, 0.07))]
    for c in wheels: circle(d, c, .115 * k, hexc('#141c25'), w)
    body = [(-0.50, 0.06), (-0.49, -0.05), (-0.42, -0.09), (-0.20, -0.12), (-0.07, -0.22), (0.12, -0.225), (0.27, -0.13),
            (0.44, -0.095), (0.51, -0.02), (0.50, 0.06), (0.40, 0.065), (0.38, 0.02), (0.22, 0.02), (0.20, 0.065),
            (-0.19, 0.065), (-0.21, 0.02), (-0.37, 0.02), (-0.39, 0.065)]
    poly(d, T(body), hexc(body_col), w)
    # sọc trắng dọc thân + bóng sáng nóc
    d.line(T([(-0.44, -0.035), (0.42, -0.06)]), fill=(255, 255, 255, 210), width=int(.025 * k))
    d.line(T([(-0.14, -0.155), (0.0, -0.205)]), fill=(255, 255, 255, 120), width=int(.018 * k))
    # kính
    poly(d, T([(-0.14, -0.125), (-0.03, -0.198), (0.11, -0.2), (0.235, -0.13)]), hexc('#1d3348'), int(w * .7))
    d.line(T([(0.0, -0.185), (0.06, -0.135)]), fill=(255, 255, 255, 150), width=int(.02 * k))
    # cánh gió sau, đèn
    poly(d, T([(-0.53, -0.14), (-0.36, -0.135), (-0.37, -0.105), (-0.49, -0.1)]), hexc('#1d2b3a'), int(w * .7))
    poly(d, T([(0.40, -0.075), (0.48, -0.06), (0.47, -0.035), (0.40, -0.045)]), hexc('#FFE27A'), int(w * .5))
    poly(d, T([(-0.495, -0.04), (-0.45, -0.045), (-0.45, -0.015), (-0.495, -0.012)]), hexc('#FF5A4E'), int(w * .5))
    for c in wheels:
        circle(d, c, .062 * k, hexc('#c9d3dd'), int(w * .6))
        circle(d, c, .02 * k, hexc('#5c6a78'))
    return T, P


def thick_arc(d, box, start, end, width, color):
    d.arc(box, start, end, fill=OUTLINE, width=int(width * 1.6))
    d.arc(box, start + 1, end - 1, fill=color, width=int(width))


def icon_stop(d, S):
    """Thắng tay (phanh tay) ô tô: đế có ốp da, cần kim loại kéo chéo lên, tay nắm cao su có gân, nút nhả đỏ ở đầu;
    biểu tượng phanh tay (P) xanh ở góc."""
    w = int(S * .022)
    # đế + ốp da (boot)
    rrect(d, (S * .22, S * .55, S * .70, S * .64), S * .03, hexc('#2a323d'), w)
    boot = [(S * .27, S * .56), (S * .32, S * .47), (S * .44, S * .44), (S * .52, S * .47), (S * .56, S * .56)]
    poly(d, boot, hexc('#3a3f47'), w)
    for k in range(3):
        y = S * .49 + k * S * .022
        d.line([(S * .31 + k * S * .01, y), (S * .53 - k * S * .005, y)], fill=(0, 0, 0, 120), width=int(S * .008))
    # cần kéo chéo lên phải
    c = (S * .42, S * .47); ang = -38
    lever = rot([(c[0] - S * .02, c[1] - S * .035), (c[0] + S * .19, c[1] - S * .035), (c[0] + S * .19, c[1] + S * .035), (c[0] - S * .02, c[1] + S * .035)], ang, c)
    poly(d, lever, hexc('#C9D2DC'), w)
    d.line(rot([(c[0], c[1] - S * .012), (c[0] + S * .18, c[1] - S * .012)], ang, c), fill=(255, 255, 255, 200), width=int(S * .01))
    # tay nắm cao su có gân
    grip = rot([(c[0] + S * .17, c[1] - S * .055), (c[0] + S * .36, c[1] - S * .06), (c[0] + S * .38, c[1]), (c[0] + S * .36, c[1] + S * .06), (c[0] + S * .17, c[1] + S * .055)], ang, c)
    poly(d, grip, hexc('#1b2129'), w)
    for k in range(5):
        x = c[0] + S * .2 + k * S * .03
        d.line(rot([(x, c[1] - S * .045), (x, c[1] + S * .045)], ang, c), fill=hexc('#4a5562'), width=int(S * .012))
    tip = rot([(c[0] + S * .385, c[1])], ang, c)[0]
    circle(d, tip, S * .035, hexc('#E5483B'), int(w * .7))
    circle(d, (tip[0] - S * .008, tip[1] - S * .01), S * .01, (255, 255, 255, 200))
    # mũi tên kéo + ký hiệu phanh tay (P)
    poly(d, chevron(S * .78, S * .48, S * .09, S * .055, S * .026), hexc('#9CC8FF'), int(w * .6))
    pc = (S * .25, S * .26); circle(d, pc, S * .075, hexc('#2F7FE6'), w)
    d.arc((pc[0] - S * .11, pc[1] - S * .11, pc[0] + S * .11, pc[1] + S * .11), 130, 230, fill=hexc('#9CC8FF'), width=int(S * .016))
    d.arc((pc[0] - S * .11, pc[1] - S * .11, pc[0] + S * .11, pc[1] + S * .11), 310, 50, fill=hexc('#9CC8FF'), width=int(S * .016))
    f = ImageFont.truetype(FONT, int(S * .1))
    bb = d.textbbox((0, 0), 'P', font=f); d.text((pc[0] - (bb[2] - bb[0]) / 2 - bb[0], pc[1] - (bb[3] - bb[1]) / 2 - bb[1]), 'P', font=f, fill=(255, 255, 255, 255))


def icon_reverse(d, S):
    """Xe thể thao đang lùi: mũi tên cong lớn màu xanh lá vòng qua nóc chỉ về phía sau, vệt tốc độ phía trước xe."""
    w = int(S * .02)
    # vệt chuyển động (xe đi lùi sang trái → vệt ở phía đầu xe bên phải)
    for k, (x0, y) in enumerate(((S * .74, S * .47), (S * .78, S * .52), (S * .73, S * .57))):
        d.line([(x0, y), (x0 + S * .1 - k * S * .02, y)], fill=(255, 255, 255, 150), width=int(S * .014))
    car_side(d, S, S * .47, S * .56, .5, 0, w)
    # mũi tên cong từ phải vòng qua trên nóc xuống phía sau (trái)
    box = (S * .25, S * .17, S * .71, S * .45)
    cxm, cym = (box[0] + box[2]) / 2, (box[1] + box[3]) / 2; rx, ry = (box[2] - box[0]) / 2, (box[3] - box[1]) / 2
    thick_arc(d, box, 184, 345, S * .045, hexc('#4CD97E'))
    ex, ey = cxm - rx, cym                                   # đầu trái (9 giờ): tiếp tuyến chỉ thẳng xuống
    hw, hl = S * .075, S * .1
    poly(d, [(ex - hw, ey - S * .01), (ex + hw, ey - S * .01), (ex, ey + hl)], hexc('#4CD97E'), int(w * .9))
    rc = (S * .48, S * .29); circle(d, rc, S * .05, hexc('#1E6E3D'), int(w * .8))
    f = ImageFont.truetype(FONT, int(S * .07)); bb = d.textbbox((0, 0), 'R', font=f)
    d.text((rc[0] - (bb[2] - bb[0]) / 2 - bb[0], rc[1] - (bb[3] - bb[1]) / 2 - bb[1]), 'R', font=f, fill=(255, 255, 255, 255))


def icon_jump(d, S):
    """Xe thể thao chồm lên (đầu xe ngóc), 4 cụm khói phản lực trắng dưới gầm, bóng xe trên đất, mũi tên lên vàng."""
    w = int(S * .02)
    d.ellipse((S * .27, S * .625, S * .73, S * .665), fill=(5, 12, 20, 110))
    # khói phản lực
    for (x, y, r) in ((S * .38, S * .57, S * .055), (S * .47, S * .6, S * .06), (S * .58, S * .585, S * .05), (S * .31, S * .6, S * .04)):
        circle(d, (x, y), r, hexc('#E9F0F6'), int(w * .7))
        circle(d, (x - r * .3, y - r * .3), r * .35, (255, 255, 255, 255))
    car_side(d, S, S * .47, S * .42, .5, -12, w)
    for k, col in enumerate((hexc('#FFD23F'), hexc('#FFFFFF'))):
        poly(d, chevron(S * .75, S * .30 - k * S * .065, S * .12, S * .06, S * .028), col, int(w * .8))


def icon_missile(d, S):
    """Tên lửa chéo lên phải như PV_MissileHud: thân trắng, mũi + cánh đỏ, cửa sổ xanh, lửa vàng."""
    w = int(S * .022); c = (S * .5, S * .41); ang = 45
    def R(pts): return rot(pts, ang, c)
    poly(d, R([(c[0] - S * .04, c[1] + S * .17), (c[0], c[1] + S * .3), (c[0] + S * .04, c[1] + S * .17)]), hexc('#FFC93C'), int(w * .7))
    poly(d, R([(c[0] - S * .022, c[1] + S * .17), (c[0], c[1] + S * .24), (c[0] + S * .022, c[1] + S * .17)]), hexc('#FFF2B0'), int(w * .4))
    for side in (-1, 1):
        poly(d, R([(c[0] + side * S * .05, c[1] + S * .04), (c[0] + side * S * .125, c[1] + S * .16), (c[0] + side * S * .05, c[1] + S * .17)]), hexc('#E5483B'), w)
    poly(d, R([(c[0] - S * .05, c[1] + S * .18), (c[0] - S * .05, c[1] - S * .1), (c[0] + S * .05, c[1] - S * .1), (c[0] + S * .05, c[1] + S * .18)]), hexc('#F2F7FB'), w)
    poly(d, R([(c[0] - S * .05, c[1] - S * .095), (c[0], c[1] - S * .22), (c[0] + S * .05, c[1] - S * .095)]), hexc('#E5483B'), w)
    poly(d, R([(c[0] - S * .028, c[1] - S * .04), (c[0] + S * .028, c[1] - S * .04), (c[0] + S * .028, c[1] + S * .02), (c[0] - S * .028, c[1] + S * .02)]), hexc('#2C8DEB'), int(w * .6))
    d.line(R([(c[0] - S * .05, c[1] + S * .1), (c[0] + S * .05, c[1] + S * .1)]), fill=OUTLINE, width=int(w * .6))


def icon_arrow(d, S, sign):
    """21.270: mũi tên lái dày (sign = +1 phải, −1 trái): thân + đầu nhọn xanh ngọc có viền đậm, dải sáng phía trên,
    ba vệt tốc độ phía đuôi."""
    w = int(S * .026); cx, cy = S * .5, S * .40
    def X(pts): return [(cx + sign * x * S, cy + y * S) for x, y in pts]
    for k, (x0, y0, ln) in enumerate(((-.36, -.075, .09), (-.39, 0.0, .12), (-.36, .075, .09))):
        d.line(X([(x0, y0), (x0 + ln, y0)]), fill=(255, 255, 255, 170 - k * 20), width=int(S * .024))
    arrow = [(-.23, -.075), (.02, -.075), (.02, -.19), (.25, 0.0), (.02, .19), (.02, .075), (-.23, .075)]
    poly(d, X(arrow), hexc('#2FC7E8'), w)
    d.polygon(X([(-.21, -.058), (.04, -.058), (.04, -.155), (.205, -.012), (.04, -.012), (-.21, -.012)]), fill=hexc('#9BEBFF'))
    d.polygon(X([(.04, .03), (.19, .012), (.04, .15)]), fill=hexc('#1C9DC2'))


def icon_left(d, S): icon_arrow(d, S, -1)


def icon_right(d, S): icon_arrow(d, S, 1)


BUTTONS = [
    ('Left', 'TRÁI', '#2FC7E8', icon_left, .16),
    ('Right', 'PHẢI', '#2FC7E8', icon_right, .16),
    ('Gas', 'GA', '#FF8A3D', icon_gas, .17),
    ('Reverse', 'LÙI', '#35C76B', icon_reverse, .17),
    ('Stop', 'STOP', '#3D8BF0', icon_stop, .15),
    ('Jump', 'NHẢY', '#F4C24A', icon_jump, .15),
    ('Missile', 'TÊN LỬA', '#E8463A', icon_missile, .12),
]


def build_button(name, text, accent_hex, icon, size_frac, final=320):
    S = final * 3; accent = hexc(accent_hex)
    img = plate(S, accent)
    d = ImageDraw.Draw(img)
    icon(d, S)
    label(img, text, accent, size_frac)
    return img.resize((final, final), Image.LANCZOS)


def main():
    os.makedirs(OUT, exist_ok=True)
    # 21.270: vô lăng thay bằng 2 nút mũi tên trái/phải (build_wheel giữ lại để tham khảo, không xuất nữa).
    sheet = Image.new('RGBA', (330 * len(BUTTONS), 320), (70, 80, 90, 255))
    for i, (name, text, acc, icon, sf) in enumerate(BUTTONS):
        im = build_button(name, text, acc, icon, sf); im.save(os.path.join(OUT, 'PV_Btn_%s.png' % name))
        sheet.alpha_composite(im, (i * 330, 0))
    prev = os.path.join(ROOT, 'ArtSource', 'UI', 'touch_controls_preview.png')
    sheet.save(prev)
    print('ok', OUT)


if __name__ == '__main__':
    main()
