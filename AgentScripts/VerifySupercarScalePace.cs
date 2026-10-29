using System;using System.Linq;using System.Reflection;using System.IO;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifySupercarScalePace
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  var checks=new List<string>();var failures=new List<string>();var cases=new List<object>();Action<bool,string> check=(b,s)=>{if(b)checks.Add(s);else failures.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action<float> reset=speed=>{g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);set("timeRiding",(speed-17)/.36f);};
  try
  {
   Time.timeScale=0;reset(17);var car=g.rider.GetComponent<PlayerCarVisual>();car.Advance(0);
   check(Mathf.Abs(car.ModelScale-.85f)<.00001f,"Visual model is uniformly 15 percent smaller");
   check(Vector3.Distance(car.size,new Vector3(1.760576f,.914984f,3.74f))<.00001f,"Width/height/length all scale by 0.85");
   check(Mathf.Abs(g.PlayerHalfWidth-.880288f)<.00001f&&Mathf.Abs(g.PlayerHalfLength-1.87f)<.00001f&&Mathf.Abs(g.PlayerHeight-.914984f)<.00001f,"Collision volume scales with the model");
   check(Mathf.Abs(car.frontAxle-1.02f)<.00001f&&Mathf.Abs(car.rearAxle-1.122f)<.00001f&&Mathf.Abs(car.wheelRadius-.26775f)<.00001f,"Axles/rolling radius scale once, not twice");
   // Inspect socket placement in the authored rest pose, independent of the
   // Drive clip's small suspension bounce at the test's current animation clock.
   car.animationPlayer.Play("Idle");car.animationPlayer["Idle"].normalizedTime=0;car.animationPlayer.Sample();car.animationPlayer.Stop();
   var lamp=g.daylight.vehicles.Single(v=>v.root==g.rider);Vector3 head=g.rider.InverseTransformPoint(lamp.head.position);
   check(Vector3.Distance(new Vector3(Mathf.Abs(head.x),head.y,head.z),new Vector3(.6205f,.391f,1.734f))<.002f,"Headlight socket scales with the body: "+head.ToString("F5"));
   foreach(var t in car.exhaustMounts){Vector3 p=g.rider.InverseTransformPoint(t.position);check(Mathf.Abs(Mathf.Abs(p.x)-.6205f)<.002f&&Mathf.Abs(p.z+1.8105f)<.002f,"Exhaust stays on the scaled car: "+t.name);}
   foreach(var t in car.jumpMounts){Vector3 p=g.rider.InverseTransformPoint(t.position);check(Mathf.Abs(Mathf.Abs(p.x)-.6545f)<.002f&&Mathf.Abs(p.y-.1445f)<.002f,"Lift jet stays at the scaled corner: "+t.name);}
   var records=new Dictionary<string,float>();
   foreach(int direction in new[]{-1,1})foreach(float speed in new[]{17f,24f,30f})foreach(bool boost in new[]{false,true})foreach(float dt in new[]{1f/60,1f/30,.05f})
   {
    reset(speed);tick(dt);if(boost)g.TryBoost();tick(dt);float start=g.Distance,roadSpeed=g.Speed;int lane=g.CurrentLane+direction;g.MoveLane(direction);
    float age=0,last=g.rider.localPosition.x,maxStep=0,maxRoll=0;bool monotonic=true;int guard=0;
    while(Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[lane])>.001f&&g.State==PhoVietGame.RideState.Riding&&guard++<200)
    {tick(dt);car.Advance(dt);age+=dt;float x=g.rider.localPosition.x;monotonic&=(x-last)*direction>=-.0001f;maxStep=Mathf.Max(maxStep,Mathf.Abs(x-last));last=x;maxRoll=Mathf.Max(maxRoll,Mathf.Abs(Mathf.DeltaAngle(0,g.rider.localEulerAngles.z)));}
    float metres=g.Distance-start;string label=direction+"/"+speed+"/"+boost+"/"+dt;
    check(g.State==PhoVietGame.RideState.Riding&&monotonic&&guard<200,"Smooth monotonic lane path at "+label);
    check(metres>15.8f&&metres<20,"Equivalent forward travel despite different speed/FPS: "+label+",metres="+metres);
    check(Mathf.Abs(age-(22.95f/1.3915f)/roadSpeed)<dt+.035f,"Cumulative 1.3915 pace lane changes still scale with speed: "+label+",seconds="+age);
    check(maxStep<1.05f&&maxRoll<2.6f,"Fast lane changes remain bounded, without a position snap: "+label);
    string key=direction+"/"+speed+"/"+dt;if(!boost)records[key]=age;else check(age<records[key]*.55f+dt,"W speeds up the lane change approximately 2.25 times: "+label);
    cases.Add(new{direction,speed,boost,dt,age,roadSpeed,metres,maxStep,maxRoll});
   }
   // Changing speed while already changing lanes must immediately affect the
   // remaining path, without teleporting to a different point on the curve.
   foreach(float dt in new[]{1f/60,1f/30,.05f})
   {
    reset(17);tick(dt);g.MoveLane(1);float age=0;for(;age<.30f;age+=dt)tick(dt);float before=g.rider.localPosition.x;g.TryBoost();tick(dt);age+=dt;
    check(g.rider.localPosition.x>before&&g.rider.localPosition.x-before<.9f,"Mid-lane W advances continuously at dt="+dt);
    for(int i=0;i<150&&Mathf.Abs(g.rider.localPosition.x-4.65f)>.001f;i++){tick(dt);age+=dt;}
    check(Mathf.Abs(age-(.30f+(1.35f/1.3915f-.30f)/2.25f))<2*dt+.035f&&g.State==PhoVietGame.RideState.Riding,"W accelerates an already-started lane change at dt="+dt+",seconds="+age);
   }
   reset(30);g.TryBoost();tick(.02f);g.MoveLane(1);for(int i=0;i<6;i++)tick(.02f);float x0=g.rider.localPosition.x,velocity=g.RiderLateralVelocity;g.MoveLane(-1);tick(.02f);
   check(Mathf.Abs(g.rider.localPosition.x-x0-velocity*.02f)<.07f,"Opposite input preserves high-speed motion continuity");
   for(int i=0;i<150&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
   check(g.State==PhoVietGame.RideState.Riding&&Mathf.Abs(g.rider.localPosition.x-1.55f)<.001f,"High-speed opposite input settles in requested lane");
   check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Ride jobs remain locked");
   var report=new{success=failures.Count==0,count=checks.Count,failures,checks,cases};File.WriteAllText("Captures/supercar-scale-pace-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
