using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyTurnBikes2112
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks;static List<object> traces;
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add(text);}
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static PhoVietGame.TrafficVehicle[] Bikes()=>g.vehicles.Where(v=>v.active&&v.turnBikeGeneration==v.spawnGeneration).ToArray();
 static void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);}
 static void Fresh(){g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=false;g.turnExitTrafficEnabled=true;g.ResetRide();g.StartRide();Clear();Set("Speed",17f);}
 static void Queue()=>Call("QueueTurnBikes");
 static void Tick(float dt)=>Call("TickTurnBikes",dt);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();traces=new List<object>();float time=Time.timeScale;
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=g.BestDistance;
  try
  {
   Time.timeScale=0;Fresh();Queue();
   Check(g.TurnOncomingBikeSpawns==1&&g.TurnRearBikeSpawns==1&&g.PendingTurnBikeWaves==0,"Both bikes spawn in queue call, without countdown or later tick");
   Check(PhoVietGame.TurnBikeWarningSeconds==0&&!g.TurnBikeRearWarningVisible(2)&&!g.TurnBikeRearWarningVisible(3)&&g.TurnBikeRearWarningRemaining(2)<0,"No warning/countdown remains for either new bike");
   var front=Bikes().Single(v=>!v.turnBikeFromRear);var rear=Bikes().Single(v=>v.turnBikeFromRear);
   Check(front.speed==-25.5f&&rear.speed==25.5f&&front.model.GetComponent<UrbanRiderMotion>()!=null&&rear.model.GetComponent<UrbanRiderMotion>()!=null,"Existing rider models launch at 1.5 times player speed");
   Check(Mathf.Abs(rear.model.localPosition.z+8)<.001f&&rear.model.localPosition.z>g.rideCamera.transform.localPosition.z,"Rear entry is close behind player rather than hidden far behind camera");
   Check((bool)Call("TurnBikeSpace",rear,new Vector3(100,0,-8)),"Space probe remains geometry based");
   foreach(float speed in new[]{0f,8f,17f,30f,67.5f,17f})
   {
    Set("Speed",speed);Call("TickLongitudinalVehicle",rear,.01f);Call("TickLongitudinalVehicle",front,.01f);
    Check(Mathf.Abs(rear.speed-1.5f*speed)<.0001f&&Mathf.Abs(front.speed+1.5f*speed)<.0001f,"Both directions follow current speed including W/zero/resume: "+speed);
   }
   Set("Speed",30f);rear.model.localPosition=new Vector3(PhoVietGame.LaneX[rear.lane],0,8);Call("TickLongitudinalVehicle",rear,.01f);Check(rear.speed==45,"Passing player does not reduce the bike's speed");
   foreach(var v in new[]{rear,front}){v.changingLane=false;v.turnBikeSwerveTimer=0;Call("TickLongitudinalVehicle",v,.01f);}
   Check(rear.changingLane&&front.changingLane&&rear.lane>=2&&front.lane<2&&rear.changeProgress<.1f&&front.changeProgress<.1f,"Sudden lane changes remain smooth and in directional lanes");
   for(int playerLane=0;playerLane<4;playerLane++){Fresh();g.rider.localPosition=new Vector3(PhoVietGame.LaneX[playerLane],0,0);Queue();Check(Bikes().Single(v=>v.turnBikeFromRear).lane==(playerLane==3?3:2),"Rear bike chooses the closest legal same-direction lane: "+playerLane);}
   Fresh();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}Queue();Tick(1);
   Check(g.PendingTurnBikeWaves==1&&g.TurnRearBikeSpawns==0&&g.TurnOncomingBikeSpawns==0,"Full pool retains pair without replacing active models");
   foreach(var v in g.vehicles.Where(v=>!v.eventOnly&&v.halfWidth<.7f&&v.model.GetComponent<UrbanRiderMotion>()!=null).Take(2))v.active=false;Tick(.001f);
   Check(g.TurnRearBikeSpawns==1&&g.TurnOncomingBikeSpawns==1&&g.PendingTurnBikeWaves==0,"Freed slots release pair immediately without a fresh delay");
   // Try both lanes at each near-to-far rear distance before accepting a farther entry.
   Fresh();var blockers=g.vehicles.Where(v=>!v.eventOnly&&v.halfWidth>.7f).Take(2).ToArray();
   for(int i=0;i<2;i++){blockers[i].active=true;blockers[i].model.localPosition=new Vector3(PhoVietGame.LaneX[i+2],0,-8);}Queue();rear=Bikes().Single(v=>v.turnBikeFromRear);
   bool closest=true;rear.active=false;for(float z=-8;z>rear.model.localPosition.z+.01f;z-=3)for(int lane=2;lane<4;lane++)closest&=!(bool)Call("TurnBikeSpace",rear,new Vector3(PhoVietGame.LaneX[lane],0,z));rear.active=true;
   Check(closest&&rear.model.localPosition.z< -8,"Rear search never skips a closer clear lane and never overlaps blockers");
   // A blocked post-turn tank must finish spawning before the oncoming bike is allowed in.
   Fresh();g.dinosaursEnabled=g.tankBarrageEnabled=true;Set("junctionAt",g.Distance+70);foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}Call("QueueTurnHazards");Queue();
   var freeBikes=g.vehicles.Where(v=>!v.eventOnly&&v.halfWidth<.7f&&v.model.GetComponent<UrbanRiderMotion>()!=null).Take(2).ToArray();foreach(var v in freeBikes)v.active=false;Tick(.01f);
   Check(g.TurnRearBikeSpawns==1&&g.TurnOncomingBikeSpawns==0,"Rear launches immediately while incoming waits for pending tank");
   var car=g.vehicles.First(v=>!v.eventOnly&&v.model.GetComponent<MeshyCarVariants>()?.ArmoredIndex>=0);car.active=false;Call("TickTurnHazards",.01f);Tick(.01f);
   front=Bikes().Single(v=>!v.turnBikeFromRear);var tank=g.vehicles.Single(v=>v.active&&v.armored);var dino=g.dinosaurs.Single(d=>d.active);
   Check(g.TanksScheduled==1&&g.DinosaursSpawned==1&&front.model.localPosition.z<tank.model.localPosition.z-tank.halfLength-front.halfLength-5&&front.model.localPosition.z<dino.root.localPosition.z-12,"Incoming bike is on player side of newly spawned tank and dinosaur");
   Fresh();g.dinosaursEnabled=true;Set("junctionAt",g.Distance+25);Call("QueueTurnHazards");Queue();Check(g.TurnRearBikeSpawns==1&&g.TurnOncomingBikeSpawns==0&&g.PendingTurnBikeWaves==1,"Unsafe narrow gap defers only incoming bike, with no forced overlap");
   foreach(var d in g.dinosaurs.Where(d=>d.active))d.root.localPosition=new Vector3(0,0,-10);Tick(.001f);Check(g.TurnOncomingBikeSpawns==1,"Incoming retry occurs as soon as safe space exists");
   Fresh();tank=g.SpawnScheduledTank();tank.model.localPosition=new Vector3(100,0,25);Queue();Check(g.TurnOncomingBikeSpawns==1,"A tank on the old side street does not block incoming spawn on the new road");
   Fresh();dino=g.dinosaurs[0];dino.active=true;dino.root.localPosition=new Vector3(100,0,25);Queue();Check(g.TurnOncomingBikeSpawns==1,"A dinosaur off the new road cannot falsely consume its incoming gap");
   foreach(int direction in new[]{-1,1})foreach(float dt in new[]{1f/60,1f/30,.05f})foreach(bool boost in new[]{false,true})
   {
    Fresh();g.turnExitTrafficEnabled=false;int guard=0;while(g.NextJunctionDistance>24&&guard++<1000){Call("TickRide",dt);Clear();}
    int lane=direction<0?0:3;Set("CurrentLane",lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);g.turnExitTrafficEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=true;Set("tankSpawnTimer",1000f);Set("dinosaurTimer",1000f);if(boost)g.TryBoost();g.MoveLane(direction);guard=0;
    while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<1000){Call("TickRide",dt);if(g.CompletedTurns==0)Clear();}
    Check(g.CompletedTurns==1&&g.DinosaursSpawned==1&&g.TanksScheduled==1&&g.TurnOncomingBikeSpawns==1&&g.TurnRearBikeSpawns==1,"Actual turn emits hazards and both bikes immediately: "+direction+"/"+dt+"/"+boost);
    front=Bikes().Single(v=>!v.turnBikeFromRear);rear=Bikes().Single(v=>v.turnBikeFromRear);tank=g.vehicles.Single(v=>v.active&&v.armored);dino=g.dinosaurs.Single(d=>d.active);
    Check(front.model.localPosition.z<tank.model.localPosition.z-tank.halfLength-front.halfLength-5&&front.model.localPosition.z<dino.root.localPosition.z-12,"Actual turn incoming spawn clears the front of both hazards");
    float endFrameMinimum=Mathf.Max(8,g.Speed*.35f)-g.Speed*dt-.15f;
    Check(Mathf.Abs(front.speed+1.5f*g.Speed)<.001f&&Mathf.Abs(rear.speed-1.5f*g.Speed)<.001f&&g.rider.localPosition.z-rear.model.localPosition.z>=endFrameMinimum,"Actual launch uses live 1.5x speed and close safe rear gap after unused frame travel");
    float initialGap=tank.model.localPosition.z-front.model.localPosition.z;
    for(int step=0;step<(int)(.6f/dt);step++)Call("TickTrafficMotion",dt);
    Check(!front.knocked&&!front.wallCrashed&&!front.riverFalling&&tank.model.localPosition.z-front.model.localPosition.z>initialGap,"Incoming fast bike moves away from tank instead of catching its rear");
    traces.Add(new{direction,dt,boost,speed=g.Speed,initialGap,afterGap=tank.model.localPosition.z-front.model.localPosition.z,rearZ=rear.model.localPosition.z});
   }
   Fresh();Queue();g.TogglePause();int count=g.TurnOncomingBikeSpawns+g.TurnRearBikeSpawns;Queue();Tick(3);Check(g.TurnOncomingBikeSpawns+g.TurnRearBikeSpawns==count,"Pause cannot add repeated pairs");g.TogglePause();
   Fresh();Call("TryExitTraffic",true);int warned=g.OrdinaryRearWarningVisible(2)?2:3;Call("TickOrdinaryRearWarnings",1.99f);Check(g.ExitRearSpawns==0&&g.OrdinaryRearWarningRemaining(warned)>0,"Unrelated ordinary entries still warn for two seconds");Call("TickOrdinaryRearWarnings",.02f);Check(g.ExitRearSpawns==1,"Ordinary entry appears after its original warning");
   Fresh();Queue();Queue();Check(g.TurnRearBikeSpawns==2&&g.TurnOncomingBikeSpawns==2,"Repeated turns retain exactly one pair per turn");g.ResetRide();Check(g.PendingTurnBikeWaves==0&&g.TurnRearBikeSpawns==0&&g.TurnOncomingBikeSpawns==0&&g.vehicles.All(v=>v.turnBikeGeneration==-1),"Retry clears new pair state and speed tags");
   Fresh();g.riversEnabled=true;g.ResetRide();g.StartRide();Clear();Set("Speed",17f);Queue();Check(Bikes().All(v=>!g.OverRiver(v.model.localPosition)),"Bridge-aware immediate entries stay on solid road");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   var report=new{success=true,count=checks.Count,checks,traces};File.WriteAllText("Captures/turn-bikes-2112-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.riversEnabled=flags[2];g.thunderstormEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();Time.timeScale=time;}
 }
}
