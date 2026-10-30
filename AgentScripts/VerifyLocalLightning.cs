using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyLocalLightning
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks;static float scale;static bool rivers,dinos,tanks,enabled;
 static void Check(bool b,string n){if(!b)throw new Exception(n);checks.Add(n);}
 static void Set(string n,object value){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,value);else typeof(PhoVietGame).GetField(n,F).SetValue(g,value);}
 static object Call(string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
 static void Fresh()
 {
  g.ResetRide();g.StartRide();g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=false;g.thunderstormEnabled=true;
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}g.policeBike.gameObject.SetActive(false);
 }
 static void Queue(int seed)
 {Set("StormTime",18f);Set("nextLightning",0f);Set("localStormRandom",new System.Random(seed));g.TickThunderstorm(.001f,true);}
 static Vector3[] Points()=>Enumerable.Range(0,g.AdditionalLightningCount).Select(i=>g.AdditionalLightningTarget(i)).ToArray();
 static void CheckTile(Transform tile,string label)
 {
  var lanes=new HashSet<int>();
  foreach(var point in Points())
  {
   var local=tile.InverseTransformPoint(g.transform.TransformPoint(point));int lane=Array.FindIndex(PhoVietGame.LaneX,x=>Mathf.Abs(x-local.x)<.001f);
   Check(lane>=0&&lanes.Add(lane),"Distinct valid lane in current block: "+label);
   Check(local.z>=3.999f&&local.z<=76.001f&&Mathf.Abs(local.y)<.001f,"Strike stays inside current street tile: "+label);
  }
  Check(lanes.Count==3,"Exactly three different additional lanes: "+label);
 }
 static int VisibleWarnings(ThunderstormVisual fx)=>fx.GetComponentsInChildren<MeshRenderer>(true).Count(r=>r.name.StartsWith("Cảnh báo sét")&&r.gameObject.activeInHierarchy);
 static int VisibleBolts(ThunderstormVisual fx)=>fx.GetComponentsInChildren<LineRenderer>(true).Count(r=>r.gameObject.activeInHierarchy)/5;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();
  scale=Time.timeScale;rivers=g.riversEnabled;dinos=g.dinosaursEnabled;tanks=g.tankBarrageEnabled;enabled=g.thunderstormEnabled;Time.timeScale=0;
  try
  {
   var skipped=new HashSet<int>();
   for(int seed=0;seed<48;seed++)
   {
    Fresh();var tile=g.blocks[1];var rng=new System.Random(g.RunSeed^0x514b27);int oldLane=rng.Next(4);float lead=Mathf.Max(12,g.Speed*.8f);
    var original=new Vector3(PhoVietGame.LaneX[oldLane],0,g.rider.localPosition.z+lead-3+(float)rng.NextDouble()*23);float next=2.1f+(float)rng.NextDouble()*.7f;
    Queue(seed);Check(g.LightningPending&&g.AdditionalLightningCount==3,"Automatic wave queues original plus three: "+seed);CheckTile(tile,"seed "+seed);
    Check(Vector3.Distance(g.LightningTarget,original)<.0001f,"Original random strike position preserved: "+seed);
    Check(Mathf.Abs((float)typeof(PhoVietGame).GetField("nextLightning",F).GetValue(g)-next)<.0001f,"Original random cadence preserved: "+seed);
    var lanes=Points().Select(p=>Array.FindIndex(PhoVietGame.LaneX,x=>Mathf.Abs(x-tile.InverseTransformPoint(g.transform.TransformPoint(p)).x)<.001f)).ToArray();skipped.Add(Enumerable.Range(0,4).Single(i=>!lanes.Contains(i)));
   }
   Check(skipped.Count==4,"Any of four lanes can be the unselected lane");
   foreach(int fps in new[]{20,30,60})
   {
    Fresh();Queue(fps);float elapsed=0,dt=1f/fps;while(g.LightningStrikes==0&&elapsed<1){g.TickThunderstorm(dt,false);elapsed+=dt;}
    Check(g.LightningStrikes==4,"Entire wave lands in one simulation step at "+fps+" FPS");
    Check(elapsed>=.8f-.00001f&&elapsed<=.8f+dt+.00001f,"Full warning lead time at "+fps+" FPS");
   }
   foreach(float z in new[]{.01f,20f,60f,79.99f})foreach(float speed in new[]{17f,40f,70f})
   {
    Fresh();var tile=g.blocks[1];g.rider.localPosition=g.transform.InverseTransformPoint(tile.TransformPoint(new Vector3(PhoVietGame.LaneX[2],0,z)));Set("Speed",speed);Queue((int)(z*10+speed));CheckTile(tile,"z="+z+" speed="+speed);
   }
   foreach(int side in new[]{-1,1})foreach(float yaw in new[]{30f,60f})
   {
    Fresh();Set("IsTurning",true);Set("TurnYaw",side*yaw);Set("turnSide",side);
    int row=(int)typeof(PhoVietGame).GetField(side<0?"leftRow":"rightRow",F).GetValue(g);
    var tile=yaw<45?g.blocks[1]:g.roadRows[row].tiles[0];g.rider.localPosition=g.transform.InverseTransformPoint(tile.TransformPoint(new Vector3(PhoVietGame.LaneX[2],0,8)));Queue(side+7);CheckTile(tile,"turn "+side+" yaw "+yaw);
   }
   Fresh();Queue(17);var points=Points();var primary=g.LightningTarget;Call("ScrollThunderstorm",5f);
   for(int i=0;i<3;i++)Check(Vector3.Distance(g.AdditionalLightningTarget(i),points[i]-Vector3.forward*5)<.0001f,"Extra warning follows road scroll "+i);
   foreach(int side in new[]{-1,1})
   {
    points=Points();var inverse=Quaternion.Euler(0,side*90,0);var origin=new Vector3(3,0,7);Call("RebaseThunderstorm",inverse,origin);
    for(int i=0;i<3;i++)Check(Vector3.Distance(g.AdditionalLightningTarget(i),inverse*(points[i]-origin))<.0001f,"Warning keeps announced point across rebase "+side+" "+i);
   }
   Fresh();Queue(21);points=Points();float time=g.StormTime,remaining=g.LightningWarningRemaining;g.TogglePause();g.TickThunderstorm(10,true);g.StrikeLightning(g.rider.localPosition);
   Check(g.StormTime==time&&g.LightningWarningRemaining==remaining&&g.AdditionalLightningCount==3&&g.LightningStrikes==0,"Pause freezes entire four-strike batch, including external damage calls");
   for(int i=0;i<3;i++)Check(g.AdditionalLightningTarget(i)==points[i],"Paused warning stays fixed "+i);g.TogglePause();
   var fx=g.GetComponent<ThunderstormVisual>();Call("RenderThunderstorm");Check(VisibleWarnings(fx)==4,"Four simultaneous warning circles rendered");
   var targets=new List<PhoVietGame.TrafficVehicle>();
   for(int i=0;i<3;i++)
   {
    int at=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth<.7f);var v=g.SpawnTraffic(2,40,at);Check(v!=null,"Traffic target available "+i);v.model.localPosition=points[i];v.model.localRotation=Quaternion.identity;targets.Add(v);
   }
   g.TickThunderstorm(.79f,false);Check(g.LightningStrikes==0&&g.AdditionalLightningCount==3,"All four warnings keep the full 0.8 second lead time");
   g.TickThunderstorm(.011f,false);Call("RenderThunderstorm");Check(g.LightningStrikes==4&&g.AdditionalLightningCount==0&&!g.LightningPending,"Exactly four rays resolve together, then clear queue");
   Check(targets.All(v=>v.lightningWreck&&v.knocked&&v.recovery==null),"All three local strikes damage traffic with existing permanent wreck rule");
   Check(VisibleWarnings(fx)==0&&VisibleBolts(fx)==4,"All four bolts remain visible simultaneously without overwriting one another");
   int lineCount=fx.GetComponentsInChildren<LineRenderer>(true).Length;Check(lineCount==20&&fx.LightningCapacity==4,"Fixed four-bolt pool, five lines each");
   g.TickThunderstorm(.13f,false);g.StrikeLightning(new Vector3(-5,0,25));g.TickThunderstorm(.11f,false);Call("RenderThunderstorm");Check(VisibleBolts(fx)==1,"New primary strike never revives expired additional bolts");
   foreach(float height in new[]{0f,4.1f})
   {
    Fresh();Queue(31);g.rider.localPosition=g.AdditionalLightningTarget(0)+Vector3.up*height;Set("IsChased",true);g.TickThunderstorm(.801f,false);
    Check(g.KilledByLightning&&g.State==PhoVietGame.RideState.Falling&&!g.CrashDuringPursuit&&!g.IsChased,"Additional ray ends player ride even during pursuit/in air: "+height);
    Check(g.LightningStrikes==4,"Remaining rays still resolve after player starts falling: "+height);
   }
   foreach(bool down in new[]{false,true})
   {
    Fresh();Queue(32);Set("IsChased",true);if(down)g.Crash();g.policeBike.gameObject.SetActive(true);g.policeBike.localPosition=g.AdditionalLightningTarget(0);g.TickThunderstorm(.801f,false);
    Check(g.PoliceLightningWreck&&!g.IsChased&&!g.CrashDuringPursuit,"Additional ray cancels pursuit and waiting-for-officer state: "+down);
    Check(g.LightningStrikes==4,"Officer hit never interrupts other rays in the same wave: "+down);
   }
   Fresh();Queue(5);g.WarnLightning(new Vector3(5,0,40));Check(g.AdditionalLightningCount==0,"Direct single warning replaces a pending batch without stale extras");g.TickThunderstorm(.801f,false);Check(g.LightningStrikes==1,"Manual single-strike API remains single");
   Fresh();Queue(6);g.thunderstormEnabled=false;g.TickThunderstorm(.1f,true);Call("RenderThunderstorm");Check(!g.LightningPending&&g.AdditionalLightningCount==0&&VisibleWarnings(fx)==0&&VisibleBolts(fx)==0,"Disabling weather cancels entire pending batch and visuals");
   Fresh();Queue(7);g.ResetRide();Call("RenderThunderstorm");Check(!g.LightningPending&&g.AdditionalLightningCount==0&&g.LightningStrikes==0&&VisibleWarnings(fx)==0&&VisibleBolts(fx)==0,"Retry clears all added warnings and bolts");
   Check(g.daylight.lightPool.Length==6&&fx.ParticleBudget==2400&&fx.GetComponentsInChildren<LineRenderer>(true).Length==lineCount,"Existing lights/rain budget unchanged and no bolt-pool growth");Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs stay locked");
   var result=new{success=true,count=checks.Count,checks,randomSeeds=48,streetPositions=12,turnCases=4,boltPool=lineCount};File.WriteAllText("Captures/rain-three-strikes-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.thunderstormEnabled=enabled;g.ResetRide();Time.timeScale=scale;}
 }
}
