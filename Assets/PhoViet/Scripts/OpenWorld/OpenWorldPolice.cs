using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // Bản đồ mở — CSGT truy đuổi (21.254, đợt 7 phần 2). Tách hẳn khỏi PhoVietGame (map cuộn cũ), giữ luật cũ:
    // • Đầu lượt (và khi chơi lại) xe tuần tra đứng yên 3 giây ở ~38 m phía sau trên mặt đường, rồi xuất phát từ 0 km/h.
    // • Tìm đường trên đồ thị đường của OpenWorldTraffic (Dijkstra từ vị trí người chơi), chạy lệch phải tim đường,
    //   giảm tốc trước góc cua; khi gần và nhìn thấy xe người chơi thì lao thẳng tới (đón đầu theo vận tốc).
    // • Né xe/nhà/cột bằng 5 tia SphereCast quạt ±36°; bị kẹt thì lùi rồi đi lại; kẹt mãi/lạc quá xa/rơi sông
    //   → cử xe thay thế ở ~90 m phía sau, ngoài khung hình.
    // • Tốc độ: xa thì 25 m/s tăng dần thêm 0,2 m/s mỗi giây tới +11; khi áp sát chỉ bằng ~97 % tốc độ người chơi
    //   (người chơi có ưu thế nhỏ trên đường thoáng như game cũ), bị cản/giảm tốc là bị đuổi kịp.
    // • Va mạnh vào xe/nhà (> 10 m/s, đâm trực diện) → khựng 1,2 giây (như mô tô/xe CSGT cũ ngã 1,2 s).
    // • Bắt: xe người chơi gần như đứng yên cách tâm xe CSGT ≤ 9,5 m (hai đầu xe cách ~2 m), CSGT đang dừng suốt 1,2 giây → khóa lái,
    //   1,1 giây sau hiện phòng giam (ảnh Resources/Jail) và nút CHƠI LẠI.
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(100)]                     // sau OpenWorldTraffic.Start (mạng đường dựng xong mới đặt xe)
    public class OpenWorldPolice : MonoBehaviour
    {
        public OpenWorldTraffic traffic;
        public OpenWorldDriver player;
        [Header("Đèn ưu tiên")]
        public Renderer[] redBeacons, blueBeacons;
        public Renderer[] redHalos, blueHalos;
        public Material beaconOff, beaconRed, beaconBlue;
        [Header("Lái")]
        public float startDelay = 3f, startBehind = 38f;
        public float baseSpeed = 25f, maxBoost = 11f, boostRate = 0.2f;
        public float accel = 8.5f, brakeDecel = 13f, steerDegPerSec = 115f;
        public float halfLength = 1.87f, halfWidth = 0.78f;
        [Tooltip("21.260: khối lượng (tấn) khi húc xe NPC — CSGT không bị văng/nổ, chỉ khựng")] public float massTons = 1.5f;
        [Header("Bắt")]
        public float arrestDistance = 9.5f, arrestHold = 1.2f, jailDelay = 1.1f;

        public enum State { Waiting, Chasing, Stunned, Reversing, Sinking, Arrested }
        public State Mode { get; private set; }
        public float Speed { get; private set; }
        public float Distance { get; private set; }
        public float ChaseTime { get; private set; }
        public float WaitRemaining => Mathf.Max(0f, waitUntil - Time.time);
        public bool Jailed => Mode == State.Arrested && Time.time - arrestedAt >= jailDelay;
        public int Stuns { get; private set; }
        public int Respawns { get; private set; }
        public int Reverses { get; private set; }
        public int Arrests { get; private set; }
        public bool DirectChase { get; private set; }
        public int Runs { get; private set; }
        // 21.257: OpenWorldGame vẽ thẻ phòng giam + khoảng cách CSGT trên thanh HUD của nó.
        [System.NonSerialized] public bool drawJail = true, showDistance = true;
        public void Halt() { if (siren) siren.Stop(); Speed = 0f; if (rb) rb.linearVelocity = Vector3.zero; }

        Rigidbody rb; Rigidbody playerRb; Collider[] own;
        AudioSource siren;
        Texture2D jailWide, jailTall;
        Vector3 playerStartPos; Quaternion playerStartRot;
        float waitUntil, stateUntil, arrestTimer, arrestedAt = -99f, stuckTime, lastStuck = -99f, nextPlan;
        int stuckCount;
        float reverseSteer;

        // đồ thị đường
        readonly List<Vector3> gp = new List<Vector3>();
        readonly List<Vector3Int> ge = new List<Vector3Int>();
        List<int>[] adj; float[] dist; int[] pred;
        readonly List<Vector3> path = new List<Vector3>();
        readonly List<float> pathOff = new List<float>();
        bool graphReady;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 1500f; rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            own = GetComponentsInChildren<Collider>();
            jailWide = Resources.Load<Texture2D>("Jail/JailWide");
            jailTall = Resources.Load<Texture2D>("Jail/JailTall");
            BuildSiren();
        }

        void Start()
        {
            if (player) { playerRb = player.GetComponent<Rigidbody>(); playerStartPos = player.transform.position; playerStartRot = player.transform.rotation; }
            if (traffic) traffic.blockers.Add(rb);
            ResetChase(false);
        }

        void OnDestroy() { if (traffic) traffic.blockers.Remove(rb); }

        bool EnsureGraph()
        {
            if (graphReady) return true;
            if (!traffic || traffic.NodeCount == 0) return false;
            traffic.RoadGraph(gp, ge);
            adj = new List<int>[gp.Count];
            for (int i = 0; i < gp.Count; i++) adj[i] = new List<int>();
            for (int e = 0; e < ge.Count; e++) { adj[ge[e].x].Add(e); adj[ge[e].y].Add(e); }
            dist = new float[gp.Count]; pred = new int[gp.Count];
            graphReady = gp.Count > 0;
            return graphReady;
        }

        // ------------------------------------------------------------ bắt đầu / chơi lại
        public void ResetChase(bool resetPlayer)
        {
            if (resetPlayer && player) { player.ResetTo(playerStartPos, playerStartRot); Runs++; }   // Runs: OpenWorldHazards dọn hiểm họa khi chơi lại
            if (player) player.frozen = false;
            Mode = State.Waiting; waitUntil = Time.time + startDelay; ChaseTime = 0; Speed = 0;
            arrestTimer = 0; arrestedAt = -99f; stuckTime = 0; stuckCount = 0;
            if (siren) siren.Stop();
            if (player)
            {
                var f = player.transform.forward; f.y = 0;
                if (!PlaceOnRoad(player.transform.position, f.normalized, startBehind, false))
                    Teleport(player.transform.position - f.normalized * startBehind + Vector3.up * 0.3f, Quaternion.LookRotation(f.normalized));
            }
        }

        void Teleport(Vector3 pos, Quaternion rot)
        {
            rb.position = pos; rb.rotation = rot; transform.SetPositionAndRotation(pos, rot);
            rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; rb.useGravity = true; Speed = 0;
        }

        // Đặt xe trên làn phải của một đoạn đường cách người chơi ~`behind` m, ưu tiên phía sau; hidden = ngoài khung hình.
        bool PlaceOnRoad(Vector3 pp, Vector3 pfwd, float behind, bool hidden)
        {
            if (!EnsureGraph()) return false;
            var cam = Camera.main;
            float best = float.MaxValue; Vector3 bestP = Vector3.zero, bestD = Vector3.forward;
            for (int e = 0; e < ge.Count; e++)
            {
                Vector3 a = gp[ge[e].x], b = gp[ge[e].y]; float len = Vector3.Distance(a, b);
                if (len < 1f) continue;
                var d = (b - a) / len;
                float off = ge[e].z == 1 ? 3.5f : 1.75f;
                for (float t = 8f; t < len - 8f; t += 4f)
                {
                    var c = a + d * t;
                    float r = Vector3.Distance(new Vector3(c.x, 0, c.z), new Vector3(pp.x, 0, pp.z));
                    if (r < behind - 15f || r > behind + 25f) continue;
                    var to = (pp - c); to.y = 0;
                    var dir = Vector3.Dot(d, to) >= 0 ? d : -d;                    // hướng về phía người chơi
                    var right = new Vector3(dir.z, 0, -dir.x);
                    var q = c + right * off; q.y = 0;
                    if (hidden && cam)
                    {
                        var vp = cam.WorldToViewportPoint(q + Vector3.up);
                        if (vp.z > 0 && vp.x > -0.1f && vp.x < 1.1f && vp.y > -0.1f && vp.y < 1.1f) continue;
                    }
                    if (Occupied(q + Vector3.up * 1.0f, new Vector3(halfWidth + 0.4f, 0.7f, halfLength + 1.5f), Quaternion.LookRotation(dir))) continue;
                    float score = Mathf.Abs(r - behind) + (1f + Vector3.Dot(pfwd, (q - pp).normalized)) * 25f;   // phía sau được ưu tiên
                    if (score < best) { best = score; bestP = q; bestD = dir; }
                }
            }
            if (best == float.MaxValue) return false;
            Teleport(bestP + Vector3.up * 0.25f, Quaternion.LookRotation(bestD));
            return true;
        }

        readonly Collider[] overlap = new Collider[16];
        bool Occupied(Vector3 c, Vector3 half, Quaternion rot)
        {
            int n = Physics.OverlapBoxNonAlloc(c, half, overlap, rot, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                bool mine = false; foreach (var o in own) if (overlap[i] == o) { mine = true; break; }
                if (!mine) return true;                               // thân xe CSGT ở chỗ cũ không tính là vật chắn
            }
            return false;
        }

        // ------------------------------------------------------------ tìm đường
        static float ProjectSeg(Vector3 p, Vector3 a, Vector3 b, out Vector3 q, out float t)
        {
            var ab = b - a; ab.y = 0; float l2 = Mathf.Max(1e-4f, ab.sqrMagnitude);
            var ap = p - a; ap.y = 0;
            t = Mathf.Clamp01((ap.x * ab.x + ap.z * ab.z) / l2);
            q = a + ab * t; q.y = 0;
            var dq = p - q; dq.y = 0; return dq.sqrMagnitude;
        }

        int NearestEdge(Vector3 p, out Vector3 q, out float t)
        {
            int best = -1; float bd = float.MaxValue; q = p; t = 0;
            for (int e = 0; e < ge.Count; e++)
            {
                float d = ProjectSeg(p, gp[ge[e].x], gp[ge[e].y], out var qq, out var tt);
                if (d < bd) { bd = d; best = e; q = qq; t = tt; }
            }
            return best;
        }

        void Plan(Vector3 pos, Vector3 pp)
        {
            path.Clear(); pathOff.Clear();
            int pe = NearestEdge(pp, out var pq, out var pt);
            int me = NearestEdge(pos, out var mq, out var mt);
            if (pe < 0 || me < 0) { path.Add(pp); pathOff.Add(0); return; }
            for (int i = 0; i < gp.Count; i++) { dist[i] = float.MaxValue; pred[i] = -2; }
            int pa = ge[pe].x, pb = ge[pe].y;
            dist[pa] = Vector3.Distance(gp[pa], pq); pred[pa] = -1;
            dist[pb] = Vector3.Distance(gp[pb], pq); pred[pb] = -1;
            var done = new bool[gp.Count];
            for (int it = 0; it < gp.Count; it++)
            {
                int u = -1; float du = float.MaxValue;
                for (int i = 0; i < gp.Count; i++) if (!done[i] && dist[i] < du) { du = dist[i]; u = i; }
                if (u < 0) break; done[u] = true;
                foreach (int e in adj[u])
                {
                    int w = ge[e].x == u ? ge[e].y : ge[e].x;
                    float nd = du + Vector3.Distance(gp[u], gp[w]);
                    if (nd < dist[w]) { dist[w] = nd; pred[w] = u; }
                }
            }
            path.Add(mq); pathOff.Add(EdgeOff(me));
            if (me != pe)
            {
                int ma = ge[me].x, mb = ge[me].y;
                float ca = Vector3.Distance(mq, gp[ma]) + dist[ma], cb = Vector3.Distance(mq, gp[mb]) + dist[mb];
                int n = ca <= cb ? ma : mb, guard = 0;
                while (n >= 0 && guard++ < 80)
                {
                    path.Add(gp[n]);
                    int nx = pred[n];
                    pathOff.Add(nx >= 0 ? EdgeOffBetween(n, nx) : EdgeOff(pe));
                    if (nx < 0) break;
                    n = nx;
                }
            }
            path.Add(pq); pathOff.Add(0f);
            path.Add(new Vector3(pp.x, 0, pp.z)); pathOff.Add(0f);
        }

        // Khoảng vượt ra ngoài lòng đường (m) của một điểm: 0 nếu còn trong mặt đường (chừa 1,2 m sát lề).
        float OffRoad(Vector3 p)
        {
            float best = float.MaxValue;
            for (int e = 0; e < ge.Count; e++)
            {
                float d2 = ProjectSeg(p, gp[ge[e].x], gp[ge[e].y], out _, out _);
                float half = ge[e].z == 1 ? 7f : 3.5f;
                float over = Mathf.Sqrt(d2) - (half - 1.2f);
                if (over < best) best = over;
                if (best <= 0f) return 0f;
            }
            return Mathf.Max(0f, best);
        }

        float EdgeOff(int e) => ge[e].z == 1 ? 3.0f : 1.5f;
        float EdgeOffBetween(int a, int b)
        {
            foreach (int e in adj[a]) if (ge[e].x == b || ge[e].y == b) return EdgeOff(e);
            return 1.5f;
        }

        // Điểm nhìn trước dọc đường đi (lệch phải theo đoạn chứa điểm đó) + khoảng tới góc cua kế và độ gắt góc đó.
        Vector3 LookAhead(Vector3 pos, float ahead, out float cornerDist, out float cornerAngle)
        {
            cornerDist = 999f; cornerAngle = 0f;
            if (path.Count < 2) return path.Count == 1 ? path[0] : pos;
            // bỏ qua các đỉnh đã đi qua
            float walked = 0; Vector3 target = path[path.Count - 1];
            bool found = false; float acc = 0;
            for (int i = 0; i + 1 < path.Count; i++)
            {
                Vector3 a = i == 0 ? new Vector3(pos.x, 0, pos.z) : path[i], b = path[i + 1];
                float l = Vector3.Distance(a, b);
                if (i >= 1 && cornerDist > 900f && i + 1 < path.Count)
                {
                    var d0 = path[i] - (i == 1 ? new Vector3(pos.x, 0, pos.z) : path[i - 1]); var d1 = b - path[i];
                    d0.y = d1.y = 0;
                    if (d0.sqrMagnitude > 1f && d1.sqrMagnitude > 1f)
                    {
                        float ang = Vector3.Angle(d0, d1);
                        if (ang > 35f) { cornerDist = acc; cornerAngle = ang; }
                    }
                }
                if (!found && walked + l >= ahead)
                {
                    var d = (b - a).normalized; var right = new Vector3(d.z, 0, -d.x);
                    target = a + d * (ahead - walked) + right * pathOff[i];
                    found = true;
                }
                walked += l; acc += l;
                if (found && cornerDist < 900f) break;
            }
            return target;
        }

        // ------------------------------------------------------------ lái
        static readonly float[] Fan = { 0f, -18f, 18f, -36f, 36f };
        readonly RaycastHit[] hits = new RaycastHit[16];
        float Free(Vector3 origin, Vector3 dir, float len)
        {
            int n = Physics.SphereCastNonAlloc(origin, 0.65f, dir, hits, len, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
            float best = len;
            for (int i = 0; i < n; i++)
            {
                var h = hits[i]; if (h.distance <= 0f) continue;
                bool mine = false; foreach (var c in own) if (h.collider == c) { mine = true; break; }
                if (mine) continue;
                if (h.normal.y > 0.7f) continue;                      // mặt đường/dốc cầu
                if (h.distance < best) best = h.distance;
            }
            return best;
        }

        bool LineOfSight(Vector3 a, Vector3 b)
        {
            var d = b - a; float l = d.magnitude; if (l < 0.1f) return true;
            int n = Physics.RaycastNonAlloc(a, d / l, hits, l, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++) if (hits[i].collider.attachedRigidbody == null && hits[i].normal.y < 0.7f) return false;
            return true;
        }

        void FixedUpdate()
        {
            if (!player) return;
            float dt = Time.fixedDeltaTime, now = Time.time;
            var pos = rb.position; var pp = player.transform.position;
            var fwd = transform.forward; fwd.y = 0; fwd.Normalize();
            Distance = Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(pp.x, 0, pp.z));
            var pvel = playerRb ? playerRb.linearVelocity : Vector3.zero; pvel.y = 0;
            float pSpeed = Mathf.Min(Mathf.Abs(player.Speed), pvel.magnitude);   // húc tường mà vẫn đạp ga = đứng yên
            float desired = 0f, steerAngle = 0f;

            // rơi sông
            if (Mode != State.Sinking && pos.y < OpenWorldDriver.WaterY + 0.3f) { Mode = State.Sinking; stateUntil = now + 2.5f; if (siren) siren.Stop(); }
            switch (Mode)
            {
                case State.Sinking:
                    Speed = 0; rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.9f, Mathf.Max(rb.linearVelocity.y, -1.2f), rb.linearVelocity.z * 0.9f);
                    if (now > stateUntil) Replace();
                    return;
                case State.Waiting:
                    if (now >= waitUntil) { Mode = State.Chasing; if (siren) siren.Play(); }
                    break;
                case State.Stunned:
                    if (now >= stateUntil) Mode = State.Chasing;
                    break;
                case State.Reversing:
                    if (now >= stateUntil) Mode = State.Chasing;
                    else desired = -6f;
                    break;
                case State.Arrested:
                    desired = 0f;
                    break;
            }

            if (Mode == State.Chasing)
            {
                ChaseTime += dt;
                if (!EnsureGraph()) return;
                bool playerStill = pSpeed < 2.5f && !player.Sinking;
                var eye = pos + Vector3.up * 1.0f;
                DirectChase = Distance < 45f && LineOfSight(eye, pp + Vector3.up * 0.8f);
                Vector3 target; float cornerDist = 999f, cornerAng = 0f;
                if (DirectChase)
                {
                    var pv = playerRb ? playerRb.linearVelocity : Vector3.zero; pv.y = 0;
                    float lead = Mathf.Clamp(Distance / Mathf.Max(8f, Speed), 0f, 1.2f) * 0.6f;
                    target = pp + pv * lead;
                }
                else
                {
                    if (now >= nextPlan || path.Count == 0) { Plan(pos, pp); nextPlan = now + 0.4f; }
                    target = LookAhead(pos, 9f + Speed * 0.55f, out cornerDist, out cornerAng);
                }
                var to = target - pos; to.y = 0;
                float targetAngle = to.sqrMagnitude > 0.01f ? Vector3.SignedAngle(fwd, to, Vector3.up) : 0f;

                // tốc độ mục tiêu
                float far = baseSpeed + Mathf.Min(maxBoost, boostRate * ChaseTime);
                float near = Mathf.Max(10f, pSpeed * 0.97f);
                desired = Mathf.Lerp(near, far, Mathf.InverseLerp(15f, 40f, Distance));
                if (cornerDist < 900f)
                {
                    float vc = Mathf.Lerp(12f, 7.5f, Mathf.InverseLerp(40f, 110f, cornerAng));
                    desired = Mathf.Min(desired, Mathf.Sqrt(vc * vc + 2f * brakeDecel * 0.6f * Mathf.Max(0f, cornerDist - 7f)));
                }
                float absA = Mathf.Abs(targetAngle);
                if (absA > 25f) desired = Mathf.Min(desired, Mathf.Lerp(16f, 6f, Mathf.InverseLerp(25f, 90f, absA)));
                if (playerStill && Distance < 16f)
                    desired = Mathf.Min(desired, Mathf.Sqrt(2f * 6f * Mathf.Max(0f, Distance - 5.2f)));

                // né vật cản: quạt 5 tia
                float look = Mathf.Clamp(6f + Speed * 1.1f, 8f, 40f);
                var origin = pos + Vector3.up * 0.75f + fwd * (halfLength - 0.6f);
                float bestScore = float.MinValue, bestAng = 0f, bestFree = look, centerFree = look;
                float clampT = Mathf.Clamp(targetAngle, -36f, 36f);
                bool offRoadOK = DirectChase && Distance < 15f && OffRoad(pp) > 0f;   // người chơi trên vỉa hè/quảng trường
                for (int i = 0; i < Fan.Length; i++)
                {
                    var dir = Quaternion.Euler(0, Fan[i], 0) * fwd;
                    float f = Free(origin, dir, look);
                    if (i == 0) centerFree = f;
                    float score = f - Mathf.Abs(Fan[i] - clampT) * 0.12f * (look / 20f);
                    // 21.254: lách trong lòng đường (được lấn làn ngược chiều), không leo vỉa hè đâm bồn cây/cột.
                    if (!offRoadOK) score -= OffRoad(origin + dir * Mathf.Min(f, look * 0.7f)) * 4f;
                    if (score > bestScore) { bestScore = score; bestAng = Fan[i]; bestFree = f; }
                }
                bool blocked = centerFree < look * 0.95f && !(playerStill && Distance < 9f);
                steerAngle = blocked ? bestAng : targetAngle;
                float freeAhead = blocked ? bestFree : centerFree;
                if (freeAhead < look * 0.95f) desired = Mathf.Min(desired, Mathf.Sqrt(2f * brakeDecel * 0.7f * Mathf.Max(0f, freeAhead - 1.8f)));

                // kẹt → lùi
                float actual = Vector3.Dot(rb.linearVelocity, fwd);
                if (Mathf.Abs(actual) < 0.8f && desired > 2f && !(playerStill && Distance < arrestDistance + 3f)) stuckTime += dt; else stuckTime = 0f;
                if (stuckTime > 1.6f)
                {
                    stuckTime = 0f; Reverses++;
                    if (now - lastStuck > 15f) stuckCount = 0;
                    stuckCount++; lastStuck = now;
                    if (stuckCount >= 4) { Replace(); return; }
                    Mode = State.Reversing; stateUntil = now + 1.3f; Note("kẹt → lùi");
                    reverseSteer = (Mathf.Abs(targetAngle) > 5f ? Mathf.Sign(targetAngle) : (bestAng < 0 ? -1f : 1f)) * 45f;   // độ/giây
                    desired = -6f;
                }
                if (Distance > 320f) { Replace(); return; }

                // bắt
                if (playerStill && Distance < arrestDistance && Mathf.Abs(Speed) < 3.5f)
                {
                    arrestTimer += dt;
                    if (arrestTimer >= arrestHold) { Mode = State.Arrested; arrestedAt = now; Arrests++; player.frozen = true; Note("bắt"); }
                }
                else arrestTimer = 0f;
            }

            // vật lý: như xe người chơi (tốc độ dọc thân + xoay quanh trục đứng)
            if (Mode == State.Waiting || Mode == State.Stunned || Mode == State.Arrested) desired = 0f;
            float rate = desired > Speed ? (Speed < 0 ? brakeDecel : accel) : brakeDecel;
            Speed = Mathf.MoveTowards(Speed, desired, rate * dt);
            float actualV = Vector3.Dot(rb.linearVelocity, fwd);
            if (Mathf.Abs(actualV) < Mathf.Abs(Speed) - 4f) Speed = Mathf.MoveTowards(Speed, actualV, 60f * dt);
            float steerRate = steerDegPerSec * Mathf.Clamp01(Mathf.Abs(Speed) / 5f);
            float steer = Mode == State.Reversing ? reverseSteer * Mathf.Clamp01(Mathf.Abs(Speed) / 3f) * dt
                : Speed > 0.2f ? Mathf.Clamp(steerAngle, -steerRate * dt, steerRate * dt) : 0f;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, steer, 0f));
            var vel = rb.linearVelocity; var tv = fwd * Speed; tv.y = vel.y;
            rb.linearVelocity = Vector3.Lerp(vel, tv, 0.35f);
        }

        void Replace()
        {
            Respawns++; stuckCount = 0; path.Clear(); Note("cử xe thay thế");
            var f = player.transform.forward; f.y = 0;
            if (!PlaceOnRoad(player.transform.position, f.normalized, 90f, true) && !PlaceOnRoad(player.transform.position, f.normalized, 140f, false))
                Teleport(player.transform.position - f.normalized * 90f + Vector3.up * 0.3f, Quaternion.LookRotation(f.normalized));
            Mode = State.Chasing;
            if (siren && !siren.isPlaying) siren.Play();
        }

        // 21.255: đuôi/thân khủng long, vụ nổ đạn pháo → xe tuần tra khựng (như game cũ 1,2 s).
        public void Stun(float seconds, string why)
        {
            if (Mode != State.Chasing && Mode != State.Reversing) return;
            Mode = State.Stunned; stateUntil = Time.time + seconds; Speed = 0f; Stuns++; Note("khựng: " + why);
        }

        void OnCollisionEnter(Collision c)
        {
            if (Mode != State.Chasing) return;
            if (c.rigidbody && c.rigidbody.GetComponent<OpenWorldDriver>()) return;      // chạm xe người chơi: không khựng
            var fwd = transform.forward;
            if (c.relativeVelocity.magnitude > 10f && Vector3.Dot(c.GetContact(0).normal, fwd) < -0.5f)
            { Mode = State.Stunned; stateUntil = Time.time + 1.2f; Speed = 0f; Stuns++; Note("khựng: " + c.collider.name + " " + c.relativeVelocity.magnitude.ToString("F1") + " m/s"); }
        }

        // ------------------------------------------------------------ đèn ưu tiên, còi
        void Update()
        {
            bool on = Mode != State.Sinking;
            int phase = Mathf.FloorToInt(Time.time * 4f) % 2;
            bool red = on && phase == 0, blue = on && phase == 1;
            if (redBeacons != null) foreach (var r in redBeacons) if (r) r.sharedMaterial = red ? beaconRed : beaconOff;
            if (blueBeacons != null) foreach (var r in blueBeacons) if (r) r.sharedMaterial = blue ? beaconBlue : beaconOff;
            if (redHalos != null) foreach (var r in redHalos) if (r) r.enabled = red;
            if (blueHalos != null) foreach (var r in blueHalos) if (r) r.enabled = blue;
            if (Mode == State.Arrested && Jailed && siren && siren.isPlaying) siren.Stop();
            if (drawJail && Mode == State.Arrested && Jailed && (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))) ResetChase(true);
        }

        void BuildSiren()
        {
            siren = PhoVietAudio.Route(gameObject.AddComponent<AudioSource>());
            siren.loop = true; siren.playOnAwake = false; siren.volume = 0.45f;
            siren.spatialBlend = 0.75f; siren.minDistance = 14f; siren.maxDistance = 260f; siren.rolloffMode = AudioRolloffMode.Linear;
            const int hz = 22050; var wave = new float[hz * 2]; float phase = 0;
            // Còi hụ lên xuống như còi xe tuần tra game cũ (650 ± 200 Hz, chu kỳ 1 s).
            for (int i = 0; i < wave.Length; i++)
            {
                float t = (float)i / hz, f = 650 + 200 * Mathf.Sin(t * Mathf.PI * 2);
                phase += f / hz * Mathf.PI * 2; wave[i] = Mathf.Sin(phase) * 0.35f;
            }
            var clip = AudioClip.Create("Còi xe tuần tra", wave.Length, 1, hz, false); clip.SetData(wave, 0); siren.clip = clip;
        }

        // ------------------------------------------------------------ HUD
        GUIStyle big, small, btn;
        State toastMode = (State)(-1); float toastAt, closeAt; bool wasClose;
        void OnGUI()
        {
            if (big == null)
            {
                big = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                small = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperRight };
                btn = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            }
            GUI.depth = -10;                                   // vẽ sau cùng: ảnh phòng giam che chữ HUD khác
            int fs = Mathf.Max(18, Screen.height / 28);
            big.fontSize = Mathf.RoundToInt(fs * 1.5f); small.fontSize = fs; btn.fontSize = fs;
            float W = Screen.width, H = Screen.height;

            if (Jailed && !drawJail) return;
            if (Jailed)
            {
                var tex = W >= H ? jailWide : jailTall;
                if (tex) GUI.DrawTexture(new Rect(0, 0, W, H), tex, ScaleMode.ScaleAndCrop);
                else { GUI.color = new Color(0, 0, 0, 0.85f); GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture); GUI.color = Color.white; }
                Shadowed(new Rect(0, H * 0.12f, W, fs * 3f), "CSGT ĐÃ ĐƯA BẠN VÀO TÙ", big, new Color(1f, 0.85f, 0.3f));
                float bw = Mathf.Max(220, fs * 9), bh = fs * 2.4f;
                if (GUI.Button(new Rect((W - bw) / 2, H * 0.78f, bw, bh), "CHƠI LẠI", btn)) ResetChase(true);
                return;
            }
            // 21.291: thông báo dạng thẻ (OpenWorldToast) thay chữ to có bóng; màu nhấn nháy đỏ/xanh như đèn xe tuần tra.
            bool flash = Mathf.FloorToInt(Time.time * 4f) % 2 == 0;
            var siren = flash ? new Color(1f, .33f, .29f) : new Color(.36f, .66f, 1f);
            if (Mode != toastMode) { toastMode = Mode; toastAt = Time.time; }
            bool close = Mode == State.Chasing && Distance < 15f;
            if (close != wasClose) { wasClose = close; closeAt = Time.time; }
            if (Mode == State.Arrested)
            {
                OpenWorldToast.Show("CSGT", "Bị CSGT bắt!", "Xe đã bị chặn lại", OpenWorldToast.Kind.Danger, Time.time - toastAt, -1f, accentOverride: siren, center: true);
                return;
            }
            if (Mode == State.Waiting)
            {
                int left = Mathf.CeilToInt(WaitRemaining);
                OpenWorldToast.Show("CHUẨN BỊ", "CSGT xuất phát sau " + left + " giây", "Chạy ngay kẻo bị bắt!", OpenWorldToast.Kind.Warn,
                                    Time.time - toastAt, WaitRemaining, badge: left.ToString(), progress: startDelay > 0 ? WaitRemaining / startDelay : -1f);
            }

            var col = Distance < 15f ? siren : Color.white;
            if (showDistance) Shadowed(new Rect(W - 420, 16, 400, fs * 1.6f), "CSGT  " + Mathf.RoundToInt(Distance) + " m", small, col);
            if (close)
                OpenWorldToast.Show("CSGT ÁP SÁT", "Còn " + Mathf.RoundToInt(Distance) + " m", "Tăng tốc hoặc rẽ để cắt đuôi", OpenWorldToast.Kind.Danger,
                                    Time.time - closeAt, -1f, accentOverride: siren);

            // Mũi chỉ hướng xe CSGT khi ngoài khung hình (thường là phía sau).
            var cam = Camera.main;
            if (cam && Mode != State.Sinking)
            {
                var vp = cam.WorldToViewportPoint(transform.position + Vector3.up);
                bool onScreen = vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1;
                if (!onScreen)
                {
                    var rel = cam.transform.InverseTransformDirection(transform.position - cam.transform.position);
                    float ang = Mathf.Atan2(rel.x, rel.z);                // 0 = trước, ±π = sau
                    float x = W / 2 + Mathf.Sin(ang) * W * 0.42f;
                    float sc = OpenWorldToast.Scale;
                    float y = rel.z < 0 ? H - OpenWorldGame.SafeBottomPx - 46f * sc : OpenWorldGame.SafeTopPx + 150f * sc;
                    OpenWorldToast.Chip(x, y, (rel.z < 0 ? "CSGT phía sau  " : "CSGT phía trước  ")   /* 21.295: font web không có ▼/▲ */ + Mathf.RoundToInt(Distance) + " m", siren);
                }
            }
        }

        static void Shadowed(Rect r, string text, GUIStyle st, Color c)
        {
            var o = st.normal.textColor;
            st.normal.textColor = new Color(0, 0, 0, 0.75f); GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, st);
            st.normal.textColor = c; GUI.Label(r, text, st);
            st.normal.textColor = o;
        }

        // ------------------------------------------------------------ kiểm thử
        public readonly List<string> Log = new List<string>();
        void Note(string m) { Log.Add($"{ChaseTime:F1}s {m} @({transform.position.x:F0},{transform.position.z:F0}) d={Distance:F0}"); if (Log.Count > 60) Log.RemoveAt(0); }
        public string Report() => $"mode={Mode} dist={Distance:F1} speed={Speed:F1} direct={DirectChase} chase={ChaseTime:F1}s stuns={Stuns} reverses={Reverses} respawns={Respawns} arrests={Arrests}";
    }
}
