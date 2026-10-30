using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class CaptureTankCraters
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Call(object g,string n,params object[] a)=>g.GetType().GetMethod(n,F).Invoke(g,a);
 static void Set(object g,string n,object v)=>g.GetType().GetField(n,F).SetValue(g,v);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var camera=g.rideCamera;
  bool tanks=g.tankBarrageEnabled,dinos=g.dinosaursEnabled,rivers=g.riversEnabled;float time=Time.timeScale,fov=camera.fieldOfView;var pos=camera.transform.position;var rot=camera.transform.rotation;int images=0;
  var roadMaterials=g.scenery.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null&&m.shader.name=="PhoViet/Road With Tank Craters").Distinct().ToArray();var roadShaders=roadMaterials.Select(m=>m.shader).ToArray();
  void Terrain(float dt)=>Call(g,"UpdateTankCraterTerrain",dt);
  void Step(float dt){Call(g,"TickRide",dt);Terrain(dt);g.rider.GetComponent<PlayerCarVisual>().Advance(dt);}
  void Shot(string name){Save(camera,"Captures/crater-"+name+".png");images++;}
  void Fixture(){g.tankBarrageEnabled=false;g.dinosaursEnabled=false;g.riversEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set(g,"spawnTimer",10000f);Set(g,"nextIntersectionDistance",float.PositiveInfinity);g.daylight.SetCycleTime(9);}
  try
  {
   Time.timeScale=0;Fixture();var at=new Vector3(1.75f,0,12);var target=g.transform.TransformPoint(at);
   camera.transform.position=target+g.transform.TransformDirection(new Vector3(2,7,-6));camera.transform.LookAt(target+Vector3.down*.15f);camera.fieldOfView=48;
   foreach(var m in roadMaterials)m.shader=Shader.Find("Standard");Shot("road-standard-baseline");for(int i=0;i<roadMaterials.Length;i++)roadMaterials[i].shader=roadShaders[i];Shot("road-before");
   Call(g,"Detonate",new PhoVietGame.TankShell(),at,false);Terrain(.1f);
   foreach(var ps in g.GetComponentsInChildren<ParticleSystem>(true))if(ps.gameObject.activeInHierarchy)ps.Simulate(.12f,false,false);
   Shot("impact-debris");
   foreach(var ps in g.GetComponentsInChildren<ParticleSystem>(true))if(ps.gameObject.activeInHierarchy)ps.Simulate(.25f,false,false);
   Terrain(.15f);Shot("flying-asphalt");
   foreach(var ps in g.GetComponentsInChildren<ParticleSystem>(true))ps.Clear();Terrain(.3f);Shot("road-after");
   float a=camera.aspect;camera.aspect=1200f/900;
   var screen=camera.WorldToViewportPoint(target-Vector3.up*.35f);
   var corners=new List<float[]>();foreach(float x in new[]{-4.3f,4.3f})foreach(float z in new[]{-4.3f,4.3f}){var p=camera.WorldToViewportPoint(target+new Vector3(x,0,z));corners.Add(new[]{p.x*1200,(1-p.y)*900});}camera.aspect=a;
   File.WriteAllText("Captures/crater-render-regions.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{centre=new[]{screen.x*1200,(1-screen.y)*900},exclusion=corners,width=1200,height=900},Newtonsoft.Json.Formatting.Indented));
   camera.transform.position=target+new Vector3(-3,2.5f,-5);camera.transform.LookAt(target+Vector3.down*.12f);camera.fieldOfView=50;Shot("pit-depth");
   g.daylight.SetCycleTime(30);Shot("night");g.daylight.SetCycleTime(9);
   Fixture();g.CreateTankCrater(new Vector3(g.rider.localPosition.x+1.4f,0,10));Terrain(.3f);int n=0;
   while((g.CraterWheelHits==0||g.CraterDriftOffset<.35f)&&n++<150&&g.State==PhoVietGame.RideState.Riding)Step(1f/60);
   Call(g,"PositionCamera",true);Shot("wheel-hit-drift");var drift=new{offset=g.CraterDriftOffset,speed=g.CraterDriftSpeed,yaw=g.RiderSteerYaw,hits=g.CraterWheelHits,state=g.State.ToString()};
   g.MoveLane(-1);for(int i=0;i<70;i++)Step(1f/60);Call(g,"PositionCamera",true);Shot("countersteer-recovered");
   Fixture();g.CreateTankCrater(new Vector3(g.rider.localPosition.x+.5f,0,17));Terrain(.3f);g.TryJump();for(int i=0;i<43;i++)Step(1f/60);Call(g,"PositionCamera",true);Shot("jump-clearance");
   Fixture();g.riversEnabled=true;var tile=g.blocks[1];tile.GetComponent<RiverBlock>().SetBridge(true);var point=g.transform.InverseTransformPoint(tile.TransformPoint(new Vector3(1.75f,0,30)));g.CreateTankCrater(point);Terrain(.3f);
   target=g.transform.TransformPoint(point);camera.transform.position=target+new Vector3(-1,7,-7);camera.transform.LookAt(target);camera.fieldOfView=54;Shot("bridge-deck");
   var report=new{success=true,images,drift,source="Actual Unity shell detonation/road cutout/pit geometry/mesh debris and player physics"};File.WriteAllText("Captures/crater-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{for(int i=0;i<roadMaterials.Length;i++)roadMaterials[i].shader=roadShaders[i];g.tankBarrageEnabled=tanks;g.dinosaursEnabled=dinos;g.riversEnabled=rivers;g.ResetRide();camera.transform.position=pos;camera.transform.rotation=rot;camera.fieldOfView=fov;g.daylight.SetCycleTime(0);Time.timeScale=time;}
 }
 static void Save(Camera c,string path)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1200,900,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.aspect=1200f/900;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(1200,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1200,900),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
