using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.365 — thử nước văng khi xe người chơi lao xuống sông (Play, Editor tạm dừng, bước từng khung; camera tắt vẽ khi bước — luật GPU).
// Setup() → Drop(speedKmh) (đặt xe trên mặt sông, lao xuống; camera đứng yên nhìn ngang) → After(giây sau khi chạm nước) rồi chụp.
public static class Splash21365
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static Vector3 water;   // run_script biên dịch lại mỗi lần → thời điểm chạm nước lưu ở SessionState
    static float splashAt { get => SessionState.GetFloat("Splash21365.at", -1f); set => SessionState.SetFloat("Splash21365.at", value); }

    public static string Setup()
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); OpenWorldWeather.ForceNext = false; g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        var h = Object.FindAnyObjectByType<OpenWorldHazards>(); if (h) h.enabled = false;
        EditorApplication.isPaused = true;
        Step(.2f);
        return "ok " + P.transform.position + " splashComp=" + (P.GetComponent<OpenWorldWaterSplash>() != null);
    }

    static bool IsWater(Vector3 p)
    {
        if (!Physics.Raycast(new Vector3(p.x, 40f, p.z), Vector3.down, out var hit, 80f, ~0, QueryTriggerInteraction.Ignore)) return true;
        return hit.point.y < OpenWorldDriver.WaterY - .3f;
    }

    static void Step(float sec)
    {
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        float t0 = Time.time; int guard = 0;
        while (Time.time - t0 < sec && guard++ < 2000) EditorApplication.Step();
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
    }

    public static string Drop(float kmh, float camSide, float camBack, float camUp)
    {
        var p = P; var o = p.transform.position;
        // tìm mặt nước gần nhất theo hai bên xe (sông ven đường Tôn Đức Thắng)
        Vector3 best = Vector3.zero; float bestD = 1e9f;
        foreach (var dir in new[] { p.transform.right, -p.transform.right, p.transform.forward })
            for (float d = 4; d < 160; d += 2)
            {
                var q = o + dir * d;
                if (IsWater(q) && IsWater(q + dir * 8) && IsWater(q + dir * 16)) { if (d < bestD) { bestD = d; best = q + dir * 10; } break; }
            }
        if (bestD > 1e8f) return "không thấy mặt nước";
        water = new Vector3(best.x, OpenWorldDriver.WaterY, best.z);
        var inDir = water - o; inDir.y = 0; inDir.Normalize();
        var rb = p.GetComponent<Rigidbody>();
        var start = water - inDir * 6f + Vector3.up * 2.6f;
        rb.position = start; p.transform.position = start; rb.rotation = Quaternion.LookRotation(inDir); p.transform.rotation = rb.rotation;
        rb.linearVelocity = inDir * (kmh / 3.6f) + Vector3.down * 2f; rb.angularVelocity = Vector3.zero;
        var camCtl = Object.FindAnyObjectByType<OpenWorldCamera>();
        if (camBack > 0)          // camBack ≤ 0: giữ camera bám xe của game (góc gần như người chơi thấy)
        {
            if (camCtl) camCtl.enabled = false;
            var cam = Camera.main; var side = Vector3.Cross(Vector3.up, inDir);
            cam.transform.position = water - inDir * camBack + side * camSide + Vector3.up * camUp; cam.transform.LookAt(water + Vector3.up * 1.6f);
        }
        else if (camCtl) camCtl.enabled = true;
        int falls = p.RiverFalls; float t0 = Time.time; splashAt = -1;
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        while (Time.time - t0 < 3f && p.RiverFalls == falls) EditorApplication.Step();
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        splashAt = p.RiverFalls != falls ? Time.time : -1;
        var sp = p.GetComponent<OpenWorldWaterSplash>();
        return $"water {water} dist {bestD} entryVel {p.WaterEntryVelocity.magnitude:F1} m/s splashes {(sp ? sp.Splashes : -1)} at {(splashAt > 0 ? (splashAt - t0).ToString("F2") : "none")}";
    }

    public static string After(float secSinceSplash)
    {
        if (splashAt < 0) return "chưa chạm nước";
        float need = splashAt + secSinceSplash - Time.time; if (need > 0) Step(need);
        EditorApplication.Step();                                         // một khung có camera bật để ảnh chụp có cảnh
        return $"t={Time.time - splashAt:F2}s car y {P.transform.position.y:F2}";
    }
}
