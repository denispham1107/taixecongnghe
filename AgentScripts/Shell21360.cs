using PhoViet.Editor;
// 21.360 — làm mới vỏ web (sw.js kho dữ liệu game riêng, iPhone lưu gói dữ liệu sau khi game mở xong)
// trên bản build 1.95.0 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21360
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
