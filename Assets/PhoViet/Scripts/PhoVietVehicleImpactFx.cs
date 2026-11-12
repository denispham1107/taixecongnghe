using UnityEngine;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  ParticleSystem vehicleShards;
  Mesh vehicleShardMesh;
  Material vehicleShardMaterial;

  void EnsureVehicleImpactFx()
  {
   if(vehicleShards!=null||tankCraterMaterial==null)return;
   EnsureTankFx();
   vehicleShards=FxSystem("Mảnh vỡ va chạm phương tiện",null,1,1,1.1f,false);
   var main=vehicleShards.main;
   main.maxParticles=320;
   main.startSize3D=true;
   main.startRotation3D=true;
   var spin=vehicleShards.rotationOverLifetime;
   spin.enabled=true;
   spin.separateAxes=true;
   spin.x=new ParticleSystem.MinMaxCurve(-11f,11f);
   spin.y=new ParticleSystem.MinMaxCurve(-14f,14f);
   spin.z=new ParticleSystem.MinMaxCurve(-9f,9f);
   var size=vehicleShards.sizeOverLifetime;
   size.enabled=true;
   size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(
       new Keyframe(0,1),new Keyframe(.72f,1),new Keyframe(1,0)));
   vehicleShardMesh=TankCraterGeometry.Debris();
   vehicleShardMesh.name="PV_VehiclePanelFragment";
   vehicleShardMaterial=new Material(tankCraterMaterial){name="PV_VehiclePanelFragment",color=Color.white};
   vehicleShardMaterial.SetFloat("_UseVertexColor",1);
   var renderer=vehicleShards.GetComponent<ParticleSystemRenderer>();
   renderer.renderMode=ParticleSystemRenderMode.Mesh;
   renderer.mesh=vehicleShardMesh;
   renderer.sharedMaterial=vehicleShardMaterial;
  }

  void EmitVehicleImpact(Vector3 point,Vector3 normal,float closing)
  {
   if(!Application.isPlaying||(point-rider.localPosition).sqrMagnitude>115*115)return;
   EnsureVehicleImpactFx();
   if(vehicleShards==null)return;
   float severity=Mathf.InverseLerp(.35f,42f,closing);
   int count=Mathf.RoundToInt(Mathf.Lerp(4,32,severity));
   float speed=Mathf.Lerp(2.5f,13,severity);
   var origin=tankFxAnchor.InverseTransformPoint(transform.TransformPoint(point));
   var axis=tankFxAnchor.InverseTransformDirection(transform.TransformDirection(normal));
   var up=tankFxAnchor.InverseTransformDirection(transform.up);
   for(int i=0;i<count;i++)
   {
    float size=Random.Range(.13f,.32f)*(1+severity*.65f);
    Vector3 direction=axis*(i%2==0?.65f:-.65f)+Random.insideUnitSphere*.85f+up*.62f;
    Color tone=i%3==0?new Color(.25f,.28f,.32f):i%3==1?new Color(.87f,.91f,.95f):new Color(.26f,.51f,.76f);
    var fragment=new ParticleSystem.EmitParams{
     position=origin+Random.insideUnitSphere*.12f,
     velocity=direction*Random.Range(speed*.55f,speed),
     startSize3D=new Vector3(size,size*.27f,size*.72f),
     rotation3D=Random.insideUnitSphere*180,
     startColor=tone,
     startLifetime=Random.Range(.28f,.73f)+severity*.17f
    };
    vehicleShards.Emit(fragment,1);
   }
   int sparks=2+Mathf.RoundToInt(severity*8);
   for(int i=0;i<sparks;i++)
    Emit(sparkFx,point,(Random.insideUnitSphere+normal*.3f+Vector3.up*.4f)*speed,
     Random.Range(.04f,.095f),Random.Range(.09f,.24f),new Color(1,.72f,.35f,.95f));
  }

  void EmitCrashExplosion(Transform target,Bounds bounds,Vector3 motion,bool airborne)
  {
   Vector3 point=transform.InverseTransformPoint(target.TransformPoint(bounds.center));
   Vector3 viewer=ShowFootCamera&&!InsideShop?footActor.localPosition:rider.localPosition;
   if((point-viewer).sqrMagnitude>115*115)return;
   EnsureVehicleImpactFx();
   float scale=Mathf.Clamp(bounds.size.z*.22f,.54f,1.55f);
   var rotation=Quaternion.Inverse(transform.rotation)*target.rotation;
   PlayRoadExplosion(point-Vector3.up*(1.25f*scale),scale);
   var extent=bounds.extents;
   if(bounds.size.z>3)
   {
    for(int side=-1;side<=1;side+=2)
    {
     var end=point+rotation*new Vector3(0,0,side*extent.z*.65f);
     Emit(blastFx,end,Vector3.up*.8f,4.4f*scale,.64f,new Color(1,.84f,.62f,1));
     Emit(smokeFx,end,Vector3.up*1.5f,3.4f*scale,2.1f,new Color(.28f,.26f,.25f,.88f));
    }
   }
   if(vehicleShards==null)return;
   Color paint=target.TryGetComponent<MeshyCarVariants>(out var car)?car.CurrentPaint:
    airborne?new Color(.8f,.24f,.15f):new Color(.16f,.42f,.84f);
   int count=Mathf.RoundToInt(Mathf.Lerp(36,84,Mathf.InverseLerp(1.5f,7,bounds.size.z)));
   for(int i=0;i<count;i++)
   {
    bool panel=i<count/5,glass=!panel&&i%3==0;
    Vector3 offset=new Vector3(Random.Range(-1f,1f)*extent.x,Random.Range(-.65f,.9f)*extent.y,Random.Range(-1f,1f)*extent.z);
    if(panel)offset.x=(i%2==0?-1:1)*extent.x;
    Vector3 outward=rotation*new Vector3(offset.x/Mathf.Max(.1f,extent.x),.36f,offset.z/Mathf.Max(.1f,extent.z));
    Vector3 velocity=(outward+Random.insideUnitSphere*.35f).normalized*Random.Range(4.5f,14f)*Mathf.Sqrt(scale)+
     Vector3.up*Random.Range(1.1f,4.8f)+Vector3.ClampMagnitude(motion,16)*.3f;
    float span=panel?Random.Range(.4f,.85f)*scale:Random.Range(.08f,.35f)*scale;
    Color tone=panel?Color.Lerp(paint,new Color(.15f,.12f,.11f),Random.Range(0,.48f)):
     glass?new Color(.53f,.71f,.79f):new Color(.24f,.22f,.21f);
    vehicleShards.Emit(new ParticleSystem.EmitParams{
     position=tankFxAnchor.InverseTransformPoint(transform.TransformPoint(point+rotation*offset)),
     velocity=tankFxAnchor.InverseTransformDirection(transform.TransformDirection(velocity)),
     startSize3D=new Vector3(span,span*(panel?.18f:.35f),span*(panel?.82f:.6f)),
     rotation3D=Random.insideUnitSphere*180,startColor=tone,startLifetime=Random.Range(.85f,1.75f)
    },1);
   }
  }

  void ShutdownVehicleImpactFx()
  {
   if(vehicleShardMesh!=null)Destroy(vehicleShardMesh);
   if(vehicleShardMaterial!=null)Destroy(vehicleShardMaterial);
  }
 }
}
