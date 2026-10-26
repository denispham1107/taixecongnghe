using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class CaptureUrbanRider
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;var files=new List<string>();var cam=new GameObject("Urban preview").AddComponent<Camera>();
  try
  {
   Time.timeScale=0;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}g.daylight.SetCycleTime(8);RenderSettings.fog=false;
   var v1=g.SpawnTraffic(2,8,g.vehicles.FindIndex(v=>v.model.GetComponent<UrbanRiderMotion>()!=null&&!v.eventOnly));v1.model.localRotation=Quaternion.identity;var b=v1.model.GetComponent<BikeVisualAnimator>();b.Initialize();b.Advance(.02f);
   cam.CopyFrom(g.rideCamera);cam.fieldOfView=40;cam.nearClipPlane=.05f;
   Action<string,Vector3,Vector3> shot=(name,offset,look)=>{cam.transform.position=v1.model.position+offset;cam.transform.LookAt(v1.model.position+look);string file="Captures/urban-rider-"+name+".png";Render(cam,b,file);files.Add(file);};
   shot("front",new Vector3(2.6f,2.0f,3.7f),Vector3.up*1.05f);shot("side",new Vector3(3.5f,1.65f,.1f),Vector3.up*1.05f);shot("rear",new Vector3(2.3f,2.0f,-3.6f),Vector3.up*1.05f);
   v1.motionVelocity=Vector3.forward*9;
   for(int i=0;i<25;i++){v1.model.localRotation=Quaternion.Euler(0,i*1.2f,0);b.Advance(.02f);}
   shot("steering",new Vector3(2.3f,1.9f,3.7f),Vector3.up*1.1f);
   shot("grip",new Vector3(1.05f,1.65f,1.25f),new Vector3(.1f,1.4f,.1f));
   // Full-lock corner inspection and separate colour variants.
   for(int i=0;i<40;i++){v1.model.localRotation=Quaternion.Euler(0,30+i*3.5f,0);b.Advance(.02f);}
   shot("corner",new Vector3(1.5f,2.1f,-3.8f),Vector3.up*1.1f);
   var original=b.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(s=>s.sharedMaterial).ToArray();
   foreach(var source in g.GetComponentsInChildren<BikeVisualAnimator>(true).Where(a=>a.urbanMotion!=null).GroupBy(a=>a.AssetName).Select(a=>a.First()))
   {
    var mat=source.visual.GetComponentInChildren<SkinnedMeshRenderer>(true).sharedMaterial;
    foreach(var skin in b.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.sharedMaterial=mat;
    v1.model.localRotation=Quaternion.identity;for(int i=0;i<100;i++)b.Advance(.02f);
    shot("colour-"+source.AssetName,new Vector3(2.6f,2.0f,3.7f),Vector3.up*1.05f);
   }
   var restore=b.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);for(int i=0;i<restore.Length;i++)restore[i].sharedMaterial=original[i];
   v1.model.localRotation=Quaternion.identity;for(int i=0;i<100;i++)b.Advance(.02f);
   cam.CopyFrom(g.rideCamera);cam.transform.SetPositionAndRotation(g.rideCamera.transform.position,g.rideCamera.transform.rotation);Render(cam,b,"Captures/urban-rider-game-landscape.png");files.Add("Captures/urban-rider-game-landscape.png");
   g.daylight.SetCycleTime(30);g.daylight.UpdateLamps();shot("night",new Vector3(2.6f,2.0f,3.7f),Vector3.up*1.05f);
   return new{success=true,files,steering=b.urbanMotion.SteeringAngle,lean=b.urbanMotion.LeanAngle,leftError=b.urbanMotion.LeftArmError,rightError=b.urbanMotion.RightArmError};
  }
  finally{UnityEngine.Object.DestroyImmediate(cam.gameObject);g.ResetRide();Time.timeScale=scale;}
 }
 static void Render(Camera cam,BikeVisualAnimator b,string file)
 {
  var skins=b.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);var states=skins.Select(s=>s.enabled).ToArray();var skin=(SkinnedMeshRenderer)b.visual.GetComponent<LODGroup>().GetLODs()[0].renderers[0];var mesh=new Mesh();skin.BakeMesh(mesh);var p=new GameObject("Urban baked preview");p.transform.SetPositionAndRotation(skin.transform.position,skin.transform.rotation);p.transform.localScale=skin.transform.lossyScale;p.AddComponent<MeshFilter>().sharedMesh=mesh;p.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;foreach(var s in skins)s.enabled=false;
  var rt=new RenderTexture(1100,800,24){antiAliasing=4};var tex=new Texture2D(1100,800,TextureFormat.RGB24,false);var previous=RenderTexture.active;
  try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1100,800),0,0);tex.Apply();File.WriteAllBytes(file,tex.EncodeToPNG());}
  finally{cam.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(p);UnityEngine.Object.DestroyImmediate(mesh);for(int i=0;i<skins.Length;i++)skins[i].enabled=states[i];}
 }
}
