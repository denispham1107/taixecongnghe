using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class ProfileRenderCost
{
 public static object Main()=>Run("before");public static object After()=>Run("after");
 static object Run(string label)
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play required");float time=Time.timeScale;var cam=g.rideCamera;var previous=cam.targetTexture;var rt=new RenderTexture(960,540,24){antiAliasing=4};var tex=new Texture2D(960,540,TextureFormat.RGB24,false);var pixel=new Texture2D(1,1,TextureFormat.RGB24,false);var active=RenderTexture.active;
  var materials=g.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Distinct().ToArray();var flags=materials.Select(m=>m.enableInstancing).ToArray();var samples=new Dictionary<string,List<double>>();
  var oldFog=RenderSettings.fog;var mirror=cam.GetComponent<RiverReflection>();bool reflected=mirror.enabled;
  try
  {
   Time.timeScale=0;g.ResetRide();g.daylight.SetCycleTime(8);RenderSettings.fog=false;cam.targetTexture=rt;
   foreach(string name in new[]{"baseline","instancing","reflection off"})samples[name]=new List<double>();
   for(int round=0;round<4;round++)foreach(string name in samples.Keys.ToArray())
   {
    for(int i=0;i<materials.Length;i++)materials[i].enableInstancing=name=="instancing"||flags[i];mirror.enabled=name!="reflection off"&&reflected;
    for(int i=0;i<9;i++)
    {
     var w=System.Diagnostics.Stopwatch.StartNew();cam.Render();RenderTexture.active=rt;pixel.ReadPixels(new Rect(0,0,1,1),0,0);pixel.Apply();w.Stop();if(i>=3)samples[name].Add(w.Elapsed.TotalMilliseconds);
    }
    if(round==0&&name!="reflection off"){tex.ReadPixels(new Rect(0,0,960,540),0,0);tex.Apply();File.WriteAllBytes("Captures/perf-"+label+"-"+name+".png",tex.EncodeToPNG());}
   }
   var results=samples.Select(p=>{p.Value.Sort();return new{name=p.Key,ms=p.Value[p.Value.Count/2]};}).ToArray();var report=new{success=true,label,results,materials=materials.Length,instancingInitially=flags.Count(x=>x),shaders=materials.GroupBy(m=>m.shader.name).Select(a=>new{name=a.Key,count=a.Count()}).ToArray(),screen="960x540",aa=4};File.WriteAllText("Captures/render-cost-"+label+".json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{for(int i=0;i<materials.Length;i++)materials[i].enableInstancing=flags[i];mirror.enabled=reflected;cam.targetTexture=previous;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(pixel);RenderSettings.fog=oldFog;g.ResetRide();Time.timeScale=time;}
 }
}
