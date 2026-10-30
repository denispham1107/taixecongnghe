using System;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public partial class TrafficVehicle
  {
   [NonSerialized] public TrafficRecovery recovery;
   public bool IsStandingUp=>recovery!=null&&recovery.phase==1;
   public float StandUpProgress=>IsStandingUp?Mathf.Clamp01(recovery.standAge/recovery.StandDuration):0;
  }
  public sealed class TrafficRecovery
  {
   public float age,standAge,standTime,speed,curveAt,curveLength;
   public int phase,lane;public bool finishTurn,reckless,crossing;
   public Vector3 forward,right,roadOrigin,heading,goal,endHeading,standStart,standTarget,a,b,c,d;
   public Quaternion standRotation;
   public float StandDuration=>Mathf.Max(.65f,standTime);
  }
  const float StandUpSweepSpeed=10;
  void RememberTrafficCourse(TrafficVehicle v)
  {
   if(v.wallCrashed||v.lightningWreck||!vehicles.Contains(v))return; // Police keep their separate 1.2-second recovery.
   var r=v.recovery;
   if(r==null)
   {
    Vector3 velocity=BodyVelocity(v);velocity.y=0;
    if(velocity.sqrMagnitude<.01f)velocity=v.model.localRotation*Vector3.forward*Mathf.Max(4,Mathf.Abs(v.baseTrafficSpeed));
    r=new TrafficRecovery{speed=Mathf.Max(4,v.crossing?Mathf.Abs(v.crossSpeed):Mathf.Abs(v.baseTrafficSpeed)),heading=velocity.normalized,reckless=v.isReckless,crossing=v.crossing};
    r.forward=Vector3.forward;r.right=Vector3.right;r.roadOrigin=new Vector3(0,0,v.model.localPosition.z);
    if(v.crossing||v.routeDetached&&Mathf.Abs(velocity.x)>Mathf.Abs(velocity.z))
    {
     float centre=v.model.localPosition.z,best=float.PositiveInfinity;Vector3 junction=r.roadOrigin;
     foreach(var tile in blocks){float z=tile.localPosition.z+VietnamStreet.IntersectionCenter;float distance=Mathf.Abs(z-centre);if(distance<best){best=distance;junction=new Vector3(v.model.localPosition.x,0,z);}}
     // Xe bị hất ngang giữa đoạn thẳng không nằm trên phố ngang: lấy làn của ngã tư xa
     // làm đích đứng dậy sẽ trượt xuyên 15–20 m trong 0,65 s (21.50). Chỉ dùng khung phố
     // ngang khi tâm xe còn trong bốn làn của phố ngang.
     if(v.crossing||best<=Mathf.Abs(LaneX[3])+1.55f){r.roadOrigin=junction;r.forward=Vector3.right;r.right=Vector3.back;}
    }
    // Hướng chạy gần như vuông góc với đường thì dấu của nó không cho biết chiều làn; giữ
    // xe ở nửa đường nó đang đứng thay vì kéo sang làn ngược chiều bên kia (tới 9,6 m).
    if(Mathf.Abs(Vector3.Dot(r.heading,r.forward))<Mathf.Abs(Vector3.Dot(r.heading,r.right)))
     r.heading=r.forward*(Vector3.Dot(v.model.localPosition-r.roadOrigin,r.right)>=0?1:-1);
    float direction=Vector3.Dot(r.heading,r.forward);r.lane=direction>=0?2:0;
    if(v.roadTurning)
    {
     r.finishTurn=true;r.goal=v.roadTurnStart+v.roadTurnRadius*(v.roadTurnForward+v.roadTurnSide);r.endHeading=v.roadTurnSide;r.speed=v.roadTurnSpeed;
    }
    else if(v.crossing&&v.turning)
    {
     r.finishTurn=true;r.goal=new Vector3(v.turnEndX,0,v.turnOriginZ+v.turnZSign*v.turnRadius-Distance);r.endHeading=Vector3.forward*v.turnZSign;r.speed=Mathf.Abs(v.crossSpeed);
    }
    v.recovery=r;
   }
   // A second actual impact restarts the complete three-second wait, while the
   // original destination/traffic direction survives the new physical impulse.
   if(r.phase==2){var current=BodyVelocity(v);current.y=0;if(current.sqrMagnitude>.01f)r.heading=current.normalized;}
   r.age=0;r.phase=0;r.standAge=0;
  }
  void ScrollTrafficRecovery(TrafficVehicle v,Vector3 offset)
  {
   var r=v.recovery;if(r==null)return;
   r.roadOrigin-=offset;r.goal-=offset;r.standStart-=offset;r.standTarget-=offset;r.a-=offset;r.b-=offset;r.c-=offset;r.d-=offset;
  }
  void RebaseTrafficRecovery(TrafficVehicle v,Quaternion inverse,Vector3 origin)
  {
   var r=v.recovery;if(r==null)return;
   r.roadOrigin=inverse*(r.roadOrigin-origin);r.goal=inverse*(r.goal-origin);r.standStart=inverse*(r.standStart-origin);r.standTarget=inverse*(r.standTarget-origin);
   r.a=inverse*(r.a-origin);r.b=inverse*(r.b-origin);r.c=inverse*(r.c-origin);r.d=inverse*(r.d-origin);
   r.forward=inverse*r.forward;r.right=inverse*r.right;r.heading=inverse*r.heading;r.endHeading=inverse*r.endHeading;r.standRotation=inverse*r.standRotation;
  }
  bool RecoveryLaneClear(TrafficVehicle v,Vector3 target)
  {
   Vector3 heading=v.recovery.heading;float yaw=Mathf.Atan2(heading.x,heading.z),c=Mathf.Abs(Mathf.Cos(yaw)),s=Mathf.Abs(Mathf.Sin(yaw));
   Vector2 half=new Vector2(v.halfWidth*c+v.halfLength*s,v.halfLength*c+v.halfWidth*s);
   foreach(var other in vehicles)
   {
    if(!other.active||other==v)continue;var delta=other.model.localPosition-target;Vector2 otherHalf=VehicleFootprint(other);
    if(Mathf.Abs(delta.x)<half.x+otherHalf.x+.3f&&Mathf.Abs(delta.z)<half.y+otherHalf.y+.6f)return false;
   }
   return Mathf.Abs(target.x-rider.localPosition.x)>half.x+.65f||Mathf.Abs(target.z-rider.localPosition.z)>half.y+1.4f;
  }
  bool TickTrafficRecovery(TrafficVehicle v,float dt)
  {
   var r=v.recovery;if(r==null||v.wallCrashed||v.lightningWreck)return false;
   if(r.phase==2){TickRecoveryTurn(v,dt);return true;}
   r.age+=dt;
   if(r.phase==0)
   {
    if(r.age<3)return false;
    Vector3 p=v.model.localPosition,target=p;target.y=0;
    if(!r.finishTurn)
    {
     float lateral=Vector3.Dot(p-r.roadOrigin,r.right),along=Vector3.Dot(p-r.roadOrigin,r.forward);
     int first=Vector3.Dot(r.heading,r.forward)>=0?2:0,near=Mathf.Abs(lateral-LaneX[first])<=Mathf.Abs(lateral-LaneX[first+1])?first:first+1;
     target=r.roadOrigin+r.forward*along+r.right*LaneX[near];target.y=0;
     if(!RecoveryLaneClear(v,target))
     {
      near=near==first?first+1:first;target=r.roadOrigin+r.forward*along+r.right*LaneX[near];target.y=0;
      if(!RecoveryLaneClear(v,target))return false;
     }
     r.lane=near;r.heading=r.forward*Mathf.Sign(Vector3.Dot(r.heading,r.forward));
    }
    else if(!RecoveryLaneClear(v,target))return false;
    r.phase=1;r.standAge=0;r.standStart=p;r.standTarget=target;r.standRotation=v.model.localRotation;
    // Đứng dậy vẫn 0,65 s khi chỉ nhích vào làn; xe dài phải xoay/trượt xa thì kéo dài để
    // điểm xa nhất của thân không quét nhanh hơn StandUpSweepSpeed (SmoothStep đỉnh ×1,5),
    // không hất CSGT/xe máy đang ngã như bị dịch chuyển tức thời (21.50).
    float turn=Mathf.Abs(Mathf.DeltaAngle(v.model.localEulerAngles.y,Mathf.Atan2(r.heading.x,r.heading.z)*Mathf.Rad2Deg))*Mathf.Deg2Rad;
    r.standTime=1.5f*(Vector3.Distance(p,target)+turn*Mathf.Max(v.halfLength,v.halfWidth))/StandUpSweepSpeed;
    v.knockVelocity=Vector3.zero;v.angularVelocity=Vector3.zero;v.speed=0;
   }
   float duration=r.StandDuration;r.standAge=Mathf.Min(duration,r.standAge+dt);float t=Mathf.SmoothStep(0,1,r.standAge/duration);
   v.model.localPosition=Vector3.Lerp(r.standStart,r.standTarget,t);
   v.model.localRotation=Quaternion.Slerp(r.standRotation,Quaternion.LookRotation(r.heading,Vector3.up),t);
   if(r.standAge<duration)return true;
   v.knocked=false;v.physicalCrash=false;v.knockAge=0;v.knockVelocity=Vector3.zero;v.angularVelocity=Vector3.zero;
   v.changingLane=v.turning=v.roadTurning=v.enteringFromBehind=false;v.brakeRemaining=0;v.laneCooldown=1.5f;v.brakeCooldown=1.5f;
   v.speed=v.baseTrafficSpeed=r.speed*Mathf.Sign(Vector3.Dot(r.heading,r.forward));v.lane=r.lane;v.motionVelocity=r.heading*r.speed;v.crossing=false;v.isReckless=r.reckless;
   if(r.finishTurn)
   {
    r.phase=2;r.curveAt=0;r.a=v.model.localPosition;r.d=r.goal;
    float reach=Mathf.Max(1,Vector3.Distance(r.a,r.d)*.45f);r.b=r.a+r.heading*reach;r.c=r.d-r.endHeading*reach;
    r.curveLength=0;var previous=r.a;for(int i=1;i<=32;i++){var q=RecoveryPoint(r,i/32f);r.curveLength+=Vector3.Distance(previous,q);previous=q;}
    return true;
   }
   bool longitudinal=Vector3.Dot(r.forward,Vector3.forward)>.99f;
   v.routeDetached=!longitudinal;v.routeVelocity=r.heading*r.speed;
   if(v.isReckless){v.startPosition=v.model.localPosition;v.recklessAge=0;v.swerveDirection=v.lane==3?-1:1;}
   v.recovery=null;return true;
  }
  static Vector3 RecoveryPoint(TrafficRecovery r,float t)
  {float u=1-t;return u*u*u*r.a+3*u*u*t*r.b+3*u*t*t*r.c+t*t*t*r.d;}
  void TickRecoveryTurn(TrafficVehicle v,float dt)
  {
   var r=v.recovery;float previous=r.curveAt;r.curveAt=Mathf.Min(1,r.curveAt+dt*Mathf.Max(4,r.speed)/Mathf.Max(.01f,r.curveLength));
   var p=RecoveryPoint(r,r.curveAt);var heading=p-RecoveryPoint(r,Mathf.Max(0,r.curveAt-.01f));heading.y=0;
   v.model.localPosition=p;v.model.localRotation=Quaternion.LookRotation(heading.sqrMagnitude>.0001f?heading:r.heading,Vector3.up);
   if(r.curveAt<1)return;
   v.hasTurned=true;v.routeDetached=true;v.routeVelocity=r.endHeading*r.speed;v.motionVelocity=v.routeVelocity;v.speed=v.baseTrafficSpeed=r.speed;v.recovery=null;
  }
 }
}
