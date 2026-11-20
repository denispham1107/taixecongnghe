using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorldEditor
{
    // 21.290 — menu 15: chữ vẽ trên các mảng tường hông TRỐNG (nhà phố cuối dãy ở giao lộ, nhà rút ngắn, nhà nổi bật).
    // Người dùng (21/11/2026): chữ to rõ, canh giữa tường; tỉ lệ 25 % «Cù Lao Petshop - Tắm Làm Đẹp Thú Cưng 66K»,
    // 25 % «… Lưu Trú Thú Cưng 66K», 25 % «… Hỏa Táng Thú Cưng 300K», 25 % còn lại chia đều «CẤM ĐÁI BẬY», «AI XE ÔM KHÔNG»,
    // «CẤM ĐỔ RÁC», «HÚT BỒN CẦU ĐÂY»; màu chữ khác nhau, không trùng màu tường; tiếng Việt có dấu đúng font.
    // • Lưới chữ: Library/OW_ChuTuong.fbx (Blender build_wall_texts.py, Arial Bold, 2 slot: dòng chính / dòng nhấn).
    // • Dò tường: mỗi căn, hai mặt hông (±y cục bộ). Lưới ô 1 × 1 m trên mặt tường (sâu 0,4 m → hết thân, cao 0,6 m → đỉnh tường
    //   thật lấy từ đỉnh lưới sát mặt hông); ô «lộ» khi trước nó 2,5 m không có nhà/khối khác và điểm sát tường không nằm trong khối
    //   khác. Lấy hình chữ nhật lộ lớn nhất (thuật toán histogram), tối thiểu 5 × 3 m, giữa vùng phải thoáng ≥ 10 m (nhìn ra phố/giao lộ);
    //   chữ co giãn vừa (lề 0,6 m, dòng chính 1–2,4 m; tự chọn bố cục ngang 3 dòng hoặc dọc 3–5 dòng cho chữ to nhất)
    //   và đặt CHÍNH GIỮA hình chữ nhật đó, cách tường 3,5 cm.
    // • Màu: 2 màu khác nhau mỗi bảng từ bảng 10 màu đậm; loại màu tương phản kém (tỉ lệ độ sáng < 3) với màu tường căn đó.
    // • Gốc scene «ChuTuong» (ngoài BanDoMo → không đổi cờ occlusion; renderer động vẫn bị occlusion che). Chạy lại = dựng lại.
    public static class PhoVietOpenWorldWallTextBuilder
    {
        const string Fbx = "Assets/PhoViet/OpenWorld/Library/OW_ChuTuong.fbx";
        const string MatDir = "Assets/PhoViet/OpenWorld/Materials/";
        static readonly string[] Petshop = { "OW_ChuTuong_P1", "OW_ChuTuong_P2", "OW_ChuTuong_P3" };
        static readonly string[] Slogans = { "OW_ChuTuong_S1", "OW_ChuTuong_S2", "OW_ChuTuong_S3", "OW_ChuTuong_S4" };
        static readonly string[] Palette = { "#C62828", "#1565C0", "#2E7D32", "#6A1B9A", "#E65100", "#AD1457", "#00695C", "#283593", "#212121", "#8E24AA" };
        static readonly Dictionary<string, string> Walls = new Dictionary<string, string>
        { { "cream", "#D9CDB3" }, { "gray", "#B9B5AB" }, { "yellowish", "#D6C38E" }, { "offwhite", "#D4D0C6" }, { "olive", "#AAA48E" }, { "pinkish", "#CBB19F" } };

        [MenuItem("Phố Việt/Bản đồ mở/15. Chữ trên tường trống giao lộ (21.290, scene đang mở)")]
        public static void BuildMenu() => Debug.Log("[Bản đồ mở] " + Apply());

        static float Lum(Color c) { c = c.linear; return 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b; }
        static float Contrast(Color a, Color b) { float x = Lum(a), y = Lum(b); return (Mathf.Max(x, y) + 0.05f) / (Mathf.Min(x, y) + 0.05f); }

        static Material PaintMat(string hex)
        {
            string path = MatDir + "OW_ChuTuong_" + hex.TrimStart('#') + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("PhoViet/OpenWorld/Triplanar")); AssetDatabase.CreateAsset(m, path); }
            ColorUtility.TryParseHtmlString(hex, out var c);
            m.SetColor("_Color", c); m.SetFloat("_Glossiness", 0.15f); m.enableInstancing = true;
            EditorUtility.SetDirty(m); return m;
        }

        // đỉnh tường thật ở mặt hông (đỉnh lưới sát mặt hông, sâu > 1 m), cache theo lưới
        static readonly Dictionary<Mesh, float> topCache = new Dictionary<Mesh, float>();
        static float WallTop(Mesh m)
        {
            if (topCache.TryGetValue(m, out var t)) return t;
            var b = m.bounds; float side = b.extents.y; t = 0;
            foreach (var v in m.vertices)
                if (v.x > 1.5f && Mathf.Abs(Mathf.Abs(v.y - b.center.y) - side) < 0.05f) t = Mathf.Max(t, v.z);
            if (t <= 0) t = b.max.z - 1f;
            topCache[m] = t; return t;
        }

        static bool Ignored(Collider c)
        {
            if (!c || c.isTrigger) return true;
            for (var t = c.transform; t; t = t.parent)
            {
                string n = t.name;
                if (n == "VaCham_Cay" || n == "CotDen" || n == "VaCham_CotDen" || n.Contains("HangRao") || n.Contains("LanCan") || n == "ChuTuong") return true;
            }
            return false;
        }

        public static string Apply()
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(Fbx);
            if (imp.materialImportMode != ModelImporterMaterialImportMode.None || imp.addCollider)
            { imp.materialImportMode = ModelImporterMaterialImportMode.None; imp.addCollider = false; imp.importAnimation = false; imp.SaveAndReimport(); }
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
            var meshes = new Dictionary<string, (Mesh m, Quaternion rot)>();
            foreach (var mf in fbx.GetComponentsInChildren<MeshFilter>())
                meshes[mf.name] = (mf.sharedMesh, mf.transform.localRotation);
            var mats = new List<Material>(); foreach (var h in Palette) mats.Add(PaintMat(h));
            AssetDatabase.SaveAssets();

            var old = GameObject.Find("ChuTuong"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ChuTuong");
            Physics.SyncTransforms();

            var houses = new List<(Transform t, Mesh m, string fac)>();
            foreach (Transform h in GameObject.Find("BanDoMo/NhaPho").transform)
            {
                var mf = h.GetComponent<MeshFilter>(); var mr = h.GetComponent<MeshRenderer>(); if (!mf || !mr) continue;
                houses.Add((h, mf.sharedMesh, FacadeKey(mr)));
            }
            foreach (var t in GameObject.Find("BanDoMo").GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith("OW_NhaPho_") && t.parent && t.parent.name.StartsWith("OW_NhaNoiBat_"))
                    houses.Add((t, t.GetComponent<MeshFilter>().sharedMesh, FacadeKey(t.GetComponent<MeshRenderer>())));

            int walls = 0, placed = 0, narrow = 0; var count = new Dictionary<string, int>();
            foreach (var (h, mesh, fac) in houses)
            {
                var b = mesh.bounds; float half = b.extents.y, yc = b.center.y;
                float x0 = 0.45f, x1 = b.max.x - 0.3f, z0 = 0.6f, z1 = WallTop(mesh) - 0.2f;
                if (x1 - x0 < 4f || z1 - z0 < 3f) continue;
                int nx = Mathf.FloorToInt(x1 - x0), nz = Mathf.FloorToInt(z1 - z0);
                foreach (int s in new[] { 1, -1 })
                {
                    walls++;
                    var ok = new bool[nx, nz];
                    Vector3 outW = h.TransformDirection(new Vector3(0, s, 0)).normalized;
                    for (int i = 0; i < nx; i++)
                        for (int k = 0; k < nz; k++)
                        {
                            var p = h.TransformPoint(new Vector3(x0 + i + 0.5f, yc + s * (half + 0.08f), z0 + k + 0.5f));
                            bool blocked = false;
                            foreach (var c in Physics.OverlapSphere(p, 0.05f)) if (!Ignored(c)) { blocked = true; break; }
                            if (!blocked)
                                foreach (var hit in Physics.RaycastAll(p, outW, 2.5f)) if (!Ignored(hit.collider)) { blocked = true; break; }
                            ok[i, k] = !blocked;
                        }
                    if (!BestRect(ok, nx, nz, out int ri, out int rk, out int rw, out int rh)) continue;
                    if (rw < 5 || rh < 3) continue;
                    // chỉ tường nhìn ra khoảng trống (phố, giao lộ, trên mái nhà thấp): giữa vùng chữ thoáng ≥ 10 m
                    {
                        var pc = h.TransformPoint(new Vector3(x0 + ri + rw * 0.5f, yc + s * (half + 0.08f), z0 + rk + rh * 0.5f));
                        bool open = true;
                        foreach (var hit in Physics.RaycastAll(pc, outW, 10f)) if (!Ignored(hit.collider)) { open = false; break; }
                        if (!open) { narrow++; continue; }
                    }
                    // tường chỉ nhận chữ khi đủ chỗ cho câu dài nhất (P1, ngang hoặc dọc) với dòng chính ≥ 1 m → bốc nội dung
                    // ngẫu nhiên SAU bước lọc nên giữ đúng tỉ lệ 25/25/25/25
                    {
                        float fit = 0;
                        foreach (var kf in new[] { "OW_ChuTuong_P1", "OW_ChuTuong_P1N" })
                            if (meshes.TryGetValue(kf, out var cf)) fit = Mathf.Max(fit, Mathf.Min((rw - 1.2f) / cf.m.bounds.size.x, (rh - 1.2f) / cf.m.bounds.size.z));
                        if (fit < 1.0f) continue;
                    }
                    // nội dung theo tỉ lệ 25/25/25/25 (khẩu hiệu chia đều phần cuối), ngẫu nhiên ổn định theo vị trí tường
                    var pos0 = h.position; int seed = Mathf.RoundToInt(pos0.x * 13.7f) * 7349 ^ Mathf.RoundToInt(pos0.z * 7.3f) * 92821 ^ (s > 0 ? 0x5bd1 : 0x2c9f);
                    var rnd = new System.Random(seed);
                    double u = rnd.NextDouble();
                    string key = u < 0.25 ? Petshop[0] : u < 0.5 ? Petshop[1] : u < 0.75 ? Petshop[2] : Slogans[Mathf.Min(3, (int)((u - 0.75) / 0.0625))];
                    // hai bố cục: ngang (3 dòng) và dọc (key + "N", nhiều dòng ngắn) — chọn bố cục cho chữ to nhất trong vùng tường
                    float rectW = rw, rectH = rh, mainLine = key.Contains("_P") ? 1f : 1.2f;
                    float sc = -1; Mesh tmMesh = null;
                    foreach (var k2 in new[] { key, key + "N" })
                    {
                        if (!meshes.TryGetValue(k2, out var cand)) continue;
                        var tb = new Vector2(cand.m.bounds.size.x, cand.m.bounds.size.z);   // lưới chữ: rộng x, lên +z, mặt đọc −y
                        float s2 = Mathf.Min((rectW - 1.2f) / tb.x, (rectH - 1.2f) / tb.y, 2.4f / mainLine);   // dòng chính ≤ 2,4 m
                        if (s2 > sc) { sc = s2; tmMesh = cand.m; }
                    }
                    if (tmMesh == null || sc < 1.0f) continue;                       // chữ to rõ: dòng chính ≥ 1 m
                    var tm = (m: tmMesh, rot: Quaternion.identity);
                    // tâm hình chữ nhật trên mặt tường
                    var cl = new Vector3(x0 + ri + rw * 0.5f, yc + s * (half + 0.035f), z0 + rk + rh * 0.5f);
                    var cw = h.TransformPoint(cl);
                    var go = new GameObject(key.Replace("OW_ChuTuong_", "Chu_") + "_" + h.name);
                    go.transform.SetParent(root.transform, false);
                    go.transform.position = cw;
                    // lưới: +z = lên dòng chữ, −y = mặt đọc → +z ↦ lên thế giới, +y ↦ vào trong tường (−outW)
                    go.transform.rotation = Quaternion.LookRotation(Vector3.up, -outW);
                    go.transform.localScale = Vector3.one * sc;
                    go.AddComponent<MeshFilter>().sharedMesh = tm.m;
                    var r = go.AddComponent<MeshRenderer>();
                    Color wall = Walls.TryGetValue(fac, out var wh) && ColorUtility.TryParseHtmlString(wh, out var wc) ? wc : new Color(0.8f, 0.78f, 0.72f);
                    var okMats = new List<Material>();
                    foreach (var m in mats) if (Contrast(m.GetColor("_Color"), wall) >= 3f) okMats.Add(m);
                    var a = okMats[rnd.Next(okMats.Count)]; Material bm2; do bm2 = okMats[rnd.Next(okMats.Count)]; while (bm2 == a && okMats.Count > 1);
                    r.sharedMaterials = new[] { a, bm2 };
                    r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = true;
                    r.lightProbeUsage = LightProbeUsage.Off; r.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    var lg = go.AddComponent<LODGroup>(); lg.SetLODs(new[] { new LOD(0.012f, new Renderer[] { r }) }); lg.RecalculateBounds();
                    placed++; count[key] = count.TryGetValue(key, out var cc) ? cc + 1 : 1;
                }
            }
            EditorSceneManager.MarkSceneDirty(root.scene);
            var sb = new System.Text.StringBuilder();
            foreach (var kv in count) sb.Append(kv.Key.Replace("OW_ChuTuong_", "") + "=" + kv.Value + " ");
            return "chữ tường: " + placed + " bảng / " + walls + " mặt hông dò, bỏ " + narrow + " tường quay vào khe hẹp (" + sb + ")";
        }

        static string FacadeKey(MeshRenderer mr)
        {
            foreach (var m in mr.sharedMaterials) if (m && m.name.StartsWith("OW_Facade_")) return m.name.Substring(10);
            return "";
        }

        // hình chữ nhật ô true lớn nhất (theo diện tích) trong lưới ok[nx, nz]
        static bool BestRect(bool[,] ok, int nx, int nz, out int bi, out int bk, out int bw, out int bh)
        {
            bi = bk = bw = bh = 0; int best = 0; var hgt = new int[nx];
            for (int k = 0; k < nz; k++)
            {
                for (int i = 0; i < nx; i++) hgt[i] = ok[i, k] ? hgt[i] + 1 : 0;
                for (int i = 0; i < nx; i++)
                {
                    int hmin = int.MaxValue;
                    for (int j = i; j < nx && hgt[j] > 0; j++)
                    {
                        hmin = Mathf.Min(hmin, hgt[j]); int w = j - i + 1, area = w * hmin;
                        if (area > best) { best = area; bi = i; bw = w; bh = hmin; bk = k - hmin + 1; }
                    }
                }
            }
            return best > 0;
        }
    }
}
