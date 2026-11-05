using UnityEditor;using UnityEngine;
// 21.140: HUD import settings for the flight button icons, matching the missile icon.
public static class InstallFlight2140
{
 public static object Main()
 {
  foreach(var path in new[]{"Assets/PhoViet/Resources/PlayerFlight/PV_FlightUp.png","Assets/PhoViet/Resources/PlayerFlight/PV_FlightLand.png"})
  {
   AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
   var i=(TextureImporter)AssetImporter.GetAtPath(path);i.mipmapEnabled=false;i.alphaIsTransparency=true;i.wrapMode=TextureWrapMode.Clamp;i.filterMode=FilterMode.Bilinear;i.isReadable=false;i.SaveAndReimport();
  }
  AssetDatabase.Refresh();
  return new{up=Resources.Load<Texture2D>("PlayerFlight/PV_FlightUp")!=null,land=Resources.Load<Texture2D>("PlayerFlight/PV_FlightLand")!=null};
 }
}
