using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyDistanceTanks2199
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;
  bool dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,rivers=g.riversEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var cases=new List<object>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<string,object> prop=(n,v)=>typeof(PhoVietGame).GetProperty(n).SetValue(g,v);
  Func<string,object> get=n=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> schedule=dt=>call("TickDistanceTankSchedule",new object[]{dt});
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);call("ClearShells",Array.Empty<object>());};
  Action reset=()=>{g.dinosaursEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.tankBarrageEnabled=true;g.RestartRide();clear();set("tankSpawnTimer",1000f);};
  Func<int> pending=()=> (int)get("pendingDistanceTanks");
  try
  {
   Time.timeScale=0;reset();check(PhoVietGame.TankDistanceInterval==400&&g.DistanceTanksScheduled==0&&(float)get("nextDistanceTank")==400,"Fresh run begins at the 400 m milestone");
   float[] distances={399.99f,400,400.01f,799.99f,800,800.01f,1200.01f};int[] expected={0,0,1,1,1,2,3};
   for(int i=0;i<distances.Length;i++)
   {
    prop("Distance",distances[i]);schedule(.016f);check(g.DistanceTanksScheduled==expected[i],"Exactly one extra tank per crossed 400 m: "+distances[i]);
    foreach(var v in g.vehicles.Where(v=>v.active&&v.armored))check(v.baseTrafficSpeed<0&&v.lane<2&&v.model.localPosition.z>=122,"Distance tank is ahead and oncoming");
    clear();for(int j=0;j<10;j++)schedule(.016f);check(g.DistanceTanksScheduled==expected[i]&&pending()==0,"Milestone cannot repeat at the same distance: "+distances[i]);
   }
   check((float)get("tankSpawnTimer")==1000,"Distance waves leave the periodic countdown untouched");
   reset();prop("Distance",1200.1f);schedule(.01f);check(g.DistanceTanksScheduled==1&&pending()==2,"Large distance step queues all three milestones and spawns one per free frame");
   clear();schedule(.01f);clear();schedule(.01f);check(g.DistanceTanksScheduled==3&&pending()==0&&(float)get("nextDistanceTank")==1600,"No milestone is lost on a large distance step");
   reset();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}prop("Distance",401f);
   for(int j=0;j<20;j++)schedule(.016f);check(g.TanksScheduled==0&&pending()==1&&(float)get("nextDistanceTank")==800,"Full pool retains exactly one request without duplicating it");
   var slot=g.vehicles.First(v=>!v.eventOnly&&v.model.GetComponent<MeshyCarVariants>()?.ArmoredIndex>=0);slot.active=false;schedule(.016f);check(g.DistanceTanksScheduled==1&&pending()==0,"Freed pool slot releases the distance tank");
   reset();var blockers=g.vehicles.Where(v=>!v.eventOnly).Take(2).ToArray();for(int i=0;i<2;i++){blockers[i].active=true;blockers[i].model.localPosition=new Vector3(PhoVietGame.LaneX[i],0,126);}
   prop("Distance",401f);schedule(.016f);check(g.TanksScheduled==0&&pending()==1,"Occupied oncoming lanes defer the additional tank");clear();schedule(.016f);check(g.DistanceTanksScheduled==1&&pending()==0,"Cleared lanes release exactly one tank");
   foreach(var state in new[]{PhoVietGame.RideState.Ready,PhoVietGame.RideState.Paused,PhoVietGame.RideState.Falling,PhoVietGame.RideState.GameOver,PhoVietGame.RideState.Arrested,PhoVietGame.RideState.Jailed})
   {reset();prop("State",state);prop("Distance",401f);schedule(10);check(g.DistanceTanksScheduled==0&&pending()==0&&(float)get("nextDistanceTank")==400,"Non-riding state does not queue or spawn: "+state);}
   reset();prop("Distance",401f);schedule(0);schedule(-1);check(g.TanksScheduled==0&&pending()==0,"Zero/negative time never advances milestones");g.tankBarrageEnabled=false;schedule(1);check(g.TanksScheduled==0&&pending()==0,"Disabled tanks cannot create distance waves");
   reset();prop("IsTurning",true);prop("Distance",401f);schedule(.016f);check(g.DistanceTanksScheduled==0&&pending()==1,"Milestone crossed in a turn is retained until the new street is active");prop("IsTurning",false);schedule(.016f);check(g.DistanceTanksScheduled==1&&pending()==0,"First straight frame releases the retained milestone");
   reset();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}prop("Distance",401f);schedule(.016f);g.ResetRide();check(g.DistanceTanksScheduled==0&&pending()==0&&(float)get("nextDistanceTank")==400,"Retry clears queued milestones and distance counters");g.StartRide();schedule(.016f);check(g.DistanceTanksScheduled==0,"Retry cannot inherit the previous run's extra tank");
   reset();prop("Distance",400.1f);set("tankSpawnTimer",.001f);call("TickTankBarrage",new object[]{.01f});check(g.TanksScheduled==2&&g.DistanceTanksScheduled==1&&Mathf.Abs(g.TankSpawnCountdown-9)<.001f,"Periodic deadline and 400 m milestone each add a tank on the same frame");
   reset();g.dinosaursEnabled=true;prop("Distance",400.1f);set("junctionAt",g.Distance+65);call("QueueTurnHazards",Array.Empty<object>());schedule(.01f);check(g.DinosaursSpawned==1&&g.TanksScheduled==2&&g.DistanceTanksScheduled==1,"Turn pair and distance milestone remain independent on the same frame");
   // Actual road travel through both milestones, with and without W, at desktop/mobile-sized simulation steps.
   foreach(bool boost in new[]{false,true})foreach(float dt in new[]{1f/60,.05f})
   {
    reset();set("tankSpawnTimer",9f);float elapsed=0;int guard=0,last=0;var crossed=new List<float>();
    while(g.Distance<=805&&g.State==PhoVietGame.RideState.Riding&&guard++<5000)
    {
     if(boost)g.TryBoost();call("TickRide",new object[]{dt});elapsed+=dt;
     if(g.DistanceTanksScheduled!=last){crossed.Add(g.Distance);last=g.DistanceTanksScheduled;check(g.vehicles.Any(v=>v.active&&v.armored&&v.baseTrafficSpeed<0),"Real travel milestone produces an oncoming tank");}
     clear();
    }
    string label="boost="+boost+",dt="+dt;check(g.State==PhoVietGame.RideState.Riding&&g.Distance>805,"Real drive completes both distance milestones: "+label);
    check(g.DistanceTanksScheduled==2&&crossed.Count==2&&crossed[0]>400&&crossed[0]<405&&crossed[1]>800&&crossed[1]<805,"400/800 m crossings are immediate and independent of speed: "+label);
    check(g.TanksScheduled==2+Mathf.FloorToInt(elapsed/9),"Nine-second periodic tanks are additional to the two distance tanks: "+label);
    cases.Add(new{boost,dt,elapsed,distance=g.Distance,crossed,totalTanks=g.TanksScheduled,distanceTanks=g.DistanceTanksScheduled});
   }
   check(!PhoVietGame.RideJobsReleased&&!g.jobsEnabled&&PlayerPrefs.GetInt("PhoViet.Coins.v1",0)==coins&&g.BestDistance==best,"Jobs, coins and high score unchanged");
   var result=new{success=true,count=checks.Count,checks,cases};File.WriteAllText("Captures/tank2199-distance-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.riversEnabled=rivers;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.ResetRide();PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);PlayerPrefs.Save();Time.timeScale=time;}
 }
}
