using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.319 — đo độ nhạy lái (người dùng: đổi hướng trái/phải nặng nề, như lôi cả chiếc xe, chậm). Bước khung 0,02 s, tắt va chạm +
// trọng lực xe (như Corner21316e). Kịch bản: chạy thẳng ở kmh, giữ Phải `hold` giây rồi giữ Trái `hold` giây rồi thả.
// Báo: thời gian từ lúc bấm tới khi thân xe quay 5°/15°, tốc độ quay sau 0,1/0,2/0,3 s, góc đầu xe sau 0,5 s, độ lệch hướng chạy
// so với đầu xe (trượt), thời gian đổi chiều quay khi chuyển Phải → Trái, độ lệch ngang sau 1 s, tốc độ còn lại.
public static class Resp21319
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();

    public static string Run(float kmh, float hold)
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        var h = Object.FindAnyObjectByType<OpenWorldHazards>(); if (h) h.enabled = false;
        var w = Object.FindAnyObjectByType<OpenWorldWeather>(); if (w) w.enabled = false;
        var p = P; var rb = p.GetComponent<Rigidbody>();
        foreach (var c in p.GetComponentsInChildren<Collider>()) c.enabled = false;
        rb.useGravity = false; rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        EditorApplication.isPaused = true;
        for (int i = 0; i < 3; i++) EditorApplication.Step();
        var f = typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        f.SetValue(p, kmh / 3.6f); rb.linearVelocity = p.transform.forward * (kmh / 3.6f);
        for (int i = 0; i < 10; i++) { f.SetValue(p, kmh / 3.6f); EditorApplication.Step(); }
        p.testTouchUI = true; p.testPedal = 1; p.testSteer = 1;
        var cam = Camera.main;
        float t0 = Time.time, yaw0 = p.transform.eulerAngles.y, camYaw0 = cam ? cam.transform.eulerAngles.y : 0;
        var pos0 = p.transform.position; var dir0 = p.transform.forward; dir0.y = 0; dir0.Normalize(); var right0 = new Vector3(dir0.z, 0, -dir0.x);
        float t5 = -1, t15 = -1, r01 = 0, r02 = 0, r03 = 0, head05 = 0, slipMax = 0, maxRate = 0, flipAt = -1, lat10 = 0, cam05 = 0, camMax = 0, lastCam = camYaw0;
        bool left = false; float leftAt = -1, lastYaw = yaw0, turned = 0, rateAtSwitch = 0, peakLeft = 0;
        var sb = new System.Text.StringBuilder();
        while (Time.time - t0 < hold * 2 + 0.6f)
        {
            float el0 = Time.time - t0;
            if (!left && el0 >= hold) { left = true; leftAt = el0; p.testSteer = -1; rateAtSwitch = p.YawRate; }
            if (el0 >= hold * 2) p.testSteer = 0;
            EditorApplication.Step();
            float el = Time.time - t0;
            float y = p.transform.eulerAngles.y; turned += Mathf.DeltaAngle(lastYaw, y); lastYaw = y;
            if (cam) { float cy = cam.transform.eulerAngles.y; camMax = Mathf.Max(camMax, Mathf.Abs(Mathf.DeltaAngle(lastCam, cy)) / Time.deltaTime); lastCam = cy; }
            if (t5 < 0 && turned >= 5) t5 = el; if (t15 < 0 && turned >= 15) t15 = el;
            if (Mathf.Abs(el - 0.1f) < 0.011f) r01 = p.YawRate; if (Mathf.Abs(el - 0.2f) < 0.011f) r02 = p.YawRate; if (Mathf.Abs(el - 0.3f) < 0.011f) r03 = p.YawRate;
            if (Mathf.Abs(el - 0.5f) < 0.011f) { head05 = turned; if (cam) cam05 = Mathf.DeltaAngle(camYaw0, cam.transform.eulerAngles.y); }
            if (Mathf.Abs(el - 1.0f) < 0.011f) lat10 = Vector3.Dot(p.transform.position - pos0, right0);
            slipMax = Mathf.Max(slipMax, Mathf.Abs(p.SlipAngle)); maxRate = Mathf.Max(maxRate, Mathf.Abs(p.YawRate));
            if (left && flipAt < 0 && p.YawRate < 0) flipAt = el - leftAt;
            if (left) peakLeft = Mathf.Min(peakLeft, p.YawRate);
            if (((int)(el * 50)) % 5 == 0) sb.Append($" {el:F1}:{p.YawRate:F0}");
        }
        var res = $"[{kmh} km/h, giữ {hold}s mỗi bên] quay 5° sau {t5:F2}s, 15° sau {t15:F2}s | tốc độ quay @0,1/0,2/0,3s = {r01:F0}/{r02:F0}/{r03:F0}°/s, tối đa {maxRate:F0}°/s | đầu xe @0,5s {head05:F0}°, camera @0,5s {cam05:F0}° (quay nhanh nhất {camMax:F0}°/s) | lệch ngang @1s {lat10:F1} m | trượt tối đa {slipMax:F1}° | Phải→Trái: đổi chiều sau {flipAt:F2}s, đỉnh trái {peakLeft:F0}°/s | tốc độ còn {p.Speed * 3.6f:F0} km/h\n  quay(°/s):{sb}";
        p.testSteer = 0; p.testPedal = 0; p.testTouchUI = false;
        foreach (var c in p.GetComponentsInChildren<Collider>(true)) c.enabled = true;
        rb.useGravity = true; rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (h) h.enabled = true; if (w) w.enabled = true;
        return res;
    }
}
