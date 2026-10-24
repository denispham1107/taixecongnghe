using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet;
// Xóa người đứng vỉa hè (StreetArt.Person, đã gộp vào mesh "Geometry • …" của mỗi khối) theo yêu cầu 24/10/2026.
// Người cũ ở x=±8,15, z=15/33/51, cao tới ~1,8 m; chỉ tam giác nằm trọn trong hộp đó bị gỡ.
public static class RemoveSidewalkPeople
{
 public static readonly float[] PersonZ={15,33,51};
 public static bool InsidePerson(Vector3 p)=>Mathf.Abs(Mathf.Abs(p.x)-8.15f)<.6f&&p.y>.2f&&p.y<2.1f&&PersonZ.Any(z=>Mathf.Abs(p.z-z)<.6f);
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Exit Play Mode");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null||g.roadRows.Count!=5)throw new Exception("PhoViet scene required");
  var cleaned=new Dictionary<Mesh,Mesh>();int removed=0,groups=0;
  foreach(var row in g.roadRows)foreach(var block in row.tiles)
  {
   // Nếu còn nhóm "Người đi bộ" chưa gộp thì xóa luôn.
   foreach(var t in block.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Người đi bộ").ToArray()){UnityEngine.Object.DestroyImmediate(t.gameObject);groups++;}
   foreach(var filter in block.GetComponentsInChildren<MeshFilter>(true))
   {
    if(filter.transform.parent!=block||!filter.name.StartsWith("Geometry • ",StringComparison.Ordinal)||filter.sharedMesh==null)continue;
    if(!cleaned.TryGetValue(filter.sharedMesh,out var clean)){clean=Strip(filter.sharedMesh,ref removed);cleaned.Add(filter.sharedMesh,clean);}
    filter.sharedMesh=clean;
   }
  }
  EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
  return new{success=true,removedTriangles=removed,groups,cleanedMeshes=cleaned.Count(p=>p.Key!=p.Value)};
 }
 static Mesh Strip(Mesh source,ref int removed)
 {
  var v=source.vertices;var tris=source.triangles;var keep=new List<int>();int skipped=0;
  for(int i=0;i<tris.Length;i+=3)
  {
   if(InsidePerson(v[tris[i]])&&InsidePerson(v[tris[i+1]])&&InsidePerson(v[tris[i+2]])){skipped++;continue;}
   keep.Add(tris[i]);keep.Add(tris[i+1]);keep.Add(tris[i+2]);
  }
  if(skipped==0)return source;removed+=skipped;
  string path="Assets/PhoViet/Generated/PV_NoPeople_"+AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source))+".asset";
  var clean=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(clean==null){clean=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(clean,path);}
  clean.name=source.name;clean.triangles=keep.ToArray();clean.RecalculateBounds();EditorUtility.SetDirty(clean);return clean;
 }
}
