using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyCraterTurns
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,F).Invoke(o,a);
 static void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  var checks=new List<string>();void Check(bool b,string t){if(!b)throw new Exception(t);checks.Add(t);}
  bool tank=g.tankBarrageEnabled,dino=g.dinosaursEnabled,river=g.riversEnabled,exits=g.turnExitTrafficEnabled;float time=Time.timeScale;
  void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}}
  void Fixture(int lane)
  {
   g.tankBarrageEnabled=false;g.dinosaursEnabled=false;g.riversEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();Clear();Set(g,"spawnTimer",10000f);Set(g,"nextIntersectionDistance",float.PositiveInfinity);
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);Call(g,"ClearLaneMotion");
  }
  void Terrain(float dt)=>Call(g,"UpdateTankCraterTerrain",dt);
  void Step(float dt){Call(g,"TickRide",dt);Terrain(dt);Clear();}
  try
  {
   Time.timeScale=0;
   foreach(int side in new[]{-1,1})foreach(float dt in new[]{1f/60,.05f})
   {
    Fixture(side<0?1:2);int guard=0;while(g.NextJunctionDistance>18&&guard++<400)Step(dt);
    g.MoveLane(side);Check(g.QueuedTurn==side||g.IsTurning,"Turn queued "+side+" / "+dt);
    var c=g.CreateTankCrater(new Vector3(g.rider.localPosition.x+1.4f,0,g.rider.localPosition.z+1));Terrain(.3f);Step(dt);
    Check(g.CraterSteeringActive&&g.CraterWheelHits==1,"Real wheel contact before corner "+side+" / "+dt);
    // Isolate coordinate rebasing from the player's deliberate lack of correction:
    // use a gentle drift so this fixture does not drive into buildings first.
    Set(g,"craterHeading",.35f);Set(g,"craterSideSpeed",.1f);
    int row=(int)g.GetType().GetField(side<0?"leftRow":"rightRow",F).GetValue(g);var anchor=g.roadRows[row].tiles[0];
    Vector3 previous=anchor.InverseTransformPoint(g.rider.position);float maxRatio=0;guard=0;
    while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<500)
    {
     float speed=g.Speed;Step(dt);var p=anchor.InverseTransformPoint(g.rider.position);maxRatio=Mathf.Max(maxRatio,Vector3.Distance(p,previous)/(Mathf.Max(speed,17)*dt));previous=p;
    }
    Check(g.CompletedTurns==1&&g.State==PhoVietGame.RideState.Riding,"Turn completes with active drift "+side+" / "+dt+" state="+g.State);
    Check(g.CraterSteeringActive&&g.CraterDriftOffset>.05f,"Turn never auto-corrects the crater heading "+side+" / "+dt);
    Check(Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[g.CurrentLane]-g.CraterDriftOffset)<.015f,"Offset retained in outgoing road frame "+side+" / "+dt);
    Check(maxRatio<1.12f,"No rebase teleport relative to outgoing road "+side+" / "+dt+" ratio="+maxRatio);
    Check(g.tankCraters.Contains(c),"Old-road crater survives turn "+side+" / "+dt);
    var masks=Shader.GetGlobalVectorArray("_PVCraters");int count=Shader.GetGlobalInt("_PVCraterCount");
    foreach(var hole in g.tankCraters.Where(x=>x.model.gameObject.activeInHierarchy))
    {var p=g.transform.InverseTransformPoint(hole.model.position);Check(masks.Take(count).Any(m=>new Vector2(m.x-p.x,m.y-p.z).magnitude<.001f),"Mask remains anchored after rebase");}
    var at=g.rider.localPosition;g.MoveLane(-1);Check(!g.CraterSteeringActive&&Vector3.Distance(at,g.rider.localPosition)<.00001f,"Manual correction after corner without snap");
    for(int i=0;i<Mathf.CeilToInt(1.6f/dt);i++)Step(dt);Check(Mathf.Abs(g.CraterDriftOffset)<.01f,"Corner drift settles only after input");
   }
   Fixture(2);var crater=g.CreateTankCrater(new Vector3(1.75f,0,10));Terrain(.3f);var pos=g.transform.position;var rot=g.transform.rotation;
   try
   {
    g.transform.SetPositionAndRotation(new Vector3(1000,0,-800),Quaternion.Euler(0,90,0));Terrain(0);var p=Shader.GetGlobalMatrix("_PVCraterWorldToGame").MultiplyPoint3x4(crater.model.position);var m=Shader.GetGlobalVectorArray("_PVCraters")[0];
    Check(new Vector2(p.x-m.x,p.z-m.y).magnitude<.001f,"World-to-game cutout matrix follows translated/rotated root");
   }finally{g.transform.SetPositionAndRotation(pos,rot);Terrain(0);}
   crater.owner.SetBridge(!crater.owner.IsBridge);Terrain(0);Check(g.ActiveCraterCount==0,"Recycled/replaced road cannot resurrect old damage");
   var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/crater-turn-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.tankBarrageEnabled=tank;g.dinosaursEnabled=dino;g.riversEnabled=river;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=time;}
 }
}
