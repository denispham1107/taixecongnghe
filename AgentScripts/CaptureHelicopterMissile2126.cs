using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureHelicopterMissile2126
{
 static PhoVietGame g;static float scale;static int phase;static bool running,waiting;static double next;
 static readonly int[] widths={1440,1440,540,540,1440,540,1440},heights={900,900,960,960,900,960,900};
 static readonly string[] names={"missile-close","flight","flight-portrait","roll","collision","collision-portrait","missile-hit-night"};
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  if(running)return new{started=true};
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;Time.timeScale=0;
  phase=0;waiting=false;running=true;Setup();EditorApplication.update+=Update;
  return new{started=true,frames=7};
 }
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static object Call(string name,params object[] args)=>typeof(PhoVietGame).GetMethod(name,F).Invoke(g,args);
 static void Set(string name,object v){var p=typeof(PhoVietGame).GetProperty(name,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(name,F).SetValue(g,v);}
 static object Actor(int i)=>((Array)typeof(PhoVietGame).GetField("helicopters",F).GetValue(g)).GetValue(i);
 static object Missile(int i)=>((Array)typeof(PhoVietGame).GetField("helicopterMissiles",F).GetValue(g)).GetValue(i);
 static void Setup()
 {
  GameViewSetupSize(widths[phase],heights[phase]);g.RestartRide();
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
  Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);Set("jumpFlying",true);Set("jumpLaunchVelocity",Mathf.Sqrt(48.4f*PhoVietGame.JumpHeightAtSpeed(17)));Set("jumpHeight",3.4f);Set("jumpPreviousHeight",3.4f);g.rider.localPosition+=Vector3.up*3.4f;g.TryJump();Set("Speed",20f);
  Call("TickHelicopter",.001f,g.rider.localPosition,true);
  for(int i=0;i<2;i++){g.GetHelicopterRoot(i).localPosition=new Vector3(PhoVietGame.LaneX[i==0?0:3],g.rider.localPosition.y+g.PlayerHeight*.5f,phase==1?40+i*16:25+i*16);Actor(i).GetType().GetField("shotRemaining").SetValue(Actor(i),100f);}
  if(phase==4||phase==5)
  {
   g.HelicopterRoot.localPosition=new Vector3(g.rider.localPosition.x,g.rider.localPosition.y+g.PlayerHeight*.5f,6);Call("ScrollHelicopter",12f);Call("TickHelicopter",.05f,g.rider.localPosition,true);
   for(int i=0;i<(phase==4?30:18);i++){Call("TickRiderCrash",.02f);Call("TickHelicopter",.02f,g.rider.localPosition,false);}
  }
  else
  {
   g.HelicopterRoot.localPosition=new Vector3(g.rider.localPosition.x,g.rider.localPosition.y+g.PlayerHeight*.5f,phase==1?40:25);Call("FireHelicopterMissile",Actor(0));
   if(phase==3){g.TryBarrelRoll();Call("TickJump",.4f);}
   for(int i=0;i<(phase==6?45:phase==1?25:phase==3?33:14);i++)
   {Call("ScrollHelicopterMissiles",.2f);Call("TickHelicopterMissiles",.01f,g.rider.localPosition,phase==6||phase==3);var m=Missile(0);var r=(Transform)m.GetType().GetField("root").GetValue(m);r.GetComponent<MissileFlightVisual>().smoke.Simulate(.01f,false,false);r.GetComponent<MissileFlightVisual>().Advance(.01f);}
   if(phase==6)for(int i=0;i<12;i++){Call("TickRiderCrash",.02f);Call("TickHelicopter",.02f,g.rider.localPosition,false);}
  }
  g.rider.GetComponent<PlayerCarVisual>().Advance(.01f);g.daylight.SetCycleTime(phase==6?30:6);Call("PositionCamera",true);g.daylight.UpdateLamps();Call("RenderThunderstorm");
  foreach(var ps in g.GetComponentsInChildren<ParticleSystem>(true))if(ps.gameObject.activeInHierarchy&&ps.isPlaying)ps.Simulate(.35f,false,false);
  if(phase==4){var camera=Camera.main;camera.transform.position=g.transform.TransformPoint(new Vector3(10,8,-14));camera.transform.LookAt(g.transform.TransformPoint(new Vector3(0,1,0)));}
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
   string path="Captures/missile2126-"+names[phase]+".png";
   if(!waiting){
    if(phase==4){var camera=Camera.main;var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var rt=new RenderTexture(1440,900,24);camera.targetTexture=rt;camera.transform.position=g.transform.TransformPoint(new Vector3(12,9,-17));camera.transform.LookAt(g.transform.TransformPoint(new Vector3(0,1,-2)));camera.Render();RenderTexture.active=rt;var texture=new Texture2D(1440,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1440,900),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());camera.targetTexture=oldTarget;RenderTexture.active=oldActive;camera.transform.SetPositionAndRotation(oldPosition,oldRotation);UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(rt);}
    else ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+1;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Missing capture: "+path);
   phase++;waiting=false;
   if(phase==7){Finish();File.WriteAllText("Captures/missile2126-visual-results.json","{\"success\":true,\"frames\":7}");return;}
   Setup();
  }
  catch(Exception e){File.WriteAllText("Captures/missile2126-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;g.ResetRide();Time.timeScale=scale;}
 [Serializable]class Error{public string error;}
}
