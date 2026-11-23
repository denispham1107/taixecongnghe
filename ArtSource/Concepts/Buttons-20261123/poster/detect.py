# 21.341 — dò khung nút cũ trong 3 ảnh trang chủ: tìm viền màu nhấn quanh tâm ước lượng (đo từ ảnh xem trước 1389 px × 2).
import sys, json
import numpy as np
from PIL import Image
ACC = {'Left': '2FC7E8', 'Right': '2FC7E8', 'Gas': 'FF8A3D', 'Reverse': '35C76B', 'Stop': '3D8BF0', 'Jump': 'F4C24A',
       'Missile': 'E8463A', 'Laser': 'A35CFF', 'Shield': '33F0D2'}
def hx(h): return np.array([int(h[i:i + 2], 16) for i in (0, 2, 4)], float)
def find(a, name, cx, cy, half):
    x0, y0 = max(0, cx - half), max(0, cy - half); x1, y1 = min(a.shape[1], cx + half), min(a.shape[0], cy + half)
    sub = a[y0:y1, x0:x1]; c = hx(ACC[name])
    # sắc giống màu nhấn: so hướng màu (chuẩn hoá) + đủ bão hoà
    n = sub / np.maximum(1, np.linalg.norm(sub, axis=2, keepdims=True)); cn = c / np.linalg.norm(c)
    sat = sub.max(2) - sub.min(2)
    m = ((n * cn).sum(2) > .985) & (sat > 60)
    rows = m.sum(1); cols = m.sum(0)
    ry = np.where(rows > rows.max() * .45)[0]; cxs = np.where(cols > cols.max() * .45)[0]
    return [int(x0 + cxs.min()), int(y0 + ry.min()), int(x0 + cxs.max()), int(y0 + ry.max())]
if __name__ == '__main__':
    img = sys.argv[1]; est = json.loads(sys.argv[2]); half = int(sys.argv[3]) if len(sys.argv) > 3 else 170
    a = np.asarray(Image.open(img).convert('RGB')).astype(float)
    out = {k: find(a, k, int(v[0]), int(v[1]), half) for k, v in est.items()}
    print(json.dumps(out))
