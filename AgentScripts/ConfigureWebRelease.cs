public static class ConfigureWebRelease
{
 public static object Main()
 {
  PhoViet.Editor.PhoVietWebBuild.Configure();
  UnityEditor.PlayerSettings.bundleVersion="1.28.1";
  UnityEditor.AssetDatabase.SaveAssets();
  return new{success=true,version=UnityEngine.Application.version};
 }
}
