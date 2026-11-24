using PhoViet.Editor;
// 21.381 — làm mới vỏ web 1.95.4: nút «CÁCH NHẬN 200,000VNĐ» + dòng giới thiệu bạn bè + hộp bảng mốc thưởng
// (index.html + style.css + app.js) trên bản build 1.95.4 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21381
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
