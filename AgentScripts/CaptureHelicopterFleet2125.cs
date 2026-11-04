using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureHelicopterFleet2125
{
 static PhoVietGame g;static float scale;static int phase;static bool running,waiting;static double next;
 static readonly int[] widths={1440,540,1440,540},heights={900,960,900,960};
 static readonly string[] names={"two-desktop","two-portrait","five-desktop","five-portrait"};
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  if(running)return new{started=true};
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;Time.timeScale=0;
  phase=0;waiting=false;running=true;Setup();EditorApplication.update+=Update;
  return new{started=true,frames=4};
 }
 static void Setup()
 {
  GameViewSetupSize(widths[phase],heights[phase]);g.RestartRide();
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
  var flags=BindingFlags.NonPublic|BindingFlags.Instance;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,flags).SetValue(g,v);
  set("<Energy>k__BackingField",phase==0?150:120);set("spawnTimer",99999f);set("nextIntersectionDistance",999999f);
  set("jumpFlying",true);set("jumpLaunchVelocity",Mathf.Sqrt(48.4f*PhoVietGame.JumpHeightAtSpeed(17)));set("jumpHeight",3.4f);set("jumpPreviousHeight",3.4f);g.rider.localPosition+=Vector3.up*3.4f;g.TryJump();
  typeof(PhoVietGame).GetProperty("Distance").SetValue(g,phase<2?0f:1201f);
  typeof(PhoVietGame).GetMethod("TickHelicopter",flags).Invoke(g,new object[]{.001f,g.rider.localPosition,true});
  for(int i=0;i<g.HelicopterPoolCount;i++)if(g.IsHelicopterActive(i))
  {g.GetHelicopterRoot(i).localPosition-=Vector3.forward*71;g.GetHelicopterRoot(i).GetComponent<HelicopterVisual>().Advance(.037f);}

  typeof(PhoVietGame).GetMethod("TickEnergyPickups",flags).Invoke(g,new object[]{0f,g.rider.localPosition});
  g.rider.GetComponent<PlayerCarVisual>().Advance(.01f);
  g.daylight.SetCycleTime(6);typeof(PhoVietGame).GetMethod("PositionCamera",flags).Invoke(g,new object[]{true});g.daylight.UpdateLamps();
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
   string path="Captures/heli2125-"+names[phase]+".png";
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+1;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing capture: "+path);
   phase++;waiting=false;
   if(phase==4){Finish();File.WriteAllText("Captures/heli2125-visual-results.json","{\"success\":true,\"frames\":4}");return;}
   Setup();
  }
  catch(Exception e){File.WriteAllText("Captures/heli2125-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Error{public string error;}
}
