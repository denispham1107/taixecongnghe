using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class CaptureMeshyCars
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale,cycle=g.daylight.ElapsedTime;
  var probe=new GameObject("Meshy car inspection camera").AddComponent<Camera>();var files=new List<string>();
  var cars=g.vehicles.Where(v=>v.model.GetComponent<MeshyCarVariants>()!=null).ToArray();
  try
  {
   Time.timeScale=0;g.RestartRide();g.daylight.SetCycleTime(0);probe.cullingMask=1<<30;probe.clearFlags=CameraClearFlags.SolidColor;probe.backgroundColor=new Color(.33f,.40f,.44f);probe.orthographic=true;probe.nearClipPlane=.05f;probe.farClipPlane=60;
   foreach(string asset in new[]{"PV_MeshySport","PV_MeshyPickup","PV_MeshyCovered","PV_MeshyContainer"})
   {
    var v=cars.First(v=>v.model.GetComponent<MeshyCarVariants>().variants.Any(e=>e.asset==asset));var c=v.model.GetComponent<MeshyCarVariants>();int index=Array.FindIndex(c.variants,e=>e.asset==asset);v.active=true;v.model.gameObject.SetActive(true);v.model.SetPositionAndRotation(Vector3.zero,Quaternion.identity);v.speed=0;v.knocked=false;
    foreach(int paint in new[]{0,2,3})
    {
     c.Select(index,paint,v);c.driver.Advance(0);var entry=c.variants[index];var skin=(SkinnedMeshRenderer)entry.skins[0];var mesh=new Mesh();skin.BakeMesh(mesh);
     var proxy=new GameObject("Baked Meshy car inspection");proxy.layer=30;proxy.transform.SetPositionAndRotation(skin.transform.position,skin.transform.rotation);proxy.transform.localScale=skin.transform.lossyScale;proxy.AddComponent<MeshFilter>().sharedMesh=mesh;var r=proxy.AddComponent<MeshRenderer>();r.sharedMaterial=skin.sharedMaterial;var block=new MaterialPropertyBlock();skin.GetPropertyBlock(block);r.SetPropertyBlock(block);
     float length=entry.size.z,height=entry.size.y;probe.orthographicSize=length*.43f;probe.transform.position=new Vector3(length*.85f,height*1.7f,length*1.10f);probe.transform.LookAt(new Vector3(0,height*.45f,0));
     string file="Captures/meshy-cars-"+asset+"-paint"+paint+".png";Render(probe,file);files.Add(file);UnityEngine.Object.DestroyImmediate(proxy);UnityEngine.Object.DestroyImmediate(mesh);
    }
    v.active=false;v.model.gameObject.SetActive(false);
   }
   return new{success=true,files};
  }
  finally{UnityEngine.Object.DestroyImmediate(probe.gameObject);g.ResetRide();g.daylight.SetCycleTime(cycle);Time.timeScale=time;}
 }
 static void Render(Camera camera,string file)
 {
  var rt=new RenderTexture(960,640,24){antiAliasing=4};var image=new Texture2D(960,640,TextureFormat.RGB24,false);var previous=RenderTexture.active;
  try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,960,640),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());}
  finally{camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
 }
}
