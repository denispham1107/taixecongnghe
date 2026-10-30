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
   check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(17)/oldHeight-.84975f)<.00001f,"Starting jump height is the 1.49 starting height plus exactly three percent");
   check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(30)-oldHeight*.84975f)<.00001f,"At high speed the target height remains fixed at the starting value");
   float previous=0;for(int speed=17;speed<=75;speed++){float height=PhoVietGame.JumpHeightAtSpeed(speed);check(Mathf.Abs(height-oldHeight*.84975f)<.00001f,"Height is fixed regardless of speed: "+speed);previous=height;}
   foreach(float dt in new[]{1f/60,1f/30,.05f})foreach(float speed in new[]{17f,24f,30f})foreach(bool boosted in new[]{false,true})
   {
    reset(speed);if(boosted)g.TryBoost();check(g.TryJump(),"Takeoff accepted "+speed+"/"+boosted+"/"+dt);float peak=0,launch=0,velocity=0;bool airborne=false;float oldPose=0;bool monotonic=true;
    for(int i=0;i<Mathf.CeilToInt(4.2f/dt)&&g.State==PhoVietGame.RideState.Riding;i++)
    {tick(dt);peak=Mathf.Max(peak,g.RiderJumpHeight);if(g.RiderAirborne){if(!airborne){launch=g.RiderJumpTakeoffSpeed;velocity=(float)typeof(PhoVietGame).GetField("jumpLaunchVelocity",F).GetValue(g);}monotonic&=g.RiderJumpPose>=oldPose-.0001f;oldPose=g.RiderJumpPose;airborne=true;}}
    float expected=PhoVietGame.JumpHeightAtSpeed(launch);string label=$"speed={speed},boost={boosted},dt={dt}";
    check(Mathf.Abs(velocity/Mathf.Sqrt(48.4f*oldHeight*.84975f)-1f)<.00001f,"Launch velocity always gives the same fixed height: "+label);
    check(airborne&&Mathf.Abs(peak-expected)<.015f,"Measured apex matches the fixed height: "+label);
    check(boosted?launch>speed*2.24f:launch>=speed&&launch<speed+.2f,"Takeoff uses road speed including W, without the later airborne bonus: "+label);
    check(g.State==PhoVietGame.RideState.Riding&&!g.RiderAirborne&&g.RiderJumpHeight==0,"Higher/lower jump lands normally: "+label);check(monotonic,"Air pose progresses smoothly at each jump height: "+label);
    cases.Add(new{speed,boosted,dt,launch,velocity,peak,expected});
   }
   // Launch at normal speed, then W while ascending: horizontal speed changes,
   // while vertical velocity and the stored apex remain on the original parabola.
   reset(17);g.TryJump();while(!g.RiderAirborne)tick(.01f);float frozen=g.RiderJumpApex,launchSpeed=g.RiderJumpTakeoffSpeed;for(int i=0;i<15;i++)tick(.01f);float vy=(float)typeof(PhoVietGame).GetField("jumpVelocity",F).GetValue(g);
   check(g.TryBoost(),"W can be activated during flight");tick(.01f);float next=(float)typeof(PhoVietGame).GetField("jumpVelocity",F).GetValue(g);
   check(Mathf.Abs(next-(vy-.242f))<.0001f&&g.RiderJumpApex==frozen&&g.RiderJumpTakeoffSpeed==launchSpeed,"Mid-air W preserves the current vertical trajectory and frozen takeoff speed");
   float max=0;for(int i=0;i<200;i++){tick(.01f);max=Mathf.Max(max,g.RiderJumpHeight);}check(Mathf.Abs(max-frozen)<.005f,"Mid-air W does not raise the current apex");
   check(g.TryJump(),"A subsequent jump can begin while the boost remains active");while(!g.RiderAirborne)tick(.01f);check(Mathf.Abs(g.RiderJumpApex-frozen)<.00001f&&g.RiderJumpTakeoffSpeed>38,"A boosted subsequent jump retains the same vertical apex");
   reset(17);g.TryJump();g.TryBoost();while(!g.RiderAirborne)tick(.01f);check(g.RiderJumpTakeoffSpeed>38,"W during windup is included at actual takeoff");
   reset(17);g.TryJump();while(!g.RiderAirborne)tick(.01f);while((float)typeof(PhoVietGame).GetField("jumpVelocity",F).GetValue(g)>-.5f)tick(.01f);
   float falling=(float)typeof(PhoVietGame).GetField("jumpVelocity",F).GetValue(g),fallY=g.RiderJumpHeight;tick(.05f);
   check(Mathf.Abs((float)typeof(PhoVietGame).GetField("jumpVelocity",F).GetValue(g)-(falling-1.1f))<.00001f,"Falling acceleration remains 22 m/s2");
   check(Mathf.Abs(g.RiderJumpHeight-(fallY+falling*.05f-.0275f))<.00001f,"Descending displacement retains the original gravity");
   var result=new{success=failures.Count==0,count=checks.Count,failures,checks,cases};File.WriteAllText("Captures/release150-jump-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.tankBarrageEnabled=tanks;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
