using PhoViet.Editor;
// 21.348–21.349 — làm mới vỏ web (21.348 sửa thanh trên rỗng; 21.349 khẩu hiệu + tiêu đề mới) lên bản build 1.94.0 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21348
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
