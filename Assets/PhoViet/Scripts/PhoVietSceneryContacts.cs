using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  const float CurbRolloverSpeed=65f/3.6f,StreetCurbFace=6.99f,StreetCurbHeight=.35f;
  const string CurbImpact="Bó vỉa";
  float curbFeedbackUntil;
  StreetObstacleSet[] streetObstacles;
  public string LastSceneryImpact {get;private set;}="";
  void CacheStreetObstacles(){streetObstacles=scenery!=null?scenery.GetComponentsInChildren<StreetObstacleSet>(true):new StreetObstacleSet[0];LastSceneryImpact="";curbFeedbackUntil=0;}
  bool SceneryContact(Vector3 from,Vector3 to,out Vector2 normal,out float at,out string kind)
  {
   if(!IsPlayerCar)return SceneryContactAtRotation(from,to,rider.localRotation,out normal,out at,out kind);
   // Using a long car's final heading for the whole sweep invents contact at
   // its old position. Intersect short segments with the interpolated heading.
   Quaternion before=Quaternion.Euler(0,oldRiderYaw,0),after=rider.localRotation;
   int steps=Mathf.Clamp(Mathf.CeilToInt(Quaternion.Angle(before,after)/3),1,64);
   normal=Vector2.zero;at=2;kind="";
   for(int i=0;i<steps;i++)
   {
    float a=i/(float)steps,b=(i+1)/(float)steps;
    if(!SceneryContactAtRotation(Vector3.Lerp(from,to,a),Vector3.Lerp(from,to,b),Quaternion.Slerp(before,after,(a+b)*.5f),out var n,out float t,out string k))continue;
    normal=n;at=Mathf.Lerp(a,b,t);kind=k;return true;
   }
   return false;
  }
  bool SceneryContactAtRotation(Vector3 from,Vector3 to,Quaternion rotation,out Vector2 normal,out float at,out string kind)
  {
   normal=Vector2.zero;at=2;kind="";
   if(IsPlayerCar&&State==RideState.Riding&&!IsTurning&&RoutesReady&&
      Mathf.Min(from.y,to.y)<StreetCurbHeight&&
      Mathf.Max(Mathf.Abs(from.x),Mathf.Abs(to.x))+PlayerHalfLength>=StreetCurbFace)
   {
    CheckRowCurb(roadRows[mainRow],from,to,rotation,ref normal,ref at);
    if(at<=1)kind=CurbImpact;
   }
   if(streetObstacles==null)CacheStreetObstacles();
   EnsureObstacleBatches();
   foreach(var batch in obstacleBatches)
   {
    if(!NearbyObstacleBatch(batch,from,to))continue;
    foreach(var set in batch.sets)
    {
    if(set==null||!set.gameObject.activeInHierarchy)continue;
    var group=set.LocalBounds;Vector3 origin=transform.InverseTransformPoint(set.transform.TransformPoint(group.center));float radius=group.extents.magnitude+1.1f;
    if(Mathf.Abs(origin.x-to.x)>radius+Mathf.Abs(to.x-from.x)||Mathf.Abs(origin.z-to.z)>radius+Mathf.Abs(to.z-from.z))continue;
    Vector3 a=set.transform.InverseTransformPoint(transform.TransformPoint(from)),b=set.transform.InverseTransformPoint(transform.TransformPoint(to));
    Vector3 forward=set.transform.InverseTransformDirection(transform.TransformDirection(rotation*Vector3.forward));
    Vector3 right=set.transform.InverseTransformDirection(transform.TransformDirection(rotation*Vector3.right));
    var extent=new Vector3(Mathf.Abs(forward.x)*PlayerHalfLength+Mathf.Abs(right.x)*PlayerHalfWidth,0,Mathf.Abs(forward.z)*PlayerHalfLength+Mathf.Abs(right.z)*PlayerHalfWidth);
    foreach(var obstacle in set.obstacles)
    {
     var bounds=obstacle.bounds;Vector3 min=bounds.min-extent,max=bounds.max+extent;min.y-=PlayerHeight;
     Vector3 delta=b-a;float enter=0,leave=1;Vector3 face=Vector3.zero;bool valid=true;
     for(int axis=0;axis<3;axis++)
     {
      if(Mathf.Abs(delta[axis])<.000001f){if(a[axis]<min[axis]||a[axis]>max[axis]){valid=false;break;}continue;}
      float t1=(min[axis]-a[axis])/delta[axis],t2=(max[axis]-a[axis])/delta[axis];float near=Mathf.Min(t1,t2),far=Mathf.Max(t1,t2);
      if(near>enter){enter=near;face=Vector3.zero;face[axis]=Mathf.Sign(delta[axis]);}
      leave=Mathf.Min(leave,far);if(enter>leave){valid=false;break;}
     }
     if(!valid||leave<0||enter>1||enter>=at)continue;
     // Roof/table top clearance must not turn a safe jump into a side impact.
     if(Mathf.Abs(face.y)>.5f||Mathf.Min(from.y,to.y)>=set.transform.TransformPoint(bounds.max).y-transform.position.y+.02f)continue;
     if(face.sqrMagnitude<.5f)
     {
      float dx=Mathf.Min(Mathf.Abs(a.x-min.x),Mathf.Abs(max.x-a.x)),dz=Mathf.Min(Mathf.Abs(a.z-min.z),Mathf.Abs(max.z-a.z));
      face=dx<dz?new Vector3(a.x<bounds.center.x?1:-1,0,0):new Vector3(0,0,a.z<bounds.center.z?1:-1);
      if(Vector3.Dot(delta,face)<=.00001f)continue; // Already touching: allow steering away after standing up.
     }
     var n=transform.InverseTransformDirection(set.transform.TransformDirection(face));normal=new Vector2(n.x,n.z).normalized;at=Mathf.Clamp01(enter);kind=obstacle.kind;
    }
   }
   }
   return at<=1;
  }
  void CheckRowCurb(RoadRow row,Vector3 from,Vector3 to,Quaternion rotation,ref Vector2 normal,ref float at)
  {
   // The main street tiles are already aligned with the gameplay frame.
   Vector3 forward=rotation*Vector3.forward,right=rotation*Vector3.right;
   float reach=Mathf.Abs(forward.x)*PlayerHalfLength+Mathf.Abs(right.x)*PlayerHalfWidth;
   float front=Mathf.Abs(forward.z)*PlayerHalfLength+Mathf.Abs(right.z)*PlayerHalfWidth;
   float delta=to.x-from.x;
   if(Mathf.Abs(delta)<.00001f)return;
   float side=Mathf.Sign(delta),face=StreetCurbFace-reach;
   if(side*to.x<face)return;
   float contact=Mathf.Max(0,(face-side*from.x)/Mathf.Abs(delta));
   if(contact>=at||contact>1||Mathf.Lerp(from.y,to.y,contact)>=StreetCurbHeight)return;
   float z=Mathf.Lerp(from.z,to.z,contact);
   foreach(var tile in row.tiles)
   {
    float localZ=z-tile.localPosition.z;
    if(localZ+front<0||localZ-front>63)continue;
    var river=RiverOf(tile);
    if(river!=null&&river.IsBridge&&localZ>RiverBlock.Near&&localZ<RiverBlock.Far)continue;
    at=contact;normal=new Vector2(side,0);return;
   }
  }
  bool ResolveSceneryImpact(Vector3 from,Vector3 velocity,float beforeTrafficAt)
  {
   if(!SceneryContact(from,rider.localPosition,out var normal,out float at,out string kind)||at>=beforeTrafficAt)return false;
   rider.localPosition=Vector3.Lerp(from,rider.localPosition,at)-new Vector3(normal.x,0,normal.y)*.035f;
   LastSceneryImpact=kind;
   if(kind==CurbImpact)
   {
    Vector2 horizontal=Flat(velocity);
    float speed=Mathf.Max(horizontal.magnitude,driveSpeedBeforeRoadBoundary);
    Vector3 outward=new Vector3(normal.x,0,normal.y);
    if(timeRiding>=curbFeedbackUntil)
    {
     EmitVehicleImpact(rider.localPosition+outward*PlayerHalfWidth+Vector3.up*.2f,outward,speed);
     if(sound&&fx!=null)fx.PlayOneShot(crashClip,.4f);
     curbFeedbackUntil=timeRiding+.24f;
    }
    if(speed<CurbRolloverSpeed)
    {
     SlowCarAtCurb(horizontal,normal);
     return true;
    }
    BeginRiderImpact(null,normal,rider.localPosition+outward*PlayerHalfWidth,velocity);
    exitAfterCollision=true;
    return true;
   }
   BeginRiderImpact(null,normal,rider.localPosition+new Vector3(normal.x,0,normal.y)*.5f,velocity);return true;
  }
  void SlowCarAtCurb(Vector2 incoming,Vector2 normal)
  {
   float intoCurb=Mathf.Max(0,Vector2.Dot(incoming,normal));
   Vector2 along=incoming-normal*intoCurb;
   float carry=Mathf.Max(along.magnitude*.8f,incoming.magnitude*.22f);
   Vector2 away=along.sqrMagnitude>.01f?along.normalized:Vector2.up;
   away=(away-normal*.12f).normalized;
   freeDriveHeading=steerYaw=Mathf.Atan2(away.x,away.y)*Mathf.Rad2Deg;
   rider.localRotation=Quaternion.Euler(0,steerYaw,lean);
   driveYawRate=0;
   freeDriveSpeed=Speed=carry;
   riderRoadVelocity=new Vector3(away.x,0,away.y)*carry;
  }
  void ResolveFallenScenery(Vector3 from)
  {
   if(!SceneryContact(from,rider.localPosition,out var normal,out float at,out string kind)||Vector2.Dot(Flat(riderFallVelocity),normal)<.5f)return;
   rider.localPosition=Vector3.Lerp(from,rider.localPosition,at)-new Vector3(normal.x,0,normal.y)*.04f;
   LastSceneryImpact=kind;ApplyRiderImpulse(null,normal,rider.localPosition+new Vector3(normal.x,0,normal.y)*.5f,riderFallVelocity);
   State=RideState.Falling;
  }
 }
}
