using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.316g — người dùng 22/11/2026: «khi phanh drift cho hiệu ứng khói bụi trên các lốp xe và tiếng động khi phanh và drift».
    // Đọc mức trượt lốp từ mô hình lái (OpenWorldDriver.RearSlip / FrontSlip, 21.316e):
    // • Drift (thắng tay/P-DRIFT + đánh lái): khói trắng xám dày cuộn ra từ 2 bánh sau, bị kéo lại phía sau theo hướng xe chạy.
    // • Phanh gấp (Space / P-DRIFT không đánh lái, phím lùi khi đang chạy nhanh): khói ở cả 4 bánh, nhẹ hơn ở bánh trước.
    // • Ôm cua quá giới hạn bám (thiếu lái) / sát giới hạn: khói mỏng ở bánh trước.
    // • Tiếng lốp rít tự tổng hợp (vòng lặp 2 s: tiếng rít 1–1,6 kHz rung + tiếng xào xạc mặt đường), âm lượng và cao độ theo mức
    //   trượt (drift cao và to hơn phanh); vào/ra mượt 0,08/0,2 s. Đi qua PhoVietAudio (âm lượng tổng + nút loa).
    // Khói dùng vật liệu khói trắng của vòi nhảy (OpenWorldJumpJets.smokeMaterial = OW_KhoiNhay, 4×4 khung) → không thêm asset; tối đa 480 hạt.
    public class OpenWorldTireFx : MonoBehaviour
    {
        [Tooltip("Vị trí 4 bánh trong khung xe (x ngang, y sát mặt đường, z dọc): trước / sau")]
        public Vector3 frontWheel = new Vector3(0.82f, 0.12f, 1.32f), rearWheel = new Vector3(0.84f, 0.12f, -1.3f);
        public float smokePerSecond = 72f, maxVolume = 0.55f;

        OpenWorldDriver driver; Rigidbody rb; ParticleSystem smoke; AudioSource screech;
        float accum, vol;
        public int Puffs { get; private set; }
        public float Volume => vol;

        void Awake() { driver = GetComponent<OpenWorldDriver>(); rb = GetComponent<Rigidbody>(); }

        void Build()
        {
            if (smoke || !driver) return;
            var go = new GameObject("KhoiLop");                         // không làm con xe: khói ở lại không gian thế giới
            smoke = go.AddComponent<ParticleSystem>(); smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = smoke.main; m.loop = false; m.playOnAwake = false; m.maxParticles = 480;
            m.simulationSpace = ParticleSystemSimulationSpace.World; m.gravityModifier = -0.03f;
            var em = smoke.emission; em.enabled = false; var sh = smoke.shape; sh.enabled = false;
            var col = smoke.colorOverLifetime; col.enabled = true; var gr = new Gradient();
            gr.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                       new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.08f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = gr;
            var size = smoke.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.45f, 1f, 2.6f));
            var vel = smoke.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            // 21.317: cả 3 trục cùng kiểu «hai hằng số» (khác kiểu → Unity báo lỗi «Particle Velocity curves must all be in the same mode»)
            vel.x = new ParticleSystem.MinMaxCurve(0f, 0f); vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.y = new ParticleSystem.MinMaxCurve(0.15f, 0.6f);
            var noise = smoke.noise; noise.enabled = true; noise.strength = 0.35f; noise.frequency = 0.6f; noise.scrollSpeed = 0.4f;
            var lim = smoke.limitVelocityOverLifetime; lim.enabled = true; lim.limit = 3f; lim.dampen = 0.25f;
            // khói trắng của vòi nhảy (OW_KhoiNhay, ảnh 4×4 khung) — sáng như khói lốp thật; không có thì dùng khói xe hỏng (sẫm hơn)
            var jets = GetComponent<OpenWorldJumpJets>();
            var mat = jets && jets.smokeMaterial ? jets.smokeMaterial : driver.smokeMaterial;
            var sheet = smoke.textureSheetAnimation; sheet.enabled = true;
            if (jets && jets.smokeMaterial)
            {
                sheet.numTilesX = jets.sheet.x; sheet.numTilesY = jets.sheet.y; sheet.animation = ParticleSystemAnimationType.WholeSheet;
                sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0f, 1, .999f));
            }
            else { sheet.numTilesX = 4; sheet.numTilesY = 2; sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .75f, 1, .999f)); }
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false; r.sortingFudge = 3; r.sortMode = ParticleSystemSortMode.Distance;
            r.SetActiveVertexStreams(new System.Collections.Generic.List<ParticleSystemVertexStream> { ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV });
            smoke.Play();

            screech = PhoVietAudio.Route(gameObject.AddComponent<AudioSource>());
            screech.clip = ScreechClip(); screech.loop = true; screech.playOnAwake = false; screech.volume = 0f;
            screech.spatialBlend = 0.35f; screech.minDistance = 8f; screech.maxDistance = 90f;
        }

        void Update()
        {
            if (!driver) return;
            Build(); if (!smoke) return;
            float dt = Time.deltaTime;
            bool live = !driver.Crashed && !driver.Sinking && Time.timeScale > 0f;
            float rear = live ? driver.RearSlip : 0f, front = live ? driver.FrontSlip : 0f;
            float k = Mathf.Max(rear, front);
            // khói: số hạt mỗi bánh theo mức trượt
            if (k > 0.04f)
            {
                accum += smokePerSecond * dt;
                int n = (int)accum; accum -= n;
                var t = transform; var v = rb ? rb.linearVelocity : Vector3.zero; v.y = 0f;
                for (int i = 0; i < n; i++)
                {
                    Puff(t, new Vector3(-rearWheel.x, rearWheel.y, rearWheel.z), rear, v, true);
                    Puff(t, new Vector3(rearWheel.x, rearWheel.y, rearWheel.z), rear, v, true);
                    Puff(t, new Vector3(-frontWheel.x, frontWheel.y, frontWheel.z), front, v, false);
                    Puff(t, new Vector3(frontWheel.x, frontWheel.y, frontWheel.z), front, v, false);
                }
            }
            else accum = 0f;
            // tiếng lốp rít
            float want = Mathf.Clamp01(k * 1.15f) * maxVolume;
            vol = Mathf.MoveTowards(vol, want, dt / (want > vol ? 0.08f : 0.2f) * maxVolume);
            if (vol > 0.005f)
            {
                if (!screech.isPlaying) { screech.time = Random.Range(0f, 1.8f); screech.Play(); }
                screech.volume = vol;
                screech.pitch = Mathf.Lerp(0.85f, 1.18f, rear) + Random.Range(-0.02f, 0.02f);
            }
            else if (screech.isPlaying) screech.Stop();
        }

        void Puff(Transform t, Vector3 local, float slip, Vector3 carVel, bool rearWheelPuff)
        {
            if (slip < 0.04f || Random.value > slip) return;            // mức trượt thấp → thưa hạt
            Puffs++;
            var pos = t.TransformPoint(local) + Random.insideUnitSphere * 0.12f; pos.y = Mathf.Max(pos.y, t.position.y + 0.05f);
            var side = t.right * Mathf.Sign(local.x);
            var vel = carVel * Random.Range(0.05f, 0.25f) + side * Random.Range(0.3f, 1.4f) + Vector3.up * Random.Range(0.2f, 0.8f) - t.forward * Random.Range(0.2f, 1.2f);
            float big = rearWheelPuff ? 1f : 0.8f;
            smoke.Emit(new ParticleSystem.EmitParams
            {
                position = pos, velocity = vel, startSize = Random.Range(1.0f, 1.7f) * big * Mathf.Lerp(0.6f, 1.2f, slip),
                startLifetime = Random.Range(1.1f, 1.9f) * Mathf.Lerp(0.6f, 1f, slip), rotation = Random.Range(0f, 360f),
                startColor = new Color(0.93f, 0.93f, 0.92f, Mathf.Lerp(0.25f, 0.5f, slip))
            }, 1);
        }

        // tiếng lốp rít: dải rít 1–1,6 kHz (3 thành phần lệch nhau, rung theo nhịp mặt đường) + tiếng xào xạc nền, vòng lặp 2 s
        static AudioClip ScreechClip()
        {
            const int hz = 22050; int n = hz * 2; var w = new float[n]; var r = new System.Random(316);
            float ph1 = 0, ph2 = 0, ph3 = 0, lp = 0, bp = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / hz;
                float wob = Mathf.Sin(2 * Mathf.PI * 0.5f * t) * 0.5f + Mathf.Sin(2 * Mathf.PI * 1.5f * t) * 0.3f;   // tròn vòng 2 s
                ph1 += 2 * Mathf.PI * (1180f + 120f * wob) / hz;
                ph2 += 2 * Mathf.PI * (1470f + 90f * wob) / hz;
                ph3 += 2 * Mathf.PI * (2350f + 200f * wob) / hz;
                float am = 0.65f + 0.35f * Mathf.Sin(2 * Mathf.PI * 23f * t + Mathf.Sin(2 * Mathf.PI * 3f * t) * 2f);
                float noise = (float)(r.NextDouble() * 2 - 1);
                lp += (noise - lp) * 0.35f; bp = noise - lp;                // tiếng xào xạc (lọc thông cao đơn giản)
                float squeal = (Mathf.Sin(ph1) * 0.45f + Mathf.Sin(ph2) * 0.3f + Mathf.Sin(ph3) * 0.1f) * am;
                w[i] = Mathf.Clamp(squeal * 0.5f + bp * 0.22f + lp * 0.12f, -1f, 1f);
            }
            // nối vòng mượt: trộn 0,05 s đầu/cuối
            int f = hz / 20;
            for (int i = 0; i < f; i++) { float a = (float)i / f; w[i] = w[i] * a + w[n - f + i] * (1 - a); }
            var clip = AudioClip.Create("Lốp rít", n - f, 1, hz, false);
            var outw = new float[n - f]; System.Array.Copy(w, outw, n - f); clip.SetData(outw, 0); return clip;
        }

        void OnDestroy() { if (smoke) Destroy(smoke.gameObject); }
    }
}
