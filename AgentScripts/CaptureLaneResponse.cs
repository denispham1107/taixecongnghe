using System;using System.IO;using System.Reflection;using UnityEngine;using PhoViet;
public static class CaptureLaneResponse
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var car=g.rider.GetComponent<PlayerCarVisual>();
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;float time=Time.timeScale;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);set("junctionAt",float.PositiveInfinity);g.daylight.SetCycleTime(8);g.MoveLane(1);
   float t=0;foreach(int milliseconds in new[]{50,150,450,1600})
   {
    while(t<milliseconds/1000f-.0001f){typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{1f/60});car.Advance(1f/60);t+=1f/60;}
    if(g.State!=PhoVietGame.RideState.Riding)throw new Exception("Capture fixture crashed");
    typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});Shot(g.rideCamera,"Captures/lane-response-"+milliseconds+"ms.png");
   }
   return new{success=true,frames=4};
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=time;}
 }
 static void Shot(Camera camera,string path)
 {
  var old=camera.targetTexture;float aspect=camera.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,720,24);Texture2D texture=null;
  try{camera.aspect=1280f/720;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
  finally{camera.targetTexture=old;camera.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);}
 }
}
