using System.IO;
using UnityEngine;
// 21.302 — ảnh nhìn thẳng từ trên xuống (trực giao) để đối chiếu ranh giới vùng chơi: Main(tag, cx, cz, nửa cạnh m, px).
// Ảnh vuông px×px, tâm (cx, cz), phủ [cx−h, cx+h] × [cz−h, cz+h] (trục x sang phải, z lên trên).
public static class TopDown21302
{
    public static string Main(string tag, float cx, float cz, float half, int px)
    {
        var go = new GameObject("CamTopDown"); var cam = go.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = half; cam.aspect = 1f;
        cam.nearClipPlane = 1f; cam.farClipPlane = 6000f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.1f, .1f, .12f);
        go.transform.position = new Vector3(cx, 3000f, cz); go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var fogWas = RenderSettings.fog; RenderSettings.fog = false;
        var rt = new RenderTexture(px, px, 24) { antiAliasing = 2 };
        cam.targetTexture = rt; cam.Render();
        RenderSettings.fog = fogWas;
        RenderTexture.active = rt; var t = new Texture2D(px, px, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, px, px), 0, 0); t.Apply();
        RenderTexture.active = null; cam.targetTexture = null;
        Directory.CreateDirectory("Captures"); string path = "Captures/topdown21302-" + tag + ".png"; File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
        return path;
    }
}
