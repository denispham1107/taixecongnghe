using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.317 — người dùng 22/11/2026: thời tiết ngẫu nhiên mỗi lần bắt đầu game / CHƠI LẠI (THỬ MỘT CHUYẾN NỮA) / VỀ ĐẦU PHỐ:
    // 50 % trời thường, 50 % mưa giông. Đang trời thường mà bấm VỀ ĐẦU PHỐ (hoặc chơi lại) ra mưa → chuyển nhanh sang mưa (2 s).
    // • Mưa kéo dài 2 phút lái (đếm khi đang chạy; màn chào/tạm dừng/kết thúc không trừ), rồi tạnh dần trong 20 s về trời thường.
    // • Khi mưa to: cứ mỗi đợt 7 tia sét đánh ĐỒNG THỜI xuống đường phố, lề đường, lề sông, lề biển quanh tầm nhìn người chơi;
    //   1 s trước có vòng cảnh báo xanh điện dưới đất. Đánh xong nghỉ 15 s rồi đánh đợt tiếp.
    //   Một tia nhắm lối xe đang chạy (~1,25 s phía trước) để phải lách/phanh. Sét có hại (OpenWorldHazards.LightningBlast):
    //   trúng xe người chơi → cháy nổ hết lượt (KHIÊN chặn được); xe NPC → cháy rồi nổ; CSGT khựng. Màn chào/kết thúc: sét
    //   chỉ đánh xa xe người chơi, không gây hại cho người chơi.
    // • Hình ảnh dựng bằng Blender (ArtSource/Blender/OpenWorld/build_mua_set.py → Resources/ThoiTiet):
    //   hạt mưa (vệt kéo dài, ~3.600 hạt/s quanh camera), 2 ống màn mưa cuộn quanh camera (mưa xa dày), giọt bắn vương miện
    //   trên mặt đường/nóc xe, 8 tia sét phân nhánh cao 150 m (shader PhoViet/OpenWorld/Lightning nở theo hướng nhìn), vết cháy
    //   sém nơi sét đánh (mờ dần 9 s).
    // • Ánh sáng: trời tối xám (bầu trời, mặt trời, ánh sáng môi trường, sương mù gần lại); mỗi lần sét: tia dẫn đường đi xuống
    //   0,06 s → chớp chính + 2 lần tái phóng (0,14 s, 0,30 s) làm sáng cả bầu trời, mặt đất (1 đèn điểm ở tia gần camera),
    //   tia lửa + khói ở chân sét; sấm: tiếng nổ chát ở tia gần + tiếng rền ở tia xa, trễ theo khoảng cách (340 m/s).
    [DefaultExecutionOrder(130)]
    public class OpenWorldWeather : MonoBehaviour
    {
        public const float RainSeconds = 120f, EndFade = 20f, QuickFade = 2f, VolleyPause = 15f, WarnSeconds = 1f, StrikeRadius = 3f;
        public const int VolleySize = 7;
        [Range(0, 1)] public float rainChance = 0.5f;
        [Tooltip("Kiểm thử: lần tung tiếp theo ép ra mưa (true) / trời thường (false)")] public static bool? ForceNext;

        public bool Raining { get; private set; }
        public float Wet { get; private set; }
        public float RainTime { get; private set; }
        public float Flash { get; private set; }
        public int Rolls { get; private set; }
        public int RainRolls { get; private set; }
        public int Volleys { get; private set; }
        public int Strikes { get; private set; }
        public float NextVolleyIn => nextVolley;
        public bool VolleyActive => volleyOn;
        public int ParticleCount => rain ? rain.particleCount : 0;
        public readonly List<string> LastKinds = new List<string>();
        public readonly List<Vector3> LastTargets = new List<Vector3>();

        OpenWorldGame game; OpenWorldDriver player; OpenWorldHazards hazards; OpenWorldTraffic traffic; Rigidbody playerRb;
        float fadeSeconds = QuickFade, nextVolley, volleyClock, toastAt = -99f, toastUntil = -99f; bool volleyOn, built;
        string toastTitle, toastDetail; OpenWorldToast.Kind toastKind; int rumbles;

        // ------------------------------------------------------------ tung thời tiết
        public void Roll(bool instant)
        {
            Rolls++;
            bool wet = ForceNext ?? (Random.value < rainChance); ForceNext = null;
            if (wet) RainRolls++;
            Raining = wet; RainTime = 0f; fadeSeconds = QuickFade;
            if (instant) Wet = wet ? 1f : 0f;
            nextVolley = 3f; CancelVolley();
        }
        // Bắt đầu lượt lái: đồng hồ 2 phút mưa tính lại từ đầu.
        public void BeginRun()
        {
            RainTime = 0f; nextVolley = 3.5f; CancelVolley();
            if (Raining) Toast("Trời đổ mưa giông", "Sét đánh từng đợt 7 tia · né vòng xanh", 3f, OpenWorldToast.Kind.Warn);
        }

        void Awake() { game = GetComponent<OpenWorldGame>(); }

        void Start()
        {
            if (!game) game = FindAnyObjectByType<OpenWorldGame>();
            player = game ? game.player : FindAnyObjectByType<OpenWorldDriver>();
            hazards = game && game.hazards ? game.hazards : FindAnyObjectByType<OpenWorldHazards>();
            traffic = game && game.traffic ? game.traffic : FindAnyObjectByType<OpenWorldTraffic>();
            if (player) playerRb = player.GetComponent<Rigidbody>();
            Build();
        }

        void Update()
        {
            if (!built) return;
            float dt = Time.deltaTime;
            bool riding = game && game.Mode == OpenWorldGame.State.Riding;
            if (dt > 0f)
            {
                if (Raining && riding)
                {
                    RainTime += dt;
                    if (RainTime >= RainSeconds)
                    {   // hết 2 phút: tạnh dần
                        Raining = false; fadeSeconds = EndFade; CancelVolley();
                        Toast("Trời tạnh mưa", "Hết giông sét", 2.4f, OpenWorldToast.Kind.Good);
                    }
                }
                Wet = Mathf.MoveTowards(Wet, Raining ? 1f : 0f, dt / Mathf.Max(0.1f, fadeSeconds));
                if (Raining && Wet > 0.85f && !volleyOn && (riding || !game || game.Mode != OpenWorldGame.State.Paused))
                {
                    nextVolley -= dt;
                    if (nextVolley <= 0f) StartVolley();
                }
                TickVolley(dt);
                TickRain(dt);
            }
            Apply();
        }

        void LateUpdate()
        {
            if (!built) return;
            var cam = Camera.main; if (!cam) return;
            var cp = cam.transform.position;
            if (tubes) tubes.position = cp;
            if (audioRoot) audioRoot.position = cp;
        }

        // ------------------------------------------------------------ dựng
        Transform root, tubes, audioRoot;
        ParticleSystem rain, splash, sparks, smoke, glints;
        Material domeMat, rainMat, splashMat, curtainNear, curtainFar, boltMat, scorchMat, warnMat, sparkMat;
        Mesh[] boltMeshes; Mesh warnMesh;
        Light sun, boltLight; Material sky; bool skyOwned;
        float baseSun, baseShadow, baseAmb, baseRefl, baseFogStart, baseFogEnd, baseExposure, baseThick, baseSunSize;
        Color baseSunCol, baseFog, baseSkyTint, baseGround;
        AudioSource rainAudio; readonly List<AudioSource> thunder = new List<AudioSource>(); AudioClip crackClip, rumbleClip;

        static Material Copy(string path) { var m = Resources.Load<Material>(path); return m ? new Material(m) : null; }

        void Build()
        {
            if (built) return;
            root = new GameObject("ThoiTiet").transform; root.SetParent(transform, false);
            rainMat = Copy("ThoiTiet/PV_Mua_Hat"); splashMat = Copy("ThoiTiet/PV_Mua_Toe"); scorchMat = Copy("ThoiTiet/PV_Set_VetChay");
            curtainNear = Copy("ThoiTiet/PV_Mua_Man"); curtainFar = Copy("ThoiTiet/PV_Mua_Man"); boltMat = Copy("ThoiTiet/PV_Set_Tia");
            sparkMat = Copy("ThoiTiet/PV_Set_Hat");
            if (hazards && hazards.warningMaterial) warnMat = new Material(hazards.warningMaterial);
            if (!rainMat || !boltMat) { Debug.LogWarning("[Phố Việt] thiếu vật liệu Resources/ThoiTiet"); return; }

            // hạt mưa: vệt kéo dài theo vận tốc (+ vận tốc camera → xe chạy nhanh thấy mưa xiên)
            rain = NewPs("MuaHat", rainMat, 4200, ParticleSystemRenderMode.Stretch);
            var r = rain.GetComponent<ParticleSystemRenderer>(); r.velocityScale = 0.045f; r.lengthScale = 2f; r.cameraVelocityScale = 0.025f;
            var vel = rain.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(1.6f, 2.8f); vel.y = new ParticleSystem.MinMaxCurve(-27f, -23f); vel.z = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            // giọt bắn vương miện (4 khung) dựng đứng quay về camera
            splash = NewPs("MuaToe", splashMat, 500, ParticleSystemRenderMode.VerticalBillboard);
            var sheet = splash.textureSheetAnimation; sheet.enabled = true; sheet.numTilesX = 4; sheet.numTilesY = 1;
            sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, .999f));
            // chân sét: tia lửa điện, chớp sáng tại chỗ, khói
            sparks = NewPs("SetTiaLua", sparkMat, 600, ParticleSystemRenderMode.Billboard);
            var sm = sparks.main; sm.gravityModifier = 1.2f;
            glints = NewPs("SetChop", sparkMat, 60, ParticleSystemRenderMode.Billboard);
            var jets = player ? player.GetComponent<OpenWorldJumpJets>() : null;
            var smokeM = jets && jets.smokeMaterial ? jets.smokeMaterial : null;
            if (smokeM)
            {
                smoke = NewPs("SetKhoi", smokeM, 200, ParticleSystemRenderMode.Billboard);
                var ss = smoke.textureSheetAnimation; ss.enabled = true; ss.numTilesX = jets.sheet.x; ss.numTilesY = jets.sheet.y;
                ss.animation = ParticleSystemAnimationType.WholeSheet; ss.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, .999f));
                var sz = smoke.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, .5f, 1, 2.2f));
            }

            // 2 ống màn mưa quanh camera
            var meshGo = Resources.Load<GameObject>("ThoiTiet/PV_Mua");
            tubes = new GameObject("MuaMan").transform; tubes.SetParent(root, false);
            if (meshGo)
                foreach (var mf in meshGo.GetComponentsInChildren<MeshFilter>(true))
                {
                    var go = new GameObject(mf.name); go.transform.SetParent(tubes, false);
                    go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                    var mr = go.AddComponent<MeshRenderer>(); bool near = mf.name.Contains("Gan");
                    mr.sharedMaterial = near ? curtainNear : curtainFar; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                    mr.lightProbeUsage = LightProbeUsage.Off; mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    mr.localBounds = new Bounds(Vector3.zero, Vector3.one * 90f);
                }
            // vòm mây giông (cầu r 500 m theo camera): che trời xanh bằng chân mây xám đen cuộn, sát chân trời = màu sương mù
            if (rainMat)
            {
                domeMat = new Material(rainMat.shader); domeMat.SetFloat("_Mode", 3); domeMat.SetColor("_Color", new Color(.2f, .22f, .26f, 1f));
                domeMat.renderQueue = 2950;
                var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(dome.GetComponent<Collider>());
                dome.name = "VomMay"; dome.transform.SetParent(tubes, false); dome.transform.localScale = Vector3.one * 1000f;
                var dr = dome.GetComponent<MeshRenderer>(); dr.sharedMaterial = domeMat; dr.shadowCastingMode = ShadowCastingMode.Off; dr.receiveShadows = false;
                dr.lightProbeUsage = LightProbeUsage.Off; dr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
            if (curtainNear) { curtainNear.SetFloat("_Speed", 3.6f); curtainNear.mainTextureScale = new Vector2(1f, 1f); }
            if (curtainFar) { curtainFar.SetFloat("_Speed", 2.4f); curtainFar.mainTextureScale = new Vector2(1f, 1f); }

            // tia sét
            var boltGo = Resources.Load<GameObject>("ThoiTiet/PV_Set");
            var list = new List<Mesh>();
            if (boltGo) foreach (var mf in boltGo.GetComponentsInChildren<MeshFilter>(true)) list.Add(mf.sharedMesh);
            boltMeshes = list.ToArray();
            warnMesh = new Mesh { name = "OW_VongSet" };
            warnMesh.vertices = new[] { new Vector3(-.5f, 0, -.5f), new Vector3(.5f, 0, -.5f), new Vector3(.5f, 0, .5f), new Vector3(-.5f, 0, .5f) };
            warnMesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up }; warnMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            warnMesh.RecalculateNormals(); warnMesh.RecalculateBounds();
            for (int i = 0; i < VolleySize; i++) strikes.Add(NewStrike(i));

            boltLight = new GameObject("AnhSet").AddComponent<Light>(); boltLight.transform.SetParent(root, false);
            boltLight.type = LightType.Point; boltLight.range = 60f; boltLight.color = new Color(.74f, .82f, 1f); boltLight.intensity = 0f;
            boltLight.shadows = LightShadows.None; boltLight.renderMode = LightRenderMode.ForcePixel; boltLight.enabled = false;

            // ánh sáng gốc của cảnh
            foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Exclude)) if (l.type == LightType.Directional) { sun = l; break; }
            if (sun) { baseSun = sun.intensity; baseSunCol = sun.color; baseShadow = sun.shadowStrength; }
            baseAmb = RenderSettings.ambientIntensity; baseRefl = RenderSettings.reflectionIntensity;
            baseFog = RenderSettings.fogColor; baseFogStart = RenderSettings.fogStartDistance; baseFogEnd = RenderSettings.fogEndDistance;
            if (RenderSettings.skybox)
            {
                sky = new Material(RenderSettings.skybox); skyOwned = true; RenderSettings.skybox = sky;
                baseExposure = Get(sky, "_Exposure", 1.3f); baseThick = Get(sky, "_AtmosphereThickness", 1f); baseSunSize = Get(sky, "_SunSize", .04f);
                baseSkyTint = sky.HasProperty("_SkyTint") ? sky.GetColor("_SkyTint") : Color.gray;
                baseGround = sky.HasProperty("_GroundColor") ? sky.GetColor("_GroundColor") : baseFog;
            }

            // âm thanh: tiếng mưa (2D) + 4 nguồn sấm
            audioRoot = new GameObject("AmThanhThoiTiet").transform; audioRoot.SetParent(root, false);
            rainAudio = PhoVietAudio.Route(audioRoot.gameObject.AddComponent<AudioSource>());
            rainAudio.clip = RainClip(); rainAudio.loop = true; rainAudio.playOnAwake = false; rainAudio.volume = 0f; rainAudio.spatialBlend = 0f;
            crackClip = ThunderClip(true); rumbleClip = ThunderClip(false);
            for (int i = 0; i < 4; i++)
            {
                var g = new GameObject("Sam" + i); g.transform.SetParent(root, false);
                var a = PhoVietAudio.Route(g.AddComponent<AudioSource>());
                a.playOnAwake = false; a.spatialBlend = 0.55f; a.minDistance = 40f; a.maxDistance = 900f; a.rolloffMode = AudioRolloffMode.Linear; a.dopplerLevel = 0f;
                thunder.Add(a);
            }
            built = true;
        }
        static float Get(Material m, string p, float d) => m.HasProperty(p) ? m.GetFloat(p) : d;

        ParticleSystem NewPs(string name, Material mat, int max, ParticleSystemRenderMode mode)
        {
            var go = new GameObject(name); go.transform.SetParent(root, false);
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = ps.main; m.loop = false; m.playOnAwake = false; m.maxParticles = max; m.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission; em.enabled = false; var sh = ps.shape; sh.enabled = false;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = mat; r.renderMode = mode;
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false; r.lightProbeUsage = LightProbeUsage.Off; r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            if (mode == ParticleSystemRenderMode.Stretch) r.sortMode = ParticleSystemSortMode.None;
            ps.Play(); return ps;
        }

        // ------------------------------------------------------------ mưa
        float splashAcc;
        void TickRain(float dt)
        {
            var cam = Camera.main; if (!cam || !rain) return;
            float w = Wet;
            var ct = cam.transform; var cv = playerRb ? playerRb.linearVelocity : Vector3.zero; cv.y = 0f;
            var fwd = ct.forward; fwd.y = 0f; fwd = fwd.sqrMagnitude > 1e-4f ? fwd.normalized : Vector3.forward;
            // hạt mưa: rải trong hộp 46 × 46 m phía trên camera, lệch tới trước theo vận tốc xe để luôn đủ mưa trước mặt
            if (w > 0.01f)
            {
                int n = Mathf.Min(260, Mathf.RoundToInt(3600f * w * w * dt + Random.value));
                var c = ct.position + cv * 0.55f + fwd * 6f;
                for (int i = 0; i < n; i++)
                {
                    var p = c + new Vector3(Random.Range(-23f, 23f), Random.Range(6f, 24f), Random.Range(-23f, 23f));
                    rain.Emit(new ParticleSystem.EmitParams
                    {
                        position = p, velocity = Vector3.zero, startSize = Random.Range(0.022f, 0.036f),
                        startLifetime = Random.Range(0.9f, 1.25f), startColor = new Color(1, 1, 1, Random.Range(0.45f, 1f))
                    }, 1);
                }
                // giọt bắn trên mặt đường/nóc xe phía trước camera
                splashAcc += 130f * w * dt;
                int k = (int)splashAcc; splashAcc -= k;
                for (int i = 0; i < k; i++)
                {
                    float ang = Random.Range(-75f, 75f), dist = Random.Range(2.5f, 30f);
                    var dir = Quaternion.Euler(0, ang, 0) * fwd;
                    var q = ct.position + cv * 0.25f + dir * dist;
                    if (!Physics.Raycast(new Vector3(q.x, ct.position.y + 12f, q.z), Vector3.down, out var hit, 40f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (hit.normal.y < 0.7f || hit.point.y < -0.6f) continue;
                    splash.Emit(new ParticleSystem.EmitParams
                    {
                        position = hit.point + Vector3.up * 0.12f, velocity = Vector3.zero, startSize = Random.Range(0.32f, 0.55f),
                        startLifetime = Random.Range(0.22f, 0.32f), startColor = new Color(1, 1, 1, Random.Range(0.5f, 0.9f))
                    }, 1);
                }
            }
            // tiếng mưa
            if (rainAudio)
            {
                rainAudio.volume = 0.34f * w;
                if (w > 0.01f && !rainAudio.isPlaying) { rainAudio.time = Random.Range(0f, 2f); rainAudio.Play(); }
                else if (w <= 0.01f && rainAudio.isPlaying) rainAudio.Stop();
            }
        }

        // ------------------------------------------------------------ ánh sáng + bầu trời
        static readonly Color RainFog = new Color(.42f, .46f, .51f), RainSun = new Color(.7f, .77f, .9f), FlashTint = new Color(.8f, .86f, 1f);
        void Apply()
        {
            if (!built) return;
            float w = Mathf.SmoothStep(0f, 1f, Wet), f = Flash;
            if (sun)
            {
                sun.intensity = Mathf.Lerp(baseSun, baseSun * 0.3f, w) + f * 0.55f;
                sun.color = Color.Lerp(Color.Lerp(baseSunCol, RainSun, w), FlashTint, Mathf.Clamp01(f) * 0.6f);
                sun.shadowStrength = Mathf.Lerp(baseShadow, 0.35f, w);
            }
            RenderSettings.ambientIntensity = Mathf.Lerp(baseAmb, baseAmb * 0.62f, w) + f * 0.4f;
            RenderSettings.reflectionIntensity = Mathf.Lerp(baseRefl, baseRefl * 0.55f, w) + f * 0.2f;
            RenderSettings.fogColor = Color.Lerp(baseFog, RainFog, w) + FlashTint * (f * 0.22f);
            RenderSettings.fogStartDistance = Mathf.Lerp(baseFogStart, 40f, w);
            RenderSettings.fogEndDistance = Mathf.Lerp(baseFogEnd, 560f, w);
            if (sky)
            {
                if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", Mathf.Lerp(baseExposure, 0.36f, w) + f * 1.3f);
                if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", Color.Lerp(baseSkyTint, new Color(.3f, .33f, .38f), w));
                if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", Mathf.Lerp(baseThick, 0.55f, w));
                if (sky.HasProperty("_SunSize")) sky.SetFloat("_SunSize", Mathf.Lerp(baseSunSize, 0f, w));
                if (sky.HasProperty("_GroundColor")) sky.SetColor("_GroundColor", Color.Lerp(baseGround, RainFog, w));
            }
            float vis = Mathf.Clamp01(Wet * 1.2f);
            var tint = Color.Lerp(new Color(.72f, .77f, .85f), new Color(1f, 1f, 1f), Mathf.Clamp01(f));
            if (rainMat) rainMat.SetColor("_Color", new Color(tint.r, tint.g, tint.b, 0.5f));
            if (splashMat) splashMat.SetColor("_Color", new Color(tint.r, tint.g, tint.b, 0.7f));
            if (curtainNear) { curtainNear.SetColor("_Color", new Color(tint.r, tint.g, tint.b, 0.2f)); curtainNear.SetFloat("_Alpha", vis); }
            if (curtainFar) { curtainFar.SetColor("_Color", new Color(tint.r * .9f, tint.g * .92f, tint.b * .96f, 0.34f)); curtainFar.SetFloat("_Alpha", vis); }
            if (domeMat) { domeMat.SetFloat("_Alpha", w); domeMat.SetFloat("_Flash", f); }
            if (tubes) tubes.gameObject.SetActive(Wet > 0.01f);
        }

        void OnDestroy()
        {
            if (!built) return;
            if (sun) { sun.intensity = baseSun; sun.color = baseSunCol; sun.shadowStrength = baseShadow; }
            RenderSettings.ambientIntensity = baseAmb; RenderSettings.reflectionIntensity = baseRefl;
            RenderSettings.fogColor = baseFog; RenderSettings.fogStartDistance = baseFogStart; RenderSettings.fogEndDistance = baseFogEnd;
            if (skyOwned && sky && RenderSettings.skybox == sky) { RenderSettings.skybox = null; }
        }

        // ------------------------------------------------------------ sét
        class Strike
        {
            public Vector3 p; public string kind; public float delay, age; public bool fired, live;
            public Renderer warn; public MeshFilter mf; public MeshRenderer mr; public MaterialPropertyBlock mpb;
            public Transform scorch; public Renderer scorchR; public float scorchAge = 99f, seed;
        }
        readonly List<Strike> strikes = new List<Strike>();
        static readonly int RevealId = Shader.PropertyToID("_Reveal"), FlashId = Shader.PropertyToID("_Flash"), BranchId = Shader.PropertyToID("_Branch"),
                            SeedId = Shader.PropertyToID("_Seed"), ProgressId = Shader.PropertyToID("_Progress"), RecolorId = Shader.PropertyToID("_Recolor"),
                            TintId = Shader.PropertyToID("_Color"), FillId = Shader.PropertyToID("_Fill");
        Strike NewStrike(int i)
        {
            var s = new Strike { mpb = new MaterialPropertyBlock() };
            var g = new GameObject("TiaSet" + i); g.transform.SetParent(root, false);
            s.mf = g.AddComponent<MeshFilter>(); s.mr = g.AddComponent<MeshRenderer>(); s.mr.sharedMaterial = boltMat;
            s.mr.shadowCastingMode = ShadowCastingMode.Off; s.mr.receiveShadows = false; s.mr.lightProbeUsage = LightProbeUsage.Off; s.mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            s.mr.localBounds = new Bounds(new Vector3(0, 75, 0), new Vector3(240, 175, 240));   // dải nở trong shader: tránh bị cắt khi khuất một phần
            g.SetActive(false);
            if (warnMat)
            {
                var w = new GameObject("VongSet" + i); w.transform.SetParent(root, false);
                w.AddComponent<MeshFilter>().sharedMesh = warnMesh; var wr = w.AddComponent<MeshRenderer>(); wr.sharedMaterial = warnMat;
                wr.shadowCastingMode = ShadowCastingMode.Off; wr.receiveShadows = false; s.warn = wr; w.SetActive(false);
                var b = new MaterialPropertyBlock(); wr.GetPropertyBlock(b);
                b.SetFloat(RecolorId, 1f); b.SetColor(TintId, new Color(.45f, .85f, 1f, 1f)); b.SetColor(FillId, new Color(.35f, .7f, 1f, .38f)); wr.SetPropertyBlock(b);
            }
            if (scorchMat)
            {
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad); Destroy(q.GetComponent<Collider>());
                q.name = "VetChay" + i; q.transform.SetParent(root, false); q.transform.rotation = Quaternion.Euler(90, 0, 0);
                s.scorchR = q.GetComponent<MeshRenderer>(); s.scorchR.sharedMaterial = scorchMat; s.scorchR.shadowCastingMode = ShadowCastingMode.Off;
                s.scorchR.receiveShadows = false; s.scorch = q.transform; q.SetActive(false);
            }
            return s;
        }

        void CancelVolley()
        {
            volleyOn = false; volleyClock = 0f;
            foreach (var s in strikes) { s.live = false; if (s.warn) s.warn.gameObject.SetActive(false); if (s.mr) s.mr.gameObject.SetActive(false); }
            if (boltLight) boltLight.enabled = false;
            Flash = 0f;
        }

        public void StartVolley()
        {
            if (!built || boltMeshes == null || boltMeshes.Length == 0) return;
            LastTargets.Clear(); LastKinds.Clear();
            PickTargets(LastTargets, LastKinds);
            if (LastTargets.Count == 0) { nextVolley = 2f; return; }
            Volleys++; volleyOn = true; volleyClock = 0f; rumbles = 0;
            for (int i = 0; i < strikes.Count; i++)
            {
                var s = strikes[i]; s.live = i < LastTargets.Count; s.fired = false; s.age = 0f;
                if (!s.live) continue;
                s.p = LastTargets[i]; s.kind = LastKinds[i]; s.delay = i == 0 ? 0f : Random.Range(0f, 0.12f); s.seed = Random.Range(0f, 100f);
                if (s.warn) { s.warn.gameObject.SetActive(true); s.warn.transform.position = s.p + Vector3.up * 0.05f; s.warn.transform.localScale = Vector3.one * StrikeRadius * 2f; }
            }
        }

        void TickVolley(float dt)
        {
            if (volleyOn)
            {
                volleyClock += dt;
                bool any = false; float flash = 0f; Strike lightAt = null; float best = float.MaxValue;
                var cam = Camera.main; var cp = cam ? cam.transform.position : Vector3.zero;
                foreach (var s in strikes)
                {
                    if (!s.live) continue;
                    any = true;
                    float t = volleyClock - WarnSeconds - s.delay;          // < 0: đang cảnh báo
                    if (s.warn && s.warn.gameObject.activeSelf)
                    {
                        float prog = Mathf.Clamp01(volleyClock / WarnSeconds);
                        s.warn.GetPropertyBlock(s.mpb); s.mpb.SetFloat(ProgressId, prog); s.warn.SetPropertyBlock(s.mpb);
                        if (t > 0.08f) s.warn.gameObject.SetActive(false);
                    }
                    if (t < -0.06f) continue;
                    if (!s.mr.gameObject.activeSelf && !s.fired)
                    {   // tia dẫn đường bắt đầu đi xuống
                        s.mf.sharedMesh = boltMeshes[Random.Range(0, boltMeshes.Length)];
                        s.mr.transform.SetPositionAndRotation(s.p, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                        s.mr.gameObject.SetActive(true);
                    }
                    float tau = t;                                           // 0 = chớp chính (sét chạm đất)
                    if (!s.fired && tau >= 0f) Fire(s);
                    float f = StrokeCurve(tau);
                    s.mr.GetPropertyBlock(s.mpb);
                    s.mpb.SetFloat(RevealId, tau < 0f ? Mathf.Clamp01((tau + 0.06f) / 0.06f) : 1.2f);
                    s.mpb.SetFloat(FlashId, tau < 0f ? 0.45f : f);
                    s.mpb.SetFloat(BranchId, tau < 0f ? 0.9f : Mathf.Lerp(0.15f, 1f, Mathf.Exp(-tau / 0.09f)));
                    s.mpb.SetFloat(SeedId, s.seed);
                    s.mr.SetPropertyBlock(s.mpb);
                    if (tau > 0.62f) { s.mr.gameObject.SetActive(false); s.live = false; continue; }
                    if (tau >= 0f) { flash = Mathf.Max(flash, f); float d = (s.p - cp).sqrMagnitude; if (d < best) { best = d; lightAt = s; } }
                }
                Flash = Mathf.Clamp01(flash) * 0.9f;
                if (boltLight)
                {
                    boltLight.enabled = lightAt != null && Flash > 0.02f;
                    if (lightAt != null) { boltLight.transform.position = lightAt.p + Vector3.up * 12f; boltLight.intensity = 2f * Flash; }
                }
                if (!any) { volleyOn = false; Flash = 0f; if (boltLight) boltLight.enabled = false; nextVolley = VolleyPause; }
            }
            // vết cháy mờ dần
            foreach (var s in strikes)
            {
                if (!s.scorch || !s.scorch.gameObject.activeSelf) continue;
                s.scorchAge += dt;
                float a = Mathf.Clamp01(1f - (s.scorchAge - 6f) / 3f);
                s.scorchR.GetPropertyBlock(s.mpb); s.mpb.SetColor(TintId, new Color(1, 1, 1, 0.92f * a)); s.scorchR.SetPropertyBlock(s.mpb);
                if (a <= 0f) s.scorch.gameObject.SetActive(false);
            }
        }

        // Chớp chính + 2 lần tái phóng theo đúng kênh cũ, dòng điện kéo dài mờ dần.
        static float StrokeCurve(float tau)
        {
            if (tau < 0f) return 0f;
            float f = 0f;
            f += Pulse(tau, 0f, 1.25f, 0.05f);
            f += Pulse(tau, 0.14f, 0.85f, 0.04f);
            f += Pulse(tau, 0.30f, 0.6f, 0.045f);
            f += 0.28f * Mathf.Exp(-tau / 0.22f);
            return f * Mathf.Clamp01((0.62f - tau) / 0.12f);
        }
        static float Pulse(float t, float at, float amp, float decay) => t < at ? 0f : amp * Mathf.Exp(-(t - at) / decay);

        void Fire(Strike s)
        {
            s.fired = true; Strikes++;
            var p = s.p;
            // tia lửa điện văng + chớp tại chân sét + khói
            if (sparks)
                for (int i = 0; i < 46; i++)
                {
                    var dir = new Vector3(Random.Range(-1f, 1f), Random.Range(.35f, 1.4f), Random.Range(-1f, 1f));
                    sparks.Emit(new ParticleSystem.EmitParams { position = p + Vector3.up * .25f, velocity = dir * Random.Range(4f, 11f), startSize = Random.Range(.12f, .3f), startLifetime = Random.Range(.35f, .8f), startColor = new Color(.85f, .93f, 1f, 1f) }, 1);
                }
            if (glints)
            {
                glints.Emit(new ParticleSystem.EmitParams { position = p + Vector3.up * 1.2f, velocity = Vector3.zero, startSize = 6.5f, startLifetime = .22f, startColor = new Color(.7f, .82f, 1f, .7f) }, 1);
                glints.Emit(new ParticleSystem.EmitParams { position = p + Vector3.up * .4f, velocity = Vector3.zero, startSize = 3.2f, startLifetime = .35f, startColor = new Color(1f, 1f, 1f, .8f) }, 1);
            }
            if (smoke)
                for (int i = 0; i < 6; i++)
                    smoke.Emit(new ParticleSystem.EmitParams { position = p + new Vector3(Random.Range(-1f, 1f), .3f + i * .25f, Random.Range(-1f, 1f)), velocity = Vector3.up * Random.Range(.6f, 1.6f), startSize = Random.Range(1.6f, 2.6f), startLifetime = Random.Range(1.8f, 2.8f), startColor = new Color(.42f, .42f, .44f, .55f), rotation = Random.Range(0f, 6.28f) }, 1);
            if (s.scorch)
            {
                s.scorch.gameObject.SetActive(true); s.scorch.position = p + Vector3.up * 0.03f;
                s.scorch.rotation = Quaternion.Euler(90, Random.Range(0f, 360f), 0); s.scorch.localScale = Vector3.one * Random.Range(3.4f, 4.4f); s.scorchAge = 0f;
            }
            // sấm: tia gần camera nổ chát, các tia khác rền; trễ theo khoảng cách
            var cam = Camera.main; float dist = cam ? Vector3.Distance(cam.transform.position, p) : 50f;
            bool nearest = true; if (cam) foreach (var o in strikes) if (o.live && o != s && (o.p - cam.transform.position).sqrMagnitude < dist * dist) { nearest = false; break; }
            int slot = nearest ? 0 : (rumbles < thunder.Count - 1 ? 1 + rumbles++ : -1);
            if (slot >= 0 && thunder[slot])
            {   // nguồn 0 = tiếng nổ chát của tia gần nhất, 1–3 = tiếng rền của các tia khác
                var a = thunder[slot]; a.transform.position = p + Vector3.up * 8f;
                a.clip = nearest ? crackClip : rumbleClip; a.volume = nearest ? 1f : 0.55f; a.pitch = Random.Range(0.9f, 1.08f);
                a.PlayDelayed(Mathf.Min(0.6f, dist / 340f));
            }
            // gây hại (chỉ hại xe người chơi khi đang lái)
            bool riding = game && game.Mode == OpenWorldGame.State.Riding;
            if (hazards) hazards.LightningBlast(p, StrikeRadius, riding);
        }

        // ------------------------------------------------------------ chọn chỗ đánh
        static List<List<Vector2>> riverLines;
        readonly List<OpenWorldTraffic.RoadDef> nearRoads = new List<OpenWorldTraffic.RoadDef>();
        void PickTargets(List<Vector3> pts, List<string> kinds)
        {
            if (!player) return;
            var cam = Camera.main;
            var pp = player.transform.position; pp.y = 0f;
            var fwd = cam ? cam.transform.forward : player.transform.forward; fwd.y = 0f; fwd = fwd.sqrMagnitude > 1e-4f ? fwd.normalized : Vector3.forward;
            bool danger = game && game.Mode == OpenWorldGame.State.Riding && !player.Crashed;
            float minR = danger ? 9f : 16f;
            nearRoads.Clear();
            if (traffic && traffic.roads != null)
                foreach (var r in traffic.roads) if (SegDist(new Vector2(pp.x, pp.z), r.a, r.b) < 150f) nearRoads.Add(r);

            // 1) tia nhắm lối xe đang chạy
            if (danger && playerRb)
            {
                var v = playerRb.linearVelocity; v.y = 0f;
                if (v.magnitude > 6f)
                {
                    var right = new Vector3(v.z, 0, -v.x).normalized;
                    for (int t = 0; t < 6; t++)
                    {
                        var p = pp + v * Random.Range(0.95f, 1.2f) + right * Random.Range(-2.2f, 2.2f);   // chỗ xe sẽ tới lúc sét đánh: phải phanh hoặc lách
                        if (Try(pts, kinds, p, "lối xe chạy", 0, minR)) break;
                    }
                }
            }
            // 2) theo loại: lề sông, lề biển, lề đường ×2, rồi đường phố cho đủ 7
            Fill(pts, kinds, "lề sông", 1, pp, fwd, minR, GenRiver);
            Fill(pts, kinds, "lề biển", 1, pp, fwd, minR, GenCoast);
            Fill(pts, kinds, "lề đường", 2, pp, fwd, minR, GenSidewalk);
            Fill(pts, kinds, "đường phố", VolleySize - pts.Count, pp, fwd, minR, GenRoad);
            if (pts.Count < VolleySize) Fill(pts, kinds, "lề đường", VolleySize - pts.Count, pp, fwd, minR, GenSidewalk);
            if (pts.Count < VolleySize) Fill(pts, kinds, "đường phố", VolleySize - pts.Count, pp, fwd, minR * 0.8f, GenRoad);
            // lượt cuối: nới điều kiện hướng (vẫn giữ cách nhau 11 m, ngoài bán kính an toàn) cho đủ 7 tia
            relax = true;
            for (int k = 0; k < 3 && pts.Count < VolleySize; k++)
            {
                Fill(pts, kinds, "lề đường", VolleySize - pts.Count, pp, fwd, minR, GenSidewalk);
                Fill(pts, kinds, "đường phố", VolleySize - pts.Count, pp, fwd, minR, GenRoad);
            }
            relax = false;
        }
        bool relax;

        delegate bool Gen(Vector3 pp, Vector3 fwd, out Vector3 p, out int need);   // need: 0 bất kỳ, 1 lòng đường, 2 mặt lề
        void Fill(List<Vector3> pts, List<string> kinds, string kind, int count, Vector3 pp, Vector3 fwd, float minR, Gen gen)
        {
            int got = 0;
            for (int t = 0; t < 60 && got < count && pts.Count < VolleySize; t++)
            {
                if (!gen(pp, fwd, out var p, out int need)) continue;
                var d = p - pp; d.y = 0f;
                // phần lớn trong tầm nhìn phía trước; nửa sau số lần thử cho phép mọi hướng
                if (!relax && t < 40 && d.sqrMagnitude > 1f && Vector3.Dot(d.normalized, fwd) < 0.25f) continue;
                if (Try(pts, kinds, p, kind, need, minR)) got++;
            }
        }

        bool Try(List<Vector3> pts, List<string> kinds, Vector3 p, string kind, int need, float minR)
        {
            var pp = player.transform.position; var d = p - pp; d.y = 0f; float dist = d.magnitude;
            if (dist < minR || dist > 135f) return false;
            foreach (var o in pts)
            {
                var e = o - p; e.y = 0f; if (e.sqrMagnitude < 11f * 11f) return false;
                // tỏa đều trong tầm nhìn: hai tia xa cùng hướng nhìn (lệch < 6°) sẽ chồng lên nhau thành một cột sáng
                var od = o - pp; od.y = 0f;
                if (!relax && dist > 30f && od.magnitude > 30f && Vector3.Angle(d, od) < 6f) return false;
            }
            if (!Ground(p, need, out float y)) return false;
            p.y = y; pts.Add(p); kinds.Add(kind); return true;
        }

        // Dò từ trên cao xuống: nhận mặt đường/lề (cao −0,6..0,6 m) hoặc nóc xe; loại mái nhà, cây, rào, mặt nước.
        static bool Ground(Vector3 p, int need, out float y)
        {
            y = 0f;
            if (!Physics.Raycast(new Vector3(p.x, 90f, p.z), Vector3.down, out var hit, 140f, ~0, QueryTriggerInteraction.Ignore)) return false;
            if (hit.rigidbody) { y = 0.02f; return need != 2; }
            if (hit.point.y > 0.6f || hit.point.y < -0.6f || hit.normal.y < 0.8f) return false;
            bool le = hit.collider.gameObject.layer == OpenWorldGroundFit.SurfaceLayer;
            if (need == 1 && le) return false;
            if (need == 2 && !le) return false;
            y = hit.point.y; return true;
        }

        static float SegDist(Vector2 q, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(q - a, ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
            return (q - (a + ab * t)).magnitude;
        }

        bool RoadPoint(Vector3 pp, bool sidewalk, out Vector3 p, out int need)
        {
            p = pp; need = sidewalk ? 2 : 1;
            if (nearRoads.Count == 0) return false;
            var r = nearRoads[Random.Range(0, nearRoads.Count)];
            var ab = r.b - r.a; float len = ab.magnitude; if (len < 1f) return false;
            var q = new Vector2(pp.x, pp.z);
            float t0 = Vector2.Dot(q - r.a, ab) / (len * len);
            float t = Mathf.Clamp01(t0 + Random.Range(-115f, 115f) / len);
            var basePt = r.a + ab * t; var perp = new Vector2(-ab.y, ab.x) / len;
            float half = r.main ? 7f : 3.5f;
            float off = sidewalk ? (Random.value < .5f ? -1f : 1f) * (half + Random.Range(1f, 3.2f)) : Random.Range(-(half - 0.8f), half - 0.8f);
            var w = basePt + perp * off; p = new Vector3(w.x, 0f, w.y); return true;
        }
        bool GenRoad(Vector3 pp, Vector3 fwd, out Vector3 p, out int need) => RoadPoint(pp, false, out p, out need);
        bool GenSidewalk(Vector3 pp, Vector3 fwd, out Vector3 p, out int need) => RoadPoint(pp, true, out p, out need);
        bool GenRiver(Vector3 pp, Vector3 fwd, out Vector3 p, out int need)
        {
            p = pp; need = 0;
            if (riverLines == null) riverLines = OpenWorldBoats.Centerlines();
            if (riverLines == null || riverLines.Count == 0) return false;
            var q = new Vector2(pp.x, pp.z); float best = float.MaxValue; Vector2 foot = q, dir = Vector2.right;
            foreach (var line in riverLines)
                for (int i = 0; i + 1 < line.Count; i++)
                {
                    Vector2 a = line[i], b = line[i + 1], ab = b - a; float l2 = Mathf.Max(1e-4f, ab.sqrMagnitude);
                    float t = Mathf.Clamp01(Vector2.Dot(q - a, ab) / l2); var f = a + ab * t; float d = (q - f).sqrMagnitude;
                    if (d < best) { best = d; foot = f; dir = ab.normalized; }
                }
            if (best > 230f * 230f) return false;                       // sông xa quá
            var n = new Vector2(-dir.y, dir.x); float sideSign = Mathf.Sign(Vector2.Dot(q - foot, n)); if (sideSign == 0f) sideSign = 1f;
            if (Random.value < 0.2f) sideSign = -sideSign;               // thỉnh thoảng đánh lề bờ bên kia
            var w = foot + dir * Random.Range(-110f, 110f) + n * sideSign * Random.Range(84f, 96f);
            p = new Vector3(w.x, 0f, w.y); return true;
        }
        bool GenCoast(Vector3 pp, Vector3 fwd, out Vector3 p, out int need)
        {
            need = 0; return OpenWorldHazards.CoastPoint(pp, 150f, Random.Range(-110f, 110f), out p);
        }

        // ------------------------------------------------------------ âm thanh tổng hợp
        static AudioClip RainClip()
        {
            const int hz = 22050; int n = hz * 3; var w = new float[n]; var r = new System.Random(3171);
            float lp = 0f, lp2 = 0f;
            for (int i = 0; i < n; i++)
            {
                float x = (float)(r.NextDouble() * 2 - 1);
                lp += (x - lp) * 0.22f; lp2 += (lp - lp2) * 0.05f;
                float hiss = (x - lp) * 0.35f + lp * 0.5f + lp2 * 0.6f;
                // lộp độp: giọt to rơi ngẫu nhiên
                float drop = r.NextDouble() < 0.0016 ? (float)(r.NextDouble() * 0.9 + 0.3) : 0f;
                w[i] = hiss * (0.85f + 0.15f * Mathf.Sin(i * 0.00031f)) + drop;
            }
            for (int i = 1; i < n; i++) w[i] = Mathf.Clamp(w[i] * 0.9f + w[i - 1] * 0.1f, -1f, 1f);
            int f = hz / 10;
            for (int i = 0; i < f; i++) { float a = (float)i / f; w[i] = w[i] * a + w[n - f + i] * (1 - a); }
            var outw = new float[n - f]; System.Array.Copy(w, outw, n - f);
            var clip = AudioClip.Create("Tiếng mưa", n - f, 1, hz, false); clip.SetData(outw, 0); return clip;
        }
        // near = tiếng nổ chát rồi ầm (sét gần); không = tiếng rền kéo dài (sét xa)
        static AudioClip ThunderClip(bool near)
        {
            const int hz = 22050; float sec = near ? 3.2f : 4f; int n = (int)(hz * sec); var w = new float[n]; var r = new System.Random(near ? 317 : 7131);
            float low = 0f, rum = 0f, rum2 = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / hz, x = (float)(r.NextDouble() * 2 - 1);
                low += (x - low) * 0.12f; rum += (x - rum) * 0.02f; rum2 += (rum - rum2) * 0.04f;
                float crack = near ? (Mathf.Exp(-t / 0.02f) * 1.1f + 0.7f * Mathf.Exp(-Mathf.Abs(t - 0.07f) / 0.012f) + 0.45f * Mathf.Exp(-Mathf.Abs(t - 0.16f) / 0.018f)) : 0f;
                float body = (1 - Mathf.Exp(-t / (near ? 0.06f : 0.35f))) * Mathf.Exp(-t / (near ? 0.9f : 1.4f));
                float roll = 1f + 0.35f * Mathf.Sin(t * 7.3f) * Mathf.Sin(t * 2.1f + 1f);      // lăn ầm ì
                float end = Mathf.Clamp01((n - i) / (hz * 0.3f));
                w[i] = Mathf.Clamp((x - low) * crack * 0.7f + body * roll * (low * 0.6f + rum * 5f + rum2 * 9f), -0.98f, 0.98f) * end;
            }
            var clip = AudioClip.Create(near ? "Sấm gần" : "Sấm xa", n, 1, hz, false); clip.SetData(w, 0); return clip;
        }

        // ------------------------------------------------------------ thẻ thông báo
        void Toast(string title, string detail, float seconds, OpenWorldToast.Kind kind)
        { toastTitle = title; toastDetail = detail; toastKind = kind; toastAt = Time.time; toastUntil = Time.time + seconds; }
        void OnGUI()
        {
            if (Time.time >= toastUntil || !game || game.Mode != OpenWorldGame.State.Riding || (player && player.Crashed)) return;
            OpenWorldToast.Show("THỜI TIẾT", toastTitle, toastDetail, toastKind, Time.time - toastAt, toastUntil - Time.time);
        }

        public string Report() => $"weather rain={Raining} wet={Wet:F2} t={RainTime:F1} rolls={RainRolls}/{Rolls} volleys={Volleys} strikes={Strikes} next={nextVolley:F1} flash={Flash:F2} drops={ParticleCount} hits={(hazards ? hazards.LightningHits : 0)} npc={(hazards ? hazards.LightningNpc : 0)} kinds=[{string.Join(",", LastKinds)}]";
    }
}
