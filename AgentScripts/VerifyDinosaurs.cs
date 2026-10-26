using System;using System.IO;using System.Reflection;using System.Collections.Generic;using System.Linq;using UnityEngine;using PhoViet;
public static class VerifyDinosaurs
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  float scale=Time.timeScale;bool enabled=g.dinosaursEnabled,rivers=g.riversEnabled,tanks=g.tankBarrageEnabled,reckless=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();var failures=new List<string>();var evidence=new List<object>();
  Action<bool,string> check=(ok,s)=>{if(ok)checks.Add(s);else failures.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action reset=()=>{g.dinosaursEnabled=true;g.RestartRide();g.riversEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);g.rider.localPosition=new Vector3(30,0,0);((PhoVietGame.TrafficVehicle)typeof(PhoVietGame).GetField("riderBody",F).GetValue(g)).previousPosition=g.rider.localPosition;};
  Func<PhoVietGame.Dinosaur> actor=()=>{var d=g.SpawnDinosaur();d.root.localPosition=Vector3.zero;d.visual.ResetPose();call("CopyDinosaurTail",new object[]{d});return d;};
  Func<PhoVietGame.Dinosaur,Vector3,bool> touches=(d,p)=>{object[] args={d,p,Quaternion.identity,.33f,.91f,2.15f,Vector3.zero,false};return (bool)call("DinosaurTouches",args);};
  Action<PhoVietGame.Dinosaur,Vector3> placeRider=(d,p)=>{g.rider.localPosition=p;((PhoVietGame.TrafficVehicle)typeof(PhoVietGame).GetField("riderBody",F).GetValue(g)).previousPosition=p;set("riderRoadVelocity",Vector3.forward*17);call("CopyDinosaurTail",new object[]{d});};
  try
  {
   Time.timeScale=0;reset();check(g.dinosaurPrefab!=null&&enabled,"Dinosaur prefab installed/enabled without enabling jobs");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   var d=actor();check(d.visual.walk.legacy&&d.visual.sweep.legacy,"Real Blender Walk/TailSweep clips imported as legacy");
   check(Mathf.Abs(d.visual.walk.length-1.4f)<.01f&&Mathf.Abs(d.visual.sweep.length-2.8f)<.01f,"Walk and sweep clip durations preserved");
   check(d.visual.tail.Length==6&&d.visual.tail.All(t=>t!=null),"Collision tracks all six real tail joints");
   var skin=d.visual.GetComponentInChildren<SkinnedMeshRenderer>();check(skin.bones.Length>=18&&skin.sharedMesh.boneWeights.Any(w=>w.weight1>0)&&Enumerable.Range(0,5).All(i=>skin.bones.Any(t=>t.name=="Tail"+i)),"Skin has a smooth multiweight rig and the complete deforming tail chain");
   var baked=new Mesh();skin.BakeMesh(baked);check(baked.vertexCount==skin.sharedMesh.vertexCount&&baked.vertices.All(p=>!float.IsNaN(p.x)&&!float.IsInfinity(p.x)&&!float.IsNaN(p.y)&&!float.IsInfinity(p.y)),"Sampled mesh keeps every vertex and finite deformations");UnityEngine.Object.DestroyImmediate(baked);
   var raw=skin.sharedMesh.bounds;var b=new Bounds(d.root.InverseTransformPoint(skin.transform.TransformPoint(raw.min)),Vector3.zero);
   for(int c=0;c<8;c++)b.Encapsulate(d.root.InverseTransformPoint(skin.transform.TransformPoint(raw.center+Vector3.Scale(raw.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1)))));
   check(Mathf.Abs(b.size.x-4.65f)<.03f,"Rest mesh is 1.5 lanes wide: "+b.size.x);
   check(Mathf.Abs(b.size.y-6.0811f)<.03f&&Mathf.Abs(b.size.z-8.6758f)<.03f,"Source height and length keep their proportions: "+b.size);
   Vector3 foot=d.visual.leftFoot.position;d.visual.Advance(.35f);check(Vector3.Distance(foot,d.visual.leftFoot.position)>.15f,"Walk visibly moves the planted/lifted feet");
   d.visual.ResetPose();Vector3 start=d.visual.tail[5].position;d.visual.Advance(1.4f);check(Vector3.Distance(start,d.visual.tail[5].position)<.04f,"Walk loop closes without a tail snap");
   float[] laneTimes=new float[4];Vector3[] lanePoints=new Vector3[4];float[] nearest=Enumerable.Repeat(float.MaxValue,4).ToArray();float minX=100,maxX=-100,minY=100,maxY=-100,maxStep=0;Vector3 old=Vector3.zero;
   d.visual.ResetPose();d.visual.StartSweep();check(!d.visual.TailDangerous,"Windup warns before the damaging sweep");
   for(int frame=1;frame<=168;frame++)
   {
    d.visual.Advance(1f/60);Vector3 tip=g.transform.InverseTransformPoint(d.visual.tail[5].position);
    if(d.visual.TailDangerous)
    {
     minX=Mathf.Min(minX,tip.x);maxX=Mathf.Max(maxX,tip.x);minY=Mathf.Min(minY,tip.y);maxY=Mathf.Max(maxY,tip.y);
     if(old!=Vector3.zero)maxStep=Mathf.Max(maxStep,Vector3.Distance(old,tip));old=tip;
     for(int lane=0;lane<4;lane++)if(Mathf.Abs(tip.x-PhoVietGame.LaneX[lane])<nearest[lane]){nearest[lane]=Mathf.Abs(tip.x-PhoVietGame.LaneX[lane]);laneTimes[lane]=d.visual.SweepAge;lanePoints[lane]=new Vector3(PhoVietGame.LaneX[lane],0,tip.z);}
    }
   }
   check(minX< -6.2f&&maxX>6.2f,"Visible distal tail sweeps both road edges: "+minX+".."+maxX);
   check(minY>.3f&&maxY<1.15f,"Distal tail really sweeps low enough to jump: "+minY+".."+maxY);
   check(maxStep<1.2f,"Sweep accelerates continuously without a teleport: max 60fps step="+maxStep);
   for(int lane=0;lane<4;lane++)
   {
    d.visual.ResetPose();d.visual.StartSweep();d.visual.Advance(laneTimes[lane]);call("CopyDinosaurTail",new object[]{d});
    check(nearest[lane]<.60f,"Real tail reaches lane "+lane+" within the true tip radius plus rider half-width");
    check(touches(d,lanePoints[lane]),"Grounded rider in lane "+lane+" is physically hit by tail");
    check(!touches(d,lanePoints[lane]+Vector3.up*2.6f),"A high-enough jump clears the real tail in lane "+lane);
    check(!touches(d,lanePoints[lane]+Vector3.forward*22),"No invisible whole-road/remote hit in lane "+lane);
   }
   evidence.Add(new{minX,maxX,minY,maxY,maxStep,laneTimes,lanePoints=lanePoints.Select(p=>new{p.x,p.y,p.z}).ToArray()});
   foreach(float dt in new[]{1f/60,1f/30,.05f})
   {
    reset();for(float t=0;t<9.9f;t+=dt)call("TickDinosaurs",new object[]{dt,true});check(g.DinosaursSpawned==0,"No spawn before ten seconds at dt="+dt);
    for(int i=0;i<Mathf.CeilToInt(.2f/dt);i++)call("TickDinosaurs",new object[]{dt,true});check(g.DinosaursSpawned==1,"First spawn at ten seconds at dt="+dt);
    for(int i=0;i<Mathf.CeilToInt(10f/dt);i++)call("TickDinosaurs",new object[]{dt,true});check(g.DinosaursSpawned==2,"Second spawn ten seconds later at dt="+dt);
   }
   reset();d=actor();d.root.localPosition=new Vector3(0,0,80);Vector3 initial=d.root.localPosition;
   call("ScrollDinosaurs",new object[]{1.7f});call("TickDinosaurs",new object[]{.1f,false});float own=(initial.z-d.root.localPosition.z)-1.7f;
   check(Mathf.Abs(own-PhoVietGame.DinosaurWalkSpeed*.1f)<.01f&&d.root.forward.z<0,"Dinosaur walks toward player at real 3m/s independently of road scrolling");
   initial=d.root.localPosition;call("ScrollDinosaurs",new object[]{3.825f});call("TickDinosaurs",new object[]{.1f,false});check(Mathf.Abs((initial.z-d.root.localPosition.z)-3.825f-own)<.01f,"Player boost does not boost the dinosaur");
   var inverse=Quaternion.Euler(0,-90,0);Vector3 origin=new Vector3(3,0,20),position=d.root.localPosition,heading=d.root.localRotation*Vector3.forward;
   call("RebaseDinosaurs",new object[]{inverse,origin});check(Vector3.Distance(d.root.localPosition,inverse*(position-origin))<.001f&&Vector3.Distance(d.root.localRotation*Vector3.forward,inverse*heading)<.001f,"Junction rebase preserves position and original walking direction");
   float clock=d.visual.Clock;var pausedTip=d.visual.tail[5].position;g.TogglePause();g.SendMessage("Update");check(g.State==PhoVietGame.RideState.Paused&&clock==d.visual.Clock&&Vector3.Distance(pausedTip,d.visual.tail[5].position)<.001f,"Pause freezes dinosaur timer/pose");g.TogglePause();
   foreach(bool chase in new[]{false,true})
   {
    reset();d=actor();d.visual.StartSweep();d.visual.Advance(laneTimes[0]);placeRider(d,lanePoints[0]);if(chase)call("StartChase",Array.Empty<object>());
    call("ResolveDinosaurHits",new object[]{d,1f});
    check(g.State==PhoVietGame.RideState.Falling&&g.HitByDinosaur,"Real tail starts the existing falling flow; chase="+chase);
    check(g.CrashDuringPursuit==chase,"Existing police-wait vs ordinary game-over rule is retained; chase="+chase);
    check(g.RiderFallVelocity.y>=5&&g.RiderFallVelocity.magnitude>10,"Tail physically throws the rider; chase="+chase);
    for(int i=0;i<2100&&g.State==PhoVietGame.RideState.Falling;i++)call("TickRiderCrash",new object[]{1f/60});
    check(g.State==(chase?PhoVietGame.RideState.Arrested:PhoVietGame.RideState.GameOver),"Fallen rider reaches the correct end state with dinosaur still present; chase="+chase+",state="+g.State+",settled="+g.RiderFallComplete+",rider="+g.rider.localPosition+",police="+g.policeBike.localPosition+",fallen="+g.PoliceFallen+",pursuit="+g.CrashDuringPursuit);
    if(chase){call("TickArrest",new object[]{1.2f});check(g.State==PhoVietGame.RideState.Jailed,"Police still completes jail transition after dinosaur collision");check(g.dinosaurs.All(x=>!x.active),"Street dinosaurs are hidden from the jail scene");}
   }
   foreach(bool truck in new[]{false,true})
   {
    reset();d=actor();d.visual.StartSweep();d.visual.Advance(laneTimes[0]);call("CopyDinosaurTail",new object[]{d});
    var v=truck?g.SpawnReckless(true,true):g.SpawnTraffic(2,30,g.vehicles.FindIndex(x=>x.halfWidth>.7f&&!x.eventOnly));
    check(v!=null,"Car/drowsy-truck fixture spawned; truck="+truck);if(v==null)continue;
    v.model.localPosition=lanePoints[0];v.previousPosition=v.model.localPosition;call("ResolveDinosaurHits",new object[]{d,1f});
    check(v.knocked&&v.physicalCrash&&v.knockVelocity.y>=5,"Tail physically throws car/drowsy truck; truck="+truck);
   }
   reset();d=actor();d.visual.StartSweep();d.visual.Advance(laneTimes[3]);placeRider(d,lanePoints[3]+Vector3.up*2.6f);call("ResolveDinosaurHits",new object[]{d,1f});check(g.State==PhoVietGame.RideState.Riding&&!g.HitByDinosaur,"A high actual rider position survives ResolveDinosaurHits");
   foreach(bool armor in new[]{false,true})
   {
    reset();d=actor();d.visual.StartSweep();d.visual.Advance(laneTimes[0]);call("CopyDinosaurTail",new object[]{d});
    var v=armor?g.SpawnScheduledTank():g.SpawnTraffic(2,30,g.vehicles.FindIndex(x=>x.halfWidth<.7f&&!x.eventOnly));
    check(v!=null&&(!armor||v.armored),"Traffic fixture spawned; armored="+armor);if(v==null)continue;
    v.model.localPosition=lanePoints[0];v.previousPosition=v.model.localPosition;
    call("ResolveDinosaurHits",new object[]{d,1f});check(v.knocked&&v.physicalCrash&&v.knockVelocity.y>=5,"Tail throws "+(armor?"armored tank":"motorcycle"));
    int count=g.DinosaurTailHits;call("ResolveDinosaurHits",new object[]{d,1f});check(g.DinosaurTailHits==count,"One impulse per vehicle per sweep; armored="+armor);
   }
   reset();d=actor();d.visual.StartSweep();d.visual.Advance(laneTimes[0]);call("CopyDinosaurTail",new object[]{d});call("StartChase",Array.Empty<object>());g.policeBike.localPosition=lanePoints[0];
   call("ResolveDinosaurHits",new object[]{d,1f});check(g.PoliceFallen&&Mathf.Abs(g.PoliceFallRemaining-1.2f)<.001f,"Tail hits CSGT with the unchanged 1.2-second recovery");call("TickDinosaurPolicePush",new object[]{.1f});check(g.policeBike.localPosition.y>.1f,"CSGT has a physical hop, not a position teleport");
   g.dinosaursEnabled=false;call("TickDinosaurs",new object[]{.01f,false});check(g.dinosaurs.All(v=>!v.active),"Disabled feature retires its pool");g.ResetRide();check(g.DinosaursSpawned==0&&g.DinosaurCountdown==10&&g.dinosaurs.All(v=>!v.active),"Restart resets schedule, counters and pool");
   var result=new{success=failures.Count==0,count=checks.Count,failures,checks,evidence};File.WriteAllText("Captures/dinosaur-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally
  {
   g.dinosaursEnabled=enabled;g.riversEnabled=rivers;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;
  }
 }
}
