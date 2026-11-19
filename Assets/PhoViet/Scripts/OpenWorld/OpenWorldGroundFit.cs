using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.274 — xe không lún khi lên vỉa hè / lối ven sông.
    // Vỉa hè (OW_Duong_*_ViaHe) và lối ven sông (OW_Map_LoiVenSong) cao 0,15–0,17 m nhưng va chạm mặt đất chỉ có ở y = 0
    // (thêm bậc va chạm thật thì mép bó vỉa 15 cm sẽ chặn hộp va chạm của xe). Nên: MeshCollider của các mesh mặt lề nằm ở
    // layer MatLe (28) — vật lý bỏ qua va chạm với mọi layer, WorldMask cũng bỏ qua — chỉ dùng để dò độ cao. Mỗi khung, với mọi
    // xe mặt đất (người chơi, CSGT, xe NPC, xe tăng): dò mặt lề dưới tâm xe; nếu cao hơn gốc xe (0–0,35 m) thì nâng các con
    // trực tiếp của xe (mô hình, đèn, khói) lên đúng chừng đó, mượt 2 m/s (như bánh leo lên bó vỉa).
    [DefaultExecutionOrder(900)]
    public sealed class OpenWorldGroundFit : MonoBehaviour
    {
        public const int SurfaceLayer = 28;
        public float maxLift = 0.35f, liftSpeed = 2f;

        sealed class Entry { public Transform root; public float lift, applied; public readonly Dictionary<Transform, Vector3> baseLocal = new Dictionary<Transform, Vector3>(); }
        readonly Dictionary<Transform, Entry> entries = new Dictionary<Transform, Entry>();
        readonly List<Transform> dead = new List<Transform>();
        float nextScan;
        OpenWorldTraffic traffic;

        public static int Tracked { get; private set; }
        public float LiftOf(Transform root) => root && entries.TryGetValue(root, out var e) ? e.lift : 0f;

        void Awake()
        {
            for (int i = 0; i < 32; i++) Physics.IgnoreLayerCollision(SurfaceLayer, i, true);
        }

        void Scan()
        {
            if (!traffic) traffic = FindAnyObjectByType<OpenWorldTraffic>();
            foreach (var rb in FindObjectsByType<Rigidbody>(FindObjectsSortMode.None))
            {
                var t = rb.transform; if (entries.ContainsKey(t)) continue;
                bool vehicle = rb.GetComponent<OpenWorldDriver>() || rb.GetComponent<OpenWorldPolice>()
                               || (traffic && t.parent == traffic.transform) || t.name.StartsWith("XeTang");
                if (vehicle) entries[t] = new Entry { root = t };
            }
            Tracked = entries.Count;
        }

        public static float SurfaceAt(Vector3 p, float above = 0.6f, float down = 1.2f)
        {
            return Physics.Raycast(p + Vector3.up * above, Vector3.down, out var h, above + down, 1 << SurfaceLayer, QueryTriggerInteraction.Ignore)
                ? h.point.y : float.NegativeInfinity;
        }

        void LateUpdate()
        {
            if (Time.time >= nextScan) { nextScan = Time.time + 1f; Scan(); }
            float dt = Time.deltaTime;
            dead.Clear();
            foreach (var kv in entries)
            {
                var e = kv.Value; var t = e.root;
                if (!t) { dead.Add(kv.Key); continue; }
                float target = 0f;
                if (t.gameObject.activeInHierarchy && t.up.y > 0.8f)
                {
                    float s = SurfaceAt(t.position);
                    float d = s - t.position.y;
                    if (d > 0.005f && d < maxLift) target = d;
                }
                e.lift = Mathf.MoveTowards(e.lift, target, liftSpeed * dt);
                if (e.lift == e.applied && t.childCount == e.baseLocal.Count) continue;     // không đổi → khỏi ghi
                e.applied = e.lift;
                var upLocal = t.InverseTransformDirection(Vector3.up) / Mathf.Max(1e-4f, t.lossyScale.y);
                for (int i = 0; i < t.childCount; i++)
                {
                    var c = t.GetChild(i);
                    if (!e.baseLocal.TryGetValue(c, out var b)) { b = c.localPosition; e.baseLocal[c] = b; }
                    c.localPosition = b + upLocal * e.lift;
                }
            }
            foreach (var k in dead) entries.Remove(k);
        }
    }
}
