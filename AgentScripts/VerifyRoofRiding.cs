using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyRoofRiding
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action<float> tick=dt=>call("TickRide",new object[]{dt});
  Action reset=()=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  var checks=new List<string>();var failures=new List<string>();int cases=0,lateCases=0,stepCases=0;
  Action<bool,string> check=(ok,s)=>{if(ok)checks.Add(s);else failures.Add(s);};
  try
  {
   Time.timeScale=0;
   foreach(string asset in new[]{"PV_Sedan","PV_Hatchback","PV_Taxi","PV_DeliveryVan","PV_BoxTruck","PV_DrowsyTruck"})
   {
    int index=g.vehicles.FindIndex(v=>v.model.GetComponent<CarVisualAnimator>()?.AssetName==asset);var pool=g.vehicles[index];
    // Meshy trucks pick covered/container at spawn. Pin each shape before and after
    // SpawnTraffic so timing, roof height and the tested body are the same model.
    var meshy=pool.model.GetComponent<MeshyCarVariants>();
    for(int variant=0;variant<(meshy!=null?meshy.variants.Length:1);variant++)foreach(float dt in new[]{.016f,.033f,.05f,.1f})foreach(bool boost in new[]{false,true})
    {
     reset();pool.model.localRotation=Quaternion.identity;meshy?.Select(variant,0,pool);string model=meshy!=null?meshy.ModelAsset:asset;
     float roof=(meshy!=null?meshy.SupportHeight(-.73f,.90f):PhoVietGame.JumpRoofHeight(pool))+.025f;if(boost)g.TryBoost();g.TryJump();
     float time=0,oldHeight=0,oldDistance=0,startDistance=g.Distance,riderZ=g.rider.localPosition.z,apex=0,peak=0;
     var flight=new List<Vector3>{new Vector3(0,0,0)};
     for(int i=0;i<300;i++){oldHeight=g.RiderJumpHeight;oldDistance=g.Distance;tick(dt);time+=dt;flight.Add(new Vector3(time,g.RiderJumpHeight,g.Distance-startDistance));if(g.RiderJumpHeight>peak){peak=g.RiderJumpHeight;apex=time;}if(oldHeight>roof&&g.RiderJumpHeight<=roof)break;}
     float fraction=Mathf.InverseLerp(oldHeight,g.RiderJumpHeight,roof),contactTime=time-dt+fraction*dt,contactDistance=Mathf.Lerp(oldDistance,g.Distance,fraction);
     foreach(float nominal in new[]{-12f,0f,7f,28f})foreach(float offset in new[]{-.5f,0f,.5f})
     {
      // Xe tăng Meshy chỉ chạy 40% tốc độ ô tô (speedScale, 21.33): thử ở tốc độ thật nó có thể có.
      float speed=nominal*(meshy!=null?meshy.variants[variant].speedScale:1);
      float spawnZ=contactDistance-speed*contactTime+offset;
      // A late jump: the vehicle body reaches the free-flight path while the rider is still
      // climbing below it (e.g. a 12.8 m container overtaking at 28 m/s arrives 0.3 s after
      // the press). That is a real rear/front impact, not a missed landing; expect a crash.
      // Sweep between frames like the game's contact test, not only at frame ends.
      bool late=false;float margin=float.PositiveInfinity,length=meshy!=null?meshy.variants[variant].size.z*.5f:pool.halfLength;
      for(int i=1;i<flight.Count&&!late&&flight[i-1].x<apex;i++)for(int k=0;k<=20;k++)
      {
       var p=Vector3.Lerp(flight[i-1],flight[i],k/20f);float dz=riderZ-(spawnZ+speed*p.x-p.z),localZ=speed<0?-dz:dz;if(Mathf.Abs(localZ)>length+.91f)continue;
       float top=meshy!=null?meshy.SupportHeight(localZ-.73f,localZ+.90f):PhoVietGame.JumpRoofHeight(pool);margin=Mathf.Min(margin,p.y-top);if(p.y<top-.04f){late=true;break;}
      }
      reset();bool eventOnly=pool.eventOnly;pool.eventOnly=false;var v=g.SpawnTraffic(2,spawnZ,index);pool.eventOnly=eventOnly;meshy?.Select(variant,0,v);
      v.speed=v.baseTrafficSpeed=speed;v.model.localRotation=Quaternion.Euler(0,speed<0?180:0,0);v.previousPosition=v.model.localPosition;
      if(boost)g.TryBoost();check(g.TryJump(),$"takeoff {model}/{dt}/{boost}/{speed}/{offset}");bool supported=false;float supportTime=0,t=0,crashAt=-1;
      for(int i=0;i<Mathf.CeilToInt(4/dt)&&g.State==PhoVietGame.RideState.Riding;i++){tick(dt);t+=dt;supported|=g.RiderOnRoof||typeof(PhoVietGame).GetField("roofContactVehicle",F).GetValue(g)==v;if(g.RiderOnRoof)supportTime+=dt;if(g.State!=PhoVietGame.RideState.Riding)crashAt=t;}
      string label=$"{model},pool={asset},dt={dt},boost={boost},speed={speed},offset={offset},state={g.State},height={g.RiderJumpHeight},support={supportTime},crashAt={crashAt},late={late},climbMargin={margin}";
      if(late)
      {
       lateCases++;
       check(!supported,"Late jump gets no roof support from a body that reached it below roof height: "+label);
       check(g.State==PhoVietGame.RideState.Falling,"Late jump is a real impact and fall: "+label);
       check(crashAt>0&&crashAt<=apex+dt,"Late-jump impact happens while still climbing, before the planned touchdown: "+label);
      }
      else if(meshy!=null&&meshy.variants[variant].armored&&g.State==PhoVietGame.RideState.Falling&&supportTime>0)
      {
       // Xe tăng (21.33): đáp lên nòng pháo/sàn trước (~1,40 m) rồi gặp tháp pháo cao hơn (~1,7–1,8 m) là va chạm thật
       // theo luật "đang trên nóc gặp phần cao hơn thì đâm" (JumpClearsVehicle), như đâm vách thùng từ cabin thấp.
       // Chỉ chấp nhận khi đã thực sự được nóc đỡ và phía sau phần trước có phần cao hơn ≥0,2 m.
       stepCases++;var prof=meshy.variants[variant].roofProfile;float front=prof[prof.Length-1].y,highest=prof.Max(q=>q.y);
       check(supported&&highest-front>.2f,"Tank landing on the low front then meeting the higher turret is a real step impact: "+label+",rise="+(highest-front));
      }
      else
      {
       check(supported,"Real ballistic jump contacts roof: "+label);
       check(g.State==PhoVietGame.RideState.Riding,"Landing, roof travel and roof exit preserve normal riding: "+label);
       check(g.RiderOnRoof||!g.RiderAirborne&&g.RiderJumpHeight==0,"After flight bike is supported by road or moving roof: "+label);
      }
      cases++;
     }
    }
   }
   return new{success=failures.Count==0,count=checks.Count,cases,lateCases,stepCases,failures,checks};
  }
  finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;}
 }
}
