using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyRearWarningTiming
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks;
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static void Check(bool ok,string s){if(!ok)throw new Exception(s);checks.Add(s);}
 static void Fresh(){g.ResetRide();g.StartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();float time=Time.timeScale;
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=g.BestDistance;
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.thunderstormEnabled=g.trafficSurprises=false;g.recklessTrafficEnabled=g.turnExitTrafficEnabled=true;
   foreach(int kind in new[]{0,1,2,3,4})foreach(float dt in new[]{1f/60,1f/30,.05f})
   {
    Fresh();bool danger=kind>=3;int lane;PhoVietGame.TrafficVehicle candidate;
    if(danger){Check((bool)Call("QueueRearReckless",kind==4),"Queue dangerous rear "+kind+"/"+dt);lane=g.RearRecklessWarningLane;candidate=g.vehicles.Single(v=>v.eventOnly&&v.model==(kind==4?g.recklessTruckModel:g.recklessBikeModel));}
    else
    {
     candidate=g.vehicles.First(v=>!v.eventOnly&&(kind==0?v.halfWidth<.7f:kind==1?v.halfWidth>.7f&&v.halfLength<3:v.halfLength>3));
     foreach(var v in g.vehicles.Where(v=>!v.eventOnly&&v!=candidate)){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}
     Check((bool)Call("TryExitTraffic",true),"Reserve bike/car/truck "+kind+"/"+dt);lane=g.OrdinaryRearWarningVisible(2)?2:3;
    }
    Func<float> remaining=()=>danger?g.RearRecklessWarningRemaining:g.OrdinaryRearWarningRemaining(lane);
    Action<float> tick=t=>Call(danger?"TickRearRecklessWarning":"TickOrdinaryRearWarnings",t);
    Check(lane>=2&&lane<=3&&!candidate.active&&Mathf.Abs(remaining()-.6f)<.00001f,"Correct lane warns 0.6 seconds without creating vehicle");
    float elapsed=0;while(elapsed+dt<.59f){tick(dt);elapsed+=dt;}tick(.59f-elapsed);
    Check(!candidate.active&&remaining()>0,"No early spawn at 0.59 seconds");
    g.TogglePause();float before=remaining();Call("Update");Check(!candidate.active&&remaining()==before,"Pause freezes final warning interval");g.TogglePause();
    tick(.02f);Check(candidate.active&&candidate.enteringFromBehind&&candidate.lane==lane,"Exact warned actor/lane enters at first tick past 0.6 seconds");
    Check(candidate.model.localPosition.z<g.rideCamera.transform.localPosition.z-candidate.halfLength,"Rear entry remains outside camera");
    tick(.5f);Check((danger?g.RecklessEvents:g.ExitRearSpawns)==1,"No duplicate spawn after countdown");
   }
   foreach(bool danger in new[]{false,true})
   {
    Fresh();Call(danger?"QueueRearReckless":"TryExitTraffic",danger?(object)false:true);int lane=danger?g.RearRecklessWarningLane:g.OrdinaryRearWarningVisible(2)?2:3;
    Action<float> tick=t=>Call(danger?"TickRearRecklessWarning":"TickOrdinaryRearWarnings",t);
    Func<float> remaining=()=>danger?g.RearRecklessWarningRemaining:g.OrdinaryRearWarningRemaining(lane);
    tick(.2f);Set("IsTurning",true);tick(.2f);Check(Mathf.Abs(remaining()-.6f)<.00001f,"Turn restores full 0.6 seconds: "+danger);Set("IsTurning",false);
    tick(.59f);Check((danger?g.RecklessEvents:g.ExitRearSpawns)==0,"New road retains warning: "+danger);tick(.02f);Check((danger?g.RecklessEvents:g.ExitRearSpawns)==1,"New road spawns after 0.6 seconds: "+danger);
    Fresh();Call(danger?"QueueRearReckless":"TryExitTraffic",danger?(object)false:true);g.ResetRide();Check(g.RearRecklessWarningLane==-1&&g.OrdinaryRearWarningRemaining(2)<0&&g.OrdinaryRearWarningRemaining(3)<0,"Reset clears pending rear entries: "+danger);
   }
   Fresh();var incoming=g.SpawnReckless(false,true);Call("QueueRearReckless",true);Call("TickRearRecklessWarning",5f);
   Check(g.RearRecklessWarningLane==-1&&Mathf.Abs(g.RearRecklessWarningRemaining-.6f)<.00001f&&g.RecklessEvents==1,"Moving oncoming danger blocks countdown and rear spawn");
   incoming.model.localPosition=new Vector3(10,0,30);Call("CheckBuildingImpact",incoming);Check(incoming.knocked,"Oncoming threat really crashes before rear warning");
   Call("TickRearRecklessWarning",.59f);Check(g.RecklessEvents==1&&g.RearRecklessWarningLane>=2,"After impact, rear threat still warns for 0.6 seconds");Call("TickRearRecklessWarning",.02f);Check(g.RecklessEvents==2,"Danger rear entry follows completed warning");
   Fresh();Call("TryExitTraffic",true);int blocked=g.OrdinaryRearWarningVisible(2)?2:3;int index=0;
   var reserved=(PhoVietGame.TrafficVehicle[])typeof(PhoVietGame).GetField("rearExitReserved",F).GetValue(g);var pending=reserved[blocked-2];
   foreach(var v in g.vehicles.Where(v=>!v.eventOnly&&v!=pending)){v.active=true;v.model.localPosition=new Vector3(PhoVietGame.LaneX[blocked],0,-20-index++*3);}
   Call("TickOrdinaryRearWarnings",.61f);Check(!pending.active&&g.OrdinaryRearWarningVisible(blocked),"Occupied gap defers spawn after warning");
   foreach(var v in g.vehicles.Where(v=>v!=pending))v.active=false;Call("TickOrdinaryRearWarnings",.001f);Check(pending.active&&pending.lane==blocked,"Free gap releases same warned lane immediately without new delay");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   var report=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/rear-warning-2114-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.riversEnabled=flags[2];g.thunderstormEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();Time.timeScale=time;}
 }
}
