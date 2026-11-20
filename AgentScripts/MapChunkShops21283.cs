using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
// 21.283: liệt kê con của các khối OW_Map_* (tên + số đỉnh + vật liệu) để tìm dãy nhà phố dựng sẵn có bảng hiệu riêng.
public static class MapChunkShops21283
{
 public static object Main(string root)
 {
  var res=new List<string>();
  var r=GameObject.Find("BanDoMo").transform.Find(root);
  foreach(Transform c in r){
   var rr=c.GetComponentsInChildren<Renderer>(true);
   int v=0; foreach(var x in rr){var mf=x.GetComponent<MeshFilter>(); if(mf&&mf.sharedMesh) v+=mf.sharedMesh.vertexCount;}
   var mats=string.Join(",",rr.SelectMany(x=>x.sharedMaterials).Where(m=>m).Select(m=>m.name).Distinct().Take(6));
   res.Add(c.name+" ch"+c.childCount+" r"+rr.Length+" v"+v+" pos"+c.position.ToString("F0")+" "+mats);
  }
  return res.ToArray();
 }
}
