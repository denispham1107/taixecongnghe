using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyCrashPoliceNavigation
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action<float> tick=dt=>call("TickRiderCrash",new object[]{dt});
  Action<int> reset=lane=>
  {
   g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();clear();call("StartChase",null);
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
   g.policeBike.localPosition=g.rider.localPosition+Vector3.back*1000;g.Crash();clear();int guard=0;
   while(!g.RiderFallComplete&&guard++<1200)tick(.02f);
   check(g.RiderFallComplete&&g.State==PhoVietGame.RideState.Falling,"Pursuit crash settles and waits for an actual officer");
  };
  Func<Vector3,float,PhoVietGame.TrafficVehicle> truck=(p,yaw)=>
  {
   int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfLength>3);var v=g.SpawnTraffic(2,p.z,index);
   v.model.localPosition=v.previousPosition=p;v.model.localRotation=Quaternion.Euler(0,yaw,0);v.previousYaw=yaw;
   v.speed=v.baseTrafficSpeed=0;v.motionVelocity=Vector3.zero;v.knockVelocity=Vector3.zero;v.wallCrashed=v.knocked=false;v.recovery=null;
   v.routeDetached=true;v.routeVelocity=Vector3.zero;return v;
  };
  Action<string,float> finish=(label,dt)=>
  {
   Vector3 previous=g.policeBike.localPosition;int steps=0;
   while(g.State==PhoVietGame.RideState.Falling&&steps++<Mathf.CeilToInt(35/dt))
   {
    tick(dt);var now=g.policeBike.localPosition;
    if(Vector3.Distance(previous,now)>Mathf.Max(12,g.PoliceSpeed+2)*dt+ .2f)throw new Exception("Unbounded police movement in "+label);
    previous=now;
   }
   check(g.State==PhoVietGame.RideState.Arrested,label+" reaches fallen rider and arrests within 35 seconds; final="+g.policeBike.localPosition+" rider="+g.rider.localPosition+" gap="+g.PoliceDistance+" falls="+g.PoliceFalls);
   call("TickArrest",new object[]{1.2f});check(g.State==PhoVietGame.RideState.Jailed&&g.jailRoom.gameObject.activeSelf,label+" completes the prison transition");
  };
  try
  {
   Time.timeScale=0;
   // The officer must sometimes drive away from the target to get out of a U-shaped wreck pocket.
   reset(2);var r=g.rider.localPosition;
   truck(r+new Vector3(-2.7f,0,9),0);truck(r+new Vector3(2.7f,0,9),0);truck(r+new Vector3(0,0,5),90);
   g.policeBike.localPosition=r+new Vector3(0,0,10);finish("Officer ahead inside a three-truck pocket",.02f);
   foreach(int lane in new[]{0,2,3})foreach(int side in new[]{-1,1})foreach(float dt in new[]{.02f,.05f,.1f})foreach(int layout in new[]{0,1,2})
   {
    reset(lane);r=g.rider.localPosition;
    if(layout==1)truck(r+new Vector3(0,0,7*side),90);
    if(layout==2){truck(r+new Vector3(-2.7f,0,9*side),0);truck(r+new Vector3(2.7f,0,9*side),0);truck(r+new Vector3(0,0,5*side),90);}
    g.policeBike.localPosition=r+Vector3.forward*(layout==2?10:15)*side;
    finish("Approach lane="+lane+" side="+side+" dt="+dt+" layout="+layout,dt);
   }
   foreach(int side in new[]{-1,1})
   {
    reset(2);r=g.rider.localPosition;truck(r,0);g.policeBike.localPosition=r+Vector3.forward*15*side;
    finish("Arrest position beside the vehicle covering the fallen rider: "+side,.05f);
    reset(2);r=g.rider.localPosition;truck(r+Vector3.forward*6*side,90);g.policeBike.localPosition=r+Vector3.forward*15*side;
    typeof(PhoVietGame).GetProperty("PoliceFallRemaining").SetValue(g,1.2f);
    finish("Officer gets up, detours and captures: "+side,.02f);
   }
   reset(2);r=g.rider.localPosition;truck(r+Vector3.forward*5,90);g.policeBike.localPosition=r+Vector3.forward*14;
   for(int i=0;i<8;i++)tick(.02f);int replans=g.CrashPoliceReplans;var parked=truck(r+new Vector3(-6,0,10),0);parked.routeVelocity=Vector3.right*6;parked.motionVelocity=parked.routeVelocity;
   finish("Moving traffic changes the planned route",.02f);check(g.CrashPoliceReplans>=replans,"Dynamic path invalidation never rolls back navigation state");
   reset(2);r=g.rider.localPosition;truck(r+Vector3.forward*5,90);g.policeBike.localPosition=r+Vector3.forward*14;tick(.02f);
   var beforePause=g.policeBike.localPosition;int planCount=g.CrashPoliceReplans;g.TogglePause();tick(2);
   check(g.policeBike.localPosition==beforePause&&g.CrashPoliceReplans==planCount,"Pause freezes crash pursuit movement and path planning");g.TogglePause();finish("Resumed crash pursuit",.02f);
   g.RestartRide();check(g.CrashPoliceReplans==0&&!g.CrashDuringPursuit&&!g.IsChased,"Restart clears cached crash navigation and terminal state");
   foreach(bool pickup in new[]{true,false})
   {
    g.jobsEnabled=true;g.RestartRide();g.ShowJobOffers();g.AcceptJob(0);clear();typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,pickup?PhoVietGame.WorkPhase.Pickup:PhoVietGame.WorkPhase.Delivery);
    call("BeginService",null);call("StartChase",null);g.policeBike.localPosition=g.rider.localPosition+Vector3.back*1000;g.Crash();clear();int wait=0;while(!g.RiderFallComplete&&wait++<1200)tick(.02f);
    r=g.rider.localPosition;truck(r+Vector3.forward*6,90);g.policeBike.localPosition=r+Vector3.forward*15;
    finish("Crash while passenger service is active: pickup="+pickup,.02f);
   }
   foreach(int direction in new[]{-1,1})foreach(float angle in new[]{30f,60f})foreach(float dt in new[]{.02f,.05f})
   {
    g.jobsEnabled=false;g.RestartRide();clear();int turnLane=direction<0?0:3;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,turnLane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[turnLane],0,0);
    int guard=0;while(g.NextJunctionDistance>22&&guard++<1000)call("TickRide",new object[]{dt});g.MoveLane(direction);
    while((!g.IsTurning||Mathf.Abs(g.TurnYaw)<angle)&&guard++<1600)call("TickRide",new object[]{dt});
    check(g.IsTurning,"Real junction turn reached before pursuit crash: "+direction+","+angle+","+dt);
    call("StartChase",null);g.policeBike.localPosition=new Vector3(PhoVietGame.LaneX[2],0,g.NextJunctionDistance-15);g.Crash();clear();
    finish("Officer reaches rider thrown during a real junction turn: "+direction+","+angle+","+dt,dt);
   }
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;
  }
 }
}
