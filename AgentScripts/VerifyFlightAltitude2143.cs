using System;using System.IO;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyFlightAltitude2143
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static PhoVietGame g;static List<string> checks;
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Check(bool b,string n){if(!b)throw new Exception(n);checks.Add(n);}
 static void Fresh(){g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);}
 static void Step(float dt){Call("TickJump",dt);var p=g.rider.localPosition;p.y=g.RiderJumpHeight;g.rider.localPosition=p;Call("ResolveJumpSurface",dt);}
 static void UpSwipe(int id){g.ApplyBrowserGesture("{\"phase\":0,\"id\":"+id+",\"x\":0.55,\"y\":0.6,\"at\":10}");g.ApplyBrowserGesture("{\"phase\":1,\"id\":"+id+",\"x\":0.55,\"y\":0.3,\"action\":2,\"at\":10.1}");g.ApplyBrowserGesture("{\"phase\":2,\"id\":"+id+",\"x\":0.55,\"y\":0.3,\"at\":10.2}");}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();float scale=Time.timeScale;int best=g.BestDistance;
  bool[] flags={g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   float apex=4.1166525f,target=apex*.7f*1.2f;
   Check(Mathf.Abs(PhoVietGame.FlightHeightAboveSupport-target)<.00001f,"Flight level is old70percent level plus20percent");
   Check(Mathf.Abs(PhoVietGame.AirPickupHeight-(target+.65f))<.00001f,"Air pickups follow the higher car body");
   foreach(float speed in new[]{17f,30f,60f})foreach(float dt in new[]{1/120f,1/60f,1/30f,.05f})
   {
    Fresh();Set("Speed",speed);Check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(speed)-apex)<.00001f,"Jump apex unchanged at "+speed+" dt "+dt);
    Check(g.TryJump()&&g.Energy==PhoVietGame.MaximumEnergy-5,"Normal jump starts and costs5");float max=0;for(int i=0;i<400&&g.RiderAirborne;i++){Step(dt);max=Mathf.Max(max,g.RiderJumpHeight);}
    Check(Mathf.Abs(max-apex)<.008f&&!g.RiderFlying&&!g.RiderAirborne,"Plain jump reaches original apex and lands without flight");
   }
   foreach(float delay in new[]{0f,.15f,.4f,.9f,1.05f})foreach(float dt in new[]{1/120f,1/30f,.05f})
   {
    Fresh();UpSwipe(1);Check(g.RiderAirborne&&!g.RiderFlightPending,"First up swipe remains ordinary jump");
    for(float t=0;t<delay-.00001f;t+=dt)Step(Mathf.Min(dt,delay-t));float before=g.RiderJumpHeight,expected=Mathf.Max(before,target);
    UpSwipe(2);Check(g.FlightEngaged,"Second up swipe requests flight even during descent "+delay+" dt "+dt);
    Check(Mathf.Abs(before-g.RiderJumpHeight)<.000001f,"Request does not teleport the car");
    float largestStep=0;for(int i=0;i<300&&!g.RiderFlying;i++){float old=g.RiderJumpHeight;Step(dt);largestStep=Mathf.Max(largestStep,Mathf.Abs(g.RiderJumpHeight-old));}
    Check(g.RiderFlying&&Mathf.Abs(g.RiderJumpHeight-expected)<.001f,"Car climbs to raised flight height, preserving a higher jump position");
    Check(largestStep<.9f,"Ascent is continuous at tested frame rate");float held=g.RiderJumpHeight;Step(.1f);Check(Mathf.Abs(g.RiderJumpHeight-held)<.00001f,"Holds reached altitude");
    UpSwipe(3);Check(!g.FlightEngaged,"Third up swipe exits flight");for(int i=0;i<300&&g.RiderAirborne;i++)Step(dt);Check(!g.RiderAirborne&&g.RiderJumpHeight==0,"Descent still lands normally");
   }
   Fresh();Check(g.TryToggleFlightButton(),"Flight button lifts from ground");for(int i=0;i<120&&!g.RiderFlying;i++)Step(1/120f);
   Check(g.RiderFlying&&Mathf.Abs(g.RiderJumpHeight-target)<.001f,"Flight button uses the same raised altitude");
   Check((bool)Call("SpawnHelicopter"),"Helicopter can spawn at new flight height");
   Check(Mathf.Abs(g.HelicopterRoot.localPosition.y-(g.RiderJumpHeight+g.PlayerHeight*.5f))<.001f,"Helicopter remains level with raised car centre");
   Set("Energy",100);Set("flightEnergyClock",0f);Step(.5f);Check(g.Energy==90,"Flight drain remains10 per half-second");
   Check(g.PlayerMissiles==5&&PhoVietGame.MaximumPlayerMissiles==5,"New run starts with5 of5 missiles");
   for(int i=0;i<5;i++){if(i>0)Call("TickPlayerMissiles",.4f);Check(g.TryFirePlayerMissile()&&g.PlayerMissiles==4-i,"Fire carried missile "+(i+1));}
   Call("TickPlayerMissiles",.4f);Check(!g.TryFirePlayerMissile()&&g.PlayerMissiles==0,"Sixth shot rejected when empty");
   Fresh();Check(g.PlayerMissiles==5&&g.PlayerMissilesFired==0&&g.ActivePlayerMissileCount==0,"Retry restores5 and clears projectiles");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased&&!PhoVietGame.BoostReleased,"Jobs and boost remain locked");
   var report=new{success=true,count=checks.Count,jumpHeight=apex,oldFlightHeight=apex*.7f,newFlightHeight=target,checks};File.WriteAllText("Captures/flight2143-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.count,report.newFlightHeight};
  }
  finally{g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.recklessTrafficEnabled=flags[3];g.trafficSurprises=flags[4];g.turnExitTrafficEnabled=flags[5];g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);Time.timeScale=scale;}
 }
}
