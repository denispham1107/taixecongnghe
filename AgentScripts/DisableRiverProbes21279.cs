using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
// 21.279: tắt thành phần ReflectionProbe của PhanChieuSong (giữ cubemap cho OpenWorldRiverReflections), so hộp chiếu cũ/mới, lưu scene.
public static class DisableRiverProbes21279
{
 public static object Main()
 {
  var root=GameObject.Find("PhanChieuSong"); if(!root) return "no PhanChieuSong";
  var sb=new System.Text.StringBuilder(); float worst=0;
  foreach(var p in root.GetComponentsInChildren<ReflectionProbe>(true)){
   var b=p.bounds; Vector3 c=p.transform.position+p.center,h=p.size*0.5f;
   worst=Mathf.Max(worst,(b.min-(c-h)).magnitude,(b.max-(c+h)).magnitude);
   Undo.RecordObject(p,"tat probe"); p.enabled=false; EditorUtility.SetDirty(p);
  }
  var scene=root.scene; EditorSceneManager.MarkSceneDirty(scene); bool saved=EditorSceneManager.SaveScene(scene);
  int on=0; foreach(var p in Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include)) if(p.enabled) on++;
  return "worst bounds diff="+worst+" saved="+saved+" probes still enabled="+on;
 }
}
