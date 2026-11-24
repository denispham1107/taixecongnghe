using PhoViet.Editor;
// 21.377 — làm mới vỏ web 1.95.4: thẻ «Cách chơi» trong hộp «Tải game», nút «Hướng dẫn cài game» thay ô bánh răng
// (index.html + app.js + style.css) trên bản build 1.95.4 có sẵn trong Builds/WebGL, không build lại.
public static class Shell21377
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
