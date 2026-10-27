using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace PhoViet.Editor
{
 public static class PhoVietPerformanceUpgrade
 {
  [MenuItem("Phố Việt/Performance/Apply distant street detail budget")]
  public static void Menu(){Debug.Log(JsonUtility.ToJson(Install()));}
  public static object Install()
  {
   if(Application.isPlaying)throw new Exception("Edit Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null||g.jobsEnabled||g.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity")throw new Exception("Correct scene with jobs locked required");
   if(!File.Exists(".tools/PhoViet-before-performance.unity"))EditorSceneManager.SaveScene(g.gameObject.scene,".tools/PhoViet-before-performance.unity",true);
   var before=AssetDatabase.GetDependencies(g.gameObject.scene.path,true).OrderBy(p=>p).ToArray();int changed=0;
   foreach(var lod in g.scenery.GetComponentsInChildren<LODGroup>(true))
   {
    var levels=lod.GetLODs();if(levels.Length!=3||!levels[0].renderers.Any(r=>r!=null&&(r.name.StartsWith("PV_StreetShop")||r.name.StartsWith("PV_StreetTree"))))continue;
    // Existing, hand-authored LODs preserve facade signs, striped awnings,
    // materials and the original silhouette. Only their distant selection changes.
    levels[0].screenRelativeTransitionHeight=.45f;levels[1].screenRelativeTransitionHeight=.175f;
    lod.SetLODs(levels);EditorUtility.SetDirty(lod);changed++;
   }
   if(changed!=700)throw new Exception("Unexpected street LOD group count: "+changed);
   EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
   var after=AssetDatabase.GetDependencies(g.gameObject.scene.path,true).OrderBy(p=>p).ToArray();if(!before.SequenceEqual(after))throw new Exception("Scene assets unexpectedly changed");
   var report=new{success=true,changed,assetsUnchanged=true,aa=QualitySettings.antiAliasing,lodBias=QualitySettings.lodBias,shadowDistance=QualitySettings.shadowDistance,jobs=g.jobsEnabled};File.WriteAllText("Captures/performance-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
 }
}
