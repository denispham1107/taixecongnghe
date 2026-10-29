using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PhoViet;
public static class VerifyRelease149Scene
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  check(!Application.isPlaying&&g!=null&&!g.gameObject.scene.isDirty,"Saved scene in Edit Mode");
  check(Application.version=="1.49.0","Version1.49.0");
  check(PlayerSettings.WebGL.initialMemorySize==128&&PlayerSettings.WebGL.maximumMemorySize==512,"128MiB initial /512MiB maximum heap");
  check(PlayerSettings.WebGL.memoryGrowthMode==WebGLMemoryGrowthMode.Geometric&&Mathf.Abs(PlayerSettings.WebGL.geometricMemoryGrowthStep-.1f)<.00001f&&PlayerSettings.WebGL.memoryGeometricGrowthCap==32,"Conservative10percent/32MiB heap growth");
  check(QualitySettings.antiAliasing==4&&Mathf.Abs(QualitySettings.shadowDistance-100)<.001f,"AA4 and100m shadows retained");
  check(EditorUserBuildSettings.overrideTextureCompression==UnityEditor.Build.OverrideTextureCompression.NoOverride,"Temporary recovery import override cleared");
  check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
  var car=g.rider.GetComponent<PlayerCarVisual>();
  check(car!=null&&Mathf.Abs(car.ModelScale-.85f)<.00001f&&car.jumpMounts.Length==4&&car.exhaustMounts.Length==2,"Player scale/rig unchanged");
  var authored=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Captures/release144-authored-state.json"));
  var start=JsonUtility.FromJson<Vector3>(authored["start"].ToString());
  check(Vector3.Distance(g.rider.localPosition,start)<.001f&&car.visual.localPosition.sqrMagnitude<.000001f,"Current authored starting position preserved");
  check(g.roadRows.Count==5&&g.roadRows.All(r=>r.tiles.Count==7),"All five pooled road rows retained");
  check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==10983,"All scenery renderers retained");
  check(g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).Sum(t=>t.obstacles.Length)==7670,"All physical scenery obstacles retained");
  check(File.ReadAllText("Assets/PhoViet/Scripts/PhoVietGame.cs").Contains("/17f*1.3915f"),"21.94 player lane pace is 1.3915");
  check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(17)-3.99675f)<.001f,"21.94 starting jump height is about 4 metres");
  check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(30)-5.329f)<.001f,"Speed-based jump height retains another 10 percent");
  const System.Reflection.BindingFlags flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
  check(Mathf.Abs((float)typeof(PhoVietGame).GetField("JumpRiseGravity",flags).GetRawConstantValue()-24.2f)<.0001f&&(float)typeof(PhoVietGame).GetField("JumpGravity",flags).GetRawConstantValue()==22,"Faster launch retains original falling gravity");
  var river=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Assets/PhoViet/Art/River/PV_RiverApartments.json"));
  check(river["parts"].Count()==24&&river["parts"].All(p=>p["rental_signs"].Count()==1&&Mathf.Abs((float)p["rental_signs"][0]["font_size"]-.8352f)<.0001f),"All 24 imported rental plaques use the larger lettering");
  var deps=AssetDatabase.GetDependencies(g.gameObject.scene.path,true);
  bool desktop=((TextureImporter)AssetImporter.GetAtPath("Assets/PhoViet/Art/UrbanRider/PV_UrbanRider_Normal.png")).GetPlatformTextureSettings("WebGL").format==TextureImporterFormat.DXT5;
  foreach(var path in deps)
  {
   var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
   var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(tex==null||tex.width*tex.height<65536)continue;
   var w=importer.GetPlatformTextureSettings("WebGL");
   check(w.overridden&&(desktop?(w.format==TextureImporterFormat.DXT1||w.format==TextureImporterFormat.DXT5):(w.format==TextureImporterFormat.ETC2_RGB4||w.format==TextureImporterFormat.ETC2_RGBA8||w.format==TextureImporterFormat.EAC_RG)),"Matching GPU texture format: "+path);
   check(w.maxTextureSize>=importer.maxTextureSize,"Texture resolution preserved: "+path);
  }
  check(g.gameObject.scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts");
  File.WriteAllText("Captures/release149-dependencies.json",Newtonsoft.Json.JsonConvert.SerializeObject(deps,Newtonsoft.Json.Formatting.Indented));
  check(g.scenery.GetComponentsInChildren<LODGroup>(true).All(l=>l.GetLODs().All(d=>d.renderers.All(r=>r!=null))),"All LOD renderer links retained");
  var report=new{success=true,count=checks.Count,checks,dependencies=deps.Length};
  File.WriteAllText(desktop?"Captures/release149-desktop-scene-checks.json":"Captures/release149-scene-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
