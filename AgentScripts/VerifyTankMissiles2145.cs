using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyTankMissiles2145
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static PhoVietGame g;static List<string> checks;
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static object Field(object o,string n)=>o.GetType().GetField(n).GetValue(o);
 static void Check(bool b,string n){if(!b)throw new Exception(n);checks.Add(n);}
 static void Fresh(){g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);Set("collisionImmunity",0f);}
 static PhoVietGame.TrafficVehicle Tank(float x,float z,float yaw=180)
 {var v=g.SpawnScheduledTank();if(v==null)throw new Exception("No free tank slot");v.model.localPosition=v.previousPosition=new Vector3(x,0,z);v.model.localRotation=Quaternion.Euler(0,yaw,0);v.previousYaw=yaw;v.speed=v.baseTrafficSpeed=yaw==180?-8:8;v.playerGrace=0;return v;}
 static void Step(float dt){Call("ScrollPlayerMissiles",g.Speed*dt);Call("TickTrafficMotion",dt);Call("TickPlayerMissiles",dt);Call("RenderThunderstorm");}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.thunderstormEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   Fresh();var tiles=(IList)Get("energyTiles");var allLanes=new HashSet<int>();int bridges=0;
   Check(PhoVietGame.GroundPickupsPerTile==6&&PhoVietGame.AirPickupsPerTile==7&&tiles.Count==35,"Each of35 tiles gets old2+4 ground and old3+4 air icons");
   Check(g.EnergyPickupPoolCount==210,"Fixed ground pool has210 icons");
   var roots=new List<Transform>();
   foreach(var e in tiles)
   {
    var tile=(Transform)Field(e,"tile");var ground=(Transform[])Field(e,"icons");var air=(Transform[])Field(e,"airIcons");var lanes=(int[])Field(e,"lanes");var airLanes=(int[])Field(e,"airLanes");var river=tile.GetComponent<RiverBlock>();bool bridge=river!=null&&river.IsBridge;if(bridge)bridges++;
    Check(ground.Length==6&&air.Length==7&&((bool[])Field(e,"collected")).Length==6&&((bool[])Field(e,"airCollected")).Length==7,"Array sizes match density "+tile.name);
    Check(lanes.All(l=>l>=0&&l<4)&&airLanes.All(l=>l>=0&&l<4),"Random lanes stay inside road "+tile.name);
    Check(Enumerable.Range(1,6-1).All(i=>lanes[i]!=lanes[i-1]),"Consecutive ground icons use different lanes "+tile.name);
    if(bridge)Check(lanes.All(l=>l==1||l==2)&&ground.All(t=>t.localPosition.z>RiverBlock.Near&&t.localPosition.z<RiverBlock.Far),"Bridge icons stay on deck "+tile.name);
    Check(ground.All(t=>t.localPosition.z>0&&t.localPosition.z<VietnamStreet.Length)&&air.All(t=>t.localPosition.z>0&&t.localPosition.z<VietnamStreet.Length),"All13 icons stay within their tile "+tile.name);
    Check(air.All(t=>!t.gameObject.activeSelf&&Mathf.Abs(t.localPosition.y-PhoVietGame.AirPickupHeight)<.001f),"Air icons retain flight height and are hidden on ground "+tile.name);
    foreach(int l in lanes.Concat(airLanes))allLanes.Add(l);roots.AddRange(ground);roots.AddRange(air);
   }
   Check(bridges>0&&allLanes.SetEquals(new[]{0,1,2,3}),"Street/bridge distribution covers all four lanes");
   Fresh();var nextRoots=new List<Transform>();foreach(var e in tiles){nextRoots.AddRange((Transform[])Field(e,"icons"));nextRoots.AddRange((Transform[])Field(e,"airIcons"));}
   Check(roots.SequenceEqual(nextRoots)&&roots.Count==455,"Retry reuses all455 pickup objects instead of growing the pool");
   // The extra sixth ground icon is collectable and still worth15.
   Fresh();object row=tiles.Cast<object>().First(e=>((Transform)Field(e,"tile")).gameObject.activeInHierarchy);foreach(var e in tiles)foreach(var t in (Transform[])Field(e,"icons"))t.position=Vector3.one*10000;
   var icon=((Transform[])Field(row,"icons"))[5];icon.position=g.rider.position;Set("Energy",60);Call("TickEnergyPickups",0f,g.rider.localPosition);Check(g.Energy==75&&((bool[])Field(row,"collected"))[5],"An added ground icon restores15 on real contact");
   Fresh();Check(g.PlayerCanFire&&g.TryFirePlayerMissile()&&g.PlayerMissiles==4,"Ground fire without a target launches and spends one");Check(!g.TryFirePlayerMissile(),"Ground fire respects0.4s cooldown");
   g.TogglePause();Check(!g.TryFirePlayerMissile(),"Pause blocks ground fire");g.TogglePause();
   foreach(float dt in new[]{1/60f,.05f})foreach(float speed in new[]{17f,60f})foreach(float yaw in new[]{0f,180f})
   {
    Fresh();g.riversEnabled=false;Set("Speed",speed);var tank=Tank(-1.55f,85,yaw);Check(g.TryFirePlayerMissile(),"Launch against moving tank dt="+dt+" speed="+speed+" heading="+yaw);
    for(int i=0;i<250&&!tank.tankDestroyed;i++)Step(dt);
    Check(tank.tankDestroyed&&g.PlayerTanksDestroyed==1&&g.PlayerMissiles==4,"One missile destroys a moving tank");
    Check(tank.active&&tank.armored&&tank.knocked&&tank.speed==0&&tank.baseTrafficSpeed==0&&tank.motionVelocity==Vector3.zero&&tank.recovery==null,"Destroyed tank remains a solid stationary body");
    var p=tank.model.localPosition;for(int i=0;i<80;i++)Call("TickTrafficMotion",.05f);
    Check(Vector3.Distance(tank.model.localPosition+Vector3.forward*speed*4,p)<.008f&&tank.recovery==null,"Wreck stays fixed in road space after4s, no recovery");
    g.tankBarrageEnabled=true;int fired=g.ShellsFired;Call("TickTankFire",1f);Check(g.ShellsFired==fired&&g.FireTankShell(tank,2)==null,"Destroyed tank cannot fire via scheduler or direct API");g.tankBarrageEnabled=false;
    Call("RenderThunderstorm");var fx=(ThunderstormVisual)Get("stormVisual");Check(fx!=null&&fx.BurningCount==1,"Persistent fire remains active outside thunderstorm");
    var burns=(IList)typeof(ThunderstormVisual).GetField("burns",F).GetValue(fx);var burn=burns.Cast<object>().First(b=>(bool)Field(b,"active"));var fire=(Transform)Field(burn,"root");
    Check(fire.localScale.x==2.8f&&fire.GetComponentsInChildren<ParticleSystem>(true).Length>=2&&fire.GetComponentsInChildren<ParticleSystem>(true).All(ps=>ps.main.scalingMode==ParticleSystemScalingMode.Hierarchy)&&!fire.GetComponentsInChildren<Light>(true).Any(l=>l.enabled),"Large baked flame and smoke inherit2.8x scale with no realtime lights");
   }
   Fresh();var near=Tank(-1.55f,45);var far=Tank(4.65f,95);Check(g.TryFirePlayerMissile(),"Launch with two target tanks");var missile=((Array)Get("playerMissiles")).GetValue(0);Check(Field(missile,"tankTarget")==near,"Ground fire selects nearest valid tank ahead");
   int generation=near.spawnGeneration;Call("InitializeDriver",near);Call("TickPlayerMissiles",.01f);Check(near.spawnGeneration!=generation&&Field(missile,"tankTarget")==null&&!near.tankDestroyed,"Recycled slot cannot inherit damage or a homing lock");
   // A wreck directly ahead must still end the ordinary ride through the existing collision path.
   Fresh();Set("Speed",20f);var wreck=Tank(g.rider.localPosition.x,25);Call("DestroyPlayerTank",wreck,wreck.model.localPosition);var at=g.rider.localPosition;wreck.model.localPosition=wreck.previousPosition=at+Vector3.forward*(g.PlayerHalfLength+wreck.halfLength-.1f);Set("oldRiderZ",at.z);Set("oldRiderYaw",g.rider.localEulerAngles.y);
   Call("ResolveRiderContacts",at.x,.05f);Check(g.State==PhoVietGame.RideState.Falling,"Driving into a destroyed tank still crashes");float score=g.Distance;
   for(int i=0;i<700&&g.State==PhoVietGame.RideState.Falling;i++)Call("TickRiderCrash",.05f);
   Check(g.State==PhoVietGame.RideState.GameOver&&g.Distance==score,"Wreck collision ends run after fall, without scoring throw distance");
   var rest=wreck.model.localPosition;var rotation=wreck.model.localRotation;for(int i=0;i<20;i++)g.TickRiverAftermath(.05f);
   Check(Vector3.Distance(rest,wreck.model.localPosition)<.0001f&&Quaternion.Angle(rotation,wreck.model.localRotation)<.0001f,"Tank wreck stays still through GameOver aftermath");
   Fresh();Call("RenderThunderstorm");Check(g.PlayerTanksDestroyed==0&&g.vehicles.All(v=>!v.tankDestroyed)&&g.PlayerMissiles==5,"Retry clears tank wreck flags, score and restores5 ammo");
   var pooledFx=(ThunderstormVisual)Get("stormVisual");pooledFx.Burn(g.rider,null);var pooledBurns=(IList)typeof(ThunderstormVisual).GetField("burns",F).GetValue(pooledFx);
   var reused=pooledBurns.Cast<object>().First(b=>(bool)Field(b,"active"));var psList=(ParticleSystem[])Field(reused,"particles");
   Check(psList.Select(ps=>ps.main.scalingMode).SequenceEqual((ParticleSystemScalingMode[])Field(reused,"originalScaleModes"))&&Mathf.Abs(((Transform)Field(reused,"root")).localScale.x-1.3f)<.001f,"Reusing a tank fire for another actor restores original scaling modes and size");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased&&!PhoVietGame.BoostReleased,"Jobs/boost remain locked");
   var result=new{success=true,count=checks.Count,groundIcons=210,airIcons=245,checks};File.WriteAllText("Captures/tank2145-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.thunderstormEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);Time.timeScale=scale;}
 }
}
