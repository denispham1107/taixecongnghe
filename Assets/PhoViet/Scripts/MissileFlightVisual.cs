using UnityEngine;
namespace PhoViet
{
 public sealed class MissileFlightVisual:MonoBehaviour
 {
  public Transform flame;public ParticleSystem smoke;public AnimationCurve plumePulse;public float plumePeriod=.16666667f;
  PhoVietGame game;float clock;bool emitting;readonly ParticleSystem.Particle[] trail=new ParticleSystem.Particle[24];
  void Awake(){game=GetComponentInParent<PhoVietGame>();}
  public void Begin(){clock=0;emitting=true;if(game==null)game=GetComponentInParent<PhoVietGame>();if(smoke!=null){var main=smoke.main;main.customSimulationSpace=game.transform;smoke.Clear();smoke.Play();}if(flame!=null)flame.gameObject.SetActive(true);}
  public void End(){emitting=false;if(smoke!=null)smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);if(flame!=null)flame.gameObject.SetActive(false);}
  void LateUpdate(){Advance(Mathf.Min(Time.deltaTime,.05f));}
  public void Advance(float dt)
  {
   if(game==null)game=GetComponentInParent<PhoVietGame>();if(game==null||!emitting)return;
   if(game.State==PhoVietGame.RideState.Paused){if(smoke!=null&&smoke.isPlaying)smoke.Pause();return;}
   clock+=dt;if(smoke!=null&&smoke.isPaused)smoke.Play();
   if(flame!=null){float pulse=plumePulse==null?1:plumePulse.Evaluate(Mathf.Repeat(clock/Mathf.Max(.01f,plumePeriod),1));flame.localScale=new Vector3(1,pulse,1);}
  }
  public void RebaseTrail(){if(smoke!=null)smoke.Clear();}
  public void Scroll(float distance){if(smoke==null)return;int count=smoke.GetParticles(trail);for(int i=0;i<count;i++)trail[i].position-=Vector3.forward*distance;smoke.SetParticles(trail,count);}
  void OnDisable(){End();}
 }
}
