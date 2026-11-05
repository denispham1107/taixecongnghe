using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 // Separate from weather/aircraft fire: many surface attachments per vehicle,
 // generation-safe pooling and no change to crash physics or recovery rules.
 public sealed class ContactFireVisual:MonoBehaviour
 {
  sealed class Spot
  {
   public Transform root;public ParticleSystem[] particles;
   public PhoVietGame.TrafficVehicle body;public Vector3 anchor;public int generation;
   public bool active;
  }
  readonly List<Spot> spots=new List<Spot>();PhoVietGame game;Transform template,container;bool paused;
  public int ActiveCount{get{int n=0;foreach(var s in spots)if(s.active)n++;return n;}}
  public int PoolCount=>spots.Count;
  public int CountFor(Transform target){int n=0;foreach(var s in spots)if(s.active&&s.body.model==target)n++;return n;}
  public void Initialize(PhoVietGame owner,Transform source)
  {game=owner;template=source;container=new GameObject("Cháy tại điểm va chạm • pooled").transform;container.SetParent(transform,false);}
  // 21.136: instantiate during loading rather than at the first crash.
  public void Prewarm(int count)
  {
   while(template!=null&&spots.Count<count)
   {
    var root=Instantiate(template,container);root.name="Lửa + khói • tiếp xúc";root.gameObject.SetActive(false);
    foreach(var light in root.GetComponentsInChildren<Light>(true))light.enabled=false;
    var spot=new Spot{root=root,particles=root.GetComponentsInChildren<ParticleSystem>(true)};
    foreach(var ps in spot.particles){var m=ps.main;m.maxParticles=Mathf.Min(32,m.maxParticles);}
    spots.Add(spot);
   }
  }
  public void Ignite(PhoVietGame.TrafficVehicle body,Vector3 anchor)
  {
   if(body.armored||template==null)return;
   Spot free=null;
   foreach(var s in spots)
   {
    // Spawns/retirements can occur several times in one simulation step, before
    // rendering. Reclaim stale generations here as well as in Render.
    if(s.active&&!Valid(s))Release(s);
    if(s.active&&s.body==body&&s.generation==body.spawnGeneration&&(s.anchor-anchor).sqrMagnitude<.65f*.65f)
    {return;}
    if(!s.active&&free==null)free=s;
   }
   if(free==null)
   {
    var root=Instantiate(template,container);root.name="Lửa + khói • tiếp xúc";root.gameObject.SetActive(false);
    foreach(var light in root.GetComponentsInChildren<Light>(true))light.enabled=false;
    free=new Spot{root=root,particles=root.GetComponentsInChildren<ParticleSystem>(true)};
    // Baked flipbooks retain detail without any new real-time lights/shadows.
    foreach(var ps in free.particles){var m=ps.main;m.maxParticles=Mathf.Min(32,m.maxParticles);}
    spots.Add(free);
   }
   free.body=body;free.generation=body.spawnGeneration;free.anchor=anchor;
   free.root.localScale=Vector3.one*(body.halfWidth>.7f?.70f:.48f);StartSpot(free);Position(free);
  }
  void StartSpot(Spot s)
  {s.active=true;s.root.gameObject.SetActive(true);foreach(var ps in s.particles){ps.Clear();ps.Play();}}
  void Position(Spot s)
  {s.root.position=s.body.model.TransformPoint(s.anchor);s.root.rotation=Quaternion.identity;}
  void Release(Spot s)
  {s.active=false;s.root.gameObject.SetActive(false);foreach(var ps in s.particles)ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
  bool Valid(Spot s)
  {
   var b=s.body;if(b==null||b.model==null)return false;
   bool player=b.model==game.rider,police=b.model==game.policeBike;
   return b.model.gameObject.activeInHierarchy&&!b.armored&&b.spawnGeneration==s.generation&&(player||police||b.active)&&!b.riverFalling&&b.model.localPosition.y>RiverBlock.WaterY+.25f&&game.State!=PhoVietGame.RideState.Ready;
  }
  public void Render()
  {
   bool pause=game.State==PhoVietGame.RideState.Paused;
   if(paused!=pause){paused=pause;foreach(var s in spots)if(s.active)foreach(var ps in s.particles)if(pause)ps.Pause();else ps.Play();}
   foreach(var s in spots)if(s.active)
   {
    if(!Valid(s))
    {Release(s);continue;}
    Position(s);
   }
  }
  public void Clear(){foreach(var s in spots)if(s.active)Release(s);paused=false;}
  void OnDestroy(){if(container!=null)Destroy(container.gameObject);}
 }
}
