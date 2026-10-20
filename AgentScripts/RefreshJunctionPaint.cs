using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PhoViet;
public static class RefreshJunctionPaint
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  var template=JunctionStreetArt.CreateArrowMesh();var updated=new HashSet<Mesh>();
  foreach(var f in g.scenery.GetComponentsInChildren<MeshFilter>(true))
  {
   if(!f.name.StartsWith("Vạch mũi tên giao lộ",StringComparison.Ordinal)||f.sharedMesh==null||!updated.Add(f.sharedMesh))continue;
   if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(f.sharedMesh)))throw new Exception("Arrow mesh must be a saved asset");
   // Use mesh setters as well as dirty/save so the live renderer's GPU buffer
   // refreshes immediately, rather than retaining the old straight-only paint.
   var mesh=f.sharedMesh;mesh.Clear();mesh.vertices=template.vertices;mesh.triangles=template.triangles;
   mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.UploadMeshData(false);EditorUtility.SetDirty(mesh);
  }
  int vertices=template.vertexCount;UnityEngine.Object.DestroyImmediate(template);AssetDatabase.SaveAssets();return new{success=true,meshes=updated.Count,vertices};
 }
}
