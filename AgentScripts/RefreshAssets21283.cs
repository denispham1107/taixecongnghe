using UnityEditor;
// 21.283: làm mới AssetDatabase để Unity thấy file .cs vừa sửa bên ngoài (tự làm mới đang tắt).
public static class RefreshAssets21283
{
 public static object Main()
 {
  AssetDatabase.ImportAsset("Assets/PhoViet/Editor/PhoVietWebBuild.cs", ImportAssetOptions.ForceUpdate);
  AssetDatabase.Refresh();
  return "refreshed";
 }
}
