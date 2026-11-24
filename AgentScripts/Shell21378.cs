using PhoViet.Editor;
// 21.378 — làm mới vỏ web 1.95.4: trang quản trị /admin + ghi lượt chơi (account.js) + sw.js bỏ qua /admin
// (index.html + account.js + sw.js + admin/) trên bản build 1.95.4 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21378
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
