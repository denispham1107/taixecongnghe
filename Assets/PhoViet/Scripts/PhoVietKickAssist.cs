using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  enum AssistedKickPhase { None,Approach,Swing,Return }
  AssistedKickPhase assistedKick;
  TrafficVehicle kickTarget;
  int kickTargetSpawn;
  float kickApproachAge,kickSwingAge;
  bool kickHit,kickOncoming;
  Vector3 kickAppliedOffset,kickPreviousRider;
  Quaternion kickPreviousRotation;
  public const float KickApproachLateralSpeed=32;
  const float KickSideSpeed=KickApproachLateralSpeed,KickSwingDuration=.30f,KickFootRadius=.08f,KickFootLead=.085f;
  public bool KickAssistActive=>assistedKick!=AssistedKickPhase.None;
  public bool KickSwingActive=>assistedKick==AssistedKickPhase.Swing;
  public float KickSwingPose=>Mathf.Clamp01(kickSwingAge/KickSwingDuration);
  public TrafficVehicle KickTarget=>kickTarget;
  public Vector3 LastKickFootPoint {get;private set;}
  public float LastKickCentreDistance {get;private set;}
  public float LastKickFootGap {get;private set;}=-1;
  void ResetAssistedKick()
  {assistedKick=AssistedKickPhase.None;kickTarget=null;kickAppliedOffset=Vector3.zero;kickHit=false;kickApproachAge=kickSwingAge=0;LastKickFootGap=-1;LastKickCentreDistance=0;}
  void CancelAssistedKick(bool returnToLane=true)
  {
   kickTarget=null;kickTime=0;
   assistedKick=returnToLane&&kickAppliedOffset.sqrMagnitude>.0001f?AssistedKickPhase.Return:AssistedKickPhase.None;
   if(!returnToLane)kickAppliedOffset=Vector3.zero;
  }
  Quaternion KickRoadFrame=>Quaternion.Euler(0,IsTurning?TurnYaw:0,0);
  bool KickTargetValid(TrafficVehicle v)
  {
   if(v==null||!v.active||v.knocked||v.wallCrashed||v.crossing||v.halfWidth>.7f||Mathf.Abs(v.model.localPosition.y-rider.localPosition.y)>.35f)return false;
   return Mathf.Abs(Vector3.Dot(v.model.forward,transform.TransformDirection(KickRoadFrame*Vector3.forward)))>.75f;
  }
  Vector3 KickTrafficVelocity(TrafficVehicle v)=>v.routeDetached&&!v.knocked&&!v.wallCrashed?v.routeVelocity:BodyVelocity(v);
  bool BeginAssistedKick()
  {
   if(State!=RideState.Riding||ServiceStopped||RiderJumpActive||RiderOnRoof||KickAssistActive||kickCooldown>0)return false;
   var animator=rider.GetComponent<BikeVisualAnimator>();if(animator==null||!animator.HasKickContactPose)return false;
   TrafficVehicle selected=null;float closest=float.MaxValue;Quaternion inverse=Quaternion.Inverse(KickRoadFrame);
   foreach(var v in vehicles)
   {
    if(!KickTargetValid(v))continue;
    Vector3 d=inverse*(v.model.localPosition-rider.localPosition),velocity=inverse*KickTrafficVelocity(v);
    bool oncoming=velocity.z<0;
    if(Mathf.Abs(d.x)<.75f||Mathf.Abs(d.x)>3.65f||(!oncoming&&Mathf.Abs(d.z)>2)||(oncoming&&(d.z<0||d.z>16)))continue;
    float gap=v.halfWidth+.50f,approach=Mathf.Max(0,Mathf.Abs(d.x)-gap)/KickSideSpeed;
    float intercept=KickInterceptSpeed(v,d,WorkSpeedLimit(Mathf.Max(Speed,TrafficPaceSpeed)),approach+KickFootLead);
    float relative=velocity.z-intercept,arrival=d.z+relative*(approach+KickFootLead);
    // Never chase an oncoming bike that will have passed before the foot extends.
    if(oncoming&&arrival<.4f-v.halfLength-.15f)continue;
    if(!oncoming&&(arrival<-.6f||arrival>1.45f))continue;
    Vector3 destination=rider.localPosition+KickRoadFrame*new Vector3(d.x-Mathf.Sign(d.x)*gap,0,0);
    if(!KickAssistPathClear(rider.localPosition,destination,Mathf.Max(.04f,approach+KickFootLead),intercept))continue;
    float score=Mathf.Abs(d.z)+Mathf.Abs(d.x)*.12f-(v.isReckless?5:0);
    if(score<closest){selected=v;closest=score;kickOncoming=oncoming;}
   }
   if(selected==null){notice="KHÔNG CÓ XE MÁY CÓ THỂ ÁP SÁT AN TOÀN";noticeTimer=1;return false;}
   kickTarget=selected;kickTargetSpawn=selected.spawnGeneration;kickDirection=Mathf.Sign((inverse*(selected.model.localPosition-rider.localPosition)).x);
   kickApproachAge=kickSwingAge=0;kickHit=false;assistedKick=AssistedKickPhase.Approach;kickCooldown=.8f;
   return true;
  }
  float KickInterceptSpeed(TrafficVehicle target,Vector3 delta,float desired,float lead)
  {
   Quaternion inverse=Quaternion.Inverse(KickRoadFrame);float targetSpeed=(inverse*KickTrafficVelocity(target)).z;
   float cap=ServiceRearProtected?desired:Mathf.Max(desired,TrafficPaceSpeed*BoostMultiplier);
   if(targetSpeed<0)return Mathf.Min(cap,Mathf.Max(desired,TrafficPaceSpeed*1.8f));
   return Mathf.Clamp(targetSpeed+(delta.z-.35f)/Mathf.Max(.025f,lead),2,cap);
  }
  float AssistedKickSpeed(float desired,float previous,float dt)
  {
   if(assistedKick==AssistedKickPhase.Return||KickAssistActive&&kickHit)return Mathf.MoveTowards(previous,desired,120*dt);
   if((assistedKick!=AssistedKickPhase.Approach&&assistedKick!=AssistedKickPhase.Swing)||!KickTargetValid(kickTarget))return desired;
   Quaternion q=KickRoadFrame,inverse=Quaternion.Inverse(q);Vector3 d=inverse*(kickTarget.model.localPosition-rider.localPosition);
   float sideGap=kickTarget.halfWidth+.50f,sideTime=Mathf.Max(0,Mathf.Abs(d.x)-sideGap)/KickSideSpeed;
   float lead=assistedKick==AssistedKickPhase.Approach?sideTime+KickFootLead:Mathf.Max(.025f,KickFootLead-kickSwingAge);
   float intercept=KickInterceptSpeed(kickTarget,d,desired,lead);
   float proposed=Mathf.MoveTowards(previous,intercept,(intercept>previous?400:600)*dt);
   Vector3 candidate=rider.localPosition+q*Vector3.right*Mathf.Clamp(d.x-kickDirection*sideGap,-KickSideSpeed*dt,KickSideSpeed*dt);
   if(!KickAssistPathClear(rider.localPosition,candidate,dt,proposed)){CancelAssistedKick();return Mathf.Min(previous,desired);}
   return proposed;
  }
  bool KickAssistPathClear(Vector3 from,Vector3 to,float horizon,float roadSpeed=-1)
  {
   if(roadSpeed<0)roadSpeed=Speed;
   Quaternion q=KickRoadFrame;Vector3 f=q*Vector3.forward,r=q*Vector3.right;
   Vector2 body=new Vector2(Mathf.Abs(r.x)*.33f+Mathf.Abs(f.x)*.91f,Mathf.Abs(r.z)*.33f+Mathf.Abs(f.z)*.91f)+new Vector2(.04f,.04f);
   foreach(var v in vehicles)
   {
    if(!v.active||v.model.localPosition.y>2.4f)continue;
    Vector3 future=v.model.localPosition+(KickTrafficVelocity(v)-Vector3.forward*roadSpeed)*horizon;
    if(SweptOverlap(Flat(v.model.localPosition-from),Flat(future-to),VehicleFootprint(v)+body))return false;
   }
   if(SceneryContact(from,to,out _,out _,out _))return false;
   return true;
  }
  void TickAssistedKickMotion(float dt,Vector3 previous,Quaternion previousRotation)
  {
   kickPreviousRider=previous;kickPreviousRotation=previousRotation;
   if(!KickAssistActive)return;
   Vector3 baseline=rider.localPosition,oldOffset=kickAppliedOffset;
   if(ServiceStopped||RiderJumpActive||RiderOnRoof){CancelAssistedKick();}
   if(assistedKick==AssistedKickPhase.Approach||assistedKick==AssistedKickPhase.Swing&&!kickHit)
   {
    if(!KickTargetValid(kickTarget)||kickTarget.spawnGeneration!=kickTargetSpawn)CancelAssistedKick();
   }
   if(assistedKick==AssistedKickPhase.Approach||assistedKick==AssistedKickPhase.Swing&&!kickHit)
   {
    kickApproachAge+=dt;Quaternion q=KickRoadFrame,inverse=Quaternion.Inverse(q);
    Vector3 nextTarget=kickTarget.model.localPosition+(KickTrafficVelocity(kickTarget)-Vector3.forward*Speed)*dt;
    Vector3 d=inverse*(nextTarget-baseline);float gap=kickTarget.halfWidth+.50f;
    float desired=d.x-kickDirection*gap;
    Vector3 offset=q*Vector3.right*Mathf.MoveTowards(Vector3.Dot(oldOffset,q*Vector3.right),desired,KickSideSpeed*dt);
    Vector3 candidate=baseline+offset;
    if(kickApproachAge>.55f||!KickAssistPathClear(previous,candidate,dt)){CancelAssistedKick();}
    else
    {
     kickAppliedOffset=offset;rider.localPosition=candidate;
     Vector3 beside=inverse*(nextTarget-candidate);float relative=(inverse*KickTrafficVelocity(kickTarget)).z-Speed;
     bool nearSide=Mathf.Abs(Mathf.Abs(beside.x)-gap)<.07f;
     bool timed=kickOncoming?Mathf.Abs(beside.z+relative*KickFootLead-.45f)<=kickTarget.halfLength+.25f:Mathf.Abs(beside.z-.4f)<=.9f;
     if(assistedKick==AssistedKickPhase.Approach&&nearSide&&timed)
     {assistedKick=AssistedKickPhase.Swing;kickSwingAge=0;rider.GetComponent<BikeVisualAnimator>()?.Kick(kickDirection);}
     else if(kickOncoming&&beside.z< -kickTarget.halfLength-.2f)CancelAssistedKick();
    }
   }
   if(assistedKick==AssistedKickPhase.Swing)rider.localPosition=baseline+kickAppliedOffset;
   if(assistedKick==AssistedKickPhase.Return)
   {
    Vector3 offset=Vector3.MoveTowards(oldOffset,Vector3.zero,KickSideSpeed*dt),candidate=baseline+offset;
    if(KickAssistPathClear(previous,candidate,dt))kickAppliedOffset=offset;
    rider.localPosition=baseline+kickAppliedOffset;
    if(kickAppliedOffset.sqrMagnitude<.00001f){kickAppliedOffset=Vector3.zero;assistedKick=AssistedKickPhase.None;}
   }
  }
  static bool KickSegmentBox(Vector3 from,Vector3 to,Vector3 half,out float at)
  {
   float enter=0,leave=1;Vector3 delta=to-from;at=0;
   for(int axis=0;axis<3;axis++)
   {
    if(Mathf.Abs(delta[axis])<.000001f){if(Mathf.Abs(from[axis])>half[axis])return false;continue;}
    float a=(-half[axis]-from[axis])/delta[axis],b=(half[axis]-from[axis])/delta[axis];enter=Mathf.Max(enter,Mathf.Min(a,b));leave=Mathf.Min(leave,Mathf.Max(a,b));if(enter>leave)return false;
   }
   at=enter;return true;
  }
  void TickAssistedKickContact(float dt)
  {
   if(!KickSwingActive)return;
   float before=kickSwingAge;kickSwingAge=Mathf.Min(KickSwingDuration,kickSwingAge+dt);kickTime=.38f*(1-KickSwingPose);
   if(!kickHit&&(!KickTargetValid(kickTarget)||kickTarget.spawnGeneration!=kickTargetSpawn)){CancelAssistedKick();return;}
   if(!kickHit)
   {
    var animator=rider.GetComponent<BikeVisualAnimator>();
    // Subsample the actual animated shoe and the moving target. A fast oncoming
    // bike can cross the whole foot window within one rendered frame.
    int samples=Mathf.Max(1,Mathf.CeilToInt(dt/.008f));
    for(int i=1;i<=samples&&!kickHit;i++)
    {
     float a=(float)(i-1)/samples,b=(float)i/samples,age0=Mathf.Lerp(before,kickSwingAge,a),age1=Mathf.Lerp(before,kickSwingAge,b);
     if(age1/KickSwingDuration<.18f||age0/KickSwingDuration>.78f)continue;
     Quaternion targetRotation=Quaternion.Euler(0,kickTarget.model.localEulerAngles.y,0),inverse=Quaternion.Inverse(targetRotation);
     Vector3 target0=Vector3.Lerp(kickTarget.previousPosition,kickTarget.model.localPosition,a),target1=Vector3.Lerp(kickTarget.previousPosition,kickTarget.model.localPosition,b);
     Vector3 root0=Vector3.Lerp(kickPreviousRider,rider.localPosition,a),root1=Vector3.Lerp(kickPreviousRider,rider.localPosition,b);
     Quaternion q0=Quaternion.Slerp(kickPreviousRotation,rider.localRotation,a),q1=Quaternion.Slerp(kickPreviousRotation,rider.localRotation,b);
     for(int toe=0;toe<2&&!kickHit;toe++)
     {
      Vector3 shoe0=root0+q0*animator.KickFootLocal(kickDirection,age0/KickSwingDuration,toe==1),shoe1=root1+q1*animator.KickFootLocal(kickDirection,age1/KickSwingDuration,toe==1);
      Vector3 local0=inverse*(shoe0-target0)-Vector3.up*.95f,local1=inverse*(shoe1-target1)-Vector3.up*.95f;
      // Narrow torso/fairing contact surface excludes the empty handlebar envelope.
      Vector3 half=new Vector3(Mathf.Min(kickTarget.halfWidth,.23f),.65f,Mathf.Min(kickTarget.halfLength,.75f))+Vector3.one*KickFootRadius;
      if(!KickSegmentBox(local0,local1,half,out float at))continue;
      LastKickFootPoint=Vector3.Lerp(shoe0,shoe1,at);LastKickCentreDistance=Vector3.Distance(rider.localPosition,kickTarget.model.localPosition);
      Vector3 local=Vector3.Lerp(local0,local1,at),bodyHalf=half-Vector3.one*KickFootRadius;
      LastKickFootGap=(local-new Vector3(Mathf.Clamp(local.x,-bodyHalf.x,bodyHalf.x),Mathf.Clamp(local.y,-bodyHalf.y,bodyHalf.y),Mathf.Clamp(local.z,-bodyHalf.z,bodyHalf.z))).magnitude;
      ApplyAssistedKickImpact();
     }
    }
   }
   if(kickSwingAge>=KickSwingDuration){kickTarget=null;assistedKick=AssistedKickPhase.Return;}
  }
  void ApplyAssistedKickImpact()
  {
   kickHit=true;KickCount++;bool selfDefense=kickTarget.isReckless;Vector3 velocity=KickTrafficVelocity(kickTarget);
   KnockVehicle(kickTarget,kickDirection,false);kickTarget.knockVelocity=velocity+KickRoadFrame*Vector3.right*kickDirection*9;kickTarget.knockVelocity.y=2.2f;
   if(!selfDefense)StartChase();
   notice=selfDefense?"TỰ VỆ: HẠ QUÁI XẾ · KHÔNG GỌI CSGT":kickDirection>0?"ĐẠP PHẢI!  CSGT ĐANG ĐẾN":"ĐẠP TRÁI!  CSGT ĐANG ĐẾN";noticeTimer=1.5f;
  }
 }
}
