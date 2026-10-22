// Separate editor frames allow the skinned mesh to catch up to the sampled physical kick pose.
using System;using System.IO;using System.Reflection;using UnityEditor;using UnityEngine;using PhoViet;
public static class CaptureKickContact
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static BikeVisualAnimator hero;static PhoVietGame.TrafficVehicle target;static Camera camera;
 static float scale,fov;static bool jobs,danger,surprises,exits;static Vector3 cameraPosition;static Quaternion cameraRotation;static int phase;static double next;static bool running;
 static void Call(string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");if(running)return new{started=true};
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();hero=g.rider.GetComponent<BikeVisualAnimator>();camera=g.rideCamera;
  scale=Time.timeScale;fov=camera.fieldOfView;cameraPosition=camera.transform.localPosition;cameraRotation=camera.transform.localRotation;
  jobs=g.jobsEnabled;danger=g.recklessTrafficEnabled;surprises=g.trafficSurprises;exits=g.turnExitTrafficEnabled;Time.timeScale=0;
  phase=0;running=true;Prepare();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;return new{started=true,frames=6};
 }
 static void Prepare()
 {
  g.jobsEnabled=false;g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;g.RestartRide();
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,10000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);
  bool left=phase>=3;int lane=left?2:3;int player=left?3:2;
  typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,player);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[player],0,0);
  int index=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);target=g.SpawnTraffic(lane,left?12:0,index);target.speed=target.baseTrafficSpeed=left?-12:17;target.model.localRotation=Quaternion.Euler(0,left?180:0,0);
  if(!g.TryKick())throw new Exception("Capture target unavailable");
  int stage=phase%3;if(stage==1){for(int i=0;i<4;i++)Call("TickRide",.02f);}
  if(stage==2){for(int i=0;i<120&&!target.knocked;i++)Call("TickRide",.005f);if(!target.knocked)throw new Exception("No actual foot contact");}
  hero.Advance(0);target.model.GetComponent<BikeVisualAnimator>().Advance(0);
  foreach(var skin in g.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   string name=(phase>=3?"left-oncoming":"right-same")+new[]{"-before","-approach","-contact"}[phase%3];Capture(name);phase++;
   if(phase==6){Finish();File.WriteAllText("Captures/kick-contact-captures.json","{\"success\":true,\"frames\":6}");return;}
   Prepare();next=EditorApplication.timeSinceStartup+.8;
  }
  catch(Exception e){Finish();File.WriteAllText("Captures/kick-contact-captures.json",e.ToString());}
 }
 static void Capture(string name)
 {
  Vector3 mid=(g.rider.position+target.model.position)*.5f;var rt=new RenderTexture(1200,900,24);var image=new Texture2D(1200,900,TextureFormat.RGB24,false);var old=camera.targetTexture;var active=RenderTexture.active;
  try{camera.transform.position=mid+new Vector3(0,3,-5);camera.transform.LookAt(mid+Vector3.up*.9f);camera.fieldOfView=48;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes("Captures/kick-"+name+".png",image.EncodeToPNG());}
  finally{camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
 }
 static void Finish()
 {
  running=false;EditorApplication.update-=Update;g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
  foreach(var skin in g.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=false;
  g.ResetRide();Time.timeScale=scale;camera.transform.localPosition=cameraPosition;camera.transform.localRotation=cameraRotation;camera.fieldOfView=fov;
 }
}
