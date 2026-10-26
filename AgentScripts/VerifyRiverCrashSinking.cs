using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyRiverCrashSinking
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()=>Run(false);
 public static object Reproduce()=>Run(true);
 static object Run(bool quick)
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var failures=new List<string>();var data=new List<object>();
  Action<bool,string> check=(ok,s)=>{if(ok)checks.Add(s);else failures.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>{try{return typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);}catch(TargetInvocationException e){throw new Exception(n+": "+e.InnerException,e.InnerException);}};
  Action<PhoVietGame.RideState> state=s=>typeof(PhoVietGame).GetProperty("State").SetValue(g,s);
  Action reset=()=>{g.dinosaursEnabled=false;g.riversEnabled=true;g.RestartRide();g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}foreach(var row in g.roadRows)foreach(var tile in row.tiles)tile.GetComponent<RiverBlock>().SetBridge(g.blocks.Contains(tile)&&Mathf.Abs(tile.localPosition.z)<1);set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);typeof(PhoVietGame).GetProperty("Speed").SetValue(g,0f);g.rider.localPosition=new Vector3(1.55f,0,-80);};
  Func<string,PhoVietGame.TrafficVehicle> spawn=kind=>
  {
   if(kind=="reckless"||kind=="drowsy")return g.SpawnReckless(kind=="drowsy",true);
   if(kind=="tank")return g.SpawnScheduledTank();
   if(kind=="bike")return g.SpawnTraffic(2,30,g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f));
   var v0=g.vehicles.First(v=>!v.eventOnly&&v.model.GetComponent<MeshyCarVariants>()?.variants.Any(m=>m.asset==kind)==true);
   var v=g.SpawnTraffic(2,30,g.vehicles.IndexOf(v0));var car=v.model.GetComponent<MeshyCarVariants>();car.PrepareVariant(Array.FindIndex(car.variants,m=>m.asset==kind),v);return v;
  };
  try
  {
   Time.timeScale=0;
   foreach(string kind in quick?new[]{"PV_MeshySport"}:new[]{"bike","PV_MeshySport","PV_MeshyPickup","PV_MeshyCovered","PV_MeshyContainer","reckless","drowsy","tank"})
   foreach(float dt in quick?new[]{1f/60}:new[]{1f/60,1f/30,.05f})foreach(string mode in quick?new[]{"recovery","aftermath"}:new[]{"recovery","detached","aftermath"})
   {
    reset();var v=spawn(kind);if(v==null)throw new Exception("Missing fixture "+kind);
    v.model.localPosition=v.previousPosition=new Vector3(4.9f,2,30);v.model.localRotation=Quaternion.identity;v.previousYaw=0;
    call("ActivateCollisionBody",new object[]{v,Vector3.forward*8,new Vector3(3,4,2),Vector2.right*PhoVietGame.VehicleMass(v)*8,Vector2.zero,false,true,8f});
    v.knockVelocity=new Vector3(3,4,2);v.motionVelocity=v.knockVelocity;
    string label=kind+"/"+mode+"/dt="+dt;check(v.recovery!=null,label+" reproduces a real collision with recovery, not a manually-set knocked flag");
    if(mode=="detached"){v.routeDetached=true;v.crossing=true;v.roadTurning=true;}
    if(mode=="aftermath")state(PhoVietGame.RideState.GameOver);
    Action tick=()=>{if(mode=="aftermath")g.TickRiverAftermath(dt);else call("TickTrafficMotion",new object[]{dt});};
    tick();check(v.riverFalling,label+" enters river while still airborne and carrying recovery/course flags");
    check(v.recovery==null,label+" cancels land recovery instead of standing on water after three seconds");
    float lowest=v.model.localPosition.y;bool splash=v.riverSplashed;bool rose=false;float last=lowest;
    for(float t=0;t<10&&v.active;t+=dt){tick();float y=v.model.localPosition.y;lowest=Mathf.Min(lowest,y);splash|=v.riverSplashed;if(last<RiverBlock.WaterY-.5f&&y>last+.03f)rose=true;last=y;}
    check(splash&&lowest<RiverBlock.WaterY-3,label+" splashes and sinks deep under the water");check(!rose,label+" never snaps back to road height while sinking");
    check(g.VehiclesFellInRiver==1,label+" river entry is counted once");
    data.Add(new{kind,mode,dt,lowest,splash,rose,active=v.active,y=v.model.localPosition.y});
   }
   if(!quick)
   {
    // A real traffic collision creates recovery before the water check.
    reset();var a=spawn("bike");var b=spawn("PV_MeshySport");
    a.previousPosition=new Vector3(2.5f,0,30);a.model.localPosition=new Vector3(4.8f,0,30);a.motionVelocity=Vector3.right*17;
    b.previousPosition=new Vector3(6.4f,0,30);b.model.localPosition=new Vector3(5.8f,0,30);b.motionVelocity=Vector3.left*5;
    call("ResolveVehicleCollisions",Array.Empty<object>());check(a.recovery!=null||b.recovery!=null,"Actual NPC-to-NPC swept contact creates collision recovery");
    call("TickTrafficMotion",new object[]{1f/60});check(a.riverFalling||b.riverFalling,"Actual NPC collision off the deck immediately enters sinking physics");
    reset();var hit=spawn("bike");g.rider.localPosition=new Vector3(4.9f,0,30);set("oldRiderZ",30f);
    hit.previousPosition=new Vector3(4.9f,0,33);hit.model.localPosition=new Vector3(4.9f,0,30.6f);hit.motionVelocity=Vector3.back*12;
    call("ResolveRiderContacts",new object[]{4.9f,1f/60});check(g.State==PhoVietGame.RideState.Falling&&hit.recovery!=null,"Actual player contact creates NPC recovery");
    call("TickTrafficMotion",new object[]{1f/60});check(hit.riverFalling,"NPC hit by player falls instead of recovering on water");
    // Old/side street frames stay physical after turning; lookup cannot be limited to blocks.
    reset();foreach(var tile in g.blocks)tile.GetComponent<RiverBlock>().SetBridge(false);
    var row=g.roadRows.First(r=>!r.tiles.Contains(g.blocks[0]));row.root.gameObject.SetActive(true);row.root.localPosition=Vector3.zero;row.root.localRotation=Quaternion.Euler(0,90,0);var side=row.tiles[0];side.localPosition=Vector3.zero;side.GetComponent<RiverBlock>().SetBridge(true);
    var rotated=spawn("bike");rotated.model.localPosition=rotated.previousPosition=g.transform.InverseTransformPoint(side.TransformPoint(new Vector3(4.9f,0,30)));
    call("ActivateCollisionBody",new object[]{rotated,Vector3.zero,Vector3.up*2,Vector2.right*500,Vector2.zero,false,true,8f});call("TickTrafficMotion",new object[]{1f/60});
    check(rotated.riverFalling&&rotated.riverTile==side,"A knocked vehicle on a rotated active branch falls into that branch river");
    reset();row=g.roadRows.First(r=>!r.tiles.Contains(g.blocks[0]));row.root.gameObject.SetActive(true);row.root.localPosition=new Vector3(-25,0,30);row.root.localRotation=Quaternion.Euler(0,90,0);side=row.tiles[0];side.localPosition=Vector3.zero;side.GetComponent<RiverBlock>().SetBridge(true);
    check(!g.OverRiver(new Vector3(5,0,30)),"A real overlapping deck takes priority over water below it");
    // End card can appear before the flung vehicle leaves the bridge.
    reset();var edge=spawn("PV_MeshyContainer");edge.model.localPosition=edge.previousPosition=new Vector3(3.1f,0,30);
    call("ActivateCollisionBody",new object[]{edge,Vector3.zero,new Vector3(8,2,0),Vector2.right*40000,Vector2.zero,false,true,12f});edge.knockVelocity=new Vector3(8,2,0);state(PhoVietGame.RideState.GameOver);
    for(int i=0;i<600&&edge.active;i++)g.TickRiverAftermath(1f/60);
    check(edge.model.localPosition.y<RiverBlock.WaterY-3||!edge.active,"A vehicle still above the deck at game-over continues its impact flight into the river");
    // Dry land must retain the existing three-second recovery.
    reset();var land=spawn("bike");land.model.localPosition=land.previousPosition=new Vector3(1.55f,0,65);
    call("ActivateCollisionBody",new object[]{land,Vector3.zero,new Vector3(1,0,0),Vector2.right*220,Vector2.zero,false,true,2f});
    for(int i=0;i<300;i++)call("TickTrafficMotion",new object[]{1f/60});check(!land.riverFalling&&!land.knocked,"Dry-land traffic recovery still runs normally");
    // Officer sinking explicitly ends pursuit, including a player already waiting after a crash.
    foreach(bool fallen in new[]{false,true})
    {
     reset();call("StartChase",Array.Empty<object>());
     if(fallen){g.rider.localPosition=new Vector3(1.55f,0,65);g.Crash();}
     g.policeBike.localPosition=new Vector3(4.9f,0,30);call("CheckPoliceRiver",new object[]{g.policeBike.localPosition,1f/60});
     var body=(PhoVietGame.TrafficVehicle)typeof(PhoVietGame).GetField("policeBody",F).GetValue(g);
     check(body.riverFalling&&!g.IsChased,"CSGT entering river ends pursuit; playerFallen="+fallen);
     for(int i=0;i<600;i++)
     {
      if(fallen&&g.State==PhoVietGame.RideState.Falling)call("TickRiderCrash",new object[]{1f/60});
      else if(g.State==PhoVietGame.RideState.GameOver)g.TickRiverAftermath(1f/60);
      else call("TickPoliceRiver",new object[]{1f/60,true});
     }
     check(g.policeBike.localPosition.y<RiverBlock.WaterY-3||!g.policeBike.gameObject.activeSelf,"CSGT continues sinking after pursuit ends; playerFallen="+fallen);
     check(fallen?g.State==PhoVietGame.RideState.GameOver:g.State==PhoVietGame.RideState.Riding,"Player result follows chosen CSGT river rule; playerFallen="+fallen);
    }
   }
   var report=new{success=failures.Count==0,count=checks.Count,failures,checks,cases=data};string file=quick?"Captures/river-crash-sinking-red.json":"Captures/river-crash-sinking-checks.json";File.WriteAllText(file,Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
