using System;using System.Linq;using System.Reflection;using System.IO;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class ProfileRuntimeCost
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 delegate bool Sweep(Vector3 a,Vector3 b,out Vector2 n,out float t,out string k);
 public static object Main()=>Run("before");
 public static object After()=>Run("after");
 static object Run(string label)
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;bool river=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var results=new List<object>();
  Action<string,Action,int> measure=(name,action,count)=>{for(int i=0;i<10;i++)action();var times=new List<double>();long alloc=0;for(int round=0;round<5;round++){long memory=GC.GetAllocatedBytesForCurrentThread();var watch=System.Diagnostics.Stopwatch.StartNew();for(int i=0;i<count;i++)action();watch.Stop();alloc+=GC.GetAllocatedBytesForCurrentThread()-memory;times.Add(watch.Elapsed.TotalMilliseconds/count);}times.Sort();results.Add(new{name,ms=times[2],allocatedPerCall=alloc/(double)(count*5)});};
  Action<string,int> cost=(name,count)=>{var method=typeof(PhoVietGame).GetMethod(name,F);var action=(Action)Delegate.CreateDelegate(typeof(Action),g,method);measure(name,action,count);};
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   cost("HideBuildingsOverRiver",100);cost("UpdateRiverMurk",1000);cost("AnimateLights",200);
   var sweep=(Sweep)Delegate.CreateDelegate(typeof(Sweep),g,typeof(PhoVietGame).GetMethod("SceneryContact",F));measure("scenery straight",()=>sweep(new Vector3(1.55f,0,0),new Vector3(1.55f,0,.5f),out _,out _,out _),200);
   var tick=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),g,typeof(PhoVietGame).GetMethod("TickRide",F));measure("TickRide zero step",()=>tick(0),60);
   foreach(float hour in new[]{0f,30f}){g.daylight.SetCycleTime(hour);measure("UpdateLamps "+hour,()=>g.daylight.UpdateLamps(),100);measure("SetCycleTime "+hour,()=>g.daylight.SetCycleTime(hour),100);}
   var bike=g.GetComponentsInChildren<BikeVisualAnimator>(true).First(b=>b.urbanMotion!=null&&b.role==BikeVisualAnimator.ActorRole.Parked);bike.Initialize();measure("parked bike pose",()=>bike.Advance(.016f),100);
   var renderers=g.GetComponentsInChildren<Renderer>(true);var report=new{success=true,label,results,renderers=renderers.Length,activeRenderers=renderers.Count(r=>r.gameObject.activeInHierarchy),lods=g.GetComponentsInChildren<LODGroup>(true).Length,lights=g.daylight.streets.Length,obstacleSets=g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).Length,quality=QualitySettings.GetQualityLevel(),aa=QualitySettings.antiAliasing,lodBias=QualitySettings.lodBias,shadows=QualitySettings.shadowDistance,screen=Screen.width+"x"+Screen.height};File.WriteAllText("Captures/runtime-cost-"+label+".json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=river;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
