using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;

// 21.279: kiểm kê texture/mesh của scene bản đồ mở (gom theo thư mục) để tìm phần tăng bộ nhớ 1.75 → 1.76.
public static class AuditOpenWorldMemory
{
 static string Dir(string p){var parts=p.Split('/');return string.Join("/",parts.Take(Math.Min(parts.Length-1,5)));}
 public static object Main()
 {
  const string scene="Assets/PhoViet/Scenes/PhoVietOpenWorld.unity";
  var deps=AssetDatabase.GetDependencies(scene,true);
  var tex=deps.Where(p=>AssetImporter.GetAtPath(p) is TextureImporter||p.EndsWith(".exr")||p.EndsWith(".hdr")).Select(p=>{
   var t=AssetDatabase.LoadAssetAtPath<Texture>(p);var i=AssetImporter.GetAtPath(p) as TextureImporter;
   var w=i?.GetPlatformTextureSettings("WebGL");
   return new{path=p,w=t?t.width:0,h=t?t.height:0,type=t?t.GetType().Name:"",fmt=t is Texture2D t2?t2.format.ToString():t is Cubemap c?c.format.ToString():"",bytes=t?Profiler.GetRuntimeMemorySizeLong(t):0,readable=t&&t.isReadable,max=i?.maxTextureSize??0,webMax=w!=null&&w.overridden?w.maxTextureSize:0};
  }).OrderByDescending(t=>t.bytes).ToArray();
  var meshes=deps.SelectMany(p=>AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>().Select(m=>new{path=p,name=m.name,bytes=Profiler.GetRuntimeMemorySizeLong(m),v=m.vertexCount,readable=m.isReadable})).OrderByDescending(m=>m.bytes).ToArray();
  var texDirs=tex.GroupBy(t=>Dir(t.path)).Select(g=>new{dir=g.Key,n=g.Count(),mb=Math.Round(g.Sum(x=>x.bytes)/1048576.0,1)}).OrderByDescending(g=>g.mb).Take(25).ToArray();
  var meshDirs=meshes.GroupBy(m=>Dir(m.path)).Select(g=>new{dir=g.Key,n=g.Count(),mb=Math.Round(g.Sum(x=>x.bytes)/1048576.0,1),readableMb=Math.Round(g.Where(x=>x.readable).Sum(x=>x.bytes)/1048576.0,1)}).OrderByDescending(g=>g.mb).Take(25).ToArray();
  var report=new{texMb=Math.Round(tex.Sum(t=>t.bytes)/1048576.0,1),meshMb=Math.Round(meshes.Sum(m=>m.bytes)/1048576.0,1),texDirs,meshDirs,topTex=tex.Take(30),topMesh=meshes.Take(30)};
  File.WriteAllText("Captures/openworld-memory-audit-21279.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{report,tex,meshes},Newtonsoft.Json.Formatting.Indented));
  return new{report.texMb,report.meshMb,texDirs,meshDirs,topTex=tex.Take(15).Select(t=>t.path.Replace("Assets/PhoViet/","")+" "+t.w+"x"+t.h+" "+t.fmt+" "+Math.Round(t.bytes/1048576.0,1)+"MB"+(t.readable?" RW":""))};
 }
}
