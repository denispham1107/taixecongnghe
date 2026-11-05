using System;using System.IO;using System.Reflection;using UnityEditor;using UnityEngine;using PhoViet;
public static class CaptureMissilePickupC2131
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static float scale;static int phase;static bool waiting,running;static double next;static bool[] flags;static bool sound,insets;static Vector4 oldInsets;static int best,coins;
 static readonly int[] widths={1440,1440,540,960,1440},heights={900,900,960,540,900};
 static readonly string[] names={"day","night","portrait","mobile-landscape","close"};
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static void HField(int i,string n,object v){var h=((Array)Get("helicopters")).GetValue(i);h.GetType().GetField(n).SetValue(h,v);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");if(running)return new{started=true};g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;Time.timeScale=0;
  flags=new[]{g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};sound=(bool)Get("sound");insets=(bool)Get("hasBrowserInsets");oldInsets=(Vector4)Get("browserInsets");best=g.BestDistance;coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  phase=0;waiting=false;running=true;Setup();EditorApplication.update+=Update;return new{started=true,frames=5};
 }
 static void Setup()
 {
  GameViewSetupSize(widths[phase],heights[phase]);g.RestartRide();g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("sound",false);Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);Set("hasBrowserInsets",true);Set("browserInsets",phase==2?new Vector4(0,.05f,0,.025f):phase==3?new Vector4(.05f,0,.04f,.03f):Vector4.zero);
  Set("PlayerMissiles",2);
  Call("TickMissilePickups",0f,g.rider.localPosition);g.daylight.SetCycleTime(phase==1?30:6);g.daylight.UpdateLamps();Call("PositionCamera",true);
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
   string path="Captures/pickup2131-"+names[phase]+".png";
   if(!waiting){
    if(phase==4)
    {
     var tiles=(System.Collections.IEnumerable)Get("missilePickupTiles");Transform target=null;float bestZ=float.MaxValue;
     foreach(var e in tiles)foreach(var icon in (Transform[])e.GetType().GetField("icons").GetValue(e))
     {var local=g.transform.InverseTransformPoint(icon.position);if(icon.gameObject.activeInHierarchy&&local.z>8&&local.z<bestZ){target=icon;bestZ=local.z;}}
     if(target==null)throw new Exception("Missing visible C pickup");
     var camera=Camera.main;var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var rt=new RenderTexture(1440,900,24);var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);
     try{camera.transform.position=target.position+g.transform.TransformVector(new Vector3(1.5f,1.3f,-3.5f));camera.transform.LookAt(target.position);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
     finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;camera.transform.SetPositionAndRotation(oldPosition,oldRotation);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);}
    }
    else ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+1;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing capture "+path);
   phase++;waiting=false;if(phase==5){Finish();File.WriteAllText("Captures/pickup2131-visual-results.json","{\"success\":true,\"frames\":5}");return;}Setup();
  }
  catch(Exception e){File.WriteAllText("Captures/pickup2131-visual-results.json",e.ToString());Finish();}
 }
 static void Finish()
 {
  running=false;EditorApplication.update-=Update;g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];Set("sound",sound);Set("hasBrowserInsets",insets);Set("browserInsets",oldInsets);Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);g.ResetRide();Time.timeScale=scale;
 }
}
