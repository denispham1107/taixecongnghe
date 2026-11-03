using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEngine.Rendering;
public static class InstallEnergy2122
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Exit Play before importing Energy C");
  var previewScene=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
  const string model="Assets/PhoViet/Art/Energy/PV_EnergyHalo.fbx",matPath="Assets/PhoViet/Art/Energy/PV_EnergyHalo.mat",prefab="Assets/PhoViet/Resources/Energy/PV_EnergyPickup.prefab",hud="Assets/PhoViet/Resources/Energy/PV_EnergyHaloHud.png";
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  var importer=(ModelImporter)AssetImporter.GetAtPath(model);importer.importAnimation=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.addCollider=false;importer.meshCompression=ModelImporterMeshCompression.Off;importer.SaveAndReimport();
  var tex=(TextureImporter)AssetImporter.GetAtPath(hud);tex.alphaIsTransparency=true;tex.mipmapEnabled=false;tex.textureCompression=TextureImporterCompression.Uncompressed;tex.filterMode=FilterMode.Bilinear;tex.wrapMode=TextureWrapMode.Clamp;tex.SaveAndReimport();
  var shader=Shader.Find("PhoViet/Energy Pickup");if(shader==null||ShaderUtil.ShaderHasError(shader))throw new Exception("Energy shader failed");
  var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,matPath);}else material.shader=shader;
  var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(model),previewScene);root.name="Năng lượng C • vòng sét xanh";
  try
  {
   var renderers=root.GetComponentsInChildren<MeshRenderer>(true).OrderBy(r=>r.name).ToArray();if(renderers.Length!=3)throw new Exception("Expected 3 C LOD meshes");
   foreach(var r in renderers){r.sharedMaterials=new[]{material};r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;var mesh=r.GetComponent<MeshFilter>().sharedMesh;if(mesh.colors32.Length!=mesh.vertexCount)throw new Exception("Missing energy vertex colours");}
   var group=root.GetComponent<LODGroup>()??root.AddComponent<LODGroup>();group.fadeMode=LODFadeMode.None;group.SetLODs(new[]{new LOD(.045f,new Renderer[]{renderers[0]}),new LOD(.018f,new Renderer[]{renderers[1]}),new LOD(.004f,new Renderer[]{renderers[2]})});group.RecalculateBounds();
   PrefabUtility.SaveAsPrefabAsset(root,prefab);
   var report=new{success=true,prefab,lods=renderers.Select(r=>new{name=r.name,triangles=r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3,vertices=r.GetComponent<MeshFilter>().sharedMesh.vertexCount,bounds=new[]{r.bounds.size.x,r.bounds.size.y,r.bounds.size.z}}).ToArray(),hud=AssetDatabase.LoadAssetAtPath<Texture2D>(hud).width,temporaryPreviewOnly=true};
   File.WriteAllText("Captures/energy2122-import-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }finally{UnityEngine.Object.DestroyImmediate(root);UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(previewScene);AssetDatabase.SaveAssets();}
 }
}
