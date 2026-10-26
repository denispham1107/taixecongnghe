using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
public static class VerifyRelease140Scene
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(Application.isPlaying||g==null||g.gameObject.scene.isDirty)throw new Exception("Saved scene in Edit Mode required");
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};var c=g.rider.GetComponent<PlayerCarVisual>();
  check(Application.version=="1.40.0","Release version is 1.40.0");check(c!=null&&Mathf.Abs(c.ModelScale-.85f)<.00001f,"Blue Supercar at the confirmed 85 percent scale");
  check(Mathf.Abs(c.size.z-3.74f)<.00001f&&c.jumpMounts.Length==4&&c.exhaustMounts.Length==2,"Scaled dimensions/four lift sockets/two exhausts saved");
  check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");check(g.riversEnabled&&g.dinosaursEnabled&&g.dinosaurPrefab!=null,"Rivers and installed dinosaur enabled");
  check(g.vehicles.Count==28&&g.policeBike.GetComponent<BikeVisualAnimator>()!=null,"Traffic pool and original police retained");
  check(g.rider.GetComponent<BikeVisualAnimator>()==null,"No former player motorcycle animator");
  var dependencies=AssetDatabase.GetDependencies(g.gameObject.scene.path,true);check(dependencies.All(File.Exists),"Every scene dependency exists");
  check(g.gameObject.scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts");
  File.WriteAllText(".tools/release140-dependencies.json",Newtonsoft.Json.JsonConvert.SerializeObject(dependencies,Newtonsoft.Json.Formatting.Indented));
  var report=new{success=true,count=checks.Count,checks,dependencies=dependencies.Length,scale=c.ModelScale,size=c.size.ToString("F6"),version=Application.version};File.WriteAllText("Captures/release140-scene-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
