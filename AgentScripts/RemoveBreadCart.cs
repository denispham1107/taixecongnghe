using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet;
// Xóa xe đẩy "BÁNH MÌ" cũ (VietnamStreet.Cart) theo yêu cầu 24/10/2026: nó đứng trước tủ kính Bún Bò Huế.
// Hình khối xe đã gộp vào mesh "Geometry • …" của mỗi khối; chỉ tam giác nằm trọn trong hộp xe bị gỡ,
// nhóm "Xe bánh mì" (chữ biển) bị xóa, rồi cài lại vật cản để bỏ hộp "Xe đẩy bán hàng".
public static class RemoveBreadCart
{
 public static bool InsideCart(Vector3 p)=>Mathf.Abs(p.x)>7.3f&&Mathf.Abs(p.x)<9.8f&&p.y>.25f&&p.y<2.7f&&Mathf.Abs(p.z-43)<1.2f;
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Exit Play Mode");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null||g.roadRows.Count!=5)throw new Exception("PhoViet scene required");
  var cleaned=new Dictionary<Mesh,Mesh>();int removed=0,groups=0;
  foreach(var row in g.roadRows)foreach(var block in row.tiles)
  {
   foreach(var t in block.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Xe bánh mì").ToArray()){UnityEngine.Object.DestroyImmediate(t.gameObject);groups++;}
   foreach(var filter in block.GetComponentsInChildren<MeshFilter>(true))
   {
    if(filter.transform.parent!=block||!filter.name.StartsWith("Geometry • ",StringComparison.Ordinal)||filter.sharedMesh==null)continue;
    if(!cleaned.TryGetValue(filter.sharedMesh,out var clean)){clean=Strip(filter.sharedMesh,ref removed);cleaned.Add(filter.sharedMesh,clean);}
    filter.sharedMesh=clean;
   }
  }
  var obstacles=PhoViet.Editor.PhoVietObstacleUpgrade.Install();
  EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
  return new{success=true,removedTriangles=removed,groups,cleanedMeshes=cleaned.Count(p=>p.Key!=p.Value),obstacles};
 }
 static Mesh Strip(Mesh source,ref int removed)
 {
  var v=source.vertices;var tris=source.triangles;var keep=new List<int>();int skipped=0;
  for(int i=0;i<tris.Length;i+=3)
  {
   if(InsideCart(v[tris[i]])&&InsideCart(v[tris[i+1]])&&InsideCart(v[tris[i+2]])){skipped++;continue;}
   keep.Add(tris[i]);keep.Add(tris[i+1]);keep.Add(tris[i+2]);
  }
  if(skipped==0)return source;removed+=skipped;
  string path="Assets/PhoViet/Generated/PV_NoCart_"+AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source))+".asset";
  var clean=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(clean==null){clean=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(clean,path);}
  clean.name=source.name;clean.triangles=keep.ToArray();clean.RecalculateBounds();EditorUtility.SetDirty(clean);return clean;
 }
}
