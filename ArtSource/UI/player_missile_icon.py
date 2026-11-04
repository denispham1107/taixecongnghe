from pathlib import Path
from PIL import Image, ImageDraw

# Code-authored vector silhouette, rasterized at 4x for the IMGUI texture.
root = Path(__file__).resolve().parents[2]
svg = '''<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256">
<g transform="rotate(38 128 128)" stroke="#123952" stroke-width="5" stroke-linejoin="round">
<path d="M114 197L110 219L128 241L146 219L142 197Z" fill="#ffb63c"/>
<path d="M106 143L79 182L79 215L112 197Z" fill="#e54c4f"/>
<path d="M150 143L177 182L177 215L144 197Z" fill="#b93447"/>
<path d="M102 72L102 190Q102 203 115 203H141Q154 203 154 190V72Z" fill="#eaf8ff"/>
<path d="M140 73H154V190Q154 203 140 203Z" fill="#77b5d4" stroke="none"/>
<path d="M102 74Q104 46 128 20Q152 46 154 74Z" fill="#fa665b"/>
<path d="M128 20Q145 52 142 74H154Q152 46 128 20" fill="#cf3549" stroke="none"/>
<path d="M110 103H146V128H110Z" fill="#139dd3" stroke="none"/>
<path d="M111 83V177" fill="none" stroke="#ffffff" stroke-width="6" stroke-linecap="round"/>
</g></svg>'''
(root / 'ArtSource/UI/player_missile_icon.svg').write_text(svg, encoding='utf8')
scale = 4
im = Image.new('RGBA', (256*scale, 256*scale))
d = ImageDraw.Draw(im)
def poly(points, fill, outline='#123952'):
    pts = [(int(x*scale), int(y*scale)) for x,y in points]
    d.polygon(pts, fill=fill)
    if outline: d.line(pts+[pts[0]], fill=outline, width=5*scale, joint='curve')
poly([(114,197),(110,219),(128,241),(146,219),(142,197)], '#ffb63c')
poly([(106,143),(79,182),(79,215),(112,197)], '#e54c4f')
poly([(150,143),(177,182),(177,215),(144,197)], '#b93447')
d.rounded_rectangle((102*scale,66*scale,154*scale,203*scale), radius=12*scale, fill='#eaf8ff', outline='#123952', width=5*scale)
d.rectangle((140*scale,73*scale,149*scale,195*scale), fill='#77b5d4')
poly([(102,74),(106,54),(128,20),(150,54),(154,74)], '#fa665b')
poly([(128,20),(150,54),(154,74),(142,74),(139,50)], '#cf3549', None)
d.rectangle((110*scale,103*scale,146*scale,128*scale), fill='#139dd3')
d.line([(111*scale,83*scale),(111*scale,177*scale)], fill='white', width=6*scale)
im = im.rotate(-38, Image.Resampling.BICUBIC).resize((256,256), Image.Resampling.LANCZOS)
im.save(root / 'Assets/PhoViet/Resources/PlayerMissile/PV_MissileHud.png')
