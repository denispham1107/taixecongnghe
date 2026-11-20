using UnityEditor;
// 21.283: nhập lại 2 bộ nội thất 6 m / 8 m để tấm ảnh xa giữ UV (PhoVietInteriorMeshPostprocessor bỏ hậu tố .00x).
public static class ReimportInteriors21283
{
 public static object Main()
 {
  foreach(var f in new[]{"6_0","8_0"})
   AssetDatabase.ImportAsset("Assets/PhoViet/OpenWorld/Library/NoiThat/OW_NoiThat_"+f+".fbx", ImportAssetOptions.ForceUpdate);
  return "ok";
 }
}
