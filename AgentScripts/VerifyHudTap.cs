using System;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyHudTap
{
 public static object Main()
 {
  if(!Application.isPlaying||Screen.width<Screen.height)throw new Exception("Wide Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();bool jobs=g.jobsEnabled,sound=g.sound;float time=Time.timeScale;
  var checks=new List<string>();Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception(name);checks.Add(name);};
  Action<float,float> tap=(x,y)=>g.ApplyHudTap(JsonUtility.ToJson(new Tap{x=x,y=y}));
  try
  {
   Time.timeScale=0;g.jobsEnabled=false;g.RestartRide();g.ApplyBrowserInsets("{\"left\":0,\"top\":0,\"right\":0,\"bottom\":0}");
   float scale=Mathf.Min(Screen.width/1280f,Screen.height/420f),x=(Screen.width/scale-1000)/2;
   tap((x+964)*scale/Screen.width,46*scale/Screen.height);
   check(g.State==PhoVietGame.RideState.Paused,"Browser HUD tap pauses game");
   tap((x+964)*scale/Screen.width,46*scale/Screen.height);
   check(g.State==PhoVietGame.RideState.Riding,"Browser HUD tap resumes game");
   tap(.5f,.5f);check(g.State==PhoVietGame.RideState.Riding,"World tap does not pause");
   tap(-1,0);tap(0,2);check(g.State==PhoVietGame.RideState.Riding,"Invalid coordinates ignored");
   tap((x+940)*scale/Screen.width,81*scale/Screen.height);
   check(g.sound!=sound,"Landscape sound tap toggles audio");
   check(g.State==PhoVietGame.RideState.Riding,"Audio tap does not interrupt ride");
   g.jobsEnabled=true;g.ShowJobOffers();
   tap((x+964)*scale/Screen.width,46*scale/Screen.height);
   check(g.JobOffersOpen&&g.State==PhoVietGame.RideState.Paused,"HUD tap cannot resume job selection");
   return new{success=true,count=checks.Count,checks};
  }
  finally{g.sound=sound;g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=time;}
 }
 [Serializable]class Tap{public float x,y;}
}
