using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using PhoViet;
public static class ConfigureTankCraters
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  if(g.gameObject.scene.isDirty||g.jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Clean scene/jobs locked required");
  var shader=Shader.Find("PhoViet/Road With Tank Craters");var surface=Shader.Find("PhoViet/Tank Crater Surface");if(shader==null||surface==null)throw new Exception("Import/compile crater shaders first");
  var prefixes=new[]{"PV_555D5DFFFalse","PV_FFF1CFFFFalse","PV_FFD35AFFFalse"};
  var mats=g.scenery.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null&&prefixes.Any(s=>m.name.StartsWith(s,StringComparison.Ordinal))).Distinct().ToArray();
  if(mats.Length!=3)throw new Exception("Expected only three shared asphalt/road paint materials: "+mats.Length);
  Directory.CreateDirectory(".tools/crater-before");File.Copy(g.gameObject.scene.path,".tools/crater-before/PhoViet.unity",true);
  foreach(var m in mats)
  {
   string path=AssetDatabase.GetAssetPath(m);File.Copy(path,".tools/crater-before/"+Path.GetFileName(path),true);
   if(m.shader.name!="Standard"&&m.shader!=shader)throw new Exception("Unexpected road material shader");
   var color=m.color;float gloss=m.GetFloat("_Glossiness"),metal=m.GetFloat("_Metallic");var texture=m.mainTexture;
   m.shader=shader;
   if(m.color!=color||m.mainTexture!=texture||m.GetFloat("_Glossiness")!=gloss||m.GetFloat("_Metallic")!=metal)throw new Exception("Road look changed");EditorUtility.SetDirty(m);
  }
  const string materialPath="Assets/PhoViet/Art/TankFx/PV_TankCrater.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
  if(mat==null){mat=new Material(surface){name="PV_TankCrater"};AssetDatabase.CreateAsset(mat,materialPath);}
  mat.enableInstancing=true;EditorUtility.SetDirty(mat);g.tankCraterMaterial=mat;EditorUtility.SetDirty(g);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
  var report=new{success=true,materials=mats.Select(m=>AssetDatabase.GetAssetPath(m)),craterMaterial=materialPath,scene=g.gameObject.scene.path};File.WriteAllText("Captures/crater-configure.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
