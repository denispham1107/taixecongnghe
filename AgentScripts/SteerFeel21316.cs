using System.IO;
using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.316 — đo cảm giác đổi hướng: xe chạy thật (testTouchUI + testPedal), giữ nút Phải 0,25 s → Trái 0,28 s → thả (đổi làn);
// ghi tốc độ quay thân xe / camera mỗi bước và chụp khung hình camera (không HUD) để ghép ảnh động so sánh trước/sau.
public static class SteerFeel21316
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();

    public static string Record(string tag, int frames)
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        var p = P; p.testTouchUI = true; p.testPedal = 1; p.testSteer = 0;
        EditorApplication.isPaused = true;
        float t0 = Time.time; int n = 0;
        while (Time.time - t0 < 2.6f && n < 4000) { EditorApplication.Step(); n++; }
        var cam = Camera.main;
        string dir = "Captures/steer21316/" + tag; Directory.CreateDirectory(dir);
        foreach (var f in Directory.GetFiles(dir)) File.Delete(f);
        var rt = new RenderTexture(960, 444, 24); var tex = new Texture2D(960, 444, TextureFormat.RGB24, false);
        float lastCarYaw = p.transform.eulerAngles.y, lastCamYaw = cam.transform.eulerAngles.y, start = Time.time;
        var sb = new System.Text.StringBuilder();
        float maxCarJerk = 0, lastCarRate = 0, maxCamRate = 0, maxCarRate = 0;
        for (int i = 0; i < frames; i++)
        {
            float el = Time.time - start;
            p.testSteer = el < 0.15f ? 0 : el < 0.4f ? 1 : el < 0.68f ? -1 : 0;
            EditorApplication.Step();
            float dt = Mathf.Max(1e-4f, Time.deltaTime);
            float cy = p.transform.eulerAngles.y, my = cam.transform.eulerAngles.y;
            float carRate = Mathf.DeltaAngle(lastCarYaw, cy) / dt, camRate = Mathf.DeltaAngle(lastCamYaw, my) / dt;
            maxCarJerk = Mathf.Max(maxCarJerk, Mathf.Abs(carRate - lastCarRate)); lastCarRate = carRate;
            maxCamRate = Mathf.Max(maxCamRate, Mathf.Abs(camRate)); maxCarRate = Mathf.Max(maxCarRate, Mathf.Abs(carRate));
            lastCarYaw = cy; lastCamYaw = my;
            if (i % 3 == 0) sb.AppendLine($"t={el:F2} steer={p.testSteer} v={p.Speed * 3.6f:F0} carRate={carRate:F0}°/s camRate={camRate:F0}°/s slip={p.SlipAngle:F1} roll={p.BodyRoll:F1}");
            var old = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = old;
            RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 960, 444), 0, 0); tex.Apply(); RenderTexture.active = null;
            File.WriteAllBytes($"{dir}/f{i:000}.png", tex.EncodeToPNG());
        }
        p.testSteer = 0; p.testPedal = 0; p.testTouchUI = false;
        Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
        return $"maxCarRate={maxCarRate:F0} maxCarJerk/frame={maxCarJerk:F0} maxCamRate={maxCamRate:F0}\n" + sb;
    }
}
