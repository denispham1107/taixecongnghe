using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
// Real current-scene renders. Pose/time/actors are staged only in Play Mode; no scene save.
public static class CaptureLanding2115
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()=>Capture(true);
 public static object ActionOnly()=>Capture(false);
 static object Capture(bool morning)
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;
  bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.riversEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  void Clear(){foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}}
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   g.RestartRide();Clear();g.rider.GetComponent<PlayerCarVisual>().Advance(0);Call("PositionCamera",true);Call("AnimateLights");g.daylight.SetCycleTime(1.5f);
   if(morning){Shot(g.rideCamera,1920,1080,"Assets/WebGLTemplates/PhoVietPWA/poster-morning.jpg");Shot(g.rideCamera,1280,720,"Assets/WebGLTemplates/PhoVietPWA/poster-morning-portrait.jpg");}
   g.dinosaursEnabled=g.tankBarrageEnabled=true;
   var d=g.SpawnDinosaur();d.root.localPosition=new Vector3(2.2f,0,28);d.root.localRotation=Quaternion.Euler(0,180,0);d.visual.Advance(.22f);
   var tank=g.SpawnScheduledTank();tank.model.localPosition=new Vector3(-2.4f,0,25);tank.previousPosition=tank.model.localPosition;tank.model.localRotation=Quaternion.Euler(0,160,0);
   typeof(PhoVietGame).GetProperty("Speed").SetValue(g,8f);var shell=g.FireTankShell(tank,3);if(shell==null)throw new Exception("Tank did not fire");
   // Let the real muzzle flash and its smoke expire before the shell lands.
   foreach(var ps in g.GetComponentsInChildren<ParticleSystem>())if(ps.particleCount>0)ps.Simulate(shell.flight+.05f,true,false,true);
   Call("TickShells",shell.flight);
   // Photograph the real ground detonation while keeping the dinosaur's approved pose frozen.
   foreach(var ps in g.GetComponentsInChildren<ParticleSystem>())if(ps.particleCount>0)ps.Simulate(.12f,true,false,true);
   var muzzle=(ParticleSystem)typeof(PhoVietGame).GetField("muzzleFx",F).GetValue(g);
   var blast=(ParticleSystem)typeof(PhoVietGame).GetField("blastFx",F).GetValue(g);
   if(g.ShellBlasts!=1||g.shells.Count!=0||muzzle.particleCount!=0||blast.particleCount==0)throw new Exception("Expected ground explosion without muzzle flash");
   g.daylight.SetCycleTime(6);g.daylight.UpdateLamps();
   Shot(g.rideCamera,1920,1080,"Assets/WebGLTemplates/PhoVietPWA/poster-action.jpg");
   var report=new{success=true,source="Current Unity scene, actual game camera, real ground detonation; original tank/dinosaur pose unchanged",morningReplaced=morning,action=new{width=1920,height=1080,night=g.daylight.NightLevel,dinosaurYaw=d.root.localEulerAngles.y,dinosaur=new[]{d.root.localPosition.x,d.root.localPosition.z},tank=new[]{tank.model.localPosition.x,tank.model.localPosition.z},shells=g.shells.Count,blasts=g.ShellBlasts,muzzleParticles=muzzle.particleCount,blastParticles=blast.particleCount,groundTarget=new[]{shell.target.x,shell.target.y,shell.target.z}}};
   File.WriteAllText("Captures/landing-2116-source.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.riversEnabled=flags[2];g.thunderstormEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.ResetRide();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();Time.timeScale=time;}
 }
 static void Shot(Camera c,int w,int h,string path)
 {
  var old=c.targetTexture;float aspect=c.aspect,fov=c.fieldOfView;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.fieldOfView=fov*1.1f;c.aspect=(float)w/h;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(w,h,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();string staged=path+".capture";File.WriteAllBytes(staged,t.EncodeToJPG(95));if(File.Exists(path))File.Replace(staged,path,null);else File.Move(staged,path);}
  finally{c.targetTexture=old;c.aspect=aspect;c.fieldOfView=fov;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
