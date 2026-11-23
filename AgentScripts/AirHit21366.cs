using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.366 — thử: xe người chơi đang nhảy đụng vật cản (cây, cột, tường, xe…) phải rơi xuống đất ngay, không treo lơ lửng.
// Setup() → List(r) (vật cản cao quanh xe) → Jump(lọc tên, kmh, cách m) (đặt xe trên đường, lao thẳng vào vật + nhảy, ghi độ cao).
public static class AirHit21366
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();

    public static string Setup()
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); OpenWorldWeather.ForceNext = false; g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        var h = Object.FindAnyObjectByType<OpenWorldHazards>(); if (h) h.enabled = false;
        EditorApplication.isPaused = true;
        Step(.2f);
        return "ok " + P.transform.position;
    }

    static void Step(float sec)
    {
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        float t0 = Time.time; int guard = 0;
        while (Time.time - t0 < sec && guard++ < 4000) EditorApplication.Step();
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
    }

    // nhảy trên đường trống (không vật cản): phải đủ đỉnh ~4,1 m, không bị tính là cú đụng
    public static string FreeJump(float kmh, float turn)
    {
        var p = P; var rb = p.GetComponent<Rigidbody>(); p.ResetTo(p.transform.position, p.transform.rotation * Quaternion.Euler(0, turn, 0)); var dir = p.transform.forward; dir.y = 0; dir.Normalize();
        typeof(OpenWorldDriver).GetProperty("Speed").SetValue(p, kmh / 3.6f); rb.linearVelocity = dir * kmh / 3.6f;
        p.scriptedInput = new Vector2(0, 1); Step(.1f); p.RequestJump();
        int k0 = p.AirKnocks; float rs = Road(p.transform.position), top = 0, t0 = Time.time, land = -1;
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        while (Time.time - t0 < 2.5f) { EditorApplication.Step(); top = Mathf.Max(top, p.transform.position.y - rs); if (Time.time - t0 > .3f && !p.Airborne) { land = Time.time - t0; break; } }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        p.scriptedInput = Vector2.zero;
        return $"đỉnh {top:F2} m, đáp {land:F2} s, cú đụng {p.AirKnocks - k0}, tốc độ sau {p.Speed * 3.6f:F0} km/h";
    }

    public static string After(float sec)
    {
        Step(sec); EditorApplication.Step();                              // một khung có camera để chụp
        var p = P; P.scriptedInput = Vector2.zero;
        return $"y {p.transform.position.y - Road(p.transform.position):F2} airborne {p.Airborne} knock {p.AirKnocked} knocks {p.AirKnocks}";
    }

    static float Road(Vector3 p)
    {
        var hits = Physics.RaycastAll(new Vector3(p.x, p.y + 30f, p.z), Vector3.down, 60f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.NaN;
        foreach (var hh in hits) if (hh.collider.name == "MatDat" || hh.collider.name == "MatCau") if (float.IsNaN(best) || hh.point.y > best) best = hh.point.y;
        return best;
    }

    // tia ngang ở độ cao 2,5 m từ các điểm quanh xe → vật cản cao có mặt đứng
    static List<(Collider c, Vector3 pt, Vector3 n, Vector3 from)> Scan(float radius, string filter)
    {
        var res = new List<(Collider, Vector3, Vector3, Vector3)>(); var seen = new HashSet<Collider>();
        var o = P.transform.position; float r0 = Road(o); if (float.IsNaN(r0)) r0 = o.y;
        for (float x = -radius; x <= radius; x += 6f)
            for (float z = -radius; z <= radius; z += 6f)
            {
                var s = new Vector3(o.x + x, 0, o.z + z); float rs = Road(s); if (float.IsNaN(rs) || Mathf.Abs(rs - r0) > 1.5f) continue;
                bool inside = false;                                          // điểm xuất phát không được nằm trong nhà/vật
                foreach (var oc in Physics.OverlapSphere(new Vector3(s.x, rs + 1.2f, s.z), 1.6f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore))
                    if (oc.name != "MatDat" && oc.name != "MatCau" && !oc.attachedRigidbody) inside = true;
                if (inside) continue;
                for (int k = 0; k < 16; k++)
                {
                    float a = k * Mathf.PI / 8f; var d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                    var from = new Vector3(s.x, rs + 2.5f, s.z);
                    if (!Physics.Raycast(from, d, out var hit, 10f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore)) continue;
                    if (hit.distance < 9.5f || hit.collider.name == "MatDat" || hit.collider.name == "MatCau" || Mathf.Abs(hit.normal.y) > .3f) continue;
                    if (filter != "" && !Path(hit.collider.transform).ToLowerInvariant().Contains(filter.ToLowerInvariant())) continue;
                    if (seen.Contains(hit.collider)) continue;
                    // đường chạy thấp (0,6 m) phải trống tới gần vật
                    if (Physics.Raycast(new Vector3(s.x, rs + 0.6f, s.z), d, hit.distance - 2.5f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore)) continue;
                    seen.Add(hit.collider); res.Add((hit.collider, hit.point, hit.normal, new Vector3(s.x, rs, s.z)));
                }
            }
        return res;
    }

    static string Path(Transform t) { var s = t.name; for (int i = 0; i < 3 && t.parent; i++) { t = t.parent; s = t.name + "/" + s; } return s; }

    public static string List(float radius)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var e in Scan(radius, "")) sb.AppendLine($"{Path(e.c.transform)} size {e.c.bounds.size:F1} at {e.pt:F1}");
        return sb.Length > 0 ? sb.ToString() : "không có";
    }

    // xe NPC đứng yên (tắt giao thông): lao từ phía sau xe theo trục thân xe
    public static string JumpAtCar(float kmh)
    {
        foreach (var t in Object.FindObjectsByType<OpenWorldTraffic>(FindObjectsInactive.Exclude)) t.enabled = false;
        Collider best = null; float bd = 1e9f; var o = P.transform.position;
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude))
            if (c.name == "OtoNPC" && c.attachedRigidbody && !float.IsNaN(Road(c.bounds.center)))
            { float d = (c.bounds.center - o).sqrMagnitude; if (d < bd) { bd = d; best = c; } }
        if (!best) return "không thấy OtoNPC";
        var rbn = best.attachedRigidbody; rbn.linearVelocity = Vector3.zero; rbn.isKinematic = true;
        var fwd = best.transform.forward; fwd.y = 0; fwd.Normalize();
        var pt = best.bounds.center - fwd * (best.bounds.extents.magnitude * .6f);
        return Run(best, pt, fwd, kmh, -1f);
    }

    public static string Jump(string filter, float kmh, int index) => JumpStop(filter, kmh, index, -1f);
    public static string JumpStop(string filter, float kmh, int index, float stopAfterHit)
    {
        var all = Scan(70f, filter); if (all.Count == 0) return "không thấy vật cản: " + filter;
        var e = all[Mathf.Clamp(index, 0, all.Count - 1)];
        var dir0 = e.pt - e.from; dir0.y = 0;
        return Run(e.c, e.pt, dir0.normalized, kmh, stopAfterHit);
    }

    static string Run(Collider target, Vector3 pt, Vector3 dir, float kmh, float stopAfterHit)
    {
        var e = (c: target, pt: pt);
        var p = P; var rb = p.GetComponent<Rigidbody>();
        // xuất phát ~0,5 s trước vật (đỉnh nhảy ≈ 0,58 s), trên mặt đường
        float back = Mathf.Max(6f, kmh / 3.6f * .5f);
        var start = e.pt - dir * back; float rs = Road(start); if (float.IsNaN(rs)) rs = 0f; start.y = rs + 0.05f;
        p.ResetTo(start, Quaternion.LookRotation(dir));
        typeof(OpenWorldDriver).GetProperty("Speed").SetValue(p, kmh / 3.6f);
        rb.linearVelocity = dir * kmh / 3.6f;
        p.scriptedInput = new Vector2(0, 1);
        Step(.1f);
        p.RequestJump();
        int knocks0 = p.AirKnocks; var sb = new System.Text.StringBuilder();
        sb.AppendLine($"vật {Path(e.c.transform)} cao {e.c.bounds.size.y:F1} m, chạy {kmh} km/h từ {back:F1} m; đường y {rs:F2}");
        float t0 = Time.time, hitAt = -1, landAt = -1, hitY = 0; float maxStuck = 0, stuckY = -99, stuckT = 0;
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        float lastLog = -1;
        while (Time.time - t0 < 3f)
        {
            EditorApplication.Step();
            float t = Time.time - t0, y = p.transform.position.y - rs;
            if (hitAt < 0 && p.AirKnocks != knocks0) { hitAt = t; hitY = y; }
            if (hitAt >= 0 && landAt < 0 && !p.AirKnocked && !p.Airborne) landAt = t;
            // treo: độ cao > 0,8 m mà gần như không đổi
            if (y > .8f && Mathf.Abs(y - stuckY) < .03f) { stuckT += Time.deltaTime; maxStuck = Mathf.Max(maxStuck, stuckT); } else { stuckT = 0; stuckY = y; }
            if (t - lastLog >= .1f) { lastLog = t; sb.AppendLine($"t {t:F2} y {y:F2} vy {rb.linearVelocity.y:F1} speed {p.Speed * 3.6f:F0} jump {p.Airborne} knock {p.AirKnocked}"); }
            if (landAt > 0 && t > landAt + .3f) break;
            if (stopAfterHit >= 0 && hitAt >= 0 && t >= hitAt + stopAfterHit) break;
        }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        if (stopAfterHit >= 0) EditorApplication.Step(); else p.scriptedInput = Vector2.zero;   // một khung có camera để chụp
        sb.Insert(0, $"KẾT QUẢ: đụng lúc {hitAt:F2}s ở {hitY:F2} m, chạm đất {landAt:F2}s → rơi {(landAt > 0 && hitAt > 0 ? landAt - hitAt : -1):F2}s, treo lâu nhất {maxStuck:F2}s\n");
        return sb.ToString();
    }
}
