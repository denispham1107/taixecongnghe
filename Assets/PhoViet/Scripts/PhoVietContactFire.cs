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
  void BurnVehicleContact(TrafficVehicle a,TrafficVehicle b,Vector2 normal,float at=1)
  {
   if(a==null||b==null||a.riverFalling||b.riverFalling)return;
   if(contactFire==null&&Application.isPlaying&&stormFireTemplate!=null)
   {contactFire=gameObject.AddComponent<ContactFireVisual>();contactFire.Initialize(this,stormFireTemplate);}
   if(contactFire==null)return;
   Vector3 pa=Vector3.Lerp(a.previousPosition,a.model.localPosition,at),pb=Vector3.Lerp(b.previousPosition,b.model.localPosition,at);
   BurnContactSide(a,pa,pb,normal);BurnContactSide(b,pb,pa,-normal);
  }
  void BurnContactSide(TrafficVehicle body,Vector3 centre,Vector3 other,Vector2 face)
  {
   if(body.armored)return;
   Quaternion rotation=body.model.localRotation;
   // Find the struck face in the yaw footprint rather than using the midpoint
   // between centres (which puts fire metres away from a long truck's bumper).
   Quaternion yaw=Quaternion.Euler(0,body.model.localEulerAngles.y,0);
   Vector3 toward=Quaternion.Inverse(yaw)*(other-centre),n=Quaternion.Inverse(yaw)*new Vector3(face.x,0,face.y);
   float width=BodyWidth(body),length=body.halfLength;
   var p=new Vector3(Mathf.Clamp(toward.x,-width,width),.55f,Mathf.Clamp(toward.z,-length,length));
   if(Mathf.Abs(n.x)>Mathf.Abs(n.z))p.x=Mathf.Sign(n.x)*width;else p.z=Mathf.Sign(n.z)*length;
   // Convert the contact at impact into model space, including the current fall
   // roll/pitch. The attachment then follows all subsequent motion/rebases.
   Vector3 road=centre+yaw*p;
   float height=body==riderBody?PlayerHeight:body.model.TryGetComponent<MeshyCarVariants>(out var car)?car.Height:body.halfWidth>.7f?1.9f:.9f;
   road.y=Mathf.Clamp((centre.y+other.y)*.5f+.5f,centre.y+.12f,centre.y+Mathf.Max(.15f,height*.6f));
   Vector3 local=Quaternion.Inverse(rotation)*(road-centre);
   contactFire.Ignite(body,local);
  }
  void RenderContactFire(){if(contactFire!=null)contactFire.Render();}
  void ResetContactFire(){if(contactFire!=null)contactFire.Clear();}
 }
}
