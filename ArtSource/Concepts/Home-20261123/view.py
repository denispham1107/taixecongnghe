# 21.330 — ảnh xem nhanh: trang điện thoại chia 2 cột
import sys
from PIL import Image
for m in sys.argv[1:] or 'ABC':
    im=Image.open(f'out/{m}-phone.jpg'); w,h=im.size; half=(h+1)//2
    c=Image.new('RGB',(w*2+20,half),'white'); c.paste(im.crop((0,0,w,half)),(0,0)); c.paste(im.crop((0,half,w,h)),(w+20,0))
    c.resize((c.width//2,c.height//2)).save(f'raw/{m}-phone-view.jpg',quality=85)
