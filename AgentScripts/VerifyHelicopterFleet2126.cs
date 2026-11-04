using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyHelicopterFleet2126
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks;
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Check(bool b,string s){if(!b)throw new Exception(s);checks.Add(s);}
 static object Actor(int i)=>((Array)Get("helicopters")).GetValue(i);
 static object Field(int i,string n)=>Actor(i).GetType().GetField(n).GetValue(Actor(i));
 static void SetActor(int i,string n,object v)=>Actor(i).GetType().GetField(n).SetValue(Actor(i),v);
 static Transform Root(int i)=>g.GetHelicopterRoot(i);
 static void Tick(float dt=.02f,bool collisions=true)=>Call("TickHelicopter",dt,g.rider.localPosition,collisions);
 static void Fresh(){g.RestartRide();g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);}
 static void Hover(){Check(g.TryJump(),"Real launch accepted");Call("TickJump",.4f);var p=g.rider.localPosition;p.y=g.RiderJumpHeight;g.rider.localPosition=p;Check(g.TryJump()&&g.RiderFlying,"Second input holds actual airborne height");}
 static void Formation(int expected)
 {
  Check(g.ActiveHelicopterCount==expected,"Active count matches distance limit: "+expected);
  for(int i=0;i<5;i++)if(g.IsHelicopterActive(i))
  {
   Check(Array.Exists(PhoVietGame.LaneX,x=>Mathf.Abs(x-Root(i).localPosition.x)<.001f),"Helicopter selects a real lane "+i);
   Check(Vector3.Distance((Vector3)Field(i,"velocity"),Vector3.back*16)<.001f,"Helicopter flies opposite at16m/s "+i);
   Check(Mathf.Abs(Root(i).localPosition.y-g.rider.localPosition.y-g.PlayerHeight*.5f)<.001f,"Helicopter stays at car body altitude "+i);
   for(int j=i+1;j<5;j++)if(g.IsHelicopterActive(j))Check(Mathf.Abs(Root(i).localPosition.z-Root(j).localPosition.z)>=11.99f,"Rotor-safe formation spacing "+i+"/"+j);
  }
 }
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();
  float time=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);bool sound=(bool)Get("sound");
  bool[] flags={g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  try
  {
   Time.timeScale=0;
   float[] distances={0,399.99f,400,400.01f,800,800.01f,1200,1200.01f,1600,999999};int[] limits={2,2,2,3,3,4,4,5,5,5};
   for(int i=0;i<distances.Length;i++)Check(PhoVietGame.HelicopterLimitAtDistance(distances[i])==limits[i],"Milestone boundary "+distances[i]+" => "+limits[i]);
   Fresh();Check(g.HelicopterPoolCount==5&&!g.HelicopterActive,"Five pooled models are created once and start hidden");
   var pooled=Enumerable.Range(0,5).Select(Root).ToArray();
   var clip=Root(0).GetComponent<HelicopterVisual>().rotorAudio.clip;
   var mesh=Root(0).GetComponentInChildren<MeshFilter>(true).sharedMesh;
   for(int i=1;i<5;i++)Check(Root(i).GetComponent<HelicopterVisual>().rotorAudio.clip==clip&&Root(i).GetComponentsInChildren<MeshFilter>(true).Any(m=>m.sharedMesh==mesh),"Pool shares imported mesh/audio assets "+i);
   Tick();Check(!g.HelicopterActive,"Ground play never spawns a fleet");Set("Distance",2000f);Tick();Check(!g.HelicopterActive,"Ground milestone does not spawn aircraft");
   Fresh();g.TryJump();Tick();Check(!g.HelicopterActive,"Ordinary jump is not hovering");
   Fresh();Hover();Tick();Formation(2);Check(Root(0).localPosition.x!=Root(1).localPosition.x,"First two aircraft occupy distinct lanes");Check(g.HelicoptersSpawned==2,"Both starting aircraft spawn during one update");
   foreach(var pair in new[]{new Vector2(400,2),new Vector2(400.01f,3),new Vector2(800,3),new Vector2(800.01f,4),new Vector2(1200.01f,5),new Vector2(10000,5)})
   {Set("Distance",pair.x);Tick();Formation((int)pair.y);Check(g.HelicoptersSpawned==(int)pair.y,"Only newly earned slots spawn at "+pair.x);}
   for(int i=0;i<20;i++)Tick();Check(g.ActiveHelicopterCount==5&&g.HelicoptersSpawned==5,"Repeated updates never exceed five or spawn extras");
   var positions=pooled.Select(p=>p.localPosition).ToArray();Call("ScrollHelicopter",2f);Tick(.1f);
   for(int i=0;i<5;i++)Check(Vector3.Distance(Root(i).localPosition,positions[i]-Vector3.forward*3.6f)<.001f,"Every aircraft scrolls and flies independently "+i);
   var visuals=pooled.Select(p=>p.GetComponent<HelicopterVisual>()).ToArray();foreach(var v in visuals)v.Advance(.037f);
   Check(visuals.All(v=>v.MainAngle!=0&&v.TailAngle!=0&&v.rotorAudio.isPlaying),"All five models animate both rotors and play their clips");
   var angles=visuals.Select(v=>v.MainAngle).ToArray();positions=pooled.Select(p=>p.localPosition).ToArray();g.TogglePause();Tick(2);
   for(int i=0;i<5;i++){visuals[i].Advance(.5f);Check(Root(i).localPosition==positions[i]&&visuals[i].MainAngle==angles[i]&&!visuals[i].rotorAudio.isPlaying,"Pause freezes movement, rotors and sound "+i);}
   g.TogglePause();Set("sound",false);foreach(var v in visuals)v.Advance(.02f);Check(visuals.All(v=>v.rotorAudio.volume==0),"Mute covers the entire fleet");Set("sound",true);
   foreach(var v in visuals)v.Advance(.02f);Check(visuals.All(v=>v.rotorAudio.isPlaying&&v.rotorAudio.volume>0),"Resume and unmute restore all aircraft");
   positions=pooled.Select(p=>p.localPosition).ToArray();Quaternion inverse=Quaternion.Euler(0,-90,0);Vector3 origin=new Vector3(2,0,3);Call("ScrollHelicopter",2f);Call("RebaseHelicopter",inverse,origin);
   for(int i=0;i<5;i++)Check(Vector3.Distance(Root(i).localPosition,inverse*(positions[i]-origin))<.001f&&Vector3.Distance((Vector3)Field(i,"velocity"),inverse*Vector3.back*16)<.001f&&Vector3.Distance((Vector3)Field(i,"scroll"),inverse*Vector3.forward*2)<.001f,"Rebase transforms every actor and pending scroll "+i);
   Tick(.02f,false);Check(!(bool)Get("helicopterRebased"),"Previous player position is rebased once for all five sweeps");
   Fresh();Hover();Set("Distance",1200.01f);Tick();int spawned=g.HelicoptersSpawned;
   for(int wave=0;wave<20;wave++)
   {
    for(int i=0;i<5;i++)Root(i).localPosition=new Vector3(0,4,-40);
    Tick();Check(g.ActiveHelicopterCount==5&&g.HelicoptersSpawned==spawned+(wave+1)*5,"Departing aircraft are replaced up to the current cap, wave "+wave);
    Check(Enumerable.Range(0,5).All(i=>Root(i)==pooled[i]),"Replacements allocate no extra actor, wave "+wave);
   }
   g.TryJump();for(int i=0;i<5;i++)Root(i).localPosition=new Vector3(0,4,-40);Tick();Tick();Check(!g.HelicopterActive,"Descent lets all aircraft leave without spawning replacements");
   Fresh();Hover();Set("Distance",399.99f);Tick();Call("TickRide",.02f);Check(g.Distance>400&&g.ActiveHelicopterCount==3,"Real road update adds the third aircraft when the score crosses400m");
   var used=new HashSet<int>();
   for(int seed=0;seed<12;seed++)
   {
    Fresh();Set("RunSeed",seed);Call("ResetHelicopter");Hover();Tick();Check(Root(0).localPosition.x!=Root(1).localPosition.x,"Random initial pair is distinct for seed "+seed);
    for(int i=0;i<2;i++)used.Add(Array.FindIndex(PhoVietGame.LaneX,x=>Mathf.Abs(x-Root(i).localPosition.x)<.001f));
   }
   Check(used.SetEquals(new[]{0,1,2,3}),"Random pairs use all four lanes across runs");
   foreach(bool chase in new[]{false,true})for(int hit=0;hit<5;hit++)
   {
    Fresh();Hover();Set("Distance",1201f);Tick();for(int i=0;i<5;i++)Root(i).localPosition=new Vector3(0,4,300);
    Root(hit).localPosition=new Vector3(g.rider.localPosition.x,g.rider.localPosition.y+g.PlayerHeight*.5f,15);if(chase)Call("StartChase");float score=g.Distance;Call("ScrollHelicopter",30f);Tick(.1f);
    Check(g.HitByHelicopter&&g.State==PhoVietGame.RideState.Falling&&!g.CrashDuringPursuit&&g.RiderAirAttackBurning,"Swept impact works for slot "+hit+", pursuit="+chase);
    int before=g.HelicoptersSpawned;Tick(.02f,false);Call("TickRiderCrash",.1f);Check(g.Distance==score&&g.HelicoptersSpawned==before,"Crash freezes score and new spawns for slot "+hit);
   }
   g.ResetRide();Check(g.Distance==0&&g.HelicopterTargetCount==2&&!g.HelicopterActive&&g.HelicoptersSpawned==0&&!g.HitByHelicopter,"Retry resets distance, fleet and progression");Check(Enumerable.Range(0,5).All(i=>Root(i)==pooled[i]),"Retry retains exactly the same five pooled models");
   var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/missile2126-regression-fleet-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally
  {
   g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];Set("sound",sound);Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);Time.timeScale=time;g.ResetRide();
  }
 }
}
