using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// Render the production shader in isolation; a pasted/flat cookie must fail these checks.
public static class VerifyHeadlightProjectionRender
{
    struct Reading { public float energy;public Vector2 centroid; }
    public static object Main()
    {
        var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
        GameObject receiver=null,cameraObject=null;Material material=null;RenderTexture target=null;Texture2D pixels=null;
        float night=Shader.GetGlobalFloat("_PVNightLights");bool fog=RenderSettings.fog;var old=RenderTexture.active;
        try
        {
            Shader.SetGlobalFloat("_PVNightLights",1);RenderSettings.fog=false;
            receiver=new GameObject("Temporary headlight projection receiver"){hideFlags=HideFlags.HideAndDontSave,layer=31};
            receiver.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/PhoViet/Art/Lighting/PV_OpticalGround.asset");
            var renderer=receiver.AddComponent<MeshRenderer>();material=new Material(Shader.Find("PhoViet/Optical Light"));
            material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Art/Lighting/PV_HeadlightCookie.png");material.SetColor("_Tint",new Color(.75f,.88f,1,.28f));material.SetFloat("_ProjectHeadlight",1);renderer.sharedMaterial=material;receiver.transform.localScale=new Vector3(64,1,64);
            cameraObject=new GameObject("Temporary optical verification camera"){hideFlags=HideFlags.HideAndDontSave};var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;
            camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.orthographic=true;camera.orthographicSize=32;camera.nearClipPlane=.1f;camera.farClipPlane=100;
            camera.transform.SetPositionAndRotation(new Vector3(0,50,0),Quaternion.Euler(90,0,0));
            target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;pixels=new Texture2D(256,256,TextureFormat.RGB24,false);
            Func<string,Vector3,Quaternion,Reading> render=(name,source,rotation)=>
            {
                material.SetMatrix("_WorldToHead",Matrix4x4.TRS(source,rotation,Vector3.one).inverse);material.SetVector("_HeadPosition",source);
                camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,256,256),0,0);pixels.Apply();
                var colors=pixels.GetPixels();float sum=0;Vector2 centre=Vector2.zero;
                for(int y=0;y<256;y++)for(int x=0;x<256;x++){var c=colors[y*256+x];float value=.2126f*c.r+.7152f*c.g+.0722f*c.b;sum+=value;centre+=new Vector2((x+.5f)/256*64-32,(y+.5f)/256*64-32)*value;}
                System.IO.File.WriteAllBytes("Captures/night-jump-projection-"+name+".png",pixels.EncodeToPNG());
                return new Reading{energy=sum,centroid=sum>.0001f?centre/sum:Vector2.zero};
            };
            var road=render("road",new Vector3(0,1.1f,0),Quaternion.identity);
            check(road.energy>10&&road.centroid.y>4,"Level headlamp renders an illuminated footprint ahead of the bike");
            check(Mathf.Abs(road.centroid.x)<.3f,"Level projected cookie stays centered on the headlamp direction");
            var wheelie=render("wheelie",new Vector3(0,1.4f,0),Quaternion.Euler(-35,0,0));
            check(wheelie.energy<road.energy*.15f,"Wheelie lifts the beam off the near road instead of leaving the original ground patch");
            var landing=render("landing",new Vector3(0,1.2f,0),Quaternion.Euler(20,0,0));
            check(landing.energy>1&&landing.centroid.y<road.centroid.y*.6f,"Nose-down landing brings the projected footprint closer to the bike");
            var roof=render("roof",new Vector3(0,4.3f,0),Quaternion.identity);
            check(roof.energy<road.energy*.7f&&Vector2.Distance(roof.centroid,road.centroid)>1,"Higher roof source changes and attenuates the road footprint");
            var turned=render("turned",new Vector3(0,1.1f,0),Quaternion.Euler(0,90,0));
            check(turned.centroid.x>4&&Mathf.Abs(turned.centroid.y)<.3f,"Reprojected headlight follows a ninety-degree street turn");
            check(Mathf.Abs(turned.energy-road.energy)<road.energy*.06f,"Street-frame rotation preserves the headlight profile and brightness");
            Shader.SetGlobalFloat("_PVNightLights",0);var day=render("day",new Vector3(0,1.1f,0),Quaternion.identity);
            check(day.energy<.001f,"Projection remains fully off in daylight");
            return new{success=true,count=checks.Count,roadEnergy=road.energy,wheelieEnergy=wheelie.energy,roofEnergy=roof.energy,checks};
        }
        finally
        {
            RenderTexture.active=old;RenderSettings.fog=fog;Shader.SetGlobalFloat("_PVNightLights",night);
            if(cameraObject!=null)UnityEngine.Object.DestroyImmediate(cameraObject);if(receiver!=null)UnityEngine.Object.DestroyImmediate(receiver);if(material!=null)UnityEngine.Object.DestroyImmediate(material);if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
        }
    }
}
