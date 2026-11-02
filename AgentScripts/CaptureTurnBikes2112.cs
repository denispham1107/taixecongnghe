using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class CaptureTurnBikes2112
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var evidence=new List<object>();
  try
  {
   Time.timeScale=0;
   foreach(int direction in new[]{-1,1})
   {
    g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.ResetRide();g.StartRide();Clear();
    int guard=0;while(g.NextJunctionDistance>24&&guard++<1000){Call("TickRide",.02f);Clear();}
    int lane=direction<0?0:3;Set("CurrentLane",lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);g.turnExitTrafficEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=true;Set("dinosaurTimer",1000f);Set("tankSpawnTimer",1000f);g.MoveLane(direction);guard=0;
    while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<1000){Call("TickRide",1f/60);if(g.CompletedTurns==0)Clear();}
    if(g.TurnOncomingBikeSpawns!=1||g.TurnRearBikeSpawns!=1)throw new Exception("Real turn did not queue motorcycle pair");
    int warned=-1;float initialWarning=g.TurnBikeRearWarningRemaining(2);
    var pair=g.vehicles.Where(v=>v.active&&v.turnBikeGeneration==v.spawnGeneration).ToArray();if(pair.Length!=2)throw new Exception("Expected both motorcycles");
    foreach(var v in pair){v.model.GetComponent<UrbanRiderMotion>()?.Initialize();v.turnBikeSwerveTimer=0;var before=v.model.localPosition;Call("TickLongitudinalVehicle",v,.2f);v.motionVelocity=(v.model.localPosition-before)/.2f+Vector3.forward*g.Speed;v.model.GetComponent<UrbanRiderMotion>()?.Advance(.2f,false);}
    g.daylight.SetCycleTime(8);Shot("Captures/turn-bikes-2112-"+(direction<0?"left":"right")+".png",true);
    if(direction>0)Shot("Captures/turn-bikes-2112-front-close.png",false,pair.First(v=>!v.turnBikeFromRear).model);
    evidence.Add(new{direction,initialWarning,warningLane=warned,playerSpeed=g.Speed,tankZ=g.vehicles.Single(v=>v.active&&v.armored).model.localPosition.z,dinosaurZ=g.dinosaurs.Single(d=>d.active).root.localPosition.z,bikes=pair.Select(v=>new{rear=v.turnBikeFromRear,speed=v.speed,lane=v.lane,position=new[]{v.model.localPosition.x,v.model.localPosition.y,v.model.localPosition.z},changing=v.changingLane}).ToArray()});
   }
   var result=new{success=true,evidence};File.WriteAllText("Captures/turn-bikes-2112-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.riversEnabled=flags[2];g.thunderstormEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();Time.timeScale=time;}
 }
 static void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);}
 static void Shot(string path,bool overview,Transform close=null)
 {
  Call("PositionCamera",true);Call("AnimateLights");var camera=g.rideCamera;var position=camera.transform.position;var rotation=camera.transform.rotation;float aspect=camera.aspect;var old=camera.targetTexture;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,850,24);Texture2D image=null;
  try
  {
   if(overview){camera.transform.position=g.transform.TransformPoint(new Vector3(0,12,-25));camera.transform.LookAt(g.transform.TransformPoint(new Vector3(0,0,12)));}
   if(close!=null){camera.transform.position=close.position+g.transform.TransformDirection(new Vector3(4,2.5f,6));camera.transform.LookAt(close.position+g.transform.up);}
   camera.aspect=1280f/850;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image=new Texture2D(1280,850,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,850),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
  }
  finally{camera.transform.SetPositionAndRotation(position,rotation);camera.aspect=aspect;camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(image!=null)UnityEngine.Object.DestroyImmediate(image);}
 }
}
