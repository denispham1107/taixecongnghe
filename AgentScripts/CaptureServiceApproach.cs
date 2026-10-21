using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureServiceApproach
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static float scale;static bool jobs,surprises,danger,exits,running,waiting;static int phase,coins,best;static double next;
 static readonly string[] names={"pickup-approach","pickup-stop","pickup-departure","dropoff-approach","dropoff-stop","dropoff-departure"};
 static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");if(running)return new{started=true};
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;jobs=g.jobsEnabled;surprises=g.trafficSurprises;danger=g.recklessTrafficEnabled;exits=g.turnExitTrafficEnabled;
  coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Time.timeScale=0;phase=0;waiting=false;running=true;
  try{Setup();next=EditorApplication.timeSinceStartup+.7;EditorApplication.update+=Update;return new{started=true,frames=names.Length};}catch{Finish();throw;}
 }
 static void Setup()
 {
  bool pickup=phase<3;int stage=phase%3;
  g.jobsEnabled=true;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();g.ShowJobOffers();g.AcceptJob(0);
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);Set("nextIntersectionDistance",float.PositiveInfinity);Set("jobTrafficYieldRemaining",0f);
  typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,3);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[3],0,0);
  var address=new RideAddress{node=Vector2Int.zero,axis=0,offset=g.NavigationPosition.y+20,curb=1};
  if(pickup){g.ActiveJob.pickup=address;g.ActiveJob.dropoff=new RideAddress{node=new Vector2Int(0,4),axis=0,offset=40,curb=1};}else g.ActiveJob.dropoff=address;
  typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,pickup?PhoVietGame.WorkPhase.Pickup:PhoVietGame.WorkPhase.Delivery);Call("ReplanJob",false);
  if(!pickup){var customer=g.malePassenger;Set("customer",customer);customer.SetParent(g.rider,false);customer.localPosition=new Vector3(0,.16f,-.9f);customer.localRotation=Quaternion.identity;customer.gameObject.SetActive(true);customer.GetComponent<PassengerVisualAnimator>()?.Pose("Sit",0);}
  foreach(int lane in new[]{2,3})
  {
   int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth>.7f&&(lane==3?v.halfLength>3:v.halfLength<3));var v=g.SpawnTraffic(lane,-15,index);
   v.speed=v.baseTrafficSpeed=36;v.motionVelocity=Vector3.forward*36;v.enteringFromBehind=true;v.rearStartX=v.startPosition.x=PhoVietGame.LaneX[lane];v.brakeCooldown=v.laneCooldown=10000;
  }
  for(int i=0;i<40;i++)Call("TickRide",.05f);
  if(stage>0){int guard=0;while(!g.ServiceStopped&&guard++<200)Call("TickRide",.05f);if(!g.ServiceStopped)throw new Exception("Capture did not reach service");}
  if(stage==1)for(int i=0;i<20;i++)Call("TickRide",.05f);
  if(stage==2){int guard=0;while(g.ServiceStopped&&guard++<100)Call("TickRide",.05f);for(int i=0;i<12;i++)Call("TickRide",.05f);}
  if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Capture collided in "+names[phase]);
  g.daylight.SetCycleTime(10);Call("PositionCamera",true);
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   string path="Captures/service-approach-"+names[phase]+".png";
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.7;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing capture "+path);
   phase++;waiting=false;if(phase==names.Length){Finish();File.WriteAllText("Captures/service-approach-visual-results.json","{\"success\":true,\"frames\":6}");return;}
   Setup();next=EditorApplication.timeSinceStartup+.7;
  }
  catch(Exception e){File.WriteAllText("Captures/service-approach-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Error{public string error;}
}
