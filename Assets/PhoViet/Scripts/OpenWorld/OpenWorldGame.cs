using System.Collections.Generic;
using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet.OpenWorld
{
    // Bản đồ mở — vòng chơi, điểm, thua, menu (21.257, đợt 7 phần 4). Giữ như game cũ (mục 2.1, PhoVietGame.OnGUI):
    // • Màn chào (Ready): thương hiệu PHỐ VIỆT, thẻ «LÊN XE THÔI» (Enter/Space), kỷ lục; xe đứng yên ở điểm xuất phát,
    //   giao thông vẫn chạy, CSGT và hiểm họa tạm nghỉ.
    // • Đang chạy: thanh HUD trên cùng — tên phố đang đi (có dấu), số mét (= quãng đường lái, OpenWorldHazards.DistanceDriven:
    //   không cộng khi xe hỏng/bị bắt/chìm), km/h, KỶ LỤC, khoảng cách CSGT, nút tạm dừng (P/Esc) và nút loa bật/tắt tiếng (21.295, vẽ bằng hình).
    // • Tạm dừng: «NGHỈ MỘT CHÚT» — ĐI TIẾP / VỀ ĐẦU PHỐ (Time.timeScale = 0, tắt tiếng tạm thời).
    // • Thua (luật cũ): xe hỏng hoặc bị chặn → CSGT tới bắt → thẻ phòng giam «ĐÃ BỊ BẮT» trên ảnh Jail; rơi xuống sông →
    //   «KẾT THÚC CHUYẾN XE · Rơi xuống sông!» (không hồi sinh). Cả hai lưu kỷ lục cục bộ (PlayerPrefs, khóa riêng của bản
    //   đồ mở) và có THỬ MỘT CHUYẾN NỮA / CHƠI LẠI (R, Enter, Space) và VỀ ĐẦU PHỐ.
    [DefaultExecutionOrder(120)]
    public partial class OpenWorldGame : MonoBehaviour
    {
        public OpenWorldDriver player;
        public OpenWorldPolice police;
        public OpenWorldHazards hazards;
        public OpenWorldTraffic traffic;
        public Font font;
        public Texture2D jailWide, jailTall, missileIcon;

        public enum State { Ready, Riding, Paused, Jailed, GameOver }
        public State Mode { get; private set; }
        public int Best { get; private set; }
        public bool NewBest { get; private set; }
        public float Score => hazards ? hazards.DistanceDriven : 0f;
        public int Runs { get; private set; }
        // 21.396: chế độ TẬP LUYỆN (nút dưới LÊN XE THÔI) — cùng bản đồ, hiểm họa thưa (OpenWorldHazards.Practice.cs), không hiện/đếm
        // số mét, không lưu kỷ lục, không gửi bảng xếp hạng; trang web tính giờ chơi tập luyện riêng (account.js plays.practice).
        // Chơi lại / THỬ LẠI giữ chế độ đang chơi; VỀ ĐẦU PHỐ về chế độ thật.
        public bool Practice { get; private set; }
        public string EndReason { get; private set; }
        public const string BestKey = "PhoViet.OpenWorld.BestMetres.v1", SoundKey = "PhoViet.OpenWorld.Sound.v1";
        public bool Sound { get; private set; } = true;

        float sinkAt = -1f, crashAt = -1f;
        public string EndDetail { get; private set; }

        // 21.259: mọi lần xe hỏng (tên lửa, đạn pháo, khủng long, xác trực thăng, đâm xe tăng, nổ xe bị húc, húc ở tốc độ tối đa)
        // là kết thúc chuyến ngay — không chờ CSGT. Tiêu đề + dòng phụ cho thẻ «KẾT THÚC CHUYẾN XE».
        static (string title, string detail) CrashText(string reason)
        {
            switch (reason)
            {
                case "TRÚNG TÊN LỬA TRỰC THĂNG": return ("Trúng tên lửa!", "Tên lửa do trực thăng phóng.");
                case "TRÚNG ĐẠN XE TĂNG": return ("Trúng đạn xe tăng!", "Đạn pháo nổ ngay cạnh xe.");
                case "KHỦNG LONG QUẤT ĐUÔI": return ("Khủng long quất đuôi!", "Lần sau nhảy lên để qua đuôi.");
                case "KHỦNG LONG HÚC": return ("Khủng long hất văng!", "Tránh xa thân và chân khủng long.");
                case "TRỰC THĂNG RƠI TRÚNG": return ("Trực thăng rơi trúng xe!", "Xác trực thăng nổ ngay cạnh xe.");
                case "ĐÂM XE TĂNG": return ("Đâm vào xe tăng!", "Thép xe tăng không nhường đường.");
                case "XE BỊ HÚC NỔ TUNG": return ("Bị cuốn vào vụ nổ!", "Xe vừa bị húc phát nổ ngay cạnh.");
                case "HÚC XE Ở TỐC ĐỘ TỐI ĐA": return ("Xe nổ tung!", "Va chạm từ 90 km/h với xe không nhỏ hơn.");
                case "SÉT ĐÁNH TRÚNG": return ("Sét đánh trúng xe!", "Né vòng xanh dưới đất hoặc bật KHIÊN.");   // 21.317
                default: return ("Chuyến đi kết thúc!", reason);
            }
        }

        // Tên phố có dấu theo id tuyến trong layout.json.
        static readonly Dictionary<string, string> StreetNames = new Dictionary<string, string>
        {
            { "R1", "Đại lộ Lê Lợi" }, { "R2", "Tôn Đức Thắng" }, { "R3", "Bạch Đằng" }, { "R4", "Hai Bà Trưng" },
            { "L1", "Lý Thường Kiệt" }, { "L2", "Trần Hưng Đạo" }, { "L3", "Quang Trung" }, { "L4", "Lý Nam Đế" },
            { "L5", "Đinh Bộ Lĩnh" }, { "L6", "Nguyễn Trãi" }, { "L7", "Pasteur" }, { "L8", "Đồng Khởi" },
            { "P1", "Vành đai Tây" }, { "P2", "Vành đai Nam" }, { "E1", "Ven sông Thủ Thiêm" }, { "E2", "Mai Chí Thọ" },
            { "E3", "Vành đai Đông" }, { "E4", "Vành đai Nam bờ Đông" }, { "N1", "Ven sông Bắc" }, { "N2", "Vành đai Bắc" },
            { "N3", "Vành đai Tây Bắc" },
        };

        // ------------------------------------------------------------ 21.264: bản web (khung PWA app.js) + điện thoại
        // app.js gọi SendMessage("VongChoi", …): PauseForBrowser (ẩn tab/mở menu), ApplyBrowserOrientation ("1" = đang cầm dọc,
        // khung trang che bằng lời nhắc xoay ngang), ApplyBrowserInsets (vùng tai thỏ/thanh home, tỉ lệ 0–0,4 theo khung canvas).
        [System.Serializable] class BrowserInsetData { public float left, top, right, bottom, pixelRatio, nativeRatio; }
        static Vector4 insetFraction;
        public static float InsetLeft => insetFraction.x * Screen.width;
        public static float InsetTop => insetFraction.y * Screen.height;
        public static float InsetRight => insetFraction.z * Screen.width;
        public static float InsetBottom => insetFraction.w * Screen.height;
        public static bool MobileProfile { get; private set; }
        // 21.265: vùng an toàn thực = lớn hơn giữa Screen.safeArea (Simulator/app gốc) và vùng khung trang web báo về (iPhone X trở lên).
        public static float SafeLeftPx => Mathf.Max(Screen.safeArea.xMin, InsetLeft);
        public static float SafeRightPx => Mathf.Max(Screen.width - Screen.safeArea.xMax, InsetRight);
        public static float SafeBottomPx => Mathf.Max(Screen.safeArea.yMin, InsetBottom);
        public static float SafeTopPx => Mathf.Max(Screen.height - Screen.safeArea.yMax, InsetTop);
        float SL => SafeLeftPx / uiScale; float SR => SafeRightPx / uiScale; float ST => SafeTopPx / uiScale; float SB => SafeBottomPx / uiScale;
        public void PauseForBrowser() { if (Mode == State.Riding) TogglePause(); }
        public void ApplyBrowserOrientation(string blocked) { if (blocked == "1" && Mode == State.Riding) TogglePause(); }
        public void ApplyBrowserInsets(string json)
        {
            try
            {
                var d = JsonUtility.FromJson<BrowserInsetData>(json);
                if (d != null)
                {
                    insetFraction = new Vector4(Mathf.Clamp(d.left, 0f, .4f), Mathf.Clamp(d.top, 0f, .4f), Mathf.Clamp(d.right, 0f, .4f), Mathf.Clamp(d.bottom, 0f, .4f));
                    OnBrowserRatios(d.pixelRatio, d.nativeRatio);           // 21.291: cấu hình đồ họa (OpenWorldGame.Graphics.cs)
                }
            }
            catch (System.Exception) { }
        }

        // 21.282: công tắc CHẨN ĐOÁN qua link (?diag=a,b,...) — chỉ để tìm nguyên nhân Safari iPhone đóng game; bản mặc định
        // (không có ?diag) giữ nguyên toàn bộ nội dung. noinst = tắt GPU instancing, noshadow = tắt bóng, near = tầm nhìn 350 m,
        // noint = ẩn nội thất, notraffic = không xe NPC, noboats = không tàu, noriver = tắt phản chiếu sông, nopolice = không CSGT.
        public static string Diag { get; private set; } = "";
        static bool Has(string k) => k == "inst" ? System.Array.IndexOf(Diag.Split(','), "inst") >= 0 : Diag.Contains(k);
        void ApplyDiagnostics()
        {
            string url = Application.absoluteURL ?? ""; int i = url.IndexOf("diag=");
            if (i >= 0) Diag = url.Substring(i + 5).Split('&', '#')[0].ToLowerInvariant();
            // Mặc định trên điện thoại: TẮT GPU instancing. Đo WebGL: instancing cần ~500–700 bộ đệm UBO 8 KB mỗi khung, Unity giữ
            // kho ~3.000–4.600 bộ đệm cho các khung đang chờ (phình trong 15–20 s đầu, đúng lúc Safari iPhone đóng game); tắt đi
            // thì không còn kho này và còn nhanh hơn (Chrome: 28 → 32 FPS). Hình ảnh không đổi. ?diag=inst bật lại để so sánh.
            if (UnityEngine.Device.Application.isMobilePlatform && !Has("inst") && !Diag.Contains("noinst")) Diag = Diag.Length > 0 ? Diag + ",noinst" : "noinst";
            if (Diag.Length == 0) return;
            if (Has("all")) Diag = "noinst,noshadow,near,noint,notraffic,noboats,noriver,nopolice";
            if (Has("noinst")) foreach (var m in Resources.FindObjectsOfTypeAll<Material>()) if (m) m.enableInstancing = false;
            if (Has("noshadow")) QualitySettings.shadows = ShadowQuality.Disable;
            if (Has("noint")) foreach (var lg in FindObjectsByType<LODGroup>(FindObjectsInactive.Exclude)) if (lg.name == "NoiThat") lg.gameObject.SetActive(false);
            var tr = traffic ? traffic : FindAnyObjectByType<OpenWorldTraffic>();
            if (Has("notraffic") && tr) { tr.carCount = 0; tr.bikeCount = 0; }
            if (Has("noboats")) { var b = FindAnyObjectByType<OpenWorldBoats>(); if (b) b.gameObject.SetActive(false); }
            if (Has("noriver")) { var rr = FindAnyObjectByType<OpenWorldRiverReflections>(); if (rr) rr.enabled = false; }
            if (Has("nopolice")) { var po = police ? police : FindAnyObjectByType<OpenWorldPolice>(); if (po) po.gameObject.SetActive(false); }
            Debug.Log("[Phố Việt] chẩn đoán: " + Diag);
        }

        void Awake()
        {
            ApplyDiagnostics();
            // Điện thoại/máy tính bảng (trình duyệt di động): bóng ngắn hơn, LOD sớm hơn, bớt xe NPC (đặt trước Start của giao thông).
            if (!UnityEngine.Device.Application.isMobilePlatform) return;
            MobileProfile = true;
            QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance, 45f);
            QualitySettings.lodBias *= 0.75f;
            QualitySettings.antiAliasing = 0;
            Application.targetFrameRate = 60;
            var tr = traffic ? traffic : FindAnyObjectByType<OpenWorldTraffic>();
            if (tr) { tr.carCount = Mathf.Min(tr.carCount, 24); tr.bikeCount = Mathf.Min(tr.bikeCount, 18); }
        }

        void Start()
        {
            if (Has("near")) foreach (var c in FindObjectsByType<Camera>(FindObjectsInactive.Exclude)) c.farClipPlane = Mathf.Min(c.farClipPlane, 350f);
            if (!hazards) hazards = FindAnyObjectByType<OpenWorldHazards>();
            if (!police) police = FindAnyObjectByType<OpenWorldPolice>();
            if (!traffic) traffic = FindAnyObjectByType<OpenWorldTraffic>();
            if (player && !player.GetComponent<OpenWorldWaterSplash>()) player.gameObject.AddComponent<OpenWorldWaterSplash>();   // 21.365: nước văng khi rơi sông/biển
            Best = PlayerPrefs.GetInt(BestKey, 0);
            Sound = PlayerPrefs.GetInt(SoundKey, 1) == 1; AudioListener.volume = Sound ? 1f : 0f;
            if (!jailWide) jailWide = Resources.Load<Texture2D>("Jail/JailWide");
            if (!jailTall) jailTall = Resources.Load<Texture2D>("Jail/JailTall");
            if (!missileIcon) missileIcon = Resources.Load<Texture2D>("PlayerMissile/PV_MissileHud");
            if (player) { player.respawnAfterSink = false; player.showSpeed = false; }
            if (police) { police.drawJail = false; police.showDistance = false; }
            if (player) { laser = player.GetComponent<OpenWorldLaser>(); if (!laser) laser = player.gameObject.AddComponent<OpenWorldLaser>(); }   // 21.300
            if (player) { shield = player.GetComponent<OpenWorldShield>(); if (!shield) shield = player.gameObject.AddComponent<OpenWorldShield>(); }   // 21.312
            if (player && !player.GetComponent<OpenWorldTireFx>()) player.gameObject.AddComponent<OpenWorldTireFx>();   // 21.316g: khói lốp + tiếng rít
            weather = GetComponent<OpenWorldWeather>(); if (!weather) weather = gameObject.AddComponent<OpenWorldWeather>();   // 21.317: mưa giông
            GoReady();
            InitGraphics();                                                 // 21.291: mức đồ họa đã lưu (sau thiết lập điện thoại ở Awake)
        }

        // ------------------------------------------------------------ chuyển trạng thái
        // 21.317: thời tiết tung lại (50 % mưa giông) khi mở game, VỀ ĐẦU PHỐ và CHƠI LẠI / THỬ MỘT CHUYẾN NỮA; lần đầu hiện ngay,
        // các lần sau chuyển nhanh 2 s. Bấm LÊN XE THÔI ở màn chào giữ thời tiết đang có, chỉ tính lại 2 phút mưa.
        OpenWorldWeather weather; bool weatherRolled;
        public OpenWorldWeather Weather => weather;

        public void GoReady()
        {
            Resume();
            if (weather) { weather.Roll(!weatherRolled); weatherRolled = true; }
            ClearSquad();                                                   // 21.301
            if (police) { police.enabled = true; police.ResetChase(true); police.Halt(); police.enabled = false; }
            if (hazards) { hazards.ClearAll(); hazards.enabled = false; }
            if (player) player.frozen = true;
            ReportReadyToWeb();                                             // 21.396: trang web thôi tính giờ tập luyện
            Practice = false;
            Mode = State.Ready; EndReason = null; EndDetail = null; NewBest = false; sinkAt = -1f; crashAt = -1f;
        }

        /// 21.396: nút TẬP LUYỆN (phím T ở màn chào).
        public void StartPractice() { Practice = true; StartRun(); }
        /// Nút LÊN XE THÔI: luôn là lượt thật (tính mét + bảng xếp hạng).
        public void StartRealRun() { Practice = false; StartRun(); }

        /// Lượt mới theo chế độ đang chọn (Practice).
        public void StartRun()
        {
            Resume();
            ReportRunStartToWeb(Practice);                                          // 21.375: trang tắt thông báo xếp hạng khi đang chơi
            if (weather)
            {
                if (Mode == State.GameOver || Mode == State.Jailed || !weatherRolled) { weather.Roll(!weatherRolled); weatherRolled = true; }
                weather.BeginRun();
            }
            if (hazards) { hazards.practice = Practice; hazards.ClearAll(); hazards.enabled = true; }   // số mét về 0 ngay khung hình này
            ClearSquad();                                                   // 21.301: xóa xe CSGT tăng viện của lượt trước
            if (police) { police.enabled = true; police.ResetChase(true); }   // đưa xe về chỗ xuất phát, Runs++ → hiểm họa dọn sạch
            if (player) player.frozen = false;
            if (laser) laser.ResetLaser();
            if (shield) shield.ResetShield();                               // 21.312
            Mode = State.Riding; EndReason = null; EndDetail = null; NewBest = false; sinkAt = -1f; crashAt = -1f; Runs++; GraphicsOpen = false;
        }

        public void TogglePause()
        {
            if (Mode == State.Riding) { Mode = State.Paused; Time.timeScale = 0f; AudioListener.pause = true; }
            else if (Mode == State.Paused) { Mode = State.Riding; GraphicsOpen = false; Resume(); }
        }
        void Resume() { Time.timeScale = 1f; AudioListener.pause = false; }

        void EndRun(State end, string reason)
        {
            Mode = end; EndReason = reason;
            if (Practice) ReportPracticeEndToWeb();                          // 21.396: tập luyện không kỷ lục, không bảng xếp hạng
            else
            {
                int m = Mathf.FloorToInt(Score);
                if (m > Best) { Best = m; NewBest = true; PlayerPrefs.SetInt(BestKey, Best); PlayerPrefs.Save(); }
                ReportRunToWeb(m);                                          // 21.356: bảng xếp hạng chung (OpenWorldGame.Leaderboard.cs)
            }
            if (hazards) hazards.enabled = false;
            if (player) player.frozen = true;                               // ẩn nút NHẢY, khóa lái
            if (end == State.GameOver && police) { police.Halt(); police.enabled = false; }
            if (end == State.GameOver) HaltSquad();
        }

        public void SetSound(bool on)
        {
            Sound = on; AudioListener.volume = on ? 1f : 0f;
            PlayerPrefs.SetInt(SoundKey, on ? 1 : 0); PlayerPrefs.Save();
        }

        void Update()
        {
            switch (Mode)
            {
                case State.Ready:
                    if (GraphicsOpen) { GraphicsKeys(); break; }
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) StartRealRun();
                    else if (Input.GetKeyDown(KeyCode.T)) StartPractice();   // 21.396
                    break;
                case State.Riding:
                    TickGovernor();
                    if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) { TogglePause(); break; }
                    TickSquad();
                    if ((police && police.Jailed) || SquadJailed()) { EndRun(State.Jailed, "CSGT đã đưa bạn vào phòng giam."); break; }
                    if (player && player.Crashed)
                    {
                        if (crashAt < 0) crashAt = Time.time;
                        else if (Time.time - crashAt > 1.6f)
                        {
                            var ct = CrashText(player.CrashReason);
                            EndRun(State.GameOver, ct.title); EndDetail = ct.detail; break;
                        }
                    }
                    if (player && player.Sinking)
                    {
                        if (sinkAt < 0) sinkAt = Time.time;
                        // 21.365: 1,4 → 2,4 s — chờ nước văng (1,2 s), cột nước và sóng chấn động hiện rõ rồi mới hiện thẻ kết thúc
                        else if (Time.time - sinkAt > 2.4f) EndRun(State.GameOver, "Rơi xuống sông!");
                    }
                    break;
                case State.Paused:
                    if (GraphicsOpen) { GraphicsKeys(); break; }
                    if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) TogglePause();
                    break;
                case State.Jailed:
                case State.GameOver:
                    if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) StartRun();
                    break;
            }
        }

        // Tên phố gần nhất với xe.
        public string CurrentStreet
        {
            get
            {
                if (!traffic || traffic.roads == null || !player) return "GờTa Việt";
                var p = new Vector2(player.transform.position.x, player.transform.position.z);
                float best = float.MaxValue; string id = null;
                foreach (var r in traffic.roads)
                {
                    var ab = r.b - r.a; float t = Mathf.Clamp01(Vector2.Dot(p - r.a, ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
                    float d = (r.a + ab * t - p).sqrMagnitude;
                    if (d < best) { best = d; id = r.id; }
                }
                return id != null && StreetNames.TryGetValue(id, out var n) ? n : "GờTa Việt";
            }
        }

        // ------------------------------------------------------------ IMGUI theo kiểu game cũ
        OpenWorldLaser laser;                                              // 21.300
        OpenWorldShield shield;                                            // 21.312
        GUIStyle label, button; bool portrait; float uiScale = 1, uiW, uiH;
        static readonly Color Glass = new Color(.07f, .19f, .21f, .64f), WelcomeWhite = new Color(.97f, .985f, 1, .93f), WelcomeGlass = new Color(.97f, .985f, 1, .82f);
        Texture2D star;

        void InitStyles()
        {
            if (label != null) return;
            if (font) GUI.skin.font = font;                                 // chữ HUD của CSGT/hiểm họa/xe cũng dùng font tiếng Việt
            label = new GUIStyle { font = font, wordWrap = true }; label.normal.textColor = Ink;
            button = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter }; button.normal.textColor = Cream; button.hover.textColor = Gold;
        }
        void Panel(Rect r, Color c, float radius = 12) { GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, c, 0, radius); }
        void Txt(string v, float x, float y, float w, float h, int size, Color c, TextAnchor a = TextAnchor.UpperLeft)
        { label.fontSize = size; label.normal.textColor = c; label.alignment = a; GUI.Label(new Rect(x, y, w, h), v, label); }
        bool Btn(string v, Rect r, Color bg, Color fg, int size = 17)
        { Panel(r, bg); button.fontSize = size; button.normal.textColor = fg; return GUI.Button(r, v, button); }
        Rect Fullscreen() => new Rect(0, 0, uiW, uiH);
        static string Metres(float m) => Mathf.Max(0, Mathf.FloorToInt(m)).ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".") + " m";

        void OnGUI()
        {
            GUI.depth = -20;                                                // thẻ menu nằm trên mọi HUD khác
            InitStyles();
            portrait = Screen.width < Screen.height;
            uiScale = portrait ? Screen.width / 480f : Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            uiScale = Mathf.Max(0.35f, uiScale);
            uiW = Screen.width / uiScale; uiH = Screen.height / uiScale;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * uiScale);
            switch (Mode)
            {
                case State.Ready:
                    BeginMenu();
                    if (GraphicsOpen) { GUI.enabled = false; ReadyScreen(); GUI.enabled = true; GraphicsWindow(); }
                    else ReadyScreen();
                    EndMenu();
                    break;
                case State.Riding: Hud(); break;
                case State.Paused:                                          // 21.322: mở được cấu hình đồ họa khi tạm dừng
                    Hud(); BeginMenu();
                    if (GraphicsOpen) { GUI.enabled = false; EndCard(true); GUI.enabled = true; GraphicsWindow(); }
                    else EndCard(true);
                    EndMenu();
                    break;
                case State.Jailed: BeginMenu(); JailScreen(); EndMenu(); break;
                case State.GameOver: Hud(); BeginMenu(); EndCard(false); EndMenu(); break;
            }
            GUI.matrix = Matrix4x4.identity;
        }

        // 21.322: ReadyScreen/EndCard/JailScreen chuyển sang OpenWorldGame.Menus.cs (mẫu C «Tem đua» bản nhỏ).

        // 21.297 — thanh thông tin mẫu A (người dùng chọn 21/11/2026): một dải kính tối bo tròn 20 (giữ độ trong suốt Glass cũ),
        // viền sáng mảnh, to hơn ~12 % (760 × 76 → 850 × 86). Mỗi mục có biểu tượng vàng vẽ bằng mã (ghim địa điểm, cúp kỷ lục,
        // đèn CSGT, tên lửa); quãng đường số to + đơn vị nhỏ + ô tốc độ; kho tên lửa thành 10 vạch bo góc; nút loa/tạm dừng tròn.
        // 21.300: thêm hàng NĂNG LƯỢNG dưới tên lửa → cao 86 → 112 (cột khác canh giữa), hàng phụ máy dọc 40 → 66.
        public const float HudHeight = 112f, HudTopLandscape = 18f, HudTopPortrait = 12f, HudMissileRow = 66f;
        // 21.299: người dùng — thanh còn nhỏ, to thêm 10 % cả khung lẫn chữ → phóng cả thanh 1,1× quanh mép trên giữa (máy ngang).
        public const float HudScaleLandscape = 1.1f;
        static readonly Color HudLine = new Color(.99f, .95f, .81f, .18f), HudBtn = new Color(1, .945f, .812f, .88f);
        Texture2D pinTex, trophyTex, sirenTex, rocketTex, boltTex;

        void HudLabel(Texture2D icon, string text, float x, float y, float w)
        {
            if (icon) DrawIcon(new Rect(x, y + 1, 14, 14), icon, Gold);
            Txt(text, x + (icon ? 19 : 0), y, w, 17, 11, Gold);
        }

        static string MetresNum(float m) => Mathf.Max(0, Mathf.FloorToInt(m)).ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");

        // Số to + «m» nhỏ; trả về x ngay sau chữ «m».
        float BigMetres(float m, float x, float y, int size)
        {
            string n = MetresNum(m);
            label.fontSize = size; float nw = label.CalcSize(new GUIContent(n)).x;
            Txt(n, x, y, nw + 4, size + 10, size, Cream);
            int us = Mathf.RoundToInt(size * .5f); label.fontSize = us; float uw = label.CalcSize(new GUIContent("m")).x;
            Txt("m", x + nw + 2, y + size * .48f, uw + 4, us + 6, us, new Color(.99f, .95f, .81f, .85f));
            return x + nw + 2 + uw;
        }

        void Hud()
        {
            EnsureHudIcons();
            float bw = portrait ? uiW - 24 - SL - SR : 850, x = SL + (uiW - SL - SR - bw) / 2, y = (portrait ? HudTopPortrait : HudTopLandscape) + ST;
            float h = HudHeight;
            var hudMatrix = GUI.matrix;
            if (!portrait) GUIUtility.ScaleAroundPivot(Vector2.one * HudScaleLandscape, new Vector2(x + bw / 2, y) * uiScale);
            var bar = new Rect(x, y, bw, h);
            Panel(bar, Glass, 20);
            GUI.DrawTexture(bar, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(1, 1, 1, .2f), 1f, 20);

            // quãng đường + tốc độ (21.300: cột canh giữa theo chiều cao mới — lệch oy so với bố cục 86)
            float lx = x + 18, oy = portrait ? 0f : (h - 86f) * .5f;
            HudLabel(pinTex, CurrentStreet.ToUpperInvariant(), lx, y + oy + 14, (portrait ? bw * .4f : 210) - 20);
            // 21.396: tập luyện không đếm mét — ô quãng đường hiện nhãn TẬP LUYỆN
            float after = Practice ? PracticeTag(lx, y + oy + 34) : BigMetres(Score, lx, y + oy + 30, 32);
            float kmh = player ? Mathf.Abs(player.Speed) * 3.6f : 0f;
            string sp = Mathf.RoundToInt(kmh) + " km/h";
            label.fontSize = 11; float spw = label.CalcSize(new GUIContent(sp)).x + 16;
            var pill = portrait ? new Rect(lx, y + h - 22, spw, 18) : new Rect(Mathf.Max(after + 10, lx + 92), y + oy + 46, spw, 20);
            Panel(pill, new Color(.99f, .95f, .81f, .14f), pill.height * .5f);
            Txt(sp, pill.x, pill.y, pill.width, pill.height, 11, Cream, TextAnchor.MiddleCenter);

            // kỷ lục
            float c1 = x + (portrait ? bw * 0.42f : 250);
            Panel(new Rect(c1 - 17, y + 14, 1, h - 28), HudLine, 0);
            HudLabel(trophyTex, Practice ? "XẾP HẠNG" : "KỶ LỤC", c1, y + oy + 14, 130);
            if (Practice) Txt("Không tính", c1, y + oy + 41, 140, 30, 18, new Color(Cream.r, Cream.g, Cream.b, .75f));
            else Txt(Metres(Mathf.Max(Best, Score)), c1, y + oy + 38, 140, 30, 22, Cream);

            // CSGT
            var nearPol = NearestPolice();                                  // 21.301: xe gần nhất (gốc hoặc tăng viện)
            if (nearPol)
            {
                float c2 = x + (portrait ? bw * 0.68f : 405);
                bool flash = Mathf.FloorToInt(Time.unscaledTime * 4f) % 2 == 0;
                var col = nearPol.Distance < 15f ? (flash ? Hex("FF5A4D") : Hex("5FA8FF")) : Cream;
                Panel(new Rect(c2 - 17, y + 14, 1, h - 28), HudLine, 0);
                HudLabel(sirenTex, SquadCount > 0 ? "CSGT ×" + (SquadCount + 1) : "CSGT", c2, y + oy + 14, 100);
                Txt(Mathf.RoundToInt(nearPol.Distance) + " m", c2, y + oy + 38, 110, 30, 22, col);
            }

            // kho tên lửa: 10 vạch bo góc (còn = vàng, đã bắn = mờ)
            if (hazards)
            {
                float c3, ly, by, ey;
                if (portrait)
                {
                    var row = new Rect(x, y + h + 6, bw, HudMissileRow);
                    Panel(row, Glass, 16);
                    GUI.DrawTexture(row, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(1, 1, 1, .2f), 1f, 16);
                    c3 = x + 16; ly = row.y + 10; by = row.y + 10; ey = row.y + 38;
                }
                else
                {
                    c3 = x + 535; ly = y + 12; by = y + 32; ey = y + 56;
                    Panel(new Rect(c3 - 17, y + 14, 1, h - 28), HudLine, 0);
                }
                HudLabel(rocketTex, "TÊN LỬA  " + hazards.PlayerMissiles + "/" + hazards.maxMissiles, c3, ly, 150);
                float bx = portrait ? c3 + 160 : c3;
                for (int i = 0; i < hazards.maxMissiles; i++)
                    Panel(new Rect(bx + i * 14, by, 10, portrait ? 16 : 15), i < hazards.PlayerMissiles ? Gold : new Color(.99f, .95f, .81f, .2f), 3);
                if (hazards.PlayerMissiles < hazards.maxMissiles)
                {   // 21.301: vạch kế tiếp đầy dần theo đồng hồ hồi 12 s
                    float ph = (portrait ? 16 : 15) * hazards.MissileRegenProgress;
                    Panel(new Rect(bx + hazards.PlayerMissiles * 14, by + (portrait ? 16 : 15) - ph, 10, ph), new Color(Gold.r, Gold.g, Gold.b, .65f), 2);
                }
                EnergyRow(c3, ey, portrait);
            }
            // 21.295: font web không có «Ⅱ»/«▶» → vẽ biểu tượng bằng hình; nút loa bên trái nút tạm dừng (tắt tiếng: gạch chéo đỏ).
            // 21.297: nút tròn 44.
            const float bs = 44;
            var pauseRect = new Rect(x + bw - 16 - bs, y + (h - bs) / 2, bs, bs);
            if (RoundBtn(pauseRect)) TogglePause();
            DrawPauseIcon(pauseRect, Mode == State.Paused, Ink);
            if (!portrait)
            {
                var soundRect = new Rect(pauseRect.x - 10 - bs, pauseRect.y, bs, bs);
                if (RoundBtn(soundRect)) SetSound(!Sound);
                DrawSpeakerIcon(soundRect, Sound, Ink);
            }
            GUI.matrix = hudMatrix;
            FireButton();
            LaserButton();
            ShieldButton();
            SquadToast();
        }

        // 21.300: hàng «NĂNG LƯỢNG 100/100» (dưới tên lửa) — thanh xanh điện; đang bắn nhấp nháy; đang khóa hiện «HỒI n s» + vệt hồi.
        static readonly Color EnergyCol = new Color(.27f, .86f, 1f), EnergyTrack = new Color(.99f, .95f, .81f, .16f);
        void EnergyRow(float x0, float y0, bool row)
        {
            if (!laser) return;
            int e = Mathf.RoundToInt(laser.Energy);
            bool cool = laser.CooldownLeft > 0f;
            string t = cool ? "NĂNG LƯỢNG  " + e + "/100 · HỒI " + Mathf.CeilToInt(laser.CooldownLeft) + " s" : "NĂNG LƯỢNG  " + e + "/100";
            if (boltTex) DrawIcon(new Rect(x0, y0 + 1, 14, 14), boltTex, EnergyCol);
            Txt(t, x0 + 19, y0, 200, 17, 11, cool ? new Color(.99f, .95f, .81f, .75f) : EnergyCol);
            var track = row ? new Rect(x0 + 200, y0 + 2, 136, 12) : new Rect(x0, y0 + 20, 136, 12);
            Panel(track, EnergyTrack, 6);
            float k = Mathf.Clamp01(laser.Energy / OpenWorldLaser.MaxEnergy);
            if (k > 0.001f)
            {
                var fill = new Rect(track.x, track.y, Mathf.Max(12f, track.width * k), track.height);
                float pulse = laser.Firing ? .75f + .25f * Mathf.Sin(Time.unscaledTime * 30f) : 1f;
                Panel(fill, new Color(EnergyCol.r, EnergyCol.g, EnergyCol.b, pulse), 6);
                Panel(new Rect(fill.x + 3, fill.y + 2, fill.width - 6, 3), new Color(1, 1, 1, .35f * pulse), 2);
            }
            if (cool)
            {   // vệt hồi chạy dần tới đầy
                float r = 1f - laser.CooldownLeft / OpenWorldLaser.CooldownSeconds;
                Panel(new Rect(track.x, track.y + track.height - 3, track.width * r, 3), new Color(EnergyCol.r, EnergyCol.g, EnergyCol.b, .7f), 1.5f);
            }
        }

        // 21.300: nút LAZER (21.328: bấm 1 lần là bắn tới khi hết năng lượng) phía trên nút NHẢY; máy cảm ứng do OpenWorldDriver đọc đa điểm, chuột/phím W (21.383) ở đây.
        bool laserMouse;
        void LaserButton()
        {
            if (!laser || !player || player.frozen || player.Crashed || Mode != State.Riding) { if (laser) laser.mouseHeld = laserMouse = false; return; }
            OpenWorldDriver.EnsureTouchArt();
            var f = OpenWorldDriver.LaserRect; bool touch = player.TouchControlsVisible;
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity;
            bool ready = laser.Ready || laser.Firing;
            OpenWorldDriver.DrawArtButton(f, OpenWorldDriver.LaserButtonTex, new Color(.64f, .36f, 1f), laser.Firing, ready);
            if (laser.CooldownLeft > 0f)
            {   // khóa: số giây còn lại giữa nút
                float bs = f.width * .42f; var badge = new Rect(f.center.x - bs / 2, f.y + f.height * .22f, bs, bs);
                GUI.DrawTexture(badge, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(.06f, .1f, .16f, .85f), 0, bs * .5f);
                GUI.DrawTexture(badge, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(.64f, .36f, 1f), bs * .08f, bs * .5f);
                var st = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(bs * .5f) }; st.normal.textColor = Cream;
                GUI.Label(badge, Mathf.CeilToInt(laser.CooldownLeft).ToString(), st);
            }
            if (!touch) OpenWorldDriver.KeyBadge(f, "W");   // 21.383
            var ev = Event.current;
            if (!touch && ev.type == EventType.MouseDown && f.Contains(ev.mousePosition)) { laserMouse = true; ev.Use(); }
            GUI.matrix = m;
            if (!Input.GetMouseButton(0)) laserMouse = false;
            laser.mouseHeld = laserMouse;
        }

        // 21.312: nút KHIÊN (bấm 1 lần → khiên 6 s, hồi 10 s (21.313)) phía trên giữa 2 nút TRÁI/PHẢI; máy cảm ứng do OpenWorldDriver đọc
        // đa điểm, chuột/phím E (21.383; 21.375 ;) ở đây. Đang bật: nút sáng + quầng xanh ngọc + số giây còn lại; đang hồi: xám + số giây hồi.
        void ShieldButton()
        {
            if (!shield || !player || player.frozen || player.Crashed || Mode != State.Riding) return;
            OpenWorldDriver.EnsureTouchArt();
            var f = OpenWorldDriver.ShieldRect; bool touch = player.TouchControlsVisible;
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity;
            var acc = new Color(.2f, .94f, .82f);
            OpenWorldDriver.DrawArtButton(f, OpenWorldDriver.ShieldButtonTex, acc, shield.Active || player.ShieldHeld, shield.Ready || shield.Active);
            if (shield.Active || shield.CooldownLeft > 0f)
            {
                float left = shield.Active ? shield.ActiveLeft : shield.CooldownLeft;
                float bs = f.width * .42f; var badge = new Rect(f.center.x - bs / 2, f.y + f.height * .22f, bs, bs);
                GUI.DrawTexture(badge, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, shield.Active ? new Color(.02f, .3f, .27f, .9f) : new Color(.06f, .1f, .16f, .85f), 0, bs * .5f);
                GUI.DrawTexture(badge, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, acc, bs * .08f, bs * .5f);
                var st = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(bs * .5f) }; st.normal.textColor = Cream;
                GUI.Label(badge, Mathf.CeilToInt(left).ToString(), st);
            }
            if (!touch) OpenWorldDriver.KeyBadge(f, "E");   // 21.383
            var ev = Event.current;
            if (!touch && ev.type == EventType.MouseDown && f.Contains(ev.mousePosition)) { shield.requested = true; ev.Use(); }
            GUI.matrix = m;
        }

        bool RoundBtn(Rect r)
        {
            Panel(r, HudBtn, r.height * .5f);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        void EnsureHudIcons()
        {
            if (pinTex) return;
            // v: 0 = trên. Ghim địa điểm: đầu tròn có lỗ + mũi nhọn xuống.
            pinTex = MaskTex((u, v) =>
            {
                float dx = u - .5f, dy = v - .38f, d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < .3f && d > .12f) return true;
                return v > .5f && InTri(u, v, new Vector2(.24f, .5f), new Vector2(.76f, .5f), new Vector2(.5f, .96f)) && !(d < .12f);
            });
            // 21.300: tia sét năng lượng
            boltTex = MaskTex((u, v) => InTri(u, v, new Vector2(.62f, .04f), new Vector2(.22f, .56f), new Vector2(.52f, .56f))
                                     || InTri(u, v, new Vector2(.48f, .44f), new Vector2(.78f, .44f), new Vector2(.36f, .97f)));
            // Cúp: thân chén nửa elip, hai quai, cổ và đế.
            trophyTex = MaskTex((u, v) =>
            {
                if (v >= .1f && v <= .52f) { float t = (v - .1f) / .42f; if (Mathf.Abs(u - .5f) < .28f * Mathf.Sqrt(Mathf.Max(0, 1 - t * t))) return true; }
                float hx = u < .5f ? .25f : .75f, ddx = u - hx, ddy = v - .27f, hd = Mathf.Sqrt(ddx * ddx + ddy * ddy);
                if (hd > .09f && hd < .15f && (u < .5f ? u < .25f : u > .75f)) return true;
                if (Mathf.Abs(u - .5f) < .055f && v > .5f && v < .78f) return true;
                return u > .3f && u < .7f && v > .76f && v < .9f;
            });
            // Đèn CSGT: chóp tròn + đế + tia sáng.
            sirenTex = MaskTex((u, v) =>
            {
                float dx = u - .5f, dy = v - .62f;
                if (v <= .62f && v >= .3f && dx * dx + dy * dy < .29f * .29f) return true;
                if (u > .12f && u < .88f && v > .66f && v < .82f) return true;
                if (Mathf.Abs(u - .5f) < .045f && v > .04f && v < .2f) return true;
                float ax = Mathf.Abs(u - .5f);                                      // hai tia chéo
                return ax > .3f && ax < .44f && Mathf.Abs((v - .3f) + (ax - .3f) * .9f) < .045f;
            });
            // Tên lửa nghiêng 45°, mũi hướng lên phải: thân, mũi, hai cánh.
            rocketTex = MaskTex((u0, v0) =>
            {
                float cu = u0 - .5f, cv = v0 - .5f, c = .7071f;
                float u = .5f + c * (cu + cv), v = .5f + c * (cv - cu);
                if (Mathf.Abs(u - .5f) < .12f && v > .28f && v < .74f) return true;
                if (InTri(u, v, new Vector2(.38f, .29f), new Vector2(.62f, .29f), new Vector2(.5f, .02f))) return true;
                if (InTri(u, v, new Vector2(.38f, .56f), new Vector2(.38f, .8f), new Vector2(.2f, .84f))) return true;
                if (InTri(u, v, new Vector2(.62f, .56f), new Vector2(.62f, .8f), new Vector2(.8f, .84f))) return true;
                return Mathf.Abs(u - .5f) < .06f && v > .74f && v < .92f;
            });
        }

        // Nút TÊN LỬA phía trên nút NHẢY (góc phải dưới); bàn phím: Q (21.383; 21.375 K, trước F).
        void FireButton()
        {
            if (!hazards || !player || player.frozen || player.Crashed || Mode != State.Riding) return;
            // 21.265: vị trí theo bố cục nút cảm ứng của OpenWorldDriver (px → đơn vị GUI). Máy cảm ứng: chạm do OpenWorldDriver đọc
            // đa điểm (vừa lái vừa bắn); nút IMGUI chỉ còn cho chuột.
            var f = OpenWorldDriver.FireRect; var r = new Rect(f.x / uiScale, f.y / uiScale, f.width / uiScale, f.height / uiScale);
            bool ready = hazards.CanFire, touch = player.TouchControlsVisible;
            // 21.266: nút vẽ sẵn (PV_Btn_Missile) + huy hiệu số tên lửa; máy tính thêm chữ phím Q (21.383), bấm chuột vẫn bắn.
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity;
            OpenWorldDriver.DrawArtButton(f, OpenWorldDriver.MissileButtonTex, new Color(.91f, .27f, .23f), false, ready);
            float bs = f.width * .3f; var badge = new Rect(f.x - bs * .1f, f.y - bs * .1f, bs, bs);
            GUI.DrawTexture(badge, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, ready ? Gold : new Color(.55f, .58f, .62f), 0, bs * .5f);
            GUI.DrawTexture(badge, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Ink, bs * .1f, bs * .5f);
            var st = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(bs * .56f) }; st.normal.textColor = Ink;
            GUI.Label(badge, hazards.PlayerMissiles.ToString(), st);
            if (!touch) OpenWorldDriver.KeyBadge(f, "Q");   // 21.383
            bool click = !touch && Event.current.type == EventType.MouseDown && f.Contains(Event.current.mousePosition);
            GUI.matrix = m;
            if (click) { hazards.RequestFire(); Event.current.Use(); }
        }

        // 21.295: biểu tượng vẽ bằng texture tạo trong mã (font web thiếu glyph). Tô màu bằng GUI.color.
        Texture2D playTex, speakerTex, wavesTex, slashTex;
        static Texture2D MaskTex(System.Func<float, float, bool> inside, int size = 96)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[size * size];
            for (int yy = 0; yy < size; yy++)
                for (int xx = 0; xx < size; xx++)
                {
                    int hit = 0;                                                   // khử răng cưa 4×4
                    for (int sy = 0; sy < 4; sy++) for (int sx = 0; sx < 4; sx++)
                            if (inside((xx + (sx + .5f) / 4f) / size, 1f - (yy + (sy + .5f) / 4f) / size)) hit++;   // v: 0 = trên
                    px[yy * size + xx] = new Color32(255, 255, 255, (byte)(hit * 255 / 16));
                }
            t.SetPixels32(px); t.Apply(false, true); return t;
        }
        static bool InTri(float x, float y, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = (x - b.x) * (a.y - b.y) - (a.x - b.x) * (y - b.y), d2 = (x - c.x) * (b.y - c.y) - (b.x - c.x) * (y - c.y), d3 = (x - a.x) * (c.y - a.y) - (c.x - a.x) * (y - a.y);
            return !((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0));
        }
        void DrawIcon(Rect r, Texture2D t, Color c) { var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, t, ScaleMode.ScaleToFit); GUI.color = old; }
        Rect IconRect(Rect b, float s) { float h = b.height * s; return new Rect(b.center.x - h / 2, b.center.y - h / 2, h, h); }

        void DrawPauseIcon(Rect b, bool paused, Color c)
        {
            var ir = IconRect(b, .52f);
            if (paused)
            {
                if (!playTex) playTex = MaskTex((u, v) => InTri(u, v, new Vector2(.22f, .12f), new Vector2(.22f, .88f), new Vector2(.86f, .5f)));
                DrawIcon(ir, playTex, c);
                return;
            }
            float w = ir.width * .26f;                                             // hai vạch dọc bo góc
            GUI.DrawTexture(new Rect(ir.x + ir.width * .16f, ir.y + ir.height * .1f, w, ir.height * .8f), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, c, 0, w * .3f);
            GUI.DrawTexture(new Rect(ir.xMax - ir.width * .16f - w, ir.y + ir.height * .1f, w, ir.height * .8f), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, c, 0, w * .3f);
        }

        void DrawSpeakerIcon(Rect b, bool on, Color c)
        {
            if (!speakerTex)
            {
                speakerTex = MaskTex((u, v) => (u >= .08f && u <= .3f && v >= .36f && v <= .64f)
                    || InTri(u, v, new Vector2(.26f, .36f), new Vector2(.54f, .12f), new Vector2(.54f, .88f))
                    || InTri(u, v, new Vector2(.26f, .36f), new Vector2(.54f, .88f), new Vector2(.26f, .64f)));
                wavesTex = MaskTex((u, v) =>
                {
                    float dx = u - .54f, dy = v - .5f, d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dx <= 0 || Mathf.Abs(dy) > dx * 1.05f) return false;
                    return (d > .15f && d < .22f) || (d > .3f && d < .37f);
                });
                slashTex = MaskTex((u, v) =>
                {
                    // đường chéo từ trên-trái xuống dưới-phải, dày 0,1
                    float t = Mathf.Clamp01(((u - .1f) * .8f + (v - .1f) * .8f) / (.8f * .8f * 2f));
                    float px = .1f + t * .8f, py = .1f + t * .8f, dx = u - px, dy = v - py;
                    return dx * dx + dy * dy < .055f * .055f;
                });
            }
            var ir = IconRect(b, .66f);
            DrawIcon(ir, speakerTex, c);
            if (on) DrawIcon(ir, wavesTex, c);
            else DrawIcon(ir, slashTex, Red);
        }

        // Ngôi sao 5 cánh (font web không có glyph ★, như DrawGuiStar của game cũ).
        void DrawStar(Rect r, Color c)
        {
            if (!star)
            {
                const int S = 64; star = new Texture2D(S, S, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                var pts = new Vector2[10];
                for (int i = 0; i < 10; i++)
                {
                    float a = Mathf.PI / 2 + i * Mathf.PI / 5, rad = i % 2 == 0 ? 0.48f : 0.19f;
                    pts[i] = new Vector2(0.5f + Mathf.Cos(a) * rad, 0.5f + Mathf.Sin(a) * rad);
                }
                for (int yy = 0; yy < S; yy++)
                    for (int xx = 0; xx < S; xx++)
                    {
                        var p = new Vector2((xx + .5f) / S, (yy + .5f) / S); bool inside = false;
                        for (int i = 0, j = 9; i < 10; j = i++)
                            if ((pts[i].y > p.y) != (pts[j].y > p.y) && p.x < (pts[j].x - pts[i].x) * (p.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x) inside = !inside;
                        star.SetPixel(xx, yy, inside ? Color.white : new Color(1, 1, 1, 0));
                    }
                star.Apply();
            }
            var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, star); GUI.color = old;
        }

        // 21.396: nhãn «TẬP LUYỆN» xanh dương thay số mét trên thanh thông tin; trả về x sau nhãn.
        float PracticeTag(float x, float y)
        {
            const string t = "TẬP LUYỆN";
            label.fontSize = 17; float w = label.CalcSize(new GUIContent(t)).x + 20;
            Panel(new Rect(x, y, w, 30), new Color32(46, 125, 230, 235), 8);
            Txt(t, x, y, w, 30, 17, Cream, TextAnchor.MiddleCenter);
            return x + w;
        }

        public string Report() => $"mode={Mode} practice={Practice} score={Score:F0} best={Best} newBest={NewBest} runs={Runs} street={CurrentStreet} reason={EndReason}";
    }
}
