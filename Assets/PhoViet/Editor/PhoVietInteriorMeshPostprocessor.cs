using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorldEditor
{
    // 21.272 — nén đỉnh mesh nội thất (Library/NoiThat/*.fbx) khi nhập: vị trí Float32×3, pháp tuyến SNorm8×4, màu UNorm8×4
    // = 20 byte/đỉnh (FBX nhập mặc định ~48 byte: pháp tuyến Float32×3 + màu Float32×4 + tiếp tuyến/UV). Tấm ảnh xa giữ UV.
    public sealed class PhoVietInteriorMeshPostprocessor : AssetPostprocessor
    {
        static bool IsCard(string n) => System.Text.RegularExpressions.Regex.Replace(n, @"\.\d{3}$", "").EndsWith("_Xa");

        // 21.372 — atlas ảnh xa HD (người dùng: mặt tiền cửa hàng trên điện thoại rất mờ): OW_NoiThat_AnhXa.png đổi từ lưới 10 × 6
        // (ô 192 × 144) sang lưới 7 × 8 (ô 584 × 438, 4088 × 3504 ≤ 4096) — ArtSource/Blender/OpenWorld/ow_interior_atlas_hd.py.
        // FBX OW_NoiThat_5_0/6_0/8_0 xuất năm 21.272 mang UV lưới cũ → đổi sang ô cùng số thứ tự trong lưới mới. Chỉ đổi khi tấm
        // ảnh rộng đúng 1/10 (lưới cũ) nên FBX xuất lại sau này bằng build_interiors.py (đã đổi sang lưới 7 cột) không bị đổi hai lần.
        // Atlas nhà nổi bật (5 × 3) và PetShop (3 × 1) giữ lưới, chỉ tăng độ phân giải → không đổi UV.
        public const int OldCols = 10, OldRows = 6, NewCols = 7, NewRows = 8;
        void RemapCardUv(Mesh m)
        {
            var file = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (file != "OW_NoiThat_5_0" && file != "OW_NoiThat_6_0" && file != "OW_NoiThat_8_0") return;
            var uv = new System.Collections.Generic.List<Vector2>(); m.GetUVs(0, uv);
            if (uv.Count == 0) return;
            float u0 = 1, u1 = 0, v0 = 1, v1 = 0;
            foreach (var p in uv) { u0 = Mathf.Min(u0, p.x); u1 = Mathf.Max(u1, p.x); v0 = Mathf.Min(v0, p.y); v1 = Mathf.Max(v1, p.y); }
            if (Mathf.Abs((u1 - u0) - 1f / OldCols) > .004f || Mathf.Abs((v1 - v0) - 1f / OldRows) > .004f) return;   // không phải lưới cũ
            int cx = Mathf.FloorToInt((u0 + u1) * .5f * OldCols), cy = Mathf.FloorToInt((1f - (v0 + v1) * .5f) * OldRows);
            int idx = cy * OldCols + cx, nx = idx % NewCols, ny = idx / NewCols;
            for (int i = 0; i < uv.Count; i++)
            {
                float fu = uv[i].x * OldCols - cx, fv = (1f - uv[i].y) * OldRows - cy;
                uv[i] = new Vector2((nx + fu) / NewCols, 1f - (ny + fv) / NewRows);
            }
            m.SetUVs(0, uv);
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.Replace('\\', '/').Contains("/OpenWorld/Library/NoiThat/")) return;
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var m = mf.sharedMesh;
                // 21.283: Blender đặt hậu tố .001/.002 cho tấm ảnh xa ở bộ 6 m/8 m (NT_TapHoa_Xa.001) → trước đây không khớp
                // EndsWith("_Xa"), tấm ảnh bị nén mất UV, chỉ hiện một màu nâu (gói điện thoại chỉ có ảnh xa). Bỏ hậu tố trước khi so.
                if (!m) continue;
                if (IsCard(m.name) || IsCard(mf.name)) { RemapCardUv(m); continue; }
                var pos = m.vertices; var nrm = m.normals; var col = m.colors32;
                int n = pos.Length; bool hasCol = col != null && col.Length == n; bool hasNrm = nrm != null && nrm.Length == n;
                var subs = new int[m.subMeshCount][];
                for (int s = 0; s < subs.Length; s++) subs[s] = m.GetTriangles(s);
                var data = new byte[n * 20];
                for (int i = 0; i < n; i++)
                {
                    int o = i * 20;
                    System.Buffer.BlockCopy(System.BitConverter.GetBytes(pos[i].x), 0, data, o, 4);
                    System.Buffer.BlockCopy(System.BitConverter.GetBytes(pos[i].y), 0, data, o + 4, 4);
                    System.Buffer.BlockCopy(System.BitConverter.GetBytes(pos[i].z), 0, data, o + 8, 4);
                    var v = hasNrm ? nrm[i] : Vector3.up;
                    data[o + 12] = (byte)(sbyte)Mathf.RoundToInt(Mathf.Clamp(v.x, -1, 1) * 127);
                    data[o + 13] = (byte)(sbyte)Mathf.RoundToInt(Mathf.Clamp(v.y, -1, 1) * 127);
                    data[o + 14] = (byte)(sbyte)Mathf.RoundToInt(Mathf.Clamp(v.z, -1, 1) * 127);
                    data[o + 15] = 0;
                    var c = hasCol ? col[i] : new Color32(200, 200, 200, 0);
                    data[o + 16] = c.r; data[o + 17] = c.g; data[o + 18] = c.b; data[o + 19] = c.a;
                }
                var bounds = m.bounds;
                m.Clear();
                m.SetVertexBufferParams(n,
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm8, 4),
                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4));
                m.SetVertexBufferData(data, 0, 0, data.Length);
                m.indexFormat = n > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                m.subMeshCount = subs.Length;
                for (int s = 0; s < subs.Length; s++) m.SetTriangles(subs[s], s, false);
                m.bounds = bounds;
            }
        }
    }
}
