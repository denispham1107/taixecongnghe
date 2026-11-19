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
            var s1 = Line(new Vector2(317.25f, -900f), new Vector2(317.25f, 140f));
            var s2 = new List<Vector2>();
            Vector2 C = new Vector2(234.5f, 231.5f); const float R = 82.75f;
            for (int k = 0; k <= 30; k++) { float a = Mathf.Deg2Rad * Mathf.Lerp(35f, 90f, k / 30f); s2.Add(C + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * R); }
            Line(new Vector2(234.5f, 314.25f), new Vector2(-75f, 314.25f), s2);
            var s3 = Line(new Vector2(-221f, 314.25f), new Vector2(-900f, 314.25f));
            return new List<List<Vector2>> { s1, s2, s3 };
        }

        // ---------------------------------------------------------------- tàu
        sealed class Boat
        {
            public Transform tr; public Loop loop; public float s, speed, phase; public bool moored;
            public Vector2 pos, dir; public Mesh wake; public float pitch, roll;
        }
        readonly List<Boat> boats = new List<Boat>();
        public int MovingCount { get; private set; }
        public int MooredCount { get; private set; }
        public Transform BoatTransform(int i) => i >= 0 && i < boats.Count ? boats[i].tr : null;
        public bool IsMoored(int i) => boats[i].moored;
        public float BoatS(int i) => boats[i].s;

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
                for (int k = 0; k < n; k++)
                {
                    var b = Spawn(tints[ti++ % tints.Length]);
                    b.loop = loop; b.speed = e.v; b.s = start + loop.Length * k / n; b.phase = (float)rnd.NextDouble() * 10f;
                    b.wake = new Mesh { name = "OW_BoatWake" }; b.wake.MarkDynamic();
                    var w = new GameObject("VetSong"); w.transform.SetParent(transform, false);
                    w.AddComponent<MeshFilter>().sharedMesh = b.wake;
                    var mr = w.AddComponent<MeshRenderer>(); mr.sharedMaterial = wakeMaterial;
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
                    boats.Add(b); MovingCount++;
                }
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
                if (!b.moored) { b.s += b.speed * dt; b.pos = b.loop.Sample(b.s, out b.dir); }
                Place(b, t, dt);
                if (!b.moored) BuildWake(b);
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
            b.tr.SetPositionAndRotation(new Vector3(b.pos.x, waterY + hullOffset + h * 0.85f, b.pos.y), yaw * Quaternion.Euler(-b.roll, 0, b.pitch));
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
            var m = b.wake; m.Clear(); m.SetVertices(wv); m.SetUVs(0, wu); m.SetColors(wc); m.SetTriangles(wi, 0, false);
            m.bounds = new Bounds(new Vector3(b.pos.x, waterY, b.pos.y), new Vector3(wakeLength * 2.4f, 2f, wakeLength * 2.4f));
        }

        void OnDestroy() { foreach (var b in boats) if (b.wake) Destroy(b.wake); }
    }
}
