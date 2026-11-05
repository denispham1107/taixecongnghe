using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using Unity.Profiling;using PhoViet;
// 21.136: sample real frames while riding at a graphics level, with the WebGL mobile quality baseline. Play Mode only.
public static class SampleFrames2136
{
 static readonly string[] Timers={"PlayerLoop","Update.ScriptRunBehaviourUpdate","PreLateUpdate.ScriptRunBehaviourLateUpdate","PostLateUpdate.UpdateAllRenderers","PostLateUpdate.UpdateAllSkinnedMeshes","PostLateUpdate.FinishFrameRendering","Camera.Render","Culling","SceneCulling","Render.OpaqueGeometry","Render.TransparentGeometry","Shadows.RenderShadowMap","Shadows.PrepareShadowmap","RenderForward.RenderLoopJob","Camera.ImageEffects","Camera.RenderSkybox","Depth","Render.DepthTexture","UpdateDepthTexture","MeshSkinning.Update","GUI.Repaint","GUIUtility.ProcessEvent","ParticleSystem.Update","ParticleSystem.Draw","TransformChangedDispatch","PhysicsFixedUpdate","FixedUpdate.PhysicsFixedUpdate","Audio","GC.Collect"};
 static readonly (string,string)[] Counters={("Render","Draw Calls Count"),("Render","SetPass Calls Count"),("Render","Batches Count"),("Render","Triangles Count"),("Render","Vertices Count"),("Render","Shadow Casters Count"),("Render","Visible Skinned Meshes Count"),("Render","Dynamic Batched Draw Calls Count"),("Render","Static Batched Draw Calls Count"),("Render","Instanced Batched Draw Calls Count"),("Memory","GC Allocated In Frame"),("Memory","GC Allocation In Frame Count")};
 static ProfilerRecorder[] timers,counters;static List<long>[] timerData,counterData;static List<float> dts;static int frames,target;static string label;static PhoVietGame game;static int restarts;
 static void Web(PhoVietGame g,int level)
 {
  g.forceTouchGraphics=true;
  QualitySettings.SetQualityLevel(3,true);QualitySettings.lodBias=1;QualitySettings.shadowDistance=45;QualitySettings.shadowResolution=ShadowResolution.Low;QualitySettings.shadowCascades=0;QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.All;QualitySettings.pixelLightCount=2;
  var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
  typeof(PhoVietGame).GetField("graphicsInitialized",f).SetValue(g,false);g.GetType().GetMethod("InitializeGraphicsSettings",f).Invoke(g,null);g.SetGraphicsQuality(level);
 }
 static void Begin(int level,string name,int count,float hour)
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();Web(game,level);game.RestartRide();game.daylight.SetCycleTime(hour);
  label=name;target=count;frames=-90;restarts=0;
  timers=Timers.Select(n=>ProfilerRecorder.StartNew(ProfilerCategory.Internal,n,1)).ToArray();counters=Counters.Select(c=>ProfilerRecorder.StartNew(new ProfilerCategory(c.Item1),c.Item2,1)).ToArray();
  timerData=Timers.Select(_=>new List<long>()).ToArray();counterData=Counters.Select(_=>new List<long>()).ToArray();dts=new List<float>();
  UnityEditor.EditorApplication.update-=Tick;UnityEditor.EditorApplication.update+=Tick;
 }
 static int lastFrame=-1,skip;
 static void Tick()
 {
  if(!Application.isPlaying){Stop(false);return;}if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;
  if(game.State!=PhoVietGame.RideState.Riding){game.RestartRide();restarts++;skip=15;return;}
  if(skip>0){skip--;return;}
  if(frames++<0)return;
  for(int i=0;i<timers.Length;i++)timerData[i].Add(timers[i].LastValue);for(int i=0;i<counters.Length;i++)counterData[i].Add(counters[i].LastValue);dts.Add(Time.unscaledDeltaTime*1000);
  if(frames>=target)Stop(true);
 }
 static double Pct(List<long> v,double p){var s=v.OrderBy(x=>x).ToList();return s.Count==0?0:s[Math.Min(s.Count-1,(int)(p*s.Count))];}
 static void Stop(bool write)
 {
  UnityEditor.EditorApplication.update-=Tick;
  if(write)
  {
   var t=Timers.Select((n,i)=>new{n,ms=Math.Round(timerData[i].Average()/1e6,3),p95=Math.Round(Pct(timerData[i],.95)/1e6,2)}).Where(x=>x.ms>0.0005).OrderByDescending(x=>x.ms).ToArray();
   var c=Counters.Select((n,i)=>new{n=n.Item2,avg=Math.Round(counterData[i].Average()),max=counterData[i].Max()}).ToArray();
   var d=dts.OrderBy(x=>x).ToList();
   var report=new{label,frames=d.Count,restarts,dtAvg=Math.Round(d.Average(),2),dtP95=Math.Round(d[(int)(d.Count*.95)],2),dtMax=Math.Round(d.Last(),2),quality=new{QualitySettings.lodBias,QualitySettings.shadowDistance,QualitySettings.antiAliasing,QualitySettings.pixelLightCount},depth=game.rideCamera.depthTextureMode.ToString(),t,c,gcTotalMB=Math.Round(GC.GetTotalMemory(false)/1048576.0,1)};
   File.WriteAllText("Captures/perf2136-sample-"+label+".json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
  }
  foreach(var r in timers)r.Dispose();foreach(var r in counters)r.Dispose();
 }
 public static object Cao(){Begin(0,"cao-ngay",420,8);return "started";}
 public static object Trung(){Begin(1,"trung-ngay",420,8);return "started";}
 public static object Thap(){Begin(2,"thap-ngay",420,8);return "started";}
 public static object ThapDem(){Begin(2,"thap-dem",420,26);return "started";}
}
