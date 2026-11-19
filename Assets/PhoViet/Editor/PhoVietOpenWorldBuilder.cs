using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PhoViet.OpenWorld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.OpenWorldEditor
{
    // Đợt 6 (21.249): dựng scene bản đồ mở PhoVietOpenWorld.unity từ dữ liệu xuất của Blender (Assets/PhoViet/OpenWorld).
    // Scene cũ PhoViet.unity và bản build web KHÔNG bị đụng tới. Gốc "BanDoMo" xoay 180° quanh Y để Unity x/z = map X/Z
    // (quy ước trục dự án: Unity (x,y,z) ↔ Blender (−x, −z, y)).
    public static class PhoVietOpenWorldBuilder
    {
        const string Root = "Assets/PhoViet/OpenWorld/";
        const string ScenePath = "Assets/PhoViet/Scenes/PhoVietOpenWorld.unity";
        static readonly string[] Chunks = { "ground", "roads", "river", "bridges", "showcase", "blocks", "street" };

        [System.Serializable] class MatEntry { public string name; public float[] color; public float rough; public float metal; public float[] emit; public float emitStrength; public string tex; public float tileH; public float tileV; public bool water; }
        [System.Serializable] class MatTable { public MatEntry[] materials; }
        [System.Serializable] class Place { public string a; public float[] p; public float yaw; public float s; }
        [System.Serializable] class PlaceTable { public Place[] items; }
        [System.Serializable] class Rect2 { public string name; public float x0, z0, x1, z1, top; }
        [System.Serializable] class Spawn { public float[] p; public float yaw; }
        [System.Serializable] class ColTable { public Rect2[] land; public Rect2[] bridgeDecks; public Rect2[] solids; public Spawn spawn; }

        [MenuItem("Phố Việt/Bản đồ mở/1. Vật liệu + gán cho FBX")]
        public static void SetupMaterials()
        {
            var mats = BuildMaterials();
            RemapModels(mats);
            Debug.Log("[Bản đồ mở] vật liệu: " + mats.Count);
        }

        [MenuItem("Phố Việt/Bản đồ mở/2. Dựng scene PhoVietOpenWorld")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupLighting();
            var root = new GameObject("BanDoMo"); root.transform.rotation = Quaternion.Euler(0, 180, 0);
            foreach (var k in Chunks)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Chunks/OW_Map_" + k + ".fbx");
                if (!asset) { Debug.LogWarning("[Bản đồ mở] thiếu khối " + k); continue; }
                var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
                go.transform.SetParent(root.transform, false);
                SetStatic(go);
                if (k == "showcase")                                   // từng quầy hàng/quán/nhà nổi bật ẩn riêng khi xa
                    foreach (Transform ch in go.transform)
                    {
                        if (ch.childCount > 0) foreach (Transform g in ch) CullLOD(g.gameObject, 0.02f);
                        else CullLOD(ch.gameObject, 0.015f);
                    }
                if (k == "river" || k == "street")                     // đoạn lan can ven sông / hàng rào 60–80 m
                    foreach (Transform ch in go.transform)
                        if (ch.name.Contains("LanCanSong") || ch.name.Contains("HangRao")) CullLOD(ch.gameObject, 0.03f);
            }
            int n = PlaceInstances(root.transform, scene);
            BuildWater();
            var cols = JsonUtility.FromJson<ColTable>(File.ReadAllText(Root + "Data/colliders.json"));
            BuildColliders(cols);
            var car = SpawnPlayer(cols.spawn);
            BuildTraffic(car.transform);
            BuildPolice(car.transform);
            BuildHazards(car.transform);
            BuildGameLoop(car.transform);
            var cam = new GameObject("CameraChinh"); cam.tag = "MainCamera";
            var c = cam.AddComponent<Camera>(); c.farClipPlane = 1500; c.nearClipPlane = 0.2f; c.fieldOfView = 58;
            cam.AddComponent<AudioListener>();
            var oc = cam.AddComponent<OpenWorldCamera>(); oc.target = car.transform;
            cam.transform.position = car.transform.position + new Vector3(0, 4.2f, -9.5f);
            cam.transform.LookAt(car.transform.position + Vector3.forward * 6);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[Bản đồ mở] đã dựng scene với " + n + " instance → " + ScenePath);
        }

        // 21.250: đo tải thật ở camera chính — chỉ tính renderer trong vùng nhìn và đúng mức LOD mà LODGroup chọn
        // theo khoảng cách (UnityStats cộng cả Scene view nên không dùng được). Không tính bóng đổ.
        [MenuItem("Phố Việt/Bản đồ mở/3. Đo tải camera chính")]
        public static void MeasureMenu() => Debug.Log("[Bản đồ mở] " + MeasureCamera(Camera.main));

        public static string MeasureCamera(Camera cam)
        {
            if (!cam) return "không có camera";
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            var tris = new Dictionary<string, long>(); var draws = new Dictionary<string, int>();
            var groups = new HashSet<(Object, int, Object)>();
            var inLod = new HashSet<Renderer>();
            float half = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            string G(Transform t) { while (t.parent != null && t.parent.name != "BanDoMo") t = t.parent; return t.parent == null ? "ngoài" : t.name; }
            void Add(Renderer r)
            {
                var mf = r.GetComponent<MeshFilter>(); var m = mf ? mf.sharedMesh : null; if (!m) return;
                long t = 0; for (int i = 0; i < m.subMeshCount; i++) t += m.GetIndexCount(i) / 3;
                var g = G(r.transform);
                tris[g] = (tris.TryGetValue(g, out var a) ? a : 0) + t;
                draws[g] = (draws.TryGetValue(g, out var d) ? d : 0) + m.subMeshCount;
                var mats = r.sharedMaterials;
                for (int i = 0; i < m.subMeshCount; i++) groups.Add((m, i, i < mats.Length ? mats[i] : null));
            }
            foreach (var lg in Object.FindObjectsByType<LODGroup>())
            {
                var lods = lg.GetLODs(); foreach (var L in lods) foreach (var r in L.renderers) if (r) inLod.Add(r);
                var wc = lg.transform.TransformPoint(lg.localReferencePoint);
                float size = lg.size * Mathf.Max(lg.transform.lossyScale.x, lg.transform.lossyScale.y, lg.transform.lossyScale.z);
                float dist = Vector3.Distance(cam.transform.position, wc);
                float rel = size / (2f * Mathf.Max(dist, 0.01f) * half) * QualitySettings.lodBias;
                int idx = -1; for (int i = 0; i < lods.Length; i++) if (rel >= lods[i].screenRelativeTransitionHeight) { idx = i; break; }
                if (idx < 0 || dist > cam.farClipPlane) continue;
                foreach (var r in lods[idx].renderers)
                    if (r && r.enabled && r.gameObject.activeInHierarchy && GeometryUtility.TestPlanesAABB(planes, r.bounds)) Add(r);
            }
            foreach (var r in Object.FindObjectsByType<MeshRenderer>())
                if (!inLod.Contains(r) && r.enabled && r.gameObject.activeInHierarchy && GeometryUtility.TestPlanesAABB(planes, r.bounds)) Add(r);
            long sum = 0; int dsum = 0; var sb = new System.Text.StringBuilder();
            foreach (var kv in tris) { sum += kv.Value; dsum += draws[kv.Key]; sb.Append("\n  " + kv.Key + ": " + kv.Value + " tam giác, " + draws[kv.Key] + " lượt vẽ"); }
            return "camera " + cam.name + " @" + cam.transform.position + " lodBias=" + QualitySettings.lodBias + " → TỔNG " + sum + " tam giác, " + dsum + " lượt vẽ trước gộp, " + groups.Count + " nhóm instancing (mesh+vật liệu)" + sb;
        }

        // ------------------------------------------------------------ vật liệu
        static string Strip(string n) => Regex.Replace(n, @"\.\d{3}$", "");
        static Color Lin(float[] c) => c == null || c.Length < 3 ? Color.white : new Color(c[0], c[1], c[2]).gamma;

        static Dictionary<string, Material> BuildMaterials()
        {
            Directory.CreateDirectory(Root + "Materials");
            var shader = Shader.Find("PhoViet/OpenWorld/Triplanar");
            var table = JsonUtility.FromJson<MatTable>(File.ReadAllText(Root + "Data/materials.json"));
            var dict = new Dictionary<string, Material>();
            foreach (var e in table.materials)
            {
                string path = Root + "Materials/" + e.name + ".mat";
                Material m;
                if (e.water || e.name == "OW_RiverWater")
                {
                    // Dùng lại hiệu ứng nước sông có sẵn (RiverWater.shader), tắt thông số riêng của khối cầu cũ.
                    if (!File.Exists(path)) AssetDatabase.CopyAsset("Assets/PhoViet/Art/River/PV_RiverWater.mat", path);
                    m = AssetDatabase.LoadAssetAtPath<Material>(path);
                    m.SetVector("_Deck", new Vector4(0, -100, -100, 0));
                    m.SetVector("_Banks", new Vector4(-1e5f, 1e5f, -100, 0));
                    m.SetVector("_Piers", new Vector4(1e5f, 1e5f, 0, 0));
                    m.SetFloat("_WaterY", -2.2f);
                }
                else
                {
                    m = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (!m) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
                    m.shader = shader;
                    m.color = Lin(e.color);
                    m.mainTexture = string.IsNullOrEmpty(e.tex) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(Root + e.tex);
                    m.SetFloat("_TileH", e.tileH > 0 ? e.tileH : 4f);
                    m.SetFloat("_TileV", e.tileV > 0 ? e.tileV : 4f);
                    m.SetFloat("_Glossiness", Mathf.Clamp(1f - e.rough, 0f, 0.92f));
                    m.SetFloat("_Metallic", e.metal);
                    m.SetColor("_EmissionColor", Lin(e.emit) * e.emitStrength * 0.6f);
                    m.enableInstancing = true;
                }
                EditorUtility.SetDirty(m);
                dict[e.name] = m;
            }
            AssetDatabase.SaveAssets();
            return dict;
        }

        static void RemapModels(Dictionary<string, Material> mats)
        {
            var atlas = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { Root + "Library", Root + "Chunks" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;
                imp.importCameras = false; imp.importLights = false; imp.importAnimation = false;
                imp.animationType = ModelImporterAnimationType.None;
                imp.indexFormat = ModelImporterIndexFormat.Auto;
                imp.addCollider = path.Contains("OW_Map_blocks") || path.Contains("OW_Map_bridges");
                foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                {
                    if (!(o is Material mm)) continue;
                    var key = Strip(mm.name);
                    if (mats.TryGetValue(key, out var target)) imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mm.name), target);
                    else if (key == "PV_StreetAtlas" && atlas) imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mm.name), atlas);
                }
                imp.SaveAndReimport();
            }
        }

        // ------------------------------------------------------------ ánh sáng
        static void SetupLighting()
        {
            var sun = new GameObject("MatTroi");
            var l = sun.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f; l.color = new Color(1f, 0.87f, 0.72f);
            l.shadows = LightShadows.Soft; l.shadowStrength = 0.75f;
            sun.transform.rotation = Quaternion.Euler(30, 235, 0);
            RenderSettings.sun = l;
            RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox; RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 300; RenderSettings.fogEndDistance = 1450;
            RenderSettings.fogColor = new Color(0.78f, 0.86f, 0.93f);
        }

        // ------------------------------------------------------------ đặt instance
        static readonly Dictionary<string, GameObject> assetCache = new Dictionary<string, GameObject>();
        static readonly Dictionary<GameObject, Bounds> boundsCache = new Dictionary<GameObject, Bounds>();

        static GameObject Resolve(string key)
        {
            if (assetCache.TryGetValue(key, out var g)) return g;
            string path;
            if (key.StartsWith("Library/")) path = Root + key + ".fbx";
            else if (key.StartsWith("Tree")) path = "Assets/PhoViet/Art/Streets/PV_StreetTree" + key.Substring(4) + ".fbx";
            else if (key == "FishingBoat") path = "Assets/PhoViet/Art/River/FishingBoat/PV_FishingBoat.fbx";
            else path = null;
            g = path == null ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
            assetCache[key] = g;
            return g;
        }

        static Bounds LocalBounds(GameObject asset)
        {
            if (boundsCache.TryGetValue(asset, out var b)) return b;
            var tmp = Object.Instantiate(asset); tmp.transform.position = Vector3.zero;   // giữ góc xoay gốc của FBX
            bool any = false; b = new Bounds();
            foreach (var r in tmp.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            Object.DestroyImmediate(tmp);
            boundsCache[asset] = b; return b;
        }

        static int PlaceInstances(Transform root, UnityEngine.SceneManagement.Scene scene)
        {
            var table = JsonUtility.FromJson<PlaceTable>(File.ReadAllText(Root + "Data/placements.json"));
            var groups = new Dictionary<string, Transform>();
            Transform Group(string name)
            {
                if (groups.TryGetValue(name, out var t)) return t;
                t = new GameObject(name).transform; t.SetParent(root, false); groups[name] = t; return t;
            }
            int n = 0, missing = 0;
            try
            {
                for (int i = 0; i < table.items.Length; i++)
                {
                    var it = table.items[i];
                    if (i % 400 == 0) EditorUtility.DisplayProgressBar("Bản đồ mở", "Đặt instance " + i + "/" + table.items.Length, i / (float)table.items.Length);
                    var asset = Resolve(it.a);
                    if (!asset) { missing++; continue; }
                    string grp = it.a.Contains("NhaPho") ? "NhaPho" : it.a.Contains("_Bang_") ? "BangHieu" : it.a.Contains("CotDen") ? "CotDen" : it.a.StartsWith("Tree") || it.a.Contains("ChauCay") ? "Cay" : "Khac";
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
                    go.transform.SetParent(Group(grp), false);
                    var lp = new Vector3(-it.p[0], it.p[2], -it.p[1]); var lr = Quaternion.Euler(0, -it.yaw, 0); float sc = it.s > 0 ? it.s : 1f;
                    go.transform.localPosition = lp;
                    go.transform.localRotation = lr * asset.transform.localRotation;      // giữ góc xoay gốc của FBX (FBX một đối tượng có gốc xoay −90° X)
                    go.transform.localScale = asset.transform.localScale * sc;
                    if (it.a.StartsWith("Tree")) PrepareTree(go);
                    // Va chạm nằm trên đối tượng con riêng, hướng thẳng đứng, không phụ thuộc góc xoay gốc FBX.
                    GameObject Col()
                    {
                        var c = new GameObject("VaCham"); c.transform.SetParent(Group("VaCham_" + grp), false);
                        c.transform.localPosition = lp; c.transform.localRotation = lr; c.transform.localScale = Vector3.one * sc; c.isStatic = true; return c;
                    }
                    if (grp == "NhaPho")
                    {
                        var b = LocalBounds(asset);
                        float minX = Mathf.Max(b.min.x, 0f);                      // bỏ phần mái hiên/ban công nhô ra vỉa hè (mặt tiền ở x = 0)
                        var bc = Col().AddComponent<BoxCollider>();
                        bc.center = new Vector3((minX + b.max.x) / 2, b.center.y, b.center.z);
                        bc.size = new Vector3(b.max.x - minX, b.size.y, b.size.z);
                    }
                    else if (grp == "Cay")
                    {
                        var c = Col();
                        var cc = c.AddComponent<CapsuleCollider>(); cc.radius = 0.3f; cc.height = 6f; cc.center = new Vector3(0, 3f, 0);
                        var bc = c.AddComponent<BoxCollider>(); bc.size = new Vector3(1.46f, 0.5f, 1.46f); bc.center = new Vector3(0, 0.25f, 0);
                    }
                    else if (grp == "CotDen")
                    {
                        var cc = Col().AddComponent<CapsuleCollider>(); cc.radius = 0.18f; cc.height = 9f; cc.center = new Vector3(0, 4.6f, 0);
                    }
                    if (grp != "Khac") SetStatic(go);
                    // 21.250: ẩn theo kích thước trên màn hình (LOD một mức + cull).
                    if (grp == "NhaPho") BuildingLOD(go, asset); else if (grp == "BangHieu") CullLOD(go, 0.06f); else if (grp == "CotDen") CullLOD(go, 0.02f); else if (grp == "Khac") CullLOD(go, 0.01f);
                    n++;
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            if (missing > 0) Debug.LogWarning("[Bản đồ mở] thiếu asset cho " + missing + " instance");
            return n;
        }

        // Cây có sẵn PV_StreetTree*: vật liệu atlas + LODGroup (game cũ gán bằng PhoVietStreetUpgrade).
        static Material treeAtlas;
        static void PrepareTree(GameObject go)
        {
            if (!treeAtlas) treeAtlas = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
            var lods = new List<LOD>();
            float[] h = { 0.12f, 0.04f, 0.008f };
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (treeAtlas) { r.sharedMaterial = treeAtlas; treeAtlas.enableInstancing = true; }
                for (int k = 0; k < 3; k++) if (r.name.EndsWith("LOD" + k)) lods.Add(new LOD(h[k], new[] { r }));
            }
            var have = go.GetComponent<LODGroup>();
            if (have)                                                            // 21.250: ngưỡng LOD cây gắt hơn (LOD0 29 nghìn tam giác chỉ ở gần)
            {
                var L = have.GetLODs(); float[] th = { 0.5f, 0.22f, 0.09f };       // cây cao ~10 m: LOD0 < ~35 m, LOD1 < ~80 m, LOD2 < ~200 m, xa hơn ẩn (có sương)
                for (int k = 0; k < L.Length && k < 3; k++) L[k].screenRelativeTransitionHeight = th[k];
                have.SetLODs(L);
            }
            if (lods.Count > 1 && have == null)       // FBX cây có sẵn có thể đã kèm LODGroup
            {
                lods.Sort((a, b) => b.screenRelativeTransitionHeight.CompareTo(a.screenRelativeTransitionHeight));
                var lg = go.AddComponent<LODGroup>();
                if (lg) lg.SetLODs(lods.ToArray());
            }
        }

        // Mặt sông dùng lại RiverWater.shader: lưới ô 5 m giữ trục Blender (Z lên), renderer xoay −90° quanh X như mặt sông cũ.
        static void BuildWater()
        {
            var src = GameObject.Find("OW_Map_Song"); if (src) { var r0 = src.GetComponent<Renderer>(); if (r0) r0.enabled = false; }
            // Đáy sông màu nước sâu tối (nước trong suốt; đáy đá sáng làm sông trông như cát).
            var bedPath = Root + "Materials/OW_DaySong.mat";
            var bedMat = AssetDatabase.LoadAssetAtPath<Material>(bedPath);
            if (!bedMat) { bedMat = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.05f, 0.16f, 0.2f) }; AssetDatabase.CreateAsset(bedMat, bedPath); }
            var bed = GameObject.Find("OW_Map_DaySong"); if (bed) bed.GetComponent<Renderer>().sharedMaterial = bedMat;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/OW_RiverWater.mat");
            var go = new GameObject("MatSong"); go.transform.rotation = Quaternion.Euler(-90, 0, 0);
            var verts = new List<Vector3>(); var tris = new List<int>();
            void Rect(float x0, float z0, float x1, float z1, float step)
            {
                int nx = Mathf.CeilToInt((x1 - x0) / step), nz = Mathf.CeilToInt((z1 - z0) / step); int b = verts.Count;
                for (int j = 0; j <= nz; j++) for (int i = 0; i <= nx; i++)
                {
                    float x = Mathf.Lerp(x0, x1, i / (float)nx), z = Mathf.Lerp(z0, z1, j / (float)nz);
                    verts.Add(new Vector3(x, -z, -2.2f));                         // cục bộ (X, −Z, cao độ) → thế giới (X, cao độ, Z)
                }
                for (int j = 0; j < nz; j++) for (int i = 0; i < nx; i++)
                {
                    int a = b + j * (nx + 1) + i;
                    tris.AddRange(new[] { a, a + 1, a + nx + 1, a + 1, a + nx + 2, a + nx + 1 });
                }
            }
            Rect(234.5f, -1000f, 400f, 400f, 5f);         // nhánh dọc (+ góc)
            Rect(-1000f, 228.5f, 234.5f, 400f, 5f);       // nhánh ngang
            var mesh = new Mesh { name = "OW_MatSong", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(verts); mesh.SetTriangles(tris, 0, true); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            var meshPath = Root + "Materials/OW_MatSong.asset";
            AssetDatabase.DeleteAsset(meshPath); AssetDatabase.CreateAsset(mesh, meshPath);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // ------------------------------------------------------------ va chạm mặt đất, mặt cầu, khối đặc
        static void BuildColliders(ColTable t)
        {
            var root = new GameObject("VaCham").transform;
            void Box(string name, Rect2 r, float top, float depth)
            {
                var g = new GameObject(name); g.transform.SetParent(root, false);
                var bc = g.AddComponent<BoxCollider>();
                bc.center = new Vector3((r.x0 + r.x1) / 2, top - depth / 2, (r.z0 + r.z1) / 2);
                bc.size = new Vector3(Mathf.Abs(r.x1 - r.x0), depth, Mathf.Abs(r.z1 - r.z0));
                g.isStatic = true;
            }
            foreach (var r in t.land) Box("MatDat", r, 0f, 2f);
            foreach (var r in t.bridgeDecks) Box("MatCau", r, 0f, 1.2f);
            if (t.solids != null) foreach (var r in t.solids) Box(r.name, r, r.top, r.top);
        }

        // ------------------------------------------------------------ giao thông NPC (21.251, đợt 7 phần 1)
        [System.Serializable] class LayoutRoad { public string id; public string profile; public float[] @from; public float[] to; }
        [System.Serializable] class LayoutTable { public LayoutRoad[] roads; }
        [System.Serializable] class CarLights { public float[] headlight; public float[] taillight; }
        static Vector3 V3(float[] a) => a != null && a.Length == 3 ? new Vector3(a[0], a[1], a[2]) : Vector3.zero;

        // 21.253: atlas số LED 7 đoạn: 11 ô (0–9 + ô trống) 48×80 px; đoạn sáng trắng, đoạn tắt xám mờ (ánh "bóng ma" như LED thật).
        static Texture2D SaveTexture(string name, Texture2D tex, bool alpha)
        {
            var path = Root + "Textures/" + name + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG()); Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.wrapMode = TextureWrapMode.Clamp; imp.alphaIsTransparency = alpha; imp.mipmapEnabled = true;
            imp.textureCompression = TextureImporterCompression.Uncompressed; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Texture2D DigitAtlas()
        {
            const int W = 48, Hh = 80, N = 11;
            var tex = new Texture2D(W * N, Hh, TextureFormat.RGBA32, false);
            var px = new Color32[W * N * Hh];
            // đoạn: a trên, b phải-trên, c phải-dưới, d dưới, e trái-dưới, f trái-trên, g giữa (x0,y0,x1,y1; y hướng lên)
            int[][] seg =
            {
                new[] { 12, 68, 36, 76 }, new[] { 36, 44, 44, 68 }, new[] { 36, 12, 44, 36 }, new[] { 12, 4, 36, 12 },
                new[] { 4, 12, 12, 36 }, new[] { 4, 44, 12, 68 }, new[] { 12, 36, 36, 44 },
            };
            string[] on = { "abcdef", "bc", "abged", "abgcd", "fgbc", "afgcd", "afgedc", "abc", "abcdefg", "abcdfg", "" };
            for (int g = 0; g < N; g++)
                for (int k = 0; k < 7; k++)
                {
                    bool lit = on[g].IndexOf((char)('a' + k)) >= 0;
                    byte v = lit ? (byte)255 : (byte)30;
                    var r = seg[k];
                    for (int y = r[1]; y < r[3]; y++)
                        for (int x = r[0]; x < r[2]; x++)
                        {
                            // vát góc đoạn cho giống LED
                            bool horiz = r[2] - r[0] > r[3] - r[1];
                            int inset = horiz ? Mathf.Min(x - r[0], r[2] - 1 - x) : Mathf.Min(y - r[1], r[3] - 1 - y);
                            int across = horiz ? Mathf.Abs(2 * y - (r[1] + r[3] - 1)) / 2 : Mathf.Abs(2 * x - (r[0] + r[2] - 1)) / 2;
                            if (across > inset + 1) continue;
                            px[y * W * N + g * W + x] = new Color32(v, v, v, 255);
                        }
                }
            tex.SetPixels32(px); tex.Apply();
            return SaveTexture("OW_SoDemNguoc", tex, false);
        }
        static Texture2D HaloTexture()
        {
            const int S = 64; var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(S / 2f, S / 2f)) / (S / 2f);
                    float f = Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f);
                    tex.SetPixel(x, y, new Color(f, f, f, f));
                }
            tex.Apply();
            return SaveTexture("OW_QuangDen", tex, true);
        }
        static Material DigitMat(string name, Texture2D atlas, Color emit)
        {
            var m = GlowMat(name, new Color(0.05f, 0.05f, 0.05f), emit, 1f);
            m.SetTexture("_MainTex", atlas); m.SetTexture("_EmissionMap", atlas); m.SetFloat("_Glossiness", 0.6f);
            EditorUtility.SetDirty(m); return m;
        }
        static Material HaloMat(string name, Texture2D halo, Color tint)
        {
            var path = Root + "Materials/" + name + ".mat";
            var sh = Shader.Find("Legacy Shaders/Particles/Additive");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
            m.shader = sh; m.SetTexture("_MainTex", halo); m.SetColor("_TintColor", new Color(tint.r, tint.g, tint.b, 0.8f));
            m.enableInstancing = true; EditorUtility.SetDirty(m); return m;
        }

        // Vật liệu phát sáng dùng chung (kính đèn tín hiệu, đèn pha/hậu/phanh) — lưu thành asset để bản build giữ biến thể shader.
        static Material GlowMat(string name, Color baseColor, Color emit, float intensity)
        {
            var path = Root + "Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m, path); }
            m.shader = Shader.Find("Standard");
            m.SetColor("_Color", baseColor); m.SetFloat("_Glossiness", 0.85f); m.SetFloat("_Metallic", 0f);
            if (intensity > 0) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emit * intensity); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; }
            else { m.DisableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Color.black); }
            m.enableInstancing = true; EditorUtility.SetDirty(m);
            return m;
        }
        const string LayoutPath = "ArtSource/Concepts/OpenWorld-20261117/layout.json";

        [MenuItem("Phố Việt/Bản đồ mở/4. Dựng lại giao thông NPC (scene đang mở)")]
        public static void RebuildTrafficMenu()
        {
            var car = GameObject.Find("XeNguoiChoi");
            if (!car) { Debug.LogError("[Bản đồ mở] mở scene PhoVietOpenWorld trước"); return; }
            BuildTraffic(car.transform);
            EditorSceneManager.MarkSceneDirty(car.scene); EditorSceneManager.SaveScene(car.scene);
        }

        static void BuildTraffic(Transform player)
        {
            var old = GameObject.Find("GiaoThong"); if (old) Object.DestroyImmediate(old);
            var lay = JsonUtility.FromJson<LayoutTable>(File.ReadAllText(LayoutPath));
            var defs = new List<OpenWorldTraffic.RoadDef>();
            foreach (var r in lay.roads)
                defs.Add(new OpenWorldTraffic.RoadDef { id = r.id, main = r.profile == "main", a = new Vector2(r.@from[0], r.@from[1]), b = new Vector2(r.to[0], r.to[1]) });
            var tr = new GameObject("GiaoThong").AddComponent<OpenWorldTraffic>();
            tr.roads = defs.ToArray(); tr.player = player;
            // Xe ô tô Meshy của game cũ (bỏ xe tăng: để dành cho hiểm họa ở phần 3).
            const string M = "Assets/PhoViet/Art/Cars/Meshy/PV_Meshy";
            var cars = new List<OpenWorldTraffic.CarModel>();
            // 21.260: khối lượng (tấn) cho luật va chạm theo cỡ xe.
            foreach (var (n, w, mass) in new[] { ("Sport", 3f, 1.4f), ("Pickup", 2f, 2.2f), ("Covered", 1.2f, 5f), ("Container", 0.6f, 12f) })
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(M + n + ".fbx");
                var mat = AssetDatabase.LoadAssetAtPath<Material>(M + n + ".mat");
                var lights = File.Exists(M + n + ".json") ? JsonUtility.FromJson<CarLights>(File.ReadAllText(M + n + ".json")) : null;
                if (model) cars.Add(new OpenWorldTraffic.CarModel { model = model, material = mat, weight = w, mass = mass,
                    headlight = V3(lights?.headlight), taillight = V3(lights?.taillight) });
            }
            tr.cars = cars.ToArray();
            // Xe máy + người lái Urban Rider (bộ hiện hành của game cũ), 5 bộ màu.
            tr.bikeModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/UrbanRider/PV_UrbanRider.fbx");
            var bm = new List<Material>();
            foreach (var n in new[] { "CityScooter", "HeroScooter", "Racer", "SportBike", "Underbone" })
            { var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/UrbanRider/PV_" + n + ".mat"); if (m) bm.Add(m); }
            tr.bikeMaterials = bm.ToArray();
            // 21.252: cột đèn giao thông có sẵn của game cũ (PV_TrafficSignal.fbx + atlas phố) và kính đèn 4 trạng thái.
            tr.signalModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/Streets/PV_TrafficSignal.fbx");
            tr.signalMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
            Color H(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
            tr.lensMaterials = new[]
            {
                GlowMat("OW_Den_Tat", H("2E3B39"), Color.black, 0f),
                // 21.253: màn hình không HDR → phát sáng > 1 mỗi kênh bị cháy thành trắng. Dùng màu bão hòa (kênh chính = 1,
                // kênh phụ thấp) và nền kính tối cùng tông để không bị ánh nắng làm nhạt; độ "chói" thêm bằng quầng sáng.
                GlowMat("OW_Den_Do", new Color(0.32f, 0.03f, 0.02f), new Color(1f, 0.10f, 0.05f), 1f),
                GlowMat("OW_Den_Vang", new Color(0.34f, 0.22f, 0.02f), new Color(1f, 0.70f, 0.04f), 1f),
                GlowMat("OW_Den_Xanh", new Color(0.02f, 0.28f, 0.14f), new Color(0.08f, 1f, 0.45f), 1f),
            };
            // 21.253: bảng LED đếm ngược (atlas 7 đoạn) + quầng sáng cộng màu trước kính.
            var digits = DigitAtlas();
            tr.countdownMaterials = new[]
            {
                DigitMat("OW_SoDem_Do", digits, new Color(1f, 0.10f, 0.05f)),
                DigitMat("OW_SoDem_Vang", digits, new Color(1f, 0.70f, 0.04f)),
                DigitMat("OW_SoDem_Xanh", digits, new Color(0.08f, 1f, 0.45f)),
            };
            var halo = HaloTexture();
            tr.haloMaterials = new[]
            {
                HaloMat("OW_QuangDen_Do", halo, new Color(1f, 0.12f, 0.06f)),
                HaloMat("OW_QuangDen_Vang", halo, new Color(1f, 0.65f, 0.05f)),
                HaloMat("OW_QuangDen_Xanh", halo, new Color(0.08f, 1f, 0.42f)),
            };
            tr.headMaterial = GlowMat("OW_DenPha", H("FFF4DC"), H("FFE9B8"), 1.4f);
            tr.tailMaterial = GlowMat("OW_DenHau", new Color(0.3f, 0.03f, 0.02f), new Color(0.55f, 0.04f, 0.02f), 1f);
            tr.brakeMaterial = GlowMat("OW_DenPhanh", new Color(0.4f, 0.03f, 0.02f), new Color(1f, 0.07f, 0.03f), 1f);   // 21.253: bão hòa, không cháy trắng
            AssetDatabase.SaveAssets();
            Debug.Log("[Bản đồ mở] giao thông: " + defs.Count + " tuyến, " + cars.Count + " mẫu ô tô, " + bm.Count + " màu xe máy");
        }

        // ------------------------------------------------------------ CSGT truy đuổi (21.254, đợt 7 phần 2)
        [MenuItem("Phố Việt/Bản đồ mở/5. Dựng lại CSGT (scene đang mở)")]
        public static void RebuildPoliceMenu()
        {
            var car = GameObject.Find("XeNguoiChoi");
            if (!car) { Debug.LogError("[Bản đồ mở] mở scene PhoVietOpenWorld trước"); return; }
            BuildPolice(car.transform);
            EditorSceneManager.MarkSceneDirty(car.scene); EditorSceneManager.SaveScene(car.scene);
        }

        // Xe Meshy "Vietnam Traffic Police Cruiser" + mặt sơn chữ CẢNH SÁT GIAO THÔNG của game cũ (21.184–21.185),
        // thu đều cho dài bằng xe người chơi (PhoVietGame.PoliceCruiserLength = 3,74 m). Vị trí thật đặt lúc chạy.
        static void BuildPolice(Transform player)
        {
            var old = GameObject.Find("CSGT"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("CSGT");
            var f = player.forward; f.y = 0; f.Normalize();
            root.transform.SetPositionAndRotation(player.position - f * 38f, Quaternion.LookRotation(f));
            var src = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MeshyImports/Vietnam_Traffic_Police_Cruiser_20261110_230018/Meshy_AI_Vietnam_Traffic_Police_Cruiser_20261110_texture.fbx");
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/PoliceCruiser/PV_PoliceCruiser.mat");
            Vector3 size = new Vector3(1.55f, 1.36f, PhoViet.PhoVietGame.PoliceCruiserLength);
            if (src)
            {
                var vis = (GameObject)PrefabUtility.InstantiatePrefab(src);
                vis.name = "Vietnam Traffic Police Cruiser";
                var t = vis.transform; t.SetParent(root.transform, false);
                t.localRotation = Quaternion.Euler(0, -90, 0) * t.localRotation; t.localPosition = Vector3.zero;
                var mf = vis.GetComponentInChildren<MeshFilter>(true); var b = mf.sharedMesh.bounds;
                Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
                for (int i = 0; i < 8; i++)
                {
                    var c = root.transform.InverseTransformPoint(mf.transform.TransformPoint(b.center + Vector3.Scale(b.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
                    min = Vector3.Min(min, c); max = Vector3.Max(max, c);
                }
                float k = PhoViet.PhoVietGame.PoliceCruiserLength / (max.z - min.z);
                t.localScale *= k;
                t.localPosition = new Vector3(-(min.x + max.x) * 0.5f, -min.y, -(min.z + max.z) * 0.5f) * k;
                size = (max - min) * k;
                foreach (var r in vis.GetComponentsInChildren<Renderer>(true)) { if (mat) r.sharedMaterial = mat; }
            }
            else Debug.LogWarning("[Bản đồ mở] thiếu FBX xe CSGT Meshy");
            var bc = root.AddComponent<BoxCollider>();
            // 21.274: đáy hộp va chạm = đáy bánh (trước đây cao hơn 0,12 m → xe CSGT lún 12 cm xuống mặt đường)
            bc.center = new Vector3(0, size.y / 2, 0); bc.size = new Vector3(size.x, size.y, size.z);
            root.AddComponent<Rigidbody>();
            var pol = root.AddComponent<OpenWorldPolice>();
            pol.player = player.GetComponent<OpenWorldDriver>();
            pol.traffic = Object.FindAnyObjectByType<OpenWorldTraffic>();
            pol.halfLength = size.z / 2; pol.halfWidth = size.x / 2;

            // Đèn ưu tiên: dàn đèn trên nóc (đỏ trái, xanh phải) + quầng sáng chéo chữ thập nhìn được mọi phía.
            pol.beaconOff = GlowMat("OW_CSGT_DenTat", new Color(0.55f, 0.58f, 0.62f), Color.black, 0f);
            pol.beaconRed = GlowMat("OW_CSGT_DenDo", new Color(0.4f, 0.03f, 0.02f), new Color(1f, 0.08f, 0.05f), 1f);
            pol.beaconBlue = GlowMat("OW_CSGT_DenXanh", new Color(0.02f, 0.08f, 0.4f), new Color(0.1f, 0.35f, 1f), 1f);
            var haloTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Textures/OW_QuangDen.png");
            if (!haloTex) haloTex = HaloTexture();
            var haloRed = HaloMat("OW_QuangDen_Do", haloTex, new Color(1f, 0.12f, 0.06f));
            var haloBlue = HaloMat("OW_QuangDen_XanhDuong", haloTex, new Color(0.15f, 0.4f, 1f));
            var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            float roof = size.y - 0.02f;
            var reds = new List<Renderer>(); var blues = new List<Renderer>(); var rh = new List<Renderer>(); var bh = new List<Renderer>();
            var bar = new GameObject("DanDenUuTien"); bar.transform.SetParent(root.transform, false);
            bar.transform.localPosition = new Vector3(0, roof, -0.12f);
            Part("DeDanDen", bar.transform, cube, pol.beaconOff, new Vector3(0, 0.02f, 0), new Vector3(0.9f, 0.05f, 0.24f));
            foreach (var (side, list, halos, hm) in new[] { (-1f, reds, rh, haloRed), (1f, blues, bh, haloBlue) })
            {
                list.Add(Part(side < 0 ? "DenDo" : "DenXanh", bar.transform, cube, pol.beaconOff, new Vector3(side * 0.22f, 0.1f, 0), new Vector3(0.4f, 0.12f, 0.22f)));
                for (int a = 0; a < 2; a++)
                {
                    var h = Part("QuangSang", bar.transform, quad, hm, new Vector3(side * 0.22f, 0.12f, 0), new Vector3(1.6f, 1.1f, 1f));
                    h.transform.localRotation = Quaternion.Euler(0, a * 90f, 0); h.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    h.enabled = false; halos.Add(h);
                }
            }
            pol.redBeacons = reds.ToArray(); pol.blueBeacons = blues.ToArray(); pol.redHalos = rh.ToArray(); pol.blueHalos = bh.ToArray();
            // Đèn pha/hậu theo vị trí lắp của game cũ (InstallPoliceCruiser21184.FitLamps, đã nhân 0,85).
            var head = GlowMat("OW_DenPha", new Color(1f, 0.957f, 0.863f), new Color(1f, 0.914f, 0.722f), 1.4f);
            var tail = GlowMat("OW_DenHau", new Color(0.3f, 0.03f, 0.02f), new Color(0.55f, 0.04f, 0.02f), 1f);
            float hz = size.z / 2;
            foreach (float x in new[] { -0.58f, 0.58f })
            {
                var hd = Part("DenPha", root.transform, quad, head, new Vector3(x, 0.71f, hz + 0.01f), new Vector3(0.26f, 0.12f, 1f));
                hd.transform.localRotation = Quaternion.Euler(0, 180, 0); hd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var tl = Part("DenHau", root.transform, quad, tail, new Vector3(x, 0.75f, -hz - 0.01f), new Vector3(0.24f, 0.1f, 1f));
                tl.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Bản đồ mở] CSGT: xe {size.z:F2}×{size.x:F2}×{size.y:F2} m");
        }

        // ------------------------------------------------------------ hiểm họa (21.255, đợt 7 phần 3)
        [MenuItem("Phố Việt/Bản đồ mở/6. Dựng lại hiểm họa (scene đang mở)")]
        public static void RebuildHazardsMenu()
        {
            var car = GameObject.Find("XeNguoiChoi");
            if (!car) { Debug.LogError("[Bản đồ mở] mở scene PhoVietOpenWorld trước"); return; }
            BuildHazards(car.transform);
            EditorSceneManager.MarkSceneDirty(car.scene); EditorSceneManager.SaveScene(car.scene);
        }

        // Dùng lại nguyên asset game cũ: khủng long Emerald Tyrant (prefab + DinosaurVisual), xe tăng Iron Sentinel
        // (PV_MeshyTank), đạn/vòng báo/vụ nổ/tia lửa/hố của Art/TankFx.
        static void BuildHazards(Transform player)
        {
            var old = GameObject.Find("HiemHoa"); if (old) Object.DestroyImmediate(old);
            var hz = new GameObject("HiemHoa").AddComponent<OpenWorldHazards>();
            var drv = player.GetComponent<OpenWorldDriver>();
            hz.player = drv;
            hz.traffic = Object.FindAnyObjectByType<OpenWorldTraffic>();
            hz.police = Object.FindAnyObjectByType<OpenWorldPolice>();
            hz.dinosaurPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/Dinosaur/PV_EmeraldTyrant.prefab");
            hz.tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/Cars/Meshy/PV_MeshyTank.fbx");
            hz.tankMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Cars/Meshy/PV_MeshyTank.mat");
            const string F = "Assets/PhoViet/Art/TankFx/";
            hz.shellModel = AssetDatabase.LoadAssetAtPath<GameObject>(F + "PV_TankShell.fbx");
            hz.warningMaterial = AssetDatabase.LoadAssetAtPath<Material>(F + "PV_TankWarning.mat");
            hz.explosionMaterial = AssetDatabase.LoadAssetAtPath<Material>(F + "PV_TankExplosion.mat");
            hz.muzzleMaterial = AssetDatabase.LoadAssetAtPath<Material>(F + "PV_TankMuzzle.mat");
            hz.sparkMaterial = AssetDatabase.LoadAssetAtPath<Material>(F + "PV_TankSpark.mat");
            hz.craterMaterial = AssetDatabase.LoadAssetAtPath<Material>(F + "PV_TankCrater.mat");
            // 21.256: trực thăng + tên lửa của game cũ (Resources/Helicopter).
            hz.helicopterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Resources/Helicopter/PV_Helicopter.prefab");
            hz.missilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Resources/Helicopter/PV_HelicopterMissile.prefab");
            if (drv) { drv.smokeMaterial = hz.explosionMaterial; EditorUtility.SetDirty(drv); }
            // 21.258: ô nạp tên lửa của game cũ + khói trắng phản lực khi nhảy (4 vòi, texture Blender PV_WhiteBoostSmoke).
            hz.pickupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Resources/PlayerMissile/PV_MissilePickup.prefab");
            // 21.259: khói xe NPC bị húc + sao choáng trên nóc xe người chơi + bật Read/Write để móp thân 4 mẫu ô tô Meshy.
            if (hz.traffic) { hz.traffic.smokeMaterial = hz.explosionMaterial; EditorUtility.SetDirty(hz.traffic); }
            if (drv) { drv.stunMaterial = StunStarMaterial(); EditorUtility.SetDirty(drv); }
            MakeMeshyReadable();
            var loop = Object.FindAnyObjectByType<OpenWorldGame>(); if (loop) { loop.hazards = hz; EditorUtility.SetDirty(loop); }   // dựng lại hiểm họa thì nối lại vòng chơi
            if (drv)
            {
                var jets = drv.GetComponent<OpenWorldJumpJets>(); if (!jets) jets = drv.gameObject.AddComponent<OpenWorldJumpJets>();
                jets.driver = drv;
                var matPath = Root + "Materials/OW_KhoiNhay.mat";
                var sm = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (!sm) { sm = new Material(Shader.Find("PhoViet/Soft Particle")); AssetDatabase.CreateAsset(sm, matPath); }
                sm.shader = Shader.Find("PhoViet/Soft Particle");
                sm.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Resources/Boost/PV_WhiteBoostSmoke.png");
                EditorUtility.SetDirty(sm);
                jets.smokeMaterial = sm;
                var bc = drv.GetComponent<BoxCollider>();
                if (bc) { jets.nozzleHalf = new Vector2(bc.size.x * 0.38f, bc.size.z * 0.38f); jets.nozzleHeight = Mathf.Max(0.2f, bc.center.y - bc.size.y * 0.5f + 0.15f); }
                EditorUtility.SetDirty(jets);
                AssetDatabase.SaveAssets();
            }
            Debug.Log("[Bản đồ mở] hiểm họa: khủng long=" + (hz.dinosaurPrefab != null) + " xe tăng=" + (hz.tankModel != null) + " đạn=" + (hz.shellModel != null) + " hố=" + (hz.craterMaterial != null) + " trực thăng=" + (hz.helicopterPrefab != null) + " tên lửa=" + (hz.missilePrefab != null) + " ô nạp=" + (hz.pickupPrefab != null));
        }

        // ------------------------------------------------------------ 21.261: va chạm hàng rào / lan can bờ sông / nhà nổi bật
        // • Hàng rào sắt trước cửa hàng (lưới gộp OW_Duong_*_HangRao_*): đo từng tấm (khe ≥ 0,12 m dọc trục) → OpenWorldFences
        //   (vùng chạm + húc đổ lúc chạy; không thêm lưới vào asset).
        // • Lan can + trụ bờ sông (OW_Map_LanCanSong_*): hộp va chạm tĩnh theo hướng chính của lưới, cao 1,5 m — không húc đổ.
        // • Nhà nổi bật / nhà thuốc / chân tháp trong OW_Map_showcase chưa có va chạm: hộp theo dấu chân tầng trệt (đỉnh tầng trệt
        //   0,3–2,6 m để bỏ mái hiên nhô ra vỉa hè), cao tới đỉnh nhà (tối đa 40 m).
        [MenuItem("Phố Việt/Bản đồ mở/8. Va chạm hàng rào, lan can, nhà nổi bật (scene đang mở)")]
        public static void BuildBarriersMenu()
        {
            var map = GameObject.Find("BanDoMo");
            if (!map) { Debug.LogError("[Bản đồ mở] mở scene PhoVietOpenWorld trước"); return; }
            var old = GameObject.Find("VaCham_RaoLanCan"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("VaCham_RaoLanCan");
            var fences = root.AddComponent<OpenWorldFences>();
            fences.traffic = Object.FindAnyObjectByType<OpenWorldTraffic>();
            fences.hazards = Object.FindAnyObjectByType<OpenWorldHazards>();
            fences.player = Object.FindAnyObjectByType<OpenWorldDriver>();
            fences.police = Object.FindAnyObjectByType<OpenWorldPolice>();
            var rails = new GameObject("LanCanSong").transform; rails.SetParent(root.transform, false);
            var houses = new GameObject("NhaNoiBat").transform; houses.SetParent(root.transform, false);
            int nFence = 0, nSeg = 0, nRail = 0, nHouse = 0;
            foreach (var mf in map.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh = mf.sharedMesh; if (!mesh) continue;
                string n = mesh.name;
                bool fence = n.Contains("_HangRao_"), rail = n.Contains("LanCanSong");
                bool house = !fence && !rail && mf.transform.parent && mf.transform.parent.parent && mf.transform.parent.parent.name == "OW_Map_showcase"
                             && (mf.name.StartsWith("OW_NhaPho_") || mf.name.StartsWith("OW_NhaThuoc")) || mf.name == "OW_ThapTaiChinh";
                if (!fence && !rail && !house) continue;
                var w = WorldVerts(mf);
                if (w.Count == 0) continue;
                if (house)
                {
                    // dấu chân tầng trệt (đỉnh < 2,6 m) để bỏ mái hiên/ban công nhô ra; nhà chỉ dựng mặt tiền (dấu chân mỏng < 1 m
                    // theo một trục) thì kéo hộp từ mặt tiền về phía sau tới mép xa của cả nhà.
                    float x0 = float.MaxValue, x1 = float.MinValue, z0 = float.MaxValue, z1 = float.MinValue, top = 0f;
                    float X0 = float.MaxValue, X1 = float.MinValue, Z0 = float.MaxValue, Z1 = float.MinValue;
                    foreach (var v in w)
                    {
                        top = Mathf.Max(top, v.y);
                        X0 = Mathf.Min(X0, v.x); X1 = Mathf.Max(X1, v.x); Z0 = Mathf.Min(Z0, v.z); Z1 = Mathf.Max(Z1, v.z);
                        if (v.y > 2.6f) continue;
                        x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); z0 = Mathf.Min(z0, v.z); z1 = Mathf.Max(z1, v.z);
                    }
                    if (x0 > x1) { x0 = X0; x1 = X1; z0 = Z0; z1 = Z1; }
                    if (x1 - x0 < 1f) { float f = (x0 + x1) / 2; if (X1 - f > f - X0) { x0 = f; x1 = X1; } else { x0 = X0; x1 = f; } }
                    if (z1 - z0 < 1f) { float f = (z0 + z1) / 2; if (Z1 - f > f - Z0) { z0 = f; z1 = Z1; } else { z0 = Z0; z1 = f; } }
                    top = Mathf.Min(top, 40f);
                    var g = new GameObject("VaCham_" + mf.name); g.transform.SetParent(houses, false); g.isStatic = true;
                    var bc = g.AddComponent<BoxCollider>();
                    bc.center = new Vector3((x0 + x1) / 2, top / 2, (z0 + z1) / 2); bc.size = new Vector3(x1 - x0, top, z1 - z0);
                    nHouse++; continue;
                }
                // hướng chính (PCA trên mặt phẳng ngang)
                double mx = 0, mz = 0; foreach (var v in w) { mx += v.x; mz += v.z; } mx /= w.Count; mz /= w.Count;
                double xx = 0, xz = 0, zz = 0; foreach (var v in w) { double dx = v.x - mx, dz = v.z - mz; xx += dx * dx; xz += dx * dz; zz += dz * dz; }
                float ang = 0.5f * Mathf.Atan2((float)(2 * xz), (float)(xx - zz));
                var dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)); var perp = new Vector3(dir.z, 0, -dir.x);
                var origin = new Vector3((float)mx, 0, (float)mz);
                var ts = new List<float>(w.Count); float l0 = float.MaxValue, l1 = float.MinValue, y0 = float.MaxValue, y1 = float.MinValue;
                foreach (var v in w)
                {
                    var d = v - origin; ts.Add(Vector3.Dot(d, dir));
                    float l = d.x * perp.x + d.z * perp.z; l0 = Mathf.Min(l0, l); l1 = Mathf.Max(l1, l);
                    y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
                }
                ts.Sort();
                if (rail)
                {
                    var g = new GameObject("VaCham_" + n); g.transform.SetParent(rails, false); g.isStatic = true;
                    float a0 = ts[0], a1 = ts[ts.Count - 1];
                    g.transform.SetPositionAndRotation(origin + dir * ((a0 + a1) / 2) + perp * ((l0 + l1) / 2), Quaternion.LookRotation(dir));
                    var bc = g.AddComponent<BoxCollider>();
                    bc.center = new Vector3(0, 0.75f, 0); bc.size = new Vector3(Mathf.Max(0.35f, l1 - l0), 1.5f, a1 - a0);
                    nRail++; continue;
                }
                var segs = new List<Vector2>(); float s0 = ts[0], prev = ts[0];
                for (int i = 1; i < ts.Count; i++)
                {
                    if (ts[i] - prev > 0.12f) { segs.Add(new Vector2(s0, prev)); s0 = ts[i]; }
                    prev = ts[i];
                }
                segs.Add(new Vector2(s0, prev));
                fences.runs.Add(new OpenWorldFences.Run
                {
                    renderer = mf.GetComponent<MeshRenderer>(), origin = origin, dir = dir, lateral = (l0 + l1) / 2,
                    yMin = y0, yMax = y1, segs = segs.ToArray()
                });
                nFence++; nSeg += segs.Count;
            }
            EditorUtility.SetDirty(fences);
            EditorSceneManager.MarkSceneDirty(map.scene); EditorSceneManager.SaveScene(map.scene);
            Debug.Log($"[Bản đồ mở] 21.261: hàng rào {nFence} đoạn / {nSeg} tấm, lan can sông {nRail} hộp, nhà nổi bật {nHouse} hộp");
        }

        static List<Vector3> WorldVerts(MeshFilter mf)
        {
            var list = new List<Vector3>();
            var vs = mf.sharedMesh.vertices; var tf = mf.transform;
            foreach (var v in vs) list.Add(tf.TransformPoint(v));
            return list;
        }

        // ------------------------------------------------------------ vòng chơi / điểm / menu (21.257, đợt 7 phần 4)
        [MenuItem("Phố Việt/Bản đồ mở/7. Dựng lại vòng chơi (scene đang mở)")]
        public static void RebuildGameLoopMenu()
        {
            var car = GameObject.Find("XeNguoiChoi");
            if (!car) { Debug.LogError("[Bản đồ mở] mở scene PhoVietOpenWorld trước"); return; }
            BuildGameLoop(car.transform);
            EditorSceneManager.MarkSceneDirty(car.scene); EditorSceneManager.SaveScene(car.scene);
        }

        static void BuildGameLoop(Transform player)
        {
            var old = GameObject.Find("VongChoi"); if (old) Object.DestroyImmediate(old);
            var g = new GameObject("VongChoi").AddComponent<OpenWorldGame>();
            g.player = player.GetComponent<OpenWorldDriver>();
            g.police = Object.FindAnyObjectByType<OpenWorldPolice>();
            g.hazards = Object.FindAnyObjectByType<OpenWorldHazards>();
            g.traffic = Object.FindAnyObjectByType<OpenWorldTraffic>();
            g.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/PhoViet/Fonts/BeVietnamPro-SemiBold.ttf");
            g.jailWide = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Resources/Jail/JailWide.jpg");
            g.jailTall = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Resources/Jail/JailTall.jpg");
            g.missileIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Resources/PlayerMissile/PV_MissileHud.png");
            Debug.Log("[Bản đồ mở] vòng chơi: font=" + (g.font != null) + " cảnh sát=" + (g.police != null) + " hiểm họa=" + (g.hazards != null));
        }

        // Sao vàng 5 cánh viền mềm (vòng choáng) — texture tự vẽ + Particles/Alpha Blended.
        static Material StunStarMaterial()
        {
            const int S = 64; var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++) { float a = Mathf.PI / 2 + i * Mathf.PI / 5, r = i % 2 == 0 ? 0.47f : 0.2f; pts[i] = new Vector2(0.5f + Mathf.Cos(a) * r, 0.5f + Mathf.Sin(a) * r); }
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float best = 0f;
                    for (int sy = 0; sy < 3; sy++) for (int sx = 0; sx < 3; sx++)
                        {
                            var q = new Vector2((x + (sx + .5f) / 3f) / S, (y + (sy + .5f) / 3f) / S); bool inside = false;
                            for (int i = 0, j = 9; i < 10; j = i++)
                                if ((pts[i].y > q.y) != (pts[j].y > q.y) && q.x < (pts[j].x - pts[i].x) * (q.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x) inside = !inside;
                            if (inside) best += 1f / 9f;
                        }
                    float d = Vector2.Distance(new Vector2((x + .5f) / S, (y + .5f) / S), new Vector2(.5f, .5f));
                    var c = Color.Lerp(new Color(1f, .62f, .1f), new Color(1f, .98f, .55f), Mathf.Clamp01(1f - d * 2.4f));
                    tex.SetPixel(x, y, new Color(c.r, c.g, c.b, best));
                }
            tex.Apply();
            var t = SaveTexture("OW_SaoChoang", tex, true);
            var path = Root + "Materials/OW_SaoChoang.mat";
            var sh = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
            m.shader = sh; m.SetTexture("_MainTex", t); m.SetColor("_TintColor", new Color(.5f, .5f, .5f, .5f));
            m.enableInstancing = true; EditorUtility.SetDirty(m); AssetDatabase.SaveAssets();
            return m;
        }

        static void MakeMeshyReadable()
        {
            foreach (var n in new[] { "Sport", "Pickup", "Covered", "Container" })
            {
                var imp = AssetImporter.GetAtPath("Assets/PhoViet/Art/Cars/Meshy/PV_Meshy" + n + ".fbx") as ModelImporter;
                if (imp && !imp.isReadable) { imp.isReadable = true; imp.SaveAndReimport(); }
            }
        }

        static Renderer Part(string name, Transform parent, Mesh mesh, Material m, Vector3 pos, Vector3 scale)
        {
            var g = new GameObject(name); g.transform.SetParent(parent, false);
            g.transform.localPosition = pos; g.transform.localScale = scale;
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = m; return r;
        }

        // ------------------------------------------------------------ xe người chơi
        static GameObject SpawnPlayer(Spawn s)
        {
            var car = new GameObject("XeNguoiChoi");
            var p = s != null && s.p != null && s.p.Length == 3 ? new Vector3(s.p[0], s.p[1] + 0.4f, s.p[2]) : new Vector3(218.5f, 0.7f, -400f);
            car.transform.position = p; car.transform.rotation = Quaternion.Euler(0, s != null ? s.yaw : 0, 0);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/PlayerCar/PV_BlueSupercar.fbx");
            if (model)
            {
                var v = (GameObject)PrefabUtility.InstantiatePrefab(model);
                v.transform.SetParent(car.transform, false); v.transform.localScale = Vector3.one * 0.85f;
                var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/PlayerCar/PV_BlueSupercar.mat");
                foreach (var r in v.GetComponentsInChildren<Renderer>()) { if (mat) r.sharedMaterial = mat; }
                // Chỉ giữ LOD0 (_Body) cho xe người chơi.
                foreach (var r in v.GetComponentsInChildren<Renderer>()) if (r.name.EndsWith("LOD1") || r.name.EndsWith("LOD2")) r.gameObject.SetActive(false);
                var b = new Bounds(); bool any = false;
                foreach (var r in v.GetComponentsInChildren<Renderer>()) { if (!r.gameObject.activeSelf) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
                var bc = car.AddComponent<BoxCollider>();
                bc.center = car.transform.InverseTransformPoint(b.center) + new Vector3(0, 0.05f, 0);
                bc.size = new Vector3(Mathf.Max(1.6f, b.size.x), Mathf.Max(1.0f, b.size.y - 0.1f), Mathf.Max(3.6f, b.size.z));
            }
            else car.AddComponent<BoxCollider>().size = new Vector3(1.9f, 1.2f, 4.4f);
            car.AddComponent<Rigidbody>();
            car.AddComponent<OpenWorldDriver>();
            return car;
        }

        // 21.250: nhà phố ở xa = một khối hộp dùng CHUNG lưới hộp đơn vị + vật liệu mặt tiền của chính căn đó
        // → GPU instancing gộp hàng nghìn căn xa thành vài lần vẽ. LOD0 (đầy đủ chi tiết) chỉ khi nhà đủ lớn trên màn hình.
        static Mesh unitCube;
        static Mesh UnitCube()
        {
            if (unitCube) return unitCube;
            var path = Root + "Materials/OW_HopDonVi.asset";
            unitCube = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (unitCube) return unitCube;
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            unitCube = Object.Instantiate(tmp.GetComponent<MeshFilter>().sharedMesh); unitCube.name = "OW_HopDonVi";
            Object.DestroyImmediate(tmp);
            AssetDatabase.CreateAsset(unitCube, path);
            return unitCube;
        }

        static void BuildingLOD(GameObject go, GameObject asset)
        {
            if (go.GetComponent<LODGroup>()) return;
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return;
            Material facade = null;
            foreach (var r in rs) foreach (var m in r.sharedMaterials) if (m && m.name.StartsWith("OW_Facade_")) { facade = m; break; }
            if (!facade) facade = rs[0].sharedMaterial;
            var b = LocalBounds(asset);
            float minX = Mathf.Max(b.min.x, 0f);                     // khối nhà (bỏ mái hiên/ban công nhô ra)
            var box = new GameObject("LOD1_Hop");
            box.transform.SetParent(go.transform, false);
            box.transform.localPosition = new Vector3((minX + b.max.x) / 2, b.center.y, b.center.z);
            box.transform.localScale = new Vector3(b.max.x - minX, b.size.y, b.size.z);
            box.AddComponent<MeshFilter>().sharedMesh = UnitCube();
            var mr = box.AddComponent<MeshRenderer>(); mr.sharedMaterial = facade;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            var lg = go.AddComponent<LODGroup>();
            lg.SetLODs(new[] { new LOD(0.22f, rs), new LOD(0.01f, new Renderer[] { mr }) });
            lg.RecalculateBounds();
        }

        static void CullLOD(GameObject go, float height)
        {
            if (go.GetComponent<LODGroup>()) return;
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return;
            var lg = go.AddComponent<LODGroup>(); lg.SetLODs(new[] { new LOD(height, rs) }); lg.RecalculateBounds();
        }

        static void SetStatic(GameObject go)
        {
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);   // 21.250: KHÔNG gộp tĩnh (nhân bản lưới ~7 GB); dùng GPU instancing
        }
    }
}
