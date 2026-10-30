from pathlib import Path
from PIL import Image
import numpy as np,json
before=np.asarray(Image.open('Captures/crater-road-before.png').convert('RGB')).astype(float)
baseline=np.asarray(Image.open('Captures/crater-road-standard-baseline.png').convert('RGB')).astype(float)
after=np.asarray(Image.open('Captures/crater-road-after.png').convert('RGB')).astype(float)
r=json.loads(Path('Captures/crater-render-regions.json').read_text())
corners=np.asarray(r['exclusion']);x0,y0=np.floor(corners.min(axis=0)).astype(int);x1,y1=np.ceil(corners.max(axis=0)).astype(int)
outside=np.ones(before.shape[:2],dtype=bool);outside[max(y0,0):min(y1,900),max(x0,0):min(x1,1200)]=False
cx,cy=np.rint(r['centre']).astype(int);patch=(slice(cy-8,cy+9),slice(cx-8,cx+9))
metrics=dict(noHoleMeanPixelError=float(np.abs(before-baseline).mean()),outsideHoleMeanPixelError=float(np.abs(after-before)[outside].mean()),outsidePixels=int(outside.sum()),floorLuminanceBefore=float(before[patch].mean()),floorLuminanceAfter=float(after[patch].mean()))
checks=[]
def check(text,ok):
 assert ok,(text,metrics)
 checks.append(text)
check('No-hole road preserves original Standard appearance',metrics['noHoleMeanPixelError']<.75)
check('Comparison includes a meaningful untouched region',metrics['outsidePixels']>40000)
check('Only crater footprint and rim shadow alter the scene',metrics['outsideHoleMeanPixelError']<.75)
check('Cutout reveals the dark depressed floor rather than city base',metrics['floorLuminanceBefore']-metrics['floorLuminanceAfter']>18)
report=dict(success=True,count=len(checks),checks=checks,metrics=metrics)
Path('Captures/crater-render-checks.json').write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report))
