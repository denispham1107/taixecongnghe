using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyJobWayfinding
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool jobs=g.jobsEnabled,reckless=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exit=g.turnExitTrafficEnabled;
  var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clean=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;};
  Action begin=()=>{g.jobsEnabled=true;g.RestartRide();g.ShowJobOffers();g.AcceptJob(0);clean();};
  try
  {
   Time.timeScale=0;begin();
   foreach(int side in new[]{-1,1})
   {
    g.ActiveJob.pickup=new RideAddress{node=Vector2Int.zero,axis=0,offset=40,curb=side};
    call("PositionCamera",new object[]{true});
    call("UpdateJobMarker",null);
    check(g.JobMarkerVisible&&g.JobMarkerLabel=="ĐÓN KHÁCH","Visible pickup has labeled arrow at curb "+side);
    var marker=(Transform)typeof(PhoVietGame).GetField("jobMarker",F).GetValue(g);
    check(Mathf.Abs(marker.localPosition.x-side*7.6f)<.01f,"Arrow points to actual customer curb "+side);
    check(Vector3.Dot(marker.forward,g.rideCamera.transform.forward)>.999f,"Arrow faces camera "+side);
    var green=marker.Find("Mũi tên 3D/Mũi tên xanh lá").GetComponent<Renderer>();
    check(green.sharedMaterial.shader.isSupported&&green.sharedMaterial.color.g>.95f,"Bright green shader is supported "+side);
    check(marker.Find("Mũi tên 3D/Viền sáng")!=null&&marker.Find("Mũi tên 3D/Quầng sáng xanh")!=null,"Arrow has contrasting outline and glow "+side);
    var mesh=green.GetComponent<MeshFilter>().sharedMesh;
    check(mesh.bounds.size.z>.4f,"Waypoint has genuine solid depth rather than stacked flat arrows "+side);
    check(mesh.normals.Any(n=>n.z<-.98f)&&mesh.normals.Any(n=>n.z>.98f),"Arrow volume has front and back faces "+side);
    check(mesh.normals.Any(n=>Mathf.Abs(n.z)>.1f&&Mathf.Abs(n.z)<.95f),"Arrow edges contain sloped bevel faces "+side);
    check(mesh.triangles.Length/3<128&&marker.Find("Vòng sáng điểm hẹn")!=null,"Solid arrow is lightweight and has a ground locator halo "+side);
   }
   g.ActiveJob.dropoff=new RideAddress{node=Vector2Int.zero,axis=0,offset=40,curb=1};
   typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,PhoVietGame.WorkPhase.Delivery);call("UpdateJobMarker",null);
   check(g.JobMarkerVisible&&g.JobMarkerLabel=="TRẢ KHÁCH","Delivery marker has a distinct label");
   g.ActiveJob.dropoff.offset=-30;call("UpdateJobMarker",null);check(!g.JobMarkerVisible,"Point behind the camera is hidden");
   g.ActiveJob.dropoff.offset=500;call("UpdateJobMarker",null);check(!g.JobMarkerVisible,"Far point outside view is hidden");
   begin();g.ActiveJob.pickup=new RideAddress{node=new Vector2Int(0,1),axis=1,offset=22.4f,curb=1};
   g.rideCamera.transform.localPosition=new Vector3(0,15,0);g.rideCamera.transform.LookAt(new Vector3(22.4f,2.35f,70));call("UpdateJobMarker",null);
   check(g.JobMarkerVisible,"Customer visible on a cross street also has an arrow");
   foreach(int side in new[]{-1,1})foreach(bool late in new[]{false,true})
   {
    begin();g.ActiveJob.pickup=new RideAddress{node=side>0?new Vector2Int(0,1):new Vector2Int(-1,1),axis=1,offset=40,curb=1};
    int lane=side<0?0:3;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
    set("junctionAt",g.Distance+(late?0:11));call("ReplanJob",new object[]{false});
    var original=g.ActiveRoute;var node=g.NavigationNextNode;var pickup=g.ActiveJob.pickup;int heading=g.NavigationHeading;
    typeof(PhoVietGame).GetProperty("QueuedTurn").SetValue(g,side);call("StartJunctionTurn",new object[]{late});
    string name=(late?"late ":"queued ")+side;
    check(g.IsTurning&&!ReferenceEquals(g.NavigationRoute,original),"Exit route replaces stale map immediately for "+name);
    check(g.NavigationNextNode==node&&g.NavigationHeading==heading,"Map preview does not prematurely advance city coordinates for "+name);
    check(g.NavigationRoute.direct&&Mathf.Abs(g.NavigationRemainingDistance-35.35f)<.01f,"Exit distance is correct in the very first turning frame for "+name);
    check((g.NavigationRoute.points.Last()-pickup.Point).sqrMagnitude<.001f,"Preview preserves actual pickup address for "+name);
    Vector2 p=g.NavigationMapPosition;call("BeginOrContinueTurn",new object[]{2f});
    check((g.NavigationMapPosition-p).sqrMagnitude>.01f&&Mathf.Abs(g.NavigationMapDirection.x)>.01f,"Map position and heading move during the curve for "+name);
    call("PositionCamera",new object[]{true});
    g.rideCamera.transform.LookAt(new Vector3(side*40,2.35f,g.NextJunctionDistance-7.6f));call("UpdateJobMarker",null);
    check(g.JobMarkerVisible,"Visible point remains marked while turning for "+name);
    int guard=0;while(g.IsTurning&&guard++<300){clean();call("TickRide",new object[]{.04f});}
    check(guard<300&&g.NavigationHeading==(heading+side+4)%4&&g.CompletedTurns==1,"Committed turn updates city exactly once for "+name);
    check(ReferenceEquals(g.NavigationRoute,g.ActiveRoute)&&(g.ActiveRoute.points.Last()-pickup.Point).sqrMagnitude<.001f,"Final route hands off without an old-route delay for "+name);
   }
   begin();g.ActiveJob.pickup=new RideAddress{node=new Vector2Int(0,1),axis=1,offset=40,curb=1};
   set("junctionAt",g.Distance+11);call("ReplanJob",new object[]{false});
   int previousReroutes=g.Reroutes;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,0);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[0],0,0);
   typeof(PhoVietGame).GetProperty("QueuedTurn").SetValue(g,-1);call("StartJunctionTurn",new object[]{false});
   check(g.NavigationRemainingDistance>200,"Wrong turn recalculates longer detour immediately");
   int frames=0;while(g.IsTurning&&frames++<300){clean();call("TickRide",new object[]{.04f});}
   check(g.Reroutes==previousReroutes+1,"Wrong turn counts one reroute at committed handoff");
   g.ResetRide();call("UpdateJobMarker",null);check(!g.JobMarkerVisible,"Restart removes the previous job arrow");
   return new{success=true,count=checks.Count,checks};
  }
  finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exit;g.ResetRide();Time.timeScale=scale;}
 }
}
