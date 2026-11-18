using UnityEngine;

namespace PhoViet.OpenWorld
{
    // Nhận va chạm của thân xe NPC (kinematic) và chuyển cho bộ điều khiển giao thông.
    public class OpenWorldNpcBody : MonoBehaviour
    {
        public OpenWorldTraffic owner; public int index;
        void OnCollisionEnter(Collision c) { if (owner) owner.OnHit(index, c); }
    }
}
