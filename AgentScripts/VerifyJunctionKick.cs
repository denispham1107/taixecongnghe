using System;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyJunctionKick
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
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action<int,float> prepare=(turn,fraction)=>
  {
   g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();clear();
   while(g.NextJunctionDistance>11.65f){tick(.02f);clear();}
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,turn<0?1:2);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[g.CurrentLane],0,0);
   if(turn==0)return;typeof(PhoVietGame).GetProperty("QueuedTurn").SetValue(g,turn);call("StartJunctionTurnAt",new object[]{false,g.NextJunctionDistance});
   float length=(float)typeof(PhoVietGame).GetField("turnLength",F).GetValue(g);
   while((float)typeof(PhoVietGame).GetField("turnTravel",F).GetValue(g)<length*fraction){tick(.01f);clear();}
  };
  Func<float,int,bool,PhoVietGame.TrafficVehicle> spawn=(heading,side,cross)=>
  {
   Quaternion q=Quaternion.Euler(0,g.TurnYaw,0);int index=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);var v=g.SpawnTraffic(3,0,index);
   v.model.localPosition=g.rider.localPosition+q*new Vector3(side*3.1f,0,heading==180?(g.IsTurning?9:12):Mathf.Abs(heading)==90?3:0);v.previousPosition=v.model.localPosition;v.model.localRotation=q*Quaternion.Euler(0,heading,0);
   v.routeDetached=true;v.routeVelocity=v.model.localRotation*Vector3.forward*(heading==180?12:heading==0?17:6);v.crossing=cross;v.crossSpeed=v.routeVelocity.x;v.speed=v.baseTrafficSpeed=v.routeVelocity.z;if(g.IsTurning&&heading==0){v.roadTurning=true;float angle=Mathf.Abs(g.TurnYaw)*Mathf.Deg2Rad;int direction=g.TurnYaw<0?-1:1;v.roadTurnAngle=angle;v.roadTurnRadius=Mathf.Max(4,10-direction*side*3.1f);v.roadTurnSpeed=17;v.roadTurnForward=Vector3.forward;v.roadTurnSide=Vector3.right*direction;v.roadTurnStart=v.model.localPosition-v.roadTurnRadius*(Vector3.forward*Mathf.Sin(angle)+v.roadTurnSide*(1-Mathf.Cos(angle)));}return v;
  };
  var checks=new List<string>();var failures=new List<string>();var cases=new List<object>();var coverage=new HashSet<string>();
  Action<bool,string> check=(ok,s)=>{if(ok)checks.Add(s);else failures.Add(s);};
  try
  {
   Time.timeScale=0;
   foreach(float dt in new[]{.02f,.05f})foreach(int turn in new[]{0,-1,1})foreach(int side in new[]{-1,1})foreach(float heading in new[]{0f,90f,-90f,180f})
   {
    prepare(turn,.4f);int lane=g.CurrentLane;var target=spawn(heading,side,Mathf.Abs(heading)==90);string label=$"dt={dt},turn={turn},side={side},heading={heading}";
    bool selected=g.TryKick();check(selected,"Nearby junction bike is eligible regardless of heading: "+label);check(!target.knocked&&g.KickCount==0,"No force before actual foot contact: "+label);
    float age=0,hitAt=-1;for(int i=0;i<100&&g.State==PhoVietGame.RideState.Riding&&g.KickAssistActive;i++){tick(dt);age+=dt;if(target.knockFromPlayer&&hitAt<0)hitAt=age;}
    if(target.knockFromPlayer)coverage.Add(turn+"/"+heading);check(target.knockFromPlayer?g.KickCount==1&&g.LastKickFootGap<=.081f&&g.LastKickFootGap>=0:g.KickCount==0&&!target.knocked,"Only actual foot contact hits; escaping target may safely miss: "+label);
    check(g.State==PhoVietGame.RideState.Riding&&!g.KickAssistActive&&g.CurrentLane==lane,"Kick returns to road without false impact: "+label+",state="+g.State);
    cases.Add(new{label,selected,hitAt,age,hit=target.knockFromPlayer,gap=g.LastKickFootGap,state=g.State.ToString()});
   }
   foreach(int direction in new[]{-1,1})
   {
    prepare(0,.4f);var crossing=g.SpawnCrossTraffic(direction,3,false,6);crossing.model.localPosition=new Vector3(g.rider.localPosition.x-direction*3.1f,0,3);crossing.previousPosition=crossing.model.localPosition;
    check(g.TryKick(),"Actual SpawnCrossTraffic controller can be targeted: "+direction);check(!crossing.knocked,"Crossing controller has no force before shoe contact: "+direction);
    for(int i=0;i<100&&g.State==PhoVietGame.RideState.Riding&&g.KickAssistActive;i++)tick(.02f);
    check(crossing.knockFromPlayer&&g.KickCount==1&&g.LastKickFootGap<=.081f,"Actual TickIntersectionVehicle motion receives physical shoe contact: "+direction);
    check(g.State==PhoVietGame.RideState.Riding&&!g.KickAssistActive,"Actual crossing kick returns safely: "+direction);
   }
   foreach(int turn in new[]{0,-1,1})foreach(float heading in new[]{0f,90f,-90f,180f})check(coverage.Contains(turn+"/"+heading),"Physical hits cover heading "+heading+" while turn="+turn);
   foreach(int turn in new[]{-1,1})
   {
    prepare(turn,.85f);var target=spawn(0,turn<0?1:-1,false);bool selected=g.TryKick();bool crossed=false;for(int i=0;i<120&&g.State==PhoVietGame.RideState.Riding&&(i==0||g.KickAssistActive);i++){tick(.02f);crossed|=g.CompletedTurns>0;}
    check(selected&&crossed&&target.knockFromPlayer&&g.State==PhoVietGame.RideState.Riding,"Active kick and actual foot contact survive world rebase on turn "+turn);
    prepare(turn,.4f);target=spawn(0,1,false);int carIndex=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f);var car=g.SpawnTraffic(3,0,carIndex);car.model.localPosition=Vector3.Lerp(g.rider.localPosition,target.model.localPosition,.5f);car.model.localRotation=g.rider.localRotation;car.routeDetached=true;car.routeVelocity=target.routeVelocity;
    check(!g.TryKick()&&!target.knocked,"Car blocking curved approach prevents a kick through it: "+turn);
   }
   return new{success=failures.Count==0,count=checks.Count,failures,checks,cases};
  }
  finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
