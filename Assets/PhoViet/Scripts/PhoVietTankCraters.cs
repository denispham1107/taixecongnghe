using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const float TankCraterRadius=BlastRadius*.92f;
  public const int TankCraterCapacity=64;
  public Material tankCraterMaterial;
  public int CratersCreated {get;private set;}
  public int CraterWheelHits {get;private set;}
  public int ActiveCraterCount=>tankCraters.Count;
  public bool CraterSteeringActive=>Mathf.Abs(craterHeading)>0.01f;
  public float CraterDriftOffset=>craterOffset;
  public float CraterDriftSpeed=>craterSideSpeed;
  public float RiderCraterDip=>craterDip;
  public float RiderCraterPitch=>craterPitch;
  public float RiderCraterRoll=>craterRoll;
  public sealed class TankCrater
  {
   public Transform model;public Renderer renderer;public RiverBlock owner;public uint revision;
   public Vector3 previousCentre;public float age,seed;public bool touching;
   public readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
  }
  public readonly List<TankCrater> tankCraters=new List<TankCrater>(TankCraterCapacity);
  readonly Stack<TankCrater> craterPool=new Stack<TankCrater>(TankCraterCapacity);
  readonly Vector4[] craterMasks=new Vector4[TankCraterCapacity];
  readonly float[] craterMaskYaw=new float[TankCraterCapacity];
  readonly Vector3[] craterWheels=new Vector3[4];
  Mesh[] craterMeshes;Mesh craterDebrisMesh;ParticleSystem craterDebris;Material craterDebrisMaterial;
  Transform craterPoolRoot;bool craterWheelsReady;
  float craterHeading,craterOffset,craterSideSpeed,craterSpeedDamping,craterRecoverySpeed,craterSideAcceleration;
  float craterDip,craterPitch,craterRoll;bool craterRecovering;
  Vector3 craterAppliedOffset;
  static readonly int CraterCountId=Shader.PropertyToID("_PVCraterCount"),CraterDataId=Shader.PropertyToID("_PVCraters"),CraterMatrixId=Shader.PropertyToID("_PVCraterWorldToGame");
  void EnsureTankCraters()
  {
   if(craterPoolRoot!=null)return;
   craterPoolRoot=new GameObject("Hố đạn xe tăng • pooled terrain").transform;craterPoolRoot.SetParent(transform,false);
   craterMeshes=new[]{TankCraterGeometry.Build(0),TankCraterGeometry.Build(1),TankCraterGeometry.Build(2)};
   craterDebrisMesh=TankCraterGeometry.Debris();
   EnsureTankFx();
   var go=new GameObject("Mảnh nhựa đường văng");go.transform.SetParent(tankFxAnchor,false);
   craterDebris=go.AddComponent<ParticleSystem>();craterDebris.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=craterDebris.main;main.loop=false;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.maxParticles=384;main.gravityModifier=1.3f;
   var emission=craterDebris.emission;emission.enabled=false;var shape=craterDebris.shape;shape.enabled=false;
   var rotation=craterDebris.rotationOverLifetime;rotation.enabled=true;rotation.separateAxes=true;rotation.x=3;rotation.y=5;rotation.z=2;
   craterDebrisMaterial=new Material(tankCraterMaterial){name="PV_AsphaltDebris_Runtime",color=new Color(.245f,.23f,.21f)};craterDebrisMaterial.SetFloat("_UseVertexColor",0);
   var r=go.GetComponent<ParticleSystemRenderer>();r.renderMode=ParticleSystemRenderMode.Mesh;r.mesh=craterDebrisMesh;r.sharedMaterial=craterDebrisMaterial;r.shadowCastingMode=ShadowCastingMode.Off;
   craterDebris.Play();
   var car=rider.GetComponent<PlayerCarVisual>();
   for(int i=0;i<4;i++)craterWheels[i]=car!=null&&car.wheels.Length>i?rider.InverseTransformPoint(car.wheels[i].position):new Vector3(i%2==0?-.6f:.6f,0,i<2?1:-1);
   craterWheelsReady=true;
   for(int i=0;i<8;i++){var c=NewTankCrater();c.model.gameObject.SetActive(false);craterPool.Push(c);}
  }
  public TankCrater CreateTankCrater(Vector3 at)
  {
   if(tankCraterMaterial==null||Mathf.Abs(at.y)>.1f||riversEnabled&&OverRiver(at))return null;
   RiverBlock owner=null;Vector3 local=Vector3.zero;
   foreach(var tile in blocks)
   {
    var p=tile.InverseTransformPoint(transform.TransformPoint(at));
    if(Mathf.Abs(p.x)<=7.05f&&p.z>=0&&p.z<=VietnamStreet.Length){owner=tile.GetComponent<RiverBlock>();local=p;break;}
   }
   if(owner==null)return null;
   EnsureTankCraters();
   // Repeated hits in the same depression reuse it, while fresh debris still flies.
   foreach(var existing in tankCraters)if(Vector3.Distance(existing.model.position,transform.TransformPoint(at))<TankCraterRadius*.65f){ThrowCraterDebris(at);return existing;}
   if(tankCraters.Count>=TankCraterCapacity)
   {
    TankCrater far=null;float distance=20;
    foreach(var c in tankCraters){float z=-transform.InverseTransformPoint(c.model.position).z;if(z>distance){distance=z;far=c;}}
    if(far==null){ThrowCraterDebris(at);return null;}ReleaseTankCrater(far);
   }
   var crater=craterPool.Count>0?craterPool.Pop():NewTankCrater();int variant=CratersCreated%3;crater.seed=variant*1.73f+.41f;
   crater.model.GetComponent<MeshFilter>().sharedMesh=craterMeshes[variant];crater.owner=owner;crater.revision=owner.LayoutRevision;crater.age=0;crater.touching=false;
   crater.model.SetParent(owner.transform,false);local.y=0;crater.model.localPosition=local;crater.model.localRotation=Quaternion.identity;crater.model.localScale=new Vector3(.001f,.001f,.001f);crater.model.gameObject.SetActive(true);
   crater.previousCentre=transform.InverseTransformPoint(crater.model.position);tankCraters.Add(crater);CratersCreated++;ThrowCraterDebris(at);SyncTankCraterMasks();return crater;
  }
  TankCrater NewTankCrater()
  {
   var go=new GameObject("Hố đạn • lòng lõm, viền vỡ và vết nứt");go.transform.SetParent(craterPoolRoot,false);go.AddComponent<MeshFilter>();
   var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=tankCraterMaterial;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
   return new TankCrater{model=go.transform,renderer=r};
  }
  void ThrowCraterDebris(Vector3 at)
  {
   for(int i=0;i<18;i++)
   {
    float a=i*Mathf.PI*2/18;var v=new Vector3(Mathf.Cos(a)*(3+i%3),4+i%4,Mathf.Sin(a)*(3+i%3));
    Emit(craterDebris,at+Vector3.up*.07f,v,.22f+(i%4)*.07f,.7f+(i%3)*.18f,Color.white);
   }
  }
  void ReleaseTankCrater(TankCrater c)
  {tankCraters.Remove(c);c.model.gameObject.SetActive(false);c.model.SetParent(craterPoolRoot,false);c.owner=null;c.touching=false;craterPool.Push(c);}
  void ResetTankCraters()
  {
   for(int i=tankCraters.Count-1;i>=0;i--)ReleaseTankCrater(tankCraters[i]);
   if(craterDebris!=null)craterDebris.Clear();
   CratersCreated=CraterWheelHits=0;craterHeading=craterOffset=craterSideSpeed=craterSpeedDamping=craterRecoverySpeed=craterSideAcceleration=0;
   craterDip=craterPitch=craterRoll=0;craterAppliedOffset=Vector3.zero;craterRecovering=false;Shader.SetGlobalInt(CraterCountId,0);
  }
  void UpdateTankCraterTerrain(float dt)
  {
   if(State==RideState.Paused)dt=0;
   for(int i=tankCraters.Count-1;i>=0;i--)
   {
    var c=tankCraters[i];var p=transform.InverseTransformPoint(c.model.position);
    if(c.owner==null||c.owner.LayoutRevision!=c.revision||p.z< -90||p.sqrMagnitude>220*220){ReleaseTankCrater(c);continue;}
    c.age+=dt;float open=Mathf.SmoothStep(0,1,Mathf.Clamp01(c.age/.18f));c.model.localScale=new Vector3(TankCraterRadius*open,open,TankCraterRadius*open);
    if(c.owner.IsBridge){c.block.SetMatrix("_PVCraterOwnerInverse",c.owner.transform.worldToLocalMatrix);c.block.SetVector("_PVCraterClipBand",new Vector4(RiverBlock.Near,RiverBlock.Far,RiverBlock.DeckHalf,1));}
    else c.block.SetVector("_PVCraterClipBand",Vector4.zero);
    c.renderer.SetPropertyBlock(c.block);
   }
   SyncTankCraterMasks();
  }
  void SyncTankCraterMasks(bool rebase=false)
  {
   if(tankCraters.Count==0){Shader.SetGlobalInt(CraterCountId,0);return;}
   int count=0;
   foreach(var c in tankCraters)
   {
    var p=transform.InverseTransformPoint(c.model.position);if(rebase)c.previousCentre=p;
    if(!c.model.gameObject.activeInHierarchy)continue;
    var axis=transform.InverseTransformDirection(c.model.right);craterMaskYaw[count]=Mathf.Atan2(axis.z,axis.x);
    craterMasks[count++]=new Vector4(p.x,p.z,c.model.localScale.x,c.seed);
   }
   Shader.SetGlobalMatrix(CraterMatrixId,transform.worldToLocalMatrix);Shader.SetGlobalVectorArray(CraterDataId,craterMasks);Shader.SetGlobalFloatArray("_PVCraterYaw",craterMaskYaw);Shader.SetGlobalInt(CraterCountId,count);
  }
  void AdvanceCraterSteering(float dt)
  {
   if(ServiceStopped||dt<=0)return;float before=craterSideSpeed;
   if(craterRecovering)
   {
    float old=craterOffset;craterOffset=Mathf.SmoothDamp(craterOffset,0,ref craterRecoverySpeed,.34f,Mathf.Infinity,dt);craterSideSpeed=(craterOffset-old)/dt;
    if(Mathf.Abs(craterOffset)<.001f&&Mathf.Abs(craterSideSpeed)<.01f){craterOffset=craterSideSpeed=0;craterRecovering=false;}
   }
   else
   {
    float target=Mathf.Clamp(Mathf.Tan(craterHeading*Mathf.Deg2Rad)*Speed,-2.6f,2.6f);
    craterSideSpeed=Mathf.SmoothDamp(craterSideSpeed,target,ref craterSpeedDamping,.10f,Mathf.Infinity,dt);craterOffset+=craterSideSpeed*dt;
   }
   craterSideAcceleration=(craterSideSpeed-before)/dt;
  }
  bool CorrectCraterSteering(int direction)
  {
   if(!CraterSteeringActive||direction==0||Mathf.Sign(direction)==Mathf.Sign(craterHeading))return false;
   craterHeading=0;craterRecovering=true;craterRecoverySpeed=craterSideSpeed;craterSpeedDamping=0;
   notice="ĐÃ CHỈNH LẠI HƯỚNG XE";noticeTimer=1;return true;
  }
  void ApplyCraterOffset()
  {craterAppliedOffset=Quaternion.Euler(0,IsTurning?TurnYaw:0,0)*Vector3.right*craterOffset;rider.localPosition+=craterAppliedOffset;}
  static float CraterSegmentDistance(Vector2 from,Vector2 to,out Vector2 nearest)
  {var d=to-from;float u=d.sqrMagnitude>.000001f?Mathf.Clamp01(-Vector2.Dot(from,d)/d.sqrMagnitude):0;nearest=from+d*u;return nearest.magnitude;}
  void TickCraterWheelContacts(float dt,Vector3 previous,Quaternion rotation)
  {
   if(!craterWheelsReady)return;float left=0,right=0,front=0,rear=0;bool grounded=jumpHeight<.18f&&!RiderAirborne;
   var oldYaw=Quaternion.Euler(0,rotation.eulerAngles.y,0);var yaw=Quaternion.Euler(0,rider.localEulerAngles.y,0);
   foreach(var c in tankCraters)
   {
    var centre=transform.InverseTransformPoint(c.model.position);bool hit=false;float severity=0;
    if(grounded&&c.model.gameObject.activeInHierarchy)for(int i=0;i<4;i++)
    {
     var wheel=craterWheels[i];wheel.y=0;var a=previous+oldYaw*wheel-c.previousCentre;var b=rider.localPosition+yaw*wheel-centre;
     float distance=CraterSegmentDistance(new Vector2(a.x,a.z),new Vector2(b.x,b.z),out var nearest);
     var axis=transform.InverseTransformDirection(c.model.right);
     float radius=TankCraterRadius*.96f*TankCraterGeometry.Outline(Mathf.Atan2(nearest.y,nearest.x)-Mathf.Atan2(axis.z,axis.x),c.seed);
     if(distance<radius){hit=true;severity=Mathf.Max(severity,1-distance/radius);}
     float depth=Mathf.Clamp01(1-new Vector2(b.x,b.z).magnitude/TankCraterRadius)*.22f;
     if(i%2==0)left=Mathf.Max(left,depth);else right=Mathf.Max(right,depth);if(i<2)front=Mathf.Max(front,depth);else rear=Mathf.Max(rear,depth);
    }
    if(hit&&!c.touching)
    {
     float side=Vector3.Dot(centre-rider.localPosition,yaw*Vector3.right);if(Mathf.Abs(side)<.12f)side=Mathf.Sin(c.seed)>0?1:-1;
     craterHeading=Mathf.Sign(side)*(3.4f+severity*.6f);craterRecovering=false;craterSpeedDamping=0;CraterWheelHits++;CancelAssistedKick(false);
     notice="TRÚNG HỐ ĐẠN · LÁI "+(side>0?"TRÁI":"PHẢI")+" ĐỂ GIỮ HƯỚNG";noticeTimer=1.8f;
    }
    c.touching=hit;c.previousCentre=centre;
   }
   float blend=1-Mathf.Exp(-Mathf.Max(0,dt)*18);
   craterDip=Mathf.Lerp(craterDip,grounded?(left+right)*.27f:0,blend);
   craterPitch=Mathf.Lerp(craterPitch,grounded?(front-rear)*12:0,blend);craterRoll=Mathf.Lerp(craterRoll,grounded?(left-right)*16:0,blend);
  }
  void ShutdownTankCraters()
  {
   Shader.SetGlobalInt(CraterCountId,0);
   if(craterMeshes!=null)foreach(var mesh in craterMeshes)if(mesh!=null)Destroy(mesh);
   if(craterDebrisMesh!=null)Destroy(craterDebrisMesh);
   if(craterDebrisMaterial!=null)Destroy(craterDebrisMaterial);
  }
 }
}
