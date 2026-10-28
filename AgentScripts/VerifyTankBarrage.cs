using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Xe tăng bắn đạn (25/10/2026, huongdan.md 21.38): mỗi 9 s một xe tăng ngược chiều, bắn mỗi giây vào 1 trong 4 làn
// phía trước, vòng cảnh báo dưới đất, trúng/nổ trong bán kính → văng ra và kết thúc; xe khác trong bán kính văng ra.
public static class VerifyTankBarrage
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool dinosaurs=g.dinosaursEnabled,rivers=g.riversEnabled,barrage=g.tankBarrageEnabled,random=g.randomTanks,surprises=g.trafficSurprises,danger=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();var data=new List<string>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Action<float> tick=dt=>{if(g.State==PhoVietGame.RideState.Riding)typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});else if(g.State==PhoVietGame.RideState.Falling)typeof(PhoVietGame).GetMethod("TickRiderCrash",F).Invoke(g,new object[]{dt});};
  var clear=typeof(PhoVietGame).GetMethod("ClearShells",F);
  Action prepare=()=>
  {
   g.dinosaursEnabled=false;g.riversEnabled=false;g.tankBarrageEnabled=true;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;
   g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
  };
  try
  {
   Time.timeScale=0;
   check(g.tankShellPrefab!=null&&g.tankShellMaterial!=null&&g.tankMuzzleMaterial?.mainTexture!=null&&g.tankExplosionMaterial?.mainTexture!=null&&g.tankSparkMaterial?.mainTexture!=null,"Blender shell model, muzzle flash and explosion flipbooks are installed");
   check(g.tankWarningMaterial!=null&&g.tankWarningMaterial.shader.name=="PhoViet/Tank Warning"&&g.tankWarningMaterial.mainTexture!=null,"Ground warning ring uses the Blender texture and its own shader");
   var sceneText=System.IO.File.ReadAllText("Assets/PhoViet/Scenes/PhoViet.unity");
   check(sceneText.Contains("randomTanks: 0")&&sceneText.Contains("tankBarrageEnabled: 1"),"Saved scene: random car-slot tanks off, 9-second tank schedule on");
   g.randomTanks=false;
   // ---- Không còn xe tăng ngẫu nhiên
   prepare();g.tankBarrageEnabled=false;
   var tankSlots=g.vehicles.Where(v=>v.model.GetComponent<MeshyCarVariants>()?.ArmoredIndex>=0).ToList();
   int randomTanks=0;
   for(int i=0;i<300;i++){var slot=tankSlots[i%tankSlots.Count];var v=g.SpawnTraffic(i%4,60,g.vehicles.IndexOf(slot));if(v!=null&&v.armored)randomTanks++;if(v!=null){v.active=false;v.model.gameObject.SetActive(false);}}
   check(randomTanks==0,"Ordinary car spawns never turn into tanks any more");
   foreach(var slot in tankSlots){var m=slot.model.GetComponent<MeshyCarVariants>();m.PrepareForSpawn(new System.Random(3),slot,false);m.SelectForSpawn(new System.Random(3),slot);check(!slot.armored,"Turn-exit traffic never prepares a random tank: "+slot.model.name);}
   // ---- Lịch 9 giây
   prepare();g.StartRide();
   var spawnTimes=new List<float>();var lanes=new List<int>();var fireTimes=new Dictionary<string,List<float>>();var fireSpeeds=new Dictionary<string,List<float>>();var targets=new List<string>();
   float t=0;int lastTanks=0,lastFired=0;
   for(int i=0;i<30*40&&g.State==PhoVietGame.RideState.Riding;i++)
   {
    tick(1/30f);t+=1/30f;
    if(g.TanksScheduled>lastTanks){lastTanks=g.TanksScheduled;spawnTimes.Add(t);
     var tank=g.vehicles.Where(v=>v.active&&v.armored).OrderByDescending(v=>v.model.localPosition.z).First();
     check(tank.baseTrafficSpeed<0&&(tank.lane==0||tank.lane==1)&&Mathf.Abs(tank.speedScale-.4f)<1e-4f&&tank.model.localPosition.z>100,"Scheduled tank is oncoming, in an oncoming lane, 60% slower and appears far ahead");}
    if(g.ShellsFired>lastFired)
    {
     foreach(var s in g.shells.Where(s=>s.age<=1/30f+1e-4f))
     {
      lanes.Add(s.lane);string key=g.vehicles.IndexOf(s.tank)+"/"+s.tank.spawnGeneration;if(!fireTimes.ContainsKey(key)){fireTimes[key]=new List<float>();fireSpeeds[key]=new List<float>();}fireTimes[key].Add(t);fireSpeeds[key].Add(g.Speed);
      check(s.marker!=null&&s.marker.gameObject.activeSelf&&Mathf.Abs(s.marker.transform.localPosition.x-PhoVietGame.LaneX[s.lane])<.01f,"A warning ring appears on the ground at the landing lane as soon as the tank fires");
      float ahead=s.target.z-g.rider.localPosition.z,expected=Mathf.Max(g.Speed,8)*PhoVietGame.ShellFlight;
      targets.Add(ahead.ToString("F1")+"/"+expected.ToString("F1"));
      check(Mathf.Abs(ahead-expected)<2.5f,"The shell lands where the rider will be after the flight time");
      check(s.tank.baseTrafficSpeed<0&&s.start.z>s.target.z,"Only oncoming tanks fire, from the muzzle toward the rider");
     }
     lastFired=g.ShellsFired;clear.Invoke(g,null); // không để đạn trúng người chơi trong bài đo lịch
    }
   }
   data.Add("tankSpawnTimes="+string.Join(",",spawnTimes.Select(x=>x.ToString("F2"))));
   check(spawnTimes.Count==4&&spawnTimes.Select((x,i)=>Mathf.Abs(x-9*(i+1))).Max()<.15f,"A tank appears exactly every 9 seconds (9, 18, 27, 36 s)");
   // Khoảng giữa hai phát của cùng một xe = khoảng tính theo tốc độ người chơi lúc bắn phát trước (21.39).
   var gaps=new List<float>();var gapErr=new List<float>();
   foreach(var key in fireTimes.Keys){var ts=fireTimes[key];var sp=fireSpeeds[key];for(int k=1;k<ts.Count;k++){gaps.Add(ts[k]-ts[k-1]);gapErr.Add(Mathf.Abs(ts[k]-ts[k-1]-PhoVietGame.TankFireIntervalAt(sp[k-1])));}}
   data.Add("shots="+lanes.Count+" lanes="+string.Join("",lanes)+" gaps="+string.Join(",",gaps.Select(x=>x.ToString("F2"))));
   check(gaps.Count>=4&&gapErr.All(x=>x<.05f),"Each tank fires at the speed-based interval while in range"+(gapErr.All(x=>x<.05f)?"":" ["+data.Last()+"]"));
   check(gaps.All(x=>x<=1.02f&&x>=.38f),"Fire interval stays between 1.0 s and 0.4 s");
   check(lanes.Distinct().Count()>=3&&lanes.All(l=>l>=0&&l<4),"Shells land in random lanes among all four");
   // ---- Khoảng bắn theo tốc độ: 1,0 s ở 17 m/s → 0,4 s ở 30 m/s, không dưới 0,4 s
   check(Mathf.Abs(PhoVietGame.TankFireIntervalAt(17)-1)<1e-4f&&Mathf.Abs(PhoVietGame.TankFireIntervalAt(23.5f)-.7f)<1e-4f&&Mathf.Abs(PhoVietGame.TankFireIntervalAt(30)-.4f)<1e-4f,"Fire interval is 1.0 s at 17 m/s, 0.7 s at 23.5 m/s and 0.4 s at 30 m/s");
   check(PhoVietGame.TankFireIntervalAt(45)==.4f&&PhoVietGame.TankFireIntervalAt(60)==.4f&&PhoVietGame.TankFireIntervalAt(8)==1f,"Faster than 30 m/s (boost/jump) never fires faster than every 0.4 s");
   float prev=2;bool monotonic=true;for(float sp=17;sp<=31;sp+=.5f){float iv=PhoVietGame.TankFireIntervalAt(sp);if(iv>prev+1e-5f)monotonic=false;prev=iv;}
   check(monotonic,"The faster the rider, the shorter the interval between shots");
   prepare();g.StartRide();set("timeRiding",400f);tick(1/30f);
   check(Mathf.Abs(g.Speed-30)<.01f,"Top cruising speed reached for the fast-fire test");
   set("tankSpawnTimer",.01f);float tt=0;var fastTimes=new List<float>();int seen=g.ShellsFired;
   for(int i=0;i<30*12&&g.State==PhoVietGame.RideState.Riding&&fastTimes.Count<4;i++)
   {
    tick(1/30f);tt+=1/30f;set("tankSpawnTimer",100f);
    if(g.ShellsFired>seen){seen=g.ShellsFired;fastTimes.Add(tt);clear.Invoke(g,null);}
   }
   var fastGaps=fastTimes.Zip(fastTimes.Skip(1),(a,b)=>b-a).ToList();
   data.Add("fastSpeed="+g.Speed.ToString("F1")+" fastGaps="+string.Join(",",fastGaps.Select(x=>x.ToString("F2"))));
   check(fastGaps.Count>=2&&fastGaps.All(x=>Mathf.Abs(x-.4f)<.04f),"At top speed a tank fires every 0.4 s"+(fastGaps.Count>=2&&fastGaps.All(x=>Mathf.Abs(x-.4f)<.04f)?"":" ["+data.Last()+"]"));
   // ---- Trúng làn người chơi → văng ra, kết thúc
   prepare();g.tankBarrageEnabled=false;g.StartRide();
   var shooter=g.SpawnScheduledTank();for(int i=0;i<300&&shooter.model.localPosition.z>95;i++)tick(1/60f);
   var hit=g.FireTankShell(shooter,g.CurrentLane);check(hit!=null,"Tank in range fires at the rider's lane");
   float hitAt=-1;for(int i=0;i<60*3&&hitAt<0;i++){tick(1/60f);if(g.State!=PhoVietGame.RideState.Riding)hitAt=i/60f;}
   data.Add("hitAfter="+hitAt.ToString("F2"));
   check(hitAt>=PhoVietGame.ShellFlight-.1f&&hitAt<=PhoVietGame.ShellFlight+.1f&&g.KilledByShell,"A shell landing on the rider's lane throws the rider off at impact");
   float peak=0;for(int i=0;i<60*8&&g.State==PhoVietGame.RideState.Falling;i++){tick(1/60f);peak=Mathf.Max(peak,g.rider.localPosition.y);}
   check(peak>1.2f&&g.State==PhoVietGame.RideState.GameOver,"The blast flings the rider into the air and the ride ends");
   check(g.ShellBlasts>=1,"The impact explodes");
   // ---- Làn bên cạnh: an toàn (ngoài bán kính 2,2 m)
   prepare();g.tankBarrageEnabled=false;g.StartRide();
   shooter=g.SpawnScheduledTank();for(int i=0;i<300&&shooter.model.localPosition.z>95;i++)tick(1/60f);
   g.FireTankShell(shooter,3);for(int i=0;i<60*2;i++)tick(1/60f);
   check(g.State==PhoVietGame.RideState.Riding&&g.ShellBlasts==1&&!g.KilledByShell,"A shell exploding in the next lane misses the rider");
   // ---- Né: đổi làn sau khi thấy cảnh báo
   prepare();g.tankBarrageEnabled=false;g.StartRide();
   shooter=g.SpawnScheduledTank();for(int i=0;i<300&&shooter.model.localPosition.z>95;i++)tick(1/60f);
   g.FireTankShell(shooter,g.CurrentLane);for(int i=0;i<18;i++)tick(1/60f);g.MoveLane(-1);
   for(int i=0;i<60*2;i++)tick(1/60f);
   check(g.State==PhoVietGame.RideState.Riding&&g.ShellBlasts==1,"Changing lane after seeing the warning dodges the shell");
   // ---- Xe khác trong bán kính văng ra; xe tăng thì không
   prepare();g.tankBarrageEnabled=false;g.StartRide();
   var car=g.SpawnTraffic(3,40);var tankNear=g.SpawnScheduledTank();tankNear.model.localPosition=new Vector3(PhoVietGame.LaneX[1],0,40);
   typeof(PhoVietGame).GetMethod("EnsureTankFx",F).Invoke(g,null);
   var dummy=new PhoVietGame.TankShell();
   typeof(PhoVietGame).GetMethod("Detonate",F).Invoke(g,new object[]{dummy,new Vector3(PhoVietGame.LaneX[2]+.4f,0,40),false});
   check(car.knocked&&car.knockVelocity.y>2&&g.VehiclesBlasted>=1,"A car inside the blast radius is thrown up and away");
   check(!tankNear.knocked,"The tank itself is not pushed by explosions");
   check(g.State==PhoVietGame.RideState.Riding,"A blast 40 m ahead does not hurt the rider");
   // ---- Trên cầu: đạn rơi xuống sông thì bắn nước, không nổ trên mặt đường
   g.riversEnabled=true;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);g.tankBarrageEnabled=false;
   foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(Mathf.Abs(b.localPosition.z-80)<1);
   int splashes=g.ShellSplashes,blasts=g.ShellBlasts;
   var water=new PhoVietGame.TankShell{overWater=true};
   typeof(PhoVietGame).GetMethod("Detonate",F).Invoke(g,new object[]{water,new Vector3(PhoVietGame.LaneX[3],RiverBlock.WaterY,105),false});
   check(g.ShellSplashes==splashes+1&&g.ShellBlasts==blasts,"A shell landing in the river splashes instead of exploding");
   // ---- Chơi lại xóa đạn đang bay
   prepare();g.tankBarrageEnabled=false;g.StartRide();shooter=g.SpawnScheduledTank();for(int i=0;i<300&&shooter.model.localPosition.z>95;i++)tick(1/60f);
   var flying=g.FireTankShell(shooter,0);var body=flying.body;g.RestartRide();
   check(g.shells.Count==0&&!body.gameObject.activeSelf,"Restarting clears shells and warnings in flight");
   return new{success=true,count=checks.Count,checks=checks.ToArray(),data=data.ToArray()};
  }
  finally
  {
   Time.timeScale=scale;g.dinosaursEnabled=dinosaurs;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.randomTanks=random;g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;
   g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();
  }
 }
}
