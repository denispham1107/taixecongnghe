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
    // Cài cầu vượt sông (24/10/2026, huongdan.md 21.35; dài 50,4 m + nước gương 21.46) vào cả 35 khối phố của 5 dãy đường.
    // Mỗi khối có thêm "Cầu vượt sông • Blender" (tắt sẵn) + RiverBlock; runtime chọn khối nào thành cầu.
    public static class PhoVietRiverUpgrade
    {
        public const string Folder="Assets/PhoViet/Art/River/";
        const string Root="Cầu vượt sông • Blender";
        [Serializable] sealed class BoatInfo{public string asset;public float x,z,yaw;}
        [Serializable] sealed class ObstacleInfo{public string kind;public float[] center,size;}
        [Serializable] sealed class PartInfo{public string name;public string[] slots;}
        [Serializable] sealed class PartsInfo{public PartInfo[] parts;}
        [Serializable] sealed class Info{public float[] water;public float water_y,deck_half,lamp_x;public float[] lamps,piers;public BoatInfo[] boats;public ObstacleInfo[] obstacles;public string[] slots;}

        [MenuItem("Phố Việt/Art/Install river bridge blocks")]
        public static object Install()
        {
            if(Application.isPlaying)throw new Exception("Exit Play Mode first");
            var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null)throw new Exception("Open PhoViet scene first");
            AssetDatabase.Refresh();
            foreach(string name in new[]{"PV_RiverBridge","PV_RiverApartments","PV_RiverBoat0","PV_RiverBoat1"})
            {
                var importer=AssetImporter.GetAtPath(Folder+name+".fbx") as ModelImporter;if(importer==null)throw new Exception("Missing "+name+".fbx");
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
                importer.addCollider=false;importer.globalScale=1;importer.isReadable=false;importer.importNormals=ModelImporterNormals.Import;
                importer.bakeAxisConversion=false;importer.SaveAndReimport();
            }
            var info=JsonUtility.FromJson<Info>(System.IO.File.ReadAllText(Folder+"PV_RiverBridge.json"));
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"PV_RiverBridge.fbx");
            // Căn hộ ven sông là mô hình riêng, mỗi khối một LODGroup (21.54).
            var aptInfo=JsonUtility.FromJson<PartsInfo>(System.IO.File.ReadAllText(Folder+"PV_RiverApartments.json"));
            var aptModel=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"PV_RiverApartments.fbx");if(aptModel==null)throw new Exception("Missing PV_RiverApartments.fbx");
            var boats=new Dictionary<string,GameObject>();foreach(var b in info.boats)if(!boats.ContainsKey(b.asset))boats[b.asset]=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+b.asset+".fbx");
            var atlas=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");if(atlas==null)throw new Exception("Missing street atlas");
            // Mặt sông kiểu gương theo ảnh mẫu người dùng (21.45–21.46): phản chiếu thật từ RiverReflection trên camera chơi,
            // thân nước xanh đen, sát kè ngả bùn, sóng êm. Đặt lại mọi thông số mỗi lần cài để vật liệu không giữ số của shader cũ.
            var water=Asset("PV_RiverWater.mat",()=>new Material(Shader.Find("PhoViet/River Water Mirror")));water.shader=Shader.Find("PhoViet/River Water Mirror");
            water.SetColor("_DeepColor",new Color(.015f,.075f,.12f));water.SetColor("_ShallowColor",new Color(.04f,.15f,.2f));water.SetColor("_EdgeColor",new Color(.17f,.18f,.12f));
            water.SetColor("_ReflectTint",new Color(.68f,.79f,.88f));water.SetColor("_FoamColor",new Color(.86f,.9f,.88f));
            water.SetFloat("_Reflectivity",.5f);water.SetFloat("_Distort",.05f);water.SetFloat("_Ripple",.075f);water.SetFloat("_EdgeWidth",2.2f);
            water.SetFloat("_WaveHeight",.35f);water.SetFloat("_WaveSpeed",.7f);water.SetFloat("_Alpha",.93f);water.SetFloat("_Sparkle",1.2f);water.SetFloat("_ShadowStrength",.62f);water.SetFloat("_WaterY",info.water_y);
            // Hình khối che nắng cho bóng giải tích: mép hai bờ và hai trụ cầu theo JSON của mô hình.
            water.SetVector("_Deck",new Vector4(3.7f,-.9f,1.15f,0));water.SetVector("_Banks",new Vector4(info.water[0],info.water[1],.15f,0));
            water.SetVector("_Piers",new Vector4(info.piers[0],info.piers[1],.62f,3.3f));
            if(Mathf.Abs(info.water[0]-RiverBlock.Near)>.01f||Mathf.Abs(info.water[1]-RiverBlock.Far)>.01f)throw new Exception("RiverBlock.Near/Far do not match PV_RiverBridge.json");
            var mask=Asset("PV_RiverGroundMask.mat",()=>new Material(Shader.Find("PhoViet/River Ground Mask")));mask.shader=Shader.Find("PhoViet/River Ground Mask");
            var murk=Asset("PV_RiverMurk.mat",()=>new Material(Shader.Find("PhoViet/River Murk")));murk.shader=Shader.Find("PhoViet/River Murk");
            // Vật liệu nhựa đường/vạch kẻ lấy đúng của mesh gộp khối phố để mặt cầu liền màu với đường.
            var sample=g.roadRows[0].tiles[1];
            Material TileMat(string color)=>sample.Find("Geometry • PV_"+color+"FFFalse").GetComponent<MeshRenderer>().sharedMaterial;
            var slotMats=new Dictionary<string,Material>{{"PV_StreetAtlas",atlas},{"PV_RiverWater",water},{"PV_RiverGroundMask",mask},{"PV_RiverMurk",murk},
                {"PV_RiverAsphalt",TileMat("555D5D")},{"PV_RiverLineWhite",TileMat("FFF1CF")},{"PV_RiverLineYellow",TileMat("FFD35A")},
                // Kính trong suốt của căn hộ ven sông (21.49) dùng chung vật liệu kính của phố.
                {"PV_StreetGlass",AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetGlass.mat")}};
            if(slotMats["PV_StreetGlass"]==null)throw new Exception("Missing street glass material");
            // Phòng căn hộ sáng đèn ban đêm (21.51): atlas phố + phát sáng theo _PVNightLights của DayNightLighting.
            var room=Asset("PV_RiverRoomLight.mat",()=>new Material(Shader.Find("PhoViet/Apartment Room")));room.shader=Shader.Find("PhoViet/Apartment Room");
            room.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Art/Streets/PV_StreetPalette.png"));
            room.SetFloat("_Glow",1.15f);room.SetColor("_Warm",new Color(1,.86f,.66f));room.SetFloat("_Glossiness",.2f);
            if(room.GetTexture("_MainTex")==null)throw new Exception("Missing street palette");
            slotMats["PV_RiverRoomLight"]=room;EditorUtility.SetDirty(room);
            // Nền thành phố bỏ qua lòng sông nhờ stencil.
            var groundRenderer=g.GetComponentsInChildren<MeshRenderer>(true).First(r=>r.name=="Nền thành phố");
            var oldGround=groundRenderer.sharedMaterial;
            var ground=Asset("PV_CityGround.mat",()=>new Material(Shader.Find("PhoViet/City Ground")));ground.shader=Shader.Find("PhoViet/City Ground");
            if(oldGround!=ground){ground.color=oldGround.color;ground.SetFloat("_Glossiness",oldGround.HasProperty("_Glossiness")?oldGround.GetFloat("_Glossiness"):.12f);ground.SetFloat("_Metallic",0);}
            groundRenderer.sharedMaterial=ground;
            // Đèn cầu: đúng mẫu đèn LED phố và hệ quang học ngày–đêm đang dùng.
            var lampModel=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/Lighting/PV_LEDStreetLamp.fbx");
            Material L(string n)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Lighting/"+n+".mat");
            var lampMats=new[]{L("PV_StreetLEDMetal"),L("PV_StreetLEDSilver"),L("PV_StreetLEDLens")};
            var floorQuad=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/PhoViet/Art/Lighting/PV_OpticalGround.asset");
            var haloQuad=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/PhoViet/Art/Lighting/PV_OpticalHalo.asset");
            var streetPool=L("PV_StreetSpill");var whiteGlow=L("PV_WhiteGlare");
            if(lampModel==null||lampMats.Any(m=>m==null)||floorQuad==null||haloQuad==null||streetPool==null||whiteGlow==null)throw new Exception("Missing street lamp assets");
            var lighting=g.daylight;if(lighting==null)throw new Exception("Day-night lighting not installed");
            var streets=lighting.streets.Where(s=>s.source!=null&&!s.source.GetComponentsInParent<Transform>(true).Any(t=>t.name==Root)).ToList();
            var clipped=new Dictionary<Mesh,Mesh>();int tiles=0,hidden=0,clips=0,lamps=0,boatCount=0,houses=0,aptParts=0;
            System.IO.Directory.CreateDirectory(Folder+"Clipped");
            foreach(var row in g.roadRows)foreach(var tile in row.tiles)
            {
                var old=tile.Find(Root);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
                var block=tile.GetComponent<RiverBlock>();if(block==null)block=tile.gameObject.AddComponent<RiverBlock>();
                // Trả mesh gộp gốc trước khi tính lại (cài lặp lại an toàn).
                for(int i=0;i<block.clipFilters.Length;i++)if(block.clipFilters[i]!=null&&block.fullMeshes[i]!=null)block.clipFilters[i].sharedMesh=block.fullMeshes[i];
                foreach(var item in block.streetOnly)if(item!=null)item.SetActive(true);
                var street=new List<GameObject>();var filters=new List<MeshFilter>();var full=new List<Mesh>();var cut=new List<Mesh>();
                foreach(Transform child in tile)
                {
                    if(!TileRange(tile,child,out float z0,out float z1)||z1<=RiverBlock.Near+.4f||z0>=RiverBlock.Far-.4f)continue;
                    var filter=child.GetComponent<MeshFilter>();
                    if(filter!=null&&filter.sharedMesh!=null&&child.childCount==0&&child.GetComponent<StreetObstacleSet>()==null&&(child.name.StartsWith("Geometry • ",StringComparison.Ordinal)||child.name.StartsWith("Vạch mũi tên",StringComparison.Ordinal)))
                    {
                        if(child.localPosition!=Vector3.zero||child.localRotation!=Quaternion.identity||child.localScale!=Vector3.one)throw new Exception("Unexpected merged mesh transform: "+child.name);
                        if(!clipped.TryGetValue(filter.sharedMesh,out var c)){c=ClipRiver(filter.sharedMesh);clipped.Add(filter.sharedMesh,c);}
                        filters.Add(filter);full.Add(filter.sharedMesh);cut.Add(c);clips++;
                    }
                    else {street.Add(child.gameObject);hidden++;}
                }
                var root=(GameObject)PrefabUtility.InstantiatePrefab(model,tile);root.name=Root;
                root.transform.localPosition=Vector3.zero;root.transform.localRotation=Quaternion.identity;root.transform.localScale=Vector3.one;
                SetupLods(root,info.slots,slotMats,ShadowCastingMode.On,null,1);
                // 24 khối căn hộ, mỗi khối LODGroup riêng: tòa gần giữ LOD0 đầy đủ, tòa xa dùng LOD1/LOD2, tòa ngoài khung
                // nhìn không vẽ (trước đây cả khối cầu 190 m chung một LOD nên tòa xa 90 m vẫn ở LOD0 — 21.54).
                var apts=(GameObject)PrefabUtility.InstantiatePrefab(aptModel,root.transform);apts.name="Căn hộ ven sông • Blender";
                apts.transform.localPosition=Vector3.zero;apts.transform.localRotation=Quaternion.identity;apts.transform.localScale=Vector3.one;
                // Trình nhập FBX tự thêm một LODGroup ở gốc (thấy tên _LODn bên dưới), bao cả 24 khối — bỏ để từng khối tự chọn LOD.
                var rootLod=apts.GetComponent<LODGroup>();if(rootLod!=null)UnityEngine.Object.DestroyImmediate(rootLod);
                foreach(var part in aptInfo.parts)
                {
                    var pt=apts.transform.Find(part.name);if(pt==null)throw new Exception("Missing apartment part "+part.name);
                    pt.localPosition=Vector3.zero;pt.localRotation=Quaternion.identity;pt.localScale=Vector3.one;
                    SetupLods(pt.gameObject,part.slots,slotMats,ShadowCastingMode.On,ApartmentLods,2);aptParts++;
                }
                // Vật cản của dãy nhà ống ven sông (21.48): hộp trong hệ khối, chỉ có hiệu lực khi khối là cầu (root bật).
                if(info.obstacles!=null&&info.obstacles.Length>0)
                {
                    var set=root.AddComponent<StreetObstacleSet>();
                    set.obstacles=info.obstacles.Select(o=>new StreetObstacleSet.Obstacle(o.kind,new Vector3(o.center[0],o.center[1],o.center[2]),new Vector3(o.size[0],o.size[1],o.size[2]))).ToArray();
                    houses+=set.obstacles.Length;
                }
                foreach(var b in info.boats)
                {
                    var boat=(GameObject)PrefabUtility.InstantiatePrefab(boats[b.asset],root.transform);boat.name="Thuyền neo • "+b.asset;
                    var rest=new Vector3(b.x,info.water_y,b.z);boat.transform.localPosition=rest;boat.transform.localRotation=Quaternion.Euler(0,b.yaw,0);
                    SetupLods(boat,new[]{"PV_StreetAtlas"},slotMats,ShadowCastingMode.On);
                    var bob=boat.AddComponent<RiverBoat>();bob.restPosition=rest;bob.restYaw=b.yaw;bob.phase=b.x*.37f+b.z*.11f;boatCount++;
                }
                var lampGroup=Group("Đèn cầu • Blender",root.transform);
                foreach(float side in new[]{-1f,1f})foreach(float z in info.lamps)
                {
                    var unit=Group("Đèn LED cầu • "+side,lampGroup,new Vector3(side*info.lamp_x,0,z));unit.localRotation=Quaternion.Euler(0,side>0?180:0,0);
                    var instance=(GameObject)PrefabUtility.InstantiatePrefab(lampModel,unit);instance.name="Blender • đèn đường LED";
                    instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=lampModel.transform.localRotation;instance.transform.localScale=lampModel.transform.localScale;
                    foreach(var r in instance.GetComponentsInChildren<MeshRenderer>(true)){r.sharedMaterials=lampMats;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;}
                    var lodRenderers=instance.GetComponentsInChildren<MeshRenderer>(true).OrderBy(r=>r.name.Contains("_LOD2")?2:r.name.Contains("_LOD1")?1:0).ToArray();
                    if(lodRenderers.Length==3){var lod=instance.GetComponent<LODGroup>();if(lod==null)lod=instance.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.13f,new Renderer[]{lodRenderers[0]}),new LOD(.05f,new Renderer[]{lodRenderers[1]}),new LOD(.005f,new Renderer[]{lodRenderers[2]})});lod.RecalculateBounds();}
                    var source=Group("Tâm quang học LED",unit,new Vector3(2.35f,8.48f,0));source.localRotation=Quaternion.Euler(90,0,0);
                    var pool=Overlay("Vệt sáng đèn cầu",lampGroup,streetPool,floorQuad);pool.transform.localPosition=new Vector3(side*(info.lamp_x-2.35f),.038f,z);pool.transform.localScale=new Vector3(14,1,14);
                    var glow=Overlay("Quầng đèn cầu",lampGroup,whiteGlow,haloQuad);glow.transform.localScale=Vector3.one*.9f;
                    streets.Add(new DayNightLighting.StreetLamp{source=source,pool=pool,glow=glow});lamps++;
                }
                block.bridge=root;block.streetOnly=street.ToArray();block.clipFilters=filters.ToArray();block.fullMeshes=full.ToArray();block.clippedMeshes=cut.ToArray();
                block.SetBridge(false);EditorUtility.SetDirty(block);tiles++;
            }
            lighting.streets=streets.ToArray();
            var lens=lampMats[2];
            lighting.emissionUsers=lighting.emissionUsers.Where(r=>r!=null).Concat(g.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Contains(lens))).Distinct().ToArray();
            // Lớp phản chiếu: chỉ cầu/kè/thuyền/đèn cầu và các tòa nhà phố hiện trong ảnh phản chiếu mặt sông (21.46).
            var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layerName=tags.FindProperty("layers").GetArrayElementAtIndex(RiverReflection.ReflectLayer);
            if(layerName.stringValue!="PV_RiverReflect"){if(!string.IsNullOrEmpty(layerName.stringValue))throw new Exception("Layer "+RiverReflection.ReflectLayer+" already used: "+layerName.stringValue);layerName.stringValue="PV_RiverReflect";tags.ApplyModifiedProperties();}
            int reflectLayers=0;
            foreach(var row in g.roadRows)foreach(var tile in row.tiles)foreach(Transform child in tile)
            {
                bool reflect=child.name==Root||child.name.StartsWith("Blender • ",StringComparison.Ordinal);
                if(!reflect)continue;
                foreach(var t in child.GetComponentsInChildren<Transform>(true)){t.gameObject.layer=RiverReflection.ReflectLayer;reflectLayers++;}
            }
            // Camera chơi render ảnh phản chiếu mặt sông.
            var reflection=g.rideCamera.GetComponent<RiverReflection>();if(reflection==null)reflection=g.rideCamera.gameObject.AddComponent<RiverReflection>();
            reflection.game=g;reflection.resolution=.42f;reflection.maxSize=768;reflection.farClip=120;reflection.clipOffset=.05f;reflection.maxLod=1;EditorUtility.SetDirty(reflection);
            EditorUtility.SetDirty(lighting);EditorUtility.SetDirty(g);EditorUtility.SetDirty(ground);EditorUtility.SetDirty(water);EditorUtility.SetDirty(mask);EditorUtility.SetDirty(murk);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
            return new{success=true,reflectLayers,houses,aptParts,tiles,hiddenPerTile=hidden/(float)Mathf.Max(1,tiles),clippedMeshes=clipped.Count,clips,lamps,boats=boatCount,streets=lighting.streets.Length,jobs=g.jobsEnabled};
        }
        static bool TileRange(Transform tile,Transform child,out float z0,out float z1)
        {
            z0=float.MaxValue;z1=float.MinValue;bool any=false;
            foreach(var r in child.GetComponentsInChildren<Renderer>(true))
            {
                var b=r.bounds;
                for(int i=0;i<8;i++)
                {
                    var corner=new Vector3((i&1)==0?b.min.x:b.max.x,(i&2)==0?b.min.y:b.max.y,(i&4)==0?b.min.z:b.max.z);
                    float z=tile.InverseTransformPoint(corner).z;z0=Mathf.Min(z0,z);z1=Mathf.Max(z1,z);any=true;
                }
            }
            return any;
        }
        // Ngưỡng LOD theo chiều cao màn hình của từng khối căn hộ (~25 m): LOD0 trong ~80 m, LOD1 tới ~300 m.
        static readonly float[] DefaultLods={.12f,.04f,.002f},ApartmentLods={.3f,.08f,.006f};
        // proxyLevel ≥ 0: các LOD chi tiết hơn KHÔNG đổ bóng; một bản sao "ShadowsOnly" của LOD proxyLevel đổ bóng thay
        // (cùng khối bao nên bóng trông như cũ, nhưng shadow map vẽ ít tam giác hơn nhiều — 21.54).
        static void SetupLods(GameObject root,string[] slots,Dictionary<string,Material> mats,ShadowCastingMode shadows,float[] thresholds=null,int proxyLevel=-1)
        {
            thresholds=thresholds??DefaultLods;
            foreach(Transform old in root.transform.Cast<Transform>().ToArray())if(old.name.StartsWith("Bóng • ",StringComparison.Ordinal))UnityEngine.Object.DestroyImmediate(old.gameObject);
            var renderers=root.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.transform.parent==root.transform).OrderBy(r=>r.name.Contains("_LOD2")?2:r.name.Contains("_LOD1")?1:0).ToArray();
            foreach(var r in renderers)
            {
                int count=r.GetComponent<MeshFilter>().sharedMesh.subMeshCount;
                r.sharedMaterials=Enumerable.Range(0,count).Select(i=>mats[slots[Mathf.Min(i,slots.Length-1)]]).ToArray();
                // Mesh xuất từ Blender còn trục Z lên: xoay -90° quanh X để trùng hệ khối phố (x ngang, y lên, z dọc đường).
                // Shader nước đổi ngược trục này (ToTile/FromTile) để tính sóng/bóng trên mặt nằm ngang.
                r.transform.localRotation=Quaternion.Euler(-90,0,0);
                r.shadowCastingMode=shadows;r.receiveShadows=true;r.gameObject.SetActive(true);
            }
            if(renderers.Length!=3)throw new Exception("Expected three LODs in "+root.name);
            var levels=new[]{new List<Renderer>{renderers[0]},new List<Renderer>{renderers[1]},new List<Renderer>{renderers[2]}};
            if(proxyLevel>=0&&proxyLevel<3)
            {
                var src=renderers[proxyLevel];
                var proxy=new GameObject("Bóng • "+src.name);proxy.transform.SetParent(root.transform,false);
                proxy.transform.localPosition=src.transform.localPosition;proxy.transform.localRotation=src.transform.localRotation;proxy.transform.localScale=src.transform.localScale;
                proxy.layer=src.gameObject.layer;
                proxy.AddComponent<MeshFilter>().sharedMesh=src.GetComponent<MeshFilter>().sharedMesh;
                var pr=proxy.AddComponent<MeshRenderer>();pr.sharedMaterials=src.sharedMaterials;pr.shadowCastingMode=ShadowCastingMode.ShadowsOnly;pr.receiveShadows=false;
                for(int i=0;i<proxyLevel;i++){renderers[i].shadowCastingMode=ShadowCastingMode.Off;levels[i].Add(pr);}
            }
            var lod=root.GetComponent<LODGroup>();if(lod==null)lod=root.AddComponent<LODGroup>();
            lod.SetLODs(new[]{new LOD(thresholds[0],levels[0].ToArray()),new LOD(thresholds[1],levels[1].ToArray()),new LOD(thresholds[2],levels[2].ToArray())});lod.RecalculateBounds();
        }
        static Renderer Overlay(string name,Transform parent,Material material,Mesh mesh)
        {var t=Group(name,parent);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return r;}
        static T Asset<T>(string file,Func<T> create) where T:UnityEngine.Object
        {var value=AssetDatabase.LoadAssetAtPath<T>(Folder+file);if(value==null){value=create();AssetDatabase.CreateAsset(value,Folder+file);}return value;}

        // Cắt bỏ phần mesh gộp nằm trong dải z (Near, Far): giữ hai bờ, mở lòng sông cho mô hình cầu.
        public static Mesh ClipRiver(Mesh source)
        {
            string path=Folder+"Clipped/"+source.name.Replace(' ','_')+"_"+AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)).Substring(0,8)+".asset";
            var v=source.vertices;var n=source.normals;var uv=source.uv;var tan=source.tangents;var col=source.colors;
            bool hasN=n.Length==v.Length,hasUv=uv.Length==v.Length,hasT=tan.Length==v.Length,hasC=col.Length==v.Length;
            var outV=new List<Vector3>();var outN=new List<Vector3>();var outUv=new List<Vector2>();var outT=new List<Vector4>();var outC=new List<Color>();
            var subs=new List<List<int>>();
            for(int s=0;s<source.subMeshCount;s++)
            {
                var tris=source.GetTriangles(s);var list=new List<int>();subs.Add(list);
                for(int i=0;i<tris.Length;i+=3)
                {
                    var poly=new List<Vert>{Make(tris[i]),Make(tris[i+1]),Make(tris[i+2])};
                    foreach(var part in new[]{Clip(poly,RiverBlock.Near,true),Clip(poly,RiverBlock.Far,false)})
                    {
                        if(part.Count<3)continue;int start=outV.Count;
                        foreach(var p in part){outV.Add(p.p);outN.Add(p.n);outUv.Add(p.uv);outT.Add(p.t);outC.Add(p.c);}
                        for(int k=1;k+1<part.Count;k++){list.Add(start);list.Add(start+k);list.Add(start+k+1);}
                    }
                }
            }
            Vert Make(int i)=>new Vert{p=v[i],n=hasN?n[i]:Vector3.up,uv=hasUv?uv[i]:Vector2.zero,t=hasT?tan[i]:Vector4.zero,c=hasC?col[i]:Color.white};
            // Ghi thẳng vào asset có sẵn qua API Mesh (21.47): CopySerialized cập nhật dữ liệu lưu nhưng bản trên GPU của
            // Editor vẫn giữ mesh cắt cũ (16,8–42), nên đầu cầu dài còn hiện nền đường thừa trên mặt sông.
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            var mesh=existing!=null?existing:new Mesh();
            mesh.Clear();mesh.name=source.name+"_River";mesh.indexFormat=outV.Count>65000?IndexFormat.UInt32:IndexFormat.UInt16;
            mesh.SetVertices(outV);if(hasN)mesh.SetNormals(outN);if(hasUv)mesh.SetUVs(0,outUv);if(hasT)mesh.SetTangents(outT);if(hasC)mesh.SetColors(outC);
            mesh.subMeshCount=subs.Count;for(int s=0;s<subs.Count;s++)mesh.SetTriangles(subs[s],s);
            mesh.RecalculateBounds();mesh.UploadMeshData(false);
            if(existing!=null){EditorUtility.SetDirty(existing);return existing;}
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        struct Vert{public Vector3 p,n;public Vector2 uv;public Vector4 t;public Color c;}
        static Vert Lerp(Vert a,Vert b,float t)=>new Vert{p=Vector3.Lerp(a.p,b.p,t),n=Vector3.Lerp(a.n,b.n,t).normalized,uv=Vector2.Lerp(a.uv,b.uv,t),t=Vector4.Lerp(a.t,b.t,t),c=Color.Lerp(a.c,b.c,t)};
        // Sutherland–Hodgman theo một mặt phẳng z: below=true giữ z ≤ plane, false giữ z ≥ plane.
        static List<Vert> Clip(List<Vert> poly,float plane,bool below)
        {
            var result=new List<Vert>();
            for(int i=0;i<poly.Count;i++)
            {
                var a=poly[i];var b=poly[(i+1)%poly.Count];
                float da=below?plane-a.p.z:a.p.z-plane,db=below?plane-b.p.z:b.p.z-plane;
                if(da>=0)result.Add(a);
                if((da>=0)!=(db>=0))result.Add(Lerp(a,b,da/(da-db)));
            }
            return result;
        }
    }
}
