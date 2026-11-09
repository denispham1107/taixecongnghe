using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const int MaximumDistanceHelicopters=3;
  const float HelicopterDistanceStep=200;
  const float HelicopterSpeed=16,HelicopterSpacing=12,AdjacentHelicopterSpacing=6,TurnHelicopterHeadClearance=1.55f;
  sealed class HelicopterActor
  {
   public Transform root;public HelicopterVisual visual;public bool active,wrecked,turnFirstShotPending;
   public float age,shotRemaining,shotInterval;public Vector3 velocity,scroll,wreckEuler,wreckSpin,missilePrevious;public int generation;
  }
  readonly List<HelicopterActor> helicopters=new List<HelicopterActor>(MaximumDistanceHelicopters);
  readonly int[] helicopterLanes={0,1,2,3};int helicopterLaneCursor=4;
  GameObject helicopterPrefab;System.Random helicopterRng;
  bool helicopterRebased,turnHelicopterReinforcements;Vector3 helicopterRebaseOrigin;Quaternion helicopterRebaseRotation;
  public bool HelicopterActive=>ActiveHelicopterCount>0;
  public int ActiveHelicopterCount{get{int count=0;foreach(var h in helicopters)if(h!=null&&h.active)count++;return count;}}
  public int HelicopterPoolCount=>helicopters.Count;
  public int HelicopterTargetCount=>HelicopterLimitAtDistance(Distance)+CompletedTurns;
  public int HelicoptersSpawned{get;private set;}
  public bool HitByHelicopter{get;private set;}
  public bool HelicopterSoundEnabled=>sound;
  public Transform HelicopterRoot=>GetHelicopterRoot(0);
  public Transform GetHelicopterRoot(int index)=>index>=0&&index<helicopters.Count?helicopters[index].root:null;
  public bool IsHelicopterActive(int index)=>index>=0&&index<helicopters.Count&&helicopters[index].active;
  public static int HelicopterLimitAtDistance(float metres)
   =>metres>=2*HelicopterDistanceStep?MaximumDistanceHelicopters:metres>=HelicopterDistanceStep?2:1;
  HelicopterActor CreateHelicopter()
  {
   if(!Application.isPlaying||helicopterPrefab==null)return null;
   var root=Instantiate(helicopterPrefab,transform).transform;root.name="Helicopter "+(helicopters.Count+1);
   var actor=new HelicopterActor{root=root,visual=root.GetComponent<HelicopterVisual>()};
   helicopters.Add(actor);return actor;
  }
  void ResetHelicopter()
  {
   helicopterRebased=turnHelicopterReinforcements=false;HelicoptersSpawned=0;HitByHelicopter=false;helicopterLaneCursor=4;
   helicopterRng=new System.Random(RunSeed^0x47ad124);
   for(int i=0;i<helicopterLanes.Length;i++)helicopterLanes[i]=i;
   if(Application.isPlaying&&helicopterPrefab==null)helicopterPrefab=Resources.Load<GameObject>("Helicopter/PV_Helicopter");
   while(helicopters.Count<MaximumDistanceHelicopters&&CreateHelicopter()!=null){}
   foreach(var h in helicopters)
   {
    h.active=h.wrecked=h.turnFirstShotPending=false;h.age=0;h.shotInterval=h.shotRemaining=CurrentHelicopterMissileInterval;h.velocity=h.scroll=Vector3.zero;h.root.gameObject.SetActive(false);
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
   var free=FreeHelicopterSlot();if(free==null)return false;
   float laneX=NextFreeHelicopterLaneX();
   float z=Mathf.Max(95,(Speed+HelicopterSpeed)*2.8f);
   foreach(var h in helicopters)
   {
    if(!h.active||h.wrecked||h.velocity.z>=-1||Mathf.Abs(h.velocity.x)>=.1f)continue;
    float lateral=Mathf.Abs(h.root.localPosition.x-laneX);
    if(CompletedTurns==0||lateral<.1f)z=Mathf.Max(z,h.root.localPosition.z+HelicopterSpacing);
    else if(lateral<5.5f)z=Mathf.Max(z,h.root.localPosition.z+AdjacentHelicopterSpacing);
   }
   float altitude=RiderFlying?rider.localPosition.y:FlightHeightAboveSupport;
   LaunchHelicopter(free,new Vector3(laneX,altitude+PlayerHeight*.5f,z));
   if(RiderFlying){notice="TRỰC THĂNG NGƯỢC CHIỀU · CHÚ Ý KHI BAY";noticeTimer=2;}
   return true;
  }
  HelicopterActor FreeHelicopterSlot()
  {
   if(ActiveHelicopterCount>=HelicopterTargetCount)return null;
   foreach(var h in helicopters)if(!h.active)return h;
   return CreateHelicopter();
  }
  float NextFreeHelicopterLaneX()
  {
   int lane=NextHelicopterLane();
   for(int attempt=1;attempt<8&&HelicopterLaneOccupied(lane);attempt++)lane=NextHelicopterLane();
   return LaneX[lane];
  }
  void LaunchHelicopter(HelicopterActor actor,Vector3 position,bool postTurn=false)
  {
   actor.root.localPosition=actor.missilePrevious=position;
   actor.root.localRotation=Quaternion.Euler(0,180,0);actor.velocity=Vector3.back*HelicopterSpeed;
   actor.generation++;actor.age=0;actor.scroll=Vector3.zero;
   actor.shotInterval=actor.shotRemaining=CurrentHelicopterMissileInterval;
   actor.active=true;actor.wrecked=false;
   actor.turnFirstShotPending=postTurn;
   actor.root.gameObject.SetActive(true);actor.visual.ResetPose();HelicoptersSpawned++;
   if(postTurn)TickHelicopterWeapon(actor,0);
  }
  void SpawnTurnHelicopter(Dinosaur dinosaur)
  {
   var lead=FreeHelicopterSlot();
   foreach(var h in helicopters)
   {
    if(h==null||!h.active)continue;
    // Prioritize the next junction over an aircraft from the previous street.
    if(lead==null||(lead.active&&Mathf.Abs(h.root.localPosition.x)>Mathf.Abs(lead.root.localPosition.x)))lead=h;
   }
   if(lead==null)return;
   Vector3 head=DinosaurPoint(dinosaur.visual.head);
   LaunchHelicopter(lead,head+Vector3.up*TurnHelicopterHeadClearance,true);
   lead.scroll=Vector3.forward*dinosaurScroll;
   turnHelicopterReinforcements=ActiveHelicopterCount<HelicopterTargetCount;
  }
  void ScrollHelicopter(float delta){foreach(var h in helicopters)if(h!=null&&h.active)h.scroll+=Vector3.forward*delta;ScrollHelicopterMissiles(delta);ScrollPlayerMissiles(delta);ScrollMissileLaunchAudio(delta);}
  void RebaseHelicopter(Quaternion inverse,Vector3 origin)
  {
   turnHelicopterReinforcements=false;
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
   if(!RiderFlying&&!turnHelicopterReinforcements)return;
   // Fill the opening formation immediately; feed larger formations over successive steps.
   for(int i=0;i<MaximumDistanceHelicopters&&SpawnHelicopter();i++){}
   if(ActiveHelicopterCount>=HelicopterTargetCount)turnHelicopterReinforcements=false;
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
