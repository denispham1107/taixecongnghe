using System;
using System.IO;
using UnityEditor;
using PhoViet.Editor;
// 21.292 (bản 1.85.0): chờ nướng occlusion nền (menu 12) xong → lưu scene (Finish) → PhoVietWebBuild.Build() (build sạch 2 gói),
// ghi trạng thái ra Builds/release1850-status.json.
public static class QueueRelease21292
{
 const string status="Builds/release1850-status.json";
 public static object Main()
 {
  File.WriteAllText(status,"{\"state\":\"waiting-occlusion\"}");
  EditorApplication.update+=Tick;
  return "queued";
 }
 static void Tick()
 {
  if(StaticOcclusionCulling.isRunning)return;
  EditorApplication.update-=Tick;
  string occ=PhoViet.OpenWorldEditor.PhoVietOpenWorldOcclusionBuilder.Finish();
  File.WriteAllText(status,"{\"state\":\"building\",\"occlusion\":\""+occ+"\"}");
  EditorApplication.delayCall+=()=>{
   var t0=DateTime.Now;
   try{PhoVietWebBuild.Build();File.WriteAllText(status,"{\"state\":\"done\",\"seconds\":"+(int)(DateTime.Now-t0).TotalSeconds+",\"occlusion\":\""+occ+"\",\"stripped\":"+PhoVietMobileSceneStrip.LastStripped+"}");}
   catch(Exception e){File.WriteAllText(status,"{\"state\":\"error\",\"msg\":\""+e.Message.Replace("\"","'").Replace("\n"," ")+"\"}");}
  };
 }
}
