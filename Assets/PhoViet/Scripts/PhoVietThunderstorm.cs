using System;using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public partial class TrafficVehicle { [NonSerialized] public bool lightningWreck; }
  public bool thunderstormEnabled=true;
  public Material stormRainMaterial,stormBoltMaterial,stormWarningMaterial;
  public Transform stormFireTemplate;
  public const float StormCycle=30,LightningWarningSeconds=.8f,LightningRadius=1.05f;
  public float StormTime {get;private set;}
  public float StormIntensity {get;private set;}
  public float StormFlash {get;private set;}
  public int LightningStrikes {get;private set;}
  public int LightningVehiclesHit {get;private set;}
  public bool KilledByLightning {get;private set;}
  public bool PoliceLightningWreck {get;private set;}
  public bool LightningPending=>lightningWarning>0;
  public Vector3 LightningTarget=>lightningPoint;
  public float LightningWarningRemaining=>lightningWarning;
  ThunderstormVisual stormVisual;System.Random stormRandom;
  Vector3 lightningPoint;float lightningWarning,nextLightning,boltAge=10;
  public static float RainAt(float time)
  {
   float t=Mathf.Repeat(Mathf.Max(0,time),StormCycle);
   return t<15?0:t<18?Mathf.SmoothStep(0,1,(t-15)/3):t<27?1:1-Mathf.SmoothStep(0,1,(t-27)/3);
  }
  void EnsureThunderstorm()
  {
   if(stormVisual!=null||!Application.isPlaying||stormRainMaterial==null)return;
   stormVisual=gameObject.AddComponent<ThunderstormVisual>();
   stormVisual.Initialize(this,stormRainMaterial,stormBoltMaterial,stormWarningMaterial,stormFireTemplate);
  }
  void ResetThunderstorm()
  {
   StormTime=StormIntensity=StormFlash=lightningWarning=0;boltAge=10;nextLightning=.55f;
   LightningStrikes=LightningVehiclesHit=0;KilledByLightning=PoliceLightningWreck=false;
   stormRandom=new System.Random(RunSeed^0x514b27);
   foreach(var v in vehicles)v.lightningWreck=false;
   EnsureThunderstorm();if(stormVisual!=null)stormVisual.Clear();Shader.SetGlobalFloat("_PVStormWetness",0);
  }
  // Called at the end of each control step; all decisions use simulation time.
  public void TickThunderstorm(float dt,bool advanceClock)
  {
   if(dt<=0||State==RideState.Paused)return;
   if(!thunderstormEnabled){StormIntensity=StormFlash=lightningWarning=0;return;}
   EnsureThunderstorm();
   bool driving=advanceClock&&State==RideState.Riding&&!JobOffersOpen;
   if(driving)StormTime+=dt;
   StormIntensity=RainAt(StormTime);boltAge+=dt;
   StormFlash=boltAge<.2f?(.24f*Mathf.Exp(-boltAge*30)+.12f*Mathf.Exp(-Mathf.Pow((boltAge-.105f)/.022f,2))):0;
   if(State==RideState.GameOver||State==RideState.Jailed||State==RideState.Arrested){lightningWarning=0;return;}
   if(lightningWarning>0)
   {
    lightningWarning=Mathf.Max(0,lightningWarning-dt);
    if(lightningWarning==0)StrikeLightning(lightningPoint);
   }
   float phase=Mathf.Repeat(StormTime,StormCycle);
   if(driving&&phase>=18&&phase<27&&!LightningPending)
   {
    nextLightning-=dt;
    if(nextLightning<=0)
    {
     // Separate random stream: adding weather never changes the traffic seed sequence.
     int lane=stormRandom.Next(4);float lead=Mathf.Max(12,Speed*LightningWarningSeconds);
     WarnLightning(new Vector3(LaneX[lane],0,rider.localPosition.z+lead-3+(float)stormRandom.NextDouble()*23));
     nextLightning=2.1f+(float)stormRandom.NextDouble()*.7f;
    }
   }
   else if(phase<18)nextLightning=.55f;
  }
  public void WarnLightning(Vector3 at)
  {lightningPoint=at;lightningWarning=LightningWarningSeconds;}
  bool UnderLightning(Transform body,float width,float length,Vector3 at)
  {
   var d=Quaternion.Inverse(Quaternion.Euler(0,body.localEulerAngles.y,0))*(at-body.localPosition);
   float x=Mathf.Max(0,Mathf.Abs(d.x)-width),z=Mathf.Max(0,Mathf.Abs(d.z)-length);
   return x*x+z*z<=LightningRadius*LightningRadius;
  }
  public void StrikeLightning(Vector3 at)
  {
   if(!thunderstormEnabled||State==RideState.Paused||State==RideState.Ready||State==RideState.Jailed)return;
   EnsureThunderstorm();lightningWarning=0;LightningStrikes++;boltAge=0;StormFlash=.24f;
   var visualPoint=at;if(IsStormWater(at))visualPoint.y=RiverBlock.WaterY;
   if(stormVisual!=null)stormVisual.Strike(visualPoint,LightningStrikes);
   if(sound&&fx!=null){EnsureTankFx();fx.PlayOneShot(blastClip,.6f);}
   foreach(var v in vehicles)
   {
    if(!v.active||v.armored||v.lightningWreck||v.riverFalling||v.wallCrashed||!UnderLightning(v.model,v.halfWidth,v.halfLength,at))continue;
    Vector3 before=BodyVelocity(v),direction=before;direction.y=0;
    if(direction.sqrMagnitude<.1f)direction=v.model.localRotation*Vector3.forward;direction.Normalize();
    ActivateCollisionBody(v,before,direction*Mathf.Clamp(before.magnitude*.7f+4,5,22),new Vector2(direction.x,direction.z)*VehicleMass(v)*8,Vector2.zero,false,true,12);
    v.lightningWreck=true;v.recovery=null;v.knockVelocity.y=4.5f;v.motionVelocity=v.knockVelocity;
    v.angularVelocity+=new Vector3(18,0,v.halfWidth<.7f?90:35);LightningVehiclesHit++;
    if(stormVisual!=null)stormVisual.Burn(v.model,v);
   }
   if(policeBike!=null&&policeBike.gameObject.activeSelf&&!PoliceLightningWreck&&!PoliceInRiver&&UnderLightning(policeBike,.5f,1.2f,at))StrikeStormPolice();
   if((State==RideState.Riding||State==RideState.Falling)&&UnderLightning(rider,PlayerHalfWidth,PlayerHalfLength,at))StrikeStormRider();
  }
  void StrikeStormRider()
  {
   Vector3 before=State==RideState.Falling?riderFallVelocity:riderVelocityValid?riderRoadVelocity:rider.localRotation*Vector3.forward*Speed;
   if(State==RideState.Riding)BeginRiderImpact(null,Vector2.up,rider.localPosition,before);
   if(State!=RideState.Falling)return;
   Vector3 flat=before;flat.y=0;float speed=flat.magnitude;Vector3 direction=speed>.2f?flat/speed:rider.localRotation*Vector3.forward;
   riderFallVelocity=direction*Mathf.Clamp(speed*.7f+4,6,25)+Vector3.up*5.5f;
   riderFallSpin=new Vector3(30,0,75);CrashDuringPursuit=false;KilledByLightning=true;
   IsChased=false;routePolice=false;ResetCrashPoliceNavigation();siren?.Stop();
   if(stormVisual!=null)stormVisual.Burn(rider,null);
   notice="TRÚNG SÉT · CHUYẾN ĐI KẾT THÚC";noticeTimer=5;
  }
  void StrikeStormPolice()
  {
   if(policeBody==null)policeBody=new TrafficVehicle{model=policeBike,halfWidth=.5f,halfLength=1.2f};
   Vector3 before=policeBike.localRotation*Vector3.forward*Mathf.Max(PoliceSpeed,8);
   ActivateCollisionBody(policeBody,before,before*.7f,Vector2.up*100,Vector2.zero,false,true,12);
   policeBody.knockVelocity.y=4.5f;policeBody.lightningWreck=true;PoliceLightningWreck=true;
   IsChased=false;routePolice=false;PoliceSpeed=PoliceFallRemaining=chaseBoost=0;CrashDuringPursuit=false;
   dinosaurPoliceVelocity=Vector3.zero;ResetCrashPoliceNavigation();siren?.Stop();
   if(stormVisual!=null)stormVisual.Burn(policeBike,policeBody);
   notice=State==RideState.Falling?"CSGT TRÚNG SÉT · CHUYẾN ĐI KẾT THÚC":"CSGT TRÚNG SÉT · ĐÃ THOÁT TRUY ĐUỔI";noticeTimer=3;
  }
  void TickStormPoliceWreck(float dt)
  {
   if(!PoliceLightningWreck||policeBody==null||PoliceInRiver||dt<=0)return;
   Vector3 previous=policeBike.localPosition;TickCollisionBody(policeBody,dt);CheckPoliceRiver(previous,dt);
   if(policeBike.localPosition.z< -90)policeBike.gameObject.SetActive(false);
  }
  void ScrollThunderstorm(float delta){if(LightningPending)lightningPoint-=Vector3.forward*delta;stormVisual?.Scroll(delta);}
  void RebaseThunderstorm(Quaternion inverse,Vector3 origin)
  {lightningPoint=inverse*(lightningPoint-origin);stormVisual?.Rebase(inverse,origin);}
  public bool IsStormWater(Vector3 point)=>riversEnabled&&OverRiver(point);
  void RenderThunderstorm()
  {if(stormVisual!=null)stormVisual.Render(StormIntensity,StormFlash,lightningWarning,lightningPoint,boltAge,sound);}
 }
}
