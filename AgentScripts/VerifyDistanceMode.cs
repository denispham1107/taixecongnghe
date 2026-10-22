using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyDistanceMode
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  float scale=Time.timeScale;int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<string,object> prop=(n,v)=>typeof(PhoVietGame).GetProperty(n).SetValue(g,v);
  try
  {
   Time.timeScale=0;g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;
   check(!PhoVietGame.RideJobsReleased,"Ride jobs explicitly disabled for this release");
   check(!jobs,"Saved scene defaults to distance mode");
   foreach(bool legacyEnabled in new[]{false,true})
   {
    g.jobsEnabled=legacyEnabled;g.RestartRide();clear();
    check(!g.JobsAvailable,"Old enabled flag cannot restore jobs: "+legacyEnabled);
    g.ShowJobOffers();check(!g.JobOffersOpen&&g.JobOffers.Count==0&&g.State==PhoVietGame.RideState.Riding,"Direct offer call does not pause or generate work: "+legacyEnabled);
    check(!g.AcceptJob(0),"Direct acceptance rejected: "+legacyEnabled);
    for(int i=0;i<1800;i++)
    {
     clear();set("spawnTimer",1000f);call("UpdateWork",new object[]{1f/30});call("TickRide",new object[]{1f/30});
     if(g.State!=PhoVietGame.RideState.Riding||g.JobOffersOpen||g.ActiveJob!=null)throw new Exception("Unexpected job/stop at simulation frame "+i);
    }
    check(g.Distance>1000&&g.Speed>17,"60-second run accumulates distance and speed without job pauses: "+legacyEnabled);
    check(g.ScheduledIntersections>0,"Street grid/intersections continue without jobs: "+legacyEnabled);
    check(!g.ServiceStopped&&!g.ServiceRearProtected&&g.RearDepartureProtection==0&&g.JobTrafficYieldRemaining==0,"No service stop or job-specific traffic protection: "+legacyEnabled);
    check(g.ActiveRoute==null&&g.JobPhase==PhoVietGame.WorkPhase.None,"No GPS route or service phase: "+legacyEnabled);
    check(!g.malePassenger.gameObject.activeSelf&&!g.femalePassenger.gameObject.activeSelf,"Both passenger models stay hidden: "+legacyEnabled);
    check(g.Coins==coins&&PlayerPrefs.GetInt("PhoViet.Coins.v1",0)==coins,"Saved coins preserved without rewards: "+legacyEnabled);
    g.TogglePause();check(g.State==PhoVietGame.RideState.Paused,"Manual pause works: "+legacyEnabled);g.TogglePause();check(g.State==PhoVietGame.RideState.Riding,"Manual resume works: "+legacyEnabled);
   }
   // A stale external/editor call cannot accept or complete a job behind the gate.
   g.JobOffers.Add(new PhoVietGame.RideJob{reward=999});prop("JobOffersOpen",true);
   check(!g.AcceptJob(0)&&g.ActiveJob==null,"Stale offer cannot be accepted");
   prop("ActiveJob",g.JobOffers[0]);prop("JobPhase",PhoVietGame.WorkPhase.Alighting);prop("JobOffersOpen",false);
   call("TickService",new object[]{20f});call("BeginService",new object[0]);call("UpdateNavigationVoice",new object[0]);call("SpeakNavigation",new object[]{1});
   check(g.Coins==coins&&PlayerPrefs.GetInt("PhoViet.Coins.v1",0)==coins,"Stale service completion cannot award coins");
   check(!g.ServiceStopped&&!g.ServiceRearProtected,"Stale service state cannot stop rider or traffic");
   check(typeof(PhoVietGame).GetField("navigationAudio",F).GetValue(g)==null,"No navigation voice source created");
   g.RestartRide();clear();prop("Distance",best+123f);g.Crash();
   for(int i=0;i<1200&&g.State==PhoVietGame.RideState.Falling;i++)call("TickRiderCrash",new object[]{.02f});
   check(g.State==PhoVietGame.RideState.GameOver,"Crash still ends distance run after the fall");
   check(g.BestDistance>=best+123,"Distance high score still recorded");
   g.RestartRide();check(g.State==PhoVietGame.RideState.Riding&&!g.JobOffersOpen&&g.ActiveJob==null,"Retry starts another distance-only run");
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();prop("BestDistance",best);g.ResetRide();Time.timeScale=scale;
  }
 }
}
