using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet;
public static class InstallCrashUpgrade
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");AssetDatabase.Refresh();
  foreach(string name in new[]{"PV_WreckFlame","PV_WreckSmoke"})
  {
   string path="Assets/PhoViet/Resources/Wreck/"+name+".png";var i=(TextureImporter)AssetImporter.GetAtPath(path);
   i.textureType=TextureImporterType.Default;i.alphaSource=TextureImporterAlphaSource.FromInput;i.alphaIsTransparency=true;i.npotScale=TextureImporterNPOTScale.None;
   i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.wrapMode=TextureWrapMode.Clamp;i.filterMode=FilterMode.Bilinear;i.maxTextureSize=1024;i.SaveAndReimport();
  }
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();int effects=0;
  foreach(var v in g.vehicles.Where(v=>v.eventOnly))
  {
   Material oldFire=v.fire!=null?v.fire.GetComponent<ParticleSystemRenderer>().sharedMaterial:null,oldSmoke=v.smoke!=null?v.smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial:null;
   if(v.crashEffect!=null)UnityEngine.Object.DestroyImmediate(v.crashEffect.gameObject);
   v.crashEffect=RecklessArt.CrashFx(g.transform,out v.smoke,out v.fire);effects++;
   foreach(var r in v.crashEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
   {var m=r.sharedMaterial;var old=r.GetComponent<ParticleSystem>()==v.fire?oldFire:oldSmoke;
    if(old!=null&&AssetDatabase.Contains(old)&&old.name.StartsWith("PV_Mantaflow")){EditorUtility.CopySerialized(m,old);r.sharedMaterial=old;EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(m);}
    else AssetDatabase.CreateAsset(m,AssetDatabase.GenerateUniqueAssetPath("Assets/PhoViet/Resources/Wreck/"+m.name+".mat"));}
  }
  var obstacles=PhoViet.Editor.PhoVietObstacleUpgrade.Install();EditorUtility.SetDirty(g);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
  return new{success=true,effects,obstacles};
 }
}
