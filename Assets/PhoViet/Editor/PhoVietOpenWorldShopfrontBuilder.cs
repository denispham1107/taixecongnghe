using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.OpenWorldEditor
{
    // 21.272 — menu 10: mặt tiền kính + nội thất tầng trệt nhà phố.
    // • 48 FBX nhà mẫu (Library/OW_Mau_NhaPho_*) đã được build_interiors.py (MODE export_houses) gỡ tường tầng trệt và gắn
    //   mặt tiền kính (vật liệu NT_KhungNhom/NT_Inox/NT_KinhMatTien/NT_LanhTo/NT_HopCuaCuon) → tạo vật liệu Unity + remap.
    // • Nội thất Library/NoiThat/OW_NoiThat_<w>.fbx: mỗi file 18 cặp NT_<loại> (3D, màu đỉnh) + NT_<loại>_Xa (ảnh xa trong atlas).
    // • Mỗi căn trong BanDoMo/NhaPho: căn _B lấy loại theo biển đặt cùng chỗ (BanDoMo/BangHieu, OW_Mau_Bang_<w>_<i>, i = SIGN_SET);
    //   căn không biển = 1 trong 4 kiểu nhà ở (băm theo vị trí). Thêm con «NoiThat» có LODGroup: 3D ≤ ~40 m, ảnh xa tới ~300 m.
    public static class PhoVietOpenWorldShopfrontBuilder
    {
        const string Root = "Assets/PhoViet/OpenWorld/";
        static readonly string[] Kinds = { "PhoBo", "ComTam", "BanhMi", "CaPhe", "TraSua", "TapHoa", "NhaThuoc", "SuaXe", "HuTieu", "TiemVang",
                                           "DienThoai", "BunBoHue", "ThoiTrang", "BachHoa", "NhaO_Salon", "NhaO_XeMay", "NhaO_PhongKhach", "NhaO_ViVong" };
        static string Strip(string n) => Regex.Replace(n, @"\.\d{3}$", "");

        static Material Mat(string name, string shader, string hex, System.Action<Material> setup = null)
        {
            string path = Root + "Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var sh = Shader.Find(shader);
            if (!m) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
            m.shader = sh;
            if (hex != null && ColorUtility.TryParseHtmlString(hex, out var c) && m.HasProperty("_Color")) m.color = c;
            m.enableInstancing = true;
            setup?.Invoke(m);
            EditorUtility.SetDirty(m);
            return m;
        }

        [MenuItem("Phố Việt/Bản đồ mở/10. Mặt tiền kính + nội thất tầng trệt (scene đang mở)")]
        public static void BuildMenu() => Debug.Log("[Bản đồ mở] " + Build());

        public static string Build()
        {
            var map = GameObject.Find("BanDoMo");
            if (!map) return "mở scene PhoVietOpenWorld trước";
            // 1) vật liệu
            var mats = new Dictionary<string, Material>();
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "Materials" }))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); if (m) mats[m.name] = m;
            }
            void Tri(string key, string hex, float gloss, float metal) =>
                mats[key] = Mat(key.Replace("NT_", "OW_"), "PhoViet/OpenWorld/Triplanar", hex, m => { m.mainTexture = null; m.SetFloat("_Glossiness", gloss); m.SetFloat("_Metallic", metal); });
            Tri("NT_KhungNhom", "#2F3438", 0.6f, 0.5f); Tri("NT_Inox", "#C9CED3", 0.75f, 0.9f);
            Tri("NT_LanhTo", "#E8DEC4", 0.1f, 0f); Tri("NT_HopCuaCuon", "#9AA0A6", 0.5f, 0.4f);
            mats["NT_KinhMatTien"] = Mat("OW_KinhMatTien", "PhoViet/OpenWorld/ShopGlass", null);
            var interior = Mat("OW_NoiThat", "PhoViet/OpenWorld/Interior", null);
            var interiorGlass = Mat("OW_NoiThatKinh", "PhoViet/OpenWorld/ShopGlass", null, m => { m.SetFloat("_MinAlpha", 0.18f); m.SetFloat("_MaxAlpha", 0.5f); m.SetFloat("_Reflect", 0.6f); });
            var far = Mat("OW_NoiThatXa", "Unlit/Texture", null, m => m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Library/NoiThat/OW_NoiThat_AnhXa.png"));
            AssetDatabase.SaveAssets();

            // 2) remap vật liệu 48 nhà mẫu (tên trong FBX mới có thể mang hậu tố .001)
            var atlas = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
            int remapped = 0;
            foreach (var guid in AssetDatabase.FindAssets("OW_Mau_NhaPho t:Model", new[] { Root + "Library" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = AssetImporter.GetAtPath(path) as ModelImporter; if (imp == null) continue;
                foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                {
                    if (!(o is Material mm)) continue;
                    var key = Strip(mm.name);
                    if (mats.TryGetValue(key, out var target)) { imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mm.name), target); remapped++; }
                }
                imp.SaveAndReimport();
            }
            // 3) nhập nội thất
            var interiorAssets = new Dictionary<string, GameObject>();
            var childIndex = new Dictionary<string, Dictionary<string, Transform>>();   // tên con đã bỏ hậu tố .001 của Blender
            foreach (var w in NtKeys)
            {
                string p = Root + "Library/NoiThat/OW_NoiThat_" + w + ".fbx";
                var imp = AssetImporter.GetAtPath(p) as ModelImporter; if (imp == null) return "thiếu " + p;
                if (imp.materialImportMode != ModelImporterMaterialImportMode.None || imp.importAnimation || imp.addCollider)
                { imp.materialImportMode = ModelImporterMaterialImportMode.None; imp.importAnimation = false; imp.importCameras = false; imp.importLights = false; imp.addCollider = false; imp.SaveAndReimport(); }
                interiorAssets[w] = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                var idx = new Dictionary<string, Transform>();
                foreach (Transform c in interiorAssets[w].transform) idx[Strip(c.name)] = c;
                childIndex[w] = idx;
            }
            var atlasTex = AssetImporter.GetAtPath(Root + "Library/NoiThat/OW_NoiThat_AnhXa.png") as TextureImporter;
            if (atlasTex && (atlasTex.maxTextureSize != 2048 || atlasTex.mipmapEnabled == false)) { atlasTex.maxTextureSize = 2048; atlasTex.mipmapEnabled = true; atlasTex.SaveAndReimport(); }

            // 4) biển hiệu theo vị trí
            var signs = new List<(Vector3 p, int i)>();
            var signRoot = map.transform.Find("BangHieu");
            if (signRoot)
                foreach (Transform s in signRoot)
                {
                    var src = PrefabUtility.GetCorrespondingObjectFromSource(s.gameObject); if (!src) continue;
                    var m = Regex.Match(src.name, @"OW_Mau_Bang_\d_\d_(\d+)$"); if (!m.Success) continue;
                    signs.Add((s.position, int.Parse(m.Groups[1].Value)));
                }
            var grid = new Dictionary<Vector2Int, List<(Vector3 p, int i)>>();
            foreach (var s in signs) { var k = new Vector2Int(Mathf.FloorToInt(s.p.x / 4), Mathf.FloorToInt(s.p.z / 4)); if (!grid.TryGetValue(k, out var l)) grid[k] = l = new List<(Vector3, int)>(); l.Add(s); }
            int SignAt(Vector3 p)
            {
                var k = new Vector2Int(Mathf.FloorToInt(p.x / 4), Mathf.FloorToInt(p.z / 4)); int best = -1; float bd = 0.8f;
                for (int dx = -1; dx <= 1; dx++) for (int dz = -1; dz <= 1; dz++)
                    if (grid.TryGetValue(new Vector2Int(k.x + dx, k.y + dz), out var l))
                        foreach (var s in l) { float d = Vector2.Distance(new Vector2(s.p.x, s.p.z), new Vector2(p.x, p.z)); if (d < bd) { bd = d; best = s.i; } }
                return best;
            }

            // 4b) gỡ chồng lấn ở góc phố (21.272 sửa): nhà hai đường vuông góc cùng chạy tới góc, thân sâu 16 m đâm xuyên
            //     tầng trệt của nhau (trước đây bị mặt tường đặc che, nay lộ qua kính).
            string corners = ResolveCorners(map) + "; " + ClearRooms(map);
            // 5) gắn nội thất từng căn
            var houses = map.transform.Find("NhaPho");
            int nShop = 0, nHome = 0, nMissingSign = 0, missing = 0, placed = 0;
            var counts = new int[Kinds.Length];
            var matrixCache = new Dictionary<string, Matrix4x4>();
            foreach (Transform h in houses)
            {
                var old = h.Find("NoiThat"); if (old) Object.DestroyImmediate(old.gameObject);
                var src = PrefabUtility.GetCorrespondingObjectFromSource(h.gameObject); if (!src) continue;
                var m = Regex.Match(src.name, @"OW_Mau_NhaPho_(\d_\d)_(\d)(_B)?$"); if (!m.Success) continue;
                string w = m.Groups[1].Value; bool slot = m.Groups[3].Success;
                int kind;
                if (slot) { int si = SignAt(h.position); if (si < 0) { nMissingSign++; kind = 14 + Hash(h.position) % 4; } else { kind = si; nShop++; } }
                else { kind = 14 + Hash(h.position) % 4; nHome++; }
                counts[kind]++;
                float wf = float.Parse(w.Replace('_', '.'), System.Globalization.CultureInfo.InvariantCulture);
                string nk = NtKey(wf); var S = Matrix4x4.Scale(new Vector3(1f, wf / NtWidth(nk), 1f));
                childIndex[nk].TryGetValue("NT_" + Kinds[kind], out var t3); childIndex[nk].TryGetValue("NT_" + Kinds[kind] + "_Xa", out var tx);
                if (!t3 || !tx) { missing++; continue; }
                var nt = new GameObject("NoiThat"); nt.transform.SetParent(h, false);
                Matrix4x4 inv = src.transform.localToWorldMatrix.inverse;
                var r3 = Child(nt.transform, "3D", t3, inv, interior, interiorGlass); SetLocal(r3.transform, S * (inv * t3.localToWorldMatrix));
                var rx = Child(nt.transform, "Xa", tx, inv, far, null); SetLocal(rx.transform, S * (inv * tx.localToWorldMatrix));
                var lg = nt.AddComponent<LODGroup>();
                lg.SetLODs(new[] { new LOD(0.17f, new Renderer[] { r3 }), new LOD(0.02f, new Renderer[] { rx }) });
                lg.RecalculateBounds(); placed++;
            }
            string featured = corners + " | " + BuildFeatured(map, childIndex, interior, interiorGlass, far);
            EditorSceneManager.MarkSceneDirty(map.scene); EditorSceneManager.SaveScene(map.scene);
            var sb = new System.Text.StringBuilder(); sb.Append(featured).Append(" | ");
            sb.Append("gắn ").Append(placed).Append(", thiếu mẫu ").Append(missing).Append("; remap ").Append(remapped).Append("; cửa hàng ").Append(nShop).Append(", nhà ở ").Append(nHome).Append(", nhà có chỗ biển nhưng không thấy biển ").Append(nMissingSign).Append(" |");
            for (int i = 0; i < Kinds.Length; i++) sb.Append(' ').Append(Kinds[i]).Append('=').Append(counts[i]);
            return sb.ToString();
        }

        // ------------------------------------------------------------ 14 nhà nổi bật theo ảnh mẫu (OW_NhaNoiBat_*_Goc)
        // Lưới riêng trong scene (không phải bản đặt của 48 mẫu): chép mesh, bỏ tam giác tường tầng trệt (mặt trước khối thân
        // x = 0,35 cục bộ), tấm kính đèn OW_ShopGlow*, đố cũ; gắn mặt tiền kính + nội thất lấy từ mẫu cùng bề ngang (giãn ngang
        // nếu bề ngang không có mẫu). Loại nội thất theo chữ biển.
        static readonly float[] LibW = { 4.5f, 5f, 5.5f, 6f, 7f, 8f };
        // 3 bộ nội thất (5 / 6 / 8 m); căn bề ngang khác co giãn theo chiều ngang (≤ ±12,5 %)
        static readonly string[] NtKeys = { "5_0", "6_0", "8_0" };
        static float NtWidth(string k) => float.Parse(k.Replace('_', '.'), System.Globalization.CultureInfo.InvariantCulture);
        static string NtKey(float w) { string best = NtKeys[0]; foreach (var k in NtKeys) if (Mathf.Abs(NtWidth(k) - w) < Mathf.Abs(NtWidth(best) - w)) best = k; return best; }
        static int KindFromSign(string t)
        {
            t = t.ToLowerInvariant();
            if (t.Contains("phở") || t.Contains("lẩu")) return 0; if (t.Contains("cơm tấm")) return 1; if (t.Contains("bánh mì")) return 2;
            if (t.Contains("cà phê")) return 3; if (t.Contains("trà sữa")) return 4; if (t.Contains("tạp hóa") || t.Contains("nhà sách")) return 5;
            if (t.Contains("thuốc")) return 6; if (t.Contains("sửa xe")) return 7; if (t.Contains("hủ tiếu") || t.Contains("ốc") || t.Contains("hải sản")) return 8;
            if (t.Contains("vàng")) return 9; if (t.Contains("điện")) return 10; if (t.Contains("bún")) return 11; if (t.Contains("thời trang")) return 12;
            if (t.Contains("bách hóa")) return 13; return 0;
        }

        static void SetLocal(Transform t, Matrix4x4 m)
        {
            t.localPosition = m.GetColumn(3);
            t.localRotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
            t.localScale = new Vector3(((Vector3)m.GetColumn(0)).magnitude, ((Vector3)m.GetColumn(1)).magnitude, ((Vector3)m.GetColumn(2)).magnitude);
        }

        static string BuildFeatured(GameObject map, Dictionary<string, Dictionary<string, Transform>> childIndex, Material interior, Material interiorGlass, Material far)
        {
            System.IO.Directory.CreateDirectory(Root + "Library/NhaNoiBat");
            // nội thất riêng 14 nhà nổi bật (đúng tên + bề ngang thật): Library/NoiThat/OW_NoiThat_NoiBat.fbx, con NT_F_<id> / NT_F_<id>_Xa
            const string fpath = Root + "Library/NoiThat/OW_NoiThat_NoiBat.fbx";
            var fimp = AssetImporter.GetAtPath(fpath) as ModelImporter;
            if (fimp != null && (fimp.materialImportMode != ModelImporterMaterialImportMode.None || fimp.addCollider))
            { fimp.materialImportMode = ModelImporterMaterialImportMode.None; fimp.importAnimation = false; fimp.addCollider = false; fimp.SaveAndReimport(); }
            var fasset = AssetDatabase.LoadAssetAtPath<GameObject>(fpath);
            var fidx = new Dictionary<string, Transform>();
            if (fasset) foreach (Transform c in fasset.transform) fidx[Strip(c.name)] = c;
            var farF = Mat("OW_NoiThatXaNoiBat", "Unlit/Texture", null, mm => mm.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Library/NoiThat/OW_NoiThat_NoiBat_AnhXa.png"));
            int done = 0, dropped = 0;
            var gocs = new List<Transform>();
            foreach (var t in map.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith("OW_NhaNoiBat_") && t.name.EndsWith("_Goc")) gocs.Add(t);
            foreach (var g in gocs)
            {
                Transform hm = null; string sign = "";
                foreach (Transform c in g) { if (c.name.StartsWith("OW_NhaPho_")) hm = c; if (c.name.StartsWith("OW_Bang_")) sign = Strip(c.name.Substring(8)); }
                if (!hm) continue;
                var mf = hm.GetComponent<MeshFilter>(); var mr = hm.GetComponent<MeshRenderer>();
                var srcMesh = mf.sharedMesh;
                float w = srcMesh.bounds.size.y;
                if (!srcMesh.name.EndsWith("_Kinh"))
                {
                    var m = Object.Instantiate(srcMesh); m.name = srcMesh.name + "_Kinh";
                    var v = m.vertices; var mats = mr.sharedMaterials;
                    for (int sub = 0; sub < m.subMeshCount && sub < mats.Length; sub++)
                    {
                        string mn = mats[sub] ? Strip(mats[sub].name) : "";
                        var tri = m.GetTriangles(sub); var keep = new List<int>(tri.Length);
                        for (int i = 0; i < tri.Length; i += 3)
                        {
                            Vector3 a = v[tri[i]], b = v[tri[i + 1]], c = v[tri[i + 2]];
                            float minz = Mathf.Min(a.z, Mathf.Min(b.z, c.z)), maxz = Mathf.Max(a.z, Mathf.Max(b.z, c.z));
                            bool kill = mn.StartsWith("OW_ShopGlow");
                            if (!kill && mn.StartsWith("OW_Facade") && Mathf.Abs(a.x - 0.35f) < 0.03f && Mathf.Abs(b.x - 0.35f) < 0.03f && Mathf.Abs(c.x - 0.35f) < 0.03f && minz < 0.3f && maxz > 3f) kill = true;
                            if (!kill && mn == "OW_FrameDark" && a.x > 0.3f && a.x < 0.5f && b.x > 0.3f && b.x < 0.5f && c.x > 0.3f && c.x < 0.5f && maxz < 3.6f) kill = true;
                            if (kill) { dropped++; continue; }
                            keep.Add(tri[i]); keep.Add(tri[i + 1]); keep.Add(tri[i + 2]);
                        }
                        m.SetTriangles(keep, sub);
                    }
                    string path = Root + "Library/NhaNoiBat/" + m.name + ".asset";
                    AssetDatabase.DeleteAsset(path); AssetDatabase.CreateAsset(m, path);
                    mf.sharedMesh = m;
                }
                // mẫu cùng/gần bề ngang
                float wl = LibW[0]; foreach (var x in LibW) if (Mathf.Abs(x - w) < Mathf.Abs(wl - w)) wl = x;
                string wk = wl.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture).Replace('.', '_');
                var S = Matrix4x4.Scale(new Vector3(1f, w / wl, 1f));
                var lib = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Library/OW_Mau_NhaPho_" + wk + "_0_B.fbx");
                var inv = lib.transform.localToWorldMatrix.inverse;
                foreach (var n in new[] { "MatTienKinh", "NoiThat" }) { var o = hm.Find(n); if (o) Object.DestroyImmediate(o.gameObject); }
                var sfSrc = lib.transform.Find("OW_MatTienKinh");
                if (sfSrc)
                {
                    var sf = new GameObject("MatTienKinh"); sf.transform.SetParent(hm, false);
                    SetLocal(sf.transform, S * (inv * sfSrc.localToWorldMatrix));
                    sf.AddComponent<MeshFilter>().sharedMesh = sfSrc.GetComponent<MeshFilter>().sharedMesh;
                    sf.AddComponent<MeshRenderer>().sharedMaterials = sfSrc.GetComponent<MeshRenderer>().sharedMaterials;
                }
                int kind = KindFromSign(sign);
                string nk = NtKey(w); var S2 = Matrix4x4.Scale(new Vector3(1f, w / NtWidth(nk), 1f));
                childIndex[nk].TryGetValue("NT_" + Kinds[kind], out var t3); childIndex[nk].TryGetValue("NT_" + Kinds[kind] + "_Xa", out var tx);
                string fid = hm.name.Replace("OW_NhaPho_", "");
                var farMat = far;
                if (fidx.TryGetValue("NT_F_" + fid, out var f3) && fidx.TryGetValue("NT_F_" + fid + "_Xa", out var fx)) { t3 = f3; tx = fx; S2 = Matrix4x4.identity; farMat = farF; }
                if (t3 && tx)
                {
                    var nt = new GameObject("NoiThat"); nt.transform.SetParent(hm, false);
                    var r3 = Child(nt.transform, "3D", t3, inv, interior, interiorGlass); SetLocal(r3.transform, S2 * (inv * t3.localToWorldMatrix));
                    var rx = Child(nt.transform, "Xa", tx, inv, farMat, null); SetLocal(rx.transform, S2 * (inv * tx.localToWorldMatrix));
                    var lg = nt.AddComponent<LODGroup>();
                    lg.SetLODs(new[] { new LOD(0.17f, new Renderer[] { r3 }), new LOD(0.02f, new Renderer[] { rx }) });
                    lg.RecalculateBounds();
                }
                done++;
            }
            return "nhà nổi bật " + done + " (bỏ " + dropped + " tam giác)";
        }

        // ------------------------------------------------------------ chồng lấn góc phố
        // Bước 1: căn nào có thân đè lên phòng tầng trệt (sâu 0,5–8,25 m) của căn khác → rút thân còn 8,6 m (mesh chép
        //         Library/NhaNgan/*_Ngan.asset: đỉnh sâu > 8,7 m kéo về 8,6 m; hộp LOD1 + hộp va chạm co theo).
        // Bước 2: còn xung đột (khối góc 8,6 × 8,6 m) → bỏ căn có nhiều xung đột nhất (kèm biển + va chạm) tới khi hết.
        const float ShortDepth = 8.6f;
        public static string ResolveCorners(GameObject map)
        {
            var root = map.transform.Find("NhaPho"); var hs = new List<Transform>(); foreach (Transform h in root) hs.Add(h);
            int N = hs.Count; var D = new float[N]; var W = new float[N]; var Hh = new float[N]; var alive = new bool[N];
            for (int i = 0; i < N; i++)
            {
                var m = hs[i].GetComponent<MeshFilter>().sharedMesh; var b = m.bounds;
                W[i] = b.size.y; Hh[i] = b.max.z; D[i] = m.name.EndsWith("_Ngan") ? ShortDepth : 16f; alive[i] = true;
            }
            var grid = new Dictionary<Vector2Int, List<int>>();
            Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x / 20), Mathf.FloorToInt(p.z / 20));
            for (int i = 0; i < N; i++) { var k = Cell(hs[i].position); if (!grid.TryGetValue(k, out var l)) grid[k] = l = new List<int>(); l.Add(i); }
            bool Intrudes(int j, int i)
            {
                var bj = new Bounds(new Vector3((0.35f + D[j]) / 2, 0, Hh[j] / 2), new Vector3(D[j] - 0.35f, W[j] - 0.06f, Hh[j]));
                for (float x = 0.5f; x < 8.25f; x += 0.25f)
                    for (float y = -W[i] / 2 + 0.3f; y <= W[i] / 2 - 0.3f + 1e-3f; y += (W[i] - 0.6f) / 6f)
                        if (bj.Contains(hs[j].InverseTransformPoint(hs[i].TransformPoint(new Vector3(x, y, 1.5f))))) return true;
                return false;
            }
            List<(int v, int j)> Conflicts()
            {
                var res = new List<(int, int)>();
                for (int i = 0; i < N; i++)
                {
                    if (!alive[i]) continue; var k0 = Cell(hs[i].position);
                    for (int dx = -1; dx <= 1; dx++) for (int dz = -1; dz <= 1; dz++)
                        if (grid.TryGetValue(new Vector2Int(k0.x + dx, k0.y + dz), out var l))
                            foreach (int j in l) if (j != i && alive[j] && Intrudes(j, i)) res.Add((i, j));
                }
                return res;
            }
            var c0 = Conflicts(); int start = c0.Count;
            var toShort = new HashSet<int>(); foreach (var c in c0) if (D[c.j] > 9f) { D[c.j] = ShortDepth; toShort.Add(c.j); }
            int removed = 0;
            while (true)
            {
                var cs = Conflicts(); if (cs.Count == 0 || removed > 800) break;
                var cnt = new Dictionary<int, int>();
                foreach (var c in cs) { cnt[c.v] = cnt.TryGetValue(c.v, out var a) ? a + 1 : 1; cnt[c.j] = cnt.TryGetValue(c.j, out var b2) ? b2 + 1 : 1; }
                int best = -1, bc = -1; foreach (var kv in cnt) if (kv.Value > bc || (kv.Value == bc && W[kv.Key] < W[best])) { best = kv.Key; bc = kv.Value; }
                alive[best] = false; removed++;
            }
            // áp vào scene
            System.IO.Directory.CreateDirectory(Root + "Library/NhaNgan");
            var colRoot = map.transform.Find("VaCham_NhaPho"); var signRoot = map.transform.Find("BangHieu");
            Transform Near(Transform parent, Vector3 p, float r) { if (!parent) return null; foreach (Transform c in parent) if ((c.position - p).sqrMagnitude < r * r) return c; return null; }
            var shortCache = new Dictionary<Mesh, Mesh>();
            int nShort = 0;
            for (int i = 0; i < N; i++)
            {
                var h = hs[i];
                if (!alive[i])
                {
                    var col = Near(colRoot, h.position, 0.05f); if (col) Object.DestroyImmediate(col.gameObject);
                    var sg = Near(signRoot, h.position, 0.8f); if (sg) Object.DestroyImmediate(sg.gameObject);
                    Object.DestroyImmediate(h.gameObject); continue;
                }
                if (!toShort.Contains(i)) continue;
                var mf = h.GetComponent<MeshFilter>(); var src = mf.sharedMesh; if (src.name.EndsWith("_Ngan")) continue;
                if (!shortCache.TryGetValue(src, out var sm))
                {
                    string path = Root + "Library/NhaNgan/" + src.name + "_Ngan.asset";
                    sm = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (!sm)
                    {
                        sm = Object.Instantiate(src); sm.name = src.name + "_Ngan";
                        var v = sm.vertices; for (int k = 0; k < v.Length; k++) if (v[k].x > ShortDepth + 0.1f) v[k].x = ShortDepth;
                        sm.vertices = v; sm.RecalculateBounds();
                        AssetDatabase.CreateAsset(sm, path);
                    }
                    shortCache[src] = sm;
                }
                mf.sharedMesh = sm; nShort++;
                var lod1 = h.Find("LOD1_Hop");
                if (lod1) { var lp = lod1.localPosition; var ls = lod1.localScale; float x0 = lp.x - ls.x / 2; lod1.localPosition = new Vector3((x0 + ShortDepth) / 2, lp.y, lp.z); lod1.localScale = new Vector3(ShortDepth - x0, ls.y, ls.z); }
                var vc = Near(colRoot, h.position, 0.05f);
                if (vc) { var bc = vc.GetComponent<BoxCollider>(); if (bc) { float x0 = bc.center.x - bc.size.x / 2; bc.center = new Vector3((x0 + ShortDepth) / 2, bc.center.y, bc.center.z); bc.size = new Vector3(ShortDepth - x0, bc.size.y, bc.size.z); } }
                var lg = h.GetComponent<LODGroup>(); if (lg) lg.RecalculateBounds();
            }
            AssetDatabase.SaveAssets();
            return "góc phố: " + start + " cặp chồng lấn → rút ngắn " + nShort + " căn, bỏ " + removed + " căn";
        }

        // Bắn tia vật lý từ sau kính vào sâu 7,4 m (5 vị trí ngang × 2 độ cao) mỗi căn: cây trồng lọt vào nhà → bỏ cây;
        // vật cản khác cách kính < 4,5 m (khối lõi, nhà nổi bật đè lên) → bỏ căn; ≥ 4,5 m coi như tường hậu lùi vào, giữ.
        public static string ClearRooms(GameObject map)
        {
            Physics.SyncTransforms();
            var colRoot = map.transform.Find("VaCham_NhaPho"); var signRoot = map.transform.Find("BangHieu");
            var treeRoot = map.transform.Find("Cay"); var treeCol = map.transform.Find("VaCham_Cay");
            Transform Near(Transform parent, Vector3 p, float r) { if (!parent) return null; foreach (Transform c in parent) if ((c.position - p).sqrMagnitude < r * r) return c; return null; }
            int trees = 0, houses = 0, kept = 0;
            for (int pass = 0; pass < 3; pass++)
            {
                var list = new List<Transform>(); foreach (Transform h in map.transform.Find("NhaPho")) list.Add(h);
                bool changed = false;
                foreach (var h in list)
                {
                    var b = h.GetComponent<MeshFilter>().sharedMesh.bounds; float w = b.size.y; RaycastHit first = default; bool hit = false;
                    foreach (float z in new[] { 0.9f, 2.2f })
                        for (int k = 0; k < 5; k++)
                        {
                            var o = h.TransformPoint(new Vector3(0.6f, -w / 2 + 0.45f + k * (w - 0.9f) / 4f, z)); var d = h.TransformDirection(Vector3.right);
                            if (Physics.Raycast(o, d, out var rh, 7.4f, ~0, QueryTriggerInteraction.Ignore) && (!hit || rh.distance < first.distance)) { first = rh; hit = true; }
                        }
                    if (!hit) continue;
                    var c = first.collider.transform;
                    if (c.parent && c.parent.name == "VaCham_Cay")
                    {
                        var tree = Near(treeRoot, c.position, 0.05f); if (tree) Object.DestroyImmediate(tree.gameObject);
                        Object.DestroyImmediate(c.gameObject); trees++; changed = true;
                    }
                    else if (first.distance < 4.5f)
                    {
                        var col = Near(colRoot, h.position, 0.05f); if (col) Object.DestroyImmediate(col.gameObject);
                        var sg = Near(signRoot, h.position, 0.8f); if (sg) Object.DestroyImmediate(sg.gameObject);
                        Object.DestroyImmediate(h.gameObject); houses++; changed = true;
                    }
                    else if (pass == 0) kept++;
                }
                if (!changed) break;
                Physics.SyncTransforms();
            }
            // nhà nổi bật (thân + nội thất không có va chạm riêng) đè lên phòng căn thường ở 4,5 m đầu → bỏ căn thường
            var feat = new List<Bounds>();
            foreach (var t in map.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith("OW_NhaNoiBat_") && t.name.EndsWith("_Goc"))
                    foreach (var r in t.GetComponentsInChildren<MeshRenderer>()) if (r.name == "3D" || r.name.StartsWith("OW_NhaPho_")) feat.Add(r.bounds);
            var all = new List<Transform>(); foreach (Transform h in map.transform.Find("NhaPho")) all.Add(h);
            foreach (var h in all)
            {
                var b = h.GetComponent<MeshFilter>().sharedMesh.bounds; float w = b.size.y; bool hit = false;
                for (float x = 0.7f; x < 4.5f && !hit; x += 0.3f)
                    for (int k = 0; k < 5 && !hit; k++)
                    {
                        var wp = h.TransformPoint(new Vector3(x, -w / 2 + 0.5f + k * (w - 1f) / 4f, 1.5f));
                        foreach (var fb in feat) if (fb.Contains(wp)) { hit = true; break; }
                    }
                if (!hit) continue;
                var col = Near(colRoot, h.position, 0.05f); if (col) Object.DestroyImmediate(col.gameObject);
                var sg = Near(signRoot, h.position, 0.8f); if (sg) Object.DestroyImmediate(sg.gameObject);
                Object.DestroyImmediate(h.gameObject); houses++;
            }
            return "trong phòng: bỏ " + trees + " cây lọt vào nhà, bỏ " + houses + " căn bị đè sát kính, giữ " + kept + " căn có vật cản ≥ 4,5 m (như tường hậu)";
        }

        static int Hash(Vector3 p) => Mathf.Abs(Mathf.RoundToInt(p.x * 7.31f) * 73856093 ^ Mathf.RoundToInt(p.z * 3.17f) * 19349663) & 0x7fffffff;

        static Renderer Child(Transform parent, string name, Transform srcChild, Matrix4x4 inv, Material m0, Material m1)
        {
            var g = new GameObject(name); g.transform.SetParent(parent, false);
            var mtx = inv * srcChild.localToWorldMatrix;
            g.transform.localPosition = mtx.GetColumn(3);
            g.transform.localRotation = mtx.rotation;
            g.transform.localScale = mtx.lossyScale;
            var mesh = srcChild.GetComponent<MeshFilter>().sharedMesh;
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterials = mesh.subMeshCount > 1 && m1 ? new[] { m0, m1 } : new[] { m0 };
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            return r;
        }
    }
}
