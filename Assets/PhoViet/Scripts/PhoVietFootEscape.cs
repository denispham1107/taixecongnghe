using System;
using UnityEngine;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  public GameObject footCharacterPrefab;
  public AnimationClip footRunClip;
  public Material footCharacterMaterial;
  public const float ForcedVehicleExitDelay=3f,FootVehicleStopTime=8f;
  public const float FootEscapeSeconds=10f;
  const float FootSpeed=5.6f,FootExitFlight=.55f,FootDoorTime=.78f;
  public bool PlayerOnFoot=>State==RideState.OnFoot;
  public Transform FootCharacter=>footActor;
  public string TakenVehicleModel=>claimedVehicle?.model.GetComponent<MeshyCarVariants>()?.ModelAsset??claimedVehicle?.model.name;
  public partial class TrafficVehicle
  {
   [NonSerialized] public float footStopRemaining;
   [NonSerialized] public Vector3 footResumeVelocity;
   [NonSerialized] public bool playerClaimed;
   [NonSerialized] public bool trafficCrashTakeable;
  }

  Transform footActor,footSkin,footDoor;
  Vector3 footFacing=Vector3.forward,footJumpStart,footJumpEnd,entryStart,entryDoorSide;
  Vector2 footMove;
  float footJumpTime,footRunClock,entryTime,footThrowPose,abandonedFallSpeed;
  Quaternion footViewHeading=Quaternion.identity;
  double footEscapeTime;
  public float FootEscapeRemaining=>!InsideShop&&!shopEvaded&&
   (PlayerOnFoot||State==RideState.Paused&&pauseFromState==RideState.OnFoot)?(float)footEscapeTime:0;
  TrafficVehicle entryVehicle,claimedVehicle;
  bool exitAfterCollision;
  Vector3 claimedSize;
  float takeoverGrace;
  Renderer[] supercarRenderers;
  bool[] supercarRendererStates;
  Vector3 footSkinPosition;
  Quaternion footSkinRotation;
  bool ShowFootCamera=>footActor!=null&&footActor.gameObject.activeSelf;
  Vector3 PoliceTargetPosition=>ShowFootCamera?footActor.localPosition:rider.localPosition;
  bool VehicleOnRoadForExit=>!RiderAirborne&&!RiderFlying&&!RiderOnRoof&&!RiderInRiver&&rider.localPosition.y<=.001f;
  bool CanExitVehicle=>State==RideState.Riding&&IsPlayerCar&&!ServiceStopped&&!JobOffersOpen&&footCharacterPrefab!=null&&VehicleOnRoadForExit;
  const float FootRearCarInterval=3f;
  float footRearCarTimer;
  int footRearCarLane=2;

  void ResetFootEscape()
  {
   ResetShopHideout();
   CancelVehicleEntry();CancelFootPointer();HideFootActors();RestoreClaimedVehicle();
   exitAfterCollision=false;footJumpTime=footRunClock=footThrowPose=takeoverGrace=abandonedFallSpeed=0;footEscapeTime=0;footFacing=Vector3.forward;
   footViewHeading=Quaternion.identity;footFallRemaining=0;footContactGrace=0;
   footFatalTitle=null;
   footRearCarTimer=0;footRearCarLane=2;
   NotifyFootBrowser(false);
  }

  void HideFootActors()
  {
   if(footActor!=null)footActor.gameObject.SetActive(false);
   HidePoliceOfficer();NotifyFootBrowser(false);
  }

  void EnsureFootCharacter()
  {
   if(footActor!=null||footCharacterPrefab==null)return;
   footActor=StreetArt.Group("Golden Star Stance · người đi bộ",transform);
   footSkin=Instantiate(footCharacterPrefab,footActor,false).transform;
   var animation=footSkin.GetComponentInChildren<Animator>(true);
   if(animation!=null)animation.enabled=false;
   var legacy=footSkin.GetComponentInChildren<Animation>(true);if(legacy!=null)legacy.enabled=false;
   var renderers=footSkin.GetComponentsInChildren<SkinnedMeshRenderer>(true);
   for(int i=0;i<renderers.Length;i++)
   {
    var skin=renderers[i];skin.updateWhenOffscreen=true;
    if(footCharacterMaterial!=null)skin.sharedMaterial=footCharacterMaterial;
   }
   footSkin.localScale*=FootStandingHeight/FootSourceStandingHeight;
   footSkinPosition=footSkin.localPosition;footSkinRotation=footSkin.localRotation;
   if(renderers.Length>0)PrepareFootStandingPose(renderers[0]);
   CacheParkedFootObstacles();
   footActor.gameObject.SetActive(false);
  }

  public bool TryExitVehicle()=>CanExitVehicle&&EnterFootMode();
  bool EnterFootMode()=>BeginFootExit(false);
  bool BeginFootExit(bool emergency)
  {
   if(footCharacterPrefab==null||RiderInRiver||!emergency&&!VehicleOnRoadForExit)return false;
   EnsureFootCharacter();
   Vector3 from=rider.localPosition;
   Quaternion yaw=Quaternion.Euler(0,rider.localEulerAngles.y,0);
   if(!TryFindFootLanding(from,yaw,PlayerHalfWidth+1.3f,out footJumpEnd)&&
      (!emergency||!TryFindFootLanding(from,yaw,PlayerHalfWidth+5.3f,out footJumpEnd)))return false;
   footFacing=yaw*Vector3.forward;
   footJumpStart=from+Vector3.up*.65f;
   footActor.localPosition=footJumpStart;footActor.localRotation=yaw;footActor.gameObject.SetActive(true);
   footFrameStart=footJumpStart;
   footJumpTime=FootExitFlight;
   footRunBlend=0;ApplyFootStandingPose(0);
   footViewHeading=Quaternion.identity;footFallRemaining=0;footContactGrace=0;
   footEscapeTime=FootEscapeSeconds;
   footRearCarTimer=0;
   CancelSwipe();CancelAssistedKick(false);CancelBoost();ResetJump();ClearLaneMotion();
   Speed=0;riderFallVelocity=Vector3.zero;RiderFallComplete=true;exitAfterCollision=false;QueuedTurn=0;IsTurning=false;TurnYaw=0;abandonedFallSpeed=0;
   State=RideState.OnFoot;engine?.Stop();
   if(!IsChased&&!PoliceInRiver)StartChase();
   NotifyFootBrowser(true);
   noticeTimer=0;
   return true;
  }

  bool TryFindFootLanding(Vector3 from,Quaternion yaw,float firstRadius,out Vector3 landing)
  {
   from.y=0;
   for(int ring=0;ring<4;ring++)
    for(int i=0;i<8;i++)
    {
     float angle=(i%2==0?-90:90)+(i/2)*45;
     Vector3 point=from+yaw*Quaternion.Euler(0,angle,0)*Vector3.forward*(firstRadius+ring);
     if(ClearFootLanding(point)){landing=point;return true;}
    }
   landing=from;
   return false;
  }

  bool ClearFootLanding(Vector3 point)
  {
   if(!FootGround(point,9.15f,out _,out _)||!FootTrafficClear(point,null,true,FootContactRadius))return false;
   if(policeBike!=null&&policeBike.gameObject.activeInHierarchy&&!PoliceInRiver&&
      FootInsideVehicle(point,policeBike,PoliceHalfWidth,PoliceHalfLength,FootContactRadius))return false;
   Vector3 world=transform.TransformPoint(point);
   foreach(var parked in parkedFootObstacles)
   {
    if(!parked.gameObject.activeInHierarchy)continue;
    Vector3 local=parked.InverseTransformPoint(world);
    float clearance=FootContactRadius/parked.localScale.x;
    if(Mathf.Abs(local.x)<.48f+clearance&&Mathf.Abs(local.z)<.96f+clearance)return false;
   }
   return true;
  }

  void FinishFootLanding()
  {
   // Recheck after traffic and the patrol have moved, since the arc's target was chosen at takeoff.
   Vector3 grounded=footActor.localPosition;grounded.y=0;
   if(ClearFootLanding(footJumpEnd))grounded=footJumpEnd;
   else if(!ClearFootLanding(grounded)&&TryFindFootLanding(grounded,Quaternion.LookRotation(footFacing),.65f,out var alternate))
    grounded=alternate;
   footActor.localPosition=grounded;
  }

  bool FootGround(Vector3 point,float width,out int rowIndex,out Transform tile)
  {
   rowIndex=mainRow;tile=null;
   if(OverRiver(point))return false;
   Vector3 world=transform.TransformPoint(point);
   float closest=width;
   if(!RoutesReady)return Mathf.Abs(point.x)<=width;
   for(int index=0;index<roadRows.Count;index++)
   {
    var row=roadRows[index];if(!row.root.gameObject.activeInHierarchy)continue;
    foreach(var candidate in row.tiles)
    {
     Vector3 local=candidate.InverseTransformPoint(world);
     if(Mathf.Abs(local.x)>closest||local.z<-.02f||local.z>VietnamStreet.Length+.02f)continue;
     closest=Mathf.Abs(local.x);tile=candidate;rowIndex=index;
    }
   }
   if(tile!=null)return true;
   if(Mathf.Abs(point.z-NextJunctionDistance)>7)return false;
   // Main pavement ends before the side row begins at x = ±10.
   return Mathf.Abs(point.x)<=Mathf.Max(width,10.2f);
  }

  bool FootTrafficClear(Vector3 point,TrafficVehicle except,bool includeWreck=false,float clearance=.22f)
  {
   if(includeWreck&&AbandonedCarPresent&&FootInsideVehicle(point,rider,PlayerHalfWidth,PlayerHalfLength,clearance))return false;
   foreach(var v in vehicles)
   {
    if(!v.active||v==except||v.riverFalling||v.model.localPosition.y>1.7f)continue;
    if(FootInsideVehicle(point,v.model,v.halfWidth,v.halfLength,clearance))return false;
   }
   return true;
  }

  static bool FootInsideVehicle(Vector3 point,Transform car,float halfWidth,float halfLength,float clearance)
  {
   Quaternion yaw=Quaternion.Euler(0,car.localEulerAngles.y,0);
   Vector3 local=Quaternion.Inverse(yaw)*(point-car.localPosition);
   return Mathf.Abs(local.x)<halfWidth+clearance&&Mathf.Abs(local.z)<halfLength+clearance;
  }

  void TickOnFoot(float dt)
  {
   if(dt<=0||State!=RideState.OnFoot)return;
   timeRiding+=dt;Speed=0;TickActionTimers(dt);AdvanceDaylight(dt);
   footThrowPose=Mathf.Max(0,footThrowPose-dt);
   footFrameStart=footActor.localPosition;
   footWasJumping=footJumpTime>0;
   if(!footWasJumping&&footFallRemaining<=0&&!InsideShop&&entryVehicle==null&&footFrameStart.y!=0)
   {footFrameStart.y=0;footActor.localPosition=footFrameStart;}
   footContactGrace=Mathf.Max(0,footContactGrace-dt);
   footEscapeTime=Math.Max(0,footEscapeTime-dt);
   if(footEscapeTime<.00001)footEscapeTime=0;
   bool walkedOutside=false;
   if(footFallRemaining>0)
   {
    AdvanceFootFall(dt);
    if(footFatalTitle!=null)
    {
     PoseFootCharacter(0,false);
     if(FootFallSeconds-footFallRemaining>=footFallLandingTime+.20f){CompleteFatalFootFall();return;}
    }
   }
   else if(footJumpTime>0)
   {
    footJumpTime=Mathf.Max(0,footJumpTime-dt);
    float u=1-footJumpTime/FootExitFlight;
    footActor.localPosition=Vector3.Lerp(footJumpStart,footJumpEnd,u)+Vector3.up*(4*u*(1-u)*.85f);
    PoseFootCharacter(dt,true);
   }
   else
   {
    if(InsideShop)TickShopHideout(dt);
    else if(entryVehicle!=null)TickVehicleEntry(dt);
    else{WalkFoot(dt);walkedOutside=!InsideShop;}
   }
   if(State!=RideState.OnFoot)return;
   bool posedThrow=walkedOutside&&footThrowPose>0;
   if(posedThrow)PoseFootCharacter(dt,(footActor.localPosition-footFrameStart).sqrMagnitude>.000001f);
   if(!InsideShop){TickAbandonedCar(dt);RecenterFootStreet();}
   TickTrafficMotion(dt);
   if(!InsideShop&&footFatalTitle==null)TickTankBarrage(dt);else TickShells(dt);
   if(!PlayerOnFoot)return;
   TickDinosaurs(dt,!InsideShop&&footFatalTitle==null);
   if(!PlayerOnFoot)return;
   TickThunderstorm(dt,false);TickFootTraffic(dt);
   if(InsideShop||footFatalTitle!=null)return;
   var patrol=IsChased&&!PoliceInRiver?policeBody:null;
   Vector3 patrolStart=policeBike!=null?policeBike.localPosition:Vector3.zero;
   footArrestRequested=false;deferFootArrest=true;
   TickChase(dt);TickPoliceRiver(dt);
   deferFootArrest=false;
   if(!PlayerOnFoot)return;
   ResolveFootTrafficContacts(patrol!=policeBody&&policeBike!=null?policeBike.localPosition:patrolStart,dt);
   if(PlayerOnFoot&&footWasJumping&&footJumpTime<=0&&footFallRemaining<=0)FinishFootLanding();
   if(footArrestRequested&&footFatalTitle==null)Arrest();
   if(PlayerOnFoot&&footFallRemaining>0)PoseFootCharacter(0,false);
   else if(PlayerOnFoot&&(walkedOutside&&!posedThrow||footActionPoseApplied&&footThrowPose<=0))
    PoseFootCharacter(dt,footFallRemaining<=0&&(footActor.localPosition-footFrameStart).sqrMagnitude>.000001f);
   if(PlayerOnFoot&&footFallRemaining<=0)ContinueFootStreet();
  }

  void TickAbandonedCar(float dt)
  {
   Vector3 p=rider.localPosition;
   if(!AbandonedCarPresent||p.y<=0)return;
   abandonedFallSpeed+=9.81f*dt;
   p.y=Mathf.Max(0,p.y-abandonedFallSpeed*dt);rider.localPosition=p;
  }

  void RecenterFootStreet()
  {
   if(footWasJumping||entryVehicle!=null||Mathf.Abs(footActor.localPosition.z)<26)return;
   if(!FootGround(footActor.localPosition,9.15f,out int row,out _)||row!=mainRow)return;
   float shift=footActor.localPosition.z;
   Vector3 scroll=Vector3.forward*shift;
   crashTravelDistance+=shift;
   footActor.localPosition-=scroll;footFrameStart-=scroll;rider.localPosition-=scroll;
   if(rideCamera!=null)rideCamera.transform.localPosition-=scroll;
   if(policeBike!=null&&!PoliceWaitingToStart)policeBike.localPosition-=scroll;
   if(PoliceInRiver)policeBody.previousPosition-=scroll;
   if(OfficerOnFoot)policeOfficer.localPosition-=scroll;
   crashPolicePath.Clear();crashPoliceReplan=0;
   foreach(var v in vehicles)
   {
    if(!v.active)continue;
    v.model.localPosition-=scroll;v.previousPosition-=scroll;v.roadTurnStart-=scroll;
    if(v.crashEffect!=null)v.crashEffect.localPosition-=scroll;
    ScrollTrafficRecovery(v,scroll);
   }
   ScrollRoutes(shift);
   if(NextJunctionDistance< -18){PassCityJunction();PrepareBranches(NextJunctionDistance+80);}
  }

  void ContinueFootStreet()
  {
   if(!RoutesReady||footJumpTime>0||entryVehicle!=null)return;
   Vector3 point=footActor.localPosition;
   if(!FootGround(point,9.15f,out int selected,out _)||(selected!=leftRow&&selected!=rightRow))return;
   var row=roadRows[selected].root;
   Vector3 forward=row.localRotation*Vector3.forward;
   if(Vector3.Dot(point-row.localPosition,forward)<.35f)return;
   int side=selected==leftRow?-1:1,oldMain=mainRow,unused=side<0?rightRow:leftRow;
   Quaternion inverse=Quaternion.Inverse(row.localRotation);
   Vector3 origin=row.localPosition+forward*Vector3.Dot(point-row.localPosition,forward);
   ClearShells();RebaseStreetActors(inverse,origin,side*90);
   RebaseFootActor(footActor,inverse,origin);
   footFrameStart=inverse*(footFrameStart-origin);
   RebaseFootActor(rider,inverse,origin);
   if(OfficerOnFoot)RebaseFootActor(policeOfficer,inverse,origin);
   footFacing=inverse*footFacing;footViewHeading=inverse*footViewHeading;
   routePolice=false;crashPolicePath.Clear();crashPoliceReplan=0;
   TurnCityJunction(side);RouteStreet=junctionStreet;visitedStreets.Add(RouteStreet);
   UseStreetRow(selected);leftRow=oldMain;rightRow=unused;PrepareNextStreetJunction();SyncTankCraterMasks(true);
  }

  static void RebaseFootActor(Transform actor,Quaternion inverse,Vector3 origin)
  {actor.SetLocalPositionAndRotation(inverse*(actor.localPosition-origin),inverse*actor.localRotation);}

  void TickFootTraffic(float dt)
  {
   Vector3 foot=FootStreetPosition;
   foreach(var v in vehicles)
   {
    if(!v.active||v==entryVehicle||v.footStopRemaining>0)continue;
    if(Flat(v.model.localPosition-foot).sqrMagnitude>125*125)RetireVehicle(v);
   }
   if(!InsideShop&&footJumpTime<=0&&footFallRemaining<=0&&entryVehicle==null)
   {
    footRearCarTimer-=dt;
    if(footRearCarTimer<=0)
     footRearCarTimer=TrySpawnFootRearCar(foot)?FootRearCarInterval:.30f;
   }
   spawnTimer-=dt;
   if(spawnTimer>0)return;
   spawnTimer=2.4f;
   int lane=rng.Next(4);float z=foot.z+(lane<2?42:-40);
   if(!FootGround(foot,9.15f,out int row,out _)||row!=mainRow||
      !FootGround(new Vector3(LaneX[lane],0,z),7,out _,out _))return;
   foreach(var v in vehicles)if(v.active&&Mathf.Abs(v.model.localPosition.x-LaneX[lane])<2&&Mathf.Abs(v.model.localPosition.z-z)<8)return;
   SpawnTraffic(lane,z);
  }

  bool TrySpawnFootRearCar(Vector3 foot)
  {
   if(!FootGround(foot,9.15f,out int row,out _)||row!=mainRow)return false;
   int selected=-1,count=0;
   for(int i=0;i<vehicles.Count;i++)
   {
    var candidate=vehicles[i];
    if(candidate.active||candidate.eventOnly||RearExitReserved(candidate))continue;
    var car=candidate.model.GetComponent<MeshyCarVariants>();
    string role=car?.driver.AssetName;
    if((role=="PV_Sedan"||role=="PV_Hatchback"||role=="PV_Taxi"||role=="PV_DeliveryVan")&&rng.Next(++count)==0)
     selected=i;
   }
   if(selected<0)return false;
   for(int gap=16;gap<=32;gap+=8)
   {
    for(int attempt=0;attempt<2;attempt++)
    {
     int lane=attempt==0?footRearCarLane:5-footRearCarLane;
     Vector3 position=new Vector3(LaneX[lane],0,foot.z-gap);
     if(lane==3&&OuterLaneNearBridge(position,lane))continue;
     if(!FootGround(position,7,out int spawnRow,out _)||spawnRow!=mainRow)continue;
     if(!FootRearCarSpaceClear(position))continue;
     var vehicle=vehicles[selected];
     vehicle.model.GetComponent<MeshyCarVariants>().PrepareForSpawn(rng,vehicle,false);
     if(SpawnTraffic(lane,position.z,selected)==null)return false;
     footRearCarLane=5-lane;
     return true;
    }
   }
   return false;
  }

  bool FootRearCarSpaceClear(Vector3 position)
  {
   foreach(var other in vehicles)
   {
    if(!other.active||other.riverFalling||other.model.localPosition.y>2.4f)continue;
    Vector2 half=VehicleFootprint(other);
    Vector3 delta=position-other.model.localPosition;
    if(Mathf.Abs(delta.x)<half.x+1.45f&&Mathf.Abs(delta.z)<half.y+4.4f)return false;
   }
   if(AbandonedCarPresent&&FootInsideVehicle(position,rider,PlayerHalfWidth,PlayerHalfLength,3))return false;
   return !IsChased||policeBike==null||!policeBike.gameObject.activeInHierarchy||
    !FootInsideVehicle(position,policeBike,PoliceHalfWidth,PoliceHalfLength,3);
  }

  void WalkFoot(float dt)
  {
   Vector3 move=footViewHeading*new Vector3(footMove.x,0,footMove.y),start=footActor.localPosition;
   if(move.sqrMagnitude>.005f)
   {
    if(footThrowPose<=FootThrowDuration-FootThrowRelease)footFacing=move.normalized;
    Vector3 step=move*(FootSpeed*dt),next=start+step;
    if(TryWalkIntoShop(start,next))return;
    if(CanWalkTo(next))footActor.localPosition=next;
    else if(CanWalkTo(start+Vector3.right*step.x))footActor.localPosition=start+Vector3.right*step.x;
    else if(CanWalkTo(start+Vector3.forward*step.z))footActor.localPosition=start+Vector3.forward*step.z;
    footActor.localRotation=Quaternion.RotateTowards(footActor.localRotation,Quaternion.LookRotation(footFacing),dt*760);
   }
  }

  bool CanWalkTo(Vector3 point)=>FootGround(point,9.15f,out _,out _);

  void PoseFootCharacter(float dt,bool moving)
  {
   if(footSkin==null)return;
   footSkin.localPosition=footSkinPosition;
   footSkin.localRotation=footSkinRotation;
   if(footActionPoseApplied)ApplyFootStandingPose(0);
   footActionPoseApplied=false;
   AdvanceFootPose(dt,moving);
   if(footFallRemaining>0){ApplyFootFallPose();return;}
   if(footThrowPose>0&&!InsideShop&&entryVehicle==null){ApplyFootThrowPose();return;}
   float lean=footJumpTime>0?-12*Mathf.Sin(footJumpTime/FootExitFlight*Mathf.PI):entryVehicle!=null?18*Mathf.Sin(entryTime/FootDoorTime*Mathf.PI):0;
   footSkin.localRotation=Quaternion.Euler(lean,0,0)*footSkinRotation;
  }

  void PositionFootCamera(bool snap,float dt)
  {
   Vector3 point=footActor.localPosition;
   point.y=0;
   bool portraitView=Screen.width<Screen.height;
   float lateral=Mathf.Clamp(point.x,-9.15f,9.15f);
   float blend=snap?1:1-Mathf.Exp(-dt*5);
   footViewHeading=snap?Quaternion.identity:EaseStreetView(footViewHeading,dt);
   Vector3 target=point+footViewHeading*new Vector3(-lateral*(portraitView?.15f:.75f),7.8f,-14f);
   rideCamera.transform.localPosition=Vector3.Lerp(rideCamera.transform.localPosition,target,blend);
   Vector3 look=point+footViewHeading*new Vector3(-lateral*(portraitView?.15f:.82f),1.2f,8);
   Quaternion view=Quaternion.LookRotation(transform.TransformPoint(look)-rideCamera.transform.position);
   rideCamera.transform.rotation=Quaternion.Slerp(rideCamera.transform.rotation,view,blend);
   rideCamera.fieldOfView=Mathf.Lerp(rideCamera.fieldOfView,RideCameraFov(portraitView,0),blend);
  }

  static Quaternion EaseStreetView(Quaternion heading,float dt)=>
   Quaternion.Slerp(heading,Quaternion.identity,1-Mathf.Exp(-dt*2.6f));

  bool TickFootGrenadeStop(TrafficVehicle v,Vector3 previous,float dt)
  {
   if((v.footStopRemaining<=0&&v!=entryVehicle)||v.riverFalling)return false;
   if(v!=entryVehicle)v.footStopRemaining=Mathf.Max(0,v.footStopRemaining-dt);
   Vector3 scroll=Vector3.forward*(RoadScrollSpeed*dt);
   v.model.localPosition=previous-scroll;v.roadTurnStart-=scroll;
   v.motionVelocity=Vector3.zero;
   return true;
  }
 }
}
