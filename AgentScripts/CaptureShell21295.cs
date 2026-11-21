using System.IO;
using UnityEngine;
using UnityEditor;
// 21.295: chụp cận cảnh viên đạn pháo (vật liệu PV_TankShell_Mat) → Captures/shell21295.png
public static class CaptureShell21295
{
    public static string Main()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/TankFx/PV_TankShell.fbx");
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/TankFx/PV_TankShell_Mat.mat");
        var root = new GameObject("ShellTmp"); root.transform.position = new Vector3(218, 6, -380);
        for (int k = 0; k < 2; k++)
        {
            var t = Object.Instantiate(model, root.transform).transform; t.localPosition = new Vector3(k * 1.2f - .6f, 0, 0);
            foreach (var r in t.GetComponentsInChildren<MeshRenderer>(true)) { r.transform.localRotation = Quaternion.Euler(-90, 0, 0); r.gameObject.SetActive(r.name.EndsWith("_LOD0")); r.sharedMaterial = mat; }
            var lod = t.GetComponent<LODGroup>(); if (lod) Object.DestroyImmediate(lod);
            t.localScale = Vector3.one * 1.8f; t.localRotation = Quaternion.Euler(0, k == 0 ? 60 : -120, 0);
        }
        var cg = new GameObject("Cam"); var cam = cg.AddComponent<Camera>(); cam.transform.position = root.transform.position + new Vector3(0, .8f, -3.2f);
        cam.transform.LookAt(root.transform.position); cam.fieldOfView = 40;
        var rt = new RenderTexture(960, 540, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var tex = new Texture2D(960, 540, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); tex.Apply(); RenderTexture.active = null;
        File.WriteAllBytes("Captures/shell21295.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex); cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(cg); Object.DestroyImmediate(root);
        return "ok";
    }
}
