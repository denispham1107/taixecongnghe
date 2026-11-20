using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.Editor
{
    // 21.292 — sửa khối hộp nhìn từ xa (LOD1_Hop) của 3.283 nhà phố bản đồ mở (người dùng 21/11/2026, 3 ảnh):
    //  (1) «gần tới giao lộ nhìn từ xa luôn thấy một bức tường nằm ngang lấn ra đường, xe tới gần mới biến mất»;
    //  (2) «chế độ Yếu có mảng tường lớn che hết cửa kính cửa hàng, Trung bình ngắn hơn, tới gần thì hiện kính».
    // Nguyên nhân: BuildingLOD (PhoVietOpenWorldBuilder, 21.250) đặt hộp đơn vị theo khung bao tính ở hệ trục y-lên, nhưng
    // nhà phố trong scene có trục cục bộ x = sâu, y = ngang, z = lên → hộp NẰM NGHIÊNG: cao 3 m (nửa dưới đất), dài 18 m dọc
    // phố (thò sang 2–3 căn kế, ra tới giao lộ), mặt trước đứng ngay trước cửa kính. Mức Yếu/Trung bình hạ lodBias nên hộp
    // thay nhà thật sớm hơn → thấy rõ hơn (ghi chú «nghi bị đảo trục» ở 21.287 chính là lỗi này).
    // Sửa: mỗi căn một lưới hộp ĐÚNG khối nhà lấy từ lưới LOD0 (x 0 → sâu thân, ngang đúng bề rộng, cao tới mái) và
    // CHỪA tầng trệt: mặt trước tầng trệt lùi ra sau kính (sau mặt kính OW_MatTienKinh + 8 cm) tới đỉnh kính → từ xa vẫn
    // thấy kính cửa hàng + ảnh nội thất xa (NoiThat/Xa) như ở gần. Lưới dùng chung theo kích thước (thư mục Library/HopXa),
    // chỉ vị trí + pháp tuyến (shader Triplanar), vật liệu mặt tiền giữ nguyên. Sau khi chạy: nướng lại occlusion (menu 12)
    // vì hộp là OccludeeStatic.
    public static class PhoVietOpenWorldFarBoxBuilder
    {
        const string Dir = "Assets/PhoViet/OpenWorld/Library/HopXa";

        [MenuItem("Phố Việt/Bản đồ mở/16. Hộp nhà phố ở xa đúng khối + chừa kính tầng trệt (21.292, scene đang mở)")]
        public static void Menu() => Debug.Log(Run());

        public static string Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            Transform np = null;
            foreach (var g in scene.GetRootGameObjects()) if (g.name == "BanDoMo") np = g.transform.Find("NhaPho");
            if (!np) return "không thấy BanDoMo/NhaPho";
            if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
            var cache = new Dictionary<string, Mesh>();
            int fixedN = 0, notch = 0, skipped = 0, created = 0;
            foreach (Transform h in np)
            {
                var hop = h.Find("LOD1_Hop"); var mf = h.GetComponent<MeshFilter>();
                if (!hop || !mf || !mf.sharedMesh) { skipped++; continue; }
                var b = mf.sharedMesh.bounds;                                   // hệ cục bộ căn nhà: x sâu, y ngang, z lên
                float x0 = Mathf.Max(0f, b.min.x), d = b.max.x, y0 = b.min.y, y1 = b.max.y, z0 = b.min.z, z1 = b.max.z;
                float r = 0f, gh = z0;
                var kinh = h.Find("OW_MatTienKinh");
                var kmf = kinh ? kinh.GetComponent<MeshFilter>() : null;
                if (kmf && kmf.sharedMesh)
                {
                    var m = h.worldToLocalMatrix * kinh.localToWorldMatrix; var kb = kmf.sharedMesh.bounds;
                    Vector3 mx = Vector3.negativeInfinity;
                    for (int i = 0; i < 8; i++) mx = Vector3.Max(mx, m.MultiplyPoint3x4(kb.center + Vector3.Scale(kb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
                    if (mx.x > x0 && mx.x < 2f && mx.z > z0 + 1f && mx.z < z1 - 1f) { r = mx.x + 0.08f; gh = mx.z + 0.05f; notch++; }
                }
                string key = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F2}_{1:F2}_{2:F2}_{3:F2}_{4:F2}_{5:F2}_{6:F2}_{7:F2}", x0, d, y0, y1, z0, z1, r, gh);
                if (!cache.TryGetValue(key, out var mesh))
                {
                    string path = Dir + "/OW_HopXa_" + key.Replace('.', 'p').Replace('-', 'm') + ".asset";
                    mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (!mesh) { mesh = Box(x0, d, y0, y1, z0, z1, r, gh); mesh.name = Path.GetFileNameWithoutExtension(path); AssetDatabase.CreateAsset(mesh, path); created++; }
                    cache[key] = mesh;
                }
                Undo.RecordObject(hop, "far box"); Undo.RecordObject(hop.GetComponent<MeshFilter>(), "far box");
                hop.localPosition = Vector3.zero; hop.localRotation = Quaternion.identity; hop.localScale = Vector3.one;
                hop.GetComponent<MeshFilter>().sharedMesh = mesh;
                var lg = h.GetComponent<LODGroup>(); if (lg) lg.RecalculateBounds();
                fixedN++;
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            return $"hộp xa: sửa {fixedN} căn (chừa kính tầng trệt {notch}), lưới mới {created}, dùng chung {cache.Count}, bỏ qua {skipped}";
        }

        // Hộp kín (không đáy) có hốc tầng trệt phía trước: mặt trước x = x0 từ gh lên z1, mặt hốc x = r từ z0 tới gh, trần hốc.
        // Mặt trước nhà quay về −x (phố), thân kéo về +x.
        static Mesh Box(float x0, float d, float y0, float y1, float z0, float z1, float r, float gh)
        {
            var v = new List<Vector3>(); var n = new List<Vector3>(); var t = new List<int>();
            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 e, Vector3 nn)
            {
                int i = v.Count; v.Add(a); v.Add(b); v.Add(c); v.Add(e); for (int k = 0; k < 4; k++) n.Add(nn);
                // hướng tam giác theo pháp tuyến
                var cross = Vector3.Cross(b - a, c - a);
                if (Vector3.Dot(cross, nn) > 0) { t.Add(i); t.Add(i + 1); t.Add(i + 2); t.Add(i); t.Add(i + 2); t.Add(i + 3); }
                else { t.Add(i); t.Add(i + 2); t.Add(i + 1); t.Add(i); t.Add(i + 3); t.Add(i + 2); }
            }
            bool hasNotch = r > x0 + 0.01f && gh > z0 + 0.01f;
            float fz = hasNotch ? gh : z0;
            // mặt trước phần trên
            Quad(new Vector3(x0, y0, fz), new Vector3(x0, y1, fz), new Vector3(x0, y1, z1), new Vector3(x0, y0, z1), Vector3.left);
            if (hasNotch)
            {
                Quad(new Vector3(r, y0, z0), new Vector3(r, y1, z0), new Vector3(r, y1, gh), new Vector3(r, y0, gh), Vector3.left);   // mặt hốc (sau kính)
                Quad(new Vector3(x0, y0, gh), new Vector3(r, y0, gh), new Vector3(r, y1, gh), new Vector3(x0, y1, gh), Vector3.back);  // trần hốc (nhìn từ dưới)
                // hai má hốc (hông nhà ở tầng trệt, phần trước mặt hốc)
                Quad(new Vector3(x0, y0, z0), new Vector3(r, y0, z0), new Vector3(r, y0, gh), new Vector3(x0, y0, gh), Vector3.down);
                Quad(new Vector3(x0, y1, z0), new Vector3(r, y1, z0), new Vector3(r, y1, gh), new Vector3(x0, y1, gh), Vector3.up);
            }
            float sx = hasNotch ? r : x0;
            // hai hông (phần sau hốc chạy suốt; phần trên hốc từ x0)
            Quad(new Vector3(sx, y0, z0), new Vector3(d, y0, z0), new Vector3(d, y0, z1), new Vector3(sx, y0, z1), Vector3.down);
            Quad(new Vector3(sx, y1, z0), new Vector3(d, y1, z0), new Vector3(d, y1, z1), new Vector3(sx, y1, z1), Vector3.up);
            if (hasNotch)
            {
                Quad(new Vector3(x0, y0, gh), new Vector3(sx, y0, gh), new Vector3(sx, y0, z1), new Vector3(x0, y0, z1), Vector3.down);
                Quad(new Vector3(x0, y1, gh), new Vector3(sx, y1, gh), new Vector3(sx, y1, z1), new Vector3(x0, y1, z1), Vector3.up);
            }
            Quad(new Vector3(d, y0, z0), new Vector3(d, y1, z0), new Vector3(d, y1, z1), new Vector3(d, y0, z1), Vector3.right);       // mặt hậu
            Quad(new Vector3(x0, y0, z1), new Vector3(d, y0, z1), new Vector3(d, y1, z1), new Vector3(x0, y1, z1), Vector3.forward);    // mái
            var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetNormals(n); mesh.SetTriangles(t, 0); mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
            return mesh;
        }
    }
}
