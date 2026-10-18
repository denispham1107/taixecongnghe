using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PhoViet.Editor
{
 public static class PhoVietStreetUpgrade
 {
  const string Folder="Assets/PhoViet/Art/Streets/";
  static Material material;
  public static void ConfigureFactories()
  {
   material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"PV_StreetAtlas.mat");
   if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,Folder+"PV_StreetAtlas.mat");}
   material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"PV_StreetPalette.png");material.color=Color.white;material.SetFloat("_Glossiness",.12f);EditorUtility.SetDirty(material);
   VietnamStreet.ShopFactory=(p,s,z,seed)=>{int i=seed%12;var t=Place("PV_StreetShop"+i.ToString("00"),p,new Vector3(s*13.2f,0,z),s*90);t.name="Blender • "+VietnamStreet.Shops[i];};
   VietnamStreet.TreeFactory=(p,x,z)=>Place("PV_StreetTree"+((int)z%3),p,new Vector3(x,0,z),0);
   VietnamStreet.PavementFactory=(p,s)=>
   {
    for(int i=0;i<8;i++){var t=Place("PV_StreetPavement",p,new Vector3(s*9.1f,0,i<7?4.2f+i*8.4f:60.9f),s>0?0:180);if(i==7)t.localScale=new Vector3(1,1,.5f);}
   };
  }
  static Transform Place(string asset,Transform parent,Vector3 position,float yaw)
  {
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+asset+".fbx");if(source==null)throw new Exception("Missing "+asset);
   var t=StreetArt.Group(asset,parent,position);t.localRotation=Quaternion.Euler(0,yaw,0);
   var go=(GameObject)PrefabUtility.InstantiatePrefab(source,t);go.transform.localPosition=Vector3.zero;go.transform.localRotation=source.transform.localRotation;go.transform.localScale=source.transform.localScale;
   var renderers=go.GetComponentsInChildren<MeshRenderer>(true).OrderBy(r=>r.name).ToArray();if(renderers.Length!=3)throw new Exception(asset+": expected three LODs");
   foreach(var r in renderers){r.sharedMaterial=material;r.enabled=true;r.gameObject.SetActive(true);}
   var lod=go.GetComponent<LODGroup>()??go.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.18f,new Renderer[]{renderers[0]}),new LOD(.07f,new Renderer[]{renderers[1]}),new LOD(.006f,new Renderer[]{renderers[2]})});lod.RecalculateBounds();return t;
  }
  [MenuItem("Phố Việt/Art/Install Blender streets")]
  public static void UpgradeOpenScene()
  {
   if(Application.isPlaying)throw new Exception("Exit Play Mode");
   var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null||g.roadRows.Count!=5)throw new Exception("Existing route scene required");
   AssetDatabase.Refresh();ConfigureFactories();StreetArt.ConfigureWorldText(g.vietnameseFont,g.worldTextMaterial);
   var oldRows=g.roadRows.ToArray();var parked=oldRows[0].root.GetComponentsInChildren<BikeVisualAnimator>(true).Where(b=>b.role==BikeVisualAnimator.ActorRole.Parked).ToArray();
   foreach(Transform child in g.scenery.Cast<Transform>().ToArray())if(!oldRows.Any(r=>r.root==child))UnityEngine.Object.DestroyImmediate(child.gameObject);
   if(parked.Length!=14)throw new Exception("Fourteen parked bikes required");
   int bikeIndex=0;StreetArt.ParkedBikeFactory=(p,n)=>{var t=UnityEngine.Object.Instantiate(parked[bikeIndex++%14].gameObject,p).transform;t.name=n;return t;};
   var rows=new List<PhoVietGame.RoadRow>();var lights=new List<Renderer>();
   var row0=new PhoVietGame.RoadRow{root=StreetArt.Group("Đường chính • Blender",g.scenery)};
   for(int j=0;j<7;j++){var b=VietnamStreet.Build(row0.root,j,lights);PhoVietBuilder.Combine(b);row0.tiles.Add(b);}rows.Add(row0);
   for(int i=1;i<5;i++)
   {
    var row=new PhoVietGame.RoadRow{root=StreetArt.Group("Đường nhánh • Blender "+i,g.scenery)};
    for(int j=0;j<7;j++)
    {
     var b=UnityEngine.Object.Instantiate(row0.tiles[(j+i)%7].gameObject,row.root).transform;b.localPosition=new Vector3(0,0,j*80);b.localRotation=Quaternion.identity;row.tiles.Add(b);
     foreach(var bike in b.GetComponentsInChildren<BikeVisualAnimator>(true))
     {
      var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/PhoViet/Generated/RouteParked_"+bike.AssetName+".asset");
      var skin=bike.GetComponentInChildren<SkinnedMeshRenderer>(true);var mat=skin.sharedMaterial;
      for(int c=bike.transform.childCount-1;c>=0;c--)UnityEngine.Object.DestroyImmediate(bike.transform.GetChild(c).gameObject);
      var visual=StreetArt.Group("Xe đậu Blender • tĩnh",bike.transform);visual.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;visual.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(bike);
     }
    }
    row.root.gameObject.SetActive(false);rows.Add(row);
   }
   foreach(var row in oldRows)UnityEngine.Object.DestroyImmediate(row.root.gameObject);
   g.roadRows=rows;g.blocks=new List<Transform>(row0.tiles);g.trafficLights=g.scenery.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("Tín hiệu ",StringComparison.Ordinal)).ToList();
   StreetArt.ParkedBikeFactory=null;PhoVietBuilder.PersistGeometry(g.gameObject);AssetDatabase.SaveAssets();EditorUtility.SetDirty(g);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
  }
 }
 public sealed class PhoVietStreetImport:AssetPostprocessor
 {
  void OnPreprocessModel()
  {
   if(!assetPath.StartsWith("Assets/PhoViet/Art/Streets/",StringComparison.Ordinal))return;
   var i=(ModelImporter)assetImporter;i.materialImportMode=ModelImporterMaterialImportMode.None;i.importAnimation=false;i.importCameras=false;i.importLights=false;i.isReadable=false;i.globalScale=1;i.useFileScale=true;
  }
  void OnPreprocessTexture()
  {
   if(!assetPath.EndsWith("PV_StreetPalette.png",StringComparison.Ordinal))return;
   var i=(TextureImporter)assetImporter;i.filterMode=FilterMode.Point;i.mipmapEnabled=false;i.maxTextureSize=128;i.textureCompression=TextureImporterCompression.Uncompressed;
  }
 }
}
