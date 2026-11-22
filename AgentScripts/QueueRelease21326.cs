using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet.Editor;
// 21.326 (bản 1.91.0 = 21.319–21.325: lái nhạy, sương mưa, menu Tem đua, ô tô móp/bể):
// lưu scene → PhoVietWebBuild.Build() (build sạch 2 gói), ghi trạng thái ra Builds/release1910-status.json.
// Occlusion giữ dữ liệu nướng ở 21.302 (vật mới coi như động, vẫn được loại che đúng).
// Main() hẹn build qua delayCall — lần này không chạy (55 phút vẫn «building»); BuildNow() gọi build ngay trên luồng chính.
public static class QueueRelease21326
{
 const string status="Builds/release1910-status.json";
 static string Esc(string m)=>m.Replace("\"","'").Replace("\n"," ").Replace("\r"," ");
 public static object Main()
 {
  EditorSceneManager.SaveOpenScenes();
  File.WriteAllText(status,"{\"state\":\"building\"}");
  EditorApplication.delayCall+=()=>Run();
  return "queued";
 }
 public static object BuildNow()
 {
  EditorSceneManager.SaveOpenScenes();
  File.WriteAllText(status,"{\"state\":\"building-now\"}");
  Run();
  return File.ReadAllText(status);
 }
 static void Run()
 {
  var t0=DateTime.Now;
  try{PhoVietWebBuild.Build();File.WriteAllText(status,"{\"state\":\"done\",\"seconds\":"+(int)(DateTime.Now-t0).TotalSeconds+",\"stripped\":"+PhoVietMobileSceneStrip.LastStripped+"}");}
  catch(Exception e){File.WriteAllText(status,"{\"state\":\"error\",\"msg\":\""+Esc(e.Message)+"\"}");}
 }
}
