"""Queue the six comparison renders in the live Blender MCP session."""
import bpy, json, os, time
OUT=r'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Concepts/Bridge-20261028'
META=json.load(open(os.path.join(OUT,'concepts.json'),encoding='utf-8'))
JOBS=[(d,v) for d in META['studies'] for v in ('day','street')]
DONE=[]
START=time.time()
def render_next_study():
    if not JOBS:return None
    d,v=JOBS.pop(0);scene=bpy.data.scenes[d['scene']]
    path=os.path.join(OUT,d['code']+'-'+v+'.png')
    state={'status':'rendering','current':d['code']+'-'+v,'completed':DONE.copy(),'remaining':len(JOBS)}
    with open(os.path.join(OUT,'render-progress.json'),'w') as f:json.dump(state,f)
    try:
        role='overview' if v=='day' else 'street'
        scene.camera=next(o for o in scene.objects if o.type=='CAMERA' and o.get('view_role')==role)
        scene.render.filepath=path
        bpy.ops.render.render(write_still=True,scene=scene.name)
        DONE.append({'frame':d['code']+'-'+v,'path':path,'bytes':os.path.getsize(path)})
        for obj in scene.objects:
            if obj.type=='CAMERA' and obj.get('view_role')=='overview':scene.camera=obj;break
        state={'status':'completed' if not JOBS else 'between_renders','completed':DONE.copy(),'remaining':len(JOBS),'seconds':round(time.time()-START,2)}
        if not JOBS:
            for study in META['studies']:
                root=bpy.data.objects[study['root']];meshes=[o for o in root.children_recursive if o.type=='MESH']
                study['mesh_objects']=len(meshes);study['base_triangles']=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)
            with open(os.path.join(OUT,'concepts.json'),'w',encoding='utf-8') as f:json.dump(META,f,indent=2)
            bpy.context.window.scene=bpy.data.scenes[META['studies'][0]['scene']]
            bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'PhoViet_Bridge_Concepts.blend'),copy=True)
        with open(os.path.join(OUT,'render-progress.json'),'w') as f:json.dump(state,f,indent=2)
    except Exception as e:
        with open(os.path.join(OUT,'render-progress.json'),'w') as f:json.dump({'status':'failed','error':str(e),'completed':DONE},f,indent=2)
        return None
    return 1.0 if JOBS else None
bpy.app.timers.register(render_next_study,first_interval=1.0)
print('Queued six real Blender renders (same overview and approach cameras).')
