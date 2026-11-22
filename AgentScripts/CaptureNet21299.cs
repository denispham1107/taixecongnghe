using System.IO;
using UnityEngine;
// 21.299 — chụp cận cảnh xe người chơi (lưới điện CSGT) bằng camera tạm: Main(tag, góc độ quanh xe).
public static class CaptureNet21299
{
    public static string Main(string tag, float yawDeg)
    {
        var d = Object.FindAnyObjectByType<PhoViet.OpenWorld.OpenWorldDriver>();
        var c = d.transform.position + Vector3.up * 0.8f;
        var dir = Quaternion.Euler(0, d.transform.eulerAngles.y + yawDeg, 0) * Vector3.forward;
        var go = new GameObject("CamLuoi"); var cam = go.AddComponent<Camera>();
        cam.fieldOfView = 45; cam.nearClipPlane = .1f; cam.farClipPlane = 1500;
        go.transform.position = c + dir * 6.2f + Vector3.up * 1.6f; go.transform.LookAt(c);
        var rt = new RenderTexture(1600, 1000, 24) { antiAliasing = 4 };
        cam.targetTexture = rt; cam.Render();
        RenderTexture.active = rt; var t = new Texture2D(1600, 1000, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); t.Apply();
        RenderTexture.active = null; cam.targetTexture = null;
        string path = "Captures/net21299-" + tag + ".png"; File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
        return path;
    }
}
