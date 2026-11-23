using PhoViet.Editor;
// 21.361 — làm mới vỏ web 1.95.1 (bảng xếp hạng mở trong game ghi «Đăng nhập ở trang chủ…»)
// trên bản build 1.95.1 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21361
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
