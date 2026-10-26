using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifySupercarMotionFx
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;float scale=Time.timeScale;
  int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var failures=new List<string>();var traces=new List<object>();
  Action<bool,string> check=(b,s)=>{if(b)checks.Add(s);else failures.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action reset=()=>{g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  try
  {
   Time.timeScale=0;reset();var car=g.rider.GetComponent<PlayerCarVisual>();var jets=g.rider.GetComponent<CarJumpJetVisual>();
   check(car!=null&&car.visual.name.Contains("Blue Supercar"),"New received Blue Petshop Supercar is installed");
   check(g.rider.GetComponent<BikeVisualAnimator>()==null&&!g.rider.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="PV_MeshyRider"),"No old player rider/bike remains");
   check(car.size.x<2.2f&&car.size.y<1.2f&&Mathf.Abs(car.size.z-3.74f)<.001f,"Car proportions preserved and fit a lane");
   var lod=car.visual.GetComponent<LODGroup>();var skins=lod.GetLODs().Select(l=>(SkinnedMeshRenderer)l.renderers[0]).ToArray();
   check(skins.Length==3&&skins[0].sharedMesh.triangles.Length/3<=28000&&skins[2].sharedMesh.triangles.Length/3<5000,"Mobile geometry budget and three real LODs");
   check(Mathf.Abs(car.frontAxle-1.02f)<.001f&&Mathf.Abs(car.rearAxle-1.122f)<.001f,"Actual new wheelbase used for roof support and steering");
   foreach(int direction in new[]{-1,1})foreach(bool boost in new[]{false,true})foreach(float dt in new[]{1f/60,1f/30,.05f})
   {
    reset();tick(dt);if(boost)g.TryBoost();float from=g.rider.localPosition.x;int lane=g.CurrentLane+direction;g.MoveLane(direction);
    float last=from,oldYaw=0,done=-1,doneDistance=-1,startDistance=g.Distance,maxRoll=0,maxYaw=0,maxYawStep=0,maxStep=0;bool speed=true,monotonic=true;var samples=new List<object>();
    for(float t=0;t<2.8f;t+=dt)
    {
     tick(dt);car.Advance(dt);float x=g.rider.localPosition.x,yaw=g.RiderSteerYaw,roll=Mathf.Abs(Mathf.DeltaAngle(0,g.rider.localEulerAngles.z));
     monotonic&=(x-last)*direction>=-.0002f;maxStep=Mathf.Max(maxStep,Mathf.Abs(x-last));maxYaw=Mathf.Max(maxYaw,Mathf.Abs(yaw));maxYawStep=Mathf.Max(maxYawStep,Mathf.Abs(yaw-oldYaw));maxRoll=Mathf.Max(maxRoll,roll);
     float expected=Mathf.Min(17+g.DifficultyTime*.12f,30)*(g.BoostActive?PhoVietGame.BoostMultiplier:1);speed&=Mathf.Abs(expected-g.Speed)<.001f;
     if(done<0&&Mathf.Abs(x-PhoVietGame.LaneX[lane])<.001f){done=t;doneDistance=g.Distance-startDistance;}
     if(samples.Count<12&&t>=samples.Count*.24f)samples.Add(new{t,x,yaw,roll,wheel=car.WheelSteerAngle});last=x;oldYaw=yaw;
    }
    string label="direction="+direction+",W="+boost+",dt="+dt;
    check(g.State==PhoVietGame.RideState.Riding&&monotonic,"Continuous monotonic lane path: "+label);
    check(doneDistance>22&&doneDistance<25&&Mathf.Abs(last-PhoVietGame.LaneX[lane])<.001f,"Lane change follows travelled distance and finishes: "+label);
    check(speed,"No cruise/boost speed loss during lane change: "+label);
    check(maxRoll<2.6f&&maxYaw>2&&maxYaw<20&&maxYawStep<4&&maxStep<.60f,"Bounded yaw/body suspension, no motorcycle bank or snaps: "+label);
    check(Mathf.Abs(g.RiderSteerYaw)<.1f&&Mathf.Abs(Mathf.DeltaAngle(0,g.rider.localEulerAngles.y))<.1f,"Visual and collision heading settle together: "+label);
    traces.Add(new{label,done,maxRoll,maxYaw,maxYawStep,maxStep,samples});
   }
   reset();tick(.02f);g.MoveLane(1);for(int i=0;i<18;i++)tick(.02f);float before=g.rider.localPosition.x,velocity=g.RiderLateralVelocity;g.MoveLane(-1);tick(.02f);
   check(Mathf.Abs(g.rider.localPosition.x-before-velocity*.02f)<.015f,"Interrupted lane change retains current velocity rather than snapping");
   for(int i=0;i<150;i++)tick(.02f);check(g.State==PhoVietGame.RideState.Riding&&Mathf.Abs(g.rider.localPosition.x-1.55f)<.002f,"Opposite input returns smoothly to requested lane");
   reset();jets.Initialize();check(car.jumpMounts.Length==4&&jets.NozzleCount==4,"Exactly four lift nozzles");
   check(car.jumpMounts.Select(t=>Mathf.Sign(t.localPosition.x)).Distinct().Count()==2,"Lift engines occupy both sides");
   check(car.jumpMounts.All(t=>t.IsChildOf(car.visual)),"All lift sockets follow the animated car");
   check(g.TryJump(),"Space/mobile jump API accepted");jets.Simulate(.10f);check(jets.Emitting&&jets.ParticleCount>0,"Four white jets ignite during takeoff windup");
   for(int i=0;i<40;i++){tick(.02f);car.Advance(.02f);jets.Simulate(.02f);}
   check(g.RiderAirborne&&g.RiderJumpHeight>2,"Car physically rises with retained speed-based jump");
   foreach(var mount in car.jumpMounts)
   {var p=mount.GetComponentInChildren<ParticleSystem>();check(p!=null&&p.particleCount>0,"Lift jet has live smoke: "+mount.name);check(mount.position.y>g.rider.position.y&&Vector3.Dot(p.transform.forward,Vector3.down)>.97f,"Lift origin rises with car and emits downward: "+mount.name);}
   g.TogglePause();int count=jets.ParticleCount;float height=g.RiderJumpHeight;jets.Simulate(.2f);check(jets.ParticleCount==count&&g.RiderJumpHeight==height,"Pause freezes lift particles and vehicle");g.TogglePause();
   for(int i=0;i<180;i++){tick(.02f);car.Advance(.02f);jets.Simulate(.02f);}check(!jets.Emitting&&g.RiderJumpHeight==0&&g.State==PhoVietGame.RideState.Riding,"Jets fade after landing and car drives on");
   reset();check(jets.ParticleCount==0,"Restart clears all four lift trails");g.TryBoost();tick(.02f);var smoke=g.rider.GetComponent<BoostSmokeVisual>();smoke.Simulate(.2f);
   check(car.exhaustMounts.Length==2&&smoke.NozzleCount==2&&smoke.Emitting,"W ignites exactly two exhausts");
   foreach(var mount in car.exhaustMounts)check(mount.GetComponentsInChildren<ParticleSystem>().Sum(p=>p.particleCount)>0,"Live white boost plume at "+mount.name);
   check(car.exhaustMounts[0].localPosition.x*car.exhaustMounts[1].localPosition.x<0,"Boost sockets are on separate left/right exhausts");
   g.Crash();smoke.Advance();jets.Advance();check(!smoke.Emitting&&!jets.Emitting&&smoke.ParticleCount==0&&jets.ParticleCount==0,"Crash cancels boost/lift effects");
   check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Ride jobs remain locked");
   var report=new{success=failures.Count==0,count=checks.Count,failures,checks,traces};File.WriteAllText("Captures/supercar-motion-fx-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
