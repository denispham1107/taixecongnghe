using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 21.284: icon tên lửa trên đường hai mặt — mặt sau (z < 0) là tấm nền xanh đen, nhìn từ phía sau thấy một khối lục giác đen.
// Bỏ các tam giác quay ra sau (pháp tuyến mặt z < −0,35), thay bằng bản gương (z → −z) của các tam giác quay ra trước; thành bên
// giữ nguyên. Chạy lại vẫn ra cùng kết quả (mặt sau luôn dựng lại từ mặt trước).
public static class PickupTwoSided21284
{
 public static object Main()
 {
  var res = new List<string>();
  foreach (var lod in new[] { "LOD0", "LOD1", "LOD2" })
  {
   string path = "Assets/PhoViet/Resources/PlayerMissile/PV_MissilePickup_" + lod + ".asset";
   var m = AssetDatabase.LoadAssetAtPath<Mesh>(path);
   var v = m.vertices; var n = m.normals; var c = m.colors; var t = m.triangles;
   var nv = new List<Vector3>(); var nn = new List<Vector3>(); var nc = new List<Color>(); var nt = new List<int>();
   var mapF = new Dictionary<int, int>(); var mapB = new Dictionary<int, int>();
   int Add(int i, bool mirror)
   {
    var map = mirror ? mapB : mapF;
    if (map.TryGetValue(i, out int k)) return k;
    k = nv.Count; map[i] = k;
    var p = v[i]; var q = n[i];
    nv.Add(mirror ? new Vector3(p.x, p.y, -p.z) : p);
    nn.Add(mirror ? new Vector3(q.x, q.y, -q.z) : q);
    nc.Add(c.Length == v.Length ? c[i] : Color.white);
    return k;
   }
   int front = 0, span = 0, dropped = 0;
   for (int i = 0; i < t.Length; i += 3)
   {
    // chia theo HƯỚNG mặt tam giác (không theo vị trí: tấm nền lục giác nằm ở z ≈ −0,02 nhưng quay ra trước)
    var fn = Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]);
    float nz = fn.sqrMagnitude > 1e-12f ? fn.normalized.z : 0f;
    if (nz < -0.35f) { dropped++; continue; }                         // quay ra sau → bỏ, thay bằng gương của mặt trước
    nt.Add(Add(t[i], false)); nt.Add(Add(t[i + 1], false)); nt.Add(Add(t[i + 2], false));
    if (nz > 0.35f)
    {   // quay ra trước → thêm bản gương (đảo chiều quấn để mặt quay ra sau)
     front++;
     nt.Add(Add(t[i], true)); nt.Add(Add(t[i + 2], true)); nt.Add(Add(t[i + 1], true));
    }
    else span++;
   }
   var bounds = m.bounds;
   m.Clear();
   m.SetVertices(nv); m.SetNormals(nn); m.SetColors(nc); m.SetTriangles(nt, 0); m.bounds = bounds;
   EditorUtility.SetDirty(m);
   res.Add(lod + ": front " + front + " mirrored, side " + span + ", dropped back " + dropped + ", verts " + nv.Count + ", tris " + nt.Count / 3);
  }
  AssetDatabase.SaveAssets();
  return res.ToArray();
 }
}
