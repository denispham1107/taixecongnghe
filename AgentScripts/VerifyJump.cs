using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyJump
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;clear();};
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  Func<string,PhoVietGame.TrafficVehicle> spawn=name=>
  {
   int i=g.vehicles.FindIndex(v=>!v.eventOnly&&!v.active&&v.model.GetComponent<CarVisualAnimator>()?.AssetName==name);
   if(i<0)throw new Exception("Missing "+name);var v=g.SpawnTraffic(2,0,i);v.speed=v.baseTrafficSpeed=7;v.brakeCooldown=v.laneCooldown=1000;return v;
  };
  try
  {
   Time.timeScale=0;g.ResetRide();check(!g.TryJump(),"Ready screen rejects jumping");reset();
   check(PhoVietGame.IsJumpSwipe(new Vector2(15,100),540),"Upward touch swipe triggers jump");
   check(!PhoVietGame.IsJumpSwipe(new Vector2(100,25),540)&&!PhoVietGame.IsJumpSwipe(new Vector2(0,-100),540),"Horizontal/downward swipes do not trigger jump");
   check(g.TryJump(),"Grounded rider can initiate wheelie");check(!g.TryJump(),"Repeated SpaceBar cannot restart windup");
   tick(.12f);check(!g.RiderAirborne&&g.RiderJumpClip=="JumpTakeoff","Wheelie precedes takeoff");
   var hero=g.rider.GetComponent<BikeVisualAnimator>();hero.Advance(.01f);check(hero.CurrentClip=="JumpTakeoff","Blender takeoff action is sampled by the rider animator");
   float maximum=0;for(int i=0;i<70;i++){tick(.02f);maximum=Mathf.Max(maximum,g.RiderJumpHeight);}
   check(maximum>3.3f&&maximum<3.8f,"Ballistic jump reaches consistent fixed height");
   check(!g.RiderAirborne&&g.RiderJumpHeight==0,"Unobstructed jump lands on road");
   check(g.State==PhoVietGame.RideState.Riding&&g.LivesRemaining==3,"Safe landing costs no lives");
   check(!g.TryJump(),"Two-second cooldown persists after landing");for(int i=0;i<40;i++)tick(.02f);
   check(g.TryJump(),"Cooldown allows next jump after two seconds");
   g.TogglePause();float h=g.RiderJumpHeight;check(!g.TryJump()&&g.RiderJumpHeight==h,"Pause blocks jump input and preserves height");g.TogglePause();
   reset();set("jumpFlying",true);set("jumpHeight",2.8f);set("jumpVelocity",-3f);set("jumpPreviousHeight",2.8f);check(!g.TryJump(),"Mid-air jump is rejected");
   // Fallen motorcycle directly under the real ballistic path.
   reset();int bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);var fallen=g.SpawnTraffic(2,11,bike);
   fallen.knocked=true;fallen.physicalCrash=true;fallen.knockVelocity=Vector3.zero;fallen.model.localRotation=Quaternion.Euler(0,0,-78);
   g.TryJump();for(int i=0;i<85&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
   check(g.State==PhoVietGame.RideState.Riding&&g.LivesRemaining==3,"Actual jump clears fallen motorcycle without collision immunity");
   reset();var moving=spawn("PV_Sedan");moving.model.localPosition=new Vector3(PhoVietGame.LaneX[2],0,14);moving.previousPosition=moving.model.localPosition;
   check(g.TryJump(),"Approaching a moving sedan can start a real jump");bool actualRoof=false;
   for(int i=0;i<180&&g.State==PhoVietGame.RideState.Riding;i++){tick(.01f);actualRoof|=g.RiderOnRoof;}
   check(actualRoof&&g.LivesRemaining==3,"Full takeoff-flight-descent lands on moving car roof");
   foreach(string asset in new[]{"PV_Sedan","PV_Hatchback","PV_Taxi","PV_DeliveryVan"})
   {
    reset();var v=spawn(asset);float roof=PhoVietGame.JumpRoofHeight(v);set("jumpHeight",roof+.2f);set("jumpPreviousHeight",roof+.2f);set("jumpVelocity",-3f);set("jumpFlying",true);
    g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],roof+.2f,0);
    bool landed=false;for(int i=0;i<12&&g.State==PhoVietGame.RideState.Riding;i++){tick(.02f);landed|=g.RiderOnRoof;}
    check(landed&&g.LivesRemaining==3,"Descending rider lands on "+asset+" roof without losing life");
    for(int i=0;i<120&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
    check(g.State==PhoVietGame.RideState.Riding&&!g.RiderOnRoof&&g.RiderJumpHeight==0,"Faster rider drives off "+asset+" roof and returns to road: "+g.State+" y="+g.RiderJumpHeight+" life="+g.LivesRemaining+" carZ="+v.model.localPosition.z);
   }
   reset();var car=spawn("PV_Sedan");set("roofVehicle",car);set("jumpHeight",1.625f);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],1.625f,0);
   check(g.TryJump(),"A supported rider can jump from a car roof");
   reset();var support=spawn("PV_Sedan");set("roofVehicle",support);set("jumpHeight",1.625f);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],1.625f,0);support.active=false;support.model.gameObject.SetActive(false);tick(.02f);
   check(g.RiderAirborne&&!g.RiderOnRoof,"A removed or crashed support cannot leave rider floating");
   reset();set("jumpVelocity",-11f);call("LandJump",new object[]{0f,null});check(g.RiderJumpClip=="LandFront","Fast descending travel selects front-wheel landing");
   reset();var slowRoof=spawn("PV_Sedan");slowRoof.motionVelocity=Vector3.forward*16;set("jumpVelocity",-5f);call("LandJump",new object[]{1.625f,slowRoof});check(g.RiderJumpClip=="LandRear","Small relative roof speed selects rear-wheel landing");
   reset();set("jumpVelocity",-5f);call("LandJump",new object[]{0f,null});check(g.RiderJumpClip=="LandBoth","Moderate descent selects two-wheel landing");
   reset();g.TryJump();for(int i=0;i<30;i++)tick(.02f);float beforeTurn=g.RiderJumpHeight;
   set("turnSide",1);call("FinishJunctionTurn",new object[0]);
   check(Mathf.Abs(g.rider.localPosition.y-beforeTurn)<.001f,"Junction rebase preserves airborne height");
   check(g.blocks.TrueForAll(b=>Mathf.Abs(b.position.y)<.01f),"Turning in air does not lift or sink the streets");
   reset();var body=spawn("PV_Sedan");g.TryJump();tick(.02f);check(g.State==PhoVietGame.RideState.Falling&&g.LivesRemaining==2,"Insufficient takeoff clearance hits car body and spends one life");
   check(!g.RiderAirborne&&!g.RiderJumpActive,"Crash clears jump and roof state");
   reset();var truck=g.vehicles.Find(v=>v.halfLength>3);check(PhoVietGame.JumpRoofHeight(truck)<0,"Medium box/drowsy trucks are not eligible small roofs");
   reset();call("StartChase",new object[0]);var collision=spawn("PV_Sedan");g.TryJump();tick(.02f);
   check(g.LivesRemaining==2&&g.State==PhoVietGame.RideState.Falling,"Body collision during pursuit still spends one life");
   reset();call("StartChase",new object[0]);g.TryJump();g.Arrest();check(g.State==PhoVietGame.RideState.Arrested,"Police capture still ends immediately during a jump");
   reset();g.TryJump();g.ResetRide();check(!g.RiderJumpActive&&g.RiderJumpCooldown==0&&g.RiderJumpHeight==0,"Restart resets all jump state");
   var data=JsonUtility.FromJson<BikeJumpPose.Library>(Resources.Load<TextAsset>("PhoVietJump").text);
   check(data.clips.Length==5,"Five actual Blender actions are exported to Unity");foreach(var c in data.clips)check(c.samples.Length==61,"Smooth sampled action "+c.name);
   return new{success=true,count=checks.Count,maximumHeight=maximum,checks};
  }
  finally
  {
   g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
  }
 }
}
