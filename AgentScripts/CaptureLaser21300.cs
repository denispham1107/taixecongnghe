using System.IO;
using UnityEngine;
// 21.300b — chụp cận cảnh tia lazer bằng camera tạm: Main(tag, dọc tia (m) chỗ nhìn, lệch ngang (m), cao (m)).
// Ví dụ: ("ngon", 14, 4.5, 1.6) nhìn ngọn tia khi bắn trượt; ("trung", d, 4, 1.5) nhìn chỗ trúng.
public static class CaptureLaser21300
{
    public static string Main(string tag, float along, float side, float up)
    {
        var d = Object.FindAnyObjectByType<PhoViet.OpenWorld.OpenWorldDriver>();
        var f = d.transform.forward; f.y = 0; f.Normalize(); var r = Vector3.Cross(Vector3.up, f);
        var look = d.transform.position + f * along + Vector3.up * 0.55f;
        var go = new GameObject("CamLazer"); var cam = go.AddComponent<Camera>();
        cam.fieldOfView = 50; cam.nearClipPlane = .1f; cam.farClipPlane = 1500;
        go.transform.position = look + r * side + Vector3.up * up - f * 1.5f; go.transform.LookAt(look);
        var rt = new RenderTexture(1600, 1000, 24) { antiAliasing = 4 };
        cam.targetTexture = rt; cam.Render();
        RenderTexture.active = rt; var t = new Texture2D(1600, 1000, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); t.Apply();
        RenderTexture.active = null; cam.targetTexture = null;
        string path = "Captures/laser21300b-" + tag + ".png"; File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
        return path;
    }
}
