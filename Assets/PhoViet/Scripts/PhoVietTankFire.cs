using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 // Xe tăng bắn đạn (25/10/2026, huongdan.md 21.38): cứ 9 giây một xe tăng chạy ngược chiều xuất hiện phía trước
 // (bỏ xe tăng ngẫu nhiên ở ô xe con); khi còn ở phía trước trong tầm, mỗi giây bắn một quả đạn rơi vào 1 trong 4 làn
 // ở đoạn đường người chơi sắp chạy tới. Vòng cảnh báo đỏ hiện dưới đất ngay khi bắn; đạn trúng hoặc nổ trong bán kính
 // làm người chơi văng ra và kết thúc chuyến, xe khác trong bán kính cũng văng. Hiệu ứng dựng/render trong Blender MCP
 // (pho_viet_tank_fx.py): đạn PV_TankShell, lửa đầu nòng, vụ nổ, vòng cảnh báo.
 // 21.99: thêm xe tăng cùng lúc với khủng long sau cua, và một xe ngược chiều cho mỗi mốc vượt400m.
 public partial class PhoVietGame
 {
  public bool randomTanks=false;          // xe tăng ngẫu nhiên ở ô xe con (21.33) — tắt từ 21.38
  public bool tankBarrageEnabled=true;    // xe tăng theo lịch 9 s và bắn đạn
  public const float TankInterval=9f,TankDistanceInterval=400f,TankFireInterval=1f,TankFireIntervalMin=.4f,ShellFlight=1.6f,BlastRadius=2.2f,TankFireNear=24f,TankFireFar=115f;
  public GameObject tankShellPrefab;
  public Material tankShellMaterial,tankMuzzleMaterial,tankExplosionMaterial,tankWarningMaterial,tankSparkMaterial;
  public int TanksScheduled {get;private set;}
  public int DistanceTanksScheduled {get;private set;}
  public int ShellsFired {get;private set;}
  public int ShellBlasts {get;private set;}
  public int ShellSplashes {get;private set;}
  public int VehiclesBlasted {get;private set;}
  public bool KilledByShell {get;private set;}
  public float TankSpawnCountdown=>tankSpawnTimer;
  float tankSpawnTimer=TankInterval;
  float nextDistanceTank=TankDistanceInterval;
  int pendingDistanceTanks;
  public sealed class TankShell {public Vector3 start,target;public float age,flight,apex;public int lane;public Transform body;public Renderer marker;public TrafficVehicle tank;public bool overWater;}
  public readonly List<TankShell> shells=new List<TankShell>();
  readonly Stack<Transform> shellPool=new Stack<Transform>();readonly Stack<Renderer> markerPool=new Stack<Renderer>();
  Transform tankFxAnchor;ParticleSystem muzzleFx,blastFx,smokeFx,sparkFx,trailFx;Mesh markerMesh;MaterialPropertyBlock markerBlock;
  AudioClip cannonClip,blastClip;
  static readonly int ProgressId=Shader.PropertyToID("_Progress");

  void ResetTankBarrage()
  {
   ResetTankCraters();
   ClearShells();tankSpawnTimer=TankInterval;nextDistanceTank=TankDistanceInterval;pendingDistanceTanks=0;DistanceTanksScheduled=0;TanksScheduled=0;ShellsFired=0;ShellBlasts=0;ShellSplashes=0;VehiclesBlasted=0;KilledByShell=false;
   if(tankFxAnchor!=null){tankFxAnchor.localPosition=Vector3.zero;tankFxAnchor.localRotation=Quaternion.identity;foreach(var ps in new[]{muzzleFx,blastFx,smokeFx,sparkFx,trailFx})if(ps!=null)ps.Clear();}
  }
  // Đạn/cảnh báo đang bay bị hủy khi cua (thế giới quay 90°) hoặc chơi lại.
  void ClearShells()
  {
   foreach(var s in shells){ReleaseShell(s);}
   shells.Clear();
  }
  void ReleaseShell(TankShell s)
  {
   if(s.body!=null){s.body.gameObject.SetActive(false);shellPool.Push(s.body);s.body=null;}
   if(s.marker!=null){s.marker.gameObject.SetActive(false);markerPool.Push(s.marker);s.marker=null;}
  }
  // Phố cuộn: đạn, cảnh báo và hạt hiệu ứng trôi cùng mặt đường.
  void ScrollTankFire(float delta)
  {
   if(tankFxAnchor!=null)tankFxAnchor.localPosition-=Vector3.forward*delta;
   foreach(var s in shells){s.start-=Vector3.forward*delta;s.target-=Vector3.forward*delta;}
  }

  // ------------------------------------------------------------ lịch xe tăng 9 giây
  void TickTankSchedule(float dt)
  {
   if(!tankBarrageEnabled||IsTurning||ServiceStopped||dt<=0)return;
   tankSpawnTimer-=dt;
   if(tankSpawnTimer>0)return;
   tankSpawnTimer=SpawnScheduledTank()!=null?TankInterval:.25f;
  }
  // Each crossed distance milestone adds a separate tank; it never consumes the nine-second schedule.
  void TickDistanceTankSchedule(float dt)
  {
   if(!tankBarrageEnabled||State!=RideState.Riding||ServiceStopped||dt<=0)return;
   while(Distance>nextDistanceTank){pendingDistanceTanks++;nextDistanceTank+=TankDistanceInterval;}
   if(IsTurning||pendingDistanceTanks==0)return;
   // Retain queued milestones if both lanes or every traffic slot are occupied.
   if(SpawnScheduledTank()!=null){pendingDistanceTanks--;DistanceTanksScheduled++;}
  }
  // Người chơi càng nhanh thì xe tăng bắn càng dày (25/10/2026, 21.39): 1,0 s ở tốc độ khởi đầu 17 m/s,
  // giảm đều tới 0,4 s ở 30 m/s (tốc độ chạy tối đa), tăng tốc/nhảy nhanh hơn nữa vẫn giữ 0,4 s.
  public static float TankFireIntervalAt(float speed)=>Mathf.Max(TankFireIntervalMin,Mathf.Lerp(TankFireInterval,TankFireIntervalMin,Mathf.InverseLerp(17,30,speed)));
  public float CurrentTankFireInterval=>TankFireIntervalAt(Speed);
  public TrafficVehicle SpawnScheduledTank()
  {return SpawnTankAt(null);}
  TrafficVehicle SpawnTankAt(float? junctionZ)
  {
   int first=rng.Next(2);
   for(int attempt=0;attempt<2;attempt++)
   {
    int lane=(first+attempt)%2;float z=junctionZ??(122+(float)rng.NextDouble()*8);
    bool clear=true;
    foreach(var o in vehicles)if(o.active&&Mathf.Abs(o.model.localPosition.x-LaneX[lane])<2.6f&&Mathf.Abs(o.model.localPosition.z-z)<14){clear=false;break;}
    if(!clear)continue;
    for(int i=0;i<vehicles.Count;i++)
    {
     var v=vehicles[i];if(v.active||v.eventOnly||RearExitReserved(v))continue;
     var meshy=v.model.GetComponent<MeshyCarVariants>();if(meshy==null)continue;
     int tank=meshy.ArmoredIndex;if(tank<0)continue;
     var size=meshy.variants[tank].size;
     if(junctionZ.HasValue&&!TurnTankSpace(lane,z,size.x*.5f,size.z*.5f))continue;
     meshy.PrepareVariant(tank,v);
     var spawned=SpawnTraffic(lane,z,i);
     if(spawned==null||!spawned.armored)continue;
     spawned.tankFireTimer=.4f;TanksScheduled++;
     notice="XE TĂNG PHÍA TRƯỚC · NÉ ĐẠN PHÁO";noticeTimer=1.6f;
     return spawned;
    }
   }
   return null;
  }

  // ------------------------------------------------------------ bắn
  bool TankCanFire(TrafficVehicle v)
  {
   if(!v.active||!v.armored||v.knocked||v.riverFalling||v.routeDetached||v.crossing||v.roadTurning||v.baseTrafficSpeed>=0)return false;
   float z=v.model.localPosition.z;if(z<=TankFireNear||z>=TankFireFar)return false;
   // Đủ xa để đạn rơi đúng chỗ người chơi sẽ tới (không bắn khi xe tăng đã quá gần).
   return TankMuzzle(v).z-5>=Mathf.Max(Speed,8)*ShellFlight;
  }
  void TickTankFire(float dt)
  {
   if(!tankBarrageEnabled||State!=RideState.Riding||IsTurning||dt<=0)return;
   foreach(var v in vehicles)
   {
    if(!TankCanFire(v))continue;
    // Khoảng bắn rút ngắn ngay khi người chơi tăng tốc (không chờ hết khoảng cũ dài hơn).
    v.tankFireTimer=Mathf.Min(v.tankFireTimer,CurrentTankFireInterval)-dt;
    if(v.tankFireTimer>0)continue;
    v.tankFireTimer=CurrentTankFireInterval;FireTankShell(v,rng.Next(4));
   }
  }
  public Vector3 TankMuzzle(TrafficVehicle v)=>transform.InverseTransformPoint(v.model.TransformPoint(new Vector3(0,1.42f,3.62f)));
  // Đạn rơi đúng chỗ người chơi sẽ tới sau thời gian bay, ở làn được chọn.
  public TankShell FireTankShell(TrafficVehicle v,int lane)
  {
   if(v==null)return null;
   EnsureTankFx();
   var muzzle=TankMuzzle(v);
   float ahead=Mathf.Max(Speed,8)*ShellFlight;
   var target=new Vector3(LaneX[Mathf.Clamp(lane,0,3)],0,rider.localPosition.z+ahead);
   if(target.z>muzzle.z-5)return null;
   bool water=riversEnabled&&OverRiver(target);if(water)target.y=RiverBlock.WaterY;
   var s=new TankShell{start=muzzle,target=target,flight=ShellFlight,lane=lane,tank=v,overWater=water,apex=3.2f+Vector3.Distance(muzzle,target)*.06f};
   s.body=shellPool.Count>0?shellPool.Pop():NewShellBody();s.body.gameObject.SetActive(true);s.body.localPosition=muzzle;
   s.marker=markerPool.Count>0?markerPool.Pop():NewMarker();s.marker.gameObject.SetActive(true);
   PlaceMarker(s,0);
   shells.Add(s);ShellsFired++;
   // Lửa đầu nòng và khói phụt theo hướng nòng.
   var forward=(transform.InverseTransformDirection(v.model.forward)).normalized;
   Emit(muzzleFx,muzzle,forward*2.5f,3.4f,.22f,Color.white);
   for(int i=0;i<5;i++)Emit(smokeFx,muzzle+forward*(.6f+i*.4f),forward*Random.Range(1.5f,3f)+Vector3.up*Random.Range(.4f,1.2f),Random.Range(1.4f,2.4f),Random.Range(.9f,1.5f),new Color(.75f,.72f,.68f,.85f));
   if(sound&&fx!=null)fx.PlayOneShot(cannonClip,Mathf.Lerp(.55f,.18f,Mathf.InverseLerp(20,115,muzzle.z)));
   return s;
  }
  Vector3 ShellPosition(TankShell s,float t)
  {
   float u=Mathf.Clamp01(t/s.flight);
   return Vector3.Lerp(s.start,s.target,u)+Vector3.up*(4*s.apex*u*(1-u));
  }
  void PlaceMarker(TankShell s,float progress)
  {
   var t=s.marker.transform;t.localPosition=new Vector3(s.target.x,s.target.y+.035f,s.target.z);
   float size=BlastRadius*2*Mathf.Lerp(1.25f,1f,progress);t.localScale=new Vector3(size,1,size);
   if(markerBlock==null)markerBlock=new MaterialPropertyBlock();
   s.marker.GetPropertyBlock(markerBlock);markerBlock.SetFloat(ProgressId,progress);s.marker.SetPropertyBlock(markerBlock);
  }
  void TickShells(float dt)
  {
   if(dt<=0||shells.Count==0)return;
   for(int i=shells.Count-1;i>=0;i--)
   {
    var s=shells[i];s.age+=dt;
    var p=ShellPosition(s,s.age);var next=ShellPosition(s,s.age+.02f);
    s.body.localPosition=p;if((next-p).sqrMagnitude>1e-6f)s.body.localRotation=Quaternion.LookRotation(next-p);
    PlaceMarker(s,Mathf.Clamp01(s.age/s.flight));
    // Vệt lửa sáng sát đuôi đạn + vệt khói xám nhạt kéo dài.
    Emit(trailFx,p,Vector3.zero,Random.Range(.7f,1f),.12f,new Color(1,.78f,.35f,1));
    Emit(trailFx,p-(next-p).normalized*.4f,Vector3.up*.25f,Random.Range(.5f,.8f),Random.Range(.4f,.7f),new Color(.85f,.83f,.8f,.7f));
    // Trúng trực tiếp giữa đường bay (chỉ khi người chơi còn đang chạy).
    bool direct=State==RideState.Riding&&Vector3.Distance(p,rider.localPosition+Vector3.up*(.9f+jumpHeight))<1f;
    if(s.age>=s.flight||direct)
    {
     shells.RemoveAt(i);Detonate(s,direct?p:s.target,direct);ReleaseShell(s);
    }
   }
  }
  void Detonate(TankShell s,Vector3 at,bool direct)
  {
   if(s.overWater&&!direct)
   {
    ShellSplashes++;RiverZone(at,out _,out var tile);Splash(tile,at,1.6f);
    if(sound&&fx!=null)fx.PlayOneShot(blastClip,.25f);
    return;
   }
   ShellBlasts++;
   if(!direct)CreateTankCrater(at);
   Emit(blastFx,at+Vector3.up*1.6f,Vector3.zero,7.5f,.9f,Color.white);
   for(int i=0;i<26;i++)
   {
    var dir=new Vector3(Random.Range(-1f,1f),Random.Range(.4f,1.3f),Random.Range(-1f,1f));
    Emit(sparkFx,at+Vector3.up*.4f,dir*Random.Range(4f,9f),Random.Range(.12f,.28f),Random.Range(.5f,1f),new Color(1,.62f,.2f,1));
   }
   for(int i=0;i<4;i++)Emit(smokeFx,at+new Vector3(Random.Range(-1f,1f),1+i*.5f,Random.Range(-1f,1f)),Vector3.up*Random.Range(1f,2.2f),Random.Range(3f,4.5f),Random.Range(1.6f,2.4f),new Color(.32f,.3f,.29f,.85f));
   if(sound&&fx!=null)fx.PlayOneShot(blastClip,Mathf.Lerp(.75f,.3f,Mathf.InverseLerp(0,60,Mathf.Abs(at.z))));
   // Xe khác trong bán kính nổ văng ra (xe tăng không bị đẩy).
   foreach(var v in vehicles)
   {
    if(!v.active||v.armored||v.riverFalling||v.wallCrashed)continue;
    Vector3 d=v.model.localPosition-at;d.y=0;float dist=d.magnitude;
    if(dist>BlastRadius+Mathf.Max(v.halfWidth,v.halfLength)*.6f)continue;
    Vector3 away=dist>.05f?d/dist:Vector3.right;
    Vector3 before=BodyVelocity(v),after=before+away*11f+Vector3.up*4;
    ActivateCollisionBody(v,before,after,new Vector2(away.x,away.z)*VehicleMass(v)*11f,Vector2.zero,false,true,11f);
    v.knockVelocity.y=Mathf.Max(v.knockVelocity.y,v.halfWidth>.7f?3.5f:5.5f);VehiclesBlasted++;
   }
   // Người chơi: trúng trực tiếp hoặc trong bán kính nổ (không đang nhảy cao qua) → văng ra, kết thúc chuyến.
   if(State!=RideState.Riding)return;
   Vector3 r=rider.localPosition-at;float flat=new Vector2(r.x,r.z).magnitude;
   if(direct||(flat<BlastRadius&&jumpHeight<2.2f))BlastRider(at);
  }
  void BlastRider(Vector3 at)
  {
   Vector3 away=rider.localPosition-at;away.y=0;away=away.sqrMagnitude>.01f?away.normalized:(rng.Next(2)==0?Vector3.left:Vector3.right);
   BeginRiderImpact(null,new Vector2(away.x,away.z),at,Vector3.forward*Speed);
   if(State!=RideState.Falling)return;
   // Nổ hất lên cao và ra xa; luôn kết thúc chuyến (kể cả khi đang bị CSGT đuổi).
   riderFallVelocity=away*8f+Vector3.up*7.5f+Vector3.forward*Speed*.25f;
   riderFallSpin=new Vector3(Random.Range(-220,220),Random.Range(-160,160),Random.Range(-260,260));
   CrashDuringPursuit=false;KilledByShell=true;
   notice="TRÚNG ĐẠN XE TĂNG · CHUYẾN ĐI KẾT THÚC";noticeTimer=4;
  }
  // Mỗi bước mô phỏng (đang chạy, đang té, sau kết thúc): đạn đang bay vẫn bay tới và nổ.
  void TickTankBarrage(float dt)
  {
   TickTankSchedule(dt);TickDistanceTankSchedule(dt);TickTankFire(dt);TickShells(dt);
  }

  // ------------------------------------------------------------ hiệu ứng (tạo một lần)
  void EnsureTankFx()
  {
   if(tankFxAnchor!=null)return;
   tankFxAnchor=new GameObject("Hiệu ứng đạn xe tăng • Blender").transform;tankFxAnchor.SetParent(transform,false);
   muzzleFx=FxSystem("Lửa đầu nòng",tankMuzzleMaterial,2,2,0,false);
   blastFx=FxSystem("Vụ nổ đạn pháo",tankExplosionMaterial,4,2,0,false);
   smokeFx=FxSystem("Khói nổ",tankExplosionMaterial,4,2,-.05f,true);
   sparkFx=FxSystem("Tia lửa mảnh vụn",tankSparkMaterial,1,1,1.4f,false);
   trailFx=FxSystem("Vệt khói đạn bay",tankSparkMaterial,1,1,-.1f,false);
   markerMesh=new Mesh{name="PV_TankWarningQuad"};
   markerMesh.vertices=new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(.5f,0,.5f),new Vector3(-.5f,0,.5f)};
   markerMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};markerMesh.triangles=new[]{0,2,1,0,3,2};markerMesh.RecalculateNormals();markerMesh.RecalculateBounds();
   cannonClip=Boom("Pháo xe tăng",.55f,38,.11f);blastClip=Boom("Đạn nổ",1.1f,28,.25f);
  }
  ParticleSystem FxSystem(string name,Material material,int tilesX,int tilesY,float gravity,bool smoke)
  {
   var go=new GameObject(name);go.transform.SetParent(tankFxAnchor,false);
   var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.playOnAwake=false;main.loop=false;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.maxParticles=400;main.gravityModifier=gravity;
   var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
   if(tilesX*tilesY>1)
   {
    var sheet=ps.textureSheetAnimation;sheet.enabled=true;sheet.numTilesX=tilesX;sheet.numTilesY=tilesY;
    // Khói nổ chỉ dùng 2 khung khói đen cuối của tấm vụ nổ.
    sheet.frameOverTime=smoke?new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.75f,1,.999f)):new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,0,1,.999f));
   }
   var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();
   gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,smoke?.4f:.75f),new GradientAlphaKey(0,1)});fade.color=gradient;
   if(smoke){var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.7f,1,1.6f));}
   var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Billboard;
   renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.sortingFudge=smoke?2:0;
   ps.Play();return ps;
  }
  void Emit(ParticleSystem ps,Vector3 gameLocal,Vector3 velocity,float size,float life,Color color)
  {
   if(ps==null)return;
   var e=new ParticleSystem.EmitParams{position=tankFxAnchor.InverseTransformPoint(transform.TransformPoint(gameLocal)),velocity=tankFxAnchor.InverseTransformDirection(transform.TransformDirection(velocity)),startSize=size,startLifetime=life,startColor=color,rotation=Random.Range(0,360)};
   ps.Emit(e,1);
  }
  Transform NewShellBody()
  {
   Transform t;
   if(tankShellPrefab!=null)
   {
    t=Instantiate(tankShellPrefab,tankFxAnchor.parent).transform;
    // Mesh Blender còn trục Z lên (giống mô hình cầu): xoay LOD về hệ Unity, chỉ giữ LOD0.
    foreach(var r in t.GetComponentsInChildren<MeshRenderer>(true))
    {r.transform.localRotation=Quaternion.Euler(-90,0,0);r.gameObject.SetActive(r.name.EndsWith("_LOD0"));r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;if(tankShellMaterial!=null)r.sharedMaterial=tankShellMaterial;}
    var lod=t.GetComponent<LODGroup>();if(lod!=null)Destroy(lod);
   }
   else {t=GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;t.SetParent(transform,false);t.localScale=new Vector3(.22f,.45f,.22f);Destroy(t.GetComponent<Collider>());}
   // Phóng to 1,8 lần so với cỡ thật để nhìn thấy đường bay từ góc máy sau xe.
   t.localScale=Vector3.one*1.8f;t.name="Đạn pháo xe tăng";return t;
  }
  Renderer NewMarker()
  {
   var go=new GameObject("Vòng cảnh báo đạn pháo");go.transform.SetParent(transform,false);
   go.AddComponent<MeshFilter>().sharedMesh=markerMesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=tankWarningMaterial;
   r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;return r;
  }
  static AudioClip Boom(string name,float seconds,float pitch,float decay)
  {
   int hz=22050,n=(int)(hz*seconds);var data=new float[n];var noise=new System.Random(name.Length*31);float low=0;
   for(int i=0;i<n;i++)
   {
    float t=(float)i/hz,env=Mathf.Exp(-t/decay);low=Mathf.Lerp(low,(float)noise.NextDouble()*2-1,.08f);
    data[i]=(Mathf.Sin(t*2*Mathf.PI*pitch*(1-t*.6f))*.55f+low*1.6f+((float)noise.NextDouble()*2-1)*.25f*Mathf.Exp(-t/.03f))*env;
   }
   var clip=AudioClip.Create(name,n,1,hz,false);clip.SetData(data,0);return clip;
  }
 }
}
