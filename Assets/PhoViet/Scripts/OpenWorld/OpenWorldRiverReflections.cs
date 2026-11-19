using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.271 — trộn phản chiếu mặt sông theo từng điểm ảnh (thay cách Unity chọn probe cho cả renderer: mỗi khúc sông
    // 160 m lấy một probe nên chỗ hai khúc giáp nhau đổi tông thành đường ranh).
    // Probe con của object này (nướng sẵn, chế độ Custom) được xếp theo s = quãng dọc tim sông (cùng toạ độ uv.x của
    // lưới mặt sông). Mỗi khung chọn 4 probe liên tiếp quanh vị trí camera, đưa vào shader River (_OWRefl0..3 + vị trí,
    // hộp chiếu, s); điểm nước nội suy tuyến tính giữa hai probe kẹp s của nó, ngoài khoảng thì lấy probe đầu/cuối.
    [DefaultExecutionOrder(1000)]
    public sealed class OpenWorldRiverReflections : MonoBehaviour
    {
        // Tim sông (khớp build_river_surface.py): nhánh ngang z 314,25 từ x −1400, cung tâm (234,5; 231,5) R 82,75, nhánh dọc x 317,25.
        const float Far = -1400f, CX = 234.5f, CY = 231.5f, R = 82.75f;
        public static float SOf(float x, float z)
        {
            if (x < CX) return x - Far;
            float arcStart = CX - Far, arc = R * Mathf.PI / 2f;
            if (z >= CY) { float th = Mathf.Atan2(z - CY, x - CX); return arcStart + R * (Mathf.PI / 2f - Mathf.Clamp(th, 0f, Mathf.PI / 2f)); }
            return arcStart + arc + (CY - z);
        }

        ReflectionProbe[] probes; float[] sp;
        static readonly int OnId = Shader.PropertyToID("_OWReflOn"), SId = Shader.PropertyToID("_OWReflS");
        static readonly int[] TexId = { Shader.PropertyToID("_OWRefl0"), Shader.PropertyToID("_OWRefl1"), Shader.PropertyToID("_OWRefl2"), Shader.PropertyToID("_OWRefl3") };
        static readonly int[] PosId = { Shader.PropertyToID("_OWReflPos0"), Shader.PropertyToID("_OWReflPos1"), Shader.PropertyToID("_OWReflPos2"), Shader.PropertyToID("_OWReflPos3") };
        static readonly int[] MinId = { Shader.PropertyToID("_OWReflMin0"), Shader.PropertyToID("_OWReflMin1"), Shader.PropertyToID("_OWReflMin2"), Shader.PropertyToID("_OWReflMin3") };
        static readonly int[] MaxId = { Shader.PropertyToID("_OWReflMax0"), Shader.PropertyToID("_OWReflMax1"), Shader.PropertyToID("_OWReflMax2"), Shader.PropertyToID("_OWReflMax3") };
        public int WindowStart { get; private set; } = -1;

        // 21.279: thành phần ReflectionProbe phải TẮT (enabled = false). Probe bật làm Unity tự gán/trộn phản chiếu cho cả
        // ~28.000 renderer của bản đồ (mất GPU instancing, thêm ~560 MB và rò bộ nhớ trên WebKit → Safari iPhone sập).
        // Script chỉ đọc cubemap + vị trí + hộp của probe; mặt sông vẫn phản chiếu như cũ.
        Vector3[] bmin, bmax;

        void Awake()
        {
            foreach (var p in GetComponentsInChildren<ReflectionProbe>(true)) p.enabled = false;
        }

        void OnEnable()
        {
            var list = new List<ReflectionProbe>();
            foreach (var p in GetComponentsInChildren<ReflectionProbe>(true)) { p.enabled = false; if (p.customBakedTexture) list.Add(p); }
            list.Sort((a, b) => SOf(a.transform.position.x, a.transform.position.z).CompareTo(SOf(b.transform.position.x, b.transform.position.z)));
            probes = list.ToArray(); sp = new float[probes.Length];
            bmin = new Vector3[probes.Length]; bmax = new Vector3[probes.Length];
            for (int i = 0; i < probes.Length; i++)
            {
                var t = probes[i].transform; sp[i] = SOf(t.position.x, t.position.z);
                Vector3 c = t.position + probes[i].center, h = probes[i].size * 0.5f;   // hộp chiếu (probe trục thẳng, không xoay)
                bmin[i] = c - h; bmax[i] = c + h;
            }
            WindowStart = -1; Apply(Camera.main ? Camera.main.transform.position : Vector3.zero, true);
        }

        void OnDisable() { Shader.SetGlobalFloat(OnId, 0f); }

        void LateUpdate()
        {
            var cam = Camera.main; if (cam) Apply(cam.transform.position, false);
        }

        void Apply(Vector3 at, bool force)
        {
            if (probes == null || probes.Length < 4) { Shader.SetGlobalFloat(OnId, 0f); return; }
            float sc = SOf(at.x, at.z);
            int k = 0; float best = float.MaxValue;
            for (int i = 0; i < sp.Length; i++) { float d = Mathf.Abs(sp[i] - sc); if (d < best) { best = d; k = i; } }
            int start = Mathf.Clamp(sc >= sp[k] ? k - 1 : k - 2, 0, probes.Length - 4);   // 4 probe: 1 sau + 2 trước camera (hoặc ngược lại)
            if (!force && start == WindowStart) return;
            WindowStart = start;
            var sv = Vector4.zero;
            for (int j = 0; j < 4; j++)
            {
                var p = probes[start + j]; var c = p.transform.position;
                Shader.SetGlobalTexture(TexId[j], p.customBakedTexture);
                Shader.SetGlobalVector(PosId[j], c);
                Shader.SetGlobalVector(MinId[j], bmin[start + j]);
                Shader.SetGlobalVector(MaxId[j], bmax[start + j]);
                sv[j] = sp[start + j];
            }
            Shader.SetGlobalVector(SId, sv);
            Shader.SetGlobalFloat(OnId, 1f);
        }
    }
}
