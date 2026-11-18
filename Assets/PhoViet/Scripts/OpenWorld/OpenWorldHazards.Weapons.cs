using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.258 — tên lửa của người chơi + ô nạp tên lửa trên phố (bản đồ mở). Dựa trên PhoVietPlayerMissiles/MissilePickups cũ:
    // • Vào lượt có 5 quả, tối đa 10 (người dùng chốt). Phím F hoặc nút TÊN LỬA; hồi 0,25 s.
    // • Tự khóa xe tăng (chưa bị hạ) hoặc trực thăng gần nhất phía trước (trong 180 m, lệch ≤ 55°), bay ≥ 70 m/s (= tốc độ xe
    //   + 50), bẻ hướng dò mục tiêu 300°/s, sống 4 s; không có mục tiêu thì bay thẳng. Lửa đuôi/khói = PV_HelicopterMissile.
    // • Trúng xe tăng → nổ, xe tăng thành xác cháy đứng yên (vẫn là vật cản, ngừng bắn), dọn sau 25 s.
    //   Trúng trực thăng → bị hạ: rơi xoáy, chạm đất thì nổ (vụ nổ như đạn pháo, có hố; xe gần đó vẫn có thể hỏng).
    //   Trúng nhà/mặt đất → nổ nhỏ (chỉ hất xe NPC, không làm hỏng xe người chơi).
    // • Ô nạp (Resources/PlayerMissile/PV_MissilePickup): luôn có ~14 ô rải trên mặt đường quanh xe (60–200 m, ưu tiên phía
    //   trước), nhấp nhô và quay về camera; xe chạm (kể cả đang nhảy thấp) → +1 quả nếu chưa đầy 10.
    public partial class OpenWorldHazards
    {
        [Header("Tên lửa người chơi (21.258)")]
        public GameObject pickupPrefab;
        public int startMissiles = 5, maxMissiles = 10, pickupCount = 14;
        public float playerMissileInterval = 0.25f;

        public int PlayerMissiles { get; private set; }
        public int PlayerMissilesFired { get; private set; }
        public int TanksDestroyed { get; private set; }
        public int HelisDowned { get; private set; }
        public int PickupsCollected { get; private set; }
        public bool CanFire => PlayerMissiles > 0 && fireCooldown <= 0f && player && !player.Crashed && !player.frozen;

        bool fireRequest; float fireCooldown, pickupClock, pickupCheck;
        class PMissile { public Transform root; public MissileFlightVisual vis; public Vector3 vel; public float age, speed; public Tank tank; public Heli heli; }
        readonly List<PMissile> pmissiles = new List<PMissile>();
        readonly Stack<PMissile> pmPool = new Stack<PMissile>();
        class Pickup { public Transform root; public bool live; public float phase; }
        readonly List<Pickup> pickups = new List<Pickup>();

        public void RequestFire() { fireRequest = true; }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F)) fireRequest = true;
            PosePickups(Time.deltaTime);
        }

        void ResetWeapons()
        {
            PlayerMissiles = startMissiles; fireCooldown = 0f; fireRequest = false;
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
            TickPickups(dt, pp, pv);
        }

        // ------------------------------------------------------------ bắn
        bool TryFire(Vector3 pv)
        {
            if (!CanFire || !missilePrefab) return false;
            var fwd = player.transform.forward; fwd.y = 0; fwd.Normalize();
            var origin = player.transform.position + fwd * 2.7f + Vector3.up * 0.9f;
            Tank bestT = null; Heli bestH = null; float bd = 180f * 180f;
            foreach (var t in tanks)
            {
                if (!t.root || t.destroyed) continue;
                var d = TankAim(t) - origin; if (Vector3.Dot(d.normalized, fwd) < 0.57f || d.sqrMagnitude >= bd) continue;
                bd = d.sqrMagnitude; bestT = t; bestH = null;
            }
            foreach (var h in helis)
            {
                if (!h.root || h.downed) continue;
                var d = h.root.position - origin; var flat = new Vector3(d.x, 0, d.z);
                if (Vector3.Dot(flat.normalized, fwd) < 0.57f || d.sqrMagnitude >= bd) continue;
                bd = d.sqrMagnitude; bestH = h; bestT = null;
            }
            var dir = bestT != null ? (TankAim(bestT) - origin).normalized : bestH != null ? (bestH.root.position - origin).normalized : fwd;
            var m = pmPool.Count > 0 ? pmPool.Pop() : NewPM(); if (m == null) return false;
            m.speed = Mathf.Max(70f, Mathf.Abs(player.Speed) + 50f); m.vel = dir * m.speed; m.age = 0f; m.tank = bestT; m.heli = bestH;
            m.root.gameObject.SetActive(true); m.root.SetPositionAndRotation(origin, Quaternion.LookRotation(dir));
            if (m.vis) { m.vis.standalone = true; m.vis.Begin(); }
            pmissiles.Add(m);
            PlayerMissiles--; PlayerMissilesFired++; fireCooldown = playerMissileInterval;
            Emit(muzzleFx, origin, dir * 4f, 1.4f, .14f, Color.white);
            PlayAt(launchClip, origin, 0.7f);
            Say(bestT != null ? "TÊN LỬA · ĐÃ KHÓA XE TĂNG" : bestH != null ? "TÊN LỬA · ĐÃ KHÓA TRỰC THĂNG" : "TÊN LỬA · BAY THẲNG", 1.0f);
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
            m.tank = null; m.heli = null; pmPool.Push(m);
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
                var aim = m.tank != null ? TankAim(m.tank) : m.heli != null ? m.heli.root.position : (Vector3?)null;
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
                if (hitT != null) { DestroyTank(hitT, to); pmissiles.RemoveAt(i); RetirePM(m, false); continue; }
                if (hitH != null) { DownHeli(hitH, m.vel); pmissiles.RemoveAt(i); RetirePM(m, false); continue; }
                // nhà/mặt đất (không tính xe người chơi, xe NPC, CSGT)
                bool wall = false; Vector3 wallAt = to;
                int n = Physics.RaycastNonAlloc(from, (to - from).normalized, pmHits, Vector3.Distance(from, to), OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
                for (int k = 0; k < n; k++) if (pmHits[k].collider.attachedRigidbody == null && !pmHits[k].collider.GetComponentInParent<OpenWorldHazardBody>()) { wall = true; wallAt = pmHits[k].point; break; }
                if (wall || m.age > 4f)
                {
                    if (wall) SmallBlast(wallAt);
                    pmissiles.RemoveAt(i); RetirePM(m, false); continue;
                }
                m.root.position = to; m.root.rotation = Quaternion.LookRotation(m.vel);
            }
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

        void DestroyTank(Tank t, Vector3 at)
        {
            t.destroyed = true; t.destroyedAt = t.age; TanksDestroyed++;
            if (t.anim) t.anim.Stop();
            Emit(blastFx, at + Vector3.up * 1.2f, Vector3.zero, 8.5f, 1.0f, Color.white);
            for (int i = 0; i < 30; i++)
                Emit(sparkFx, at, Random.insideUnitSphere * 9f + Vector3.up * 4f, Random.Range(.12f, .3f), Random.Range(.6f, 1.1f), new Color(1, .62f, .2f, 1));
            PlayAt(blastClip, at, 1f);
            if (traffic) NpcKnocked += traffic.KnockAround(at, blastRadius);
            // xác cháy: khói đen + lửa liên tục
            t.fire1 = BurnFx(t.root, new Vector3(0, 2.1f, -0.6f));
            // xác xe tăng sẫm lại
            foreach (var r in t.root.GetComponentsInChildren<Renderer>())
            {
                var b = new MaterialPropertyBlock(); r.GetPropertyBlock(b); b.SetColor("_Color", new Color(.28f, .26f, .24f)); b.SetColor("_PaintColor", new Color(.28f, .26f, .24f)); r.SetPropertyBlock(b);
            }
            Say("XE TĂNG BỊ HẠ · TRÁNH XÁC XE", 1.8f); Note("người chơi hạ xe tăng");
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
            Say("HẠ TRỰC THĂNG · TRÚNG TÊN LỬA", 1.6f); Note("người chơi hạ trực thăng");
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
            else Explode(at, false, true, "TRỰC THĂNG RƠI TRÚNG", "trực thăng rơi");
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
                    Say("NHẶT TÊN LỬA +1 · " + PlayerMissiles + "/" + maxMissiles, 1.2f);
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
