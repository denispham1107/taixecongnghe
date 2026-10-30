using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class BuildWebRelease
{
 public static object Main()
 {return Queue(false);}
 public static object Resume()
 {return Queue(true);}
 public static object Desktop()
 {return Queue(false,true);}
 static object Queue(bool resume,bool desktop=false)
 {
  if(Application.isPlaying||EditorApplication.isCompiling||BuildPipeline.isBuildingPlayer)throw new InvalidOperationException("Editor must be idle in Edit Mode");
  const string path="Captures/release152-build-report.json";
  if(resume)
  {
   var prior=BuildReport.GetLatestReport().summary;
   if(prior.result!=BuildResult.Succeeded)throw new InvalidOperationException("A successful mobile build is required before resuming");
   File.WriteAllText("Captures/release152-mobile-build-report.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{result=prior.result.ToString(),prior.totalErrors,prior.totalWarnings,buildMs=prior.totalTime.TotalMilliseconds},Newtonsoft.Json.Formatting.Indented));
  }
  File.WriteAllText(path,"{\"status\":\"queued\"}");
  // delayCall requires an inspector cycle and can stall in a headless/minimized editor.
  // Remove only our legacy queued delegate, then use a one-shot editor update.
  if(EditorApplication.delayCall!=null)foreach(var pending in EditorApplication.delayCall.GetInvocationList())
   if(pending.Method.DeclaringType?.FullName.StartsWith("BuildWebRelease",StringComparison.Ordinal)==true)
    EditorApplication.delayCall-=(EditorApplication.CallbackFunction)pending;
  if(EditorApplication.update!=null)foreach(var pending in EditorApplication.update.GetInvocationList())
   if(pending.Method.DeclaringType?.FullName.StartsWith("BuildWebRelease",StringComparison.Ordinal)==true)
    throw new InvalidOperationException("A release build is already queued");
  EditorApplication.CallbackFunction build=null;
  build=()=>
  {
   EditorApplication.update-=build;
   var timer=System.Diagnostics.Stopwatch.StartNew();
   File.WriteAllText(path,"{\"status\":\"building\"}");
   try
   {
    if(desktop)typeof(PhoViet.Editor.PhoVietWebBuild).GetMethod("BuildSingle",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,null);
    else if(resume)PhoViet.Editor.PhoVietWebBuild.CompleteTextureVariants();else PhoViet.Editor.PhoVietWebBuild.Build();var s=BuildReport.GetLatestReport().summary;
    File.WriteAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(new{status="completed",success=s.result==BuildResult.Succeeded,result=s.result.ToString(),s.totalErrors,s.totalWarnings,lastBuildMs=s.totalTime.TotalMilliseconds,totalMs=timer.ElapsedMilliseconds,version=Application.version},Newtonsoft.Json.Formatting.Indented));
   }
   catch(Exception e){File.WriteAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(new{status="failed",success=false,error=e.ToString(),totalMs=timer.ElapsedMilliseconds},Newtonsoft.Json.Formatting.Indented));Debug.LogException(e);}
  };
  EditorApplication.update+=build;EditorApplication.QueuePlayerLoopUpdate();
  return new{queued=true,path};
 }
}
