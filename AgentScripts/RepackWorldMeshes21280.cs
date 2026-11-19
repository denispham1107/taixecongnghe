using System.Linq;
using UnityEngine;
using UnityEditor;
using PhoViet.OpenWorldEditor;
// 21.280: nhập lại mesh thế giới qua PhoVietWorldMeshPostprocessor, nén mesh .asset nhà (NhaNgan/NhaNoiBat), cây bỏ tangent.
public static class RepackWorldMeshes21280
{
 public static object Main()
 {
  long before=0,after=0;
  System.Func<Mesh,long> B=m=>{long b=0;foreach(var a in m.GetVertexAttributes()){b+=(long)m.vertexCount*a.dimension*(a.format==UnityEngine.Rendering.VertexAttributeFormat.Float32||a.format==UnityEngine.Rendering.VertexAttributeFormat.UInt32?4:a.format==UnityEngine.Rendering.VertexAttributeFormat.Float16?2:1);}return b;};
  var fbx=AssetDatabase.FindAssets("t:Model",new[]{"Assets/PhoViet/OpenWorld"}).Select(AssetDatabase.GUIDToAssetPath).Where(PhoVietWorldMeshPostprocessor.Applies).ToArray();
  foreach(var p in fbx)before+=AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>().Sum(B);
  AssetDatabase.StartAssetEditing();
  try{foreach(var p in fbx)AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceUpdate);
   foreach(var t in new[]{"PV_StreetTree0","PV_StreetTree1","PV_StreetTree2"}){var p="Assets/PhoViet/Art/Streets/"+t+".fbx";var mi=(ModelImporter)AssetImporter.GetAtPath(p);if(mi&&mi.importTangents!=ModelImporterTangents.None){mi.importTangents=ModelImporterTangents.None;mi.SaveAndReimport();}}
  }finally{AssetDatabase.StopAssetEditing();}
  foreach(var p in fbx)after+=AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>().Sum(B);
  int assets=0;long ab=0,aa=0;
  foreach(var g in AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/PhoViet/OpenWorld/Library/NhaNgan","Assets/PhoViet/OpenWorld/Library/NhaNoiBat"})){
   var p=AssetDatabase.GUIDToAssetPath(g);if(!p.EndsWith(".asset"))continue;
   foreach(var m in AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>()){ab+=B(m);if(PhoVietWorldMeshPostprocessor.Repack(m)){EditorUtility.SetDirty(m);assets++;}aa+=B(m);}
  }
  AssetDatabase.SaveAssets();
  return new{fbx=fbx.Length,fbxMB=System.Math.Round(before/1048576.0,1)+" -> "+System.Math.Round(after/1048576.0,1),assetMeshes=assets,assetMB=System.Math.Round(ab/1048576.0,1)+" -> "+System.Math.Round(aa/1048576.0,1)};
 }
}
