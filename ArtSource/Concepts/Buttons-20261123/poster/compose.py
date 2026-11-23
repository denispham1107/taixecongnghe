# 21.341 — thay bộ nút cũ trong 3 ảnh trang chủ (poster-home-{mobile,lazer,khien}.jpg) bằng 9 nút mẫu A mới.
# Đọc ảnh gốc goc-poster-home-*.jpg (bản sao trước khi sửa) + khung tấm nút cũ box-*.json (match.py), cho mỗi nút:
#   1) xoá nút cũ: lấp vùng tấm nút (nới 7 %) bằng khuếch tán nhiều tầng từ viền (không cần OpenCV);
#   2) dán nút mới raw/game-*.png cùng khung (tấm nút cũ = vùng 36..477 của ảnh 512 → suy ra khung nút), đục 68 % như trong game;
#      nút đang giữ trong ảnh (quầng màu quanh nút: GA, LAZER) dán đục 100 %, thu 7 % như DrawArtButton;
#   3) vẽ lại huy hiệu số «10» của TÊN LỬA.
# Chạy:  python -X utf8 compose.py   → ghi đè Assets/WebGLTemplates/PhoVietPWA/poster-home-*.jpg (cỡ giữ nguyên)
import json, os
import numpy as np
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__)); ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'WebGLTemplates', 'PhoVietPWA')
RAW = os.path.join(HERE, '..', 'raw')
FONT = os.path.join(ROOT, 'Assets', 'PhoViet', 'Fonts', 'BeVietnamPro-SemiBold.ttf')
HELD = {'mobile': {'Gas'}, 'lazer': {'Laser'}, 'khien': {'Gas', 'Laser'}}


def diffuse(region, mask):
    """Lấp điểm mask=True bằng nghiệm Laplace từ điểm biết (thô → mịn)."""
    out = region.copy()
    levels = []
    r, m = region, mask
    while min(m.shape) > 24:
        levels.append((r, m))
        h, w = m.shape[0] // 2 * 2, m.shape[1] // 2 * 2
        r = r[:h, :w].reshape(h // 2, 2, w // 2, 2, 3).mean((1, 3)); m = m[:h, :w].reshape(h // 2, 2, w // 2, 2).any((1, 3))
    levels.append((r, m))
    guess = None
    for r, m in reversed(levels):
        cur = r.copy()
        if guess is not None:
            g = np.asarray(Image.fromarray(np.clip(guess, 0, 255).astype(np.uint8)).resize((r.shape[1], r.shape[0]), Image.BILINEAR)).astype(float)
            cur[m] = g[m]
        else:
            cur[m] = r[~m].mean(0) if (~m).any() else 128
        for _ in range(60):
            p = np.pad(cur, ((1, 1), (1, 1), (0, 0)), mode='edge')
            avg = (p[:-2, 1:-1] + p[2:, 1:-1] + p[1:-1, :-2] + p[1:-1, 2:]) / 4
            cur[m] = avg[m]
        guess = cur
    out[mask] = guess[mask]
    return out


def badge(img, rect, text):
    x, y, R = rect
    cx, cy, r = x + .115 * R, y + .11 * R, .15 * R
    d = ImageDraw.Draw(img)
    d.ellipse((cx - r - r * .18, cy - r - r * .18, cx + r + r * .18, cy + r + r * .18), fill=(10, 22, 34, 255))
    d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=(255, 205, 64, 255))
    f = ImageFont.truetype(FONT, int(r * 1.15))
    d.text((cx, cy + r * .04), text, font=f, fill=(16, 38, 58, 255), anchor='mm')


def run(name):
    src = Image.open(os.path.join(HERE, f'goc-poster-home-{name}.jpg')).convert('RGB')
    a = np.asarray(src).astype(float)
    boxes = json.load(open(os.path.join(HERE, f'box-{name}.json')))
    rects = {}
    for k, (x0, y0, x1, y1, _) in boxes.items():
        s = x1 - x0; R = s * 512 / 441; rx, ry = x0 - 36 / 441 * s, y0 - 36 / 441 * s
        rects[k] = (rx, ry, R)
        pad = int(s * .07); X0, Y0, X1, Y1 = max(0, x0 - pad), max(0, y0 - pad), min(a.shape[1], x1 + pad), min(a.shape[0], y1 + pad)
        m = Image.new('L', (X1 - X0 + 40, Y1 - Y0 + 40), 0)
        ImageDraw.Draw(m).rounded_rectangle((20, 20, 20 + X1 - X0, 20 + Y1 - Y0), radius=int(s * .2), fill=255)
        mk = np.asarray(m) > 0
        sy0, sx0 = max(0, Y0 - 20), max(0, X0 - 20)
        reg = a[sy0:Y1 + 20, sx0:X1 + 20]
        mk = mk[(sy0 - (Y0 - 20)):(sy0 - (Y0 - 20)) + reg.shape[0], (sx0 - (X0 - 20)):(sx0 - (X0 - 20)) + reg.shape[1]]
        a[sy0:Y1 + 20, sx0:X1 + 20] = diffuse(reg, mk)
    img = Image.fromarray(np.clip(a, 0, 255).astype(np.uint8)).convert('RGBA')
    for k, (rx, ry, R) in rects.items():
        held = k in HELD[name]
        btn = Image.open(os.path.join(RAW, f'game-{k}.png')).convert('RGBA')
        size = R * (.93 if held else 1.0); ox, oy = rx + (R - size) / 2, ry + (R - size) / 2
        b = btn.resize((round(size), round(size)), Image.LANCZOS)
        al = b.getchannel('A').point(lambda v, f=(1.0 if held else .68): int(v * f)); b.putalpha(al)
        img.alpha_composite(b, (round(ox), round(oy)))
        if k == 'Missile': badge(img, (rx, ry, R), '10')
    img.convert('RGB').save(os.path.join(OUT, f'poster-home-{name}.jpg'), quality=90, optimize=True, progressive=True)
    print(name, src.size, len(rects), 'nút')


for n in ('mobile', 'lazer', 'khien'):
    run(n)
