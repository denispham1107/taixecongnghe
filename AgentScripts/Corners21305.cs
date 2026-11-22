using System.IO;
using UnityEngine;
// 21.305 — ảnh trên xuống các góc bán đảo + mép cửa sông để soát lan can / lề biển: Main(nửa cạnh m) → Captures/goc21305-<tên>.png
public static class Corners21305
{
    static string Shot(string tag, float cx, float cz, float half, int px)
    {
        var go = new GameObject("CamGoc"); var cam = go.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = half; cam.aspect = 1f; cam.nearClipPlane = 1f; cam.farClipPlane = 600f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.1f, .1f, .12f);
        go.transform.position = new Vector3(cx, 300f, cz); go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var fog = RenderSettings.fog; RenderSettings.fog = false;
        var rt = new RenderTexture(px, px, 24) { antiAliasing = 4 }; cam.targetTexture = rt; cam.Render(); RenderSettings.fog = fog;
        RenderTexture.active = rt; var t = new Texture2D(px, px, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, px, px), 0, 0); t.Apply();
        RenderTexture.active = null; cam.targetTexture = null;
        string path = "Captures/goc21305-" + tag + ".png"; File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go); return path;
    }
    public static string Main(float half)
    {
        var pts = new (string n, float x, float z)[] { ("SW", -567.5f, -567.5f), ("SE", 700f, -567.5f), ("NW", -567.5f, 700f), ("NE", 700f, 700f),
            ("MouthS_W", 234f, -567.5f), ("MouthS_E", 400f, -567.5f), ("MouthW_S", -567.5f, 229f), ("MouthW_N", -567.5f, 400f) };
        string r = "";
        foreach (var p in pts) r += Shot(p.n, p.x, p.z, half, 900) + " ";
        return r;
    }
    public static string One(string tag, float x, float z, float half) => Shot(tag, x, z, half, 900);
    public static string Mouths()
    {
        string r = "";
        foreach (var p in new (string n, float x, float z)[] { ("mS_W", 234f, -567.5f), ("mS_E", 400f, -567.5f), ("mW_S", -567.5f, 229f), ("mW_N", -567.5f, 400f) })
            r += Shot(p.n, p.x, p.z, 6f, 900) + " ";
        return r;
    }
}
