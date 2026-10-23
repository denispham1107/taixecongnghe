"""Project a five-point vector star onto the received shirt's back UVs.
Run with Blender in an isolated background process; source FBX remains untouched.
"""
import sys,json,math
from pathlib import Path
import numpy as np
import bpy
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).parent))
import integrate_meshy_riders as rider
p,faces,uv,triangles,colours=rider.source()
size=2048;mask=np.zeros((size,size,4),dtype=np.float32);mask[:,:,3]=1
star=[]
for i in range(10):
 angle=math.pi/2+i*math.pi/5;radius=.125 if i%2==0 else .125*.381966
 star.append((math.cos(angle)*radius,1.18+math.sin(angle)*radius))
star=np.array(star)
def inside(x,y):
 result=np.zeros(x.shape,bool)
 for a,b in zip(star,np.roll(star,1,axis=0)):
  result^=((a[1]>y)!=(b[1]>y))&(x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1]+1e-20)+a[0])
 return result
for ids,coords in zip(faces,uv):
 xyz=p[ids]
 if xyz[:,1].max()<1.055 or xyz[:,1].min()>1.305 or xyz[:,2].min()>-.02:continue
 coords=np.array(coords)*(size-1)
 for k in range(1,len(coords)-1):
  a,b,c=coords[0],coords[k],coords[k+1];pa,pb,pc=xyz[0],xyz[k],xyz[k+1]
  lo=np.maximum(0,np.floor(np.minimum(np.minimum(a,b),c)).astype(int));hi=np.minimum(size-1,np.ceil(np.maximum(np.maximum(a,b),c)).astype(int))
  xx,yy=np.meshgrid(np.arange(lo[0],hi[0]+1),np.arange(lo[1],hi[1]+1));dx=xx-a[0];dy=yy-a[1];ab=b-a;ac=c-a;det=ab[0]*ac[1]-ab[1]*ac[0]
  if abs(det)<1e-8:continue
  u=(dx*ac[1]-dy*ac[0])/det;v=(ab[0]*dy-ab[1]*dx)/det
  q=pa+u[:,:,None]*(pb-pa)+v[:,:,None]*(pc-pa)
  good=(u>=0)&(v>=0)&(u+v<=1)&(q[:,:,2]<-.02)&inside(q[:,:,0],q[:,:,1])
  mask[yy[good],xx[good],:3]=1
out=ROOT/'ArtSource/Blender/MeshyHero_BackStarMask.png'
im=bpy.data.images.new('BackStarMask',width=size,height=size,alpha=True);im.pixels.foreach_set(mask.ravel());im.filepath_raw=str(out);im.file_format='PNG';im.save()
count=int((mask[:,:,0]>.5).sum());assert count>500,'Missing back shirt UVs'
(ROOT/'Captures/meshy-back-star-mask.json').write_text(json.dumps({'success':True,'centre':[0,1.18],'outerRadius':.125,'tips':5,'sourceMaskPixels':count,'sourceUntouched':True},indent=2))
print('BACK_STAR_PIXELS',count)
