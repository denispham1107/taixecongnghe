using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class CaptureInnerTurns
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  var target=c.targetTexture;var pos=c.transform.localPosition;var rot=c.transform.localRotation;float fov=c.fieldOfView;var active=RenderTexture.active;
  var rt=new RenderTexture(1440,900,24);var pixels=new Texture2D(1440,900,TextureFormat.RGB24,false);
  const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>{typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{false});};
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action<string> shot=name=>{g.rider.GetComponent<BikeVisualAnimator>().Advance(.02f);c.targetTexture=rt;c.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,1440,900),0,0);pixels.Apply();File.WriteAllBytes("Captures/inner-turns-"+name+".png",pixels.EncodeToPNG());};
  try
  {
   Time.timeScale=0;reset();while(g.NextJunctionDistance>24)tick(.02f);
   c.transform.localPosition=new Vector3(0,13,-3);c.transform.LookAt(g.transform.TransformPoint(new Vector3(0,0,11)));c.fieldOfView=56;shot("paint");
   foreach(int lane in new[]{1,2})
   {
    reset();while(g.NextJunctionDistance>24)tick(.02f);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);g.MoveLane(lane==1?-1:1);
    int guard=0;while((!g.IsTurning||Mathf.Abs(g.TurnYaw)<43)&&guard++<500)tick(.02f);shot("lane-"+lane+"-curve");
    while(g.IsTurning&&guard++<900)tick(.02f);for(int i=0;i<25;i++)tick(.02f);shot("lane-"+lane+"-exit");
   }
   return new{success=true,files=5};
  }
  finally
  {g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=scale;c.targetTexture=target;c.transform.localPosition=pos;c.transform.localRotation=rot;c.fieldOfView=fov;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(pixels);}
 }
}
