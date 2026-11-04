using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyHelicopterMissiles2126
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;static PhoVietGame g;static List<string> checks;
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static object Actor(int i)=>((Array)Get("helicopters")).GetValue(i);static object Missile(int i)=>((Array)Get("helicopterMissiles")).GetValue(i);
 static object Field(object o,string n)=>o.GetType().GetField(n).GetValue(o);static void Field(object o,string n,object v)=>o.GetType().GetField(n).SetValue(o,v);
 static Transform Root(int i)=>(Transform)Field(Missile(i),"root");
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 static void Fresh(){g.RestartRide();g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);}
 static void Hover(){g.TryJump();Call("TickJump",.4f);var p=g.rider.localPosition;p.y=g.RiderJumpHeight;g.rider.localPosition=p;Check(g.TryJump()&&g.RiderFlying,"Real jump and hover fixture");Set("Speed",20f);}
 static object Gun(float z=55){Call("SpawnHelicopter");var h=Actor(0);g.HelicopterRoot.localPosition=new Vector3(g.rider.localPosition.x,g.rider.localPosition.y+g.PlayerHeight*.5f,z);return h;}
 static void MissileStep(float dt,bool jump=false)
 {if(jump){Call("TickJump",dt);var p=g.rider.localPosition;p.y=g.RiderJumpHeight;g.rider.localPosition=p;Call("ResolveJumpSurface",dt);}Call("ScrollHelicopterMissiles",g.Speed*dt);Call("TickHelicopterMissiles",dt,g.rider.localPosition,true);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);bool sound=(bool)Get("sound");
  bool[] flags={g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  try
  {
   Time.timeScale=0;float[] distances={0,399.99f,400,799.99f,800,1199.99f,1200,10000};float[] periods={1,1,.8f,.8f,.6f,.6f,.5f,.5f};
   for(int i=0;i<distances.Length;i++)Check(Mathf.Abs(PhoVietGame.HelicopterMissileInterval(distances[i])-periods[i])<.0001f,"Cooldown milestone "+distances[i]);
   foreach(float dt in new[]{.05f,1f/30,1f/60})foreach(float distance in new[]{0f,400f,800f,1200f})
   {
    Fresh();Hover();Set("Distance",distance);var h=Gun();float period=PhoVietGame.HelicopterMissileInterval(distance);int steps=Mathf.RoundToInt(3/dt);float duration=steps*dt;
    for(int i=0;i<steps;i++)Call("TickHelicopterWeapon",h,dt);
    Check(g.HelicopterMissilesFired==Mathf.FloorToInt((duration+.001f)/period),"Cadence respects one projectile per interval at "+distance+" / dt="+dt+" actual="+g.HelicopterMissilesFired);
    Check(g.ActiveHelicopterMissileCount==g.HelicopterMissilesFired,"Each accepted shot creates one missile");
   }
   Fresh();Hover();var gun=Gun();Call("TickHelicopterWeapon",gun,.4f);Set("Distance",400f);Call("TickHelicopterWeapon",gun,.01f);Check(Mathf.Abs((float)Field(gun,"shotRemaining")-.39f)<.001f,"Crossing milestone preserves elapsed time and shortens current cooldown");
   g.TogglePause();float clock=(float)Field(gun,"shotRemaining");Call("TickHelicopter",2f,g.rider.localPosition,true);Check(g.HelicopterMissilesFired==0&&(float)Field(gun,"shotRemaining")==clock,"Pause never advances gun timers");g.TogglePause();
   g.TryJump();Call("TickHelicopterWeapon",gun,3f);Check(g.HelicopterMissilesFired==0,"Descent stops new shots");
   foreach(float dt in new[]{.05f,1f/30,1f/60})foreach(string evade in new[]{"none","lane","land","roll"})
   {
    Fresh();Hover();gun=Gun(evade=="roll"?25:55);Check((bool)Call("FireHelicopterMissile",gun),"Missile launches at airborne car: "+evade);
    Vector3 velocity=(Vector3)Field(Missile(0),"velocity");float score=g.Distance;
    if(evade=="lane")g.rider.localPosition+=Vector3.left*6;
    if(evade=="land")g.TryJump();if(evade=="roll")Check(g.TryBarrelRoll(),"Roll can be activated for a near missile");
    int guard=0;while(g.ActiveHelicopterMissileCount>0&&g.State==PhoVietGame.RideState.Riding&&guard++<300)MissileStep(dt,evade=="land"||evade=="roll");
    Check((Vector3)Field(Missile(0),"velocity")==velocity,"Projectile never homes after launch: "+evade+"/"+dt);
    if(evade=="none")Check(g.State==PhoVietGame.RideState.Falling&&g.HitByHelicopterMissile&&g.RiderAirAttackBurning&&g.Distance==score,"Direct missile hit falls and burns without adding score "+dt);
    else Check(g.State==PhoVietGame.RideState.Riding&&!g.HitByHelicopterMissile&&!g.RiderAirAttackBurning,"Evasion succeeds: "+evade+"/"+dt);
    if(evade=="land")Check(g.RiderJumpHeight==0&&!g.RiderFlying,"Real descent reaches the road below the projectile");
    if(evade=="roll")Check(g.HelicopterMissilesDodged==1,"A rolling near miss is counted once "+dt);
   }
   Fresh();Hover();gun=Gun(25);g.TryBarrelRoll();Call("TickJump",.95f);Check(!g.RiderRolling,"Roll protection expires with the existing animation");Call("FireHelicopterMissile",gun);for(int i=0;i<120&&g.State==PhoVietGame.RideState.Riding;i++)MissileStep(.02f);Check(g.HitByHelicopterMissile,"An expired roll grants no permanent immunity");
   Fresh();Hover();gun=Gun();var pool=Enumerable.Range(0,40).Select(Root).ToArray();
   for(int i=0;i<40;i++)Check((bool)Call("FireHelicopterMissile",gun),"Pool slot accepts one projectile "+i);
   Check(!(bool)Call("FireHelicopterMissile",gun)&&g.ActiveHelicopterMissileCount==40,"Projectile pool is bounded at40");
   Call("TickHelicopterMissiles",4f,g.rider.localPosition,false);Check(g.ActiveHelicopterMissileCount==0,"Expired missiles release all slots");Call("FireHelicopterMissile",gun);Check(Root(0)==pool[0]&&g.ActiveHelicopterMissileCount==1,"Next shot reuses the same object");
   var missile=Missile(0);Vector3 old=Root(0).localPosition,oldVelocity=(Vector3)Field(missile,"velocity"),origin=new Vector3(3,0,4);Quaternion inverse=Quaternion.Euler(0,-90,0);Call("ScrollHelicopterMissiles",2f);Call("RebaseHelicopterMissiles",inverse,origin);
   Check(Vector3.Distance(Root(0).localPosition,inverse*(old-origin))<.001f&&Vector3.Distance((Vector3)Field(missile,"velocity"),inverse*oldVelocity)<.001f&&Vector3.Distance((Vector3)Field(missile,"scroll"),inverse*Vector3.forward*2)<.001f,"Turn preserves projectile position, direction and queued scroll");
   Fresh();Hover();Set("Distance",1201f);Call("TickHelicopter",.001f,g.rider.localPosition,true);
   for(int i=0;i<5;i++){g.GetHelicopterRoot(i).localPosition=new Vector3(PhoVietGame.LaneX[i%4],g.rider.localPosition.y+g.PlayerHeight*.5f,55+i*12);Field(Actor(i),"velocity",Vector3.zero);}
   for(int i=0;i<30;i++)Call("TickHelicopter",1f/60,g.rider.localPosition,true);Check(g.HelicopterMissilesFired==5,"All five aircraft fire one projectile after half a second beyond1200m, actual="+g.HelicopterMissilesFired+"");
   foreach(bool chase in new[]{false,true})foreach(bool roll in new[]{false,true})foreach(int slot in new[]{0,4})
   {
    Fresh();Hover();Set("Distance",1201f);Call("TickHelicopter",.001f,g.rider.localPosition,true);for(int i=0;i<5;i++){g.GetHelicopterRoot(i).localPosition=new Vector3(0,4,250);Field(Actor(i),"shotRemaining",100f);}
    g.GetHelicopterRoot(slot).localPosition=new Vector3(g.rider.localPosition.x,g.rider.localPosition.y+g.PlayerHeight*.5f,6);if(chase)Call("StartChase");if(roll)g.TryBarrelRoll();float score=g.Distance;
    Call("ScrollHelicopter",12f);Call("TickHelicopter",.05f,g.rider.localPosition,true);
    Check(g.HitByHelicopter&&g.RiderAirAttackBurning&&g.BurningHelicopterCount==1&&g.State==PhoVietGame.RideState.Falling,"Body collision burns both actors, slot="+slot+" chase="+chase+" roll="+roll);
    Check(!g.IsChased&&!g.CrashDuringPursuit&&g.GetHelicopterRoot(slot).GetComponent<HelicopterVisual>().Wrecked,"Body collision stops pursuit and begins aircraft wreck physics");
    var visual=(ThunderstormVisual)Get("stormVisual");Call("RenderThunderstorm");Check(visual.BurningCount==2,"Existing baked fire/smoke stays active on car and helicopter");
    g.TogglePause();Vector3 pose=g.GetHelicopterRoot(slot).localPosition;Call("TickHelicopter",2f,g.rider.localPosition,false);Check(g.GetHelicopterRoot(slot).localPosition==pose,"Pause freezes a falling aircraft");g.TogglePause();
    int n=0;while(g.State==PhoVietGame.RideState.Falling&&n++<500){Call("TickRiderCrash",.05f);Call("TickHelicopter",.05f,g.rider.localPosition,false);}
    Check(g.State==PhoVietGame.RideState.GameOver&&g.RiderFallComplete&&g.Distance==score,"Burning car settles and ends the run, never waits for police");
    Check(g.GetHelicopterRoot(slot).localPosition.y<=1.1f,"Crashed helicopter reaches ground rather than hovering");
   }
   foreach(string action in new[]{"lane","land","roll"})
   {
    Fresh();Hover();gun=Gun(action=="roll"?25:55);Call("FireHelicopterMissile",gun);
    // Isolate this existing projectile from a later body collision or a new salvo.
    var at=g.HelicopterRoot.localPosition;at.x=30;g.HelicopterRoot.localPosition=at;
    int lane=g.CurrentLane;if(action=="lane")g.MoveLane(lane>0?-1:1);else if(action=="land")g.TryJump();else g.TryBarrelRoll();
    for(int step=0;step<70&&g.State==PhoVietGame.RideState.Riding;step++)
    {for(int i=0;i<5;i++)if(g.IsHelicopterActive(i))Field(Actor(i),"shotRemaining",100f);Call("TickRide",1f/60);}
    Check(g.State==PhoVietGame.RideState.Riding&&!g.RiderAirAttackBurning,"Actual control input and TickRide evade projectile: "+action);
    if(action=="lane")Check(g.CurrentLane!=lane,"Lane change is commanded through MoveLane, without teleporting the player");
   }
   Fresh();Hover();gun=Gun(25);Call("StartChase");Call("FireHelicopterMissile",gun);for(int i=0;i<100&&g.State==PhoVietGame.RideState.Riding;i++)MissileStep(.02f);Check(g.HitByHelicopterMissile&&!g.CrashDuringPursuit&&!g.IsChased,"Missile impact also overrides police waiting");
   g.ResetRide();Check(!g.RiderAirAttackBurning&&!g.HitByHelicopterMissile&&g.BurningHelicopterCount==0&&g.ActiveHelicopterMissileCount==0&&g.HelicopterMissilesFired==0,"Retry clears every missile, wreck and burn flag");Check(Enumerable.Range(0,40).All(i=>Root(i)==pool[i]),"Retry preserves the original bounded missile pool");
   var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/missile2126-gameplay-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally
  {g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];Set("sound",sound);Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);g.ResetRide();Time.timeScale=scale;}
 }
}
