using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public bool IsPlayerCar=>rider!=null&&rider.GetComponent<PlayerCarVisual>()!=null;
  public float PlayerHalfWidth=>IsPlayerCar?rider.GetComponent<PlayerCarVisual>().size.x*.5f:.33f;
  public float PlayerHalfLength=>IsPlayerCar?rider.GetComponent<PlayerCarVisual>().size.z*.5f:.91f;
  public float PlayerHeight=>IsPlayerCar?rider.GetComponent<PlayerCarVisual>().size.y:2.15f;
  float PlayerRearAxle=>IsPlayerCar?1.1643f:.73f;
  float PlayerFrontAxle=>IsPlayerCar?1.2105f:.90f;
  string PlayerAction=>IsPlayerCar?"HÚC":"ĐẠP";
  public string LastRamContact {get;private set;}="";

  // A deliberate side ram is resolved by the same swept body contact as crashes.
  // Frontal contacts, other traffic and scenery still enter the ordinary crash flow.
  bool TryResolvePlayerRam(TrafficVehicle hit,Vector2 normal,float at)
  {
   if(IsPlayerCar)LastRamContact="phase="+assistedKick+",same="+(hit==kickTarget)+",valid="+KickTargetValid(hit)+",active="+hit.active+",knocked="+hit.knocked+",wall="+hit.wallCrashed+",width="+hit.halfWidth+",y="+hit.model.localPosition.y+"/"+rider.localPosition.y+",generation="+(hit==kickTarget&&hit.spawnGeneration==kickTargetSpawn);
   if(!IsPlayerCar||!KickSwingActive||kickHit||hit!=kickTarget||!KickTargetValid(hit)||hit.spawnGeneration!=kickTargetSpawn)return false;
   Vector3 right=KickRoadFrame*Vector3.right;
   LastRamContact+=",normal="+Vector2.Dot(normal,Flat(right));
   // At a junction the bike's front face can touch the car's side panel at an
   // oblique angle; retain the front/rear exclusion below instead of demanding
   // nearly parallel vehicle headings.
   if(Mathf.Abs(Vector2.Dot(normal,Flat(right)))<.40f)return false;
   Vector3 p=Vector3.Lerp(riderBody.previousPosition,rider.localPosition,at);
   Vector3 target=Vector3.Lerp(hit.previousPosition,hit.model.localPosition,at);
   Vector3 delta=Quaternion.Inverse(KickRoadFrame)*(target-p);
   LastRamContact+=",delta="+delta+",side="+kickDirection+",at="+at;
   if(Mathf.Sign(delta.x)!=kickDirection||Mathf.Abs(delta.z)>PlayerHalfLength-.25f)return false;
   rider.localPosition=p;hit.model.localPosition=target;
   LastKickCentreDistance=delta.magnitude;LastKickFootGap=0;
   LastKickFootPoint=p+right*kickDirection*PlayerHalfWidth+Vector3.up*.8f;
   ApplyAssistedKickImpact();
   // Separate the real panels at contact, retaining the return-to-lane offset.
   // Resolve along the contact face, which is oblique for crossing/oncoming
   // bikes in a curve. A sideways-only offset leaves the two boxes touching
   // and falsely blocks every return path on the next control step.
   Vector3 separation=-new Vector3(normal.x,0,normal.y)*.10f;
   rider.localPosition+=separation;kickAppliedOffset+=separation;
   return true;
  }
 }
}
