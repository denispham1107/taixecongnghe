using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using PhoViet;
// Ảnh trang chủ (21.134): xe người chơi đang bay, 2 trực thăng phía trước, 1 chiếc đã phóng tên lửa và tên lửa sắp tới xe.
// Render camera chơi ra 1920×1080 (cùng cỡ poster-home.png), không có HUD; chỉ đặt tình huống, không đổi code game.
public static class CaptureHomeFlight2134
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;
 static object Call(string name,params object[] args)=>typeof(PhoVietGame).GetMethod(name,F).Invoke(g,args);
 static void Set(string name,object v){var p=typeof(PhoVietGame).GetProperty(name,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(name,F).SetValue(g,v);}
 static object Actor(int i)=>((Array)typeof(PhoVietGame).GetField("helicopters",F).GetValue(g)).GetValue(i);
 static object Missile(int i)=>((Array)typeof(PhoVietGame).GetField("helicopterMissiles",F).GetValue(g)).GetValue(i);
 public static string EnergyIconPrefix="Năng lượng C";public static float HideIconsBefore=40f,MissileGap=5.5f,CycleTime=8f,HeliNear=27f,HeliFar=38f,HeliRise=.5f,Dt=1/60f,Fov=56,CamX=-.4f,CamUp=2.5f,CamBack=8.2f,LookX=-.4f,LookUp=.4f,LookAhead=16;public static int WarmFrames=40;public static int Lane=2;public static string Output="Captures/home-flight.png";
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;Time.timeScale=0;
  try
  {
   g.tankBarrageEnabled=false;g.RestartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(false);
   Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,Lane);var p=g.rider.localPosition;p.x=PhoVietGame.LaneX[Lane];g.rider.localPosition=p;
   Set("jumpFlying",true);Set("jumpLaunchVelocity",Mathf.Sqrt(48.4f*PhoVietGame.JumpHeightAtSpeed(17)));Set("jumpHeight",3.4f);Set("jumpPreviousHeight",3.4f);g.rider.localPosition+=Vector3.up*3.4f;g.TryJump();
   var car=g.rider.GetComponent<PlayerCarVisual>();var jets=g.rider.GetComponent<CarJumpJetVisual>();var systems=g.GetComponentsInChildren<ParticleSystem>(true);jets.Initialize();systems=g.GetComponentsInChildren<ParticleSystem>(true);
   // Bước game thật từng khung (thời gian đang dừng nên tự gọi hiệu ứng) để phản lực, khói và vệt lửa tích lũy như lúc chơi.
   Action step=()=>
   {
    Call("TickRide",Dt);Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}
    car.Advance(Dt);jets.Advance();foreach(var ps in systems)if(ps.gameObject.activeInHierarchy&&ps.isPlaying)ps.Simulate(Dt,false,false);
   };
   for(int i=0;i<WarmFrames;i++)step();
   if(!g.RiderFlying)throw new Exception("Not flying");
   Call("TickHelicopter",.001f,g.rider.localPosition,false);
   float y=g.rider.localPosition.y+g.PlayerHeight*.5f;
   g.GetHelicopterRoot(0).localPosition=new Vector3(PhoVietGame.LaneX[0],y+HeliRise,HeliNear);
   g.GetHelicopterRoot(1).localPosition=new Vector3(PhoVietGame.LaneX[3],y+HeliRise,HeliFar);
   for(int i=0;i<2;i++)Actor(i).GetType().GetField("shotRemaining").SetValue(Actor(i),100f);
   if(!(bool)Call("FireHelicopterMissile",Actor(0)))throw new Exception("Missile not fired");
   object m=null;foreach(var x in (Array)typeof(PhoVietGame).GetField("helicopterMissiles",F).GetValue(g))if(x!=null&&(bool)x.GetType().GetField("active").GetValue(x)){m=x;break;}
   var root=(Transform)m.GetType().GetField("root").GetValue(m);var fx=root.GetComponent<MissileFlightVisual>();
   int steps=0;while(root.localPosition.z-g.rider.localPosition.z>MissileGap&&steps++<400){step();fx.Advance(Dt);}
   if(!(bool)m.GetType().GetField("active").GetValue(m))throw new Exception("Missile hit before the shot");
   for(int i=0;i<2;i++)Actor(i).GetType().GetField("shotRemaining").SetValue(Actor(i),100f);
   g.daylight.SetCycleTime(CycleTime);Call("PositionCamera",true);g.daylight.UpdateLamps();
   var cc=g.rideCamera;var rp=g.rider.position;
   cc.fieldOfView=Fov;cc.transform.position=rp+new Vector3(CamX-g.rider.localPosition.x*.35f,CamUp,-CamBack);cc.transform.LookAt(rp+new Vector3(LookX-g.rider.localPosition.x,LookUp,LookAhead));
   // Vật phẩm luôn quay mặt về camera mỗi khung; thời gian đang dừng nên xoay tay ở đây.
   // Ẩn vật phẩm ở tiền cảnh (gần hơn tên lửa) để không che xe và tên lửa; chỉ trong ảnh này, ResetRide đặt lại.
   foreach(var t2 in g.GetComponentsInChildren<Transform>())if(t2.name.StartsWith("Nạp tên lửa",StringComparison.Ordinal)||t2.name.StartsWith(EnergyIconPrefix,StringComparison.Ordinal))
   {
    float ahead=g.transform.InverseTransformPoint(t2.position).z;if(ahead<HideIconsBefore){t2.gameObject.SetActive(false);continue;}
    var view=cc.transform.position-t2.position;view.y=0;if(view.sqrMagnitude>.01f)t2.rotation=Quaternion.LookRotation(view,Vector3.up);
   }
   var c=g.rideCamera;var old=c.targetTexture;var active=RenderTexture.active;
   var rt=new RenderTexture(1920,1080,24){antiAliasing=4};var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);
   c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1920,1080),0,0);t.Apply();File.WriteAllBytes(Output,t.EncodeToPNG());
   c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
   return new{success=true,steps,missile=root.localPosition.ToString(),rider=g.rider.localPosition.ToString(),heli0=g.GetHelicopterRoot(0).localPosition.ToString(),heli1=g.GetHelicopterRoot(1).localPosition.ToString(),flying=g.RiderFlying,state=g.State.ToString(),fired=g.HelicopterMissilesFired,jetParticles=jets==null?-1:jets.ParticleCount,jetEmitting=jets!=null&&jets.Emitting};
  }
  finally{Time.timeScale=scale;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.ResetRide();}
 }
}
