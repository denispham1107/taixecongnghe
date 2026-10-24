using System.Linq;
using UnityEngine;
using PhoViet;
// Ảnh so sánh phố Sài Gòn: góc chơi thật, góc cao giống ảnh mẫu và cận mặt tiền.
// Chỉ render camera vào file, không đổi scene đã lưu (camera được trả lại ngay).
public static class CaptureSaigonStreet
{
 public static object Before(){return Run("saigon-street-before");}
 public static object After(){return Run("saigon-street-after");}
 // Play Mode: ngày (60 s, sương đã tan) và đêm (30 s) để xem mây, đèn đường cong và tín hiệu.
 public static object DayNight()
 {
  if(!Application.isPlaying)throw new System.Exception("Play Mode required");
  var g=Object.FindAnyObjectByType<PhoVietGame>();var light=g.daylight;float keep=light.ElapsedTime;
  try{light.SetCycleTime(60);Run("saigon-street-day");light.SetCycleTime(30);Run("saigon-street-night");}
  finally{light.SetCycleTime(keep);}
  return new{success=true};
 }
 // Cận cảnh từng loại tiệm: camera đặt trên lòng đường, nhìn chéo vào vỉa hè trước tiệm.
 public static object Shops()
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;var shots=new System.Collections.Generic.List<string>();
  try
  {
   foreach(var name in VietnamStreet.Shops)
   {
    Transform shop=null;foreach(var t in g.roadRows[0].root.GetComponentsInChildren<Transform>(true))if(t.name=="Blender • "+name&&t.gameObject.activeInHierarchy){shop=t;break;}
    if(shop==null)continue;
    var front=shop.TransformPoint(new Vector3(0,1.3f,-4.4f));var eye=shop.TransformPoint(new Vector3(-3.2f,2.1f,-9.6f));
    c.transform.position=eye;c.transform.LookAt(front);c.fieldOfView=50;
    string file="saigon-street-shop-"+System.Array.IndexOf(VietnamStreet.Shops,name).ToString("00");Capture(c,file);shots.Add(file);
   }
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;}
  return new{success=true,shots};
 }
 // Cây đứng riêng trên nền trời để so với ảnh mẫu tree_reference.webp.
 public static object Tree()
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  try
  {
   Transform tree=null;foreach(var t in g.roadRows[0].tiles[1].Cast<Transform>())if(t.name.StartsWith("PV_StreetTree")&&t.localPosition.x>0){tree=t;break;}
   var basePos=tree.position;c.transform.position=basePos+new Vector3(-6.5f,3.6f,-3.5f);c.transform.LookAt(basePos+new Vector3(0,3.7f,0));c.fieldOfView=58;Capture(c,"saigon-street-tree-closeup");
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;}
  return new{success=true};
 }
 // Cận bảng tên dưới tủ kính (21.25): camera nhìn thẳng mặt trước tủ của từng tiệm có tủ.
 public static object Cabinets()
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;var shots=new System.Collections.Generic.List<string>();
  try
  {
   foreach(int i in new[]{0,2,4,7,8,9,11})
   {
    Transform shop=null;foreach(var t in g.roadRows[0].root.GetComponentsInChildren<Transform>(true))if(t.name=="Blender • "+VietnamStreet.Shops[i]&&t.gameObject.activeInHierarchy){shop=t;break;}
    if(shop==null)continue;float cx=(i==4||i==11)?2.5f:2.3f;
    c.transform.position=shop.TransformPoint(new Vector3(cx,.9f,-6.6f));c.transform.LookAt(shop.TransformPoint(new Vector3(cx,.7f,-4.4f)));c.fieldOfView=40;
    string file="saigon-street-cabinet-"+i.ToString("00");Capture(c,file);shots.Add(file);
   }
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;}
  return new{success=true,shots};
 }
 // Nhà cao tầng phía sau (21.27): góc cao trên trục đường như ảnh người dùng gửi, và cận mặt kính một tòa.
 public static object Towers()
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  try
  {
   c.transform.position=new Vector3(0,30,-14);c.transform.LookAt(new Vector3(0,8,60));c.fieldOfView=60;Capture(c,"saigon-street-towers-high");
   Transform shop=null;foreach(var t in g.roadRows[0].tiles[1].GetComponentsInChildren<Transform>(true))if(t.name=="Blender • "+VietnamStreet.Shops[7]){shop=t;break;}
   c.transform.position=shop.TransformPoint(new Vector3(-6,22,-14));c.transform.LookAt(shop.TransformPoint(new Vector3(0,22,6)));c.fieldOfView=55;Capture(c,"saigon-street-tower-front");
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;}
  return new{success=true};
 }
 // Góc chơi màn hình dọc thật (Game View 540×960) sau khi zoom gần hơn 10% (21.27).
 public static object Portrait()
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;
  var rt=new RenderTexture(540,960,24);var t=new Texture2D(540,960,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;
  c.aspect=540f/960f;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,540,960),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/saigon-street-portrait-zoom.png",t.EncodeToPNG());
  c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);
  return new{success=true,fov=c.fieldOfView};
 }
 static object Run(string prefix)
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;
  var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  try
  {
   Capture(c,prefix+"-game");
   // Góc nhìn trục đường từ trên cao như ảnh mẫu.
   c.transform.position=new Vector3(0,9.5f,-12);c.transform.LookAt(new Vector3(0,2.2f,48));c.fieldOfView=62;Capture(c,prefix+"-axis");
   c.transform.position=new Vector3(2,4.5f,-2);c.transform.LookAt(new Vector3(12.5f,4.6f,14));c.fieldOfView=58;Capture(c,prefix+"-facade-right");
   c.transform.position=new Vector3(-2,4.5f,-2);c.transform.LookAt(new Vector3(-12.5f,4.6f,14));c.fieldOfView=58;Capture(c,prefix+"-facade-left");
   c.transform.position=new Vector3(3.2f,1.7f,4);c.transform.LookAt(new Vector3(9.5f,1.6f,11));c.fieldOfView=55;Capture(c,prefix+"-sidewalk");
   c.transform.position=new Vector3(5.2f,2.0f,6.5f);c.transform.LookAt(new Vector3(9.3f,1.6f,12.4f));c.fieldOfView=55;Capture(c,prefix+"-storefront");
   c.transform.position=new Vector3(-5.2f,2.0f,30.5f);c.transform.LookAt(new Vector3(-9.3f,1.6f,37.6f));c.fieldOfView=55;Capture(c,prefix+"-storefront-left");
   c.transform.position=new Vector3(4.6f,6.8f,3.5f);c.transform.LookAt(new Vector3(8.2f,6.1f,10));c.fieldOfView=50;Capture(c,prefix+"-tree");
   c.transform.position=new Vector3(5.6f,1.9f,24);c.transform.LookAt(new Vector3(8.4f,1.2f,33));c.fieldOfView=55;Capture(c,prefix+"-people-removed");
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;}
  return new{success=true,prefix};
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1600,900,24);var t=new Texture2D(1600,900,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1600,900),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);
 }
}
