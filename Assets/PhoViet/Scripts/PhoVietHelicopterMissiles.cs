using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const int HelicopterMissileCapacity=40;
  const float HelicopterMissileSpeed=50,HelicopterMissileLifetime=3.2f;
  sealed class HelicopterMissile
  {public Transform root;public MissileFlightVisual visual;public bool active,dodgeCounted;public Vector3 velocity,scroll;public float age;}
  readonly HelicopterMissile[] helicopterMissiles=new HelicopterMissile[HelicopterMissileCapacity];GameObject helicopterMissilePrefab;
  public int HelicopterMissilesFired{get;private set;}public int HelicopterMissilesDodged{get;private set;}
  public bool HitByHelicopterMissile{get;private set;}public bool RiderAirAttackBurning{get;private set;}
  public int ActiveHelicopterMissileCount{get{int n=0;foreach(var m in helicopterMissiles)if(m!=null&&m.active)n++;return n;}}
  public int BurningHelicopterCount{get{int n=0;foreach(var h in helicopters)if(h!=null&&h.active&&h.wrecked)n++;return n;}}
  public static float HelicopterMissileInterval(float metres)=>metres>=1200?.5f:metres>=800?.6f:metres>=400?.8f:1;
  void ResetHelicopterMissiles()
  {
   HelicopterMissilesFired=HelicopterMissilesDodged=0;HitByHelicopterMissile=RiderAirAttackBurning=false;
   if(Application.isPlaying&&helicopterMissilePrefab==null)helicopterMissilePrefab=Resources.Load<GameObject>("Helicopter/PV_HelicopterMissile");
   for(int i=0;i<helicopterMissiles.Length;i++)
   {
    var m=helicopterMissiles[i];
    if(m==null&&Application.isPlaying&&helicopterMissilePrefab!=null)
    {var root=Instantiate(helicopterMissilePrefab,transform).transform;root.name="Helicopter missile "+(i+1);m=helicopterMissiles[i]=new HelicopterMissile{root=root,visual=root.GetComponent<MissileFlightVisual>()};}
    if(m==null)continue;m.active=false;m.age=0;m.scroll=Vector3.zero;m.visual.End();m.root.gameObject.SetActive(false);
   }
  }
  void TickHelicopterWeapon(HelicopterActor h,float dt)
  {
   float interval=HelicopterMissileInterval(Distance);
   if(State!=RideState.Riding||!RiderFlying||IsTurning||h.wrecked){h.shotInterval=h.shotRemaining=interval;return;}
   h.shotRemaining=Mathf.Max(0,h.shotRemaining+interval-h.shotInterval);h.shotInterval=interval;h.shotRemaining-=dt;
   Vector3 delta=rider.localPosition-h.root.localPosition;
   if(h.shotRemaining>.00001f||delta.sqrMagnitude>125*125||Vector3.Dot(h.root.localRotation*Vector3.forward,delta)<8)return;
   // At most one missile per aircraft per control step. Never queue a burst while out of range.
   if(FireHelicopterMissile(h))h.shotRemaining=interval;else h.shotRemaining=.05f;
  }
  bool FireHelicopterMissile(HelicopterActor h)
  {
   if(h==null||!h.active||h.wrecked||State!=RideState.Riding||!RiderFlying)return false;
   HelicopterMissile free=null;foreach(var m in helicopterMissiles)if(m!=null&&!m.active){free=m;break;}if(free==null)return false;
   Vector3 origin=h.root.localPosition+h.root.localRotation*new Vector3(0,-.15f,1.3f);
   Vector3 target=rider.localPosition+Vector3.up*(PlayerHeight*.5f),offset=target-origin;
   // Lead only the road travel at launch. Steering/altitude changes after firing are evasion.
   Vector3 roadVelocity=Vector3.forward*Speed;
   float a=roadVelocity.sqrMagnitude-HelicopterMissileSpeed*HelicopterMissileSpeed,b=2*Vector3.Dot(offset,roadVelocity),c=offset.sqrMagnitude;
   float t=offset.magnitude/(HelicopterMissileSpeed+Mathf.Max(0,Speed)),disc=b*b-4*a*c;
   if(Mathf.Abs(a)>.001f&&disc>=0){float r1=(-b-Mathf.Sqrt(disc))/(2*a),r2=(-b+Mathf.Sqrt(disc))/(2*a);if(r1>0)t=r1;if(r2>0&&(r1<=0||r2<r1))t=r2;}
   free.velocity=(target+roadVelocity*t-origin).normalized*HelicopterMissileSpeed;free.scroll=Vector3.zero;free.age=0;free.dodgeCounted=false;
   free.root.localPosition=origin;free.root.localRotation=Quaternion.LookRotation(free.velocity);free.active=true;free.root.gameObject.SetActive(true);free.visual.Begin();HelicopterMissilesFired++;
   notice="TÊN LỬA · ĐỔI LÀN / HẠ / LỘN NÉ";noticeTimer=1.2f;return true;
  }
  void ScrollHelicopterMissiles(float distance){foreach(var m in helicopterMissiles)if(m!=null&&m.active){m.scroll+=Vector3.forward*distance;m.visual.Scroll(distance);}}
  void RebaseHelicopterMissiles(Quaternion inverse,Vector3 origin)
  {
   foreach(var m in helicopterMissiles)if(m!=null&&m.active)
   {m.root.localPosition=inverse*(m.root.localPosition-origin);m.root.localRotation=inverse*m.root.localRotation;m.velocity=inverse*m.velocity;m.scroll=inverse*m.scroll;m.visual.RebaseTrail();}
  }
  void TickHelicopterMissiles(float dt,Vector3 previousRider,bool collisions)
  {
   foreach(var m in helicopterMissiles)if(m!=null&&m.active)
   {
    Vector3 from=m.root.localPosition;m.root.localPosition+=m.velocity*dt-m.scroll;m.scroll=Vector3.zero;m.age+=dt;
    Vector3 nose=m.velocity.normalized*.65f;
    if(collisions&&State==RideState.Riding&&MissileTouchesRider(from+nose,m.root.localPosition+nose,previousRider,out float at))
    {
     if(RiderRolling){if(!m.dodgeCounted){m.dodgeCounted=true;HelicopterMissilesDodged++;}}
     else{Vector3 point=Vector3.Lerp(from,m.root.localPosition,at);AirAttackBurst(point);BurnAirAttackRider(point,true);RetireHelicopterMissile(m);continue;}
    }
    if(m.age>=HelicopterMissileLifetime||m.root.localPosition.z< -35||Mathf.Abs(m.root.localPosition.x)>38||m.root.localPosition.y<-.1f)RetireHelicopterMissile(m);
   }
  }
  void RetireHelicopterMissile(HelicopterMissile m){m.active=false;m.visual.End();m.root.gameObject.SetActive(false);}
  bool MissileTouchesRider(Vector3 from,Vector3 to,Vector3 previousRider,out float time)
  {
   Quaternion inverse=Quaternion.Inverse(Quaternion.Euler(0,rider.localEulerAngles.y,0));Vector3 center=Vector3.up*(PlayerHeight*.5f);
   Vector3 p=inverse*(from-previousRider-center),q=inverse*(to-rider.localPosition-center),delta=q-p;
   Vector3 half=new Vector3(PlayerHalfWidth+.13f,PlayerHeight*.5f+.13f,PlayerHalfLength+.13f);float enter=0,exit=1;
   for(int i=0;i<3;i++)
   {
    if(Mathf.Abs(delta[i])<.00001f){if(Mathf.Abs(p[i])>half[i]){time=0;return false;}continue;}
    float a=(-half[i]-p[i])/delta[i],b=(half[i]-p[i])/delta[i];if(a>b){float swap=a;a=b;b=swap;}
    enter=Mathf.Max(enter,a);exit=Mathf.Min(exit,b);if(enter>exit){time=0;return false;}
   }
   time=enter;return true;
  }
  void BurnAirAttackRider(Vector3 point,bool missile)
  {
   if(State!=RideState.Riding||RiderAirAttackBurning)return;
   Vector3 before=riderVelocityValid?riderRoadVelocity:rider.localRotation*Vector3.forward*Speed;
   Vector3 direction=before;direction.y=0;if(direction.sqrMagnitude<.1f)direction=rider.localRotation*Vector3.forward;direction.Normalize();
   BeginRiderImpact(null,new Vector2(direction.x,direction.z),point,before);
   riderFallVelocity=direction*Mathf.Clamp(before.magnitude*.6f,6,25)+Vector3.down*2;riderFallSpin=new Vector3(20,8,55);
   RiderAirAttackBurning=true;HitByHelicopterMissile|=missile;CrashDuringPursuit=false;IsChased=false;routePolice=false;chaseBoost=0;ResetCrashPoliceNavigation();siren?.Stop();
   EnsureThunderstorm();stormVisual?.Burn(rider,null);
   notice=missile?"TRÚNG TÊN LỬA · CHUYẾN ĐI KẾT THÚC":"VA TRỰC THĂNG · CHUYẾN ĐI KẾT THÚC";noticeTimer=5;
  }
  void KnockHelicopter(HelicopterActor h,Vector3 before,Vector3 away)
  {
   if(h.wrecked)return;h.wrecked=true;h.age=0;h.velocity=h.velocity*.55f+before*.15f-away*3+Vector3.down*2;
   h.wreckEuler=h.root.localEulerAngles;h.wreckSpin=new Vector3(25,16,away.x>=0?-65:65);h.visual.SetWrecked();
   EnsureThunderstorm();stormVisual?.Burn(h.root,null,1.65f);AirAttackBurst(h.root.localPosition);
  }
  void TickHelicopterWreck(HelicopterActor h,float dt)
  {
   h.age+=dt;h.velocity.y-=9.81f*dt;Vector3 p=h.root.localPosition+h.velocity*dt-h.scroll;h.scroll=Vector3.zero;
   bool water=OverRiver(p);float floor=water?RiverBlock.WaterY-3:.85f;
   if(p.y<=floor){p.y=floor;h.velocity=Vector3.MoveTowards(new Vector3(h.velocity.x,0,h.velocity.z),Vector3.zero,dt*12);h.wreckSpin*=Mathf.Exp(-dt*4);}
   h.wreckEuler+=h.wreckSpin*dt;h.root.localPosition=p;h.root.localRotation=Quaternion.Euler(h.wreckEuler);
   if(h.age>24||p.z< -40||Mathf.Abs(p.x)>38||water&&p.y<=floor){h.active=false;h.root.gameObject.SetActive(false);}
  }
  public bool IsAircraftBurning(Transform target)
  {
   if(target==rider)return RiderAirAttackBurning;
   foreach(var h in helicopters)if(h!=null&&h.root==target)return h.active&&h.wrecked;return false;
  }
  void AirAttackBurst(Vector3 at)
  {
   EnsureTankFx();Emit(blastFx,at,Vector3.zero,2.6f,.55f,Color.white);
   Emit(smokeFx,at,Vector3.up*1.2f,2.2f,1.3f,new Color(.3f,.29f,.28f,.8f));
   if(sound&&fx!=null)fx.PlayOneShot(blastClip,.4f);
  }
 }
}
