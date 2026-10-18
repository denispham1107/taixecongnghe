using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PhoViet;
public static class VerifyStreetArt
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  var all=g.scenery.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.StartsWith("PV_Street",StringComparison.Ordinal)).ToArray();
  check(all.Length==1260,"Every block in all five road rows has the complete Blender kit");
  check(all.Select(l=>AssetDatabase.GetAssetPath(l.GetComponentInChildren<MeshFilter>(true).sharedMesh)).Distinct().Count()==16,"Twelve shops, three trees and pavement are instanced from sixteen shared Blender FBX assets");
  foreach(var row in g.roadRows)
  {
   check(row.tiles.Count==7,"Seven connected blocks retained on "+row.root.name);
   foreach(var b in row.tiles)
   {
    var lods=b.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.StartsWith("PV_Street",StringComparison.Ordinal)).ToArray();
    check(lods.Length==36,b.name+": fourteen shopfronts, six trees and sixteen pavement segments");
    var shops=b.Cast<Transform>().Where(t=>t.name.StartsWith("Blender • ",StringComparison.Ordinal)).ToArray();
    check(shops.Length==14,b.name+": both street sides use detailed shop fronts");
    check(shops.All(t=>Mathf.Abs(Vector3.Dot(t.localRotation*Vector3.forward,Vector3.right))>.99f),b.name+": shop facades and integral wall-mounted signs face their own road at ninety degrees");
    check(lods.All(l=>l.size>4&&l.size<25),b.name+": model dimensions remain in metres after FBX conversion");
    check(lods.All(l=>l.GetLODs().Length==3&&l.GetLODs().All(d=>d.renderers.Length==1&&d.renderers[0]!=null)),b.name+": three complete distance LODs for every scenery instance");
   }
  }
  check(all.SelectMany(l=>l.GetComponentsInChildren<MeshRenderer>(true)).All(r=>r.sharedMaterial.name=="PV_StreetAtlas"),"All scenery uses one shared atlas material");
  check(g.GetComponentsInChildren<BikeVisualAnimator>(true).Length==33,"Detailed motorcycles and riders retained");
  check(g.GetComponentsInChildren<CarVisualAnimator>(true).Length==11,"Detailed four-wheel traffic retained");
  check(g.trafficLights.Count==420,"All junction traffic lamps retained");
  return new{success=true,count=checks.Count,checks};
 }
}
