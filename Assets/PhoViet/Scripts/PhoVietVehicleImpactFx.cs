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

  void ShutdownVehicleImpactFx()
  {
   if(vehicleShardMesh!=null)Destroy(vehicleShardMesh);
   if(vehicleShardMaterial!=null)Destroy(vehicleShardMaterial);
  }
 }
}
