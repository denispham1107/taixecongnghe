public static class ConfigureWebRelease
{
 public static object Main()
 {
  PhoViet.Editor.PhoVietWebBuild.Configure();
  UnityEditor.PlayerSettings.bundleVersion="1.21.0";
  UnityEditor.AssetDatabase.SaveAssets();
  return new{success=true,version=UnityEngine.Application.version};
 }
}
