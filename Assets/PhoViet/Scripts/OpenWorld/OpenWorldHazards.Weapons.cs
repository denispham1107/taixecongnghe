using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.258 — tên lửa của người chơi + ô nạp tên lửa trên phố (bản đồ mở). Dựa trên PhoVietPlayerMissiles/MissilePickups cũ:
    // • Vào lượt có 5 quả, tối đa 10 (người dùng chốt). Phím Q (21.383; 21.375 K, trước F) hoặc nút TÊN LỬA; hồi 0,3 s (21.284; trước 0,25 s).
    // • 21.327: mỗi lần bắn 2 quả từ 2 đèn pha, khóa 2 mục tiêu, trừ 2 quả; kho tự hồi 1 quả mỗi 5 s (21.328; 21.327 là 7,2 s, trước 12 s). Xem TryFire.
    // • 21.284: khóa cả tàu trên sông (OpenWorldHazards.Boats) — trúng thì tàu cháy, nổ, chìm.
    // • Tự khóa xe tăng (chưa bị hạ) hoặc trực thăng gần nhất phía trước (trong 180 m, lệch ≤ 55°), bay ≥ 70 m/s (= tốc độ xe
    //   + 50), bẻ hướng dò mục tiêu 300°/s, sống 4 s; không có mục tiêu thì bay thẳng. Lửa đuôi/khói = PV_HelicopterMissile.
    // • Trúng xe tăng → nổ, xe tăng thành xác cháy đứng yên (vẫn là vật cản, ngừng bắn), dọn sau 25 s.
    //   Trúng trực thăng → bị hạ: rơi xoáy, chạm đất thì nổ (vụ nổ như đạn pháo, có hố; xe gần đó vẫn có thể hỏng).
    //   Trúng nhà/mặt đất → nổ nhỏ (chỉ hất xe NPC, không làm hỏng xe người chơi).
    // • Ô nạp (Resources/PlayerMissile/PV_MissilePickup): luôn có ~14 ô rải trên mặt đường quanh xe (60–200 m, ưu tiên phía
    //   trước), nhấp nhô và quay về camera; xe chạm (kể cả đang nhảy thấp) → +1 quả nếu chưa đầy 10.
    // • 21.401 (người dùng, cả chơi thật lẫn TẬP LUYỆN): tên lửa bắn nổ được MỌI xe NPC trên đường (ô tô, xe máy, cả xác cháy) —
    //   trúng thì bốc cháy, bốc khói rồi nổ tung thành mảnh (OpenWorldTraffic.MissileKill). Thứ tự khóa mục tiêu: (1) xe tăng +
    //   trực thăng/máy bay (phía trước ≤ 180 m, thiếu thì mọi phía ≤ 260 m); chỉ khi không còn chiếc nào trong tầm mới tới (2) tàu,
    //   rồi (3) xe trên đường. Không khóa và không nổ xe CSGT (người dùng chọn — tên lửa bay xuyên qua như cũ).
    public partial class OpenWorldHazards
    {
        [Header("Tên lửa người chơi (21.258)")]
        public GameObject pickupPrefab;
        public int startMissiles = 10, maxMissiles = 10, pickupCount = 14;   // 21.295 (người dùng): vào game mang sẵn đầy 10 quả
        [Tooltip("21.284: người dùng chốt 0,3 s giữa hai lần bắn")] public float playerMissileInterval = 0.3f;
        // 21.301 (người dùng): bỏ ô nạp tên lửa trên đường (không sinh, không hiện); thay bằng tự hồi 1 quả mỗi 12 s khi chưa đầy.
        [Tooltip("21.301: tắt = không rải ô nạp tên lửa trên đường")] public bool pickupsEnabled = false;
        public float missileRegenSeconds = 5f;                           // 21.327: 12 → 7,2 s; 21.328 (người dùng): 7,2 → 5 s
        public float MissileRegenProgress => PlayerMissiles >= maxMissiles ? 1f : Mathf.Clamp01(regenClock / Mathf.Max(0.1f, missileRegenSeconds));
        public int MissilesRegenerated { get; private set; }
        float regenClock;

        public int PlayerMissiles { get; private set; }
        public int PlayerMissilesFired { get; private set; }
        public int TanksDestroyed { get; private set; }
        public int HelisDowned { get; private set; }
        public int PickupsCollected { get; private set; }
        public bool CanFire => PlayerMissiles > 0 && fireCooldown <= 0f && player && !player.Crashed && !player.frozen;

        bool fireRequest; float fireCooldown, pickupClock, pickupCheck;
        class PMissile { public Transform root; public MissileFlightVisual vis; public Vector3 vel; public float age, speed; public Tank tank; public Heli heli; public int boat = -1, car = -1; }
        readonly List<PMissile> pmissiles = new List<PMissile>();
        readonly Stack<PMissile> pmPool = new Stack<PMissile>();
        class Pickup { public Transform root; public bool live; public float phase; }
        readonly List<Pickup> pickups = new List<Pickup>();

        public void RequestFire() { fireRequest = true; }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) fireRequest = true;   // 21.383: phím Q (21.375 K; trước F)
            PosePickups(Time.deltaTime);
        }

        void ResetWeapons()
        {
            PlayerMissiles = startMissiles; fireCooldown = 0f; fireRequest = false; regenClock = 0f;
            foreach (var p in pickups) p.live = false;        // rải lại quanh vị trí mới
            pickupCheck = 0f;
        }
        void ClearWeapons()
        {
            foreach (var m in pmissiles) RetirePM(m, false);
            pmissiles.Clear();
        }

        void TickWeapons(float dt, Vector3 pp, Vector3 pv)
        {
            fireCooldown = Mathf.Max(0f, fireCooldown - dt);
            if (fireRequest) { fireRequest = false; TryFire(pv); }
            TickPlayerMissiles(dt);
            if (pickupsEnabled) TickPickups(dt, pp, pv);
            else foreach (var p in pickups) if (p.live) { p.live = false; if (p.root) p.root.gameObject.SetActive(false); }
            // 21.301: hồi 1 quả mỗi missileRegenSeconds khi chưa đầy (đầy thì đồng hồ đứng ở 0)
            if (PlayerMissiles < maxMissiles)
            {
                regenClock += dt;
                if (regenClock >= missileRegenSeconds) { regenClock -= missileRegenSeconds; PlayerMissiles++; MissilesRegenerated++; }
            }
            else regenClock = 0f;
        }

        // ------------------------------------------------------------ bắn
        // 21.327 (người dùng): mỗi lần bấm bắn CÙNG LÚC 2 tên lửa từ 2 đèn pha, tự khóa 2 mục tiêu khác nhau — ưu tiên mục tiêu
        // trong tầm nhắm (phía trước, ≤ 180 m, lệch ≤ 55°) gần nhất, thiếu thì lấy mục tiêu gần xe nhất mọi phía (≤ 120 m).
        // Chỉ có 1 mục tiêu → cả 2 quả cùng bám nó; không có → 2 quả bay thẳng song song. Trừ 2 quả mỗi lần (còn 1 thì bắn 1).
        // 21.401: tier 0 = xe tăng/trực thăng/máy bay, 1 = tàu, 2 = xe NPC trên đường (v = chỉ số trong OpenWorldTraffic)
        struct Lock { public Tank t; public Heli h; public int b, v, tier; public float d2; public Vector3 aim; }
        readonly List<Lock> lockBuf = new List<Lock>();
        public int LastSalvo { get; private set; }
        public int LastSalvoTargets { get; private set; }

        void GatherLocks(Vector3 origin, Vector3 fwd, bool aimCone, float range, bool priority)
        {
            lockBuf.Clear(); float r2 = range * range;
            bool Ok(Vector3 d) { var f = new Vector3(d.x, 0, d.z); return d.sqrMagnitude < r2 && (!aimCone || Vector3.Dot(f.normalized, fwd) >= 0.57f); }
            if (priority)
            {
                foreach (var t in tanks) { if (!t.root || t.destroyed) continue; var a = TankAim(t); var d = a - origin; if (Ok(d)) lockBuf.Add(new Lock { t = t, b = -1, v = -1, tier = 0, d2 = d.sqrMagnitude, aim = a }); }
                foreach (var h in helis) { if (!h.root || h.downed) continue; var a = h.root.position; var d = a - origin; if (Ok(d)) lockBuf.Add(new Lock { h = h, b = -1, v = -1, tier = 0, d2 = d.sqrMagnitude, aim = a }); }
            }
            else
            {
                if (boats) for (int i = 0; i < boats.Count; i++) { if (!boats.Alive(i)) continue; var a = boats.BoatCenter(i); var d = a - origin; if (Ok(d)) lockBuf.Add(new Lock { b = i, v = -1, tier = 1, d2 = d.sqrMagnitude, aim = a }); }
                if (traffic) for (int i = 0; i < traffic.VehicleSlots; i++)
                    {
                        if (traffic.MissileBody(i, out var a, out _, out _, out _) != 1) continue;
                        var d = a - origin; if (Ok(d)) lockBuf.Add(new Lock { b = -1, v = i, tier = 2, d2 = d.sqrMagnitude, aim = a });
                    }
            }
            lockBuf.Sort((x, y) => x.tier != y.tier ? x.tier.CompareTo(y.tier) : x.d2.CompareTo(y.d2));
        }
        static bool SameLock(Lock a, Lock b) => a.t == b.t && a.h == b.h && a.b == b.b && a.v == b.v;
        public int PriorityLocks { get; private set; }      // 21.401: số mục tiêu ưu tiên (xe tăng/máy bay) trong lần bắn cuối
        public int CarLocks { get; private set; }           // 21.401: số xe đường bị khóa trong lần bắn cuối

        bool TryFire(Vector3 pv)
        {
            if (!CanFire || !missilePrefab) return false;
            var fwd = player.transform.forward; fwd.y = 0; fwd.Normalize();
            var center = player.transform.position + fwd * 2.7f + Vector3.up * 0.9f;
            // chọn tối đa 2 mục tiêu
            var picks = new List<Lock>(2);
            void Add() { foreach (var l in lockBuf) { if (picks.Count >= 2) break; if (picks.Count == 1 && SameLock(picks[0], l)) continue; picks.Add(l); } }
            // 21.401: (1) xe tăng/máy bay trước — phía trước ≤ 180 m, thiếu thì mọi phía ≤ 260 m (tên lửa bay 4 s, ≥ 70 m/s, với tới)
            GatherLocks(center, fwd, true, 180f, true); Add();
            if (picks.Count < 2) { GatherLocks(center, fwd, false, 260f, true); Add(); }
            // (2)(3) không còn xe tăng/máy bay trong tầm → tàu rồi xe trên đường (như cũ: phía trước ≤ 180 m, thiếu thì mọi phía ≤ 120 m)
            if (picks.Count == 0)
            {
                GatherLocks(center, fwd, true, 180f, false); Add();
                if (picks.Count < 2) { GatherLocks(center, fwd, false, 120f, false); Add(); }
            }
            PriorityLocks = 0; CarLocks = 0; foreach (var l in picks) { if (l.tier == 0) PriorityLocks++; else if (l.tier == 2) CarLocks++; }
            int shots = Mathf.Min(2, PlayerMissiles);
            // 2 đèn pha (cùng vị trí đèn của Lazer)
            var lz = player.GetComponent<OpenWorldLaser>();
            var lamp = lz ? lz.lampLocal : new Vector3(0.6f, 0.52f, 1.86f);
            int fired = 0;
            for (int k = 0; k < shots; k++)
            {
                float side = shots == 1 ? 0f : (k == 0 ? -1f : 1f);
                var origin = player.transform.TransformPoint(new Vector3(side * lamp.x, lamp.y + 0.05f, lamp.z + 0.35f));
                Lock? tgt = picks.Count == 0 ? (Lock?)null : picks[Mathf.Min(k, picks.Count - 1)];
                var dir = tgt.HasValue ? (tgt.Value.aim - origin).normalized : fwd;
                var m = pmPool.Count > 0 ? pmPool.Pop() : NewPM(); if (m == null) break;
                m.speed = Mathf.Max(70f, Mathf.Abs(player.Speed) + 50f); m.vel = dir * m.speed; m.age = 0f;
                m.tank = tgt?.t; m.heli = tgt?.h; m.boat = tgt.HasValue ? tgt.Value.b : -1; m.car = tgt.HasValue ? tgt.Value.v : -1;
                m.root.gameObject.SetActive(true); m.root.SetPositionAndRotation(origin, Quaternion.LookRotation(dir));
                if (m.vis) { m.vis.standalone = true; m.vis.Begin(); }
                pmissiles.Add(m); fired++;
                Emit(muzzleFx, origin, dir * 4f, 1.1f, .12f, Color.white);
            }
            if (fired == 0) return false;
            PlayerMissiles -= fired; PlayerMissilesFired += fired; fireCooldown = playerMissileInterval;
            LastSalvo = fired; LastSalvoTargets = picks.Count;
            PlayAt(launchClip, center, 0.8f);
            string Name(Lock l) => l.b >= 0 ? "tàu" : l.v >= 0 ? "xe" : l.t != null ? "xe tăng" : l.h != null ? (l.h.plane ? "máy bay" : "trực thăng") : "?";
            string title = picks.Count >= 2 && fired >= 2 ? (Name(picks[0]) == Name(picks[1]) ? "Khóa 2 " + Name(picks[0]) : "Khóa " + Name(picks[0]) + " + " + Name(picks[1]))
                         : picks.Count == 1 ? "Đã khóa " + Name(picks[0]) : "Bay thẳng";
            Say(title, picks.Count > 0 ? (fired >= 2 ? "2 tên lửa tự bám mục tiêu" : "Tên lửa tự bám mục tiêu") : "Không có mục tiêu trong tầm", 1.0f, OpenWorldToast.Kind.Info, "TÊN LỬA");
            return true;
        }
        static Vector3 TankAim(Tank t) => t.root.TransformPoint(new Vector3(0, 1.2f, -0.9f));

        PMissile NewPM()
        {
            if (!missilePrefab) return null;
            var go = Instantiate(missilePrefab, transform); go.name = "TenLuaNguoiChoi";
            foreach (var col in go.GetComponentsInChildren<Collider>(true)) Destroy(col);
            return new PMissile { root = go.transform, vis = go.GetComponent<MissileFlightVisual>() };
        }
        void RetirePM(PMissile m, bool removeFromList)
        {
            if (m.vis) m.vis.End();
            if (m.root) m.root.gameObject.SetActive(false);
            m.tank = null; m.heli = null; m.boat = -1; m.car = -1; pmPool.Push(m);
            if (removeFromList) pmissiles.Remove(m);
        }

        readonly RaycastHit[] pmHits = new RaycastHit[8];
        void TickPlayerMissiles(float dt)
        {
            for (int i = pmissiles.Count - 1; i >= 0; i--)
            {
                var m = pmissiles[i]; m.age += dt;
                if (m.tank != null && (!m.tank.root || m.tank.destroyed)) m.tank = null;
                if (m.heli != null && (!m.heli.root || m.heli.downed)) m.heli = null;
                if (m.boat >= 0 && !boats.Alive(m.boat)) m.boat = -1;
                Vector3 carAim = Vector3.zero;
                if (m.car >= 0 && (!traffic || traffic.MissileBody(m.car, out carAim, out _, out _, out _) != 1)) m.car = -1;
                var aim = m.boat >= 0 ? boats.BoatCenter(m.boat) : m.car >= 0 ? carAim : m.tank != null ? TankAim(m.tank) : m.heli != null ? m.heli.root.position : (Vector3?)null;
                if (aim.HasValue) { var want = aim.Value - m.root.position; if (want.sqrMagnitude > .001f) m.vel = Vector3.RotateTowards(m.vel, want.normalized * m.speed, 300f * Mathf.Deg2Rad * dt, 0); }
                var from = m.root.position; var to = from + m.vel * dt;
                // trúng xe tăng (hộp thân) / trực thăng (cầu 3,2 m)
                Tank hitT = null; Heli hitH = null;
                foreach (var t in tanks)
                {
                    if (!t.root || t.destroyed) continue;
                    var inv = Quaternion.Inverse(Quaternion.Euler(0, t.root.eulerAngles.y, 0));
                    if (SegHit(from, to, 0.4f, t.root.position + t.root.forward * -0.94f, inv, 1.25f, 2.55f, 2.1f)) { hitT = t; break; }
                }
                if (hitT == null)
                    foreach (var h in helis)
                    {
                        if (!h.root || h.downed) continue;
                        if (DistPointSeg(h.root.position, from, to) < 3.2f) { hitH = h; break; }
                    }
                int hitB = hitT == null && hitH == null ? BoatHit(from, to) : -1;
                if (hitB >= 0) { HitBoat(hitB, to); pmissiles.RemoveAt(i); RetirePM(m, false); continue; }
                if (hitT != null) { DestroyTank(hitT, to); pmissiles.RemoveAt(i); RetirePM(m, false); continue; }
                if (hitH != null) { DownHeli(hitH, m.vel); pmissiles.RemoveAt(i); RetirePM(m, false); continue; }
                // 21.401: trúng xe NPC trên đường (cả xác cháy) → bốc cháy, khói rồi nổ tung thành mảnh. Quả đang bám xe tăng/máy bay/tàu
                // bay vượt qua xe đường (ưu tiên mục tiêu chính); chỉ quả bám xe hoặc bay thẳng mới nổ vào xe.
                int hitV = m.tank == null && m.heli == null && m.boat < 0 ? CarHit(from, to) : -1;
                if (hitV >= 0) { HitCar(hitV, to, m.vel); pmissiles.RemoveAt(i); RetirePM(m, false); continue; }
                // nhà/mặt đất (không tính xe người chơi, xe NPC, CSGT)
                bool wall = false; Vector3 wallAt = to;
                // 21.284: tên lửa khóa tàu bay chúi qua lan can/kè ven sông → không tính va vật tĩnh (giữa lề và sông không có nhà)
                int n = m.boat >= 0 ? 0 : Physics.RaycastNonAlloc(from, (to - from).normalized, pmHits, Vector3.Distance(from, to), OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
                for (int k = 0; k < n; k++) if (pmHits[k].collider.attachedRigidbody == null && !pmHits[k].collider.GetComponentInParent<OpenWorldHazardBody>()) { wall = true; wallAt = pmHits[k].point; break; }
                if (wall || m.age > 4f)
                {
                    if (wall) SmallBlast(wallAt);
                    pmissiles.RemoveAt(i); RetirePM(m, false); continue;
                }
                m.root.position = to; m.root.rotation = Quaternion.LookRotation(m.vel);
            }
        }
        // 21.401: đoạn bay from→to chạm thân xe NPC (viên nang dọc thân xe: bán kính = nửa bề ngang + 0,45 m)
        int CarHit(Vector3 from, Vector3 to)
        {
            if (!traffic) return -1;
            int best = -1; float bd = float.MaxValue;
            for (int i = 0; i < traffic.VehicleSlots; i++)
            {
                if (traffic.MissileBody(i, out var c, out var f, out float hl, out float hw) == 0) continue;
                if ((c - from).sqrMagnitude > 900f && (c - to).sqrMagnitude > 900f) continue;
                f.y = 0f; if (f.sqrMagnitude < .01f) f = Vector3.forward; f.Normalize();
                float reach = Mathf.Max(0f, hl - hw), r = hw + 0.45f;
                var a = c - f * reach; var b = c + f * reach;
                float d = float.MaxValue;
                for (int k = 0; k <= 8; k++) d = Mathf.Min(d, DistPointSeg(Vector3.Lerp(a, b, k / 8f), from, to));
                if (d < r && d < bd) { bd = d; best = i; }
            }
            return best;
        }
        public int CarsHitByMissile { get; private set; }
        void HitCar(int idx, Vector3 at, Vector3 vel)
        {
            if (!traffic.MissileKill(idx, at, vel.sqrMagnitude > .01f ? vel.normalized : Vector3.forward)) return;
            CarsHitByMissile++;
            Emit(blastFx, at + Vector3.up * .4f, Vector3.zero, 4f, .7f, Color.white);
            for (int k = 0; k < 16; k++)
                Emit(sparkFx, at, Random.insideUnitSphere * 7f + Vector3.up * 3f, Random.Range(.1f, .22f), Random.Range(.4f, .9f), new Color(1, .62f, .2f, 1));
            PlayAt(blastClip, at, 0.6f);
            Note("tên lửa hạ xe trên đường");
        }

        static float DistPointSeg(Vector3 p, Vector3 a, Vector3 b)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(1e-5f, ab.sqrMagnitude));
            return Vector3.Distance(p, a + ab * t);
        }

        void SmallBlast(Vector3 at)
        {
            Emit(blastFx, at + Vector3.up * .6f, Vector3.zero, 3.2f, .6f, Color.white);
            for (int i = 0; i < 10; i++)
                Emit(sparkFx, at, Random.insideUnitSphere * 6f + Vector3.up * 2f, Random.Range(.1f, .2f), Random.Range(.4f, .8f), new Color(1, .62f, .2f, 1));
            PlayAt(blastClip, at, 0.45f);
            if (traffic) NpcKnocked += traffic.KnockAround(at, 1.4f);
        }

        // 21.336: xe tăng trúng tên lửa (hoặc Lazer) → NỔ TUNG THÀNH MẢNH và biến mất (trước: để xác sẫm màu cháy, bốc khói 25 s).
        // Mô hình ẩn ngay, tắt va chạm + bỏ khỏi danh sách vật cản giao thông; lửa nhỏ cháy tại chỗ ~4 s (LaserBlast); gốc dọn sau 2 s.
        void DestroyTank(Tank t, Vector3 at)
        {
            t.destroyed = true; t.destroyedAt = t.age; TanksDestroyed++;
            if (t.anim) t.anim.Stop();
            var c = t.root.position + Vector3.up * 1.1f;
            Emit(blastFx, c + Vector3.up * .4f, Vector3.zero, 9.5f, 1.1f, Color.white);
            for (int i = 0; i < 30; i++)
                Emit(sparkFx, at, Random.insideUnitSphere * 9f + Vector3.up * 4f, Random.Range(.12f, .3f), Random.Range(.6f, 1.1f), new Color(1, .62f, .2f, 1));
            PlayAt(blastClip, at, 1f);
            if (traffic) NpcKnocked += traffic.KnockAround(at, blastRadius);
            foreach (var r in t.root.GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (var col in t.root.GetComponentsInChildren<Collider>()) col.enabled = false;
            if (traffic && t.rb) { traffic.blockers.Remove(t.rb); traffic.blockerHalf.Remove(t.rb); }
            LaserBlast(c, new[] { new Color(.33f, .37f, .22f), new Color(.2f, .22f, .15f), new Color(.12f, .12f, .12f), new Color(.45f, .42f, .36f) }, 2.8f, false);
            Say("Hạ xe tăng!", "Xe tăng nổ tung", 1.8f, OpenWorldToast.Kind.Good, "HẠ GỌN"); Note("người chơi hạ xe tăng");
        }

        ParticleSystem BurnFx(Transform parent, Vector3 local)
        {
            if (!explosionMaterial) return null;
            var go = new GameObject("XacChay"); go.transform.SetParent(parent, false); go.transform.localPosition = local;
            go.transform.rotation = Quaternion.identity;
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = ps.main; m.loop = true; m.startLifetime = 2.4f; m.startSpeed = 2.2f; m.startSize = new ParticleSystem.MinMaxCurve(1.6f, 2.8f);
            m.simulationSpace = ParticleSystemSimulationSpace.World; m.gravityModifier = -0.08f; m.maxParticles = 150;
            var em = ps.emission; em.rateOverTime = 16f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 15f; sh.radius = 0.6f; sh.rotation = new Vector3(-90, 0, 0);
            var sheet = ps.textureSheetAnimation; sheet.enabled = true; sheet.numTilesX = 4; sheet.numTilesY = 2;
            sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .45f, 1, .999f));   // lửa → khói
            var col = ps.colorOverLifetime; col.enabled = true; var gr = new Gradient();
            gr.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(.4f, .38f, .36f), 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }); col.color = gr;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = explosionMaterial; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ps.Play(); return ps;
        }

        void DownHeli(Heli h, Vector3 hitVel)
        {
            h.downed = true; HelisDowned++;
            h.vel = h.vel * 0.4f + hitVel.normalized * 6f; h.vel.y = Mathf.Max(h.vel.y, 2f);
            h.spin = new Vector3(Random.Range(-40f, 40f), Random.Range(260f, 380f) * (Random.value < .5f ? -1 : 1), Random.Range(-60f, 60f));
            if (h.vis) h.vis.SetWrecked();
            Emit(blastFx, h.root.position, Vector3.zero, 6f, .8f, Color.white);
            PlayAt(blastClip, h.root.position, 0.9f);
            BurnFx(h.root, Vector3.zero);
            if (h.plane) { PlanesDowned++; planeLostAt = Time.time; }
            Say(h.plane ? "Hạ máy bay!" : "Hạ trực thăng!", "Tên lửa trúng đích", 1.6f, OpenWorldToast.Kind.Good, "HẠ GỌN"); Note(h.plane ? "người chơi hạ máy bay" : "người chơi hạ trực thăng");
        }

        // trực thăng bị hạ: rơi xoáy, chạm đất/nước thì nổ — true = dọn
        bool TickDowned(Heli h, float dt)
        {
            h.vel += Physics.gravity * dt; h.vel.x *= 0.995f; h.vel.z *= 0.995f;
            h.root.position += h.vel * dt;
            h.root.rotation = Quaternion.Euler(h.spin * dt) * h.root.rotation;
            float ground = GroundY(h.root.position, out bool water);
            if (h.root.position.y > ground + 0.8f && h.age < 30f) return false;
            var at = new Vector3(h.root.position.x, ground, h.root.position.z);
            if (water) { Splashes++; for (int k = 0; k < 20; k++) Emit(sparkFx, at + Vector3.up * .2f, new Vector3(Random.Range(-3f, 3f), Random.Range(6f, 11f), Random.Range(-3f, 3f)), Random.Range(.3f, .6f), Random.Range(.6f, 1f), new Color(.8f, .9f, 1f, .9f)); PlayAt(blastClip, at, .4f); }
            else Explode(at, false, true, h.plane ? "MÁY BAY RƠI TRÚNG" : "TRỰC THĂNG RƠI TRÚNG", h.plane ? "máy bay rơi" : "trực thăng rơi");
            return true;
        }

        // ------------------------------------------------------------ ô nạp tên lửa
        void TickPickups(float dt, Vector3 pp, Vector3 pv)
        {
            if (!pickupPrefab) return;
            while (pickups.Count < pickupCount)
            {
                var go = Instantiate(pickupPrefab, transform); go.name = "NapTenLua";
                foreach (var col in go.GetComponentsInChildren<Collider>(true)) Destroy(col);
                go.SetActive(false); pickups.Add(new Pickup { root = go.transform, phase = pickups.Count * 1.7f });
            }
            // nhặt
            foreach (var p in pickups)
            {
                if (!p.live) continue;
                var d = p.root.position - (pp + Vector3.up * 0.7f);
                if (new Vector2(d.x, d.z).magnitude < 2.0f && Mathf.Abs(d.y) < 2.3f && !player.Crashed && !player.frozen && PlayerMissiles < maxMissiles)
                {
                    p.live = false; p.root.gameObject.SetActive(false);
                    PlayerMissiles = Mathf.Min(maxMissiles, PlayerMissiles + 1); PickupsCollected++;
                    Say("Nhặt tên lửa +1", "Trong kho " + PlayerMissiles + "/" + maxMissiles, 1.2f, OpenWorldToast.Kind.Good, "TIẾP TẾ");
                    PlayAt(launchClip, p.root.position, 0.25f);
                }
            }
            // rải lại ô ở xa / đã nhặt — mỗi 0,5 s
            pickupCheck -= dt; if (pickupCheck > 0f) return; pickupCheck = 0.5f;
            if (!EnsureGraph()) return;
            var pf = pv.sqrMagnitude > 4f ? pv.normalized : player.transform.forward; pf.y = 0; pf.Normalize();
            int budget = 3;
            foreach (var p in pickups)
            {
                var rel = p.root.position - pp; rel.y = 0;
                bool stale = !p.live || rel.magnitude > 230f || (Vector3.Dot(rel, pf) < -60f);
                if (!stale || budget-- <= 0) continue;
                if (PlacePickup(p, pp, pf)) { p.live = true; p.root.gameObject.SetActive(true); }
                else { p.live = false; p.root.gameObject.SetActive(false); }
            }
        }

        bool PlacePickup(Pickup p, Vector3 pp, Vector3 pf)
        {
            var cam = Camera.main;
            for (int tries = 0; tries < 24; tries++)
            {
                int e = rnd.Next(ge.Count);
                Vector3 a = gp[ge[e].x], b = gp[ge[e].y]; float len = Vector3.Distance(a, b); if (len < 20f) continue;
                var dir = (b - a) / len; var c = a + dir * Mathf.Lerp(10f, len - 10f, (float)rnd.NextDouble());
                var rel = c - pp; rel.y = 0; float r = rel.magnitude;
                if (r < 60f || r > 200f) continue;
                if (Vector3.Dot(rel / r, pf) < -0.2f && rnd.NextDouble() < 0.75) continue;      // ưu tiên phía trước
                float[] lanes = ge[e].z == 1 ? new[] { -5.25f, -1.75f, 1.75f, 5.25f } : new[] { -1.75f, 1.75f };
                var q = c + new Vector3(dir.z, 0, -dir.x) * lanes[rnd.Next(lanes.Length)];
                if (cam) { var vp = cam.WorldToViewportPoint(q + Vector3.up); if (vp.z > 0 && vp.z < 70f && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1) continue; }   // không hiện ra ngay trước mắt
                bool near = false; foreach (var o in pickups) if (o != p && o.live && Flat(o.root.position - q) < 25f) { near = true; break; }
                if (near) continue;
                float gy = GroundY(q, out bool water); if (water) continue;
                p.root.position = new Vector3(q.x, gy + 1.15f, q.z);
                return true;
            }
            return false;
        }

        void PosePickups(float dt)
        {
            pickupClock += dt; var cam = Camera.main;
            foreach (var p in pickups)
            {
                if (!p.live) continue;
                var pos = p.root.position; pos.y += (Mathf.Sin((pickupClock + p.phase) * 2.6f) * .08f) - (Mathf.Sin((pickupClock - dt + p.phase) * 2.6f) * .08f);
                p.root.position = pos;
                if (cam) { var v = cam.transform.position - p.root.position; v.y = 0; if (v.sqrMagnitude > .01f) p.root.rotation = Quaternion.LookRotation(v, Vector3.up); }
            }
        }

        public int LivePickups { get { int n = 0; foreach (var p in pickups) if (p.live) n++; return n; } }
        public Vector3 NearestPickup(Vector3 from)
        {
            Vector3 best = Vector3.zero; float bd = float.MaxValue;
            foreach (var p in pickups) if (p.live) { float d = (p.root.position - from).sqrMagnitude; if (d < bd) { bd = d; best = p.root.position; } }
            return best;
        }
    }
}
