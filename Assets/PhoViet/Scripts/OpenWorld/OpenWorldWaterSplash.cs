using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.365: xe người chơi rơi xuống nước (sông/biển, mặt nước y = OpenWorldDriver.WaterY) — trước đây xe chỉ chìm xuyên xuống.
    // Dựng lại theo pha vật lý của vật nặng lao xuống nước (ảnh render trong Blender: ArtSource/Blender/OpenWorld/ow_water_splash.py):
    //   1–3. Nước văng: HÌNH 3D 20 khung (Resources/NuocVang/PV_NuocVang3D.fbx, 1,2 s, shader PhoViet/OpenWorld/WaterSplash) —
    //        vương miện nước nở + ngón nước + giọt to, cột nước giữa (Worthington) dâng rồi đổ, ụ bọt chân; cỡ theo tốc độ lao.
    //        21.365e: thay ảnh phẳng 4×4 256²/khung (bể + mờ khi camera lia gần) — hình 3D nét ở mọi khoảng cách, đúng phối cảnh,
    //        thành nước phía trước che xe/phía sau bị xe che, phản chiếu trời + phố thật.
    //   2.   Giọt nước văng (hạt kéo dài theo vận tốc, rơi theo trọng lực).
    //   4.   Sóng chấn động: 3 vòng sóng mép bọt (PV_SongTron) nở trên mặt nước, vòng sau nhỏ/yếu hơn.
    //   5.   Mảng bọt trắng (PV_BotNuoc) loang rồi tan + sủi bọt khí suốt lúc xe chìm.
    // Giọt/sóng/bọt là hạt (ParticleSystem) dùng shader mưa PhoViet/OpenWorld/Rain (_Mode 0), số hạt nhỏ cho điện thoại.
    [DefaultExecutionOrder(130)]
    public class OpenWorldWaterSplash : MonoBehaviour
    {
        const float CrownSeconds = 1.2f;                                       // ow_water_splash.py DUR
        OpenWorldDriver driver;
        ParticleSystem drops, rings, foam, boil, mist;
        Mesh[] crownFrames; float[] crownHeights; GameObject crown; MeshFilter crownMf; MeshRenderer crownMr; MaterialPropertyBlock crownMpb;
        Coroutine crownPlay;
        int seenFalls;
        public int Splashes { get; private set; }                                // kiểm thử
        // 21.369: âm thanh từ lúc chạm nước tới khi chìm hẳn (ArtSource/Audio/ow_water_fall_audio.py → Resources/NuocVang/PV_Am*.wav):
        //   «ÙM» chạm nước (to/trầm theo tốc độ lao) → xe chìm (PV_AmXeChim: nước ào vào khoang, bọt ùng ục) → đúng lúc nóc
        //   xe xuống dưới mặt nước: tắt nhanh tiếng chìm, phát PV_AmXeNgap («ực» lớn + bóp nghẹt như nghe dưới nước, bọt lẻ tẻ
        //   rồi lặng khi xe nằm đáy). 21.371: bỏ tiếng gió khi đang rơi theo yêu cầu người dùng (âm thanh bắt đầu lúc chạm nước).
        AudioSource splashAudio, sinkAudio, underAudio; float splashAt = -10f; bool submerged; Collider body;
        public bool SinkPlaying => sinkAudio && sinkAudio.isPlaying;
        public float SubmergedAt { get; private set; } = -10f;

        void Awake()
        {
            driver = GetComponent<OpenWorldDriver>();
            var root = new GameObject("NuocVang").transform;                    // không gắn vào xe: hiệu ứng nằm yên trên mặt nước
            root.SetParent(null, false);
            // 21.365b: nước văng + giọt giữ đúng màu ảnh (nước trong xanh lục nhạt, mép sáng, lóa) — không nhân trắng xám nữa
            Material Mat(string tex, float a, bool keepColor = false)
            {
                var t = Resources.Load<Texture2D>("NuocVang/" + tex); var sh = Shader.Find("PhoViet/OpenWorld/Rain");
                if (!t || !sh) return null;
                var m = new Material(sh) { mainTexture = t }; m.SetFloat("_Mode", 0); m.SetColor("_Color", keepColor ? new Color(1f, 1f, 1f, a) : new Color(.9f, .94f, .97f, a));
                return m;
            }
            drops = Make(root, "NuocVang_Giot", Mat("PV_GiotNuoc4x2", 1f, true), ParticleSystemRenderMode.Billboard, 1f, 1);
            rings = Make(root, "NuocVang_SongTron", Mat("PV_SongTron", 1f), ParticleSystemRenderMode.HorizontalBillboard, 0f, 1);
            foam = Make(root, "NuocVang_Bot", Mat("PV_BotNuoc", .95f), ParticleSystemRenderMode.HorizontalBillboard, 0f, 1);
            boil = Make(root, "NuocVang_SuiBot", Mat("PV_BotNuoc", .9f), ParticleSystemRenderMode.HorizontalBillboard, 0f, 1);
            // 21.365g: sương nước mờ bốc lên quanh nước văng (dùng lại ảnh khói trắng Boost/PV_WhiteBoostSmoke)
            {
                var t = Resources.Load<Texture2D>("Boost/PV_WhiteBoostSmoke"); var sh = Shader.Find("PhoViet/OpenWorld/Rain");
                if (t && sh) { var m = new Material(sh) { mainTexture = t }; m.SetFloat("_Mode", 0); m.SetColor("_Color", new Color(.93f, .96f, .98f, .32f));
                    mist = Make(root, "NuocVang_Suong", m, ParticleSystemRenderMode.Billboard, -.04f, 1); }
            }
            BuildCrown(root);
            BuildAudio(root);
            if (drops)
            {
                // 21.365d: 8 dáng giọt thật (hạt lệ, kéo sợi, méo, dẹt — PV_GiotNuoc4x2), mỗi giọt 1 dáng ngẫu nhiên, xoay + lộn khi bay.
                // Giọt trong + nhỏ: dưới nắng phải lóa sáng mới thấy → ảnh × 1,25 sáng, alpha × 2 (thử Play).
                drops.GetComponent<ParticleSystemRenderer>().sharedMaterial.SetColor("_Color", new Color(1.25f, 1.25f, 1.25f, 2f));
                var sheet = drops.textureSheetAnimation; sheet.enabled = true; sheet.numTilesX = 4; sheet.numTilesY = 2;
                sheet.animation = ParticleSystemAnimationType.WholeSheet; sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0);
                sheet.startFrame = new ParticleSystem.MinMaxCurve(0, 7.99f);
                var rot = drops.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
                var c = drops.colorOverLifetime; c.enabled = true; c.color = Fade(0f, .75f);
            }
            if (rings)
            {
                var sz = rings.sizeOverLifetime; sz.enabled = true;
                sz.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .12f, 0, 2.6f), new Keyframe(1, 1f, .3f, 0)));
                var c = rings.colorOverLifetime; c.enabled = true; c.color = Fade(.08f, .35f);
            }
            if (foam)
            {
                var sz = foam.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, .55f, 1, 1f));
                var c = foam.colorOverLifetime; c.enabled = true; c.color = Fade(.06f, .45f);
                var rot = foam.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-.08f, .08f);
            }
            if (mist)
            {
                var sz = mist.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, .55f, 1, 1.5f));
                var c = mist.colorOverLifetime; c.enabled = true; c.color = Fade(.12f, .45f);
                var rot = mist.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-.5f, .5f);
                var lv = mist.limitVelocityOverLifetime; lv.enabled = true; lv.dampen = .08f; lv.limit = 1.5f;
            }
            if (boil)
            {
                var sz = boil.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, .3f, 1, 1f));
                var c = boil.colorOverLifetime; c.enabled = true; c.color = Fade(.15f, .3f);
            }
            if (driver) seenFalls = driver.RiverFalls;
        }

        void OnDestroy() { foreach (var p in new[] { drops, rings, foam, boil, mist }) if (p) Destroy(p.transform.parent ? p.transform.parent.gameObject : p.gameObject); if (crown) Destroy(crown.transform.parent ? crown.transform.parent.gameObject : crown); }

        // 21.365e: lưới 3D nước văng — các khung NV_F00…NV_F19 trong FBX (đã áp Solidify/Subsurf trong Blender), vật liệu 0 nước, 1 ụ bọt.
        void BuildCrown(Transform root)
        {
            var src = Resources.Load<GameObject>("NuocVang/PV_NuocVang3D");
            var water = Resources.Load<Material>("NuocVang/PV_NuocVang3D_Nuoc"); var foamMat = Resources.Load<Material>("NuocVang/PV_NuocVang3D_Bot");
            if (!src || !water || !foamMat) { Debug.LogWarning("NuocVang 3D: thiếu FBX/vật liệu"); return; }
            var list = new List<MeshFilter>(src.GetComponentsInChildren<MeshFilter>(true));
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            // Lưới FBX giữ tọa độ Blender (z lên); phép xoay dựng đứng nằm ở transform khung con (−90° quanh x) → dùng lại cho lưới.
            var meshRot = list.Count > 0 ? list[0].transform.localRotation : Quaternion.identity;
            crownFrames = new Mesh[list.Count]; crownHeights = new float[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                crownFrames[i] = list[i].sharedMesh; var b = crownFrames[i].bounds; float top = 0;
                for (int k = 0; k < 8; k++)
                    top = Mathf.Max(top, (meshRot * new Vector3((k & 1) == 0 ? b.min.x : b.max.x, (k & 2) == 0 ? b.min.y : b.max.y, (k & 4) == 0 ? b.min.z : b.max.z)).y);
                crownHeights[i] = Mathf.Max(.3f, top);
            }
            crown = new GameObject("NuocVang_3D"); crown.transform.SetParent(root, false);
            var mesh = new GameObject("Luoi"); mesh.transform.SetParent(crown.transform, false); mesh.transform.localRotation = meshRot;
            crownMf = mesh.AddComponent<MeshFilter>(); crownMr = mesh.AddComponent<MeshRenderer>();
            crownMr.sharedMaterials = new[] { water, foamMat }; crownMr.shadowCastingMode = ShadowCastingMode.Off; crownMr.receiveShadows = false;
            crownMr.lightProbeUsage = LightProbeUsage.Off; crownMr.reflectionProbeUsage = ReflectionProbeUsage.BlendProbesAndSkybox;
            crownMpb = new MaterialPropertyBlock();
            crown.SetActive(false);
        }

        IEnumerator PlayCrown(Vector3 c, Vector3 fwd, float k)
        {
            var tr = crown.transform; float scale = Mathf.Lerp(.85f, 1.25f, k);
            tr.position = c; tr.rotation = Quaternion.LookRotation(fwd) * Quaternion.Euler(0, -90, 0);   // trục dài bầu dục (x Blender) theo thân xe
            tr.localScale = Vector3.one * scale; crown.SetActive(true);
            for (float t = 0; t < CrownSeconds; t += Time.deltaTime)
            {
                int f = Mathf.Clamp((int)(t / CrownSeconds * crownFrames.Length), 0, crownFrames.Length - 1);
                crownMf.sharedMesh = crownFrames[f];
                crownMpb.SetFloat("_CrownH", crownHeights[f] * scale);
                crownMpb.SetFloat("_Fade", Mathf.Clamp01((CrownSeconds - t) / .25f));      // tắt dần 0,25 s cuối
                crownMr.SetPropertyBlock(crownMpb, 0); crownMr.SetPropertyBlock(crownMpb, 1);
                yield return null;
            }
            crown.SetActive(false); crownPlay = null;
        }

        // Hiện rõ rồi mờ dần: alpha 0 → 1 trong [0, inEnd], giữ, rồi 1 → 0 từ outStart tới cuối đời hạt.
        static ParticleSystem.MinMaxGradient Fade(float inEnd, float outStart)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                      inEnd > 0 ? new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, inEnd), new GradientAlphaKey(1, outStart), new GradientAlphaKey(0, 1) }
                                : new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, outStart), new GradientAlphaKey(0, 1) });
            return new ParticleSystem.MinMaxGradient(g);
        }

        static ParticleSystem Make(Transform root, string name, Material mat, ParticleSystemRenderMode mode, float gravity, int maxCount)
        {
            if (!mat) return null;
            var go = new GameObject(name); go.transform.SetParent(root, false);
            var p = go.AddComponent<ParticleSystem>(); p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = p.main; m.loop = false; m.playOnAwake = false; m.duration = 1; m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.startSpeed = 0;                                                    // hạt không ghi vận tốc thì đứng yên (mặc định 5 m/s theo trục z làm vòng sóng trôi đi)
            m.gravityModifier = gravity; m.maxParticles = mode == ParticleSystemRenderMode.Stretch ? 160 : 64;
            m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            var em = p.emission; em.enabled = false;
            var sh = p.shape; sh.enabled = false;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.renderMode = mode; r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false; r.sortMode = ParticleSystemSortMode.Distance;
            r.maxParticleSize = 4f;
            if (mode == ParticleSystemRenderMode.HorizontalBillboard) r.sortingFudge = 10;   // nằm sát mặt nước: vẽ trước các hạt đứng
            r.SetActiveVertexStreams(new List<ParticleSystemVertexStream> { ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV });
            p.Play();
            return p;
        }

        void LateUpdate()
        {
            if (!driver) return;
            if (driver.RiverFalls != seenFalls)
            {
                seenFalls = driver.RiverFalls;
                Splash(driver.WaterEntryPoint, driver.WaterEntryVelocity);
            }
            TickAudio();
        }

        // ------------------------------------------------------------ 21.369: âm thanh rơi sông/biển
        void BuildAudio(Transform root)
        {
            AudioSource Src(GameObject g, string clip, bool loop)
            {
                var c = Resources.Load<AudioClip>("NuocVang/" + clip); if (!c) { Debug.LogWarning("NuocVang: thiếu âm " + clip); return null; }
                var a = PhoViet.PhoVietAudio.Route(g.AddComponent<AudioSource>());
                a.clip = c; a.loop = loop; a.playOnAwake = false; a.spatialBlend = 0f; a.dopplerLevel = 0f; a.volume = 0f;   // camera luôn bám sát xe → 2D, giữ stereo
                return a;
            }
            splashAudio = Src(root.gameObject, "PV_AmNuocVang", false);
            sinkAudio = Src(root.gameObject, "PV_AmXeChim", false);
            underAudio = Src(root.gameObject, "PV_AmXeNgap", false);
            body = GetComponent<Collider>();
        }

        void TickAudio()
        {
            float dt = Time.unscaledDeltaTime;
            // nóc xe vừa xuống dưới mặt nước → «ực» + tiếng nghẹt dưới nước; tiếng chìm trên mặt nước tắt nhanh (0,25 s)
            if (driver.Sinking && !submerged && Time.time - splashAt < 6f && body && body.bounds.max.y < OpenWorldDriver.WaterY)
            {
                submerged = true; SubmergedAt = Time.time;
                if (underAudio) { underAudio.Stop(); underAudio.volume = .95f; underAudio.pitch = Random.Range(.96f, 1.04f); underAudio.Play(); }
            }
            if (submerged && sinkAudio && sinkAudio.volume > 0f)
            {
                sinkAudio.volume = Mathf.MoveTowards(sinkAudio.volume, 0f, dt * 3.6f);
                if (sinkAudio.volume <= 0f) sinkAudio.Stop();
            }
            // xe được đưa lên bờ / chơi lại giữa chừng → tắt mọi tiếng nước nhanh
            if (!driver.Sinking && Time.time - splashAt > .3f)
                foreach (var a in new[] { splashAudio, sinkAudio, underAudio })
                    if (a && a.isPlaying) { a.volume = Mathf.MoveTowards(a.volume, 0f, dt * 4f); if (a.volume <= 0f) a.Stop(); }
        }

        void PlaySplashAudio(float k)
        {
            splashAt = Time.time;
            if (splashAudio)
            {   // lao càng mạnh càng to + trầm hơn
                splashAudio.Stop(); splashAudio.volume = Mathf.Lerp(.75f, 1f, k); splashAudio.pitch = Mathf.Lerp(1.08f, .9f, k) * Random.Range(.97f, 1.03f);
                splashAudio.Play();
            }
            if (sinkAudio) { sinkAudio.Stop(); sinkAudio.volume = .9f; sinkAudio.pitch = Random.Range(.96f, 1.04f); sinkAudio.Play(); }
            if (underAudio) underAudio.Stop();
            submerged = false;
        }

        /// Nước văng + sóng chấn động + bọt tại điểm chạm nước; vel = vận tốc lúc chạm (cỡ hiệu ứng theo tốc độ lao).
        public void Splash(Vector3 at, Vector3 vel)
        {
            Splashes++;
            float y = OpenWorldDriver.WaterY, speed = vel.magnitude, k = Mathf.Clamp01(speed / 25f);
            var c = new Vector3(at.x, y, at.z);
            var flat = new Vector3(vel.x, 0, vel.z); var fwd = flat.sqrMagnitude > .01f ? flat.normalized : transform.forward;
            PlaySplashAudio(k);                                                 // 21.369
            if (crown && crownFrames != null && crownFrames.Length > 0)
            {
                if (crownPlay != null) StopCoroutine(crownPlay);
                crownPlay = StartCoroutine(PlayCrown(c, fwd, k));
            }
            if (drops)
            {
                int n = Mathf.RoundToInt(Mathf.Lerp(55, 95, k));
                for (int i = 0; i < n; i++)
                {
                    float a = Random.value * Mathf.PI * 2f; var dir = new Vector3(Mathf.Cos(a) * 1.25f, 0, Mathf.Sin(a));
                    bool far = Random.value < .25f;
                    var v = dir * (far ? Random.Range(3f, 7f) : Random.Range(.8f, 3f)) + Vector3.up * (far ? Random.Range(3f, 7f) : Random.Range(5f, 11f) * Mathf.Lerp(.8f, 1.15f, k)) + fwd * speed * .12f;
                    drops.Emit(new ParticleSystem.EmitParams { position = c + dir * Random.Range(1.2f, 2.6f) + Vector3.up * .3f, velocity = v,
                        startSize = .08f + .22f * Random.value * Random.value, rotation = Random.Range(0, 360f), startLifetime = Random.Range(1.1f, 1.9f), startColor = Color.white }, 1);   // nhiều giọt nhỏ, ít giọt to
                }
            }
            if (mist)
            {
                int n = Mathf.RoundToInt(Mathf.Lerp(12, 20, k));
                for (int i = 0; i < n; i++)
                {
                    float a = Random.value * Mathf.PI * 2f; var dir = new Vector3(Mathf.Cos(a) * 1.2f, 0, Mathf.Sin(a));
                    mist.Emit(new ParticleSystem.EmitParams { position = c + dir * Random.Range(1.5f, 3.5f) + Vector3.up * Random.Range(.4f, 2.2f),
                        velocity = dir * Random.Range(.6f, 2f) + Vector3.up * Random.Range(1f, 3f) + fwd * speed * .05f,
                        startSize = Random.Range(2.5f, 5f), startLifetime = Random.Range(1.4f, 2.3f), rotation = Random.Range(0, 360f), startColor = Color.white }, 1);
                }
            }
            StartCoroutine(Waves(c, k));
        }

        IEnumerator Waves(Vector3 c, float k)
        {
            var surf = c + Vector3.up * .3f;   // mặt sông có sóng ±0,2–0,4 m (OpenWorldRiverWaves.cginc) — đặt thấp hơn là bị nước trùm mất
            // 5. mảng bọt loang ngay chỗ xe chìm
            if (foam) foam.Emit(new ParticleSystem.EmitParams { position = surf + Vector3.up * .01f, startSize = Mathf.Lerp(7f, 10f, k), startLifetime = 7f,
                rotation = Random.Range(0, 360f), startColor = Color.white }, 1);
            // 4. sóng chấn động: 3 vòng nở ra, vòng sau nhỏ và yếu hơn
            float[] size = { 22f, 16f, 11f }, life = { 2.8f, 2.5f, 2.2f }, alpha = { 1f, .8f, .6f };
            for (int i = 0; i < 3; i++)
            {
                if (rings) rings.Emit(new ParticleSystem.EmitParams { position = surf + Vector3.up * (.02f * i), startSize = size[i] * Mathf.Lerp(.85f, 1.15f, k),
                    startLifetime = life[i], rotation = Random.Range(0, 360f), startColor = new Color(1, 1, 1, alpha[i]) }, 1);
                yield return new WaitForSeconds(.32f);
            }
            if (foam) foam.Emit(new ParticleSystem.EmitParams { position = surf + Vector3.up * .015f, startSize = Mathf.Lerp(5f, 7f, k), startLifetime = 6f,
                rotation = Random.Range(0, 360f), startColor = new Color(1, 1, 1, .8f) }, 1);
            // 5. sủi bọt khí suốt lúc xe chìm (~4 s)
            for (float t = 0; t < 4f && boil; t += .12f)
            {
                var off = Random.insideUnitCircle * 1.6f;
                boil.Emit(new ParticleSystem.EmitParams { position = surf + new Vector3(off.x, .03f, off.y), startSize = Random.Range(.6f, 1.6f),
                    startLifetime = Random.Range(.8f, 1.4f), rotation = Random.Range(0, 360f), startColor = new Color(1, 1, 1, Mathf.Lerp(1f, .35f, t / 4f)) }, 1);
                yield return new WaitForSeconds(.12f);
            }
        }
    }
}
