using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.316e — đo mô hình lái kiểu ô tô thật theo từng tình huống (bước khung 0,02 s). Tạm tắt va chạm + trọng lực của xe người chơi
// (khóa độ cao) để xe vòng tự do qua nhà, chỉ đo hành vi lái: tốc độ quay, gia tốc ngang (g), bán kính vòng, mất tốc độ, góc trượt.
public static class Corner21316e
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();

    // pedal: 1 ga, 0 thả, -1 phanh/lùi, 2 Stop (thắng tay trên cảm ứng); steer: 1 phải; untilDeg: dừng khi quay đủ góc (0 = hết giờ)
    public static string Run(string name, float kmh, int pedal, int steer, float seconds, float untilDeg)
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        var h = Object.FindAnyObjectByType<OpenWorldHazards>(); if (h) h.enabled = false;
        var p = P; var rb = p.GetComponent<Rigidbody>();
        foreach (var c in p.GetComponentsInChildren<Collider>()) c.enabled = false;
        rb.useGravity = false; rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        EditorApplication.isPaused = true;
        for (int i = 0; i < 3; i++) EditorApplication.Step();
        var f = typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        f.SetValue(p, kmh / 3.6f); rb.linearVelocity = p.transform.forward * (kmh / 3.6f);
        p.testTouchUI = true; p.testPedal = pedal; p.testSteer = steer;
        float t0 = Time.time, yaw0 = p.transform.eulerAngles.y, turned = 0, lastYaw = yaw0, path = 0, maxG = 0, maxRate = 0, maxSlip = 0;
        var pos0 = p.transform.position; var lastPos = pos0; var sb = new System.Text.StringBuilder(); float next = 0;
        float v0 = p.Speed;
        while (Time.time - t0 < seconds)
        {
            EditorApplication.Step();
            float y = p.transform.eulerAngles.y; turned += Mathf.DeltaAngle(lastYaw, y); lastYaw = y;
            var pp = p.transform.position; path += Vector3.Distance(new Vector3(pp.x, 0, pp.z), new Vector3(lastPos.x, 0, lastPos.z)); lastPos = pp;
            float latG = Mathf.Abs(p.YawRate * Mathf.Deg2Rad * p.Speed) / 9.81f;
            maxG = Mathf.Max(maxG, latG); maxRate = Mathf.Max(maxRate, Mathf.Abs(p.YawRate)); maxSlip = Mathf.Max(maxSlip, Mathf.Abs(p.SlipAngle));
            float el = Time.time - t0;
            if (el >= next) { next += 0.25f; sb.AppendLine($"  t={el:F2} v={p.Speed * 3.6f:F0}km/h quay={p.YawRate:F0}°/s ngang={latG:F2}g trượt={p.SlipAngle:F1}° nghiêng={p.BodyRoll:F1}° thiếuLái={p.Understeering} drift={p.Drifting} đãQuay={turned:F0}°"); }
            if (untilDeg > 0 && Mathf.Abs(turned) >= untilDeg) break;
        }
        float el2 = Time.time - t0;
        float radius = Mathf.Abs(turned) > 5 ? path / (Mathf.Abs(turned) * Mathf.Deg2Rad) : -1;
        var res = $"[{name}] {kmh} km/h pedal={pedal}: {el2:F2} s quay {turned:F0}°, quãng {path:F1} m, bán kính ≈ {radius:F1} m, tốc độ {v0 * 3.6f:F0} → {p.Speed * 3.6f:F0} km/h, tối đa {maxRate:F0}°/s, {maxG:F2} g, trượt {maxSlip:F1}°\n" + sb;
        p.testSteer = 0; p.testPedal = 0; p.testTouchUI = false;
        foreach (var c in p.GetComponentsInChildren<Collider>(true)) c.enabled = true;
        rb.useGravity = true; rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (h) h.enabled = true;
        return res;
    }
}
