using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.Editor
{
 public static class PhoVietUrbanRiderUpgrade
 {
  public const string Folder="Assets/PhoViet/Art/UrbanRider/";
  public const string Model=Folder+"PV_UrbanRider.fbx";
  static readonly string[] Names={"Idle","Ride","KickLeft","KickRight","FallLeft","FallRight","Recover"};
  static readonly string[] Variants={"PV_HeroScooter","PV_CityScooter","PV_Underbone","PV_SportBike","PV_Racer"};
  static readonly string[] Shirts={"E8F0F5","FFFFFF","28657D","BC703E","536B37"};
  static readonly string[] Paint={"2878D4","FFFFFF","18ABBB","F3B62B","D93A38"};
  public static object Install()
  {
   if(Application.isPlaying)throw new Exception("Exit Play Mode first");
   var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
   if(g==null||g.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity"||g.jobsEnabled)throw new Exception("Open PhoViet scene with jobs locked");
   Directory.CreateDirectory(".tools");
   if(!File.Exists(".tools/PhoViet-before-urban-rider.unity"))EditorSceneManager.SaveScene(g.gameObject.scene,".tools/PhoViet-before-urban-rider.unity",true);
   var police=g.policeBike.GetComponent<BikeVisualAnimator>();var policeVisual=police.visual;
   string policeBefore=EditorJsonUtility.ToJson(police),playerBefore=EditorJsonUtility.ToJson(g.rider.GetComponent<PlayerCarVisual>());
   AssetDatabase.Refresh();ConfigureModel();
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Model);
   var clips=AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Where(c=>Names.Contains(c.name)).ToArray();
   if(prefab==null||clips.Length!=7)throw new Exception("Urban rider model/clips missing");
   var mats=Variants.ToDictionary(v=>v,v=>MakeMaterial(v));
   var drivers=g.GetComponentsInChildren<BikeVisualAnimator>(true).Where(b=>b.role!=BikeVisualAnimator.ActorRole.Police&&b.AssetName!="PV_TrafficPolice").ToArray();
   int parked=drivers.Count(b=>b.role==BikeVisualAnimator.ActorRole.Parked),traffic=drivers.Length-parked,anchored=0;
   var newDrivers=new List<BikeVisualAnimator>();
   foreach(var old in drivers)
   {
    var root=old.transform;string asset=old.AssetName;var role=old.role;
    if(!mats.ContainsKey(asset))throw new Exception("Unexpected bike variant "+asset);
    var lamp=g.daylight.vehicles.FirstOrDefault(l=>l.root==root);
    if(lamp!=null)
    {
     if(lamp.head!=null)lamp.head.SetParent(root,true);
     foreach(var halo in lamp.halos)if(halo.source!=null)halo.source.SetParent(root,true);
    }
    if(old.urbanMotion!=null)UnityEngine.Object.DestroyImmediate(old.urbanMotion);
    if(old.visual!=null)UnityEngine.Object.DestroyImmediate(old.visual.gameObject);
    UnityEngine.Object.DestroyImmediate(old);
    var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);
    PrefabUtility.UnpackPrefabInstance(instance,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
    instance.name="Urban Rider • "+asset;var visual=instance.transform;
    visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.identity;visual.localScale=Vector3.one;
    var skins=instance.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(r=>r.name.Contains("_LOD2")?2:r.name.Contains("_LOD1")?1:0).ToArray();
    if(skins.Length!=3)throw new Exception("Expected three urban LODs");
    foreach(var r in skins)
    {r.sharedMaterial=mats[asset];r.enabled=true;r.quality=SkinQuality.Bone2;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;r.updateWhenOffscreen=false;r.localBounds=new Bounds(new Vector3(0,1.08f,0),new Vector3(3.5f,3.5f,3.5f));}
    foreach(var l in instance.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(l);
    var lod=instance.AddComponent<LODGroup>();lod.fadeMode=LODFadeMode.None;lod.SetLODs(new[]{new LOD(.16f,new Renderer[]{skins[0]}),new LOD(.055f,new Renderer[]{skins[1]}),new LOD(.008f,new Renderer[]{skins[2]})});lod.size=2.3f;lod.localReferencePoint=new Vector3(0,1.08f,0);
    var anim=instance.GetComponent<Animation>()??instance.AddComponent<Animation>();anim.playAutomatically=false;anim.cullingType=AnimationCullingType.AlwaysAnimate;
    var bike=root.gameObject.AddComponent<BikeVisualAnimator>();bike.AssetName=asset;bike.role=role;bike.visual=visual;bike.animationPlayer=anim;bike.clips=clips;
    var bones=instance.GetComponentsInChildren<Transform>(true).GroupBy(t=>t.name).ToDictionary(a=>a.Key,a=>a.First());
    bike.frontWheel=bones["WheelFront"];bike.rearWheel=bones["WheelRear"];
    var motion=root.gameObject.AddComponent<UrbanRiderMotion>();bike.urbanMotion=motion;motion.visual=visual;motion.steering=bones["Steering"];motion.frontWheel=bike.frontWheel;motion.rearWheel=bike.rearWheel;
    motion.leftUpper=bones["LUpperArm"];motion.leftForearm=bones["LForearm"];motion.leftHand=bones["LHand"];motion.rightUpper=bones["RUpperArm"];motion.rightForearm=bones["RForearm"];motion.rightHand=bones["RHand"];
    Mount(root,bones["Frame"],"PV_MeshyExhaust",new Vector3(.24f,.40f,-.84f));
    if(lamp!=null)
    {
     var head=lamp.head??new GameObject("Quang học đèn xe • pha").transform;
     Place(head,root,bones["Steering"],new Vector3(0,1.17f,.46f));lamp.head=head;
     var tail=lamp.halos.Where(h=>h.rear).Select(h=>h.source).FirstOrDefault(t=>t!=null)??new GameObject("Quang học đèn xe • hậu").transform;
     Place(tail,root,bones["Frame"],new Vector3(0,.88f,-.90f));
     foreach(var h in lamp.halos)h.source=h.rear?tail:head;
     anchored++;
    }
    var vehicle=g.vehicles.Find(v=>v.model==root);
    if(vehicle!=null)
    {
     vehicle.halfWidth=.33f;vehicle.halfLength=1.1801609f;
     if(vehicle.brakeLights!=null)
     {
      foreach(var light in vehicle.brakeLights.GetComponentsInChildren<Renderer>(true))
      {var p=light.transform.localPosition;p.y=.88f;p.z=-.91f;light.transform.localPosition=p;}
     }
    }
    newDrivers.Add(bike);
   }
   int baked=0;
   foreach(var b in newDrivers.Where(b=>b.role==BikeVisualAnimator.ActorRole.Parked).GroupBy(b=>b.AssetName).Select(a=>a.First()))
   {
    var skin=b.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(s=>s.name.EndsWith("_LOD1",StringComparison.Ordinal));
    var mesh=new Mesh{name="PV_UrbanParked_"+b.AssetName};skin.BakeMesh(mesh);
    string path=Folder+mesh.name+".asset";var target=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    if(target==null){AssetDatabase.CreateAsset(mesh,path);target=mesh;}else{EditorUtility.CopySerialized(mesh,target);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(target);}
    foreach(var f in g.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh!=null&&(f.sharedMesh.name=="PV_RouteParked_"+b.AssetName||f.sharedMesh==target)))
    {f.sharedMesh=target;f.GetComponent<MeshRenderer>().sharedMaterial=mats[b.AssetName];baked++;}
   }
   // Raw preview is not game content; keep its oversized original asset locally, out of the scene/build.
   int previews=0;
   foreach(var root in g.gameObject.scene.GetRootGameObjects())
   {
    string path=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
    if(path.StartsWith("Assets/MeshyImports/Urban Rider_",StringComparison.Ordinal)){UnityEngine.Object.DestroyImmediate(root);previews++;}
   }
   var used=g.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).ToHashSet();
   // Civilian atlases from the previous rider are no longer rendered. Remove
   // their lighting-only references so WebGL does not carry both texture sets.
   g.daylight.emissionMaterials=g.daylight.emissionMaterials.Where(m=>m!=null&&(used.Contains(m)||!AssetDatabase.GetAssetPath(m).Contains("/Bikes/Meshy"))).Concat(mats.Values).Distinct().ToArray();
   g.daylight.emissionUsers=g.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&(g.daylight.emissionMaterials.Contains(m)||m==g.daylight.streetLens))).ToArray();
   bool policeUnchanged=policeVisual==police.visual&&policeBefore==EditorJsonUtility.ToJson(police);
   bool playerUnchanged=playerBefore==EditorJsonUtility.ToJson(g.rider.GetComponent<PlayerCarVisual>());
   if(!policeUnchanged||!playerUnchanged||g.jobsEnabled)throw new Exception("Protected actor/jobs changed");
   EditorUtility.SetDirty(g);EditorUtility.SetDirty(g.daylight);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
   return new{success=true,replaced=newDrivers.Count,traffic,parked,baked,anchored,previews,policeUnchanged,playerUnchanged,jobsEnabled=g.jobsEnabled};
  }
  static void ConfigureModel()
  {
   var i=(ModelImporter)AssetImporter.GetAtPath(Model);i.animationType=ModelImporterAnimationType.Legacy;i.importAnimation=true;i.optimizeGameObjects=false;i.materialImportMode=ModelImporterMaterialImportMode.None;i.importCameras=false;i.importLights=false;i.addCollider=false;i.globalScale=1;i.meshCompression=ModelImporterMeshCompression.Off;i.animationCompression=ModelImporterAnimationCompression.Off;
   i.SaveAndReimport();var clips=i.defaultClipAnimations;
   foreach(var c in clips){string name=Names.FirstOrDefault(n=>c.name.EndsWith(n,StringComparison.OrdinalIgnoreCase));if(name==null)continue;c.name=name;c.loopTime=name=="Idle"||name=="Ride";c.wrapMode=c.loopTime?WrapMode.Loop:WrapMode.ClampForever;}
   i.clipAnimations=clips;i.SaveAndReimport();
  }
  static Material MakeMaterial(string variant)
  {
   string path=Folder+variant+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);var shader=Shader.Find("PhoViet/Urban Rider");if(shader==null)throw new Exception("Urban shader missing");
   if(mat==null){mat=new Material(shader);AssetDatabase.CreateAsset(mat,path);}mat.shader=shader;
   foreach(var map in new[]{("_MainTex","Palette",true,false),("_BumpMap","Normal",false,true),("_Surface","Surface",false,false),("_RegionMask","Regions",false,false),("_EmissionMap","Emission",true,false)})
   {
    string file=Folder+"PV_UrbanRider_"+map.Item2+".png";var t=(TextureImporter)AssetImporter.GetAtPath(file);t.textureType=map.Item4?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=map.Item3;t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.wrapMode=TextureWrapMode.Clamp;t.anisoLevel=4;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();mat.SetTexture(map.Item1,AssetDatabase.LoadAssetAtPath<Texture2D>(file));
   }
   int n=Array.IndexOf(Variants,variant);ColorUtility.TryParseHtmlString("#"+Shirts[n],out var shirt);ColorUtility.TryParseHtmlString("#"+Paint[n],out var paint);mat.SetColor("_ShirtColor",shirt);mat.SetColor("_BikeColor",paint);mat.SetColor("_EmissionColor",Color.black);EditorUtility.SetDirty(mat);return mat;
  }
  static void Place(Transform t,Transform root,Transform bone,Vector3 p){t.SetParent(bone,false);t.position=root.TransformPoint(p);t.rotation=root.rotation;}
  static void Mount(Transform root,Transform bone,string name,Vector3 p){var t=new GameObject(name).transform;Place(t,root,bone,p);}
 }
}
