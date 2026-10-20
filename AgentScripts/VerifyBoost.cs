using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyBoost
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;clear();};
  Action<float> clock=dt=>call("TickBoost",new object[]{dt});Action<float> tick=dt=>call("TickRide",new object[]{dt});
  try
  {
   Time.timeScale=0;g.ResetRide();check(!g.TryBoost(),"Ready screen rejects boost");reset();
   check(PhoVietGame.IsBoostSwipe(new Vector2(10,-100),540),"Downward mobile swipe selects boost");
   check(!PhoVietGame.IsBoostSwipe(new Vector2(10,100),540)&&!PhoVietGame.IsBoostSwipe(new Vector2(110,-10),540),"Up/horizontal swipes do not boost");
   check(PhoVietGame.IsJumpSwipe(new Vector2(0,100),540)&&!PhoVietGame.IsJumpSwipe(new Vector2(0,-100),540),"Boost and jump gestures remain distinct");
   set("timeRiding",25f);tick(.02f);float normal=g.Speed;reset();set("timeRiding",25f);
   check(g.TryBoost()&&g.BoostRemaining==4&&g.BoostCooldown==0,"Boost begins with four seconds and no concurrent cooldown");tick(.02f);
   check(Mathf.Abs(g.Speed/normal-2.25f)<.0001f,"Boost multiplies current normal speed by exactly 2.25");
   float arrival=(float)call("TimeToIntersection",new object[]{60f});check(arrival<60/normal,"Cross traffic schedules arrival using boosted speed");
   set("boostRemaining",.5f);float shortBoost=(float)call("TimeToIntersection",new object[]{90f});check(shortBoost>.5f&&shortBoost<90/normal,"Intersection prediction accounts for boost ending before arrival");set("boostRemaining",3.98f);
   float before=g.BoostRemaining;check(!g.TryBoost()&&g.BoostRemaining==before,"Repeated W cannot stack or extend boost");
   var visual=g.rider.GetComponent<BoostSmokeVisual>();visual.Simulate(.18f);
   check(visual.Emitting&&visual.ParticleCount>0,"Blender smoke emits actual particles during boost");
   var texture=Resources.Load<Texture2D>("Boost/PV_WhiteBoostSmoke");check(texture!=null&&texture.width==texture.height*4,"Four-frame Blender flipbook is available in player Resources");
   g.TogglePause();float remaining=g.BoostRemaining;clock(2);visual.Advance();check(g.BoostRemaining==remaining,"Pause freezes boost duration");
   foreach(var p in visual.GetComponentsInChildren<ParticleSystem>(true))check(!p.isPlaying,"Paused particle simulation is frozen");g.TogglePause();visual.Advance();
   check(visual.Emitting,"Resuming continues active boost exhaust");
   reset();g.TryBoost();clock(3.99f);check(g.BoostActive&&g.BoostRemaining<.011f,"Boost lasts through 3.99 seconds");clock(.01f);
   check(!g.BoostActive&&Mathf.Abs(g.BoostCooldown-2)<.001f,"Four seconds ends boost and starts a two-second cooldown");
   check(!g.TryBoost(),"Cooldown rejects W");clock(1.99f);check(!g.TryBoost(),"Cooldown is still enforced just before two seconds");clock(.02f);check(g.TryBoost(),"Boost becomes available after two seconds");
   reset();g.TryJump();tick(.3f);check(g.RiderAirborne&&g.TryBoost(),"Boost can activate during the existing jump");tick(.02f);check(g.RiderAirborne&&g.BoostActive&&g.LivesRemaining==3,"Jump and boost coexist without canceling jump or spending a life");
   reset();g.TryBoost();g.TogglePause();check(!g.TryBoost(),"Paused state rejects a new boost");g.TogglePause();
   g.jobsEnabled=true;g.ShowJobOffers();check(g.JobOffersOpen,"Job list can pause an active boost");remaining=g.BoostRemaining;clock(2);check(g.BoostRemaining==remaining,"Job selection freezes boost timer");g.AcceptJob(0);clear();
   call("BeginService",new object[0]);check(!g.BoostActive&&g.BoostCooldown==2&&g.ServiceStopped,"Pickup cancels boost immediately and starts cooldown");
   check(!g.TryBoost(),"Boarding passenger blocks boost");clock(1);check(Mathf.Abs(g.BoostCooldown-1)<.001f,"Cooldown continues while stopped for a customer");clock(1);check(g.BoostCooldown==0,"Full two-second service wait completes cooldown");
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);clear();typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,PhoVietGame.WorkPhase.Delivery);g.TryBoost();
   call("BeginService",new object[0]);check(g.ServiceStopped&&!g.BoostActive&&g.BoostCooldown==2,"Dropoff also cancels active boost");
   reset();g.TryBoost();g.Crash();check(g.State==PhoVietGame.RideState.Falling&&!g.BoostActive&&g.BoostCooldown==2&&g.LivesRemaining==2,"Impact cancels boost, keeps three-life rule and starts cooldown");
   call("TickRiderCrash",new object[]{.2f});check(g.BoostCooldown<2,"Cooldown continues during physical falling");check(!g.TryBoost(),"Falling state rejects boost");
   reset();set("timeRiding",150f);int car=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3);var obstacle=g.SpawnTraffic(2,4,car);obstacle.speed=obstacle.baseTrafficSpeed=0;
   g.TryBoost();for(int i=0;i<8&&g.State==PhoVietGame.RideState.Riding;i++)tick(.05f);
   check(g.State==PhoVietGame.RideState.Falling&&g.LivesRemaining==2&&!g.BoostActive,"Real high-speed swept collision spends one life and cancels boost instead of tunneling");
   reset();call("StartChase",new object[0]);g.TryBoost();tick(.02f);check(g.IsChased&&g.BoostActive,"Boost is available during pursuit");g.Arrest();check(g.State==PhoVietGame.RideState.Arrested&&!g.BoostActive,"Capture cancels boost and ends the ride immediately");
   reset();g.TryBoost();set("turnSide",1);call("FinishJunctionTurn",new object[0]);check(g.BoostActive,"Street rebase preserves remaining boost duration");visual.Advance();check(visual.Emitting,"Smoke restarts in the new street coordinate frame");
   reset();g.TryBoost();g.ResetRide();check(!g.BoostActive&&g.BoostRemaining==0&&g.BoostCooldown==0&&visual.ParticleCount==0,"New run resets timers and clears exhaust particles");
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
  }
 }
}
