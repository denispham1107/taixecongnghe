using PhoViet.Editor;
// 21.368 — làm mới vỏ web 1.95.2: gotaviet.web.app / gotaviet.firebaseapp.com tự chuyển sang gotaviet.com
// (index.html + sw.js) trên bản build 1.95.2 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21368
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
