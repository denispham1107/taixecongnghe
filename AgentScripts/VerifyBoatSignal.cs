using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;
using UnityEngine;using UnityEditor;using PhoViet;
public static class VerifyBoatSignal
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  void Check(bool b,string s){if(!b)throw new Exception(s);checks.Add(s);}
  const string folder="Assets/PhoViet/Art/River/FishingBoat/";
  var boats=g.scenery.GetComponentsInChildren<RiverBoat>(true);var meshes=AssetDatabase.LoadAllAssetsAtPath(folder+"PV_FishingBoat.fbx").OfType<Mesh>().OrderBy(m=>m.name).ToArray();
  Check(boats.Length==140&&meshes.Length==3,"140 anchored boats / three shared LOD meshes");
  var mat=AssetDatabase.LoadAssetAtPath<Material>(folder+"PV_FishingBoat.mat");Check(mat!=null&&mat.enableInstancing,"Dedicated textured instanced material");
  for(int i=0;i<3;i++)Check(meshes[i].GetIndexCount(0)/3<=new[]{40000,20000,10000}[i],"LOD mobile triangle budget "+i);
  foreach(var b in boats)
  {
   var rs=b.GetComponentsInChildren<MeshRenderer>(true);var fs=b.GetComponentsInChildren<MeshFilter>(true);var lod=b.GetComponent<LODGroup>();
   Check(fs.Length==3&&fs.All(f=>meshes.Contains(f.sharedMesh))&&rs.All(r=>r.sharedMaterial==mat),"Boat uses new model and material "+b.phase);
   Check(lod!=null&&lod.GetLODs().Length==3&&lod.GetLODs().All(l=>l.renderers.Length==1&&l.renderers[0]!=null),"All three boat LOD references "+b.phase);
   var r=rs.OrderBy(x=>x.name).First();var size=r.bounds.size;
   Check(Mathf.Abs(fs[0].sharedMesh.bounds.size.x-9)<.03f&&size.y>4&&size.y<5.5f,"Fishing boat proportions/9m length "+b.phase);
   Check(Mathf.Abs(b.restPosition.y-RiverBlock.WaterY)<.001f,"Original waterline anchor "+b.phase);
  }
  Check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==10983,"No added scenery renderers");
  Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Ride jobs remain locked");
  if(Application.isPlaying)
  {
   Check(g.trafficLights.Count==420,"Original 420 signal renderers retained");
   var filters=g.trafficLights.Select(r=>r.GetComponent<MeshFilter>()).ToArray();var signalMeshes=filters.Select(f=>f.sharedMesh).Distinct().ToArray();
   Check(signalMeshes.Length<=6&&signalMeshes.All(m=>m.vertexCount==388&&m.triangles.Length/3==672),"At most six shared meshes: main and auxiliary, two faces");
   for(int i=0;i<filters.Length;i++)
   {
    var t=filters[i].transform;var pole=t.parent.parent;var v=filters[i].sharedMesh.vertices;
    for(int side=0;side<2;side++)
    {
     var actual=t.TransformPoint(v[194+97*side]);var expected=pole.TransformPoint(new Vector3(0,3.85f-i%3*.3f,side==0?-.159f:.159f));
     Check(Vector3.Distance(actual,expected)<.001f,"Auxiliary lens at housing / face"+side+" / lamp"+i);
    }
   }
   var apply=typeof(PhoVietGame).GetMethod("ApplyTrafficSignalState",BindingFlags.Instance|BindingFlags.NonPublic);
   foreach(int phase in new[]{0,1,2})
   {
    apply.Invoke(g,new object[]{phase});
    for(int i=0;i<420;i++)
    {
     Color expected=i%3!=phase?StreetArt.Hex("354B47"):phase==0?StreetArt.Hex("FF6453"):phase==1?StreetArt.Gold:StreetArt.Hex("64E5A3");
     Check(g.trafficLights[i].sharedMaterials.Length==1&&Vector4.Distance(g.trafficLights[i].sharedMaterial.color,expected)<.001f,"Both main/aux faces share state "+phase+" / lamp"+i);
    }
   }
   var before=filters.Select(f=>f.sharedMesh).ToArray();typeof(PhoVietGame).GetMethod("InitializeTrafficSignalFaces",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
   Check(filters.Select((f,i)=>f.sharedMesh==before[i]).All(x=>x),"Signal initialization is idempotent");
  }
  else
  {
   Check(!g.gameObject.scene.isDirty,"Saved clean scene");
   var deps=AssetDatabase.GetDependencies(g.gameObject.scene.path,true);
   Check(!deps.Any(p=>p.Contains("Vietnamese_Fishing_Boat_20261030")||p.EndsWith("PV_RiverBoat0.fbx")||p.EndsWith("PV_RiverBoat1.fbx")),"Raw Meshy 109MB and old boats excluded from release dependencies");
   Check(Vector3.Distance(g.rider.localPosition,new Vector3(1.55f,0,1.92f))<.001f,"Authored player start preserved");
  }
  var result=new{success=true,count=checks.Count,play=Application.isPlaying,checks};File.WriteAllText(Application.isPlaying?"Captures/boat-signal-runtime-checks.json":"Captures/boat-signal-editor-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
 }
}
