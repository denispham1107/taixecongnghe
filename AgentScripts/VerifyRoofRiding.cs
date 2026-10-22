using System;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyRoofRiding
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  Action reset=()=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  var checks=new List<string>();var failures=new List<string>();int cases=0;
  Action<bool,string> check=(ok,s)=>{if(ok)checks.Add(s);else failures.Add(s);};
  try
  {
   Time.timeScale=0;
   foreach(string asset in new[]{"PV_Sedan","PV_Hatchback","PV_Taxi","PV_DeliveryVan","PV_BoxTruck","PV_DrowsyTruck"})foreach(float dt in new[]{.016f,.033f,.05f,.1f})foreach(bool boost in new[]{false,true})
   {
    reset();int index=g.vehicles.FindIndex(v=>v.model.GetComponent<CarVisualAnimator>()?.AssetName==asset);var pool=g.vehicles[index];pool.model.localRotation=Quaternion.identity;
    float roof=PhoVietGame.JumpRoofHeight(pool)+.025f;if(boost)g.TryBoost();g.TryJump();float time=0,oldHeight=0,oldDistance=0;
    for(int i=0;i<300;i++){oldHeight=g.RiderJumpHeight;oldDistance=g.Distance;tick(dt);time+=dt;if(oldHeight>roof&&g.RiderJumpHeight<=roof)break;}
    float fraction=Mathf.InverseLerp(oldHeight,g.RiderJumpHeight,roof),contactTime=time-dt+fraction*dt,contactDistance=Mathf.Lerp(oldDistance,g.Distance,fraction);
    foreach(float speed in new[]{-12f,0f,7f,28f})foreach(float offset in new[]{-.5f,0f,.5f})
    {
     reset();bool eventOnly=pool.eventOnly;pool.eventOnly=false;var v=g.SpawnTraffic(2,contactDistance-speed*contactTime+offset,index);pool.eventOnly=eventOnly;
     v.speed=v.baseTrafficSpeed=speed;v.model.localRotation=Quaternion.Euler(0,speed<0?180:0,0);v.previousPosition=v.model.localPosition;
     if(boost)g.TryBoost();check(g.TryJump(),$"takeoff {asset}/{dt}/{boost}/{speed}/{offset}");bool supported=false;float supportTime=0;
     for(int i=0;i<Mathf.CeilToInt(4/dt)&&g.State==PhoVietGame.RideState.Riding;i++){tick(dt);supported|=g.RiderOnRoof||typeof(PhoVietGame).GetField("roofContactVehicle",F).GetValue(g)==v;if(g.RiderOnRoof)supportTime+=dt;}
     string label=$"{asset},dt={dt},boost={boost},speed={speed},offset={offset},state={g.State},height={g.RiderJumpHeight},support={supportTime}";
     check(supported,"Real ballistic jump contacts roof: "+label);
     check(g.State==PhoVietGame.RideState.Riding,"Landing, roof travel and roof exit preserve normal riding: "+label);
     check(g.RiderOnRoof||!g.RiderAirborne&&g.RiderJumpHeight==0,"After flight bike is supported by road or moving roof: "+label);cases++;
    }
   }
   return new{success=failures.Count==0,count=checks.Count,cases,failures,checks};
  }
  finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
