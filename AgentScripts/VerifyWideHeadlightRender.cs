using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;
public static class VerifyWideHeadlightRender
{
 sealed class Reading {public float energy,width,z;public Color[] pixels;}
 public static object Main()
 {
  var checks=new List<string>();Action<bool,string> check=(b,n)=>{if(!b)throw new Exception(n);checks.Add(n);};
  GameObject plane=null,camObject=null;Material material=null;Shader baseline=null;RenderTexture target=null;Texture2D tex=null;
  var previous=RenderTexture.active;bool fog=RenderSettings.fog;float night=Shader.GetGlobalFloat("_PVNightLights");
  try
  {
   var shader=Shader.Find("PhoViet/Optical Light");check(shader!=null&&shader.isSupported&&!ShaderUtil.ShaderHasError(shader),"Production shader supported without compile errors");
   baseline=ShaderUtil.CreateShaderAsset(File.ReadAllText("AgentScripts/Fixtures/OpticalLight152.shader.txt").Replace("PhoViet/Optical Light","Hidden/PV_HeadlightBaseline"),true);
   check(!ShaderUtil.ShaderHasError(baseline),"Previous single beam compiled as an isolated comparison");
   Shader.SetGlobalFloat("_PVNightLights",1);RenderSettings.fog=false;
   plane=new GameObject("Temporary wide headlamp receiver"){hideFlags=HideFlags.HideAndDontSave,layer=31};
   plane.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/PhoViet/Art/Lighting/PV_OpticalGround.asset");
   plane.transform.localScale=new Vector3(64,1,64);var renderer=plane.AddComponent<MeshRenderer>();
   material=new Material(shader);material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Art/Lighting/PV_HeadlightCookie.png");material.SetColor("_Tint",new Color(.75f,.88f,1,.28f));material.SetFloat("_ProjectHeadlight",1);renderer.sharedMaterial=material;
   camObject=new GameObject("Temporary wide headlamp camera"){hideFlags=HideFlags.HideAndDontSave};var cam=camObject.AddComponent<Camera>();cam.enabled=false;cam.cullingMask=1<<31;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;cam.orthographic=true;cam.orthographicSize=32;cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.transform.SetPositionAndRotation(new Vector3(0,50,0),Quaternion.Euler(90,0,0));
   target=new RenderTexture(512,512,24,RenderTextureFormat.ARGB32);cam.targetTexture=target;tex=new Texture2D(512,512,TextureFormat.RGB24,false);
   Func<Shader,bool,Vector3,Quaternion,Reading> render=(s,pair,source,rot)=>
   {
    material.shader=s;if(s==shader){material.SetFloat("_HeadlightWidth",pair?2:1);material.SetFloat("_HeadlightHeight",pair?.65f:1.1f);}
    material.SetMatrix("_WorldToHead",Matrix4x4.TRS(source,rot,Vector3.one).inverse);material.SetVector("_HeadPosition",source);
    cam.Render();RenderTexture.active=target;tex.ReadPixels(new Rect(0,0,512,512),0,0);tex.Apply();var pixels=tex.GetPixels();float sum=0,x2=0,z=0;
    for(int y=0;y<512;y++)for(int x=0;x<512;x++){float v=pixels[y*512+x].grayscale,wx=(x+.5f)/512*64-32,wz=(y+.5f)/512*64-32;sum+=v;x2+=wx*wx*v;z+=wz*v;}
    return new Reading{energy=sum,width=Mathf.Sqrt(x2/Mathf.Max(sum,.001f)),z=z/Mathf.Max(sum,.001f),pixels=pixels};
   };
   var old=render(baseline,false,new Vector3(0,.391f,0),Quaternion.identity);var single=render(shader,false,new Vector3(0,.391f,0),Quaternion.identity);
   float delta=old.pixels.Zip(single.pixels,(a,b)=>Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)).Max();check(delta<.012f,"Single headlamp retains previous rendered pixels (maximum RGB error below one byte per channel)");
   var wideRoad=render(shader,true,new Vector3(0,.391f,0),Quaternion.identity);File.WriteAllBytes("Captures/wide-headlights-projected-road.png",tex.EncodeToPNG());
   check(wideRoad.width>old.width*1.6f,"Wide beam footprint is visibly wider than the former central cone");check(wideRoad.z>old.z*1.35f,"Wide car fan reaches the intended area ahead");
   Func<Reading,float,float,float> sample=(r,x,z)=>r.pixels[Mathf.Clamp((int)((z+32)/64*512),0,511)*512+Mathf.Clamp((int)((x+32)/64*512),0,511)].grayscale;
   check(sample(wideRoad,-2,6)>sample(old,-2,6)+.005f&&sample(wideRoad,2,6)>sample(old,2,6)+.005f,"Both outer sides gain illumination symmetrically");
   float symmetry=0;bool smooth=true;
   foreach(float z in new[]{1.5f,3f,6f})
   {
    int row=(int)((z+32)/64*512);float previousSample=wideRoad.pixels[row*512+256].grayscale;
    for(int x=256;x<512;x++)
    {
     float value=wideRoad.pixels[row*512+x].grayscale;float mirror=wideRoad.pixels[row*512+511-x].grayscale;symmetry=Mathf.Max(symmetry,Mathf.Abs(value-mirror));
     if(value>previousSample+2.1f/255)smooth=false;previousSample=value;
    }
   }
   check(symmetry<2.1f/255,"Left and right beam edges match within two levels of 8-bit render quantization");
   check(smooth,"One connected fan has a single centre peak, without two separate lobes");
   var turned=render(shader,true,new Vector3(0,.391f,0),Quaternion.Euler(0,90,0));check(Mathf.Abs(turned.energy-wideRoad.energy)<wideRoad.energy*.02f,"Wide footprint survives a 90-degree street-frame turn without brightness change");
   var raised=render(shader,true,new Vector3(0,3.5f,0),Quaternion.identity);check(raised.z>wideRoad.z&&raised.energy<wideRoad.energy,"Raised roof/jump source changes reach and attenuation");
   var noseUp=render(shader,true,new Vector3(0,1.4f,0),Quaternion.Euler(-35,0,0));check(noseUp.energy<wideRoad.energy*.15f,"Whole fan lifts away from road during a wheelie");
   var noseDown=render(shader,true,new Vector3(0,1.2f,0),Quaternion.Euler(20,0,0));check(noseDown.energy>1&&noseDown.z<wideRoad.z,"Nose-down landing brings whole fan closer");
   Shader.SetGlobalFloat("_PVNightLights",0);var day=render(shader,true,new Vector3(0,.391f,0),Quaternion.identity);check(day.energy<.001f,"Wide headlights remain off in clear daylight");
   Shader.SetGlobalFloat("_PVNightLights",1);material.SetFloat("_ProjectHeadlight",0);
   var oldFlat=render(baseline,false,new Vector3(0,1.1f,0),Quaternion.identity);var newFlat=render(shader,false,new Vector3(0,1.1f,0),Quaternion.identity);
   check(oldFlat.pixels.Zip(newFlat.pixels,(a,b)=>Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)).Max()<.001f,"Non-projecting street pools and halos retain identical rendered pixels");
   var result=new{success=true,count=checks.Count,checks,oldWidth=old.width,newWidth=wideRoad.width,oldReach=old.z,newReach=wideRoad.z,singlePixelMaxError=delta,symmetryPixelMaxError=symmetry};File.WriteAllText("Captures/wide-headlights-render-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{RenderTexture.active=previous;RenderSettings.fog=fog;Shader.SetGlobalFloat("_PVNightLights",night);if(camObject!=null)UnityEngine.Object.DestroyImmediate(camObject);if(plane!=null)UnityEngine.Object.DestroyImmediate(plane);if(material!=null)UnityEngine.Object.DestroyImmediate(material);if(baseline!=null)UnityEngine.Object.DestroyImmediate(baseline);if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}}
 }
}
