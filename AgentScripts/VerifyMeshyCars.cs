using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;
using UnityEngine;using UnityEditor;using PhoViet;
public static class VerifyMeshyCars
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  bool surprises=g.trafficSurprises,danger=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;var checks=new List<string>();
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action clean=()=>{g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,10000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);};
  var spawnStats=new List<object>();
  try
  {
   Time.timeScale=0;clean();var cars=g.vehicles.Where(v=>v.halfWidth>.7f).ToArray();
   check(cars.Length==11&&cars.All(v=>v.model.GetComponent<MeshyCarVariants>()!=null),"All eleven four-wheel pool actors use received Meshy variants");
   check(g.GetComponentsInChildren<BikeVisualAnimator>(true).Length==33,"All motorcycle actors preserved");
   check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   check(g.daylight.vehicles.Length==30&&g.daylight.vehicles.All(v=>v.head!=null&&v.halos.All(h=>h.source!=null)),"All thirty optical bindings survive installation");
   foreach(var v in cars)
   {
    var c=v.model.GetComponent<MeshyCarVariants>();var d=c.driver;v.active=true;v.model.gameObject.SetActive(true);v.model.localRotation=Quaternion.identity;v.model.localPosition=new Vector3(0,0,30);
    check(c.variants.Length==(v.eventOnly||d.AssetName=="PV_BoxTruck"?2:1),v.model.name+": correct civilian/dual truck variants");
    for(int i=0;i<c.variants.Length;i++)
    {
     c.Select(i,3,v);var e=c.variants[i];v.speed=v.baseTrafficSpeed=14;d.Advance(.1f);
     var skins=e.skins.Cast<SkinnedMeshRenderer>().ToArray();
     check(e.visual.gameObject.activeSelf&&c.variants.Count(t=>t.visual.gameObject.activeSelf)==1,e.asset+": exactly one visual active");
     check(Mathf.Abs(v.halfLength*2-e.size.z)<.001f&&Mathf.Abs(v.halfWidth*2-e.size.x)<.001f,e.asset+": collision bounds switch with actual shape");
     check(e.skins.Length==3&&e.visual.GetComponent<LODGroup>().GetLODs().Length==3,e.asset+": three LODs");
     int bones=e.asset.Contains("Container")?23:15;
     check(skins.All(s=>s.bones.Length==bones&&s.sharedMaterials.Length==1&&s.quality==SkinQuality.Bone1),e.asset+": one material and all axle bones");
     check(e.clips.Length==7&&e.clips.All(a=>a.legacy)&&e.clips.Select(a=>a.name).Distinct().Count()==7,e.asset+": seven semantic legacy clips");
     var counts=skins.Select(s=>(long)s.sharedMesh.GetIndexCount(0)/3).ToArray();check(counts[0]>counts[1]&&counts[1]>counts[2]&&counts[2]<4000,e.asset+": descending detail budgets");
     check(d.wheels.Length==(e.asset.Contains("Container")?8:4)&&d.wheels[0].position.z>v.model.position.z&&d.wheels[0].position.x<v.model.position.x,e.asset+": all axle pivots, forward and left axes");
     float a=d.WheelAngle;Vector3 body=d.body.localPosition;float bounce=0;for(int n=0;n<10;n++){d.Advance(.05f);bounce=Mathf.Max(bounce,Vector3.Distance(body,d.body.localPosition));}
     check(d.CurrentClip=="Drive"&&Mathf.Abs(a-d.WheelAngle)>1&&bounce>.005f,e.asset+": real wheel rotation and suspension");
     check(c.headAnchor.IsChildOf(d.bumperFront)&&c.tailHalos.All(t=>t.IsChildOf(d.bumperRear)),e.asset+": lamp anchors follow active bones");
     check(Vector3.Distance(v.model.InverseTransformPoint(c.headAnchor.position),e.headlight)<.08f,e.asset+": headlights positioned at active vehicle front");
     var block=new MaterialPropertyBlock();e.skins[0].GetPropertyBlock(block);check(block.GetColor("_PaintColor").g>.4f,e.asset+": instance paint reaches renderer");
     v.brakeRemaining=1;d.Advance(.18f);check(d.CurrentClip=="Brake",e.asset+": braking animation retained");v.brakeRemaining=0;
     v.knocked=true;foreach(var hit in new[]{(Vector3.back,"ImpactFront"),(Vector3.forward,"ImpactRear"),(Vector3.right,"ImpactLeft"),(Vector3.left,"ImpactRight")}){d.ReceiveImpact(hit.Item1,14);d.Advance(.12f);check(d.CurrentClip==hit.Item2,e.asset+": "+hit.Item2);}v.knocked=false;
     c.Select(i,3,v);check(Vector3.Distance(v.model.InverseTransformPoint(c.headAnchor.position),e.headlight)<.001f,e.asset+": recycling restores lamp mount after impact pose");
     check(PhoVietGame.JumpRoofHeight(v)>0&&c.RoofProfile.Length==49,e.asset+": actual roof profile available");
    }
    v.active=false;v.model.gameObject.SetActive(false);
    var colors=new HashSet<int>();var shapes=new HashSet<string>();bool eventOnly=v.eventOnly;int slot=g.vehicles.IndexOf(v);
    for(int n=0;n<40;n++)
    {
     v.eventOnly=false;var spawned=g.SpawnTraffic(n%4,100,slot);check(spawned==v,"Actual SpawnTraffic reuses selected slot "+slot+"/"+n);v.eventOnly=eventOnly;
     colors.Add(c.PaintIndex);shapes.Add(c.ModelAsset);check(v.halfLength==c.variants[c.SelectedVariant].size.z/2,"Spawn-selected shape and footprint agree "+slot+"/"+n);
     v.active=false;v.model.gameObject.SetActive(false);
    }
    check(colors.Count>=6,"Slot "+slot+": independently randomized paint across forty actual appearances");
    check(shapes.Count==c.variants.Length,"Slot "+slot+": every allowed model can appear");spawnStats.Add(new{slot,colors=colors.Count,models=shapes.ToArray()});
   }
   clean();g.daylight.SetCycleTime(30);check(g.daylight.LampLevel>.99f,"Night cycle still enables vehicle lamps");
   foreach(var material in g.daylight.emissionUsers.SelectMany(r=>r.sharedMaterials).Where(m=>m!=null&&m.shader.name=="PhoViet/Meshy Car Paint").Distinct())check(material.GetColor("_EmissionColor").maxColorComponent>1,"Runtime cloned Meshy material receives night emission");
   check(!ShaderUtil.ShaderHasError(Shader.Find("PhoViet/Meshy Car Paint")),"Paint shader compiles without errors");
   return new{success=true,count=checks.Count,checks,spawnStats};
  }
  finally{g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();Time.timeScale=time;}
 }
}
