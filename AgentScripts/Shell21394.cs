using PhoViet.Editor;
// 21.394 — làm mới vỏ web 1.95.5: 8 ảnh hướng dẫn cài iPhone (install/ios-*.jpg) đổi chữ gotaviet.web.app → gotaviet.com
// + dòng «Bản cập nhật» (index.html) trên bản build 1.95.5 có sẵn, không build lại.
public static class Shell21394
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
