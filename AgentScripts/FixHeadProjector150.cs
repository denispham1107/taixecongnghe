using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor.SceneManagement;using PhoViet;
public static class FixHeadProjector150
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(Application.isPlaying||g.gameObject.scene.isDirty)throw new Exception("Clean Edit Mode required");
  if(g.jobsEnabled||PhoVietGame.RideJobsReleased||Vector3.Distance(g.rider.localPosition,new Vector3(1.55f,0,1.92f))>.001f)throw new Exception("Retain authored start/jobs");
  bool created=PhoViet.Editor.PhoVietSupercarUpgrade.CenterHeadProjector(g);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
  var head=g.daylight.vehicles.Single(v=>v.root==g.rider).head;var p=g.rider.InverseTransformPoint(head.position);
  var r=new{success=true,created,position=new[]{p.x,p.y,p.z},scene=g.gameObject.scene.path};File.WriteAllText("Captures/release150-head-fix.json",Newtonsoft.Json.JsonConvert.SerializeObject(r,Newtonsoft.Json.Formatting.Indented));return r;
 }
}
