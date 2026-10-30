using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using PhoViet;
public static class ProbeCraterSources
{
 public static object Main(){var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  var rs=g.blocks[0].GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("Geometry •")||r.name.StartsWith("PV_RiverBridge_LOD")||r.name.Contains("mũi tên")).ToArray();
  var report=new{play=Application.isPlaying,dirty=g.gameObject.scene.isDirty,renderers=rs.Select(r=>new{r.name,materials=r.sharedMaterials.Select(m=>new{m.name,shader=m.shader.name,path=AssetDatabase.GetAssetPath(m)})}),wheels=g.rider.GetComponent<PlayerCarVisual>().wheels.Select(w=>{var p=g.rider.InverseTransformPoint(w.position);return new{w.name,centre=new[]{p.x,p.y,p.z}};})};
  File.WriteAllText("Captures/crater-source-probe.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{success=true,renderers=rs.Length};
 }
}
