using PhoViet.OpenWorld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.OpenWorldEditor
{
    // 21.274 — menu 11: xe không lún khi lên vỉa hè/lối ven sông + sửa hộp va chạm CSGT.
    // • Tạo «MatLe»: MeshCollider sao từ mesh OW_Duong_*_ViaHe và OW_Map_LoiVenSong, layer 28 «MatLe» (vật lý bỏ qua mọi
    //   va chạm với layer này; WorldMask cũng bỏ qua) — chỉ để OpenWorldGroundFit dò độ cao mặt lề.
    // • Gắn OpenWorldGroundFit vào scene.
    // • Hộp va chạm CSGT: đáy = đáy bánh (trước cao hơn 0,12 m).
    public static class PhoVietOpenWorldGroundFitBuilder
    {
        [MenuItem("Phố Việt/Bản đồ mở/11. Mặt lề cho xe + sửa xe CSGT lún (scene đang mở)")]
        public static void BuildMenu() => Debug.Log("[Bản đồ mở] " + Build());

        public static string Build()
        {
            var map = GameObject.Find("BanDoMo"); if (!map) return "mở scene PhoVietOpenWorld trước";
            // tên layer 28
            var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tm.FindProperty("layers"); var l28 = layers.GetArrayElementAtIndex(OpenWorldGroundFit.SurfaceLayer);
            if (string.IsNullOrEmpty(l28.stringValue)) { l28.stringValue = "MatLe"; tm.ApplyModifiedPropertiesWithoutUndo(); }
            for (int i = 0; i < 32; i++) Physics.IgnoreLayerCollision(OpenWorldGroundFit.SurfaceLayer, i, true);

            var old = GameObject.Find("MatLe"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("MatLe"); root.layer = OpenWorldGroundFit.SurfaceLayer;
            int n = 0;
            foreach (var r in map.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!(r.name.StartsWith("OW_Duong_") && r.name.EndsWith("_ViaHe")) && r.name != "OW_Map_LoiVenSong") continue;
                var mf = r.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh) continue;
                var g = new GameObject(r.name + "_MatLe"); g.layer = OpenWorldGroundFit.SurfaceLayer;
                g.transform.SetParent(root.transform, false);
                g.transform.SetPositionAndRotation(r.transform.position, r.transform.rotation);
                g.transform.localScale = r.transform.lossyScale;
                var mc = g.AddComponent<MeshCollider>(); mc.sharedMesh = mf.sharedMesh;
                g.isStatic = true; n++;
            }
            var fit = Object.FindAnyObjectByType<OpenWorldGroundFit>() ?? root.AddComponent<OpenWorldGroundFit>();

            // hộp va chạm CSGT: đáy bằng đáy bánh
            string pol = "không thấy CSGT";
            var police = Object.FindAnyObjectByType<OpenWorldPolice>(FindObjectsInactive.Include);
            if (police)
            {
                var bc = police.GetComponent<BoxCollider>();
                if (bc) { float bottom = bc.center.y - bc.size.y / 2; if (bottom > 0.001f) { bc.size = new Vector3(bc.size.x, bc.size.y + bottom, bc.size.z); bc.center = new Vector3(bc.center.x, bc.size.y / 2, bc.center.z); } pol = "CSGT đáy hộp " + bottom.ToString("F2") + " → 0"; }
            }
            EditorSceneManager.MarkSceneDirty(map.scene); EditorSceneManager.SaveScene(map.scene);
            AssetDatabase.SaveAssets();
            return "mặt lề " + n + " mesh; " + pol;
        }
    }
}
