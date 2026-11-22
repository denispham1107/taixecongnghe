using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.307 — kiểm thử: CSGT dịch chuyển tức thời khi lạc > 70 m; máy bay bắn tên lửa từ xa.
public static class Police21307
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static void Step(float sec) { EditorApplication.isPaused = true; float t0 = Time.time; int n = 0; while (Time.time - t0 < sec && n < 4000) { EditorApplication.Step(); n++; } }
    static string Pols()
    {
        string s = "";
        foreach (var p in OpenWorldPolice.All) if (p) s += $"[{p.name} {p.Mode} d={p.Distance:F1} tp={p.Teleports} pos=({p.transform.position.x:F0},{p.transform.position.z:F0})] ";
        return s;
    }
    public static string Start()
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); if (g.Mode != OpenWorldGame.State.Riding) g.StartRun();
        Step(4f); return "sau 4 s: " + Pols();
    }
    // dịch chuyển xe người chơi tới (x, z) (trên đường), đứng yên; chạy sec giây rồi báo
    public static string Jump(float x, float z, float yaw, float sec)
    {
        var p = P; var rb = p.GetComponent<Rigidbody>(); var pos = new Vector3(x, 0.4f, z); var rot = Quaternion.Euler(0, yaw, 0);
        rb.position = pos; rb.rotation = rot; p.transform.SetPositionAndRotation(pos, rot); rb.linearVelocity = Vector3.zero;
        var before = Pols(); Step(sec);
        return $"trước: {before}\nsau {sec}s: {Pols()} | lastTP {Time.time - OpenWorldPolice.LastTeleportAt:F2}s trước, cách {OpenWorldPolice.LastTeleportDist:F1} m, ahead={OpenWorldPolice.LastTeleportAhead}, tổng {OpenWorldPolice.TeleportsAll}";
    }
    public static string Calm() { var h = Object.FindAnyObjectByType<OpenWorldHazards>(); h.maxShellsInFlight = 0; return "ok"; }
}
