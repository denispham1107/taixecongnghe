using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet;
public static class CleanupMeshyRiders
{
 public static object Main(){
  if(Application.isPlaying)throw new Exception("Exit Play Mode");
  const string old="Assets/MeshyImports/Vietnam Star Streetwear_20261023_104420";
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();int removed=0,references=0;
  foreach(var root in g.gameObject.scene.GetRootGameObjects()){
   var src=PrefabUtility.GetCorrespondingObjectFromSource(root);if(src==null)continue;
   var path=AssetDatabase.GetAssetPath(src);
   if(path.StartsWith(old+"/",StringComparison.Ordinal)){UnityEngine.Object.DestroyImmediate(root);removed++;}
   else if(path.StartsWith("Assets/MeshyImports/",StringComparison.Ordinal)){root.tag="EditorOnly";root.SetActive(false);references++;}
  }
  foreach(var skin in g.GetComponentsInChildren<SkinnedMeshRenderer>(true))if(AssetDatabase.GetAssetPath(skin.sharedMesh).StartsWith(old+"/",StringComparison.Ordinal))throw new Exception("Old human still in use");
  bool deleted=!AssetDatabase.IsValidFolder(old)||AssetDatabase.DeleteAsset(old);
  if(!deleted)throw new Exception("Could not remove superseded static human");
  EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
  return new{success=true,removed,deleted,referenceModelsEditorOnly=references,jobs=g.jobsEnabled};
 }
}
