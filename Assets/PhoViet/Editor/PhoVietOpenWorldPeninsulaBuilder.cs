using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.Editor
{
    // 21.302 — người dùng 21/11/2026 (ảnh khoanh đỏ quanh khu phố): «xóa tất cả cảnh vật ngoài phạm vi đường đỏ, vào game không hiển
    // thị và xe không đi vào được (khi cần mở rộng sẽ mở lại); các vùng đã xóa hiển thị cho đẹp, hợp lý» → chọn «biển/vịnh bao quanh».
    // Menu 20 (scene đang mở):
    //   1. Ranh giới = mép nền bê tông thành phố: x, z ∈ [−600, 700] (OW_Map_Nen submesh OW_Concrete).
    //   2. Xóa mọi vật nằm hẳn ngoài ranh giới (cao ốc vành chân trời, cỏ, sông kéo dài, va chạm ngoài…); lưới vắt qua ranh giới
    //      thuộc mặt đất/sông/kè/lòng sông/mặt sông → CẮT tam giác đúng theo ranh giới (giữ UV, màu đỉnh) → lưới mới
    //      OpenWorld/Library/BanDao/*.asset; vật nhỏ vắt qua: tâm ngoài thì xóa.
    //   3. Dựng «BanDao» (gốc scene): MẶT BIỂN quanh bán đảo tới ±3 km (shader sông, _SeaMode = 1, cùng hàm sóng → nối liền mặt
    //      sông ở 2 cửa sông; màu đỉnh r = bọt vỗ chân kè), BỜ KÈ đá dọc mép (trừ 2 cửa sông) + mũ kè đá granite + LAN CAN sắt,
    //      TƯỜNG VÔ HÌNH (layer Ignore Raycast: xe va vào, tia/tên lửa xuyên qua) cao 40 m quanh ranh giới, 6 ĐẢO ĐỒI xa
    //      800–1250 m mờ trong sương, bầu trời procedural có màu dưới chân trời = màu sương (không lộ vạch xám ở chân trời).
    //   Khôi phục: lấy lại scene PhoVietOpenWorld.unity ở commit trước 21.302 (mọi lưới FBX gốc còn nguyên).
    public static class PhoVietOpenWorldPeninsulaBuilder
    {
        // 21.304 (người dùng, ảnh khoanh đường đỏ): cạnh nam + tây cắt vào 32,5 m (bỏ cả dải đường dạo bê tông rộng 33 m, mép mới
        // = mép ngoài vỉa hè phía biển, đo từ OW_Duong_*_ViaHe min = −567,5); cạnh bắc + đông giữ nguyên (đường dạo 13 m).
        public const float X0 = -567.5f, X1 = 700f, Z0 = -567.5f, Z1 = 700f;
        const string Dir = "Assets/PhoViet/OpenWorld/Library/BanDao";
        const float WaterY = -2.2f;
        // 2 cửa sông trên ranh giới (không xây kè): mép nam z = Z0, x ∈ [234, 400]; mép tây x = X0, z ∈ [229, 400]
        // 21.305: bờ thật đo theo lan can sông (tâm trụ cách bờ 0,35 m): cửa nam x 234,5–400, cửa tây z 228,5–400
        static readonly Vector2 MouthSouth = new Vector2(234.5f, 400f), MouthWest = new Vector2(228.5f, 400f);

        [MenuItem("Phố Việt/Bản đồ mở/20. Bán đảo: xóa ngoài ranh giới + biển bao quanh (21.302, scene đang mở)")]
        public static void Menu() => Debug.Log(Run());

        public static string Run()
        {
            Directory.CreateDirectory(Dir);
            var log = new System.Text.StringBuilder();
            int lodDel = 0, rDel = 0, clipped = 0, cDel = 0, cClip = 0;
            var roots = new List<Transform>();
            foreach (var n in new[] { "BanDoMo", "VaCham", "VaCham_RaoLanCan", "MatLe" })
                foreach (var r in EditorSceneManager.GetActiveScene().GetRootGameObjects()) if (r.name == n) roots.Add(r.transform);
            var clipParents = new HashSet<string> { "OW_Map_ground", "OW_Map_river", "MatSongMoi" };

            // 0) 21.304 (người dùng khoanh đỏ): bỏ tàu du lịch trang trí đứng yên trên sông (OW_Map_showcase/OW_TauDuLich_The_Goc)
            foreach (var path in new[] { "BanDoMo/OW_Map_showcase/OW_TauDuLich_The_Goc" })
            { var go = GameObject.Find(path); if (go) { Kill(go); log.Append("Xóa ").Append(path).Append(". "); } }
            // 1) LODGroup nằm hẳn ngoài (cao ốc vành chân trời, cây/cột ngoài…)
            foreach (var root in roots)
                foreach (var lg in root.GetComponentsInChildren<LODGroup>(true))
                {
                    if (!lg) continue;
                    var b = WorldBounds(lg.gameObject); if (!b.HasValue) continue;
                    if (Outside(b.Value) || (Crosses(b.Value) && !InsideXZ(b.Value.center))) { Kill(lg.gameObject); lodDel++; }
                }
            // 2) Renderer
            foreach (var root in roots)
                foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (!mr) continue;
                    var b = mr.bounds; var mf = mr.GetComponent<MeshFilter>();
                    if (Outside(b)) { KillRenderer(mr); rDel++; continue; }
                    if (!Crosses(b)) continue;
                    bool terrain = mf && mf.sharedMesh && (InClipParent(mr.transform, clipParents) || b.size.x > 60f || b.size.z > 60f);
                    if (terrain)
                    {
                        var nm = ClipMesh(mf.sharedMesh, mr.transform, mr.name);
                        if (nm == null) { KillRenderer(mr); rDel++; continue; }
                        var mc = mr.GetComponent<MeshCollider>(); if (mc && mc.sharedMesh == mf.sharedMesh) mc.sharedMesh = nm;
                        mf.sharedMesh = nm; clipped++;
                    }
                    else if (!InsideXZ(b.center)) { KillRenderer(mr); rDel++; }
                }
            // 3) Collider còn lại
            foreach (var root in roots)
                foreach (var c in root.GetComponentsInChildren<Collider>(true))
                {
                    if (!c) continue;
                    var b = c.bounds;
                    if (b.size == Vector3.zero) { var mcol = c as MeshCollider; if (!mcol || !mcol.sharedMesh) continue; b = TransformBounds(mcol.sharedMesh.bounds, c.transform); }
                    if (Outside(b)) { Object.DestroyImmediate(c); cDel++; continue; }
                    // 21.304: hộp va chạm mặt đất (VaCham/MatDat…) không xoay vắt qua ranh giới → co hộp vào trong ranh giới
                    if (Crosses(b) && c is BoxCollider bc && Quaternion.Angle(bc.transform.rotation, Quaternion.identity) < 0.1f)
                    {
                        var lo = new Vector3(Mathf.Max(b.min.x, X0), b.min.y, Mathf.Max(b.min.z, Z0));
                        var hi = new Vector3(Mathf.Min(b.max.x, X1), b.max.y, Mathf.Min(b.max.z, Z1));
                        if (hi.x - lo.x < 0.5f || hi.z - lo.z < 0.5f) { Object.DestroyImmediate(bc); cDel++; continue; }
                        var ls = bc.transform.lossyScale;
                        bc.center = bc.transform.InverseTransformPoint((lo + hi) * 0.5f);
                        bc.size = new Vector3((hi.x - lo.x) / Mathf.Abs(ls.x), (hi.y - lo.y) / Mathf.Abs(ls.y), (hi.z - lo.z) / Mathf.Abs(ls.z));
                        cClip++; continue;
                    }
                    if (Crosses(b) && c is MeshCollider mc2 && mc2.sharedMesh && (b.size.x > 60f || b.size.z > 60f))
                    {
                        var nm = ClipMesh(mc2.sharedMesh, mc2.transform, mc2.name + "_VaCham");
                        if (nm == null) { Object.DestroyImmediate(mc2); cDel++; } else { mc2.sharedMesh = nm; cClip++; }
                    }
                }
            log.Append($"Xóa {lodDel} cụm LOD, {rDel} lưới, {cDel} va chạm; cắt {clipped} lưới + {cClip} lưới va chạm theo ranh giới. ");

            // 4) Biển + kè + lan can + tường + đảo
            var old = GameObject.Find("BanDao"); if (old) Object.DestroyImmediate(old);
            var bd = new GameObject("BanDao").transform;
            log.Append(BuildSea(bd)); log.Append(BuildCornerFix(bd)); log.Append(BuildSeawall(bd)); log.Append(BuildPromenade(bd)); log.Append(BuildBarrier(bd)); log.Append(Sky());
            // 21.302b (người dùng): bỏ hết đảo, chỉ còn biển → xóa lưới/vật liệu đảo cũ nếu còn
            foreach (var a in new[] { Dir + "/OW_BanDao_Dao.asset", Dir + "/OW_BanDao_DaoRung.mat" }) if (AssetDatabase.LoadMainAssetAtPath(a)) AssetDatabase.DeleteAsset(a);
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.GetActiveScene(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return log.ToString();
        }

        // ------------------------------------------------------------ kiểm tra vùng
        static bool InsideXZ(Vector3 p) => p.x >= X0 && p.x <= X1 && p.z >= Z0 && p.z <= Z1;
        static bool Outside(Bounds b) => b.max.x <= X0 || b.min.x >= X1 || b.max.z <= Z0 || b.min.z >= Z1;
        static bool Crosses(Bounds b) => !Outside(b) && (b.min.x < X0 - .05f || b.max.x > X1 + .05f || b.min.z < Z0 - .05f || b.max.z > Z1 + .05f);
        static bool InClipParent(Transform t, HashSet<string> names) { for (var p = t; p; p = p.parent) if (names.Contains(p.name)) return true; return false; }
        static Bounds? WorldBounds(GameObject go)
        {
            Bounds? b = null;
            foreach (var r in go.GetComponentsInChildren<Renderer>(true)) { if (b == null) b = r.bounds; else { var x = b.Value; x.Encapsulate(r.bounds); b = x; } }
            return b;
        }
        static Bounds TransformBounds(Bounds lb, Transform t)
        {
            var b = new Bounds(t.TransformPoint(lb.center), Vector3.zero);
            for (int i = 0; i < 8; i++) b.Encapsulate(t.TransformPoint(lb.center + Vector3.Scale(lb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
            return b;
        }
        static void Kill(GameObject go)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                var root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (root == go) { Object.DestroyImmediate(go); return; }
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            Object.DestroyImmediate(go);
        }
        static void KillRenderer(MeshRenderer mr)
        {
            var go = mr.gameObject;
            if (go.transform.childCount == 0 && go.GetComponents<Component>().Length <= 4 && !go.GetComponent<LODGroup>()) { Kill(go); return; }
            if (PrefabUtility.IsPartOfPrefabInstance(go)) PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(go), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var mf = go.GetComponent<MeshFilter>(); Object.DestroyImmediate(mr); if (mf) Object.DestroyImmediate(mf);
        }

        // ------------------------------------------------------------ cắt lưới theo hình chữ nhật XZ (Sutherland–Hodgman)
        struct V { public Vector3 p, n; public Vector4 t; public Color c; public Vector2 u0, u1; public Vector3 w; }
        static V Lerp(V a, V b, float k) => new V
        {
            p = Vector3.Lerp(a.p, b.p, k), n = Vector3.Lerp(a.n, b.n, k), t = Vector4.Lerp(a.t, b.t, k), c = Color.Lerp(a.c, b.c, k),
            u0 = Vector2.Lerp(a.u0, b.u0, k), u1 = Vector2.Lerp(a.u1, b.u1, k), w = Vector3.Lerp(a.w, b.w, k)
        };
        static List<V> ClipPlane(List<V> poly, int axis, float lim, bool keepGreater)
        {
            var o = new List<V>(); int n = poly.Count; if (n == 0) return o;
            for (int i = 0; i < n; i++)
            {
                V a = poly[i], b = poly[(i + 1) % n];
                float da = (axis == 0 ? a.w.x : a.w.z) - lim, db = (axis == 0 ? b.w.x : b.w.z) - lim;
                if (!keepGreater) { da = -da; db = -db; }
                bool ia = da >= 0, ib = db >= 0;
                if (ia) o.Add(a);
                if (ia != ib) o.Add(Lerp(a, b, da / (da - db)));
            }
            return o;
        }

        static Mesh ClipMesh(Mesh src, Transform tr, string name)
        {
            var pos = src.vertices; var nrm = src.normals; var tan = src.tangents; var col = src.colors; var uv0 = src.uv; var uv1 = src.uv2;
            bool hn = nrm.Length == pos.Length, ht = tan.Length == pos.Length, hc = col.Length == pos.Length, h0 = uv0.Length == pos.Length, h1 = uv1.Length == pos.Length;
            var outV = new List<V>(); var map = new Dictionary<int, int>(); var subs = new List<List<int>>(); int total = 0;
            V Get(int i) => new V { p = pos[i], n = hn ? nrm[i] : Vector3.up, t = ht ? tan[i] : Vector4.zero, c = hc ? col[i] : Color.white, u0 = h0 ? uv0[i] : Vector2.zero, u1 = h1 ? uv1[i] : Vector2.zero, w = tr.TransformPoint(pos[i]) };
            for (int s = 0; s < src.subMeshCount; s++)
            {
                var tri = src.GetTriangles(s); var o = new List<int>();
                for (int k = 0; k < tri.Length; k += 3)
                {
                    var a = Get(tri[k]); var b = Get(tri[k + 1]); var c = Get(tri[k + 2]);
                    if (InsideXZ(a.w) && InsideXZ(b.w) && InsideXZ(c.w))
                    {
                        foreach (int idx in new[] { tri[k], tri[k + 1], tri[k + 2] })
                        { if (!map.TryGetValue(idx, out int ni)) { ni = outV.Count; outV.Add(Get(idx)); map[idx] = ni; } o.Add(ni); }
                        continue;
                    }
                    var poly = new List<V> { a, b, c };
                    poly = ClipPlane(poly, 0, X0, true); poly = ClipPlane(poly, 0, X1, false);
                    poly = ClipPlane(poly, 1, Z0, true); poly = ClipPlane(poly, 1, Z1, false);
                    if (poly.Count < 3) continue;
                    int b0 = outV.Count; outV.AddRange(poly);
                    for (int q = 1; q + 1 < poly.Count; q++) { o.Add(b0); o.Add(b0 + q); o.Add(b0 + q + 1); }
                }
                subs.Add(o); total += o.Count;
            }
            if (total == 0) return null;
            var m = new Mesh { name = name + "_BanDao" };
            if (outV.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(outV.ConvertAll(v => v.p));
            if (hn) m.SetNormals(outV.ConvertAll(v => v.n));
            if (ht) m.SetTangents(outV.ConvertAll(v => v.t));
            if (hc) m.SetColors(outV.ConvertAll(v => v.c));
            if (h0) m.SetUVs(0, outV.ConvertAll(v => v.u0));
            if (h1) m.SetUVs(1, outV.ConvertAll(v => v.u1));
            m.subMeshCount = subs.Count; for (int s = 0; s < subs.Count; s++) m.SetTriangles(subs[s], s);
            m.RecalculateBounds();
            return SaveMesh(m, SafeName(name));
        }
        static string SafeName(string n) { foreach (var ch in Path.GetInvalidFileNameChars()) n = n.Replace(ch, '_'); return n.Replace(' ', '_'); }
        static Mesh SaveMesh(Mesh m, string file)
        {
            string path = Dir + "/" + file + ".asset";
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (old) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(m, path); return m;
        }
        static Material Mat(string n) => AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/OpenWorld/Materials/" + n + ".mat");

        // ------------------------------------------------------------ biển
        static string BuildSea(Transform parent)
        {
            // lưới vuông [−3000, 3100]², bước nhỏ sát bờ, thưa dần ra xa; bỏ ô nằm trong bán đảo
            List<float> Axis(float a0, float a1)
            {
                var L = new List<float>();
                void Run(float from, float to, int dir)
                {   // từ mép bán đảo ra xa: 4 m tới 40 m, 10 m tới 200, 30 m tới 600, 90 m tới 1500, 250 m tới hết
                    float d = 0; var pts = new List<float>();
                    while (true) { float step = d < 40 ? 4 : d < 200 ? 10 : d < 600 ? 30 : d < 1500 ? 90 : 250; d += step; float v = from + dir * d; if ((dir > 0 && v >= to) || (dir < 0 && v <= to)) { pts.Add(to); break; } pts.Add(v); }
                    L.AddRange(pts);
                }
                L.Add(a0); for (float v = a0 + 12f; v < a1; v += 12f) L.Add(v); L.Add(a1);
                Run(a0, -3000f, -1); Run(a1, 3100f, 1);
                L.Sort(); var u = new List<float>(); foreach (var v in L) if (u.Count == 0 || v - u[u.Count - 1] > 0.5f) u.Add(v);
                return u;
            }
            var xs = Axis(X0, X1); var zs = Axis(Z0, Z1);
            var vs = new List<Vector3>(); var uvs = new List<Vector2>(); var cs = new List<Color>(); var nm = new List<Vector3>(); var tri = new List<int>();
            var seaEdges = Edges();
            var id = new int[xs.Count, zs.Count];
            for (int i = 0; i < xs.Count; i++) for (int j = 0; j < zs.Count; j++) id[i, j] = -1;
            int Vert(int i, int j)
            {
                if (id[i, j] >= 0) return id[i, j];
                float x = xs[i], z = zs[j];
                // khoảng cách tới CHÂN KÈ (các đoạn kè thật, không tính 2 cửa sông) → bọt vỗ kè không vẽ ngang cửa sông
                float d = float.MaxValue; var q = new Vector2(x, z);
                foreach (var (ea, eb, _) in seaEdges)
                {
                    Vector2 a2 = new Vector2(ea.x, ea.z), b2 = new Vector2(eb.x, eb.z), ab = b2 - a2;
                    float tt = Mathf.Clamp01(Vector2.Dot(q - a2, ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
                    d = Mathf.Min(d, Vector2.Distance(q, a2 + ab * tt));
                }
                // 21.302c: đỉnh của dải lót cửa sông (nằm trong bán đảo) hạ 0,15 m dưới mép lưới sông → khe giữa 2 lưới không lộ trời
                bool inner = x > X0 + .1f && x < X1 - .1f && z > Z0 + .1f && z < Z1 - .1f;
                vs.Add(new Vector3(x, WaterY - (inner ? .15f : 0f), z)); nm.Add(Vector3.up);
                uvs.Add(new Vector2(z, -x));                                   // (s dọc dòng = z, d = −x) — dòng trôi hướng +z như shader sông
                // a: gần cửa sông = 1 (màu nước sông) → 0 ở 450 m (màu biển) — không lộ vạch nối ở 2 cửa sông
                float dm = Mathf.Min(Vector2.Distance(new Vector2(x, z), new Vector2(317f, Z0)), Vector2.Distance(new Vector2(x, z), new Vector2(X0, 314.5f)));
                float near = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(110f, 450f, dm));
                cs.Add(new Color(Mathf.Clamp01(1f - d / 10f), 0.5f, 1f, near));  // r: bọt vỗ chân kè; gb: hướng trôi (0, 1)
                return id[i, j] = vs.Count - 1;
            }
            for (int i = 0; i + 1 < xs.Count; i++)
                for (int j = 0; j + 1 < zs.Count; j++)
                {
                    float cx = (xs[i] + xs[i + 1]) * .5f, cz = (zs[j] + zs[j + 1]) * .5f;
                    // trong bán đảo bỏ, trừ dải lót 12 m dưới mép lưới sông ở 2 cửa sông (21.302c)
                    bool lining = (cx > MouthSouth.x && cx < MouthSouth.y && cz < Z0 + 12.5f) || (cz > MouthWest.x && cz < MouthWest.y && cx < X0 + 12.5f);
                    if (cx > X0 && cx < X1 && cz > Z0 && cz < Z1 && !lining) continue;
                    int a = Vert(i, j), b = Vert(i + 1, j), c = Vert(i + 1, j + 1), d = Vert(i, j + 1);
                    tri.AddRange(new[] { a, d, c, a, c, b });
                }
            var m = new Mesh { name = "OW_Bien", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            m.SetVertices(vs); m.SetNormals(nm); m.SetUVs(0, uvs); m.SetColors(cs); m.SetTriangles(tri, 0);
            m.bounds = new Bounds(new Vector3(50, WaterY, 50), new Vector3(6200, 8, 6200));
            SaveMesh(m, "OW_Bien");
            var river = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/OpenWorld/River/OW_River.mat");
            string mp = Dir + "/OW_Bien.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(mp);
            if (!mat) { mat = new Material(river); AssetDatabase.CreateAsset(mat, mp); } else mat.CopyPropertiesFromMaterial(river);
            // 21.302b/c (người dùng): nước biển giống hệt nước sông → giữ nguyên mọi thông số OW_River (màu, gợn, dòng trôi, bọt,
            // lấp lánh, phản chiếu); _SeaMode = 1 chỉ để shader lấy probe phản chiếu của sông theo vị trí thế giới.
            mat.SetFloat("_SeaMode", 1f);
            EditorUtility.SetDirty(mat);
            var go = new GameObject("MatBien"); go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = m;
            var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat;
            // 21.302c: thiết lập renderer y như các khúc mặt sông (OW_MatSong_*): không light probe (ánh sáng môi trường lấy SH
            // bầu trời như sông, không bị probe ven phố làm tối/sáng khác), không reflection probe Unity, đổ + nhận bóng.
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; mr.receiveShadows = true;
            mr.allowOcclusionWhenDynamic = false;                                 // mặt biển phủ cả bản đồ: không để occlusion loại nhầm
            return $"Biển {vs.Count} đỉnh. ";
        }

        // ------------------------------------------------------------ kè + mũ kè + lan can (từng cạnh, chừa 2 cửa sông)
        static List<(Vector3 a, Vector3 b, Vector3 outN)> Edges()
        {
            var e = new List<(Vector3, Vector3, Vector3)>();
            // nam (z = Z0, pháp tuyến −z), chừa cửa sông x ∈ [234, 400]
            e.Add((new Vector3(X0, 0, Z0), new Vector3(MouthSouth.x, 0, Z0), Vector3.back));
            e.Add((new Vector3(MouthSouth.y, 0, Z0), new Vector3(X1, 0, Z0), Vector3.back));
            e.Add((new Vector3(X1, 0, Z0), new Vector3(X1, 0, Z1), Vector3.right));            // đông
            e.Add((new Vector3(X1, 0, Z1), new Vector3(X0, 0, Z1), Vector3.forward));          // bắc
            // tây (x = X0, pháp tuyến −x), chừa cửa sông z ∈ [229, 400]
            e.Add((new Vector3(X0, 0, Z1), new Vector3(X0, 0, MouthWest.y), Vector3.left));
            e.Add((new Vector3(X0, 0, MouthWest.x), new Vector3(X0, 0, Z0), Vector3.left));
            return e;
        }

        // 21.305 (người dùng: «dựng lại tất cả hàng rào ở lề biển giống hàng rào lề sông»; góc hàng rào đâm xuyên nhau):
        // lan can theo đúng số đo OW_Map_LanCanSong — trụ đá granite (đế 0,56 × 0,20, thân 0,44 × 0,80, mũ 0,56 × 0,12), bước trụ
        // ~3,82 m, tâm trụ cách mép kè 0,35 m; 2 thanh sắt 6 cm (cao 0,21 và 0,90 từ chân trụ) + song sắt 2 cm mỗi 13 cm.
        // Đầu mỗi cạnh lùi 0,35 m theo chiều dọc → ở 4 góc hai cạnh chung MỘT trụ góc, thanh/song chỉ chạy giữa mặt trụ (không đâm
        // xuyên); ở cửa sông trụ cuối nằm đúng hàng lan can sông. Chân trụ theo mặt lề (vỉa hè 0,15; đường dạo bắc/đông −0,08).
        // Chia khúc ~16 ô (≈ 61 m), mỗi khúc 1 LODGroup: gần đủ song sắt, xa (song < 1 điểm ảnh) chỉ trụ + 2 thanh, rất xa ẩn.
        const float PostPitch = 3.82f, PostIn = 0.35f;
        static float LeTop(Vector3 p) =>
            Physics.Raycast(new Vector3(p.x, 5f, p.z), Vector3.down, out var h, 10f, 1 << PhoViet.OpenWorld.OpenWorldGroundFit.SurfaceLayer, QueryTriggerInteraction.Ignore) ? h.point.y : -0.08f;

        static string BuildSeawall(Transform parent)
        {
            var wall = new MeshBuilder(); var cap = new MeshBuilder();
            const float bottom = -7.5f;
            foreach (var (a, b, n) in Edges())
            {
                float baseY = LeTop((a + b) * .5f - n * 1.5f);
                // mặt kè đá (hướng ra biển, chân chìm, hơi dốc) + gờ đá granite ở mép (nhô 0,15 m, cao hơn mặt lề 5 cm)
                var tA = a + Vector3.up * baseY; var tB = b + Vector3.up * baseY;
                wall.Quad(tA, tB, b + n * 0.8f + Vector3.up * bottom, a + n * 0.8f + Vector3.up * bottom, n);
                var o = n * 0.15f; float ct = baseY + .05f, cb = baseY - .25f;
                // ở 4 góc gờ đá kéo dài thêm 0,15 m để hai cạnh phủ kín ô vuông góc ngoài (không hở)
                var dir = (b - a).normalized; Vector3 ca = a, cbb = b;
                bool Corner(Vector3 q) => (Mathf.Abs(q.x - X0) < .01f || Mathf.Abs(q.x - X1) < .01f) && (Mathf.Abs(q.z - Z0) < .01f || Mathf.Abs(q.z - Z1) < .01f);
                if (Corner(a)) ca -= dir * .15f;
                if (Corner(b)) cbb += dir * .15f;
                cap.Quad(ca - n * .06f + Vector3.up * ct, cbb - n * .06f + Vector3.up * ct, cbb + o + Vector3.up * ct, ca + o + Vector3.up * ct, Vector3.up);
                cap.Quad(ca + o + Vector3.up * ct, cbb + o + Vector3.up * ct, cbb + o + Vector3.up * cb, ca + o + Vector3.up * cb, n);
            }
            void Make(string name, MeshBuilder mb, string mat)
            {
                var m = SaveMesh(mb.Build("OW_BanDao_" + name), "OW_BanDao_" + name);
                var go = new GameObject(name); go.transform.SetParent(parent, false);
                go.AddComponent<MeshFilter>().sharedMesh = m; go.AddComponent<MeshRenderer>().sharedMaterial = Mat(mat);
            }
            Make("KeBien", wall, "OW_BankStone"); Make("MuKe", cap, "OW_Granite");
            return BuildRailing(parent);
        }

        static string BuildRailing(Transform parent)
        {
            string path = Dir + "/OW_BanDao_LanCanBien.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)) AssetDatabase.DeleteAsset(path);
            var root = new GameObject("LanCanBien").transform; root.SetParent(parent, false);
            Material granite = Mat("OW_Granite"), iron = Mat("OW_IronBlack");
            var placed = new HashSet<Vector2Int>();
            int chunks = 0, posts = 0, panelsAll = 0, bars = 0; bool first = true;
            MeshBuilder tru = null, thanh = null, song = null; int inChunk = 0;
            void Flush()
            {
                if (tru == null || inChunk == 0) return;
                var go = new GameObject("LanCan_" + chunks.ToString("000")); go.transform.SetParent(root, false);
                Renderer Part(string nm, MeshBuilder mb, Material mat, bool shadow)
                {
                    var m = mb.Build("OW_LanCanBien_" + chunks.ToString("000") + "_" + nm);
                    if (first) { AssetDatabase.CreateAsset(m, path); first = false; } else AssetDatabase.AddObjectToAsset(m, path);
                    var g = new GameObject(nm); g.transform.SetParent(go.transform, false);
                    g.AddComponent<MeshFilter>().sharedMesh = m; var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = mat;
                    r.shadowCastingMode = shadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    return r;
                }
                var rTru = Part("Tru", tru, granite, true); var rThanh = Part("Thanh", thanh, iron, true); var rSong = Part("Song", song, iron, false);
                var lg = go.AddComponent<LODGroup>();
                lg.SetLODs(new[] { new LOD(0.55f, new[] { rTru, rThanh, rSong }), new LOD(0.012f, new[] { rTru, rThanh }) });
                lg.RecalculateBounds();
                chunks++; tru = thanh = song = null; inChunk = 0;
            }
            void Begin() { if (tru == null) { tru = new MeshBuilder(); thanh = new MeshBuilder(); song = new MeshBuilder(); } }
            // một dãy lan can từ tâm trụ A tới tâm trụ B (skipLast: trụ B là trụ lan can sông có sẵn → không dựng lại)
            void Run(Vector3 A, Vector3 B, bool skipLast)
            {
                var dir = (B - A).normalized;
                float L = (B - A).magnitude; int panels = Mathf.Max(1, Mathf.RoundToInt(L / PostPitch)); float pitch = L / panels;
                var P = new Vector3[panels + 1]; var Y = new float[panels + 1];
                for (int k = 0; k <= panels; k++) { P[k] = A + dir * (pitch * k); P[k].y = 0f; Y[k] = LeTop(P[k]); }
                Begin();
                for (int k = 0; k <= panels; k++)
                {
                    var key = new Vector2Int(Mathf.RoundToInt(P[k].x * 10f), Mathf.RoundToInt(P[k].z * 10f));
                    if (!(skipLast && k == panels) && placed.Add(key))
                    {   // trụ: đế, thân, mũ
                        float y = Y[k];
                        tru.Box(P[k] + Vector3.up * (y + .10f), new Vector3(.56f, .20f, .56f), dir);
                        tru.Box(P[k] + Vector3.up * (y + .60f), new Vector3(.44f, .80f, .44f), dir);
                        tru.Box(P[k] + Vector3.up * (y + 1.06f), new Vector3(.56f, .12f, .56f), dir);
                        posts++;
                    }
                    if (k == panels) break;
                    // ô lan can giữa 2 mặt trụ
                    var s0 = P[k] + dir * .22f; var s1 = P[k + 1] - dir * .22f; float span = (s1 - s0).magnitude; var mid = (s0 + s1) * .5f;
                    float yb = Mathf.Min(Y[k], Y[k + 1]);
                    thanh.Box(mid + Vector3.up * (yb + .21f), new Vector3(.06f, .06f, span), dir);
                    thanh.Box(mid + Vector3.up * (yb + .90f), new Vector3(.06f, .06f, span), dir);
                    int nb = Mathf.Max(2, Mathf.RoundToInt(span / .13f));
                    // 21.310 (trước phát hành 1.88.0): song sắt 2 cm dựng bằng 2 mặt phẳng (trước/sau, 8 đỉnh) thay hộp 20 đỉnh —
                    // nhìn như nhau (song quá mảnh để thấy cạnh bên), lưới nhẹ ~60 % cho điện thoại
                    var sideN = Vector3.Cross(Vector3.up, dir);
                    for (int j = 1; j < nb; j++)
                    {
                        var bc = s0 + dir * (span * j / nb); var lo = Vector3.up * (yb + .20f); var hi = Vector3.up * (yb + .90f); var hw = dir * .01f;
                        song.Quad(bc - hw + lo, bc + hw + lo, bc + hw + hi, bc - hw + hi, sideN);
                        song.Quad(bc + hw + lo, bc - hw + lo, bc - hw + hi, bc + hw + hi, -sideN);
                        bars++;
                    }
                    panelsAll++; inChunk++;
                    if (inChunk >= 16) { Flush(); Begin(); }
                }
                Flush();
            }
            foreach (var (a, b, n) in Edges())
            {
                var dir = (b - a).normalized;
                Run(a - n * PostIn + dir * PostIn, b - n * PostIn - dir * PostIn, false);
            }
            // 21.305: 4 đoạn lan can sông sát cửa sông bị xóa khi cắt 21.304 (tâm đoạn nằm ngoài ranh giới mới) → nối từ trụ góc
            // lan can biển tới trụ cuối lan can sông còn lại (va chạm VaCham_OW_Map_LanCanSong_* của đoạn cũ vẫn còn)
            float zs = Z0 + PostIn, xs = X0 + PostIn;
            Run(new Vector3(MouthSouth.x - PostIn, 0, zs), new Vector3(MouthSouth.x - PostIn, 0, -542.68f), true);
            Run(new Vector3(MouthSouth.y + PostIn, 0, zs), new Vector3(MouthSouth.y + PostIn, 0, -542.68f), true);
            Run(new Vector3(xs, 0, MouthWest.x - PostIn), new Vector3(-545.31f, 0, MouthWest.x - PostIn), true);
            Run(new Vector3(xs, 0, MouthWest.y + PostIn), new Vector3(-545.31f, 0, MouthWest.y + PostIn), true);
            return $"Kè + lan can kiểu lan can sông: {posts} trụ, {panelsAll} ô, {bars} song sắt, {chunks} khúc LOD. ";
        }

        // 21.305 (người dùng: «lề biển lấn thừa ra ngoài đường»): khi cắt cạnh nam/tây (21.304) mảnh nút giao + mảnh vỉa hè góc
        // tây nam (tâm nằm ngoài ranh giới mới) bị xóa → lộ nền bê tông thấp. Đúc lại: mặt đường nút giao x[−563,5; −556,5] ×
        // z[−563,5; −560] (đỉnh 0, khớp OW_Duong_P1/P2_MatDuong) + vỉa hè chữ L ôm góc (đỉnh 0,15, bó vỉa 0,17 dày 0,25 m, khớp
        // OW_Duong_P1/P2_ViaHe) + va chạm lề (layer MatLe) cho vỉa hè.
        static string BuildCornerFix(Transform parent)
        {
            var road = new MeshBuilder(); var walk = new MeshBuilder();
            road.Box(new Vector3(-560f, -.15f, -561.75f), new Vector3(7f, .30f, 3.5f), Vector3.forward);
            walk.Box(new Vector3(-565.625f, .05f, -563.75f), new Vector3(3.75f, .20f, 7.5f), Vector3.forward);    // dọc cạnh tây tới góc
            walk.Box(new Vector3(-561.875f, .05f, -565.625f), new Vector3(3.75f, .20f, 3.75f), Vector3.forward);  // dọc cạnh nam tới góc
            walk.Box(new Vector3(-563.625f, .06f, -561.75f), new Vector3(.25f, .22f, 3.5f), Vector3.forward);     // bó vỉa tây
            walk.Box(new Vector3(-561.75f, .06f, -563.625f), new Vector3(3.5f, .22f, .25f), Vector3.forward);     // bó vỉa nam
            walk.Box(new Vector3(-563.625f, .06f, -563.625f), new Vector3(.25f, .22f, .25f), Vector3.forward);    // góc bó vỉa
            var go = new GameObject("GocTayNam"); go.transform.SetParent(parent, false);
            void Part(string nm, MeshBuilder mb, string mat, bool le)
            {
                var m = SaveMesh(mb.Build("OW_BanDao_GocTayNam_" + nm), "OW_BanDao_GocTayNam_" + nm);
                var g = new GameObject(nm); g.transform.SetParent(go.transform, false);
                g.AddComponent<MeshFilter>().sharedMesh = m; g.AddComponent<MeshRenderer>().sharedMaterial = Mat(mat);
                if (le) { var c = new GameObject(nm + "_MatLe"); c.transform.SetParent(go.transform, false); c.layer = PhoViet.OpenWorld.OpenWorldGroundFit.SurfaceLayer; c.AddComponent<MeshCollider>().sharedMesh = m; }
            }
            Part("MatDuong", road, "OW_Asphalt", false); Part("ViaHe", walk, "OW_Paving", true);
            return "Góc tây nam: đúc lại nút giao + vỉa hè. ";
        }

        // ------------------------------------------------------------ đường dạo ven biển: hàng dừa mỗi 18 m + cột đèn cờ mỗi 36 m
        // (sao chép cây dừa / cột đèn có sẵn + va chạm của chúng), cách mép kè 4,5 m, bỏ chỗ trùng cây/cột/vật cản sẵn có.
        static string BuildPromenade(Transform parent)
        {
            var root = GameObject.Find("BanDoMo").transform;
            var cay = root.Find("Cay"); var cot = root.Find("CotDen"); var vcCay = root.Find("VaCham_Cay"); var vcCot = root.Find("VaCham_CotDen");
            if (!cay || cay.childCount == 0) return "Không có cây mẫu. ";
            Transform treeSrc = cay.GetChild(0), lampSrc = null;
            foreach (Transform c in cot) if (c.name.Contains("_Co")) { lampSrc = c; break; }
            Transform treeCol = vcCay && vcCay.childCount > 0 ? vcCay.GetChild(0) : null, lampCol = vcCot && vcCot.childCount > 0 ? vcCot.GetChild(0) : null;
            var existing = new List<Vector3>();
            foreach (Transform c in cay) existing.Add(c.position); foreach (Transform c in cot) existing.Add(c.position);
            var holder = new GameObject("DuongDaoVenBien").transform; holder.SetParent(parent, false);
            int trees = 0, lamps = 0;
            bool Free(Vector3 p)
            {
                foreach (var e in existing) if ((new Vector2(e.x - p.x, e.z - p.z)).sqrMagnitude < 7f * 7f) return false;
                foreach (var hit in Physics.OverlapSphere(p + Vector3.up * 1.9f, 1.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))   // trên mặt đất/bó vỉa
                    if (!hit.transform.IsChildOf(parent)) return false;
                return true;
            }
            foreach (var (a, b, n) in Edges())
            {
                if (n == Vector3.back || n == Vector3.left) continue;   // 21.304: nam/tây đã cắt hết đường dạo, kè sát vỉa hè → không trồng
                var dir = (b - a).normalized; float len = (b - a).magnitude;
                for (float s = 9f; s < len - 6f; s += 9f)
                {
                    bool lamp = Mathf.RoundToInt(s / 9f) % 4 == 2, tree = Mathf.RoundToInt(s / 9f) % 2 == 0 && !lamp;
                    if (!lamp && !tree) continue;
                    var p = a + dir * s - n * 4.5f; p.y = 0f;
                    if (!Free(p)) continue;
                    if (tree)
                    {
                        var t = Object.Instantiate(treeSrc.gameObject, holder).transform; t.name = "CayDuaVenBien";
                        t.SetPositionAndRotation(p, Quaternion.Euler(0, (s * 37f) % 360f, 0)); t.localScale = treeSrc.localScale * (0.9f + 0.2f * Mathf.PerlinNoise(s * .05f, a.x * .01f));
                        if (treeCol) { var c = Object.Instantiate(treeCol.gameObject, holder).transform; c.name = "VaCham_CayVenBien"; c.SetPositionAndRotation(new Vector3(p.x, treeCol.position.y, p.z), t.rotation); }
                        existing.Add(p); trees++;
                    }
                    else if (lampSrc)
                    {
                        // tay đèn (trục up cục bộ của cột) quay vào trong đất liền, cờ hướng ra đường dạo
                        var t = Object.Instantiate(lampSrc.gameObject, holder).transform; t.name = "CotDenVenBien";
                        t.position = p; t.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(lampSrc.right, -n, Vector3.up), Vector3.up) * lampSrc.rotation;   // tay đèn = trục x lưới
                        if (lampCol) { var c = Object.Instantiate(lampCol.gameObject, holder).transform; c.name = "VaCham_CotDenVenBien"; c.position = new Vector3(p.x, lampCol.position.y, p.z); }
                        existing.Add(p); lamps++;
                    }
                }
            }
            return $"Đường dạo: {trees} cây dừa, {lamps} cột đèn. ";
        }

        // ------------------------------------------------------------ tường vô hình quanh ranh giới (kể cả 2 cửa sông)
        static string BuildBarrier(Transform parent)
        {
            var g = new GameObject("TuongRanhGioi"); g.transform.SetParent(parent, false); g.layer = 2;   // Ignore Raycast
            const float h = 40f, t = 4f, inset = 0.7f;      // 21.305: mặt trong tường sau mặt trong trụ lan can (0,63 m) → xe không lấn vào lan can
            void Box(Vector3 c, Vector3 s) { var b = g.AddComponent<BoxCollider>(); b.center = c; b.size = s; }
            float w = X1 - X0 + 2 * t, d = Z1 - Z0 + 2 * t, cx = (X0 + X1) * .5f, cz = (Z0 + Z1) * .5f;
            Box(new Vector3(cx, h * .5f - 5f, Z0 + inset - t * .5f), new Vector3(w, h, t));
            Box(new Vector3(cx, h * .5f - 5f, Z1 - inset + t * .5f), new Vector3(w, h, t));
            Box(new Vector3(X0 + inset - t * .5f, h * .5f - 5f, cz), new Vector3(t, h, d));
            Box(new Vector3(X1 - inset + t * .5f, h * .5f - 5f, cz), new Vector3(t, h, d));
            return "Tường ranh giới 4 mặt. ";
        }

        // ------------------------------------------------------------ đảo đồi xa (mờ trong sương 300–1450 m)
        static string BuildIslands(Transform parent)
        {
            // 7 đảo quanh bán đảo, mép đảo (kể cả nhấp nhô ×1,1) cách kè ≥ 220 m: (x, z, bán kính, cao)
            var spots = new (float x, float z, float rad, float hgt)[]
            {
                (-50f, 1290f, 280f, 120f), (1180f, 980f, 250f, 90f), (1250f, 150f, 330f, 150f), (650f, -1150f, 300f, 110f),
                (-450f, -1100f, 240f, 80f), (-1150f, -150f, 320f, 140f), (-1100f, 900f, 220f, 70f),
            };
            var mb = new MeshBuilder(); var rnd = new System.Random(302);
            foreach (var s in spots)
            {
                float seed = (float)rnd.NextDouble() * 100f;
                mb.Island(new Vector3(s.x, 0, s.z), s.rad, s.hgt, seed, WaterY);
            }
            var m = SaveMesh(mb.Build("OW_BanDao_Dao"), "OW_BanDao_Dao");
            var go = new GameObject("DaoXa"); go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = m;
            // rừng xanh thẫm (cỏ Triplanar tối lại) — sương xa làm dịu thành xanh lam mờ
            string mp = Dir + "/OW_BanDao_DaoRung.mat"; var grass = Mat("OW_Grass");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(mp);
            if (!mat) { mat = new Material(grass); AssetDatabase.CreateAsset(mat, mp); } else mat.CopyPropertiesFromMaterial(grass);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", new Color(0.42f, 0.55f, 0.4f));
            EditorUtility.SetDirty(mat);
            var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            mr.allowOcclusionWhenDynamic = false;
            return spots.Length + " đảo xa. ";
        }

        // bầu trời: màu dưới chân trời = màu sương → mặt biển (cắt ở 1,5 km) nối liền chân trời
        static string Sky()
        {
            var src = RenderSettings.skybox; if (!src || !src.shader || src.shader.name != "Skybox/Procedural") return "Bầu trời giữ nguyên. ";
            string p = Dir + "/OW_BauTroiBien.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (!m) { m = new Material(src); AssetDatabase.CreateAsset(m, p); } else if (src != m) m.CopyPropertiesFromMaterial(src);   // chạy lại: không tự chép từ chính nó
            m.SetColor("_GroundColor", RenderSettings.fogColor);
            EditorUtility.SetDirty(m); RenderSettings.skybox = m;
            return "Bầu trời: chân trời màu sương. ";
        }

        // ------------------------------------------------------------ dựng lưới đơn giản
        class MeshBuilder
        {
            readonly List<Vector3> v = new List<Vector3>(); readonly List<Vector3> n = new List<Vector3>(); readonly List<int> t = new List<int>();
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 nrm)
            {
                int i = v.Count; v.AddRange(new[] { a, b, c, d }); for (int k = 0; k < 4; k++) n.Add(nrm);
                // hướng mặt theo pháp tuyến
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), nrm) >= 0) t.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
                else t.AddRange(new[] { i, i + 2, i + 1, i, i + 3, i + 2 });
            }
            public void Box(Vector3 c, Vector3 size, Vector3 along)
            {
                var f = along.normalized; var r = Vector3.Cross(Vector3.up, f); var u = Vector3.up;
                var hx = r * size.x * .5f; var hy = u * size.y * .5f; var hz = f * size.z * .5f;
                Quad(c + hx - hy - hz, c + hx - hy + hz, c + hx + hy + hz, c + hx + hy - hz, r);
                Quad(c - hx - hy + hz, c - hx - hy - hz, c - hx + hy - hz, c - hx + hy + hz, -r);
                Quad(c - hx + hy - hz, c + hx + hy - hz, c + hx + hy + hz, c - hx + hy + hz, u);
                Quad(c - hx - hy + hz, c + hx - hy + hz, c + hx + hy + hz, c - hx + hy + hz, f);
                Quad(c + hx - hy - hz, c - hx - hy - hz, c - hx + hy - hz, c + hx + hy - hz, -f);
            }
            public void Island(Vector3 c, float R, float H, float seed, float water)
            {
                const int rings = 18, segs = 56; int b0 = v.Count;
                for (int i = 0; i <= rings; i++)
                    for (int j = 0; j < segs; j++)
                    {
                        float rr = i / (float)rings, a = j * Mathf.PI * 2 / segs;
                        float wob = 0.78f + 0.32f * Mathf.PerlinNoise(seed + Mathf.Cos(a) * 1.6f, seed + Mathf.Sin(a) * 1.6f);
                        float r = rr * R * wob;
                        float hill = Mathf.Pow(Mathf.Clamp01(1 - rr * rr), 1.25f) * H * (0.7f + 0.6f * Mathf.PerlinNoise(seed + Mathf.Cos(a) * rr * 3f, seed + 7 + Mathf.Sin(a) * rr * 3f));
                        float y = i == rings ? water - 6f : water + 0.6f + hill;
                        v.Add(c + new Vector3(Mathf.Cos(a) * r, y - c.y, Mathf.Sin(a) * r)); n.Add(Vector3.up);
                    }
                for (int i = 0; i < rings; i++)
                    for (int j = 0; j < segs; j++)
                    {
                        int a = b0 + i * segs + j, b = b0 + i * segs + (j + 1) % segs, cc = b0 + (i + 1) * segs + (j + 1) % segs, d = b0 + (i + 1) * segs + j;
                        t.AddRange(new[] { a, b, cc, a, cc, d });
                    }
            }
            public Mesh Build(string name)
            {
                var m = new Mesh { name = name }; if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                m.SetVertices(v); m.SetNormals(n); m.SetTriangles(t, 0); m.RecalculateNormals(); m.RecalculateBounds(); return m;
            }
        }
    }
}
