using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyInnerTurns
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action<int,float> approach=(lane,near)=>{int guard=0;while(g.NextJunctionDistance>near&&guard++<1200)tick(.01f);if(guard>=1200)throw new Exception("Approach timeout");typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);};
  try
  {
   Time.timeScale=0;
   foreach(var lane in new[]{0,1,2,3})check(PhoVietGame.CanTurnFromLane(lane<2?-1:1,lane)&&!PhoVietGame.CanTurnFromLane(lane<2?1:-1,lane),"Lane "+lane+" accepts only its marked turn direction");
   reset();var mesh=g.blocks[0].Find("Vạch mũi tên giao lộ • 4 hướng").GetComponent<MeshFilter>().sharedMesh;
   check(mesh.vertexCount==264&&mesh.triangles.Length==264,"Saved paint has sixteen arrows including both inner branches on all four arms");
   for(int arm=0;arm<4;arm++)foreach(int lane in new[]{1,2})
   {
    int start=arm*66+(lane==1?15:33);Quaternion inv=Quaternion.Inverse(Quaternion.Euler(0,arm*90,0));
    var p=mesh.vertices.Skip(start).Take(18).Select(v=>inv*(v-new Vector3(0,.035f,70))-new Vector3(PhoVietGame.LaneX[lane],0,0)).ToArray();
    check(lane==1?p.Min(v=>v.x)<-1.1f:p.Max(v=>v.x)>1.1f,"Arm "+arm+" lane "+lane+" has the requested turn head");
    check(p.Max(v=>Mathf.Abs(v.x))<1.5f,"Arm "+arm+" lane "+lane+" paint stays inside its lane");
   }
   foreach(int lane in new[]{1,2})foreach(float near in new[]{24f,6f,0f,-6.6f})
   {
    reset();approach(lane,near);int side=lane==1?-1:1;float before=g.Distance,centre=g.NextJunctionDistance;string street=g.NextStreet;g.MoveLane(side);
    check(near>12?g.QueuedTurn==side&&!g.IsTurning:g.IsTurning,"Inner lane "+lane+" near "+near+" queues/turns with one matching input");
    int guard=0;bool started=g.IsTurning,onRoad=true,finite=true;
    while((!started||g.IsTurning)&&guard++<1400)
    {
     tick(.01f);started|=g.IsTurning;if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Unexpected crash");
     if(g.IsTurning){var p=g.rider.localPosition+Vector3.forward*(g.Distance-before);onRoad&=Mathf.Abs(p.x)<6.9f||Mathf.Abs(p.z-centre)<6.9f;finite&=!float.IsNaN(g.TurnYaw)&&!float.IsNaN(p.x)&&!float.IsNaN(p.z);}
    }
    check(guard<1400&&onRoad&&finite,"Inner lane "+lane+" near "+near+" follows connected road surfaces with finite motion");
    check(g.CompletedTurns==1&&g.CurrentStreet==street&&g.CurrentLane==lane,"Inner lane "+lane+" near "+near+" retains lane and changes street");
    check(Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[lane])<.001f&&g.blocks.All(b=>Mathf.Abs(b.localPosition.x)<.001f),"Inner lane "+lane+" near "+near+" rebase aligns new road");
   }
   foreach(int lane in new[]{1,2})
   {
    reset();approach(lane,24);g.MoveLane(lane==1?1:-1);check(!g.IsTurning&&g.QueuedTurn==0&&g.CurrentLane==(lane==1?2:1),"Wrong-direction input remains a lane change from inner lane "+lane);
    reset();approach(lane,24);float d=g.Distance;for(int i=0;i<600&&g.NextJunctionDistance>-19;i++)tick(.01f);check(g.CompletedTurns==0&&g.CurrentLane==lane&&g.Distance>d,"No turn input keeps inner lane "+lane+" going straight");
   }
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
