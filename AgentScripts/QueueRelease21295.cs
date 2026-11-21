using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet.Editor;
// 21.295 (bản 1.86.0 = 21.293 Tháp Tài Chính + 21.294 mặt tiền nhà xa + 21.295 nút/khói/đạn/xe máy/ảnh trang chủ):
// lưu scene → PhoVietWebBuild.Build() (build sạch 2 gói) trên khung kế tiếp, ghi trạng thái ra Builds/release1860-status.json.
// Occlusion đã nướng lại ở 21.293; 21.294 không đổi hình khối nên không nướng lại.
public static class QueueRelease21295
{
 const string status="Builds/release1860-status.json";
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
