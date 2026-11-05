using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEditor;using UnityEngine;
// 21.136: release dependencies that cost memory on phones: big textures and meshes kept readable on the CPU.
public static class ListHeavyAssets2136
{
 static string[] Deps()
 {
  var paths=new List<string>{"Assets/PhoViet/Scenes/PhoViet.unity"};
  foreach(var p in new[]{"Assets/PhoViet/Resources/Helicopter/PV_Helicopter.prefab","Assets/PhoViet/Resources/Helicopter/PV_HelicopterMissile.prefab","Assets/PhoViet/Resources/PlayerMissile/PV_MissilePickup.prefab","Assets/PhoViet/Resources/Energy/PV_EnergyPickup.prefab"})if(File.Exists(p))paths.Add(p);
  return paths.SelectMany(p=>AssetDatabase.GetDependencies(p,true)).Distinct().ToArray();
 }
 public static object Main()
 {
  var deps=Deps();
  var tex=deps.Select(p=>new{p,i=AssetImporter.GetAtPath(p) as TextureImporter,t=AssetDatabase.LoadAssetAtPath<Texture2D>(p)}).Where(x=>x.i!=null&&x.t!=null).Select(x=>new{x.p,w=x.t.width,h=x.t.height,fmt=x.i.GetPlatformTextureSettings("WebGL").format.ToString(),max=x.i.GetPlatformTextureSettings("WebGL").maxTextureSize,readable=x.i.isReadable,normal=x.i.textureType==TextureImporterType.NormalMap}).OrderByDescending(x=>x.w*x.h).ToArray();
  var models=deps.Select(p=>new{p,i=AssetImporter.GetAtPath(p) as ModelImporter}).Where(x=>x.i!=null).Select(x=>new{x.p,readable=x.i.isReadable,mb=Math.Round(new FileInfo(x.p).Length/1048576.0,2)}).OrderByDescending(x=>x.mb).ToArray();
  var other=deps.Where(p=>p.EndsWith(".asset")||p.EndsWith(".mesh")).Select(p=>AssetDatabase.LoadAssetAtPath<Mesh>(p)).Where(m=>m!=null).Select(m=>new{m.name,m.isReadable,m.vertexCount}).ToArray();
  var report=new{textures=tex.Length,big=tex.Where(t=>t.w>=1024).ToArray(),models=models.Length,readableModels=models.Count(m=>m.readable),modelList=models,other};
  File.WriteAllText("Captures/perf2136-assets.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
  return new{report.textures,big=report.big.Length,report.models,report.readableModels,other=other.Length};
 }
}
