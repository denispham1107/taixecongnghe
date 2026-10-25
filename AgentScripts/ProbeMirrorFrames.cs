using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Đo thời gian khung hình thật (Play Mode, vSync tắt) khi có sông trong khung nhìn: xen kẽ bật/tắt RiverReflection
// mỗi 90 khung (bỏ 20 khung đầu mỗi pha), ghi Captures/river-mirror-frames.json (huongdan.md 21.46).
public static class ProbeMirrorFrames
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static List<float> on=new List<float>(),off=new List<float>();static int frame;static RiverReflection mirror;static int vsync,target;static float scale;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  scale=Time.timeScale;Time.timeScale=0;g.tankBarrageEnabled=false;g.RestartRide();
  foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(Mathf.Abs(b.localPosition.z-80)<1);
  for(int i=0;i<60*4;i++){typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{1/60f});if(g.Distance>70)break;}
  mirror=g.rideCamera.GetComponent<RiverReflection>();vsync=QualitySettings.vSyncCount;target=Application.targetFrameRate;
  QualitySettings.vSyncCount=0;Application.targetFrameRate=1000;on.Clear();off.Clear();frame=0;
  UnityEditor.EditorApplication.update-=Step;UnityEditor.EditorApplication.update+=Step;
  return new{success=true,distance=g.Distance};
 }
 static void Step()
 {
  int phase=frame/90,inPhase=frame%90;bool enabled=phase%2==0;
  if(inPhase==0)mirror.enabled=enabled;
  if(inPhase>=20)(enabled?on:off).Add(Time.unscaledDeltaTime*1000);
  frame++;
  if(phase>=8)
  {
   UnityEditor.EditorApplication.update-=Step;mirror.enabled=true;QualitySettings.vSyncCount=vsync;Application.targetFrameRate=target;Time.timeScale=scale;
   float Med(List<float> l){var s=l.OrderBy(x=>x).ToList();return s[s.Count/2];}
   System.IO.File.WriteAllText("Captures/river-mirror-frames.json","{\"onMedianMs\":"+Med(on).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+",\"offMedianMs\":"+Med(off).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+",\"onFrames\":"+on.Count+",\"offFrames\":"+off.Count+"}\n");
  }
 }
}
