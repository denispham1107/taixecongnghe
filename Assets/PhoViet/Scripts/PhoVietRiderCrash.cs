using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public bool CrashDuringPursuit {get;private set;}
  public bool RiderFallComplete {get;private set;}
  public Vector3 RiderFallVelocity=>riderFallVelocity;
  public float RiderFallTravel {get;private set;}
  public float LastRiderImpactSpeed {get;private set;}
  public float RiderFallRoll=>fallTargetRoll;
  public bool RiderCrashActive=>State==RideState.Falling||State==RideState.Paused&&pauseFromState==RideState.Falling;
  RideState pauseFromState=RideState.Riding;
  TrafficVehicle riderBody;
  Vector3 riderFallVelocity,riderFallEuler,riderFallSpin,riderRoadVelocity;
  float settledTime,fallTargetRoll,fallTargetPitch,oldRiderYaw;
  float[] riderContactUntil;
  bool riderVelocityValid;
  const float RiderGroundDrag=9f;
  void ResetRiderImpact()
  {
   CrashDuringPursuit=false;RiderFallComplete=false;RiderFallTravel=0;LastRiderImpactSpeed=0;
   fallTime=0;settledTime=0;pauseFromState=RideState.Riding;
   ResetCrashPoliceNavigation();
   riderFallVelocity=Vector3.zero;riderFallSpin=Vector3.zero;riderRoadVelocity=Vector3.zero;riderVelocityValid=false;
   riderContactUntil=new float[vehicles.Count];
   riderBody=new TrafficVehicle{model=rider,halfWidth=.33f,halfLength=.91f};
   CacheStreetObstacles();
  }
  void SaveRideRecord()
  {
   if((int)Distance<=BestDistance)return;
   BestDistance=(int)Distance;PlayerPrefs.SetInt(BestKey,BestDistance);PlayerPrefs.Save();
  }
  void BeginRiderImpact(TrafficVehicle hit,Vector2 normal,Vector3 contact,Vector3 before)
  {
   if(State!=RideState.Riding)return;
   CrashDuringPursuit=IsChased;RiderFallComplete=false;CancelSwipe();
   ResetCrashPoliceNavigation();
   crashTurnRotation=Quaternion.Euler(0,IsTurning?TurnYaw:0,0);
   QueuedTurn=0;fallTime=0;settledTime=0;RiderFallTravel=0;
   riderFallVelocity=before;riderFallEuler=rider.localEulerAngles;
   riderFallEuler.x=Mathf.DeltaAngle(0,riderFallEuler.x);riderFallEuler.z=Mathf.DeltaAngle(0,riderFallEuler.z);riderFallSpin=Vector3.zero;
   before.y=jumpVelocity;ResetJump();CancelBoost();
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
   float invA=1f/220,invB=hit==null||hit.wallCrashed?0:1f/VehicleMass(hit);
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
   fallTime+=dt;timeRiding+=dt;TickActionTimers(dt);TickBoost(dt);
   Vector3 previous=rider.localPosition;
   Speed=Mathf.Max(0,riderFallVelocity.z);
   float scroll=Speed*dt;Distance+=scroll;ScrollRoutes(scroll);
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
   ResolveFallenScenery(previous);
   TickChase(dt);RetireTraffic(false);
   if(State==RideState.Arrested)return;
   if(!IsTurning)EndRouteFrame(dt);
   if(rider.localPosition.y<=.001f&&riderFallVelocity.sqrMagnitude<.1f&&fallTime>.4f)
   {
    riderFallVelocity=Vector3.zero;Speed=0;settledTime+=dt;
    if(settledTime>=.7f)
    {
     RiderFallComplete=true;
     if(!CrashDuringPursuit){State=RideState.GameOver;SaveRideRecord();siren?.Stop();}
    }
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
  void TickCrashPolice(float dt)
  {
   if(policeBike==null||dt<=0)return;
   if(PoliceFallen){crashPolicePath.Clear();crashPoliceReplan=0;if(routePolice)TickRoutePolice(dt);else TickPoliceDriving(dt);return;}
   if(policeBody==null)policeBody=new TrafficVehicle{model=policeBike,halfWidth=.5f,halfLength=1.2f};
   Vector3 before=policeBike.localPosition,start=before-Vector3.forward*Speed*dt,next=start;
   policeBody.previousPosition=before;policeBody.previousYaw=policeBike.localEulerAngles.y;policeBody.knocked=false;
   float nominal=Mathf.Max(12,cruiseSpeed+5);bool clear=false;
   bool arrived=Vector3.Distance(start,rider.localPosition)<=2.1f;
   if(!arrived&&CrashPoliceWaypoint(start,dt,out Vector3 target))
   {
    next=Vector3.MoveTowards(start,target,nominal*dt);Vector3 direction=next-start;
    float yaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
    if(RoutePolicePathClear(before,next,yaw,dt)){policeBike.localRotation=Quaternion.Euler(0,yaw,0);clear=true;}
    else {next=start;crashPolicePath.Clear();}
   }
   PoliceSpeed=clear?nominal:0;if(!clear&&!arrived)PoliceAvoidances++;
   policeBike.localPosition=next;ResolvePoliceContact(before,(next-before)/dt+Vector3.forward*Speed,true);
   if(PoliceFallen)policeFallYaw=policeBike.localEulerAngles.y;
   PoliceDistance=Vector2.Distance(Flat(policeBike.localPosition),Flat(rider.localPosition));
   if(!PoliceFallen&&Vector3.Distance(policeBike.localPosition,rider.localPosition)<=2.2f)Arrest();
  }
 }
}
