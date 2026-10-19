using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyRiderLives
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exit=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;clear();};
  Action<float> fall=dt=>call("TickRiderCrash",new object[]{dt});
  Func<int,float,bool,PhoVietGame.TrafficVehicle> impact=(direction,speed,car)=>
  {
   int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&(v.halfWidth>.7f)==car);
   var v=g.SpawnTraffic(2,0,index);Vector3 p=g.rider.localPosition;
   Vector3 velocity=direction==0?Vector3.forward*speed:direction==2?Vector3.back*speed:Vector3.right*direction*speed;
   v.speed=v.baseTrafficSpeed=velocity.z;v.motionVelocity=velocity;v.model.localRotation=Quaternion.Euler(0,Mathf.Atan2(velocity.x,velocity.z)*Mathf.Rad2Deg,0);
   v.previousPosition=p-velocity.normalized*12;v.model.localPosition=p+velocity.normalized*12;
   typeof(PhoVietGame).GetProperty("Speed").SetValue(g,0f);set("oldRiderZ",p.z);
   call("ResolveRiderContacts",new object[]{p.x,.05f});return v;
  };
  Func<int> finish=()=>{int guard=0;while((g.State==PhoVietGame.RideState.Falling||g.State==PhoVietGame.RideState.Recovering)&&guard++<1200)fall(.02f);return guard;};
  try
  {
   Time.timeScale=0;reset();check(g.LivesRemaining==3,"Every new run starts with exactly three lives");
   float leftRoll=0;
   foreach(int direction in new[]{1,-1})
   {
    reset();impact(direction,12,true);check(g.State==PhoVietGame.RideState.Falling&&g.LivesRemaining==2,"Side impact "+direction+" starts one crash and spends one life");
    check(Mathf.Sign(g.RiderFallVelocity.x)==direction,"Side impact launches the rider away from the actual contact side "+direction);
    if(direction==1)leftRoll=g.RiderFallRoll;else check(leftRoll*g.RiderFallRoll<0,"Opposite side impacts produce opposite fall rolls");
    clear();fall(.2f);check(Mathf.Sign(g.rider.localEulerAngles.z>180?g.rider.localEulerAngles.z-360:g.rider.localEulerAngles.z)==Mathf.Sign(g.RiderFallRoll),"Outer motorcycle tilts toward the computed impact direction");
    int lives=g.LivesRemaining;call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,.02f});check(g.LivesRemaining==lives,"Repeated contact checks during the same fall cannot spend more lives");
    check(finish()<1200&&g.State==PhoVietGame.RideState.Riding&&g.LivesRemaining==2,"Rider slides, settles, gets up and resumes with two lives");
    check(g.RecoveryTrafficYieldRemaining==3,"Finishing recovery starts exactly three seconds of traffic yielding");
    check(Mathf.Abs(g.rider.localPosition.z)<.001f&&g.rider.localPosition.x*direction>0,"Recovery retains lateral crash displacement while rebasing the scrolling road");
    check(PhoVietGame.LaneX.All(x=>Mathf.Abs(x-g.rider.localPosition.x)>=Mathf.Abs(PhoVietGame.LaneX[g.CurrentLane]-g.rider.localPosition.x)-.001f),"Recovery selects the nearest lane to the actual settled position");
    var hero=g.rider.GetComponent<BikeVisualAnimator>();hero.Advance(.02f);check(hero.CurrentClip=="Ride"||hero.CurrentClip=="Idle","Player skeleton exits its fallen pose after recovery");
   }
   float slowTravel=0,slowLaunch=0;
   foreach(float speed in new[]{7f,21f})
   {
    reset();impact(0,speed,true);float launch=g.RiderFallVelocity.z;clear();check(finish()<1200,"Rear impact at "+speed+" m/s eventually settles");
    if(speed==7){slowTravel=g.RiderFallTravel;slowLaunch=launch;}
    else check(launch>slowLaunch*1.8f&&g.RiderFallTravel>slowTravel*1.8f,"Faster rear impact produces a faster launch and a longer physical slide");
   }
   reset();impact(0,16,false);float bikeLaunch=g.RiderFallVelocity.z;
   reset();impact(0,16,true);check(g.RiderFallVelocity.z>bikeLaunch,"At equal impact speed a heavier car transfers more momentum than another motorcycle");
   reset();impact(2,16,true);check(g.RiderFallVelocity.z<0,"Head-on impact throws the rider backward instead of using a fixed sideways fall");clear();check(finish()<1200,"A backward slide can recover without snapping back to the collision point");
   check(g.NextJunctionDistance<=80&&g.NavigationNextNode.y==0,"Sliding backward across the preceding junction realigns the GPS node with that physical intersection");
   reset();impact(0,7,true);clear();g.TogglePause();Vector3 paused=g.rider.localPosition;fall(2);check(g.State==PhoVietGame.RideState.Paused&&g.rider.localPosition==paused,"Pause freezes crash physics");g.TogglePause();check(g.State==PhoVietGame.RideState.Falling,"Unpausing restores the falling phase instead of prematurely riding");
   int guard=0;while(g.State==PhoVietGame.RideState.Falling&&guard++<1200)fall(.02f);
   check(g.State==PhoVietGame.RideState.Recovering,"A settled rider enters the stand-up phase");
   Vector3 standPosition=g.rider.localPosition;
   var visual=g.rider.GetComponent<BikeVisualAnimator>();visual.Advance(.02f);check(visual.CurrentClip=="Recover","Player uses the imported stand-up animation");
   g.TogglePause();float progress=g.RiderRecoveryProgress;fall(1);check(g.RiderRecoveryProgress==progress,"Pause freezes the stand-up animation progress too");g.TogglePause();finish();
   check(Mathf.Abs(g.rider.localPosition.x-standPosition.x)<.001f,"Standing up does not teleport the rider to a lane centre");
   call("TickRide",new object[]{.5f});check(Mathf.Abs(g.RecoveryTrafficYieldRemaining-2.5f)<.001f,"Recovery yielding counts riding simulation time, even with jobs disabled");
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);var job=g.ActiveJob;int wallet=g.Coins;
   clear();impact(0,7,true);clear();finish();check(g.ActiveJob==job&&g.JobPhase==PhoVietGame.WorkPhase.Pickup&&g.Coins==wallet,"Recovering retains the accepted customer job and never pays unfinished work");
   reset();
   for(int life=2;life>=0;life--)
   {
    impact(0,7,true);check(g.LivesRemaining==life&&g.State==PhoVietGame.RideState.Falling,"Impact spends life and shows the fall before resolving the run: "+life);
    clear();check(finish()<1200,"Crash sequence finishes for remaining lives "+life);
    check(g.State==(life>0?PhoVietGame.RideState.Riding:PhoVietGame.RideState.GameOver),"Only the third traffic crash ends the run");
   }
   g.RestartRide();check(g.LivesRemaining==3,"Restart restores all three lives");
   foreach(bool duringFall in new[]{false,true})
   {
    reset();call("StartChase",null);if(duringFall){impact(0,7,true);clear();}else clear();
    g.policeBike.localPosition=g.rider.localPosition+new Vector3(-.5f,0,-.5f);
    if(duringFall)fall(.02f);else call("TickRide",new object[]{.02f});
    check(g.State==PhoVietGame.RideState.Arrested&&g.LivesRemaining>0,"Police capture immediately ends control despite remaining lives, falling="+duringFall);
    call("TickArrest",new object[]{1.2f});check(g.State==PhoVietGame.RideState.Jailed,"Police capture still transitions directly to the jail scene");
   }
   reset();call("StartChase",null);g.policeBike.localPosition=new Vector3(0,0,-1000);impact(0,7,true);clear();finish();
   check(g.State==PhoVietGame.RideState.Riding&&g.LivesRemaining==2&&g.IsChased,"A traffic collision during pursuit spends one life, permits recovery and retains the chase");
   reset();call("StartChase",null);g.policeBike.localPosition=g.rider.localPosition+Vector3.forward*10;typeof(PhoVietGame).GetProperty("PoliceDistance").SetValue(g,-10f);
   call("TickRide",new object[]{.02f});check(g.State==PhoVietGame.RideState.Riding,"Police ahead after a backward crash cannot capture the rider from ten metres away");
   foreach(int side in new[]{-1,1})
   {
    reset();int lane=side<0?0:3;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
    int guardTurn=0;while(g.NextJunctionDistance>22&&guardTurn++<1000)call("TickRide",new object[]{.02f});
    g.MoveLane(side);while(!g.IsTurning&&guardTurn++<1500)call("TickRide",new object[]{.02f});
    check(g.IsTurning,"Corner "+side+": enter the real junction before impact");call("TickRide",new object[]{.15f});
    impact(-side,8,true);clear();check(g.State==PhoVietGame.RideState.Falling,"Corner "+side+": actual corner contact spends a life");
    check(finish()<1200&&g.State==PhoVietGame.RideState.Riding&&g.IsTurning,"Corner "+side+": recovery retains the connected turn");
    guardTurn=0;while(g.IsTurning&&guardTurn++<2000){clear();call("TickRide",new object[]{.02f});}
    check(!g.IsTurning&&g.CompletedTurns==1&&g.CurrentStreet!="Trần Hưng Đạo","Corner "+side+": resumed route finishes on the correct new street");
   }
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);call("BeginService",null);call("TickService",new object[]{3.6f});
   var delivery=g.ActiveJob;var passenger=g.ActiveJob.female?g.femalePassenger:g.malePassenger;clear();impact(0,7,true);clear();finish();
   check(g.ActiveJob==delivery&&g.JobPhase==PhoVietGame.WorkPhase.Delivery&&passenger.parent==g.rider,"An onboard passenger and the same destination remain attached through a crash and recovery");
   foreach(bool car in new[]{false,true})foreach(bool oncoming in new[]{false,true})
   {
    reset();impact(0,7,true);clear();finish();
    int index=g.vehicles.FindIndex(v=>!v.eventOnly&&(v.halfWidth>.7f)==car);var traffic=g.SpawnTraffic(g.CurrentLane,oncoming?12:-9,index);
    traffic.speed=traffic.baseTrafficSpeed=oncoming?-14:30;traffic.model.localRotation=Quaternion.Euler(0,oncoming?180:0,0);
    for(int i=0;i<40&&g.State==PhoVietGame.RideState.Riding;i++)call("TickRide",new object[]{.025f});
    check(g.State==PhoVietGame.RideState.Riding&&g.LivesRemaining==2&&traffic.jobYieldLane>=0,"Recovery makes a "+(oncoming?"oncoming":"rear")+" "+(car?"car":"motorcycle")+" yield even when jobs are disabled");
   }
   reset();impact(0,7,true);clear();Vector3 centre=g.rider.localPosition;
   var secondary=g.SpawnCrossTraffic(1,centre.z,true,15);secondary.previousPosition=centre-Vector3.right*10;secondary.model.localPosition=centre+Vector3.right*10;secondary.motionVelocity=Vector3.right*15;
   set("fallTime",.4f);call("ResolveFallenRiderContacts",new object[]{centre});
   check(g.LivesRemaining==2&&g.RiderFallVelocity.x>0,"A second impact can physically deflect the falling rider without consuming another life in the same crash");
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exit;
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);
   g.ResetRide();Time.timeScale=scale;
  }
 }
}
