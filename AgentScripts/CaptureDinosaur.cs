using System;using System.IO;using System.Reflection;using UnityEngine;using PhoViet;
public static class CaptureDinosaur
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;var camera=new GameObject("Dino preview").AddComponent<Camera>();
  try
  {
   Time.timeScale=0;g.RestartRide();g.daylight.SetCycleTime(8);
   var d=g.SpawnDinosaur();d.root.localPosition=new Vector3(0,0,18);
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   camera.fieldOfView=52;camera.nearClipPlane=.1f;camera.farClipPlane=220;camera.clearFlags=CameraClearFlags.Skybox;
   camera.transform.position=g.transform.TransformPoint(new Vector3(0,6,-10));camera.transform.LookAt(g.transform.TransformPoint(new Vector3(0,2,15)));
   Render(camera,"Captures/dinosaur-unity-approach.png");
   d.visual.StartSweep();d.visual.Advance(1.18f);Render(camera,"Captures/dinosaur-unity-sweep.png");
   return new{success=true,tail0=g.transform.InverseTransformPoint(d.visual.tail[0].position),tailTip=g.transform.InverseTransformPoint(d.visual.tail[5].position),d.visual.SweepAge};
  }
  finally{UnityEngine.Object.DestroyImmediate(camera.gameObject);g.ResetRide();Time.timeScale=scale;}
 }
 static void Render(Camera camera,string file)
 {
  var rt=new RenderTexture(1100,760,24){antiAliasing=4};var image=new Texture2D(1100,760,TextureFormat.RGB24,false);var old=RenderTexture.active;
  var copies=new System.Collections.Generic.List<GameObject>();var meshes=new System.Collections.Generic.List<Mesh>();var skins=UnityEngine.Object.FindObjectsByType<DinosaurVisual>(FindObjectsSortMode.None);
  try
  {
   // Synchronous CLI captures run before the normal skinning/render loop: bake the sampled pose.
   foreach(var actor in skins)foreach(var skin in actor.GetComponentsInChildren<SkinnedMeshRenderer>())
   {var mesh=new Mesh();skin.BakeMesh(mesh);meshes.Add(mesh);var go=new GameObject("Preview baked dinosaur");go.transform.SetParent(skin.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;copies.Add(go);skin.enabled=false;}
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1100,760),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());
  }
  finally{foreach(var go in copies)UnityEngine.Object.DestroyImmediate(go);foreach(var mesh in meshes)UnityEngine.Object.DestroyImmediate(mesh);foreach(var actor in skins)foreach(var skin in actor.GetComponentsInChildren<SkinnedMeshRenderer>())skin.enabled=true;camera.targetTexture=null;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
 }
}
