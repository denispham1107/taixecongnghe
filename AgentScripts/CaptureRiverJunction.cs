using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Ảnh giao lộ sát cầu dài khi chạy tự nhiên (Play Mode, huongdan.md 21.48): giao lộ ngay sau cầu và trước cầu, nhìn dọc
// đường ngang về phía sông, nhìn từ trên; kèm số đo mép đường ngang/nhà đường ngang trong hệ khối cầu.
public static class CaptureRiverJunction
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled;var c=g.rideCamera;
  Action<float> tick=dt=>{typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,100000f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}};
  var cam=typeof(PhoVietGame).GetMethod("PositionRideCamera",F);var shots=new List<string>();var data=new List<string>();
  void Shot(string n){Capture(c,n);shots.Add(n);}
  try
  {
   Time.timeScale=0;g.riversEnabled=true;g.tankBarrageEnabled=false;g.RestartRide();g.daylight?.SetCycleTime(9);g.StartRide();
   Transform bridge=null;
   for(int i=0;i<60*150&&g.State==PhoVietGame.RideState.Riding;i++)
   {
    tick(1/60f);
    bridge=g.blocks.Where(b=>b.GetComponent<RiverBlock>().IsBridge&&b.localPosition.z+RiverBlock.Near>g.rider.localPosition.z+10).OrderBy(b=>b.localPosition.z).FirstOrDefault();
    if(bridge!=null&&bridge.localPosition.z-g.rider.localPosition.z<22)break;
   }
   if(bridge==null)throw new Exception("No bridge ahead");
   // Rider ở giao lộ TRƯỚC cầu (z khối cầu ≈ -10).
   cam.Invoke(g,new object[]{true,0f});Shot("river-junction-before");
   c.fieldOfView=60;c.transform.position=bridge.TransformPoint(new Vector3(-8,3,-10));c.transform.LookAt(bridge.TransformPoint(new Vector3(-40,0,2)));Shot("river-junction-before-left");
   c.transform.position=bridge.TransformPoint(new Vector3(0,45,-40));c.transform.LookAt(bridge.TransformPoint(new Vector3(0,-3,10)));Shot("river-junction-before-top");
   // Số đo: mọi renderer của khối KHÁC nằm gần dải z -20..10 và 50..80 của khối cầu, theo tên nhóm.
   foreach(var row in g.roadRows)foreach(var t in row.tiles)
   {
    if(t==bridge||(t.position-bridge.position).magnitude>200)continue;
    foreach(Transform ch in t)
    {
     var rs=ch.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;var bb=rs[0].bounds;foreach(var r in rs)bb.Encapsulate(r.bounds);
     float x0=1e9f,x1=-1e9f,z0=1e9f,z1=-1e9f;
     for(int k=0;k<4;k++){var p=bridge.InverseTransformPoint(new Vector3((k&1)==0?bb.min.x:bb.max.x,0,(k&2)==0?bb.min.z:bb.max.z));x0=Mathf.Min(x0,p.x);x1=Mathf.Max(x1,p.x);z0=Mathf.Min(z0,p.z);z1=Mathf.Max(z1,p.z);}
     if(x1<-60||x0>60)continue;
     if((z1>-25&&z0<12)||(z1>48&&z0<85))data.Add(t.name+" | "+ch.name+" | active="+ch.gameObject.activeInHierarchy+" x["+x0.ToString("F1")+","+x1.ToString("F1")+"] z["+z0.ToString("F1")+","+z1.ToString("F1")+"]");
    }
   }
   // Đi qua cầu tới giao lộ SAU cầu (z khối cầu ≈ 61).
   for(int i=0;i<60*10;i++){tick(1/60f);if(g.rider.localPosition.z>bridge.localPosition.z+61)break;}
   cam.Invoke(g,new object[]{true,0f});Shot("river-junction-after");
   c.fieldOfView=60;c.transform.position=bridge.TransformPoint(new Vector3(-8,3,70));c.transform.LookAt(bridge.TransformPoint(new Vector3(-40,0,58)));Shot("river-junction-after-left");
   c.transform.position=bridge.TransformPoint(new Vector3(0,45,110));c.transform.LookAt(bridge.TransformPoint(new Vector3(0,-3,50)));Shot("river-junction-after-top");
   return new{success=true,shots=shots.ToArray(),data=data.Distinct().ToArray()};
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
