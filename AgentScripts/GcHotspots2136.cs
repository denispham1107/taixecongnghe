using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditorInternal;using UnityEditor.Profiling;using PhoViet;
// 21.136: which methods allocate per frame while riding, from a deep-profile capture. Arm in Edit Mode, then Play, then Start.
public static class GcHotspots2136
{
 public static object Arm(){if(Application.isPlaying)throw new Exception("Edit Mode");ProfilerDriver.deepProfiling=true;return ProfilerDriver.deepProfiling;}
 public static object Disarm(){ProfilerDriver.deepProfiling=false;ProfilerDriver.enabled=false;return "off";}
 static int frames;static int first;
 public static object Start()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();g.forceTouchGraphics=true;g.SetGraphicsQuality(2);g.RestartRide();ProfilerDriver.ClearAllFrames();UnityEngine.Profiling.Profiler.enabled=true;ProfilerDriver.enabled=true;ProfilerDriver.profileEditor=false;frames=0;first=-1;
  EditorApplication.update-=Tick;EditorApplication.update+=Tick;return "started";
 }
 static int last=-1;
 static void Tick()
 {
  if(!Application.isPlaying){EditorApplication.update-=Tick;return;}if(Time.frameCount==last)return;last=Time.frameCount;
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g.State!=PhoVietGame.RideState.Riding)g.RestartRide();
  if(++frames<220)return;EditorApplication.update-=Tick;ProfilerDriver.enabled=false;
  var sum=new Dictionary<string,double[]>();int count=0;
  int end=ProfilerDriver.lastFrameIndex,start=Math.Max(ProfilerDriver.firstFrameIndex,end-90);
  for(int f=start;f<=end;f++)
  {
   using var view=ProfilerDriver.GetHierarchyFrameDataView(f,0,HierarchyFrameDataView.ViewModes.Default,HierarchyFrameDataView.columnGcMemory,false);if(view==null||!view.valid)continue;count++;
   var stack=new Stack<(int id,string path)>();stack.Push((view.GetRootItemID(),""));var kids=new List<int>();
   while(stack.Count>0)
   {
    var (id,path)=stack.Pop();double gc=view.GetItemColumnDataAsDouble(id,HierarchyFrameDataView.columnGcMemory);
    string name=view.GetItemName(id);view.GetItemChildren(id,kids);double childGc=0;var list=kids.ToArray();foreach(int k in list)childGc+=view.GetItemColumnDataAsDouble(k,HierarchyFrameDataView.columnGcMemory);
    string here=path+"/"+name;double self=gc-childGc;
    if(!sum.TryGetValue(name,out var s))sum[name]=s=new double[5];s[0]+=gc;s[1]+=self;s[2]+=view.GetItemColumnDataAsDouble(id,HierarchyFrameDataView.columnCalls);s[3]+=view.GetItemColumnDataAsDouble(id,HierarchyFrameDataView.columnSelfTime);s[4]+=view.GetItemColumnDataAsDouble(id,HierarchyFrameDataView.columnTotalTime);
    
    foreach(int k in list)stack.Push((k,here));
   }
  }
  var rows=sum.OrderByDescending(p=>p.Value[4]).Take(90).Select(p=>new{p.Key,gc=Math.Round(p.Value[0]/count),calls=Math.Round(p.Value[2]/count,1),selfMs=Math.Round(p.Value[3]/count,3),totalMs=Math.Round(p.Value[4]/count,3)}).ToArray();
  File.WriteAllText("Captures/perf2136-gc.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{frames=count,rows},Newtonsoft.Json.Formatting.Indented));
 }
}
