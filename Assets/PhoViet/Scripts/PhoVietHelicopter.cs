using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public bool HelicopterActive=>helicopterActive;
  public int HelicoptersSpawned{get;private set;}
  public bool HitByHelicopter{get;private set;}
  public bool HelicopterSoundEnabled=>sound;
  public Transform HelicopterRoot=>helicopter;
  const float HelicopterSpeed=16;
  Transform helicopter;HelicopterVisual helicopterVisual;GameObject helicopterPrefab;
  bool helicopterActive,helicopterRebased;float helicopterAge;
  Vector3 helicopterVelocity,helicopterScroll,helicopterRebaseOrigin;Quaternion helicopterRebaseRotation;
  System.Random helicopterRng;
  void ResetHelicopter()
  {
   helicopterActive=helicopterRebased=false;helicopterScroll=Vector3.zero;helicopterAge=0;HelicoptersSpawned=0;HitByHelicopter=false;
   helicopterRng=new System.Random(RunSeed^0x47ad124);
   if(helicopter!=null)helicopter.gameObject.SetActive(false);
   if(!Application.isPlaying)return;
   if(helicopterPrefab==null)helicopterPrefab=Resources.Load<GameObject>("Helicopter/PV_Helicopter");
   if(helicopter==null&&helicopterPrefab!=null){helicopter=Instantiate(helicopterPrefab,transform).transform;helicopterVisual=helicopter.GetComponent<HelicopterVisual>();helicopter.gameObject.SetActive(false);}
  }
  bool SpawnHelicopter()
  {
   if(helicopter==null||helicopterActive||!RiderFlying||IsTurning||State!=RideState.Riding)return false;
   helicopter.localPosition=new Vector3(LaneX[helicopterRng.Next(2)],rider.localPosition.y+PlayerHeight*.5f,Mathf.Max(95,(Speed+HelicopterSpeed)*2.8f));
   helicopter.localRotation=Quaternion.Euler(0,180,0);helicopterVelocity=Vector3.back*HelicopterSpeed;
   helicopterAge=0;helicopterActive=true;helicopter.gameObject.SetActive(true);helicopterVisual.ResetPose();HelicoptersSpawned++;
   notice="TRỰC THĂNG NGƯỢC CHIỀU · CHÚ Ý KHI BAY";noticeTimer=2;return true;
  }
  void ScrollHelicopter(float delta){if(helicopterActive)helicopterScroll+=Vector3.forward*delta;}
  void RebaseHelicopter(Quaternion inverse,Vector3 origin)
  {
   if(!helicopterActive)return;
   helicopter.localPosition=inverse*(helicopter.localPosition-origin);helicopter.localRotation=inverse*helicopter.localRotation;
   helicopterVelocity=inverse*helicopterVelocity;helicopterScroll=inverse*helicopterScroll;
   helicopterRebased=true;helicopterRebaseRotation=inverse;helicopterRebaseOrigin=origin;
  }
  void TickHelicopter(float dt,Vector3 previousRider,bool collisions=true)
  {
   if(dt<=0||State==RideState.Paused)return;
   // Both ends of a collision sweep must use the destination street coordinates.
   if(helicopterRebased){previousRider=helicopterRebaseRotation*(previousRider-helicopterRebaseOrigin);helicopterRebased=false;}
   if(!helicopterActive){helicopterScroll=Vector3.zero;if(collisions)SpawnHelicopter();return;}
   Vector3 from=helicopter.localPosition;helicopter.localPosition+=helicopterVelocity*dt-helicopterScroll;helicopterScroll=Vector3.zero;helicopterAge+=dt;
   if(collisions&&State==RideState.Riding)
   {
    int steps=Mathf.Max(1,Mathf.CeilToInt(((helicopter.localPosition-from)-(rider.localPosition-previousRider)).magnitude/.22f));
    for(int i=0;i<=steps;i++)
    {
     float t=(float)i/steps;Vector3 at=Vector3.Lerp(from,helicopter.localPosition,t),p=Vector3.Lerp(previousRider,rider.localPosition,t);
     if(!HelicopterTouches(at,helicopter.localRotation,p,rider.localRotation))continue;
     Vector3 away=p-at;away.y=0;if(away.sqrMagnitude<.01f)away=Vector3.back;away.Normalize();
     Vector3 before=riderRoadVelocity;rider.localPosition=p;BeginRiderImpact(null,new Vector2(-away.x,-away.z),at,before);
     riderFallVelocity=(before-helicopterVelocity)*.30f+away*8+Vector3.up*3;HitByHelicopter=true;
     notice=CrashDuringPursuit?"VA TRỰC THĂNG · CHỜ CSGT ĐẾN BẮT":"VA TRỰC THĂNG · CHUYẾN ĐI KẾT THÚC";noticeTimer=4;break;
    }
   }
   if(helicopter.localPosition.z< -32||Mathf.Abs(helicopter.localPosition.x)>38||helicopterAge>14)
   {helicopterActive=false;helicopter.gameObject.SetActive(false);}
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
