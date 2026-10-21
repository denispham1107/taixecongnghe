using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifySmoothTurns
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  var checks=new List<string>();var times=new List<double>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object> get=n=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> tick=dt=>{var sw=System.Diagnostics.Stopwatch.StartNew();call("TickRide",new object[]{dt});times.Add(sw.Elapsed.TotalMilliseconds);};
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",100000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action reset=()=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();clear();};
  Action<int,float> approach=(lane,near)=>
  {
   int guard=0;while(g.NextJunctionDistance>near&&guard++<1000){tick(.01f);clear();}
   if(guard>=1000)throw new Exception("Approach timeout");
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);
   g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
  };
  try
  {
   Time.timeScale=0;
   foreach(int lane in new[]{0,1,2,3})foreach(float near in new[]{24f,3f,-6.5f})foreach(int profile in new[]{0,1,2})foreach(float dt in new[]{1f/60,.05f})
   {
    reset();approach(lane,near);int side=lane<2?-1:1;string street=g.NextStreet;
    set("timeRiding",profile==0?0f:100f);if(profile==2)g.TryBoost();
    // The outgoing tile is a fixed physical reference across the coordinate rebase.
    int row=(int)get(side<0?"leftRow":"rightRow");Transform anchor=g.roadRows[row].tiles[0];
    Vector3 previous=anchor.InverseTransformPoint(g.rider.position);float minRatio=1,maxRatio=0;
    bool speedOK=true,distanceOK=true,moving=true;int guard=0;g.MoveLane(side);
    do
    {
     float before=g.Distance;tick(dt);clear();
     float normal=Mathf.Min(17+g.DifficultyTime*.12f,30),expected=normal*(g.BoostActive?PhoVietGame.BoostMultiplier:1);
     speedOK&=Mathf.Abs(g.Speed-expected)<.001f;
     distanceOK&=Mathf.Abs((g.Distance-before)-g.Speed*dt)<.0002f;
     Vector3 point=anchor.InverseTransformPoint(g.rider.position);
     float ratio=Vector3.Distance(point,previous)/(g.Speed*dt);
     // This is a chord, not arc length: a very late turn can bend back inside
     // the intersection during one 50 ms boosted frame. It must still advance.
     minRatio=Mathf.Min(minRatio,ratio);maxRatio=Mathf.Max(maxRatio,ratio);moving&=ratio>.15f&&ratio<1.025f;
     previous=point;
    }while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<700);
    string key="lane "+lane+", near "+near+", profile "+profile+", dt "+dt;
    check(speedOK,key+": actual speed retains straight cruise/boost through entry, arc and final frame");
    check(distanceOK,key+": every frame advances the full speed times delta, without a shortened exit frame");
    check(moving,key+": measured travel against stationary road has no stalled/teleported frame ("+minRatio.ToString("F4")+".."+maxRatio.ToString("F4")+")");
    check(guard<700&&g.State==PhoVietGame.RideState.Riding&&g.CompletedTurns==1&&g.CurrentStreet==street&&g.CurrentLane==lane,key+": connected turn completes into the same lane and correct street");
    float speed=g.Speed;tick(dt);clear();check(g.Speed>=speed-.001f,key+": straight travel resumes without a post-turn speed drop");
   }
   foreach(int lane in new[]{0,1,2,3})
   {
    reset();approach(lane,24);int side=lane<2?-1:1;g.MoveLane(side);
    while(!g.IsTurning){tick(.02f);clear();}
    float length=(float)get("turnLength"),travel=(float)get("turnTravel");
    var first=(Vector3)call("TurnTangent",new object[]{0f});var last=(Vector3)call("TurnTangent",new object[]{1f});
    var early=(Vector3)call("TurnTangent",new object[]{.001f});var late=(Vector3)call("TurnTangent",new object[]{.999f});
    check(Vector3.Angle(first,Vector3.forward)<.001f&&Vector3.Angle(last,Vector3.right*side)<.001f,"Lane "+lane+": curve joins both straight roads at their exact headings");
    check(Vector3.Angle(first,early)<.01f&&Vector3.Angle(late,last)<.01f,"Lane "+lane+": steering eases in/out instead of abruptly starting/stopping angular motion");
    float delta=length-travel+.73f;
    typeof(PhoVietGame).GetProperty("Distance").SetValue(g,g.Distance+delta);
    call("ScrollRoutes",new object[]{delta});call("BeginOrContinueTurn",new object[]{delta});call("PositionCamera",new object[]{true});
    var points=new[]{new Vector3(0,0,0),new Vector3(0,1.8f,0),new Vector3(.4f,.7f,1)};
    var view=points.Select(p=>g.rideCamera.WorldToViewportPoint(g.rider.TransformPoint(p))).ToArray();
    int destination=(int)get(side<0?"leftRow":"rightRow");var anchor=g.roadRows[destination].tiles[0];
    Vector3 beforePosition=anchor.InverseTransformPoint(g.rider.position);
    call("EndRouteFrame",new object[]{.02f});
    check(Vector3.Distance(beforePosition,anchor.InverseTransformPoint(g.rider.position))<.0002f,"Lane "+lane+": carrying 0.73 m beyond the curve survives the world rebase exactly");
    float error=0;for(int i=0;i<points.Length;i++)error=Mathf.Max(error,Vector3.Distance(view[i],g.rideCamera.WorldToViewportPoint(g.rider.TransformPoint(points[i]))));
    check(error<.0001f,"Lane "+lane+": camera/rider projection is continuous across the destination-road rebase");
   }
   foreach(int lane in new[]{0,1,2,3})foreach(float dt in new[]{1f/60,.05f})foreach(float near in new[]{24f,3f})
   {
    reset();approach(lane,near);set("timeRiding",100f);g.TryBoost();
    call("PositionRideCamera",new object[]{true,dt});g.MoveLane(lane<2?-1:1);
    bool visible=true;int guard=0,after=0;float minX=1,maxX=0,minY=1,maxY=0;
    while(guard++<400&&(g.CompletedTurns==0||after++<24))
    {
     tick(dt);clear();call("PositionRideCamera",new object[]{false,dt});
     foreach(float height in new[]{0f,2f})
     {
      Vector3 p=g.rideCamera.WorldToViewportPoint(g.rider.position+Vector3.up*height);
      minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);
      visible&=p.z>0&&p.x>.01f&&p.x<.99f&&p.y>.01f&&p.y<.99f;
     }
    }
    check(visible&&guard<400,"Boosted camera lane "+lane+", dt "+dt+", near "+near+": rider stays visible through the curve and settling ("+minX+","+maxX+"; "+minY+","+maxY+")");
   }
   reset();approach(2,3);g.TryJump();g.TryBoost();g.MoveLane(1);for(int i=0;i<5;i++)tick(.05f);
   check(g.IsTurning&&g.RiderAirborne&&g.BoostActive&&Mathf.Abs(g.Speed-(Mathf.Min(17+g.DifficultyTime*.12f,30)+3)*PhoVietGame.BoostMultiplier)<.001f,"Airborne boosted turn retains both forward jump speed and boost");
   g.TogglePause();float paused=g.Distance,yaw=g.TurnYaw;call("Update",null);
   check(g.Distance==paused&&g.TurnYaw==yaw,"Pause still freezes a full-speed turn");g.TogglePause();
   reset();approach(3,3);g.MoveLane(1);tick(.05f);var hit=g.SpawnTraffic(2,40,0);hit.model.localPosition=g.rider.localPosition;hit.previousPosition=hit.model.localPosition;
   call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,0f});
   check(g.State==PhoVietGame.RideState.Falling,"Full-speed turn still causes a real crash when traffic is hit");
   reset();approach(0,3);call("StartChase",null);g.MoveLane(-1);int count=0;
   while(g.CompletedTurns==0&&count++<400){tick(.02f);clear();}
   check(g.CompletedTurns==1&&g.IsChased&&g.State==PhoVietGame.RideState.Riding&&g.PoliceDistance>0,"Police pursuit follows the faster turn without resetting or capturing through the rebase");
   times.Sort();return new{success=true,count=checks.Count,checks,measuredTickMs=new{samples=times.Count,median=times[times.Count/2],p95=times[(int)(times.Count*.95)],max=times.Last()},screen=new{Screen.width,Screen.height}};
  }
  finally
  {
   g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
  }
 }
}
