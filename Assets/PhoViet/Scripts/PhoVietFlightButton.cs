using UnityEngine;
namespace PhoViet
{
 // 21.140: one press lifts the car to flight height, another press brings it down, instead of two precise swipes.
 public partial class PhoVietGame
 {
  public bool FlightEngaged=>energyHover||energyHoverRequested;
  public bool FlightButtonReady=>FlightEngaged||jumpFlying&&Energy>0||Energy>=JumpEnergyCost;
  const float FlightClimbHoldDelay=.25f,FlightClimbSpeed=3.1f;
  bool flightButtonHeld,flightButtonCanClimb;
  float flightButtonHeldTime;

  bool BeginFlightButtonPress(Vector2 position)
  {
   if(State!=RideState.Riding||ServiceStopped||JobOffersOpen||!FlightButtonAt(position))return false;
   lastTapTime=-100;
   flightButtonHeld=true;flightButtonCanClimb=energyHover;flightButtonHeldTime=0;
   return true;
  }
  void EndFlightButtonPress(Vector2 position,bool released)
  {
   bool toggle=flightButtonHeld&&released&&FlightButtonAt(position)&&
    (!flightButtonCanClimb||flightButtonHeldTime<FlightClimbHoldDelay);
   CancelFlightButtonPress();
   if(toggle)TryToggleFlightButton();
  }
  void CancelFlightButtonPress()
  {
   flightButtonHeld=flightButtonCanClimb=false;flightButtonHeldTime=0;
  }
  void AdvanceFlightClimb(float dt)
  {
   if(!flightButtonHeld||!flightButtonCanClimb)return;
   float previous=flightButtonHeldTime;
   flightButtonHeldTime+=dt;
   float climbTime=Mathf.Min(dt,Mathf.Max(0,flightButtonHeldTime-Mathf.Max(previous,FlightClimbHoldDelay)));
   jumpHeight=Mathf.MoveTowards(jumpHeight,jumpBaseHeight+MaximumFlightHeightAboveSupport,FlightClimbSpeed*climbTime);
  }
  public Rect FlightButtonPixels()=>FlightButtonLayout(GameGuiArea(),Screen.width<Screen.height,browserPixelRatio);
  // Stacked above the missile button, in the same left-thumb column.
  public static Rect FlightButtonLayout(Rect area,bool narrow,float pixelRatio=1)
   =>DriveActionLayout(area,narrow,3,pixelRatio);
  bool FlightButtonAt(Vector2 position)
  {
   if(State!=RideState.Riding&&State!=RideState.Paused)return false;
   var r=FlightButtonPixels();var p=new Vector2(position.x,Screen.height-position.y);
   return (p-r.center).sqrMagnitude<=r.width*r.width*.25f;
  }
  public bool TryToggleFlightButton()
  {
   if(State!=RideState.Riding||ServiceStopped)return false;
   jumpQueued=0;
   if(energyHover){EndEnergyHover();return true;}
   if(energyHoverRequested){energyHoverRequested=false;return true;}
   if(jumpFlying)
   {
    return RequestEnergyHover();
   }
   if(jumpHeight>.06f&&roofVehicle==null)return false;
   if(!SpendEnergy(JumpEnergyCost))return false;
   // The button works straight after a landing: it does not wait for the swipe jump's cooldown.
   BeginJump();energyHoverRequested=true;return true;
  }
  void DrawFlightButton()
  {
   if(State!=RideState.Riding&&State!=RideState.Paused)return;
   var pixels=FlightButtonPixels();var area=GameGuiArea();
   var r=new Rect((pixels.x-area.x)/uiScale,(pixels.y-area.y)/uiScale,pixels.width/uiScale,pixels.height/uiScale);
   DrawArcadeControl(r,FlightEngaged?ArcadeControl.Land:ArcadeControl.Flight,FlightButtonReady?1:.55f);
  }
 }
}
