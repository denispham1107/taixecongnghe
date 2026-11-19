using PhoViet.OpenWorldEditor;
// 21.282: gắn cờ che khuất + bắt đầu nướng nền (args: "start") hoặc lưu khi xong (args: "finish").
public static class Occlusion21282
{
 public static object Main(string mode)
 {
  if(mode=="start"){var r=PhoVietOpenWorldOcclusionBuilder.ApplyFlags();PhoVietOpenWorldOcclusionBuilder.Bake(false);return r+" | bắt đầu nướng";}
  return PhoVietOpenWorldOcclusionBuilder.Finish();
 }
}
