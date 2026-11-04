using UnityEngine;
namespace PhoViet
{
 public sealed class MissileFlightVisual:MonoBehaviour
 {
  public Transform flame;public ParticleSystem smoke;public AnimationCurve plumePulse;public float plumePeriod=.16666667f;
  public LineRenderer fireTrail;
  public const float FireTrailLifetime=.14f,FireTrailLength=6.5f;
  PhoVietGame game;float clock;bool emitting;readonly ParticleSystem.Particle[] trail=new ParticleSystem.Particle[32];
  readonly Vector3[] firePoints=new Vector3[32];readonly float[] fireAges=new float[32];int fireCount;
  MaterialPropertyBlock fireProperties;static readonly int FirePhase=Shader.PropertyToID("_Phase");
  public int FireTrailPointCount=>fireCount;
  void Awake(){game=GetComponentInParent<PhoVietGame>();fireProperties=new MaterialPropertyBlock();}
  public void Begin(){clock=0;emitting=true;if(game==null)game=GetComponentInParent<PhoVietGame>();if(smoke!=null){var main=smoke.main;main.customSimulationSpace=game.transform;smoke.Clear();smoke.Play();}if(flame!=null){flame.gameObject.SetActive(true);flame.localScale=new Vector3(1.8f,2.2f,1.8f);}ClearFire();UpdateFire(0);}
  public void End(){emitting=false;if(smoke!=null)smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);if(flame!=null)flame.gameObject.SetActive(false);ClearFire();}
  void LateUpdate(){Advance(Mathf.Min(Time.deltaTime,.05f));}
  public void Advance(float dt)
  {
   if(game==null)game=GetComponentInParent<PhoVietGame>();if(game==null||!emitting)return;
   if(game.State==PhoVietGame.RideState.Paused){if(smoke!=null&&smoke.isPlaying)smoke.Pause();return;}
   clock+=dt;if(smoke!=null&&smoke.isPaused)smoke.Play();
   if(flame!=null){float pulse=plumePulse==null?1:plumePulse.Evaluate(Mathf.Repeat(clock/Mathf.Max(.01f,plumePeriod),1));flame.localScale=new Vector3(1.8f,2.2f*pulse,1.8f);}
   UpdateFire(dt);
  }
  void ClearFire(){fireCount=0;if(fireTrail!=null){fireTrail.positionCount=0;fireTrail.enabled=false;}}
  void UpdateFire(float dt)
  {
   if(fireTrail==null)return;
   if(fireProperties==null)fireProperties=new MaterialPropertyBlock();
   fireProperties.SetFloat(FirePhase,clock);fireTrail.SetPropertyBlock(fireProperties);
   for(int i=0;i<fireCount;i++)fireAges[i]+=Mathf.Max(0,dt);
   while(fireCount>0&&fireAges[fireCount-1]>FireTrailLifetime)fireCount--;
   Vector3 nozzle=transform.TransformPoint(new Vector3(0,0,-.65f));
   if(fireCount==0){firePoints[0]=nozzle;firePoints[1]=nozzle-transform.forward*1.6f;fireAges[0]=fireAges[1]=0;fireCount=2;}
   else if((nozzle-firePoints[0]).sqrMagnitude>.0025f)
   {
    int last=Mathf.Min(fireCount,firePoints.Length-1);
    for(int i=last;i>0;i--){firePoints[i]=firePoints[i-1];fireAges[i]=fireAges[i-1];}
    firePoints[0]=nozzle;fireAges[0]=0;fireCount=Mathf.Min(fireCount+1,firePoints.Length);
   }
   else{firePoints[0]=nozzle;fireAges[0]=0;}
   // Bound overdraw even at boosted speed. History follows the curved flight,
   // rather than drawing a straight cone through the inside of a homing turn.
   float length=0;
   for(int i=1;i<fireCount;i++)
   {
    float segment=Vector3.Distance(firePoints[i-1],firePoints[i]);
    if(length+segment>FireTrailLength){firePoints[i]=Vector3.Lerp(firePoints[i-1],firePoints[i],(FireTrailLength-length)/Mathf.Max(.00001f,segment));fireCount=i+1;break;}
    length+=segment;
   }
   fireTrail.positionCount=fireCount;fireTrail.SetPositions(firePoints);fireTrail.enabled=fireCount>1;
  }
  public void RebaseTrail(){if(smoke!=null)smoke.Clear();ClearFire();}
  public void Scroll(float distance)
  {
   if(smoke!=null){int count=smoke.GetParticles(trail);for(int i=0;i<count;i++)trail[i].position-=Vector3.forward*distance;smoke.SetParticles(trail,count);}
   Vector3 delta=game!=null?game.transform.TransformVector(Vector3.forward*distance):Vector3.forward*distance;
   for(int i=0;i<fireCount;i++)firePoints[i]-=delta;
   if(fireTrail!=null&&fireCount>0)fireTrail.SetPositions(firePoints);
  }
  void OnDisable(){End();}
 }
}
