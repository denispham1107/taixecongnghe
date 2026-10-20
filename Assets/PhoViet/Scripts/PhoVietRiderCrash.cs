using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public int LivesRemaining {get;private set;}=3;
  public float RiderRecoveryProgress {get;private set;}
  public float RecoveryTrafficYieldRemaining=>recoveryTrafficYieldRemaining;
  public Vector3 RiderFallVelocity=>riderFallVelocity;
  public float RiderFallTravel {get;private set;}
  public float LastRiderImpactSpeed {get;private set;}
  public float RiderFallRoll=>fallTargetRoll;
  public bool RiderCrashActive=>State==RideState.Falling||State==RideState.Recovering||State==RideState.Paused&&(pauseFromState==RideState.Falling||pauseFromState==RideState.Recovering);
  RideState pauseFromState=RideState.Riding;
  TrafficVehicle riderBody;
  Vector3 riderFallVelocity,riderFallEuler,riderFallSpin,riderRoadVelocity,fallTurnExit;
  Quaternion recoveryRotation;
  float settledTime,fallTargetRoll,fallTargetPitch,recoveryTrafficYieldRemaining,oldRiderYaw;
  float[] riderContactUntil;
  int crashLane;
  bool riderVelocityValid;
  const float RiderGroundDrag=9f,GetUpDuration=.65f;
  void ResetRiderImpact()
  {
   LivesRemaining=3;RiderRecoveryProgress=0;RiderFallTravel=0;LastRiderImpactSpeed=0;
   fallTime=0;settledTime=0;recoveryTrafficYieldRemaining=0;pauseFromState=RideState.Riding;
   riderFallVelocity=Vector3.zero;riderFallSpin=Vector3.zero;riderRoadVelocity=Vector3.zero;riderVelocityValid=false;
   riderContactUntil=new float[vehicles.Count];
   riderBody=new TrafficVehicle{model=rider,halfWidth=.33f,halfLength=.91f};
  }
  void SaveRideRecord()
  {
   if((int)Distance<=BestDistance)return;
   BestDistance=(int)Distance;PlayerPrefs.SetInt(BestKey,BestDistance);PlayerPrefs.Save();
  }
  void BeginRiderImpact(TrafficVehicle hit,Vector2 normal,Vector3 contact,Vector3 before)
  {
   if(State!=RideState.Riding)return;
   LivesRemaining=Mathf.Max(0,LivesRemaining-1);crashLane=CurrentLane;
   turnCrash=IsTurning;crashTurnPosition=rider.localPosition;crashTurnRotation=Quaternion.Euler(0,IsTurning?TurnYaw:0,0);
   if(IsTurning)fallTurnExit=(lateTurn?lateEnd:turnStart+new Vector3(turnSide*turnRadius,0,turnRadius))-Vector3.forward*turnTravel;
   QueuedTurn=0;fallTime=0;settledTime=0;RiderFallTravel=0;RiderRecoveryProgress=0;
   riderFallVelocity=before;riderFallEuler=rider.localEulerAngles;
   riderFallEuler.x=Mathf.DeltaAngle(0,riderFallEuler.x);riderFallEuler.z=Mathf.DeltaAngle(0,riderFallEuler.z);riderFallSpin=Vector3.zero;
   before.y=jumpVelocity;ResetJump();
   State=RideState.Falling;riderBody.knocked=true;ApplyRiderImpulse(hit,normal,contact,before);
   if(hit!=null){int index=vehicles.IndexOf(hit);if(index>=0)riderContactUntil[index]=.35f;}
   Speed=0;engine?.Stop();navigationAudio?.Stop();kickTime=0;
   if(sound)fx?.PlayOneShot(crashClip);SaveRideRecord();
  }
  void ApplyRiderImpulse(TrafficVehicle hit,Vector2 normal,Vector3 contact,Vector3 before)
  {
   Vector3 other=hit==null?Vector3.zero:BodyVelocity(hit);
   Vector2 relative=Flat(other-before);
   float closing=-Vector2.Dot(relative,normal);
   // Initial overlap can lack a separating entry face. Use the actual approach
   // direction instead of an arbitrary side, including for very fast swept hits.
   if(closing<.35f&&relative.sqrMagnitude>.1f){normal=-relative.normalized;closing=relative.magnitude;}
   closing=Mathf.Max(.35f,closing);LastRiderImpactSpeed=closing;
   float invA=1f/220,invB=hit!=null&&hit.wallCrashed?0:1f/(hit==null?1350:VehicleMass(hit));
   Vector2 lever=Flat(contact-rider.localPosition),otherLever=hit==null?Vector2.zero:Flat(contact-hit.model.localPosition);
   float torque=LeverCross(lever,normal),otherTorque=LeverCross(otherLever,normal);
   float inertia=220*(4*.33f*.33f+4*.91f*.91f)/12;
   float denominator=invA+invB+torque*torque/inertia+(hit==null||hit.wallCrashed?0:otherTorque*otherTorque/BodyInertia(hit));
   float impulse=1.12f*closing/denominator;
   Vector2 tangent=new Vector2(-normal.y,normal.x);
   float friction=Mathf.Clamp(-Vector2.Dot(relative,tangent)/(invA+invB),-impulse*.24f,impulse*.24f);
   Vector2 push=normal*impulse+tangent*friction;
   Vector3 after=before-new Vector3(push.x,0,push.y)*invA;
   Vector2 horizontal=Vector2.ClampMagnitude(Flat(after),28);
   float deltaV=Flat(after-before).magnitude;
   riderFallVelocity=new Vector3(horizontal.x,Mathf.Max(before.y,Mathf.Clamp((deltaV-3)*.13f,0,2.8f)),horizontal.y);
   Vector3 localDelta=Quaternion.Inverse(crashTurnRotation)*(after-before);
   Vector3 localContact=Quaternion.Inverse(crashTurnRotation)*(contact-rider.localPosition);
   float side=Mathf.Abs(localDelta.x)>.25f?-Mathf.Sign(localDelta.x):Mathf.Abs(localContact.x)>.08f?Mathf.Sign(localContact.x):0;
   fallTargetRoll=side*78;fallTargetPitch=side==0?Mathf.Clamp(-localDelta.z*4,-78,78):Mathf.Clamp(-localDelta.z*1.5f,-24,24);
   if(Mathf.Abs(fallTargetRoll)+Mathf.Abs(fallTargetPitch)<20)fallTargetPitch=55;
   riderFallSpin+=new Vector3(Mathf.Clamp(-localDelta.z*2.5f,-110,110),Mathf.Clamp(-LeverCross(lever,push)/inertia*Mathf.Rad2Deg,-180,180),Mathf.Clamp(-localDelta.x*9,-170,170));
   if(hit!=null&&!hit.wallCrashed)
   {
    Vector3 otherAfter=other+new Vector3(push.x,0,push.y)*invB;
    ActivateCollisionBody(hit,other,otherAfter,push,otherLever,false,!hit.knocked,closing);TrafficCollisions++;
   }
   settledTime=0;
  }
  void TickRiderCrash(float dt)
  {
   if(dt<=0||!RiderCrashActive||State==RideState.Paused)return;
   bool gettingUp=State==RideState.Recovering;
   fallTime+=dt;timeRiding+=dt;TickActionTimers(dt);
   Vector3 previous=rider.localPosition;
   Speed=gettingUp?0:Mathf.Max(0,riderFallVelocity.z);
   float scroll=Speed*dt;Distance+=scroll;ScrollRoutes(scroll);
   if(turnCrash){fallTurnExit-=Vector3.forward*scroll;crashTurnPosition-=Vector3.forward*scroll;}
   if(!gettingUp)
   {
    riderFallVelocity.y-=9.81f*dt;
    Vector3 p=previous+(riderFallVelocity-Vector3.forward*Speed)*dt;
    bool grounded=p.y<=0;
    if(grounded)
    {
     p.y=0;riderFallVelocity.y=0;
     Vector2 velocity=Vector2.MoveTowards(Flat(riderFallVelocity),Vector2.zero,RiderGroundDrag*dt);
     riderFallVelocity.x=velocity.x;riderFallVelocity.z=velocity.y;
    }
    riderFallEuler+=riderFallSpin*dt;riderFallSpin*=Mathf.Exp(-dt*(grounded?3f:1f));
    riderFallEuler.z=Mathf.MoveTowards(riderFallEuler.z,fallTargetRoll,dt*160);
    riderFallEuler.x=Mathf.MoveTowards(riderFallEuler.x,fallTargetPitch,dt*130);
    rider.localRotation=Quaternion.Euler(riderFallEuler);
    float edge=9.3f-VehicleFootprint(riderBody).x;
    float road=Mathf.Repeat(RoadProgress+p.z,VietnamStreet.Length);
    if(road<58.4f&&Mathf.Abs(p.x)>edge)
    {p.x=Mathf.Sign(p.x)*edge;riderFallVelocity.x=-riderFallVelocity.x*.12f;riderFallSpin.y*=.4f;}
    RiderFallTravel+=Flat(p-previous+Vector3.forward*scroll).magnitude;rider.localPosition=p;
   }
   TickTrafficMotion(dt);
   ResolveFallenRiderContacts(previous);
   TickChase(dt);RetireTraffic(false);
   if(State==RideState.Arrested)return;
   if(!IsTurning)EndRouteFrame(dt);
   if(State==RideState.Recovering)
   {
    RiderRecoveryProgress=Mathf.Min(1,RiderRecoveryProgress+dt/GetUpDuration);
    rider.localRotation=Quaternion.Slerp(recoveryRotation,crashTurnRotation,Mathf.SmoothStep(0,1,RiderRecoveryProgress));
    if(RiderRecoveryProgress>=1)FinishRiderRecovery();
   }
   else if(rider.localPosition.y<=.001f&&riderFallVelocity.sqrMagnitude<.1f&&fallTime>.4f)
   {
    riderFallVelocity=Vector3.zero;Speed=0;settledTime+=dt;
    if(LivesRemaining==0&&settledTime>=.7f){State=RideState.GameOver;SaveRideRecord();siren?.Stop();}
    else if(LivesRemaining>0&&settledTime>=1.2f)
    {State=RideState.Recovering;RiderRecoveryProgress=0;recoveryRotation=rider.localRotation;BeginRecoveryTrafficYield();}
   }
  }
  void ResolveFallenRiderContacts(Vector3 previous)
  {
   riderBody.previousPosition=previous;riderBody.knocked=true;
   for(int i=0;i<vehicles.Count;i++)
   {
    var v=vehicles[i];if(!v.active||riderContactUntil[i]>fallTime)continue;
    if(!VehicleContact(riderBody,v,out Vector2 normal,out float at,out float depth))continue;
    float closing=-Vector2.Dot(Flat(BodyVelocity(v)-riderFallVelocity),normal);
    if(closing<.5f)continue;
    if(at>0)rider.localPosition=Vector3.Lerp(previous,rider.localPosition,at);
    v.model.localPosition=Vector3.Lerp(v.previousPosition,v.model.localPosition,at);
    if(depth>0)rider.localPosition-=new Vector3(normal.x,0,normal.y)*(Mathf.Min(depth,.8f)+.02f);
    ApplyRiderImpulse(v,normal,(rider.localPosition+v.model.localPosition)*.5f,riderFallVelocity);
    riderContactUntil[i]=fallTime+.3f;State=RideState.Falling;
   }
  }
  void BeginRecoveryTrafficYield()
  {
   recoveryTrafficYieldRemaining=3;foreach(var v in vehicles)v.jobYieldLane=-1;
  }
  void FinishRiderRecovery()
  {
   float lateral=turnCrash?LaneX[crashLane]+Vector3.Dot(rider.localPosition-crashTurnPosition,crashTurnRotation*Vector3.right):rider.localPosition.x;
   int nearest=0;for(int i=1;i<4;i++)if(Mathf.Abs(LaneX[i]-lateral)<Mathf.Abs(LaneX[nearest]-lateral))nearest=i;
   CurrentLane=nearest;
   RebaseAfterRiderSlide();
   if(turnCrash&&IsTurning)
   {
    lateTurn=true;turnStart=rider.localPosition;turnStart.y=0;turnTravel=0;
    lateEnd=fallTurnExit+Quaternion.Euler(0,turnSide*90,0)*Vector3.right*(LaneX[nearest]-LaneX[crashLane]);
    float reach=Mathf.Max(1,Vector3.Distance(turnStart,lateEnd)*.3f);
    lateControl1=turnStart+crashTurnRotation*Vector3.forward*Mathf.Min(3,reach);
    lateControl2=lateEnd-Quaternion.Euler(0,turnSide*90,0)*Vector3.forward*Mathf.Min(6,reach);
    Vector3 last=turnStart;lateArcLengths[0]=0;
    for(int i=1;i<65;i++){Vector3 p=LateTurnPoint(i/64f);lateArcLengths[i]=lateArcLengths[i-1]+Vector3.Distance(last,p);last=p;}
    turnLength=Mathf.Max(.01f,lateArcLengths[64]);
   }
   rider.localRotation=crashTurnRotation;lastX=rider.localPosition.x;oldRiderZ=rider.localPosition.z;
   riderBody.knocked=false;State=RideState.Riding;BeginRecoveryTrafficYield();
   if(IsChased&&policeBike!=null)
   {
    float gap=Vector2.Distance(Flat(policeBike.localPosition),Flat(rider.localPosition));
    routePolice=IsTurning;policeRouteAt=Distance-gap;PoliceDistance=gap;
    if(routePolice){routeTrail.Clear();for(int i=0;i<=8;i++){float t=i/8f;routeTrail.Add(new RoutePoint(Mathf.Lerp(policeRouteAt,Distance,t),Vector3.Lerp(policeBike.localPosition,rider.localPosition,t),Mathf.LerpAngle(policeBike.localEulerAngles.y,TurnYaw,t)));}}
   }
   ReplanJob(false);notice="ĐÃ DỰNG XE · CÒN "+LivesRemaining+" MẠNG";noticeTimer=3;
   if(sound){engine?.Play();if(IsChased)siren?.UnPause();}
  }
  void RebaseAfterRiderSlide()
  {
   float z=rider.localPosition.z;if(Mathf.Abs(z)<.0001f)return;
   Vector3 offset=Vector3.forward*z;Distance+=z;ScrollRoutes(z);fallTurnExit-=offset;crashTurnPosition-=offset;
   foreach(var v in vehicles)
   {
    if(!v.active)continue;v.model.localPosition-=offset;v.previousPosition-=offset;v.roadTurnStart-=offset;
    if(v.crashEffect!=null)v.crashEffect.localPosition-=offset;
   }
   if(policeBike!=null)policeBike.localPosition-=offset;
   rideCamera.transform.localPosition-=offset;rider.localPosition-=offset;
   // A head-on hit can slide back across the preceding junction. Keep the
   // physical next intersection and the GPS grid node in agreement.
   while(!IsTurning&&NextJunctionDistance>80)
   {navNext-=CityNavigation.Directions[navHeading];PrepareBranches(NextJunctionDistance-80);}
  }
  void TickCrashPolice(float dt)
  {
   if(policeBike==null||dt<=0)return;
   if(PoliceFallen){if(routePolice)TickRoutePolice(dt);else TickPoliceDriving(dt);return;}
   if(policeBody==null)policeBody=new TrafficVehicle{model=policeBike,halfWidth=.5f,halfLength=1.2f};
   Vector3 before=policeBike.localPosition,start=before-Vector3.forward*Speed*dt,next=start;
   policeBody.previousPosition=before;policeBody.previousYaw=policeBike.localEulerAngles.y;policeBody.knocked=false;
   float nominal=Mathf.Max(12,cruiseSpeed+5);bool clear=false;
   foreach(float side in new[]{-1.1f,1.1f,0})
   {
    Vector3 destination=rider.localPosition+crashTurnRotation*Vector3.right*side;destination.y=0;
    Vector3 candidate=Vector3.MoveTowards(start,destination,nominal*dt);
    Vector3 direction=candidate-start;float yaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
    if(!RoutePolicePathClear(before,candidate,yaw,dt))continue;
    next=candidate;policeBike.localRotation=Quaternion.Euler(0,yaw,0);clear=true;break;
   }
   PoliceSpeed=clear?nominal:0;if(!clear)PoliceAvoidances++;
   policeBike.localPosition=next;ResolvePoliceContact(before,(next-before)/dt+Vector3.forward*Speed,true);
   if(PoliceFallen)policeFallYaw=policeBike.localEulerAngles.y;
   PoliceDistance=Vector2.Distance(Flat(policeBike.localPosition),Flat(rider.localPosition));
   if(!PoliceFallen&&Vector3.Distance(policeBike.localPosition,rider.localPosition)<=2.2f)Arrest();
  }
 }
}
