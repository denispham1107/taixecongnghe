using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyCrashOutcome
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;
  bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  var checks=new List<string>();Action<bool,string> check=(v,s)=>{if(!v)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;clear();};
  Action<float> tick=dt=>call("TickRiderCrash",new object[]{dt});
  Action chase=()=>{call("StartChase",null);g.policeBike.localPosition=new Vector3(g.rider.localPosition.x,0,-1000);};
  Func<int> settle=()=>{int i=0;while(!g.RiderFallComplete&&i++<1200)tick(.02f);return i;};
  Action<int,float,bool> hit=(side,speed,car)=>
  {
   int index=g.vehicles.FindIndex(v=>!v.eventOnly&&(v.halfWidth>.7f)==car);var v=g.SpawnTraffic(2,0,index);var p=g.rider.localPosition;
   Vector3 velocity=side==0?Vector3.forward*speed:side==2?Vector3.back*speed:Vector3.right*side*speed;
   v.speed=v.baseTrafficSpeed=velocity.z;v.motionVelocity=velocity;v.model.localRotation=Quaternion.Euler(0,Mathf.Atan2(velocity.x,velocity.z)*Mathf.Rad2Deg,0);
   v.previousPosition=p-velocity.normalized*12;v.model.localPosition=p+velocity.normalized*12;
   typeof(PhoVietGame).GetProperty("Speed").SetValue(g,0f);set("oldRiderZ",p.z);call("ResolveRiderContacts",new object[]{p.x,.05f});
  };
  try
  {
   Time.timeScale=0;reset();check(typeof(PhoVietGame).GetProperty("LivesRemaining")==null,"No life counter remains in runtime");
   foreach(bool car in new[]{false,true})foreach(int direction in new[]{-1,1,0,2})
   {
    reset();hit(direction,12,car);check(g.State==PhoVietGame.RideState.Falling&&!g.RiderFallComplete&&!g.CrashDuringPursuit,"First impact starts visible throw: "+car+","+direction);
    if(direction== -1||direction==1)check(Mathf.Sign(g.RiderFallVelocity.x)==direction,"Side impact launches away from contact: "+car+","+direction);
    clear();check(settle()<1200&&g.State==PhoVietGame.RideState.GameOver,"First collision ends only after physical fall: "+car+","+direction);
    var hero=g.rider.GetComponent<BikeVisualAnimator>();hero.Advance(1);check(hero.CurrentClip.StartsWith("Fall"),"Terminal rider stays in fallen skeleton pose: "+car+","+direction);
   }
   float slow=0;foreach(float speed in new[]{7f,21f}){reset();hit(0,speed,true);clear();settle();if(speed==7)slow=g.RiderFallTravel;else check(g.RiderFallTravel>slow*1.8f,"Impact speed still determines physical throw distance");}
   reset();g.Crash();g.TogglePause();var position=g.rider.localPosition;tick(3);check(g.State==PhoVietGame.RideState.Paused&&g.rider.localPosition==position&&!g.RiderFallComplete,"Pause freezes throw and completion");g.TogglePause();check(g.State==PhoVietGame.RideState.Falling,"Resume returns to fall, never to riding");clear();settle();
   reset();chase();g.Crash();clear();check(settle()<1200&&g.State==PhoVietGame.RideState.Falling&&g.CrashDuringPursuit,"Pursuit impact settles but does not show game over");
   position=g.rider.localPosition;for(int i=0;i<300;i++)tick(.02f);check(g.State==PhoVietGame.RideState.Falling&&Vector3.Distance(position,g.rider.localPosition)<.01f,"Rider waits on ground beyond old recovery time");
   int lane=g.CurrentLane;g.MoveLane(-1);check(g.CurrentLane==lane&&!g.TryJump()&&!g.TryKick()&&!g.TryBoost(),"Waiting rider cannot steer, jump, kick or boost");
   typeof(PhoVietGame).GetProperty("PoliceFallRemaining").SetValue(g,1.2f);g.policeBike.localPosition=position+Vector3.back*12;
   for(int i=0;i<59;i++)tick(.02f);check(g.PoliceFallen&&g.State==PhoVietGame.RideState.Falling,"Rider waits while police uses original 1.2 second get-up timer");tick(.02f);check(!g.PoliceFallen,"Police still gets up after exactly 1.2 seconds");
   int guard=0;while(g.State==PhoVietGame.RideState.Falling&&guard++<600)tick(.02f);check(g.State==PhoVietGame.RideState.Arrested,"Recovered officer physically approaches and arrests fallen rider");
   call("TickArrest",new object[]{1.2f});check(g.State==PhoVietGame.RideState.Jailed&&g.jailRoom.gameObject.activeSelf&&!g.rider.gameObject.activeSelf,"Capture switches to prison scene");
   reset();chase();g.policeBike.localPosition=g.rider.localPosition+new Vector3(-.5f,0,-.5f);g.Crash();g.Arrest();check(g.State==PhoVietGame.RideState.Falling&&!g.RiderFallComplete,"Nearby police cannot cut off unfinished fall");
   guard=0;while(g.State==PhoVietGame.RideState.Falling&&guard++<600)tick(.02f);check(g.RiderFallComplete&&g.State==PhoVietGame.RideState.Arrested,"Nearby officer captures after landing animation completes");
   reset();chase();g.Crash();clear();settle();position=g.rider.localPosition;
   int sedan=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3);var block=g.SpawnTraffic(2,position.z-5,sedan);block.model.localPosition=new Vector3(position.x,0,position.z-5);block.previousPosition=block.model.localPosition;block.speed=block.baseTrafficSpeed=0;block.laneCooldown=block.brakeCooldown=1000;
   g.policeBike.localPosition=position+Vector3.back*13;bool detoured=false;guard=0;var previous=g.policeBike.localPosition;
   while(g.State==PhoVietGame.RideState.Falling&&guard++<1500)
   {
    tick(.02f);var now=g.policeBike.localPosition;detoured|=Mathf.Abs(now.x-position.x)>1.8f;
    if(Vector3.Distance(previous,now)>.8f)throw new Exception("Officer teleported during obstacle avoidance");previous=now;
   }
   check(detoured&&g.PoliceFalls==0&&g.State==PhoVietGame.RideState.Arrested,"Officer detours around a stationary car and captures without tunneling");
   foreach(int direction in new[]{-1,1})
   {
    reset();int turnLane=direction<0?0:3;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,turnLane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[turnLane],0,0);
    guard=0;while(g.NextJunctionDistance>22&&guard++<1000)call("TickRide",new object[]{.02f});g.MoveLane(direction);while(!g.IsTurning&&guard++<1600)call("TickRide",new object[]{.02f});check(g.IsTurning,"Begin real turn "+direction);
    chase();g.Crash();clear();check(settle()<1200&&g.State==PhoVietGame.RideState.Falling,"Turn crash waits on its actual route position "+direction);g.policeBike.localPosition=g.rider.localPosition+new Vector3(-.5f,0,-1);tick(.02f);check(g.State==PhoVietGame.RideState.Arrested,"Officer captures on turned street "+direction);
   }
   reset();g.jobsEnabled=true;g.ShowJobOffers();g.AcceptJob(0);int wallet=g.Coins;var job=g.ActiveJob;g.Crash();clear();settle();check(g.State==PhoVietGame.RideState.GameOver&&g.Coins==wallet&&g.ActiveJob==job,"Unfinished trip pays no Coin when ordinary crash ends run");
   reset();chase();g.Arrest();check(g.State==PhoVietGame.RideState.Arrested&&!g.CrashDuringPursuit,"Police capture during riding remains immediate");call("TickArrest",new object[]{1.2f});g.RestartRide();check(g.State==PhoVietGame.RideState.Riding&&!g.CrashDuringPursuit&&!g.RiderFallComplete&&!g.IsChased&&!g.jailRoom.gameObject.activeSelf,"Restart clears terminal and capture state");
   return new{success=true,count=checks.Count,checks};
  }
  finally{g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=time;}
 }
}
