"""Build single-material bike/rider atlases from the received Meshy textures.
Run with normal Python (Pillow + NumPy), before integrate_meshy_bikes.py in Blender.
The imported source textures and existing bike assets are never overwritten.
"""
from pathlib import Path
import numpy as np
from PIL import Image
import json
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Assets/MeshyImports/Red Honda Scooter Front View_20261023_023822'
OUT=ROOT/'Assets/PhoViet/Art/Bikes/Meshy'
VARIANTS={'PV_HeroScooter':(0.96,0.97,1.0),'PV_CityScooter':(.07,.70,.64),'PV_Underbone':(.10,.33,.95),'PV_SportBike':(1.,.74,.10),'PV_Racer':(1.,.20,.07)}
def build():
 OUT.mkdir(parents=True,exist_ok=True)
 source=np.asarray(Image.open(SOURCE/'meshy_basecolor.png').convert('RGB').resize((2048,1920),Image.Resampling.LANCZOS)).astype(np.float32)/255
 red=(source[:,:,0]>source[:,:,1]*1.3)&(source[:,:,0]>source[:,:,2]*1.3)&(source[:,:,0]-source[:,:,1]>.08)
 for name,color in VARIANTS.items():
  paint=source.copy();value=source.max(axis=2)
  for channel in range(3):paint[:,:,channel][red]=value[red]*color[channel]
  atlas=Image.new('RGB',(2048,2048),(50,50,50));atlas.paste(Image.fromarray(np.uint8(np.clip(paint,0,1)*255)),(0,0))
  palette=Image.open(ROOT/f'Assets/PhoViet/Art/Bikes/{name}_Palette.png').convert('RGB')
  # Palette occupies the whole bottom strip, preserving the UV grid and cloth colors.
  atlas.paste(palette.resize((2048,128),Image.Resampling.NEAREST),(0,1920));atlas.save(OUT/f'{name}_Palette.png')
  surface=Image.new('RGBA',(2048,2048),(0,0,0,80));surface.paste(Image.open(SOURCE/'meshy_metallic_smoothness.png').convert('RGBA').resize((2048,1920),Image.Resampling.LANCZOS),(0,0))
  surface.paste(Image.open(ROOT/f'Assets/PhoViet/Art/Bikes/{name}_Surface.png').convert('RGBA').resize((2048,128),Image.Resampling.NEAREST),(0,1920));surface.save(OUT/f'{name}_Surface.png')
 normal=Image.new('RGB',(2048,2048),(128,128,255));normal.paste(Image.open(SOURCE/'meshy_normal.png').convert('RGB').resize((2048,1920),Image.Resampling.LANCZOS),(0,0));normal.save(OUT/'Meshy_Normal.png')
 (OUT/'source.json').write_text(json.dumps({'source':str(SOURCE.relative_to(ROOT)),'triangles':15739,'paintColors':VARIANTS,'atlasSize':2048,'bikeUvY':[.0625,1],'riderUvY':[0,.0625],'sourceUnchanged':True},indent=2))
if __name__=='__main__':build()
