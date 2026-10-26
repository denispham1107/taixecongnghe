using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace PhoViet
{
 // Four downward nozzles use the authored Blender smoke flipbook. The mount
 // follows the animated car; released particles remain in the street frame.
 [DefaultExecutionOrder(110),DisallowMultipleComponent]
 public sealed class CarJumpJetVisual:MonoBehaviour
 {
  readonly List<ParticleSystem> systems=new List<ParticleSystem>();
  PhoVietGame game;PlayerCarVisual car;Material material;
  public bool Emitting {get;private set;}
  public int NozzleCount=>systems.Count/2;
  public int ParticleCount {get {int n=0;foreach(var p in systems)if(p!=null)n+=p.particleCount;return n;}}
  public void Initialize()
  {
   if(systems.Count>0)return;
   game=GetComponentInParent<PhoVietGame>();car=GetComponent<PlayerCarVisual>();
   if(game==null||car==null||car.jumpMounts==null)return;
   var texture=Resources.Load<Texture2D>("Boost/PV_WhiteBoostSmoke");if(texture==null)return;
   material=new Material(Shader.Find("PhoViet/Soft Particle")){name="PV_CarLiftSmoke",mainTexture=texture};
   foreach(var mount in car.jumpMounts)
   {
    if(mount==null)continue;
    var go=new GameObject("Khói trắng nâng xe • Blender");go.transform.SetParent(mount,false);
    go.transform.localRotation=Quaternion.Euler(90,0,0);
    var p=go.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    var m=p.main;m.loop=true;m.playOnAwake=false;m.duration=3;m.maxParticles=56;
    m.simulationSpace=ParticleSystemSimulationSpace.Custom;m.customSimulationSpace=game.transform;
    m.startLifetime=new ParticleSystem.MinMaxCurve(.24f,.42f);m.startSpeed=new ParticleSystem.MinMaxCurve(10,14);
    m.startSize=new ParticleSystem.MinMaxCurve(.18f,.27f);m.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
    var e=p.emission;e.rateOverTime=100;
    var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.055f;shape.angle=8;
    var size=p.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,.65f,1,3.8f));
    var c=p.colorOverLifetime;c.enabled=true;var gradient=new Gradient();
    gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.88f,.94f,1),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.9f,.06f),new GradientAlphaKey(.6f,.4f),new GradientAlphaKey(0,1)});c.color=gradient;
    var sheet=p.textureSheetAnimation;sheet.enabled=true;sheet.numTilesX=4;sheet.numTilesY=1;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(0,1);
    var noise=p.noise;noise.enabled=true;noise.strength=.10f;noise.frequency=2;noise.quality=ParticleSystemNoiseQuality.Low;
    var r=p.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.sortMode=ParticleSystemSortMode.Distance;
    r.SetActiveVertexStreams(new List<ParticleSystemVertexStream>{ParticleSystemVertexStream.Position,ParticleSystemVertexStream.Color,ParticleSystemVertexStream.UV});
    systems.Add(p);
    // A short, dense core gives each downward plume continuous thrust at the
    // nozzle, while the larger layer above breaks up into soft drifting smoke.
    var core=Instantiate(p,mount,false);core.name="Lõi phụt trắng";
    var cm=core.main;cm.maxParticles=48;cm.startLifetime=new ParticleSystem.MinMaxCurve(.12f,.20f);
    cm.startSize=new ParticleSystem.MinMaxCurve(.12f,.18f);cm.startSpeed=14;
    var ce=core.emission;ce.rateOverTime=200;var cs=core.shape;cs.radius=.035f;cs.angle=4;
    var cn=core.noise;cn.enabled=false;
    var cr=core.GetComponent<ParticleSystemRenderer>();cr.renderMode=ParticleSystemRenderMode.Stretch;cr.lengthScale=1.6f;cr.velocityScale=.02f;
    systems.Add(core);
   }
  }
  void LateUpdate(){Advance();}
  public void Advance()
  {
   Initialize();if(game==null)return;
   if(game.State==PhoVietGame.RideState.Paused){foreach(var p in systems)if(p.isPlaying)p.Pause();return;}
   bool on=game.State==PhoVietGame.RideState.Riding&&game.RiderJumpActive&&!game.ServiceStopped;
   if(on){foreach(var p in systems)if(!p.isPlaying)p.Play();Emitting=true;}
   else if(Emitting){foreach(var p in systems)p.Stop(true,ParticleSystemStopBehavior.StopEmitting);Emitting=false;}
   if(game.State!=PhoVietGame.RideState.Riding)Clear();
  }
  public void Simulate(float dt){Advance();if(game!=null&&game.State!=PhoVietGame.RideState.Paused)foreach(var p in systems)p.Simulate(dt,false,false);}
  public void Clear(){Emitting=false;foreach(var p in systems)if(p!=null)p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
  void OnDestroy(){if(material!=null)Destroy(material);}
 }
}
