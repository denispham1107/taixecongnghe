using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PhoViet.Editor
{
 public static class PhoVietPassengerUpgrade
 {
  const string Folder="Assets/PhoViet/Art/Passengers/";
  public static readonly string[] Clips={"Idle","Walk","Board","Sit"};
  public static void UpgradeOpenScene()
  {
   if(Application.isPlaying)throw new Exception("Exit Play Mode");AssetDatabase.Refresh();
   var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
   foreach(Transform child in g.transform.Cast<Transform>().ToArray())if(child.name.StartsWith("PV_Passenger",StringComparison.Ordinal)&&child!=g.malePassenger&&child!=g.femalePassenger)UnityEngine.Object.DestroyImmediate(child.gameObject);
   g.malePassenger=Replace(g,"PV_PassengerMale",g.malePassenger);g.femalePassenger=Replace(g,"PV_PassengerFemale",g.femalePassenger);
   g.navigationLeft=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PhoViet/Audio/Navigation/left.mp3");
   g.navigationRight=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PhoViet/Audio/Navigation/right.mp3");
   g.navigationStraight=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PhoViet/Audio/Navigation/straight.mp3");
   if(g.navigationLeft==null||g.navigationRight==null||g.navigationStraight==null)throw new Exception("Three navigation clips required");
   g.jobsEnabled=PhoVietGame.RideJobsReleased;EditorUtility.SetDirty(g);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
  }
  static Transform Replace(PhoVietGame g,string name,Transform old)
  {
   var importer=(ModelImporter)AssetImporter.GetAtPath(Folder+name+".fbx");Configure(importer);importer.SaveAndReimport();
   var definitions=importer.defaultClipAnimations;
   foreach(var c in definitions){string key=Clips.FirstOrDefault(n=>c.name.EndsWith(n,StringComparison.Ordinal));if(key==null)continue;c.name=key;c.loopTime=key!="Board";c.wrapMode=c.loopTime?WrapMode.Loop:WrapMode.ClampForever;}
   importer.clipAnimations=definitions;importer.SaveAndReimport();
   if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
   var root=StreetArt.Group(name,g.transform);var source=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+name+".fbx");
   var model=(GameObject)PrefabUtility.InstantiatePrefab(source,root);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.Euler(0,180,0);
   var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+name+".mat");
   if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,Folder+name+".mat");}
   material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+name+"_Palette.png");material.color=Color.white;material.SetFloat("_Glossiness",.15f);EditorUtility.SetDirty(material);
   var skins=model.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(s=>s.name.Contains("_LOD1")?1:s.name.Contains("_LOD2")?2:0).ToArray();
   if(skins.Length!=3)throw new Exception("Three passenger LODs required");
   foreach(var skin in skins){skin.sharedMaterial=material;skin.quality=SkinQuality.Bone1;skin.enabled=true;skin.gameObject.SetActive(true);skin.localBounds=new Bounds(new Vector3(0,1,0),new Vector3(2.2f,2.7f,2.2f));}
   foreach(var autoLod in model.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(autoLod);
   var lod=root.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.12f,new Renderer[]{skins[0]}),new LOD(.055f,new Renderer[]{skins[1]}),new LOD(.006f,new Renderer[]{skins[2]})});lod.RecalculateBounds();
   var animation=model.GetComponent<Animation>();if(animation==null)animation=model.AddComponent<Animation>();animation.playAutomatically=false;animation.cullingType=AnimationCullingType.AlwaysAnimate;
   var clips=AssetDatabase.LoadAllAssetsAtPath(Folder+name+".fbx").OfType<AnimationClip>().Where(c=>Clips.Contains(c.name)).ToArray();
   if(clips.Length!=4)throw new Exception("Four passenger clips required: "+string.Join(",",clips.Select(c=>c.name)));
   foreach(var clip in clips)animation.AddClip(clip,clip.name);animation.Stop();
   root.gameObject.AddComponent<PassengerVisualAnimator>().animationPlayer=animation;root.gameObject.SetActive(false);return root;
  }
  public static void Configure(ModelImporter i)
  {
   i.materialImportMode=ModelImporterMaterialImportMode.None;i.importAnimation=true;i.animationType=ModelImporterAnimationType.Legacy;i.optimizeGameObjects=false;i.importCameras=false;i.importLights=false;
   i.globalScale=1;i.useFileScale=true;i.bakeAxisConversion=true;i.isReadable=false;
  }
 }
 public sealed class PhoVietPassengerImport:AssetPostprocessor
 {
  void OnPreprocessModel(){if(assetPath.StartsWith("Assets/PhoViet/Art/Passengers/",StringComparison.Ordinal))PhoVietPassengerUpgrade.Configure((ModelImporter)assetImporter);}
  void OnPreprocessTexture()
  {
   if(!assetPath.StartsWith("Assets/PhoViet/Art/Passengers/",StringComparison.Ordinal))return;
   var i=(TextureImporter)assetImporter;i.maxTextureSize=128;i.filterMode=FilterMode.Point;i.mipmapEnabled=false;
  }
 }
}
