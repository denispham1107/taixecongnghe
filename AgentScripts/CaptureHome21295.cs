using System.IO;
using UnityEngine;
// 21.295: ảnh duy nhất cho trang chủ — chụp camera bám xe (Camera.main) trong Play Mode, cấu hình đồ họa Rất cao, không HUD
// (OnGUI không vẽ vào camera), 2778×1284 (tỉ lệ màn iPhone ngang như 3 ảnh cũ), MSAA 8×. Tham số: dy/back/pitch nâng-lùi camera
// so với camera bám xe (0 = giữ nguyên). Ra Captures/home21295.png.
public static class CaptureHome21295
{
    public static string Main(float dy = 0f, float back = 0f, float pitch = 0f, int w = 2778, int h = 1284)
    {
        var main = Camera.main; if (!main) return "no main camera";
        var go = new GameObject("HomeCam"); var cam = go.AddComponent<Camera>(); cam.CopyFrom(main);
        go.transform.position = main.transform.position + Vector3.up * dy - main.transform.forward * back;
        go.transform.rotation = Quaternion.Euler(main.transform.eulerAngles.x + pitch, main.transform.eulerAngles.y, 0);
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { antiAliasing = 8 };
        cam.targetTexture = rt; cam.aspect = (float)w / h;
        cam.Render(); RenderTexture.active = rt;
        var t = new Texture2D(w, h, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, w, h), 0, 0); t.Apply(); RenderTexture.active = null;
        File.WriteAllBytes("Captures/home21295.png", t.EncodeToPNG());
        Object.DestroyImmediate(t); cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
        return $"ok fov {main.fieldOfView:F1} pos {main.transform.position} rot {main.transform.eulerAngles}";
    }
}
