using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using UnityEngine;using PhoViet;
public static class VerifyThunderstorm
{
 static PhoVietGame g;static List<string> checks=new List<string>();
 static object Call(string method,params object[] args)=>typeof(PhoVietGame).GetMethod(method,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(g,args);
 static void Set(string property,object value)=>typeof(PhoVietGame).GetProperty(property).SetValue(g,value);
 static void Check(bool yes,string label){if(!yes)throw new Exception(label);checks.Add(label);}
 static void Fresh(){g.ResetRide();g.StartRide();g.riversEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}if(g.policeBike!=null)g.policeBike.gameObject.SetActive(false);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;Time.timeScale=0;
  bool river=g.riversEnabled,dino=g.dinosaursEnabled,tank=g.tankBarrageEnabled,enabled=g.thunderstormEnabled;
  var dimensions=g.vehicles.Select(v=>new Vector2(v.halfWidth,v.halfLength)).ToArray();
  int best=g.BestDistance,savedBest=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);bool hadBest=PlayerPrefs.HasKey("PhoViet.BestMetres.v1");
  g.thunderstormEnabled=true;g.dinosaursEnabled=false;g.tankBarrageEnabled=false;
  try
  {
   foreach(float offset in new[]{0f,30f,60f})foreach(var sample in new[]{new Vector2(0,0),new Vector2(14.99f,0),new Vector2(15,0),new Vector2(16.5f,.5f),new Vector2(18,1),new Vector2(26.99f,1),new Vector2(27,1),new Vector2(28.5f,.5f),new Vector2(29.999f,0)})Check(Mathf.Abs(PhoVietGame.RainAt(sample.x+offset)-sample.y)<.001f,"Continuous rain envelope "+(sample.x+offset));
   Fresh();for(int i=0;i<1500;i++)g.TickThunderstorm(.05f,true);
   Check(Mathf.Abs(g.StormTime-75)<.003f,"30s repeating simulation clock");Check(g.LightningStrikes>=6&&g.LightningStrikes<=10,"Bounded strike cadence across repeated storms");
   float time=g.StormTime;int strikes=g.LightningStrikes;g.TogglePause();g.TickThunderstorm(10,true);Check(g.StormTime==time&&g.LightningStrikes==strikes,"Pause freezes clock and damage");g.TogglePause();
   Fresh();g.WarnLightning(new Vector3(5,0,50));g.TickThunderstorm(.79f,true);Check(g.LightningPending&&g.LightningStrikes==0,"Warning stays visible before 0.8s");g.TickThunderstorm(.011f,true);Check(!g.LightningPending&&g.LightningStrikes==1,"Warning resolves exactly once after countdown");
   Fresh();g.WarnLightning(new Vector3(1.6f,0,40));Vector3 point=g.LightningTarget;Call("ScrollThunderstorm",4f);Check(Vector3.Distance(g.LightningTarget,point-Vector3.forward*4)<.0001f,"Warning follows scrolling road");
   foreach(int side in new[]{-1,1}){point=g.LightningTarget;var inverse=Quaternion.Euler(0,side*90,0);var origin=new Vector3(2,0,4);Call("RebaseThunderstorm",inverse,origin);Check(Vector3.Distance(g.LightningTarget,inverse*(point-origin))<.0001f,"Warning preserves world point across turn "+side);}
   var dirs=new[]{Vector3.forward,Vector3.back,Vector3.right,Vector3.left};
   foreach(float width in new[]{.43f,1.1f})foreach(var direction in dirs)
   {
    Fresh();g.rider.localPosition=new Vector3(30,0,0);var v=g.vehicles.First(x=>!x.eventOnly);v.active=true;v.model.gameObject.SetActive(true);v.model.localPosition=new Vector3(0,0,40);v.model.localRotation=Quaternion.LookRotation(direction);v.halfWidth=width;v.halfLength=width<.7f?1.1f:3.2f;v.motionVelocity=direction*14;v.armored=false;
    g.StrikeLightning(v.model.localPosition);
    Check(v.lightningWreck&&v.knocked&&v.physicalCrash,"Lightning wreck state "+width+direction);Check(v.recovery==null&&Vector3.Dot(v.knockVelocity,direction)>9,"Thrown along pre-strike travel direction "+direction);Check(v.knockVelocity.y>=4,"Impact lifts vehicle");
    Call("RememberTrafficCourse",v);Check(v.recovery==null&&!(bool)Call("TickTrafficRecovery",v,8f),"Lightning wreck never resumes after 3s");
    Set("Speed",0f);for(int i=0;i<600;i++)Call("TickCollisionBody",v,1/60f);v.model.localPosition=new Vector3(0,0,40);Call("RetireTraffic",false);
    Check(v.active&&v.knockVelocity.sqrMagnitude<.05f,"Wreck remains near road after 10s");
   }
   Fresh();g.rider.localPosition=new Vector3(30,0,0);var armored=g.vehicles.First(x=>!x.eventOnly);armored.active=true;armored.model.gameObject.SetActive(true);armored.model.localPosition=new Vector3(0,0,30);armored.armored=true;var pose=armored.model.localPosition;g.StrikeLightning(pose);Check(!armored.knocked&&!armored.lightningWreck&&armored.model.localPosition==pose,"Tank immune to lightning");
   g.dinosaursEnabled=true;var dinosaur=g.SpawnDinosaur();g.dinosaursEnabled=false;dinosaur.root.gameObject.SetActive(true);dinosaur.active=true;dinosaur.root.localPosition=new Vector3(0,0,30);g.StrikeLightning(dinosaur.root.localPosition);Check(dinosaur.active&&dinosaur.root.gameObject.activeSelf&&dinosaur.root.localPosition==pose,"Dinosaur immune to lightning");
   foreach(bool chased in new[]{false,true})foreach(float height in new[]{0f,4.1f})
   {
    Fresh();g.rider.localPosition=new Vector3(1.6f,height,0);Set("IsChased",chased);g.StrikeLightning(g.rider.localPosition);
    Check(g.State==PhoVietGame.RideState.Falling&&g.KilledByLightning,"Player hit on ground/in air "+chased+height);Check(!g.CrashDuringPursuit&&!g.IsChased,"Lightning overrides waiting for police");Check(g.RiderFallVelocity.z>0&&g.RiderFallVelocity.y>0,"Player thrown forward and upward");
    for(int i=0;i<900&&g.State==PhoVietGame.RideState.Falling;i++)Call("TickRiderCrash",1/60f);
    Check(g.State==PhoVietGame.RideState.GameOver,"Lightning ends game after fall "+chased+height);
   }
   foreach(bool down in new[]{false,true})
   {
    Fresh();Set("IsChased",true);if(down)g.Crash();g.policeBike.gameObject.SetActive(true);g.policeBike.localPosition=new Vector3(-4,0,20);Set("PoliceSpeed",16f);g.StrikeLightning(g.policeBike.localPosition);
    Check(g.PoliceLightningWreck&&!g.IsChased&&!g.CrashDuringPursuit,"Officer strike cancels chase/wait "+down);
    var body=(PhoVietGame.TrafficVehicle)typeof(PhoVietGame).GetField("policeBody",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(g);Check(body.lightningWreck&&body.knocked&&body.recovery==null,"Officer permanent burning wreck");
    if(down){for(int i=0;i<900&&g.State==PhoVietGame.RideState.Falling;i++)Call("TickRiderCrash",1/60f);Check(g.State==PhoVietGame.RideState.GameOver,"No stuck waiting after police strike");}
    else
    {
     Check(g.State==PhoVietGame.RideState.Riding,"Unhurt player continues after officer strike");Call("StartChase");
     var fresh=(PhoVietGame.TrafficVehicle)typeof(PhoVietGame).GetField("policeBody",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(g);
     Check(g.IsChased&&!g.PoliceLightningWreck&&!fresh.lightningWreck&&!fresh.knocked&&!fresh.physicalCrash&&!body.lightningWreck,"A later new offense starts a fresh officer without old wreck physics");
     Call("RenderThunderstorm");Check(g.GetComponent<ThunderstormVisual>().BurningCount==0,"A later officer never inherits the old fire effect");
    }
   }
   Fresh();var visual=g.GetComponent<ThunderstormVisual>();Check(visual!=null&&visual.ParticleBudget==2400,"Bounded rain particle volume");
   g.riversEnabled=true;var tile=g.blocks[1];tile.GetComponent<RiverBlock>().SetBridge(true);var water=g.transform.InverseTransformPoint(tile.TransformPoint(new Vector3(5,0,30)));
   Check(g.IsStormWater(water),"River strike fixture is over water");g.StrikeLightning(water);var bolt=visual.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Sét phân nhánh • hai nhịp sáng");Check(Mathf.Abs(bolt.localPosition.y-RiverBlock.WaterY)<.001f,"Lightning reaches actual water surface rather than floating over river");
   Fresh();visual=g.GetComponent<ThunderstormVisual>();
   g.WarnLightning(new Vector3(1,0,20));g.TogglePause();g.StrikeLightning(g.rider.localPosition);Check(g.LightningStrikes==0&&!g.KilledByLightning,"No damage while paused");g.TogglePause();g.ResetRide();
   Check(g.StormTime==0&&g.StormIntensity==0&&g.StormFlash==0&&!g.LightningPending&&!g.KilledByLightning&&!g.PoliceLightningWreck&&g.vehicles.All(v=>!v.lightningWreck),"Retry clears complete storm state");Check(visual.BurningCount==0,"Retry returns all fire effects to pool");
   var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/storm-gameplay-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally
  {
   for(int i=0;i<dimensions.Length;i++){g.vehicles[i].halfWidth=dimensions[i].x;g.vehicles[i].halfLength=dimensions[i].y;}
   if(hadBest)PlayerPrefs.SetInt("PhoViet.BestMetres.v1",savedBest);else PlayerPrefs.DeleteKey("PhoViet.BestMetres.v1");PlayerPrefs.Save();Set("BestDistance",best);
   g.riversEnabled=river;g.dinosaursEnabled=dino;g.tankBarrageEnabled=tank;g.thunderstormEnabled=enabled;g.ResetRide();Time.timeScale=scale;
  }
 }
}
