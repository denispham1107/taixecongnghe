using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.312 — lưới điện CSGT VỠ TUNG khi chạm khiên năng lượng (OpenWorldShield). Mảnh lưới dựng bằng Blender MCP
    // (build_khien.py → Resources/Khien/PV_LuoiVo.fbx: 10 mảnh lưới thoi rách có tua dây đứt, dây lõi + quầng; vòng sóng xung kích).
    // • 16 mảnh văng ra từ mặt lưới theo pháp tuyến (5–10 m/s, hất lên 2–5 m/s, cộng vận tốc xe), xoay lộn, rơi theo trọng lực,
    //   ánh điện nhấp nháy rồi tắt dần trong 0,9–1,4 s; vật liệu điện của lưới (Resources/PoliceNet) nên cùng màu lưới.
    // • Vòng sóng xung kích nở 1,2 → 7 m sát mặt đường; 160 tia lửa điện tóe ra; chớp đèn xanh 0,4 s; tiếng nổ điện «tạch» tổng hợp.
    public class OpenWorldNetShatter : MonoBehaviour
    {
        public static int Spawned;
        static readonly List<Mesh> pieces = new List<Mesh>(); static Mesh waveMesh;
        static Material coreMat, haloMat, waveMat, dotMat; static AudioClip crackClip;
        static readonly int ColorId = Shader.PropertyToID("_Color");

        class Piece { public Transform t; public Renderer r; public Vector3 v, w; public float life; }
        readonly List<Piece> list = new List<Piece>();
        Transform wave; Renderer waveR; Light flash; ParticleSystem sparks; MaterialPropertyBlock mpb;
        float age; Color coreC, haloC, waveC;

        static void Load()
        {
            if (pieces.Count > 0) return;
            var model = Resources.Load<GameObject>("Khien/PV_LuoiVo");
            if (model)
                foreach (var mf in model.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh.name.StartsWith("LuoiVo_Manh")) pieces.Add(mf.sharedMesh);
                    else if (mf.sharedMesh.name == "LuoiVo_Song") waveMesh = mf.sharedMesh;
                }
            coreMat = Resources.Load<Material>("PoliceNet/PV_LuoiDien_Day");
            haloMat = Resources.Load<Material>("PoliceNet/PV_LuoiDien_Quang");
            dotMat = Resources.Load<Material>("PoliceNet/PV_LuoiDien_Hat");
            waveMat = Resources.Load<Material>("Khien/PV_LuoiVo_Song");
            crackClip = CrackClip();
        }

        // center/rot/halfExt: vỏ lưới lúc vỡ (OpenWorldPoliceNet); carVel: vận tốc xe người chơi.
        public static OpenWorldNetShatter Spawn(Vector3 center, Quaternion rot, Vector3 halfExt, Vector3 carVel)
        {
            Load(); Spawned++;
            var go = new GameObject("LuoiDienVo");
            var s = go.AddComponent<OpenWorldNetShatter>(); s.Init(center, rot, halfExt, carVel); return s;
        }

        void Init(Vector3 center, Quaternion rot, Vector3 halfExt, Vector3 carVel)
        {
            mpb = new MaterialPropertyBlock();
            coreC = coreMat ? coreMat.GetColor(ColorId) : new Color(.8f, 1.2f, 2f); haloC = haloMat ? haloMat.GetColor(ColorId) : new Color(.3f, .6f, 1.2f);
            waveC = waveMat ? waveMat.GetColor(ColorId) : new Color(.5f, .9f, 1.8f);
            transform.position = center;
            var cv = carVel; cv.y = 0;
            for (int i = 0; i < 16 && pieces.Count > 0; i++)
            {
                // điểm trên nửa trên vỏ elip của lưới
                var u = Random.onUnitSphere; u.y = Mathf.Abs(u.y) * 0.85f + 0.15f; u.Normalize();
                var lp = Vector3.Scale(u, halfExt);
                var ln = new Vector3(u.x / halfExt.x, u.y / halfExt.y, u.z / halfExt.z).normalized;
                var p = center + rot * lp; var n = rot * ln;
                var go = new GameObject("ManhLuoi" + i); go.transform.SetParent(transform, true);
                go.transform.SetPositionAndRotation(p, Quaternion.FromToRotation(Vector3.up, n) * Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                go.AddComponent<MeshFilter>().sharedMesh = pieces[i % pieces.Count];
                var r = go.AddComponent<MeshRenderer>();
                r.sharedMaterials = new[] { coreMat, haloMat }; Quiet(r);
                list.Add(new Piece
                {
                    t = go.transform, r = r, life = Random.Range(0.9f, 1.4f),
                    v = n * Random.Range(5f, 10f) + Vector3.up * Random.Range(2f, 5f) + cv * 0.6f,
                    w = new Vector3(Random.Range(-540f, 540f), Random.Range(-360f, 360f), Random.Range(-540f, 540f))
                });
            }
            // vòng sóng sát mặt đường
            if (waveMesh)
            {
                var wg = new GameObject("SongXungKich"); wg.transform.SetParent(transform, false);
                wg.transform.position = new Vector3(center.x, center.y - halfExt.y + 0.08f, center.z);
                wg.AddComponent<MeshFilter>().sharedMesh = waveMesh; waveR = wg.AddComponent<MeshRenderer>(); waveR.sharedMaterial = waveMat; Quiet(waveR);
                wave = wg.transform;
            }
            // chớp đèn
            var lg = new GameObject("ChopDien"); lg.transform.SetParent(transform, false); lg.transform.position = center + Vector3.up * 0.6f;
            flash = lg.AddComponent<Light>(); flash.type = LightType.Point; flash.color = new Color(.45f, .8f, 1f); flash.range = 10f; flash.intensity = 7f;
            flash.shadows = LightShadows.None; flash.renderMode = LightRenderMode.ForcePixel;
            // tia lửa điện tóe
            var sg = new GameObject("TiaLuaVo"); sg.transform.SetParent(transform, false);
            sparks = sg.AddComponent<ParticleSystem>(); sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var mm = sparks.main; mm.loop = false; mm.playOnAwake = false; mm.maxParticles = 300; mm.simulationSpace = ParticleSystemSimulationSpace.World; mm.gravityModifier = 1.2f;
            var em = sparks.emission; em.enabled = false; var sh = sparks.shape; sh.enabled = false;
            var pr = sg.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = dotMat; pr.renderMode = ParticleSystemRenderMode.Stretch; pr.velocityScale = 0.04f; pr.lengthScale = 1.5f;
            pr.shadowCastingMode = ShadowCastingMode.Off; pr.receiveShadows = false;
            sparks.Play();
            for (int k = 0; k < 160; k++)
            {
                var u = Random.onUnitSphere; u.y = Mathf.Abs(u.y);
                var p = center + rot * Vector3.Scale(u, halfExt);
                sparks.Emit(new ParticleSystem.EmitParams { position = p, velocity = (rot * u) * Random.Range(3f, 11f) + Vector3.up * Random.Range(0f, 3f) + cv * 0.5f, startSize = Random.Range(.08f, .24f), startLifetime = Random.Range(.25f, .7f), startColor = Color.white }, 1);
            }
            var a = PhoVietAudio.Route(gameObject.AddComponent<AudioSource>());
            a.playOnAwake = false; a.spatialBlend = 0.4f; a.minDistance = 8f; a.maxDistance = 90f;
            if (crackClip) a.PlayOneShot(crackClip, 0.9f);
            Destroy(gameObject, 1.8f);
        }

        void Update()
        {
            float dt = Time.deltaTime; age += dt;
            foreach (var p in list)
            {
                if (!p.t) continue;
                if (age >= p.life) { if (p.r.enabled) p.r.enabled = false; continue; }
                p.v += Physics.gravity * 1.1f * dt; p.v *= 1f - 0.6f * dt;
                p.t.position += p.v * dt; p.t.rotation = Quaternion.Euler(p.w * dt) * p.t.rotation;
                float k = 1f - Mathf.Clamp01((age - p.life * 0.45f) / (p.life * 0.55f));
                float flick = 0.7f + 0.3f * Mathf.Sin(age * 60f + p.life * 30f);
                p.r.GetPropertyBlock(mpb, 0); mpb.SetColor(ColorId, coreC * k * flick); p.r.SetPropertyBlock(mpb, 0);
                p.r.GetPropertyBlock(mpb, 1); mpb.SetColor(ColorId, haloC * k * flick); p.r.SetPropertyBlock(mpb, 1);
            }
            if (wave)
            {
                float w = Mathf.Clamp01(age / 0.5f);
                wave.localScale = Vector3.one * Mathf.Lerp(1.2f, 7f, 1f - (1f - w) * (1f - w));
                waveR.GetPropertyBlock(mpb); mpb.SetColor(ColorId, waveC * (1f - w)); waveR.SetPropertyBlock(mpb);
                if (w >= 1f) waveR.enabled = false;
            }
            if (flash) flash.intensity = Mathf.Max(0f, 7f * (1f - age / 0.4f));
        }

        static void Quiet(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off; r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        static AudioClip CrackClip()
        {
            const int hz = 22050; int n = (int)(hz * 0.7f); var w = new float[n]; var r = new System.Random(13); float crack = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / hz;
                if (r.NextDouble() < 0.02 * Mathf.Exp(-t * 6f)) crack = 1f;
                crack *= 0.993f;
                float env = Mathf.Exp(-t * 5f);
                float boom = Mathf.Sin(t * 2 * Mathf.PI * 70f * (1 - t * 0.5f)) * Mathf.Exp(-t * 9f);
                float noise = (float)(r.NextDouble() * 2 - 1);
                w[i] = Mathf.Clamp(boom * 0.6f + noise * (0.35f * env + 0.5f * crack) + Mathf.Sin(t * 2 * Mathf.PI * 1900f) * 0.12f * Mathf.Exp(-t * 20f), -1f, 1f);
            }
            var clip = AudioClip.Create("Lưới điện vỡ", n, 1, hz, false); clip.SetData(w, 0); return clip;
        }
    }
}
