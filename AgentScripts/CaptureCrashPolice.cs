using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureCrashPolice
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static float scale;static bool jobs,surprises,danger,exits,running,waiting;static int phase,coins,best;static double next;
 static readonly string[] names={"waiting","detour","arrested","jail"};
 static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Tick(float dt){Call("TickRiderCrash",dt);foreach(var bike in g.GetComponentsInChildren<BikeVisualAnimator>(true))bike.Advance(dt);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");if(running)return new{started=true};
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;jobs=g.jobsEnabled;surprises=g.trafficSurprises;danger=g.recklessTrafficEnabled;exits=g.turnExitTrafficEnabled;
  coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Time.timeScale=0;phase=0;waiting=false;running=true;
  try
  {
   g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();Clear();Call("StartChase");g.policeBike.localPosition=g.rider.localPosition+Vector3.back*1000;g.Crash();Clear();
   int guard=0;while(!g.RiderFallComplete&&guard++<1200)Tick(.02f);if(!g.RiderFallComplete)throw new Exception("Rider did not settle");
   var r=g.rider.localPosition;Truck(r+new Vector3(-2.7f,0,9),0);Truck(r+new Vector3(2.7f,0,9),0);Truck(r+new Vector3(0,0,5),90);g.policeBike.localPosition=r+Vector3.forward*10;
   g.daylight.SetCycleTime(30);Call("PositionCamera",true);next=EditorApplication.timeSinceStartup+.7;EditorApplication.update+=Update;return new{started=true,frames=4};
  }catch{Finish();throw;}
 }
 static void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);Set("nextIntersectionDistance",float.PositiveInfinity);}
 static void Truck(Vector3 p,float yaw)
 {
  int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfLength>3);var v=g.SpawnTraffic(2,p.z,index);v.model.localPosition=v.previousPosition=p;v.model.localRotation=Quaternion.Euler(0,yaw,0);v.previousYaw=yaw;
  v.speed=v.baseTrafficSpeed=0;v.motionVelocity=Vector3.zero;v.routeDetached=true;v.routeVelocity=Vector3.zero;
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   string path="Captures/crash-police-capture-"+names[phase]+".png";
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.7;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing capture "+path);
   phase++;waiting=false;if(phase==names.Length){Finish();File.WriteAllText("Captures/crash-police-visual-results.json","{\"success\":true,\"frames\":4}");return;}
   if(phase==1)for(int i=0;i<30;i++)Tick(.02f);
   if(phase==2){int guard=0;while(g.State==PhoVietGame.RideState.Falling&&guard++<1000)Tick(.02f);if(g.State!=PhoVietGame.RideState.Arrested)throw new Exception("Police did not arrive");}
   if(phase==3){Call("TickArrest",1.2f);if(g.State!=PhoVietGame.RideState.Jailed)throw new Exception("Prison transition did not finish");}
   Call("PositionCamera",true);next=EditorApplication.timeSinceStartup+.7;
  }
  catch(Exception e){File.WriteAllText("Captures/crash-police-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Error{public string error;}
}
