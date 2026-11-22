using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // Bản đồ mở — giao thông NPC (21.251–21.252, đợt 7 phần 1). Tách hẳn khỏi PhoVietGame.TrafficVehicle (map cuộn cũ).
    // • Mạng làn dựng lúc chạy từ danh sách tuyến đường của layout.json (builder chép vào trường `roads`):
    //   mọi tuyến thẳng trục; giao lộ = điểm hai tuyến vuông góc chạm nhau. Đi bên phải; đường chính 2 làn mỗi
    //   chiều (k=0 làn trong sát vạch giữa 1,75 m, k=1 làn ngoài sát lề 5,25 m), đường nội bộ 1 làn mỗi chiều.
    //   Làn dừng trước vạch người đi bộ (nửa lòng đường cắt + 3,2 m); qua giao lộ theo đường cong Bezier.
    //   Rẽ phải từ làn ngoài sang làn ngoài, rẽ trái từ làn trong sang làn trong.
    // • 21.252: giao lộ có ≥ 3 hướng dựng CỘT ĐÈN GIAO THÔNG có sẵn (PV_TrafficSignal.fbx của game cũ) ở góc phải
    //   mỗi hướng vào; đèn 2 pha (đông–tây / bắc–nam): xanh 13 s, vàng 3 s, đỏ toàn phần 1,5 s, lệch pha ngẫu nhiên.
    //   Xe đi khi xanh; rẽ trái nhường xe đi thẳng ngược chiều. Giao lộ góc (2 hướng) không đèn: mỗi lúc một hướng.
    // • Không vào giao lộ khi làn ra đã kín. Giữ khoảng cách; chuyển làn né xe đứng yên; xe máy lách sát vạch làn.
    // • Đèn pha/đèn hậu, đèn phanh sáng khi giảm tốc; còi khi bị xe người chơi chắn/đâm; bị đâm: ô tô giật theo hướng
    //   va chạm và đứng 2,5 s, xe máy ngã rồi dựng lại.
    // • Pool cố định quanh người chơi (ô tô Meshy + xe máy Urban Rider dùng lại của game cũ).
    public class OpenWorldTraffic : MonoBehaviour
    {
        [Serializable] public struct RoadDef { public string id; public bool main; public Vector2 a, b; }
        [Serializable] public class CarModel { public GameObject model; public Material material; public float weight = 1; public Vector3 headlight, taillight; [Tooltip("21.260: khối lượng (tấn) cho luật va chạm; 0 = tự suy theo tên mẫu/kích thước")] public float mass; }

        public RoadDef[] roads;
        public CarModel[] cars;
        public GameObject bikeModel;
        // 21.295 (người dùng: xe máy + người lái to vô lý so với xe người chơi): mẫu PV_UrbanRider gốc cao 2,15 m, dài 2,27 m, rộng 1,1 m
        // → thu 0,75 (cao 1,61 m) — người dùng vẫn thấy to → 0,64: cao ~1,38 m, dài ~1,45 m, rộng ~0,7 m = cỡ xe tay ga + người ngồi
        // ngoài đời nhân cùng tỉ lệ ~0,84 của xe người chơi (dài 3,8 m so với xe thể thao thật ~4,5 m). Đèn, va chạm, khói, độ nâng khi ngã theo tỉ lệ.
        [Range(0.5f, 1f)] public float bikeScale = 0.704f;   // 21.299: +10 % (0,64 → 0,704, người dùng: xe máy hơi bé)
        public Material[] bikeMaterials;
        public Transform player;
        public int carCount = 34, bikeCount = 26;
        public float spawnMin = 130f, spawnMax = 240f, despawnRadius = 280f, initialMin = 35f;
        public int seed = 1751;

        [Header("Đèn giao thông (21.252)")]
        public GameObject signalModel;
        public Material signalMaterial;
        [Tooltip("0 tắt, 1 đỏ, 2 vàng, 3 xanh")] public Material[] lensMaterials;
        [Tooltip("21.253: bảng LED đếm ngược — 0 đỏ, 1 vàng, 2 xanh (atlas 7 đoạn 0–9 + ô trống)")] public Material[] countdownMaterials;
        [Tooltip("21.253: quầng sáng cộng màu trước kính đèn — 0 đỏ, 1 vàng, 2 xanh")] public Material[] haloMaterials;
        public float greenTime = 13f, yellowTime = 3f, allRedTime = 1.5f;

        [Header("Đèn xe (21.252)")]
        public Material headMaterial, tailMaterial, brakeMaterial;

        // ------------------------------------------------------------ mạng làn
        class Node
        {
            public Vector3 p; public float halfEW, halfNS; public int roadsEW, roadsNS;
            public int count, active = -1;
            public readonly int[] waitCount = new int[4]; public readonly float[] waitStart = new float[4];
            public bool signal; public float offset;
            public readonly int[] inApp = new int[4], leftIn = new int[4];
            public readonly int[] shown = { -1, -1 }, shownNum = { -1, -1 }, shownNumState = { -1, -1 };
            public readonly Mesh[] countMesh = new Mesh[2];
            public readonly List<Renderer>[] countRends = { new List<Renderer>(), new List<Renderer>() };
            public readonly List<Renderer>[] halos = { new List<Renderer>(), new List<Renderer>(), new List<Renderer>(), new List<Renderer>(), new List<Renderer>(), new List<Renderer>() };
            public readonly List<Renderer>[] lenses = { new List<Renderer>(), new List<Renderer>(), new List<Renderer>(), new List<Renderer>(), new List<Renderer>(), new List<Renderer>() };
        }
        class Lane
        {
            public Vector3 a, b, dir, right; public float len; public Node start, end; public int k, nk, approach; public bool main; public Lane sibling;
            public readonly List<Conn> outs = new List<Conn>();
        }
        // kind: 0 thẳng, 1 phải, 2 trái, 3 quay đầu
        class Conn { public Vector3[] pts; public float[] cum; public float len; public Lane from, to; public Node node; public bool turn; public int kind; }

        readonly List<Node> nodes = new List<Node>();
        readonly List<Lane> lanes = new List<Lane>();
        float laneTotal;

        static readonly float[] LaneOffsets = { 1.75f, 5.25f };
        const float StopMargin = 3.2f;

        Node GetNode(Vector2 q)
        {
            foreach (var n in nodes) if (Mathf.Abs(n.p.x - q.x) < 1f && Mathf.Abs(n.p.z - q.y) < 1f) return n;
            var nn = new Node { p = new Vector3(q.x, 0, q.y) }; nodes.Add(nn); return nn;
        }

        void BuildNetwork()
        {
            int R = roads.Length;
            var ns = new bool[R]; var c = new float[R]; var lo = new float[R]; var hi = new float[R];
            for (int i = 0; i < R; i++)
            {
                var r = roads[i]; ns[i] = Mathf.Abs(r.a.x - r.b.x) < 0.01f;
                c[i] = ns[i] ? r.a.x : r.a.y;
                lo[i] = ns[i] ? Mathf.Min(r.a.y, r.b.y) : Mathf.Min(r.a.x, r.b.x);
                hi[i] = ns[i] ? Mathf.Max(r.a.y, r.b.y) : Mathf.Max(r.a.x, r.b.x);
            }
            var along = new List<(float t, Node n)>[R];
            for (int i = 0; i < R; i++) along[i] = new List<(float, Node)>();
            for (int i = 0; i < R; i++)
                for (int j = i + 1; j < R; j++)
                {
                    if (ns[i] == ns[j]) continue;
                    if (c[j] < lo[i] - 1 || c[j] > hi[i] + 1 || c[i] < lo[j] - 1 || c[i] > hi[j] + 1) continue;
                    var q = ns[i] ? new Vector2(c[i], c[j]) : new Vector2(c[j], c[i]);
                    var n = GetNode(q);
                    along[i].Add((c[j], n)); along[j].Add((c[i], n));
                }
            for (int i = 0; i < R; i++)
            {
                float hw = roads[i].main ? 7f : 3.5f;
                foreach (var e in along[i])
                {
                    if (ns[i]) { e.n.halfNS = Mathf.Max(e.n.halfNS, hw); e.n.roadsNS++; }
                    else { e.n.halfEW = Mathf.Max(e.n.halfEW, hw); e.n.roadsEW++; }
                }
                foreach (var t in new[] { lo[i], hi[i] })
                    if (!along[i].Exists(e => Mathf.Abs(e.t - t) < 1f))
                    {
                        var n = GetNode(ns[i] ? new Vector2(c[i], t) : new Vector2(t, c[i]));
                        along[i].Add((t, n));
                    }
                along[i].Sort((x, y) => x.t.CompareTo(y.t));
            }
            for (int i = 0; i < R; i++)
            {
                var L = along[i]; int nk = roads[i].main ? 2 : 1;
                for (int s = 0; s + 1 < L.Count; s++)
                    for (int dirSign = 0; dirSign < 2; dirSign++)
                    {
                        var A = dirSign == 0 ? L[s].n : L[s + 1].n; var B = dirSign == 0 ? L[s + 1].n : L[s].n;
                        var d = (B.p - A.p); if (d.magnitude < 20f) continue; d.Normalize();
                        var right = new Vector3(d.z, 0, -d.x);
                        float clA = Clear(A, ns[i]), clB = Clear(B, ns[i]);
                        for (int k = 0; k < nk; k++)
                        {
                            var ln = new Lane
                            {
                                a = A.p + d * clA + right * LaneOffsets[k], b = B.p - d * clB + right * LaneOffsets[k],
                                dir = d, right = right, start = A, end = B, k = k, nk = nk, main = roads[i].main,
                                approach = Approach(d)
                            };
                            ln.len = Vector3.Distance(ln.a, ln.b);
                            if (ln.len > 4f) { lanes.Add(ln); laneTotal += ln.len; }
                        }
                    }
            }
            foreach (var L in lanes)
                if (L.nk == 2) foreach (var M in lanes) if (M != L && M.start == L.start && M.end == L.end) { L.sibling = M; break; }
            foreach (var L in lanes)
            {
                foreach (var M in lanes)
                {
                    if (M.start != L.end) continue;
                    float dot = Vector3.Dot(L.dir, M.dir);
                    if (dot < -0.9f) continue;
                    float cross = L.dir.x * M.dir.z - L.dir.z * M.dir.x;          // < 0: rẽ phải
                    bool straight = dot > 0.9f, rightTurn = !straight && cross < 0;
                    // Làn ngoài (k = nk-1) rẽ phải, làn trong (k = 0) rẽ trái; đường 1 làn đi mọi hướng.
                    if (L.nk == 2 && !straight && (rightTurn ? L.k != L.nk - 1 : L.k != 0)) continue;
                    int target = straight ? Mathf.Min(L.k, M.nk - 1) : rightTurn ? M.nk - 1 : 0;
                    if (M.k != target) continue;
                    L.outs.Add(MakeConn(L, M, straight ? 0 : rightTurn ? 1 : 2));
                }
                if (L.outs.Count == 0)
                    foreach (var M in lanes)
                        if (M.start == L.end && Vector3.Dot(L.dir, M.dir) < -0.9f && M.k == 0) { L.outs.Add(MakeConn(L, M, 3)); break; }
            }
            // Giao lộ có ít nhất 3 hướng vào → có đèn.
            var rr = new System.Random(seed + 7);
            foreach (var n in nodes)
            {
                int apps = 0;
                for (int a = 0; a < 4; a++) if (lanes.Exists(l => l.end == n && l.approach == a)) apps++;
                n.signal = apps >= 3 && signalModel != null;
                n.offset = (float)rr.NextDouble() * Cycle;
            }
        }

        static float Clear(Node n, bool laneIsNS)
        {
            float h = laneIsNS ? n.halfEW : n.halfNS; int cnt = laneIsNS ? n.roadsEW : n.roadsNS;
            return cnt > 0 ? h + StopMargin : 0f;
        }
        static int Approach(Vector3 d) => Mathf.Abs(d.x) > Mathf.Abs(d.z) ? (d.x > 0 ? 0 : 2) : (d.z > 0 ? 1 : 3);

        Conn MakeConn(Lane L, Lane M, int kind)
        {
            bool turn = kind != 0;
            Vector3 p0 = L.b, p3 = M.a, c1, c2;
            if (!turn) { c1 = Vector3.Lerp(p0, p3, 1f / 3); c2 = Vector3.Lerp(p0, p3, 2f / 3); }
            else if (kind == 3)
            {
                float r = Vector3.Distance(p0, p3) * 0.9f + 3f;
                c1 = p0 + L.dir * r; c2 = p3 - M.dir * r;
            }
            else
            {
                var corner = Mathf.Abs(L.dir.z) > 0.5f ? new Vector3(p0.x, 0, p3.z) : new Vector3(p3.x, 0, p0.z);
                c1 = p0 + (corner - p0) * 0.5523f * 1.2f; c2 = p3 + (corner - p3) * 0.5523f * 1.2f;
            }
            int n = turn ? 12 : 2;
            var pts = new Vector3[n + 1]; var cum = new float[n + 1];
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n, u = 1 - t;
                pts[i] = u * u * u * p0 + 3 * u * u * t * c1 + 3 * u * t * t * c2 + t * t * t * p3;
                if (i > 0) cum[i] = cum[i - 1] + Vector3.Distance(pts[i - 1], pts[i]);
            }
            return new Conn { pts = pts, cum = cum, len = cum[n], from = L, to = M, node = L.end, turn = turn, kind = kind };
        }

        // ------------------------------------------------------------ đèn giao thông
        float Cycle => 2f * (greenTime + yellowTime + allRedTime);
        // 0 đỏ, 1 vàng, 2 xanh cho trục (0 đông–tây, 1 bắc–nam)
        int SignalState(Node n, int axis, float now)
        {
            float half = greenTime + yellowTime + allRedTime;
            float t = Mathf.Repeat(now + n.offset, Cycle) - axis * half;
            if (t < 0 || t >= half) return 0;
            return t < greenTime ? 2 : t < greenTime + yellowTime ? 1 : 0;
        }
        public int SignalAt(Vector3 nodePos, int axis)
        {
            foreach (var n in nodes) if (n.signal && (n.p - nodePos).sqrMagnitude < 4f) return SignalState(n, axis, Time.time);
            return -1;
        }

        Mesh lensMain, lensAux, quadBack, quadFront;
        static Mesh LensMesh(float radius, float zFront, float zBack)
        {
            // Hai mặt kính tròn hơi lồi (trước/sau) — đầu đèn hai mặt như mẫu Blender.
            const int sides = 20;
            var v = new List<Vector3>(); var nrm = new List<Vector3>(); var idx = new List<int>();
            for (int face = 0; face < 2; face++)
            {
                float z = face == 0 ? zFront : zBack, dir = face == 0 ? -1f : 1f; int c0 = v.Count;
                v.Add(new Vector3(0, 0, z + dir * radius * 0.25f)); nrm.Add(new Vector3(0, 0, dir));
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2f / sides;
                    v.Add(new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, z));
                    nrm.Add(new Vector3(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f, dir).normalized);
                }
                for (int i = 0; i < sides; i++)
                {
                    int a = c0 + 1 + i, b = c0 + 1 + (i + 1) % sides;
                    if (face == 0) { idx.Add(c0); idx.Add(a); idx.Add(b); } else { idx.Add(c0); idx.Add(b); idx.Add(a); }
                }
            }
            var m = new Mesh { name = "OW_KinhDen" }; m.SetVertices(v); m.SetNormals(nrm); m.SetTriangles(idx, 0); m.RecalculateBounds();
            // Đảm bảo mặt trước quay về -z theo pháp tuyến (đổi chiều nếu cần).
            var tri = m.triangles; var vv = m.vertices;
            for (int t = 0; t < tri.Length; t += 3)
            {
                var nn = Vector3.Cross(vv[tri[t + 1]] - vv[tri[t]], vv[tri[t + 2]] - vv[tri[t]]);
                float want = vv[tri[t]].z < (zFront + zBack) * 0.5f ? -1f : 1f;
                if (Mathf.Sign(nn.z) != want) { int tmp = tri[t + 1]; tri[t + 1] = tri[t + 2]; tri[t + 2] = tmp; }
            }
            m.triangles = tri;
            return m;
        }
        static Mesh Quad(float facing)
        {
            var m = new Mesh { name = facing < 0 ? "OW_DenSau" : "OW_DenTruoc" };
            m.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0), new Vector3(.5f, .5f, 0), new Vector3(-.5f, .5f, 0) };
            m.normals = new[] { Vector3.forward * facing, Vector3.forward * facing, Vector3.forward * facing, Vector3.forward * facing };
            m.triangles = facing < 0 ? new[] { 0, 2, 1, 0, 3, 2 } : new[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateBounds(); return m;
        }

        public int SignalPoles { get; private set; }
        void BuildSignals()
        {
            if (!signalModel) return;
            // 21.253: kính to hơn (0,15 → 0,175 m; phụ 0,09 → 0,1 m) cho dễ nhìn từ xa / điện thoại.
            lensMain = LensMesh(0.175f, -0.218f, 0.174f);
            lensAux = LensMesh(0.10f, -0.131f, 0.131f);
            haloFront = Quad(-1f); haloBack = Quad(1f);
            unitCube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var root = new GameObject("DenGiaoThong").transform; root.SetParent(transform, false);
            int mask = OpenWorldFences.WorldMask;   // 21.274: bỏ qua mặt lề MatLe
            foreach (var n in nodes)
            {
                if (!n.signal) continue;
                for (int a = 0; a < 4; a++)
                {
                    var L = lanes.Find(l => l.end == n && l.approach == a); if (L == null) continue;
                    var d = L.dir; var r = L.right;
                    float halfOwn = L.main ? 7f : 3.5f;
                    float halfCross = Clear(n, Mathf.Abs(d.z) > 0.5f) - StopMargin;
                    Vector3 pos = n.p - d * (halfCross + 3.4f) + r * (halfOwn + 1.0f);
                    // Né cây/cột đèn/ghế đã có: lùi dọc vỉa hè tối đa 6 m.
                    for (int tries = 0; tries < 5; tries++)
                    {
                        bool hit = false;
                        foreach (var c in Physics.OverlapCapsule(pos + Vector3.up * 0.4f, pos + Vector3.up * 3f, 0.75f, mask, QueryTriggerInteraction.Ignore))
                        {
                            if (c.attachedRigidbody != null) continue;
                            var bs = c.bounds.size; if (bs.x > 20f || bs.z > 20f) continue;     // mặt đất/mặt cầu
                            hit = true; break;
                        }
                        if (!hit) break;
                        pos -= d * 1.5f;
                    }
                    var pole = new GameObject("CotDen_" + a).transform; pole.SetParent(root, false);
                    pole.SetPositionAndRotation(pos, Quaternion.LookRotation(-d));
                    var inst = Instantiate(signalModel, pole, false); inst.name = signalModel.name;
                    inst.transform.localRotation = signalModel.transform.localRotation;
                    var rends = new List<Renderer>(inst.GetComponentsInChildren<Renderer>(true));
                    Renderer l0 = null, l1 = null, l2 = null;
                    foreach (var rd in rends)
                    {
                        if (signalMaterial) rd.sharedMaterial = signalMaterial;
                        if (rd.name.EndsWith("_LOD1")) l1 = rd; else if (rd.name.EndsWith("_LOD2")) l2 = rd; else l0 = rd;
                    }
                    var cap = pole.gameObject.AddComponent<CapsuleCollider>(); cap.center = new Vector3(0, 3.9f, 0); cap.height = 7.8f; cap.radius = 0.16f;
                    int axis = a % 2;
                    var lensRends = new List<Renderer>();
                    for (int i = 0; i < 3; i++)
                    {
                        var lm = Lens(pole, lensMain, new Vector3(3.0f, 6.82f + 0.43f - i * 0.43f, 0f), n, axis, i);
                        lensRends.Add(lm);
                        lensRends.AddRange(Halos(lm.transform, n, axis, i, 1.15f, -0.52f, 0.48f));   // trước mái che kính (sâu 0,48 m)
                        var la = Lens(pole, lensAux, new Vector3(0f, 3.85f - i * 0.3f, 0f), n, axis, i);
                        lensRends.Add(la);
                        lensRends.AddRange(Halos(la.transform, n, axis, i, 0.5f, -0.16f, 0.16f));
                    }
                    var countRends = CountdownBox(pole, n, axis);
                    var lg = inst.GetComponent<LODGroup>(); if (!lg) lg = inst.AddComponent<LODGroup>();
                    // Kính + quầng sáng ở cả 3 mức LOD → đèn vẫn thấy rõ ở xa; bảng đếm chỉ khi gần.
                    var all0 = new List<Renderer>(lensRends); all0.AddRange(countRends); if (l0) all0.Add(l0);
                    var all1 = new List<Renderer>(lensRends); all1.AddRange(countRends); if (l1) all1.Add(l1);
                    var all2 = new List<Renderer>(lensRends); if (l2) all2.Add(l2);
                    lg.SetLODs(new[] { new LOD(0.22f, all0.ToArray()), new LOD(0.06f, all1.ToArray()), new LOD(0.012f, all2.ToArray()) });
                    lg.RecalculateBounds();
                    SignalPoles++;
                }
            }
            UpdateSignalLenses(true);
        }
        Renderer Lens(Transform pole, Mesh mesh, Vector3 local, Node n, int axis, int color)
        {
            var g = new GameObject("TinHieu_" + color); g.transform.SetParent(pole, false); g.transform.localPosition = local;
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>(); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.sharedMaterial = lensMaterials != null && lensMaterials.Length > 0 ? lensMaterials[0] : null;
            n.lenses[axis * 3 + color].Add(r);
            return r;
        }
        Mesh haloFront, haloBack, unitCube;
        List<Renderer> Halos(Transform lens, Node n, int axis, int color, float size, float zFront, float zBack)
        {
            var list = new List<Renderer>();
            if (haloMaterials == null || haloMaterials.Length < 3) return list;
            foreach (var (mesh, z) in new[] { (haloFront, zFront), (haloBack, zBack) })
            {
                var g = new GameObject("QuangSang"); g.transform.SetParent(lens, false);
                g.transform.localPosition = new Vector3(0, 0, z); g.transform.localScale = new Vector3(size, size, 1);
                g.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = haloMaterials[color];
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; r.enabled = false;
                n.halos[axis * 3 + color].Add(r); list.Add(r);
            }
            return list;
        }

        // 21.253: bảng LED đếm ngược hai mặt gắn sát hộp đèn chính (phía cột). Mỗi (giao lộ, trục) dùng chung một lưới,
        // chỉ đổi UV khi số đổi → tối đa 2 lần cập nhật lưới mỗi giây mỗi giao lộ.
        const float CountX = 2.29f, CountY = 6.82f;
        List<Renderer> CountdownBox(Transform pole, Node n, int axis)
        {
            var list = new List<Renderer>();
            if (countdownMaterials == null || countdownMaterials.Length < 3) return list;
            var box = new GameObject("HopDemNguoc"); box.transform.SetParent(pole, false);
            box.transform.localPosition = new Vector3(CountX, CountY, -0.022f); box.transform.localScale = new Vector3(0.62f, 0.46f, 0.3f);
            box.AddComponent<MeshFilter>().sharedMesh = unitCube;
            var br = box.AddComponent<MeshRenderer>(); br.sharedMaterial = lensMaterials[0]; list.Add(br);
            if (n.countMesh[axis] == null) n.countMesh[axis] = CountMesh();
            var d = new GameObject("SoDemNguoc"); d.transform.SetParent(pole, false); d.transform.localPosition = new Vector3(CountX, CountY, 0f);
            d.AddComponent<MeshFilter>().sharedMesh = n.countMesh[axis];
            var dr = d.AddComponent<MeshRenderer>(); dr.sharedMaterial = countdownMaterials[0];
            dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            n.countRends[axis].Add(dr); list.Add(dr);
            return list;
        }
        static Mesh CountMesh()
        {
            // 2 mặt × 2 chữ số. Mặt trước nhìn từ -z (bên phải người xem = +x), mặt sau nhìn từ +z (bên phải = -x).
            var v = new List<Vector3>(); var nr = new List<Vector3>(); var tri = new List<int>();
            const float w = 0.2f, h = 0.34f, gapX = 0.115f;
            for (int face = 0; face < 2; face++)
            {
                float z = face == 0 ? -0.176f : 0.132f, nz = face == 0 ? -1f : 1f;
                for (int dg = 0; dg < 2; dg++)          // 0 hàng chục, 1 hàng đơn vị (theo mắt người xem)
                {
                    float cx = (dg == 0 ? -gapX : gapX) * (face == 0 ? 1f : -1f);
                    // góc theo mắt người xem: trái-dưới, phải-dưới, phải-trên, trái-trên
                    float l = face == 0 ? cx - w / 2 : cx + w / 2, r = face == 0 ? cx + w / 2 : cx - w / 2;
                    int b0 = v.Count;
                    v.Add(new Vector3(l, -h / 2, z)); v.Add(new Vector3(r, -h / 2, z)); v.Add(new Vector3(r, h / 2, z)); v.Add(new Vector3(l, h / 2, z));
                    for (int k = 0; k < 4; k++) nr.Add(new Vector3(0, 0, nz));
                    tri.AddRange(new[] { b0, b0 + 2, b0 + 1, b0, b0 + 3, b0 + 2 });
                }
            }
            // Mỗi tam giác quay mặt đúng phía pháp tuyến của nó.
            for (int i = 0; i < tri.Count; i += 3)
            {
                var c = Vector3.Cross(v[tri[i + 1]] - v[tri[i]], v[tri[i + 2]] - v[tri[i]]);
                float want = nr[tri[i]].z;
                if (Mathf.Sign(c.z) != Mathf.Sign(want)) { int tmp = tri[i + 1]; tri[i + 1] = tri[i + 2]; tri[i + 2] = tmp; }
            }
            var m = new Mesh { name = "OW_SoDemNguoc" };
            m.SetVertices(v); m.SetNormals(nr); m.SetTriangles(tri, 0);
            m.MarkDynamic(); SetDigits(m, 10, 10); m.RecalculateBounds();
            return m;
        }
        static readonly List<Vector2> uvBuf = new List<Vector2>(16);
        static void SetDigits(Mesh m, int tens, int units)
        {
            uvBuf.Clear();
            for (int face = 0; face < 2; face++)
                for (int dg = 0; dg < 2; dg++)
                {
                    int g = dg == 0 ? tens : units; float u0 = g / 11f, u1 = (g + 1) / 11f;
                    uvBuf.Add(new Vector2(u0, 0)); uvBuf.Add(new Vector2(u1, 0)); uvBuf.Add(new Vector2(u1, 1)); uvBuf.Add(new Vector2(u0, 1));
                }
            m.SetUVs(0, uvBuf);
        }
        // Số giây còn lại của màu hiện tại (xanh tới vàng, vàng tới đỏ, đỏ tới xanh).
        float Remaining(Node n, int axis, float now)
        {
            float half = greenTime + yellowTime + allRedTime;
            float t = Mathf.Repeat(now + n.offset - axis * half, Cycle);
            if (t < greenTime) return greenTime - t;
            if (t < greenTime + yellowTime) return greenTime + yellowTime - t;
            return Cycle - t;
        }
        public int CountdownAt(Vector3 nodePos, int axis)
        {
            foreach (var n in nodes) if (n.signal && (n.p - nodePos).sqrMagnitude < 4f) return Mathf.CeilToInt(Remaining(n, axis, Time.time));
            return -1;
        }

        void UpdateSignalLenses(bool force)
        {
            if (lensMaterials == null || lensMaterials.Length < 4) return;
            float now = Time.time;
            foreach (var n in nodes)
            {
                if (!n.signal) continue;
                for (int axis = 0; axis < 2; axis++)
                {
                    int st = SignalState(n, axis, now);
                    if (!force && st == n.shown[axis]) continue;
                    n.shown[axis] = st;
                    // lens màu: 0 đỏ (sáng khi st 0), 1 vàng (st 1), 2 xanh (st 2)
                    for (int c = 0; c < 3; c++)
                    {
                        bool on = (c == 0 && st == 0) || (c == 1 && st == 1) || (c == 2 && st == 2);
                        var m = on ? lensMaterials[c + 1] : lensMaterials[0];
                        foreach (var r in n.lenses[axis * 3 + c]) r.sharedMaterial = m;
                        foreach (var r in n.halos[axis * 3 + c]) r.enabled = on;
                    }
                }
                // Bảng đếm ngược: số = giây còn lại (làm tròn lên), màu theo đèn đang sáng.
                for (int axis = 0; axis < 2; axis++)
                {
                    if (n.countMesh[axis] == null) continue;
                    int st = n.shown[axis], num = Mathf.Clamp(Mathf.CeilToInt(Remaining(n, axis, now)), 0, 99);
                    if (!force && num == n.shownNum[axis] && st == n.shownNumState[axis]) continue;
                    if (num != n.shownNum[axis] || force) SetDigits(n.countMesh[axis], num >= 10 ? num / 10 : 10, num % 10);
                    if (st != n.shownNumState[axis] || force)
                    {
                        var m = countdownMaterials[st == 0 ? 0 : st == 1 ? 1 : 2];
                        foreach (var r in n.countRends[axis]) r.sharedMaterial = m;
                    }
                    n.shownNum[axis] = num; n.shownNumState[axis] = st;
                }
            }
        }

        // ------------------------------------------------------------ xe
        class Vehicle
        {
            public int index; public Transform root, visual; public Rigidbody rb; public Animation anim; public bool bike;
            public Renderer[] renderers; public float halfL, halfW, vmax, speed, lateral, baseLateral, stopTime, ghostUntil, lean, shift, blockedTime;
            public bool active, inConn, committed, claimed, waiting, exitBlocked; public Lane lane; public Conn conn, next; public float s;
            public Vector3 pos, fwd; public Quaternion rot; public string clip;
            public int claimApp = -1; public bool claimLeft; public Node claimNode;
            public Renderer[] brakeLights; public bool braking; public float prevSpeed;
            public float hitUntil, recoverAt, filterUntil, honkBlocked, nextHonk; public string hitClip;
            // 21.259: bị xe người chơi húc → văng, bốc khói, 1 s sau nổ (xe máy biến mất, ô tô thành xác cháy).
            public Color paint = Color.white; public bool wreck, exploded, landed, laserKill; public Vector3 wreckVel, wreckSpin; public float explodeAt;
            public ParticleSystem smoke; public OpenWorldCarDent dent; public float wreckAt;
            // 21.260: khối lượng; choáng rồi về làn chạy tiếp (không nổ); xe ẩu (vượt đèn đỏ, không giữ khoảng cách).
            public float mass = 1.4f; public bool fatal, recovering, reckless;
            public float fallSign, fallSev, fallRoll, fallPitch, fallLift, fallStart = -1f;   // 21.284: xe máy nghiêng ngã khi va chạm
            public float recWait, stunFor, stunUntil, recoverT, recDur, recS, recklessUntil, vmaxBase, lastHitAt; public int lastHit = -1;
            public Vector3 recFromPos; public Quaternion recFromRot; public Lane recLane;
        }
        readonly List<Vehicle> pool = new List<Vehicle>();
        System.Random rnd;
        MaterialPropertyBlock mpb;
        static readonly int PaintId = Shader.PropertyToID("_PaintColor");
        static readonly Color[] Paints = { new Color(.94f, .96f, 1), new Color(.06f, .20f, .48f), new Color(.82f, .08f, .055f),
            new Color(.045f, .55f, .43f), new Color(.92f, .63f, .06f), new Color(.40f, .15f, .65f), new Color(.09f, .10f, .12f), new Color(.94f, .27f, .06f) };
        OpenWorldDriver playerDriver;

        public int ActiveCount { get; private set; }
        public int LaneCount => lanes.Count;
        public int NodeCount => nodes.Count;
        public int Spawned { get; private set; }
        public int GhostEvents { get; private set; }
        public int Honks { get; private set; }
        public int Hits { get; private set; }
        public int RedStops { get; private set; }
        public readonly List<string> GhostLog = new List<string>();
        // 21.254: xe do script khác lái (CSGT) — xe NPC giữ khoảng cách như với xe người chơi.
        public readonly List<Rigidbody> blockers = new List<Rigidbody>();
        public readonly Dictionary<Rigidbody, Vector2> blockerHalf = new Dictionary<Rigidbody, Vector2>();   // (nửa rộng, nửa dài)

        // 21.254: đồ thị đường (giao lộ/đầu đường + đoạn nối hai chiều) cho xe CSGT tìm đường.
        public void RoadGraph(List<Vector3> points, List<Vector3Int> edges)
        {
            points.Clear(); edges.Clear();
            foreach (var n in nodes) points.Add(n.p);
            var seen = new HashSet<long>();
            foreach (var L in lanes)
            {
                int a = nodes.IndexOf(L.start), b = nodes.IndexOf(L.end);
                if (a < 0 || b < 0) continue;
                long key = (long)Mathf.Min(a, b) * 100000 + Mathf.Max(a, b);
                if (seen.Add(key)) edges.Add(new Vector3Int(a, b, L.main ? 1 : 0));
            }
        }

        void Start()
        {
            rnd = new System.Random(seed); mpb = new MaterialPropertyBlock();
            if (roads == null || roads.Length == 0) { enabled = false; return; }
            BuildNetwork();
            BuildSignals();
            BuildHorns();
            quadBack = Quad(-1f); quadFront = Quad(1f);
            if (player) playerDriver = player.GetComponent<OpenWorldDriver>();
            for (int i = 0; i < carCount + bikeCount; i++)
            {
                var v = CreateVehicle(i >= carCount);
                if (v != null) { v.index = pool.Count; pool.Add(v); }
            }
            foreach (var v in pool) TrySpawn(v, initialMin, spawnMax, false, 60);
        }

        Vehicle CreateVehicle(bool bike)
        {
            GameObject model; Material mat = null; CarModel chosen = null;
            if (bike) { model = bikeModel; if (bikeMaterials != null && bikeMaterials.Length > 0) mat = bikeMaterials[rnd.Next(bikeMaterials.Length)]; }
            else
            {
                float total = 0; foreach (var cm in cars) total += Mathf.Max(0, cm.weight);
                float pick = (float)rnd.NextDouble() * total; chosen = cars[0];
                foreach (var cm in cars) { chosen = cm; pick -= Mathf.Max(0, cm.weight); if (pick < 0) break; }
                model = chosen.model; mat = chosen.material;
            }
            if (!model) return null;
            var root = new GameObject(bike ? "XeMayNPC" : "OtoNPC");
            root.transform.SetParent(transform, false);
            var vis = Instantiate(model, root.transform, false); vis.name = model.name;
            if (bike) vis.transform.localScale = Vector3.one * bikeScale;
            var rends = vis.GetComponentsInChildren<Renderer>(true);
            Renderer lod0 = null, lod1 = null, lod2 = null;
            foreach (var r in rends)
            {
                if (mat) r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                if (r.name.EndsWith("_Body")) lod0 = r; else if (r.name.EndsWith("_LOD1")) lod1 = r; else if (r.name.EndsWith("_LOD2")) lod2 = r;
                if (r is SkinnedMeshRenderer sk) sk.updateWhenOffscreen = false;
            }
            if (lod1) lod1.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (lod2) lod2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (!bike)
            {
                paintPick = Paints[rnd.Next(Paints.Length)];
                mpb.Clear(); mpb.SetColor(PaintId, paintPick);
                foreach (var r in rends) r.SetPropertyBlock(mpb);
            }
            else paintPick = Color.white;
            var b = lod0 ? lod0.bounds : new Bounds(root.transform.position + Vector3.up, new Vector3(2, 1.5f, 4.5f));
            // Đèn pha (trắng ấm) + đèn hậu (đỏ, sáng mạnh khi phanh): tấm phẳng phát sáng theo vị trí đèn trong JSON mẫu.
            var lights = new List<Renderer>(); var brakes = new List<Renderer>();
            Vector3 head = bike ? new Vector3(0, 1.17f, 0.46f) : chosen.headlight, tail = bike ? new Vector3(0, 0.88f, -0.9f) : chosen.taillight;
            if (!bike && tail == Vector3.zero) tail = new Vector3(b.size.x * 0.33f, 0.9f, -b.size.z * 0.5f);
            if (!bike && head == Vector3.zero) head = new Vector3(0, 0.75f, b.size.z * 0.5f);
            float tx = bike ? 0f : Mathf.Max(0.45f, tail.x), hx = bike ? 0f : tx * 0.92f;
            foreach (float sx in bike ? new[] { 0f } : new[] { -1f, 1f })
            {
                var t = LightQuad(vis.transform, quadBack, new Vector3(sx * tx, tail.y, tail.z - 0.03f), bike ? new Vector2(0.12f, 0.07f) : new Vector2(0.26f, 0.12f), tailMaterial);
                if (t) { lights.Add(t); brakes.Add(t); }
                var h = LightQuad(vis.transform, quadFront, new Vector3(sx * hx, head.y, head.z + 0.03f), bike ? new Vector2(0.13f, 0.1f) : new Vector2(0.24f, 0.11f), headMaterial);
                if (h) lights.Add(h);
            }
            if (lod0 && lod1 && lod2)
            {
                var lg = vis.GetComponent<LODGroup>(); if (!lg) lg = vis.AddComponent<LODGroup>();
                var l0 = new List<Renderer>(lights) { lod0 }; var l1 = new List<Renderer>(lights) { lod1 };
                lg.SetLODs(new[]
                {
                    new LOD(bike ? 0.10f : 0.16f, l0.ToArray()),
                    new LOD(bike ? 0.035f : 0.055f, l1.ToArray()),
                    new LOD(bike ? 0.009f : 0.012f, new[] { lod2 }),
                });
                lg.RecalculateBounds();
            }
            var anim = vis.GetComponent<Animation>();
            if (anim) { anim.cullingType = AnimationCullingType.BasedOnRenderers; anim.playAutomatically = false; }
            float halfL = Mathf.Max(bike ? 0.88f : 2.0f, (bike ? b.size.z - 0.1f : b.size.z) / 2), halfW = bike ? 0.4f * bikeScale / 0.64f : Mathf.Max(0.9f, b.size.x / 2 - 0.08f);
            var bc = root.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, b.size.y / 2, 0); bc.size = new Vector3(halfW * 2, Mathf.Max(1f, b.size.y - 0.1f), halfL * 2);
            var rb = root.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.interpolation = RigidbodyInterpolation.Interpolate;
            var body = root.AddComponent<OpenWorldNpcBody>(); body.owner = this; body.index = pool.Count;
            root.SetActive(false);
            float mass = bike ? 0.25f : chosen.mass > 0f ? chosen.mass : ModelMass(model.name, 4f * halfW * halfL);
            return new Vehicle
            {
                root = root.transform, visual = vis.transform, rb = rb, anim = anim, bike = bike, renderers = rends, brakeLights = brakes.ToArray(),
                halfL = halfL, halfW = halfW, paint = paintPick, vmax = bike ? 10.5f + (float)rnd.NextDouble() * 2.5f : 11f + (float)rnd.NextDouble() * 3f, mass = mass
            };
        }
        static Renderer LightQuad(Transform parent, Mesh mesh, Vector3 pos, Vector2 size, Material mat)
        {
            if (!mat) return null;
            var g = new GameObject(mesh.name); g.transform.SetParent(parent, false);
            g.transform.localPosition = pos; g.transform.localScale = new Vector3(size.x, size.y, 1);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return r;
        }

        // ------------------------------------------------------------ còi
        readonly List<AudioSource> horns = new List<AudioSource>();
        AudioClip carHorn, bikeHorn;
        void BuildHorns()
        {
            const int hz = 22050;
            float[] Make(float seconds, Func<float, float> f)
            { var w = new float[(int)(hz * seconds)]; for (int i = 0; i < w.Length; i++) w[i] = f(i / (float)hz); return w; }
            // Ô tô: hai âm 415 + 520 Hz gần vuông, 0,45 s. Xe máy: hai tiếng "tít tít" 880 Hz.
            var car = Make(0.45f, t =>
            {
                float env = Mathf.Clamp01(t / 0.02f) * Mathf.Clamp01((0.45f - t) / 0.06f);
                float s = Mathf.Sign(Mathf.Sin(t * 2 * Mathf.PI * 415f)) * 0.5f + Mathf.Sign(Mathf.Sin(t * 2 * Mathf.PI * 520f)) * 0.5f;
                return s * 0.22f * env;
            });
            var bk = Make(0.42f, t =>
            {
                float on = (t < 0.15f || (t > 0.24f && t < 0.39f)) ? 1f : 0f;
                return Mathf.Sign(Mathf.Sin(t * 2 * Mathf.PI * 880f)) * 0.16f * on;
            });
            carHorn = AudioClip.Create("Coi o to", car.Length, 1, hz, false); carHorn.SetData(car, 0);
            bikeHorn = AudioClip.Create("Coi xe may", bk.Length, 1, hz, false); bikeHorn.SetData(bk, 0);
            for (int i = 0; i < 4; i++)
            {
                var g = new GameObject("Coi_" + i); g.transform.SetParent(transform, false);
                var a = PhoVietAudio.Route(g.AddComponent<AudioSource>());
                a.playOnAwake = false; a.spatialBlend = 1f; a.minDistance = 6f; a.maxDistance = 90f; a.rolloffMode = AudioRolloffMode.Linear; a.volume = 0.7f;
                horns.Add(a);
            }
        }
        void Honk(Vehicle v)
        {
            if (Time.time < v.nextHonk || horns.Count == 0) return;
            if (player && (v.pos - player.position).sqrMagnitude > 80f * 80f) return;
            AudioSource src = null; foreach (var h in horns) if (!h.isPlaying) { src = h; break; }
            if (!src) return;
            src.transform.position = v.pos + Vector3.up; src.clip = v.bike ? bikeHorn : carHorn;
            src.pitch = 0.92f + (float)rnd.NextDouble() * 0.16f; src.Play();
            v.nextHonk = Time.time + 3.5f + (float)rnd.NextDouble() * 3f; Honks++;
        }

        Color paintPick;

        // ------------------------------------------------------------ 21.259 + 21.260: va chạm giữa các phương tiện
        // Luật chung (21.260) cho mọi cú va chạm xe ↔ xe: xe người chơi ↔ NPC, NPC ↔ NPC (xe ẩu, xe bị văng trúng xe khác),
        // CSGT húc NPC (bản thân CSGT chỉ khựng, không nổ):
        // • Vận tốc khép < 90 km/h (FatalClosing): cả hai văng theo vận tốc, bốc khói, choáng 1,2–4 s; hết choáng khói tắt dần,
        //   xe NPC trượt về làn gần nhất rồi chạy tiếp (vẫn móp). Dưới 40 km/h ô tô chỉ bị đẩy nhẹ.
        // • Vận tốc khép ≥ 90 km/h: hai xe cùng cỡ → cả hai văng, khói, 1 s sau nổ; xe nặng hơn ≥ 2,5 lần (BigRatio) chỉ văng +
        //   khói + choáng, không nổ; xe nhẹ thì nổ.
        // • Xe nặng hơn văng ít và choáng ngắn hơn, xe nhẹ văng xa hơn (chia theo tỉ lệ khối lượng).
        [Header("Va chạm phương tiện (21.259/21.260)")]
        public Material smokeMaterial;
        [Tooltip("Vận tốc khép (m/s) từ đó ô tô bị húc văng xa; dưới ngưỡng chỉ bị đẩy nhẹ (40 km/h = 11,1 m/s)")] public float flingSpeed = 11.11f;
        public float explodeDelay = 1.0f;
        public const float FatalClosing = 25f, BigRatio = 2.5f, MinClosing = 2.5f;     // 90 km/h; nặng hơn 2,5 lần; bỏ qua chạm < 9 km/h
        public struct CrashOutcome { public bool explode; public float fling, stun; }
        public static CrashOutcome CrashRule(float self, float other, float closing)
        {
            float share = Mathf.Clamp(2f * other / Mathf.Max(0.01f, self + other), 0.25f, 1.6f);   // 1 = cùng cỡ; < 1 xe này nặng hơn
            float k = Mathf.InverseLerp(MinClosing, FatalClosing, closing);
            return new CrashOutcome
            {
                explode = closing >= FatalClosing && self < other * BigRatio,
                fling = share,
                stun = Mathf.Lerp(1.2f, 4f, k) * Mathf.Clamp(share, 0.35f, 1.15f)
            };
        }
        // Khối lượng (tấn): xe máy + người 0,25; Sport 1,4; Pickup 2,2; xe tải thùng 5; đầu kéo container 12.
        static float ModelMass(string name, float footprint)
        {
            if (name.Contains("Container")) return 12f;
            if (name.Contains("Covered")) return 5f;
            if (name.Contains("Pickup")) return 2.2f;
            if (name.Contains("Sport")) return 1.4f;
            return Mathf.Max(0.3f, footprint * 0.17f);
        }
        public System.Action<Vector3, Vector3, float> ImpactFx;   // điểm chạm, pháp tuyến, vận tốc khép → mảnh vỡ
        public System.Action<Vector3, bool> WreckExploded;        // vị trí nổ, là xe máy
        // 21.300: tia LAZER của xe người chơi — xe trúng bốc cháy 0,45 s rồi nổ tung thành mảnh (OpenWorldHazards.Laser vẽ vụ nổ,
        // không tính sát thương cho xe người chơi), xe biến mất (thu về hồ xe, sinh lại chỗ khác như thường).
        public System.Action<Vector3, Color, bool> LaserExploded; // vị trí, màu sơn, là xe máy
        public int LaserKills { get; private set; }
        public int VehicleIndexOf(Rigidbody rb)
        {
            if (!rb) return -1;
            for (int i = 0; i < pool.Count; i++) if (pool[i].active && pool[i].rb == rb) return i;
            return -1;
        }
        public bool LaserKill(int index, Vector3 point, Vector3 dir)
        {
            if (index < 0 || index >= pool.Count) return false;
            var v = pool[index]; if (!v.active || v.exploded || v.laserKill) return false;
            var o = new CrashOutcome { explode = true, fling = 0.25f, stun = 0f };
            StartWreck(v, point, -dir, point - dir * 2f, dir * 4f, 6f, o, Vector3.zero, false);
            v.laserKill = true; v.explodeAt = Time.time + 0.45f; Smoke(v, true); LaserKills++;
            return true;
        }
        // 21.317: sét đánh trúng xe NPC trong bán kính → bốc cháy rồi nổ tung thành mảnh như trúng LAZER (LaserExploded).
        public int LightningAround(Vector3 p, float radius)
        {
            int n = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                var v = pool[i]; if (!v.active || v.exploded || v.laserKill) continue;
                var d = v.pos - p; d.y = 0f;
                if (d.magnitude > radius + Mathf.Max(v.halfW, v.halfL) * 0.5f) continue;
                if (LaserKill(i, p, d.sqrMagnitude > 0.01f ? d.normalized : Vector3.forward)) n++;
            }
            return n;
        }
        public int Wrecks { get; private set; }
        public int Explosions { get; private set; }
        public int Recovered { get; private set; }
        public int NpcCrashes { get; private set; }
        public int RecklessStarts { get; private set; }
        public int PoliceHits { get; private set; }
        public string LastCrash { get; private set; }
        public bool IsWreck(int index) => index >= 0 && index < pool.Count && pool[index].active && pool[index].wreck;
        public float WreckAge(int index) => IsWreck(index) ? Time.time - pool[index].wreckAt : -1f;
        public bool IsBike(int index) => index >= 0 && index < pool.Count && pool[index].bike;
        // 21.261: hàng rào cần vận tốc xe NPC (kinematic) và làm xe đang văng mất đà khi húc đổ rào.
        public Vector3 NpcVelocity(int index) => index >= 0 && index < pool.Count && pool[index].active ? Velocity(pool[index]) : Vector3.zero;
        public void FenceHit(int index)
        {
            if (index < 0 || index >= pool.Count) return;
            var v = pool[index]; if (!v.active || !v.wreck || v.recovering) return;
            v.wreckVel = new Vector3(v.wreckVel.x * 0.8f, v.wreckVel.y, v.wreckVel.z * 0.8f);
        }
        public bool NearestLanePoint(Vector3 p, out Vector3 q)
        {
            q = p; float bd = float.MaxValue;
            foreach (var L in lanes)
            {
                float sl = Mathf.Clamp(Vector3.Dot(p - L.a, L.dir), 0f, L.len);
                var c = L.a + L.dir * sl; var d = c - p; d.y = 0;
                if (d.sqrMagnitude < bd) { bd = d.sqrMagnitude; q = c; }
            }
            return bd < float.MaxValue;
        }
        public int WallHits { get; private set; }
        public float Mass(int index) => index >= 0 && index < pool.Count ? pool[index].mass : 1.4f;
        // xe đang bay/trượt nhanh (khác xác nằm yên) — đâm vào người chơi thì tính như một cú va chạm
        public bool WreckMoving(int index) => IsWreck(index) && !pool[index].recovering
            && (!pool[index].landed || new Vector2(pool[index].wreckVel.x, pool[index].wreckVel.z).sqrMagnitude > 9f);

        void StartWreck(Vehicle v, Vector3 point, Vector3 normal, Vector3 from, Vector3 pushVel, float closing, CrashOutcome o, Vector3 carry, bool fx = true)
        {
            if (v.waiting) StopWaiting(v);
            if (v.claimed) Release(v);
            if (v.reckless) EndReckless(v);
            v.inConn = false; v.committed = false; v.next = null;
            v.wreck = true; v.exploded = false; v.landed = false; v.recovering = false; v.speed = 0f; v.wreckAt = Time.time;
            v.fatal = o.explode; v.explodeAt = o.explode ? Time.time + explodeDelay : float.MaxValue;
            v.stunFor = o.stun; v.stunUntil = float.MaxValue; v.recWait = 0f;
            var away = v.pos - from; away.y = 0; away = away.sqrMagnitude > 0.01f ? away.normalized : v.fwd;
            var push = new Vector3(pushVel.x, 0, pushVel.z); push = push.sqrMagnitude > 0.5f ? push.normalized : away;
            var dir = (push * 0.6f + away * 0.4f).normalized;
            float h, up, f = o.fling, fu = Mathf.Min(1f, f), sgn = rnd.NextDouble() < .5 ? -1 : 1;
            if (v.bike) { h = (4f + closing * 0.55f) * f; up = (2.5f + closing * 0.22f) * fu; v.wreckSpin = new Vector3(0, sgn * (180f + closing * 12f) * fu, 0); }
            else if (closing >= flingSpeed) { h = closing * 0.45f * f; up = (2f + closing * 0.15f) * fu; v.wreckSpin = new Vector3(0, sgn * (120f + closing * 8f) * fu, 0); }
            else { h = (1.2f + closing * 0.1f) * f; up = 0f; v.wreckSpin = new Vector3(0, sgn * 25f, 0); }
            carry.y = 0;
            v.wreckVel = dir * h + Vector3.up * up + carry;
            // hoạt ảnh té / giật
            var local = v.root.InverseTransformPoint(point);
            v.hitClip = v.bike ? (local.x > 0 ? "FallLeft" : "FallRight") : (Mathf.Abs(local.x) / Mathf.Max(0.3f, v.halfW) > Mathf.Abs(local.z) / Mathf.Max(0.5f, v.halfL) ? (local.x > 0 ? "ImpactRight" : "ImpactLeft") : (local.z > 0 ? "ImpactFront" : "ImpactRear"));
            v.hitUntil = float.MaxValue; v.recoverAt = 0f;
            if (v.anim && v.anim.GetClip(v.hitClip)) { v.anim.Stop(); v.anim.Play(v.hitClip); var st = v.anim[v.hitClip]; if (st != null) { st.speed = 1f; st.wrapMode = WrapMode.ClampForever; } v.clip = v.hitClip; }
            if (v.bike) BeginFall(v, local.x, closing);
            DentAt(v, point, closing);
            Smoke(v, false);
            SetBrake(v, false, true);
            Hits++; Wrecks++;
            if (fx) ImpactFx?.Invoke(point, normal, closing);
        }

        // ---- 21.284: xe máy nghiêng ngã (người dùng: «văng ra nhưng vẫn thẳng đứng như đang chạy là không đúng»)
        // • Ngã về phía ngược chỗ bị va (va bên phải → đổ sang trái). Va chạm càng mạnh (vận tốc khép 9 → 90 km/h) đổ càng nhanh;
        //   từ ~55 km/h còn lộn thêm một vòng trên không, chúi đầu/bốc đầu theo nhịp văng.
        // • Đang bay: nghiêng dần; chạm đất (hoặc bị hất nhẹ tại chỗ): nằm hẳn xuống mặt đường (nghiêng 80°), nâng gốc 0,28 m để
        //   tay lái/gác chân không chìm xuống đường. Dựng xe về làn (TickRecover / «Recover») thì đứng dần lên.
        const float LieRoll = 80f, LieLift = 0.28f, KnockRoll = 62f;
        void BeginFall(Vehicle v, float hitLocalX, float closing)
        {
            v.fallSign = hitLocalX > 0f ? 1f : -1f;                      // Euler z dương: đổ sang trái (−x)
            v.fallSev = Mathf.InverseLerp(MinClosing, FatalClosing, closing);
            if (v.fallStart < 0f) { v.fallRoll = 0f; v.fallPitch = 0f; }
            v.fallStart = Time.time;
        }
        Quaternion FallPose(Vehicle v, float now)
        {
            float dt = Time.deltaTime, k = v.fallSev;
            bool down = !v.wreck ? now < v.hitUntil && (v.recoverAt <= 0f || now < v.recoverAt) : !v.recovering;
            float extra = k > 0.55f ? 360f : 0f;
            float target = down ? v.fallSign * ((v.wreck ? LieRoll : KnockRoll) + extra) : 0f;   // hất nhẹ tại chỗ (không nâng gốc) đổ 62°
            bool flying = v.wreck && !v.landed && !v.recovering;
            float rate = down ? (flying ? 220f + 520f * k : 300f) : 160f;
            if (v.recovering) target = v.fallSign * LieRoll * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(v.recoverT)));
            else if (!down && Mathf.Abs(v.fallRoll) > 180f) v.fallRoll -= Mathf.Sign(v.fallRoll) * 360f;
            if (v.wreck && !flying && Mathf.Abs(v.fallRoll) > 180f && Mathf.Abs(target) < 180f) v.fallRoll -= Mathf.Sign(v.fallRoll) * 360f;
            v.fallRoll = v.recovering ? target : Mathf.MoveTowards(v.fallRoll, target, rate * dt);
            float pitchWant = flying ? Mathf.Sin((now - v.fallStart) * (6f + 6f * k)) * (8f + 22f * k) : 0f;
            v.fallPitch = Mathf.MoveTowards(v.fallPitch, pitchWant, 120f * dt);
            float lie = Mathf.Abs(Mathf.Sin(v.fallRoll * Mathf.Deg2Rad));
            v.fallLift = LieLift * lie * (v.bike ? bikeScale : 1f);
            if (!down && !v.recovering && Mathf.Abs(v.fallRoll) < 0.5f && Mathf.Abs(v.fallPitch) < 0.5f) { v.fallStart = -1f; v.fallLift = 0f; return Quaternion.Euler(0, 0, v.lean); }
            return Quaternion.Euler(v.fallPitch, 0f, v.fallRoll);
        }

        void DentAt(Vehicle v, Vector3 point, float closing)
        {
            if (v.bike) return;
            if (v.dent == null) v.dent = new OpenWorldCarDent(v.root, v.renderers);
            var outward = point - v.pos; outward.y = 0;
            v.dent.Dent(point, outward, closing);
        }

        readonly RaycastHit[] wreckHits = new RaycastHit[6];
        float WreckGround(Vehicle v)
        {
            int n = Physics.RaycastNonAlloc(v.pos + Vector3.up * 4f, Vector3.down, wreckHits, 12f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
            float best = -6f;
            for (int i = 0; i < n; i++)
            {
                var col = wreckHits[i].collider; if (col.attachedRigidbody != null) continue;
                if (col.name != "MatDat" && col.name != "MatCau") continue;                 // chỉ mặt đất/mặt cầu, không nóc bồn cây/ghế
                if (wreckHits[i].point.y > best && wreckHits[i].point.y < v.pos.y + 1.5f) best = wreckHits[i].point.y;
            }
            return best;
        }

        void TickWreck(Vehicle v, float dt)
        {
            if (v.recovering) { TickRecover(v, dt); return; }
            float ground = WreckGround(v);
            if (!v.landed)
            {
                v.wreckVel += Physics.gravity * dt;
                var step = v.wreckVel * dt; BlockWreck(v, ref step);
                v.pos += step;
                v.rot = Quaternion.Euler(v.wreckSpin * dt) * v.rot;
                if (v.pos.y <= ground && v.wreckVel.y < 0f)
                {
                    v.pos.y = ground;
                    var hv = new Vector3(v.wreckVel.x, 0, v.wreckVel.z);
                    if (hv.magnitude > 3f && -v.wreckVel.y > 2f) { v.wreckVel = hv * 0.6f + Vector3.up * (-v.wreckVel.y * 0.25f); v.wreckSpin *= 0.5f; }
                    else { v.landed = true; v.wreckVel = hv; if (!v.fatal) v.stunUntil = Time.time + v.stunFor; }
                }
            }
            else
            {
                var hv = Vector3.MoveTowards(v.wreckVel, Vector3.zero, 7f * dt); v.wreckVel = hv;
                v.wreckSpin = Vector3.MoveTowards(v.wreckSpin, Vector3.zero, 300f * dt);
                var step = hv * dt; BlockWreck(v, ref step);
                v.pos += step; v.pos.y = Mathf.MoveTowards(v.pos.y, ground + (v.bike ? v.fallLift : 0f), 4f * dt);
                v.rot = Quaternion.Euler(v.wreckSpin * dt) * v.rot;
            }
            v.fwd = v.rot * Vector3.forward;
            // 21.260: xe đang văng nhanh trúng xe khác → va chạm dây chuyền
            var flat = new Vector3(v.wreckVel.x, 0, v.wreckVel.z);
            if (!v.exploded && flat.sqrMagnitude > MinClosing * MinClosing) SweepHits(v, flat);
            if (v.fatal)
            {
                if (!v.exploded && Time.time >= v.explodeAt)
                {
                    v.exploded = true; Explosions++;
                    if (v.laserKill) { LaserExploded?.Invoke(v.pos + Vector3.up * 0.6f, v.bike ? new Color(.2f, .22f, .26f) : v.paint, v.bike); Despawn(v); return; }
                    WreckExploded?.Invoke(v.pos + Vector3.up * 0.6f, v.bike);
                    if (v.bike) { Despawn(v); return; }
                    // xác cháy: sẫm màu + lửa/khói đen
                    mpb.Clear(); mpb.SetColor(PaintId, new Color(.12f, .11f, .10f)); mpb.SetColor("_Color", new Color(.32f, .30f, .28f));
                    foreach (var r in v.renderers) if (r) r.SetPropertyBlock(mpb);
                    foreach (var r in v.brakeLights) if (r) r.enabled = false;
                    Smoke(v, true);
                }
            }
            else if (v.landed)
            {
                // choáng: khói giảm dần trong 1,2 s cuối rồi tắt; hết choáng → về làn chạy tiếp
                float left = v.stunUntil - Time.time;
                if (v.smoke && v.smoke.isEmitting) { var em = v.smoke.emission; em.rateOverTime = SmokeRate * Mathf.Clamp01(left / 1.2f); }
                if (left <= 0f) { BeginRecover(v); if (!v.active) return; }
            }
            v.rb.MovePosition(v.pos); v.rb.MoveRotation(v.rot);
        }

        // ---- 21.261: xe NPC kinematic không tự va vật tĩnh → quét hộp theo bước di chuyển; trúng nhà/cây/cột/lan can thì dừng
        // sát vật, bật ngược 30 % theo pháp tuyến (không xuyên vào trong nhà).
        readonly RaycastHit[] castHits = new RaycastHit[12];
        bool BlockWreck(Vehicle v, ref Vector3 step)
        {
            var flat = new Vector3(step.x, 0, step.z); float dist = flat.magnitude;
            if (dist < 1e-4f) return false;
            var dir = flat / dist;
            var center = v.pos + Vector3.up * 1.0f;
            var half = new Vector3(v.halfW * 0.85f, 0.42f, v.halfL * 0.85f);
            int n = Physics.BoxCastNonAlloc(center, half, dir, castHits, Quaternion.LookRotation(FlatDir(v.fwd)), dist + 0.05f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue; int bi = -1;
            for (int i = 0; i < n; i++)
            {
                var h = castHits[i]; var col = h.collider;
                if (col.attachedRigidbody != null || col.name == "MatDat" || col.name == "MatCau") continue;
                if (h.distance <= 0f && Vector3.Dot(h.normal, dir) > -0.1f) continue;          // chồng sẵn mà đang đi ra
                if (h.distance < best) { best = h.distance; bi = i; }
            }
            if (bi < 0) return false;
            var hit = castHits[bi]; var nrm = hit.normal; nrm.y = 0; nrm = nrm.sqrMagnitude > 1e-4f ? nrm.normalized : -dir;
            float allow = Mathf.Max(0f, best - 0.05f);
            step = new Vector3(dir.x * allow, step.y, dir.z * allow);
            var hv = new Vector3(v.wreckVel.x, 0, v.wreckVel.z); float into = -Vector3.Dot(hv, nrm);
            if (into > 0f)
            {
                hv = Vector3.Reflect(hv, nrm) * 0.3f;
                v.wreckVel = new Vector3(hv.x, v.wreckVel.y, hv.z); v.wreckSpin *= 0.5f;
                if (into > 3f) { ImpactFx?.Invoke(hit.distance > 0f ? hit.point : v.pos + Vector3.up, nrm, into); DentAt(v, hit.distance > 0f ? hit.point : v.pos + dir * v.halfL, into); }
                WallHits++;
            }
            return true;
        }

        // ---- 21.260: hết choáng → trượt về làn gần nhất (cùng hướng xe đang quay nếu được) rồi chạy tiếp
        const float SmokeRate = 30f;
        void BeginRecover(Vehicle v)
        {
            if (v.pos.y < OpenWorldDriver.WaterY + 0.3f) { Despawn(v); return; }       // rơi xuống sông: chìm luôn
            Lane best = null; float bd = float.MaxValue, bs = 0f;
            foreach (var L in lanes)
            {
                if (L.len < 2f * v.halfL + 4f) continue;
                float sl = Mathf.Clamp(Vector3.Dot(v.pos - L.a, L.dir), v.halfL + 1f, L.len - v.halfL - 3f);
                var dd = L.a + L.dir * sl - v.pos; dd.y = 0;
                float d = dd.sqrMagnitude + (Vector3.Dot(L.dir, v.fwd) < 0f ? 6f : 0f);
                if (d < bd) { bd = d; best = L; bs = sl; }
            }
            if (best == null || bd > 25f * 25f) { Despawn(v); return; }
            var target = best.a + best.dir * bs;
            foreach (var o in pool)                                                    // chỗ trên làn còn bị chiếm → chờ thêm
            {
                if (!o.active || o == v) continue;
                var r = o.pos - target; r.y = 0;
                if (r.magnitude < v.halfL + Mathf.Max(o.halfL, o.halfW) + 0.8f) { WaitRecover(v); return; }
            }
            if (player) { var r = cachedPlayer - target; r.y = 0; if (r.magnitude < v.halfL + 3f) { WaitRecover(v); return; } }
            v.recWait = 0f;
            v.recovering = true; v.recoverT = 0f; v.recFromPos = v.pos; v.recFromRot = v.rot; v.recLane = best; v.recS = bs;
            v.recDur = Mathf.Clamp(Mathf.Sqrt(bd) / 3f, 0.8f, 2.5f);
            if (v.smoke) v.smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);   // hạt cũ tan dần
            if (v.bike) v.recoverAt = Time.time - 0.01f;                                   // LateUpdate phát «Recover»
        }

        // chỗ về làn bị chiếm: chờ thêm 0,6 s (khói tắt hẳn); chờ quá 6 s mà ngoài khung hình thì thu hồi xe
        void WaitRecover(Vehicle v)
        {
            v.stunUntil = Time.time + 0.6f; v.recWait += 0.6f;
            if (v.smoke && v.smoke.isEmitting) v.smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (v.recWait < 6f) return;
            var cam = Camera.main;
            if (cam) { var vp = cam.WorldToViewportPoint(v.pos + Vector3.up); if (vp.z > 0 && vp.x > -0.1f && vp.x < 1.1f && vp.y > -0.1f && vp.y < 1.1f) return; }
            Despawn(v);
        }

        void TickRecover(Vehicle v, float dt)
        {
            v.recoverT += dt / Mathf.Max(0.1f, v.recDur);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(v.recoverT));
            var L = v.recLane;
            v.pos = Vector3.Lerp(v.recFromPos, L.a + L.dir * v.recS, t);
            v.rot = Quaternion.Slerp(v.recFromRot, Quaternion.LookRotation(L.dir), t); v.fwd = v.rot * Vector3.forward;
            if (v.recoverT >= 1f)
            {
                v.wreck = false; v.recovering = false; v.landed = false; v.fatal = false;
                v.lane = L; v.s = v.recS; v.inConn = false; v.committed = false; v.claimed = false; v.waiting = false; v.exitBlocked = false; v.conn = null;
                v.next = PickNext(L, v); v.speed = 0f; v.prevSpeed = 0f; v.lateral = 0f; v.shift = 0f; v.stopTime = 0f; v.blockedTime = 0f;
                v.hitUntil = 0f; v.recoverAt = 0f; v.clip = null; v.lean = 0f; v.fallStart = -1f; v.fallLift = 0f; v.visual.localRotation = Quaternion.identity;
                Recovered++;
            }
            v.rb.MovePosition(v.pos); v.rb.MoveRotation(v.rot);
        }

        // ---- 21.260: va chạm NPC ↔ NPC (xe NPC là kinematic nên tự dò chồng hộp 2D)
        static Vector3 FlatDir(Vector3 f) { f.y = 0; return f.sqrMagnitude > 1e-4f ? f.normalized : Vector3.forward; }
        static bool Separated(Vector3 ax, Vector3 d, Vector3 af, Vector3 ar, float aL, float aW, Vector3 bf, Vector3 br, float bL, float bW)
        {
            float ra = aL * Mathf.Abs(Vector3.Dot(af, ax)) + aW * Mathf.Abs(Vector3.Dot(ar, ax));
            float rb = bL * Mathf.Abs(Vector3.Dot(bf, ax)) + bW * Mathf.Abs(Vector3.Dot(br, ax));
            return Mathf.Abs(Vector3.Dot(d, ax)) > ra + rb;
        }
        static bool Overlap(Vehicle a, Vehicle b)
        {
            if (Mathf.Abs(a.pos.y - b.pos.y) > 1.6f) return false;                      // bay qua nóc
            var d = b.pos - a.pos; d.y = 0;
            float r = a.halfL + a.halfW + b.halfL + b.halfW; if (d.sqrMagnitude > r * r) return false;
            Vector3 af = FlatDir(a.fwd), bf = FlatDir(b.fwd), ar = new Vector3(af.z, 0, -af.x), br = new Vector3(bf.z, 0, -bf.x);
            return !(Separated(af, d, af, ar, a.halfL, a.halfW, bf, br, b.halfL, b.halfW) || Separated(ar, d, af, ar, a.halfL, a.halfW, bf, br, b.halfL, b.halfW)
                  || Separated(bf, d, af, ar, a.halfL, a.halfW, bf, br, b.halfL, b.halfW) || Separated(br, d, af, ar, a.halfL, a.halfW, bf, br, b.halfL, b.halfW));
        }
        static Vector3 Velocity(Vehicle v) => v.wreck ? (v.recovering ? Vector3.zero : new Vector3(v.wreckVel.x, 0, v.wreckVel.z)) : FlatDir(v.fwd) * v.speed;

        // xe `v` (đang văng hoặc xe ẩu) chồng lên xe khác với vận tốc khép ≥ 9 km/h → va chạm
        void SweepHits(Vehicle v, Vector3 vel)
        {
            foreach (var o in pool)
            {
                if (!o.active || o == v || o.exploded) continue;
                if ((v.lastHit == o.index && Time.time - v.lastHitAt < 0.8f) || (o.lastHit == v.index && Time.time - o.lastHitAt < 0.8f)) continue;
                if (!Overlap(v, o)) continue;
                var ov = Velocity(o); float closing = (vel - ov).magnitude;
                if (closing < MinClosing) continue;
                Collide(v, o, closing, vel, ov);
                if (!v.active || !v.wreck) return;
            }
        }

        void Collide(Vehicle a, Vehicle b, float closing, Vector3 va, Vector3 vb)
        {
            float now = Time.time;
            a.lastHit = b.index; a.lastHitAt = now; b.lastHit = a.index; b.lastHitAt = now;
            var n = a.pos - b.pos; n.y = 0; n = n.sqrMagnitude > 1e-4f ? n.normalized : -FlatDir(a.fwd);
            var point = Vector3.Lerp(b.pos, a.pos, b.halfW / Mathf.Max(0.1f, a.halfW + b.halfW)) + Vector3.up * 0.75f;
            var oa = CrashRule(a.mass, b.mass, closing); var ob = CrashRule(b.mass, a.mass, closing);
            NpcCrashes++;
            LastCrash = $"{a.visual.name}({a.mass:0.##}t{(a.reckless ? ", ẩu" : "")}) ↔ {b.visual.name}({b.mass:0.##}t) {closing * 3.6f:0} km/h → {(oa.explode ? "nổ" : "choáng " + oa.stun.ToString("0.0") + " s")} / {(ob.explode ? "nổ" : "choáng " + ob.stun.ToString("0.0") + " s")}";
            Strike(a, oa, point, n, b.pos, vb, va, closing, true);
            Strike(b, ob, point, -n, a.pos, va, vb, closing, false);
        }

        void Strike(Vehicle v, CrashOutcome o, Vector3 point, Vector3 normal, Vector3 from, Vector3 otherVel, Vector3 ownVel, float closing, bool fx)
        {
            if (!v.wreck) { StartWreck(v, point, normal, from, otherVel, closing, o, ownVel * 0.35f, fx); return; }
            // đang văng/choáng: nhận thêm xung; có thể chuyển sang nổ
            if (fx) ImpactFx?.Invoke(point, normal, closing);
            DentAt(v, point, closing);
            if (o.explode && !v.fatal) { v.fatal = true; v.explodeAt = Time.time + explodeDelay; v.recovering = false; }
            var push = otherVel; push.y = 0;
            v.wreckVel = new Vector3(v.wreckVel.x * 0.4f, v.wreckVel.y, v.wreckVel.z * 0.4f) + push * 0.45f * o.fling;
            if (v.recovering) { v.recovering = false; v.landed = true; }
            if (!v.fatal) { v.stunFor = Mathf.Max(v.stunFor, o.stun); if (v.landed) v.stunUntil = Mathf.Max(v.stunUntil, Time.time + o.stun); Smoke(v, false); }
            Hits++;
        }

        // ---- 21.260: xe ẩu — thỉnh thoảng một xe NPC gần người chơi chạy nhanh, vượt đèn đỏ, không giữ khoảng cách với xe NPC khác
        [Header("Xe ẩu (21.260)")]
        public float recklessEveryMin = 14f, recklessEveryMax = 26f;
        public float recklessSpeedMin = 16f, recklessSpeedMax = 24f, recklessFor = 22f;
        public int recklessMax = 2;
        float nextReckless = 12f;

        bool TryMakeReckless(Vector3 pp)
        {
            int n = 0; foreach (var v in pool) if (v.active && v.reckless) n++;
            if (n >= recklessMax) return false;
            var cam = Camera.main; Vehicle pick = null; double bestScore = -1;
            foreach (var v in pool)
            {
                if (!v.active || v.wreck || v.reckless || v.inConn || v.committed) continue;
                var d = v.pos - pp; d.y = 0; float dist = d.magnitude;
                if (dist < 35f || dist > 150f) continue;
                bool seen = false;
                if (cam) { var vp = cam.WorldToViewportPoint(v.pos + Vector3.up); seen = vp.z > 0 && vp.x > 0.05f && vp.x < 0.95f && vp.y > 0.05f && vp.y < 0.95f; }
                double score = (seen ? 2.0 : 0.0) + (v.bike ? 0.0 : 0.6) + rnd.NextDouble();
                if (score > bestScore) { bestScore = score; pick = v; }
            }
            if (pick == null) return false;
            MakeReckless(pick, Mathf.Lerp(recklessSpeedMin, recklessSpeedMax, (float)rnd.NextDouble()), recklessFor);
            return true;
        }
        void MakeReckless(Vehicle v, float speed, float seconds)
        {
            if (v.waiting) StopWaiting(v);
            v.reckless = true; v.vmaxBase = v.vmax; v.vmax = v.bike ? Mathf.Min(speed, 19f) : speed; v.recklessUntil = Time.time + seconds;
            v.nextHonk = 0f; Honk(v); RecklessStarts++;
        }
        void EndReckless(Vehicle v) { if (!v.reckless) return; v.reckless = false; v.vmax = v.vmaxBase; }

        void Smoke(Vehicle v, bool fire)
        {
            if (!smokeMaterial) return;
            if (!v.smoke)
            {
                var go = new GameObject("KhoiXeBiHuc"); go.transform.SetParent(v.root, false); go.transform.localPosition = new Vector3(0, v.bike ? 0.8f * bikeScale : 1.1f, v.bike ? 0f : v.halfL * 0.45f);
                var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var m = ps.main; m.loop = true; m.simulationSpace = ParticleSystemSimulationSpace.World; m.maxParticles = 120; m.gravityModifier = -0.07f;
                m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
                var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.35f;
                var sheet = ps.textureSheetAnimation; sheet.enabled = true; sheet.numTilesX = 4; sheet.numTilesY = 2;
                var col = ps.colorOverLifetime; col.enabled = true; var gr = new Gradient();
                gr.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(.9f, 0), new GradientAlphaKey(0, 1) }); col.color = gr;
                var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .6f, 1, 2f));
                var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = smokeMaterial; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                v.smoke = ps;
            }
            var mm = v.smoke.main; var em = v.smoke.emission; var sa = v.smoke.textureSheetAnimation;
            if (fire)
            {   // lửa → khói đen
                mm.startLifetime = 2.2f; mm.startSpeed = 2.4f; mm.startSize = new ParticleSystem.MinMaxCurve(1.4f, 2.6f); mm.startColor = Color.white;
                em.rateOverTime = 18f; sa.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .45f, 1, .999f));
            }
            else
            {   // khói xám bốc lên (choáng hoặc trước khi nổ) — 21.260: dày và to hơn để nhìn rõ từ xa
                mm.startLifetime = 1.9f; mm.startSpeed = 2f; mm.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.3f); mm.startColor = new Color(.42f, .41f, .39f, .9f);
                em.rateOverTime = SmokeRate; sa.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .75f, 1, .999f));
            }
            v.smoke.transform.rotation = Quaternion.identity;
            if (!v.smoke.isPlaying) v.smoke.Play();
        }

        void ClearWreck(Vehicle v)
        {
            EndReckless(v); v.lastHit = -1;
            if (!v.wreck && (v.dent == null || !v.dent.Damaged)) return;
            v.wreck = false; v.exploded = false; v.landed = false; v.fatal = false; v.recovering = false;
            if (v.dent != null) v.dent.Restore();
            if (v.smoke) v.smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            mpb.Clear(); if (!v.bike) mpb.SetColor(PaintId, v.paint);
            foreach (var r in v.renderers) if (r) r.SetPropertyBlock(mpb);
            foreach (var r in v.brakeLights) if (r) r.enabled = true;
        }

        // ------------------------------------------------------------ bị đâm
        internal void OnHit(int index, Collision c)
        {
            if (index < 0 || index >= pool.Count) return;
            var v = pool[index]; if (!v.active || v.wreck || c.rigidbody == null) return;
            // 21.259/21.260: xe người chơi hoặc xe CSGT húc → luật va chạm chung theo vận tốc khép + khối lượng
            var pv = c.rigidbody.GetComponent<OpenWorldDriver>();
            var pol = pv ? null : c.rigidbody.GetComponent<OpenWorldPolice>();
            if (!pv && !pol) return;
            float closing = c.relativeVelocity.magnitude;
            if (closing < MinClosing) return;
            var ct = c.GetContact(0);
            var o = CrashRule(v.mass, pv ? pv.massTons : pol.massTons, closing);
            float sp = pv ? Mathf.Abs(pv.Speed) : pol.Speed;
            StartWreck(v, ct.point, ct.normal, c.rigidbody.position, c.rigidbody.transform.forward * Mathf.Max(sp, closing), closing, o, Velocity(v) * 0.35f);
            if (pol) PoliceHits++;
        }

        // 21.255: vụ nổ đạn pháo / đuôi, thân khủng long / xe tăng hất các xe NPC trong bán kính (cùng phản ứng như bị đâm).
        public int KnockAround(Vector3 p, float radius)
        {
            int n = 0;
            foreach (var v in pool)
            {
                if (!v.active || v.wreck || Time.time < v.hitUntil) continue;
                var d = v.pos - p; d.y = 0;
                if (d.magnitude > radius + Mathf.Max(v.halfW, v.halfL) * 0.6f) continue;
                HitReaction(v, p); n++;
            }
            return n;
        }

        void HitReaction(Vehicle v, Vector3 point)
        {
            var local = v.root.InverseTransformPoint(point);
            bool side = Mathf.Abs(local.x) / Mathf.Max(0.3f, v.halfW) > Mathf.Abs(local.z) / Mathf.Max(0.5f, v.halfL);
            if (v.bike)
            {
                v.hitClip = local.x > 0 ? "FallLeft" : "FallRight";       // bị đâm bên phải → ngã sang trái
                v.hitUntil = Time.time + 3.6f; v.recoverAt = Time.time + 2.4f;
                BeginFall(v, local.x, 6f);
            }
            else
            {
                v.hitClip = side ? (local.x > 0 ? "ImpactRight" : "ImpactLeft") : (local.z > 0 ? "ImpactFront" : "ImpactRear");
                v.hitUntil = Time.time + 2.5f; v.recoverAt = 0f;
            }
            v.speed = 0f; Hits++;
            v.nextHonk = 0f; Honk(v);
            if (v.anim && v.anim.GetClip(v.hitClip)) { v.anim.Stop(); v.anim.Play(v.hitClip); var st = v.anim[v.hitClip]; if (st != null) { st.speed = 1f; st.wrapMode = WrapMode.ClampForever; } v.clip = v.hitClip; }
        }

        // ------------------------------------------------------------ sinh / thu hồi
        bool TrySpawn(Vehicle v, float minR, float maxR, bool hideFromCamera, int tries)
        {
            var pp = player ? player.position : Vector3.zero;
            var cam = Camera.main;
            for (int t = 0; t < tries; t++)
            {
                float pick = (float)rnd.NextDouble() * laneTotal; Lane L = lanes[0];
                foreach (var l in lanes) { L = l; pick -= l.len; if (pick < 0) break; }
                if (v.bike && L.nk == 2 && L.k == 0 && rnd.NextDouble() < 0.7) continue;    // xe máy thường đi làn ngoài (sát lề)
                float lo = v.halfL + 1f, hi = L.len - v.halfL - 6f; if (hi <= lo) continue;
                float s = lo + (float)rnd.NextDouble() * (hi - lo);
                var p = L.a + L.dir * s; float d = Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(pp.x, 0, pp.z));
                if (d < minR || d > maxR) continue;
                if (hideFromCamera && cam)
                {
                    var vp = cam.WorldToViewportPoint(p + Vector3.up);
                    if (vp.z > 0 && vp.x > -0.1f && vp.x < 1.1f && vp.y > -0.1f && vp.y < 1.1f && d < 230f) continue;
                }
                bool clear = true;
                foreach (var o in pool) if (o.active && o != v && (o.pos - p).sqrMagnitude < 14f * 14f) { clear = false; break; }
                if (!clear) continue;
                v.lane = L; v.s = s; v.inConn = false; v.committed = false; v.claimed = false; v.waiting = false; v.exitBlocked = false; v.conn = null;
                v.next = PickNext(L, v); v.speed = v.vmax * 0.7f; v.prevSpeed = v.speed; v.stopTime = 0; v.ghostUntil = 0; v.shift = 0; v.blockedTime = 0;
                v.hitUntil = 0; v.recoverAt = 0; v.filterUntil = 0; v.honkBlocked = 0; v.claimNode = null;
                ClearWreck(v); v.laserKill = false;
                v.baseLateral = v.bike ? ((float)rnd.NextDouble() * 2f - 1f) * 0.7f : 0f; v.lateral = v.baseLateral;
                v.pos = p + L.right * v.lateral; v.fwd = L.dir; v.rot = Quaternion.LookRotation(L.dir);
                v.root.SetPositionAndRotation(v.pos, v.rot); v.rb.position = v.pos; v.rb.rotation = v.rot;
                v.root.gameObject.SetActive(true); v.active = true; v.clip = null; v.lean = 0; v.fallStart = -1f; v.fallLift = 0f; v.visual.localRotation = Quaternion.identity;
                SetBrake(v, false, true);
                if (v.bike && bikeMaterials != null && bikeMaterials.Length > 0)
                {
                    var m = bikeMaterials[rnd.Next(bikeMaterials.Length)];
                    foreach (var r in v.renderers) if (r.name.Contains("UrbanRider")) r.sharedMaterial = m;
                }
                Spawned++;
                return true;
            }
            return false;
        }

        Conn PickNext(Lane L, Vehicle v)
        {
            if (L.outs.Count == 0) return null;
            float total = 0; foreach (var c in L.outs) total += c.turn ? 1f : 2.2f;
            float pick = (float)rnd.NextDouble() * total;
            foreach (var c in L.outs) { pick -= c.turn ? 1f : 2.2f; if (pick < 0) return c; }
            return L.outs[L.outs.Count - 1];
        }

        void Despawn(Vehicle v)
        {
            if (v.waiting) StopWaiting(v);
            if (v.claimed) Release(v);
            v.active = false; v.committed = false; v.claimed = false; v.inConn = false; v.root.gameObject.SetActive(false);
        }

        void SetBrake(Vehicle v, bool on, bool force = false)
        {
            if (!force && v.braking == on) return;
            v.braking = on;
            var m = on ? brakeMaterial : tailMaterial; if (!m) return;
            foreach (var r in v.brakeLights) if (r) r.sharedMaterial = m;
        }

        // ------------------------------------------------------------ giao lộ
        bool CanEnter(Node n, int app, Vehicle v)
        {
            float now = Time.time;
            if (n.signal)
            {
                int axis = app % 2, st = SignalState(n, axis, now);
                if (st == 0) return false;
                if (st == 1 && v.speed < 5f) return false;                         // vàng: chỉ xe không kịp dừng
                if (n.inApp[(app + 1) % 4] + n.inApp[(app + 3) % 4] > 0) return false;  // xe pha trước còn trong giao lộ
                int opp = (app + 2) % 4, kind = v.next != null ? v.next.kind : 0;
                if (kind == 2)
                {
                    if (n.inApp[opp] - n.leftIn[opp] > 0) return false;             // rẽ trái nhường xe ngược chiều
                    if (OpposingApproaching(n, opp)) return false;
                }
                else if (n.leftIn[opp] > 0) return false;
                return true;
            }
            if (n.count > 0 && n.active != app) return false;
            float mine = n.waitCount[app] > 0 ? n.waitStart[app] : now;
            for (int a = 0; a < 4; a++)
            {
                if (a == app || n.waitCount[a] == 0) continue;
                if (n.count > 0) { if (now - n.waitStart[a] > 2.5f) return false; }
                else if (n.waitStart[a] < mine) return false;
            }
            return true;
        }
        bool OpposingApproaching(Node n, int opp)
        {
            foreach (var o in pool)
            {
                if (!o.active || o.inConn || o.committed || o.lane.end != n || o.lane.approach != opp) continue;
                if (o.next == null || o.next.kind == 2) continue;
                float toStop = o.lane.len - o.s - o.halfL;
                if (toStop < 30f && o.speed > 1.5f) return true;
            }
            return false;
        }
        bool TryEnter(Vehicle v, bool ghost)
        {
            if (ghost) { Commit(v, true); return true; }
            if (!ExitRoom(v.next.to, v)) { if (v.waiting) StopWaiting(v); v.exitBlocked = true; return false; }
            v.exitBlocked = false;
            if (CanEnter(v.lane.end, v.lane.approach, v)) { Commit(v, false); return true; }
            if (!v.waiting) { StartWaiting(v); if (v.lane.end.signal) RedStops++; }
            return false;
        }
        void Commit(Vehicle v, bool ghost)
        {
            if (v.waiting) StopWaiting(v);
            v.stopTime = 0f;                                                   // thời gian chờ đèn không tính là kẹt sau khi được đi
            if (!ghost)
            {
                var n = v.lane.end; int app = v.lane.approach; bool left = v.next != null && v.next.kind == 2;
                n.count++; n.active = app; n.inApp[app]++; if (left) n.leftIn[app]++;
                v.claimed = true; v.claimNode = n; v.claimApp = app; v.claimLeft = left;
            }
            v.committed = true;
        }
        void Release(Vehicle v)
        {
            var n = v.claimNode; if (n == null) { v.claimed = false; return; }
            n.count = Mathf.Max(0, n.count - 1); if (n.count == 0) n.active = -1;
            n.inApp[v.claimApp] = Mathf.Max(0, n.inApp[v.claimApp] - 1);
            if (v.claimLeft) n.leftIn[v.claimApp] = Mathf.Max(0, n.leftIn[v.claimApp] - 1);
            v.claimed = false; v.claimNode = null;
        }
        void StartWaiting(Vehicle v)
        {
            var n = v.lane.end; int a = v.lane.approach;
            if (n.waitCount[a]++ == 0) n.waitStart[a] = Time.time;
            v.waiting = true;
        }
        void StopWaiting(Vehicle v)
        {
            var n = v.lane.end; int a = v.lane.approach;
            n.waitCount[a] = Mathf.Max(0, n.waitCount[a] - 1); v.waiting = false;
        }

        // ------------------------------------------------------------ mô phỏng
        int respawnCursor;
        Vector3 cachedPlayer;
        void Update() { UpdateSignalLenses(false); }

        void FixedUpdate()
        {
            if (lanes.Count == 0) return;
            float dt = Time.fixedDeltaTime, now = Time.time;
            var pp = player ? player.position : Vector3.zero; cachedPlayer = pp;
            int spawnBudget = 2;
            for (int i = 0; i < pool.Count; i++)
            {
                var v = pool[(respawnCursor + i) % pool.Count];
                if (v.active && (new Vector2(v.pos.x - pp.x, v.pos.z - pp.z)).sqrMagnitude > despawnRadius * despawnRadius) Despawn(v);
                if (!v.active && spawnBudget > 0) { spawnBudget--; TrySpawn(v, spawnMin, spawnMax, true, 12); }
            }
            respawnCursor = (respawnCursor + 3) % Mathf.Max(1, pool.Count);
            if (recklessEveryMax > 0f && now >= nextReckless)
            {
                nextReckless = now + Mathf.Lerp(recklessEveryMin, recklessEveryMax, (float)rnd.NextDouble());
                if (player && !TryMakeReckless(pp)) nextReckless = now + 3f;
            }

            int act = 0;
            foreach (var v in pool)
            {
                if (!v.active) continue; act++;
                if (v.wreck) { TickWreck(v, dt); continue; }
                if (v.reckless && now > v.recklessUntil) EndReckless(v);
                bool ghost = now < v.ghostUntil, hit = now < v.hitUntil, wild = v.reckless;
                // 1) khoảng trống phía trước (xe khác + xe người chơi)
                float gap = 60f, blockerSpeed = 99f; bool blockerPlayer = false, blockerBike = false;
                if (!ghost && !wild)                                                 // 21.260: xe ẩu không giữ khoảng cách với xe NPC
                    foreach (var o in pool)
                    {
                        if (!o.active || o == v) continue;
                        var rel = o.pos - v.pos; float al = rel.x * v.fwd.x + rel.z * v.fwd.z;
                        if (al <= 0 || al > 40f) continue;
                        if (!o.wreck && o.fwd.x * v.fwd.x + o.fwd.z * v.fwd.z < -0.7f) continue;
                        float lat = Mathf.Abs(rel.x * v.fwd.z - rel.z * v.fwd.x);
                        float ow = o.wreck ? Mathf.Max(o.halfW, o.halfL) * 0.8f : o.halfW;   // xác xe nằm xoay ngang
                        if (lat > v.halfW + ow + 0.35f) continue;
                        float g = al - v.halfL - (o.wreck ? ow : o.halfL);
                        if (g < gap) { gap = g; blockerSpeed = o.speed; blockerBike = o.bike; }
                    }
                if (player)
                {
                    var rel = pp - v.pos; float al = rel.x * v.fwd.x + rel.z * v.fwd.z;
                    float lat = Mathf.Abs(rel.x * v.fwd.z - rel.z * v.fwd.x);
                    float g = al - v.halfL - 2.3f;
                    if (al > 0 && al < 40f && lat < v.halfW + 1.3f && g < gap) { gap = g; blockerSpeed = playerDriver ? Mathf.Abs(playerDriver.Speed) : 0f; blockerPlayer = true; blockerBike = false; }
                }
                // 21.254: xe khác do script khác lái (xe CSGT) — phanh như với xe người chơi.
                foreach (var b in blockers)
                {
                    if (!b || !b.gameObject.activeInHierarchy) continue;
                    var rel = b.position - v.pos; float al = rel.x * v.fwd.x + rel.z * v.fwd.z;
                    float lat = Mathf.Abs(rel.x * v.fwd.z - rel.z * v.fwd.x);
                    var hs = blockerHalf.TryGetValue(b, out var hw) ? hw : new Vector2(1.2f, 2.0f); float bw = hs.x;   // 21.255: khủng long/xe tăng
                    float g = al - v.halfL - hs.y;
                    if (al > 0 && al < 40f && lat < v.halfW + bw && g < gap) { gap = g; blockerSpeed = b.linearVelocity.magnitude; blockerBike = false; }
                }
                // 2) vạch dừng / đèn
                float vT = v.vmax, stopGap = 999f;
                if (!v.inConn)
                {
                    float toStop = v.lane.len - v.s - v.halfL;
                    if (v.next == null) stopGap = toStop;
                    else if (!v.committed)
                    {
                        // Giao lộ có đèn: xin vào từ khoảng phanh để đi qua khi xanh mà không phải dừng.
                        float window = 4f + (v.lane.end.signal ? v.speed * v.speed / 10f + 2f : 0f);
                        if (toStop < window && !hit) TryEnter(v, ghost || wild);            // xe ẩu: vượt đèn đỏ, không xin giao lộ
                        if (!v.committed) stopGap = toStop;
                    }
                    if (v.next != null && v.next.turn) vT = Mathf.Min(vT, (wild ? 11f : 6.5f) + 0.35f * Mathf.Max(0, v.lane.len - v.s));
                }
                else if (v.conn.turn) vT = Mathf.Min(vT, wild ? 11f : 6.5f);
                float minGap = v.bike ? 1.2f : 2.0f;
                float vGap = Mathf.Sqrt(2f * 5f * Mathf.Max(0f, gap - minGap));
                float vStop = Mathf.Sqrt(2f * 5f * Mathf.Max(0f, stopGap - 0.4f));
                vT = Mathf.Min(vT, Mathf.Min(vGap, vStop));
                if (hit) vT = 0f;
                v.speed = Mathf.MoveTowards(v.speed, vT, (vT < v.speed ? 10f : 3.2f) * dt);
                if (gap < minGap * 0.5f) v.speed = 0f;
                SetBrake(v, v.speed < 0.3f || v.speed < v.prevSpeed - 0.03f);
                v.prevSpeed = v.speed;
                // Còi: bị xe người chơi chắn đường > 1,2 s.
                if (blockerPlayer && gap < 8f && v.speed < 1f) { v.honkBlocked += dt; if (v.honkBlocked > 1.2f) Honk(v); } else v.honkBlocked = 0f;
                // Bị xe đứng yên chắn: ô tô chuyển làn kề (đường 2 làn); xe máy lách sát vạch làn (kể cả hàng chờ đèn đỏ).
                bool blockedStill = gap < 8f && blockerSpeed < 0.6f && (v.bike || stopGap > gap + 6f);
                v.blockedTime = blockedStill ? v.blockedTime + dt : 0f;
                if (v.bike)
                {
                    if (v.blockedTime > 0.6f && !blockerBike && !v.inConn) v.filterUntil = now + 3f;
                }
                else if (v.blockedTime > 1.5f && !v.inConn && !v.committed && v.lane.sibling != null && Mathf.Abs(v.shift) < 0.1f && LaneClear(v.lane.sibling, v.s, v, pp))
                {
                    var from = v.lane; v.lane = from.sibling;
                    v.shift = Vector3.Dot(v.pos - (v.lane.a + v.lane.dir * v.s), v.lane.right);
                    v.next = PickNext(v.lane, v); v.blockedTime = 0f;
                }
                // Kẹt lâu ở giao lộ (vòng chờ khép kín): xe "xuyên" tạm 4 s.
                // (chờ đèn đỏ là bình thường: chỉ coi là kẹt sau 45 s)
                if (v.speed < 0.2f && !blockerPlayer && !hit && (v.waiting || v.committed) && !v.exitBlocked)
                {
                    v.stopTime += dt;
                    if (v.stopTime > (v.waiting && v.lane.end.signal ? 45f : 12f))
                    {
                        v.ghostUntil = now + 4f; v.stopTime = 0; GhostEvents++;
                        var nd = v.inConn ? v.conn.node : v.lane.end;
                        if (GhostLog.Count < 40) GhostLog.Add($"t={now:F0} {(v.bike ? "B" : "C")} {v.pos:F0} conn={v.inConn} com={v.committed} wait={v.waiting} kind={(v.inConn ? v.conn.kind : v.next != null ? v.next.kind : -1)} app={v.lane.approach} node={nd.p:F0} sig={nd.signal} ax0={SignalState(nd, 0, now)} inApp={string.Join(",", nd.inApp)} left={string.Join(",", nd.leftIn)} gap={gap:F1} bs={blockerSpeed:F1}");
                    }
                }
                else v.stopTime = 0;

                // 3) tiến theo làn / đường cong giao lộ
                v.s += v.speed * dt;
                if (!v.inConn && v.s >= v.lane.len)
                {
                    if (v.next == null) { Despawn(v); continue; }
                    if (!v.committed && !TryEnter(v, ghost || wild)) { v.s = v.lane.len - 0.01f; v.speed = 0f; }
                    if (v.committed) { v.s -= v.lane.len; v.inConn = true; v.conn = v.next; }
                }
                if (v.inConn && v.s >= v.conn.len)
                {
                    if (v.claimed) Release(v);
                    v.committed = false; v.inConn = false;
                    v.s -= v.conn.len; v.lane = v.conn.to; v.conn = null; v.next = PickNext(v.lane, v);
                }
                // Lệch ngang: xe máy lách / về vị trí riêng trong làn; vào giao lộ thì về tâm làn.
                if (v.bike)
                {
                    float target = v.committed || v.inConn ? 0f
                        : now < v.filterUntil ? (v.lane.nk == 2 && v.lane.k == 0 ? 1.95f : -1.95f) : v.baseLateral;
                    v.lateral = Mathf.MoveTowards(v.lateral, target, Mathf.Max(1.2f, v.speed * 0.3f) * dt);
                }
                Vector3 p, tan;
                if (v.inConn) Sample(v.conn, v.s, out p, out tan);
                else { p = v.lane.a + v.lane.dir * v.s; tan = v.lane.dir; }
                var tn = new Vector3(tan.x, 0, tan.z).normalized; var rightNow = new Vector3(tn.z, 0, -tn.x);
                float sh0 = v.shift;
                v.shift = Mathf.MoveTowards(v.shift, 0f, 1.6f * dt);
                p += rightNow * (v.lateral + v.shift);
                if (Mathf.Abs(sh0) > 0.01f && v.speed > 0.1f) tan = tn * Mathf.Max(v.speed, 2f) - rightNow * Mathf.Sign(sh0) * 1.6f;
                var prevFwd = v.fwd;
                v.pos = p;
                if (tan.sqrMagnitude > 1e-6f)
                {
                    var target = Quaternion.LookRotation(new Vector3(tan.x, 0, tan.z));
                    v.rot = Quaternion.Slerp(v.rot, target, Mathf.Clamp01(10f * dt));
                }
                v.fwd = v.rot * Vector3.forward;
                if (v.bike)
                {
                    float yawRate = Vector3.SignedAngle(prevFwd, v.fwd, Vector3.up) / Mathf.Max(dt, 1e-4f);
                    v.lean = Mathf.Lerp(v.lean, Mathf.Clamp(-yawRate * v.speed * 0.025f, -20f, 20f), 6f * dt);
                }
                v.rb.MovePosition(v.pos); v.rb.MoveRotation(v.rot);
                if (wild && v.speed > MinClosing) SweepHits(v, FlatDir(v.fwd) * v.speed);
            }
            ActiveCount = act;
        }

        bool ExitRoom(Lane L, Vehicle self)
        {
            float need = 2f * self.halfL + 3f;
            foreach (var o in pool)
            {
                if (!o.active || o == self) continue;
                if (!o.inConn && o.lane == L && o.s - o.halfL < need) return false;
                if (o.inConn && o.conn.to == L && o.speed < 2f) return false;
            }
            if (player)
            {
                var rel = cachedPlayer - L.a; float al = Vector3.Dot(rel, L.dir);
                if (al > -3f && al < need + 2f && Mathf.Abs(Vector3.Dot(rel, L.right)) < 2.6f) return false;
            }
            return true;
        }

        bool LaneClear(Lane L, float s, Vehicle self, Vector3 pp)
        {
            foreach (var o in pool)
                if (o.active && o != self && !o.inConn && o.lane == L && Mathf.Abs(o.s - s) < 12f) return false;
            if (player)
            {
                var c = L.a + L.dir * s; var rel = pp - c;
                if (Mathf.Abs(Vector3.Dot(rel, L.right)) < 2.2f && Mathf.Abs(Vector3.Dot(rel, L.dir)) < 12f) return false;
            }
            return true;
        }

        static void Sample(Conn c, float s, out Vector3 p, out Vector3 tan)
        {
            int n = c.pts.Length - 1;
            for (int i = 1; i <= n; i++)
                if (s <= c.cum[i] || i == n)
                {
                    float seg = Mathf.Max(1e-4f, c.cum[i] - c.cum[i - 1]);
                    float t = Mathf.Clamp01((s - c.cum[i - 1]) / seg);
                    p = Vector3.Lerp(c.pts[i - 1], c.pts[i], t);
                    var d0 = c.pts[i] - c.pts[i - 1];
                    var d1 = i < n ? c.pts[i + 1] - c.pts[i] : d0;
                    tan = Vector3.Lerp(d0.normalized, d1.normalized, t);
                    return;
                }
            p = c.pts[n]; tan = c.pts[n] - c.pts[n - 1];
        }

        void LateUpdate()
        {
            float now = Time.time;
            foreach (var v in pool)
            {
                if (!v.active) continue;
                bool hit = now < v.hitUntil;
                if (v.bike) v.visual.localRotation = v.fallStart >= 0f ? FallPose(v, now) : hit ? Quaternion.identity : Quaternion.Euler(0, 0, v.lean);
                if (!v.anim) continue;
                if (hit)
                {
                    if (v.bike && v.recoverAt > 0 && now > v.recoverAt && v.clip != "Recover" && v.anim.GetClip("Recover"))
                    { v.anim.CrossFade("Recover", 0.2f); var rs = v.anim["Recover"]; if (rs != null) rs.wrapMode = WrapMode.ClampForever; v.clip = "Recover"; }
                    continue;
                }
                string want = v.speed > 0.3f ? (v.bike ? "Ride" : "Drive") : "Idle";
                if (want != v.clip) { v.anim.CrossFade(want, 0.25f); var ws = v.anim[want]; if (ws != null) ws.wrapMode = WrapMode.Loop; v.clip = want; }
                var st = v.anim[want]; if (st != null) st.speed = want == "Idle" ? 1f : Mathf.Clamp(v.speed / 3.5f, 0.3f, 5f);
            }
        }

        // Kiểm thử (21.259/21.260): đặt một ô tô / xe máy (lọc theo tên mẫu nếu có) đứng yên trên làn gần nhất cách điểm `at`
        // theo hướng `dir`, giữ yên `hold` giây. Trả về chỉ số xe (-1 nếu không được).
        public int TestPlace(Vector3 at, Vector3 dir, bool bike, float hold = 20f, string model = null)
        {
            var pick = TestPick(at, bike, model); if (pick == null || !PlaceOnLane(pick, at, dir)) return -1;
            pick.hitUntil = Time.time + hold; pick.recoverAt = 0f; pick.hitClip = null;
            return pick.index;
        }
        // Kiểm thử (21.260): đặt một xe trên làn tại `at` và cho thành xe ẩu chạy `speed` m/s.
        public int TestReckless(Vector3 at, Vector3 dir, bool bike, float speed, string model = null)
        {
            var pick = TestPick(at, bike, model); if (pick == null || !PlaceOnLane(pick, at, dir)) return -1;
            pick.hitUntil = 0f; MakeReckless(pick, speed, 30f); pick.speed = speed;
            return pick.index;
        }
        public string VehicleInfo(int i)
        {
            if (i < 0 || i >= pool.Count) return "-";
            var v = pool[i];
            return $"{v.visual.name} m={v.mass:0.##}t active={v.active} wreck={v.wreck} fatal={v.fatal} exploded={v.exploded} landed={v.landed} recovering={v.recovering} reckless={v.reckless} stunLeft={(v.wreck && v.stunUntil < 1e6f ? v.stunUntil - Time.time : -1f):0.0} speed={v.speed:0.0} pos={v.pos:F1}";
        }
        Vehicle TestPick(Vector3 at, bool bike, string model)
        {
            Vehicle pick = null;
            foreach (var v in pool)
                if (v.bike == bike && v.active && !v.wreck && (model == null || v.visual.name.Contains(model))
                    && (pick == null || (v.pos - at).sqrMagnitude > (pick.pos - at).sqrMagnitude)) pick = v;
            if (pick == null && model != null)
                foreach (var v in pool) if (v.bike == bike && !v.active && v.visual.name.Contains(model)) { pick = v; break; }
            return pick;
        }
        bool PlaceOnLane(Vehicle pick, Vector3 at, Vector3 dir)
        {
            Lane best = null; float bd = float.MaxValue, bs = 0;
            foreach (var L in lanes)
            {
                if (Vector3.Dot(L.dir, dir) < 0.9f) continue;
                float sl = Mathf.Clamp(Vector3.Dot(at - L.a, L.dir), 1f, L.len - 1f); var q = L.a + L.dir * sl; float d = (q - at).sqrMagnitude;
                if (d < bd) { bd = d; best = L; bs = sl; }
            }
            if (best == null) return false;
            if (pick.waiting) StopWaiting(pick); if (pick.claimed) Release(pick);
            if (!pick.active) { pick.root.gameObject.SetActive(true); pick.active = true; ClearWreck(pick); }
            pick.lane = best; pick.s = bs; pick.inConn = false; pick.committed = false; pick.next = PickNext(best, pick); pick.speed = 0f; pick.lateral = 0f;
            pick.pos = best.a + best.dir * bs; pick.fwd = best.dir; pick.rot = Quaternion.LookRotation(best.dir);
            pick.root.SetPositionAndRotation(pick.pos, pick.rot); pick.rb.position = pick.pos; pick.rb.rotation = pick.rot;
            return true;
        }

        // Kiểm thử: thông tin nhanh cho eval.
        public string Report()
        {
            int waiting = 0, conn = 0, moving = 0, filtering = 0, braking = 0;
            foreach (var v in pool)
            {
                if (!v.active) continue; if (v.waiting) waiting++; if (v.inConn) conn++; if (v.speed > 1f) moving++;
                if (v.bike && Mathf.Abs(v.lateral) > 1.5f) filtering++; if (v.braking) braking++;
            }
            int busy = 0, sig = 0; foreach (var n in nodes) { if (n.count > 0) busy++; if (n.signal) sig++; }
            return $"lanes={lanes.Count} nodes={nodes.Count} signals={sig} poles={SignalPoles} pool={pool.Count} active={ActiveCount} moving={moving} waiting={waiting} inJunction={conn} busyNodes={busy} spawned={Spawned} ghosts={GhostEvents} redStops={RedStops} braking={braking} filtering={filtering} honks={Honks} hits={Hits} wrecks={Wrecks} explosions={Explosions} recovered={Recovered} npcCrashes={NpcCrashes} reckless={RecklessStarts} policeHits={PoliceHits}";
        }

        // Kiểm thử: khoảng cách nhỏ nhất giữa hai xe NPC cùng chiều (phát hiện chồng xe).
        public float MinSpacing()
        {
            float best = 999f;
            for (int i = 0; i < pool.Count; i++)
            {
                var a = pool[i]; if (!a.active) continue;
                for (int j = i + 1; j < pool.Count; j++)
                {
                    var b = pool[j]; if (!b.active) continue;
                    var rel = b.pos - a.pos; float al = Mathf.Abs(rel.x * a.fwd.x + rel.z * a.fwd.z);
                    float lat = Mathf.Abs(rel.x * a.fwd.z - rel.z * a.fwd.x);
                    if (lat < a.halfW + b.halfW - 0.2f) best = Mathf.Min(best, al - a.halfL - b.halfL);
                }
            }
            return best;
        }

        // Kiểm thử: xe vào giao lộ có đèn khi trục của nó đang đỏ (phải = 0 ngoài các xe "xuyên").
        public int RedRunners()
        {
            int n = 0; float now = Time.time;
            foreach (var v in pool)
                if (v.active && v.inConn && v.claimed && v.claimNode != null && v.claimNode.signal && v.s < 1f
                    && SignalState(v.claimNode, v.claimApp % 2, now) == 0 && SignalState(v.claimNode, v.claimApp % 2, now - 4.5f) == 0) n++;
            return n;
        }
    }
}
