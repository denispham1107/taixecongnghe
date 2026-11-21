using System.IO;
using UnityEngine;
// 21.298 — chụp cận cảnh cờ đỏ sao vàng trên cột đèn gần camera chính (Play Mode để vải chuyển động theo _Time).
// Main(tag): camera tạm đặt chéo trước lá cờ gần nhất, 1600×1000 → Captures/flag21298-<tag>.png. Main2: góc nhìn xa dọc phố.
public static class CaptureFlag21298
{
    static Transform Nearest(Vector3 p)
    {
        Transform best = null; float bd = 1e9f;
        foreach (var lg in Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Exclude))
        {
            if (lg.name != "OW_Mau_CotDen_Co") continue;
            float d = (lg.transform.position - p).sqrMagnitude; if (d < bd) { bd = d; best = lg.transform; }
        }
        return best;
    }

    static string Shot(Vector3 pos, Vector3 look, float fov, string tag)
    {
        var go = new GameObject("CamCo"); var cam = go.AddComponent<Camera>();
        cam.fieldOfView = fov; cam.nearClipPlane = .1f; cam.farClipPlane = 1500;
        go.transform.position = pos; go.transform.LookAt(look);
        var rt = new RenderTexture(1600, 1000, 24) { antiAliasing = 4 };
        cam.targetTexture = rt; cam.Render();
        RenderTexture.active = rt; var t = new Texture2D(1600, 1000, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); t.Apply();
        RenderTexture.active = null; cam.targetTexture = null;
        Directory.CreateDirectory("Captures"); string path = "Captures/flag21298-" + tag + ".png";
        File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
        return path;
    }

    public static string Main(string tag)
    {
        var main = Camera.main; var f = Nearest(main.transform.position + main.transform.forward * 25f);
        // khung cục bộ cột: x (transform.up thế giới do xoay FBX) ra ngoài, z cục bộ lên; tâm cờ ~ (0,62; 0; 6,3)
        var center = f.TransformPoint(new Vector3(.62f, 0, 6.28f));
        var outward = (f.TransformPoint(new Vector3(1, 0, 6.28f)) - f.TransformPoint(new Vector3(0, 0, 6.28f))).normalized;
        var face = (f.TransformPoint(new Vector3(0, 1, 6.28f)) - f.TransformPoint(new Vector3(0, 0, 6.28f))).normalized;
        var pos = center + face * 3.4f + outward * 1.2f - Vector3.up * 1.0f;
        return Shot(pos, center, 45, tag) + " t=" + Time.time.ToString("F2");
    }

    // góc xiên (nhìn gần dọc mặt vải) để thấy rõ nếp sóng
    public static string Main3(string tag)
    {
        var main = Camera.main; var f = Nearest(main.transform.position + main.transform.forward * 25f);
        var center = f.TransformPoint(new Vector3(.62f, 0, 6.28f));
        var outward = (f.TransformPoint(new Vector3(1, 0, 6.28f)) - f.TransformPoint(new Vector3(0, 0, 6.28f))).normalized;
        var face = (f.TransformPoint(new Vector3(0, 1, 6.28f)) - f.TransformPoint(new Vector3(0, 0, 6.28f))).normalized;
        var pos = center + face * 1.6f + outward * 2.6f - Vector3.up * 0.6f;
        return Shot(pos, center, 45, tag) + " t=" + Time.time.ToString("F2");
    }

    // phóng to vùng thanh treo (trực diện, ngang tầm) để soi khe giữa vải và thanh
    public static string Main4(string tag)
    {
        var main = Camera.main; var f = Nearest(main.transform.position + main.transform.forward * 25f);
        var center = f.TransformPoint(new Vector3(.62f, 0, 7.08f));
        var face = (f.TransformPoint(new Vector3(0, 1, 7.08f)) - f.TransformPoint(new Vector3(0, 0, 7.08f))).normalized;
        return Shot(center + face * 1.3f, center, 30, tag);
    }

    public static string Main2(string tag)
    {
        var main = Camera.main;
        return Shot(main.transform.position, main.transform.position + main.transform.forward * 30f, main.fieldOfView, tag);
    }
}
