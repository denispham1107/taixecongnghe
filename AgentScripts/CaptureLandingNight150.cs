using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
public static class CaptureLandingNight150
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled;
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   int car=g.vehicles.FindIndex(v=>!v.eventOnly&&v.model.GetComponent<MeshyCarVariants>()!=null);int bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);g.SpawnTraffic(0,38,car);g.SpawnTraffic(3,53,bike);
   g.rider.GetComponent<PlayerCarVisual>().Advance(0);typeof(PhoVietGame).GetMethod("PositionCamera",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,new object[]{true});g.daylight.SetCycleTime(30);g.daylight.UpdateLamps();
   var c=g.rideCamera;var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1920,1280,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D texture=null;
   try{c.aspect=1.5f;c.targetTexture=rt;c.Render();RenderTexture.active=rt;texture=new Texture2D(1920,1280,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1920,1280),0,0);texture.Apply();File.WriteAllBytes("Assets/WebGLTemplates/PhoVietPWA/poster-night.jpg",texture.EncodeToJPG(93));}
   finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);}
   var lamp=g.daylight.vehicles.Single(v=>v.root==g.rider);var p=g.rider.InverseTransformPoint(lamp.head.position);var result=new{success=true,night=g.daylight.NightLevel,source="Current game camera / no HUD / centered headlight",width=1920,height=1280,head=new[]{p.x,p.y,p.z}};File.WriteAllText("Captures/release150-night-source.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.ResetRide();Time.timeScale=scale;}
 }
}
