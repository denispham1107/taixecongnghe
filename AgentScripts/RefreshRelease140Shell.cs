public static class RefreshRelease140Shell
{
 public static object Main()
 {
  if(UnityEngine.Application.isPlaying||UnityEngine.Application.version!="1.40.0")throw new System.Exception("Saved 1.40.0 scene required");
  PhoViet.Editor.PhoVietWebBuild.RefreshWebShell();
  return new{success=true};
 }
}
