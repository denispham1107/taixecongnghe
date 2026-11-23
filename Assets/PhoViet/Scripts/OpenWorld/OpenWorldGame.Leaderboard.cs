namespace PhoViet.OpenWorld
{
    // 21.356: bảng xếp hạng chung (Firebase «gotaviet»). Hết lượt (bị bắt / kết thúc chuyến) → báo số mét cho trang web
    // (jslib GotaRunEnded → window.gotaRunEnded trong account.js). Trang web ghi kỷ lục lên Firestore nếu đã đăng nhập,
    // khách thì giữ chờ đăng nhập; game không biết gì về tài khoản.
    public partial class OpenWorldGame
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void GotaRunEnded(int metres, int best);
#else
        static void GotaRunEnded(int metres, int best) { }
#endif
        void ReportRunToWeb(int metres)
        {
            try { GotaRunEnded(metres, Best); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("GotaRunEnded: " + e.Message); }
        }
    }
}
