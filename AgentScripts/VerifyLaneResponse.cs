using System;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyLaneResponse
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Measure(){return Run(false);}
 public static object Main(){return Run(true);}
 static object Run(bool verify)
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var car=g.rider.GetComponent<PlayerCarVisual>();
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;float scale=Time.timeScale;int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=g.BestDistance;
  var checks=new List<string>();var traces=new List<object>();
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>{typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});car.Advance(dt);};
  Action reset=()=>{g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);set("junctionAt",float.PositiveInfinity);typeof(PlayerCarVisual).GetProperty("WheelSteerAngle").SetValue(car,0f);};
  Action<int,int> input=(kind,dir)=>
  {
   if(kind==0)g.MoveLane(dir);
   else if(kind==1){var p=new Vector2(Screen.width*.5f,Screen.height*.5f);typeof(PhoVietGame).GetMethod("BeginSwipe",F).Invoke(g,new object[]{p,10f,7});typeof(PhoVietGame).GetMethod("ContinueSwipe",F).Invoke(g,new object[]{p+Vector2.right*dir*Mathf.Min(Screen.width,Screen.height)*.08f,false,10.01f,true});}
   else{g.ApplyBrowserGesture("{\"phase\":0,\"id\":7,\"action\":0,\"x\":0.5,\"y\":0.5,\"at\":10}");g.ApplyBrowserGesture("{\"phase\":1,\"id\":7,\"action\":"+dir+",\"x\":0.5,\"y\":0.5,\"at\":10.01}");}
  };
  try
  {
   Time.timeScale=0;
   foreach(int kind in verify?new[]{0,1,2}:new[]{0})foreach(int dir in verify?new[]{-1,1}:new[]{1})foreach(bool boost in verify?new[]{false,true}:new[]{false})foreach(float dt in verify?new[]{1f/60,1f/30,.05f}:new[]{1f/60})
   {
    reset();tick(dt);if(boost)g.TryBoost();tick(dt);float x0=g.rider.localPosition.x,start=g.Distance,speed=g.Speed;int lane=g.CurrentLane+dir;input(kind,dir);
    string label=$"kind={kind},dir={dir},W={boost},dt={dt}";
    check(g.CurrentLane==lane,"Input accepted before release or another frame: "+label);check(Mathf.Abs(g.rider.localPosition.x-x0)<.00001f,"Input does not teleport the car: "+label);
    float t=0,firstMove=0,firstYaw=0,firstWheel=0,at50=0,at100=0,response=0,done=0,last=x0,maxStep=0,maxYawStep=0,previousYaw=0;bool monotonic=true;int guard=0;
    while(t<2.8f&&guard++<500)
    {
     tick(dt);t+=dt;float dx=(g.rider.localPosition.x-x0)*dir,yaw=g.RiderSteerYaw;
     if(guard==1){firstMove=dx;firstYaw=Mathf.Abs(yaw);firstWheel=Mathf.Abs(car.WheelSteerAngle);}
     if(at50==0&&t>=.05f-.0001f)at50=dx;if(at100==0&&t>=.1f-.0001f)at100=dx;
     if(response==0&&dx>=.01f)response=t;
     monotonic&=(g.rider.localPosition.x-last)*dir>=-.0001f;maxStep=Mathf.Max(maxStep,Mathf.Abs(g.rider.localPosition.x-last));maxYawStep=Mathf.Max(maxYawStep,Mathf.Abs(yaw-previousYaw));last=g.rider.localPosition.x;previousYaw=yaw;
     if(done==0&&Mathf.Abs(last-PhoVietGame.LaneX[lane])<.001f)done=g.Distance-start;
    }
    if(verify)
    {
     check(firstMove>=.001f&&firstYaw>.05f&&firstWheel>.1f,"Body position, heading and wheels respond on first simulated frame: "+label);
     check(response>0&&response<=.067f,"First centimetre responds within 67ms including low-FPS quantization: "+label);
     // 21.94 adds another 10% pace; allow proportional per-frame motion plus yaw quantization.
     check(monotonic&&maxStep<.66f&&maxYawStep<4.86f,"Lane path remains monotonic with no position/yaw snaps: "+label);
     check(done>15.8f&&done<20&&Mathf.Abs(g.RiderSteerYaw)<.1f,"Cumulative 1.3915 pace lane distance and gradual steering recovery: "+label);
    }
    traces.Add(new{kind,dir,boost,dt,firstMove,firstYaw,firstWheel,at50,at100,response,done,maxStep,maxYawStep});
   }
   check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased&&g.BestDistance==best&&PlayerPrefs.GetInt("PhoViet.Coins.v1",0)==coins,"Jobs, coins and record preserved");
   var report=new{success=true,count=checks.Count,checks,traces};File.WriteAllText(verify?"Captures/lane-response-checks.json":"Captures/lane-response-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=scale;}
 }
}
