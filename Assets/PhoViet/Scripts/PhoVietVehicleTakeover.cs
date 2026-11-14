using UnityEngine;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  Renderer[] doorOriginals,doorCopies;
  Vector3 doorHinge;
  Vector4 doorCut,doorSpan;
  MaterialPropertyBlock doorProperties;
  static readonly int DoorCutId=Shader.PropertyToID("_PVDoorCut"),DoorSpanId=Shader.PropertyToID("_PVDoorSpan"),DoorFrameId=Shader.PropertyToID("_PVDoorFrame");
  internal bool HasClaimedCar=>claimedVehicle!=null;
  bool CanTakeVehicle(TrafficVehicle v)
  {
   if(v==null||!v.active||v.armored||v.riverFalling||v.wallCrashed||v.lightningWreck||
      v.halfWidth<=.7f||v.model.localPosition.y>=.15f)return false;
   if(v==entryVehicle)return true;
   var burn=FindCrashBurn(v.model);
   if(burn!=null&&CrashBurnValid(burn)&&CrashExplosionDelay-burn.age<FootDoorTime+FootExitFlight+.1f)return false;
   if(!v.knocked)return v.footStopRemaining>0;
   return v.trafficCrashTakeable&&v.recovery?.phase==0&&Flat(v.knockVelocity).sqrMagnitude<=9&&
    Mathf.Abs(v.knockVelocity.y)<.6f&&v.angularVelocity.sqrMagnitude<18*18;
  }
  static Quaternion VehicleYaw(TrafficVehicle v)=>Quaternion.Euler(0,v.model.localEulerAngles.y,0);
  static float DoorCentreZ(TrafficVehicle v)=>v.halfLength*(v.halfLength>2.9f?.53f:.21f);
  static Vector3 LeftDoorPosition(TrafficVehicle v)=>v.model.localPosition+VehicleYaw(v)*new Vector3(-v.halfWidth-.54f,0,DoorCentreZ(v));

  TrafficVehicle NearbyTakeableVehicle()
  {
   if(State!=RideState.OnFoot||InsideShop||footJumpTime>0||footFallRemaining>0||entryVehicle!=null)return null;
   float distance=1.95f*1.95f;TrafficVehicle nearest=null;
   foreach(var v in vehicles)
   {
    if(!CanTakeVehicle(v))continue;
    Vector3 side=Quaternion.Inverse(VehicleYaw(v))*(footActor.localPosition-v.model.localPosition);
    if(side.x>-v.halfWidth+.12f)continue;
    Vector3 door=LeftDoorPosition(v);
    float gap=(footActor.localPosition-door).sqrMagnitude;
    if(gap>distance||!FootGround(door,9.15f,out _,out _)||!FootTrafficClear(door,v))continue;
    nearest=v;distance=gap;
   }
   return nearest;
  }

  public bool TryTakeVehicle()
  {
   var target=NearbyTakeableVehicle();if(target==null)return false;
   entryVehicle=target;entryTime=0;entryStart=footActor.localPosition;entryDoorSide=LeftDoorPosition(target);
   CancelFootPointer();PrepareLeftDoor(target);
   notice="ĐANG MỞ CỬA TRÁI";noticeTimer=FootDoorTime;
   return true;
  }

  void TickVehicleEntry(float dt)
  {
   if(!CanTakeVehicle(entryVehicle)){CancelVehicleEntry();return;}
   entryTime+=dt;float phase=Mathf.Clamp01(entryTime/FootDoorTime);
   footActor.localPosition=Vector3.Lerp(entryStart,entryDoorSide,Mathf.SmoothStep(0,1,Mathf.Clamp01(phase*2.8f)));
   if(phase>.46f)
   {
    Vector3 seat=entryVehicle.model.localPosition+VehicleYaw(entryVehicle)*new Vector3(-entryVehicle.halfWidth*.35f,.62f,DoorCentreZ(entryVehicle));
    footActor.localPosition=Vector3.Lerp(entryDoorSide,seat,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.46f,1,phase)));
   }
   footActor.localRotation=Quaternion.RotateTowards(footActor.localRotation,VehicleYaw(entryVehicle)*Quaternion.Euler(0,90,0),dt*690);
   if(footDoor!=null)
   {
    float open=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.14f,.54f,phase))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.82f,1,phase)));
    footDoor.localRotation=Quaternion.Euler(0,open*68,0);DrawLeftDoor(entryVehicle,open>.015f);
   }
   PoseFootCharacter(dt,phase<.4f);
   if(phase>=1)TakeVehicle(entryVehicle);
  }

  void PrepareLeftDoor(TrafficVehicle v)
  {
   var source=v.model.GetComponent<CarVisualAnimator>()?.visual;
   if(source==null)return;
   float length=Mathf.Clamp(v.halfLength*.66f,.84f,1.22f);
   float centre=DoorCentreZ(v),height=v.model.GetComponent<MeshyCarVariants>()?.Height??1.72f;
   doorHinge=new Vector3(-v.halfWidth*.94f,0,centre+length*.5f);
   doorCut=new Vector4(-1,-v.halfWidth*.63f,.40f,height*.90f);
   doorSpan=new Vector4(centre-length*.5f,centre+length*.5f,0,0);
   footDoor=StreetArt.Group("Bản lề cửa trái",v.model,doorHinge);
   Transform copy=Instantiate(source.gameObject,footDoor,false).transform;
   copy.localPosition=v.model.InverseTransformPoint(source.position)-doorHinge;
   copy.localRotation=Quaternion.Inverse(v.model.rotation)*source.rotation;
   doorOriginals=source.GetComponentsInChildren<Renderer>(true);
   doorCopies=copy.GetComponentsInChildren<Renderer>(true);
   foreach(var motion in copy.GetComponentsInChildren<Animation>(true))motion.enabled=false;
   foreach(var motion in copy.GetComponentsInChildren<Animator>(true))motion.enabled=false;
   foreach(var light in copy.GetComponentsInChildren<Light>(true))light.enabled=false;
   foreach(var collider in copy.GetComponentsInChildren<Collider>(true))collider.enabled=false;
   footDoor.gameObject.SetActive(false);
  }

  void DrawLeftDoor(TrafficVehicle v,bool open)
  {
   footDoor.gameObject.SetActive(open);
   if(doorProperties==null)doorProperties=new MaterialPropertyBlock();
   var copyFrame=Matrix4x4.Translate(doorHinge)*footDoor.worldToLocalMatrix;
   for(int i=0;i<doorOriginals.Length&&i<doorCopies.Length;i++)
   {
    var original=doorOriginals[i];var copy=doorCopies[i];
    bool supported=original.sharedMaterial!=null&&original.sharedMaterial.HasProperty(DoorCutId);
    copy.enabled=supported&&original.enabled;
    if(!supported)continue;
    original.GetPropertyBlock(doorProperties);doorProperties.SetVector(DoorCutId,open?doorCut:Vector4.zero);
    doorProperties.SetVector(DoorSpanId,doorSpan);doorProperties.SetMatrix(DoorFrameId,v.model.worldToLocalMatrix);original.SetPropertyBlock(doorProperties);
    var duplicate=doorCut;duplicate.x=1;
    doorProperties.SetVector(DoorCutId,duplicate);doorProperties.SetMatrix(DoorFrameId,copyFrame);copy.SetPropertyBlock(doorProperties);
   }
  }

  void CancelVehicleEntry()
  {
   if(footDoor!=null)
   {
    if(entryVehicle!=null)DrawLeftDoor(entryVehicle,false);
    Destroy(footDoor.gameObject);footDoor=null;doorOriginals=doorCopies=null;
   }
   entryVehicle=null;entryTime=0;
  }

  void TakeVehicle(TrafficVehicle v)
  {
   Vector3 at=v.model.localPosition;
   FootGround(at,8.4f,out int selected,out _);
   CancelVehicleEntry();RestoreClaimedVehicle();
   v.active=false;v.playerClaimed=true;v.footStopRemaining=0;v.motionVelocity=Vector3.zero;
   v.trafficCrashTakeable=false;v.recovery=null;v.knocked=v.physicalCrash=false;
   v.knockVelocity=v.angularVelocity=Vector3.zero;
   claimedVehicle=v;
   var car=v.model.GetComponent<MeshyCarVariants>();
   claimedSize=new Vector3(v.halfWidth*2,car!=null?car.Height:1.85f,v.halfLength*2);
   var original=rider.GetComponent<PlayerCarVisual>();
   if(supercarRenderers==null)
   {
    supercarRenderers=original.visual.GetComponentsInChildren<Renderer>(true);
    supercarRendererStates=new bool[supercarRenderers.Length];
    for(int i=0;i<supercarRenderers.Length;i++)supercarRendererStates[i]=supercarRenderers[i].enabled;
   }
   foreach(var renderer in supercarRenderers)renderer.enabled=false;
   v.model.SetParent(rider,false);v.model.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
   if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(false);
   StopRecklessFx(v);
   ResumeDrivingAt(at,selected);
  }

  void RestoreClaimedVehicle()
  {
   if(claimedVehicle==null)return;
   claimedVehicle.model.SetParent(transform,false);
   claimedVehicle.model.gameObject.SetActive(false);claimedVehicle.playerClaimed=false;claimedVehicle=null;
   for(int i=0;i<supercarRenderers.Length;i++)if(supercarRenderers[i]!=null)supercarRenderers[i].enabled=supercarRendererStates[i];
  }

  void ResumeDrivingAt(Vector3 at,int selected)
  {
   float x=at.x;
   if(RoutesReady)
   {
    var row=roadRows[selected];Quaternion heading=row.root.localRotation,inverse=Quaternion.Inverse(heading);
    x=(inverse*(at-row.root.localPosition)).x;
    Vector3 origin=at-heading*Vector3.right*x;origin.y=0;
    ClearShells();RebaseStreetActors(inverse,origin,Mathf.DeltaAngle(0,heading.eulerAngles.y));
    if(selected!=mainRow)
    {
     int direction=Vector3.Dot(heading*Vector3.forward,Vector3.right)>0?1:-1;
     TurnCityJunction(direction);RouteStreet=junctionStreet;visitedStreets.Add(RouteStreet);
    }
    UseStreetRow(selected);ExtendDrivingStreetBehind();PrepareNextStreetJunction();SyncTankCraterMasks(true);
   }
   int lane=0;for(int i=1;i<LaneX.Length;i++)if(Mathf.Abs(LaneX[i]-x)<Mathf.Abs(LaneX[lane]-x))lane=i;
   rider.localPosition=new Vector3(Mathf.Clamp(x,-6,6),0,0);rider.localRotation=Quaternion.identity;CurrentLane=lane;
   oldRiderZ=0;lastX=rider.localPosition.x;IsTurning=false;TurnYaw=0;QueuedTurn=0;routePolice=false;cameraTurnBlend=.7f;
   ResetDriveInput();ResetBikeImpact();ResetJump();ClearLaneMotion();ResetRiderImpactForVehicleEntry();
   Speed=0;drivePedal=DrivePedalMode.Cruise;takeoverGrace=.65f;HideFootActors();
   State=RideState.Riding;rider.gameObject.SetActive(true);if(sound)engine?.Play();
   riderWreckExploded=false;RiderAirAttackBurning=false;
   shopEvaded=false;ScheduleChaseAfterTakeover();
   notice="ĐÃ LẤY XE · CSGT ĐUỔI LẠI SAU 3 GIÂY";noticeTimer=PoliceTakeoverDelay;
  }

  void ResetRiderImpactForVehicleEntry()
  {
   CrashDuringPursuit=false;exitAfterCollision=false;RiderFallComplete=false;fallTime=settledTime=0;
   riderFallVelocity=riderFallSpin=riderRoadVelocity=Vector3.zero;riderVelocityValid=false;
   riderBody=new TrafficVehicle{model=rider,halfWidth=PlayerHalfWidth,halfLength=PlayerHalfLength};
   ResetCrashPoliceNavigation();
  }

  internal void PositionClaimedCar(Transform original)
  {
   if(claimedVehicle!=null)claimedVehicle.model.SetLocalPositionAndRotation(original.localPosition,original.localRotation);
  }
 }
}
