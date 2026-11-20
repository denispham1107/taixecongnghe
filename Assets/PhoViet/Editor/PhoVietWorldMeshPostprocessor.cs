using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorldEditor
{
    // 21.280 — nén đỉnh mesh thế giới của bản đồ mở khi nhập (Chunks/OW_Map_*.fbx và 48 nhà mẫu Library/OW_Mau_NhaPho_*.fbx).
    // Các mesh này chỉ vẽ bằng PhoViet/OpenWorld/Triplanar, ShopGlass (và Unlit/Color, RiverWaterMirror ở OW_Map_river) — các shader
    // chỉ đọc VỊ TRÍ + PHÁP TUYẾN (vân lấy theo toạ độ thế giới). FBX nhập mặc định ~52 byte/đỉnh (pháp tuyến Float32×3, tiếp tuyến
    // Float32×4, UV, màu) → còn 16 byte: vị trí Float32×3 (giữ nguyên chính xác) + pháp tuyến SNorm8×4 (sai < 0,5°).
    // Hình học, số tam giác, texture không đổi; giảm bộ nhớ GPU, heap lúc nạp và gói dữ liệu (Safari iPhone sập vì bộ nhớ, 21.279–21.280).
    // Mesh nhà đã sao chép thành .asset (Library/NhaNgan, NhaNoiBat) được nén bằng Repack() từ AgentScripts/RepackWorldMeshes21280.cs.
    public sealed class PhoVietWorldMeshPostprocessor : AssetPostprocessor
    {
        public override uint GetVersion() => 2;

        public static bool Applies(string path)
        {
            path = path.Replace('\\', '/');
            return (path.Contains("/OpenWorld/Chunks/OW_Map_") || path.Contains("/OpenWorld/Library/OW_Mau_NhaPho_")) && path.EndsWith(".fbx");
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!Applies(assetPath)) return;
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!mf.sharedMesh) continue;
                Repack(mf.sharedMesh);
                var r = mf.GetComponent<MeshRenderer>(); if (r) StripAirCon(mf.sharedMesh, r.sharedMaterials);
            }
        }

        // 21.284 (người dùng: «khối vuông chữ nhật màu trắng trên bảng hiệu, ban công, tường… xóa đi»): cục điều hòa (vật liệu
        // OW_AirCon) dựng sẵn trong mặt tiền nhà — ở tầng trệt nó đè lên bảng hiệu. Làm rỗng submesh đó (giữ số submesh để vật
        // liệu các phần khác không lệch). Nhà .asset (NhaNgan/NhaNoiBat) xử lý bằng AgentScripts/RemoveAirCon21284.cs.
        public static int StripAirCon(Mesh m, Material[] mats)
        {
            int n = 0;
            for (int s = 0; s < m.subMeshCount && s < mats.Length; s++)
            {
                if (!mats[s] || !mats[s].name.StartsWith("OW_AirCon") || m.GetIndexCount(s) == 0) continue;
                m.SetIndices(new int[0], m.GetTopology(s), s, false); n++;
            }
            return n;
        }

        public static bool Repack(Mesh m)
        {
            if (m.vertexCount == 0) return false;   // mesh rỗng (vd. OW_Duong_*_DoViaHe): ghi 0 đỉnh ném lỗi và làm dừng cả file
            if (m.GetVertexAttributes().Length == 2 && m.GetVertexAttributeFormat(VertexAttribute.Normal) == VertexAttributeFormat.SNorm8) return false;
            // Lúc nhập (và trong Editor) dữ liệu vẫn đọc được dù cờ Read/Write tắt; không có đỉnh = không đọc được → bỏ qua.
            var pos = m.vertices; var nrm = m.normals; int n = pos.Length; bool hasNrm = nrm != null && nrm.Length == n;
            if (n == 0 && m.vertexCount > 0) { Debug.LogWarning("[Phố Việt] không đọc được đỉnh, bỏ qua nén: " + m.name); return false; }
            var subs = new int[m.subMeshCount][]; var topo = new MeshTopology[m.subMeshCount];
            for (int s = 0; s < subs.Length; s++) { subs[s] = m.GetIndices(s); topo[s] = m.GetTopology(s); }
            var data = new byte[n * 16];
            for (int i = 0; i < n; i++)
            {
                int o = i * 16;
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(pos[i].x), 0, data, o, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(pos[i].y), 0, data, o + 4, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(pos[i].z), 0, data, o + 8, 4);
                var v = hasNrm ? nrm[i] : Vector3.up;
                data[o + 12] = (byte)(sbyte)Mathf.RoundToInt(Mathf.Clamp(v.x, -1, 1) * 127);
                data[o + 13] = (byte)(sbyte)Mathf.RoundToInt(Mathf.Clamp(v.y, -1, 1) * 127);
                data[o + 14] = (byte)(sbyte)Mathf.RoundToInt(Mathf.Clamp(v.z, -1, 1) * 127);
                data[o + 15] = 0;
            }
            var bounds = m.bounds;
            m.Clear();
            m.SetVertexBufferParams(n,
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm8, 4));
            m.SetVertexBufferData(data, 0, 0, data.Length);
            m.indexFormat = n > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            m.subMeshCount = subs.Length;
            for (int s = 0; s < subs.Length; s++) m.SetIndices(subs[s], topo[s], s, false);
            m.bounds = bounds;
            return true;
        }
    }
}
