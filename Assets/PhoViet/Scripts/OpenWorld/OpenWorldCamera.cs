using UnityEngine;

namespace PhoViet.OpenWorld
{
    // Camera bám sau xe, mượt.
    public class OpenWorldCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 9.5f, height = 4.2f, lookAhead = 6f, follow = 6f;
        void LateUpdate()
        {
            if (!target) return;
            var fwd = target.forward; fwd.y = 0; fwd.Normalize();
            var want = target.position - fwd * distance + Vector3.up * height;
            transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-follow * Time.deltaTime));
            transform.rotation = Quaternion.LookRotation(target.position + fwd * lookAhead + Vector3.up * 1.2f - transform.position, Vector3.up);
        }
    }
}
