using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.312 — kiểm thử nút KHIÊN (miễn đạn/va chạm, phá lưới điện), trúng đạn → cháy + nổ tung, bố cục nút.
public static class Shield21312
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static OpenWorldHazards H => Object.FindAnyObjectByType<OpenWorldHazards>();
    static OpenWorldShield S => P.GetComponent<OpenWorldShield>();
    static float maxY;

    public static string Start(int gas)
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); g.GoReady(); g.StartRun();
        P.testTouchUI = true; P.testPedal = gas;
        EditorApplication.isPaused = true; for (int i = 0; i < 3; i++) EditorApplication.Step();
        return Layout() + "\n" + Status();
    }

    public static string Layout()
    {
        Rect l = OpenWorldDriver.LeftRect, r = OpenWorldDriver.RightRect, sh = OpenWorldDriver.ShieldRect, f = OpenWorldDriver.FireRect,
             lz = OpenWorldDriver.LaserRect, j = OpenWorldDriver.JumpRect, rev = OpenWorldDriver.RevRect;
        return $"screen {Screen.width}x{Screen.height}\nleft {l}\nright {r}\nshield {sh} centerX={sh.center.x:F1} midGap={(l.xMax + r.x) / 2:F1}\nfire {f}\nlaser {lz} (yMax {lz.yMax:F1}) jump {j} (gap {j.y - lz.yMax:F1}, plateGap {(j.y + j.height * .07f) - (lz.yMax - lz.height * .07f):F1})\nrev {rev}";
    }

    public static string Status()
    {
        var p = P; var s = S;
        return $"t={Time.time:F2} crashed={p.Crashed} burning={p.Burning} netted={p.Netted} shield={(s ? s.Active : false)} left={(s ? s.ActiveLeft : 0):F1} cd={(s ? s.CooldownLeft : 0):F1} " +
               $"blocks={(s ? s.Blocks : 0)} bumps={p.ShieldBumps} nets={OpenWorldShield.NetsBroken} shatters={OpenWorldNetShatter.Spawned} y={p.transform.position.y:F2} maxY={maxY:F2} v={p.GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f:F0} explodeAt={p.ExplodeAt:F2} reason={p.CrashReason} | {H.Report().Substring(0, 60)}";
    }

    public static string Run(float seconds)
    {
        float t0 = Time.time; int n = 0; string s = ""; float next = 0.5f; maxY = P.transform.position.y;
        while (Time.time - t0 < seconds && n < 8000)
        {
            EditorApplication.Step(); n++;
            maxY = Mathf.Max(maxY, P.transform.position.y);
            if (Time.time - t0 >= next) { next += 0.5f; s += Status() + "\n"; }
        }
        return s;
    }

    public static string Shield() => "activate=" + S.TryActivate() + " " + Status();
    public static string Heli() => "heli=" + H.SpawnHelicopterAhead(150f);
    public static string Crash() { P.Crash(P.transform.position + P.transform.forward * 3f, "TEST HÚC", 10f); return Status(); }

    public static string Net()
    {
        var net = Object.FindAnyObjectByType<OpenWorldPoliceNet>();
        typeof(OpenWorldPoliceNet).GetField("readyAt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(net, 0f);
        net.TryFire(3f);
        return "net state=" + net.State + " " + Status();
    }
    public static string NetState() => "net state=" + Object.FindAnyObjectByType<OpenWorldPoliceNet>().State + " " + Status();
    public static void Gas(int g) { P.testPedal = g; }
    public static void End() { P.testPedal = 0; P.testTouchUI = false; }
}
