using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyRideRefinements
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,reckless=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exit=g.turnExitTrafficEnabled,sound=g.sound;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Func<string,object> get=n=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;clear();};
  Action<float> step=dt=>call("TickRide",new object[]{dt});
  try
  {
   Time.timeScale=0;
   for(int i=0;i<12;i++){g.ResetRide();check(g.OpeningTrafficReduced&&g.vehicles.Count(v=>v.active)>=3&&g.vehicles.Count(v=>v.active)<=4,"Restart "+i+": opening traffic is 3–4 rather than 5–7 vehicles");}
   reset();set("spawnTimer",100f);step(1);check(Mathf.Abs((float)get("spawnTimer")-99.5f)<.001f,"Opening waves consume the spawn clock at half rate");
   set("timeRiding",9.9f);step(.05f);check(g.OpeningTrafficReduced,"Half traffic lasts through 9.95 seconds of riding");
   set("timeRiding",10f);set("spawnTimer",100f);step(.05f);check(!g.OpeningTrafficReduced&&Mathf.Abs((float)get("spawnTimer")-99.95f)<.001f,"Normal wave rate returns after ten seconds");
   reset();g.turnExitTrafficEnabled=true;call("QueueExitTraffic",null);for(int i=0;i<60;i++){call("TickExitTraffic",new object[]{.2f});call("TickOrdinaryRearWarnings",new object[]{.2f});}
   check(g.ExitRearSpawns>=2&&g.ExitRearSpawns<=3&&g.ExitOncomingSpawns>=2&&g.ExitOncomingSpawns<=3,"Opening turn exits have only 2–3 vehicles per direction instead of 3–5");set("timeRiding",10f);
   g.TogglePause();call("UpdateWork",new object[]{5f});check(!g.OpeningTrafficReduced,"A pause does not restart the opening period");
   foreach(bool car in new[]{false,true})foreach(bool oncoming in new[]{false,true})
   {
    reset();g.jobsEnabled=true;g.ShowJobOffers();check(g.AcceptJob(0)&&g.JobTrafficYieldRemaining==3,"Accepting work starts three seconds of traffic yielding");
    clear();var v=g.SpawnTraffic(2,oncoming?12:-9,g.vehicles.FindIndex(x=>!x.eventOnly&&(x.halfWidth>.7f)==car));
    v.speed=v.baseTrafficSpeed=oncoming?-14:30;v.model.localRotation=Quaternion.Euler(0,oncoming?180:0,0);
    float startX=v.model.localPosition.x;
    for(int i=0;i<40;i++)step(.025f);
    check(g.State==PhoVietGame.RideState.Riding&&Mathf.Abs(v.model.localPosition.x-startX)>2,"A "+(oncoming?"oncoming":"rear")+" "+(car?"car":"motorcycle")+" actively changes to a clear lane without hitting the rider");
    if(!oncoming)check(v.lane>=2,"Ordinary same-direction traffic stays on its own side while yielding");
    check(g.JobTrafficYieldRemaining>1.9f&&g.JobTrafficYieldRemaining<2.1f,"Yield timer uses simulated riding time");
    g.TogglePause();float before=g.JobTrafficYieldRemaining;call("UpdateWork",new object[]{5f});check(g.JobTrafficYieldRemaining==before,"Job selection/pause does not consume avoidance time");g.TogglePause();
   }
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);clear();var blocked=g.SpawnTraffic(2,-12);blocked.speed=blocked.baseTrafficSpeed=30;
   foreach(int lane in new[]{0,1,3}){var neighbour=g.SpawnTraffic(lane,-12);neighbour.speed=neighbour.baseTrafficSpeed=30;}
   call("TickJobTrafficYield",new object[]{blocked,.1f});check(blocked.jobYieldLane<0&&blocked.speed<30,"Rear traffic brakes when every alternate lane is occupied");
   call("TickWorkMotion",new object[]{3.1f});check(g.JobTrafficYieldRemaining==0,"Avoidance expires after three riding seconds");
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);clear();
   for(int lane=0;lane<4;lane++)
   {
    var v=g.SpawnTraffic(lane,6,g.vehicles.FindIndex(x=>!x.active&&!x.eventOnly&&x.halfWidth>.7f));
    v.speed=v.baseTrafficSpeed=-14;v.model.localRotation=Quaternion.Euler(0,180,0);
   }
   var threat=g.vehicles.Single(v=>v.active&&v.lane==2);float lowestPlayer=float.PositiveInfinity,lowestVehicle=float.PositiveInfinity;
   for(int i=0;i<112&&g.State==PhoVietGame.RideState.Riding;i++){step(.025f);lowestPlayer=Mathf.Min(lowestPlayer,g.Speed);lowestVehicle=Mathf.Min(lowestVehicle,Mathf.Abs(threat.speed));}
   check(g.State==PhoVietGame.RideState.Riding&&lowestPlayer<.1f,"A blocked oncoming row makes the player brake/stop before contact: "+g.State);
   check(lowestVehicle<.1f,"The blocked oncoming vehicle also stops while yielding");
   check(g.JobTrafficYieldRemaining>0&&g.JobTrafficYieldRemaining<.3f,"Automatic stopping still consumes the original three-second acceptance window");
   clear();step(.025f);check(g.Speed>0,"Player resumes when the blocked route clears");
   foreach(bool chase in new[]{false,true})
   {
    reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);clear();if(chase)call("StartChase",null);
    var hit=g.SpawnTraffic(2,0);hit.previousPosition=hit.model.localPosition;call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,.05f});
    check(g.State==PhoVietGame.RideState.Falling,"Actual contact starts a physical crash "+(chase?"during police pursuit":"during acceptance avoidance"));
   }
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);clear();g.sound=true;
   typeof(PhoVietGame).GetProperty("Speed").SetValue(g,20f);set("junctionAt",g.Distance+78);set("spokenKey","");
   var route=new CityRoute{nextTurn=-1};route.points.Add(Vector2.zero);route.points.Add(new Vector2(0,78));route.points.Add(new Vector2(-80,78));
   typeof(PhoVietGame).GetProperty("ActiveRoute").SetValue(g,route);
   call("UpdateNavigationVoice",null);check((int)get("navigationCueStage")==1&&((AudioSource)get("navigationAudio")).clip==g.navigationLeft,"Left instruction starts about four seconds before the junction");
   set("junctionAt",g.Distance+39);call("UpdateNavigationVoice",null);check((int)get("navigationCueStage")==2,"Instruction repeats at two seconds");
   call("UpdateNavigationVoice",null);check((int)get("navigationCueStage")==2,"Repeated updates do not restart an already spoken stage");
   route.nextTurn=1;set("spokenKey","");set("junctionAt",g.Distance+78);call("UpdateNavigationVoice",null);check(((AudioSource)get("navigationAudio")).clip==g.navigationRight,"Right instruction uses the same early announcement window");
   route.nextTurn=0;route.points.Clear();route.points.Add(Vector2.zero);route.points.Add(new Vector2(0,10));route.points.Add(new Vector2(0,70));route.points.Add(new Vector2(80,70));
   set("spokenKey","");set("routePlanDistance",g.Distance);set("junctionAt",g.Distance+10);call("UpdateNavigationVoice",null);
   check((int)get("navigationCueStage")==0,"Straight speech is suppressed when a following turn is less than four seconds away");
   route.points[2]=new Vector2(0,150);route.points[3]=new Vector2(80,150);set("spokenKey","");call("UpdateNavigationVoice",null);
   check((int)get("navigationCueStage")==2&&((AudioSource)get("navigationAudio")).clip==g.navigationStraight,"Straight speech remains available when the next turn is safely distant");
   g.RestartRide();check(g.JobTrafficYieldRemaining==0,"Restart clears job avoidance state");
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exit;g.sound=sound;
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;
  }
 }
}
