using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.255: đánh dấu collider thân khủng long / xe tăng — xe người chơi đâm vào là xe hỏng (OpenWorldDriver.Crash).
    public class OpenWorldHazardBody : MonoBehaviour
    {
        public string reason = "ĐÂM XE TĂNG";
    }
}
