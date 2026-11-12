using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const int HelicopterMissileCapacity=40; // Initial pool size; sustained fire can grow it.
  const float EnemyMissileTurnReduction=.1f,EnemyMissileMinimumInterval=.25f;
  const float HelicopterMissileSpeed=36,HelicopterMissileLifetime=128f/HelicopterMissileSpeed;
  enum EnemyMissileSource { None, Tank, Helicopter, Skywing }
  sealed class HelicopterMissile
  {public Transform root;public MissileFlightVisual visual;public AudioSource flightAudio;public bool active,dodgeCounted;public EnemyMissileSource source;public Vector3 velocity,scroll;public float age;}
  readonly List<HelicopterMissile> helicopterMissiles=new List<HelicopterMissile>(HelicopterMissileCapacity);GameObject helicopterMissilePrefab;
  public int HelicopterMissilesFired{get;private set;}public int HelicopterMissilesDodged{get;private set;}
  public int TankMissilesFired{get;private set;}
  EnemyMissileSource missileHitSource;
  public bool HitByHelicopterMissile=>missileHitSource==EnemyMissileSource.Helicopter||missileHitSource==EnemyMissileSource.Skywing;
  public bool HitByTankMissile=>missileHitSource==EnemyMissileSource.Tank;
  public bool RiderAirAttackBurning{get;private set;}
  string MissileHitSourceLabel=>missileHitSource switch
  {
   EnemyMissileSource.Tank=>"xe tăng",
   EnemyMissileSource.Helicopter=>"trực thăng",
   EnemyMissileSource.Skywing=>"máy bay hai tầng cánh",
   _=>null
  };
  public int ActiveHelicopterMissileCount=>CountEnemyMissiles(false);
  public int ActiveTankMissileCount=>CountEnemyMissiles(true);
  int CountEnemyMissiles(bool fromTank)
  {
   int count=0;
   foreach(var m in helicopterMissiles)if(m!=null&&m.active&&(m.source==EnemyMissileSource.Tank)==fromTank)count++;
   return count;
  }
  public int BurningHelicopterCount{get{int n=0;foreach(var h in helicopters)if(h!=null&&h.active&&h.wrecked)n++;return n;}}
  public static float HelicopterMissileInterval(float metres)=>metres>=1200?.5f:metres>=800?.6f:metres>=400?.8f:1;
  float AirMissileIntervalAfterTurns(float interval)=>Mathf.Max(EnemyMissileMinimumInterval,interval-EnemyMissileTurnReduction*CompletedTurns);
  float CurrentHelicopterMissileInterval=>AirMissileIntervalAfterTurns(HelicopterMissileInterval(Distance));
  HelicopterMissile CreateEnemyMissile()
  {
   if(!Application.isPlaying||helicopterMissilePrefab==null)return null;
   var root=Instantiate(helicopterMissilePrefab,transform).transform;root.name="Enemy missile "+(helicopterMissiles.Count+1);
   var missile=new HelicopterMissile{root=root,visual=root.GetComponent<MissileFlightVisual>(),flightAudio=CreateMissileFlightAudio(root,false)};
   helicopterMissiles.Add(missile);return missile;
  }
  void ResetHelicopterMissiles()
  {
   HelicopterMissilesFired=HelicopterMissilesDodged=TankMissilesFired=0;
   missileHitSource=EnemyMissileSource.None;RiderAirAttackBurning=false;
   if(Application.isPlaying&&helicopterMissilePrefab==null)helicopterMissilePrefab=Resources.Load<GameObject>("Helicopter/PV_HelicopterMissile");
   while(helicopterMissiles.Count<HelicopterMissileCapacity&&CreateEnemyMissile()!=null){}
   foreach(var m in helicopterMissiles){m.age=0;m.scroll=Vector3.zero;RetireHelicopterMissile(m);}
  }
  void TickHelicopterWeapon(HelicopterActor h,float dt)
  {
   float interval=CurrentHelicopterMissileInterval;
   if(State!=RideState.Riding||!RiderFlying||IsTurning||h.wrecked){h.shotInterval=h.shotRemaining=interval;return;}
   h.shotRemaining=Mathf.Max(0,h.shotRemaining+interval-h.shotInterval);h.shotInterval=interval;h.shotRemaining-=dt;
   Vector3 delta=rider.localPosition-h.root.localPosition;
   if((!h.turnFirstShotPending&&h.shotRemaining>.00001f)||delta.sqrMagnitude>125*125||Vector3.Dot(h.root.localRotation*Vector3.forward,delta)<8)return;
   // At most one missile per aircraft per control step. Never queue a burst while out of range.
   if(FireHelicopterMissile(h)){h.shotRemaining=interval;h.turnFirstShotPending=false;}else h.shotRemaining=.05f;
  }
  bool FireHelicopterMissile(HelicopterActor h)
  {
   if(h==null||!h.active||h.wrecked||State!=RideState.Riding||!RiderFlying)return false;
   Vector3 launcher=h.skywing!=null?new Vector3(0,-.55f,1.05f):new Vector3(0,-.15f,1.3f);
   Vector3 origin=h.root.localPosition+h.root.localRotation*launcher;
   return FireEnemyMissile(origin,h.skywing==null?EnemyMissileSource.Helicopter:EnemyMissileSource.Skywing);
  }
  bool FireEnemyMissile(Vector3 origin,EnemyMissileSource source)
  {
   HelicopterMissile free=null;foreach(var m in helicopterMissiles)if(!m.active){free=m;break;}
   free??=CreateEnemyMissile();if(free==null)return false;
   Vector3 target=rider.localPosition+Vector3.up*(PlayerHeight*.5f),offset=target-origin;
   // Lead only the road travel at launch. Steering/altitude changes after firing are evasion.
   Vector3 roadVelocity=Vector3.forward*Speed;
   float a=roadVelocity.sqrMagnitude-HelicopterMissileSpeed*HelicopterMissileSpeed,b=2*Vector3.Dot(offset,roadVelocity),c=offset.sqrMagnitude;
   float t=offset.magnitude/(HelicopterMissileSpeed+Mathf.Max(0,Speed)),disc=b*b-4*a*c;
   if(Mathf.Abs(a)>.001f&&disc>=0){float r1=(-b-Mathf.Sqrt(disc))/(2*a),r2=(-b+Mathf.Sqrt(disc))/(2*a);if(r1>0)t=r1;if(r2>0&&(r1<=0||r2<r1))t=r2;}
   free.velocity=(target+roadVelocity*t-origin).normalized*HelicopterMissileSpeed;free.scroll=Vector3.zero;free.age=0;free.dodgeCounted=false;
   free.source=source;
   free.root.localPosition=origin;free.root.localRotation=Quaternion.LookRotation(free.velocity);free.active=true;free.root.gameObject.SetActive(true);free.visual.Begin();
   bool fromTank=source==EnemyMissileSource.Tank;
   if(fromTank)TankMissilesFired++;else HelicopterMissilesFired++;
   PlayMissileAudio(free.flightAudio,origin,false);
   notice=fromTank?"TÊN LỬA XE TĂNG · ĐỔI LÀN / HẠ / LỘN NÉ":"TÊN LỬA · ĐỔI LÀN / HẠ / LỘN NÉ";noticeTimer=1.2f;return true;
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
     if(RiderRolling){if(!m.dodgeCounted){m.dodgeCounted=true;if(m.source!=EnemyMissileSource.Tank)HelicopterMissilesDodged++;}}
     else{Vector3 point=Vector3.Lerp(from,m.root.localPosition,at);AirAttackBurst(point);BurnAirAttackRider(point,m.source);RetireHelicopterMissile(m);continue;}
    }
    if(m.age>=HelicopterMissileLifetime||m.root.localPosition.z< -35||Mathf.Abs(m.root.localPosition.x)>38||m.root.localPosition.y<-.1f)RetireHelicopterMissile(m);
   }
  }
  void RetireHelicopterMissile(HelicopterMissile m){m.active=false;m.visual.End();m.flightAudio.Stop();m.root.gameObject.SetActive(false);}
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
  void BurnAirAttackRider(Vector3 point,EnemyMissileSource source=EnemyMissileSource.None,bool biplane=false)
  {
   if(State!=RideState.Riding||RiderAirAttackBurning)return;
   if(source==EnemyMissileSource.None)StartChase();
   Vector3 before=riderVelocityValid?riderRoadVelocity:rider.localRotation*Vector3.forward*Speed;
   Vector3 direction=before;direction.y=0;if(direction.sqrMagnitude<.1f)direction=rider.localRotation*Vector3.forward;direction.Normalize();
   BeginRiderImpact(null,new Vector2(direction.x,direction.z),point,before,source==EnemyMissileSource.None);
   riderFallVelocity=direction*Mathf.Clamp(before.magnitude*.6f,6,25)+Vector3.down*2;riderFallSpin=new Vector3(20,8,55);
   RiderAirAttackBurning=true;missileHitSource=source;
   if(source!=EnemyMissileSource.None)StopPursuitForFatalImpact();
   EnsureThunderstorm();stormVisual?.Burn(rider,null);
   notice=source!=EnemyMissileSource.None?"TRÚNG TÊN LỬA · "+MissileHitSourceLabel.ToUpperInvariant():biplane?"VA MÁY BAY · CSGT ĐANG ĐẾN":"VA TRỰC THĂNG · CSGT ĐANG ĐẾN";noticeTimer=5;
  }
  void KnockHelicopter(HelicopterActor h,Vector3 before,Vector3 away)
  {
   if(h.wrecked)return;h.wrecked=true;h.age=0;h.velocity=h.velocity*.55f+before*.15f-away*3+Vector3.down*2;
   h.wreckEuler=h.root.localEulerAngles;h.wreckSpin=new Vector3(25,16,away.x>=0?-65:65);
   if(h.skywing!=null)h.skywing.SetWrecked();else h.visual.SetWrecked();
   EnsureThunderstorm();stormVisual?.Burn(h.root,null,1.65f);AirAttackBurst(h.root.localPosition);
  }
  void TickHelicopterWreck(HelicopterActor h,float dt)
  {
   h.age+=dt;h.velocity.y-=9.81f*dt;h.velocity=LimitRoadVelocity(h.velocity);h.contactVelocity=h.velocity;
   Vector3 p=h.root.localPosition+h.velocity*dt-h.scroll;h.scroll=Vector3.zero;
   h.wreckEuler+=h.wreckSpin*dt;h.root.localRotation=Quaternion.Euler(h.wreckEuler);
   bool water=OverRiver(p);float floor=water?RiverBlock.WaterY-3:AircraftWreckFloor(h);
   if(p.y<=floor){p.y=floor;h.velocity=Vector3.MoveTowards(new Vector3(h.velocity.x,0,h.velocity.z),Vector3.zero,dt*12);h.wreckSpin*=Mathf.Exp(-dt*4);}
   h.root.localPosition=p;
   if(h.age>24||p.z< -40||Mathf.Abs(p.x)>38||water&&p.y<=floor){h.active=false;h.root.gameObject.SetActive(false);}
  }
  float AircraftWreckFloor(HelicopterActor h)
  {
   // Use the rotated parts, including wings and skids, rather than a fixed pivot height.
   float lowest=h.skywing!=null?h.skywing.WreckBottomY():h.visual.WreckBottomY();
   return .02f+h.root.position.y-lowest;
  }
  public bool IsAircraftBurning(Transform target)
  {
   if(target==rider)return RiderAirAttackBurning;
   foreach(var plane in skywings)if(plane.root==target)return plane.active&&plane.wrecked;
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
