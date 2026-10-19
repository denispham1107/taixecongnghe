using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureRiderLives
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static BikeVisualAnimator hero;static float scale;static bool jobs,reckless,surprises,exit,running;static int phase;static double next;
 static readonly string[] names={"left-impact","right-impact","stand-up","resumed"};
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
 public static object Main()
 {
  if(running)return new{started=true};g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  scale=Time.timeScale;jobs=g.jobsEnabled;reckless=g.recklessTrafficEnabled;surprises=g.trafficSurprises;exit=g.turnExitTrafficEnabled;
  Time.timeScale=0;phase=0;running=true;Setup();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;return new{started=true,frames=4};
 }
 static void Setup()
 {
  g.jobsEnabled=false;g.RestartRide();g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);Set("nextIntersectionDistance",float.PositiveInfinity);
  hero=g.rider.GetComponent<BikeVisualAnimator>();Vector3 p=g.rider.localPosition;int direction=phase==0?1:-1;
  var hit=g.SpawnCrossTraffic(direction,0,true,12);hit.previousPosition=p-Vector3.right*direction*8;hit.model.localPosition=p+Vector3.right*direction*8;hit.motionVelocity=Vector3.right*direction*12;
  typeof(PhoVietGame).GetProperty("Speed").SetValue(g,0f);Call("ResolveRiderContacts",p.x,.05f);
  hit.active=false;hit.model.gameObject.SetActive(false);
  if(phase<2)for(int i=0;i<20;i++){Call("TickRiderCrash",.02f);hero.Advance(.02f);}
  else
  {
   int guard=0;while(g.State==PhoVietGame.RideState.Falling&&guard++<1200){Call("TickRiderCrash",.02f);hero.Advance(.02f);}
   for(int i=0;i<16;i++){Call("TickRiderCrash",.02f);hero.Advance(.02f);}
  }
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   var c=g.rideCamera;Vector3 focus=g.rider.position+Vector3.up*.65f;c.transform.position=focus+new Vector3(phase==0?-4:4,3,3.8f);c.transform.LookAt(focus);c.fieldOfView=44;
   var rt=new RenderTexture(1200,750,24);var texture=new Texture2D(1200,750,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
   try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1200,750),0,0);texture.Apply();File.WriteAllBytes("Captures/rider-lives-"+names[phase]+".png",texture.EncodeToPNG());}
   finally{c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}
   phase++;if(phase==4){Finish();File.WriteAllText("Captures/rider-lives-visual-results.json","{\"success\":true,\"frames\":4}");return;}
   if(phase<3)Setup();else{int guard=0;while(g.State==PhoVietGame.RideState.Recovering&&guard++<100){Call("TickRiderCrash",.02f);hero.Advance(.02f);}for(int i=0;i<10;i++){Call("TickRide",.04f);hero.Advance(.04f);}}
   next=EditorApplication.timeSinceStartup+.8;
  }
  catch(Exception e){File.WriteAllText("Captures/rider-lives-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;g.jobsEnabled=jobs;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exit;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Error{public string error;}
}
