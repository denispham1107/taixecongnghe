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
  Func<string,float,float,PhoVietGame.TrafficVehicle> spawn=(name,z,speed)=>
  {
   int i=g.vehicles.FindIndex(v=>v.model.GetComponent<CarVisualAnimator>()?.AssetName==name);
   if(i<0)throw new Exception("Missing "+name);var selected=g.vehicles[i];bool eventOnly=selected.eventOnly;selected.eventOnly=false;
   var v=g.SpawnTraffic(2,z,i);selected.eventOnly=eventOnly;
   v.isReckless=false;v.speed=v.baseTrafficSpeed=speed;v.brakeCooldown=v.laneCooldown=1000;
   v.model.localRotation=Quaternion.Euler(0,speed<0?180:0,0);v.previousPosition=v.model.localPosition;return v;
  };
  Action<PhoVietGame.TrafficVehicle> mount=v=>{set("roofVehicle",v);set("jumpHeight",3.2f);set("jumpPreviousHeight",3.2f);g.rider.localPosition=new Vector3(v.model.localPosition.x,3.2f,0);};
  try
  {
   Time.timeScale=0;
   foreach(string asset in new[]{"PV_BoxTruck","PV_DrowsyTruck"})
   {
    foreach(float speed in new[]{7f,0f,-10f})
    {
     reset();var v=spawn(asset,25.1f-speed*1.29f,speed);
     check(Mathf.Abs(PhoVietGame.JumpRoofHeight(v)-3.175f)<.001f,asset+" exposes its Blender cargo roof");
     check(g.TryJump(),asset+" speed "+speed+": real ground takeoff starts");
     bool landed=false;float supported=0;
     for(int i=0;i<210&&g.State==PhoVietGame.RideState.Riding;i++){tick(.01f);if(g.RiderOnRoof){landed=true;supported+=.01f;}}
     check(landed&&g.State==PhoVietGame.RideState.Riding,asset+" speed "+speed+": full ballistic flight lands safely on roof, state="+g.State);
     check(supported>.02f,asset+" speed "+speed+": roof actually supports riding");
     for(int i=0;i<300&&g.State==PhoVietGame.RideState.Riding;i++)tick(.01f);
     check(!g.RiderOnRoof&&!g.RiderAirborne&&g.RiderJumpHeight==0&&g.State==PhoVietGame.RideState.Riding,asset+" speed "+speed+": rides beyond roof and physically lands on road, state="+g.State+" height="+g.RiderJumpHeight);
    }
    reset();var support=spawn(asset,0,17);mount(support);
    check(g.TryJump(),asset+": can jump again from supported truck roof");tick(.3f);
    check(g.RiderAirborne&&g.RiderJumpHeight>3.2f,asset+": rooftop takeoff keeps existing roof altitude");
    reset();support=spawn(asset,0,17);mount(support);support.knocked=true;support.physicalCrash=true;
    check(PhoVietGame.JumpRoofHeight(support)>0,asset+": damaged but upright roof remains eligible");
    support.model.localRotation=Quaternion.Euler(0,0,38);
    check(PhoVietGame.JumpRoofHeight(support)<0,asset+": tilted wreck cannot be landed on");
    call("ResolveJumpSurface",new object[]{.01f});check(g.RiderAirborne&&!g.RiderOnRoof,asset+": rider falls when supporting truck tips over");
    reset();var body=spawn(asset,1,7);g.TryJump();tick(.02f);
    check(g.State==PhoVietGame.RideState.Falling,asset+": insufficient clearance still hits truck body and starts a physical crash");
    reset();var boosted=spawn(asset,47.4f,7);g.TryBoost();g.TryJump();bool boostedRoof=false;
    for(int i=0;i<65&&g.State==PhoVietGame.RideState.Riding;i++){tick(.05f);boostedRoof|=g.RiderOnRoof;}
    check(boostedRoof&&g.State==PhoVietGame.RideState.Riding&&g.RiderJumpHeight==0,asset+": boosted jump at 20 physics steps/sec lands on roof then road without tunneling");
    reset();support=spawn(asset,0,17);mount(support);support.active=false;support.model.gameObject.SetActive(false);tick(.01f);
    check(g.RiderAirborne&&!g.RiderOnRoof,asset+": retired truck releases its roof support");
    reset();support=spawn(asset,1,-10);mount(support);set("jumpHeight",2.735f);set("jumpPreviousHeight",2.735f);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],2.735f,0);
    for(int i=0;i<30&&g.State==PhoVietGame.RideState.Riding;i++)tick(.01f);
    check(g.State==PhoVietGame.RideState.Falling,asset+": driving from low cab into higher cargo wall collides instead of snapping up");
   }
   // Exercise the actual weaving controller on the drowsy-truck skin, then drive
   // off it. This is not a special invulnerability path for dangerous traffic.
   reset();var weaving=spawn("PV_DrowsyTruck",0,18);weaving.isReckless=true;weaving.willHitWall=false;
   weaving.startPosition=weaving.model.localPosition;weaving.swerveAmplitude=.18f;weaving.swerveFrequency=2;weaving.swerveSpeed=8;weaving.swerveDirection=1;mount(weaving);
   float startX=weaving.model.localPosition.x;for(int i=0;i<50;i++)tick(.01f);
   check(g.RiderOnRoof&&g.State==PhoVietGame.RideState.Riding&&Mathf.Abs(weaving.model.localPosition.x-startX)>.01f,"Upright drowsy truck really weaves while rider stays supported on roof");
   weaving.isReckless=false;weaving.speed=weaving.baseTrafficSpeed=7;
   bool cabinFall=false;float oldHeight=g.RiderJumpHeight;
   for(int i=0;i<350&&g.State==PhoVietGame.RideState.Riding;i++){tick(.01f);if(g.RiderAirborne&&g.RiderJumpHeight<oldHeight&&g.RiderJumpHeight>2.7f)cabinFall=true;oldHeight=g.RiderJumpHeight;}
   check(cabinFall,"Cargo-to-cab height step begins a physical fall instead of instant downward snapping");
   check(g.State==PhoVietGame.RideState.Riding&&g.RiderJumpHeight==0&&!g.RiderAirborne,"Weaving truck can be ridden across and exited without a false collision");
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
