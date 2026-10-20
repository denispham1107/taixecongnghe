using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyOrdinaryRearWarnings
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  bool jobs=g.jobsEnabled,exit=g.turnExitTrafficEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises;float time=Time.timeScale;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Func<PhoVietGame.TrafficVehicle[]> reserved=()=>(PhoVietGame.TrafficVehicle[])typeof(PhoVietGame).GetField("rearExitReserved",F).GetValue(g);
  Action<float> tick=dt=>call("TickOrdinaryRearWarnings",new object[]{dt});
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.turnExitTrafficEnabled=true;g.recklessTrafficEnabled=false;g.trafficSurprises=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}};
  try
  {
   Time.timeScale=0;var lanes=new HashSet<int>();
   foreach(int kind in new[]{0,1,2})for(int run=0;run<5;run++)
   {
    reset();var candidate=g.vehicles.First(v=>!v.eventOnly&&(kind==0?v.halfWidth<.7f:kind==1?v.halfWidth>.7f&&v.halfLength<3:v.halfLength>3));
    foreach(var v in g.vehicles.Where(v=>!v.eventOnly&&v!=candidate)){v.active=true;v.model.gameObject.SetActive(true);v.model.localPosition=new Vector3(0,0,400);}
    check((bool)call("TryExitTraffic",new object[]{true}),"Reserve ordinary bike/car/truck: "+kind+","+run);
    int lane=reserved()[0]!=null?2:3;lanes.Add(lane);
    check(g.OrdinaryRearWarningVisible(lane)&&g.OrdinaryRearWarningRemaining(lane)==2&&g.ExitRearSpawns==0&&!candidate.active,"Warn correct lane without creating the vehicle");
    check(g.SpawnTraffic(lane,100)==null&&g.SpawnCrossTraffic(1,60,kind!=0,10)==null,"Ordinary waves and crossings cannot steal a reserved model");
    tick(1.99f);check(g.ExitRearSpawns==0&&!candidate.active&&g.OrdinaryRearWarningRemaining(lane)>0,"No ordinary rear spawn before two seconds");
    tick(.02f);check(g.ExitRearSpawns==1&&candidate.active&&candidate.enteringFromBehind&&candidate.lane==lane,"Reserved model enters the exact warned lane after two seconds");
    check(candidate.model.localPosition.z<g.rideCamera.transform.localPosition.z-candidate.halfLength&&Mathf.Abs(candidate.model.localPosition.x-PhoVietGame.LaneX[lane])<.001f,"Vehicle starts continuously behind camera on warned lane");
    check(!g.OrdinaryRearWarningVisible(lane)&&reserved().All(v=>v==null),"Completed entry clears its arrow and reservation");
   }
   check(lanes.Count==2,"Ordinary rear entries vary between both same-direction lanes");
   reset();check((bool)call("TryExitTraffic",new object[]{false})&&g.ExitOncomingSpawns==1,"Oncoming traffic still enters normally");
   check(!g.OrdinaryRearWarningVisible(2)&&!g.OrdinaryRearWarningVisible(3),"Oncoming entries do not display a misleading bottom arrow");
   reset();call("TryExitTraffic",new object[]{true});call("TryExitTraffic",new object[]{true});
   check(g.OrdinaryRearWarningVisible(2)&&g.OrdinaryRearWarningVisible(3)&&reserved()[0]!=reserved()[1],"Two pending lanes have distinct models and arrows");
   foreach(bool chased in new[]{false,true})foreach(int playerLane in new[]{0,1,2,3})
   {
    typeof(PhoVietGame).GetProperty("IsChased").SetValue(g,chased);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,playerLane);
    g.rider.localPosition=new Vector3(PhoVietGame.LaneX[playerLane],0,0);call("PositionCamera",new object[]{true});
    var area=(Rect)call("GameGuiArea",null);float pixels=Mathf.Clamp(area.height*.07f,30,46),bottom=area.yMax-pixels*.5f-9;
    var ray=g.rideCamera.ScreenPointToRay(new Vector3(Screen.width*.5f,Screen.height-bottom,0));var plane=new Plane(g.transform.up,g.transform.position+g.transform.up*.03f);
    if(!plane.Raycast(ray,out float distance))throw new Exception("Warning plane not visible");float z=g.transform.InverseTransformPoint(ray.GetPoint(distance)).z;
    var left=(Vector3)call("RearWarningScreenPosition",new object[]{2,z,pixels,area});var right=(Vector3)call("RearWarningScreenPosition",new object[]{3,z,pixels,area});
    check(right.x-left.x>=pixels*1.2f,"Two arrows remain distinct in player lane "+playerLane+", chase="+chased);
    check(left.x>=area.xMin+pixels*.59f&&right.x<=area.xMax-pixels*.59f,"Both arrows stay inside safe area in lane "+playerLane+", chase="+chased);
   }
   typeof(PhoVietGame).GetProperty("IsChased").SetValue(g,false);
   check(!(bool)call("TryExitTraffic",new object[]{true}),"A lane cannot be reserved twice during its warning");
   tick(1);g.TogglePause();call("Update",null);check(g.OrdinaryRearWarningRemaining(2)==1&&g.OrdinaryRearWarningRemaining(3)==1,"Pause does not consume either countdown");g.TogglePause();
   typeof(PhoVietGame).GetProperty("IsTurning").SetValue(g,true);tick(.2f);
   check(!g.OrdinaryRearWarningVisible(2)&&g.OrdinaryRearWarningRemaining(2)==2,"Turning hides and restarts the full ordinary warning");typeof(PhoVietGame).GetProperty("IsTurning").SetValue(g,false);
   tick(1.99f);check(g.ExitRearSpawns==0,"New road cannot bypass the full two-second warning");tick(.02f);
   check(g.ExitRearSpawns==2&&g.vehicles.Count(v=>v.active&&v.enteringFromBehind)==2,"Two warned lanes enter once each without doubling the wave");
   reset();call("TryExitTraffic",new object[]{true});int blockedLane=reserved()[0]!=null?2:3;var pending=reserved()[blockedLane-2];int index=0;
   foreach(var v in g.vehicles.Where(v=>!v.eventOnly&&v!=pending)){v.active=true;v.model.gameObject.SetActive(true);v.model.localPosition=new Vector3(PhoVietGame.LaneX[blockedLane],0,-20-index++*3);}
   tick(2.01f);check(g.ExitRearSpawns==0&&!pending.active&&g.OrdinaryRearWarningVisible(blockedLane),"Occupied spawn gap defers entry while retaining its arrow");
   foreach(var v in g.vehicles.Where(v=>v!=pending)){v.active=false;v.model.gameObject.SetActive(false);}tick(.01f);
   check(g.ExitRearSpawns==1&&pending.lane==blockedLane,"Deferred entry stays in its warned lane instead of silently changing lanes");
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);call("TryExitTraffic",new object[]{true});int serviceLane=reserved()[0]!=null?2:3;
   typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,PhoVietGame.WorkPhase.Delivery);call("BeginService",null);tick(3);
   check(g.ExitRearSpawns==0&&!g.OrdinaryRearWarningVisible(serviceLane)&&g.OrdinaryRearWarningRemaining(serviceLane)==2,"No rear entry or countdown while passengers alight");
   typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,PhoVietGame.WorkPhase.Delivery);tick(1.99f);
   check(g.ExitRearSpawns==0,"Departure restores a full two-second warning");tick(.02f);check(g.ExitRearSpawns==1,"Entry resumes after passenger stop and warning");
   reset();call("TryExitTraffic",new object[]{true});g.turnExitTrafficEnabled=false;tick(.1f);check(reserved().All(v=>v==null)&&!g.OrdinaryRearWarningVisible(2)&&!g.OrdinaryRearWarningVisible(3),"Disabling exit traffic releases all pending models");
   reset();call("TryExitTraffic",new object[]{true});g.ResetRide();check(reserved().All(v=>v==null)&&g.ExitRearSpawns==0&&g.OrdinaryRearWarningRemaining(2)<0&&g.OrdinaryRearWarningRemaining(3)<0,"Restart clears both lane countdowns and reserved models");
   return new{success=true,count=checks.Count,checks};
  }
  finally
  {
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();
   g.jobsEnabled=jobs;g.turnExitTrafficEnabled=exit;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.ResetRide();Time.timeScale=time;
  }
 }
}
