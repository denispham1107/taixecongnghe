using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoViet.Editor
{
    // 21.277 — gói dữ liệu điện thoại (ETC2) nhẹ hơn: trong lúc build gói này (Active = true, chỉ do PhoVietWebBuild bật),
    // bỏ nội thất 3D tầng trệt (con «3D» của mọi «NoiThat»), LODGroup chỉ còn tấm ảnh xa → mesh nội thất 3D không vào gói
    // điện thoại. Chỉ sửa bản scene trong quá trình build, không đổi file scene. Gói máy tính giữ đủ 3D.
    // Lý do: 1.76.0 làm Safari iPhone/iPad sập («Đã có sự cố xảy ra liên tục») vì quá bộ nhớ.
    public sealed class PhoVietMobileSceneStrip : IProcessSceneWithReport
    {
        public static bool Active;
        public static int LastStripped { get; private set; }
        public int callbackOrder => 0;

        // 21.279: móc kiểm thử (build chia đôi đo bộ nhớ) — chỉ đặt từ script kiểm thử, null khi build phát hành.
        public static System.Action<Scene> Extra;
        // 21.279: số lần scene thật sự được xử lý trong build — build tăng dần có thể dùng lại scene đã xử lý từ lần trước
        // (không gọi OnProcessScene) → bản phát hành build sạch (CleanBuildCache) và kiểm tra số này.
        public static int Processed;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report != null) Processed++;
            if (report != null && Extra != null) Extra(scene);
            if (!Active || report == null) return;
            int n = 0;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var lg in root.GetComponentsInChildren<LODGroup>(true))
                {
                    if (lg.name != "NoiThat") continue;
                    var t3 = lg.transform.Find("3D"); var tx = lg.transform.Find("Xa");
                    if (!t3 || !tx) continue;
                    var card = tx.GetComponent<Renderer>();
                    lg.SetLODs(new[] { new LOD(0.02f, new Renderer[] { card }) });
                    Object.DestroyImmediate(t3.gameObject);
                    n++;
                }
            LastStripped = n;
            Debug.Log("[Phố Việt] gói điện thoại: bỏ " + n + " nội thất 3D (giữ ảnh xa)");
        }
    }
}
