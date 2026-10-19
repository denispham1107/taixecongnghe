using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureCompactHud
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static float scale;static bool jobs,running,waiting;static int phase;static double next;
 static readonly string[] names={"pickup","delivery","idle-long-values"};
 static object Call(string name,params object[] values)=>typeof(PhoVietGame).GetMethod(name,F).Invoke(g,values);
 public static object Main()
 {
  if(running)return new{started=true};if(!Application.isPlaying||Screen.width>=Screen.height)throw new Exception("Portrait Play Mode required");
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;jobs=g.jobsEnabled;Time.timeScale=0;phase=0;running=true;waiting=false;
  Setup();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;return new{started=true,frames=3};
 }
 static void Setup()
 {
  g.jobsEnabled=phase<2;g.RestartRide();
  if(phase<2)
  {
   g.ShowJobOffers();g.AcceptJob(0);
   g.ActiveJob.pickup=new RideAddress{node=Vector2Int.zero,axis=0,offset=30,curb=1};
   g.ActiveJob.dropoff=new RideAddress{node=Vector2Int.zero,axis=0,offset=56,curb=-1};
   if(phase==1){typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,PhoVietGame.WorkPhase.Delivery);typeof(PhoVietGame).GetProperty("Coins").SetValue(g,3450);}
   Call("ReplanJob",false);
  }
  else
  {
   typeof(PhoVietGame).GetProperty("Coins").SetValue(g,int.MaxValue);
   typeof(PhoVietGame).GetProperty("RouteStreet").SetValue(g,"Nguyễn Thị Minh Khai");
  }
  Call("PositionCamera",true);Call("UpdateJobMarker");
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   string path="Captures/compact-hud-native-"+names[phase]+".png";
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.8;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Screenshot not saved: "+path);
   phase++;waiting=false;
   if(phase==names.Length){Finish();File.WriteAllText("Captures/compact-hud-visual-results.json","{\"success\":true,\"frames\":3}");return;}
   Setup();next=EditorApplication.timeSinceStartup+.8;
  }
  catch(Exception e){File.WriteAllText("Captures/compact-hud-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Error{public string error;}
}
