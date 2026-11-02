using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const float TurnBikeSpeedMultiplier=1.5f,TurnBikeWarningSeconds=.5f;
  public partial class TrafficVehicle
  {
   [System.NonSerialized] public int turnBikeGeneration=-1;
   [System.NonSerialized] public float turnBikeSwerveTimer;
   [System.NonSerialized] public bool turnBikeFromRear;
  }
  sealed class TurnBikeWave
  {public bool frontDone,rearDone;public TrafficVehicle rear;public int lane;public float warning=TurnBikeWarningSeconds;}
  readonly List<TurnBikeWave> turnBikeWaves=new List<TurnBikeWave>();
  System.Random turnBikeRandom;
  public int TurnRearBikeSpawns {get;private set;}
  public int TurnOncomingBikeSpawns {get;private set;}
  public int TurnBikeLaneChanges {get;private set;}
  public int PendingTurnBikeWaves=>turnBikeWaves.Count;
  bool HasTurnBike(TrafficVehicle v)=>v.active&&v.turnBikeGeneration==v.spawnGeneration;
  bool TurnBikeReserved(TrafficVehicle v){foreach(var wave in turnBikeWaves)if(wave.rear==v)return true;return false;}
  public bool TurnBikeRearWarningVisible(int lane)
  {if(!turnExitTrafficEnabled||IsTurning||ServiceStopped||State!=RideState.Riding)return false;foreach(var wave in turnBikeWaves)if(wave.rear!=null&&wave.lane==lane)return true;return false;}
  public float TurnBikeRearWarningRemaining(int lane)
  {foreach(var wave in turnBikeWaves)if(wave.rear!=null&&wave.lane==lane)return wave.warning;return -1;}
  void ResetTurnBikes()
  {
   turnBikeWaves.Clear();turnBikeRandom=new System.Random(RunSeed^0x21b111);
   TurnRearBikeSpawns=TurnOncomingBikeSpawns=TurnBikeLaneChanges=0;
   foreach(var v in vehicles){v.turnBikeGeneration=-1;v.turnBikeSwerveTimer=0;v.turnBikeFromRear=false;}
  }
  void QueueTurnBikes()
  {
   if(!turnExitTrafficEnabled||State!=RideState.Riding)return;
   var wave=new TurnBikeWave();turnBikeWaves.Add(wave);ReserveTurnBikeRear(wave);
   wave.frontDone=SpawnTurnBike(false,-1,null);
  }
  TrafficVehicle FindTurnBikeCandidate()
  {
   TrafficVehicle chosen=null;int count=0;
   foreach(var v in vehicles)if(!v.active&&!v.eventOnly&&!RearExitReserved(v)&&v.halfWidth<.7f&&v.model.GetComponent<UrbanRiderMotion>()!=null&&turnBikeRandom.Next(++count)==0)chosen=v;
   return chosen;
  }
  float TurnBikeRearZ(TrafficVehicle v)=>Mathf.Min(-24,rideCamera.transform.localPosition.z-v.halfLength-6);
  int TurnBikeSafeLane(int lane,float z)
  {var p=new Vector3(LaneX[lane],0,z);return (lane==0||lane==3)&&riversEnabled&&(RiverZoneAt(p)||OuterLaneNearBridge(p,lane))?(lane==0?1:2):lane;}
  void ReserveTurnBikeRear(TurnBikeWave wave)
  {
   if(wave.rearDone||wave.rear!=null)return;
   var v=FindTurnBikeCandidate();if(v==null)return;
   int first=turnBikeRandom.Next(2,4);
   for(int attempt=0;attempt<2;attempt++)
   {
    int lane=TurnBikeSafeLane(2+(first-2+attempt)%2,TurnBikeRearZ(v));
    if(rearExitReserved[lane-2]!=null||TurnBikeRearWarningVisible(lane))continue;
    wave.rear=v;wave.lane=lane;wave.warning=TurnBikeWarningSeconds;return;
   }
  }
  void TickTurnBikes(float dt)
  {
   if(!turnExitTrafficEnabled){turnBikeWaves.Clear();return;}
   if(dt<=0||State!=RideState.Riding)return;
   foreach(var wave in turnBikeWaves)
   {
    if(IsTurning||ServiceStopped){wave.warning=TurnBikeWarningSeconds;continue;}
    bool alreadyWarned=wave.rear!=null;ReserveTurnBikeRear(wave);
    if(!wave.frontDone)wave.frontDone=SpawnTurnBike(false,-1,null);
    if(wave.rear!=null)
    {
     if(!alreadyWarned)continue;
     int lane=TurnBikeSafeLane(wave.lane,TurnBikeRearZ(wave.rear));
     if(lane!=wave.lane){wave.lane=lane;wave.warning=TurnBikeWarningSeconds;continue;}
     wave.warning=Mathf.Max(0,wave.warning-dt);
     if(wave.warning<=.000001f)
     {
      var reserved=wave.rear;wave.rear=null;
      if(SpawnTurnBike(true,wave.lane,reserved))wave.rearDone=true;else wave.rear=reserved;
     }
    }
   }
   turnBikeWaves.RemoveAll(wave=>wave.frontDone&&wave.rearDone);
  }
  bool TurnBikeSpace(TrafficVehicle candidate,Vector3 p)
  {
   if(Mathf.Abs(p.x-rider.localPosition.x)<candidate.halfWidth+PlayerHalfWidth+.5f&&Mathf.Abs(p.z-rider.localPosition.z)<candidate.halfLength+PlayerHalfLength+3)return false;
   foreach(var other in vehicles)if(other.active)
   {var half=VehicleFootprint(other);var d=p-other.model.localPosition;if(Mathf.Abs(d.x)<candidate.halfWidth+half.x+.6f&&Mathf.Abs(d.z)<candidate.halfLength+half.y+3)return false;}
   if(IsChased&&policeBike!=null&&policeBike.gameObject.activeSelf){var d=p-policeBike.localPosition;if(Mathf.Abs(d.x)<candidate.halfWidth+.8f&&Mathf.Abs(d.z)<candidate.halfLength+3)return false;}
   foreach(var d in dinosaurs)if(d.active&&Mathf.Abs(p.x-d.root.localPosition.x)<candidate.halfWidth+DinosaurWidth*.5f+.5f&&Mathf.Abs(p.z-d.root.localPosition.z)<candidate.halfLength+7)return false;
   return true;
  }
  bool SpawnTurnBike(bool rear,int fixedLane,TrafficVehicle reserved)
  {
   var candidate=rear?reserved:FindTurnBikeCandidate();if(candidate==null||candidate.active)return false;
   for(int attempt=0;attempt<8;attempt++)
   {
    float z=rear?TurnBikeRearZ(candidate)-attempt*6:104+attempt*7;
    int lane=TurnBikeSafeLane(rear?fixedLane:turnBikeRandom.Next(2),z);
    if(rear&&lane!=fixedLane)continue;
    if(!TurnBikeSpace(candidate,new Vector3(LaneX[lane],0,z)))continue;
    var v=SpawnTraffic(lane,z,vehicles.IndexOf(candidate));if(v==null)return false;
    v.turnBikeGeneration=v.spawnGeneration;v.turnBikeFromRear=rear;v.turnBikeSwerveTimer=.15f+(float)turnBikeRandom.NextDouble()*.2f;
    v.speed=v.baseTrafficSpeed=(rear?1:-1)*Speed*TurnBikeSpeedMultiplier;
    v.hasTurned=true;v.brakeRemaining=0;v.enteringFromBehind=rear;v.rearEntryBoost=0;
    if(rear)TurnRearBikeSpawns++;else TurnOncomingBikeSpawns++;return true;
   }
   return false;
  }
  void TickTurnBikeSteering(TrafficVehicle v,float dt)
  {
   v.turnBikeSwerveTimer-=dt;if(v.changingLane||v.turnBikeSwerveTimer>0||dt<=0)return;
   int lane=v.baseTrafficSpeed>0?(v.lane==2?3:2):(v.lane==0?1:0);
   if(OuterLaneNearBridge(v.model.localPosition,lane)){v.turnBikeSwerveTimer=.2f;return;}
   if(BeginSuddenLaneChange(v,lane)){TurnBikeLaneChanges++;v.turnBikeSwerveTimer=1.4f+(float)turnBikeRandom.NextDouble()*.6f;}
  }
 }
}
