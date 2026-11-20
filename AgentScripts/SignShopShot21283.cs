using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
// 21.283: tìm đoạn phố có 3 biển liền nhau Bún Bò Huế (11) – Thời Trang (12) – Tạp Hóa (5) như ảnh người dùng,
// chụp từ mặt đường (có 3D / giả lập gói điện thoại chỉ ảnh xa).
public static class SignShopShot21283
{
 public static object Main(string mode)
 {
  var map=GameObject.Find("BanDoMo");
  var signs=new List<(Transform t,int k)>();
  foreach(Transform s in map.transform.Find("BangHieu")){
   var src=PrefabUtility.GetCorrespondingObjectFromSource(s.gameObject); if(!src) continue;
   var m=Regex.Match(src.name,@"OW_Mau_Bang_\d_\d_(\d+)$"); if(!m.Success) continue;
   signs.Add((s,int.Parse(m.Groups[1].Value)));}
  var res=new List<string>();
  (Transform a,Transform b,Transform c) hit=(null,null,null);
  foreach(var a in signs.Where(x=>x.k==11)){
   var nb=signs.Where(x=>x.t!=a.t && (x.t.position-a.t.position).magnitude<25).ToList();
   var b=nb.FirstOrDefault(x=>x.k==12); if(!b.t) continue;
   var c=signs.FirstOrDefault(x=>x.k==5 && x.t!=b.t && (x.t.position-b.t.position).magnitude<15 && (x.t.position-a.t.position).magnitude>(b.t.position-a.t.position).magnitude);
   if(!c.t) continue; hit=(a.t,b.t,c.t); break;}
  if(!hit.a) return "không thấy";
  res.Add("A "+hit.a.position+" B "+hit.b.position+" C "+hit.c.position);
  // hướng ra đường: trục của biển
  var house=map.transform.Find("NhaPho").Cast<Transform>().OrderBy(h=>(h.position-hit.b.position).sqrMagnitude).First();
  res.Add("house "+house.name+" fwd "+house.forward+" right "+house.right+" up "+house.up);
  var hiddenList=new List<GameObject>();
  if(mode=="phone") foreach(var lg in map.GetComponentsInChildren<LODGroup>(false)) if(lg.name=="NoiThat"){var t3=lg.transform.Find("3D"); if(t3&&t3.gameObject.activeSelf){t3.gameObject.SetActive(false);hiddenList.Add(t3.gameObject);} lg.enabled=false;}
  try{
   var go=new GameObject("TmpCam"); var cam=go.AddComponent<Camera>();
   var src=Camera.main; if(src){cam.CopyFrom(src);} cam.fieldOfView=60; cam.farClipPlane=1500;
   Vector3 mid=hit.b.position; mid.y=2.2f;
   // tìm hướng ra đường: thử 4 hướng, chọn hướng tia không trúng nhà trong 12 m
   Vector3 best=-house.right;
   res.Add("out "+best);
   var side=Vector3.Cross(Vector3.up,best);
   go.transform.position=mid+best*14f+side*-6f+Vector3.up*2.5f;
   go.transform.LookAt(mid+side*3f);
   var rt=new RenderTexture(1600,740,24); cam.targetTexture=rt; cam.Render();
   RenderTexture.active=rt; var tex=new Texture2D(1600,740,TextureFormat.RGB24,false); tex.ReadPixels(new Rect(0,0,1600,740),0,0); tex.Apply();
   System.IO.File.WriteAllBytes(@"C:\Users\HP\AppData\Local\Temp\claude\78e7c772c3f4e7cd\81b886ae-2611-457f-9b66-bf2c4f68510c\scratchpad\shot_"+mode+".png",tex.EncodeToPNG());
   RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
  } finally {
   foreach(var g in hiddenList) g.SetActive(true);
   foreach(var lg in map.GetComponentsInChildren<LODGroup>(true)) if(lg.name=="NoiThat") lg.enabled=true;
  }
  return res.ToArray();
 }
}
