using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Đo chi phí từng phần khu sông/cầu (Play Mode, vSync tắt, khung hình thật; huongdan.md 21.54): người chơi giữa cầu.
// Các biến thể chạy xen kẽ ROUNDS vòng × 60 khung (bỏ 15 khung đầu mỗi lần), gộp trung vị theo biến thể để bớt trôi do máy bận.
// Ghi Captures/river-water-cost.json (ms/khung; "delta" = chênh với baseline).
public static class ProbeWaterCost
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;const int ROUNDS=4,FRAMES=60,WARM=15;
 static List<(string name,Action on,Action off)> variants;static int index,frame,round;
 static Dictionary<string,List<float>> samples=new Dictionary<string,List<float>>();
 static int vsync,target;static float scale;static PhoVietGame g;
 public static object Main()
 {
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  scale=Time.timeScale;Time.timeScale=0;g.riversEnabled=true;g.tankBarrageEnabled=false;g.RestartRide();
  foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(Mathf.Abs(b.localPosition.z-80)<1);
  for(int i=0;i<60*6;i++){typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{1/60f});if(g.Distance>100)break;}
  typeof(PhoVietGame).GetMethod("PositionRideCamera",F).Invoke(g,new object[]{true,0f});
  var mirror=g.rideCamera.GetComponent<RiverReflection>();
  var lods=g.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("PV_RiverBridge_LOD")).ToArray();
  var aptRends=g.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.StartsWith("PV_RiverApt")).SelectMany(l=>l.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
  var bridgeRoot=g.blocks.First(b=>b.GetComponent<RiverBlock>().IsBridge).GetComponent<RiverBlock>().bridge;
  var waterMat=lods[0].sharedMaterials.First(m=>m.shader.name=="PhoViet/River Water Mirror");
  var murkMat=lods[0].sharedMaterials.First(m=>m.shader.name=="PhoViet/River Murk");
  Shader waterShader=waterMat.shader,murkShader=murkMat.shader,unlit=Shader.Find("Unlit/Color");
  Action<MeshRenderer[],UnityEngine.Rendering.ShadowCastingMode> cast=(rs,m)=>{foreach(var r in rs)r.shadowCastingMode=m;};
  variants=new List<(string,Action,Action)>
  {
   ("baseline",()=>{},()=>{}),
   ("mirror off",()=>mirror.enabled=false,()=>mirror.enabled=true),
   ("water shader flat",()=>waterMat.shader=unlit,()=>waterMat.shader=waterShader),
   ("murk shader flat",()=>murkMat.shader=unlit,()=>murkMat.shader=murkShader),
   ("deck+water no shadow cast",()=>cast(lods,UnityEngine.Rendering.ShadowCastingMode.Off),()=>cast(lods,UnityEngine.Rendering.ShadowCastingMode.On)),
   ("deck+water hidden",()=>{foreach(var r in lods)r.enabled=false;},()=>{foreach(var r in lods)r.enabled=true;}),
   ("apartments no shadow cast",()=>cast(aptRends,UnityEngine.Rendering.ShadowCastingMode.Off),()=>cast(aptRends,UnityEngine.Rendering.ShadowCastingMode.On)),
   ("apartments hidden",()=>{foreach(var r in aptRends)r.enabled=false;},()=>{foreach(var r in aptRends)r.enabled=true;}),
   ("whole bridge hidden",()=>bridgeRoot.SetActive(false),()=>bridgeRoot.SetActive(true)),
  };
  samples.Clear();foreach(var v in variants)samples[v.name]=new List<float>();
  vsync=QualitySettings.vSyncCount;target=Application.targetFrameRate;QualitySettings.vSyncCount=0;Application.targetFrameRate=1000;
  index=0;frame=0;round=0;
  UnityEditor.EditorApplication.update-=Step;UnityEditor.EditorApplication.update+=Step;
  return new{success=true,distance=g.Distance,screen=Screen.width+"x"+Screen.height};
 }
 static void Step()
 {
  var v=variants[index];
  if(frame==0)v.on();
  if(frame>=WARM)samples[v.name].Add(Time.unscaledDeltaTime*1000);
  frame++;
  if(frame<FRAMES)return;
  v.off();frame=0;index++;
  if(index<variants.Count)return;
  index=0;round++;
  if(round<ROUNDS)return;
  UnityEditor.EditorApplication.update-=Step;QualitySettings.vSyncCount=vsync;Application.targetFrameRate=target;Time.timeScale=scale;
  float Med(List<float> l){var s=l.OrderBy(x=>x).ToList();return s[s.Count/2];}
  var inv=System.Globalization.CultureInfo.InvariantCulture;float b=Med(samples["baseline"]);
  var parts=variants.Select(x=>"\""+x.name+"\": {\"ms\": "+Med(samples[x.name]).ToString("F2",inv)+", \"delta\": "+(Med(samples[x.name])-b).ToString("F2",inv)+"}");
  System.IO.File.WriteAllText("Captures/river-water-cost.json","{\"screen\":\""+Screen.width+"x"+Screen.height+"\",\"rounds\":"+ROUNDS+","+string.Join(",",parts)+"}\n");
 }
}
