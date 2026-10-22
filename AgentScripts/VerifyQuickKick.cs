using System;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyQuickKick
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var cases=new List<object>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action<int> reset=lane=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);};
  try
  {
   Time.timeScale=0;
   foreach(float dt in new[]{.02f,.05f})foreach(int lane in new[]{2,3})foreach(bool incoming in new[]{false,true})
   {
    reset(lane);int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth<.7f);var target=g.SpawnTraffic(lane==2?3:2,incoming?12:2,index);target.speed=target.baseTrafficSpeed=incoming?-12:28;target.model.localRotation=Quaternion.Euler(0,incoming?180:0,0);target.motionVelocity=Vector3.zero;
    string label=$"dt={dt},lane={lane},incoming={incoming}";float normal=g.Speed;
    check(g.TryKick(),"Fast nearby motorcycle selected: "+label);check(!target.knocked&&g.KickCount==0,"No force on pressing kick: "+label);
    float age=0,hitAt=-1,peak=0,maxStep=0;Vector3 previous=g.rider.localPosition;
    for(int i=0;i<100&&g.State==PhoVietGame.RideState.Riding&&g.KickAssistActive;i++)
    {
     tick(dt);age+=dt;peak=Mathf.Max(peak,g.Speed);maxStep=Mathf.Max(maxStep,Mathf.Abs(g.rider.localPosition.x-previous.x));previous=g.rider.localPosition;
     if(target.knocked&&hitAt<0)hitAt=age;
    }
    cases.Add(new{label,hitAt,age,peak,maxStep,state=g.State.ToString(),gap=g.LastKickFootGap});
    check(target.knocked&&hitAt>0&&hitAt<=(incoming?.35f:.30f),"Physical foot hit within fast interception budget: "+label+",hit="+hitAt+",state="+g.State);
    check(peak>normal*1.15f,"Only rider surges to intercept target: "+label+",peak="+peak);
    check(g.State==PhoVietGame.RideState.Riding&&!g.KickAssistActive&&g.CurrentLane==lane&&Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[lane])<.01f,"Fast kick returns to original lane safely: "+label);
    check(g.LastKickFootGap>=0&&g.LastKickFootGap<=.081f&&g.KickCount==1,"Fast surge preserves animated shoe contact and one impact: "+label);
    check(maxStep<=PhoVietGame.KickApproachLateralSpeed*dt+.01f,"Fast approach still has bounded motion, no teleport: "+label);
   }
   foreach(int lane in new[]{2,3})
   {
    reset(lane);int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth<.7f);var target=g.SpawnTraffic(lane==2?3:2,2,index);target.speed=target.baseTrafficSpeed=20;
    check(g.BeginSuddenLaneChange(target,lane)&&g.TryKick(),"Can intercept a nearby bike moving between lanes: "+lane);float hitAt=-1,age=0;
    for(int i=0;i<100&&g.KickAssistActive&&g.State==PhoVietGame.RideState.Riding;i++){tick(.02f);age+=.02f;if(target.knocked&&hitAt<0)hitAt=age;}
    check(target.knocked?hitAt>0&&hitAt<.4f&&g.KickCount==1&&g.LastKickFootGap<=.081f:g.KickCount==0&&!g.IsChased,"A target cutting across the approach is hit only by shoe contact, otherwise canceled safely: "+lane);
    check(g.State==PhoVietGame.RideState.Riding&&!g.KickAssistActive&&Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[lane])<.01f,"Cut-in cancellation/interception returns without crossing the bike: "+lane);
    cases.Add(new{label="moving-lane-"+lane,hitAt,age,gap=g.LastKickFootGap});
   }
   return new{success=true,count=checks.Count,checks,cases};
  }
  finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
