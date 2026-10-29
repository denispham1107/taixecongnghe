using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PhoViet;
using Newtonsoft.Json.Linq;

// Asset-only 21.87: imported bridge keeps its GUIDs/material bindings and every saved scene object.
public static class VerifyBridgeFixtures
{
 const string Model="Assets/PhoViet/Art/River/PV_RiverBridge.fbx";
 static PhoVietGame Game()=>UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
 static float[] V(Vector3 v)=>new[]{v.x,v.y,v.z};
 static JObject Snapshot()
 {
  var g=Game();var meshes=AssetDatabase.LoadAllAssetsAtPath(Model).OfType<Mesh>().OrderBy(m=>m.name).ToArray();
  var refs=g.GetComponentsInChildren<MeshFilter>(true).Where(m=>m.sharedMesh!=null&&m.sharedMesh.name.StartsWith("PV_RiverBridge_LOD",StringComparison.Ordinal)).ToArray();
  // Canonical JSON round-trip avoids comparing in-memory Single values to parsed Double values.
  return JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(new{
   start=V(g.rider.localPosition),jobs=g.jobsEnabled,scene=g.gameObject.scene.path,dirty=g.gameObject.scene.isDirty,
   renderers=g.scenery.GetComponentsInChildren<Renderer>(true).Length,
   obstacles=g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).Sum(s=>s.obstacles.Length),
   blocks=g.roadRows.SelectMany(r=>r.tiles).Count(t=>t.GetComponent<RiverBlock>()!=null),
   meshes=meshes.Select(m=>new{name=m.name,id=GlobalObjectId.GetGlobalObjectIdSlow(m).ToString(),bounds=new{center=V(m.bounds.center),extents=V(m.bounds.extents)},
    triangles=Enumerable.Range(0,m.subMeshCount).Sum(s=>(long)m.GetIndexCount(s))/3,submeshes=m.subMeshCount}).ToArray(),
   references=refs.OrderBy(m=>GlobalObjectId.GetGlobalObjectIdSlow(m).ToString()).Select(m=>new{
    id=GlobalObjectId.GetGlobalObjectIdSlow(m).ToString(),mesh=GlobalObjectId.GetGlobalObjectIdSlow(m.sharedMesh).ToString(),
    materials=m.GetComponent<Renderer>().sharedMaterials.Select(x=>x==null?"MISSING":x.name).ToArray()}).ToArray()
  }));
 }
 public static object Baseline()
 {
  if(EditorApplication.isPlaying||Game().gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  var data=Snapshot();File.WriteAllText("Captures/bridge-fixtures-before-unity.json",data.ToString());
  return new{success=true,meshes=data["meshes"],references=((JArray)data["references"]).Count};
 }
 public static object Import()
 {
  if(EditorApplication.isPlaying||Game().gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  AssetDatabase.ImportAsset(Model,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
  return Main();
 }
 public static object Main()
 {
  if(EditorApplication.isPlaying)throw new Exception("Edit Mode required");
  var old=JObject.Parse(File.ReadAllText("Captures/bridge-fixtures-before-unity.json"));var now=Snapshot();
  var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  foreach(string key in new[]{"start","jobs","scene","dirty","renderers","obstacles","blocks","references"})
   check(JToken.DeepEquals(old[key],now[key]),"Preserved "+key);
  check(((JArray)now["meshes"]).Count==3,"Exactly three imported bridge LOD meshes");
  var info=JObject.Parse(File.ReadAllText("Assets/PhoViet/Art/River/PV_RiverBridge.json"));
  for(int i=0;i<3;i++)
  {
   var a=old["meshes"][i];var b=now["meshes"][i];
   foreach(string key in new[]{"name","id","submeshes"})check(JToken.DeepEquals(a[key],b[key]),"LOD"+i+" retains "+key);
   check(a["bounds"]["center"].Zip(b["bounds"]["center"],(x,y)=>Math.Abs((float)x-(float)y)<.05f).All(x=>x)
    &&a["bounds"]["extents"].Zip(b["bounds"]["extents"],(x,y)=>Math.Abs((float)x-(float)y)<.05f).All(x=>x),"LOD"+i+" bounds retained within new bracket cap 5cm");
   check((long)b["triangles"]==(long)info["lod_triangles"][i],"LOD"+i+" imported triangle count matches Blender");
  }
  check((int)info["bridge_fixtures"]["flower_boxes"]==0,"No bridge flower boxes in generated asset");
  check((int)info["bridge_fixtures"]["flags"]==8&&(int)info["bridge_fixtures"]["supports_per_flag"]==2,"Eight flags with two supports each");
  check(Game().rider.GetComponent<PlayerCarVisual>()!=null&&!Game().jobsEnabled,"Supercar retained and jobs locked");
  var report=new{success=true,count=checks.Count,checks,snapshot=now};
  File.WriteAllText("Captures/bridge-fixtures-unity-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
