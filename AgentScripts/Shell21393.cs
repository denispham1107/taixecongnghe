using PhoViet.Editor;
// 21.393 — làm mới vỏ web 1.95.5: thẻ «Android · Máy tính» đổi «chọn» thành «Bấm vào nút»
// «Bấm vào nút “TẢI GAME” …» (index.html) trên bản build 1.95.5 có sẵn, không build lại.
public static class Shell21393
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
