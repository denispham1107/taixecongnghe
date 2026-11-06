using UnityEngine;
namespace PhoViet
{
 // 21.140: one press lifts the car to flight height, another press brings it down, instead of two precise swipes.
 public partial class PhoVietGame
 {
  Texture2D flightUpIcon,flightLandIcon;
  public bool FlightEngaged=>energyHover||energyHoverRequested;
  public bool FlightButtonReady=>FlightEngaged||jumpFlying&&Energy>0||Energy>=JumpEnergyCost;
  public Rect FlightButtonPixels()=>FlightButtonLayout(GameGuiArea(),Screen.width<Screen.height,browserPixelRatio);
  // Stacked above the missile button, in the same left-thumb column.
  public static Rect FlightButtonLayout(Rect area,bool narrow,float pixelRatio=1)
  {
   var missile=PlayerMissileButtonLayout(area,narrow,pixelRatio);
   float scale=narrow?area.width/480f:LandscapeHudScale(area);
   return new Rect(missile.x,missile.y-missile.height-14*scale,missile.width,missile.height);
  }
  bool FlightButtonAt(Vector2 position)
  {
   if(State!=RideState.Riding&&State!=RideState.Paused)return false;
   var r=FlightButtonPixels();var p=new Vector2(position.x,Screen.height-position.y);
   return (p-r.center).sqrMagnitude<=r.width*r.width*.25f;
  }
  void EnsureFlightIcons()
  {
   if(flightUpIcon==null)flightUpIcon=Resources.Load<Texture2D>("PlayerFlight/PV_FlightUp");
   if(flightLandIcon==null)flightLandIcon=Resources.Load<Texture2D>("PlayerFlight/PV_FlightLand");
  }
  public bool TryToggleFlightButton()
  {
   if(State!=RideState.Riding||ServiceStopped)return false;
   jumpQueued=0;
   if(energyHover){energyHover=false;barrelRolling=false;jumpVelocity=0;return true;}
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
   EnsureFlightIcons();var pixels=FlightButtonPixels();var area=GameGuiArea();
   var r=new Rect((pixels.x-area.x)/uiScale,(pixels.y-area.y)/uiScale,pixels.width/uiScale,pixels.height/uiScale);
   bool engaged=FlightEngaged,ready=FlightButtonReady;
   Panel(r,engaged?new Color(1,.8f,.3f,.95f):new Color(.25f,.72f,1,ready?.8f:.38f),r.width*.5f);
   var inner=new Rect(r.x+2,r.y+2,r.width-4,r.height-4);Panel(inner,new Color(.025f,.12f,.22f,ready?.87f:.67f),inner.width*.5f);
   var icon=engaged?flightLandIcon:flightUpIcon;var old=GUI.color;GUI.color=ready?Color.white:new Color(.64f,.72f,.8f,.75f);
   if(icon!=null)GUI.DrawTexture(new Rect(r.x+r.width*.12f,r.y+r.height*.12f,r.width*.76f,r.height*.76f),icon,ScaleMode.ScaleToFit,true);GUI.color=old;
   bool click=GUI.Button(r,GUIContent.none,GUIStyle.none);
#if !UNITY_WEBGL || UNITY_EDITOR
   if(click&&Time.unscaledTime>=hudTouchSuppressUntil&&(Event.current.mousePosition-r.center).sqrMagnitude<=r.width*r.width*.25f)TryToggleFlightButton();
#endif
  }
 }
}
