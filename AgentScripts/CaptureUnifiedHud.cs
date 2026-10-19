using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureUnifiedHud
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static float scale;static bool jobs,running,waiting;static int phase;static double next;
 static readonly string[] names={"pickup","delivery","idle-long-values"};
 static object Call(string name,params object[] values)=>typeof(PhoVietGame).GetMethod(name,F).Invoke(g,values);
 public static object Main()
 {
  if(running)return new{started=true};if(!Application.isPlaying)throw new Exception("Play Mode required");
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;jobs=g.jobsEnabled;Time.timeScale=0;phase=0;running=true;waiting=false;
  Setup();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;return new{started=true,frames=3};
 }
 static void Setup()
 {
  g.jobsEnabled=phase<2;g.RestartRide();
  if(phase<2)
  {
   g.ShowJobOffers();g.AcceptJob(0);
   g.ActiveJob.pickup=new RideAddress{node=Vector2Int.zero,axis=0,offset=30,curb=1};
   g.ActiveJob.dropoff=new RideAddress{node=Vector2Int.zero,axis=0,offset=56,curb=-1};
   if(phase==1){typeof(PhoVietGame).GetProperty("JobPhase").SetValue(g,PhoVietGame.WorkPhase.Delivery);typeof(PhoVietGame).GetProperty("Coins").SetValue(g,3450);}
   Call("ReplanJob",false);
  }
  else
  {
   typeof(PhoVietGame).GetProperty("Coins").SetValue(g,int.MaxValue);
   typeof(PhoVietGame).GetProperty("RouteStreet").SetValue(g,"Nguyễn Thị Minh Khai");
  }
  Call("PositionCamera",true);Call("UpdateJobMarker");
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   string path="Captures/waypoint-3d-native-"+(Screen.width<Screen.height?"portrait-":"wide-")+names[phase]+".png";
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.8;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Screenshot not saved: "+path);
   if(phase==0)CaptureArrowDetail();
   phase++;waiting=false;
   if(phase==names.Length){Finish();File.WriteAllText("Captures/waypoint-3d-"+(Screen.width<Screen.height?"portrait":"wide")+"-visual-results.json","{\"success\":true,\"frames\":3}");return;}
   Setup();next=EditorApplication.timeSinceStartup+.8;
  }
  catch(Exception e){File.WriteAllText("Captures/waypoint-3d-"+(Screen.width<Screen.height?"portrait":"wide")+"-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=scale;}
 static void CaptureArrowDetail()
 {
  var c=g.rideCamera;Vector3 position=c.transform.position;Quaternion rotation=c.transform.rotation;float fov=c.fieldOfView,aspect=c.aspect;
  var rt=new RenderTexture(640,640,24);var image=new Texture2D(640,640,TextureFormat.RGB24,false);var target=c.targetTexture;var active=RenderTexture.active;
  try
  {
   c.transform.position=g.transform.TransformPoint(new Vector3(7.6f,3,14));c.transform.LookAt(g.transform.TransformPoint(new Vector3(7.6f,1.5f,20)));c.fieldOfView=38;c.aspect=1;Call("UpdateJobMarker");
   c.targetTexture=rt;c.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,640,640),0,0);image.Apply();
   File.WriteAllBytes("Captures/waypoint-3d-detail-"+(Screen.width<Screen.height?"portrait":"wide")+".png",image.EncodeToPNG());
  }
  finally{c.targetTexture=target;RenderTexture.active=active;c.transform.SetPositionAndRotation(position,rotation);c.fieldOfView=fov;c.aspect=aspect;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);Call("UpdateJobMarker");}
 }
 [Serializable]class Error{public string error;}
}
