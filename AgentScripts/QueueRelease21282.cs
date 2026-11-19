using System;
using System.IO;
using UnityEditor;
using PhoViet.Editor;
// 21.282: chạy PhoVietWebBuild.Build() (build sạch 2 gói) ở khung editor kế tiếp, ghi trạng thái ra Builds/release180-status.json.
public static class QueueRelease21282
{
 public static object Main()
 {
  const string status="Builds/release180-status.json";
  File.WriteAllText(status,"{\"state\":\"queued\"}");
  EditorApplication.delayCall+=()=>{
   var t0=DateTime.Now;
   try{PhoVietWebBuild.Build();File.WriteAllText(status,"{\"state\":\"done\",\"seconds\":"+(int)(DateTime.Now-t0).TotalSeconds+",\"stripped\":"+PhoVietMobileSceneStrip.LastStripped+"}");}
   catch(Exception e){File.WriteAllText(status,"{\"state\":\"error\",\"msg\":\""+e.Message.Replace("\"","'").Replace("\n"," ")+"\"}");}
  };
  return "queued";
 }
}
