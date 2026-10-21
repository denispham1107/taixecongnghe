using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyRearWarningsService
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exit=g.turnExitTrafficEnabled;float time=Time.timeScale;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> warning=dt=>call("TickRearRecklessWarning",new object[]{dt});
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.recklessTrafficEnabled=true;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;clear();};
  Action<int> lane=n=>{typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,n);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[n],0,0);};
  try
  {
   Time.timeScale=0;var lanes=new HashSet<int>();
   foreach(bool truck in new[]{false,true})for(int run=0;run<8;run++)
   {
    reset();check((bool)call("QueueRearReckless",new object[]{truck}),"Queue one rear threat: "+truck+","+run);
    int selected=g.RearRecklessWarningLane;lanes.Add(selected);
    check(selected==2||selected==3,"Warning selects only a same-direction lane");
    warning(1.99f);check(g.RecklessEvents==0&&g.RearRecklessWarningRemaining>0,"No rear spawn before full two-second warning");
    warning(.02f);var v=g.vehicles.Single(x=>x.active&&x.isReckless);
    check(v.lane==selected&&Mathf.Abs(v.model.localPosition.x-PhoVietGame.LaneX[selected])<.001f,"Rear spawn uses the exact warned lane");
    check(v.enteringFromBehind&&v.model.localPosition.z<g.rideCamera.transform.localPosition.z-v.halfLength,"Whole rear entry begins behind camera");
    check(!(bool)call("QueueRearReckless",new object[]{!truck})&&g.SpawnReckless(!truck)==null,"Cannot overlap a moving rear wave");
   }
   check(lanes.Count==2,"Rear warnings randomly cover both same-direction lanes across runs");
   foreach(bool wall in new[]{false,true})
   {
    reset();var incoming=g.SpawnReckless(false,true);call("QueueRearReckless",new object[]{true});
    warning(5);check(g.RearRecklessWarningLane==-1&&g.RearRecklessWarningRemaining==2&&g.RecklessEvents==1,"Moving oncoming danger blocks warning and rear entry");
    if(wall){incoming.model.localPosition=new Vector3(10,0,30);call("CheckBuildingImpact",new object[]{incoming});}
    else
    {
     incoming.model.localPosition=new Vector3(0,0,40);incoming.previousPosition=new Vector3(-2,0,42);incoming.motionVelocity=new Vector3(10,0,-20);
     var target=g.SpawnTraffic(1,39);target.model.localPosition=new Vector3(.8f,0,39);target.previousPosition=target.model.localPosition;target.speed=0;target.motionVelocity=Vector3.zero;
     call("ResolveVehicleCollisions",null);
    }
    check(incoming.knocked,"Oncoming blocker really hit "+(wall?"a building":"another vehicle"));
    warning(1);check(g.RearRecklessWarningLane>=2&&g.RecklessEvents==1,"Warning starts only after confirmed impact");
    warning(1.01f);check(g.RecklessEvents==2&&g.vehicles.Count(v=>v.active&&v.isReckless&&!v.knocked&&v.baseTrafficSpeed>0)==1,"One rear entry follows completed warning after impact");
   }
   foreach(int side in new[]{-1,1})
   {
    reset();g.recklessTrafficEnabled=false;int guard=0;
    while(g.NextJunctionDistance>24&&guard++<400)call("TickRide",new object[]{.02f});
    lane(side<0?0:3);g.MoveLane(side);guard=0;
    while(!g.IsTurning&&guard++<200)call("TickRide",new object[]{.02f});
    check(g.IsTurning,"Real turn begins: "+side);g.recklessTrafficEnabled=true;call("QueueRearReckless",new object[]{false});guard=0;
    while(g.IsTurning&&guard++<300)call("TickRide",new object[]{.02f});
    check(!g.IsTurning&&g.RecklessEvents==0&&g.RearRecklessWarningRemaining==2,"Turn does not bypass or consume rear warning: "+side);
    warning(1.99f);check(g.RecklessEvents==0,"Full warning remains after entering new road: "+side);
    warning(.02f);check(g.RecklessEvents==1,"Single rear threat enters after turn and warning: "+side);
   }
   reset();call("QueueRearReckless",new object[]{false});g.TogglePause();float before=g.RearRecklessWarningRemaining;call("Update",null);
   check(g.RearRecklessWarningRemaining==before&&g.RecklessEvents==0,"Pause freezes warning countdown");g.ResetRide();
   check(g.RearRecklessWarningLane==-1&&g.RearRecklessWarningRemaining==0,"Restart clears warning and reservation");
   foreach(int stopLane in new[]{0,3})foreach(int kind in new[]{0,1,2})foreach(bool front in new[]{false,true})
   {
    reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);clear();lane(stopLane);
    typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,PhoVietGame.WorkPhase.Delivery);call("BeginService",null);set("jobTrafficYieldRemaining",0f);
    check(g.ServiceStopped&&g.JobPhase==PhoVietGame.WorkPhase.Alighting,"Alighting protection is independent of acceptance timer");
    int index=g.vehicles.FindIndex(v=>!v.eventOnly&&(kind==0?v.halfWidth<.7f:kind==1?v.halfWidth>.7f&&v.halfLength<3:v.halfLength>3));
    var traffic=g.SpawnTraffic(stopLane,front?14:-14,index);traffic.speed=traffic.baseTrafficSpeed=front?-14:24;traffic.model.localRotation=Quaternion.Euler(0,front?180:0,0);float startX=traffic.model.localPosition.x;
    for(int i=0;i<150;i++)
    {
     call("TickTrafficMotion",new object[]{.025f});
     call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,.025f});
     if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Alighting struck: "+stopLane+","+kind+","+front);
    }
    check(g.State==PhoVietGame.RideState.Riding&&(Mathf.Abs(traffic.model.localPosition.x-startX)>1||Mathf.Abs(traffic.speed)<.1f),"Bike/car/truck steers or yields throughout alighting: "+stopLane+","+kind+","+front);
   }
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);clear();lane(3);typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,PhoVietGame.WorkPhase.Delivery);call("BeginService",null);
   var crossing=g.SpawnCrossTraffic(1,0,true,12);crossing.model.localPosition=new Vector3(-5,0,0);crossing.previousPosition=crossing.model.localPosition;crossing.crossDelay=0;crossing.turnChoice=0;
   for(int i=0;i<120;i++){call("TickTrafficMotion",new object[]{.025f});call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,.025f});}
   check(g.State==PhoVietGame.RideState.Riding,"Cross traffic holds instead of striking an alighting rider");
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exit;g.ResetRide();Time.timeScale=time;
  }
 }
}
