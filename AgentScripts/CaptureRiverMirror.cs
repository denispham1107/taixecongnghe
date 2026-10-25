using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Ảnh duyệt mặt sông kiểu gương (Play Mode, huongdan.md 21.45): cùng góc, trước = nước cũ, sau = shader River Water Mirror
// + RiverReflection gắn tạm lên camera. Không lưu gì vào scene; vật liệu và camera trả lại như cũ.
public static class CaptureRiverMirror
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  var swapped=new List<(Renderer r,Material[] mats)>();RiverReflection mirror=null;Material preview=null;var shots=new List<string>();
  try
  {
   Time.timeScale=0;g.riversEnabled=true;g.tankBarrageEnabled=false;g.RestartRide();g.daylight?.SetCycleTime(9);
   foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(Mathf.Abs(b.localPosition.z-80)<1);
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   g.SpawnTraffic(1,112);g.SpawnTraffic(2,128);
   typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,1000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);
   for(int i=0;i<60*3;i++){tick(1/60f);if(g.Distance>70)break;}
   var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);
   var tile=g.blocks.First(b=>b.GetComponent<RiverBlock>().IsBridge);
   // Các góc: góc chơi tới cầu, nhìn từ bờ như ảnh mẫu, sát mặt nước, đêm.
   var views=new (string name,Action set)[]
   {
    ("ride",()=>{cam.Invoke(g,new object[]{true,0f});}),
    ("bank",()=>{c.fieldOfView=60;c.transform.position=tile.TransformPoint(new Vector3(-36,2.4f,16.2f));c.transform.LookAt(tile.TransformPoint(new Vector3(-3,-4.2f,29)));}),
    ("side",()=>{c.fieldOfView=55;c.transform.position=tile.TransformPoint(new Vector3(-42,7,24));c.transform.LookAt(tile.TransformPoint(new Vector3(0,-.5f,30)));}),
    ("deck",()=>{c.fieldOfView=55;c.transform.position=tile.TransformPoint(new Vector3(3.2f,2.6f,22));c.transform.LookAt(tile.TransformPoint(new Vector3(24,-5,31)));}),
   };
   void Shot(string name){g.SendMessage("LateUpdate",SendMessageOptions.DontRequireReceiver);Capture(c,name);shots.Add(name);}
   void All(string tag)
   {
    foreach(var v in views){v.set();Shot("river-mirror-"+tag+"-"+v.name);}
   }
   // Chỉ dùng cho bản xem trước trước khi cài (21.45); sau 21.46 scene đã dùng nước gương.
   if(c.GetComponent<RiverReflection>()!=null)throw new Exception("Scene already uses the mirror water; use CaptureRiverBridge");
   All("before");
   // Bật nước gương: thay vật liệu nước cũ bằng bản xem trước, giữ thông số hình khối.
   var shader=Shader.Find("PhoViet/River Water Mirror");if(shader==null)throw new Exception("Mirror shader missing");
   foreach(var r in g.GetComponentsInChildren<Renderer>(true))
   {
    var mats=r.sharedMaterials;int k=Array.FindIndex(mats,m=>m!=null&&m.shader.name=="PhoViet/River Water");if(k<0)continue;
    if(preview==null){preview=new Material(shader){name="PV_RiverWaterMirror (preview)"};foreach(var p in new[]{"_Deck","_Banks","_Piers"})preview.SetVector(p,mats[k].GetVector(p));preview.SetFloat("_WaterY",mats[k].GetFloat("_WaterY"));preview.SetFloat("_ShadowStrength",mats[k].GetFloat("_ShadowStrength"));}
    swapped.Add((r,mats));var copy=(Material[])mats.Clone();copy[k]=preview;r.sharedMaterials=copy;
   }
   mirror=c.gameObject.AddComponent<RiverReflection>();mirror.game=g;
   All("after");
   // Ảnh phản chiếu thô (gỡ lỗi): góc nhìn từ bờ.
   views[2].set();Shot("river-mirror-debug-view");
   var tex=Shader.GetGlobalTexture("_RiverReflectionTex") as RenderTexture;
   if(tex!=null){var a2=RenderTexture.active;RenderTexture.active=tex;var t2=new Texture2D(tex.width,tex.height,TextureFormat.RGB24,false);t2.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0);t2.Apply();System.IO.File.WriteAllBytes("Captures/river-mirror-debug-reflection.png",t2.EncodeToPNG());RenderTexture.active=a2;UnityEngine.Object.DestroyImmediate(t2);}
   // Trên cầu (góc chơi): chạy tiếp tới giữa cầu, chụp nước mới rồi trả nước cũ chụp lại cùng khung hình.
   for(int i=0;i<60*4;i++){tick(1/60f);if(g.Distance>99)break;}
   void Bridge(string tag)
   {
    g.daylight?.SetCycleTime(9);cam.Invoke(g,new object[]{true,0f});Shot("river-mirror-"+tag+"-onbridge");
    g.daylight?.SetCycleTime(g.daylight.CycleSeconds*.5f);cam.Invoke(g,new object[]{true,0f});Shot("river-mirror-"+tag+"-night");
    g.daylight?.SetCycleTime(9);
   }
   Bridge("after");
   int count=swapped.Count;foreach(var (r,mats) in swapped)r.sharedMaterials=mats;swapped.Clear();UnityEngine.Object.DestroyImmediate(mirror);mirror=null;
   Bridge("before");
   return new{success=true,swapped=count,shots=shots.ToArray()};
  }
  finally
  {
   if(mirror!=null)UnityEngine.Object.DestroyImmediate(mirror);
   foreach(var (r,mats) in swapped)if(r!=null)r.sharedMaterials=mats;
   if(preview!=null)UnityEngine.Object.DestroyImmediate(preview);
   c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;Time.timeScale=scale;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.daylight?.SetCycleTime(0);g.ResetRide();
  }
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
