using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.312 — người dùng 22/11/2026: nút «KHIÊN» (trên giữa 2 nút Trái/Phải, cỡ nút TÊN LỬA mới). Bấm → khiên năng lượng 6 s (21.313):
    // • Miễn nhiễm mọi va chạm: húc xe NPC/xe tăng/khủng long/CSGT không văng, không choáng, không nổ — chỉ giảm tốc.
    // • Miễn nhiễm mọi đạn: đạn pháo xe tăng/tàu, tên lửa trực thăng/máy bay cánh kép (OpenWorldHazards.Explode → Absorb).
    // • Phá vỡ lưới điện CSGT: đang dính lưới thì bật khiên là lưới vỡ tung (OpenWorldNetShatter), lưới bắn tới khi khiên đang bật
    //   cũng vỡ ngay khi chạm khiên; xe chạy tiếp được.
    // • Hết 6 s → hồi 10 s mới dùng lại (21.313; 21.312 là 4 s / 15 s). Phím K trên máy tính. Vào lượt mới: sẵn sàng.
    // Hình: lưới Blender PV_Khien.fbx (build_khien.py): vỏ elip fresnel + 162 ô lục giác (khung sáng + mặt ô lóe ngẫu nhiên) + vành
    // sáng dưới đất, shader cộng sáng PhoViet/OpenWorld/Shield; 12 TIA NĂNG LƯỢNG (6 mẫu tia sét có nhánh) xẹt quanh mặt khiên, đổi
    // chỗ mỗi 0,055 s, một phần phóng chĩa ra ngoài; tia lửa ở đầu tia; đèn điểm xanh ngọc nhấp nháy; tiếng ù năng lượng + xẹt.
    // Bật: khiên nở 0,55 → 1 trong 0,22 s + chớp; 0,9 s cuối nháy báo sắp tắt; bị trúng: gợn sóng lan từ chỗ trúng + tóe tia.
    public class OpenWorldShield : MonoBehaviour
    {
        public const float Duration = 6f, Cooldown = 10f;           // 21.313 (người dùng): 4 → 6 s, hồi 15 → 10 s
        public float ActiveLeft { get; private set; }
        public float CooldownLeft { get; private set; }
        public bool Active => ActiveLeft > 0f;
        public bool Ready => !Active && CooldownLeft <= 0f;
        public int Uses { get; private set; }
        public int Blocks { get; private set; }
        public static int NetsBroken;
        public float ShownAlpha => shown;
        [System.NonSerialized] public bool requested;

        static readonly Vector3 Semi = new Vector3(1.55f, 1.30f, 2.65f), Center = new Vector3(0, 0.55f, 0);
        static readonly int AlphaId = Shader.PropertyToID("_Alpha"), HitId = Shader.PropertyToID("_Hit"), ColorId = Shader.PropertyToID("_Color");
        const int BoltCount = 12;

        OpenWorldDriver driver; OpenWorldGame game;
        Transform root, bubble; readonly List<Renderer> shellR = new List<Renderer>();
        readonly List<Transform> bolts = new List<Transform>(); readonly List<MeshFilter> boltMf = new List<MeshFilter>(); readonly List<Renderer> boltR = new List<Renderer>();
        readonly List<Mesh> boltMeshes = new List<Mesh>();
        Renderer ringR; MaterialPropertyBlock mpb; Material boltMat, dotMat; Color boltC;
        ParticleSystem sparks; Light glow; AudioSource hum, sfx; AudioClip upClip, blockClip;
        float shown, nextBolt, hitAt = -99f, startAt = -99f; Vector3 hitDir = Vector3.forward;
        readonly System.Random rnd = new System.Random(312);

        void Awake() { driver = GetComponent<OpenWorldDriver>(); }

        public void ResetShield()
        {
            ActiveLeft = 0f; CooldownLeft = 0f; shown = 0f; requested = false;
            if (root) root.gameObject.SetActive(false);
            if (hum) hum.Stop();
        }

        bool CanUse()
        {
            if (!game) game = FindAnyObjectByType<OpenWorldGame>();
            if (game && game.Mode != OpenWorldGame.State.Riding) return false;
            return driver && !driver.frozen && !driver.Crashed && !driver.Sinking && Time.timeScale > 0f;
        }

        public bool TryActivate()
        {
            if (!Ready || !CanUse()) return false;
            ActiveLeft = Duration; Uses++; startAt = Time.time; hitAt = -99f;
            Build(); root.gameObject.SetActive(true); Pose();
            if (driver.Netted) driver.ClearNet();
            foreach (var n in OpenWorldPoliceNet.All) if (n) n.ShatterIfOnPlayer();
            if (sfx && upClip) sfx.PlayOneShot(upClip, 0.8f);
            if (hum) { hum.volume = 0f; hum.Play(); }
            Burst(70, Vector3.zero);
            return true;
        }

        // Khiên chặn một cú trúng (đạn/tên lửa/va chạm) tại điểm thế giới; strength 0..1.
        public void Absorb(Vector3 worldPoint, float strength)
        {
            Blocks++; hitAt = Time.time;
            if (root)
            {
                var c = root.TransformPoint(Center); var d = worldPoint - c;
                if (d.sqrMagnitude < 0.01f) d = root.forward;
                hitDir = Quaternion.Inverse(root.rotation) * d.normalized;
                Burst(Mathf.RoundToInt(25 + 45 * Mathf.Clamp01(strength)), root.TransformPoint(Center + Vector3.Scale(Unit(hitDir), Semi)));
            }
            if (sfx && blockClip) sfx.PlayOneShot(blockClip, 0.55f + 0.4f * Mathf.Clamp01(strength));
        }

        // hướng (hệ khiên, tính từ tâm) → vector đơn vị u sao cho Center + Scale(u, Semi) nằm trên mặt khiên theo đúng hướng đó
        static Vector3 Unit(Vector3 d)
        {
            var q = new Vector3(d.x / Semi.x, d.y / Semi.y, d.z / Semi.z); float m = q.magnitude;
            return m < 1e-5f ? Vector3.forward : new Vector3(q.x / m, q.y / m, q.z / m);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.K)) requested = true;
            if (requested) { requested = false; TryActivate(); }
            if (Active)
            {
                ActiveLeft -= dt;
                if (ActiveLeft <= 0f || (driver && (driver.Sinking || driver.Crashed))) { ActiveLeft = 0f; CooldownLeft = Cooldown; }
            }
            else if (CooldownLeft > 0f) CooldownLeft = Mathf.Max(0f, CooldownLeft - dt);
        }

        void Pose()
        {
            var t = transform; root.SetPositionAndRotation(t.position, Quaternion.Euler(0f, t.eulerAngles.y, 0f));
        }

        void LateUpdate()
        {
            if (!root || !root.gameObject.activeSelf) return;
            float dt = Time.deltaTime, now = Time.time;
            shown = Mathf.MoveTowards(shown, Active ? 1f : 0f, dt / (Active ? 0.16f : 0.3f));
            if (shown <= 0f && !Active) { root.gameObject.SetActive(false); if (hum) hum.Stop(); return; }
            Pose();
            float grow = Mathf.Clamp01((now - startAt) / 0.22f); grow = 1f - (1f - grow) * (1f - grow) * (1f - grow);
            float scale = Mathf.Lerp(0.55f, 1f, grow) * (1f + 0.012f * Mathf.Sin(now * 9f));
            bubble.localScale = Vector3.one * scale;
            float blink = Active && ActiveLeft < 0.9f ? (Mathf.Sin(now * 34f) > 0f ? 1f : 0.35f) : 1f;
            float flash = (1f - grow) * 1.6f;
            float a = shown * blink + flash;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            var hit = new Vector4(hitDir.x, hitDir.y, hitDir.z, now - hitAt);
            foreach (var r in shellR) { r.GetPropertyBlock(mpb); mpb.SetFloat(AlphaId, a); mpb.SetVector(HitId, hit); r.SetPropertyBlock(mpb); }
            ringR.GetPropertyBlock(mpb); mpb.SetFloat(AlphaId, a); ringR.SetPropertyBlock(mpb);
            // tia năng lượng xẹt quanh mặt khiên
            if (now >= nextBolt)
            {
                nextBolt = now + 0.055f;
                for (int i = 0; i < bolts.Count; i++)
                {
                    bool on = Rand() < 0.72f && shown > 0.3f;
                    boltR[i].enabled = on; if (!on) continue;
                    var d = new Vector3(Rand() * 2 - 1, Rand() * 1.3f - 0.25f, Rand() * 2 - 1); if (d.sqrMagnitude < 0.01f) d = Vector3.up;
                    var u = d.normalized;
                    var p = Center + Vector3.Scale(u, Semi) * scale * 1.01f;
                    var nrm = new Vector3(u.x / Semi.x, u.y / Semi.y, u.z / Semi.z).normalized;
                    var tg = Vector3.Cross(nrm, Random.onUnitSphere); if (tg.sqrMagnitude < 1e-4f) tg = Vector3.Cross(nrm, Vector3.right); tg.Normalize();
                    float outward = i % 3 == 0 ? Mathf.Lerp(0.6f, 1.4f, Rand()) : Mathf.Lerp(0.05f, 0.35f, Rand());
                    var dir = (tg + nrm * outward).normalized;
                    float len = Mathf.Lerp(0.55f, 1.35f, Rand()) * (i % 3 == 0 ? 0.8f : 1f);
                    boltMf[i].sharedMesh = boltMeshes[rnd.Next(boltMeshes.Count)];
                    bolts[i].localPosition = p;
                    bolts[i].localRotation = Quaternion.LookRotation(-dir, nrm);   // lưới tia chạy từ gốc theo −Z
                    bolts[i].localScale = new Vector3(2.4f, 2.4f, len);
                    if (sparks && Rand() < 0.5f) Spark(root.TransformPoint(p + dir * len), root.rotation * (dir * 2.5f + nrm), 1);
                }
                boltMat.SetColor(ColorId, boltC * (0.85f + 0.3f * Rand()) * Mathf.Clamp01(shown * blink));
            }
            if (glow) { glow.intensity = (1.6f + 1.1f * Rand() + flash * 3f) * shown * blink; glow.transform.position = root.TransformPoint(Center + Vector3.up * 0.4f); }
            if (hum) hum.volume = 0.32f * shown;
        }

        float Rand() => (float)rnd.NextDouble();

        void Spark(Vector3 at, Vector3 vel, int n)
        {
            for (int k = 0; k < n; k++)
                sparks.Emit(new ParticleSystem.EmitParams { position = at, velocity = vel + Random.insideUnitSphere * 2.5f, startSize = Random.Range(.06f, .16f), startLifetime = Random.Range(.15f, .4f), startColor = Color.white }, 1);
        }

        void Burst(int n, Vector3 at)
        {
            if (!sparks || !root) return;
            for (int k = 0; k < n; k++)
            {
                var u = Random.onUnitSphere; u.y = Mathf.Abs(u.y) * 0.9f + 0.05f;
                var p = at == Vector3.zero ? root.TransformPoint(Center + Vector3.Scale(u.normalized, Semi)) : at + Random.insideUnitSphere * 0.25f;
                sparks.Emit(new ParticleSystem.EmitParams { position = p, velocity = root.rotation * (u * Random.Range(2.5f, 7f)), startSize = Random.Range(.08f, .22f), startLifetime = Random.Range(.2f, .55f), startColor = Color.white }, 1);
            }
        }

        // ------------------------------------------------------------ dựng (một lần)
        void Build()
        {
            if (root) return;
            root = new GameObject("KhienNangLuong").transform;
            bubble = new GameObject("BongKhien").transform; bubble.SetParent(root, false);
            var model = Resources.Load<GameObject>("Khien/PV_Khien");
            var mats = new Dictionary<string, Material>
            {
                { "Khien_Vo", Mat("PV_Khien_Vo") }, { "Khien_Khung", Mat("PV_Khien_Khung") }, { "Khien_O", Mat("PV_Khien_O") }, { "Khien_Vanh", Mat("PV_Khien_Vanh") }
            };
            boltMat = new Material(Mat("PV_Khien_Tia")) { name = "PV_Khien_Tia (bản)" }; boltC = boltMat.GetColor(ColorId);
            dotMat = Mat("PV_Khien_Hat");
            if (model)
                foreach (var mf in model.GetComponentsInChildren<MeshFilter>(true))
                {
                    var name = mf.sharedMesh.name;
                    if (name.StartsWith("Khien_Tia")) { boltMeshes.Add(mf.sharedMesh); continue; }
                    if (!mats.TryGetValue(name, out var m)) continue;
                    var go = new GameObject(name); go.transform.SetParent(name == "Khien_Vanh" ? root : bubble, false);
                    go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                    var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = m; Quiet(r);
                    if (name == "Khien_Vanh") ringR = r; else shellR.Add(r);
                }
            if (!ringR) { var g = new GameObject("Khien_Vanh"); g.transform.SetParent(root, false); ringR = g.AddComponent<MeshRenderer>(); g.AddComponent<MeshFilter>(); }
            for (int i = 0; i < BoltCount && boltMeshes.Count > 0; i++)
            {
                var g = new GameObject("TiaNangLuong" + i); g.transform.SetParent(bubble, false);
                var mf = g.AddComponent<MeshFilter>(); mf.sharedMesh = boltMeshes[i % boltMeshes.Count];
                var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = boltMat; Quiet(r);
                bolts.Add(g.transform); boltMf.Add(mf); boltR.Add(r);
            }
            // tia lửa năng lượng
            var sg = new GameObject("TiaLuaKhien"); sg.transform.SetParent(root, false);
            sparks = sg.AddComponent<ParticleSystem>(); sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var mm = sparks.main; mm.loop = false; mm.playOnAwake = false; mm.maxParticles = 500; mm.simulationSpace = ParticleSystemSimulationSpace.World; mm.gravityModifier = 0.6f;
            var em = sparks.emission; em.enabled = false; var sh = sparks.shape; sh.enabled = false;
            var col = sparks.colorOverLifetime; col.enabled = true; var gr = new Gradient();
            gr.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(.3f, 1f, .85f), 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }); col.color = gr;
            var pr = sg.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = dotMat; pr.renderMode = ParticleSystemRenderMode.Stretch; pr.velocityScale = 0.05f; pr.lengthScale = 1.4f;
            pr.shadowCastingMode = ShadowCastingMode.Off; pr.receiveShadows = false;
            sparks.Play();
            // đèn điểm xanh ngọc
            var lg = new GameObject("DenKhien"); lg.transform.SetParent(root, false);
            glow = lg.AddComponent<Light>(); glow.type = LightType.Point; glow.color = new Color(.25f, 1f, .85f); glow.range = 7f;
            glow.shadows = LightShadows.None; glow.renderMode = LightRenderMode.ForcePixel; glow.intensity = 0f;
            // âm thanh
            hum = PhoVietAudio.Route(root.gameObject.AddComponent<AudioSource>());
            hum.loop = true; hum.playOnAwake = false; hum.spatialBlend = 0.4f; hum.minDistance = 8f; hum.maxDistance = 80f; hum.clip = HumClip();
            sfx = PhoVietAudio.Route(root.gameObject.AddComponent<AudioSource>());
            sfx.playOnAwake = false; sfx.spatialBlend = 0.3f; sfx.minDistance = 8f; sfx.maxDistance = 80f;
            upClip = UpClip(); blockClip = BlockClip();
            root.gameObject.SetActive(false);
        }

        static Material Mat(string name)
        {
            var m = Resources.Load<Material>("Khien/" + name);
            return m ? m : new Material(Shader.Find("PhoViet/OpenWorld/Electric"));
        }

        static void Quiet(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off; r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        // ------------------------------------------------------------ âm thanh tự tổng hợp
        static AudioClip HumClip()
        {
            const int hz = 22050; var w = new float[hz * 2]; var r = new System.Random(31); float crack = 0;
            for (int i = 0; i < w.Length; i++)
            {
                float t = (float)i / hz;
                if (r.NextDouble() < 0.0011) crack = 1f;
                crack *= 0.996f;
                float hum = Mathf.Sin(t * 2 * Mathf.PI * 120f) * 0.45f + Mathf.Sin(t * 2 * Mathf.PI * 240f) * 0.22f + Mathf.Sin(t * 2 * Mathf.PI * 723f) * 0.06f * (0.5f + 0.5f * Mathf.Sin(t * 2 * Mathf.PI * 3f));
                float noise = (float)(r.NextDouble() * 2 - 1);
                w[i] = Mathf.Clamp(hum * 0.2f + noise * 0.45f * crack, -1f, 1f);
            }
            var clip = AudioClip.Create("Khiên ù", w.Length, 1, hz, false); clip.SetData(w, 0); return clip;
        }
        static AudioClip UpClip()
        {
            const int hz = 22050; int n = (int)(hz * 0.55f); var w = new float[n]; var r = new System.Random(5); float ph = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / hz, f = Mathf.Lerp(180f, 1250f, Mathf.Pow(t / 0.55f, 0.6f));
                ph += 2 * Mathf.PI * f / hz;
                float env = Mathf.Min(1f, t * 30f) * Mathf.Exp(-t * 3.5f);
                w[i] = (Mathf.Sin(ph) * 0.5f + Mathf.Sin(ph * 1.5f) * 0.2f + (float)(r.NextDouble() * 2 - 1) * 0.25f * Mathf.Exp(-t * 12f)) * env;
            }
            var clip = AudioClip.Create("Khiên bật", n, 1, hz, false); clip.SetData(w, 0); return clip;
        }
        static AudioClip BlockClip()
        {
            const int hz = 22050; int n = (int)(hz * 0.32f); var w = new float[n]; var r = new System.Random(9);
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / hz, env = Mathf.Exp(-t * 14f);
                w[i] = Mathf.Clamp((Mathf.Sin(t * 2 * Mathf.PI * 520f * (1 - t)) * 0.45f + (float)(r.NextDouble() * 2 - 1) * 0.6f) * env, -1f, 1f);
            }
            var clip = AudioClip.Create("Khiên chặn", n, 1, hz, false); clip.SetData(w, 0); return clip;
        }

        void OnDestroy()
        {
            if (root) Destroy(root.gameObject);
            if (boltMat) Destroy(boltMat);
        }
    }
}
