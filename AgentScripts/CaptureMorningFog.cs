using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureMorningFog
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static float scale;static bool jobs,surprises,danger,exits,running,waiting;static int phase;static double next;static string mode;
 static readonly float[] seconds={0,1.5f,3,4.5f,6,30,60};
 static readonly string[] names={"start","early","middle","late","clear","night","next-day"};
 static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");if(running)return new{started=true};
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;jobs=g.jobsEnabled;surprises=g.trafficSurprises;danger=g.recklessTrafficEnabled;exits=g.turnExitTrafficEnabled;
  Time.timeScale=0;phase=0;waiting=false;running=true;mode=Screen.width<Screen.height?"portrait":"wide";Setup();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;
  return new{started=true,frames=seconds.Length,mode};
 }
 static void Setup()
 {
  g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);Set("nextIntersectionDistance",float.PositiveInfinity);
  int car=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3),bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);
  g.SpawnTraffic(3,25,car);g.SpawnTraffic(1,42,bike);g.daylight.SetCycleTime(seconds[phase]);Call("PositionCamera",true);
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   string path="Captures/morning-fog-native-"+mode+"-"+names[phase]+".png";
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.8;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing screenshot "+path);
   phase++;waiting=false;if(phase==seconds.Length){Finish();File.WriteAllText("Captures/morning-fog-"+mode+"-visual-results.json","{\"success\":true,\"frames\":7}");return;}
   Setup();next=EditorApplication.timeSinceStartup+.8;
  }
  catch(Exception e){File.WriteAllText("Captures/morning-fog-"+mode+"-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Error{public string error;}
}
