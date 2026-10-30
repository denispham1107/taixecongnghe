using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using PhoViet;using PhoViet.Editor;
public static class InstallWallFinish
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(Application.isPlaying||g.gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  const string detailPath="Assets/PhoViet/Art/Streets/PV_WallStuccoDetail.png";
  AssetDatabase.ImportAsset(detailPath,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
  var t=(TextureImporter)AssetImporter.GetAtPath(detailPath);t.textureType=TextureImporterType.Default;t.sRGBTexture=false;t.alphaIsTransparency=false;t.alphaSource=TextureImporterAlphaSource.FromInput;
  t.mipmapEnabled=true;t.mipMapBias=0;t.filterMode=FilterMode.Trilinear;t.wrapMode=TextureWrapMode.Repeat;t.anisoLevel=4;t.maxTextureSize=1024;t.textureCompression=TextureImporterCompression.CompressedHQ;t.compressionQuality=50;
  var web=t.GetPlatformTextureSettings("WebGL");web.name="WebGL";web.overridden=true;web.maxTextureSize=1024;web.format=TextureImporterFormat.DXT5;web.compressionQuality=50;t.SetPlatformTextureSettings(web);t.SaveAndReimport();
  var detail=AssetDatabase.LoadAssetAtPath<Texture2D>(detailPath);
  var atlas=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
  var room=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/River/PV_RiverRoomLight.mat");
  var shader=Shader.Find("PhoViet/Street Wall Finish");if(shader==null)throw new Exception("Wall finish shader missing");atlas.shader=shader;
  foreach(var m in new[]{atlas,room})
  {m.SetTexture("_StuccoDetail",detail);m.SetFloat("_StuccoScale",.65f);m.SetFloat("_StuccoStrength",.42f);m.SetFloat("_StuccoContrast",.28f);m.SetFloat("_WallSmoothness",.015f);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);}
  var paths=AssetDatabase.FindAssets("t:Model",new[]{"Assets/PhoViet/Art/Streets","Assets/PhoViet/Art/River"}).Select(AssetDatabase.GUIDToAssetPath).Where(PhoVietWallFinish.IsBuilding).ToArray();
  foreach(var p in paths)AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
  string scene=g.gameObject.scene.path;UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
  var report=new{success=true,modelFiles=paths.Length,paths,texture=detailPath,width=detail.width,height=detail.height,rendererCount=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>().scenery.GetComponentsInChildren<Renderer>(true).Length};
  File.WriteAllText("Captures/rental-walls-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
