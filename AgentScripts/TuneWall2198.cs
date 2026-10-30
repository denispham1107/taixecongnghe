using System;using System.IO;using UnityEngine;using UnityEditor;using PhoViet;
public static class TuneWall2198
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(Application.isPlaying||g.gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  var paths=new[]{"Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat","Assets/PhoViet/Art/River/PV_RiverRoomLight.mat"};
  foreach(var path in paths){var m=AssetDatabase.LoadAssetAtPath<Material>(path);m.SetFloat("_StuccoScale",.65f);m.SetFloat("_StuccoStrength",.42f);m.SetFloat("_StuccoContrast",.28f);m.SetFloat("_WallSmoothness",.015f);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);}
  var result=new{success=true,paths,scale=.65,strength=.42,contrast=.28,smoothness=.015,geometryReimported=false};File.WriteAllText("Captures/wall2198-tuning.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
}
