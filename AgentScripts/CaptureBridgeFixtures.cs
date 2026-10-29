using System;using System.IO;using System.Linq;using UnityEngine;using PhoViet;

// 21.87: play-only visual proof. No scene save or installer; existing start stays authored.
public static class CaptureBridgeFixtures
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;
  float scale=Time.timeScale,oldFov=c.fieldOfView;var oldPos=c.transform.position;var oldRot=c.transform.rotation;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled;
  LODGroup[] lods=new LODGroup[0];
  var images=new System.Collections.Generic.List<string>();
  try
  {
   Time.timeScale=0;g.riversEnabled=true;g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   foreach(var row in g.roadRows)foreach(var t in row.tiles)t.GetComponent<RiverBlock>().SetBridge(false);
   var tile=g.blocks.OrderBy(t=>Mathf.Abs(t.localPosition.z-80)).First();var bridge=tile.GetComponent<RiverBlock>();bridge.SetBridge(true);
   lods=bridge.bridge.GetComponentsInChildren<LODGroup>(true).Where(l=>l.GetLODs().Any(d=>d.renderers.Any(r=>r!=null&&r.name=="PV_RiverBridge_LOD0"))).ToArray();
   if(lods.Length!=1)throw new Exception("Expected one bridge mesh LOD group, found "+lods.Length);
   foreach(var l in lods)l.ForceLOD(0);g.daylight.SetCycleTime(9);
   Action<Vector3,Vector3,float,string> shot=(pos,target,fov,name)=>{
    c.transform.position=tile.TransformPoint(pos);c.transform.LookAt(tile.TransformPoint(target));c.fieldOfView=fov;
    Save(c,"Captures/"+name+".png");images.Add(name+".png");
   };
   shot(new Vector3(-7,8,-1),new Vector3(0,1,29),61,"bridge-fixtures-overview");
   for(int i=0;i<3;i++)
   {
    foreach(var l in lods)l.ForceLOD(i);
    shot(new Vector3(-5.6f,4.75f,6.3f),new Vector3(-4.15f,4.6f,10),36,"bridge-fixtures-flag-left-lod"+i);
   }
   foreach(var l in lods)l.ForceLOD(0);
   shot(new Vector3(5.6f,4.75f,13.7f),new Vector3(4.15f,4.6f,10),36,"bridge-fixtures-flag-right");
   g.daylight.SetCycleTime(g.daylight.CycleSeconds*.5f);
   shot(new Vector3(-7,8,-1),new Vector3(0,1,29),61,"bridge-fixtures-night");
   var result=new{success=true,images=images.ToArray(),lodGroups=lods.Length,jobs=g.jobsEnabled,source="Actual Unity Game camera; isolated Play Mode capture"};
   File.WriteAllText("Captures/bridge-fixtures-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally
  {
   foreach(var l in lods)if(l!=null)l.ForceLOD(-1);
   g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.ResetRide();Time.timeScale=scale;
   c.transform.position=oldPos;c.transform.rotation=oldRot;c.fieldOfView=oldFov;g.daylight.SetCycleTime(0);
  }
 }
 static void Save(Camera c,string path)
 {
  var previous=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;
  var rt=RenderTexture.GetTemporary(1600,1000,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.aspect=1.6f;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(1600,1000,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1600,1000),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=previous;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
