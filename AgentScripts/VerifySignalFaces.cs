using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;
using UnityEngine;using PhoViet;
public static class VerifySignalFaces
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  var filters=g.trafficLights.Select(r=>r.GetComponent<MeshFilter>()).ToArray();
  check(filters.Length==420&&filters.All(f=>f!=null&&f.sharedMesh!=null),"All 420 signal lenses retained");
  var meshes=filters.Select(f=>f.sharedMesh).Distinct().ToArray();
  check(meshes.Length==1&&meshes[0].name=="PV_SignalLenses_BothFaces","One shared two-face mesh, no per-lamp allocations");
  check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==10983,"Scenery renderer count unchanged");
  var mesh=meshes[0];var v=mesh.vertices;var n=mesh.normals;int half=v.Length/2;
  check(v.Length==194&&mesh.triangles.Length/3==336,"Compact domed lenses retain smooth normals");
  float offset=mesh.bounds.center.z*2;
  check(Mathf.Abs(offset-(.416f/.11f))<.0001f,"Rear lenses match housing reflection plane z=-0.022");
  for(int i=0;i<half;i++)check(Mathf.Abs(v[i].x-v[i+half].x)<.00001f&&Mathf.Abs(v[i].y-v[i+half].y)<.00001f&&Mathf.Abs(v[i].z+v[i+half].z-offset)<.00001f&&Mathf.Abs(n[i].z+n[i+half].z)<.00001f,"Mirrored lens vertex and normal "+i);
  check(n.All(x=>Mathf.Abs(x.magnitude-1)<.0001f),"Lens normals remain unit length");
  check(g.trafficLights.All(r=>r.sharedMaterials.Length==1),"Both sides share each lamp colour and emission material");
  typeof(PhoVietGame).GetMethod("InitializeTrafficSignalFaces",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
  g.ResetRide();
  check(filters.All(f=>f.sharedMesh==mesh),"Reset/reinitialization do not duplicate meshes");
  check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Ride jobs remain locked");
  var report=new{success=true,count=checks.Count,checks,rendererCount=10983,lensMeshes=1,triangles=336,vertexCount=194};
  File.WriteAllText("Captures/signal-rental-runtime-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
