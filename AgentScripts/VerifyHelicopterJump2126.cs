using System;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyHelicopterJump2126
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks=new List<string>();
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static object Slot()=>((Array)Get("helicopters")).GetValue(0);
 static void SetSlot(string n,object value)=>Slot().GetType().GetField(n).SetValue(Slot(),value);
 static Vector3 SlotScroll()=>(Vector3)Slot().GetType().GetField("scroll").GetValue(Slot());
 static void Check(bool ok,string n){if(!ok)throw new Exception(n);checks.Add(n);}
 static void Fresh(){g.RestartRide();g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);}
 static void JumpStep(float dt){Call("TickJump",dt);var p=g.rider.localPosition;p.y=g.RiderJumpHeight;g.rider.localPosition=p;Call("ResolveJumpSurface",dt);}
 static void Hover(){g.TryJump();JumpStep(.4f);g.TryJump();Check(g.RiderFlying,"Fixture holds altitude after a real launch");}
 static void Packet(int phase,int id,int action,float at)=>g.ApplyBrowserGesture("{\"phase\":"+phase+",\"id\":"+id+",\"action\":"+action+",\"x\":0.5,\"y\":0.5,\"at\":"+at.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);Time.timeScale=0;
  bool sound=(bool)Get("sound");bool[] flags={g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  try
  {
   foreach(float dt in new[]{1f/20,1f/30,1f/60,1f/120})
   {
    Fresh();Check(g.TryJump()&&g.Energy==145&&g.RiderAirborne,"Jump starts immediately without a windup delay "+dt);JumpStep(dt);Check(g.RiderJumpHeight>.1f,"Wheels rise on the first control step "+dt);
    float peak=g.RiderJumpHeight,pose=g.RiderJumpPose;bool smooth=true;for(int i=0;i<Mathf.CeilToInt(1.5f/dt);i++){JumpStep(dt);peak=Mathf.Max(peak,g.RiderJumpHeight);if(g.RiderAirborne){smooth&=g.RiderJumpPose>=pose-.0001f;pose=g.RiderJumpPose;}}
    Check(Mathf.Abs(peak-4.1166525f)<.02f&&g.RiderJumpHeight==0&&!g.RiderAirborne,"Single swipe reaches unchanged full apex and lands without auto-hover "+dt);Check(smooth,"Takeoff and airborne poses have no phase reset "+dt);
    foreach(float early in new[]{0f,.03f,.12f,.20f})
    {
     Fresh();g.TryJump();if(early>0)JumpStep(early);Check(g.TryJump()&&g.RiderFlightPending&&!g.RiderFlying,"Early second gesture queues hover rather than holding near road "+dt+"/"+early);Check(!g.TryJump(),"Repeated queued input cannot toggle into low flight");
     int guard=0;while(!g.RiderFlying&&guard++<100)JumpStep(dt);Check(g.RiderFlying&&!g.RiderFlightPending&&Mathf.Abs(g.RiderJumpHeight-g.RiderJumpApex*.7f)<.0002f,"Queued flight catches exactly70percent regardless of FPS "+dt+"/"+early);
     float held=g.RiderJumpHeight;int energy=g.Energy;JumpStep(.5f);Check(g.Energy==energy-10&&g.RiderJumpHeight==held,"Billing and height remain correct after queued flight");g.TryJump();JumpStep(.1f);Check(!g.RiderFlying&&g.RiderJumpHeight<held,"Third gesture releases gravity");
    }
   }
   Fresh();Packet(0,5,0,10);Packet(1,5,2,10.1f);Packet(0,5,0,10.11f);Packet(1,5,2,10.12f);Check(g.Energy==145&&g.RiderAirborne&&!g.RiderFlightPending,"Duplicate down/move for one touch never requests flight");Packet(3,5,0,10.15f);Packet(0,6,0,10.2f);Packet(1,6,2,10.3f);Check(g.RiderFlightPending,"A separate second swipe queues flight");
   Fresh();Check(!(bool)Call("SpawnHelicopter"),"Ground driving never spawns a helicopter");g.TryJump();Check(!(bool)Call("SpawnHelicopter"),"An ordinary jump never spawns a helicopter");Fresh();Hover();float heldNow=g.RiderJumpHeight;Check(heldNow>g.RiderJumpApex*.7f,"Late second input holds actual height without snapping down");g.TogglePause();Check(!g.TryJump()&&g.RiderJumpHeight==heldNow,"Paused input cannot jump or alter flight");g.TogglePause();
   Check(g.HelicopterRoot!=null&&!g.HelicopterActive,"Pooled helicopter is ready without appearing on the ground");var pooled=g.HelicopterRoot;Check((bool)Call("SpawnHelicopter"),"Helicopter appears when player is hovering");
   Check(g.HelicopterRoot==pooled&&g.HelicoptersSpawned==1&&g.HelicopterActive,"Spawn reuses its original pooled model");Check((bool)Call("SpawnHelicopter")&&g.ActiveHelicopterCount==2&&!((bool)Call("SpawnHelicopter")),"Two helicopter slots fill without exceeding the starting limit");Check(Array.Exists(PhoVietGame.LaneX,x=>Mathf.Abs(pooled.localPosition.x-x)<.01f),"Spawn chooses one of the four road lanes");
   Check(Mathf.Abs(pooled.localPosition.y-(g.rider.localPosition.y+g.PlayerHeightForTest()))<.01f,"Helicopter is at the car body altitude");
   Vector3 before=pooled.localPosition;Call("ScrollHelicopter",2f);Call("TickHelicopter",.1f,g.rider.localPosition,true);Check(Mathf.Abs(pooled.localPosition.z-before.z+3.6f)<.0002f,"Flight moves opposite the player and scrolls with the street");
   var visual=pooled.GetComponent<HelicopterVisual>();visual.Advance(.037f);Check(visual.MainAngle!=0&&visual.TailAngle!=0&&visual.MainAngle!=visual.TailAngle&&Quaternion.Angle(visual.mainRotor.localRotation,Quaternion.identity)>1&&Quaternion.Angle(visual.tailRotor.localRotation,Quaternion.identity)>1,"Both actual rotor meshes spin independently");Check(visual.rotorAudio.clip!=null&&visual.rotorAudio.loop&&visual.rotorAudio.spatialBlend>.8f&&visual.rotorAudio.isPlaying,"Looping spatial rotor audio is playing");
   float angle=visual.MainAngle;g.TogglePause();visual.Advance(.4f);Check(visual.MainAngle==angle&&!visual.rotorAudio.isPlaying,"Pause freezes rotors and audio");g.TogglePause();visual.Advance(.1f);Check(visual.MainAngle!=angle&&visual.rotorAudio.isPlaying,"Resume restores rotors and audio");Set("sound",false);visual.Advance(.1f);Check(visual.rotorAudio.volume==0,"Sound toggle also mutes rotor loop");Set("sound",true);
   Vector3 old=pooled.localPosition,origin=new Vector3(1,0,3);Quaternion inverse=Quaternion.Euler(0,-90,0);Call("RebaseHelicopter",inverse,origin);Check(Vector3.Distance(pooled.localPosition,inverse*(old-origin))<.001f,"Turn rebase preserves the helicopter route instead of teleporting into new lanes");
   Fresh();Hover();Call("SpawnHelicopter");pooled=g.HelicopterRoot;
   Vector3 oldCar=new Vector3(20,g.rider.localPosition.y,0),turnOrigin=new Vector3(20,0,0);Quaternion turnInverse=Quaternion.Euler(0,-90,0);
   pooled.localPosition=turnOrigin+Quaternion.Inverse(turnInverse)*new Vector3(10,oldCar.y+g.PlayerHeightForTest(),0);SetSlot("velocity",Vector3.zero);
   Call("RebaseHelicopter",turnInverse,turnOrigin);g.rider.localPosition=turnInverse*(oldCar-turnOrigin);Call("TickHelicopter",.02f,oldCar,true);
   Check(!g.HitByHelicopter&&g.State==PhoVietGame.RideState.Riding,"A completed turn never sweeps the old player coordinates through a distant helicopter");
   Call("ScrollHelicopter",2f);Call("RebaseHelicopter",turnInverse,Vector3.zero);
   Check(Vector3.Distance(SlotScroll(),turnInverse*Vector3.forward*2)<.001f,"Pending road scrolling rotates with a completed turn");
   Vector3 beforeTurnScroll=pooled.localPosition;Call("TickHelicopter",.02f,g.rider.localPosition,false);
   Check(Vector3.Distance(pooled.localPosition,beforeTurnScroll-turnInverse*Vector3.forward*2)<.001f,"Rebased scroll is consumed once along its correct axis");
   foreach(bool chase in new[]{false,true})
   {
    Fresh();Hover();Call("SpawnHelicopter");pooled=g.HelicopterRoot;pooled.localPosition=new Vector3(g.rider.localPosition.x,g.rider.localPosition.y+g.PlayerHeightForTest(),15);pooled.localRotation=Quaternion.Euler(0,180,0);if(chase)Call("StartChase");
    float distance=g.Distance;Call("ScrollHelicopter",30f);Call("TickHelicopter",.1f,g.rider.localPosition,true);Check(g.HitByHelicopter&&g.State==PhoVietGame.RideState.Falling&&!g.CrashDuringPursuit&&g.RiderAirAttackBurning,"Swept helicopter contact follows ordinary crash/pursuit rules "+chase);Call("TickRiderCrash",.1f);Check(g.Distance==distance,"Helicopter impact never scores throw distance");
   }
   Fresh();Hover();Call("SpawnHelicopter");pooled=g.HelicopterRoot;var airborne=pooled.localPosition;airborne.x=g.rider.localPosition.x;airborne.z=g.rider.localPosition.z;
   Check(!(bool)Call("HelicopterTouches",airborne,Quaternion.identity,new Vector3(airborne.x,0,airborne.z),Quaternion.identity),"A ground car passes underneath a high fuselage");Check((bool)Call("HelicopterTouches",airborne,Quaternion.identity,new Vector3(airborne.x,airborne.y-g.PlayerHeightForTest(),airborne.z),Quaternion.identity),"A car at fuselage height contacts the obstacle");
   g.TryJump();pooled.localPosition=new Vector3(0,4,-40);Call("TickHelicopter",.01f,g.rider.localPosition,true);Call("TickHelicopter",.01f,g.rider.localPosition,true);Check(!g.HelicopterActive,"Descending stops new spawns while the old helicopter departs");g.RestartRide();Check(g.HelicopterRoot==pooled&&!g.HelicopterActive&&g.HelicoptersSpawned==0&&!g.HitByHelicopter,"Retry reuses the pool and clears helicopter state");
   var r=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/missile2126-regression-gameplay-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(r,Newtonsoft.Json.Formatting.Indented));return r;
  }
  finally{g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];Set("sound",sound);Set("<BestDistance>k__BackingField",best);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);Time.timeScale=time;g.ResetRide();}
 }
 // Read the existing public collision dimensions without adding a production test API.
 static float PlayerHeightForTest(this PhoVietGame game)=>(float)typeof(PhoVietGame).GetProperty("PlayerHeight",F).GetValue(game)*.5f;
}
