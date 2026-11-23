# 21.341 — tìm khung nút cũ trong ảnh trang chủ bằng so khớp mẫu (NCC qua FFT, numpy) với ảnh nút cũ (commit a66cd40).
# Trả về khung TẤM NÚT (vùng 36..477 của ảnh 512) theo toạ độ ảnh gốc.
import sys, json
import numpy as np
from PIL import Image

def gray(a): return a[..., 0] * .299 + a[..., 1] * .587 + a[..., 2] * .114

def tmpl(name, s):
    im = Image.open(f'old-{name}.png').convert('RGBA').crop((36, 36, 477, 482)).resize((s, s), Image.LANCZOS)
    a = np.asarray(im).astype(float) / 255
    return gray(a[..., :3]) * a[..., 3] * .68           # nền tối giả định (premultiplied)

def ncc(I, T):
    h, w = T.shape; H, W = I.shape
    t = T - T.mean(); tn = np.sqrt((t * t).sum())
    F = np.fft.rfft2(I); Ft = np.fft.rfft2(t[::-1, ::-1], s=I.shape)
    c = np.fft.irfft2(F * Ft, s=I.shape)[h - 1:, w - 1:]
    S = np.pad(I, ((1, 0), (1, 0))).cumsum(0).cumsum(1); S2 = np.pad(I * I, ((1, 0), (1, 0))).cumsum(0).cumsum(1)
    def box(S): return S[h:, w:] - S[:-h, w:] - S[h:, :-w] + S[:-h, :-w]
    n = h * w; var = box(S2) - box(S) ** 2 / n
    return c / (np.sqrt(np.maximum(var, 1e-6)) * tn)

def find(img, name, cx, cy, smin, smax, pad=90):
    best = (-9, None)
    for s in range(smin, smax + 1, 2):
        x0, y0 = int(cx - s / 2 - pad), int(cy - s / 2 - pad); x0, y0 = max(0, x0), max(0, y0)
        x1, y1 = min(img.shape[1], int(cx + s / 2 + pad)), min(img.shape[0], int(cy + s / 2 + pad))
        I = img[y0:y1, x0:x1]
        if I.shape[0] <= s or I.shape[1] <= s: continue
        r = ncc(I, tmpl(name, s)); k = np.unravel_index(np.argmax(r), r.shape)
        if r[k] > best[0]: best = (float(r[k]), [x0 + int(k[1]), y0 + int(k[0]), x0 + int(k[1]) + s, y0 + int(k[0]) + s])
    return best

if __name__ == '__main__':
    img = gray(np.asarray(Image.open(sys.argv[1]).convert('RGB')).astype(float) / 255)
    est = json.loads(sys.argv[2]); out = {}
    for k, v in est.items():
        sc, box = find(img, k, v[0], v[1], v[2], v[3]); out[k] = box + [round(sc, 3)]
    print(json.dumps(out))
