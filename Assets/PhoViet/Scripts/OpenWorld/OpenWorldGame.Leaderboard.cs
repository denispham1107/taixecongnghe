namespace PhoViet.OpenWorld
{
    // 21.356: bảng xếp hạng chung (Firebase «gotaviet»). Hết lượt (bị bắt / kết thúc chuyến) → báo số mét cho trang web
    // (jslib GotaRunEnded → window.gotaRunEnded trong account.js). Trang web ghi kỷ lục lên Firestore nếu đã đăng nhập,
    // khách thì giữ chờ đăng nhập; game không biết gì về tài khoản (đăng nhập/đăng ký chỉ ở trang chủ web).
    // 21.375: vào lượt mới → jslib GotaRunStarted → window.gotaRunStarted: trang tắt thông báo xếp hạng còn đang hiện và không
    // hiện thông báo nào trong lúc đang chơi (người dùng: «khi đang chơi không cho xuất hiện thông báo bảng xếp hạng»).
    // 21.357: ô «Bảng xếp hạng» trên màn chào (MenuRanks) → jslib GotaOpenRanks → window.gotaOpenRanks mở hộp Top 100 của trang.
    public partial class OpenWorldGame
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void GotaRunStarted();
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void GotaRunEnded(int metres, int best);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void GotaOpenRanks();
#else
        static void GotaRunStarted() { }
        static void GotaRunEnded(int metres, int best) { }
        static void GotaOpenRanks() { UnityEngine.Debug.Log("GotaOpenRanks (chỉ có trên bản web)"); }
#endif
        [System.NonSerialized] public int runStartsReported;            // kiểm thử trong Editor
        void ReportRunStartToWeb()
        {
            runStartsReported++;
            try { GotaRunStarted(); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("GotaRunStarted: " + e.Message); }
        }

        void ReportRunToWeb(int metres)
        {
            try { GotaRunEnded(metres, Best); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("GotaRunEnded: " + e.Message); }
        }

        [System.NonSerialized] public int ranksOpened;                  // kiểm thử trong Editor
        void OpenRanksOnWeb()
        {
            ranksOpened++;
            try { GotaOpenRanks(); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("GotaOpenRanks: " + e.Message); }
        }
    }
}
