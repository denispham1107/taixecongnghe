using UnityEngine;
using static PhoViet.StreetArt;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  // 21.136: format once per displayed change, not on every GUI event.
  int shownDistance=-1,shownBest=-1;string shownDistanceText,shownBestText;
  string DistanceHudText{get{int v=Mathf.Max(0,Mathf.FloorToInt(Distance));if(v!=shownDistance||shownDistanceText==null){shownDistance=v;shownDistanceText=v.ToString("N0")+" m";}return shownDistanceText;}}
  string BestHudText{get{if(BestDistance!=shownBest||shownBestText==null){shownBest=BestDistance;shownBestText=BestDistance.ToString("N0")+" m";}return shownBestText;}}
  float LandscapeBarWidth=>JobsAvailable?1000:640;
  float PortraitNoticeY=>PortraitNoticeTop;
  void DistancePortraitHud()
  {
   Panel(new Rect(10,PortraitBarTop,uiW-20,PortraitBarHeight),PortraitGlass,10);
   CompactHudText(CurrentStreet.ToUpperInvariant(),22,18,uiW-92,10,Gold);
   CompactHudText(DistanceHudText,22,35,154,24,Cream);
   CompactHudText(Mathf.RoundToInt(Speed*3.6f)+" km/h",22,65,140,10,Hex("B5CFC2"));
   Panel(new Rect(180,27,1,69),new Color(.8f,.95f,.9f,.18f),0);
   CompactHudText("KỶ LỤC",196,22,uiW-266,10,Gold);
   CompactHudText(BestHudText,196,38,uiW-266,18,Cream);
   DrawEnergyBar(new Rect(196,66,uiW-218,27));
   DrawPlayerMissileAmmo(new Rect(22,83,140,20));
   if(HudButton(State==RideState.Paused?"▶":"Ⅱ",new Rect(uiW-54,18,34,34),new Color(1,.945f,.812f,.86f),Ink,18))TogglePause();
  }
  void DistanceLandscapeHud(float x,float y)
  {
   Panel(new Rect(x,y,LandscapeBarWidth,76),PortraitGlass,10);
   CompactHudText(CurrentStreet.ToUpperInvariant(),x+14,y+8,330,11,Gold);
   CompactHudText(DistanceHudText,x+14,y+27,160,28,Cream);
   CompactHudText(Mathf.RoundToInt(Speed*3.6f)+" km/h",x+18,y+59,180,11,Hex("B5CFC2"));
   DrawEnergyBar(new Rect(x+185,y+26,143,27));
   DrawPlayerMissileAmmo(new Rect(x+185,y+54,143,20));
   Panel(new Rect(x+350,y+11,1,54),new Color(.8f,.95f,.9f,.18f),0);
   CompactHudText("KỶ LỤC",x+370,y+13,142,11,Gold);
   CompactHudText(BestHudText,x+370,y+33,142,20,Cream);
   if(HudButton(State==RideState.Paused?"▶":"Ⅱ",new Rect(x+LandscapeBarWidth-56,y+10,40,36),new Color(1,.945f,.812f,.86f),Ink,19))TogglePause();
   if(HudButton(sound?"ÂM: BẬT":"ÂM: TẮT",new Rect(x+LandscapeBarWidth-103,y+53,87,20),new Color(.07f,.19f,.21f,.35f),Cream,11))sound=!sound;
  }
 }
}
