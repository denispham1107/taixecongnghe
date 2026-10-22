using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  enum AssistedKickPhase { None,Approach,Swing,Return }
  AssistedKickPhase assistedKick;
  TrafficVehicle kickTarget;
  int kickTargetSpawn;
  float kickApproachAge,kickSwingAge,kickBalance;
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
  {assistedKick=AssistedKickPhase.None;kickTarget=null;kickAppliedOffset=Vector3.zero;kickHit=false;kickApproachAge=kickSwingAge=kickBalance=0;LastKickFootGap=-1;LastKickCentreDistance=0;}
  void CancelAssistedKick(bool returnToLane=true)
  {
   kickTarget=null;kickTime=0;
   assistedKick=returnToLane&&kickAppliedOffset.sqrMagnitude>.0001f?AssistedKickPhase.Return:AssistedKickPhase.None;
   if(!returnToLane)kickAppliedOffset=Vector3.zero;
  }
  Quaternion KickRoadFrame=>Quaternion.Euler(0,IsTurning?TurnYaw:0,0);
  bool KickTargetValid(TrafficVehicle v)
  {
   return v!=null&&v.active&&!v.knocked&&!v.wallCrashed&&v.halfWidth<=.7f&&Mathf.Abs(v.model.localPosition.y-rider.localPosition.y)<=.35f;
  }
  float KickSideGap(TrafficVehicle v)
  {
   Vector3 side=Quaternion.Inverse(v.model.localRotation)*(KickRoadFrame*Vector3.right);
   return Mathf.Abs(side.x)*v.halfWidth+Mathf.Abs(side.z)*v.halfLength+Mathf.Max(.40f,.50f-.14f*Mathf.Abs(side.z));
  }
  float KickAimForward(TrafficVehicle v,float side)
  {
   Quaternion inverse=Quaternion.Inverse(v.model.localRotation);Vector3 f=inverse*(KickRoadFrame*Vector3.forward);
   Vector3 p=inverse*(KickRoadFrame*Vector3.right)*(side*(.74f-KickSideGap(v)));
   float lo=-2,hi=2;
   for(int i=0;i<2;i++)
   {
    int axis=i==0?0:2;float half=(i==0?Mathf.Min(v.halfWidth,.23f):Mathf.Min(v.halfLength,.75f))+.06f;
    if(Mathf.Abs(f[axis])<.0001f)continue;
    float a=(-half-p[axis])/f[axis],b=(half-p[axis])/f[axis];lo=Mathf.Max(lo,Mathf.Min(a,b));hi=Mathf.Min(hi,Mathf.Max(a,b));
   }
   float near=Mathf.Clamp(0,lo,Mathf.Max(lo,hi));
   return .54f-Mathf.Lerp(near,(lo+hi)*.5f,.15f);
  }
  float KickTravelRatio()
  {
   if(!IsTurning||kickAppliedOffset.sqrMagnitude<.00001f)return 1;
   float arc=Mathf.Min(turnLength,turnTravel+.2f);int i=1;while(i<turnArcLengths.Length-1&&turnArcLengths[i]<arc)i++;
   float t=(i-1+Mathf.InverseLerp(turnArcLengths[i-1],turnArcLengths[i],arc))/(turnArcLengths.Length-1);
   Vector3 tangent=TurnTangent(t);float yaw=Mathf.Atan2(tangent.x,tangent.z)*Mathf.Rad2Deg;
   float curvature=Mathf.DeltaAngle(TurnYaw,yaw)*Mathf.Deg2Rad/.2f;
   return Mathf.Clamp(1-curvature*Vector3.Dot(kickAppliedOffset,KickRoadFrame*Vector3.right),.35f,2);
  }
  // Predict the same curved baseline used by BeginOrContinueTurn. World scroll
  // remains along Z even while the rider's direction is turning through 90°.
  Vector3 KickBaselineAdvance(float distance)
  {
   if(!IsTurning)return Vector3.zero;
   float travel=turnTravel+distance,arc=Mathf.Min(turnLength,travel);int i=1;
   while(i<turnArcLengths.Length-1&&turnArcLengths[i]<arc)i++;
   float t=(i-1+Mathf.InverseLerp(turnArcLengths[i-1],turnArcLengths[i],arc))/(turnArcLengths.Length-1);
   return TurnPoint(t)+Vector3.right*turnSide*Mathf.Max(0,travel-turnLength)-Vector3.forward*travel-(rider.localPosition-kickAppliedOffset);
  }
  void RebaseAssistedKick(Quaternion inverse,Vector3 origin)
  {
   kickAppliedOffset=inverse*kickAppliedOffset;kickPreviousRider=inverse*(kickPreviousRider-origin);kickPreviousRotation=inverse*kickPreviousRotation;
   LastKickFootPoint=inverse*(LastKickFootPoint-origin);
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
    bool oncoming=velocity.z< -1;
    bool parallel=Mathf.Abs(Vector3.Dot(v.model.forward,transform.TransformDirection(KickRoadFrame*Vector3.forward)))>.75f;
    if(Mathf.Abs(d.x)<.75f||Mathf.Abs(d.x)>3.65f||(parallel&&!oncoming&&Mathf.Abs(d.z)>2)||((oncoming||!parallel)&&(d.z<0||d.z>16)))continue;
    float gap=KickSideGap(v),approach=Mathf.Max(0,Mathf.Abs(d.x)-gap)/KickSideSpeed;
    float intercept=KickInterceptSpeed(v,d,WorkSpeedLimit(Mathf.Max(Speed,TrafficPaceSpeed)),approach+KickFootLead);
    float relative=velocity.z-intercept,arrival=d.z+relative*(approach+KickFootLead);
    // Never chase an oncoming bike that will have passed before the foot extends.
    if(oncoming&&arrival<.4f-v.halfLength-.15f)continue;
    if(!oncoming&&(arrival<-.6f||arrival>1.45f))continue;
    float lead=approach+KickFootLead;
    Vector3 ahead=v.model.localPosition+(KickTrafficVelocity(v)-Vector3.forward*intercept)*lead;
    Vector3 baseline=rider.localPosition+KickBaselineAdvance(intercept*lead);
    float side=(inverse*(ahead-baseline)).x;
    if(Mathf.Sign(side)!=Mathf.Sign(d.x))continue;
    Vector3 destination=baseline+KickRoadFrame*Vector3.right*(side-Mathf.Sign(d.x)*gap);
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
   if(targetSpeed< -1)return Mathf.Min(cap,Mathf.Max(desired,TrafficPaceSpeed*1.8f));
   return Mathf.Clamp((targetSpeed+(delta.z-KickAimForward(target,Mathf.Sign(delta.x)))/Mathf.Max(.025f,lead))/KickTravelRatio(),2,cap);
  }
  float AssistedKickSpeed(float desired,float previous,float dt)
  {
   if(assistedKick==AssistedKickPhase.Return||KickAssistActive&&kickHit)return KickReturnSpeed(Mathf.MoveTowards(previous,desired,120*dt),dt);
   if((assistedKick!=AssistedKickPhase.Approach&&assistedKick!=AssistedKickPhase.Swing)||!KickTargetValid(kickTarget))return desired;
   Quaternion q=KickRoadFrame,inverse=Quaternion.Inverse(q);Vector3 d=inverse*(kickTarget.model.localPosition-rider.localPosition);
   float sideGap=KickSideGap(kickTarget),sideTime=Mathf.Max(0,Mathf.Abs(d.x)-sideGap)/KickSideSpeed;
   float lead=assistedKick==AssistedKickPhase.Approach?sideTime+KickFootLead:Mathf.Max(.025f,KickFootLead-kickSwingAge);
   float intercept=KickInterceptSpeed(kickTarget,d,desired,lead);
   float proposed=Mathf.MoveTowards(previous,intercept,(intercept>previous?400:600)*dt);
   Vector3 advance=KickBaselineAdvance(proposed*dt);
   Vector3 next=kickTarget.model.localPosition+(KickTrafficVelocity(kickTarget)-Vector3.forward*proposed)*dt;
   float nextSide=(inverse*(next-rider.localPosition-advance)).x;
   Vector3 candidate=rider.localPosition+advance+q*Vector3.right*Mathf.Clamp(nextSide-kickDirection*sideGap,-KickSideSpeed*dt,KickSideSpeed*dt);
   if(!KickAssistPathClear(rider.localPosition,candidate,dt,proposed)){CancelAssistedKick();return KickReturnSpeed(Mathf.Min(previous,desired),dt);}
   return proposed;
  }
  float KickReturnSpeed(float desired,float dt)
  {
   Vector3 delta=Vector3.MoveTowards(kickAppliedOffset,Vector3.zero,KickSideSpeed*dt)-kickAppliedOffset;
   for(int i=0;i<3;i++)
   {
    float speed=i==0?desired:i==1?0:TrafficPaceSpeed*BoostMultiplier;
    Vector3 candidate=rider.localPosition+KickBaselineAdvance(speed*dt)+delta;
    if(KickAssistPathClear(rider.localPosition,candidate,dt,speed))return speed;
   }
   return 0;
  }
  bool KickAssistPathClear(Vector3 from,Vector3 to,float horizon,float roadSpeed=-1)
  {
   if(roadSpeed<0)roadSpeed=Speed;
   Quaternion q=KickRoadFrame;
   foreach(var v in vehicles)
   {
    if(!v.active||v.model.localPosition.y>2.4f)continue;
    Vector3 future=v.model.localPosition+(KickTrafficVelocity(v)-Vector3.forward*roadSpeed)*horizon;
    if(KickBodySweep(v,from,to,future,q,horizon))return false;
   }
   if(SceneryContact(from,to,out _,out _,out _))return false;
   return true;
  }
  static bool KickBodySweep(TrafficVehicle v,Vector3 from,Vector3 to,Vector3 future,Quaternion frame,float horizon)
  {
   Vector2 right=Flat(frame*Vector3.right),forward=Flat(frame*Vector3.forward);
   Vector2 delta=Flat(future-to)-Flat(v.model.localPosition-from),offset=Flat(v.model.localPosition-from);
   float enter=0,leave=1;
   for(int i=0;i<4;i++)
   {
    Vector2 axis=i==0?right:i==1?forward:i==2?BodyRight(v):BodyForward(v);
    float radius=ProjectedRadius(v,axis)+Mathf.Abs(Vector2.Dot(right,axis))*.33f+Mathf.Abs(Vector2.Dot(forward,axis))*.91f+.04f;
    float growth=0;
    if(v.knocked&&v.halfWidth<.7f&&!v.physicalCrash)
    {
     float roll=Mathf.Clamp01((v.knockAge+horizon)/.42f)*82*Mathf.Deg2Rad;
     float width=Mathf.Lerp(v.halfWidth,1.05f,Mathf.Sin(roll));
     growth=Mathf.Max(0,width-BodyWidth(v))*Mathf.Abs(Vector2.Dot(BodyRight(v),axis));
    }
    float p=Vector2.Dot(offset,axis),d=Vector2.Dot(delta,axis);
    if(!ContactHalfSpace(p-radius,d-growth,ref enter,ref leave)||!ContactHalfSpace(-p-radius,-d-growth,ref enter,ref leave))return false;
   }
   return leave>=0&&enter<=1;
  }
  void TickAssistedKickMotion(float dt,Vector3 previous,Quaternion previousRotation)
  {
   kickPreviousRider=previous;kickPreviousRotation=previousRotation;
   // Stabilise the bike before extending a leg. Banking into a left turn used
   // to pull the right shoe inward by ~30 cm, beyond physical contact range.
   kickBalance=Mathf.MoveTowards(kickBalance,assistedKick==AssistedKickPhase.Approach||assistedKick==AssistedKickPhase.Swing?1:0,dt*12);
   if(kickBalance>0){var angles=rider.localEulerAngles;angles.z=Mathf.DeltaAngle(0,angles.z)*(1-kickBalance);rider.localRotation=Quaternion.Euler(angles);}
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
    Vector3 d=inverse*(nextTarget-baseline);float gap=KickSideGap(kickTarget);
    float desired=d.x-kickDirection*gap;
    Vector3 offset=q*Vector3.right*Mathf.MoveTowards(Vector3.Dot(oldOffset,q*Vector3.right),desired,KickSideSpeed*dt);
    Vector3 candidate=baseline+offset;
    if(kickApproachAge>.55f||!KickAssistPathClear(previous,candidate,dt)){CancelAssistedKick();}
    else
    {
     kickAppliedOffset=offset;rider.localPosition=candidate;
     Vector3 beside=inverse*(nextTarget-candidate);float relative=(inverse*KickTrafficVelocity(kickTarget)).z-Speed*KickTravelRatio();
     bool nearSide=Mathf.Abs(Mathf.Abs(beside.x)-gap)<.07f;
     float aim=KickAimForward(kickTarget,kickDirection);
     bool timed=kickOncoming?Mathf.Abs(beside.z+relative*KickFootLead-aim)<=kickTarget.halfLength+.25f:Mathf.Abs(beside.z-aim)<=.9f;
     if(assistedKick==AssistedKickPhase.Approach&&nearSide&&timed)
     {assistedKick=AssistedKickPhase.Swing;kickSwingAge=0;rider.GetComponent<BikeVisualAnimator>()?.Kick(kickDirection);}
     else if(kickOncoming&&beside.z< -kickTarget.halfLength-.2f)CancelAssistedKick();
    }
   }
   if(assistedKick==AssistedKickPhase.Swing)rider.localPosition=baseline+kickAppliedOffset;
   if(assistedKick==AssistedKickPhase.Return||assistedKick==AssistedKickPhase.Swing&&kickHit)
   {
    Vector3 offset=Vector3.MoveTowards(oldOffset,Vector3.zero,KickSideSpeed*dt),candidate=baseline+offset;
    if(KickAssistPathClear(previous,candidate,dt))kickAppliedOffset=offset;
    rider.localPosition=baseline+kickAppliedOffset;
    if(kickAppliedOffset.sqrMagnitude<.00001f){kickAppliedOffset=Vector3.zero;if(assistedKick==AssistedKickPhase.Return)assistedKick=AssistedKickPhase.None;}
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
   Vector3 away=KickRoadFrame*Vector3.right*kickDirection;float outward=Vector3.Dot(velocity,away);
   KnockVehicle(kickTarget,kickDirection,false);kickTarget.knockVelocity=velocity+away*(Mathf.Max(9,outward+9)-outward);kickTarget.knockVelocity.y=2.2f;
   if(!selfDefense)StartChase();
   notice=selfDefense?"TỰ VỆ: HẠ QUÁI XẾ · KHÔNG GỌI CSGT":kickDirection>0?"ĐẠP PHẢI!  CSGT ĐANG ĐẾN":"ĐẠP TRÁI!  CSGT ĐANG ĐẾN";noticeTimer=1.5f;
  }
 }
}
