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
   check(g.DinosaursSpawned==1&&g.TanksScheduled==0&&pending()==1,"Extra dinosaur appears immediately, tank remains pending");
   var d=g.dinosaurs.First(x=>x.active);check(Mathf.Abs(d.root.localPosition.z-65)<.01f&&Mathf.Abs(Mathf.DeltaAngle(d.root.localEulerAngles.y,180))<.01f,"Dinosaur stands at the upcoming junction facing the player");
   float dc=g.DinosaurCountdown,tc=g.TankSpawnCountdown;
   g.TogglePause();schedule(10);check(g.TanksScheduled==0&&pending()==1,"Pause cannot advance or release the wave");g.TogglePause();
   separate();for(int i=0;i<199;i++)schedule(.01f);check(g.TanksScheduled==0,"Tank does not appear before two seconds");schedule(.01f);
   check(g.TanksScheduled==1&&pending()==0,"Tank appears at two seconds after the dinosaur");
   var tank=g.vehicles.First(x=>x.active&&x.armored);check(Mathf.Abs(tank.model.localPosition.z-65)<.01f&&tank.baseTrafficSpeed<0,"Extra tank uses the upcoming junction and oncoming direction");
   check(g.DinosaurCountdown==dc&&g.TankSpawnCountdown==tc,"Additional wave does not reset or consume periodic timers");
   for(int i=0;i<100;i++)schedule(.05f);check(g.DinosaursSpawned==1&&g.TanksScheduled==1,"Finished wave is not repeated on subsequent frames");
   reset();queue();separate();set("junctionAt",g.Distance-20);schedule(2);
   check(g.TanksScheduled==1&&Mathf.Abs(g.vehicles.First(x=>x.active&&x.armored).model.localPosition.z-60)<.01f,"If the first junction is behind, tank chooses the next physical junction ahead");
   reset();queue();g.ResetRide();g.StartRide();schedule(5);check(pending()==0&&g.DinosaursSpawned==0&&g.TanksScheduled==0,"Retry cancels all pending post-turn hazards");
   reset();queue();typeof(PhoVietGame).GetProperty("State").SetValue(g,PhoVietGame.RideState.Falling);schedule(3);check(g.TanksScheduled==0,"Falling does not spawn a pending tank");
   reset();for(int i=0;i<4;i++)g.SpawnDinosaur();queue();schedule(5);
   check(g.DinosaursSpawned==4&&g.TanksScheduled==0&&pending()==1,"Full dinosaur pool retains request and never spawns tank first");
   d=g.dinosaurs.First(x=>x.active);d.active=false;d.root.gameObject.SetActive(false);schedule(.01f);
   check(g.DinosaursSpawned==5&&g.TanksScheduled==0,"Freed dinosaur slot is reused before tank countdown begins");separate();schedule(1.99f);check(g.TanksScheduled==0,"Pool delay cannot shorten the following two-second gap");schedule(.01f);check(g.TanksScheduled==1,"Tank follows the actual successful dinosaur spawn");
   reset();queue();separate();foreach(var v in g.vehicles){v.active=true;v.model.localPosition=new Vector3(1000,0,1000);}schedule(2);
   check(g.TanksScheduled==0&&pending()==1,"Full traffic pool keeps tank pending without replacing active vehicles");
   var slot=g.vehicles.First(x=>!x.eventOnly&&x.model.GetComponent<MeshyCarVariants>()!=null&&x.model.GetComponent<MeshyCarVariants>().ArmoredIndex>=0);slot.active=false;schedule(.01f);
   check(g.TanksScheduled==1&&pending()==0,"Freed traffic slot releases exactly one queued tank");
   reset();queue();separate();g.rider.localPosition=new Vector3(0,0,65);
   // Occupy both possible oncoming lanes with physical blockers.
   var blockers=g.vehicles.Where(x=>x.halfWidth<.7f&&!x.eventOnly).Take(2).ToArray();
   for(int i=0;i<2;i++){blockers[i].active=true;blockers[i].model.localPosition=new Vector3(PhoVietGame.LaneX[i],0,65);}
   var variants=g.vehicles.Select(v=>v.model.GetComponent<MeshyCarVariants>()).Where(v=>v!=null).ToArray();var prepared=typeof(MeshyCarVariants).GetField("prepared",F);var before=variants.Select(v=>v.SelectedVariant+"/"+prepared.GetValue(v)).ToArray();
   schedule(2);check(g.TanksScheduled==0,"Occupied junction cannot spawn a tank through another vehicle");
   check(before.SequenceEqual(variants.Select(v=>v.SelectedVariant+"/"+prepared.GetValue(v))),"Rejected position does not prepare or convert any ordinary vehicle into a tank");
   clear();g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],0,0);schedule(.01f);check(g.TanksScheduled==1,"Blocked junction is retried once space is clear");
   reset();queue();schedule(1);queue();separate();schedule(1);
   check(g.DinosaursSpawned==2&&g.TanksScheduled==1&&pending()==1,"A second turn keeps the first tank deadline and its own following wave");
   clear();schedule(.99f);check(g.TanksScheduled==1,"Second tank keeps its own full two-second delay");schedule(.01f);
   check(g.TanksScheduled==2&&pending()==0,"Both turn waves complete exactly once");
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
    check(g.CompletedTurns==1&&g.State==PhoVietGame.RideState.Riding&&g.DinosaursSpawned==1&&g.TanksScheduled==0,"One extra dinosaur at real turn completion: "+label);
    d=g.dinosaurs.First(x=>x.active);check(Mathf.Abs(d.root.localPosition.z-g.NextJunctionDistance)<.3f,"Turn rebase puts dinosaur at next junction on new street: "+label);
    // Lift the fixture above damage so the encounter cannot end this scheduling test.
    set("jumpFlying",true);set("jumpHeight",50f);set("jumpPreviousHeight",50f);set("jumpVelocity",20f);g.rider.localPosition+=Vector3.up*50;
    float elapsed=0;while(g.TanksScheduled==0&&elapsed<2.3f&&g.State==PhoVietGame.RideState.Riding){tick(dt);elapsed+=dt;}
    check(g.TanksScheduled==1&&elapsed>=1.9f&&elapsed<=2.3f,"Normal ride clock releases one following tank: "+label+", t="+elapsed+", tanks="+g.TanksScheduled+", next="+g.NextJunctionDistance+", dinosaur="+d.root.localPosition+", rider="+g.rider.localPosition+", widths="+string.Join(",",g.vehicles.Where(v=>v.armored).Select(v=>v.halfWidth+"/"+v.halfLength)));
    tank=g.vehicles.First(x=>x.active&&x.armored);check(tank.model.localPosition.z>0&&tank.baseTrafficSpeed<0,"Following tank remains ahead even during boost: "+label);
    evidence.Add(new{lane,boost,dt,elapsed,dinosaurZ=d.root.localPosition.z,tankZ=tank.model.localPosition.z});
   }
   reset();g.dinosaursEnabled=g.tankBarrageEnabled=false;queue();schedule(3);check(pending()==0&&g.DinosaursSpawned==0&&g.TanksScheduled==0,"Disabled hazard fixture cannot create post-turn hazards");
   check(!PhoVietGame.RideJobsReleased&&!g.jobsEnabled&&PlayerPrefs.GetInt("PhoViet.Coins.v1",0)==coins&&g.BestDistance==best,"Jobs, coins and high score remain unchanged");
   var report=new{success=true,count=checks.Count,checks,evidence};File.WriteAllText("Captures/turn-hazards-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally
  {
   g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.riversEnabled=rivers;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=time;
  }
 }
}
