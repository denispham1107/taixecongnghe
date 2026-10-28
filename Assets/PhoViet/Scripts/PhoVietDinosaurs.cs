using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public bool dinosaursEnabled=true;
  public GameObject dinosaurPrefab;
  public const float DinosaurInterval=10,DinosaurWalkSpeed=3,DinosaurWidth=4.65f;
  public int DinosaursSpawned {get;private set;}
  public int DinosaurTailHits {get;private set;}
  public bool HitByDinosaur {get;private set;}
  public float DinosaurCountdown=>dinosaurTimer;
  public sealed class Dinosaur
  {
   public Transform root;public DinosaurVisual visual;public bool active;
   public readonly HashSet<TrafficVehicle> hits=new HashSet<TrafficVehicle>();
   public bool hitPolice,hitRider;public float nextSweep;
   public Vector3[] previous=new Vector3[6];
  }
  public readonly List<Dinosaur> dinosaurs=new List<Dinosaur>();
  float dinosaurTimer=DinosaurInterval,dinosaurScroll;
  Vector3 dinosaurPoliceVelocity;
  void HideDinosaurs(){foreach(var d in dinosaurs){d.active=false;d.root.gameObject.SetActive(false);}}
  void ResetDinosaurs()
  {
   dinosaurTimer=DinosaurInterval;dinosaurScroll=0;dinosaurPoliceVelocity=Vector3.zero;DinosaursSpawned=0;DinosaurTailHits=0;HitByDinosaur=false;
   foreach(var d in dinosaurs){d.active=false;d.root.gameObject.SetActive(false);d.hits.Clear();}
  }
  public Dinosaur SpawnDinosaur()
  {return SpawnDinosaurAt(Mathf.Max(110,Speed*2.5f+20));}
  Dinosaur SpawnDinosaurAt(float z)
  {
   if(!dinosaursEnabled||dinosaurPrefab==null)return null;
   Dinosaur d=dinosaurs.Find(v=>!v.active);
   if(d==null)
   {
    if(dinosaurs.Count>=4)return null;
    var go=Instantiate(dinosaurPrefab,transform);d=new Dinosaur{root=go.transform,visual=go.GetComponent<DinosaurVisual>()};dinosaurs.Add(d);
   }
   d.active=true;d.root.gameObject.SetActive(true);d.root.localRotation=Quaternion.Euler(0,180,0);
   d.root.localPosition=new Vector3(0,0,z+dinosaurScroll);
   d.hits.Clear();d.hitPolice=d.hitRider=false;d.nextSweep=0;d.visual.ResetPose();
   CopyDinosaurTail(d);DinosaursSpawned++;
   notice="KHỦNG LONG PHÍA TRƯỚC · NHẢY NÉ ĐUÔI";noticeTimer=2;
   return d;
  }
  void ScrollDinosaurs(float delta){dinosaurScroll+=delta;}
  void RebaseDinosaurs(Quaternion inverse,Vector3 origin)
  {
   dinosaurPoliceVelocity=inverse*dinosaurPoliceVelocity;
   foreach(var d in dinosaurs)if(d.active)
   {
    d.root.localPosition=inverse*(d.root.localPosition-origin);d.root.localRotation=inverse*d.root.localRotation;
    for(int i=0;i<d.previous.Length;i++)d.previous[i]=inverse*(d.previous[i]-origin);
   }
  }
  Vector3 DinosaurPoint(Transform point)=>transform.InverseTransformPoint(point.position);
  void CopyDinosaurTail(Dinosaur d){for(int i=0;i<6;i++)d.previous[i]=DinosaurPoint(d.visual.tail[i]);}
  void TickDinosaurs(float dt,bool schedule=true)
  {
   if(dt<=0)return;
   if(!dinosaursEnabled){dinosaurScroll=0;foreach(var d in dinosaurs)if(d.active){d.active=false;d.root.gameObject.SetActive(false);}return;}
   if(schedule&&State==RideState.Riding)
   {
    dinosaurTimer-=dt;
    // Keep the ten-second game clock through a turn; release on its first straight frame.
    if(dinosaurTimer<=0&&!IsTurning&&SpawnDinosaur()!=null)dinosaurTimer+=DinosaurInterval;
   }
   float scroll=dinosaurScroll;dinosaurScroll=0;int count=Mathf.CeilToInt(dt/(1f/120));float step=dt/count;
   foreach(var d in dinosaurs)if(d.active)
   {
    for(int n=0;n<count;n++)
    {
     d.root.localPosition+=(d.root.localRotation*Vector3.forward*DinosaurWalkSpeed*dt-Vector3.forward*scroll)/count;
     d.nextSweep-=step;
     Vector3 toward=rider.localPosition-d.root.localPosition;
     float closing=Mathf.Max(1,Speed+DinosaurWalkSpeed);
     // Predict the real tail crossing for the rider's lateral position. A late
     // right-edge crossing must load earlier than a left-edge crossing.
     float lane=Mathf.InverseLerp(-4.65f,4.65f,rider.localPosition.x);
     float meetAt=Mathf.Lerp(1.10f,1.28f,lane),reach=4.7f+Mathf.Sin(lane*Mathf.PI)*1.15f;
     if(!d.visual.Sweeping&&d.nextSweep<=0&&Vector3.Dot(toward,d.root.localRotation*Vector3.forward)>0&&toward.magnitude<closing*meetAt+reach)
     {
      d.hits.Clear();d.hitPolice=d.hitRider=false;d.visual.StartSweep();d.nextSweep=3.4f;
      notice="QUẤT ĐUÔI · NHẢY LÊN!";noticeTimer=1.1f;
     }
     d.visual.Advance(step);
     ResolveDinosaurHits(d,(float)(n+1)/count);
     CopyDinosaurTail(d);
    }
    Vector3 p=d.root.localPosition-rider.localPosition;
    if(p.z< -65||Mathf.Abs(p.x)>100||p.magnitude>240){d.active=false;d.root.gameObject.SetActive(false);}
   }
  }
  // Capsule against the actual oriented vehicle footprint, including vertical clearance.
  // Tail endpoints come from the same sampled bones that deform the rendered skin.
  static bool DinosaurSegmentHit(Vector3 a,Vector3 b,float radius,Vector3 p,Quaternion yaw,float width,float length,float height)
  {
   Quaternion inv=Quaternion.Inverse(yaw);a=inv*(a-p);b=inv*(b-p);
   int steps=Mathf.Max(1,Mathf.CeilToInt((b-a).magnitude/.22f));
   for(int i=0;i<=steps;i++)
   {
    Vector3 v=Vector3.Lerp(a,b,(float)i/steps);
    if(v.y+radius<0||v.y-radius>height)continue;
    float dx=Mathf.Max(0,Mathf.Abs(v.x)-width),dz=Mathf.Max(0,Mathf.Abs(v.z)-length);
    if(dx*dx+dz*dz<=radius*radius)return true;
   }
   return false;
  }
  bool DinosaurTouches(Dinosaur d,Vector3 p,Quaternion yaw,float width,float length,float height,out Vector3 hit,out bool tailHit)
  {
   hit=Vector3.zero;tailHit=false;
   if((p-d.root.localPosition).sqrMagnitude>400)return false;
   if(d.visual.TailDangerous)
   {
    for(int i=0;i<5;i++)
    {
     Vector3 a=DinosaurPoint(d.visual.tail[i]),b=DinosaurPoint(d.visual.tail[i+1]);
     float radius=Mathf.Lerp(.72f,.27f,i/4f);
     int sweep=Mathf.Max(1,Mathf.CeilToInt(Mathf.Max((a-d.previous[i]).magnitude,(b-d.previous[i+1]).magnitude)/.18f));
     for(int k=0;k<=sweep;k++)if(DinosaurSegmentHit(Vector3.Lerp(d.previous[i],a,(float)k/sweep),Vector3.Lerp(d.previous[i+1],b,(float)k/sweep),radius,p,yaw,width,length,height))
     {hit=(a+b)*.5f;tailHit=true;return true;}
    }
   }
   Vector3 pelvis=DinosaurPoint(d.visual.pelvis),head=DinosaurPoint(d.visual.head);
   // The trunk/skull are narrower than the full mesh (feet and curled tail).
   if(DinosaurSegmentHit(pelvis,head,.9f,p,yaw,width,length,height))
   {hit=pelvis;return true;}
   for(int i=0;i<2;i++)
   {Vector3 f=DinosaurPoint(i==0?d.visual.leftFoot:d.visual.rightFoot);if(DinosaurSegmentHit(f,f+Vector3.up*1.6f,.55f,p,yaw,width,length,height)){hit=f;return true;}}
   return false;
  }
  void ResolveDinosaurHits(Dinosaur d,float fraction)
  {
   foreach(var v in vehicles)
   {
    if(!v.active||v.wallCrashed||v.riverFalling||d.hits.Contains(v))continue;
    float height=Mathf.Max(v.halfWidth>.7f?1.7f:2.15f,JumpRoofHeight(v));
    Vector3 sampled=Vector3.Lerp(v.previousPosition,v.model.localPosition,fraction);
    if(!DinosaurTouches(d,sampled,Quaternion.Euler(0,Mathf.LerpAngle(v.previousYaw,v.model.localEulerAngles.y,fraction),0),BodyWidth(v),v.halfLength,height,out Vector3 at,out bool tail))continue;
    d.hits.Add(v);Vector3 away=v.model.localPosition-at;away.y=0;if(away.sqrMagnitude<.01f)away=Vector3.right;away.Normalize();
    Vector3 before=BodyVelocity(v),after=before+away*14+Vector3.up*5;
    // Deliberate exception to armored traffic immunity: the dinosaur also throws tanks.
    ActivateCollisionBody(v,before,after,new Vector2(away.x,away.z)*VehicleMass(v)*14,Vector2.zero,false,true,14);
    v.knockVelocity.y=Mathf.Max(v.knockVelocity.y,5);v.motionVelocity=v.knockVelocity;
    if(tail)DinosaurTailHits++;
   }
   Vector3 riderAt=Vector3.Lerp(riderBody.previousPosition,rider.localPosition,fraction);
   if(State==RideState.Riding&&!d.hitRider&&DinosaurTouches(d,riderAt,Quaternion.Euler(0,rider.localEulerAngles.y,0),PlayerHalfWidth,PlayerHalfLength,PlayerHeight,out Vector3 contact,out bool tailRider))
   {
    d.hitRider=true;Vector3 away=rider.localPosition-contact;away.y=0;if(away.sqrMagnitude<.01f)away=Vector3.right;away.Normalize();
    Vector3 before=riderRoadVelocity;rider.localPosition=riderAt;
    BeginRiderImpact(null,new Vector2(-away.x,-away.z),contact,before);
    riderFallVelocity=away*11+Vector3.up*6+before*.2f;
    riderFallSpin=new Vector3(-90,120,-Mathf.Sign(away.x)*200);HitByDinosaur=true;
    if(tailRider)DinosaurTailHits++;
    notice=CrashDuringPursuit?"BỊ KHỦNG LONG HẤT · CHỜ CSGT ĐẾN BẮT":"BỊ KHỦNG LONG HẤT · CHUYẾN ĐI KẾT THÚC";noticeTimer=4;
   }
   if(IsChased&&!d.hitPolice&&policeBike.gameObject.activeSelf&&DinosaurTouches(d,policeBike.localPosition,policeBike.localRotation,.36f,.95f,2.15f,out Vector3 policeHit,out bool tailPolice))
   {
    d.hitPolice=true;PoliceFallRemaining=1.2f;PoliceFalls++;PoliceSpeed=0;
    policeFallSide=policeBike.localPosition.x<policeHit.x?-1:1;policeBody.knocked=true;
    // Integrate a real shove/hop during the existing 1.2-second police recovery.
    var push=policeBike.localPosition-policeHit;push.y=0;if(push.sqrMagnitude<.01f)push=Vector3.right;
    dinosaurPoliceVelocity=push.normalized*7+Vector3.up*3;if(tailPolice)DinosaurTailHits++;
   }
  }
  void TickDinosaurPolicePush(float dt)
  {
   if(dinosaurPoliceVelocity.sqrMagnitude<.001f)return;
   dinosaurPoliceVelocity.y-=9.81f*dt;var p=policeBike.localPosition+dinosaurPoliceVelocity*dt;
   if(p.y<0){p.y=0;dinosaurPoliceVelocity.y=0;}
   p.x=Mathf.Clamp(p.x,-7,7);policeBike.localPosition=p;
   dinosaurPoliceVelocity.x=Mathf.MoveTowards(dinosaurPoliceVelocity.x,0,dt*8);
   dinosaurPoliceVelocity.z=Mathf.MoveTowards(dinosaurPoliceVelocity.z,0,dt*8);
  }
 }
}
