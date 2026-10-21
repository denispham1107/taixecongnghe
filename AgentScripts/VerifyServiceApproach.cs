using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyServiceApproach
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
  Action<bool,int,float> reset=(pickup,lane,ahead)=>
  {
   g.jobsEnabled=true;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();g.ShowJobOffers();g.AcceptJob(0);
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
   set("jobTrafficYieldRemaining",0f);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);
   g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
   var address=new RideAddress{node=Vector2Int.zero,axis=0,offset=g.NavigationPosition.y+ahead,curb=lane==3?1:-1};
   if(pickup){g.ActiveJob.pickup=address;g.ActiveJob.dropoff=new RideAddress{node=new Vector2Int(0,6),axis=0,offset=40,curb=1};}
   else g.ActiveJob.dropoff=address;
   typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,pickup?PhoVietGame.WorkPhase.Pickup:PhoVietGame.WorkPhase.Delivery);
   call("ReplanJob",new object[]{false});
  };
  Func<int,int,float,float,PhoVietGame.TrafficVehicle> spawn=(kind,lane,z,speed)=>
  {
   PhoVietGame.TrafficVehicle v;
   if(kind>=3)v=g.SpawnReckless(kind==4);
   else {int index=g.vehicles.FindIndex(t=>!t.active&&!t.eventOnly&&(kind==0?t.halfWidth<.7f:kind==1?t.halfWidth>.7f&&t.halfLength<3:t.halfLength>3));v=g.SpawnTraffic(lane,z,index);}
   if(v==null)throw new Exception("Missing traffic kind "+kind);
   v.model.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,z);v.previousPosition=v.model.localPosition;v.model.localRotation=Quaternion.identity;
   v.lane=lane;v.speed=v.baseTrafficSpeed=speed;v.startPosition=v.model.localPosition;v.rearStartX=v.startPosition.x;v.enteringFromBehind=true;
   v.willHitWall=false;v.brakeCooldown=v.laneCooldown=10000;v.motionVelocity=Vector3.forward*speed;return v;
  };
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  try
  {
   Time.timeScale=0;
   foreach(bool pickup in new[]{true,false})foreach(int lane in new[]{0,3})foreach(int kind in new[]{0,1,2,3,4})foreach(float dt in new[]{.02f,.05f,.1f})
   {
    reset(pickup,lane,20);var v=spawn(kind,lane,-10,45);
    bool stopped=false,departed=false;float departure=0;
    for(int i=0;i<Mathf.CeilToInt(14/dt);i++)
    {
     tick(dt);
     if(g.State!=PhoVietGame.RideState.Riding)
     {
      string traffic="";foreach(var t in g.vehicles)if(t.active)traffic+=" ["+t.model.name+" p="+t.model.localPosition+" prev="+t.previousPosition+" yaw="+t.model.localEulerAngles+" half="+t.halfWidth+","+t.halfLength+" knocked="+t.knocked+" wall="+t.wallCrashed+"]";
      throw new Exception("Approach/service/departure strike: pickup="+pickup+" lane="+lane+" kind="+kind+" dt="+dt+" step="+i+" phase="+g.JobPhase+" rider="+g.rider.localPosition+" protected="+g.ServiceRearProtected+" impact="+g.LastRiderImpactSpeed+traffic);
     }
     if(g.ServiceStopped)stopped=true;
     if(stopped&&!g.ServiceStopped){departed=true;departure+=dt;if(departure>=1.9f)break;}
    }
    check(stopped&&departed,"Natural slow approach, passenger stop and departure stay safe: "+pickup+","+lane+","+kind+","+dt);
   }
   // Full lanes: the follower must brake throughout deceleration, not only after BeginService.
   foreach(bool pickup in new[]{true,false})foreach(int kind in new[]{1,4})
   {
    reset(pickup,3,20);var rear=spawn(kind,3,-10,50);
    for(int lane=0;lane<3;lane++)spawn(1,lane,-10,50);
    for(int i=0;i<180;i++){tick(.025f);if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Blocked rear approach strike: "+pickup+","+kind+","+i);}
    check(g.State==PhoVietGame.RideState.Riding,"Blocked rear approach safely brakes: "+pickup+","+kind);
   }
   foreach(bool pickup in new[]{true,false})foreach(int lane in new[]{0,3})foreach(int kind in new[]{0,1,2,3,4})foreach(int mode in new[]{0,1,2,3})
   {
    reset(pickup,lane,18);int adjacent=lane==3?2:1;var rear=spawn(kind,mode==0?adjacent:lane,-10,48);
    if(mode==0){rear.changingLane=true;rear.changeFromX=rear.model.localPosition.x;rear.targetX=PhoVietGame.LaneX[lane];rear.changeDuration=.52f;rear.changeProgress=0;rear.lane=lane;}
    if(mode==1){rear.routeDetached=true;rear.routeVelocity=Vector3.forward*48;}
    if(mode==2){rear.knocked=rear.physicalCrash=true;rear.knockVelocity=Vector3.forward*55;rear.angularVelocity=new Vector3(0,80,0);rear.crashEuler=Vector3.zero;}
    if(mode==3)g.rider.localPosition=new Vector3(PhoVietGame.LaneX[adjacent],0,0);
    bool stopped=false;float departure=0;
    for(int i=0;i<300;i++)
    {
     tick(.05f);
     if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Cut-in/detached/crash/curb-entry strike: "+pickup+","+lane+","+kind+","+mode+" step="+i);
     if(g.ServiceStopped)stopped=true;
     if(stopped&&!g.ServiceStopped){departure+=.05f;if(departure>=1.9f)break;}
    }
    check(stopped&&departure>=1.9f,"Cut-in/detached/crash/curb-entry completes service safely: "+pickup+","+lane+","+kind+","+mode);
   }
   // A real rear-end chain collision must still leave the service approach clear.
   foreach(bool pickup in new[]{true,false})foreach(int lane in new[]{0,3})
   {
    reset(pickup,lane,18);var lead=spawn(1,lane,-9,15);var ram=spawn(2,lane,-21,70);
    for(int i=0;i<100;i++){tick(.05f);if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Rear chain collision hits rider: "+pickup+","+lane+","+i);}
    check(g.TrafficCollisions>0,"A real traffic/traffic impact was exercised near the service approach: "+pickup+","+lane);
   }
   // With no job / a distant or wrong-side target, rear traffic is still ordinary collidable traffic.
   reset(true,3,50);check(!g.ServiceRearProtected,"Distant target does not grant service protection");
   reset(true,3,20);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,2);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],0,0);
   check(!g.ServiceRearProtected,"Wrong curb lane does not grant service protection");
   reset(true,3,50);spawn(1,3,0,25);call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,.025f});
   check(g.State==PhoVietGame.RideState.Falling,"Collision away from service still triggers the normal crash");
   reset(true,3,18);call("BeginService",null);spawn(1,3,0,25);call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,.025f});
   check(g.State==PhoVietGame.RideState.Falling,"Service safety comes from steering/braking, not disabling actual collision detection");
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
  }
 }
}
