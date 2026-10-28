using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const float TurnTankDelay=2;
  sealed class TurnHazardWave {public bool dinosaurSpawned,tankWanted;public float remaining;}
  readonly List<TurnHazardWave> turnHazardWaves=new List<TurnHazardWave>();
  void ResetTurnHazards(){turnHazardWaves.Clear();}
  float TurnHazardJunction()
  {
   float z=NextJunctionDistance;
   if(float.IsNaN(z)||float.IsInfinity(z))return -1;
   // At high speed the junction used by the dinosaur can already be behind
   // the player two seconds later. Always choose a physical junction ahead.
   while(z<=0)z+=VietnamStreet.Length;
   return z;
  }
  void QueueTurnHazards()
  {
   if(State!=RideState.Riding||!dinosaursEnabled||dinosaurPrefab==null)return;
   var wave=new TurnHazardWave{tankWanted=tankBarrageEnabled,remaining=TurnTankDelay};
   turnHazardWaves.Add(wave);TryTurnDinosaur(wave);
  }
  bool TryTurnDinosaur(TurnHazardWave wave)
  {
   float z=TurnHazardJunction();if(z<=0||SpawnDinosaurAt(z)==null)return false;
   wave.dinosaurSpawned=true;wave.remaining=TurnTankDelay;return true;
  }
  void TickTurnHazards(float dt)
  {
   if(State!=RideState.Riding||dt<=0||ServiceStopped)return;
   if(!dinosaursEnabled){ResetTurnHazards();return;}
   for(int i=0;i<turnHazardWaves.Count;)
   {
    var wave=turnHazardWaves[i];
    if(wave.dinosaurSpawned)wave.remaining=Mathf.Max(0,wave.remaining-dt);
    if(!IsTurning)
    {
     if(!wave.dinosaurSpawned)TryTurnDinosaur(wave);
     if(wave.dinosaurSpawned)
     {
      if(!wave.tankWanted||!tankBarrageEnabled){turnHazardWaves.RemoveAt(i);continue;}
      float z=TurnHazardJunction();
      if(wave.remaining<=.00001f&&z>0&&SpawnTankAt(z)!=null){turnHazardWaves.RemoveAt(i);continue;}
     }
    }
    i++;
   }
  }
  bool TurnTankSpace(int lane,float z,float halfWidth,float halfLength)
  {
   // SpawnTraffic's bridge entry moves the outer oncoming lane to lane 1.
   // Check the final position, rather than the outer lane it will leave.
   var point=new Vector3(LaneX[lane],0,z);
   if(riversEnabled&&lane==0&&(RiverZoneAt(point)||BridgeAhead(point,false,30)))lane=1;
   float x=LaneX[lane];
   if(Mathf.Abs(x-rider.localPosition.x)<halfWidth+riderBody.halfWidth+.5f&&Mathf.Abs(z-rider.localPosition.z)<halfLength+riderBody.halfLength+2)return false;
   foreach(var other in vehicles)
    if(other.active&&Mathf.Abs(x-other.model.localPosition.x)<halfWidth+other.halfWidth+.5f&&Mathf.Abs(z-other.model.localPosition.z)<halfLength+other.halfLength+2)return false;
   foreach(var d in dinosaurs)
    if(d.active&&Mathf.Abs(x-d.root.localPosition.x)<halfWidth+DinosaurWidth*.5f+.5f&&Mathf.Abs(z-d.root.localPosition.z)<halfLength+4.35f+2)return false;
   return true;
  }
 }
}
