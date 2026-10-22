using System;using System.IO;using System.Reflection;using UnityEditor;using UnityEngine;using PhoViet;
public static class CaptureJunctionRoof
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static Camera camera;static PhoVietGame.TrafficVehicle target;
 static float scale,fov;static bool jobs,danger,surprises,exits,running;static int phase,coins,best;static double next;
 static Vector3 cameraPosition;static Quaternion cameraRotation;
 static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
 static object Call(string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
 static void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);Set("nextIntersectionDistance",float.PositiveInfinity);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");if(running)return new{started=true};
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();camera=g.rideCamera;scale=Time.timeScale;fov=camera.fieldOfView;cameraPosition=camera.transform.localPosition;cameraRotation=camera.transform.localRotation;
  jobs=g.jobsEnabled;danger=g.recklessTrafficEnabled;surprises=g.trafficSurprises;exits=g.turnExitTrafficEnabled;coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Time.timeScale=0;phase=0;running=true;Prepare();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;return new{started=true,frames=4};
 }
 static void Prepare()
 {
  g.jobsEnabled=false;g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;g.RestartRide();Clear();
  if(phase<2)
  {
   int turn=phase==0?-1:1,side=-turn;while(g.NextJunctionDistance>11.65f){Call("TickRide",.02f);Clear();}
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,turn<0?1:2);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[g.CurrentLane],0,0);
   typeof(PhoVietGame).GetProperty("QueuedTurn").SetValue(g,turn);Call("StartJunctionTurnAt",false,g.NextJunctionDistance);
   float length=(float)typeof(PhoVietGame).GetField("turnLength",F).GetValue(g);while((float)typeof(PhoVietGame).GetField("turnTravel",F).GetValue(g)<length*.4f)Call("TickRide",.01f);
   Quaternion q=Quaternion.Euler(0,g.TurnYaw,0);int index=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);target=g.SpawnTraffic(3,0,index);
   target.model.localPosition=g.rider.localPosition+q*Vector3.right*side*3.1f;target.previousPosition=target.model.localPosition;target.model.localRotation=q;target.routeDetached=true;target.routeVelocity=q*Vector3.forward*17;target.speed=target.baseTrafficSpeed=17;
   if(!g.TryKick())throw new Exception("No junction capture target");for(int i=0;i<150&&!target.knockFromPlayer&&g.State==PhoVietGame.RideState.Riding;i++)Call("TickRide",.005f);
   if(!target.knockFromPlayer||g.LastKickFootGap>.081f)throw new Exception("Capture requires real foot contact");
  }
  else
  {
   string name=phase==2?"PV_Sedan":"PV_BoxTruck";int index=g.vehicles.FindIndex(v=>v.model.GetComponent<CarVisualAnimator>()?.AssetName==name);bool eventOnly=g.vehicles[index].eventOnly;g.vehicles[index].eventOnly=false;
   target=g.SpawnTraffic(2,phase==2?18:16.07f,index);g.vehicles[index].eventOnly=eventOnly;target.speed=target.baseTrafficSpeed=7;
   g.TryJump();for(int i=0;i<250&&!g.RiderOnRoof&&g.State==PhoVietGame.RideState.Riding;i++)Call("TickRide",.01f);
   if(!g.RiderOnRoof||g.State!=PhoVietGame.RideState.Riding)throw new Exception("Capture requires actual roof support");
  }
  g.rider.GetComponent<BikeVisualAnimator>().Advance(0);target.model.GetComponent<BikeVisualAnimator>()?.Advance(0);target.model.GetComponent<CarVisualAnimator>()?.Advance(0);
  foreach(var skin in g.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try{Capture();phase++;if(phase==4){Finish();File.WriteAllText("Captures/junction-roof-captures.json","{\"success\":true,\"frames\":4}");return;}Prepare();next=EditorApplication.timeSinceStartup+.8;}
  catch(Exception e){Finish();File.WriteAllText("Captures/junction-roof-captures.json",e.ToString());}
 }
 static void Capture()
 {
  var rt=new RenderTexture(1200,900,24);var image=new Texture2D(1200,900,TextureFormat.RGB24,false);var old=camera.targetTexture;var active=RenderTexture.active;
  string name=new[]{"kick-turn-left","kick-turn-right","ride-sedan-roof","ride-truck-roof"}[phase];
  try
  {
   Vector3 mid=(g.rider.position+target.model.position)*.5f;Quaternion q=Quaternion.Euler(0,g.rider.eulerAngles.y,0);Vector3 offset=phase<2?new Vector3(phase==0?4:-4,3,-5):new Vector3(-6,4,-7);
   camera.transform.position=mid+q*offset;camera.transform.LookAt(mid+Vector3.up*(phase<2?.9f:1.2f));camera.fieldOfView=48;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes("Captures/junction-"+name+".png",image.EncodeToPNG());
  }
  finally{camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
 }
 static void Finish()
 {
  running=false;EditorApplication.update-=Update;g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
  foreach(var skin in g.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=false;
  PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;camera.transform.localPosition=cameraPosition;camera.transform.localRotation=cameraRotation;camera.fieldOfView=fov;
 }
}
