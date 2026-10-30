using System;using System.IO;using System.Linq;
using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.Rendering;
namespace PhoViet.Editor
{
 // Targeted boat conversion; never rebuilds the river, apartments or bridge.
 public static class PhoVietFishingBoatUpgrade
 {
  public const string Folder="Assets/PhoViet/Art/River/FishingBoat/";
  public const string SourceFolder="Assets/MeshyImports/Vietnamese_Fishing_Boat_20261030_145637/";
  public static void ImportAssets()
  {
   AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
   var importer=(ModelImporter)AssetImporter.GetAtPath(Folder+"PV_FishingBoat.fbx");
   importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importAnimation=false;importer.isReadable=false;importer.globalScale=1;importer.useFileScale=false;importer.addCollider=false;importer.meshCompression=ModelImporterMeshCompression.Off;importer.SaveAndReimport();
   foreach(var pair in new[]{("meshy_basecolor","Palette"),("meshy_normal","Normal"),("meshy_metallic_smoothness","Surface")})
   {
    string path=Folder+"PV_FishingBoat_"+pair.Item2+".png";
    if(!File.Exists(path))File.Copy(SourceFolder+pair.Item1+".png",path);
    AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
    var t=(TextureImporter)AssetImporter.GetAtPath(path);t.maxTextureSize=2048;t.mipmapEnabled=true;t.isReadable=false;t.filterMode=FilterMode.Trilinear;t.anisoLevel=4;t.sRGBTexture=pair.Item2=="Palette";t.textureType=pair.Item2=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;
    var web=t.GetPlatformTextureSettings("WebGL");web.overridden=true;web.maxTextureSize=2048;web.format=pair.Item2=="Palette"?TextureImporterFormat.DXT1:TextureImporterFormat.DXT5;web.textureCompression=TextureImporterCompression.CompressedHQ;web.compressionQuality=50;t.SetPlatformTextureSettings(web);t.SaveAndReimport();
   }
   string matPath=Folder+"PV_FishingBoat.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
   if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,matPath);}
   mat.color=Color.white;mat.mainTexture=Texture("Palette");mat.SetTexture("_BumpMap",Texture("Normal"));mat.EnableKeyword("_NORMALMAP");mat.SetTexture("_MetallicGlossMap",Texture("Surface"));mat.EnableKeyword("_METALLICGLOSSMAP");mat.SetFloat("_GlossMapScale",.35f);mat.SetFloat("_BumpScale",.7f);mat.enableInstancing=true;EditorUtility.SetDirty(mat);AssetDatabase.SaveAssets();
  }
  static Texture2D Texture(string suffix)=>AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"PV_FishingBoat_"+suffix+".png");
  public static bool ApplyOne(GameObject boat)
  {
   var meshes=AssetDatabase.LoadAllAssetsAtPath(Folder+"PV_FishingBoat.fbx").OfType<Mesh>().OrderBy(m=>m.name).ToArray();
   var mat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"PV_FishingBoat.mat");if(meshes.Length==0||mat==null)return false;
   if(meshes.Length!=3)throw new Exception("Fishing boat requires three LOD meshes");
   if(PrefabUtility.IsPartOfPrefabInstance(boat))PrefabUtility.UnpackPrefabInstance(boat,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
   var filters=boat.GetComponentsInChildren<MeshFilter>(true).OrderBy(f=>f.name).ToArray();if(filters.Length!=3)throw new Exception("Expected original three boat LODs");
   for(int i=0;i<3;i++)
   {
    filters[i].sharedMesh=meshes[i];filters[i].transform.localRotation=Quaternion.Euler(-90,0,0);
    var r=filters[i].GetComponent<MeshRenderer>();r.sharedMaterial=mat;r.receiveShadows=true;r.shadowCastingMode=ShadowCastingMode.On;
   }
   boat.name="Tàu đánh cá • Meshy";boat.GetComponent<LODGroup>().RecalculateBounds();return true;
  }
  public static object Apply()
  {
   if(Application.isPlaying||EditorApplication.isCompiling)throw new Exception("Idle Edit Mode required");
   var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var scene=g.gameObject.scene;
   if(g.jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Jobs must stay locked");
   var boats=g.scenery.GetComponentsInChildren<RiverBoat>(true);if(boats.Length!=140)throw new Exception("Expected 140 anchored boats");
   foreach(var b in boats)if(!ApplyOne(b.gameObject))throw new Exception("Missing fishing boat assets");
   // Meshy Bridge adds a temporary raw import at the origin. It is backed up
   // in .tools/boat-signal-before.unity; the game uses the optimized boats.
   int previews=0;
   foreach(var root in scene.GetRootGameObjects())if(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root).StartsWith(SourceFolder,StringComparison.Ordinal)){UnityEngine.Object.DestroyImmediate(root);previews++;}
   EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new Exception("Scene save failed");
   var report=new{success=true,boats=boats.Length,removedRawPreview=previews,scene=scene.path};File.WriteAllText("Captures/fishing-boat-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
 }
}
