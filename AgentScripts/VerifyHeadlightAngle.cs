using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyHeadlightAngle
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var light=g.daylight;float scale=Time.timeScale;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,storm=g.thunderstormEnabled;
  var checks=new List<string>();var models=new HashSet<string>();int poses=0,bikes=0;var block=new MaterialPropertyBlock();Action<bool,string> check=(b,n)=>{if(!b)throw new Exception(n);checks.Add(n);};
  Action<DayNightLighting.VehicleLamps,string> verify=(lamp,label)=>
  {
   lamp.roadLight.GetPropertyBlock(block);
   check(Mathf.Abs(block.GetFloat("_HeadlightWidth")-1.8f)<.0001f&&!lamp.roadLight.sharedMaterial.HasProperty("_HeadlightHeight"),"Original optical model with widened horizontal angle: "+label);
   var matrix=block.GetMatrix("_WorldToHead");var position=block.GetVector("_HeadPosition");
   check(Vector3.Distance(position,lamp.head.position)<.0001f&&matrix.MultiplyPoint3x4(lamp.head.position).magnitude<.0001f,"One centred projector follows physical headlamp frame: "+label);
   var left=matrix.MultiplyPoint3x4(lamp.head.position+lamp.head.rotation*new Vector3(-4,0,8));var right=matrix.MultiplyPoint3x4(lamp.head.position+lamp.head.rotation*new Vector3(4,0,8));
   check(Mathf.Abs(left.x+right.x)<.0001f&&Mathf.Abs(left.z-right.z)<.0001f&&Mathf.Abs(left.z-8)<.0001f,"Both edges stay symmetrical around forward axis: "+label);
  };
  try
  {
   Time.timeScale=0;g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=g.thunderstormEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   var car=g.rider.GetComponent<PlayerCarVisual>();var player=light.vehicles.Single(v=>v.root==g.rider);var roadMesh=player.roadLight.GetComponent<MeshFilter>().sharedMesh;var roadMaterial=player.roadLight.sharedMaterial;
   foreach(float height in new[]{0f,1f,4.1166525f,3.2f})foreach(float yaw in new[]{0f,-15f,15f,-90f,90f})foreach(float pitch in new[]{-4f,4f})
   {
    g.rider.localPosition=new Vector3(3.1f,height,0);g.rider.localRotation=Quaternion.Euler(0,yaw,2);car.visual.localRotation=Quaternion.Euler(pitch,0,0);light.SetCycleTime(30);string label="player h="+height+" yaw="+yaw+" pitch="+pitch;verify(player,label);poses++;
    check(Mathf.Abs(light.lightPool[0].spotAngle-(2*Mathf.Atan(Mathf.Tan(29*Mathf.Deg2Rad)*1.8f)*Mathf.Rad2Deg))<.001f&&Vector3.Distance(light.lightPool[0].transform.position,player.head.position)<.0001f,"Original spotlight opens wider at the same source: "+label);
    check(Mathf.Abs(player.roadLight.transform.position.y-g.transform.position.y-.035f)<.0001f,"Receiver remains on road while both sources jump: "+label);
   }
   g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],0,0);g.rider.localRotation=Quaternion.identity;car.Advance(0);
   for(int n=0;n<g.vehicles.Count;n++)
   {
    var v=g.vehicles[n];var variants=v.model.GetComponent<MeshyCarVariants>();if(variants==null)continue;
    for(int m=0;m<variants.variants.Length;m++)
    {
     // Event-only/rear-reserved actors deliberately cannot use SpawnTraffic(preferred).
     // Exercise their actual model-binding method directly, including every truck/tank shape.
     v.active=false;v.model.gameObject.SetActive(false);variants.Select(m,0,v);v.active=true;v.model.gameObject.SetActive(true);v.model.localPosition=new Vector3(PhoVietGame.LaneX[0],0,16);
     check(variants.ModelAsset==variants.variants[m].asset,"Select pooled car "+n+" variant "+m);v.model.localRotation=Quaternion.Euler(0,m%2==0?180:90,0);light.SetCycleTime(30);
     var lamp=light.vehicles.Single(l=>l.root==v.model);verify(lamp,"actor "+n+" "+variants.ModelAsset);models.Add(variants.ModelAsset);
     check(lamp.roadLight.GetComponent<MeshFilter>().sharedMesh==roadMesh&&lamp.roadLight.sharedMaterial==roadMaterial,"Shared receiver/material retained for "+n+" "+m);
     v.active=false;v.model.gameObject.SetActive(false);
    }
   }
   foreach(var v in g.vehicles.Where(v=>!v.eventOnly&&v.halfWidth<.7f&&v.model.GetComponent<MeshyCarVariants>()==null).Take(3))
   {
    var index=g.vehicles.IndexOf(v);g.SpawnTraffic(2,10,index);light.SetCycleTime(30);var lamp=light.vehicles.Single(l=>l.root==v.model);lamp.roadLight.GetPropertyBlock(block);
    check(block.GetFloat("_HeadlightWidth")==1,"Motorcycle retains single beam: "+index);bikes++;v.active=false;v.model.gameObject.SetActive(false);
   }
   light.SetCycleTime(30);check(light.lightPool.Length==6&&light.ActiveRealtimeLights<=6,"Six-light runtime budget retained");
   check(light.lightPool[0].range==32&&Mathf.Abs(light.lightPool[0].intensity-2.8f)<.0001f,"Original headlight range and intensity retained");
   check(light.lightPool[4].spotAngle==99&&light.lightPool[5].spotAngle==99,"Street-lamp fill angles unchanged");
   check(player.roadLight.GetComponent<MeshFilter>().sharedMesh==roadMesh&&player.roadLight.sharedMaterial==roadMaterial,"Player uses original receiver mesh/material without cloning");
   light.SetCycleTime(0);check(light.ActiveRealtimeLights==0&&!player.roadLight.enabled,"All headlamps remain off in clear daylight");
   check(bikes==3&&models.Count>=4,"Motorcycles and every received car/truck shape covered");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Passenger jobs remain locked");
   var result=new{success=true,count=checks.Count,checks,poses,bikes,models=models.OrderBy(x=>x).ToArray()};File.WriteAllText("Captures/headlight-angle-binding-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count,poses,bikes,models};
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.thunderstormEnabled=storm;g.ResetRide();Time.timeScale=scale;}
 }
}
