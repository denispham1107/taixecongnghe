using System;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifySpeedJump
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  bool danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled,tanks=g.tankBarrageEnabled;var checks=new List<string>();var failures=new List<string>();var cases=new List<object>();
  Action<bool,string> check=(b,s)=>{if(b)checks.Add(s);else failures.Add(s);};Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action<float> reset=speed=>{g.RestartRide();g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=g.tankBarrageEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);set("timeRiding",Mathf.Max(0,(speed-17)/.36f));};
  try
  {
   Time.timeScale=0;const float oldHeight=14.6f*14.6f/44;
   check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(17)/oldHeight-.75f)<.00001f,"At 17 m/s the target height is exactly 75 percent of the previous jump");
   check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(30)-oldHeight)<.00001f,"At 30 m/s the previous height is recovered");
   float previous=0;for(int speed=17;speed<=75;speed++){float height=PhoVietGame.JumpHeightAtSpeed(speed);check(height>previous,"Height increases continuously with speed: "+speed);previous=height;}
   foreach(float dt in new[]{1f/60,1f/30,.05f})foreach(float speed in new[]{17f,24f,30f})foreach(bool boosted in new[]{false,true})
   {
    reset(speed);if(boosted)g.TryBoost();check(g.TryJump(),"Takeoff accepted "+speed+"/"+boosted+"/"+dt);float peak=0,launch=0,velocity=0;bool airborne=false;float oldPose=0;bool monotonic=true;
    for(int i=0;i<Mathf.CeilToInt(4.2f/dt)&&g.State==PhoVietGame.RideState.Riding;i++)
    {tick(dt);peak=Mathf.Max(peak,g.RiderJumpHeight);if(g.RiderAirborne){if(!airborne){launch=g.RiderJumpTakeoffSpeed;velocity=(float)typeof(PhoVietGame).GetField("jumpLaunchVelocity",F).GetValue(g);}monotonic&=g.RiderJumpPose>=oldPose-.0001f;oldPose=g.RiderJumpPose;airborne=true;}}
    float expected=PhoVietGame.JumpHeightAtSpeed(launch);string label=$"speed={speed},boost={boosted},dt={dt}";
    check(airborne&&Mathf.Abs(peak-expected)<.015f,"Measured apex matches takeoff speed: "+label);
    check(boosted?launch>speed*2.24f:launch>=speed&&launch<speed+.2f,"Takeoff uses road speed including W, without the later airborne bonus: "+label);
    check(g.State==PhoVietGame.RideState.Riding&&!g.RiderAirborne&&g.RiderJumpHeight==0,"Higher/lower jump lands normally: "+label);check(monotonic,"Air pose progresses smoothly at each jump height: "+label);
    cases.Add(new{speed,boosted,dt,launch,velocity,peak,expected});
   }
   // Launch at normal speed, then W while ascending: horizontal speed changes,
   // while vertical velocity and the stored apex remain on the original parabola.
   reset(17);g.TryJump();while(!g.RiderAirborne)tick(.01f);float frozen=g.RiderJumpApex,launchSpeed=g.RiderJumpTakeoffSpeed;for(int i=0;i<15;i++)tick(.01f);float vy=(float)typeof(PhoVietGame).GetField("jumpVelocity",F).GetValue(g);
   check(g.TryBoost(),"W can be activated during flight");tick(.01f);float next=(float)typeof(PhoVietGame).GetField("jumpVelocity",F).GetValue(g);
   check(Mathf.Abs(next-(vy-.22f))<.0001f&&g.RiderJumpApex==frozen&&g.RiderJumpTakeoffSpeed==launchSpeed,"Mid-air W preserves the current vertical trajectory and frozen takeoff speed");
   float max=0;for(int i=0;i<200;i++){tick(.01f);max=Mathf.Max(max,g.RiderJumpHeight);}check(Mathf.Abs(max-frozen)<.005f,"Mid-air W does not raise the current apex");
   check(g.TryJump(),"A subsequent jump can begin while the boost remains active");while(!g.RiderAirborne)tick(.01f);check(g.RiderJumpApex>frozen&&g.RiderJumpTakeoffSpeed>38,"The next jump includes the still-active speed boost");
   reset(17);g.TryJump();g.TryBoost();while(!g.RiderAirborne)tick(.01f);check(g.RiderJumpTakeoffSpeed>38,"W during windup is included at actual takeoff");
   var result=new{success=failures.Count==0,count=checks.Count,failures,checks,cases};File.WriteAllText("Captures/speed-jump-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.tankBarrageEnabled=tanks;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
