"""Read received car models in a separate Blender process, preserving live scenes."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
folders=sorted(p for p in (ROOT/'Assets/MeshyImports').iterdir() if p.is_dir() and '20261024_022' in p.name)
reports=[]
for idx,folder in enumerate(folders):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    path=next(folder.glob('*.fbx'));bpy.ops.import_scene.fbx(filepath=str(path))
    objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
    coords=[o.matrix_world@v.co for o in objects for v in o.data.vertices]
    lo=Vector(tuple(min(v[i] for v in coords) for i in range(3)));hi=Vector(tuple(max(v[i] for v in coords) for i in range(3)));center=(lo+hi)/2;size=hi-lo
    report=dict(folder=folder.name,file=path.name,bounds=[list(lo),list(hi)],size=list(size),objects=[dict(name=o.name,vertices=len(o.data.vertices),faces=len(o.data.polygons),triangles=sum(len(p.vertices)-2 for p in o.data.polygons),materials=[m.name for m in o.data.materials],uvs=len(o.data.uv_layers)) for o in objects]);reports.append(report)
    mat=bpy.data.materials.new('Inspection'+str(idx));mat.use_nodes=True
    nodes=mat.node_tree.nodes;shader=nodes.get('Principled BSDF');shader.inputs['Roughness'].default_value=.55
    texture=nodes.new('ShaderNodeTexImage');texture.image=bpy.data.images.load(str(folder/'meshy_basecolor.png'));mat.node_tree.links.new(texture.outputs['Color'],shader.inputs['Base Color'])
    for o in objects:o.data.materials.clear();o.data.materials.append(mat)
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True
    scene.render.resolution_x=640;scene.render.resolution_y=480;scene.render.resolution_percentage=100
    scene.world.color=(.25,.25,.25);scene.view_settings.view_transform='Standard'
    bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.type='ORTHO';cam.data.ortho_scale=max(size)*1.5;cam.data.lens=45
    bpy.ops.object.light_add(type='AREA',location=center+Vector((0,-max(size),max(size)*1.5)));bpy.context.object.data.energy=1800*max(size)**2;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=max(size)*2
    for view,axis in [('front34',Vector((1,-1,.65))),('rear34',Vector((-1,1,.65)))]:
        cam.location=center+axis.normalized()*max(size)*2.5;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(ROOT/f'Captures/meshy-car-source-{idx}-{view}.png');bpy.ops.render.render(write_still=True)
(ROOT/'Captures/meshy-cars-source-inspection.json').write_text(json.dumps(reports,indent=2),encoding='utf-8')
print('CAR_SOURCES',json.dumps(reports),flush=True)
