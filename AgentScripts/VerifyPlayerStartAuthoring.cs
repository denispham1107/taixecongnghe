using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet;

public static class VerifyPlayerStartAuthoring
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var car=g.rider.GetComponent<PlayerCarVisual>();
  var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  var originalRoot=g.rider.position;var originalVisual=car.visual.localPosition;
  try
  {
   foreach(var delta in new[]{Vector3.forward*2,Vector3.back*3,Vector3.right,Vector3.left*2,Vector3.up,Vector3.down})
   {
    var before=car.visual.position;Undo.IncrementCurrentGroup();
    Undo.RecordObject(car.visual,"Verify authored player start");car.visual.position+=delta;Undo.FlushUndoRecordObjects();
    check(Vector3.Distance(car.visual.position,before+delta)<.002f,"Model drag preserves requested world position: "+delta);
    check(Vector3.Distance(g.rider.position,car.visual.position)<.002f&&car.visual.localPosition.sqrMagnitude<.000001f,"Dragging the model also moves its collision body: "+delta);
   }
   check(g.gameObject.scene.isDirty,"Authored Transform edits mark the scene for Save");
   var report=new{passed=checks.Count,checks};File.WriteAllText("Captures/player-start-authoring-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally
  {
   g.rider.position=originalRoot;car.visual.localPosition=originalVisual;
   PrefabUtility.RecordPrefabInstancePropertyModifications(g.rider);PrefabUtility.RecordPrefabInstancePropertyModifications(car.visual);
   EditorSceneManager.MarkSceneDirty(g.gameObject.scene);
  }
 }
}
