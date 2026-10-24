using System;using System.Reflection;using UnityEngine;using PhoViet;
// Ảnh so sánh các kiểu ánh sáng buổi sáng có sương (Play Mode, huongdan.md 21.41): gốc + MorningLook.All,
// mỗi kiểu chụp lúc sương dày (0,6 s) và lúc sương đang tan (3,5 s), cùng góc chơi, cùng xe trên đường.
public static class CaptureMorningLooks
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  var d=g.daylight;int keep=d.morningLook;float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;
  var c=g.rideCamera;var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);var shots=new System.Collections.Generic.List<string>();
  try
  {
   Time.timeScale=0;g.riversEnabled=false;g.tankBarrageEnabled=false;
   for(int look=-1;look<MorningLook.All.Length;look++)
   {
    d.morningLook=look;g.RestartRide();
    foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    g.SpawnTraffic(2,42,3);g.SpawnTraffic(1,70,8);g.SpawnTraffic(0,95,12);
    foreach(float at in new[]{.6f,3.5f})
    {
     d.SetCycleTime(at);cam.Invoke(g,new object[]{true,0f});
     string name="morning-look-"+(look<0?"goc":((char)('a'+look)).ToString())+"-"+(at<1?"day-suong":"suong-tan");
     Capture(c,name);shots.Add(name);
    }
   }
   return new{success=true,shots=shots.ToArray()};
  }
  finally{d.morningLook=keep;Time.timeScale=scale;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.ResetRide();}
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
