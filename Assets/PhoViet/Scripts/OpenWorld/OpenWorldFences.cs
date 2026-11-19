using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhoViet.OpenWorld
{
    // 21.261 — hàng rào sắt thấp trước cửa hàng (lưới gộp OW_Duong_*_HangRao_* dựng trong Blender) húc đổ được.
    // • Builder (menu 8) đo từng đoạn rào trong lưới gộp (khe ≥ 0,12 m dọc trục) → danh sách `runs` (không thêm lưới vào asset).
    // • Lúc chạy: mỗi tấm rào có một vùng chạm trigger (layer Ignore Raycast). Xe người chơi, xe NPC (cả xác đang văng), xe CSGT
    //   chạm với vận tốc ≥ 0,8 m/s → đoạn lưới gộp đó ẩn đi, thay bằng từng tấm rào dựng bằng code (giống `_fence_segment`
    //   của Blender: 2 trụ, 2 thanh ngang, song sắt mỗi 0,12 m); tấm bị chạm thành vật thể vật lý bay/đổ theo hướng và vận tốc xe
    //   (chậm thì chỉ ngã ra), có tia lửa + mảnh sắt, rồi nằm yên ngẫu nhiên trên vỉa hè/lòng đường (tắt va chạm).
    // • Tấm rào đổ không cản xe người chơi/CSGT; vỉa hè (cao 0,15 m, chỉ có hình, không có collider) được thay bằng hộp ẩn layer 29
    //   chỉ va với tấm rào đổ (layer 30) để tấm nằm đúng mặt vỉa hè.
    public class OpenWorldFences : MonoBehaviour
    {
        [Serializable] public class Run
        {
            public MeshRenderer renderer;
            public Vector3 origin, dir;           // gốc + hướng dọc đoạn rào (mặt phẳng ngang)
            public float lateral, yMin, yMax;     // lệch ngang của đường rào so với gốc; đáy/đỉnh rào
            public Vector2[] segs;                // [t0, t1] từng tấm dọc trục
        }
        public List<Run> runs = new List<Run>();
        public OpenWorldTraffic traffic;
        public OpenWorldHazards hazards;
        public OpenWorldDriver player;
        public OpenWorldPolice police;
        public float sidewalkY = 0.15f, curbInset = 0.55f, minSpeed = 0.8f;

        public const int SidewalkLayer = 29, DownLayer = 30;
        // Các truy vấn vật lý khác của bản đồ mở bỏ qua hộp vỉa hè ẩn và tấm rào đổ.
        // 21.274: thêm layer MatLe (28, mặt lề chỉ để nâng hình xe — OpenWorldGroundFit).
        public const int WorldMask = ~((1 << SidewalkLayer) | (1 << DownLayer) | (1 << OpenWorldGroundFit.SurfaceLayer));

        class Panel
        {
            public OpenWorldFencePanel comp; public BoxCollider col; public Rigidbody rb; public MeshRenderer mr;
            public int run, seg; public float len, h; public bool down, settled; public float downAt;
        }
        class RunState { public bool broken; public Panel[] panels; public Vector3 outward; public GameObject sidewalk; }
        RunState[] state;
        readonly List<Panel> flying = new List<Panel>();
        readonly Dictionary<int, Mesh> meshCache = new Dictionary<int, Mesh>();
        Collider[] playerCols, policeCols;

        public int Knocked { get; private set; }
        public int RunsBroken { get; private set; }
        public int PanelCount { get; private set; }
        public int RailsOff { get; private set; }
        public string LastKnock { get; private set; }

        void Awake()
        {
            for (int l = 0; l < 32; l++) Physics.IgnoreLayerCollision(SidewalkLayer, l, l != DownLayer);
        }

        System.Collections.IEnumerator Start()
        {
            yield return null;                                   // chờ OpenWorldTraffic dựng làn xe
            if (!traffic) traffic = FindAnyObjectByType<OpenWorldTraffic>();
            if (!hazards) hazards = FindAnyObjectByType<OpenWorldHazards>();
            if (!player) player = FindAnyObjectByType<OpenWorldDriver>();
            if (!police) police = FindAnyObjectByType<OpenWorldPolice>();
            playerCols = player ? player.GetComponentsInChildren<Collider>(true) : new Collider[0];
            policeCols = police ? police.GetComponentsInChildren<Collider>(true) : new Collider[0];
            // lan can bờ sông trùng làn xe (đầu cầu, chỗ đường cắt qua bờ) → tắt hộp để không chặn đường
            var rails = transform.Find("LanCanSong");
            if (rails && traffic)
                foreach (var bc in rails.GetComponentsInChildren<BoxCollider>())
                {
                    for (int k = 0; k <= 10; k++)
                    {
                        var pt = bc.transform.TransformPoint(bc.center + new Vector3(0, 0, bc.size.z * (k / 10f - 0.5f)));
                        if (traffic.NearestLanePoint(pt, out var lq)) { var d = lq - pt; d.y = 0; if (d.magnitude < 1.6f) { bc.enabled = false; RailsOff++; break; } }
                    }
                }
            state = new RunState[runs.Count];
            for (int r = 0; r < runs.Count; r++)
            {
                var R = runs[r]; var st = state[r] = new RunState();
                var perp = new Vector3(R.dir.z, 0, -R.dir.x);
                var mid = R.origin + R.dir * ((R.segs.Length > 0 ? (R.segs[0].x + R.segs[R.segs.Length - 1].y) / 2f : 0f)) + perp * R.lateral;
                // phía vỉa hè = phía xa làn xe gần nhất
                float sgn = 1f;
                if (traffic && traffic.NearestLanePoint(mid, out var q)) sgn = Vector3.Dot(mid - q, perp) >= 0 ? 1f : -1f;
                st.outward = perp * sgn;
                st.panels = new Panel[R.segs.Length];
                for (int i = 0; i < R.segs.Length; i++)
                {
                    float t0 = R.segs[i].x, t1 = R.segs[i].y, len = Mathf.Max(0.2f, t1 - t0), h = Mathf.Max(0.5f, R.yMax - R.yMin);
                    var go = new GameObject("RaoChan"); go.layer = 2; go.transform.SetParent(transform, false);
                    var p = R.origin + R.dir * ((t0 + t1) / 2f) + perp * R.lateral; p.y = R.yMin;
                    go.transform.SetPositionAndRotation(p, Quaternion.LookRotation(R.dir));
                    var bc = go.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.center = new Vector3(0, h / 2f, 0); bc.size = new Vector3(0.3f, h, len);
                    var comp = go.AddComponent<OpenWorldFencePanel>(); comp.owner = this; comp.run = r; comp.seg = i;
                    st.panels[i] = new Panel { comp = comp, col = bc, run = r, seg = i, len = len, h = h };
                    PanelCount++;
                }
            }
        }

        // vận tốc ngang của vật chạm (xe NPC kinematic không có vận tốc vật lý)
        Vector3 VelocityOf(Collider other, out Rigidbody rb, out OpenWorldNpcBody npc)
        {
            rb = other.attachedRigidbody; npc = null;
            if (!rb) return Vector3.zero;
            npc = rb.GetComponent<OpenWorldNpcBody>();
            var v = npc && npc.owner ? npc.owner.NpcVelocity(npc.index) : rb.linearVelocity;
            v.y = 0; return v;
        }

        public void Touch(OpenWorldFencePanel comp, Collider other)
        {
            if (state == null || comp.run >= state.Length) return;
            var p = state[comp.run].panels[comp.seg]; if (p.down) return;
            if (other.gameObject.layer == DownLayer || other.GetComponentInParent<OpenWorldFencePanel>()) return;
            var vel = VelocityOf(other, out var rb, out var npc);
            if (!rb || vel.magnitude < minSpeed) return;
            if (npc && npc.owner && !npc.owner.IsWreck(npc.index)) return;   // xe NPC chạy bình thường (xe máy lách sát lề) không phá rào
            var point = other.ClosestPoint(comp.transform.position + Vector3.up * (p.h * 0.5f));
            Knock(p, point, vel, rb);
            // xe chạm: mất bớt đà
            var drv = rb.GetComponent<OpenWorldDriver>();
            if (drv) drv.HitFence(point, vel.magnitude);
            else if (npc && npc.owner) npc.owner.FenceHit(npc.index);
        }

        void Break(int r)
        {
            var st = state[r]; if (st.broken) return;
            st.broken = true; RunsBroken++;
            var R = runs[r];
            if (R.renderer) R.renderer.enabled = false;
            var mat = R.renderer ? R.renderer.sharedMaterial : null;
            foreach (var p in st.panels)
            {
                var go = p.comp.gameObject;
                go.AddComponent<MeshFilter>().sharedMesh = PanelMesh(p.len);
                p.mr = go.AddComponent<MeshRenderer>(); p.mr.sharedMaterial = mat;
            }
            // hộp vỉa hè ẩn cho tấm rào đổ (chỉ va với layer 30)
            var perp = new Vector3(R.dir.z, 0, -R.dir.x);
            float a = R.segs[0].x - 2f, b = R.segs[R.segs.Length - 1].y + 2f;
            var curb = R.origin + perp * R.lateral - st.outward * curbInset;
            var sw = new GameObject("ViaHeAn"); sw.layer = SidewalkLayer; sw.transform.SetParent(transform, false);
            sw.transform.SetPositionAndRotation(curb + R.dir * ((a + b) / 2f) + st.outward * 3f + Vector3.up * (sidewalkY - 0.25f), Quaternion.LookRotation(R.dir));
            var bc = sw.AddComponent<BoxCollider>(); bc.size = new Vector3(6f, 0.5f, b - a);
            st.sidewalk = sw;
        }

        void Knock(Panel p, Vector3 point, Vector3 vel, Rigidbody hitter)
        {
            Break(p.run);
            p.down = true; p.downAt = Time.time; Knocked++;
            var go = p.comp.gameObject; go.layer = DownLayer;
            p.col.isTrigger = false; p.col.center = new Vector3(0, p.h * 0.5f, 0); p.col.size = new Vector3(0.1f, p.h, p.len);
            foreach (var c in playerCols) if (c) Physics.IgnoreCollision(p.col, c);
            foreach (var c in policeCols) if (c) Physics.IgnoreCollision(p.col, c);
            if (hitter) foreach (var c in hitter.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(p.col, c);
            var rb = p.rb = go.AddComponent<Rigidbody>();
            rb.mass = 18f; rb.linearDamping = 0.15f; rb.angularDamping = 0.6f; rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            float sp = vel.magnitude; var dir = sp > 0.01f ? vel / sp : go.transform.right;
            var tip = Vector3.Cross(Vector3.up, dir);                     // trục ngã theo hướng xe đẩy
            if (sp < 5f)
            {   // chậm: chỉ xô ngã ra phía trước
                rb.linearVelocity = dir * (0.8f + sp * 0.5f) + Vector3.up * Random.Range(0.3f, 0.9f);
                rb.angularVelocity = tip * Random.Range(2f, 3.5f) + Random.insideUnitSphere * 0.6f;
            }
            else
            {   // nhanh: hất bay, xoay lộn
                rb.linearVelocity = vel * Random.Range(0.45f, 0.85f) + Vector3.up * Mathf.Clamp(1.2f + sp * 0.16f, 1.2f, 5.5f) + Random.insideUnitSphere * 1.2f;
                rb.angularVelocity = tip * (3f + sp * 0.25f) + Random.insideUnitSphere * (1.5f + sp * 0.12f);
            }
            flying.Add(p);
            if (hazards) hazards.ImpactShards(point, -dir, Mathf.Max(2f, sp * 0.55f), true);
            LastKnock = $"rào {p.run}/{p.seg} {sp * 3.6f:0} km/h bởi {(hitter ? hitter.name : "?")}";
        }

        void FixedUpdate()
        {
            for (int i = flying.Count - 1; i >= 0; i--)
            {
                var p = flying[i];
                if (!p.rb) { flying.RemoveAt(i); continue; }
                float age = Time.time - p.downAt;
                bool still = p.rb.linearVelocity.sqrMagnitude < 0.05f && p.rb.angularVelocity.sqrMagnitude < 0.2f;
                if (age > 6f || p.comp.transform.position.y < -3f || (age > 0.8f && (still || p.rb.IsSleeping())))
                {
                    // nằm yên: bỏ vật lý, tắt va chạm (xe chạy qua không vướng)
                    p.settled = true; p.col.enabled = false;
                    Destroy(p.rb); p.rb = null;
                    flying.RemoveAt(i);
                }
            }
        }

        // ---------------------------------------------------------------- lưới tấm rào (cùng kiểu `_fence_segment` trong Blender)
        Mesh PanelMesh(float L)
        {
            int key = Mathf.RoundToInt(L * 100f);
            if (meshCache.TryGetValue(key, out var m) && m) return m;
            var v = new List<Vector3>(); var n = new List<Vector3>(); var t = new List<int>();
            // gốc ở chân trụ (y = 0 = 0,15 m trên mặt đường), z dọc tấm từ −L/2 đến L/2
            Box(v, n, t, new Vector3(0, 0.85f, 0), new Vector3(0.05f, 0.05f, L));           // thanh ngang trên
            Box(v, n, t, new Vector3(0, 0.20f, 0), new Vector3(0.05f, 0.04f, L));           // thanh ngang dưới
            int bars = Mathf.Max(1, (int)(L / 0.12f));
            for (int i = 0; i <= bars; i++) Box(v, n, t, new Vector3(0, 0.5f, -L / 2f + i * L / bars), new Vector3(0.02f, 0.7f, 0.02f));
            Box(v, n, t, new Vector3(0, 0.45f, -L / 2f), new Vector3(0.07f, 0.9f, 0.07f));  // hai trụ
            Box(v, n, t, new Vector3(0, 0.45f, L / 2f), new Vector3(0.07f, 0.9f, 0.07f));
            m = new Mesh { name = "OW_TamRao_" + key };
            if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(v); m.SetNormals(n); m.SetTriangles(t, 0); m.RecalculateBounds();
            var uv = new Vector2[v.Count]; m.uv = uv;
            meshCache[key] = m; return m;
        }
        static readonly Vector3[] FaceN = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        static void Box(List<Vector3> v, List<Vector3> n, List<int> t, Vector3 c, Vector3 s)
        {
            var h = s * 0.5f;
            foreach (var f in FaceN)
            {
                Vector3 u = f.x != 0 ? Vector3.up : f.y != 0 ? Vector3.forward : Vector3.right;
                Vector3 w = Vector3.Cross(f, u);
                Vector3 fc = c + Vector3.Scale(f, h);
                Vector3 du = Vector3.Scale(u, h), dw = Vector3.Scale(w, h);
                int b = v.Count;
                v.Add(fc - du - dw); v.Add(fc + du - dw); v.Add(fc + du + dw); v.Add(fc - du + dw);
                for (int k = 0; k < 4; k++) n.Add(f);
                t.Add(b); t.Add(b + 1); t.Add(b + 2); t.Add(b); t.Add(b + 2); t.Add(b + 3);
            }
        }

        // Kiểm thử: tâm tấm rào gần điểm p nhất (để đặt xe húc).
        public bool NearestPanel(Vector3 p, out Vector3 center, out Vector3 along, out Vector3 outward)
        {
            center = along = outward = Vector3.zero; float best = float.MaxValue;
            if (state == null) return false;
            for (int r = 0; r < runs.Count; r++)
                foreach (var pn in state[r].panels)
                {
                    if (pn.down) continue;
                    var c = pn.comp.transform.position; float d = (c - p).sqrMagnitude;
                    if (d < best) { best = d; center = c; along = runs[r].dir; outward = state[r].outward; }
                }
            return best < float.MaxValue;
        }
        public string Report() => $"railsOff={RailsOff} runs={runs.Count} panels={PanelCount} broken={RunsBroken} knocked={Knocked} flying={flying.Count} last={LastKnock}";
    }
}
