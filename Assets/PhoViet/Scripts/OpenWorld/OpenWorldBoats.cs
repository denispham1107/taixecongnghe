using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.271 — tàu đánh cá chạy trên sông bản đồ mở (dùng lại PV_FishingBoat.fbx + PV_FishingBoat.mat).
    // • Mặt cầu chỉ cao hơn mặt nước ≈ 1 m nên tàu không chui qua gầm: mỗi đoạn sông giữa hai cầu có các vòng chạy kín
    //   (đi bên phải tim sông, quay đầu vòng trái trước cầu, chạy ngược bên kia, quay đầu ở đầu kia). Các vòng cùng đoạn
    //   đồng tâm (lệch 24 m / 46 m) nên không cắt nhau; tàu cùng vòng chạy cùng tốc độ nên không đâm nhau.
    // • Tàu nhấp nhô/chúc/lắc theo đúng hàm sóng của shader (WaveHeight = OpenWorldRiverWaves.cginc).
    // • Vệt sóng: dải băng dựng theo chính đường tàu vừa đi (lấy mẫu đường chạy phía sau mũi tàu), shader BoatWake.
    // • Vài tàu neo sát kè, chỉ nhấp nhô. Điện thoại: ít tàu hơn.
    public sealed class OpenWorldBoats : MonoBehaviour
    {
        public GameObject boatPrefab;
        public Material boatMaterial, wakeMaterial;
        public float waterY = -2.2f, hullOffset = 0.05f, waveAmp = 1f;
        public bool mobileProfile;                     // kiểm thử: ép cấu hình điện thoại
        [Header("Vệt sóng")] public float wakeLength = 36f; public int wakeSegments = 22;

        static readonly int WaveAmpId = Shader.PropertyToID("_OWWaveAmp");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        // ---------------------------------------------------------------- sóng (khớp OpenWorldRiverWaves.cginc)
        public static float WaveHeight(float x, float z, float t, float amp)
        {
            float h = 0;
            h += 0.10f * amp * Mathf.Sin((0.94f * x + 0.34f * z) * 0.21f + t * Mathf.Sqrt(9.8f * 0.21f));
            h += 0.06f * amp * Mathf.Sin((-0.45f * x + 0.89f * z) * 0.37f + t * Mathf.Sqrt(9.8f * 0.37f) + 1.7f);
            h += 0.035f * amp * Mathf.Sin((0.20f * x - 0.98f * z) * 0.63f + t * Mathf.Sqrt(9.8f * 0.63f) + 4.1f);
            return h;
        }

        // ---------------------------------------------------------------- đường chạy
        sealed class Loop
        {
            public readonly List<Vector2> p = new List<Vector2>(); public readonly List<float> d = new List<float>();
            public float Length => d[d.Count - 1];
            public void Add(Vector2 q) { if (p.Count > 0 && (q - p[p.Count - 1]).sqrMagnitude < 0.25f) return; d.Add(p.Count == 0 ? 0 : d[d.Count - 1] + (q - p[p.Count - 1]).magnitude); p.Add(q); }
            public void Close() { Add(p[0]); }
            public Vector2 Sample(float s, out Vector2 dir)
            {
                float L = Length; s %= L; if (s < 0) s += L;
                int lo = 0, hi = d.Count - 1;
                while (hi - lo > 1) { int m = (lo + hi) >> 1; if (d[m] <= s) lo = m; else hi = m; }
                float seg = Mathf.Max(1e-4f, d[hi] - d[lo]);
                dir = (p[hi] - p[lo]) / seg;
                return Vector2.Lerp(p[lo], p[hi], (s - d[lo]) / seg);
            }
        }

        static Vector2 Right(Vector2 f) => new Vector2(f.y, -f.x);

        // Tim sông của một đoạn (điểm dày 2 m) → vòng kín lệch o mét: phải chiều đi, quay đầu vòng trái ở cuối, chạy về bên kia.
        static Loop MakeLoop(List<Vector2> c, float o)
        {
            var L = new Loop(); int n = c.Count;
            Vector2 Dir(int i) => (c[Mathf.Min(i + 1, n - 1)] - c[Mathf.Max(i - 1, 0)]).normalized;
            for (int i = 0; i < n; i++) L.Add(c[i] + Right(Dir(i)) * o);
            Vector2 fe = Dir(n - 1), E = c[n - 1];
            for (int k = 1; k < 24; k++) { float a = Mathf.PI * k / 24; L.Add(E + Right(fe) * o * Mathf.Cos(a) + fe * o * Mathf.Sin(a)); }
            for (int i = n - 1; i >= 0; i--) L.Add(c[i] - Right(Dir(i)) * o);
            Vector2 fs = -Dir(0), S = c[0];
            for (int k = 1; k < 24; k++) { float a = Mathf.PI * k / 24; L.Add(S + Right(fs) * o * Mathf.Cos(a) + fs * o * Mathf.Sin(a)); }
            L.Close(); return L;
        }

        static List<Vector2> Line(Vector2 a, Vector2 b, List<Vector2> into = null)
        {
            into = into ?? new List<Vector2>(); int n = Mathf.Max(2, Mathf.CeilToInt((b - a).magnitude / 2f));
            for (int i = 0; i <= n; i++) { var q = Vector2.Lerp(a, b, i / (float)n); if (into.Count == 0 || (q - into[into.Count - 1]).sqrMagnitude > 0.01f) into.Add(q); }
            return into;
        }

        // Ba đoạn sông giữa các cầu (toạ độ thế giới X, Z; tim nhánh dọc x 317,25, nhánh ngang z 314,25; cầu B1 z 203–227 trên
        // nhánh dọc, cầu B2 x −162…−137 trên nhánh ngang). Đầu mút cách cầu đủ cho vòng quay đầu 46 m + nửa thân tàu.
        public static List<List<Vector2>> Centerlines()
        {
            // 21.302: bán đảo — ranh giới x, z ∈ [−600, 700] (PhoVietOpenWorldPeninsulaBuilder): tàu chạy trong ranh giới, đầu mút
            // cách mép 85 m (đủ vòng quay đầu bán kính 66 m của hàng sát bờ) thay vì −900 ngoài bản đồ.
            var s1 = Line(new Vector2(317.25f, -515f), new Vector2(317.25f, 140f));
            var s2 = new List<Vector2>();
            Vector2 C = new Vector2(234.5f, 231.5f); const float R = 82.75f;
            for (int k = 0; k <= 30; k++) { float a = Mathf.Deg2Rad * Mathf.Lerp(35f, 90f, k / 30f); s2.Add(C + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * R); }
            Line(new Vector2(234.5f, 314.25f), new Vector2(-75f, 314.25f), s2);
            var s3 = Line(new Vector2(-221f, 314.25f), new Vector2(-515f, 314.25f));
            return new List<List<Vector2>> { s1, s2, s3 };
        }

        // ---------------------------------------------------------------- tàu
        sealed class Boat
        {
            public Transform tr; public Loop loop; public float s, speed, phase; public bool moored;
            public Vector2 pos, dir; public Mesh wake; public float pitch, roll;
            public Renderer wakeRenderer; public float baseSpeed;
            // 21.284: bị tên lửa người chơi bắn → cháy (dừng dần), nổ, chìm rồi ẩn; sau respawnAfter giây xuất hiện lại
            public int state; public float stateAt, sink; public GameObject fx;          // 0 chạy, 1 cháy, 2 chìm, 3 ẩn
            public bool sea;                                                             // 21.303: tàu chiến trên biển (bờ biển)
        }
        readonly List<Boat> boats = new List<Boat>();
        public int MovingCount { get; private set; }
        public int MooredCount { get; private set; }
        public Transform BoatTransform(int i) => i >= 0 && i < boats.Count ? boats[i].tr : null;
        public bool IsMoored(int i) => boats[i].moored;
        public float BoatS(int i) => boats[i].s;

        // ---------------------------------------------------------------- 21.284: tàu bắn / bị bắn
        [Header("Bị bắn (21.284)")] public float burnSeconds = 1.2f, sinkSeconds = 5f, respawnAfter = 30f;
        public int Count => boats.Count;
        public bool Alive(int i) => i >= 0 && i < boats.Count && boats[i].state == 0;
        public bool Burning(int i) => i >= 0 && i < boats.Count && boats[i].state == 1;
        public Vector3 BoatCenter(int i) => boats[i].tr.position + Vector3.up * 1.6f;
        public Vector3 BoatForward(int i) { var f = boats[i].dir.sqrMagnitude > 1e-4f ? boats[i].dir.normalized : Vector2.up; return new Vector3(f.x, 0, f.y); }
        public int Sunk { get; private set; }
        /// Bắt đầu cháy (gọi khi trúng tên lửa); fx = hiệu ứng lửa/khói gắn lên tàu (hủy khi chìm hẳn). Trả về false nếu tàu đã cháy/chìm.
        public bool Ignite(int i, GameObject fx)
        {
            if (!Alive(i)) return false;
            var b = boats[i]; b.state = 1; b.stateAt = Time.time; b.fx = fx; return true;
        }
        /// Đang cháy đủ lâu → true một lần để bên gọi phát vụ nổ, tàu chuyển sang chìm.
        public bool ExplodeDue(int i)
        {
            var b = boats[i]; if (b.state != 1 || Time.time - b.stateAt < burnSeconds) return false;
            b.state = 2; b.stateAt = Time.time; Sunk++; return true;
        }
        void TickDamage(Boat b, float dt)
        {
            if (b.state == 1) b.speed = Mathf.MoveTowards(b.speed, 0f, 4f * dt);
            else if (b.state == 2)
            {
                b.speed = 0f; b.sink = Mathf.Clamp01((Time.time - b.stateAt) / sinkSeconds);
                if (b.sink >= 1f) { b.state = 3; b.stateAt = Time.time; b.tr.gameObject.SetActive(false); if (b.fx) Destroy(b.fx); b.fx = null; }
            }
            else if (b.state == 3 && !b.sea && Time.time - b.stateAt > respawnAfter)   // 21.303: tàu biển do OpenWorldHazards gọi lại
            {
                b.state = 0; b.sink = 0f; b.speed = b.baseSpeed; b.tr.gameObject.SetActive(true);
                if (!b.moored) b.s += b.loop.Length * 0.5f;                      // hiện lại ở phía bên kia vòng chạy
            }
            if (b.wakeRenderer) b.wakeRenderer.enabled = b.state == 0 || (b.state == 1 && b.speed > 0.5f);
        }

        void OnEnable() { Shader.SetGlobalFloat(WaveAmpId, waveAmp); }
        void OnValidate() { Shader.SetGlobalFloat(WaveAmpId, waveAmp); }

        void Start()
        {
            Shader.SetGlobalFloat(WaveAmpId, waveAmp);
            if (!boatPrefab) { enabled = false; return; }
            bool mobile = mobileProfile || UnityEngine.Device.Application.isMobilePlatform;
            var rnd = new System.Random(271);
            var lines = Centerlines();
            // (đoạn, lệch, số tàu, tốc độ m/s) — desktop 15 tàu chạy; điện thoại 8
            var plan = new (int seg, float off, int n, float v)[]
            {
                (0, 24f, 3, 4.6f), (0, 46f, 3, 6.2f),
                (1, 24f, 2, 4.0f), (1, 44f, 3, 5.4f),
                (2, 24f, 2, 5.0f), (2, 46f, 2, 6.6f),
            };
            Color[] tints = { Color.white, new Color(1f, .93f, .86f), new Color(.86f, .93f, 1f), new Color(.9f, 1f, .9f), new Color(1f, .9f, .88f), new Color(.95f, .95f, .95f) };
            int ti = 0;
            foreach (var e in plan)
            {
                int n = mobile ? Mathf.Max(1, (e.n + 1) / 2) : e.n;
                var loop = MakeLoop(lines[e.seg], e.off);
                float start = (float)rnd.NextDouble() * loop.Length;
                for (int k = 0; k < n; k++) AddMover(loop, start + loop.Length * k / n, e.v, tints[ti++ % tints.Length], rnd);
            }
            // 21.301 (người dùng): thêm 5 tàu xuất hiện CÙNG LÚC ở vị trí ngẫu nhiên, chạy qua lại SÁT BỜ (vòng lệch 66 m khỏi tim
            // sông — kè ở ~83 m nên cách kè ~17 m, ngoài hàng tàu neo cách kè 9 m). Đầu mút tim sông cắt bớt 24 m để vòng quay đầu
            // bán kính 66 m không chạm cầu. Điện thoại cũng đủ 5 tàu. Tàu này như mọi tàu khác: có thể bắn người chơi ở lề sông,
            // bị tên lửa hạ thì cháy/chìm rồi hiện lại.
            var shore = new (int seg, int n)[] { (0, 2), (1, 2), (2, 1) };
            foreach (var e in shore)
            {
                var loop = MakeLoop(Trim(lines[e.seg], 24f), ShoreOffset);
                for (int k = 0; k < e.n; k++)
                    AddMover(loop, (float)rnd.NextDouble() * loop.Length, 4.2f + (float)rnd.NextDouble() * 1.8f, tints[ti++ % tints.Length], rnd);
                ShoreCount += e.n;
            }
            // tàu neo sát kè (song song bờ, cách kè 9 m): (x, z, hướng mũi độ)
            var moor = new (float x, float z, float yaw)[]
            {
                (243.5f, -300f, 0f), (391f, -40f, 180f), (243.5f, 60f, 0f),
                (-400f, 391f, 90f), (0f, 237.5f, 270f), (-560f, 237.5f, 90f),
            };
            for (int k = 0; k < moor.Length; k++)
            {
                if (mobile && k % 2 == 1) continue;
                var b = Spawn(tints[ti++ % tints.Length]); b.moored = true; b.phase = k * 1.3f;
                b.pos = new Vector2(moor[k].x, moor[k].z);
                float a = moor[k].yaw * Mathf.Deg2Rad; b.dir = new Vector2(Mathf.Sin(a), Mathf.Cos(a));
                boats.Add(b); MooredCount++;
            }
            Tick(0f);
        }

        // ---------------------------------------------------------------- 21.303: tàu trên biển quanh bán đảo
        // Xe người chơi vào lề sát biển → OpenWorldHazards đặt đủ 5 tàu ngoài khơi gần xe. Tàu tuần quanh chỗ được đặt (vòng tròn
        // bán kính seaPatrolRadius) — rời bờ biển tàu vẫn ở đó; bị bắn chìm thì nằm ẩn tới khi hazards đặt lại (không tự hiện lại).
        [Header("Tàu biển (21.303)")] public float seaPatrolRadius = 16f, seaSpeed = 3f;
        public bool IsSea(int i) => i >= 0 && i < boats.Count && boats[i].sea;
        public int SeaAlive { get { int n = 0; foreach (var b in boats) if (b.sea && b.state == 0) n++; return n; } }
        public int SeaCount { get { int n = 0; foreach (var b in boats) if (b.sea) n++; return n; } }
        /// Tàu biển đang chìm hẳn (ẩn) / đang cháy-chìm không tính; true = tàu biển có thể đặt lại.
        public bool SeaFree(int i) => IsSea(i) && boats[i].state == 3;
        static Loop Circle(Vector2 c, float r, float phase)
        {
            var L = new Loop(); for (int k = 0; k < 48; k++) { float a = phase + Mathf.PI * 2f * k / 48f; L.Add(c + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * r); }
            L.Close(); return L;
        }
        /// Đặt một tàu biển tuần quanh center: dùng lại tàu biển đã chìm nếu có, không thì tạo mới. Trả về chỉ số tàu.
        public int PlaceSeaBoat(Vector2 center)
        {
            if (!boatPrefab) return -1;
            int idx = -1;
            for (int i = 0; i < boats.Count; i++) if (SeaFree(i)) { idx = i; break; }
            var loop = Circle(center, seaPatrolRadius, Random.Range(0f, Mathf.PI * 2f));
            Boat b;
            if (idx < 0)
            {
                var rnd = new System.Random(boats.Count * 7919 + 303);
                AddMover(loop, 0f, seaSpeed, new Color(.88f, .9f, .94f), rnd);
                idx = boats.Count - 1; b = boats[idx]; b.sea = true; b.tr.name = "TauBien";
            }
            else
            {
                b = boats[idx]; b.loop = loop; b.s = 0f; b.state = 0; b.sink = 0f; b.speed = b.baseSpeed; b.tr.gameObject.SetActive(true);
                if (b.fx) Destroy(b.fx); b.fx = null;
            }
            b.pos = b.loop.Sample(b.s, out b.dir); Place(b, Time.timeSinceLevelLoad, 0f);
            return idx;
        }
        /// Chơi lại: ẩn mọi tàu biển (hiện lại khi xe vào bờ biển).
        public void ClearSea()
        {
            foreach (var b in boats)
            {
                if (!b.sea) continue;
                b.state = 3; b.stateAt = Time.time; b.sink = 0f; b.tr.gameObject.SetActive(false);
                if (b.fx) Destroy(b.fx); b.fx = null;
                if (b.wakeRenderer) b.wakeRenderer.enabled = false;
            }
        }

        public const float ShoreOffset = 66f;
        public int ShoreCount { get; private set; }

        void AddMover(Loop loop, float s, float speed, Color tint, System.Random rnd)
        {
            var b = Spawn(tint);
            b.loop = loop; b.speed = speed; b.s = s; b.phase = (float)rnd.NextDouble() * 10f;
            b.wake = new Mesh { name = "OW_BoatWake" }; b.wake.MarkDynamic();
            var w = new GameObject("VetSong"); w.transform.SetParent(transform, false);
            w.AddComponent<MeshFilter>().sharedMesh = b.wake;
            var mr = w.AddComponent<MeshRenderer>(); mr.sharedMaterial = wakeMaterial; b.wakeRenderer = mr; b.baseSpeed = speed;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            boats.Add(b); MovingCount++;
        }

        // cắt bớt m mét ở hai đầu một đường tim (theo chiều dài cung)
        static List<Vector2> Trim(List<Vector2> c, float m)
        {
            float total = 0; for (int i = 1; i < c.Count; i++) total += (c[i] - c[i - 1]).magnitude;
            var o = new List<Vector2>(); float acc = 0;
            for (int i = 0; i < c.Count; i++)
            {
                if (i > 0) acc += (c[i] - c[i - 1]).magnitude;
                if (acc >= m && acc <= total - m) o.Add(c[i]);
            }
            return o.Count >= 2 ? o : c;
        }

        Boat Spawn(Color tint)
        {
            var go = Instantiate(boatPrefab, transform); go.name = "TauDanhCa";
            var rs = go.GetComponentsInChildren<Renderer>(true);
            var mpb = new MaterialPropertyBlock(); mpb.SetColor(ColorId, tint);
            var lod0 = new List<Renderer>(); var lod1 = new List<Renderer>(); var lod2 = new List<Renderer>();
            foreach (var r in rs)
            {
                if (boatMaterial) r.sharedMaterial = boatMaterial;
                r.SetPropertyBlock(mpb);
                if (r.name.EndsWith("LOD0")) lod0.Add(r); else if (r.name.EndsWith("LOD1")) lod1.Add(r); else lod2.Add(r);
            }
            var lg = go.GetComponent<LODGroup>() ?? go.AddComponent<LODGroup>();
            lg.SetLODs(new[] { new LOD(0.22f, lod0.ToArray()), new LOD(0.07f, lod1.ToArray()), new LOD(0.012f, lod2.ToArray()) });
            lg.RecalculateBounds();
            return new Boat { tr = go.transform };
        }

        void Update() { Tick(Time.deltaTime); }

        void Tick(float dt)
        {
            float t = Time.timeSinceLevelLoad;           // = _Time.y của shader
            foreach (var b in boats)
            {
                if (b.state != 0) TickDamage(b, dt);
                if (b.state == 3) continue;
                if (!b.moored) { b.s += b.speed * dt; b.pos = b.loop.Sample(b.s, out b.dir); }
                Place(b, t, dt);
                if (!b.moored && (b.wakeRenderer == null || b.wakeRenderer.enabled)) BuildWake(b);
            }
        }

        void Place(Boat b, float t, float dt)
        {
            Vector2 f = b.dir.sqrMagnitude > 1e-4f ? b.dir.normalized : Vector2.up, r = Right(f);
            float h = WaveHeight(b.pos.x, b.pos.y, t, waveAmp);
            float hb = WaveHeight(b.pos.x + f.x * 4f, b.pos.y + f.y * 4f, t, waveAmp), hs = WaveHeight(b.pos.x - f.x * 4f, b.pos.y - f.y * 4f, t, waveAmp);
            float hr = WaveHeight(b.pos.x + r.x * 1.6f, b.pos.y + r.y * 1.6f, t, waveAmp), hl = WaveHeight(b.pos.x - r.x * 1.6f, b.pos.y - r.y * 1.6f, t, waveAmp);
            float targetPitch = Mathf.Atan2(hb - hs, 8f) * Mathf.Rad2Deg * 0.8f;
            float targetRoll = Mathf.Atan2(hr - hl, 3.2f) * Mathf.Rad2Deg * 0.9f + Mathf.Sin(t * 0.9f + b.phase) * 0.8f;
            float k = dt <= 0 ? 1f : 1f - Mathf.Exp(-dt * 3f);
            b.pitch = Mathf.Lerp(b.pitch, targetPitch, k); b.roll = Mathf.Lerp(b.roll, targetRoll, k);
            var yaw = Quaternion.LookRotation(new Vector3(f.x, 0, f.y)) * Quaternion.Euler(0, -90f, 0);    // mũi tàu = trục +X của mô hình
            // mô hình: +X mũi, +Z mạn trái → chúc mũi = quay quanh Z, lắc = quay quanh X
            // 21.284: chìm — hạ 3,2 m, chúc mũi 18°, nghiêng 24°
            float sk = b.sink * b.sink;
            b.tr.SetPositionAndRotation(new Vector3(b.pos.x, waterY + hullOffset + h * 0.85f - 3.2f * sk, b.pos.y), yaw * Quaternion.Euler(-b.roll - 24f * sk, 0, b.pitch - 18f * sk));
        }

        readonly List<Vector3> wv = new List<Vector3>(); readonly List<Vector2> wu = new List<Vector2>(); readonly List<Color> wc = new List<Color>(); readonly List<int> wi = new List<int>();
        void BuildWake(Boat b)
        {
            wv.Clear(); wu.Clear(); wc.Clear(); wi.Clear();
            int n = Mathf.Max(4, wakeSegments);
            for (int k = 0; k <= n; k++)
            {
                float a = k / (float)n, back = a * wakeLength;
                var p = b.loop.Sample(b.s + 4.2f - back, out var d);           // bắt đầu từ mũi tàu (+4,2 m)
                d = d.sqrMagnitude > 1e-4f ? d.normalized : b.dir;
                var r = Right(d); float half = 1.5f + back * 0.36f;              // góc vệt Kelvin ≈ 19,5°
                wv.Add(new Vector3(p.x - r.x * half, waterY, p.y - r.y * half)); wu.Add(new Vector2(a, -1));
                wv.Add(new Vector3(p.x + r.x * half, waterY, p.y + r.y * half)); wu.Add(new Vector2(a, 1));
                var c = new Color(1, 1, 1, Mathf.Clamp01(b.speed / 5f)); wc.Add(c); wc.Add(c);
                if (k < n) { int i = k * 2; wi.Add(i); wi.Add(i + 2); wi.Add(i + 1); wi.Add(i + 1); wi.Add(i + 2); wi.Add(i + 3); }
            }
            var m = b.wake;
            // 21.279: không Clear() mỗi khung (WebGL tạo lại bộ đệm GPU mỗi lần) — số đỉnh cố định, chỉ ghi lại đỉnh/uv/màu
            if (m.vertexCount != wv.Count) { m.Clear(); m.SetVertices(wv); m.SetUVs(0, wu); m.SetColors(wc); m.SetTriangles(wi, 0, false); }
            else { m.SetVertices(wv); m.SetUVs(0, wu); m.SetColors(wc); }
            m.bounds = new Bounds(new Vector3(b.pos.x, waterY, b.pos.y), new Vector3(wakeLength * 2.4f, 2f, wakeLength * 2.4f));
        }

        void OnDestroy() { foreach (var b in boats) if (b.wake) Destroy(b.wake); }
    }
}
