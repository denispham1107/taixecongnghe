"""Isolated Blue Petshop Supercar conversion; source FBX/other assets stay intact."""
from pathlib import Path
import shutil
ROOT=Path(__file__).resolve().parents[2]
__name__='supercar_library'
library=(ROOT/'ArtSource/Blender/integrate_meshy_cars.py').read_text(encoding='utf-8')
library=library.replace("mesh=source.data;raw=", "optimize_source(source);mesh=source.data;raw=")
exec(compile(library,'integrate_meshy_cars.py','exec'))
ROOT=Path(__file__).resolve().parents[2]
OUT=str(ROOT/'Assets/PhoViet/Art/PlayerCar');NAME='PV_BlueSupercar'
Path(OUT).mkdir(parents=True,exist_ok=True)
def optimize_source(source):
    bpy.context.view_layer.objects.active=source;source.select_set(True)
    source.data.calc_loop_triangles();original=len(source.data.loop_triangles)
    dec=source.modifiers.new('PlayerMobileBudget','DECIMATE')
    dec.ratio=min(1,28000/original);dec.use_collapse_triangulate=True
    bpy.ops.object.modifier_apply(modifier=dec.name)
    print('MOBILE_BUDGET',original,len(source.data.polygons),flush=True)
# Measured from the sidewall projection. RANSAC over this AI mesh mistakes the
# wheel arches for tyres, so bind the small inner rims around the real axles.
def axle_fit(points,interval,radii):
    lo=points.min(axis=0);hi=points.max(axis=0);scale=4.4/(hi[0]-lo[0]);mid=(hi[0]+lo[0])/2
    z=1.20 if interval[0]<0 else -1.32
    return np.array([mid-z/scale,lo[2]+.315/scale]),.315/scale
config=(NAME,'Blue Petshop Supercar_20261027_004421',4.4,[(-.78,-.25),(.27,.78)],(.09,.19))
report=build(config)
folder=ROOT/'Assets/MeshyImports'/config[1]
for old,new in [('meshy_basecolor','Palette'),('meshy_normal','Normal'),('meshy_metallic_smoothness','Surface')]:
    shutil.copyfile(folder/(old+'.png'),Path(OUT)/(NAME+'_'+new+'.png'))
report['player']=True
report['sourceTriangles']=1016646
report['headlight']=[0,.46,2.04];report['taillight']=[.73,.62,-2.10]
report['liftNozzles']=[[-.77,.17,1.35],[.77,.17,1.35],[-.77,.17,-1.45],[.77,.17,-1.45]]
report['boostNozzles']=[[-.73,.20,-2.13],[.73,.20,-2.13]]
def decorate_source():
    """Keep the saved Blender handoff textured, with visible nozzle helpers."""
    rig,obj,_=MODELS[NAME];mat=obj.data.materials[0];mat.use_nodes=True
    nodes=mat.node_tree.nodes;links=mat.node_tree.links;bsdf=nodes.get('Principled BSDF')
    if not any(n.type=='TEX_IMAGE' for n in nodes):
        tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(Path(OUT)/(NAME+'_Palette.png')),check_existing=True)
        links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
    bsdf.inputs['Roughness'].default_value=.45
    for group,key in [('Lift','liftNozzles'),('Exhaust','boostNozzles')]:
        for i,p in enumerate(report[key]):
            name=group+'Socket'+str(i)
            if bpy.data.objects.get(name):continue
            helper=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(helper);helper.parent=rig;helper.location=B(p);helper.empty_display_type='ARROWS';helper.empty_display_size=.15
    for image in bpy.data.images:
        if image.users and not image.packed_file:
            try:image.pack()
            except RuntimeError:pass
decorate_source()
(Path(OUT)/(NAME+'.json')).write_text(json.dumps(report,indent=2),encoding='utf-8')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Blender/PhoViet_BlueSupercar.blend'))
(ROOT/'Captures/supercar-export.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('SUPERCAR_EXPORT',json.dumps({k:report[k] for k in ('size','lod_triangles','axles')}))
