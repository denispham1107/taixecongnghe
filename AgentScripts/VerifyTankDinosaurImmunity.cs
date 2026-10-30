using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyTankDinosaurImmunity
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;
  bool dinos=g.dinosaursEnabled,rivers=g.riversEnabled,tanks=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var cases=new List<object>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  try
  {
   Time.timeScale=0;
   foreach(bool tail in new[]{false,true})foreach(float dt in new[]{1f/60,1f/30,.05f})
   {
    g.dinosaursEnabled=true;g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.tankBarrageEnabled=true;g.RestartRide();
    foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
    var tank=g.SpawnScheduledTank();check(tank!=null&&tank.armored,"Scheduled tank fixture exists");
    var d=g.SpawnDinosaur();d.root.localPosition=new Vector3(0,0,75);d.visual.ResetPose();
    tank.lane=1;tank.targetX=PhoVietGame.LaneX[1];
    Func<Vector3,bool> contact=p=>
    {
     object[] args={d,p,tank.model.localRotation,tank.halfWidth,tank.halfLength,Mathf.Max(1.7f,PhoVietGame.JumpRoofHeight(tank)),Vector3.zero,false};
     return (bool)call("DinosaurTouches",args)&&(bool)args[7]==tail;
    };
    Vector3 at=Vector3.zero;bool found=false;
    if(tail)
    {
     d.visual.StartSweep();
     for(int i=0;i<168&&!found;i++)
     {
      d.visual.Advance(1f/60);if(!d.visual.TailDangerous)continue;
      at=g.transform.InverseTransformPoint(d.visual.tail[5].position);at.x=PhoVietGame.LaneX[1];at.y=0;found=contact(at);
     }
    }
    else
    {
     foreach(var foot in new[]{d.visual.leftFoot,d.visual.rightFoot})
     {at=g.transform.InverseTransformPoint(foot.position);at.x=PhoVietGame.LaneX[1];at.y=0;if(contact(at)){found=true;break;}}
    }
    string label="tail="+tail+",dt="+dt;
    check(found,"Real rendered dinosaur collision overlaps tank: "+label);
    tank.model.localPosition=tank.previousPosition=at;tank.previousYaw=tank.model.localEulerAngles.y;call("CopyDinosaurTail",new object[]{d});
    float speed=tank.speed,baseSpeed=tank.baseTrafficSpeed,fireTimer=tank.tankFireTimer;var pose=tank.model.localRotation;int hits=g.DinosaurTailHits;
    call("ResolveDinosaurHits",new object[]{d,1f});
    check(!tank.knocked&&!tank.physicalCrash&&tank.knockVelocity.sqrMagnitude==0,"Body/tail contact does not throw the tank: "+label);
    check(tank.model.localPosition==at&&tank.model.localRotation==pose&&tank.speed==speed&&tank.baseTrafficSpeed==baseSpeed,"Contact preserves tank position heading and speed: "+label);
    check(tank.tankFireTimer==fireTimer&&!d.hits.Contains(tank)&&g.DinosaurTailHits==hits,"Contact preserves firing timer and does not credit a tail hit: "+label);
    check((bool)call("TankCanFire",new object[]{tank}),"Tank remains eligible to fire immediately: "+label);
    int fired=g.ShellsFired;
    for(int i=0;i<Mathf.CeilToInt(1.5f/dt);i++)call("TickRide",new object[]{dt});
    check(tank.active&&!tank.knocked&&!tank.physicalCrash&&Mathf.Abs(tank.model.localPosition.y)<.001f,"Tank stays grounded through subsequent dinosaur ticks: "+label);
    check(tank.model.localPosition.z<at.z-1&&Mathf.Abs(tank.speed-baseSpeed)<.001f,"Tank continues travelling at its original traffic speed: "+label);
    check(g.ShellsFired>=fired+2&&g.shells.Count>=2,"Tank continues normal automatic cannon fire: "+label);
    check(g.State==PhoVietGame.RideState.Riding,"The observation interval does not interrupt play: "+label);
    cases.Add(new{tail,dt,start=at.z,end=tank.model.localPosition.z,fired=g.ShellsFired-fired});
   }
   check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Ride jobs remain locked");
   var result=new{success=true,count=checks.Count,checks,cases};File.WriteAllText("Captures/tank-dino-immunity-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally
  {
   g.dinosaursEnabled=dinos;g.riversEnabled=rivers;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=time;
  }
 }
}
