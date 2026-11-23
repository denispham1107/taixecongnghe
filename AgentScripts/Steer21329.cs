using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.329 — thử lái gốc 1.89.0 (classicSteer) so với lái 21.316–21.319: chạy 60 km/h, giữ Phải 0,5 s, đo tốc độ quay thân xe.
// Bước khung với camera cullingMask = 0 (tránh GPU bị reset — 21.324). Dùng sau Feat21327.Start.
public static class Steer21329
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static readonly FieldInfo SpeedF = typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

    static void Steps(float sec, System.Action each = null)
    {
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, c => c.cullingMask);
        foreach (var c in cams) c.cullingMask = 0;
        float t0 = Time.time; int n = 0;
        while (Time.time - t0 < sec && n < 3000) { each?.Invoke(); EditorApplication.Step(); n++; }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
    }

    static float Yaw(Transform t) { var f = t.forward; return Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg; }

    public static string Run(bool classic, float kmh)
    {
        var p = P; p.classicSteer = classic;
        var cam = Object.FindAnyObjectByType<OpenWorldCamera>();
        // đặt tốc độ thẳng (tránh kẹt làn), giữ Ga cho ổn định 0,3 s
        p.scriptedInput = new Vector2(0f, 1f);
        Steps(0.3f, () => SpeedF.SetValue(p, kmh / 3.6f));
        float y0 = Yaw(p.transform), last = y0, maxRate = 0f, t5 = -1f, roll = 0f, slip = 0f;
        float c0 = Yaw(cam.transform), camMax = 0f, cLast = c0; float t0 = Time.time;
        p.scriptedInput = new Vector2(1f, 1f);
        Steps(0.5f, () =>
        {
            float y = Yaw(p.transform), r = Mathf.DeltaAngle(last, y) / Time.fixedDeltaTime; last = y;
            maxRate = Mathf.Max(maxRate, Mathf.Abs(r));
            if (t5 < 0 && Mathf.Abs(Mathf.DeltaAngle(y0, y)) >= 5f) t5 = Time.time - t0;
            roll = Mathf.Max(roll, Mathf.Abs(p.BodyRoll)); slip = Mathf.Max(slip, Mathf.Abs(p.SlipAngle));
            float cy = Yaw(cam.transform); camMax = Mathf.Max(camMax, Mathf.Abs(Mathf.DeltaAngle(cLast, cy)) / Time.deltaTime); cLast = cy;
        });
        float turned = Mathf.DeltaAngle(y0, Yaw(p.transform));
        p.scriptedInput = Vector2.zero;
        return $"{(classic ? "GỐC 1.89.0" : "HIỆN GIỜ")} {kmh} km/h giữ Phải 0,5 s: quay {turned:F1}°, tối đa {maxRate:F0}°/s, 5° sau {t5:F2} s, nghiêng {roll:F1}°, trượt {slip:F1}°, camera tối đa {camMax:F0}°/s (headingLag {cam.headingLag}), tốc độ cuối {p.Speed * 3.6f:F0}";
    }
    public static string Restore() { P.classicSteer = true; P.scriptedInput = Vector2.zero; return "classicSteer = " + P.classicSteer; }
}
