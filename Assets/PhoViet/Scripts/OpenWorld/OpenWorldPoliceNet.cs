using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.299 — người dùng 21/11/2026: «Xe CSGT khi cách xe người chơi trong phạm vi 4,5 m sẽ bắn ra 1 tấm lưới điện bao trùm xe
    // người chơi (tia điện xẹt xẹt chạy quanh lưới). Xe dính lưới không di chuyển được trong 3 giây → CSGT dễ bắt hơn.»
    // 21.299b — «lưới còn sơ sài xấu, dựng lại đẹp hơn, các tia điện có hiệu ứng ánh sáng»:
    // • Lưới thật: mắt lưới HÌNH THOI đan chéo (2 họ dây chéo ~0,3 m) phủ vỏ siêu elip ôm thân xe, hai đầu túm thành vòng dây,
    //   dây viền to ở mép dưới, 6 QUẢ NẶNG kim loại có đèn nháy ở mép (kiểu súng bắn lưới). Dây là dải chéo chữ thập (nhìn góc nào
    //   cũng thấy) gộp 1 lưới: lõi sáng trắng-xanh + quầng xanh rộng (2 lượt vẽ cho cả tấm), xung điện chạy dọc dây (shader).
    // • Ánh sáng: 9 tia điện mỗi tia lõi trắng + quầng xanh rộng (giả bloom), đổi hình lởm chởm mỗi 0,05 s, có tia phóng xuống đất;
    //   chớp sáng ở đầu tia; ĐÈN ĐIỂM xanh thật chiếu lên xe/mặt đường, nhấp nháy theo tia; vệt sáng loang trên mặt đường dưới xe;
    //   ~40 nút lưới lóe sáng ngẫu nhiên; tia lửa xẹt rơi xuống. Tiếng điện rè + xẹt tự tổng hợp.
    // • Luật: OpenWorldPolice (Chasing) gọi TryFire: khoảng cách CSGT–xe (số trên HUD) ≤ range, xe chạm đất, chưa hỏng/chìm/bị bắt,
    //   hết hồi → lưới bay 0,3 s theo đường cong, nở 20 % → 100 %, tới nơi OpenWorldDriver.Net(holdSeconds) khóa xe; hết thì mờ 0,35 s.
    // • Vật liệu trong Resources/PoliceNet (shader PhoViet/OpenWorld/Electric cộng sáng; quả nặng Standard) → chắc chắn vào build;
    //   mỗi lưới nhân bản vật liệu để mờ dần/nhấp nháy riêng.
    public class OpenWorldPoliceNet : MonoBehaviour
    {
        public float range = 6f, holdSeconds = 25f, flightTime = 0.3f, cooldown = 4f;   // 21.300: tầm 6 m; giữ tới khi bị bắt (25 s chỉ là chốt an toàn khi CSGT kẹt)
        public OpenWorldDriver player;
        public enum Phase { Idle, Flying, Holding, Fading }
        public Phase State { get; private set; }
        public int Shots { get; private set; }
        public int Hits { get; private set; }
        // 21.312: khiên năng lượng (OpenWorldShield) phá vỡ lưới — lưới đang giữ xe vỡ ngay khi bật khiên, lưới bắn tới khi khiên
        // đang bật vỡ lúc chạm khiên (OpenWorldNetShatter: mảnh lưới Blender văng tung + sóng xung kích + tia lửa điện).
        public int Breaks { get; private set; }
        public static float LastBreakAt = -99f;
        public static readonly List<OpenWorldPoliceNet> All = new List<OpenWorldPoliceNet>();
        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }
        public void ShatterIfOnPlayer() { if (State == Phase.Holding) Shatter(); }
        void Shatter()
        {
            if (!root) return;
            var rb = player ? player.GetComponent<Rigidbody>() : null;
            OpenWorldNetShatter.Spawn(root.position, root.rotation, halfExt * root.localScale.x, rb ? rb.linearVelocity : Vector3.zero);
            if (player && State == Phase.Holding) player.ClearNet();
            if (player && player.Shield) player.Shield.Absorb(root.position + Vector3.up * 0.4f, 0.7f);
            State = Phase.Idle; readyAt = Time.time + cooldown; root.gameObject.SetActive(false); if (buzz) buzz.Stop();
            Breaks++; OpenWorldShield.NetsBroken++; LastBreakAt = Time.time;
        }

        const float PhiMax = 1.35f, Cell = 0.1f;
        const int Arcs = 9, ArcPts = 16, Nodes = 40;
        float t0, readyAt, fadeUntil, nextArc, flash;
        Vector3 fromPos; Quaternion fromRot;
        Transform root; MeshFilter ropeMf; Mesh ropeMesh; Material coreMat, haloMat, arcMat, arcHaloMat, dotMat, groundMat;
        LineRenderer[] arcs, arcHalos; ParticleSystem sparks, dots; Light glow; Transform ground; Transform[] weights; Renderer[] weightLeds;
        AudioSource buzz;
        Vector3 halfExt = new Vector3(1.05f, 0.7f, 2.05f), center = new Vector3(0, 0.65f, 0);
        readonly System.Random rnd = new System.Random(299);
        readonly List<Vector3[]> lines = new List<Vector3[]>();     // dây lưới (đơn vị, trước khi nhân kích thước) — cho tia điện chạy theo
        readonly List<Vector3> nodes = new List<Vector3>();          // nút lưới (đơn vị)
        Color coreC, haloC, arcC, arcHaloC, dotC, groundC;
        ParticleSystem.Particle[] dotBuf;

        public void ResetNet()
        {
            State = Phase.Idle; readyAt = Time.time + 1f;
            if (root) root.gameObject.SetActive(false);
            if (buzz) buzz.Stop();
        }

        // Gọi từ OpenWorldPolice.FixedUpdate khi đang đuổi.
        public void TryFire(float distance)
        {
            if (State != Phase.Idle || Time.time < readyAt || !player) return;
            if (distance > range || player.Crashed || player.Sinking || player.frozen || player.Airborne || player.Netted) return;
            Build();
            FitToPlayer();
            BuildRopeMesh();
            State = Phase.Flying; t0 = Time.time; Shots++;
            fromPos = transform.position + Vector3.up * 1.7f + transform.forward * 1.2f; fromRot = transform.rotation;
            root.gameObject.SetActive(true);
            if (buzz) { buzz.volume = 0.25f; buzz.Play(); }
        }

        void LateUpdate()
        {
            if (State == Phase.Idle || !root || !player) return;
            float now = Time.time;
            var pt = player.transform; var pr = Quaternion.Euler(0, pt.eulerAngles.y, 0);
            Vector3 targetPos = pt.position + pr * center;
            float alpha = 1f, land = 1f;
            switch (State)
            {
                case Phase.Flying:
                {
                    float k = Mathf.Clamp01((now - t0) / flightTime), e = 1 - (1 - k) * (1 - k);
                    var p = Vector3.Lerp(fromPos, targetPos, e) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                    root.SetPositionAndRotation(p, Quaternion.Slerp(fromRot, pr, e) * Quaternion.Euler(0, (1 - e) * 140f, 0));
                    root.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, e);
                    alpha = 0.55f + 0.45f * e; land = 0f;
                    if (k >= 1f)
                    {
                        if (player.Crashed || player.Sinking) { Fade(now); break; }
                        if (player.Shielded) { Shatter(); return; }          // 21.312: chạm khiên → vỡ tung
                        player.Net(holdSeconds); Hits++;
                        State = Phase.Holding; t0 = now; flash = 1f;
                        if (buzz) buzz.volume = 0.55f;
                        Burst(40, true);
                    }
                    break;
                }
                case Phase.Holding:
                    root.SetPositionAndRotation(targetPos, pr); root.localScale = Vector3.one;
                    if (player.Shielded) { Shatter(); return; }              // 21.312
                    // 21.300: lưới giữ xe tới khi CSGT bắt (OpenWorldPolice.ResetChase/ResetNet dọn khi chơi lại); chốt an toàn holdSeconds
                    if (now - t0 >= holdSeconds || !player.Netted) Fade(now);
                    break;
                case Phase.Fading:
                    root.SetPositionAndRotation(targetPos, pr);
                    alpha = Mathf.Clamp01((fadeUntil - now) / 0.35f);
                    if (buzz) buzz.volume = 0.55f * alpha;
                    if (now >= fadeUntil) { State = Phase.Idle; readyAt = now + cooldown; root.gameObject.SetActive(false); if (buzz) buzz.Stop(); return; }
                    break;
            }
            Animate(now, alpha, land);
        }

        void Fade(float now) { State = Phase.Fading; fadeUntil = now + 0.35f; }

        // ------------------------------------------------------------ hình học
        static float SP(float v, float e) => Mathf.Sign(v) * Mathf.Pow(Mathf.Abs(v), e);
        // vỏ siêu elip đơn vị: th ∈ [0, π] (phải-dưới → nóc → trái-dưới), ph ∈ [−π/2, π/2] (đuôi → đầu)
        static Vector3 Hull(float th, float ph)
        {
            const float e = 0.45f; float cp = Mathf.Cos(ph);
            return new Vector3(SP(Mathf.Cos(th), e) * SP(cp, e), -1f + 2f * SP(Mathf.Sin(th), e) * SP(cp, e), SP(Mathf.Sin(ph), e));
        }
        static Vector3 HullUV(float u, float v) => Hull(Mathf.Lerp(0.03f, Mathf.PI - 0.03f, u), Mathf.Lerp(-PhiMax, PhiMax, v));

        void BuildLines()
        {
            lines.Clear(); nodes.Clear();
            // 2 họ dây chéo v = u + c và v = −u + c trên miền (u, v) ∈ [0,1]² → mắt lưới hình thoi
            for (int fam = 0; fam < 2; fam++)
                for (float c = -1f + Cell * 0.5f; c < 2f; c += Cell)
                {
                    float uA, uB;
                    if (fam == 0) { uA = Mathf.Max(0, -c); uB = Mathf.Min(1, 1 - c); }      // v = u + c
                    else { uA = Mathf.Max(0, c - 1); uB = Mathf.Min(1, c); }              // v = −u + c
                    if (uB - uA < 0.04f) continue;
                    int n = Mathf.Max(3, Mathf.CeilToInt((uB - uA) / 0.035f));
                    var pts = new Vector3[n + 1];
                    for (int i = 0; i <= n; i++) { float u = Mathf.Lerp(uA, uB, i / (float)n), v = fam == 0 ? u + c : -u + c; pts[i] = HullUV(u, v); }
                    lines.Add(pts);
                }
            // dây viền: mép dưới hai bên + vòng túm hai đầu
            for (int side = 0; side < 2; side++)
            {
                var pts = new Vector3[33]; for (int i = 0; i <= 32; i++) pts[i] = HullUV(side, i / 32f); lines.Add(pts);
                var ring = new Vector3[33]; for (int i = 0; i <= 32; i++) ring[i] = HullUV(i / 32f, side); lines.Add(ring);
            }
            for (int a = 0; a < 10; a++) for (int b = 0; b < 10; b++) nodes.Add(HullUV((a + .5f) / 10f, (b + .5f) / 10f + ((a & 1) == 0 ? 0 : .05f)));
        }

        // Một lưới cho cả tấm: mỗi đoạn dây là dải chữ thập (dải nằm trên mặt lưới + dải dựng đứng). Submesh 0 = lõi, 1 = quầng.
        void BuildRopeMesh()
        {
            if (lines.Count == 0) BuildLines();
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var core = new List<int>(); var halo = new List<int>();
            int borderFrom = lines.Count - 4;
            for (int li = 0; li < lines.Count; li++)
            {
                var src = lines[li]; bool border = li >= borderFrom;
                float wCore = border ? 0.028f : 0.012f, wHalo = border ? 0.11f : 0.065f, along = 0f;
                for (int i = 0; i < src.Length - 1; i++)
                {
                    var a = Vector3.Scale(src[i], halfExt); var b = Vector3.Scale(src[i + 1], halfExt);
                    var d = b - a; float len = d.magnitude; if (len < 1e-4f) continue; d /= len;
                    var mid = (a + b) * 0.5f; var nrm = Vector3.Scale(mid, new Vector3(1 / (halfExt.x * halfExt.x), 1 / (halfExt.y * halfExt.y), 1 / (halfExt.z * halfExt.z))).normalized;
                    var t1 = Vector3.Cross(d, nrm).normalized;   // trong mặt lưới
                    var t2 = nrm;                                  // dựng đứng khỏi mặt
                    foreach (var (list, w) in new[] { (core, wCore), (halo, wHalo) })
                        foreach (var off in new[] { t1, t2 })
                        {
                            int bi = v.Count;
                            v.Add(a - off * w); v.Add(a + off * w); v.Add(b - off * w); v.Add(b + off * w);
                            uv.Add(new Vector2(along, 0)); uv.Add(new Vector2(along, 1)); uv.Add(new Vector2(along + len, 0)); uv.Add(new Vector2(along + len, 1));
                            list.AddRange(new[] { bi, bi + 2, bi + 1, bi + 1, bi + 2, bi + 3 });
                        }
                    along += len;
                }
            }
            if (!ropeMesh) { ropeMesh = new Mesh { name = "LuoiDien_Day" }; ropeMesh.MarkDynamic(); }
            ropeMesh.Clear();
            ropeMesh.indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            ropeMesh.SetVertices(v); ropeMesh.SetUVs(0, uv); ropeMesh.subMeshCount = 2;
            ropeMesh.SetTriangles(core, 0); ropeMesh.SetTriangles(halo, 1);
            ropeMesh.bounds = new Bounds(Vector3.zero, halfExt * 2.4f);
            ropeMf.sharedMesh = ropeMesh;
            // quả nặng ở mép dưới: 4 góc + giữa hai bên
            var wp = new[] { new Vector2(0, 0.04f), new Vector2(0, 0.5f), new Vector2(0, 0.96f), new Vector2(1, 0.04f), new Vector2(1, 0.5f), new Vector2(1, 0.96f) };
            for (int i = 0; i < weights.Length; i++) weights[i].localPosition = Vector3.Scale(HullUV(wp[i].x, wp[i].y), halfExt) + Vector3.up * 0.05f;
            ground.localPosition = new Vector3(0, -halfExt.y + 0.03f, 0);
            ground.localScale = new Vector3(halfExt.x * 3.4f, halfExt.z * 2.6f, 1);
            glow.transform.localPosition = new Vector3(0, halfExt.y * 0.4f, 0);
        }

        Material Own(string name, out Color c)
        {
            var src = Resources.Load<Material>("PoliceNet/" + name);
            var m = src ? new Material(src) : new Material(Shader.Find("PhoViet/OpenWorld/Electric"));
            c = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white; return m;
        }

        void Build()
        {
            if (root) return;
            root = new GameObject("LuoiDienCSGT").transform;
            coreMat = Own("PV_LuoiDien_Day", out coreC); haloMat = Own("PV_LuoiDien_Quang", out haloC);
            arcMat = Own("PV_LuoiDien_Tia", out arcC); arcHaloMat = Own("PV_LuoiDien_TiaQuang", out arcHaloC);
            dotMat = Own("PV_LuoiDien_Hat", out dotC); groundMat = Own("PV_LuoiDien_Nen", out groundC);

            var rope = new GameObject("MatLuoi"); rope.transform.SetParent(root, false);
            ropeMf = rope.AddComponent<MeshFilter>(); var mr = rope.AddComponent<MeshRenderer>();
            mr.sharedMaterials = new[] { coreMat, haloMat }; Quiet(mr);

            arcs = new LineRenderer[Arcs]; arcHalos = new LineRenderer[Arcs];
            for (int a = 0; a < Arcs; a++) { arcHalos[a] = Line("QuangTia" + a, arcHaloMat, 0.3f); arcs[a] = Line("TiaDien" + a, arcMat, 0.05f); }

            // quả nặng kim loại + đèn nháy
            var metal = Resources.Load<Material>("PoliceNet/PV_LuoiDien_Qua");
            weights = new Transform[6]; weightLeds = new Renderer[6];
            for (int i = 0; i < 6; i++)
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(w.GetComponent<Collider>());
                w.name = "QuaNang" + i; w.transform.SetParent(root, false); w.transform.localScale = Vector3.one * 0.13f;
                var r = w.GetComponent<MeshRenderer>(); if (metal) r.sharedMaterial = metal; r.shadowCastingMode = ShadowCastingMode.Off;
                weights[i] = w.transform;
                var led = GameObject.CreatePrimitive(PrimitiveType.Quad); Destroy(led.GetComponent<Collider>());
                led.name = "DenNhay"; led.transform.SetParent(w.transform, false); led.transform.localScale = Vector3.one * 3.2f;
                var lr = led.GetComponent<MeshRenderer>(); lr.sharedMaterial = dotMat; Quiet(lr); weightLeds[i] = lr;
            }

            // vệt sáng loang trên mặt đường
            var g = GameObject.CreatePrimitive(PrimitiveType.Quad); Destroy(g.GetComponent<Collider>());
            g.name = "VetSangMatDuong"; g.transform.SetParent(root, false); g.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var gr = g.GetComponent<MeshRenderer>(); gr.sharedMaterial = groundMat; Quiet(gr); ground = g.transform;

            // đèn điểm thật
            var lg = new GameObject("DenDien"); lg.transform.SetParent(root, false);
            glow = lg.AddComponent<Light>(); glow.type = LightType.Point; glow.color = new Color(.45f, .78f, 1f); glow.range = 7.5f;
            glow.intensity = 0; glow.shadows = LightShadows.None; glow.renderMode = LightRenderMode.ForcePixel;

            sparks = Particles("TiaLuaXet", 400, true);
            dots = Particles("NutLuoiLoe", Nodes, false); dotBuf = new ParticleSystem.Particle[Nodes];

            buzz = PhoVietAudio.Route(root.gameObject.AddComponent<AudioSource>());
            buzz.loop = true; buzz.playOnAwake = false; buzz.spatialBlend = 0.6f; buzz.minDistance = 6f; buzz.maxDistance = 80f;
            buzz.clip = BuzzClip();
            root.gameObject.SetActive(false);
        }

        static void Quiet(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off; r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        ParticleSystem Particles(string name, int max, bool physics)
        {
            var go = new GameObject(name); go.transform.SetParent(root, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = ps.main; m.loop = true; m.playOnAwake = false; m.maxParticles = max;
            m.simulationSpace = physics ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
            m.startLifetime = physics ? new ParticleSystem.MinMaxCurve(0.15f, 0.4f) : new ParticleSystem.MinMaxCurve(1000f);
            m.startSpeed = physics ? new ParticleSystem.MinMaxCurve(1.5f, 6f) : new ParticleSystem.MinMaxCurve(0f);
            m.startSize = physics ? new ParticleSystem.MinMaxCurve(0.04f, 0.12f) : new ParticleSystem.MinMaxCurve(0.2f);
            m.gravityModifier = physics ? 1.4f : 0f;
            var em = ps.emission; em.enabled = false;
            var sh = ps.shape; sh.enabled = physics; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.04f;
            if (physics)
            {
                var col = ps.colorOverLifetime; col.enabled = true;
                var gr = new Gradient(); gr.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(.4f, .8f, 1f), 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
                col.color = gr;
            }
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = dotMat; Quiet(r);
            ps.Play();
            return ps;
        }

        LineRenderer Line(string name, Material mat, float width)
        {
            var lr = new GameObject(name).AddComponent<LineRenderer>();
            lr.transform.SetParent(root, false);
            lr.useWorldSpace = false; lr.sharedMaterial = mat; lr.positionCount = ArcPts; lr.widthMultiplier = width;
            lr.alignment = LineAlignment.View; lr.textureMode = LineTextureMode.Stretch; lr.numCapVertices = 3;
            Quiet(lr);
            return lr;
        }

        // Kích thước lưới theo khung va chạm của xe người chơi (cục bộ theo hướng xe).
        void FitToPlayer()
        {
            var pt = player.transform; var inv = Quaternion.Inverse(Quaternion.Euler(0, pt.eulerAngles.y, 0));
            bool any = false; Vector3 mn = Vector3.zero, mx = Vector3.zero;
            foreach (var c in player.GetComponentsInChildren<Collider>())
            {
                if (!c.enabled || c.isTrigger) continue;
                var b = c.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
                    var l = inv * (corner - pt.position);
                    if (!any) { mn = mx = l; any = true; } else { mn = Vector3.Min(mn, l); mx = Vector3.Max(mx, l); }
                }
            }
            if (!any) return;
            mn.y = Mathf.Max(mn.y, 0.02f);
            halfExt = (mx - mn) * 0.5f + new Vector3(0.16f, 0.1f, 0.2f);
            center = (mx + mn) * 0.5f; center.y = mn.y + halfExt.y - 0.06f;
        }

        Vector3 OnNet(Vector3 unit) => Vector3.Scale(unit, halfExt);

        void Animate(float now, float alpha, float land)
        {
            var cam = Camera.main;                                        // quầng đèn nháy trên quả nặng quay về camera
            if (cam) foreach (var l in weightLeds) l.transform.rotation = cam.transform.rotation;
            float buzzK = 0.75f + 0.25f * Mathf.PerlinNoise(now * 11f, 0.3f);
            flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 5f);
            coreMat.SetColor("_Color", coreC * alpha * (buzzK + flash * 0.8f));
            haloMat.SetColor("_Color", haloC * alpha * (buzzK + flash));
            groundMat.SetColor("_Color", groundC * alpha * land * (0.6f + 0.4f * buzzK + flash));

            if (now >= nextArc)
            {
                nextArc = now + 0.05f;
                for (int a = 0; a < Arcs; a++)
                {
                    bool on = rnd.NextDouble() < 0.8;
                    arcs[a].enabled = arcHalos[a].enabled = on; if (!on) continue;
                    bool toGround = land > 0.5f && rnd.NextDouble() < 0.22;
                    var pts = new Vector3[ArcPts];
                    if (toGround)
                    {
                        // tia phóng từ mép dưới lưới xuống mặt đường
                        var s = OnNet(HullUV(rnd.Next(2), (float)rnd.NextDouble()));
                        var e = s + new Vector3(((float)rnd.NextDouble() - .5f) * 0.8f, 0, ((float)rnd.NextDouble() - .5f) * 0.8f); e.y = -halfExt.y - 0.02f;
                        for (int i = 0; i < ArcPts; i++) pts[i] = Vector3.Lerp(s, e, i / (float)(ArcPts - 1));
                        Burst(4, false, root.TransformPoint(e));
                    }
                    else
                    {
                        // tia chạy dọc một đoạn dây lưới
                        var src = lines[rnd.Next(lines.Count)];
                        float start = (float)rnd.NextDouble() * 0.55f, len = 0.25f + (float)rnd.NextDouble() * 0.45f;
                        for (int i = 0; i < ArcPts; i++)
                        {
                            float u = Mathf.Clamp01(start + len * i / (ArcPts - 1)) * (src.Length - 1);
                            int i0 = Mathf.FloorToInt(u), i1 = Mathf.Min(src.Length - 1, i0 + 1);
                            pts[i] = OnNet(Vector3.Lerp(src[i0], src[i1], u - i0));
                        }
                    }
                    for (int i = 1; i < ArcPts - 1; i++)
                        pts[i] += new Vector3((float)rnd.NextDouble() - .5f, (float)rnd.NextDouble() - .5f, (float)rnd.NextDouble() - .5f) * (toGround ? 0.22f : 0.15f);
                    arcs[a].SetPositions(pts); arcHalos[a].SetPositions(pts);
                    float w = 0.03f + (float)rnd.NextDouble() * 0.05f;
                    arcs[a].widthMultiplier = w; arcHalos[a].widthMultiplier = w * 6f;
                    var ca = new Color(1, 1, 1, alpha); arcs[a].startColor = arcs[a].endColor = ca; arcHalos[a].startColor = arcHalos[a].endColor = ca;
                }
                arcMat.SetColor("_Color", arcC * (0.8f + (float)rnd.NextDouble() * 0.5f));
                arcHaloMat.SetColor("_Color", arcHaloC * (0.7f + (float)rnd.NextDouble() * 0.6f));
                if (State == Phase.Holding && rnd.NextDouble() < 0.75) Burst(3 + rnd.Next(6), false);
                if (rnd.NextDouble() < 0.12) flash = 1f;

                // nút lưới lóe sáng
                for (int i = 0; i < Nodes; i++)
                {
                    var p = OnNet(nodes[rnd.Next(nodes.Count)]);
                    float s = rnd.NextDouble() < 0.25 ? 0.18f + (float)rnd.NextDouble() * 0.25f : 0.06f + (float)rnd.NextDouble() * 0.08f;
                    dotBuf[i] = new ParticleSystem.Particle { position = p, startSize = s, startColor = new Color(1, 1, 1, alpha), remainingLifetime = 1000f, startLifetime = 1000f };
                }
                dots.SetParticles(dotBuf, Nodes);
                for (int i = 0; i < weightLeds.Length; i++) weightLeds[i].enabled = ((int)(now * 6f) + i) % 3 != 0;
            }
            glow.intensity = alpha * (land > 0.5f ? 1f : 0.4f) * (1.6f + 2.2f * buzzK * (float)rnd.NextDouble() + 3f * flash);
        }

        void Burst(int n, bool all) => Burst(n, all, Vector3.zero);
        void Burst(int n, bool all, Vector3 at)
        {
            if (!sparks || lines.Count == 0) return;
            int reps = all ? 8 : 1;
            for (int k = 0; k < reps; k++)
            {
                var p = at;
                if (p == Vector3.zero) { var src = lines[rnd.Next(lines.Count)]; p = root.TransformPoint(OnNet(src[rnd.Next(src.Length)])); }
                var ep = new ParticleSystem.EmitParams { position = p, applyShapeToPosition = true };
                sparks.Emit(ep, all ? Mathf.Max(1, n / reps) : n);
            }
        }

        static AudioClip BuzzClip()
        {
            const int hz = 22050; var w = new float[hz * 2]; var r = new System.Random(7); float crack = 0;
            for (int i = 0; i < w.Length; i++)
            {
                float t = (float)i / hz;
                if (r.NextDouble() < 0.0016) crack = 1f;                        // tiếng «xẹt» ngẫu nhiên
                crack *= 0.9965f;
                float hum = Mathf.Sin(t * 2 * Mathf.PI * 100f) * 0.5f + Mathf.Sin(t * 2 * Mathf.PI * 200f) * 0.25f;   // rè điện 100 Hz
                float noise = (float)(r.NextDouble() * 2 - 1);
                w[i] = Mathf.Clamp(hum * 0.18f * (0.7f + 0.3f * Mathf.Sin(t * 37f)) + noise * (0.05f + 0.55f * crack), -1f, 1f);
            }
            var clip = AudioClip.Create("Lưới điện rè", w.Length, 1, hz, false); clip.SetData(w, 0); return clip;
        }

        void OnDestroy()
        {
            if (root) Destroy(root.gameObject);
            foreach (var m in new[] { coreMat, haloMat, arcMat, arcHaloMat, dotMat, groundMat }) if (m) Destroy(m);
            if (ropeMesh) Destroy(ropeMesh);
        }
    }
}
