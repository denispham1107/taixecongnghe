# 21.330 — cắt phần nền hồng (#ff00fe) dưới trang, xuất JPG xem trước
import sys, glob, os
from PIL import Image
import numpy as np
os.makedirs('out', exist_ok=True)
for f in sorted(glob.glob('raw/*.png')):
    im = Image.open(f).convert('RGB')
    if 'phone' in f: im = im.crop((0,0,780,im.height))   # khung 390 px × 2
    a = np.asarray(im)
    mag = (abs(a[:,:,0].astype(int)-255)<6)&(a[:,:,1]<6)&(abs(a[:,:,2].astype(int)-254)<6)
    rows = np.where(mag.mean(1) < 0.98)[0]
    h = rows.max()+1 if len(rows) else a.shape[0]
    im.crop((0,0,a.shape[1],h)).save('out/'+os.path.basename(f)[:-4]+'.jpg', quality=88)
    print(f, a.shape, '->', h, 'magenta at bottom' if h < a.shape[0] else 'NO MAGENTA (page taller than window)')
