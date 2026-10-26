using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Cầu vượt sông (24/10/2026, huongdan.md 21.35; dài 50,4 m + nước gương 21.46): luật xuất hiện, cầu 2 làn giữa, rơi xuống sông cho người chơi và xe.
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
   check(bridgeRenderers.All(r=>r.sharedMaterials.Any(m=>m.shader.name=="PhoViet/River Murk")&&r.sharedMaterials.First(m=>m.shader.name=="PhoViet/River Water Mirror").renderQueue>=3000),"Water is a translucent mirror surface over murky depth layers");
   check(bridgeRenderers.Length==3&&bridgeRenderers.All(r=>r.sharedMaterials.Any(m=>m.shader.name=="PhoViet/River Water Mirror")&&r.sharedMaterials.Any(m=>m.shader.name=="PhoViet/River Ground Mask")&&r.sharedMaterials.Any(m=>m.name=="PV_StreetAtlas")),"Bridge LODs use the street atlas, river water and ground mask materials");
   check(sample.bridge.GetComponentsInChildren<RiverBoat>(true).Length==4,"Each river carries four moored boats that bob on the water");
   check(g.daylight.streets.Count(s=>s.source!=null&&s.source.GetComponentsInParent<Transform>(true).Any(t=>t.name=="Cầu vượt sông • Blender"))==280,"Eight bridge lamps per block are part of the day-night street lighting");
   // ---- Cầu dài gấp đôi (25/10/2026): 50,4 m, nằm giữa vạch mũi tên của hai giao lộ
   check(Mathf.Abs(RiverBlock.Far-RiverBlock.Near-50.4f)<.01f,"The river under the bridge is 50.4 m long, double the first 25.2 m");
   var arrows=sample.transform.Cast<Transform>().First(t=>t.name.StartsWith("Vạch mũi tên",StringComparison.Ordinal)).GetComponent<Renderer>().bounds;
   check(RiverBlock.Near>=4.6f&&sample.transform.InverseTransformPoint(arrows.min).z>=RiverBlock.Far,"The long river stays between the previous and next junction arrow markings");
   var waterMat=bridgeRenderers[0].sharedMaterials.First(m=>m.shader.name=="PhoViet/River Water Mirror");
   check(Mathf.Abs(waterMat.GetVector("_Banks").x-RiverBlock.Near)<.01f&&Mathf.Abs(waterMat.GetVector("_Banks").y-RiverBlock.Far)<.01f&&Mathf.Abs(waterMat.GetVector("_Piers").x-(RiverBlock.Near+16.8f))<.01f&&Mathf.Abs(waterMat.GetVector("_Piers").y-(RiverBlock.Far-16.8f))<.01f,"Water shadows use the long river banks and its two piers");
   // Dãy nhà ống ven sông (21.48): lấp bờ ngắn giữa đường ngang và kè, không lấn mặt nước, có vật cản.
   var houses=sample.bridge.GetComponent<StreetObstacleSet>();
   check(houses!=null&&houses.obstacles.Length>=16&&houses.obstacles.All(o=>o.kind=="Nhà ven sông"&&o.bounds.size.y>=18),"Each bridge block carries a row of 6–9 storey riverside apartment blocks with collision boxes ("+(houses!=null?houses.obstacles.Length:0)+")");
   // Căn hộ là 24 mô hình riêng, mỗi khối LODGroup riêng (21.54).
   var aptRoot=sample.bridge.transform.Find("Căn hộ ven sông • Blender");
   var aptLods=aptRoot!=null?aptRoot.GetComponentsInChildren<LODGroup>(true):new LODGroup[0];
   var aptRenderers=aptRoot!=null?aptRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("PV_RiverApt",StringComparison.Ordinal)).ToArray():new MeshRenderer[0];
   var aptProxies=aptRoot!=null?aptRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("Bóng • ",StringComparison.Ordinal)).ToArray():new MeshRenderer[0];
   check(aptLods.Length==24&&aptLods.All(l=>l.lodCount==3&&l.GetLODs()[0].screenRelativeTransitionHeight>=.25f)&&aptRenderers.Length==72,"Each riverside apartment block has its own three-level LOD group ("+aptLods.Length+" groups)");
   check(bridgeRenderers.All(r=>!r.sharedMaterials.Any(m=>m!=null&&m.name=="PV_StreetGlass")),"The bridge/water model itself no longer carries the apartments");
   check(aptRenderers.All(r=>r.sharedMaterials.Any(m=>m!=null&&m.name=="PV_StreetGlass")&&r.gameObject.layer==RiverReflection.ReflectLayer),"Riverside apartments use the street's transparent glass and appear in the river reflection");
   // Giảm tải 21.54: bóng đổ do bản LOD2 (căn hộ) / LOD1 (cầu) vẽ riêng; LOD chi tiết hơn không vẽ vào shadow map.
   check(aptProxies.Length==24&&aptProxies.All(r=>r.shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly&&r.name.EndsWith("_LOD2",StringComparison.Ordinal))
     &&aptRenderers.Where(r=>!r.name.EndsWith("_LOD2",StringComparison.Ordinal)).All(r=>r.shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.Off),"Apartment shadows come from a LOD2 shadow-only proxy, detailed LODs skip the shadow map");
   var deckProxy=sample.bridge.transform.Cast<Transform>().FirstOrDefault(t=>t.name=="Bóng • PV_RiverBridge_LOD1");
   check(deckProxy!=null&&deckProxy.GetComponent<MeshRenderer>().shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly&&bridgeRenderers.First(r=>r.name.EndsWith("_LOD0",StringComparison.Ordinal)).shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.Off,"Bridge deck shadows come from a LOD1 shadow-only proxy");
   check(Shader.GetGlobalTexture("_RiverNoise") is Texture2D nz&&nz.width==256&&nz.wrapMode==TextureWrapMode.Repeat&&g.rideCamera.GetComponent<RiverReflection>().maxLod==1,"Water ripples read a baked tiling noise texture; the reflection camera renders LOD1+");
   var roomMat=aptRenderers.Length>0?aptRenderers[0].sharedMaterials.FirstOrDefault(m=>m!=null&&m.shader.name=="PhoViet/Apartment Room"):null;
   check(roomMat!=null&&roomMat.GetTexture("_MainTex")!=null&&aptRenderers.All(r=>r.sharedMaterials.Contains(roomMat)),"Apartment rooms behind the glass have a night-lit material in every LOD");
   g.daylight.SetCycleTime(g.daylight.CycleSeconds*.5f);float nightLights=Shader.GetGlobalFloat("_PVNightLights");g.daylight.SetCycleTime(0);float dayLights=Shader.GetGlobalFloat("_PVNightLights");
   check(nightLights>.95f&&dayLights<.05f,"Apartment rooms glow at night and switch off by day (_PVNightLights "+nightLights.ToString("F2")+"/"+dayLights.ToString("F2")+")");
   check(houses.obstacles.All(o=>Mathf.Abs(o.bounds.center.x)-o.bounds.extents.x>=11.2f&&(o.bounds.max.z<=RiverBlock.Near-.25f&&o.bounds.min.z>=0||o.bounds.min.z>=RiverBlock.Far+.25f&&o.bounds.max.z<=58.8f)),"Riverside houses stand on the short banks, clear of the main sidewalk and never over the water");
   check(houses.obstacles.Where(o=>o.bounds.center.z<30).Sum(o=>o.bounds.size.x)>=2*(95-11.4f)-.5f&&houses.obstacles.Where(o=>o.bounds.center.z>30).Sum(o=>o.bounds.size.x)>=2*(95-11.4f)-.5f,"Houses fill both banks from the main sidewalk to the end of the river, no empty land");
   var reflection=g.rideCamera.GetComponent<RiverReflection>();
   check(reflection!=null&&reflection.game==g&&reflection.enabled,"The ride camera renders the river mirror reflection");
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
   check(!g.OverRiver(new Vector3(1.55f,0,100))&&!g.OverRiver(new Vector3(-1.55f,0,120))&&g.OverRiver(new Vector3(4.65f,0,100))&&g.OverRiver(new Vector3(-4.65f,0,121))&&!g.OverRiver(new Vector3(4.65f,0,83))&&!g.OverRiver(new Vector3(4.65f,0,137)),"Only the two centre lanes are deck over the water; outer lanes end at the embankments");
   check(g.RiverZoneAt(new Vector3(0,0,100))&&!g.RiverZoneAt(new Vector3(0,0,60)),"River zone follows the bridge block");
   // Ảnh phản chiếu chỉ render khi có sông trong khung nhìn.
   Func<float> renderFlag=()=>{var rt=RenderTexture.GetTemporary(320,180,16);var old=g.rideCamera.targetTexture;g.rideCamera.targetTexture=rt;g.rideCamera.Render();g.rideCamera.targetTexture=old;RenderTexture.ReleaseTemporary(rt);return Shader.GetGlobalFloat("_RiverReflectionOn");};
   typeof(PhoVietGame).GetMethod("PositionRideCamera",F).Invoke(g,new object[]{true,0f});
   check(renderFlag()>.5f&&Shader.GetGlobalTexture("_RiverReflectionTex")!=null,"A river in view gets a live mirror reflection");
   foreach(var t in g.roadRows.SelectMany(r=>r.tiles))t.GetComponent<RiverBlock>().SetBridge(false);
   check(renderFlag()<.5f,"No reflection is rendered when no river is in view");
   check(!g.RiverMurkVisible&&Shader.GetGlobalFloat("_RiverMurkOn")<.5f,"Murky depth layers are skipped while nothing is under water (21.54)");
   prepare(80);
   // ---- Người chơi đổi làn ra ngoài trên cầu: rơi xuống sông, kết thúc chuyến
   prepare(80);lane(2);g.StartRide();
   for(int i=0;i<600&&g.Distance<80+RiverBlock.Near+6;i++)tick(1/60f);
   check(g.State==PhoVietGame.RideState.Riding,"Rider reaches the deck in a bridge lane");
   g.MoveLane(1);float fellAt=-1;float minY=0;
   for(int i=0;i<60*5&&g.State!=PhoVietGame.RideState.GameOver;i++){tick(1/60f);if(g.RiderInRiver&&fellAt<0)fellAt=i/60f;minY=Mathf.Min(minY,g.rider.localPosition.y);}
   data.Add("laneChangeFall="+fellAt.ToString("F2")+"s minY="+minY.ToString("F2"));
   check(fellAt>=0&&fellAt<.6f,"Steering off the two-lane bridge drops the rider into the river");
   check(minY<RiverBlock.WaterY,"Rider falls below the water surface");
   check(g.RiverMurkVisible&&Shader.GetGlobalFloat("_RiverMurkOn")>.5f,"Murky depth layers turn on while the rider sinks (21.54)");
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
   var same=ordinary(3,50);check(same!=null&&same.lane==3,"Same-direction car spawned in the outer lane before the bridge");
   // Đủ nhanh để tới cầu (bắt đầu 84,8) trước khi người chơi vượt (xe máy chậm 7 m/s sẽ bị vượt trước cầu).
   same.speed=same.baseTrafficSpeed=14;
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
   // ---- Nhà đường ngang trên lòng sông dài (21.47): chạy tự nhiên tới cầu, không tòa nhà nào của khối khác đứng trên mặt sông
   g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;g.riversEnabled=true;g.tankBarrageEnabled=false;
   g.RestartRide();g.StartRide();Transform reached=null;
   for(int i=0;i<60*120&&g.State==PhoVietGame.RideState.Riding;i++)
   {
    tick(1/60f);set("spawnTimer",100000f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}
    reached=g.blocks.Where(t=>t.GetComponent<RiverBlock>().IsBridge&&t.localPosition.z+RiverBlock.Near-g.rider.localPosition.z<14&&t.localPosition.z+RiverBlock.Far>g.rider.localPosition.z).OrderBy(t=>t.localPosition.z).FirstOrDefault();
    if(reached!=null)break;
   }
   check(reached!=null,"A natural ride reaches a long river bridge");
   // Đếm tòa nhà đang hiện của khối khác đứng trên mặt nước của BẤT KỲ khối cầu đang bật nào.
   string floatExample="";
   Func<int> floating=()=>
   {
    int n=0;floatExample="";var bridges=g.roadRows.SelectMany(r=>r.tiles).Where(t=>t.gameObject.activeInHierarchy&&t.GetComponent<RiverBlock>().IsBridge).ToArray();
    foreach(var t in g.roadRows.SelectMany(r=>r.tiles))foreach(Transform child in t)
    {
     if(!child.name.StartsWith("Blender • ",StringComparison.Ordinal)||!child.gameObject.activeInHierarchy)continue;
     var rs=child.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;var bb=rs[0].bounds;foreach(var r in rs)bb.Encapsulate(r.bounds);
     foreach(var br in bridges)
     {
      if(br==t)continue;float x0=float.MaxValue,x1=float.MinValue,z0=float.MaxValue,z1=float.MinValue;
      for(int k=0;k<4;k++){var p=br.InverseTransformPoint(new Vector3((k&1)==0?bb.min.x:bb.max.x,0,(k&2)==0?bb.min.z:bb.max.z));x0=Mathf.Min(x0,p.x);x1=Mathf.Max(x1,p.x);z0=Mathf.Min(z0,p.z);z1=Mathf.Max(z1,p.z);}
      if(x1>-95&&x0<95&&z1>RiverBlock.Near+.5f&&z0<RiverBlock.Far-.5f){n++;if(floatExample=="")floatExample=t.parent.name+"/"+t.name+"/"+child.name+" over "+br.parent.name+"/"+br.name+" x["+x0.ToString("F0")+","+x1.ToString("F0")+"] z["+z0.ToString("F1")+","+z1.ToString("F1")+"] state="+g.State+" dist="+g.Distance.ToString("F0");break;}
     }
    }
    return n;
   };
   int overlapping=floating();
   data.Add("buildingsHiddenOverRiver="+g.BuildingsHiddenOverRiver+" overlapping="+overlapping);
   check(g.BuildingsHiddenOverRiver>0&&overlapping==0,"Cross-street buildings standing over the long river are hidden, none float over the water");
   // ---- 21.52: va chạm/té ngay trước cầu, phố vẫn cuộn khi xe trượt và khi rơi sông chìm dần — nhà không được hiện lại trên sông
   int worstCrash=0,crashFrames=0;
   g.Crash();set("riderFallVelocity",new Vector3(0,0,14));
   for(int i=0;i<60*6&&g.State==PhoVietGame.RideState.Falling;i++){tick(1/60f);worstCrash=Mathf.Max(worstCrash,floating());crashFrames++;}
   var aftermathTick=typeof(PhoVietGame).GetMethod("TickRiverAftermath");
   for(int i=0;i<60*3;i++){aftermathTick.Invoke(g,new object[]{1/60f});if(i%10==0)worstCrash=Mathf.Max(worstCrash,floating());}
   data.Add("crashFrames="+crashFrames+" worstFloatingDuringCrash="+worstCrash+" state="+g.State);
   check(crashFrames>10&&worstCrash==0,"While the rider crashes and slides before a bridge, no cross-street building reappears over the river");
   // Rơi xuống sông: đi làn ngoài thẳng vào mép kè, chìm dần, phố còn cuộn.
   int worstSink=0;bool sank=false;
   for(int attempt=0;attempt<6&&!sank;attempt++)
   {
    g.RestartRide();g.StartRide();
    for(int i=0;i<60*120&&g.State==PhoVietGame.RideState.Riding;i++)
    {
     tick(1/60f);set("spawnTimer",100000f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}
     var near=g.blocks.Where(t=>t.GetComponent<RiverBlock>().IsBridge&&t.localPosition.z+RiverBlock.Near-g.rider.localPosition.z<30&&t.localPosition.z+RiverBlock.Near>g.rider.localPosition.z).FirstOrDefault();
     if(near!=null&&g.CurrentLane!=3)lane(3);
    }
    sank=g.RiderInRiver;
    for(int i=0;i<60*4&&g.State!=PhoVietGame.RideState.GameOver;i++){tick(1/60f);if(i%5==0)worstSink=Mathf.Max(worstSink,floating());}
    for(int i=0;i<60*4;i++){aftermathTick.Invoke(g,new object[]{1/60f});if(i%10==0)worstSink=Mathf.Max(worstSink,floating());}
   }
   data.Add("riverFallSank="+sank+" worstFloatingDuringSink="+worstSink);
   check(sank&&worstSink==0,"While the rider falls and sinks in the river, no cross-street building floats over the water"+(worstSink>0?" ["+floatExample+"]":""));
   g.RestartRide();
   int afterRestart=floating();check(afterRestart==0,"After a restart (ready screen, before riding) no cross-street building stands over a river"+(afterRestart>0?" ["+afterRestart+": "+floatExample+"]":""));
   return new{success=true,count=checks.Count,checks=checks.ToArray(),data=data.ToArray()};
  }
  finally
  {
   Time.timeScale=scale;g.riversEnabled=rivers;g.tankBarrageEnabled=barrage;g.recklessTrafficEnabled=danger;g.trafficSurprises=surprises;g.turnExitTrafficEnabled=exits;g.jobsEnabled=jobs;
   g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();
  }
 }
}
