using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using PhoViet;
public static class ProbeBoatSignal
{
 public static object Main(){
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(Application.isPlaying)throw new Exception("Edit required");
  string folder="Assets/MeshyImports/Vietnamese_Fishing_Boat_20261030_145637";var path=Directory.GetFiles(folder,"*.fbx").Single();var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
  var boats=g.scenery.GetComponentsInChildren<RiverBoat>(true);
  var r=new{success=model!=null,path,modelMeshes=model.GetComponentsInChildren<MeshFilter>(true).Select(m=>new{m.name,m.sharedMesh.vertexCount,triangles=m.sharedMesh.triangles.Length/3,bounds=m.sharedMesh.bounds}),modelMaterials=model.GetComponentsInChildren<Renderer>(true).SelectMany(x=>x.sharedMaterials).Distinct().Select(m=>new{m.name,shader=m.shader.name}),boats=boats.Length,sample=boats.Take(4).Select(b=>new{b.name,b.restPosition,b.restYaw,b.phase,renderers=b.GetComponentsInChildren<Renderer>(true).Select(x=>new{x.name,bounds=x.bounds}),lod=b.GetComponent<LODGroup>()?.lodCount}),signals=g.trafficLights.Count,lights=g.trafficLights.Take(4).Select(x=>new{x.name,parent=x.transform.parent.name,position=x.transform.localPosition,scale=x.transform.localScale}),scene=UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty};
  File.WriteAllText("Captures/boat-signal-source.json",Newtonsoft.Json.JsonConvert.SerializeObject(r,Newtonsoft.Json.Formatting.Indented,new Newtonsoft.Json.JsonSerializerSettings{ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore}));return new{success=true,boats=boats.Length,signals=g.trafficLights.Count};
 }
}
