using PhoViet.Editor;
// 21.356 — làm mới vỏ web (account.js: phím gõ trong ô nhập không tới Unity) trên bản build 1.95.0 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21356
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
