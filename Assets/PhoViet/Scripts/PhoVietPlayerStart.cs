using UnityEngine;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  Vector3 playerStart, authoredGamePosition;
  bool playerStartCaptured;
  public Vector3 PlayerStartPosition=>playerStart;
  // The moving-road simulation uses z=0 for the player. Move its coordinate
  // frame to the authored start, rather than leaving the visible car ahead of
  // its collision body. Scene transforms remain the source of the start pose.
  void CapturePlayerStart()
  {
   if(playerStartCaptured||rider==null)return;
   authoredGamePosition=transform.localPosition;
   var car=rider.GetComponent<PlayerCarVisual>();
   playerStart=transform.InverseTransformPoint(car!=null&&car.visual!=null?car.visual.position:rider.position);
   if(car!=null&&car.visual!=null)car.visual.localPosition=Vector3.zero;
   playerStartCaptured=true;
  }
  void RestorePlayerStartFrame()
  {
   CapturePlayerStart();
   if(!playerStartCaptured)return;
   transform.localPosition=authoredGamePosition;
   transform.position+=transform.TransformVector(Vector3.forward*playerStart.z);
   CurrentLane=2;
   for(int i=0;i<LaneX.Length;i++)
    if(Mathf.Abs(playerStart.x-LaneX[i])<Mathf.Abs(playerStart.x-LaneX[CurrentLane]))CurrentLane=i;
  }
  int StartStreetIndex=>Mathf.FloorToInt((playerStart.z+10)/VietnamStreet.Length);
  float StartStreetPhase=>playerStart.z-StartStreetIndex*VietnamStreet.Length;
  void PlacePlayerAtStart()
  {
   if(rider==null)return;
   rider.localPosition=new Vector3(playerStart.x,playerStart.y,0);
   rider.localRotation=Quaternion.identity;lastX=playerStart.x;
   jumpHeight=jumpPreviousHeight=playerStart.y;
   // A start placed above the road falls onto the same road/roof surfaces as
   // every other airborne car; it does not redefine the ground height.
   jumpFlying=Mathf.Abs(playerStart.y)>.001f;jumpVelocity=0;
  }
#if UNITY_EDITOR
  // Called only for an intentional Transform edit in the Editor's Ready state.
  public void SetPlayerStartFromEditor(Vector3 worldPosition)
  {
   CapturePlayerStart();
   var p=transform.InverseTransformPoint(worldPosition);p.z+=playerStart.z;
   playerStart=p;ResetRide();
  }
#endif
 }
}
