using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class ProbeRouteRestart2118
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var cases=new List<object>();
  object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
  void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);}
  float Physical()=>g.blocks.Select(b=>b.localPosition.z+VietnamStreet.IntersectionCenter).Where(z=>z>=-1).Min();
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   foreach(int side in new[]{-1,1})
   {
    g.RestartRide();Clear();var p=g.rider.localPosition;var hit=g.SpawnTraffic(2,0,g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3));hit.speed=hit.baseTrafficSpeed=45;hit.motionVelocity=Vector3.forward*45;hit.previousPosition=p-Vector3.forward*12;hit.model.localPosition=p+Vector3.forward*12;hit.model.localRotation=Quaternion.identity;Set("Speed",0f);Set("oldRiderZ",p.z);Call("ResolveRiderContacts",p.x,.05f);Clear();int guard=0;
    while(g.State==PhoVietGame.RideState.Falling&&guard++<2000)Call("TickRiderCrash",.02f);
    float unscored=(float)typeof(PhoVietGame).GetField("crashTravelDistance",F).GetValue(g);g.RestartRide();Clear();float predicted=g.NextJunctionDistance,physical=Physical();
    g.MoveLane(side);if(side<0)g.MoveLane(side);guard=0;
    while(Physical()>20&&guard++<1000){Call("TickRide",.02f);Clear();}
    float atInput=g.NextJunctionDistance,visibleAtInput=Physical();g.MoveLane(side);int queued=g.QueuedTurn;guard=0;
    while(g.NextJunctionDistance>20&&g.State==PhoVietGame.RideState.Riding&&guard++<1000){Call("TickRide",.02f);Clear();}
    g.MoveLane(side);guard=0;while(g.Distance<150&&g.State==PhoVietGame.RideState.Riding&&guard++<1000){Call("TickRide",.02f);Clear();}
    if(side>0){g.daylight.SetCycleTime(6);Call("PositionCamera",true);Shot(g.rideCamera,"Captures/route-restart-2118-before.png");}
    cases.Add(new{side,unscored,predicted,physical,offset=predicted-physical,atInput,visibleAtInput,queued,completed=g.CompletedTurns,sceneryImpact=g.LastSceneryImpact,state=g.State.ToString(),distance=g.Distance,rows=g.roadRows.Select(r=>new{name=r.root.name,active=r.root.gameObject.activeSelf,p=new[]{r.root.localPosition.x,r.root.localPosition.z},yaw=r.root.localEulerAngles.y}).ToArray()});
   }
   var result=new{success=true,cases};File.WriteAllText("Captures/route-restart-2118-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.riversEnabled=flags[2];g.thunderstormEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();Time.timeScale=time;}
 }
 static void Shot(Camera c,string path)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,720,24);Texture2D t=null;
  try{c.aspect=1280f/720;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
