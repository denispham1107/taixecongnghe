using System.Collections.Generic;
using UnityEngine;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const float FootFallSeconds=1.5f;
  const float FootContactRadius=.32f;
  const float FootVehicleMovementThreshold=.08f;
  const float FatalFootImpactSpeed=60f/3.6f;
  readonly List<Transform> parkedFootObstacles=new List<Transform>();
  float footFallRemaining,footContactGrace,footFallLandingTime,footFallDistance,footFallLift;
  Vector3 footFallDirection;
  Vector3 footFrameStart;
  bool footWasJumping;
  bool deferFootArrest,footArrestRequested;
  string footFatalTitle;

  void CacheParkedFootObstacles()
  {
   foreach(var row in roadRows)
    foreach(var tile in row.tiles)
     foreach(Transform child in tile)
      if(child.name=="Xe máy đậu trên vỉa hè")parkedFootObstacles.Add(child);
  }

  enum FootVehicleResponse { Block, Fall, FatalFall, TankImpact }

  void ResolveFootTrafficContacts(Vector3 patrolStart,float dt)
  {
   bool fallen=footFallRemaining>0;
   Vector3 footEnd=footActor.localPosition;
   float first=float.PositiveInfinity;
   Vector3 normal=Vector3.zero;
   Vector3 impactVelocity=Vector3.zero;
   var response=FootVehicleResponse.Block;
   float movingThreshold=FootVehicleMovementThreshold*dt;
   foreach(var v in vehicles)
   {
    if(v.active&&v!=entryVehicle&&!v.riverFalling)ConsiderVehicle(v,v.previousPosition,TrafficContactHeight(v));
   }
   if(policeBody!=null&&policeBike.gameObject.activeInHierarchy&&!PoliceInRiver)
    ConsiderVehicle(policeBody,patrolStart,PoliceHeight);
   if(!fallen&&!footWasJumping&&AbandonedCarPresent)
    ConsiderVehicle(riderBody,rider.localPosition,TrafficContactHeight(riderBody));

   if((footEnd-footFrameStart).sqrMagnitude>.000001f)
    foreach(var parked in parkedFootObstacles)
    {
     if(!parked.gameObject.activeInHierarchy)continue;
     Vector3 position=transform.InverseTransformPoint(parked.position);
     if((footFrameStart-position).sqrMagnitude>9)continue;
     Quaternion yaw=Quaternion.Euler(0,(Quaternion.Inverse(transform.rotation)*parked.rotation).eulerAngles.y,0);
     float scale=parked.localScale.x;
     Consider(position,position,yaw,.48f*scale,.96f*scale,FootStandingHeight*scale,FootVehicleResponse.Block,Vector3.zero);
    }

   if(float.IsPositiveInfinity(first))return;
   if(response==FootVehicleResponse.TankImpact){EndFootCollision("Va vào xe tăng!");return;}
   Vector3 impact=Vector3.Lerp(footFrameStart,footEnd,first);
   if(response==FootVehicleResponse.Block||response==FootVehicleResponse.Fall&&footContactGrace>0)
   {
    // Blocking walking motion must not rewind the jump or fall's vertical step.
    if(entryVehicle==null)impact.y=footEnd.y;
    if(FootGround(impact,9.15f,out _,out _))footActor.localPosition=impact;
    return;
   }
   Vector3 clear=impact+normal*.36f;clear.y=0;
   if(FootGround(clear,9.15f,out _,out _))footActor.localPosition=clear;
   else{impact.y=0;footActor.localPosition=impact;}
   BeginFootVehicleFall(normal,impactVelocity,response==FootVehicleResponse.FatalFall);

   void ConsiderVehicle(TrafficVehicle vehicle,Vector3 from,float height)
   {
    Vector3 to=vehicle.model.localPosition;
    Vector3 displacement=to-from;
    Vector3 velocity=vehicle==policeBody?displacement/dt:vehicle.motionVelocity;
    bool moving=displacement.sqrMagnitude>movingThreshold*movingThreshold;
    var contact=FootVehicleResponse.Block;
    if(moving&&vehicle.armored)contact=FootVehicleResponse.TankImpact;
    else if(moving&&IsFatalFootVehicleSpeed(velocity))contact=FootVehicleResponse.FatalFall;
    else if(moving&&!fallen)contact=FootVehicleResponse.Fall;
    Consider(from,to,VehicleYaw(vehicle),BodyWidth(vehicle),vehicle.halfLength,height,contact,velocity);
   }

   void Consider(Vector3 from,Vector3 to,Quaternion yaw,float width,float length,float height,FootVehicleResponse contact,Vector3 velocity)
   {
    if(!FootVehicleSweep(from,to,yaw,width,length,height,footEnd,contact==FootVehicleResponse.Block,out float time,out Vector3 direction))return;
    if(time>first||(time==first&&contact<=response))return;
    first=time;normal=direction;response=contact;impactVelocity=velocity;
   }
  }

  static bool IsFatalFootVehicleSpeed(Vector3 velocity)=>Flat(velocity).magnitude>FatalFootImpactSpeed+.0005f;

  void BeginFootVehicleFall(Vector3 away,Vector3 velocity,bool fatal)
  {
   BeginFootFall(away,velocity);
   if(!fatal)return;
   footFatalTitle="Bị xe tông ở tốc độ cao!";
   notice="VA CHẠM MẠNH · CHUYẾN ĐI KẾT THÚC";
   StopPursuitForFatalImpact();HidePoliceOfficer();
  }

  bool FootVehicleSweep(Vector3 from,Vector3 to,Quaternion yaw,float width,float length,float height,
   Vector3 footEnd,bool stationary,out float time,out Vector3 outward)
  {
   time=0;outward=Vector3.zero;
   Quaternion inverse=Quaternion.Inverse(yaw);
   Vector3 start=inverse*(footFrameStart-from),end=inverse*(footEnd-to),delta=end-start;
   if(stationary&&delta.sqrMagnitude<=.000001f)return false;
   Vector3 low=new Vector3(-width-FootContactRadius,-FootStandingHeight,-length-FootContactRadius);
   Vector3 high=new Vector3(-low.x,height,-low.z);
   float enter=0,leave=1,nearest=float.MaxValue;
   Vector3 direction=Vector3.zero;
   for(int axis=0;axis<3;axis++)
   {
    float before=enter;
    if(!ContactHalfSpace(start[axis]-high[axis],delta[axis],ref enter,ref leave)||
       !ContactHalfSpace(low[axis]-start[axis],-delta[axis],ref enter,ref leave))return false;
    if(axis==1)continue;
    float p=start[axis],edge=high[axis]-Mathf.Abs(p);
    if(enter>before){direction=Vector3.zero;direction[axis]=delta[axis]<0?1:-1;}
    else if(enter<=0&&edge<nearest){direction=Vector3.zero;direction[axis]=p>=0?1:-1;nearest=edge;}
   }
   if(direction.sqrMagnitude<.5f)direction=Vector3.right;
   float separation=Vector3.Dot(direction,delta);
   if(enter<=.0001f&&(stationary?separation>=0:separation>.002f))return false;
   time=enter;outward=yaw*direction;
   return true;
  }

  void BeginFootFall(Vector3 away,Vector3 vehicleVelocity)
  {
   CancelVehicleEntry();CancelFootPointer();footJumpTime=0;footThrowPose=0;
   vehicleVelocity.y=0;
   away.y=0;
   footFallDirection=(away*.8f+vehicleVelocity.normalized*.35f).normalized;
   float severity=Mathf.InverseLerp(5,27,vehicleVelocity.magnitude);
   footFallDistance=Mathf.Lerp(.15f,4.5f,severity*severity);
   footFallLift=Mathf.Lerp(0,.95f,severity*severity);
   footFallLandingTime=Mathf.Lerp(.28f,.70f,severity);
   footFallRemaining=FootFallSeconds;footContactGrace=FootFallSeconds+.25f;
   notice="VA CHẠM · ĐANG ĐỨNG DẬY";noticeTimer=FootFallSeconds;
   if(sound&&fx!=null)fx.PlayOneShot(crashClip,.45f);
  }

  void AdvanceFootFall(float dt)
  {
   float previousAge=FootFallSeconds-footFallRemaining;
   footFallRemaining=Mathf.Max(0,footFallRemaining-dt);
   if(footFallRemaining<.00001f)footFallRemaining=0;
   float progress=Mathf.Clamp01((FootFallSeconds-footFallRemaining)/footFallLandingTime);
   float previousProgress=Mathf.Clamp01(previousAge/footFallLandingTime);
   float travel=footFallDistance*(progress-previousProgress)*(2-progress-previousProgress);
   Vector3 position=footActor.localPosition;
   Vector3 next=position+footFallDirection*travel;
   if(FootGround(next,9.15f,out _,out _))position=next;
   position.y=4*footFallLift*progress*(1-progress);
   footActor.localPosition=position;
  }

  void EndFootCollision(string title)
  {
   if(!PlayerOnFoot||footFatalTitle!=null)return;
   BeginFootFall(Vector3.right,Vector3.zero);footFallRemaining=FootFallSeconds-.30f;PoseFootCharacter(0,false);
   footFatalTitle=title;
   CompleteFatalFootFall();
  }

  void CompleteFatalFootFall()
  {
   StopPursuitForFatalImpact();HidePoliceOfficer();NotifyFootBrowser(false);
   Speed=0;State=RideState.GameOver;SaveRideRecord();
  }
 }
}
