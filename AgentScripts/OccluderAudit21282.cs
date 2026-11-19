using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 21.282: renderer theo shader/kiểu × cờ Occluder — kính (ShopGlass), trong suốt, nội thất không được làm vật che.
public static class OccluderAudit21282
{
 public static object Main()
 {
  var c=new Dictionary<string,int>();
  foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include)){
   var f=GameObjectUtility.GetStaticEditorFlags(r.gameObject);
   bool occ=(f&StaticEditorFlags.OccluderStatic)!=0;
   var sh=string.Join("+",r.sharedMaterials.Where(m=>m).Select(m=>m.shader.name.Replace("PhoViet/OpenWorld/","").Replace("PhoViet/",""))).Trim();
   bool transp=r.sharedMaterials.Any(m=>m&&m.renderQueue>=2450);
   var k=(occ?"OCC ":"--- ")+sh+(transp?" [alpha]":"")+(r.GetComponentInParent<LODGroup>()&&r.GetComponentInParent<LODGroup>().name=="NoiThat"?" [NoiThat]":"");
   c[k]=(c.TryGetValue(k,out var o)?o:0)+1;}
  return c.OrderByDescending(x=>x.Value).Take(30).Select(x=>x.Value+" "+x.Key).ToArray();
 }
}
