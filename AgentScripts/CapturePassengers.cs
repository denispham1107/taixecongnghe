using UnityEngine;
using PhoViet;
using System.Reflection;
public static class CapturePassengers
{
 public static object MaleSit(){return Prepare(false,true);}
 public static object FemaleSit(){return Prepare(true,true);}
 public static object FemaleWalk(){return Prepare(true,false);}
 static object Prepare(bool female,bool sit)
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();g.ResetRide();var p=female?g.femalePassenger:g.malePassenger;
  p.SetParent(g.rider,false);p.localPosition=new Vector3(0,.16f,-.9f);p.localRotation=Quaternion.identity;p.gameObject.SetActive(true);
  p.GetComponent<PassengerVisualAnimator>().Pose(sit?"Sit":"Walk",sit?0:.3f);return true;
 }
 public static object Detail()
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;var pos=c.transform.position;var rot=c.transform.rotation;float fov=c.fieldOfView;
  c.transform.position=g.rider.position+new Vector3(3,2.1f,-3.4f);c.transform.LookAt(g.rider.position+new Vector3(0,1.05f,0));c.fieldOfView=43;
  Capture(c,g.femalePassenger.gameObject.activeSelf?"passenger-female-seated":"passenger-male-seated");c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;return true;
 }
 public static object Main(){return Detail();}
 static void Capture(Camera c,string name)
 {
  var rt=new RenderTexture(1200,900,24);var t=new Texture2D(1200,900,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1200,900),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);
 }
}
