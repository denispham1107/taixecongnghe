using System;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyKickContact
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);var checks=new List<string>();var contacts=new List<object>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  Action<int> reset=lane=>
  {
   g.jobsEnabled=false;g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;g.RestartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
  };
  Func<int,float,float,PhoVietGame.TrafficVehicle> bike=(lane,z,speed)=>
  {
   int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth<.7f);var v=g.SpawnTraffic(lane,z,index);
   v.speed=v.baseTrafficSpeed=speed;v.model.localRotation=Quaternion.Euler(0,speed<0?180:0,0);v.motionVelocity=Vector3.zero;return v;
  };
  try
  {
   Time.timeScale=0;
   foreach(float dt in new[]{.02f,.05f,.1f})foreach(int lane in new[]{2,3})foreach(bool incoming in new[]{false,true})
   {
    reset(lane);int targetLane=lane==2?3:2;var v=bike(targetLane,incoming?12:0,incoming?-12:17);string label=$"dt={dt},lane={lane},incoming={incoming}";
    check(g.TryKick(),"Safe target selected: "+label);
    check(!v.knocked&&g.KickCount==0&&!g.IsChased,"No remote force or police on button press: "+label);
    var hero=g.rider.GetComponent<BikeVisualAnimator>();hero.Advance(0);check(hero.CurrentClip!="KickLeft"&&hero.CurrentClip!="KickRight","Leg stays on bike during approach: "+label);
    bool sawSwing=false;float elapsed=0,maxStep=0;Vector3 previous=g.rider.localPosition;
    for(int i=0;i<180&&g.State==PhoVietGame.RideState.Riding&&(i==0||g.KickAssistActive);i++)
    {
     tick(dt);elapsed+=dt;hero.Advance(0);maxStep=Mathf.Max(maxStep,Mathf.Abs(g.rider.localPosition.x-previous.x));previous=g.rider.localPosition;
     if(g.KickSwingActive){sawSwing=true;check(hero.CurrentClip==(lane==2?"KickRight":"KickLeft"),"Visible leg follows physical contact clock: "+label+",frame="+i);}
    }
    check(sawSwing&&v.knocked&&g.KickCount==1,"Approach then one physical foot impact: "+label);
    check(g.LastKickFootGap>=0&&g.LastKickFootGap<=.081f&&g.LastKickCentreDistance<2.6f,"Impact lies on animated shoe surface, not remote range: "+label);
    check(g.State==PhoVietGame.RideState.Riding&&!g.KickAssistActive&&g.CurrentLane==lane&&Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[lane])<.01f,"Returns to original lane without crashing: "+label);
    check(maxStep<=PhoVietGame.KickApproachLateralSpeed*dt+.02f,"No lateral teleport: "+label);check(g.IsChased,"Ordinary bike summons police only after contact: "+label);
    contacts.Add(new{label,elapsed,g.LastKickFootGap,g.LastKickCentreDistance,maxStep});
   }
   reset(2);var boostTarget=bike(3,0,17);g.TryKick();for(int i=0;i<100&&g.KickAssistActive;i++)tick(.02f);float beforeBoost=g.Speed;
   check(boostTarget.knocked&&!g.KickAssistActive&&g.TryBoost(),"Boost remains available after assisted kick completes");tick(.02f);check(g.Speed>beforeBoost*2.1f,"Finished kick cannot leave a permanent acceleration cap on boost");
   foreach(float z in new[]{1f,3f,17f,30f})
   {reset(2);var v=bike(3,z,-20);check(!g.TryKick()&&!v.knocked,"Reject impossible or over-16m oncoming approach: "+z);}
   reset(2);var target=bike(3,0,17);check(g.TryKick(),"Cancellation fixture starts");tick(.04f);float approached=g.rider.localPosition.x;g.MoveLane(-1);check(!g.KickAssistActive,"Manual steering cancels approach immediately");for(int i=0;i<5;i++)tick(.02f);check(!target.knocked&&g.KickCount==0&&!g.IsChased,"Canceled kick cannot hit later");
   foreach(string action in new[]{"TryJump","TryBoost","Crash"})
   {reset(2);target=bike(3,0,17);check(g.TryKick(),"Action cancellation fixture: "+action);tick(.02f);typeof(PhoVietGame).GetMethod(action,BindingFlags.Public|BindingFlags.Instance).Invoke(g,null);check(!g.KickAssistActive&&!target.knocked,"Action cancels kick without invisible impact: "+action);}
   reset(2);target=bike(3,0,17);g.TryKick();tick(.04f);g.TogglePause();Vector3 paused=g.rider.localPosition;float pose=g.KickSwingPose;g.rider.GetComponent<BikeVisualAnimator>().Advance(.5f);check(g.rider.localPosition==paused&&g.KickSwingPose==pose&&!g.TryKick(),"Pause freezes approach and rejects another kick");g.TogglePause();for(int i=0;i<60&&g.KickAssistActive;i++)tick(.02f);check(target.knocked,"Resumed kick still reaches real contact");
   reset(2);target=bike(3,0,17);g.TryKick();tick(.02f);call("RetireVehicle",new object[]{target});for(int i=0;i<60&&g.KickAssistActive;i++)tick(.02f);check(g.KickCount==0&&!g.IsChased&&!g.KickAssistActive,"Retired target cancels and returns safely");
   reset(2);target=bike(3,0,17);g.TryKick();tick(.02f);int old=target.spawnGeneration;call("RetireVehicle",new object[]{target});var recycled=g.SpawnTraffic(3,25,g.vehicles.IndexOf(target));check(recycled.spawnGeneration!=old,"Pool generation changes on reuse");for(int i=0;i<60&&g.KickAssistActive;i++)tick(.02f);check(!recycled.knocked&&g.KickCount==0,"Reused model is never mistaken for original target");
   foreach(bool late in new[]{false,true})
   {
    reset(2);target=bike(3,0,17);if(late){check(g.TryKick(),"Dynamic blocker fixture starts");tick(.02f);}
    int carIndex=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth>.7f);var blocker=g.SpawnTraffic(3,0,carIndex);blocker.model.localPosition=new Vector3(3.1f,0,0);blocker.speed=blocker.baseTrafficSpeed=17;
    if(!late)check(!g.TryKick(),"Car blocking approach prevents selecting unreachable motorcycle");
    else{for(int i=0;i<50&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);check(g.KickCount==0&&!target.knockFromPlayer,"New blocker cancels without kick-through or immunity");}
   }
   reset(2);target=bike(3,0,17);g.TryKick();tick(.02f);var collision=g.SpawnTraffic(2,0,0);collision.model.localPosition=g.rider.localPosition;tick(.02f);check(g.State==PhoVietGame.RideState.Falling&&g.KickCount==0,"Actual car contact during approach still crashes player");
   foreach(int side in new[]{-1,1})
   {
    reset(2);target=bike(side<0?1:3,12,-12);check(g.TryKick(),"True opposing-lane target can be approached on side "+side);
    for(int i=0;i<100&&g.KickAssistActive&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
    check(target.knocked&&target.knockDirection==side&&g.State==PhoVietGame.RideState.Riding,"Oncoming foot contact and safe return on side "+side);
    reset(2);var racer=g.SpawnReckless(false);racer.enteringFromBehind=false;racer.willHitWall=false;racer.routeDetached=true;racer.routeVelocity=Vector3.forward*17;racer.speed=racer.baseTrafficSpeed=17;racer.model.localRotation=Quaternion.identity;racer.model.localPosition=g.rider.localPosition+Vector3.right*side*3.1f;
    check(g.TryKick(),"Self-defense target can be approached on side "+side);
    for(int i=0;i<100&&g.KickAssistActive&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
    check(racer.knocked&&g.KickCount==1&&!g.IsChased,"Only actual self-defense contact knocks racer without police on side "+side);
   }
   reset(2);target=bike(3,0,17);g.TryKick();for(int i=0;i<40&&!g.KickSwingActive;i++)tick(.01f);g.MoveLane(-1);for(int i=0;i<50&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);check(!target.knocked&&g.KickCount==0&&!g.IsChased,"Steering can cancel the swing before its foot makes contact");
   reset(2);target=bike(3,0,17);g.TryKick();target.routeDetached=true;target.routeVelocity=Vector3.forward*100;target.speed=100;for(int i=0;i<100&&g.KickAssistActive;i++)tick(.02f);check(g.KickCount==0&&!target.knocked&&!g.KickAssistActive,"Escaping target produces a miss and returns, never a remote impact");
   foreach(bool pickup in new[]{true,false})
   {
    reset(2);g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    target=bike(3,0,17);g.TryKick();typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,pickup?PhoVietGame.WorkPhase.Pickup:PhoVietGame.WorkPhase.Delivery);call("BeginService",null);
    check(!g.KickAssistActive&&g.ServiceStopped&&!target.knocked,"Boarding/alighting cancels assisted kick: "+pickup);
   }
   reset(2);target=bike(3,0,17);g.TryKick();g.RestartRide();check(!g.KickAssistActive&&g.KickCount==0&&g.LastKickFootGap<0,"Restart clears all assist, target and foot contact state");
   return new{success=true,count=checks.Count,checks,contacts};
  }
  finally
  {
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;
  }
 }
}
