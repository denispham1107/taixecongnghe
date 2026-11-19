"""21.271 — texture cho mặt sông bản đồ mở (không cần Blender, chỉ numpy + PIL).

- OW_RiverNormal.png 512² : pháp tuyến sóng lăn tăn, lặp liền (tổng hợp phổ bằng FFT nên tự tuần hoàn), sóng dài theo trục U
  (= chiều dòng chảy trên lưới mặt sông) và gợn ngắn nhiều hướng. Mã hóa OpenGL (G = +V), Unity nhập dạng Normal map.
- OW_RiverFoam.png   256² : bọt nước xám (ô bọt Worley + loang FBM), lặp liền; dùng cho đường nước vỗ sát kè và vệt sóng sau tàu.

Chạy:  python ArtSource/Blender/OpenWorld/build_river_textures.py
"""
import os
import numpy as np
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'PhoViet', 'OpenWorld', 'River')
rng = np.random.default_rng(271)


def spectral_height(n, slope, wind=(1.0, 0.0), spread=1.2, kmin=2, kmax=None):
    """Chiều cao tuần hoàn: phổ biên độ k^-slope, ưu tiên hướng gió (|cos|^spread), lọc dải [kmin, kmax]."""
    kx = np.fft.fftfreq(n) * n
    ky = np.fft.fftfreq(n) * n
    KX, KY = np.meshgrid(kx, ky)
    k = np.hypot(KX, KY)
    k[0, 0] = 1
    amp = k ** (-slope)
    wx, wy = wind
    cosang = np.abs((KX * wx + KY * wy) / k)
    amp *= 0.35 + 0.65 * cosang ** spread
    amp[k < kmin] = 0
    if kmax: amp[k > kmax] *= np.exp(-(k[k > kmax] - kmax) / 6.0)
    phase = rng.uniform(0, 2 * np.pi, (n, n))
    spec = amp * np.exp(1j * phase)
    h = np.real(np.fft.ifft2(spec))
    return (h - h.mean()) / (h.std() + 1e-9)


def normal_map(n=512):
    swell = spectral_height(n, 2.2, wind=(1, 0), spread=2.0, kmin=2, kmax=10)          # sóng dài theo dòng
    chop = spectral_height(n, 1.6, wind=(0.8, 0.6), spread=0.6, kmin=8, kmax=60)       # gợn ngắn nhiều hướng
    fine = spectral_height(n, 1.2, wind=(0.3, 1.0), spread=0.3, kmin=40, kmax=140)     # lăn tăn
    h = swell * 1.0 + chop * 0.10 + fine * 0.012     # đạo hàm khuếch đại tần số cao → trọng số gợn nhỏ phải thấp
    dx = (np.roll(h, -1, axis=1) - np.roll(h, 1, axis=1)) * 0.5
    dy = (np.roll(h, -1, axis=0) - np.roll(h, 1, axis=0)) * 0.5
    s = 0.32 / (np.sqrt((dx * dx + dy * dy).mean()) + 1e-9)          # độ dốc trung bình ≈ 0,32 (≈ 18°); shader chỉnh thêm
    nx, ny = -dx * s, dy * s          # ảnh: hàng tăng xuống dưới → đảo dấu y cho G = +V (OpenGL)
    nz = np.ones_like(h)
    l = np.sqrt(nx * nx + ny * ny + nz * nz)
    rgb = np.stack([nx / l, ny / l, nz / l], -1) * 0.5 + 0.5
    return Image.fromarray((rgb * 255).round().clip(0, 255).astype(np.uint8), 'RGB')


def worley(n, cells):
    jitter = rng.uniform(0, 1, (cells, cells, 2))
    yy, xx = np.mgrid[0:n, 0:n] / n * cells
    f1 = np.full((n, n), 9.0); f2 = np.full((n, n), 9.0)
    cx, cy = np.floor(xx).astype(int), np.floor(yy).astype(int)
    for oy in (-1, 0, 1):
        for ox in (-1, 0, 1):
            gx, gy = cx + ox, cy + oy
            j = jitter[gy % cells, gx % cells]
            px, py = gx + j[..., 0], gy + j[..., 1]
            d = np.hypot(px - xx, py - yy)
            f2 = np.where(d < f1, f1, np.minimum(f2, d)); f1 = np.minimum(f1, d)
    return f1, f2


def foam_map(n=256):
    f1, f2 = worley(n, 14)
    edges = 1 - np.clip((f2 - f1) / 0.22, 0, 1)                   # viền ô bọt
    f1b, f2b = worley(n, 34)
    small = 1 - np.clip((f2b - f1b) / 0.18, 0, 1)
    patch = spectral_height(n, 1.8, wind=(1, 0.2), spread=0.4, kmin=2, kmax=24)
    patch = np.clip(patch * 0.45 + 0.5, 0, 1)
    foam = np.clip(edges * 0.65 + small * 0.35, 0, 1) * (0.35 + 0.65 * patch)
    foam = np.clip((foam - 0.12) / 0.75, 0, 1) ** 0.9
    return Image.fromarray((foam * 255).round().astype(np.uint8), 'L')


def main():
    os.makedirs(OUT, exist_ok=True)
    normal_map().save(os.path.join(OUT, 'OW_RiverNormal.png'))
    foam_map().save(os.path.join(OUT, 'OW_RiverFoam.png'))
    print('ok', OUT)


if __name__ == '__main__':
    main()
