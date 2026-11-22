using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.315 — dàn cảnh chụp ảnh trang chủ (điện thoại 2778×1284): xe bật KHIÊN + bắn LAZER nổ xe phía trước, vụ nổ hai bên đường.
public static class Poster21315
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static OpenWorldHazards H => Object.FindAnyObjectByType<OpenWorldHazards>();
    static OpenWorldTraffic T => Object.FindAnyObjectByType<OpenWorldTraffic>();
    static OpenWorldLaser L => P.GetComponent<OpenWorldLaser>();

    public static string Start(float drive)
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); g.GoReady(); g.StartRun();
        P.testTouchUI = true; P.testPedal = 1;
        EditorApplication.isPaused = true;
        float t0 = Time.time; int n = 0;
        while (Time.time - t0 < drive && n < 5000) { EditorApplication.Step(); n++; }
        return Status();
    }
    public static string Pedal(int p) { P.testPedal = p; return Status(); }

    public static string Place(float ahead, float side, string model)
    {
        var f = P.transform.forward; f.y = 0; f.Normalize(); var r = new Vector3(f.z, 0, -f.x);
        int i = T.TestPlace(P.transform.position + f * ahead + r * side, f, false, 30f, string.IsNullOrEmpty(model) ? null : model);
        return "placed " + i + " " + (i >= 0 ? T.VehicleInfo(i) : "");
    }

    public static string Shield() => "shield=" + P.GetComponent<OpenWorldShield>().TryActivate();
    public static string Laser(bool on) { L.testHold = on; return Status(); }

    public static string Blast(float ahead, float side, bool crater)
    {
        var f = P.transform.forward; f.y = 0; f.Normalize(); var r = new Vector3(f.z, 0, -f.x);
        var at = P.transform.position + f * ahead + r * side; at.y = 0.05f;
        var m = typeof(OpenWorldHazards).GetMethod("Explode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        m.Invoke(H, new object[] { at, false, crater, "THỬ", "nổ dàn cảnh" });
        return "blast at " + at;
    }

    public static string Run(float seconds)
    {
        float t0 = Time.time; int n = 0;
        while (Time.time - t0 < seconds && n < 5000) { EditorApplication.Step(); n++; }
        return Status();
    }

    public static string Status()
    {
        var p = P; var l = L;
        return $"t={Time.time:F2} v={p.GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f:F0} crashed={p.Crashed} shield={p.Shielded} laserFiring={(l ? l.Firing : false)} hits={(l ? l.Hits : 0)} energy={(l ? l.Energy : 0):F0} pos={p.transform.position:F1}";
    }
    public static void End() { P.testPedal = 0; P.testTouchUI = false; L.testHold = false; }
}
