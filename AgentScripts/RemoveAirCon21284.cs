using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PhoViet.OpenWorldEditor;
// 21.284: xóa cục điều hòa (submesh vật liệu OW_AirCon) khỏi nhà phố bản đồ mở.
// mode "fbx": nhập lại các FBX có OW_AirCon (PhoVietWorldMeshPostprocessor.StripAirCon chạy khi nhập).
// mode "asset": làm rỗng submesh OW_AirCon của mesh .asset (NhaNgan/NhaNoiBat) theo renderer trong scene đang mở.
// mode "count": đếm renderer trong scene còn submesh OW_AirCon có tam giác.
public static class RemoveAirCon21284
{
 public static object Main(string mode)
 {
  var res = new List<string>();
  if (mode == "fbx")
  {
   var paths = new List<string>();
   foreach (var g in AssetDatabase.FindAssets("t:Model", new[] { "Assets/PhoViet/OpenWorld" }))
   {
    var p = AssetDatabase.GUIDToAssetPath(g);
    if (!PhoVietWorldMeshPostprocessor.Applies(p)) continue;
    var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
    if (go.GetComponentsInChildren<MeshRenderer>(true).Any(r => r.sharedMaterials.Any(m => m && m.name.StartsWith("OW_AirCon")))) paths.Add(p);
   }
   AssetDatabase.StartAssetEditing();
   try { foreach (var p in paths) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate); }
   finally { AssetDatabase.StopAssetEditing(); }
   res.Add("reimported " + paths.Count);
   return res.ToArray();
  }
  var done = new HashSet<Mesh>(); int left = 0, stripped = 0;
  foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
  {
   var mats = r.sharedMaterials; if (!mats.Any(m => m && m.name.StartsWith("OW_AirCon"))) continue;
   var mf = r.GetComponent<MeshFilter>(); var mesh = mf ? mf.sharedMesh : null; if (!mesh) continue;
   if (mode == "count")
   {
    for (int s = 0; s < mesh.subMeshCount && s < mats.Length; s++) if (mats[s] && mats[s].name.StartsWith("OW_AirCon") && mesh.GetIndexCount(s) > 0) { left++; break; }
    continue;
   }
   if (mode == "asset" && AssetDatabase.GetAssetPath(mesh).EndsWith(".asset") && done.Add(mesh))
   {
    int n = PhoVietWorldMeshPostprocessor.StripAirCon(mesh, mats);
    if (n > 0) { stripped++; EditorUtility.SetDirty(mesh); }
   }
  }
  if (mode == "asset") AssetDatabase.SaveAssets();
  res.Add(mode == "count" ? "renderers with AC triangles: " + left : "asset meshes stripped: " + stripped + " / " + done.Count);
  return res.ToArray();
 }
}
