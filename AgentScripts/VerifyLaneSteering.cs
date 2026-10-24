using System;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Đổi làn kiểu đánh lái (24/10/2026): quỹ đạo chữ S êm, đầu xe quay theo hướng đi, nghiêng đúng phía,
// hộp va chạm giữ hướng đường; chụp ảnh giữa lúc đổi làn để xem bằng mắt.
public static class VerifyLaneSteering
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var cases=new List<object>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action<int> reset=lane=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);};
  var visual=g.rider.GetComponentInChildren<BikeVisualAnimator>();
  try
  {
   Time.timeScale=0;
   foreach(float dt in new[]{1/60f,1/30f,.05f})foreach(int dir in new[]{-1,1})foreach(float speed in new[]{17f,30f})
   {
    int from=dir>0?1:2;reset(from);set("timeRiding",(speed-17)/.12f/3f);tick(dt);
    string label=$"dt={dt:0.000},dir={dir},speed={speed}";float x0=g.rider.localPosition.x,target=PhoVietGame.LaneX[from+dir];
    g.MoveLane(dir);check(g.CurrentLane==from+dir,"A/D or swipe selects the adjacent lane: "+label);
    float t=0,reached=-1,ninety=-1,peakYaw=0,peakAt=0,upright=-1,maxDrop=0,lastYaw=0,firstLean=0,lastX=x0,maxRootYaw=0;bool monotonic=true,overshoot=false;
    for(int i=0;i<200&&t<1.5f;i++)
    {
     tick(dt);t+=dt;float x=g.rider.localPosition.x;
     if((x-lastX)*dir<-1e-5f)monotonic=false;if((x-target)*dir>1e-4f)overshoot=true;lastX=x;
     float yaw=g.RiderSteerYaw;if(Mathf.Abs(yaw)>Mathf.Abs(peakYaw)){peakYaw=yaw;peakAt=t;}
     if(peakAt>0&&t>peakAt)maxDrop=Mathf.Max(maxDrop,(Mathf.Abs(lastYaw)-Mathf.Abs(yaw))/dt);lastYaw=yaw;
     if(upright<0&&peakAt>0&&t>peakAt&&Mathf.Abs(yaw)<2)upright=t;
     if(ninety<0&&Mathf.Abs(x-x0)>=.9f*Mathf.Abs(target-x0))ninety=t;
     float roll=Mathf.DeltaAngle(0,g.rider.localEulerAngles.z);if(firstLean==0&&Mathf.Abs(roll)>.5f)firstLean=roll;
     maxRootYaw=Mathf.Max(maxRootYaw,Mathf.Abs(Mathf.DeltaAngle(0,g.rider.localEulerAngles.y)));
     if(reached<0&&Mathf.Abs(x-target)<.01f)reached=t;
    }
    check(ninety>0&&ninety<=.36f,"90% of the lane is crossed quickly enough for dodging (<=0.36 s): "+label+",t="+ninety);
    check(reached>0&&reached<=1.2f,"Bike settles exactly on the lane centre (<=1.2 s): "+label+",t="+reached);
    check(upright-peakAt>=.3f,"Nose straightens gradually while riding on, not snapping (>=0.3 s from peak to <2°): "+label+",t="+(upright-peakAt));
    check(maxDrop<=125,"No abrupt straightening: steering returns at most 125°/s: "+label+",rate="+maxDrop);
    check(monotonic&&!overshoot,"Smooth S-path moves only toward the new lane and never overshoots: "+label);
    check(peakYaw*dir>6&&Mathf.Abs(peakYaw)<=26.01f,"Bike nose turns toward the travel direction (6–26°): "+label+",yaw="+peakYaw);
    check(firstLean*dir<0,"Bike first leans into the turn (right = negative roll, same as junction turns): "+label+",lean="+firstLean);
    check(Mathf.Abs(g.RiderSteerYaw)<1.5f&&Mathf.Abs(Mathf.DeltaAngle(0,g.rider.localEulerAngles.z))<2f,"Bike straightens and stands up after the lane change: "+label+",yaw="+g.RiderSteerYaw+",roll="+Mathf.DeltaAngle(0,g.rider.localEulerAngles.z)+",x="+g.rider.localPosition.x+",t="+t);
    check(maxRootYaw<.01f,"Gameplay body keeps the road heading; steering yaw is visual only: "+label);
    cases.Add(new{label,ninety,reached,peakYaw,peakAt,upright,maxDrop,firstLean});
   }
   // Phần hình thực sự quay theo hướng lái.
   reset(1);tick(1/60f);visual.Advance(0);var straight=Quaternion.Inverse(g.rider.rotation)*visual.visual.rotation;
   g.MoveLane(1);for(int i=0;i<8;i++)tick(1/60f);visual.Advance(0);
   var turned=Quaternion.Inverse(g.rider.rotation)*visual.visual.rotation;var delta=(turned*Quaternion.Inverse(straight)).eulerAngles;
   check(g.RiderSteerYaw>6,"Steering is active mid lane change");
   check(Mathf.Abs(Mathf.DeltaAngle(delta.y,g.RiderSteerYaw))<.5f&&Mathf.Abs(Mathf.DeltaAngle(0,delta.x))<.5f&&Mathf.Abs(Mathf.DeltaAngle(0,delta.z))<.5f,"Rider and motorcycle model rotate about the vertical axis by exactly the steering yaw: "+delta+" vs "+g.RiderSteerYaw);
   Capture(g,"lane-steering-right");
   reset(2);tick(1/60f);g.MoveLane(-1);for(int i=0;i<8;i++)tick(1/60f);visual.Advance(0);Capture(g,"lane-steering-left");
   // Đổi hướng giữa chừng: quay về làn cũ êm, không giật.
   reset(1);tick(1/60f);g.MoveLane(1);for(int i=0;i<6;i++)tick(1/60f);g.MoveLane(-1);float back=0;float px=g.rider.localPosition.x;bool jump=false;
   for(int i=0;i<90;i++){tick(1/60f);float x=g.rider.localPosition.x;if(Mathf.Abs(x-px)>LaneStep())jump=true;px=x;if(Mathf.Abs(x-PhoVietGame.LaneX[1])<.01f&&back==0)back=i/60f;}
   check(!jump&&back>0,"Reversing mid change returns to the original lane without a position jump");
   // Chuỗi ảnh một lần sang phải: 0,10 / 0,25 / 0,45 / 0,65 / 0,90 s để xem lấy lại thăng bằng từ từ.
   reset(1);tick(1/60f);g.MoveLane(1);float clock=0;int shot=0;float[] at={.1f,.25f,.45f,.65f,.9f};
   for(int i=0;i<60&&shot<at.Length;i++){tick(1/60f);clock+=1/60f;if(clock>=at[shot]-1e-4f){visual.Advance(0);Capture(g,"lane-steering-seq-"+shot);shot++;}}
   return new{success=true,count=checks.Count,checks,cases};
  }
  finally
  {
   Time.timeScale=scale;g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();
  }
 }
 static float LaneStep()=>15f/60f+.01f;
 static void Capture(PhoVietGame g,string name)
 {
  var c=g.rideCamera;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  var pos=c.transform.position;var rot=c.transform.rotation;
  c.transform.position=g.rider.position+new Vector3(0,2.6f,-5.2f);c.transform.LookAt(g.rider.position+new Vector3(0,1,2));
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();System.IO.File.WriteAllBytes("Captures/"+name+".png",t.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;c.transform.position=pos;c.transform.rotation=rot;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
 }
}
