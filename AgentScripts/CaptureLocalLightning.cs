using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class CaptureLocalLightning
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,enabled=g.thunderstormEnabled;var camera=g.rideCamera;var cp=camera.transform.position;var cr=camera.transform.rotation;float fov=camera.fieldOfView;var images=new List<string>();
  void Set(string name,object value){var p=typeof(PhoVietGame).GetProperty(name,F);if(p!=null)p.SetValue(g,value);else typeof(PhoVietGame).GetField(name,F).SetValue(g,value);}
  void Call(string name,params object[] args)=>typeof(PhoVietGame).GetMethod(name,F).Invoke(g,args);
  void Render(){Call("RenderThunderstorm");g.daylight.SetCycleTime(11);}
  void Shot(string label,int w=1280,int h=850){var file="Captures/rain-three-strikes-"+label+".png";Save(camera,file,w,h);images.Add(file);}
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=false;g.thunderstormEnabled=true;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   int primaryLane=new System.Random(g.RunSeed^0x514b27).Next(4),localSeed=0;
   while(new System.Random(localSeed).Next(4)!=primaryLane)localSeed++;
   Set("localStormRandom",new System.Random(localSeed));Set("StormTime",18f);Set("nextLightning",0f);g.TickThunderstorm(.001f,true);
   var targets=new[]{g.LightningTarget,g.AdditionalLightningTarget(0),g.AdditionalLightningTarget(1),g.AdditionalLightningTarget(2)};
   Call("PositionCamera",true);var landscapePosition=camera.transform.position;var landscapeRotation=camera.transform.rotation;float landscapeFov=camera.fieldOfView;
   g.TickThunderstorm(.25f,false);Render();var fx=g.GetComponent<ThunderstormVisual>();
   foreach(var ps in fx.GetComponentsInChildren<ParticleSystem>(true))if(ps.gameObject.activeInHierarchy)ps.Simulate(.7f,false,false);
   Shot("warnings");camera.transform.position=g.rider.position+new Vector3(0,13,-21);camera.transform.LookAt(g.rider.position+Vector3.forward*20);camera.fieldOfView=65;Shot("warnings-portrait",576,1024);
   g.TickThunderstorm(.56f,false);g.TickThunderstorm(.025f,false);Render();
   camera.transform.position=landscapePosition;camera.transform.rotation=landscapeRotation;camera.fieldOfView=landscapeFov;Shot("bolts");
   camera.transform.position=g.rider.position+new Vector3(0,13,-21);camera.transform.LookAt(g.rider.position+Vector3.forward*20);camera.fieldOfView=65;Shot("bolts-portrait",576,1024);
   int visible=fx.GetComponentsInChildren<LineRenderer>(true).Count(r=>r.gameObject.activeInHierarchy)/5;
   var report=new{success=g.LightningStrikes==4&&visible==4,images,targets=targets.Select(p=>new[]{p.x,p.y,p.z}).ToArray(),strikes=g.LightningStrikes,visibleBolts=visible,source="Automatic original plus 3 local strikes / actual Unity camera / fixed RNG example"};File.WriteAllText("Captures/rain-three-strikes-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.thunderstormEnabled=enabled;g.ResetRide();camera.transform.position=cp;camera.transform.rotation=cr;camera.fieldOfView=fov;Time.timeScale=scale;}
 }
 static void Save(Camera c,string file,int w,int h)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D tex=null;
  try{c.aspect=(float)w/h;c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex=new Texture2D(w,h,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(file,tex.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);}
 }
}
