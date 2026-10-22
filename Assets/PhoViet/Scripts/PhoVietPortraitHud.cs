using UnityEngine;
using static PhoViet.StreetArt;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  const float PortraitWorldScale=.75f*.70f; // A further 30% smaller than the 1.27.1 portrait view.
  const float PortraitBarTop=10,PortraitBarHeight=96,PortraitNoticeTop=112,PortraitNoticeHeight=28;
  static readonly Color PortraitGlass=new Color(.07f,.19f,.21f,.64f);
  bool swipeCanSteer;
  int hudTouchChoice=-1;Vector2 hudTouchStart;float hudTouchSuppressUntil;
  int HudButtonAt(Vector2 position)
  {
   var area=GameGuiArea();bool narrow=Screen.width<Screen.height;
   float scale=narrow?area.width/480f:LandscapeHudScale(area),width=area.width/scale;
   var p=new Vector2((position.x-area.x)/scale,(Screen.height-position.y-area.y)/scale);
   if(narrow)return new Rect(width-54,18,34,34).Contains(p)?0:-1;
   float x=(width-LandscapeBarWidth)/2;
   if(new Rect(x+LandscapeBarWidth-56,28,40,36).Contains(p))return 0;
   return new Rect(x+LandscapeBarWidth-103,71,87,20).Contains(p)?1:-1;
  }
  void ReadHudTouch()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
   // Browser touch taps arrive through ApplyHudTap even when a frame spans the whole tap.
   return;
#else
   if(JobOffersOpen||State==RideState.Ready||State==RideState.GameOver||State==RideState.Jailed||State==RideState.Arrested){hudTouchChoice=-1;return;}
   if(Input.touchCount==0)return;
   var t=Input.GetTouch(0);
   if(t.phase==TouchPhase.Began){hudTouchChoice=HudButtonAt(t.position);hudTouchStart=t.position;}
   if(hudTouchChoice>=0)hudTouchSuppressUntil=Time.unscaledTime+.6f;
   if(t.phase==TouchPhase.Ended)
   {
    int choice=hudTouchChoice;hudTouchChoice=-1;
    if(choice>=0&&HudButtonAt(t.position)==choice&&Vector2.Distance(t.position,hudTouchStart)<Screen.width*.035f)
    {if(choice==0)TogglePause();else sound=!sound;}
   }
   if(t.phase==TouchPhase.Canceled)hudTouchChoice=-1;
#endif
  }
  bool HudButton(string value,Rect rect,Color bg,Color fg,int size)
  {
   bool clicked=Btn(value,rect,bg,fg,size);
#if UNITY_WEBGL && !UNITY_EDITOR
   // The browser pointer bridge owns these buttons, including mouse, touch and pen.
   // Never process its later compatibility mouse event a second time.
   return false;
#else
   // A browser may synthesize a mouse event after touch; consume each HUD tap once.
   return Time.unscaledTime>=hudTouchSuppressUntil&&clicked;
#endif
  }
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
   if(!JobsAvailable){DistancePortraitHud();DrawRideNotice();return;}
   Panel(new Rect(10,PortraitBarTop,uiW-20,PortraitBarHeight),PortraitGlass,10);
   CompactHudText(CurrentStreet.ToUpperInvariant(),22,18,298,10,Gold);
   CompactHudText(Mathf.Max(0,Mathf.FloorToInt(Distance)).ToString("N0")+" m",22,36,140,22,Cream);
   CompactHudText("Coin: "+FormatCoins(Coins),22,66,140,12,Gold);
   CompactHudText(Mathf.RoundToInt(Speed*3.6f)+" km/h",22,86,132,10,Hex("B5CFC2"));
   Panel(new Rect(160,38,1,57),new Color(.8f,.95f,.9f,.18f),0);
   if(JobsAvailable&&ActiveJob!=null)
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
   if(HudButton(State==RideState.Paused?"▶":"Ⅱ",new Rect(uiW-54,18,34,34),new Color(1,.945f,.812f,.86f),Ink,18))TogglePause();
   DrawRideNotice();
  }
  void DrawRideNotice()
  {
   if(State!=RideState.Riding&&State!=RideState.Falling)return;
   if(noticeTimer<=0&&State==RideState.Riding)return;
   string message=State==RideState.Falling?(CrashDuringPursuit?(RiderFallComplete?"ĐÃ TÉ · CHỜ CSGT ĐẾN BẮT":"VA CHẠM · CSGT ĐANG ĐẾN"):"VA CHẠM · CHUYẾN ĐI KẾT THÚC"):notice;
   if(portrait)
   {
    float width=uiW-20;Panel(new Rect(10,PortraitNoticeY,width,PortraitNoticeHeight),new Color(.07f,.19f,.21f,.70f),7);
    Txt(HudEllipsis(message,width-18,11),19,PortraitNoticeY+3,width-18,22,11,Gold,TextAnchor.MiddleCenter);
   }
   else
   {
    float width=Mathf.Min(uiW-40,680);Panel(new Rect((uiW-width)/2,100,width,26),new Color(.07f,.19f,.21f,.70f),7);
    Txt(HudEllipsis(message,width-20,14),(uiW-width)/2+10,102,width-20,22,14,Gold,TextAnchor.MiddleCenter);
   }
  }
  bool GameplayPointerAllowed(Vector2 position)
  {
   var area=GameGuiArea();bool narrow=Screen.width<Screen.height;
   float scale=narrow?area.width/480f:Mathf.Min(area.width/1440f,area.height/900f);
   float top=Screen.height-position.y;
   float reservedTop=narrow?(noticeTimer>0?PortraitNoticeY+PortraitNoticeHeight:PortraitBarTop+(JobsAvailable?PortraitBarHeight:76)):(noticeTimer>0?126:94)*LandscapeHudScale(area)/scale;
   float reservedBottom=narrow?0:110;
   return position.x>=area.xMin&&position.x<=area.xMax&&top>=area.yMin+reservedTop*scale&&top<=area.yMax-reservedBottom*scale;
  }
 }
}
