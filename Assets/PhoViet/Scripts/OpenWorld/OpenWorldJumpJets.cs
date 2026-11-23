using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.258 — khói trắng phản lực khi nhảy: 4 vòi phụt thẳng xuống ở 4 góc gầm xe.
    // 21.295 (người dùng: «còn xấu và đơn giản quá») — dựng lại: texture Blender mới Resources/Boost/PV_JumpSmoke4x4 (16 khung khói
    // thể tích Cycles có nắng chiếu: cụm đặc → nở → rách thành sợi → tan; ArtSource/Blender/OpenWorld/ow_jump_smoke.py), mỗi vòi 3 lớp:
    //   (1) lõi phụt trắng sáng ngắn kéo dài theo vận tốc, (2) khói cuộn: phụt xuống nhanh rồi bị cản lại, nở to, xoay chậm, hơi bốc
    //   lên và chạy khung tan dần theo tuổi, (3) khói tỏa: nhánh phụt chéo ra ngoài như luồng khí đập mặt đất.
    // Thêm vòng khói bung sát mặt đất khi PHÓNG (36 cụm lớn tỏa ngang) và vòng nhỏ khi ĐÁP (22 cụm).
    // Khói thả ra nằm lại trong không gian thế giới (xe bay đi, khói ở lại). Shader PhoViet/Soft Particle (màu × texture).
    [DefaultExecutionOrder(110), DisallowMultipleComponent]
    public class OpenWorldJumpJets : MonoBehaviour
    {
        public OpenWorldDriver driver;
        public Material smokeMaterial;
        [Tooltip("Vị trí 4 vòi theo khung xe (x ± nửa rộng, z ± nửa dài)")] public Vector2 nozzleHalf = new Vector2(0.68f, 1.42f);
        public float nozzleHeight = 0.32f;
        [Tooltip("Số khung texture (cột × hàng)")] public Vector2Int sheet = new Vector2Int(4, 4);

        readonly List<ParticleSystem> systems = new List<ParticleSystem>();
        ParticleSystem ring;
        public bool Emitting { get; private set; }
        public int Bursts { get; private set; }
        public int ParticleCount { get { int n = ring ? ring.particleCount : 0; foreach (var p in systems) if (p) n += p.particleCount; return n; } }
        int seenJumps, seenLandings;

        static Gradient Fade(Color a, Color b, float peak, float mid)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(a, 0), new GradientColorKey(b, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(peak, .05f), new GradientAlphaKey(mid, .45f), new GradientAlphaKey(0, 1) });
            return g;
        }

        void Start()
        {
            if (!driver) driver = GetComponent<OpenWorldDriver>();
            if (!smokeMaterial) return;
            foreach (var sx in new[] { -1f, 1f })
                foreach (var sz in new[] { -1f, 1f })
                {
                    var mount = new GameObject("VoiKhoiNhay").transform; mount.SetParent(transform, false);
                    mount.localPosition = new Vector3(sx * nozzleHalf.x, nozzleHeight, sz * nozzleHalf.y);
                    mount.localRotation = Quaternion.Euler(90, 0, 0);                      // phụt thẳng xuống

                    // (1) lõi phụt: khung đặc đầu dãy, sáng, kéo dài
                    var core = Make(mount, "Lõi phụt trắng", 0f, .12f, false);
                    var cm = core.main; cm.startLifetime = new ParticleSystem.MinMaxCurve(.10f, .17f); cm.startSpeed = new ParticleSystem.MinMaxCurve(15, 19);
                    cm.startSize = new ParticleSystem.MinMaxCurve(.16f, .24f); cm.maxParticles = 40;
                    var ce = core.emission; ce.rateOverTime = 150;
                    var cs = core.shape; cs.radius = .04f; cs.angle = 3;
                    var cz = core.sizeOverLifetime; cz.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .7f, 1, 2.2f));
                    var cc = core.colorOverLifetime; cc.color = Fade(new Color(1, .99f, .96f), new Color(.95f, .97f, 1), 1f, .85f);
                    var cr = core.GetComponent<ParticleSystemRenderer>(); cr.renderMode = ParticleSystemRenderMode.Stretch; cr.lengthScale = 2.2f; cr.velocityScale = .025f;
                    systems.Add(core);

                    // (2) khói cuộn: phụt xuống rồi bị cản (giới hạn vận tốc + cản), nở to, bốc lên nhẹ, tan theo khung
                    var bill = Make(mount, "Khói cuộn", 0f, 1f, true);
                    var bm = bill.main; bm.startLifetime = new ParticleSystem.MinMaxCurve(.75f, 1.15f); bm.startSpeed = new ParticleSystem.MinMaxCurve(8, 12);
                    bm.startSize = new ParticleSystem.MinMaxCurve(.45f, .7f); bm.maxParticles = 90;
                    var be = bill.emission; be.rateOverTime = 72;
                    var bs = bill.shape; bs.radius = .07f; bs.angle = 11;
                    var bz = bill.sizeOverLifetime; bz.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .6f), new Keyframe(.18f, 2.2f), new Keyframe(1, 5.4f)));
                    var bc = bill.colorOverLifetime; bc.color = Fade(Color.white, new Color(.86f, .88f, .92f), .72f, .5f);   // trong hơn để các cụm chồng lên nhau thành cột liền
                    var lv = bill.limitVelocityOverLifetime; lv.enabled = true; lv.limit = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 12), new Keyframe(.15f, 3.5f), new Keyframe(1, 1.2f))); lv.dampen = .35f;
                    var bv = bill.velocityOverLifetime; bv.enabled = true; bv.space = ParticleSystemSimulationSpace.World;
                    bv.x = new ParticleSystem.MinMaxCurve(0, 0); bv.z = new ParticleSystem.MinMaxCurve(0, 0); bv.y = new ParticleSystem.MinMaxCurve(.4f, 1.3f);   // khói nóng bốc lên
                    var rot = bill.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-.7f, .7f);
                    var noise = bill.noise; noise.enabled = true; noise.strength = .35f; noise.frequency = .9f; noise.scrollSpeed = .4f; noise.quality = ParticleSystemNoiseQuality.Low;
                    systems.Add(bill);

                    // (3) khói tỏa: nhánh chéo ra ngoài (luồng khí đập mặt đường), nhỏ hơn, tan nhanh
                    var spread = Make(mount, "Khói tỏa", .15f, 1f, true);
                    var sm = spread.main; sm.startLifetime = new ParticleSystem.MinMaxCurve(.45f, .7f); sm.startSpeed = new ParticleSystem.MinMaxCurve(5, 8);
                    sm.startSize = new ParticleSystem.MinMaxCurve(.32f, .5f); sm.maxParticles = 32;
                    var se = spread.emission; se.rateOverTime = 34;
                    var ss = spread.shape; ss.radius = .1f; ss.angle = 38;
                    var sz2 = spread.sizeOverLifetime; sz2.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, .7f, 1, 3.4f));
                    var sc = spread.colorOverLifetime; sc.color = Fade(new Color(.97f, .98f, 1), new Color(.84f, .86f, .9f), .55f, .35f);
                    var sl = spread.limitVelocityOverLifetime; sl.enabled = true; sl.limit = 2.5f; sl.dampen = .3f;
                    var sr = spread.rotationOverLifetime; sr.enabled = true; sr.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    systems.Add(spread);
                }
            // vòng khói mặt đất (bắn bằng Emit khi phóng/đáp)
            var rg = new GameObject("VongKhoiDat").transform; rg.SetParent(transform, false);
            ring = Make(rg, "Vòng khói mặt đất", 0f, 1f, true);
            var rm = ring.main; rm.loop = true; rm.maxParticles = 110; rm.startLifetime = 1.3f;   // luôn chạy (phát xạ tắt) để hạt Emit được mô phỏng
            var re = ring.emission; re.enabled = false;
            var rz = ring.sizeOverLifetime; rz.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .6f), new Keyframe(.2f, 1.5f), new Keyframe(1, 2.8f)));
            var rc = ring.colorOverLifetime; rc.color = Fade(Color.white, new Color(.84f, .86f, .9f), .62f, .4f);
            var rl = ring.limitVelocityOverLifetime; rl.enabled = true; rl.limit = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 9), new Keyframe(.3f, 2f), new Keyframe(1, .6f))); rl.dampen = .25f;
            var rr = ring.rotationOverLifetime; rr.enabled = true; rr.z = new ParticleSystem.MinMaxCurve(-.6f, .6f);
            ring.Play();
            if (driver) { seenJumps = driver.Jumps; seenLandings = driver.Landings; }
        }

        ParticleSystem Make(Transform mount, string name, float frame0, float frameSpan, bool animate)
        {
            var go = new GameObject(name); go.transform.SetParent(mount, false);
            var p = go.AddComponent<ParticleSystem>(); p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = p.main; m.loop = true; m.playOnAwake = false; m.duration = 3; m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;           // vòi dưới gầm xe: không để Unity coi là ngoài màn hình rồi tạm dừng
            m.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var sh = p.shape; sh.shapeType = ParticleSystemShapeType.Cone;
            var size = p.sizeOverLifetime; size.enabled = true;
            var c = p.colorOverLifetime; c.enabled = true;
            var sheetAnim = p.textureSheetAnimation; sheetAnim.enabled = true; sheetAnim.numTilesX = sheet.x; sheetAnim.numTilesY = sheet.y;
            sheetAnim.animation = ParticleSystemAnimationType.WholeSheet;
            if (animate) { sheetAnim.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, frame0, 1, Mathf.Min(.999f, frame0 + frameSpan))); sheetAnim.startFrame = 0; }
            else { sheetAnim.frameOverTime = new ParticleSystem.MinMaxCurve(0); sheetAnim.startFrame = new ParticleSystem.MinMaxCurve(frame0 * sheet.x * sheet.y, (frame0 + frameSpan) * sheet.x * sheet.y); }
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = smokeMaterial; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            r.sortMode = ParticleSystemSortMode.Distance; r.maxParticleSize = 1.5f;
            r.SetActiveVertexStreams(new List<ParticleSystemVertexStream> { ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV });
            return p;
        }

        // Vòng khói tỏa ngang sát mặt đường dưới xe.
        void GroundRing(int count, float speed, float size, float life)
        {
            if (!ring) return;
            var c = transform.position; c.y = driver ? driver.transform.position.y + .15f : c.y;
            for (int i = 0; i < count; i++)
            {
                float a = (i + Random.value * .6f) / count * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                var pos = c + dir * Random.Range(.9f, 1.6f) + Vector3.up * Random.Range(0f, .35f);
                ring.Emit(new ParticleSystem.EmitParams
                {
                    position = pos, velocity = dir * speed * Random.Range(.7f, 1.2f) + Vector3.up * Random.Range(.4f, 1.4f),
                    startSize = size * Random.Range(.75f, 1.25f), startLifetime = life * Random.Range(.8f, 1.15f),
                    rotation = Random.Range(0, 360f), startColor = Color.white
                }, 1);
            }
            Bursts++;
        }

        void LateUpdate()
        {
            if (!driver || systems.Count == 0) return;
            if (driver.Jumps != seenJumps) { seenJumps = driver.Jumps; GroundRing(36, 6.5f, 1.7f, 1.4f); }
            if (driver.Landings != seenLandings) { seenLandings = driver.Landings; if (!driver.Crashed) GroundRing(22, 4f, 1.25f, 1f); }
            bool on = driver.Airborne && !driver.Crashed && !driver.AirKnocked;   // 21.366: tắt luồng phụt khi bị đụng rơi xuống
            if (on && !Emitting) { foreach (var p in systems) p.Play(); Emitting = true; }
            else if (!on && Emitting) { foreach (var p in systems) p.Stop(true, ParticleSystemStopBehavior.StopEmitting); Emitting = false; }
        }
    }
}
