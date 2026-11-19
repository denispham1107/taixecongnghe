using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 21.282: thống kê cờ static (Occluder/Occludee/Batching) của renderer trong scene bản đồ mở, theo nhóm gốc + tên.
public static class StaticFlags21282
{
 public static object Main()
 {
  var c=new Dictionary<string,int>(); int occluder=0,occludee=0,total=0,cams=0;
  foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include)){
   total++; var f=GameObjectUtility.GetStaticEditorFlags(r.gameObject);
   if((f&StaticEditorFlags.OccluderStatic)!=0)occluder++; if((f&StaticEditorFlags.OccludeeStatic)!=0)occludee++;
   var k=r.transform.root.name+" flags="+(int)f; c[k]=(c.TryGetValue(k,out var o)?o:0)+1;}
  foreach(var cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include))cams+=cam.useOcclusionCulling?1:0;
  return new{total,occluder,occludee,camsWithOcclusion=cams,hasData=UnityEditor.StaticOcclusionCulling.isRunning+"/"+UnityEditor.StaticOcclusionCulling.umbraDataSize,groups=c.OrderByDescending(x=>x.Value).Take(15).Select(x=>x.Value+" "+x.Key).ToArray()};
 }
}
