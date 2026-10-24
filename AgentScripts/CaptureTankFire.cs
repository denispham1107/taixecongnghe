using System;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
// Ảnh xe tăng bắn đạn (Play Mode, huongdan.md 21.38): lửa đầu nòng, đạn bay + vòng cảnh báo, nổ trượt làn bên, nổ trúng người chơi.
public static class CaptureTankFire
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;
  var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);
  Action<float> tick=dt=>{if(g.State==PhoVietGame.RideState.Riding)typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});else if(g.State==PhoVietGame.RideState.Falling)typeof(PhoVietGame).GetMethod("TickRiderCrash",F).Invoke(g,new object[]{dt});
   var fx=g.transform.Find("Hiệu ứng đạn xe tăng • Blender");if(fx!=null)foreach(var ps in fx.GetComponentsInChildren<ParticleSystem>())ps.Simulate(dt,false,false,false);
   cam.Invoke(g,new object[]{false,dt});};
  Action<float> run=s=>{for(int i=0;i<Mathf.RoundToInt(s*60);i++)tick(1/60f);};
  var shots=new System.Collections.Generic.List<string>();var notes=new System.Collections.Generic.List<string>();
  try
  {
   Time.timeScale=0;g.riversEnabled=false;g.tankBarrageEnabled=false;g.RestartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,1000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);
   g.StartRide();
   var tank=g.SpawnScheduledTank();if(tank==null)throw new Exception("No tank slot");
   for(int i=0;i<600&&tank.model.localPosition.z>78;i++)tick(1/60f);
   g.FireTankShell(tank,3);run(.05f);
   cam.Invoke(g,new object[]{true,0f});Capture(c,"tank-fire-muzzle");shots.Add("tank-fire-muzzle");
   var muzzle=g.transform.TransformPoint(g.TankMuzzle(tank));
   var m=g.TankMuzzle(tank);c.transform.position=g.transform.TransformPoint(new Vector3(0,3.2f,m.z-11));c.transform.LookAt(muzzle);c.fieldOfView=52;
   Capture(c,"tank-fire-muzzle-close");shots.Add("tank-fire-muzzle-close");
   run(.6f);g.FireTankShell(tank,0);run(.25f);cam.Invoke(g,new object[]{true,0f});
   Capture(c,"tank-fire-flight");shots.Add("tank-fire-flight");notes.Add("shellsInFlight="+g.shells.Count);
   run(.8f);Capture(c,"tank-fire-blast-miss");shots.Add("tank-fire-blast-miss");notes.Add("afterMiss state="+g.State+" blasts="+g.ShellBlasts);
   run(.35f);Capture(c,"tank-fire-blast-miss-2");shots.Add("tank-fire-blast-miss-2");
   // Phát trúng làn người chơi.
   var second=g.SpawnScheduledTank();if(second==null)throw new Exception("No second tank");
   for(int i=0;i<600&&second.model.localPosition.z>95;i++)tick(1/60f);
   int lane=g.CurrentLane;var hit=g.FireTankShell(second,lane);notes.Add("hitShell="+(hit!=null));run(1.0f);
   Capture(c,"tank-fire-incoming");shots.Add("tank-fire-incoming");
   run(.75f);Capture(c,"tank-fire-hit");shots.Add("tank-fire-hit");notes.Add("hit state="+g.State+" killed="+g.KilledByShell);
   run(1.2f);Capture(c,"tank-fire-hit-after");shots.Add("tank-fire-hit-after");
   return new{success=true,shots=shots.ToArray(),notes=notes.ToArray(),fired=g.ShellsFired,blasts=g.ShellBlasts};
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;Time.timeScale=scale;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.ResetRide();}
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
