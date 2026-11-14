"""Bake weathered plaster from PhoViet_Jail.blend into the game's packed wall map.

Run with Blender in background with the jail blend loaded. Output is staged in
.tools/wall-21234; the source scene and installed texture are never written.
"""

from pathlib import Path
import json
import bpy
import numpy as np

root = Path(__file__).resolve().parents[2]
stage = root / ".tools/wall-21234"
stage.mkdir(parents=True, exist_ok=True)
source = bpy.data.materials.get("Weathered lime plaster")
assert source is not None
source_nodes = source.node_tree.nodes
noise_layers = sorted((node for node in source_nodes if node.type=="TEX_NOISE"),
                      key=lambda node: node.inputs["Scale"].default_value)
assert len(noise_layers)==2
scene = bpy.data.scenes.new("Wall texture bake")
bpy.context.window.scene = scene
scene.render.engine = "CYCLES"
scene.cycles.samples = 1
scene.render.threads_mode="FIXED"
scene.render.threads=10

# Keep the jail noise in world metres. The overlap is blended before derivatives are packed.
size, overlap, repeat_metres = 1024, 256, 2.0
resolution = size + overlap
bpy.ops.mesh.primitive_plane_add(size=repeat_metres*resolution/size)
plane=bpy.context.object
mat = source.copy()
plane.data.materials.append(mat)
tree=mat.node_tree
noise_layers = sorted((node for node in tree.nodes if node.type=="TEX_NOISE"),
                      key=lambda node: node.inputs["Scale"].default_value)
output = next(node for node in tree.nodes if node.type=="OUTPUT_MATERIAL")
channels=tree.nodes.new("ShaderNodeCombineColor")
tree.links.new(noise_layers[0].outputs["Fac"],channels.inputs[0])
tree.links.new(noise_layers[1].outputs["Fac"],channels.inputs[1])
emit = tree.nodes.new("ShaderNodeEmission")
tree.links.new(channels.outputs[0],emit.inputs["Color"])
tree.links.new(emit.outputs[0], output.inputs["Surface"])
bake = bpy.data.images.new("Jail noise layers", resolution, resolution, float_buffer=True)
bake.colorspace_settings.name="Non-Color"
target=tree.nodes.new("ShaderNodeTexImage")
target.image=bake
tree.nodes.active=target
bpy.ops.object.bake(type="EMIT", use_clear=True, margin=0)
buffer=np.empty(resolution*resolution*4,np.float32)
bake.pixels.foreach_get(buffer)
sample=buffer.reshape(resolution,resolution,4)[:,:,:2]

def wrap_axis(arr):
    out=arr[:size].copy()
    x=np.linspace(0,1,overlap,dtype=np.float32)
    mix=(x*x*(3-2*x))[:,None,None]
    out[:overlap]=arr[size:]* (1-mix) + out[:overlap]*mix
    return out

seamless = wrap_axis(wrap_axis(sample).transpose((1,0,2))).transpose((1,0,2))
tone, fine = seamless[:,:,0], seamless[:,:,1]
dx=(np.roll(fine,-1,axis=1)-np.roll(fine,1,axis=1))*size/(2*repeat_metres)
dy=(np.roll(fine,-1,axis=0)-np.roll(fine,1,axis=0))*size/(2*repeat_metres)
# Encode daylight-visible relief with the same fine noise and deeper pores.
relief_metres=.014
packed=np.empty((size,size,4),np.float32)
packed[:,:,0]=np.clip(.5+(tone-tone.mean())*.95+(fine-fine.mean())*.24,0,1)
packed[:,:,1]=.5-.5*np.clip(dy*relief_metres,-.98,.98)
packed[:,:,2]=.5
packed[:,:,3]=.5-.5*np.clip(dx*relief_metres,-.98,.98)
result=bpy.data.images.new("Wall Stucco Detail",size,size,alpha=True)
result.colorspace_settings.name="Non-Color"
result.pixels.foreach_set(packed.ravel())
result.filepath_raw=str(stage/"PV_WallStuccoDetail.png")
result.file_format="PNG"
result.save()
report=dict(noiseScales=[n.inputs["Scale"].default_value for n in noise_layers],
            repeatMetres=repeat_metres,resolution=size,encodedDepth=relief_metres,
            signalStd=[float(packed[:,:,i].std()) for i in (0,1,3)])
(stage/"bake.json").write_text(json.dumps(report,indent=2),encoding="utf-8")
print(report)
