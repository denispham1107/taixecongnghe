using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.261: vùng chạm (trigger) của một tấm hàng rào sắt trước cửa hàng — xe chạm vào thì báo cho OpenWorldFences húc đổ.
    public class OpenWorldFencePanel : MonoBehaviour
    {
        [System.NonSerialized] public OpenWorldFences owner;
        [System.NonSerialized] public int run, seg;
        void OnTriggerEnter(Collider other) { if (owner) owner.Touch(this, other); }
    }
}
