using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using PhoViet;
public static class ProfileStreetDetail
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var cam=g.rideCamera;var old=cam.targetTexture;var active=RenderTexture.active;float time=Time.timeScale;var rt=new RenderTexture(1100,700,24){antiAliasing=4};var tex=new Texture2D(1100,700,TextureFormat.RGB24,false);var pixel=new Texture2D(1,1,TextureFormat.RGB24,false);
  var entries=new List<(LODGroup group,LOD[] levels)>();var samples=new Dictionary<string,List<double>>();
  try
  {
   Time.timeScale=0;g.ResetRide();g.daylight.SetCycleTime(8);cam.targetTexture=rt;
   foreach(var lod in g.scenery.GetComponentsInChildren<LODGroup>(true))
   {
    var levels=lod.GetLODs();if(levels.Length<2||!levels[0].renderers.Any(r=>r!=null&&(r.name.StartsWith("PV_StreetShop")||r.name.StartsWith("PV_StreetTree"))))continue;
    entries.Add((lod,levels));
   }
   foreach(var n in new[]{"original","optimized"})samples[n]=new List<double>();
   for(int round=0;round<4;round++)foreach(string mode in samples.Keys.ToArray())
   {
    foreach(var e in entries){var levels=(LOD[])e.levels.Clone();levels[0].screenRelativeTransitionHeight=mode=="original"?.18f:.45f;levels[1].screenRelativeTransitionHeight=mode=="original"?.07f:.175f;e.group.SetLODs(levels);}
    for(int i=0;i<10;i++){var w=System.Diagnostics.Stopwatch.StartNew();cam.Render();RenderTexture.active=rt;pixel.ReadPixels(new Rect(0,0,1,1),0,0);pixel.Apply();w.Stop();if(i>=3)samples[mode].Add(w.Elapsed.TotalMilliseconds);}
    if(round==0){tex.ReadPixels(new Rect(0,0,1100,700),0,0);tex.Apply();File.WriteAllBytes("Captures/perf-street-"+mode.Replace(' ','-')+".png",tex.EncodeToPNG());}
   }
   var report=new{success=true,groups=entries.Count,note="Same frozen scene; camera render plus synchronous 1px readback, not gameplay FPS",results=samples.Select(p=>{p.Value.Sort();return new{name=p.Key,ms=p.Value[p.Value.Count/2]};}).ToArray()};File.WriteAllText("Captures/street-lod-controlled.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{foreach(var e in entries)e.group.SetLODs(e.levels);cam.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(pixel);g.ResetRide();Time.timeScale=time;}
 }
}
