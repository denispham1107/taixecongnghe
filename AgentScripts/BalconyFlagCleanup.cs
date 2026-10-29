using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using PhoViet;using Newtonsoft.Json.Linq;

// Asset-only balcony rod cleanup; never invokes an installer or saves a scene.
public static class BalconyFlagCleanup
{
 static string[] Paths=>new[]{0,3,6,9}.SelectMany(i=>new[]{"","_Petshop"}.Select(s=>"Assets/PhoViet/Art/Streets/PV_StreetShop"+i.ToString("00")+s+".fbx")).ToArray();
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
  var s=Snapshot();if(((JArray)s["meshes"]).Count!=24||((JArray)s["references"]).Count==0)throw new Exception("Missing shop mesh references");
  File.WriteAllText("Captures/balcony-flags-before-unity.json",s.ToString());return new{success=true,meshes=24,references=((JArray)s["references"]).Count};
 }
 public static object Import()
 {
  if(EditorApplication.isPlaying||Game().gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  foreach(string p in Paths)AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
  var a=JObject.Parse(File.ReadAllText("Captures/balcony-flags-before-unity.json"));var b=Snapshot();
  foreach(var old in (JArray)a["meshes"]){var fresh=((JArray)b["meshes"]).Single(m=>(string)m["name"]==(string)old["name"]);if((long)old["triangles"]-(long)fresh["triangles"]!=16)throw new Exception("Unexpected geometry delta "+old["name"]);old["triangles"]=fresh["triangles"].DeepClone();}
  if(!JToken.DeepEquals(a,b))throw new Exception("Scene or mesh bindings changed unexpectedly");
  var report=new{success=true,meshes=24,references=((JArray)b["references"]).Count,removedTrianglesPerLod=16,snapshot=b};
  File.WriteAllText("Captures/balcony-flags-unity-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.meshes,report.references};
 }
 public static object Capture()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=Game();var c=g.rideCamera;float scale=Time.timeScale;
  var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;LODGroup lod=null;GameObject preview=null;
  try{
   Time.timeScale=0;g.daylight.SetCycleTime(9);
   var m=g.scenery.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.gameObject.activeInHierarchy&&f.sharedMesh!=null&&f.sharedMesh.name=="PV_StreetShop06_LOD0").OrderBy(f=>Vector3.Distance(f.transform.position,g.rider.position)).First();
   lod=m.GetComponentInParent<LODGroup>();if(lod==null)throw new Exception("Missing house LODGroup");
   // Isolate the exact in-game model for inspection, without moving/hiding the street trees.
   preview=UnityEngine.Object.Instantiate(lod.gameObject);preview.name="Temporary balcony flag inspection";preview.transform.position+=Vector3.up*100;
   lod=preview.GetComponent<LODGroup>();m=preview.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.sharedMesh!=null&&f.sharedMesh.name=="PV_StreetShop06_LOD0");
   // FBX import into Unity reverses Blender's X; use the imported mesh coordinates.
   var target=m.transform.TransformPoint(new Vector3(-3.05f,4.9f,6.15f));
   var front=m.transform.TransformDirection(Vector3.up).normalized;var right=m.transform.TransformDirection(Vector3.right).normalized;
   for(int i=0;i<3;i++){
    lod.ForceLOD(i);c.transform.position=target+front*3.6f+right*1.4f+Vector3.up*.8f;c.transform.LookAt(target);c.fieldOfView=42;
    Save(c,"Captures/balcony-flags-close-lod"+i+".png");
   }
   lod.ForceLOD(0);c.transform.position=target+front*6+right*2+Vector3.up*2;c.transform.LookAt(target+Vector3.up*.3f);c.fieldOfView=55;Save(c,"Captures/balcony-flags-balcony.png");
   return new{success=true,source="Actual in-game house model, isolated temporary Play Mode copy in Unity",target=new[]{target.x,target.y,target.z},images=4};
  }finally{if(lod!=null)lod.ForceLOD(-1);if(preview!=null)UnityEngine.Object.DestroyImmediate(preview);c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;g.daylight.SetCycleTime(0);Time.timeScale=scale;}
 }
 static void Save(Camera c,string path)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,900,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.aspect=1280f/900;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(1280,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,900),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
