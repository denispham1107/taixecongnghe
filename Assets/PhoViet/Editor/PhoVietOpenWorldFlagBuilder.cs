using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.Editor
{
    // 21.298 — người dùng 21/11/2026: «thiết kế lại lá cờ đỏ sao vàng cho đẹp và thật hơn, có gió thổi bay nhè nhẹ, dài hơn 10 %».
    // Cờ cũ (build_ref3_segment.py banner(), 17/11) là một hộp 1,0 × 1,5 m dính liền lưới cột đèn OW_Mau_CotDen_Co (2 thanh
    // treo trên/dưới, sao phẳng) → đứng im, không thớ vải. Menu 19 (chạy sau mỗi lần dựng lại bản đồ/thư viện cột đèn):
    //   • ảnh vải OW_CoVN.png (620 × 1024): đỏ #DA251D có thớ dệt ngang/dọc, viền gấp mép + đường may, ống luồn sát cột,
    //     sao vàng 5 cánh chuẩn (bán kính ngoại tiếp = 1/5 chiều dài cờ) khử răng cưa 4×4;
    //   • lưới OW_CoVN (12 × 20 ô, một lớp vẽ hai mặt) dài 1,65 m (+10 %), rộng 1,0 m, mép trên giữ nguyên độ cao cũ (7,1 m);
    //     + lưới OW_CoVN_Thanh: thanh treo trên + 2 khoen giữ mép cờ vào cột (thép xám như cột);
    //   • vật liệu OW_CoVN (shader PhoViet/OpenWorld/FlagWind: sóng vải chạy từ cột ra, bao biên 0 ở cột/thanh, lệch nhịp
    //     theo vị trí cột);
    //   • 235 cột OW_Mau_CotDen_Co: lưới cột → OW_Mau_CotDen (cùng cột + đèn, không cờ), thêm con «CoVN» (vải) + «CoVN_Thanh»
    //     vào LOD0 của LODGroup (ẩn cùng cột). Lượt vẽ mỗi cột 4 → 4 (cột 2 + vải 1 + thanh 1), không thêm tam giác đáng kể.
    public static class PhoVietOpenWorldFlagBuilder
    {
        const string Dir = "Assets/PhoViet/OpenWorld/Library/CoVN";
        const float W = 1.0f, H = 1.65f, X0 = 0.12f, Top = 7.1f, RodZ = Top + 0.025f, SleeveR = 0.042f;          // khung cục bộ lưới cột: x ra ngoài, z lên, mặt cờ ⟂ y
        const int TW = 620, TH = 1024;

        [MenuItem("Phố Việt/Bản đồ mở/19. Cờ đỏ sao vàng bay trong gió trên cột đèn (21.298, scene đang mở)")]
        public static void Menu() => Debug.Log(Run());

        public static string Run()
        {
            Directory.CreateDirectory(Dir);
            var tex = BuildTexture();
            var cloth = BuildCloth();
            var rods = BuildRods();
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Dir + "/OW_CoVN.mat");
            if (!mat) { mat = new Material(Shader.Find("PhoViet/OpenWorld/FlagWind")); AssetDatabase.CreateAsset(mat, Dir + "/OW_CoVN.mat"); }
            mat.shader = Shader.Find("PhoViet/OpenWorld/FlagWind");
            mat.SetTexture("_MainTex", tex); mat.SetVector("_Size", new Vector4(W, H, 0, 0)); mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
            var metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/OpenWorld/Materials/OW_MetalGray.mat");
            var lampBulb = AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/OpenWorld/Materials/OW_LampBulb.mat");

            // lưới cột không cờ (cùng street_lamp) lấy từ thư viện OW_Mau_CotDen.fbx
            Mesh plain = null;
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/PhoViet/OpenWorld/Library/OW_Mau_CotDen.fbx")) if (o is Mesh m) { plain = m; break; }
            if (!plain) return "Không tìm thấy lưới OW_Mau_CotDen";

            int n = 0, fixedOld = 0;
            foreach (var lg in Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Include))
            {
                if (lg.name != "OW_Mau_CotDen_Co") continue;
                var go = lg.gameObject; var mf = go.GetComponent<MeshFilter>(); var mr = go.GetComponent<MeshRenderer>();
                if (!mf || !mr) continue;
                if (mf.sharedMesh != plain) { mf.sharedMesh = plain; mr.sharedMaterials = new[] { metal, lampBulb }; fixedOld++; }
                var cloth0 = Child(go.transform, "CoVN", cloth, mat, mr);
                var rod0 = Child(go.transform, "CoVN_Thanh", rods, metal, mr);
                var lods = lg.GetLODs();
                lods[0].renderers = new Renderer[] { mr, cloth0, rod0 };
                lg.SetLODs(lods); lg.RecalculateBounds();
                EditorUtility.SetDirty(go); n++;
            }
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Cờ mới: " + n + " cột (đổi lưới cột " + fixedOld + "), vải " + cloth.vertexCount + " đỉnh, ảnh " + TW + "×" + TH;
        }

        static Renderer Child(Transform parent, string name, Mesh mesh, Material mat, MeshRenderer like)
        {
            var t = parent.Find(name);
            if (!t) { t = new GameObject(name).transform; t.SetParent(parent, false); }
            t.localPosition = Vector3.zero; t.localRotation = Quaternion.identity; t.localScale = Vector3.one;
            t.gameObject.layer = parent.gameObject.layer;
            var mf = t.GetComponent<MeshFilter>(); if (!mf) mf = t.gameObject.AddComponent<MeshFilter>(); mf.sharedMesh = mesh;
            var mr = t.GetComponent<MeshRenderer>(); if (!mr) mr = t.gameObject.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat;
            mr.shadowCastingMode = like.shadowCastingMode; mr.receiveShadows = like.receiveShadows;
            mr.lightProbeUsage = like.lightProbeUsage; mr.reflectionProbeUsage = like.reflectionProbeUsage;
            // không đánh dấu tĩnh: vải chuyển động bằng shader; occlusion động vẫn loại khi khuất
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
            return mr;
        }

        // ------------------------------------------------------------ ảnh vải
        static Texture2D BuildTexture()
        {
            var px = new Color32[TW * TH];
            var red = new Color(0xDA / 255f, 0x25 / 255f, 0x1D / 255f); var yellow = new Color(1f, 0.87f, 0.0f);
            // sao: tâm giữa cờ, bán kính ngoại tiếp = H/5 (quy định: 1/5 chiều dài), một cánh hướng lên
            float R = H / 5f, r = R * 0.381966f; var cx = W * 0.5f; var cy = H * 0.5f;
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++) { float a = Mathf.PI / 2 + i * Mathf.PI / 5, rad = i % 2 == 0 ? R : r; pts[i] = new Vector2(cx + Mathf.Cos(a) * rad, cy + Mathf.Sin(a) * rad); }
            var rng = new System.Random(298);
            float[] rowN = new float[TH], colN = new float[TW];
            for (int i = 0; i < TH; i++) rowN[i] = (float)rng.NextDouble();
            for (int i = 0; i < TW; i++) colN[i] = (float)rng.NextDouble();
            for (int y = 0; y < TH; y++)
                for (int x = 0; x < TW; x++)
                {
                    // m (mét) trên tấm cờ: gốc dưới-trái (sát cột), y lên
                    float mx = (x + .5f) / TW * W, my = (y + .5f) / TH * H;
                    int hit = 0;
                    if (Mathf.Abs(mx - cx) < R + .01f && Mathf.Abs(my - cy) < R + .01f)
                    for (int sy = 0; sy < 4; sy++) for (int sx = 0; sx < 4; sx++)
                            if (InPoly(new Vector2((x + (sx + .5f) / 4f) / TW * W, (y + (sy + .5f) / 4f) / TH * H), pts)) hit++;
                    var c = Color.Lerp(red, yellow, hit / 16f);
                    // thớ dệt: sợi ngang + dọc lệch sáng nhẹ, xen kẽ ô 2 px
                    float weave = ((x + y) & 1) == 0 ? 1.02f : 0.98f;
                    weave *= 1f + (rowN[y] - .5f) * .05f + (colN[x] - .5f) * .05f;
                    // viền gấp mép 2,5 cm (dày hai lớp → tối hơn) + đường may
                    float edge = Mathf.Min(Mathf.Min(mx, W - mx), Mathf.Min(my, H - my));
                    float hem = edge < .025f ? .86f : 1f;
                    if (Mathf.Abs(edge - .03f) < .0022f) hem *= .8f;
                    // ống luồn sát cột 4 cm: tối + vạch may
                    if (mx < .04f) hem *= .78f + .1f * Mathf.Sin(my * 90f) * .5f;
                    // nếp gấp tĩnh rất nhẹ theo chiều dọc (vải treo lâu)
                    float fold = 1f + .035f * Mathf.Sin(mx * 13f + Mathf.Sin(my * 2.1f) * 1.3f);
                    c *= weave * hem * fold; c.a = 1;
                    px[y * TW + x] = c;
                }
            var t = new Texture2D(TW, TH, TextureFormat.RGBA32, false);
            t.SetPixels32(px); t.Apply();
            string path = Dir + "/OW_CoVN.png";
            File.WriteAllBytes(path, t.EncodeToPNG()); Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.sRGBTexture = true; imp.mipmapEnabled = true; imp.wrapMode = TextureWrapMode.Clamp; imp.anisoLevel = 2;
            imp.maxTextureSize = 1024; imp.textureCompression = TextureImporterCompression.CompressedHQ; imp.alphaSource = TextureImporterAlphaSource.None;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static bool InPoly(Vector2 p, Vector2[] v)
        {
            bool inside = false;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i++)
                if ((v[i].y > p.y) != (v[j].y > p.y) && p.x < (v[j].x - v[i].x) * (p.y - v[i].y) / (v[j].y - v[i].y) + v[i].x) inside = !inside;
            return inside;
        }

        // ------------------------------------------------------------ lưới vải
        static Mesh BuildCloth()
        {
            const int NX = 12, NY = 20;
            var v = new Vector3[(NX + 1) * (NY + 1)]; var uv = new Vector2[v.Length]; var nrm = new Vector3[v.Length]; var tan = new Vector4[v.Length];
            for (int j = 0; j <= NY; j++)
                for (int i = 0; i <= NX; i++)
                {
                    int k = j * (NX + 1) + i; float s = i / (float)NX, t = j / (float)NY;    // t: 0 trên → 1 dưới
                    v[k] = new Vector3(X0 + s * W, 0, Top - t * H);
                    uv[k] = new Vector2(s, 1 - t);
                    nrm[k] = new Vector3(0, -1, 0);
                    // tangent = +x (hướng s); cross(n, +x) = cross(−y, x) = +z (lên) → w = −1 để hướng t chỉ xuống
                    tan[k] = new Vector4(1, 0, 0, -1);
                }
            var tri = new int[NX * NY * 6]; int q = 0;
            for (int j = 0; j < NY; j++)
                for (int i = 0; i < NX; i++)
                {
                    int a = j * (NX + 1) + i, b = a + 1, c = a + NX + 1, d = c + 1;
                    tri[q++] = a; tri[q++] = c; tri[q++] = b; tri[q++] = b; tri[q++] = c; tri[q++] = d;
                }
            // 21.298b — ống luồn vải trên (người dùng: cờ lơ lửng, hở khe dưới thanh treo): mép trên cờ cuộn quanh thanh treo,
            // cùng vật liệu vải; uv.y = 1,05 (t < 0) → shader không lắc (bao biên = 0) và lấy màu hàng viền trên.
            var lv = new System.Collections.Generic.List<Vector3>(v); var luv = new System.Collections.Generic.List<Vector2>(uv);
            var ln = new System.Collections.Generic.List<Vector3>(nrm); var lt = new System.Collections.Generic.List<Vector4>(tan);
            var ltri = new System.Collections.Generic.List<int>(tri);
            const int SEG = 10; int sb = lv.Count;
            for (int i = 0; i <= NX; i++)
                for (int k = 0; k <= SEG; k++)
                {
                    float s = i / (float)NX, an = k * Mathf.PI * 2 / SEG;
                    var o = new Vector3(0, Mathf.Cos(an), Mathf.Sin(an));
                    lv.Add(new Vector3(X0 + s * W, 0, RodZ) + o * SleeveR); luv.Add(new Vector2(s, 1.05f)); ln.Add(o); lt.Add(new Vector4(1, 0, 0, -1));
                }
            for (int i = 0; i < NX; i++)
                for (int k = 0; k < SEG; k++)
                {
                    int a = sb + i * (SEG + 1) + k, b = a + 1, c = a + SEG + 1, d = c + 1;
                    ltri.AddRange(new[] { a, c, b, b, c, d });
                }
            var m = Save("OW_CoVN", new Mesh { vertices = lv.ToArray(), uv = luv.ToArray(), normals = ln.ToArray(), tangents = lt.ToArray(), triangles = ltri.ToArray() });
            // vùng bao rộng hơn để sóng không bị cắt khi loại khỏi khung nhìn
            m.bounds = new Bounds(new Vector3(X0 + W / 2, 0, Top - H / 2), new Vector3(W + .2f, .5f, H + .2f));
            EditorUtility.SetDirty(m);
            return m;
        }

        static Mesh BuildRods()
        {
            var vs = new System.Collections.Generic.List<Vector3>(); var ns = new System.Collections.Generic.List<Vector3>(); var ts = new System.Collections.Generic.List<int>();
            void Cyl(Vector3 a, Vector3 b, float rad, int seg)
            {
                var dir = (b - a).normalized; var u = Vector3.Cross(dir, Mathf.Abs(dir.z) < .9f ? Vector3.forward : Vector3.right).normalized; var w = Vector3.Cross(dir, u);
                int baseI = vs.Count;
                for (int i = 0; i <= seg; i++)
                {
                    float an = i * Mathf.PI * 2 / seg; var o = u * Mathf.Cos(an) + w * Mathf.Sin(an);
                    vs.Add(a + o * rad); ns.Add(o); vs.Add(b + o * rad); ns.Add(o);
                }
                for (int i = 0; i < seg; i++) { int p = baseI + i * 2; ts.AddRange(new[] { p, p + 2, p + 1, p + 1, p + 2, p + 3 }); }
                // nắp đầu ngoài
                int cIdx = vs.Count; vs.Add(b); ns.Add(dir);
                for (int i = 0; i < seg; i++) { int p = baseI + i * 2 + 1; ts.AddRange(new[] { cIdx, p, p + 2 }); }
            }
            Cyl(new Vector3(0, 0, RodZ), new Vector3(X0 + W + .06f, 0, RodZ), .026f, 10);                  // thanh treo trên (xuyên ống vải)
            Cyl(new Vector3(X0 + W + .06f, 0, RodZ), new Vector3(X0 + W + .11f, 0, RodZ), .05f, 10);          // núm đầu thanh
            foreach (var z in new[] { Top - .45f, Top - 1.05f })                                              // khoen giữ mép cờ vào cột
                Cyl(new Vector3(0, 0, z), new Vector3(X0 + .01f, 0, z), .016f, 8);
            return Save("OW_CoVN_Thanh", new Mesh { vertices = vs.ToArray(), normals = ns.ToArray(), triangles = ts.ToArray() });
        }

        static Mesh Save(string name, Mesh m)
        {
            m.name = name; m.RecalculateBounds();
            string path = Dir + "/" + name + ".asset";
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            // 21.298b: CopySerialized không đẩy lại dữ liệu lên GPU (vẫn vẽ lưới cũ) → gán lại bằng API Mesh
            if (old)
            {
                old.Clear();
                old.vertices = m.vertices; old.normals = m.normals; old.uv = m.uv;
                if (m.tangents != null && m.tangents.Length == m.vertexCount) old.tangents = m.tangents;
                old.triangles = m.triangles; old.RecalculateBounds(); old.UploadMeshData(false);
                EditorUtility.SetDirty(old); Object.DestroyImmediate(m); return old;
            }
            AssetDatabase.CreateAsset(m, path); return m;
        }
    }
}
