using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using UnityEditor;using UnityEditor.SceneManagement;
public static class InstallPlayerMissile2128
{
 const string Res="Assets/PhoViet/Resources/PlayerMissile/";
 static Mesh PickupMesh(Mesh body,int level)
 {
  var vertices=new List<Vector3>();var normals=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
  var rot=Quaternion.Euler(0,0,155);var source=body.vertices;var ns=body.normals;var cs=body.colors;
  for(int i=0;i<source.Length;i++){vertices.Add(rot*source[i]);normals.Add(rot*ns[i]);var c=cs.Length==source.Length?cs[i]:Color.white;c.a=0;colors.Add(c);}triangles.AddRange(body.triangles);
  int segments=level==0?24:level==1?18:12,sides=level==0?6:level==1?5:4,offset=vertices.Count;
  for(int i=0;i<segments;i++)for(int j=0;j<sides;j++)
  {
   float a=i*Mathf.PI*2/segments,b=j*Mathf.PI*2/sides;var n=new Vector3(Mathf.Cos(a)*Mathf.Cos(b),Mathf.Sin(a)*Mathf.Cos(b),Mathf.Sin(b));
   vertices.Add(new Vector3(Mathf.Cos(a)*.9f,Mathf.Sin(a)*.9f,0)+n*.035f);normals.Add(n);colors.Add(new Color(.10f,.65f,.96f,.7f));
   int p=offset+i*sides+j,q=offset+((i+1)%segments)*sides+j,r=offset+((i+1)%segments)*sides+(j+1)%sides,s=offset+i*sides+(j+1)%sides;
   triangles.AddRange(new[]{p,q,r,p,r,s});
  }
  var mesh=new Mesh{name="PV_MissilePickup_LOD"+level};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
 }
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  var meshes=AssetDatabase.LoadAllAssetsAtPath("Assets/PhoViet/Art/Helicopter/Missile/PV_Missile.fbx").OfType<Mesh>().ToArray();var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Energy/PV_EnergyHalo.mat");
  if(material==null||ShaderUtil.ShaderHasError(material.shader))throw new Exception("Pickup material missing/error");
  var importer=(TextureImporter)AssetImporter.GetAtPath(Res+"PV_MissileHud.png");importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=256;importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="WebGL",overridden=true,maxTextureSize=256,format=TextureImporterFormat.DXT5});importer.SaveAndReimport();
  var preview=EditorSceneManager.NewPreviewScene();var root=new GameObject("Tên lửa +1 • vòng xanh");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,preview);
  try
  {
   var lods=new LOD[3];var counts=new int[3];
   for(int i=0;i<3;i++)
   {
    var mesh=PickupMesh(meshes.Single(m=>m.name=="PV_MissileBody_LOD"+i),i);string path=Res+mesh.name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    if(old==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}
    counts[i]=mesh.triangles.Length/3;var child=new GameObject(mesh.name);child.transform.SetParent(root.transform,false);child.AddComponent<MeshFilter>().sharedMesh=mesh;var r=child.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;lods[i]=new LOD(i==0?.045f:i==1?.018f:.003f,new Renderer[]{r});
   }
   var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();PrefabUtility.SaveAsPrefabAsset(root,Res+"PV_MissilePickup.prefab");AssetDatabase.SaveAssets();
   var result=new{success=true,lodTriangles=counts,renderersPerLod=1,particles=0,lights=0,hud=256,sceneDirty=UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty};File.WriteAllText("Captures/player2128-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }finally{UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.ClosePreviewScene(preview);}
 }
}
