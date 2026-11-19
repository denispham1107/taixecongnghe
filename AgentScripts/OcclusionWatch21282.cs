using UnityEditor;
using PhoViet.OpenWorldEditor;
// 21.282: theo dõi nướng che khuất; xong thì lưu scene và ghi Builds/occlusion-status.json.
public static class OcclusionWatch21282
{
 public static object Main()
 {
  System.IO.File.WriteAllText("Builds/occlusion-status.json","{\"state\":\"running\"}");
  EditorApplication.CallbackFunction cb=null;
  cb=()=>{ if(StaticOcclusionCulling.isRunning)return; EditorApplication.update-=cb;
   string r; try{r=PhoVietOpenWorldOcclusionBuilder.Finish();}catch(System.Exception e){r="lỗi "+e.Message;}
   System.IO.File.WriteAllText("Builds/occlusion-status.json","{\"state\":\"done\",\"result\":\""+r.Replace("\"","'")+"\"}"); };
  EditorApplication.update+=cb; return "watching";
 }
}
