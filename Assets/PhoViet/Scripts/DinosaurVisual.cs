using UnityEngine;
namespace PhoViet
{
 // Animation is sampled by the gameplay clock: pause, swept hits and the displayed tail agree.
 public sealed class DinosaurVisual:MonoBehaviour
 {
  public Animation player;
  public AnimationClip walk,sweep;
  public Transform[] tail;
  public Transform pelvis,head,leftFoot,rightFoot;
  public float Clock {get;private set;}
  public float SweepAge {get;private set;}=-1;
  public bool Sweeping=>SweepAge>=0;
  public bool TailDangerous=>SweepAge>=.85f&&SweepAge<=1.55f;
  bool ready;
  public void Initialize()
  {
   if(ready)return;
   player.playAutomatically=false;player.cullingType=AnimationCullingType.AlwaysAnimate;
   player.AddClip(walk,"DinoWalk");player.AddClip(sweep,"DinoSweep");ready=true;
   foreach(var r in GetComponentsInChildren<SkinnedMeshRenderer>(true))
    r.localBounds=new Bounds(new Vector3(0,3,0),new Vector3(20,12,20));
  }
  public void ResetPose(){Initialize();Clock=0;SweepAge=-1;Sample();}
  public void StartSweep(){if(Sweeping)return;SweepAge=0;Sample();}
  public void Advance(float dt)
  {
   Initialize();Clock+=dt;
   if(Sweeping){SweepAge+=dt;if(SweepAge>=2.8f)SweepAge=-1;}
   Sample();
  }
  void Sample()
  {
   var w=player["DinoWalk"];var s=player["DinoSweep"];
   float blend=Sweeping?Mathf.SmoothStep(0,1,Mathf.Clamp01(SweepAge/.20f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((2.8f-SweepAge)/.20f)):0;
   w.enabled=true;w.wrapMode=WrapMode.Loop;w.time=Clock%1.4f;w.weight=1-blend;
   s.enabled=Sweeping;s.wrapMode=WrapMode.ClampForever;s.time=Mathf.Max(0,SweepAge);s.weight=blend;
   w.speed=s.speed=0;
   player.Sample();
  }
 }
}
