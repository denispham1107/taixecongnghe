using PhoViet.Editor;
// 21.363 — làm mới vỏ web 1.95.1 (rút gọn chữ hướng dẫn cài iPhone Chrome/Safari)
// trên bản build 1.95.1 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21363
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
