using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PhoViet.Editor
{
 public static class PhoVietMissilePickupUpgrade
 {
  const string Source="ArtSource/Concepts/MissilePickup-20261105/";
  const string Resource="Assets/PhoViet/Resources/PlayerMissile/";
  public static object Install()
  {
   if(Application.isPlaying)throw new InvalidOperationException("Install pickup in Edit Mode");
   var approval=JObject.Parse(File.ReadAllText(Source+"approval.json"));
   if(!(bool)approval["approved"]||(string)approval["selected"]!="C")throw new InvalidOperationException("C must be approved before installing");
   var data=JObject.Parse(File.ReadAllText(Source+"approved-C-geometry.json"));
   if((string)data["approved"]!="C"||data["lods"].Count()!=3)throw new InvalidOperationException("Expected three approved C LODs");
   var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Energy/PV_EnergyHalo.mat");
   if(material==null||ShaderUtil.ShaderHasError(material.shader))throw new InvalidOperationException("Pickup shader missing/error");
   var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject("Nạp tên lửa +1 • C Hex Ammo");
   UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
   var counts=new int[3];
   try
   {
    var lods=new LOD[3];
    for(int i=0;i<3;i++)
    {
     var item=data["lods"][i];var mesh=new Mesh{name="PV_MissilePickup_LOD"+i};
     mesh.vertices=item["vertices"].Select(v=>new Vector3((float)v[0],(float)v[1],(float)v[2])).ToArray();
     mesh.normals=item["normals"].Select(v=>new Vector3((float)v[0],(float)v[1],(float)v[2])).ToArray();
     mesh.colors=item["colors"].Select(v=>new Color((float)v[0],(float)v[1],(float)v[2],(float)v[3])).ToArray();
     mesh.triangles=item["triangles"].Select(v=>(int)v).ToArray();mesh.RecalculateBounds();
     if(mesh.bounds.size.x>1.86f||mesh.bounds.size.y>2||mesh.vertexCount==0)throw new InvalidOperationException("Pickup geometry outside lane budget");
     string path=Resource+mesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
     if(existing==null)AssetDatabase.CreateAsset(mesh,path);
     else
     {
      // Mesh setters invalidate the native/GPU buffers as well as the serialized
      // payload; CopySerialized alone can leave an already loaded renderer stale.
      existing.Clear();existing.vertices=mesh.vertices;existing.normals=mesh.normals;existing.colors=mesh.colors;existing.triangles=mesh.triangles;existing.RecalculateBounds();
      UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);
     }
     mesh.UploadMeshData(false);
     counts[i]=mesh.triangles.Length/3;
     var child=new GameObject(mesh.name);child.transform.SetParent(root.transform,false);child.AddComponent<MeshFilter>().sharedMesh=mesh;
     var renderer=child.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
     renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
     lods[i]=new LOD(i==0?.045f:i==1?.018f:.003f,new Renderer[]{renderer});
    }
    var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
    PrefabUtility.SaveAsPrefabAsset(root,Resource+"PV_MissilePickup.prefab");AssetDatabase.SaveAssets();
    var result=new{success=true,approved="C",lodTriangles=counts,renderersPerLod=1,particles=0,lights=0,colliders=0,sceneDirty=UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty};
    File.WriteAllText("Captures/pickup2131-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
   }
   finally{UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.ClosePreviewScene(scene);}
  }
 }
}
