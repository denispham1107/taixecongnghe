using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
// 21.302c — nướng lại 13 probe phản chiếu sông (PhanChieuSong) theo cảnh bán đảo hiện tại (đã xóa 70 cao ốc vành ngoài, có biển,
// kè, đường dạo, bầu trời mới). Gắn tạm cờ ReflectionProbeStatic cho gốc BanDao (trừ mặt biển) rồi trả lại cờ cũ.
public static class RebakeProbes21302
{
    public static string Main()
    {
        var map = GameObject.Find("BanDoMo"); var root = GameObject.Find("PhanChieuSong"); var bd = GameObject.Find("BanDao");
        if (!map || !root) return "thiếu BanDoMo/PhanChieuSong";
        var saved = new Dictionary<GameObject, StaticEditorFlags>();
        if (bd && !bd.transform.IsChildOf(map.transform))
            foreach (var t in bd.GetComponentsInChildren<Transform>(false))
            {
                if (t.name == "MatBien") continue;
                var f = GameObjectUtility.GetStaticEditorFlags(t.gameObject); saved[t.gameObject] = f;
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, f | StaticEditorFlags.ReflectionProbeStatic);
            }
        string r;
        try
        {
            var list = new List<ReflectionProbe>(root.GetComponentsInChildren<ReflectionProbe>(true));
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            r = PhoViet.OpenWorldEditor.PhoVietOpenWorldRiverBuilder.BakeProbes(map, list);
        }
        finally { foreach (var kv in saved) if (kv.Key) GameObjectUtility.SetStaticEditorFlags(kv.Key, kv.Value); }
        var sc = EditorSceneManager.GetActiveScene(); EditorSceneManager.MarkSceneDirty(sc); EditorSceneManager.SaveScene(sc);
        return r + (bd ? (bd.transform.IsChildOf(map.transform) ? " (BanDao nằm trong BanDoMo)" : $" (+{saved.Count} vật BanDao)") : " (không thấy BanDao)");
    }
}
