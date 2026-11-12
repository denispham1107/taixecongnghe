using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using PhoViet;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public static class GenerateWreckSupport21204
{
    public static object Main()
    {
        var game = UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        game.RestartRide();
        var car = game.rider.GetComponent<PlayerCarVisual>();
        typeof(PhoVietGame).GetProperty("State").SetValue(game,PhoVietGame.RideState.Falling);
        game.rider.localRotation=Quaternion.identity;
        car.Advance(0);
        var models = new Dictionary<string,Vector3[]>();
        Add(models,"Player",car.visual);
        foreach (var v in game.vehicles)
        {
            if (!v.model.TryGetComponent<CarVisualAnimator>(out var animator)) continue;
            v.active=true;
            v.model.gameObject.SetActive(true);
            if (v.model.TryGetComponent<MeshyCarVariants>(out var variants))
            {
                for(int i=0;i<variants.variants.Length;i++)
                {
                    var key = variants.variants[i].asset;
                    if (models.ContainsKey(key)) continue;
                    variants.Select(i,0,v);
                    SampleTraffic(models,key,animator,v);
                }
            }
            else if (!models.ContainsKey(animator.AssetName)) SampleTraffic(models,animator.AssetName,animator,v);
            v.active=false;
            v.model.gameObject.SetActive(false);
        }
        var bike=game.vehicles.First(v=>v.model.GetComponent<BikeVisualAnimator>()?.urbanMotion!=null);
        var bikeAnimator=bike.model.GetComponent<BikeVisualAnimator>();
        bike.active=bike.knocked=true;
        bike.knockAge=2;
        bike.model.gameObject.SetActive(true);
        foreach(int side in new[]{-1,1})
        {
            bike.model.localRotation=Quaternion.Euler(0,0,side*78);
            bikeAnimator.Advance(0);
            Add(models,side<0?"Urban Rider right":"Urban Rider left",bikeAnimator.visual);
        }
        foreach(string name in new[]{"Helicopter", "Skywing"})
        {
            var prefab=Resources.Load<GameObject>(name+"/PV_"+name);
            var instance=UnityEngine.Object.Instantiate(prefab,game.transform);
            var airframe=instance.transform.Find("Airframe");
            Add(models,name,airframe,true);
            foreach(Transform child in airframe)
                if(child.name=="Main rotor"||child.name=="Tail rotor"||child.name=="Front propeller")Add(models,child.name,child);
            UnityEngine.Object.DestroyImmediate(instance);
        }
        var rows=models.Select(pair=>"            { \""+pair.Key+"\", new[]\n            {\n"+
            string.Join(",\n",Enumerable.Range(0,(pair.Value.Length+2)/3).Select(i=>"                "+string.Join(", ",pair.Value.Skip(i*3).Take(3).Select(Format))))+"\n            } }");
        const string path="Assets/PhoViet/Scripts/VehicleWreckSupport.cs";
        const string begin="            // BEGIN GENERATED POINTS";
        const string end="            // END GENERATED POINTS";
        string text=File.ReadAllText(path);
        int first=text.IndexOf(begin,StringComparison.Ordinal)+begin.Length;
        int last=text.IndexOf(end,StringComparison.Ordinal);
        File.WriteAllText(path,text.Substring(0,first)+"\n"+string.Join(",\n",rows)+"\n"+text.Substring(last));
        game.ResetRide();
        return models.ToDictionary(pair=>pair.Key,pair=>pair.Value.Length);
    }

    static void SampleTraffic(Dictionary<string,Vector3[]> models,string name,CarVisualAnimator animator,PhoVietGame.TrafficVehicle v)
    {
        v.knocked=false;
        v.speed=0;
        animator.Advance(0);
        Add(models,name,animator.visual);
    }

    static string Format(Vector3 p)
    {
        string F(float x)=>x.ToString("0.000",CultureInfo.InvariantCulture)+"f";
        return "new Vector3("+F(p.x)+","+F(p.y)+","+F(p.z)+")";
    }

    static void Add(Dictionary<string,Vector3[]> models,string name,Transform visual,bool direct=false)
    {
        var vertices=new List<Vector3>();
        var temp=new Mesh();
        foreach(var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>(false))
        {
            if (!renderer.enabled) continue;
            renderer.BakeMesh(temp);
            foreach(var p in temp.vertices)vertices.Add(visual.InverseTransformPoint(renderer.transform.TransformPoint(p)));
        }
        UnityEngine.Object.DestroyImmediate(temp);
        foreach(var filter in visual.GetComponentsInChildren<MeshFilter>(false))
        {
            if (direct&&filter.transform.parent!=visual || filter.name.Contains("blur",StringComparison.OrdinalIgnoreCase) ||
                !filter.TryGetComponent<MeshRenderer>(out var renderer) || !renderer.enabled)continue;
            using var data = MeshUtility.AcquireReadOnlyMeshData(filter.sharedMesh);
            using var positions = new NativeArray<Vector3>(data[0].vertexCount, Allocator.Temp);
            data[0].GetVertices(positions);
            foreach(var p in positions)vertices.Add(visual.InverseTransformPoint(renderer.transform.TransformPoint(p)));
        }
        if(vertices.Count==0)throw new InvalidOperationException(name+" has no visible mesh");
        var extremes=new HashSet<Vector3>();
        for(int x=-3;x<=3;x++)for(int y=-3;y<=3;y++)for(int z=-3;z<=3;z++)
        {
            if(Mathf.Max(Mathf.Abs(x),Mathf.Abs(y),Mathf.Abs(z))!=3)continue;
            var direction=new Vector3(x,y,z);
            float lowest=float.PositiveInfinity;
            Vector3 best=default;
            foreach(var p in vertices)
            {
                float along=Vector3.Dot(direction,p);
                if(along<lowest){lowest=along;best=p;}
            }
            extremes.Add(new Vector3(Round(best.x),Round(best.y),Round(best.z)));
        }
        models.Add(name,extremes.OrderBy(p=>p.x).ThenBy(p=>p.y).ThenBy(p=>p.z).ToArray());
    }

    static float Round(float n)=>Mathf.Round(n*1000)*.001f;
}
