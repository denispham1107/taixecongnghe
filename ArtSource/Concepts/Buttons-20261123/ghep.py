# 21.337 — ghép bộ nút lên ảnh màn chơi theo đúng công thức bố cục điện thoại (OpenWorldDriver: BtnSize = max(70, 13 % cao màn)).
# Ảnh nền raw/nen.png 1280×590 (tỉ lệ iPhone Pro Max ngang). Nút để yên vẽ 68 % đục như trong game.
# python -X utf8 ghep.py  → out/mau-{HienTai,A,B,C}.png + out/tonghop.jpg
import os
from PIL import Image, ImageDraw, ImageFont
HERE = os.path.dirname(os.path.abspath(__file__)); ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
RES = os.path.join(ROOT, 'Assets', 'PhoViet', 'Resources', 'TouchControls')
FONT = os.path.join(ROOT, 'Assets', 'PhoViet', 'Fonts', 'BeVietnamPro-SemiBold.ttf')
W, H = 1280, 590
B = max(70, H * .13); Gap = B * .14
Big, Stop, Small = B * 1.44375, B * 1.155, B * 1.21; Fire = Small * 1.05; Steer = B * 1.65
gas = (W - Big - 24, H - Big - 24, Big)
rev = (gas[0] - Small - Gap, gas[1] + Big - Small, Small)
stop = (rev[0] - Stop - Gap, rev[1] + Small - Stop, Stop)
jump = (gas[0], gas[1] - Big - Gap, Big)
fire = (rev[0] + Small / 2 - Fire / 2, rev[1] - Fire - Gap, Fire)
laser = (jump[0] + Big / 2 - Small / 2, jump[1] - Small - Gap + B * .16, Small)
left = (24 + 30, H - Steer - 24, Steer)            # +30: tai thỏ (Safe Area trái) khi cầm ngang
right = (left[0] + Steer + Gap, left[1], Steer)
shield = ((left[0] + Steer + right[0]) / 2 - Fire / 2, left[1] - Fire - Gap * 1.5, Fire)
KEEP = {'gas': 'Gas', 'stop': 'Stop', 'shield': 'Shield'}
NEW = {'left': left, 'right': right, 'laser': laser, 'missile': fire, 'jump': jump, 'reverse': rev}
OLD = {'left': 'Left', 'right': 'Right', 'laser': 'Laser', 'missile': 'Missile', 'jump': 'Jump', 'reverse': 'Reverse'}


def put(img, src, box, alpha=.68):
    x, y, s = box; b = src.convert('RGBA').resize((round(s), round(s)), Image.LANCZOS)
    a = b.getchannel('A').point(lambda v: int(v * alpha)); b.putalpha(a); img.alpha_composite(b, (round(x), round(y)))


def compose(mau):
    img = Image.open(os.path.join(HERE, 'raw', 'nen.png')).convert('RGBA').resize((W, H))
    for k, box in (('gas', gas), ('stop', stop), ('shield', shield)):
        put(img, Image.open(os.path.join(RES, f'PV_Btn_{KEEP[k]}.png')), box)
    for k, box in NEW.items():
        src = Image.open(os.path.join(RES, f'PV_Btn_{OLD[k]}.png')) if mau == 'HienTai' else Image.open(os.path.join(HERE, 'raw', f'{mau}-{k}.png'))
        put(img, src, box)
    d = ImageDraw.Draw(img); f = ImageFont.truetype(FONT, 30)
    title = {'HienTai': 'HIỆN TẠI', 'A': 'MẪU A · KÍNH NEON', 'B': 'MẪU B · TEM ĐUA', 'C': 'MẪU C · HUY HIỆU TRÒN'}[mau]
    d.rounded_rectangle((20, 16, 40 + d.textlength(title, font=f), 64), 10, fill=(10, 22, 34, 220))
    d.text((30, 20), title, font=f, fill=(255, 244, 214))
    os.makedirs(os.path.join(HERE, 'out'), exist_ok=True)
    img.convert('RGB').save(os.path.join(HERE, 'out', f'mau-{mau}.png'))
    return img.convert('RGB')


ims = [compose(m) for m in ('HienTai', 'A', 'B', 'C')]
sheet = Image.new('RGB', (W * 2 + 12, H * 2 + 12), (20, 24, 28))
for i, im in enumerate(ims): sheet.paste(im, ((i % 2) * (W + 12), (i // 2) * (H + 12)))
sheet.save(os.path.join(HERE, 'out', 'tonghop.jpg'), quality=90)
# bảng nút rời (nền tối) để nhìn chi tiết
rows = [Image.open(os.path.join(HERE, 'raw', f'sheet-{m}.png')).convert('RGB') for m in 'ABC']
tb = Image.new('RGB', (rows[0].width, sum(r.height for r in rows)))
y = 0
for r in rows: tb.paste(r, (0, y)); y += r.height
tb.save(os.path.join(HERE, 'out', 'bangnut.jpg'), quality=92)
print('ok')
