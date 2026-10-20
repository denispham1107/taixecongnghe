using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureRearWarnings
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static bool jobs,waiting;static float scale;static int phase;static double next;
 public static object Main()
 {
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();jobs=g.jobsEnabled;scale=Time.timeScale;Time.timeScale=0;phase=0;waiting=false;
  Setup();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;return new{started=true};
 }
 static void Setup()
 {
  g.jobsEnabled=false;g.RestartRide();
  typeof(PhoVietGame).GetField("rearRecklessPending",F).SetValue(g,true);
  typeof(PhoVietGame).GetField("rearRecklessLane",F).SetValue(g,phase==0?2:3);
  typeof(PhoVietGame).GetField("rearRecklessWarning",F).SetValue(g,1.4f);
  typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});
 }
 static void Update()
 {
  if(EditorApplication.timeSinceStartup<next)return;
  string format=Screen.width<Screen.height?"portrait":"wide",path="Captures/rear-warning-"+format+"-lane"+(phase==0?2:3)+".png";
  try
  {
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.7;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing screenshot");
   phase++;waiting=false;
   if(phase<2){Setup();next=EditorApplication.timeSinceStartup+.8;return;}
   File.WriteAllText("Captures/rear-warning-"+format+"-visual-results.json","{\"success\":true,\"frames\":2}");Finish();
  }
  catch(Exception e){File.WriteAllText("Captures/rear-warning-"+format+"-visual-results.json",JsonUtility.ToJson(new Failure{error=e.ToString()}));Finish();}
 }
 static void Finish(){EditorApplication.update-=Update;g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Failure{public string error;}
}
