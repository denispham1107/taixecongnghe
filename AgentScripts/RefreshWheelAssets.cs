using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class RefreshWheelAssets
{
    public static object Main()
    {
        if(Application.isPlaying)throw new Exception("Exit Play Mode first");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        string[] clips={"Idle","Ride","KickLeft","KickRight","FallLeft","FallRight","Recover"};var report=new List<object>();
        foreach(string n in new[]{"PV_HeroScooter","PV_CityScooter","PV_Underbone","PV_SportBike","PV_Racer"})
        {
            string path="Assets/PhoViet/Art/Bikes/MeshyRiders/"+n+".fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(path);var takes=importer.defaultClipAnimations;
            foreach(var take in takes)
            {
                string name=clips.FirstOrDefault(c=>take.name.EndsWith(c,StringComparison.OrdinalIgnoreCase));
                if(name==null)continue;take.name=name;take.loopTime=name=="Idle"||name=="Ride";take.wrapMode=take.loopTime?WrapMode.Loop:WrapMode.ClampForever;
            }
            if(takes.Length!=7)throw new Exception(n+": exported source must retain seven takes, got "+takes.Length);
            importer.clipAnimations=takes;importer.SaveAndReimport();
            var imported=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>clips.Contains(c.name)).ToArray();
            if(imported.Length!=7||!imported.All(c=>c.legacy))throw new Exception(n+": seven semantic legacy clips must import");
            report.Add(new{asset=n,clips=imported.Select(c=>c.name).ToArray()});
        }
        return new{success=true,count=report.Count,assets=report};
    }
}
