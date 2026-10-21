using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyJailLighting
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var l=g.daylight;float scale=Time.timeScale;int count=0;
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);count++;};
  try
  {
   Time.timeScale=0;g.ResetRide();l.SetCycleTime(30);l.UpdateLamps();
   check(l.vehicles.Any(v=>v.roadLight.enabled)&&l.ActiveRealtimeLights>0,"Night setup has active road spills and real lamps");
   typeof(PhoVietGame).GetProperty("State").SetValue(g,PhoVietGame.RideState.Jailed);
   typeof(DayNightLighting).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(l,null);
   check(l.vehicles.All(v=>!v.roadLight.enabled&&v.halos.All(h=>!h.glow.enabled)),"Jail hides all detached vehicle optics, even after the traffic models disappear");
   check(l.streets.All(s=>!s.pool.enabled&&!s.glow.enabled),"Jail hides street pools and halos");
   check(l.ActiveRealtimeLights==0&&l.lightPool.All(s=>!s.enabled),"Jail has no remaining real street/vehicle lights");
   check(!l.sun.enabled&&!l.moon.enabled,"Street sun and moon do not affect jail lighting");
   check(g.rideCamera.clearFlags==CameraClearFlags.SolidColor,"Jail uses its own background");
   g.ResetRide();check(l.sun.enabled&&l.moon.enabled&&g.rideCamera.clearFlags==CameraClearFlags.Skybox&&l.NightLevel==0,"Restart restores the morning and skybox");
   return new{success=true,count};
  }
  finally{g.ResetRide();Time.timeScale=scale;}
 }
}
