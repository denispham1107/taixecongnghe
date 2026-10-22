using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public bool RiderAirborne=>jumpFlying;
  public bool RiderOnRoof=>roofVehicle!=null;
  public bool RiderJumpActive=>jumpWindup>0||jumpFlying||jumpLanding>0;
  public string RiderJumpClip=>jumpWindup>0?"JumpTakeoff":jumpFlying?"JumpAir":landingClip;
  public float RiderJumpPose=>jumpWindup>0?1-jumpWindup/.24f:jumpFlying?Mathf.Clamp01(.5f-jumpVelocity/(2*JumpLaunch)):1-jumpLanding/.48f;
  public float RiderJumpHeight=>jumpHeight;
  public float RiderJumpCooldown=>jumpCooldown;
  float jumpHeight,jumpVelocity,jumpCooldown,jumpWindup,jumpLanding,jumpFlightAge,jumpPreviousHeight;
  bool jumpFlying;
  string landingClip="LandBoth";
  TrafficVehicle roofVehicle,previousRoofVehicle,roofContactVehicle;
  float roofContactHeight;
  const float JumpGravity=22f,JumpLaunch=14.6f;
  public static bool IsJumpSwipe(Vector2 delta,float width)=>delta.y>width*.12f&&delta.y>Mathf.Abs(delta.x)*1.3f;
  public void PoseJumpPassenger(float pitch,float lift)
  {
   if(customer==null||customer.parent!=rider)return;
   Vector3 pivot=new Vector3(0,0,-.73f);Quaternion balance=Quaternion.Euler(pitch,0,0);
   customer.localPosition=pivot+balance*(new Vector3(0,.16f,-.9f)-pivot)+Vector3.up*lift;
   customer.localRotation=balance;
  }
  void ResetJump()
  {jumpHeight=jumpPreviousHeight=jumpVelocity=jumpCooldown=jumpWindup=jumpLanding=jumpFlightAge=roofContactHeight=0;jumpFlying=false;roofVehicle=previousRoofVehicle=roofContactVehicle=null;landingClip="LandBoth";}
  public bool TryJump()
  {
   if(State!=RideState.Riding||ServiceStopped||jumpFlying||jumpWindup>0||jumpCooldown>0||jumpLanding>0)return false;
   // A roof edge starts a physical fall, so it cannot be used for an extra mid-air jump.
   if(jumpHeight>.06f&&roofVehicle==null)return false;
   CancelAssistedKick(false);jumpWindup=.24f;jumpCooldown=2;return true;
  }
  // A roof is usable while upright, including a weaving or damaged truck. A tilted
  // wreck remains a collision obstacle rather than becoming an invisible platform.
  public static float JumpRoofHeight(TrafficVehicle v)
  {
   if(v==null||v.model==null||v.halfWidth<=.7f)return -1;
   if(Mathf.Abs(Mathf.DeltaAngle(0,v.model.localEulerAngles.z))>12||Mathf.Abs(Mathf.DeltaAngle(0,v.model.localEulerAngles.x))>12)return -1;
   if(v.halfLength>3)return 3.175f;
   if(v.knocked||v.wallCrashed)return -1;
   var a=v.model.GetComponent<CarVisualAnimator>();string n=a!=null?a.AssetName:"";
   return n=="PV_DeliveryVan"?1.91f:n=="PV_Hatchback"?1.69f:1.60f;
  }
  bool RoofContains(TrafficVehicle v,Vector3 point,float padding=0)
   =>RoofContainsAt(v,point,v.model.localPosition,v.model.localRotation,padding);
  bool RoofContainsAt(TrafficVehicle v,Vector3 point,Vector3 position,Quaternion rotation,float padding=0)
  {
   if(!v.active||JumpRoofHeight(v)<0)return false;
   Vector3 local=Quaternion.Inverse(rotation)*(point-position);
   // Support lasts until the trailing wheel/body clears the bonnet, rather than
   // dropping the bike while its rear half is still overlapping the same car.
   return Mathf.Abs(local.x)<v.halfWidth-.14f+padding&&Mathf.Abs(local.z)<v.halfLength+.92f+padding;
  }
  float RoofSurface(TrafficVehicle v,Vector3 point)
   =>RoofSurfaceAt(v,point,v.model.localPosition,v.model.localRotation,rider.localRotation);
  float RoofSurfaceAt(TrafficVehicle v,Vector3 point,Vector3 position,Quaternion rotation,Quaternion riderRotation)
  {
   Vector3 local=Quaternion.Inverse(rotation)*(point-position);
   float roof=JumpRoofHeight(v),z=local.z;
   if(v.halfLength>3)
   {
    // Blender box roof ends at z=1.225, cab roof is 2.71 m high. Keep the
    // rear wheel supported on the box until it clears that edge, then fall
    // onto the lower cab instead of snapping downward or floating over it.
    Vector3 relativeForward=Quaternion.Inverse(rotation)*riderRotation*Vector3.forward;
    float rearZ=z-.73f*relativeForward.z;
    float truckSurface=rearZ<=1.225f?roof:rearZ<=2.75f?2.71f:
      Mathf.Lerp(2.71f,1.0f,Mathf.Clamp01((rearZ-2.75f)/.48f));
    return (position+rotation*new Vector3(local.x,truckSurface,local.z)).y+.025f;
   }
   // Sloping bonnet and boot avoid floating above the lower ends of the mesh.
   // Both wheels support the bike. Sampling only its centre drops it into the
   // windscreen/bonnet while its trailing wheel is still on the higher roof.
   Vector3 forward=Quaternion.Inverse(rotation)*riderRotation*Vector3.forward;
   float a=z-.73f*forward.z,b=z+.90f*forward.z;
   float surface=Mathf.Max(CarRoofProfile(roof,a),CarRoofProfile(roof,b));
   if(Mathf.Min(a,b)<=.15f&&Mathf.Max(a,b)>=-.85f)surface=roof;
   return (position+rotation*new Vector3(local.x,surface,local.z)).y+.025f;
  }
  static float CarRoofProfile(float roof,float z)
  {
   if(roof>1.8f&&z<.15f)return roof;
   return z>.15f?Mathf.Lerp(roof,1.07f,Mathf.Clamp01((z-.15f)/.85f)):
    z<-.85f?Mathf.Lerp(roof,1.1f,Mathf.Clamp01((-z-.85f)/.65f)):roof;
  }
  void TickJump(float dt)
  {
   previousRoofVehicle=roofVehicle;roofContactVehicle=null;roofContactHeight=0;jumpPreviousHeight=jumpHeight;jumpCooldown=Mathf.Max(0,jumpCooldown-dt);jumpLanding=Mathf.Max(0,jumpLanding-dt);
   if(jumpWindup>0)
   {
    jumpWindup=Mathf.Max(0,jumpWindup-dt);
    if(jumpWindup==0){jumpFlying=true;jumpVelocity=JumpLaunch;jumpFlightAge=0;roofVehicle=null;}
   }
   if(jumpFlying)
   {jumpHeight+=jumpVelocity*dt-.5f*JumpGravity*dt*dt;jumpVelocity-=JumpGravity*dt;jumpFlightAge+=dt;}
  }
  void ResolveJumpSurface(float dt)
  {
   Vector3 point=rider.localPosition;
   if(roofVehicle!=null)
   {
    float surface=RoofContains(roofVehicle,point)?RoofSurface(roofVehicle,point):-1;
    if(surface>=0&&(roofVehicle.halfLength<=3||Mathf.Abs(surface-jumpHeight)<=.12f))jumpHeight=surface;
    else{roofVehicle=null;jumpFlying=true;jumpVelocity=0;jumpFlightAge=JumpLaunch/JumpGravity;}
   }
   if(jumpFlying&&jumpVelocity<=0)
   {
    TrafficVehicle landed=null;float top=0,first=2;
    foreach(var v in vehicles)
    {
     if(!v.active||JumpRoofHeight(v)<0)continue;
     if(FindRoofLanding(v,point,dt,out float at,out float surface)&&at<first){landed=v;top=surface;first=at;}
    }
    if(landed!=null)
    {
     LandJump(top,landed);roofContactVehicle=landed;roofContactHeight=top;
     // Spend the rest of this frame riding across the roof. At high relative
     // speed the landing and the roof edge can both occur between two frames.
     int steps=Mathf.Clamp(Mathf.CeilToInt(dt/.005f),2,100);
     for(int i=1;i<=steps;i++)
     {
      float t=Mathf.Lerp(first,1,i/(float)steps);RoofPose(landed,point,t,out var p,out var q,out var r,out var bike);
      float surface=RoofSurfaceAt(landed,p,q,r,bike);
      if(!RoofContainsAt(landed,p,q,r)||surface>jumpHeight+.12f||landed.halfLength>3&&surface<jumpHeight-.12f)
      {roofVehicle=null;jumpFlying=true;float fall=dt*(1-t);jumpVelocity=-JumpGravity*fall;jumpHeight-=.5f*JumpGravity*fall*fall;jumpFlightAge=JumpLaunch/JumpGravity;break;}
      jumpHeight=surface;roofContactHeight=Mathf.Max(roofContactHeight,surface);
     }
    }
    else if(jumpHeight<=0)LandJump(0,null);
   }
   var resolved=rider.localPosition;resolved.y=jumpHeight;rider.localPosition=resolved;
  }
  void RoofPose(TrafficVehicle v,Vector3 point,float t,out Vector3 riderPoint,out Vector3 vehiclePoint,out Quaternion vehicleRotation,out Quaternion riderRotation)
  {
   riderPoint=Vector3.Lerp(riderBody.previousPosition,point,t);vehiclePoint=Vector3.Lerp(v.previousPosition,v.model.localPosition,t);
   vehicleRotation=Quaternion.Slerp(Quaternion.Euler(0,v.previousYaw,0),v.model.localRotation,t);
   riderRotation=Quaternion.Slerp(Quaternion.Euler(0,oldRiderYaw,0),rider.localRotation,t);
  }
  bool FindRoofLanding(TrafficVehicle v,Vector3 point,float dt,out float at,out float height)
  {
   at=2;height=0;float freeHeight=jumpHeight;
   float radius=Mathf.Max(v.halfLength,v.halfWidth)+1.05f;
   if(!SweptOverlap(Flat(v.previousPosition-riderBody.previousPosition),Flat(v.model.localPosition-point),Vector2.one*radius))return false;
   float travel=((point-riderBody.previousPosition)-(v.model.localPosition-v.previousPosition)).magnitude;
   int steps=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(dt/.005f,travel/.1f)),2,128);
   for(int i=1;i<=steps;i++)
   {
    float a=(i-1)/(float)steps,b=i/(float)steps;
    RoofPose(v,point,b,out var p,out var position,out var rotation,out var bike);
    if(!RoofContainsAt(v,p,position,rotation))continue;
    float surface=RoofSurfaceAt(v,p,position,rotation,bike);
    RoofPose(v,point,a,out var oldPoint,out var oldPosition,out var oldRotation,out var oldBike);
    float oldSurface=RoofContainsAt(v,oldPoint,oldPosition,oldRotation)?RoofSurfaceAt(v,oldPoint,oldPosition,oldRotation,oldBike):surface;
    float before=Mathf.Lerp(jumpPreviousHeight,freeHeight,a),after=Mathf.Lerp(jumpPreviousHeight,freeHeight,b);
    if(before<Mathf.Max(oldSurface,surface)-.035f||after>surface)continue;
    at=b;height=surface;return true;
   }
   return false;
  }
  void LandJump(float y,TrafficVehicle surface)
  {
   // Downward velocity and relative forward travel determine which wheel contacts first.
   float relative=surface==null?Speed:Speed-BodyVelocity(surface).z;
   landingClip=jumpVelocity< -9&&relative>12?"LandFront":relative<5?"LandRear":"LandBoth";
   jumpHeight=y;jumpVelocity=0;jumpFlying=false;jumpLanding=.48f;roofVehicle=surface;
  }
  bool JumpClearsVehicle(TrafficVehicle v,float at)
  {
   float y=Mathf.Lerp(jumpPreviousHeight,jumpHeight,at);
   float top=JumpRoofHeight(v);
   if(top>=0)
   {
    // Collision broad-phase uses a conservative car/truck box. Resolve the
    // actual moving roof over the same interval, including the landing frame.
    // Comparing an earlier rider height with the roof at the frame's end used
    // to turn valid landings and bonnet exits into side impacts.
    Vector3 from=riderBody.previousPosition,to=rider.localPosition;
    float travel=((to-from)-(v.model.localPosition-v.previousPosition)).magnitude;
    int steps=Mathf.Clamp(Mathf.CeilToInt(travel/.1f),2,128);
    bool supported=roofVehicle==v||previousRoofVehicle==v||roofContactVehicle==v;
    for(int i=0;i<=steps;i++)
    {
     float t=Mathf.Lerp(at,1,i/(float)steps);
     Quaternion vehicleRotation=Quaternion.Slerp(Quaternion.Euler(0,v.previousYaw,0),v.model.localRotation,t);
     Quaternion riderRotation=Quaternion.Slerp(Quaternion.Euler(0,oldRiderYaw,0),rider.localRotation,t);
     Vector3 point=Vector3.Lerp(from,to,t),position=Vector3.Lerp(v.previousPosition,v.model.localPosition,t);
     Vector3 local=Quaternion.Inverse(vehicleRotation)*(point-position),f=Quaternion.Inverse(vehicleRotation)*riderRotation*Vector3.forward;
     if(Mathf.Abs(local.x)>v.halfWidth+Mathf.Abs(f.z)*.33f+Mathf.Abs(f.x)*.91f||Mathf.Abs(local.z)>v.halfLength+Mathf.Abs(f.x)*.33f+Mathf.Abs(f.z)*.91f)continue;
     top=RoofSurfaceAt(v,point,position,vehicleRotation,riderRotation);
     // A supported wheel follows the roof profile, not the straight chord from
     // one frame's height to the next. A higher cargo wall still blocks it.
     float height=supported?Mathf.Max(Mathf.Max(jumpPreviousHeight,jumpHeight),roofContactVehicle==v?roofContactHeight:0):Mathf.Lerp(jumpPreviousHeight,jumpHeight,t);
     if(height<top-.04f)return false;
    }
    return true;
   }
   else top=v.model.localPosition.y+(v.halfLength>3?3.2f:v.halfWidth>.7f?1.9f:v.knocked?.6f:2.4f);
   return y>=top-.018f;
  }
 }
}
