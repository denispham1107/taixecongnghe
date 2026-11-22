using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.317 — thử thời tiết mưa giông trong Play (bước từng khung khi Editor tạm dừng).
public static class Weather21317
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static OpenWorldGame G => Object.FindAnyObjectByType<OpenWorldGame>();
    static OpenWorldWeather W => G.Weather;

    public static string Start(bool rain, float drive, int pedal)
    {
        var g = G; OpenWorldWeather.ForceNext = rain; g.GoReady(); g.StartRun();
        P.testTouchUI = true; P.testPedal = pedal;
        EditorApplication.isPaused = true;
        return Run(drive);
    }
    public static string Ready(bool rain)
    {
        OpenWorldWeather.ForceNext = rain; G.GoReady(); EditorApplication.isPaused = true; return Run(0.1f);
    }
    public static string Pedal(int p) { P.testPedal = p; return Status(); }
    public static string Steer(int x) { P.testSteer = x; return Status(); }
    public static string Shield() => "shield=" + P.GetComponent<OpenWorldShield>().TryActivate();
    public static string Volley() { W.StartVolley(); return Status() + " targets=" + string.Join(" ", W.LastTargets); }

    public static string Run(float seconds)
    {
        float t0 = Time.time; int n = 0;
        while (Time.time - t0 < seconds && n < 6000) { EditorApplication.Step(); n++; }
        return Status();
    }
    // bước tới khi chớp sáng vượt ngưỡng (để chụp ảnh đúng lúc sét đánh)
    public static string UntilFlash(float min, float maxSeconds)
    {
        float t0 = Time.time; int n = 0;
        while (W.Flash < min && Time.time - t0 < maxSeconds && n < 6000) { EditorApplication.Step(); n++; }
        return Status();
    }
    public static string UntilWarn(float maxSeconds)
    {
        float t0 = Time.time; int n = 0;
        while (!W.VolleyActive && Time.time - t0 < maxSeconds && n < 9000) { EditorApplication.Step(); n++; }
        return Status() + " targets=" + string.Join(" ", W.LastTargets);
    }
    public static string Status()
    {
        var p = P; var g = G;
        return $"t={Time.time:F2} dt={Time.deltaTime:F3} mode={g.Mode} v={p.GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f:F0} crashed={p.Crashed} reason={p.CrashReason} pos={p.transform.position:F1} {W.Report()} sun={RenderSettings.sun?.intensity:F2} fog={RenderSettings.fogEndDistance:F0}";
    }
    public static string SetRainTime(float t)
    {
        typeof(OpenWorldWeather).GetProperty("RainTime").SetValue(W, t); return Status();
    }
    public static string ReadyRoll(int n)
    {   // tung thời tiết n lần qua VỀ ĐẦU PHỐ, đếm số lần ra mưa
        int r0 = W.RainRolls, k0 = W.Rolls;
        for (int i = 0; i < n; i++) G.GoReady();
        return $"rain {W.RainRolls - r0}/{W.Rolls - k0} " + Status();
    }
    public static string Teleport(float x, float z, float yaw)
    {
        var rb = P.GetComponent<Rigidbody>(); var pos = new Vector3(x, 0.05f, z); var rot = Quaternion.Euler(0, yaw, 0);
        P.transform.SetPositionAndRotation(pos, rot); rb.position = pos; rb.rotation = rot; rb.linearVelocity = Vector3.zero;
        return Run(0.3f);
    }
    public static string Coast(float x, float z)
    {
        var ok = OpenWorldHazards.CoastPoint(new Vector3(x, 0, z), 150f, 0f, out var p);
        return $"coast={ok} p={p} isCoast={OpenWorldHazards.IsCoast(p)}";
    }
    public static string Roads()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var r in Object.FindAnyObjectByType<OpenWorldTraffic>().roads) sb.Append($"{r.id}:{r.a}->{r.b} main={r.main}; ");
        return sb.ToString();
    }
    public static void End() { P.testPedal = 0; P.testSteer = 0; P.testTouchUI = false; }
}
