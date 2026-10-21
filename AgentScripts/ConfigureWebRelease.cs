public static class ConfigureWebRelease
{
 public static object Main()
 {
  PhoViet.Editor.PhoVietWebBuild.Configure();
  UnityEditor.PlayerSettings.bundleVersion="1.27.1";
  UnityEditor.AssetDatabase.SaveAssets();
  return new{success=true,version=UnityEngine.Application.version};
 }
}
