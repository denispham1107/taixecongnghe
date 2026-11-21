using System.IO;
using UnityEngine;
// 21.293: chụp một góc nhìn bất kỳ trong scene đang mở (kiểm tra tháp Tài Chính). Ảnh ra Captures/thap21293-<tag>.png.
public static class CaptureView21293
{
    public static string Main(string tag, float x, float y, float z, float yaw, float pitch, float fov = 55f, int w = 1280, int h = 720)
    {
        var go = new GameObject("CapCam"); var cam = go.AddComponent<Camera>();
        cam.transform.position = new Vector3(x, y, z); cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        if (fov < 0) { cam.orthographic = true; cam.orthographicSize = -fov; } else cam.fieldOfView = fov; cam.farClipPlane = 2500; cam.nearClipPlane = 0.3f;
        var main = Camera.main; if (main) { cam.clearFlags = main.clearFlags; cam.backgroundColor = main.backgroundColor; }
        bool fog = RenderSettings.fog; if (fov < 0) RenderSettings.fog = false;
        var rt = new RenderTexture(w, h, 24); cam.targetTexture = rt;
        try
        {
            cam.Render(); RenderTexture.active = rt;
            var t = new Texture2D(w, h, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, w, h), 0, 0); t.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes("Captures/thap21293-" + tag + ".png", t.EncodeToPNG()); Object.DestroyImmediate(t);
        }
        finally { RenderSettings.fog = fog; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(go); }
        return "ok " + tag;
    }
}
