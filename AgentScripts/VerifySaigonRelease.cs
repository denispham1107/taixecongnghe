using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class VerifySaigonRelease
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  if(g==null||g.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity"||g.gameObject.scene.isDirty)throw new Exception("Expected saved PhoViet scene");
  if(g.jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Jobs must remain locked");
  var sets=g.scenery.GetComponentsInChildren<StreetObstacleSet>(true);
  var lods=g.scenery.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.StartsWith("PV_Street",StringComparison.Ordinal)).ToArray();
  var glass=lods.SelectMany(l=>l.GetComponentsInChildren<MeshRenderer>(true)).Count(r=>r.sharedMaterials.Length==2);
  if(sets.Length!=735||sets.Sum(s=>s.obstacles.Length)!=8050||lods.Length!=1260||glass!=1350||g.trafficLights.Count!=420)throw new Exception("Incomplete Saigon street installation");
  var dependencies=AssetDatabase.GetDependencies(g.gameObject.scene.path,true);
  var missing=dependencies.Where(p=>p.StartsWith("Assets/",StringComparison.Ordinal)&&!File.Exists(p)).ToArray();
  if(missing.Length!=0)throw new Exception("Missing scene dependencies: "+string.Join(",",missing));
  return new{success=true,scene=g.gameObject.scene.path,dirty=g.gameObject.scene.isDirty,version=PlayerSettings.bundleVersion,jobs=g.jobsEnabled,obstacleSets=sets.Length,obstacleBoxes=sets.Sum(s=>s.obstacles.Length),streetLods=lods.Length,glassRenderers=glass,signals=g.trafficLights.Count,generatedDependencies=dependencies.Where(p=>p.StartsWith("Assets/PhoViet/Generated/",StringComparison.Ordinal)).ToArray()};
 }
}
