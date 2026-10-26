using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 // Cầu vượt sông (24/10/2026, huongdan.md 21.35): khối phố phía trước có 50% thành cầu, không bao giờ
 // quá 2 khối phố liền nhau (đi một lúc là gặp sông), không hai cầu liền nhau, khối xuất phát luôn là phố.
 // Cầu chỉ 2 làn giữa (|x| ≤ 3,1). Trên mặt sông, ai ra ngoài mặt cầu (|x| > 3,35) đều rơi xuống sông:
 // người chơi rơi là kết thúc chuyến; xe thường tự nhập làn cầu trước khi lên cầu.
 public partial class PhoVietGame
 {
  public bool riversEnabled=true;
  public int BridgeBlocksAssigned {get;private set;}
  public int StreetBlocksAssigned {get;private set;}
  public int VehiclesFellInRiver {get;private set;}
  public int BridgeMerges {get;private set;}
  public bool RiderInRiver {get;private set;}
  System.Random riverRng=new System.Random(1);
  readonly Dictionary<Transform,RiverBlock> riverBlocks=new Dictionary<Transform,RiverBlock>();
  bool riderRiverSplashed;float riderRiverTime;

  RiverBlock RiverOf(Transform tile)
  {
   if(tile==null)return null;
   if(!riverBlocks.TryGetValue(tile,out var block)){block=tile.GetComponent<RiverBlock>();riverBlocks[tile]=block;}
   return block;
  }
  void ResetRivers()
  {
   riverRng=new System.Random(RunSeed^0x5bd1e995);BridgeBlocksAssigned=0;StreetBlocksAssigned=0;
   VehiclesFellInRiver=0;BridgeMerges=0;RiderInRiver=false;riderRiverSplashed=false;riderRiverTime=0;
   RestoreBuildingsOverRiver();RiverNoise.Ensure();UpdateRiverMurk();
  }
  // Lớp nước đục chỉ vẽ khi có người chơi/xe dưới nước (21.54): lúc khác chúng nằm sau mặt nước đục ≥ 93%, không thấy.
  static readonly int MurkOn=Shader.PropertyToID("_RiverMurkOn");
  public bool RiverMurkVisible {get;private set;}
  void UpdateRiverMurk()
  {
   bool on=RiderInRiver;
   if(!on&&vehicles!=null)foreach(var v in vehicles)if(v.active&&v.riverFalling){on=true;break;}
   if(on!=RiverMurkVisible||!murkSet){RiverMurkVisible=on;murkSet=true;Shader.SetGlobalFloat(MurkOn,on?1:0);}
  }
  bool murkSet;
  // Gọi ở đầu TickRide, cuối ScrollRoutes (cả khi té/rơi sông vẫn cuộn phố) và cuối PrepareBranches (21.52).
  // Sông dài 50,4 m (21.46) chạy suốt giữa hai giao lộ nên các tòa nhà của đường ngang (dãy đường nhánh, sâu tới
  // 25,8 m tính từ mặt tiền) đứng một phần trên lòng sông ở hai đầu cầu (21.47). Mỗi khung: tòa nhà nào của khối
  // KHÁC có mặt bằng chồng lên dải mặt sông của một khối cầu đang bật thì ẩn; hết chồng thì hiện lại (trừ khi chính
  // khối của nó đang là cầu và đã ẩn nó). Mặt bằng tính một lần từ bounds mesh trong hệ khối nên dùng được cả khi đang ẩn.
  sealed class RiverBuilding{public GameObject go;public Transform tile;public RiverBlock own;public bool ownStreetOnly;public Vector2 min,max;public bool hidden;}
  List<RiverBuilding> riverBuildings;
  readonly Dictionary<Transform,List<RiverBuilding>> riverBuildingTiles=new Dictionary<Transform,List<RiverBuilding>>();
  public int BuildingsHiddenOverRiver {get;private set;}
  const float RiverHalfWidth=95f;
  void PrepareRiverBuildings()
  {
   riverBuildings=new List<RiverBuilding>();riverBuildingTiles.Clear();
   if(roadRows==null)return;
   foreach(var row in roadRows)foreach(var tile in row.tiles)
   {
    var own=RiverOf(tile);var list=new List<RiverBuilding>();riverBuildingTiles[tile]=list;
    foreach(Transform child in tile)
    {
     if(!child.name.StartsWith("Blender • ",System.StringComparison.Ordinal))continue;
     Vector2 lo=new Vector2(float.MaxValue,float.MaxValue),hi=new Vector2(float.MinValue,float.MinValue);bool any=false;
     foreach(var mf in child.GetComponentsInChildren<MeshFilter>(true))
     {
      if(mf.sharedMesh==null)continue;var b=mf.sharedMesh.bounds;
      for(int i=0;i<8;i++)
      {
       var c=new Vector3((i&1)==0?b.min.x:b.max.x,(i&2)==0?b.min.y:b.max.y,(i&4)==0?b.min.z:b.max.z);
       var l=tile.InverseTransformPoint(mf.transform.TransformPoint(c));lo=Vector2.Min(lo,new Vector2(l.x,l.z));hi=Vector2.Max(hi,new Vector2(l.x,l.z));any=true;
      }
     }
     if(!any)continue;
     var item=new RiverBuilding{go=child.gameObject,tile=tile,own=own,ownStreetOnly=own!=null&&System.Array.IndexOf(own.streetOnly,child.gameObject)>=0,min=lo,max=hi};
     riverBuildings.Add(item);list.Add(item);
    }
   }
  }
  void HideBuildingsOverRiver()
  {
   if(riverBuildings==null)PrepareRiverBuildings();
   foreach(var item in riverBuildings)item.hidden=false;
   if(riversEnabled&&roadRows!=null)
    foreach(var row in roadRows)foreach(var bridgeTile in row.tiles)
    {
     var block=RiverOf(bridgeTile);if(block==null||!block.IsBridge||!bridgeTile.gameObject.activeInHierarchy)continue;
     var toBridge=bridgeTile.worldToLocalMatrix;
     foreach(var pair in riverBuildingTiles)
     {
      var tile=pair.Key;if(tile==bridgeTile||pair.Value.Count==0)continue;
      if((tile.position-bridgeTile.position).sqrMagnitude>260f*260f)continue;
      var m=toBridge*tile.localToWorldMatrix;
      foreach(var item in pair.Value)
      {
       if(item.hidden)continue;
       float x0=float.MaxValue,x1=float.MinValue,z0=float.MaxValue,z1=float.MinValue;
       for(int k=0;k<4;k++)
       {
        var p=m.MultiplyPoint3x4(new Vector3((k&1)==0?item.min.x:item.max.x,0,(k&2)==0?item.min.y:item.max.y));
        x0=Mathf.Min(x0,p.x);x1=Mathf.Max(x1,p.x);z0=Mathf.Min(z0,p.z);z1=Mathf.Max(z1,p.z);
       }
       if(x1>-RiverHalfWidth&&x0<RiverHalfWidth&&z1>RiverBlock.Near+.3f&&z0<RiverBlock.Far-.3f)item.hidden=true;
      }
     }
    }
   int count=0;
   foreach(var item in riverBuildings)
   {
    bool ownHidden=item.ownStreetOnly&&item.own!=null&&item.own.IsBridge;
    bool want=!item.hidden&&!ownHidden;
    if(item.go.activeSelf!=want&&(item.hidden||!ownHidden))item.go.SetActive(want);
    if(item.hidden)count++;
   }
   BuildingsHiddenOverRiver=count;
  }
  void RestoreBuildingsOverRiver()
  {
   if(riverBuildings==null)return;
   foreach(var item in riverBuildings)
   {
    if(!item.hidden)continue;item.hidden=false;
    bool ownHidden=item.ownStreetOnly&&item.own!=null&&item.own.IsBridge;
    if(!ownHidden)item.go.SetActive(true);
   }
   BuildingsHiddenOverRiver=0;
  }
  // Khối thứ j của một dãy (tăng dần theo z); khối đầu tiên được ép là phố.
  void AssignRiverRow(List<Transform> tiles,int firstRiver)
  {
   RiverBlock behind=null;
   for(int j=0;j<tiles.Count;j++)
   {
    var block=RiverOf(tiles[j]);if(block==null)continue;
    if(j<firstRiver){block.SetBridge(false);block.streetRun=j<firstRiver-1?0:1;}
    else AssignRiverBlock(block,behind);
    behind=block;
   }
  }
  void AssignRiverBlock(RiverBlock block,RiverBlock behind)
  {
   bool bridge=false;
   if(riversEnabled&&block.bridge!=null)
   {
    if(behind==null)bridge=riverRng.NextDouble()<.5;
    else if(behind.IsBridge)bridge=false;
    else if(behind.streetRun>=2)bridge=true;
    else bridge=riverRng.NextDouble()<.5;
   }
   block.SetBridge(bridge);
   block.streetRun=bridge?0:(behind!=null&&!behind.IsBridge?behind.streetRun+1:1);
   if(bridge)BridgeBlocksAssigned++;else StreetBlocksAssigned++;
  }
  // Khối vừa được cuộn lên phía trước: khối liền sau nó là khối có z nhỏ hơn đúng 80 m.
  void AssignRecycledRiverBlock(Transform tile)
  {
   Transform behind=null;float target=tile.localPosition.z-VietnamStreet.Length;
   foreach(var b in blocks)if(b!=tile&&Mathf.Abs(b.localPosition.z-target)<6){behind=b;break;}
   var block=RiverOf(tile);if(block!=null)AssignRiverBlock(block,RiverOf(behind));
  }
  // Vị trí trong hệ game nằm trên dải mặt sông của một khối cầu (bất kể x).
  bool RiverZone(Vector3 p,out Vector3 local,out Transform tile)
  {
   local=Vector3.zero;tile=null;
   foreach(var b in blocks)
   {
    var block=RiverOf(b);if(block==null||!block.IsBridge||!b.gameObject.activeInHierarchy)continue;
    var l=b.InverseTransformPoint(transform.TransformPoint(p));
    if(l.z>RiverBlock.Near&&l.z<RiverBlock.Far){local=l;tile=b;return true;}
   }
   return false;
  }
  public bool RiverZoneAt(Vector3 p)=>RiverZone(p,out _,out _);
  public bool OverRiver(Vector3 p)=>RiverZone(p,out var l,out _)&&Mathf.Abs(l.x)>RiverBlock.DeckHalf;
  // Một xe/người (theo transform) đang ở ngoài mặt cầu trên lòng sông — hình không bám mặt đường.
  public bool InRiverWater(Transform actor)=>actor!=null&&OverRiver(transform.InverseTransformPoint(actor.position));
  // Khoảng cách tới đoạn sông kế tiếp theo hướng chạy (âm/0 nếu đang ở trên đoạn sông).
  bool BridgeAhead(Vector3 p,bool forward,float within)
  {
   foreach(var b in blocks)
   {
    var block=RiverOf(b);if(block==null||!block.IsBridge)continue;
    float tileZ=transform.InverseTransformPoint(b.position).z;
    float start=tileZ+RiverBlock.Near,end=tileZ+RiverBlock.Far;
    float distance=forward?start-p.z:p.z-end;
    if(distance<within&&(forward?p.z<end:p.z>start))return true;
   }
   return false;
  }
  // Xe thường ở làn ngoài nhập làn cầu trước khi tới mặt sông (xe lạng lách/văng ra thì không).
  bool MergeForBridge(TrafficVehicle v)
  {
   if(!riversEnabled||v.changingLane||v.isReckless||v.crossing||v.routeDetached||v.roadTurning||(v.lane!=0&&v.lane!=3))return false;
   bool forward=v.baseTrafficSpeed>0;
   if(!BridgeAhead(v.model.localPosition,forward,forward?38:58))return false;
   int lane=v.lane==0?1:2;
   v.changeFromX=v.model.localPosition.x;v.targetX=LaneX[lane];v.changeProgress=0;
   v.changeDuration=v.halfLength>3?.95f:v.halfWidth>.7f?.75f:.55f;v.changingLane=true;v.lane=lane;v.laneCooldown=2.5f;
   BridgeMerges++;return true;
  }
  bool OuterLaneNearBridge(Vector3 p,int lane)=>riversEnabled&&(lane==0||lane==3)&&(BridgeAhead(p,true,45)||BridgeAhead(p,false,60));
  // Xe xuất hiện ở làn ngoài trên mặt sông hoặc quá sát để kịp nhập làn được đặt thẳng vào làn cầu cùng chiều.
  void KeepSpawnOnBridge(TrafficVehicle v)
  {
   if(!riversEnabled||(v.lane!=0&&v.lane!=3))return;
   var p=v.model.localPosition;bool forward=v.baseTrafficSpeed>0;
   if(!RiverZoneAt(p)&&!BridgeAhead(p,forward,forward?14:30))return;
   v.lane=v.lane==0?1:2;p.x=LaneX[v.lane];v.model.localPosition=p;v.previousPosition=p;
  }

  // ------------------------------------------------------------ người chơi rơi xuống sông
  bool CheckRiderRiver()
  {
   if(State!=RideState.Riding||IsTurning||jumpHeight>.02f||!OverRiver(rider.localPosition))return false;
   BeginRiverFall();return true;
  }
  void BeginRiverFall()
  {
   CrashDuringPursuit=false;RiderFallComplete=false;CancelSwipe();CancelAssistedKick(false);ResetCrashPoliceNavigation();
   crashTurnRotation=Quaternion.identity;QueuedTurn=0;fallTime=0;settledTime=0;RiderFallTravel=0;
   float side=Mathf.Sign(rider.localPosition.x);
   rider.localRotation=Quaternion.Euler(0,steerYaw,0)*rider.localRotation;
   // Lật qua lan can rồi rơi gần chân cầu (không lao xa giữa sông/qua bờ kè).
   riderFallVelocity=new Vector3(side*(1.6f+Mathf.Min(Mathf.Abs(laneVelocity),6)*.3f),1.5f,Speed*.45f);
   riderFallEuler=rider.localEulerAngles;riderFallEuler.x=Mathf.DeltaAngle(0,riderFallEuler.x);riderFallEuler.z=Mathf.DeltaAngle(0,riderFallEuler.z);
   riderFallSpin=new Vector3(55,side*20,-side*95);fallTargetRoll=-side*78;fallTargetPitch=40;
   steerYaw=0;ClearLaneMotion();ResetJump();CancelBoost();
   State=RideState.Falling;riderBody.knocked=true;EnterRiver();
   Speed=riderFallVelocity.z;engine?.Stop();navigationAudio?.Stop();kickTime=0;
   if(sound)fx?.PlayOneShot(crashClip,.45f);SaveRideRecord();
  }
  void EnterRiver()
  {
   RiderInRiver=true;riderRiverSplashed=false;riderRiverTime=0;riderRippleTimer=0;riderBubbleTimer=0;UpdateRiverMurk();
   RiverZone(rider.localPosition,out _,out riderRiverTile);
   notice="RƠI XUỐNG SÔNG · CHUYẾN ĐI KẾT THÚC";noticeTimer=4;
  }
  // Rơi tự do tới mặt nước, bắn nước, rồi chìm dần xuống đáy sông (không nổi lềnh bềnh).
  // Chạy cả khi đã hiện thẻ kết thúc (TickRiverAftermath) để không bị đứng hình giữa chừng.
  void SinkRider(float dt,bool scrolling)
  {
   Vector3 previous=rider.localPosition;
   if(!riderRiverSplashed)riderFallVelocity.y-=9.81f*dt;
   else
   {
    riderFallVelocity.x=Mathf.MoveTowards(riderFallVelocity.x,0,dt*3.5f);riderFallVelocity.z=Mathf.MoveTowards(riderFallVelocity.z,0,dt*3.5f);
    riderFallVelocity.y=Mathf.MoveTowards(riderFallVelocity.y,-SinkSpeed,dt*(riderFallVelocity.y<-SinkSpeed?9:1.6f));
   }
   Vector3 p=previous+(riderFallVelocity-(scrolling?Vector3.forward*Speed:Vector3.zero))*dt;
   if(!riderRiverSplashed&&p.y<=RiverBlock.WaterY)
   {
    riderRiverSplashed=true;Splash(riderRiverTile,p,1.15f);
    riderFallVelocity=new Vector3(riderFallVelocity.x*.35f,riderFallVelocity.y*.18f,riderFallVelocity.z*.35f);
    if(sound)fx?.PlayOneShot(crashClip,.25f);
   }
   if(p.y<=RiverBlock.BedY+.7f){p.y=RiverBlock.BedY+.7f;riderFallVelocity=Vector3.zero;}
   if(KeepInRiver(riderRiverTile,ref p,1.1f))riderFallVelocity.z=0;
   riderFallEuler+=riderFallSpin*dt;riderFallSpin*=Mathf.Exp(-dt*(riderRiverSplashed?2.5f:.4f));
   riderFallEuler.z=Mathf.MoveTowards(riderFallEuler.z,fallTargetRoll,dt*120);
   if(riderRiverSplashed)riderFallEuler.x=Mathf.MoveTowards(riderFallEuler.x,fallTargetPitch,dt*25);
   rider.localRotation=Quaternion.Euler(riderFallEuler);
   RiderFallTravel+=Flat(p-previous).magnitude;rider.localPosition=p;
   if(riderRiverSplashed)SinkingSurface(riderRiverTile,p,ref riderRippleTimer,ref riderBubbleTimer,dt,1.1f);
  }
  void TickRiderRiver(float dt)
  {
   Speed=Mathf.Max(0,riderFallVelocity.z);float scroll=Speed*dt;Distance+=scroll;ScrollRoutes(scroll);
   SinkRider(dt,true);
   TickTrafficMotion(dt);RetireTraffic(false);
   if(!IsTurning)EndRouteFrame(dt);
   if(riderRiverSplashed)
   {
    riderRiverTime+=dt;
    if(riderRiverTime>=1.6f&&State==RideState.Falling)
    {RiderFallComplete=true;State=RideState.GameOver;Speed=0;SaveRideRecord();siren?.Stop();}
   }
  }
  // Sau khi chuyến kết thúc: người/xe đang rơi hoặc đang chìm vẫn tiếp tục cho tới đáy (phố đứng yên).
  public void TickRiverAftermath(float dt)
  {
   UpdateRiverMurk();
   if(dt<=0||State!=RideState.GameOver)return;
   Speed=0;
   if(RiderInRiver&&rider.localPosition.y>RiverBlock.BedY+.71f)SinkRider(dt,false);
   foreach(var v in vehicles)if(v.active&&v.riverFalling)
   {
    TickVehicleRiverFall(v,dt,false);
    if(v.riverAge>9||v.model.localPosition.y<=RiverBlock.BedY+.41f&&v.riverAge>3){v.active=false;v.model.gameObject.SetActive(false);v.riverFalling=false;}
   }
  }
  const float SinkSpeed=.75f;
  // Mép nước phía bờ gần (hệ game) của khúc sông người chơi rơi xuống, cộng 1 m: camera không lùi sau kè.
  float RiverCameraMinZ()
  {
   if(riderRiverTile==null)return float.NegativeInfinity;
   return transform.InverseTransformPoint(riderRiverTile.TransformPoint(new Vector3(0,0,RiverBlock.Near))).z+1f;
  }

  // ------------------------------------------------------------ xe rơi xuống sông
  bool RiverCandidate(TrafficVehicle v)=>v.active&&!v.riverFalling&&!v.crossing&&!v.routeDetached&&!v.roadTurning&&!v.wallCrashed&&v.recovery==null&&v.model.localPosition.y<.6f;
  void BeginVehicleRiverFall(TrafficVehicle v,Vector3 previous,float dt)
  {
   Vector3 velocity=dt>0?(v.model.localPosition-previous)/dt+Vector3.forward*Speed:v.motionVelocity;
   float side=Mathf.Sign(v.model.localPosition.x);
   v.riverFalling=true;v.riverAge=0;v.riverSplashed=false;v.riverRipple=0;v.riverBubble=0;UpdateRiverMurk();
   v.riverVelocity=new Vector3(Mathf.Clamp(velocity.x,-4,4)+side*.8f,Mathf.Max(velocity.y,.5f),velocity.z);
   RiverZone(v.model.localPosition,out _,out v.riverTile);
   v.riverSpin=new Vector3(Mathf.Clamp(velocity.z*2.2f,-40,40),0,-side*(v.halfWidth>.7f?38:90));
   v.changingLane=false;v.brakeRemaining=0;if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(false);
   VehiclesFellInRiver++;
  }
  void TickVehicleRiverFall(TrafficVehicle v,float dt,bool scrolling=true)
  {
   v.riverAge+=dt;var p=v.model.localPosition;
   if(!v.riverSplashed)v.riverVelocity.y-=9.81f*dt;
   else
   {
    // Ô tô chìm nhanh hơn xe máy; vận tốc ngang/dọc tắt dần trong nước.
    float sink=v.halfWidth>.7f?1.15f:SinkSpeed;
    v.riverVelocity.x=Mathf.MoveTowards(v.riverVelocity.x,0,dt*3);v.riverVelocity.z=Mathf.MoveTowards(v.riverVelocity.z,0,dt*3);
    v.riverVelocity.y=Mathf.MoveTowards(v.riverVelocity.y,-sink,dt*(v.riverVelocity.y<-sink?8:1.8f));
   }
   p+=(v.riverVelocity-(scrolling?Vector3.forward*Speed:Vector3.zero))*dt;
   if(!v.riverSplashed&&p.y<=RiverBlock.WaterY+.2f)
   {
    v.riverSplashed=true;Splash(v.riverTile,p,v.halfWidth>.7f?1.6f:1f);
    v.riverVelocity=new Vector3(v.riverVelocity.x*.3f,v.riverVelocity.y*.15f,v.riverVelocity.z*.3f);
   }
   if(p.y<RiverBlock.BedY+.4f){p.y=RiverBlock.BedY+.4f;v.riverVelocity=Vector3.zero;}
   if(KeepInRiver(v.riverTile,ref p,v.halfLength*.6f))v.riverVelocity.z=0;
   v.model.localPosition=p;
   v.model.localRotation=Quaternion.Euler(v.riverSpin*dt*(v.riverSplashed?.2f:1))*v.model.localRotation;
   if(v.riverSplashed)SinkingSurface(v.riverTile,p,ref v.riverRipple,ref v.riverBubble,dt,v.halfWidth>.7f?1.5f:1f);
  }

  // ------------------------------------------------------------ hiệu ứng mặt nước
  // Mỗi khối cầu có riêng bọt bắn, vòng gợn sóng và bọt khí (mô phỏng trong hệ khối nên trôi cùng phố).
  sealed class RiverFx {public ParticleSystem splash,rings,bubbles;}
  readonly Dictionary<Transform,RiverFx> riverFx=new Dictionary<Transform,RiverFx>();
  Material splashMaterial,ringMaterial;
  float riderRippleTimer,riderBubbleTimer;Transform riderRiverTile;
  public int RiverRipples {get;private set;}
  public int RiverBubbles {get;private set;}
  // Kè hai bờ chặn vật đang rơi/chìm: giữ trong lòng sông của khối đã rơi xuống (không trôi vào đất bờ).
  bool KeepInRiver(Transform tile,ref Vector3 p,float margin)
  {
   if(tile==null)return false;
   var local=tile.InverseTransformPoint(transform.TransformPoint(p));
   float z=Mathf.Clamp(local.z,RiverBlock.Near+margin,RiverBlock.Far-margin);
   if(Mathf.Approximately(z,local.z))return false;
   local.z=z;p=transform.InverseTransformPoint(tile.TransformPoint(local));return true;
  }
  void SinkingSurface(Transform tile,Vector3 p,ref float ripple,ref float bubble,float dt,float size)
  {
   float depth=RiverBlock.WaterY-p.y;
   ripple-=dt;bubble-=dt;
   if(ripple<=0&&depth<2.6f){ripple=.38f;EmitRiver(tile,p,size,1);}
   if(bubble<=0&&depth<4.2f){bubble=.12f;EmitRiver(tile,p,size,2);}
  }
  void Splash(Transform tile,Vector3 position,float size){EmitRiver(tile,position,size,0);EmitRiver(tile,position,size,1);}
  void EmitRiver(Transform tile,Vector3 position,float size,int kind)
  {
   if(tile==null&&!RiverZone(position,out _,out tile))return;
   var fx=RiverFxFor(tile);var system=kind==0?fx.splash:kind==1?fx.rings:fx.bubbles;
   var local=tile.InverseTransformPoint(transform.TransformPoint(new Vector3(position.x,RiverBlock.WaterY+.06f,position.z)));
   var e=new ParticleSystem.EmitParams();
   if(kind==0)
   {
    for(int i=0;i<Mathf.RoundToInt(48*size);i++)
    {
     var dir=new Vector3(UnityEngine.Random.Range(-.6f,.6f),1,UnityEngine.Random.Range(-.6f,.6f)).normalized;
     e.position=local+new Vector3(UnityEngine.Random.Range(-.4f,.4f),0,UnityEngine.Random.Range(-.4f,.4f))*size;
     e.velocity=dir*UnityEngine.Random.Range(2.5f,6.5f)*Mathf.Sqrt(size);e.startSize=UnityEngine.Random.Range(.16f,.45f)*size;e.startLifetime=UnityEngine.Random.Range(.55f,1.1f);
     system.Emit(e,1);
    }
   }
   else if(kind==1){e.position=local;e.velocity=Vector3.zero;e.startSize=.9f*size;e.startLifetime=1.7f;system.Emit(e,1);RiverRipples++;}
   else
   {
    for(int i=0;i<3;i++)
    {
     e.position=local+new Vector3(UnityEngine.Random.Range(-.7f,.7f),0,UnityEngine.Random.Range(-.7f,.7f))*size;
     e.velocity=new Vector3(0,UnityEngine.Random.Range(.15f,.45f),0);e.startSize=UnityEngine.Random.Range(.07f,.18f);e.startLifetime=UnityEngine.Random.Range(.35f,.8f);
     system.Emit(e,1);
    }
    RiverBubbles++;
   }
  }
  RiverFx RiverFxFor(Transform tile)
  {
   if(riverFx.TryGetValue(tile,out var fx)&&fx.splash!=null)return fx;
   if(splashMaterial==null)
   {
    var shader=Shader.Find("PhoViet/Soft Particle");if(shader==null)shader=Shader.Find("Sprites/Default");
    splashMaterial=new Material(shader){name="PV_RiverSplash",mainTexture=Dot(),renderQueue=3100};
    ringMaterial=new Material(shader){name="PV_RiverRipple",mainTexture=Ring(),renderQueue=3100};
   }
   fx=new RiverFx{splash=RiverParticles("Bọt nước bắn",tile,splashMaterial,1.1f,false),rings=RiverParticles("Vòng gợn sóng",tile,ringMaterial,0,true),bubbles=RiverParticles("Bọt khí sủi",tile,splashMaterial,-.15f,false)};
   riverFx[tile]=fx;return fx;
  }
  static ParticleSystem RiverParticles(string name,Transform parent,Material material,float gravity,bool ring)
  {
   var go=new GameObject(name);go.transform.SetParent(parent,false);
   var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.playOnAwake=false;main.loop=false;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.maxParticles=ring?40:240;
   main.gravityModifier=gravity;main.startColor=ring?new Color(.95f,1,1,.6f):new Color(.93f,.98f,1,.85f);
   var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
   var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();
   gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(ring?.75f:.9f,0),new GradientAlphaKey(0,1)});fade.color=gradient;
   if(ring){var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,.6f,1,5.2f));}
   var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ring?ParticleSystemRenderMode.HorizontalBillboard:ParticleSystemRenderMode.Billboard;
   renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
   ps.Play();return ps;
  }
  static Texture2D Dot()
  {
   var t=new Texture2D(32,32,TextureFormat.RGBA32,false){name="PV_RiverSplashDot",wrapMode=TextureWrapMode.Clamp};
   for(int y=0;y<32;y++)for(int x=0;x<32;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(16,16))/16;t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(1-d*d)));}
   t.Apply();return t;
  }
  static Texture2D Ring()
  {
   var t=new Texture2D(64,64,TextureFormat.RGBA32,false){name="PV_RiverRippleRing",wrapMode=TextureWrapMode.Clamp};
   for(int y=0;y<64;y++)for(int x=0;x<64;x++)
   {
    float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32))/32;
    float a=Mathf.Exp(-Mathf.Pow((d-.82f)/.07f,2))+.35f*Mathf.Exp(-Mathf.Pow((d-.62f)/.05f,2));
    t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(a)*(d<1?1:0)));
   }
   t.Apply();return t;
  }
 }
}
