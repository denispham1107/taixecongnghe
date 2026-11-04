using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections;using UnityEditor;using UnityEngine;using PhoViet;
public static class CaptureContactFire2127
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static object Call(PhoVietGame g,string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Set(PhoVietGame g,string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);bool sound=(bool)typeof(PhoVietGame).GetField("sound",F).GetValue(g);
  bool[] flags={g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  var cam=Camera.main;var position=cam.transform.position;var rotation=cam.transform.rotation;var oldTarget=cam.targetTexture;var oldActive=RenderTexture.active;
  try
  {
   Time.timeScale=0;Set(g,"sound",false);
   for(int frame=0;frame<4;frame++)
   {
    g.RestartRide();g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
    foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set(g,"spawnTimer",99999f);Set(g,"Speed",0f);
    int car=g.vehicles.FindIndex(v=>!v.eventOnly&&v.model.GetComponent<MeshyCarVariants>()!=null);int bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);
    var a=g.SpawnTraffic(2,12,car);a.model.GetComponent<MeshyCarVariants>().Select(0,3,a);a.model.localPosition=a.previousPosition=new Vector3(0,0,10);a.model.localRotation=Quaternion.identity;a.motionVelocity=Vector3.zero;
    var b=g.SpawnTraffic(2,0,bike);b.model.localPosition=b.previousPosition=new Vector3(0,0,0);b.model.localRotation=Quaternion.identity;b.motionVelocity=Vector3.forward*30;b.model.localPosition=new Vector3(0,0,10);Call(g,"ResolveVehicleCollisions");
    // A second real side impact on the same already-crashed car.
    b.active=false;b.model.gameObject.SetActive(false);int bike2=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f&&v!=b);b=g.SpawnTraffic(2,20,bike2);a.previousPosition=a.model.localPosition;
    b.previousPosition=a.model.localPosition+Vector3.left*(a.halfWidth+b.halfWidth+6);b.model.localPosition=a.model.localPosition;b.model.localRotation=Quaternion.identity;b.motionVelocity=Vector3.right*30;Set(g,"timeRiding",1f);Call(g,"ResolveVehicleCollisions");
    // Fixture composition keeps the actual local contact anchors visible.
    a.model.localPosition=new Vector3(-1.8f,0,10);a.model.localRotation=Quaternion.Euler(0,15,0);b.model.localPosition=new Vector3(-4,0,9);b.model.localRotation=Quaternion.Euler(0,20,78);
    if(frame==2)
    {for(int i=0;i<100&&a.knocked;i++)Call(g,"TickTrafficRecovery",a,.05f);a.model.localPosition=new Vector3(-1.8f,0,10);a.model.localRotation=Quaternion.identity;}
    // Player's genuine swept rear collision, separated for the close view.
    var p=g.rider.localPosition;var hit=g.SpawnTraffic(2,0,car+1);hit.model.localRotation=Quaternion.identity;hit.previousPosition=p-Vector3.forward*(hit.halfLength+g.PlayerHalfLength+8);hit.model.localPosition=p;hit.motionVelocity=Vector3.forward*35;Set(g,"oldRiderZ",p.z);Call(g,"ResolveRiderContacts",p.x,.05f);
    hit.model.localPosition+=Vector3.right*3;g.rider.localRotation=Quaternion.Euler(0,-8,-18);
    g.daylight.SetCycleTime(frame==1?30:6);g.daylight.UpdateLamps();Call(g,"RenderThunderstorm");
    var fx=(ContactFireVisual)typeof(PhoVietGame).GetField("contactFire",F).GetValue(g);
    foreach(var spot in (IEnumerable)typeof(ContactFireVisual).GetField("spots",F).GetValue(fx))if((bool)spot.GetType().GetField("active").GetValue(spot))foreach(var ps in (ParticleSystem[])spot.GetType().GetField("particles").GetValue(spot))ps.Simulate(.75f,false,false);
    int w=frame==3?540:1440,h=frame==3?960:900;
    cam.transform.position=g.transform.TransformPoint(frame==2?new Vector3(6,5,0):frame==3?new Vector3(5,13,-16):new Vector3(5.5f,9,-14));cam.transform.LookAt(g.transform.TransformPoint(frame==2?new Vector3(-1,1,10):new Vector3(0,1,3)));cam.aspect=(float)w/h;
    var rt=new RenderTexture(w,h,24);var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
    try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes("Captures/contact2127-"+new[]{"day-multiple","night","recovered-moving","portrait"}[frame]+".png",tex.EncodeToPNG());}
    finally{cam.targetTexture=oldTarget;RenderTexture.active=oldActive;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);}
   }
   return new{success=true,frames=4};
  }
  finally{cam.targetTexture=oldTarget;RenderTexture.active=oldActive;cam.transform.SetPositionAndRotation(position,rotation);cam.ResetAspect();g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];Set(g,"sound",sound);Set(g,"BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);g.ResetRide();Time.timeScale=scale;}
 }
}
