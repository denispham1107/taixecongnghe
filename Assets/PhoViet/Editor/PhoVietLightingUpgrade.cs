using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using static PhoViet.StreetArt;

namespace PhoViet.Editor
{
    public static class PhoVietLightingUpgrade
    {
        const string Folder="Assets/PhoViet/Art/Lighting/";
        static Mesh floorQuad,haloQuad;
        static Material headPool,streetPool,whiteGlow,redGlow,lens;
        static Transform opticalRoot;
        [MenuItem("Phố Việt/Ánh sáng/Install Blender day-night lighting")]
        public static void Main()
        {
            if(Application.isPlaying)throw new Exception("Exit Play Mode first");
            var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null)throw new Exception("Open PhoViet scene first");
            AssetDatabase.Refresh();ConfigureTextures();
            if(g.daylight!=null)UnityEngine.Object.DestroyImmediate(g.daylight);
            var old=g.transform.Find("Ánh sáng động • Blender MCP");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            opticalRoot=Group("Ánh sáng động • Blender MCP",g.transform);
            floorQuad=Quad("PV_OpticalGround",true);haloQuad=Quad("PV_OpticalHalo",false);
            headPool=Optical("PV_HeadlightSpill","PV_HeadlightCookie",new Color(.75f,.88f,1,.28f));
            streetPool=Optical("PV_StreetSpill","PV_StreetPool",new Color(.83f,.92f,1,.44f));
            whiteGlow=Optical("PV_WhiteGlare","PV_OpticalGlare",new Color(.88f,.95f,1,.22f));
            redGlow=Optical("PV_RedGlare","PV_OpticalGlare",new Color(1,.025f,.05f,.4f));
            lens=Standard("PV_StreetLEDLens","F4FAFF",.1f,.85f);lens.EnableKeyword("_EMISSION");
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"PV_LEDStreetLamp.fbx");if(model==null)throw new Exception("Blender LED model missing");
            var metal=Standard("PV_StreetLEDMetal","344553",.4f,.5f);var silver=Standard("PV_StreetLEDSilver","BFCED8",.45f,.55f);
            var streets=new List<DayNightLighting.StreetLamp>();var cleaned=new Dictionary<Mesh,Mesh>();int trianglesRemoved=0;
            foreach(var row in g.roadRows)foreach(var block in row.tiles)
            {
                var previous=block.Find("Chiếu sáng phố • Blender");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
                foreach(var filter in block.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(filter.transform.parent!=block||!filter.name.StartsWith("Geometry • ",StringComparison.Ordinal)||filter.sharedMesh==null)continue;
                    if(!cleaned.TryGetValue(filter.sharedMesh,out var clean)){clean=RemoveOldFixture(filter.sharedMesh,ref trianglesRemoved);cleaned.Add(filter.sharedMesh,clean);}filter.sharedMesh=clean;
                }
                var group=Group("Chiếu sáng phố • Blender",block);
                foreach(float side in new[]{-1f,1f})
                {
                    var unit=Group("Đèn LED • "+side,group,new Vector3(side*7.6f,0,28));unit.localRotation=Quaternion.Euler(0,side>0?180:0,0);
                    var instance=(GameObject)PrefabUtility.InstantiatePrefab(model,unit);instance.name="Blender • đèn đường LED";
                    instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=model.transform.localRotation;instance.transform.localScale=model.transform.localScale;
                    foreach(var renderer in instance.GetComponentsInChildren<MeshRenderer>(true)){renderer.sharedMaterials=new[]{metal,silver,lens};renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;}
                    var lodRenderers=instance.GetComponentsInChildren<MeshRenderer>(true).OrderBy(r=>r.name.Contains("_LOD2")?2:r.name.Contains("_LOD1")?1:0).ToArray();
                    if(lodRenderers.Length==3)
                    {
                        var lod=instance.GetComponent<LODGroup>();if(lod==null)lod=instance.AddComponent<LODGroup>();
                        lod.SetLODs(new[]{new LOD(.13f,new Renderer[]{lodRenderers[0]}),new LOD(.05f,new Renderer[]{lodRenderers[1]}),new LOD(.005f,new Renderer[]{lodRenderers[2]})});lod.RecalculateBounds();
                    }
                    var source=Group("Tâm quang học LED",unit,new Vector3(1.45f,6.64f,0));source.localRotation=Quaternion.Euler(90,0,0);
                    var pool=Overlay("Vệt sáng đèn đường",group,streetPool,true);pool.transform.localPosition=new Vector3(side*6.15f,.038f,28);pool.transform.localScale=new Vector3(18,1,18);
                    var glow=Overlay("Quầng đèn đường",group,whiteGlow,false);glow.transform.localScale=Vector3.one*.9f;
                    streets.Add(new DayNightLighting.StreetLamp{source=source,pool=pool,glow=glow});
                }
            }
            var vehicles=new List<DayNightLighting.VehicleLamps>();
            foreach(var bike in g.GetComponentsInChildren<BikeVisualAnimator>(true).Where(b=>b.role!=BikeVisualAnimator.ActorRole.Parked))vehicles.Add(Bike(bike,g));
            foreach(var car in g.GetComponentsInChildren<CarVisualAnimator>(true))vehicles.Add(Car(car));
            var emission=new List<Material>();
            foreach(var path in PhoVietBikeUpgrade.Assets.Select(a=>System.IO.Path.ChangeExtension(PhoVietBikeUpgrade.ModelPath(a),"mat").Replace('\\','/')).Concat(PhoVietCarUpgrade.Assets.Select(a=>"Assets/PhoViet/Art/Cars/"+a+".mat")))
            {
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null)throw new Exception("Missing vehicle material: "+path);
                string emissionPath=PhoVietBikeUpgrade.IsMeshyPath(path)?System.IO.Path.GetDirectoryName(path).Replace('\\','/')+"/Meshy_Emission.png":Folder+(path.Contains("/Bikes/")?"PV_BikeEmission":"PV_CarEmission")+".png";
                material.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(emissionPath));
                material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",Color.black);EditorUtility.SetDirty(material);emission.Add(material);
            }
            ApplyColorfulStreetPaint(g);
            var lighting=g.gameObject.AddComponent<DayNightLighting>();g.daylight=lighting;lighting.game=g;lighting.authoredProfile=AssetDatabase.LoadAssetAtPath<TextAsset>(Folder+"PV_LightingProfile.json");
            lighting.sun=RenderSettings.sun!=null?RenderSettings.sun:UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(l=>l.type==LightType.Directional);
            lighting.sun.name="Nắng ban mai • chu kỳ ngày đêm";lighting.sun.shadows=LightShadows.Soft;lighting.sun.shadowStrength=.65f;
            var moonGo=Group("Ánh trăng",opticalRoot);lighting.moon=moonGo.gameObject.AddComponent<Light>();lighting.moon.type=LightType.Directional;lighting.moon.shadows=LightShadows.None;
            lighting.sky=Asset<Material>("PV_DayNightSky.mat",()=>new Material(Shader.Find("PhoViet/Day Night Sky")));
            lighting.neutralReflection=Reflection();lighting.streetLens=lens;lighting.vehicles=vehicles.ToArray();lighting.streets=streets.ToArray();lighting.emissionMaterials=emission.ToArray();
            lighting.emissionUsers=g.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&(emission.Contains(m)||m==lens))).ToArray();
            lighting.lightPool=new Light[6];
            for(int i=0;i<6;i++)
            {
                var t=Group("Nguồn sáng gần camera • "+i,opticalRoot);var l=t.gameObject.AddComponent<Light>();l.type=LightType.Spot;l.renderMode=i==0?LightRenderMode.ForcePixel:LightRenderMode.Auto;
                l.shadowBias=.05f;l.shadowNormalBias=.2f;l.enabled=false;lighting.lightPool[i]=l;
            }
            QualitySettings.pixelLightCount=4;lighting.Initialize(g);lighting.ResetCycle();
            foreach(var m in emission)EditorUtility.SetDirty(m);EditorUtility.SetDirty(lighting.sky);EditorUtility.SetDirty(lens);
            EditorUtility.SetDirty(g);EditorUtility.SetDirty(lighting);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("Blender daylight installed: "+streets.Count+" street fixtures, "+vehicles.Count+" vehicles, six bounded real lights; removed "+trianglesRemoved+" old lamp triangles.");
        }
        static void ConfigureTextures()
        {
            foreach(string file in new[]{"PV_HeadlightCookie","PV_StreetPool","PV_OpticalGlare","PV_BikeEmission","PV_CarEmission"})
            {
                var importer=AssetImporter.GetAtPath(Folder+file+".png") as TextureImporter;if(importer==null)throw new Exception("Missing Blender texture "+file);
                bool palette=file.Contains("Emission")||file.Contains("Palette");importer.sRGBTexture=true;importer.mipmapEnabled=!palette;importer.wrapMode=TextureWrapMode.Clamp;
                importer.filterMode=palette?FilterMode.Point:FilterMode.Bilinear;importer.maxTextureSize=palette?128:512;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            var model=AssetImporter.GetAtPath(Folder+"PV_LEDStreetLamp.fbx") as ModelImporter;if(model==null)throw new Exception("Missing Blender lamp");
            model.materialImportMode=ModelImporterMaterialImportMode.None;model.importAnimation=false;model.importCameras=false;model.importLights=false;model.addCollider=false;model.globalScale=1;model.SaveAndReimport();
        }
        static T Asset<T>(string file,Func<T> create) where T:UnityEngine.Object
        {var value=AssetDatabase.LoadAssetAtPath<T>(Folder+file);if(value==null){value=create();AssetDatabase.CreateAsset(value,Folder+file);}return value;}
        static Material Standard(string name,string hex,float metal,float gloss)
        {var m=Asset(name+".mat",()=>new Material(Shader.Find("Standard")));m.name=name;m.color=Hex(hex);m.SetFloat("_Metallic",metal);m.SetFloat("_Glossiness",gloss);EditorUtility.SetDirty(m);return m;}
        static Material Optical(string name,string texture,Color tint)
        {var m=Asset(name+".mat",()=>new Material(Shader.Find("PhoViet/Optical Light")));m.name=name;m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+texture+".png");m.SetColor("_Tint",tint);EditorUtility.SetDirty(m);return m;}
        static Mesh Quad(string name,bool ground)
        {
            return Asset(name+".asset",()=>
            {
                var m=new Mesh{name=name};m.vertices=ground?new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(.5f,0,.5f),new Vector3(-.5f,0,.5f)}:
                    new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};
                m.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};m.triangles=new[]{0,2,1,0,3,2};m.RecalculateNormals();m.RecalculateBounds();return m;
            });
        }
        static Renderer Overlay(string name,Transform parent,Material material,bool ground)
        {var t=Group(name,parent);t.gameObject.AddComponent<MeshFilter>().sharedMesh=ground?floorQuad:haloQuad;var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return r;}
        static Transform Anchor(Transform root,Transform bone,string name,Vector3 position)
        {var t=Group(name,bone);t.position=root.TransformPoint(position);t.rotation=root.rotation;return t;}
        static void ClearAnchors(Transform root)
        {foreach(var t in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Quang học đèn xe • ",StringComparison.Ordinal)).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);}
        static DayNightLighting.Halo Halo(Transform source,bool rear,float size)
        {return new DayNightLighting.Halo{source=source,rear=rear,size=size,glow=Overlay(rear?"Quầng đèn hậu":"Quầng đèn pha",opticalRoot,rear?redGlow:whiteGlow,false)};}
        static DayNightLighting.VehicleLamps Bike(BikeVisualAnimator bike,PhoVietGame game)
        {
            var root=bike.transform;ClearAnchors(root);var bones=bike.visual.GetComponentsInChildren<Transform>(true);
            // Read received Meshy mounts; preserve original police and legacy Wave fallbacks.
            bool police=bike.AssetName=="PV_TrafficPolice";
            var front=bones.First(t=>t.name=="Steering");var rear=bones.First(t=>t.name=="Frame");
            var head=Anchor(root,front,"Quang học đèn xe • pha",PhoVietMeshyBikeUpgrade.Mount(bike.AssetName,"headlight")??(police?new Vector3(0,1.13f,.68f):new Vector3(0,1.095f,.80f)));
            var tail=Anchor(root,rear,"Quang học đèn xe • hậu",PhoVietMeshyBikeUpgrade.Mount(bike.AssetName,"taillight")??(police?new Vector3(0,.805f,-1.112f):new Vector3(0,.81f,-1.085f)));
            return new DayNightLighting.VehicleLamps{root=root,head=head,player=root==game.rider,roadLight=GroundLight(),halos=new[]{Halo(head,false,.7f),Halo(tail,true,.45f)}};
        }
        static Renderer GroundLight(){var r=Overlay("Vệt đèn pha trên mặt đường",opticalRoot,headPool,true);r.transform.localScale=new Vector3(32,1,32);return r;}
        static DayNightLighting.VehicleLamps Car(CarVisualAnimator car)
        {
            var root=car.transform;ClearAnchors(root);bool heavy=car.wheelRadius>.45f;float y=heavy?1.14f:.89f,z=heavy?3.28f:2.23f;
            var head=Anchor(root,car.bumperFront,"Quang học đèn xe • tâm pha",new Vector3(0,y,z));var halos=new List<DayNightLighting.Halo>();
            foreach(float side in new[]{-1f,1f})
            {
                var front=Anchor(root,car.bumperFront,"Quang học đèn xe • pha "+side,new Vector3(side*(heavy?.77f:.60f),y,z));
                var tail=Anchor(root,car.bumperRear,"Quang học đèn xe • hậu "+side,new Vector3(side*car.tailX,car.tailY,car.tailZ-.05f));
                halos.Add(Halo(front,false,.85f));halos.Add(Halo(tail,true,.6f));
            }
            return new DayNightLighting.VehicleLamps{root=root,head=head,roadLight=GroundLight(),halos=halos.ToArray()};
        }
        static Mesh RemoveOldFixture(Mesh source,ref int removed)
        {
            var vertices=source.vertices;var triangles=source.triangles;var keep=new List<int>();int skipped=0;
            Func<Vector3,bool> inside=p=>Mathf.Abs(p.x)>5.6f&&Mathf.Abs(p.x)<7.85f&&p.y>.12f&&p.y<7.12f&&Mathf.Abs(p.z-28)<.36f;
            for(int i=0;i<triangles.Length;i+=3)
            {
                if(inside(vertices[triangles[i]])&&inside(vertices[triangles[i+1]])&&inside(vertices[triangles[i+2]])){skipped++;continue;}
                keep.Add(triangles[i]);keep.Add(triangles[i+1]);keep.Add(triangles[i+2]);
            }
            if(skipped==0)return source;removed+=skipped;string guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));
            var clean=Asset("PV_NoOldLamps_"+guid+".asset",()=>UnityEngine.Object.Instantiate(source));clean.name="PV_Combined • neutral morning";clean.triangles=keep.ToArray();clean.RecalculateBounds();EditorUtility.SetDirty(clean);return clean;
        }
        [MenuItem("Phố Việt/Ánh sáng/Restore colorful street materials")]
        public static void RestoreColorfulStreetPaint()
        {
            if(Application.isPlaying)throw new Exception("Exit Play Mode first");
            var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(game==null)throw new Exception("Open PhoViet scene first");
            ApplyColorfulStreetPaint(game);
        }
        static void ApplyColorfulStreetPaint(PhoVietGame game)
        {
            // White sunlight belongs to the lighting rig; keep the authored pigments of the city.
            var atlas=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
            atlas.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Art/Streets/PV_StreetPalette.png");EditorUtility.SetDirty(atlas);AssetDatabase.SaveAssetIfDirty(atlas);
            // Retain the neutral white road markings, restore warm paving and green roadside soil.
            var colors=new Dictionary<string,string>{{"FFF1CF","F6F9FE"},{"E2E7ED","E7D8B9"},{"CCD3DB","CABCA2"},{"A6B0BE","AF9F86"},{"C7CDD5","B99B7B"},{"C3CCD5","92A68A"}};
            foreach(var m in game.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null&&m.mainTexture==null&&m.HasProperty("_Color")).Distinct())
            {string key=ColorUtility.ToHtmlStringRGB(m.color);if(colors.TryGetValue(key,out var hex)){m.color=Hex(hex);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);}}
        }
        static Cubemap Reflection()
        {
            return Asset("PV_NeutralReflection.asset",()=>
            {
                var cube=new Cubemap(32,TextureFormat.RGB24,true);var pixels=new Color[1024];
                for(int i=0;i<pixels.Length;i++)pixels[i]=Color.Lerp(Hex("8F9EAF"),Hex("E6F0FA"),(i/32)/31f);
                foreach(CubemapFace face in new[]{CubemapFace.PositiveX,CubemapFace.NegativeX,CubemapFace.PositiveY,CubemapFace.NegativeY,CubemapFace.PositiveZ,CubemapFace.NegativeZ})cube.SetPixels(pixels,face);
                cube.Apply(true);return cube;
            });
        }
    }
}
