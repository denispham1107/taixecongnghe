using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyTurnBikes2111
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks;static List<object> traces;
 static void Check(bool ok,string s){if(!ok)throw new Exception(s);checks.Add(s);}
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static PhoVietGame.TrafficVehicle[] Bikes()=>g.vehicles.Where(v=>v.active&&v.turnBikeGeneration==v.spawnGeneration).ToArray();
 static int WarningLane()=>g.TurnBikeRearWarningVisible(2)?2:g.TurnBikeRearWarningVisible(3)?3:-1;
 static void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);}
 static void Fresh(){g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=false;g.turnExitTrafficEnabled=true;g.ResetRide();g.StartRide();Clear();Set("Speed",17f);}
 static void Queue()=>Call("QueueTurnBikes");
 static void Tick(float dt)=>Call("TickTurnBikes",dt);
 static void Move(PhoVietGame.TrafficVehicle v,float dt)=>Call("TickLongitudinalVehicle",v,dt);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();traces=new List<object>();float time=Time.timeScale;
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=g.BestDistance;
  try
  {
   Time.timeScale=0;Fresh();Queue();int lane=WarningLane();
   Check(g.TurnOncomingBikeSpawns==1&&g.TurnRearBikeSpawns==0&&g.PendingTurnBikeWaves==1,"Queue adds exactly one oncoming bike, rear bike waits for warning");
   Check(lane>=2&&g.TurnBikeRearWarningRemaining(lane)==.5f&&(bool)Call("RearLaneWarningVisible",lane),"New rear warning uses existing HUD on exact lane for half a second");
   var front=Bikes().Single();Check(front.baseTrafficSpeed==-25.5f&&front.lane<2&&front.model.localPosition.z>=104&&front.model.GetComponent<UrbanRiderMotion>()!=null&&!front.isReckless,"Oncoming bike uses existing rider asset and 1.5 times player speed");
   var reserved=g.vehicles.Single(v=>!v.active&&(bool)Call("TurnBikeReserved",v));var probe=g.SpawnTraffic(3,90,g.vehicles.IndexOf(reserved));Check(probe!=reserved&&!reserved.active,"Ordinary spawn cannot steal reserved rear bike");if(probe!=null){probe.active=false;probe.model.gameObject.SetActive(false);}
   Tick(.499f);Check(g.TurnRearBikeSpawns==0&&g.TurnBikeRearWarningRemaining(lane)>0,"No rear spawn at 0.499 seconds");Tick(.001f);
   var rear=Bikes().Single(v=>v.turnBikeFromRear);Check(g.TurnRearBikeSpawns==1&&rear.lane==lane&&rear.model.localPosition.z<g.rideCamera.transform.localPosition.z-rear.halfLength,"Rear bike appears behind camera on warned lane at 0.5 seconds");
   Check(g.PendingTurnBikeWaves==0&&WarningLane()<0,"Completed pair clears queue and new warning");
   foreach(float speed in new[]{0f,8f,17f,30f,67.5f,17f})
   {
    Set("Speed",speed);Move(rear,.01f);Move(front,.01f);
    Check(Mathf.Abs(rear.speed-speed*1.5f)<.0001f&&Mathf.Abs(front.speed+speed*1.5f)<.0001f,"Both directions track live speed including zero, boost and boost end: "+speed);
   }
   Set("Speed",30f);rear.model.localPosition=new Vector3(PhoVietGame.LaneX[rear.lane],0,8);Move(rear,.01f);Check(rear.baseTrafficSpeed==45,"Rear special bike never takes ordinary post-overtake slowdown");
   int shifts=g.TurnBikeLaneChanges;front.turnBikeSwerveTimer=rear.turnBikeSwerveTimer=0;front.changingLane=rear.changingLane=false;front.model.localPosition=new Vector3(PhoVietGame.LaneX[front.lane],0,70);rear.model.localPosition=new Vector3(PhoVietGame.LaneX[rear.lane],0,-10);
   int frontLane=front.lane,rearLane=rear.lane;Move(front,.01f);Move(rear,.01f);
   Check(g.TurnBikeLaneChanges==shifts+2&&front.changingLane&&rear.changingLane&&front.lane!=frontLane&&rear.lane!=rearLane,"Both new bikes deliberately begin sudden lane changes");
   Check(front.lane<2&&rear.lane>=2&&front.changeProgress<.1f&&rear.changeProgress<.1f,"Sudden change stays within directional lanes and never teleports");
   float maxJump=0;for(int i=0;i<90;i++){float x=front.model.localPosition.x;Move(front,1f/60);maxJump=Mathf.Max(maxJump,Mathf.Abs(front.model.localPosition.x-x));}
   Check(maxJump<.3f&&float.IsFinite(front.model.localEulerAngles.y),"Existing smooth rider steering completes without one-frame jumps");
   // An ordinary bike remains independent of W, despite special bikes following the player's speed.
   var ordinary=g.vehicles.First(v=>!v.active&&!v.eventOnly&&v.halfWidth<.7f&&!(bool)Call("TurnBikeReserved",v));ordinary=g.SpawnTraffic(2,90,g.vehicles.IndexOf(ordinary));ordinary.speed=ordinary.baseTrafficSpeed=12;ordinary.hasTurned=true;
   foreach(float speed in new[]{17f,67.5f,30f}){Set("Speed",speed);Move(ordinary,.01f);Check(ordinary.speed==12&&ordinary.turnBikeGeneration!=ordinary.spawnGeneration,"Ordinary bike still keeps its own speed: "+speed);}
   foreach(float dt in new[]{1f/60,1f/30,.05f})
   {
    Fresh();Queue();float elapsed=0;while(g.TurnRearBikeSpawns==0&&elapsed<1){Tick(dt);elapsed+=dt;}
    Check(elapsed>=.5f-.00001f&&elapsed<=.5f+dt+.00001f&&g.TurnRearBikeSpawns==1,"Half-second warning at frame step "+dt);traces.Add(new{dt,warningElapsed=elapsed});
   }
   Fresh();Queue();lane=WarningLane();Tick(.2f);float left=g.TurnBikeRearWarningRemaining(lane);g.TogglePause();Tick(5);
   Check(g.TurnBikeRearWarningRemaining(lane)==left&&g.TurnRearBikeSpawns==0,"Pause freezes new half-second countdown");g.TogglePause();Set("IsTurning",true);Tick(.2f);
   Check(WarningLane()<0&&g.TurnBikeRearWarningRemaining(lane)==.5f,"A following turn hides and restarts pending warning");Set("IsTurning",false);Tick(.499f);Check(g.TurnRearBikeSpawns==0,"New frame cannot skip restarted warning");Tick(.001f);Check(g.TurnRearBikeSpawns==1,"Restarted warning resolves once");
   Fresh();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}Queue();Tick(2);
   Check(g.TurnOncomingBikeSpawns==0&&g.TurnRearBikeSpawns==0&&g.PendingTurnBikeWaves==1,"Full pool preserves pair without replacing another vehicle");
   var available=g.vehicles.Where(v=>!v.eventOnly&&v.halfWidth<.7f&&v.model.GetComponent<UrbanRiderMotion>()!=null).Take(2).ToArray();foreach(var v in available)v.active=false;Tick(.1f);lane=WarningLane();
   Check(lane>=2&&g.TurnBikeRearWarningRemaining(lane)==.5f&&g.TurnOncomingBikeSpawns==1,"Freed pool reserves a bike with a full fresh warning and spawns the other");Tick(.499f);Check(g.TurnRearBikeSpawns==0,"Pool retry still waits full warning");Tick(.001f);Check(g.TurnRearBikeSpawns==1,"Pool retry emits exactly one rear bike");
   Fresh();Queue();Tick(.5f);Queue();Tick(.5f);Check(g.TurnOncomingBikeSpawns==2&&g.TurnRearBikeSpawns==2&&g.PendingTurnBikeWaves==0,"Every turn gets its own pair without duplication");
   rear=Bikes().First(v=>v.turnBikeFromRear);rear.active=false;var reuse=g.SpawnTraffic(3,110,g.vehicles.IndexOf(rear));Check(reuse==rear&&!(bool)Call("HasTurnBike",reuse),"Recycled slot no longer follows special speed or swerves");
   Fresh();Queue();g.turnExitTrafficEnabled=false;Tick(.1f);Check(g.PendingTurnBikeWaves==0&&WarningLane()<0&&g.vehicles.All(v=>!(bool)Call("TurnBikeReserved",v)),"Disabling exits releases new reservations");
   Fresh();Queue();g.ResetRide();Check(g.PendingTurnBikeWaves==0&&g.TurnRearBikeSpawns==0&&g.TurnOncomingBikeSpawns==0&&g.vehicles.All(v=>v.turnBikeGeneration==-1),"Retry clears pair, counters and special generations");
   Fresh();Call("TryExitTraffic",true);int ordinaryLane=g.OrdinaryRearWarningVisible(2)?2:3;Call("TickOrdinaryRearWarnings",1.99f);Check(g.ExitRearSpawns==0&&g.OrdinaryRearWarningRemaining(ordinaryLane)>0,"Existing ordinary rear entry still warns for two seconds");Call("TickOrdinaryRearWarnings",.02f);Check(g.ExitRearSpawns==1,"Existing ordinary rear entry still spawns after its full old warning");
   foreach(int direction in new[]{-1,1})foreach(float dt in new[]{1f/60,.05f})foreach(bool boost in new[]{false,true})
   {
    Fresh();g.turnExitTrafficEnabled=false;int guard=0;while(g.NextJunctionDistance>24&&guard++<1000){Call("TickRide",dt);Clear();}
    int playerLane=direction<0?0:3;Set("CurrentLane",playerLane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[playerLane],0,0);g.turnExitTrafficEnabled=true;if(boost)g.TryBoost();g.MoveLane(direction);guard=0;
    while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<1000){Call("TickRide",dt);if(g.CompletedTurns==0)Clear();}
    Check(g.CompletedTurns==1&&g.TurnOncomingBikeSpawns==1&&g.TurnRearBikeSpawns==0,"Actual turn creates pair and warning: "+direction+"/"+dt+"/"+boost);
    Tick(.5f);Check(g.TurnOncomingBikeSpawns==1&&g.TurnRearBikeSpawns==1&&Bikes().Length==2,"Actual turn adds both motorcycles beyond original exit wave");
    foreach(var bike in Bikes()){float sign=bike.turnBikeFromRear?1:-1;Check(Mathf.Abs(bike.baseTrafficSpeed-sign*1.5f*g.Speed)<.001f,"Actual spawned bike uses live speed and correct direction");}
   }
   Fresh();g.riversEnabled=true;g.ResetRide();g.StartRide();Clear();Set("Speed",17f);Queue();Tick(.5f);
   Check(Bikes().All(v=>!g.OverRiver(v.model.localPosition)),"Bridge-aware pair never spawns over water");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain disabled");
   var result=new{success=true,count=checks.Count,checks,traces};File.WriteAllText("Captures/turn-bikes-2111-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.riversEnabled=flags[2];g.thunderstormEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);Set("BestDistance",best);PlayerPrefs.Save();Time.timeScale=time;}
 }
}
