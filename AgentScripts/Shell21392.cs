using PhoViet.Editor;
// 21.392 — làm mới vỏ web 1.95.5: thẻ «Android · Máy tính» trong hộp Tải game chỉ còn một dòng
// «chọn “TẢI GAME” để tải và cài đặt Game trực tiếp vào máy nhé» (index.html) trên bản build 1.95.5 có sẵn, không build lại.
public static class Shell21392
{
 public static string Run(){ PhoVietWebBuild.RefreshWebShell(); return System.IO.File.ReadAllText("Builds/WebGL/build-info.json"); }
}
