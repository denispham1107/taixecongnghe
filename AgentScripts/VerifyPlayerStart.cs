using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;

public static class VerifyPlayerStart
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var car=g.rider.GetComponent<PlayerCarVisual>();
  var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  var startField=typeof(PhoVietGame).GetField("playerStart",F);Vector3 saved=g.PlayerStartPosition;
  var frame=g.transform.localToWorldMatrix*Matrix4x4.Translate(Vector3.back*saved.z);
  float time=Time.timeScale;
  bool river=g.riversEnabled,dino=g.dinosaursEnabled,tank=g.tankBarrageEnabled,reckless=g.recklessTrafficEnabled,surprise=g.trafficSurprises,exit=g.turnExitTrafficEnabled;
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,100000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);};
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   check(Vector3.Distance(car.visual.localPosition,Vector3.zero)<.001f,"Saved FBX offset is transferred to the player body at startup");
   foreach(var p in new[]{new Vector3(1.55f,0,4.39f),new Vector3(-4.65f,0,24),new Vector3(4.65f,0,145),new Vector3(-1.55f,0,-24),new Vector3(1.55f,2,8),new Vector3(1.55f,0,0)})
   {
    startField.SetValue(g,p);g.ResetRide();car.Advance(0);var expected=frame.MultiplyPoint3x4(p);
    check(Vector3.Distance(g.rider.position,expected)<.002f,"Authored xyz start retained: "+p);
    check(Vector3.Distance(car.visual.position,g.rider.position)<.002f,"Visible model and collision body coincide: "+p);
    check(g.Distance==0&&g.State==PhoVietGame.RideState.Ready,"New start begins at zero metres: "+p);
    check(Mathf.Abs(g.NextJunctionDistance-(70-(p.z-Mathf.Floor((p.z+10)/80)*80)))<.002f,"Junction distance agrees with road geometry: "+p);
    var branch=g.roadRows.First(r=>r!=g.roadRows[0]&&r.root.gameObject.activeSelf);
    check(Mathf.Abs(branch.root.localPosition.z-g.NextJunctionDistance)<.002f,"Cross-street houses move with the actual junction: "+p);
    for(int i=0;i<3;i++){g.ResetRide();car.Advance(0);}
    check(Vector3.Distance(g.rider.position,expected)<.002f,"Repeated reset never accumulates the start offset: "+p);
    check(!g.JobsAvailable&&!g.jobsEnabled,"Jobs remain locked: "+p);
    clear();g.StartRide();for(int i=0;i<10;i++)tick(.02f);
    check(Mathf.Abs(g.rider.localPosition.z)<.001f&&g.State==PhoVietGame.RideState.Riding,"Starting gameplay does not snap the frame back: "+p);
    if(p.y>0){for(int i=0;i<80;i++)tick(.02f);check(Mathf.Abs(g.RiderJumpHeight)<.002f,"Elevated start lands on the real road");}
   }
   foreach(float z in new[]{4.39f,145f,-24f})foreach(int direction in new[]{-1,1})
   {
    startField.SetValue(g,new Vector3(PhoVietGame.LaneX[direction<0?0:3],0,z));g.ResetRide();g.StartRide();clear();
    int guard=0;while(g.NextJunctionDistance>24&&guard++<500)tick(.02f);
    typeof(PhoVietGame).GetMethod("SelectJunctionTurn",F).Invoke(g,new object[]{direction});
    while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<900){clear();tick(.02f);}
    check(g.CompletedTurns==1&&g.State==PhoVietGame.RideState.Riding,"Turn succeeds after custom spawn z="+z+", direction="+direction);
    check(Mathf.Abs(g.rider.localPosition.z)<.002f,"Turn rebase preserves the simulation frame");
   }
   foreach(float z in new[]{4.39f,145f})
   {
    startField.SetValue(g,new Vector3(1.55f,0,z));g.riversEnabled=true;g.ResetRide();
    foreach(var row in g.roadRows)foreach(var tile in row.tiles)tile.GetComponent<RiverBlock>().SetBridge(false);
    var bridge=g.blocks[2];bridge.GetComponent<RiverBlock>().SetBridge(true);
    var outside=g.transform.InverseTransformPoint(bridge.TransformPoint(new Vector3(4.65f,0,30)));
    var deck=g.transform.InverseTransformPoint(bridge.TransformPoint(new Vector3(1.55f,0,30)));
    check(g.OverRiver(outside)&&!g.OverRiver(deck),"River collision uses the same rebased frame at start z="+z);
    clear();g.StartRide();for(int i=0;i<400&&g.Distance<150&&g.State==PhoVietGame.RideState.Riding;i++)tick(.025f);
    check(g.State==PhoVietGame.RideState.Riding&&!g.RiderInRiver&&g.Distance>=150,"Custom start crosses the bridge safely at z="+z);
   }
   var report=new{passed=checks.Count,checks};File.WriteAllText("Captures/player-start-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally
  {
   startField.SetValue(g,saved);g.riversEnabled=river;g.dinosaursEnabled=dino;g.tankBarrageEnabled=tank;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprise;g.turnExitTrafficEnabled=exit;g.ResetRide();Time.timeScale=time;
  }
 }
}
