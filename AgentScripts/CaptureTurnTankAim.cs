using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
public static class CaptureTurnTankAim
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  bool dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,rivers=g.riversEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;float time=Time.timeScale;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);};
  bool weather=g.thunderstormEnabled;g.thunderstormEnabled=false;
  try
  {
   Time.timeScale=0;var evidence=new System.Collections.Generic.List<object>();
   foreach(int direction in new[]{-1,1})
   {
   g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();clear();
   int guard=0;while(g.NextJunctionDistance>24&&guard++<1000){tick(.02f);clear();}
   int lane=direction<0?0:3;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
   g.dinosaursEnabled=g.tankBarrageEnabled=true;set("dinosaurTimer",1000f);set("tankSpawnTimer",1000f);g.MoveLane(direction);guard=0;
   while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<500){tick(1f/60);if(g.CompletedTurns==0)clear();}
   if(g.CompletedTurns!=1||g.DinosaursSpawned!=1||g.TanksScheduled!=1||g.State!=PhoVietGame.RideState.Riding)throw new Exception("Simultaneous turn pair fixture failed");
   var aimingTank=g.vehicles.First(v=>v.active&&v.armored);set("turnTankAimRandom",new System.Random(1));typeof(PhoVietGame).GetMethod("TickTankFire",F).Invoke(g,new object[]{.401f});if(g.shells.Count!=1)throw new Exception("Expected automatic preferred shot");
   g.daylight.SetCycleTime(8);Shot(g,"Captures/turn-tank-aim-"+(direction<0?"left":"right")+".png");
   if(direction>0)Shot(g,"Captures/turn-tank-aim-area-close.png",true);
   var tank=g.vehicles.First(v=>v.active&&v.armored);var target=g.shells[0].target;var anchor=aimingTank.turnTankTarget;evidence.Add(new{direction,elapsedAfterTurn=0,target=new[]{target.x,target.y,target.z},anchor=new[]{anchor.x,anchor.y,anchor.z},dinosaurZ=g.dinosaurs.First(d=>d.active).root.localPosition.z,tankZ=tank.model.localPosition.z});
   }
   var report=new{success=true,evidence};File.WriteAllText("Captures/turn-tank-aim-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.thunderstormEnabled=weather;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.riversEnabled=rivers;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=time;}
 }
 static void Shot(PhoVietGame g,string path,bool detail=false)
 {
  typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});typeof(PhoVietGame).GetMethod("AnimateLights",F).Invoke(g,null);
  var camera=g.rideCamera;var old=camera.targetTexture;float aspect=camera.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,720,24);Texture2D texture=null;
  Vector3 oldPosition=camera.transform.position;Quaternion oldRotation=camera.transform.rotation;
  if(detail){var at=g.transform.TransformPoint(g.shells[0].target);camera.transform.position=at+g.transform.TransformDirection(new Vector3(-4,12,-14));camera.transform.LookAt(at+g.transform.forward*4);}
  try{camera.aspect=1280f/720;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
  finally{camera.transform.SetPositionAndRotation(oldPosition,oldRotation);camera.targetTexture=old;camera.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);}
 }
}
