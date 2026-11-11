using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  ContactFireVisual contactFire;
  void PrewarmContactFire()
  {
   if(contactFire==null&&Application.isPlaying&&stormFireTemplate!=null)
   {contactFire=gameObject.AddComponent<ContactFireVisual>();contactFire.Initialize(this,stormFireTemplate);}
   if(contactFire!=null)contactFire.Prewarm(4);
  }
  // Called only at confirmed body contacts, never by a generic impulse (kicks,
  // dinosaur attacks, lightning and explosions have their own effects).
  void ShowVehicleContact(TrafficVehicle a,TrafficVehicle b,Vector2 normal,float closing,float at=1)
  {
   if(a==null||b==null||a.riverFalling||b.riverFalling)return;
   Vector3 pa=Vector3.Lerp(a.previousPosition,a.model.localPosition,at),pb=Vector3.Lerp(b.previousPosition,b.model.localPosition,at);
   Vector3 faceA=VehicleContactFace(a,pa,pb,normal),faceB=VehicleContactFace(b,pb,pa,-normal);
   EmitVehicleImpact((faceA+faceB)*.5f,new Vector3(normal.x,0,normal.y),closing);
   ApplyVehicleContactDamage(a,b,pa,faceA,normal,closing);
   ApplyVehicleContactDamage(b,a,pb,faceB,-normal,closing);
  }
  void ApplyVehicleContactDamage(TrafficVehicle body,TrafficVehicle other,Vector3 centre,Vector3 point,Vector2 face,float closing)
  {
   if(body.model==rider&&IsMotorcycleContact(other))
   {
    Quaternion inverse=Quaternion.Inverse(rider.localRotation);
    Vector3 outward=inverse*new Vector3(face.x,0,face.y);
    // Align the deformation with the same panel chosen for the contact point.
    outward=Mathf.Abs(outward.x)>Mathf.Abs(outward.z)?Vector3.right*Mathf.Sign(outward.x):Vector3.forward*Mathf.Sign(outward.z);
    rider.GetComponent<PlayerCarVisual>()?.DentBody(inverse*(point-centre),outward,closing);
    return;
   }
   IgniteVehicleContact(body,centre,point);
  }
  void IgniteVehicleContact(TrafficVehicle body,Vector3 centre,Vector3 point)
  {
   if(body.armored||body.model==policeBike)return;
   if(contactFire==null&&Application.isPlaying&&stormFireTemplate!=null)
   {contactFire=gameObject.AddComponent<ContactFireVisual>();contactFire.Initialize(this,stormFireTemplate);}
   contactFire?.Ignite(body,Quaternion.Inverse(body.model.localRotation)*(point-centre));
  }
  Vector3 VehicleContactFace(TrafficVehicle body,Vector3 centre,Vector3 other,Vector2 face)
  {
   // Find the struck face in the yaw footprint rather than using the midpoint
   // between centres (which puts fire metres away from a long truck's bumper).
   Quaternion yaw=Quaternion.Euler(0,body.model.localEulerAngles.y,0);
   Vector3 toward=Quaternion.Inverse(yaw)*(other-centre),n=Quaternion.Inverse(yaw)*new Vector3(face.x,0,face.y);
   float width=BodyWidth(body),length=body.halfLength;
   var p=new Vector3(Mathf.Clamp(toward.x,-width,width),.55f,Mathf.Clamp(toward.z,-length,length));
   if(Mathf.Abs(n.x)>Mathf.Abs(n.z))p.x=Mathf.Sign(n.x)*width;else p.z=Mathf.Sign(n.z)*length;
   Vector3 road=centre+yaw*p;
   float height=body==riderBody?PlayerHeight:body.model.TryGetComponent<MeshyCarVariants>(out var car)?car.Height:body.halfWidth>.7f?1.9f:.9f;
   road.y=Mathf.Clamp((centre.y+other.y)*.5f+.5f,centre.y+.12f,centre.y+Mathf.Max(.15f,height*.6f));
   return road;
  }
  void RenderContactFire(){if(contactFire!=null)contactFire.Render();}
  void ResetContactFire(){if(contactFire!=null)contactFire.Clear();}
 }
}
