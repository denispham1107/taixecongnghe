using System;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
// Ảnh rơi xuống sông và chìm dần (Play Mode, huongdan.md 21.36): bắn nước, gợn sóng, chìm sâu sau thẻ kết thúc; mặt nước có bóng cầu.
public static class CaptureRiverSink
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled;
  Action<float> tick=dt=>{if(g.State==PhoVietGame.RideState.Riding)typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});else if(g.State==PhoVietGame.RideState.Falling)typeof(PhoVietGame).GetMethod("TickRiderCrash",F).Invoke(g,new object[]{dt});else g.TickRiverAftermath(dt);};
  var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);
  var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  var shots=new System.Collections.Generic.List<string>();
  Action<string> shot=n=>{Capture(c,n);shots.Add(n);};
  // Time.timeScale=0 khi chụp: tự mô phỏng hạt bọt nước/gợn sóng theo bước của game.
  Action<float> run=seconds=>{for(int i=0;i<Mathf.RoundToInt(seconds*60);i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});foreach(var b in g.blocks)foreach(var ps in b.GetComponentsInChildren<ParticleSystem>())ps.Simulate(1/60f,false,false,false);}};
  try
  {
   Time.timeScale=0;g.riversEnabled=true;g.trafficSurprises=false;g.RestartRide();g.daylight?.SetCycleTime(4);
   foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(Mathf.Abs(b.localPosition.z-80)<1);
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,1000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);
   g.StartRide();
   // Hai xe máy chạy ngược chiều phía trước để có xe rơi cùng lúc.
   var other=g.SpawnTraffic(1,150);
   for(int i=0;i<60*8&&g.Distance<80+RiverBlock.Near+7;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   cam.Invoke(g,new object[]{true,0f});shot("river-water-surface");
   if(other!=null&&other.active){other.changeFromX=other.model.localPosition.x;other.targetX=PhoVietGame.LaneX[0];other.changeProgress=0;other.changeDuration=.45f;other.changingLane=true;other.lane=0;}
   g.MoveLane(1);
   run(.55f);shot("river-sink-splash");
   run(.9f);shot("river-sink-going-under");
   run(1.6f);shot("river-sink-end-card");
   run(3f);shot("river-sink-deep");
   var tile=g.blocks.First(b=>b.GetComponent<RiverBlock>().IsBridge);
   g.daylight?.SetCycleTime(7);
   c.transform.position=tile.TransformPoint(new Vector3(12,1.2f,20));c.transform.LookAt(tile.TransformPoint(new Vector3(-2,-3.2f,30)));c.fieldOfView=58;shot("river-water-shadow");
   return new{success=true,shots=shots.ToArray(),riderY=g.rider.localPosition.y,state=g.State.ToString(),ripples=g.RiverRipples,bubbles=g.RiverBubbles};
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;Time.timeScale=scale;g.riversEnabled=rivers;g.trafficSurprises=true;g.daylight?.SetCycleTime(0);g.ResetRide();}
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
