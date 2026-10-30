"""14 additional rental plaques and a seamless plaster map. Run only in a fresh task Blender."""
import bpy,json,hashlib,subprocess
import numpy as np
from pathlib import Path
ROOT=Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe')
EXPECTED={12:[1,5],13:[2,6],14:[3,5],15:[2],16:[1],18:[1,3],19:[3,5],20:[1,3]}

def old_text(path):return subprocess.check_output(['git','show','1122572:'+path],cwd=ROOT).decode('utf8')

def detail_map():
    n=1024;rng=np.random.default_rng(2197)
    f=np.fft.fftfreq(n)*n;xx,yy=np.meshgrid(f,f);radius=xx*xx+yy*yy
    def layer(cut):
        x=np.fft.ifft2(np.fft.fft2(rng.standard_normal((n,n)))*np.exp(-radius/(2*cut*cut))).real
        return x/max(x.std(),1e-8)
    height=layer(180)*.62+layer(38)*.27+layer(5)*.11
    dx=(np.roll(height,-1,1)-np.roll(height,1,1))*.5
    dy=(np.roll(height,-1,0)-np.roll(height,1,0))*.5
    scale=max(np.quantile(np.sqrt(dx*dx+dy*dy),.995),1e-5)
    pix=np.empty((n,n,4),dtype=np.float32)
    pix[:,:,0]=.5+np.clip(height/3,-1,1)*.18
    pix[:,:,1]=.5+np.clip(dy/scale,-1,1)*.22
    pix[:,:,2]=.5
    pix[:,:,3]=.5+np.clip(dx/scale,-1,1)*.22
    image=bpy.data.images.new('PV_WallStuccoDetail',width=n,height=n,alpha=True)
    image.colorspace_settings.name='Non-Color';image.pixels.foreach_set(pix.ravel())
    image.filepath_raw=str(ROOT/'Assets/PhoViet/Art/Streets/PV_WallStuccoDetail.png');image.file_format='PNG';image.save();image.pack()
    return dict(size=n,channels='R height / G normalY / B unused / A normalX',mean=[float(x) for x in pix.mean(axis=(0,1))],minimum=float(pix.min()),maximum=float(pix.max()),periodic=True,seed=2197)

def main():
    assert bpy.data.filepath=='' and len(bpy.data.objects)<=3,'Fresh task scene required'
    base={'__name__':'fixture_export_library'}
    exec(compile((ROOT/'ArtSource/Blender/upgrade_fixture_details.py').read_text(encoding='utf8'),'fixture_library.py','exec'),base)
    ns=base['library'](river=True);base['materials'](ns);parts=ns['apartment_parts']()
    old=base['library'](river=True);exec(compile(old_text('ArtSource/Blender/pho_viet_fixture_details.py'),'baseline_fixture.py','exec'),old)
    previous=old['apartment_parts']();fresh=ns['rental_sign'];ns['rental_sign']=old['rental_sign'];reference=ns['apartment_parts']();ns['rental_sign']=fresh
    checks=[]
    def check(label,ok):
        if not ok:raise RuntimeError(label)
        checks.append(label)
    count=0
    for i,((name,gs),(before,oldgs),(ref,refs)) in enumerate(zip(parts,previous,reference)):
        check(name+' no changes outside new plaques',name==before==ref and all(base['digest'](a)==base['digest'](b) for a,b in zip(oldgs,refs)))
        extra=EXPECTED.get(i,[])
        for d,(a,b) in enumerate(zip(oldgs,gs)):
            x,y=a.rental_signs,b.rental_signs
            check(name+' LOD'+str(d)+' retains original plaque exactly',y[0]==x[0])
            check(name+' LOD'+str(d)+' exact marked floors',len(y)==1+len(extra) and [s['floor'] for s in y[1:]]==extra)
            check(name+' LOD'+str(d)+' preserves width font height facing',all(all(s[k]==x[0][k] for k in ('label','width','height','font_size','facing')) for s in y))
        count+=len(gs[0].rental_signs)-1
    check('Exactly fourteen additional plaques in the marked positions',count==14)
    for o in list(bpy.data.objects):bpy.data.objects.remove(o,do_unlink=True)
    bpy.context.scene.name='PV_RentalWalls_2197';coll=bpy.data.collections.new('PV_RentalWalls_2197');bpy.context.scene.collection.children.link(coll)
    out=ROOT/'.tools/rental-walls-export';out.mkdir(exist_ok=True);ns['OUT']=str(out)
    info=ns['export_parts']('PV_RiverApartments',parts,coll,(0,60,0),ns['river_materials']())
    detail=detail_map()
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_RentalWalls2197.blend'))
    result=dict(success=True,count=len(checks),checks=checks,newPlaques=count,totalPlaques=sum(len(g[0].rental_signs) for _,g in parts),floors=EXPECTED,detail=detail,triangles=info['lod_triangles'])
    (ROOT/'Captures/rental-walls-blender-checks.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf8')
    print(json.dumps({k:v for k,v in result.items() if k!='checks'}))
