using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet;
public static class InstallMorningFog
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Exit Play Mode first");
  const string folder="Assets/PhoViet/Art/Fog/";
  var importer=(TextureImporter)AssetImporter.GetAtPath(folder+"PV_MorningFogDensity.png");
  importer.sRGBTexture=false;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
  importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=512;
  importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaSource=TextureImporterAlphaSource.None;importer.SaveAndReimport();
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  var fog=g.rideCamera.GetComponent<MorningFog>();if(fog==null)fog=g.rideCamera.gameObject.AddComponent<MorningFog>();
  fog.lighting=g.daylight;fog.authoredProfile=AssetDatabase.LoadAssetAtPath<TextAsset>(folder+"PV_MorningFog.json");
  fog.densityAtlas=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"PV_MorningFogDensity.png");fog.volumeShader=Shader.Find("PhoViet/Morning Volume Fog");
  if(fog.authoredProfile==null||fog.densityAtlas==null||fog.volumeShader==null)throw new Exception("Fog assets missing");
  fog.enabled=false;g.daylight.morningFog=fog;
  EditorUtility.SetDirty(fog);EditorUtility.SetDirty(g.daylight);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
  return new{success=true,textureWidth=fog.densityAtlas.width,textureHeight=fog.densityAtlas.height,shader=fog.volumeShader.name};
 }
}
