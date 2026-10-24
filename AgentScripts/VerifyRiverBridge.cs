using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Cầu vượt sông (24/10/2026, huongdan.md 21.35): luật xuất hiện, cầu 2 làn giữa, rơi xuống sông cho người chơi và xe.
public static class VerifyRiverBridge
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;bool rivers=g.riversEnabled,barrage=g.tankBarrageEnabled,danger=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exits=g.turnExitTrafficEnabled,jobs=g.jobsEnabled;
  int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  var checks=new List<string>();var data=new List<string>();
  Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
  Func<string,object> get=n=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
  Action<float> tick=dt=>{if(g.State==PhoVietGame.RideState.Riding)typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});else if(g.State==PhoVietGame.RideState.Falling)typeof(PhoVietGame).GetMethod("TickRiderCrash",F).Invoke(g,new object[]{dt});};
  Func<RiverBlock[]> mainBlocks=()=>g.blocks.OrderBy(b=>b.localPosition.z).Select(b=>b.GetComponent<RiverBlock>()).ToArray();
  Action<int> lane=l=>{typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,l);var p=g.rider.localPosition;p.x=PhoVietGame.LaneX[l];g.rider.localPosition=p;};
  // Khối cầu đặt ở khối có gốc z = at (mặt sông at+16,8 … at+42).
  Action<float> prepare=at=>
  {
   g.riversEnabled=true;g.tankBarrageEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.jobsEnabled=false;
   g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
   set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
   foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(Mathf.Abs(b.localPosition.z-at)<1);
  };
  // Xe thường (không phải xe tăng chậm 60%, kẻo người chơi vượt trước khi xe tới cầu).
  Func<int,float,PhoVietGame.TrafficVehicle> ordinary=(l,z)=>
  {
   for(int i=0;i<40;i++){var v=g.SpawnTraffic(l,z);if(v==null)return null;if(!v.armored)return v;v.active=false;v.model.gameObject.SetActive(false);}
   return null;
  };
  try
  {
   Time.timeScale=0;
   // ---- Cài đặt scene
   var all=g.roadRows.SelectMany(r=>r.tiles).ToList();
   check(all.Count==35&&all.All(t=>t.GetComponent<RiverBlock>()!=null&&t.GetComponent<RiverBlock>().bridge!=null),"All 35 street blocks carry a river bridge variant");
   var sample=all[3].GetComponent<RiverBlock>();
   check(sample.streetOnly.Length>=15&&sample.clipFilters.Length>=4&&sample.clipFilters.Length==sample.clippedMeshes.Length,"Bridge blocks hide the shops/trees/pavement over the river and swap clipped merged meshes");
   foreach(var mesh in all.SelectMany(t=>t.GetComponent<RiverBlock>().clippedMeshes).Distinct())
   {
    var v=mesh.vertices;var tris=mesh.triangles;
    for(int i=0;i<tris.Length;i+=3){float z=(v[tris[i]].z+v[tris[i+1]].z+v[tris[i+2]].z)/3;if(z>RiverBlock.Near+.01f&&z<RiverBlock.Far-.01f)throw new Exception("Clipped mesh keeps road over the river: "+mesh.name);}
   }
   check(true,"No clipped road/sidewalk/wire triangle remains over the water");
   var ground=g.GetComponentsInChildren<MeshRenderer>(true).First(r=>r.name=="Nền thành phố");
   check(ground.sharedMaterial.shader.name=="PhoViet/City Ground","City ground skips the river cut through the stencil mask");
   var bridgeRenderers=sample.bridge.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("PV_RiverBridge_LOD",StringComparison.Ordinal)).ToArray();
   check(bridgeRenderers.All(r=>r.sharedMaterials.Any(m=>m.shader.name=="PhoViet/River Murk")&&r.sharedMaterials.First(m=>m.shader.name=="PhoViet/River Water").renderQueue>=3000),"Water is a translucent wavy surface over murky depth layers");
   check(bridgeRenderers.Length==3&&bridgeRenderers.All(r=>r.sharedMaterials.Any(m=>m.shader.name=="PhoViet/River Water")&&r.sharedMaterials.Any(m=>m.shader.name=="PhoViet/River Ground Mask")&&r.sharedMaterials.Any(m=>m.name=="PV_StreetAtlas")),"Bridge LODs use the street atlas, river water and ground mask materials");
   check(sample.bridge.GetComponentsInChildren<RiverBoat>(true).Length==4,"Each river carries four moored boats that bob on the water");
   check(g.daylight.streets.Count(s=>s.source!=null&&s.source.GetComponentsInParent<Transform>(true).Any(t=>t.name=="Cầu vượt sông • Blender"))==140,"Four bridge lamps per block are part of the day-night street lighting");
   // ---- Luật xuất hiện: khối xuất phát là phố, không 2 cầu liền nhau, không quá 2 khối phố liền nhau
   // Tắt xe tăng bắn (21.38) để đạn pháo không làm người chơi thua giữa đường thử cầu.
   g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;g.riversEnabled=true;g.tankBarrageEnabled=false;
   int bridgesSeen=0,runs=0;float maxGap=0;
   for(int run=0;run<25;run++)
   {
    g.RestartRide();runs++;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);
    var order=mainBlocks();
    check(!order[0].IsBridge&&!order[1].IsBridge,"Start blocks are streets (run "+run+")");
    foreach(int row in new[]{(int)get("leftRow"),(int)get("rightRow")})check(!g.roadRows[row].tiles[0].GetComponent<RiverBlock>().IsBridge,"Junction branch roads begin with a street block");
    float lastBridge=0;var seen=new HashSet<int>();var start=new HashSet<RiverBlock>{order[0],order[1]};
    for(int i=0;i<30*35&&g.State==PhoVietGame.RideState.Riding;i++)
    {
     tick(1/30f);
     if(i%15!=0)continue;
     var seq=mainBlocks();
     for(int k=1;k<seq.Length;k++)if(seq[k].IsBridge&&seq[k-1].IsBridge)throw new Exception("Two bridges in a row");
     for(int k=2;k<seq.Length;k++)if(!seq[k].IsBridge&&!seq[k-1].IsBridge&&!seq[k-2].IsBridge&&!start.Contains(seq[k])&&!start.Contains(seq[k-1])&&!start.Contains(seq[k-2]))throw new Exception("More than two street blocks in a row");
     foreach(var b in seq)if(b.IsBridge&&b.transform.localPosition.z<0&&seen.Add(Mathf.RoundToInt((g.Distance+b.transform.localPosition.z)/VietnamStreet.Length))){bridgesSeen++;maxGap=Mathf.Max(maxGap,g.Distance-lastBridge);lastBridge=g.Distance;}
    }
    check(g.State==PhoVietGame.RideState.Riding&&!g.RiderInRiver,"Riding straight in a bridge lane crosses every river safely (run "+run+")");
   }
   data.Add("bridgesCrossed="+bridgesSeen+" maxGap="+maxGap.ToString("F0")+" runs="+runs);
   check(bridgesSeen>=runs*3&&maxGap<=3*VietnamStreet.Length+20,"A river bridge comes up regularly: at most two street blocks between bridges ("+maxGap.ToString("F0")+" m)");
   check(g.BridgeBlocksAssigned>0&&g.StreetBlocksAssigned>0,"Both bridge and street blocks are generated");
   // ---- Vị trí trên cầu / ngoài cầu
   prepare(80);
   check(!g.OverRiver(new Vector3(1.55f,0,100))&&!g.OverRiver(new Vector3(-1.55f,0,120))&&g.OverRiver(new Vector3(4.65f,0,100))&&g.OverRiver(new Vector3(-4.65f,0,121))&&!g.OverRiver(new Vector3(4.65f,0,95))&&!g.OverRiver(new Vector3(4.65f,0,123)),"Only the two centre lanes are deck over the water; outer lanes end at the embankments");
   check(g.RiverZoneAt(new Vector3(0,0,100))&&!g.RiverZoneAt(new Vector3(0,0,60)),"River zone follows the bridge block");
   // ---- Người chơi đổi làn ra ngoài trên cầu: rơi xuống sông, kết thúc chuyến
   prepare(80);lane(2);g.StartRide();
   for(int i=0;i<600&&g.Distance<80+RiverBlock.Near+6;i++)tick(1/60f);
   check(g.State==PhoVietGame.RideState.Riding,"Rider reaches the deck in a bridge lane");
   g.MoveLane(1);float fellAt=-1;float minY=0;
   for(int i=0;i<60*5&&g.State!=PhoVietGame.RideState.GameOver;i++){tick(1/60f);if(g.RiderInRiver&&fellAt<0)fellAt=i/60f;minY=Mathf.Min(minY,g.rider.localPosition.y);}
   data.Add("laneChangeFall="+fellAt.ToString("F2")+"s minY="+minY.ToString("F2"));
   check(fellAt>=0&&fellAt<.6f,"Steering off the two-lane bridge drops the rider into the river");
   check(minY<RiverBlock.WaterY,"Rider falls below the water surface");
   check(g.State==PhoVietGame.RideState.GameOver&&g.RiderInRiver,"Falling into the river ends the ride");
   var aftermath=typeof(PhoVietGame).GetMethod("TickRiverAftermath");
   float yAtEnd=g.rider.localPosition.y;
   for(int i=0;i<60*8;i++)aftermath.Invoke(g,new object[]{1/60f});
   data.Add("riderYAtEndCard="+yAtEnd.ToString("F2")+" riderYAfter="+g.rider.localPosition.y.ToString("F2"));
   check(yAtEnd>g.rider.localPosition.y+1&&g.rider.localPosition.y<=RiverBlock.BedY+.75f,"After the end card the rider keeps sinking to the river bed instead of floating");
   check(g.RiverRipples>0&&g.RiverBubbles>0,"Sinking makes ripple rings and bubbles on the water surface");
   // Hình xe/người (BikeVisualAnimator) đi theo thân chìm, không bị nâng lên mặt đường.
   var bike=g.rider.GetComponentInChildren<BikeVisualAnimator>();
   if(bike!=null)typeof(BikeVisualAnimator).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)?.Invoke(bike,null);
   var skin=g.rider.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&r.GetComponent<ParticleSystem>()==null).OrderByDescending(r=>r.bounds.size.sqrMagnitude).First();
   data.Add("riderVisualMaxY="+skin.bounds.max.y.ToString("F2"));
   check(skin.bounds.max.y<RiverBlock.WaterY,"The sunk rider and bike are drawn under the water, not lifted to road level");
   // ---- Người chơi đi thẳng ở làn ngoài: rơi ở mép kè
   foreach(int outer in new[]{3,0})
   {
    prepare(80);lane(outer);g.StartRide();float edge=-1;
    for(int i=0;i<60*12&&g.State!=PhoVietGame.RideState.GameOver;i++){tick(1/60f);if(g.RiderInRiver&&edge<0)edge=g.Distance;}
    data.Add("outerLane"+outer+"Fall="+edge.ToString("F1"));
    check(edge>80+RiverBlock.Near-1&&edge<80+RiverBlock.Near+3&&g.State==PhoVietGame.RideState.GameOver,"Riding straight in outer lane "+outer+" falls off the embankment and ends the ride");
   }
   // ---- Hai làn cầu đều an toàn
   foreach(int safe in new[]{1,2})
   {
    prepare(80);lane(safe);g.StartRide();
    for(int i=0;i<60*12&&g.Distance<80+RiverBlock.Far+10;i++)tick(1/60f);
    check(g.State==PhoVietGame.RideState.Riding&&!g.RiderInRiver,"Bridge lane "+safe+" crosses the river safely");
   }
   // ---- Té trên cầu rồi trượt ra ngoài: rơi xuống sông
   prepare(80);lane(2);g.StartRide();
   for(int i=0;i<600&&g.Distance<80+RiverBlock.Near+8;i++)tick(1/60f);
   g.Crash();set("riderFallVelocity",new Vector3(9,0,6));
   for(int i=0;i<60*5&&g.State!=PhoVietGame.RideState.GameOver;i++)tick(1/60f);
   check(g.RiderInRiver&&g.State==PhoVietGame.RideState.GameOver&&g.rider.localPosition.y<RiverBlock.WaterY,"A crash that slides off the deck ends in the river");
   // ---- Xe thường cùng chiều ở làn ngoài tự nhập làn cầu
   prepare(80);lane(1);g.StartRide();
   var same=ordinary(3,55);check(same!=null&&same.lane==3,"Same-direction car spawned in the outer lane before the bridge");
   bool sameOnDeck=false;
   for(int i=0;i<60*10&&same.active;i++)
   {
    tick(1/60f);
    if(g.RiverZoneAt(same.model.localPosition)){sameOnDeck=true;if(Mathf.Abs(same.model.localPosition.x-1.55f)>.05f||same.riverFalling)throw new Exception("Merged car left bridge lane 2 over the water");}
   }
   check(sameOnDeck&&g.VehiclesFellInRiver==0&&g.BridgeMerges>=1,"Ordinary same-direction traffic merges into the bridge lane and crosses"+(sameOnDeck&&g.VehiclesFellInRiver==0?"":" [onDeck="+sameOnDeck+" fell="+g.VehiclesFellInRiver+" active="+same.active+" lane="+same.lane+" pos="+same.model.localPosition+" speed="+same.speed+" model="+same.model.name+" state="+g.State+"]"));
   // ---- Xe ngược chiều ở làn ngoài
   prepare(80);lane(2);g.StartRide();
   var oncoming=ordinary(0,190);check(oncoming!=null&&oncoming.lane==0,"Oncoming car spawned in the outer lane beyond the bridge");
   bool oncomingOnDeck=false;
   for(int i=0;i<60*10&&oncoming.active;i++)
   {
    tick(1/60f);
    if(g.RiverZoneAt(oncoming.model.localPosition)){oncomingOnDeck=true;if(Mathf.Abs(oncoming.model.localPosition.x+1.55f)>.05f||oncoming.riverFalling)throw new Exception("Merged oncoming car left bridge lane 1 over the water");}
   }
   check(oncomingOnDeck&&g.VehiclesFellInRiver==0,"Ordinary oncoming traffic merges into the bridge lane and crosses"+(oncomingOnDeck&&g.VehiclesFellInRiver==0?"":" [onDeck="+oncomingOnDeck+" fell="+g.VehiclesFellInRiver+" active="+oncoming.active+" lane="+oncoming.lane+" pos="+oncoming.model.localPosition+" speed="+oncoming.speed+" model="+oncoming.model.name+" state="+g.State+"]"));
   // ---- Xe xuất hiện ngay trên mặt sông ở làn ngoài được đặt vào làn cầu
   prepare(80);
   var spawned3=g.SpawnTraffic(3,105);var spawned0=g.SpawnTraffic(0,110);
   check(spawned3.lane==2&&Mathf.Abs(spawned3.model.localPosition.x-1.55f)<.01f&&spawned0.lane==1&&Mathf.Abs(spawned0.model.localPosition.x+1.55f)<.01f,"Traffic never spawns in an outer lane over the river");
   var nearBridge=typeof(PhoVietGame).GetMethod("OuterLaneNearBridge",F);
   check((bool)nearBridge.Invoke(g,new object[]{new Vector3(4.65f,0,70),3})&&!(bool)nearBridge.Invoke(g,new object[]{new Vector3(1.55f,0,70),2}),"Sudden lane changes toward an outer lane are suppressed near the bridge");
   // ---- Xe lạng/văng ra khỏi mặt cầu: rơi xuống sông, không còn va chạm, được thu hồi
   prepare(80);lane(1);g.StartRide();
   var weave=ordinary(2,108);weave.speed=weave.baseTrafficSpeed=10;
   tick(1/60f);
   weave.changeFromX=weave.model.localPosition.x;weave.targetX=PhoVietGame.LaneX[3];weave.changeProgress=0;weave.changeDuration=.5f;weave.changingLane=true;weave.lane=3;
   bool fell=false,splashed=false;float lowest=0;int before=g.VehiclesFellInRiver;
   for(int i=0;i<60*10&&weave.active;i++){tick(1/60f);fell|=weave.riverFalling;splashed|=weave.riverSplashed;lowest=Mathf.Min(lowest,weave.model.localPosition.y);}
   check(fell&&splashed&&lowest<RiverBlock.WaterY&&g.VehiclesFellInRiver==before+1,"A car swerving off the bridge falls and splashes into the river");
   check(lowest<RiverBlock.WaterY-2.5f,"A fallen car sinks deep under the water, not floating on it");
   check(!weave.active,"A sunk car is retired from the traffic pool");
   // Xe đang rơi khi chuyến kết thúc vẫn rơi tiếp và chìm (không đứng hình giữa không trung).
   prepare(80);lane(1);g.StartRide();
   var air=ordinary(2,106);tick(1/60f);
   air.changeFromX=air.model.localPosition.x;air.targetX=PhoVietGame.LaneX[3];air.changeProgress=0;air.changeDuration=.4f;air.changingLane=true;air.lane=3;
   for(int i=0;i<120&&!air.riverFalling;i++)tick(1/60f);
   float airY=air.model.localPosition.y;
   typeof(PhoVietGame).GetProperty("State").SetValue(g,PhoVietGame.RideState.GameOver);
   float airLowest=airY;for(int i=0;i<60*8&&air.active;i++){aftermath.Invoke(g,new object[]{1/60f});airLowest=Mathf.Min(airLowest,air.model.localPosition.y);}
   data.Add("airborneAtEnd="+airY.ToString("F2")+" lowestAfter="+airLowest.ToString("F2"));
   check(air.riverFalling==false&&airY>RiverBlock.WaterY&&airLowest<RiverBlock.WaterY-2.5f,"A vehicle still in the air when the ride ends keeps falling and sinks after the end card");
   prepare(80);lane(1);g.StartRide();
   var knocked=ordinary(2,104);tick(1/60f);
   knocked.knocked=true;knocked.knockAge=0;knocked.knockVelocity=new Vector3(7,0,8);
   bool knockedFell=false;for(int i=0;i<60*4&&knocked.active;i++){tick(1/60f);knockedFell|=knocked.riverFalling;}
   check(knockedFell,"A vehicle knocked sideways off the deck falls into the river");
   // Xe đang rơi không va chạm với người chơi/xe khác.
   prepare(80);var a=g.SpawnTraffic(2,105);var b2=g.SpawnTraffic(1,106);a.riverFalling=true;
   var contact=typeof(PhoVietGame).GetMethod("VehicleContact",F);var args=new object[]{a,b2,null,null,null};
   check(!(bool)contact.Invoke(g,args),"Falling vehicles take no part in collisions");
   return new{success=true,count=checks.Count,checks=checks.ToArray(),data=data.ToArray()};
  }
  finally
  {
   Time.timeScale=scale;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.jobsEnabled=jobs;
   g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();
  }
 }
}
