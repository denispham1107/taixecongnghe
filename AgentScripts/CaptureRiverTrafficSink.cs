using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class CaptureRiverTrafficSink
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;bool dinos=g.dinosaursEnabled,rivers=g.riversEnabled,tanks=g.tankBarrageEnabled;
  var camera=new GameObject("River traffic evidence").AddComponent<Camera>();var samples=new List<object>();
  var flags=BindingFlags.Instance|BindingFlags.NonPublic;Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,flags).Invoke(g,a);
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=false;g.riversEnabled=true;g.RestartRide();g.tankBarrageEnabled=false;g.daylight.SetCycleTime(8);
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}foreach(var row in g.roadRows)foreach(var t in row.tiles)t.GetComponent<RiverBlock>().SetBridge(g.blocks.Contains(t)&&Mathf.Abs(t.localPosition.z)<1);
   int order=0;var actors=new List<PhoVietGame.TrafficVehicle>();
   foreach(string asset in new[]{"PV_MeshySport","bike","PV_MeshyContainer"})
   {
    var v=asset=="bike"?g.SpawnTraffic(2,30,g.vehicles.FindIndex(x=>x.halfWidth<.7f&&!x.eventOnly)):g.SpawnTraffic(2,30,g.vehicles.FindIndex(x=>!x.active&&!x.eventOnly&&x.model.GetComponent<MeshyCarVariants>()?.variants.Any(m=>m.asset==asset)==true));
    var model=v.model.GetComponent<MeshyCarVariants>();if(model!=null)model.PrepareVariant(Array.FindIndex(model.variants,m=>m.asset==asset),v);
    v.model.localPosition=v.previousPosition=new Vector3(6+(order==1?2:0),1.5f,17+order*12);v.model.localRotation=Quaternion.identity;
    call("ActivateCollisionBody",new object[]{v,Vector3.zero,new Vector3(1,2,0),Vector2.right*PhoVietGame.VehicleMass(v)*6,Vector2.zero,false,true,8f});v.knockVelocity=new Vector3(1,2,0);actors.Add(v);order++;
   }
   typeof(PhoVietGame).GetProperty("State").SetValue(g,PhoVietGame.RideState.GameOver);
   camera.fieldOfView=55;camera.nearClipPlane=.1f;camera.farClipPlane=240;camera.clearFlags=CameraClearFlags.Skybox;
   camera.transform.position=g.transform.TransformPoint(new Vector3(0,12,7));camera.transform.LookAt(g.transform.TransformPoint(new Vector3(8,-4,32)));
   int step=0;foreach(float at in new[]{.6f,3.5f,6.5f})
   {
    while(step<Mathf.RoundToInt(at*60)){g.TickRiverAftermath(1f/60);step++;}
    foreach(var v in actors){v.model.GetComponent<BikeVisualAnimator>()?.Advance(0);v.model.GetComponent<CarVisualAnimator>()?.Advance(0);}
    Render(camera,"Captures/river-npc-sink-"+at.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+".png",actors);
    samples.Add(new{time=at,actors=actors.Select(v=>new{name=v.model.name,v.active,v.riverFalling,v.riverSplashed,x=v.model.localPosition.x,y=v.model.localPosition.y,z=v.model.localPosition.z}).ToArray()});
   }
   var report=new{success=true,samples};File.WriteAllText("Captures/river-npc-sink-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{UnityEngine.Object.DestroyImmediate(camera.gameObject);g.dinosaursEnabled=dinos;g.riversEnabled=rivers;g.tankBarrageEnabled=tanks;g.ResetRide();Time.timeScale=scale;}
 }
 static void Render(Camera camera,string file,List<PhoVietGame.TrafficVehicle> actors)
 {
  var rt=new RenderTexture(1100,760,24){antiAliasing=4};var image=new Texture2D(1100,760,TextureFormat.RGB24,false);var old=RenderTexture.active;
  var copies=new List<GameObject>();var meshes=new List<Mesh>();var skins=actors.SelectMany(v=>v.model.GetComponentsInChildren<SkinnedMeshRenderer>()).Where(s=>s.enabled&&s.gameObject.activeInHierarchy).ToArray();
  try
  {
   foreach(var skin in skins){var mesh=new Mesh();skin.BakeMesh(mesh);meshes.Add(mesh);var go=new GameObject("Baked sinking evidence");go.transform.SetParent(skin.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;copies.Add(go);skin.enabled=false;}
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1100,760),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());
  }
  finally{foreach(var o in copies)UnityEngine.Object.DestroyImmediate(o);foreach(var m in meshes)UnityEngine.Object.DestroyImmediate(m);foreach(var s in skins)s.enabled=true;camera.targetTexture=null;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
 }
}
