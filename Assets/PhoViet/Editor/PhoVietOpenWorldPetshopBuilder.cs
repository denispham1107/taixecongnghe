using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.OpenWorldEditor
{
    // 21.285 — menu 13: «Cù Lao Petshop» cho 50 % nhà mặt tiền KHÔNG có biển (nhà ở), không hai tiệm Petshop liền kề.
    // • Nhà ở dùng mẫu OW_Mau_NhaPho_<w>_<v> (có ban công tầng 2 đúng chỗ bảng hiệu) → đổi lưới thân + mặt tiền kính sang mẫu
    //   cửa hàng OW_Mau_NhaPho_<w>_<v>_B cùng bề ngang/kiểu (chừa chỗ biển, mặt tiền kính kiểu tiệm). Căn đã rút thân (21.273,
    //   lưới *_Ngan) thì rút thân mẫu _B tương tự (Library/NhaNgan/<lưới>_Ngan.asset). Hộp LOD1 co theo chiều cao mới.
    // • Biển Library/OW_Mau_Bang_<w>_14.fbx (build_petshop_signs.py: «Cù Lao Petshop» chữ lớn + «Spa-Lưu Trú-Thú y» chữ nhỏ,
    //   icon dấu chân) đặt vào BanDoMo/BangHieu cùng vị trí/hướng như biển các tiệm khác.
    // • Nội thất Library/NoiThat/OW_NoiThat_PetShop.fbx (Blender MCP, ow_interiors.furnish_PetShop): NT_PetShop_<5_0|6_0|8_0>
    //   (3D) + _Xa (ảnh xa trong OW_NoiThat_PetShop_AnhXa.png) — cùng LOD như mọi căn (3D gần, ảnh xa, điện thoại chỉ ảnh xa).
    // • Chọn: duyệt từng dãy nhà theo thứ tự dọc phố, lấy xen kẽ căn nhà ở chưa kề Petshop nào; đủ 50 % thì dừng. Đánh dấu căn
    //   bằng hậu tố tên «_PetShop» → chạy lại (hoặc menu 10) chỉ gắn lại nội thất/biển, không chọn lại.
    public static class PhoVietOpenWorldPetshopBuilder
    {
        const string Root = "Assets/PhoViet/OpenWorld/";
        public const string Mark = "_PetShop";
        public const int SignIndex = 14;
        static readonly Regex HouseRx = new Regex(@"OW_Mau_NhaPho_(\d_\d)_(\d)(_B)?$");

        [MenuItem("Phố Việt/Bản đồ mở/13. Cù Lao Petshop cho 50 % nhà không biển (scene đang mở)")]
        public static void BuildMenu() => Debug.Log("[Bản đồ mở] " + Apply(GameObject.Find("BanDoMo"), true, true));

        static Material SignMat(string name, string hex, Material like)
        {
            string path = Root + "Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(like); AssetDatabase.CreateAsset(m, path); }
            if (ColorUtility.TryParseHtmlString(hex, out var c) && m.HasProperty("_Color")) m.color = c;
            EditorUtility.SetDirty(m); return m;
        }

        static void PrepareAssets(out Dictionary<string, Transform> nt, out Material interior, out Material interiorGlass, out Material far)
        {
            // vật liệu biển (cùng shader Triplanar như các biển cũ)
            var like = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/OW_SignBG_2B7A3B.mat");
            var mats = new Dictionary<string, Material>
            {
                ["OW_SignBG_16808A"] = SignMat("OW_SignBG_16808A", "#16808A", like),
                ["OW_PetshopPaw"] = SignMat("OW_PetshopPaw", "#F28C28", like),
                ["OW_FrameWhite"] = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/OW_FrameWhite.mat"),
                ["OW_SignTextWhite"] = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/OW_SignTextWhite.mat"),
            };
            AssetDatabase.SaveAssets();
            foreach (var w in new[] { "4_5", "5_0", "5_5", "6_0", "7_0", "8_0" })
            {
                string p = Root + "Library/OW_Mau_Bang_" + w + "_" + SignIndex + ".fbx";
                var imp = AssetImporter.GetAtPath(p) as ModelImporter; if (imp == null) throw new System.Exception("thiếu " + p);
                bool dirty = imp.importAnimation || imp.importCameras || imp.importLights;
                imp.importAnimation = false; imp.importCameras = false; imp.importLights = false;
                foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(p))
                {
                    if (!(o is Material mm)) continue;
                    if (mats.TryGetValue(PhoVietOpenWorldShopfrontBuilder.Strip(mm.name), out var target) && target)
                    { imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mm.name), target); dirty = true; }
                }
                if (dirty) imp.SaveAndReimport();
            }
            // nội thất
            string ip = Root + "Library/NoiThat/OW_NoiThat_PetShop.fbx";
            var ii = AssetImporter.GetAtPath(ip) as ModelImporter; if (ii == null) throw new System.Exception("thiếu " + ip);
            if (ii.materialImportMode != ModelImporterMaterialImportMode.None || ii.importAnimation || ii.addCollider)
            { ii.materialImportMode = ModelImporterMaterialImportMode.None; ii.importAnimation = false; ii.importCameras = false; ii.importLights = false; ii.addCollider = false; ii.SaveAndReimport(); }
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ip);
            nt = new Dictionary<string, Transform>(); foreach (Transform c in asset.transform) nt[PhoVietOpenWorldShopfrontBuilder.Strip(c.name)] = c;
            string ap = Root + "Library/NoiThat/OW_NoiThat_PetShop_AnhXa.png";
            var ti = AssetImporter.GetAtPath(ap) as TextureImporter;
            if (ti && (!ti.mipmapEnabled || ti.maxTextureSize != 2048 || ti.wrapMode != TextureWrapMode.Clamp)) { ti.mipmapEnabled = true; ti.maxTextureSize = 2048; ti.wrapMode = TextureWrapMode.Clamp; ti.SaveAndReimport(); }
            interior = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/OW_NoiThat.mat");
            interiorGlass = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/OW_NoiThatKinh.mat");
            far = PhoVietOpenWorldShopfrontBuilder.Mat("OW_NoiThatXaPetShop", "Unlit/Texture", null, m => m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ap));
            AssetDatabase.SaveAssets();
        }

        public static string Apply(GameObject map, bool select, bool save)
        {
            if (!map) return "mở scene PhoVietOpenWorld trước";
            if (Application.isPlaying) return "thoát Play trước";
            PrepareAssets(out var nt, out var interior, out var interiorGlass, out var far);
            var houses = map.transform.Find("NhaPho"); var signRoot = map.transform.Find("BangHieu");
            var hs = new List<Transform>(); foreach (Transform h in houses) hs.Add(h);
            int N = hs.Count; var W = new float[N]; var home = new bool[N]; var pet = new bool[N];
            for (int i = 0; i < N; i++)
            {
                var src = PrefabUtility.GetCorrespondingObjectFromSource(hs[i].gameObject);
                var m = src ? HouseRx.Match(src.name) : null;
                home[i] = m != null && m.Success && !m.Groups[3].Success;
                pet[i] = hs[i].name.EndsWith(Mark);
                W[i] = hs[i].GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
            }
            // láng giềng cùng dãy: cùng hướng, mặt tiền thẳng hàng (lệch sâu < 0,6 m), sát cạnh nhau theo bề ngang
            var grid = new Dictionary<Vector2Int, List<int>>();
            Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x / 20), Mathf.FloorToInt(p.z / 20));
            for (int i = 0; i < N; i++) { var k = Cell(hs[i].position); if (!grid.TryGetValue(k, out var l)) grid[k] = l = new List<int>(); l.Add(i); }
            var nb = new List<int>[N];
            for (int i = 0; i < N; i++)
            {
                nb[i] = new List<int>(); var k0 = Cell(hs[i].position);
                for (int dx = -1; dx <= 1; dx++) for (int dz = -1; dz <= 1; dz++)
                    if (grid.TryGetValue(new Vector2Int(k0.x + dx, k0.y + dz), out var l))
                        foreach (int j in l)
                        {
                            if (j == i || Mathf.Abs(Mathf.DeltaAngle(hs[i].eulerAngles.y, hs[j].eulerAngles.y)) > 5f) continue;
                            var lp = hs[i].InverseTransformPoint(hs[j].position);
                            if (Mathf.Abs(lp.x) < 0.6f && Mathf.Abs(lp.y) <= (W[i] + W[j]) / 2 + 0.6f) nb[i].Add(j);
                        }
            }
            int homes = 0; for (int i = 0; i < N; i++) if (home[i]) homes++;
            int target = Mathf.RoundToInt(homes * 0.5f), picked = 0, already = 0;
            for (int i = 0; i < N; i++) if (pet[i]) already++;
            if (select && already == 0)
            {
                // thứ tự dọc phố: nhóm theo hướng + vị trí ngang mặt tiền, rồi theo toạ độ dọc dãy
                var order = new List<int>(); for (int i = 0; i < N; i++) if (home[i]) order.Add(i);
                order.Sort((a, b) =>
                {
                    float ya = Mathf.Round(hs[a].eulerAngles.y / 5f), yb = Mathf.Round(hs[b].eulerAngles.y / 5f);
                    if (ya != yb) return ya.CompareTo(yb);
                    var ra = hs[a].right; float fa = Mathf.Round(Vector3.Dot(hs[a].position, ra)), fb = Mathf.Round(Vector3.Dot(hs[b].position, ra));
                    if (fa != fb) return fa.CompareTo(fb);
                    var ua = hs[a].up; return Vector3.Dot(hs[a].position, ua).CompareTo(Vector3.Dot(hs[b].position, ua));
                });
                foreach (int i in order)
                {
                    if (picked >= target) break;
                    bool ok = true; foreach (int j in nb[i]) if (pet[j]) { ok = false; break; }
                    if (!ok) continue;
                    pet[i] = true; picked++;
                }
            }
            // áp vào scene
            var signs = new Dictionary<string, GameObject>(); var bAssets = new Dictionary<string, GameObject>();
            var sample = signRoot.childCount > 0 ? signRoot.GetChild(0).gameObject : null;
            var shortCache = new Dictionary<Mesh, Mesh>();
            int converted = 0, signsMade = 0, interiors = 0, shortened = 0, adjacent = 0;
            for (int i = 0; i < N; i++)
            {
                if (!pet[i]) continue;
                var h = hs[i];
                var src = PrefabUtility.GetCorrespondingObjectFromSource(h.gameObject);
                var m = HouseRx.Match(src.name); string w = m.Groups[1].Value, v = m.Groups[2].Value;
                if (!h.name.EndsWith(Mark))
                {
                    if (!bAssets.TryGetValue(w + v, out var b)) bAssets[w + v] = b = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Library/OW_Mau_NhaPho_" + w + "_" + v + "_B.fbx");
                    var mf = h.GetComponent<MeshFilter>(); var oldMesh = mf.sharedMesh; float hOld = oldMesh.bounds.max.z;
                    var bMesh = b.GetComponent<MeshFilter>().sharedMesh;
                    if (oldMesh.name.EndsWith("_Ngan"))
                    {
                        if (!shortCache.TryGetValue(bMesh, out var sm))
                        {
                            string path = Root + "Library/NhaNgan/" + bMesh.name + "_Ngan.asset";
                            sm = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                            if (!sm)
                            {
                                sm = Object.Instantiate(bMesh); sm.name = bMesh.name + "_Ngan";
                                var vv = sm.vertices; for (int k = 0; k < vv.Length; k++) if (vv[k].x > PhoVietOpenWorldShopfrontBuilder.ShortDepth + 0.1f) vv[k].x = PhoVietOpenWorldShopfrontBuilder.ShortDepth;
                                sm.vertices = vv; sm.RecalculateBounds(); AssetDatabase.CreateAsset(sm, path);
                            }
                            shortCache[bMesh] = sm;
                        }
                        bMesh = sm; shortened++;
                    }
                    mf.sharedMesh = bMesh;
                    h.GetComponent<MeshRenderer>().sharedMaterials = b.GetComponent<MeshRenderer>().sharedMaterials;
                    var gNew = b.transform.Find("OW_MatTienKinh"); var gOld = h.Find("OW_MatTienKinh");
                    if (gNew && gOld)
                    {
                        gOld.GetComponent<MeshFilter>().sharedMesh = gNew.GetComponent<MeshFilter>().sharedMesh;
                        gOld.GetComponent<MeshRenderer>().sharedMaterials = gNew.GetComponent<MeshRenderer>().sharedMaterials;
                    }
                    float hNew = bMesh.bounds.max.z;
                    var lod1 = h.Find("LOD1_Hop");
                    if (lod1 && hOld > 0.1f) { float k = hNew / hOld; lod1.localPosition = new Vector3(lod1.localPosition.x, lod1.localPosition.y * k, lod1.localPosition.z); lod1.localScale = new Vector3(lod1.localScale.x, lod1.localScale.y * k, lod1.localScale.z); }
                    h.name += Mark; converted++;
                }
                // biển
                bool hasSign = false;
                foreach (Transform s in signRoot) if ((s.position - h.position).sqrMagnitude < 0.64f) { hasSign = true; break; }
                if (!hasSign)
                {
                    if (!signs.TryGetValue(w, out var sp)) signs[w] = sp = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Library/OW_Mau_Bang_" + w + "_" + SignIndex + ".fbx");
                    var s = (GameObject)PrefabUtility.InstantiatePrefab(sp, signRoot);
                    s.transform.SetPositionAndRotation(h.position, Quaternion.Euler(0, h.eulerAngles.y, 0));
                    if (sample) { GameObjectUtility.SetStaticEditorFlags(s, GameObjectUtility.GetStaticEditorFlags(sample)); s.layer = sample.layer; foreach (Transform c in s.transform) { GameObjectUtility.SetStaticEditorFlags(c.gameObject, GameObjectUtility.GetStaticEditorFlags(sample)); c.gameObject.layer = sample.layer; } }
                    signsMade++;
                }
                // nội thất PetShop
                var old = h.Find("NoiThat"); if (old) Object.DestroyImmediate(old.gameObject);
                float wf = float.Parse(w.Replace('_', '.'), System.Globalization.CultureInfo.InvariantCulture);
                string nk = PhoVietOpenWorldShopfrontBuilder.NtKey(wf);
                var S = Matrix4x4.Scale(new Vector3(1f, wf / PhoVietOpenWorldShopfrontBuilder.NtWidth(nk), 1f));
                if (!nt.TryGetValue("NT_PetShop_" + nk, out var t3) || !nt.TryGetValue("NT_PetShop_" + nk + "_Xa", out var tx)) continue;
                var go = new GameObject("NoiThat"); go.transform.SetParent(h, false);
                Matrix4x4 inv = src.transform.localToWorldMatrix.inverse;
                var r3 = PhoVietOpenWorldShopfrontBuilder.Child(go.transform, "3D", t3, inv, interior, interiorGlass); PhoVietOpenWorldShopfrontBuilder.SetLocal(r3.transform, S * (inv * t3.localToWorldMatrix));
                var rx = PhoVietOpenWorldShopfrontBuilder.Child(go.transform, "Xa", tx, inv, far, null); PhoVietOpenWorldShopfrontBuilder.SetLocal(rx.transform, S * (inv * tx.localToWorldMatrix));
                var lg = go.AddComponent<LODGroup>();
                lg.SetLODs(new[] { new LOD(0.17f, new Renderer[] { r3 }), new LOD(0.02f, new Renderer[] { rx }) });
                lg.RecalculateBounds(); interiors++;
                var hl = h.GetComponent<LODGroup>(); if (hl) hl.RecalculateBounds();
                foreach (int j in nb[i]) if (pet[j]) adjacent++;
            }
            int total = 0; for (int i = 0; i < N; i++) if (pet[i]) total++;
            if (save) { EditorSceneManager.MarkSceneDirty(map.scene); EditorSceneManager.SaveScene(map.scene); }
            else EditorSceneManager.MarkSceneDirty(map.scene);
            return $"Cù Lao Petshop: {total}/{homes} nhà không biển ({100f * total / Mathf.Max(1, homes):0.0} %), mới chọn {picked}, đổi mẫu {converted} (rút thân {shortened}), biển mới {signsMade}, nội thất {interiors}, cặp liền kề {adjacent / 2}";
        }
    }
}
