using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;

public static class AuditWebMemory
{
 public static object Main()
 {
  const string scene="Assets/PhoViet/Scenes/PhoViet.unity";
  var dependencies=AssetDatabase.GetDependencies(scene,true);
  var textures=dependencies.Where(p=>AssetImporter.GetAtPath(p) is TextureImporter).Select(p=>{
   var t=AssetDatabase.LoadAssetAtPath<Texture2D>(p);var i=(TextureImporter)AssetImporter.GetAtPath(p);var w=i.GetPlatformTextureSettings("WebGL");
   return new{path=p,width=t.width,height=t.height,format=t.format.ToString(),bytes=Profiler.GetRuntimeMemorySizeLong(t),readable=t.isReadable,mips=t.mipmapCount,normal=i.textureType==TextureImporterType.NormalMap,maxSize=i.maxTextureSize,compression=i.textureCompression.ToString(),webOverride=w.overridden,webFormat=w.format.ToString()};
  }).OrderByDescending(t=>t.bytes).ToArray();
  var meshes=dependencies.SelectMany(p=>AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>().Select(m=>new{path=p,name=m.name,bytes=Profiler.GetRuntimeMemorySizeLong(m),vertices=m.vertexCount,readable=m.isReadable})).OrderByDescending(m=>m.bytes).ToArray();
  var report=new{textureBytes=textures.Sum(t=>t.bytes),meshBytes=meshes.Sum(m=>m.bytes),textures,meshes,totalAllocated=Profiler.GetTotalAllocatedMemoryLong(),totalReserved=Profiler.GetTotalReservedMemoryLong(),mono=Profiler.GetMonoUsedSizeLong()};
  File.WriteAllText("Captures/web-memory-asset-audit.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
  return new{report.textureBytes,report.meshBytes,textures=textures.Take(14),meshes=meshes.Take(8),subtarget=EditorUserBuildSettings.webGLBuildSubtarget.ToString()};
 }
}
