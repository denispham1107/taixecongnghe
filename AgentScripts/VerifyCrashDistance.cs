using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyCrashDistance
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static PhoVietGame g;static List<string> checks;
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static void Check(bool b,string s){if(!b)throw new Exception(s);checks.Add(s);}
 static void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);}
 static void Fresh(){g.ResetRide();g.StartRide();Clear();Set("Distance",42.625f);Set("BestDistance",0);Set("Speed",0f);}
 static void RearHit(float speed)
 {
  int i=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3);var v=g.SpawnTraffic(2,0,i);var p=g.rider.localPosition;
  v.speed=v.baseTrafficSpeed=speed;v.motionVelocity=Vector3.forward*speed;v.model.localRotation=Quaternion.identity;v.previousPosition=p-Vector3.forward*12;v.model.localPosition=p+Vector3.forward*12;
  Set("oldRiderZ",p.z);Call("ResolveRiderContacts",p.x,.05f);Clear();
 }
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();float time=Time.timeScale;
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   foreach(float dt in new[]{1f/60,1f/30,.05f})foreach(float speed in new[]{12f,30f,60f})
   {
    Fresh();RearHit(speed);float score=g.Distance,road=g.RoadProgress;Check(g.State==PhoVietGame.RideState.Falling&&!g.RiderFallComplete,"Real rear contact starts physical fall: "+speed+"/"+dt);
    g.TogglePause();var pose=g.rider.localPosition;Call("TickRiderCrash",2f);Check(g.Distance==score&&g.rider.localPosition==pose,"Pause preserves crash score and pose");g.TogglePause();
    bool frozen=true,earlyEnd=false;int guard=0;while(g.State==PhoVietGame.RideState.Falling&&guard++<1500){Call("TickRiderCrash",dt);frozen&=g.Distance==score;earlyEnd|=g.State==PhoVietGame.RideState.GameOver&&!g.RiderFallComplete;}
    Check(frozen&&g.Distance==42.625f&&g.BestDistance==42&&PlayerPrefs.GetInt("PhoViet.BestMetres.v1")==42,"HUD/end score and saved best exclude every metre of throw");
    Check(g.RiderFallTravel>.1f&&g.RoadProgress>road+.05f,"Physical throw and scrolling continue independently of score");
    Check(!earlyEnd&&g.State==PhoVietGame.RideState.GameOver&&g.RiderFallComplete&&g.Speed==0,"Game ends only when fall has settled");
    g.RestartRide();Check(g.Distance==0&&Mathf.Abs(g.RoadProgress-(float)typeof(PhoVietGame).GetProperty("StartStreetPhase",F).GetValue(g))<.001f,"Retry clears score and unscored travel");
   }
   Fresh();Call("StartChase");g.policeBike.localPosition=new Vector3(1.55f,0,-1000);RearHit(30);float pursuitScore=g.Distance;int n=0;while(!g.RiderFallComplete&&n++<1000)Call("TickRiderCrash",.02f);
   Check(g.State==PhoVietGame.RideState.Falling&&g.CrashDuringPursuit&&g.Distance==pursuitScore,"Pursuit keeps frozen score while waiting on ground");
   g.policeBike.localPosition=g.rider.localPosition+new Vector3(0,0,-8);n=0;while(g.State==PhoVietGame.RideState.Falling&&n++<1000)Call("TickRiderCrash",.02f);
   Check(g.State==PhoVietGame.RideState.Arrested&&g.Distance==pursuitScore,"Officer physically captures without adding score");Call("TickArrest",1.2f);Check(g.State==PhoVietGame.RideState.Jailed&&g.Distance==pursuitScore&&g.BestDistance==42,"Prison and best retain impact score");
   Fresh();Set("Speed",30f);Call("StrikeStormRider");float stormScore=g.Distance;n=0;while(g.State==PhoVietGame.RideState.Falling&&n++<1000)Call("TickRiderCrash",.02f);Check(g.KilledByLightning&&g.State==PhoVietGame.RideState.GameOver&&g.Distance==stormScore,"Lightning throw cannot add score");
   Fresh();g.riversEnabled=true;g.ResetRide();g.StartRide();Clear();foreach(var row in g.roadRows)foreach(var tile in row.tiles)tile.GetComponent<RiverBlock>().SetBridge(g.blocks.Contains(tile)&&Mathf.Abs(tile.localPosition.z+(float)typeof(PhoVietGame).GetProperty("StartStreetPhase",F).GetValue(g))<1);
   g.rider.localPosition=new Vector3(5,0,30-(float)typeof(PhoVietGame).GetProperty("StartStreetPhase",F).GetValue(g));Set("Speed",20f);Call("BeginRiverFall");float riverScore=g.Distance;n=0;while(g.State==PhoVietGame.RideState.Falling&&n++<1000)Call("TickRiderCrash",.02f);
   Check(g.RiderInRiver&&g.State==PhoVietGame.RideState.GameOver&&g.Distance==riverScore,"River fall and sinking cannot add score");for(int i=0;i<200;i++)g.TickRiverAftermath(.02f);Check(g.Distance==riverScore,"Sinking aftermath leaves final score unchanged");
   Fresh();float before=g.Distance;Call("TickRide",.1f);Check(g.Distance>before&&g.State==PhoVietGame.RideState.Riding,"Normal driving still earns distance");Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/crash-distance-2115-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.riversEnabled=flags[2];g.thunderstormEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();Time.timeScale=time;}
 }
}
