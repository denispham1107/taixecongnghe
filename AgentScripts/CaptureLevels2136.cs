using System;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
// 21.136: the same frame at each graphics level, phone-sized, by day and night, to judge what each level looks like.
public static class CaptureLevels2136
{
 static PhoVietGame g;static int step;static int wait;static float time;
 static readonly (int level,string name,float hour,float ride)[] Shots={(0,"cao-ngay",8,0),(1,"trung-ngay",8,0),(2,"thap-ngay",8,0),(0,"cao-dem",26,0),(1,"trung-dem",26,0),(2,"thap-dem",26,0)};
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  QualitySettings.SetQualityLevel(3,true);QualitySettings.lodBias=1;QualitySettings.shadowDistance=45;QualitySettings.shadowResolution=ShadowResolution.Low;QualitySettings.shadowCascades=0;QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.All;QualitySettings.pixelLightCount=2;
  var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;g.forceTouchGraphics=true;typeof(PhoVietGame).GetField("graphicsInitialized",f).SetValue(g,false);g.GetType().GetMethod("InitializeGraphicsSettings",f).Invoke(g,null);
  time=Time.timeScale;step=0;wait=0;Directory.CreateDirectory("Captures");EditorApplication.update-=Tick;EditorApplication.update+=Tick;return "started";
 }
 static int last=-1;
 static void Tick()
 {
  if(!Application.isPlaying){EditorApplication.update-=Tick;return;}if(Time.frameCount==last)return;last=Time.frameCount;
  if(wait==0){var s=Shots[step];g.SetGraphicsQuality(s.level);g.ResetRide();g.StartRide();g.daylight.SetCycleTime(s.hour);Time.timeScale=0;}
  if(++wait<8)return;
  var shot=Shots[step];int cap=shot.level==0?2:shot.level==1?2:1;float dpr=shot.level==0?2f:shot.level==1?1.6f:1.25f;int w=Mathf.RoundToInt(844*dpr),h=Mathf.RoundToInt(390*dpr);
  var rt=new RenderTexture(w,h,24){antiAliasing=Math.Max(1,QualitySettings.antiAliasing)};var old=g.rideCamera.targetTexture;g.rideCamera.targetTexture=rt;g.rideCamera.Render();g.rideCamera.targetTexture=old;
  var active=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(w,h,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();RenderTexture.active=active;
  File.WriteAllBytes("Captures/perf2136-level-"+shot.name+".png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
  wait=0;if(++step>=Shots.Length){EditorApplication.update-=Tick;Time.timeScale=time;g.SetGraphicsQuality(0);g.forceTouchGraphics=false;g.ResetRide();}
 }
}
