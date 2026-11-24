using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.369 — thử âm thanh rơi sông/biển: đặt xe cao trên mặt sông, lao xuống; ghi theo thời gian độ cao xe (21.371: đã bỏ tiếng gió),
// lúc chạm nước, lúc nóc xe ngập hẳn (để khớp «ực» trong PV_AmXeChim.wav), lúc xe nằm đáy, trạng thái nguồn âm.
public static class SinkAudio21369
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();

    public static string Setup()
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); OpenWorldWeather.ForceNext = false; g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        var h = Object.FindAnyObjectByType<OpenWorldHazards>(); if (h) h.enabled = false;
        EditorApplication.isPaused = true;
        int g0 = 0; while (g0++ < 12) EditorApplication.Step();
        var sp = P.GetComponent<OpenWorldWaterSplash>(); var srcs = sp ? sp.GetComponentsInChildren<AudioSource>() : null;
        string clips = ""; foreach (var a in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include)) if (a.clip && a.clip.name.StartsWith("PV_Am")) clips += a.clip.name + "(" + a.clip.length.ToString("F2") + "s, " + a.clip.channels + "ch) ";
        return "ok " + P.transform.position + " | " + clips;
    }

    static bool IsWater(Vector3 p)
    {
        if (!Physics.Raycast(new Vector3(p.x, 40f, p.z), Vector3.down, out var hit, 80f, ~0, QueryTriggerInteraction.Ignore)) return true;
        return hit.point.y < OpenWorldDriver.WaterY - .3f;
    }

    // chạy thời gian thật (Editor không tạm dừng — khi tạm dừng AudioSource.isPlaying luôn false): đặt xe rồi bỏ tạm dừng
    public static string DropLive(float height, float kmh)
    {
        var r = Place(height, kmh); if (r != null) return r;
        EditorApplication.isPaused = false; return "đang rơi (thời gian thật)";
    }

    public static string Report()
    {
        var p = P; var sp = p.GetComponent<OpenWorldWaterSplash>(); var sb = new System.Text.StringBuilder();
        sb.Append($"y {p.transform.position.y:F2} chìm {p.Sinking} ngập sau chạm {(sp.SubmergedAt > 0 ? (sp.SubmergedAt - Time.time).ToString("F2") + "s trước" : "chưa")} | ");
        foreach (var a in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
            if (a.clip && a.clip.name.StartsWith("PV_Am")) sb.Append($"{a.clip.name}: {(a.isPlaying ? "PHÁT" : "tắt")} {a.time:F2}s vol {a.volume:F2} pitch {a.pitch:F2}; ");
        return sb.ToString();
    }

    static string Place(float height, float kmh)
    {
        var p = P; var o = p.transform.position; var rb = p.GetComponent<Rigidbody>();
        Vector3 best = Vector3.zero; float bestD = 1e9f;
        foreach (var dir in new[] { p.transform.right, -p.transform.right, p.transform.forward })
            for (float d = 4; d < 160; d += 2)
            {
                var q = o + dir * d;
                if (IsWater(q) && IsWater(q + dir * 8) && IsWater(q + dir * 16)) { if (d < bestD) { bestD = d; best = q + dir * 10; } break; }
            }
        if (bestD > 1e8f) return "không thấy mặt nước";
        var water = new Vector3(best.x, OpenWorldDriver.WaterY, best.z);
        var inDir = water - o; inDir.y = 0; inDir.Normalize();
        float tFall = Mathf.Sqrt(2 * height / 9.81f);
        var start = water - inDir * (kmh / 3.6f * tFall) + Vector3.up * height;
        p.ResetTo(start, Quaternion.LookRotation(inDir));
        rb.linearVelocity = inDir * (kmh / 3.6f);
        typeof(OpenWorldDriver).GetProperty("Speed").SetValue(p, kmh / 3.6f);
        return null;
    }

    public static string Drop(float height, float kmh)
    {
        var p = P; var o = p.transform.position; var rb = p.GetComponent<Rigidbody>(); var sp = p.GetComponent<OpenWorldWaterSplash>();
        Vector3 best = Vector3.zero; float bestD = 1e9f;
        foreach (var dir in new[] { p.transform.right, -p.transform.right, p.transform.forward })
            for (float d = 4; d < 160; d += 2)
            {
                var q = o + dir * d;
                if (IsWater(q) && IsWater(q + dir * 8) && IsWater(q + dir * 16)) { if (d < bestD) { bestD = d; best = q + dir * 10; } break; }
            }
        if (bestD > 1e8f) return "không thấy mặt nước";
        var water = new Vector3(best.x, OpenWorldDriver.WaterY, best.z);
        var inDir = water - o; inDir.y = 0; inDir.Normalize();
        float tFall = Mathf.Sqrt(2 * height / 9.81f);
        var start = water - inDir * (kmh / 3.6f * tFall) + Vector3.up * height;
        p.ResetTo(start, Quaternion.LookRotation(inDir));
        rb.linearVelocity = inDir * (kmh / 3.6f);
        typeof(OpenWorldDriver).GetProperty("Speed").SetValue(p, kmh / 3.6f); p.scriptedInput = new Vector2(0, 1);   // giữ Ga để giữ đà bay ra
        var col = p.GetComponent<Collider>();
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        var sb = new System.Text.StringBuilder(); float t0 = Time.time, hitAt = -1, subAt = -1, bottomAt = -1, lastLog = -1;
        int falls = p.RiverFalls; float sub0 = sp.SubmergedAt;
        var srcs = new System.Collections.Generic.List<AudioSource>();
        foreach (var au in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include)) if (au.clip && au.clip.name.StartsWith("PV_Am")) srcs.Add(au);
        while (Time.time - t0 < tFall + 5f)
        {
            EditorApplication.Step();
            float t = Time.time - t0;
            if (hitAt < 0 && p.RiverFalls != falls) hitAt = t;
            if (hitAt >= 0 && subAt < 0 && sp.SubmergedAt != sub0) subAt = t;
            if (hitAt >= 0 && bottomAt < 0 && p.transform.position.y < -5.5f) bottomAt = t;
            if (t - lastLog >= .1f) { lastLog = t; { string v = ""; foreach (var au in srcs) v += au.clip.name.Substring(5) + " " + au.volume.ToString("F2") + " "; sb.AppendLine($"t {t:F2} y {p.transform.position.y:F2} top {col.bounds.max.y:F2} vy {rb.linearVelocity.y:F1} | {v}"); } }
            if (bottomAt > 0 && t > bottomAt + .5f) break;
        }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        p.scriptedInput = Vector2.zero;
        sb.Insert(0, $"rơi {height} m, {kmh} km/h: chạm nước {hitAt:F2}s; nóc ngập sau chạm {(subAt - hitAt):F2}s; nằm đáy sau chạm {(bottomAt - hitAt):F2}s; cao xe {col.bounds.size.y:F2}\n");
        return sb.ToString();
    }
}
