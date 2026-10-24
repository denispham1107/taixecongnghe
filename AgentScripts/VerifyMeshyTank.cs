using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Xe tăng Meshy (24/10/2026): xuất hiện ngẫu nhiên như xe con, một màu gốc, chậm hơn 60%,
// va chạm không đổi hướng/không dừng — mọi phương tiện khác (và người chơi) văng ra.
public static class VerifyMeshyTank
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool jobs=g.jobsEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);var checks=new List<string>();var data=new Dictionary<string,object>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object> get=n=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action reset=()=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,0);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[0],0,0);};
  var slots=g.vehicles.Where(v=>v.model.GetComponent<MeshyCarVariants>()?.variants.Any(x=>x.armored)==true).ToList();
  try
  {
   Time.timeScale=0;
   check(slots.Count==5,"Five ordinary car slots can become the Meshy tank: "+slots.Count);
   foreach(var s in slots)
   {
    var m=s.model.GetComponent<MeshyCarVariants>();var tank=m.variants.Single(x=>x.armored);
    check(m.variants.Length==2&&m.variants[0].asset=="PV_MeshySport"&&tank.asset=="PV_MeshyTank","Car slot keeps the sport car and adds the tank: "+s.model.name);
    check(tank.fixedPaint&&Mathf.Abs(tank.speedScale-.4f)<1e-4f&&tank.weight>0&&tank.skins.Length==3&&tank.skins.All(r=>r.sharedMaterial!=null&&r.sharedMaterial.name=="PV_MeshyTank_Atlas"),"Tank variant has three LODs, its own material, fixed paint and 0.4 speed scale: "+s.model.name);
    check(Mathf.Abs(tank.size.z-7)<.01f&&tank.size.x<3.1f,"Tank is 7 m long and narrower than a 3.1 m lane: "+tank.size);
   }
   // Tỉ lệ xuất hiện và loại trừ khi vượt từ phía sau.
   var rand=new System.Random(4021);var slot=slots[0];var meshy=slot.model.GetComponent<MeshyCarVariants>();int tanks=0,n=3000;
   for(int i=0;i<n;i++){meshy.SelectForSpawn(rand,slot);if(meshy.Armored)tanks++;}
   float share=tanks/(float)n;data["tankShare"]=share;
   check(Mathf.Abs(share-1/3f)<.03f,"Tank appears randomly in about one third of car-slot spawns: "+share);
   int rearTanks=0;for(int i=0;i<500;i++){meshy.PrepareForSpawn(rand,slot,false);meshy.SelectForSpawn(rand,slot);if(meshy.Armored)rearTanks++;}
   check(rearTanks==0,"Rear overtaking spawns never choose the slow tank");
   // Một màu gốc: mọi chỉ số sơn đều cho màu trắng (không nhuộm) trên xe tăng.
   int tankIndex=Array.FindIndex(meshy.variants,x=>x.armored);var block=new MaterialPropertyBlock();
   for(int p=0;p<8;p++){meshy.Select(tankIndex,p,slot);meshy.variants[tankIndex].skins[0].GetPropertyBlock(block);check(block.GetColor("_PaintColor")==Color.white,"Tank keeps its single original colour for paint index "+p);}
   check(slot.armored&&Mathf.Abs(slot.speedScale-.4f)<1e-4f,"Selecting the tank marks the pool vehicle armored and slow");
   meshy.Select(0,3,slot);check(!slot.armored&&slot.speedScale==1,"Selecting the sport car clears the tank flags");
   // Tốc độ: so cùng hàm sinh xe giữa xe tăng và xe thể thao.
   reset();var tankSpeeds=new List<float>();var carSpeeds=new List<float>();var crossTank=new List<float>();var crossCar=new List<float>();
   for(int i=0;i<600&&(tankSpeeds.Count<40||carSpeeds.Count<40);i++)
   {
    foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    var v2=g.SpawnTraffic(2,40,g.vehicles.IndexOf(slots[i%slots.Count]));if(v2==null)continue;
    (v2.armored?tankSpeeds:carSpeeds).Add(v2.speed);check(v2.baseTrafficSpeed==v2.speed,"Base speed follows the scaled spawn speed");
   }
   for(int i=0;i<600&&(crossTank.Count<30||crossCar.Count<30);i++)
   {
    foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    var c=g.SpawnCrossTraffic(1,70,true,12);if(c==null||c.model.GetComponent<MeshyCarVariants>()?.variants.Any(x=>x.armored)!=true)continue;
    (c.armored?crossTank:crossCar).Add(Mathf.Abs(c.crossSpeed));
   }
   check(tankSpeeds.Count>=40&&carSpeeds.Count>=40&&crossTank.Count>=30&&crossCar.Count>=30,"Enough tank and car spawns sampled");
   float ratio=tankSpeeds.Average()/carSpeeds.Average();data["sameDirectionSpeedRatio"]=ratio;data["tankMean"]=tankSpeeds.Average();data["carMean"]=carSpeeds.Average();
   check(Mathf.Abs(ratio-.4f)<.05f&&tankSpeeds.All(s=>s>=.4f*7-.01f&&s<=.4f*12+.01f),"Tank drives 60% slower than cars (mean ratio ≈ 0.4): "+ratio);
   check(crossTank.All(s=>Mathf.Abs(s-12*.4f)<.01f)&&crossCar.All(s=>Mathf.Abs(s-12)<.01f),"Crossing tanks also move at 40% of the car crossing speed");
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   var on=g.SpawnTraffic(1,60,g.vehicles.IndexOf(slots[0]));for(int i=0;i<200&&!on.armored;i++){on.active=false;on.model.gameObject.SetActive(false);on=g.SpawnTraffic(1,60,g.vehicles.IndexOf(slots[0]));}
   check(on.armored&&on.speed<0&&on.speed>=-.4f*17-.01f&&on.speed<=-.4f*10+.01f,"Oncoming tanks are also 60% slower: "+on.speed);
   // Va chạm xe–xe: ô tô và xe máy đâm vào/bị xe tăng đâm đều văng; xe tăng giữ hướng và tốc độ.
   foreach(string kind in new[]{"car-rear-ends-tank","tank-rear-ends-car","bike-rear-ends-tank","tank-meets-oncoming-car"})
   {
    reset();var t=Force(g,slots[0],tankIndex,3,30,kind=="tank-rear-ends-car"?20f:4.4f);
    TrafficVehicleProxy other;
    if(kind=="bike-rear-ends-tank"){int bi=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth<.7f);var b=g.SpawnTraffic(3,18,bi);b.speed=b.baseTrafficSpeed=16;b.model.localRotation=Quaternion.identity;other=new TrafficVehicleProxy(b);}
    else
    {
     var o=slots[1];var m=o.model.GetComponent<MeshyCarVariants>();
     if(kind=="tank-meets-oncoming-car"){var c=g.SpawnTraffic(3,46,g.vehicles.IndexOf(o));m.Select(0,2,c);c.speed=c.baseTrafficSpeed=-10;c.model.localRotation=Quaternion.Euler(0,180,0);other=new TrafficVehicleProxy(c);}
     else{float z=kind=="car-rear-ends-tank"?18:42;var c=g.SpawnTraffic(3,z,g.vehicles.IndexOf(o));m.Select(0,2,c);c.speed=c.baseTrafficSpeed=kind=="car-rear-ends-tank"?16:0.5f;c.model.localRotation=Quaternion.identity;other=new TrafficVehicleProxy(c);}
    }
    float tankSpeed=t.speed;bool hit=false;float minYaw=0,maxYaw=0;Vector3 otherBefore=Vector3.zero,otherAtHit=Vector3.zero;
    for(int i=0;i<240;i++)
    {
     tick(1/60f);
     float yaw=Mathf.DeltaAngle(0,t.model.localEulerAngles.y);minYaw=Mathf.Min(minYaw,yaw);maxYaw=Mathf.Max(maxYaw,yaw);
     if(other.v.knocked&&!hit){hit=true;otherAtHit=other.v.knockVelocity;}else if(!hit)otherBefore=new Vector3(0,0,other.v.speed);
     check(!t.knocked&&!t.physicalCrash&&Mathf.Abs(t.speed-tankSpeed)<1e-4f&&Mathf.Abs(t.model.localPosition.x-PhoVietGame.LaneX[3])<1e-3f,"Tank never stops, slows or is pushed sideways: "+kind+" frame "+i);
    }
    check(hit&&other.v.physicalCrash,"The other vehicle is knocked away by the tank: "+kind);
    check(Mathf.Abs(minYaw)<.01f&&Mathf.Abs(maxYaw)<.01f,"Tank heading never changes: "+kind);
    float change=(otherAtHit-otherBefore).magnitude;
    check(change>1f,"The other vehicle takes the whole impact and bounces off: "+kind+",dv="+change);
    data[kind]=new{velocityChange=change,otherBefore,otherAtHit};
   }
   // Người chơi đâm xe tăng: người chơi té, xe tăng không bị đẩy.
   reset();typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,2);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],0,0);
   var target=Force(g,slots[0],tankIndex,2,10,4.4f);float before=target.speed;Vector3 p0=target.model.localPosition;
   for(int i=0;i<180&&g.State==PhoVietGame.RideState.Riding;i++)tick(1/60f);
   check(g.State==PhoVietGame.RideState.Falling,"Rider crashing into a tank falls");
   check(!target.knocked&&target.speed==before&&Mathf.Abs(target.model.localPosition.x-p0.x)<1e-3f,"Tank is not pushed by the rider");
   return new{success=true,count=checks.Count,checks,data=data.Select(kv=>kv.Key+" = "+kv.Value).ToArray()};
  }
  finally
  {
   Time.timeScale=scale;g.jobsEnabled=jobs;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;
   PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();
  }
 }
 sealed class TrafficVehicleProxy{public PhoVietGame.TrafficVehicle v;public TrafficVehicleProxy(PhoVietGame.TrafficVehicle x){v=x;}}
 // Đặt đúng mẫu xe tăng vào ô và áp tốc độ đã nhân 0,4 như InitializeDriver.
 static PhoVietGame.TrafficVehicle Force(PhoVietGame g,PhoVietGame.TrafficVehicle slot,int tankIndex,int lane,float z,float speed)
 {
  var t=g.SpawnTraffic(lane,z,g.vehicles.IndexOf(slot));var m=t.model.GetComponent<MeshyCarVariants>();m.Select(tankIndex,0,t);
  t.speed=t.baseTrafficSpeed=speed*t.speedScale;t.model.localRotation=Quaternion.identity;t.previousPosition=t.model.localPosition;return t;
 }
}
