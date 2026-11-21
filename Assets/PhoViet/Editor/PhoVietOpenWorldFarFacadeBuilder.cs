using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.Editor
{
    // 21.294 — người dùng 21/11/2026 (3 ảnh điện thoại): nhìn từ xa (dọc phố, bên kia sông) nhà phố chỉ còn «hộp tường» che hết
    // mặt tiền; phải luôn thấy rõ mặt tiền ở MỌI cấu hình. Nguyên nhân: hộp xa LOD1_Hop (21.250/21.292) phủ vật liệu tường trơn
    // OW_Facade_* (Triplanar) — không cửa sổ, ban công, cây; LOD0 (8.111 tam giác/căn, 15 vật liệu) ×3.283 căn quá nặng để giữ ở xa.
    // Sửa: chụp ẢNH THẬT 5 mặt (trước, sau, 2 hông, mái) của từng mẫu nhà (93 tổ hợp lưới + vật liệu) bằng camera trực giao và
    // shader thay thế (màu gốc, không ánh sáng), xếp vào MỘT atlas 2048², rồi dựng lại lưới hộp (đúng khối + hốc kính tầng trệt
    // như 21.292) có UV từng mặt trỏ vào ảnh tương ứng; vật liệu chung OW_HopXa_MatTien (shader PhoViet/OpenWorld/FarFacade,
    // Standard, GPU instancing). Hình khối hộp không đổi → không cần nướng lại occlusion (vẫn nên chạy menu 12 nếu đổi hình).
    public static class PhoVietOpenWorldFarFacadeBuilder
    {
        const string Dir = "Assets/PhoViet/OpenWorld/Library/HopXa/MatTien";
        const string AtlasPath = Dir + "/OW_HopXa_MatTien.png";
        const string MatPath = Dir + "/OW_HopXa_MatTien.mat";
        const string CaptureShader = "Assets/PhoViet/Editor/Shaders/FarFacadeCapture.shader";
        const int Layer = 31, Border = 4, AtlasSize = 2048;
        // điểm ảnh / mét: mặt trước rõ nhất (nhìn dọc phố, bên kia sông), sau/hông ít thấy hơn, mái nhìn từ trên.
        const float PpmFront = 14.5f, PpmBack = 4.5f, PpmSide = 4.5f, PpmTop = 3f;
        // hộp phẳng thiếu mặt nhô (ban công, lan can) bắt nắng → tăng nhẹ độ sáng cho khớp nhà thật khi chuyển LOD
        static readonly Color Gain = new Color(1.12f, 1.12f, 1.12f, 1f);

        [MenuItem("Phố Việt/Bản đồ mở/18. Mặt tiền ảnh cho hộp nhà phố ở xa (21.294, scene đang mở)")]
        public static void Menu() => Debug.Log(Run());

        class Spec
        {
            public string key; public Transform rep; public Mesh mesh; public Material[] mats;
            public float x0, d, y0, y1, z0, z1, r, gh;
            public View[] views; public Mesh box;
        }
        class View
        {
            public Vector3 fwd, up, right, c; public float w, h, ppm; public Texture2D tex; public Rect uv;
        }

        public static string Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            Transform np = null;
            foreach (var g in scene.GetRootGameObjects()) if (g.name == "BanDoMo") np = g.transform.Find("NhaPho");
            if (!np) return "không thấy BanDoMo/NhaPho";
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(CaptureShader);
            if (!shader) return "không thấy shader chụp " + CaptureShader;
            Directory.CreateDirectory(Dir);

            // 1) gom mẫu: lưới + vật liệu + thông số hộp (hốc kính lấy theo kính mặt tiền của từng căn như 21.292)
            var specs = new Dictionary<string, Spec>(); var houseSpec = new Dictionary<Transform, Spec>(); int skipped = 0;
            foreach (Transform h in np)
            {
                var hop = h.Find("LOD1_Hop"); var mf = h.GetComponent<MeshFilter>(); var mr = h.GetComponent<MeshRenderer>();
                if (!hop || !mf || !mf.sharedMesh || !mr) { skipped++; continue; }
                var b = mf.sharedMesh.bounds;
                float x0 = Mathf.Max(0f, b.min.x), r = 0f, gh = b.min.z;
                var kinh = h.Find("OW_MatTienKinh"); var kmf = kinh ? kinh.GetComponent<MeshFilter>() : null;
                if (kmf && kmf.sharedMesh)
                {
                    var m = h.worldToLocalMatrix * kinh.localToWorldMatrix; var kb = kmf.sharedMesh.bounds;
                    Vector3 mx = Vector3.negativeInfinity;
                    for (int i = 0; i < 8; i++) mx = Vector3.Max(mx, m.MultiplyPoint3x4(kb.center + Vector3.Scale(kb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
                    if (mx.x > x0 && mx.x < 2f && mx.z > b.min.z + 1f && mx.z < b.max.z - 1f) { r = mx.x + 0.08f; gh = mx.z + 0.05f; }
                }
                string key = mf.sharedMesh.name + "|" + string.Join(",", System.Array.ConvertAll(mr.sharedMaterials, x => x ? x.name : "-"))
                    + string.Format(System.Globalization.CultureInfo.InvariantCulture, "|{0:F2}_{1:F2}", r, gh);
                if (!specs.TryGetValue(key, out var s))
                {
                    s = new Spec { key = key, rep = h, mesh = mf.sharedMesh, mats = mr.sharedMaterials, x0 = x0, d = b.max.x, y0 = b.min.y, y1 = b.max.y, z0 = b.min.z, z1 = b.max.z, r = r, gh = gh };
                    specs[key] = s;
                }
                houseSpec[h] = s;
            }

            // 2) chụp 5 mặt mỗi mẫu
            var camGo = new GameObject("FarFacadeCam"); var cam = camGo.AddComponent<Camera>();
            cam.enabled = false; cam.orthographic = true; cam.cullingMask = 1 << Layer; cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0); cam.nearClipPlane = 0.01f; cam.farClipPlane = 200f; cam.allowMSAA = true;
            var all = new List<View>();
            try
            {
                foreach (var s in specs.Values)
                {
                    float cy = (s.y0 + s.y1) / 2, cz = (s.z0 + s.z1) / 2, cx = (s.x0 + s.d) / 2, W = s.y1 - s.y0, H = s.z1 - s.z0, D = s.d - s.x0;
                    s.views = new[]
                    {
                        new View { fwd = Vector3.right,   up = Vector3.forward, c = new Vector3(s.x0, cy, cz), w = W, h = H, ppm = PpmFront },  // trước (nhìn từ −x)
                        new View { fwd = Vector3.left,    up = Vector3.forward, c = new Vector3(s.d, cy, cz),  w = W, h = H, ppm = PpmBack },   // hậu
                        new View { fwd = Vector3.up,      up = Vector3.forward, c = new Vector3(cx, s.y0, cz), w = D, h = H, ppm = PpmSide },   // hông −y
                        new View { fwd = Vector3.down,    up = Vector3.forward, c = new Vector3(cx, s.y1, cz), w = D, h = H, ppm = PpmSide },   // hông +y
                        new View { fwd = Vector3.back,    up = Vector3.right,   c = new Vector3(cx, cy, s.z1), w = W, h = D, ppm = PpmTop },    // mái (nhìn từ trên)
                    };
                    var tmp = new GameObject("FarFacadeTmp"); tmp.layer = Layer;
                    tmp.transform.SetPositionAndRotation(new Vector3(0, -3000, 0), s.rep.rotation);
                    tmp.AddComponent<MeshFilter>().sharedMesh = s.mesh; tmp.AddComponent<MeshRenderer>().sharedMaterials = s.mats;
                    try { foreach (var v in s.views) { Capture(cam, tmp.transform, v, shader); all.Add(v); } }
                    finally { Object.DestroyImmediate(tmp); }
                }
            }
            finally { Object.DestroyImmediate(camGo); }

            // 3) atlas
            var atlas = new Texture2D(2, 2, TextureFormat.RGB24, true);
            var rects = atlas.PackTextures(all.ConvertAll(v => v.tex).ToArray(), 2, AtlasSize, false);
            float scale = rects[0].width * atlas.width / all[0].tex.width;
            for (int i = 0; i < all.Count; i++)
            {
                var v = all[i]; var rc = rects[i];
                float bx = Border / (float)v.tex.width * rc.width, by = Border / (float)v.tex.height * rc.height;
                v.uv = new Rect(rc.x + bx, rc.y + by, rc.width - 2 * bx, rc.height - 2 * by);
            }
            File.WriteAllBytes(AtlasPath, atlas.EncodeToPNG());
            int aw = atlas.width, ah = atlas.height;
            Object.DestroyImmediate(atlas); foreach (var v in all) Object.DestroyImmediate(v.tex);
            AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
            imp.sRGBTexture = true; imp.mipmapEnabled = true; imp.maxTextureSize = AtlasSize; imp.alphaSource = TextureImporterAlphaSource.None;
            imp.wrapMode = TextureWrapMode.Clamp; imp.anisoLevel = 4; imp.textureCompression = TextureImporterCompression.Compressed;
            imp.SaveAndReimport();

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
            if (!mat) { mat = new Material(Shader.Find("PhoViet/OpenWorld/FarFacade")); AssetDatabase.CreateAsset(mat, MatPath); }
            mat.shader = Shader.Find("PhoViet/OpenWorld/FarFacade");
            mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath));
            mat.SetColor("_Color", Gain); mat.SetFloat("_Glossiness", 0.12f); mat.enableInstancing = true; EditorUtility.SetDirty(mat);

            // 4) lưới hộp có UV
            foreach (var f in Directory.GetFiles(Dir, "OW_HopXaMT_*.asset")) AssetDatabase.DeleteAsset(f.Replace('\\', '/'));
            int n = 0;
            foreach (var s in specs.Values)
            {
                s.box = Box(s); s.box.name = "OW_HopXaMT_" + (n++).ToString("D3");
                AssetDatabase.CreateAsset(s.box, Dir + "/" + s.box.name + ".asset");
            }

            // 5) gán
            int fixedN = 0;
            foreach (var kv in houseSpec)
            {
                var hop = kv.Key.Find("LOD1_Hop");
                Undo.RecordObject(hop, "far facade");
                hop.localPosition = Vector3.zero; hop.localRotation = Quaternion.identity; hop.localScale = Vector3.one;
                var mf = hop.GetComponent<MeshFilter>(); var mr = hop.GetComponent<MeshRenderer>();
                Undo.RecordObject(mf, "far facade"); Undo.RecordObject(mr, "far facade");
                mf.sharedMesh = kv.Value.box; mr.sharedMaterial = mat;
                var lg = kv.Key.GetComponent<LODGroup>(); if (lg) lg.RecalculateBounds();
                fixedN++;
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return $"mặt tiền ảnh: {specs.Count} mẫu, {all.Count} ảnh, atlas {aw}×{ah} (tỉ lệ {scale:F2}), gán {fixedN} căn, bỏ qua {skipped}";
        }

        static void Capture(Camera cam, Transform t, View v, Shader shader)
        {
            int pw = Mathf.Max(4, Mathf.CeilToInt(v.w * v.ppm)), ph = Mathf.Max(4, Mathf.CeilToInt(v.h * v.ppm));
            var wf = t.TransformDirection(v.fwd); var wu = t.TransformDirection(v.up);
            cam.transform.SetPositionAndRotation(t.TransformPoint(v.c - v.fwd * 60f), Quaternion.LookRotation(wf, wu));
            v.right = t.InverseTransformDirection(cam.transform.right);
            cam.orthographicSize = v.h / 2f; cam.aspect = v.w / v.h;
            var rt = RenderTexture.GetTemporary(pw * 2, ph * 2, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 4);
            cam.targetTexture = rt; cam.RenderWithShader(shader, "");
            // thu nhỏ 2× (khử răng cưa) vào ảnh đúng cỡ
            var small = RenderTexture.GetTemporary(pw, ph, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Graphics.Blit(rt, small);
            var prev = RenderTexture.active; RenderTexture.active = small;
            var raw = new Texture2D(pw, ph, TextureFormat.RGBA32, false); raw.ReadPixels(new Rect(0, 0, pw, ph), 0, 0); raw.Apply();
            RenderTexture.active = prev; cam.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt); RenderTexture.ReleaseTemporary(small);
            var px = raw.GetPixels32(); Object.DestroyImmediate(raw);
            // nền trống (trên mái thấp, ngoài lan can) → màu trung bình phần có nhà; bán trong suốt ở mép → trộn
            long sr = 0, sg = 0, sb = 0, cnt = 0;
            foreach (var p in px) if (p.a > 250) { sr += p.r; sg += p.g; sb += p.b; cnt++; }
            var avg = cnt > 0 ? new Color32((byte)(sr / cnt), (byte)(sg / cnt), (byte)(sb / cnt), 255) : new Color32(170, 160, 150, 255);
            for (int i = 0; i < px.Length; i++)
            {
                var p = px[i]; if (p.a >= 255) continue;
                float a = p.a / 255f;   // màu đã nhân alpha khi trộn với nền (0,0,0,0)
                px[i] = new Color32((byte)Mathf.Min(255, p.r + avg.r * (1 - a)), (byte)Mathf.Min(255, p.g + avg.g * (1 - a)), (byte)Mathf.Min(255, p.b + avg.b * (1 - a)), 255);
            }
            // viền Border điểm ảnh lặp mép → không rỉ màu ô bên cạnh khi lọc/mipmap
            int tw = pw + 2 * Border, th = ph + 2 * Border; var o = new Color32[tw * th];
            for (int y = 0; y < th; y++)
            {
                int sy = Mathf.Clamp(y - Border, 0, ph - 1);
                for (int x = 0; x < tw; x++) o[y * tw + x] = px[sy * pw + Mathf.Clamp(x - Border, 0, pw - 1)];
            }
            v.tex = new Texture2D(tw, th, TextureFormat.RGBA32, false); v.tex.SetPixels32(o); v.tex.Apply();
        }

        // Hộp như 21.292 (đúng khối, hốc kính tầng trệt, không đáy) + UV theo hướng mặt.
        static Mesh Box(Spec s)
        {
            float x0 = s.x0, d = s.d, y0 = s.y0, y1 = s.y1, z0 = s.z0, z1 = s.z1, r = s.r, gh = s.gh;
            var v = new List<Vector3>(); var n = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
            Vector2 UV(Vector3 p, Vector3 nn)
            {
                View w = nn == Vector3.left || nn == Vector3.back ? s.views[0] : nn == Vector3.right ? s.views[1]
                       : nn == Vector3.down ? s.views[2] : nn == Vector3.up ? s.views[3] : s.views[4];
                var q = p - w.c;
                float u = Mathf.Clamp01(Vector3.Dot(q, w.right) / w.w + 0.5f), vv = Mathf.Clamp01(Vector3.Dot(q, w.up) / w.h + 0.5f);
                return new Vector2(w.uv.x + u * w.uv.width, w.uv.y + vv * w.uv.height);
            }
            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 e, Vector3 nn)
            {
                int i = v.Count; v.Add(a); v.Add(b); v.Add(c); v.Add(e);
                for (int k = 0; k < 4; k++) n.Add(nn);
                uv.Add(UV(a, nn)); uv.Add(UV(b, nn)); uv.Add(UV(c, nn)); uv.Add(UV(e, nn));
                var cross = Vector3.Cross(b - a, c - a);
                if (Vector3.Dot(cross, nn) > 0) { t.Add(i); t.Add(i + 1); t.Add(i + 2); t.Add(i); t.Add(i + 2); t.Add(i + 3); }
                else { t.Add(i); t.Add(i + 2); t.Add(i + 1); t.Add(i); t.Add(i + 3); t.Add(i + 2); }
            }
            bool hasNotch = r > x0 + 0.01f && gh > z0 + 0.01f;
            float fz = hasNotch ? gh : z0;
            Quad(new Vector3(x0, y0, fz), new Vector3(x0, y1, fz), new Vector3(x0, y1, z1), new Vector3(x0, y0, z1), Vector3.left);
            if (hasNotch)
            {
                Quad(new Vector3(r, y0, z0), new Vector3(r, y1, z0), new Vector3(r, y1, gh), new Vector3(r, y0, gh), Vector3.left);
                Quad(new Vector3(x0, y0, gh), new Vector3(r, y0, gh), new Vector3(r, y1, gh), new Vector3(x0, y1, gh), Vector3.back);
                Quad(new Vector3(x0, y0, z0), new Vector3(r, y0, z0), new Vector3(r, y0, gh), new Vector3(x0, y0, gh), Vector3.down);
                Quad(new Vector3(x0, y1, z0), new Vector3(r, y1, z0), new Vector3(r, y1, gh), new Vector3(x0, y1, gh), Vector3.up);
            }
            float sx = hasNotch ? r : x0;
            Quad(new Vector3(sx, y0, z0), new Vector3(d, y0, z0), new Vector3(d, y0, z1), new Vector3(sx, y0, z1), Vector3.down);
            Quad(new Vector3(sx, y1, z0), new Vector3(d, y1, z0), new Vector3(d, y1, z1), new Vector3(sx, y1, z1), Vector3.up);
            if (hasNotch)
            {
                Quad(new Vector3(x0, y0, gh), new Vector3(sx, y0, gh), new Vector3(sx, y0, z1), new Vector3(x0, y0, z1), Vector3.down);
                Quad(new Vector3(x0, y1, gh), new Vector3(sx, y1, gh), new Vector3(sx, y1, z1), new Vector3(x0, y1, z1), Vector3.up);
            }
            Quad(new Vector3(d, y0, z0), new Vector3(d, y1, z0), new Vector3(d, y1, z1), new Vector3(d, y0, z1), Vector3.right);
            Quad(new Vector3(x0, y0, z1), new Vector3(d, y0, z1), new Vector3(d, y1, z1), new Vector3(x0, y1, z1), Vector3.forward);
            var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetNormals(n); mesh.SetUVs(0, uv); mesh.SetTriangles(t, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents(); mesh.UploadMeshData(false);
            return mesh;
        }
    }
}
