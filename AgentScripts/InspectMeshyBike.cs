using System;using System.Linq;using UnityEditor;using UnityEngine;
public static class InspectMeshyBike
{
 public static object Main()
 {
  string path=AssetDatabase.FindAssets("t:Model",new[]{"Assets/MeshyImports"}).Select(AssetDatabase.GUIDToAssetPath).OrderByDescending(p=>System.IO.File.GetLastWriteTimeUtc(p)).First();
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
  return new{path,meshes=model.GetComponentsInChildren<MeshFilter>(true).Select(f=>new{name=f.name,vertices=f.sharedMesh.vertexCount,triangles=f.sharedMesh.triangles.Length/3,subMeshes=f.sharedMesh.subMeshCount,readable=f.sharedMesh.isReadable,boundsCentre=new[]{f.sharedMesh.bounds.center.x,f.sharedMesh.bounds.center.y,f.sharedMesh.bounds.center.z},boundsSize=new[]{f.sharedMesh.bounds.size.x,f.sharedMesh.bounds.size.y,f.sharedMesh.bounds.size.z},position=f.transform.localPosition.ToString(),rotation=f.transform.localEulerAngles.ToString(),scale=f.transform.localScale.ToString()}).ToArray(),skins=model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length,materials=model.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Distinct().Select(m=>new{name=m.name,shader=m.shader.name,texture=m.mainTexture!=null?AssetDatabase.GetAssetPath(m.mainTexture):""}).ToArray(),nodes=model.GetComponentsInChildren<Transform>(true).Select(t=>t.name).ToArray()};
 }
}
