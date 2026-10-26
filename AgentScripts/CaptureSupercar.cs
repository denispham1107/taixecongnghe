using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class CaptureSupercar
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  var car=g.rider.GetComponent<PlayerCarVisual>();float scale=Time.timeScale;bool dinos=g.dinosaursEnabled,rivers=g.riversEnabled,tanks=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises;var files=new List<string>();
  var cam=new GameObject("Supercar preview").AddComponent<Camera>();const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action reset=()=>{g.dinosaursEnabled=g.riversEnabled=g.tankBarrageEnabled=g.recklessTrafficEnabled=g.trafficSurprises=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,10000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);g.daylight.SetCycleTime(8);RenderSettings.fog=false;};
  Action<string,int,int> shot=(name,w,h)=>{string path="Captures/supercar-"+name+".png";Render(cam,car,path,w,h);files.Add(path);};
  try
  {
   Time.timeScale=0;reset();car.Advance(0);cam.CopyFrom(g.rideCamera);cam.fieldOfView=45;cam.nearClipPlane=.05f;
   foreach(int dir in new[]{1,-1}){cam.transform.position=g.rider.position+new Vector3(4,2.5f,dir*5);cam.transform.LookAt(g.rider.position+Vector3.up*.45f);shot(dir>0?"front":"rear",960,640);}
   foreach(bool portrait in new[]{false,true})
   {typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});cam.CopyFrom(g.rideCamera);cam.transform.SetPositionAndRotation(g.rideCamera.transform.position,g.rideCamera.transform.rotation);
    if(portrait){cam.fieldOfView=2*Mathf.Atan(Mathf.Tan(65*Mathf.Deg2Rad*.5f)/(.75f*.70f*1.10f))*Mathf.Rad2Deg;cam.transform.localPosition=new Vector3(g.rider.localPosition.x*.85f,7.15f,-9.6f);cam.transform.LookAt(g.transform.TransformPoint(new Vector3(g.rider.localPosition.x*.85f,1.2f,8)));}
    shot(portrait?"portrait":"landscape",portrait?540:1100,portrait?960:680);}
   reset();g.TryBoost();for(int i=0;i<12;i++){tick(.02f);car.Advance(.02f);g.rider.GetComponent<BoostSmokeVisual>().Simulate(.02f);}cam.fieldOfView=50;cam.transform.position=g.rider.position+new Vector3(3.9f,2.6f,-5.5f);cam.transform.LookAt(g.rider.position+new Vector3(0,.5f,-.6f));shot("dual-boost",1100,720);
   reset();g.TryJump();var jets=g.rider.GetComponent<CarJumpJetVisual>();
   for(int i=0;i<37;i++){tick(.02f);car.Advance(.02f);jets.Simulate(.02f);}cam.fieldOfView=53;cam.transform.position=g.rider.position+new Vector3(4,1.3f,-6.2f);cam.transform.LookAt(g.rider.position+Vector3.down*.7f);shot("four-lift-jets",1100,760);
   reset();tick(.02f);g.MoveLane(1);for(int i=0;i<33;i++){tick(.02f);car.Advance(.02f);}cam.fieldOfView=45;cam.transform.position=g.rider.position+new Vector3(0,4,-7);cam.transform.LookAt(g.rider.position+Vector3.up*.3f);shot("lane-change",1000,700);
   g.daylight.SetCycleTime(30);g.daylight.UpdateLamps();shot("night-lights",1000,700);
   return new{success=true,files};
  }
  finally{UnityEngine.Object.DestroyImmediate(cam.gameObject);g.dinosaursEnabled=dinos;g.riversEnabled=rivers;g.tankBarrageEnabled=tanks;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.ResetRide();Time.timeScale=scale;}
 }
 static void Render(Camera cam,PlayerCarVisual car,string file,int width,int height)
 {
  var meshes=new List<Mesh>();var proxies=new List<GameObject>();var skins=car.visual.GetComponentsInChildren<SkinnedMeshRenderer>();var enabled=skins.Select(s=>s.enabled).ToArray();
  var skin=(SkinnedMeshRenderer)car.visual.GetComponent<LODGroup>().GetLODs()[0].renderers[0];var mesh=new Mesh();skin.BakeMesh(mesh);meshes.Add(mesh);
  var proxy=new GameObject("Supercar baked preview");proxy.transform.SetPositionAndRotation(skin.transform.position,skin.transform.rotation);proxy.transform.localScale=skin.transform.lossyScale;proxy.AddComponent<MeshFilter>().sharedMesh=mesh;proxy.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;proxies.Add(proxy);foreach(var s in skins)s.enabled=false;
  var rt=new RenderTexture(width,height,24){antiAliasing=4};var image=new Texture2D(width,height,TextureFormat.RGB24,false);var previous=RenderTexture.active;
  try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());}
  finally{cam.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);foreach(var p in proxies)UnityEngine.Object.DestroyImmediate(p);foreach(var m in meshes)UnityEngine.Object.DestroyImmediate(m);for(int i=0;i<skins.Length;i++)skins[i].enabled=enabled[i];}
 }
}
