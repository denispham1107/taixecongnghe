using System;
using UnityEngine;
using UnityEngine.Rendering;
using static PhoViet.StreetArt;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  const float ChainRange=4f,ChainCastTime=.09f,ChainTensionTime=.10f,ChainRetractTime=.16f;
  const float ChainClearance=.30f,ChainCooldown=.65f;

  enum ChainPhase { Idle, Cast, Tension, Retract }
  [Serializable] sealed class ChainMeshData { public Vector3[] vertices; public int[] triangles; }
  [Serializable] sealed class ChainArtData { public ChainMeshData link,hook; public float[] cast,retract; }
  ChainPhase chainPhase;
  TrafficVehicle chainBike;
  int chainBikeGeneration;
  float chainAge,chainCastDuration;
  Transform chainRoot,chainHook;
  readonly Transform[] chainLinks=new Transform[25];
  Mesh chainLinkMesh,chainHookMesh;
  float[] chainCastCurve,chainRetractCurve;

  bool ChainCanCast=>State==RideState.Riding&&!ServiceStopped&&!energyHover;
  bool ChainBikeValid(TrafficVehicle v)=>v!=null&&v.active&&!v.knocked&&!v.wallCrashed&&v.halfWidth<=.7f&&
   !v.armored&&!v.riverFalling&&!v.lightningWreck;

  bool TryThrowChain()
  {
   if(!ChainCanCast||chainPhase!=ChainPhase.Idle||kickCooldown>0||chainRoot==null)return false;
   TrafficVehicle closest=null;
   float closestDistance=ChainRange*ChainRange;
   foreach(var bike in vehicles)
   {
    if(!ChainBikeValid(bike))continue;
    float distance=(bike.model.localPosition-rider.localPosition).sqrMagnitude;
    if(distance>closestDistance||(closest!=null&&distance==closestDistance))continue;
    closest=bike;
    closestDistance=distance;
   }
   if(closest==null)return false;
   chainBike=closest;
   chainBikeGeneration=closest.spawnGeneration;
   chainPhase=ChainPhase.Cast;
   chainAge=0;
   chainCastDuration=ChainTimeToLatch(closest);
   kickCooldown=ChainCooldown;
   // A bike closing in fast can enter the collision envelope within one frame.
   if(chainCastDuration<=.05f)LatchChain();
   return true;
  }

  float ChainTimeToLatch(TrafficVehicle bike)
  {
   Vector2 offset=Flat(bike.model.localPosition-rider.localPosition);
   Vector2 relativeVelocity=Flat(BodyVelocity(bike)-riderRoadVelocity);
   float earliestSafeLatch=0;
   for(int i=0;i<2;i++)
   {
    Vector2 axis=i==0?BodyRight(riderBody):BodyForward(riderBody);
    float separation=Vector2.Dot(offset,axis);
    float gap=Mathf.Abs(separation)-ProjectedRadius(riderBody,axis)-ProjectedRadius(bike,axis);
    if(gap<=0)continue;
    float closing=-Vector2.Dot(relativeVelocity,axis)*Mathf.Sign(separation);
    if(closing<=0)return ChainCastTime;
    earliestSafeLatch=Mathf.Max(earliestSafeLatch,gap/closing);
   }
   // Let an existing overlap reach the normal collision path.
   if(earliestSafeLatch==0)return ChainCastTime;
   return Mathf.Min(ChainCastTime,earliestSafeLatch*.75f);
  }

  void LatchChain()
  {
   PullChainBike(chainBike);
   chainPhase=ChainPhase.Tension;
  }

  void CancelChain()
  {
   if(chainRoot!=null&&chainRoot.gameObject.activeSelf)chainRoot.gameObject.SetActive(false);
   chainPhase=ChainPhase.Idle;
   chainBike=null;
  }

  void TickChain(float dt)
  {
   if(chainPhase==ChainPhase.Idle)return;
   if(!ChainCanCast||!chainBike.active||chainBike.spawnGeneration!=chainBikeGeneration||
      chainBike.riverFalling||(chainPhase==ChainPhase.Cast&&!ChainBikeValid(chainBike)))
   {
    CancelChain();
    return;
   }

   chainAge+=dt;
   if(chainPhase==ChainPhase.Cast&&chainAge>=chainCastDuration)
   {
    LatchChain();
    chainAge-=chainCastDuration;
   }
   if(chainPhase==ChainPhase.Tension&&chainAge>=ChainTensionTime)
   {
    chainPhase=ChainPhase.Retract;
    chainAge-=ChainTensionTime;
   }
   if(chainPhase==ChainPhase.Retract&&chainAge>=ChainRetractTime)CancelChain();
  }

  void PullChainBike(TrafficVehicle bike)
  {
   Vector2 right=BodyRight(riderBody),forward=BodyForward(riderBody);
   Vector2 bikeRight=BodyRight(bike);
   Vector2 offset=Flat(bike.model.localPosition-rider.localPosition);
   // The circumscribed radius covers the whole bike as it widens and yaws while falling.
   float fallenRadius=new Vector2(1.05f,bike.halfLength).magnitude;
   float sideGap=Mathf.Abs(Vector2.Dot(offset,right))-PlayerHalfWidth-fallenRadius;
   float frontGap=Mathf.Abs(Vector2.Dot(offset,forward))-PlayerHalfLength-fallenRadius;
   Vector2 axis=sideGap>=frontGap?right:forward;
   Vector2 away=axis*Mathf.Sign(Vector2.Dot(offset,axis));
   float pull=Mathf.Clamp(Mathf.Max(sideGap,frontGap)-ChainClearance,0,.46f);
   Vector2 inward=-offset.normalized*pull;

   Vector3 velocity=BodyVelocity(bike);
   Vector2 fallSide=bikeRight*Mathf.Sign(Vector2.Dot(bikeRight,offset));
   Vector2 shove=fallSide*4.5f;
   // Keep the separating axis opening as the bike falls, including head-on and crossing traffic.
   float opening=Vector2.Dot(Flat(velocity-riderRoadVelocity)+shove,away);
   shove+=away*Mathf.Max(0,5.5f-opening);
   KnockVehicle(bike,Mathf.Sign(shove.x),false);
   bike.model.localPosition+=new Vector3(inward.x,0,inward.y);
   bike.knockVelocity=velocity+new Vector3(shove.x,.65f,shove.y);
   // Rotate the fallen pose away from the car in the bike's own frame.
   bike.knockDirection=Mathf.Sign(Vector2.Dot(bikeRight,shove));
   bike.playerGrace=0;
   KickCount++;
   notice="MÓC XÍCH · XE MÁY ĐÃ NGÃ";
   noticeTimer=1.25f;
  }

  void EnsureChainArt()
  {
   if(chainRoot!=null)return;
   var source=Resources.Load<TextAsset>("Chain/PV_Chain");
   if(source==null){Debug.LogError("[PhoViet] Missing Blender chain meshes.");return;}
   var art=JsonUtility.FromJson<ChainArtData>(source.text);
   chainCastCurve=art.cast;chainRetractCurve=art.retract;
   chainLinkMesh=MakeChainMesh("PV Chain link",art.link);
   chainHookMesh=MakeChainMesh("PV Forged hook",art.hook);
   chainRoot=Group("Player chain effect",rider.parent);
   Material steel=Mat(Hex("94C7D9")),hook=Mat(Hex("FFBA5B"));
   for(int i=0;i<chainLinks.Length;i++)chainLinks[i]=MakeChainPart("Link",chainLinkMesh,steel);
   chainHook=MakeChainPart("Hook",chainHookMesh,hook);
   chainRoot.gameObject.SetActive(false);
  }

  static Mesh MakeChainMesh(string name,ChainMeshData data)
  {
   var mesh=new Mesh{name=name,vertices=data.vertices,triangles=data.triangles};
   mesh.RecalculateNormals();
   mesh.RecalculateBounds();
   return mesh;
  }

  Transform MakeChainPart(string name,Mesh mesh,Material material)
  {
   Transform part=Group(name,chainRoot);
   part.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
   var renderer=part.gameObject.AddComponent<MeshRenderer>();
   renderer.sharedMaterial=material;
   renderer.shadowCastingMode=ShadowCastingMode.Off;
   return part;
  }

  void RenderChain()
  {
   if(chainPhase==ChainPhase.Idle)return;
   if((State!=RideState.Paused&&!ChainCanCast)||!chainBike.active||chainBike.spawnGeneration!=chainBikeGeneration)
   {
    CancelChain();
    return;
   }

   Vector3 target=chainBike.model.localPosition+chainBike.model.localRotation*new Vector3(0,.67f,.13f);
   Vector3 side=Quaternion.Inverse(rider.localRotation)*(target-rider.localPosition);
   Vector3 origin=rider.localPosition+rider.localRotation*new Vector3(
    Mathf.Clamp(side.x,-PlayerHalfWidth,PlayerHalfWidth),PlayerHeight*.58f,Mathf.Clamp(side.z,-PlayerHalfLength,PlayerHalfLength));
   float extension=chainPhase==ChainPhase.Cast?SampleChainCurve(chainCastCurve,chainAge/chainCastDuration):
    chainPhase==ChainPhase.Retract?SampleChainCurve(chainRetractCurve,chainAge/ChainRetractTime):1;
   Vector3 span=(target-origin)*extension;
   float length=span.magnitude;
   if(length<.05f){chainRoot.gameObject.SetActive(false);return;}
   float sag=Mathf.Min(.19f,length*.08f)*(chainPhase==ChainPhase.Tension?.3f:1);
   int count=Mathf.Clamp(Mathf.CeilToInt(length/.20f),1,chainLinks.Length);
   for(int i=0;i<chainLinks.Length;i++)
   {
    Transform link=chainLinks[i];
    if(link.gameObject.activeSelf!=(i<count))link.gameObject.SetActive(i<count);
    if(i>=count)continue;
    float t=(i+.5f)/count;
    Vector3 tangent=span-Vector3.up*(sag*Mathf.PI*Mathf.Cos(t*Mathf.PI));
    link.localPosition=origin+span*t-Vector3.up*(sag*Mathf.Sin(t*Mathf.PI));
    link.localScale=new Vector3(1,1,Mathf.Max(1,length/(count*.20f)));
    link.localRotation=Quaternion.LookRotation(tangent)*Quaternion.AngleAxis(i%2==0?0:90,Vector3.forward);
   }
   chainHook.localPosition=origin+span;
   chainHook.localRotation=Quaternion.LookRotation(span+Vector3.up*(sag*Mathf.PI));
   chainRoot.gameObject.SetActive(true);
  }

  static float SampleChainCurve(float[] curve,float progress)
  {
   float sample=Mathf.Clamp01(progress)*(curve.Length-1);
   int index=Mathf.Min((int)sample,curve.Length-2);
   return Mathf.Lerp(curve[index],curve[index+1],sample-index);
  }

  void ShutdownChainArt()
  {
   if(chainLinkMesh!=null)Destroy(chainLinkMesh);
   if(chainHookMesh!=null)Destroy(chainHookMesh);
   if(chainRoot!=null)Destroy(chainRoot.gameObject);
  }
 }
}
