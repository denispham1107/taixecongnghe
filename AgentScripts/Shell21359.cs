using PhoViet.Editor;
// 21.358–21.359 — làm mới vỏ web (hộp «Tải game» hướng dẫn iPhone Chrome/Safari có ảnh; tiêu điểm hộp thoại vào tiêu đề thay vì nút ×)
// trên bản build 1.95.0 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21359
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
