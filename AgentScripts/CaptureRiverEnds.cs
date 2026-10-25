using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Ảnh hai đầu cầu khi chạy tự nhiên (Play Mode, huongdan.md 21.47): đường nhánh của giao lộ trước/sau ở đúng chỗ như lúc chơi,
// để soi nền đường thừa và nhà đường ngang nằm trên mặt sông.
public static class CaptureRiverEnds
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static bool debugMirror=false;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;var c=g.rideCamera;
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);var shots=new List<string>();var data=new List<string>();
  void Shot(string n){g.SendMessage("LateUpdate",SendMessageOptions.DontRequireReceiver);Capture(c,n);shots.Add(n);}
  try
  {
   Time.timeScale=0;g.riversEnabled=true;g.tankBarrageEnabled=false;g.RestartRide();g.daylight?.SetCycleTime(9);g.StartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,100000f);
   // Chạy tới khi khối cầu đầu tiên của đường chính còn cách 14 m.
   Transform bridge=null;
   for(int i=0;i<60*120&&g.State==PhoVietGame.RideState.Riding;i++)
   {
    tick(1/60f);typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,100000f);
    bridge=g.blocks.Where(b=>b.GetComponent<RiverBlock>().IsBridge&&b.localPosition.z+RiverBlock.Near>2).OrderBy(b=>b.localPosition.z).FirstOrDefault();
    if(bridge!=null&&bridge.localPosition.z+RiverBlock.Near-g.rider.localPosition.z<14)break;
   }
   if(bridge==null)throw new Exception("No bridge reached");
   data.Add("distance="+g.Distance.ToString("F1"));
   cam.Invoke(g,new object[]{true,0f});Shot("river-ends-near");
   for(int i=0;i<60*3;i++){tick(1/60f);typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,100000f);if(bridge.localPosition.z+RiverBlock.Near-g.rider.localPosition.z<1)break;}
   cam.Invoke(g,new object[]{true,0f});Shot("river-ends-entering");
   var mirror=c.GetComponent<RiverReflection>();
   if(debugMirror)
   {
    var tex=Shader.GetGlobalTexture("_RiverReflectionTex") as RenderTexture;
    if(tex!=null){var a2=RenderTexture.active;RenderTexture.active=tex;var t2=new Texture2D(tex.width,tex.height,TextureFormat.RGB24,false);t2.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0);t2.Apply();System.IO.File.WriteAllBytes("Captures/river-ends-debug-reflection.png",t2.EncodeToPNG());RenderTexture.active=a2;UnityEngine.Object.DestroyImmediate(t2);}
    mirror.enabled=false;Shot("river-ends-debug-nomirror");mirror.enabled=true;
   }
   for(int i=0;i<60*5;i++){tick(1/60f);typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,100000f);if(bridge.localPosition.z+RiverBlock.Far-g.rider.localPosition.z<20)break;}
   cam.Invoke(g,new object[]{true,0f});Shot("river-ends-far");
   c.fieldOfView=60;c.transform.position=bridge.TransformPoint(new Vector3(0,40,-22));c.transform.LookAt(bridge.TransformPoint(new Vector3(0,-4,30)));Shot("river-ends-top");
   return new{success=true,shots=shots.ToArray(),data=data.ToArray()};
  }
  finally{Time.timeScale=scale;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.daylight?.SetCycleTime(0);g.ResetRide();}
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
