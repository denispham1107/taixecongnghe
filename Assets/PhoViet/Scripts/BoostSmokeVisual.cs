using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
namespace PhoViet
{
 // Two small particle layers reuse a Blender-rendered volume flipbook. No fluid solver in WebGL.
 [DefaultExecutionOrder(100)]
 public sealed class BoostSmokeVisual:MonoBehaviour
 {
  PhoVietGame game;
  Transform nozzle;
  ParticleSystem jet,smoke;
  readonly List<Transform> nozzles=new List<Transform>();
  readonly List<ParticleSystem> systems=new List<ParticleSystem>();
  public int NozzleCount=>nozzles.Count;
  Material material;
  bool emitting;
  public bool Emitting=>emitting;
  public int ParticleCount {get {int n=0;foreach(var p in systems)if(p!=null)n+=p.particleCount;return n;}}
  public void Initialize(PhoVietGame owner)
  {
   if(nozzle!=null)return;game=owner;
   var sprite=Resources.Load<Texture2D>("Boost/PV_WhiteBoostSmoke");
   if(sprite==null){Debug.LogError("Missing Blender boost smoke flipbook");return;}
   var visual=GetComponent<BikeVisualAnimator>();var car=GetComponent<PlayerCarVisual>();
   var sockets=new List<Transform>();
   if(car!=null&&car.exhaustMounts!=null)foreach(var t in car.exhaustMounts)if(t!=null)sockets.Add(t);
   if(sockets.Count==0&&car!=null&&car.exhaustMount!=null)sockets.Add(car.exhaustMount);
   if(sockets.Count==0&&visual!=null&&visual.visual!=null)
    foreach(var t in visual.visual.GetComponentsInChildren<Transform>(true))if(t.name=="PV_MeshyExhaust"){sockets.Add(t);break;}
   if(sockets.Count==0)sockets.Add(null);
   material=new Material(Shader.Find("PhoViet/Soft Particle")){name="PV_WhiteBoostSmoke",mainTexture=sprite};
   foreach(var socket in sockets)
   {
    nozzle=new GameObject("Khói trắng phản lực • Blender").transform;
    nozzle.SetParent(socket!=null?socket:visual!=null?visual.visual:transform,false);
    nozzle.localPosition=socket!=null?Vector3.zero:new Vector3(.214f,.45f,-1.02f);
    nozzles.Add(nozzle);systems.Add(Create("Luồng phụt trắng",true));systems.Add(Create("Khói trắng tản",false));
   }
   nozzle=nozzles[0];jet=systems[0];smoke=systems[1];
  }

  ParticleSystem Create(string name,bool core)
  {
   var root=new GameObject(name);root.transform.SetParent(nozzle,false);root.transform.localRotation=Quaternion.Euler(0,180,0);
   var p=root.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var m=p.main;m.playOnAwake=false;m.loop=true;m.duration=4;m.maxParticles=64;
   m.simulationSpace=ParticleSystemSimulationSpace.Custom;m.customSimulationSpace=game.transform;
   m.startLifetime=new ParticleSystem.MinMaxCurve(core?.26f:.45f,core?.38f:.9f);
   m.startSpeed=new ParticleSystem.MinMaxCurve(core?8:5,core?10:7);
   m.startSize=new ParticleSystem.MinMaxCurve(core?.24f:.35f,core?.34f:.52f);
   m.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
   var e=p.emission;e.rateOverTime=core?120:50;
   var s=p.shape;s.shapeType=ParticleSystemShapeType.Cone;s.radius=core?.035f:.07f;s.angle=core?4:11;
   var size=p.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,core?.8f:.7f,1,core?1.5f:3.6f));
   var color=p.colorOverLifetime;color.enabled=true;
   var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.88f,.94f,.98f),1)},
    new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(core?1:.60f,.08f),new GradientAlphaKey(core?.85f:.3f,.55f),new GradientAlphaKey(0,1)});color.color=g;
   var sheet=p.textureSheetAnimation;sheet.enabled=true;sheet.numTilesX=4;sheet.numTilesY=1;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(0,1);sheet.cycleCount=1;
   if(!core){var noise=p.noise;noise.enabled=true;noise.strength=.22f;noise.frequency=1.7f;noise.scrollSpeed=.45f;noise.quality=ParticleSystemNoiseQuality.Low;}
   var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.sortMode=ParticleSystemSortMode.Distance;
   renderer.SetActiveVertexStreams(new List<ParticleSystemVertexStream>{ParticleSystemVertexStream.Position,ParticleSystemVertexStream.Color,ParticleSystemVertexStream.UV});
   return p;
  }
  void LateUpdate(){Advance();}
  public void Advance()
  {
   if(game==null||jet==null)return;
   if(game.State==PhoVietGame.RideState.Paused)
   {foreach(var p in systems)if(p.isPlaying)p.Pause();return;}
   bool on=game.BoostActive&&game.State==PhoVietGame.RideState.Riding&&!game.ServiceStopped;
   if(on)
   {foreach(var p in systems)if(!p.isPlaying)p.Play();emitting=true;}
   else if(emitting)
   {foreach(var p in systems)p.Stop(true,ParticleSystemStopBehavior.StopEmitting);emitting=false;}
   else if(game.State!=PhoVietGame.RideState.Riding)Clear();
  }
  public void Clear()
  {emitting=false;foreach(var p in systems)if(p!=null)p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
  // Existing particles belong to the old street frame; clear only the trail at a rebase.
  public void Rebase(){Clear();}
  public void Simulate(float dt)
  {Advance();if(jet!=null&&game.State!=PhoVietGame.RideState.Paused)foreach(var p in systems)p.Simulate(dt,false,false);}
  void OnDestroy(){if(material!=null)Destroy(material);foreach(var t in nozzles)if(t!=null)Destroy(t.gameObject);}
 }
}
