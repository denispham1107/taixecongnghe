# 21.330 — ảnh tổng hợp mỗi mẫu: máy tính 1440 px bên trái, điện thoại 390 px (2 cột) bên phải
from PIL import Image, ImageDraw, ImageFont
NAMES={'A':'MẪU A — ĐÊM ĐUA','B':'MẪU B — GIẤY ĐUA SÁNG','C':'MẪU C — MÀN CHÀO TRONG GAME'}
font=ImageFont.truetype('font.ttf',46); small=ImageFont.truetype('font.ttf',26)
for m,t in NAMES.items():
    pc=Image.open(f'out/{m}-pc.jpg'); ph=Image.open(f'out/{m}-phone.jpg')
    ph=ph.resize((390,ph.height*390//780)); half=(ph.height+1)//2
    W=60+pc.width+60+390*2+30+60; H=140+max(pc.height,half)+60
    c=Image.new('RGB',(W,H),(232,226,210)); d=ImageDraw.Draw(c)
    d.text((60,40),t,font=font,fill=(18,35,58))
    d.text((60+pc.width+60,58),'ĐIỆN THOẠI (390 px, 2 cột nối tiếp)',font=small,fill=(18,35,58))
    d.text((60+pc.width-420,58),'MÁY TÍNH (1440 px)',font=small,fill=(18,35,58))
    c.paste(pc,(60,120)); x=60+pc.width+60
    c.paste(ph.crop((0,0,390,half)),(x,120)); c.paste(ph.crop((0,half,390,ph.height)),(x+420,120))
    c.save(f'trangchu21330-mau{m}.jpg',quality=86); print(m,c.size)
