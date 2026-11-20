using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PhoViet.OpenWorldEditor
{
    // 21.288 — Cao ốc mới cho bản đồ mở (thay các hộp trơn + lưới cửa sổ cũ của chunk OW_Map_blocks).
    // Nguồn: ArtSource/Blender/OpenWorld/ow_towers.py (MODE='export') → Towers/Models/*.fbx (mỗi file 3 mesh _LOD0.._LOD2) + Towers/Textures;
    //        ow_towers_apply_map.py → Data/towers.json (vị trí, hướng, tỉ lệ 293 cao ốc) và chunk blocks đã bỏ hộp cao ốc.
    // Menu «Phố Việt/Bản đồ mở/13. Cao ốc mới»: vật liệu Standard (màu + kim loại/độ bóng), prefab sạch (LODGroup + BoxCollider,
    // không lồng FBX), đặt dưới BanDoMo/CaoOc, cờ Occluder/Occludee; rồi chạy menu 12 để nướng lại occlusion.
    // Chạy nền: Unity -batchmode -executeMethod PhoViet.OpenWorldEditor.PhoVietOpenWorldTowerBuilder.Batch (cài + nướng + chụp ảnh).
    public static class PhoVietOpenWorldTowerBuilder
    {
        const string Root = "Assets/PhoViet/OpenWorld/Towers/";
        const string ScenePath = "Assets/PhoViet/Scenes/PhoVietOpenWorld.unity";
        public const float Lod0 = 0.35f, Lod1 = 0.07f, Lod2 = 0.0015f;

        [Serializable] class Item { public string m; public string zone; public float[] p; public float yaw; public float sxy; public float sz; }
        [Serializable] class Table { public Item[] items; }

        [MenuItem("Phố Việt/Bản đồ mở/13. Cao ốc mới (21.288)")]
        public static void Menu() => Debug.Log("[Cao ốc] " + Install());

        public static string Install()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath);
            var mats = BuildMaterials();
            var prefabs = BuildPrefabs(mats, out string prefabLog);
            var root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "BanDoMo");
            if (!root) throw new Exception("Không thấy BanDoMo");
            var old = root.transform.Find("CaoOc");
            if (old) Object.DestroyImmediate(old.gameObject);
            var parent = new GameObject("CaoOc").transform;
            parent.SetParent(root.transform, false);
            var table = JsonUtility.FromJson<Table>(File.ReadAllText("Assets/PhoViet/OpenWorld/Data/towers.json"));
            int n = 0; var missing = new HashSet<string>();
            foreach (var it in table.items)
            {
                if (!prefabs.TryGetValue(it.m, out var pf)) { missing.Add(it.m); continue; }
                var go = (GameObject)PrefabUtility.InstantiatePrefab(pf, parent);
                go.name = it.m.Replace("OW_CaoOc_", "CaoOc_") + "_" + n;
                go.transform.localPosition = new Vector3(-it.p[0], 0f, -it.p[1]);
                go.transform.localRotation = Quaternion.Euler(0f, -it.yaw, 0f);
                go.transform.localScale = new Vector3(it.sxy, it.sz, it.sxy);
                n++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return $"đặt {n} cao ốc dưới BanDoMo/CaoOc, {prefabs.Count} prefab, {mats.Count} vật liệu{(missing.Count > 0 ? ", THIẾU mẫu: " + string.Join(",", missing) : "")}\n{prefabLog}";
        }

        // ------------------------------------------------------------ texture + vật liệu
        static Dictionary<string, Material> BuildMaterials()
        {
            Directory.CreateDirectory(Root + "Materials");
            var dict = new Dictionary<string, Material>();
            var shader = Shader.Find("Standard");
            foreach (var f in Directory.GetFiles(Root + "Textures", "*.png"))
            {
                string path = f.Replace('\\', '/');
                bool ms = path.EndsWith("_MS.png");
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                bool dirty = false;
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
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var msTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Textures/" + name + "_MS.png");
                string mpath = Root + "Materials/" + name + ".mat";
                var m = AssetDatabase.LoadAssetAtPath<Material>(mpath);
                if (!m) { m = new Material(shader); AssetDatabase.CreateAsset(m, mpath); }
                m.shader = shader;
                m.SetTexture("_MainTex", tex);
                m.SetColor("_Color", Color.white);
                if (msTex)
                {
                    m.SetTexture("_MetallicGlossMap", msTex); m.EnableKeyword("_METALLICGLOSSMAP");
                    m.SetFloat("_GlossMapScale", 0.92f); m.SetFloat("_SmoothnessTextureChannel", 0f);
                }
                else { m.SetFloat("_Metallic", 0f); m.SetFloat("_Glossiness", 0.2f); }
                m.enableInstancing = true;
                EditorUtility.SetDirty(m);
                dict[name] = m;
            }
            AssetDatabase.SaveAssets();
            return dict;
        }

        // ------------------------------------------------------------ prefab sạch: root (LODGroup + BoxCollider) → LOD0..2 (MeshFilter + MeshRenderer)
        static Dictionary<string, GameObject> BuildPrefabs(Dictionary<string, Material> mats, out string log)
        {
            Directory.CreateDirectory(Root + "Prefabs");
            var res = new Dictionary<string, GameObject>(); var sb = new System.Text.StringBuilder();
            foreach (var f in Directory.GetFiles(Root + "Models", "*.fbx"))
            {
                string path = f.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(path);
                var mi = (ModelImporter)AssetImporter.GetAtPath(path);
                if (mi.importAnimation || mi.importCameras || mi.importLights || mi.isReadable)
                {
                    mi.importAnimation = false; mi.importCameras = false; mi.importLights = false; mi.isReadable = false;
                    mi.SaveAndReimport();
                }
                var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var tmp = Object.Instantiate(fbx);
                tmp.transform.position = Vector3.zero; tmp.transform.rotation = Quaternion.identity; tmp.transform.localScale = Vector3.one;
                var root = new GameObject(name);
                var lods = new List<LOD>(); long[] tris = new long[3];
                Bounds colB = default; bool haveCol = false;
                for (int k = 0; k < 3; k++)
                {
                    var src = tmp.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(mf => mf.name == name + "_LOD" + k);
                    if (!src) { sb.Append("\n  thiếu " + name + "_LOD" + k); continue; }
                    var go = new GameObject("LOD" + k);
                    go.transform.SetParent(root.transform, false);
                    go.transform.position = src.transform.position; go.transform.rotation = src.transform.rotation; go.transform.localScale = src.transform.lossyScale;
                    go.AddComponent<MeshFilter>().sharedMesh = src.sharedMesh;
                    var mr = go.AddComponent<MeshRenderer>();
                    var srcMats = src.GetComponent<MeshRenderer>().sharedMaterials;
                    mr.sharedMaterials = srcMats.Select(sm =>
                    {
                        string key = sm ? sm.name.Replace(" (Instance)", "") : "";
                        if (mats.TryGetValue(key, out var mm)) return mm;
                        sb.Append("\n  vật liệu lạ " + key + " ở " + name); return sm;
                    }).ToArray();
                    mr.shadowCastingMode = k == 2 ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
                    mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    lods.Add(new LOD(k == 0 ? Lod0 : k == 1 ? Lod1 : Lod2, new Renderer[] { mr }));
                    for (int s = 0; s < src.sharedMesh.subMeshCount; s++) tris[k] += src.sharedMesh.GetIndexCount(s) / 3;
                    if (k == 1) { colB = mr.bounds; haveCol = true; }
                }
                Object.DestroyImmediate(tmp);
                var lg = root.AddComponent<LODGroup>();
                lg.SetLODs(lods.ToArray()); lg.RecalculateBounds();
                if (haveCol)
                {
                    var bc = root.AddComponent<BoxCollider>();
                    bc.center = colB.center; bc.size = colB.size;
                }
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
                var pf = PrefabUtility.SaveAsPrefabAsset(root, Root + "Prefabs/" + name + ".prefab");
                Object.DestroyImmediate(root);
                res[name] = pf;
                sb.Append($"\n  {name}: {tris[0]}/{tris[1]}/{tris[2]} tam giác, va chạm {colB.size}");
            }
            log = sb.ToString();
            return res;
        }

        // ------------------------------------------------------------ chạy nền: cài + nướng occlusion + chụp ảnh kiểm tra
        public static void Batch()
        {
            var lines = new List<string>();
            int code = 0;
            try
            {
                lines.Add(Install());
                lines.Add(PhoVietOpenWorldOcclusionBuilder.ApplyFlags());
                var t0 = DateTime.Now;
                PhoVietOpenWorldOcclusionBuilder.Bake(true);
                lines.Add(PhoVietOpenWorldOcclusionBuilder.Finish() + $" ({(DateTime.Now - t0).TotalSeconds:0} s)");
                lines.AddRange(Capture());
            }
            catch (Exception e) { lines.Add("LỖI " + e); code = 1; }
            File.WriteAllText("Builds/towers21288-status.txt", string.Join("\n", lines));
            Debug.Log("[Cao ốc] " + string.Join("\n", lines));
            EditorApplication.Exit(code);
        }

        public static void BatchCapture()
        {
            var lines = new List<string>(); int code = 0;
            try { EditorSceneManager.OpenScene(ScenePath); lines.AddRange(Capture()); }
            catch (Exception e) { lines.Add("LỖI " + e); code = 1; }
            File.WriteAllText("Builds/towers21288-capture.txt", string.Join("\n", lines));
            EditorApplication.Exit(code);
        }

        static readonly (string name, Vector3 pos, Vector3 look)[] Views =
        {
            ("pho-xuat-phat", new Vector3(218.5f, 3.4f, -409f), new Vector3(218.5f, 6f, -360f)),
            ("bo-song", new Vector3(231f, 3.2f, -150f), new Vector3(330f, 25f, -150f)),
            ("bo-song-bac", new Vector3(100f, 3.2f, 232f), new Vector3(100f, 22f, 330f)),
            ("tai-chinh-tren-cao", new Vector3(60f, 95f, -520f), new Vector3(110f, 40f, -150f)),
            ("tai-chinh-gan", new Vector3(110f, 2.5f, -300f), new Vector3(110f, 40f, -200f)),
        };

        public static List<string> Capture()
        {
            var lines = new List<string>();
            Directory.CreateDirectory("Captures");
            var main = Camera.main ?? Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
            var go = new GameObject("CamKiemTra21288");
            var cam = go.AddComponent<Camera>();
            if (main) cam.CopyFrom(main);
            cam.fieldOfView = 60f; cam.enabled = false;
            var rt = new RenderTexture(1600, 900, 24) { antiAliasing = 4 };
            cam.targetTexture = rt;
            var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            foreach (var v in Views)
            {
                go.transform.position = v.pos; go.transform.LookAt(v.look);
                cam.Render();
                RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); tex.Apply(); RenderTexture.active = null;
                File.WriteAllBytes("Captures/towers21288-" + v.name + ".png", tex.EncodeToPNG());
                lines.Add("ảnh " + v.name + ": " + PhoVietOpenWorldBuilder.MeasureCamera(cam).Replace("\n", " |"));
            }
            cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
            return lines;
        }
    }
}
