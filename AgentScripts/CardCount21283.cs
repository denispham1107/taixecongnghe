using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
// 21.283: đếm căn có tấm ảnh xa thuộc bộ 6 m/8 m (trước 21.283 mất UV) — cửa hàng (_B) / nhà ở; scene có bẩn không.
public static class CardCount21283
{
 public static object Main()
 {
  int shop=0,home=0,all=0;
  foreach(Transform h in GameObject.Find("BanDoMo").transform.Find("NhaPho")){
   var xa=h.Find("NoiThat/Xa"); if(!xa) continue; all++;
   var n=xa.GetComponent<MeshFilter>().sharedMesh.name;
   if(n.EndsWith(".001")||n.EndsWith(".002")){ if(h.name.EndsWith("_B")) shop++; else home++; }
  }
  return $"all {all} fixed shops {shop} homes {home} dirty {EditorSceneManager.GetActiveScene().isDirty}";
 }
}
