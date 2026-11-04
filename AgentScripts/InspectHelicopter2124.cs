using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;
public static class InspectHelicopter2124
{
 public static object Main()
 {
  const string path="Assets/MeshyImports/Redline_Rescue_Helicopter_20261104_195539/Meshy_AI_Redline_Rescue_Helicopter_20261104_texture.fbx";
  var go=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(go==null)throw new Exception("Helicopter FBX has not imported");
  var r=new{success=true,path,meshes=go.GetComponentsInChildren<MeshFilter>(true).Select(f=>new{name=f.name,vertices=f.sharedMesh.vertexCount,triangles=f.sharedMesh.triangles.Length/3}).ToArray(),renderers=go.GetComponentsInChildren<Renderer>(true).Length,rig=go.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length,play=Application.isPlaying,scene=UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path};
  File.WriteAllText("Captures/heli2124-unity-source.json",Newtonsoft.Json.JsonConvert.SerializeObject(r,Newtonsoft.Json.Formatting.Indented));return r;
 }
}
