using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  ContactFireVisual contactFire;
  const float CrashExplosionDelay=2f,CrashFireExitTime=1.15f,CrashFireEmergencyExitTime=1.42f;
  sealed class CrashBurn
  {
   public TrafficVehicle body;
   public HelicopterActor aircraft;
   public int generation,spreadIndex;
   public float age;
   public Bounds bounds;
   public Vector3[] spread;
  }
  readonly List<CrashBurn> crashBurns=new List<CrashBurn>();
  bool riderWreckExploded;
  void PrewarmContactFire()
  {
   if(contactFire==null&&Application.isPlaying&&stormFireTemplate!=null)
   {contactFire=gameObject.AddComponent<ContactFireVisual>();contactFire.Initialize(this,stormFireTemplate);}
   if(contactFire!=null)contactFire.Prewarm(4);
  }
  // Called only at confirmed body contacts, never by a generic impulse (kicks,
  // dinosaur attacks, lightning and explosions have their own effects).
  void ShowVehicleContact(TrafficVehicle a,TrafficVehicle b,Vector2 normal,float closing,float at=1,bool gentlePair=false)
  {
   if(a==null||b==null||a.riverFalling||b.riverFalling)return;
   Vector3 pa=Vector3.Lerp(a.previousPosition,a.model.localPosition,at),pb=Vector3.Lerp(b.previousPosition,b.model.localPosition,at);
   Vector3 faceA=VehicleContactFace(a,pa,pb,normal),faceB=VehicleContactFace(b,pb,pa,-normal);
   EmitVehicleImpact((faceA+faceB)*.5f,new Vector3(normal.x,0,normal.y),closing);
   if(gentlePair)return;
   if(!IsCarBikeContact(a,b)&&!IsProtectedContainer(a,b))IgniteVehicleContact(a,pa,faceA);
   if(!IsCarBikeContact(b,a)&&!IsProtectedContainer(b,a))IgniteVehicleContact(b,pb,faceB);
  }
  void IgniteVehicleContact(TrafficVehicle body,Vector3 centre,Vector3 point)
  {
   if(body.armored||body.model==policeBike)return;
   float height=body==riderBody?PlayerHeight:TrafficContactHeight(body);
   Vector3 anchor=Quaternion.Inverse(body.model.localRotation)*(point-centre);
   if(body==riderBody&&claimedVehicle!=null)body=claimedVehicle;
   BeginCrashBurn(body,null,new Bounds(Vector3.up*(height*.5f),new Vector3(body.halfWidth*2,height,body.halfLength*2)),
    anchor);
  }
  void IgniteAircraftContact(HelicopterActor aircraft,Vector3 point)
  {
   Bounds bounds=aircraft.skywing==null?new Bounds(new Vector3(0,.25f,-.65f),new Vector3(1.8f,1.6f,6.5f)):
    new Bounds(new Vector3(0,.22f,.35f),new Vector3(5.8f,1.4f,4.3f));
   var burn=FindCrashBurn(aircraft.root);
   if(burn!=null&&burn.generation!=aircraft.generation){RemoveCrashBurn(burn);burn=null;}
   var body=burn?.body??new TrafficVehicle{model=aircraft.root,active=true,halfWidth=bounds.extents.x,halfLength=bounds.extents.z};
   BeginCrashBurn(body,aircraft,bounds,bounds.ClosestPoint(aircraft.root.InverseTransformPoint(transform.TransformPoint(point))));
  }
  void BeginCrashBurn(TrafficVehicle body,HelicopterActor aircraft,Bounds bounds,Vector3 anchor)
  {
   if(contactFire==null&&Application.isPlaying&&stormFireTemplate!=null)
   {contactFire=gameObject.AddComponent<ContactFireVisual>();contactFire.Initialize(this,stormFireTemplate);}
   if(contactFire==null)return;
   var existing=FindCrashBurn(body.model);
   int generation=aircraft?.generation??body.spawnGeneration;
   if(existing!=null&&(existing.generation!=generation||existing.body!=body))
   {RemoveCrashBurn(existing);existing=null;}
   if(existing==null)
   {
    var size=bounds.extents;var p=bounds.center;
    var anchors=new[]{p+new Vector3(-size.x*.87f,0,size.z*.68f),p+new Vector3(size.x*.87f,0,size.z*.68f),
     p+new Vector3(-size.x*.88f,0,-size.z*.68f),p+new Vector3(size.x*.88f,0,-size.z*.68f),
     p+new Vector3(0,size.y*.82f,0),p+new Vector3(0,size.y*.43f,size.z*.92f),p+new Vector3(0,size.y*.5f,-size.z*.92f)};
    System.Array.Sort(anchors,(a,b)=>(a-anchor).sqrMagnitude.CompareTo((b-anchor).sqrMagnitude));
    crashBurns.Add(new CrashBurn{body=body,aircraft=aircraft,generation=generation,bounds=bounds,spread=anchors});
   }
   contactFire.Ignite(body,anchor);
  }
  CrashBurn FindCrashBurn(Transform target)
  {
   foreach(var burn in crashBurns)if(burn.body.model==target)return burn;
   return null;
  }
  bool CrashBurnValid(CrashBurn burn)
  {
   var body=burn.body;var target=body.model;
   if(target==null||!target.gameObject.activeInHierarchy||body.riverFalling||body.armored)return false;
   if(burn.aircraft!=null)return burn.aircraft.active&&burn.aircraft.generation==burn.generation&&
    (target.localPosition.y>RiverBlock.WaterY+.25f||!OverRiver(target.localPosition));
   if(body.spawnGeneration!=burn.generation)return false;
   return target==rider?body==riderBody&&!RiderInRiver:body.active||body==claimedVehicle;
  }
  bool IsCollisionBurning(TrafficVehicle body)
  {
   var burn=FindCrashBurn(body.model);
   return burn!=null&&burn.body==body&&CrashBurnValid(burn);
  }
  float RiderExitCountdown()
  {
   var burn=FindCrashBurn(claimedVehicle?.model??rider);
   return burn!=null&&CrashBurnValid(burn)?CrashFireExitTime-burn.age:ForcedVehicleExitDelay-fallTime;
  }
  void RemoveCrashBurn(CrashBurn burn)
  {
   contactFire.ReleaseFor(burn.body);
   crashBurns.Remove(burn);
  }
  void TickCrashBurns(float dt)
  {
   if(dt<=0||State==RideState.Paused||State==RideState.Ready)return;
   for(int i=crashBurns.Count-1;i>=0;i--)
   {
    var burn=crashBurns[i];var body=burn.body;
    if(!CrashBurnValid(burn)){RemoveCrashBurn(burn);continue;}
    burn.age+=dt;
    while(burn.spreadIndex<burn.spread.Length&&burn.age>=.40f+.22f*burn.spreadIndex)
     contactFire.Ignite(body,burn.spread[burn.spreadIndex++]);
    bool playerCar=body.model==rider||body==claimedVehicle;
    bool seated=playerCar&&(State==RideState.Riding||State==RideState.Falling);
    if(seated&&burn.age>=CrashFireExitTime&&BeginFootExit(burn.age>=CrashFireEmergencyExitTime))
    {notice="ĐÃ RỜI XE · XE SẮP NỔ";noticeTimer=1.2f;}
    if(burn.age+.00001f<CrashExplosionDelay)continue;
    Vector3 motion=burn.aircraft!=null?burn.aircraft.velocity:body.model==rider?riderFallVelocity:BodyVelocity(body);
    EmitCrashExplosion(body.model,burn.bounds,motion,burn.aircraft!=null);
    RemoveCrashBurn(burn);
    if(playerCar)
    {
     riderWreckExploded=true;riderBody.knocked=false;
     RestoreClaimedVehicle();rider.gameObject.SetActive(false);
     if(State==RideState.Riding||State==RideState.Falling)
     {State=RideState.GameOver;Speed=0;SaveRideRecord();engine?.Stop();siren?.Stop();}
    }
    else if(burn.aircraft!=null)
    {burn.aircraft.active=false;body.model.gameObject.SetActive(false);}
    else
    {
     if(body==entryVehicle)CancelVehicleEntry();
     RetireVehicle(body);
    }
   }
  }
  Vector3 VehicleContactFace(TrafficVehicle body,Vector3 centre,Vector3 other,Vector2 face)
  {
   // Find the struck face in the yaw footprint rather than using the midpoint
   // between centres (which puts fire metres away from a long truck's bumper).
   Quaternion yaw=Quaternion.Euler(0,body.model.localEulerAngles.y,0);
   Vector3 toward=Quaternion.Inverse(yaw)*(other-centre),n=Quaternion.Inverse(yaw)*new Vector3(face.x,0,face.y);
   float width=BodyWidth(body),length=body.halfLength;
   var p=new Vector3(Mathf.Clamp(toward.x,-width,width),.55f,Mathf.Clamp(toward.z,-length,length));
   if(Mathf.Abs(n.x)>Mathf.Abs(n.z))p.x=Mathf.Sign(n.x)*width;else p.z=Mathf.Sign(n.z)*length;
   Vector3 road=centre+yaw*p;
   float height=body==riderBody?PlayerHeight:body.model.TryGetComponent<MeshyCarVariants>(out var car)?car.Height:body.halfWidth>.7f?1.9f:.9f;
   road.y=Mathf.Clamp((centre.y+other.y)*.5f+.5f,centre.y+.12f,centre.y+Mathf.Max(.15f,height*.6f));
   return road;
  }
  void RenderContactFire(){if(contactFire!=null)contactFire.Render();}
  void ResetContactFire(){crashBurns.Clear();riderWreckExploded=false;if(contactFire!=null)contactFire.Clear();}
 }
}
