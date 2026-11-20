using System.Collections.Generic;
using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet.OpenWorld
{
    // Bản đồ mở — vòng chơi, điểm, thua, menu (21.257, đợt 7 phần 4). Giữ như game cũ (mục 2.1, PhoVietGame.OnGUI):
    // • Màn chào (Ready): thương hiệu PHỐ VIỆT, thẻ «LÊN XE THÔI» (Enter/Space), kỷ lục; xe đứng yên ở điểm xuất phát,
    //   giao thông vẫn chạy, CSGT và hiểm họa tạm nghỉ.
    // • Đang chạy: thanh HUD trên cùng — tên phố đang đi (có dấu), số mét (= quãng đường lái, OpenWorldHazards.DistanceDriven:
    //   không cộng khi xe hỏng/bị bắt/chìm), km/h, KỶ LỤC, khoảng cách CSGT, nút Ⅱ (P/Esc) và ÂM: BẬT/TẮT.
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
            Best = PlayerPrefs.GetInt(BestKey, 0);
            Sound = PlayerPrefs.GetInt(SoundKey, 1) == 1; AudioListener.volume = Sound ? 1f : 0f;
            if (!jailWide) jailWide = Resources.Load<Texture2D>("Jail/JailWide");
            if (!jailTall) jailTall = Resources.Load<Texture2D>("Jail/JailTall");
            if (!missileIcon) missileIcon = Resources.Load<Texture2D>("PlayerMissile/PV_MissileHud");
            if (player) { player.respawnAfterSink = false; player.showSpeed = false; }
            if (police) { police.drawJail = false; police.showDistance = false; }
            GoReady();
            InitGraphics();                                                 // 21.291: mức đồ họa đã lưu (sau thiết lập điện thoại ở Awake)
        }

        // ------------------------------------------------------------ chuyển trạng thái
        public void GoReady()
        {
            Resume();
            if (police) { police.enabled = true; police.ResetChase(true); police.Halt(); police.enabled = false; }
            if (hazards) { hazards.ClearAll(); hazards.enabled = false; }
            if (player) player.frozen = true;
            Mode = State.Ready; EndReason = null; EndDetail = null; NewBest = false; sinkAt = -1f; crashAt = -1f;
        }

        public void StartRun()
        {
            Resume();
            if (hazards) { hazards.ClearAll(); hazards.enabled = true; }   // số mét về 0 ngay khung hình này
            if (police) { police.enabled = true; police.ResetChase(true); }   // đưa xe về chỗ xuất phát, Runs++ → hiểm họa dọn sạch
            if (player) player.frozen = false;
            Mode = State.Riding; EndReason = null; EndDetail = null; NewBest = false; sinkAt = -1f; crashAt = -1f; Runs++; GraphicsOpen = false;
        }

        public void TogglePause()
        {
            if (Mode == State.Riding) { Mode = State.Paused; Time.timeScale = 0f; AudioListener.pause = true; }
            else if (Mode == State.Paused) { Mode = State.Riding; Resume(); }
        }
        void Resume() { Time.timeScale = 1f; AudioListener.pause = false; }

        void EndRun(State end, string reason)
        {
            Mode = end; EndReason = reason;
            int m = Mathf.FloorToInt(Score);
            if (m > Best) { Best = m; NewBest = true; PlayerPrefs.SetInt(BestKey, Best); PlayerPrefs.Save(); }
            if (hazards) hazards.enabled = false;
            if (player) player.frozen = true;                               // ẩn nút NHẢY, khóa lái
            if (end == State.GameOver && police) { police.Halt(); police.enabled = false; }
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
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) StartRun();
                    break;
                case State.Riding:
                    TickGovernor();
                    if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) { TogglePause(); break; }
                    if (police && police.Jailed) { EndRun(State.Jailed, "CSGT đã đưa bạn vào phòng giam."); break; }
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
                        else if (Time.time - sinkAt > 1.4f) EndRun(State.GameOver, "Rơi xuống sông!");
                    }
                    break;
                case State.Paused:
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
                if (!traffic || traffic.roads == null || !player) return "Phố Việt";
                var p = new Vector2(player.transform.position.x, player.transform.position.z);
                float best = float.MaxValue; string id = null;
                foreach (var r in traffic.roads)
                {
                    var ab = r.b - r.a; float t = Mathf.Clamp01(Vector2.Dot(p - r.a, ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
                    float d = (r.a + ab * t - p).sqrMagnitude;
                    if (d < best) { best = d; id = r.id; }
                }
                return id != null && StreetNames.TryGetValue(id, out var n) ? n : "Phố Việt";
            }
        }

        // ------------------------------------------------------------ IMGUI theo kiểu game cũ
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
                    if (GraphicsOpen) { GUI.enabled = false; ReadyScreen(); GUI.enabled = true; GraphicsWindow(); }
                    else ReadyScreen();
                    break;
                case State.Riding: Hud(); break;
                case State.Paused: Hud(); EndCard(true); break;
                case State.Jailed: JailScreen(); break;
                case State.GameOver: Hud(); EndCard(false); break;
            }
            GUI.matrix = Matrix4x4.identity;
        }

        void Brand(float x, float y)
        {
            Panel(new Rect(x - 10, y - 8, 259, 57), new Color(.97f, .985f, 1, .83f), 14);
            Panel(new Rect(x, y, 36, 36), Red, 10); DrawStar(new Rect(x + 5, y + 5, 26, 26), Gold);
            Txt("PHỐ VIỆT", x + 48, y - 3, 200, 30, 24, Ink); Txt("CHUYẾN XE KHÔNG NGỪNG", x + 49, y + 27, 198, 17, 9, Teal);
        }

        void ReadyScreen()
        {
            Brand((portrait ? 24 : 48) + SL, (portrait ? 24 : 38) + ST);
            if (!portrait)
            {
                Panel(new Rect(uiW - 277 - SR, 40 + ST, 229, 56), WelcomeGlass);
                Txt("ĐỊA ĐIỂM HÔM NAY", uiW - 257 - SR, 48 + ST, 200, 18, 9, Teal); Txt(CurrentStreet, uiW - 257 - SR, 67 + ST, 200, 25, 16, Ink);
            }
            float w = portrait ? 432 : 360, h = portrait ? 258 : 276;
            float y = Mathf.Clamp(uiH * (portrait ? .15f : .27f), 105 + ST, Mathf.Max(105 + ST, uiH - h - 95 - SB));
            var card = new Rect(portrait ? SL + (uiW - SL - SR - w) / 2 : 48 + SL, y, w, h); float x = card.x;
            Panel(new Rect(x - 3, y + 5, w + 6, h + 3), new Color(.04f, .13f, .20f, .14f), 21);
            Panel(new Rect(x - 1, y - 1, w + 2, h + 2), new Color(1, 1, 1, .68f), 19); Panel(card, WelcomeWhite, 18);
            Panel(new Rect(x + 22, y + 20, 5, 13), Red, 2);
            Txt("BẢN ĐỒ MỞ · SÀI GÒN", x + 36, y + 18, w - 110, 21, 11, Teal);
            GearButton(card);                                               // 21.291: cấu hình đồ họa
            Txt("LÁI QUA\nPHỐ QUEN.", x + 20, y + 48, w - 40, 108, portrait ? 39 : 41, Ink);
            Txt("Né CSGT, khủng long, xe tăng\nvà trực thăng trên phố mở.", x + 23, y + (portrait ? 143 : 152), w - 46, 42, 15, Hex("4C657A"));
            if (Btn("LÊN XE THÔI", new Rect(x + 22, y + h - 70, w - 44, 44), Red, Color.white, 17)) StartRun();
            Txt("KỶ LỤC  /  " + Metres(Best), x + 23, y + h - 20, w - 46, 17, 10, Teal);
            if (!portrait && !Input.touchSupported && !UnityEngine.Device.Application.isMobilePlatform)
            {
                float by = uiH - 82 - SB; Panel(new Rect(48 + SL, by, 754, 43), WelcomeGlass, 12);
                Txt("MŨI TÊN / WASD: LÁI     J: NHẢY     F: TÊN LỬA     SPACE: PHANH TAY     P: TẠM DỪNG", 64 + SL, by + 13, 724, 23, 11, Ink);
            }
            else
            {
                float hw = portrait ? uiW - 36 - SL - SR : 600, hx = SL + (uiW - SL - SR - hw) / 2;
                Panel(new Rect(hx, uiH - 68 - SB, hw, 44), WelcomeGlass, 10);
                Txt("GIỮ TRÁI / PHẢI ĐỂ LÁI · GIỮ GA / LÙI / STOP · NHẢY · TÊN LỬA", hx + 7, uiH - 55 - SB, hw - 14, 16, 10, Ink, TextAnchor.MiddleCenter);
            }
        }

        void Hud()
        {
            float bw = portrait ? uiW - 24 - SL - SR : 760, x = SL + (uiW - SL - SR - bw) / 2, y = (portrait ? 12 : 18) + ST;
            Panel(new Rect(x, y, bw, 76), Glass, 10);
            Txt(CurrentStreet.ToUpperInvariant(), x + 14, y + 8, bw * 0.45f, 16, 11, Gold);
            Txt(Metres(Score), x + 14, y + 25, 190, 34, 28, Cream);
            float kmh = player ? Mathf.Abs(player.Speed) * 3.6f : 0f;
            Txt(Mathf.RoundToInt(kmh) + " km/h", x + 18, y + 57, 120, 16, 11, Hex("B5CFC2"));
            float c1 = x + (portrait ? bw * 0.42f : 220);
            Panel(new Rect(c1 - 16, y + 11, 1, 54), new Color(.8f, .95f, .9f, .18f), 0);
            Txt("KỶ LỤC", c1, y + 13, 120, 16, 11, Gold);
            Txt(Metres(Mathf.Max(Best, Score)), c1, y + 33, 130, 26, 20, Cream);
            if (police && police.enabled && police.Mode != OpenWorldPolice.State.Waiting)
            {
                float c2 = x + (portrait ? bw * 0.68f : 370);
                bool flash = Mathf.FloorToInt(Time.unscaledTime * 4f) % 2 == 0;
                var col = police.Distance < 15f ? (flash ? Hex("FF5A4D") : Hex("5FA8FF")) : Cream;
                Panel(new Rect(c2 - 16, y + 11, 1, 54), new Color(.8f, .95f, .9f, .18f), 0);
                Txt("CSGT", c2, y + 13, 100, 16, 11, Gold);
                Txt(Mathf.RoundToInt(police.Distance) + " m", c2, y + 33, 110, 26, 20, col);
            }
            // 21.258: kho tên lửa (biểu tượng × số quả / tối đa)
            if (hazards)
            {
                float c3 = x + (portrait ? bw * 0.03f : 480);
                float cy = portrait ? y + 80 : y;
                if (portrait) Panel(new Rect(x, cy, bw, 34), Glass, 10);
                else Panel(new Rect(c3 - 16, y + 11, 1, 54), new Color(.8f, .95f, .9f, .18f), 0);
                Txt("TÊN LỬA  " + hazards.PlayerMissiles + "/" + hazards.maxMissiles, c3, cy + (portrait ? 8 : 13), 150, 16, 11, Gold);
                if (missileIcon)
                    for (int i = 0; i < hazards.maxMissiles; i++)
                    {
                        var old = GUI.color; GUI.color = i < hazards.PlayerMissiles ? Color.white : new Color(1, 1, 1, .22f);
                        GUI.DrawTexture(new Rect(c3 + (portrait ? 120 : 0) + i * 16, cy + (portrait ? 9 : 36), 16, 16), missileIcon, ScaleMode.ScaleToFit);
                        GUI.color = old;
                    }
            }
            FireButton();
            if (Btn(Mode == State.Paused ? "▶" : "Ⅱ", new Rect(x + bw - 56, y + 10, 40, 36), new Color(1, .945f, .812f, .86f), Ink, 19)) TogglePause();
            if (!portrait && Btn(Sound ? "ÂM: BẬT" : "ÂM: TẮT", new Rect(x + bw - 103, y + 53, 87, 20), new Color(.07f, .19f, .21f, .35f), Cream, 11)) SetSound(!Sound);
        }

        // Nút TÊN LỬA phía trên nút NHẢY (góc phải dưới); bàn phím: F.
        void FireButton()
        {
            if (!hazards || !player || player.frozen || player.Crashed || Mode != State.Riding) return;
            // 21.265: vị trí theo bố cục nút cảm ứng của OpenWorldDriver (px → đơn vị GUI). Máy cảm ứng: chạm do OpenWorldDriver đọc
            // đa điểm (vừa lái vừa bắn); nút IMGUI chỉ còn cho chuột.
            var f = OpenWorldDriver.FireRect; var r = new Rect(f.x / uiScale, f.y / uiScale, f.width / uiScale, f.height / uiScale);
            bool ready = hazards.CanFire, touch = player.TouchControlsVisible;
            // 21.266: nút vẽ sẵn (PV_Btn_Missile) + huy hiệu số tên lửa; máy tính thêm chữ phím F, bấm chuột vẫn bắn.
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity;
            OpenWorldDriver.DrawArtButton(f, OpenWorldDriver.MissileButtonTex, new Color(.91f, .27f, .23f), false, ready);
            float bs = f.width * .3f; var badge = new Rect(f.x - bs * .1f, f.y - bs * .1f, bs, bs);
            GUI.DrawTexture(badge, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, ready ? Gold : new Color(.55f, .58f, .62f), 0, bs * .5f);
            GUI.DrawTexture(badge, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Ink, bs * .1f, bs * .5f);
            var st = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(bs * .56f) }; st.normal.textColor = Ink;
            GUI.Label(badge, hazards.PlayerMissiles.ToString(), st);
            if (!touch) OpenWorldDriver.KeyBadge(f, "F");
            bool click = !touch && Event.current.type == EventType.MouseDown && f.Contains(Event.current.mousePosition);
            GUI.matrix = m;
            if (click) { hazards.RequestFire(); Event.current.Use(); }
        }

        void EndCard(bool paused)
        {
            Panel(Fullscreen(), new Color(.04f, .12f, .14f, .63f), 0);
            float w = portrait ? 432 : 480, h = paused ? 340 : 465, x = (uiW - w) / 2, y = (uiH - h) / 2;
            Panel(new Rect(x, y, w, h), Cream, 18);
            Txt(paused ? "NGHỈ MỘT CHÚT" : "KẾT THÚC CHUYẾN XE", x + 28, y + 28, w - 56, 25, 12, Red);
            Txt(paused ? "Dừng chân bên phố." : EndReason ?? "Chuyến đi kết thúc!", x + 27, y + 62, w - 54, 48, portrait ? 31 : 36, Ink);
            if (!paused && !string.IsNullOrEmpty(EndDetail)) Txt(EndDetail, x + 28, y + 103, w - 56, 24, portrait ? 16 : 18, Ink);
            if (paused)
            {
                Txt("Phố vẫn đợi. Sẵn sàng thì mình đi tiếp nhé.", x + 28, y + 140, w - 56, 50, 16, Teal);
                if (Btn("ĐI TIẾP", new Rect(x + 28, y + 215, w - 56, 58), Teal, Cream, 18)) TogglePause();
                if (Btn("VỀ ĐẦU PHỐ", new Rect(x + 28, y + 285, w - 56, 34), Cream, Teal, 12)) GoReady();
            }
            else
            {
                Txt("Bạn đã đi được", x + 28, y + 137, w - 56, 27, 15, Teal);
                Txt(Metres(Score), x + 24, y + 172, w - 48, 75, 58, Ink);
                Panel(new Rect(x + 28, y + 267, w - 56, 1), Hex("D8D6BF"), 0);
                Txt(NewBest ? "KỶ LỤC MỚI!" : "", x + 28, y + 286, 200, 27, 13, Teal);
                Txt("KỶ LỤC  " + Metres(Best), x + w - 225, y + 286, 200, 27, 13, Red, TextAnchor.UpperRight);
                if (Btn("THỬ MỘT CHUYẾN NỮA", new Rect(x + 28, y + 334, w - 56, 58), Red, Cream, portrait ? 15 : 18)) StartRun();
                if (Btn("VỀ ĐẦU PHỐ", new Rect(x + 28, y + 404, w - 56, 34), Cream, Teal, 12)) GoReady();
            }
        }

        void JailScreen()
        {
            var tex = portrait ? (jailTall ? jailTall : jailWide) : (jailWide ? jailWide : jailTall);
            if (tex) GUI.DrawTexture(Fullscreen(), tex, ScaleMode.ScaleAndCrop);
            float w = portrait ? 440 : 408, h = portrait ? 262 : 318, x = portrait ? 20 + SL : uiW - w - 42 - SR, y = portrait ? uiH - h - 22 - SB : (uiH - h) / 2;
            Panel(new Rect(x, y, w, h), Cream, 18);
            Txt("PHỐ VIỆT  ·  TẠM GIỮ", x + 26, y + 20, w - 52, 22, 13, Red);
            Txt("ĐÃ BỊ BẮT", x + 24, y + 45, w - 48, 52, portrait ? 34 : 40, Ink);
            Txt(EndReason ?? "CSGT đã đưa bạn vào phòng giam.", x + 26, y + 100, w - 52, 25, 16, Ink);
            if (player && !string.IsNullOrEmpty(player.CrashReason)) Txt("Lý do: " + player.CrashReason.ToLowerInvariant(), x + 26, y + 124, w - 52, 22, 13, Hex("4C657A"));
            Txt("Quãng đường  " + Metres(Score) + (NewBest ? "  ·  KỶ LỤC MỚI!" : "  ·  Kỷ lục " + Metres(Best)), x + 26, y + 150, w - 52, 28, 16, Teal);
            if (Btn("CHƠI LẠI", new Rect(x + 26, y + (portrait ? 190 : 196), w - 52, 48), Red, Cream, 18)) StartRun();
            if (!portrait && Btn("VỀ ĐẦU PHỐ", new Rect(x + 26, y + 258, w - 52, 32), Cream, Teal, 13)) GoReady();
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

        public string Report() => $"mode={Mode} score={Score:F0} best={Best} newBest={NewBest} runs={Runs} street={CurrentStreet} reason={EndReason}";
    }
}
