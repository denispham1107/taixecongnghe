using System;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
// Ảnh xe tăng Meshy trong game (Play Mode): cận xe tăng trên phố và khoảnh khắc ô tô tông đuôi bị văng.
public static class CaptureMeshyTank
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool surprises=g.trafficSurprises,danger=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  var slots=g.vehicles.Where(v=>v.model.GetComponent<MeshyCarVariants>()?.variants.Any(x=>x.armored)==true).ToList();
  var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  try
  {
   Time.timeScale=0;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
   typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,1);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[1],0,0);
   var tank=Spawn(g,slots[0],true,3,16,4.4f);tick(1/60f);
   c.transform.position=g.transform.TransformPoint(new Vector3(-.5f,3.2f,4));c.transform.LookAt(tank.model.position+Vector3.up*1.1f);c.fieldOfView=48;Capture(c,"meshy-tank-street");
   c.transform.position=g.transform.TransformPoint(new Vector3(PhoVietGame.LaneX[3]-5.5f,2.2f,tank.model.localPosition.z+6));c.transform.LookAt(tank.model.position+Vector3.up*1.1f);c.fieldOfView=50;Capture(c,"meshy-tank-side");
   c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;Capture(c,"meshy-tank-game");
   var car=Spawn(g,slots[1],false,3,tank.model.localPosition.z-9,16);bool hit=false;float after=0;
   for(int i=0;i<240;i++){tick(1/60f);if(car.knocked)hit=true;if(hit){after+=1/60f;if(after>=.2f)break;}}
   c.transform.position=g.transform.TransformPoint(new Vector3(-1.5f,4.5f,tank.model.localPosition.z-14));c.transform.LookAt(tank.model.position+Vector3.up*.8f);c.fieldOfView=50;Capture(c,"meshy-tank-impact");
   return new{success=true,hit,tankSpeed=tank.speed,carKnocked=car.knocked};
  }
  finally{c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;Time.timeScale=scale;g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;g.ResetRide();}
 }
 static PhoVietGame.TrafficVehicle Spawn(PhoVietGame g,PhoVietGame.TrafficVehicle slot,bool tank,int lane,float z,float speed)
 {
  var v=g.SpawnTraffic(lane,z,g.vehicles.IndexOf(slot));var m=v.model.GetComponent<MeshyCarVariants>();m.Select(tank?Array.FindIndex(m.variants,x=>x.armored):0,1,v);
  v.speed=v.baseTrafficSpeed=speed*v.speedScale;v.model.localRotation=Quaternion.identity;v.previousPosition=v.model.localPosition;return v;
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
