"""Selective cloth/paint masks retain the original face, hands and denim colours."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import numpy as np,json
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'Assets/PhoViet/Art/UrbanRider';NAME='PV_UrbanRider'
image=Image.open(OUT/(NAME+'_Palette.png')).convert('RGB');a=np.asarray(image,dtype=float)/255
layers={key:Image.new('L',image.size) for key in ['shirt','paint','head','tail']};draw={key:ImageDraw.Draw(v) for key,v in layers.items()}
for face in json.loads((ROOT/'.tools/urban-rider-uvregions.json').read_text()):
    uv=[(u*(image.width-1),(1-v)*(image.height-1)) for u,v in face['uv']]
    for key in layers:
        if face[key]:draw[key].polygon(uv,fill=255)
r,g,b=a[:,:,0],a[:,:,1],a[:,:,2];value=a.max(2);sat=(value-a.min(2))/np.maximum(value,.001)
white=(value>.42)&(sat<.28);skin=(r>b*1.12)&(g>b*1.05)
# UV edges need padding for bilinear filtering and mipmaps: otherwise every
# decimated triangle acquires a white outline when the cloth/body is tinted.
shirt=np.asarray(layers['shirt'].filter(ImageFilter.MaxFilter(11)))>0
paint=np.asarray(layers['paint'].filter(ImageFilter.MaxFilter(11)))>0
mask=np.zeros_like(a,dtype=np.uint8);mask[:,:,0]=(shirt&white&~skin)*255;mask[:,:,1]=(paint&white&~shirt)*255
Image.fromarray(mask).filter(ImageFilter.GaussianBlur(.35)).save(OUT/(NAME+'_Regions.png'))
emission=np.zeros_like(mask);head=(np.asarray(layers['head'])>0)&(value>.6)&(sat<.22);tail=(np.asarray(layers['tail'])>0)&(r>g*1.4)&(r>b*1.4)&(r>.22)
emission[head]=[190,220,255];emission[tail]=[255,8,3];Image.fromarray(emission).save(OUT/(NAME+'_Emission.png'))
report=dict(shirtPixels=int((mask[:,:,0]>0).sum()),paintPixels=int((mask[:,:,1]>0).sum()),headPixels=int(head.sum()),tailPixels=int(tail.sum()),originalColoursRetained=True)
(ROOT/'Captures/urban-rider-maps.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report))
