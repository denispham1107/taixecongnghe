using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyRoadDrivers
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play required");
  var flags=BindingFlags.NonPublic|BindingFlags.Instance;var checks=new List<string>();float scale=Time.timeScale;
  bool surprises=g.trafficSurprises;int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,flags).Invoke(g,a);
  try
  {
   Time.timeScale=0;g.trafficSurprises=true;
   foreach(bool car in new[]{false,true})foreach(int direction in new[]{-1,1})foreach(int side in new[]{-1,1})
   {
    g.RestartRide();foreach(var t in g.vehicles){t.active=false;t.model.gameObject.SetActive(false);}
    int index=g.vehicles.FindIndex(v=>!v.eventOnly&&(v.halfWidth>.7f)==car&&v.halfLength<3);
    var v=g.SpawnTraffic(direction>0?3:0,0,index);v.speed=v.baseTrafficSpeed=direction*8;
    float radius=car?5.4f:4.2f,centre=g.blocks.Select(b=>b.localPosition.z+70).First(z=>z>10);
    v.model.localPosition=new Vector3(PhoVietGame.LaneX[v.lane],0,centre-direction*radius);
    v.roadDecisionAt=g.Distance+centre;v.turnChoice=side;Vector3 start=v.model.localPosition;
    check((bool)call("TryBeginRoadTurn",new object[]{v,.02f}),"Longitudinal bike/car starts turn: "+car+","+direction+","+side);
    float max=0;for(int i=0;i<300&&v.roadTurning;i++){var p=v.model.localPosition;call("TickRoadTurn",new object[]{v,.02f});max=Mathf.Max(max,(v.model.localPosition-p+Vector3.forward*g.Speed*.02f).magnitude);}
    check(v.hasTurned&&v.routeDetached&&!v.roadTurning,"Turn finishes on side road");
    check(v.routeVelocity.x*direction*side>0&&Mathf.Abs(v.routeVelocity.z)<.001f,"Exits in requested left/right heading");
    check(max<.25f,"Turn is continuous at normal simulation step");
   }
   foreach(int kind in new[]{0,1,2})
   {
    g.RestartRide();foreach(var t in g.vehicles){t.active=false;t.model.gameObject.SetActive(false);}
    int index=g.vehicles.FindIndex(v=>!v.eventOnly&&(kind==2?v.halfLength>3:kind==1?v.halfWidth>.7f&&v.halfLength<3:v.halfWidth<.7f));
    var v=g.SpawnTraffic(1,40,index);
    check(g.BeginSuddenLaneChange(v,2),"Oncoming class "+kind+" crosses yellow centre line");
    for(int i=0;i<60;i++)call("TickLongitudinalVehicle",new object[]{v,.02f});
    check(v.speed<0&&v.model.forward.z<0,"Intruding oncoming actor still faces and drives against player");
    check(Mathf.Abs(v.model.localPosition.x-1.55f)<.01f,"Intruding actor reaches player-side lane continuously");
    check(g.BeginSuddenLaneChange(v,1),"Intruding actor can return to original side");
   }
   g.ResetRide();check(g.RoadTrafficTurns==0,"Restart clears road-turn counter");
   return new{success=true,count=checks.Count,checks};
  }
  finally{PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);g.trafficSurprises=surprises;g.ResetRide();Time.timeScale=scale;}
 }
}
