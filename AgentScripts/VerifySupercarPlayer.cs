using System;using System.Linq;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifySupercarPlayer
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,surprises=g.trafficSurprises,danger=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  Action<int> reset=lane=>{g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);};
  var checks=new List<string>();var failures=new List<string>();var cases=new List<object>();Action<bool,string> check=(b,s)=>{if(b)checks.Add(s);else failures.Add(s);};
  try
  {
   Time.timeScale=0;reset(2);var car=g.rider.GetComponent<PlayerCarVisual>();car.Advance(.02f);
   check(g.IsPlayerCar&&g.rider.GetComponent<BikeVisualAnimator>()==null&&g.rider.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length==3,"Player uses only the received car, no old rider or motorcycle");
   check(Mathf.Abs(g.PlayerHalfWidth-.880288f)<.001f&&Mathf.Abs(g.PlayerHalfLength-1.87f)<.001f,"Player collision footprint matches real car dimensions");
   check(car.visual.GetComponent<LODGroup>().GetLODs().Length==3&&car.clips.Length==7&&car.wheels.Length==4,"Three LODs, four wheel pivots and seven Blender clips");
   check(g.policeBike.GetComponent<BikeVisualAnimator>()!=null&&g.vehicles.Where(v=>v.halfWidth<.7f).All(v=>v.model.GetComponent<BikeVisualAnimator>()!=null),"NPC motorcycles and police retained");
   check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   foreach(float dt in new[]{.016f,.033f,.05f})foreach(int lane in new[]{2,3})foreach(bool incoming in new[]{false,true})
   {
    reset(lane);int slot=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);var target=g.SpawnTraffic(lane==2?3:2,incoming?12:2,slot);target.speed=target.baseTrafficSpeed=incoming?-12:28;target.model.localRotation=Quaternion.Euler(0,incoming?180:0,0);target.motionVelocity=Vector3.zero;
    string label=$"ram dt={dt},lane={lane},incoming={incoming}";check(g.TryKick(),"Target selected: "+label);check(!target.knocked&&g.KickCount==0,"No remote force at input: "+label);
    float age=0,at=-1;for(int i=0;i<150&&g.State==PhoVietGame.RideState.Riding&&g.KickAssistActive;i++){tick(dt);age+=dt;if(target.knocked&&at<0)at=age;}
    cases.Add(new{label,at,state=g.State.ToString(),gap=g.LastKickFootGap,x=g.rider.localPosition.x,target=target.model.localPosition.ToString()});
    check(target.knocked&&at>0&&at<.55f&&g.KickCount==1&&g.LastKickFootGap==0,"One swept panel contact: "+label);
    check(g.State==PhoVietGame.RideState.Riding&&!g.KickAssistActive&&Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[lane])<.04f,"Safe return to original lane: "+label);
    // The new car intentionally finishes the last centimetres along its smooth
    // lane curve instead of snapping to the centre when the ram offset ends.
    for(int settle=0;settle<40&&g.State==PhoVietGame.RideState.Riding;settle++)tick(.02f);
    check(g.State==PhoVietGame.RideState.Riding&&Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[lane])<.001f,"Smooth lane-centre settling after ram: "+label);
   }
   reset(2);int index=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);var bike=g.SpawnTraffic(3,1,index);bike.speed=bike.baseTrafficSpeed=17;
   var blocker=g.SpawnTraffic(2,0,g.vehicles.FindIndex(v=>v.halfWidth>.7f&&!v.eventOnly));blocker.model.localPosition=new Vector3(3,0,.5f);blocker.speed=blocker.baseTrafficSpeed=17;
   check(!g.TryKick()&&!bike.knocked,"Vehicle blocking approach cancels ram");
   reset(2);check(g.TryJump(),"Jump input retained");float max=0;for(int i=0;i<160;i++){tick(.02f);car.Advance(.02f);max=Mathf.Max(max,g.RiderJumpHeight);}check(max>3.6f&&g.State==PhoVietGame.RideState.Riding&&!g.RiderAirborne&&g.RiderJumpHeight==0,"Ballistic car jump lands safely");
   reset(2);check(g.TryBoost(),"Boost input retained");tick(.02f);check(g.Speed>38&&PhoVietGame.BoostMultiplier==2.25f&&PhoVietGame.BoostDuration==4&&PhoVietGame.BoostRecovery==2,"Car keeps +125 percent / 4 seconds / 2 seconds recovery");
   var smoke=g.rider.GetComponent<BoostSmokeVisual>();check(smoke!=null&&car.exhaustMount.GetComponentsInChildren<ParticleSystem>(true).Length==2,"Boost smoke attached to animated car exhaust");
   reset(2);g.daylight.SetCycleTime(30);g.TryJump();for(int i=0;i<34;i++){tick(.02f);car.Advance(.02f);g.daylight.UpdateLamps();}
   var lamp=g.daylight.vehicles.Single(v=>v.root==g.rider);check(lamp.head.IsChildOf(car.visual)&&lamp.head.position.y>2.6f&&lamp.halos.All(h=>h.source.IsChildOf(car.visual)),"Headlights and tail lamps follow airborne car");
   reset(2);var hit=g.SpawnTraffic(2,2.3f,index);hit.speed=hit.baseTrafficSpeed=0;tick(.05f);check(g.State==PhoVietGame.RideState.Falling,"Ordinary frontal impact still crashes");
   for(int i=0;i<1500&&g.State==PhoVietGame.RideState.Falling;i++)call("TickRiderCrash",new object[]{.02f});check(g.State==PhoVietGame.RideState.GameOver,"Ordinary car crash settles then ends the run");
   reset(2);call("StartChase",null);g.policeBike.localPosition=g.rider.localPosition+Vector3.back*18;g.Crash();
   for(int i=0;i<1500&&g.State==PhoVietGame.RideState.Falling;i++)call("TickRiderCrash",new object[]{.02f});
   check(g.State==PhoVietGame.RideState.Arrested,"Police physically reaches the crashed car before arrest");call("TickArrest",new object[]{1.2f});check(g.State==PhoVietGame.RideState.Jailed,"Car pursuit crash still completes jail transition");
   var result=new{success=failures.Count==0,count=checks.Count,failures,checks,cases};File.WriteAllText("Captures/supercar-player-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
