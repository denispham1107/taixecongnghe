using PhoViet.Editor;
// 21.364 — làm mới vỏ web 1.95.1 (nút đóng/menu dùng icon SVG, chặn bôi chọn chữ trên nút)
// trên bản build 1.95.1 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21364
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
