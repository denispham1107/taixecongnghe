using PhoViet.Editor;
// 21.380 — làm mới vỏ web 1.95.4: account.js gửi lại điểm chờ khi có mạng lại / ván kế tiếp + dòng «Bản cập nhật» (index.html),
// trên bản build 1.95.4 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21380
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
