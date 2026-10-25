using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Ảnh lúc té/rơi sông ở cầu dài (Play Mode, huongdan.md 21.52): chạy tự nhiên tới cầu, (a) đi làn ngoài rơi xuống sông
// và chìm, (b) té trên đường ngay trước cầu rồi trượt; chụp camera chơi trong lúc té và ở thẻ kết thúc.
public static class CaptureRiverCrash
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;var c=g.rideCamera;
  Action<float> tick=dt=>{if(g.State==PhoVietGame.RideState.Riding)typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});else if(g.State==PhoVietGame.RideState.Falling)typeof(PhoVietGame).GetMethod("TickRiderCrash",F).Invoke(g,new object[]{dt});else if(g.State==PhoVietGame.RideState.GameOver)g.TickRiverAftermath(dt);
   typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,100000f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}};
  var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);var shots=new List<string>();
  void Shot(string n){cam.Invoke(g,new object[]{false,1/60f});Capture(c,n);shots.Add(n);}
  Transform Ride(float within)
  {
   g.RestartRide();g.StartRide();Transform b=null;
   for(int i=0;i<60*150&&g.State==PhoVietGame.RideState.Riding;i++)
   {
    tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});
    b=g.blocks.Where(t=>t.GetComponent<RiverBlock>().IsBridge&&t.localPosition.z+RiverBlock.Near>g.rider.localPosition.z).OrderBy(t=>t.localPosition.z).FirstOrDefault();
    if(b!=null&&b.localPosition.z+RiverBlock.Near-g.rider.localPosition.z<within)return b;
   }
   return b;
  }
  try
  {
   Time.timeScale=0;g.riversEnabled=true;g.tankBarrageEnabled=false;g.daylight?.SetCycleTime(g.daylight.CycleSeconds*.5f);
   // (a) Rơi sông từ làn ngoài.
   Ride(12);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,3);var p=g.rider.localPosition;p.x=PhoVietGame.LaneX[3];g.rider.localPosition=p;
   for(int i=0;i<60*3&&g.State==PhoVietGame.RideState.Riding;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   for(int i=0;i<30;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   Shot("river-crash-fall");
   for(int i=0;i<60*4;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   Shot("river-crash-sunk");
   // (c) Rơi ở cuối cầu (gần bờ xa), camera nhìn về kè và dãy nhà bờ xa như ảnh người dùng.
   var far=Ride(12);float farEnd=far.localPosition.z+RiverBlock.Far;string farInfo="start gap="+(farEnd-g.rider.localPosition.z).ToString("F1")+" state="+g.State;
   for(int i=0;i<60*8&&g.State==PhoVietGame.RideState.Riding;i++)
   {
    tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});
    if(far.localPosition.z+RiverBlock.Far-g.rider.localPosition.z<9){typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,3);var q=g.rider.localPosition;q.x=PhoVietGame.LaneX[3];g.rider.localPosition=q;break;}
   }
   for(int i=0;i<60*3&&g.State==PhoVietGame.RideState.Riding;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   farInfo+=" end state="+g.State+" inRiver="+g.RiderInRiver+" gap="+(far.localPosition.z+RiverBlock.Far-g.rider.localPosition.z).ToString("F1");
   for(int i=0;i<40;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   Shot("river-crash-far-fall");
   for(int i=0;i<60*4;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   Shot("river-crash-far-sunk");
   // (b) Té trên đường ngay trước cầu, trượt về phía cầu.
   Ride(18);g.Crash();typeof(PhoVietGame).GetField("riderFallVelocity",F).SetValue(g,new Vector3(0,0,12));
   for(int i=0;i<40;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   Shot("river-crash-road");
   for(int i=0;i<60*4;i++){tick(1/60f);cam.Invoke(g,new object[]{false,1/60f});}
   Shot("river-crash-road-end");
   return new{success=true,shots=shots.ToArray(),hidden=g.BuildingsHiddenOverRiver,farInfo};
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
