using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
namespace PhoViet.Editor
{
 public static class PhoVietSupercarUpgrade
 {
  public const string Folder="Assets/PhoViet/Art/PlayerCar/",Asset="PV_BlueSupercar";
  public const float PlayerScale=.85f;
  public static void ApplyPlayerScale(PhoVietGame g)
  {
   var car=g.rider.GetComponent<PlayerCarVisual>();
   if(car==null||!car.visual.name.Contains("Blue Supercar"))throw new Exception("Blue Supercar must be installed first");
   car.visual.localScale=Vector3.one*PlayerScale;
   PrefabUtility.RecordPrefabInstancePropertyModifications(car.visual);
   car.size=new Vector3(2.071266f,1.076452f,4.4f)*PlayerScale;
   car.frontAxle=1.2f*PlayerScale;car.rearAxle=1.32f*PlayerScale;car.wheelRadius=.315f*PlayerScale;
   var lamp=g.daylight.vehicles.Single(v=>v.root==g.rider);
   foreach(var halo in lamp.halos)halo.size=(halo.rear?.45f:.65f)*PlayerScale;
   EditorUtility.SetDirty(car);EditorUtility.SetDirty(g.daylight);EditorUtility.SetDirty(g);
  }
  [MenuItem("Phố Việt/Art/Install Blue Supercar player")]
  public static void MenuInstall(){Debug.Log(Install());}
  public static object Install()
  {
   if(Application.isPlaying)throw new Exception("Exit Play Mode before installation");
   var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
   if(g==null||g.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity"||g.daylight==null)throw new Exception("Open complete PhoViet scene");
   Directory.CreateDirectory(".tools");
   if(!File.Exists(".tools/PhoViet-before-supercar-player-memory.unity"))EditorSceneManager.SaveScene(g.gameObject.scene,".tools/PhoViet-before-supercar-player-memory.unity",true);
   if(g.rider.GetComponent<PlayerCarVisual>()!=null&&g.rider.GetComponent<PlayerCarVisual>().visual.name.Contains("Blue Supercar")){PairLamps(g);ApplyPlayerScale(g);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);return new{success=true,alreadyInstalled=true,scale=PlayerScale};}
   var npc=g.vehicles.Select(v=>EditorJsonUtility.ToJson(v.model.GetComponent<BikeVisualAnimator>()??(UnityEngine.Object)v.model.GetComponent<CarVisualAnimator>())).ToArray();
   string police=EditorJsonUtility.ToJson(g.policeBike.GetComponent<BikeVisualAnimator>());
   Configure();var mat=AssetDatabase.LoadAssetAtPath<Material>(Folder+Asset+".mat");
   var lamp=g.daylight.vehicles.Single(v=>v.root==g.rider);
   var anchors=new[]{lamp.head}.Concat(lamp.halos.Select(h=>h.source)).Where(t=>t!=null).Distinct().ToArray();
   foreach(var t in anchors)t.SetParent(g.rider,true);
   var old=g.rider.GetComponent<BikeVisualAnimator>();if(old!=null)UnityEngine.Object.DestroyImmediate(old);
   foreach(var component in g.rider.GetComponents<MonoBehaviour>())if(component is PlayerCarVisual||component is BoostSmokeVisual||component is CarJumpJetVisual)UnityEngine.Object.DestroyImmediate(component);
   foreach(var child in g.rider.Cast<Transform>().ToArray())if(!anchors.Contains(child))UnityEngine.Object.DestroyImmediate(child.gameObject);
   g.leftKickLeg=g.rightKickLeg=null;
   var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+Asset+".fbx"),g.rider);
   go.name="Blue Supercar • xe người chơi";go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
   var skins=go.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(s=>s.name.Contains("_LOD2")?2:s.name.Contains("_LOD1")?1:0).ToArray();
   if(skins.Length!=3)throw new Exception("Three Supercar LODs required");
   var car=g.rider.gameObject.AddComponent<PlayerCarVisual>();car.visual=go.transform;car.size=new Vector3(2.071266f,1.076452f,4.4f);car.wheelRadius=.315f;car.frontAxle=1.2f;car.rearAxle=1.32f;
   foreach(var s in skins){s.sharedMaterial=mat;s.quality=SkinQuality.Bone1;s.enabled=true;s.updateWhenOffscreen=false;s.shadowCastingMode=ShadowCastingMode.On;s.receiveShadows=true;s.localBounds=new Bounds(new Vector3(0,car.size.y*.5f,0),car.size+Vector3.one*.5f);}
   var lod=go.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.19f,new Renderer[]{skins[0]}),new LOD(.075f,new Renderer[]{skins[1]}),new LOD(.006f,new Renderer[]{skins[2]})});lod.localReferencePoint=new Vector3(0,car.size.y*.5f,0);lod.size=car.size.z;lod.ForceLOD(-1);
   var bones=go.GetComponentsInChildren<Transform>(true);car.body=bones.Single(t=>t.name=="Body");
   car.wheels=new[]{"FL","FR","RL","RR"}.Select(s=>bones.Single(t=>t.name=="Wheel"+s)).ToArray();car.steering=new[]{"FL","FR","RL","RR"}.Select(s=>bones.Single(t=>t.name=="Steer"+s)).ToArray();
   car.animationPlayer=go.GetComponent<Animation>()??go.AddComponent<Animation>();car.animationPlayer.playAutomatically=false;car.animationPlayer.Stop();
   car.clips=AssetDatabase.LoadAllAssetsAtPath(Folder+Asset+".fbx").OfType<AnimationClip>().Where(c=>PhoVietCarUpgrade.ClipNames.Contains(c.name)).ToArray();
   if(car.clips.Length!=7)throw new Exception("Seven Supercar suspension/impact clips required");
   Transform front=bones.Single(t=>t.name=="BumperFront"),rear=bones.Single(t=>t.name=="BumperRear"),frame=bones.Single(t=>t.name=="Frame");
   Mount(lamp.head,front,g.rider,new Vector3(0,.46f,2.04f));
   int headIndex=0,tailIndex=0;foreach(var halo in lamp.halos)
   {float side=(halo.rear?tailIndex++:headIndex++)%2==0?-1:1;Mount(halo.source,halo.rear?rear:front,g.rider,new Vector3(side*.73f,halo.rear?.62f:.46f,halo.rear?-2.10f:2.04f));halo.size=halo.rear?.45f:.65f;}
   PairLamps(g);
   car.exhaustMounts=new[]{-1f,1f}.Select(side=>{var t=new GameObject("Supercar exhaust "+(side<0?"left":"right")).transform;Mount(t,rear,g.rider,new Vector3(side*.73f,.20f,-2.13f));return t;}).ToArray();car.exhaustMount=car.exhaustMounts[0];
   car.jumpMounts=new[]{new Vector3(-.77f,.17f,1.35f),new Vector3(.77f,.17f,1.35f),new Vector3(-.77f,.17f,-1.45f),new Vector3(.77f,.17f,-1.45f)}.Select((p,i)=>{var t=new GameObject("Lift jet "+i).transform;Mount(t,frame,g.rider,p);return t;}).ToArray();
   g.rider.gameObject.AddComponent<CarJumpJetVisual>();
   g.daylight.emissionMaterials=g.daylight.emissionMaterials.Concat(new[]{mat}).Distinct().ToArray();
   g.daylight.emissionUsers=g.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&(g.daylight.emissionMaterials.Contains(m)||m==g.daylight.streetLens))).ToArray();
   foreach(var preview in g.gameObject.scene.GetRootGameObjects().Where(o=>o!=g.gameObject))
    if(preview.GetComponentsInChildren<Renderer>(true).Any(r=>AssetDatabase.GetAssetPath((r as SkinnedMeshRenderer)?.sharedMesh??r.GetComponent<MeshFilter>()?.sharedMesh).Contains("Blue Petshop Supercar_20261027_004421")))
    {preview.tag="EditorOnly";preview.SetActive(false);}
   if(police!=EditorJsonUtility.ToJson(g.policeBike.GetComponent<BikeVisualAnimator>()))throw new Exception("Police changed");
   for(int i=0;i<npc.Length;i++)if(npc[i]!=EditorJsonUtility.ToJson(g.vehicles[i].model.GetComponent<BikeVisualAnimator>()??(UnityEngine.Object)g.vehicles[i].model.GetComponent<CarVisualAnimator>()))throw new Exception("NPC changed: "+i);
   if(g.jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Jobs must remain locked");
   ApplyPlayerScale(g);EditorUtility.SetDirty(car);EditorUtility.SetDirty(g);EditorUtility.SetDirty(g.daylight);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
   return new{success=true,asset=Asset,size=car.size,npcPreserved=npc.Length,policePreserved=true,jobsLocked=true};
  }
  static void Mount(Transform socket,Transform parent,Transform root,Vector3 point)
  {socket.SetParent(parent,false);socket.position=root.TransformPoint(point);socket.rotation=root.rotation;}
  static void PairLamps(PhoVietGame g)
  {
   var lamp=g.daylight.vehicles.Single(v=>v.root==g.rider);var halos=lamp.halos.ToList();
   foreach(bool rear in new[]{false,true})
   {
    var group=halos.Where(h=>h.rear==rear).ToArray();if(group.Length!=1)continue;
    var original=group[0];var socket=new GameObject(rear?"Supercar tail right":"Supercar head right").transform;
    Mount(socket,original.source.parent,g.rider,new Vector3(.73f,rear?.62f:.46f,rear?-2.10f:2.04f));
    var glow=UnityEngine.Object.Instantiate(original.glow.gameObject,original.glow.transform.parent).GetComponent<Renderer>();glow.name=rear?"Supercar right tail halo":"Supercar right head halo";
    halos.Add(new DayNightLighting.Halo{source=socket,glow=glow,rear=rear,size=original.size});
   }
   lamp.halos=halos.ToArray();
  }
  static void Configure()
  {
   AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
   var i=(ModelImporter)AssetImporter.GetAtPath(Folder+Asset+".fbx");i.animationType=ModelImporterAnimationType.Legacy;i.importAnimation=true;i.optimizeGameObjects=false;i.materialImportMode=ModelImporterMaterialImportMode.None;i.importCameras=false;i.importLights=false;i.addCollider=false;i.globalScale=1;i.meshCompression=ModelImporterMeshCompression.Off;
   var takes=i.defaultClipAnimations;foreach(var c in takes){var n=PhoVietCarUpgrade.ClipNames.FirstOrDefault(n=>c.name.EndsWith(n,StringComparison.Ordinal));if(n!=null){c.name=n;c.loopTime=n=="Idle"||n=="Drive";c.wrapMode=c.loopTime?WrapMode.Loop:WrapMode.ClampForever;}}i.clipAnimations=takes;i.SaveAndReimport();
   foreach(string suffix in new[]{"Palette","Normal","Surface","Emission"})
   {var texture=(TextureImporter)AssetImporter.GetAtPath(Folder+Asset+"_"+suffix+".png");texture.textureType=suffix=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;texture.sRGBTexture=suffix=="Palette"||suffix=="Emission";texture.mipmapEnabled=true;texture.filterMode=FilterMode.Trilinear;texture.anisoLevel=4;texture.maxTextureSize=2048;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.wrapMode=TextureWrapMode.Clamp;texture.SaveAndReimport();}
   string path=Folder+Asset+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,path);}
   mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture>(Folder+Asset+"_Palette.png");mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture>(Folder+Asset+"_Normal.png"));mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture>(Folder+Asset+"_Surface.png"));mat.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture>(Folder+Asset+"_Emission.png"));mat.SetColor("_EmissionColor",Color.black);mat.SetFloat("_GlossMapScale",.32f);mat.EnableKeyword("_NORMALMAP");mat.EnableKeyword("_METALLICGLOSSMAP");mat.EnableKeyword("_EMISSION");EditorUtility.SetDirty(mat);
  }
 }
}
