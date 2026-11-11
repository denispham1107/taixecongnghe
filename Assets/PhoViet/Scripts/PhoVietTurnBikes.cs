using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const float TurnBikeSpeedMultiplier=1.2f,TurnBikeWarningSeconds=0f;
  public partial class TrafficVehicle
  {
   [System.NonSerialized] public int turnBikeGeneration=-1;
   [System.NonSerialized] public float turnBikeSwerveTimer;
   [System.NonSerialized] public bool turnBikeFromRear;
  }
  sealed class TurnBikeWave
  {public bool frontDone,rearDone;}
  readonly List<TurnBikeWave> turnBikeWaves=new List<TurnBikeWave>();
  System.Random turnBikeRandom;
  public int TurnRearBikeSpawns {get;private set;}
  public int TurnOncomingBikeSpawns {get;private set;}
  public int TurnBikeLaneChanges {get;private set;}
  public int PendingTurnBikeWaves=>turnBikeWaves.Count;
  bool HasTurnBike(TrafficVehicle v)=>v.active&&v.turnBikeGeneration==v.spawnGeneration;
  // Retain the old queries for callers; these two special bikes no longer reserve a warning.
  bool TurnBikeReserved(TrafficVehicle v)=>false;
  public bool TurnBikeRearWarningVisible(int lane)=>false;
  public float TurnBikeRearWarningRemaining(int lane)=>-1;
  void ResetTurnBikes()
  {
   turnBikeWaves.Clear();turnBikeRandom=new System.Random(RunSeed^0x21b111);
   TurnRearBikeSpawns=TurnOncomingBikeSpawns=TurnBikeLaneChanges=0;
   foreach(var v in vehicles){v.turnBikeGeneration=-1;v.turnBikeSwerveTimer=0;v.turnBikeFromRear=false;}
  }
  void QueueTurnBikes()
  {
   if(!turnExitTrafficEnabled||State!=RideState.Riding)return;
   var wave=new TurnBikeWave();turnBikeWaves.Add(wave);TryTurnBikePair(wave);
   turnBikeWaves.RemoveAll(item=>item.frontDone&&item.rearDone);
  }
  TrafficVehicle FindTurnBikeCandidate()
  {
   TrafficVehicle chosen=null;int count=0;
   foreach(var v in vehicles)if(!v.active&&!v.eventOnly&&!RearExitReserved(v)&&v.halfWidth<.7f&&v.model.GetComponent<UrbanRiderMotion>()!=null&&turnBikeRandom.Next(++count)==0)chosen=v;
   return chosen;
  }
  float TurnBikeRearZ(TrafficVehicle v)=>rider.localPosition.z-Mathf.Max(8,Mathf.Max(PlayerHalfLength+v.halfLength+3,Speed*.35f));
  int TurnBikeSafeLane(int lane,float z)
  {var p=new Vector3(LaneX[lane],0,z);return (lane==0||lane==3)&&riversEnabled&&(RiverZoneAt(p)||OuterLaneNearBridge(p,lane))?(lane==0?1:2):lane;}
  void TryTurnBikePair(TurnBikeWave wave)
  {
   if(!wave.rearDone)wave.rearDone=SpawnTurnBike(true,-1,null);
   // A blocked tank/dinosaur must claim its junction before the incoming motorcycle.
   if(!wave.frontDone&&turnHazardWaves.Count==0)wave.frontDone=SpawnTurnBike(false,-1,null);
  }
  void TickTurnBikes(float dt)
  {
   if(!turnExitTrafficEnabled){turnBikeWaves.Clear();return;}
   if(dt<=0||State!=RideState.Riding)return;
   foreach(var wave in turnBikeWaves)
   {
    if(IsTurning||ServiceStopped)continue;
    TryTurnBikePair(wave);
   }
   turnBikeWaves.RemoveAll(wave=>wave.frontDone&&wave.rearDone);
  }
  bool TurnBikeSpace(TrafficVehicle candidate,Vector3 p)
  {
   if(Mathf.Abs(p.x-rider.localPosition.x)<candidate.halfWidth+PlayerHalfWidth+.5f&&Mathf.Abs(p.z-rider.localPosition.z)<candidate.halfLength+PlayerHalfLength+3)return false;
   foreach(var other in vehicles)if(other.active)
   {var half=VehicleFootprint(other);var d=p-other.model.localPosition;if(Mathf.Abs(d.x)<candidate.halfWidth+half.x+.6f&&Mathf.Abs(d.z)<candidate.halfLength+half.y+3)return false;}
   if(IsChased&&policeBike!=null&&policeBike.gameObject.activeSelf){var d=p-policeBike.localPosition;if(Mathf.Abs(d.x)<candidate.halfWidth+PoliceHalfWidth+.3f&&Mathf.Abs(d.z)<candidate.halfLength+PoliceHalfLength+1.8f)return false;}
   foreach(var d in dinosaurs)if(d.active&&Mathf.Abs(p.x-d.root.localPosition.x)<candidate.halfWidth+DinosaurWidth*.5f+.5f&&Mathf.Abs(p.z-d.root.localPosition.z)<candidate.halfLength+7)return false;
   return true;
  }
  bool SpawnTurnBike(bool rear,int fixedLane,TrafficVehicle reserved)
  {
   var candidate=reserved??FindTurnBikeCandidate();if(candidate==null||candidate.active)return false;
   float near=rider.localPosition.z+Mathf.Max(18,Speed*.75f),far=rider.localPosition.z+104;
   if(!rear)
   {
    // Place it on the player's side of every approaching hazard, never behind a slow tank.
    foreach(var v in vehicles)if(v.active&&v.armored&&!v.riverFalling&&Mathf.Abs(v.model.localPosition.x)<7+VehicleFootprint(v).x&&v.model.localPosition.z>rider.localPosition.z)
     far=Mathf.Min(far,v.model.localPosition.z-VehicleFootprint(v).y-candidate.halfLength-6);
    foreach(var d in dinosaurs)if(d.active&&Mathf.Abs(d.root.localPosition.x)<7+DinosaurWidth*.5f&&d.root.localPosition.z>rider.localPosition.z)
     far=Mathf.Min(far,d.root.localPosition.z-candidate.halfLength-12);
    if(far<near)return false;
   }
   int first=turnBikeRandom.Next(2);
   if(rear&&Mathf.Abs(Mathf.Abs(LaneX[2]-rider.localPosition.x)-Mathf.Abs(LaneX[3]-rider.localPosition.x))>.001f)
    first=Mathf.Abs(LaneX[2]-rider.localPosition.x)<Mathf.Abs(LaneX[3]-rider.localPosition.x)?0:1;
   for(int attempt=0;attempt<8;attempt++)
   {
    float z=rear?TurnBikeRearZ(candidate)-attempt*3:far-attempt*4;if(!rear&&z<near)break;
    for(int side=0;side<2;side++)
    {
     int lane=TurnBikeSafeLane(fixedLane>=0?fixedLane:(rear?2:0)+(first+side)%2,z);
     if(!TurnBikeSpace(candidate,new Vector3(LaneX[lane],0,z)))continue;
     var v=SpawnTraffic(lane,z,vehicles.IndexOf(candidate));if(v==null)return false;
     v.turnBikeGeneration=v.spawnGeneration;v.turnBikeFromRear=rear;v.turnBikeSwerveTimer=.15f+(float)turnBikeRandom.NextDouble()*.2f;
     v.speed=v.baseTrafficSpeed=TurnBikeRoadSpeed(rear);
     v.hasTurned=true;v.brakeRemaining=0;v.enteringFromBehind=rear;v.rearEntryBoost=0;
     if(rear)TurnRearBikeSpawns++;else TurnOncomingBikeSpawns++;return true;
    }
   }
   return false;
  }
  float TurnBikeRoadSpeed(bool rear)=>(rear?1:-1)*TrafficPaceSpeed*TurnBikeSpeedMultiplier*OpeningTrafficPace;
  void TickTurnBikeSteering(TrafficVehicle v,float dt)
  {
   v.turnBikeSwerveTimer-=dt;if(v.changingLane||v.turnBikeSwerveTimer>0||dt<=0)return;
   int lane=v.baseTrafficSpeed>0?(v.lane==2?3:2):(v.lane==0?1:0);
   if(OuterLaneNearBridge(v.model.localPosition,lane)){v.turnBikeSwerveTimer=.2f;return;}
   if(BeginSuddenLaneChange(v,lane)){TurnBikeLaneChanges++;v.turnBikeSwerveTimer=1.4f+(float)turnBikeRandom.NextDouble()*.6f;}
  }
 }
}
