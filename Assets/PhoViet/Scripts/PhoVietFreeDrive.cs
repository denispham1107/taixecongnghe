using UnityEngine;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  const float DriveStickDeadZone=.12f,StreetDriveEdge=6.45f,JunctionDriveEdge=6.35f;
  const float DriveTightCornerSpeed=5.5f,DriveCornerDeceleration=14f,DriveYawAcceleration=400f;
  float freeDriveHeading,freeDriveSpeed,driveScrollSpeed,driveRouteOffset,unscoredDriveMileage;
  float driveYawRate;
  float driveSpeedBeforeRoadBoundary;
  bool driveNeutralBraking;
  Vector2 driveKeys,driveInputAtTurn;
  Quaternion driveViewHeading=Quaternion.identity;
  Quaternion driveTurnInputHeading=Quaternion.identity;

  float RoadScrollSpeed=>State==RideState.Riding&&IsPlayerCar?driveScrollSpeed:Speed;
  float ChaseTrailDistance=>Distance+crashTravelDistance+unscoredDriveMileage;

  void ResetFreeDrive()
  {
   driveRouteOffset=unscoredDriveMileage=0;
   ResetDriveInput();
  }

  void ResetDriveInput()
  {
   freeDriveHeading=freeDriveSpeed=driveScrollSpeed=driveYawRate=driveSpeedBeforeRoadBoundary=0;
   driveNeutralBraking=false;
   driveKeys=driveInputAtTurn=Vector2.zero;
   driveViewHeading=Quaternion.identity;
   CancelFootPointer();
  }

  void SelectNearestDrivingLane(float x)
  {
   int lane=0;
   for(int i=1;i<LaneX.Length;i++)
    if(Mathf.Abs(LaneX[i]-x)<Mathf.Abs(LaneX[lane]-x))lane=i;
   CurrentLane=lane;
  }

  // Changing the same joystick angle at speed needs a wider arc.
  float TurnDrivingCar(float aim,bool hasSteering,float dt)
  {
   float error=Mathf.DeltaAngle(freeDriveHeading,aim);
   float speedRatio=Mathf.Clamp01(Speed/VehicleSpeedLimit);
   float rateLimit=Mathf.Lerp(138f,29f,Mathf.Sqrt(speedRatio));
   float targetRate=hasSteering?Mathf.Clamp(error*8,-rateLimit,rateLimit):0;
   float oldRate=driveYawRate;
   driveYawRate=Mathf.MoveTowards(driveYawRate,targetRate,DriveYawAcceleration*dt);
   float arc=(oldRate+driveYawRate)*.5f*dt;
   if(arc*error>0&&Mathf.Abs(arc)>=Mathf.Abs(error))
   {
    arc=error;driveYawRate=0;
   }
   freeDriveHeading=Mathf.DeltaAngle(0,freeDriveHeading+arc);
   return freeDriveHeading-arc*.5f;
  }

  void FreeDriveStep(float baseX,float dt,out float x,out float scroll,out float travelled)
  {
   Vector2 input=footPointer!=int.MinValue?footStick:driveKeys;
   float strength=input.magnitude;
   driveViewHeading=EaseStreetView(driveViewHeading,dt);
   if(ServiceStopped||JobOffersOpen)
   {
    driveInputAtTurn=Vector2.zero;
    freeDriveSpeed=Speed=driveYawRate=driveSpeedBeforeRoadBoundary=0;
    driveNeutralBraking=false;
    x=baseX;scroll=travelled=0;
    laneAccel=laneVelocity=0;
    return;
   }

   bool neutral=strength<=DriveStickDeadZone;
   float aim=freeDriveHeading,requested=0;
   if(neutral)driveInputAtTurn=Vector2.zero;
   else
   {
    // Keep a held direction steady while the camera swings onto the new street.
    if((input-driveInputAtTurn).sqrMagnitude>.12f*.12f)driveInputAtTurn=Vector2.zero;
    Quaternion inputHeading=driveInputAtTurn==Vector2.zero?driveViewHeading:driveTurnInputHeading;
    Vector3 move=inputHeading*new Vector3(input.x,0,input.y);
    if(Mathf.Abs(move.x)>.2f)CorrectCraterSteering(move.x<0?-1:1);
    aim=Mathf.Atan2(move.x,move.z)*Mathf.Rad2Deg;
    if(!DriveBraking)
     requested=JobAcceptanceSpeedLimit(WorkSpeedLimit(VehicleSpeedLimit*Mathf.InverseLerp(DriveStickDeadZone,1,strength)))*(1-bikeImpactSlowdown);
   }

   float motionHeading=TurnDrivingCar(aim,!neutral&&!DriveBraking,dt);
   float turnError=Mathf.Abs(Mathf.DeltaAngle(freeDriveHeading,aim));
   float corner=Mathf.SmoothStep(0,1,Mathf.InverseLerp(12f,65f,turnError));
   float cornerSpeed=Mathf.Lerp(VehicleSpeedLimit,DriveTightCornerSpeed,corner);
   // A car pointing away from the input must turn before it accelerates in the wrong direction.
   cornerSpeed*=Mathf.InverseLerp(112f,60f,turnError);
   bool speedLimitedByCorner=cornerSpeed<requested;
   requested=Mathf.Min(requested,cornerSpeed);
   float deceleration=speedLimitedByCorner?DriveCornerDeceleration:BrakeDeceleration;
   float acceleration=requested<freeDriveSpeed?deceleration:LaunchAcceleration;
   if(neutral)freeDriveSpeed=Mathf.Min(freeDriveSpeed,Speed);
   float previousSpeed=freeDriveSpeed;
   freeDriveSpeed=Mathf.MoveTowards(freeDriveSpeed,requested,dt*acceleration);
   driveSpeedBeforeRoadBoundary=freeDriveSpeed;

   float radians=motionHeading*Mathf.Deg2Rad;
   // Use only the moving fraction of the last braking step, with no overshoot.
   float stepDistance=neutral?(previousSpeed+freeDriveSpeed)*.5f*Mathf.Min(dt,previousSpeed/BrakeDeceleration):freeDriveSpeed*dt;
   Vector2 step=new Vector2(Mathf.Sin(radians),Mathf.Cos(radians))*stepDistance;
   // Allow the same side-by-side passage as walking through the junction apron.
   float junction=NextJunctionDistance;
   bool inJunction=RoutesReady&&Mathf.Abs(junction)<=JunctionDriveEdge;
   float limit=inJunction?13:Mathf.Max(StreetDriveEdge,Mathf.Abs(baseX));
   x=Mathf.Clamp(baseX+step.x,-limit,limit);
   scroll=Mathf.Abs(x)>StreetDriveEdge&&inJunction?Mathf.Clamp(step.y,junction-JunctionDriveEdge,junction+JunctionDriveEdge):step.y;
   float lateral=(x-baseX)/dt;
   laneAccel=(lateral-laneVelocity)/dt;laneVelocity=lateral;
   float pathLength=new Vector2(x-baseX,scroll).magnitude;
   Speed=neutral&&stepDistance>0?freeDriveSpeed*Mathf.Clamp01(pathLength/stepDistance):pathLength/dt;
   // A fully blocked move must not bank speed. A diagonal can still slide along the edge.
   if(pathLength<.00001f)freeDriveSpeed=0;
   driveNeutralBraking=neutral&&Speed>0;
   travelled=scroll<-.0001f?0:pathLength;
   unscoredDriveMileage+=pathLength-travelled;
   SelectNearestDrivingLane(x);
  }

  bool ContinueFreeDriveStreet()
  {
   if(!IsPlayerCar||!RoutesReady||Speed<=0||Mathf.Abs(rider.localPosition.x)<10.9f)return false;
   Vector3 point=rider.localPosition;
   int side=point.x<0?-1:1,oldMain=mainRow,unused=side<0?rightRow:leftRow;
   int selected=side<0?leftRow:rightRow;
   var row=roadRows[selected].root;
   Quaternion heading=row.localRotation,inverse=Quaternion.Inverse(heading);
   Vector3 forward=heading*Vector3.forward;
   if(!row.gameObject.activeInHierarchy||Vector3.Dot(point-row.localPosition,forward)<.9f||
      Mathf.Abs((inverse*(point-row.localPosition)).x)>6.45f)return false;
   Vector3 origin=row.localPosition+forward*Vector3.Dot(point-row.localPosition,forward);
   origin.y=0;
   ClearShells();
   rider.GetComponent<CarJumpJetVisual>()?.Clear();
   if(IsChased&&!routePolice){routePolice=true;policeRouteAt=ChaseTrailDistance-PoliceDistance;}
   RebaseStreetActors(inverse,origin,side*90);
   RebaseFootActor(rider,inverse,origin);
   riderRoadVelocity=inverse*riderRoadVelocity;
   craterAppliedOffset=inverse*craterAppliedOffset;bikeAppliedOffset=inverse*bikeAppliedOffset;
   driveTurnInputHeading=inverse*(driveInputAtTurn==Vector2.zero?driveViewHeading:driveTurnInputHeading);
   driveInputAtTurn=footPointer!=int.MinValue?footStick:driveKeys;
   driveViewHeading=inverse*driveViewHeading;
   freeDriveHeading=Mathf.DeltaAngle(side*90,freeDriveHeading);
   steerYaw=Mathf.DeltaAngle(side*90,steerYaw);
   laneVelocity=laneAccel=0;
   SelectNearestDrivingLane(rider.localPosition.x);
   lastX=rider.localPosition.x;oldRiderZ=rider.localPosition.z;
   UseStreetRow(selected);
   ExtendDrivingStreetBehind();
   FinishStreetTransition(side,oldMain,unused);
   return true;
  }

  // A branch starts at the junction. Once it becomes our street, load a
  // previous tile so the car can turn around and keep driving in that direction.
  void ExtendDrivingStreetBehind()
  {
   if(blocks.Count<2)return;
   Transform farthest=blocks[0];
   foreach(var tile in blocks)
   {
    if(tile.localPosition.z<=-VietnamStreet.Length*.5f)return;
    if(tile.localPosition.z>farthest.localPosition.z)farthest=tile;
   }
   farthest.localPosition-=Vector3.forward*(VietnamStreet.Length*blocks.Count);
   var river=RiverOf(farthest);
   if(river!=null){river.SetBridge(false);river.streetRun=0;}
   RefreshEnergyTile(farthest);
   RefreshMissilePickupTile(farthest);
   tileCullPending=true;
  }
 }
}
