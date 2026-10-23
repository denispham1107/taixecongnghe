"""2048 single-material bike/person atlases, after Blender --phase prepare.
Shared normal/surface/emission textures avoid duplicating GPU data for clothing colours.
"""
from pathlib import Path
from collections import deque
import numpy as np
from PIL import Image,ImageFilter
import json
ROOT=Path(__file__).resolve().parents[2];OLD=ROOT/'Assets/PhoViet/Art/Bikes/Meshy';OUT=ROOT/'Assets/PhoViet/Art/Bikes/MeshyRiders'
PERSON=ROOT/'Assets/MeshyImports/Golden Star Stance_20261023_150430'
COLOURS={'PV_HeroScooter':None,'PV_CityScooter':(.35,.68,.92),'PV_Underbone':(.92,.82,.57),'PV_SportBike':(.52,.29,.76),'PV_Racer':(.22,.25,.29)}

def largest_component(mask):
 remaining=set(map(tuple,np.argwhere(mask)));best=[]
 while remaining:
  seed=remaining.pop();queue=deque([seed]);component=[seed]
  while queue:
   y,x=queue.popleft()
   for n in [(y-1,x),(y+1,x),(y,x-1),(y,x+1)]:
    if n in remaining:remaining.remove(n);queue.append(n);component.append(n)
  if len(component)>len(best):best=component
 result=np.zeros(mask.shape,bool)
 if best:
  y,x=np.asarray(best).T;result[y,x]=True
 return result

def remove_star(pixels,mask):
 y,x=np.where(mask);assert len(y)>100,'Expected the received yellow chest star'
 y0,y1=max(0,y.min()-12),min(mask.shape[0],y.max()+13);x0,x1=max(0,x.min()-12),min(mask.shape[1],x.max()+13)
 a=pixels[y0:y1,x0:x1].copy();unknown=mask[y0:y1,x0:x1].copy();original=unknown.copy();known=~unknown
 for _ in range(500):
  total=np.zeros_like(a);count=np.zeros(known.shape,np.float32)
  for axis,shift in [(0,-1),(0,1),(1,-1),(1,1)]:
   valid=np.roll(known,shift,axis);total+=np.roll(a,shift,axis)*valid[:,:,None];count+=valid
  fill=unknown&(count>0)
  if not fill.any():break
  a[fill]=total[fill]/count[fill,None];known[fill]=True;unknown[fill]=False
  if not unknown.any():break
 assert not unknown.any(),'Could not fill star mask'
 for _ in range(16):
  average=(np.roll(a,-1,0)+np.roll(a,1,0)+np.roll(a,-1,1)+np.roll(a,1,1))*.25;a[original]=average[original]
 result=pixels.copy();result[y0:y1,x0:x1]=a;return result

def atlas(bike,rider,mode='RGB'):
 image=Image.new(mode,(2048,2048));image.paste(bike.resize((2048,1024),Image.Resampling.LANCZOS),(0,0));image.paste(rider.resize((2048,1024),Image.Resampling.LANCZOS),(0,1024));return image

def main():
 OUT.mkdir(parents=True,exist_ok=True)
 source=np.asarray(Image.open(PERSON/'meshy_basecolor.png').convert('RGB')).astype(np.float32)/255
 shirt=np.asarray(Image.open(ROOT/'ArtSource/Blender/MeshyRider_ShirtMask.png').convert('L').filter(ImageFilter.MaxFilter(9)))>127
 gold=shirt&(source[:,:,0]>.43)&(source[:,:,1]>.31)&(source[:,:,2]<.38)&(source[:,:,1]>source[:,:,2]*1.45)&(source[:,:,0]>source[:,:,1]*.82)
 star=largest_component(gold);star=np.asarray(Image.fromarray(star.astype('uint8')*255).filter(ImageFilter.MaxFilter(7)))>0
 plain=remove_star(source,star)
 for name,colour in COLOURS.items():
  body=source.copy() if colour is None else plain.copy()
  if colour is None:
   back_mask=ROOT/'ArtSource/Blender/MeshyHero_BackStarMask.png'
   if not back_mask.exists():raise FileNotFoundError('Run bake_hero_back_star.py with Blender first')
   alpha=np.asarray(Image.open(back_mask).convert('L').filter(ImageFilter.GaussianBlur(.45))).astype(np.float32)/255
   shade=.55+.45*source[:,:,0]
   gold=np.array([1.,.82,.09])[None,None,:]*shade[:,:,None]
   body=body*(1-alpha[:,:,None])+gold*alpha[:,:,None]
  if colour is not None:
   red=shirt&(body[:,:,0]>body[:,:,1]*1.6)&(body[:,:,0]>body[:,:,2]*1.6)&(body[:,:,0]-body[:,:,1]>.08)
   value=body.max(axis=2)
   for channel in range(3):body[:,:,channel][red]=value[red]*colour[channel]
  bike=Image.open(OLD/f'{name}_Palette.png').convert('RGB').crop((0,0,2048,1920))
  atlas(bike,Image.fromarray(np.uint8(np.clip(body,0,1)*255))).save(OUT/f'{name}_Palette.png')
 for name,source_name in [('Meshy_Normal.png','meshy_normal.png'),('Meshy_Surface.png','meshy_metallic_smoothness.png')]:
  old=OLD/('Meshy_Normal.png' if 'Normal' in name else 'PV_HeroScooter_Surface.png')
  atlas(Image.open(old).convert('RGBA').crop((0,0,2048,1920)),Image.open(PERSON/source_name).convert('RGBA'),'RGBA').save(OUT/name)
 emission=Image.new('RGB',(2048,2048));emission.paste(Image.open(OLD/'Meshy_Emission.png').convert('RGB').crop((0,0,2048,1920)).resize((2048,1024),Image.Resampling.LANCZOS),(0,0));emission.save(OUT/'Meshy_Emission.png')
 p=OUT/'source.json';report=json.loads(p.read_text());report.update(shirtColours=COLOURS,starPixelsRemoved=int(star.sum()),heroOriginalTexturePreserved=False,heroFrontTexturePreserved=True,heroBackStarAdded=True,sharedMaps=['normal','metallicSmoothness','emission']);p.write_text(json.dumps(report,indent=2))
 print(json.dumps({'success':True,'starPixelsRemoved':int(star.sum()),'variants':len(COLOURS)}))
if __name__=='__main__':main()
