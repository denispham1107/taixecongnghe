using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyJobs
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play required");
  bool enabled=g.jobsEnabled,reckless=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exit=g.turnExitTrafficEnabled;float scale=Time.timeScale;
  bool hadCoins=PlayerPrefs.HasKey("PhoViet.Coins.v1");int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action clean=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;};
  Action<float> tick=dt=>{clean();typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});};
  try
  {
   Time.timeScale=0;g.jobsEnabled=true;PlayerPrefs.SetInt("PhoViet.Coins.v1",0);g.RestartRide();clean();
   check(g.Coins==0,"First wallet starts at zero");
   check(PhoVietGame.FormatCoins(50)=="50"&&PhoVietGame.FormatCoins(1000)=="1,000"&&PhoVietGame.FormatCoins(1234567)=="1,234,567","Coin formatting uses comma thousands separators");
   var city=new CityNavigation(new[]{"Trần Hưng Đạo","Quang Trung","Lê Lợi","Hai Bà Trưng"},123);
   string stable=city.Street(new Vector2Int(2,3),0);check(stable==city.Street(new Vector2Int(2,-9),2),"Same vertical street keeps its name on a return journey");
   for(int h=0;h<4;h++)for(int x=-2;x<=2;x++)for(int y=-2;y<=2;y++)
   {
    var target=new RideAddress{node=new Vector2Int(x,y),axis=(x-y)&1,offset=40,curb=1};
    var route=city.Plan(Vector2.zero,CityNavigation.Directions[h],h,80,target);
    check(route.points.Count>=2&&route.distance>=0&&(route.points.Last()-target.Point).sqrMagnitude<.001f,"Routing reaches exact address without moving target: "+h+","+x+","+y);
   }
   g.ShowJobOffers();check(g.JobOffersOpen&&g.State==PhoVietGame.RideState.Paused,"Three-job selection pauses gameplay");
   check(g.JobOffers.Count==3&&g.JobOffers.Select(j=>j.pickup.Point).Distinct().Count()==3,"Three distinct random pickup positions");
   check(g.JobOffers.Select(j=>j.reward).Distinct().Count()==3&&g.JobOffers.All(j=>j.reward>=20),"Three different rewards, at least twenty Coin");
   check(g.JobOffers.Zip(g.JobOffers.Skip(1),(a,b)=>a.pickupMetres+a.tripMetres<=b.pickupMetres+b.tripMetres&&a.reward<b.reward).All(v=>v),"Longer combined pickup and passenger trips offer higher rewards");
   check(g.AcceptJob(0)&&g.State==PhoVietGame.RideState.Riding&&g.ActiveJob!=null,"Accepting a job resumes the journey");
   int reward=g.ActiveJob.reward;Vector2 pickup=g.ActiveJob.pickup.Point,drop=g.ActiveJob.dropoff.Point;
   check(g.Coins==0,"Accepting work does not pay Coin in advance");
   int guard=0;bool boarded=false,seenProtection=false,paidBeforeDrop=false;int originalReroutes=g.Reroutes;
   while(g.ActiveJob!=null&&guard++<18000)
   {
    if(g.JobPhase==PhoVietGame.WorkPhase.Delivery){boarded=true;seenProtection|=g.RearDepartureProtection>0;}
    if(!g.IsTurning&&!g.ServiceStopped)
    {
     int desired;
     if(g.ActiveRoute.direct)
     {
      Vector2 curb=g.JobPhase==PhoVietGame.WorkPhase.Pickup?g.ActiveJob.pickup.CurbDirection:g.ActiveJob.dropoff.CurbDirection;
      desired=Vector2.Dot(curb,CityNavigation.Directions[(g.NavigationHeading+1)%4])>0?3:0;
     }
     else desired=g.ActiveRoute.nextTurn<0?0:g.ActiveRoute.nextTurn>0?3:2;
     if(g.CurrentLane!=desired)g.MoveLane(Math.Sign(desired-g.CurrentLane));
     else if(!g.ActiveRoute.direct&&g.ActiveRoute.nextTurn!=0&&g.NextJunctionDistance<25&&g.NextJunctionDistance>13&&g.QueuedTurn==0&&Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[desired])<.3f)g.MoveLane(g.ActiveRoute.nextTurn);
    }
    tick(.04f);
    if(g.JobPhase==PhoVietGame.WorkPhase.Delivery)paidBeforeDrop|=g.Coins!=0;
    if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Unexpected ride state "+g.State);
   }
   check(guard<18000&&boarded&&seenProtection,"Full genuine route reaches pickup, boards, carries and drops off the customer");
   check(!paidBeforeDrop,"Coin remains unpaid throughout transport");
   check(g.Coins==reward,"Completed delivery pays exactly the quoted reward");
   int earned=g.Coins;for(int i=0;i<20;i++)tick(.04f);check(g.Coins==earned,"Payment cannot repeat after delivery");
   g.ResetRide();check(g.Coins==earned,"Coin persists through restart");
   check(g.JobPhase==PhoVietGame.WorkPhase.None&&!g.malePassenger.gameObject.activeSelf&&!g.femalePassenger.gameObject.activeSelf,"Restart clears uncompleted work and both passenger actors");
   check(g.navigationLeft.length>1&&g.navigationRight.length>1&&g.navigationStraight.length>1,"All three prerecorded female navigation phrases are present");
   return new{success=true,count=checks.Count,checks,simulationFrames=guard};
  }
  finally
  {
   if(hadCoins)PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);else PlayerPrefs.DeleteKey("PhoViet.Coins.v1");
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.jobsEnabled=enabled;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exit;g.ResetRide();Time.timeScale=scale;
  }
 }
}
