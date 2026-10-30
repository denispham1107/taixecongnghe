using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
namespace PhoViet
{
 // A local rain volume, five shared-material bolt draws and pooled baked fires.
 // No per-drop GameObjects, scene-wide material clones or extra realtime lights.
 public sealed class ThunderstormVisual:MonoBehaviour
 {
  sealed class BurnEffect {public Transform root,target;public PhoVietGame.TrafficVehicle body;public ParticleSystem[] particles;public bool active;}
  PhoVietGame game;Transform root,rainRoot,boltRoot,warning,fireTemplate;
  ParticleSystem rain,splashes;AudioSource rainAudio;AudioClip rainClip;
  readonly List<BurnEffect> burns=new List<BurnEffect>();
  readonly List<Material> owned=new List<Material>();
  LineRenderer[] bolts;Mesh warningMesh;Material warningMaterial;
  bool paused;float splashClock;System.Random random=new System.Random(5105);
  public int BurningCount {get{int count=0;foreach(var b in burns)if(b.active)count++;return count;}}
  public int ParticleBudget=>2400;
  public void Initialize(PhoVietGame owner,Material rainMat,Material boltMat,Material warnMat,Transform fire)
  {
   game=owner;fireTemplate=fire;
   root=new GameObject("Giông sét • FX pooled").transform;root.SetParent(owner.transform,false);
   rainRoot=new GameObject("Mưa • vùng quanh camera").transform;rainRoot.SetParent(root,false);
   rain=MakeRain("Hạt mưa nhỏ",rainMat,2200);
   var main=rain.main;main.startLifetime=.85f;main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.025f,.045f);main.startColor=new Color(.76f,.86f,1,.5f);
   var shape=rain.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(18,1,82);
   var velocity=rain.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=1.5f;velocity.y=-29;velocity.z=-2;
   var renderer=rain.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=4;renderer.velocityScale=.035f;
   rain.transform.localRotation=Quaternion.identity;
   splashes=MakeRain("Giọt bắn mặt đường",rainMat,200);main=splashes.main;main.startLifetime=.15f;main.startSize=new ParticleSystem.MinMaxCurve(.035f,.08f);main.startSpeed=0;main.startColor=new Color(.7f,.83f,1,.45f);
   splashes.transform.SetParent(root,false);var splashShape=splashes.shape;splashShape.enabled=false;
   boltRoot=new GameObject("Sét phân nhánh • hai nhịp sáng").transform;boltRoot.SetParent(root,false);
   bolts=new LineRenderer[5];for(int i=0;i<5;i++)
   {
    var line=new GameObject(i==0?"Lõi sét":i==1?"Quầng sáng":"Nhánh sét").AddComponent<LineRenderer>();line.transform.SetParent(boltRoot,false);
    line.useWorldSpace=false;line.sharedMaterial=boltMat;line.alignment=LineAlignment.View;line.textureMode=LineTextureMode.Stretch;
    line.numCapVertices=2;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
    line.widthMultiplier=i==1?.42f:i==0?.12f:.045f;line.startColor=line.endColor=i==1?new Color(.3f,.62f,1,.18f):new Color(.8f,.93f,1,.95f);bolts[i]=line;
   }
   boltRoot.gameObject.SetActive(false);
   warning=new GameObject("Cảnh báo sét • 0,8 giây").transform;warning.SetParent(root,false);
   warningMesh=new Mesh{name="PV_LightningWarningQuad"};warningMesh.vertices=new[]{new Vector3(-1,0,-1),new Vector3(1,0,-1),new Vector3(1,0,1),new Vector3(-1,0,1)};
   warningMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};warningMesh.colors=new[]{Color.white,Color.white,Color.white,Color.white};warningMesh.triangles=new[]{0,2,1,0,3,2};warningMesh.RecalculateNormals();
   warning.gameObject.AddComponent<MeshFilter>().sharedMesh=warningMesh;var wr=warning.gameObject.AddComponent<MeshRenderer>();warningMaterial=new Material(warnMat);owned.Add(warningMaterial);wr.sharedMaterial=warningMaterial;wr.shadowCastingMode=ShadowCastingMode.Off;warning.gameObject.SetActive(false);
   rainAudio=root.gameObject.AddComponent<AudioSource>();rainAudio.loop=true;rainAudio.playOnAwake=false;rainAudio.volume=0;
   var noise=new System.Random(361);float[] samples=new float[22050*2];float low=0;
   for(int i=0;i<samples.Length;i++){float n=(float)noise.NextDouble()*2-1;low=Mathf.Lerp(low,n,.12f);samples[i]=(n*.28f+low*.5f)*(.8f+.2f*Mathf.Sin(i*.0006f));}
   rainClip=AudioClip.Create("Mưa giông • tiếng mưa",samples.Length,1,22050,false);rainClip.SetData(samples,0);rainAudio.clip=rainClip;
   for(int i=0;i<8&&fireTemplate!=null;i++)NewBurn();
  }
  ParticleSystem MakeRain(string name,Material material,int limit)
  {
   var go=new GameObject(name);go.transform.SetParent(rainRoot,false);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var m=ps.main;m.loop=true;m.playOnAwake=false;m.maxParticles=limit;m.simulationSpace=ParticleSystemSimulationSpace.World;
   var e=ps.emission;e.enabled=false;var r=go.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return ps;
  }
  BurnEffect NewBurn()
  {
   var copy=Instantiate(fireTemplate,root);copy.name="Cháy do sét • Mantaflow baked";copy.gameObject.SetActive(false);
   foreach(var light in copy.GetComponentsInChildren<Light>(true))light.enabled=false;
   var b=new BurnEffect{root=copy,particles=copy.GetComponentsInChildren<ParticleSystem>(true)};
   foreach(var ps in b.particles){var main=ps.main;main.maxParticles=Mathf.Min(64,main.maxParticles);}
   burns.Add(b);return b;
  }
  public void Burn(Transform target,PhoVietGame.TrafficVehicle body)
  {
   if(fireTemplate==null)return;BurnEffect chosen=null;
   foreach(var b in burns){if(b.active&&b.target==target)return;if(!b.active&&chosen==null)chosen=b;}
   if(chosen==null&&burns.Count<game.vehicles.Count+2)chosen=NewBurn();if(chosen==null)return;
   chosen.target=target;chosen.body=body;chosen.active=true;chosen.root.localScale=Vector3.one*(body==null||body.halfWidth>.7f?1.3f:.9f);
   chosen.root.gameObject.SetActive(true);PositionFire(chosen);foreach(var ps in chosen.particles){ps.Clear();ps.Play();}
  }
  void PositionFire(BurnEffect b)
  {
   bool car=b.body==null||b.body.halfWidth>.7f;float front=b.body==null?1.1f:Mathf.Min(b.body.halfLength*.42f,2.2f);
   Vector3 p=b.target.TransformPoint(new Vector3(0,car?.9f:.65f,front));p.y=Mathf.Max(p.y,b.target.position.y+.3f);
   b.root.position=p;b.root.rotation=Quaternion.identity;
  }
  public void Strike(Vector3 point,int seed)
  {
   boltRoot.localPosition=point;boltRoot.localRotation=Quaternion.identity;
   var rng=new System.Random(seed*7919);var points=new Vector3[23];
   for(int i=0;i<points.Length;i++){float y=30*(1-i/22f);points[i]=new Vector3(i==22?0:((float)rng.NextDouble()-.5f)*1.8f,y+.06f,i==22?0:((float)rng.NextDouble()-.5f)*1.3f);}
   for(int i=0;i<2;i++){bolts[i].positionCount=points.Length;bolts[i].SetPositions(points);}
   for(int i=2;i<5;i++){int at=4+i*3;var branch=new Vector3[7];for(int j=0;j<7;j++)branch[j]=points[at]+new Vector3((i%2==0?1:-1)*j*.45f+((float)rng.NextDouble()-.5f)*.5f,-j*.8f,j*.25f);bolts[i].positionCount=branch.Length;bolts[i].SetPositions(branch);}
   boltRoot.gameObject.SetActive(true);
  }
  public void Render(float intensity,float flash,float remaining,Vector3 point,float age,bool sound)
  {
   if(root==null)return;bool hide=game.State==PhoVietGame.RideState.Ready||game.State==PhoVietGame.RideState.Jailed||!game.thunderstormEnabled;
   bool pause=game.State==PhoVietGame.RideState.Paused;
   if(paused!=pause){paused=pause;foreach(var ps in root.GetComponentsInChildren<ParticleSystem>())if(pause)ps.Pause();else ps.Play();}
   var emission=rain.emission;emission.enabled=!hide&&intensity>.001f;emission.rateOverTime=Mathf.Lerp(80,1900,intensity)*intensity;
   rainRoot.position=game.rider.position+Vector3.up*17+game.transform.forward*30;
   var wind=rain.velocityOverLifetime;wind.z=-(game.Speed+2);
   if(!hide&&!pause&&intensity>.001f){if(!rain.isPlaying)rain.Play();if(!splashes.isPlaying)splashes.Play();}
   if(hide){rain.Clear();splashes.Clear();}
   bool warn=!hide&&remaining>0;warning.gameObject.SetActive(warn);
   if(warn){var p=point;p.y=game.riversEnabled&&game.IsStormWater(point)?RiverBlock.WaterY+.04f:.045f;warning.localPosition=p;warning.localScale=Vector3.one*1.8f;warningMaterial.SetFloat("_Progress",1-remaining/PhoVietGame.LightningWarningSeconds);}
   boltRoot.gameObject.SetActive(!hide&&age<.22f&&(age<.065f||age>.09f));
   if(!hide&&!pause&&intensity>.05f){splashClock+=Time.deltaTime*intensity*45;int count=Mathf.Min(8,(int)splashClock);splashClock-=count;for(int i=0;i<count;i++){var at=new Vector3((float)random.NextDouble()*13-6.5f,.035f,game.rider.localPosition.z+(float)random.NextDouble()*65);if(game.IsStormWater(at))continue;var e=new ParticleSystem.EmitParams{position=game.transform.TransformPoint(at),velocity=Vector3.up*.5f};splashes.Emit(e,1);}}
   if(!hide&&!pause&&intensity>.01f&&!rainAudio.isPlaying)rainAudio.Play();rainAudio.volume=!hide&&!pause&&sound?intensity*.16f:0;
   Shader.SetGlobalFloat("_PVStormWetness",hide?0:intensity);
   foreach(var b in burns)if(b.active)
   {
    bool valid=b.target!=null&&b.target.gameObject.activeInHierarchy&&b.target.localPosition.y>RiverBlock.WaterY+.25f&&(b.body==null?game.KilledByLightning:b.body.lightningWreck);
    if(!valid){b.active=false;foreach(var ps in b.particles)ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);b.root.gameObject.SetActive(false);continue;}
    PositionFire(b);
   }
  }
  public void Scroll(float distance){if(boltRoot!=null)boltRoot.localPosition-=Vector3.forward*distance;}
  public void Rebase(Quaternion inverse,Vector3 origin){if(boltRoot!=null){boltRoot.localPosition=inverse*(boltRoot.localPosition-origin);boltRoot.localRotation=inverse*boltRoot.localRotation;}}
  public void Clear()
  {
   if(root==null)return;rain.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);splashes.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);rainAudio.Stop();
   foreach(var b in burns){b.active=false;b.root.gameObject.SetActive(false);foreach(var ps in b.particles)ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
   warning.gameObject.SetActive(false);boltRoot.gameObject.SetActive(false);splashClock=0;paused=false;
  }
  void OnDestroy(){Shader.SetGlobalFloat("_PVStormWetness",0);foreach(var mat in owned)if(mat!=null)Destroy(mat);if(warningMesh!=null)Destroy(warningMesh);if(rainClip!=null)Destroy(rainClip);}
 }
}
