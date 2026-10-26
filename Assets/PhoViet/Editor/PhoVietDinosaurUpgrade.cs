using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PhoViet.Editor
{
 public static class PhoVietDinosaurUpgrade
 {
  public const string Folder="Assets/PhoViet/Art/Dinosaur/";
  const string Source="Assets/MeshyImports/Emerald Tyrant_20261026_221137/";
  public static object Install()
  {
   if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required");
   var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
   if(game==null||game.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity")throw new Exception("Wrong scene");
   if(game.jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Jobs must stay locked");
   AssetDatabase.Refresh();string path=Folder+"PV_EmeraldTyrant.fbx";
   var importer=(ModelImporter)AssetImporter.GetAtPath(path);
   importer.animationType=ModelImporterAnimationType.Legacy;importer.importAnimation=true;
   importer.animationCompression=ModelImporterAnimationCompression.Off;importer.optimizeGameObjects=false;importer.isReadable=false;importer.SaveAndReimport();
   var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"PV_EmeraldSkin.mat");
   if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,Folder+"PV_EmeraldSkin.mat");}
   material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"meshy_basecolor.png");material.color=Color.white;
   material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"meshy_normal.png"));material.EnableKeyword("_NORMALMAP");material.SetFloat("_BumpScale",.65f);
   material.SetFloat("_Glossiness",.18f);material.SetFloat("_Metallic",0);EditorUtility.SetDirty(material);
   var outer=new GameObject("Khủng long • Emerald Tyrant");
   try
   {
    var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);var body=(GameObject)PrefabUtility.InstantiatePrefab(model);body.transform.SetParent(outer.transform,false);
    foreach(var skin in body.GetComponentsInChildren<SkinnedMeshRenderer>(true)){skin.sharedMaterial=material;skin.updateWhenOffscreen=false;}
    var visual=outer.AddComponent<DinosaurVisual>();visual.player=body.GetComponent<Animation>()??body.AddComponent<Animation>();visual.player.playAutomatically=false;
    var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
    visual.walk=clips.Single(c=>c.name.EndsWith("Walk",StringComparison.Ordinal));visual.sweep=clips.Single(c=>c.name.EndsWith("TailSweep",StringComparison.Ordinal));
    Transform Find(string name)=>body.GetComponentsInChildren<Transform>(true).Single(t=>t.name==name);
    visual.tail=Enumerable.Range(0,5).Select(i=>Find("Tail"+i)).Concat(new[]{Find("TailTip")}).ToArray();
    visual.pelvis=Find("Pelvis");visual.head=Find("Head");visual.leftFoot=Find("FootL");visual.rightFoot=Find("FootR");
    game.dinosaurPrefab=PrefabUtility.SaveAsPrefabAsset(outer,Folder+"PV_EmeraldTyrant.prefab");game.dinosaursEnabled=true;
    // Preserve the user's newly imported reference root, excluding only that loose copy from builds.
    foreach(var root in game.gameObject.scene.GetRootGameObjects())
    {
     var source=PrefabUtility.GetCorrespondingObjectFromSource(root);
     if(source!=null&&AssetDatabase.GetAssetPath(source).StartsWith(Source,StringComparison.Ordinal))root.tag="EditorOnly";
    }
    EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(game.gameObject.scene);
    return new{success=true,clips=clips.Select(c=>new{c.name,c.length,c.legacy}).ToArray(),width=PhoVietGame.DinosaurWidth,height=6.0811f,speed=PhoVietGame.DinosaurWalkSpeed,jobs=game.jobsEnabled};
   }
   finally{UnityEngine.Object.DestroyImmediate(outer);}
  }
 }
}
