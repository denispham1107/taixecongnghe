using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.300 — người dùng 21/11/2026: nút «LAZER» (trên nút NHẢY, cỡ nút TÊN LỬA) + thanh «NĂNG LƯỢNG 100/100» dưới số tên lửa.
    // • Giữ nút (phím L / chuột giữ nút trên máy tính) → 2 tia lazer phóng liên tục từ 2 đèn pha. 21.328: chỉ cần bấm 1 lần,
    //   tia tự bắn tới khi hết năng lượng. Ảnh hiệu ứng dựng bằng Blender
    //   (ArtSource/Blender/OpenWorld/ow_laser_fx.py → Resources/Laser), shader cộng sáng PhoViet/OpenWorld/Laser.
    // • Năng lượng 100: bấm là trừ 25 ngay, mỗi 0,5 s trừ thêm 25 (bắn ~1,5–2 s). Hết năng lượng → khóa 5 s
    //   (21.327, trước 10 s); hết 5 s năng lượng hồi đầy 100 (người dùng chọn). Vào lượt mới: đầy 100, không khóa.
    // • Tia trúng: xe NPC (ô tô/xe máy) → OpenWorldTraffic.LaserKill (bốc cháy rồi nổ tung thành mảnh); xe tăng/khủng long →
    //   OpenWorldHazards.LaserHit (nung 0,3/0,5 s rồi nổ tung); xe CSGT → chỉ khựng 1,5 s (người dùng chọn) + khói; nhà/cột/cây
    //   → tia dừng ở đó, tóe lửa. Vụ nổ do lazer không làm hại xe người chơi.
    // 21.300b — người dùng: «tia còn đơn giản, xấu → thêm nhiều hiệu ứng ánh sáng, bán kính +10 %, tầm 9 → 15 m; dựng lại hiệu ứng
    // trúng đích; bắn trượt cũng phải thấy ngọn tia đẹp»:
    // • Thân tia 3 lớp: quầng đỏ rộng (3,4× bề rộng, dải Gauss mềm), thân tia năng lượng (lõi + 3 sợi xoắn cuộn dọc), lõi trắng nóng
    //   mảnh; bề rộng 0,42 → 0,462 m (+10 %); nhấp nháy năng lượng. Xung sáng chạy dọc tia 32 m/s (hạt kéo dài) + đốm năng lượng
    //   bay ra hai bên. Đầu đèn: 2 chớp sao xoay ngược chiều + quầng tròn.
    // • Trúng đích: chớp sao lớn 2 lớp xoay, quầng sáng 2,4 m, VÒNG SÓNG xung kích nở liên tục (3 vòng/tia), MƯA TIA LỬA nóng
    //   chảy kéo vệt văng ngược + rơi theo trọng lực, đốm than hồng, VẾT CHÁY phát sáng áp trên bề mặt trúng (theo pháp tuyến),
    //   đèn cam đỏ mạnh hơn; khói vẫn do OpenWorldHazards.LaserScorch.
    // • Bắn trượt: thân tia dừng cách tầm 2,2 m, nối NGỌN TIA (ảnh Blender: thon nhọn, sợi xoắn thu về mũi, chùm năng lượng tỏa ra
    //   rồi tan) dài 2,6 m + chấm sáng mũi + đốm năng lượng tan ra; quầng/lõi thon dần ở cuối.
    public class OpenWorldLaser : MonoBehaviour
    {
        public const float MaxEnergy = 100f, Cost = 25f, TickSeconds = 0.5f, CooldownSeconds = 5f, Range = 15f;   // 21.327: khóa/hồi năng lượng 10 → 5 s
        [Tooltip("Vị trí 2 đèn pha trong khung xe (x ngang, y cao, z dọc)")] public Vector3 lampLocal = new Vector3(0.6f, 0.52f, 1.86f);
        public float beamWidth = 0.462f;                                   // 21.300b: +10 % (0,42)
        const float TipLen = 2.6f, TipBack = 2.2f, PulseSpeed = 32f;

        public float Energy { get; private set; } = MaxEnergy;
        public float CooldownLeft { get; private set; }
        public bool Firing { get; private set; }
        public bool Ready => CooldownLeft <= 0f && Energy >= Cost - 0.01f;
        public int Shots { get; private set; }
        public int Hits { get; private set; }
        [System.NonSerialized] public bool mouseHeld, testHold;

        OpenWorldDriver driver; OpenWorldGame game; OpenWorldHazards hazards; OpenWorldTraffic traffic;
        float tick;

        class Beam
        {
            public LineRenderer halo, body, core, tip;
            public Transform muzzle, muzzle2, muzzleGlow, flare, flare2, flareGlow, scorch, tipGlow;
            public Transform[] rings = new Transform[3]; public Renderer[] ringR = new Renderer[3]; public float[] ringAge = { 9, 9, 9 };
            public float nextRing;
        }
        readonly Beam[] beams = { new Beam(), new Beam() };
        Transform root; MaterialPropertyBlock mpb; Light glow; AudioSource hum;
        ParticleSystem pulses, motes, molten;
        readonly RaycastHit[] hits = new RaycastHit[16];
        readonly HashSet<int> killedThisFrame = new HashSet<int>();
        AnimationCurve fullCurve, taperCurve;

        public void ResetLaser() { StopFiring(false); Energy = MaxEnergy; CooldownLeft = 0f; }

        void Awake() { driver = GetComponent<OpenWorldDriver>(); }

        bool Held => (driver && driver.LaserHeld) || Input.GetKey(KeyCode.L) || mouseHeld || testHold;
        bool wasHeld;

        bool CanShoot()
        {
            if (!game) game = FindAnyObjectByType<OpenWorldGame>();
            if (game && game.Mode != OpenWorldGame.State.Riding) return false;
            return driver && !driver.frozen && !driver.Crashed && !driver.Sinking && !driver.Netted && Time.timeScale > 0f;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (CooldownLeft > 0f)
            {
                CooldownLeft -= dt;
                if (CooldownLeft <= 0f) { CooldownLeft = 0f; Energy = MaxEnergy; }
            }
            // 21.328 (người dùng): bỏ giữ nút — chạm/bấm 1 lần là Lazer tự bắn tới khi hết năng lượng (100 → ~1,5 s), rồi khóa
            // 5 s. Chỉ tính lúc vừa bấm xuống (giữ hay bấm thêm khi đang bắn không có tác dụng). Hết lượt/xe hỏng/kẹt lưới → ngừng.
            bool held = Held, pressed = held && !wasHeld; wasHeld = held;
            bool can = CanShoot();
            if (Firing)
            {
                if (!can) { StopFiring(true); return; }
                tick += dt;
                while (tick >= TickSeconds)
                {
                    tick -= TickSeconds;
                    if (Energy < Cost - 0.01f) { StopFiring(true); return; }
                    Energy -= Cost;
                }
            }
            else if (pressed && can && Ready)
            {
                Firing = true; tick = 0f; Energy -= Cost; Shots++;
                Build(); root.gameObject.SetActive(true);
                foreach (var b in beams) { for (int i = 0; i < 3; i++) b.ringAge[i] = 9f; b.nextRing = 0f; }
                if (hum) { hum.time = 0; hum.Play(); }
            }
        }

        void StopFiring(bool cooldown)
        {
            if (Firing && cooldown) CooldownLeft = CooldownSeconds;
            Firing = false;
            if (root) root.gameObject.SetActive(false);
            if (hum) hum.Stop();
        }

        void LateUpdate()
        {
            if (!Firing || !root) return;
            if (!hazards) hazards = FindAnyObjectByType<OpenWorldHazards>();
            if (!traffic) traffic = FindAnyObjectByType<OpenWorldTraffic>();
            var t = transform; var fwd = t.forward; fwd.y = 0; fwd.Normalize();
            var cam = Camera.main; var camRot = cam ? cam.transform.rotation : Quaternion.identity;
            float now = Time.time, dt = Time.deltaTime;
            killedThisFrame.Clear();
            Vector3 lightAt = Vector3.zero; bool anyHit = false;
            for (int bi = 0; bi < 2; bi++)
            {
                var B = beams[bi];
                var o = t.TransformPoint(new Vector3(bi == 0 ? -lampLocal.x : lampLocal.x, lampLocal.y, lampLocal.z));
                float len = Range; bool hit = false; Vector3 hitPoint = o + fwd * Range, hitNormal = -fwd;
                int n = Physics.SphereCastNonAlloc(o - fwd * 0.3f, 0.17f, fwd, hits, Range + 0.3f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                System.Array.Sort(hits, 0, n, HitComparer);
                for (int i = 0; i < n; i++)
                {
                    var h = hits[i]; var c = h.collider; if (!c) continue;
                    if (c.transform.IsChildOf(t)) continue;                                 // thân xe người chơi
                    var p = h.distance > 0f ? h.point : c.ClosestPoint(o);
                    float d = Mathf.Max(0.2f, Vector3.Dot(p - o, fwd));
                    Apply(c, p, fwd, dt);
                    len = Mathf.Min(Range, d); hitPoint = o + fwd * len; hit = true;
                    hitNormal = h.distance > 0f && h.normal.sqrMagnitude > .5f ? h.normal : -fwd;
                    break;
                }
                anyHit |= hit;
                float flick = 0.9f + 0.2f * Mathf.PerlinNoise(now * 23f, bi * 3.1f);
                float w = beamWidth * flick;
                // thân tia: trúng → tới điểm trúng; trượt → dừng trước tầm TipBack m, nối ngọn tia
                float bodyLen = hit ? len : Mathf.Max(0.5f, len - TipBack);
                var end = o + fwd * bodyLen;
                SetLine(B.halo, o, hit ? hitPoint : o + fwd * (len - 0.3f), w * 3.4f, hit ? fullCurve : taperCurve, new Color(1f, .16f, .1f, .55f + .15f * flick));
                SetLine(B.body, o, end, w, fullCurve, Color.white);
                SetLine(B.core, o, hit ? hitPoint : o + fwd * (len - 0.6f), w * 0.26f, hit ? fullCurve : taperCurve, new Color(1f, .96f, .92f, 1f));
                mpb.Clear(); mpb.SetVector("_MainTex_ST", new Vector4(bodyLen / (8f * w), 1f, -now * 3.4f - bi * 0.37f, 0f)); B.body.SetPropertyBlock(mpb);
                mpb.Clear(); mpb.SetVector("_MainTex_ST", new Vector4(bodyLen / 6f, 1f, -now * 2.1f, 0f)); B.halo.SetPropertyBlock(mpb); B.core.SetPropertyBlock(mpb);
                B.tip.gameObject.SetActive(!hit);
                if (!hit)
                {
                    var tipStart = end - fwd * 0.02f;
                    SetLine(B.tip, tipStart, tipStart + fwd * TipLen, w, fullCurve, Color.white);
                    mpb.Clear(); mpb.SetVector("_MainTex_ST", new Vector4(1f, 1f, 0f, 0f)); B.tip.SetPropertyBlock(mpb);
                    var tipPt = tipStart + fwd * (TipLen * 0.88f);
                    B.tipGlow.position = tipPt; B.tipGlow.rotation = camRot; B.tipGlow.localScale = Vector3.one * (0.55f + 0.25f * Mathf.PerlinNoise(now * 29f, bi));
                    if (Random.value < 0.6f) Mote(tipPt + Random.insideUnitSphere * 0.15f, fwd * Random.Range(1f, 3f) + Random.insideUnitSphere * 1.2f, new Color(1f, .45f, .35f, 1f));
                }
                B.tipGlow.gameObject.SetActive(!hit);
                // đầu đèn
                B.muzzle.position = B.muzzle2.position = B.muzzleGlow.position = o;
                B.muzzle.rotation = camRot * Quaternion.Euler(0, 0, now * 160f);
                B.muzzle2.rotation = camRot * Quaternion.Euler(0, 0, 45f - now * 240f);
                B.muzzleGlow.rotation = camRot;
                float mp = 0.8f + 0.3f * Mathf.PerlinNoise(now * 31f, bi);
                B.muzzle.localScale = Vector3.one * mp; B.muzzle2.localScale = Vector3.one * mp * 0.7f; B.muzzleGlow.localScale = Vector3.one * mp;
                // xung sáng chạy dọc + đốm năng lượng bay ra
                float span = hit ? len : len + TipLen * 0.6f;
                if (Random.value < dt * 26f)
                {
                    float life = Mathf.Max(0.05f, span / PulseSpeed);
                    pulses.Emit(new ParticleSystem.EmitParams { position = o + fwd * 1.1f, velocity = fwd * PulseSpeed, startLifetime = life, startSize = w * 0.55f, startColor = new Color(1f, .82f, .78f, 1f) }, 1);
                }
                for (int k = 0; k < 2; k++)
                    if (Random.value < dt * 30f)
                        Mote(o + fwd * Random.Range(0.3f, span), (Random.insideUnitSphere + Vector3.up * .4f) * Random.Range(.3f, 1.1f), Random.value < .5f ? new Color(1f, .3f, .22f, 1f) : new Color(1f, .75f, .6f, 1f));
                // trúng đích
                SetImpact(B, hit, hitPoint, hitNormal, fwd, camRot, now, dt, bi);
                lightAt += hit ? hitPoint - fwd * 0.5f : o + fwd * (len * 0.55f);
            }
            glow.transform.position = lightAt * 0.5f + Vector3.up * 0.45f;
            glow.color = anyHit ? new Color(1f, .42f, .2f) : new Color(1f, .2f, .15f);
            glow.range = anyHit ? 9f : 7.5f;
            glow.intensity = (anyHit ? 3.6f : 2.4f) + 1.8f * Mathf.PerlinNoise(now * 19f, 0.7f);
        }

        void SetImpact(Beam B, bool hit, Vector3 p, Vector3 nrm, Vector3 fwd, Quaternion camRot, float now, float dt, int bi)
        {
            B.flare.gameObject.SetActive(hit); B.flare2.gameObject.SetActive(hit); B.flareGlow.gameObject.SetActive(hit); B.scorch.gameObject.SetActive(hit);
            for (int i = 0; i < 3; i++) if (!hit) B.rings[i].gameObject.SetActive(false);
            if (!hit) return;
            var at = p - fwd * 0.06f;
            float j = Mathf.PerlinNoise(now * 27f, bi + 5);
            B.flare.position = at; B.flare.rotation = camRot * Quaternion.Euler(0, 0, now * 260f); B.flare.localScale = Vector3.one * (1.5f + 0.7f * j);
            B.flare2.position = at; B.flare2.rotation = camRot * Quaternion.Euler(0, 0, 22f - now * 380f); B.flare2.localScale = Vector3.one * (0.9f + 0.5f * (1 - j));
            B.flareGlow.position = at; B.flareGlow.rotation = camRot; B.flareGlow.localScale = Vector3.one * (2.3f + 0.6f * j);
            // vết cháy phát sáng áp lên bề mặt
            B.scorch.position = p + nrm * 0.03f; B.scorch.rotation = Quaternion.LookRotation(-nrm) * Quaternion.Euler(0, 0, now * 40f);
            B.scorch.localScale = Vector3.one * (0.9f + 0.25f * j);
            // vòng sóng nở liên tục
            B.nextRing -= dt;
            if (B.nextRing <= 0f)
            {
                B.nextRing = 0.11f;
                int oldest = 0; for (int i = 1; i < 3; i++) if (B.ringAge[i] > B.ringAge[oldest]) oldest = i;
                B.ringAge[oldest] = 0f;
            }
            for (int i = 0; i < 3; i++)
            {
                B.ringAge[i] += dt; float a = B.ringAge[i] / 0.32f;
                bool on = a < 1f; B.rings[i].gameObject.SetActive(on); if (!on) continue;
                B.rings[i].position = at; B.rings[i].rotation = camRot * Quaternion.Euler(0, 0, i * 40f);
                B.rings[i].localScale = Vector3.one * Mathf.Lerp(0.35f, 2.6f, 1 - (1 - a) * (1 - a));
                mpb.Clear(); mpb.SetColor("_Color", new Color(1.7f, 1.2f, 1.1f, 1f) * (1f - a)); B.ringR[i].SetPropertyBlock(mpb);
            }
            // mưa tia lửa nóng chảy văng ngược + đốm than hồng
            int sparks = Mathf.Max(2, Mathf.RoundToInt(dt * 220f));
            for (int k = 0; k < sparks; k++)
            {
                var v = (nrm * 0.7f - fwd * 0.4f + Random.insideUnitSphere * 0.9f + Vector3.up * 0.55f).normalized * Random.Range(4f, 11f);
                molten.Emit(new ParticleSystem.EmitParams { position = at, velocity = v, startLifetime = Random.Range(.3f, .7f), startSize = Random.Range(.08f, .17f), startColor = Color.white }, 1);
            }
            if (Random.value < dt * 40f) Mote(at + Random.insideUnitSphere * .2f, (nrm + Vector3.up + Random.insideUnitSphere) * Random.Range(.6f, 1.8f), new Color(1f, .5f, .2f, 1f));
        }

        void Mote(Vector3 pos, Vector3 vel, Color c) =>
            motes.Emit(new ParticleSystem.EmitParams { position = pos, velocity = vel, startLifetime = Random.Range(.18f, .45f), startSize = Random.Range(.05f, .13f), startColor = c }, 1);

        static void SetLine(LineRenderer lr, Vector3 a, Vector3 b, float w, AnimationCurve curve, Color c)
        {
            lr.SetPosition(0, a); lr.SetPosition(1, b); lr.widthMultiplier = w; lr.widthCurve = curve;
            lr.startColor = c; lr.endColor = c;
        }

        static readonly IComparer<RaycastHit> HitComparer = Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

        // mọi vật trúng đều chặn tia (tia dừng ở vật gần nhất)
        void Apply(Collider c, Vector3 p, Vector3 dir, float dt)
        {
            var rb = c.attachedRigidbody;
            if (rb && traffic)
            {
                int idx = traffic.VehicleIndexOf(rb);
                if (idx >= 0)
                {
                    if (killedThisFrame.Add(idx) && traffic.LaserKill(idx, p, dir)) Hits++;
                    if (hazards) hazards.LaserScorch(p, dir);
                    return;
                }
            }
            var pol = c.GetComponentInParent<OpenWorldPolice>();
            if (pol)
            {
                if (pol.Mode == OpenWorldPolice.State.Chasing || pol.Mode == OpenWorldPolice.State.Reversing) { pol.Stun(1.5f, "trúng lazer"); Hits++; }
                if (hazards) hazards.LaserScorch(p, dir);
                return;
            }
            if (hazards && hazards.LaserHit(c, p, dir, dt)) { Hits++; return; }
            if (hazards) hazards.LaserScorch(p, dir);                              // nhà, cột, cây, mặt đường…
        }

        // ------------------------------------------------------------ dựng hình
        void Build()
        {
            if (root) return;
            mpb = new MaterialPropertyBlock();
            fullCurve = AnimationCurve.Constant(0, 1, 1);
            taperCurve = new AnimationCurve(new Keyframe(0, 0.85f), new Keyframe(0.08f, 1f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0.12f));
            root = new GameObject("TiaLazer").transform;
            Material M(string n) => Resources.Load<Material>("Laser/" + n);
            Material tia = M("PV_Lazer_Tia"), nong = M("PV_Lazer_Nong"), trung = M("PV_Lazer_Trung"), quang = M("PV_Lazer_Quang"),
                     ngon = M("PV_Lazer_Ngon"), vong = M("PV_Lazer_Vong"), hat = M("PV_Lazer_Hat"), tiaLua = M("PV_Lazer_TiaLua");
            for (int bi = 0; bi < 2; bi++)
            {
                var B = beams[bi];
                B.halo = Line("QuangTia" + bi, quang); B.body = Line("Tia" + bi, tia); B.core = Line("LoiTia" + bi, quang); B.tip = Line("NgonTia" + bi, ngon);
                B.muzzleGlow = Quad("QuangDenPha" + bi, hat, 1.1f, new Color(1f, .25f, .2f));
                B.muzzle = Quad("ChopDenPha" + bi, nong, 0.95f, Color.white);
                B.muzzle2 = Quad("ChopDenPha2_" + bi, nong, 0.95f, new Color(1f, .8f, .75f));
                B.flareGlow = Quad("QuangTrung" + bi, hat, 1f, new Color(1f, .4f, .22f));
                B.flare = Quad("ChopTrung" + bi, trung, 1f, Color.white);
                B.flare2 = Quad("ChopTrung2_" + bi, nong, 1f, new Color(1f, .9f, .8f));
                B.scorch = Quad("VetChay" + bi, hat, 1f, new Color(1f, .5f, .18f));
                B.tipGlow = Quad("ChamNgon" + bi, hat, 1f, new Color(1f, .55f, .5f));
                for (int i = 0; i < 3; i++) { B.rings[i] = Quad("VongSong" + bi + "_" + i, vong, 1f, Color.white); B.ringR[i] = B.rings[i].GetComponentInChildren<Renderer>(); }
            }
            pulses = Particles("XungNangLuong", hat, 120, 0f, true, 0.028f);
            motes = Particles("DomNangLuong", hat, 260, -0.15f, false, 0f);
            molten = Particles("TiaLuaNongChay", tiaLua, 600, 1.6f, true, 0.06f);
            var lg = new GameObject("DenLazer"); lg.transform.SetParent(root, false);
            glow = lg.AddComponent<Light>(); glow.type = LightType.Point; glow.color = new Color(1f, .22f, .16f); glow.range = 7.5f;
            glow.shadows = LightShadows.None; glow.renderMode = LightRenderMode.ForcePixel;
            hum = PhoVietAudio.Route(gameObject.AddComponent<AudioSource>());
            hum.loop = true; hum.playOnAwake = false; hum.spatialBlend = 0.3f; hum.volume = 0.45f; hum.clip = HumClip();
            root.gameObject.SetActive(false);
        }

        LineRenderer Line(string name, Material m)
        {
            var lr = new GameObject(name).AddComponent<LineRenderer>(); lr.transform.SetParent(root, false);
            lr.useWorldSpace = true; lr.positionCount = 2; lr.sharedMaterial = m; lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch; lr.numCapVertices = 0; Quiet(lr);
            return lr;
        }

        Transform Quad(string name, Material m, float size, Color tint)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad); Destroy(q.GetComponent<Collider>());
            q.name = name;
            var holder = new GameObject(name + "_Goc").transform; holder.SetParent(root, false);
            q.transform.SetParent(holder, false); q.transform.localScale = Vector3.one * size;
            var r = q.GetComponent<MeshRenderer>(); r.sharedMaterial = m; Quiet(r);
            if (tint != Color.white && m) { var p = new MaterialPropertyBlock(); p.SetColor("_Color", m.GetColor("_Color") * tint); r.SetPropertyBlock(p); }
            return holder;
        }

        ParticleSystem Particles(string name, Material m, int max, float gravity, bool stretch, float velScale)
        {
            var go = new GameObject(name); go.transform.SetParent(root, false);
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.playOnAwake = false; main.maxParticles = max; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = gravity; main.startSpeed = 0f;
            var em = ps.emission; em.enabled = false; var sh = ps.shape; sh.enabled = false;
            var col = ps.colorOverLifetime; col.enabled = true; var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(.8f, .6f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = m; Quiet(r);
            if (stretch) { r.renderMode = ParticleSystemRenderMode.Stretch; r.velocityScale = velScale; r.lengthScale = 1.6f; }
            ps.Play(); return ps;
        }

        static void Quiet(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off; r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        static AudioClip HumClip()
        {
            const int hz = 22050; var w = new float[hz]; var r = new System.Random(300); float ph1 = 0, ph2 = 0;
            for (int i = 0; i < w.Length; i++)
            {
                float t = (float)i / hz;
                float f = 180f + 6f * Mathf.Sin(t * 2 * Mathf.PI * 6f);                    // 6 vòng rung / giây → lặp liền 1 s
                ph1 += f / hz; ph2 += f * 2.01f / hz;
                float saw = (ph1 % 1f) * 2f - 1f, saw2 = (ph2 % 1f) * 2f - 1f;
                float crackle = (float)(r.NextDouble() * 2 - 1) * 0.12f;
                w[i] = (saw * 0.32f + saw2 * 0.18f + Mathf.Sin(t * 2 * Mathf.PI * 1200f) * 0.06f + crackle) * 0.7f;
            }
            var clip = AudioClip.Create("Lazer ù", w.Length, 1, hz, false); clip.SetData(w, 0); return clip;
        }

        void OnDestroy() { if (root) Destroy(root.gameObject); }
    }
}
