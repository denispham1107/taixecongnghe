using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using PhoViet;
public static class BackupBoatSignalScene
{
 public static object Main(){var s=EditorSceneManager.GetActiveScene();if(Application.isPlaying)throw new Exception("Exit Play");
  string path=Path.GetFullPath(".tools/boat-signal-before.unity");bool saved=EditorSceneManager.SaveScene(s,path,true);
  var report=new{saved,dirty=s.isDirty,path=s.path,roots=s.GetRootGameObjects().Select(x=>new{x.name,components=x.GetComponents<Component>().Select(c=>c==null?"Missing":c.GetType().Name),asset=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(x)})};
  File.WriteAllText("Captures/boat-signal-scene-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
