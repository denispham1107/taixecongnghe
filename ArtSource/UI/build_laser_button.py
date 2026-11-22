"""21.300 — nút LAZER cho bản đồ mở, cùng phong cách «B Arcade» với các nút cảm ứng (build_touch_controls.py: nền kính xanh đen,
viền xanh đen đậm + viền màu nhấn, chữ kem viền đậm). Màu nhấn tím điện để khác nút TÊN LỬA (đỏ).
Icon: siêu xe xanh nhìn ngang (như nút NHẢY) phóng 2 tia lazer đỏ từ đèn pha — lõi trắng, quầng đỏ phát sáng (làm mờ Gauss),
hai tia tỏa nhẹ, chớp sáng ở đầu đèn và chớp sao chỗ tia trúng, vài tia lửa.
Chạy:  python ArtSource/UI/build_laser_button.py   →  Assets/PhoViet/Resources/TouchControls/PV_Btn_Laser.png (512²)
"""
import math
import os
import sys
from PIL import Image, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(__file__))
import build_touch_controls as tc  # noqa: E402

ACCENT = '#A35CFF'


def glow_line(layer, pts, width, color, blur):
    g = Image.new('RGBA', layer.size, (0, 0, 0, 0)); ImageDraw.Draw(g).line(pts, fill=color, width=int(width), joint='curve')
    layer.alpha_composite(g.filter(ImageFilter.GaussianBlur(blur)))


def glow_disc(layer, c, r, color, blur):
    g = Image.new('RGBA', layer.size, (0, 0, 0, 0)); ImageDraw.Draw(g).ellipse((c[0] - r, c[1] - r, c[0] + r, c[1] + r), fill=color)
    layer.alpha_composite(g.filter(ImageFilter.GaussianBlur(blur)))


def build(final=tc.FINAL):
    S = final * 3
    img = tc.plate(S, tc.hexc(ACCENT))
    d = ImageDraw.Draw(img)
    w = int(S * .02)
    # xe nhìn ngang, đầu bên phải, hơi chúc xuống bên trái khung
    T, P = tc.car_side(d, S, S * .365, S * .45, .37, 0, w)
    lamp = P((0.45, -0.058))
    lamp2 = (lamp[0] - S * .012, lamp[1] + S * .016)          # đèn pha bên kia (khuất sau)
    # 2 tia gần song song, tỏa rất nhẹ, dừng trong khung nút
    ends = [(S * .815, lamp[1] - S * .055), (S * .815, lamp2[1] + S * .038)]
    starts = [lamp, lamp2]
    fx = Image.new('RGBA', img.size, (0, 0, 0, 0))
    for st0, e in zip(starts, ends):
        glow_line(fx, [st0, e], S * .07, (255, 40, 30, 150), S * .028)     # quầng rộng
        glow_line(fx, [st0, e], S * .034, (255, 70, 50, 230), S * .010)    # quầng hẹp
    img.alpha_composite(fx)
    d = ImageDraw.Draw(img)
    for st0, e in zip(starts, ends):
        d.line([st0, e], fill=(255, 120, 100, 255), width=int(S * .026))
        d.line([st0, e], fill=(255, 250, 245, 255), width=int(S * .012))   # lõi trắng
    # chớp đầu đèn
    fl = Image.new('RGBA', img.size, (0, 0, 0, 0))
    for st0 in starts:
        glow_disc(fl, st0, S * .045, (255, 90, 60, 230), S * .02)
        glow_disc(fl, st0, S * .02, (255, 255, 240, 255), S * .006)
    # chớp sao chỗ trúng (ngôi sao 4 cánh dài + quầng)
    hit = ((ends[0][0] + ends[1][0]) / 2 + S * .01, (ends[0][1] + ends[1][1]) / 2)
    glow_disc(fl, hit, S * .085, (255, 60, 40, 170), S * .035)
    img.alpha_composite(fl)
    d = ImageDraw.Draw(img)
    for e in ends:
        st = tc.star(e, S * .05, S * .015, -90)
        d.polygon(st, fill=(255, 236, 200, 255))
        d.polygon(tc.star(e, S * .03, S * .01, -45), fill=(255, 255, 255, 255))
    # tia lửa bắn ra chỗ trúng
    for k, (dx, dy) in enumerate(((.05, -.06), (.07, .0), (.045, .07), (-.01, -.075), (.0, .085))):
        a = (hit[0] + dx * S * .35, hit[1] + dy * S * .35); b = (hit[0] + dx * S * .7, hit[1] + dy * S * .7)
        d.line([a, b], fill=(255, 210, 120, 235), width=int(S * .008))
    tc.label(img, 'LAZER', tc.hexc(ACCENT), .15)
    return img.resize((final, final), Image.LANCZOS)


if __name__ == '__main__':
    out = os.path.join(tc.OUT, 'PV_Btn_Laser.png')
    build().save(out, optimize=True)
    print('ok', out)
