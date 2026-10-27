using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyGraphicsSettings
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g.State!=PhoVietGame.RideState.Ready)throw new Exception("Ready screen required");
  bool had=PlayerPrefs.HasKey(PhoVietGame.GraphicsPreferenceKey),wasOpen=g.GraphicsSettingsOpen;int pref=PlayerPrefs.GetInt(PhoVietGame.GraphicsPreferenceKey),original=(int)g.SelectedGraphics;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=g.BestDistance;var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  var t=typeof(PhoVietGame);var oldInsets=t.GetField("browserInsets",F).GetValue(g);bool hadInsets=(bool)t.GetField("hasBrowserInsets",F).GetValue(g);float time=Time.timeScale;
  var reflection=g.rideCamera.GetComponent<RiverReflection>();int meshCount=g.scenery.GetComponentsInChildren<Renderer>(true).Length;float fov=g.rideCamera.fieldOfView,far=g.rideCamera.farClipPlane;int textureLimit=QualitySettings.globalTextureMipmapLimit;
  try
  {
   Time.timeScale=0;g.SetGraphicsQuality(0);int aa=QualitySettings.antiAliasing,cascades=QualitySettings.shadowCascades;float distance=QualitySettings.shadowDistance,lod=QualitySettings.lodBias,scale=reflection.resolution,reflectFar=reflection.farClip;int size=reflection.maxSize,interval=reflection.interval;var shadows=QualitySettings.shadows;var shadowResolution=QualitySettings.shadowResolution;
   Action<string> high=label=>check(QualitySettings.antiAliasing==aa&&QualitySettings.shadows==shadows&&QualitySettings.shadowDistance==distance&&QualitySettings.shadowResolution==shadowResolution&&QualitySettings.shadowCascades==cascades&&QualitySettings.lodBias==lod&&reflection.resolution==scale&&reflection.maxSize==size&&reflection.interval==interval&&reflection.farClip==reflectFar,label);
   foreach(int level in new[]{1,2,0,2,1,0})
   {
    g.SetGraphicsQuality(level);check((int)g.SelectedGraphics==level&&PlayerPrefs.GetInt(PhoVietGame.GraphicsPreferenceKey,-1)==level,"Selected level and persistent preference "+level);
    if(level==0)high("High restores every captured platform/reflection parameter");
    else
    {
     check(QualitySettings.antiAliasing==(level==1?Mathf.Min(aa,2):0),"MSAA responds to level "+level);
     check(Mathf.Abs(QualitySettings.shadowDistance-(level==1?distance*.65f:0))<.0001f&&QualitySettings.shadows==(level==2?ShadowQuality.Disable:shadows==ShadowQuality.Disable?ShadowQuality.Disable:ShadowQuality.HardOnly),"Real shadows respond to level "+level);
     check(Mathf.Abs(QualitySettings.lodBias-lod*(level==1?.75f:.5f))<.0001f,"Distant mesh LOD responds to level "+level);
     check(reflection.interval>= (level==1?2:3)&&reflection.maxSize<= (level==1?512:384)&&Mathf.Abs(reflection.resolution-scale*(level==1?.75f:.5f))<.0001f,"River rendering budget responds to level "+level);
    }
    check(QualitySettings.globalTextureMipmapLimit==textureLimit&&g.rideCamera.fieldOfView==fov&&g.rideCamera.farClipPlane==far,"Texture detail, camera and view distance unchanged");
    check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==meshCount&&!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Scenery population and locked jobs unchanged");
   }
   g.SetGraphicsQuality(0);g.SetGraphicsQuality(-1);g.SetGraphicsQuality(99);check(g.SelectedGraphics==PhoVietGame.GraphicsLevel.High&&PlayerPrefs.GetInt(PhoVietGame.GraphicsPreferenceKey)==0,"Out-of-range settings rejected");
   // Simulate a new session on the same platform, starting from its unmodified High baseline.
   foreach(int level in new[]{1,2,0,77})
   {
    g.SetGraphicsQuality(0);PlayerPrefs.SetInt(PhoVietGame.GraphicsPreferenceKey,level);t.GetField("graphicsInitialized",F).SetValue(g,false);t.GetMethod("InitializeGraphicsSettings",F).Invoke(g,null);
    check((int)g.SelectedGraphics==(level==77?0:level),"Saved preference reload / invalid preference fallback "+level);
   }
   g.SetGraphicsQuality(0);high("Baseline capture does not drift after preferences reload");
   foreach(var dimensions in new[]{new Vector2(480,1040),new Vector2(1440,900),new Vector2(1600,900),new Vector2(480,700)})
   {
    bool portrait=dimensions.x<dimensions.y;var card=PhoVietGame.WelcomeCardRect(dimensions.x,dimensions.y,portrait);var gear=PhoVietGame.WelcomeSettingsRect(card);var window=PhoVietGame.GraphicsWindowRect(dimensions.x,dimensions.y,portrait);
    check(gear.width==44&&gear.height==44&&card.Contains(gear.min)&&card.Contains(gear.max-Vector2.one),"Gear has a full touch target in "+dimensions);
    check(!gear.Overlaps(PhoVietGame.WelcomeStartRect(card)),"Gear cannot hit start button in "+dimensions);
    check(window.x>=16&&window.y>=16&&window.xMax<=dimensions.x-16&&window.yMax<=dimensions.y-16,"Window stays within viewport "+dimensions);
    for(int i=0;i<3;i++)check(window.Contains(PhoVietGame.GraphicsChoiceRect(window,i).center)&&!PhoVietGame.GraphicsDoneRect(window).Overlaps(PhoVietGame.GraphicsChoiceRect(window,i)),"Separate visible option "+i+" in "+dimensions);
   }
   foreach(bool inset in new[]{false,true})
   {
    t.GetField("hasBrowserInsets",F).SetValue(g,inset);t.GetField("browserInsets",F).SetValue(g,new Vector4(.025f,.045f,.025f,.035f));var area=(Rect)t.GetMethod("GameGuiArea",F).Invoke(g,null);bool portrait=Screen.width<Screen.height;float uiScale=portrait?area.width/480f:Mathf.Min(area.width/1440f,area.height/900f),w=area.width/uiScale,h=area.height/uiScale;
    Action<Vector2> tap=p=>g.ApplyHudTap(JsonUtility.ToJson(new Tap{x=(area.x+p.x*uiScale)/Screen.width,y=(area.y+p.y*uiScale)/Screen.height}));
    g.CloseGraphicsSettings();tap(PhoVietGame.WelcomeSettingsRect(PhoVietGame.WelcomeCardRect(w,h,portrait)).center);check(g.GraphicsSettingsOpen&&g.State==PhoVietGame.RideState.Ready,"Browser gear tap opens only settings / safe area="+inset);
    g.StartRide();check(g.State==PhoVietGame.RideState.Ready,"Start API is blocked by modal");
    for(int i=0;i<3;i++){tap(PhoVietGame.GraphicsChoiceRect(PhoVietGame.GraphicsWindowRect(w,h,portrait),i).center);check((int)g.SelectedGraphics==i&&g.GraphicsSettingsOpen&&g.State==PhoVietGame.RideState.Ready,"Browser tap selects only option "+i+" / safe area="+inset);}
    tap(new Vector2(1,1));check(!g.GraphicsSettingsOpen&&g.State==PhoVietGame.RideState.Ready,"Outside tap closes without clicking through");
    g.OpenGraphicsSettings();tap(PhoVietGame.GraphicsDoneRect(PhoVietGame.GraphicsWindowRect(w,h,portrait)).center);check(!g.GraphicsSettingsOpen&&g.State==PhoVietGame.RideState.Ready,"Done closes without starting ride");
    g.OpenGraphicsSettings();tap(PhoVietGame.GraphicsCloseRect(PhoVietGame.GraphicsWindowRect(w,h,portrait)).center);check(!g.GraphicsSettingsOpen,"Close button works through browser bridge");
   }
   g.SetGraphicsQuality(2);g.ResetRide();check(g.SelectedGraphics==PhoVietGame.GraphicsLevel.Low&&!g.GraphicsSettingsOpen,"Restart preserves graphics level");g.StartRide();g.OpenGraphicsSettings();check(g.State==PhoVietGame.RideState.Riding&&!g.GraphicsSettingsOpen,"Welcome settings do not open during gameplay");g.ResetRide();
   check(PlayerPrefs.GetInt("PhoViet.Coins.v1",0)==coins&&g.BestDistance==best,"Coins and record preserved");
   var report=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/graphics-settings-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally
  {
   g.ResetRide();g.SetGraphicsQuality(original);if(had)PlayerPrefs.SetInt(PhoVietGame.GraphicsPreferenceKey,pref);else PlayerPrefs.DeleteKey(PhoVietGame.GraphicsPreferenceKey);PlayerPrefs.Save();
   t.GetField("browserInsets",F).SetValue(g,oldInsets);t.GetField("hasBrowserInsets",F).SetValue(g,hadInsets);if(wasOpen)g.OpenGraphicsSettings();Time.timeScale=time;
  }
 }
 [Serializable]sealed class Tap{public float x,y;}
}
