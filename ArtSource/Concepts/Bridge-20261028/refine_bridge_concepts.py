"""Final study refinements, safe to run again on these study scenes only."""
import bpy, bmesh, math
from mathutils import Vector
from math import pi,sin
for o in bpy.data.objects:
    if o.name.startswith('PV85_') and o.type=='MESH':
        bm=bmesh.new();bm.from_mesh(o.data)
        if len(bm.faces)>1:bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data)
        bm.free()
if not any(o.name.startswith('PV85_A_ArchSpandrel') for o in bpy.data.objects):
    coll=bpy.data.collections['PV85_A_BRIDGE_ONLY'];root=bpy.data.objects['PV85_A_Bridge']
    for side in (-1,1):
        x=side*3.35
        for span in range(3):
            y0=-25.2+16.8*span;verts=[];n=32
            for i in range(n+1):
                t=i/n;y=y0+16.8*t;z=-4.35+3.4*sin(pi*t)
                verts.extend([(x-.27,y,z),(x+.27,y,z),(x-.27,y,-.64),(x+.27,y,-.64)])
            faces=[(0,2,3,1),(4*n,4*n+1,4*n+3,4*n+2)]
            for i in range(n):
                a=i*4;b=a+4;faces.extend([(a,b,b+1,a+1),(a+2,a+3,b+3,b+2),(a,a+2,b+2,b),(a+1,b+1,b+3,a+3)])
            me=bpy.data.meshes.new('PV85_A_ArchSpandrel');me.from_pydata(verts,[],faces);me.update()
            bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
            ob=bpy.data.objects.new('PV85_A_ArchSpandrel',me);coll.objects.link(ob);ob.parent=root;me.materials.append(bpy.data.materials['PV85_ivory'])
            mod=ob.modifiers.new('Masonry edge softness',next(e.identifier for e in bpy.types.Modifier.bl_rna.properties['type'].enum_items if e.identifier=='BEVEL'));mod.width=.03;mod.segments=2
for scene in bpy.data.scenes:
    if not scene.name.startswith('PV85_'):continue
    for ob in scene.objects:
        if ob.type=='CAMERA' and ob.get('view_role')=='overview':
            ob.location=(42,-60,25);ob.rotation_euler=(Vector((0,0,.6))-ob.location).to_track_quat('-Z','Y').to_euler();ob.data.ortho_scale=57
        if ob.type=='LIGHT' and ob.data.type=='SUN':ob.data.energy=3.2
m=bpy.data.materials['PV85_asphalt']
next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'].default_value=(.035,.05,.06,1)
