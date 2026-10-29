using System;using System.IO;using System.Linq;using UnityEngine;using PhoViet;using Newtonsoft.Json.Linq;
public static class CaptureSignalRental
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;
  float time=Time.timeScale,fov=c.fieldOfView;var pos=c.transform.position;var rot=c.transform.rotation;GameObject preview=null;LODGroup lod=null;int images=0;
  void Shot(string name){Save(c,"Captures/signal-rental-"+name+".png");images++;}
  try{
   Time.timeScale=0;g.daylight.SetCycleTime(9);
   var source=g.scenery.GetComponentsInChildren<MeshFilter>(true).First(f=>f.sharedMesh!=null&&f.sharedMesh.name=="PV_TrafficSignal");
   preview=UnityEngine.Object.Instantiate(source.transform.parent.parent.gameObject);preview.name="Temporary fixture detail preview";preview.transform.position+=Vector3.up*100;
   var m=preview.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.sharedMesh!=null&&f.sharedMesh.name=="PV_TrafficSignal");lod=m.GetComponentInParent<LODGroup>();
   var target=m.transform.TransformPoint(new Vector3(3,0,6.82f));var front=m.transform.up;var right=m.transform.right;
   target=m.transform.TransformPoint(new Vector3(3,.022f,6.82f));
   for(int i=0;i<3;i++)foreach(int side in new[]{1,-1}){lod.ForceLOD(i);c.transform.position=target+front*side*3.4f+right*.85f+Vector3.up*.35f;c.transform.LookAt(target);c.fieldOfView=43;Shot("signal-"+(side==1?"front":"back")+"-lod"+i);}
   lod.ForceLOD(0);target=m.transform.TransformPoint(new Vector3(1.5f,0,4));c.transform.position=target+front*13+right*3+Vector3.up*2;c.transform.LookAt(target);c.fieldOfView=47;Shot("signal-pole");
   UnityEngine.Object.DestroyImmediate(preview);preview=null;lod=null;
   var tile=g.blocks[2];tile.GetComponent<RiverBlock>().SetBridge(true);
   c.transform.position=tile.TransformPoint(new Vector3(-6,13,16));c.transform.LookAt(tile.TransformPoint(new Vector3(-29,14,57)));c.fieldOfView=62;Shot("river-day");
   c.transform.position=tile.TransformPoint(new Vector3(-30,15,29));c.transform.LookAt(tile.TransformPoint(new Vector3(-33,15,57)));c.fieldOfView=55;Shot("rental-close");
   g.daylight.SetCycleTime(g.daylight.CycleSeconds*.5f);Shot("rental-night");
   File.WriteAllText("Captures/signal-rental-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=true,images,source="Unity actual meshes/LOD; signal uses an isolated temporary Play copy, river uses runtime bridge tile"},Newtonsoft.Json.Formatting.Indented));return new{success=true,images};
  }finally{if(lod!=null)lod.ForceLOD(-1);if(preview!=null)UnityEngine.Object.DestroyImmediate(preview);g.ResetRide();c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;g.daylight.SetCycleTime(0);Time.timeScale=time;}
 }
 static void Save(Camera c,string path)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,900,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.aspect=1280f/900;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(1280,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,900),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
