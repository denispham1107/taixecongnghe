using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class CaptureJobWayfinding
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;bool jobs=g.jobsEnabled;
  try
  {
   Time.timeScale=0;g.jobsEnabled=true;g.RestartRide();g.ShowJobOffers();g.AcceptJob(0);
   g.ActiveJob.pickup=new RideAddress{node=Vector2Int.zero,axis=0,offset=47.6f,curb=-1};
   g.ActiveJob.dropoff=new RideAddress{node=Vector2Int.zero,axis=0,offset=47.6f,curb=1};
   foreach(bool delivery in new[]{false,true})
   {
    typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,delivery?PhoVietGame.WorkPhase.Delivery:PhoVietGame.WorkPhase.Pickup);
    typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});
    typeof(PhoVietGame).GetMethod("UpdateJobMarker",F).Invoke(g,null);
    var c=g.rideCamera;var rt=new RenderTexture(1440,900,24);var texture=new Texture2D(1440,900,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
    try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1440,900),0,0);texture.Apply();File.WriteAllBytes("Captures/wayfinding-native-"+(delivery?"dropoff":"pickup")+".png",texture.EncodeToPNG());}
    finally{c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}
   }
   return new{success=true,frames=2};
  }
  finally{g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=scale;}
 }
}
