"""Reusable placement of the three actual Blender MCP pet meshes (author axes)."""
import json, math
from pathlib import Path
_PET_DATA = None
def append_pet(g, species, pos, yaw=0, scale=1, location='inside'):
    global _PET_DATA
    if _PET_DATA is None:
        _PET_DATA=json.loads(Path('C:/Users/HP/Documents/GameUnity/Taixecongnghe/Assets/PhoViet/Art/Pets/PV_PetGeometry.json').read_text(encoding='utf-8'))['models']
    mesh=_PET_DATA[species][g.d];a=math.radians(yaw);c,s=math.cos(a),math.sin(a)
    def rotate(p):return (c*p[0]+s*p[2],p[1],-s*p[0]+c*p[2])
    def point(p):return tuple(pos[i]+scale*v for i,v in enumerate(rotate(p)))
    start=len(g.v);g.v.extend(point(p) for p in mesh['v']);g.f.extend(tuple(start+i for i in face) for face in mesh['f'])
    g.cols.extend(mesh['cols']);g.tags.extend(mesh['tags'])
    g.hint.extend((kind,point(p) if kind=='c' else rotate(p)) for kind,p in mesh['hint'])
    if not hasattr(g,'pet_instances'):g.pet_instances=[]
    g.pet_instances.append(dict(species=species,position=list(pos),yaw=yaw,scale=scale,location=location))

def street_petshop_interior(g,spec):
    # Common room shell/tiled floor/ceiling lights, with no restaurant branches.
    plain=dict(spec,kind='petshop');_original_interior(g,plain)
    for species,x,z,yaw,scale in [('dog',-2.45,-2.45,18,.78),('cat',-1.15,-2.45,-18,.66),('rabbit',2.6,-2.25,12,.65),('dog',1.65,-1.25,-25,.72),('cat',-.9,-1.2,25,.62)]:
        append_pet(g,species,(x,.27,z),yaw,scale)
    # Flat sleeping mats and pet supplies; no tables, chairs or display cabinets.
    for x in (-2.4,0,2.4):g.cube((x,.257,-1.1),(1.15,.014,.75),'awnBlue' if x else 'pinkLight')
    for k in range(5):g.cube((-2.5+k*.48,.42,-.77),(.27,.32,.18),('blue','pink','white','goodsB','awnGreen')[k])

def street_petshop_sidewalk(g,spec,obstacles):
    for species,x,z,yaw,scale in [('dog',-2.35,-5.05,22,.85),('cat',-1.1,-5.05,-12,.68),('rabbit',1.8,-5.1,-20,.68),('rabbit',2.75,-5.02,24,.58)]:
        append_pet(g,species,(x,.22,z),yaw,scale,'outside')
    if g.d<2:
        for x in (-1.65,2.25):g.cyl((x,.22,-4.65),(x,.27,-4.65),.075,'white',n=6,r2=.1)

def river_petshop_animals(g,x0,x1,base,zf,f):
    cx=(x0+x1)*.5;span=(x1-x0)*.26;outward_yaw=180 if f>0 else 0
    for species,x,scale in [('dog',cx-span,.57),('cat',cx,.55),('rabbit',cx+span,.55)]:
        append_pet(g,species,(x,.22,zf+f*.65),outward_yaw,scale,'outside')
        # The narrow shopfront bay is separate from the glazed rear room.
        append_pet(g,species,(x,base+.2,zf-f*.37),outward_yaw,.43 if species=='dog' else .45,'inside-front')
        append_pet(g,species,(x,base+.2,zf-f*1.42),outward_yaw+180,.65 if species=='dog' else .58,'inside-river')
