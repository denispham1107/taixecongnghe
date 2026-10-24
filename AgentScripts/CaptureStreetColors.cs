using System;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
// Ảnh kiểm tra cây đứng ở khe giữa hai mái hiên và mặt tiền nhiều màu (Play Mode, huongdan.md 21.43).
public static class CaptureStreetColors
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;
  var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);
  var shots=new System.Collections.Generic.List<string>();
  try
  {
   Time.timeScale=0;g.riversEnabled=false;g.tankBarrageEnabled=false;g.RestartRide();g.daylight?.SetCycleTime(9);
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   var tile=g.blocks.OrderBy(b=>Mathf.Abs(b.localPosition.z)).First();
   void Shot(string n){Capture(c,n);shots.Add(n);}
   cam.Invoke(g,new object[]{true,0f});Shot("street-colors-ride");
   c.fieldOfView=50;
   c.transform.position=tile.TransformPoint(new Vector3(-3.2f,5.2f,1));c.transform.LookAt(tile.TransformPoint(new Vector3(-8.6f,2.8f,13)));Shot("street-trees-left");
   c.transform.position=tile.TransformPoint(new Vector3(3.2f,5.2f,26));c.transform.LookAt(tile.TransformPoint(new Vector3(8.6f,2.8f,38)));Shot("street-trees-right");
   c.transform.position=tile.TransformPoint(new Vector3(0,9,-12));c.transform.LookAt(tile.TransformPoint(new Vector3(0,6,40)));c.fieldOfView=62;Shot("street-colors-wide");
   c.transform.position=tile.TransformPoint(new Vector3(6.6f,2.2f,46));c.transform.LookAt(tile.TransformPoint(new Vector3(8.4f,2.6f,50.2f)));c.fieldOfView=58;Shot("street-tree-gap-close");
   return new{success=true,shots=shots.ToArray()};
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
