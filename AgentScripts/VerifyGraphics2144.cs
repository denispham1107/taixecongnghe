using System;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyGraphics2144
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;const string Auto="PhoViet.GraphicsAutoDrop.v1";
 static PhoVietGame g;static List<string> checks;static int originalVSync;
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
 static void Check(bool b,string n){if(!b)throw new Exception(n);checks.Add(n);}
 static void Reload(int value,int drop,bool touch)
 {Call("RestoreGraphicsBaseline");QualitySettings.vSyncCount=originalVSync;g.forceTouchGraphics=touch;PlayerPrefs.SetInt(PhoVietGame.GraphicsPreferenceKey,value);PlayerPrefs.SetInt(Auto,drop);Set("graphicsInitialized",false);Call("InitializeGraphicsSettings");}
 static void SlowWindow(float median)
 {g.CloseGraphicsSettings();g.StartRide();Time.timeScale=1;var f=(float[])typeof(PhoVietGame).GetField("governorSamples",F).GetValue(g);for(int i=0;i<f.Length;i++)f[i]=median;Set("governorCount",44);Set("governorElapsed",1.3f);Call("TickGraphicsGovernor");}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();
  float time=Time.timeScale;originalVSync=QualitySettings.vSyncCount;bool touch=g.forceTouchGraphics,force=g.forceGraphicsGovernor;int selected=(int)g.SelectedGraphics;
  bool had=PlayerPrefs.HasKey(PhoVietGame.GraphicsPreferenceKey),hadAuto=PlayerPrefs.HasKey(Auto);int saved=PlayerPrefs.GetInt(PhoVietGame.GraphicsPreferenceKey),auto=PlayerPrefs.GetInt(Auto);
  try
  {
   g.ResetRide();Time.timeScale=0;
   var order=new[]{PhoVietGame.GraphicsLevel.VeryHigh,PhoVietGame.GraphicsLevel.High,PhoVietGame.GraphicsLevel.Medium,PhoVietGame.GraphicsLevel.Low};
   Check((int)order[0]==0&&(int)order[2]==1&&(int)order[3]==2&&(int)order[1]==3,"v1 preference IDs retain the old choices");
   for(int i=0;i<4;i++)Check(PhoVietGame.GraphicsLevelAt(i)==order[i]&&PhoVietGame.GraphicsRank(order[i])==i,"Display and governor rank "+i);
   Check(PhoVietGame.GraphicsName(order[0])=="Rất cao"&&PhoVietGame.GraphicsName(order[1])=="Cao","Distinct Vietnamese names");
   var vh=PhoVietGame.PlanFor(order[0]);var high=PhoVietGame.PlanFor(order[1]);var medium=PhoVietGame.PlanFor(order[2]);
   Check(vh.pixelCap==2&&vh.pixelFloor==1.5f&&vh.lodBias==.8f&&vh.shadowDistance==30&&vh.far==165&&vh.smallPropCull==110&&vh.reflectionSize==512&&vh.reflectionScale==.33f,"VeryHigh retains the previous mobile High plan");
   Check(high.pixelCap>medium.pixelCap&&high.pixelCap<vh.pixelCap&&high.lodBias>medium.lodBias&&high.lodBias<vh.lodBias,"High pixel and detail budgets lie strictly between Medium and VeryHigh");
   Check(high.far>medium.far&&high.far<vh.far&&high.shadowDistance>medium.shadowDistance&&high.shadowDistance<vh.shadowDistance,"High view and shadow distances lie between adjacent levels");
   Check(high.reflectionSize>medium.reflectionSize&&high.reflectionSize<vh.reflectionSize&&high.reflectionScale>medium.reflectionScale&&high.reflectionScale<vh.reflectionScale,"High reflections lie between adjacent levels");
   foreach(bool mobile in new[]{false,true})
   {
    foreach(int id in new[]{0,1,2,3,99})
    {
     Reload(id,0,mobile);Check((int)g.SelectedGraphics==(id==99?0:id),"Reload selected ID "+id+" touch="+mobile);
    }
    Reload(0,2,mobile);Check(g.SelectedGraphics==order[0]&&g.EffectiveGraphics==order[0],"Old High ignores persisted learned downgrade, touch="+mobile);
    float cap=g.RenderPixelCap,lod=QualitySettings.lodBias,far=g.rideCamera.farClipPlane;int steps=g.GraphicsGovernorSteps,vs=QualitySettings.vSyncCount;
    g.forceGraphicsGovernor=true;g.SetGraphicsGovernor(1);
    for(int i=0;i<20;i++){SlowWindow(.08f);Call("GovernorStepDown");Call("GovernorStepUp");SlowWindow(.008f);}
    Check(g.SelectedGraphics==order[0]&&g.EffectiveGraphics==order[0]&&g.RenderPixelCap==cap&&g.GraphicsGovernorSteps==steps,"VeryHigh never changes level or pixel cap, touch="+mobile);
    Check(QualitySettings.vSyncCount==vs&&QualitySettings.lodBias==lod&&g.rideCamera.farClipPlane==far,"VeryHigh does not auto-change refresh or scene detail, touch="+mobile);
    g.forceGraphicsGovernor=false;Reload(3,0,mobile);g.SetGraphicsGovernor(1);g.ResetRide();SlowWindow(.08f);
    Check(g.GraphicsGovernorSteps>steps&&g.RenderPixelCap<high.pixelCap,"High auto-reduces in a slow sampled window without a test override, touch="+mobile);
    var seen=new HashSet<PhoVietGame.GraphicsLevel>();for(int i=0;i<20;i++){seen.Add(g.EffectiveGraphics);Call("GovernorStepDown");}
    Check(seen.Contains(order[2])&&seen.Contains(order[3])&&g.EffectiveGraphics==order[3]&&g.SelectedGraphics==order[1],"High degrades through Medium to Low without changing selection");
    Check(g.RenderPixelCap>=1&&PlayerPrefs.GetInt(Auto)==2,"Learned drop is quality ranks, not raw enum subtraction");
    QualitySettings.vSyncCount=originalVSync;for(int i=0;i<30;i++)Call("GovernorStepUp");
    Check(g.EffectiveGraphics==order[1]&&Mathf.Abs(g.RenderPixelCap-high.pixelCap)<.001f,"Recovery stops at chosen High, never enters VeryHigh");
    Reload(3,1,mobile);Check(g.SelectedGraphics==order[1]&&g.EffectiveGraphics==order[2],"New High reloads a learned drop to Medium");
    g.SetGraphicsQuality(0);Check(g.EffectiveGraphics==order[0]&&PlayerPrefs.GetInt(Auto)==0&&QualitySettings.vSyncCount==originalVSync,"Selecting VeryHigh restores its fixed baseline and refresh");
   }
   foreach(var size in new[]{new Vector2(480,1040),new Vector2(480,700),new Vector2(1440,900),new Vector2(1280,420),new Vector2(667,375)})
   {
    var w=PhoVietGame.GraphicsWindowRect(size.x,size.y,size.x<size.y);var done=PhoVietGame.GraphicsDoneRect(w);Rect previous=new Rect();
    for(int i=0;i<4;i++){var r=PhoVietGame.GraphicsChoiceRect(w,i);Check(w.Contains(r.min)&&w.Contains(r.max)&&!r.Overlaps(done)&&(i==0||!r.Overlaps(previous)),"Four separate rows inside modal "+size+" row="+i);previous=r;}
   }
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased&&!PhoVietGame.BoostReleased,"Jobs and boost stay disabled");
   var report=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/graphics2144-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.count};
  }
  finally
  {
   g.forceTouchGraphics=touch;g.forceGraphicsGovernor=force;g.SetGraphicsGovernor(1);g.ResetRide();g.SetGraphicsQuality(selected);
   if(had)PlayerPrefs.SetInt(PhoVietGame.GraphicsPreferenceKey,saved);else PlayerPrefs.DeleteKey(PhoVietGame.GraphicsPreferenceKey);if(hadAuto)PlayerPrefs.SetInt(Auto,auto);else PlayerPrefs.DeleteKey(Auto);PlayerPrefs.Save();QualitySettings.vSyncCount=originalVSync;Time.timeScale=time;
  }
 }
}
