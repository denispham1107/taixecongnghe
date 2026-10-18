using System;
using System.Linq;
using UnityEngine;
using PhoViet;
public static class CaptureFabricLeaves
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();g.ResetRide();var c=g.rideCamera;
  var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  var lods=g.scenery.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.StartsWith("PV_Street",StringComparison.Ordinal)).ToArray();
  try
  {
   Capture(c,"fabric-leaves-game");
   c.transform.position=new Vector3(2,5,-3);c.transform.LookAt(new Vector3(13.2f,4.3f,4));c.fieldOfView=55;
   foreach(int level in new[]{0,1,2}){foreach(var l in lods)l.ForceLOD(level);Capture(c,"fabric-leaves-awning-lod"+level);}
   foreach(var l in lods)l.ForceLOD(0);
   c.transform.position=new Vector3(-2.7f,5.3f,5);c.transform.LookAt(new Vector3(-8.2f,4.4f,10));c.fieldOfView=46;Capture(c,"fabric-leaves-tree");
   return new{success=true};
  }
  finally{foreach(var l in lods)l.ForceLOD(-1);c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;}
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1440,1000,24);var t=new Texture2D(1440,1000,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1440,1000),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
