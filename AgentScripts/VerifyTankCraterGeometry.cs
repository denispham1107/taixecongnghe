using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
public static class VerifyTankCraterGeometry
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  void Check(bool b,string text){if(!b)throw new Exception(text);checks.Add(text);}
  Check(Mathf.Abs(PhoVietGame.TankCraterRadius/PhoVietGame.BlastRadius-.92f)<.00001f,"Crater radius is 92% of blast radius");
  Check(g.tankCraterMaterial!=null,"Crater material is a saved release dependency");
  foreach(string name in new[]{"PhoViet/Road With Tank Craters","PhoViet/Tank Crater Surface","PhoViet/City Ground"})
  {var s=Shader.Find(name);Check(s!=null&&s.isSupported,"Shader supported: "+name);Check(!ShaderUtil.ShaderHasError(s),"No shader errors: "+name);}
  var counts=new List<int>();
  for(int variant=0;variant<3;variant++)
  {
   var mesh=TankCraterGeometry.Build(variant);
   try
   {
    var v=mesh.vertices;var t=mesh.triangles;var n=mesh.normals;var c=mesh.colors;
    Check(v.Length==c.Length&&v.Length==n.Length,"Complete vertex colors/normals "+variant);
    Check(t.Length/3<900&&v.Length<2700,"Shared mobile mesh budget "+variant);counts.Add(t.Length/3);
    Check(mesh.bounds.min.y<-.39f&&mesh.bounds.max.y>.1f,"True depressed floor and raised broken rim "+variant);
    bool area=true;for(int i=0;i<t.Length;i+=3)if(Vector3.Cross(v[t[i+1]]-v[t[i]],v[t[i+2]]-v[t[i]]).sqrMagnitude<1e-12f)area=false;
    Check(area,"No degenerate floor or crack triangles "+variant);
    Check(n.All(x=>Mathf.Abs(x.magnitude-1)<.001f),"Unit mesh normals "+variant);
    Check(v.Any(p=>new Vector2(p.x,p.z).magnitude>1.3f),"Branching cracks extend beyond the lip "+variant);
   }finally{UnityEngine.Object.DestroyImmediate(mesh);}
  }
  var mats=g.scenery.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null&&m.shader.name=="PhoViet/Road With Tank Craters").Distinct().ToArray();
  Check(mats.Length==3,"Only shared asphalt and white/yellow road paint use cutout");
  foreach(var m in mats)Check(Mathf.Abs(m.GetFloat("_Glossiness")-.12f)<.001f&&m.GetFloat("_Metallic")==0,"Original road roughness/metal preserved "+m.name);
  Check(!g.gameObject.scene.isDirty&&!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Clean saved scene/jobs locked");
  Check(Vector3.Distance(g.rider.localPosition,new Vector3(1.55f,0,1.92f))<.001f,"Authored player start unchanged");
  var result=new{success=true,count=checks.Count,checks,triangles=counts};File.WriteAllText("Captures/crater-geometry-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
}
