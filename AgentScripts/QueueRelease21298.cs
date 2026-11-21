using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet.Editor;
// 21.298 (bản 1.87.0 = 21.297 thanh thông tin mẫu A + 21.298 cờ đỏ sao vàng bay gió):
// lưu scene → PhoVietWebBuild.Build() (build sạch 2 gói) trên khung kế tiếp, ghi trạng thái ra Builds/release1870-status.json.
// Không đổi hình khối/scene nên không nướng lại occlusion.
public static class QueueRelease21298
{
 const string status="Builds/release1870-status.json";
 public static object Main()
 {
  EditorSceneManager.SaveOpenScenes();
  File.WriteAllText(status,"{\"state\":\"building\"}");
  EditorApplication.delayCall+=()=>{
   var t0=DateTime.Now;
   try{PhoVietWebBuild.Build();File.WriteAllText(status,"{\"state\":\"done\",\"seconds\":"+(int)(DateTime.Now-t0).TotalSeconds+",\"stripped\":"+PhoVietMobileSceneStrip.LastStripped+"}");}
   catch(Exception e){File.WriteAllText(status,"{\"state\":\"error\",\"msg\":\""+e.Message.Replace("\"","'").Replace("\n"," ")+"\"}");}
  };
  return "queued";
 }
}
