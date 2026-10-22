using System;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyBoostTrafficIsolation
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var rows=new List<object>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  Action reset=()=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);set("timeRiding",25f);tick(.02f);};
  Func<string,PhoVietGame.TrafficVehicle> spawn=kind=>
  {
   if(kind.StartsWith("racer")||kind.StartsWith("drowsy"))
   {
    bool truck=kind.StartsWith("drowsy"),incoming=kind.EndsWith("oncoming");var v=g.SpawnReckless(truck,incoming);v.willHitWall=false;v.swerveAmplitude=.05f;v.swerveFrequency=2;v.swerveDirection=1;v.swerveSpeed=10;
    v.speed=v.baseTrafficSpeed=incoming?-12:kind.EndsWith("rear")?40:30;v.enteringFromBehind=kind.EndsWith("rear");v.model.localPosition=new Vector3(4.65f,0,v.enteringFromBehind?-35:35);v.startPosition=v.model.localPosition;v.rearStartX=4.65f;return v;
   }
   if(kind=="crossing")return g.SpawnCrossTraffic(1,35,false,10);
   int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&(kind.StartsWith("bike")?v.halfWidth<.7f:kind=="truck"?v.halfLength>3:kind=="van"?v.model.GetComponent<CarVisualAnimator>()?.AssetName=="PV_DeliveryVan":v.halfWidth>.7f&&v.halfLength<3));
   if(index<0)throw new Exception("Missing vehicle kind: "+kind);var vehicle=g.SpawnTraffic(3,kind=="bike-rear"?-35:kind=="bike-oncoming"?35:0,index);
   vehicle.speed=vehicle.baseTrafficSpeed=kind=="bike-oncoming"?-12:kind=="bike-rear"?40:12;
   vehicle.enteringFromBehind=kind=="bike-rear";vehicle.rearEntryBoost=11;vehicle.model.localRotation=Quaternion.Euler(0,vehicle.speed<0?180:0,0);
   if(kind=="bike-detached"){vehicle.routeDetached=true;vehicle.routeVelocity=Vector3.forward*12;}
   return vehicle;
  };
  try
  {
   Time.timeScale=0;
   foreach(string kind in new[]{"bike","car","van","truck","bike-rear","bike-oncoming","bike-detached","crossing","racer-rear","drowsy-rear","racer-weave","drowsy-weave","racer-oncoming","drowsy-oncoming"})
   {
    float[] control=new float[15],boosted=new float[15];Vector3[] controlMotion=new Vector3[15],boostMotion=new Vector3[15];float riderControl=0,riderBoost=0;
    for(int pass=0;pass<2;pass++)
    {
     reset();var v=spawn(kind);if(pass==1)check(g.TryBoost(),"Player boost accepted with "+kind+" present");
     for(int frame=0;frame<15;frame++)
     {
      Vector3 before=v.model.localPosition;tick(.02f);Vector3 actual=(v.model.localPosition-before)/.02f+Vector3.forward*g.Speed;
      if(pass==0){control[frame]=v.speed;controlMotion[frame]=actual;riderControl=g.Speed;}else{boosted[frame]=v.speed;boostMotion[frame]=actual;riderBoost=g.Speed;}
      check(v.active&&!v.knocked&&g.State==PhoVietGame.RideState.Riding,"Isolation scenario remains collision-free: "+kind+",pass="+pass+",frame="+frame);
     }
    }
    float maxSpeedError=0,maxMotionError=0;for(int i=0;i<15;i++){maxSpeedError=Mathf.Max(maxSpeedError,Mathf.Abs(control[i]-boosted[i]));maxMotionError=Mathf.Max(maxMotionError,(controlMotion[i]-boostMotion[i]).magnitude);}
    check(maxSpeedError<.002f,"W cannot change NPC world speed: "+kind+",error="+maxSpeedError);
    check(maxMotionError<.004f,"World displacement matches unboosted control after removing scroll: "+kind+",error="+maxMotionError);
    check(Mathf.Abs(riderBoost/riderControl-2.25f)<.001f,"Only player receives 2.25x multiplier with "+kind);
    rows.Add(new{kind,maxSpeedError,maxMotionError,riderControl,riderBoost,otherSpeed=boosted[14]});
   }
   float[] spawnSpeeds=new float[4];
   for(int pass=0;pass<2;pass++)foreach(bool truck in new[]{false,true})
   {reset();if(pass==1){g.TryBoost();tick(.02f);}set("rng",new System.Random(1234));var v=g.SpawnReckless(truck);spawnSpeeds[pass*2+(truck?1:0)]=v.speed;}
   check(Mathf.Abs(spawnSpeeds[0]-spawnSpeeds[2])<.03f&&Mathf.Abs(spawnSpeeds[1]-spawnSpeeds[3])<.03f,"Dangerous vehicles spawned during W use independent speed, not boosted player speed");
   float normalPolice=0,boostPolice=0;
   for(int pass=0;pass<2;pass++){reset();if(pass==1){g.TryBoost();tick(.02f);}call("StartChase",null);if(pass==0)normalPolice=g.PoliceSpeed;else boostPolice=g.PoliceSpeed;}
   check(Mathf.Abs(normalPolice-boostPolice)<.03f,"New police pursuit cannot inherit the player's temporary surge");
   return new{success=true,count=checks.Count,checks,rows};
  }
  finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
