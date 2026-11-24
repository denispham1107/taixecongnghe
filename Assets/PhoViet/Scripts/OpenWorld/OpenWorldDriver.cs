using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // Lái tự do tối giản cho scene bản đồ mở (21.249, đợt 6). Tách hẳn khỏi PhoVietGame (map cuộn cũ).
    // Bàn phím (21.383): ↑/↓ ga/phanh-lùi, ←/→ lái (bỏ W/A/S/D), R phanh tay (21.375 J; trước Space). Cảm ứng: kéo nửa trái màn hình
    // (lên = ga, xuống = phanh/lùi, trái/phải = lái). Rơi khỏi mặt đất (xuống sông) thì vật lý tự xử lý.
    [RequireComponent(typeof(Rigidbody))]
    public class OpenWorldDriver : MonoBehaviour
    {
        public float maxSpeed = 27f;          // ≈ 97 km/h
        public float reverseSpeed = 8f;
        // 21.292 (người dùng): thả ga xe trôi lâu hơn — drag 2,2 → 1,6 m/s² (từ 97 km/h về 0: 12 s → 17 s).
        // 21.362 (người dùng): thả Ga tự giảm tốc chậm hơn 30 % nữa — drag 1,6 → 1,12 m/s² (110 km/h về 0: ~19 s → ~27 s).
        // Giá trị cũng lưu trong scene PhoVietOpenWorld (trường public) — sửa cả hai.
        public float accel = 7f, brake = 16f, drag = 1.12f;
        // 21.316 (người dùng: đổi hướng trái/phải chưa mượt) — «vô lăng ảo»: nút/phím lái là bật/tắt (−1/0/+1) nên trước đây thân xe
        // nhảy ngay 0 → 95°/s → −95°/s trong 1 bước. Nay góc lái tăng dần tới đủ trong steerRise (0,2 s), trả về thẳng trong
        // steerReturn (0,12 s); đổi bên thì trả qua giữa rồi mới bẻ sang. Tốc độ quay giảm theo tốc độ xe: từ 8 m/s tới maxSpeed
        // còn highSpeedSteer (58 % → ~55°/s ở 97 km/h), đi chậm vẫn quay gắt như cũ. 21.316e: thay bằng mô hình ô tô thật bên dưới (bỏ steerDegPerSec/highSpeedSteer).
        // 21.329 (người dùng: «cho trở về lại phiên bản đổi hướng quẹo trái quẹo phải lần đầu tiên chưa chỉnh sửa gì cả để chơi thử so
        // sánh; khi cần sẽ bảo trả lại giống hiện giờ; riêng góc quay, tốc độ quay camera, chuyển cảnh camera giữ nguyên»):
        // classicSteer = true → lái y hệt 1.89.0 (trước 21.316): giữ nút/phím là thân xe quay ngay 95°/s, mạnh dần từ 0 tới 6 m/s, đảo
        // chiều khi lùi; không vô lăng ảo, không giới hạn bám/thiếu lái/cản cua, không trượt hướng chạy, không nghiêng thân, Space/P-DRIFT
        // là phanh gấp như cũ (không drift); vận tốc bám đầu xe 0,35/bước. Camera (OpenWorldCamera, trễ 0,6 s) không đổi.
        // TRẢ LẠI LÁI HIỆN GIỜ (21.316–21.319): đặt classicSteer = false (mã mặc định + scene nếu có ghi đè). Mọi thông số bên dưới giữ nguyên.
        [Header("Lái gốc 1.89.0 (21.329)")]
        public bool classicSteer = true;
        public float classicSteerDegPerSec = 95f;
        [Header("Lái mượt (21.316)")]
        public float steerRise = 0.08f, steerReturn = 0.06f;                    // 21.319 (thử điện thoại: nặng, chậm): 0,18/0,14 → 0,08/0,06 s (trước: 21.316b 0,28; 21.316d 0,18)
        // 21.316e (người dùng: góc bẻ lái, ôm cua, quay đầu chưa giống ngoài đời) — LÁI KIỂU Ô TÔ THẬT (mô hình xe hai trục + bám lốp):
        // • Góc bánh trước δ = góc lái ảo × maxWheelAngle (35°). Tốc độ quay thân xe ổn định r = v·tanδ / (L + Ku·v²) — L chiều dài cơ
        //   sở 2,65 m, Ku hệ số thiếu lái: chạy chậm xe vòng đúng bán kính hình học (quay đầu R ≈ L/tan35° ≈ 3,8 m), chạy nhanh tự thiếu lái.
        // • Giới hạn bám lốp: gia tốc ngang v·r ≤ μ·g (xe thể thao thật ~1,0 g; game gripG = 1,7 g cho dễ chơi) → r ≤ μg/v. Phanh khi cua
        //   dồn tải lên bánh trước (+12 % bám, ôm cua chặt hơn), đạp ga khi cua bớt bám (−4 %).
        // • Quán tính quay thân xe yawLag (0,09 s; 21.319: 0,045 s): tốc độ quay đuổi theo mục tiêu, không nhảy bậc.
        // • 21.319 (người dùng thử 1.90.0 trên điện thoại: đổi hướng nặng nề, như lôi cả chiếc xe, chậm; camera đã ổn): bám 1,7 → 2,6 g,
        //   vô lăng ảo 0,08/0,06 s, quán tính 0,045 s, trượt hướng chạy 0,02–0,05 s, cản cua 0,9 m/s² × tỉ lệ², thiếu lái 2 m/s², lực kéo
        //   khi cua ≥ 50 %, vận tốc bám hướng chạy 0,5/bước. Camera giữ nguyên (headingLag 0,6 s).
        // • Vòng ma sát: đang dùng bám ngang thì lực kéo dọc còn √(1 − tỉ lệ²) (tối thiểu 25 %) → cua gắt ở tốc độ cao không tăng tốc được;
        //   vượt giới hạn (lốp trước trượt, thiếu lái) mất thêm scrubDecel (3,5 m/s²) + lực cản cua 2,2 m/s² × tỉ lệ² → xe tự giảm tốc khi ôm cua.
        // • Lái tỉ số thay đổi: đi nhanh, bấm hết lái chỉ tới góc làm xe sát giới hạn bám ×1,15 (chạm ngắn ăn lái tỉ lệ).
        // • Thắng tay (Space; cảm ứng: nút Stop) khi > 4 m/s: bánh sau mất bám → cho quay tới handbrakeYawBoost × giới hạn, hướng chạy
        //   trễ handbrakeSlide (0,6 s) → đuôi văng (drift) để quay đầu gấp / ôm cua tay áo; khi đang đánh lái chỉ hãm ~0,6 g (bánh sau).
        [Header("Lái kiểu ô tô thật (21.316e)")]
        public float wheelBase = 2.65f;
        public float maxWheelAngle = 35f;
        public float understeerK = 0.002f;
        public float gripG = 2.6f;                                              // 21.319: 1,7 → 2,6 g (90 km/h: 35 → ~58°/s)
        public float yawLag = 0.045f;                                           // 21.319: 0,09 → 0,045 s
        public float scrubDecel = 2f;                                           // 21.319: 3,5 → 2 m/s²
        public float handbrakeSlide = 0.6f, handbrakeYawBoost = 1.8f;
        float yawRate, latUse;
        public float YawRate => yawRate * Mathf.Rad2Deg;
        public float GripUse => latUse;                 // gia tốc ngang / giới hạn bám (0..1+)
        public bool Understeering { get; private set; }
        public bool Drifting { get; private set; }
        // 21.316g: mức trượt lốp 0..1 cho khói + tiếng lốp rít (OpenWorldTireFx): sau = drift/phanh gấp, trước = phanh gấp/thiếu lái
        public float RearSlip { get; private set; }
        public float FrontSlip { get; private set; }
        float steerVal;
        public float SteerValue => steerVal;
        // 21.316b (người dùng: «xe như xoay tại chỗ», camera xoay theo nhiều) — cảm giác vào cua:
        // • Hướng CHẠY (moveDir) đuổi theo hướng ĐẦU XE theo hàm mũ, thời gian gripLow (chạy chậm, gần như bám ngay) → gripHigh (tốc
        //   độ tối đa): vào cua đầu xe chúc vào trong trước, đuôi trượt nhẹ (~6–8° khi cua gắt ở tốc độ cao) rồi xe mới đổi đường chạy.
        // • Thân xe (mô hình) nghiêng ra ngoài cua theo gia tốc ngang (tốc độ quay × tốc độ), tối đa bodyRollMax, mượt 0,15 s.
        // • Camera (OpenWorldCamera) bám theo hướng CHẠY thay vì hướng đầu xe.
        [Header("Cảm giác vào cua (21.316b)")]
        public float gripLow = 0.02f, gripHigh = 0.05f;                        // 21.319: 0,04/0,11 → 0,02/0,05 s (xe đi theo đầu xe, bớt «lôi cả xe»; 21.316e: 0,04/0,11)
        public float bodyRollMax = 3.5f, bodyRollGain = 0.16f;
        Vector3 moveDir; float yawRateNow, bodyRoll; Transform bodyModel; Quaternion bodyBase;
        public Vector3 MoveDir => moveDir;
        public float BodyRoll => bodyRoll;
        public float SlipAngle { get { var f = transform.forward; f.y = 0; return moveDir.sqrMagnitude > 0.5f ? Vector3.SignedAngle(moveDir, Speed >= 0 ? f : -f, Vector3.up) : 0f; } }
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
            // 21.383 (người dùng): bỏ W/A/S/D — chỉ phím mũi tên ←/→ lái, ↑ ga, ↓ lùi (W giờ là Lazer, E khiên, Q tên lửa, R phanh).
            float x = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
            return new Vector2(x, y);
        }

        // ------------------------------------------------------------ 21.265/21.270: nút lái + bàn đạp cảm ứng
        // • 21.270: góc trái dưới là 2 nút MŨI TÊN TRÁI / PHẢI (thay vô lăng xoay 21.265). Giữ nút = y hệt giữ phím ←/→ trên
        //   máy tính (Input.GetAxisRaw: −1 / +1 ngay lập tức, thả = 0); giữ cả hai = 0. Trượt ngón từ nút này sang nút kia
        //   thì đổi hướng luôn, không cần nhấc tay.
        // • Nút bên phải (giữ để dùng): Ga = tiến (như ↑), Lùi = lùi/phanh (như ↓), Stop = phanh gấp về 0 trong ≤ 1 s;
        //   NHẢY, TÊN LỬA chạm là ăn. Mỗi ngón tay được gán cho đúng một nút khi chạm xuống (đa điểm): vừa giữ nút lái
        //   vừa giữ Ga và chạm TÊN LỬA được (trước 21.265 nút TÊN LỬA là nút IMGUI chỉ nhận một ngón).
        // • 21.286: chạm/vuốt ngoài các nút không làm gì (trước đây nửa phải = NHẢY, vuốt lên cũng nhảy). Bàn phím giữ như cũ.
        [Header("Nút lái + bàn đạp cảm ứng (21.265/21.270)")]
        [Tooltip("Hiện nút cảm ứng cả khi máy không có cảm ứng (thử trong Editor)")] public bool touchControlsAlwaysVisible;
        [System.NonSerialized] public bool testTouchUI;                                                    // kiểm thử: hiện nút cảm ứng
        [System.NonSerialized] public int testSteer;                                                       // kiểm thử: −1 Trái, 1 Phải
        [System.NonSerialized] public int testPedal;                                                       // kiểm thử: 1 Ga, −1 Lùi, 2 Stop
        enum Ctl { None, Left, Right, Gas, Rev, Stop, Jump, Fire, Laser, Shield, Other }
        readonly System.Collections.Generic.Dictionary<int, Ctl> fingers = new System.Collections.Generic.Dictionary<int, Ctl>();
        bool leftHeld, rightHeld, gasHeld, revHeld, stopHeld, laserHeld;
        public bool LaserHeld => laserHeld;                                    // 21.300: giữ nút LAZER (OpenWorldLaser đọc)
        // 21.327 — GA TỰ ĐỘNG (người dùng): chạm nhanh 2 lần vào nút Ga (điện thoại) hoặc phím ↑ (máy tính; 21.383 bỏ W) → xe giữ nguyên
        // vận tốc đang chạy, không cần giữ Ga; nút Ga đổi biểu tượng (quầng xanh lá + nhãn «TỰ ĐỘNG» trong nút; máy tính: thẻ nhỏ
        // bên trái nút Nhảy). Chạm nhanh 2 lần lần nữa → về bình thường. Người dùng chốt: bấm Lùi/P-DRIFT/Space, bị choáng, va
        // chạm mạnh, dính lưới, bị bắt, hết lượt hoặc chơi lại → tự tắt. Đang tự động mà giữ Ga → tăng tốc, thả ra giữ tốc độ mới.
        // Hai lần chạm tính là «nhanh» khi lần bấm thứ hai bắt đầu trong 0,4 s sau lần thứ nhất; xe phải đang chạy ≥ 7 km/h.
        public const float CruiseTapWindow = 0.4f, CruiseMinSpeed = 2f;
        bool cruise; float cruiseSpeed, lastGasTapAt = -10f;
        public bool Cruise => cruise;
        public float CruiseSpeed => cruiseSpeed;
        public int CruiseToggles { get; private set; }
        void GasTap()
        {
            float now = Time.time;
            if (now - lastGasTapAt <= CruiseTapWindow)
            {
                lastGasTapAt = -10f;
                if (cruise) { cruise = false; CruiseToggles++; }
                else if (Speed >= CruiseMinSpeed && !Crashed && !frozen) { cruise = true; cruiseSpeed = Speed; CruiseToggles++; }
                return;
            }
            lastGasTapAt = now;
        }
        public void CancelCruise() { cruise = false; }
        public void TestGasTap() => GasTap();                                  // kiểm thử
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
        // 21.284 (người dùng): Ga + Nhảy to hơn 25 %, Lùi/Stop/Tên lửa to hơn 10 %; hàng dưới canh đáy, Nhảy đứng thẳng trên Ga,
        // Tên lửa canh giữa trên Lùi (thấp hơn Nhảy một chút → hàng trên chạy theo vòng ngón cái).
        // 21.292 (người dùng): Ga + Nhảy và hai mũi tên to thêm 10 % (1,25 → 1,375; 1,5 → 1,65 × nút chuẩn).
        // 21.295 (người dùng): Nhảy, Ga, Stop to thêm 5 % (1,375 → 1,444; 1,1 → 1,155); Tên lửa, Lùi to thêm 10 % (1,1 → 1,21).
        static float BigSize => BtnSize * 1.44375f;
        static float StopSize => BtnSize * 1.155f;
        static float SmallSize => BtnSize * 1.21f;                                  // Lùi (+ Lazer)
        // 21.312 (người dùng): Tên lửa to thêm 5 % (1,21 → 1,2705 × nút chuẩn); nút KHIÊN cùng cỡ.
        static float FireSize => SmallSize * 1.05f;
        public static Rect GasRect => new Rect(Screen.width - BigSize - 24 - OpenWorldGame.SafeRightPx, Screen.height - BigSize - 24 - OpenWorldGame.SafeBottomPx, BigSize, BigSize);
        public static Rect RevRect { get { var g = GasRect; return new Rect(g.x - SmallSize - Gap, g.yMax - SmallSize, SmallSize, SmallSize); } }
        public static Rect StopRect { get { var r = RevRect; return new Rect(r.x - StopSize - Gap, r.yMax - StopSize, StopSize, StopSize); } }
        // Máy tính (không có Ga/Lùi/Stop): giữ bố cục cũ — Nhảy góc phải dưới, Tên lửa ngay trên.
        static bool touchLayout = true;
        public static Rect JumpRect { get { var g = GasRect; return touchLayout ? new Rect(g.x, g.y - BigSize - Gap, BigSize, BigSize) : g; } }
        public static Rect FireRect { get { var r = touchLayout ? RevRect : GasRect; return new Rect(r.center.x - FireSize * .5f, r.y - FireSize - Gap, FireSize, FireSize); } }
        // 21.300 (người dùng): nút LAZER cỡ nút TÊN LỬA, ngay phía trên nút NHẢY (canh giữa). Máy tính: bên trái nút TÊN LỬA.
        public static Rect LaserRect
        {
            get
            {
                if (!touchLayout) { var f = FireRect; return new Rect(f.x - SmallSize - Gap, f.y, SmallSize, SmallSize); }
                // 21.312 (người dùng): xích xuống 0,16 × nút chuẩn (khe 0,14 → −0,02); viền trong suốt 7 % của hai ảnh nút vẫn chừa
                // ~0,17 × nút chuẩn giữa hai tấm nút → không đè nút NHẢY. Vùng chạm NHẢY được xét trước LAZER.
                var j = JumpRect; return new Rect(j.center.x - SmallSize * .5f, j.y - SmallSize - Gap + BtnSize * 0.16f, SmallSize, SmallSize);
            }
        }
        // 21.312 (người dùng): nút KHIÊN phía trên 2 nút TRÁI/PHẢI, canh giữa khe hai nút, cỡ bằng nút TÊN LỬA mới; khe dưới 1,5 × Gap để
        // không lẫn vùng chạm nới rộng của nút lái. Máy tính (không có nút lái): 21.383 (người dùng) bên trái nút NHẢY, canh giữa theo
        // chiều cao nút NHẢY (trước: bên trái nút LAZER); phím E.
        public static Rect ShieldRect
        {
            get
            {
                if (!touchLayout) { var j = JumpRect; return new Rect(j.x - FireSize - Gap, j.center.y - FireSize * .5f, FireSize, FireSize); }
                Rect l = LeftRect, r = RightRect; float cx = (l.xMax + r.x) * 0.5f;
                return new Rect(cx - FireSize * .5f, l.y - FireSize - Gap * 1.5f, FireSize, FireSize);
            }
        }
        // 21.270: hai nút mũi tên góc trái dưới (phải giữ liên tục). 21.284: to hơn 25 % (1,2 → 1,5 × nút chuẩn).
        static float SteerSize => BtnSize * 1.65f;
        public static Rect LeftRect => new Rect(OpenWorldGame.SafeLeftPx + 24, Screen.height - SteerSize - 24 - OpenWorldGame.SafeBottomPx, SteerSize, SteerSize);
        public static Rect RightRect { get { var l = LeftRect; return new Rect(l.xMax + Gap, l.y, SteerSize, SteerSize); } }
        static Vector2 ToGui(Vector2 screen) => new Vector2(screen.x, Screen.height - screen.y);
        static float Grow(Rect r, Vector2 p, float pad) => (p.x >= r.x - pad && p.x <= r.xMax + pad && p.y >= r.y - pad && p.y <= r.yMax + pad) ? 1f : 0f;

        Ctl HitControl(Vector2 g)
        {
            float pad = BtnSize * 0.06f;
            // 21.312: KHIÊN xét trước nút lái nhưng chỉ trong tấm nút thật (bỏ viền trong suốt 7 %), để chạm hụt lên trên nút lái vẫn là lái
            var sr = ShieldRect; float si = sr.width * 0.07f;
            if (shieldRef && Grow(new Rect(sr.x + si, sr.y + si, sr.width - 2 * si, sr.height - 2 * si), g, 0f) > 0) return Ctl.Shield;
            var s = SteerControl(g); if (s != Ctl.None) return s;
            if (Grow(FireRect, g, pad) > 0 && FireVisible) return Ctl.Fire;
            if (Grow(JumpRect, g, pad) > 0) return Ctl.Jump;
            if (Grow(LaserRect, g, pad) > 0 && laserRef) return Ctl.Laser;
            if (Grow(GasRect, g, pad) > 0) return Ctl.Gas;
            if (Grow(RevRect, g, pad) > 0) return Ctl.Rev;
            if (Grow(StopRect, g, pad) > 0) return Ctl.Stop;
            // 21.286 (người dùng): bỏ «chạm/vuốt ở nửa phải màn hình = nhảy» — chỉ nút NHẢY mới nhảy.
            return Ctl.Other;
        }
        bool FireVisible => hazardsRef && hazardsRef.enabled;
        OpenWorldLaser laserRef;
        OpenWorldShield shieldRef;
        public OpenWorldShield Shield { get { if (!shieldRef) shieldRef = GetComponent<OpenWorldShield>(); return shieldRef; } }
        public bool Shielded => Shield && shieldRef.Active;
        public bool ShieldHeld { get { foreach (var k in fingers.Values) if (k == Ctl.Shield) return true; return false; } }
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
            if (c == Ctl.Gas) GasTap();                                        // 21.327: chạm 2 lần → Ga tự động
            if (c == Ctl.Jump) jumpRequest = true;
            else if (c == Ctl.Fire && hazardsRef) { hazardsRef.RequestFire(); FireTaps++; }
            else if (c == Ctl.Shield && Shield) shieldRef.requested = true;
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
            if (!laserRef) laserRef = GetComponent<OpenWorldLaser>();
            if (!shieldRef) shieldRef = GetComponent<OpenWorldShield>();
            touchLayout = touchControlsAlwaysVisible || Input.touchSupported || UnityEngine.Device.Application.isMobilePlatform || testTouchUI;
            if (!TouchControlsVisible) { fingers.Clear(); leftHeld = rightHeld = gasHeld = revHeld = stopHeld = laserHeld = false; return; }
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
            leftHeld = rightHeld = gasHeld = revHeld = stopHeld = laserHeld = false;
            foreach (var c in fingers.Values)
            {
                if (c == Ctl.Laser) laserHeld = true;
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
        public static Texture2D LaserButtonTex { get; private set; }
        public static Texture2D ShieldButtonTex { get; private set; }
        public static void EnsureTouchArt() => LoadTouchArt();
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
            LaserButtonTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Laser");
            ShieldButtonTex = Resources.Load<Texture2D>("TouchControls/PV_Btn_Shield");     // 21.312 (ArtSource/UI/build_shield_button.py)
        }

        // Vẽ một nút ảnh: đang giữ → thu nhỏ 7 % + quầng màu nhấn phía sau; tắt (hồi chiêu/hết đạn) → xám mờ.
        // 21.284: trong suốt hơn để không che cảnh — để yên 68 % (nền nút trong ảnh cũng mỏng hơn), đang giữ 100 %.
        public static void DrawArtButton(Rect b, Texture2D tex, Color accent, bool held, bool enabled = true, float alpha = .68f)
        {
            if (held) Disc(new Rect(b.x - b.width * .06f, b.y - b.width * .06f, b.width * 1.12f, b.height * 1.12f), new Color(accent.r, accent.g, accent.b, .38f));
            var q = held ? new Rect(b.x + b.width * .035f, b.y + b.height * .035f, b.width * .93f, b.height * .93f) : b;
            var tint = enabled ? new Color(1f, 1f, 1f, held ? 1f : alpha) : new Color(.55f, .58f, .62f, .42f);
            if (tex) GUI.DrawTexture(q, tex, ScaleMode.ScaleToFit, true, 0, tint, 0, 0);
            else Bar(q, new Color(.1f, .15f, .2f, .7f * tint.a), q.width * .2f);
        }

        // Máy tính: chữ phím nhỏ ở góc nút.
        public static void KeyBadge(Rect b, string key)
        {
            float s = b.width * .26f; bool wide = key.Length > 2;          // 21.375: «SPACE» → viên thuốc dài thay vòng tròn
            float w = wide ? s * 2.4f : s;
            var q = new Rect(b.xMax - w - s * .05f, b.y + s * .05f, w, s);
            Bar(q, new Color(.06f, .12f, .19f, .92f), s * .5f);
            var st = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(s * (wide ? .42f : .58f)) };
            st.normal.textColor = new Color(1f, .96f, .84f); GUI.Label(q, key, st);
        }

        void DrawTouchControls()
        {
            if (!TouchControlsVisible) return;
            LoadTouchArt();
            var steerAccent = new Color(.18f, .78f, .91f);
            DrawArtButton(LeftRect, leftTex, steerAccent, LeftHeld);
            DrawArtButton(RightRect, rightTex, steerAccent, RightHeld);
            if (cruise) DrawCruiseGas(GasRect); else DrawArtButton(GasRect, gasTex, new Color(1f, .54f, .24f), GasHeld);
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
            if (transform.position.y < WaterY + 0.3f)
            {
                // 21.365: lưu vận tốc + điểm chạm nước trước khi bị kìm lại để OpenWorldWaterSplash dựng nước văng theo cú lao.
                WaterEntryVelocity = rb.linearVelocity; WaterEntryPoint = transform.position;
                sinking = true; sinkTime = 0; RiverFalls++; jumping = false; return;
            }
            // 21.255: xe hỏng (trúng đạn pháo/khủng long/đâm xe tăng) → văng theo vật lý, khóa lái, nằm chờ CSGT.
            if (Crashed) { Speed = 0; return; }
            TickJump(dt);
            TickUnstick(dt);
            if (TickKnockFall()) return;                                      // 21.366: đụng vật cản trên không → rơi thẳng xuống
            bool stunned = Time.time < stunUntil, stopping = Time.time < stopUntil;
            if (Time.time < flingUntil) { Speed = 0f; return; }               // 21.259: đang văng sau cú húc — để vật lý tự bay
            if (transform.position.y > -0.2f && Mathf.Abs(rb.linearVelocity.y) < 0.5f && Time.time - lastSafeTime > 1f)
            { lastSafePos = transform.position; lastSafeRot = transform.rotation; lastSafeTime = Time.time; }
            bool netted = Time.time < netUntil;                              // 21.299: dính lưới điện CSGT
            bool locked = frozen || stunned || stopping || netted;
            var inp = locked ? Vector2.zero : ReadInput();
            // 21.265/21.270: nút ← / → + Ga/Lùi trên màn hình cảm ứng (bàn phím/kiểm thử vẫn ưu tiên khi có tín hiệu)
            if (!locked && scriptedInput == Vector2.zero && TouchDriving)
            {
                if (Mathf.Abs(inp.x) < 0.05f) inp.x = TouchSteer;
                if (Mathf.Abs(inp.y) < 0.05f) inp.y = GasHeld ? 1f : RevHeld ? -1f : 0f;
            }
            bool hand = locked || Input.GetKey(KeyCode.R);   // 21.383: phanh tay R (21.375 J, trước Space)
            bool hardStop = !locked && StopHeld && TouchControlsVisible;
            // Bị nhà/cây/cột chặn: tốc độ trên HUD tụt theo vận tốc thực.
            var fwd0 = transform.forward; fwd0.y = 0; fwd0.Normalize();
            float actual = Vector3.Dot(rb.linearVelocity, fwd0);
            if (Mathf.Abs(actual) < Mathf.Abs(Speed) - 4f && Time.time - spawnTime > 0.5f) Speed = Mathf.MoveTowards(Speed, actual, 60f * dt);
            if (hardStop) inp.y = 0f;
            // 21.316e: vòng ma sát — đang cua gắt thì ga kéo được ít hơn
            float traction = Mathf.Max(0.5f, Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Clamp01(latUse) * Mathf.Clamp01(latUse))));
            // 21.327: Ga tự động — tắt khi phanh/lùi/thắng tay hoặc bị khóa lái (choáng, va, lưới, bị bắt)
            if (cruise && (locked || inp.y < -0.05f || hardStop || Input.GetKey(KeyCode.R))) cruise = false;
            if (inp.y > 0.05f) Speed = Mathf.MoveTowards(Speed, maxSpeed * inp.y, (Speed < 0 ? brake : accel * traction) * dt);
            else if (inp.y < -0.05f) Speed = Mathf.MoveTowards(Speed, -reverseSpeed, (Speed > 0 ? brake : accel) * dt);
            else if (cruise) Speed = Mathf.MoveTowards(Speed, cruiseSpeed, (Speed < cruiseSpeed ? accel * traction : drag) * dt);
            else Speed = Mathf.MoveTowards(Speed, 0f, drag * dt);
            if (cruise && inp.y > 0.05f) cruiseSpeed = Mathf.Max(cruiseSpeed, Speed);          // giữ Ga khi tự động → giữ tốc độ mới
            float steer, v = Speed, vAbs = Mathf.Abs(v), demand = 0f;
            bool driftBrake = false;
            if (classicSteer)
            {   // 21.329: lái gốc 1.89.0 — quay đều classicSteerDegPerSec (95°/s) khi giữ nút/phím, mạnh dần tới 6 m/s, đảo chiều khi lùi
                steerVal = Mathf.Clamp(inp.x, -1f, 1f);
                steer = inp.x * classicSteerDegPerSec * Mathf.Clamp01(vAbs / 6f) * Mathf.Sign(Speed == 0 ? 1 : Speed) * dt;
                yawRate = 0f; latUse = 0f; Understeering = false; Drifting = false;
                if (hand) Speed = Mathf.MoveTowards(Speed, 0f, brake * 1.5f * dt);
                if (netted) { Speed = Mathf.MoveTowards(Speed, 0f, 32f * dt); steer = 0f; }
            }
            else
            {
            // 21.316: vô lăng ảo (góc lái tăng dần steerRise, trả về steerReturn)
            float steerTarget = Mathf.Clamp(inp.x, -1f, 1f);
            bool easing = Mathf.Abs(steerTarget) < Mathf.Abs(steerVal) || steerTarget * steerVal < 0f;
            steerVal = Mathf.MoveTowards(steerVal, steerTarget, dt / Mathf.Max(0.01f, easing ? steerReturn : steerRise));
            // 21.316e: mô hình xe hai trục + giới hạn bám lốp (xem chú thích ở phần khai báo)
            bool handbrake = !locked && (Input.GetKey(KeyCode.R) || hardStop) && vAbs > 4f;
            bool braking = handbrake || (inp.y < -0.05f && v > 0.5f);
            float mu = gripG * 9.81f * (braking ? 1.12f : inp.y > 0.05f ? 0.96f : 1f);
            float rLim = mu / Mathf.Max(vAbs, 0.5f);
            // trợ lực lái biến thiên (như xe thể thao có hệ thống lái tỉ số thay đổi): đi nhanh, bấm hết lái chỉ đưa bánh tới góc làm xe
            // sát giới hạn bám (×1,15) → chạm phím ngắn ăn lái tỉ lệ, không vượt bám liên tục; thắng tay thì cho hết góc.
            float wb = wheelBase + understeerK * v * v;
            float deltaMax = maxWheelAngle * Mathf.Deg2Rad;
            if (!handbrake) deltaMax = Mathf.Min(deltaMax, Mathf.Atan(rLim * 1.15f * wb / Mathf.Max(vAbs, 0.5f)));
            float delta = steerVal * deltaMax;
            float rKin = v * Mathf.Tan(delta) / wb;
            demand = Mathf.Abs(rKin) / rLim;
            float rTarget = handbrake ? Mathf.Clamp(rKin * 1.3f, -rLim * handbrakeYawBoost, rLim * handbrakeYawBoost) : Mathf.Clamp(rKin, -rLim, rLim);
            yawRate = Mathf.Lerp(yawRate, locked ? 0f : rTarget, 1f - Mathf.Exp(-dt / Mathf.Max(0.01f, yawLag)));
            steer = yawRate * Mathf.Rad2Deg * dt;
            latUse = Mathf.Abs(yawRate * v) / mu;
            Understeering = !handbrake && demand > 1.05f && vAbs > 8f;
            Drifting = handbrake && Mathf.Abs(yawRate) > 0.35f;
            if (vAbs > 3f)
            {   // lốp trượt / cản khi cua → xe tự giảm tốc khi ôm cua gắt
                float scrub = scrubDecel * Mathf.Clamp01(demand - 1f) + 0.9f * Mathf.Clamp01(latUse) * Mathf.Clamp01(latUse) + (handbrake ? 2.5f : 0f);   // 21.319: cản cua 2,2 → 0,9
                Speed = Mathf.MoveTowards(Speed, 0f, scrub * dt);
            }
            // 21.316e: thắng tay khi đang đánh lái chỉ hãm bánh sau (~0,6 g) để xe trượt xoay (drift) thay vì đứng khựng
            driftBrake = handbrake && Mathf.Abs(steerVal) > 0.3f;
            if (driftBrake) Speed = Mathf.MoveTowards(Speed, 0f, 6f * dt);
            else if (hand) Speed = Mathf.MoveTowards(Speed, 0f, brake * 1.5f * dt);
            if (netted) { Speed = Mathf.MoveTowards(Speed, 0f, 32f * dt); steer = 0f; yawRate = 0f; }   // lưới ghì xe dừng gấp, không lái
            }
            // Stop: phanh gấp — từ tốc độ tối đa về 0 trong 1 giây
            {   // 21.316g: mức trượt lốp
                bool grounded = !jumping;
                float hardBrake = !locked && (hand || hardStop) && !driftBrake ? Mathf.InverseLerp(5f, 18f, vAbs) : 0f;
                float pedalBrake = !locked && inp.y < -0.05f && v > 6f ? Mathf.InverseLerp(6f, 25f, v) * 0.45f : 0f;
                float drift = driftBrake || Drifting ? Mathf.Clamp01(0.55f + Mathf.Abs(SlipAngle) / 45f) * Mathf.InverseLerp(2f, 8f, vAbs) : 0f;
                float under = Understeering ? Mathf.Clamp01((demand - 1f) * 1.5f) * 0.55f : 0f;
                float edge = vAbs > 12f ? Mathf.Clamp01((latUse - 0.9f) / 0.1f) * 0.3f : 0f;
                RearSlip = grounded ? Mathf.Max(drift, hardBrake * 0.9f, pedalBrake, edge * 0.6f) : 0f;
                FrontSlip = grounded ? Mathf.Max(hardBrake * 0.75f, pedalBrake * 0.8f, under, edge, drift * 0.3f) : 0f;
            }
            if (hardStop && !driftBrake) { if (Mathf.Abs(Speed) > 0.05f && Mathf.Abs(Speed) > maxSpeed * dt) StopBrakes++; Speed = Mathf.MoveTowards(Speed, 0f, maxSpeed * dt); }
            if (Time.time < joltUntil && !jumping)
            {
                float k = (joltUntil - Time.time) / Mathf.Max(0.01f, joltLength);
                steer += joltYaw * k * Mathf.Clamp01(Mathf.Abs(Speed) / 6f) * dt;
                // Lái ngược hướng lệch thì lấy lại hướng nhanh hơn (như game cũ: chỉnh ngược chiều để cân bằng).
                if (inp.x * joltYaw < 0f) joltUntil -= dt * 1.5f;
            }
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, steer, 0f));
            yawRateNow = steer / dt;
            var fwd = transform.forward; fwd.y = 0; fwd.Normalize();
            var vel = rb.linearVelocity;
            // 21.316b: hướng chạy trễ sau hướng đầu xe (trượt nhẹ khi cua); chạy chậm/vừa va chạm/lùi đổi chiều → bám ngay
            float spAbs = Mathf.Abs(Speed);
            var want = Speed >= 0f ? fwd : -fwd;
            if (classicSteer || moveDir.sqrMagnitude < 0.5f || spAbs < 1.5f || Vector3.Angle(moveDir, want) > (Drifting ? 80f : 45f)) moveDir = want;   // 21.329: lái gốc không trượt
            else
            {
                // 21.316e: sát giới hạn bám thì trượt nhiều hơn; thắng tay → đuôi văng (drift)
                float tau = Drifting ? handbrakeSlide : Mathf.Lerp(gripLow, gripHigh, Mathf.InverseLerp(8f, maxSpeed, spAbs)) * (1f + 0.3f * Mathf.Clamp01(latUse - 0.7f) / 0.3f);
                moveDir = Vector3.Slerp(moveDir, want, 1f - Mathf.Exp(-dt / Mathf.Max(0.005f, tau))); moveDir.y = 0f; moveDir.Normalize();
            }
            var target = moveDir * spAbs; target.y = vel.y;
            rb.linearVelocity = Vector3.Lerp(vel, target, classicSteer ? 0.35f : 0.5f);   // 21.319: 0,35 → 0,5 (vận tốc bám hướng chạy nhanh hơn); 21.329 lái gốc: 0,35
        }

        // ------------------------------------------------------------ 21.255: nhảy như game cũ
        // Đỉnh cố định 4,1166525 m so với mặt đỡ lúc phóng (21.100), gia tốc lên 24,2 m/s², rơi 22 m/s², hồi 2 s,
        // chỉ nhảy khi bánh chạm mặt đường/cầu. Phím Space (21.375; J là phanh tay) hoặc nút NHẢY (21.286: bỏ chạm nửa phải màn hình).
        // 21.295 (người dùng): đáp đất là nhảy lại được ngay — bỏ thời gian hồi 2 s tính từ lúc phóng (cũ: chờ thêm ~0,8 s sau khi
        // đáp). Chỉ chặn bấm lặp trong 0,3 s đầu; bấm Nhảy hơi sớm (≤ 0,2 s trước khi chạm đất) được giữ lại và phóng ngay khi đáp.
        public const float JumpApex = 4.1166525f, JumpRise = 24.2f, JumpFall = 22f, JumpCooldown = 0.3f, JumpBuffer = 0.2f;
        public bool Airborne => jumping;
        public float Lift => jumping ? Mathf.Max(0f, transform.position.y - jumpBase) : 0f;   // độ cao so với mặt đỡ lúc phóng
        public int Jumps { get; private set; }
        public int Landings { get; private set; }
        public float LandedAt { get; private set; } = -10f;
        public float JumpedAt => jumpAt;
        bool jumping, jumpRequest; float jumpBase, jumpAt = -10f, jumpRequestAt = -10f;
        public void RequestJump() { jumpRequest = true; }
        // Chạm đất: tia dò ở tâm xe, hoặc mặt dưới thân xe đang tì lên vật gì đó (bồn cây, nóc xe khác…) — 21.258.
        float supportAt = -10f;
        bool Grounded()
        {
            return Time.time - supportAt < 0.12f || Physics.Raycast(transform.position + Vector3.up * 0.35f, Vector3.down, 0.6f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
        }
        void OnCollisionStay(Collision c)
        {
            if (!knockFall && (jumping || Time.time - supportAt > 0.12f) && !IsRoad(c.collider) && c.contactCount > 0)
            {
                var c0 = c.GetContact(0);
                if (c0.normal.y < 0.3f) AirKnock(c0.normal, Mathf.Abs(Vector3.Dot(c.relativeVelocity, c0.normal)));   // 21.366
            }
            for (int i = 0; i < c.contactCount; i++)
            {
                if (c.GetContact(i).normal.y <= 0.55f) continue;
                supportAt = Time.time;
                if (IsRoad(c.collider)) roadSupportAt = Time.time; else { otherSupport = c.collider; otherSupportAt = Time.time; }
                return;
            }
        }

        // ------------------------------------------------------------ 21.284: không kẹt lơ lửng trên xe khác / mép vật
        // Lỗi: đang nhảy mà va xe NPC → xe người chơi rơi lên nóc/mép xe kia. Thân xe bị khóa nghiêng (FreezeRotationX/Z) nên
        // tì một góc lên mép là treo luôn, không trượt xuống; xe NPC bên dưới lại chờ người chơi rời chỗ mới về làn (WaitRecover,
        // không thu hồi khi còn trong khung hình) → kẹt mãi. Sửa: xe đứng yên theo phương đứng, cao hơn mặt đường ≥ 0,35 m, chỉ
        // tì lên xe khác (hoặc vật tĩnh thấp ≥ 0,6 m mà không nhích được) quá 0,3 s → tạm bỏ va chạm với vật đó, đẩy xe trượt ra
        // phía gần nhất và để rơi xuống đường; hết chồng lấn thì bật va chạm lại.
        public int Unsticks { get; private set; }
        float roadSupportAt = -10f, otherSupportAt = -10f, perchTime, unstickClearAt, unstickGiveUpAt;
        Collider otherSupport, body;
        readonly List<Collider> unstickIgnored = new List<Collider>();
        readonly RaycastHit[] roadHits = new RaycastHit[8];

        static bool IsRoad(Collider c) => c && c.attachedRigidbody == null && (c.name == "MatDat" || c.name == "MatCau");

        // Độ cao mặt đường/mặt cầu ngay dưới xe (bỏ nóc xe, bồn cây…); NaN nếu không thấy.
        float RoadBelow()
        {
            var p = transform.position;
            int n = Physics.RaycastNonAlloc(p + Vector3.up * 0.5f, Vector3.down, roadHits, 30f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore);
            float best = float.NaN;
            for (int i = 0; i < n; i++)
                if (IsRoad(roadHits[i].collider) && roadHits[i].point.y <= p.y + 0.3f && (float.IsNaN(best) || roadHits[i].point.y > best)) best = roadHits[i].point.y;
            return best;
        }

        void TickUnstick(float dt)
        {
            if (!body) body = GetComponent<Collider>();
            if (unstickIgnored.Count > 0 && Time.time >= unstickClearAt)
            {
                bool clear = true;
                foreach (var c in unstickIgnored)
                    if (c && c.enabled && c.gameObject.activeInHierarchy && Physics.ComputePenetration(body, body.transform.position, body.transform.rotation, c, c.transform.position, c.transform.rotation, out _, out _)) { clear = false; break; }
                if (clear || Time.time >= unstickGiveUpAt) RestoreUnstick();
            }
            bool onOther = otherSupport && Time.time - otherSupportAt < 0.12f && Time.time - roadSupportAt > 0.12f;
            float road = onOther ? RoadBelow() : float.NaN;
            float h = float.IsNaN(road) ? 0f : transform.position.y - road;
            bool vehicle = onOther && otherSupport.attachedRigidbody != null && otherSupport.attachedRigidbody != rb;
            var flat = rb.linearVelocity; flat.y = 0;
            bool perched = onOther && Mathf.Abs(rb.linearVelocity.y) < 0.8f
                           && (vehicle ? h > 0.35f : h > 0.6f && flat.magnitude < 0.6f && otherSupport.bounds.size.y < 3f);
            perchTime = perched ? perchTime + dt : 0f;
            if (perchTime < (vehicle ? 0.3f : 0.6f)) return;
            perchTime = 0f; Unsticks++;
            var sup = otherSupport;
            var away = transform.position - sup.bounds.center; away.y = 0;
            if (away.sqrMagnitude < 0.04f) away = transform.right; away.Normalize();
            var cols = vehicle ? sup.attachedRigidbody.GetComponentsInChildren<Collider>() : new[] { sup };
            foreach (var c in cols) if (c && !c.isTrigger && !unstickIgnored.Contains(c)) { Physics.IgnoreCollision(body, c, true); unstickIgnored.Add(c); }
            unstickClearAt = Time.time + 0.4f; unstickGiveUpAt = Time.time + 5f;
            // trượt ra mép gần nhất ~2,2 m trong 0,5 s rồi rơi theo trọng lực (lái/ga khóa trong lúc trượt như khi bị văng)
            rb.linearVelocity = away * 4.5f + Vector3.up * Mathf.Min(0f, rb.linearVelocity.y);
            flingUntil = Mathf.Max(flingUntil, Time.time + 0.5f);
            Speed = 0f; jumping = false; otherSupport = null;
        }

        void RestoreUnstick()
        {
            if (!body) body = GetComponent<Collider>();
            foreach (var c in unstickIgnored) if (c && body) Physics.IgnoreCollision(body, c, false);
            unstickIgnored.Clear();
        }

        // ------------------------------------------------------------ 21.366: đụng vật cản lúc đang trên không → rơi xuống ngay
        // Lỗi (người dùng: «xe đang nhảy mà đụng vật cản thì bị dừng ở độ cao đó luôn cho đến 1 lúc rồi mới rớt xuống»): trên
        // không, FixedUpdate vẫn kéo vận tốc ngang về hướng chạy × Speed (Ga tự động còn giữ Speed) → mỗi bước vật lý xe bị ép
        // vào thân cây/cột/tường/xe khác; lực ép lớn nên ma sát giữ xe dính trên vật cản, chỉ rơi khi Speed tụt hết.
        // Sửa: chạm sườn (pháp tuyến ngang) khi xe cao hơn mặt đường > AirKnockHeight → bỏ vận tốc lao vào vật, bật nhẹ ra khỏi
        // vật, vận tốc đứng ≤ −1,5 m/s, Speed = 0, khóa lái/ga và kéo xuống bằng gia tốc rơi của cú nhảy (22 m/s²) tới khi chạm
        // đất (tối đa 3 s). Áp dụng mọi vật cản: cây, cột, tường, lan can, bồn cây, xác xe, CSGT, xe NPC (cả khi có khiên).
        public const float AirKnockHeight = 0.5f;
        public int AirKnocks { get; private set; }
        public bool AirKnocked => knockFall;
        bool knockFall; float knockAt = -10f;

        bool HighInAir()
        {
            if (!jumping && Grounded()) return false;
            float road = RoadBelow();
            return float.IsNaN(road) || transform.position.y - road > AirKnockHeight;
        }

        // n: pháp tuyến chỗ chạm (hướng từ vật cản về phía xe). Trả về true nếu đã xử lý như cú đụng trên không.
        bool AirKnock(Vector3 n, float into)
        {
            if (Crashed || sinking || n.y >= 0.55f || !HighInAir()) return false;
            if (knockFall && Time.time - knockAt < 0.15f) return true;
            n.y = 0f; if (n.sqrMagnitude < 0.01f) n = -transform.forward; n.Normalize();
            var v = rb.linearVelocity; var flat = v; flat.y = 0f;
            var tangent = flat - n * Vector3.Dot(flat, n);
            var nv = tangent * 0.3f + n * Mathf.Clamp(into * 0.25f, 1.5f, 4f);
            nv.y = Mathf.Min(v.y, -1.5f);
            rb.linearVelocity = nv; rb.angularVelocity = Vector3.zero;
            Speed = 0f; cruise = false; knockFall = true; knockAt = Time.time; AirKnocks++;
            return true;
        }

        // true = đang rơi sau cú đụng (bỏ qua phần lái/ga trong FixedUpdate)
        bool TickKnockFall()
        {
            if (!knockFall) return false;
            Speed = 0f;
            if (!jumping) rb.AddForce(Vector3.down * (JumpFall + Physics.gravity.y), ForceMode.Acceleration);   // khi nhảy TickJump đã kéo 22 m/s²
            bool landed = Time.time - knockAt > 0.1f && rb.linearVelocity.y <= 0.5f && Grounded();
            if (landed || Time.time - knockAt > 3f) { knockFall = false; return false; }
            return true;
        }
        void TickJump(float dt)
        {
            if (jumpRequest) { jumpRequest = false; jumpRequestAt = Time.time; }
            if (jumping)
            {
                float g = -Physics.gravity.y;
                rb.AddForce(Vector3.down * ((rb.linearVelocity.y > 0 ? JumpRise : JumpFall) - g), ForceMode.Acceleration);
                if (Time.time - jumpAt > 0.25f && rb.linearVelocity.y <= 0.5f && Grounded()) { jumping = false; Landings++; LandedAt = Time.time; }
            }
            if (!jumping && Time.time - jumpRequestAt <= JumpBuffer && !frozen && Time.time >= netUntil && Time.time - jumpAt >= JumpCooldown && Grounded())
            {
                jumpRequestAt = -10f;
                jumping = true; jumpAt = Time.time; jumpBase = transform.position.y; Jumps++;
                var v = rb.linearVelocity; v.y = Mathf.Sqrt(2f * JumpRise * JumpApex); rb.linearVelocity = v;
            }
        }
        void Update()
        {
            UpdateStars();
            UpdateBodyRoll();
            TickStunSmoke();
            UpdateTouchControls();
            // Khói xe hỏng luôn bốc thẳng lên trời dù xe lật ngửa.
            if (smoke && Crashed) smoke.transform.SetPositionAndRotation(transform.position + Vector3.up * 0.9f, Quaternion.identity);
            if (burn) burn.transform.SetPositionAndRotation(transform.position + Vector3.up * 0.7f, Quaternion.identity);   // 21.312: lửa luôn bốc lên
            // 21.375 (người dùng): máy tính nhảy bằng Space (trước J; J giờ là phanh tay). Bỏ qua 0,3 s đầu lượt vì Space cũng
            // là phím «Lên xe» ở màn chào/màn kết thúc — tránh vừa vào lượt đã nhảy.
            if (Input.GetKeyDown(KeyCode.Space) && Time.time - spawnTime > 0.3f) jumpRequest = true;
            if (Input.GetKeyDown(KeyCode.UpArrow)) GasTap();   // 21.327: ↑ chạm 2 lần → Ga tự động (21.383: bỏ W — W là Lazer)
        }

        // 21.316b: nghiêng thân xe (chỉ mô hình con, không đụng vật lý) ra ngoài cua
        void UpdateBodyRoll()
        {
            if (!bodyModel)
            {
                int best = 0;
                foreach (Transform c in transform)
                {
                    if (c.name.StartsWith("VoiKhoi") || c.name.StartsWith("VongKhoi") || c.name.StartsWith("Khoi") || c.name == "VongChoang") continue;
                    int n = c.GetComponentsInChildren<Renderer>(true).Length;
                    if (n > best) { best = n; bodyModel = c; }
                }
                if (!bodyModel) return;
                bodyBase = bodyModel.localRotation;
            }
            bool calm = Crashed || jumping || frozen || sinking || knockFall || Time.time < flingUntil;
            float want = calm || classicSteer ? 0f : Mathf.Clamp(yawRateNow * Mathf.Deg2Rad * Speed * bodyRollGain, -bodyRollMax, bodyRollMax);
            bodyRoll = Mathf.Lerp(bodyRoll, want, 1f - Mathf.Exp(-Time.deltaTime / 0.15f));
            bodyModel.localRotation = bodyBase * Quaternion.Euler(0f, 0f, bodyRoll);   // cua phải (yaw +) → roll + : sườn phải nhổm lên, sườn trái (ngoài cua) hạ
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
        float stunUntil, stopUntil, flingUntil, stunAt, stunTotal = 1f, crashShownAt;
        OpenWorldCarDent dent;
        Transform stars; readonly List<Transform> starList = new List<Transform>();
        ParticleSystem stunSmoke; float stunSmokeUntil;

        // 21.261: húc đổ tấm hàng rào trước cửa hàng — mất ~15 % tốc độ, móp nhẹ.
        public int FenceHits { get; private set; }
        public int WallDents { get; private set; }                       // 21.324: số lần móp do tông xe CSGT
        public int ObstacleDents { get; private set; }                   // 21.325: số lần móp do đụng cảnh quan/vật cản
        public const float ObstacleDentMin = 3f;                         // 21.325: vận tốc đâm vào tối thiểu (m/s) để móp
        public OpenWorldCarDent Damage => dent;
        public void HitFence(Vector3 point, float speed)
        {
            if (Crashed) return;
            FenceHits++; Speed *= 0.85f;
            if (speed > ObstacleDentMin && !Shielded) { Dent(point, speed * 0.7f); ObstacleDents++; }     // 21.325: 0,3 → 0,7 (rõ hơn)
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
            if (dent == null) dent = new OpenWorldCarDent(transform, GetComponentsInChildren<Renderer>(false)) { paint = new Color(.13f, .36f, .86f) };
            var outward = point - transform.position; outward.y = 0;
            dent.Dent(point, outward, closing);
        }

        void HitVehicle(Collision c, float otherMass)
        {
            var ct = c.GetContact(0); float closing = c.relativeVelocity.magnitude;
            bool air = HighInAir();                                             // 21.366
            if (closing < OpenWorldTraffic.MinClosing) { if (air) AirKnock(ct.normal, closing); return; }
            if (Shielded) { ShieldBump(ct.point, closing); if (air) AirKnock(ct.normal, closing); return; }   // 21.312: khiên — không văng/choáng/nổ, chỉ giảm tốc
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
                if (air) { var fv = rb.linearVelocity; fv.y = -1.5f; rb.linearVelocity = fv; knockFall = true; knockAt = Time.time; AirKnocks++; }   // 21.366: trên không → văng ra + rơi, không hất lên
                rb.angularVelocity = new Vector3(0, (Random.value < .5f ? -1f : 1f) * (2f + k * 3f) * fu, 0);
                flingUntil = Time.time + (0.6f + k * 0.6f) * Mathf.Clamp(f, 0.4f, 1f);
                stunUntil = Time.time + o.stun; stunAt = Time.time; stunTotal = Mathf.Max(.1f, o.stun);
                Speed = 0f; Stuns++; jumping = false;
                StunSmoke(o.stun);
                return;
            }
            // nhẹ: đổi hướng nhẹ, phanh gấp tới dừng (21.366: trên không → rơi xuống ngay)
            if (air && AirKnock(ct.normal, closing)) { Bumps++; StunSmoke(0.9f); return; }
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
        // 21.312: khiên chặn va chạm — chỉ giảm tốc (cú mạnh giảm nhiều hơn), khiên gợn sóng + tóe tia ở chỗ chạm.
        public int ShieldBumps { get; private set; }
        void ShieldBump(Vector3 at, float closing)
        {
            ShieldBumps++;
            Speed *= Mathf.Lerp(0.8f, 0.45f, Mathf.InverseLerp(2f, 30f, closing));
            if (Shield) shieldRef.Absorb(at, Mathf.InverseLerp(2f, 30f, closing));
        }
        public void ClearNet() { netUntil = 0f; }

        // 21.312 (người dùng): trúng đạn pháo/tên lửa → xe bốc cháy, bốc khói rồi nổ tung (OpenWorldHazards gọi sau Crash).
        ParticleSystem burn;
        public bool Burning => burn && burn.isPlaying;
        public void Ignite(ParticleSystem fire, float explodeAfter)
        {
            if (!Crashed) return;
            if (burn && burn != fire) Destroy(burn.gameObject);
            burn = fire;
            if (explodeAfter >= 0f && ExplodeAt <= 0f) ExplodeAt = Time.time + explodeAfter;
        }
        // nổ tung: hất xe lên, lộn vòng
        public void BlastUp(Vector3 at)
        {
            if (sinking) return;
            rb.constraints = RigidbodyConstraints.None;
            var away = transform.position - at; away.y = 0;
            if (away.sqrMagnitude < 0.01f) away = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            away.Normalize();
            rb.linearVelocity = rb.linearVelocity * 0.3f + Vector3.up * 8.5f + away * 3f;
            rb.angularVelocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 7f;
        }

        public void Crash(Vector3 from, string reason, float power)
        {
            if (Crashed || sinking) return;
            if (Shielded) { ShieldBump(from, power * 2.5f); return; }       // 21.312: khiên miễn nhiễm mọi va chạm/đạn
            Crashed = true; frozen = true; CrashReason = reason; crashShownAt = Time.time; Crashes++; jumping = false; Speed = 0;
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
        public Vector3 WaterEntryVelocity { get; private set; }               // 21.365: nước văng (OpenWorldWaterSplash)
        public Vector3 WaterEntryPoint { get; private set; }
        // 21.257: OpenWorldGame tắt — rơi sông là kết thúc chuyến (luật cũ); HUD km/h do thanh HUD vòng chơi vẽ.
        [System.NonSerialized] public bool respawnAfterSink = true, showSpeed = true;
        // 21.254: CSGT bắt → khóa điều khiển (phanh tới đứng yên); chơi lại → đưa xe về vị trí xuất phát.
        [System.NonSerialized] public bool frozen;
        // 21.299: lưới điện CSGT (OpenWorldPoliceNet) — khóa lái/ga/nhảy, phanh gấp về 0 trong thời gian dính lưới.
        float netUntil;
        public bool Netted => Time.time < netUntil;
        public float NetRemaining => Mathf.Max(0f, netUntil - Time.time);
        public int NetHits { get; private set; }
        public void Net(float seconds) { if (Crashed || sinking || Shielded) return; netUntil = Time.time + seconds; NetHits++; }
        public void ResetTo(Vector3 pos, Quaternion rot)
        {
            sinking = false; rb.useGravity = true; Speed = 0; frozen = false;
            Crashed = false; CrashReason = null; jumping = false; jumpRequest = false; jumpRequestAt = -10f; joltUntil = 0f;
            stunUntil = stopUntil = flingUntil = netUntil = 0f; ExplodeAt = -1f; LastCrashInfo = null; steerVal = 0f; moveDir = Vector3.zero; yawRateNow = 0f; bodyRoll = 0f; yawRate = 0f; latUse = 0f; RearSlip = FrontSlip = 0f;
            if (dent != null) dent.Restore();
            cruise = false; lastGasTapAt = -10f;                               // 21.327
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            if (smoke) smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (burn) { Destroy(burn.gameObject); burn = null; }               // 21.312
            if (stunSmoke) stunSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); stunSmokeUntil = 0f; LastHit = null;
            rb.position = pos; rb.rotation = rot; transform.SetPositionAndRotation(pos, rot);
            rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; spawnTime = Time.time;
            RestoreUnstick(); perchTime = 0f; otherSupport = null; knockFall = false;   // 21.366
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
                if (npc.owner.IsWreck(npc.index) && !npc.owner.WreckMoving(npc.index) && npc.owner.WreckAge(npc.index) > 0.25f)
                {   // 21.325: tông xác xe nằm yên như vật cản → xe người chơi móp/bể chỗ va
                    var cw = c.GetContact(0); float iw = Mathf.Abs(Vector3.Dot(c.relativeVelocity, cw.normal));
                    if (iw > ObstacleDentMin && cw.normal.y < 0.55f && !Shielded) { Dent(cw.point, iw); ObstacleDents++; }
                    if (!AirKnock(cw.normal, iw)) Speed *= 0.25f;                 // 21.366
                    return;
                }
                HitVehicle(c, npc.owner.Mass(npc.index));
                return;
            }
            var hz = c.collider ? c.collider.GetComponentInParent<OpenWorldHazardBody>() : null;
            if (hz) { LastCrashInfo = "va collider " + c.collider.name + " tại " + c.GetContact(0).point.ToString("F1"); Crash(c.GetContact(0).point, hz.reason, 6f); return; }
            // 21.324: tông xe CSGT → cả hai móp/bể chỗ va. 21.325 (người dùng: «xe người chơi đụng cảnh quan, đồ vật, chướng
            // ngại cũng biến dạng, bể, móp méo ở chỗ va — chỉ xe người chơi»): MỌI vật cản (tường nhà, cột đèn/biển, cây, bồn
            // cây, lan can, ghế, xác xe, vật rơi…) với vận tốc đâm vào > 3 m/s (~11 km/h) → xe người chơi móp/bể theo vận tốc
            // đó; vật cản không đổi. Bỏ mặt đường/mặt cầu và chạm từ trên xuống (đáp xuống nóc vật). Khiên chặn.
            var ct0 = c.GetContact(0);
            var pol = c.rigidbody ? c.rigidbody.GetComponent<OpenWorldPolice>() : null;
            float into = Mathf.Abs(Vector3.Dot(c.relativeVelocity, ct0.normal));
            if (!IsRoad(c.collider) && ct0.normal.y < 0.55f && (pol ? into > 4f : into > ObstacleDentMin))
            {
                if (!Shielded && !Crashed) { Dent(ct0.point, pol ? c.relativeVelocity.magnitude : into); if (pol) WallDents++; else ObstacleDents++; }
                if (pol) pol.DamageAt(ct0.point, c.relativeVelocity.magnitude);
            }
            // 21.366: đang trên không → rơi xuống ngay (không treo trên thân cây/cột/tường)
            if (!IsRoad(c.collider) && AirKnock(ct0.normal, into)) return;
            // Đâm nhà/cây/cột: mất phần lớn tốc độ (giữ cảm giác va chạm như game cũ).
            if (Vector3.Dot(ct0.normal, transform.forward) < -0.5f) Speed *= 0.25f;
        }

        // «TRÚNG ĐẠN TÀU» → «Trúng đạn tàu» (giữ chữ hoa của tên riêng viết tắt như CSGT).
        public static string SentenceCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return "Xe hỏng";
            var words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i];
                if (w == "CSGT") continue;
                w = w.ToLowerInvariant();
                if (i == 0 && w.Length > 0) w = char.ToUpperInvariant(w[0]) + w.Substring(1);
                words[i] = w;
            }
            return string.Join(" ", words);
        }

        void OnGUI()
        {
            var st = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, Screen.height / 28), fontStyle = FontStyle.Bold };
            st.normal.textColor = Color.white;
            if (showSpeed) GUI.Label(new Rect(20, 16, 400, 60), Mathf.RoundToInt(Mathf.Abs(Speed) * 3.6f) + " km/h", st);
            if (sinking && respawnAfterSink) GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 3, 400, 60), "Rơi xuống sông!", st);
            // 21.291: thông báo dạng thẻ (OpenWorldToast) thay chữ to có bóng.
            if (Stunned && !Crashed)
                OpenWorldToast.Show("CHOÁNG", "Xe bị húc choáng", "Lái lại được sau " + StunRemaining.ToString("0.0") + " s", OpenWorldToast.Kind.Warn,
                                    Time.time - stunAt, StunRemaining, progress: StunRemaining / stunTotal);
            if (Crashed && Time.time - crashShownAt < 1.75f)                 // OpenWorldGame mở thẻ KẾT THÚC sau 1,6 s
                OpenWorldToast.Show("XE HỎNG", SentenceCase(CrashReason), "Chuyến xe dừng tại đây", OpenWorldToast.Kind.Danger,
                                    Time.time - crashShownAt, 1.75f - (Time.time - crashShownAt), center: true);
            else if (!frozen)
            {
                // Nút NHẢY (cảm ứng: chỉ nút này; bàn phím: Space từ 21.375), mờ khi đang hồi.
                LoadTouchArt();
                var b = JumpRect; float sz = b.width;
                bool ready = !jumping && Time.time - jumpAt >= JumpCooldown;
                bool held = false; foreach (var k in fingers.Values) if (k == Ctl.Jump) held = true;
                DrawArtButton(b, jumpTex, new Color(.96f, .76f, .29f), held, ready);
                if (!TouchControlsVisible) KeyBadge(b, "SPACE");
            }
            DrawTouchControls();
            if (cruise && !TouchControlsVisible && !frozen && !Crashed) DrawCruiseChip();
        }

        // 21.327: nút Ga khi đang tự động — quầng xanh lá nhịp thở, ảnh Ga ánh xanh, viền xanh, nhãn «TỰ ĐỘNG» trong nút.
        static readonly Color CruiseGreen = new Color(.2f, .92f, .45f);
        void DrawCruiseGas(Rect b)
        {
            float pulse = .5f + .5f * Mathf.Sin(Time.unscaledTime * 4.2f);
            Disc(new Rect(b.x - b.width * .1f, b.y - b.width * .1f, b.width * 1.2f, b.height * 1.2f), new Color(CruiseGreen.r, CruiseGreen.g, CruiseGreen.b, .22f + .22f * pulse));
            Disc(new Rect(b.x - b.width * .03f, b.y - b.width * .03f, b.width * 1.06f, b.height * 1.06f), new Color(CruiseGreen.r, CruiseGreen.g, CruiseGreen.b, .85f), b.width * .045f);
            bool held = GasHeld; var q = held ? new Rect(b.x + b.width * .035f, b.y + b.height * .035f, b.width * .93f, b.height * .93f) : b;
            if (gasTex) GUI.DrawTexture(q, gasTex, ScaleMode.ScaleToFit, true, 0, new Color(.72f, 1f, .8f, 1f), 0, 0);
            // nhãn «TỰ ĐỘNG» nằm TRONG nút (phía trên, đè lên icon bàn đạp) — không lấn nút Nhảy phía trên / mép màn hình
            float h = b.height * .2f, w = b.width * .8f; var pill = new Rect(b.center.x - w / 2, b.y + b.height * .07f, w, h);
            Bar(pill, new Color(.04f, .2f, .1f, .94f), h * .5f);
            Bar(pill, new Color(CruiseGreen.r, CruiseGreen.g, CruiseGreen.b, .3f + .25f * pulse), h * .5f);
            var st = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(h * .55f), wordWrap = false };
            st.normal.textColor = Color.white;
            GUI.Label(pill, "TỰ ĐỘNG", st);
        }
        // Máy tính: thẻ nhỏ bên trái nút Nhảy.
        void DrawCruiseChip()
        {
            var b = JumpRect; float h = b.height * .3f;
            var st = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(h * .42f), wordWrap = false };
            st.normal.textColor = new Color(.86f, 1f, .9f);
            string text = "GA TỰ ĐỘNG " + Mathf.RoundToInt(cruiseSpeed * 3.6f) + " km/h · ↑↑ tắt";
            float w = st.CalcSize(new GUIContent(text)).x + h * 1.7f;
            float left = shieldRef ? ShieldRect.x : b.x;   // 21.383: nút KHIÊN máy tính giờ nằm bên trái nút Nhảy → thẻ ra bên trái KHIÊN
            var r = new Rect(left - w - b.width * .12f, b.center.y - h / 2, w, h);   // bên trái nút Nhảy/Khiên (hàng Lazer/Tên lửa phía trên)
            float pulse = .5f + .5f * Mathf.Sin(Time.unscaledTime * 4.2f);
            Bar(r, new Color(.04f, .16f, .09f, .92f), h * .5f);
            Bar(new Rect(r.x + h * .2f, r.y + h * .2f, h * .6f, h * .6f), new Color(CruiseGreen.r, CruiseGreen.g, CruiseGreen.b, .6f + .4f * pulse), h * .3f);
            GUI.Label(new Rect(r.x + h * 1.0f, r.y, r.width - h * 1.1f, r.height), text, st);
        }
    }
}
