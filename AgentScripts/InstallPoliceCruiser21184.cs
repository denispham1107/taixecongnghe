using System;
using System.Linq;
using PhoViet;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class InstallPoliceCruiser21184
{
    const string Import="Assets/MeshyImports/Vietnam_Traffic_Police_Cruiser_20261110_230018/";
    const string Art="Assets/PhoViet/Art/PoliceCruiser/";
    const string VisualName="Vietnam Traffic Police Cruiser";

    public static object Main()
    {
        var game=Object.FindAnyObjectByType<PhoVietGame>();
        if(Application.isPlaying||game==null||game.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity")
            throw new InvalidOperationException("Open PhoViet.unity in Edit Mode.");
        if(game.jobsEnabled||PhoVietGame.RideJobsReleased)throw new InvalidOperationException("Jobs must remain disabled.");
        var police=game.policeBike;
        var lamps=game.daylight.vehicles.Single(v=>v.root==police);
        var source=AssetDatabase.LoadAssetAtPath<GameObject>(Import+"Meshy_AI_Vietnam_Traffic_Police_Cruiser_20261110_texture.fbx");
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"PV_PoliceLivery.png");
        var original=AssetDatabase.LoadAssetAtPath<Material>(Import+"Material.001.mat");
        if(source==null||texture==null||original==null||texture.width<2048)throw new InvalidOperationException("Cruiser FBX, material or 2K livery missing.");

        var material=AssetDatabase.LoadAssetAtPath<Material>(Art+"PV_PoliceCruiser.mat");
        if(material==null){material=new Material(original){name="PV Police Cruiser"};AssetDatabase.CreateAsset(material,Art+"PV_PoliceCruiser.mat");}
        material.mainTexture=texture;
        EditorUtility.SetDirty(material);

        var next=(GameObject)PrefabUtility.InstantiatePrefab(source,police);
        next.name=VisualName;
        var visual=next.transform;
        visual.SetLocalPositionAndRotation(Vector3.zero,Quaternion.Euler(0,-90,0)*visual.localRotation);
        var body=next.GetComponentsInChildren<MeshRenderer>(true).Single();
        var size=FitCruiser(police,visual);
        body.sharedMaterial=material;

        var anchors=lamps.halos.Select(h=>h.source).Concat(lamps.halos.Select(h=>h.glow.transform))
            .Concat(new[]{lamps.head,lamps.roadLight.transform,game.redBeacon.transform,game.blueBeacon.transform}).Distinct().ToArray();
        foreach(var anchor in anchors)if(anchor.IsChildOf(police)&&anchor.parent!=police)anchor.SetParent(police,true);
        foreach(var child in police.Cast<Transform>().ToArray())
            if(child!=visual&&!anchors.Contains(child))Object.DestroyImmediate(child.gameObject);
        var oldAnimator=police.GetComponent<BikeVisualAnimator>();
        if(oldAnimator!=null)Object.DestroyImmediate(oldAnimator);
        police.name="Ô tô Cảnh sát giao thông";

        var halos=lamps.halos.ToList();
        foreach(bool rear in new[]{false,true})
        {
            var pair=halos.Where(h=>h.rear==rear).ToArray();
            var first=pair[0];
            if(pair.Length==1)
            {
                var socket=new GameObject(rear?"CSGT tail right":"CSGT head right").transform;
                var glow=Object.Instantiate(first.glow.gameObject,first.glow.transform.parent).GetComponent<Renderer>();
                halos.Add(new DayNightLighting.Halo{source=socket,glow=glow,rear=rear,size=first.size});
            }
        }
        lamps.halos=halos.ToArray();
        if(lamps.halos.Any(h=>h.source==lamps.head))lamps.head=new GameObject("CSGT centered headlamp projector").transform;
        FitLamps(game,lamps);
        game.daylight.emissionUsers=game.daylight.emissionUsers.Where(r=>r!=null).ToArray();
        police.gameObject.SetActive(false);
        foreach(var preview in game.gameObject.scene.GetRootGameObjects())
        {
            if(PrefabUtility.GetCorrespondingObjectFromSource(preview)!=source)continue;
            preview.tag="EditorOnly";
            preview.SetActive(false);
        }
        EditorUtility.SetDirty(game.daylight);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
        if(!EditorSceneManager.SaveScene(game.gameObject.scene))throw new InvalidOperationException("Could not save PhoViet.unity.");
        AssetDatabase.SaveAssets();
        return new{model=visual.name,length=size.z,width=size.x,height=size.y,halos=halos.Count};
    }

    public static object Resize()
    {
        var game=Object.FindAnyObjectByType<PhoVietGame>();
        if(Application.isPlaying||game==null||game.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity"||game.gameObject.scene.isDirty)
            throw new InvalidOperationException("Open the saved PhoViet scene in Edit Mode.");
        var visual=game.policeBike.Find(VisualName);
        if(visual==null)throw new InvalidOperationException("Install the Meshy cruiser first.");
        var player=game.rider.GetComponent<PlayerCarVisual>();
        if(player==null||Mathf.Abs(player.size.z-PhoVietGame.PoliceCruiserLength)>.01f)
            throw new InvalidOperationException("Police and player lengths must match.");
        var size=FitCruiser(game.policeBike,visual);
        FitLamps(game,game.daylight.vehicles.Single(v=>v.root==game.policeBike));
        EditorUtility.SetDirty(game.daylight);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
        if(!EditorSceneManager.SaveScene(game.gameObject.scene))throw new InvalidOperationException("Could not save PhoViet.unity.");
        return new{model=visual.name,length=size.z,width=size.x,height=size.y,playerLength=player.size.z};
    }

    static Vector3 FitCruiser(Transform police,Transform visual)
    {
        visual.localPosition=Vector3.zero;
        var mesh=visual.GetComponentInChildren<MeshFilter>(true);
        var bounds=mesh.sharedMesh.bounds;
        var corners=Enumerable.Range(0,8).Select(i=>police.InverseTransformPoint(mesh.transform.TransformPoint(bounds.center+
            Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))))).ToArray();
        var min=corners.Aggregate(Vector3.Min);var max=corners.Aggregate(Vector3.Max);
        float scale=PhoVietGame.PoliceCruiserLength/(max.z-min.z);
        visual.localScale*=scale;
        visual.localPosition=new Vector3(-(min.x+max.x)*.5f,-min.y,-(min.z+max.z)*.5f)*scale;
        PrefabUtility.RecordPrefabInstancePropertyModifications(visual);
        return (max-min)*scale;
    }

    static void FitLamps(PhoVietGame game,DayNightLighting.VehicleLamps lamps)
    {
        var police=game.policeBike;
        float scale=PhoVietGame.PoliceCruiserScale;
        var beaconSize=new Vector3(.19f,.045f,.16f)*scale;
        Mount(game.redBeacon.transform,police,new Vector3(-.16f,1.595f,-.14f)*scale,beaconSize);
        Mount(game.blueBeacon.transform,police,new Vector3(.16f,1.595f,-.14f)*scale,beaconSize);
        foreach(bool rear in new[]{false,true})
        {
            float x=-.68f;
            foreach(var halo in lamps.halos.Where(h=>h.rear==rear))
            {
                Mount(halo.source,police,new Vector3(x,rear?.88f:.83f,rear?-2.06f:2.0f)*scale,Vector3.one);
                halo.size=(rear?.35f:.45f)*scale;x=-x;
            }
        }
        Mount(lamps.head,police,new Vector3(0,.83f,2.0f)*scale,Vector3.one);
    }

    static void Mount(Transform node,Transform police,Vector3 position,Vector3 scale)
    {
        node.SetParent(police,false);
        node.SetLocalPositionAndRotation(position,Quaternion.identity);
        node.localScale=scale;
    }
}
