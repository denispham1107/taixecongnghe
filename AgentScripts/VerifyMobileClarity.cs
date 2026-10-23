using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using PhoViet;
public static class VerifyMobileClarity
{
    public static object Main()
    {
        var checks=new List<string>();Action<bool,string> check=(ok,msg)=>{if(!ok)throw new Exception(msg);checks.Add(msg);};
        var files=System.IO.Directory.GetFiles("Assets/PhoViet/Art/Bikes/MeshyRiders","*.png");
        var textures=new List<object>();long bytes=0;
        foreach(var path in files)
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            check(texture!=null&&texture.width==2048&&texture.height==4096,path+": imported full resolution survives platform settings");
            check(texture.mipmapCount==13,path+": complete mip chain prevents distant shimmer");
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            check(importer.filterMode==FilterMode.Trilinear&&importer.anisoLevel==4,path+": oblique texture filtering");
            check(importer.textureCompression==TextureImporterCompression.CompressedHQ&&importer.compressionQuality==100,path+": high quality compression");
            check(!texture.isReadable,path+": no duplicate readable CPU texture retained");
            long memory=Profiler.GetRuntimeMemorySizeLong(texture);bytes+=memory;
            textures.Add(new{path,width=texture.width,height=texture.height,format=texture.format.ToString(),mips=texture.mipmapCount,bytes=memory});
        }
        check(files.Length==8,"Five color atlases plus three shared maps, no per-actor duplicates");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
        // Unity removes a few degenerate source triangles during FBX import.
        int heroTriangles=g.rider.GetComponent<BikeVisualAnimator>().visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Max(s=>s.sharedMesh.triangles.Length/3);
        check(heroTriangles>42000&&heroTriangles<=42939,"Hero full-detail source survives FBX import without LOD decimation");
        return new{success=true,count=checks.Count,checks,textures,totalImportedTextureBytes=bytes,heroTriangles};
    }
}
