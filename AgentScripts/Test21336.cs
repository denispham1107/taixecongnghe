using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.336 — thử: xe tăng trúng tên lửa nổ tung + biến mất; xe NPC né sét (so sánh bật/tắt né).
// (Trước đó chạy Reimport21336.Run để Unity biên dịch mã mới.) Bước khung với cullingMask = 0 (tránh GPU reset — 21.324).
//   Start → Tank → Volleys(n,true) → Volleys(n,false) → End
public static class Test21336
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static OpenWorldGame G => Object.FindAnyObjectByType<OpenWorldGame>();
    static OpenWorldHazards H => Object.FindAnyObjectByType<OpenWorldHazards>();
    static OpenWorldTraffic T => Object.FindAnyObjectByType<OpenWorldTraffic>();

    static void Steps(float sec, System.Action each = null)
    {
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        float t0 = Time.time; int n = 0;
        while (Time.time - t0 < sec && n < 4000) { each?.Invoke(); EditorApplication.Step(); n++; }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
    }

    public static string Start(bool rain)
    {
        OpenWorldWeather.ForceNext = rain; G.GoReady(); G.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        EditorApplication.isPaused = true; Steps(0.5f);
        return "run " + G.Mode + " rain " + G.Weather.Raining + " pos " + P.transform.position;
    }

    static int TankCount() => ((IList)typeof(OpenWorldHazards).GetField("tanks", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(H)).Count;

    public static string Tank()
    {
        var h = H; int c0 = TankCount();
        h.SpawnTankAhead(60f, 0, false); Steps(0.4f);
        var list = (IList)typeof(OpenWorldHazards).GetField("tanks", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(h);
        var tank = list[list.Count - 1]; var root = (Transform)tank.GetType().GetField("root").GetValue(tank);
        int rOn0 = 0; foreach (var r in root.GetComponentsInChildren<Renderer>()) if (r.enabled) rOn0++;
        int td0 = h.TanksDestroyed; h.RequestFire(); float hitT = -1; float t0 = Time.time;
        Steps(3f, () => { if (hitT < 0 && h.TanksDestroyed > td0) hitT = Time.time - t0; });
        bool alive = root; int rOn = 0, colOn = 0;
        if (alive) { foreach (var r in root.GetComponentsInChildren<Renderer>()) if (r.enabled && !(r is ParticleSystemRenderer)) rOn++; foreach (var c in root.GetComponentsInChildren<Collider>()) if (c.enabled) colOn++; }
        Steps(2.5f);
        return $"xe tăng: {c0}→sinh, renderer bật {rOn0}; trúng sau {hitT:F2} s (hạ +{h.TanksDestroyed - td0}); 3 s sau bắn: gốc còn={alive} renderer bật={rOn} collider bật={colOn}; 5,5 s: gốc còn={(bool)root} danh sách {TankCount()}";
    }

    // ảnh: sinh xe tăng 45 m, bắn, bước tới lúc trúng rồi thêm `after` giây (sau đó chụp Game view)
    public static string TankShot(float after)
    {
        var h = H; h.SpawnTankAhead(45f, 0, false); Steps(0.4f);
        int td0 = h.TanksDestroyed; h.RequestFire();
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        int n = 0; while (h.TanksDestroyed == td0 && n < 400) { EditorApplication.Step(); n++; }
        float t0 = Time.time; while (Time.time - t0 < after && n < 800) { EditorApplication.Step(); n++; }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        return "hạ +" + (h.TanksDestroyed - td0) + " sau " + n + " bước";
    }

    // ảnh: bắt đầu đợt sét, bước `sec` giây (trong 1 s cảnh báo) rồi chụp; trả số xe đang né
    public static string VolleyShot(float sec)
    {
        var t = T; int k = 0;
        for (; k < 12; k++) { G.Weather.StartVolley(); Steps(sec); if (t.DodgingNow >= 2) break; Steps(1.6f); }
        return "lần " + k + " đang né " + t.DodgingNow + " | " + G.Weather.Report();
    }

    // n đợt sét gọi tay, mỗi đợt bước 2,2 s; xe người chơi đứng yên giữa phố
    public static string Volleys(int n, bool dodge)
    {
        var t = T; var h = H; var w = G.Weather; t.dodgeLightning = dodge;
        int hit0 = h.LightningNpc, b0 = t.StrikeBrakes, e0 = t.StrikeEscapes, r0 = t.StrikeRushes, v0 = w.Volleys, k0 = h.NpcKnocked; int maxDodge = 0;
        var p = P; p.testPedal = 0;
        for (int i = 0; i < n; i++)
        {
            w.StartVolley();
            Steps(2.2f, () => { maxDodge = Mathf.Max(maxDodge, t.DodgingNow); p.testPedal = 0; });
            if (p.Crashed) return "người chơi bị trúng — dừng ở đợt " + i;
        }
        t.dodgeLightning = true;
        string log = string.Join(" | ", t.LightningLog); t.LightningLog.Clear();
        return $"né={dodge}: {w.Volleys - v0} đợt, NPC trúng sét {h.LightningNpc - hit0}, bị hất {h.NpcKnocked - k0}; phanh {t.StrikeBrakes - b0} / thoát {t.StrikeEscapes - e0} / vọt qua {t.StrikeRushes - r0}; tối đa đang né cùng lúc {maxDodge}; xe đang chạy {Active()} || trúng: {log}";
    }
    static int Active() { int n = 0; foreach (var x in Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude)) n++; return n; }

    public static string Report()
    {
        var w = Object.FindAnyObjectByType<OpenWorldWeather>(); var t = T; var h = H;
        return (w ? w.Report() : "no weather") + $" | pending={t.PendingStrikes} dodgingNow={t.DodgingNow} tanksDestroyed={h.TanksDestroyed}";
    }

    public static void End() { var p = P; if (p) { p.testPedal = 0; p.testTouchUI = false; } }
}
