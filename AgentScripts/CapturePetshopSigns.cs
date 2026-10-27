using System;using System.Linq;using System.IO;using System.Reflection;using UnityEngine;using PhoViet;
public static class CapturePetshopSigns
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");var cam=g.rideCamera;var old=cam.targetTexture;var active=RenderTexture.active;var position=cam.transform.position;var rotation=cam.transform.rotation;float fov=cam.fieldOfView,time=Time.timeScale;
  var rt=new RenderTexture(1100,720,24){antiAliasing=4};var texture=new Texture2D(1100,720,TextureFormat.RGB24,false);var files=new System.Collections.Generic.List<string>();
  Action<string> shot=label=>{g.daylight.UpdateLamps();cam.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1100,720),0,0);texture.Apply();string file="Captures/petshop-sign-"+label+".png";File.WriteAllBytes(file,texture.EncodeToPNG());files.Add(file);};
  try
  {
   Time.timeScale=0;g.ResetRide();cam.targetTexture=rt;foreach(var row in g.roadRows.Skip(1))row.root.gameObject.SetActive(false);
   var tile=g.blocks.OrderBy(b=>Mathf.Abs(b.localPosition.z)).First();foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(false);
   var sign=tile.GetComponentsInChildren<MeshFilter>(true).First(f=>f.sharedMesh!=null&&f.sharedMesh.name.Contains("_Petshop_LOD0"));
   var target=sign.transform.TransformPoint(new Vector3(0,4.118f,4.1f));var outward=sign.transform.TransformDirection(Vector3.up);
   cam.fieldOfView=40;cam.transform.position=target+outward*13+Vector3.up*2;cam.transform.LookAt(target+Vector3.up*1.5f);g.daylight.SetCycleTime(8);shot("street-day");g.daylight.SetCycleTime(30);shot("street-night");
   tile.GetComponent<RiverBlock>().SetBridge(true);typeof(PhoVietGame).GetMethod("HideBuildingsOverRiver",F).Invoke(g,null);
   var info=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Captures/petshop-sign-checks.json"));var sample=info["riverSamples"][0];string part=(string)sample["part"];float x=((float)sample["x0"]+(float)sample["x1"])*.5f,z=(float)sample["zf"],facing=(float)sample["f"];
   var apt=tile.GetComponent<RiverBlock>().bridge.transform.Find("Căn hộ ven sông • Blender").Find(part);var lod=apt.GetComponent<LODGroup>();var mesh=lod.GetLODs()[0].renderers[0].GetComponent<MeshFilter>();
   target=mesh.transform.TransformPoint(new Vector3(x,-(z+facing*.21f),3.35f));outward=mesh.transform.TransformDirection(new Vector3(0,-facing,0));
   cam.transform.position=target+outward*13+Vector3.up*1.5f;cam.transform.LookAt(target+Vector3.up*1.3f);g.daylight.SetCycleTime(8);lod.ForceLOD(0);shot("river-day");lod.ForceLOD(1);shot("river-lod1");lod.ForceLOD(2);shot("river-lod2");lod.ForceLOD(-1);g.daylight.SetCycleTime(30);shot("river-night");
   target=mesh.transform.TransformPoint(new Vector3(x,-(z-facing*3-facing*.14f),3.35f));outward=mesh.transform.TransformDirection(new Vector3(0,facing,0));
   cam.transform.position=target+outward*13+Vector3.up*1.5f;cam.transform.LookAt(target+Vector3.up*1.3f);g.daylight.SetCycleTime(8);shot("river-waterfront-day");g.daylight.SetCycleTime(30);shot("river-waterfront-night");
   cam.transform.position=position;cam.transform.rotation=rotation;cam.fieldOfView=fov;g.daylight.SetCycleTime(8);shot("game-wide");
   var report=new{success=true,files,riverPart=part};File.WriteAllText("Captures/petshop-sign-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{cam.targetTexture=old;RenderTexture.active=active;cam.transform.position=position;cam.transform.rotation=rotation;cam.fieldOfView=fov;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);foreach(var l in g.scenery.GetComponentsInChildren<LODGroup>(true))l.ForceLOD(-1);g.ResetRide();Time.timeScale=time;}
 }
}
