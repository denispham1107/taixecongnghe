using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class CaptureCrashUpgrade
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  var c=g.rideCamera;float scale=Time.timeScale;bool jobs=g.jobsEnabled;var target=c.targetTexture;var pos=c.transform.localPosition;var rot=c.transform.localRotation;float fov=c.fieldOfView;
  var rt=new RenderTexture(1200,900,24);var image=new Texture2D(1200,900,TextureFormat.RGB24,false);var previous=RenderTexture.active;
  const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
  Action<string,object[]> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action clean=()=>{g.jobsEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}typeof(PhoVietGame).GetProperty("Speed").SetValue(g,0f);};
  Transform follow=null;
  Action<string> shot=name=>{if(follow!=null){var focus=follow.localPosition+Vector3.up*.8f;c.transform.localPosition=focus+new Vector3(-3,2.2f,-4);c.transform.LookAt(g.transform.TransformPoint(focus));c.fieldOfView=48;}c.targetTexture=rt;c.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes("Captures/crash-upgrade-"+name+".png",image.EncodeToPNG());};
  try
  {
   Time.timeScale=0;
   foreach(bool truck in new[]{false,true})
   {
    clean();var v=g.SpawnReckless(truck,true);v.enteringFromBehind=false;v.model.localPosition=new Vector3(10,0,14);call("CheckBuildingImpact",new object[]{v});
    v.model.GetComponent<CarVisualAnimator>()?.Advance(.02f);v.model.GetComponent<BikeVisualAnimator>()?.Advance(.02f);
    v.fire.Simulate(3,true,true);v.smoke.Simulate(3,true,true);
    c.transform.localPosition=new Vector3(1,3.7f,7);c.transform.LookAt(g.transform.TransformPoint(new Vector3(7.2f,1.8f,14)));c.fieldOfView=53;
    shot(truck?"truck-fire":"bike-fire");
   }
   clean();int i=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);var bike=g.SpawnTraffic(3,5,i);bike.speed=bike.baseTrafficSpeed=8;bike.motionVelocity=Vector3.forward*8;bike.laneCooldown=bike.brakeCooldown=1000;
   call("KnockVehicle",new object[]{bike,1f,false,false});
   follow=bike.model;
   c.transform.localPosition=new Vector3(1,3,-2);c.transform.LookAt(g.transform.TransformPoint(new Vector3(4.5f,1,5)));c.fieldOfView=50;
   for(int j=0;j<45;j++)call("TickTrafficMotion",new object[]{.02f});bike.model.GetComponent<BikeVisualAnimator>().Advance(.01f);shot("fallen");
   for(int j=0;j<120;j++)call("TickTrafficMotion",new object[]{.02f});bike.model.GetComponent<BikeVisualAnimator>().Advance(.01f);shot("standing");
   for(int j=0;j<28;j++)call("TickTrafficMotion",new object[]{.02f});bike.model.GetComponent<BikeVisualAnimator>().Advance(.01f);shot("resumed");
   return new{success=true,state=g.State.ToString(),npc=bike.model.GetComponent<BikeVisualAnimator>().CurrentClip,knocked=bike.knocked,phase=bike.recovery!=null?bike.recovery.phase:-1,standing=bike.StandUpProgress,speed=bike.speed};
  }
  finally{g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=scale;c.targetTexture=target;c.transform.localPosition=pos;c.transform.localRotation=rot;c.fieldOfView=fov;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
 }
}
