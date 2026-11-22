using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.308 — kiểm thử trực thăng / máy bay bắn tên lửa khi xe ĐANG CHẠY (giữ ga bằng testPedal).
public static class Fire21308
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static OpenWorldHazards H => Object.FindAnyObjectByType<OpenWorldHazards>();
    static void Put(float x, float z, float yaw)
    {
        var p = P; var rb = p.GetComponent<Rigidbody>(); var pos = new Vector3(x, 0.4f, z); var rot = Quaternion.Euler(0, yaw, 0);
        rb.position = pos; rb.rotation = rot; p.transform.SetPositionAndRotation(pos, rot); rb.linearVelocity = Vector3.zero;
    }
    public static string Run(string what, float x, float z, float yaw, float seconds, int gas)
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); if (g.Mode != OpenWorldGame.State.Riding || P.Crashed) { g.GoReady(); g.StartRun(); }
        var h = H; h.maxShellsInFlight = 0;                    // không đạn pháo (chỉ đo tên lửa)
        var pols = Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude); foreach (var pol in pols) pol.enabled = false;
        Put(x, z, yaw); P.testTouchUI = gas > 0; P.testPedal = gas > 0 ? 1 : 0;
        EditorApplication.isPaused = true; for (int i = 0; i < 3; i++) EditorApplication.Step();
        if (what == "heli") h.SpawnHelicopterAhead(160f);
        int f0 = h.MissilesFired; string s = ""; float t0 = Time.time, next = 1f; int n = 0;
        while (Time.time - t0 < seconds && n < 6000)
        {
            EditorApplication.Step(); n++;
            if (Time.time - t0 >= next)
            {
                next += 1f; string ac = "";
                foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
                    if (t.name == "TrucThang" || t.name == "MayBayCanhKep")
                    { var d = t.position - P.transform.position; ac += $"{(t.name == "TrucThang" ? "H" : "M")}{d.magnitude:F0}m/fwdDot{Vector3.Dot(t.forward, -d.normalized):F1} "; }
                s += $"t={Time.time - t0:F0} v={P.GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f:F0}km/h fired={h.MissilesFired - f0} crashed={P.Crashed} {ac}\n";
                if (P.Crashed) break;
            }
        }
        
        P.testPedal = 0; P.testTouchUI = false;
        foreach (var pol in pols) if (pol) pol.enabled = true;
        return s;
    }
}
