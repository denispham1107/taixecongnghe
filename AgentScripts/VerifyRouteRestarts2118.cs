using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyRouteRestarts2118
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks;static int pointer;
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static void Check(bool ok,string s){if(!ok)throw new Exception(s);checks.Add(s);}
 static void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);}
 static float Physical()=>g.blocks.Select(b=>b.localPosition.z+VietnamStreet.IntersectionCenter).Where(z=>z>=-1).Min();
 static void Input(int side,bool browser)
 {
  if(!browser){g.MoveLane(side);return;}
  int id=++pointer;g.ApplyBrowserGesture(Newtonsoft.Json.JsonConvert.SerializeObject(new{phase=0,id,action=0,x=.5f,y=.5f,at=(float)id}));g.ApplyBrowserGesture(Newtonsoft.Json.JsonConvert.SerializeObject(new{phase=1,id,action=side,x=.5f+side*.1f,y=.5f,at=id+.05f}));
 }
 static void DriveTo(float near,float dt)
 {int n=0;while(Physical()>near&&g.State==PhoVietGame.RideState.Riding&&n++<2000){Call("TickRide",dt);Clear();}Check(n<2000&&g.State==PhoVietGame.RideState.Riding,"Reach visible intersection with real scenery contacts");}
 static void Alignment(string at)
 {
  Check(Mathf.Abs(Physical()-g.NextJunctionDistance)<.003f,"Visual and logical junction agree "+at);
  int left=(int)typeof(PhoVietGame).GetField("leftRow",F).GetValue(g),right=(int)typeof(PhoVietGame).GetField("rightRow",F).GetValue(g);
  Check(Mathf.Abs(g.roadRows[left].root.localPosition.z-g.NextJunctionDistance)<.003f&&Mathf.Abs(g.roadRows[right].root.localPosition.z-g.NextJunctionDistance)<.003f,"Both cross streets align with real road opening "+at);
 }
 static void OpenRoad(string label)
 {
  float end=Mathf.Min(30,g.NextJunctionDistance-10);if(end<6)return;
  foreach(int lane in new[]{0,1,2,3})
  {
   if(g.riversEnabled&&(g.OverRiver(new Vector3(PhoVietGame.LaneX[lane],0,end))||g.OverRiver(new Vector3(PhoVietGame.LaneX[lane],0,5))))continue;
   object[] a={new Vector3(PhoVietGame.LaneX[lane],0,5),new Vector3(PhoVietGame.LaneX[lane],0,end),Quaternion.identity,Vector2.zero,0f,null};bool hit=(bool)typeof(PhoVietGame).GetMethod("SceneryContactAtRotation",F).Invoke(g,a);
   Check(!hit,"No wall/house/scenery crosses lane "+lane+" "+label+" obstacle="+a[5]);
  }
 }
 public static object Main()=>Run(false);
 public static object Mobile()=>Run(true);
 static object Run(bool browser)
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();var cases=new List<object>();float time=Time.timeScale;
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.ApplyBrowserInsets("{\"left\":0,\"right\":0,\"top\":0,\"bottom\":0}");
   foreach(int lane in new[]{0,1,2,3})foreach(float dt in new[]{1f/60,1f/30,.05f})foreach(bool boost in new[]{false,true})
   {
    int side=lane<2?-1:1;string label=lane+"/"+dt+"/"+boost+"/"+browser;g.riversEnabled=false;g.RestartRide();Clear();
    var p=g.rider.localPosition;var hit=g.SpawnTraffic(2,0,g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3));hit.speed=hit.baseTrafficSpeed=45;hit.motionVelocity=Vector3.forward*45;hit.previousPosition=p-Vector3.forward*12;hit.model.localPosition=p+Vector3.forward*12;hit.model.localRotation=Quaternion.identity;Set("Speed",0f);Set("oldRiderZ",p.z);Call("ResolveRiderContacts",p.x,.05f);Clear();int n=0;
    while(g.State==PhoVietGame.RideState.Falling&&n++<2000)Call("TickRiderCrash",dt);
    Check(g.State==PhoVietGame.RideState.GameOver&&g.Distance==0&&g.RiderFallTravel>1,"Real impact travels without scoring before retry "+label);
    float old=(float)typeof(PhoVietGame).GetField("crashTravelDistance",F).GetValue(g);Check(old>1,"Previous run carries real unscored travel "+label);
    if(browser){g.ResetRide();Check(g.State==PhoVietGame.RideState.Ready,"Return to welcome after crash");g.StartRide();}else g.RestartRide();Clear();
    Check(g.Distance==0&&(float)typeof(PhoVietGame).GetProperty("TravelDistance",F).GetValue(g)==0,"New score and physical coordinate reset together "+label);Alignment("retry "+label);OpenRoad("retry "+label);
    int laneInputs=0;while(g.CurrentLane!=lane&&laneInputs++<4)Input(g.CurrentLane<lane?1:-1,browser);Check(g.CurrentLane==lane,"Input reaches requested lane "+label);if(boost)g.TryBoost();DriveTo(20,dt);Alignment("input "+label);Input(side,browser);
    Check(g.QueuedTurn==side||g.IsTurning,"Keyboard/browser swipe accepts turn at visible opening "+label);n=0;
    while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&n++<1000){Call("TickRide",dt);Clear();}
    Check(g.CompletedTurns==1&&g.State==PhoVietGame.RideState.Riding&&g.CurrentLane==lane,"Turn finishes in original numbered lane without hitting houses "+label);Alignment("after turn "+label);OpenRoad("after turn "+label);
    // Continue across an intersection without turning, then use the next one. This exercises road-row reuse.
    float start=g.Distance;while(g.Distance-start<95&&g.State==PhoVietGame.RideState.Riding&&n++<2500){Call("TickRide",dt);Clear();}
    Check(g.State==PhoVietGame.RideState.Riding,"Straight passage after turn stays clear "+label);Alignment("after straight "+label);OpenRoad("after straight "+label);
    DriveTo(20,dt);Input(side,browser);n=0;while(g.CompletedTurns<2&&g.State==PhoVietGame.RideState.Riding&&n++<1000){Call("TickRide",dt);Clear();}
    Check(g.CompletedTurns==2&&g.State==PhoVietGame.RideState.Riding,"Second turn after a straight junction remains driveable "+label);Alignment("second turn "+label);OpenRoad("second turn "+label);
    cases.Add(new{lane,dt,boost,browser,oldUnscored=old,turns=g.CompletedTurns,distance=g.Distance});
   }
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");var result=new{success=true,count=checks.Count,checks,cases};File.WriteAllText("Captures/route-restart-2118-"+(browser?"mobile":"desktop")+"-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.riversEnabled=flags[2];g.thunderstormEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();Time.timeScale=time;}
 }
}
