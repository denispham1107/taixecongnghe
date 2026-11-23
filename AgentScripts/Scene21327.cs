using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.327 — ghi vào scene: kho tên lửa tự hồi 12 → 7,2 s (21.327) → 5 s (21.328) (giá trị scene đè mặc định trong mã). Chỉ chạy khi không Play.
public static class Scene21327
{
    public static string Run()
    {
        if (EditorApplication.isPlaying) return "đang Play — dừng trước";
        var sb = new System.Text.StringBuilder();
        foreach (var h in Object.FindObjectsByType<OpenWorldHazards>(FindObjectsInactive.Include))
        {
            var so = new SerializedObject(h); var p = so.FindProperty("missileRegenSeconds");
            sb.Append($"{h.name}: missileRegenSeconds {p.floatValue}→5; "); p.floatValue = 5f;
            so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(h);
        }
        EditorSceneManager.MarkAllScenesDirty(); EditorSceneManager.SaveOpenScenes();
        return sb.ToString();
    }
}
