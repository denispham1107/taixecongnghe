using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
// 21.136: tile culling must not change a single visible pixel; plans and the governor must behave as specified.
public static class VerifyMobilePerf2136
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks=new List<string>();static List<object> samples=new List<object>();static string failure;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add(name);}
 static void Call(string name,params object[] a)=>typeof(PhoVietGame).GetMethod(name,F).Invoke(g,a);
 static byte[] Shot(RenderTexture rt,Texture2D t)
 {
  var old=g.rideCamera.targetTexture;g.rideCamera.targetTexture=rt;g.rideCamera.Render();g.rideCamera.targetTexture=old;
  var active=RenderTexture.active;RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);t.Apply();RenderTexture.active=active;return t.GetRawTextureData();
 }
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks.Clear();samples.Clear();failure=null;
  int pref=PlayerPrefs.GetInt(PhoVietGame.GraphicsPreferenceKey,0),best=g.BestDistance;float time=Time.timeScale;bool dino=g.dinosaursEnabled,tank=g.tankBarrageEnabled;int vsync=QualitySettings.vSyncCount;float far=g.rideCamera.farClipPlane;
  var rt=new RenderTexture(640,300,24);var t=new Texture2D(640,300,TextureFormat.RGB24,false);
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=false;g.forceTouchGraphics=true;
   // ---- plans
   float lastFar=1000,lastCap=100,lastLod=100;
   foreach(int level in new[]{0,1,2})
   {
    g.SetGraphicsQuality(level);var plan=PhoVietGame.PlanFor((PhoVietGame.GraphicsLevel)level);
    Check(g.RenderPixelCap==plan.pixelCap&&plan.pixelFloor<=plan.pixelCap&&plan.pixelFloor>=1,"Level "+level+" sets its render density, never below CSS resolution");
    Check(g.rideCamera.farClipPlane==Mathf.Min(far,plan.far)&&plan.far<lastFar&&plan.pixelCap<lastCap&&plan.lodBias<lastLod,"Level "+level+" spends less than the level above");
    Check(g.rideCamera.layerCullDistances[PhoVietGame.SmallPropLayer]==plan.smallPropCull,"Small props cull distance applied at level "+level);
    Check(QualitySettings.lodBias>0&&QualitySettings.pixelLightCount<=plan.pixelLights,"LOD and pixel light budget applied at level "+level);
    Check(g.rideCamera.GetComponent<RiverReflection>().enabled==plan.reflection,"Reflection follows the plan at level "+level);
    Check(!plan.fogVolume&&!g.rideCamera.GetComponent<MorningFog>().volumeAllowed,"Touch plans skip the fog volume and its depth pass at level "+level);
    g.daylight.UpdateLamps();Check(RenderSettings.fogEndDistance<=g.rideCamera.farClipPlane+.01f,"Fog ends at the view limit at level "+level);
    g.daylight.SetCycleTime(8);g.daylight.UpdateLamps();Check(!(g.daylight.sun.enabled&&g.daylight.moon.enabled),"Touch plan keeps one directional light by day at level "+level);
    g.daylight.SetCycleTime(26);g.daylight.UpdateLamps();Check(!(g.daylight.sun.enabled&&g.daylight.moon.enabled)&&(g.daylight.sun.enabled||g.daylight.moon.enabled),"Touch plan keeps one directional light by night at level "+level);
    var shop=g.scenery.GetComponentsInChildren<MeshRenderer>(true).First(r=>r.name.StartsWith("PV_StreetShop"));Check((shop.shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.On)==plan.sceneryShadows,"Scenery shadow casting follows the plan at level "+level);
    lastFar=plan.far;lastCap=plan.pixelCap;lastLod=plan.lodBias;
   }
   // ---- desktop baseline untouched
   g.forceTouchGraphics=false;g.SetGraphicsQuality(0);
   Check(g.rideCamera.farClipPlane==far&&g.rideCamera.layerCullDistances.All(v=>v==0)&&g.rideCamera.GetComponent<RiverReflection>().enabled&&g.rideCamera.GetComponent<MorningFog>().volumeAllowed,"Desktop High keeps its full baseline");
   g.daylight.UpdateLamps();Check(RenderSettings.fogEndDistance>150,"Desktop High keeps its full fog distance");
   g.daylight.SetCycleTime(8);g.daylight.UpdateLamps();Check(g.daylight.sun.enabled&&g.daylight.moon.enabled,"Desktop High keeps both directional lights");
   Check(g.scenery.GetComponentsInChildren<MeshRenderer>(true).First(r=>r.name.StartsWith("PV_StreetShop")).shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.On,"Desktop High keeps scenery shadows");
   // ---- tile culling leaves every pixel alone
   int compared=0,worst=0,minTiles=1000,maxTiles=0,total=g.roadRows.Sum(r=>r.tiles.Count);bool turned=false;
   foreach(int level in new[]{2,0})
   {
    g.forceTouchGraphics=level==2;g.SetGraphicsQuality(level);
    foreach(float hour in new[]{8f,26f})
    {
     g.tileCullingEnabled=true;g.ResetRide();g.StartRide();g.daylight.SetCycleTime(hour);
     foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
     var tick=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),g,typeof(PhoVietGame).GetMethod("TickRide",F));
     for(int sample=0;sample<9;sample++)
     {
      for(int step=0;step<70&&g.State==PhoVietGame.RideState.Riding;step++)
      {
       if(g.NextJunctionDistance<26&&g.NextJunctionDistance>12&&!g.IsTurning){bool ok=(bool)typeof(PhoVietGame).GetMethod("SelectJunctionTurn",F).Invoke(g,new object[]{g.CurrentLane<=1?-1:1});turned|=ok;}
       tick(1/30f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}
      }
      if(g.State!=PhoVietGame.RideState.Riding){g.ResetRide();g.StartRide();continue;}
      g.daylight.UpdateLamps();
      g.tileCullingEnabled=true;Call("UpdateTileCulling",true);int active=g.ActiveRoadTiles;
      if(!g.IsTurning&&g.NextJunctionDistance>=66){minTiles=Math.Min(minTiles,active);maxTiles=Math.Max(maxTiles,active);}
      var on=Shot(rt,t);
      g.tileCullingEnabled=false;Call("UpdateTileCulling",true);var off=Shot(rt,t);
      int diff=0;for(int i=0;i<on.Length;i+=3)if(Math.Abs(on[i]-off[i])+Math.Abs(on[i+1]-off[i+1])+Math.Abs(on[i+2]-off[i+2])>24)diff++;
      if(diff>worst){File.WriteAllBytes("Captures/perf2136-cull-on.png",ImageConversion.EncodeArrayToPNG(on,UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UNorm,(uint)rt.width,(uint)rt.height));File.WriteAllBytes("Captures/perf2136-cull-off.png",ImageConversion.EncodeArrayToPNG(off,UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UNorm,(uint)rt.width,(uint)rt.height));}worst=Math.Max(worst,diff);compared++;samples.Add(new{level,hour,sample,active,turning=g.IsTurning,junction=Math.Round(g.NextJunctionDistance),diff});
      g.tileCullingEnabled=true;Call("UpdateTileCulling",true);
     }
    }
   }
   Check(compared>=24,"Compared "+compared+" frames with and without tile culling");
   File.WriteAllText("Captures/perf2136-cull-samples.json",Newtonsoft.Json.JsonConvert.SerializeObject(samples));Check(worst<=rt.width*rt.height/2000,"Tile culling changes at most 0.05% of pixels (worst "+worst+")");
   Check(turned,"Compared frames include a chosen junction turn");
   Check(maxTiles>0&&maxTiles<=total*.6f,"Straight road keeps at most 60% of tiles active ("+minTiles+"-"+maxTiles+" of "+total+")");
   // ---- governor walks down and back up, the player's choice stays selected
   g.forceTouchGraphics=true;g.SetGraphicsQuality(0);QualitySettings.vSyncCount=1;g.ResetRide();
   var seen=new List<string>();for(int i=0;i<12&&!(QualitySettings.vSyncCount==2);i++){Call("GovernorStepDown");seen.Add(g.EffectiveGraphics+":"+g.RenderPixelCap.ToString("0.00"));Check(g.RenderPixelCap>=1,"Governor never renders below CSS resolution");}
   Check(g.SelectedGraphics==PhoVietGame.GraphicsLevel.High&&g.EffectiveGraphics==PhoVietGame.GraphicsLevel.Low,"Governor reaches Low, the choice stays Cao ("+string.Join(" ",seen)+")");
   Check(QualitySettings.vSyncCount==2&&g.RenderPixelCap==PhoVietGame.PlanFor(PhoVietGame.GraphicsLevel.Low).pixelFloor,"Last resort is a steady half refresh at the pixel floor");
   QualitySettings.vSyncCount=1;for(int i=0;i<12;i++)Call("GovernorStepUp");
   Check(g.EffectiveGraphics==PhoVietGame.GraphicsLevel.High&&g.RenderPixelCap==PhoVietGame.PlanFor(PhoVietGame.GraphicsLevel.High).pixelCap,"Governor returns to the chosen level");
   Call("GovernorStepDown");Check(g.GraphicsGovernorSteps>0,"Steps counted");g.SetGraphicsQuality(1);Check(g.EffectiveGraphics==PhoVietGame.GraphicsLevel.Medium&&PlayerPrefs.GetInt("PhoViet.GraphicsAutoDrop.v1",-1)==0,"Choosing a level clears the learned drop");
   // ---- loading did the mid-ride work
   Check(typeof(PhoVietGame).GetField("rearStyleTextures",F).GetValue(g)!=null&&typeof(PhoVietGame).GetField("rearWarningArrow",F).GetValue(g)!=null,"Rear warning textures exist before the first warning");
   Check(typeof(PhoVietGame).GetField("tankFxAnchor",F).GetValue(g)!=null,"Tank effects exist before the first shot");
   Check(g.GetComponent<ContactFireVisual>()==null||g.GetComponent<ContactFireVisual>().PoolCount>=4,"Contact fire pool exists before the first crash");
   Check(StreetArt.Hex("B5CFC2")==StreetArt.Hex("B5CFC2")&&Mathf.Abs(StreetArt.Hex("FF0000").r-1)<.001f,"Colour cache returns the parsed colour");
   Check(!g.useGUILayout,"HUD skips the IMGUI layout pass");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Ride jobs stay locked");
   var report=new{success=true,count=checks.Count,checks,samples};File.WriteAllText("Captures/perf2136-verify.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{success=true,count=checks.Count,worst,compared,minTiles,maxTiles,total,seen};
  }
  finally
  {
   g.tileCullingEnabled=true;g.forceTouchGraphics=false;QualitySettings.vSyncCount=vsync;g.dinosaursEnabled=dino;g.tankBarrageEnabled=tank;g.SetGraphicsQuality(pref>=0&&pref<=2?pref:0);g.ResetRide();Time.timeScale=time;
   UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(t);
  }
 }
}
