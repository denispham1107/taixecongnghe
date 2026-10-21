using UnityEngine;
using UnityEditor;
using PhoViet;
public static class RestoreStreetColors
{
 public static object Main()
 {
  PhoViet.Editor.PhoVietLightingUpgrade.RestoreColorfulStreetPaint();
  var atlas=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
  var ground=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Generated/PV_92A68AFFFalse.mat");
  return new{success=true,palette=AssetDatabase.GetAssetPath(atlas.mainTexture),ground=ColorUtility.ToHtmlStringRGB(ground.color)};
 }
}
