using PhoViet.Editor;
// 21.400 — làm mới vỏ web 1.95.6: nút trang chủ «LÊN XE THÔI» → «VÀO GAME» + dòng «Bản cập nhật» (index.html)
// trên bản build 1.95.6 có sẵn, không build lại.
public static class Shell21400
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
