using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public int PlayerTanksDestroyed{get;private set;}
  static bool PlayerTankTargetValid(TrafficVehicle v)=>v!=null&&v.active&&v.armored&&!v.tankDestroyed&&!v.riverFalling&&v.model!=null&&v.model.gameObject.activeInHierarchy;
  static float PlayerTankHeight(TrafficVehicle v){var car=v.model.GetComponent<MeshyCarVariants>();return car!=null?car.Height:2.4f;}
  static Vector3 PlayerTankAim(TrafficVehicle v)=>v.model.localPosition+Vector3.up*(PlayerTankHeight(v)*.5f);
  TrafficVehicle PlayerTankTarget(Vector3 origin,Vector3 forward)
  {
   TrafficVehicle best=null;float distance=160*160;
   foreach(var v in vehicles)if(PlayerTankTargetValid(v))
   {var d=PlayerTankAim(v)-origin;if(Vector3.Dot(d,forward)<=0||d.sqrMagnitude>=distance)continue;best=v;distance=d.sqrMagnitude;}
   return best;
  }
  static bool PlayerMissileTouchesTank(Vector3 from,Vector3 to,TrafficVehicle v,Vector3 scroll,out float at)
  {
   // Swept relative motion in the tank's oriented hull, including camera/road scroll.
   var inverse=Quaternion.Inverse(v.model.localRotation);float height=PlayerTankHeight(v);
   var centre=Vector3.up*(height*.5f);var a=inverse*(from-(v.previousPosition-scroll))-centre;var b=inverse*(to-v.model.localPosition)-centre;
   var half=new Vector3(v.halfWidth+.15f,height*.5f+.15f,v.halfLength+.15f);var delta=b-a;float enter=0,leave=1;
   for(int i=0;i<3;i++)
   {
    if(Mathf.Abs(delta[i])<1e-7f){if(Mathf.Abs(a[i])>half[i]){at=2;return false;}continue;}
    float p=(-half[i]-a[i])/delta[i],q=(half[i]-a[i])/delta[i];enter=Mathf.Max(enter,Mathf.Min(p,q));leave=Mathf.Min(leave,Mathf.Max(p,q));
    if(enter>leave){at=2;return false;}
   }
   at=enter;return enter<=leave;
  }
  void DestroyPlayerTank(TrafficVehicle v,Vector3 impact)
  {
   if(!PlayerTankTargetValid(v))return;
   v.tankDestroyed=true;v.knocked=true;v.physicalCrash=false;v.recovery=null;v.speed=v.baseTrafficSpeed=v.crossSpeed=0;v.tankFireTimer=float.PositiveInfinity;
   v.knockVelocity=v.motionVelocity=v.routeVelocity=v.angularVelocity=Vector3.zero;v.changingLane=v.turning=v.roadTurning=false;
   if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(false);
   // Keep active, armored and the full collision hull: a burnt tank is still a solid obstacle.
   PlayerTanksDestroyed++;EnsureThunderstorm();stormVisual?.Burn(v.model,v,2.8f);AirAttackBurst(impact);
   notice="XE TĂNG BỊ HẠ · TRÁNH XÁC XE";noticeTimer=1.8f;
  }
 }
}
