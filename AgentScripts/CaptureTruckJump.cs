using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class CaptureTruckJump
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  var c=g.rideCamera;float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  var target=c.targetTexture;var pos=c.transform.localPosition;var rot=c.transform.localRotation;float fov=c.fieldOfView;
  var rt=new RenderTexture(1200,900,24);var image=new Texture2D(1200,900,TextureFormat.RGB24,false);var previous=RenderTexture.active;
  const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action<string> shot=name=>{foreach(var v in g.vehicles)if(v.active)v.model.GetComponent<CarVisualAnimator>()?.Advance(.01f);g.rider.GetComponent<BikeVisualAnimator>().Advance(.01f);c.targetTexture=rt;c.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes("Captures/truck-jump-"+name+".png",image.EncodeToPNG());};
  try
  {
   Time.timeScale=0;g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
   int index=g.vehicles.FindIndex(v=>!v.eventOnly&&v.model.GetComponent<CarVisualAnimator>()?.AssetName=="PV_BoxTruck");var truck=g.SpawnTraffic(2,16.1f,index);truck.speed=truck.baseTrafficSpeed=7;truck.laneCooldown=truck.brakeCooldown=1000;
   float x=g.rider.localPosition.x;c.transform.localPosition=new Vector3(x+3.8f,6.5f,-8);c.transform.LookAt(g.transform.TransformPoint(new Vector3(x,2.7f,1)));c.fieldOfView=52;
   g.TryJump();for(int i=0;i<90;i++)tick(.01f);shot("apex");
   bool roof=false;for(int i=0;i<100;i++){tick(.01f);if(g.RiderOnRoof){roof=true;break;}}if(!roof)throw new Exception("Capture never reached roof");
   shot("roof");bool cab=false;for(int i=0;i<120;i++){tick(.01f);if(g.RiderAirborne&&g.RiderJumpHeight<3&&g.RiderJumpHeight>2.6f){cab=true;break;}}if(!cab)throw new Exception("Capture never crossed cab");shot("cab");
   for(int i=0;i<160&&(g.RiderOnRoof||g.RiderAirborne);i++)tick(.01f);shot("road");
   if(g.LivesRemaining!=3||g.RiderJumpHeight!=0)throw new Exception("Unsafe capture landing");
   return new{success=true,files=new[]{"truck-jump-apex.png","truck-jump-roof.png","truck-jump-cab.png","truck-jump-road.png"}};
  }
  finally{g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=scale;c.targetTexture=target;c.transform.localPosition=pos;c.transform.localRotation=rot;c.fieldOfView=fov;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
 }
}
