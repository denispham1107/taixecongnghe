using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.300 — tia LAZER của xe người chơi (OpenWorldLaser) trúng phương tiện trên đường:
    // • Xe tăng, khủng long: cộng «độ nóng» theo thời gian tia chạm (xe tăng 0,3 s, khủng long 0,5 s) — trong lúc nung: tia lửa + khói
    //   tại điểm chạm; đủ nóng → nổ tung: quả cầu lửa, tia lửa, khói, hàng trăm mảnh vỡ theo màu vật, lửa cháy tại chỗ; mô hình biến
    //   mất (xe tăng: 21.336 dùng chung DestroyTank với tên lửa, gốc dọn sau 2 s; khủng long: xóa).
    // • Xe NPC (ô tô, xe máy): OpenWorldTraffic.LaserKill → bốc cháy 0,45 s → LaserExploded → LaserBlast ở đây (mảnh màu sơn xe).
    // • Vụ nổ do Lazer KHÔNG tính sát thương cho xe người chơi (khác Explode của đạn pháo); hất xe NPC gần đó như vụ nổ nhỏ.
    public partial class OpenWorldHazards
    {
        public int LaserTankKills { get; private set; }
        public int LaserDinoKills { get; private set; }
        public int LaserBlasts { get; private set; }
        readonly Dictionary<object, Vector2> laserHeat = new Dictionary<object, Vector2>();   // x = độ nóng (s), y = lần chạm cuối
        float nextScorch;

        void HookLaser()
        {
            if (!traffic) return;
            traffic.LaserExploded -= OnLaserExploded; traffic.LaserExploded += OnLaserExploded;
        }

        float Heat(object key, float dt)
        {
            laserHeat.TryGetValue(key, out var h);
            if (Time.time - h.y > 0.3f) h.x = 0f;                 // tia rời đi lâu → nguội lại
            h.x += dt; h.y = Time.time; laserHeat[key] = h;
            return h.x;
        }

        /// <summary>Collider c có thuộc xe tăng/khủng long không; có thì nung/nổ và trả true (tia dừng ở đây).</summary>
        public bool LaserHit(Collider c, Vector3 point, Vector3 dir, float dt)
        {
            if (!c) return false;
            foreach (var t in tanks)
            {
                if (!t.root || t.destroyed || !c.transform.IsChildOf(t.root)) continue;
                LaserScorch(point, dir);
                if (Heat(t, dt) >= 0.3f) LaserTank(t, point);
                return true;
            }
            for (int i = 0; i < dinos.Count; i++)
            {
                var d = dinos[i];
                if (!d.root || !c.transform.IsChildOf(d.root)) continue;
                LaserScorch(point, dir);
                if (Heat(d, dt) >= 0.5f) { LaserDino(d, point); dinos.RemoveAt(i); }
                return true;
            }
            return false;
        }

        /// <summary>Tia lửa + khói tại điểm tia chạm (gọi mỗi khung khi tia trúng vật).</summary>
        public void LaserScorch(Vector3 point, Vector3 dir)
        {
            if (sparkFx) for (int i = 0; i < 2; i++)
                    Emit(sparkFx, point, (-dir + Random.insideUnitSphere * .9f + Vector3.up * .5f) * Random.Range(2f, 6f), Random.Range(.05f, .12f), Random.Range(.15f, .4f), new Color(1, .55f, .3f, 1));
            if (Time.time < nextScorch || !smokeFx) return;
            nextScorch = Time.time + 0.08f;
            Emit(smokeFx, point + Vector3.up * .2f, Vector3.up * Random.Range(.8f, 1.6f) - dir * .4f, Random.Range(.6f, 1.1f), Random.Range(.7f, 1.2f), new Color(.3f, .28f, .27f, .7f));
        }

        void LaserTank(Tank t, Vector3 at)
        {
            DestroyTank(t, at);                                    // 21.336: nổ tung thành mảnh + biến mất (chung với tên lửa)
            LaserTankKills++;
            Note("lazer hạ xe tăng");
        }

        void LaserDino(Dino d, Vector3 at)
        {
            var c = d.root.position + Vector3.up * 2.2f;
            LaserBlast(c, new[] { new Color(.3f, .45f, .22f), new Color(.45f, .36f, .22f), new Color(.6f, .2f, .15f) }, 3.2f, false);
            Emit(blastFx, c + Vector3.up, Vector3.zero, 9f, 1.1f, Color.white);
            Kill(d.root.gameObject, d.rb);
            LaserDinoKills++;
            Say("Hạ khủng long!", "Tia lazer thiêu rụi", 1.8f, OpenWorldToast.Kind.Good, "HẠ GỌN"); Note("lazer hạ khủng long");
        }

        void OnLaserExploded(Vector3 at, Color paint, bool bike)
        {
            var dark = new Color(paint.r * .35f, paint.g * .35f, paint.b * .35f);
            LaserBlast(at, new[] { paint, dark, new Color(.12f, .12f, .13f), new Color(.75f, .85f, .95f) }, bike ? 0.9f : 1.5f, true);
        }

        /// <summary>Nổ tung thành mảnh: quả cầu lửa, tia lửa, khói, mảnh vỡ theo màu, lửa cháy tại chỗ. Không gây sát thương xe người chơi.</summary>
        void LaserBlast(Vector3 at, Color[] tones, float scale, bool knock)
        {
            LaserBlasts++;
            EnsureShards();
            Emit(blastFx, at + Vector3.up * .3f, Vector3.zero, 4.5f * scale, .85f, Color.white);
            for (int i = 0; i < 16 + (int)(10 * scale); i++)
                Emit(sparkFx, at, Random.insideUnitSphere * 8f * Mathf.Sqrt(scale) + Vector3.up * 4f, Random.Range(.1f, .25f), Random.Range(.5f, 1.1f), new Color(1, .6f, .22f, 1));
            for (int i = 0; i < 3 + (int)(2 * scale); i++)
                Emit(smokeFx, at + new Vector3(Random.Range(-1f, 1f), .6f + i * .45f, Random.Range(-1f, 1f)) * Mathf.Sqrt(scale), Vector3.up * Random.Range(1f, 2.2f), Random.Range(2.5f, 4f) * Mathf.Sqrt(scale), Random.Range(1.6f, 2.6f), new Color(.2f, .19f, .18f, .9f));
            if (shards)
            {
                int n = Mathf.RoundToInt(70 * scale);
                for (int i = 0; i < n; i++)
                {
                    float sz = Random.Range(.08f, .3f) * Mathf.Lerp(1f, 1.4f, scale / 3f);
                    var dir = Random.insideUnitSphere + Vector3.up * .9f;
                    shards.Emit(new ParticleSystem.EmitParams
                    {
                        position = at + Random.insideUnitSphere * .5f * scale, velocity = dir.normalized * Random.Range(4f, 13f) * Mathf.Sqrt(scale),
                        startSize3D = new Vector3(sz, sz * Random.Range(.2f, .5f), sz * Random.Range(.5f, 1f)), rotation3D = Random.insideUnitSphere * 180,
                        startColor = tones[i % tones.Length], startLifetime = Random.Range(.8f, 1.6f)
                    }, 1);
                }
            }
            PlayAt(blastClip, at, Mathf.Clamp01(.55f + .15f * scale));
            if (knock && traffic) NpcKnocked += traffic.KnockAround(at, 1.6f);
            // lửa cháy tại chỗ ~4 s rồi tắt dần
            var holder = new GameObject("LuaLazer"); holder.transform.position = new Vector3(at.x, GroundY(at, out _) + .1f, at.z);
            var ps = BurnFx(holder.transform, Vector3.zero);
            if (ps) { ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); var m = ps.main; m.duration = 4f; m.loop = false; ps.Play(); }
            Destroy(holder, 7f);
        }
    }
}
