using UnityEditor;
using UnityEngine;
public static class ConfigureBoostArt
{
 public static object Main()
 {
  const string path="Assets/PhoViet/Resources/Boost/PV_WhiteBoostSmoke.png";
  AssetDatabase.Refresh();var i=(TextureImporter)AssetImporter.GetAtPath(path);
  i.textureType=TextureImporterType.Default;i.alphaSource=TextureImporterAlphaSource.FromInput;i.alphaIsTransparency=true;
  i.npotScale=TextureImporterNPOTScale.None;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;
  i.wrapMode=TextureWrapMode.Clamp;i.filterMode=FilterMode.Bilinear;i.maxTextureSize=1024;i.SaveAndReimport();
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);return new{success=true,width=texture.width,height=texture.height};
 }
}
