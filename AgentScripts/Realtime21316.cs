using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.316b — đo lái + camera đúng NHỊP KHI CHƠI THẬT: mỗi khung hình 1/fps giây (Time.captureDeltaTime, mặc định 60 fps) trong khi
// vật lý vẫn 50 Hz → có khung 0 hoặc 2 bước vật lý như máy thật (lần đo 21.316 bước 0,02 s = đúng 1 bước vật lý/khung nên che mất
// lỗi lệch nhịp). Ghi mỗi khung: hướng thân xe, hướng camera, vị trí xe trên màn hình; lái theo kịch bản Phải/Trái (testSteer).
public static class Realtime21316
{
    struct S { public float t, dt, carYaw, camYaw, sx, steer, speed, dist; }

    public static string Run(float seconds, float fps, string tag)
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        var P = Object.FindAnyObjectByType<OpenWorldDriver>(); P.testTouchUI = true; P.testPedal = 1; P.testSteer = 0;
        EditorApplication.isPaused = true;
        Time.captureDeltaTime = 1f / fps;
        var cam = Camera.main; var rec = new List<S>(); float t0 = Time.time; int guard = 0;
        while (Time.time - t0 < seconds && guard++ < 20000)
        {
            float e = Time.time - t0 - 3f;
            int ph = e < 0 ? -1 : Mathf.FloorToInt(e / 0.35f) % 4;
            P.testSteer = ph == 0 ? 1 : ph == 2 ? -1 : 0;
            EditorApplication.Step();
            var sp = cam.WorldToScreenPoint(P.transform.position + Vector3.up * 0.6f);
            rec.Add(new S { t = Time.time - t0, dt = Time.deltaTime, carYaw = P.transform.eulerAngles.y, camYaw = cam.transform.eulerAngles.y,
                            sx = sp.x / cam.pixelWidth, steer = P.SteerValue, speed = P.Speed, dist = Vector3.Distance(cam.transform.position, P.transform.position) });
        }
        Time.captureDeltaTime = 0f;
        P.testSteer = 0; P.testPedal = 0; P.testTouchUI = false;
        var sb = new System.Text.StringBuilder();
        int n = rec.Count; float camJ = 0, carJ = 0, sxJ = 0, dJ = 0; int bigCam = 0, bigCar = 0, m = 0;
        for (int i = 2; i < n; i++)
        {
            var a = rec[i - 2]; var b = rec[i - 1]; var c = rec[i];
            if (c.t < 3f) continue; m++;
            float cam1 = Mathf.DeltaAngle(a.camYaw, b.camYaw) / b.dt, cam2 = Mathf.DeltaAngle(b.camYaw, c.camYaw) / c.dt;
            float car1 = Mathf.DeltaAngle(a.carYaw, b.carYaw) / b.dt, car2 = Mathf.DeltaAngle(b.carYaw, c.carYaw) / c.dt;
            camJ += Mathf.Abs(cam2 - cam1); carJ += Mathf.Abs(car2 - car1);
            sxJ += Mathf.Abs((c.sx - b.sx) / c.dt - (b.sx - a.sx) / b.dt);
            dJ += Mathf.Abs((c.dist - b.dist) / c.dt - (b.dist - a.dist) / b.dt);
            if (Mathf.Abs(cam2 - cam1) > 15f) bigCam++; if (Mathf.Abs(car2 - car1) > 15f) bigCar++;
        }
        sb.AppendLine($"[{tag}] fps={fps} frames={n} (lái từ giây 3) | TB |Δ tốc độ quay camera|/khung={camJ / m:F1}°/s, thân xe={carJ / m:F1}°/s, " +
                      $"giật xe trên màn hình (|Δ vận tốc ngang|)={sxJ / m:F3}/s, giật khoảng cách camera={dJ / m:F2} m/s; khung giật>15: cam {bigCam}, xe {bigCar}");
        for (int i = 1; i < n; i++)
        {
            var b = rec[i - 1]; var c = rec[i];
            if (c.t < 2.95f || c.t > 4.5f) continue;
            sb.AppendLine($"t={c.t:F3} steer={c.steer:F2} v={c.speed * 3.6f:F0} car={Mathf.DeltaAngle(b.carYaw, c.carYaw) / c.dt:F0} cam={Mathf.DeltaAngle(b.camYaw, c.camYaw) / c.dt:F0} sx={c.sx:F3} d={c.dist:F2}");
        }
        System.IO.Directory.CreateDirectory("Captures/steer21316");
        System.IO.File.WriteAllText("Captures/steer21316/realtime-" + tag + ".txt", sb.ToString());
        return sb.ToString().Split('\n')[0];
    }
}
