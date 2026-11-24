using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.322 — Menu bản đồ mở theo mẫu C «Tem đua» bản nhỏ (21.321b, người dùng chọn 22/11/2026). Số đo lấy từ đối tượng C2 trong
    // ArtSource/Concepts/Menus-20261122/menus.html (khung điểm 1947 × 900 = iPhone Pro Max ngang; máy tính 1440 × 900 cùng cỡ).
    // Khối nghiêng −11° vẽ bằng GUI.matrix có hệ số trượt (chạm/bấm vẫn đúng vì IMGUI đổi tọa độ chuột theo ma trận nghịch đảo).
    // Màn dọc (chỉ khi trình duyệt chưa xoay): cả lớp menu phóng về khung rộng 1000 điểm để giữ bố cục.
    public partial class OpenWorldGame
    {
        static readonly Color MNavy = new Color32(18, 35, 58, 255), MNavy2 = new Color32(11, 23, 38, 255), MCream = new Color32(252, 242, 207, 255),
            MGold = new Color32(245, 197, 66, 255), MTeal = new Color32(51, 224, 200, 255), MTealDark = new Color32(17, 160, 145, 255),
            MRed = new Color32(255, 90, 74, 255), MRedDark = new Color32(212, 46, 34, 255), MBlue = new Color32(74, 163, 255, 255),
            MPanel = new Color(10 / 255f, 22 / 255f, 34 / 255f, .94f), MShadow = new Color(0, 0, 0, .3f);
        const float SkewK = -0.19438f;                                     // tan(−11°): x' = x + k·(y − tâm)

        float mW, mH, mf = 1f;                                             // khung điểm của lớp menu
        Matrix4x4 menuM;
        Texture2D gradV, gradBand, gradRow, checkerTex, hazardTex, chevTex, retryTex, homeTex, checkTex, closeTex, boomTex;
        Texture2D[] hintBtns;
        static readonly string[] HintBtnNames = { "Left", "Right", "Shield", "Reverse", "Gas", "Jump", "Missile", "Laser" };

        float MSL => SL / mf; float MSR => SR / mf; float MST => ST / mf; float MSB => SB / mf;

        void BeginMenu()
        {
            mf = portrait ? Mathf.Max(.2f, uiW / 1000f) : 1f;
            mW = uiW / mf; mH = uiH / mf;
            menuM = Matrix4x4.Scale(Vector3.one * uiScale * mf);
            GUI.matrix = menuM;
            EnsureMenuArt();
        }
        void EndMenu() { GUI.matrix = Matrix4x4.Scale(Vector3.one * uiScale); }

        // ---------------------------------------------------------------- ma trận nghiêng / xoay
        void Skew(float pivotY) => GUI.matrix = menuM * Matrix4x4.Translate(new Vector3(0, pivotY, 0)) * Shear() * Matrix4x4.Translate(new Vector3(0, -pivotY, 0));
        void Unskew() => GUI.matrix = menuM;
        static Matrix4x4 Shear() { var s = Matrix4x4.identity; s.m01 = SkewK; return s; }
        static Matrix4x4 RotZ(Vector2 pivot, float deg) =>
            Matrix4x4.Translate(new Vector3(pivot.x, pivot.y, 0)) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, deg)) * Matrix4x4.Translate(new Vector3(-pivot.x, -pivot.y, 0));
        /// Vị trí x của điểm (x, y) sau khi khối quanh tâm pivotY bị nghiêng (để đặt hình không nghiêng đúng chỗ).
        static float SkewX(float x, float y, float pivotY) => x + SkewK * (y - pivotY);

        // ---------------------------------------------------------------- vẽ cơ bản
        void Rect0(Rect r, Color c) => GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, c, 0, 0);
        void Frame0(Rect r, Color c, float w) => GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, c, w, 0);
        void Grad(Rect r, Texture2D t, Color c) => GUI.DrawTexture(r, t, ScaleMode.StretchToFill, true, 0, c, 0, 0);
        void Tile(Rect r, Texture2D t, float cell) => GUI.DrawTextureWithTexCoords(r, t, new Rect(0, 0, r.width / cell, r.height / cell));
        void Checker(Rect r) => Tile(r, checkerTex, 20f);
        float TextW(string t, int size) { label.fontSize = size; return label.CalcSize(new GUIContent(t)).x; }
        // 21.327: khung cao 2,8 × cỡ chữ (trước 2 ×) — chữ hoa có 2 dấu chồng (Ố, Ấ, Ế…) ở cỡ 80 bị cắt mất dấu sắc trên cùng
        void TxtL(string t, float x, float cy, int size, Color c, float w = 1200) => Txt(t, x, cy - size * 1.4f, w, size * 2.8f, size, c, TextAnchor.MiddleLeft);
        void TxtR(string t, float right, float cy, int size, Color c, float w = 900) => Txt(t, right - w, cy - size, w, size * 2, size, c, TextAnchor.MiddleRight);
        void TxtC(string t, Rect r, int size, Color c) => Txt(t, r.x - 200, r.y, r.width + 400, r.height, size, c, TextAnchor.MiddleCenter);
        void Ico(Texture2D t, float x, float y, float s, Color c) => DrawIcon(new Rect(x, y, s, s), t, c);
        void Chevrons(float x, float cy, float h, Color c)
        {
            for (int i = 0; i < 3; i++) Ico(chevTex, x + i * h * .46f, cy - h / 2, h, new Color(c.r, c.g, c.b, 1f - i * .25f));
        }
        float ChevronsW(float h) => h * 1.92f;

        /// Khối tối nghiêng có viền màu trái (hoặc phải) + bóng đổ lệch.
        void SkewPanel(Rect r, Color accent, float border, bool rightBorder = false, float shadow = 13, float alpha = .94f)
        {
            Skew(r.center.y);
            if (shadow > 0) Rect0(new Rect(r.x + shadow, r.y + shadow, r.width, r.height), MShadow);
            Rect0(r, new Color(MPanel.r, MPanel.g, MPanel.b, alpha));
            // 21.355: border 0 (logo màn chào, 21.350) thì không vẽ vạch — Rect rộng 0 vẫn lọt 4 chấm đỏ ở mép trái khi khối bị nghiêng
            if (border > 0f) Rect0(rightBorder ? new Rect(r.xMax - border, r.y, border, r.height) : new Rect(r.x, r.y, border, r.height), accent);
            Unskew();
        }

        enum MBtn { Red, Teal, Ghost, Dark }
        /// Nút nghiêng kiểu tem đua; trả về true khi bấm. icon/iconSize tùy chọn, đặt trước chữ.
        bool SkewButton(Rect r, string text, int size, MBtn kind, Texture2D icon = null, float iconSize = 0, bool chevrons = false)
        {
            Skew(r.center.y);
            bool hover = r.Contains(Event.current.mousePosition);
            switch (kind)
            {
                case MBtn.Red: Rect0(new Rect(r.x + 8, r.y + 8, r.width, r.height), MNavy2); Grad(r, gradV, hover ? Color.Lerp(MRed, Color.white, .08f) : MRed); break;
                case MBtn.Teal: Rect0(new Rect(r.x + 8, r.y + 8, r.width, r.height), MNavy2); Grad(r, gradV, hover ? Color.Lerp(MTeal, Color.white, .08f) : MTeal); break;
                case MBtn.Ghost: Rect0(r, new Color(MCream.r, MCream.g, MCream.b, hover ? .16f : .1f)); Frame0(r, new Color(MCream.r, MCream.g, MCream.b, .55f), 3); break;
                case MBtn.Dark: Rect0(r, new Color(MPanel.r, MPanel.g, MPanel.b, .9f)); Frame0(r, new Color(MCream.r, MCream.g, MCream.b, hover ? .8f : .55f), 3); break;
            }
            Color fg = kind == MBtn.Teal ? MNavy : MCream;
            float tw = string.IsNullOrEmpty(text) ? 0 : TextW(text, size), gap = 12;
            float iw = icon ? iconSize : 0, cw = chevrons ? ChevronsW(size * .75f) : 0;
            float total = iw + (iw > 0 && tw > 0 ? gap : 0) + tw + (cw > 0 ? gap + cw : 0);
            float x = r.center.x - total / 2;
            if (icon) { Ico(icon, x, r.center.y - iconSize / 2, iconSize, fg); x += iw + (tw > 0 ? gap : 0); }
            if (tw > 0) { TxtL(text, x, r.center.y, size, fg, tw + 40); x += tw; }
            if (chevrons) Chevrons(x + gap, r.center.y, size * .75f, fg);
            bool click = GUI.Button(r, GUIContent.none, GUIStyle.none);
            Unskew();
            return click;
        }

        void Flag(Rect r) { Panel(r, new Color32(218, 37, 29, 255), r.width * .26f); DrawStar(new Rect(r.x + r.width * .16f, r.y + r.height * .16f, r.width * .68f, r.height * .68f), MGold); }

        // ---------------------------------------------------------------- màn chào
        void ReadyScreen()
        {
            float x = Mathf.Lerp(70, 110, Mathf.InverseLerp(1440, 1900, mW)) + MSL, top = 146 + MST * .6f;
            // dải tối nghiêng bên trái cho chữ dễ đọc
            Skew(mH / 2); Grad(new Rect(-200, 0, x + 770, mH), gradBand, new Color(6 / 255f, 14 / 255f, 22 / 255f, .8f)); Unskew();

            MenuBrand(x, 32 + MST);
            MenuRanks(mW - (x - MSL) - MSR, 32 + MST);   // 21.357: thay ô «Địa điểm hôm nay»

            // nhãn + tiêu đề + mô tả (một khối nghiêng)
            float py = top + 160;
            Skew(py);
            float tagW = TextW("BẢN ĐỒ MỞ · SÀI GÒN", 17) + 36;
            Rect0(new Rect(x, top, tagW, 40), MRed); TxtL("BẢN ĐỒ MỞ · SÀI GÒN", x + 18, top + 20, 17, MCream);
            TxtL("ĐẠI NÁO", x - 4, top + 56 + 32, 80, MCream);   // 21.350: «LÁI XE QUA» → «ĐẠI NÁO»
            // 21.350: ô vàng + chữ «PHỐ QUEN.» nhỏ lại (80 → 62); ô cao 88, chữ đặt thấp trong ô (trên tâm 50, dưới 38)
            // để cả dấu mũ + dấu sắc chồng trên «Ố» nằm trong ô vàng (chữ xanh đậm lọt ra nền tối là mất dấu).
            float bw = TextW("PHỐ QUEN.", 62) + 34, by = top + 140, ty = by + 48;   // ô hạ 10 điểm: dấu nặng «Ạ» của «ĐẠI» không chạm mép ô
            Rect0(new Rect(x + 9, ty - 50 + 8, bw, 88), MRed); Rect0(new Rect(x, ty - 50, bw, 88), MGold);
            TxtL("PHỐ QUEN.", x + 17, ty, 62, MNavy);
            Txt("Trốn CSGT. Tiêu diệt khủng long, xe tăng,\ntrực thăng, tàu trên bản đồ.", x, by + 104, 600, 64, 22, new Color(MCream.r, MCream.g, MCream.b, .82f));   // 21.327
            Unskew();

            float cy = top + 352;
            if (SkewButton(new Rect(x + 14, cy, 460, 82), "LÊN XE THÔI", 32, MBtn.Red, null, 0, true)) StartRun();
            // bánh răng — cấu hình đồ họa (21.291)
            var g = new Rect(x + 492, cy, 82, 82);
            Skew(g.center.y);
            Rect0(g, new Color(MPanel.r, MPanel.g, MPanel.b, .92f)); Frame0(g, MTeal, 3);
            bool gearHit = GUI.Button(new Rect(g.x - 8, g.y - 8, g.width + 16, g.height + 16), GUIContent.none, GUIStyle.none);
            Unskew();
            DrawGear(new Rect(g.center.x - 20, g.center.y - 20, 40, 40), MTeal);
            if (gearHit) OpenGraphics();

            // kỷ lục
            float ry = cy + 104 + 14;
            Skew(ry);
            EnsureHudIcons();
            Ico(trophyTex, x + 20, ry - 12, 23, MGold);
            TxtL("KỶ LỤC", x + 53, ry, 14, MGold);
            float vx = x + 53 + TextW("KỶ LỤC", 14) + 10;
            TxtL(Metres(Best), vx, ry, 25, MCream);
            Checker(new Rect(vx + TextW(Metres(Best), 25) + 14, ry - 5, 80, 10));
            Unskew();

            bool touchUI = Input.touchSupported || UnityEngine.Device.Application.isMobilePlatform || (player && player.testTouchUI);
            if (touchUI) TouchHints(x); else KeyHints(x);
        }

        void MenuBrand(float x, float y)
        {
            // 21.350: bỏ vạch đỏ 8 px trước lá cờ (border 0), dời nội dung sang trái 8 px; khẩu hiệu «VÒNG VÂY RỰC LỬA».
            float w = 14 + 42 + 13 + Mathf.Max(TextW("GờTa Việt", 25), TextW("VÒNG VÂY RỰC LỬA", 11)) + 24;
            var r = new Rect(x, y, w, 66);
            SkewPanel(r, MRed, 0, false, 0, .9f);
            Skew(r.center.y);
            Flag(new Rect(x + 14, y + 12, 42, 42));
            TxtL("GờTa Việt", x + 69, y + 25, 25, MCream);
            TxtL("VÒNG VÂY RỰC LỬA", x + 70, y + 48, 11, MGold);
            Unskew();
        }

        // 21.357: người dùng — thay ô «ĐỊA ĐIỂM HÔM NAY / tên phố» góc phải trên màn chào bằng ô «Bảng xếp hạng» có cúp phía trước;
        // bấm vào hiện danh sách bảng xếp hạng (hộp Top 100 của trang web, account.js — OpenWorldGame.Leaderboard.cs).
        void MenuRanks(float right, float y)
        {
            EnsureHudIcons();
            const string label = "Bảng xếp hạng";
            float w = 20 + 36 + 14 + TextW(label, 26) + 16 + ChevronsW(14) + 20 + 8;
            var r = new Rect(right - w, y, w, 68);
            bool hover = r.Contains(Event.current.mousePosition);
            SkewPanel(r, MGold, 8, true, 0, hover ? .98f : .9f);
            Skew(r.center.y);
            Ico(trophyTex, r.x + 20, r.center.y - 18, 36, MGold);
            float tx = r.x + 20 + 36 + 14;
            TxtL(label, tx, r.center.y, 26, MCream);
            Chevrons(tx + TextW(label, 26) + 16, r.center.y, 14, MGold);
            bool hit = GUI.Button(new Rect(r.x - 8, r.y - 8, r.width + 16, r.height + 16), GUIContent.none, GUIStyle.none);
            Unskew();
            if (hit) OpenRanksOnWeb();
        }

        void KeyHints(float x)
        {
            float h = 62, y = mH - 32 - h - MSB, cy = y + h / 2;
            // 21.383 (người dùng): bỏ WASD — mũi tên lái/ga/lùi; Q tên lửa, W lazer, E khiên, R phanh.
            string[][] items = { new[] { "←", "→", "LÁI" }, new[] { "↑", "↓", "GA/LÙI" }, new[] { "SPACE", "NHẢY" }, new[] { "Q", "TÊN LỬA" }, new[] { "W", "LAZER" }, new[] { "E", "KHIÊN" }, new[] { "R", "PHANH" }, new[] { "P", "TẠM DỪNG" } };
            float w = 8 + 22;
            foreach (var it in items) { for (int i = 0; i < it.Length - 1; i++) w += Mathf.Max(36, TextW(it[i], 16) + 18) + 7; w += TextW(it[it.Length - 1], 17) + 20; }
            w += 2;
            SkewPanel(new Rect(x, y, w, h), MGold, 8, false, 0, .9f);
            Skew(cy);
            float px = x + 8 + 22;
            foreach (var it in items)
            {
                for (int i = 0; i < it.Length - 1; i++)
                {
                    float kw = Mathf.Max(36, TextW(it[i], 16) + 18);
                    var k = new Rect(px, cy - 18, kw, 36);
                    Panel(new Rect(k.x, k.y + 3, k.width, k.height), MNavy, 9);
                    Panel(k, MCream, 9);
                    GUI.DrawTexture(k, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, MNavy, 3, 9);
                    TxtC(it[i], k, 16, MNavy);
                    px += kw + 7;
                }
                string lab = it[it.Length - 1];
                TxtL(lab, px, cy, 17, MCream);
                px += TextW(lab, 17) + 20;
            }
            Unskew();
        }

        void TouchHints(float x)
        {
            float h = 84, y = mH - 28 - h - MSB, cy = y + h / 2;
            float w = 8 + 20 + HintBtnNames.Length * (52 + 13) + 4 + Mathf.Max(TextW("Giữ TRÁI / PHẢI để lái", 17), TextW("GA · LÙI · NHẢY · TÊN LỬA", 17)) + 20;
            SkewPanel(new Rect(x, y, w, h), MGold, 8, false, 0, .9f);
            float px = x + 8 + 20;
            for (int i = 0; i < hintBtns.Length; i++)
            {
                if (hintBtns[i]) GUI.DrawTexture(new Rect(px, cy - 26, 52, 52), hintBtns[i], ScaleMode.ScaleToFit);
                px += 52 + 13;
            }
            Skew(cy);
            Txt("Giữ TRÁI / PHẢI để lái\nGA · LÙI · NHẢY · TÊN LỬA", px + 4, cy - 26, 320, 52, 17, new Color(MCream.r, MCream.g, MCream.b, .86f), TextAnchor.MiddleLeft);
            Unskew();
        }

        // ---------------------------------------------------------------- cấu hình đồ họa (21.291, giao diện 21.322)
        public static Rect GearRect(Rect card) => new Rect(card.xMax - 64, card.y + 12, 50, 50);   // giữ cho mã cũ/kiểm tra

        void GraphicsWindow()
        {
            Rect0(new Rect(0, 0, mW, mH), new Color(3 / 255f, 10 / 255f, 16 / 255f, .76f));
            float w = Mathf.Min(900, mW - 60 - MSL - MSR), h = Mathf.Min(640, mH - 24 - MST - MSB);
            var r = new Rect(MSL + (mW - MSL - MSR - w) / 2, MST + (mH - MST - MSB - h) / 2, w, h);
            var e = Event.current;                                          // chạm ra ngoài cửa sổ = đóng
            if (e.type == EventType.MouseDown && !r.Contains(e.mousePosition)) { CloseGraphics(); e.Use(); return; }
            SkewPanel(r, MTeal, 11, false, 13, .95f);
            float ix = r.x + 47, cw = r.width - 83;

            Skew(r.center.y);
            DrawGear(new Rect(ix, r.y + 28, 40, 40), MTeal);
            TxtL("CẤU HÌNH ĐỒ HỌA", ix + 53, r.y + 48, 34, MCream);
            Checker(new Rect(ix + cw - 62 - 110, r.y + 43, 110, 11));
            TxtL("Chọn mức hợp với máy của bạn · lưu cho lần chơi sau.", ix, r.y + 88, 18, MGold);
            var close = new Rect(r.xMax - 24 - 52, r.y + 22, 52, 52);
            Rect0(close, close.Contains(e.mousePosition) ? Color.Lerp(MRed, Color.white, .1f) : MRed);
            Ico(closeTex, close.center.x - 12, close.center.y - 12, 24, MCream);
            bool closeHit = GUI.Button(close, GUIContent.none, GUIStyle.none);

            string[] desc =
            {
                "Độ phân giải gốc của màn hình · nét nhất. Đã tối ưu bộ nhớ để không văng game.",
                "Nét gần bằng Rất cao, nhẹ máy hơn một chút.",
                "Tự động điều chỉnh khi đang chơi theo tình trạng máy.",
                "Dành cho máy yếu · mượt nhất, tắt bóng đổ.",
            };
            int[] lv = { 4, 3, 2, 1 };
            float rowTop = r.y + 116, rowH = Mathf.Min(92, (h - 116 - 26 - 76 - 24 - 30) / 4f);
            int pick = -1;
            for (int i = 0; i < 4; i++)
            {
                var g = (Gfx)i; bool sel = Graphics == g;
                var row = new Rect(ix, rowTop + i * (rowH + 10), cw, rowH);
                bool hov = row.Contains(e.mousePosition);
                if (sel) Grad(row, gradRow, new Color(MTeal.r, MTeal.g, MTeal.b, .28f));
                else Rect0(row, new Color(MCream.r, MCream.g, MCream.b, hov ? .09f : .05f));
                if (sel) Rect0(new Rect(row.xMax - 7, row.y, 7, row.height), MTeal);
                var num = new Rect(row.x, row.y, 88, row.height);
                Rect0(num, sel ? MTeal : new Color(MCream.r, MCream.g, MCream.b, .08f));
                TxtC(lv[i].ToString(), num, 50, sel ? MNavy : MCream);
                // ô độ phân giải + vạch mức (phải sang trái)
                string t = g == Gfx.Medium ? (sel ? "TỰ ĐỘNG · " + ResolutionText(g) : "TỰ ĐỘNG") : ResolutionText(g);
                float pw = Mathf.Max(170, TextW(t, 15) + 24);
                var pill = new Rect(row.xMax - 27 - pw, row.center.y - 17, pw, 34);
                Rect0(pill, sel ? MTeal : new Color(MCream.r, MCream.g, MCream.b, .1f));
                TxtC(t, pill, 15, sel ? MNavy : MCream);
                float bx = pill.x - 20 - (4 * 13 + 3 * 5);
                for (int k = 0; k < 4; k++)
                {
                    float bh = 13 + k * 8;
                    Rect0(new Rect(bx + k * 18, row.center.y + 19 - bh, 13, bh), k < lv[i] ? (sel ? MTeal : MGold) : new Color(MCream.r, MCream.g, MCream.b, .15f));
                }
                float tx = row.x + 108, tw = bx - 20 - tx;
                TxtL(GfxName(g), tx, row.y + rowH * .3f, 26, MCream, tw);
                Txt(desc[i], tx, row.y + rowH * .5f, tw, rowH * .5f, 16, new Color(MCream.r, MCream.g, MCream.b, .7f));
                if (GUI.Button(row, GUIContent.none, GUIStyle.none)) pick = i;
            }
            Unskew();
            if (pick >= 0) SetGraphics(pick);
            if (SkewButton(new Rect(ix, r.yMax - 26 - 76, cw, 76), "XONG", 28, MBtn.Teal, checkTex, 30) || closeHit) CloseGraphics();
        }

        // ---------------------------------------------------------------- tạm dừng / kết thúc chuyến
        float MenuOffsetY => (mH - 900) / 2 + (MST - MSB) / 2;

        void EndCard(bool paused)
        {
            Rect0(new Rect(0, 0, mW, mH), new Color(4 / 255f, 12 / 255f, 18 / 255f, .7f));
            if (paused) PauseCard(); else GameOverCard();
        }

        void PauseCard()
        {
            float oy = MenuOffsetY, x = MSL + (mW - MSL - MSR - 740) / 2, y = 250 + oy;
            var r = new Rect(x, y, 740, 350);
            SkewPanel(r, MTeal, 11);
            Skew(r.center.y);
            var ib = new Rect(x + 51, y + 32, 88, 88);
            Rect0(ib, MTeal); DrawPauseIcon(ib, false, MNavy);
            TxtL("NGHỈ MỘT CHÚT", ib.xMax + 20, y + 48, 16, MGold);
            TxtL("Dừng chân bên phố.", ib.xMax + 18, y + 90, 54, MCream);
            TxtL("Phố vẫn đợi. Sẵn sàng thì mình đi tiếp nhé.", x + 51, y + 162, 21, new Color(MCream.r, MCream.g, MCream.b, .8f));
            Checker(new Rect(r.xMax - 32 - 96, y + 28, 96, 11));
            Unskew();
            if (SkewButton(new Rect(x + 32, y + 250, 450, 84), "ĐI TIẾP", 28, MBtn.Teal, playIcon, 30)) { TogglePause(); return; }
            if (SkewButton(new Rect(x + 32, y + 364, 320, 64), "VỀ ĐẦU PHỐ", 20, MBtn.Dark, homeTex, 23)) { GoReady(); return; }
            var sb = new Rect(x + 376, y + 364, 74, 64);
            if (SkewButton(sb, null, 20, MBtn.Dark)) SetSound(!Sound);
            DrawSpeakerIcon(new Rect(sb.center.x - 22, sb.center.y - 22, 44, 44), Sound, MCream);
            var gb = new Rect(x + 474, y + 364, 74, 64);
            if (SkewButton(gb, null, 20, MBtn.Dark)) OpenGraphics();
            DrawGear(new Rect(gb.center.x - 16, gb.center.y - 16, 32, 32), MTeal);
        }

        Texture2D playIcon
        {
            get
            {
                if (!playTex) playTex = MaskTex((u, v) => InTri(u, v, new Vector2(.22f, .12f), new Vector2(.22f, .88f), new Vector2(.86f, .5f)));
                return playTex;
            }
        }

        void GameOverCard()
        {
            float oy = MenuOffsetY;
            // băng đỏ chéo «KẾT THÚC CHUYẾN XE» có viền ca-rô
            var band = new Rect(-80, 196 + oy, mW + 160, 120);
            GUI.matrix = menuM * RotZ(band.center, -4f);
            Rect0(new Rect(band.x, band.y + 11, band.width, band.height), MShadow);
            Grad(band, gradV, MRed);
            Checker(new Rect(band.x, band.y, band.width, 13)); Checker(new Rect(band.x, band.yMax - 13, band.width, 13));
            GUI.matrix = menuM * RotZ(band.center, -4f) * Matrix4x4.Translate(new Vector3(0, band.center.y, 0)) * Shear() * Matrix4x4.Translate(new Vector3(0, -band.center.y, 0));
            TxtC("KẾT THÚC CHUYẾN XE", new Rect(band.x, band.y + 13, band.width, band.height - 26), 62, MCream);
            Unskew();

            float x = MSL + (mW - MSL - MSR - 880) / 2, y = 380 + oy;
            var r = new Rect(x, y, 880, 244);
            SkewPanel(r, MGold, 11);
            Skew(r.center.y);
            float ix = x + 47;
            bool crash = !string.IsNullOrEmpty(EndDetail);
            if (crash) { Ico(boomTex, ix, y + 24, 56, new Color32(243, 154, 59, 255)); Ico(boomTex, ix + 14, y + 38, 28, MGold); }
            float tx = crash ? ix + 74 : ix;
            TxtL(EndReason ?? "Chuyến đi kết thúc!", tx, y + (crash ? 44 : 50), 38, MCream, r.xMax - tx - 30);
            if (crash) TxtL(EndDetail, tx, y + 80, 19, new Color(MCream.r, MCream.g, MCream.b, .75f), r.xMax - tx - 30);

            TxtL("BẠN ĐÃ ĐI ĐƯỢC", ix, y + 118, 16, MGold);
            int sc = Mathf.Max(0, Mathf.FloorToInt(Score));
            string num = sc.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
            TxtL(num, ix - 3, y + 180, 84, MGold);
            float nw = TextW(num, 84);
            TxtL("m", ix + nw + 6, y + 196, 32, MCream);

            float bx0 = ix + Mathf.Max(220, nw + 70), bx1 = r.xMax - 36;
            float best = Mathf.Max(1, Best);
            EnsureHudIcons();
            string bestTxt = Metres(Best), bestLab = NewBest ? "KỶ LỤC MỚI!" : "KỶ LỤC";
            float bw = TextW(bestTxt, 23), lw = TextW(bestLab, 13);
            TxtR(bestTxt, bx1, y + 160, 23, MCream);
            TxtR(bestLab, bx1 - bw - 10, y + 160, 13, MGold);
            Ico(trophyTex, bx1 - bw - lw - 20 - 21, y + 150, 21, MGold);
            int fill = NewBest ? 24 : Mathf.Clamp(Mathf.RoundToInt(24f * sc / best), 0, 24);
            float sw = (bx1 - bx0 - 23 * 3) / 24f;
            for (int k = 0; k < 24; k++) Rect0(new Rect(bx0 + k * (sw + 3), y + 180, sw, 14), k < fill ? MGold : new Color(MCream.r, MCream.g, MCream.b, .14f));
            int left = Best - sc;
            string note = NewBest ? "Chuyến này là kỷ lục mới!" : left > 0 ? "Còn " + Metres(left) + " nữa là chạm kỷ lục" : "Bằng kỷ lục!";
            TxtR(note, bx1, y + 212, 15, new Color(MCream.r, MCream.g, MCream.b, .72f));
            Unskew();

            if (SkewButton(new Rect(x + 24, y + 276, 510, 84), "THỬ MỘT CHUYẾN NỮA", 28, MBtn.Red, retryTex, 32)) { StartRun(); return; }
            if (SkewButton(new Rect(x + 566, y + 276, 290, 84), "VỀ ĐẦU PHỐ", 20, MBtn.Ghost, homeTex, 25)) GoReady();
        }

        // ---------------------------------------------------------------- phòng giam
        void JailScreen()
        {
            var tex = portrait ? (jailTall ? jailTall : jailWide) : (jailWide ? jailWide : jailTall);
            if (tex) GUI.DrawTexture(new Rect(0, 0, mW, mH), tex, ScaleMode.ScaleAndCrop);
            Tape(new Rect(-100, 44, 760, 52), -8f, "CSGT · TẠM GIỮ", "CSGT · TẠM GIỮ");
            Tape(new Rect(-40, mH - 76 - 52, 840, 52), 5f, "KHÔNG PHẬN SỰ MIỄN VÀO", "CSGT");

            float oy = MenuOffsetY;
            string why = player && !string.IsNullOrEmpty(player.CrashReason) ? "Lý do: " + player.CrashReason.ToLowerInvariant() : null;
            float extra = why != null ? 26 : 0;
            float x = mW - 147 - 500 - MSR, y = 190 + oy - extra / 2;
            var r = new Rect(x, y, 500, 390 + extra);
            SkewPanel(r, MBlue, 11);
            Skew(r.center.y);
            float ix = x + 43;
            EnsureHudIcons();
            Ico(sirenTex, ix, y + 28, 31, new Color32(255, 123, 110, 255));
            TxtL("GờTa Việt · TẠM GIỮ", ix + 41, y + 44, 16, new Color32(255, 123, 110, 255));
            TxtL("ĐÃ BỊ BẮT", ix - 3, y + 112, 74, MCream);
            TxtL(EndReason ?? "CSGT đã đưa bạn vào phòng giam.", ix, y + 168, 21, new Color(MCream.r, MCream.g, MCream.b, .8f), 440);
            if (why != null) TxtL(why, ix, y + 194, 15, new Color(MCream.r, MCream.g, MCream.b, .6f), 440);
            float sy = y + 204 + extra;
            TxtL("QUÃNG ĐƯỜNG", ix, sy + 8, 13, MGold);
            TxtL(Metres(Score), ix - 2, sy + 44, 42, MGold);
            float c2 = ix + Mathf.Max(TextW("QUÃNG ĐƯỜNG", 13), TextW(Metres(Score), 42)) + 24;
            Rect0(new Rect(c2, sy + 6, 3, 56), new Color(MCream.r, MCream.g, MCream.b, .2f));
            TxtL(NewBest ? "KỶ LỤC MỚI!" : "KỶ LỤC", c2 + 27, sy + 8, 13, MGold);
            TxtL(Metres(Best), c2 + 25, sy + 44, 42, MCream);
            Unskew();
            if (SkewButton(new Rect(x + 24, y + 306 + extra, 450, 82), "CHƠI LẠI", 28, MBtn.Red, retryTex, 32)) { StartRun(); return; }
            if (SkewButton(new Rect(x + 8, y + 416 + extra, 450, 60), "VỀ ĐẦU PHỐ", 20, MBtn.Dark, homeTex, 23)) GoReady();
        }

        void Tape(Rect r, float deg, string a, string b)
        {
            GUI.matrix = menuM * RotZ(r.center, deg);
            Rect0(new Rect(r.x, r.y + 7, r.width, r.height), new Color(0, 0, 0, .35f));
            Tile(r, hazardTex, 72f);
            var inner = new Rect(r.x, r.y + 8, r.width, r.height - 16);
            Rect0(inner, MGold);
            TxtC(a, new Rect(r.x, inner.y, r.width * .5f, inner.height), 21, MNavy);
            TxtC(b, new Rect(r.center.x, inner.y, r.width * .5f, inner.height), 21, MNavy);
            Unskew();
        }

        // ---------------------------------------------------------------- texture tạo trong mã
        void EnsureMenuArt()
        {
            if (gradV) return;
            gradV = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            for (int y = 0; y < 64; y++) { float t = Mathf.Lerp(.83f, 1f, y / 63f); gradV.SetPixel(0, y, new Color(t, t, t, 1)); }
            gradV.Apply(false, true);
            gradBand = new Texture2D(64, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            for (int x = 0; x < 64; x++) { float u = x / 63f; gradBand.SetPixel(x, 0, new Color(1, 1, 1, u < .55f ? 1f : 1f - (u - .55f) / .45f)); }
            gradBand.Apply(false, true);
            gradRow = new Texture2D(64, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            for (int x = 0; x < 64; x++) gradRow.SetPixel(x, 0, new Color(1, 1, 1, Mathf.Lerp(1f, .21f, x / 63f)));
            gradRow.Apply(false, true);
            checkerTex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave };
            checkerTex.SetPixels(new[] { MCream, MNavy, MNavy, MCream }); checkerTex.Apply(false, true);
            hazardTex = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };
            var hp = new Color[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) hp[y * 64 + x] = ((x + y) & 63) < 32 ? MGold : MNavy;
            hazardTex.SetPixels(hp); hazardTex.Apply(false, true);

            chevTex = MaskTex((u, v) => { float d = Mathf.Abs(v - .5f); return d <= .4f && u >= .5f - d && u <= .85f - d; });
            retryTex = MaskTex((u, v) =>
            {
                float dx = u - .5f, dy = .5f - v, d = Mathf.Sqrt(dx * dx + dy * dy), a = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                if (d > .25f && d < .37f && !(a > 18f && a < 78f)) return true;
                return InTri(u, v, new Vector2(.5f, .02f), new Vector2(.5f, .38f), new Vector2(.8f, .2f));
            });
            homeTex = MaskTex((u, v) =>
            {
                if (InTri(u, v, new Vector2(.06f, .5f), new Vector2(.5f, .08f), new Vector2(.94f, .5f))) return true;
                return u > .2f && u < .8f && v > .46f && v < .92f && !(u > .41f && u < .59f && v > .62f);
            });
            checkTex = MaskTex((u, v) => SegD(u, v, .14f, .52f, .4f, .78f) < .085f || SegD(u, v, .4f, .78f, .88f, .24f) < .085f);
            closeTex = MaskTex((u, v) => SegD(u, v, .18f, .18f, .82f, .82f) < .085f || SegD(u, v, .82f, .18f, .18f, .82f) < .085f);
            boomTex = MaskTex((u, v) =>
            {
                float dx = u - .5f, dy = v - .5f, d = Mathf.Sqrt(dx * dx + dy * dy), a = Mathf.Atan2(dy, dx);
                float t = Mathf.Repeat(a / (Mathf.PI * 2f) * 12f, 1f), rad = Mathf.Lerp(.27f, .49f, 1f - Mathf.Abs(t - .5f) * 2f);
                return d < rad;
            });
            hintBtns = new Texture2D[HintBtnNames.Length];
            for (int i = 0; i < HintBtnNames.Length; i++) hintBtns[i] = Resources.Load<Texture2D>("TouchControls/PV_Btn_" + HintBtnNames[i]);
        }

        static float SegD(float px, float py, float ax, float ay, float bx, float by)
        {
            float vx = bx - ax, vy = by - ay, t = Mathf.Clamp01(((px - ax) * vx + (py - ay) * vy) / (vx * vx + vy * vy));
            float dx = px - (ax + t * vx), dy = py - (ay + t * vy);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
