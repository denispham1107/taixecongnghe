using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
// 21.283: biển hiệu nào không có nội thất phía sau? Mỗi biển trong BanDoMo/BangHieu → căn NhaPho gần nhất (≤ 3 m) có NoiThat?
public static class SignInteriorAudit21283
{
 public static object Main()
 {
  var map=GameObject.Find("BanDoMo");
  var res=new List<string>();
  var houses=new List<(Vector3 p,Transform t)>();
  foreach(var r in map.GetComponentsInChildren<LODGroup>(true)) if(r.name=="NoiThat") houses.Add((r.transform.parent.position,r.transform.parent));
  res.Add("NoiThat total "+houses.Count);
  var signRoot=map.transform.Find("BangHieu");
  int n=0,withInt=0,noInt=0; var srcNames=new Dictionary<string,int>(); var noSamples=new List<string>();
  foreach(Transform s in signRoot){
   n++;
   var src=PrefabUtility.GetCorrespondingObjectFromSource(s.gameObject);
   string sn=src?src.name:"(none) "+s.name;
   string key=Regex.Replace(sn,@"\d+$","#");
   float bd=99; Transform bh=null;
   foreach(var h in houses){float d=Vector2.Distance(new Vector2(h.p.x,h.p.z),new Vector2(s.position.x,s.position.z)); if(d<bd){bd=d;bh=h.t;}}
   if(bd<3){withInt++;}else{noInt++; srcNames[key]=(srcNames.TryGetValue(key,out var o)?o:0)+1; if(noSamples.Count<15) noSamples.Add(sn+" @"+s.position.ToString("F1")+" near "+(bh?bh.name:"-")+" "+bd.ToString("F1"));}
  }
  res.Add($"signs {n} withInterior {withInt} without {noInt}");
  res.AddRange(srcNames.OrderByDescending(x=>x.Value).Take(20).Select(x=>x.Value+" "+x.Key));
  res.AddRange(noSamples);
  // root children summary
  foreach(Transform c in map.transform) res.Add("root "+c.name+" "+c.childCount);
  return res.ToArray();
 }
}
