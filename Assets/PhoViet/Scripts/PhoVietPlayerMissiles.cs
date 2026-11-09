using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const int MaximumPlayerMissiles=5,PlayerMissilePoolCapacity=12;
  public const float PlayerMissileInterval=.4f;
  const float PlayerMissileLifetime=4;
  public int PlayerMissiles{get;private set;}=MaximumPlayerMissiles;
  public int PlayerMissilesFired{get;private set;}
  public int PlayerHelicoptersDowned{get;private set;}
  public float PlayerMissileCooldown=>playerMissileCooldown;
  public int ActivePlayerMissileCount{get{int n=0;foreach(var m in playerMissiles)if(m!=null&&m.active)n++;return n;}}
  public bool PlayerCanFire=>State==RideState.Riding&&(RiderFlying||!RiderAirborne)&&!ServiceStopped&&PlayerMissiles>0&&playerMissileCooldown<=.00001f;
  sealed class PlayerMissile
  {public Transform root;public MissileFlightVisual visual;public AudioSource flightAudio;public HelicopterActor target;public TrafficVehicle tankTarget;public int targetGeneration,tankGeneration;public bool active,groundShot;public Vector3 velocity,scroll;public float age,speed;}
  readonly PlayerMissile[] playerMissiles=new PlayerMissile[PlayerMissilePoolCapacity];float playerMissileCooldown;
  void ResetPlayerMissiles()
  {
   PlayerMissiles=MaximumPlayerMissiles;PlayerMissilesFired=PlayerHelicoptersDowned=PlayerTanksDestroyed=0;playerMissileCooldown=0;
   if(Application.isPlaying&&helicopterMissilePrefab==null)helicopterMissilePrefab=Resources.Load<GameObject>("Helicopter/PV_HelicopterMissile");
   for(int i=0;i<playerMissiles.Length;i++)
   {
    var m=playerMissiles[i];
    if(m==null&&Application.isPlaying&&helicopterMissilePrefab!=null)
    {var root=Instantiate(helicopterMissilePrefab,transform).transform;root.name="Tên lửa người chơi • "+(i+1);m=playerMissiles[i]=new PlayerMissile{root=root,visual=root.GetComponent<MissileFlightVisual>(),flightAudio=CreateMissileFlightAudio(root,true)};}
    if(m!=null)RetirePlayerMissile(m);
   }
   ResetMissilePickups();
  }
  HelicopterActor PlayerMissileTarget(Vector3 origin,Vector3 forward)
  {
   HelicopterActor best=null;float distance=160*160;
   foreach(var h in helicopters)if(h!=null&&h.active&&!h.wrecked)
   {Vector3 delta=h.root.localPosition-origin;if(Vector3.Dot(delta,forward)<=0||delta.sqrMagnitude>=distance)continue;distance=delta.sqrMagnitude;best=h;}
   return best;
  }
  public bool TryFirePlayerMissile()
  {
   if(!PlayerCanFire)return false;
   PlayerMissile free=null;foreach(var m in playerMissiles)if(m!=null&&!m.active){free=m;break;}if(free==null)return false;
   // Roll is a visual evasion manoeuvre, not an instruction to launch into the
   // road: use travel yaw and place the launcher just beyond the front bumper.
   Vector3 forward=Quaternion.Euler(0,rider.localEulerAngles.y,0)*Vector3.forward;
   Vector3 origin=rider.localPosition+forward*(PlayerHalfLength+.85f)+Vector3.up*(PlayerHeight*.55f);
   bool ground=!RiderFlying;
   var target=ground?null:PlayerMissileTarget(origin,forward);
   var tank=PlayerTankTarget(origin,forward);
   if(target!=null&&tank!=null)
   {
    if((PlayerTankAim(tank)-origin).sqrMagnitude<(target.root.localPosition-origin).sqrMagnitude)target=null;
    else tank=null;
   }
   Vector3 direction=target!=null?(target.root.localPosition+Vector3.up*.1f-origin).normalized:tank!=null?(PlayerTankAim(tank)-origin).normalized:forward;
   free.target=target;free.targetGeneration=target!=null?target.generation:0;free.speed=Mathf.Max(70,Speed+50);free.velocity=direction*free.speed;
   free.groundShot=ground;free.tankTarget=tank;free.tankGeneration=tank!=null?tank.spawnGeneration:0;
   free.scroll=Vector3.zero;free.age=0;free.root.localPosition=origin;free.root.localRotation=Quaternion.LookRotation(direction);free.active=true;free.root.gameObject.SetActive(true);free.visual.Begin();
   PlayMissileAudio(free.flightAudio,origin,true);
   PlayerMissiles--;PlayerMissilesFired++;playerMissileCooldown=PlayerMissileInterval;
   notice=target!=null?"TÊN LỬA · ĐÃ KHÓA TRỰC THĂNG":tank!=null?"TÊN LỬA · ĐÃ KHÓA XE TĂNG":"TÊN LỬA · BAY THẲNG";noticeTimer=1.1f;return true;
  }
  void ScrollPlayerMissiles(float delta){foreach(var m in playerMissiles)if(m!=null&&m.active){m.scroll+=Vector3.forward*delta;m.visual.Scroll(delta);}}
  void RebasePlayerMissiles(Quaternion inverse,Vector3 origin)
  {foreach(var m in playerMissiles)if(m!=null&&m.active){m.root.localPosition=inverse*(m.root.localPosition-origin);m.root.localRotation=inverse*m.root.localRotation;m.velocity=inverse*m.velocity;m.scroll=inverse*m.scroll;m.visual.RebaseTrail();}}
  void TickPlayerMissiles(float dt)
  {
   if(dt<=0||State==RideState.Paused)return;
   if(State==RideState.Riding)playerMissileCooldown=Mathf.Max(0,playerMissileCooldown-dt);
   foreach(var m in playerMissiles)if(m!=null&&m.active)
   {
    Vector3 frameScroll=m.scroll,from=m.root.localPosition-frameScroll;m.scroll=Vector3.zero;
    if(m.tankTarget!=null)
    {
     var tank=m.tankTarget;
     if(!PlayerTankTargetValid(tank)||tank.spawnGeneration!=m.tankGeneration)m.tankTarget=null;
     else{Vector3 desired=PlayerTankAim(tank)-from;if(desired.sqrMagnitude>.001f)m.velocity=Vector3.RotateTowards(m.velocity,desired.normalized*m.speed,300*Mathf.Deg2Rad*dt,0);}
    }
    if(m.target!=null)
    {
     if(!m.target.active||m.target.wrecked||m.target.generation!=m.targetGeneration)m.target=null;
     else
     {
      Vector3 aim=m.target.root.localPosition+Vector3.up*.1f;
      Vector3 desired=aim-from;
      if(desired.sqrMagnitude>.001f)m.velocity=Vector3.RotateTowards(m.velocity,desired.normalized*m.speed,300*Mathf.Deg2Rad*dt,0);
     }
    }
    Vector3 to=from+m.velocity*dt;m.age+=dt;
    HelicopterActor hit=null;float first=2;
    TrafficVehicle hitTank=null;
    foreach(var tank in vehicles)
     if(tank.active&&tank.armored&&!tank.riverFalling&&PlayerMissileTouchesTank(from,to,tank,frameScroll,out float at)&&at<first)
     {hitTank=tank;first=at;}
    if(!m.groundShot)
     foreach(var h in helicopters)
      if(h!=null&&h.active&&!h.wrecked&&PlayerMissileTouchesHelicopter(from,to,h,out float at)&&at<first)
      {hit=h;hitTank=null;first=at;}
    if(hitTank!=null){m.root.localPosition=Vector3.Lerp(from,to,first);DestroyPlayerTank(hitTank,m.root.localPosition);RetirePlayerMissile(m);continue;}
    if(hit!=null)
    {
     m.root.localPosition=Vector3.Lerp(from,to,first);
     KnockHelicopter(hit,m.velocity*.2f,-m.velocity.normalized);PlayerHelicoptersDowned++;
     notice="HẠ TRỰC THĂNG · TRÚNG TÊN LỬA";noticeTimer=1.6f;RetirePlayerMissile(m);continue;
    }
    m.root.localPosition=to;m.root.localRotation=Quaternion.LookRotation(m.velocity);
    if(m.age>=PlayerMissileLifetime||Mathf.Abs(to.x)>180||to.z< -60||to.z>420||to.y<-.5f)RetirePlayerMissile(m);
   }
  }
  static bool PlayerMissileTouchesHelicopter(Vector3 from,Vector3 to,HelicopterActor h,out float at)
  {
   // Relative swept motion includes the aircraft travelling toward the car and
   // the shared road scroll. Sample a capsule along the fuselage/tail, not the
   // empty corners of a giant box containing the whole rotor.
   Quaternion inverse=Quaternion.Inverse(h.root.localRotation);
   Vector3 a=inverse*(from-h.missilePrevious),b=inverse*(to-h.root.localPosition),d=b-a;at=2;
   for(int i=0;i<=20;i++)
   {
    float z=Mathf.Lerp(-4.3f,2.1f,i/20f),radius=z< -1.4f?.31f:.83f;
    Vector3 p=a-new Vector3(0,z< -1.4f?.2f:.1f,z);
    float c=p.sqrMagnitude-radius*radius;if(c<=0){at=0;break;}
    float aa=d.sqrMagnitude,bb=Vector3.Dot(p,d),disc=bb*bb-aa*c;
    if(aa<1e-8f||disc<0)continue;float t=(-bb-Mathf.Sqrt(disc))/aa;if(t>=0&&t<=1)at=Mathf.Min(at,t);
   }
   // Rotor disc: clip by its thin vertical slab, then solve the horizontal circle.
   float enter=0,leave=1,y=a.y-1.125f;
   if(Mathf.Abs(d.y)<1e-7f){if(Mathf.Abs(y)>.2f)return at<=1;}
   else{float p=(-.2f-y)/d.y,q=(.2f-y)/d.y;enter=Mathf.Max(0,Mathf.Min(p,q));leave=Mathf.Min(1,Mathf.Max(p,q));}
   if(enter<=leave)
   {
    float aa=d.x*d.x+d.z*d.z,bb=a.x*d.x+a.z*d.z,c=a.x*a.x+a.z*a.z-2.68f*2.68f;
    if(aa<1e-8f){if(c<=0)at=Mathf.Min(at,enter);}
    else{float disc=bb*bb-aa*c;if(disc>=0){float p=(-bb-Mathf.Sqrt(disc))/aa,q=(-bb+Mathf.Sqrt(disc))/aa;float t=Mathf.Max(enter,p);if(t<=Mathf.Min(leave,q))at=Mathf.Min(at,t);}}
   }
   return at<=1;
  }
  void RetirePlayerMissile(PlayerMissile m){m.active=false;m.target=null;m.tankTarget=null;m.groundShot=false;m.scroll=Vector3.zero;m.age=0;m.visual.End();m.flightAudio.Stop();m.root.gameObject.SetActive(false);}
 }
}
