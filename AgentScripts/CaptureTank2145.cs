using System;using System.IO;using System.Reflection;using UnityEditor;using UnityEngine;using PhoViet;
// 21.140: the real Game view, with HUD, in flight with air icons ahead, sideways and upright, plus the ground state.
public static class CaptureTank2145
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static float scale;static int phase;static double next;static bool[] flags;static bool capturePending;
 static readonly int[] widths={1440,1440,780},heights={900,900,1688};
 static readonly string[] names={"burn-day","burn-night","air-energy"};
 static int oldViewSize;static PropertyInfo selectedViewSize;static EditorWindow oldView;
 static readonly bool[] flying={false,false,true};
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null&&p.CanWrite)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;Time.timeScale=0;
  var viewType=typeof(Editor).Assembly.GetType("UnityEditor.GameView");oldView=EditorWindow.GetWindow(viewType);selectedViewSize=viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);oldViewSize=(int)selectedViewSize.GetValue(oldView);
  flags=new[]{g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};phase=0;capturePending=false;Setup();EditorApplication.update-=Update;EditorApplication.update+=Update;return "started";
 }
 static void Setup()
 {
  SetSize(widths[phase],heights[phase]);g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;g.RestartRide();
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);g.daylight.SetCycleTime(6);
  var tick=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),g,typeof(PhoVietGame).GetMethod("TickRide",F));
  if(flying[phase]){g.TryToggleFlightButton();for(int i=0;i<60&&!g.RiderFlying;i++)tick(1/60f);}
  if(!flying[phase])
  {
   var tank=g.SpawnScheduledTank();if(tank==null)throw new Exception("No tank for capture");tank.model.localPosition=tank.previousPosition=new Vector3(-1.55f,0,40);
   if(!g.TryFirePlayerMissile())throw new Exception("Ground launcher unavailable");
   for(int i=0;i<180&&!tank.tankDestroyed;i++){Call("ScrollPlayerMissiles",g.Speed/60f);Call("TickTrafficMotion",1/60f);Call("TickPlayerMissiles",1/60f);Call("RenderThunderstorm");}
   if(!tank.tankDestroyed)throw new Exception("Capture missile missed");
   var storm=(ThunderstormVisual)typeof(PhoVietGame).GetField("stormVisual",F).GetValue(g);
   var burns=(System.Collections.IList)typeof(ThunderstormVisual).GetField("burns",F).GetValue(storm);
   foreach(var burn in burns)if((bool)burn.GetType().GetField("active").GetValue(burn))foreach(var ps in (ParticleSystem[])burn.GetType().GetField("particles").GetValue(burn)){ps.Simulate(1.1f,false,true,true);ps.Pause();}
   g.daylight.SetCycleTime(phase==0?8:26);Call("RenderThunderstorm");
  }
  else
  {
   for(int i=0;i<50;i++){tick(1/60f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}}
  }
  g.daylight.UpdateLamps();Call("PositionCamera",true);next=EditorApplication.timeSinceStartup+1.2;
 }
 static void Update()
 {
  if(!Application.isPlaying){EditorApplication.update-=Update;return;}
  if(EditorApplication.timeSinceStartup<next)return;
  // CaptureScreenshot writes at end of frame: do not switch scene/time-of-day before it finishes.
  if(!capturePending){ScreenCapture.CaptureScreenshot(Path.GetFullPath("Captures/tank2145-"+names[phase]+".png"));capturePending=true;next=EditorApplication.timeSinceStartup+1;return;}
  capturePending=false;
  phase++;
  if(phase>=names.Length){EditorApplication.update-=Update;EditorApplication.delayCall+=()=>{g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.recklessTrafficEnabled=flags[3];g.trafficSurprises=flags[4];g.turnExitTrafficEnabled=flags[5];g.ResetRide();selectedViewSize.SetValue(oldView,oldViewSize);Time.timeScale=scale;};return;}
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
