using System;using System.Reflection;using UnityEngine;using PhoViet;
public static class ProbeBoostKick
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  Action reset=()=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,10000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);};
  Action tick=()=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{.02f});
  try
  {
   Time.timeScale=0;reset();var v=g.SpawnTraffic(3,0,1);v.speed=v.baseTrafficSpeed=12;tick();float before=v.speed;g.TryBoost();tick();float after=v.speed;
   reset();var target=g.SpawnTraffic(3,2,1);target.speed=target.baseTrafficSpeed=28;bool selected=g.TryKick();for(int i=0;i<30&&g.State==PhoVietGame.RideState.Riding;i++)tick();
   return new{success=true,otherBikeBefore=before,otherBikeAfterPlayerBoost=after,boostLeaksToTraffic=after>before+1,fastNearbyBikeSelected=selected,fastBikeHit=target.knocked};
  }
  finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
