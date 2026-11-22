using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet.OpenWorld
{
    // 21.291 — cấu hình đồ họa bản đồ mở (người dùng 21/11/2026: «độ phân giải trên điện thoại quá thấp, xe cộ mờ nhòe»).
    // Nút bánh răng ở góc phải trên thẻ chào «LÁI QUA PHỐ QUEN.» mở cửa sổ 4 mức, lưu PlayerPrefs (GfxKey) cho lần sau:
    //   Rất cao    — mật độ điểm ảnh GỐC của màn hình (iPhone 3x, tối đa 3,8 triệu điểm ảnh trên máy cảm ứng), cố định.
    //                Chống văng mà vẫn giữ độ phân giải: không MSAA (mật độ điểm ảnh gốc đã mịn răng cưa — bộ đệm khung MSAA 4×
    //                ở 3 triệu điểm ảnh tốn thêm ~90 MB), bóng 1 tầng (cascade), 1 đèn điểm ảnh, không lọc dị hướng; giữ
    //                nguyên mọi nội dung (xe NPC, tàu, nhà, cây), texture, tầm nhìn.
    //   Cao        — 80 % mật độ gốc (iPhone 2,4x), cố định, bóng như Rất cao.
    //   Trung bình — tự động: bắt đầu 55 % mật độ gốc, đo thời gian khung khi đang chạy; chậm (trung vị > 25 ms hai lần liền)
    //                thì hạ 0,15x tới 40 % gốc rồi tắt bóng; mượt (< 19 ms) liên tục 12 s thì nâng lại tới 67 % gốc.
    //   Yếu        — mật độ 1x, tắt bóng, LOD gần hơn, không đèn điểm ảnh.
    // Mặc định: máy cảm ứng = Trung bình, máy tính = Rất cao. Mức không đổi số xe NPC/tàu/CSGT (người dùng chốt: không bớt nội
    // dung trên điện thoại). Mật độ điểm ảnh đặt qua jslib PhoVietSetPixelPlan → app.js phoVietSetPixelPlan (lưu localStorage
    // để lần mở sau tải đúng mật độ ngay từ đầu); app.js báo lại pixelRatio thực + nativeRatio qua ApplyBrowserInsets.
    public partial class OpenWorldGame
    {
        public enum Gfx { VeryHigh = 0, High = 1, Medium = 2, Low = 3 }
        public const string GfxKey = "PhoViet.OpenWorld.Graphics.v1";
        static readonly string[] GfxNames = { "Rất cao", "Cao", "Trung bình", "Yếu" };
        public static string GfxName(Gfx g) => GfxNames[(int)g];
        const long TouchBudget = 3800000, DesktopBudget = 8300000;

        public Gfx Graphics { get; private set; } = Gfx.VeryHigh;
        public bool GraphicsOpen { get; private set; }
        public float RequestedRatio { get; private set; }               // mật độ điểm ảnh đang yêu cầu (0 = chưa đặt)
        public bool AdaptiveShadowsOff { get; private set; }
        public int GovernorSteps { get; private set; }
        static float nativeRatio = 1f, reportedRatio = 1f;

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void PhoVietSetPixelPlan(float cap, float budget);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int PhoVietTouchDevice();
        static bool TouchDevice => UnityEngine.Device.Application.isMobilePlatform || PhoVietTouchDevice() != 0;
#else
        static void PhoVietSetPixelPlan(float cap, float budget) { }
        static bool TouchDevice => UnityEngine.Device.Application.isMobilePlatform;
#endif
        [System.NonSerialized] public bool forceTouchGraphics;          // kiểm thử trong Editor
        bool TouchGfx => forceTouchGraphics || TouchDevice;

        bool gfxInit; int baseAA, baseCascades, basePixelLights; float baseShadowDist, baseLod;
        ShadowQuality baseShadows; AnisotropicFiltering baseAniso;
        float adaptRatio; Texture2D gear;

        struct Plan { public float ratio, minRatio, maxRatio, lod, shadowDist; public bool shadows, adaptive; public int cascades, pixelLights; public long budget; }

        Plan PlanFor(Gfx g)
        {
            float n = Mathf.Max(1f, nativeRatio); bool t = TouchGfx;
            var p = new Plan
            {
                budget = t ? TouchBudget : DesktopBudget, lod = baseLod, shadows = baseShadows != ShadowQuality.Disable,
                shadowDist = baseShadowDist, cascades = t ? 1 : baseCascades, pixelLights = t ? Mathf.Min(basePixelLights, 1) : basePixelLights
            };
            switch (g)
            {
                case Gfx.VeryHigh: p.ratio = Mathf.Min(n, t ? 3f : 2f); break;
                case Gfx.High: p.ratio = Mathf.Max(1f, Mathf.Min(n * .8f, t ? 2.4f : 1.6f)); break;
                case Gfx.Medium:
                    p.adaptive = true; p.minRatio = Mathf.Max(1f, n * .4f); p.maxRatio = Mathf.Max(p.minRatio, Mathf.Min(n * .67f, 2f));
                    p.ratio = Mathf.Clamp(n * .55f, p.minRatio, p.maxRatio);
                    p.lod = baseLod * .85f; p.shadowDist = Mathf.Min(baseShadowDist, 30f); p.cascades = 1; break;
                default:
                    p.ratio = 1f; p.lod = baseLod * .6f; p.shadows = false; p.pixelLights = 0; p.cascades = 1; break;
            }
            return p;
        }

        void InitGraphics()
        {
            if (gfxInit) return;
            baseAA = QualitySettings.antiAliasing; baseCascades = QualitySettings.shadowCascades; basePixelLights = QualitySettings.pixelLightCount;
            baseShadowDist = QualitySettings.shadowDistance; baseLod = QualitySettings.lodBias; baseShadows = QualitySettings.shadows;
            baseAniso = QualitySettings.anisotropicFiltering;
            gfxInit = true;
            int stored = PlayerPrefs.GetInt(GfxKey, -1);
            Graphics = stored >= 0 && stored <= 3 ? (Gfx)stored : TouchGfx ? Gfx.Medium : Gfx.VeryHigh;
            ApplyGraphics(true);
        }

        void RestoreGraphicsBaseline()
        {
            if (!gfxInit) return;
            QualitySettings.antiAliasing = baseAA; QualitySettings.shadowCascades = baseCascades; QualitySettings.pixelLightCount = basePixelLights;
            QualitySettings.shadowDistance = baseShadowDist; QualitySettings.lodBias = baseLod; QualitySettings.shadows = baseShadows;
            QualitySettings.anisotropicFiltering = baseAniso;
        }

        void ApplyGraphics(bool resetAdaptive)
        {
            if (!gfxInit) return;
            var p = PlanFor(Graphics);
            if (resetAdaptive || !p.adaptive) { adaptRatio = p.ratio; AdaptiveShadowsOff = false; }
            float ratio = p.adaptive ? Mathf.Clamp(adaptRatio, p.minRatio, p.maxRatio) : p.ratio;
            bool shadows = p.shadows && !(p.adaptive && AdaptiveShadowsOff);
            RestoreGraphicsBaseline();
            // Máy cảm ứng: không MSAA ở mọi mức (mật độ điểm ảnh cao đã mịn cạnh; MSAA nhân bộ nhớ bộ đệm khung).
            if (TouchGfx) QualitySettings.antiAliasing = 0;
            QualitySettings.shadows = shadows ? baseShadows : ShadowQuality.Disable;
            QualitySettings.shadowDistance = shadows ? p.shadowDist : 0f;
            QualitySettings.shadowCascades = p.cascades;
            QualitySettings.lodBias = p.lod;
            QualitySettings.pixelLightCount = p.pixelLights;
            if (TouchGfx || Graphics == Gfx.Low) QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            RequestedRatio = ratio;
            PhoVietSetPixelPlan(ratio, p.budget);
            MatchHouseLods();
        }

        // 21.296 — người dùng 21/11/2026: «Play trong Unity thấy nhà phố xa đầy đủ chi tiết, trên điện thoại thì biến dạng».
        // Editor chạy mức chất lượng Ultra (lodBias 2); bản web dùng mức High (lodBias 1), điện thoại × 0,75, Trung bình × 0,85
        // (≈ 0,64) → nhà phố đổi sang hộp ảnh xa ở ~47 m thay vì ~150 m. Đặt lodBias 2 cho mọi thứ thì số tam giác gấp đôi
        // (1,56 → 3,15 triệu), nên chỉ nhóm nhà phố (nhà, nội thất qua kính, bảng hiệu) được chia ngưỡng LOD theo tỉ lệ
        // lodBias hiện tại / 2 → đổi LOD và ẩn đúng khoảng cách như Play trong Editor ở mọi cấu hình và mọi máy (+36 % tam giác).
        // Cây, cao ốc, cột đèn giữ lodBias của từng mức.
        const float EditorLodBias = 2f;                                  // ProjectSettings/QualitySettings: Ultra = mức Editor
        LODGroup[] houseLods; float[][] houseLodBase; float houseLodBias = -1f;

        static bool IsHouseLod(LODGroup lg)
        {
            if (lg.name == "NoiThat") return true;
            var p = lg.transform.parent; if (!p) return false;
            return p.name.StartsWith("NhaPho") || p.name.StartsWith("BangHieu");
        }

        void MatchHouseLods()
        {
            float bias = QualitySettings.lodBias;
            if (Mathf.Abs(bias - houseLodBias) < 1e-4f) return;
            if (houseLods == null)
            {
                var list = new System.Collections.Generic.List<LODGroup>();
                foreach (var lg in FindObjectsByType<LODGroup>(FindObjectsInactive.Include)) if (lg && IsHouseLod(lg)) list.Add(lg);
                houseLods = list.ToArray(); houseLodBase = new float[houseLods.Length][];
                for (int i = 0; i < houseLods.Length; i++)
                {
                    var lods = houseLods[i].GetLODs(); var b = new float[lods.Length];
                    for (int j = 0; j < lods.Length; j++) b[j] = lods[j].screenRelativeTransitionHeight;
                    houseLodBase[i] = b;
                }
            }
            float k = Mathf.Min(1f, bias / EditorLodBias);
            for (int i = 0; i < houseLods.Length; i++)
            {
                var lg = houseLods[i]; if (!lg) continue;
                var lods = lg.GetLODs(); var b = houseLodBase[i]; if (lods.Length != b.Length) continue;
                for (int j = 0; j < lods.Length; j++) lods[j].screenRelativeTransitionHeight = b[j] * k;
                lg.SetLODs(lods);
            }
            houseLodBias = bias;
        }

        public string HouseLodReport() => $"houseLods={(houseLods == null ? 0 : houseLods.Length)} bias={QualitySettings.lodBias:F3} k={Mathf.Min(1f, QualitySettings.lodBias / EditorLodBias):F3}";

        public void SetGraphics(int level)
        {
            if (level < 0 || level > 3) return;
            InitGraphics();
            Graphics = (Gfx)level; GovernorSteps = 0; ResetGovernor();
            ApplyGraphics(true);
            PlayerPrefs.SetInt(GfxKey, level); PlayerPrefs.Save();
        }

        // app.js báo mật độ thực/gốc (ApplyBrowserInsets). Mật độ gốc đổi (lần báo đầu, đổi màn hình) → tính lại mức.
        void OnBrowserRatios(float pixelRatio, float native)
        {
            if (pixelRatio > 0) reportedRatio = pixelRatio;
            if (native > 0 && Mathf.Abs(native - nativeRatio) > .01f) { nativeRatio = native; ApplyGraphics(true); }
        }

        // ---------------------------------------------------- tự điều chỉnh (chỉ mức Trung bình)
        const int GovWindow = 45; const float GovSlow = .025f, GovClean = .019f;
        readonly float[] govSamples = new float[GovWindow];
        int govCount, govSlowWindows; float govElapsed, govClean, govRaiseAfter = 12f, govLastRaise = -1000f, govLastChange = -1000f;
        void ResetGovernor() { govCount = 0; govElapsed = 0; govSlowWindows = 0; govClean = 0; govRaiseAfter = 12f; }

        void TickGovernor()
        {
            if (!gfxInit || Graphics != Gfx.Medium || GraphicsOpen || Time.timeScale <= 0f) { govCount = 0; govElapsed = 0; return; }
            float dt = Time.unscaledDeltaTime; if (dt > .4f) { govCount = 0; govElapsed = 0; return; }   // đổi tab / giật nạp một lần
            govSamples[govCount++] = dt; govElapsed += dt;
            if (govCount < GovWindow && (govElapsed < 1.5f || govCount < 8)) return;
            int n = govCount; float window = govElapsed; govCount = 0; govElapsed = 0;
            System.Array.Sort(govSamples, 0, n); float median = govSamples[n / 2];
            if (Time.unscaledTime - govLastChange < 2.5f) return;              // chờ khung hình ổn định sau lần đổi trước
            if (median > GovSlow)
            {
                govClean = 0;
                if (++govSlowWindows >= 2 || median > .05f) { govSlowWindows = 0; GovernorStep(-1); }
                return;
            }
            govSlowWindows = 0;
            if (median < GovClean) { govClean += window; if (govClean >= govRaiseAfter) { govClean = 0; GovernorStep(+1); } }
        }

        void GovernorStep(int dir)
        {
            var p = PlanFor(Gfx.Medium);
            if (dir < 0)
            {
                if (Time.unscaledTime - govLastRaise < 30f) govRaiseAfter = Mathf.Min(120f, govRaiseAfter * 2f);   // vừa nâng đã phải hạ: chờ lâu hơn
                if (adaptRatio > p.minRatio + .01f) adaptRatio = Mathf.Max(p.minRatio, adaptRatio - .15f);
                else if (!AdaptiveShadowsOff && p.shadows) AdaptiveShadowsOff = true;
                else return;
            }
            else
            {
                if (AdaptiveShadowsOff) AdaptiveShadowsOff = false;
                else if (adaptRatio < p.maxRatio - .01f) adaptRatio = Mathf.Min(p.maxRatio, adaptRatio + .15f);
                else return;
                govLastRaise = Time.unscaledTime;
            }
            GovernorSteps++; govLastChange = Time.unscaledTime;
            ApplyGraphics(false);
        }

        // ---------------------------------------------------- giao diện
        public void OpenGraphics() { if (Mode == State.Ready || Mode == State.Paused) GraphicsOpen = true; }
        public void CloseGraphics() => GraphicsOpen = false;

        void GraphicsKeys()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) CloseGraphics();
            else if (Input.GetKeyDown(KeyCode.Alpha1)) SetGraphics(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SetGraphics(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SetGraphics(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SetGraphics(3);
        }

        string ResolutionText(Gfx g)
        {
            var p = PlanFor(g);
            float cssW = Screen.width / Mathf.Max(.1f, reportedRatio), cssH = Screen.height / Mathf.Max(.1f, reportedRatio);
            float r = p.adaptive ? (Graphics == Gfx.Medium ? RequestedRatio : p.ratio) : p.ratio;
            r = Mathf.Min(r, Mathf.Sqrt(p.budget / Mathf.Max(1f, cssW * cssH)));
            if (Application.isEditor && nativeRatio <= 1.01f) r = p.ratio;     // Editor: không có khung trang báo mật độ
            return Mathf.RoundToInt(cssW * r) + " × " + Mathf.RoundToInt(cssH * r);
        }

        // 21.322: GraphicsWindow/GearButton/GearRect chuyển sang OpenWorldGame.Menus.cs (mẫu C).

        // Bánh răng 8 răng vẽ sẵn 96×96 (khử răng cưa 2×2), như nút cài đặt của phố cuộn cũ (PhoVietGraphicsSettings).
        void DrawGear(Rect rect, Color c)
        {
            if (!gear)
            {
                const int n = 96; var px = new Color[n * n];
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        float a = 0;
                        for (int sy = 0; sy < 2; sy++)
                            for (int sx = 0; sx < 2; sx++)
                            {
                                float u = (x + (sx + .5f) * .5f - n * .5f) / (n * .5f), v = (y + (sy + .5f) * .5f - n * .5f) / (n * .5f), rad = Mathf.Sqrt(u * u + v * v);
                                bool inside = rad < .7f;
                                for (int k = 0; k < 8 && !inside; k++)
                                {
                                    float an = k * Mathf.PI * .25f, along = u * Mathf.Cos(an) + v * Mathf.Sin(an), across = -u * Mathf.Sin(an) + v * Mathf.Cos(an);
                                    inside = along > .58f && along < .95f && Mathf.Abs(across) < .14f;
                                }
                                if (inside && rad > .3f) a += .25f;
                            }
                        px[y * n + x] = new Color(1, 1, 1, a);
                    }
                gear = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "OW gear", filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };
                gear.SetPixels(px); gear.Apply(false, true);
            }
            GUI.DrawTexture(rect, gear, ScaleMode.ScaleToFit, true, 0, c, 0, 0);
        }

        void OnDestroy()
        {
#if UNITY_EDITOR
            RestoreGraphicsBaseline();                                       // Play Mode không để lại chất lượng đã đổi cho Editor
#endif
            if (gear) Destroy(gear);
        }

        public string GraphicsReport() => $"gfx={Graphics} ratio={RequestedRatio:F2} native={nativeRatio:F2} reported={reportedRatio:F2} shadows={QualitySettings.shadows} dist={QualitySettings.shadowDistance:F0} casc={QualitySettings.shadowCascades} lod={QualitySettings.lodBias:F2} aa={QualitySettings.antiAliasing} steps={GovernorSteps} adaptShadowsOff={AdaptiveShadowsOff}";
    }
}
