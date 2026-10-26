"""Repaint only the scooter's original paint texels; preserve rider, stars and black trim."""
from pathlib import Path
from PIL import Image
import numpy as np,json,hashlib
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/PhoViet/Art/Bikes/MeshyRiders'
source=ROOT/'Assets/MeshyImports/Red Honda Scooter Front View_20261023_023822/meshy_basecolor.png'
original=np.asarray(Image.open(source).convert('RGB')).astype(np.float32)/255
mask=(original[:,:,0]>original[:,:,1]*1.3)&(original[:,:,0]>original[:,:,2]*1.3)&(original[:,:,0]-original[:,:,1]>.08)
hero=np.array(Image.open(OUT/'PV_HeroScooter_Palette.png').convert('RGB'))
assert hero.shape==(4096,2048,3)
result=hero.copy();bike=result[:2048];shade=original.max(2)
blue=np.array([.035,.30,.90],dtype=np.float32)
bike[mask]=np.uint8(np.clip(shade[mask,None]*blue*255,0,255))
assert np.array_equal(result[2048:],hero[2048:]),'Rider and stars changed'
assert np.array_equal(result[:2048][~mask],hero[:2048][~mask]),'Black trim or mechanical parts changed'
path=OUT/'PV_HeroScooter_BlueBlack_Palette.png';Image.fromarray(result).save(path)
report=dict(success=True,size=[2048,4096],paintPixels=int(mask.sum()),colour='blue-black',blue=list(map(float,blue)),riderAndStarsUnchanged=True,blackTrimUnchanged=True,sourceAtlasUnchanged=True,texture=str(path.relative_to(ROOT)))
(ROOT/'Captures/hero-blue-black-texture-checks.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))
