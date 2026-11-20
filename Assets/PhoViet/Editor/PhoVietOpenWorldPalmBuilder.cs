using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorldEditor
{
    // 21.286 — menu 14: thay TOÀN BỘ cây bản đồ mở (BanDoMo/Cay, trước là PV_StreetTree0..2 có chậu nằm trong lưới) bằng cây dừa
    // Meshy (người dùng gửi) đã cắt bệ đất + gắn bệ gạch bát giác cũ P2 (ArtSource/Blender/OpenWorld/build_meshy_palm.py).
    // • FBX Assets/PhoViet/Art/Streets/CayDua/PV_CayDua.fbx: CayDua_LOD0..2 (UV + texture Meshy) và BeCay_LOD0..2 (màu đỉnh).
    // • Prefab PV_CayDua.prefab: vật liệu shader PhoViet/OpenWorld/Palm (lá hai mặt / bệ màu đỉnh), LODGroup 0,5 / 0,22 / 0,09
    //   như cây cũ (gần ~35 m LOD0, ~80 m LOD1, ~200 m LOD2, xa hơn ẩn trong sương).
    // • Mỗi cây cũ → một cây dừa cùng vị trí, cùng hướng; tỉ lệ k = tỉ lệ cũ × 1,46 / 2,6 (bệ mới rộng bằng hộp va chạm chậu cũ).
    //   Cờ tĩnh chỉ OccludeeStatic (tán lá thưa không được che khuất vật khác khi nướng occlusion).
    // • Va chạm VaCham_Cay dựng lại cùng vị trí: hộp bệ bát giác + con nhộng thân cây.
    // Sau khi chạy: menu 12 nướng lại occlusion.
    public static class PhoVietOpenWorldPalmBuilder
    {
        const string Dir = "Assets/PhoViet/Art/Streets/CayDua/";
        const string Fbx = Dir + "PV_CayDua.fbx";
        const string PrefabPath = Dir + "PV_CayDua.prefab";
        const float OldPlanter = 1.46f, NewPlanter = 2.6f, BedTop = 0.49f;

        [MenuItem("Phố Việt/Bản đồ mở/14. Cây dừa Meshy thay toàn bộ cây (21.286, scene đang mở)")]
        public static void BuildMenu() => Debug.Log("[Bản đồ mở] " + Apply(GameObject.Find("BanDoMo")));

        static Material Mat(string name, bool vertexColor, Texture2D tex)
        {
            string path = Dir + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("PhoViet/OpenWorld/Palm")); AssetDatabase.CreateAsset(m, path); }
            m.shader = Shader.Find("PhoViet/OpenWorld/Palm");
            m.SetTexture("_MainTex", vertexColor ? null : tex);
            m.SetFloat("_UseVertexColor", vertexColor ? 1 : 0);
            m.SetFloat("_Cull", (float)CullMode.Off);      // lá Meshy một mặt; vài mặt nắp bệ quấn ngược → vẽ hai mặt
            m.SetFloat("_Smoothness", vertexColor ? 0.04f : 0.1f);
            m.enableInstancing = true;
            EditorUtility.SetDirty(m); return m;
        }

        public static GameObject BuildPrefab()
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(Fbx);
            imp.materialImportMode = ModelImporterMaterialImportMode.None;
            imp.importAnimation = false; imp.importCameras = false; imp.importLights = false;
            imp.isReadable = false; imp.importNormals = ModelImporterNormals.Import; imp.importBlendShapes = false;
            imp.SaveAndReimport();
            var ti = (TextureImporter)AssetImporter.GetAtPath(Dir + "PV_CayDua_Albedo.png");
            ti.maxTextureSize = 2048; ti.mipmapEnabled = true; ti.sRGBTexture = true; ti.alphaSource = TextureImporterAlphaSource.None;
            ti.SaveAndReimport();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "PV_CayDua_Albedo.png");
            var treeMat = Mat("PV_CayDua", false, tex); var bedMat = Mat("PV_CayDua_Be", true, null);
            AssetDatabase.SaveAssets();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = "PV_CayDua";
            var lodR = new List<Renderer>[3]; for (int k = 0; k < 3; k++) lodR[k] = new List<Renderer>();
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                bool bed = r.name.StartsWith("BeCay");
                r.sharedMaterial = bed ? bedMat : treeMat;
                int k = r.name.EndsWith("LOD0") ? 0 : r.name.EndsWith("LOD1") ? 1 : 2;
                lodR[k].Add(r);
                r.shadowCastingMode = k == 2 && bed ? ShadowCastingMode.Off : ShadowCastingMode.On;
                r.receiveShadows = true; r.lightProbeUsage = LightProbeUsage.Off; r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
            var lg = go.GetComponent<LODGroup>() ?? go.AddComponent<LODGroup>();
            float[] th = { 0.5f, 0.22f, 0.09f };
            var lods = new LOD[3]; for (int k = 0; k < 3; k++) lods[k] = new LOD(th[k], lodR[k].ToArray());
            lg.SetLODs(lods); lg.RecalculateBounds();
            var pf = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return pf;
        }

        public static string Apply(GameObject map)
        {
            if (!map) return "không thấy BanDoMo";
            var pf = BuildPrefab();
            var cay = map.transform.Find("Cay"); var vc = map.transform.Find("VaCham_Cay");
            // kích thước bệ / thân trong không gian prefab (tỉ lệ 1)
            var tmp = (GameObject)PrefabUtility.InstantiatePrefab(pf); tmp.transform.position = Vector3.zero;
            Bounds bed = default, trunk = default;
            foreach (var r in tmp.GetComponentsInChildren<Renderer>())
            {
                if (r.name == "BeCay_LOD0") bed = r.bounds;
                if (r.name == "CayDua_LOD0") trunk = r.bounds;
            }
            Object.DestroyImmediate(tmp);
            var olds = new List<Transform>(); foreach (Transform t in cay) olds.Add(t);
            var oldCols = new List<Transform>(); foreach (Transform t in vc) oldCols.Add(t);
            int n = 0;
            foreach (var t in olds)
            {
                Vector3 p = t.position; float yaw = t.eulerAngles.y; float k = t.localScale.x * OldPlanter / NewPlanter;
                if (t.name.StartsWith("PV_CayDua")) k = t.localScale.x;             // chạy lại: giữ tỉ lệ đã đổi
                var go = (GameObject)PrefabUtility.InstantiatePrefab(pf, map.scene);
                go.transform.SetParent(cay, true);
                go.transform.SetPositionAndRotation(p, Quaternion.Euler(0, yaw, 0) * pf.transform.localRotation);
                go.transform.localScale = pf.transform.localScale * k;
                foreach (var c in go.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.SetStaticEditorFlags(c.gameObject, StaticEditorFlags.OccludeeStatic);
                var col = new GameObject("VaCham"); col.transform.SetParent(vc, false);
                col.transform.SetPositionAndRotation(p, Quaternion.Euler(0, yaw, 0)); col.transform.localScale = Vector3.one * k; col.isStatic = true;
                var bc = col.AddComponent<BoxCollider>();
                bc.size = new Vector3(NewPlanter, BedTop, NewPlanter); bc.center = new Vector3(0, BedTop / 2, 0);   // nắp bệ (cỏ cao hơn không tính)
                var cc = col.AddComponent<CapsuleCollider>(); cc.radius = 0.3f; cc.height = 7f; cc.center = new Vector3(0, 3.5f, 0);
                Object.DestroyImmediate(t.gameObject); n++;
            }
            foreach (var c in oldCols) if (c) Object.DestroyImmediate(c.gameObject);
            EditorSceneManager.MarkSceneDirty(map.scene);
            return "cây dừa: thay " + n + " cây; bệ " + bed.size.ToString("F2") + ", tán " + trunk.size.ToString("F2") + " (tỉ lệ 1). Nhớ chạy menu 12.";
        }
    }
}
