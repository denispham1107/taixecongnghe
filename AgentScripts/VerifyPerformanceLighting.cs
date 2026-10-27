using System;using System.Linq;using System.Reflection;using System.IO;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyPerformanceLighting
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float time=Time.timeScale;bool river=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprise=g.trafficSurprises,exits=g.turnExitTrafficEnabled;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};var l=g.daylight;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  var tick=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),g,typeof(PhoVietGame).GetMethod("TickRide",F));
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
   check(l.streets.Length==350&&l.vehicles.Count(v=>v.player)==1&&l.lightPool.Length==6,"Current scene retains 350 street sources, player binding and six real lights");
   float previous=0;for(int i=0;i<=300;i++){l.SetCycleTime(i*.1f);check(l.NightLevel>=previous-1e-6f,"Continuous dawn to night "+i);previous=l.NightLevel;}
   check(l.ActiveRealtimeLights>0&&l.ActiveRealtimeLights<=6&&l.LampLevel==1,"Night enables original bounded headlight/street pool");
   for(int i=301;i<=600;i++){l.SetCycleTime(i*.1f);check(l.NightLevel<=previous+1e-6f,"Continuous night to dawn "+i);previous=l.NightLevel;}
   check(l.ActiveRealtimeLights==0&&l.LampLevel==0,"Day switches off vehicle and street illumination");
   l.SetCycleTime(2);var fog=l.morningFog;float strength=fog.Strength;g.TogglePause();l.AdvanceSimulation(3);l.UpdateLamps();check(l.ElapsedTime==2&&fog.Strength==strength,"Pause freezes pending visual clock");g.TogglePause();
   l.AdvanceSimulation(.5f);l.UpdateLamps();check(l.ElapsedTime==2.5f&&fog.Strength<strength,"Resume disperses the same opening fog");
   previous=1;for(int i=0;i<=60;i++){l.SetCycleTime(i*.1f);check(fog.Strength<=previous+1e-6f,"Fog fades monotonically "+i);previous=fog.Strength;}
   check(fog.Strength==0&&!fog.enabled,"Fog disables its depth integration after six seconds");
   foreach(float phase in new[]{0f,15f,30f,45f,59.99f})
   {
    l.SetCycleTime(phase);for(int i=0;i<10;i++)l.AdvanceSimulation(.005f);l.UpdateLamps();var angle=l.sun.transform.rotation;var color=l.sun.color;float elapsed=l.ElapsedTime,level=l.LampLevel;
    l.SetCycleTime(phase);for(int i=0;i<10;i++)l.Advance(.005f);
    check(Mathf.Abs(elapsed-l.ElapsedTime)<1e-6f&&Quaternion.Angle(angle,l.sun.transform.rotation)<.001f&&Vector4.Distance(color,l.sun.color)<1e-5f&&level==l.LampLevel,"Coalesced and immediate optics identical at "+phase);
   }
   l.SetCycleTime(30);set("timeRiding",100f);var car=g.rider.GetComponent<PlayerCarVisual>();var binding=l.vehicles.Single(v=>v.player);car.Advance(.01f);l.UpdateLamps();float ground=binding.head.position.y,peak=ground;
   check(g.TryJump(),"Current Supercar jump starts");var block=new MaterialPropertyBlock();
   for(int i=0;i<240;i++)
   {
    tick(.01f);car.Advance(.01f);l.UpdateLamps();peak=Mathf.Max(peak,binding.head.position.y);
    if(i%12!=0)continue;var light=l.lightPool[0];check(Vector3.Distance(light.transform.position,binding.head.position)<.001f,"Headlight follows final animated source "+i);
    binding.roadLight.GetPropertyBlock(block);var matrix=block.GetMatrix("_WorldToHead");check(matrix.MultiplyPoint3x4(binding.head.position).magnitude<.001f,"Road projection follows jump pose "+i);
   }
   check(peak-ground>4.5f&&!g.RiderAirborne&&g.State==PhoVietGame.RideState.Riding,"Full jump lands safely; headlight climbed with car");
   typeof(PhoVietGame).GetProperty("State").SetValue(g,PhoVietGame.RideState.Jailed);l.UpdateLamps();check(l.ActiveRealtimeLights==0&&!binding.roadLight.enabled,"Jail hides all detached road lights");
   check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   var report=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/performance-lighting-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=river;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprise;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=time;}
 }
}
