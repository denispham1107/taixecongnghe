using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.284 — tàu trên sông bắn xe người chơi khi xe lên lề / lối ven sông; người chơi bắn tên lửa hạ được tàu.
    // • «Bờ sông»: xe đứng trên mặt lề (layer MatLe: vỉa hè, lối ven sông) và cách tim sông < riverBankRange (sông rộng ~165 m,
    //   kè cách tim ~83–86 m; lề bên nhà của đường ven sông cách tim > 100 m nên không tính). Trên lòng đường thì không bị bắn.
    // • Mỗi tàu còn chạy trong 25–140 m bắn đạn pháo theo đúng nhịp xe tăng (1,0 → 0,55 s theo tốc độ xe), cùng đường đạn
    //   vòng cung + vòng đỏ báo điểm rơi (FireShell); trúng → «TRÚNG ĐẠN TÀU». Đạn đang bay tối đa maxShellsInFlight.
    // • Tên lửa người chơi khóa tàu như xe tăng/trực thăng (TryFire); trúng thân tàu → cháy (lửa + khói đen), 1,2 s sau nổ tung,
    //   chìm 5 s rồi ẩn; 30 s sau tàu xuất hiện lại ở phía bên kia vòng chạy (OpenWorldBoats).
    public partial class OpenWorldHazards
    {
        [Header("Tàu bắn (21.284)")]
        public OpenWorldBoats boats;
        public float riverBankRange = 100f, boatFireNear = 25f, boatFireFar = 140f;
        public int BoatShells { get; private set; }
        public int BoatsSunk { get; private set; }
        public bool OnRiverBank { get; private set; }

        float[] boatFire;
        readonly List<int> burningBoats = new List<int>();
        static List<List<Vector2>> riverLines;
        float bankNoticeAt = -99f;

        void ResetBoatGuns() { if (boatFire != null) for (int i = 0; i < boatFire.Length; i++) boatFire[i] = 1.5f; }

        // Xe đang trên lề / lối ven sông sát sông?
        public bool IsRiverBank(Vector3 p)
        {
            if (!Physics.Raycast(p + Vector3.up * 0.6f, Vector3.down, 2f, 1 << OpenWorldGroundFit.SurfaceLayer, QueryTriggerInteraction.Ignore)) return false;
            if (riverLines == null) riverLines = OpenWorldBoats.Centerlines();
            var q = new Vector2(p.x, p.z); float best = float.MaxValue;
            foreach (var line in riverLines)
                for (int i = 0; i + 1 < line.Count; i++)
                {
                    Vector2 a = line[i], b = line[i + 1], ab = b - a;
                    float t = Mathf.Clamp01(Vector2.Dot(q - a, ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
                    best = Mathf.Min(best, (q - (a + ab * t)).sqrMagnitude);
                }
            return best < riverBankRange * riverBankRange;
        }

        void TickBoatGuns(float dt, Vector3 pp, Vector3 pv, bool live)
        {
            if (!boats) boats = FindAnyObjectByType<OpenWorldBoats>();
            if (!boats || boats.Count == 0) return;
            if (boatFire == null || boatFire.Length != boats.Count) { boatFire = new float[boats.Count]; ResetBoatGuns(); }
            // tàu đang cháy → tới giờ thì nổ tung
            for (int k = burningBoats.Count - 1; k >= 0; k--)
            {
                int i = burningBoats[k];
                if (!boats.ExplodeDue(i)) continue;
                burningBoats.RemoveAt(k); BoatsSunk++;
                BoatBlast(boats.BoatCenter(i));
            }
            bool bank = live && player.Lift < 2.2f && IsRiverBank(pp);
            if (bank && !OnRiverBank && Time.time - bankNoticeAt > 6f) { Say("TÀU TRÊN SÔNG NỔ SÚNG · RỜI BỜ SÔNG HOẶC BẮN HẠ TÀU", 2.2f); bankNoticeAt = Time.time; }
            OnRiverBank = bank;
            for (int i = 0; i < boats.Count; i++)
            {
                boatFire[i] -= dt;
                if (boatFire[i] > 0f) continue;
                if (!bank || !boats.Alive(i) || player.Crashed || player.frozen || shells.Count >= maxShellsInFlight) { boatFire[i] = 0.25f; continue; }
                var c = boats.BoatCenter(i); var to = pp - c; to.y = 0; float dist = to.magnitude;
                if (dist < boatFireNear || dist > boatFireFar) { boatFire[i] = 0.25f; continue; }
                var muzzle = c + Vector3.up * 1.2f + to / dist * 2f;
                FireShell(muzzle, to / dist, pp, pv, false, "TRÚNG ĐẠN TÀU", "đạn tàu");
                BoatShells++;
                boatFire[i] = Mathf.Lerp(1.0f, 0.55f, Mathf.InverseLerp(8f, 27f, Mathf.Abs(player.Speed)));
            }
        }

        // ---- tên lửa người chơi ↔ tàu
        int NearestBoatTarget(Vector3 origin, Vector3 fwd, ref float bd)
        {
            if (!boats) return -1;
            int best = -1;
            for (int i = 0; i < boats.Count; i++)
            {
                if (!boats.Alive(i)) continue;
                var d = boats.BoatCenter(i) - origin; var flat = new Vector3(d.x, 0, d.z);
                if (Vector3.Dot(flat.normalized, fwd) < 0.57f || d.sqrMagnitude >= bd) continue;
                bd = d.sqrMagnitude; best = i;
            }
            return best;
        }

        // Thân tàu ~10 m: 3 điểm dọc thân, bán kính 2,4 m.
        int BoatHit(Vector3 from, Vector3 to)
        {
            if (!boats) return -1;
            for (int i = 0; i < boats.Count; i++)
            {
                if (!boats.Alive(i)) continue;
                var c = boats.BoatCenter(i); var f = boats.BoatForward(i);
                if ((c - from).sqrMagnitude > 60f * 60f && (c - to).sqrMagnitude > 60f * 60f) continue;
                for (int k = -1; k <= 1; k++) if (DistPointSeg(c + f * (3.4f * k), from, to) < 2.4f) return i;
            }
            return -1;
        }

        void HitBoat(int i, Vector3 at)
        {
            var tr = boats.BoatTransform(i);
            var fx = BurnFx(tr, new Vector3(0, 2.4f, 0));
            if (!boats.Ignite(i, fx ? fx.gameObject : null)) { if (fx) Destroy(fx.gameObject); return; }
            burningBoats.Add(i);
            Emit(blastFx, at + Vector3.up * .8f, Vector3.zero, 5.5f, .7f, Color.white);
            for (int k = 0; k < 18; k++)
                Emit(sparkFx, at, Random.insideUnitSphere * 7f + Vector3.up * 3f, Random.Range(.12f, .26f), Random.Range(.5f, .9f), new Color(1, .62f, .2f, 1));
            PlayAt(blastClip, at, 0.8f);
            Say("TRÚNG TÀU · TÀU BỐC CHÁY", 1.4f); Note("người chơi bắn trúng tàu");
        }

        void BoatBlast(Vector3 at)
        {
            Emit(blastFx, at + Vector3.up * 1.4f, Vector3.zero, 10f, 1.1f, Color.white);
            for (int k = 0; k < 34; k++)
                Emit(sparkFx, at, Random.insideUnitSphere * 10f + Vector3.up * 5f, Random.Range(.14f, .32f), Random.Range(.7f, 1.3f), new Color(1, .6f, .18f, 1));
            for (int k = 0; k < 6; k++)
                Emit(smokeFx, at + new Vector3(Random.Range(-2f, 2f), 1 + k * .6f, Random.Range(-2f, 2f)), Vector3.up * Random.Range(1.2f, 2.6f), Random.Range(3.5f, 5.5f), Random.Range(1.8f, 2.8f), new Color(.2f, .19f, .18f, .9f));
            for (int k = 0; k < 16; k++)                                          // cột nước
                Emit(sparkFx, at + Vector3.down * 1.2f, new Vector3(Random.Range(-3f, 3f), Random.Range(7f, 12f), Random.Range(-3f, 3f)), Random.Range(.3f, .6f), Random.Range(.7f, 1.1f), new Color(.8f, .9f, 1f, .9f));
            PlayAt(blastClip, at, 1f);
            Say("TÀU NỔ TUNG", 1.4f); Note("tàu nổ");
        }
    }
}
