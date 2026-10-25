using System;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
// Ảnh cầu vượt sông trong game (Play Mode, huongdan.md 21.35; cầu 50,4 m + nước gương 21.46): góc chơi tiến tới cầu, trên cầu, toàn cảnh, nhìn ngang sông, bờ sông, đêm.
public static class CaptureRiverBridge
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  try
  {
   Time.timeScale=0;g.riversEnabled=true;g.tankBarrageEnabled=false;g.RestartRide();g.daylight?.SetCycleTime(9);
   var blocks=g.blocks;
   foreach(var b in blocks){var rb=b.GetComponent<RiverBlock>();rb.SetBridge(Mathf.Abs(b.localPosition.z-80)<1);}
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   // Vài xe trên cầu để thấy tỷ lệ.
   g.SpawnTraffic(1,112);g.SpawnTraffic(1,135);
   typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,1000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);
   for(int i=0;i<8;i++)tick(1/60f);
   string Shot(string name){g.SendMessage("LateUpdate",SendMessageOptions.DontRequireReceiver);Capture(c,name);return name;}
   c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;
   // Đi tới gần đầu cầu (khối ở z 80: mặt sông 84,8–135,2).
   for(int i=0;i<60*3;i++){tick(1/60f);if(g.Distance>62)break;}
   var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);cam.Invoke(g,new object[]{true,0f});
   Shot("river-bridge-approach");
   for(int i=0;i<60*4;i++){tick(1/60f);if(g.Distance>104)break;}
   cam.Invoke(g,new object[]{true,0f});Shot("river-bridge-on");
   var tile=blocks.First(b=>b.GetComponent<RiverBlock>().IsBridge);
   c.transform.position=tile.TransformPoint(new Vector3(-14,40,-30));c.transform.LookAt(tile.TransformPoint(new Vector3(0,-3,32)));c.fieldOfView=58;Shot("river-bridge-overview");
   // Căn hộ ven sông bên kia cầu (21.49): nhìn từ mặt cầu và từ mặt nước.
   c.transform.position=tile.TransformPoint(new Vector3(-6,6,14));c.transform.LookAt(tile.TransformPoint(new Vector3(-26,12,56)));c.fieldOfView=60;Shot("river-bridge-apartments");
   c.transform.position=tile.TransformPoint(new Vector3(-30,7,22));c.transform.LookAt(tile.TransformPoint(new Vector3(-30,9,57)));c.fieldOfView=62;Shot("river-bridge-apartments-close");
   g.daylight?.SetCycleTime(g.daylight.CycleSeconds*.5f);
   c.transform.position=tile.TransformPoint(new Vector3(-6,6,14));c.transform.LookAt(tile.TransformPoint(new Vector3(-26,12,56)));c.fieldOfView=60;Shot("river-bridge-apartments-night");
   c.transform.position=tile.TransformPoint(new Vector3(-30,7,22));c.transform.LookAt(tile.TransformPoint(new Vector3(-30,9,57)));c.fieldOfView=62;Shot("river-bridge-apartments-night-close");
   g.daylight?.SetCycleTime(g.daylight.CycleSeconds*.3f);
   c.transform.position=tile.TransformPoint(new Vector3(-6,6,14));c.transform.LookAt(tile.TransformPoint(new Vector3(-26,12,56)));c.fieldOfView=60;Shot("river-bridge-apartments-dusk");
   g.daylight?.SetCycleTime(9);
   c.transform.position=tile.TransformPoint(new Vector3(-46,8,30));c.transform.LookAt(tile.TransformPoint(new Vector3(0,-1,30)));c.fieldOfView=58;Shot("river-bridge-side");
   c.transform.position=tile.TransformPoint(new Vector3(2.4f,3.2f,30));c.transform.LookAt(tile.TransformPoint(new Vector3(-31,-1.8f,48)));c.fieldOfView=55;Shot("river-bridge-boats");
   c.transform.position=tile.TransformPoint(new Vector3(-24,2.2f,1.2f));c.transform.LookAt(tile.TransformPoint(new Vector3(-2,-3,26)));c.fieldOfView=60;Shot("river-bridge-bank");
   c.transform.position=tile.TransformPoint(new Vector3(12,4.5f,62));c.transform.LookAt(tile.TransformPoint(new Vector3(6,0,52)));c.fieldOfView=60;Shot("river-bridge-far-bank");
   g.daylight?.SetCycleTime(g.daylight.CycleSeconds*.5f);
   cam.Invoke(g,new object[]{true,0f});Shot("river-bridge-night");
   g.daylight?.SetCycleTime(0);
   return new{success=true,distance=g.Distance,bridges=blocks.Count(b=>b.GetComponent<RiverBlock>().IsBridge)};
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;Time.timeScale=scale;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.daylight?.SetCycleTime(0);g.ResetRide();}
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
