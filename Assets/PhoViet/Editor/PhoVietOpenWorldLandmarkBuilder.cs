using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PhoViet.OpenWorldEditor
{
    // 21.293 — Tháp Tài Chính dựng lại theo ảnh mẫu người dùng gửi 21/11/2026 (tháp búp sen 262 m, cánh xoắn, sân trực thăng
    // nhô trái, đỉnh vát + ăng-ten, khối đế kính có khối ốp nhôm và biển tên).
    // Nguồn: ArtSource/Blender/OpenWorld/ow_thap_taichinh.py (MODE='export') → ThapTaiChinh/Models/OW_ThapTaiChinh.fbx (LOD0..2 +
    // _VaCham lăng trụ lồi) + ThapTaiChinh/Textures. Menu 17: vật liệu Standard (màu + kim loại/độ bóng), prefab sạch
    // (LODGroup + MeshCollider convex), đặt ở BanDoMo/ThapTaiChinh gần chỗ tháp cũ, mặt chính quay ra sông (đông),
    // tắt tháp cũ (OW_ThapTaiChinh + _SanTrucThang trong OW_Map_showcase), xóa hộp va chạm cũ. Sau đó chạy menu 12 (occlusion).
    public static class PhoVietOpenWorldLandmarkBuilder
    {
        const string Root = "Assets/PhoViet/OpenWorld/ThapTaiChinh/";
        const string Name = "OW_ThapTaiChinh";
        // Sân trong của khối nhà chỉ rộng ~51 m (đông–tây) × 70 m (bắc–nam): mặt chính (có khối nhôm + biển tên, sân trực thăng bên
        // trái) quay ra SÔNG (hướng đông) → đứng ở đường ven sông/bờ đông nhìn đúng như ảnh mẫu, khối nhôm nằm phía bắc không lấn nhà.
        static readonly Vector3 Pos = new Vector3(156f, 0f, -58f);
        const float Yaw = 90f;
        public const float Lod0 = 0.2f, Lod1 = 0.06f, Lod2 = 0.002f;

        [MenuItem("Phố Việt/Bản đồ mở/17. Tháp Tài Chính theo ảnh mẫu (21.293, scene đang mở)")]
        public static void Menu() => Debug.Log("[Tháp Tài Chính] " + Install());

        public static string Install()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var map = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "BanDoMo");
            if (!map) return "không thấy BanDoMo";
            var mats = BuildMaterials();
            var pf = BuildPrefab(mats, out string log);
            var old = map.transform.Find("ThapTaiChinh");
            if (old) Object.DestroyImmediate(old.gameObject);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(pf, map.transform);
            go.name = "ThapTaiChinh";
            go.transform.SetPositionAndRotation(Pos, Quaternion.Euler(0f, Yaw, 0f));
            int off = 0;
            var sc = map.transform.Find("OW_Map_showcase");
            if (sc)
                foreach (Transform t in sc)
                    if ((t.name == "OW_ThapTaiChinh" || t.name == "OW_ThapTaiChinh_SanTrucThang") && t.gameObject.activeSelf) { t.gameObject.SetActive(false); off++; }
            int col = 0;
            foreach (var g in scene.GetRootGameObjects())
            {
                if (g.name != "VaCham_RaoLanCan") continue;
                var c = g.transform.Find("NhaNoiBat/VaCham_OW_ThapTaiChinh");
                if (c) { Object.DestroyImmediate(c.gameObject); col++; }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return $"đặt ThapTaiChinh tại {Pos} quay {go.transform.eulerAngles.y:F1}°, tắt {off} lưới tháp cũ, xóa {col} hộp va chạm cũ{log}";
        }

        static Dictionary<string, Material> BuildMaterials()
        {
            Directory.CreateDirectory(Root + "Materials");
            var dict = new Dictionary<string, Material>();
            var shader = Shader.Find("Standard");
            foreach (var f in Directory.GetFiles(Root + "Textures", "*.png"))
            {
                string path = f.Replace('\\', '/'); bool ms = path.EndsWith("_MS.png");
                var imp = (TextureImporter)AssetImporter.GetAtPath(path); bool dirty = false;
                if (imp.sRGBTexture == ms) { imp.sRGBTexture = !ms; dirty = true; }
                if (imp.maxTextureSize != 1024) { imp.maxTextureSize = 1024; dirty = true; }
                if (imp.anisoLevel != 4) { imp.anisoLevel = 4; dirty = true; }
                if (!imp.mipmapEnabled) { imp.mipmapEnabled = true; dirty = true; }
                if (imp.textureCompression != TextureImporterCompression.Compressed) { imp.textureCompression = TextureImporterCompression.Compressed; dirty = true; }
                if (dirty) imp.SaveAndReimport();
            }
            foreach (var f in Directory.GetFiles(Root + "Textures", "*.png"))
            {
                string path = f.Replace('\\', '/');
                if (path.EndsWith("_MS.png")) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                var m = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/" + name + ".mat");
                if (!m) { m = new Material(shader); AssetDatabase.CreateAsset(m, Root + "Materials/" + name + ".mat"); }
                m.shader = shader;
                m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                m.SetColor("_Color", Color.white);
                m.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Textures/" + name + "_MS.png"));
                m.EnableKeyword("_METALLICGLOSSMAP");
                m.SetFloat("_GlossMapScale", 0.92f); m.SetFloat("_SmoothnessTextureChannel", 0f);
                m.enableInstancing = true;
                EditorUtility.SetDirty(m);
                dict[name] = m;
            }
            AssetDatabase.SaveAssets();
            return dict;
        }

        static GameObject BuildPrefab(Dictionary<string, Material> mats, out string log)
        {
            Directory.CreateDirectory(Root + "Prefabs");
            var sb = new System.Text.StringBuilder();
            string path = Root + "Models/" + Name + ".fbx";
            var mi = (ModelImporter)AssetImporter.GetAtPath(path);
            if (mi.importAnimation || mi.importCameras || mi.importLights || mi.isReadable || mi.materialImportMode == ModelImporterMaterialImportMode.None)
            {
                mi.importAnimation = false; mi.importCameras = false; mi.importLights = false; mi.isReadable = false;
                mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;   // cần tên khe vật liệu để gán đúng
                mi.SaveAndReimport();
            }
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var tmp = Object.Instantiate(fbx);
            tmp.transform.position = Vector3.zero; tmp.transform.rotation = Quaternion.identity; tmp.transform.localScale = Vector3.one;
            var root = new GameObject(Name);
            var lods = new List<LOD>(); var tris = new long[3];
            string[] matOrder = { "OW_ThapTC_Kinh", "OW_ThapTC_De", "OW_ThapTC_Phu" };
            for (int k = 0; k < 3; k++)
            {
                var src = tmp.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(mf => mf.name == Name + "_LOD" + k);
                if (!src) { sb.Append("\n  thiếu LOD" + k); continue; }
                var go = new GameObject("LOD" + k);
                go.transform.SetParent(root.transform, false);
                go.transform.SetPositionAndRotation(src.transform.position, src.transform.rotation); go.transform.localScale = src.transform.lossyScale;
                go.AddComponent<MeshFilter>().sharedMesh = src.sharedMesh;
                var mr = go.AddComponent<MeshRenderer>();
                // gán theo TÊN khe vật liệu của FBX (thứ tự submesh có thể khác Blender)
                mr.sharedMaterials = src.GetComponent<MeshRenderer>().sharedMaterials.Select(sm =>
                {
                    string key = sm ? sm.name.Replace(" (Instance)", "") : "";
                    if (mats.TryGetValue(key, out var mm)) return mm;
                    sb.Append("\n  vật liệu lạ " + key); return mats[matOrder[0]];
                }).ToArray();
                mr.shadowCastingMode = k == 2 ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                lods.Add(new LOD(k == 0 ? Lod0 : k == 1 ? Lod1 : Lod2, new Renderer[] { mr }));
                for (int s = 0; s < src.sharedMesh.subMeshCount; s++) tris[k] += src.sharedMesh.GetIndexCount(s) / 3;
            }
            var colSrc = tmp.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(mf => mf.name == Name + "_VaCham");
            if (colSrc)
            {
                var cg = new GameObject("VaCham");
                cg.transform.SetParent(root.transform, false);
                cg.transform.SetPositionAndRotation(colSrc.transform.position, colSrc.transform.rotation); cg.transform.localScale = colSrc.transform.lossyScale;
                var mc = cg.AddComponent<MeshCollider>(); mc.sharedMesh = colSrc.sharedMesh; mc.convex = true;
            }
            else sb.Append("\n  thiếu lưới va chạm");
            Object.DestroyImmediate(tmp);
            var lg = root.AddComponent<LODGroup>();
            lg.SetLODs(lods.ToArray()); lg.RecalculateBounds();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
            var pf = PrefabUtility.SaveAsPrefabAsset(root, Root + "Prefabs/" + Name + ".prefab");
            Object.DestroyImmediate(root);
            sb.Insert(0, $"\n  tam giác LOD0/1/2: {tris[0]}/{tris[1]}/{tris[2]}");
            log = sb.ToString();
            return pf;
        }
    }
}
