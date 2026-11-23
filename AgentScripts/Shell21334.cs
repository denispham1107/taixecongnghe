using PhoViet.Editor;
// 21.334 — chỉ cập nhật vỏ web (index.html/style.css/app.js/ảnh/controls) lên bản build 1.92.0 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21334
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
