using UnityEngine;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  // The helicopter cabin, tail and rotor occupy separate heights.
  static readonly Bounds[] HelicopterContactParts={
   new Bounds(new Vector3(0,.1f,.35f),new Vector3(1.4f,1.4f,4.7f)),
   new Bounds(new Vector3(0,.2f,-2.85f),new Vector3(.36f,.4f,2.9f)),
   new Bounds(new Vector3(0,1.125f,0),new Vector3(3.6f,.24f,3.6f)),
   new Bounds(new Vector3(0,1.125f,0),new Vector3(4.9f,.24f,1.7f)),
   new Bounds(new Vector3(0,1.125f,0),new Vector3(1.7f,.24f,4.9f))
  };

  int AircraftPoolCount=>helicopters.Count+skywings.Count;
  HelicopterActor AircraftAt(int i)=>i<helicopters.Count?helicopters[i]:skywings[i-helicopters.Count];
  static Bounds[] AircraftContactParts(HelicopterActor actor)=>actor.skywing==null?HelicopterContactParts:SkywingCollisionParts;

  void ResolveAircraftContacts(float dt)
  {
   for(int i=0;i<AircraftPoolCount;i++)
   {
    var a=AircraftAt(i);
    if(a==null||!a.active)continue;
    for(int j=i+1;j<AircraftPoolCount;j++)
    {
     var b=AircraftAt(j);
     if(b==null||!b.active||a.wrecked&&b.wrecked)continue;
     if(!SweptAircraftParts(a,b.previousContact,b.root,b.root.localRotation,AircraftContactParts(b),default,
         out float at,out Vector3 normal,out Vector3 point))continue;
     var av=a.contactVelocity;
     var bv=b.contactVelocity;
     float closing=Vector3.Dot(av-bv,normal);
     if(closing<.35f)continue;
     EmitVehicleImpact(point,normal,closing);
     CrashAircraftAtContact(a,at,bv,normal);
     CrashAircraftAtContact(b,at,av,-normal);
    }
   }
   for(int i=0;i<AircraftPoolCount;i++)
   {
    var aircraft=AircraftAt(i);
    if(aircraft!=null&&aircraft.active)ResolveAircraftRoadContact(aircraft,dt);
   }
  }

  void ResolveAircraftRoadContact(HelicopterActor aircraft,float dt)
  {
   aircraft.roadContactCooldown-=dt;
   if(aircraft.roadContactCooldown>0)return;
   // Road vehicles only need the narrow phase close to the road or a tossed vehicle.
   float aircraftBottom=Mathf.Min(aircraft.previousContact.y,aircraft.root.localPosition.y)-(aircraft.wrecked?4.8f:1);
   TrafficVehicle hit=null;
   float first=2,closing=0;
   Vector3 normal=Vector3.zero,point=Vector3.zero;
   foreach(var vehicle in vehicles)
   {
    if(!vehicle.active||vehicle.riverFalling)continue;
    float maxBase=Mathf.Max(vehicle.previousPosition.y,vehicle.model.localPosition.y);
    float reach=5+vehicle.halfLength+vehicle.halfWidth;
    float fromZ=vehicle.previousPosition.z-aircraft.previousContact.z;
    float toZ=vehicle.model.localPosition.z-aircraft.root.localPosition.z;
    if(Mathf.Min(fromZ,toZ)>reach||Mathf.Max(fromZ,toZ)<-reach)continue;
    float height=TrafficContactHeight(vehicle);
    if(aircraftBottom>maxBase+height)continue;
    var bounds=new Bounds(Vector3.up*(height*.5f),new Vector3(BodyWidth(vehicle)*2,height,vehicle.halfLength*2));
    var yaw=Quaternion.Euler(0,vehicle.model.localEulerAngles.y,0);
    if(!SweptAircraftParts(aircraft,vehicle.previousPosition,vehicle.model,yaw,null,bounds,
         out float at,out Vector3 face,out Vector3 contact)||at>=first)continue;
    float impact=Vector3.Dot(aircraft.contactVelocity-BodyVelocity(vehicle),face);
    if(impact<.35f)continue;
    hit=vehicle;first=at;closing=impact;normal=face;point=contact;
   }
   if(hit==null)return;
   var before=BodyVelocity(hit);
   EmitVehicleImpact(point,normal,closing);
   IgniteVehicleContact(hit,Vector3.Lerp(hit.previousPosition,hit.model.localPosition,first),point);
   CrashAircraftAtContact(aircraft,first,before,normal);
   aircraft.roadContactCooldown=.18f;
   if(hit.wallCrashed||hit.armored)return;
   var direction=Flat(normal);
   if(direction.sqrMagnitude<.04f)direction=Flat(aircraft.contactVelocity-before).normalized;
   Vector2 impulse=direction*VehicleMass(hit)*Mathf.Min(closing,16);
   var push=new Vector3(direction.x,0,direction.y)*Mathf.Min(closing*.5f,10);
   ActivateCollisionBody(hit,before,before+push,impulse,
       Flat(point-hit.model.localPosition),false,!hit.knocked,closing);
  }

  void CrashAircraftAtContact(HelicopterActor aircraft,float at,Vector3 otherVelocity,Vector3 towardOther)
  {
   if(aircraft.wrecked)return;
   aircraft.root.localPosition=Vector3.Lerp(aircraft.previousContact,aircraft.root.localPosition,at);
   KnockHelicopter(aircraft,otherVelocity,towardOther);
   // An aircraft struck during this step must use the actual contact position in later sweeps.
   aircraft.previousContact=aircraft.root.localPosition;
   aircraft.contactVelocity=aircraft.velocity;
  }

  static bool SweptAircraftParts(HelicopterActor a,Vector3 previousB,Transform b,Quaternion rotationB,
      Bounds[] partsB,Bounds singlePartB,out float at,out Vector3 normal,out Vector3 point)
  {
   at=2;normal=point=Vector3.zero;
   var from=previousB-a.previousContact;
   var to=b.localPosition-a.root.localPosition;
   var delta=to-from;
   float reach=partsB==null?5+singlePartB.center.magnitude+singlePartB.extents.magnitude:10;
   if(from.sqrMagnitude>(reach+delta.magnitude)*(reach+delta.magnitude))return false;
   var rotationA=a.root.localRotation;
   foreach(var partA in AircraftContactParts(a))
    for(int i=0;i<(partsB?.Length??1);i++)
    {
     var partB=partsB==null?singlePartB:partsB[i];
     var offsetA=rotationA*partA.center;
     var offsetB=rotationB*partB.center;
     var relative=from+offsetB-offsetA;
     if(!SweptPartContact(relative,delta,rotationA,partA.extents,rotationB,partB.extents,out float t,out Vector3 face)||t>=at)continue;
     at=t;normal=face;
     var centerA=Vector3.Lerp(a.previousContact,a.root.localPosition,t)+offsetA;
     var centerB=Vector3.Lerp(previousB,b.localPosition,t)+offsetB;
     // Use the middle of the opposing faces in the normal direction.
     var middle=(centerA+centerB)*.5f;
     float surface=(Vector3.Dot(centerA+centerB,face)+PartRadius(rotationA,partA.extents,face)-PartRadius(rotationB,partB.extents,face))*.5f;
     point=middle+face*(surface-Vector3.Dot(middle,face));
    }
   return at<=1;
  }

  static Vector3 PartAxis(int i)=>i==0?Vector3.right:i==1?Vector3.up:Vector3.forward;
  static float PartRadius(Quaternion rotation,Vector3 half,Vector3 axis)
  {
   var local=Quaternion.Inverse(rotation)*axis;
   return Mathf.Abs(local.x)*half.x+Mathf.Abs(local.y)*half.y+Mathf.Abs(local.z)*half.z;
  }

  static bool SweptPartContact(Vector3 from,Vector3 delta,Quaternion a,Vector3 halfA,
      Quaternion b,Vector3 halfB,out float at,out Vector3 normal)
  {
   at=0;normal=Vector3.zero;
   float leave=1,minDepth=float.PositiveInfinity;
   Vector3 restingNormal=Vector3.zero;
   for(int i=0;i<15;i++)
   {
    Vector3 axis=i<3?a*PartAxis(i):i<6?b*PartAxis(i-3):
        Vector3.Cross(a*PartAxis((i-6)/3),b*PartAxis((i-6)%3));
    if(axis.sqrMagnitude<.00001f)continue;
    axis.Normalize();
    float radius=PartRadius(a,halfA,axis)+PartRadius(b,halfB,axis);
    float start=Vector3.Dot(from,axis),speed=Vector3.Dot(delta,axis);
    float depth=radius-Mathf.Abs(start),before=at;
    if(depth<minDepth){minDepth=depth;restingNormal=start<0?-axis:axis;}
    if(!ContactHalfSpace(start-radius,speed,ref at,ref leave)||
       !ContactHalfSpace(-start-radius,-speed,ref at,ref leave))return false;
    if(at>before)normal=start+speed*at<0?-axis:axis;
   }
   if(normal==Vector3.zero)normal=restingNormal;
   return true;
  }
 }
}
