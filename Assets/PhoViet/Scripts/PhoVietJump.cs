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
  TrafficVehicle roofVehicle;
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
  {jumpHeight=jumpPreviousHeight=jumpVelocity=jumpCooldown=jumpWindup=jumpLanding=jumpFlightAge=0;jumpFlying=false;roofVehicle=null;landingClip="LandBoth";}
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
  {
   if(!v.active||JumpRoofHeight(v)<0)return false;
   Vector3 local=Quaternion.Inverse(v.model.localRotation)*(point-v.model.localPosition);
   // Support lasts until the trailing wheel/body clears the bonnet, rather than
   // dropping the bike while its rear half is still overlapping the same car.
   return Mathf.Abs(local.x)<v.halfWidth-.14f+padding&&Mathf.Abs(local.z)<v.halfLength+.92f+padding;
  }
  float RoofSurface(TrafficVehicle v,Vector3 point)
  {
   Vector3 local=Quaternion.Inverse(v.model.localRotation)*(point-v.model.localPosition);
   float roof=JumpRoofHeight(v),z=local.z;
   if(v.halfLength>3)
   {
    // Blender box roof ends at z=1.225, cab roof is 2.71 m high. Keep the
    // rear wheel supported on the box until it clears that edge, then fall
    // onto the lower cab instead of snapping downward or floating over it.
    Vector3 relativeForward=Quaternion.Inverse(v.model.localRotation)*rider.localRotation*Vector3.forward;
    float rearZ=z-.73f*relativeForward.z;
    float truckSurface=rearZ<=1.225f?roof:rearZ<=2.75f?2.71f:
      Mathf.Lerp(2.71f,1.0f,Mathf.Clamp01((rearZ-2.75f)/.48f));
    return v.model.TransformPoint(new Vector3(local.x,truckSurface,local.z)).y-transform.position.y+.025f;
   }
   // Sloping bonnet and boot avoid floating above the lower ends of the mesh.
   float surface=z>.15f?Mathf.Lerp(roof,1.07f,Mathf.Clamp01((z-.15f)/.85f)):
      z<-.85f?Mathf.Lerp(roof,1.1f,Mathf.Clamp01((-z-.85f)/.65f)):roof;
   if(roof>1.8f&&z<.15f)surface=roof;
   return v.model.localPosition.y+surface+.025f;
  }
  void TickJump(float dt)
  {
   jumpPreviousHeight=jumpHeight;jumpCooldown=Mathf.Max(0,jumpCooldown-dt);jumpLanding=Mathf.Max(0,jumpLanding-dt);
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
    TrafficVehicle landed=null;float top=0;
    foreach(var v in vehicles)
    {
     if(!RoofContains(v,point))continue;
     float surface=RoofSurface(v,point);
     float oldSurface=surface+v.previousPosition.y-v.model.localPosition.y;
     if(jumpPreviousHeight>=oldSurface-.035f&&jumpHeight<=surface&&surface>top){landed=v;top=surface;}
    }
    if(landed!=null)LandJump(top,landed);
    else if(jumpHeight<=0)LandJump(0,null);
   }
   var p=rider.localPosition;p.y=jumpHeight;rider.localPosition=p;
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
   if(top>=0)top=RoofSurface(v,rider.localPosition);
   else top=v.model.localPosition.y+(v.halfLength>3?3.2f:v.halfWidth>.7f?1.9f:v.knocked?.6f:2.4f);
   return y>=top-.018f;
  }
 }
}
