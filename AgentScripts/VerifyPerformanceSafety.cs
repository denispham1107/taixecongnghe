using System;using System.Linq;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyPerformanceSafety
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 delegate bool Sweep(Vector3 a,Vector3 b,Quaternion q,out Vector2 n,out float t,out string kind);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var failures=new List<string>();Action<bool,string> check=(ok,s)=>{if(ok)checks.Add(s);else failures.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);Func<string,object[],object> call=(n,args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();
   var all=(StreetObstacleSet[])typeof(PhoVietGame).GetField("streetObstacles",F).GetValue(g);var sweep=(Sweep)Delegate.CreateDelegate(typeof(Sweep),g,typeof(PhoVietGame).GetMethod("SceneryContactAtRotation",F));var random=new System.Random(1193);
   for(int i=0;i<1200;i++)
   {
    var a=new Vector3((float)random.NextDouble()*45-22.5f,i%4==0?5:i%4==1?2:0,(float)random.NextDouble()*220-50);var b=a+new Vector3((float)random.NextDouble()*6-3,0,(float)random.NextDouble()*6-3);var q=Quaternion.Euler(0,i%8*45,0);
    bool x=Legacy(g,all,a,b,q,out var n0,out var t0,out var k0),y=sweep(a,b,q,out var n1,out var t1,out var k1);check(x==y&&(!x||Mathf.Abs(t0-t1)<.00001f&&Vector2.Distance(n0,n1)<.0001f&&k0==k1),"Exact scenery sweep equivalence "+i);
   }
   var single=all.First(s=>s.obstacles!=null&&s.obstacles.Length>1);set("streetObstacles",new[]{single});bool p=Legacy(g,new[]{single},Vector3.zero,Vector3.forward,Quaternion.identity,out _,out _,out _),n=sweep(Vector3.zero,Vector3.forward,Quaternion.identity,out _,out _,out _);check(p==n,"Replacement obstacle-set array rebuilds broadphase");set("streetObstacles",all);
   var objects=g.scenery.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Blender • ")).Select(t=>t.gameObject).ToArray();
   for(int i=0;i<120;i++)
   {
    if(i%17==0){var tile=g.roadRows[i%g.roadRows.Count].tiles[(i/17)%7];var block=tile.GetComponent<RiverBlock>();block.SetBridge(!block.IsBridge);}
    call("ScrollRoutes",new object[]{.2833333f});call("HideBuildingsOverRiver",null);var visible=objects.Select(o=>o.activeSelf).ToArray();int hidden=g.BuildingsHiddenOverRiver;
    set("riverLayoutValid",false);call("HideBuildingsOverRiver",null);check(hidden==g.BuildingsHiddenOverRiver&&visible.SequenceEqual(objects.Select(o=>o.activeSelf)),"Cached river layout matches full recomputation "+i);
   }
   call("HideBuildingsOverRiver",null);int work=g.RiverVisibilityRefreshes;for(int i=0;i<30;i++)call("ScrollRoutes",new object[]{.2833333f});check(g.RiverVisibilityRefreshes-work<3,"Common scrolling skips repeated river intersection work");
   g.ResetRide();g.StartRide();g.daylight.SetCycleTime(0);for(int i=0;i<10;i++)g.daylight.AdvanceSimulation(.005f);g.daylight.UpdateLamps();var angle=g.daylight.sun.transform.rotation;var color=g.daylight.sun.color;float elapsed=g.daylight.ElapsedTime;
   g.daylight.SetCycleTime(0);for(int i=0;i<10;i++)g.daylight.Advance(.005f);check(Mathf.Abs(elapsed-g.daylight.ElapsedTime)<1e-6f&&Quaternion.Angle(angle,g.daylight.sun.transform.rotation)<.001f&&Vector4.Distance(color,g.daylight.sun.color)<.00001f,"Coalesced lighting matches ten immediate simulation steps exactly");
   check(QualitySettings.antiAliasing==4&&Mathf.Abs(QualitySettings.shadowDistance-100)<.01f&&QualitySettings.lodBias==2,"AA4, original shadow distance and global detail quality retained");
   var street=g.scenery.GetComponentsInChildren<LODGroup>(true).Where(l=>l.GetLODs()[0].renderers.Any(r=>r!=null&&(r.name.StartsWith("PV_StreetShop")||r.name.StartsWith("PV_StreetTree")))).ToArray();check(street.Length==700&&street.All(l=>Mathf.Abs(l.GetLODs()[0].screenRelativeTransitionHeight-.45f)<.001f&&Mathf.Abs(l.GetLODs()[1].screenRelativeTransitionHeight-.175f)<.001f&&l.GetLODs().Length==3),"Only static street distance LOD budgets changed; all three source meshes retained");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   var report=new{success=failures.Count==0,count=checks.Count,failures,checks};File.WriteAllText("Captures/performance-safety-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=time;}
 }
  static bool Legacy(PhoVietGame g,StreetObstacleSet[] streetObstacles,Vector3 from,Vector3 to,Quaternion rotation,out Vector2 normal,out float at,out string kind)
  {
   normal=Vector2.zero;at=2;kind="";
   foreach(var set in streetObstacles)
   {
    if(set==null||!set.gameObject.activeInHierarchy)continue;
    var group=set.LocalBounds;Vector3 origin=g.transform.InverseTransformPoint(set.transform.TransformPoint(group.center));float radius=group.extents.magnitude+1.1f;
    if(Mathf.Abs(origin.x-to.x)>radius+Mathf.Abs(to.x-from.x)||Mathf.Abs(origin.z-to.z)>radius+Mathf.Abs(to.z-from.z))continue;
    Vector3 a=set.transform.InverseTransformPoint(g.transform.TransformPoint(from)),b=set.transform.InverseTransformPoint(g.transform.TransformPoint(to));
    Vector3 forward=set.transform.InverseTransformDirection(g.transform.TransformDirection(rotation*Vector3.forward));
    Vector3 right=set.transform.InverseTransformDirection(g.transform.TransformDirection(rotation*Vector3.right));
    var extent=new Vector3(Mathf.Abs(forward.x)*g.PlayerHalfLength+Mathf.Abs(right.x)*g.PlayerHalfWidth,0,Mathf.Abs(forward.z)*g.PlayerHalfLength+Mathf.Abs(right.z)*g.PlayerHalfWidth);
    foreach(var obstacle in set.obstacles)
    {
     var bounds=obstacle.bounds;Vector3 min=bounds.min-extent,max=bounds.max+extent;min.y-=g.PlayerHeight;
     Vector3 delta=b-a;float enter=0,leave=1;Vector3 face=Vector3.zero;bool valid=true;
     for(int axis=0;axis<3;axis++)
     {
      if(Mathf.Abs(delta[axis])<.000001f){if(a[axis]<min[axis]||a[axis]>max[axis]){valid=false;break;}continue;}
      float t1=(min[axis]-a[axis])/delta[axis],t2=(max[axis]-a[axis])/delta[axis];float near=Mathf.Min(t1,t2),far=Mathf.Max(t1,t2);
      if(near>enter){enter=near;face=Vector3.zero;face[axis]=Mathf.Sign(delta[axis]);}
      leave=Mathf.Min(leave,far);if(enter>leave){valid=false;break;}
     }
     if(!valid||leave<0||enter>1||enter>=at)continue;
     // Roof/table top clearance must not turn a safe jump into a side impact.
     if(Mathf.Abs(face.y)>.5f||Mathf.Min(from.y,to.y)>=set.transform.TransformPoint(bounds.max).y-g.transform.position.y+.02f)continue;
     if(face.sqrMagnitude<.5f)
     {
      float dx=Mathf.Min(Mathf.Abs(a.x-min.x),Mathf.Abs(max.x-a.x)),dz=Mathf.Min(Mathf.Abs(a.z-min.z),Mathf.Abs(max.z-a.z));
      face=dx<dz?new Vector3(a.x<bounds.center.x?1:-1,0,0):new Vector3(0,0,a.z<bounds.center.z?1:-1);
      if(Vector3.Dot(delta,face)<=.00001f)continue; // Already touching: allow steering away after standing up.
     }
     var n=g.transform.InverseTransformDirection(set.transform.TransformDirection(face));normal=new Vector2(n.x,n.z).normalized;at=Mathf.Clamp01(enter);kind=obstacle.kind;
    }
   }
   return at<=1;
  }

}
