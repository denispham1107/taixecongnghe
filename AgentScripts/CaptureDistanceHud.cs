using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureDistanceHud
{
 static PhoVietGame g;static float scale;static int phase;static bool running,waiting;static double next;
 static readonly int[] widths={540,960,1440},heights={960,540,900};
 static readonly string[] names={"portrait","mobile-landscape","desktop"};
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  if(running)return new{started=true};
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;Time.timeScale=0;
  phase=0;waiting=false;running=true;Setup();EditorApplication.update+=Update;
  return new{started=true,frames=3};
 }
 static void Setup()
 {
  GameViewSetupSize(widths[phase],heights[phase]);g.RestartRide();
  typeof(PhoVietGame).GetProperty("Distance").SetValue(g,1234f);
  typeof(PhoVietGame).GetProperty("Speed").SetValue(g,22.5f);
  next=EditorApplication.timeSinceStartup+1;
 }
 static void GameViewSetupSize(int width,int height)
 {
  var asm=typeof(Editor).Assembly;var sizesType=asm.GetType("UnityEditor.GameViewSizes");
  var single=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
  var sizes=single.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
  var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{GameViewSizeGroupType.Standalone});
  var type=group.GetType();var sizeType=asm.GetType("UnityEditor.GameViewSize");var mode=asm.GetType("UnityEditor.GameViewSizeType");
  string label="PhoViet "+width+"x"+height;var texts=(string[])type.GetMethod("GetDisplayTexts").Invoke(group,null);int selected=Array.FindIndex(texts,t=>t.Contains(label));
  if(selected<0){var size=sizeType.GetConstructor(new[]{mode,typeof(int),typeof(int),typeof(string)}).Invoke(new[]{Enum.ToObject(mode,1),(object)width,height,label});type.GetMethod("AddCustomSize").Invoke(group,new[]{size});selected=texts.Length;}
  var viewType=asm.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(viewType);
  viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,selected);view.Show();view.Focus();view.Repaint();
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   string path="Captures/distance-hud-"+names[phase]+".png";
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+1;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing capture: "+path);
   phase++;waiting=false;
   if(phase==3){Finish();File.WriteAllText("Captures/distance-hud-visual-results.json","{\"success\":true,\"frames\":3}");return;}
   Setup();
  }
  catch(Exception e){File.WriteAllText("Captures/distance-hud-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Error{public string error;}
}
