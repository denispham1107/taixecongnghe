using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.259 — hiệu ứng va chạm khi xe người chơi húc xe NPC (bản đồ mở):
    // • Mảnh vỡ thân xe (lưới TankCraterGeometry.Debris + vật liệu hố, 3 tông xám/trắng/xanh) + tia lửa ở điểm chạm, số lượng
    //   và tốc độ theo vận tốc khép — như EmitVehicleImpact của game cũ.
    // • Xe bị húc nổ (OpenWorldTraffic.WreckExploded, 1 s sau va chạm): vụ nổ như đạn pháo (không hố), hất xe NPC quanh đó,
    //   CSGT khựng; xe người chơi còn trong bán kính nổ 2,2 m (chưa nhảy cao) → «XE BỊ HÚC NỔ TUNG», kết thúc lượt.
    // • Xe người chơi húc ở tốc độ tối đa (OpenWorldDriver.ExplodeAt): văng, cháy, 1 s sau nổ tung.
    public partial class OpenWorldHazards
    {
        ParticleSystem shards; Mesh shardMesh; Material shardMaterial;
        public int Impacts { get; private set; }
        public int WreckBlasts { get; private set; }

        void HookTraffic()
        {
            if (!traffic) return;
            traffic.ImpactFx -= OnImpactFx; traffic.ImpactFx += OnImpactFx;
            traffic.WreckExploded -= OnWreckExploded; traffic.WreckExploded += OnWreckExploded;
            OpenWorldCarDent.Broke -= OnCarBroke; OpenWorldCarDent.Broke += OnCarBroke;
        }
        void OnDestroy()
        {
            OpenWorldCarDent.Broke -= OnCarBroke;
            if (!traffic) return;
            traffic.ImpactFx -= OnImpactFx; traffic.WreckExploded -= OnWreckExploded; traffic.LaserExploded -= OnLaserExploded;
        }

        void EnsureShards()
        {
            if (shards || !craterMaterial) return;
            shards = Fx("ManhVoVaCham", null, 1, 1, 1.1f, false);
            var main = shards.main; main.maxParticles = 900;   // 21.300: 320 → 900 (vụ nổ Lazer vỡ nhiều mảnh) main.startSize3D = true; main.startRotation3D = true;
            var spin = shards.rotationOverLifetime; spin.enabled = true; spin.separateAxes = true;
            spin.x = new ParticleSystem.MinMaxCurve(-11f, 11f); spin.y = new ParticleSystem.MinMaxCurve(-14f, 14f); spin.z = new ParticleSystem.MinMaxCurve(-9f, 9f);
            var size = shards.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 1), new Keyframe(.72f, 1), new Keyframe(1, 0)));
            shardMesh = TankCraterGeometry.Debris(); shardMesh.name = "OW_ManhVoThanXe";
            shardMaterial = new Material(craterMaterial) { name = "OW_ManhVoThanXe", color = Color.white };
            shardMaterial.SetFloat("_UseVertexColor", 1);
            var r = shards.GetComponent<ParticleSystemRenderer>(); r.renderMode = ParticleSystemRenderMode.Mesh; r.mesh = shardMesh; r.sharedMaterial = shardMaterial;
        }

        public void ImpactShards(Vector3 point, Vector3 normal, float closing, bool metal = false)
        {
            EnsureShards(); if (!shards) return;
            Impacts++;
            float severity = Mathf.InverseLerp(.35f, 42f, closing);
            int count = Mathf.RoundToInt(Mathf.Lerp(6, 34, severity));
            float speed = Mathf.Lerp(2.5f, 13f, severity);
            for (int i = 0; i < count; i++)
            {
                float sz = Random.Range(.13f, .32f) * (1 + severity * .65f);
                var dir = normal * (i % 2 == 0 ? .65f : -.65f) + Random.insideUnitSphere * .85f + Vector3.up * .62f;
                var tone = metal ? (i % 2 == 0 ? new Color(.1f, .1f, .11f) : new Color(.26f, .26f, .28f))      // 21.261: mảnh sắt hàng rào
                    : i % 3 == 0 ? new Color(.25f, .28f, .32f) : i % 3 == 1 ? new Color(.87f, .91f, .95f) : new Color(.26f, .51f, .76f);
                shards.Emit(new ParticleSystem.EmitParams
                {
                    position = point + Random.insideUnitSphere * .12f, velocity = dir * Random.Range(speed * .55f, speed),
                    startSize3D = new Vector3(sz, sz * .27f, sz * .72f), rotation3D = Random.insideUnitSphere * 180, startColor = tone,
                    startLifetime = Random.Range(.28f, .73f) + severity * .17f
                }, 1);
            }
            int sparks = 3 + Mathf.RoundToInt(severity * 10);
            for (int i = 0; i < sparks; i++)
                Emit(sparkFx, point, (Random.insideUnitSphere + normal * .3f + Vector3.up * .4f) * speed, Random.Range(.05f, .11f), Random.Range(.09f, .24f), new Color(1, .72f, .35f, .95f));
            PlayAt(blastClip, point, Mathf.Lerp(.15f, .5f, severity));
        }

        void OnImpactFx(Vector3 point, Vector3 normal, float closing) => ImpactShards(point, normal, closing);

        // 21.324: ô tô móp/bể (OpenWorldCarDent.Broke) → mảnh tôn cùng màu sơn xe + mảnh kính trong (kính nứt) + mảnh đèn
        // trắng/đỏ (đèn vỡ); số mảnh và tốc độ văng theo mức hỏng. Mảnh kính nhỏ, sáng, rơi nhanh.
        public int CarDebrisBursts { get; private set; }
        void OnCarBroke(Vector3 point, Vector3 outward, float s, Color paint, bool glass, bool lamp)
        {
            EnsureShards(); if (!shards) return;
            CarDebrisBursts++;
            int count = Mathf.RoundToInt(Mathf.Lerp(4, 22, s));
            float speed = Mathf.Lerp(2f, 9f, s);
            var dark = paint * 0.55f; dark.a = 1f;
            for (int i = 0; i < count; i++)
            {
                float sz = Random.Range(.08f, .24f) * (1 + s * .6f);
                var dir = outward * .8f + Random.insideUnitSphere * .7f + Vector3.up * .55f;
                shards.Emit(new ParticleSystem.EmitParams
                {
                    position = point + Random.insideUnitSphere * .15f, velocity = dir * Random.Range(speed * .5f, speed),
                    startSize3D = new Vector3(sz, sz * .2f, sz * .8f), rotation3D = Random.insideUnitSphere * 180,
                    startColor = i % 3 == 2 ? dark : paint, startLifetime = Random.Range(.5f, 1.1f)
                }, 1);
            }
            int bits = (glass ? 26 : 0) + (lamp ? 14 : 0);
            for (int i = 0; i < bits; i++)
            {
                bool isLamp = lamp && (!glass || i >= 26);
                float sz = Random.Range(.03f, .09f);
                var col = isLamp ? (Random.value < .5f ? new Color(1f, .95f, .85f) : new Color(.85f, .08f, .06f)) : new Color(.82f, .9f, .96f);
                var dir = outward * .6f + Random.insideUnitSphere * .9f + Vector3.up * .8f;
                shards.Emit(new ParticleSystem.EmitParams
                {
                    position = point + Vector3.up * (isLamp ? 0f : .5f) + Random.insideUnitSphere * .25f, velocity = dir * Random.Range(1.5f, 5.5f),
                    startSize3D = new Vector3(sz, sz * .25f, sz), rotation3D = Random.insideUnitSphere * 180, startColor = col,
                    startLifetime = Random.Range(.4f, .9f)
                }, 1);
            }
            if (glass) PlayAt(blastClip, point, .18f);
        }

        void OnWreckExploded(Vector3 at, bool bike)
        {
            WreckBlasts++;
            float gy = GroundY(at, out bool water);
            if (water || at.y < OpenWorldDriver.WaterY + 0.5f)
            {
                Splashes++;
                for (int k = 0; k < 14; k++) Emit(sparkFx, at + Vector3.up * .2f, new Vector3(Random.Range(-2f, 2f), Random.Range(5f, 9f), Random.Range(-2f, 2f)), Random.Range(.3f, .6f), Random.Range(.6f, 1f), new Color(.8f, .9f, 1f, .9f));
                PlayAt(blastClip, at, .3f); return;
            }
            Explode(new Vector3(at.x, Mathf.Max(gy, at.y - 0.6f), at.z), false, false, "XE BỊ HÚC NỔ TUNG", bike ? "xe máy nổ" : "ô tô nổ");
        }

        void TickPlayerExplosion()
        {
            if (!player || player.ExplodeAt <= 0f || Time.time < player.ExplodeAt) return;
            var at = player.transform.position;
            player.ConsumeExplosion();
            Emit(blastFx, at + Vector3.up * 1.2f, Vector3.zero, 9f, 1.0f, Color.white);
            for (int i = 0; i < 34; i++)
                Emit(sparkFx, at, Random.insideUnitSphere * 9f + Vector3.up * 4f, Random.Range(.12f, .3f), Random.Range(.6f, 1.1f), new Color(1, .62f, .2f, 1));
            for (int i = 0; i < 5; i++)
                Emit(smokeFx, at + new Vector3(Random.Range(-1f, 1f), 1 + i * .5f, Random.Range(-1f, 1f)), Vector3.up * Random.Range(1f, 2.2f), Random.Range(3.5f, 5f), Random.Range(1.8f, 2.6f), new Color(.25f, .23f, .22f, .9f));
            PlayAt(blastClip, at, 1f);
            // 21.312: nổ tung — mảnh vỡ thân xe văng tứ phía, quả cầu lửa thứ hai, xe bị hất lên lộn vòng
            ImpactShards(at + Vector3.up * 0.8f, Vector3.up, 42f);
            Emit(blastFx, at + Vector3.up * 2.4f, Vector3.up * 2f, 6.5f, 1.1f, new Color(1f, .85f, .7f, 1f));
            for (int i = 0; i < 6; i++)
                Emit(blastFx, at + new Vector3(Random.Range(-1.6f, 1.6f), Random.Range(.6f, 2f), Random.Range(-1.6f, 1.6f)), Random.insideUnitSphere * 2f, Random.Range(3f, 4.5f), Random.Range(.6f, .9f), Color.white);
            player.BlastUp(at - player.transform.forward * 0.3f);
            if (traffic) NpcKnocked += traffic.KnockAround(at, blastRadius + 1f);
            foreach (var pol in OpenWorldPolice.All) if (pol && Flat(pol.transform.position - at) < blastRadius + 2f) pol.Stun(1.2f, "xe người chơi nổ");
            Note("xe người chơi nổ tung");
        }
    }
}
