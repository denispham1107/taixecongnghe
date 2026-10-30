using System;using System.IO;using System.Linq;using UnityEngine;using PhoViet;
public static class CaptureRentalWalls
{
 public static object Before()=>Run("before");
 public static object After()=>Run("after");
 static object Run(string phase)
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;
  float time=Time.timeScale,fov=c.fieldOfView;var pos=c.transform.position;var rot=c.transform.rotation;GameObject preview=null;int images=0;
  void Shot(string name,int w=1280,int h=900){Save(c,"Captures/rental-walls-"+phase+"-"+name+".png",w,h);images++;}
  try
  {
   Time.timeScale=0;g.daylight.SetCycleTime(9);Shot("street");
   var source=g.scenery.GetComponentsInChildren<MeshFilter>(true).First(f=>f.sharedMesh!=null&&f.sharedMesh.name=="PV_StreetShop02_LOD0");
   var lod=source.GetComponentInParent<LODGroup>();preview=UnityEngine.Object.Instantiate(lod.gameObject);preview.transform.position+=Vector3.up*100;
   preview.GetComponent<LODGroup>().ForceLOD(0);
   var m=preview.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.sharedMesh!=null&&f.sharedMesh.name=="PV_StreetShop02_LOD0");
   var target=m.transform.TransformPoint(new Vector3(4.18f,0,6));var outward=m.transform.TransformDirection(Vector3.right).normalized;
   c.transform.position=target+outward*2.2f+Vector3.up*.1f;c.transform.LookAt(target);c.fieldOfView=55;Shot("plaster-close");
   UnityEngine.Object.DestroyImmediate(preview);preview=null;
   var tile=g.blocks[2];tile.GetComponent<RiverBlock>().SetBridge(true);
   c.transform.position=tile.TransformPoint(new Vector3(0,19,8));c.transform.LookAt(tile.TransformPoint(new Vector3(0,13,55)));c.fieldOfView=72;Shot("river-wide",1640,620);
   c.transform.position=tile.TransformPoint(new Vector3(-29,17,27));c.transform.LookAt(tile.TransformPoint(new Vector3(-38,17,55.4f)));c.fieldOfView=62;Shot("rental-close");
   target=tile.TransformPoint(new Vector3(-11.4f,9,57.3f));c.transform.position=target+tile.right*2.5f;c.transform.LookAt(target);c.fieldOfView=55;Shot("river-plaster-close");
   c.transform.position=tile.TransformPoint(new Vector3(0,19,8));c.transform.LookAt(tile.TransformPoint(new Vector3(0,13,55)));c.fieldOfView=72;g.daylight.SetCycleTime(g.daylight.CycleSeconds*.5f);Shot("river-night",1640,620);
   var result=new{success=true,phase,images};File.WriteAllText("Captures/rental-walls-"+phase+"-visuals.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally
  {if(preview!=null)UnityEngine.Object.DestroyImmediate(preview);g.ResetRide();c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;g.daylight.SetCycleTime(0);Time.timeScale=time;}
 }
 static void Save(Camera c,string path,int w,int h)
 {
  var old=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default,4);Texture2D t=null;
  try{c.aspect=(float)w/h;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t=new Texture2D(w,h,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}
  finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(t!=null)UnityEngine.Object.DestroyImmediate(t);}
 }
}
