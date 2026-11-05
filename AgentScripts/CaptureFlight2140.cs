using System;using System.IO;using System.Reflection;using UnityEditor;using UnityEngine;using PhoViet;
// 21.140: the real Game view, with HUD, in flight with air icons ahead, sideways and upright, plus the ground state.
public static class CaptureFlight2140
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static float scale;static int phase;static double next;static bool[] flags;
 static readonly int[] widths={1688,780},heights={780,1688};
 static readonly string[] names={"ngang-canh-xe","doc-canh-xe"};
 static readonly bool[] flying={true,true};
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null&&p.CanWrite)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;Time.timeScale=0;
  flags=new[]{g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};phase=0;Setup();EditorApplication.update-=Update;EditorApplication.update+=Update;return "started";
 }
 static void Setup()
 {
  SetSize(widths[phase],heights[phase]);g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);g.daylight.SetCycleTime(6);
  var tick=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),g,typeof(PhoVietGame).GetMethod("TickRide",F));
  if(flying[phase]){g.TryToggleFlightButton();for(int i=0;i<60&&!g.RiderFlying;i++)tick(1/60f);}
  for(int i=0;i<(flying[phase]?70:20);i++){tick(1/60f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}}
  // Bring one air icon close enough to judge its height against the car.
  if(flying[phase])
  {
   var tiles=(System.Collections.IList)typeof(PhoVietGame).GetField("energyTiles",F).GetValue(g);int moved=0;
   foreach(var e in tiles){foreach(Transform t in (Transform[])e.GetType().GetField("airIcons").GetValue(e)){if(moved<2&&t.gameObject.activeInHierarchy){var at=t.parent.InverseTransformPoint(new Vector3(g.rider.position.x+(moved==0?3.2f:-3.2f),0,g.rider.position.z+3+moved*7));t.localPosition=new Vector3(at.x,PhoVietGame.AirPickupHeight,at.z);moved++;}}}
  }
  g.daylight.UpdateLamps();Call("PositionCamera",true);next=EditorApplication.timeSinceStartup+1.2;
 }
 static void Update()
 {
  if(!Application.isPlaying){EditorApplication.update-=Update;return;}
  if(EditorApplication.timeSinceStartup<next)return;
  ScreenCapture.CaptureScreenshot(Path.GetFullPath("Captures/flight2140-"+names[phase]+".png"));
  phase++;
  if(phase>=names.Length){EditorApplication.update-=Update;EditorApplication.delayCall+=()=>{g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.recklessTrafficEnabled=flags[3];g.trafficSurprises=flags[4];g.turnExitTrafficEnabled=flags[5];g.ResetRide();Time.timeScale=scale;};return;}
  next=EditorApplication.timeSinceStartup+1;EditorApplication.delayCall+=Setup;next=EditorApplication.timeSinceStartup+2.2;
 }
 static void SetSize(int width,int height)
 {
  var asm=typeof(Editor).Assembly;var sizesType=asm.GetType("UnityEditor.GameViewSizes");
  var single=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
  var sizes=single.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
  var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{GameViewSizeGroupType.Standalone});
  var type=group.GetType();var sizeType=asm.GetType("UnityEditor.GameViewSize");var mode=asm.GetType("UnityEditor.GameViewSizeType");
  string label="PhoViet "+width+"x"+height;var texts=(string[])type.GetMethod("GetDisplayTexts").Invoke(group,null);int selected=Array.FindIndex(texts,t=>t.Contains(label));
  if(selected<0){var size=sizeType.GetConstructor(new[]{mode,typeof(int),typeof(int),typeof(string)}).Invoke(new[]{Enum.ToObject(mode,1),(object)width,height,label});type.GetMethod("AddCustomSize").Invoke(group,new[]{size});selected=texts.Length;}
  var view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,selected);view.Repaint();
 }
}
