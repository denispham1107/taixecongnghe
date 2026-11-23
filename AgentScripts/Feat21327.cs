using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.327 — thử: tỉ lệ mưa, Ga tự động (chạm 2 lần), loạt 2 tên lửa khóa 2 mục tiêu, Lazer khóa 5 s.
// Bước khung với camera cullingMask = 0 (tránh GPU bị reset — 21.324).
public static class Feat21327
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static OpenWorldGame G => Object.FindAnyObjectByType<OpenWorldGame>();
    static OpenWorldHazards H => Object.FindAnyObjectByType<OpenWorldHazards>();
    static readonly FieldInfo SpeedF = typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

    static void Steps(float sec, System.Action each = null)
    {
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        float t0 = Time.time; int n = 0;
        while (Time.time - t0 < sec && n < 3000) { each?.Invoke(); EditorApplication.Step(); n++; }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
    }

    public static string Ready() { OpenWorldWeather.ForceNext = false; G.GoReady(); EditorApplication.isPaused = true; Steps(0.2f); return "ready " + G.Mode; }

    // tỉ lệ mưa qua n lần tung (không chạy cảnh)
    public static string Rolls(int n)
    {
        var w = G.Weather; int r0 = w.RainRolls, t0 = w.Rolls;
        for (int i = 0; i < n; i++) w.Roll(true);
        return $"mưa {w.RainRolls - r0}/{w.Rolls - t0} = {(float)(w.RainRolls - r0) / (w.Rolls - t0):P0} (rainChance {w.rainChance})";
    }

    public static string Start()
    {
        OpenWorldWeather.ForceNext = false; G.GoReady(); G.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        EditorApplication.isPaused = true; Steps(0.3f);
        return "run " + G.Mode + " pos " + P.transform.position;
    }

    // Ga tự động: chạy tới kmh bằng Ga, thả, chạm 2 lần, để 4 s không ga → tốc độ còn? rồi bấm Lùi 1 nhịp → tắt?
    public static string Cruise(float kmh)
    {
        var p = P; p.testTouchUI = true; p.testPedal = 1;
        Steps(8f, () => { if (p.Speed * 3.6f >= kmh) p.testPedal = 0; });
        p.testPedal = 0;
        float v0 = p.Speed * 3.6f;
        p.TestGasTap(); Steps(0.1f); p.TestGasTap();
        bool on = p.Cruise; float cs = p.CruiseSpeed * 3.6f;
        Steps(4f);
        float v4 = p.Speed * 3.6f; bool still = p.Cruise;
        // giữ Ga 1 s khi đang tự động → tốc độ giữ mới
        p.testPedal = 1; Steps(1f); p.testPedal = 0; Steps(1.5f);
        float vUp = p.Speed * 3.6f, csUp = p.CruiseSpeed * 3.6f;
        return $"bật lúc {v0:F0} km/h → cruise {on} giữ {cs:F0} | sau 4 s không ga: {v4:F0} km/h (cruise {still}) | giữ Ga 1 s rồi thả 1,5 s: {vUp:F0} km/h, giữ mới {csUp:F0}";
    }
    public static string CruiseOff(string how)
    {
        var p = P; bool before = p.Cruise;
        if (how == "tap") { p.TestGasTap(); Steps(0.1f); p.TestGasTap(); Steps(2f); }
        else if (how == "rev") { p.testPedal = -1; Steps(0.2f); p.testPedal = 0; Steps(1f); }
        return $"{how}: cruise {before} → {p.Cruise}, tốc độ {p.Speed * 3.6f:F0} km/h";
    }
    public static string CruiseOn() { var p = P; p.TestGasTap(); Steps(0.1f); p.TestGasTap(); return "cruise " + p.Cruise + " " + (p.CruiseSpeed * 3.6f).ToString("F0"); }

    // loạt tên lửa: sinh 1 xe tăng + 1 trực thăng phía trước, bắn 1 lần, theo dõi 3 s
    public static string Salvo(int tanks, int helis)
    {
        var h = H; var p = P;
        for (int i = 0; i < tanks; i++) h.SpawnTankAhead(70f + i * 25f, i, false);
        for (int i = 0; i < helis; i++) h.SpawnHelicopterAhead(90f + i * 30f, false);
        Steps(0.5f);
        int m0 = h.PlayerMissiles, f0 = h.PlayerMissilesFired, td0 = h.TanksDestroyed, hd0 = h.HelisDowned;
        h.RequestFire(); Steps(0.1f);
        string salvo = $"bắn {h.LastSalvo} quả, khóa {h.LastSalvoTargets} mục tiêu, kho {m0}→{h.PlayerMissiles}";
        Steps(3f);
        return salvo + $" | sau 3 s: xe tăng hạ +{h.TanksDestroyed - td0}, trực thăng hạ +{h.HelisDowned - hd0}";
    }
    public static string Regen()
    {
        var h = H; int m0 = h.PlayerMissiles; Steps(7.4f);
        return $"kho {m0} → {h.PlayerMissiles} sau 7,4 s (regen {h.missileRegenSeconds} s)";
    }

    // Lazer: giữ 0,6 s rồi thả → khóa bao lâu?
    public static string Laser()
    {
        var lz = P.GetComponent<OpenWorldLaser>(); lz.testHold = true; Steps(0.6f); lz.testHold = false; Steps(0.05f);
        float cd = lz.CooldownLeft; float t = 0; Steps(6f, () => { if (lz.CooldownLeft > 0f) t += Time.deltaTime; });
        return $"khóa ban đầu {cd:F2} s, hết khóa sau ~{t:F2} s, năng lượng {lz.Energy}";
    }

    public static void End() { var p = P; p.testPedal = 0; p.testTouchUI = false; }
}
public static class Feat21327b
{
    // theo dõi từng tên lửa người chơi đang bay mỗi 0,2 s
    public static string Track(float sec)
    {
        var h = Object.FindAnyObjectByType<OpenWorldHazards>();
        var f = typeof(OpenWorldHazards).GetField("pmissiles", BindingFlags.NonPublic | BindingFlags.Instance);
        var tf = typeof(OpenWorldHazards).GetField("tanks", BindingFlags.NonPublic | BindingFlags.Instance);
        var hf = typeof(OpenWorldHazards).GetField("helis", BindingFlags.NonPublic | BindingFlags.Instance);
        var sb = new System.Text.StringBuilder();
        foreach (var t in (IList)tf.GetValue(h)) { var r = (Transform)t.GetType().GetField("root").GetValue(t); if (r) sb.Append($"tank {r.position:F0} destroyed {t.GetType().GetField("destroyed").GetValue(t)}; "); }
        foreach (var t in (IList)hf.GetValue(h)) { var r = (Transform)t.GetType().GetField("root").GetValue(t); if (r) sb.Append($"heli {r.position:F0}; "); }
        sb.AppendLine();
        h.RequestFire();
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask); foreach (var c in cams) c.cullingMask = 0;
        float t0 = Time.time, next = 0;
        while (Time.time - t0 < sec)
        {
            EditorApplication.Step();
            if (Time.time - t0 >= next)
            {
                next += 0.2f; sb.Append($"{Time.time - t0:F1}: ");
                foreach (var m in (IList)f.GetValue(h))
                {
                    var r = (Transform)m.GetType().GetField("root").GetValue(m);
                    var tk = m.GetType().GetField("tank").GetValue(m); var he = m.GetType().GetField("heli").GetValue(m); int b = (int)m.GetType().GetField("boat").GetValue(m);
                    sb.Append($"[{r.position:F0} {(tk != null ? "T" : he != null ? "H" : b >= 0 ? "B" : "-")}] ");
                }
                sb.AppendLine();
            }
        }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        return sb.ToString() + $" tanks hạ {h.TanksDestroyed}, heli hạ {h.HelisDowned}, salvo {h.LastSalvo}/{h.LastSalvoTargets}";
    }
}
public static class Feat21327c
{
    public static string Spawn(float tankDist, float heliDist)
    {
        var h = Object.FindAnyObjectByType<OpenWorldHazards>();
        bool t = h.SpawnTankAhead(tankDist, 0, false), he = h.SpawnHelicopterAhead(heliDist, false);
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask); foreach (var c in cams) c.cullingMask = 0;
        float t0 = Time.time; while (Time.time - t0 < 1.5f) EditorApplication.Step();
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        return $"tank {t} heli {he} enabled {h.enabled}";
    }
}
public static class Feat21327d
{
    public static string State()
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); var p = Object.FindAnyObjectByType<OpenWorldDriver>();
        return $"mode {g.Mode} crashed {p.Crashed} reason {p.CrashReason} frozen {p.frozen}";
    }
}
public static class Feat21327e
{
    // đặt tốc độ kmh rồi chạm 2 lần (bật Ga tự động), chạy 1,5 s
    public static string ForceCruise(float kmh, bool touch)
    {
        var p = Object.FindAnyObjectByType<OpenWorldDriver>(); var rb = p.GetComponent<Rigidbody>();
        p.testTouchUI = touch;
        var f = typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        f.SetValue(p, kmh / 3.6f); rb.linearVelocity = p.transform.forward * (kmh / 3.6f);
        p.TestGasTap(); EditorApplication.Step(); p.TestGasTap();
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask); foreach (var c in cams) c.cullingMask = 0;
        float t0 = Time.time; while (Time.time - t0 < 1.5f) EditorApplication.Step();
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        return $"cruise {p.Cruise} {p.CruiseSpeed * 3.6f:F0} km/h, tốc độ {p.Speed * 3.6f:F0}";
    }
}
public static class Feat21328
{
    // Lazer chạm 1 lần (1 khung): bắn bao lâu, khóa bao lâu, năng lượng sau khóa
    public static string LaserTap()
    {
        var lz = Object.FindAnyObjectByType<OpenWorldDriver>().GetComponent<OpenWorldLaser>();
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask); foreach (var c in cams) c.cullingMask = 0;
        lz.testHold = true; EditorApplication.Step(); lz.testHold = false;
        bool started = lz.Firing; float fire = 0, lockT = 0, t0 = Time.time; int shots0 = lz.Shots;
        while (Time.time - t0 < 9f) { EditorApplication.Step(); if (lz.Firing) fire += Time.deltaTime; else if (lz.CooldownLeft > 0f) lockT += Time.deltaTime; }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        return $"bắt đầu {started}, bắn {fire:F2} s sau 1 lần chạm, khóa {lockT:F2} s, năng lượng {lz.Energy}, Ready {lz.Ready}";
    }
}
