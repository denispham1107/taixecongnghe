using System;using System.IO;using System.Reflection;using UnityEditor;using UnityEngine;using PhoViet;
public static class CaptureGraphicsSettings
{
 static PhoVietGame g;static float time;static bool running,waiting,hadPreference;static int phase,oldSize,oldLevel,oldPreference;static double next;static EditorWindow view;static PropertyInfo selected;
 static readonly int[] widths={540,960,1440},heights={960,540,900};static readonly string[] names={"portrait","landscape","desktop"};
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");if(running)return new{started=true};g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g.State!=PhoVietGame.RideState.Ready)throw new Exception("Ready screen required");
  var viewType=typeof(Editor).Assembly.GetType("UnityEditor.GameView");view=EditorWindow.GetWindow(viewType);selected=viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);oldSize=(int)selected.GetValue(view);
  time=Time.timeScale;oldLevel=(int)g.SelectedGraphics;hadPreference=PlayerPrefs.HasKey(PhoVietGame.GraphicsPreferenceKey);oldPreference=PlayerPrefs.GetInt(PhoVietGame.GraphicsPreferenceKey);Time.timeScale=0;
  phase=0;waiting=false;running=true;Setup();EditorApplication.update+=Update;return new{started=true,frames=6};
 }
 static void Size(int width,int height)
 {
  var asm=typeof(Editor).Assembly;var sizesType=asm.GetType("UnityEditor.GameViewSizes");var single=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);var sizes=single.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{GameViewSizeGroupType.Standalone});var type=group.GetType();var sizeType=asm.GetType("UnityEditor.GameViewSize");var mode=asm.GetType("UnityEditor.GameViewSizeType");string label="PhoViet "+width+"x"+height;var texts=(string[])type.GetMethod("GetDisplayTexts").Invoke(group,null);int index=Array.FindIndex(texts,t=>t.Contains(label));
  if(index<0){var size=sizeType.GetConstructor(new[]{mode,typeof(int),typeof(int),typeof(string)}).Invoke(new[]{Enum.ToObject(mode,1),(object)width,height,label});type.GetMethod("AddCustomSize").Invoke(group,new[]{size});index=texts.Length;}
  selected.SetValue(view,index);view.Show();view.Focus();view.Repaint();
 }
 static void Setup(){int i=phase/2;Size(widths[i],heights[i]);g.CloseGraphicsSettings();g.SetGraphicsQuality(0);g.daylight.SetCycleTime(8);if(phase%2==1){g.SetGraphicsQuality(i);g.OpenGraphicsSettings();}next=EditorApplication.timeSinceStartup+1.4;}
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   string file="Captures/graphics-settings-"+names[phase/2]+(phase%2==0?"-gear":"-window")+".png";
   if(!waiting){ScreenCapture.CaptureScreenshot(file);waiting=true;next=EditorApplication.timeSinceStartup+1;return;}
   if(!File.Exists(file)||new FileInfo(file).Length==0)throw new Exception("Missing capture "+file);
   phase++;waiting=false;if(phase==6){Finish();File.WriteAllText("Captures/graphics-settings-visuals.json","{\"success\":true,\"frames\":6}");return;}Setup();
  }
  catch(Exception e){Finish();File.WriteAllText("Captures/graphics-settings-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=false,error=e.ToString()}));}
 }
 static void Finish()
 {
  running=false;EditorApplication.update-=Update;g.CloseGraphicsSettings();g.SetGraphicsQuality(oldLevel);if(hadPreference)PlayerPrefs.SetInt(PhoVietGame.GraphicsPreferenceKey,oldPreference);else PlayerPrefs.DeleteKey(PhoVietGame.GraphicsPreferenceKey);PlayerPrefs.Save();selected.SetValue(view,oldSize);g.daylight.ResetCycle();Time.timeScale=time;
 }
}
