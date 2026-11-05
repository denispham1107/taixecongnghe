using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using Unity.Profiling;using PhoViet;
// 21.136: audit of what each graphics level actually renders, using the WebGL-mobile quality baseline
// (quality level 3 "High", shadow 45 m/Low, AA as chosen by ConfigureBrowserGraphics). Play Mode only.
public static class ProfileMobileAudit2136
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  var all=g.GetComponentsInChildren<Renderer>(true);
  var skinned=g.GetComponentsInChildren<SkinnedMeshRenderer>(true);
  var materials=new HashSet<Material>();var shaders=new Dictionary<string,int>();
  foreach(var r in all)foreach(var m in r.sharedMaterials)if(m!=null&&materials.Add(m)){string n=m.shader!=null?m.shader.name:"null";shaders[n]=shaders.TryGetValue(n,out int c)?c+1:1;}
  var textures=new HashSet<Texture>();long textureBytes=0;var bigTextures=new List<object>();
  foreach(var m in materials)foreach(var id in m.GetTexturePropertyNameIDs()){var t=m.GetTexture(id);if(t!=null&&textures.Add(t)){long b=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t);textureBytes+=b;if(t.width>=1024)bigTextures.Add(new{t.name,t.width,t.height,mb=Math.Round(b/1048576.0,2)});}}
  var meshes=new Dictionary<Mesh,int>();foreach(var f in g.GetComponentsInChildren<MeshFilter>(true))if(f.sharedMesh!=null)meshes[f.sharedMesh]=meshes.TryGetValue(f.sharedMesh,out int c)?c+1:1;
  var biggest=meshes.OrderByDescending(p=>(long)p.Key.vertexCount*p.Value).Take(15).Select(p=>new{p.Key.name,verts=p.Key.vertexCount,tris=p.Key.triangles.Length/3,refs=p.Value}).ToArray();
  var skinInfo=skinned.GroupBy(s=>s.sharedMesh!=null?s.sharedMesh.name:"null").Select(x=>new{mesh=x.Key,count=x.Count(),verts=x.First().sharedMesh!=null?x.First().sharedMesh.vertexCount:0,bones=x.First().bones.Length,active=x.Count(s=>s.gameObject.activeInHierarchy)}).OrderByDescending(x=>x.verts*x.active).Take(20).ToArray();
  var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(l=>new{l.name,type=l.type.ToString(),active=l.isActiveAndEnabled,shadows=l.shadows.ToString(),mode=l.renderMode.ToString()}).ToArray();
  var scripts=g.GetComponentsInChildren<MonoBehaviour>(true).GroupBy(b=>b.GetType().Name).Select(x=>new{type=x.Key,count=x.Count(),enabled=x.Count(b=>b.isActiveAndEnabled)}).OrderByDescending(x=>x.count).Take(15).ToArray();
  var particles=g.GetComponentsInChildren<ParticleSystem>(true);
  var report=new
  {
   renderers=all.Length,activeRenderers=all.Count(r=>r.gameObject.activeInHierarchy&&r.enabled),
   lodGroups=g.GetComponentsInChildren<LODGroup>(true).Length,
   materialCount=materials.Count,shaders=shaders.OrderByDescending(p=>p.Value).Select(p=>p.Key+":"+p.Value).ToArray(),
   textureCount=textures.Count,textureMB=Math.Round(textureBytes/1048576.0,1),bigTextures,
   biggest,skinnedCount=skinned.Length,skinInfo,lights,scripts,particleSystems=particles.Length,activeParticleSystems=particles.Count(p=>p.gameObject.activeInHierarchy),
   instancingMaterials=materials.Count(m=>m.enableInstancing),
   srpBatcher=GraphicsSettings.useScriptableRenderPipelineBatching,
   camera=new{g.rideCamera.farClipPlane,g.rideCamera.renderingPath,g.rideCamera.useOcclusionCulling,g.rideCamera.allowMSAA,g.rideCamera.depthTextureMode,effects=g.rideCamera.GetComponents<MonoBehaviour>().Select(b=>b.GetType().Name).ToArray()}
  };
  File.WriteAllText("Captures/perf2136-inventory.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
  return report;
 }

 // Emulates the three levels on the same fixed scene and returns real renderer statistics.
 public static object Levels()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var cam=g.rideCamera;var oldTarget=cam.targetTexture;var active=RenderTexture.active;float time=Time.timeScale;int quality=QualitySettings.GetQualityLevel();int chosen=(int)g.SelectedGraphics;
  var rt=new RenderTexture(1170,540,24);var pixel=new Texture2D(1,1,TextureFormat.RGB24,false);var result=new List<object>();
  try
  {
   Time.timeScale=0;
   // Web mobile baseline, as ConfigureBrowserGraphics would produce from quality level 3.
   QualitySettings.SetQualityLevel(3,true);QualitySettings.shadowDistance=45;QualitySettings.shadowResolution=ShadowResolution.Low;QualitySettings.shadowCascades=0;QualitySettings.antiAliasing=4;
   typeof(PhoVietGame).GetField("graphicsInitialized",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(g,false);
   g.GetType().GetMethod("InitializeGraphicsSettings",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(g,null);
   foreach(float hour in new[]{8f,26f})foreach(int level in new[]{0,1,2})
   {
    g.SetGraphicsQuality(level);g.ResetRide();g.daylight.SetCycleTime(hour);cam.targetTexture=rt;
    var times=new List<double>();int draws=0,batches=0,setPass=0,tris=0,verts=0,shadow=0;
    for(int i=0;i<16;i++)
    {
     var w=System.Diagnostics.Stopwatch.StartNew();cam.Render();RenderTexture.active=rt;pixel.ReadPixels(new Rect(0,0,1,1),0,0);pixel.Apply();w.Stop();
     if(i>5)times.Add(w.Elapsed.TotalMilliseconds);
    }
    times.Sort();
#if UNITY_EDITOR
    draws=UnityEditor.UnityStats.drawCalls;batches=UnityEditor.UnityStats.renderTextureChanges;setPass=UnityEditor.UnityStats.setPassCalls;tris=UnityEditor.UnityStats.triangles;verts=UnityEditor.UnityStats.vertices;shadow=UnityEditor.UnityStats.shadowCasters;
#endif
    result.Add(new{level=level==0?"Cao":level==1?"TrungBinh":"Thap",hour,ms=Math.Round(times[times.Count/2],2),draws,batches,setPass,tris,verts,shadow,aa=QualitySettings.antiAliasing,shadowDistance=QualitySettings.shadowDistance,lodBias=QualitySettings.lodBias,pixelLights=QualitySettings.pixelLightCount});
   }
   var report=new{device=SystemInfo.graphicsDeviceName,result};File.WriteAllText("Captures/perf2136-levels.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{cam.targetTexture=oldTarget;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(pixel);QualitySettings.SetQualityLevel(quality,true);g.ResetRide();Time.timeScale=time;}
 }
}
