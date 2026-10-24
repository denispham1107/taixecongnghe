using System;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
// Ảnh cầu vượt sông trong game (Play Mode, huongdan.md 21.35): góc chơi tiến tới cầu, trên cầu, nhìn ngang sông, đêm.
public static class CaptureRiverBridge
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled;
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  try
  {
   Time.timeScale=0;g.riversEnabled=true;g.RestartRide();g.daylight?.SetCycleTime(0);
   var blocks=g.blocks;
   foreach(var b in blocks){var rb=b.GetComponent<RiverBlock>();rb.SetBridge(Mathf.Abs(b.localPosition.z-80)<1);}
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   // Vài xe trên cầu để thấy tỷ lệ.
   g.SpawnTraffic(1,112);g.SpawnTraffic(1,135);
   typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,1000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);
   for(int i=0;i<8;i++)tick(1/60f);
   string Shot(string name){g.SendMessage("LateUpdate",SendMessageOptions.DontRequireReceiver);Capture(c,name);return name;}
   c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;
   // Đi tới gần đầu cầu (khối ở z 80: mặt sông từ 96,8).
   for(int i=0;i<60*3;i++){tick(1/60f);if(g.Distance>50)break;}
   var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);cam.Invoke(g,new object[]{true,0f});
   Shot("river-bridge-approach");
   for(int i=0;i<60*4;i++){tick(1/60f);if(g.Distance>98)break;}
   cam.Invoke(g,new object[]{true,0f});Shot("river-bridge-on");
   var tile=blocks.First(b=>b.GetComponent<RiverBlock>().IsBridge);
   c.transform.position=tile.TransformPoint(new Vector3(-42,7,24));c.transform.LookAt(tile.TransformPoint(new Vector3(0,-.5f,30)));c.fieldOfView=55;Shot("river-bridge-side");
   c.transform.position=tile.TransformPoint(new Vector3(2.4f,3.2f,24));c.transform.LookAt(tile.TransformPoint(new Vector3(-31,-1.8f,38)));c.fieldOfView=55;Shot("river-bridge-boats");
   g.daylight?.SetCycleTime(g.daylight.CycleSeconds*.5f);
   cam.Invoke(g,new object[]{true,0f});Shot("river-bridge-night");
   g.daylight?.SetCycleTime(0);
   return new{success=true,distance=g.Distance,bridges=blocks.Count(b=>b.GetComponent<RiverBlock>().IsBridge)};
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;Time.timeScale=scale;g.riversEnabled=rivers;g.ResetRide();}
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
