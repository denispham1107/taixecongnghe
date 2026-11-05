using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using Unity.Profiling;using PhoViet;
// 21.136: A/B on one frozen frame with the WebGL mobile baseline; what each switch saves on the same image.
public static class AbFrozen2136
{
 static PhoVietGame g;static Queue<(string name,Action on,Action off)> plan;static (string name,Action on,Action off) cur;static int wait;static List<object> rows=new List<object>();static List<double> dts=new List<double>();
 static ProfilerRecorder tris,verts,setp,shadow,dyn,inst,render;static float time;static float baseLod,baseFar;static Camera cam;static object breakdown;
 static void Base(){QualitySettings.SetQualityLevel(3,true);QualitySettings.lodBias=1;QualitySettings.shadowDistance=45;QualitySettings.shadowResolution=ShadowResolution.Low;QualitySettings.shadowCascades=0;QualitySettings.antiAliasing=0;QualitySettings.shadows=ShadowQuality.All;QualitySettings.pixelLightCount=2;}
 static object Break()
 {
  var map=new Dictionary<string,long[]>();
  foreach(var r in g.GetComponentsInChildren<Renderer>(true)){if(!r.isVisible||!r.enabled)continue;Mesh m=null;if(r is SkinnedMeshRenderer s)m=s.sharedMesh;else{var f=r.GetComponent<MeshFilter>();if(f!=null)m=f.sharedMesh;}if(m==null)continue;string n=m.name;n=System.Text.RegularExpressions.Regex.Replace(n,@"(\d+)?(_LOD\d|_Body)?$","");int lod=m.name.Contains("LOD1")?1:m.name.Contains("LOD2")?2:0;if(!map.TryGetValue(n,out var v))map[n]=v=new long[5];v[0]++;v[1]+=m.triangles.Length/3;v[2+lod]++;}
  return map.OrderByDescending(p=>p.Value[1]).Take(22).Select(p=>new{p.Key,count=p.Value[0],tris=p.Value[1],lod0=p.Value[2],lod1=p.Value[3],lod2=p.Value[4]}).ToArray();
 }
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();cam=g.rideCamera;Base();
  var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;typeof(PhoVietGame).GetField("graphicsInitialized",f).SetValue(g,false);g.GetType().GetMethod("InitializeGraphicsSettings",f).Invoke(g,null);g.SetGraphicsQuality(0);
  g.RestartRide();g.daylight.SetCycleTime(30);time=Time.timeScale;Time.timeScale=0;baseLod=1;baseFar=cam.farClipPlane;
  tris=ProfilerRecorder.StartNew(new ProfilerCategory("Render"),"Triangles Count",1);verts=ProfilerRecorder.StartNew(new ProfilerCategory("Render"),"Vertices Count",1);setp=ProfilerRecorder.StartNew(new ProfilerCategory("Render"),"SetPass Calls Count",1);shadow=ProfilerRecorder.StartNew(new ProfilerCategory("Render"),"Shadow Casters Count",1);dyn=ProfilerRecorder.StartNew(new ProfilerCategory("Render"),"Dynamic Batched Draw Calls Count",1);inst=ProfilerRecorder.StartNew(new ProfilerCategory("Render"),"Instanced Batched Draw Calls Count",1);render=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Camera.Render",1);
  var refl=cam.GetComponent<RiverReflection>();var fog=cam.GetComponent<MorningFog>();
  plan=new Queue<(string,Action,Action)>();Action none=()=>{};
  void P(string n,Action a,Action b)=>plan.Enqueue((n,a,b));
  P("base-Cao",none,none);
  P("noPixelLights",()=>QualitySettings.pixelLightCount=0,()=>QualitySettings.pixelLightCount=2);
  P("pixelLights1",()=>QualitySettings.pixelLightCount=1,()=>QualitySettings.pixelLightCount=2);
  P("noShadow",()=>QualitySettings.shadowDistance=0,()=>QualitySettings.shadowDistance=45);
  P("noReflection",()=>refl.enabled=false,()=>refl.enabled=true);
  P("noDepthTex",()=>cam.depthTextureMode=DepthTextureMode.None,()=>cam.depthTextureMode=DepthTextureMode.None);
  P("lodBias0.5",()=>QualitySettings.lodBias=.5f,()=>QualitySettings.lodBias=1);
  P("lodBias0.25",()=>QualitySettings.lodBias=.25f,()=>QualitySettings.lodBias=1);
  P("maxLOD1",()=>QualitySettings.maximumLODLevel=1,()=>QualitySettings.maximumLODLevel=0);
  P("maxLOD2",()=>QualitySettings.maximumLODLevel=2,()=>QualitySettings.maximumLODLevel=0);
  P("far120",()=>cam.farClipPlane=120,()=>cam.farClipPlane=baseFar);
  P("far90",()=>cam.farClipPlane=90,()=>cam.farClipPlane=baseFar);
  P("sortNoDistance",()=>cam.opaqueSortMode=UnityEngine.Rendering.OpaqueSortMode.NoDistanceSort,()=>cam.opaqueSortMode=UnityEngine.Rendering.OpaqueSortMode.Default);
  P("base-again",none,none);
  P("Thap-now",()=>g.SetGraphicsQuality(2),()=>g.SetGraphicsQuality(0));
  P("Trung-now",()=>g.SetGraphicsQuality(1),()=>g.SetGraphicsQuality(0));
  rows.Clear();cur=plan.Dequeue();wait=0;dts.Clear();UnityEditor.EditorApplication.update-=Tick;UnityEditor.EditorApplication.update+=Tick;return "started";
 }
 static int last=-1;
 static void Tick()
 {
  if(!Application.isPlaying){Finish(false);return;}if(Time.frameCount==last)return;last=Time.frameCount;
  if(wait==0)cur.on();wait++;
  if(wait==8&&breakdown==null)breakdown=Break();
  if(wait>8)dts.Add(render.LastValue/1e6);
  if(wait>=26)
  {
   dts.Sort();rows.Add(new{mode=cur.name,tris=tris.LastValue,verts=verts.LastValue,setPass=setp.LastValue,shadowCasters=shadow.LastValue,dyn=dyn.LastValue,inst=inst.LastValue,cameraMs=Math.Round(dts[dts.Count/2],2)});
   cur.off();wait=0;dts.Clear();if(plan.Count==0){Finish(true);return;}cur=plan.Dequeue();
  }
 }
 static void Finish(bool write)
 {
  UnityEditor.EditorApplication.update-=Tick;
  if(write)File.WriteAllText("Captures/perf2136-ab.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{device=SystemInfo.graphicsDeviceName,rows,breakdown},Newtonsoft.Json.Formatting.Indented));
  tris.Dispose();verts.Dispose();setp.Dispose();shadow.Dispose();dyn.Dispose();inst.Dispose();render.Dispose();Time.timeScale=time;
 }
}
