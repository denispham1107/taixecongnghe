using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PhoViet;

public static class VerifyMeshyBikes
{
    public static object Main()
    {
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
        var drivers=g.GetComponentsInChildren<BikeVisualAnimator>(true);
        check(drivers.Length==33,"All 33 animated motorcycle actors remain present");
        check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Customer jobs remain disabled");
        check(drivers.Count(d=>d.AssetName=="PV_TrafficPolice")==1,"One separate police tourer remains");
        var schemes=new List<Color>();
        foreach(string asset in new[]{"PV_HeroScooter","PV_CityScooter","PV_Underbone","PV_SportBike","PV_Racer"})
        {
            string root="Assets/PhoViet/Art/Bikes/Meshy/"+asset;
            check(File.ReadAllText(root+".json").Contains("meshy-scooter"),asset+": received Meshy geometry provenance");
            var imported=AssetDatabase.LoadAllAssetsAtPath(root+".fbx").OfType<Mesh>().ToArray();
            check(imported.Length==3,asset+": three imported skinned LOD meshes");
            var mat=AssetDatabase.LoadAssetAtPath<Material>(root+".mat");
            check(mat!=null&&mat.mainTexture.width==2048&&mat.GetTexture("_BumpMap")!=null&&mat.GetTexture("_MetallicGlossMap")!=null&&mat.GetTexture("_EmissionMap")!=null,asset+": 2048 atlas with normal, surface and lamp emission maps");
            var tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(root+"_Palette.png"));
            var pixels=tex.GetPixels();var saturated=pixels.Where(c=>c.maxColorComponent-Mathf.Min(c.r,c.g,c.b)>.35f&&c.maxColorComponent>.4f).ToArray();
            check(saturated.Length>100,asset+": nonempty clothing/body texture");
            // Body region only, not the unchanged rider palette in the bottom 128 rows.
            var source=new Texture2D(2,2);source.LoadImage(File.ReadAllBytes("Assets/MeshyImports/Red Honda Scooter Front View_20261023_023822/meshy_basecolor.png"));
            Color paint=Color.black;int count=0;
            for(int y=256;y<1800;y+=9)for(int x=0;x<2048;x+=9)
            {
                var original=source.GetPixelBilinear(x/2048f,(y-128)/1920f);
                if(original.r>original.g*1.3f&&original.r>original.b*1.3f&&original.r-original.g>.15f)
                {paint+=tex.GetPixel(x,y);count++;}
            }
            check(count>50,asset+": paint mask samples original red panels");paint/=count;schemes.Add(paint);
            if(asset=="PV_HeroScooter")check(Mathf.Abs(paint.r-paint.g)<.04f&&Mathf.Abs(paint.g-paint.b)<.05f&&paint.r>.35f,"Hero red paint changed to white while source black details survive");
            UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(source);
        }
        check(schemes.Skip(1).All(c=>Vector4.Distance(c,schemes[0])>.2f),"Four traffic/racer body paints differ from white-black hero");
        foreach(var bike in drivers)
        {
            bool police=bike.AssetName=="PV_TrafficPolice";
            var skins=bike.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            check(skins.Length==3&&skins.All(s=>AssetDatabase.GetAssetPath(s.sharedMesh).Contains(police?"Bikes/PV_TrafficPolice.fbx":"Bikes/Meshy/")),bike.name+": correct old police or new Meshy source");
            check(skins.All(s=>s.sharedMaterials.Length==1&&s.bones.Length==17&&s.bones.All(b=>b!=null)),bike.name+": one material, intact 17 bone skin");
            check(bike.clips.Length==7&&bike.clips.All(c=>c.legacy),bike.name+": all seven compatible clips");
            if(!police)check(bike.visual.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="PV_MeshyExhaust"&&t.parent.name=="Frame"),bike.name+": exhaust follows motorcycle frame");
        }
        int branch=0;
        foreach(var f in g.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh!=null&&f.sharedMesh.name.StartsWith("PV_RouteParked_",StringComparison.Ordinal)))
        {
            check(f.sharedMesh.GetIndexCount(0)/3<=11000&&f.sharedMesh.GetIndexCount(0)/3>10000,f.name+": branch parked Meshy LOD1 baked");
            check(AssetDatabase.GetAssetPath(f.GetComponent<MeshRenderer>().sharedMaterial.mainTexture).Contains("/Meshy/"),f.name+": branch parked Meshy paint");branch++;
        }
        check(branch>0,"Branch road parked meshes updated too");
        foreach(var lamp in g.daylight.vehicles)
        {
            check(lamp.head!=null&&lamp.halos.All(h=>h.source!=null),lamp.root.name+": live head and tail lamp sources");
            var b=lamp.root.GetComponent<BikeVisualAnimator>();if(b==null||b.AssetName=="PV_TrafficPolice")continue;
            check(lamp.head.parent.name=="Steering"&&lamp.head.IsChildOf(b.visual),b.name+": Meshy headlight follows steering and jump");
            check(lamp.halos.Where(h=>h.rear).All(h=>h.source.parent.name=="Frame"),b.name+": Meshy tail lamp follows frame");
        }
        return new{success=true,count=checks.Count,checks,branch,colors=schemes.Select(c=>new[]{c.r,c.g,c.b}).ToArray()};
    }
}
