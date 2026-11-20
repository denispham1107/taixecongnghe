using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet.OpenWorld
{
    // 21.291 — thông báo trong game dạng thẻ (người dùng 21/11/2026: «thiết kế lại các câu thông báo cho đẹp mắt hơn nhưng
    // vẫn rõ ràng dễ đọc, cỡ chữ đang hơi to»). Thay chữ trơn cỡ Screen.height/26 có bóng đen bằng thẻ kính tối bo tròn:
    //   [vòng màu + biểu tượng]  NHÃN NHỎ (màu theo loại)
    //                            Tiêu đề (chữ thường có dấu, cỡ 22 đơn vị HUD)
    //                            Dòng phụ (cỡ 14)              [thanh thời gian mảnh dưới đáy nếu có]
    // Bốn loại: Info xanh dương (tên lửa khóa mục tiêu), Good xanh lá (hạ địch, nhặt đồ), Warn cam (địch phía trước, hố),
    // Danger đỏ (đạn/tên lửa lao tới, CSGT áp sát, xe hỏng). Trượt xuống + hiện dần 0,2 s, mờ dần 0,25 s cuối.
    // Cùng thang đo với HUD OpenWorldGame (1440×900 đơn vị ngang, 480 đơn vị dọc) nên cỡ chữ đi theo HUD khi đổi độ phân giải.
    // Thẻ tự xếp chồng dưới thanh HUD: mỗi lần vẽ trong cùng một sự kiện GUI chiếm một dòng (Stack).
    public static class OpenWorldToast
    {
        public enum Kind { Info, Good, Warn, Danger }
        public enum Icon { Auto, Alert, Check, Target, Plus, None }

        public static Color Accent(Kind k) =>
            k == Kind.Good ? new Color(.30f, .80f, .50f) : k == Kind.Warn ? new Color(1f, .70f, .22f) :
            k == Kind.Danger ? new Color(1f, .33f, .29f) : new Color(.36f, .66f, 1f);
        public static string DefaultCaption(Kind k) =>
            k == Kind.Good ? "TUYỆT VỜI" : k == Kind.Warn ? "CẢNH BÁO" : k == Kind.Danger ? "NGUY HIỂM" : "THÔNG TIN";

        static readonly Color Card = new Color(.05f, .11f, .15f, .9f), Detail = new Color(.78f, .87f, .89f);
        static GUIStyle titleSt, detailSt, capSt, badgeSt;

        // Thang đo giống OpenWorldGame.OnGUI.
        public static float Scale
        {
            get
            {
                bool portrait = Screen.width < Screen.height;
                float s = portrait ? Screen.width / 480f : Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
                return Mathf.Max(.35f, s);
            }
        }

        // ---- xếp chồng: dòng tiếp theo trong sự kiện GUI hiện tại
        static int stackFrame = -1; static EventType stackEvent; static float stackY;
        static float NextY(float h)
        {
            var e = Event.current != null ? Event.current.type : EventType.Repaint;
            if (stackFrame != Time.frameCount || stackEvent != e)
            {
                stackFrame = Time.frameCount; stackEvent = e;
                float sc = Scale; bool portrait = Screen.width < Screen.height;
                float top = OpenWorldGame.SafeTopPx / sc;
                stackY = top + (portrait ? 140f : 108f);                     // ngay dưới thanh HUD (+ hàng tên lửa khi cầm dọc)
            }
            float y = stackY; stackY += h + 10f; return y;
        }

        static void Init()
        {
            if (titleSt != null) return;
            var f = GUI.skin.font;
            titleSt = new GUIStyle { font = f, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Overflow };
            detailSt = new GUIStyle(titleSt) { fontStyle = FontStyle.Normal };
            capSt = new GUIStyle(titleSt);
            badgeSt = new GUIStyle(titleSt) { alignment = TextAnchor.MiddleCenter };
        }

        static Color A(Color c, float a) { c.a *= a; return c; }
        static void Fill(Rect r, Color c, float radius) => GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, c, 0, radius);
        static void Border(Rect r, Color c, float w, float radius) => GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, c, w, radius);
        static void Text(string s, Rect r, GUIStyle st, int size, Color c) { st.fontSize = size; st.normal.textColor = c; GUI.Label(r, s, st); }
        static float Width(string s, GUIStyle st, int size) { if (string.IsNullOrEmpty(s)) return 0; st.fontSize = size; return st.CalcSize(new GUIContent(s)).x; }

        /// <summary>Thẻ thông báo xếp dưới thanh HUD.</summary>
        /// <param name="age">giây từ lúc hiện (hiệu ứng vào)</param>
        /// <param name="remaining">giây còn lại (hiệu ứng ra); &lt;0 = không tự mờ</param>
        /// <param name="badge">chữ/số trong vòng tròn thay biểu tượng (vd. đếm ngược)</param>
        /// <param name="progress">0–1 vẽ thanh thời gian dưới đáy; &lt;0 = không vẽ</param>
        /// <param name="accentOverride">màu nhấn riêng (vd. đèn CSGT nháy đỏ/xanh)</param>
        public static void Show(string caption, string title, string detail, Kind kind, float age, float remaining,
                                string badge = null, float progress = -1f, Color? accentOverride = null, Icon icon = Icon.Auto, bool center = false)
        {
            if (string.IsNullOrEmpty(title)) return;
            Init();
            float sc = Scale, uiW = Screen.width / sc, uiH = Screen.height / sc;
            var keep = GUI.matrix; GUI.matrix = Matrix4x4.Scale(Vector3.one * sc);

            float big = center ? 1.18f : 1f;
            int capSize = Mathf.RoundToInt(12 * big), titleSize = Mathf.RoundToInt(22 * big), detailSize = Mathf.RoundToInt(14 * big);
            caption = string.IsNullOrEmpty(caption) ? DefaultCaption(kind) : caption;
            float iconD = 44 * big, padL = 14 * big, gap = 14 * big, padR = 22 * big;
            float textW = Mathf.Max(Width(caption, capSt, capSize), Width(title, titleSt, titleSize), Width(detail, detailSt, detailSize));
            float w = Mathf.Min(padL + iconD + gap + textW + padR, uiW - 32);
            float capH = 16 * big, titleH = 28 * big, detH = string.IsNullOrEmpty(detail) ? 0 : 20 * big;
            float h = Mathf.Max(iconD + 20 * big, 12 * big + capH + titleH + detH + 12 * big);

            float aIn = Mathf.Clamp01(age / .2f), aOut = remaining < 0 ? 1f : Mathf.Clamp01(remaining / .25f), alpha = aIn * aOut;
            float ease = 1f - (1f - aIn) * (1f - aIn);
            float y = center ? uiH * .36f - h / 2 : NextY(h);
            var r = new Rect((uiW - w) / 2, y - (1f - ease) * 14f, w, h);
            var acc = accentOverride ?? Accent(kind);

            Fill(new Rect(r.x - 2, r.y + 5, r.width + 4, r.height + 2), A(new Color(0, 0, 0, .28f), alpha), 20 * big);   // bóng đổ
            Fill(r, A(Card, alpha), 18 * big);
            Border(r, A(new Color(acc.r, acc.g, acc.b, .55f), alpha), 1.5f, 18 * big);
            Fill(new Rect(r.x + 18 * big, r.y, r.width - 36 * big, 2.5f), A(acc, .85f * alpha), 1.2f);              // vạch sáng mép trên

            // vòng biểu tượng (nguy hiểm: nhịp đập)
            float pulse = kind == Kind.Danger ? 1f + .07f * Mathf.Sin(Time.unscaledTime * 10f) : 1f;
            float d = iconD * pulse; var ic = new Rect(r.x + padL + (iconD - d) / 2, r.center.y - d / 2, d, d);
            Fill(new Rect(ic.x - 4, ic.y - 4, ic.width + 8, ic.height + 8), A(new Color(acc.r, acc.g, acc.b, .22f), alpha), (d + 8) / 2);
            Fill(ic, A(acc, alpha), d / 2);
            var ink = A(new Color(.06f, .10f, .13f), alpha);
            if (!string.IsNullOrEmpty(badge)) Text(badge, ic, badgeSt, Mathf.RoundToInt((badge.Length > 2 ? 15 : 21) * big), ink);
            else DrawIcon(icon == Icon.Auto ? (kind == Kind.Good ? Icon.Check : kind == Kind.Info ? Icon.Target : Icon.Alert) : icon, ic, ink);

            float tx = r.x + padL + iconD + gap, ty = r.y + (h - (capH + titleH + detH)) / 2, tw = r.xMax - tx - padR * .5f;
            Text(caption, new Rect(tx, ty, tw, capH), capSt, capSize, A(acc, alpha));
            Text(title, new Rect(tx, ty + capH, tw, titleH), titleSt, titleSize, A(Cream, alpha));
            if (detH > 0) Text(detail, new Rect(tx, ty + capH + titleH, tw, detH), detailSt, detailSize, A(Detail, alpha));

            if (progress >= 0f)
            {
                var bar = new Rect(r.x + 18 * big, r.yMax - 6, r.width - 36 * big, 3);
                Fill(bar, A(new Color(1, 1, 1, .14f), alpha), 1.5f);
                Fill(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(progress), bar.height), A(acc, alpha), 1.5f);
            }
            GUI.matrix = keep;
        }

        // Nhãn nhỏ dạng viên thuốc (mũi chỉ hướng CSGT ngoài khung hình). x, y theo điểm ảnh màn hình (tâm nhãn).
        public static void Chip(float px, float py, string text, Color dot)
        {
            Init();
            float sc = Scale; var keep = GUI.matrix; GUI.matrix = Matrix4x4.Scale(Vector3.one * sc);
            float w = Width(text, titleSt, 15) + 46, h = 34, x = px / sc - w / 2, y = py / sc - h / 2;
            x = Mathf.Clamp(x, 8, Screen.width / sc - w - 8);
            var r = new Rect(x, y, w, h);
            Fill(new Rect(r.x - 1, r.y + 3, r.width + 2, r.height), new Color(0, 0, 0, .25f), h / 2);
            Fill(r, Card, h / 2);
            Border(r, new Color(dot.r, dot.g, dot.b, .7f), 1.5f, h / 2);
            Fill(new Rect(r.x + 13, r.center.y - 6, 12, 12), dot, 6);
            Text(text, new Rect(r.x + 32, r.y, r.width - 38, h), titleSt, 15, Cream);
            GUI.matrix = keep;
        }

        // Xoay quanh p trong hệ toạ độ GUI hiện tại (RotateAroundPivot lệch tâm khi GUI.matrix đang co giãn).
        static Matrix4x4 Rot(Vector2 p, float deg) => Matrix4x4.Translate(p) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, deg)) * Matrix4x4.Translate(-p);

        // Biểu tượng vẽ bằng hình khối (font web không chắc có ✓, ⚠).
        static void DrawIcon(Icon icon, Rect c, Color ink)
        {
            float s = c.width; var m = c.center;
            switch (icon)
            {
                case Icon.Alert:                                                // dấu chấm than
                    Fill(new Rect(m.x - s * .06f, m.y - s * .28f, s * .12f, s * .36f), ink, s * .06f);
                    Fill(new Rect(m.x - s * .07f, m.y + s * .15f, s * .14f, s * .14f), ink, s * .07f);
                    break;
                case Icon.Plus:
                    Fill(new Rect(m.x - s * .06f, m.y - s * .25f, s * .12f, s * .5f), ink, s * .05f);
                    Fill(new Rect(m.x - s * .25f, m.y - s * .06f, s * .5f, s * .12f), ink, s * .05f);
                    break;
                case Icon.Check:                                                // dấu tích = hai thanh xoay
                {
                    var keep = GUI.matrix;
                    var p1 = new Vector2(m.x - s * .12f, m.y + s * .13f);
                    GUI.matrix = keep * Rot(p1, 45f);
                    Fill(new Rect(p1.x - s * .2f, p1.y - s * .055f, s * .25f, s * .11f), ink, s * .05f);
                    GUI.matrix = keep;
                    GUI.matrix = keep * Rot(p1, -50f);
                    Fill(new Rect(p1.x - s * .05f, p1.y - s * .055f, s * .44f, s * .11f), ink, s * .05f);
                    GUI.matrix = keep;
                    break;
                }
                case Icon.Target:                                               // tâm ngắm
                    Border(new Rect(m.x - s * .24f, m.y - s * .24f, s * .48f, s * .48f), ink, s * .07f, s * .24f);
                    Fill(new Rect(m.x - s * .06f, m.y - s * .06f, s * .12f, s * .12f), ink, s * .06f);
                    Fill(new Rect(m.x - s * .035f, m.y - s * .38f, s * .07f, s * .14f), ink, 0);
                    Fill(new Rect(m.x - s * .035f, m.y + s * .24f, s * .07f, s * .14f), ink, 0);
                    Fill(new Rect(m.x - s * .38f, m.y - s * .035f, s * .14f, s * .07f), ink, 0);
                    Fill(new Rect(m.x + s * .24f, m.y - s * .035f, s * .14f, s * .07f), ink, 0);
                    break;
            }
        }
    }
}
