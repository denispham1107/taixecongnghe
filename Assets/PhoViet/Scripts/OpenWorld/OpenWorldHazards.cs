using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // Bản đồ mở — hiểm họa (21.255, đợt 7 phần 3). Người dùng chốt: xuất hiện theo QUÃNG ĐƯỜNG LÁI (mốc đầu 400 m, sau đó
    // mỗi 400–600 m) cho khủng long; từ 21.258 xe tăng mỗi 150 m và trực thăng mỗi 200 m có lịch riêng, mỗi lượt thêm 1 chiếc, trúng → xe hỏng, nằm chờ CSGT tới bắt; thêm nhảy như game cũ.
    // • Khủng long Emerald Tyrant (prefab + DinosaurVisual của game cũ): xuất hiện ~120 m phía trước trên đường người chơi đang
    //   đi, đi ngược chiều 3 m/s giữa lòng đường; khi xe tới gần thì lấy đà quất đuôi thấp ngang đường (vùng nguy hiểm
    //   0,85–1,55 s của clip) — phải NHẢY qua đuôi. Thân/chân là vật cản (đâm vào là hỏng xe). Đuôi/thân hất xe NPC,
    //   làm xe CSGT khựng 1,2 s.
    // • Xe tăng Iron Sentinel (PV_MeshyTank): xuất hiện ~130 m phía trước ở làn ngược chiều, chạy 5 m/s theo mạng đường
    //   (tới giao lộ thì rẽ về phía người chơi), húc văng xe NPC chắn đường. Khi người chơi ở phía trước 24–115 m thì bắn
    //   đạn pháo bay 1,6 s tới chỗ người chơi sẽ tới (hoặc một làn khác của con đường đó), có vòng cảnh báo đỏ trên mặt
    //   đường; nhịp 1,0 s khi chậm → 0,55 s khi nhanh. Nổ bán kính 2,2 m: xe người chơi chưa nhảy cao quá 2,2 m → hỏng; xe
    //   NPC văng; CSGT khựng. Để lại hố bán kính 2,024 m (lưới TankCraterGeometry của game cũ): bánh lăn qua bị kéo lệch,
    //   lái ngược để chỉnh; nhảy qua thì không sao. Nổ trên sông chỉ bắn nước, không có hố.
    [DefaultExecutionOrder(110)]
    public partial class OpenWorldHazards : MonoBehaviour
    {
        public OpenWorldTraffic traffic;
        public OpenWorldPolice police;
        public OpenWorldDriver player;
        [Header("Mẫu")]
        public GameObject dinosaurPrefab;
        public GameObject tankModel; public Material tankMaterial;
        public GameObject shellModel; public Material shellMaterial;
        public GameObject helicopterPrefab, missilePrefab;
        public Material warningMaterial, explosionMaterial, muzzleMaterial, sparkMaterial, craterMaterial;
        [Header("Lịch")]
        [Tooltip("Khủng long: mốc đầu + khoảng cách ngẫu nhiên")] public float firstAt = 400f, spacingMin = 400f, spacingMax = 600f;
        [Tooltip("21.258: xe tăng mỗi 150 m lái, lượt sau đông hơn lượt trước 1 chiếc; trực thăng mỗi 200 m, cũng vậy")]
        public float tankEvery = 150f, heliEvery = 200f;
        [Tooltip("21.258: giới hạn đang có mặt cùng lúc (máy chịu tải)")] public int maxTanks = 10, maxHelis = 6;
        [Tooltip("21.258: giới hạn đạn pháo/tên lửa địch đang bay cùng lúc để vòng báo còn đọc được")] public int maxShellsInFlight = 8, maxEnemyMissilesInFlight = 6;
        public int seed = 2551;
        [Header("Khủng long")]
        public float dinoAhead = 120f, dinoWalk = 3f;
        [Header("Xe tăng")]
        public float tankAhead = 130f, tankSpeed = 5f, shellFlight = 1.6f, blastRadius = 2.2f, fireNear = 24f, fireFar = 115f;
        public float craterRadius = 2.024f;
        [Tooltip("Chỉ để kiểm thử tự động: tự nhảy khi đuôi khủng long sắp quét tới")] public bool autoJumpTest;
        [Header("Trực thăng (21.256)")]
        public float heliAhead = 160f, heliAltitude = 15f, heliStation = 55f, heliMaxSpeed = 32f, heliStay = 22f;
        public float missileSpeed = 36f;
        [Tooltip("Chỉ để kiểm thử tự động: tên lửa vừa bắn thì đánh lái trái + đạp ga 0,45 s rồi chạy thẳng lại")] public bool autoDodgeTest;
        float dodgeUntil = -1f;

        public float DistanceDriven { get; private set; }
        public float NextAt { get; private set; }
        public int NextKind { get; private set; }               // 21.258: chỉ còn khủng long theo lịch này (xe tăng/trực thăng lịch riêng)
        public float NextTankAt { get; private set; }
        public float NextHeliAt { get; private set; }
        public int TankWave { get; private set; } = 1;
        public int HeliWave { get; private set; } = 1;
        public int HelisSpawned { get; private set; }
        public int MissilesFired { get; private set; }
        public int MissileBlasts { get; private set; }
        public int ActiveHelis => helis.Count;
        public int DinosSpawned { get; private set; }
        public int TanksSpawned { get; private set; }
        public int Sweeps { get; private set; }
        public int ShellsFired { get; private set; }
        public int Blasts { get; private set; }
        public int Splashes { get; private set; }
        public int CraterJolts { get; private set; }
        public int NpcKnocked { get; private set; }
        public int ActiveDinos => dinos.Count;
        public int ActiveTanks => tanks.Count;
        public readonly List<string> Log = new List<string>();

        System.Random rnd;
        Rigidbody playerRb;
        Vector3 lastPlayerPos; int seenRun;
        string notice; float noticeUntil;

        // ------------------------------------------------------------ đồ thị đường
        readonly List<Vector3> gp = new List<Vector3>();
        readonly List<Vector3Int> ge = new List<Vector3Int>();
        List<int>[] adj; bool graphReady;
        bool EnsureGraph()
        {
            if (graphReady) return true;
            if (!traffic || traffic.NodeCount == 0) return false;
            traffic.RoadGraph(gp, ge);
            adj = new List<int>[gp.Count];
            for (int i = 0; i < gp.Count; i++) adj[i] = new List<int>();
            for (int e = 0; e < ge.Count; e++) { adj[ge[e].x].Add(e); adj[ge[e].y].Add(e); }
            graphReady = gp.Count > 0; return graphReady;
        }
        static float Off(int main) => main == 1 ? 3.5f : 1.75f;
        static float Half(int main) => main == 1 ? 7f : 3.5f;

        void Awake()
        {
            rnd = new System.Random(seed);
            BuildFx();
        }
        void Start()
        {
            if (player) { playerRb = player.GetComponent<Rigidbody>(); lastPlayerPos = player.transform.position; }
            if (police) seenRun = police.Runs;
            HookTraffic();
            NextAt = firstAt; NextKind = 0; NextTankAt = tankEvery; NextHeliAt = heliEvery; TankWave = HeliWave = 1;
            ResetWeapons();
        }

        void Note(string m) { Log.Add($"{DistanceDriven:F0}m {m}"); if (Log.Count > 60) Log.RemoveAt(0); }
        void Say(string text, float seconds) { notice = text; noticeUntil = Time.time + seconds; }

        // ------------------------------------------------------------ chơi lại
        public void ClearAll()
        {
            DespawnMovers();
            foreach (var s in shells) { s.body.gameObject.SetActive(false); s.marker.gameObject.SetActive(false); shellPool.Push(s.body); markerPool.Push(s.marker); }
            shells.Clear();
            foreach (var c in craters) Destroy(c.go);
            craters.Clear();
            DistanceDriven = 0; NextAt = firstAt; NextKind = 0; notice = null;
            NextTankAt = tankEvery; NextHeliAt = heliEvery; TankWave = HeliWave = 1;
            ResetWeapons();
            if (player) lastPlayerPos = player.transform.position;
        }
        // Dọn khủng long, xe tăng, đạn đang bay nhưng giữ hố đạn (kiểm thử).
        public void DespawnMovers()
        {
            foreach (var d in dinos) if (d.root) Kill(d.root.gameObject, d.rb);
            foreach (var t in tanks) if (t.root) Kill(t.root.gameObject, t.rb);
            dinos.Clear(); tanks.Clear();
            foreach (var h in helis) if (h.root) Destroy(h.root.gameObject);
            helis.Clear();
            ClearWeapons();
            foreach (var m in missiles) RetireMissile(m);
            missiles.Clear();
        }
        void Kill(GameObject go, Rigidbody rb)
        {
            if (traffic && rb) { traffic.blockers.Remove(rb); traffic.blockerHalf.Remove(rb); }
            Destroy(go);
        }

        // ------------------------------------------------------------ vị trí phía trước trên đường
        // Điểm tim đường cách người chơi ~dist m, nằm phía trước (theo hướng xe), ưu tiên cùng con đường.
        bool AheadOnRoad(float dist, out Vector3 center, out Vector3 towardPlayer, out int main)
        {
            center = Vector3.zero; towardPlayer = Vector3.forward; main = 0;
            if (!EnsureGraph() || !player) return false;
            var pp = player.transform.position; pp.y = 0;
            var pf = playerRb && playerRb.linearVelocity.sqrMagnitude > 4f ? playerRb.linearVelocity : player.transform.forward;
            pf.y = 0; pf.Normalize();
            float best = float.MaxValue;
            for (int e = 0; e < ge.Count; e++)
            {
                Vector3 a = gp[ge[e].x], b = gp[ge[e].y]; float len = Vector3.Distance(a, b);
                if (len < 1f) continue;
                var d = (b - a) / len;
                for (float t = 6f; t <= len - 6f; t += 4f)
                {
                    var c = a + d * t; var rel = c - pp; rel.y = 0; float r = rel.magnitude;
                    if (r < dist - 30f || r > dist + 40f) continue;
                    float dot = Vector3.Dot(pf, rel / r);
                    if (dot < 0.7f) continue;
                    float score = Mathf.Abs(r - dist) + (1f - dot) * 80f + (1f - Mathf.Abs(Vector3.Dot(d, pf))) * 25f;
                    if (score < best)
                    {
                        best = score; center = c; main = ge[e].z;
                        towardPlayer = Vector3.Dot(d, pp - c) >= 0 ? d : -d;
                    }
                }
            }
            return best < float.MaxValue;
        }

        // ------------------------------------------------------------ khủng long
        class Dino
        {
            public Transform root; public DinosaurVisual vis; public Rigidbody rb;
            public float nextSweep, age; public bool hitPlayer, hitPolice; public int sweepId = -1;
            public readonly Vector3[] prev = new Vector3[6];
        }
        readonly List<Dino> dinos = new List<Dino>();

        public bool SpawnDinosaurAhead(float dist)
        {
            if (!dinosaurPrefab || dinos.Count >= 3) return false;
            if (!AheadOnRoad(dist, out var c, out var dir, out _)) return false;
            var go = Instantiate(dinosaurPrefab, c, Quaternion.LookRotation(dir));
            go.name = "KhungLong";
            var d = new Dino { root = go.transform, vis = go.GetComponent<DinosaurVisual>() };
            if (!d.vis) { Destroy(go); return false; }
            d.vis.ResetPose();
            d.rb = go.AddComponent<Rigidbody>(); d.rb.isKinematic = true; d.rb.interpolation = RigidbodyInterpolation.Interpolate;
            var body = new GameObject("ThanKhungLong"); body.transform.SetParent(go.transform, false);
            var bc = body.AddComponent<BoxCollider>(); bc.center = new Vector3(0.5f, 2.4f, 0.5f); bc.size = new Vector3(2.2f, 3.9f, 4.4f);
            body.AddComponent<OpenWorldHazardBody>().reason = "KHỦNG LONG HÚC";
            if (traffic) { traffic.blockers.Add(d.rb); traffic.blockerHalf[d.rb] = new Vector2(2.4f, 2.6f); }
            for (int i = 0; i < 6; i++) d.prev[i] = d.vis.tail[i].position;
            dinos.Add(d); DinosSpawned++;
            Say("KHỦNG LONG PHÍA TRƯỚC · NHẢY NÉ ĐUÔI (J)", 2.2f); Note("khủng long @" + Fmt(c));
            return true;
        }

        void TickDino(Dino d, float dt, Vector3 pp, Vector3 pv)
        {
            d.age += dt;
            var fwd = d.root.forward; fwd.y = 0; fwd.Normalize();
            d.rb.MovePosition(d.rb.position + fwd * dinoWalk * dt);
            d.nextSweep -= dt;
            var toward = pp - d.root.position; toward.y = 0; float dist = toward.magnitude;
            // Lấy đà quất đuôi để đuôi quét qua đúng lúc xe tới (công thức game cũ: tốc độ khép + tầm với đuôi).
            float closing = Mathf.Max(1f, Vector3.Dot(pv, -toward / Mathf.Max(0.01f, dist)) + dinoWalk);
            if (!d.vis.Sweeping && d.nextSweep <= 0 && Vector3.Dot(toward, fwd) > 0 && dist < closing * 1.19f + 5.3f && !player.Crashed)
            {
                d.vis.StartSweep(); d.nextSweep = 3.4f; d.hitPlayer = d.hitPolice = false; d.sweepId++; Sweeps++;
                Say("QUẤT ĐUÔI · NHẢY LÊN!", 1.1f);
            }
            if (autoJumpTest && d.vis.Sweeping && d.vis.SweepAge >= 0.68f && d.vis.SweepAge < 0.68f + dt * 1.5f && dist < 40f) player.RequestJump();
            d.vis.Advance(dt);
            // Va chạm: đuôi (quét giữa vị trí cũ và mới khi đang trong pha nguy hiểm) + thân/chân.
            if (!d.hitPlayer && !player.Crashed && Touches(d, player.transform, 0.85f, 1.9f, 1.25f, out var at, out bool tail, player.Lift > 2.2f))
            { d.hitPlayer = true; Note((tail ? "đuôi" : "thân") + " trúng người chơi: " + lastPart + " tại " + at.ToString("F1") + " xe " + player.transform.position.ToString("F1") + " lift " + player.Lift.ToString("F1")); player.Crash(at, tail ? "KHỦNG LONG QUẤT ĐUÔI" : "KHỦNG LONG HÚC", tail ? 11f : 8f); }
            if (police && !d.hitPolice && Touches(d, police.transform, police.halfWidth, police.halfLength, 1.36f, out _, out bool ptail))
            { d.hitPolice = true; police.Stun(1.2f, ptail ? "đuôi khủng long" : "thân khủng long"); }
            if (traffic)
            {
                if (d.vis.TailDangerous) for (int i = 1; i < 6; i += 2) NpcKnocked += traffic.KnockAround(d.vis.tail[i].position, 1.0f);
                var feet = d.root.position + d.root.rotation * new Vector3(0.5f, 0, 0.6f);
                NpcKnocked += traffic.KnockAround(feet, 2.2f);
            }
            for (int i = 0; i < 6; i++) d.prev[i] = d.vis.tail[i].position;
        }

        string lastPart;
        bool Touches(Dino d, Transform box, float hw, float hl, float h, out Vector3 at, out bool tail, bool skipBody = false)
        {
            at = Vector3.zero; tail = false;
            var origin = box.position; var yaw = Quaternion.Euler(0, box.eulerAngles.y, 0); var inv = Quaternion.Inverse(yaw);
            if ((origin - d.root.position).sqrMagnitude > 20f * 20f) return false;
            if (d.vis.TailDangerous)
                for (int i = 0; i < 5; i++)
                {
                    Vector3 a = d.vis.tail[i].position, b = d.vis.tail[i + 1].position;
                    float r = Mathf.Lerp(.72f, .27f, i / 4f);
                    int sweep = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max((a - d.prev[i]).magnitude, (b - d.prev[i + 1]).magnitude) / .18f));
                    for (int k = 0; k <= sweep; k++)
                    {
                        float u = (float)k / sweep;
                        if (SegHit(Vector3.Lerp(d.prev[i], a, u), Vector3.Lerp(d.prev[i + 1], b, u), r, origin, inv, hw, hl, h))
                        { at = (a + b) * 0.5f; tail = true; lastPart = "đuôi " + i; return true; }
                    }
                }
            // Thân = hông → nửa cổ (đầu/cổ vung ngang cao 3–4,6 m khi quất đuôi, xe đang nhảy đi bên dưới không tính).
            if (skipBody) return false;
            var neck = Vector3.Lerp(d.vis.pelvis.position, d.vis.head.position, 0.45f);
            if (SegHit(d.vis.pelvis.position, neck, .85f, origin, inv, hw, hl, h)) { at = d.vis.pelvis.position; lastPart = "mình"; return true; }
            foreach (var f in new[] { d.vis.leftFoot, d.vis.rightFoot })
                if (SegHit(f.position, f.position + Vector3.up * 1.6f, .55f, origin, inv, hw, hl, h)) { at = f.position; lastPart = "chân " + f.name; return true; }
            return false;
        }

        // Đoạn thẳng có bán kính so với hộp xe (đáy tại origin, xoay yaw) — như DinosaurSegmentHit của game cũ.
        static bool SegHit(Vector3 a, Vector3 b, float r, Vector3 origin, Quaternion inv, float hw, float hl, float h)
        {
            a = inv * (a - origin); b = inv * (b - origin);
            int steps = Mathf.Max(1, Mathf.CeilToInt((b - a).magnitude / .22f));
            for (int i = 0; i <= steps; i++)
            {
                var v = Vector3.Lerp(a, b, (float)i / steps);
                if (v.y + r < 0 || v.y - r > h) continue;
                float dx = Mathf.Max(0, Mathf.Abs(v.x) - hw), dz = Mathf.Max(0, Mathf.Abs(v.z) - hl);
                if (dx * dx + dz * dz <= r * r) return true;
            }
            return false;
        }

        // ------------------------------------------------------------ xe tăng
        class Tank
        {
            public Transform root; public Rigidbody rb; public Animation anim;
            public Vector3 laneA, dir; public int toNode, edge, main; public float fire, age, off;
            public bool destroyed; public float destroyedAt; public ParticleSystem fire1;      // 21.258: bị tên lửa người chơi hạ
        }
        int ActiveTankCount() { int n = 0; foreach (var t in tanks) if (t.root && !t.destroyed) n++; return n; }
        readonly List<Tank> tanks = new List<Tank>();
        static readonly Vector3 MuzzleLocal = new Vector3(0, 1.42f, 3.62f);

        public bool SpawnTankAhead(float dist) => SpawnTankAhead(dist, 0, true);
        public bool SpawnTankAhead(float dist, int slot, bool announce)
        {
            if (!tankModel || ActiveTankCount() >= maxTanks) return false;
            if (!AheadOnRoad(dist, out var c, out var dir, out int main)) return false;
            // cạnh chứa điểm c
            int edge = -1; float bd = float.MaxValue;
            for (int e = 0; e < ge.Count; e++) { float d2 = DistSeg(c, gp[ge[e].x], gp[ge[e].y]); if (d2 < bd) { bd = d2; edge = e; } }
            if (edge < 0) return false;
            var right = new Vector3(dir.z, 0, -dir.x);
            float off = main == 1 && slot % 2 == 1 ? 1.75f : Off(main);           // nhiều xe tăng: xen kẽ làn ngoài/làn trong
            var pos = c + right * off;
            foreach (var o in tanks) if (o.root && Flat(o.root.position - pos) < 9f) return false;   // không chồng lên xe tăng khác
            var go = new GameObject("XeTang");
            go.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(dir));
            var vis = Instantiate(tankModel, go.transform, false); vis.name = tankModel.name;
            Renderer l0 = null, l1 = null, l2 = null;
            foreach (var r in vis.GetComponentsInChildren<Renderer>(true))
            {
                if (tankMaterial) r.sharedMaterial = tankMaterial;
                if (r.name.EndsWith("_LOD1")) l1 = r; else if (r.name.EndsWith("_LOD2")) l2 = r; else l0 = r;
            }
            if (l0 && l1)
            {
                var lg = go.AddComponent<LODGroup>();
                lg.SetLODs(new[] { new LOD(0.2f, new[] { l0 }), new LOD(0.05f, new[] { l1 }), new LOD(0.01f, l2 ? new[] { l2 } : new[] { l1 }) });
                lg.RecalculateBounds();
            }
            var t = new Tank { root = go.transform, anim = vis.GetComponent<Animation>(), dir = dir, main = main, edge = edge, fire = 1.2f };
            int ea = ge[edge].x, eb = ge[edge].y;
            t.toNode = Vector3.Dot(gp[eb] - gp[ea], dir) > 0 ? eb : ea;
            t.laneA = c + right * off; t.off = off;
            t.rb = go.AddComponent<Rigidbody>(); t.rb.isKinematic = true; t.rb.interpolation = RigidbodyInterpolation.Interpolate;
            var bc = go.AddComponent<BoxCollider>(); bc.center = new Vector3(0, 1.05f, -0.94f); bc.size = new Vector3(2.5f, 2.1f, 5.1f);
            go.AddComponent<OpenWorldHazardBody>().reason = "ĐÂM XE TĂNG";
            if (t.anim && t.anim.GetClip("Drive")) { t.anim.playAutomatically = false; t.anim["Drive"].wrapMode = WrapMode.Loop; t.anim.Play("Drive"); }
            if (traffic) { traffic.blockers.Add(t.rb); traffic.blockerHalf[t.rb] = new Vector2(1.3f, 3.5f); }
            tanks.Add(t); TanksSpawned++;
            if (announce) Say("XE TĂNG PHÍA TRƯỚC · NÉ ĐẠN PHÁO", 2.2f);
            Note("xe tăng @" + Fmt(pos));
            return true;
        }

        static float DistSeg(Vector3 p, Vector3 a, Vector3 b)
        {
            var ab = b - a; ab.y = 0; var ap = p - a; ap.y = 0;
            float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
            var q = a + ab * t; q.y = 0; p.y = 0; return (p - q).sqrMagnitude;
        }

        void TickTank(Tank t, float dt, Vector3 pp, Vector3 pv)
        {
            t.age += dt;
            if (t.destroyed) return;                                              // xác cháy: đứng yên, vẫn là vật cản
            var pos = t.rb.position; pos.y = 0;
            // tới giao lộ → chọn đoạn kế tiếp hướng về người chơi (không quay đầu trừ đường cụt)
            float rem = Vector3.Dot(gp[t.toNode] - pos, t.dir);
            if (rem < 5f)
            {
                int bestE = -1; float bestS = float.MinValue; var toP = pp - gp[t.toNode]; toP.y = 0; toP.Normalize();
                foreach (int e in adj[t.toNode])
                {
                    if (e == t.edge) continue;
                    int w = ge[e].x == t.toNode ? ge[e].y : ge[e].x;
                    var nd = (gp[w] - gp[t.toNode]).normalized;
                    float sc = Vector3.Dot(nd, toP) + Vector3.Dot(nd, t.dir) * 0.3f + (float)rnd.NextDouble() * 0.2f;
                    if (sc > bestS) { bestS = sc; bestE = e; }
                }
                if (bestE < 0) bestE = t.edge;                                   // đường cụt: quay đầu
                int from = t.toNode; int to = ge[bestE].x == from ? ge[bestE].y : ge[bestE].x;
                t.edge = bestE; t.toNode = to; t.main = ge[bestE].z;
                t.dir = (gp[to] - gp[from]).normalized;
                t.off = t.main == 1 ? t.off : Off(t.main);
                t.laneA = gp[from] + new Vector3(t.dir.z, 0, -t.dir.x) * t.off;
            }
            // bám làn: điểm dẫn 9 m phía trước trên làn phải
            float s = Vector3.Dot(pos - t.laneA, t.dir);
            var carrot = t.laneA + t.dir * (s + 9f); carrot.y = 0;
            var want = carrot - pos; want.y = 0;
            var fwd = t.root.forward; fwd.y = 0; fwd.Normalize();
            float ang = Vector3.SignedAngle(fwd, want, Vector3.up);
            var rot = t.rb.rotation * Quaternion.Euler(0, Mathf.Clamp(ang, -55f * dt, 55f * dt), 0);
            t.rb.MoveRotation(rot);
            t.rb.MovePosition(t.rb.position + (rot * Vector3.forward) * tankSpeed * dt);
            // húc văng xe NPC phía trước, khựng xe CSGT chắn đầu
            if (traffic) NpcKnocked += traffic.KnockAround(t.rb.position + fwd * 2.2f, 1.6f);
            if (police && (police.transform.position - (t.rb.position + fwd * 2.4f)).sqrMagnitude < 2.6f * 2.6f) police.Stun(1.2f, "xe tăng");
            // bắn
            t.fire -= dt;
            if (t.fire <= 0f)
            {
                var muzzle = t.root.TransformPoint(MuzzleLocal);
                var to = pp - muzzle; to.y = 0; float dist = to.magnitude;
                bool inArc = dist > fireNear && dist < fireFar && Vector3.Dot(to / dist, fwd) > 0.45f;
                if (inArc && !player.Crashed && !player.frozen && shells.Count < maxShellsInFlight)
                {
                    Fire(t, muzzle, pp, pv);
                    float sp = Mathf.Abs(player.Speed);
                    t.fire = Mathf.Lerp(1.0f, 0.55f, Mathf.InverseLerp(8f, 27f, sp));
                }
                else t.fire = 0.25f;
            }
        }

        // ------------------------------------------------------------ đạn pháo
        class Shell { public Vector3 start, target; public float age, apex; public Transform body; public Renderer marker; public bool water; }
        readonly List<Shell> shells = new List<Shell>();
        readonly Stack<Transform> shellPool = new Stack<Transform>();
        readonly Stack<Renderer> markerPool = new Stack<Renderer>();
        MaterialPropertyBlock mpb;
        static readonly int ProgressId = Shader.PropertyToID("_Progress");

        void Fire(Tank t, Vector3 muzzle, Vector3 pp, Vector3 pv)
        {
            var target = pp + new Vector3(pv.x, 0, pv.z) * shellFlight;
            // 40 %: đúng chỗ xe sẽ tới; còn lại: một làn khác của con đường (ngang hướng xe), không ra ngoài lòng đường.
            if (rnd.NextDouble() > 0.4)
            {
                var hd = pv.sqrMagnitude > 1f ? new Vector3(pv.x, 0, pv.z).normalized : player.transform.forward;
                var side = new Vector3(hd.z, 0, -hd.x);
                float[] lanes = { -5.25f, -1.75f, 1.75f, 5.25f };
                var alt = target + side * lanes[rnd.Next(4)];
                if (OnRoad(alt)) target = alt;
            }
            target.y = GroundY(target, out bool water);
            var s = new Shell { start = muzzle, target = target, water = water, apex = 3.2f + Vector3.Distance(muzzle, target) * .06f };
            s.body = shellPool.Count > 0 ? shellPool.Pop() : NewShell(); s.body.gameObject.SetActive(true); s.body.position = muzzle;
            s.marker = markerPool.Count > 0 ? markerPool.Pop() : NewMarker(); s.marker.gameObject.SetActive(true);
            PlaceMarker(s, 0f);
            shells.Add(s); ShellsFired++;
            var f = t.root.forward;
            Emit(muzzleFx, muzzle, f * 2.5f, 3.4f, .22f, Color.white);
            for (int i = 0; i < 5; i++) Emit(smokeFx, muzzle + f * (.6f + i * .4f), f * Random.Range(1.5f, 3f) + Vector3.up * Random.Range(.4f, 1.2f), Random.Range(1.4f, 2.4f), Random.Range(.9f, 1.5f), new Color(.75f, .72f, .68f, .85f));
            PlayAt(cannonClip, muzzle, 0.55f);
        }

        bool OnRoad(Vector3 p)
        {
            for (int e = 0; e < ge.Count; e++)
                if (DistSeg(p, gp[ge[e].x], gp[ge[e].y]) <= (Half(ge[e].z) - 1f) * (Half(ge[e].z) - 1f)) return true;
            return false;
        }

        float GroundY(Vector3 p, out bool water)
        {
            water = false;
            if (Physics.Raycast(new Vector3(p.x, 8f, p.z), Vector3.down, out var h, 20f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.attachedRigidbody == null) return h.point.y;
                // trúng xe → lấy mặt đường dưới xe
                var hits = Physics.RaycastAll(new Vector3(p.x, 8f, p.z), Vector3.down, 20f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
                float best = -99f; foreach (var x in hits) if (x.collider.attachedRigidbody == null && x.point.y > best) best = x.point.y;
                if (best > -50f) return best;
            }
            water = true; return OpenWorldDriver.WaterY;
        }

        Vector3 ShellPos(Shell s, float t)
        {
            float u = Mathf.Clamp01(t / shellFlight);
            return Vector3.Lerp(s.start, s.target, u) + Vector3.up * (4 * s.apex * u * (1 - u));
        }
        void PlaceMarker(Shell s, float progress)
        {
            var tr = s.marker.transform; tr.position = s.target + Vector3.up * .035f;
            float size = blastRadius * 2 * Mathf.Lerp(1.25f, 1f, progress); tr.localScale = new Vector3(size, 1, size);
            if (mpb == null) mpb = new MaterialPropertyBlock();
            s.marker.GetPropertyBlock(mpb); mpb.SetFloat(ProgressId, progress); s.marker.SetPropertyBlock(mpb);
        }

        void TickShells(float dt)
        {
            for (int i = shells.Count - 1; i >= 0; i--)
            {
                var s = shells[i]; s.age += dt;
                var p = ShellPos(s, s.age); var next = ShellPos(s, s.age + .02f);
                s.body.position = p; if ((next - p).sqrMagnitude > 1e-6f) s.body.rotation = Quaternion.LookRotation(next - p);
                PlaceMarker(s, Mathf.Clamp01(s.age / shellFlight));
                Emit(trailFx, p, Vector3.zero, Random.Range(.7f, 1f), .12f, new Color(1, .78f, .35f, 1));
                Emit(trailFx, p - (next - p).normalized * .4f, Vector3.up * .25f, Random.Range(.5f, .8f), Random.Range(.4f, .7f), new Color(.85f, .83f, .8f, .7f));
                bool direct = player && !player.Crashed && Vector3.Distance(p, player.transform.position + Vector3.up * 0.7f) < 1.2f;
                if (s.age >= shellFlight || direct)
                {
                    shells.RemoveAt(i);
                    Detonate(s, direct ? p : s.target, direct);
                    s.body.gameObject.SetActive(false); s.marker.gameObject.SetActive(false); shellPool.Push(s.body); markerPool.Push(s.marker);
                }
            }
        }

        void Detonate(Shell s, Vector3 at, bool direct)
        {
            if (s.water && !direct)
            {
                Splashes++;
                for (int i = 0; i < 14; i++)
                    Emit(sparkFx, at + Vector3.up * .2f, new Vector3(Random.Range(-2f, 2f), Random.Range(5f, 9f), Random.Range(-2f, 2f)), Random.Range(.3f, .6f), Random.Range(.6f, 1f), new Color(.8f, .9f, 1f, .9f));
                PlayAt(blastClip, at, 0.25f);
                return;
            }
            Blasts++;
            Explode(at, direct, !direct, "TRÚNG ĐẠN XE TĂNG", "đạn pháo");
        }

        // Vụ nổ dùng chung (đạn pháo xe tăng, tên lửa trực thăng): hiệu ứng, âm thanh, hất xe NPC, CSGT khựng, xe người chơi hỏng.
        void Explode(Vector3 at, bool direct, bool crater, string reason, string what)
        {
            if (crater) MakeCrater(at);
            Emit(blastFx, at + Vector3.up * 1.6f, Vector3.zero, 7.5f, .9f, Color.white);
            for (int i = 0; i < 26; i++)
            {
                var dir = new Vector3(Random.Range(-1f, 1f), Random.Range(.4f, 1.3f), Random.Range(-1f, 1f));
                Emit(sparkFx, at + Vector3.up * .4f, dir * Random.Range(4f, 9f), Random.Range(.12f, .28f), Random.Range(.5f, 1f), new Color(1, .62f, .2f, 1));
            }
            for (int i = 0; i < 4; i++)
                Emit(smokeFx, at + new Vector3(Random.Range(-1f, 1f), 1 + i * .5f, Random.Range(-1f, 1f)), Vector3.up * Random.Range(1f, 2.2f), Random.Range(3f, 4.5f), Random.Range(1.6f, 2.4f), new Color(.32f, .3f, .29f, .85f));
            PlayAt(blastClip, at, 0.9f);
            if (traffic) NpcKnocked += traffic.KnockAround(at, blastRadius);
            if (police && Flat(police.transform.position - at) < blastRadius + 1.2f) police.Stun(1.2f, what);
            if (player && !player.Crashed)
            {
                var off = player.transform.position - at;
                if (direct || (Flat(off) < blastRadius + 0.6f && player.Lift < 2.2f)) { player.Crash(at, reason, 10f); Note(what + " trúng người chơi"); }
            }
        }
        static float Flat(Vector3 v) { v.y = 0; return v.magnitude; }

        // ------------------------------------------------------------ trực thăng (21.256)
        // Bay cao ~15 m trên phố, đón đầu xe ~55 m phía trước (mũi quay về xe), bắn tên lửa thẳng 36 m/s vào chỗ xe sẽ tới
        // (đón đầu theo vận tốc lúc bắn) — vòng cảnh báo đỏ ở điểm rơi. Đổi hướng/phanh/tăng tốc là né được; nhảy không giúp
        // nhiều vì tên lửa lao xuống từ trên cao. Ở lại ~22 s rồi bay lên cao rời đi. Không có va chạm vật lý với nhà.
        class Heli
        {
            public Transform root; public HelicopterVisual vis; public Vector3 vel; public float age, fire, yaw; public bool leaving;
            public int slot; public bool downed; public Vector3 spin;                 // 21.258: dàn hàng nhiều chiếc, bị bắn rơi
        }
        int ActiveHeliCount() { int n = 0; foreach (var h in helis) if (h.root && !h.downed && !h.leaving) n++; return n; }
        // Vị trí trực chiến theo ô: lệch ngang (m) và độ cao (m) — dàn hàng phía trước xe.
        static readonly Vector2[] HeliSlots = { new Vector2(0, 15), new Vector2(-11, 17), new Vector2(11, 17), new Vector2(-22, 19), new Vector2(22, 19), new Vector2(0, 24) };
        int FreeHeliSlot()
        {
            for (int k = 0; k < HeliSlots.Length; k++)
            {
                bool used = false; foreach (var h in helis) if (h.root && !h.downed && !h.leaving && h.slot == k) { used = true; break; }
                if (!used) return k;
            }
            return 0;
        }
        Vector3 HeliStation(Heli h, Vector3 pp, Vector3 pf)
        {
            var sl = HeliSlots[Mathf.Clamp(h.slot, 0, HeliSlots.Length - 1)];
            var right = new Vector3(pf.z, 0, -pf.x);
            var st = pp + pf * (heliStation + Mathf.Abs(sl.x) * 0.3f) + right * sl.x; st.y = 0;
            return st + Vector3.up * sl.y;
        }
        readonly List<Heli> helis = new List<Heli>();
        static readonly Vector3 LauncherLocal = new Vector3(0, -.15f, 1.3f);

        public bool SpawnHelicopterAhead(float dist) => SpawnHelicopterAhead(dist, true);
        public bool SpawnHelicopterAhead(float dist, bool announce)
        {
            if (!helicopterPrefab || ActiveHeliCount() >= maxHelis) return false;
            if (!AheadOnRoad(dist, out var c, out var dir, out _)) return false;
            var go = Instantiate(helicopterPrefab, c + Vector3.up * (heliAltitude + 6f), Quaternion.LookRotation(dir));
            go.name = "TrucThang";
            foreach (var col in go.GetComponentsInChildren<Collider>(true)) Destroy(col);
            var h = new Heli { root = go.transform, vis = go.GetComponent<HelicopterVisual>(), fire = 3.0f + (float)rnd.NextDouble() * 1.5f, yaw = go.transform.eulerAngles.y, vel = dir * 20f, slot = FreeHeliSlot() };
            if (h.vis) { h.vis.standalone = true; h.vis.ResetPose(); }
            helis.Add(h); HelisSpawned++;
            if (announce) Say("TRỰC THĂNG PHÍA TRƯỚC · NÉ TÊN LỬA", 2.2f);
            Note("trực thăng @" + Fmt(c));
            return true;
        }

        // true = đã đi xa, dọn
        bool TickHeli(Heli h, float dt, Vector3 pp, Vector3 pv)
        {
            h.age += dt;
            if (h.downed) return TickDowned(h, dt);
            var pf = pv.sqrMagnitude > 4f ? pv.normalized : player.transform.forward; pf.y = 0; pf.Normalize();
            if (!h.leaving && (h.age > heliStay || (player.Crashed && h.age > 4f) || player.frozen && police && police.Jailed)) { h.leaving = true; Note("trực thăng rời đi"); }
            Vector3 want;
            if (h.leaving) want = h.root.position + (h.root.forward + Vector3.up * 0.6f).normalized * 60f;
            else
            {
                // điểm trực chiến: phía trước xe theo hướng đi, trên tim đường gần nhất nếu được
                want = HeliStation(h, pp, pf);
            }
            var acc = Vector3.ClampMagnitude((want - h.root.position) * 1.1f - h.vel * 0.9f, 22f);
            h.vel = Vector3.ClampMagnitude(h.vel + acc * dt, h.leaving ? 40f : heliMaxSpeed);
            h.root.position += h.vel * dt;
            // mũi quay về xe (rời đi: quay theo hướng bay), nghiêng mũi theo gia tốc
            var look = h.leaving ? new Vector3(h.vel.x, 0, h.vel.z) : new Vector3(pp.x - h.root.position.x, 0, pp.z - h.root.position.z);
            if (look.sqrMagnitude > 0.5f) h.yaw = Mathf.MoveTowardsAngle(h.yaw, Quaternion.LookRotation(look).eulerAngles.y, 90f * dt);
            var localAcc = Quaternion.Inverse(Quaternion.Euler(0, h.yaw, 0)) * acc;
            h.root.rotation = Quaternion.Euler(Mathf.Clamp(localAcc.z * 0.8f, -12f, 12f), h.yaw, Mathf.Clamp(-localAcc.x * 0.8f, -12f, 12f));
            if (h.leaving) return Flat(h.root.position - pp) > 260f || h.age > heliStay + 15f;
            // bắn
            h.fire -= dt;
            var to = pp - h.root.position; float d = to.magnitude;
            if (h.fire <= 0f)
            {
                bool settled = Flat(h.root.position - HeliStation(h, pp, pf)) < 18f;   // đã vào vị trí trực chiến phía trước
                if (settled && !player.Crashed && !player.frozen && d > 40f && d < 115f && Vector3.Dot(to / d, h.root.forward) > 0.6f && missiles.Count < maxEnemyMissilesInFlight)
                {
                    FireMissile(h, pp, pv);
                    h.fire = Mathf.Lerp(1.4f, 0.9f, Mathf.InverseLerp(0f, 1500f, DistanceDriven));
                }
                else h.fire = 0.2f;
            }
            return false;
        }

        class Missile { public Transform root; public MissileFlightVisual vis; public Vector3 vel, target; public float age, eta; public Renderer marker; }
        readonly List<Missile> missiles = new List<Missile>();
        readonly Stack<Missile> missilePool = new Stack<Missile>();

        void FireMissile(Heli h, Vector3 pp, Vector3 pv)
        {
            var origin = h.root.TransformPoint(LauncherLocal);
            // đón đầu: điểm trên mặt đường nơi xe sẽ tới khi tên lửa bay tới (giải gần đúng 2 vòng)
            var target = pp; float t = 0f;
            for (int k = 0; k < 3; k++) { t = Vector3.Distance(origin, target) / missileSpeed; target = pp + pv * t; }
            target.y = GroundY(target, out bool water);
            var m = missilePool.Count > 0 ? missilePool.Pop() : NewMissile();
            if (m == null) return;
            m.target = target; m.eta = Vector3.Distance(origin, target) / missileSpeed; m.age = 0f;
            m.vel = (target - origin).normalized * missileSpeed;
            m.root.gameObject.SetActive(true);
            m.root.SetPositionAndRotation(origin, Quaternion.LookRotation(m.vel));
            if (m.vis) { m.vis.standalone = true; m.vis.Begin(); }
            m.marker = markerPool.Count > 0 ? markerPool.Pop() : NewMarker(); m.marker.gameObject.SetActive(true);
            PlaceRing(m.marker, target, 0f);
            missiles.Add(m); MissilesFired++;
            if (autoDodgeTest) dodgeUntil = Time.time + 0.45f;
            Emit(muzzleFx, origin, m.vel.normalized * 4f, 1.6f, .15f, Color.white);
            PlayAt(launchClip, origin, 0.6f);
            Say("TÊN LỬA · ĐỔI HƯỚNG / PHANH ĐỂ NÉ", 1.0f);
        }

        Missile NewMissile()
        {
            if (!missilePrefab) return null;
            var go = Instantiate(missilePrefab, transform); go.name = "TenLua";
            foreach (var col in go.GetComponentsInChildren<Collider>(true)) Destroy(col);
            return new Missile { root = go.transform, vis = go.GetComponent<MissileFlightVisual>() };
        }

        void RetireMissile(Missile m)
        {
            if (m.vis) m.vis.End();
            if (m.root) m.root.gameObject.SetActive(false);
            if (m.marker) { m.marker.gameObject.SetActive(false); markerPool.Push(m.marker); m.marker = null; }
            missilePool.Push(m);
        }

        void TickMissiles(float dt)
        {
            for (int i = missiles.Count - 1; i >= 0; i--)
            {
                var m = missiles[i];
                var from = m.root.position; m.age += dt;
                var to = from + m.vel * dt; m.root.position = to;
                PlaceRing(m.marker, m.target, Mathf.Clamp01(m.age / Mathf.Max(0.01f, m.eta)));
                // chạm xe người chơi (đoạn quét so với hộp xe nới 0,3 m)
                bool direct = player && !player.Crashed && SegHit(from, to, 0.3f, player.transform.position, Quaternion.Inverse(Quaternion.Euler(0, player.transform.eulerAngles.y, 0)), 0.85f, 1.9f, 1.25f);
                if (direct || m.age >= m.eta)
                {
                    var at = direct ? to : m.target;
                    missiles.RemoveAt(i); RetireMissile(m); MissileBlasts++;
                    if (at.y < OpenWorldDriver.WaterY + 0.1f)
                    {
                        Splashes++;
                        for (int k = 0; k < 14; k++)
                            Emit(sparkFx, at + Vector3.up * .2f, new Vector3(Random.Range(-2f, 2f), Random.Range(5f, 9f), Random.Range(-2f, 2f)), Random.Range(.3f, .6f), Random.Range(.6f, 1f), new Color(.8f, .9f, 1f, .9f));
                        PlayAt(blastClip, at, 0.25f);
                    }
                    else Explode(at, direct, false, "TRÚNG TÊN LỬA TRỰC THĂNG", "tên lửa");
                }
            }
        }

        void PlaceRing(Renderer r, Vector3 at, float progress)
        {
            var tr = r.transform; tr.position = at + Vector3.up * .035f;
            float size = blastRadius * 2 * Mathf.Lerp(1.25f, 1f, progress); tr.localScale = new Vector3(size, 1, size);
            if (mpb == null) mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb); mpb.SetFloat(ProgressId, progress); r.SetPropertyBlock(mpb);
        }

        // ------------------------------------------------------------ hố đạn
        class Crater { public GameObject go; public Vector3 p; public bool inside; }
        readonly List<Crater> craters = new List<Crater>();
        static Mesh[] craterMeshes;
        void MakeCrater(Vector3 at)
        {
            if (!craterMaterial) return;
            if (craterMeshes == null) { craterMeshes = new Mesh[4]; for (int i = 0; i < 4; i++) craterMeshes[i] = TankCraterGeometry.Build(i); }
            var go = new GameObject("HoDan");
            // Mặt đường bản đồ mở không khoét lỗ được như game cũ → lòng hố ép dẹt nổi sát mặt đường (vết cháy + gờ vỡ).
            go.transform.position = at + Vector3.up * 0.065f;
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            go.transform.localScale = new Vector3(craterRadius, 0.15f, craterRadius);
            go.AddComponent<MeshFilter>().sharedMesh = craterMeshes[rnd.Next(4)];
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = craterMaterial; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            craters.Add(new Crater { go = go, p = at });
            if (craters.Count > 40) { Destroy(craters[0].go); craters.RemoveAt(0); }
        }
        void TickCraters()
        {
            if (!player || player.Crashed) return;
            var pp = player.transform.position;
            foreach (var c in craters)
            {
                bool inside = Flat(pp - c.p) < craterRadius + 0.7f && !player.Airborne;
                if (inside && !c.inside && Mathf.Abs(player.Speed) > 3f)
                {
                    // kéo lệch ra phía ngoài tâm hố; lái ngược để chỉnh
                    float side = Vector3.Dot(pp - c.p, player.transform.right);
                    float sign = Mathf.Abs(side) < 0.2f ? (rnd.Next(2) == 0 ? -1f : 1f) : Mathf.Sign(side);
                    player.Jolt(sign * 30f, 1.5f, 0.82f); CraterJolts++;
                    Say(sign > 0 ? "HỐ ĐẠN · LÁI SANG TRÁI ĐỂ CHỈNH" : "HỐ ĐẠN · LÁI SANG PHẢI ĐỂ CHỈNH", 1.4f);
                }
                c.inside = inside;
            }
        }

        // ------------------------------------------------------------ vòng lặp
        void FixedUpdate()
        {
            if (!player) return;
            float dt = Time.fixedDeltaTime;
            if (police && police.Runs != seenRun) { seenRun = police.Runs; ClearAll(); }
            TickPlayerExplosion();
            if (autoDodgeTest) player.scriptedInput = Time.time < dodgeUntil ? new Vector2(-0.8f, 1f) : new Vector2(0.001f, 0.5f);
            var pp = player.transform.position;
            var pv = playerRb ? playerRb.linearVelocity : Vector3.zero; pv.y = 0;
            // quãng đường lái (bỏ qua bước nhảy vị trí khi hồi sinh/dịch chuyển)
            var step = pp - lastPlayerPos; step.y = 0; lastPlayerPos = pp;
            bool live = !player.Crashed && !player.frozen && !player.Sinking && (!police || police.Mode != OpenWorldPolice.State.Arrested);
            if (live && step.magnitude < 3f) DistanceDriven += step.magnitude;
            if (live && DistanceDriven >= NextAt)
            {
                bool ok = SpawnDinosaurAhead(dinoAhead);
                if (ok) { NextAt = DistanceDriven + Mathf.Lerp(spacingMin, spacingMax, (float)rnd.NextDouble()); }
                else NextAt = DistanceDriven + 25f;                     // chưa có chỗ phía trước: thử lại sau 25 m
            }
            // 21.258: lượt xe tăng / trực thăng theo quãng đường, mỗi lượt thêm 1 chiếc.
            if (live && DistanceDriven >= NextTankAt)
            {
                int want = Mathf.Min(TankWave, maxTanks - ActiveTankCount()), made = 0;
                for (int i = 0; i < want; i++) if (SpawnTankAhead(tankAhead + i * 17f + (float)rnd.NextDouble() * 6f, i, false)) made++;
                if (made > 0) Say(made > 1 ? "XE TĂNG ×" + made + " PHÍA TRƯỚC · NÉ ĐẠN PHÁO" : "XE TĂNG PHÍA TRƯỚC · NÉ ĐẠN PHÁO", 2.2f);
                TankWave++; NextTankAt = DistanceDriven + tankEvery;
            }
            if (live && DistanceDriven >= NextHeliAt)
            {
                int want = Mathf.Min(HeliWave, maxHelis - ActiveHeliCount()), made = 0;
                for (int i = 0; i < want; i++) if (SpawnHelicopterAhead(heliAhead + i * 10f, false)) made++;
                if (made > 0) Say(made > 1 ? "TRỰC THĂNG ×" + made + " PHÍA TRƯỚC · NÉ TÊN LỬA" : "TRỰC THĂNG PHÍA TRƯỚC · NÉ TÊN LỬA", 2.2f);
                HeliWave++; NextHeliAt = DistanceDriven + heliEvery;
            }
            TickWeapons(dt, pp, pv);
            for (int i = dinos.Count - 1; i >= 0; i--)
            {
                var d = dinos[i];
                if (!d.root) { dinos.RemoveAt(i); continue; }
                TickDino(d, dt, pp, pv);
                var rel = d.root.position - pp; rel.y = 0;
                bool behind = Vector3.Dot(rel, pv.sqrMagnitude > 1 ? pv.normalized : player.transform.forward) < -70f;
                if (behind || rel.magnitude > 260f || d.age > 120f) { Kill(d.root.gameObject, d.rb); dinos.RemoveAt(i); }
            }
            for (int i = tanks.Count - 1; i >= 0; i--)
            {
                var t = tanks[i];
                if (!t.root) { tanks.RemoveAt(i); continue; }
                TickTank(t, dt, pp, pv);
                var rel = t.root.position - pp; rel.y = 0;
                if (rel.magnitude > 280f || t.age > 150f || (t.destroyed && t.age - t.destroyedAt > 25f)) { Kill(t.root.gameObject, t.rb); tanks.RemoveAt(i); }
            }
            for (int i = helis.Count - 1; i >= 0; i--)
            {
                var h = helis[i];
                if (!h.root) { helis.RemoveAt(i); continue; }
                if (TickHeli(h, dt, pp, pv)) { Destroy(h.root.gameObject); helis.RemoveAt(i); }
            }
            TickMissiles(dt);
            TickShells(dt);
            TickCraters();
        }

        // ------------------------------------------------------------ hiệu ứng + âm thanh (dựng một lần, như PhoVietTankFire)
        ParticleSystem muzzleFx, blastFx, smokeFx, sparkFx, trailFx;
        AudioClip cannonClip, blastClip, launchClip; readonly List<AudioSource> sources = new List<AudioSource>(); int nextSource;
        Mesh markerMesh;
        void BuildFx()
        {
            muzzleFx = Fx("LuaDauNong", muzzleMaterial, 2, 2, 0, false);
            blastFx = Fx("VuNo", explosionMaterial, 4, 2, 0, false);
            smokeFx = Fx("KhoiNo", explosionMaterial, 4, 2, -.05f, true);
            sparkFx = Fx("TiaLua", sparkMaterial, 1, 1, 1.4f, false);
            trailFx = Fx("VetDan", sparkMaterial, 1, 1, -.1f, false);
            markerMesh = new Mesh { name = "OW_VongCanhBao" };
            markerMesh.vertices = new[] { new Vector3(-.5f, 0, -.5f), new Vector3(.5f, 0, -.5f), new Vector3(.5f, 0, .5f), new Vector3(-.5f, 0, .5f) };
            markerMesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up }; markerMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            markerMesh.RecalculateNormals(); markerMesh.RecalculateBounds();
            cannonClip = Boom("Pháo xe tăng", .55f, 38, .11f); blastClip = Boom("Đạn nổ", 1.1f, 28, .25f);
            launchClip = Boom("Phóng tên lửa", .7f, 90, .3f);
            for (int i = 0; i < 6; i++)
            {
                var g = new GameObject("AmThanh" + i); g.transform.SetParent(transform, false);
                var a = PhoVietAudio.Route(g.AddComponent<AudioSource>());
                a.playOnAwake = false; a.spatialBlend = 0.85f; a.minDistance = 18f; a.maxDistance = 300f; a.rolloffMode = AudioRolloffMode.Linear;
                sources.Add(a);
            }
        }
        void PlayAt(AudioClip clip, Vector3 at, float vol)
        {
            if (!clip || sources.Count == 0) return;
            var a = sources[nextSource]; nextSource = (nextSource + 1) % sources.Count;
            a.transform.position = at; a.PlayOneShot(clip, vol);
        }
        ParticleSystem Fx(string name, Material material, int tilesX, int tilesY, float gravity, bool smoke)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.playOnAwake = false; main.loop = false; main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = 600; main.gravityModifier = gravity;
            var emission = ps.emission; emission.enabled = false; var shape = ps.shape; shape.enabled = false;
            if (tilesX * tilesY > 1)
            {
                var sheet = ps.textureSheetAnimation; sheet.enabled = true; sheet.numTilesX = tilesX; sheet.numTilesY = tilesY;
                sheet.frameOverTime = smoke ? new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .75f, 1, .999f)) : new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, .999f));
            }
            var fade = ps.colorOverLifetime; fade.enabled = true; var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, smoke ? .4f : .75f), new GradientAlphaKey(0, 1) }); fade.color = gradient;
            if (smoke) { var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .7f, 1, 1.6f)); }
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = material; r.renderMode = ParticleSystemRenderMode.Billboard;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; r.sortingFudge = smoke ? 2 : 0;
            ps.Play(); return ps;
        }
        static void Emit(ParticleSystem ps, Vector3 pos, Vector3 vel, float size, float life, Color color)
        {
            if (!ps) return;
            ps.Emit(new ParticleSystem.EmitParams { position = pos, velocity = vel, startSize = size, startLifetime = life, startColor = color, rotation = Random.Range(0, 360) }, 1);
        }
        Transform NewShell()
        {
            Transform t;
            if (shellModel)
            {
                t = Instantiate(shellModel, transform).transform;
                // Lưới Blender còn trục Z lên: xoay LOD về hệ Unity, chỉ giữ LOD0 (như game cũ).
                foreach (var r in t.GetComponentsInChildren<MeshRenderer>(true))
                {
                    r.transform.localRotation = Quaternion.Euler(-90, 0, 0); r.gameObject.SetActive(r.name.EndsWith("_LOD0"));
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; if (shellMaterial) r.sharedMaterial = shellMaterial;
                }
                var lod = t.GetComponent<LODGroup>(); if (lod) Destroy(lod);
            }
            else { t = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform; t.SetParent(transform, false); t.localScale = new Vector3(.22f, .45f, .22f); Destroy(t.GetComponent<Collider>()); }
            t.localScale = Vector3.one * 1.8f; t.name = "DanPhao"; return t;
        }
        Renderer NewMarker()
        {
            var go = new GameObject("VongCanhBao"); go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = markerMesh; var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = warningMaterial;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; return r;
        }
        static AudioClip Boom(string name, float seconds, float pitch, float decay)
        {
            int hz = 22050, n = (int)(hz * seconds); var data = new float[n]; var noise = new System.Random(name.Length * 31); float low = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / hz, env = Mathf.Exp(-t / decay); low = Mathf.Lerp(low, (float)noise.NextDouble() * 2 - 1, .08f);
                data[i] = (Mathf.Sin(t * 2 * Mathf.PI * pitch * (1 - t * .6f)) * .55f + low * 1.6f + ((float)noise.NextDouble() * 2 - 1) * .25f * Mathf.Exp(-t / .03f)) * env;
            }
            var clip = AudioClip.Create(name, n, 1, hz, false); clip.SetData(data, 0); return clip;
        }
        static string Fmt(Vector3 p) => $"({p.x:F0},{p.z:F0})";

        // ------------------------------------------------------------ HUD
        GUIStyle st;
        void OnGUI()
        {
            if (st == null) st = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            int fs = Mathf.Max(18, Screen.height / 26); st.fontSize = fs;
            if (police && police.Jailed) return;
            if (notice != null && Time.time < noticeUntil && player && !player.Crashed)
            {
                var r = new Rect(0, Screen.height * 0.26f, Screen.width, fs * 2.4f);
                st.normal.textColor = new Color(0, 0, 0, .75f); GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), notice, st);
                st.normal.textColor = new Color(1f, .78f, .2f); GUI.Label(r, notice, st);
            }
        }

        public string Report() => $"km={DistanceDriven:F0} dino@{NextAt:F0} tank@{NextTankAt:F0}×{TankWave} heli@{NextHeliAt:F0}×{HeliWave} ammo={PlayerMissiles}/{maxMissiles} fired={PlayerMissilesFired} tankKills={TanksDestroyed} heliKills={HelisDowned} pickups={PickupsCollected} helis={helis.Count}/{HelisSpawned} missiles={MissilesFired}/{MissileBlasts} dinos={dinos.Count}/{DinosSpawned} tanks={tanks.Count}/{TanksSpawned} sweeps={Sweeps} shells={ShellsFired} blasts={Blasts} splash={Splashes} craters={craters.Count} jolts={CraterJolts} npcKnocked={NpcKnocked}";
    }
}
