using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.Editor
{
    // Installs only civilian/racer Meshy visuals. Physics roots and the police bike remain intact.
    public static class PhoVietMeshyBikeUpgrade
    {
        [Serializable] sealed class Mounts { public float[] headlight,taillight,exhaust_tip; }
        [Serializable] sealed class Metadata { public string design;public Mounts anchors; }
        static readonly string[] ClipNames={"KickLeft","KickRight","FallLeft","FallRight","Recover","Idle","Ride"};
        public static Vector3? Mount(string asset,string name)
        {
            string path=Path.ChangeExtension(PhoVietBikeUpgrade.ModelPath(asset),"json");
            if(!PhoVietBikeUpgrade.IsMeshyPath(path)||!File.Exists(path))return null;
            var info=JsonUtility.FromJson<Metadata>(File.ReadAllText(path));
            float[] p=name=="headlight"?info.anchors.headlight:name=="taillight"?info.anchors.taillight:info.anchors.exhaust_tip;
            return Point(p);
        }

        [MenuItem("Phố Việt/Art/Install received Meshy motorcycles (keep police)")]
        [MenuItem("Phố Việt/Art/Install received Meshy riders (keep police)")]
        public static void MenuInstall(){Debug.Log(JsonUtility.ToJson(Install()));}
        public static object Install()
        {
            if(Application.isPlaying)throw new Exception("Exit Play Mode first");
            var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
            if(game==null||game.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity")throw new Exception("Open saved PhoViet scene first");
            if(game.daylight==null)throw new Exception("Lighting is required");
            string backup=".tools/PhoViet-before-meshy-rider.unity";Directory.CreateDirectory(".tools");
            if(!File.Exists(backup))File.Copy(game.gameObject.scene.path,backup);
            // Preserve any in-memory editor changes too; saveAsCopy keeps the active scene path.
            if(game.gameObject.scene.isDirty&&!File.Exists(".tools/PhoViet-before-meshy-rider-memory.unity"))
                EditorSceneManager.SaveScene(game.gameObject.scene,".tools/PhoViet-before-meshy-rider-memory.unity",true);
            var police=game.policeBike.GetComponent<BikeVisualAnimator>();
            var policeVisual=police.visual;string policeBefore=EditorJsonUtility.ToJson(police);
            AssetDatabase.Refresh();
            var metadata=new Dictionary<string,Metadata>();
            foreach(string asset in PhoVietBikeUpgrade.Assets.Where(a=>a!="PV_TrafficPolice"))
            {
                string path=PhoVietBikeUpgrade.ModelPath(asset);
                if(!PhoVietBikeUpgrade.IsMeshyPath(path))throw new Exception("Missing received model: "+asset);
                var info=JsonUtility.FromJson<Metadata>(File.ReadAllText(Path.ChangeExtension(path,"json")));
                if(info.design!="meshy-scooter"||info.anchors==null)throw new Exception("Invalid model metadata: "+asset);
                metadata.Add(asset,info);
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                var clips=importer.defaultClipAnimations;
                foreach(var clip in clips)
                {
                    string name=ClipNames.FirstOrDefault(n=>clip.name.EndsWith(n,StringComparison.OrdinalIgnoreCase));
                    if(name==null)continue;clip.name=name;clip.loopTime=name=="Idle"||name=="Ride";
                    clip.wrapMode=clip.loopTime?WrapMode.Loop:WrapMode.ClampForever;
                }
                importer.clipAnimations=clips;importer.SaveAndReimport();
                var imported=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>ClipNames.Contains(c.name)).ToArray();
                if(imported.Length!=7)throw new Exception("Expected seven clips: "+asset);
            }
            var replace=typeof(PhoVietBikeUpgrade).GetMethod("Replace",BindingFlags.Static|BindingFlags.NonPublic);
            var drivers=game.GetComponentsInChildren<BikeVisualAnimator>(true).Where(b=>b.role!=BikeVisualAnimator.ActorRole.Police&&b.AssetName!="PV_TrafficPolice").ToArray();
            foreach(var bike in drivers)
            {
                if(!metadata.ContainsKey(bike.AssetName))throw new Exception("Unknown bike "+bike.AssetName);
                replace.Invoke(null,new object[]{bike.transform,bike.AssetName,bike.role});
                var sign=bike.transform.Find("Biển xe nguy hiểm");if(sign!=null)sign.localPosition=new Vector3(0,2.6f,0);
            }
            game.leftKickLeg=StreetArt.Group("Chân đạp trái",game.rider,new Vector3(-.17f,1.08f,-.25f));
            game.rightKickLeg=StreetArt.Group("Chân đạp phải",game.rider,new Vector3(.17f,1.08f,-.25f));
            int branchParked=0;
            var rebaked=new List<string>();
            foreach(var bike in drivers.Where(b=>b.role==BikeVisualAnimator.ActorRole.Parked).GroupBy(b=>b.AssetName).Select(g=>g.First()))
            {
                string path="Assets/PhoViet/Generated/RouteParked_"+bike.AssetName+".asset";
                var target=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(target==null)throw new Exception("Missing branch parked mesh "+path);
                var skin=bike.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(s=>s.name.EndsWith("_LOD1",StringComparison.Ordinal));
                var baked=new Mesh{name=target.name};skin.BakeMesh(baked);
                EditorUtility.CopySerialized(baked,target);target.name="PV_RouteParked_"+bike.AssetName;EditorUtility.SetDirty(target);
                UnityEngine.Object.DestroyImmediate(baked);rebaked.Add(bike.AssetName+":"+target.vertexCount);
                foreach(var filter in game.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh==target))
                {
                    filter.GetComponent<MeshRenderer>().sharedMaterial=skin.sharedMaterial;branchParked++;
                }
            }
            int anchored=0;var lighting=game.daylight;
            foreach(var lamp in lighting.vehicles)
            {
                if(lamp.root==null)continue;var bike=lamp.root.GetComponent<BikeVisualAnimator>();
                if(bike==null||!metadata.TryGetValue(bike.AssetName,out var info))continue;
                var bones=bike.visual.GetComponentsInChildren<Transform>(true);
                lamp.head=Anchor(lamp.root,bones.Single(t=>t.name=="Steering"),"Quang học đèn xe • pha",Point(info.anchors.headlight));
                var tail=Anchor(lamp.root,bones.Single(t=>t.name=="Frame"),"Quang học đèn xe • hậu",Point(info.anchors.taillight));
                foreach(var halo in lamp.halos)halo.source=halo.rear?tail:lamp.head;
                anchored++;
            }
            var managedNames=metadata.Keys.Select(a=>a+"_Atlas").ToArray();
            var newBikeMaterials=metadata.Keys.Select(a=>AssetDatabase.LoadAssetAtPath<Material>(Path.ChangeExtension(PhoVietBikeUpgrade.ModelPath(a),"mat"))).ToArray();
            lighting.emissionMaterials=lighting.emissionMaterials.Where(m=>m!=null&&!managedNames.Contains(m.name)).Concat(newBikeMaterials).Distinct().ToArray();
            lighting.emissionUsers=game.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&(lighting.emissionMaterials.Contains(m)||m==lighting.streetLens))).ToArray();
            bool policeUnchanged=policeVisual==police.visual&&policeBefore==EditorJsonUtility.ToJson(police);
            if(!policeUnchanged)throw new Exception("Police visual unexpectedly changed");
            if(game.jobsEnabled)throw new Exception("Ride jobs must remain disabled");
            EditorUtility.SetDirty(game);EditorUtility.SetDirty(lighting);AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
            return new{success=true,replaced=drivers.Length,parked=drivers.Count(b=>b.role==BikeVisualAnimator.ActorRole.Parked),branchParked,anchored,rebaked,policeUnchanged,
                missingLampHeads=lighting.vehicles.Count(l=>l.head==null),jobsEnabled=game.jobsEnabled,scene=game.gameObject.scene.path};
        }
        static Vector3 Point(float[] p)=>new Vector3(p[0],p[1],p[2]);
        static Transform Anchor(Transform root,Transform bone,string name,Vector3 local)
        {var t=new GameObject(name).transform;t.SetParent(bone,false);t.position=root.TransformPoint(local);t.rotation=root.rotation;return t;}
    }
}
