using System.IO;
using UnityEditor;
using UnityEngine;
// 21.306 — chụp lại đúng góc camera Scene view (để đối chiếu ảnh người dùng gửi) và chiếu điểm ảnh → toạ độ thế giới.
public static class SceneCam21306
{
    public static string Shot(int w, int h)
    {
        var sv = SceneView.lastActiveSceneView; var src = sv.camera;
        var go = new GameObject("CamSV"); var cam = go.AddComponent<Camera>();
        cam.CopyFrom(src); cam.targetTexture = null; cam.fieldOfView = src.fieldOfView;
        var rt = new RenderTexture(w, h, 24) { antiAliasing = 2 }; cam.targetTexture = rt; cam.aspect = w / (float)h; cam.Render();
        RenderTexture.active = rt; var t = new Texture2D(w, h, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, w, h), 0, 0); t.Apply(); RenderTexture.active = null;
        File.WriteAllBytes("Captures/scenecam21306.png", t.EncodeToPNG());
        Object.DestroyImmediate(t); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
        return "Captures/scenecam21306.png";
    }
    // điểm ảnh (px, py từ góc trên trái) trên ảnh w×h → điểm chạm mặt đất
    public static string Pick(int w, int h, string pts)
    {
        var sv = SceneView.lastActiveSceneView; var src = sv.camera;
        var go = new GameObject("CamSV"); var cam = go.AddComponent<Camera>(); cam.CopyFrom(src); cam.aspect = w / (float)h;
        string r = "";
        foreach (var s in pts.Split(';'))
        {
            var xy = s.Split(','); float px = float.Parse(xy[0]), py = float.Parse(xy[1]);
            var ray = cam.ViewportPointToRay(new Vector3(px / w, 1f - py / h, 0));
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float d)) { var p = ray.GetPoint(d); r += $"({px},{py})→({p.x:F1},{p.z:F1}) "; }
        }
        Object.DestroyImmediate(go); return r;
    }
}
