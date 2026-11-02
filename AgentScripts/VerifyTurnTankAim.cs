using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyTurnTankAim
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 sealed class Roll:System.Random {readonly double value;public Roll(double v){value=v;}public override double NextDouble()=>value;}
 static PhoVietGame g;static List<string> checks;static List<object> samples;
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add(text);}
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Clear(){Call("ClearShells");foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);}
 static void Fresh(){g.dinosaursEnabled=g.riversEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.tankBarrageEnabled=true;g.ResetRide();g.StartRide();Clear();Set("tankSpawnTimer",1000f);Set("nextDistanceTank",float.PositiveInfinity);Set("Speed",17f);}
 static PhoVietGame.TrafficVehicle TurnTank(){Fresh();g.dinosaursEnabled=true;Set("junctionAt",g.Distance+70);Call("QueueTurnHazards");return g.vehicles.First(v=>v.active&&v.armored);}
 static PhoVietGame.TankShell Fire(PhoVietGame.TrafficVehicle v)=>(PhoVietGame.TankShell)Call("FireAutomaticTankShell",v);
 static void Release()=>Call("ClearShells");
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();samples=new List<object>();
  float time=Time.timeScale;bool[] flags={g.dinosaursEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled,g.tankBarrageEnabled};int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  try
  {
   Time.timeScale=0;Check(PhoVietGame.TurnTankAimChance==.7f,"Configured probability is 70 percent");
   var tank=TurnTank();Vector3 anchor=tank.turnTankTarget;
   Check(tank.turnTankAimGeneration==tank.spawnGeneration,"Post-turn tank marked only after actual successful spawn");
   Check(Vector3.Distance(anchor,new Vector3(PhoVietGame.LaneX[3],0,46))<.001f,"Anchor is rightmost lane 24 m before requested junction, closer to the player");
   foreach(double roll in new[]{0,.699999,.7,.999999})
   {
    Set("turnTankAimRandom",new Roll(roll));var shot=Fire(tank);bool bias=roll<PhoVietGame.TurnTankAimChance;
    Check(shot!=null&&(bias?Vector3.Distance(shot.target,anchor)<.001f:Mathf.Abs(shot.target.z-(g.rider.localPosition.z+17*PhoVietGame.ShellFlight))<.001f),"Probability boundary and target: "+roll);
    Check(shot.marker.gameObject.activeSelf&&Vector3.Distance(shot.marker.transform.localPosition,shot.target+Vector3.up*.035f)<.001f,"Warning ring matches final impact point: "+roll);
    Check(shot.flight==PhoVietGame.ShellFlight&&shot.start==g.TankMuzzle(tank),"Original flight time and muzzle retained: "+roll);Release();
   }
   foreach(int seed in new[]{1,110,2026,9182})
   {
    var reference=new System.Random(seed);Set("turnTankAimRandom",new System.Random(seed));Set("rng",new System.Random(seed+50));var oldRng=new System.Random(seed+50);int hit=0,expected=0;bool fallback=true,warning=true;
    for(int i=0;i<500;i++)
    {
     int lane=oldRng.Next(4);bool bias=reference.NextDouble()<PhoVietGame.TurnTankAimChance;var shot=Fire(tank);if(bias)expected++;
     bool actual=Vector3.Distance(shot.target,anchor)<.001f;if(actual)hit++;
     fallback&=actual==bias&&(actual||Mathf.Abs(shot.target.x-PhoVietGame.LaneX[lane])<.001f);
     warning&=shot.flight==1.6f&&shot.marker.gameObject.activeSelf;Release();
    }
    Check(hit==expected&&fallback,"Independent 70/30 rolls and unchanged fallback lane sequence: seed "+seed);
    Check(hit>=300&&hit<=400&&warning,"Observed distribution and existing warnings/flight: seed "+seed);samples.Add(new{seed,shots=500,preferred=hit,percent=hit/5f});
   }
   foreach(float speed in new[]{17f,30f,67.5f})
   {
    Set("Speed",speed);Set("turnTankAimRandom",new Roll(0));var shot=Fire(tank);
    Check(Vector3.Distance(shot.target,anchor)<.001f,"Preferred area does not home onto speed/player at "+speed);Release();
   }
   Set("Speed",17f);Set("turnTankAimRandom",new Roll(0));var flying=Fire(tank);Vector3 previous=flying.target;Call("ScrollTankFire",7f);
   Check(Vector3.Distance(tank.turnTankTarget,anchor-Vector3.forward*7)<.001f&&Vector3.Distance(flying.target,previous-Vector3.forward*7)<.001f,"Anchor and airborne target scroll by same road displacement");
   anchor=tank.turnTankTarget;Release();g.rider.localPosition+=new Vector3(-3,0,4);tank.model.localPosition-=Vector3.forward*15;
   Check(Vector3.Distance(Fire(tank).target,anchor)<.001f,"Later shots retain physical area after tank passes its original firing line");Release();
   // Reused pool slots are identified by spawnGeneration, never by a lingering boolean.
   int index=g.vehicles.IndexOf(tank);tank.active=false;var reused=g.SpawnTraffic(3,75,index);
   Check(reused==tank&&tank.turnTankAimGeneration!=tank.spawnGeneration,"Recycled traffic slot invalidates special aiming generation");
   Check(!(bool)Call("HasTurnTankAim",tank),"Reused ordinary vehicle cannot inherit post-turn aim");
   Fresh();tank=g.SpawnScheduledTank();tank.model.localPosition=new Vector3(PhoVietGame.LaneX[0],0,100);Set("turnTankAimRandom",new Roll(0));
   Check(!(bool)Call("HasTurnTankAim",tank),"Nine-second/standard tank factory has no turn aiming tag");
   bool ordinary=true;var laneRng=new System.Random(41);Set("rng",new System.Random(41));
   for(int i=0;i<32;i++){int lane=laneRng.Next(4);var s=Fire(tank);ordinary&=s.lane==lane&&Mathf.Abs(s.target.z-(g.rider.localPosition.z+17*1.6f))<.001f;Release();}
   Check(ordinary,"Standard tank uses exact old random lane and predicted landing point");
   Fresh();Set("nextDistanceTank",400f);Set("Distance",400.01f);Call("TickDistanceTankSchedule",.01f);tank=g.vehicles.First(v=>v.active&&v.armored);
   Check(g.DistanceTanksScheduled==1&&!(bool)Call("HasTurnTankAim",tank),"400 m tank does not acquire post-turn aiming");
   Fresh();Set("turnTankAimRandom",new Roll(0));g.TogglePause();Call("TickTankFire",1f);Check(g.ShellsFired==0,"Pause cannot fire special shells");g.TogglePause();
   tank=TurnTank();g.ResetRide();Check(g.vehicles.All(v=>v.turnTankAimGeneration==-1&&v.turnTankTarget==Vector3.zero)&&g.shells.Count==0,"Retry clears aim tags, anchors and airborne shells");
   // Real left/right turns at normal/boost speed and low/high frame rate.
   foreach(int direction in new[]{-1,1})foreach(float dt in new[]{1f/60,.05f})foreach(bool boost in new[]{false,true})
   {
    Fresh();g.tankBarrageEnabled=false;int guard=0;while(g.NextJunctionDistance>24&&guard++<1000){Call("TickRide",dt);Clear();}
    int lane=direction<0?0:3;Set("CurrentLane",lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
    g.tankBarrageEnabled=g.dinosaursEnabled=true;Set("dinosaurTimer",1000f);Set("tankSpawnTimer",1000f);if(boost)g.TryBoost();g.MoveLane(direction);guard=0;
    while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<1000){Call("TickRide",dt);if(g.CompletedTurns==0)Clear();}
    Check(g.CompletedTurns==1&&g.State==PhoVietGame.RideState.Riding,"Real turn completes: "+direction+"/"+dt+"/"+boost);
    tank=g.vehicles.First(v=>v.active&&v.armored);float z=g.NextJunctionDistance-PhoVietGame.TurnTankAimJunctionOffset;
    Check(tank.turnTankAimGeneration==tank.spawnGeneration&&Mathf.Abs(tank.turnTankTarget.z-z)<.1f&&tank.turnTankTarget.x==PhoVietGame.LaneX[3],"Real turn anchors correct new-road junction/right lane");
    bool allowed=(bool)Call("TankCanFire",tank);Set("turnTankAimRandom",new Roll(0));Call("TickTankFire",.401f);
    Check(allowed?g.shells.Count==1&&Vector3.Distance(g.shells[0].target,tank.turnTankTarget)<.001f:g.shells.Count==0,"Automatic preferred shot preserves original range/speed gate: "+direction+"/"+dt+"/"+boost);
   }
   // The fixed area still uses ordinary explosion/crater and player damage code.
   tank=TurnTank();Set("turnTankAimRandom",new Roll(0));var groundShot=Fire(tank);Vector3 ground=groundShot.target;int craters=g.CratersCreated;Call("TickShells",1.601f);
   Check(g.CratersCreated==craters+1&&g.ShellBlasts==1&&g.shells.Count==0,"Preferred ground strike makes existing crater and releases shell/marker");
   Check(Vector3.Distance(ground,tank.turnTankTarget)<.001f,"Preferred crater impact stays at announced fixed road point");
   tank=TurnTank();Set("turnTankAimRandom",new Roll(0));var impact=Fire(tank);int blasts=g.ShellBlasts;g.rider.localPosition=impact.target;Call("TickShells",1.601f);
   Check(g.ShellBlasts==blasts+1&&g.KilledByShell&&g.State==PhoVietGame.RideState.Falling,"Preferred impact explodes and applies existing player loss");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Ride jobs remain locked");
   var report=new{success=true,count=checks.Count,checks,samples};File.WriteAllText("Captures/turn-tank-aim-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.riversEnabled=flags[1];g.thunderstormEnabled=flags[2];g.recklessTrafficEnabled=flags[3];g.trafficSurprises=flags[4];g.turnExitTrafficEnabled=flags[5];g.tankBarrageEnabled=flags[6];g.ResetRide();PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);Set("BestDistance",best);PlayerPrefs.Save();Time.timeScale=time;}
 }
}
