using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // 21.258 — khói trắng phản lực khi nhảy, như CarJumpJetVisual của game cũ: 4 vòi phụt thẳng xuống ở 4 góc gầm xe,
    // mỗi vòi 2 lớp (lõi phụt dày kéo dài + khói mềm nở to, trôi nhẹ), texture Blender Resources/Boost/PV_WhiteBoostSmoke
    // (4 khung), shader PhoViet/Soft Particle. Khói thả ra nằm lại trong không gian thế giới. Bật trong lúc xe đang nhảy.
    [DefaultExecutionOrder(110), DisallowMultipleComponent]
    public class OpenWorldJumpJets : MonoBehaviour
    {
        public OpenWorldDriver driver;
        public Material smokeMaterial;
        [Tooltip("Vị trí 4 vòi theo khung xe (x ± nửa rộng, z ± nửa dài)")] public Vector2 nozzleHalf = new Vector2(0.68f, 1.42f);
        public float nozzleHeight = 0.32f;

        readonly List<ParticleSystem> systems = new List<ParticleSystem>();
        public bool Emitting { get; private set; }
        public int ParticleCount { get { int n = 0; foreach (var p in systems) if (p) n += p.particleCount; return n; } }

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
                    var p = Make(mount, "Khói trắng nâng xe");
                    var m = p.main; m.startLifetime = new ParticleSystem.MinMaxCurve(.24f, .42f); m.startSpeed = new ParticleSystem.MinMaxCurve(10, 14);
                    m.startSize = new ParticleSystem.MinMaxCurve(.18f, .27f); m.maxParticles = 56;
                    var e = p.emission; e.rateOverTime = 100;
                    var sh = p.shape; sh.radius = .055f; sh.angle = 8;
                    var noise = p.noise; noise.enabled = true; noise.strength = .10f; noise.frequency = 2; noise.quality = ParticleSystemNoiseQuality.Low;
                    systems.Add(p);
                    // lõi phụt ngắn, dày, kéo dài theo vận tốc
                    var core = Make(mount, "Lõi phụt trắng");
                    var cm = core.main; cm.startLifetime = new ParticleSystem.MinMaxCurve(.12f, .20f); cm.startSpeed = 14;
                    cm.startSize = new ParticleSystem.MinMaxCurve(.12f, .18f); cm.maxParticles = 48;
                    var ce = core.emission; ce.rateOverTime = 200;
                    var cs = core.shape; cs.radius = .035f; cs.angle = 4;
                    var cr = core.GetComponent<ParticleSystemRenderer>(); cr.renderMode = ParticleSystemRenderMode.Stretch; cr.lengthScale = 1.6f; cr.velocityScale = .02f;
                    systems.Add(core);
                }
        }

        ParticleSystem Make(Transform mount, string name)
        {
            var go = new GameObject(name); go.transform.SetParent(mount, false);
            var p = go.AddComponent<ParticleSystem>(); p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = p.main; m.loop = true; m.playOnAwake = false; m.duration = 3; m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;           // vòi dưới gầm xe: không để Unity coi là ngoài màn hình rồi tạm dừng
            m.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var sh = p.shape; sh.shapeType = ParticleSystemShapeType.Cone;
            var size = p.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, .65f, 1, 3.8f));
            var c = p.colorOverLifetime; c.enabled = true; var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(.88f, .94f, 1), 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(.9f, .06f), new GradientAlphaKey(.6f, .4f), new GradientAlphaKey(0, 1) });
            c.color = g;
            var sheet = p.textureSheetAnimation; sheet.enabled = true; sheet.numTilesX = 4; sheet.numTilesY = 1; sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0, 1);
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = smokeMaterial; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            r.sortMode = ParticleSystemSortMode.Distance;
            r.SetActiveVertexStreams(new List<ParticleSystemVertexStream> { ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV });
            return p;
        }

        void LateUpdate()
        {
            if (!driver || systems.Count == 0) return;
            bool on = driver.Airborne && !driver.Crashed;
            if (on && !Emitting) { foreach (var p in systems) p.Play(); Emitting = true; }
            else if (!on && Emitting) { foreach (var p in systems) p.Stop(true, ParticleSystemStopBehavior.StopEmitting); Emitting = false; }
        }
    }
}
