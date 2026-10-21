public static class ConfigureWebRelease
{
 public static object Main()
 {
  if(UnityEngine.Application.isPlaying)throw new System.InvalidOperationException("Exit Play Mode before configuring a web release.");
  PhoViet.Editor.PhoVietWebBuild.Configure();
  UnityEditor.PlayerSettings.bundleVersion="1.30.2";
  UnityEditor.AssetDatabase.SaveAssets();
  return new{success=true,version=UnityEngine.Application.version};
 }
}
