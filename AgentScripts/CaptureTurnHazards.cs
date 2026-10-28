using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
public static class CaptureTurnHazards
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  bool dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,rivers=g.riversEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;float time=Time.timeScale;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);};
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();clear();
   int guard=0;while(g.NextJunctionDistance>24&&guard++<1000){tick(.02f);clear();}
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,3);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[3],0,0);
   g.dinosaursEnabled=g.tankBarrageEnabled=true;set("dinosaurTimer",1000f);set("tankSpawnTimer",1000f);g.MoveLane(1);guard=0;
   while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<500){tick(1f/60);if(g.CompletedTurns==0)clear();}
   if(g.CompletedTurns!=1||g.DinosaursSpawned!=1)throw new Exception("Turn fixture failed");
   g.daylight.SetCycleTime(8);Shot(g,"Captures/turn-hazards-dinosaur.png");
   float elapsed=0;bool jumped=false;while(elapsed<2.4f&&g.TanksScheduled==0&&g.State==PhoVietGame.RideState.Riding)
   {if(!jumped&&elapsed>=1.2f){g.TryJump();jumped=true;}tick(1f/60);elapsed+=1f/60;}
   if(g.TanksScheduled!=1||g.State!=PhoVietGame.RideState.Riding)throw new Exception("Following tank fixture failed: "+g.State);
   Shot(g,"Captures/turn-hazards-tank.png");
   var tank=g.vehicles.First(v=>v.active&&v.armored);var report=new{success=true,elapsed,dinosaurZ=g.dinosaurs.First(d=>d.active).root.localPosition.z,tankZ=tank.model.localPosition.z};
   File.WriteAllText("Captures/turn-hazards-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.riversEnabled=rivers;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=time;}
 }
 static void Shot(PhoVietGame g,string path)
 {
  typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});typeof(PhoVietGame).GetMethod("AnimateLights",F).Invoke(g,null);
  var camera=g.rideCamera;var old=camera.targetTexture;float aspect=camera.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,720,24);Texture2D texture=null;
  try{camera.aspect=1280f/720;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
  finally{camera.targetTexture=old;camera.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);}
 }
}
