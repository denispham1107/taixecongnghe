using System.IO;
using UnityEngine;
// 21.292: chụp kiểm tra hộp xa nhà phố — cùng góc nhìn: (a) LOD tự nhiên với lodBias như mức Yếu trên điện thoại, (b) ép mọi
// nhà phố hiện LOD1 (hộp), (c) LOD0. Ảnh ra Captures/farbox21292-<tag>-<a|b|c>.png.
public static class CaptureFarBox21292
{
    public static string Main(string tag, float x, float y, float z, float yaw, float pitch)
    {
        var np = GameObject.Find("BanDoMo").transform.Find("NhaPho");
        var go = new GameObject("CapCam"); var cam = go.AddComponent<Camera>();
        cam.transform.position = new Vector3(x, y, z); cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        cam.fieldOfView = 55; cam.farClipPlane = 900; cam.nearClipPlane = 0.3f;
        var rt = new RenderTexture(1600, 720, 24); cam.targetTexture = rt;
        float lb = QualitySettings.lodBias; string res = "";
        var lgs = np.GetComponentsInChildren<LODGroup>();
        try
        {
            QualitySettings.lodBias = 0.45f; Shot(cam, rt, tag + "-a");
            QualitySettings.lodBias = lb;
            foreach (var g in lgs) if (g.transform.parent == np) g.ForceLOD(1);
            Shot(cam, rt, tag + "-b");
            foreach (var g in lgs) if (g.transform.parent == np) g.ForceLOD(0);
            Shot(cam, rt, tag + "-c");
        }
        finally
        {
            foreach (var g in lgs) g.ForceLOD(-1);
            QualitySettings.lodBias = lb; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
        }
        return "ok " + tag;
    }
    static void Shot(Camera cam, RenderTexture rt, string name)
    {
        cam.Render(); RenderTexture.active = rt;
        var t = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); t.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes("Captures/farbox21292-" + name + ".png", t.EncodeToPNG()); Object.DestroyImmediate(t);
    }
}
