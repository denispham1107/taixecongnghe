using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureCrashOutcome
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static bool running,waiting,jobs;static float time;static int phase,best,coins;static double next;
 static readonly string[] names={"hud","gameover","waiting-police","jailed"};
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 public static object Main()
 {
  if(running)return new{started=true};if(!Application.isPlaying)throw new Exception("Play Mode required");
  g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();jobs=g.jobsEnabled;time=Time.timeScale;best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  phase=0;waiting=false;running=true;Time.timeScale=0;Setup();next=EditorApplication.timeSinceStartup+.6;EditorApplication.update+=Update;return new{started=true,frames=4};
 }
 static void Setup()
 {
  g.jobsEnabled=phase==0;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
  if(phase==0){g.ShowJobOffers();g.AcceptJob(0);}
  else
  {
   if(phase>=2){Call("StartChase");g.policeBike.localPosition=g.rider.localPosition+Vector3.back*200;}
   var p=g.rider.localPosition;var hit=g.SpawnCrossTraffic(1,p.z,true,12);hit.previousPosition=p-Vector3.right*8;hit.model.localPosition=p+Vector3.right*8;hit.motionVelocity=Vector3.right*12;
   typeof(PhoVietGame).GetProperty("Speed").SetValue(g,0f);typeof(PhoVietGame).GetField("oldRiderZ",F).SetValue(g,p.z);Call("ResolveRiderContacts",p.x,.05f);hit.active=false;hit.model.gameObject.SetActive(false);
   var hero=g.rider.GetComponent<BikeVisualAnimator>();int guard=0;
   while(!g.RiderFallComplete&&guard++<1200){Call("TickRiderCrash",.02f);hero.Advance(.02f);}
   if(phase==3){g.policeBike.localPosition=g.rider.localPosition+new Vector3(-.5f,0,-1);Call("TickRiderCrash",.02f);Call("TickArrest",1.2f);}
  }
  Call("PositionCamera",true);
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  string label=Screen.width<Screen.height?"portrait":"wide",path="Captures/crash-outcome-native-"+label+"-"+names[phase]+".png";
  try
  {
   if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.7;return;}
   if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Screenshot missing: "+path);
   if(++phase==names.Length){Finish();File.WriteAllText("Captures/crash-outcome-native-"+label+"-visual-results.json","{\"success\":true,\"frames\":4}");return;}
   waiting=false;Setup();next=EditorApplication.timeSinceStartup+.6;
  }
  catch(Exception ex){Finish();File.WriteAllText("Captures/crash-outcome-native-"+label+"-visual-results.json",JsonUtility.ToJson(new Error{error=ex.ToString()}));}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=time;}
 [Serializable]class Error{public string error;}
}
