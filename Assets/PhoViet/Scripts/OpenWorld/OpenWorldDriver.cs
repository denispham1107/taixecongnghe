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

        // ------------------------------------------------------------ 21.265/21.270: nút lái + bàn đạp cảm ứng
        // • 21.270: góc trái dưới là 2 nút MŨI TÊN TRÁI / PHẢI (thay vô lăng xoay 21.265). Giữ nút = y hệt giữ phím ←/→ trên
        //   máy tính (Input.GetAxisRaw: −1 / +1 ngay lập tức, thả = 0); giữ cả hai = 0. Trượt ngón từ nút này sang nút kia
        //   thì đổi hướng luôn, không cần nhấc tay.
        // • Nút bên phải (giữ để dùng): Ga = tiến (như ↑), Lùi = lùi/phanh (như ↓), Stop = phanh gấp về 0 trong ≤ 1 s;
        //   NHẢY, TÊN LỬA chạm là ăn. Mỗi ngón tay được gán cho đúng một nút khi chạm xuống (đa điểm): vừa giữ nút lái
        //   vừa giữ Ga và chạm TÊN LỬA được (trước 21.265 nút TÊN LỬA là nút IMGUI chỉ nhận một ngón).
        // • Chạm nửa phải ngoài các nút vẫn là NHẢY. Bàn phím giữ như cũ.
        [Header("Nút lái + bàn đạp cảm ứng (21.265/21.270)")]
        [Tooltip("Hiện nút cảm ứng cả khi máy không có cảm ứng (thử trong Editor)")] public bool touchControlsAlwaysVisible;
        [System.NonSerialized] public bool testTouchUI;                                                    // kiểm thử: hiện nút cảm ứng
        [System.NonSerialized] public int testSteer;                                                       // kiểm thử: −1 Trái, 1 Phải
        [System.NonSerialized] public int testPedal;                                                       // kiểm thử: 1 Ga, −1 Lùi, 2 Stop
        enum Ctl { None, Left, Right, Gas, Rev, Stop, Jump, Fire, Other }
        readonly System.Collections.Generic.Dictionary<int, Ctl> fingers = new System.Collections.Generic.Dictionary<int, Ctl>();
        bool leftHeld, rightHeld, gasHeld, revHeld, stopHeld;
        OpenWorldHazards hazardsRef;
        const int MouseFinger = -1000;
        public bool LeftHeld => leftHeld || testSteer == -1;
        public bool RightHeld => rightHeld || testSteer == 1;
        public float TouchSteer => (RightHeld ? 1f : 0f) - (LeftHeld ? 1f : 0f);   // như GetAxisRaw("Horizontal")
        public bool GasHeld => gasHeld || testPedal == 1;
        public bool RevHeld => revHeld || testPedal == -1;
        public bool StopHeld => stopHeld || testPedal == 2;
        public int FireTaps { get; private set; }
        public int StopBrakes { get; private set; }
        public bool TouchControlsVisible => !frozen && !Crashed && (touchControlsAlwaysVisible || Input.touchSupported || UnityEngine.Device.Application.isMobilePlatform || testTouchUI);
        bool TouchDriving => TouchControlsVisible && (LeftHeld || RightHeld || GasHeld || RevHeld || StopHeld);

        // Bố cục (pixel, gốc trên-trái như GUI) — né tai thỏ/thanh home (OpenWorldGame.Safe*).
        static float BtnSize => Mathf.Max(70f, Screen.height * 0.13f);
        static float Gap => BtnSize * 0.14f;
        // 21.268 (người dùng đổi chỗ): hàng dưới Stop · Lùi · Ga (góc phải dưới); hàng trên Tên lửa (trên Lùi) · Nhảy (trên Ga).
        public static Rect GasRect => new Rect(Screen.width - BtnSize - 24 - OpenWorldGame.SafeRightPx, Screen.height - BtnSize - 24 - OpenWorldGame.SafeBottomPx, BtnSize, BtnSize);
        public static Rect RevRect { get { var g = GasRect; return new Rect(g.x - BtnSize - Gap, g.y, BtnSize, BtnSize); } }
        public static Rect StopRect { get { var r = RevRect; return new Rect(r.x - BtnSize - Gap, r.y, BtnSize, BtnSize); } }
        // Máy tính (không có Ga/Lùi/Stop): giữ bố cục cũ — Nhảy góc phải dưới, Tên lửa ngay trên.
        static bool touchLayout = true;
        public static Rect JumpRect { get { var g = GasRect; return touchLayout ? new Rect(g.x, g.y - BtnSize - Gap, BtnSize, BtnSize) : g; } }
        public static Rect FireRect { get { var r = touchLayout ? RevRect : GasRect; return new Rect(r.x, r.y - BtnSize - Gap, BtnSize, BtnSize); } }
        // 21.270: hai nút mũi tên góc trái dưới, to hơn nút bên phải 20 % vì phải giữ liên tục.
        static float SteerSize => BtnSize * 1.2f;
        public static Rect LeftRect => new Rect(OpenWorldGame.SafeLeftPx + 24, Screen.height - SteerSize - 24 - OpenWorldGame.SafeBottomPx, SteerSize, SteerSize);
        public static Rect RightRect { get { var l = LeftRect; return new Rect(l.xMax + Gap, l.y, SteerSize, SteerSize); } }
        static Vector2 ToGui(Vector2 screen) => new Vector2(screen.x, Screen.height - screen.y);
        static float Grow(Rect r, Vector2 p, float pad) => (p.x >= r.x - pad && p.x <= r.xMax + pad && p.y >= r.y - pad && p.y <= r.yMax + pad) ? 1f : 0f;

        Ctl HitControl(Vector2 g)
        {
            float pad = BtnSize * 0.06f;
            var s = SteerControl(g); if (s != Ctl.None) return s;
            if (Grow(FireRect, g, pad) > 0 && FireVisible) return Ctl.Fire;
            if (Grow(JumpRect, g, pad) > 0) return Ctl.Jump;
            if (Grow(GasRect, g, pad) > 0) return Ctl.Gas;
            if (Grow(RevRect, g, pad) > 0) return Ctl.Rev;
            if (Grow(StopRect, g, pad) > 0) return Ctl.Stop;
            return g.x >= Screen.width * 0.5f ? Ctl.Jump : Ctl.Other;      // chạm nửa phải ngoài nút = nhảy (như cũ)
        }
        bool FireVisible => hazardsRef && hazardsRef.enabled;
        // Vùng chạm lái nới rộng (ngón cái hay chạm lệch): chia đôi ở giữa khe hai nút, dư 35 % lên trên và ra mép trái.
        static Ctl SteerControl(Vector2 g)
        {
            Rect l = LeftRect, r = RightRect; float pad = SteerSize * 0.35f, mid = (l.xMax + r.x) * 0.5f;
            if (g.y < l.y - pad || g.x < l.x - pad || g.x > r.xMax + pad * 0.5f) return Ctl.None;
            return g.x < mid ? Ctl.Left : Ctl.Right;
        }

        void Press(int id, Vector2 g)
        {
            var c = HitControl(g); fingers[id] = c;
            if (c == Ctl.Jump) jumpRequest = true;
            else if (c == Ctl.Fire && hazardsRef) { hazardsRef.RequestFire(); FireTaps++; }
        }
        void Drag(int id, Vector2 g)
        {
            // ngón đang lái trượt sang nút kia → đổi hướng; trượt ra ngoài vùng lái vẫn giữ hướng cũ tới khi nhấc
            if (!fingers.TryGetValue(id, out var c) || (c != Ctl.Left && c != Ctl.Right)) return;
            var s = SteerControl(g); if (s != Ctl.None) fingers[id] = s;
        }
        void Release(int id) { fingers.Remove(id); }

        void UpdateTouchControls()
        {
            if (!hazardsRef) hazardsRef = FindAnyObjectByType<OpenWorldHazards>();
            touchLayout = touchControlsAlwaysVisible || Input.touchSupported || UnityEngine.Device.Application.isMobilePlatform || testTouchUI;
            if (!TouchControlsVisible) { fingers.Clear(); leftHeld = rightHeld = gasHeld = revHeld = stopHeld = false; return; }
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i); var g = ToGui(t.position);
                if (t.phase == TouchPhase.Began) Press(t.fingerId, g);
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) Release(t.fingerId);
                else Drag(t.fingerId, g);
            }
            if (Input.touchCount == 0)
            {   // chuột (Editor/máy tính có bật touchControlsAlwaysVisible)
                var g = ToGui(Input.mousePosition);
                if (Input.GetMouseButtonDown(0)) Press(MouseFinger, g);
                else if (Input.GetMouseButton(0)) { if (fingers.ContainsKey(MouseFinger)) Drag(MouseFinger, g); }
                else if (fingers.ContainsKey(MouseFinger)) Release(MouseFinger);
                // ngón chạm đã mất mà không có Ended (mất tiêu điểm) → thả
                if (!Input.GetMouseButton(0)) { foreach (var k in new System.Collections.Generic.List<int>(fingers.Keys)) if (k != MouseFinger) Release(k); }
            }
            leftHeld = rightHeld = gasHeld = revHeld = stopHeld = false;
            foreach (var c in fingers.Values)
            {
                if (c == Ctl.Left) leftHeld = true; else if (c == Ctl.Right) rightHeld = true;
                else if (c == Ctl.Gas) gasHeld = true; else if (c == Ctl.Rev) revHeld = true; else if (c == Ctl.Stop) stopHeld = true;
            }
        }

        static void Disc(Rect q, Color col, float border = 0f) =>
            GUI.DrawTexture(q, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, col, border, q.width * 0.5f);
        static void Bar(Rect q, Color col, float radius) =>
            GUI.DrawTexture(q, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, col, 0, radius);

        // 21.266: ảnh vẽ sẵn (ArtSource/UI/build_touch_controls.py → Resources/TouchControls), phong cách icon «B Arcade».
        static Texture2D leftTex, rightTex, gasTex, revTex, stopTex, jumpTex;
        public static Texture2D MissileButtonTex { get; private set; }
        static void LoadTouchArt()
        {
            if (leftTex) return;
            leftTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Left");
            rightTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Right");
            gasTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Gas");
            revTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Reverse");
            stopTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Stop");
            jumpTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Jump");
            MissileButtonTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Missile");
        }

        // Vẽ một nút ảnh: đang giữ → thu nhỏ 7 % + quầng màu nhấn phía sau; tắt (hồi chiêu/hết đạn) → xám mờ.
        public static void DrawArtButton(Rect b, Texture2D tex, Color accent, bool held, bool enabled = true, float alpha = .92f)
        {
            if (held) Disc(new Rect(b.x - b.width * .06f, b.y - b.width * .06f, b.width * 1.12f, b.height * 1.12f), new Color(accent.r, accent.g, accent.b, .38f));
            var q = held ? new Rect(b.x + b.width * .035f, b.y + b.height * .035f, b.width * .93f, b.height * .93f) : b;
            var tint = enabled ? new Color(1f, 1f, 1f, held ? 1f : alpha) : new Color(.55f, .58f, .62f, .55f);
            if (tex) GUI.DrawTexture(q, tex, ScaleMode.ScaleToFit, true, 0, tint, 0, 0);
            else Bar(q, new Color(.1f, .15f, .2f, .7f * tint.a), q.width * .2f);
        }

        // Máy tính: chữ phím nhỏ ở góc nút.
        public static void KeyBadge(Rect b, string key)
        {
            float s = b.width * .26f; var q = new Rect(b.xMax - s * 1.05f, b.y + s * .05f, s, s);
            Disc(q, new Color(.06f, .12f, .19f, .92f));
            var st = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(s * .58f) };
            st.normal.textColor = new Color(1f, .96f, .84f); GUI.Label(q, key, st);
        }

        void DrawTouchControls()
        {
            if (!TouchControlsVisible) return;
            LoadTouchArt();
            var steerAccent = new Color(.18f, .78f, .91f);
            DrawArtButton(LeftRect, leftTex, steerAccent, LeftHeld);
            DrawArtButton(RightRect, rightTex, steerAccent, RightHeld);
            DrawArtButton(GasRect, gasTex, new Color(1f, .54f, .24f), GasHeld);
            DrawArtButton(RevRect, revTex, new Color(.21f, .78f, .42f), RevHeld);
            DrawArtButton(StopRect, stopTex, new Color(.24f, .55f, .94f), StopHeld);
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
            var inp = locked ? Vector2.zero : ReadInput();
            // 21.265/21.270: nút ← / → + Ga/Lùi trên màn hình cảm ứng (bàn phím/kiểm thử vẫn ưu tiên khi có tín hiệu)
            if (!locked && scriptedInput == Vector2.zero && TouchDriving)
            {
                if (Mathf.Abs(inp.x) < 0.05f) inp.x = TouchSteer;
                if (Mathf.Abs(inp.y) < 0.05f) inp.y = GasHeld ? 1f : RevHeld ? -1f : 0f;
            }
            bool hand = locked || Input.GetKey(KeyCode.Space);
            bool hardStop = !locked && StopHeld && TouchControlsVisible;
            // Bị nhà/cây/cột chặn: tốc độ trên HUD tụt theo vận tốc thực.
            var fwd0 = transform.forward; fwd0.y = 0; fwd0.Normalize();
            float actual = Vector3.Dot(rb.linearVelocity, fwd0);
            if (Mathf.Abs(actual) < Mathf.Abs(Speed) - 4f && Time.time - spawnTime > 0.5f) Speed = Mathf.MoveTowards(Speed, actual, 60f * dt);
            if (hardStop) inp.y = 0f;
            if (inp.y > 0.05f) Speed = Mathf.MoveTowards(Speed, maxSpeed * inp.y, (Speed < 0 ? brake : accel) * dt);
            else if (inp.y < -0.05f) Speed = Mathf.MoveTowards(Speed, -reverseSpeed, (Speed > 0 ? brake : accel) * dt);
            else Speed = Mathf.MoveTowards(Speed, 0f, drag * dt);
            // Lái mạnh dần theo tốc độ, đảo chiều khi lùi.
            float steer = inp.x * steerDegPerSec * Mathf.Clamp01(Mathf.Abs(Speed) / 6f) * Mathf.Sign(Speed == 0 ? 1 : Speed) * dt;
            if (hand) Speed = Mathf.MoveTowards(Speed, 0f, brake * 1.5f * dt);
            // Stop: phanh gấp — từ tốc độ tối đa về 0 trong 1 giây
            if (hardStop) { if (Mathf.Abs(Speed) > 0.05f && Mathf.Abs(Speed) > maxSpeed * dt) StopBrakes++; Speed = Mathf.MoveTowards(Speed, 0f, maxSpeed * dt); }
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
        void Update()
        {
            UpdateStars();
            TickStunSmoke();
            UpdateTouchControls();
            // Khói xe hỏng luôn bốc thẳng lên trời dù xe lật ngửa.
            if (smoke && Crashed) smoke.transform.SetPositionAndRotation(transform.position + Vector3.up * 0.9f, Quaternion.identity);
            if (Input.GetKeyDown(KeyCode.J)) jumpRequest = true;
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
                LoadTouchArt();
                var b = JumpRect; float sz = b.width;
                bool ready = !jumping && Time.time - jumpAt >= JumpCooldown;
                bool held = false; foreach (var k in fingers.Values) if (k == Ctl.Jump) held = true;
                DrawArtButton(b, jumpTex, new Color(.96f, .76f, .29f), held, ready);
                if (!TouchControlsVisible) KeyBadge(b, "J");
            }
            DrawTouchControls();
        }
    }
}
