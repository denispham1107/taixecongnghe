using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.Editor
{
    public static class PhoVietCarUpgrade
    {
        const string Folder="Assets/PhoViet/Art/Cars/";
        public static readonly string[] Assets={"PV_Sedan","PV_Hatchback","PV_Taxi","PV_DeliveryVan","PV_BoxTruck","PV_DrowsyTruck"};
        public static readonly string[] ClipNames={"Idle","Drive","Brake","ImpactFront","ImpactRear","ImpactLeft","ImpactRight"};
        [MenuItem("Phố Việt/Art/Replace all four-wheel vehicles with Blender models")]
        public static void UpgradeOpenScene()
        {
            if(System.IO.File.Exists(PhoVietMeshyCarUpgrade.Folder+"PV_MeshySport.fbx")){PhoVietMeshyCarUpgrade.Install();return;}
            if(Application.isPlaying)throw new Exception("Exit Play Mode first");
            var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(game==null)throw new Exception("Open PhoViet scene");
            PhoVietCarImport.ConfigureAll();ApplyToGame(game);EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
        }
        public static void ApplyToGame(PhoVietGame game)
        {
            int sedan=0;
            foreach(var v in game.vehicles.Where(v=>v.halfWidth>.7f))
            {
                var old=v.model.GetComponent<CarVisualAnimator>();
                string asset=old!=null?old.AssetName:v.halfLength>3?(v.eventOnly?"PV_DrowsyTruck":"PV_BoxTruck"):
                    v.model.name.Contains("giao hàng")||v.model.Find("Giao hàng")!=null?"PV_DeliveryVan":
                    v.model.name.Contains("Taxi")?"PV_Taxi":sedan++%2==0?"PV_Sedan":"PV_Hatchback";
                Replace(v,asset);
            }
        }
        static void Replace(PhoVietGame.TrafficVehicle v,string asset)
        {
            var root=v.model;
            for(int i=root.childCount-1;i>=0;i--)
            {
                var child=root.GetChild(i);if(child.name=="Biển xe nguy hiểm")continue;
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            v.brakeLights=null;
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+asset+".fbx");if(source==null)throw new Exception("Missing "+asset);
            var model=(GameObject)PrefabUtility.InstantiatePrefab(source,root);model.name="Blender • "+asset;
            model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;
            var skins=model.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(s=>s.name.Contains("_LOD1")?1:s.name.Contains("_LOD2")?2:0).ToArray();
            if(skins.Length!=3)throw new Exception(asset+": three LODs required");
            bool heavy=v.halfLength>3;var material=AtlasMaterial(asset);
            foreach(var skin in skins)
            {
                skin.sharedMaterial=material;skin.quality=SkinQuality.Bone1;skin.enabled=true;skin.updateWhenOffscreen=false;
                skin.shadowCastingMode=ShadowCastingMode.On;skin.receiveShadows=true;
                skin.localBounds=new Bounds(new Vector3(0,heavy?1.6f:1,0),new Vector3(heavy?3.2f:2.5f,heavy?3.7f:2.5f,heavy?7.2f:5.2f));
            }
            var lod=model.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.19f,new Renderer[]{skins[0]}),new LOD(.075f,new Renderer[]{skins[1]}),new LOD(.009f,new Renderer[]{skins[2]})});
            lod.localReferencePoint=new Vector3(0,heavy?1.5f:1,0);lod.size=heavy?6.8f:4.7f;
            var player=model.GetComponent<Animation>()??model.AddComponent<Animation>();player.playAutomatically=false;player.cullingType=AnimationCullingType.AlwaysAnimate;player.Stop();
            var clips=AssetDatabase.LoadAllAssetsAtPath(Folder+asset+".fbx").OfType<AnimationClip>().Where(c=>ClipNames.Contains(c.name)).ToArray();
            if(clips.Length!=7)throw new Exception(asset+": seven clips required");
            var driver=root.GetComponent<CarVisualAnimator>()??root.gameObject.AddComponent<CarVisualAnimator>();
            driver.visual=model.transform;driver.animationPlayer=player;driver.clips=clips;driver.AssetName=asset;driver.wheelRadius=heavy?.51f:.38f;
            driver.tailX=heavy?.77f:.64f;driver.tailY=heavy?.85f:.94f;driver.tailZ=heavy?-3.25f:asset=="PV_DeliveryVan"?-2.42f:-2.14f;
            var bones=model.GetComponentsInChildren<Transform>(true);
            driver.body=bones.Single(t=>t.name=="Body");driver.bumperFront=bones.Single(t=>t.name=="BumperFront");driver.bumperRear=bones.Single(t=>t.name=="BumperRear");
            string[] suffix={"FL","FR","RL","RR"};driver.wheels=suffix.Select(s=>bones.Single(t=>t.name=="Wheel"+s)).ToArray();driver.steering=suffix.Select(s=>bones.Single(t=>t.name=="Steer"+s)).ToArray();
        }
        static Material AtlasMaterial(string asset)
        {
            foreach(string suffix in new[]{"_Palette.png","_Surface.png"})
            {
                var importer=AssetImporter.GetAtPath(Folder+asset+suffix) as TextureImporter;if(importer==null)throw new Exception("Missing texture");
                importer.sRGBTexture=suffix=="_Palette.png";importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Clamp;importer.mipmapEnabled=false;importer.maxTextureSize=128;importer.SaveAndReimport();
            }
            string path=Folder+asset+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,path);}
            mat.name=asset+"_Atlas";mat.color=Color.white;mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+asset+"_Palette.png");
            mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+asset+"_Surface.png"));mat.EnableKeyword("_METALLICGLOSSMAP");mat.SetFloat("_GlossMapScale",1);EditorUtility.SetDirty(mat);return mat;
        }
    }
    public sealed class PhoVietCarImport : AssetPostprocessor
    {
        const string Folder="Assets/PhoViet/Art/Cars/";
        void OnPreprocessModel(){if(assetPath.StartsWith(Folder,StringComparison.Ordinal)){Configure((ModelImporter)assetImporter);if(assetPath.Contains("/Meshy/"))((ModelImporter)assetImporter).meshCompression=ModelImporterMeshCompression.Off;}}
        static void Configure(ModelImporter i)
        {
            i.animationType=ModelImporterAnimationType.Legacy;i.importAnimation=true;i.optimizeGameObjects=false;i.materialImportMode=ModelImporterMaterialImportMode.None;
            i.importVisibility=false;i.importCameras=false;i.importLights=false;i.addCollider=false;i.globalScale=1;i.isReadable=false;
            i.meshCompression=ModelImporterMeshCompression.Low;i.animationCompression=ModelImporterAnimationCompression.KeyframeReduction;i.animationRotationError=.15f;i.animationPositionError=.002f;i.animationScaleError=.01f;
        }
        public static void ConfigureAll()
        {
            AssetDatabase.Refresh();
            foreach(string asset in PhoVietCarUpgrade.Assets)
            {
                var i=AssetImporter.GetAtPath(Folder+asset+".fbx") as ModelImporter;if(i==null)throw new Exception("Missing "+asset);Configure(i);
                var clips=i.defaultClipAnimations;
                foreach(var clip in clips)
                {
                    string n=PhoVietCarUpgrade.ClipNames.FirstOrDefault(s=>clip.name.EndsWith(s,StringComparison.Ordinal));if(n==null)continue;
                    clip.name=n;clip.loopTime=n=="Drive"||n=="Idle";clip.wrapMode=clip.loopTime?WrapMode.Loop:WrapMode.ClampForever;
                }
                i.clipAnimations=clips;i.SaveAndReimport();
            }
        }
    }
}
