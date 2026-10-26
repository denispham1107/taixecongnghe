using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using PhoViet;
public static class InspectUrbanRiderScene
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var scene=g.gameObject.scene;var deps=AssetDatabase.GetDependencies(scene.path,true);var bikes=g.GetComponentsInChildren<BikeVisualAnimator>(true);
  var result=new{success=!scene.isDirty&&!g.jobsEnabled&&!PhoVietGame.RideJobsReleased&&bikes.Count(b=>b.urbanMotion!=null)==31&&bikes.Count(b=>b.role==BikeVisualAnimator.ActorRole.Police)==1&&!deps.Any(p=>p.StartsWith("Assets/MeshyImports/Urban Rider_")),sceneDirty=scene.isDirty,jobs=g.jobsEnabled,urban=bikes.Count(b=>b.urbanMotion!=null),police=bikes.Count(b=>b.role==BikeVisualAnimator.ActorRole.Police),playerCar=g.IsPlayerCar,baked=g.GetComponentsInChildren<MeshFilter>(true).Count(f=>f.sharedMesh!=null&&f.sharedMesh.name.StartsWith("PV_UrbanParked")),rawDependency=deps.Where(p=>p.StartsWith("Assets/MeshyImports/Urban Rider_")).ToArray(),oldCivilianModels=deps.Where(p=>p.Contains("/Bikes/Meshy")&&p.EndsWith(".fbx")).ToArray(),urbanAssets=deps.Where(p=>p.Contains("/UrbanRider/")).ToArray(),missingScripts=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))};
  File.WriteAllText("Captures/urban-rider-saved-scene.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
}
