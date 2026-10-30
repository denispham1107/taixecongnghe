using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using PhoViet;
public static class CaptureBoatSignal
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;
  float ts=Time.timeScale,fov=c.fieldOfView;var pos=c.transform.position;var rot=c.transform.rotation;GameObject preview=null;int images=0;LODGroup lod=null;
  void Shot(string s){Save(c,"Captures/boat-signal-"+s+".png");images++;}
  try
  {
   Time.timeScale=0;g.daylight.SetCycleTime(9);
   var tile=g.blocks[2];tile.GetComponent<RiverBlock>().SetBridge(true);var boat=tile.GetComponentsInChildren<RiverBoat>(true).First();lod=boat.GetComponent<LODGroup>();
   var target=boat.transform.TransformPoint(new Vector3(0,1.6f,0));
   for(int i=0;i<3;i++){lod.ForceLOD(i);c.transform.position=target+tile.TransformDirection(new Vector3(8,4,-8));c.transform.LookAt(target);c.fieldOfView=43;Shot("boat-lod"+i);}
   lod.ForceLOD(-1);lod=null;g.daylight.SetCycleTime(30);Shot("boat-night");g.daylight.SetCycleTime(9);
   var pole=g.trafficLights[0].transform.parent.parent;preview=UnityEngine.Object.Instantiate(pole.gameObject);preview.name="Temporary signal102 preview";preview.transform.position+=Vector3.up*100;
   var lenses=preview.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("Tín hiệu ")).OrderBy(r=>r.name).ToArray();
   var apply=typeof(PhoVietGame).GetMethod("ApplyTrafficSignalState",BindingFlags.Instance|BindingFlags.NonPublic);
   var pt=preview.transform;var aim=pt.TransformPoint(new Vector3(1.3f,5.2f,0));
   foreach(int phase in new[]{0,1,2})
   {
    apply.Invoke(g,new object[]{phase});for(int i=0;i<3;i++)lenses[i].sharedMaterial=g.trafficLights[i].sharedMaterial;
    foreach(int side in new[]{-1,1}){c.transform.position=aim+pt.TransformDirection(new Vector3(side*.8f,.2f,side*9));c.transform.LookAt(aim);c.fieldOfView=50;Shot("signal-"+phase+"-"+(side<0?"front":"back"));}
   }
   var report=new{success=true,images,source="Actual Unity boat/LOD/water; temporary Play copy of real two-face live signal"};File.WriteAllText("Captures/boat-signal-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{if(lod!=null)lod.ForceLOD(-1);if(preview!=null)UnityEngine.Object.DestroyImmediate(preview);g.ResetRide();c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;g.daylight.SetCycleTime(0);Time.timeScale=ts;}
 }
 static void Save(Camera c,string path)
 {
  var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var rt=RenderTexture.GetTemporary(1100,900,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.aspect=1100f/900;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(1100,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1100,900),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
