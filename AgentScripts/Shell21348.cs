using PhoViet.Editor;
// 21.348 — sửa vỏ web 1.94.0 (app.js: thanh trên rỗng che màn hình game) lên bản build 1.94.0 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21348
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
