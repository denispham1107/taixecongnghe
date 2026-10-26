using UnityEngine;
namespace PhoViet
{
 // Steering is local to the bike. Hands inherit the same steering transform;
 // two-bone arms retain their grip while the fork, cowl and front wheel turn.
 [DisallowMultipleComponent]
 public sealed class UrbanRiderMotion:MonoBehaviour
 {
  public Transform visual,steering,frontWheel,rearWheel;
  public Transform leftUpper,leftForearm,leftHand,rightUpper,rightForearm,rightHand;
  public float frontRadius=.381557f,rearRadius=.321687f;
  public float SteeringAngle {get;private set;}
  public float LeanAngle {get;private set;}
  public float LeftArmError {get;private set;}
  public float RightArmError {get;private set;}
  public Vector3 FrontSpinAxis {get;private set;}
  public Vector3 RearSpinAxis {get;private set;}
  public Quaternion FrontRest {get;private set;}
  public Quaternion RearRest {get;private set;}
  PhoVietGame game;PhoVietGame.TrafficVehicle vehicle;
  Quaternion steeringRest;Vector3 steeringAxis;float previousYaw,steerVelocity,leanVelocity;bool initialized;
  sealed class Arm {public Transform upper,forearm,hand;public float a,b;public Vector3 pole,tip,scale;}
  Arm left,right;
  void OnEnable(){initialized=false;SteeringAngle=LeanAngle=steerVelocity=leanVelocity=0;}
  public void Initialize()
  {
   if(initialized||visual==null||steering==null)return;
   game=GetComponentInParent<PhoVietGame>();if(game==null)return;
   vehicle=game.vehicles.Find(v=>v.model==transform);
   steeringRest=steering.localRotation;steeringAxis=steering.parent.InverseTransformDirection(transform.TransformDirection(new Vector3(0,.96f,-.28f))).normalized;
   FrontSpinAxis=frontWheel.parent.InverseTransformDirection(transform.right);RearSpinAxis=rearWheel.parent.InverseTransformDirection(transform.right);
   FrontRest=frontWheel.localRotation;RearRest=rearWheel.localRotation;
   left=Bind(leftUpper,leftForearm,leftHand);right=Bind(rightUpper,rightForearm,rightHand);
   previousYaw=transform.localEulerAngles.y;initialized=true;
  }
  Arm Bind(Transform upper,Transform forearm,Transform hand)
  {
   Vector3 direction=(hand.position-upper.position).normalized,pole=forearm.position-upper.position;
   pole-=direction*Vector3.Dot(pole,direction);
   if(pole.sqrMagnitude<.00001f)pole=visual.right*(hand==leftHand?-1:1)-visual.up*.4f;
   return new Arm{upper=upper,forearm=forearm,hand=hand,a=Vector3.Distance(upper.position,forearm.position),b=Vector3.Distance(forearm.position,hand.position),pole=visual.InverseTransformDirection(pole.normalized),tip=forearm.InverseTransformPoint(hand.position),scale=forearm.localScale};
  }
  public void Advance(float dt,bool falling)
  {
   Initialize();if(!initialized||game.State==PhoVietGame.RideState.Paused)return;
   float yaw=transform.localEulerAngles.y;
   float speed=vehicle!=null?new Vector2(vehicle.motionVelocity.x,vehicle.motionVelocity.z).magnitude:0;
   bool moving=vehicle!=null&&vehicle.active&&!falling&&speed>.4f;
   float yawRate=dt>0?Mathf.DeltaAngle(previousYaw,yaw)*Mathf.Deg2Rad/dt:0;previousYaw=yaw;
   float curvature=moving?Mathf.Clamp(yawRate/Mathf.Max(speed,1),-.8f,.8f):0;
   float wheelbase=Vector3.Distance(frontWheel.position,rearWheel.position);
   float target=moving?Mathf.Clamp(Mathf.Atan(wheelbase*curvature)*Mathf.Rad2Deg,-25,25):0;
   bool exiting=Mathf.Abs(target)<Mathf.Abs(SteeringAngle);
   if(dt>0)SteeringAngle=Mathf.SmoothDamp(SteeringAngle,target,ref steerVelocity,exiting?.25f:.14f,180,dt);
   float bank=moving?-Mathf.Clamp(Mathf.Atan(speed*speed*curvature/9.81f)*Mathf.Rad2Deg,-23,23):0;
   if(dt>0)LeanAngle=Mathf.SmoothDamp(LeanAngle,bank,ref leanVelocity,Mathf.Abs(bank)<Mathf.Abs(LeanAngle)?.30f:.20f,140,dt);
   if(!falling)
   {
    float rootRoll=Mathf.DeltaAngle(0,transform.localEulerAngles.z);
    visual.localRotation=Quaternion.Euler(0,0,LeanAngle-rootRoll);
   }
   steering.localRotation=Quaternion.AngleAxis(SteeringAngle,steeringAxis)*steeringRest;
   // A rider turns the upper body with the bars instead of stretching an arm
   // to reach a grip while keeping both shoulders rigidly facing straight.
   var torso=leftUpper.parent;
   torso.rotation=Quaternion.AngleAxis(SteeringAngle*.65f,visual.up)*torso.rotation;
   LeftArmError=Solve(left);RightArmError=Solve(right);
  }
  float Solve(Arm arm)
  {
   arm.forearm.localScale=arm.scale;
   Vector3 a=arm.upper.position,target=arm.hand.position,direction=target-a;
   float distance=direction.magnitude;if(distance<.0001f)return 0;direction/=distance;
   float second=Mathf.Clamp(distance-arm.a+.006f,arm.b,arm.b*1.08f);
   float d=Mathf.Clamp(distance,Mathf.Abs(arm.a-second)+.001f,arm.a+second-.001f);
   float along=(arm.a*arm.a-second*second+d*d)/(2*d);
   Vector3 pole=visual.TransformDirection(arm.pole);pole-=direction*Vector3.Dot(pole,direction);pole.Normalize();
   Vector3 elbow=a+direction*along+pole*Mathf.Sqrt(Mathf.Max(0,arm.a*arm.a-along*along));
   arm.upper.rotation=Quaternion.FromToRotation(arm.forearm.position-a,elbow-a)*arm.upper.rotation;
   Vector3 toHand=target-arm.forearm.position;
   arm.forearm.rotation=Quaternion.FromToRotation(arm.forearm.TransformVector(arm.tip),toHand)*arm.forearm.rotation;
   float stretch=Mathf.Clamp(toHand.magnitude/arm.b,.98f,1.08f);arm.forearm.localScale=arm.scale*stretch;
   return Vector3.Distance(arm.forearm.TransformPoint(arm.tip),target);
  }
  public void Rebase(float yaw){if(initialized)previousYaw+=yaw;}
  public float LowestWheelSupport()
  {
   Initialize();if(!initialized)return float.PositiveInfinity;
   Vector3 fa=frontWheel.parent.TransformDirection(FrontSpinAxis).normalized,ra=rearWheel.parent.TransformDirection(RearSpinAxis).normalized;
   float front=frontWheel.position.y-frontRadius*Mathf.Sqrt(Mathf.Max(0,1-fa.y*fa.y))-.055f*Mathf.Abs(fa.y);
   float rear=rearWheel.position.y-rearRadius*Mathf.Sqrt(Mathf.Max(0,1-ra.y*ra.y))-.055f*Mathf.Abs(ra.y);
   return Mathf.Min(front,rear);
  }
 }
}
