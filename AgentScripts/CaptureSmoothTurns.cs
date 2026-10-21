using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class CaptureSmoothTurns
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,reckless=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  var target=c.targetTexture;var active=RenderTexture.active;
  var rt=new RenderTexture(Screen.width,Screen.height,24);var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
  string mode=Screen.width<Screen.height?"portrait":"wide";int files=0;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>{typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});typeof(PhoVietGame).GetMethod("PositionRideCamera",F).Invoke(g,new object[]{false,dt});};
  Action<string> shot=name=>
  {
   g.rider.GetComponent<BikeVisualAnimator>().Advance(1f/60);c.targetTexture=rt;c.Render();RenderTexture.active=rt;
   pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();
   File.WriteAllBytes("Captures/smooth-turns-"+mode+"-"+name+".png",pixels.EncodeToPNG());files++;
  };
  try
  {
   Time.timeScale=0;g.jobsEnabled=false;g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;
   foreach(int lane in new[]{0,3})
   {
    g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    set("spawnTimer",100000f);set("nextIntersectionDistance",float.PositiveInfinity);set("timeRiding",100f);
    typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
    while(g.NextJunctionDistance>24)tick(1f/60);g.MoveLane(lane==0?-1:1);
    while(!g.IsTurning||Mathf.Abs(g.TurnYaw)<35)tick(1f/60);shot(lane+"-curve");
    while(g.IsTurning)tick(1f/60);shot(lane+"-exit");
    for(int i=0;i<24;i++)tick(1f/60);shot(lane+"-straight");
   }
   return new{success=true,files,mode};
  }
  finally
  {
   c.targetTexture=target;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(pixels);
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
  }
 }
}
