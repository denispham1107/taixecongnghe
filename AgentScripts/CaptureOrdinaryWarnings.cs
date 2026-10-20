using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureOrdinaryWarnings
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static bool jobs,exit,waiting;static float scale;static int phase;static double next;
 public static object Main()
 {
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();jobs=g.jobsEnabled;exit=g.turnExitTrafficEnabled;scale=Time.timeScale;Time.timeScale=0;phase=0;waiting=false;
  Setup();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;return new{started=true};
 }
 static void Setup()
 {
  g.jobsEnabled=false;g.RestartRide();g.turnExitTrafficEnabled=true;
  typeof(PhoVietGame).GetMethod("TryExitTraffic",F).Invoke(g,new object[]{true});
  if(phase>=1)typeof(PhoVietGame).GetMethod("TryExitTraffic",F).Invoke(g,new object[]{true});
  if(phase==2){typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,0);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[0],0,0);}
  typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});
 }
 static void Update()
 {
  if(EditorApplication.timeSinceStartup<next)return;
  string format=Screen.width<Screen.height?"portrait":"wide",path="Captures/ordinary-warning-"+format+(phase==0?"-single":phase==1?"-both":"-left-edge")+".png";
  try
  {
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.7;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing screenshot");
   phase++;waiting=false;
   if(phase<3){Setup();next=EditorApplication.timeSinceStartup+.8;return;}
   File.WriteAllText("Captures/ordinary-warning-"+format+"-visual-results.json","{\"success\":true,\"frames\":3}");Finish();
  }
  catch(Exception e){File.WriteAllText("Captures/ordinary-warning-"+format+"-visual-results.json",JsonUtility.ToJson(new Failure{error=e.ToString()}));Finish();}
 }
 static void Finish(){EditorApplication.update-=Update;g.jobsEnabled=jobs;g.turnExitTrafficEnabled=exit;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Failure{public string error;}
}
