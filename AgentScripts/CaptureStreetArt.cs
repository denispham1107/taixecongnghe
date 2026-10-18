using UnityEngine;
using PhoViet;
public static class CaptureStreetArt
{
 public static object Main()
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();g.ResetRide();var c=g.rideCamera;
  var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  Capture(c,"street-upgrade-game");
  c.transform.position=new Vector3(2,5,-3);c.transform.LookAt(new Vector3(13.2f,4.3f,4));c.fieldOfView=55;Capture(c,"street-upgrade-facade");
  c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;
  return new{success=true};
 }
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1440,1000,24);var t=new Texture2D(1440,1000,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1440,1000),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);
 }
}
