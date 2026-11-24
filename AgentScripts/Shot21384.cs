using UnityEngine;
using UnityEditor;

// 21.384 — render a still from an arbitrary camera pose (for locating / checking the Petshop wall banner).
public static class Shot21384
{
    public static string Render(float px, float py, float pz, float pitch, float yaw, float fov, string file)
    {
        var main = Camera.main;
        var go = new GameObject("TmpShotCam");
        var cam = go.AddComponent<Camera>();
        if (main) cam.CopyFrom(main);
        cam.enabled = false;
        cam.orthographic = false;
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.3f; cam.farClipPlane = 3000f;
        cam.transform.SetPositionAndRotation(new Vector3(px, py, pz), Quaternion.Euler(pitch, yaw, 0));
        var rt = new RenderTexture(1600, 900, 24);
        cam.targetTexture = rt; cam.Render();
        RenderTexture.active = rt;
        var t = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); t.Apply();
        RenderTexture.active = null;
        System.IO.File.WriteAllBytes(file, t.EncodeToPNG());
        cam.targetTexture = null;
        Object.DestroyImmediate(rt); Object.DestroyImmediate(t); Object.DestroyImmediate(go);
        return file;
    }
}
