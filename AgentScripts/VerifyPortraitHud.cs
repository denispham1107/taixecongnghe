using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyPortraitHud
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying||Screen.width>=Screen.height)throw new Exception("Portrait Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;bool jobs=g.jobsEnabled;
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<string,object[]> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Func<Renderer,float> projectedHeight=r=>{float low=1e6f,high=-1e6f;var b=r.bounds;for(int mask=0;mask<8;mask++){var p=new Vector3((mask&1)==0?b.min.x:b.max.x,(mask&2)==0?b.min.y:b.max.y,(mask&4)==0?b.min.z:b.max.z);float y=g.rideCamera.WorldToViewportPoint(p).y;low=Mathf.Min(low,y);high=Mathf.Max(high,y);}return high-low;};
  try
  {
   Time.timeScale=0;g.jobsEnabled=false;g.RestartRide();g.rideCamera.aspect=(float)Screen.width/Screen.height;
   g.ApplyBrowserInsets("{\"top\":0,\"bottom\":0,\"left\":0,\"right\":0}");
   foreach(bool chased in new[]{false,true})foreach(int lane in new[]{0,1,2,3})
   {
    typeof(PhoVietGame).GetProperty("IsChased").SetValue(g,chased);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
    call("PositionCamera",new object[]{true});float expanded=g.rideCamera.fieldOfView;
    var renderer=g.rider.GetComponentInChildren<SkinnedMeshRenderer>();float now=projectedHeight(renderer);
    g.rideCamera.fieldOfView=2*Mathf.Atan(Mathf.Tan(65*Mathf.Deg2Rad*.5f)/.75f)*Mathf.Rad2Deg;float before=projectedHeight(renderer);g.rideCamera.fieldOfView=expanded;
    check(Mathf.Abs(now/before-.70f)<.001f,"Rider projects at 70 percent of 1.27.1 portrait size: chase="+chased+", lane="+lane);
    check(g.rideCamera.rect==new Rect(0,0,1,1),"Wider view keeps full-screen viewport: "+chased+","+lane);
   }
   g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   var bike=g.SpawnTraffic(3,0,1);var bottom=new Vector2(Screen.width*.5f,Screen.height*.05f);
   check(!g.RegisterTap(bottom,10),"Single tap in the former controls area does not kick");
   check(g.RegisterTap(bottom,10.2f)&&bike.knocked,"Double tap in cleared bottom gameplay space still kicks");
   set("lastTapTime",-100f);g.RegisterTap(new Vector2(Screen.width*.5f,Screen.height-40),12);
   check((float)typeof(PhoVietGame).GetField("lastTapTime",F).GetValue(g)==-100,"Merged HUD/pause area cannot trigger a kick");
   g.ApplyBrowserInsets("{\"top\":0.06,\"bottom\":0.04,\"left\":0.03,\"right\":0.03}");
   set("lastTapTime",-100f);g.RegisterTap(new Vector2(Screen.width*.5f,1),14);
   check((float)typeof(PhoVietGame).GetField("lastTapTime",F).GetValue(g)==-100,"Home-indicator safe area remains excluded");
   set("noticeTimer",0f);var centre=new Vector2(Screen.width*.5f,Screen.height*.5f);g.RegisterTap(centre,16);
   check((float)typeof(PhoVietGame).GetField("lastTapTime",F).GetValue(g)==16,"World below compact HUD remains interactive with safe insets");
   return new{success=true,count=checks.Count,checks};
  }
  finally{g.ApplyBrowserInsets("{\"top\":0,\"bottom\":0,\"left\":0,\"right\":0}");g.rideCamera.ResetAspect();g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=time;}
 }
}
