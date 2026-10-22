using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  float jobTrafficYieldRemaining;
  bool TrafficYieldActive=>JobsAvailable&&(jobTrafficYieldRemaining>0||ServiceRearProtected);
  public float JobTrafficYieldRemaining=>jobTrafficYieldRemaining;
  void BeginJobTrafficYield()
  {
   jobTrafficYieldRemaining=3;
   foreach(var v in vehicles)v.jobYieldLane=-1;
  }
  Vector2 ServiceTrafficFootprint(TrafficVehicle v)
  {
   float a=v.previousYaw*Mathf.Deg2Rad,c=Mathf.Abs(Mathf.Cos(a)),s=Mathf.Abs(Mathf.Sin(a));
   var previous=new Vector2(BodyWidth(v)*c+v.halfLength*s,v.halfLength*c+BodyWidth(v)*s);
   return Vector2.Max(previous,VehicleFootprint(v));
  }
  bool HoldCrossTrafficForService(TrafficVehicle v,float dt)
  {
   if(!ServiceStopped||dt<=0||(!v.crossing&&!v.roadTurning&&!v.routeDetached))return false;
   Vector3 velocity=v.routeDetached?v.routeVelocity:v.crossing?new Vector3(v.crossDelay>0?0:v.crossSpeed,0,0):BodyVelocity(v);
   Vector2 from=Flat(v.model.localPosition-rider.localPosition),half=VehicleFootprint(v)+new Vector2(.85f,1.35f);
   if(!SweptOverlap(from,from+Flat(velocity)*1.8f,half))return false;
   // Cross-street traffic holds before entering the passenger stop, retaining its route.
   if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(true);
   return true;
  }
  void BrakeBeforeStoppedRider(TrafficVehicle v)
  {
   if(!ServiceStopped||!v.active||v.wallCrashed)return;
   Vector2 half=VehicleFootprint(v)+new Vector2(.65f,1.15f),from=Flat(v.previousPosition-rider.localPosition),to=Flat(v.model.localPosition-rider.localPosition);
   if(!SweptOverlap(from,to,half))return;
   // Emergency braking bounds the actual displacement; collision checks remain enabled.
   float low=0,high=1;
   for(int i=0;i<12;i++){float mid=(low+high)*.5f;if(SweptOverlap(from,Vector2.Lerp(from,to,mid),half))high=mid;else low=mid;}
   v.model.localPosition=Vector3.Lerp(v.previousPosition,v.model.localPosition,Mathf.Max(0,low-.001f));
   v.speed=0;v.motionVelocity=Vector3.zero;
   if(v.knocked){v.knockVelocity.x=0;v.knockVelocity.z=0;}
   if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(true);
  }
  bool JobYieldLaneClear(TrafficVehicle v,int lane,float seconds)
  {
   float x=LaneX[lane];Vector3 p=v.model.localPosition;
   // Check the whole lateral path and future longitudinal separation, not just the destination.
   foreach(var other in vehicles)
   {
    if(other==v||!other.active||other.model.localPosition.y>2.4f)continue;
    Vector2 bounds=VehicleFootprint(other);Vector3 q=other.model.localPosition;
    for(int step=0;step<=4;step++)
    {
     float t=step*.25f,at=t*seconds;
     float ox=q.x+(other.motionVelocity.x)*at;
     float oz=q.z+(other.speed-Speed)*at;
     float vx=Mathf.Lerp(p.x,x,t),vz=p.z+(v.speed-Speed)*at;
     if(Mathf.Abs(vx-ox)<v.halfWidth+bounds.x+.35f&&Mathf.Abs(vz-oz)<v.halfLength+bounds.y+1.5f)return false;
    }
   }
   return true;
  }
  bool TickJobTrafficYield(TrafficVehicle v,float dt)
  {
   if(!TrafficYieldActive||dt<=0)return false;
   Vector3 p=v.model.localPosition,r=rider.localPosition;
   float relative=(ServiceStopped&&Mathf.Abs(v.speed)<.1f?v.baseTrafficSpeed:v.speed)-Speed,z=p.z-r.z;
   Vector2 footprint=ServiceTrafficFootprint(v);
   float separation=footprint.y+.91f;
   bool converging=z*relative<0&&Mathf.Abs(z)<separation+Mathf.Abs(relative)*1.5f+5;
   bool holdingOncoming=v.baseTrafficSpeed<0&&z>0&&z<separation+5;
   bool inPath=Mathf.Abs(p.x-r.x)<footprint.x+1.15f||v.changingLane&&Mathf.Abs(v.targetX-r.x)<footprint.x+1.15f;
   if(v.jobYieldLane<0&&(!(converging||holdingOncoming)||!inPath))return false;
   if(v.jobYieldLane>=0&&!JobYieldLaneClear(v,v.jobYieldLane,Mathf.Abs(p.x-LaneX[v.jobYieldLane])/18f))v.jobYieldLane=-1;
   if(v.jobYieldLane<0)v.jobYieldLane=FindJobYieldLane(v);
   v.changingLane=false;v.rideAlongRemaining=0;
   if(v.jobYieldLane>=0)
   {
    float oldX=p.x;p.x=Mathf.MoveTowards(p.x,LaneX[v.jobYieldLane],18*dt);
    // Reduce closing speed while steering, so the lane change finishes before reaching the rider.
    if(z>0&&v.baseTrafficSpeed<0)v.speed=Mathf.MoveTowards(v.speed,0,60*dt);
    else if(z<0)v.speed=Mathf.MoveTowards(v.speed,Mathf.Min(v.speed,Speed),60*dt);
    // A vehicle ahead keeps its own speed; the protected rider yields below.
    p.z+=(v.speed-Speed)*dt;v.model.localPosition=p;
    float sideways=(p.x-oldX)/dt;
    float yaw=Mathf.Abs(sideways)+Mathf.Abs(v.speed)<.01f?(v.baseTrafficSpeed<0?180:0):Mathf.Atan2(sideways,v.speed)*Mathf.Rad2Deg;
    v.model.localRotation=Quaternion.Euler(0,yaw,0);
    v.lane=v.jobYieldLane;v.laneCooldown=Mathf.Max(v.laneCooldown,.5f);
   }
   else
   {
    // A blocked rear driver brakes; forward traffic holds its gap. Oncoming traffic stops to yield.
    float target=v.baseTrafficSpeed<0?0:z<0?Mathf.Min(v.speed,Speed):v.speed;
    v.speed=Mathf.MoveTowards(v.speed,target,60*dt);p.z+=(v.speed-Speed)*dt;v.model.localPosition=p;
   }
   if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(true);
   return true;
  }
  int FindJobYieldLane(TrafficVehicle v)
  {
   int target=-1;float best=float.PositiveInfinity;Vector3 p=v.model.localPosition;
   for(int lane=0;lane<4;lane++)
   {
    if(v.baseTrafficSpeed>0&&lane<2&&!v.isReckless)continue;
    if(Mathf.Abs(LaneX[lane]-rider.localPosition.x)<v.halfWidth+1.15f)continue;
    float duration=Mathf.Abs(p.x-LaneX[lane])/18f;
    if(!JobYieldLaneClear(v,lane,duration))continue;
    float score=Mathf.Abs(p.x-LaneX[lane]);
    if(score<best){best=score;target=lane;}
   }
   return target;
  }
  float JobAcceptanceSpeedLimit(float desired)
  {
   if(!TrafficYieldActive)return desired;
   foreach(var v in vehicles)
   {
    if(!v.active||v.knocked||v.wallCrashed||v.crossing||v.roadTurning||v.routeDetached)continue;
    Vector3 delta=v.model.localPosition-rider.localPosition;
    if(delta.z<=0||Mathf.Abs(delta.x)>=v.halfWidth+1.15f)continue;
    if(v.baseTrafficSpeed>=0)
    {
     float forwardGap=delta.z-ServiceTrafficFootprint(v).y-.91f-.8f;
     desired=Mathf.Min(desired,Mathf.Max(0,v.speed)+Mathf.Sqrt(120f*Mathf.Max(0,forwardGap)));continue;
    }
    if(v.jobYieldLane<0&&FindJobYieldLane(v)>=0)continue;
    // Both participants brake when the road is blocked. Reserve the oncoming
    // vehicle's stopping distance before assigning any of the gap to the rider.
    float gap=delta.z-v.halfLength-.91f-.8f,oncoming=Mathf.Max(0,-v.speed);
    float available=Mathf.Max(0,gap-oncoming*oncoming/120f);
    desired=Mathf.Min(desired,Mathf.Sqrt(120f*available));
   }
   return desired;
  }
 }
}
