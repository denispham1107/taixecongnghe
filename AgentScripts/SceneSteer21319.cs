using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.319 — ghi thông số lái nhạy hơn vào scene (giá trị trong scene đè mặc định trong mã). Chỉ chạy khi Editor không Play.
public static class SceneSteer21319
{
    public static string Run()
    {
        if (EditorApplication.isPlaying) return "đang Play — dừng trước";
        var sb = new System.Text.StringBuilder();
        foreach (var d in Object.FindObjectsByType<OpenWorldDriver>(FindObjectsInactive.Include))
        {
            var so = new SerializedObject(d);
            void F(string n, float v) { var p = so.FindProperty(n); sb.Append($"{n} {p.floatValue}→{v}; "); p.floatValue = v; }
            F("steerRise", 0.08f); F("steerReturn", 0.06f); F("gripG", 2.6f); F("yawLag", 0.045f); F("scrubDecel", 2f); F("gripLow", 0.02f); F("gripHigh", 0.05f);
            so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(d);
            sb.AppendLine(" @" + d.name);
        }
        EditorSceneManager.MarkAllScenesDirty(); EditorSceneManager.SaveOpenScenes();
        return sb.ToString();
    }
}
