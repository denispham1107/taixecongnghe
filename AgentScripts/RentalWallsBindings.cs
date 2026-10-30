using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using PhoViet;using Newtonsoft.Json.Linq;

// Rental-only asset update; never invokes an installer or saves a scene.
public static class RentalWallsBindings
{
 static string[] Paths=>AssetDatabase.FindAssets("t:Model",new[]{"Assets/PhoViet/Art/Streets","Assets/PhoViet/Art/River"}).Select(AssetDatabase.GUIDToAssetPath).Where(PhoViet.Editor.PhoVietWallFinish.IsBuilding).ToArray();
 static PhoVietGame Game()=>UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
 static JObject Snapshot()
 {
  var g=Game();var paths=Paths;
  var meshes=paths.SelectMany(p=>AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>()).OrderBy(m=>m.name).ToArray();
  var refs=g.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh!=null&&paths.Contains(AssetDatabase.GetAssetPath(f.sharedMesh))).OrderBy(f=>GlobalObjectId.GetGlobalObjectIdSlow(f).ToString()).ToArray();
  return JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(new{
   start=new[]{g.rider.localPosition.x,g.rider.localPosition.y,g.rider.localPosition.z},jobs=g.jobsEnabled,scene=g.gameObject.scene.path,dirty=g.gameObject.scene.isDirty,
   renderers=g.scenery.GetComponentsInChildren<Renderer>(true).Length,obstacles=g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).Sum(s=>s.obstacles.Length),
   meshes=meshes.Select(m=>new{name=m.name,id=GlobalObjectId.GetGlobalObjectIdSlow(m).ToString(),submeshes=m.subMeshCount,triangles=Enumerable.Range(0,m.subMeshCount).Sum(s=>(long)m.GetIndexCount(s))/3}).ToArray(),
   references=refs.Select(f=>new{id=GlobalObjectId.GetGlobalObjectIdSlow(f).ToString(),mesh=GlobalObjectId.GetGlobalObjectIdSlow(f.sharedMesh).ToString(),materials=f.GetComponent<Renderer>().sharedMaterials.Select(m=>m.name).ToArray()}).ToArray()
  }));
 }
 public static object Baseline()
 {
  if(EditorApplication.isPlaying||Game().gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  if(Vector3.Distance(Game().rider.localPosition,new Vector3(1.55f,0,1.92f))>.001f||Game().jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Authored start and jobs lock must be retained");
  var s=Snapshot();if(((JArray)s["meshes"]).Count!=144||((JArray)s["references"]).Count==0)throw new Exception("Missing apartment mesh references");
  File.WriteAllText("Captures/rental-walls-bindings-before-unity.json",s.ToString());return new{success=true,meshes=((JArray)s["meshes"]).Count,references=((JArray)s["references"]).Count};
 }
 public static object Import()
 {
  if(EditorApplication.isPlaying||Game().gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  foreach(string p in Paths)AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
  // Resolve the saved renderer links after all FBXs finish reimporting, never save a transient scene.
  UnityEditor.SceneManagement.EditorSceneManager.OpenScene(Game().gameObject.scene.path);
  return Final();
 }
 public static object Final()
 {
  if(EditorApplication.isPlaying||Game().gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  var groups=Game().scenery.GetComponentsInChildren<LODGroup>(true);
  if(groups.Any(l=>l.GetLODs().Any(d=>d.renderers.Any(r=>r==null))))throw new Exception("Missing LOD renderer after scene reload");
  var a=JObject.Parse(File.ReadAllText("Captures/rental-walls-bindings-before-unity.json"));var b=Snapshot();File.WriteAllText(".tools/rental-walls-bindings-import-snapshot.json",b.ToString());
  foreach(var old in (JArray)a["meshes"]){var fresh=((JArray)b["meshes"]).SingleOrDefault(m=>(string)m["name"]==(string)old["name"]);if(fresh==null)throw new Exception("Missing imported mesh: "+old["name"]);if((long)fresh["triangles"]<=0)throw new Exception("Empty mesh "+old["name"]);old["triangles"]=fresh["triangles"].DeepClone();}
  if(!JToken.DeepEquals(a,b))throw new Exception("Scene or mesh bindings changed unexpectedly");
  var report=new{success=true,meshes=((JArray)b["meshes"]).Count,references=((JArray)b["references"]).Count,lodGroups=groups.Length,missingLodRenderers=0,trafficLenses=Game().trafficLights.Count,snapshot=b};
  File.WriteAllText("Captures/rental-walls-bindings-unity-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.meshes,report.references};
 }
}
