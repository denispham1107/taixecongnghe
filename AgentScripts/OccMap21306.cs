using System.IO;
using UnityEngine;
// 21.306 — bản đồ phân loại ô 2 m (từ trên xuống) để tìm chỗ khu phố hở (thiếu nhà phố, xe chạy vào trong được):
// đen = nước/không có gì, đỏ = nhà (va chạm cao > 2,5 m), xanh lam = vỉa hè (MatLe), xám = mặt đất/đường/sân trong.
public static class OccMap21306
{
    public static string Main(float cell)
    {
        float x0 = -570f, z0 = -570f, size = 1272f; int n = Mathf.CeilToInt(size / cell);
        var t = new Texture2D(n, n, TextureFormat.RGB24, false); int mask = ~(1 << 2);
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                float x = x0 + (i + .5f) * cell, z = z0 + (j + .5f) * cell; Color c = Color.black;
                if (Physics.Raycast(new Vector3(x, 80f, z), Vector3.down, out var h, 85f, mask, QueryTriggerInteraction.Ignore))
                {
                    if (h.point.y > 2.5f) c = new Color(.8f, .2f, .2f);
                    else if (h.collider.gameObject.layer == 28) c = new Color(.3f, .5f, 1f);
                    else if (h.point.y > -1f) c = new Color(.6f, .6f, .6f);
                }
                t.SetPixel(i, j, c);
            }
        t.Apply(); File.WriteAllBytes("Captures/occmap21306.png", t.EncodeToPNG()); Object.DestroyImmediate(t);
        return $"Captures/occmap21306.png {n}px, x0={x0} z0={z0} cell={cell}";
    }
}
