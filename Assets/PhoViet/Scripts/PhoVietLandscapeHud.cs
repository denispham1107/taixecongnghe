using UnityEngine;
using static PhoViet.StreetArt;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  static float LandscapeHudScale(Rect area)=>Mathf.Min(area.width/1280f,area.height/420f);
  void LandscapeHud()
  {
   var matrix=GUI.matrix;float previousScale=uiScale,previousWidth=uiW,previousHeight=uiH;
   var area=GameGuiArea();
   try
   {
    uiScale=LandscapeHudScale(area);uiW=area.width/uiScale;uiH=area.height/uiScale;
    GUI.matrix=Matrix4x4.TRS(new Vector3(area.x,area.y,0),Quaternion.identity,Vector3.one*uiScale);
    float x=(uiW-1000)/2,y=18;
    Panel(new Rect(x,y,1000,76),PortraitGlass,10);
    CompactHudText(CurrentStreet.ToUpperInvariant(),x+14,y+8,310,12,Gold);
    CompactHudText(Mathf.Max(0,Mathf.FloorToInt(Distance)).ToString("N0")+" m",x+14,y+27,132,28,Cream);
    CompactHudText(Mathf.RoundToInt(Speed*3.6f)+" km/h",x+18,y+59,120,11,Hex("B5CFC2"));
    CompactHudText("Coin: "+FormatCoins(Coins),x+155,y+29,170,16,Gold);
    CompactHudText("MẠNG: "+LivesRemaining+" / 3",x+155,y+53,170,14,LivesRemaining==1?Cream:Gold);
    Panel(new Rect(x+334,y+11,1,54),new Color(.8f,.95f,.9f,.18f),0);
    if(jobsEnabled&&ActiveJob!=null)
    {
     string stage=JobPhase==WorkPhase.Pickup?"ĐÓN KHÁCH":JobPhase==WorkPhase.Delivery?"TRẢ KHÁCH":JobPhase==WorkPhase.Boarding?"KHÁCH LÊN XE":"KHÁCH XUỐNG XE";
     CompactHudText(stage+" · "+Mathf.CeilToInt(NavigationRemainingDistance)+" m",x+350,y+9,280,14,Gold);
     CompactHudText(JobNavigationInstruction(),x+350,y+31,280,16,Cream);
     CompactHudText(city.Street(CurrentWorkTarget)+" · "+(rerouteNotice>0?"TÍNH LẠI ĐƯỜNG":"+"+FormatCoins(ActiveJob.reward)+" COIN"),x+350,y+57,280,11,Hex("B5CFC2"));
    }
    else
    {
     CompactHudText("CHƯA CÓ CHUYẾN",x+350,y+18,280,14,Gold);
     CompactHudText("Chờ công việc tiếp theo",x+350,y+44,280,12,Cream);
    }
    DrawJobMap(new Rect(x+650,y+8,60,60),true);
    Panel(new Rect(x+724,y+11,1,54),new Color(.8f,.95f,.9f,.18f),0);
    CompactHudText("KỶ LỤC",x+742,y+13,142,11,Hex("B5CFC2"));
    CompactHudText(BestDistance.ToString("N0")+" m",x+742,y+33,142,20,Cream);
    if(HudButton(State==RideState.Paused?"▶":"Ⅱ",new Rect(x+944,y+10,40,36),new Color(1,.945f,.812f,.86f),Ink,19))TogglePause();
    if(HudButton(sound?"ÂM: BẬT":"ÂM: TẮT",new Rect(x+897,y+53,87,20),new Color(.07f,.19f,.21f,.35f),Cream,11))sound=!sound;
    DrawRideNotice();
   }
   finally{GUI.matrix=matrix;uiScale=previousScale;uiW=previousWidth;uiH=previousHeight;}
   if(State==RideState.Riding)
   {
    Panel(new Rect(36,uiH-72,550,39),new Color(.07f,.19f,.21f,.46f));
    Txt("← → ĐỔI LÀN · Q ĐẠP · SPACE NHẢY · W TĂNG TỐC · P DỪNG · "+PassedVehicles+" XE ĐÃ NÉ",52,uiH-63,520,24,10,Cream);
   }
  }
 }
}
