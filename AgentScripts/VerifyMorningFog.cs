using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class VerifyMorningFog
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var l=g.daylight;var fog=l.morningFog;var cam=g.rideCamera;
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
  Action reset=()=>{g.jobsEnabled=false;g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;g.RestartRide();};
  RenderTexture rt=null;Texture2D texture=null;var previousTarget=cam.targetTexture;var previousActive=RenderTexture.active;
  try
  {
   Time.timeScale=0;reset();
   check(fog!=null&&fog.authoredProfile.text.Contains("Blender MCP"),"Fog loads its volume and six-second profile authored through Blender MCP");
   check(fog.densityAtlas.width==272&&fog.densityAtlas.height==136&&fog.densityAtlas.mipmapCount==1,"Padded atlas keeps all 32 density slices without NPOT scaling or mip leakage");
   var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(fog.densityAtlas));
   check(!importer.sRGBTexture&&importer.textureCompression==TextureImporterCompression.Uncompressed,"Density is linear uncompressed data rather than an sRGB picture");
   check(!ShaderUtil.ShaderHasError(fog.volumeShader),"Volume integration shader compiles without errors");
   check(fog.enabled&&fog.Strength==1&&(cam.depthTextureMode&DepthTextureMode.Depth)!=0,"Each new ride starts with full mist and a valid scene depth texture");
   float previous=1;
   for(int i=0;i<=60;i++){l.SetCycleTime(i*.1f);check(fog.Strength<=previous+.00001f&&fog.Strength>=0,"Mist density decreases monotonically at "+(i*.1f).ToString("F1")+" s");previous=fog.Strength;}
   check(fog.Strength==0&&!fog.enabled&&(cam.depthTextureMode&DepthTextureMode.Depth)==0,"At exactly six seconds fog and its extra rendering/depth work switch off");
   l.SetCycleTime(5.9f);check(fog.Strength<.001f&&fog.Strength>0,"Final mist is already negligible before six seconds, avoiding a visible cut");
   foreach(float t in new[]{6f,30f,60f,120f,600f}){l.SetCycleTime(t);check(!fog.enabled&&fog.Strength==0,"Opening fog does not repeat in later day/night cycles: "+t);}
   l.SetCycleTime(59.99f);l.Advance(.02f);check(l.ElapsedTime>60&&l.CycleTime<.02f&&!fog.enabled,"Cycle wrap retains total elapsed time and cannot restart the opening mist");
   reset();l.SetCycleTime(2);float strength=fog.Strength;g.TogglePause();l.Advance(10);check(l.ElapsedTime==2&&fog.Strength==strength,"Pause freezes mist dispersal and wind time");g.TogglePause();
   g.jobsEnabled=true;g.ShowJobOffers();l.Advance(10);check(g.JobOffersOpen&&fog.ElapsedSeconds==2,"Job selection also freezes the six-second clock");g.AcceptJob(0);l.Advance(.5f);check(fog.ElapsedSeconds==2.5f&&fog.Strength<strength,"Selecting a job resumes mist from its existing state");
   typeof(PhoVietGame).GetMethod("BeginService",F).Invoke(g,null);strength=fog.Strength;l.Advance(.5f);check(g.ServiceStopped&&fog.Strength<strength,"Live passenger boarding advances mist just like the day/night clock");
   reset();l.SetCycleTime(2);
   var point=new Vector3(3,1,25);var before=fog.WorldToField.MultiplyPoint3x4(g.transform.TransformPoint(point));
   typeof(PhoVietGame).GetProperty("Distance").SetValue(g,4f);
   var after=fog.WorldToField.MultiplyPoint3x4(g.transform.TransformPoint(point-Vector3.forward*4));
   check(Vector3.Distance(before,after)<.001f,"Density field follows the scrolling city rather than sticking to the screen");
   var inverse=Quaternion.Euler(0,-90,0);var origin=new Vector3(5,0,7);point-=Vector3.forward*4;fog.Rebase(inverse,origin);
   after=fog.WorldToField.MultiplyPoint3x4(g.transform.TransformPoint(inverse*(point-origin)));
   check(Vector3.Distance(before,after)<.001f,"A ninety-degree junction rebase preserves the same world-space fog sample");
   reset();l.SetCycleTime(1);g.Crash();float elapsed=l.ElapsedTime;l.Advance(10);check(l.ElapsedTime==elapsed,"Crash outcome preserves the existing weather-clock pause rule");
   typeof(PhoVietGame).GetProperty("State").SetValue(g,PhoVietGame.RideState.Jailed);l.SetCycleTime(1);check(!fog.enabled,"Jail scene is not covered by the street mist");
   reset();check(fog.enabled&&fog.Strength==1&&l.ElapsedTime==0,"Restart restores dawn mist and resets its spatial frame");
   // Pixel evidence uses the same illumination for all renders, isolating actual fog output.
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}l.SetCycleTime(0);
   rt=new RenderTexture(320,200,24,RenderTextureFormat.ARGB32);texture=new Texture2D(320,200,TextureFormat.RGB24,false);cam.targetTexture=rt;
   Func<string,Color[]> render=name=>{cam.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,320,200),0,0);texture.Apply();File.WriteAllBytes("Captures/morning-fog-render-"+name+".png",texture.EncodeToPNG());return texture.GetPixels();};
   fog.Hide();var clear=render("clear");fog.SetTime(0);var dense=render("start");fog.SetTime(3);var middle=render("middle");fog.SetTime(6);var end=render("end");
   Func<Color[],Color[],int,int,int,int,float> difference=(a,b,x,y,w,h)=>{float sum=0;for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++){int i=yy*320+xx;sum+=(Mathf.Abs(a[i].r-b[i].r)+Mathf.Abs(a[i].g-b[i].g)+Mathf.Abs(a[i].b-b[i].b))/3;}return sum/(w*h);};
   float distant=difference(dense,clear,60,110,200,45),near=difference(dense,clear,130,15,160,40),mid=difference(middle,clear,60,110,200,45);
   check(distant>.025f,"Real rendered distant buildings/road are visibly veiled by the volume: "+distant);
   check(distant>near*1.15f,"Fog respects scene depth; nearby road is clearer than distant streets: near="+near+" far="+distant);
   check(mid>0&&mid<distant*.85f,"Three-second render is less obscured than the initial frame");
   check(difference(end,clear,0,0,320,200)<.002f,"At six seconds actual pixels match a render with fog disabled");
   return new{success=true,count=checks.Count,distantDifference=distant,nearDifference=near,middleDifference=mid,checks};
  }
  finally
  {
   cam.targetTexture=previousTarget;RenderTexture.active=previousActive;if(rt!=null){rt.Release();UnityEngine.Object.DestroyImmediate(rt);}if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);
   g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
  }
 }
}
