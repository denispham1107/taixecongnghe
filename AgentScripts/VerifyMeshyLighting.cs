using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyMeshyLighting
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var l=g.daylight;
        var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
        var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes("Assets/PhoViet/Art/Bikes/Meshy/Meshy_Emission.png"));
        var p=image.GetPixels();int white=p.Count(c=>c.b>.4f&&c.g>.4f),red=p.Count(c=>c.r>.4f&&c.g<.05f);
        check(white>100&&red>50,"UV mask contains actual front white and rear red lamp faces");
        check(white+red<p.Length/8,"Emission is restricted to lenses, not whole white body");
        check(p.Take(2048*128).All(c=>c.r<.01f&&c.g<.01f&&c.b<.01f),"Rider palette cannot glow at night");
        UnityEngine.Object.DestroyImmediate(image);
        foreach(string asset in new[]{"PV_HeroScooter","PV_CityScooter","PV_Underbone","PV_SportBike","PV_Racer"})
        {
            check(l.emissionMaterials.Any(m=>m.name==asset+"_Atlas"),asset+": managed by the day-night system");
            var d=g.GetComponentsInChildren<BikeVisualAnimator>(true).First(b=>b.AssetName==asset);
            var skins=d.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            check(skins.All(s=>l.emissionUsers.Contains(s)),asset+": every LOD has live day-night emission");
        }
        try
        {
            l.SetCycleTime(30);l.UpdateLamps();
            var hero=g.rider.GetComponent<BikeVisualAnimator>().visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).First();
            check(hero.sharedMaterial.IsKeywordEnabled("_EMISSION")&&hero.sharedMaterial.GetColor("_EmissionColor").maxColorComponent>1,"Head/tail lens material lights up at night");
            l.SetCycleTime(0);l.UpdateLamps();
            check(hero.sharedMaterial.GetColor("_EmissionColor").maxColorComponent<.01f,"Lens emission switches off during daylight");
            var smoke=g.rider.GetComponent<BoostSmokeVisual>()??g.rider.gameObject.AddComponent<BoostSmokeVisual>();smoke.Initialize(g);
            var nozzle=smoke.GetComponentsInChildren<ParticleSystem>(true).First().transform.parent;
            check(nozzle.parent.name=="PV_MeshyExhaust"&&nozzle.localPosition.sqrMagnitude<.00001f,"Boost plume uses the Meshy frame socket, not legacy hardcoded Wave position");
            return new{success=true,count=checks.Count,checks,whitePixels=white,redPixels=red};
        }
        finally{l.SetCycleTime(0);l.UpdateLamps();}
    }
}
