using UnityEngine;
using static PhoViet.StreetArt;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  const float PortraitWorldScale=.75f;
  const float PortraitBarTop=10,PortraitBarHeight=96,PortraitNoticeTop=112,PortraitNoticeHeight=28;
  static readonly Color PortraitGlass=new Color(.07f,.19f,.21f,.64f);
  bool swipeCanSteer;
  string HudEllipsis(string text,float width,int size)
  {
   label.fontSize=size;
   if(label.CalcSize(new GUIContent(text)).x<=width)return text;
   while(text.Length>0&&label.CalcSize(new GUIContent(text+"…")).x>width)text=text.Substring(0,text.Length-1);
   return text+"…";
  }
  void CompactHudText(string value,float x,float y,float width,int size,Color color)
  {Txt(HudEllipsis(value,width,size),x,y,width,size+5,size,color);}
  void PortraitHud()
  {
   Panel(new Rect(10,PortraitBarTop,uiW-20,PortraitBarHeight),PortraitGlass,10);
   CompactHudText(CurrentStreet.ToUpperInvariant(),22,18,298,10,Gold);
   CompactHudText(Mathf.Max(0,Mathf.FloorToInt(Distance)).ToString("N0")+" m",22,36,140,22,Cream);
   CompactHudText("Coin: "+FormatCoins(Coins),22,66,140,12,Gold);
   CompactHudText("MẠNG: "+LivesRemaining+" / 3",22,84,78,11,LivesRemaining==1?Cream:Gold);
   CompactHudText(Mathf.RoundToInt(Speed*3.6f)+" km/h",104,86,54,9,Hex("B5CFC2"));
   Panel(new Rect(160,38,1,57),new Color(.8f,.95f,.9f,.18f),0);
   if(jobsEnabled&&ActiveJob!=null)
   {
    string stage=JobPhase==WorkPhase.Pickup?"ĐÓN":JobPhase==WorkPhase.Delivery?"TRẢ":JobPhase==WorkPhase.Boarding?"LÊN XE":"XUỐNG XE";
    CompactHudText(stage+" · "+Mathf.CeilToInt(NavigationRemainingDistance)+" m",172,36,152,12,Gold);
    CompactHudText(JobNavigationInstruction(),172,55,152,12,Cream);
    CompactHudText(city.Street(CurrentWorkTarget),172,74,152,10,Cream);
    CompactHudText(rerouteNotice>0?"TÍNH LẠI ĐƯỜNG":"THƯỞNG "+FormatCoins(ActiveJob.reward)+" COIN",172,90,152,9,Hex("B5CFC2"));
   }
   else
   {
    CompactHudText("CHƯA CÓ CHUYẾN",172,42,152,11,Gold);
    CompactHudText("Chờ công việc tiếp theo",172,63,152,10,Cream);
   }
   DrawJobMap(new Rect(uiW-148,18,80,80),true);
   if(Btn(State==RideState.Paused?"▶":"Ⅱ",new Rect(uiW-54,18,34,34),new Color(1,.945f,.812f,.86f),Ink,18))TogglePause();
   DrawRideNotice();
  }
  void DrawRideNotice()
  {
   if(State!=RideState.Riding&&State!=RideState.Falling&&State!=RideState.Recovering)return;
   if(noticeTimer<=0&&State==RideState.Riding)return;
   string message=State==RideState.Falling?(LivesRemaining>0?"VA CHẠM · CÒN "+LivesRemaining+" MẠNG":"ĐÃ HẾT 3 MẠNG"):State==RideState.Recovering?"ĐANG DỰNG XE · CÒN "+LivesRemaining+" MẠNG":notice;
   if(portrait)
   {
    float width=uiW-20;Panel(new Rect(10,PortraitNoticeTop,width,PortraitNoticeHeight),new Color(.07f,.19f,.21f,.70f),7);
    Txt(HudEllipsis(message,width-18,11),19,PortraitNoticeTop+3,width-18,22,11,Gold,TextAnchor.MiddleCenter);
   }
   else
   {
    float width=Mathf.Min(uiW-32,540);Panel(new Rect((uiW-width)/2,100,width,48),Ink);
    Txt(message,(uiW-width)/2+12,106,width-24,36,16,Gold,TextAnchor.MiddleCenter);
   }
  }
  bool GameplayPointerAllowed(Vector2 position)
  {
   var area=GameGuiArea();bool narrow=Screen.width<Screen.height;
   float scale=narrow?area.width/480f:Mathf.Min(area.width/1440f,area.height/900f);
   float top=Screen.height-position.y;
   float reservedTop=narrow?(noticeTimer>0?PortraitNoticeTop+PortraitNoticeHeight:PortraitBarTop+PortraitBarHeight):110;
   float reservedBottom=narrow?0:110;
   return position.x>=area.xMin&&position.x<=area.xMax&&top>=area.yMin+reservedTop*scale&&top<=area.yMax-reservedBottom*scale;
  }
 }
}
