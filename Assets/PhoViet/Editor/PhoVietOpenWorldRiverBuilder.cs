using System.Collections.Generic;
using PhoViet.OpenWorld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.OpenWorldEditor
{
    // 21.271 — menu 9: mặt sông mới (lưới Blender OW_MatSong.fbx + shader PhoViet/OpenWorld/River), tàu chạy trên sông
    // (OpenWorldBoats), Reflection Probe nướng sẵn dọc sông. Chạy lại được nhiều lần trên scene PhoVietOpenWorld đang mở.
    public static class PhoVietOpenWorldRiverBuilder
    {
        const string Dir = "Assets/PhoViet/OpenWorld/River/";
        const string Fbx = Dir + "OW_MatSong.fbx";
        const string ProbeDir = Dir + "Probes/";

        // Vị trí probe (x, z thế giới) + cỡ hộp (ngang sông, dọc sông). Nhánh dọc tim x 317,25; nhánh ngang tim z 314,25.
        static readonly (float x, float z, float sx, float sz)[] Probes =
        {
            (317.25f, -760f, 420f, 420f), (317.25f, -500f, 300f, 170f), (317.25f, -340f, 300f, 170f), (317.25f, -180f, 300f, 170f),
            (317.25f, -20f, 300f, 170f), (317.25f, 140f, 300f, 170f), (330f, 330f, 300f, 300f),
            (150f, 314.25f, 170f, 300f), (-10f, 314.25f, 170f, 300f), (-170f, 314.25f, 170f, 300f), (-330f, 314.25f, 170f, 300f),
            (-490f, 314.25f, 170f, 300f), (-760f, 314.25f, 420f, 420f),
        };

        [MenuItem("Phố Việt/Bản đồ mở/9. Mặt sông + tàu + phản chiếu (scene đang mở)")]
        public static void BuildRiverMenu() => BuildRiver(true);

        public static string BuildRiver(bool bake)
        {
            var map = GameObject.Find("BanDoMo");
            if (!map) return "mở scene PhoVietOpenWorld trước";
            var log = new System.Text.StringBuilder();
            var riverMat = AssetDatabase.LoadAssetAtPath<Material>(Dir + "OW_River.mat");
            var wakeMat = AssetDatabase.LoadAssetAtPath<Material>(Dir + "OW_BoatWake.mat");

            // 1) lưới mặt sông
            var imp = (ModelImporter)AssetImporter.GetAtPath(Fbx);
            if (imp == null) return "thiếu " + Fbx + " (chạy build_river_surface.py trong Blender)";
            if (imp.materialImportMode != ModelImporterMaterialImportMode.None || imp.isReadable || imp.importNormals != ModelImporterNormals.Import)
            { imp.materialImportMode = ModelImporterMaterialImportMode.None; imp.isReadable = false; imp.importNormals = ModelImporterNormals.Import; imp.importAnimation = false; imp.SaveAndReimport(); }
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
            var oldNew = GameObject.Find("MatSongMoi"); if (oldNew) Object.DestroyImmediate(oldNew);
            var oldFlat = GameObject.Find("MatSong"); if (oldFlat) oldFlat.SetActive(false);                   // mặt phẳng ô 5 m cũ (21.239)
            var outer = GameObject.Find("OW_Map_SongNgoaiMep"); if (outer) outer.GetComponent<Renderer>().enabled = false;   // hộp nước ngoài mép → lưới mới phủ tới −1400 m
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, map.scene);
            go.name = "MatSongMoi"; go.transform.SetParent(map.transform, false);
            int chunks = 0;
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                r.sharedMaterial = riverMat; chunks++;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;    // pass ShadowCaster để mặt nước nhận bóng cầu/tàu
                r.receiveShadows = true;
                r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;   // phản chiếu do OpenWorldRiverReflections trộn theo điểm ảnh
                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                GameObjectUtility.SetStaticEditorFlags(r.gameObject, 0);               // không vẽ chính mặt nước vào probe
            }
            log.Append("mặt sông ").Append(chunks).Append(" khúc; ");

            // 2) tàu: tắt 6 tàu đặt tĩnh (Default-Material, có chiếc nằm trên luồng tàu chạy), gắn OpenWorldBoats
            int hid = 0;
            foreach (var t in map.GetComponentsInChildren<Transform>(true))
                if (t.name == "PV_FishingBoat" && t.parent && t.parent.name == "Khac") { t.gameObject.SetActive(false); hid++; }
            var boatsGo = GameObject.Find("TauTrenSong") ?? new GameObject("TauTrenSong");
            var boats = boatsGo.GetComponent<OpenWorldBoats>() ?? boatsGo.AddComponent<OpenWorldBoats>();
            boats.boatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/River/FishingBoat/PV_FishingBoat.fbx");
            boats.boatMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/River/FishingBoat/PV_FishingBoat.mat");
            boats.wakeMaterial = wakeMat;
            if (boats.boatMaterial) boats.boatMaterial.enableInstancing = true;
            log.Append("ẩn ").Append(hid).Append(" tàu tĩnh; ");

            // 3) Reflection Probe dọc sông (hộp chiếu, nướng các vật thể tĩnh + bầu trời)
            var probeRoot = GameObject.Find("PhanChieuSong"); if (probeRoot) Object.DestroyImmediate(probeRoot);
            probeRoot = new GameObject("PhanChieuSong");
            var list = new List<ReflectionProbe>();
            for (int i = 0; i < Probes.Length; i++)
            {
                var p = Probes[i];
                var pg = new GameObject("Probe_" + i.ToString("00")); pg.transform.SetParent(probeRoot.transform, false);
                pg.transform.position = new Vector3(p.x, -0.6f, p.z);                   // ≈ 1,6 m trên mặt nước
                var rp = pg.AddComponent<ReflectionProbe>();
                rp.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
                rp.resolution = 256; rp.hdr = false; rp.boxProjection = true;
                rp.size = new Vector3(p.sx, 90f, p.sz); rp.center = new Vector3(0, 43f, 0);  // hộp từ đáy sông tới mái nhà ven sông
                rp.blendDistance = 25f; rp.importance = 1; rp.nearClipPlane = 0.5f; rp.farClipPlane = 1200f;
                rp.clearFlags = UnityEngine.Rendering.ReflectionProbeClearFlags.Skybox;
                list.Add(rp);
            }
            if (bake) log.Append(BakeProbes(map, list));
            probeRoot.AddComponent<OpenWorldRiverReflections>();
            EditorSceneManager.MarkSceneDirty(map.scene);
            EditorSceneManager.SaveScene(map.scene);
            Debug.Log("[Bản đồ mở] " + log);
            return log.ToString();
        }

        // Nướng từng probe: tạm gắn cờ ReflectionProbeStatic cho mọi thứ của BanDoMo (trừ mặt sông) rồi trả cờ cũ;
        // ảnh nướng chuyển sang chế độ Custom để không phụ thuộc Lighting Data của scene.
        public static string BakeProbes(GameObject map, List<ReflectionProbe> probes)
        {
            System.IO.Directory.CreateDirectory(ProbeDir);
            var saved = new Dictionary<GameObject, StaticEditorFlags>();
            foreach (var t in map.GetComponentsInChildren<Transform>(false))
            {
                var g = t.gameObject; if (IsWater(t)) continue;
                var f = GameObjectUtility.GetStaticEditorFlags(g); saved[g] = f;
                GameObjectUtility.SetStaticEditorFlags(g, f | StaticEditorFlags.ReflectionProbeStatic);
            }
            int ok = 0;
            try
            {
                for (int i = 0; i < probes.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Phản chiếu sông", "Probe " + i, i / (float)probes.Count);
                    var rp = probes[i]; string path = ProbeDir + "OW_SongProbe_" + i.ToString("00") + ".png";
                    rp.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
                    if (!Lightmapping.BakeReflectionProbe(rp, path)) continue;
                    AssetDatabase.ImportAsset(path);
                    var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                    if (ti)
                    {
                        ti.textureShape = TextureImporterShape.TextureCube; ti.mipmapEnabled = true; ti.maxTextureSize = 256;
                        ti.sRGBTexture = true; ti.textureCompression = TextureImporterCompression.Compressed;
                        ti.SaveAndReimport();
                    }
                    var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                    rp.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom; rp.customBakedTexture = tex;
                    if (tex) ok++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                foreach (var kv in saved) if (kv.Key) GameObjectUtility.SetStaticEditorFlags(kv.Key, kv.Value);
            }
            return "nướng " + ok + "/" + probes.Count + " probe";
        }

        static bool IsWater(Transform t)
        {
            for (var p = t; p; p = p.parent) if (p.name == "MatSongMoi" || p.name == "MatSong" || p.name == "OW_Map_SongNgoaiMep") return true;
            return false;
        }
    }
}
