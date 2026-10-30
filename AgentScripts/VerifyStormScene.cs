using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
public static class VerifyStormScene
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  void Check(bool b,string label){if(!b)throw new Exception(label);checks.Add(label);}
  Check(g!=null&&!g.gameObject.scene.isDirty,"Saved clean scene in Edit Mode");Check(g.thunderstormEnabled,"Storms enabled in authored scene");
  var mats=new[]{g.stormRainMaterial,g.stormBoltMaterial,g.stormWarningMaterial};for(int i=0;i<3;i++){var m=mats[i];Check(m!=null&&m.shader.name=="PhoViet/Thunderstorm FX"&&m.GetFloat("_Mode")==i,"Correct saved rain/bolt/warning material "+i);}
  foreach(string name in new[]{"PhoViet/Thunderstorm FX","PhoViet/Day Night Sky","PhoViet/Road With Tank Craters"}){var s=Shader.Find(name);Check(s!=null&&s.isSupported&&!ShaderUtil.ShaderHasError(s),"Supported error-free shader: "+name);}
  Check(g.stormFireTemplate!=null&&!g.stormFireTemplate.gameObject.activeSelf,"Existing baked fire template remains dormant");
  Check(g.stormFireTemplate.GetComponentsInChildren<ParticleSystem>(true).Length==2,"Baked fire and smoke reused without replacement textures");
  Check(g.GetComponent<ThunderstormVisual>()==null,"No generated storm objects saved into scene");
  Check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==10983,"All environment renderers preserved");
  Check(g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).Sum(t=>t.obstacles.Length)==7670,"All physical scenery obstacles preserved");
  Check(g.scenery.GetComponentsInChildren<RiverBoat>(true).Length==140,"New fishing boat set preserved");
  Check(g.daylight.streets.Length==350&&g.daylight.vehicles.Length==30&&g.daylight.lightPool.Length==6,"Current lighting bindings and real-light budget preserved");
  Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs stay locked");Check(Vector3.Distance(g.rider.localPosition,new Vector3(1.55f,0,1.92f))<.001f,"Authored player start preserved");
  Check(Mathf.Abs(g.rider.GetComponent<PlayerCarVisual>().ModelScale-.85f)<.0001f&&Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(17)-4.1166525f)<.001f,"Player scale and jump contract preserved");
  Check(QualitySettings.antiAliasing==4&&QualitySettings.shadowDistance==100,"AA and shadows unchanged");
  Check(g.gameObject.scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts");
  var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/storm-scene-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
 }
}
