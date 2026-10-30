using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PhoViet;
public static class VerifyRelease151Scene
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  check(!Application.isPlaying&&g!=null&&!g.gameObject.scene.isDirty,"Saved scene in Edit Mode");
  check(Application.version=="1.51.0","Version1.51.0");
  check(PlayerSettings.WebGL.initialMemorySize==128&&PlayerSettings.WebGL.maximumMemorySize==512,"128MiB initial /512MiB maximum heap");
  check(PlayerSettings.WebGL.memoryGrowthMode==WebGLMemoryGrowthMode.Geometric&&Mathf.Abs(PlayerSettings.WebGL.geometricMemoryGrowthStep-.1f)<.00001f&&PlayerSettings.WebGL.memoryGeometricGrowthCap==32,"Conservative10percent/32MiB heap growth");
  check(QualitySettings.antiAliasing==4&&Mathf.Abs(QualitySettings.shadowDistance-100)<.001f,"AA4 and100m shadows retained");
  check(EditorUserBuildSettings.overrideTextureCompression==UnityEditor.Build.OverrideTextureCompression.NoOverride,"Temporary recovery import override cleared");
  check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
  check(PhoVietGame.TurnTankDelay==0&&PhoVietGame.TankDistanceInterval==400,"Immediate turn pair and 400 m milestone configuration");
  check(g.DistanceTanksScheduled==0&&(float)typeof(PhoVietGame).GetField("nextDistanceTank",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(g)==400,"Fresh distance-wave state after reset");
  var car=g.rider.GetComponent<PlayerCarVisual>();
  check(car!=null&&Mathf.Abs(car.ModelScale-.85f)<.00001f&&car.jumpMounts.Length==4&&car.exhaustMounts.Length==2,"Player scale/rig unchanged");
  var lamp=g.daylight.vehicles.Single(v=>v.root==g.rider);var front=lamp.halos.Where(h=>!h.rear).ToArray();check(front.Length==2&&!front.Any(h=>h.source==lamp.head)&&Vector3.Distance(lamp.head.position,(front[0].source.position+front[1].source.position)*.5f)<.001f,"Player projector is centered independently of the front halos");
  check(File.Exists("Assets/WebGLTemplates/PhoVietPWA/poster-night.jpg")&&File.ReadAllText("Assets/WebGLTemplates/PhoVietPWA/index.html").Contains("night-preview"),"Real night screenshot is included below the morning image");
  var authored=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Captures/release144-authored-state.json"));
  var start=JsonUtility.FromJson<Vector3>(authored["start"].ToString());
  check(Vector3.Distance(g.rider.localPosition,start)<.001f&&car.visual.localPosition.sqrMagnitude<.000001f,"Current authored starting position preserved");
  check(g.roadRows.Count==5&&g.roadRows.All(r=>r.tiles.Count==7),"All five pooled road rows retained");
  check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==10983,"All scenery renderers retained");
  check(g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).Sum(t=>t.obstacles.Length)==7670,"All physical scenery obstacles retained");
  check(File.ReadAllText("Assets/PhoViet/Scripts/PhoVietGame.cs").Contains("/17f*1.53065f"),"21.100 player lane pace is 1.53065");
  check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(17)-4.1166525f)<.001f,"Fixed starting jump is 1.49 plus three percent");
  check(Mathf.Abs(PhoVietGame.JumpHeightAtSpeed(30)-4.1166525f)<.001f,"Fixed high-speed jump equals the starting height");
  const System.Reflection.BindingFlags flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
  check(Mathf.Abs((float)typeof(PhoVietGame).GetField("JumpRiseGravity",flags).GetRawConstantValue()-24.2f)<.0001f&&(float)typeof(PhoVietGame).GetField("JumpGravity",flags).GetRawConstantValue()==22,"Faster launch retains original falling gravity");
  var river=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Assets/PhoViet/Art/River/PV_RiverApartments.json"));
  check(river["parts"].Count()==24&&river["parts"].Sum(p=>p["rental_signs"].Count())==38&&river["parts"].SelectMany(p=>p["rental_signs"]).All(s=>Mathf.Abs((float)s["font_size"]-.8352f)<.0001f),"All 38 imported rental plaques preserve larger lettering");
  var deps=AssetDatabase.GetDependencies(g.gameObject.scene.path,true);
  check(g.tankCraterMaterial!=null&&g.tankCraterMaterial.shader.name=="PhoViet/Tank Crater Surface","Crater material/shader is a scene dependency");
  foreach(string shaderName in new[]{"PhoViet/Road With Tank Craters","PhoViet/Tank Crater Surface","PhoViet/City Ground"}){var shader=Shader.Find(shaderName);check(shader!=null&&shader.isSupported&&!ShaderUtil.ShaderHasError(shader),"Crater shader ready: "+shaderName);}
  check(Mathf.Abs(PhoVietGame.TankCraterRadius-2.024f)<.0001f&&PhoVietGame.TankCraterCapacity==64,"Crater radius/bounded pool retained");
  check(g.ActiveCraterCount==0&&!g.CraterSteeringActive,"Fresh crater terrain and steering after reset");
  check(deps.Any(p=>p.EndsWith("PV_TankCrater.mat"))&&deps.Any(p=>p.EndsWith("TankCraterRoad.shader")),"Build includes road cutout and crater assets");
  check(deps.Any(p=>p.Contains("Art/River/FishingBoat/")&&p.EndsWith(".fbx")),"New fishing boat FBX is a build dependency");
  check(g.scenery.GetComponentsInChildren<RiverBoat>(true).Length==140,"All 140 fishing boats retained");
  check(!deps.Any(p=>p.Contains("ai.meshy/")),"No raw Meshy preview models in runtime dependencies");
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
  File.WriteAllText("Captures/release151-dependencies.json",Newtonsoft.Json.JsonConvert.SerializeObject(deps,Newtonsoft.Json.Formatting.Indented));
  check(g.scenery.GetComponentsInChildren<LODGroup>(true).All(l=>l.GetLODs().All(d=>d.renderers.All(r=>r!=null))),"All LOD renderer links retained");
  var report=new{success=true,count=checks.Count,checks,dependencies=deps.Length};
  File.WriteAllText(desktop?"Captures/release151-desktop-scene-checks.json":"Captures/release151-scene-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
