# 21.336 — đổi màu chủ đạo xe người chơi (Blue Petshop Supercar) từ xanh dương sang vàng.
# Đọc bản gốc xanh (ArtSource/Concepts/XeVang-20261123/PV_BlueSupercar_Palette-xanh-goc.png), chỉ đổi điểm ảnh có sắc xanh dương
# (hue 185–265°, độ bão hoà > 0,2), giữ đen/xám/trắng/chữ/đèn; ghi đè Assets/PhoViet/Art/PlayerCar/PV_BlueSupercar_Palette.png.
# Chạy:  python -X utf8 AgentScripts/recolor_player_yellow.py [hue_vàng=50] [tăng_sáng=1.45]
# Muốn trả về xanh: chép lại tệp gốc ở thư mục ArtSource trên vào Assets/PhoViet/Art/PlayerCar/PV_BlueSupercar_Palette.png.
import sys
import numpy as np
from PIL import Image

SRC = "ArtSource/Concepts/XeVang-20261123/PV_BlueSupercar_Palette-xanh-goc.png"
DST = "Assets/PhoViet/Art/PlayerCar/PV_BlueSupercar_Palette.png"
HUE = float(sys.argv[1]) if len(sys.argv) > 1 else 50.0
GAIN = float(sys.argv[2]) if len(sys.argv) > 2 else 1.45


def smooth(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0, 1)
    return t * t * (3 - 2 * t)


a = np.asarray(Image.open(SRC).convert("RGB")).astype(np.float64) / 255.0
r, g, b = a[..., 0], a[..., 1], a[..., 2]
mx, mn = a.max(2), a.min(2)
d = mx - mn
s = np.where(mx > 1e-5, d / np.maximum(mx, 1e-5), 0)
h = np.zeros_like(mx)
nz = d > 1e-5
rm = nz & (mx == r); gm = nz & (mx == g) & ~rm; bm = nz & ~rm & ~gm
h[rm] = (60 * ((g - b)[rm] / d[rm])) % 360
h[gm] = 60 * ((b - r)[gm] / d[gm]) + 120
h[bm] = 60 * ((r - g)[bm] / d[bm]) + 240

# trọng số: sắc xanh dương + đủ bão hoà (mép mềm để không lộ viền)
w = smooth(185, 200, h) * (1 - smooth(250, 265, h)) * smooth(0.2, 0.38, s)

nh = HUE + (h - 215) * 0.12                      # giữ chút biến thiên sắc
ns = np.clip(s * 1.05, 0, 0.92)
nv = np.clip(mx * GAIN, 0, 1) ** 0.85            # vàng cần sáng hơn xanh mới ra vàng (không thành ô-liu)

c = nv * ns
x = c * (1 - np.abs((nh / 60) % 2 - 1))
m = nv - c
# HUE nằm trong [0,60) → (C, X, 0)
yr, yg, yb = c + m, x + m, m

out = np.stack([r * (1 - w) + yr * w, g * (1 - w) + yg * w, b * (1 - w) + yb * w], -1)
Image.fromarray((np.clip(out, 0, 1) * 255 + .5).astype(np.uint8)).save(DST, optimize=True)
print("đã đổi", round(float((w > .5).mean()) * 100, 1), "% điểm ảnh sang vàng, hue", HUE, "→", DST)
