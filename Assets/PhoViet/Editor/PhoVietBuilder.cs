using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using static PhoViet.StreetArt;

namespace PhoViet.Editor
{
    public static class PhoVietBuilder
    {
        [MenuItem("Phố Việt/Dựng lại scene chính")]
        public static void Build()
        {
            PhoVietBikeUpgrade.ConfigureParkedFactory();
            if(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/Streets/PV_StreetShop00.fbx")!=null)PhoVietStreetUpgrade.ConfigureFactories();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("PHỐ VIỆT • Chuyến xe không ngừng");var game=root.AddComponent<PhoVietGame>();
            game.vietnameseFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/PhoViet/Fonts/BeVietnamPro-SemiBold.ttf");
            if(game.vietnameseFont==null)throw new System.Exception("Missing Vietnamese font");
            game.BuildWorld();
            AttachActions(game);
            AttachReckless(game);
            AttachNormalTrucks(game);
            foreach(var b in game.blocks)Combine(b);
            CombineRider(game);foreach(var v in game.vehicles)Combine(v.model);
            Combine(game.policeBike);Combine(game.jailRoom);
            PhoVietBikeUpgrade.ApplyToGame(game);
            PhoVietCarUpgrade.ApplyToGame(game);
            PhoVietRouteUpgrade.ApplyToGame(game);
            var sunlight=new GameObject("Nắng chiều Sài Gòn").AddComponent<Light>();sunlight.type=LightType.Directional;
            sunlight.transform.rotation=Quaternion.Euler(48,-32,0);sunlight.color=Hex("FFE3AE");sunlight.intensity=.95f;
            sunlight.shadows=LightShadows.Soft;sunlight.shadowStrength=.63f;sunlight.shadowBias=.055f;sunlight.shadowNormalBias=.3f;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=Hex("C9DFDE");RenderSettings.ambientEquatorColor=Hex("B6B9A0");RenderSettings.ambientGroundColor=Hex("7D8170");
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=Hex("CCD9CF");RenderSettings.fogStartDistance=70;RenderSettings.fogEndDistance=175;
            RenderSettings.sun=sunlight;
            Box("Nền thành phố",root.transform,new Vector3(0,-.4f,100),new Vector3(600,.3f,800),Hex("92A68A"));
            QualitySettings.shadowDistance=100;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.antiAliasing=4;
            QualitySettings.vSyncCount=0;
            PersistGeometry(root);
            PlayerSettings.companyName="Pho Viet Studio";PlayerSettings.productName="Phố Việt - Chuyến xe không ngừng";
            PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.runInBackground=true;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
            PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            const string scene="Assets/PhoViet/Scenes/PhoViet.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene,true)};AssetDatabase.SaveAssets();
            Selection.activeGameObject=root;
            if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.LookAt(new Vector3(0,1,15),Quaternion.Euler(30,0,0),32);
            Debug.Log("PHỐ VIỆT READY • Scene saved • "+game.blocks.Count+" blocks • "+game.vehicles.Count+" pooled vehicles");
        }
        [MenuItem("Phố Việt/Cập nhật đạp xe và truy đuổi")]
        public static void UpgradeActions()
        {
            if(Application.isPlaying)throw new System.Exception("Exit Play Mode before upgrading scene");
            var game=Object.FindAnyObjectByType<PhoVietGame>();
            if(game==null)throw new System.Exception("Open PhoViet scene first");
            StreetArt.Font=game.vietnameseFont;
            if(game.rider.Find("Chân đạp trái")==null)
            {
                Vector3 pos=game.rider.localPosition;Object.DestroyImmediate(game.rider.gameObject);
                game.rider=Motorcycle(game.transform,"Tài xế • áo đỏ sao vàng",Red,Red,true);game.rider.localPosition=pos;
                CombineRider(game);
            }
            AttachActions(game);
            if(game.policeBike.GetComponentsInChildren<MeshFilter>(true).Length>15)Combine(game.policeBike);
            if(game.jailRoom.GetComponentsInChildren<MeshFilter>(true).Length>15)Combine(game.jailRoom);
            PersistGeometry(game.gameObject);EditorUtility.SetDirty(game);
            PlayerSettings.bundleVersion="1.2.0";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static void AttachActions(PhoVietGame game)
        {
            if(game.policeBike==null)game.policeBike=PursuitArt.Police(game.transform);
            if(game.jailRoom==null)game.jailRoom=PursuitArt.Jail(game.transform);
            game.leftKickLeg=game.rider.Find("Chân đạp trái");game.rightKickLeg=game.rider.Find("Chân đạp phải");
            game.redBeacon=game.policeBike.Find("Đèn ưu tiên đỏ").GetComponent<Renderer>();
            game.blueBeacon=game.policeBike.Find("Đèn ưu tiên xanh").GetComponent<Renderer>();
        }
        [MenuItem("Phố Việt/Cập nhật quái xế và xe tải")]
        public static void UpgradeReckless()
        {
            if(Application.isPlaying)throw new System.Exception("Exit Play Mode first");
            var game=Object.FindAnyObjectByType<PhoVietGame>();
            if(game==null)throw new System.Exception("Open PhoViet scene first");
            StreetArt.Font=game.vietnameseFont;AttachReckless(game);
            PersistGeometry(game.gameObject);EditorUtility.SetDirty(game);PlayerSettings.bundleVersion="1.3.0";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static void AttachReckless(PhoVietGame game)
        {
            if(game.recklessBikeModel==null){game.recklessBikeModel=RecklessArt.Exciter(game.transform);Combine(game.recklessBikeModel);}
            if(game.recklessTruckModel==null){game.recklessTruckModel=RecklessArt.Truck(game.transform);Combine(game.recklessTruckModel);}
            StreetArt.Font=game.vietnameseFont;
            game.recklessTruckModel.name="Xe tải ngủ gật";
            foreach(var model in new[]{game.recklessBikeModel,game.recklessTruckModel})
            {
                bool truck=model==game.recklessTruckModel;
                var sign=model.Find("Biển xe nguy hiểm");
                if(sign==null)
                {
                    sign=Group("Biển xe nguy hiểm",model,new Vector3(0,truck?3.6f:2.85f,truck?-1.5f:0));
                    Box("Nền tên xe",sign,Vector3.zero,new Vector3(truck?3.4f:1.75f,.45f,.08f),Ink);
                    Text(sign,"Tên xe",truck?"XE TẢI NGỦ GẬT":"QUÁI XẾ",new Vector3(0,0,-.05f),truck?.21f:.18f,Gold);
                }
                if(sign.Find("Tên xe mặt trước")==null)
                    Text(sign,"Tên xe mặt trước",truck?"XE TẢI NGỦ GẬT":"QUÁI XẾ",new Vector3(0,0,.05f),truck?.21f:.18f,Gold,new Vector3(0,180,0));
            }
            foreach(var model in new[]{game.recklessBikeModel,game.recklessTruckModel})
            {
                var v=game.vehicles.Find(t=>t.model==model);
                if(v==null){v=new PhoVietGame.TrafficVehicle {model=model,eventOnly=true,halfWidth=model==game.recklessTruckModel?1.2f:.48f,halfLength=model==game.recklessTruckModel?3.4f:1.1f};game.vehicles.Add(v);}
                if(v.crashEffect!=null&&v.crashEffect.Find("Ngọn lửa")==null){Object.DestroyImmediate(v.crashEffect.gameObject);v.crashEffect=null;}
                if(v.crashEffect==null)v.crashEffect=RecklessArt.CrashFx(game.transform,out v.smoke,out v.fire);
                foreach(var renderer in v.crashEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
                {
                    renderer.sharedMaterial.shader=Shader.Find("PhoViet/Soft Particle");
                    renderer.SetActiveVertexStreams(new List<ParticleSystemVertexStream>{ParticleSystemVertexStream.Position,ParticleSystemVertexStream.Color,ParticleSystemVertexStream.UV});
                    EditorUtility.SetDirty(renderer.sharedMaterial);
                }
                model.gameObject.SetActive(false);
            }
        }
        [MenuItem("Phố Việt/Cập nhật xe tải ngủ gật")]
        public static void UpgradeDrowsyTraffic()
        {
            if(Application.isPlaying)throw new System.Exception("Exit Play Mode first");
            var game=Object.FindAnyObjectByType<PhoVietGame>();
            if(game==null)throw new System.Exception("Open PhoViet scene first");
            AttachReckless(game);game.recklessTrafficEnabled=true;PersistGeometry(game.gameObject);
            PlayerSettings.bundleVersion="1.7.0";EditorUtility.SetDirty(game);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        [MenuItem("Phố Việt/Cập nhật giao thông bất ngờ")]
        public static void UpgradeTraffic()
        {
            if(Application.isPlaying)throw new System.Exception("Exit Play Mode first");
            var game=Object.FindAnyObjectByType<PhoVietGame>();
            if(game==null)throw new System.Exception("Open PhoViet scene first");
            AttachNormalTrucks(game);game.trafficSurprises=true;PlayerSettings.bundleVersion="1.4.0";
            EditorUtility.SetDirty(game);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static void AttachNormalTrucks(PhoVietGame game)
        {
            int count=game.vehicles.FindAll(v=>!v.eventOnly&&v.halfLength>3).Count;
            for(int i=count;i<2;i++)
            {
                var model=Object.Instantiate(game.recklessTruckModel,game.transform);model.name="Xe tải cùng chiều • "+(i+1);model.gameObject.SetActive(false);
                var dangerSign=model.Find("Biển xe nguy hiểm");if(dangerSign!=null)Object.DestroyImmediate(dangerSign.gameObject);
                game.vehicles.Add(new PhoVietGame.TrafficVehicle {model=model,halfWidth=1.2f,halfLength=3.4f,eventOnly=false});
            }
        }
        static void CombineRider(PhoVietGame game)
        {
            Combine(game.rider);
            var left=game.rider.Find("Chân đạp trái");var right=game.rider.Find("Chân đạp phải");
            if(left!=null)Combine(left);if(right!=null)Combine(right);
        }
        static bool AnimatedMesh(Transform t,Transform root)
        {
            if(t.name.StartsWith("Vạch mũi tên giao lộ"))return true;
            if(t.name.StartsWith("Đèn ưu tiên"))return true;
            for(var current=t;current!=null&&current!=root;current=current.parent)
                if(current.name.StartsWith("Chân đạp"))return true;
            return false;
        }
        internal static void Combine(Transform root)
        {
            var skeleton=new HashSet<Transform>();
            foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))foreach(var bone in skin.bones)
                for(var t=bone;t!=null&&t!=root;t=t.parent)skeleton.Add(t);
            var groups=new Dictionary<Material,List<CombineInstance>>();var old=new List<MeshFilter>();
            foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer=f.GetComponent<MeshRenderer>();
                if(renderer==null||f.sharedMesh==null||f.name.StartsWith("Tín hiệu")||f.GetComponentInParent<LODGroup>()!=null||f.name.StartsWith("Vạch mũi tên")||AnimatedMesh(f.transform,root))continue;
                Material m=renderer.sharedMaterial;if(m==null)continue;
                if(!groups.TryGetValue(m,out var list)){list=new List<CombineInstance>();groups[m]=list;}
                list.Add(new CombineInstance {mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix});old.Add(f);
            }
            foreach(var pair in groups)
            {
                var t=Group("Geometry • "+pair.Key.name,root);var mesh=new Mesh {name="PV_Combined",indexFormat=IndexFormat.UInt32};
                mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.RecalculateBounds();
                t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
            }
            foreach(var f in old){Object.DestroyImmediate(f.GetComponent<MeshRenderer>());Object.DestroyImmediate(f);}
            // Remove only empty leaves; text, lights, pool roots and semantic shop groups survive.
            for(int pass=0;pass<4;pass++)foreach(var t in root.GetComponentsInChildren<Transform>(true))
                if(t!=root&&!skeleton.Contains(t)&&t.childCount==0&&t.GetComponents<Component>().Length==1)Object.DestroyImmediate(t.gameObject);
        }
        internal static void PersistGeometry(GameObject root)
        {
            string folder="Assets/PhoViet/Generated";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/PhoViet","Generated");
            var saved=new HashSet<Object>();int n=0;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            foreach(var mat in renderer.sharedMaterials)
            {
                if(mat==null||!mat.name.StartsWith("PV_")||AssetDatabase.Contains(mat)||!saved.Add(mat))continue;
                if(mat.mainTexture!=null&&!AssetDatabase.Contains(mat.mainTexture)&&saved.Add(mat.mainTexture))
                    AssetDatabase.CreateAsset(mat.mainTexture,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+mat.mainTexture.name+".asset"));
                AssetDatabase.CreateAsset(mat,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+mat.name+".mat"));
            }
            foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh=f.sharedMesh;if(mesh==null||AssetDatabase.Contains(mesh)||!saved.Add(mesh))continue;
                AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath(folder+"/Geometry_"+(n++)+".asset"));
            }
        }
    }
}
