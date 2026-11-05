using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using PhoViet;
public static class VerifyMissilePickupC2131
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  var flags=BindingFlags.Instance|BindingFlags.NonPublic;var pool=(IEnumerable)typeof(PhoVietGame).GetField("missilePickupTiles",flags).GetValue(g);
  int[] expected={1482,1014,592};int icons=0;
  foreach(var entry in pool)
  foreach(var icon in (Transform[])entry.GetType().GetField("icons").GetValue(entry))
  {
   icons++;var lods=icon.GetComponent<LODGroup>().GetLODs();
   check(lods.Length==3&&lods.Select(l=>l.renderers.Length).All(n=>n==1),"All pickup instances keep one surface per LOD #"+icons);
   for(int i=0;i<3;i++)
   {
    var mesh=lods[i].renderers[0].GetComponent<MeshFilter>().sharedMesh;var colors=mesh.colors;
    check(mesh.triangles.Length/3==expected[i]&&colors.Any(c=>c.r>.8f&&c.g>.2f&&c.g<.5f&&c.b<.1f)&&colors.Any(c=>c.b>.9f&&c.g>.6f&&c.a>.5f),"C geometry, amber frame and cyan markers in actual pool #"+icons+" LOD"+i);
   }
  }
  check(icons==70&&g.MissilePickupPoolCount==70,"All seventy street/bridge instances use C");
  var prefab=Resources.Load<GameObject>("PlayerMissile/PV_MissilePickup");var levels=prefab.GetComponent<LODGroup>().GetLODs();
  for(int i=0;i<3;i++)
  {
   var mesh=levels[i].renderers[0].GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;var colors=mesh.colors;
   check(mesh.bounds.size.x<1.86f&&mesh.bounds.size.y<=1.851f&&Mathf.Abs(mesh.bounds.center.x)<.02f&&Mathf.Abs(mesh.bounds.center.y)<.02f,"LOD shares centred pickup size #"+i);
   check(mesh.normals.All(n=>!float.IsNaN(n.x)&&Mathf.Abs(n.magnitude-1)<.01f),"Finite unit normals #"+i);
   var red=vertices.Where((v,n)=>colors[n].r>.8f&&colors[n].g<.1f).ToArray();
   check(red.Length>8&&red.Min(v=>v.y)>0&&red.Max(v=>v.y)>.5f,"Rounded red nose stays above badge centre #"+i);
   check(colors.Any(c=>c.b>.7f&&c.r<.05f&&c.g<.3f)&&colors.Any(c=>c.r>.8f&&c.g>.8f&&c.b>.8f),"Blue shield and white rocket retained #"+i);
  }
  check(prefab.GetComponentsInChildren<Light>(true).Length==0&&prefab.GetComponentsInChildren<ParticleSystem>(true).Length==0&&prefab.GetComponentsInChildren<Collider>(true).Length==0,"No extra effects, colliders or lights");
  var result=new{success=true,count=checks.Count,icons,checks};File.WriteAllText("Captures/pickup2131-C-pool-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count,result.icons};
 }
}
