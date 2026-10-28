using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using PhoViet;

// Render the current saved game's camera, without the in-game HUD. No scene save.
public static class CaptureLandingMorning
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  var camera=g.rideCamera;float time=Time.timeScale;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled;
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   int bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);
   int car=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3);
   g.SpawnTraffic(0,31,bike);g.SpawnTraffic(3,43,car);g.SpawnTraffic(1,64);
   // The game's authored 6-second morning fog, photographed at 1.5 seconds.
   g.daylight.SetCycleTime(1.5f);
   typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});
   Shot(camera,1920,1280,"Assets/WebGLTemplates/PhoVietPWA/poster-morning.jpg");
   Shot(camera,1280,900,"Assets/WebGLTemplates/PhoVietPWA/poster-morning-portrait.jpg");
   var report=new{success=true,cycleSeconds=g.daylight.ElapsedTime,fogStrength=g.daylight.morningFog.Strength,
    player=g.rider.GetComponent<PlayerCarVisual>()!=null,source="Current saved scene / actual game camera / no HUD",cameraPosition=new{x=camera.transform.position.x,y=camera.transform.position.y,z=camera.transform.position.z},
    landscape=new{width=1920,height=1280},portrait=new{width=1280,height=900}};
   File.WriteAllText("Captures/landing-morning-source.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.ResetRide();Time.timeScale=time;}
 }
 static void Shot(Camera camera,int width,int height,string path)
 {
  var old=camera.targetTexture;float aspect=camera.aspect;var active=RenderTexture.active;
  var rt=RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D texture=null;
  try{camera.aspect=(float)width/height;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToJPG(93));}
  finally{camera.targetTexture=old;camera.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);}
 }
}
