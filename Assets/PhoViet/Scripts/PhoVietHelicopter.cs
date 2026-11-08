using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const int MaximumHelicopters=2;
  const float HelicopterSpeed=16,HelicopterSpacing=12,TurnHelicopterHeadClearance=1.55f;
  sealed class HelicopterActor
  {
   public Transform root;public HelicopterVisual visual;public bool active,wrecked,turnWingmanPending;
   public float age,shotRemaining,shotInterval;public Vector3 velocity,scroll,wreckEuler,wreckSpin,missilePrevious;public int generation;
  }
  readonly HelicopterActor[] helicopters=new HelicopterActor[MaximumHelicopters];
  readonly int[] helicopterLanes={0,1,2,3};int helicopterLaneCursor=4;
  GameObject helicopterPrefab;System.Random helicopterRng;
  bool helicopterRebased;Vector3 helicopterRebaseOrigin;Quaternion helicopterRebaseRotation;
  public bool HelicopterActive=>ActiveHelicopterCount>0;
  public int ActiveHelicopterCount{get{int count=0;foreach(var h in helicopters)if(h!=null&&h.active)count++;return count;}}
  public int HelicopterPoolCount{get{int count=0;foreach(var h in helicopters)if(h!=null)count++;return count;}}
  public int HelicopterTargetCount=>HelicopterLimitAtDistance(Distance);
  public int HelicoptersSpawned{get;private set;}
  public bool HitByHelicopter{get;private set;}
  public bool HelicopterSoundEnabled=>sound;
  public Transform HelicopterRoot=>GetHelicopterRoot(0);
  public Transform GetHelicopterRoot(int index)=>index>=0&&index<helicopters.Length?helicopters[index]?.root:null;
  public bool IsHelicopterActive(int index)=>index>=0&&index<helicopters.Length&&helicopters[index]!=null&&helicopters[index].active;
  // The second slot unlocks strictly past 500 scored metres.
  public static int HelicopterLimitAtDistance(float metres)=>metres>500?MaximumHelicopters:1;
  void ResetHelicopter()
  {
   helicopterRebased=false;HelicoptersSpawned=0;HitByHelicopter=false;helicopterLaneCursor=4;
   helicopterRng=new System.Random(RunSeed^0x47ad124);
   for(int i=0;i<helicopterLanes.Length;i++)helicopterLanes[i]=i;
   if(Application.isPlaying&&helicopterPrefab==null)helicopterPrefab=Resources.Load<GameObject>("Helicopter/PV_Helicopter");
   for(int i=0;i<helicopters.Length;i++)
   {
    var h=helicopters[i];
    if(h==null&&Application.isPlaying&&helicopterPrefab!=null)
    {
     var root=Instantiate(helicopterPrefab,transform).transform;root.name="Helicopter "+(i+1);
     h=helicopters[i]=new HelicopterActor{root=root,visual=root.GetComponent<HelicopterVisual>()};
    }
    if(h==null)continue;h.active=h.wrecked=h.turnWingmanPending=false;h.age=0;h.shotInterval=h.shotRemaining=HelicopterMissileInterval(Distance);h.velocity=h.scroll=Vector3.zero;h.root.gameObject.SetActive(false);
   }
   ResetHelicopterMissiles();
   ResetPlayerMissiles();
   ResetMissileLaunchAudio();
  }
  int NextHelicopterLane()
  {
   if(helicopterLaneCursor>=4)
   {
    for(int i=3;i>0;i--){int j=helicopterRng.Next(i+1),lane=helicopterLanes[i];helicopterLanes[i]=helicopterLanes[j];helicopterLanes[j]=lane;}
    helicopterLaneCursor=0;
   }
   return helicopterLanes[helicopterLaneCursor++];
  }
  bool HelicopterLaneOccupied(int lane)
  {
   foreach(var h in helicopters)if(h!=null&&h.active&&!h.wrecked&&h.velocity.z< -1&&Mathf.Abs(h.velocity.x)<.1f&&Mathf.Abs(h.root.localPosition.x-LaneX[lane])<.1f)return true;
   return false;
  }
  bool SpawnHelicopter()
  {
   if(!RiderFlying||IsTurning||State!=RideState.Riding)return false;
   var free=FreeHelicopterSlot();if(free==null)return false;
   float z=Mathf.Max(95,(Speed+HelicopterSpeed)*2.8f);
   // Stagger the formation so neighbouring main rotors and repeated lanes cannot overlap.
   foreach(var h in helicopters)if(h!=null&&h.active&&!h.wrecked&&h.velocity.z< -1&&Mathf.Abs(h.velocity.x)<.1f)z=Mathf.Max(z,h.root.localPosition.z+HelicopterSpacing);
   LaunchHelicopter(free,new Vector3(NextFreeHelicopterLaneX(),rider.localPosition.y+PlayerHeight*.5f,z));
   notice="TRỰC THĂNG NGƯỢC CHIỀU · CHÚ Ý KHI BAY";noticeTimer=2;return true;
  }
  HelicopterActor FreeHelicopterSlot()
  {
   if(ActiveHelicopterCount>=HelicopterTargetCount)return null;
   foreach(var h in helicopters)if(h!=null&&!h.active)return h;
   return null;
  }
  float NextFreeHelicopterLaneX()
  {
   int lane=NextHelicopterLane();
   for(int attempt=1;attempt<8&&HelicopterLaneOccupied(lane);attempt++)lane=NextHelicopterLane();
   return LaneX[lane];
  }
  void LaunchHelicopter(HelicopterActor actor,Vector3 position)
  {
   actor.root.localPosition=actor.missilePrevious=position;
   actor.root.localRotation=Quaternion.Euler(0,180,0);actor.velocity=Vector3.back*HelicopterSpeed;
   actor.generation++;actor.age=0;actor.scroll=Vector3.zero;
   actor.shotInterval=actor.shotRemaining=HelicopterMissileInterval(Distance);
   actor.active=true;actor.wrecked=actor.turnWingmanPending=false;
   actor.root.gameObject.SetActive(true);actor.visual.ResetPose();HelicoptersSpawned++;
  }
  void SpawnTurnHelicopter(Dinosaur dinosaur)
  {
   var lead=FreeHelicopterSlot();
   foreach(var h in helicopters)
   {
    if(h==null||!h.active)continue;
    // Prioritize the next junction over an aircraft from the previous street.
    if(lead==null||(lead.active&&Mathf.Abs(h.root.localPosition.x)>Mathf.Abs(lead.root.localPosition.x)))lead=h;
    h.turnWingmanPending=false;
   }
   if(lead==null)return;
   Vector3 head=DinosaurPoint(dinosaur.visual.head);
   LaunchHelicopter(lead,head+Vector3.up*TurnHelicopterHeadClearance);
   lead.scroll=Vector3.forward*dinosaurScroll;
   lead.turnWingmanPending=true;
   TryTurnHelicopterWingman(lead);
  }
  void TryTurnHelicopterWingman(HelicopterActor lead)
  {
   if(HelicopterTargetCount<2||lead.root.localPosition.z<=0)return;
   var free=FreeHelicopterSlot();
   if(free==null)
   {
    foreach(var h in helicopters)
    {
     if(h==null||h==lead||!h.active)continue;
     if(!h.wrecked&&h.velocity.z< -1&&h.root.localPosition.z>0)break;
     free=h;break;
    }
   }
   if(free!=null)
   {
    float z=Mathf.Max(28,lead.root.localPosition.z+HelicopterSpacing);
    LaunchHelicopter(free,new Vector3(NextFreeHelicopterLaneX(),FlightHeightAboveSupport+PlayerHeight*.5f,z));
   }
   lead.turnWingmanPending=false;
  }
  void ScrollHelicopter(float delta){foreach(var h in helicopters)if(h!=null&&h.active)h.scroll+=Vector3.forward*delta;ScrollHelicopterMissiles(delta);ScrollPlayerMissiles(delta);ScrollMissileLaunchAudio(delta);}
  void RebaseHelicopter(Quaternion inverse,Vector3 origin)
  {
   RebaseMissileLaunchAudio(inverse,origin);
   if(!HelicopterActive&&ActiveHelicopterMissileCount==0&&ActiveTankMissileCount==0&&ActivePlayerMissileCount==0)return;
   foreach(var h in helicopters)if(h!=null&&h.active)
   {
    h.root.localPosition=inverse*(h.root.localPosition-origin);h.root.localRotation=inverse*h.root.localRotation;
    h.velocity=inverse*h.velocity;h.scroll=inverse*h.scroll;if(h.wrecked){h.wreckEuler=h.root.localEulerAngles;h.wreckSpin=inverse*h.wreckSpin;}
   }
   RebaseHelicopterMissiles(inverse,origin);
   RebasePlayerMissiles(inverse,origin);
   helicopterRebased=true;helicopterRebaseRotation=inverse;helicopterRebaseOrigin=origin;
  }
  void TickHelicopter(float dt,Vector3 previousRider,bool collisions=true)
  {
   if(dt<=0||State==RideState.Paused)return;
   // Both ends of every collision sweep use the destination street coordinates.
   if(helicopterRebased){previousRider=helicopterRebaseRotation*(previousRider-helicopterRebaseOrigin);helicopterRebased=false;}
   foreach(var h in helicopters)if(h!=null&&h.active){h.missilePrevious=h.root.localPosition-h.scroll;TickHelicopterActor(h,dt,previousRider,collisions);}
   TickPlayerMissiles(dt);
   TickHelicopterMissiles(dt,previousRider,collisions);
   if(!collisions||State!=RideState.Riding||IsTurning)return;
   foreach(var h in helicopters)if(h!=null&&h.active&&!h.wrecked&&h.turnWingmanPending)TryTurnHelicopterWingman(h);
   while(SpawnHelicopter()){}
  }
  void TickHelicopterActor(HelicopterActor h,float dt,Vector3 previousRider,bool collisions)
  {
   if(h.wrecked){TickHelicopterWreck(h,dt);return;}
   Vector3 from=h.root.localPosition;h.root.localPosition+=h.velocity*dt-h.scroll;h.scroll=Vector3.zero;h.age+=dt;
   if(collisions&&State==RideState.Riding)
   {
    int steps=Mathf.Max(1,Mathf.CeilToInt(((h.root.localPosition-from)-(rider.localPosition-previousRider)).magnitude/.22f));
    for(int i=0;i<=steps;i++)
    {
     float t=(float)i/steps;Vector3 at=Vector3.Lerp(from,h.root.localPosition,t),p=Vector3.Lerp(previousRider,rider.localPosition,t);
     if(!HelicopterTouches(at,h.root.localRotation,p,rider.localRotation))continue;
     Vector3 away=p-at;away.y=0;if(away.sqrMagnitude<.01f)away=Vector3.back;away.Normalize();
     Vector3 before=riderRoadVelocity;rider.localPosition=p;h.root.localPosition=at;
     KnockHelicopter(h,before,away);HitByHelicopter=true;BurnAirAttackRider(at,false);break;
    }
   }
   if(collisions&&!h.wrecked)TickHelicopterWeapon(h,dt);
   if(h.root.localPosition.z< -32||Mathf.Abs(h.root.localPosition.x)>38||h.age>14)
   {h.active=false;h.root.gameObject.SetActive(false);}
  }
  bool HelicopterTouches(Vector3 at,Quaternion heading,Vector3 p,Quaternion rotation)
  {
   // Test the fuselage capsule and the full rotor disc against the car's 3D body.
   Quaternion inverse=Quaternion.Inverse(rotation);Vector3 half=new Vector3(PlayerHalfWidth,PlayerHeight*.5f,PlayerHalfLength),center=p+Vector3.up*half.y;
   for(int i=0;i<=12;i++)
   {
    Vector3 q=inverse*(at+heading*new Vector3(0,.1f,Mathf.Lerp(-1.4f,2.1f,i/12f))-center);
    Vector3 d=q-new Vector3(Mathf.Clamp(q.x,-half.x,half.x),Mathf.Clamp(q.y,-half.y,half.y),Mathf.Clamp(q.z,-half.z,half.z));
    if(d.sqrMagnitude<=.70f*.70f)return true;
   }
   Vector3 rotor=at+Vector3.up*1.125f,local=inverse*(rotor-center);
   if(Mathf.Abs(local.y)<=half.y+.12f)
   {float x=Mathf.Max(0,Mathf.Abs(local.x)-half.x),z=Mathf.Max(0,Mathf.Abs(local.z)-half.z);if(x*x+z*z<=2.55f*2.55f)return true;}
   // A thin tail boom remains solid without treating its surrounding air as a box.
   for(int i=0;i<=7;i++)
   {
    Vector3 q=inverse*(at+heading*new Vector3(0,.2f,Mathf.Lerp(-1.4f,-4.3f,i/7f))-center);
    if(Mathf.Abs(q.x)<half.x+.18f&&Mathf.Abs(q.y)<half.y+.20f&&Mathf.Abs(q.z)<half.z+.18f)return true;
   }
   return false;
  }
 }
}
