using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyServiceRearYield
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action<bool,int> reset=(pickup,lane)=>
  {
   g.jobsEnabled=true;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();g.ShowJobOffers();g.AcceptJob(0);clear();
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
   g.ActiveJob.dropoff=new RideAddress{node=new Vector2Int(0,6),axis=0,offset=40,curb=1};
   typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,pickup?PhoVietGame.WorkPhase.Pickup:PhoVietGame.WorkPhase.Delivery);
   call("BeginService",null);set("jobTrafficYieldRemaining",0f);
  };
  Func<int,int,float,float,PhoVietGame.TrafficVehicle> spawn=(kind,lane,z,speed)=>
  {
   PhoVietGame.TrafficVehicle v;
   if(kind>=3){v=g.SpawnReckless(kind==4);if(v==null)throw new Exception("Reckless spawn missing");}
   else {int index=g.vehicles.FindIndex(t=>!t.active&&!t.eventOnly&&(kind==0?t.halfWidth<.7f:kind==1?t.halfWidth>.7f&&t.halfLength<3:t.halfLength>3));v=g.SpawnTraffic(lane,z,index);}
   v.model.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,z);v.previousPosition=v.model.localPosition;v.model.localRotation=Quaternion.identity;
   v.lane=lane;v.speed=v.baseTrafficSpeed=speed;v.startPosition=v.model.localPosition;v.rearStartX=v.startPosition.x;v.enteringFromBehind=true;
   v.willHitWall=false;v.brakeCooldown=v.laneCooldown=10000;v.motionVelocity=Vector3.forward*speed;return v;
  };
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  try
  {
   Time.timeScale=0;
   foreach(bool pickup in new[]{true,false})
   {
    reset(pickup,3);call("TickService",new object[]{pickup?3.6f:3.2f});
    check(!g.ServiceStopped&&Mathf.Abs(g.RearDepartureProtection-2)<.001f,(pickup?"Pickup":"Dropoff")+" arms two moving seconds after passenger animation finishes");
    typeof(PhoVietGame).GetProperty("Speed").SetValue(g,0f);call("TickWorkMotion",new object[]{.5f});
    check(g.RearDepartureProtection==2,"Waiting at zero speed does not consume departure protection: "+pickup);
    tick(.5f);check(g.Speed>0&&Mathf.Abs(g.RearDepartureProtection-1.5f)<.001f,"Actual movement consumes exactly half a second: "+pickup);
    g.TogglePause();call("Update",null);check(Mathf.Abs(g.RearDepartureProtection-1.5f)<.001f,"Pause freezes departure protection: "+pickup);g.TogglePause();
    for(int i=0;i<14;i++)tick(.1f);check(g.ServiceRearProtected&&g.RearDepartureProtection>.09f,"Still protected after 1.9 moving seconds: "+pickup);
    tick(.11f);check(!g.ServiceRearProtected&&g.RearDepartureProtection==0,"Protection ends after two moving seconds: "+pickup);
   }
   foreach(bool pickup in new[]{true,false})foreach(int lane in new[]{0,3})foreach(int kind in new[]{0,1,2,3,4})foreach(float dt in new[]{.02f,.1f})
   {
    reset(pickup,lane);var v=spawn(kind,lane,-18,kind==4?38:45);float x=v.model.localPosition.x;
    int steps=Mathf.CeilToInt(((pickup?3.6f:3.2f)+1.95f)/dt);
    for(int i=0;i<steps;i++)
    {
     tick(dt);
     if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Rear strike during full service/departure: pickup="+pickup+" lane="+lane+" kind="+kind+" dt="+dt+" step="+i+" z="+v.model.localPosition.z);
    }
    check(Mathf.Abs(v.model.localPosition.x-x)>1||v.speed<=g.Speed+.1f,"Rear vehicle steers or yields throughout service and departure: "+pickup+","+lane+","+kind+","+dt);
    check(g.State==PhoVietGame.RideState.Riding,"Full TickRide stays safe across passenger completion: "+pickup+","+lane+","+kind+","+dt);
   }
   // A fresh dangerous driver must still yield when it reaches the rider after service has ended.
   foreach(bool pickup in new[]{true,false})foreach(int kind in new[]{0,1,2,3,4})
   {
    reset(pickup,3);call("TickService",new object[]{pickup?3.6f:3.2f});tick(.8f);var v=spawn(kind,3,-8,42);
    for(int i=0;i<40;i++){tick(.025f);if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("New rear entry struck during departure: "+pickup+","+kind);}
    check(g.ServiceRearProtected&&g.State==PhoVietGame.RideState.Riding,"New rear entry actively avoids during departure: "+pickup+","+kind);
   }
   foreach(bool pickup in new[]{true,false})foreach(int kind in new[]{1,4})
   {
    reset(pickup,3);var v=spawn(kind,3,-14,40);
    for(int lane=0;lane<3;lane++)spawn(1,lane,-14,40);
    for(int i=0;i<100;i++){tick(.025f);if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Blocked rear lane failed to brake: "+pickup+","+kind);}
    check(g.ServiceStopped&&g.State==PhoVietGame.RideState.Riding,"Blocked rear lanes brake instead of hitting passenger stop: "+pickup+","+kind);
   }
   // The stop can begin while a fast follower is already close, not only with distant spawns.
   foreach(bool pickup in new[]{true,false})foreach(int kind in new[]{0,1,2,3,4})
   {
    reset(pickup,3);var v=spawn(kind,3,-12,50);v.model.localPosition=new Vector3(PhoVietGame.LaneX[3],0,-v.halfLength-1.6f);v.previousPosition=v.model.localPosition;
    for(int i=0;i<30;i++){tick(.05f);if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Close rear follower strikes at passenger stop: "+pickup+","+kind+" step="+i);}
    check(g.ServiceStopped&&g.State==PhoVietGame.RideState.Riding,"Close fast follower safely steers/brakes: "+pickup+","+kind);
   }
   reset(true,3);call("TickService",new object[]{3.6f});for(int i=0;i<85;i++)tick(.025f);clear();
   var hit=spawn(1,3,0,25);call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,.025f});
   check(g.State==PhoVietGame.RideState.Falling,"After departure protection expires, actual collision still starts the ordinary crash");
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
  }
 }
}
