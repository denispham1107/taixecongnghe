public static class RefreshWebIcons
{
 public static object Main()
 {
  UnityEditor.AssetDatabase.Refresh();
  PhoViet.Editor.PhoVietWebBuild.RefreshWebShell();
  return new{success=true,version=UnityEngine.Application.version};
 }
}
