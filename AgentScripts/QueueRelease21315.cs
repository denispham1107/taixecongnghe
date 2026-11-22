using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet.Editor;
// 21.315 (bản 1.89.0 = 21.312–21.315: nút KHIÊN + phá lưới, trúng đạn cháy nổ, nút Tên lửa/Lazer, 2 ảnh trang chủ):
// lưu scene → PhoVietWebBuild.Build() (build sạch 2 gói) trên khung kế tiếp, ghi trạng thái ra Builds/release1890-status.json.
// Occlusion giữ dữ liệu nướng ở 21.302 (vật mới coi như động, vẫn được loại che đúng).
public static class QueueRelease21315
{
 const string status="Builds/release1890-status.json";
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
