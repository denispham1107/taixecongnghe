using System.IO;
using UnityEngine;
// 21.302 — ảnh camera tạm (giống camera chính: sương, far 1500): Main(tag, vị trí x,y,z, điểm nhìn x,y,z, fov) → Captures/bandao21302-<tag>.png
// Main2(tag): ảnh của chính camera chơi (Camera.main) không có HUD.
public static class CamShot21302
{
    static string Shot(Camera cam, string tag)
    {
        var rt = new RenderTexture(1800, 1000, 24) { antiAliasing = 4 };
        var old = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = old;
        RenderTexture.active = rt; var t = new Texture2D(1800, 1000, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, 1800, 1000), 0, 0); t.Apply();
        RenderTexture.active = null;
        string path = "Captures/bandao21302-" + tag + ".png"; File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t); rt.Release(); Object.DestroyImmediate(rt);
        return path;
    }
    public static string Main(string tag, float px, float py, float pz, float lx, float ly, float lz, float fov)
    {
        var go = new GameObject("CamBanDao"); var cam = go.AddComponent<Camera>();
        var main = Camera.main; if (main) { cam.CopyFrom(main); }
        cam.fieldOfView = fov; cam.targetTexture = null;
        go.transform.position = new Vector3(px, py, pz); go.transform.LookAt(new Vector3(lx, ly, lz));
        var p = Shot(cam, tag); Object.DestroyImmediate(go); return p;
    }
    public static string Main2(string tag) => Shot(Camera.main, tag);
}
