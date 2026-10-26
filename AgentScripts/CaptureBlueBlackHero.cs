using System;using System.IO;using System.Linq;using UnityEngine;using PhoViet;
public static class CaptureBlueBlackHero
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;var probe=new GameObject("Blue black hero preview").AddComponent<Camera>();
  try{Time.timeScale=0;g.ResetRide();g.daylight.SetCycleTime(8);var a=g.rider.GetComponent<BikeVisualAnimator>();a.Initialize();a.Advance(0);
   probe.fieldOfView=32;probe.nearClipPlane=.05f;probe.farClipPlane=150;probe.clearFlags=CameraClearFlags.Skybox;
   foreach(bool rear in new[]{false,true}){probe.transform.position=g.rider.TransformPoint(rear?new Vector3(2.6f,2f,-3.4f):new Vector3(2.9f,1.5f,2.6f));probe.transform.LookAt(g.rider.TransformPoint(new Vector3(0,1.05f,.1f)));Render(probe,"Captures/hero-blue-black-"+(rear?"rear":"front")+".png");}
   var skins=g.rider.GetComponentsInChildren<SkinnedMeshRenderer>(true);var lamp=g.daylight.vehicles.Single(v=>v.root==g.rider);
   var result=new{success=true,bike=a.AssetName,role=a.role.ToString(),car=g.IsPlayerCar,kickPose=a.HasKickContactPose,skins=skins.Length,materials=skins.Select(s=>s.sharedMaterial.name).Distinct().ToArray(),halfWidth=g.PlayerHalfWidth,halfLength=g.PlayerHalfLength,lamps=lamp.halos.Length,jobsLocked=!g.jobsEnabled&&!PhoVietGame.RideJobsReleased};
   File.WriteAllText("Captures/hero-blue-black-bindings.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }finally{UnityEngine.Object.DestroyImmediate(probe.gameObject);g.ResetRide();Time.timeScale=scale;}
 }
 static void Render(Camera camera,string path)
 {var rt=new RenderTexture(960,720,24){antiAliasing=4};var im=new Texture2D(960,720,TextureFormat.RGB24,false);var active=RenderTexture.active;try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,960,720),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());}finally{camera.targetTexture=null;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(im);}}
}
