using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

public static class WeatherCheck21317
{
    public static string Run()
    {
        var sb = new StringBuilder();
        var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Resources/ThoiTiet/PV_Set.fbx");
        var mesh = go.GetComponentsInChildren<MeshFilter>(true)[0].sharedMesh;
        var v = new List<Vector3>(); mesh.GetVertices(v);
        var u0 = new List<Vector2>(); var u1 = new List<Vector2>(); var u2 = new List<Vector2>(); var u3 = new List<Vector2>();
        mesh.GetUVs(0, u0); mesh.GetUVs(1, u1); mesh.GetUVs(2, u2); mesh.GetUVs(3, u3);
        sb.AppendLine($"readable={mesh.isReadable} n={v.Count} uv0={u0.Count} uv1={u1.Count} uv2={u2.Count} uv3={u3.Count}");
        var tri = mesh.triangles;
        // so hướng lưu trong UV với hiệu vị trí giữa hai đỉnh cùng cạnh dọc (side giống nhau, t khác nhau)
        float sumDot = 0; int cnt = 0;
        for (int i = 0; i < tri.Length && cnt < 400; i += 3)
            for (int k = 0; k < 3; k++)
            {
                int a = tri[i + k], b = tri[i + (k + 1) % 3];
                if (Mathf.Abs(u0[a].x - u0[b].x) > 0.1f || Mathf.Abs(u0[a].y - u0[b].y) < 1e-5f) continue;
                var d = (v[b] - v[a]) * Mathf.Sign(u0[b].y - u0[a].y);
                if (d.sqrMagnitude < 1e-6f) continue;
                var s = new Vector3(u1[a].x, u1[a].y, u2[a].x);
                sumDot += Vector3.Dot(d.normalized, s.normalized); cnt++;
            }
        sb.AppendLine($"mean dot(dir stored, dir actual)={sumDot / Mathf.Max(1, cnt):F3} over {cnt}");
        for (int i = 0; i < 4; i++) sb.AppendLine($"v{i} p={v[i]} uv0={u0[i]} uv1={u1[i]} uv2={u2[i]} uv3={u3[i]}");
        float tmax = 0; foreach (var q in u0) tmax = Mathf.Max(tmax, q.y);
        sb.AppendLine("tmax=" + tmax);
        return sb.ToString();
    }
}
