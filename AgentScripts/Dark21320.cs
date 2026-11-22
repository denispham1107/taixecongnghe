using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.320 — tìm «lớp đen mờ phía trước xe» khi trời mưa: liệt kê vật thể thời tiết, bật/tắt từng phần để so ảnh.
public static class Dark21320
{
    public static string Info()
    {
        var cam = Camera.main; var sb = new System.Text.StringBuilder();
        sb.AppendLine($"cam {cam.transform.position} far {cam.farClipPlane} near {cam.nearClipPlane} fog {RenderSettings.fog} {RenderSettings.fogMode} {RenderSettings.fogStartDistance}-{RenderSettings.fogEndDistance} col {RenderSettings.fogColor}");
        var w = Object.FindAnyObjectByType<OpenWorldWeather>();
        sb.AppendLine($"wet {w.Wet} raining {w.Raining}");
        foreach (var r in w.GetComponentsInChildren<Renderer>(true))
        {
            if (!r.gameObject.activeInHierarchy || !r.enabled) continue;
            var m = r.sharedMaterial;
            sb.AppendLine($"{Path(r.transform)} {r.GetType().Name} pos {r.transform.position} scale {r.transform.lossyScale} b {r.bounds.size} mat {(m ? m.name + "/" + m.shader.name + " q" + m.renderQueue : "-")}");
        }
        // vật thể con của camera
        foreach (var r in cam.GetComponentsInChildren<Renderer>(true)) sb.AppendLine("camchild " + Path(r.transform) + " " + r.enabled);
        return sb.ToString();
    }
    static string Path(Transform t) { var s = t.name; while (t.parent) { t = t.parent; s = t.name + "/" + s; } return s; }
    public static string Toggle(string name, bool on)
    {
        var w = Object.FindAnyObjectByType<OpenWorldWeather>(); int n = 0;
        foreach (var t in w.GetComponentsInChildren<Transform>(true))
            if (t.name == name) { var r = t.GetComponent<Renderer>(); if (r) r.enabled = on; else t.gameObject.SetActive(on); n++; }
        return name + " x" + n;
    }
    public static string Fog(bool on) { RenderSettings.fog = on; return "fog " + on; }
}
public static class Dark21320b
{
    public static string Restore()
    {
        foreach (var n in new[] { "Mua_OngGan", "Mua_OngXa", "MuaHat", "MuaToe", "VomMay" }) Dark21320.Toggle(n, true);
        RenderSettings.fog = true; return Shadows(true);
    }
    public static string Shadows(bool on)
    {
        var sun = RenderSettings.sun; if (!sun) foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional) sun = l;
        var s = $"sun {sun?.name} int {sun?.intensity} shadows {sun?.shadows} strength {sun?.shadowStrength} euler {sun?.transform.eulerAngles} | quality {QualitySettings.names[QualitySettings.GetQualityLevel()]} shadowDist {QualitySettings.shadowDistance} cascades {QualitySettings.shadowCascades} split {QualitySettings.shadowCascade2Split}/{QualitySettings.shadowCascade4Split}";
        if (sun) sun.shadows = on ? LightShadows.Soft : LightShadows.None;
        return s + " -> " + on;
    }
}
public static class Dark21320c
{
    // giả lập máy cảm ứng ở mức đồ họa level (0 Rất cao … 3 Yếu); trả lại PlayerPrefs cũ
    public static string Touch(bool on, int level)
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>();
        int old = PlayerPrefs.GetInt(OpenWorldGame.GfxKey, -1);
        g.forceTouchGraphics = on; g.SetGraphics(level);
        if (old < 0) PlayerPrefs.DeleteKey(OpenWorldGame.GfxKey); else PlayerPrefs.SetInt(OpenWorldGame.GfxKey, old);
        return $"touch {on} gfx {g.Graphics} shadows {QualitySettings.shadows} dist {QualitySettings.shadowDistance} casc {QualitySettings.shadowCascades} oldPref {old}";
    }
}
public static class Dark21320d
{
    public static string FogStart(float start, float end) { RenderSettings.fogStartDistance = start; RenderSettings.fogEndDistance = end; return $"fog {start}-{end}"; }
}
