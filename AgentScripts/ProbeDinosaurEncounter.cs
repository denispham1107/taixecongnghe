using System;using System.Reflection;using System.Collections.Generic;using System.IO;using UnityEngine;using PhoViet;
public static class ProbeDinosaurEncounter
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,reckless=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var cases=new List<object>();var failures=new List<string>();
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  try
  {
   Time.timeScale=0;
   foreach(float dt in new[]{1f/60,1f/30,.05f})foreach(float speed in new[]{17f,30f})foreach(bool boost in new[]{false,true})foreach(int lane in new[]{0,1,2,3})foreach(bool jump in new[]{false,true})
   {
    g.RestartRide();g.riversEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.dinosaursEnabled=true;
    foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);set("lastX",g.rider.localPosition.x);
    set("timeRiding",Mathf.Max(0,(speed-17)/.36f));if(boost)g.TryBoost();
    var d=g.SpawnDinosaur();d.root.localPosition=new Vector3(0,0,Mathf.Max(60,speed*(boost?2.25f:1)*2.5f+20));set("dinosaurTimer",10000f);bool pressed=false;float height=0,time=0;
    for(;time<5&&g.State==PhoVietGame.RideState.Riding;time+=dt)
    {if(jump&&!pressed&&d.visual.SweepAge>=.55f){pressed=g.TryJump();}tick(dt);height=Mathf.Max(height,g.RiderJumpHeight);}
    bool ok=jump?pressed&&g.State==PhoVietGame.RideState.Riding&&!g.HitByDinosaur:g.State==PhoVietGame.RideState.Falling&&g.HitByDinosaur&&g.DinosaurTailHits>0;
    if(!ok)failures.Add("dt="+dt+",speed="+speed+",boost="+boost+",lane="+lane+",jump="+jump+",state="+g.State+",tail="+g.DinosaurTailHits);
    cases.Add(new{dt,speed,boost,lane,jump,pressed,state=g.State.ToString(),hit=g.HitByDinosaur,tail=g.DinosaurTailHits,height,time,attack=d.visual.SweepAge});
   }
   var result=new{success=failures.Count==0,count=cases.Count,failures,cases};File.WriteAllText("Captures/dinosaur-encounter-probe.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
