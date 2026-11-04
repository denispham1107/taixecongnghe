using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class CleanupHelicopterPreview2124
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  var roots=scene.GetRootGameObjects().Where(o=>PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(o).StartsWith("Assets/MeshyImports/Redline_Rescue_Helicopter_20261104_195539/",StringComparison.Ordinal)).ToArray();
  if(roots.Length>1)throw new Exception("Unexpected duplicate imported previews");
  if(roots.Length>0){Directory.CreateDirectory(".tools");if(!File.Exists(".tools/heli2124-before.unity"))File.Copy(scene.path,".tools/heli2124-before.unity");foreach(var root in roots)UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new Exception("Failed to save preview cleanup");}
  var r=new{success=true,removed=roots.Length,rawSourceKept=true,scene=scene.path};File.WriteAllText("Captures/heli2124-preview-cleanup.json",Newtonsoft.Json.JsonConvert.SerializeObject(r,Newtonsoft.Json.Formatting.Indented));return r;
 }
}
