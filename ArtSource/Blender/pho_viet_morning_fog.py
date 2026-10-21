"""Run in the user's Blender via MCP: a real volume and its mobile density atlas.
Creates a separate scene/library; never overwrites existing Blender projects.
"""
import bpy, math, json, os
from mathutils import Vector, noise
ROOT=r'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
OUT=ROOT+'/Assets/PhoViet/Art/Fog'
os.makedirs(OUT,exist_ok=True)
N=32;PAD=1;TILE=N+2;COLS=8;ROWS=4

def smooth(t):return t*t*(3-2*t)
def density(x,z,h):
    # Blend four translated fBm fields to make both horizontal axes periodic.
    a=smooth(x);b=smooth(z)
    def f(dx,dz):return noise.fractal(Vector((dx*4.7,dz*7.1,h*1.35+2.4)),1.0,2.0,4.0)
    n=(1-a)*(1-b)*f(x,z)+a*(1-b)*f(x-1,z)+(1-a)*b*f(x,z-1)+a*b*f(x-1,z-1)
    return max(.08,min(1,.57+n*.58))

def material(name,color):
    m=bpy.data.materials.new(name);m.use_nodes=True
    p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value=tuple((v/255)**2.2 for v in color)+(1,);p.inputs['Roughness'].default_value=.85
    return m
def cube(scene,name,position,size,mat):
    mesh=bpy.data.meshes.new(name)
    vertices=[(x/2,y/2,z/2) for z in [-1,1] for y in [-1,1] for x in [-1,1]]
    mesh.from_pydata(vertices,[],[(0,2,3,1),(4,5,7,6),(0,1,5,4),(2,6,7,3),(0,4,6,2),(1,3,7,5)])
    obj=bpy.data.objects.new(name,mesh);scene.collection.objects.link(obj);obj.location=position;obj.scale=size;obj.data.materials.append(mat);return obj
def mathnode(nodes,op,a=None,b=None):
    node=nodes.new('ShaderNodeMath');node.operation=op
    if a is not None:node.inputs[0].default_value=a
    if b is not None:node.inputs[1].default_value=b
    return node

def build():
    original=bpy.context.window.scene;scene=bpy.data.scenes.new('PV • Six second morning mist');bpy.context.window.scene=scene
    scene.render.engine=original.render.engine
    scene.render.resolution_x=640;scene.render.resolution_y=400;scene.render.resolution_percentage=100
    scene.render.image_settings.file_format=next(i.identifier for i in bpy.types.ImageFormatSettings.bl_rna.properties['file_format'].enum_items if i.identifier=='PNG')
    scene.render.film_transparent=False;scene.render.fps=30;scene.frame_start=1;scene.frame_end=181
    world=bpy.data.worlds.new('PV clear morning world');world.use_nodes=True;scene.world=world
    bg=next(n for n in world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs['Color'].default_value=(.62,.76,.9,1);bg.inputs['Strength'].default_value=.6
    grid=[[[density(x/N,z/N,h/(N-1)) for x in range(N)] for z in range(N)] for h in range(N)]
    width,height=COLS*TILE,ROWS*TILE;pixels=[0.0]*(width*height*4)
    for h in range(N):
        for z in range(TILE):
            for x in range(TILE):
                value=grid[h][(z-PAD)%N][(x-PAD)%N]
                i=(((h//COLS)*TILE+z)*width+(h%COLS)*TILE+x)*4
                pixels[i:i+4]=[value,value,value,1]
    image=bpy.data.images.new('PV Morning Fog • 32 cubed periodic density',width,height,alpha=True)
    image.colorspace_settings.name='Non-Color';image.pixels.foreach_set(pixels)
    image.filepath_raw=OUT+'/PV_MorningFogDensity.png';image.file_format=scene.render.image_settings.file_format;image.save()
    # The preview volume reads the same two adjacent atlas slices as the Unity shader.
    fog=bpy.data.materials.new('PV • depth and height morning mist');fog.use_nodes=True;nodes=fog.node_tree.nodes;nodes.clear();links=fog.node_tree.links
    out=nodes.new('ShaderNodeOutputMaterial');volume=nodes.new('ShaderNodeVolumePrincipled');links.new(volume.outputs['Volume'],out.inputs['Volume'])
    volume.inputs['Color'].default_value=(.82,.9,.94,1);volume.inputs['Anisotropy'].default_value=.3
    coord=nodes.new('ShaderNodeTexCoord');split=nodes.new('ShaderNodeSeparateXYZ');links.new(coord.outputs['Generated'],split.inputs[0])
    layer=mathnode(nodes,'MULTIPLY',b=N-1);links.new(split.outputs['Z'],layer.inputs[0]);base=mathnode(nodes,'FLOOR');links.new(layer.outputs[0],base.inputs[0])
    fraction=mathnode(nodes,'FRACT');links.new(layer.outputs[0],fraction.inputs[0]);textures=[]
    for offset in [0,1]:
        sl=mathnode(nodes,'ADD',b=offset);links.new(base.outputs[0],sl.inputs[0]);limit=mathnode(nodes,'MINIMUM',b=N-1);links.new(sl.outputs[0],limit.inputs[0])
        col=mathnode(nodes,'MODULO',b=COLS);links.new(limit.outputs[0],col.inputs[0]);row=mathnode(nodes,'DIVIDE',b=COLS);links.new(limit.outputs[0],row.inputs[0]);floor=mathnode(nodes,'FLOOR');links.new(row.outputs[0],floor.inputs[0])
        combine=nodes.new('ShaderNodeCombineXYZ')
        for axis,slot,size in [(col,'X',width),(floor,'Y',height)]:
            cell=mathnode(nodes,'MULTIPLY',b=TILE);links.new(axis.outputs[0],cell.inputs[0]);uv=mathnode(nodes,'MULTIPLY',b=N);links.new(split.outputs['X' if slot=='X' else 'Y'],uv.inputs[0])
            add=mathnode(nodes,'ADD');links.new(cell.outputs[0],add.inputs[0]);links.new(uv.outputs[0],add.inputs[1]);pad=mathnode(nodes,'ADD',b=1.5);links.new(add.outputs[0],pad.inputs[0]);scale=mathnode(nodes,'DIVIDE',b=size);links.new(pad.outputs[0],scale.inputs[0]);links.new(scale.outputs[0],combine.inputs[slot])
        tex=nodes.new('ShaderNodeTexImage');tex.image=image;links.new(combine.outputs[0],tex.inputs['Vector']);textures.append(tex)
    mix=nodes.new('ShaderNodeMixRGB');links.new(fraction.outputs[0],mix.inputs[0]);links.new(textures[0].outputs['Color'],mix.inputs[1]);links.new(textures[1].outputs['Color'],mix.inputs[2])
    exponent=mathnode(nodes,'MULTIPLY',b=-12*.24);links.new(split.outputs['Z'],exponent.inputs[0]);falloff=mathnode(nodes,'EXPONENT');links.new(exponent.outputs[0],falloff.inputs[0])
    mask=mathnode(nodes,'MULTIPLY');links.new(mix.outputs[0],mask.inputs[0]);links.new(falloff.outputs[0],mask.inputs[1]);strength=mathnode(nodes,'MULTIPLY',b=.065);links.new(mask.outputs[0],strength.inputs[0])
    fade=nodes.new('ShaderNodeValue');fade.label='Six gameplay seconds: mist dispersal';total=mathnode(nodes,'MULTIPLY');links.new(strength.outputs[0],total.inputs[0]);links.new(fade.outputs[0],total.inputs[1]);links.new(total.outputs[0],volume.inputs['Density'])
    for i in range(61):fade.outputs[0].default_value=1-smooth(i/60);fade.outputs[0].keyframe_insert('default_value',frame=1+i*3)
    cube(scene,'PV real participating fog volume',(0,50,6),(72,160,12),fog)
    road=material('PV study asphalt',(101,114,119));pavement=material('PV study paving',(192,177,151));paint=material('PV study road paint',(246,249,254))
    cube(scene,'Street',(0,50,-.15),(15,170,.3),road)
    for side in [-1,1]:cube(scene,'Footpath',(side*8.4,50,-.02),(1.8,170,.25),pavement)
    for y in range(-20,133,6):
        cube(scene,'Centre line',(0,y,.016),(.13,3,.018),paint)
        for x in [-3.1,3.1]:cube(scene,'Lane paint',(x,y,.016),(.1,2.2,.018),paint)
    source=ROOT+'/ArtSource/Blender/PhoViet_Streets.blend'
    with bpy.data.libraries.load(source,link=False) as (src,dst):dst.objects=[n for n in src.objects if n in ['PV_StreetShop00_LOD0','PV_StreetShop01_LOD0','PV_StreetShop02_LOD0','PV_StreetTree0_LOD0']]
    assets=[o for o in dst.objects if o is not None];shops=[o for o in assets if 'Shop' in o.name];tree=next(o for o in assets if 'Tree' in o.name)
    for side in [-1,1]:
        for i,y in enumerate(range(-6,130,8)):
            shop=shops[i%len(shops)].copy();shop.data=shops[i%len(shops)].data;shop.parent=None;scene.collection.objects.link(shop);shop.location=(side*12.8,y,0);shop.rotation_euler=(0,0,side*math.pi/2);shop.hide_render=False
            if i%2==0:
                t=tree.copy();t.parent=None;scene.collection.objects.link(t);t.location=(side*8.1,y+3,0);t.rotation_euler=(0,0,0);t.hide_render=False
    lamp=bpy.data.lights.new('PV white morning sun','SUN');lamp.energy=2.5;sun=bpy.data.objects.new(lamp.name,lamp);scene.collection.objects.link(sun);sun.rotation_euler=(.5,-.3,-.4)
    cam=bpy.data.objects.new('PV mist preview camera',bpy.data.cameras.new('PV mist preview camera'));scene.collection.objects.link(cam);scene.camera=cam;cam.location=(2,-14,7.1);cam.rotation_euler=(Vector((0,26,3))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=30
    profile={'authoredIn':'Blender MCP 5.2','durationSeconds':6,'density':.065,'heightFalloff':.24,'maximumDistance':100,'color':'E1ECF1','gridSize':32,'atlasColumns':8,'atlasRows':4,'tileSize':34,'period':[72,12,144],'fade':[round(1-smooth(i/60),6) for i in range(61)]}
    with open(OUT+'/PV_MorningFog.json','w',encoding='utf-8') as f:json.dump(profile,f,indent=2)
    image.pack();scene.frame_set(1)
    for obj in scene.objects:
        for slot in obj.material_slots:
            if slot.material is None or not slot.material.use_nodes:continue
            for node in slot.material.node_tree.nodes:
                if node.type=='TEX_IMAGE' and node.image and node.image.source=='FILE' and not node.image.packed_file:node.image.pack()
    bpy.data.libraries.write(ROOT+'/ArtSource/Blender/PhoViet_MorningFog.blend',{scene},fake_user=True)
    print(json.dumps({'success':True,'scene':scene.name,'atlas':[width,height],'samples':N**3,'duration':6,'densityMin':min(pixels[::4]),'densityMax':max(pixels[::4]),'originalScene':original.name}))

build()
