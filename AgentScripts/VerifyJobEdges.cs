using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyJobEdges
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;bool enabled=g.jobsEnabled;
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object[]> call=(s,a)=>typeof(PhoVietGame).GetMethod(s,F).Invoke(g,a);
  Action<string,object> set=(s,v)=>typeof(PhoVietGame).GetField(s,F).SetValue(g,v);
  try
  {
   Time.timeScale=0;g.jobsEnabled=true;g.RestartRide();g.ShowJobOffers();g.AcceptJob(0);
   var target=g.ActiveJob.pickup;var drop=g.ActiveJob.dropoff;int wallet=g.Coins;
   call("TurnCityJunction",new object[]{1});call("CityJunctionPrepared",null);
   check(g.ActiveJob.pickup==target&&g.ActiveJob.dropoff==drop,"Wrong turn preserves original customer and destination");
   check(g.Reroutes>0&&(g.ActiveRoute.points.Last()-target.Point).sqrMagnitude<.001f,"Wrong turn calculates a new route to the same address");
   g.RestartRide();g.ShowJobOffers();g.AcceptJob(0);
   g.ActiveJob.pickup=new RideAddress{node=Vector2Int.zero,axis=0,offset=10,curb=1};
   call("TickWorkMotion",new object[]{.01f});check(!g.ServiceStopped,"Pickup does not stop in the wrong lane");
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,3);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[3],0,0);
   call("TickWorkMotion",new object[]{.01f});check(g.JobPhase==PhoVietGame.WorkPhase.Boarding,"Correct curb starts boarding");
   check(g.malePassenger.gameObject.activeSelf!=g.femalePassenger.gameObject.activeSelf,"Exactly one randomly selected passenger appears");
   foreach(var p in new[]{g.malePassenger,g.femalePassenger})
   {
    check(p.GetComponentInChildren<Animation>().GetClipCount()==4,"Passenger has four imported animation clips");
    check(p.GetComponent<LODGroup>().GetLODs().Length==3,"Passenger has three mobile detail levels");
   }
   var index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth>.7f);if(index<0)index=g.vehicles.FindIndex(v=>!v.eventOnly);
   var car=g.SpawnTraffic(3,-8,index);car.previousPosition=new Vector3(PhoVietGame.LaneX[3],0,-8);car.model.localPosition=new Vector3(PhoVietGame.LaneX[3],0,0);
   call("YieldRearForService",new object[]{car});check(car.model.localPosition.z<=-car.halfLength-2.6f,"Rear vehicle yields before reaching a stopped pickup");
   call("TickService",new object[]{3.6f});check(g.RearDepartureProtection==2&&g.JobPhase==PhoVietGame.WorkPhase.Delivery,"Boarding starts exactly two moving seconds of departure protection");
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);
   for(int i=0;i<40;i++)call("TickRide",new object[]{.025f});check(Mathf.Abs(g.RearDepartureProtection-1)<.01f,"Departure protection counts actual movement simulation time");
   g.TogglePause();float timer=g.RearDepartureProtection;call("UpdateWork",new object[]{1f});check(g.RearDepartureProtection==timer,"Pause does not consume departure protection");g.TogglePause();
   typeof(PhoVietGame).GetProperty("IsTurning").SetValue(g,true);call("TickWorkMotion",new object[]{.5f});typeof(PhoVietGame).GetProperty("IsTurning").SetValue(g,false);
   check(Mathf.Abs(g.RearDepartureProtection-1)<.01f,"Navigation work without driving does not consume departure protection");
   for(int i=0;i<41;i++)call("TickRide",new object[]{.025f});check(!g.ServiceRearProtected,"Ordinary rear traffic returns after two moving seconds");
   set("rearDepartureProtection",2f);call("StartChase",null);car.active=true;car.model.gameObject.SetActive(true);car.previousPosition=new Vector3(g.rider.localPosition.x,0,8);car.model.localPosition=new Vector3(g.rider.localPosition.x,0,-8);
   call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,.05f});check(g.State==PhoVietGame.RideState.Falling,"Actual pursuit traffic collision still starts a crash during departure yielding");
   check(g.Coins==wallet,"An unfinished or crashed job pays no Coin");
   return new{success=true,count=checks.Count,checks};
  }
  finally{g.jobsEnabled=enabled;g.ResetRide();Time.timeScale=scale;}
 }
}
