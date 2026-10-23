using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace PhoViet.Editor
{
    public static class PhoVietMeshyCarUpgrade
    {
        public const string Folder="Assets/PhoViet/Art/Cars/Meshy/";
        public static readonly string[] Models={"PV_MeshySport","PV_MeshyPickup","PV_MeshyCovered","PV_MeshyContainer"};
        [Serializable] sealed class Axle {public float x,y,z,radius;}
        [Serializable] sealed class Metadata
        {public string name,design;public float[] size,headlight,taillight;public Axle[] axles;public float wheel_radius;}
        // JsonUtility does not support nested arrays; read the profile as explicit samples.
        [Serializable] sealed class RoofData {public Vector2[] samples;}
        static Vector3 V(float[] p)=>new Vector3(p[0],p[1],p[2]);
        [MenuItem("Phố Việt/Art/Install received Meshy cars")]
        public static void MenuInstall(){Debug.Log(Install());}
        public static object Install()
        {
            if(Application.isPlaying)throw new Exception("Exit Play Mode first");
            var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
            if(game==null||game.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity"||game.daylight==null)throw new Exception("Open complete PhoViet scene");
            Directory.CreateDirectory(".tools");
            if(!File.Exists(".tools/PhoViet-before-meshy-cars.unity"))File.Copy(game.gameObject.scene.path,".tools/PhoViet-before-meshy-cars.unity");
            if(game.gameObject.scene.isDirty&&!File.Exists(".tools/PhoViet-before-meshy-cars-memory.unity"))EditorSceneManager.SaveScene(game.gameObject.scene,".tools/PhoViet-before-meshy-cars-memory.unity",true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var police=game.policeBike.GetComponent<BikeVisualAnimator>();string policeBefore=EditorJsonUtility.ToJson(police);
            foreach(var name in Models)Configure(name);
            int count=0;
            foreach(var vehicle in game.vehicles.Where(v=>v.halfWidth>.7f))
            {
                var root=vehicle.model;var driver=root.GetComponent<CarVisualAnimator>();string role=driver.AssetName;
                var lamp=game.daylight.vehicles.Single(v=>v.root==root);
                // Existing optical objects/overlays remain shared by this pool slot.
                var anchors=new[]{lamp.head}.Concat(lamp.halos.Select(h=>h.source)).Where(t=>t!=null).Distinct().ToArray();
                foreach(var a in anchors)a.SetParent(root,true);
                foreach(var old in root.Cast<Transform>().ToArray())
                {
                    if(old.name=="Biển xe nguy hiểm"||anchors.Contains(old))continue;
                    UnityEngine.Object.DestroyImmediate(old.gameObject);
                }
                vehicle.brakeLights=null;
                var component=root.GetComponent<MeshyCarVariants>()??root.gameObject.AddComponent<MeshyCarVariants>();component.driver=driver;
                component.headAnchor=lamp.head;component.frontHalos=lamp.halos.Where(h=>!h.rear).Select(h=>h.source).ToArray();component.tailHalos=lamp.halos.Where(h=>h.rear).Select(h=>h.source).ToArray();
                string[] names=role=="PV_BoxTruck"||role=="PV_DrowsyTruck"?new[]{Models[2],Models[3]}:role=="PV_DeliveryVan"?new[]{Models[1]}:new[]{Models[0]};
                component.variants=names.Select(name=>Create(root,name)).ToArray();component.Select(0,count%8,vehicle);
                var sign=root.Find("Biển xe nguy hiểm");if(sign!=null)sign.localPosition=new Vector3(0,component.Height+.4f,0);
                EditorUtility.SetDirty(component);EditorUtility.SetDirty(driver);count++;
            }
            var materials=Models.Select(n=>AssetDatabase.LoadAssetAtPath<Material>(Folder+n+".mat")).ToArray();
            game.daylight.emissionMaterials=game.daylight.emissionMaterials.Where(m=>m!=null&&!PhoVietCarUpgrade.Assets.Any(a=>m.name==a+"_Atlas")).Concat(materials).Distinct().ToArray();
            game.daylight.emissionUsers=game.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&(game.daylight.emissionMaterials.Contains(m)||m==game.daylight.streetLens))).ToArray();
            // Bridge previews stay available in Editor without entering the runtime build.
            foreach(var root in game.gameObject.scene.GetRootGameObjects().Where(o=>o!=game.gameObject))
                if(root.GetComponentsInChildren<Renderer>(true).Any(r=>AssetDatabase.GetAssetPath((r as SkinnedMeshRenderer)?.sharedMesh??r.GetComponent<MeshFilter>()?.sharedMesh).StartsWith("Assets/MeshyImports/",StringComparison.Ordinal)))
                {root.tag="EditorOnly";root.SetActive(false);}
            if(policeBefore!=EditorJsonUtility.ToJson(police))throw new Exception("Police changed");
            if(game.jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Jobs must stay locked");
            EditorUtility.SetDirty(game);EditorUtility.SetDirty(game.daylight);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
            return new{success=true,count,models=Models,heavyVariants=2,policeUnchanged=true,jobsEnabled=game.jobsEnabled};
        }
        static void Configure(string name)
        {
            var i=(ModelImporter)AssetImporter.GetAtPath(Folder+name+".fbx");i.animationType=ModelImporterAnimationType.Legacy;i.importAnimation=true;i.optimizeGameObjects=false;i.materialImportMode=ModelImporterMaterialImportMode.None;
            i.importCameras=false;i.importLights=false;i.importVisibility=false;i.addCollider=false;i.globalScale=1;i.meshCompression=ModelImporterMeshCompression.Off;i.isReadable=false;
            i.animationCompression=ModelImporterAnimationCompression.KeyframeReduction;i.animationRotationError=.15f;i.animationPositionError=.002f;i.animationScaleError=.01f;
            var takes=i.defaultClipAnimations;
            foreach(var c in takes){string n=PhoVietCarUpgrade.ClipNames.FirstOrDefault(n=>c.name.EndsWith(n,StringComparison.Ordinal));if(n==null)continue;c.name=n;c.loopTime=n=="Idle"||n=="Drive";c.wrapMode=c.loopTime?WrapMode.Loop:WrapMode.ClampForever;}
            if(takes.Length!=7)throw new Exception(name+": seven source takes required");i.clipAnimations=takes;i.SaveAndReimport();
            foreach(string suffix in new[]{"Palette","Normal","Surface","PaintMask","Emission"})
            {
                var t=(TextureImporter)AssetImporter.GetAtPath(Folder+name+"_"+suffix+".png");t.textureType=suffix=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=suffix=="Palette"||suffix=="Emission";
                t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.anisoLevel=4;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.CompressedHQ;t.compressionQuality=100;t.isReadable=false;t.wrapMode=TextureWrapMode.Clamp;t.SaveAndReimport();
            }
            string path=Folder+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find("PhoViet/Meshy Car Paint"));AssetDatabase.CreateAsset(mat,path);}
            mat.shader=Shader.Find("PhoViet/Meshy Car Paint");mat.name=name+"_Atlas";mat.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture>(Folder+name+"_Palette.png"));mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture>(Folder+name+"_Normal.png"));mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture>(Folder+name+"_Surface.png"));mat.SetTexture("_PaintMask",AssetDatabase.LoadAssetAtPath<Texture>(Folder+name+"_PaintMask.png"));mat.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture>(Folder+name+"_Emission.png"));mat.SetColor("_EmissionColor",Color.black);mat.SetColor("_PaintColor",Color.white);mat.EnableKeyword("_EMISSION");EditorUtility.SetDirty(mat);
        }
        static MeshyCarVariants.Variant Create(Transform root,string name)
        {
            var info=JsonUtility.FromJson<Metadata>(File.ReadAllText(Folder+name+".json"));
            var profile=JsonUtility.FromJson<RoofData>(File.ReadAllText(Folder+name+"_Roof.json"));
            var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+name+".fbx"),root);go.name="Meshy • "+name;
            go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
            var skins=go.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(s=>s.name.Contains("_LOD2")?2:s.name.Contains("_LOD1")?1:0).ToArray();if(skins.Length!=3)throw new Exception(name+": LOD count");
            foreach(var s in skins){s.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+name+".mat");s.quality=SkinQuality.Bone1;s.enabled=true;s.updateWhenOffscreen=false;s.shadowCastingMode=ShadowCastingMode.On;s.receiveShadows=true;s.localBounds=new Bounds(new Vector3(0,info.size[1]/2,0),V(info.size)+Vector3.one*.5f);}
            var lod=go.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.19f,new Renderer[]{skins[0]}),new LOD(.075f,new Renderer[]{skins[1]}),new LOD(.006f,new Renderer[]{skins[2]})});lod.localReferencePoint=new Vector3(0,info.size[1]/2,0);lod.size=info.size[2];
            var player=go.GetComponent<Animation>()??go.AddComponent<Animation>();player.playAutomatically=false;player.cullingType=AnimationCullingType.AlwaysAnimate;player.Stop();
            var clips=AssetDatabase.LoadAllAssetsAtPath(Folder+name+".fbx").OfType<AnimationClip>().Where(c=>PhoVietCarUpgrade.ClipNames.Contains(c.name)).ToArray();if(clips.Length!=7)throw new Exception(name+": clips");
            var bones=go.GetComponentsInChildren<Transform>(true);var suffix=new List<string>();for(int a=0;a<info.axles.Length;a++){string end=a==0?"F":a==1?"R":"R"+a;suffix.Add(end+"L");suffix.Add(end+"R");}
            return new MeshyCarVariants.Variant{asset=name,visual=go.transform,body=bones.Single(t=>t.name=="Body"),bumperFront=bones.Single(t=>t.name=="BumperFront"),bumperRear=bones.Single(t=>t.name=="BumperRear"),
                wheels=suffix.Select(s=>bones.Single(t=>t.name=="Wheel"+s)).ToArray(),steering=suffix.Select(s=>bones.Single(t=>t.name=="Steer"+s)).ToArray(),skins=skins,animationPlayer=player,clips=clips,size=V(info.size),wheelRadius=info.wheel_radius,headlight=V(info.headlight),taillight=V(info.taillight),roofProfile=profile.samples};
        }
    }
}
