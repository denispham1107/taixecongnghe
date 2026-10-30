using UnityEngine;
namespace PhoViet
{
 // Only this skin is animated. The outer player root belongs to road/impact physics.
 [DisallowMultipleComponent,SelectionBase]
 public sealed class PlayerCarVisual:MonoBehaviour
 {
  public Transform visual,body,exhaustMount;
  public Transform[] wheels,steering;
  public Transform[] exhaustMounts,jumpMounts;
  public float frontAxle=1.2105f,rearAxle=1.1643f;
  public float WheelSteerAngle {get;private set;}
  public float ModelScale=>visual!=null?Mathf.Max(.001f,Mathf.Abs(visual.localScale.x)):1;
  public Animation animationPlayer;
  public AnimationClip[] clips;
  public Vector3 size=new Vector3(1.889312f,1.949565f,3.8f);
  public float wheelRadius=.357374f;
  public float WheelAngle {get;private set;}
  public float Pitch {get;private set;}
  public string CurrentClip {get;private set;}="Idle";
  PhoVietGame game;
  bool initialized;
  float clock,pitchVelocity;
  public void Initialize()
  {
   if(initialized||visual==null||animationPlayer==null)return;
   game=GetComponentInParent<PhoVietGame>();if(game==null)return;
   animationPlayer.playAutomatically=false;animationPlayer.cullingType=AnimationCullingType.AlwaysAnimate;animationPlayer.Stop();
   foreach(var clip in clips)animationPlayer.AddClip(clip,clip.name);
   initialized=true;
  }
  void LateUpdate(){Advance(Mathf.Min(Time.deltaTime,.05f));}
  public void Advance(float dt)
  {
   Initialize();if(!initialized||game.State==PhoVietGame.RideState.Paused)return;
   clock+=dt;bool moving=game.State==PhoVietGame.RideState.Riding;
   string clip=game.RiderCrashActive?"ImpactFront":moving&&game.Speed>.1f?"Drive":"Idle";
   var state=animationPlayer[clip];
   if(state!=null){animationPlayer.Play(clip,PlayMode.StopAll);state.speed=0;state.normalizedTime=clip=="Drive"?Mathf.Repeat(clock,1):0;animationPlayer.Sample();animationPlayer.Stop();CurrentClip=clip;}
   float target=0;
   if(moving&&game.RiderJumpActive)
   {
    float t=game.RiderJumpPose;
    target=game.RiderJumpClip=="JumpTakeoff"?-3*Mathf.Sin(t*Mathf.PI*.5f):game.RiderAirborne?Mathf.Lerp(-3,4,t):game.RiderJumpClip=="LandFront"?3*(1-t):game.RiderJumpClip=="LandRear"?-3*(1-t):2*Mathf.Sin(t*Mathf.PI*2)*(1-t);
   }
   if(moving&&!game.RiderJumpActive)target+=game.RiderCraterPitch;
   Pitch=Mathf.SmoothDamp(Pitch,target,ref pitchVelocity,.085f,Mathf.Infinity,dt);
   visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.Euler(Pitch,0,moving?game.RiderCraterRoll:0);
   // Pitch about the road without driving an axle below its support surface.
   if(moving)visual.localPosition=Vector3.up*(Mathf.Max(0,Mathf.Sin(Mathf.Abs(Pitch)*Mathf.Deg2Rad)*Mathf.Max(frontAxle,rearAxle))-game.RiderCraterDip);
   if(moving)WheelAngle=Mathf.Repeat(WheelAngle+game.Speed/wheelRadius*Mathf.Rad2Deg*dt,360);
   float delta=Mathf.Atan((frontAxle+rearAxle)*game.RiderLateralAcceleration/Mathf.Max(64,game.Speed*game.Speed))*Mathf.Rad2Deg;
   WheelSteerAngle=Mathf.Lerp(WheelSteerAngle,Mathf.Clamp(delta,-24,24),1-Mathf.Exp(-dt*16));
   for(int i=0;i<wheels.Length;i++){steering[i].localRotation=Quaternion.Euler(0,i<2?WheelSteerAngle:0,0);wheels[i].localRotation=Quaternion.Euler(WheelAngle,0,0);}
   if(game.RiderCrashActive&&!game.RiderInRiver)
   {
    float low=float.PositiveInfinity;
    for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)for(int y=0;y<=1;y++)
     low=Mathf.Min(low,visual.TransformPoint(new Vector3(x*size.x*.5f,y*size.y,z*size.z*.5f)/ModelScale).y);
    visual.position+=Vector3.up*Mathf.Max(0,game.transform.position.y+.01f-low);
   }
  }
 }
}
