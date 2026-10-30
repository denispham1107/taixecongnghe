using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
public static class CaptureHeadlightAngle
{
 public static object Main()=>Capture("after",true);
 static object Capture(string label,bool poses)
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,storm=g.thunderstormEnabled;
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=g.thunderstormEnabled=false;g.RestartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   int car=g.vehicles.FindIndex(v=>!v.eventOnly&&v.model.GetComponent<MeshyCarVariants>()!=null);
   var variants=g.vehicles[car].model.GetComponent<MeshyCarVariants>();variants.PrepareVariant(0,g.vehicles[car]);g.SpawnTraffic(0,38,car);
   int bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);g.SpawnTraffic(3,53,bike);
   var hero=g.rider.GetComponent<PlayerCarVisual>();hero.Advance(0);
   Action camera=()=>typeof(PhoVietGame).GetMethod("PositionCamera",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,new object[]{true});
   camera();g.daylight.SetCycleTime(30);g.daylight.UpdateLamps();
   // Same frozen scene and camera: original beam first, then only its opening changes.
   var savedBlock=new MaterialPropertyBlock();
   foreach(var lamp in g.daylight.vehicles)if(lamp.roadLight!=null){lamp.roadLight.GetPropertyBlock(savedBlock);savedBlock.SetFloat("_HeadlightWidth",1);lamp.roadLight.SetPropertyBlock(savedBlock);}
   for(int i=0;i<4;i++)g.daylight.lightPool[i].spotAngle=58;
   Shot(g.rideCamera,"Captures/headlight-angle-original.png",1920,1280);
   g.daylight.UpdateLamps();
   Shot(g.rideCamera,"Captures/headlight-angle-"+label+"-landscape.png",1920,1280);
   g.rideCamera.aspect=.5625f;camera();g.daylight.UpdateLamps();Shot(g.rideCamera,"Captures/headlight-angle-"+label+"-portrait.png",576,1024);
   if(poses)
   {
    g.rider.localRotation=Quaternion.Euler(0,-25,0);hero.visual.localRotation=Quaternion.Euler(1,0,2);g.rideCamera.aspect=1.5f;camera();g.daylight.UpdateLamps();
    Shot(g.rideCamera,"Captures/headlight-angle-after-turn.png",1920,1280);
    g.rider.localRotation=Quaternion.identity;g.rider.localPosition+=Vector3.up*3;hero.visual.localRotation=Quaternion.Euler(-4,0,0);camera();g.daylight.UpdateLamps();
    Shot(g.rideCamera,"Captures/headlight-angle-after-jump.png",1920,1280);
   }
   return new{success=true,label,night=g.daylight.NightLevel,images=poses?5:3,realLights=g.daylight.lightPool.Length,spotCookie=g.daylight.lightPool[0].cookie!=null?g.daylight.lightPool[0].cookie.name:"none"};
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.thunderstormEnabled=storm;g.ResetRide();Time.timeScale=scale;}
 }
 static void Shot(Camera c,string path,int w,int h)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D tex=null;
  try{c.aspect=(float)w/h;c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex=new Texture2D(w,h,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);}
 }
}
