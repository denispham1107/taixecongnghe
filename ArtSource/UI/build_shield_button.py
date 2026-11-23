"""21.312 — nút KHIÊN cho bản đồ mở, cùng phong cách «B Arcade» với các nút cảm ứng (build_touch_controls.py: nền kính xanh đen,
viền xanh đen đậm + viền màu nhấn, chữ kem viền đậm). Màu nhấn xanh ngọc #33F0D2 (người dùng chọn), khớp màu khiên trong game.
Icon (vẽ 3× rồi thu nhỏ, chi tiết):
  • Quầng năng lượng xanh ngọc tỏa sau khiên + vòng sóng mảnh.
  • Khiên hình huy hiệu (đỉnh cong, hai vai, nhọn ở đáy): viền thép 2 tông có vát sáng/tối, 8 đinh tán, viền trong vàng kim.
  • Mặt khiên: chuyển màu xanh ngọc sáng → xanh lục lam đậm, LƯỚI LỤC GIÁC năng lượng (vạch sáng, vài ô lóe sáng), quầng sáng giữa.
  • Huy hiệu giữa: tia sét năng lượng trắng-ngọc có quầng (lõi trắng nóng).
  • Vệt bóng kính chéo trên vai trái; 4 tia điện lởm chởm xẹt quanh mép khiên + chớp sao 4 cánh + tia lửa nhỏ.
Chạy:  python ArtSource/UI/build_shield_button.py   →  Assets/PhoViet/Resources/TouchControls/PV_Btn_Shield.png (512²)
"""
import sys as _sys
if '--force' not in _sys.argv:
    _sys.exit('21.339/21.340: PV_Btn_Shield.png nay xuất từ ArtSource/Concepts/Buttons-20261123/xuat_game.sh (mẫu A). Thêm --force nếu thật sự muốn vẽ lại nút cũ.')

import math
import os
import random
import sys
from PIL import Image, ImageDraw, ImageFilter, ImageChops

sys.path.insert(0, os.path.dirname(__file__))
import build_touch_controls as tc  # noqa: E402

ACCENT = '#33F0D2'


def glow(base, draw_fn, blur):
    g = Image.new('RGBA', base.size, (0, 0, 0, 0)); draw_fn(ImageDraw.Draw(g))
    base.alpha_composite(g.filter(ImageFilter.GaussianBlur(blur)))


def shield_pts(cx, cy, w, h, n=72):
    """Khiên huy hiệu: đỉnh hơi cong lên, vai bo tròn, hai cạnh cong vào nhọn ở đáy. Trả polygon (tâm cx, đỉnh cy)."""
    pts = []
    top = cy; sh_y = cy + h * .12; tip = cy + h
    # mép trên (cong lên ở giữa) từ trái sang phải
    for i in range(n // 4 + 1):
        t = i / (n // 4); x = cx - w / 2 + w * t
        pts.append((x, top + h * .05 * (1 - math.sin(math.pi * t)) ** 1.0))
    # cạnh phải: vai bo rồi cong vào tới mũi
    for i in range(1, n // 2):
        t = i / (n // 2)
        x = cx + w / 2 * (1 - t ** 1.9) * (1 - .06 * math.sin(math.pi * t))
        y = sh_y + (tip - sh_y) * t
        pts.append((x, y))
    pts.append((cx, tip))
    for x, y in reversed(pts[n // 4 + 1:-1]):
        pts.append((2 * cx - x, y))
    return pts


def inset(pts, c, k):
    return [(c[0] + (x - c[0]) * k, c[1] + (y - c[1]) * k) for x, y in pts]


def bolt_path(p0, p1, segs, jag, rng):
    pts = [p0]; dx, dy = p1[0] - p0[0], p1[1] - p0[1]; L = math.hypot(dx, dy); nx, ny = -dy / L, dx / L
    for i in range(1, segs):
        t = i / segs; a = jag * L * math.sin(math.pi * t) ** .7 * rng.uniform(-1, 1)
        pts.append((p0[0] + dx * t + nx * a, p0[1] + dy * t + ny * a))
    pts.append(p1); return pts


def build(final=tc.FINAL):
    S = final * 3
    acc = tc.hexc(ACCENT)
    img = tc.plate(S, acc)
    rng = random.Random(312)
    cx, cy, W, H = S * .5, S * .185, S * .47, S * .53
    c = (cx, cy + H * .45)
    outer = shield_pts(cx, cy, W, H)

    # quầng năng lượng sau khiên + vòng sóng
    glow(img, lambda d: d.ellipse((c[0] - S * .33, c[1] - S * .32, c[0] + S * .33, c[1] + S * .32), fill=(40, 240, 210, 120)), S * .06)
    glow(img, lambda d: d.ellipse((c[0] - S * .31, c[1] - S * .3, c[0] + S * .31, c[1] + S * .3), outline=(120, 255, 235, 150), width=int(S * .008)), S * .004)

    # bóng đổ của khiên
    m = Image.new('L', img.size, 0); ImageDraw.Draw(m).polygon(outer, fill=255)
    tc.shadow(img, m, (int(S * .012), int(S * .022)), S * .016, .6)

    d = ImageDraw.Draw(img)
    # viền thép 2 tông: nửa trái sáng, nửa phải tối (vát)
    steel = tc.vgrad(img.size, tc.hexc('#E8F2F7'), tc.hexc('#7D93A6'))
    tc.paste_masked(img, steel, m)
    shade = Image.new('L', img.size, 0); ImageDraw.Draw(shade).polygon([(cx, 0), (S, 0), (S, S), (cx, S)], fill=95)
    shade = ImageChops.multiply(shade, m)
    img.alpha_composite(Image.merge('RGBA', (Image.new('L', img.size, 20), Image.new('L', img.size, 40), Image.new('L', img.size, 60), shade)))
    # viền trong vàng kim
    k1 = .9; gold = inset(outer, c, k1)
    gm = Image.new('L', img.size, 0); ImageDraw.Draw(gm).polygon(gold, fill=255)
    tc.paste_masked(img, tc.vgrad(img.size, tc.hexc('#FFE58A'), tc.hexc('#C9861E')), gm)
    # mặt khiên
    k2 = .84; face = inset(outer, c, k2)
    fm = Image.new('L', img.size, 0); ImageDraw.Draw(fm).polygon(face, fill=255)
    tc.paste_masked(img, tc.vgrad(img.size, tc.hexc('#8FFFF0'), tc.hexc('#06706E')), fm)
    # quầng sáng giữa mặt
    core = tc.rgrad(img.size, (c[0], c[1] - S * .02), (210, 255, 248, 210), (60, 230, 205, 0), S * .2)
    tc.paste_masked(img, core, fm)

    # lưới lục giác năng lượng (cắt theo mặt khiên)
    hexl = Image.new('RGBA', img.size, (0, 0, 0, 0)); hd = ImageDraw.Draw(hexl)
    r = S * .043; hw = math.sqrt(3) * r
    lit = []
    for row in range(-2, 16):
        for col in range(-2, 16):
            x = S * .2 + col * hw + (row % 2) * hw / 2; y = S * .1 + row * r * 1.5
            pts = [(x + r * math.cos(math.radians(60 * i + 30)), y + r * math.sin(math.radians(60 * i + 30))) for i in range(6)]
            if rng.random() < .13: lit.append(pts)
            hd.line(pts + [pts[0]], fill=(200, 255, 245, 150), width=int(S * .0045))
    for pts in lit: hd.polygon(pts, fill=(170, 255, 240, 70))
    hexg = hexl.filter(ImageFilter.GaussianBlur(S * .004))
    hexl.alpha_composite(hexg)
    tc.paste_masked(img, hexl, fm)

    # vệt bóng kính chéo trên vai trái
    gl = Image.new('L', img.size, 0)
    ImageDraw.Draw(gl).polygon([(cx - W * .45, cy), (cx - W * .05, cy), (cx - W * .42, cy + H * .62), (cx - W * .5, cy + H * .4)], fill=85)
    gl = ImageChops.multiply(gl.filter(ImageFilter.GaussianBlur(S * .01)), fm)
    img.alpha_composite(Image.merge('RGBA', (Image.new('L', img.size, 255),) * 3 + (gl,)))

    d = ImageDraw.Draw(img)
    w = int(S * .018)
    d.line(outer + [outer[0]], fill=tc.OUTLINE, width=w, joint='curve')
    d.line(gold + [gold[0]], fill=(90, 60, 10, 255), width=int(w * .45), joint='curve')
    d.line(face + [face[0]], fill=tc.OUTLINE, width=int(w * .6), joint='curve')
    # đinh tán trên viền thép
    k_r = (1 + k1) / 2
    for t in (.03, .17, .31, .45, .55, .69, .83, .97):
        i = int(t * (len(outer) - 1)); x, y = outer[i]
        x, y = c[0] + (x - c[0]) * (k_r + .005), c[1] + (y - c[1]) * (k_r + .005)
        tc.circle(d, (x, y), S * .011, tc.hexc('#F6FBFF'), int(S * .004))
        tc.circle(d, (x + S * .003, y + S * .003), S * .004, tc.hexc('#6D8295'))

    # huy hiệu tia sét giữa khiên (quầng + viền + lõi)
    bc = (c[0], c[1] - S * .005); s = S * .2
    bolt = [(bc[0] + s * .12, bc[1] - s * .62), (bc[0] - s * .34, bc[1] + s * .08), (bc[0] - s * .02, bc[1] + s * .08),
            (bc[0] - s * .16, bc[1] + s * .62), (bc[0] + s * .34, bc[1] - s * .12), (bc[0] + s * .03, bc[1] - s * .12)]
    glow(img, lambda dd: dd.polygon(bolt, fill=(120, 255, 235, 255)), S * .025)
    d = ImageDraw.Draw(img)
    tc.poly(d, bolt, (255, 255, 255, 255), int(w * .75))
    inner_b = inset(bolt, bc, .55)
    d.polygon(inner_b, fill=(205, 255, 248, 255))

    # tia điện xẹt quanh mép khiên
    arcs = Image.new('RGBA', img.size, (0, 0, 0, 0)); ad = ImageDraw.Draw(arcs)
    paths = []
    for (i0, i1, out) in ((4, 14, 1.12), (30, 42, 1.1), (58, 66, 1.12), (20, 27, 1.13)):
        p0 = outer[i0 % len(outer)]; p1 = outer[i1 % len(outer)]
        p0 = (c[0] + (p0[0] - c[0]) * 1.03, c[1] + (p0[1] - c[1]) * 1.03); p1 = (c[0] + (p1[0] - c[0]) * out, c[1] + (p1[1] - c[1]) * out)
        paths.append(bolt_path(p0, p1, 9, .22, rng))
    for p in paths: ad.line(p, fill=(60, 255, 220, 255), width=int(S * .02), joint='curve')
    img.alpha_composite(arcs.filter(ImageFilter.GaussianBlur(S * .012)))
    d = ImageDraw.Draw(img)
    for p in paths:
        d.line(p, fill=(150, 255, 240, 255), width=int(S * .009), joint='curve')
        d.line(p, fill=(255, 255, 255, 255), width=int(S * .004), joint='curve')
    # chớp sao ở đầu tia + tia lửa
    for p in paths:
        e = p[-1]
        glow(img, lambda dd: dd.ellipse((e[0] - S * .03, e[1] - S * .03, e[0] + S * .03, e[1] + S * .03), fill=(80, 255, 225, 200)), S * .012)
        d = ImageDraw.Draw(img)
        d.polygon(tc.star(e, S * .032, S * .008, -90), fill=(240, 255, 252, 255))
        d.polygon(tc.star(e, S * .018, S * .006, -45), fill=(255, 255, 255, 255))
        for _ in range(3):
            a = rng.uniform(0, 2 * math.pi); r0 = S * .04; r1 = S * rng.uniform(.055, .075)
            d.line([(e[0] + math.cos(a) * r0, e[1] + math.sin(a) * r0), (e[0] + math.cos(a) * r1, e[1] + math.sin(a) * r1)], fill=(200, 255, 245, 230), width=int(S * .005))

    tc.label(img, 'KHIÊN', acc, .15)
    return img.resize((final, final), Image.LANCZOS)


if __name__ == '__main__':
    out = os.path.join(tc.OUT, 'PV_Btn_Shield.png')
    build().save(out, optimize=True)
    print('ok', out)
