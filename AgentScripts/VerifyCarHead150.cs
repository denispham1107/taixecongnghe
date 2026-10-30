using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyCarHead150
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var light=g.daylight;float scale=Time.timeScale;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled;var checks=new List<string>();var cases=new List<object>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   var car=g.rider.GetComponent<PlayerCarVisual>();car.Advance(0);var lamp=light.vehicles.Single(v=>v.root==g.rider);var front=lamp.halos.Where(h=>!h.rear).ToArray();
   check(front.Length==2&&!front.Any(h=>h.source==lamp.head),"Projector is independent of both headlight halos");
   check(lamp.head.IsChildOf(car.visual),"Projector stays attached to the animated car");
   foreach(float y in new[]{0f,1f,4.1166525f,3.2f})foreach(float yaw in new[]{0f,-15f,15f,-90f,90f})foreach(float pitch in new[]{-4f,4f})
   {
    g.rider.localPosition=new Vector3(3.1f,y,0);g.rider.localRotation=Quaternion.Euler(0,yaw,2);car.visual.localRotation=Quaternion.Euler(pitch,0,0);light.SetCycleTime(30);
    string label="height="+y+",yaw="+yaw+",pitch="+pitch;var centre=(front[0].source.position+front[1].source.position)*.5f;
    check(Vector3.Distance(lamp.head.position,centre)<.0001f,"Light originates at the two lamps' midpoint: "+label);
    check(Vector3.Distance(light.lightPool[0].transform.position,centre)<.0001f,"Realtime spotlight follows the actual source: "+label);
    var block=new MaterialPropertyBlock();lamp.roadLight.GetPropertyBlock(block);var expected=Matrix4x4.TRS(centre,lamp.head.rotation,Vector3.one).inverse;var actual=block.GetMatrix("_WorldToHead");float error=0;for(int i=0;i<16;i++)error=Mathf.Max(error,Mathf.Abs(expected[i]-actual[i]));
    check(error<.0001f,"Ground cookie uses the same centered, animated source: "+label);
    check(Vector3.Dot(lamp.head.forward,front[0].source.forward)>.9999f,"Projector follows the car's yaw/pitch: "+label);
    cases.Add(new{y,yaw,pitch,centre=new[]{centre.x,centre.y,centre.z},matrixError=error});
   }
   g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],0,0);g.rider.localRotation=Quaternion.identity;car.Advance(0);
   var index=g.vehicles.FindIndex(v=>!v.eventOnly&&v.model.GetComponent<MeshyCarVariants>()!=null);var vehicle=g.vehicles[index];var mesh=vehicle.model.GetComponent<MeshyCarVariants>();
   for(int i=0;i<mesh.variants.Length;i++)
   {
    vehicle.active=false;vehicle.model.gameObject.SetActive(false);mesh.PrepareVariant(i,vehicle);var spawned=g.SpawnTraffic(0,60,index);check(spawned!=null,"NPC variant fixture "+i);var lamps=light.vehicles.Single(v=>v.root==vehicle.model);var pair=lamps.halos.Where(h=>!h.rear).ToArray();
    check(pair.Length==2&&Vector3.Distance(lamps.head.position,(pair[0].source.position+pair[1].source.position)*.5f)<.001f,"NPC lamp stays centered for "+mesh.ModelAsset);
   }
   check(light.lightPool.Length==6,"Realtime light budget remains six");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   var result=new{success=true,count=checks.Count,checks,cases};File.WriteAllText("Captures/release150-head-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.ResetRide();Time.timeScale=scale;}
 }
}
