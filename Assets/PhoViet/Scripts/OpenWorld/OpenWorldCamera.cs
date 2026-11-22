using UnityEngine;

namespace PhoViet.OpenWorld
{
    // Camera bám sau xe, mượt.
    // 21.316 (người dùng: đổi hướng chưa mượt): trước đây vị trí camera có đuổi mượt nhưng HƯỚNG nhìn bám ngay theo đầu xe nên mỗi lần
    // đánh lái cả màn hình xoay giật. Nay hướng camera (heading) đuổi theo đầu xe trễ ~headingLag (0,25 s, hàm mũ; 21.316b: 0,45 s, theo hướng chạy) — vào cua thấy thân
    // xe đổi hướng trước rồi camera mới xoay theo. Xe bị dịch chuyển/đặt lại (nhảy > 25 m hoặc lệch hướng > 120°) thì camera chụp ngay.
    public class OpenWorldCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 9.5f, height = 4.2f, lookAhead = 6f, follow = 6f;
        [Tooltip("21.316: thời gian trễ (s) để hướng camera bắt kịp hướng chạy của xe")] public float headingLag = 0.6f;   // 21.316b: 0,25 → 0,45; 21.316c: 0,6
        Vector3 heading, lastTarget; bool hasHeading; OpenWorldDriver drv;

        void LateUpdate()
        {
            if (!target) return;
            var fwd = target.forward; fwd.y = 0;
            // 21.316b: bám theo hướng CHẠY (OpenWorldDriver.MoveDir) — thân xe chúc vào cua thấy rõ trên màn hình, khung cảnh xoay ít hơn
            if (!drv || drv.transform != target) drv = target.GetComponent<OpenWorldDriver>();
            if (drv && !drv.Crashed && drv.Speed > 2f && drv.MoveDir.sqrMagnitude > 0.5f) fwd = drv.MoveDir;
            if (fwd.sqrMagnitude < 1e-4f) fwd = hasHeading ? heading : Vector3.forward;   // xe lật chúc thẳng đứng
            fwd.Normalize();
            bool jump = !hasHeading || (target.position - lastTarget).sqrMagnitude > 25f * 25f || Vector3.Angle(heading, fwd) > 120f;
            lastTarget = target.position;
            if (jump) { heading = fwd; hasHeading = true; }
            else
            {
                heading = Vector3.Slerp(heading, fwd, 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.01f, headingLag)));
                heading.y = 0; heading.Normalize();
            }
            var want = target.position - heading * distance + Vector3.up * height;
            transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-follow * Time.deltaTime));
            transform.rotation = Quaternion.LookRotation(target.position + heading * lookAhead + Vector3.up * 1.2f - transform.position, Vector3.up);
        }
    }
}
