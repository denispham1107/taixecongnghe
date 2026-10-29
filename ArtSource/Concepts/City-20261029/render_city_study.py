"""Render real Blender approval images through a timer, keeping MCP responsive."""
import bpy,json,time,os
from pathlib import Path
OUT=Path(r'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/City-20261029')
META=json.loads((OUT/'study.json').read_text(encoding='utf8'))
JOBS=META['renders'].copy();DONE=[];START=time.time()
def city_render_next():
    if not JOBS:return None
    job=JOBS.pop(0);sc=bpy.data.scenes[job['scene']];sc.camera=bpy.data.objects[job['camera']]
    sc.render.resolution_x,sc.render.resolution_y=job['size'];sc.render.resolution_percentage=100;sc.eevee.taa_render_samples=32
    sc.render.filepath=str(OUT/(job['name']+'.png'))
    state={'status':'rendering','current':job['name'],'completed':DONE.copy(),'remaining':len(JOBS)}
    (OUT/'render-progress.json').write_text(json.dumps(state,indent=2),encoding='utf8')
    try:
        bpy.ops.render.render(write_still=True,scene=sc.name)
        DONE.append({'name':job['name'],'path':sc.render.filepath,'size':job['size'],'bytes':os.path.getsize(sc.render.filepath)})
        state={'status':'completed' if not JOBS else 'between_renders','completed':DONE.copy(),'remaining':len(JOBS),'seconds':round(time.time()-START,2)}
        if not JOBS:
            sc=bpy.data.scenes['PV86_Street_Preview'];sc.camera=bpy.data.objects['PV86_Street_Wide'];sc.render.resolution_x=1920;sc.render.resolution_y=1080
            bpy.context.window.scene=sc
            bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'PhoViet_NewCity_Approval.blend'),copy=True)
        (OUT/'render-progress.json').write_text(json.dumps(state,indent=2),encoding='utf8')
    except Exception as e:
        import traceback
        (OUT/'render-progress.json').write_text(json.dumps({'status':'failed','error':str(e),'traceback':traceback.format_exc(),'completed':DONE},indent=2),encoding='utf8');return None
    return 1.0 if JOBS else None
bpy.app.timers.register(city_render_next,first_interval=1)
print('Queued %d Blender approval renders'%len(JOBS))
