using System;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using PhoViet;
public static class UpgradeMissileTrail2129
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  const string path="Assets/PhoViet/Resources/Helicopter/PV_HelicopterMissile.prefab";var scene=EditorSceneManager.NewPreviewScene();var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),scene);
  try
  {
   PrefabUtility.UnpackPrefabInstance(root,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
   PhoViet.Editor.PhoVietMissileTrailSetup.Configure(root);PrefabUtility.SaveAsPrefabAsset(root,path);AssetDatabase.SaveAssets();var v=root.GetComponent<MissileFlightVisual>();
   var result=new{success=true,fireLength=MissileFlightVisual.FireTrailLength,fireLifetime=MissileFlightVisual.FireTrailLifetime,smokeMax=v.smoke.main.maxParticles,smokeLifetime=v.smoke.main.startLifetime.constant,lights=root.GetComponentsInChildren<Light>(true).Length,previewOnly=true,sceneDirty=UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty};File.WriteAllText("Captures/trail2129-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }finally{UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.ClosePreviewScene(scene);}
 }
}
