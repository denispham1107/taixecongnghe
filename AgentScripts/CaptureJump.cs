using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class CaptureJump
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var camera=g.rideCamera;float scale=Time.timeScale;bool jobs=g.jobsEnabled;
  var target=camera.targetTexture;var pos=camera.transform.localPosition;var rot=camera.transform.localRotation;float fov=camera.fieldOfView;
  var rt=new RenderTexture(1200,900,24);var image=new Texture2D(1200,900,TextureFormat.RGB24,false);var previous=RenderTexture.active;
  const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<string> shot=name=>{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes("Captures/jump-"+name+".png",image.EncodeToPNG());};
  try
  {
   Time.timeScale=0;g.jobsEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   var hero=g.rider.GetComponent<BikeVisualAnimator>();float x=g.rider.localPosition.x;
   camera.transform.localPosition=new Vector3(x+4.8f,3.1f,-5.8f);camera.transform.LookAt(g.transform.TransformPoint(new Vector3(x,1.1f,0)));camera.fieldOfView=40;
   set("jumpWindup",.075f);for(int i=0;i<5;i++)hero.Advance(.03f);shot("wheelie");
   set("jumpWindup",0f);set("jumpFlying",true);set("jumpHeight",3.1f);set("jumpFlightAge",.55f);g.rider.localPosition=new Vector3(x,3.1f,0);
   camera.transform.localPosition=new Vector3(x+5.8f,5.3f,-7);camera.transform.LookAt(g.transform.TransformPoint(new Vector3(x,2.3f,0)));camera.fieldOfView=43;
   hero.Advance(.01f);shot("air");
   int index=g.vehicles.FindIndex(v=>v.model.GetComponent<CarVisualAnimator>()?.AssetName=="PV_Sedan");var car=g.SpawnTraffic(2,0,index);
   set("jumpFlying",false);set("jumpHeight",1.625f);set("roofVehicle",car);set("jumpLanding",.38f);set("landingClip","LandRear");g.rider.localPosition=new Vector3(x,1.625f,0);
   car.model.GetComponent<CarVisualAnimator>().Advance(.01f);hero.Advance(.01f);shot("roof");
   set("roofVehicle",null);set("jumpHeight",0f);set("jumpLanding",.35f);set("landingClip","LandFront");g.rider.localPosition=new Vector3(x,0,0);car.active=false;car.model.gameObject.SetActive(false);
   camera.transform.localPosition=new Vector3(x+4.8f,2.7f,-5.8f);camera.transform.LookAt(g.transform.TransformPoint(new Vector3(x,1,0)));camera.fieldOfView=40;hero.Advance(.01f);shot("landing");
   return new{success=true,files=new[]{"jump-wheelie.png","jump-air.png","jump-roof.png","jump-landing.png"}};
  }
  finally
  {
   g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=scale;camera.targetTexture=target;camera.transform.localPosition=pos;camera.transform.localRotation=rot;camera.fieldOfView=fov;RenderTexture.active=previous;
   UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
  }
 }
}
