using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.Editor
{
 // Treat a Transform move as an edit of the complete player, not an animation
 // offset of its skin. This also accepts old scenes with an offset FBX root.
 [InitializeOnLoad]
 public static class PhoVietPlayerStartEditor
 {
  const string PendingKey="PhoViet.PlayerStart.Pending";
  const string SceneKey="PhoViet.PlayerStart.Scene";
  static bool applying;
  static PhoVietPlayerStartEditor()
  {
   Undo.postprocessModifications+=Moved;
   EditorApplication.playModeStateChanged+=PlayStateChanged;
  }
  static UndoPropertyModification[] Moved(UndoPropertyModification[] changes)
  {
   if(applying||EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isPlaying)return changes;
   foreach(var change in changes)
   {
    var value=change.currentValue;
    if(value==null||!value.propertyPath.StartsWith("m_LocalPosition",StringComparison.Ordinal))continue;
    var t=value.target as Transform;if(t==null)continue;
    var car=t.GetComponentInParent<PlayerCarVisual>();
    if(car==null||car.visual==null||(t!=car.transform&&t!=car.visual))continue;
    var game=car.GetComponentInParent<PhoVietGame>();if(game==null||game.rider!=car.transform)continue;
    if(EditorApplication.isPlaying&&game.State!=PhoVietGame.RideState.Ready)continue;
    applying=true;
    try
    {
     var world=car.visual.position;
     if(EditorApplication.isPlaying)
     {
      game.SetPlayerStartFromEditor(world);
      SessionState.SetString(PendingKey,JsonUtility.ToJson(game.PlayerStartPosition));
      SessionState.SetString(SceneKey,game.gameObject.scene.path);
     }
     else MoveWholeCar(car,world);
    }
    finally{applying=false;}
    break;
   }
   return changes;
  }
  static void MoveWholeCar(PlayerCarVisual car,Vector3 world)
  {
   Undo.RecordObjects(new UnityEngine.Object[]{car.transform,car.visual},"Đặt vị trí xuất phát xe");
   car.transform.position=world;car.visual.localPosition=Vector3.zero;
   PrefabUtility.RecordPrefabInstancePropertyModifications(car.transform);
   PrefabUtility.RecordPrefabInstancePropertyModifications(car.visual);
   EditorSceneManager.MarkSceneDirty(car.gameObject.scene);
  }
  static void PlayStateChanged(PlayModeStateChange state)
  {
   if(state!=PlayModeStateChange.EnteredEditMode)return;
   string json=SessionState.GetString(PendingKey,"");if(string.IsNullOrEmpty(json))return;
   string scene=SessionState.GetString(SceneKey,"");
   SessionState.EraseString(PendingKey);SessionState.EraseString(SceneKey);
   foreach(var game in UnityEngine.Object.FindObjectsByType<PhoVietGame>())
   {
    if(game.gameObject.scene.path!=scene||game.rider==null)continue;
    var car=game.rider.GetComponent<PlayerCarVisual>();if(car==null||car.visual==null)continue;
    MoveWholeCar(car,game.transform.TransformPoint(JsonUtility.FromJson<Vector3>(json)));
    Debug.Log("Đã giữ vị trí xuất phát vừa kéo trong Play. Nhấn Ctrl+S để lưu Scene.",car);
    break;
   }
  }
  [MenuItem("Phố Việt/Điểm xuất phát/Chọn xe để kéo vị trí")]
  public static void SelectPlayer()
  {
   var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
   if(game==null||game.rider==null)return;
   Selection.activeGameObject=game.rider.gameObject;Tools.current=Tool.Move;
   SceneView.lastActiveSceneView?.FrameSelected();
  }
 }
}
