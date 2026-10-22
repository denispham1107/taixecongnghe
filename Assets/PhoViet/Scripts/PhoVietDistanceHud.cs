using UnityEngine;
using static PhoViet.StreetArt;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  float LandscapeBarWidth=>JobsAvailable?1000:640;
  float PortraitNoticeY=>JobsAvailable?PortraitNoticeTop:92;
  void DistancePortraitHud()
  {
   Panel(new Rect(10,PortraitBarTop,uiW-20,76),PortraitGlass,10);
   CompactHudText(CurrentStreet.ToUpperInvariant(),22,18,uiW-92,10,Gold);
   CompactHudText(Mathf.Max(0,Mathf.FloorToInt(Distance)).ToString("N0")+" m",22,35,154,24,Cream);
   CompactHudText(Mathf.RoundToInt(Speed*3.6f)+" km/h",22,65,140,10,Hex("B5CFC2"));
   Panel(new Rect(180,36,1,39),new Color(.8f,.95f,.9f,.18f),0);
   CompactHudText("KỶ LỤC",196,36,uiW-266,10,Gold);
   CompactHudText(BestDistance.ToString("N0")+" m",196,54,uiW-266,18,Cream);
   if(HudButton(State==RideState.Paused?"▶":"Ⅱ",new Rect(uiW-54,18,34,34),new Color(1,.945f,.812f,.86f),Ink,18))TogglePause();
  }
  void DistanceLandscapeHud(float x,float y)
  {
   Panel(new Rect(x,y,LandscapeBarWidth,76),PortraitGlass,10);
   CompactHudText(CurrentStreet.ToUpperInvariant(),x+14,y+8,330,11,Gold);
   CompactHudText(Mathf.Max(0,Mathf.FloorToInt(Distance)).ToString("N0")+" m",x+14,y+27,220,28,Cream);
   CompactHudText(Mathf.RoundToInt(Speed*3.6f)+" km/h",x+18,y+59,180,11,Hex("B5CFC2"));
   Panel(new Rect(x+350,y+11,1,54),new Color(.8f,.95f,.9f,.18f),0);
   CompactHudText("KỶ LỤC",x+370,y+13,142,11,Gold);
   CompactHudText(BestDistance.ToString("N0")+" m",x+370,y+33,142,20,Cream);
   if(HudButton(State==RideState.Paused?"▶":"Ⅱ",new Rect(x+LandscapeBarWidth-56,y+10,40,36),new Color(1,.945f,.812f,.86f),Ink,19))TogglePause();
   if(HudButton(sound?"ÂM: BẬT":"ÂM: TẮT",new Rect(x+LandscapeBarWidth-103,y+53,87,20),new Color(.07f,.19f,.21f,.35f),Cream,11))sound=!sound;
  }
 }
}
