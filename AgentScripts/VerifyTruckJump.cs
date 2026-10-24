using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyTruckJump
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  // Meshy trucks pick covered/container at spawn. Pin the shape under test after
  // SpawnTraffic so timing, roof heights and the body that is hit are one model.
  int shape=0;Func<string,int> slot=name=>{int i=g.vehicles.FindIndex(v=>v.model.GetComponent<CarVisualAnimator>()?.AssetName==name);if(i<0)throw new Exception("Missing "+name);return i;};
  Func<string,float,float,PhoVietGame.TrafficVehicle> spawn=(name,z,speed)=>
  {
   int i=slot(name);var selected=g.vehicles[i];bool eventOnly=selected.eventOnly;selected.eventOnly=false;
   var v=g.SpawnTraffic(2,z,i);selected.eventOnly=eventOnly;v.model.GetComponent<MeshyCarVariants>()?.Select(shape,0,v);
   v.isReckless=false;v.speed=v.baseTrafficSpeed=speed;v.brakeCooldown=v.laneCooldown=1000;
   v.model.localRotation=Quaternion.Euler(0,speed<0?180:0,0);v.previousPosition=v.model.localPosition;return v;
  };
  Func<PhoVietGame.TrafficVehicle,float,float,float> surface=(v,a,b)=>v.model.GetComponent<MeshyCarVariants>().SupportHeight(a,b)+.025f;
  Action<PhoVietGame.TrafficVehicle,float> mountAt=(v,h)=>{set("roofVehicle",v);set("jumpHeight",h);set("jumpPreviousHeight",h);g.rider.localPosition=new Vector3(v.model.localPosition.x,h,0);};
  Action<PhoVietGame.TrafficVehicle> mount=v=>mountAt(v,surface(v,-.73f,.90f));
  // Spawn z that puts the vehicle centre under a real free flight when it descends
  // to the selected roof (same construction as VerifyRoofRiding/VerifyMeshyCarRoofs).
  Func<string,float,float,bool,float> landingZ=(name,speed,dt,boost)=>
  {
   reset();var pool=g.vehicles[slot(name)];var c=pool.model.GetComponent<MeshyCarVariants>();c.Select(shape,0,pool);float roof=c.SupportHeight(-.73f,.90f)+.025f;
   if(boost)g.TryBoost();g.TryJump();float t=0,h=0,d=0;
   for(int i=0;i<400;i++){h=g.RiderJumpHeight;d=g.Distance;tick(dt);t+=dt;if(h>roof&&g.RiderJumpHeight<=roof)break;}
   float f=Mathf.InverseLerp(h,g.RiderJumpHeight,roof);return Mathf.Lerp(d,g.Distance,f)-speed*(t-dt+f*dt);
  };
  try
  {
   Time.timeScale=0;
   foreach(string pool in new[]{"PV_BoxTruck","PV_DrowsyTruck"})for(shape=0;shape<g.vehicles[slot(pool)].model.GetComponent<MeshyCarVariants>().variants.Length;shape++)
   {
    var received=g.vehicles[slot(pool)].model.GetComponent<MeshyCarVariants>().variants[shape];string asset=pool+"/"+received.asset;
    foreach(float speed in new[]{7f,0f,-10f})
    {
     float z=landingZ(pool,speed,.01f,false);reset();var v=spawn(pool,z,speed);var meshy=v.model.GetComponent<MeshyCarVariants>();
     check(meshy.ModelAsset==received.asset&&Mathf.Abs(PhoVietGame.JumpRoofHeight(v)-received.size.y)<.001f&&meshy.RoofProfile==received.roofProfile&&v.halfLength>3,asset+" exposes its received Meshy cargo roof and profile");
     check(g.TryJump(),asset+" speed "+speed+": real ground takeoff starts");
     bool landed=false;float supported=0;
     for(int i=0;i<210&&g.State==PhoVietGame.RideState.Riding;i++){tick(.01f);if(g.RiderOnRoof){landed=true;supported+=.01f;}}
     check(landed&&g.State==PhoVietGame.RideState.Riding,asset+" speed "+speed+": full ballistic flight lands safely on roof, state="+g.State);
     check(supported>.02f,asset+" speed "+speed+": roof actually supports riding");
     for(int i=0;i<300&&g.State==PhoVietGame.RideState.Riding;i++)tick(.01f);
     check(!g.RiderOnRoof&&!g.RiderAirborne&&g.RiderJumpHeight==0&&g.State==PhoVietGame.RideState.Riding,asset+" speed "+speed+": rides beyond roof and physically lands on road, state="+g.State+" height="+g.RiderJumpHeight);
    }
    reset();var support=spawn(pool,0,17);mount(support);float mounted=g.RiderJumpHeight;
    check(g.TryJump(),asset+": can jump again from supported truck roof");tick(.3f);
    check(g.RiderAirborne&&g.RiderJumpHeight>mounted,asset+": rooftop takeoff keeps existing roof altitude");
    reset();support=spawn(pool,0,17);mount(support);support.knocked=true;support.physicalCrash=true;
    check(PhoVietGame.JumpRoofHeight(support)>0,asset+": damaged but upright roof remains eligible");
    support.model.localRotation=Quaternion.Euler(0,0,38);
    check(PhoVietGame.JumpRoofHeight(support)<0,asset+": tilted wreck cannot be landed on");
    call("ResolveJumpSurface",new object[]{.01f});check(g.RiderAirborne&&!g.RiderOnRoof,asset+": rider falls when supporting truck tips over");
    reset();var body=spawn(pool,1,7);g.TryJump();tick(.02f);
    check(g.State==PhoVietGame.RideState.Falling,asset+": insufficient clearance still hits truck body and starts a physical crash");
    float boostedZ=landingZ(pool,7,.05f,true);reset();var boosted=spawn(pool,boostedZ,7);g.TryBoost();g.TryJump();bool boostedRoof=false;
    for(int i=0;i<65&&g.State==PhoVietGame.RideState.Riding;i++){tick(.05f);boostedRoof|=g.RiderOnRoof;}
    check(boostedRoof&&g.State==PhoVietGame.RideState.Riding&&g.RiderJumpHeight==0,asset+": boosted jump at 20 physics steps/sec lands on roof then road without tunneling");
    reset();support=spawn(pool,0,17);mount(support);support.active=false;support.model.gameObject.SetActive(false);tick(.01f);
    check(g.RiderAirborne&&!g.RiderOnRoof,asset+": retired truck releases its roof support");
    // Lowest cab point in the front half of the received roof (an oncoming truck faces the
    // rider, so its rear wheel is ahead of the bike in truck space), and the cargo top behind it.
    float cabZ=.5f,cab=float.PositiveInfinity;var shapeRoof=g.vehicles[slot(pool)].model.GetComponent<MeshyCarVariants>();shapeRoof.Select(shape,0,g.vehicles[slot(pool)]);
    for(float cz=.5f;cz<=received.size.z*.5f-.5f;cz+=.05f){float h=shapeRoof.SupportHeight(cz+.73f,cz-.90f);if(h<cab){cab=h;cabZ=cz;}}
    float cargo=0;foreach(var p in received.roofProfile)if(p.x<cabZ-1)cargo=Mathf.Max(cargo,p.y);
    check(cargo-cab>.3f,asset+": received truck has a cab at least 0.3 m below its cargo top (cab "+cab+" at z="+cabZ+", cargo "+cargo+")");
    reset();support=spawn(pool,cabZ,-10);mountAt(support,cab+.025f);
    for(int i=0;i<30&&g.State==PhoVietGame.RideState.Riding;i++)tick(.01f);
    check(g.State==PhoVietGame.RideState.Falling,asset+": driving from low cab into higher cargo wall collides instead of snapping up");
    if(pool!="PV_DrowsyTruck")continue;
    // Exercise the actual weaving controller on the drowsy-truck skin, then drive
    // off it. This is not a special invulnerability path for dangerous traffic.
    reset();var weaving=spawn(pool,0,18);weaving.isReckless=true;weaving.willHitWall=false;
    weaving.startPosition=weaving.model.localPosition;weaving.swerveAmplitude=.18f;weaving.swerveFrequency=2;weaving.swerveSpeed=8;weaving.swerveDirection=1;mount(weaving);
    float startX=weaving.model.localPosition.x;for(int i=0;i<50;i++)tick(.01f);
    check(g.RiderOnRoof&&g.State==PhoVietGame.RideState.Riding&&Mathf.Abs(weaving.model.localPosition.x-startX)>.01f,asset+": upright drowsy truck really weaves while rider stays supported on roof");
    weaving.isReckless=false;weaving.speed=weaving.baseTrafficSpeed=7;
    // Forward over the received cargo-to-cab transition: a step drops into a physical fall,
    // a slope is followed; neither may snap the bike down more than 0.12 m in one step.
    bool step=false;float forwardCab=float.PositiveInfinity;
    for(float cz=0;cz<=received.size.z*.5f-.5f;cz+=.05f){float h=weaving.model.GetComponent<MeshyCarVariants>().SupportHeight(cz-.73f,cz+.90f),next=weaving.model.GetComponent<MeshyCarVariants>().SupportHeight(cz+.05f-.73f,cz+.05f+.90f);step|=h-next>.12f;forwardCab=Mathf.Min(forwardCab,h);}
    bool cabinFall=false;float oldHeight=g.RiderJumpHeight,snap=0;bool wasOnRoof=g.RiderOnRoof;
    for(int i=0;i<350&&g.State==PhoVietGame.RideState.Riding;i++){tick(.01f);if(g.RiderAirborne&&g.RiderJumpHeight<oldHeight&&g.RiderJumpHeight>forwardCab+.05f)cabinFall=true;if(wasOnRoof&&g.RiderOnRoof)snap=Mathf.Max(snap,oldHeight-g.RiderJumpHeight);oldHeight=g.RiderJumpHeight;wasOnRoof=g.RiderOnRoof;}
    if(step)check(cabinFall,asset+": cargo-to-cab height step begins a physical fall instead of instant downward snapping");
    check(snap<=.121f,asset+": roof-supported descent never snaps more than 0.12 m in one 10 ms step (max "+snap+", step profile="+step+")");
    check(g.State==PhoVietGame.RideState.Riding&&g.RiderJumpHeight==0&&!g.RiderAirborne,asset+": weaving truck can be ridden across and exited without a false collision");
   }
   // Flight action is driven by vertical velocity, so higher jumps and falls
   // from trucks use the appropriate phase of the existing Blender animation.
   reset();set("jumpFlying",true);set("jumpVelocity",0f);check(Mathf.Abs(g.RiderJumpPose-.5f)<.001f,"Blender air pose stays at apex when vertical velocity is zero");
   set("jumpVelocity",-8f);check(g.RiderJumpPose>.5f&&g.RiderJumpPose<1,"Falling off a truck selects descending air pose");
   foreach(bool chased in new[]{false,true})foreach(float x in PhoVietGame.LaneX)
   {
    foreach(float apex in new[]{4.845f,8.045f})
    {
     reset();if(chased)call("StartChase",new object[0]);set("jumpHeight",apex);g.rider.localPosition=new Vector3(x,apex,0);call("PositionCamera",new object[]{true});
     var p=g.rideCamera.WorldToViewportPoint(g.rider.position+Vector3.up*1.5f);
     check(p.z>0&&p.x>.02f&&p.x<.98f&&p.y>.02f&&p.y<.94f,"Ground/rooftop apex "+apex+" stays visible: lane "+x+", chase="+chased+", viewport="+p);
    }
   }
   return new{success=true,count=checks.Count,width=Screen.width,height=Screen.height,checks};
  }
  finally{g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
