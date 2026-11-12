using System.Collections.Generic;
using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const float GrenadeCooldown=2f,FootGrenadeCooldown=.5f,GrenadeCraterRadius=1.15f;
  const float GrenadeFlight=.48f,GrenadeBlastRadius=1.4f,GrenadeSlowDuration=1.2f;
  const float FootThrowDuration=FootGrenadeCooldown,FootThrowRelease=.20f;
  float grenadeCooldown,footGrenadeCooldown;
  sealed class RoadGrenade
  {
   public Transform model;
   public Vector3 start,target;
   public Quaternion rotation;
   public float age,side;
   public bool thrownOnFoot;
  }
  readonly List<RoadGrenade> grenades=new List<RoadGrenade>(2);
  readonly Stack<RoadGrenade> grenadePool=new Stack<RoadGrenade>(2);
  readonly List<TankCrater> grenadeCraters=new List<TankCrater>();

  public partial class TrafficVehicle
  {
   [System.NonSerialized] public float grenadeSlowTime,grenadeSide,grenadeYaw;
   [System.NonSerialized] public int lastGrenadeCrater;
  }

  bool TryThrowGrenades()
  {
   if(State!=RideState.Riding||ServiceStopped||JobOffersOpen||grenadeCooldown>.00001f)return false;
   EnsureTankFx();
   Quaternion routeHeading=Quaternion.Euler(0,IsTurning?TurnYaw:0,0);
   Quaternion heading=IsTurning?routeHeading:Quaternion.Euler(0,rider.localEulerAngles.y,0);
   Vector3 origin=rider.localPosition;
   // Inherit most of the car's advance during the toss so the blast stays behind it at speed.
   Vector3 landingLead=routeHeading*Vector3.forward*(Speed*GrenadeFlight*.93f);
   for(int side=-1;side<=1;side+=2)
   {
    var grenade=grenadePool.Count>0?grenadePool.Pop():NewRoadGrenade();
    grenade.age=0;grenade.side=side;grenade.rotation=heading;grenade.thrownOnFoot=false;
    grenade.start=origin+heading*new Vector3(side*(PlayerHalfWidth+.12f),.55f,-PlayerHalfLength*.65f);
    grenade.target=origin+heading*new Vector3(side*(PlayerHalfWidth+1.05f),0,-5.4f)+landingLead;
    if(!IsTurning)grenade.target.x=Mathf.Clamp(grenade.target.x,-5.9f,5.9f);
    grenade.target.y=0;
    if(RiverZoneAt(grenade.target)&&!IsTurning)
     grenade.target.x=Mathf.Clamp(grenade.target.x,-RiverBlock.DeckHalf+.25f,RiverBlock.DeckHalf-.25f);
    grenade.model.localPosition=grenade.start;grenade.model.localRotation=heading;
    grenade.model.localScale=Vector3.one;
    grenade.model.gameObject.SetActive(true);grenades.Add(grenade);
   }
   grenadeCooldown=GrenadeCooldown;notice="ĐÃ QUĂNG LỰU ĐẠN PHÍA SAU";noticeTimer=.85f;
   return true;
  }

  public bool TryThrowFootGrenade()
  {
   if(State!=RideState.OnFoot||InsideShop||footJumpTime>0||footFallRemaining>0||entryVehicle!=null||footGrenadeCooldown>.00001f)return false;
   EnsureTankFx();
   var grenade=grenadePool.Count>0?grenadePool.Pop():NewRoadGrenade();
   grenade.age=-FootThrowRelease;grenade.side=1;grenade.thrownOnFoot=true;
   grenade.rotation=Quaternion.LookRotation(footFacing);grenade.start=FootGrenadeHandPosition();
   grenade.target=footActor.localPosition+footFacing*5.4f;grenade.target.y=0;
   grenade.model.SetLocalPositionAndRotation(grenade.start,grenade.rotation);
   grenade.model.localScale=Vector3.one*.60f;
   grenade.model.gameObject.SetActive(true);grenades.Add(grenade);
   footGrenadeCooldown=FootGrenadeCooldown;footThrowPose=FootThrowDuration;return true;
  }

  RoadGrenade NewRoadGrenade()
  {
   var model=Group("Lựu đạn phía sau",transform);
   Shape("Thân lựu đạn",model,PrimitiveType.Sphere,Vector3.zero,new Vector3(.27f,.34f,.27f),Hex("55664B"));
   Box("Đai vàng",model,new Vector3(0,.092f,0),new Vector3(.22f,.043f,.22f),Hex("E8AD42"));
   Box("Ngòi",model,new Vector3(0,.18f,0),new Vector3(.11f,.10f,.13f),Hex("2C3435"));
   return new RoadGrenade{model=model};
  }

  void TickGrenades(float dt)
  {
   if(dt<=0||State==RideState.Paused)return;
   for(int i=grenades.Count-1;i>=0;i--)
   {
    var grenade=grenades[i];
    if(grenade.age<0)
    {
     if(!PlayerOnFoot||InsideShop||footFallRemaining>0||entryVehicle!=null)
     {
      ReleaseGrenade(grenade);grenades.RemoveAt(i);continue;
     }
     grenade.start=FootGrenadeHandPosition();
     grenade.target=footActor.localPosition+grenade.rotation*Vector3.forward*5.4f;
     grenade.target.y=0;
     if(grenade.age+dt<0)
     {
      grenade.age+=dt;grenade.model.localPosition=grenade.start;continue;
     }
    }
    grenade.age+=dt;
    float progress=Mathf.Clamp01(grenade.age/GrenadeFlight);
    Vector3 point=Vector3.Lerp(grenade.start,grenade.target,progress)+Vector3.up*(4*1.25f*progress*(1-progress));
    grenade.model.localPosition=point;
    grenade.model.localRotation=grenade.rotation*Quaternion.Euler(progress*510,0,grenade.side*progress*220);
    if(progress<1)continue;
    if(grenade.thrownOnFoot)DetonateGrenadeAt(grenade.target,true);else DetonateGrenade(grenade.target);
    ReleaseGrenade(grenade);grenades.RemoveAt(i);
   }
  }

  void DetonateGrenade(Vector3 point)=>DetonateGrenadeAt(point,false);
  void DetonateGrenadeAt(Vector3 point,bool onFoot)
  {
   if(OverRiver(point))
   {
    RiverZone(point,out _,out var tile);Splash(tile,point,1);
    if(sound&&fx!=null)fx.PlayOneShot(blastClip,.2f);
    return;
   }
   var crater=CreateCrater(point,GrenadeCraterRadius,true);
   PlayRoadExplosion(point,.6f);
   if(onFoot)
   {
    foreach(var v in vehicles)if(v.active&&!v.riverFalling)StopVehicleWithGrenade(v,point);
    if(IsChased&&policeBody!=null&&!PoliceInRiver)StopVehicleWithGrenade(policeBody,point);
    return;
   }
   foreach(var v in vehicles)
    if(v.active&&!v.knocked&&!v.riverFalling)CheckGrenadeBlast(v,point,crater);
   if(IsChased&&policeBody!=null&&!PoliceInRiver&&!PoliceFallen)CheckGrenadeBlast(policeBody,point,crater);
  }

  void StopVehicleWithGrenade(TrafficVehicle v,Vector3 point)
  {
   Vector3 offset=Quaternion.Inverse(VehicleYaw(v))*(point-v.model.localPosition);
   Vector2 outside=new Vector2(Mathf.Max(0,Mathf.Abs(offset.x)-v.halfWidth),Mathf.Max(0,Mathf.Abs(offset.z)-v.halfLength));
   if(Mathf.Abs(offset.y)>.85f||outside.sqrMagnitude>GrenadeBlastRadius*GrenadeBlastRadius)return;
   if(v.footStopRemaining<=0)v.footResumeVelocity=BodyVelocity(v);
   v.footStopRemaining=FootVehicleStopTime;v.motionVelocity=Vector3.zero;
  }

  void CheckGrenadeBlast(TrafficVehicle v,Vector3 point,TankCrater crater)
  {
   Vector3 offset=v.model.localPosition-point;
   if(offset.y>.35f||Flat(offset).sqrMagnitude>GrenadeBlastRadius*GrenadeBlastRadius)return;
   DisturbVehicle(v,point,crater);
  }

  void ScrollGrenades(float delta)
  {
   Vector3 scroll=Vector3.forward*delta;
   foreach(var grenade in grenades){grenade.start-=scroll;grenade.target-=scroll;grenade.model.localPosition-=scroll;}
  }

  void RebaseGrenades(Quaternion inverse,Vector3 origin)
  {
   foreach(var grenade in grenades)
   {
    grenade.start=inverse*(grenade.start-origin);grenade.target=inverse*(grenade.target-origin);
    grenade.rotation=inverse*grenade.rotation;
    grenade.model.localPosition=inverse*(grenade.model.localPosition-origin);
    grenade.model.localRotation=inverse*grenade.model.localRotation;
   }
  }

  void ResetGrenades()
  {
   foreach(var grenade in grenades)ReleaseGrenade(grenade);
   grenades.Clear();grenadeCooldown=footGrenadeCooldown=0;
  }

  void ReleaseGrenade(RoadGrenade grenade)
  {
   grenade.model.gameObject.SetActive(false);grenadePool.Push(grenade);
  }

  static float GrenadeDriveFactor(TrafficVehicle v)=>1-.35f*Mathf.Clamp01(v.grenadeSlowTime/GrenadeSlowDuration);

  static void AdvanceGrenadeDrive(TrafficVehicle v,float dt)
  {
   if(v.grenadeYaw!=0&&!v.knocked)
    v.model.localRotation=Quaternion.Euler(0,-v.grenadeYaw,0)*v.model.localRotation;
   v.grenadeYaw=0;v.grenadeSlowTime=Mathf.Max(0,v.grenadeSlowTime-dt);
  }

  void DisturbVehicle(TrafficVehicle v,Vector3 centre,TankCrater crater)
  {
   float side=Vector3.Dot(v.model.localPosition-centre,v.model.localRotation*Vector3.right);
   v.grenadeSide=Mathf.Abs(side)<.05f?1:Mathf.Sign(side);
   v.grenadeSlowTime=GrenadeSlowDuration;
   if(crater!=null)v.lastGrenadeCrater=crater.id;
  }

  void ApplyVehicleRoadStep(TrafficVehicle v,Vector3 before,float dt)
  {
   if(dt<=0)return;
   Vector3 scroll=Vector3.forward*(Speed*dt);
   Vector3 travel=LimitRoadVelocity(v.model.localPosition-before+scroll,VehicleSpeedLimit*dt);
   if(!v.knocked&&before.y<.18f&&v.model.localPosition.y<.18f)
   {
    float remaining=Mathf.Clamp01(v.grenadeSlowTime/GrenadeSlowDuration);
    if(remaining>0&&Flat(travel).sqrMagnitude>.000001f)
    {
     float yaw=v.grenadeSide*4f*Mathf.Sin(Mathf.PI*remaining);
     var steer=Quaternion.Euler(0,yaw,0);
     travel=steer*travel;v.model.localRotation=steer*v.model.localRotation;v.grenadeYaw=yaw;
    }
    Vector3 end=before+travel-scroll;
    TankCrater touched=null;
    Vector3 touchedCentre=Vector3.zero;
    bool stillTouching=false;
    foreach(var crater in grenadeCraters)
    {
     if(crater.owner==null||crater.owner.LayoutRevision!=crater.revision)continue;
     Vector3 centre=transform.InverseTransformPoint(crater.model.position);
     float radius=crater.radius+v.halfWidth*.6f;
     if(CraterSegmentDistance(Flat(before-centre-scroll),Flat(end-centre),out _)>radius)continue;
     if(crater.id==v.lastGrenadeCrater){stillTouching=true;break;}
     touched=crater;touchedCentre=centre;
    }
    if(!stillTouching)
    {
     v.lastGrenadeCrater=0;
     if(touched!=null)DisturbVehicle(v,touchedCentre,touched);
    }
   }
   v.model.localPosition=before+travel-scroll;
  }
 }
}
