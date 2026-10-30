using System;using System.IO;using System.Reflection;using System.Linq;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class CaptureThunderstorm
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Call(object g,string method,params object[] args)=>g.GetType().GetMethod(method,F).Invoke(g,args);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;bool tanks=g.tankBarrageEnabled,dinos=g.dinosaursEnabled,rivers=g.riversEnabled;var camera=g.rideCamera;var cp=camera.transform.position;var cr=camera.transform.rotation;float fov=camera.fieldOfView;var images=new List<string>();
  void SetWeather(float t){typeof(PhoVietGame).GetProperty("StormTime").SetValue(g,t);g.TickThunderstorm(.001f,true);Call(g,"RenderThunderstorm");g.daylight.SetCycleTime(9);}
  void Simulate(float t){foreach(var ps in g.GetComponent<ThunderstormVisual>().GetComponentsInChildren<ParticleSystem>(true))if(ps.gameObject.activeInHierarchy)ps.Simulate(t,false,false);}
  void Shot(string name,int w=1280,int h=850){string path="Captures/storm-"+name+".png";Save(camera,path,w,h);images.Add(path);}
  try
  {
   Time.timeScale=0;g.tankBarrageEnabled=g.dinosaursEnabled=g.riversEnabled=false;g.ResetRide();g.StartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   var car=g.vehicles.First(v=>v.model.GetComponent<MeshyCarVariants>()!=null&&!v.eventOnly);g.SpawnTraffic(2,24,g.vehicles.IndexOf(car));car.model.localRotation=Quaternion.identity;
   var bike=g.vehicles.First(v=>v.model.GetComponent<UrbanRiderMotion>()!=null&&!v.eventOnly);g.SpawnTraffic(0,39,g.vehicles.IndexOf(bike));bike.halfWidth=.43f;bike.halfLength=1.1f;
   Call(g,"PositionCamera",true);g.daylight.SetCycleTime(9);Call(g,"RenderThunderstorm");Shot("clear");
   SetWeather(15.75f);Simulate(.65f);Shot("light-rain");
   SetWeather(21);g.WarnLightning(car.model.localPosition);g.TickThunderstorm(.3f,false);Call(g,"RenderThunderstorm");Simulate(.65f);Shot("heavy-warning");
   g.StrikeLightning(car.model.localPosition);g.TickThunderstorm(.035f,false);Call(g,"RenderThunderstorm");Simulate(.4f);g.daylight.SetCycleTime(9);Shot("lightning-impact");
   for(int i=0;i<24;i++)Call(g,"TickCollisionBody",car,1/60f);
   var target=g.transform.TransformPoint(car.model.localPosition);camera.transform.position=target+new Vector3(-6,4,-8);camera.transform.LookAt(target+Vector3.up*1.3f);camera.fieldOfView=55;
   g.TickThunderstorm(.4f,false);Call(g,"RenderThunderstorm");Simulate(.5f);g.daylight.SetCycleTime(9);Shot("burning-car");
   Call(g,"PositionCamera",true);g.daylight.SetCycleTime(30);Shot("night-rain");
   camera.transform.position=g.rider.position+new Vector3(0,13,-21);camera.transform.LookAt(g.rider.position+Vector3.forward*20);camera.fieldOfView=65;Shot("portrait",500,1050);
   var fx=g.GetComponent<ThunderstormVisual>();var rain=fx.GetComponentsInChildren<ParticleSystem>(true).First(p=>p.name=="Hạt mưa nhỏ");
   var report=new{success=true,images=images.Count,files=images,rainParticles=rain.particleCount,burning=fx.BurningCount,source="Actual Unity rain particles, lightning branches/warning, baked fire/smoke and day-night lighting"};File.WriteAllText("Captures/storm-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.tankBarrageEnabled=tanks;g.dinosaursEnabled=dinos;g.riversEnabled=rivers;g.ResetRide();camera.transform.position=cp;camera.transform.rotation=cr;camera.fieldOfView=fov;Time.timeScale=time;}
 }
 static void Save(Camera c,string path,int w,int h)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.aspect=(float)w/h;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(w,h,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
