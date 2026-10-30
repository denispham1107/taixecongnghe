using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyTurnHazards
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;
  bool dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,rivers=g.riversEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=g.BestDistance;
  var checks=new List<string>();var evidence=new List<object>();
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> tick=dt=>call("TickRide",new object[]{dt});Action<float> schedule=dt=>call("TickTurnHazards",new object[]{dt});
  Func<int> pending=()=>((ICollection)typeof(PhoVietGame).GetField("turnHazardWaves",F).GetValue(g)).Count;
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);};
  Action reset=()=>{g.dinosaursEnabled=g.tankBarrageEnabled=true;g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();clear();set("dinosaurTimer",1000f);set("tankSpawnTimer",1000f);set("junctionAt",g.Distance+65);};
  Action queue=()=>call("QueueTurnHazards",Array.Empty<object>());
  Action separate=()=>{foreach(var d in g.dinosaurs)if(d.active)d.root.localPosition=new Vector3(0,0,-30);};
  try
  {
   Time.timeScale=0;reset();queue();
   check(g.DinosaursSpawned==1&&g.TanksScheduled==1&&pending()==0,"Dinosaur and tank both appear in the QueueTurnHazards call, with no timer or extra tick");
   var d=g.dinosaurs.First(x=>x.active);check(Mathf.Abs(d.root.localPosition.z-65)<.01f&&Mathf.Abs(Mathf.DeltaAngle(d.root.localEulerAngles.y,180))<.01f,"Dinosaur stands at the upcoming junction facing the player");
   float dc=g.DinosaurCountdown,tc=g.TankSpawnCountdown;
   g.TogglePause();schedule(10);check(g.TanksScheduled==1&&pending()==0,"Pause cannot repeat a completed wave");g.TogglePause();
   check(PhoVietGame.TurnTankDelay==0,"No post-turn tank delay remains");
   var tank=g.vehicles.First(x=>x.active&&x.armored);check(Mathf.Abs(tank.model.localPosition.z-65)<.01f&&tank.baseTrafficSpeed<0,"Extra tank uses the upcoming junction and oncoming direction");
   check(g.DinosaurCountdown==dc&&g.TankSpawnCountdown==tc,"Additional wave does not reset or consume periodic timers");
   for(int i=0;i<100;i++)schedule(.05f);check(g.DinosaursSpawned==1&&g.TanksScheduled==1,"Finished wave is not repeated on subsequent frames");
   reset();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}queue();separate();clear();set("junctionAt",g.Distance-20);schedule(.01f);
   check(g.TanksScheduled==1&&Mathf.Abs(g.vehicles.First(x=>x.active&&x.armored).model.localPosition.z-60)<.01f,"If the first junction is behind, tank chooses the next physical junction ahead");
   reset();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}queue();check(pending()==1,"Retry fixture contains a genuinely blocked wave");g.ResetRide();g.StartRide();schedule(5);check(pending()==0&&g.DinosaursSpawned==0&&g.TanksScheduled==0,"Retry cancels all pending post-turn hazards");
   reset();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}queue();clear();typeof(PhoVietGame).GetProperty("State").SetValue(g,PhoVietGame.RideState.Falling);schedule(3);check(g.TanksScheduled==0&&pending()==1,"Falling does not spawn a pending tank");
   reset();for(int i=0;i<4;i++)g.SpawnDinosaur();queue();schedule(5);
   check(g.DinosaursSpawned==4&&g.TanksScheduled==0&&pending()==1,"Full dinosaur pool retains request and never spawns tank first");
   d=g.dinosaurs.First(x=>x.active);d.active=false;d.root.gameObject.SetActive(false);schedule(.01f);
   check(g.DinosaursSpawned==5&&g.TanksScheduled==1&&pending()==0,"Freed dinosaur slot releases both hazards together, without a fresh delay");
   reset();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}queue();separate();schedule(.01f);
   check(g.TanksScheduled==0&&pending()==1,"Full traffic pool keeps tank pending without replacing active vehicles");
   var slot=g.vehicles.First(x=>!x.eventOnly&&x.model.GetComponent<MeshyCarVariants>()!=null&&x.model.GetComponent<MeshyCarVariants>().ArmoredIndex>=0);slot.active=false;schedule(.01f);
   check(g.TanksScheduled==1&&pending()==0,"Freed traffic slot releases exactly one queued tank");
   reset();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}queue();g.TogglePause();clear();schedule(10);check(g.TanksScheduled==0&&pending()==1,"Pause preserves a blocked wave even after its lane becomes free");g.TogglePause();schedule(.016f);check(g.TanksScheduled==1&&pending()==0,"Resume immediately releases the cleared wave without a two-second wait");
   reset();g.rider.localPosition=new Vector3(0,0,65);
   // Occupy both possible oncoming lanes with physical blockers.
   var blockers=g.vehicles.Where(x=>x.halfWidth<.7f&&!x.eventOnly).Take(2).ToArray();
   for(int i=0;i<2;i++){blockers[i].active=true;blockers[i].model.localPosition=new Vector3(PhoVietGame.LaneX[i],0,65);}
   var variants=g.vehicles.Select(v=>v.model.GetComponent<MeshyCarVariants>()).Where(v=>v!=null).ToArray();var prepared=typeof(MeshyCarVariants).GetField("prepared",F);var before=variants.Select(v=>v.SelectedVariant+"/"+prepared.GetValue(v)).ToArray();
   queue();schedule(.01f);check(g.TanksScheduled==0,"Occupied junction cannot spawn a tank through another vehicle");
   check(before.SequenceEqual(variants.Select(v=>v.SelectedVariant+"/"+prepared.GetValue(v))),"Rejected position does not prepare or convert any ordinary vehicle into a tank");
   clear();g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],0,0);schedule(.01f);check(g.TanksScheduled==1,"Blocked junction is retried once space is clear");
   reset();queue();clear();separate();queue();
   check(g.DinosaursSpawned==2&&g.TanksScheduled==2&&pending()==0,"Two turns each release their own immediate pair");
   schedule(1);check(g.TanksScheduled==2&&pending()==0,"Completed pairs are never repeated");
   // Use SpawnTraffic itself to find a real bridge lane remap, then block its
   // final lane with the player. Checking the requested outer lane is unsafe.
   bool bridgeFound=false;
   for(int attempt=0;attempt<8&&!bridgeFound;attempt++)
   {
    reset();g.riversEnabled=true;g.RestartRide();clear();
    for(float z=20;z<=250&&!bridgeFound;z+=5)
    {
     var probe=g.SpawnTraffic(0,z);if(probe==null)throw new Exception("Bridge fixture pool empty");
     bool remapped=probe.lane==1;probe.active=false;probe.model.gameObject.SetActive(false);
     if(!remapped)continue;
     bridgeFound=true;g.rider.localPosition=new Vector3(PhoVietGame.LaneX[1],0,z);
     check(!(bool)call("TurnTankSpace",new object[]{0,z,1f,4f}),"Bridge spawn checks the actual inner lane occupied by the player");
     g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],0,0);set("junctionAt",g.Distance+z);queue();
     check(g.DinosaursSpawned==1&&g.TanksScheduled==1&&pending()==0,"Narrow bridge approach releases both hazards immediately, without waiting for the dinosaur to walk away");
     tank=g.vehicles.First(x=>x.active&&x.armored);check(Mathf.Abs(tank.model.localPosition.z-z-12)<.01f&&tank.baseTrafficSpeed<0,"Bridge tank is staggered twelve metres across the same junction instead of overlapping the dinosaur");
    }
   }
   check(bridgeFound,"Real bridge lane-remapping fixture was exercised");
   // Real turn entry, arc, coordinate rebase and normal TickRide integration.
   foreach(int seed in new[]{31,73,125})foreach(int lane in new[]{0,1,2,3})foreach(bool boost in new[]{false,true})foreach(float dt in new[]{1f/60,.05f})
   {
    reset();g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();clear();set("rng",new System.Random(seed));
    int guard=0;while(g.NextJunctionDistance>24&&guard++<1000){tick(.02f);clear();}
    typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
    g.dinosaursEnabled=g.tankBarrageEnabled=true;set("dinosaurTimer",1000f);set("tankSpawnTimer",1000f);if(boost)g.TryBoost();g.MoveLane(lane<2?-1:1);
    guard=0;while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<500){tick(dt);if(g.CompletedTurns==0)clear();}
    string label=$"seed={seed},lane={lane},boost={boost},dt={dt}";
    check(g.CompletedTurns==1&&g.State==PhoVietGame.RideState.Riding&&g.DinosaursSpawned==1&&g.TanksScheduled==1,"Both hazards appear on the same real turn-completion frame: "+label);
    d=g.dinosaurs.First(x=>x.active);check(Mathf.Abs(d.root.localPosition.z-g.NextJunctionDistance)<.3f,"Turn rebase puts dinosaur at next junction on new street: "+label);
    float elapsed=0;check(pending()==0,"Turn pair needs no later simulation frame: "+label);
    tank=g.vehicles.First(x=>x.active&&x.armored);check(tank.model.localPosition.z>0&&tank.baseTrafficSpeed<0&&Mathf.Abs(tank.model.localPosition.z-g.NextJunctionDistance)<.5f,"Immediate tank uses the same next junction and remains oncoming during boost: "+label);
    evidence.Add(new{lane,boost,dt,elapsed,dinosaurZ=d.root.localPosition.z,tankZ=tank.model.localPosition.z});
   }
   reset();g.tankBarrageEnabled=false;queue();check(g.DinosaursSpawned==1&&g.TanksScheduled==0&&pending()==0,"Disabling tanks retains the immediate dinosaur without a pending tank");
   reset();g.dinosaursEnabled=g.tankBarrageEnabled=false;queue();schedule(3);check(pending()==0&&g.DinosaursSpawned==0&&g.TanksScheduled==0,"Disabled hazard fixture cannot create post-turn hazards");
   check(!PhoVietGame.RideJobsReleased&&!g.jobsEnabled&&PlayerPrefs.GetInt("PhoViet.Coins.v1",0)==coins&&g.BestDistance==best,"Jobs, coins and high score remain unchanged");
   var report=new{success=true,count=checks.Count,checks,evidence};File.WriteAllText("Captures/tank2199-turn-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally
  {
   g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.riversEnabled=rivers;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=time;
  }
 }
}
