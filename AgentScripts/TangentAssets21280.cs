using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
// 21.280: mesh có tangent theo file nguồn + shader của renderer dùng chúng (scene + prefab gán lúc chạy) → file nào bỏ tangent an toàn.
public static class TangentAssets21280
{
 static bool NeedsTangent(Material m){ if(!m||!m.shader)return true; var n=m.shader.name;
  if(n.StartsWith("PhoViet/OpenWorld/")||n=="PhoViet/Street Wall Finish"||n.StartsWith("Unlit/")||n.StartsWith("Legacy Shaders/Particles"))return false;
  if(n=="Standard"||n.StartsWith("Standard"))return m.IsKeywordEnabled("_NORMALMAP")||m.IsKeywordEnabled("_PARALLAXMAP");
  return true; }
 public static object Main()
 {
  var use=new Dictionary<Mesh,HashSet<string>>();var need=new HashSet<Mesh>();
  foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include)){
   var m=mf.sharedMesh;if(!m||!m.HasVertexAttribute(VertexAttribute.Tangent))continue;var r=mf.GetComponent<Renderer>();
   if(!use.ContainsKey(m))use[m]=new HashSet<string>();
   if(r)foreach(var mat in r.sharedMaterials){use[m].Add(mat?mat.shader.name:"null");if(NeedsTangent(mat))need.Add(m);}
  }
  var rows=use.Keys.GroupBy(m=>AssetDatabase.GetAssetPath(m)).Select(g=>{
   long b=g.Sum(m=>(long)m.vertexCount*16);bool any=g.Any(m=>need.Contains(m));
   var imp=AssetImporter.GetAtPath(g.Key);
   return new{path=g.Key,mb=Math.Round(b/1048576.0,2),safe=!any,importer=imp?imp.GetType().Name:"",shaders=string.Join(",",g.SelectMany(m=>use[m]).Distinct())};
  }).OrderByDescending(x=>x.mb).ToList();
  return new{safeMb=Math.Round(rows.Where(r=>r.safe).Sum(r=>r.mb),1),unsafeMb=Math.Round(rows.Where(r=>!r.safe).Sum(r=>r.mb),1),rows=rows.Take(40).Select(r=>(r.safe?"SAFE ":"KEEP ")+r.mb+"MB "+r.importer+" "+r.path+" ["+r.shaders+"]").ToArray()};
 }
}
