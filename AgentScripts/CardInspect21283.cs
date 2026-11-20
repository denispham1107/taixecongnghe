using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 21.283: xem tấm ảnh xa (NoiThat/Xa) của các căn gần một điểm: vật liệu, texture, UV, pháp tuyến, vị trí so với kính.
public static class CardInspect21283
{
 public static object Main(float x, float z, float r)
 {
  var res=new List<string>();
  var map=GameObject.Find("BanDoMo");
  foreach(Transform h in map.transform.Find("NhaPho")){
   if(Mathf.Abs(h.position.x-x)>r||Mathf.Abs(h.position.z-z)>r) continue;
   var nt=h.Find("NoiThat"); if(!nt){res.Add(h.name+" no NoiThat");continue;}
   var xa=nt.Find("Xa"); var mr=xa.GetComponent<MeshRenderer>(); var mf=xa.GetComponent<MeshFilter>(); var m=mf.sharedMesh;
   var uv=m.uv; var n=m.normals; var v=m.vertices;
   Vector2 mn=new Vector2(9,9),mx=new Vector2(-9,-9); foreach(var u in uv){mn=Vector2.Min(mn,u);mx=Vector2.Max(mx,u);}
   var wn=n.Length>0?xa.TransformDirection(n[0]):Vector3.zero;
   var mat=mr.sharedMaterial;
   res.Add($"{h.name} z{h.position.z:F1} mesh {m.name} v{m.vertexCount} uv {mn.ToString("F3")}-{mx.ToString("F3")} nrmW {wn.ToString("F2")} cen {mr.bounds.center.ToString("F2")} size {mr.bounds.size.ToString("F2")} mat {mat.name}/{mat.shader.name} tex {(mat.mainTexture?mat.mainTexture.name+" "+mat.mainTexture.width+"x"+mat.mainTexture.height:"-")} en {mr.enabled} act {xa.gameObject.activeInHierarchy} 3Dact {nt.Find("3D")?.gameObject.activeSelf}");
  }
  return res.ToArray();
 }
}
