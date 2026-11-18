using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // Lái tự do tối giản cho scene bản đồ mở (21.249, đợt 6). Tách hẳn khỏi PhoVietGame (map cuộn cũ).
    // Bàn phím: W/S hoặc ↑/↓ ga/phanh-lùi, A/D hoặc ←/→ lái, Space phanh tay. Cảm ứng: kéo nửa trái màn hình
    // (lên = ga, xuống = phanh/lùi, trái/phải = lái). Rơi khỏi mặt đất (xuống sông) thì vật lý tự xử lý.
    [RequireComponent(typeof(Rigidbody))]
    public class OpenWorldDriver : MonoBehaviour
    {
        public float maxSpeed = 27f;          // ≈ 97 km/h
        public float reverseSpeed = 8f;
        public float accel = 7f, brake = 16f, drag = 2.2f;
        public float steerDegPerSec = 95f;
        public float Speed { get; private set; }

        Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 1300f; rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        [Tooltip("Kiểm thử tự động: khác 0 thì dùng thay bàn phím/cảm ứng (x = lái, y = ga).")]
        public Vector2 scriptedInput;

        Vector2 ReadInput()
        {
            if (scriptedInput != Vector2.zero) return scriptedInput;
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        // ------------------------------------------------------------ 21.262: joystick cảm ứng (góc trái dưới)
        // Kiểu game cũ (21.228): đẩy joystick về hướng nào thì xe quay MƯỢT về hướng đó — so với hướng xe lúc bắt đầu đẩy
        // (giữ nguyên khi còn giữ tay) — rồi chạy thẳng; đẩy càng xa tâm càng nhanh (tới 97 km/h). Trong ±10° quanh trục dọc coi là
        // thẳng. Kéo xuống (±40° quanh trục sau) = phanh rồi lùi, lái ngược như số lùi. Thả tay = phanh dần về 0.
        // Quay xe theo tốc độ góc có gia tốc (không giật), tốc độ góc tối đa giảm khi chạy nhanh, cua gắt thì tự giảm tốc.
        [Header("Joystick cảm ứng (21.262)")]
        public float stickDeadZone = 0.14f, stickStraightDeg = 10f, stickReverseDeg = 40f;
        public float stickTurnSlow = 125f, stickTurnFast = 50f, stickTurnAccel = 420f, stickCornerSpeed = 9f;
        [Tooltip("Hiện joystick cả khi máy không có cảm ứng (thử trong Editor)")] public bool stickAlwaysVisible;
        [System.NonSerialized] public bool testStickHeld; [System.NonSerialized] public Vector2 testStick;   // kiểm thử tự động
        int stickId = int.MinValue; Vector2 stickRaw, stickSmooth; bool stickRefValid; float stickRefYaw, yawRate;
        public bool StickActive => stickId != int.MinValue || testStickHeld;
        public Vector2 Stick => stickSmooth;
        public float StickAim { get; private set; }
        const int MouseStick = -1000;

        // vị trí joystick (pixel, gốc trên-trái như GUI) — tránh tai thỏ/viền bo (Screen.safeArea)
        public static Rect StickRect()
        {
            var sa = Screen.safeArea;
            float size = Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) * 0.36f, 150f, 420f);
            float inset = Mathf.Max(18f, size * 0.12f);
            float left = sa.xMin + inset + OpenWorldGame.InsetLeft, guiBottom = Screen.height - sa.yMin - inset - OpenWorldGame.InsetBottom;
            return new Rect(left, guiBottom - size, size, size);
        }
        static Vector2 ToGui(Vector2 screen) => new Vector2(screen.x, Screen.height - screen.y);

        void UpdateStick()
        {
            var r = StickRect(); var c = r.center; float rad = r.width * 0.5f;
            bool allowed = !frozen && !Crashed;
            if (!allowed) { stickId = int.MinValue; stickRaw = Vector2.zero; }
            Vector2 Axis(Vector2 gui) { var d = gui - c; return Vector2.ClampMagnitude(new Vector2(d.x, -d.y) / (rad * 0.62f), 1f); }
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i); var g = ToGui(t.position);
                if (t.phase == TouchPhase.Began && stickId == int.MinValue && allowed && (g - c).magnitude <= rad * 1.35f) stickId = t.fingerId;
                if (t.fingerId != stickId) continue;
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) { stickId = int.MinValue; stickRaw = Vector2.zero; continue; }
                stickRaw = Axis(g);
            }
            if (Input.touchCount == 0)
            {
                // chuột (Editor/máy tính): bấm vào joystick rồi kéo
                var g = ToGui(Input.mousePosition);
                if (Input.GetMouseButtonDown(0) && stickId == int.MinValue && allowed && (StickVisible || Input.touchSupported) && (g - c).magnitude <= rad * 1.35f) stickId = MouseStick;
                if (stickId == MouseStick) { if (Input.GetMouseButton(0)) stickRaw = Axis(g); else { stickId = int.MinValue; stickRaw = Vector2.zero; } }
                else if (stickId != int.MinValue) { stickId = int.MinValue; stickRaw = Vector2.zero; }
            }
            var want = testStickHeld ? Vector2.ClampMagnitude(testStick, 1f) : stickRaw;
            // làm mượt rung tay ngón cái (≈ 60 ms), không trễ khi đổi hướng lớn
            stickSmooth = Vector2.Lerp(stickSmooth, want, 1f - Mathf.Exp(-16f * Time.deltaTime));
            if (!StickActive && stickSmooth.sqrMagnitude < 1e-4f) stickSmooth = Vector2.zero;
        }
        bool StickVisible => !frozen && !Crashed && (stickAlwaysVisible || Input.touchSupported || Application.isMobilePlatform || StickActive);

        // trả về góc quay (độ) cho bước vật lý này; đặt Speed
        float StickDrive(float dt)
        {
            var s = stickSmooth; float strength = s.magnitude;
            float yaw = transform.eulerAngles.y;
            if (strength <= stickDeadZone)
            {
                stickRefValid = false;
                Speed = Mathf.MoveTowards(Speed, 0f, brake * 0.55f * dt);                       // thả tay: phanh dần
                yawRate = Mathf.MoveTowards(yawRate, 0f, stickTurnAccel * dt);
                return yawRate * dt * Mathf.Clamp01(Mathf.Abs(Speed) / 3f);
            }
            float ang = Mathf.Atan2(s.x, s.y) * Mathf.Rad2Deg;                                 // 0 = đẩy lên
            float k = Mathf.InverseLerp(stickDeadZone, 1f, strength);
            if (Mathf.Abs(ang) >= 180f - stickReverseDeg)
            {   // kéo xuống: phanh rồi lùi; lái như số lùi
                stickRefValid = false;
                Speed = Mathf.MoveTowards(Speed, -reverseSpeed * k, (Speed > 0.3f ? brake : accel) * dt);
                float side = Mathf.Clamp(s.x / Mathf.Max(0.2f, strength), -1f, 1f);
                float target = side * steerDegPerSec * 0.8f * Mathf.Clamp01(Mathf.Abs(Speed) / 4f) * Mathf.Sign(Speed == 0 ? 1 : Speed);
                yawRate = Mathf.MoveTowards(yawRate, target, stickTurnAccel * dt);
                return yawRate * dt;
            }
            if (Mathf.Abs(ang) <= stickStraightDeg) ang = 0f;
            if (!stickRefValid) { stickRefValid = true; stickRefYaw = yaw; }
            float aim = stickRefYaw + ang; StickAim = aim;
            float error = Mathf.DeltaAngle(yaw, aim);
            float ratio = Mathf.Clamp01(Mathf.Abs(Speed) / maxSpeed);
            float rateLimit = Mathf.Lerp(stickTurnSlow, stickTurnFast, Mathf.Sqrt(ratio));
            float move = Mathf.Clamp01((Mathf.Abs(Speed) + 1f) / 5f);                                // đứng yên thì quay rất chậm
            float targetRate = Mathf.Clamp(error * 4.5f, -rateLimit, rateLimit) * move;
            float old = yawRate;
            yawRate = Mathf.MoveTowards(yawRate, targetRate, stickTurnAccel * dt);
            float arc = (old + yawRate) * 0.5f * dt;
            if (arc * error > 0f && Mathf.Abs(arc) >= Mathf.Abs(error)) { arc = error; yawRate = 0f; }   // không quá đà
            // cua gắt thì giảm tốc; xe đang quay lưng với hướng đẩy phải quay xong mới tăng tốc
            float turnErr = Mathf.Abs(error);
            float corner = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(12f, 65f, turnErr));
            float cap = Mathf.Lerp(maxSpeed, stickCornerSpeed, corner) * Mathf.Lerp(1f, 0.35f, Mathf.InverseLerp(90f, 150f, turnErr));
            float requested = Mathf.Min(maxSpeed * k, cap);
            Speed = Mathf.MoveTowards(Speed, requested, (Speed < 0f || requested < Speed ? brake * 0.7f : accel) * dt);
            return arc;
        }

        void DrawStick()
        {
            if (!StickVisible) return;
            var r = StickRect(); float size = r.width;
            void Disc(Rect q, Color col) => GUI.DrawTexture(q, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, col, 0, q.width * 0.5f);
            Disc(r, new Color(.05f, .13f, .21f, StickActive ? .78f : .55f));
            float ring = size * .12f;
            Disc(new Rect(r.x + ring, r.y + ring, size - 2 * ring, size - 2 * ring), new Color(.19f, .35f, .49f, StickActive ? .55f : .38f));
            // vạch hướng: tiến / lùi
            var mid = r.center; float tick = size * .05f;
            GUI.DrawTexture(new Rect(mid.x - tick * .5f, r.y + ring * .35f, tick, tick * 1.6f), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(.85f, .95f, 1f, .55f), 0, tick * .5f);
            GUI.DrawTexture(new Rect(mid.x - tick * .5f, r.yMax - ring * .35f - tick * 1.6f, tick, tick * 1.6f), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(1f, .55f, .45f, .45f), 0, tick * .5f);
            float knob = size * .39f;
            var k = mid + new Vector2(stickSmooth.x, -stickSmooth.y) * size * .29f;
            Disc(new Rect(k.x - knob * .5f - 2, k.y - knob * .5f + 3, knob + 4, knob + 4), new Color(0, 0, 0, .18f));
            Disc(new Rect(k.x - knob * .5f, k.y - knob * .5f, knob, knob), new Color(0.447f, 0.784f, 0.941f, StickActive ? 1f : .85f));
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            // Rơi sông (mặt nước y = −2,2): chìm chậm tới đáy, 3 giây sau đưa về chỗ an toàn gần nhất trên bờ.
            if (sinking)
            {
                sinkTime += dt; Speed = 0;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.9f, Mathf.Max(rb.linearVelocity.y, -1.2f), rb.linearVelocity.z * 0.9f);
                if (transform.position.y < -5.5f) { rb.useGravity = false; rb.linearVelocity = Vector3.zero; }
                if (sinkTime > 3f && respawnAfterSink) Respawn();
                return;
            }
            if (transform.position.y < WaterY + 0.3f) { sinking = true; sinkTime = 0; RiverFalls++; jumping = false; return; }
            // 21.255: xe hỏng (trúng đạn pháo/khủng long/đâm xe tăng) → văng theo vật lý, khóa lái, nằm chờ CSGT.
            if (Crashed) { Speed = 0; return; }
            TickJump(dt);
            bool stunned = Time.time < stunUntil, stopping = Time.time < stopUntil;
            if (Time.time < flingUntil) { Speed = 0f; return; }               // 21.259: đang văng sau cú húc — để vật lý tự bay
            if (transform.position.y > -0.2f && Mathf.Abs(rb.linearVelocity.y) < 0.5f && Time.time - lastSafeTime > 1f)
            { lastSafePos = transform.position; lastSafeRot = transform.rotation; lastSafeTime = Time.time; }
            bool locked = frozen || stunned || stopping;
            bool useStick = !locked && StickActive && scriptedInput == Vector2.zero;
            var inp = locked || useStick ? Vector2.zero : ReadInput();
            bool hand = locked || Input.GetKey(KeyCode.Space);
            // Bị nhà/cây/cột chặn: tốc độ trên HUD tụt theo vận tốc thực.
            var fwd0 = transform.forward; fwd0.y = 0; fwd0.Normalize();
            float actual = Vector3.Dot(rb.linearVelocity, fwd0);
            if (Mathf.Abs(actual) < Mathf.Abs(Speed) - 4f && Time.time - spawnTime > 0.5f) Speed = Mathf.MoveTowards(Speed, actual, 60f * dt);
            float steer;
            if (useStick) steer = StickDrive(dt);                                    // 21.262: joystick cảm ứng
            else
            {
                stickRefValid = false; yawRate = 0f;
                if (inp.y > 0.05f) Speed = Mathf.MoveTowards(Speed, maxSpeed * inp.y, (Speed < 0 ? brake : accel) * dt);
                else if (inp.y < -0.05f) Speed = Mathf.MoveTowards(Speed, -reverseSpeed, (Speed > 0 ? brake : accel) * dt);
                else Speed = Mathf.MoveTowards(Speed, 0f, drag * dt);
                // Lái mạnh dần theo tốc độ, đảo chiều khi lùi.
                steer = inp.x * steerDegPerSec * Mathf.Clamp01(Mathf.Abs(Speed) / 6f) * Mathf.Sign(Speed == 0 ? 1 : Speed) * dt;
            }
            if (hand) Speed = Mathf.MoveTowards(Speed, 0f, brake * 1.5f * dt);
            if (Time.time < joltUntil && !jumping)
            {
                float k = (joltUntil - Time.time) / Mathf.Max(0.01f, joltLength);
                steer += joltYaw * k * Mathf.Clamp01(Mathf.Abs(Speed) / 6f) * dt;
                // Lái ngược hướng lệch thì lấy lại hướng nhanh hơn (như game cũ: chỉnh ngược chiều để cân bằng).
                if (inp.x * joltYaw < 0f) joltUntil -= dt * 1.5f;
            }
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, steer, 0f));
            var fwd = transform.forward; fwd.y = 0; fwd.Normalize();
            var vel = rb.linearVelocity;
            var target = fwd * Speed; target.y = vel.y;
            rb.linearVelocity = Vector3.Lerp(vel, target, 0.35f);
        }

        // ------------------------------------------------------------ 21.255: nhảy như game cũ
        // Đỉnh cố định 4,1166525 m so với mặt đỡ lúc phóng (21.100), gia tốc lên 24,2 m/s², rơi 22 m/s², hồi 2 s,
        // chỉ nhảy khi bánh chạm mặt đường/cầu. Phím J (Space vẫn là phanh tay) hoặc chạm nửa phải màn hình.
        public const float JumpApex = 4.1166525f, JumpRise = 24.2f, JumpFall = 22f, JumpCooldown = 2f;
        public bool Airborne => jumping;
        public float Lift => jumping ? Mathf.Max(0f, transform.position.y - jumpBase) : 0f;   // độ cao so với mặt đỡ lúc phóng
        public int Jumps { get; private set; }
        bool jumping, jumpRequest; float jumpBase, jumpAt = -10f;
        public void RequestJump() { jumpRequest = true; }
        // Chạm đất: tia dò ở tâm xe, hoặc mặt dưới thân xe đang tì lên vật gì đó (bồn cây, nóc xe khác…) — 21.258.
        float supportAt = -10f;
        bool Grounded()
        {
            return Time.time - supportAt < 0.12f || Physics.Raycast(transform.position + Vector3.up * 0.35f, Vector3.down, 0.6f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
        }
        void OnCollisionStay(Collision c)
        {
            for (int i = 0; i < c.contactCount; i++) if (c.GetContact(i).normal.y > 0.55f) { supportAt = Time.time; return; }
        }
        void TickJump(float dt)
        {
            if (jumpRequest)
            {
                jumpRequest = false;
                if (!jumping && !frozen && Time.time - jumpAt >= JumpCooldown && Grounded())
                {
                    jumping = true; jumpAt = Time.time; jumpBase = transform.position.y; Jumps++;
                    var v = rb.linearVelocity; v.y = Mathf.Sqrt(2f * JumpRise * JumpApex); rb.linearVelocity = v;
                }
            }
            if (!jumping) return;
            float g = -Physics.gravity.y;
            rb.AddForce(Vector3.down * ((rb.linearVelocity.y > 0 ? JumpRise : JumpFall) - g), ForceMode.Acceleration);
            if (Time.time - jumpAt > 0.25f && rb.linearVelocity.y <= 0.5f && Grounded()) jumping = false;
        }
        // 21.258: vùng màn hình (toạ độ GUI, gốc trên-trái) của nút TÊN LỬA — chạm vào đó không tính là nhảy.
        public static Rect TouchBlock;
        void Update()
        {
            UpdateStars();
            TickStunSmoke();
            UpdateStick();
            // Khói xe hỏng luôn bốc thẳng lên trời dù xe lật ngửa.
            if (smoke && Crashed) smoke.transform.SetPositionAndRotation(transform.position + Vector3.up * 0.9f, Quaternion.identity);
            if (Input.GetKeyDown(KeyCode.J)) jumpRequest = true;
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began && t.fingerId != stickId && t.position.x >= Screen.width * 0.5f && !TouchBlock.Contains(new Vector2(t.position.x, Screen.height - t.position.y))) jumpRequest = true;
            }
        }

        // ------------------------------------------------------------ 21.255: hố đạn kéo lệch
        float joltYaw, joltUntil, joltLength;
        public bool Jolted => Time.time < joltUntil;
        public int JoltSign => joltYaw > 0 ? 1 : -1;
        public void Jolt(float yawDegPerSec, float seconds, float speedFactor)
        {
            if (Crashed || jumping) return;
            joltYaw = yawDegPerSec; joltLength = seconds; joltUntil = Time.time + seconds; Speed *= speedFactor;
        }

        // ------------------------------------------------------------ 21.259 + 21.260: húc xe
        // Luật chung (OpenWorldTraffic.CrashRule, 21.260) theo vận tốc khép và khối lượng hai xe:
        // • < 40 km/h: xe người chơi đổi hướng nhẹ (5–12° tránh phía bị chạm), phanh gấp tới dừng (khóa ga 0,9 s), khói nhẹ.
        // • 40–90 km/h: văng ra (bật lùi/ngang + nảy + xoay), móp, dừng và CHOÁNG — vòng sao trên nóc + khói tắt dần; choáng
        //   1,2 → 4 s theo vận tốc khép. Xe kia nặng hơn → văng xa hơn, choáng lâu hơn; nhẹ hơn (xe máy) → văng ít, choáng ngắn.
        // • ≥ 90 km/h: xe kia cùng cỡ hoặc to hơn → văng lật, cháy, 1 s sau nổ tung → kết thúc lượt; xe kia nhẹ hơn ≥ 2,5 lần
        //   (xe máy) → chỉ văng + khói + choáng, xe kia nổ.
        // • Cả hai xe móp chỗ va chạm (OpenWorldCarDent) + mảnh vỡ (OpenWorldHazards.ImpactShards).
        [Header("Húc xe (21.259/21.260)")]
        public float flingSpeed = 11.11f;
        [Tooltip("21.260: khối lượng (tấn) của xe người chơi cho luật va chạm (Sport NPC 1,4; xe máy 0,25; container 12)")] public float massTons = 1.4f;
        public Material stunMaterial;
        public float StunRemaining => Mathf.Max(0f, stunUntil - Time.time);
        public bool Stunned => Time.time < stunUntil;
        public float ExplodeAt { get; private set; } = -1f;
        public void ConsumeExplosion() { ExplodeAt = -1f; }
        public int Bumps { get; private set; }
        public int Stuns { get; private set; }
        public string LastHit { get; private set; }
        float stunUntil, stopUntil, flingUntil;
        OpenWorldCarDent dent;
        Transform stars; readonly List<Transform> starList = new List<Transform>();
        ParticleSystem stunSmoke; float stunSmokeUntil;

        // 21.261: húc đổ tấm hàng rào trước cửa hàng — mất ~15 % tốc độ, móp nhẹ.
        public int FenceHits { get; private set; }
        public void HitFence(Vector3 point, float speed)
        {
            if (Crashed) return;
            FenceHits++; Speed *= 0.85f;
            if (speed > 4f) Dent(point, speed * 0.3f);
        }

        // Khói xám nhẹ sau cú va chạm không nổ: bốc trong thời gian choáng/dừng, giảm dần ở giây cuối rồi tắt.
        void StunSmoke(float seconds)
        {
            if (!stunSmoke)
            {
                stunSmoke = MakeSmoke(); if (!stunSmoke) return;
                stunSmoke.gameObject.name = "KhoiChoang";
                var m = stunSmoke.main; m.startColor = new Color(.62f, .6f, .57f, .8f); m.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.4f); m.startLifetime = 1.5f;
            }
            stunSmokeUntil = Mathf.Max(stunSmokeUntil, Time.time + Mathf.Max(0.6f, seconds));
            var em = stunSmoke.emission; em.rateOverTime = 20f;
            if (!stunSmoke.isEmitting) stunSmoke.Play();
        }
        void TickStunSmoke()
        {
            if (!stunSmoke || !stunSmoke.isEmitting) return;
            float left = stunSmokeUntil - Time.time;
            stunSmoke.transform.SetPositionAndRotation(transform.position + Vector3.up * 0.9f, Quaternion.identity);
            var em = stunSmoke.emission; em.rateOverTime = 20f * Mathf.Clamp01(left / 1f);
            if (left <= 0f || Crashed) stunSmoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        void Dent(Vector3 point, float closing)
        {
            if (dent == null) dent = new OpenWorldCarDent(transform, GetComponentsInChildren<Renderer>(false));
            var outward = point - transform.position; outward.y = 0;
            dent.Dent(point, outward, closing);
        }

        void HitVehicle(Collision c, float otherMass)
        {
            var ct = c.GetContact(0); float closing = c.relativeVelocity.magnitude;
            if (closing < OpenWorldTraffic.MinClosing) return;
            Dent(ct.point, closing);
            var other = c.collider.attachedRigidbody ? c.collider.attachedRigidbody.position : ct.point;
            var away = transform.position - other; away.y = 0; away = away.sqrMagnitude > 0.01f ? away.normalized : -transform.forward;
            var o = OpenWorldTraffic.CrashRule(massTons, otherMass, closing);
            int kmh = Mathf.RoundToInt(closing * 3.6f);
            LastHit = $"{kmh} km/h, {massTons:0.##} t ↔ {otherMass:0.##} t → " + (o.explode ? "nổ" : closing >= flingSpeed ? $"văng ×{o.fling:0.00}, choáng {o.stun:0.0} s" : "chạm nhẹ");
            if (o.explode)
            {
                LastCrashInfo = "va chạm " + kmh + " km/h với xe " + otherMass.ToString("0.##") + " t";
                Crash(ct.point, "HÚC XE Ở TỐC ĐỘ TỐI ĐA", 12f);
                ExplodeAt = Time.time + 1f;
                return;
            }
            if (closing >= flingSpeed)
            {
                float k = Mathf.InverseLerp(flingSpeed, OpenWorldTraffic.FatalClosing, closing), f = o.fling, fu = Mathf.Min(1f, f);
                var side = transform.right * (Vector3.Dot(away, transform.right) >= 0 ? 1f : -1f);
                rb.linearVelocity = (away * 0.75f + side * 0.35f).normalized * (closing * 0.38f * f) + Vector3.up * (1.6f + closing * 0.09f) * fu;
                rb.angularVelocity = new Vector3(0, (Random.value < .5f ? -1f : 1f) * (2f + k * 3f) * fu, 0);
                flingUntil = Time.time + (0.6f + k * 0.6f) * Mathf.Clamp(f, 0.4f, 1f);
                stunUntil = Time.time + o.stun;
                Speed = 0f; Stuns++; jumping = false;
                StunSmoke(o.stun);
                return;
            }
            // nhẹ: đổi hướng nhẹ, phanh gấp tới dừng
            float sideSign = Vector3.Dot(away, transform.right) >= 0 ? 1f : -1f;
            float yaw = sideSign * Mathf.Lerp(5f, 12f, Mathf.InverseLerp(OpenWorldTraffic.MinClosing, flingSpeed, closing));
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, yaw, 0));
            Speed *= 0.5f; stopUntil = Time.time + 0.9f; Bumps++;
            StunSmoke(0.9f);
        }

        // Vòng sao choáng: 5 ngôi sao vàng quay vòng trên nóc xe, luôn quay mặt về camera.
        void UpdateStars()
        {
            bool on = Stunned && !Crashed;
            if (!on) { if (stars) stars.gameObject.SetActive(false); return; }
            if (!stars)
            {
                if (!stunMaterial) return;
                stars = new GameObject("VongChoang").transform; stars.SetParent(transform, false);
                var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                for (int i = 0; i < 5; i++)
                {
                    var g = new GameObject("Sao"); g.transform.SetParent(stars, false);
                    g.AddComponent<MeshFilter>().sharedMesh = quad;
                    var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = stunMaterial; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                    g.transform.localScale = Vector3.one * 0.42f; starList.Add(g.transform);
                }
            }
            stars.gameObject.SetActive(true);
            stars.position = transform.position + Vector3.up * 1.75f; stars.rotation = Quaternion.identity;
            var cam = Camera.main; float t = Time.time;
            for (int i = 0; i < starList.Count; i++)
            {
                float a = t * 4.2f + i * Mathf.PI * 2f / starList.Count;
                starList[i].localPosition = new Vector3(Mathf.Cos(a) * 0.95f, Mathf.Sin(t * 6f + i) * 0.08f, Mathf.Sin(a) * 0.95f);
                if (cam) starList[i].rotation = Quaternion.LookRotation(starList[i].position - cam.transform.position) * Quaternion.Euler(0, 0, t * 180f + i * 40f);
            }
        }

        // ------------------------------------------------------------ 21.255: xe hỏng
        public bool Crashed { get; private set; }
        public string LastCrashInfo { get; private set; }
        public string CrashReason { get; private set; }
        public int Crashes { get; private set; }
        public Material smokeMaterial;
        ParticleSystem smoke;
        public void Crash(Vector3 from, string reason, float power)
        {
            if (Crashed || sinking) return;
            Crashed = true; frozen = true; CrashReason = reason; Crashes++; jumping = false; Speed = 0;
            var away = transform.position - from; away.y = 0;
            if (away.sqrMagnitude < 0.01f) away = transform.right; away.Normalize();
            rb.constraints = RigidbodyConstraints.None;
            rb.linearVelocity = away * power + Vector3.up * power * 0.75f + rb.linearVelocity * 0.3f;
            rb.angularVelocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 4f;
            if (!smoke) smoke = MakeSmoke();
            if (smoke) smoke.Play();
        }
        ParticleSystem MakeSmoke()
        {
            if (!smokeMaterial) return null;
            var go = new GameObject("KhoiXeHong"); go.transform.SetParent(transform, false); go.transform.localPosition = new Vector3(0, 0.9f, 1.2f);
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = ps.main; m.loop = true; m.startLifetime = 2.2f; m.startSpeed = 1.6f; m.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
            m.simulationSpace = ParticleSystemSimulationSpace.World; m.gravityModifier = -0.06f; m.maxParticles = 120;
            m.startColor = new Color(0.25f, 0.24f, 0.23f, 0.85f);
            var em = ps.emission; em.rateOverTime = 14f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 12f; sh.radius = 0.3f; sh.rotation = new Vector3(-90, 0, 0);   // nón hướng lên (+y) khi object không xoay
            var sheet = ps.textureSheetAnimation; sheet.enabled = true; sheet.numTilesX = 4; sheet.numTilesY = 2;
            sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .75f, 1, .999f));
            var col = ps.colorOverLifetime; col.enabled = true; var gr = new Gradient();
            gr.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0.9f, 0), new GradientAlphaKey(0, 1) }); col.color = gr;
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .7f, 1, 2f));
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = smokeMaterial; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return ps;
        }

        public const float WaterY = -2.2f;
        public bool Sinking => sinking;
        // 21.257: OpenWorldGame tắt — rơi sông là kết thúc chuyến (luật cũ); HUD km/h do thanh HUD vòng chơi vẽ.
        [System.NonSerialized] public bool respawnAfterSink = true, showSpeed = true;
        // 21.254: CSGT bắt → khóa điều khiển (phanh tới đứng yên); chơi lại → đưa xe về vị trí xuất phát.
        [System.NonSerialized] public bool frozen;
        public void ResetTo(Vector3 pos, Quaternion rot)
        {
            sinking = false; rb.useGravity = true; Speed = 0; frozen = false;
            Crashed = false; CrashReason = null; jumping = false; jumpRequest = false; joltUntil = 0f;
            stunUntil = stopUntil = flingUntil = 0f; ExplodeAt = -1f; LastCrashInfo = null;
            if (dent != null) dent.Restore();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            if (smoke) smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (stunSmoke) stunSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); stunSmokeUntil = 0f; LastHit = null;
            rb.position = pos; rb.rotation = rot; transform.SetPositionAndRotation(pos, rot);
            rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; spawnTime = Time.time;
            lastSafePos = pos; lastSafeRot = rot; lastSafeTime = Time.time;
        }
        public int RiverFalls { get; private set; }
        bool sinking; float sinkTime, lastSafeTime = -10f, spawnTime;
        Vector3 lastSafePos; Quaternion lastSafeRot;

        void Start() { lastSafePos = transform.position; lastSafeRot = transform.rotation; spawnTime = Time.time; }

        void Respawn()
        {
            sinking = false; rb.useGravity = true; Speed = 0;
            // Lùi về phía sau hướng an toàn 6 m để không lao xuống sông lần nữa.
            var back = lastSafeRot * Vector3.back; back.y = 0;
            rb.position = lastSafePos + back.normalized * 6f + Vector3.up * 0.4f; rb.rotation = lastSafeRot;
            transform.SetPositionAndRotation(rb.position, rb.rotation);
            rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; spawnTime = Time.time;
        }

        void OnCollisionEnter(Collision c)
        {
            // 21.255: đâm thân xe tăng/khủng long → xe hỏng.
            var npc = c.collider ? c.collider.GetComponentInParent<OpenWorldNpcBody>() : null;
            if (npc && npc.owner && !Crashed)
            {
                // xe hỏng/choáng đã nằm yên (không phải cú húc vừa xảy ra) → như vật cản tĩnh; xe đang văng tới → va chạm thật
                if (npc.owner.IsWreck(npc.index) && !npc.owner.WreckMoving(npc.index) && npc.owner.WreckAge(npc.index) > 0.25f) { Speed *= 0.25f; return; }
                HitVehicle(c, npc.owner.Mass(npc.index));
                return;
            }
            var hz = c.collider ? c.collider.GetComponentInParent<OpenWorldHazardBody>() : null;
            if (hz) { LastCrashInfo = "va collider " + c.collider.name + " tại " + c.GetContact(0).point.ToString("F1"); Crash(c.GetContact(0).point, hz.reason, 6f); return; }
            // Đâm nhà/cây/cột: mất phần lớn tốc độ (giữ cảm giác va chạm như game cũ, chưa có hiệu ứng).
            if (Vector3.Dot(c.GetContact(0).normal, transform.forward) < -0.5f) Speed *= 0.25f;
        }

        void OnGUI()
        {
            var st = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, Screen.height / 28), fontStyle = FontStyle.Bold };
            st.normal.textColor = Color.white;
            if (showSpeed) GUI.Label(new Rect(20, 16, 400, 60), Mathf.RoundToInt(Mathf.Abs(Speed) * 3.6f) + " km/h", st);
            if (sinking && respawnAfterSink) GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 3, 400, 60), "Rơi xuống sông!", st);
            if (Stunned && !Crashed)
            {
                var sc = new GUIStyle(st) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(st.fontSize * 1.15f) };
                sc.normal.textColor = new Color(1f, 0.85f, 0.25f);
                GUI.Label(new Rect(0, Screen.height * 0.36f, Screen.width, st.fontSize * 2f), "CHOÁNG  " + StunRemaining.ToString("0.0") + " s", sc);
            }
            if (Crashed)
            {
                var c = new GUIStyle(st) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(st.fontSize * 1.3f) };
                c.normal.textColor = new Color(1f, 0.82f, 0.3f);
                GUI.Label(new Rect(0, Screen.height * 0.42f, Screen.width, st.fontSize * 2.4f), CrashReason + " · XE HỎNG", c);
            }
            else if (!frozen)
            {
                // Nút NHẢY (cảm ứng: chạm nửa phải màn hình; bàn phím: J), mờ khi đang hồi.
                float sz = Mathf.Max(70, Screen.height * 0.13f);
                var b = new Rect(Screen.width - sz - 24 - OpenWorldGame.InsetRight, Screen.height - sz - 24 - OpenWorldGame.InsetBottom, sz, sz);
                var bs = new GUIStyle(GUI.skin.box) { fontSize = Mathf.RoundToInt(sz * 0.2f), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                bs.normal.textColor = Color.white;
                var old = GUI.color; GUI.color = new Color(1, 1, 1, jumping || Time.time - jumpAt < JumpCooldown ? 0.35f : 0.85f);
                GUI.Box(b, Input.touchSupported ? "NHẢY" : "NHẢY\n(J)", bs); GUI.color = old;
            }
            if (!Crashed && !frozen) DrawStick();
        }
    }
}
