using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyMeshyCarRoofs
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);bool surprises=g.trafficSurprises,danger=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action reset=()=>{g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  var checks=new List<string>();var failures=new List<string>();var impacts=new List<object>();int cases=0;Action<bool,string> check=(b,s)=>{if(b)checks.Add(s);else failures.Add(s);};
  try
  {
   Time.timeScale=0;
   foreach(string asset in new[]{"PV_MeshySport","PV_MeshyPickup","PV_MeshyCovered","PV_MeshyContainer"})foreach(float dt in new[]{.016f,.05f})foreach(bool boost in new[]{false,true})foreach(float speed in new[]{-12f,0f,7f,28f})
   {
    reset();int slot=g.vehicles.FindIndex(v=>v.model.GetComponent<MeshyCarVariants>()?.variants.Any(e=>e.asset==asset)==true);var pool=g.vehicles[slot];var c=pool.model.GetComponent<MeshyCarVariants>();int variant=Array.FindIndex(c.variants,e=>e.asset==asset);c.Select(variant,0,pool);
    float roof=c.SupportHeight(g.IsPlayerCar?-1.1643f:-.73f,g.IsPlayerCar?1.2105f:.90f)+.025f;if(boost)g.TryBoost();g.TryJump();float t=0,h=0,d=0;
    for(int i=0;i<300;i++){h=g.RiderJumpHeight;d=g.Distance;tick(dt);t+=dt;if(h>roof&&g.RiderJumpHeight<=roof)break;}
    float fraction=Mathf.InverseLerp(h,g.RiderJumpHeight,roof),contactTime=t-dt+fraction*dt,contactDistance=Mathf.Lerp(d,g.Distance,fraction);
    reset();bool eventOnly=pool.eventOnly;pool.eventOnly=false;var v=g.SpawnTraffic(2,contactDistance-speed*contactTime,slot);pool.eventOnly=eventOnly;c.Select(variant,0,v);
    v.speed=v.baseTrafficSpeed=speed;v.model.localRotation=Quaternion.Euler(0,speed<0?180:0,0);v.previousPosition=v.model.localPosition;
    if(boost)g.TryBoost();check(g.TryJump(),"takeoff "+asset+"/"+dt+"/"+boost+"/"+speed);bool supported=false;
    for(int i=0;i<Mathf.CeilToInt(5/dt)&&g.State==PhoVietGame.RideState.Riding;i++){float beforeHeight=g.RiderJumpHeight;string beforePosition=g.rider.localPosition.ToString(),beforeTarget=v.model.localPosition.ToString();tick(dt);supported|=g.RiderOnRoof||typeof(PhoVietGame).GetField("roofContactVehicle",F).GetValue(g)==v;if(g.State!=PhoVietGame.RideState.Riding)impacts.Add(new{asset,dt,boost,speed,frame=i,beforeHeight,beforePosition,beforeTarget,afterPosition=g.rider.localPosition.ToString(),target=v.model.localPosition.ToString(),scenery=g.LastSceneryImpact});}
    string label=asset+"/dt="+dt+"/boost="+boost+"/speed="+speed+"/state="+g.State+"/height="+g.RiderJumpHeight;
    // With the larger player car, this specific rear-container setup reaches
    // the rear bumper during takeoff (<0.7 m high). It is a real cab impact,
    // not a roof landing; preserve the crash instead of adding jump immunity.
    bool tooLate=g.IsPlayerCar&&asset=="PV_MeshyContainer"&&!boost&&speed==28;
    if(tooLate){check(!supported&&g.State==PhoVietGame.RideState.Falling,"Rear container reaches car before sufficient jump clearance: "+label);check(g.LastRiderImpactSpeed>1,"Insufficient-height jump retains physical collision: "+label);}
    else{check(supported,"Ballistic landing on received roof: "+label);check(g.State==PhoVietGame.RideState.Riding,"Roof travel/exit has no false collision: "+label);check(g.RiderOnRoof||!g.RiderAirborne&&g.RiderJumpHeight==0,"Final support: "+label);}cases++;
   }
   return new{success=failures.Count==0,count=checks.Count,cases,failures,impacts,checks};
  }
  finally{g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
