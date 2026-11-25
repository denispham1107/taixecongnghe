using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.303 — bờ biển bán đảo không còn là đường trốn: xe người chơi vào «lề sát biển» (đường dạo bê tông + vỉa hè phía biển,
    // không tính lòng đường) thì ngay lập tức có đủ coastBoats = 5 tàu ngoài khơi gần xe (OpenWorldBoats.PlaceSeaBoat) và
    // coastPlanes = 2 máy bay cánh kép đỏ (mẫu Crimson Skywing có sẵn, Resources/Skywing/PV_Skywing).
    // • Tàu biển bắn đạn pháo liên tục (cùng nhịp/đường đạn tàu sông, không giới hạn tầm) khi xe ở lề sát biển; xe rời bờ biển thì
    //   tàu ngừng bắn nhưng vẫn tuần tra quanh chỗ cũ, xe quay lại lề biển là bắn ngay (người dùng chọn 22/11/2026).
    // • Máy bay đuổi theo xe đến hết lượt (kể cả khi xe đã vào phố), bay vòng lao tới từng lượt, bắn tên lửa liên tục (21.304: nhịp như xe tăng/tàu,
    //   vòng báo vàng); leo qua mái nhà/cao ốc trên đường bay. Tên lửa người chơi khóa và hạ được máy bay như trực thăng.
    // • Đang ở lề sát biển: mỗi coastRefill giây bù đủ tàu/máy bay đã bị hạ.
    public partial class OpenWorldHazards
    {
        [Header("Bờ biển: tàu chiến + máy bay (21.303)")]
        public int coastBoats = 5, coastPlanes = 2;
        public float planeSpeed = 38f, planeAltitude = 22f, planeTurnRate = 70f, planeScale = 1.5f, coastRefill = 8f;
        [Tooltip("21.304: tên lửa máy bay đang bay tối đa (riêng, không chung giới hạn trực thăng)")] public int planeMaxMissiles = 12;
        public float planeFireRange = 5000f;              // 21.307: xa như đạn tàu (boatFireFar) — thấy máy bay là bị bắn
        public bool OnCoast { get; private set; }
        public int CoastEntries { get; private set; }
        public int PlanesSpawned { get; private set; }
        public int PlanesDowned { get; private set; }
        public int ActivePlanes { get { int n = 0; foreach (var h in helis) if (h.root && h.plane && !h.downed) n++; return n; } }
        float planeLostAt = -99f, nextRefill;
        GameObject planePrefab;
        static readonly Vector3 PlaneLauncherLocal = new Vector3(0f, -0.9f, 1.2f);

        // Ranh giới bán đảo (khớp Editor/PhoVietOpenWorldPeninsulaBuilder: x, z ∈ [−567,5; 700], cửa sông nam x 234–400, tây z 229–400).
        const float CX0 = -567.5f, CX1 = 700f, CZ0 = -567.5f, CZ1 = 700f;      // 21.304: nam/tây cắt vào 32,5 m
        // Bề rộng lề sát biển tính từ chân kè: cạnh bắc/đông đường dạo 0–13 + vỉa hè 13–18 m.
        // 21.304: nam/tây chỉ còn vỉa hè 0–4,5 m sát kè (đường dạo đã cắt bỏ).
        const float CoastSW = 5f, CoastNE = 18f;

        public static bool IsCoast(Vector3 p)
        {
            if (p.x < CX0 || p.x > CX1 || p.z < CZ0 || p.z > CZ1) return false;
            if (p.z - CZ0 <= CoastSW && (p.x < 234.5f || p.x > 400f)) return true;     // nam (trừ cửa sông)
            if (p.x - CX0 <= CoastSW && (p.z < 228.5f || p.z > 400f)) return true;     // tây (trừ cửa sông)
            if (CZ1 - p.z <= CoastNE) return true;                                    // bắc
            if (CX1 - p.x <= CoastNE) return true;                                    // đông
            return false;
        }

        // Cạnh kè gần xe nhất: điểm chiếu trên kè, pháp tuyến ra biển, hướng dọc kè.
        static void NearestSide(Vector3 p, out Vector2 foot, out Vector2 outN, out Vector2 along)
        {
            float dS = p.z - CZ0, dN = CZ1 - p.z, dW = p.x - CX0, dE = CX1 - p.x;
            float m = Mathf.Min(Mathf.Min(dS, dN), Mathf.Min(dW, dE));
            if (m == dS) { foot = new Vector2(p.x, CZ0); outN = Vector2.down; along = Vector2.right; }
            else if (m == dN) { foot = new Vector2(p.x, CZ1); outN = Vector2.up; along = Vector2.right; }
            else if (m == dW) { foot = new Vector2(CX0, p.z); outN = Vector2.left; along = Vector2.up; }
            else { foot = new Vector2(CX1, p.z); outN = Vector2.right; along = Vector2.up; }
        }

        void TickCoast(bool coast, Vector3 pp, Vector3 pv)
        {
            if (coast && !OnCoast)
            {
                CoastEntries++;
                int b = FillSeaBoats(pp), pl = FillPlanes(pp);
                Say("Tàu chiến và máy bay tấn công!", "Lề sát biển bị canh gác · rời bờ biển hoặc bắn hạ", 2.4f, OpenWorldToast.Kind.Danger);
                Note($"vào lề biển: +{b} tàu, +{pl} máy bay");
                nextRefill = Time.time + coastRefill;
            }
            else if (coast && Time.time >= nextRefill)
            {
                nextRefill = Time.time + coastRefill;
                int b = FillSeaBoats(pp), pl = Time.time - planeLostAt > coastRefill * 0.5f ? FillPlanes(pp) : 0;
                if (b + pl > 0) { Say(pl > 0 ? "Máy bay mới lao tới!" : "Tàu chiến mới áp sát!", "Vẫn đang ở lề sát biển", 1.6f, OpenWorldToast.Kind.Warn); Note($"bù: +{b} tàu, +{pl} máy bay"); }
            }
        }

        // Bù đủ coastBoats tàu biển còn chạy: đặt ngoài khơi cạnh kè gần xe, rải dọc kè ±100 m, cách kè 40–110 m.
        int FillSeaBoats(Vector3 pp)
        {
            if (!boats) return 0;
            int need = CoastBoats - boats.SeaAlive, made = 0;   // 21.396: tập luyện 3 tàu
            if (need <= 0) return 0;
            NearestSide(pp, out var foot, out var outN, out var along);
            float[] offs = { 0f, -55f, 55f, -105f, 105f, -150f, 150f };
            for (int k = 0, tries = 0; made < need && tries < 24; tries++, k++)
            {
                float o = offs[k % offs.Length] + Random.Range(-12f, 12f) + (k >= offs.Length ? Random.Range(-40f, 40f) : 0f);
                var c = foot + along * o + outN * (k % 2 == 0 ? Random.Range(42f, 70f) : Random.Range(75f, 110f));
                bool clash = false;
                for (int i = 0; i < boats.Count && !clash; i++)
                    if (boats.IsSea(i) && boats.Alive(i)) { var q = boats.BoatCenter(i); clash = (new Vector2(q.x, q.z) - c).sqrMagnitude < 34f * 34f; }
                if (clash) continue;
                int idx = boats.PlaceSeaBoat(c);
                if (idx < 0) break;
                if (boatFire != null && idx < boatFire.Length) boatFire[idx] = Random.Range(0f, 0.2f);
                made++;
            }
            return made;
        }

        // Bù đủ coastPlanes máy bay: bay vào từ ngoài biển (cách xe ~260 m, cao 45 m), mũi hướng về xe.
        int FillPlanes(Vector3 pp)
        {
            if (!planePrefab) planePrefab = Resources.Load<GameObject>("Skywing/PV_Skywing");
            if (!planePrefab) return 0;
            int need = CoastPlanes - ActivePlanes, made = 0;   // 21.396: tập luyện 1 chiếc bay
            NearestSide(pp, out _, out var outN, out var along);
            for (int k = 0; k < need; k++)
            {
                float side = (ActivePlanes % 2 == 0) ? -1f : 1f;
                // 21.308: xuất hiện PHÍA TRƯỚC mặt xe (~220 m theo hướng xe, lệch ±40 m) rồi lao về phía xe — người chơi thấy ngay
                // máy bay và tên lửa trong khung nhìn (trước: hiện ngoài biển bên hông xe, ngoài khung hình)
                var pf3 = player.transform.forward; pf3.y = 0; pf3 = pf3.sqrMagnitude > .01f ? pf3.normalized : new Vector3(along.x, 0, along.y);
                var r3 = new Vector3(pf3.z, 0, -pf3.x);
                var at3 = pp + pf3 * 220f + r3 * (side * 40f);
                var at2 = new Vector2(at3.x, at3.z);
                var at = new Vector3(at2.x, 30f, at2.y);                                  // 21.308: thấp hơn (45 → 30 m) để không khuất sau thanh thông tin
                var dir = pp - at; dir.y = 0; dir.Normalize();
                var go = Instantiate(planePrefab, at, Quaternion.LookRotation(dir));
                go.name = "MayBayCanhKep"; go.transform.localScale = Vector3.one * planeScale;
                foreach (var col in go.GetComponentsInChildren<Collider>(true)) Destroy(col);
                var sv = go.GetComponent<SkywingVisual>(); if (sv) sv.enabled = false;   // hoạt cảnh cũ cần PhoVietGame → tự quay cánh quạt
                Transform prop = null; foreach (var t in go.GetComponentsInChildren<Transform>(true)) if (t.name == "Front propeller") prop = t;
                var h = new Heli { root = go.transform, plane = true, prop = prop, yaw = go.transform.eulerAngles.y, vel = dir * planeSpeed,
                                   fire = Random.Range(0f, 0.2f) + k * 0.15f, slot = PlanesSpawned % 2 };
                helis.Add(h); PlanesSpawned++; made++;
                Note("máy bay @" + Fmt(at));
            }
            return made;
        }

        // Máy bay: bay lượt tấn công — lái về điểm trước xe (lệch trái/phải theo slot), qua đầu xe thì giữ thẳng ~1,6 s rồi vòng
        // lại; độ cao ≥ planeAltitude và ≥ mái nhà/cao ốc phía trước + 14 m. Bắn tên lửa (FireMissile) theo nhịp xe tăng/tàu khi xe chưa ở hẳn sau đuôi (21.304).
        bool TickPlane(Heli h, float dt, Vector3 pp, Vector3 pv)
        {
            var pos = h.root.position;
            var fwd = Quaternion.Euler(0, h.yaw, 0) * Vector3.forward;
            // độ cao an toàn: dò mái nhà ngay dưới và 45 m phía trước
            float roof = 0f;
            foreach (var probe in new[] { pos, pos + fwd * 45f })
                if (Physics.Raycast(new Vector3(probe.x, 400f, probe.z), Vector3.down, out var hit, 420f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore))
                    roof = Mathf.Max(roof, hit.point.y);
            float alt = Mathf.Max(planeAltitude + (h.slot == 1 ? 5f : 0f), roof + 14f);
            // điểm ngắm: trước xe 1,2 s, lệch ngang ±14 m theo slot
            var pf = pv.sqrMagnitude > 4f ? pv.normalized : player.transform.forward; pf.y = 0; pf.Normalize();
            var right = new Vector3(pf.z, 0, -pf.x);
            // 21.308: trực chiến PHÍA TRƯỚC xe (~85 m, lệch ±18 m) như trực thăng — máy bay vòng lượn ngay trong khung nhìn của
            // người chơi, tên lửa bay về phía camera nên thấy rõ (trước: đuổi theo xe từ phía biển/sau lưng, ngoài khung hình)
            var aim = pp + pf * 85f + pv * 0.5f + right * (h.slot == 0 ? -18f : 18f);
            var toAim = aim - pos; toAim.y = 0; float flat = toAim.magnitude;
            if (flat < 32f && h.pass <= 0f) h.pass = 1.6f;                              // vừa qua đầu xe: giữ thẳng rồi mới vòng lại
            h.pass -= dt;
            float yawRate = 0f;
            if (h.pass <= 0f && flat > 0.1f)
            {
                float want = Mathf.Atan2(toAim.x, toAim.z) * Mathf.Rad2Deg;
                float delta = Mathf.DeltaAngle(h.yaw, want);
                yawRate = Mathf.Clamp(delta * 2.2f, -planeTurnRate, planeTurnRate);
                h.yaw += yawRate * dt;
            }
            h.bank = Mathf.Lerp(h.bank, Mathf.Clamp(-yawRate * 0.7f, -55f, 55f), 1f - Mathf.Exp(-dt * 4f));
            fwd = Quaternion.Euler(0, h.yaw, 0) * Vector3.forward;
            float climb = Mathf.Clamp((alt - pos.y) * 0.9f, -7f, 14f);
            h.vel = fwd * planeSpeed + Vector3.up * climb;
            h.root.position = pos + h.vel * dt;
            float pitch = -Mathf.Atan2(climb, planeSpeed) * Mathf.Rad2Deg;
            h.root.rotation = Quaternion.Euler(pitch, h.yaw, h.bank);
            if (h.prop) { h.propAngle = Mathf.Repeat(h.propAngle + 2270f * dt, 360f); h.prop.localRotation = Quaternion.AngleAxis(h.propAngle, Vector3.forward); }
            // bắn tên lửa liên tục khi xe nằm trước mũi
            h.fire -= dt;
            if (h.fire <= 0f)
            {
                var to = pp - h.root.position; float d = to.magnitude; var tf = new Vector3(to.x, 0, to.z);
                // 21.304 (người dùng): bắn ngay khi xuất hiện, nhịp như xe tăng/tàu (1,0 → 0,55 s theo tốc độ xe), chỉ không bắn khi xe
                // nằm hẳn phía sau đuôi; vòng báo vàng dưới đất (TintMarker)
                if (!player.Crashed && !player.frozen && d > 20f && d < planeFireRange && MissilesInFlight(true) < planeMaxMissiles)
                {
                    FireMissile(h, pp, pv);
                    h.fire = Mathf.Lerp(1.0f, 0.55f, Mathf.InverseLerp(8f, 27f, Mathf.Abs(player.Speed)));
                }
                else h.fire = 0.15f;
            }
            return false;                                                               // máy bay không tự rời đi (đuổi tới hết lượt)
        }
    }
}
