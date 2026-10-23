"""Retain received 2K PBR maps; mask paint and actual lamp faces separately."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import numpy as np,json,shutil
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'Assets/PhoViet/Art/Cars/Meshy'
report=[]
for meta in sorted(OUT.glob('*.json')):
 if meta.stem.endswith('_Roof'):continue
 info=json.loads(meta.read_text(encoding='utf-8'));name=info['name'];source=ROOT/info['source'];folder=source.parent
 info['taillight'][1]=info['size'][1]*(.43 if 'Pickup' in name else .37 if 'Sport' in name else .18)
 meta.write_text(json.dumps(info,indent=2),encoding='utf-8')
 for old,new in [('meshy_basecolor','Palette'),('meshy_normal','Normal'),('meshy_metallic_smoothness','Surface')]:shutil.copyfile(folder/(old+'.png'),OUT/(name+'_'+new+'.png'))
 image=Image.open(folder/'meshy_basecolor.png').convert('RGB');size=image.size;pixels=np.asarray(image).astype(np.float32)/255
 r,g,b=[pixels[:,:,i] for i in range(3)];value=pixels.max(axis=2);sat=(value-pixels.min(axis=2))/np.maximum(value,.001)
 if 'Sport' in name:paint=(g>r*1.12)&(b>r*1.12)&(sat>.17)
 elif 'Pickup' in name:paint=(r>b*1.3)&(g>b*1.15)&(r>g*.8)&(g>r*.52)&(sat>.18)
 elif 'Covered' in name:paint=(sat<.20)&(value>.50)
 else:paint=(sat<.65)&(value>.035)
 paint_area=Image.new('L',size);front=Image.new('L',size);rear=Image.new('L',size)
 brushes=[ImageDraw.Draw(i) for i in (paint_area,front,rear)]
 wheel_pixels=np.zeros((size[1],size[0]),dtype=bool)
 regions=json.loads((ROOT/f'.tools/{name}-uvregions.json').read_text(encoding='utf-8'))
 length=info['size'][2];height=info['size'][1]
 for face in regions:
  x,y,z=face['center'];uv=[(u*(size[0]-1),(1-v)*(size[1]-1)) for u,v in face['uv']]
  # Exclude tyres, lower frame, glass and bright roof trim from neutral truck paint.
  body=True if 'Sport' in name or 'Pickup' in name else y>height*.26 and z>length*.18
  if 'Covered' in name and height*.47<y<height*.80 and z>length*.23:body=False
  if 'Container' in name:
   body=y>height*.38 and z<length*.28 or height*.26<y<height*.53 and z>=length*.28 and (abs(x)>info['size'][0]*.32 or z<length*.43)
  # Resolve fused fender/tyre triangles per texel. Whole-face exclusion leaves
  # original-colour triangular patches in the painted wheel arch.
  if any((y-a['y'])**2+(z-a['z'])**2<(a['radius']*1.5)**2 for a in info['axles']):
   xyz=np.array(face['vertices']);coords=np.array(uv)
   for tri in range(1,len(coords)-1):
    aa,bb,cc=coords[0],coords[tri],coords[tri+1];p0,p1,p2=xyz[0],xyz[tri],xyz[tri+1]
    low=np.maximum(0,np.floor(np.minimum(np.minimum(aa,bb),cc)).astype(int));high=np.minimum(np.array(size)-1,np.ceil(np.maximum(np.maximum(aa,bb),cc)).astype(int))
    if np.any(high<low):continue
    xx,yy=np.meshgrid(np.arange(low[0],high[0]+1),np.arange(low[1],high[1]+1));d=bb-aa;e=cc-aa;det=d[0]*e[1]-d[1]*e[0]
    if abs(det)<1e-8:continue
    px,py=xx-aa[0],yy-aa[1];u=(px*e[1]-py*e[0])/det;v=(d[0]*py-d[1]*px)/det;inside=(u>=0)&(v>=0)&(u+v<=1)
    p=p0+u[...,None]*(p1-p0)+v[...,None]*(p2-p0);wheel=np.zeros_like(inside)
    for a in info['axles']:wheel|=((p[:,:,1]-a['y'])**2+(p[:,:,2]-a['z'])**2<(a['radius']*1.01)**2)&(np.abs(p[:,:,0])>a['x']*.65)
    wheel_pixels[yy,xx]|=inside&wheel
  if body:brushes[0].polygon(uv,fill=255)
  if z>length*.36 and abs(y-info['headlight'][1])<height*.065 and info['size'][0]*.20<abs(x)<info['size'][0]*.49:brushes[1].polygon(uv,fill=255)
  if z<-length*.40 and abs(y-info['taillight'][1])<height*.075 and abs(x)>info['size'][0]*.20:brushes[2].polygon(uv,fill=255)
 paint&=np.asarray(paint_area)>0
 paint&=~wheel_pixels
 mask=Image.fromarray(np.uint8(paint)*255).filter(ImageFilter.MaxFilter(3)).filter(ImageFilter.GaussianBlur(.5));mask.save(OUT/(name+'_PaintMask.png'))
 emission=np.zeros((size[1],size[0],3),dtype=np.uint8)
 head=(np.asarray(front)>0)&(value>.58)&(sat<.24);tail=(np.asarray(rear)>0)&(r>g*1.3)&(r>b*1.25)&(r>.22)
 emission[head]=[185,217,255];emission[tail]=[255,6,3];Image.fromarray(emission).save(OUT/(name+'_Emission.png'))
 report.append(dict(asset=name,size=size,paintPixels=int(paint.sum()),headPixels=int(head.sum()),tailPixels=int(tail.sum()),sourceMapsCopiedUnchanged=True))
(ROOT/'Captures/meshy-cars-texture-checks.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))
