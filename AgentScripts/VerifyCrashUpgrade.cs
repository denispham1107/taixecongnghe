using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyCrashUpgrade
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);typeof(PhoVietGame).GetProperty("Speed").SetValue(g,0f);};
  Action<float> tick=dt=>call("TickTrafficMotion",new object[]{dt});
  Func<bool,bool,PhoVietGame.TrafficVehicle> spawn=(car,oncoming)=>{int i=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&(v.halfWidth>.7f)==car);var v=g.SpawnTraffic(oncoming?1:2,40,i);v.speed=v.baseTrafficSpeed=oncoming?-9:9;v.motionVelocity=Vector3.forward*v.speed;v.laneCooldown=v.brakeCooldown=1000;return v;};
  Action<PhoVietGame.TrafficVehicle> hit=v=>call("ActivateCollisionBody",new object[]{v,v.motionVelocity,new Vector3(.5f,0,v.speed*.25f),new Vector2(100,100),Vector2.zero,false,!v.knocked,12f});
  try
  {
   Time.timeScale=0;
   foreach(bool car in new[]{false,true})foreach(bool opposing in new[]{false,true})
   {
    reset();var v=spawn(car,opposing);float original=v.speed;hit(v);
    for(int i=0;i<149;i++)tick(.02f);
    check(v.knocked&&!v.IsStandingUp,"Bike/car "+car+" opposing "+opposing+": stays fallen for complete three-second wait");
    tick(.04f);check(v.IsStandingUp,"Bike/car "+car+" opposing "+opposing+": starts standing after three seconds");
    for(int i=0;i<40;i++)tick(.02f);
    check(!v.knocked&&v.recovery==null&&Mathf.Sign(v.speed)==Mathf.Sign(original),"Bike/car "+car+" opposing "+opposing+": resumes original traffic direction");
    check(v.lane>=(opposing?0:2)&&v.lane<=(opposing?1:3)&&Mathf.Abs(v.model.localPosition.x-PhoVietGame.LaneX[v.lane])<.02f,"Bike/car "+car+" opposing "+opposing+": rejoins correct nearest lane");
    check(Quaternion.Angle(v.model.localRotation,Quaternion.Euler(0,opposing?180:0,0))<1,"Bike/car "+car+" opposing "+opposing+": upright and facing original heading");
   }
   reset();var repeated=spawn(false,false);hit(repeated);for(int i=0;i<100;i++)tick(.02f);hit(repeated);for(int i=0;i<100;i++)tick(.02f);
   check(repeated.knocked&&!repeated.IsStandingUp,"A second impact restarts the full three-second wait");for(int i=0;i<90;i++)tick(.02f);check(!repeated.knocked,"Repeated-hit vehicle eventually recovers after the latest impact");
   reset();var posing=spawn(false,false);var animator=posing.model.GetComponent<BikeVisualAnimator>();animator.Initialize();var sample=typeof(BikeVisualAnimator).GetMethod("Sample",F);
   sample.Invoke(animator,new object[]{"Ride",.25f});var pose=animator.visual.GetComponentsInChildren<Transform>(true);var rotations=pose.Select(t=>t.localRotation).ToArray();var positions=pose.Select(t=>t.localPosition).ToArray();
   hit(posing);posing.recovery.phase=1;posing.recovery.standAge=.32f;animator.Advance(.01f);posing.knocked=false;posing.recovery=null;animator.Advance(0);sample.Invoke(animator,new object[]{"Ride",.25f});
   var changed=pose.Select((t,i)=>new{t.name,angle=Quaternion.Angle(t.localRotation,rotations[i]),distance=Vector3.Distance(t.localPosition,positions[i])}).Where(p=>p.angle>=.1f||p.distance>=.001f).ToArray();
   check(changed.Length==0,"Returning to Ride clears all unkeyed recovery-bone poses, including raised arms: "+string.Join(",",changed.Select(p=>p.name+" angle="+p.angle+" move="+p.distance)));
   reset();var lateHit=spawn(false,false);hit(lateHit);for(int i=0;i<155;i++)tick(.02f);check(lateHit.IsStandingUp,"Standing animation is active before interruption");hit(lateHit);tick(.1f);check(lateHit.knocked&&!lateHit.IsStandingUp&&lateHit.recovery.age<.2f,"Collision while standing interrupts recovery and restarts the timer");
   reset();var behind=spawn(true,false);behind.model.localPosition=new Vector3(1.55f,0,-25);hit(behind);call("RetireTraffic",new object[]{false});check(behind.active,"Fallen vehicle is retained long enough to finish recovery behind the camera");
   reset();var blocked=spawn(false,false);hit(blocked);blocked.knockVelocity=Vector3.zero;blocked.model.localPosition=new Vector3(1.55f,0,40);
   var blocker=spawn(true,false);blocker.model.localPosition=new Vector3(1.55f,0,40);blocker.speed=blocker.baseTrafficSpeed=0;
   var blocker2=spawn(true,false);blocker2.model.localPosition=new Vector3(4.65f,0,40);blocker2.speed=blocker2.baseTrafficSpeed=0;
   call("TickTrafficRecovery",new object[]{blocked,3.1f});check(!blocked.IsStandingUp,"Recovery waits when both destination lanes are occupied");blocker.active=blocker2.active=false;call("TickTrafficRecovery",new object[]{blocked,.02f});check(blocked.IsStandingUp,"Recovery proceeds when a legal destination lane clears");
   foreach(bool roadTurn in new[]{false,true})
   {
    reset();var v=spawn(false,false);v.model.localPosition=new Vector3(1.55f,0,40);v.motionVelocity=Vector3.forward*7;
    if(roadTurn){v.roadTurning=true;v.roadTurnStart=new Vector3(1.55f,0,36);v.roadTurnForward=Vector3.forward;v.roadTurnSide=Vector3.left;v.roadTurnRadius=4;v.roadTurnSpeed=7;v.roadTurnAngle=.6f;}
    else{v.crossing=true;v.turning=true;v.crossSpeed=7;v.turnEndX=4.65f;v.turnOriginZ=g.Distance+40;v.turnZSign=1;v.turnRadius=3.1f;}
    hit(v);Vector3 end=v.recovery.endHeading;for(int i=0;i<600;i++)tick(.02f);
    check(!v.knocked&&v.recovery==null&&v.routeDetached,"Interrupted "+(roadTurn?"road":"cross-traffic")+" turn finishes after physical recovery");
    check(Vector3.Dot(v.routeVelocity.normalized,end)>.99f,"Interrupted turn retains its originally selected destination heading");
   }
   reset();var rebased=spawn(false,false);hit(rebased);var course=rebased.recovery;var old=course.heading;Quaternion inv=Quaternion.Euler(0,-90,0);call("RebaseTrafficRecovery",new object[]{rebased,inv,new Vector3(0,0,4)});
   check(Vector3.Distance(course.heading,inv*old)<.001f,"Player junction turn rebases the saved traffic heading correctly");
   foreach(bool truck in new[]{false,true})
   {
    reset();var v=g.SpawnReckless(truck,true);v.enteringFromBehind=false;v.model.localPosition=new Vector3(-10,0,20);call("CheckBuildingImpact",new object[]{v});
    check(v.wallCrashed&&v.recovery==null,"Burning reckless wall wreck stays wrecked: truck="+truck);
    for(int i=0;i<220;i++)tick(.02f);check(v.wallCrashed&&v.knocked,"Burning wreck does not auto recover after three seconds: truck="+truck);
    check(v.crashEffect.GetComponent<WreckFireVisual>()!=null&&v.crashEffect.GetComponentsInChildren<MeshRenderer>(true).Length==0,"Fire uses Blender flipbooks without capsule/sphere flame meshes: truck="+truck);
    check(v.fire.textureSheetAnimation.numTilesX==4&&v.fire.textureSheetAnimation.numTilesY==2&&v.fire.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.name=="PhoViet/Wreck Flame","Eight-frame physical fire and emissive material installed: truck="+truck);
    v.fire.Simulate(.5f,true,false);v.smoke.Simulate(1.5f,true,false);check(v.fire.particleCount>0&&v.smoke.particleCount>0,"Blender fire and smoke emit actual animated particles: truck="+truck);
   }
   reset();var sets=g.scenery.GetComponentsInChildren<StreetObstacleSet>(true);check(sets.Length==735&&sets.Sum(s=>s.obstacles.Length)>4000,"Every main/branch street has saved lightweight scenery collision proxies");
   foreach(string kind in new[]{"Nhà cửa","Thân cây","Bàn ăn","Ghế nhựa","Quầy thức ăn","Kệ tạp hóa","Tủ dụng cụ sửa xe","Cột đèn đường","Mái hiên"})
   {
    var s=sets.First(x=>x.gameObject.activeInHierarchy&&x.obstacles.Any(b=>b.kind==kind));var original=s.obstacles;var one=original.First(b=>b.kind==kind);Vector3 position=s.transform.position;
    try
    {
     s.obstacles=new[]{one};set("streetObstacles",new[]{s});float y=Mathf.Max(0,one.bounds.min.y+.05f);
     var a=g.transform.InverseTransformPoint(s.transform.TransformPoint(new Vector3(one.bounds.center.x,y,one.bounds.min.z-2)));
     var b=g.transform.InverseTransformPoint(s.transform.TransformPoint(new Vector3(one.bounds.center.x,y,one.bounds.min.z+.2f)));
     object[] args={a,b,Vector2.zero,0f,""};bool contact=(bool)call("SceneryContact",args);check(contact&&(string)args[4]==kind,"Swept scenery contact detects "+kind);
     args=new object[]{b,a,Vector2.zero,0f,""};check(!(bool)call("SceneryContact",args),"Rider can steer away from existing overlap after recovering near "+kind);
     a.y=b.y=g.transform.InverseTransformPoint(s.transform.TransformPoint(one.bounds.max)).y+.3f;args=new object[]{a,b,Vector2.zero,0f,""};check(!(bool)call("SceneryContact",args),"Jumping clearly above "+kind+" avoids a false collision");
     check(s.transform.position==position,"Scenery stays stationary after contact with "+kind);
    }
    finally{s.obstacles=original;}
   }
   reset();var obstacle=sets.First(s=>s.gameObject.activeInHierarchy&&s.obstacles.Any(b=>b.kind=="Thân cây"));var oldBoxes=obstacle.obstacles;
   try
   {
    var box=oldBoxes.First(b=>b.kind=="Thân cây");obstacle.obstacles=new[]{box};set("streetObstacles",new[]{obstacle});
    var a=g.transform.InverseTransformPoint(obstacle.transform.TransformPoint(new Vector3(box.bounds.center.x,0,box.bounds.min.z-2)));
    var b=g.transform.InverseTransformPoint(obstacle.transform.TransformPoint(new Vector3(box.bounds.center.x,0,box.bounds.min.z+.2f)));
    g.rider.localPosition=b;set("oldRiderZ",a.z);set("jumpPreviousHeight",a.y);set("riderRoadVelocity",(b-a)/.05f);set("riderVelocityValid",true);
    call("ResolveRiderContacts",new object[]{a.x,.05f});check(g.State==PhoVietGame.RideState.Falling&&g.LastSceneryImpact=="Thân cây","Actual rider collision with a tree starts a physical crash and starts a physical throw");
   }
   finally{obstacle.obstacles=oldBoxes;}
   reset();g.Crash();for(int i=0;i<650&&g.State==PhoVietGame.RideState.Falling;i++)call("TickRiderCrash",new object[]{.02f});
   check(g.State==PhoVietGame.RideState.GameOver&&g.RiderFallComplete,"A normal crash ends the run after the physical fall");
   check(Mathf.Abs(Mathf.DeltaAngle(0,g.rider.localEulerAngles.x))+Mathf.Abs(Mathf.DeltaAngle(0,g.rider.localEulerAngles.z))>20,"Ended run retains the fallen pose rather than standing up");
   reset();typeof(PhoVietGame).GetProperty("Speed").SetValue(g,15f);g.Crash();float slow=g.RiderFallVelocity.magnitude,slowImpact=g.LastRiderImpactSpeed;
   reset();typeof(PhoVietGame).GetProperty("Speed").SetValue(g,30f);g.Crash();check(g.LastRiderImpactSpeed>slowImpact*1.9f,"Scenery impact closing speed scales with actual approach speed");check(g.RiderFallVelocity.magnitude>slow,"Faster static impacts produce a stronger physical throw");
   foreach(int lane in new[]{0,1,2,3})
   {
    reset();int guard=0;while(g.NextJunctionDistance>24&&guard++<500)call("TickRide",new object[]{.02f});
    typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);g.MoveLane(lane<2?-1:1);
    guard=0;while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<600)call("TickRide",new object[]{.02f});
    check(g.CompletedTurns==1&&g.State==PhoVietGame.RideState.Riding,"Real painted junction stays driveable with scenery contacts enabled: lane "+lane+", obstacle="+g.LastSceneryImpact);
    check(g.State==PhoVietGame.RideState.Riding,"Legal turn does not collide to nearby scenery: lane "+lane);
   }
   return new{success=true,count=checks.Count,checks};
  }
  finally{g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
