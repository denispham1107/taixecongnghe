using System;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class ProbeJunctionRoof
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action reset=()=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  var roofs=new List<object>();var kicks=new List<object>();int attempted=0,failed=0;
  try
  {
   Time.timeScale=0;
   foreach(string name in new[]{"PV_Sedan","PV_Hatchback","PV_Taxi","PV_DeliveryVan","PV_BoxTruck","PV_DrowsyTruck"})
   foreach(float dt in new[]{.016f,.033f,.05f,.1f})foreach(float speed in new[]{-12f,7f,17f,28f})foreach(float z in new[]{-.9f,0f,.8f})
   {
    reset();int index=g.vehicles.FindIndex(v=>v.model.GetComponent<CarVisualAnimator>()?.AssetName==name);var pool=g.vehicles[index];bool eventOnly=pool.eventOnly;pool.eventOnly=false;var v=g.SpawnTraffic(2,z,index);pool.eventOnly=eventOnly;
    v.speed=v.baseTrafficSpeed=speed;v.model.localRotation=Quaternion.Euler(0,speed<0?180:0,0);v.previousPosition=v.model.localPosition;
    float surface=(float)call("RoofSurface",new object[]{v,g.rider.localPosition});float y=surface+.08f;
    set("jumpHeight",y);set("jumpPreviousHeight",y);set("jumpVelocity",-4f);set("jumpFlying",true);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],y,0);
    bool landed=false;string phase="descent";for(int i=0;i<Mathf.CeilToInt(2.5f/dt)&&g.State==PhoVietGame.RideState.Riding;i++){call("TickRide",new object[]{dt});landed|=g.RiderOnRoof;if(g.RiderOnRoof)phase="roof";else if(landed)phase="exit";}
    attempted++;if(g.State!=PhoVietGame.RideState.Riding){failed++;roofs.Add(new{name,dt,speed,z,surface,landed,phase,state=g.State.ToString(),height=g.rider.localPosition.y,carX=v.model.localPosition.x,carZ=v.model.localPosition.z});}
   }
   foreach(int turn in new[]{0,-1,1})foreach(float heading in new[]{0f,90f,-90f,180f})
   {
    reset();while(g.NextJunctionDistance>11.65f){call("TickRide",new object[]{.02f});foreach(var other in g.vehicles){other.active=false;other.model.gameObject.SetActive(false);}}if(turn!=0){typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,turn<0?1:2);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[g.CurrentLane],0,0);typeof(PhoVietGame).GetProperty("QueuedTurn").SetValue(g,turn);call("StartJunctionTurnAt",new object[]{false,g.NextJunctionDistance});float length=(float)typeof(PhoVietGame).GetField("turnLength",F).GetValue(g);while((float)typeof(PhoVietGame).GetField("turnTravel",F).GetValue(g)<length*.4f)call("TickRide",new object[]{.02f});}
    Quaternion q=Quaternion.Euler(0,g.TurnYaw,0);int index=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);var target=g.SpawnTraffic(3,0,index);
    target.model.localPosition=g.rider.localPosition+q*new Vector3(3.1f,0,heading==180?12:heading==0?0:3f);target.previousPosition=target.model.localPosition;target.model.localRotation=q*Quaternion.Euler(0,heading,0);
    target.routeDetached=true;target.routeVelocity=target.model.localRotation*Vector3.forward*(heading==180?12:heading==0?17:6);target.crossing=Mathf.Abs(heading)==90;target.crossSpeed=target.routeVelocity.x;target.speed=target.baseTrafficSpeed=target.routeVelocity.z;
    bool valid=(bool)call("KickTargetValid",new object[]{target});float sideGap=(float)call("KickSideGap",new object[]{target});
    bool selected=g.TryKick();var trace=new List<string>();for(int i=0;i<45&&g.KickAssistActive&&g.State==PhoVietGame.RideState.Riding;i++){call("TickRide",new object[]{.02f});trace.Add($"{i}:pos={g.rider.localPosition},target={target.model.localPosition},yaw={g.TurnYaw},speed={g.Speed},swing={g.KickSwingActive},gap={g.LastKickFootGap},active={g.KickAssistActive}");}
    kicks.Add(new{turn,heading,valid,sideGap,selected,hit=target.knockFromPlayer,gap=g.LastKickFootGap,state=g.State.ToString(),trace});
   }
   var feet=new List<string>();var animator=g.rider.GetComponent<BikeVisualAnimator>();for(int i=0;i<=10;i++)feet.Add(i+" "+animator.KickFootLocal(1,i/10f,true));return new{success=true,attempted,failed,roofs,kicks,feet};
  }
  finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
