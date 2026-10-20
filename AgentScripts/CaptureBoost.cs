using System;
using System.IO;
using UnityEngine;
using PhoViet;
public static class CaptureBoost
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var c=g.rideCamera;float time=Time.timeScale;bool jobs=g.jobsEnabled;
  var target=c.targetTexture;var pos=c.transform.localPosition;var rot=c.transform.localRotation;float fov=c.fieldOfView;
  var rt=new RenderTexture(1200,900,24);var pixels=new Texture2D(1200,900,TextureFormat.RGB24,false);var old=RenderTexture.active;
  Action<string> shot=name=>{c.targetTexture=rt;c.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,1200,900),0,0);pixels.Apply();File.WriteAllBytes("Captures/boost-"+name+".png",pixels.EncodeToPNG());};
  try
  {
   Time.timeScale=0;g.jobsEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   g.TryBoost();var bike=g.rider.GetComponent<BikeVisualAnimator>();bike.Advance(.1f);var fx=g.rider.GetComponent<BoostSmokeVisual>();
   for(int i=0;i<16;i++)fx.Simulate(.025f);
   float x=g.rider.localPosition.x;c.transform.localPosition=new Vector3(x+4.8f,2.7f,-6.2f);c.transform.LookAt(g.transform.TransformPoint(new Vector3(x,1,-1.2f)));c.fieldOfView=44;shot("exhaust-close");
   c.transform.localPosition=pos;c.transform.localRotation=rot;c.fieldOfView=fov;shot("exhaust-game");
   g.TryJump();var method=typeof(PhoVietGame).GetMethod("TickRide",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
   for(int i=0;i<29;i++)method.Invoke(g,new object[]{.02f});bike.Advance(.1f);fx.Clear();for(int i=0;i<12;i++)fx.Simulate(.025f);
   c.transform.localPosition=new Vector3(x+5.8f,4.3f,-7.2f);c.transform.LookAt(g.transform.TransformPoint(new Vector3(x,1.9f,-1)));c.fieldOfView=48;shot("jump");
   return new{success=true,particles=fx.ParticleCount,files=new[]{"boost-exhaust-close.png","boost-exhaust-game.png","boost-jump.png"}};
  }
  finally
  {
   g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=time;c.targetTexture=target;c.transform.localPosition=pos;c.transform.localRotation=rot;c.fieldOfView=fov;RenderTexture.active=old;
   UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(pixels);
  }
 }
}
