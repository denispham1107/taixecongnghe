using System;using System.IO;using System.Linq;using UnityEngine;using PhoViet;using Newtonsoft.Json.Linq;
public static class CaptureRental2194
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;
  float time=Time.timeScale,fov=c.fieldOfView;var pos=c.transform.position;var rot=c.transform.rotation;int images=0;
  void Shot(string name){Save(c,"Captures/tuning2194-"+name+".png");images++;}
  try{
   Time.timeScale=0;g.daylight.SetCycleTime(9);
   var tile=g.blocks[2];tile.GetComponent<RiverBlock>().SetBridge(true);
   c.transform.position=tile.TransformPoint(new Vector3(-6,13,16));c.transform.LookAt(tile.TransformPoint(new Vector3(-29,14,57)));c.fieldOfView=62;Shot("river-day");
   c.transform.position=tile.TransformPoint(new Vector3(-30,15,29));c.transform.LookAt(tile.TransformPoint(new Vector3(-33,15,57)));c.fieldOfView=55;Shot("rental-close");
   g.daylight.SetCycleTime(g.daylight.CycleSeconds*.5f);Shot("rental-night");
   File.WriteAllText("Captures/tuning2194-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=true,images,source="Unity actual apartment meshes/LOD on a temporary runtime bridge tile"},Newtonsoft.Json.Formatting.Indented));return new{success=true,images};
  }finally{g.ResetRide();c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;g.daylight.SetCycleTime(0);Time.timeScale=time;}
 }
 static void Save(Camera c,string path)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,900,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.aspect=1280f/900;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(1280,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,900),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
