using System;using System.Linq;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
// 21.136: records the shader variants the ride really uses into Resources/PV_ShaderWarmup.shadervariants.
public static class TrackShaderVariants2136
{
 const string Path_="Assets/PhoViet/Resources/PV_ShaderWarmup.shadervariants";
 static readonly Type U=typeof(ShaderUtil);const BindingFlags S=BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public;
 static PhoVietGame g;static int step;static float next;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  U.GetMethod("ClearCurrentShaderVariantCollection",S).Invoke(null,null);step=0;next=0;g.forceTouchGraphics=true;
  // Every pooled hazard and effect, in front of the camera, so its pass and keywords get drawn.
  EditorApplication.update-=Tick;EditorApplication.update+=Tick;return "started";
 }
 static void ShowPools()
 {
  var cam=g.rideCamera.transform;int n=0;
  foreach(Transform child in g.transform)
  {
   string name=child.name;if(name.Contains("thành phố")||child==g.rider||child==g.rideCamera.transform)continue;
   child.gameObject.SetActive(true);foreach(var r in child.GetComponentsInChildren<Renderer>(true))r.enabled=true;foreach(var p in child.GetComponentsInChildren<ParticleSystem>(true)){p.gameObject.SetActive(true);p.Play();p.Emit(4);}
   child.position=cam.position+cam.forward*(8+(n%6)*2)+cam.right*((n%5)-2)*1.5f;n++;
  }
 }
 static void Tick()
 {
  if(!Application.isPlaying){EditorApplication.update-=Tick;return;}
  if(Time.realtimeSinceStartup<next)return;next=Time.realtimeSinceStartup+2.2f;
  int[] levels={0,1,2};float[] hours={8,26};int total=levels.Length*hours.Length;
  if(step<total)
  {
   g.SetGraphicsQuality(levels[step%3]);g.RestartRide();g.daylight.SetCycleTime(hours[step/3]);ShowPools();step++;return;
  }
  EditorApplication.update-=Tick;
  U.GetMethod("SaveCurrentShaderVariantCollection",S).Invoke(null,new object[]{Path_});
  var c=(int)U.GetMethod("GetCurrentShaderVariantCollectionVariantCount",S).Invoke(null,null);
  File.WriteAllText("Captures/perf2136-variants.json","{\"variants\":"+c+"}");
 }
}
