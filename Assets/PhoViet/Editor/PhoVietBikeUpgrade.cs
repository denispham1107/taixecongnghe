using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.Editor
{
    public static class PhoVietBikeUpgrade
    {
        const string Folder="Assets/PhoViet/Art/Bikes/";
        public static readonly string[] Assets={"PV_HeroScooter","PV_CityScooter","PV_Underbone","PV_SportBike","PV_Racer","PV_TrafficPolice"};
        static readonly string[] ClipNames={"Idle","Ride","KickLeft","KickRight","FallLeft","FallRight","Recover"};
        // The Meshy replacement is isolated from the previous Blender sources and police model.
        public static string ModelPath(string asset)
        {
            string rider=Folder+"MeshyRiders/"+asset+".fbx";
            if(System.IO.File.Exists(rider))return rider;
            string path=Folder+"Meshy/"+asset+".fbx";
            return System.IO.File.Exists(path)?path:Folder+asset+".fbx";
        }
        public static bool IsMeshyPath(string path)=>path.Contains("/Meshy/")||path.Contains("/MeshyRiders/");

        [MenuItem("Phố Việt/Art/Replace all motorcycles with Blender models")]
        public static void UpgradeOpenScene()
        {
            if(Application.isPlaying)throw new Exception("Exit Play Mode first");
            var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
            if(game==null)throw new Exception("Open PhoViet scene first");
            PhoVietBikeImport.ConfigureAll();ApplyToGame(game);
            EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
            EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("Blender motorcycle visuals installed; traffic and crash rules retained.");
        }
        public static void ApplyToGame(PhoVietGame game)
        {
            foreach(string asset in Assets)if(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(asset))==null)throw new Exception("Missing Blender asset "+asset);
            UpgradeParkedBikes(game);
            Replace(game.rider,"PV_HeroScooter",BikeVisualAnimator.ActorRole.Player);
            game.leftKickLeg=StreetArt.Group("Chân đạp trái",game.rider,new Vector3(-.17f,1.08f,-.25f));
            game.rightKickLeg=StreetArt.Group("Chân đạp phải",game.rider,new Vector3(.17f,1.08f,-.25f));
            int index=0;
            foreach(var v in game.vehicles)
            {
                if(v.halfWidth>.7f)continue;
                string asset=v.eventOnly?"PV_Racer":new[]{"PV_CityScooter","PV_Underbone","PV_SportBike"}[index++%3];
                Replace(v.model,asset,BikeVisualAnimator.ActorRole.Traffic);
                var sign=v.model.Find("Biển xe nguy hiểm");if(sign!=null)sign.localPosition=new Vector3(0,2.6f,0);
            }
            if(game.policeBike!=null)
            {
                Replace(game.policeBike,"PV_TrafficPolice",BikeVisualAnimator.ActorRole.Police);
                foreach(var pair in new[]{(game.redBeacon,-1),(game.blueBeacon,1)})
                {
                    if(pair.Item1==null)continue;var light=pair.Item1.transform;
                    light.localPosition=new Vector3(pair.Item2*.29f,1.39f,.71f);light.localScale=new Vector3(.18f,.20f,.18f);
                }
            }
        }
        public static void ConfigureParkedFactory()
        {
            int index=0;string[] assets={"PV_CityScooter","PV_Underbone","PV_SportBike"};
            StreetArt.ParkedBikeFactory=(parent,name)=>
            {
                var root=StreetArt.Group(name,parent);Replace(root,assets[index++%assets.Length],BikeVisualAnimator.ActorRole.Parked);return root;
            };
        }
        static void UpgradeParkedBikes(PhoVietGame game)
        {
            if(game.scenery.GetComponentsInChildren<BikeVisualAnimator>(true).Count(d=>d.role==BikeVisualAnimator.ActorRole.Parked)==game.blocks.Count*2)return;
            // Old parked bikes were baked into each block. Rebuild the same canonical block
            // without that old geometry, reusing its persisted mesh/material assets by name.
            ConfigureParkedFactory();StreetArt.Font=game.vietnameseFont;game.trafficLights.Clear();
            for(int i=0;i<game.blocks.Count;i++)
            {
                var old=game.blocks[i];var original=old.GetComponentsInChildren<MeshFilter>(true)
                    .Where(f=>f.name.StartsWith("Geometry")&&f.GetComponent<MeshRenderer>()!=null)
                    .GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial.name).ToDictionary(g=>g.Key,g=>g.First());
                var block=VietnamStreet.Build(game.scenery,i,game.trafficLights);block.localPosition=old.localPosition;block.localRotation=old.localRotation;block.localScale=old.localScale;
                PhoVietBuilder.Combine(block);
                foreach(var mesh in block.GetComponentsInChildren<MeshFilter>(true))
                {
                    var renderer=mesh.GetComponent<MeshRenderer>();if(renderer==null||!mesh.name.StartsWith("Geometry"))continue;
                    if(!original.TryGetValue(renderer.sharedMaterial.name,out var previous))continue;
                    string path=AssetDatabase.GetAssetPath(previous.sharedMesh);if(!path.StartsWith("Assets/PhoViet/Generated/",StringComparison.Ordinal))continue;
                    var temporary=mesh.sharedMesh;EditorUtility.CopySerialized(temporary,previous.sharedMesh);EditorUtility.SetDirty(previous.sharedMesh);
                    mesh.sharedMesh=previous.sharedMesh;renderer.sharedMaterial=previous.GetComponent<MeshRenderer>().sharedMaterial;
                    UnityEngine.Object.DestroyImmediate(temporary);
                }
                game.blocks[i]=block;UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            PhoVietBuilder.PersistGeometry(game.gameObject);
        }
        static void Replace(Transform root,string asset,BikeVisualAnimator.ActorRole role)
        {
            // Keep the existing pool/physics transform and semantic danger/beacon objects.
            for(int i=root.childCount-1;i>=0;i--)
            {
                var child=root.GetChild(i);
                if(child.name=="Biển xe nguy hiểm"||child.name.StartsWith("Đèn ưu tiên",StringComparison.Ordinal))continue;
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(asset));
            var visual=(GameObject)PrefabUtility.InstantiatePrefab(model,root);visual.name="Blender • "+asset;
            visual.transform.localPosition=Vector3.zero;visual.transform.localRotation=Quaternion.identity;visual.transform.localScale=Vector3.one;
            var rendererList=visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(r=>r.name.Contains("_LOD1")?1:r.name.Contains("_LOD2")?2:0).ToArray();
            if(rendererList.Length!=3)throw new Exception(asset+" must contain three skinned LODs");
            var material=AtlasMaterial(asset);
            foreach(var renderer in rendererList)
            {
                renderer.sharedMaterial=material;renderer.enabled=true;renderer.quality=SkinQuality.Bone2;
                renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;renderer.updateWhenOffscreen=false;
                renderer.localBounds=new Bounds(new Vector3(0,1.1f,0),new Vector3(3.3f,3.0f,3.0f));
            }
            foreach(var old in visual.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(old);
            var lod=visual.AddComponent<LODGroup>();lod.fadeMode=LODFadeMode.None;
            lod.SetLODs(new[]{new LOD(.16f,new Renderer[]{rendererList[0]}),new LOD(.055f,new Renderer[]{rendererList[1]}),new LOD(.008f,new Renderer[]{rendererList[2]})});
            lod.localReferencePoint=new Vector3(0,1.1f,0);lod.size=2.25f;
            var player=visual.GetComponent<Animation>()??visual.AddComponent<Animation>();player.playAutomatically=false;player.cullingType=AnimationCullingType.AlwaysAnimate;player.Stop();
            // Reuse the validated game clips directly: re-baking FBX a second time
            // changes local rotation curves even though the semantic rig is identical.
            string clipPath=ModelPath(asset).Contains("/MeshyRiders/")?Folder+"Meshy/"+asset+".fbx":ModelPath(asset);
            var clips=AssetDatabase.LoadAllAssetsAtPath(clipPath).OfType<AnimationClip>().Where(c=>ClipNames.Contains(c.name)).ToArray();
            if(clips.Length!=7)throw new Exception(asset+" must have exactly seven semantic clips");
            var driver=root.GetComponent<BikeVisualAnimator>()??root.gameObject.AddComponent<BikeVisualAnimator>();
            driver.visual=visual.transform;driver.animationPlayer=player;driver.clips=clips;driver.role=role;driver.AssetName=asset;
            var bones=visual.GetComponentsInChildren<Transform>(true);
            driver.frontWheel=bones.Single(t=>t.name=="WheelFront");driver.rearWheel=bones.Single(t=>t.name=="WheelRear");
            var exhaust=PhoVietMeshyBikeUpgrade.Mount(asset,"exhaust_tip");
            if(exhaust.HasValue)
            {
                var socket=new GameObject("PV_MeshyExhaust").transform;socket.SetParent(bones.Single(t=>t.name=="Frame"),false);
                socket.position=root.TransformPoint(exhaust.Value);socket.rotation=root.rotation;
            }
        }
        static Material AtlasMaterial(string asset)
        {
            string folder=System.IO.Path.GetDirectoryName(ModelPath(asset)).Replace('\\','/')+"/";
            string surfacePath=System.IO.File.Exists(folder+"Meshy_Surface.png")?folder+"Meshy_Surface.png":folder+asset+"_Surface.png";
            Texture2D texture=ImportTexture(folder+asset+"_Palette.png",true),surface=ImportTexture(surfacePath,false);
            string path=folder+asset+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,path);}
            material.name=asset+"_Atlas";material.mainTexture=texture;material.color=Color.white;
            material.SetTexture("_MetallicGlossMap",surface);material.EnableKeyword("_METALLICGLOSSMAP");material.SetFloat("_GlossMapScale",1);
            if(IsMeshyPath(folder))
            {
                material.SetTexture("_BumpMap",ImportTexture(folder+"Meshy_Normal.png",false,true));material.EnableKeyword("_NORMALMAP");material.SetFloat("_BumpScale",1);
                material.SetTexture("_EmissionMap",ImportTexture(folder+"Meshy_Emission.png",true));material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",Color.black);
            }
            material.SetFloat("_Glossiness",.35f);EditorUtility.SetDirty(material);return material;
        }
        static Texture2D ImportTexture(string path,bool srgb,bool normal=false)
        {
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new Exception("Missing texture "+path);
            bool meshy=IsMeshyPath(path);var filter=meshy?FilterMode.Bilinear:FilterMode.Point;int size=meshy?2048:128;
            var type=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
            if(importer.sRGBTexture!=srgb||importer.filterMode!=filter||importer.wrapMode!=TextureWrapMode.Clamp||importer.mipmapEnabled!=meshy||importer.maxTextureSize!=size||importer.textureType!=type)
            {
                importer.textureType=type;importer.sRGBTexture=srgb;importer.filterMode=filter;importer.wrapMode=TextureWrapMode.Clamp;
                importer.mipmapEnabled=meshy;importer.alphaIsTransparency=false;importer.maxTextureSize=size;importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
